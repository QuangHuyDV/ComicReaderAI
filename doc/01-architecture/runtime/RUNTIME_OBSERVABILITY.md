# Runtime Observability

> Project: CRAI  
> Version: 0.1  
> Status: Architecture Draft

---

## 1. Purpose

This document defines how CRAI observes, diagnoses, measures, and explains runtime behavior.

CRAI processes continuously changing screen content through an asynchronous pipeline:

```text
Capture
    ↓
Observation
    ↓
Revision
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

The pipeline also includes:

- queues
- scheduling
- cancellation
- retries
- cache reuse
- provider calls
- resource ownership
- artifact leases
- stale-result rejection
- UI authority validation

Without a unified observability model, it becomes difficult to answer questions such as:

- Why did the translation appear slowly?
- Which stage caused the delay?
- Did the provider fail or was the result stale?
- Why was a revision canceled?
- How much work was wasted?
- Is memory still owned or waiting for lease release?
- Did a retry improve the result?
- Is the UI blocked?
- Is the runtime overloaded?
- Why was a result not committed?

This document establishes the signals, identifiers, privacy rules, and diagnostic tools required to answer those questions.

---

## 2. Scope

This document covers:

- observability goals
- telemetry signal types
- structured logs
- metrics
- traces
- runtime events
- diagnostic snapshots
- correlation identifiers
- revision tracing
- WorkItem tracing
- attempt tracing
- queue visibility
- provider visibility
- retry visibility
- cancellation visibility
- stale-work visibility
- cache visibility
- artifact lifecycle visibility
- resource and memory visibility
- UI responsiveness visibility
- performance-budget monitoring
- error observability
- privacy and redaction
- cardinality control
- sampling
- telemetry retention
- development diagnostics
- production telemetry
- testing requirements
- MVP observability policy

This document does not define:

- exact telemetry vendor
- exact logging library
- final dashboard technology
- cloud monitoring infrastructure
- final data-retention periods
- detailed user-facing diagnostics UI
- analytics or product-behavior tracking

Those decisions belong to implementation and deployment documentation.

---

## 3. Observability Goals

The runtime observability system must allow developers and operators to:

- reconstruct the lifecycle of a revision
- identify the current critical-path bottleneck
- distinguish queue delay from execution delay
- distinguish failure from cancellation and staleness
- identify excessive revision churn
- identify wasted computation
- identify provider degradation
- identify retry storms
- identify resource leaks
- identify artifact retention problems
- identify UI responsiveness problems
- identify runtime overload
- validate architecture invariants
- diagnose failures without exposing reading content
- compare cold-start and steady-state behavior
- verify graceful shutdown
- measure whether performance targets are being met

---

## 4. Core Philosophy

CRAI follows this rule:

> Observability must explain runtime decisions, not merely record implementation activity.

A log such as:

```text
Translation completed
```

is insufficient.

The runtime must also be able to determine:

```text
Translation completed
    ↓
For which Session?
For which Revision?
For which Attempt?
Was it still authoritative?
Was the result published?
Was the result committed?
Was it rejected as stale?
How long did it wait?
How long did it execute?
```

Observability must reflect the architecture model.

---

## 5. Observability Is Not Logging

Observability consists of several complementary signal types.

```text
Runtime Observability
├── Metrics
├── Traces
├── Structured Logs
├── Runtime Events
└── Diagnostic Snapshots
```

Each signal answers a different type of question.

---

## 6. Metrics

Metrics answer aggregate questions.

Examples:

- How many revisions are created per minute?
- What is the P95 useful-result latency?
- How many WorkItems are waiting?
- What percentage of completed work is stale?
- How often does OCR fail?
- How much memory is retained by cache?
- How many provider requests are in flight?
- How often are retries successful?

Metrics should be:

- cheap to record
- bounded in cardinality
- suitable for aggregation
- suitable for alerting
- suitable for trend analysis

---

## 7. Traces

Traces answer causal and chronological questions.

Examples:

- What happened to Revision 142?
- Which stage delayed its completion?
- Which provider request timed out?
- Which retry eventually succeeded?
- Why was the final presentation rejected?
- Which artifact was reused from cache?

A trace should show the complete path of one logical operation.

---

## 8. Structured Logs

Structured logs answer event-specific diagnostic questions.

Examples:

- Why did artifact publication fail?
- What error caused the provider to degrade?
- Why was a duplicate terminal event ignored?
- Why did shutdown abandon a worker?
- Why was cache corruption detected?

Logs should be used for meaningful events, not every high-frequency operation.

---

## 9. Runtime Events

Runtime events are internal architectural messages.

Examples:

```text
revision.created
work.started
work.completed
artifact.published
provider.degraded
session.failed
```

Runtime events are not automatically telemetry.

The observability layer may subscribe to selected events and convert them into:

- metrics
- trace spans
- structured logs
- diagnostic state

The event bus must not depend on telemetry availability.

---

## 10. Diagnostic Snapshots

A diagnostic snapshot captures runtime state at a specific moment.

Example:

```text
Runtime Snapshot
├── Application State
├── Active Sessions
├── Current Revisions
├── Queue Depths
├── Running WorkItems
├── Provider Health
├── Memory Pressure
├── Artifact Counts
├── Cache Usage
├── Worker Utilization
└── Recent Errors
```

Snapshots are useful when the runtime is:

- stuck
- slow
- shutting down incorrectly
- retaining memory
- repeatedly retrying
- rejecting commits

A snapshot describes current state rather than historical flow.

---

## 11. Signal Selection

The runtime should use the smallest appropriate signal.

Use metrics for:

- counts
- rates
- durations
- ratios
- resource usage

Use traces for:

- causal execution flow
- revision timelines
- retry lineage
- cross-stage latency

Use logs for:

- abnormal events
- errors
- invariant violations
- state explanations

Use snapshots for:

- current runtime inspection
- support diagnostics
- leak investigation

Do not log every operation solely because it is observable.

---

## 12. Correlation Model

CRAI requires consistent correlation identifiers.

Primary hierarchy:

```text
ApplicationInstanceId
    ↓
SessionId
        ↓
RevisionId
            ↓
WorkItemId
                ↓
AttemptId
```

Additional identifiers include:

```text
ArtifactId
ProviderRequestId
TraceId
SpanId
CaptureSourceId
PresentationId
```

These identifiers serve different purposes and must not be treated as interchangeable.

---

## 13. ApplicationInstanceId

`ApplicationInstanceId` identifies one running application process or logical runtime instance.

It is useful for:

- startup and shutdown correlation
- crash investigation
- separating repeated launches
- provider-client lifecycle
- resource-leak analysis

A new value should be created on each application start.

---

## 14. SessionId

`SessionId` identifies one reading session.

A session may include:

- one capture source
- one selected screen region
- language configuration
- provider configuration
- multiple revisions

The SessionId allows all activity from one reading flow to be correlated.

---

## 15. RevisionId

`RevisionId` identifies one logical version of observed content.

A revision is created only when the observation system accepts content as meaningfully changed and stable enough to process.

Revision telemetry should answer:

- when the revision was created
- whether it became current
- which artifacts were reused
- which stages executed
- whether it succeeded
- whether it became obsolete
- whether it committed to UI
- how much work was wasted

---

## 16. WorkItemId

`WorkItemId` identifies one scheduled unit of work.

Examples:

- OCR WorkItem
- Layout WorkItem
- Translation WorkItem
- Presentation WorkItem

A retry should not reuse the same WorkItem authority unless the architecture explicitly models WorkItem separately from Attempt.

For the MVP, the recommended model is:

```text
Logical Stage Work
    ↓
One or more Attempts
```

Each physical execution attempt must still have its own AttemptId.

---

## 17. AttemptId

`AttemptId` identifies one physical execution attempt.

Example:

```text
Revision 80
└── Translation WorkItem
    ├── Attempt 1: timeout
    ├── Attempt 2: provider unavailable
    └── Attempt 3: success
```

AttemptId is required to prevent late outcomes from older attempts from appearing authoritative.

Observability must preserve attempt lineage.

---

## 18. ArtifactId

`ArtifactId` identifies an immutable artifact.

Examples:

- source image
- OCR result
- layout
- translation
- presentation model

Artifact telemetry should support:

- creation tracing
- publication tracing
- lease tracking
- cache retention
- revision ownership
- disposal timing
- reuse analysis

Artifact identifiers should not embed private content.

---

## 19. ProviderRequestId

`ProviderRequestId` identifies one provider operation.

It may be:

- generated internally
- supplied by provider
- mapped from provider response metadata

It allows CRAI logs to correlate local request state with provider diagnostics.

Provider request identifiers must never contain credentials.

---

## 20. Correlation Requirements

Every stage completion should be traceable to at least:

```text
SessionId
RevisionId
WorkItemId
AttemptId
Stage
```

Provider work should additionally include:

```text
ProviderId
ProviderRequestId
ModelId
```

Artifact events should additionally include:

```text
ArtifactId
ArtifactType
```

---

## 21. Correlation Propagation

Correlation context must propagate through:

```text
Runtime Command
    ↓
Scheduler
    ↓
Queue
    ↓
Worker
    ↓
Provider Adapter
    ↓
Completion Command
    ↓
Runtime Control
    ↓
Artifact Publication
    ↓
UI Commit
```

Correlation context should remain lightweight.

It must not contain full domain payloads.

---

## 22. Pipeline Trace

A revision trace should conceptually contain:

```text
Revision Trace
├── Observation
├── Revision Creation
├── Cache Resolution
├── OCR
├── Layout
├── Translation
├── Presentation
├── Commit Validation
└── UI Commit
```

Optional branches may include:

- retry
- fallback
- cancellation
- cache hit
- stale rejection
- partial success
- resource wait

---

## 23. Revision Root Span

Each revision should create one root trace span.

Suggested name:

```text
runtime.revision
```

Useful attributes:

```text
session.id
revision.id
revision.sequence
source.type
source.language
target.language
revision.current_at_creation
device.profile
runtime.mode
```

Do not include recognized or translated text.

---

## 24. Stage Spans

Each stage attempt should create a span.

Examples:

```text
runtime.ocr
runtime.layout
runtime.translation
runtime.presentation
runtime.ui_commit
```

Suggested attributes:

```text
stage
attempt.number
queue.wait_ms
execution.duration_ms
cache.status
provider.id
provider.model
result.status
result.current
cancellation.requested
error.code
```

---

## 25. Queue Wait Span or Attribute

Queue wait should be separately observable.

Possible model:

```text
WorkItem created
    ↓
Queue wait
    ↓
Execution
```

It may be represented as:

- a child span
- span events
- timestamps on one stage span

The important requirement is that queue delay must not be merged invisibly into execution duration.

---

## 26. Provider Span

Remote provider activity should create a dedicated span.

Suggested names:

```text
provider.ocr.request
provider.translation.request
```

Suggested fields:

```text
provider.id
provider.model
provider.operation
request.input_size
request.region_count
request.timeout_ms
response.status
provider.http_status
provider.error_code
provider.request_id
network.duration_ms
provider.duration_ms
```

Sensitive request content must not be attached.

---

## 27. Cache Span

Cache operations should be visible when they affect latency or correctness.

Examples:

```text
cache.lookup
cache.publish
cache.evict
```

Suggested attributes:

```text
artifact.type
cache.key_version
cache.result
cache.lookup_ms
cache.retention_class
cache.eviction_reason
```

The raw cache key should not be logged if it could reveal content fingerprints unnecessarily.

---

## 28. Artifact Lifecycle Trace

An artifact lifecycle may span beyond one revision.

Conceptual flow:

```text
Created
    ↓
Registered
    ↓
Published
    ↓
Leased
    ↓
Released
    ↓
Retained by Cache
    ↓
Evicted
    ↓
Disposed
```

A full distributed-style trace may not be appropriate for long-lived artifacts.

Instead, use:

- lifecycle events
- artifact-state metrics
- structured logs for anomalies
- diagnostic snapshot details

---

## 29. Trace Completion Status

A revision trace should end with one final disposition.

Possible values:

```text
COMMITTED
FAILED
CANCELED
OBSOLETE
STALE_RESULT
SESSION_CLOSED
ABANDONED
```

This final disposition is distinct from individual WorkItem outcomes.

---

## 30. Trace Sampling

Tracing every high-frequency frame or comparison may be expensive.

The runtime should prioritize tracing:

- accepted revisions
- errors
- retries
- slow revisions
- stale completions
- provider failures
- performance-budget violations
- shutdown anomalies

Frames that never become revisions may be represented through metrics rather than full traces.

---

## 31. Metrics Design Principles

Metrics must:

- use stable names
- use bounded dimensions
- avoid raw identifiers as labels
- distinguish logical and physical work
- distinguish current and stale work
- distinguish queue wait and execution
- distinguish cold and warm execution
- remain low overhead

---

## 32. Metric Naming

Suggested naming pattern:

```text
crai.runtime.<domain>.<measurement>
```

Examples:

```text
crai.runtime.revision.created_total
crai.runtime.work.queue_depth
crai.runtime.translation.duration_ms
crai.runtime.provider.requests_inflight
crai.runtime.artifact.active_count
crai.runtime.ui.commit_duration_ms
```

Exact naming may depend on the metrics system.

The conceptual names should remain stable.

---

## 33. Metric Types

Use counters for:

```text
revision.created_total
work.completed_total
work.failed_total
retry.started_total
artifact.disposed_total
```

Use gauges for:

```text
queue.depth
provider.requests_inflight
artifact.active_count
memory.active_bytes
worker.active_count
```

Use histograms for:

```text
useful_result_latency
queue_wait_duration
stage_execution_duration
provider_request_duration
artifact_lifetime
```

---

## 34. Revision Metrics

Recommended revision metrics:

```text
revision.created_total
revision.committed_total
revision.failed_total
revision.canceled_total
revision.obsolete_total
revision.active_count
revision.creation_rate
revision.lifetime_ms
revision.useful_latency_ms
revision.time_to_first_useful_result_ms
revision.commit_ratio
```

---

## 35. Revision Churn Metrics

Track:

```text
observation.frame_total
observation.changed_frame_total
observation.stable_candidate_total
revision.created_total
revision.canceled_total
revision.reached_ocr_total
revision.reached_translation_total
revision.committed_total
```

These metrics show where excessive work is entering the pipeline.

---

## 36. WorkItem Metrics

Recommended WorkItem metrics:

```text
work.created_total
work.admitted_total
work.started_total
work.succeeded_total
work.failed_total
work.canceled_total
work.stale_total
work.abandoned_total
work.active_count
work.queue_wait_ms
work.execution_ms
```

Dimensions may include:

```text
stage
result_status
execution_class
```

---

## 37. Attempt Metrics

Recommended attempt metrics:

```text
attempt.started_total
attempt.completed_total
attempt.failed_total
attempt.superseded_total
attempt.duration_ms
attempt.count_per_workitem
```

Attempt metrics should distinguish:

- initial attempt
- automatic retry
- manual retry
- fallback-provider attempt

---

## 38. Useful Work Metrics

Raw completion count is insufficient.

Track:

```text
useful_work.completed_total
useful_work.execution_ms
wasted_work.completed_total
wasted_work.execution_ms
stale_work.ratio
current_revision.commit_ratio
```

Conceptually:

```text
Useful Work Ratio =
Current Accepted Execution
/
Total Executed Work
```

---

## 39. Queue Metrics

Each bounded queue should expose:

```text
queue.depth
queue.capacity
queue.utilization_ratio
queue.enqueue_total
queue.dequeue_total
queue.rejected_total
queue.replaced_total
queue.dropped_total
queue.obsolete_removed_total
queue.wait_ms
queue.oldest_item_age_ms
```

Dimensions:

```text
queue.name
stage
priority
```

Avoid WorkItemId as a metric dimension.

---

## 40. Scheduler Metrics

Recommended Scheduler metrics:

```text
scheduler.admission_total
scheduler.admission_rejected_total
scheduler.current_revision_admission_ms
scheduler.capacity_wait_ms
scheduler.active_work_count
scheduler.decision_duration_ms
scheduler.priority_preemption_total
```

Scheduler decision duration must remain small.

---

## 41. Cancellation Metrics

Recommended cancellation metrics:

```text
cancellation.requested_total
cancellation.acknowledged_total
cancellation.propagation_ms
cancellation.running_work_total
cancellation.queued_work_total
cancellation.provider_abort_success_total
cancellation.provider_abort_failed_total
cancellation.drain_duration_ms
```

Dimensions may include:

```text
reason
stage
provider
```

Cancellation reason values must remain bounded.

---

## 42. Stale-Result Metrics

Recommended stale metrics:

```text
stale.result_total
stale.error_suppressed_total
stale.execution_ms
stale.provider_cost_estimate
stale.artifact_rejected_total
stale.ui_commit_rejected_total
```

Stale results are not provider failures.

They must not be counted as stage-failure rate unless a separate technical failure occurred.

---

## 43. Retry Metrics

Recommended retry metrics:

```text
retry.scheduled_total
retry.started_total
retry.succeeded_total
retry.failed_total
retry.skipped_total
retry.exhausted_total
retry.canceled_total
retry.delay_ms
retry.recovery_latency_ms
retry.attempt_count
retry.provider_changed_total
```

Skip reasons may include:

```text
REVISION_OBSOLETE
SESSION_CLOSED
BUDGET_EXHAUSTED
ERROR_NOT_RETRYABLE
PROVIDER_UNAVAILABLE
CANCELED
```

---

## 44. Provider Metrics

Recommended provider metrics:

```text
provider.request_total
provider.success_total
provider.failure_total
provider.timeout_total
provider.rate_limited_total
provider.canceled_total
provider.requests_inflight
provider.request_duration_ms
provider.queue_wait_ms
provider.input_size
provider.output_size
provider.health_state
provider.fallback_total
provider.cold_start_ms
```

Dimensions may include:

```text
provider
model
operation
result
```

Provider request IDs must not be metric labels.

---

## 45. Provider Health Signals

Provider health should be derived from:

- recent success rate
- timeout rate
- rate-limit rate
- tail latency
- consecutive failures
- authentication status
- quota status
- initialization state

Possible provider states:

```text
HEALTHY
SLOW
DEGRADED
RATE_LIMITED
UNAVAILABLE
PROBING
```

State transitions should be observable.

---

## 46. Capture Metrics

Recommended capture metrics:

```text
capture.frame_total
capture.frame_failed_total
capture.frame_replaced_total
capture.frame_dropped_total
capture.active_sources
capture.callback_duration_ms
capture.copy_duration_ms
capture.region_pixels
capture.effective_rate
capture.configured_rate
```

The runtime should distinguish capture rate from accepted revision rate.

---

## 47. Observation Metrics

Recommended observation metrics:

```text
observation.processed_frame_total
observation.changed_frame_total
observation.unchanged_frame_total
observation.stability_candidate_total
observation.stability_accepted_total
observation.analysis_duration_ms
observation.pending_frame_replaced_total
observation.fingerprint_duration_ms
```

---

## 48. OCR Metrics

Recommended OCR metrics:

```text
ocr.request_total
ocr.success_total
ocr.empty_total
ocr.low_confidence_total
ocr.failure_total
ocr.queue_wait_ms
ocr.execution_ms
ocr.region_count
ocr.input_pixels
ocr.cache_hit_total
ocr.cache_miss_total
```

Where confidence is provider-specific, normalize carefully before aggregation.

---

## 49. Layout Metrics

Recommended layout metrics:

```text
layout.request_total
layout.success_total
layout.failure_total
layout.fallback_total
layout.execution_ms
layout.region_count
layout.reading_order_count
```

---

## 50. Translation Metrics

Recommended translation metrics:

```text
translation.request_total
translation.success_total
translation.partial_total
translation.failure_total
translation.queue_wait_ms
translation.execution_ms
translation.unit_count
translation.input_length
translation.output_length
translation.batch_size
translation.cache_hit_total
translation.cache_miss_total
translation.provider_cost_estimate
```

Do not record raw text in metric labels.

---

## 51. Presentation Metrics

Recommended presentation metrics:

```text
presentation.build_total
presentation.build_failed_total
presentation.build_duration_ms
presentation.text_block_count
presentation.ui_dispatch_ms
presentation.commit_duration_ms
presentation.commit_rejected_total
presentation.visible_duration_ms
```

---

## 52. UI Responsiveness Metrics

Recommended UI metrics:

```text
ui.command_acknowledgment_ms
ui.dispatch_delay_ms
ui.long_task_total
ui.long_task_duration_ms
ui.frame_stall_total
ui.commit_duration_ms
ui.pending_update_count
```

Framework-specific instrumentation may vary.

The architecture requires that UI blocking remains observable.

---

## 53. Cache Metrics

Recommended cache metrics:

```text
cache.lookup_total
cache.hit_total
cache.miss_total
cache.insert_total
cache.insert_failed_total
cache.eviction_total
cache.corrupt_entry_total
cache.lookup_ms
cache.active_entries
cache.active_bytes
cache.reused_compute_ms
```

Dimensions may include:

```text
artifact_type
retention_class
eviction_reason
```

---

## 54. Artifact Metrics

Recommended artifact metrics:

```text
artifact.created_total
artifact.published_total
artifact.reused_total
artifact.active_count
artifact.active_bytes
artifact.lease_count
artifact.disposal_pending_count
artifact.disposed_total
artifact.lifetime_ms
artifact.disposal_latency_ms
```

Dimensions may include:

```text
artifact_type
storage_class
retention_class
```

---

## 55. Resource Lifecycle Metrics

Recommended resource metrics:

```text
resource.registered_total
resource.transfer_total
resource.lease_acquired_total
resource.lease_released_total
resource.dispose_requested_total
resource.dispose_completed_total
resource.dispose_failed_total
resource.pending_disposal_count
resource.draining_count
```

These metrics help validate `RESOURCE_LIFECYCLE.md`.

---

## 56. Memory Metrics

Recommended memory metrics:

```text
memory.process_bytes
memory.managed_bytes
memory.native_bytes
memory.artifact_bytes
memory.cache_bytes
memory.worker_temporary_bytes
memory.draining_bytes
memory.pressure_level
memory.admission_rejected_total
memory.allocation_failed_total
```

Where exact memory accounting is unavailable, estimated values should be marked as estimates.

---

## 57. GPU Metrics

When local GPU processing exists, measure:

```text
gpu.memory_used_bytes
gpu.memory_reserved_bytes
gpu.inference_inflight
gpu.inference_duration_ms
gpu.model_load_duration_ms
gpu.model_resident_count
gpu.queue_depth
gpu.allocation_failed_total
gpu.ui_contention_total
```

GPU telemetry should remain optional for platforms without supported access.

---

## 58. Worker Metrics

Recommended worker metrics:

```text
worker.pool_size
worker.active_count
worker.idle_count
worker.utilization_ratio
worker.task_started_total
worker.task_completed_total
worker.task_failed_total
worker.shutdown_wait_ms
worker.abandoned_total
```

Logical stage concurrency should be tracked separately from physical thread count.

---

## 59. Runtime Control Metrics

The Runtime Control context must remain responsive.

Track:

```text
runtime.command_queue_depth
runtime.command_processing_ms
runtime.command_delay_ms
runtime.loop_stall_total
runtime.state_transition_total
runtime.invalid_transition_total
runtime.duplicate_terminal_event_total
```

A slow Runtime Control loop may delay:

- cancellation
- scheduler decisions
- commit validation
- shutdown

---

## 60. Event Bus Metrics

Recommended event-bus metrics:

```text
event.published_total
event.delivered_total
event.dropped_total
event.handler_failed_total
event.handler_duration_ms
event.queue_depth
```

Dimensions should use bounded event types.

The event bus must not become a high-cardinality telemetry stream.

---

## 61. Error Metrics

Recommended error metrics:

```text
error.total
error.by_category
error.by_code
error.by_scope
error.transient_total
error.permanent_total
error.user_visible_total
error.suppressed_total
error.deduplicated_total
error.fatal_total
```

Exact implementation may flatten these into one metric with bounded dimensions.

---

## 62. Error Logs

A structured error log should conceptually contain:

```text
timestamp
severity
error.code
error.category
error.scope
error.retry_class
stage
session.id
revision.id
work_item.id
attempt.id
provider.id
message
cause.type
current_revision
result.disposition
```

Sensitive content must be excluded.

---

## 63. Performance Budget Signals

The runtime should emit a budget-violation signal when important limits are exceeded.

Examples:

```text
performance.useful_latency_exceeded
performance.queue_wait_exceeded
performance.provider_latency_exceeded
performance.ui_dispatch_exceeded
performance.memory_budget_exceeded
performance.stale_ratio_exceeded
```

Budget violations may produce:

- metrics
- trace annotations
- rate-limited warning logs
- adaptive degradation input

---

## 64. Performance Budget Context

A violation should include:

```text
budget.name
budget.value
observed.value
stage
provider
revision.status
degradation.level
```

Do not emit one noisy log for every repeated sample.

Use aggregation and rate limiting.

---

## 65. Diagnostic Snapshot Model

A runtime diagnostic snapshot may contain:

```text
RuntimeDiagnosticSnapshot
├── CapturedAt
├── Application
├── Sessions
├── Revisions
├── Queues
├── Workers
├── Providers
├── Artifacts
├── Cache
├── Resources
├── Memory
├── GPU
├── UI
├── RecentErrors
└── RecentTransitions
```

---

## 66. Application Snapshot

Suggested fields:

```text
application.instance_id
application.state
application.uptime
application.version
runtime.mode
shutdown.state
telemetry.mode
```

---

## 67. Session Snapshot

Suggested fields:

```text
session.id
session.state
capture.source_type
capture.active
current_revision.id
current_revision.age
provider.configuration
created_at
last_activity_at
```

Sensitive source titles should be omitted or sanitized.

---

## 68. Revision Snapshot

Suggested fields:

```text
revision.id
revision.state
revision.created_at
revision.is_current
revision.stage
revision.active_work_count
revision.pending_retry_count
revision.artifact_count
revision.last_error_code
revision.commit_status
```

---

## 69. Queue Snapshot

Suggested fields:

```text
queue.name
queue.capacity
queue.depth
queue.oldest_item_age
queue.current_revision_items
queue.obsolete_items
```

The snapshot should not include full WorkItem payloads by default.

---

## 70. Provider Snapshot

Suggested fields:

```text
provider.id
provider.state
provider.model
provider.requests_inflight
provider.consecutive_failures
provider.last_success_at
provider.last_error_code
provider.backoff_until
provider.initialized
```

Credentials must never be included.

---

## 71. Artifact Snapshot

Suggested fields:

```text
artifact.count_by_type
artifact.bytes_by_type
artifact.active_lease_count
artifact.disposal_pending_count
artifact.oldest_pending_disposal_age
artifact.cache_owned_count
artifact.revision_owned_count
```

Individual ArtifactIds may be exposed only in development diagnostics.

---

## 72. Snapshot Collection

Snapshot collection must:

- avoid blocking Runtime Control for a long duration
- avoid acquiring broad locks
- use immutable or copied metadata
- exclude large artifact payloads
- remain safe during shutdown
- tolerate partial data

A snapshot is best-effort diagnostic state, not a transactional database export.

---

## 73. Recent Event Ring Buffer

The development runtime may maintain a bounded in-memory ring buffer of recent lightweight events.

Examples:

```text
revision.created
work.started
work.completed
retry.scheduled
artifact.published
provider.degraded
ui.commit.rejected
```

Benefits:

- local diagnosis without persistent logging
- reconstruction of recent state transitions
- support snapshot enrichment

The buffer must be:

- bounded
- content-free
- removable in production
- safe under concurrency

---

## 74. Structured Logging Principles

Structured logs should:

- use stable event names
- use machine-readable fields
- avoid string parsing for core logic
- preserve correlation identifiers
- avoid large payloads
- avoid high-frequency noise
- support redaction
- support severity filtering

---

## 75. Suggested Log Event Names

Examples:

```text
runtime.started
runtime.shutdown.started
runtime.shutdown.completed
runtime.shutdown.timeout
session.started
session.failed
revision.created
revision.obsolete
revision.failed
work.failed
work.abandoned
retry.exhausted
provider.degraded
provider.recovered
artifact.lifecycle.invalid
resource.cleanup.failed
runtime.invariant_violated
```

---

## 76. Logging Normal Operations

Do not use high-severity logs for:

- expected cancellation
- stale-result rejection
- queue replacement
- cache miss
- normal provider fallback
- session stop

These may use:

- trace
- debug
- metrics
- low-volume informational events

---

## 77. Error Deduplication

Repeated errors must not flood logs or UI.

A possible deduplication key:

```text
ErrorCode
+
Stage
+
ProviderId
+
SessionId
+
Time Window
```

Deduplication should preserve:

- first occurrence
- repeat count
- last occurrence
- escalation if severity changes

---

## 78. Log Rate Limiting

High-frequency events should be rate-limited.

Examples:

- capture frame failure
- repeated provider unavailability
- cache lookup failure
- stale completion
- duplicate callback

Metrics should still count all occurrences when feasible.

---

## 79. Cardinality

Telemetry cardinality must remain bounded.

Unsafe metric labels include:

- SessionId
- RevisionId
- WorkItemId
- AttemptId
- ArtifactId
- provider request ID
- raw error message
- source text
- file path
- window title

These belong in traces, logs, or local diagnostics.

---

## 80. Safe Metric Dimensions

Generally safe bounded dimensions include:

```text
stage
result_status
error_code
provider
model
execution_class
cache_status
cancellation_reason
resource_type
memory_pressure_level
device_profile
```

Even these should be reviewed for bounded value sets.

---

## 81. Privacy Principles

CRAI processes private reading content.

Observability must follow:

```text
No Content by Default
```

Standard telemetry must not include:

- screenshots
- OCR text
- translation text
- prompts
- source URLs
- page titles
- selected window titles
- clipboard content
- API credentials
- tokens
- raw provider request bodies

---

## 82. Content-Derived Metadata

Some content-derived metadata may be useful.

Examples:

- image dimensions
- region count
- text length
- language code
- confidence range
- hash version

Such metadata should be:

- non-reversible where practical
- coarse when exact values are unnecessary
- disabled if privacy risk outweighs diagnostic value

---

## 83. Hashes and Fingerprints

Content hashes and perceptual fingerprints may still be identifying.

They should:

- not be exported by default
- not be used as public metric labels
- remain local unless explicitly required
- be truncated or anonymized for diagnostics
- use versioned algorithms

---

## 84. Credentials and Secrets

The observability layer must redact:

```text
Authorization
API keys
Access tokens
Refresh tokens
Cookies
Provider secrets
Encryption keys
```

Redaction must occur before storage or export.

Do not rely solely on UI hiding.

---

## 85. Technical Messages

Raw provider or native error messages may contain sensitive data.

Before logging:

```text
Raw Error
    ↓
Normalize
    ↓
Sanitize
    ↓
Structured Log
```

The original raw error may remain in memory briefly for local debugging only where safe.

---

## 86. Diagnostic Consent

Detailed content diagnostics, if ever supported, must require explicit user action.

Possible modes:

```text
STANDARD
ENHANCED_DIAGNOSTICS
CONTENT_DEBUG
```

For the MVP, `CONTENT_DEBUG` should remain disabled or development-only.

---

## 87. Telemetry Modes

Suggested conceptual modes:

```text
OFF
LOCAL_ONLY
ANONYMOUS_OPERATIONAL
DEVELOPMENT_VERBOSE
```

### OFF

Only essential in-memory runtime state.

### LOCAL_ONLY

Logs and snapshots remain on device.

### ANONYMOUS_OPERATIONAL

Bounded non-content metrics may be exported.

### DEVELOPMENT_VERBOSE

Additional traces and lifecycle diagnostics enabled.

Exact product policy will be decided later.

---

## 88. Local Diagnostics

The MVP should prioritize local diagnostics.

Local diagnostics may include:

- recent trace history
- metrics snapshot
- runtime state snapshot
- sanitized error logs
- provider health
- queue status
- memory status

This avoids requiring production telemetry infrastructure during early development.

---

## 89. Production Telemetry

Future production telemetry should prefer:

- aggregated metrics
- sampled traces
- sanitized errors
- explicit user consent where required
- bounded retention
- no reading content

Production telemetry must never become a hidden content-collection mechanism.

---

## 90. Sampling Strategy

Not all traces need equal retention.

Always retain or strongly sample:

- fatal errors
- invariant violations
- application shutdown failures
- revision failures
- provider authentication failures
- memory critical events
- excessive useful-result latency

Sample more aggressively:

- normal successful revisions
- cache hits
- unchanged observation frames
- routine cancellation
- normal queue activity

---

## 91. Tail-Based Sampling

Future systems may retain traces based on the final outcome.

Examples:

```text
Keep trace if:
- latency exceeds threshold
- revision fails
- retry occurs
- stale execution exceeds threshold
- provider changes
- cleanup fails
```

This provides more useful data than random-only sampling.

---

## 92. Telemetry Overhead

Observability must not become a performance bottleneck.

It should avoid:

- synchronous disk writes on critical path
- blocking network export
- serializing large objects
- excessive allocation
- unbounded log queues
- full tracing of every frame
- expensive stack capture for expected outcomes

Telemetry processing should use bounded asynchronous delivery where applicable.

---

## 93. Telemetry Backpressure

Telemetry queues must also be bounded.

When full, preferred behavior:

1. preserve fatal and critical events
2. preserve important errors
3. preserve aggregate counters
4. drop verbose trace details
5. drop repeated informational logs

Telemetry overload must not block the runtime pipeline.

---

## 94. Observability Failure

Telemetry failure must not break runtime correctness.

Examples:

- log write failed
- metrics exporter unavailable
- trace buffer full
- snapshot serialization failed

Preferred behavior:

```text
Record local bounded diagnostic if possible
    ↓
Continue runtime
```

Observability subsystem failure may be visible as a warning but should not terminate the session.

---

## 95. Development Diagnostic View

A development-only diagnostic view should eventually expose:

```text
Current Session
Current Revision
Pipeline Stage
Queue Depths
Running Attempts
Provider Health
Useful Result Latency
Cache Hits
Artifact Count
Memory Usage
Recent Errors
Recent Runtime Events
```

The view should use metadata only.

---

## 96. Revision Timeline View

A useful development visualization may show:

```text
Revision 210
├── Observation     35 ms
├── Queue Wait      10 ms
├── OCR             420 ms
├── Layout          28 ms
├── Translation     680 ms
├── Presentation    42 ms
└── UI Commit       16 ms
```

For failed or stale revisions:

```text
Revision 211
├── OCR             500 ms
├── Translation     900 ms
└── STALE
```

This directly exposes wasted work.

---

## 97. Queue Diagnostic View

A queue diagnostic view should show:

```text
Queue
Capacity
Depth
Oldest Age
Current Revision Items
Obsolete Items
Running Count
Rejected Count
```

This helps distinguish provider slowness from admission problems.

---

## 98. Resource Diagnostic View

A resource diagnostic view should show:

```text
Artifact Type
Active Count
Active Bytes
Revision Owners
Cache Owners
Worker Leases
Pending Disposal
Oldest Pending Disposal
```

This helps identify ownership leaks.

---

## 99. Provider Diagnostic View

A provider diagnostic view should show:

```text
Provider
Model
State
In-Flight Requests
Recent P50/P95 Latency
Recent Failure Rate
Rate-Limit Status
Backoff State
Last Error
```

Credentials and raw responses must remain hidden.

---

## 100. Observability and State Machine

Runtime state transitions should emit traceable events.

Example:

```text
SESSION_IDLE
    ↓ StartSession
SESSION_STARTING
    ↓ CaptureReady
SESSION_ACTIVE
```

For each transition, observability may record:

```text
entity
previous_state
new_state
trigger
result
duration
```

Invalid transitions should generate elevated diagnostics.

---

## 101. Observability and Scheduler

Scheduler decisions should be explainable.

Important decisions include:

- WorkItem admitted
- WorkItem deferred
- WorkItem rejected
- obsolete work removed
- retry admitted
- provider capacity unavailable
- memory admission denied
- current revision prioritized

Not every decision requires a log.

Metrics and trace annotations are usually sufficient.

---

## 102. Observability and Cancellation

A cancellation trace should answer:

```text
Who requested cancellation?
Why?
When?
Which work was queued?
Which work was running?
Did provider abort succeed?
How long until resources drained?
```

Suggested cancellation reasons:

```text
REVISION_REPLACED
SESSION_STOPPED
APPLICATION_SHUTDOWN
USER_REQUEST
PROVIDER_CHANGED
MEMORY_PRESSURE
RETRY_SUPERSEDED
```

The reason list must remain bounded.

---

## 103. Observability and Retry

A retry trace should connect attempts.

Example:

```text
Attempt 1
    ↓ failed: PROVIDER_TIMEOUT
Retry scheduled: 500 ms
    ↓
Attempt 2
    ↓ succeeded
```

Trace attributes should include:

```text
attempt.number
retry.reason
retry.delay_ms
retry.budget_remaining
provider.changed
```

---

## 104. Observability and Stale Results

Stale work should remain visible to developers but quiet for users.

A stale completion should record:

```text
original_revision
current_revision_at_completion
stage
attempt
execution_duration
provider_cost_estimate
rejection_reason
```

This information helps optimize cancellation and stability detection.

---

## 105. Observability and Cache

Cache observability should explain:

- why a lookup hit or missed
- which artifact type was reused
- how much work was avoided
- why an entry was evicted
- whether validation rejected an entry
- whether a cache failure degraded to pipeline execution

Avoid exposing full cache keys.

---

## 106. Observability and Resource Lifecycle

Resource diagnostics should verify:

```text
Create
    ↓
Register
    ↓
Publish
    ↓
Acquire Lease
    ↓
Release Lease
    ↓
Dispose
```

Anomalies include:

- lease never released
- disposal pending too long
- owner removed but bytes retained
- duplicate ownership transfer
- artifact disposed while leased
- resource registered but never published
- provider handle retained after shutdown

---

## 107. Observability and Shutdown

Shutdown observability should track:

```text
shutdown.requested_at
new_work_stopped_at
sessions_canceled_at
workers_drained_at
providers_unloaded_at
resources_disposed_at
shutdown.completed_at
```

Also track:

```text
shutdown.abandoned_work_count
shutdown.cleanup_failure_count
shutdown.duration_ms
```

---

## 108. Startup Observability

Startup should track:

```text
runtime.initialize_ms
capture.initialize_ms
provider.initialize_ms
cache.initialize_ms
model.load_ms
ui.ready_ms
first_revision_latency_ms
```

Cold-start metrics must be distinguished from steady state.

---

## 109. Health Model

The runtime may expose a summarized health state.

Possible states:

```text
HEALTHY
DEGRADED
PARTIALLY_UNAVAILABLE
CRITICAL
SHUTTING_DOWN
```

Health may be derived from:

- session capability
- provider availability
- memory pressure
- queue saturation
- control-loop responsiveness
- fatal errors

Health is a summary, not a replacement for detailed metrics.

---

## 110. Health Snapshot

Suggested health structure:

```text
RuntimeHealth
├── Overall
├── Capture
├── OCR
├── Translation
├── Provider
├── Memory
├── Scheduler
├── UI
└── Observability
```

A degraded telemetry exporter should not necessarily make the overall runtime unhealthy.

---

## 111. Alerts

During development or future production operation, useful alerts may include:

- fatal runtime error
- sustained memory growth
- sustained queue saturation
- provider failure rate above threshold
- useful-result latency above threshold
- stale-work ratio above threshold
- retry exhaustion rate above threshold
- Runtime Control loop stall
- pending resource disposal too old
- application shutdown timeout

Threshold values belong in `RUNTIME_CONFIG.md`.

---

## 112. Alert Stability

Alerts must avoid flapping.

Possible techniques:

- minimum duration
- moving window
- repeated sample threshold
- recovery threshold separate from trigger threshold
- deduplication
- cooldown period

---

## 113. Observability Configuration

Configurable observability behavior may include:

```text
telemetry.mode
metrics.enabled
tracing.enabled
trace.sample_rate
slow_revision_threshold
slow_stage_threshold
log.level
log.retention
diagnostic_ring_buffer_size
snapshot.enabled
content_logging.enabled
```

The exact configuration schema belongs in `RUNTIME_CONFIG.md`.

---

## 114. Observability Defaults

Recommended MVP defaults:

```text
Local metrics: enabled
Structured error logs: enabled
Revision traces: enabled in development
Full frame tracing: disabled
Content logging: disabled
Diagnostic snapshots: enabled
Recent event ring buffer: enabled and bounded
Remote export: disabled
```

---

## 115. Testing Observability

Observability must be tested like runtime behavior.

Tests should verify:

- identifiers propagate correctly
- one revision produces one root trace
- retry attempts link correctly
- stale results receive correct disposition
- cancellation is not counted as failure
- queue wait and execution are separated
- sensitive content is redacted
- metric dimensions remain bounded
- telemetry queue remains bounded
- telemetry failure does not break runtime
- snapshot collection is non-blocking
- shutdown telemetry completes safely

---

## 116. Deterministic Telemetry Tests

Fake clocks and fake providers should allow deterministic verification.

Example:

```text
Queue delay = 100 ms
Execution delay = 400 ms
Provider delay = 300 ms
UI commit delay = 20 ms
```

Tests should assert that telemetry reports the correct segments.

---

## 117. Privacy Tests

Automated tests should scan logs and telemetry for forbidden values.

Examples:

- API key
- authorization header
- OCR text
- translation text
- source image path
- raw screenshot bytes
- provider prompt

Redaction should be verified before export.

---

## 118. Cardinality Tests

Tests should verify that metrics do not create labels from:

- SessionId
- RevisionId
- WorkItemId
- AttemptId
- ArtifactId
- arbitrary error messages

Metrics should use stable bounded enums.

---

## 119. Failure Tests

Observability-specific failures to test:

- log sink unavailable
- metrics queue full
- trace exporter unavailable
- diagnostic snapshot failure
- event subscriber exception
- disk full
- invalid telemetry configuration
- shutdown while telemetry is flushing

Runtime correctness must remain intact.

---

## 120. Long-Session Observability Tests

Long-session testing should verify:

- diagnostic ring buffers remain bounded
- trace buffers remain bounded
- metric structures do not grow by identifier
- logs do not grow without retention
- telemetry threads remain bounded
- snapshot latency remains stable
- resource metrics match real disposal behavior

---

## 121. MVP Observability Model

The MVP should implement:

```text
Runtime Observability
├── In-Memory Metrics
├── Structured Local Logs
├── Revision Trace Timeline
├── Bounded Recent Event Buffer
└── Runtime Diagnostic Snapshot
```

Remote telemetry export is not required initially.

---

## 122. MVP Required Correlation

The MVP must propagate:

```text
SessionId
RevisionId
WorkItemId
AttemptId
Stage
```

Provider work must additionally carry:

```text
ProviderId
ProviderRequestId
```

Artifact events must carry:

```text
ArtifactId
ArtifactType
```

---

## 123. MVP Required Metrics

At minimum:

```text
revision.created_total
revision.committed_total
revision.failed_total
revision.obsolete_total
revision.useful_latency_ms

work.started_total
work.failed_total
work.canceled_total
work.stale_total

queue.depth
queue.wait_ms

provider.request_duration_ms
provider.failure_total
provider.requests_inflight

retry.started_total
retry.exhausted_total

cache.hit_total
cache.miss_total

artifact.active_count
artifact.active_bytes
artifact.pending_disposal_count

memory.process_bytes

ui.commit_duration_ms
```

---

## 124. MVP Required Logs

At minimum, structured logs should exist for:

```text
runtime startup failure
session startup failure
revision terminal failure
provider initialization failure
provider authentication failure
retry exhaustion
resource cleanup failure
invalid state transition
fatal invariant violation
shutdown timeout
```

---

## 125. MVP Required Trace

One accepted revision should be traceable through:

```text
Observation
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

Retries and stale outcomes must appear in the same logical trace.

---

## 126. MVP Diagnostic Snapshot

The MVP snapshot should include:

```text
Application State
Session State
Current Revision
Queue Depths
Active WorkItems
Provider State
Cache Usage
Artifact Counts
Memory Usage
Recent Errors
Recent Runtime Events
```

---

## 127. Observability Invariants

The runtime must preserve these invariants:

1. Every accepted revision has a traceable lifecycle.
2. Every WorkItem outcome can be correlated with its revision.
3. Every retry has a distinct AttemptId.
4. Queue wait and execution duration remain distinguishable.
5. Cancellation is not counted as failure.
6. Stale results are observable but do not become user-visible errors.
7. Raw reading content is excluded from standard telemetry.
8. Credentials and tokens are always redacted.
9. Metric cardinality remains bounded.
10. Telemetry failure cannot break runtime correctness.
11. Telemetry queues remain bounded.
12. Runtime Control is never blocked by telemetry export.
13. Diagnostic snapshots exclude large artifact payloads.
14. Artifact lifecycle anomalies are observable.
15. Provider health changes are observable.
16. Fatal errors receive elevated diagnostics.
17. Duplicate events do not create duplicate terminal outcomes.
18. Production telemetry is content-free by default.
19. Trace sampling preserves important failures and slow operations.
20. Observability overhead remains bounded and measurable.

---

## 128. Open Questions

The following questions remain open:

- Which telemetry library will be used?
- Will OpenTelemetry be adopted?
- Should local traces persist across application restarts?
- How long should local logs be retained?
- Should diagnostic snapshots be exportable as a support package?
- Should the user be able to disable all telemetry?
- Which metrics should be visible in the normal UI?
- Which metrics should remain development-only?
- What slow-revision threshold should trigger detailed trace retention?
- Should provider request cost estimates be stored?
- Should resource lifecycle events be traced individually or sampled?
- How should GPU telemetry be implemented across platforms?
- Should application crashes produce a local recovery report?
- Should anonymous production metrics be supported in the MVP?
- What level of technical detail should a support bundle contain?
- Should source application metadata ever be collected with consent?

These questions can be decided after the desktop framework, provider strategy, and deployment model are selected.

---

## 129. Related Documents

- `README.md`
- `PIPELINE_RUNTIME.md`
- `WORK_QUEUE.md`
- `SCHEDULER.md`
- `CANCELLATION.md`
- `CACHE_POLICY.md`
- `MEMORY_MODEL.md`
- `THREADING_MODEL.md`
- `RESOURCE_LIFECYCLE.md`
- `PERFORMANCE_MODEL.md`
- `ERROR_MODEL.md`
- `RETRY_POLICY.md`
- `RUNTIME_CONFIG.md`
- `RUNTIME_COMPONENTS.md`
- `../STATE_MACHINE.md`
- `../EVENT_BUS.md`
- `../DATA_FLOW.md`
- `../flows/SCREEN_COMIC_FLOW.md`

---

## 130. Next Step

The next runtime document should be:

```text
RUNTIME_CONFIG.md
```

It should define:

- configuration ownership
- configuration scopes
- application defaults
- session configuration
- provider configuration
- pipeline configuration
- capture configuration
- stability thresholds
- queue capacities
- concurrency limits
- timeout configuration
- retry configuration
- cache and memory budgets
- observability configuration
- validation
- hot reload
- configuration snapshots
- configuration versioning
- secret handling
- safe defaults
- MVP configuration policy

After that, the Runtime section should be concluded with:

```text
RUNTIME_COMPONENTS.md
```

This final document should consolidate the logical runtime components and determine whether concepts such as:

```text
Resource Manager
Attempt
Provider Manager
Runtime Store
```

need standalone implementation modules or remain architectural responsibilities.

---

## 131. Summary

CRAI observability is built from:

```text
Metrics
    +
Traces
    +
Structured Logs
    +
Runtime Events
    +
Diagnostic Snapshots
```

The main correlation model is:

```text
ApplicationInstanceId
    ↓
SessionId
        ↓
RevisionId
            ↓
WorkItemId
                ↓
AttemptId
```

Observability must explain:

```text
What happened?
Why did it happen?
How long did it take?
Was the work still relevant?
What resource did it consume?
What final authority did it have?
```

The primary observable outcome is not merely stage completion.

It is:

```text
Current Valid Revision
    ↓
Useful Translation Produced
    ↓
Presentation Successfully Committed
```

At the same time, observability must preserve privacy:

```text
No Screenshots
No OCR Text
No Translation Text
No Prompts
No Credentials
```

unless an explicit development-only diagnostic mode has been enabled.

The MVP should begin with local, bounded, content-free observability and add remote telemetry only after the runtime behavior and privacy model are stable.