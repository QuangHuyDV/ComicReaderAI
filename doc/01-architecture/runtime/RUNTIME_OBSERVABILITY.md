# runtime/RUNTIME_OBSERVABILITY.md

# Runtime Observability

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI quan sát, đo lường, chẩn đoán và giải thích hành vi của Runtime.

Observability phải trả lời được:

- WorkItem nào đã được tạo và vì sao?
- Attempt nào thực sự chạy?
- Work chậm ở queue, provider, lease hay publication?
- Completion còn authority hay đã stale?
- Candidate Artifact có được chấp nhận không?
- Ownership transfer có thành công không?
- Artifact đã publish nhưng có commit không?
- Resource nào còn retention hoặc active lease?
- Cancellation đã revoke authority và drain đến đâu?
- Retry có tạo recovery hữu ích không?
- Runtime đang pressure hay overload ở đâu?
- UI bị chậm vì Runtime, dispatcher hay rendering?
- Shutdown đang bị block bởi resource nào?

Observability phải giải thích **runtime decision**, không chỉ ghi lại implementation activity.

---

## 2. Architectural Position

```text
Runtime Operation
    ↓
Metrics / Trace / Event / Structured Log
    ↓
Bounded Observability Pipeline
    ↓
Local Diagnostics
    ↓
Optional Sanitized Export
```

Observability không:

- sở hữu Runtime state;
- thay Event Bus;
- quyết định retry;
- cấp authority;
- mutate Artifact;
- block Runtime Control;
- chứa reading content mặc định.

---

## 3. Core Philosophy

```text
Record what happened
+
Explain why it happened
+
Show whether it still mattered
```

Một log như:

```text
Translation completed
```

không đủ.

Observability cần trả lời:

```text
For which Session?
For which Revision?
For which WorkItem?
For which Attempt?
Which WorkType?
Which Provider?
Was authority valid?
Was Candidate accepted?
Was ownership transferred?
Was Artifact published?
Was Presentation committed?
Was the result stale or canceled?
How much time was spent waiting, executing and draining?
```

---

## 4. Observability Signals

```text
Runtime Observability
├── Metrics
├── Traces
├── Structured Logs
├── Runtime Events
└── Diagnostic Snapshots
```

### Metrics

Aggregate count, ratio, latency và resource state.

### Traces

Causal lifecycle của Revision, WorkItem, Attempt, provider request và publication.

### Structured Logs

Abnormal event, failure, invariant hoặc decision explanation.

### Runtime Events

Architectural messages; không tự động là telemetry.

### Diagnostic Snapshots

Best-effort current-state inspection.

---

## 5. Signal Selection

Dùng Metrics cho:

- counts;
- rates;
- durations;
- percentiles;
- ratios;
- capacity;
- pressure.

Dùng Trace cho:

- causal flow;
- Attempt lineage;
- authority validation;
- ownership transfer;
- publication;
- retry/fallback;
- UI commit.

Dùng Log cho:

- error;
- invariant violation;
- rejected ownership transfer;
- cleanup failure;
- shutdown anomaly;
- unexpected decision.

Dùng Snapshot cho:

- queue;
- authority;
- provider;
- Artifact;
- retention;
- Lease;
- resource pressure;
- recent failures.

---

## 6. Correlation Model

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

Additional identifiers:

```text
BusinessExecutionPlanId
BusinessStageId
ArtifactId
CandidateArtifactId
ResourceId
LeaseId
ProviderRequestId
PresentationId
TraceId
SpanId
CaptureSourceId
```

Các identifier không được dùng thay thế lẫn nhau.

---

## 7. Correlation Requirements

Mọi Attempt Completion phải có tối thiểu:

```text
SessionId
RevisionId
WorkItemId
AttemptId
OwnerModule
BusinessStageId
WorkType
```

Provider execution thêm:

```text
ProviderId
ProviderProfile
ProviderRequestId
ModelId
```

Artifact lifecycle thêm:

```text
ArtifactId hoặc CandidateArtifactId
ArtifactType
ResourceId
```

Lifecycle operation có thể thêm:

```text
LeaseId
RetentionClass
Owner
```

---

## 8. Correlation Propagation

```text
Runtime Command
    ↓
Runtime Control
    ↓
BusinessExecutionPlan
    ↓
WorkItem
    ↓
Scheduler
    ↓
Work Queue
    ↓
Worker / Provider Adapter
    ↓
Completion
    ↓
Authority Validation
    ↓
Ownership Transfer
    ↓
Artifact Publication
    ↓
Presentation Commit
```

Correlation context phải lightweight và content-free.

---

## 9. Revision Root Trace

Mỗi accepted Revision nên có một root trace:

```text
REVISION
├── Observation
├── Revision Creation
├── Business Planning
├── Reuse Evaluation
├── WorkItems
│   └── Attempts
├── Authority Validation
├── Candidate Validation
├── Ownership Transfer
├── Artifact Publication
├── Presentation
└── UI Commit
```

Optional branches:

- cancellation;
- retry;
- provider fallback;
- cache reuse;
- stale rejection;
- resource wait;
- degradation;
- cleanup.

---

## 10. WorkItem and Attempt Trace

`WorkItemId` định danh logical work.

`AttemptId` định danh physical execution.

```text
WorkItem W1
├── Attempt A1: timeout
├── Attempt A2: provider fallback
└── Attempt A3: accepted
```

Trace phải phân biệt:

- first Attempt;
- automatic retry;
- provider fallback;
- manual re-execution;
- abandoned Attempt;
- late Completion.

Retry giữ same WorkItemId và tạo new AttemptId.

---

## 11. Attempt Span

Suggested span:

```text
runtime.attempt
```

Attributes:

```text
owner.module
business_stage.id
work.type
attempt.number
execution.class
queue.class
queue.wait_ms
resource.wait_ms
lease.wait_ms
execution.duration_ms
provider.id
provider.profile
result.status
authority.state
cancellation.requested
error.code
```

Không dùng raw input/output.

---

## 12. Authority Trace

Authority phải observable.

Possible spans/events:

```text
AUTHORITY_VALIDATION_STARTED
AUTHORITY_ACCEPTED
AUTHORITY_REJECTED
AUTHORITY_REVOKED
COMMIT_AUTHORITY_REJECTED
LATE_COMPLETION_REJECTED
```

Attributes:

```text
authority.scope
authority.reason
authority.current_revision
authority.validation_ms
authority.requested_attempt
authority.accepted_attempt
```

---

## 13. Candidate and Publication Trace

```text
Candidate Created
    ↓
Candidate Registered
    ↓
Semantic / Integrity Validation
    ↓
Authority Validation
    ↓
Ownership Transfer
    ↓
Artifact Published
```

Metrics/spans phải phân biệt:

- candidate creation;
- candidate rejection;
- transfer failure;
- duplicate publication;
- successful publication;
- candidate cleanup.

---

## 14. Ownership Trace

Lifecycle events:

```text
OWNERSHIP_TRANSFER_REQUESTED
OWNERSHIP_TRANSFER_ACCEPTED
OWNERSHIP_TRANSFER_REJECTED
OWNERSHIP_RELEASED
```

Attributes:

```text
resource.id
resource.type
previous_owner
new_owner
transfer.reason
transfer.duration_ms
```

Payload ownership khác retention ownership.

---

## 15. Resource Lease Trace

Events:

```text
LEASE_ACQUIRED
LEASE_RELEASED
LEASE_DENIED
LEASE_WAIT_STARTED
LEASE_WAIT_COMPLETED
LEASE_LEAK_DETECTED
```

Attributes:

```text
lease.id
resource.id
resource.type
lease.owner
lease.wait_ms
lease.hold_ms
lease.result
```

Lease identifier dùng trong trace/log, không dùng metric label.

---

## 16. Retention Observability

Retention events:

```text
RETENTION_ADDED
RETENTION_REMOVED
RETENTION_EXPIRED
RETENTION_EVICTED
```

Attributes:

```text
resource.type
retention.class
retention.owner_type
retention.reason
size.estimate
```

Cache retention không được mô tả như payload ownership.

---

## 17. Resource Lifecycle Trace

Canonical lifecycle:

```text
RESOURCE_CREATED
    ↓
RESOURCE_REGISTERED
    ↓
OWNERSHIP_TRANSFERRED
    ↓
RESOURCE_PUBLISHED
    ↓
RETENTION_ADDED / LEASE_ACQUIRED
    ↓
RESOURCE_LOGICALLY_DISPOSED
    ↓
RESOURCE_DRAINING
    ↓
RESOURCE_PHYSICALLY_DISPOSED
```

Anomalies:

- resource registered nhưng không cleanup;
- candidate never accepted/released;
- Lease never released;
- duplicate ownership transfer;
- dispose while leased;
- draining quá lâu;
- resource resurrection;
- provider handle retained after shutdown.

---

## 18. Provider Trace

Suggested span:

```text
provider.request
```

Attributes:

```text
provider.id
provider.profile
provider.model
provider.operation
provider.request_size_class
provider.timeout_ms
provider.result
provider.http_status
provider.error_code
provider.request_id
provider.queue_wait_ms
provider.duration_ms
provider.abandoned
provider.abort_supported
```

Không attach prompt, source text hoặc raw response.

---

## 19. Cache and Reuse Trace

Suggested spans:

```text
cache.reuse_lookup
cache.candidate_validation
cache.promotion
cache.eviction
```

Attributes:

```text
artifact.type
owner.module
cache.scope
cache.result
cache.reject_reason
cache.lookup_ms
cache.validation_ms
cache.retention_class
cache.eviction_reason
privacy.partition
```

Raw cache key và fingerprint không export mặc định.

---

## 20. Queue Trace

Queue wait phải tách execution.

```text
WorkItem Eligible
    ↓
Scheduler Decision
    ↓
Queued
    ↓
Selected
    ↓
Dispatched
```

Có thể dùng span hoặc timestamps.

Metrics phải phân biệt logical queue class:

```text
CONTROL
INTERACTIVE
BACKGROUND
MAINTENANCE
```

---

## 21. Scheduler Observability

Scheduler decision phải explainable:

```text
ADMIT
DEFER
REJECT
REPLACE
```

Attributes:

```text
decision
reason
priority_class
work.type
queue.class
resource.pressure
capacity.available
current_revision
decision.duration_ms
```

Không log mọi decision nếu volume cao; dùng metric + sampled trace.

---

## 22. Cancellation Trace

Cancellation trace trả lời:

- ai yêu cầu;
- scope nào;
- reason;
- authority revoke lúc nào;
- queued work nào removed;
- Attempt nào signaled;
- provider abort có thành công không;
- Attempt nào abandoned;
- resource drain bao lâu;
- late Completion nào bị reject.

Canonical milestones:

```text
CANCELLATION_REQUESTED
AUTHORITY_REVOKED
QUEUED_WORK_REMOVED
ATTEMPT_SIGNALED
CANCELLATION_ACKNOWLEDGED
ATTEMPT_ABANDONED
RESOURCE_DRAIN_COMPLETED
```

---

## 23. Retry Trace

```text
Attempt 1
    ↓ failed
Retry Evaluated
    ↓ delayed / fallback / skipped
Attempt 2
```

Attributes:

```text
retry.decision
retry.strategy
retry.reason
retry.delay_ms
retry.budget_remaining
retry.provider_changed
retry.cache_satisfied
```

Retry trace phải giữ Attempt lineage.

---

## 24. Stale and Late Work

Stale work vẫn visible cho developer nhưng quiet cho user.

Record:

```text
original_revision
current_revision
work.type
attempt.number
execution.duration
provider.cost_estimate
authority.reject_reason
candidate_created
publication_attempted
```

Stale result không được tính là provider failure nếu technical execution thành công.

---

## 25. Metrics Naming

Conceptual prefix:

```text
crai.runtime.<domain>.<measurement>
```

Examples:

```text
crai.runtime.revision.created_total
crai.runtime.workitem.active_count
crai.runtime.attempt.duration_ms
crai.runtime.authority.validation_ms
crai.runtime.publication.duration_ms
crai.runtime.lease.hold_ms
crai.runtime.resource.draining_count
crai.runtime.ui.commit_ms
```

Exact backend naming có thể khác, semantics phải stable.

---

## 26. Metric Design Rules

Metrics phải:

- bounded cardinality;
- stable name;
- low overhead;
- separate logical và physical work;
- separate current và stale;
- separate queue wait và execution;
- separate accepted và rejected;
- separate logical và physical disposal;
- avoid raw identifiers.

---

## 27. Safe Metric Dimensions

Preferred dimensions:

```text
owner_module
work_type
business_stage_class
execution_class
provider
provider_profile
result_status
terminal_outcome
authority_result
cache_status
queue_class
cancellation_reason
resource_type
retention_class
pressure_level
device_profile
```

Không dùng:

- SessionId;
- RevisionId;
- WorkItemId;
- AttemptId;
- ArtifactId;
- ResourceId;
- ProviderRequestId;
- raw error message;
- content hash.

---

## 28. Revision Metrics

```text
revision.created_total
revision.current_total
revision.superseded_total
revision.committed_total
revision.failed_total
revision.canceled_total
revision.active_count
revision.lifetime_ms
revision.useful_latency_ms
revision.first_useful_result_ms
revision.commit_ratio
revision.churn_rate
```

---

## 29. WorkItem Metrics

```text
workitem.created_total
workitem.eligible_total
workitem.admitted_total
workitem.rejected_total
workitem.replaced_total
workitem.active_count
workitem.accepted_total
workitem.failed_total
workitem.canceled_total
workitem.stale_total
workitem.abandoned_total
```

WorkItem metric không trộn Attempt count.

---

## 30. Attempt Metrics

```text
attempt.started_total
attempt.completed_total
attempt.failed_total
attempt.canceled_total
attempt.abandoned_total
attempt.late_total
attempt.duration_ms
attempt.count_per_workitem
attempt.resource_wait_ms
attempt.lease_wait_ms
```

---

## 31. Authority Metrics

```text
authority.validation_total
authority.accepted_total
authority.rejected_total
authority.revoked_total
authority.validation_ms
authority.late_completion_rejected_total
authority.commit_rejected_total
```

Dimensions reason phải bounded.

---

## 32. Publication Metrics

```text
candidate.created_total
candidate.rejected_total
candidate.cleanup_failed_total
ownership.transfer_total
ownership.transfer_failed_total
ownership.transfer_ms
artifact.published_total
artifact.publication_failed_total
artifact.publication_ms
artifact.duplicate_publication_total
```

---

## 33. Queue Metrics

```text
queue.depth
queue.capacity
queue.utilization_ratio
queue.enqueue_total
queue.dispatch_total
queue.replaced_total
queue.removed_total
queue.rejected_total
queue.wait_ms
queue.oldest_item_age_ms
queue.saturation_duration_ms
```

---

## 34. Scheduler Metrics

```text
scheduler.decision_total
scheduler.admit_total
scheduler.defer_total
scheduler.reject_total
scheduler.replace_total
scheduler.decision_ms
scheduler.current_revision_admission_ms
scheduler.control_capacity_available
```

---

## 35. Cancellation Metrics

```text
cancellation.requested_total
cancellation.authority_revoke_ms
cancellation.queued_remove_ms
cancellation.acknowledged_total
cancellation.worker_ack_ms
cancellation.provider_abort_success_total
cancellation.provider_abort_failed_total
cancellation.abandoned_total
cancellation.drain_ms
cancellation.late_result_total
```

---

## 36. Retry Metrics

```text
retry.evaluated_total
retry.approved_total
retry.skipped_total
retry.delayed_total
retry.admitted_total
retry.exhausted_total
retry.canceled_total
retry.delay_ms
retry.recovery_ms
retry.provider_fallback_total
retry.concurrent_count
```

---

## 37. Provider Metrics

```text
provider.request_total
provider.success_total
provider.failure_total
provider.timeout_total
provider.rate_limited_total
provider.canceled_total
provider.abandoned_total
provider.requests_inflight
provider.request_ms
provider.queue_wait_ms
provider.cold_start_ms
provider.cost_estimate
provider.health_state
```

---

## 38. Cache Metrics

```text
cache.lookup_total
cache.hit_total
cache.miss_total
cache.validation_rejected_total
cache.compatibility_miss_total
cache.integrity_failure_total
cache.privacy_partition_miss_total
cache.promotion_total
cache.promotion_skipped_total
cache.eviction_total
cache.retained_bytes
cache.lookup_ms
cache.saved_execution_ms
cache.saved_provider_cost
cache.inflight_coalesced_total
```

---

## 39. Resource and Lease Metrics

```text
resource.created_total
resource.registered_total
resource.logical_disposed_total
resource.physical_disposed_total
resource.disposal_failed_total
resource.draining_count
resource.draining_ms
resource.leak_detected_total

lease.acquired_total
lease.released_total
lease.denied_total
lease.active_count
lease.wait_ms
lease.hold_ms
lease.leak_total

retention.added_total
retention.removed_total
retention.active_count
retention.active_bytes
```

---

## 40. Memory and Pressure Metrics

```text
memory.process_bytes
memory.managed_bytes
memory.native_bytes
memory.artifact_bytes
memory.cache_bytes
memory.attempt_local_bytes
memory.draining_bytes
gpu.memory_used_bytes
native.handle_count
resource.pressure_level
resource.admission_rejected_total
```

Estimated values phải đánh dấu estimate trong metadata.

---

## 41. Runtime Control Metrics

```text
runtime.command_queue_depth
runtime.command_processing_ms
runtime.command_delay_ms
runtime.loop_stall_total
runtime.state_transition_total
runtime.invalid_transition_total
runtime.duplicate_completion_total
runtime.authority_validation_ms
runtime.publication_coordination_ms
```

Runtime Control loop chậm sẽ làm delay cancellation, admission, publication và commit.

---

## 42. UI Metrics

```text
ui.command_ack_ms
ui.dispatch_delay_ms
ui.long_task_total
ui.long_task_ms
ui.frame_stall_total
ui.commit_ms
ui.commit_rejected_total
ui.pending_update_count
ui.authority_revalidation_ms
```

---

## 43. Error Metrics and Logs

Error metric:

```text
error.total
error.by_code
error.by_category
error.by_scope
error.transient_total
error.user_visible_total
error.suppressed_total
error.deduplicated_total
error.fatal_total
```

Structured error log fields:

```text
timestamp
severity
error.code
error.category
error.scope
error.retry_hint
owner.module
business_stage.id
work.type
session.id
revision.id
workitem.id
attempt.id
provider.id
authority.state
result.disposition
message
```

Technical message phải sanitize.

---

## 44. Runtime Events

Conceptual events:

```text
REVISION_CREATED
WORKITEM_CREATED
ATTEMPT_STARTED
ATTEMPT_COMPLETED
AUTHORITY_REVOKED
AUTHORITY_REJECTED
CANDIDATE_CREATED
OWNERSHIP_TRANSFERRED
ARTIFACT_PUBLISHED
LEASE_ACQUIRED
LEASE_RELEASED
RETENTION_ADDED
RETENTION_REMOVED
RESOURCE_LOGICALLY_DISPOSED
RESOURCE_PHYSICALLY_DISPOSED
PROVIDER_DEGRADED
SESSION_FAILED
RUNTIME_FATAL
```

Event Bus không phụ thuộc telemetry availability.

---

## 45. Diagnostic Snapshot Model

```text
RuntimeDiagnosticSnapshot
├── Application
├── Sessions
├── Revisions
├── Authority
├── WorkItems
├── Attempts
├── Queues
├── Scheduler
├── Providers
├── Candidates
├── Artifacts
├── Ownership
├── Retention
├── Leases
├── Resources
├── ResourcePressure
├── Cache
├── UI
├── RecentErrors
└── RecentEvents
```

Snapshot là best-effort, không phải transactional export.

---

## 46. Authority Snapshot

```text
application_authority
active_session_count
current_revision_by_session
revoked_scope_count
pending_completion_validation
recent_authority_rejections
commit_authority_state
```

---

## 47. Work and Attempt Snapshot

```text
workitem.active_count
workitem.by_work_type
attempt.running_count
attempt.abandoned_count
attempt.pending_retry_count
oldest_attempt_age
current_revision_attempts
obsolete_attempts
```

Không nhúng WorkItem payload.

---

## 48. Artifact and Ownership Snapshot

```text
candidate.count
artifact.count_by_type
artifact.bytes_by_type
ownership.transfer_pending_count
ownership.owner_count_by_type
artifact.publication_pending_count
artifact.publication_failed_count
```

---

## 49. Lease and Retention Snapshot

```text
lease.active_count
lease.oldest_age
lease.waiting_count
lease.leak_suspect_count
retention.count_by_class
retention.bytes_by_class
disposal.blocked_by_lease
```

---

## 50. Resource Snapshot

```text
resource.count_by_type
resource.draining_count
resource.oldest_draining_age
resource.logical_disposed_count
resource.pending_physical_disposal
resource.cleanup_failure_count
native.handle_count
gpu.resource_count
```

---

## 51. Queue and Scheduler Snapshot

```text
queue.class
queue.capacity
queue.depth
queue.oldest_age
queue.current_revision_items
queue.obsolete_items
scheduler.last_decision
scheduler.control_capacity
scheduler.pressure_state
```

---

## 52. Provider Snapshot

```text
provider.id
provider.profile
provider.state
provider.requests_inflight
provider.abandoned_requests
provider.recent_p50
provider.recent_p95
provider.failure_rate
provider.rate_limit_state
provider.backoff_until
provider.last_error_code
```

Credentials không bao giờ xuất hiện.

---

## 53. Development Diagnostic Views

Development-only views nên có:

- Revision Timeline;
- WorkItem/Attempt Lineage;
- Authority View;
- Publication View;
- Ownership View;
- Lease View;
- Retention View;
- Queue View;
- Provider View;
- Resource Pressure View;
- Shutdown Timeline.

---

## 54. Revision Timeline View

Example:

```text
Revision 210
├── Observation             35 ms
├── Planning                4 ms
├── Reuse Evaluation        6 ms
├── Queue Wait             10 ms
├── Attempt Execution     420 ms
├── Authority Validation    2 ms
├── Ownership Transfer      1 ms
├── Publication             3 ms
├── Presentation           42 ms
└── UI Commit              16 ms
```

Stale Revision:

```text
Revision 211
├── Attempt Execution     900 ms
├── Authority Reject        1 ms
└── STALE
```

---

## 55. Recent Event Ring Buffer

Development Runtime có thể giữ bounded ring buffer của lightweight event.

Yêu cầu:

- bounded;
- content-free;
- concurrency-safe;
- optional trong production;
- không thay durable log;
- không giữ raw payload.

---

## 56. Structured Logging Rules

Structured logs phải:

- stable event name;
- machine-readable field;
- correlation identifiers;
- bounded size;
- redaction;
- severity phù hợp;
- rate limiting;
- deduplication.

Không dùng high-severity cho:

- expected cancellation;
- normal stale rejection;
- cache miss;
- queue replacement;
- normal fallback;
- user session stop.

---

## 57. Cardinality Control

Unsafe metric labels:

- all runtime IDs;
- provider request ID;
- raw error;
- file path;
- window title;
- source text;
- content hash.

IDs chỉ dùng trace, structured log hoặc local development snapshot.

---

## 58. Privacy Principles

```text
No Reading Content by Default
```

Standard telemetry không chứa:

- screenshot;
- recognized text;
- translation;
- prompt;
- source URL;
- page title;
- window title;
- clipboard;
- provider body;
- secret;
- token;
- credential.

---

## 59. Content-Derived Metadata

Có thể dùng metadata coarse và bounded:

- dimensions;
- region count;
- text-length bucket;
- language code;
- confidence bucket;
- input-size class;
- hash algorithm version.

Content fingerprint:

- local by default;
- không metric label;
- không export mặc định;
- phải có privacy classification.

---

## 60. Telemetry Modes

```text
OFF
LOCAL_ONLY
ANONYMOUS_OPERATIONAL
DEVELOPMENT_VERBOSE
```

### OFF

Chỉ essential in-memory counters/state.

### LOCAL_ONLY

Logs, traces và snapshots ở device.

### ANONYMOUS_OPERATIONAL

Chỉ bounded content-free aggregate.

### DEVELOPMENT_VERBOSE

Thêm lifecycle trace và diagnostic details, vẫn không chứa content mặc định.

---

## 61. Diagnostic Consent

Detailed content diagnostics nếu có phải explicit.

Possible modes:

```text
STANDARD
ENHANCED_DIAGNOSTICS
CONTENT_DEBUG
```

MVP: `CONTENT_DEBUG` disabled hoặc development-only.

---

## 62. Sampling

Ưu tiên giữ:

- fatal/invariant;
- shutdown failure;
- authority conflict;
- publication failure;
- cleanup failure;
- resource leak;
- slow useful result;
- retry;
- stale expensive work;
- provider degradation.

Sample mạnh hơn:

- normal success;
- cache hit;
- unchanged frame;
- routine cancellation;
- normal queue operation.

Tail-based sampling có thể dùng trong tương lai.

---

## 63. Telemetry Overhead

Observability không được:

- synchronous disk write trên critical path;
- blocking network export;
- serialize payload lớn;
- tạo unbounded queue;
- trace mọi frame;
- capture expensive stack cho expected flow;
- block Runtime Control.

Telemetry queue cũng phải bounded.

---

## 64. Telemetry Backpressure

Khi telemetry saturated:

1. giữ fatal/critical;
2. giữ invariant và important error;
3. giữ aggregate metrics;
4. drop verbose span detail;
5. drop repeated info/debug;
6. increment dropped telemetry metric.

Runtime không block.

---

## 65. Observability Failure

Observability failure:

- exporter unavailable;
- log sink fail;
- trace buffer full;
- snapshot failure;
- telemetry config invalid.

Default:

```text
Record bounded local diagnostic if possible
    ↓
Degrade observability
    ↓
Continue Runtime
```

Không làm Session fail mặc định.

---

## 66. Startup Observability

Track:

```text
runtime.initialize_ms
runtime_control.ready_ms
capture.initialize_ms
provider.initialize_ms
artifact_store.initialize_ms
cache.initialize_ms
model.load_ms
ui.ready_ms
first_revision_ms
first_useful_result_ms
```

Cold start tách steady state.

---

## 67. Shutdown Observability

Track milestones:

```text
shutdown.requested_at
admission.stopped_at
authority.revoked_at
sessions.canceled_at
queues.drained_at
attempts.drained_at
providers.unloaded_at
leases.released_at
resources.disposed_at
telemetry.flushed_at
shutdown.completed_at
```

Metrics:

```text
shutdown.duration_ms
shutdown.abandoned_attempt_count
shutdown.cleanup_failure_count
shutdown.remaining_lease_count
shutdown.remaining_resource_count
```

---

## 68. Health Model

Overall:

```text
HEALTHY
DEGRADED
PARTIALLY_UNAVAILABLE
CRITICAL
SHUTTING_DOWN
```

Sub-health:

- Runtime Control;
- Capture;
- Provider;
- Scheduler;
- Artifact Store;
- Resource Manager;
- Storage;
- UI;
- Observability.

Observability degraded không tự làm overall Runtime critical.

---

## 69. Alerts

Useful alerts:

- fatal Runtime error;
- Runtime Control stall;
- sustained queue saturation;
- high stale ratio;
- useful latency exceeded;
- provider failure rate;
- retry exhaustion;
- sustained resource pressure;
- Lease leak;
- draining resource too old;
- publication failure rate;
- shutdown timeout.

Threshold thuộc `RUNTIME_CONFIG.md`.

---

## 70. Alert Stability

Dùng:

- moving window;
- minimum duration;
- trigger/recovery hysteresis;
- cooldown;
- deduplication;
- consecutive sample threshold.

---

## 71. MVP Observability Model

```text
Runtime Observability
├── In-Memory Metrics
├── Structured Local Logs
├── Revision/WorkItem/Attempt Trace
├── Bounded Recent Event Buffer
└── Runtime Diagnostic Snapshot
```

Remote export không bắt buộc.

---

## 72. MVP Required Correlation

```text
ApplicationInstanceId
SessionId
RevisionId
WorkItemId
AttemptId
OwnerModule
WorkType
```

Provider thêm:

```text
ProviderId
ProviderRequestId
```

Artifact thêm:

```text
ArtifactId hoặc CandidateArtifactId
ArtifactType
```

---

## 73. MVP Required Metrics

Minimum:

```text
revision.created_total
revision.committed_total
revision.failed_total
revision.superseded_total
revision.useful_latency_ms

workitem.created_total
workitem.admitted_total
workitem.failed_total
workitem.canceled_total
workitem.stale_total

attempt.started_total
attempt.failed_total
attempt.abandoned_total
attempt.duration_ms

authority.validation_ms
authority.rejected_total

queue.depth
queue.wait_ms

provider.request_ms
provider.failure_total
provider.requests_inflight

retry.approved_total
retry.exhausted_total

cache.hit_total
cache.miss_total

candidate.rejected_total
artifact.published_total
artifact.active_count
artifact.active_bytes

lease.active_count
resource.draining_count
resource.disposal_failed_total

memory.process_bytes
resource.pressure_level

ui.commit_ms
ui.commit_rejected_total
```

---

## 74. MVP Required Logs

Structured logs cho:

- startup failure;
- Session startup failure;
- WorkItem terminal failure;
- provider initialization/auth failure;
- retry exhaustion;
- ownership transfer failure;
- publication failure;
- cleanup failure;
- Lease leak;
- invalid state transition;
- invariant violation;
- shutdown timeout.

---

## 75. MVP Required Trace

Một accepted Revision trace:

```text
Observation
    ↓
Revision Creation
    ↓
Business Planning
    ↓
Reuse Evaluation
    ↓
WorkItem / Attempt
    ↓
Authority Validation
    ↓
Ownership Transfer
    ↓
Artifact Publication
    ↓
Presentation
    ↓
UI Commit
```

Retry, cancellation và stale rejection nằm trong cùng logical trace.

---

## 76. Testing Requirements

Test phải verify:

- correlation propagation;
- one root Revision trace;
- same WorkItem/new Attempt lineage;
- authority accepted/rejected;
- ownership transfer success/failure;
- Candidate publication/rejection;
- Lease acquire/release/contention/leak;
- retention add/remove;
- logical/physical disposal;
- stale result disposition;
- cancellation không tính failure;
- queue wait tách execution;
- privacy redaction;
- bounded metric dimensions;
- bounded telemetry queue;
- snapshot non-blocking;
- exporter failure không phá Runtime;
- shutdown telemetry;
- no content in events/logs/metrics.

---

## 77. Deterministic Tests

Dùng:

- fake clock;
- fake provider;
- manual Completion gate;
- manual authority gate;
- fake Artifact Store;
- fake Lease manager;
- deterministic telemetry sink.

Assert chính xác:

```text
Queue Wait
Execution
Authority Validation
Ownership Transfer
Publication
UI Commit
Drain
```

---

## 78. Long-Session Tests

Verify:

- metric structure không tăng theo IDs;
- trace/ring buffer bounded;
- logs retention bounded;
- telemetry worker bounded;
- snapshots stable;
- Lease metrics khớp actual state;
- resource disposal metrics khớp lifecycle;
- no retention leak;
- observability overhead stable.

---

## 79. Architecture Invariants

1. Observability giải thích decision, không chỉ activity.
2. Mọi accepted Revision có traceable lifecycle.
3. WorkItem và Attempt identity tách biệt.
4. Retry có AttemptId riêng.
5. Authority validation observable.
6. Ownership transfer observable.
7. Candidate rejection observable.
8. Publication observable.
9. Lease lifecycle observable.
10. Retention lifecycle observable.
11. Logical và physical disposal tách biệt.
12. Queue wait tách execution.
13. Cancellation không tính failure.
14. Stale work observable nhưng quiet cho user.
15. Runtime events không tự động là telemetry.
16. Event Bus không phụ thuộc telemetry.
17. Standard telemetry không chứa reading content.
18. Secrets được redact trước storage/export.
19. Metric cardinality bounded.
20. Runtime IDs không là aggregate labels.
21. Telemetry queue bounded.
22. Telemetry failure không phá correctness.
23. Runtime Control không block bởi export.
24. Snapshot không chứa payload lớn.
25. Provider health transition observable.
26. Duplicate signal không tạo duplicate terminal outcome.
27. Fatal/invariant có elevated diagnostics.
28. Sampling giữ slow/failure/cleanup traces.
29. Observability overhead measurable.
30. Cache/Storage/Runtime ownership không bị trộn trong telemetry.

---

## 80. Open Questions

- Dùng OpenTelemetry không?
- Trace local có persist qua restart không?
- Log retention bao lâu?
- Support bundle có export Snapshot không?
- User có tắt toàn bộ telemetry không?
- Metric nào user-facing?
- Slow Revision threshold?
- Provider cost estimate có lưu không?
- Lifecycle event trace toàn bộ hay sample?
- GPU telemetry đa nền tảng?
- Crash recovery report?
- Anonymous operational telemetry trong MVP?
- Authority/Ownership diagnostic view có cần user-facing không?
- Lease leak threshold theo resource type?
- Content-derived metadata nào được export?

---

## 81. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem, Attempt, Completion, authority |
| `RUNTIME_COMPONENTS.md` | Observability ownership |
| `SCHEDULER.md` | Admission decisions |
| `WORK_QUEUE.md` | Queue metrics |
| `CANCELLATION.md` | Authority revoke and drain |
| `RETRY_POLICY.md` | Attempt lineage |
| `ERROR_MODEL.md` | RuntimeError telemetry |
| `CACHE_POLICY.md` | Reuse and retention |
| `MEMORY_MODEL.md` | Memory/resource metrics |
| `RESOURCE_LIFECYCLE.md` | Ownership, Lease, disposal |
| `THREADING_MODEL.md` | Execution context |
| `PERFORMANCE_MODEL.md` | Performance budgets |
| `RUNTIME_CONFIG.md` | Telemetry settings |
| `BOOT_SEQUENCE.md` | Startup/shutdown timeline |
| `../core/EVENT_BUS.md` | Runtime event semantics |

---

## 82. Completion Criteria

`RUNTIME_OBSERVABILITY.md` được xem là đồng bộ khi:

- pipeline trace dùng BusinessExecutionPlan/WorkItem/Attempt;
- Stage không còn là dimension bắt buộc;
- authority trace và metrics tồn tại;
- Candidate → transfer → publication observable;
- ownership, retention và Lease tách rõ;
- logical/physical disposal observable;
- Snapshot có authority, ownership, Lease và pressure;
- Runtime Event naming đồng bộ;
- privacy/cardinality/sampling giữ nguyên;
- MVP metrics và tests khớp Runtime v2.

---

## 83. Summary

CRAI Observability dùng:

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

Correlation chính:

```text
Application
    ↓
Session
    ↓
Revision
    ↓
WorkItem
    ↓
Attempt
```

Lifecycle cần giải thích:

```text
Attempt Completed
    ↓
Authority Validated
    ↓
Candidate Accepted
    ↓
Ownership Transferred
    ↓
Artifact Published
    ↓
Presentation Committed
```

Observability thành công khi Runtime có thể trả lời không chỉ **điều gì đã xảy ra**, mà còn **vì sao quyết định đó được đưa ra, kết quả có còn authority không, và resource nào vẫn đang giữ nó**.
