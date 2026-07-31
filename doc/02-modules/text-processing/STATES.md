# Text Processing Module States

> **Project:** CRAI
> **Module:** `text-processing`
> **Document:** Module State Model
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-25

---

## 1. Purpose

This document defines the state model used by the `text-processing` module.

The module transforms extracted or structured source text into normalized, ordered, translation-ready segments.

Its responsibilities may include:

* Unicode and punctuation normalization;
* whitespace cleanup;
* OCR artifact cleanup;
* reading-order validation;
* line merging;
* sentence segmentation;
* paragraph reconstruction;
* dialogue grouping;
* context construction;
* language-profile processing;
* preparation of translation units.

This document defines:

* processing job states;
* valid state transitions;
* cancellation and supersession behavior;
* retry behavior;
* partial processing behavior;
* state ownership;
* emitted state-related events;
* invariants that implementations must preserve.

This document does not define:

* reading-session states;
* OCR provider states;
* translation request states;
* UI presentation states;
* persistence implementation;
* concrete algorithms for normalization or segmentation.

Those concerns belong to their respective modules and architecture documents.

---

## 2. Module Boundary

The `text-processing` module begins after usable source text and its structural metadata have been produced.

Typical upstream inputs include:

```text
Structured browser text
OCR-recognized text regions
Imported document text
Clipboard text
User-corrected source text
```

The module ends when it produces translation-ready output.

```text
Raw or extracted text
    ↓
Text processing
    ↓
Prepared text segments
    ↓
Translation module
```

The module must not:

* call a translation provider directly;
* render processed text in the user interface;
* modify OCR-owned region geometry;
* manage the lifecycle of the reading session;
* decide whether a result is still current for the whole application;
* persist permanent reading history by itself.

It may reject work that has already been marked as stale, cancelled, or superseded by its caller.

---

## 3. State Ownership

The `text-processing` module owns the lifecycle of a:

```text
TextProcessingJob
```

A job represents one attempt to transform a specific source-text snapshot into translation-ready segments.

A job should normally be scoped by identifiers such as:

```text
ReadingSessionId
ContentId
ContentRevision
TextProcessingJobId
```

Optional identifiers may include:

```text
FrameId
RegionSetId
DocumentId
ChapterId
SourceRevision
ParentJobId
```

The `text-processing` module owns the state of the job itself.

The caller owns the decision of whether the job remains relevant to the active reading experience.

---

## 4. State Model

### 4.1 Primary States

A `TextProcessingJob` uses the following primary states:

```text
CREATED
QUEUED
VALIDATING
NORMALIZING
STRUCTURING
BUILDING_CONTEXT
FINALIZING
COMPLETED

CANCEL_REQUESTED
CANCELLED
SUPERSEDED
FAILED
```

The normal successful path is:

```text
CREATED
    ↓
QUEUED
    ↓
VALIDATING
    ↓
NORMALIZING
    ↓
STRUCTURING
    ↓
BUILDING_CONTEXT
    ↓
FINALIZING
    ↓
COMPLETED
```

Terminal states are:

```text
COMPLETED
CANCELLED
SUPERSEDED
FAILED
```

Once a job enters a terminal state, it must not return to an active state.

---

## 5. State Definitions

## 5.1 `CREATED`

The processing job has been accepted and assigned an identity, but no processing work has started.

At this point:

* the input snapshot is attached to the job;
* processing options are captured;
* source identifiers are recorded;
* cancellation may already be requested;
* the job has not yet consumed worker capacity.

Typical entry condition:

```text
A valid TextProcessingRequested command is accepted.
```

Allowed next states:

```text
QUEUED
CANCEL_REQUESTED
CANCELLED
SUPERSEDED
FAILED
```

A job may fail directly from `CREATED` when mandatory metadata cannot be initialized.

---

## 5.2 `QUEUED`

The job is waiting for execution capacity.

Possible reasons include:

* worker concurrency limits;
* priority scheduling;
* batching;
* resource throttling;
* waiting for a previous revision to finish cancellation;
* backpressure from downstream processing.

No text transformation should occur while the job remains only in `QUEUED`.

Allowed next states:

```text
VALIDATING
CANCEL_REQUESTED
CANCELLED
SUPERSEDED
FAILED
```

The implementation should avoid allowing obsolete jobs to remain queued indefinitely.

When a newer content revision replaces the job before execution, the preferred terminal state is:

```text
SUPERSEDED
```

rather than `CANCELLED`.

---

## 5.3 `VALIDATING`

The module verifies that the input is processable.

Validation may include:

* checking required identifiers;
* checking that input text or regions exist;
* validating source revision metadata;
* validating region and segment identifiers;
* checking supported language-profile configuration;
* checking reading-direction metadata;
* checking structural consistency;
* rejecting malformed or excessively large requests;
* detecting duplicate segment identifiers;
* validating user correction references.

Validation must not perform translation.

Allowed next states:

```text
NORMALIZING
FINALIZING
CANCEL_REQUESTED
SUPERSEDED
FAILED
```

The job may move directly to `FINALIZING` when:

* no transformable text exists;
* the input is intentionally empty;
* all input segments are filtered out for a documented reason;
* the input is already fully prepared and only output validation is required.

An empty result is not automatically a failure.

The result must explicitly state why no translation-ready segments were produced.

---

## 5.4 `NORMALIZING`

The module transforms raw text into a stable textual representation.

Typical operations include:

* Unicode normalization;
* line-ending normalization;
* whitespace cleanup;
* punctuation normalization;
* removal of control characters;
* script-specific punctuation handling;
* OCR artifact cleanup;
* repeated-character cleanup;
* preservation of intentional paragraph boundaries;
* preservation of source-to-output mappings.

Normalization should be deterministic for the same:

```text
Input
ProcessingProfile
ModuleVersion
```

Allowed next states:

```text
STRUCTURING
CANCEL_REQUESTED
SUPERSEDED
FAILED
```

Cancellation should be checked between bounded units of work, such as:

* source regions;
* paragraphs;
* blocks;
* batches of lines.

---

## 5.5 `STRUCTURING`

The normalized content is converted into logical reading and translation units.

Typical operations include:

* line merging;
* sentence segmentation;
* paragraph reconstruction;
* dialogue grouping;
* reading-order correction;
* preservation of region relationships;
* reconstruction of text split across OCR regions;
* assignment of stable segment order;
* detection of likely names or terms;
* classification of segments as prose, dialogue, title, note, or unknown.

The module should preserve enough source mapping to relate every produced segment back to its original input.

For image-derived content, this normally includes:

```text
RegionId
SourceTextSpan
ReadingOrder
```

For structured text, this may include:

```text
NodeId
ParagraphId
SourceTextSpan
DocumentOrder
```

Allowed next states:

```text
BUILDING_CONTEXT
FINALIZING
CANCEL_REQUESTED
SUPERSEDED
FAILED
```

The job may skip `BUILDING_CONTEXT` when context construction is disabled or unnecessary.

---

## 5.6 `BUILDING_CONTEXT`

The module constructs context packages that can later be consumed by the translation module.

Context may include:

* neighboring segments;
* paragraph context;
* dialogue-group context;
* page-local context;
* chapter-local context references;
* detected names and terms;
* user glossary candidates;
* style hints;
* content-type hints;
* reading order;
* source language information.

This state does not translate the content.

The module only prepares context and translation-unit relationships.

Example:

```text
Segment 12
├── previous segment: Segment 11
├── next segment: Segment 13
├── dialogue group: Group 4
├── detected names: [NameCandidateA]
└── source regions: [Region 18, Region 19]
```

Allowed next states:

```text
FINALIZING
CANCEL_REQUESTED
SUPERSEDED
FAILED
```

Context construction must respect configured limits.

A long chapter must not be copied into every segment context without bounds.

---

## 5.7 `FINALIZING`

The module validates and assembles the final processing result.

Finalization may include:

* assigning final segment identifiers;
* verifying deterministic ordering;
* validating source mappings;
* attaching warnings;
* calculating output fingerprints;
* generating processing summaries;
* checking that no duplicate output segment identifiers exist;
* ensuring segment boundaries are valid;
* marking partially processed units;
* preparing immutable result objects.

No externally consumable successful result should be published before finalization completes.

Allowed next states:

```text
COMPLETED
CANCEL_REQUESTED
SUPERSEDED
FAILED
```

Cancellation received during finalization may be handled in one of two ways:

1. cancel before result publication; or
2. finish finalization but suppress result publication.

The implementation must not publish a result after acknowledging the job as cancelled or superseded.

---

## 5.8 `COMPLETED`

The processing job finished successfully and produced a final result.

The result may contain:

* one or more translation-ready segments;
* an intentionally empty segment list;
* source-to-segment mappings;
* segment order;
* context descriptors;
* warnings;
* processing metrics;
* processing profile and version;
* output fingerprint.

`COMPLETED` means:

```text
The text-processing module completed its own responsibility.
```

It does not mean:

* translation succeeded;
* the result was presented;
* the result is still current;
* the result was persisted;
* the user accepted the result.

Before using a completed result, the orchestrating component must still verify that its content revision remains current.

`COMPLETED` is terminal.

---

## 5.9 `CANCEL_REQUESTED`

A cancellation request has been accepted, but active work may not yet have stopped.

This state exists because cancellation is cooperative.

Possible cancellation sources include:

* user stops the reading session;
* user pauses processing with cancellation semantics;
* source is removed;
* application shuts down;
* processing timeout expires;
* orchestrator cancels a pipeline;
* a manual correction replaces the active input;
* resource policy rejects continuation.

While in `CANCEL_REQUESTED`:

* no new expensive processing phase should begin;
* active algorithms should stop at the next safe interruption point;
* no successful result should be published;
* owned temporary resources should be released;
* cancellation reason should be retained.

Allowed next states:

```text
CANCELLED
SUPERSEDED
FAILED
```

A job should not normally return from `CANCEL_REQUESTED` to an active processing state.

If processing already finished before cancellation was accepted, the state transition must be resolved atomically:

```text
COMPLETED
```

or:

```text
CANCEL_REQUESTED → CANCELLED
```

but never both.

---

## 5.10 `CANCELLED`

The job stopped because cancellation was requested.

Typical reasons include:

```text
USER_CANCELLED
SESSION_STOPPED
SOURCE_REMOVED
APPLICATION_SHUTDOWN
TIME_BUDGET_EXCEEDED
PIPELINE_CANCELLED
RESOURCE_POLICY
```

A cancelled job must not publish translation-ready output as the active result.

Diagnostic information may still retain:

* last completed phase;
* processed unit count;
* cancellation reason;
* elapsed time;
* partial internal metrics.

Partially built output should not be exposed as a normal successful result unless a future contract explicitly introduces partial-result publication.

`CANCELLED` is terminal.

---

## 5.11 `SUPERSEDED`

The job became obsolete because a newer revision replaced its input.

Examples:

* the user scrolled to different content;
* a new screen frame became stable;
* OCR correction produced a new text revision;
* region order was manually corrected;
* browser content changed;
* a newer job for the same processing slot was accepted;
* processing configuration changed;
* the source language was changed.

`SUPERSEDED` is semantically different from `CANCELLED`.

```text
CANCELLED
    = the work was explicitly stopped.

SUPERSEDED
    = the work is no longer relevant because newer work exists.
```

Superseded jobs should stop as quickly as safely possible.

Their result must never replace the result of a newer revision.

A superseded job may reference:

```text
SupersededByJobId
SupersededByContentRevision
```

`SUPERSEDED` is terminal.

---

## 5.12 `FAILED`

The module could not complete the job because of an error.

Possible failure categories include:

```text
INVALID_INPUT
UNSUPPORTED_LANGUAGE_PROFILE
MALFORMED_STRUCTURE
PROCESSING_LIMIT_EXCEEDED
NORMALIZATION_ERROR
STRUCTURING_ERROR
CONTEXT_BUILD_ERROR
INVARIANT_VIOLATION
INTERNAL_ERROR
DEPENDENCY_UNAVAILABLE
RESOURCE_EXHAUSTED
```

A failure result should include:

* stable error code;
* failure category;
* failed phase;
* retryability;
* safe diagnostic message;
* internal cause where permitted;
* processed unit count;
* module version;
* correlation identifiers.

`FAILED` is terminal for that job attempt.

A retry must create a new job or attempt identity.

The existing failed job must not be moved back into an active state.

---

## 6. State Transition Table

| Current state      | Allowed next state | Primary trigger                                    |
| ------------------ | ------------------ | -------------------------------------------------- |
| `CREATED`          | `QUEUED`           | Job accepted for scheduling                        |
| `CREATED`          | `CANCEL_REQUESTED` | Cancellation accepted before scheduling            |
| `CREATED`          | `CANCELLED`        | Job cancelled before work begins                   |
| `CREATED`          | `SUPERSEDED`       | Newer revision already exists                      |
| `CREATED`          | `FAILED`           | Initialization failure                             |
| `QUEUED`           | `VALIDATING`       | Worker starts the job                              |
| `QUEUED`           | `CANCEL_REQUESTED` | Cancellation accepted                              |
| `QUEUED`           | `CANCELLED`        | Queued job removed before execution                |
| `QUEUED`           | `SUPERSEDED`       | Newer job replaces queued job                      |
| `QUEUED`           | `FAILED`           | Scheduler or resource failure                      |
| `VALIDATING`       | `NORMALIZING`      | Input accepted                                     |
| `VALIDATING`       | `FINALIZING`       | No transformable content or already prepared input |
| `VALIDATING`       | `CANCEL_REQUESTED` | Cancellation accepted                              |
| `VALIDATING`       | `SUPERSEDED`       | Input revision becomes stale                       |
| `VALIDATING`       | `FAILED`           | Validation failure                                 |
| `NORMALIZING`      | `STRUCTURING`      | Normalization completed                            |
| `NORMALIZING`      | `CANCEL_REQUESTED` | Cancellation accepted                              |
| `NORMALIZING`      | `SUPERSEDED`       | Input revision becomes stale                       |
| `NORMALIZING`      | `FAILED`           | Normalization failure                              |
| `STRUCTURING`      | `BUILDING_CONTEXT` | Context construction required                      |
| `STRUCTURING`      | `FINALIZING`       | Context construction skipped                       |
| `STRUCTURING`      | `CANCEL_REQUESTED` | Cancellation accepted                              |
| `STRUCTURING`      | `SUPERSEDED`       | Input revision becomes stale                       |
| `STRUCTURING`      | `FAILED`           | Structuring failure                                |
| `BUILDING_CONTEXT` | `FINALIZING`       | Context construction completed                     |
| `BUILDING_CONTEXT` | `CANCEL_REQUESTED` | Cancellation accepted                              |
| `BUILDING_CONTEXT` | `SUPERSEDED`       | Input revision becomes stale                       |
| `BUILDING_CONTEXT` | `FAILED`           | Context construction failure                       |
| `FINALIZING`       | `COMPLETED`        | Final result committed                             |
| `FINALIZING`       | `CANCEL_REQUESTED` | Cancellation wins before commit                    |
| `FINALIZING`       | `SUPERSEDED`       | Newer revision wins before commit                  |
| `FINALIZING`       | `FAILED`           | Final validation or commit failure                 |
| `CANCEL_REQUESTED` | `CANCELLED`        | Active work stops                                  |
| `CANCEL_REQUESTED` | `SUPERSEDED`       | Cancellation reason is refined to supersession     |
| `CANCEL_REQUESTED` | `FAILED`           | Cleanup or cancellation handling fails critically  |

Transitions from a terminal state are forbidden.

---

## 7. Transition Diagram

```text
                         ┌──────────────┐
                         │   CREATED    │
                         └──────┬───────┘
                                │
                                ▼
                         ┌──────────────┐
                         │    QUEUED    │
                         └──────┬───────┘
                                │
                                ▼
                         ┌──────────────┐
                         │  VALIDATING  │
                         └──────┬───────┘
                                │
                                ▼
                         ┌──────────────┐
                         │ NORMALIZING  │
                         └──────┬───────┘
                                │
                                ▼
                         ┌──────────────┐
                         │ STRUCTURING  │
                         └──────┬───────┘
                                │
                   ┌────────────┴────────────┐
                   │                         │
                   ▼                         │
            ┌──────────────────┐             │
            │ BUILDING_CONTEXT │             │
            └────────┬─────────┘             │
                     │                       │
                     └───────────┬───────────┘
                                 ▼
                          ┌──────────────┐
                          │  FINALIZING  │
                          └──────┬───────┘
                                 │
                                 ▼
                          ┌──────────────┐
                          │  COMPLETED   │
                          └──────────────┘
```

From any non-terminal state, the job may also transition toward:

```text
CANCEL_REQUESTED → CANCELLED
SUPERSEDED
FAILED
```

Subject to the transition rules defined above.

---

## 8. Processing Phase versus Job State

Implementations may require more detailed progress than the primary state model provides.

Such detail should be represented as metadata rather than adding many externally visible primary states.

Example:

```text
state: STRUCTURING
phaseDetail: RECONSTRUCTING_PARAGRAPHS
progress:
    completedUnits: 24
    totalUnits: 40
```

Possible phase details include:

```text
VALIDATING_IDENTIFIERS
VALIDATING_STRUCTURE
NORMALIZING_UNICODE
NORMALIZING_WHITESPACE
CLEANING_OCR_ARTIFACTS
MERGING_LINES
SEGMENTING_SENTENCES
RECONSTRUCTING_PARAGRAPHS
GROUPING_DIALOGUE
CORRECTING_READING_ORDER
DETECTING_TERMS
BUILDING_NEIGHBOR_CONTEXT
BUILDING_DIALOGUE_CONTEXT
VALIDATING_OUTPUT
CALCULATING_FINGERPRINT
```

Phase details:

* are implementation-extensible;
* must not change the meaning of primary states;
* should not be required for cross-module correctness;
* may be used for diagnostics and progress display.

---

## 9. Job Input Snapshot

Every job must operate on an immutable logical input snapshot.

A job input should contain or reference:

```text
TextProcessingJobId
ReadingSessionId
ContentId
ContentRevision
SourceType
SourceLanguage
ReadingDirection
InputSegments
ProcessingProfile
CreatedAt
CorrelationId
```

Optional data may include:

```text
RegionGeometry
DocumentStructure
OCRConfidence
UserCorrections
GlossarySnapshot
ContentType
StyleHint
PreviousContextReference
Deadline
Priority
```

The job must not silently begin processing a newer mutable input under the same identity.

When source content changes, the caller should create:

```text
a new ContentRevision
and
a new TextProcessingJob
```

---

## 10. Output Model

A completed job should produce a `TextProcessingResult`.

Conceptual structure:

```text
TextProcessingResult
├── textProcessingJobId
├── readingSessionId
├── contentId
├── contentRevision
├── outputRevision
├── sourceLanguage
├── processingProfile
├── processingVersion
├── segments[]
├── warnings[]
├── statistics
├── inputFingerprint
├── outputFingerprint
└── completedAt
```

Each prepared segment should retain source traceability.

```text
PreparedTextSegment
├── segmentId
├── order
├── normalizedText
├── segmentType
├── sourceMappings[]
├── contextReferences[]
├── detectedTerms[]
├── warnings[]
└── metadata
```

A source mapping may be:

```text
SourceMapping
├── sourceSegmentId
├── regionId
├── paragraphId
├── sourceStart
├── sourceEnd
└── contributionType
```

Not every source type must populate every field.

---

## 11. Partial Processing

### 11.1 Default Rule

The default module contract should be atomic from the consumer's perspective.

```text
No normal result is published
until the job reaches COMPLETED.
```

Internally processed segments may exist before completion, but they are not considered a committed result.

This prevents:

* incomplete ordering;
* missing context;
* unstable segment identifiers;
* duplicated downstream translation requests;
* UI flicker;
* stale partial results replacing newer complete results.

### 11.2 Future Incremental Mode

Long structured-text content may later require incremental processing.

Possible future use cases include:

* long web-novel chapters;
* large imported documents;
* streaming browser content;
* progressive translation.

Incremental publication should use explicit batch identities such as:

```text
TextProcessingBatchId
BatchIndex
BatchRevision
IsFinalBatch
```

It must not overload the meaning of `COMPLETED`.

A job is only `COMPLETED` after all required batches have been finalized.

Incremental processing is not required for the first image-reading MVP.

---

## 12. Cancellation Semantics

Cancellation is cooperative and idempotent.

Repeated cancellation requests must not cause duplicate terminal events or invalid transitions.

A cancellation request should include:

```text
TextProcessingJobId
Reason
RequestedAt
RequestedBy
CorrelationId
```

Cancellation checks should occur:

* before each processing phase;
* between bounded groups of source units;
* before expensive language-profile operations;
* before final result commit;
* before event publication.

Cancellation must not leave the job indefinitely in `CANCEL_REQUESTED`.

A watchdog or orchestrator may treat an excessive cancellation delay as an implementation failure.

---

## 13. Supersession Semantics

Supersession protects continuous reading flows from stale work.

A job should be superseded when a newer job owns the same logical processing slot.

A logical processing slot may be identified by:

```text
ReadingSessionId
ContentId
ProcessingPurpose
```

Example:

```text
Job A
contentRevision: 12

Job B
contentRevision: 13
```

When Job B becomes authoritative, Job A should not publish a usable result.

Rules:

1. A higher revision must not be replaced by a lower revision.
2. Job completion order must not determine content authority.
3. Supersession should be decided from explicit revision or generation metadata.
4. A superseded result may be cached only if the cache policy permits it.
5. A superseded result must not be presented as current.
6. `SupersededByJobId` should be recorded when known.

---

## 14. Retry Model

A failed job must not be reset to `QUEUED`.

Retry creates a new attempt.

Conceptual model:

```text
TextProcessingOperationId
├── Attempt 1 — FAILED
├── Attempt 2 — FAILED
└── Attempt 3 — COMPLETED
```

Each retry should receive:

```text
New TextProcessingJobId
Same or linked operation identity
Incremented attempt number
Explicit parent job reference
```

Retry may be appropriate for:

* temporary resource exhaustion;
* transient dependency failure;
* recoverable worker failure;
* temporary language-profile component failure.

Retry is normally inappropriate for:

* invalid input;
* unsupported configuration;
* invariant violation;
* deterministic malformed structure;
* input exceeding hard limits.

Retries must be bounded.

The module must expose whether a failure is considered retryable but should not own global retry policy unless explicitly configured to do so.

---

## 15. Empty Input and Empty Output

Empty text must be handled explicitly.

Possible cases include:

### Valid empty input

Examples:

* a page contains no readable text;
* OCR produced no accepted regions;
* structured extraction found decorative content only.

Possible result:

```text
COMPLETED
segments: []
completionReason: NO_PROCESSABLE_TEXT
```

### Invalid missing input

Examples:

* the request claims to contain segments but none are attached;
* required source references are unresolved;
* the input schema is malformed.

Possible result:

```text
FAILED
errorCode: INVALID_INPUT
```

The distinction between valid empty content and invalid input must not be inferred solely from string length.

---

## 16. Warnings versus Failures

A warning does not prevent completion.

Examples of warnings:

```text
LOW_OCR_CONFIDENCE
AMBIGUOUS_READING_ORDER
UNKNOWN_LANGUAGE
MIXED_SCRIPT
UNMERGED_FRAGMENT
POSSIBLE_NAME_AMBIGUITY
CONTEXT_TRUNCATED
PARAGRAPH_RECONSTRUCTION_UNCERTAIN
UNSUPPORTED_PUNCTUATION_PATTERN
PARTIAL_SOURCE_MAPPING
```

A job may reach:

```text
COMPLETED_WITH_WARNINGS
```

as a derived presentation label, but `COMPLETED_WITH_WARNINGS` should not be a separate primary state.

The primary state remains:

```text
COMPLETED
```

with:

```text
warnings.length > 0
```

Failures are reserved for conditions where a valid committed result cannot be produced.

---

## 17. State Events

The module may emit state events through the event bus.

Recommended events:

```text
TextProcessingJobCreated
TextProcessingJobQueued
TextProcessingStarted
TextProcessingPhaseChanged
TextProcessingProgressed
TextProcessingCancellationRequested
TextProcessingCancelled
TextProcessingSuperseded
TextProcessingCompleted
TextProcessingFailed
```

Not every internal phase transition must become a public event.

The minimum externally useful event set is:

```text
TextProcessingStarted
TextProcessingCompleted
TextProcessingCancelled
TextProcessingSuperseded
TextProcessingFailed
```

---

## 18. Event Payload Requirements

State-related events should include:

```text
eventId
eventType
occurredAt
textProcessingJobId
readingSessionId
contentId
contentRevision
correlationId
causationId
moduleVersion
```

Events representing terminal states should additionally include relevant terminal metadata.

### Completion event

```text
resultReference
segmentCount
warningCount
inputFingerprint
outputFingerprint
durationMs
```

### Cancellation event

```text
cancellationReason
lastCompletedPhase
processedUnitCount
durationMs
```

### Supersession event

```text
supersededByJobId
supersededByContentRevision
lastCompletedPhase
```

### Failure event

```text
errorCode
failureCategory
failedPhase
retryable
safeMessage
durationMs
```

Events must not include raw source text by default.

Raw text may contain private or copyrighted reading content and should only appear in explicitly enabled diagnostics.

---

## 19. Commands Affecting State

Likely commands include:

```text
CreateTextProcessingJob
QueueTextProcessingJob
StartTextProcessingJob
CancelTextProcessingJob
SupersedeTextProcessingJob
RetryTextProcessingOperation
```

Commands describe intent.

Events describe accepted state changes.

Example:

```text
CancelTextProcessingJob
    ↓
TextProcessingCancellationRequested
    ↓
TextProcessingCancelled
```

A command may be rejected without changing state.

Example rejection reasons:

```text
JOB_NOT_FOUND
JOB_ALREADY_TERMINAL
REVISION_MISMATCH
CALLER_NOT_AUTHORIZED
INVALID_COMMAND
```

---

## 20. Idempotency

Job creation and terminal-state publication must support idempotent handling.

Recommended rules:

1. The same creation idempotency key must not create multiple active jobs.
2. Repeated cancellation commands must be safe.
3. Repeated terminal event delivery must not produce repeated downstream work.
4. Result commit and `TextProcessingCompleted` publication must be coordinated.
5. Consumers must deduplicate events using `eventId`.
6. Consumers should also guard against duplicate completion using `TextProcessingJobId`.

A job must have exactly one terminal outcome.

---

## 21. Concurrency Rules

Several jobs may exist concurrently for:

* different reading sessions;
* different content items;
* independent chapters;
* separate user corrections;
* different processing profiles.

Jobs targeting the same logical processing slot require ordering protection.

The implementation should compare:

```text
ContentRevision
ProcessingGeneration
JobCreationSequence
```

rather than relying on wall-clock timestamps alone.

A slower old job must not overwrite a faster new job.

Example:

```text
Job A — revision 20 — starts first
Job B — revision 21 — starts later
Job B — completes first
Job A — completes later
```

Required outcome:

```text
Job B remains authoritative.
Job A is rejected as stale or marked SUPERSEDED.
```

---

## 22. State Persistence

The first MVP may keep active job state in memory.

However, the state model should remain compatible with future persistence.

At minimum, diagnostics should be able to reconstruct:

```text
job identity
input revision
state history
current state
terminal reason
processing duration
warning summary
```

Raw text persistence is not required for state reconstruction.

Persisted state should avoid storing source content unless a separate privacy and retention policy permits it.

---

## 23. Timeout Behavior

A job may have one or more time budgets:

```text
QueueTimeout
ProcessingTimeout
PhaseTimeout
CancellationTimeout
```

Timeout behavior should be explicit.

Recommended mapping:

| Timeout                              | Suggested outcome                                                   |
| ------------------------------------ | ------------------------------------------------------------------- |
| Queue timeout caused by backpressure | `FAILED` with `RESOURCE_EXHAUSTED`, or cancellation by orchestrator |
| Overall user or pipeline deadline    | `CANCELLED` with `TIME_BUDGET_EXCEEDED`                             |
| Deterministic phase timeout          | `FAILED` with phase-specific error                                  |
| Job becomes obsolete before deadline | `SUPERSEDED`                                                        |
| Cancellation cannot finish safely    | `FAILED` with cancellation-handling error                           |

A timeout must not automatically be retried without considering whether the content is still current.

---

## 24. State Invariants

The following invariants are mandatory.

### 24.1 Terminal-state invariant

A job has exactly one terminal state:

```text
COMPLETED
or CANCELLED
or SUPERSEDED
or FAILED
```

### 24.2 Immutable-input invariant

A job processes one immutable logical input revision.

### 24.3 Revision invariant

A result must contain the same `ContentRevision` as the input snapshot from which it was produced.

### 24.4 Source-mapping invariant

Every output segment must retain at least one valid source mapping unless it is explicitly marked as generated structural metadata.

### 24.5 Ordering invariant

Final output segment order must be deterministic for the same input and processing profile.

### 24.6 Publication invariant

No successful output may be published after the job is committed as `CANCELLED`, `SUPERSEDED`, or `FAILED`.

### 24.7 Stale-result invariant

A result from an older revision must never replace a newer authoritative result.

### 24.8 Translation-boundary invariant

The module must not perform or own translation.

### 24.9 Presentation-boundary invariant

The module must not decide final font, layout, overlay position, or reader rendering.

### 24.10 Retry invariant

A retry creates a new attempt identity.

### 24.11 Event invariant

Only one terminal state event may be emitted for a job.

### 24.12 Privacy invariant

Raw source text must not be included in normal state-transition events.

---

## 25. Derived Status for User Interfaces

The application may derive simplified statuses for presentation.

Example mapping:

| Internal state      | UI status                 |
| ------------------- | ------------------------- |
| `CREATED`, `QUEUED` | Waiting                   |
| `VALIDATING`        | Preparing                 |
| `NORMALIZING`       | Cleaning text             |
| `STRUCTURING`       | Organizing text           |
| `BUILDING_CONTEXT`  | Preparing context         |
| `FINALIZING`        | Finishing                 |
| `COMPLETED`         | Ready                     |
| `CANCEL_REQUESTED`  | Stopping                  |
| `CANCELLED`         | Cancelled                 |
| `SUPERSEDED`        | Replaced by newer content |
| `FAILED`            | Processing failed         |

The UI mapping must not become the canonical domain state model.

---

## 26. Image-Flow Example

```text
OCR produces regions for Frame 105
    ↓
Create TextProcessingJob TP-105
    state = CREATED
    ↓
Job enters worker queue
    state = QUEUED
    ↓
Region identifiers and reading order are checked
    state = VALIDATING
    ↓
OCR text and punctuation are cleaned
    state = NORMALIZING
    ↓
Lines are merged and dialogue segments are created
    state = STRUCTURING
    ↓
Neighboring speech regions are attached as context
    state = BUILDING_CONTEXT
    ↓
Mappings and fingerprints are verified
    state = FINALIZING
    ↓
Prepared segments are committed
    state = COMPLETED
    ↓
Translation module receives prepared segments
```

---

## 27. Superseded Image-Flow Example

```text
Frame 105 becomes stable
    ↓
TextProcessingJob TP-105 starts
    state = NORMALIZING

User scrolls

Frame 106 becomes stable
    ↓
TextProcessingJob TP-106 is created
    ↓
TP-105 becomes obsolete
    state = SUPERSEDED
    ↓
TP-105 output is suppressed
    ↓
TP-106 continues as the authoritative job
```

The outcome must remain correct even if TP-105 finishes computation after TP-106 starts.

---

## 28. Structured-Text Example

```text
Browser connector extracts chapter text
    ↓
TextProcessingJob is created
    ↓
HTML-derived paragraphs are validated
    ↓
Unicode and whitespace are normalized
    ↓
Paragraphs and dialogue lines are reconstructed
    ↓
The chapter is divided into bounded translation units
    ↓
Nearby paragraph context is attached
    ↓
Prepared segments are finalized
    ↓
The result is sent to translation
```

For long chapters, future incremental processing may publish explicit batches, but the initial contract should prefer a single atomic result where practical.

---

## 29. User-Correction Example

```text
Original OCR text revision: 7
Processing job: TP-7
Result: COMPLETED

User corrects one source region

Corrected source text revision: 8
    ↓
Create processing job TP-8
    ↓
Any unfinished revision-7 job is SUPERSEDED
    ↓
Revision 8 is reprocessed
    ↓
Only revision-8 prepared segments may become current
```

The module may reuse unaffected computation through caching, but the final result must still belong to revision 8.

---

## 30. Error Example

```text
Job enters STRUCTURING
    ↓
Two output segments receive the same SegmentId
    ↓
Final invariant cannot be guaranteed
    ↓
Job enters FAILED

errorCode: DUPLICATE_SEGMENT_ID
failureCategory: INVARIANT_VIOLATION
retryable: false
failedPhase: STRUCTURING
```

The module must not publish a partially ambiguous result as successful.

---

## 31. Observability Requirements

Every state transition should be traceable through structured diagnostics.

Recommended fields:

```text
TextProcessingJobId
ReadingSessionId
ContentId
ContentRevision
PreviousState
NewState
PhaseDetail
TransitionReason
DurationInPreviousState
ProcessedUnitCount
TotalUnitCount
WarningCount
CorrelationId
Timestamp
```

Recommended metrics:

```text
text_processing_jobs_total
text_processing_jobs_active
text_processing_jobs_queued
text_processing_jobs_completed_total
text_processing_jobs_cancelled_total
text_processing_jobs_superseded_total
text_processing_jobs_failed_total
text_processing_duration_ms
text_processing_queue_duration_ms
text_processing_phase_duration_ms
text_processing_input_segments
text_processing_output_segments
text_processing_warning_total
```

Metric labels must remain bounded.

Do not use identifiers, raw text, chapter names, URLs, or arbitrary error messages as metric labels.

---

## 32. MVP Requirements

The first MVP requires support for:

```text
CREATED
QUEUED
VALIDATING
NORMALIZING
STRUCTURING
BUILDING_CONTEXT
FINALIZING
COMPLETED
CANCEL_REQUESTED
CANCELLED
SUPERSEDED
FAILED
```

The MVP must guarantee:

* immutable input revisions;
* stale-result rejection;
* cooperative cancellation;
* deterministic segment ordering;
* source-to-output mapping;
* one terminal state per job;
* explicit warnings;
* bounded context construction;
* diagnostic state transitions;
* separation from translation and presentation.

The MVP does not require:

* durable job recovery;
* distributed processing;
* cross-device job state;
* streaming partial segments;
* automatic long-chapter batching;
* persistent state-event storage;
* user-visible detailed phase progress;
* adaptive retry based on historical behavior.

---

## 33. Open Decisions

The following decisions require implementation or prototype evidence.

### Processing granularity

* Should one job represent a full frame, one page, one paragraph group, or one chapter?
* At what size should structured text be split into several jobs?
* Should comic regions be processed individually or as a page-level batch?

### Context construction

* How many neighboring segments should be included?
* Should context boundaries differ for comics and novels?
* When should previous-page or previous-chapter context be referenced?
* How should context be truncated deterministically?

### OCR cleanup

* Which OCR artifacts can be corrected safely?
* Which cleanup rules require language profiles?
* When should uncertain text remain unchanged?
* How should user corrections override automatic cleanup?

### Incremental output

* Is atomic page-level completion fast enough for normal reading?
* Do long chapters require progressive processing?
* Should downstream translation begin before the whole job is finalized?

### Persistence

* Is in-memory job state sufficient for the desktop MVP?
* Which terminal diagnostics should be retained?
* How long should failed and superseded job metadata remain available?

### Scheduling

* Should newest visible content always receive highest priority?
* Should manual reprocessing outrank automatic screen updates?
* How many text-processing jobs may run concurrently?
* Should queued jobs be coalesced by content revision?

---

## 34. Related Documents

This document should remain consistent with:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES_RULE.md
.meta/MODULES.md

docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

modules/text-processing/README.md
modules/text-processing/CONTRACTS.md
modules/text-processing/EVENTS.md
modules/text-processing/ERRORS.md
```

The global state-machine document defines cross-module lifecycle principles.

This file specializes those principles for the `text-processing` module.

---

## 35. Summary

The `text-processing` state model is centered on one immutable `TextProcessingJob`.

Its successful lifecycle is:

```text
CREATED
    ↓
QUEUED
    ↓
VALIDATING
    ↓
NORMALIZING
    ↓
STRUCTURING
    ↓
BUILDING_CONTEXT
    ↓
FINALIZING
    ↓
COMPLETED
```

Any active job may instead terminate as:

```text
CANCELLED
SUPERSEDED
FAILED
```

The most important correctness rules are:

1. one job processes one immutable content revision;
2. old results must never replace newer content;
3. cancellation and supersession must suppress result publication;
4. every output segment must remain traceable to its source;
5. translation and presentation remain outside this module;
6. every job receives exactly one terminal outcome.
