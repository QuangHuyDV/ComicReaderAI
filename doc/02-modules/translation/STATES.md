# Translation States

> **Project:** CRAI
> **Module:** Translation
> **Document:** State Machines
> **Path:** `modules/translation/STATES.md`
> **Version:** 0.2
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-03
> **Source of Truth:**
>
> * `modules/translation/MODULE.md`
> * `modules/translation/CONTRACT.md`
> * `modules/translation/EVENTS.md`

---

## 1. Purpose

This document defines the lifecycle states and valid state transitions owned by the Translation module.

It covers:

* translation job lifecycle;
* translation attempt lifecycle;
* translation batch lifecycle;
* translation result lifecycle;
* translation variant lifecycle;
* cancellation behavior;
* supersession behavior;
* invalidation behavior;
* retry behavior;
* partial completion;
* progressive publication;
* stale-result rejection;
* relationships between independent state machines.

This document does not define:

* command payloads;
* event payload schemas;
* detailed error catalogs;
* provider-specific states;
* persistence tables;
* worker implementation;
* UI display states.

---

## 2. State Ownership

Translation owns state for:

```text
TranslationJob
TranslationAttempt
TranslationBatch
TranslationResult
TranslationVariant
```

Translation does not own state for:

```text
PreparedDocument
PreparedSegment
ReadingSession
KnowledgeSnapshot
Provider credential
Presentation overlay
OCR job
```

External entity state may influence Translation transitions but remains owned by its original module.

### 2.1 Runtime execution ownership

Runtime owns queue admission, scheduling, worker execution, retry timing, concurrency enforcement, backpressure, and physical cancellation coordination.

Translation owns the domain lifecycle reflected by these state machines. Therefore, states such as `QUEUED`, `RUNNING`, and `RETRY_SCHEDULED` represent Translation's observed domain condition; they do not imply that Translation implements its own scheduler or worker runtime.

```text
Translation decides what work is valid and required.
Runtime decides when and how execution proceeds.
```

### 2.2 State Ownership Matrix

| State machine | Owner |
|---|---|
| `TranslationJobState` | Translation |
| `TranslationBatchState` | Translation |
| `TranslationAttemptState` | Translation |
| `TranslationResultState` | Translation |
| `TranslationVariantState` | Translation |
| Queue admission and scheduler state | Runtime |
| Worker and physical execution state | Runtime |
| Retry timer and backoff state | Runtime |
| Cancellation coordination state | Runtime |
| `ReadingSessionState` | Reading Session |
| Presentation lifecycle state | Presentation |

---

## 3. State Machine Separation

Translation does not use one global state enumeration for every entity.

Each lifecycle has its own state set:

```text
TranslationJobState
TranslationAttemptState
TranslationBatchState
TranslationResultState
TranslationVariantState
```

This separation is required because:

* a job may be running while one batch is already completed;
* an attempt may fail while the job remains retryable;
* a result may be partial while the job remains active;
* a variant may remain stored after its job is superseded;
* a completed result may later be invalidated administratively.

---

## 4. State Principles

### 4.1 State represents current domain truth

State describes the current lifecycle condition of an entity.

An event describes that a transition occurred.

```text
State
    = current condition

Event
    = historical fact
```

---

### 4.2 Transitions must be explicit

Entities must not jump between unrelated states without a defined transition.

Incorrect:

```text
CREATED → COMPLETED
```

unless an explicit cache-reuse path defines that transition.

Correct:

```text
CREATED
    ↓
QUEUED
    ↓
RUNNING
    ↓
COMPLETED
```

---

### 4.3 Terminal does not always mean deleted

Terminal states prevent normal execution transitions.

They do not imply physical deletion.

Historical data may be retained for:

* auditing;
* cache reuse;
* user variant selection;
* diagnostics;
* usage tracking.

---

### 4.4 Cancellation and supersession are authoritative controls

A cancelled or superseded job must not later become authoritative, even if provider execution finishes.

---

### 4.5 Errors do not always terminate the job

An attempt or batch may fail while the parent job remains active and schedules another attempt.

---

### 4.6 Results and execution are separate

Provider execution completion does not automatically mean the result is authoritative.

Result assembly, validation, source-revision verification and publication checks must occur first.

---

## 4.7 Canonical aggregate hierarchy

The canonical execution hierarchy is:

```text
TranslationJob
    ├── TranslationBatch A
    │       ├── TranslationAttempt 1
    │       └── TranslationAttempt 2
    └── TranslationBatch B
            └── TranslationAttempt 1
```

`TranslationBatch` is the stable provider execution unit derived from a selected segment set. `TranslationAttempt` is one immutable physical execution attempt for that batch. When retry changes batch membership, a replacement batch with a new `TranslationBatchId` is created.

# Part I — Translation Job State Machine

## 5. TranslationJobState

The canonical Translation job states are:

```text
CREATED
QUEUED
RUNNING
PARTIALLY_COMPLETED
RETRY_SCHEDULED
CANCELLATION_REQUESTED

COMPLETED
COMPLETED_WITH_WARNINGS
FAILED
CANCELLED
SUPERSEDED
INVALIDATED
```

---

## 6. Job State Categories

### 6.1 Initial state

```text
CREATED
```

### 6.2 Active states

```text
QUEUED
RUNNING
PARTIALLY_COMPLETED
RETRY_SCHEDULED
CANCELLATION_REQUESTED
```

### 6.3 Successful terminal states

```text
COMPLETED
COMPLETED_WITH_WARNINGS
```

### 6.4 Unsuccessful terminal states

```text
FAILED
CANCELLED
SUPERSEDED
```

### 6.5 Administrative terminal state

```text
INVALIDATED
```

`INVALIDATED` may follow a previously successful state.

It represents loss of validity, not execution failure.

---

## 7. CREATED

The job has been accepted and assigned a `TranslationJobId`.

At this point:

* source identity is fixed;
* configuration snapshot is fixed;
* selected source segments are fixed;
* execution may not yet be scheduled;
* no active attempt is required yet.

Entry causes may include:

```text
StartTranslation accepted
RequestRetranslation accepted
Manual variant request accepted
```

Expected event:

```text
TranslationJobCreated
```

Valid outgoing transitions:

```text
CREATED → QUEUED
CREATED → RUNNING
CREATED → COMPLETED
CREATED → COMPLETED_WITH_WARNINGS
CREATED → CANCELLATION_REQUESTED
CREATED → CANCELLED
CREATED → SUPERSEDED
CREATED → FAILED
```

Direct completion is allowed only for a valid cache-reuse path.

---

## 8. QUEUED

The job is waiting for execution resources.

At this point:

* the job is schedulable;
* no provider execution is required to be active;
* priority may affect scheduling order;
* cancellation and supersession remain possible.

Expected event:

```text
TranslationJobQueued
```

Valid outgoing transitions:

```text
QUEUED → RUNNING
QUEUED → CANCELLATION_REQUESTED
QUEUED → CANCELLED
QUEUED → SUPERSEDED
QUEUED → FAILED
```

A queue failure may transition the job to `FAILED` when no recovery path exists.

---

## 9. RUNNING

At least one Translation attempt is active or execution work is being assembled.

At this point:

* batches may be pending, running, completed or failed;
* partial results may exist;
* retries may still be possible;
* no final authoritative completion has been established.

Expected events may include:

```text
TranslationJobStarted
TranslationAttemptStarted
TranslationBatchStarted
TranslationProgressUpdated
```

Valid outgoing transitions:

```text
RUNNING → PARTIALLY_COMPLETED
RUNNING → RETRY_SCHEDULED
RUNNING → CANCELLATION_REQUESTED
RUNNING → COMPLETED
RUNNING → COMPLETED_WITH_WARNINGS
RUNNING → FAILED
RUNNING → CANCELLED
RUNNING → SUPERSEDED
```

---

## 10. PARTIALLY_COMPLETED

At least one selected prepared segment has an accepted translated result, while other selected segments remain incomplete or failed.

This state is used when partial progress is meaningful at the job level.

At this point:

* a partial `TranslationResult` exists;
* some segments may be authoritative under progressive publication;
* the complete job remains non-terminal;
* pending or retryable work remains.

Expected event:

```text
TranslationPartialResultAvailable
```

Possible supporting events:

```text
TranslationSegmentsCompleted
TranslationProgressUpdated
TranslationBatchFailed
```

Valid outgoing transitions:

```text
PARTIALLY_COMPLETED → RUNNING
PARTIALLY_COMPLETED → RETRY_SCHEDULED
PARTIALLY_COMPLETED → CANCELLATION_REQUESTED
PARTIALLY_COMPLETED → COMPLETED
PARTIALLY_COMPLETED → COMPLETED_WITH_WARNINGS
PARTIALLY_COMPLETED → FAILED
PARTIALLY_COMPLETED → CANCELLED
PARTIALLY_COMPLETED → SUPERSEDED
```

The state must not be used merely because an internal provider token stream has started.

At least one structurally validated translated segment must exist.

---

## 11. RETRY_SCHEDULED

A previous batch attempt failed, and Translation has requested another eligible execution. Runtime owns the actual retry timer, backoff, admission, and execution start.

At this point:

* the logical job remains active;
* a new `TranslationAttemptId` may not yet have started;
* retry policy has approved another execution;
* `TranslationJobId` remains unchanged.

Expected event:

```text
TranslationRetryScheduled
```

Possible supporting event:

```text
TranslationProviderFallbackSelected
```

Valid outgoing transitions:

```text
RETRY_SCHEDULED → RUNNING
RETRY_SCHEDULED → CANCELLATION_REQUESTED
RETRY_SCHEDULED → CANCELLED
RETRY_SCHEDULED → SUPERSEDED
RETRY_SCHEDULED → FAILED
```

`RETRY_SCHEDULED` must not create a new logical job.

---

## 12. CANCELLATION_REQUESTED

Logical cancellation has been accepted, but cleanup or active execution shutdown may still be in progress.

At this point:

* no new attempts may start;
* no new batches may be scheduled;
* active provider requests should be physically cancelled where supported;
* newly arriving results cannot become authoritative;
* partial retained data depends on policy.

Expected event:

```text
TranslationCancellationRequested
```

Valid outgoing transitions:

```text
CANCELLATION_REQUESTED → CANCELLED
CANCELLATION_REQUESTED → SUPERSEDED
```

Normally, this state must not transition to:

```text
COMPLETED
COMPLETED_WITH_WARNINGS
RUNNING
RETRY_SCHEDULED
```

A provider response received during cancellation may be retained diagnostically but cannot reverse logical cancellation.

---

## 13. COMPLETED

All required selected segments were translated successfully.

At this point:

* the final result is available;
* alignment validation passed;
* source revision checks passed;
* cancellation and supersession checks passed;
* no completion-level warnings require degraded status;
* an immutable variant exists.

Expected event:

```text
TranslationCompleted
```

Possible following event:

```text
TranslationVariantActivated
```

Normal outgoing transitions:

```text
COMPLETED → INVALIDATED
COMPLETED → SUPERSEDED
```

`SUPERSEDED` after completion is allowed when newer translation work becomes authoritative.

The completed historical result remains retained unless storage policy removes it.

---

## 14. COMPLETED_WITH_WARNINGS

All required output is usable, but one or more significant warnings exist.

Examples:

* provider fallback was used;
* some terminology remained ambiguous;
* source language confidence was low;
* untranslated sound effects were preserved;
* output-length anomalies exist;
* contextual ambiguity remains.

Expected event:

```text
TranslationCompletedWithWarnings
```

Normal outgoing transitions:

```text
COMPLETED_WITH_WARNINGS → INVALIDATED
COMPLETED_WITH_WARNINGS → SUPERSEDED
```

This state is successful.

Warnings must not be confused with fatal errors.

---

## 15. FAILED

The logical job cannot continue and no further automatic attempts are scheduled.

Possible causes:

* invalid provider output after all attempts;
* retry budget exhausted;
* no eligible provider;
* non-retryable input or configuration failure;
* unresolved alignment failure;
* required context unavailable;
* final result assembly failure.

Expected event:

```text
TranslationFailed
```

Normal outgoing transitions:

```text
FAILED → INVALIDATED
```

A normal retry command must not mutate the failed job back into `RUNNING` unless project policy explicitly allows reopening.

Recommended approach:

* same translation intent and explicit retry: create a new attempt only if the job is defined as reopenable;
* otherwise create a derived new job.

For the initial architecture, failed jobs should be terminal and manual recovery should create a new logical job through retranslation.

---

## 16. CANCELLED

Cancellation has reached its terminal logical state.

At this point:

* no new attempts may start;
* no result may become authoritative;
* active provider responses are ignored for authority;
* retained partial results remain non-authoritative unless policy explicitly preserved already published segments.

Expected event:

```text
TranslationCancelled
```

Normal outgoing transitions:

```text
CANCELLED → INVALIDATED
```

A cancelled job must not return to active execution.

A new translation request creates a new job.

---

## 17. SUPERSEDED

The job has been replaced by newer or more relevant work.

Possible reasons:

* source revision changed;
* a newer translation job was created;
* target language changed;
* translation profile changed;
* reading context changed;
* manual retranslation replaced it.

Expected event:

```text
TranslationSuperseded
```

At this point:

* the job cannot become authoritative;
* active execution should stop when practical;
* completed historical output may remain available;
* partial output cannot overwrite newer work.

Normal outgoing transitions:

```text
SUPERSEDED → INVALIDATED
```

A superseded job must not return to `RUNNING`.

---

## 18. INVALIDATED

The job or its result has been marked invalid for future authoritative use.

Invalidation may occur because:

* source alignment was incorrect;
* source content was later corrected;
* result quality was administratively rejected;
* privacy or security policy changed;
* glossary constraints were materially violated;
* stored data became incompatible.

Expected event:

```text
TranslationInvalidated
```

`INVALIDATED` is terminal.

No outgoing lifecycle transition is allowed.

A replacement requires a new translation job.

---

## 19. Translation Job State Diagram

```text
                    ┌──────────────┐
                    │   CREATED    │
                    └──────┬───────┘
                           │
                 ┌─────────┴──────────┐
                 ▼                    ▼
             ┌────────┐           ┌─────────┐
             │ QUEUED │           │ RUNNING │
             └───┬────┘           └────┬────┘
                 │                     │
                 └──────────┬──────────┘
                            ▼
                  ┌─────────────────────┐
                  │ PARTIALLY_COMPLETED │
                  └──────────┬──────────┘
                             │
             ┌───────────────┼────────────────┐
             ▼               ▼                ▼
    ┌─────────────────┐ ┌───────────┐ ┌─────────────────────────┐
    │ RETRY_SCHEDULED │ │ COMPLETED │ │ COMPLETED_WITH_WARNINGS │
    └────────┬────────┘ └───────────┘ └─────────────────────────┘
             │
             └──────────────► RUNNING
```

Alternative terminal paths from active states:

```text
CREATED
QUEUED
RUNNING
PARTIALLY_COMPLETED
RETRY_SCHEDULED
        │
        ├──► CANCELLATION_REQUESTED ──► CANCELLED
        ├──► FAILED
        └──► SUPERSEDED
```

Post-completion administrative path:

```text
COMPLETED
COMPLETED_WITH_WARNINGS
FAILED
CANCELLED
SUPERSEDED
        │
        └──► INVALIDATED
```

---

# Part II — Translation Attempt State Machine

## 20. TranslationAttemptState

Canonical attempt states:

```text
CREATED
PREPARING
RUNNING
PARTIALLY_COMPLETED
COMPLETED
FAILED
CANCELLED
SUPERSEDED
```

---

## 21. Attempt CREATED

The attempt identity exists but execution preparation has not completed.

At this point:

* attempt number is assigned;
* attempt reason is known;
* provider selection may still be unresolved;
* batches may not yet exist.

Valid outgoing transitions:

```text
CREATED → PREPARING
CREATED → CANCELLED
CREATED → SUPERSEDED
CREATED → FAILED
```

---

## 22. Attempt PREPARING

The module is preparing execution.

Activities may include:

* provider selection;
* provider capability validation;
* batch construction;
* context resolution;
* terminology snapshot resolution;
* provider limit calculation.

Valid outgoing transitions:

```text
PREPARING → RUNNING
PREPARING → FAILED
PREPARING → CANCELLED
PREPARING → SUPERSEDED
```

A failure here may still allow the parent job to schedule another attempt.

---

## 23. Attempt RUNNING

At least one batch is running or eligible to run.

Expected event:

```text
TranslationAttemptStarted
```

Valid outgoing transitions:

```text
RUNNING → PARTIALLY_COMPLETED
RUNNING → COMPLETED
RUNNING → FAILED
RUNNING → CANCELLED
RUNNING → SUPERSEDED
```

---

## 24. Attempt PARTIALLY_COMPLETED

At least one batch completed successfully while other attempt work remains pending or failed.

Valid outgoing transitions:

```text
PARTIALLY_COMPLETED → RUNNING
PARTIALLY_COMPLETED → COMPLETED
PARTIALLY_COMPLETED → FAILED
PARTIALLY_COMPLETED → CANCELLED
PARTIALLY_COMPLETED → SUPERSEDED
```

This state does not imply the parent job is necessarily `PARTIALLY_COMPLETED`.

The parent job may already contain completed segments from earlier attempts.

---

## 25. Attempt COMPLETED

The attempt completed all work assigned to it successfully.

Expected event:

```text
TranslationAttemptCompleted
```

This does not automatically imply:

```text
TranslationJob = COMPLETED
```

Result assembly or additional attempts may still be required.

`COMPLETED` is terminal for the attempt.

---

## 26. Attempt FAILED

The attempt cannot continue.

Expected event:

```text
TranslationAttemptFailed
```

The parent job may transition to:

```text
RETRY_SCHEDULED
FAILED
PARTIALLY_COMPLETED
```

depending on:

* retryability;
* retained completed batches;
* retry budget;
* fallback availability;
* publication policy.

`FAILED` is terminal for the attempt.

---

## 27. Attempt CANCELLED

The attempt stopped because the parent job or attempt scope was cancelled.

No output from the attempt may become newly authoritative after this state.

`CANCELLED` is terminal.

---

## 28. Attempt SUPERSEDED

The attempt became obsolete because:

* a newer attempt replaced it;
* the job was superseded;
* retry execution moved to another provider;
* source or intent changed.

`SUPERSEDED` is terminal.

Late provider responses are ignored for authority.

---

## 29. Attempt State Diagram

```text
CREATED
    ↓
PREPARING
    ↓
RUNNING
    ├──► PARTIALLY_COMPLETED ──► COMPLETED
    ├──► COMPLETED
    ├──► FAILED
    ├──► CANCELLED
    └──► SUPERSEDED
```

---

# Part III — Translation Batch State Machine

## 30. TranslationBatchState

Canonical batch states:

```text
CREATED
READY
RUNNING
VALIDATING
COMPLETED
FAILED
CANCELLED
SUPERSEDED
```

---

## 31. Batch CREATED

The batch identity and segment membership exist.

At this point:

* prepared segment IDs are fixed;
* context-only segment membership is fixed;
* provider request may not yet be constructible.

Valid outgoing transitions:

```text
CREATED → READY
CREATED → FAILED
CREATED → CANCELLED
CREATED → SUPERSEDED
```

---

## 32. Batch READY

The batch is fully prepared and eligible for provider execution.

At this point:

* provider is selected;
* limits are satisfied;
* required context is available;
* terminology constraints are resolved;
* request construction can proceed.

Valid outgoing transitions:

```text
READY → RUNNING
READY → CANCELLED
READY → SUPERSEDED
READY → FAILED
```

---

## 33. Batch RUNNING

Provider execution is active.

Expected event:

```text
TranslationBatchStarted
```

Valid outgoing transitions:

```text
RUNNING → VALIDATING
RUNNING → FAILED
RUNNING → CANCELLED
RUNNING → SUPERSEDED
```

An HTTP success response does not transition directly to `COMPLETED`.

The output must first enter validation.

---

## 34. Batch VALIDATING

Provider output has been received and is being checked.

Validation may include:

* response structure;
* segment identity;
* duplicate segment detection;
* missing output detection;
* target-language plausibility;
* locked terminology;
* output-length checks;
* provider-control leakage;
* source alignment.

Valid outgoing transitions:

```text
VALIDATING → COMPLETED
VALIDATING → FAILED
VALIDATING → CANCELLED
VALIDATING → SUPERSEDED
```

Cancellation or supersession during validation prevents authoritative acceptance.

---

## 35. Batch COMPLETED

The batch output passed required validation and has been accepted.

Expected event:

```text
TranslationBatchCompleted
```

At this point:

* translated segments are traceable;
* output may contribute to a partial or final result;
* the batch cannot be rerun in place.

A retry creates a new batch instance under a new attempt or retry scope.

`COMPLETED` is terminal.

---

## 36. Batch FAILED

The batch could not produce accepted output in its current attempt.

Expected event:

```text
TranslationBatchFailed
```

Possible causes:

* provider timeout;
* provider rejection;
* malformed result;
* missing segment identity;
* output validation failure;
* provider unavailable;
* unsupported request;
* context construction failure.

The parent attempt or job decides whether to retry.

`FAILED` is terminal for this batch instance.

---

## 37. Batch CANCELLED

The batch was logically cancelled.

A provider may still return physically, but the response cannot be accepted.

`CANCELLED` is terminal.

---

## 38. Batch SUPERSEDED

The batch became obsolete because:

* another attempt replaced it;
* the job was superseded;
* source revision changed;
* execution policy selected a replacement batch.

`SUPERSEDED` is terminal.

---

## 39. Batch State Diagram

```text
CREATED
    ↓
READY
    ↓
RUNNING
    ↓
VALIDATING
    ├──► COMPLETED
    ├──► FAILED
    ├──► CANCELLED
    └──► SUPERSEDED
```

Alternative transitions from pre-execution states:

```text
CREATED / READY
    ├──► FAILED
    ├──► CANCELLED
    └──► SUPERSEDED
```

---

# Part IV — Translation Result State Machine

## 40. TranslationResultState

Canonical result states:

```text
ASSEMBLING
PARTIAL
FINALIZING
AVAILABLE
AVAILABLE_WITH_WARNINGS
NON_AUTHORITATIVE
INVALIDATED
```

A Translation result is not treated as a provider execution state.

It represents assembled output.

---

## 41. Result ASSEMBLING

Accepted batch and segment outputs are being assembled.

At this point:

* some translated segments may exist;
* result ordering may still be incomplete;
* missing and failed segments are still being calculated;
* result revision may not yet be public.

Valid outgoing transitions:

```text
ASSEMBLING → PARTIAL
ASSEMBLING → FINALIZING
ASSEMBLING → NON_AUTHORITATIVE
ASSEMBLING → INVALIDATED
```

---

## 42. Result PARTIAL

The result contains validated output for only part of the selected source set.

Expected event:

```text
TranslationPartialResultAvailable
```

At this point:

* completed segments are explicit;
* missing segments are explicit;
* failed segments are explicit;
* `translationRevision` is assigned;
* publication depends on policy.

Valid outgoing transitions:

```text
PARTIAL → PARTIAL
PARTIAL → FINALIZING
PARTIAL → NON_AUTHORITATIVE
PARTIAL → INVALIDATED
```

`PARTIAL → PARTIAL` represents a new immutable result revision, not mutation of the previously published revision.

Example:

```text
Result revision 1
    3 of 10 segments

Result revision 2
    7 of 10 segments
```

---

## 43. Result FINALIZING

All required execution work has ended and the result is undergoing final checks.

Checks include:

* selected segment coverage;
* result ordering;
* duplicate alignment;
* source identity;
* content revision;
* job authority;
* cancellation status;
* supersession status;
* variant creation;
* publication policy.

Valid outgoing transitions:

```text
FINALIZING → AVAILABLE
FINALIZING → AVAILABLE_WITH_WARNINGS
FINALIZING → NON_AUTHORITATIVE
FINALIZING → INVALIDATED
```

---

## 44. Result AVAILABLE

The result is valid and eligible for authoritative use.

Expected associated event:

```text
TranslationCompleted
```

At this point:

* final alignment passed;
* result is retrievable;
* it is eligible for Reading Session authority acceptance;
* Presentation may use it only after the relevant Reading Session or equivalent authority accepts the matching translation snapshot;
* an active variant may reference it.

Valid outgoing transitions:

```text
AVAILABLE → NON_AUTHORITATIVE
AVAILABLE → INVALIDATED
```

It may become non-authoritative when a newer result or variant is activated.

---

## 45. Result AVAILABLE_WITH_WARNINGS

The result is usable and eligible for authority, but significant warnings remain.

Expected associated event:

```text
TranslationCompletedWithWarnings
```

Valid outgoing transitions:

```text
AVAILABLE_WITH_WARNINGS → NON_AUTHORITATIVE
AVAILABLE_WITH_WARNINGS → INVALIDATED
```

Warnings do not make the result structurally invalid.

---

## 46. Result NON_AUTHORITATIVE

The result exists but cannot be used as the current authoritative translation.

Possible reasons:

* job cancelled;
* job superseded;
* newer result revision exists;
* newer variant activated;
* stale source revision;
* result preserved after final job failure;
* partial output retained for diagnostics.

This state does not necessarily mean the content is linguistically incorrect.

Valid outgoing transitions:

```text
NON_AUTHORITATIVE → INVALIDATED
```

Reactivation should generally occur through variant activation rules, not by changing this result state back to `AVAILABLE`.

---

## 47. Result INVALIDATED

The result is no longer considered valid.

Possible causes:

* structural alignment defect;
* source revision corruption;
* administrative quality rejection;
* security or privacy issue;
* contract incompatibility;
* invalid glossary application.

`INVALIDATED` is terminal.

---

## 48. Result State Diagram

```text
ASSEMBLING
    ├──► PARTIAL ──► PARTIAL
    │                  │
    │                  ▼
    └────────────► FINALIZING
                       ├──► AVAILABLE
                       ├──► AVAILABLE_WITH_WARNINGS
                       ├──► NON_AUTHORITATIVE
                       └──► INVALIDATED
```

Post-publication:

```text
AVAILABLE
AVAILABLE_WITH_WARNINGS
        │
        ├──► NON_AUTHORITATIVE
        └──► INVALIDATED

NON_AUTHORITATIVE
        └──► INVALIDATED
```

---

# Part V — Translation Variant State Machine

## 49. TranslationVariantState

Canonical variant states:

```text
CREATED
AVAILABLE
ACTIVE
INACTIVE
INVALIDATED
```

---

## 50. Variant CREATED

The immutable variant has been constructed but is not yet available for selection.

At this point:

* translated segments are assigned;
* parent variant lineage may exist;
* validation or result linkage may still be pending.

Expected event:

```text
TranslationVariantCreated
```

Valid outgoing transitions:

```text
CREATED → AVAILABLE
CREATED → ACTIVE
CREATED → INVALIDATED
```

Direct activation is allowed when creation and activation are committed atomically.

---

## 51. Variant AVAILABLE

The variant is valid and may be selected.

It is not currently active for the relevant reading context.

Valid outgoing transitions:

```text
AVAILABLE → ACTIVE
AVAILABLE → INVALIDATED
```

---

## 52. Variant ACTIVE

The variant is currently selected for a compatible reading context.

Expected event:

```text
TranslationVariantActivated
```

At this point:

* Presentation may render it;
* it must match source revision and target language;
* only one compatible variant should normally be active in one reading context.

Valid outgoing transitions:

```text
ACTIVE → INACTIVE
ACTIVE → INVALIDATED
```

---

## 53. Variant INACTIVE

The variant remains valid but another variant is active.

Possible causes:

* user selected another variant;
* new retranslation completed;
* corrected variant replaced it;
* literal or natural alternative became active.

Valid outgoing transitions:

```text
INACTIVE → ACTIVE
INACTIVE → INVALIDATED
```

Unlike result authority, variant activation may be reversible when the source revision remains compatible.

---

## 54. Variant INVALIDATED

The variant is no longer eligible for activation.

Expected event:

```text
TranslationVariantInvalidated
```

Possible causes:

* result invalidated;
* source alignment changed;
* correction was rejected;
* security or policy issue;
* source revision became incompatible.

`INVALIDATED` is terminal.

---

## 55. Variant State Diagram

```text
CREATED
    ↓
AVAILABLE
    ↓
ACTIVE
    ↓
INACTIVE
    └────────► ACTIVE
```

Invalidation path:

```text
CREATED
AVAILABLE
ACTIVE
INACTIVE
    │
    └──► INVALIDATED
```

---

# Part VI — Entity State Relationships

## 56. Job and Attempt Relationship

A running batch attempt normally requires an active parent job and an active parent batch.

Allowed parent states:

```text
TranslationAttempt.RUNNING
    requires TranslationJob in:

RUNNING
PARTIALLY_COMPLETED
```

An attempt must not enter `RUNNING` when the parent job is:

```text
COMPLETED
COMPLETED_WITH_WARNINGS
FAILED
CANCELLED
SUPERSEDED
INVALIDATED
```

---

## 57. Batch and Attempt Relationship

A running attempt belongs to exactly one batch and requires that batch to remain eligible for execution.

```text
TranslationAttempt.RUNNING
    requires TranslationBatch in:

READY
RUNNING
VALIDATING
```

A batch may have multiple immutable attempts over time. A failed, cancelled, or superseded attempt never returns to `RUNNING`; retry creates a new `TranslationAttemptId` under the same logical batch or under a replacement batch when membership changes.

---

## 58. Batch and Result Relationship

Only accepted batch output may contribute to public results.

```text
TranslationBatch.COMPLETED
        ↓
TranslatedSegment accepted
        ↓
TranslationResult ASSEMBLING or PARTIAL
```

Output from batches in these states must not become authoritative:

```text
FAILED
CANCELLED
SUPERSEDED
```

---

## 59. Job and Result Relationship

Typical mappings:

```text
Job RUNNING
    → Result ASSEMBLING

Job PARTIALLY_COMPLETED
    → Result PARTIAL

Job COMPLETED
    → Result AVAILABLE

Job COMPLETED_WITH_WARNINGS
    → Result AVAILABLE_WITH_WARNINGS

Job CANCELLED
    → Result NON_AUTHORITATIVE

Job SUPERSEDED
    → Result NON_AUTHORITATIVE

Job INVALIDATED
    → Result INVALIDATED
```

A failed job may still have:

```text
Result PARTIAL
```

or:

```text
Result NON_AUTHORITATIVE
```

depending on retention and publication policy.

---

## 60. Result and Variant Relationship

A variant may become `AVAILABLE` or `ACTIVE` only when its underlying result is:

```text
AVAILABLE
AVAILABLE_WITH_WARNINGS
```

A partial progressive variant may be supported in the future, but the MVP should avoid activating incomplete whole-document variants unless Presentation explicitly supports segment-subset authority.

---

## 61. Job and Variant Relationship

A completed job should have at least one valid variant.

```text
TranslationJob.COMPLETED
    requires TranslationVariant AVAILABLE or ACTIVE
```

A job should not publish completion before its referenced variant is retrievable.

Recommended event order:

```text
TranslationVariantCreated
        ↓
TranslationCompleted
        ↓
TranslationVariantActivated
```

---

# Part VII — Command-to-State Mapping

## 62. StartTranslation

Accepted `StartTranslation` normally creates:

```text
TranslationJob = CREATED
```

Then:

```text
CREATED → QUEUED
```

or for immediate execution:

```text
CREATED → RUNNING
```

Cache reuse may produce:

```text
CREATED → COMPLETED
```

or reuse an existing completed job without creating a new one.

---

## 63. CancelTranslation

Valid cancellation targets active job states:

```text
CREATED
QUEUED
RUNNING
PARTIALLY_COMPLETED
RETRY_SCHEDULED
```

Preferred transition:

```text
active state
    ↓
CANCELLATION_REQUESTED
    ↓
CANCELLED
```

For jobs with no active resources:

```text
CREATED → CANCELLED
QUEUED → CANCELLED
```

may occur atomically.

---

## 64. RetryTranslation

Automatic or command-driven retry within the same logical intent produces:

```text
Job RUNNING or PARTIALLY_COMPLETED
        ↓
RETRY_SCHEDULED
        ↓
new Attempt CREATED
        ↓
Attempt PREPARING
        ↓
Job RUNNING
```

The previous attempt remains terminal.

---

## 65. RequestRetranslation

Retranslation does not reopen the old job.

It creates:

```text
New TranslationJob = CREATED
```

The previous job may remain:

```text
COMPLETED
COMPLETED_WITH_WARNINGS
FAILED
```

or transition to:

```text
SUPERSEDED
```

when the new job is intended to replace its authority.

---

## 66. InvalidateTranslation

Invalidation may affect different scopes.

### Job scope

```text
Job terminal state → INVALIDATED
```

### Result scope

```text
Result AVAILABLE → INVALIDATED
```

### Variant scope

```text
Variant ACTIVE / AVAILABLE / INACTIVE → INVALIDATED
```

Invalidating one variant does not necessarily invalidate the whole job.

---

## 67. SelectTranslationVariant

Selecting an available variant causes:

```text
Current ACTIVE variant → INACTIVE
Selected AVAILABLE or INACTIVE variant → ACTIVE
```

Both changes should be committed atomically for the same reading context.

---

## 68. SubmitTranslationCorrection

Correction produces a new immutable variant.

```text
Base variant ACTIVE or INACTIVE
        ↓
Corrected variant CREATED
        ↓
Corrected variant AVAILABLE
```

When activation is requested:

```text
Base variant ACTIVE → INACTIVE
Corrected variant AVAILABLE → ACTIVE
```

The base variant is not mutated.

---

# Part VIII — Event-to-State Mapping

## 69. Job Events

| Event                               | Expected state after event                        |
| ----------------------------------- | ------------------------------------------------- |
| `TranslationJobCreated`             | `CREATED`                                         |
| `TranslationJobQueued`              | `QUEUED`                                          |
| `TranslationJobStarted`             | `RUNNING`                                         |
| `TranslationPartialResultAvailable` | `PARTIALLY_COMPLETED` or active progressive state |
| `TranslationRetryScheduled`         | `RETRY_SCHEDULED`                                 |
| `TranslationCancellationRequested`  | `CANCELLATION_REQUESTED`                          |
| `TranslationCompleted`              | `COMPLETED`                                       |
| `TranslationCompletedWithWarnings`  | `COMPLETED_WITH_WARNINGS`                         |
| `TranslationFailed`                 | `FAILED`                                          |
| `TranslationCancelled`              | `CANCELLED`                                       |
| `TranslationSuperseded`             | `SUPERSEDED`                                      |
| `TranslationInvalidated`            | `INVALIDATED` where job scope applies             |

---

## 70. Attempt Events

| Event                         | Expected state after event |
| ----------------------------- | -------------------------- |
| `TranslationAttemptStarted`   | `RUNNING`                  |
| `TranslationAttemptCompleted` | `COMPLETED`                |
| `TranslationAttemptFailed`    | `FAILED`                   |

Attempt creation and preparation may remain internal state transitions without public events.

---

## 71. Batch Events

| Event                       | Expected state after event |
| --------------------------- | -------------------------- |
| `TranslationBatchStarted`   | `RUNNING`                  |
| `TranslationBatchCompleted` | `COMPLETED`                |
| `TranslationBatchFailed`    | `FAILED`                   |

The intermediate `VALIDATING` state may remain internal.

---

## 72. Variant Events

| Event                           | Expected state after event                |
| ------------------------------- | ----------------------------------------- |
| `TranslationVariantCreated`     | `CREATED` or `AVAILABLE`                  |
| `TranslationVariantActivated`   | `ACTIVE`                                  |
| `TranslationVariantInvalidated` | `INVALIDATED`                             |
| `TranslationCorrectionApplied`  | corrected variant `AVAILABLE` or `ACTIVE` |

The exact state associated with `TranslationVariantCreated` depends on whether creation includes completed validation.

Recommended MVP behavior:

```text
TranslationVariantCreated
    means variant is AVAILABLE
```

This avoids publishing events for inaccessible intermediate variants.

---

# Part IX — Retry State Rules

## 73. Retry Eligibility

An attempt failure is retryable only when:

* the normalized error category permits retry;
* retry budget remains;
* the job has not been cancelled;
* the job has not been superseded;
* provider policy has an eligible execution path;
* the source revision remains valid;
* retry would not violate privacy or cost policy.

---

## 74. Retry Transition

```text
Attempt RUNNING
    ↓ failure
Attempt FAILED
    ↓ retry approved
Job RETRY_SCHEDULED
    ↓
New Attempt CREATED
```

The failed attempt remains immutable.

---

## 75. Batch-Only Retry

When only selected batches fail:

```text
Attempt 1
    Batch A COMPLETED
    Batch B FAILED
    Batch C COMPLETED
```

The next attempt may contain only replacement work for Batch B:

```text
Attempt 2
    Batch D
        contains segments previously assigned to Batch B
```

The new batch receives a new `TranslationBatchId`.

Previously completed batches are not moved back to `RUNNING`.

---

## 76. Provider Fallback

Fallback is represented as a new attempt.

```text
Attempt 1
    provider = A
    state = FAILED

Attempt 2
    provider = B
    state = CREATED → RUNNING
```

The job transitions through:

```text
RUNNING
    ↓
RETRY_SCHEDULED
    ↓
RUNNING
```

Expected event:

```text
TranslationProviderFallbackSelected
```

---

## 77. Retry Exhaustion

When no further retry is allowed:

```text
Attempt FAILED
    ↓
Job FAILED
```

When accepted partial output exists, the result may remain:

```text
PARTIAL
```

or become:

```text
NON_AUTHORITATIVE
```

depending on publication policy.

---

# Part X — Partial Completion Rules

## 78. Partial Job Qualification

A job may enter `PARTIALLY_COMPLETED` only when:

* at least one selected segment has accepted output;
* at least one selected segment is incomplete or failed;
* an assembled partial result exists;
* segment identities are explicit.

---

## 79. Progressive Publication

For `publicationMode = PROGRESSIVE`:

```text
Job RUNNING
    ↓ accepted subset
Job PARTIALLY_COMPLETED
```

Presentation may use completed segment subsets.

However:

* incomplete segments remain explicit;
* later revisions must not duplicate completed overlays;
* cancellation prevents newly arriving segments from becoming authoritative;
* source revision changes supersede the job.

---

## 80. Atomic Publication

For `publicationMode = ATOMIC`:

* internal result state may become `PARTIAL`;
* the job may remain `RUNNING`;
* Presentation must not treat partial output as authoritative;
* the final result is published only after complete assembly.

The job-level `PARTIALLY_COMPLETED` state may still be used operationally, but it does not imply public authority.

---

## 81. Finalizing Partial Results

A partial job may finish as:

```text
PARTIALLY_COMPLETED → COMPLETED
```

when all remaining segments succeed.

It may finish as:

```text
PARTIALLY_COMPLETED → COMPLETED_WITH_WARNINGS
```

when complete usable output includes significant warnings.

It may finish as:

```text
PARTIALLY_COMPLETED → FAILED
```

when required content cannot be completed and partial completion is not accepted as successful.

---

# Part XI — Cancellation Rules

## 82. Logical Cancellation

Logical cancellation takes precedence over provider execution state.

Once the job enters:

```text
CANCELLATION_REQUESTED
```

the module must prevent:

* new attempts;
* new batches;
* authoritative result publication;
* automatic retries;
* active variant creation from late output.

---

## 83. Physical Cancellation

Provider cancellation may be:

```text
SUPPORTED
BEST_EFFORT
UNSUPPORTED
```

Physical cancellation outcome does not change logical authority.

Even when provider cancellation is unsupported:

```text
Job CANCELLATION_REQUESTED
    ↓
Job CANCELLED
```

remains valid after local cleanup and authority blocking.

---

## 84. Partial Data on Cancellation

Cancellation policy may choose:

```text
DISCARD
RETAIN_NON_AUTHORITATIVE
RETAIN_ALREADY_PUBLISHED_SEGMENTS
```

For MVP, recommended behavior:

```text
Progressive result already displayed
    → retain completed immutable segments temporarily

Unpublished partial result
    → retain as NON_AUTHORITATIVE only when cache or diagnostics require it
```

---

## 85. Cancellation Race

Possible race:

```text
Batch validation completes
        ↕
Cancellation request arrives
```

Before publication, the module must atomically verify:

```text
Job is not CANCELLATION_REQUESTED
Job is not CANCELLED
Job is not SUPERSEDED
```

If cancellation wins, batch output cannot become authoritative.

---

# Part XII — Supersession Rules

## 86. Supersession Triggers

A job may become superseded when:

* prepared document revision changes;
* a newer job replaces it;
* target language changes;
* profile changes;
* terminology snapshot changes under strict policy;
* reading context no longer permits old work;
* manual retranslation becomes authoritative.

---

## 87. Active Supersession

For active jobs:

```text
RUNNING → SUPERSEDED
PARTIALLY_COMPLETED → SUPERSEDED
RETRY_SCHEDULED → SUPERSEDED
```

All active attempts and batches should also transition to:

```text
SUPERSEDED
```

where practical.

---

## 88. Completed Supersession

A completed job may later become superseded:

```text
COMPLETED → SUPERSEDED
COMPLETED_WITH_WARNINGS → SUPERSEDED
```

Its result normally becomes:

```text
NON_AUTHORITATIVE
```

Its variant normally becomes:

```text
INACTIVE
```

unless it is incompatible, in which case it may become `INVALIDATED`.

---

## 89. Supersession Is Not Invalidation

Superseded output may still be valid for its original source revision.

Invalidated output is no longer trusted even for that original identity.

```text
SUPERSEDED
    = no longer current

INVALIDATED
    = no longer valid
```

---

# Part XIII — Invalidation Rules

## 90. Invalidation Scope

Invalidation may target:

```text
JOB
RESULT
VARIANT
SEGMENTS
CACHE_ENTRY
```

The scope determines which state machines transition.

---

## 91. Job Invalidation

Job invalidation causes:

```text
Job → INVALIDATED
```

Associated active results and variants should normally also be invalidated or made non-authoritative.

---

## 92. Result Invalidation

Result invalidation causes:

```text
Result → INVALIDATED
```

The parent job may remain historically completed, but it cannot expose the invalidated result as authoritative.

If no valid result remains, job-level invalidation should be considered.

---

## 93. Variant Invalidation

Variant invalidation causes:

```text
Variant → INVALIDATED
```

If it was active:

* it must be deactivated;
* a compatible replacement may be activated;
* otherwise no active translation remains.

---

## 94. Segment Invalidation

When only selected translated segments are invalid:

* a new corrected or partial result revision should be created;
* the original immutable result remains historical;
* affected variants may become invalid;
* unaffected segments may remain reusable.

The system must not mutate published segment text in place.

---

# Part XIV — Stale Result Rules

## 95. Stale Result Check

Before result publication, Translation must verify at least:

```text
TranslationJobId
TranslationAttemptId
PreparedDocumentId
ContentRevision
TranslationIntentId
TranslationRevision
```

Where relevant:

```text
ReadingSessionId
TargetLanguage
ContextRevision
GlossaryRevision
Active replacement job
```

---

## 96. Stale Attempt Output

Attempt output is stale when:

* a newer attempt replaced it;
* the attempt was superseded;
* the parent job is cancelled;
* the parent job is superseded;
* source revision changed.

Stale attempt output must not advance:

```text
Batch → COMPLETED
Result → AVAILABLE
Job → COMPLETED
Variant → ACTIVE
```

It may be recorded diagnostically.

---

## 97. Stale Event Handling

Consumers receiving completion events must verify:

```text
preparedDocumentId
contentRevision
translationJobId
translationVariantId
```

before updating visible state.

Event arrival alone does not prove current authority.

---

# Part XV — State Persistence

## 98. Durable Transition Rule

A public event must be emitted only after its corresponding state transition is durable enough to be queried.

Example:

```text
persist Job = COMPLETED
persist Result = AVAILABLE
persist Variant = AVAILABLE
        ↓
publish TranslationCompleted
```

---

## 99. Transactional Consistency

Preferred mechanisms include:

* transactional outbox;
* event sourcing;
* atomic aggregate persistence;
* equivalent reliable state-and-event storage.

The system must avoid:

```text
event published
state persistence failed
```

---

## 100. Optimistic Concurrency

State transitions should verify the expected current state.

Conceptual operation:

```text
transition(
    entityId,
    expectedState,
    nextState,
    expectedRevision
)
```

This prevents races such as:

* completion after cancellation;
* two variants becoming active;
* duplicate retry scheduling;
* older attempts overwriting newer attempts.

---

## 101. State Revision

Each stateful entity should maintain an internal monotonic revision.

Examples:

```text
jobStateRevision
batchStateRevision
attemptStateRevision
translationRevision
variantStateRevision
```

State revision supports:

* optimistic locking;
* event ordering;
* duplicate command handling;
* read-model synchronization.

---

# Part XVI — Invalid Transitions

## 102. Job Invalid Transitions

The following transitions are forbidden:

```text
COMPLETED → RUNNING
COMPLETED_WITH_WARNINGS → RUNNING
FAILED → RUNNING
CANCELLED → RUNNING
SUPERSEDED → RUNNING
INVALIDATED → any normal state

CANCELLATION_REQUESTED → COMPLETED
CANCELLATION_REQUESTED → RETRY_SCHEDULED
```

Recovery requires a new logical job.

---

## 103. Attempt Invalid Transitions

Forbidden:

```text
COMPLETED → RUNNING
FAILED → RUNNING
CANCELLED → RUNNING
SUPERSEDED → RUNNING
```

Retry creates a new attempt.

---

## 104. Batch Invalid Transitions

Forbidden:

```text
COMPLETED → RUNNING
FAILED → RUNNING
CANCELLED → RUNNING
SUPERSEDED → RUNNING
```

Batch retry creates a new batch identity.

---

## 105. Result Invalid Transitions

Forbidden:

```text
INVALIDATED → AVAILABLE
INVALIDATED → PARTIAL
NON_AUTHORITATIVE → AVAILABLE
```

Reusing historical content requires creation or activation of a compatible variant/result model, not direct state rollback.

---

## 106. Variant Invalid Transitions

Forbidden:

```text
INVALIDATED → ACTIVE
INVALIDATED → AVAILABLE
```

A corrected replacement must be a new variant.

---

# Part XVII — State Transition Tables

## 107. Translation Job Transition Table

| Current state             | Trigger                          | Next state                               |
| ------------------------- | -------------------------------- | ---------------------------------------- |
| `CREATED`                 | execution queued                 | `QUEUED`                                 |
| `CREATED`                 | immediate execution begins       | `RUNNING`                                |
| `CREATED`                 | compatible cache result accepted | `COMPLETED` or `COMPLETED_WITH_WARNINGS` |
| `CREATED`                 | cancellation accepted            | `CANCELLATION_REQUESTED` or `CANCELLED`  |
| `CREATED`                 | newer work replaces job          | `SUPERSEDED`                             |
| `CREATED`                 | unrecoverable setup failure      | `FAILED`                                 |
| `QUEUED`                  | scheduler starts execution       | `RUNNING`                                |
| `QUEUED`                  | cancellation accepted            | `CANCELLATION_REQUESTED` or `CANCELLED`  |
| `QUEUED`                  | newer work replaces job          | `SUPERSEDED`                             |
| `RUNNING`                 | validated subset available       | `PARTIALLY_COMPLETED`                    |
| `RUNNING`                 | retry approved                   | `RETRY_SCHEDULED`                        |
| `RUNNING`                 | all work succeeds                | `COMPLETED`                              |
| `RUNNING`                 | all work succeeds with warnings  | `COMPLETED_WITH_WARNINGS`                |
| `RUNNING`                 | no recovery remains              | `FAILED`                                 |
| `RUNNING`                 | cancellation accepted            | `CANCELLATION_REQUESTED`                 |
| `RUNNING`                 | newer work replaces job          | `SUPERSEDED`                             |
| `PARTIALLY_COMPLETED`     | more execution begins            | `RUNNING`                                |
| `PARTIALLY_COMPLETED`     | retry approved                   | `RETRY_SCHEDULED`                        |
| `PARTIALLY_COMPLETED`     | all work succeeds                | `COMPLETED`                              |
| `PARTIALLY_COMPLETED`     | complete with warnings           | `COMPLETED_WITH_WARNINGS`                |
| `PARTIALLY_COMPLETED`     | no recovery remains              | `FAILED`                                 |
| `PARTIALLY_COMPLETED`     | cancellation accepted            | `CANCELLATION_REQUESTED`                 |
| `PARTIALLY_COMPLETED`     | newer work replaces job          | `SUPERSEDED`                             |
| `RETRY_SCHEDULED`         | new attempt starts               | `RUNNING`                                |
| `RETRY_SCHEDULED`         | retry no longer possible         | `FAILED`                                 |
| `RETRY_SCHEDULED`         | cancellation accepted            | `CANCELLATION_REQUESTED`                 |
| `RETRY_SCHEDULED`         | newer work replaces job          | `SUPERSEDED`                             |
| `CANCELLATION_REQUESTED`  | cancellation finalized           | `CANCELLED`                              |
| `CANCELLATION_REQUESTED`  | replacement authority recorded   | `SUPERSEDED`                             |
| `COMPLETED`               | newer work replaces authority    | `SUPERSEDED`                             |
| `COMPLETED`               | result rejected                  | `INVALIDATED`                            |
| `COMPLETED_WITH_WARNINGS` | newer work replaces authority    | `SUPERSEDED`                             |
| `COMPLETED_WITH_WARNINGS` | result rejected                  | `INVALIDATED`                            |
| `FAILED`                  | administrative invalidation      | `INVALIDATED`                            |
| `CANCELLED`               | administrative invalidation      | `INVALIDATED`                            |
| `SUPERSEDED`              | historical result rejected       | `INVALIDATED`                            |

---

## 108. Translation Attempt Transition Table

| Current state         | Trigger                   | Next state            |
| --------------------- | ------------------------- | --------------------- |
| `CREATED`             | preparation starts        | `PREPARING`           |
| `CREATED`             | cancelled                 | `CANCELLED`           |
| `CREATED`             | replaced                  | `SUPERSEDED`          |
| `PREPARING`           | preparation succeeds      | `RUNNING`             |
| `PREPARING`           | preparation fails         | `FAILED`              |
| `PREPARING`           | cancelled                 | `CANCELLED`           |
| `PREPARING`           | replaced                  | `SUPERSEDED`          |
| `RUNNING`             | accepted subset exists    | `PARTIALLY_COMPLETED` |
| `RUNNING`             | assigned work completes   | `COMPLETED`           |
| `RUNNING`             | execution fails           | `FAILED`              |
| `RUNNING`             | cancellation finalized    | `CANCELLED`           |
| `RUNNING`             | replaced by newer attempt | `SUPERSEDED`          |
| `PARTIALLY_COMPLETED` | more batches execute      | `RUNNING`             |
| `PARTIALLY_COMPLETED` | assigned work completes   | `COMPLETED`           |
| `PARTIALLY_COMPLETED` | remaining work fails      | `FAILED`              |
| `PARTIALLY_COMPLETED` | cancelled                 | `CANCELLED`           |
| `PARTIALLY_COMPLETED` | replaced                  | `SUPERSEDED`          |

---

## 109. Translation Batch Transition Table

| Current state | Trigger                    | Next state   |
| ------------- | -------------------------- | ------------ |
| `CREATED`     | batch preparation succeeds | `READY`      |
| `CREATED`     | preparation fails          | `FAILED`     |
| `CREATED`     | cancelled                  | `CANCELLED`  |
| `CREATED`     | replaced                   | `SUPERSEDED` |
| `READY`       | provider execution starts  | `RUNNING`    |
| `READY`       | execution cannot start     | `FAILED`     |
| `READY`       | cancelled                  | `CANCELLED`  |
| `READY`       | replaced                   | `SUPERSEDED` |
| `RUNNING`     | provider output received   | `VALIDATING` |
| `RUNNING`     | provider execution fails   | `FAILED`     |
| `RUNNING`     | cancelled                  | `CANCELLED`  |
| `RUNNING`     | replaced                   | `SUPERSEDED` |
| `VALIDATING`  | validation succeeds        | `COMPLETED`  |
| `VALIDATING`  | validation fails           | `FAILED`     |
| `VALIDATING`  | cancellation wins race     | `CANCELLED`  |
| `VALIDATING`  | source becomes obsolete    | `SUPERSEDED` |

---

## 110. Translation Result Transition Table

| Current state             | Trigger                      | Next state                  |
| ------------------------- | ---------------------------- | --------------------------- |
| `ASSEMBLING`              | validated subset assembled   | `PARTIAL`                   |
| `ASSEMBLING`              | complete candidate assembled | `FINALIZING`                |
| `ASSEMBLING`              | job loses authority          | `NON_AUTHORITATIVE`         |
| `PARTIAL`                 | additional subset assembled  | `PARTIAL` with new revision |
| `PARTIAL`                 | complete candidate assembled | `FINALIZING`                |
| `PARTIAL`                 | job cancelled or superseded  | `NON_AUTHORITATIVE`         |
| `FINALIZING`              | final checks pass            | `AVAILABLE`                 |
| `FINALIZING`              | checks pass with warnings    | `AVAILABLE_WITH_WARNINGS`   |
| `FINALIZING`              | authority check fails        | `NON_AUTHORITATIVE`         |
| `FINALIZING`              | validity check fails         | `INVALIDATED`               |
| `AVAILABLE`               | newer result becomes active  | `NON_AUTHORITATIVE`         |
| `AVAILABLE`               | result rejected              | `INVALIDATED`               |
| `AVAILABLE_WITH_WARNINGS` | newer result becomes active  | `NON_AUTHORITATIVE`         |
| `AVAILABLE_WITH_WARNINGS` | result rejected              | `INVALIDATED`               |
| `NON_AUTHORITATIVE`       | result rejected permanently  | `INVALIDATED`               |

---

## 111. Translation Variant Transition Table

| Current state | Trigger                        | Next state    |
| ------------- | ------------------------------ | ------------- |
| `CREATED`     | variant validation succeeds    | `AVAILABLE`   |
| `CREATED`     | atomic creation and activation | `ACTIVE`      |
| `CREATED`     | validation fails               | `INVALIDATED` |
| `AVAILABLE`   | variant selected               | `ACTIVE`      |
| `AVAILABLE`   | variant rejected               | `INVALIDATED` |
| `ACTIVE`      | another variant selected       | `INACTIVE`    |
| `ACTIVE`      | active variant rejected        | `INVALIDATED` |
| `INACTIVE`    | variant selected again         | `ACTIVE`      |
| `INACTIVE`    | variant rejected               | `INVALIDATED` |

---

# Part XVIII — Derived Status and UI Guidance

## 112. Public State Versus UI State

Presentation may derive simpler user-facing states such as:

```text
WAITING
TRANSLATING
PARTIAL
READY
ERROR
CANCELLED
```

These are UI projections.

They are not canonical Translation domain states.

Example mapping:

```text
CREATED / QUEUED
    → WAITING

RUNNING / RETRY_SCHEDULED
    → TRANSLATING

PARTIALLY_COMPLETED
    → PARTIAL

COMPLETED / COMPLETED_WITH_WARNINGS
    → READY

FAILED
    → ERROR

CANCELLED / SUPERSEDED
    → CANCELLED or hidden
```

---

## 113. Progress Is Not State

Values such as:

```text
30%
7 of 20 segments
2 of 4 batches
```

are progress data, not lifecycle states.

Do not create states such as:

```text
THIRTY_PERCENT_COMPLETE
HALF_TRANSLATED
```

---

## 114. Warning Is Not State by Default

Warnings are attached to results, segments or execution.

Only final job success distinguishes:

```text
COMPLETED
COMPLETED_WITH_WARNINGS
```

Individual warning categories must not become separate lifecycle states.

---

# Part XIX — Recovery and Restart

## 115. Process Restart

After process restart, Translation must restore durable entities to their last known states.

Entities left in transient states may require reconciliation.

Examples:

```text
Attempt RUNNING
Batch RUNNING
Result FINALIZING
```

---

## 116. Reconciliation

A recovery process may determine:

* provider execution definitely completed;
* provider execution status is unknown;
* timeout should be applied;
* batch should fail;
* job should retry;
* job should be cancelled;
* result should resume finalization.

Recovery must use explicit transitions.

It must not silently mark work completed without accepted validated output.

---

## 117. Orphaned RUNNING State

A batch or attempt found in `RUNNING` after a lease or timeout expires may transition to:

```text
FAILED
```

with a retryable infrastructure failure.

The parent job may then enter:

```text
RETRY_SCHEDULED
```

---

## 118. Finalization Recovery

If a result is durable in `FINALIZING`, recovery may rerun idempotent final checks.

It must not create duplicate variants or duplicate completion events.

---

# Part XX — Timeouts

## 119. Queue Timeout

A job waiting too long in `QUEUED` may:

```text
QUEUED → FAILED
```

or remain queued according to policy.

Interactive requests should normally have bounded queue wait.

---

## 120. Attempt Timeout

An attempt timeout causes:

```text
Attempt RUNNING → FAILED
```

The job may then:

```text
RUNNING → RETRY_SCHEDULED
```

or:

```text
RUNNING → FAILED
```

---

## 121. Batch Timeout

A batch timeout causes:

```text
Batch RUNNING → FAILED
```

Other successful batches remain completed.

---

## 122. Job Timeout

A job-level timeout may cause:

```text
RUNNING → FAILED
PARTIALLY_COMPLETED → FAILED
RETRY_SCHEDULED → FAILED
```

or cancellation according to the configured semantic policy.

Recommended distinction:

```text
timeout caused by system execution failure
    → FAILED

timeout caused by caller cancellation deadline
    → CANCELLED
```

---

# Part XXI — Concurrency Rules

## 123. One Active Attempt Policy

The default policy should allow only one active attempt for the same job.

Active means:

```text
CREATED
PREPARING
RUNNING
PARTIALLY_COMPLETED
```

Multiple active attempts may be allowed only for explicit speculative provider comparison.

That capability is deferred beyond MVP.

---

## 124. Concurrent Batches

Multiple batches under one attempt may be `RUNNING` concurrently.

This does not change parent state semantics.

```text
Attempt RUNNING
    ├── Batch A RUNNING
    ├── Batch B COMPLETED
    └── Batch C VALIDATING
```

---

## 125. Variant Activation Concurrency

For one compatible reading context, activation must ensure:

```text
at most one ACTIVE variant
```

The transition:

```text
Old ACTIVE → INACTIVE
New AVAILABLE → ACTIVE
```

must be atomic.

---

## 126. Completion Versus Supersession Race

Possible race:

```text
Job completion
        ↕
New job supersedes old job
```

Authority selection must use optimistic state revision or equivalent locking.

Only one may win:

```text
Old job COMPLETED and authoritative
```

or:

```text
Old job SUPERSEDED
```

If completion persists first but replacement activates immediately afterward:

```text
Old job COMPLETED → SUPERSEDED
```

is valid.

---

# Part XXII — Core State Invariants

## 127. Invariant 1 — One initial job state

Every new Translation job begins in `CREATED`.

## 128. Invariant 2 — Terminal attempts never restart

A completed, failed, cancelled or superseded attempt never returns to active execution.

## 129. Invariant 3 — Terminal batches never restart

A completed, failed, cancelled or superseded batch never returns to active execution.

## 130. Invariant 4 — Retry creates a new attempt

Retry does not mutate a failed attempt back to `RUNNING`.

## 131. Invariant 5 — Batch retry creates a new batch

A failed batch instance is immutable.

## 132. Invariant 6 — Cancellation blocks publication

A job in `CANCELLATION_REQUESTED` or `CANCELLED` cannot publish an authoritative result.

## 133. Invariant 7 — Supersession blocks publication

A superseded job cannot become authoritative.

## 134. Invariant 8 — Batch completion requires validation

Provider response receipt alone cannot produce `Batch.COMPLETED`.

## 135. Invariant 9 — Partial means aligned output exists

A partial state requires at least one validated translated segment.

## 136. Invariant 10 — Missing segments remain explicit

Partial and failed results cannot silently omit selected segments.

## 137. Invariant 11 — Result authority is separate from existence

A result may exist while remaining `NON_AUTHORITATIVE`.

## 138. Invariant 12 — Variants are immutable

Correction or retranslation creates a new variant.

## 139. Invariant 13 — At most one active compatible variant

One reading context cannot have multiple active variants for the same source revision and target language.

## 140. Invariant 14 — Invalidation is terminal

Invalidated entities cannot return to valid active states.

## 141. Invariant 15 — Source revision remains fixed

A Translation job never changes its `PreparedDocumentId` or `ContentRevision`.

## 142. Invariant 16 — Attempt belongs to one job

A `TranslationAttempt` cannot move between jobs.

## 143. Invariant 17 — Batch belongs to one attempt

A `TranslationBatch` cannot move between attempts.

## 144. Invariant 18 — State events follow persistence

Events must not announce transitions that cannot be queried afterward.

---

# Part XXIII — MVP Decisions

## 145. MVP Job States

The MVP should implement all canonical job states:

```text
CREATED
QUEUED
RUNNING
PARTIALLY_COMPLETED
RETRY_SCHEDULED
CANCELLATION_REQUESTED
COMPLETED
COMPLETED_WITH_WARNINGS
FAILED
CANCELLED
SUPERSEDED
INVALIDATED
```

Some deployments may skip persistence of brief states such as `QUEUED`, but their semantic transition must remain supported.

---

## 146. MVP Attempt States

Required:

```text
CREATED
PREPARING
RUNNING
COMPLETED
FAILED
CANCELLED
SUPERSEDED
```

`PARTIALLY_COMPLETED` is recommended but may be derived from batch progress initially.

---

## 147. MVP Batch States

Required:

```text
CREATED
READY
RUNNING
VALIDATING
COMPLETED
FAILED
CANCELLED
SUPERSEDED
```

`VALIDATING` should remain explicit because provider success is not equivalent to accepted translation success.

---

## 148. MVP Result States

Required:

```text
ASSEMBLING
PARTIAL
FINALIZING
AVAILABLE
AVAILABLE_WITH_WARNINGS
NON_AUTHORITATIVE
INVALIDATED
```

---

## 149. MVP Variant States

Required:

```text
AVAILABLE
ACTIVE
INACTIVE
INVALIDATED
```

`CREATED` may remain an internal transient state if variant construction and availability are committed atomically.

---

# Part XXIV — Open Decisions

## 150. Failed Job Reopening

The project must decide whether explicit `RetryTranslation` may reopen a terminal failed job.

Recommended decision:

```text
Automatic retry
    → same job, before FAILED

Manual retry after final FAILED
    → new derived TranslationJob
```

This keeps terminal state semantics simple.

---

## 151. Partial Job State in Atomic Mode

The project must decide whether an atomically published job should expose `PARTIALLY_COMPLETED` externally.

Recommended approach:

* persist partial result progress internally;
* allow job state `PARTIALLY_COMPLETED`;
* expose that state to monitoring and progress UI;
* do not expose partial content to Presentation.

---

## 152. Completed-to-Superseded Transition

This document permits:

```text
COMPLETED → SUPERSEDED
```

because newer translation work may replace an old completed result.

An alternative is to keep the job completed and only mark its result or variant non-authoritative.

Recommended CRAI approach:

```text
Job remains historically COMPLETED
Variant becomes INACTIVE
Result becomes NON_AUTHORITATIVE
```

However, when job-level authority needs direct representation, `SUPERSEDED` is useful.

This should be finalized before implementation.

---

## 153. Recommended Supersession Model

To avoid losing historical execution outcome, the implementation may separate:

```text
executionState
authorityState
```

For example:

```text
executionState = COMPLETED
authorityState = SUPERSEDED
```

This is more precise than replacing `COMPLETED` with `SUPERSEDED`.

For the current architecture document, `SUPERSEDED` remains a job lifecycle state for simplicity.

Before code implementation, the team should decide whether to split these dimensions.

---

## 154. Cancellation of Already Published Progressive Segments

The project must decide whether already displayed progressive segments remain visible after cancellation.

Recommended behavior:

```text
User manually cancels translation
    → preserve already displayed completed segments until navigation changes

Source revision changes
    → remove or replace stale displayed segments immediately
```

Presentation owns visual removal.

Translation provides authority status.

---

## 155. Variant Availability Event

The project must decide whether:

```text
TranslationVariantCreated
```

means:

```text
state = CREATED
```

or:

```text
state = AVAILABLE
```

Recommended behavior:

```text
TranslationVariantCreated
    means the variant is valid and AVAILABLE
```

Internal `CREATED` need not be publicly observable.

---

# Part XXV — Related Documents

```text
modules/translation/MODULE.md
modules/translation/CONTRACT.md
modules/translation/EVENTS.md
modules/translation/ERRORS.md
modules/translation/README.md
```

Architecture references:

```text
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Upstream references:

```text
modules/text-processing/MODULE.md
modules/text-processing/CONTRACTS.md
modules/text-processing/EVENTS.md
modules/text-processing/STATES.md
```

Future integration references:

```text
modules/reading-session/STATES.md
modules/presentation/STATES.md
modules/knowledge/STATES.md
modules/provider-management/STATES.md
```

---

# 156. Summary

Translation uses separate state machines for:

```text
TranslationJob
TranslationAttempt
TranslationBatch
TranslationResult
TranslationVariant
```

The primary job lifecycle is:

```text
CREATED
    ↓
QUEUED
    ↓
RUNNING
    ↓
PARTIALLY_COMPLETED
    ↓
COMPLETED
```

Retry path:

```text
RUNNING
    ↓
RETRY_SCHEDULED
    ↓
RUNNING
```

Cancellation path:

```text
RUNNING
    ↓
CANCELLATION_REQUESTED
    ↓
CANCELLED
```

Failure path:

```text
RUNNING
    ↓
FAILED
```

Replacement path:

```text
RUNNING or COMPLETED
    ↓
SUPERSEDED
```

Validity path:

```text
COMPLETED
    ↓
INVALIDATED
```

Execution hierarchy:

```text
TranslationJob
      ↓
TranslationAttempt
      ↓
TranslationBatch
      ↓
TranslationResult
      ↓
TranslationVariant
```

The key rules are:

* retry creates a new attempt;
* batch retry creates a new batch;
* provider output must pass validation before completion;
* partial results remain explicitly incomplete;
* cancellation and supersession block authority;
* results may exist without being authoritative;
* variants are immutable;
* invalidated entities never return to active states;
* state transitions must be durable before events are published.
