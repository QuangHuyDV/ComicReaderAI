# Pipeline Orchestration

## Purpose

The Pipeline Orchestration layer coordinates the complete CRAI processing pipeline.

It receives user or system requests, determines which processing stages are required, schedules execution, manages dependencies between stages, and delivers results to the Presentation layer.

The orchestrator owns the execution flow but does not implement OCR, translation, or presentation logic itself.

---

## Position in the Architecture

```text
                User Action
                     │
                     ▼
        Pipeline Orchestrator
                     │
     ┌───────────────┼───────────────┐
     ▼               ▼               ▼
 Capture          Translation      Runtime
     │
     ▼
 OCR
     │
     ▼
 Text
     │
     ▼
 Translation
     │
     ▼
 Presentation
     │
     ▼
 UI
```

The orchestrator coordinates every stage while keeping each module independent.

---

## Responsibilities

The Pipeline Orchestrator is responsible for:

* Receiving processing requests
* Building execution pipelines
* Determining execution order
* Scheduling independent work
* Managing dependencies
* Tracking request lifecycle
* Coordinating cancellation
* Preventing stale updates
* Delivering incremental results
* Reporting execution progress

---

## Non-Responsibilities

The orchestrator is not responsible for:

* OCR recognition
* Text segmentation
* Translation
* Presentation layout
* Image processing
* Persistent storage
* Business logic inside individual modules

Each processing stage owns its own implementation.

---

# High-Level Pipeline

A complete request follows this sequence:

```text
Capture
    │
    ▼
OCR
    │
    ▼
Reading Order
    │
    ▼
Text Model
    │
    ▼
Segmentation
    │
    ▼
Translation Context
    │
    ▼
Translation
    │
    ▼
Presentation
```

The orchestrator ensures each stage receives valid input from the previous stage.

---

# Request Sources

Pipeline execution may begin from different sources.

Examples include:

* User opens a comic page
* User opens a novel chapter
* User scrolls
* User captures part of the screen
* Browser detects new visible content
* Manual translation request
* Refresh request
* Retry request

Different request sources may activate different subsets of the pipeline.

---

# Pipeline Types

Not every request requires the full pipeline.

Examples:

### Comic Page

```text
Capture
 ↓
OCR
 ↓
Reading Order
 ↓
Text
 ↓
Translation
 ↓
Presentation
```

### Novel

```text
DOM Extraction
 ↓
Text
 ↓
Translation
 ↓
Presentation
```

### Retranslate

```text
Translation Context
 ↓
Translation
 ↓
Presentation
```

### Presentation Refresh

```text
Presentation
```

The orchestrator selects the minimal required pipeline.

---

# Pipeline Context

Each execution owns a shared context.

```ts
interface PipelineContext {
    requestId: string;

    sourceType: SourceType;

    documentId?: string;
    chapterId?: string;
    pageId?: string;

    sourceRevision: number;

    startedAt: Date;
}
```

This context is shared across every stage.

---

# Pipeline Stage

Each processing module behaves as a pipeline stage.

```ts
interface PipelineStage {

    name: string;

    execute(
        context: PipelineContext,
        input: unknown
    ): Promise<unknown>;
}
```

Stages never communicate directly.

All communication passes through the orchestrator.

---

# Stage Dependency

The orchestrator resolves execution dependencies.

```text
OCR
 │
 ▼
Reading Order
 │
 ▼
Text Model
 │
 ▼
Segmentation
 │
 ▼
Translation
 │
 ▼
Presentation
```

A stage may begin only after its required inputs become available.

---

# Sequential Execution

Some stages must execute sequentially.

Examples:

* Reading Order after OCR
* Segmentation after Text Model
* Presentation after Translation

The orchestrator guarantees execution order.

---

# Parallel Execution

Independent work may execute simultaneously.

Examples:

```text
Page 1

Page 2

Page 3
```

or

```text
Panel A

Panel B

Panel C
```

or

```text
Segment 1

Segment 2

Segment 3
```

Parallel execution must never change logical output order.

---

# Incremental Execution

Large documents should not wait for the entire pipeline.

Instead:

```text
Page
 ├── Panel 1
 ├── Panel 2
 ├── Panel 3
```

Panel 1 may already appear while Panel 3 is still translating.

Incremental delivery improves responsiveness.

---

# Visible First

The orchestrator should prioritize visible content.

Priority example:

```text
Visible Viewport
        │
        ▼
Nearby Content
        │
        ▼
Remaining Content
```

Visible content should always receive processing resources first.

---

# Request Queue

Multiple requests may exist simultaneously.

```text
Request Queue

Request A

Request B

Request C
```

The orchestrator determines:

* priority
* scheduling
* cancellation
* concurrency

---

# Priority

Typical priorities:

1. Visible viewport
2. User interaction
3. Current page
4. Adjacent page
5. Background preload

Background work should never delay interactive work.

---

# Stage Result

Each stage produces an immutable output.

```text
OCR Result

↓

Reading Order Result

↓

Text Model

↓

Translation

↓

Presentation
```

Later stages consume outputs but never modify them.

---

# Stage Events

Every stage publishes progress.

Example:

```text
Started

Progress

Completed

Failed

Cancelled
```

The orchestrator collects these events and distributes them to interested modules.

---

# Partial Results

Stages may complete partially.

Example:

```text
Page

Panel 1 ✔

Panel 2 ✔

Panel 3 ...

Panel 4 ...
```

Completed work should immediately continue through the remaining pipeline.

The orchestrator should not wait for the entire page.

---

# Retry

If a stage fails:

```text
OCR
 ✔

Translation
 ✖

Retry

Translation
 ✔
```

Only the failed stage should be retried.

Completed stages should not execute again unless their inputs changed.

---

# Reuse

The orchestrator should reuse previous stage outputs.

Example:

```text
OCR
✔ cached

Reading Order
✔ cached

Translation
new
```

Only missing work should execute.

---

# Cancellation

Pipeline execution may stop because:

* User closes the page
* User changes chapter
* Viewport changes
* New request replaces an old request
* Application exits

Cancellation propagates through every active stage.

---

# Stale Protection

Example:

```text
Request A starts

↓

User changes page

↓

Request B starts

↓

Request A finishes

↓

Ignored
```

Old requests must never overwrite newer results.

---

# Error Isolation

Failure in one stage should not invalidate unrelated work.

Example:

```text
Panel 1 ✔

Panel 2 ✖

Panel 3 ✔
```

Only Panel 2 requires recovery.

---

# Progress Reporting

The orchestrator exposes progress.

Example:

```text
OCR

██████████

Translation

█████░░░░░

Presentation

██░░░░░░░░
```

Progress is advisory and should not imply completion quality.

---

# Recovery

Recovery strategy:

1. Retry stage
2. Use cached output
3. Skip optional stage
4. Notify user

Recovery should minimize duplicated work.

---

# Resource Scheduling

Execution resources are finite.

The orchestrator should balance:

* CPU
* GPU
* Memory
* Network
* AI providers

Heavy stages should not starve lightweight stages.

---

# Back Pressure

If downstream stages become slower:

```text
OCR

██████████

Translation

██
```

The orchestrator may slow or pause upstream work to prevent excessive buffering.

---

# Pipeline State

```ts
type PipelineState =
    | "queued"
    | "running"
    | "partial"
    | "completed"
    | "failed"
    | "cancelled";
```

---

# Pipeline Identity

Every execution owns:

* requestId
* pipelineId
* sourceRevision

These identities remain stable during execution.

---

# Observability

Useful metrics include:

* active pipelines
* queued requests
* average latency
* stage duration
* cancellation rate
* retry count
* cache reuse
* stale request count
* concurrent pipelines

---

# Design Principles

### Independent Stages

Every stage remains independently testable.

### Immutable Results

Stages never modify previous outputs.

### Incremental Delivery

Show useful results as soon as possible.

### Visible First

Always prioritize what the user is currently reading.

### Explicit Dependencies

Stage dependencies must be declared rather than inferred.

### Safe Parallelism

Parallel execution must never change logical ordering.

### Cancellation First

Cancellation should propagate quickly to avoid wasted work.

### Stale Protection

Older executions must never replace newer ones.

### Minimal Reprocessing

Reuse valid outputs whenever possible.

---

# Invariants

1. Every pipeline has one request identifier.
2. Stage outputs are immutable.
3. Stages communicate only through the orchestrator.
4. Downstream stages never modify upstream results.
5. Visible content has higher priority than background work.
6. Parallel execution must preserve logical order.
7. Partial completion is allowed.
8. Cancellation propagates through active stages.
9. Failed stages do not invalidate unrelated results.
10. Older requests never overwrite newer results.
11. Cached outputs remain valid only while their inputs are unchanged.
12. Pipeline scheduling remains independent of OCR, Translation, and Presentation implementations.

---

# Related Documents

```text
../ocr/README.md
../text/README.md
../translation/CONTEXT.md
../translation/TRANSLATION.md
../presentation/PRESENTATION.md
REQUEST_LIFECYCLE.md
CANCELLATION.md
STALE_RESULT.md
```
