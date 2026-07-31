# Runtime Scheduler

> Project: CRAI  
> Version: 0.1  
> Status: Architecture Draft

---

## 1. Purpose

This document defines how CRAI selects, prioritizes, starts, delays, retries, and rejects runtime work.

The Scheduler is the decision-making component between:

```text
Work Queue
    ↓
Scheduler
    ↓
Worker
```

The Work Queue stores pending work.

The Scheduler decides which work is allowed to execute next.

This document focuses on scheduling policy rather than implementation technology.

---

## 2. Scope

The Scheduler is responsible for:

- selecting the next work item
- assigning work to available workers
- prioritizing current user-visible content
- rejecting obsolete work
- limiting concurrency
- coordinating retry decisions
- applying fairness between sessions
- reacting to resource pressure
- protecting UI responsiveness

The Scheduler is not responsible for:

- storing pending work
- implementing OCR
- implementing translation
- rendering UI components
- storing persistent data
- directly freeing every runtime resource
- defining provider-specific retry rules

Those responsibilities belong to their respective runtime or domain modules.

---

## 3. Design Goals

The Scheduler must satisfy the following goals:

- prioritize the latest valid revision
- minimize user-visible latency
- avoid processing obsolete work
- prevent one stage from exhausting all resources
- prevent background work from blocking interactive work
- keep worker usage bounded
- support cooperative cancellation
- remain observable
- behave predictably under load
- degrade gracefully when providers are slow

---

## 4. Scheduling Philosophy

CRAI is an interactive reading application.

It is not a batch-processing system.

The Scheduler therefore optimizes for:

```text
Current User Experience
```

rather than:

```text
Completion of Every Submitted Task
```

Example:

```text
Revision 20 queued
Revision 21 queued
Revision 22 queued
```

If Revision 22 is the newest visible page, the Scheduler may:

```text
Execute Revision 22
Drop Revision 21
Drop Revision 20
```

Completing obsolete revisions would consume resources without improving the current reading experience.

---

## 5. Scheduler Inputs

The Scheduler receives decisions inputs from multiple sources.

### 5.1 Work Queue

Provides pending work items.

### 5.2 Session Manager

Provides:

- active session state
- current revision
- session priority
- user interaction state
- session termination state

### 5.3 Revision Manager

Provides:

- current valid revision
- revision age
- revision ownership
- revision obsolescence state

### 5.4 Worker Pool

Provides:

- available workers
- busy workers
- worker capability
- worker health
- provider availability

### 5.5 Resource Monitor

Provides:

- CPU pressure
- memory pressure
- GPU pressure
- network availability
- queue saturation

### 5.6 Cancellation Manager

Provides cancellation state for:

- session
- revision
- pipeline
- work item
- provider request

### 5.7 Cache Manager

Provides whether the requested result already exists.

---

## 6. Scheduling Unit

The basic scheduling unit is a `WorkItem`.

A WorkItem contains at least:

```text
WorkItem
├── WorkItemId
├── SessionId
├── RevisionId
├── Stage
├── PriorityClass
├── CreatedAt
├── Attempt
├── Deadline
├── CostHint
├── ProviderRequirement
└── CancellationHandle
```

The Scheduler must not rely on mutable payload data stored inside the queue.

Large runtime data is accessed through revision-scoped storage.

---

## 7. Scheduling Decision

For every candidate WorkItem, the Scheduler may produce one of the following decisions:

| Decision | Meaning |
|---|---|
| `RUN` | Start execution immediately |
| `DEFER` | Keep pending until a later scheduling cycle |
| `DROP` | Remove because the work is no longer useful |
| `CANCEL` | Request cancellation of already-started work |
| `RETRY` | Create a new attempt according to retry policy |
| `FAIL` | Mark the work as permanently failed |
| `CACHE_HIT` | Skip execution and continue with cached output |

A scheduling decision must be explicit and observable.

---

## 8. Priority Classes

CRAI uses a small number of priority classes.

### 8.1 Interactive Critical

Work directly required to display the newest visible content.

Examples:

- OCR for the current revision
- translation for the current revision
- presentation build for the current revision

### 8.2 Interactive Supporting

Work that improves the current reading experience but is not immediately blocking presentation.

Examples:

- glossary enrichment
- low-cost layout refinement
- alternative translation preparation

### 8.3 Background

Work that is useful but not required for immediate reading.

Examples:

- cache warming
- history indexing
- low-priority diagnostics
- model preloading

### 8.4 Maintenance

Internal cleanup or telemetry work.

Examples:

- metrics aggregation
- expired cache cleanup
- trace export

Priority order:

```text
Interactive Critical
    >
Interactive Supporting
    >
Background
    >
Maintenance
```

---

## 9. Priority Score

Priority class alone is not enough.

The Scheduler may compute a dynamic score.

Conceptually:

```text
Priority Score =
    Priority Class
    + Revision Freshness
    + User Visibility
    + Stage Urgency
    + Deadline Pressure
    - Obsolescence Penalty
    - Resource Cost Penalty
```

The exact numeric formula is implementation-specific.

The architectural requirement is that the score remains:

- deterministic
- explainable
- bounded
- observable

---

## 10. Revision Freshness

Revision freshness is one of the strongest scheduling signals.

Example:

```text
Current Revision: 42
```

Pending work:

```text
Revision 39
Revision 40
Revision 41
Revision 42
```

Default behavior:

- Revision 42 receives the highest priority.
- Revisions 39–41 are evaluated for immediate drop.
- Older work may survive only when explicitly reusable.

A revision is normally obsolete when:

```text
WorkItem.RevisionId != Session.CurrentRevisionId
```

Exceptions must be explicitly documented.

---

## 11. Latest Valid Revision Wins

The main scheduling invariant is:

> Only the newest valid revision may advance toward user-visible presentation.

This does not mean all older work must always be stopped instantly.

An older task may continue when:

- it is nearly complete
- stopping it is more expensive than finishing it
- its result can be reused safely
- it does not block current work
- it does not consume a scarce worker needed by the current revision

However, an older result must never update the active UI.

---

## 12. Stage Urgency

Different stages have different urgency.

Suggested default order:

```text
Presentation Validation
    >
Translation
    >
OCR
    >
Layout
    >
Observation
    >
Background Processing
```

This order is not a fixed universal rule.

For example:

- OCR may become more urgent if no text exists yet.
- Presentation may be deferred if its revision is already stale.
- Translation may be skipped on cache hit.

Stage urgency must always be evaluated together with revision validity.

---

## 13. Worker Capability Matching

Not every worker can execute every WorkItem.

Workers may differ by:

- stage capability
- local or remote provider
- CPU or GPU requirement
- supported language
- supported model
- concurrency limit
- current health

Example:

```text
OCR WorkItem
    ↓
Requires OCR-capable worker
    ↓
Prefers GPU worker
    ↓
Falls back to CPU worker
```

The Scheduler must select only compatible workers.

---

## 14. Concurrency Limits

Every expensive stage must have an explicit concurrency limit.

Suggested MVP defaults:

| Stage | Suggested concurrency |
|---|---:|
| Capture | 1 |
| Observation | 1 |
| OCR | 1 |
| Layout | 1–2 |
| Translation | 1–2 |
| Presentation Build | 1 |
| UI Commit | 1 |

These are starting assumptions, not permanent values.

Concurrency must be configurable because it depends on:

- device capability
- local model cost
- remote provider limits
- memory pressure
- user preference

---

## 15. Resource Classes

Workers and tasks may use different resource classes.

Suggested classes:

```text
UI
CPU_LIGHT
CPU_HEAVY
GPU
NETWORK
IO
```

The Scheduler must prevent one class from starving another.

Example:

A long-running local translation model must not block:

- screen observation
- cancellation handling
- UI presentation
- session stop commands

---

## 16. Admission Control

Before a WorkItem enters execution, the Scheduler performs admission checks.

Checks include:

- session still active
- revision still valid
- cancellation not requested
- required input exists
- compatible worker exists
- concurrency budget available
- memory budget available
- provider is healthy
- deadline has not expired

If any required check fails, the Scheduler must choose:

- defer
- drop
- retry
- fail

It must not start work blindly.

---

## 17. Obsolete Work Elimination

Obsolete work should be removed as early as possible.

Elimination may occur:

### Before Enqueue

Do not enqueue work for an invalid revision.

### While Queued

Remove pending work after a newer revision becomes current.

### Before Execution

Validate again immediately before assigning a worker.

### During Execution

Request cooperative cancellation.

### After Execution

Reject the result during commit validation.

This layered validation prevents stale work from leaking through race conditions.

---

## 18. Scheduling Cycle

A scheduling cycle may follow this sequence:

```text
Receive runtime signal
    ↓
Collect candidate WorkItems
    ↓
Remove invalid candidates
    ↓
Check cache
    ↓
Compute priority
    ↓
Match worker capability
    ↓
Check resource budgets
    ↓
Select next WorkItem
    ↓
Start execution
    ↓
Emit scheduling decision
```

Scheduling signals may include:

- new work enqueued
- worker became available
- revision changed
- task completed
- task failed
- cancellation requested
- resource pressure changed
- provider health changed

The Scheduler should be event-driven rather than continuously polling at high frequency.

---

## 19. Preemption

Preemption means interrupting already-running work to execute more valuable work.

CRAI should use cooperative preemption.

Example:

```text
Revision 40 OCR running
    ↓
Revision 41 becomes current
    ↓
Scheduler requests cancellation of Revision 40
    ↓
Worker reaches a cancellation checkpoint
    ↓
Worker stops safely
    ↓
Revision 41 starts
```

Hard thread termination must not be used as the default cancellation method.

Hard termination risks:

- corrupted provider state
- leaked memory
- locked resources
- incomplete output
- inconsistent metrics

---

## 20. Preemption Policy

A running task should be considered for preemption when:

- a newer critical revision is waiting
- the worker is scarce
- the current task is obsolete
- cancellation is supported
- estimated remaining time is significant

A task may be allowed to finish when:

- it is close to completion
- the worker is not needed elsewhere
- the result is cacheable
- cancellation has high cleanup cost
- provider cancellation is unsupported

The Scheduler should use a simple MVP policy before introducing complex prediction.

---

## 21. MVP Preemption Rule

For the first implementation:

```text
If running work is obsolete
AND a newer Interactive Critical item needs the same scarce worker
THEN request cancellation.
```

Otherwise:

```text
Allow current work to finish,
but reject stale results.
```

This rule is practical and avoids requiring precise remaining-time estimation.

---

## 22. Fairness Between Sessions

CRAI may eventually support multiple sessions.

Examples:

- multiple capture windows
- background imported image
- active screen reader
- browser-based structured text session

The Scheduler must not allow one session to consume all workers indefinitely.

Default fairness model:

```text
Active Foreground Session
    >
Visible Secondary Session
    >
Background Session
```

Within the same priority class, scheduling may use round-robin or weighted fairness.

For the MVP, only one active interactive session is required.

---

## 23. User Interaction Priority

Direct user actions always receive elevated scheduling priority.

Examples:

- stop translation
- change capture region
- retranslate selected segment
- correct OCR text
- switch provider
- open or close session

Control commands should not wait behind long OCR or translation work.

They must use a dedicated control path or reserved execution capacity.

---

## 24. Retry Scheduling

Retry decisions must consider:

- error classification
- attempt count
- revision validity
- provider health
- deadline
- current user value

Retryable errors include:

- temporary network failure
- provider rate limiting
- transient timeout
- temporary worker failure

Non-retryable errors include:

- invalid input
- unsupported format
- canceled revision
- invalid credentials
- deterministic parsing failure without fallback

---

## 25. Retry Limits

Retries must be bounded.

Suggested MVP defaults:

| Stage | Maximum attempts |
|---|---:|
| OCR | 2 |
| Layout | 1 |
| Translation | 2 |
| Presentation Build | 1 |

A retry must not occur when the revision is already obsolete.

---

## 26. Retry Delay

Retries should use bounded delay.

Suggested behavior:

```text
Attempt 1
    ↓ failure
Short delay
    ↓
Attempt 2
```

For interactive work, long exponential backoff may make the result useless.

Therefore:

- interactive retries should remain short
- provider rate limits may trigger fallback
- background work may use longer backoff
- retries must respect the revision deadline

Exact timing belongs in implementation configuration.

---

## 27. Provider Selection

The Scheduler may coordinate provider selection but must not implement provider logic.

Possible outcomes:

```text
Preferred Provider
    ↓ unavailable
Fallback Provider
    ↓ unavailable
Local Provider
    ↓ unavailable
Fail or Defer
```

Provider selection considers:

- capability
- latency
- cost
- privacy mode
- health
- concurrency limit
- language support

Detailed provider policy belongs to provider-management documentation.

---

## 28. Deadline Handling

Interactive work may carry a deadline.

Example:

```text
Translation result is useful only if returned
before the user has moved to a newer page.
```

A missed deadline may result in:

- drop
- cancellation
- fallback to faster provider
- partial presentation
- user-visible failure

The MVP should prefer dropping obsolete work rather than presenting late results.

---

## 29. Partial Results

Partial-result scheduling requires explicit support.

Possible examples:

- some translation units completed
- low-resolution OCR completed before refinement
- cached segments available while others are pending

Default MVP rule:

> Do not commit partial presentation models.

Partial intermediate results may be retained internally, but the UI receives an atomic valid presentation model.

Later versions may support progressive rendering through a separate documented flow.

---

## 30. Backpressure Coordination

When downstream capacity is exhausted, the Scheduler applies backpressure.

Example:

```text
Translation workers saturated
    ↓
Translation queue reaches limit
    ↓
New obsolete translation work is dropped
    ↓
Layout scheduling is reduced
    ↓
OCR scheduling is reduced
    ↓
Observation continues
```

Capture and observation should remain responsive even when AI stages are overloaded.

The Scheduler must avoid completely blocking source observation.

---

## 31. Memory Pressure Behavior

When memory pressure increases, the Scheduler should reduce work before the process becomes unstable.

Possible actions:

1. stop background work
2. cancel obsolete work
3. reduce concurrency
4. remove expired queued items
5. disable speculative work
6. prefer remote provider over local model when appropriate
7. reject new non-critical work

The active user-visible revision receives the highest protection.

---

## 32. Provider Pressure Behavior

When a remote provider is slow or rate-limited:

```text
Provider Health Degraded
    ↓
Reduce concurrency
    ↓
Stop speculative requests
    ↓
Use cache
    ↓
Use fallback provider
    ↓
Surface controlled failure
```

The Scheduler must not flood a degraded provider with retries.

---

## 33. Scheduler State

The Scheduler may expose a small lifecycle:

```text
STOPPED
    ↓
STARTING
    ↓
RUNNING
    ↓
PAUSED
    ↓
STOPPING
    ↓
STOPPED
```

`PAUSED` means new work is not started, but control operations and cleanup remain active.

Scheduler lifecycle is separate from reading-session lifecycle.

---

## 34. Scheduling Events

The Scheduler should emit events such as:

```text
scheduler.started
scheduler.paused
scheduler.resumed
scheduler.stopped

work.admitted
work.deferred
work.started
work.dropped
work.cancel_requested
work.retry_scheduled
work.failed
work.completed

worker.assigned
worker.released

resource.pressure_detected
provider.degraded
```

Final event names must remain consistent with `EVENT_BUS.md`.

---

## 35. Decision Reason Codes

Every non-trivial scheduling decision should include a reason code.

Examples:

```text
NEWER_REVISION_AVAILABLE
SESSION_CLOSED
REVISION_EXPIRED
CACHE_HIT
NO_COMPATIBLE_WORKER
CONCURRENCY_LIMIT
MEMORY_PRESSURE
PROVIDER_UNAVAILABLE
RETRY_LIMIT_REACHED
USER_CANCELED
DEADLINE_EXCEEDED
```

Reason codes improve:

- diagnostics
- testing
- metrics
- debugging
- future scheduler tuning

---

## 36. Scheduler Metrics

The Scheduler should expose at least:

- scheduling decisions per stage
- pending work count
- running work count
- dropped work count
- canceled work count
- retry count
- worker utilization
- scheduling delay
- queue wait time
- cache-hit bypass count
- stale-result rejection count
- provider saturation count
- resource-pressure events

Metrics must not include raw private content unless explicitly allowed.

---

## 37. Determinism and Testability

Given the same:

- queue state
- session state
- revision state
- worker availability
- resource state
- configuration

the Scheduler should produce the same decision.

Random scheduling must not be used unless:

- the randomness is intentional
- the seed is controllable
- the behavior is testable

Scheduling policy should be testable without running real OCR or translation providers.

---

## 38. Failure Isolation

A Scheduler failure must not corrupt domain data.

If the Scheduler encounters an internal error:

```text
Stop admitting new work
    ↓
Preserve control path
    ↓
Cancel or safely reject active work
    ↓
Emit fatal runtime event
    ↓
Allow session recovery or restart
```

The UI must remain capable of showing an error and stopping the session.

---

## 39. MVP Scheduling Policy

The initial implementation should use a simple and practical policy.

### 39.1 Assumptions

- one foreground reading session
- one current revision
- one OCR worker
- one translation worker
- bounded queues
- cooperative cancellation where supported

### 39.2 Selection Rules

1. Reject work from inactive sessions.
2. Reject work from obsolete revisions.
3. Resolve cache hits before worker assignment.
4. Prioritize control commands.
5. Prioritize the current revision.
6. Prioritize user-visible stages.
7. Respect per-stage concurrency limits.
8. Start the highest-priority compatible work.
9. Cancel obsolete work occupying a scarce worker.
10. Reject stale outputs before commit.

### 39.3 Queue Behavior

For each interactive stage:

```text
Keep newest valid pending item.
Drop older pending items for the same session and stage.
```

This provides a practical latest-value queue without requiring a complex general-purpose scheduler.

---

## 40. Example: Rapid Scrolling

```text
Revision 30
    ↓
OCR starts

User scrolls

Revision 31 created
    ↓
Scheduler marks Revision 30 obsolete
    ↓
Cancellation requested

User scrolls again

Revision 32 created
    ↓
Revision 31 pending OCR is dropped
    ↓
Revision 30 result is rejected if it finishes
    ↓
OCR worker starts Revision 32
```

Outcome:

- no obsolete revision updates the UI
- the newest page receives processing priority
- queue growth remains bounded

---

## 41. Example: Translation Provider Delay

```text
Revision 45 translation starts
    ↓
Provider becomes slow
    ↓
Revision 46 becomes current
    ↓
Scheduler requests cancellation of Revision 45
```

If provider cancellation is supported:

```text
Request canceled
    ↓
Translation worker starts Revision 46
```

If provider cancellation is unsupported:

```text
Revision 45 continues externally
    ↓
Local worker slot is logically released when safe
    ↓
Late Revision 45 result is rejected
```

Provider-specific resource handling must be defined in `CANCELLATION.md`.

---

## 42. Example: Cache Hit

```text
Revision 50 reaches Translation stage
    ↓
Translation cache lookup succeeds
    ↓
Scheduler returns CACHE_HIT
    ↓
No translation worker is assigned
    ↓
Presentation work is enqueued
```

Cache validation must include all required context keys.

---

## 43. Example: Memory Pressure

```text
Memory pressure detected
    ↓
Maintenance work paused
    ↓
Background work canceled
    ↓
Obsolete revisions disposed
    ↓
Translation concurrency reduced
    ↓
Current revision remains active
```

If pressure remains critical:

```text
Current pipeline fails safely
    ↓
User receives recoverable error
```

---

## 44. Architecture Invariants

The Scheduler must always preserve the following invariants:

1. An inactive session cannot start new domain work.
2. An obsolete revision cannot commit user-visible output.
3. Queue capacity and concurrency are bounded.
4. Control commands cannot be starved by domain work.
5. A worker only receives compatible work.
6. Retry count cannot exceed the configured limit.
7. Canceled work cannot re-enter execution without a new WorkItem.
8. Scheduling decisions are observable.
9. UI work is serialized through the UI execution boundary.
10. Current interactive work has priority over background work.

---

## 45. Open Questions

The following questions remain open for later implementation phases:

- Should OCR and translation use separate schedulers or one shared scheduler?
- Should local and remote providers have independent concurrency pools?
- Should work cost be estimated dynamically?
- Should nearly completed obsolete work be allowed to populate cache?
- How should multiple foreground sessions be prioritized?
- Should progressive translation be supported?
- Should the Scheduler adapt concurrency automatically?
- How should GPU memory pressure be measured?
- Can provider cancellation reliably release billing and request capacity?

These questions do not block the MVP scheduling model.

---

## 46. Related Documents

- `README.md`
- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`
- `CANCELLATION.md`
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

## 47. Next Step

The next runtime document should be:

```text
CANCELLATION.md
```

It must define:

- cancellation scopes
- cancellation-token hierarchy
- cooperative cancellation checkpoints
- queued-work removal
- provider-request cancellation
- stale-result rejection
- cleanup ownership
- cancellation events
- cancellation timeouts

---

## 48. Summary

The Scheduler is the runtime decision engine of CRAI.

It selects the most valuable valid work, protects scarce resources, removes obsolete processing, and keeps the reading experience responsive.

The MVP Scheduler should remain deliberately simple:

```text
Current Revision First
    +
Bounded Concurrency
    +
Drop Obsolete Pending Work
    +
Cancel Obsolete Running Work When Useful
    +
Reject Every Stale Result
```

More advanced adaptive scheduling should only be introduced after profiling demonstrates a real need.