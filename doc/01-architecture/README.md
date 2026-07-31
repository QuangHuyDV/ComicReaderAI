# Runtime Architecture

This directory defines how CRAI executes asynchronous and real-time work while the application is running.

Runtime architecture transforms product flows into safe, responsive, and resource-aware execution.

---

## Runtime Scope

The runtime is responsible for:

- creating and coordinating processing pipelines
- buffering work between stages
- scheduling workers
- canceling obsolete work
- applying backpressure
- managing transient resources
- preventing stale results
- maintaining UI responsiveness
- collecting execution metrics

The runtime does not implement:

- OCR algorithms
- translation logic
- layout detection algorithms
- UI component rendering
- persistent database schemas

Those responsibilities belong to their respective modules.

---

## Core Runtime Principles

### Latest Valid Revision Wins

New user-visible content has higher value than obsolete pending work.

### Everything Is Cancelable

Long-running tasks must support cooperative cancellation or safe result rejection.

### Never Block the UI Thread

Capture processing, OCR, translation, and provider requests must execute outside the UI thread.

### Immutable Processing Inputs

Workers consume immutable references and produce new immutable outputs.

### Bounded Work

Queues, worker counts, retries, and retained revisions must have explicit limits.

### Cache Before Computation

Reusable work should be resolved before expensive processing begins.

### Atomic Presentation

Only a complete and currently valid presentation model may update the UI.

### Obsolete Work Is Disposable

The runtime is optimized for current user experience, not for completing every task.

---

## Runtime Document Order

Read and design runtime behavior in the following order:

1. `PIPELINE_RUNTIME.md`
2. `WORK_QUEUE.md`
3. `SCHEDULER.md`
4. `CANCELLATION.md`
5. `CACHE_POLICY.md`
6. `MEMORY_MODEL.md`
7. `THREADING_MODEL.md`
8. `RESOURCE_LIFECYCLE.md`
9. `PERFORMANCE_MODEL.md`

Each document depends on decisions from the documents above it.

---

## Document Responsibilities

| Document | Primary question |
|---|---|
| `PIPELINE_RUNTIME.md` | How does work move through runtime stages? |
| `WORK_QUEUE.md` | Where is pending work held between stages? |
| `SCHEDULER.md` | Which work is selected for execution? |
| `CANCELLATION.md` | How is obsolete or interrupted work stopped? |
| `CACHE_POLICY.md` | Which results are reusable and how are they validated? |
| `MEMORY_MODEL.md` | Which runtime data remains in memory and for how long? |
| `THREADING_MODEL.md` | Which execution contexts run each type of work? |
| `RESOURCE_LIFECYCLE.md` | When are runtime resources created and disposed? |
| `PERFORMANCE_MODEL.md` | What latency and resource budgets must be maintained? |

---

## High-Level Runtime Model

```text
Source Observation
    ↓
Revision Creation
    ↓
Pipeline Coordinator
    ↓
Stage Queue
    ↓
Scheduler
    ↓
Worker
    ↓
Immutable Result
    ↓
Revision Validation
    ↓
Next Stage or Presentation
```

---

## Queue Payload Rule

Queues should normally store lightweight work descriptors rather than large payloads.

Example:

```text
Work Item
├── Session ID
├── Revision ID
├── Stage
├── Priority
├── Attempt
└── Cancellation Handle
```

Large data such as images and OCR results should be accessed through revision-scoped storage.

This rule reduces:

- payload copying
- queue memory usage
- synchronization cost
- accidental mutable sharing

---

## Runtime and Domain Boundaries

Runtime decides:

- when work runs
- whether work remains valid
- how much work may run concurrently
- when work should be canceled or retried

Domain modules decide:

- how OCR is performed
- how reading order is resolved
- how translation units are built
- how translation is generated
- how presentation models are constructed

Runtime must not contain domain-specific processing logic.

---

## Current Status

Completed drafts:

- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`

Next document:

```text
SCHEDULER.md
```

The scheduler will define:

- revision priority
- stage priority
- worker assignment
- obsolete-work elimination
- retry decisions
- fairness between active sessions