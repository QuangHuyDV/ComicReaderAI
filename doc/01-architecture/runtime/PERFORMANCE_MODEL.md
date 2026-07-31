# Runtime Performance Model

> Project: CRAI  
> Version: 0.1  
> Status: Architecture Draft

---

## 1. Purpose

This document defines how CRAI evaluates, budgets, protects, measures, and improves runtime performance.

CRAI is an interactive reading assistant.

Its performance cannot be evaluated only through raw throughput or individual stage speed.

The primary performance question is:

> How quickly and consistently can CRAI present a useful translation for the content the user is currently reading?

This document establishes:

- performance goals
- latency boundaries
- responsiveness requirements
- throughput expectations
- resource budgets
- stage budgets
- overload behavior
- adaptive degradation
- measurement rules
- profiling requirements
- MVP performance targets

---

## 2. Scope

This document covers:

- end-to-end latency
- interaction latency
- stage latency
- queue wait time
- capture responsiveness
- revision freshness
- useful-result latency
- throughput
- concurrency efficiency
- CPU usage
- memory usage
- GPU usage
- provider latency
- rendering performance
- cache effectiveness
- cancellation effectiveness
- backpressure performance
- degradation strategies
- performance telemetry
- benchmarking
- MVP performance policy

This document does not define:

- exact provider pricing
- final hardware requirements
- provider-specific service-level agreements
- detailed retry behavior
- detailed error classification
- persistent-storage schema
- exact implementation language or framework

Those concerns belong to related documents.

---

## 3. Performance Philosophy

CRAI follows this core rule:

> Optimize for current-revision usefulness, not maximum pipeline throughput.

A system that processes many obsolete revisions per second is not performant.

A system that quickly rejects stale work and presents the current revision is performant even if its total work throughput is lower.

The preferred optimization order is:

```text
Correct Current Result
    ↓
Responsive UI
    ↓
Low Useful-Result Latency
    ↓
Stable Resource Usage
    ↓
Provider Cost Efficiency
    ↓
Maximum Throughput
```

Correctness and responsiveness must not be traded away for benchmark numbers.

---

## 4. User-Visible Performance

The user mainly experiences:

- delay after scrolling
- delay after changing pages
- delay before translation appears
- visual flicker
- stale translation
- UI freezing
- translation instability
- repeated loading states
- excessive CPU or GPU use
- increased device heat or fan noise

Performance evaluation must include these visible effects.

Internal stage speed alone is insufficient.

---

## 5. Primary Performance Outcome

The primary outcome is:

```text
Useful Translation Latency
```

Useful Translation Latency is the duration from the point where new readable content becomes stable enough to process until a valid translation for the current revision becomes visible.

Conceptually:

```text
Stable Content Detected
    ↓
Current Revision Created
    ↓
Required Pipeline Work Completed
    ↓
Presentation Committed
```

This metric excludes results that:

- belong to obsolete revisions
- fail commit validation
- are canceled
- are too incomplete to help reading
- are displayed after the user has moved away

---

## 6. Freshness as a Performance Dimension

CRAI must measure whether completed work is still relevant.

Define:

```text
Revision Freshness
```

as the relationship between:

- revision being displayed
- current active revision
- latest stable observed content

A fast stale result is worse than a slightly slower current result.

Useful metrics include:

- percentage of completed work accepted
- percentage of completed work rejected as stale
- average stale-work execution time
- average obsolete-work cancellation delay
- current-revision commit ratio

---

## 7. Performance Dimensions

CRAI performance consists of several dimensions.

```text
Performance
├── Responsiveness
├── Latency
├── Freshness
├── Throughput
├── Stability
├── Efficiency
├── Predictability
└── Recovery
```

### Responsiveness

How quickly the application reacts to user commands.

### Latency

How long processing takes.

### Freshness

Whether work still represents the current content.

### Throughput

How much useful work can be completed over time.

### Stability

Whether performance remains bounded during long sessions.

### Efficiency

How much CPU, memory, GPU, network, and provider cost is used.

### Predictability

Whether latency remains reasonably consistent.

### Recovery

How quickly the runtime returns to normal after overload or failure.

---

## 8. Latency Categories

CRAI distinguishes multiple latency categories.

### Interaction Latency

Time from user input to immediate visible UI response.

Examples:

- start command accepted
- stop command acknowledged
- region selection updated
- loading state shown

### Observation Latency

Time from frame availability to stable-content decision.

### Pipeline Latency

Time from revision creation to completed presentation artifact.

### Commit Latency

Time from presentation readiness to visible UI replacement.

### Useful Translation Latency

Time from stable current content to valid visible translation.

### Recovery Latency

Time required to recover after cancellation, overload, or provider failure.

---

## 9. End-to-End Latency Model

End-to-end useful latency can be represented as:

```text
T_useful =
    T_capture
  + T_observation
  + T_revision
  + T_queue
  + T_cache_lookup
  + T_ocr
  + T_layout
  + T_translation
  + T_presentation
  + T_ui_dispatch
  + T_commit
```

Not every execution includes every stage.

A cache hit may remove or greatly reduce:

```text
T_ocr
T_layout
T_translation
```

Parallel work may overlap some components.

The model is conceptual rather than a strict sequential formula.

---

## 10. Critical Path

Only work on the critical path directly affects useful-result latency.

For the screen-comic MVP, the expected critical path is:

```text
Stable Frame
    ↓
Revision Creation
    ↓
OCR
    ↓
Layout
    ↓
Translation
    ↓
Presentation
    ↓
UI Commit
```

Background tasks must not delay this path.

Examples of non-critical work:

- diagnostics export
- persistent cache writes
- cache cleanup
- history indexing
- speculative provider warm-up
- detailed debug visualization

---

## 11. Critical Path Protection

The runtime must protect critical-path work by:

- prioritizing the current revision
- canceling obsolete work
- preventing background queue saturation
- reserving runtime-control capacity
- limiting provider concurrency
- limiting memory pressure
- avoiding blocking UI operations
- using bounded queues
- rejecting speculative work during pressure

---

## 12. Latency Budget

A latency budget divides the user-visible target across stages.

Conceptually:

| Segment | Example responsibility |
|---|---|
| Capture and observation | Detect stable new content |
| Queue and admission | Start relevant work |
| OCR | Recognize source text |
| Layout | Resolve regions and reading order |
| Translation | Produce Vietnamese text |
| Presentation | Build UI-ready model |
| UI commit | Display result |

Budgets are architectural targets, not guarantees.

Exact numeric budgets must be validated through implementation profiling.

---

## 13. Initial User Experience Targets

The MVP should aim for these qualitative targets:

### Immediate UI Response

Commands should produce immediate visible acknowledgment.

Examples:

- start
- stop
- retry
- region adjustment
- provider change

### Fast Cached Reuse

Previously processed identical content should appear with minimal perceptible delay.

### Acceptable New-Content Translation

A new stable comic region should produce useful translation quickly enough not to break reading flow.

### No UI Freeze

Slow OCR or provider requests must not freeze interaction.

### Fast Obsolete-Work Rejection

Rapid scrolling should not create a long backlog.

---

## 14. Suggested Initial Timing Targets

Before real profiling, CRAI may use provisional targets.

| Operation | Provisional target |
|---|---:|
| Immediate UI acknowledgment | under 100 ms |
| UI state update | under 100 ms |
| Capture callback handling | under 16 ms of active work |
| Lightweight frame observation | under 50 ms |
| Current-revision admission | under 50 ms |
| Cached presentation lookup and commit | under 200 ms |
| Presentation commit after artifact ready | under 100 ms |
| Cancellation state propagation | under 100 ms |
| Obsolete queued-work removal | under 100 ms |
| Current-revision useful result | preferably under 2 seconds |
| Slow remote-provider result | tolerable up to several seconds with visible progress |

These values are starting hypotheses.

They are not final product guarantees.

---

## 15. Percentile-Based Evaluation

Average latency alone is misleading.

CRAI should evaluate:

```text
P50
P90
P95
P99
```

for important operations.

Example:

```text
Translation latency:
P50 = typical experience
P95 = slow but recurring experience
P99 = severe tail behavior
```

User experience is often harmed more by repeated high-tail latency than by average latency.

---

## 16. Tail Latency

Tail latency may be caused by:

- provider variability
- model cold start
- CPU contention
- GPU contention
- memory pressure
- garbage collection
- large speech bubbles
- network instability
- retry
- queue backlog
- oversized images
- excessive context

The runtime should identify and report tail causes where possible.

---

## 17. Queue Wait Time

Every WorkItem should distinguish:

```text
Queue Wait Time
```

from:

```text
Execution Time
```

A slow stage may actually be waiting for:

- concurrency capacity
- memory admission
- provider quota
- current-revision priority
- dependent artifact
- resource lease

Without this distinction, optimization may target the wrong component.

---

## 18. WorkItem Timing Model

A WorkItem should support timestamps conceptually equivalent to:

```text
CreatedAt
AdmittedAt
DequeuedAt
StartedAt
ProviderRequestedAt
ProviderCompletedAt
CompletedAt
PublishedAt
CommittedAt
CanceledAt
DisposedAt
```

Not every WorkItem requires every timestamp.

The timestamps enable:

- queue analysis
- provider analysis
- stale-work analysis
- cancellation analysis
- commit analysis

---

## 19. Throughput

Throughput describes how much work can be completed over time.

Possible units:

- revisions processed per minute
- OCR regions processed per second
- translation units processed per second
- current revisions committed per minute

For CRAI, the most meaningful throughput metric is:

```text
Useful Current Revisions Committed
```

Raw completed WorkItems should not be treated as equivalent to useful throughput.

---

## 20. Useful Work Ratio

Define:

```text
Useful Work Ratio =
Accepted Current Work
/
Total Executed Work
```

Low useful-work ratio may indicate:

- cancellation arrives too late
- queues admit obsolete work
- provider concurrency is too high
- capture creates revisions too frequently
- stability detection is poor
- stage batches are too large

---

## 21. Wasted Work

Wasted work includes:

- stale results
- canceled results completed too late
- duplicated provider requests
- repeated OCR for identical content
- artifacts built but never used
- speculative work evicted before use
- presentation artifacts rejected before commit

Some wasted work is unavoidable.

It must remain bounded and observable.

---

## 22. Cancellation Efficiency

Cancellation performance is measured by:

- cancellation propagation latency
- queued-work removal latency
- running-work acknowledgment latency
- resources released after cancellation
- provider request abort success
- wasted execution after cancellation
- stale-result rejection cost

A provider that cannot abort may still be acceptable if:

- request concurrency is low
- stale results are rejected
- cost remains acceptable
- current work is not blocked

---

## 23. Capture Performance

Capture performance should be evaluated through:

- frame acquisition latency
- frame callback delay
- dropped observation count
- capture surface copy count
- capture CPU usage
- capture GPU usage
- source-region size
- capture loop stability

The goal is not to process every possible frame.

The goal is to observe enough frames to detect meaningful content changes reliably.

---

## 24. Capture Rate

A high capture rate may increase:

- CPU use
- GPU use
- memory bandwidth
- frame-copy cost
- comparison workload
- revision noise

The capture rate should be configurable and adaptive.

For comic reading, full display-refresh-rate capture may be unnecessary.

The MVP should start conservatively and increase only when stability detection requires it.

---

## 25. Observation Performance

Observation includes:

- change detection
- stability detection
- fingerprinting
- candidate revision selection

It should avoid expensive full-image operations on every frame where possible.

Potential optimizations include:

- reduced-resolution comparison
- sampled regions
- perceptual hashing
- tile comparison
- incremental change detection

These are implementation options rather than architecture requirements.

---

## 26. Stability Detection Trade-Off

Stability detection balances two delays:

```text
Wait too little
    ↓
Process transitional frames
    ↓
Waste work
```

```text
Wait too long
    ↓
Increase useful translation latency
```

The correct value depends on:

- scroll behavior
- page animation
- website rendering
- image loading
- capture frequency
- OCR cost

Stability parameters must be measurable and configurable.

---

## 27. OCR Performance

OCR performance should measure:

- queue wait
- preprocessing time
- provider initialization
- request encoding
- network latency
- local inference latency
- response normalization
- number of detected regions
- input resolution
- memory peak
- cache hit rate
- cancellation responsiveness

OCR quality and latency must be evaluated together.

Faster OCR that misses important text may increase total latency through retries or manual correction.

---

## 28. Layout Performance

Layout performance includes:

- region grouping
- reading-order calculation
- speech-bubble association
- text-block normalization
- geometry mapping

Layout should not duplicate full OCR data unnecessarily.

The layout stage should remain small relative to OCR and translation for the MVP.

If layout becomes a dominant stage, profiling should determine whether:

- geometry is too complex
- OCR output is oversized
- algorithms are superlinear
- debug structures are retained
- too many regions are processed

---

## 29. Translation Performance

Translation performance should distinguish:

```text
Request Preparation
Provider Queue
Network Time
Provider Processing
Response Download
Normalization
```

Translation latency may depend on:

- provider
- model
- input length
- context length
- glossary size
- output length
- region batching
- provider rate limits
- cold start
- retry

---

## 30. Translation Unit Batching

Batching may reduce:

- network overhead
- provider request count
- prompt duplication
- cost

But large batches may increase:

- first-result latency
- cancellation waste
- context confusion
- retry cost
- tail latency
- provider response size

The MVP should use bounded batching.

A batch should be small enough that rapid revision replacement does not waste excessive work.

---

## 31. Partial Results

CRAI may later support progressive translation.

Example:

```text
Important speech bubbles translated first
    ↓
Remaining bubbles translated afterward
```

This can reduce time to first useful result.

However, progressive rendering introduces:

- partial commit semantics
- layout stability concerns
- result ordering
- flicker risk
- more complex cancellation

The MVP may prefer atomic page-level or region-group presentation unless profiling shows unacceptable latency.

---

## 32. Time to First Useful Result

In addition to complete-result latency, CRAI may measure:

```text
Time to First Useful Result
```

This is the time until enough translated content appears to help the user continue reading.

A first useful result may be:

- first visible speech bubble
- current viewport translation
- prioritized central region
- complete small page

This metric becomes important if progressive presentation is introduced.

---

## 33. Presentation Performance

Presentation performance includes:

- translation-to-view-model mapping
- text measurement
- geometry transformation
- line wrapping
- font fallback
- side-panel model construction
- optional overlay preparation
- UI dispatch
- rendering

Presentation building should occur outside the UI thread where framework rules allow.

Only thread-affine rendering operations should occur on the UI context.

---

## 34. UI Rendering Performance

The UI should avoid:

- full-tree rebuild for small changes
- repeated text measurement
- synchronous image decoding
- unnecessary animation
- unbounded presentation history
- layout thrashing
- large event batches on the UI thread

The UI should replace complete immutable presentation state atomically when practical.

---

## 35. Frame Budget

If the UI targets smooth 60 Hz rendering, one frame has approximately:

```text
16.7 ms
```

CRAI does not need to finish the AI pipeline within one frame.

However, work performed directly on the UI thread should generally stay well below the frame budget.

Long UI operations produce:

- visible stutter
- delayed input
- broken scrolling
- delayed window movement

---

## 36. Cache Performance

Cache performance includes:

- lookup latency
- hit rate
- miss rate
- insertion latency
- eviction latency
- key-generation cost
- reused computation time
- memory cost
- stale-entry rejection

A high hit rate is not useful if:

- lookup is expensive
- key generation copies large data
- cached artifacts are incompatible
- cache consumes excessive memory
- cached presentation is stale

---

## 37. Cache Value

Cache value should be evaluated as:

```text
Avoided Cost
    -
Lookup Cost
    -
Retention Cost
    -
Eviction Cost
```

High-value cache candidates include:

- expensive OCR artifacts
- translation artifacts
- stable image fingerprints

Low-value candidates may include:

- cheap temporary structures
- rapidly invalidated presentation intermediates
- large artifacts with little reuse

---

## 38. Artifact Reuse Performance

Artifact reuse may occur across revisions.

Example:

```text
Revision 50
    └── Source fingerprint A

Revision 51
    └── Source fingerprint A
```

Reuse avoids repeated:

- OCR
- layout
- translation
- presentation construction where compatible

Metrics should report reuse by artifact type.

---

## 39. Cache Lookup Placement

Cache lookup should occur before expensive stage admission where possible.

Preferred:

```text
Stage candidate
    ↓
Resolve deterministic artifact key
    ↓
Cache lookup
    ↓
Hit: publish reference
Miss: admit worker
```

This prevents consuming worker and provider capacity for already available results.

---

## 40. CPU Performance

CPU performance should measure:

- total application CPU
- CPU by stage
- worker utilization
- CPU saturation
- observation cost
- image preprocessing cost
- local-model inference cost
- context-switch pressure
- background-task cost

CRAI must leave enough CPU capacity for:

- the UI
- screen capture
- the reader application
- operating-system interaction

Maximum CPU utilization is not the goal.

---

## 41. CPU Oversubscription

CPU oversubscription occurs when runnable work exceeds useful parallel capacity.

Symptoms include:

- higher latency
- UI stutter
- increased context switching
- increased power use
- worse provider callback handling
- longer cancellation delay

The Scheduler should reduce CPU concurrency when saturation harms useful-result latency.

---

## 42. Memory Performance

Memory performance includes:

- active revision memory
- artifact memory
- cache memory
- worker temporary memory
- provider memory
- GPU memory
- draining canceled memory
- allocation rate
- garbage-collection pause
- disposal latency

High cache hit rate must not justify unbounded memory growth.

---

## 43. Allocation Rate

Frequent allocation of large buffers may cause:

- garbage-collection pressure
- native-memory fragmentation
- allocation latency
- memory spikes

The runtime should measure allocation patterns before introducing custom pooling.

Optimization should target proven hot allocation paths.

---

## 44. GPU Performance

When GPU processing is enabled, measure:

- inference latency
- transfer latency
- GPU memory
- queue depth
- model load latency
- UI rendering contention
- utilization
- canceled-work waste
- tensor disposal latency

High GPU utilization may be harmful if it blocks UI rendering.

---

## 45. Provider Performance

Provider performance should be tracked independently for each:

```text
Provider
Model
Operation
Region
Version
```

Metrics should include:

- request latency
- success rate
- timeout rate
- cancellation support
- rate-limit response
- cold-start latency
- payload size
- token usage
- cost estimate
- stale completion ratio

Provider selection should not rely only on median latency.

---

## 46. Provider Health and Performance

A provider may be technically available but operationally degraded.

Possible states:

```text
HEALTHY
SLOW
RATE_LIMITED
UNSTABLE
UNAVAILABLE
```

Performance signals may influence provider health state.

Provider switching policy belongs in provider architecture and error handling documents.

---

## 47. Provider Warm-Up

Some providers or local models require warm-up.

Warm-up may reduce first-request latency but consumes:

- memory
- CPU
- GPU
- network
- provider quota

The MVP should not perform aggressive speculative warm-up.

Warm-up should be considered only for providers used continuously and where measured benefit is significant.

---

## 48. Cold Start

Cold-start performance includes:

- application startup
- runtime initialization
- capture initialization
- provider client creation
- model loading
- first cache initialization
- first pipeline execution

The first translation may be slower than later translations.

Cold-start latency must be measured separately from steady-state latency.

---

## 49. Steady-State Performance

Steady-state performance describes continuous reading after initialization.

Measure:

- repeated revision latency
- cache reuse
- memory stability
- CPU stability
- provider throughput
- obsolete-work rate
- UI responsiveness
- thermal behavior over long sessions

A system that performs well for one minute but degrades after thirty minutes is not acceptable.

---

## 50. Long-Session Stability

Long-session tests should verify:

- memory stabilizes
- thread count remains bounded
- queue depth remains bounded
- artifact count remains bounded
- cache respects budget
- provider clients remain healthy
- UI responsiveness remains stable
- cancellation still works
- model memory does not accumulate
- diagnostic data remains bounded

---

## 51. Backpressure Performance

Backpressure should prevent overload from moving downstream.

Signals include:

- full queue
- stage concurrency saturation
- memory pressure
- provider rate limit
- excessive stale-work ratio
- long queue wait
- UI commit delay

Backpressure actions may include:

- replace pending observation
- drop obsolete work
- delay new admission
- reduce capture rate
- reduce worker concurrency
- reduce batch size
- disable background work

---

## 52. Overload Definition

The runtime is overloaded when incoming or generated work exceeds its ability to produce relevant results within acceptable bounds.

Symptoms:

- queue growth
- increasing stale-work ratio
- increasing useful-result latency
- memory growth
- provider saturation
- repeated cancellation after expensive execution
- UI commit delay
- excessive CPU or GPU use

Overload must be detected before process instability.

---

## 53. Overload Response Order

When overloaded, CRAI should respond in this order:

1. reject obsolete results
2. remove obsolete queued work
3. cancel obsolete running work
4. stop speculative work
5. stop background work
6. reduce capture or observation frequency
7. reduce batch size
8. reduce concurrency where contention exists
9. evict low-value cache
10. lower processing resolution when permitted
11. switch provider or processing mode if configured
12. delay or reject new non-critical work

The current revision remains the priority.

---

## 54. Graceful Degradation

CRAI should degrade quality or coverage before losing responsiveness.

Possible degradation levels:

```text
FULL
REDUCED
MINIMAL
CONTROL_ONLY
```

### Full

Normal quality and configured pipeline.

### Reduced

Lower-cost processing with minor quality reduction.

### Minimal

Translate only the most important current region.

### Control Only

Pause expensive processing while keeping the UI responsive.

---

## 55. Degradation Options

Possible degradation actions include:

- reduce capture frequency
- reduce image comparison resolution
- OCR only changed regions
- reduce OCR input resolution
- reduce translation context
- translate fewer regions
- disable speculative presentation work
- disable debug artifacts
- unload unused models
- use remote instead of local provider
- use local instead of slow remote provider
- pause background cache writes

Each action must preserve correctness.

---

## 56. Quality and Performance Trade-Off

Performance tuning must account for output quality.

Examples:

```text
Lower OCR resolution
    ↓
Faster inference
    ↓
Possible text-recognition errors
```

```text
Smaller translation context
    ↓
Lower latency and cost
    ↓
Possible pronoun or name inconsistency
```

A performance improvement is acceptable only when quality remains within product expectations.

---

## 57. Adaptive Performance

Future versions may adapt based on runtime measurements.

Inputs may include:

- device CPU
- available memory
- GPU availability
- provider latency
- queue pressure
- stale-work ratio
- average page complexity
- user scrolling speed
- battery state

Outputs may include:

- capture rate
- stability delay
- concurrency
- provider choice
- batch size
- model residency
- cache budget
- OCR resolution

Adaptive behavior should not be introduced before baseline behavior is measurable and stable.

---

## 58. Device Capability Profile

CRAI may classify devices into performance profiles.

Example:

```text
LOW
STANDARD
HIGH
CUSTOM
```

A profile may configure:

- capture rate
- OCR resolution
- worker concurrency
- cache size
- local-model eligibility
- GPU use
- context size

The MVP may start with one conservative default profile.

---

## 59. Power and Thermal Performance

Desktop performance includes power and heat.

Continuous capture and local AI may cause:

- high fan noise
- battery drain
- thermal throttling
- reduced reader-application performance

The runtime should avoid unnecessary continuous maximum utilization.

Possible future modes:

```text
PERFORMANCE
BALANCED
POWER_SAVING
```

---

## 60. Current Revision Priority

The current revision should receive:

- queue priority
- worker admission
- provider capacity
- memory protection
- UI commit authority

Older revisions may retain work only when:

- their result can be reused
- cancellation is more expensive than completion
- they do not block current work
- their resource cost remains bounded

---

## 61. Revision Churn

Revision churn occurs when the runtime creates revisions too frequently.

Causes may include:

- unstable frame detection
- animation
- ads
- cursor movement
- loading indicators
- minor anti-aliasing changes
- scrolling transitions

High revision churn increases:

- canceled work
- provider cost
- queue pressure
- memory use
- stale-result ratio

Revision creation rate must be measured.

---

## 62. Revision Churn Metrics

Track:

- observed frames per second
- candidate changes per second
- stable revisions per minute
- canceled revisions per minute
- average revision lifetime
- revisions reaching OCR
- revisions reaching translation
- revisions reaching UI commit

These metrics help locate unnecessary pipeline activation.

---

## 63. Region-Level Optimization

Future versions may detect changed regions rather than reprocessing the full capture area.

Conceptually:

```text
Previous Revision
    +
Current Frame
    ↓
Changed Regions
    ↓
Reuse unchanged artifacts
    ↓
Process only changed content
```

This may greatly reduce cost during:

- incremental scroll
- animated pages
- partial panel changes
- UI overlays

Region-level incremental processing is not required for the first MVP.

---

## 64. Performance and Correctness

Performance optimizations must not bypass:

- revision validation
- artifact compatibility
- cancellation checks
- cache-key validation
- commit permission
- ownership rules
- provider safety
- privacy policy

Incorrect cached reuse is not a valid performance improvement.

---

## 65. Performance and Retry

Retries increase latency and resource consumption.

Retry performance must measure:

- first-attempt latency
- retry delay
- retry execution
- total recovery time
- duplicate provider cost
- stale status at retry time

Before retrying, the runtime must confirm that the revision is still relevant.

Detailed retry rules belong in `RETRY_POLICY.md`.

---

## 66. Performance and Errors

Failure should not indefinitely occupy capacity.

Examples:

- provider timeout must release request slot
- failed worker must release leases
- corrupt artifact must not repeatedly trigger immediate work
- repeated provider failure must trigger backoff
- UI error presentation must remain fast

Detailed error categories belong in `ERROR_MODEL.md`.

---

## 67. Performance Events

Possible events include:

```text
performance.pressure.changed
performance.budget.exceeded
performance.stage.slow
performance.provider.slow
performance.queue.saturated
performance.stale_ratio.high
performance.degradation.entered
performance.degradation.exited
performance.model.cold_start
performance.recovery.completed
```

Events should remain lightweight.

They should not contain full artifacts.

---

## 68. Performance Metrics

Core metrics should include:

### End-to-End

- useful translation latency
- time to first useful result
- current-revision commit latency
- current-revision success ratio

### Queue

- queue depth by stage
- queue wait by stage
- dropped WorkItems
- obsolete WorkItems removed

### Stage

- execution latency
- success rate
- cancellation rate
- stale completion rate
- cache hit rate

### Resources

- CPU
- memory
- GPU
- network
- provider in-flight count
- worker utilization

### User Experience

- UI dispatch delay
- UI freeze or long-task count
- presentation replacement latency
- repeated loading duration

---

## 69. Metric Dimensions

Metrics may be tagged by:

```text
Stage
Provider
Model
LanguagePair
ExecutionClass
CacheStatus
ResultStatus
CancellationReason
DeviceProfile
RevisionStatus
```

High-cardinality values such as raw IDs should not be used indiscriminately in aggregated metrics.

SessionId and RevisionId belong mainly in traces and structured logs.

---

## 70. Tracing

A revision trace should connect:

```text
Frame Observation
    ↓
Revision Creation
    ↓
Cache Lookup
    ↓
OCR
    ↓
Layout
    ↓
Translation
    ↓
Presentation
    ↓
UI Commit
```

Each stage span should include:

- queue wait
- execution time
- provider time
- cache status
- cancellation status
- result disposition
- artifact identity metadata
- revision freshness

---

## 71. Performance Logging

Structured logs should be used for unusual performance events.

Examples:

- work exceeded stage budget
- provider timeout
- cancellation not acknowledged
- queue remained full
- memory pressure entered critical
- artifact disposal exceeded expected latency
- UI commit rejected after long processing

Routine high-frequency timing should use metrics or traces rather than verbose logs.

---

## 72. Benchmark Classes

CRAI should define benchmark classes.

### Microbenchmarks

Measure one operation:

- image fingerprint
- preprocessing
- cache-key creation
- layout algorithm
- presentation mapping

### Stage Benchmarks

Measure one pipeline stage with realistic input.

### End-to-End Benchmarks

Measure stable frame to UI-ready presentation.

### Stress Benchmarks

Measure overload behavior.

### Endurance Benchmarks

Measure long-session stability.

### Provider Benchmarks

Measure provider latency and variability.

---

## 73. Benchmark Input Set

Benchmarks should use representative content:

- simple comic page
- dense comic page
- vertical Chinese text
- horizontal Chinese text
- small speech bubbles
- stylized fonts
- low-contrast text
- high-resolution source
- partial scrolling
- repeated identical frame
- rapid page changes

One easy test image is insufficient.

---

## 74. Device Benchmark Profiles

Benchmarks should eventually cover:

- minimum supported device
- typical device
- high-performance device
- device without supported GPU
- slow network
- unstable network
- remote provider degradation

Performance targets should be based primarily on minimum and typical devices.

---

## 75. Controlled Performance Testing

Performance tests should control:

- input frames
- capture timing
- revision timing
- provider delay
- cache state
- worker concurrency
- cancellation timing
- UI dispatch timing
- memory pressure
- retry outcome

Fake providers should support deterministic delay and completion ordering.

---

## 76. Performance Regression

A performance regression occurs when a change materially worsens:

- useful-result latency
- tail latency
- stale-work ratio
- CPU usage
- memory usage
- provider request count
- UI responsiveness
- long-session stability

Regression thresholds should be defined after baseline measurements exist.

---

## 77. Optimization Workflow

Performance optimization should follow:

```text
Measure
    ↓
Identify Critical Bottleneck
    ↓
Form Hypothesis
    ↓
Make One Controlled Change
    ↓
Benchmark
    ↓
Compare Quality and Resource Cost
    ↓
Keep or Revert
```

Optimization must not begin with speculative complexity.

---

## 78. Premature Optimization Policy

The MVP should avoid introducing without evidence:

- complex custom thread pools
- custom allocators
- large buffer-pool systems
- multi-process pipelines
- speculative precomputation
- aggressive parallel OCR
- distributed caching
- fine-grained incremental graph recomputation
- dynamic provider-routing algorithms

The architecture keeps these options possible but does not require them.

---

## 79. MVP Performance Policy

The MVP should use:

```text
Current Revision First
    +
Low Bounded Concurrency
    +
Latest-Frame Observation
    +
Memory-Only Artifact Cache
    +
Remote Translation with Bounded Requests
    +
Atomic Presentation Commit
```

Primary MVP optimization goals:

1. UI never freezes during pipeline processing.
2. Capture does not wait for OCR or translation.
3. Obsolete queued work is removed quickly.
4. Obsolete results never update the UI.
5. Identical content reuses compatible artifacts.
6. Memory usage stabilizes during long sessions.
7. Provider requests remain bounded.
8. Metrics expose where latency occurs.

---

## 80. Suggested MVP Stage Priorities

Priority order:

```text
Runtime Control
    ↓
Cancellation and Revision Replacement
    ↓
Current Revision Critical Path
    ↓
Current Presentation Commit
    ↓
Cache Maintenance
    ↓
Diagnostics
    ↓
Speculative Work
```

Speculative work may remain disabled initially.

---

## 81. Suggested MVP Concurrency

Initial concurrency should remain:

| Stage | Initial concurrency |
|---|---:|
| Observation | 1 |
| OCR | 1 |
| Layout | 1 |
| Translation | 1 |
| Presentation build | 1 |
| UI commit | 1 |

This does not necessarily mean six dedicated threads.

It defines maximum simultaneous execution per logical stage.

---

## 82. Suggested MVP Capture Policy

Initial capture behavior:

- one capture source
- bounded capture frequency
- latest-frame replacement
- serial observation
- stability delay
- no permanent queue of captured frames
- no full-rate capture requirement
- no revision for unchanged content

Capture parameters remain configurable.

---

## 83. Suggested MVP Cache Policy

Performance-sensitive cache behavior:

- memory-only
- deterministic keys
- cheap lookup
- bounded memory
- current-session reuse
- OCR and translation artifacts prioritized
- canceled work excluded
- no blocking persistent write on critical path

---

## 84. Suggested MVP Provider Policy

Provider execution should use:

- one request at a time per main provider
- strict timeout
- cooperative cancellation
- stale-result rejection
- bounded request body
- bounded translation context
- no unlimited retries
- metrics by provider and model

---

## 85. Suggested MVP Performance Dashboard

A development dashboard or diagnostic snapshot should display:

```text
Current Revision
Current Runtime State
Useful Result Latency
Stage Latencies
Queue Depths
Provider In-Flight Count
Cache Hit Status
CPU Usage
Memory Usage
Stale Completion Count
Cancellation Count
```

It does not need to be user-facing in the first release.

---

## 86. Example: Normal New Page

```text
New page becomes stable
    ↓ 80 ms
Revision created and admitted
    ↓ 500 ms
OCR completed
    ↓ 40 ms
Layout completed
    ↓ 700 ms
Translation completed
    ↓ 80 ms
Presentation and UI commit
```

Useful-result latency:

```text
Approximately 1.4 seconds
```

This is only an example, not a guaranteed budget.

---

## 87. Example: Cache Hit

```text
Stable frame detected
    ↓
Fingerprint resolved
    ↓
OCR artifact hit
    ↓
Layout artifact hit
    ↓
Translation artifact hit
    ↓
Presentation built or reused
    ↓
UI committed
```

The user should experience near-immediate reuse.

---

## 88. Example: Rapid Scrolling

```text
Revision 30 OCR starts
    ↓
Revision 31 appears
    ↓
Revision 30 canceled
    ↓
Revision 32 appears
    ↓
Revision 31 pending work removed
    ↓
Revision 32 becomes current
```

Performance success means:

- the queue does not grow
- old translations do not appear
- Revision 32 starts promptly
- resources from old work drain safely

---

## 89. Example: Slow Provider

```text
Translation request exceeds normal latency
    ↓
Provider marked slow
    ↓
No additional unlimited requests admitted
    ↓
UI remains responsive
    ↓
Current request completes, times out, or cancels
```

Future versions may switch providers.

The MVP may show a loading or degraded state.

---

## 90. Example: High Memory Pressure

```text
Memory pressure becomes HIGH
    ↓
Background work stopped
    ↓
Low-value cache evicted
    ↓
Obsolete revisions disposed
    ↓
Worker concurrency reduced
    ↓
Current revision preserved
```

The goal is controlled latency degradation rather than process failure.

---

## 91. Example: High Stale-Work Ratio

```text
Many provider results rejected as stale
    ↓
Runtime detects high stale ratio
```

Possible causes:

- translation batches too large
- provider too slow
- stability delay too short
- revisions created too frequently
- provider concurrency too high

Possible response:

- increase stability threshold
- reduce batch size
- avoid starting translation during rapid revision churn
- change provider configuration

---

## 92. Performance Invariants

The runtime must preserve these invariants:

1. UI responsiveness has priority over domain throughput.
2. Current-revision work has priority over obsolete work.
3. Stale results do not count as useful throughput.
4. Queues and concurrency remain bounded.
5. Capture does not queue every frame.
6. Slow providers do not create unlimited in-flight requests.
7. Background work cannot block the critical path.
8. Performance optimizations cannot bypass correctness validation.
9. Cache availability is optional for correctness.
10. Memory growth must remain bounded during long sessions.
11. Control and cancellation paths retain execution capacity.
12. Tail latency is measured, not only averages.
13. Queue wait and execution time are measured separately.
14. Quality degradation must be explicit and controlled.
15. The runtime responds to overload before uncontrolled failure.

---

## 93. Performance Testing Requirements

Tests should include:

- normal single-page processing
- cached page reuse
- rapid scrolling
- continuous slow scrolling
- provider delay
- provider timeout
- repeated cancellation
- many OCR regions
- high-resolution capture
- memory pressure
- CPU saturation
- GPU contention where supported
- UI dispatch delay
- session lasting multiple hours
- repeated provider initialization
- cache eviction during active processing
- cold start
- warm steady state
- application shutdown during slow work

---

## 94. Open Questions

The following questions remain open:

- What minimum hardware should CRAI support?
- Which desktop framework will be used?
- What capture rate is sufficient for common reading websites?
- What stability delay gives the best latency-to-waste balance?
- Which OCR provider provides acceptable Chinese comic accuracy?
- Will OCR run locally or remotely in the MVP?
- Which translation provider and model will be selected?
- What useful-result latency is acceptable to users?
- Should the UI show partial translation?
- Should changed-region processing be included after the MVP?
- What cache budget is appropriate?
- What provider timeout should be used?
- Should provider selection adapt automatically?
- Will the product expose performance or power modes?
- How should performance differ between side-panel and overlay modes?

These questions require implementation benchmarks and user testing.

---

## 95. Related Documents

- `README.md`
- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`
- `SCHEDULER.md`
- `CANCELLATION.md`
- `CACHE_POLICY.md`
- `MEMORY_MODEL.md`
- `THREADING_MODEL.md`
- `RESOURCE_LIFECYCLE.md`
- `ERROR_MODEL.md`
- `RETRY_POLICY.md`
- `RUNTIME_OBSERVABILITY.md`
- `RUNTIME_CONFIG.md`
- `../DATA_FLOW.md`
- `../STATE_MACHINE.md`
- `../EVENT_BUS.md`
- `../flows/SCREEN_COMIC_FLOW.md`

---

## 96. Next Step

The next runtime document should be:

```text
ERROR_MODEL.md
```

It should define:

- runtime error taxonomy
- expected and unexpected errors
- stage errors
- provider errors
- resource errors
- cancellation-related outcomes
- stale-result outcomes
- transient and permanent failures
- user-visible error mapping
- error ownership
- error propagation
- structured error payloads
- recovery eligibility
- fatal runtime conditions

After `ERROR_MODEL.md`, the next document should be:

```text
RETRY_POLICY.md
```

---

## 97. Summary

CRAI evaluates performance through useful current-revision output rather than raw processing speed.

The central performance model is:

```text
Stable Current Content
    ↓
Bounded Critical-Path Processing
    ↓
Current Valid Translation
    ↓
Fast Atomic Presentation
```

The main optimization priorities are:

```text
Responsiveness
    +
Freshness
    +
Useful-Result Latency
    +
Bounded Resource Usage
    +
Predictable Recovery
```

The MVP should remain conservative:

- low concurrency
- bounded queues
- latest-frame observation
- current-revision priority
- memory-only cache
- bounded provider requests
- atomic UI commit
- strong performance metrics

Advanced adaptive scheduling, incremental regions, progressive translation, automatic provider routing, and device-specific tuning should only be introduced after baseline implementation and profiling provide evidence that they are necessary.