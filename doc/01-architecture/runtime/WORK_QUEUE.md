# WORK_QUEUE

**Status:** Draft

**Version:** 1.0

---

# 1. Purpose

This document defines how work is buffered, prioritized, scheduled, and delivered between runtime pipeline stages.

The Work Queue is responsible for regulating the execution flow of CRAI.

It is not merely a storage of pending tasks.

Instead, it acts as the control point that prevents resource exhaustion, minimizes latency, and ensures that obsolete work is discarded as early as possible.

---

# 2. Goals

The work queue must satisfy the following goals.

- Keep the pipeline continuously flowing.
- Prevent worker starvation.
- Prevent memory explosion.
- Support cancellation.
- Support prioritization.
- Prefer the newest revision.
- Avoid unnecessary computation.

---

# 3. Queue Philosophy

Unlike traditional FIFO queues, CRAI uses **revision-aware queues**.

The runtime is optimized for interactive reading rather than task completion.

Example:

Revision 18

↓

Revision 19

↓

Revision 20

↓

Revision 21

If Revision 21 arrives before Revision 18 finishes,

the runtime should normally process Revision 21 first.

Completing obsolete work has lower priority than maintaining a responsive UI.

---

# 4. Queue Topology

Each runtime stage owns an independent queue.

```text
Capture

↓

Capture Queue

↓

Observation

↓

Observation Queue

↓

OCR

↓

OCR Queue

↓

Layout

↓

Layout Queue

↓

Translation

↓

Translation Queue

↓

Presentation

↓

Presentation Queue
```

Queues isolate processing stages from one another.

A slow translation engine must never block capture.

---

# 5. Queue Item

Every queue stores immutable Work Items.

A Work Item contains:

Revision ID

Session ID

Pipeline Stage

Priority

Creation Time

Expiration Policy

Payload Reference

Retry Counter

Cancellation Token

No queue stores mutable processing state.

---

# 6. Queue Ownership

Each queue belongs to exactly one pipeline stage.

| Queue | Consumer |
|--------|----------|
| Capture Queue | Capture Worker |
| Observation Queue | Observation Worker |
| OCR Queue | OCR Worker |
| Layout Queue | Layout Worker |
| Translation Queue | Translation Worker |
| Presentation Queue | Presentation Worker |

Only the owning stage may dequeue work.

Other stages communicate through the Event Bus.

---

# 7. Queue Lifecycle

```text
Create Work Item

↓

Enqueue

↓

Waiting

↓

Dequeued

↓

Executing

↓

Completed

or

Canceled

or

Expired

↓

Disposed
```

A Work Item is never returned to the queue after execution begins.

---

# 8. Queue Capacity

Each queue has a bounded capacity.

Example:

| Queue | Suggested Capacity |
|--------|--------------------|
| Capture | 2 |
| Observation | 2 |
| OCR | 2 |
| Layout | 4 |
| Translation | 4 |
| Presentation | 2 |

Bounded queues prevent unlimited memory growth.

---

# 9. Priority Model

Priority is determined by multiple factors.

Highest priority:

- Newest Revision
- Active Session
- User-visible Work

Medium priority:

- Cache Preparation
- Background Analysis

Lowest priority:

- Diagnostics
- Metrics Collection

Priority may change before execution.

---

# 10. Latest Revision Wins

The runtime follows one fundamental rule.

```
Newest Revision > Oldest Revision
```

Example:

Revision 17

↓

Waiting

Revision 18

↓

Waiting

Revision 19

↓

Waiting

Worker becomes available.

Revision 19 executes first.

Older revisions may be canceled before execution.

---

# 11. Queue Expiration

A Work Item may expire before execution.

Reasons include:

- newer revision exists
- session closed
- capture region changed
- provider unavailable
- user stopped translation

Expired items are removed without execution.

---

# 12. Backpressure

When downstream queues become saturated,

upstream stages slow down automatically.

Example:

Translation Queue is full.

↓

Layout Worker pauses.

↓

OCR Queue gradually fills.

↓

Observation reduces new revisions.

↓

Capture continues observing.

The runtime degrades gracefully instead of crashing.

---

# 13. Queue Cancellation

Every queued item has an associated cancellation token.

Cancellation may occur:

Before dequeue

During execution

After completion (result discarded)

Workers must periodically check cancellation status.

---

# 14. Retry Policy

Retry is controlled by the Scheduler.

Possible reasons:

Temporary OCR failure

Temporary network failure

Temporary AI provider timeout

Retry count is stored inside the Work Item.

Fatal failures are never retried.

---

# 15. Queue Ordering

Ordering is not strict FIFO.

The scheduler may reorder items according to:

Revision freshness

Priority

Estimated execution cost

Current system load

Therefore,

queue order is considered dynamic.

---

# 16. Queue Metrics

Each queue reports runtime statistics.

Examples:

Queue length

Average wait time

Execution latency

Cancellation count

Expiration count

Retry count

Failure rate

These metrics are used for diagnostics and adaptive scheduling.

---

# 17. Memory Considerations

Queue items should reference immutable objects.

Instead of copying large images,

queue items store lightweight references.

Example:

Revision ID

↓

Image stored in Revision Store

↓

Queue stores only the Revision ID

This minimizes memory usage.

---

# 18. Interaction With Scheduler

Queues never decide execution order.

Their responsibility is limited to:

Store

Expose pending work

Remove completed work

The Scheduler determines:

Which item executes next

Whether an item should be canceled

Whether an item should be retried

---

# 19. Design Principles

The Work Queue follows these principles.

- Immutable Work Items
- Bounded Capacity
- Revision-Aware Scheduling
- Latest Revision Wins
- Cancellation First
- Queue Isolation
- Backpressure Support
- Lightweight References
- Observable Metrics

---

# 20. Related Documents

- PIPELINE_RUNTIME.md
- SCHEDULER.md
- CANCELLATION.md
- MEMORY_MODEL.md
- PERFORMANCE_MODEL.md

---

# 21. Summary

The Work Queue is the execution buffer between runtime stages.

Its primary responsibility is not to preserve every task, but to ensure that the most relevant work reaches the workers at the right time while obsolete work is discarded efficiently.

This design keeps CRAI responsive even during rapid scrolling, high-frequency screen updates, and varying AI processing latency.