# Runtime Cancellation

> Project: CRAI  
> Version: 0.1  
> Status: Architecture Draft

---

## 1. Purpose

This document defines how CRAI stops queued, running, delayed, retried, and externally delegated work.

Cancellation is a core runtime capability.

It exists to ensure that obsolete or interrupted work does not:

- waste CPU, GPU, memory, or network resources
- delay newer user-visible work
- update the UI with stale data
- remain alive after its session or revision ends
- leak provider requests or runtime resources

This document defines cancellation behavior, ownership, scope, propagation, cleanup, and result validation.

---

## 2. Scope

This document covers:

- cancellation scopes
- cancellation propagation
- cancellation token hierarchy
- queued-work cancellation
- running-work cancellation
- provider-request cancellation
- cancellation checkpoints
- cancellation deadlines
- cleanup ownership
- stale-result rejection
- cancellation events
- cancellation reason codes
- cancellation metrics
- MVP cancellation policy

This document does not define:

- scheduler priority
- queue ordering
- cache invalidation policy
- memory-retention policy
- provider-specific SDK behavior
- operating-system thread termination

Those concerns belong to related runtime or provider documents.

---

## 3. Cancellation Goals

The cancellation system must satisfy the following goals:

- stop obsolete work as early as possible
- avoid unsafe hard termination
- propagate cancellation predictably
- keep cancellation bounded in time
- protect the active revision
- prevent stale commits
- release owned resources
- remain observable
- remain testable
- work even when external providers cannot be interrupted

---

## 4. Cancellation Philosophy

CRAI uses cooperative cancellation.

A cancellation request means:

> The runtime no longer wants this work to continue.

It does not always mean:

> The work has already stopped.

Cancellation therefore has two distinct concepts:

```text
Cancellation Requested
```

and:

```text
Cancellation Completed
```

A worker may need time to:

- reach a safe checkpoint
- stop a provider request
- release temporary buffers
- return execution capacity
- emit a final cancellation result

---

## 5. Three-Layer Protection Model

CRAI uses three layers of cancellation protection.

### Layer 1 — Prevent Execution

If work has not started yet:

```text
Remove from queue
or
Mark invalid before dequeue
```

This is the cheapest form of cancellation.

### Layer 2 — Stop Execution Cooperatively

If work is already running:

```text
Request cancellation
    ↓
Worker reaches cancellation checkpoint
    ↓
Worker exits safely
```

### Layer 3 — Reject Stale Result

If work cannot be stopped:

```text
Work finishes
    ↓
Result reaches commit boundary
    ↓
Revision validity check fails
    ↓
Result discarded
```

The third layer is mandatory even when the first two layers exist.

---

## 6. Cancellation Terminology

| Term | Meaning |
|---|---|
| `Cancellation Request` | A signal that work should stop |
| `Cancellation Token` | Read-only cancellation state available to workers |
| `Cancellation Source` | Owner capable of requesting cancellation |
| `Cancellation Scope` | Runtime boundary affected by cancellation |
| `Cancellation Checkpoint` | Safe location where a worker checks cancellation |
| `Cancellation Completion` | Confirmation that work has stopped and cleanup completed |
| `Stale Result` | Result produced for an invalid or obsolete revision |
| `Hard Termination` | Forceful thread or process stop without cooperative cleanup |

---

## 7. Cancellation Scopes

Cancellation may target different runtime scopes.

### 7.1 Application Scope

Stops all runtime activity.

Examples:

- application shutdown
- fatal runtime failure
- forced restart

### 7.2 Scheduler Scope

Stops admission of new work and requests cancellation of active runtime work.

Examples:

- runtime pause
- scheduler restart
- critical resource pressure

### 7.3 Session Scope

Stops all work belonging to one reading session.

Examples:

- user closes the session
- user stops translation
- session becomes invalid

### 7.4 Revision Scope

Stops all work belonging to one source revision.

Examples:

- user scrolls
- a newer stable frame becomes current
- capture region content changes

### 7.5 Pipeline Scope

Stops the remaining stages of one revision pipeline.

Examples:

- OCR produces no usable text
- fatal layout failure
- user cancels only one processing attempt

### 7.6 Stage Scope

Stops one specific stage.

Examples:

- translation provider switched
- OCR retry abandoned
- optional refinement canceled

### 7.7 Work Item Scope

Stops one exact queue item or execution attempt.

### 7.8 Provider Request Scope

Stops one external or local provider operation where supported.

---

## 8. Cancellation Hierarchy

Cancellation scopes form a parent-child hierarchy.

```text
Application
    ↓
Scheduler
    ↓
Session
    ↓
Revision
    ↓
Pipeline
    ↓
Stage
    ↓
Work Item
    ↓
Provider Request
```

When a parent scope is canceled, all child scopes are considered canceled.

Examples:

```text
Session canceled
    ↓
All revisions canceled
    ↓
All pipelines canceled
    ↓
All active and queued work canceled
```

Canceling a child scope does not automatically cancel its parent.

Example:

```text
Translation Work Item canceled
```

does not necessarily cancel:

```text
Entire Session
```

---

## 9. Cancellation Token Model

Every cancelable operation receives a token derived from its owning scope.

Conceptually:

```text
ApplicationToken
    ↓
SessionToken
    ↓
RevisionToken
    ↓
PipelineToken
    ↓
WorkItemToken
```

A child token is canceled when:

- its own source is canceled
- any parent source is canceled

A token should expose at least:

```text
IsCancellationRequested
Reason
RequestedAt
Scope
```

Implementation-specific APIs may expose additional helpers.

---

## 10. Linked Cancellation Tokens

A WorkItem may depend on more than one cancellation condition.

Example:

```text
WorkItem canceled when:
- session closes
- revision becomes obsolete
- scheduler shuts down
- deadline expires
```

These conditions may be represented as a linked token.

Conceptually:

```text
LinkedToken =
    SessionToken
    OR RevisionToken
    OR SchedulerToken
    OR DeadlineToken
```

The first cancellation source to trigger should define the primary reason.

Additional reasons may be preserved as metadata.

---

## 11. Cancellation Reasons

Every cancellation request must include a reason code.

Suggested reason codes:

```text
APPLICATION_SHUTDOWN
SCHEDULER_STOPPED
SESSION_CLOSED
USER_STOPPED_TRANSLATION
CAPTURE_REGION_CHANGED
NEWER_REVISION_AVAILABLE
REVISION_EXPIRED
PIPELINE_FAILED
STAGE_REPLACED
PROVIDER_SWITCHED
PROVIDER_TIMEOUT
DEADLINE_EXCEEDED
RESOURCE_PRESSURE
WORK_SUPERSEDED
RETRY_ABORTED
MANUAL_CANCEL
DEPENDENCY_CANCELED
```

Reason codes must be stable enough for:

- metrics
- tests
- diagnostics
- UI messages
- future policy tuning

---

## 12. Cancellation States

A cancelable WorkItem may move through these states:

```text
ACTIVE
    ↓
CANCEL_REQUESTED
    ↓
CANCELING
    ↓
CANCELED
```

Alternative terminal outcomes:

```text
COMPLETED
FAILED
DROPPED
EXPIRED
```

A WorkItem must never return from a terminal state to an active state.

---

## 13. Difference Between Terminal Outcomes

### Canceled

Work had been accepted or started, then was explicitly stopped.

### Dropped

Work was discarded before execution because it was no longer valuable.

### Expired

Work exceeded its validity window or deadline.

### Failed

Work could not complete because of an error.

### Completed

Work successfully produced a valid output.

These outcomes should not be collapsed into one generic status.

---

## 14. Queued-Work Cancellation

Queued work should be canceled by removal or invalidation.

Possible implementation strategies:

### Physical Removal

Remove the WorkItem from the queue immediately.

Advantages:

- frees queue capacity
- reduces future scans

Disadvantages:

- may require indexed removal
- may complicate concurrent queue access

### Logical Invalidation

Mark the WorkItem or its scope as canceled.

The queue skips it during dequeue.

Advantages:

- simpler queue implementation
- suitable for lock-free or channel-based queues

Disadvantages:

- stale entries remain until observed
- queue metrics must distinguish active and invalid items

For the MVP, logical invalidation is acceptable if queues remain small and bounded.

---

## 15. Dequeue Validation

Every WorkItem must be validated immediately before execution.

Checks include:

- cancellation requested
- session active
- revision current
- input available
- deadline valid
- attempt still permitted

Example:

```text
Dequeue WorkItem
    ↓
Validate cancellation
    ↓
Invalid
    ↓
Mark Canceled or Dropped
    ↓
Do not assign worker
```

This validation protects against race conditions between enqueue and execution.

---

## 16. Running-Work Cancellation

Running work uses cooperative cancellation.

The runtime performs:

```text
Request cancellation
    ↓
Worker observes token
    ↓
Worker stops at safe checkpoint
    ↓
Worker releases owned resources
    ↓
Worker reports Canceled
```

Workers must not ignore cancellation indefinitely.

---

## 17. Cancellation Checkpoints

A cancellation checkpoint is a safe place where a worker can stop.

Workers should check cancellation:

- before expensive work begins
- before provider calls
- after provider calls
- between processing batches
- before allocating large buffers
- before writing stage output
- before emitting completion
- before scheduling the next stage

Checkpoint frequency should reflect task cost.

---

## 18. OCR Cancellation Checkpoints

Suggested OCR checkpoints:

```text
Before image preprocessing
Before model invocation
Between detected regions
After OCR provider returns
Before storing OCR result
Before enqueueing layout work
```

For local OCR processing, cancellation may also be checked between batches or tiles.

If the OCR library exposes no cancellation API, CRAI must still reject the result after completion.

---

## 19. Layout Cancellation Checkpoints

Suggested layout checkpoints:

```text
Before region grouping
Between page regions
Before reading-order resolution
Before storing layout result
Before building translation units
```

Layout work is usually shorter than OCR or translation, so cancellation frequency may be lower.

---

## 20. Translation Cancellation Checkpoints

Suggested translation checkpoints:

```text
Before cache lookup
Before provider request
Between translation-unit batches
After provider response
Before glossary post-processing
Before storing translation result
Before presentation scheduling
```

A multi-unit translation request should support cancellation between batches when practical.

---

## 21. Presentation Cancellation Checkpoints

Suggested presentation checkpoints:

```text
Before building presentation model
Before UI-thread dispatch
Immediately before UI commit
```

The final validation before UI commit is mandatory.

---

## 22. Provider Request Cancellation

Provider operations fall into three categories.

### 22.1 Fully Cancelable

The provider supports request cancellation.

Examples:

- local task with cancellation token
- HTTP request with abort controller
- process with supported interrupt API

Behavior:

```text
Request cancellation
    ↓
Abort provider request
    ↓
Wait for provider acknowledgment
    ↓
Release provider slot
```

### 22.2 Cooperatively Cancelable

The provider supports cancellation only at certain boundaries.

Behavior:

```text
Set cancellation flag
    ↓
Provider stops at next checkpoint
```

### 22.3 Non-Cancelable

The provider call cannot be safely interrupted.

Behavior:

```text
Mark request abandoned
    ↓
Do not block newer scheduling where avoidable
    ↓
Wait for late completion
    ↓
Discard result
```

Non-cancelable requests must be bounded by timeout and provider concurrency limits.

---

## 23. Abandoned Provider Requests

An abandoned provider request is still executing externally but no longer has user value.

It must:

- lose commit permission
- lose permission to schedule downstream work
- remain tracked until completion or timeout
- not count as active user-visible work
- continue counting against provider capacity if the provider still holds resources

The runtime must not pretend that an external request was freed when it was not.

---

## 24. Provider Billing Considerations

Canceling a local runtime operation does not guarantee that a remote provider stops billing.

Provider documentation may define whether:

- request cancellation is supported
- partial requests are billed
- completed late responses are billed
- timeout releases capacity

CRAI must not assume cancellation eliminates provider cost.

Detailed provider behavior belongs in provider-specific documentation.

---

## 25. Cancellation Timeout

Cancellation must not wait forever.

Each stage may define a cancellation grace period.

Suggested conceptual behavior:

```text
Cancellation requested
    ↓
Wait for cooperative stop
    ↓
Grace period exceeded
    ↓
Mark task abandoned
    ↓
Detach commit rights
    ↓
Continue cleanup asynchronously
```

The exact timeout depends on:

- local or remote execution
- stage cost
- cleanup requirements
- provider capability

---

## 26. Hard Termination Policy

Hard termination should not be used inside the primary application process.

Forbidden by default:

- forcibly killing a thread
- interrupting arbitrary memory operations
- disposing shared state while a worker still uses it

Hard termination may only be considered when work executes in an isolated child process.

Example:

```text
OCR Worker Process
    ↓
Cancellation timeout exceeded
    ↓
Terminate process
    ↓
Restart clean worker process
```

This requires explicit process-isolation design and is not required for the MVP.

---

## 27. Commit Permission

Every running task has conditional permission to commit its result.

Completion alone does not grant commit permission.

Before storing or presenting a result, the runtime validates:

```text
Session active?
Revision current?
Pipeline valid?
Cancellation not requested?
Output belongs to expected attempt?
```

Only then may the result advance.

---

## 28. Stale-Result Rejection

Stale-result rejection is the final correctness boundary.

Conceptually:

```text
Result arrives
    ↓
Compare Result.SessionId
    ↓
Compare Result.RevisionId
    ↓
Compare Result.AttemptId
    ↓
Check cancellation state
    ↓
Valid?
```

If no:

```text
Discard result
Emit stale-result event
Release result resources
```

A stale result must never:

- update the UI
- overwrite newer cache entries incorrectly
- enqueue downstream work
- revive a canceled pipeline

---

## 29. Attempt Validation

Retries may produce multiple attempts for the same stage.

Example:

```text
Revision 60
Translation Attempt 1
Translation Attempt 2
```

If Attempt 2 becomes authoritative, a late result from Attempt 1 must be rejected.

Commit validation should therefore include:

```text
ProcessingAttemptId
```

not only:

```text
RevisionId
```

---

## 30. Cancellation and Cache

Cancellation does not automatically mean the produced result is unusable for cache.

A completed obsolete result may be cacheable only when:

- result is deterministic enough
- cache key fully describes its inputs
- result contains no partial or corrupted data
- provider completion was successful
- retention policy permits it
- storing it does not delay current work

Default MVP rule:

> Do not cache results from canceled or obsolete interactive work.

This simplifies correctness.

The policy may be relaxed later after profiling.

---

## 31. Cleanup Ownership

The component that creates or acquires a resource owns cleanup unless ownership is explicitly transferred.

Examples:

| Resource | Default cleanup owner |
|---|---|
| Image preprocessing buffer | Image-processing worker |
| OCR provider request | OCR provider adapter |
| Translation network request | Translation provider adapter |
| Temporary layout graph | Layout worker |
| WorkItem cancellation source | Pipeline coordinator |
| Revision-scoped data | Revision store |
| UI dispatch handle | Presentation coordinator |

Cancellation must not create ambiguous cleanup ownership.

---

## 32. Cleanup Sequence

A worker handling cancellation should generally:

```text
Stop producing new work
    ↓
Cancel child operations
    ↓
Release temporary local resources
    ↓
Return or detach provider resources
    ↓
Avoid committing output
    ↓
Report cancellation outcome
```

Cleanup must be idempotent where practical.

Repeated cleanup calls must not corrupt state.

---

## 33. Cancellation Propagation

Cancellation should propagate downward through owned operations.

Example:

```text
Revision canceled
    ↓
Pipeline canceled
    ↓
OCR canceled
    ↓
Translation canceled
    ↓
Presentation canceled
```

Cancellation should not propagate upward automatically.

Example:

```text
One translation unit canceled
```

does not automatically cancel:

```text
Entire application
```

Escalation requires an explicit policy decision.

---

## 34. Child Task Registration

A parent operation must register child operations before or immediately after starting them.

This prevents a race where:

```text
Parent canceled
    ↓
Child starts without inheriting cancellation
```

Correct behavior:

```text
Create child token from parent
    ↓
Register child
    ↓
Start child
```

---

## 35. Cancellation Race Conditions

Common races include:

### Completion and Cancellation at the Same Time

The result may complete just as cancellation is requested.

Resolution:

```text
Commit validation decides authority.
```

### New Revision During Queue Dequeue

The WorkItem may become stale after dequeue but before execution.

Resolution:

```text
Validate immediately before worker assignment.
```

### UI Commit After Revision Change

UI dispatch may already be queued.

Resolution:

```text
Validate again on the UI thread before applying.
```

### Provider Response After Timeout

The provider may return after the runtime marked the request abandoned.

Resolution:

```text
Discard response and release associated resources.
```

---

## 36. UI Cancellation Behavior

Cancellation should avoid unnecessary visual disruption.

When an obsolete revision is canceled:

- current visible translation may remain until replacement is ready
- loading indicators should reflect the active revision
- stale errors should not replace valid newer content
- user-initiated stop should stop loading feedback promptly
- UI must not wait for slow provider cleanup before responding

The UI reflects logical cancellation, not necessarily physical provider completion.

---

## 37. User-Initiated Cancellation

User actions may request cancellation.

Examples:

- stop translation
- close reader
- change capture region
- switch reading mode
- restart current translation
- switch provider

User commands should:

1. update session intent immediately
2. revoke commit permission
3. request runtime cancellation
4. update UI promptly
5. perform cleanup asynchronously where safe

---

## 38. Automatic Cancellation

The runtime may cancel work automatically when:

- a newer revision becomes current
- a deadline expires
- memory pressure becomes critical
- provider becomes unhealthy
- retry policy changes
- session becomes inactive
- required input is evicted
- downstream stage becomes impossible

Automatic cancellation must include a reason code.

---

## 39. Cancellation Events

Suggested events:

```text
cancellation.requested
cancellation.propagated
cancellation.acknowledged
cancellation.completed
cancellation.timeout

work.cancel_requested
work.canceled
work.abandoned
work.result_rejected

revision.canceled
pipeline.canceled
provider_request.cancel_requested
provider_request.abandoned
```

Final event names must align with `EVENT_BUS.md`.

---

## 40. Cancellation Event Payload

A cancellation event should include:

```text
EventId
Timestamp
Scope
ScopeId
SessionId
RevisionId
WorkItemId
AttemptId
ReasonCode
RequestedBy
RequestedAt
CompletedAt
Provider
Stage
```

Optional fields should be omitted when not applicable.

No raw comic text or image data should be included in cancellation events.

---

## 41. Cancellation Metrics

The runtime should track:

- cancellation requests by scope
- cancellation completion time
- queued items dropped
- running tasks canceled
- tasks abandoned after timeout
- provider requests successfully aborted
- provider requests that completed late
- stale results rejected
- cleanup failures
- cancellation reason distribution
- average cancellation acknowledgment latency

These metrics help identify providers or stages that do not stop efficiently.

---

## 42. Logging Requirements

Cancellation logs should record:

- scope
- reason
- stage
- revision
- attempt
- cancellation latency
- completion outcome

Logs must avoid:

- source images
- OCR text
- translation content
- provider secrets

---

## 43. Error Handling During Cancellation

Cleanup itself may fail.

Examples:

- provider abort throws an error
- temporary buffer cannot be released immediately
- child task does not acknowledge cancellation
- worker becomes unresponsive

Cancellation cleanup failure must:

- not restore commit permission
- not revive canceled work
- emit diagnostics
- escalate resource cleanup when needed
- preserve active revision correctness

---

## 44. Cancellation and Retry

A canceled attempt must not be retried automatically unless the Scheduler explicitly creates a new WorkItem.

Examples:

```text
Canceled because newer revision exists
    ↓
Never retry
```

```text
Canceled because provider switched
    ↓
Scheduler may create a new attempt using the new provider
```

The new attempt must receive a new:

```text
WorkItemId
ProcessingAttemptId
CancellationToken
```

---

## 45. Cancellation and Session Recovery

A canceled session is terminal unless product behavior explicitly supports resume.

Resume should create:

- a new active session state
- new revision ownership
- new cancellation scopes

Old canceled tokens must never be reused.

---

## 46. MVP Cancellation Policy

The first implementation should remain simple.

### 46.1 Required Scopes

MVP requires:

- application
- session
- revision
- work item
- provider request

Pipeline and stage scopes may be represented internally if needed.

### 46.2 Required Rules

1. Every WorkItem carries cancellation context.
2. Session close cancels all session work.
3. New revision cancels older revision work.
4. Queued obsolete work is skipped or removed.
5. Running obsolete work receives a cancellation request.
6. Workers check cancellation before and after expensive operations.
7. Provider requests use abort support when available.
8. Non-cancelable provider results are discarded.
9. Every result is validated before commit.
10. UI commit validates the active revision again.

### 46.3 MVP Cancellation Sequence

```text
New Revision Created
    ↓
Mark Previous Revision Obsolete
    ↓
Cancel Previous Revision Token
    ↓
Invalidate Queued Work
    ↓
Request Running Work Cancellation
    ↓
Schedule New Revision
    ↓
Reject Late Old Results
    ↓
Dispose Old Revision Resources
```

---

## 47. Example: Scroll During OCR

```text
Revision 70 OCR running
    ↓
User scrolls
    ↓
Revision 71 becomes current
    ↓
Revision 70 token canceled
    ↓
OCR provider abort requested
```

If OCR stops:

```text
OCR reports Canceled
    ↓
Temporary buffers released
```

If OCR cannot stop:

```text
OCR finishes late
    ↓
Commit validation fails
    ↓
Result discarded
```

---

## 48. Example: Scroll During Translation

```text
Revision 80 translation request active
    ↓
Revision 81 created
    ↓
Revision 80 canceled
```

Cancelable provider:

```text
Abort request
    ↓
Release provider slot
    ↓
Start Revision 81
```

Non-cancelable provider:

```text
Mark request abandoned
    ↓
Revision 81 receives next available safe capacity
    ↓
Late Revision 80 response discarded
```

---

## 49. Example: Session Closed

```text
User closes reading session
    ↓
Session token canceled
    ↓
Queued work invalidated
    ↓
Running work receives cancellation
    ↓
UI detaches session presentation
    ↓
Revision data disposed when safe
```

The application itself may remain running.

---

## 50. Example: Provider Switch

```text
Translation using Provider A
    ↓
User switches to Provider B
    ↓
Provider A stage work canceled
    ↓
Old attempt loses commit permission
    ↓
New attempt created for Provider B
```

The session and revision may remain active.

---

## 51. Example: Cancellation Timeout

```text
Provider request cancellation requested
    ↓
Grace period exceeded
    ↓
Request marked abandoned
    ↓
WorkItem marked Canceled
    ↓
Commit permission revoked
    ↓
Provider completion tracked asynchronously
```

The runtime continues without trusting the late result.

---

## 52. Architecture Invariants

The cancellation system must preserve these invariants:

1. Parent cancellation affects all child scopes.
2. Child cancellation does not automatically cancel its parent.
3. Cancellation never grants permission to retry automatically.
4. Canceled work cannot commit user-visible output.
5. Every UI commit validates current revision ownership.
6. Every retry creates a new processing attempt.
7. Cleanup ownership is explicit.
8. Hard thread termination is forbidden in the primary process.
9. Cancellation wait time is bounded.
10. Late external results are treated as untrusted.
11. Old cancellation tokens are never reused.
12. Cancellation events contain no raw private content.

---

## 53. Testing Requirements

Cancellation tests should cover:

- cancel before enqueue
- cancel while queued
- cancel between dequeue and execution
- cancel during OCR
- cancel during translation
- cancel during UI dispatch
- simultaneous completion and cancellation
- session cancellation
- revision replacement
- provider without abort support
- cancellation timeout
- late provider result
- retry after provider switch
- cleanup called more than once
- stale result rejected
- child task inherits parent cancellation

Tests should use deterministic fake workers and providers.

---

## 54. Open Questions

The following questions remain open:

- Which OCR libraries support cooperative cancellation?
- Should long-running local AI work execute in child processes?
- How long should each cancellation grace period be?
- Should obsolete completed results ever populate cache?
- Can remote-provider requests be detached without occupying local worker capacity?
- Should UI retain previous translation until new content is ready?
- Which cancellation reason codes should be user-visible?
- Should memory pressure cancel current work or only background work?
- How should partial translation batches be handled after cancellation?

These questions do not block the MVP policy.

---

## 55. Related Documents

- `README.md`
- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`
- `SCHEDULER.md`
- `CACHE_POLICY.md`
- `MEMORY_MODEL.md`
- `THREADING_MODEL.md`
- `RESOURCE_LIFECYCLE.md`
- `PERFORMANCE_MODEL.md`
- `../STATE_MACHINE.md`
- `../EVENT_BUS.md`
- `../DATA_FLOW.md`
- `../flows/SCREEN_COMIC_FLOW.md`

---

## 56. Next Step

The next runtime document should be:

```text
CACHE_POLICY.md
```

It should define:

- cache layers
- cache ownership
- cache keys
- revision-independent reuse
- provider and model versioning
- glossary and context dependencies
- stale-cache protection
- memory and persistent cache boundaries
- eviction
- privacy
- canceled-result cache rules

---

## 57. Summary

CRAI uses cancellation to protect responsiveness and correctness.

The practical runtime model is:

```text
Prevent Obsolete Work
    ↓
Cancel Running Work Cooperatively
    ↓
Bound Cancellation Waiting
    ↓
Reject Every Stale Result
    ↓
Release Resources Safely
```

Cancellation is not considered complete merely because a signal was sent.

Correctness is guaranteed only when stale work has lost commit permission and can no longer affect the active reading experience.