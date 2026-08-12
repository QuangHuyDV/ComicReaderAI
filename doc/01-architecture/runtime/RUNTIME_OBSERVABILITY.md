# Runtime Observability

* **Document:** Runtime Architecture / Runtime Observability
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI observes, measures, diagnoses and explains Runtime behavior.

Observability SHOULD answer questions such as:

```text
Which BusinessExecutionPlan was accepted?
Which ExecutionScope / ExecutionRevision was involved?
Which WorkItem was materialized and why?
Which Attempt physically executed?
Where was time spent?
Was execution authority still valid?
Was the Runtime result accepted?
Was a Runtime Artifact published?
Did the owning Business Module accept the result?
Did Presentation commit the result?
Was work stale, canceled or abandoned?
Which resource owner / retention / lease keeps data alive?
Did Retry create useful recovery?
Did Routing/Recovery select another execution binding?
Where is Runtime under pressure?
Why is UI/Presentation delayed?
What prevents shutdown/resource cleanup?
```

Observability MUST explain:

```text
what happened
+
why it happened
+
whether it still mattered
+
which owner accepted/rejected it
```

---

# 2. Architectural Position

```text
Runtime Operation / Decision
        |
        v
Metrics / Trace / Structured Log / Runtime Event
        |
        v
Bounded Observability Pipeline
        |
        +--> In-Memory Diagnostics
        |
        +--> Local Diagnostics
        |
        +--> Optional Sanitized Export
```

Observability does NOT:

* own Runtime state;
* replace Event Bus;
* create WorkItem;
* decide Retry;
* select Fallback;
* grant execution authority;
* accept Business result;
* mutate Runtime Artifact;
* commit Presentation/UI state;
* block Runtime Control;
* contain reading content by default.

---

# 3. Core Philosophy

```text
Record activity
+
Record decision
+
Record reason
+
Record relevance
+
Record ownership outcome
```

A message such as:

```text
Translation completed
```

is insufficient.

A useful observation SHOULD answer:

```text
Which ExecutionScope?
Which ExecutionRevision?
Which WorkItem?
Which Attempt?
Which WorkType?
Which ExecutionBinding?
What was the physical outcome?
Was execution authority accepted?
Was a Runtime Artifact published?
Was Business result accepted?
Was Presentation committed?
Was the result later stale/canceled?
How much time was spent queued, executing, validating,
publishing, accepting and draining?
```

---

# 4. Observability Signals

```text
Runtime Observability
├── Metrics
├── Traces
├── Structured Logs
├── Runtime Events
└── Diagnostic Snapshots
```

---

# 5. Metrics

Metrics are used for:

* counts;
* rates;
* ratios;
* durations;
* percentiles;
* capacity;
* pressure;
* bounded health projections.

Metrics MUST have bounded cardinality.

---

# 6. Traces

Traces describe causal lifecycle.

Useful trace subjects include:

```text
ExecutionScope
ExecutionRevision
WorkItem
Attempt
ExecutionBinding operation
Runtime Artifact lifecycle
Business acceptance
Presentation delivery
Cancellation
Retry
Recovery / Routing
Resource lifecycle
```

---

# 7. Structured Logs

Structured logs are appropriate for:

* normalized error;
* invariant violation;
* rejected authority;
* rejected ownership transfer;
* Business result rejection;
* Presentation commit failure;
* cleanup failure;
* shutdown anomaly;
* unexpected Runtime decision.

---

# 8. Runtime Events

Runtime Events are architectural messages.

They are NOT automatically telemetry.

Event Bus correctness MUST NOT depend on observability availability.

Telemetry MAY observe or derive signals from events.

---

# 9. Diagnostic Snapshots

Diagnostic Snapshot is a best-effort current-state projection.

It MUST NOT be treated as:

```text
transactional Runtime state export
```

and MUST NOT contain large payloads.

---

# 10. Signal Selection

Use **Metrics** for:

* aggregate count;
* rate;
* duration;
* percentile;
* saturation;
* bounded health.

Use **Trace** for:

* causal lifecycle;
* Attempt lineage;
* authority validation;
* ownership transfer;
* Runtime Artifact publication;
* Business acceptance;
* Retry;
* Recovery/alternative execution;
* Presentation delivery;
* resource drain.

Use **Log** for:

* error;
* invariant;
* rejected decision;
* cleanup anomaly;
* fatal state.

Use **Snapshot** for:

* current ExecutionScope/ExecutionRevision;
* queues;
* attempts;
* execution authority;
* bindings;
* Artifacts;
* ownership;
* retention;
* leases;
* resource pressure;
* recent failures.

---

# 11. Canonical Correlation Model

Primary Runtime hierarchy:

```text
ApplicationInstanceId
        |
        v
ExecutionScopeId
        |
        v
ExecutionRevisionId
        |
        v
WorkItemId
        |
        v
AttemptId
```

---

# 12. Optional Business Correlation

Additional business identifiers MAY include:

```text
BusinessExecutionPlanId
BusinessStageId
ReadingSessionId
ProjectId
DocumentId
RequestId
```

They MUST NOT replace Runtime identities.

---

# 13. Resource Correlation

Additional Runtime/resource identifiers MAY include:

```text
RuntimeArtifactId
CandidateResourceId
ResourceId
LeaseId
RetentionId
ExecutionBindingReference
PhysicalChildOperationId
PresentationId
TraceId
SpanId
```

---

# 14. Correlation Requirements

Every Attempt Completion SHOULD carry at least:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
OwnerModule
WorkType
PhysicalOutcome
```

Where applicable:

```text
BusinessStageId
ExecutionBindingReference
RuntimeConfigurationSnapshotId
```

---

# 15. Execution Binding Correlation

Execution-binding operations MAY additionally carry:

```text
ExecutionBindingReference
ExecutionClass
ProviderId?
ProviderRuntimeInstanceId?
ModelDeploymentId?
PhysicalRequestId?
```

High-cardinality identifiers belong in trace/log, not aggregate metric labels.

---

# 16. Runtime Artifact Correlation

Artifact lifecycle MAY carry:

```text
RuntimeArtifactId
CandidateResourceId
ArtifactType
ResourceId
ProducerWorkItemId
ProducerAttemptId
ProducerExecutionRevisionId
```

---

# 17. Resource Lifecycle Correlation

Resource lifecycle MAY carry:

```text
ResourceId
ResourceType
OwnerType
RetentionKind
RetentionId?
LeaseId?
DisposalState
IntegrityState?
```

---

# 18. Correlation Propagation

Recommended:

```text
Application / Business Intent
        |
        v
BusinessExecutionPlan
        |
        v
ExecutionScope / ExecutionRevision
        |
        v
WorkItem
        |
        v
Scheduler
        |
        v
Work Queue
        |
        v
Attempt / Execution Binding
        |
        v
Completion
        |
        v
Execution Authority Validation
        |
        v
Runtime Artifact Publication
        |
        v
Business Acceptance
        |
        v
Presentation / UI Commit
```

Correlation context remains lightweight and content-free.

---

# 19. ExecutionRevision Root Trace

Each significant accepted ExecutionRevision SHOULD be traceable.

Conceptually:

```text
EXECUTION_REVISION
├── Business Plan Binding
├── Observation / Source Trigger?
├── Reuse Evaluation
├── Stage Runtime Readiness
├── WorkItems
│   └── Attempts
├── Execution Authority
├── Runtime Artifact Publication
├── Business Acceptance
├── Presentation
└── Resource Drain
```

---

# 20. Optional Root Trace Branches

Possible branches:

```text
Cancellation
Retry
Recovery / Alternative Execution
Cache Reuse
Stale Rejection
Resource Wait
Degradation
Cleanup
```

Fallback is represented under Recovery/Routing, not Retry.

---

# 21. WorkItem / Attempt Lineage

```text
WorkItem W1
├── Attempt A1: FAILED
├── Attempt A2: FAILED
└── Attempt A3: COMPLETED
```

Trace MUST distinguish:

* first Attempt;
* same-work automatic Retry;
* alternative-binding recovery;
* manual re-execution;
* abandoned Attempt;
* late Completion.

Retry preserves:

```text
WorkItemId
```

and creates:

```text
new AttemptId
```

---

# 22. Attempt Span

Suggested span:

```text
runtime.attempt
```

Possible bounded attributes:

```text
owner.module
business_stage.class?
work.type
attempt.number
execution.class
execution.binding.class
queue.class
queue.wait_ms
resource.wait_ms
lease.wait_ms
execution.duration_ms
physical.outcome
authority.result
cancellation.requested
error.code
```

No raw input/output.

---

# 23. Attempt Physical Outcome

Recommended:

```text
COMPLETED
FAILED
CANCELLED
ABANDONED
```

Do NOT use:

```text
STALE
```

as physical outcome.

---

# 24. Execution Authority Trace

Execution authority MUST be observable.

Possible events/spans:

```text
AUTHORITY_VALIDATION_STARTED
AUTHORITY_ACCEPTED
AUTHORITY_REJECTED
AUTHORITY_REVOKED
LATE_COMPLETION_REJECTED
```

---

# 25. Authority Attributes

Possible:

```text
authority.scope
authority.reason
authority.execution_revision_state
authority.validation_ms
authority.requested_attempt
authority.accepted_attempt?
```

Avoid:

```text
COMMIT_AUTHORITY_REJECTED
```

because Runtime does not own all commit semantics.

---

# 26. Authority vs Owner Acceptance

Observability MUST distinguish:

```text
Execution Authority Accepted
```

from:

```text
Business Result Accepted
```

and:

```text
Presentation Commit Accepted
```

These are three different boundaries.

---

# 27. Runtime Artifact Publication Trace

Recommended:

```text
Candidate Resource Created
        |
        v
Candidate Registered
        |
        v
Technical / Integrity Validation
        |
        v
Execution Authority Validation
        |
        v
Ownership Transfer
        |
        v
Runtime Artifact Published
```

---

# 28. Business Semantic Validation Is Separate

Do NOT model:

```text
Semantic Validation
```

as a generic Runtime Artifact publication prerequisite.

Business semantic validation belongs to the owning module.

Recommended separate trace:

```text
Runtime Artifact Published
        |
        v
Business Validation Started
        |
        +--> BUSINESS_ACCEPTED
        |
        +--> BUSINESS_REJECTED
```

where such a contract exists.

---

# 29. Publication Metrics / Trace

Distinguish:

* candidate creation;
* candidate rejection;
* integrity rejection;
* authority rejection;
* transfer failure;
* duplicate publication;
* successful publication;
* candidate cleanup.

---

# 30. Business Acceptance Trace

Suggested span:

```text
business.result_acceptance
```

Attributes MAY include:

```text
owner.module
business_stage.class?
result.type
result.acceptance
business.error_code?
acceptance.duration_ms
```

No user content.

---

# 31. Presentation Trace

Suggested spans:

```text
presentation.prepare
presentation.target_validate
presentation.commit
```

Distinguish:

```text
Runtime execution relevance
```

from:

```text
Presentation target/view validity
```

---

# 32. Ownership Trace

Possible events:

```text
OWNERSHIP_TRANSFER_REQUESTED
OWNERSHIP_TRANSFER_ACCEPTED
OWNERSHIP_TRANSFER_REJECTED
OWNERSHIP_RELEASED
```

Attributes:

```text
resource.type
previous_owner_type
new_owner_type
transfer.reason
transfer.duration_ms
```

ResourceId stays in trace/log only where needed.

---

# 33. Visibility Trace

Because Resource Lifecycle is multidimensional, visibility SHOULD be observable separately.

Possible:

```text
RESOURCE_PUBLISHED
RESOURCE_WITHDRAWN
```

Publication does not imply Business acceptance.

---

# 34. Resource Lease Trace

Possible:

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
resource.type
lease.owner_type
lease.wait_ms
lease.hold_ms
lease.result
```

LeaseId is not a metric label.

---

# 35. Retention Observability

Possible:

```text
RETENTION_ADDED
RETENTION_REMOVED
RETENTION_EXPIRED
RETENTION_EVICTED
```

Attributes:

```text
resource.type
retention.kind
retention.owner_type
retention.reason
size.estimate
```

Cache retention MUST NOT be described as payload ownership.

---

# 36. Resource Lifecycle Observability

Do NOT force one canonical linear lifecycle onto every resource.

Observe independent dimensions:

```text
Registration
Ownership
Visibility
Retention
Lease
Usage
Disposal
Integrity
```

---

# 37. Disposal Trace

Possible disposal events:

```text
RESOURCE_LOGICAL_DISPOSAL_STARTED
RESOURCE_LOGICALLY_DISPOSED
RESOURCE_DRAINING
RESOURCE_PHYSICAL_DISPOSAL_COMPLETED
RESOURCE_DISPOSAL_FAILED
RESOURCE_LEAK_DETECTED
```

---

# 38. Resource Anomalies

Examples:

* registered resource never cleanup;
* transfer pending too long;
* candidate never released;
* Lease never released;
* dispose while leased;
* draining too long;
* resurrection attempt;
* physical child operation retained after shutdown.

---

# 39. Execution Binding Trace

Suggested generic span:

```text
runtime.execution_binding
```

or adapter-specific child span.

Attributes MAY include:

```text
execution.binding.class
execution.class
operation
request_size_class
timeout_ms
physical.result
normalized.error_code
queue_wait_ms
duration_ms
abandoned
abort_supported
```

Optional low-cardinality provider/model attributes MAY be attached where safe.

---

# 40. Provider-Specific Trace

Provider adapters MAY additionally record:

```text
provider.id
model.deployment_class
provider.request_id
http.status_class
```

Raw Prompt/source/response is forbidden by default.

---

# 41. Provider Management Boundary

Observability MAY record:

```text
ExecutionBindingDegraded
ExecutionBindingRecovered
```

Canonical Provider health/governance state remains external.

Do NOT make Runtime telemetry the source of truth for Provider Management state.

---

# 42. Cache / Reuse Trace

Suggested:

```text
cache.reuse_lookup
cache.candidate_validation
cache.promotion
cache.eviction
```

Possible attributes:

```text
result.type
owner.module
reuse.scope
reuse.result
reuse.reject_reason
lookup_ms
validation_ms
retention.kind
eviction.reason
reuse.partition.class
```

Do not export raw cache key/fingerprint by default.

---

# 43. Cache Promotion Trace

Default promotion path SHOULD allow distinguishing:

```text
Runtime Artifact Published
Business Result Accepted
Owner Cache Eligibility Accepted
Cache Retention Added
```

A published Artifact that Business rejects MUST NOT appear as a normal successful promotion.

---

# 44. Queue Trace

```text
Candidate Eligible
        |
        v
Scheduler Decision
        |
        v
Queued
        |
        v
Selected
        |
        v
Dispatched
```

Queue wait remains separate from execution duration.

---

# 45. Queue Classes

Metrics MAY distinguish:

```text
CONTROL
INTERACTIVE
BACKGROUND
MAINTENANCE
```

provided classes remain low-cardinality.

---

# 46. Scheduler Observability

Canonical decisions:

```text
ADMIT
DEFER
REJECT
REPLACE
```

Possible attributes:

```text
decision
reason
priority_class
work.type
queue.class
resource.pressure
capacity.available
execution_revision_state
decision.duration_ms
```

High-volume normal decisions SHOULD use aggregate metrics + sampling rather than full logs.

---

# 47. Cancellation Trace

Cancellation trace SHOULD answer:

* requester;
* scope;
* reason;
* authority revocation;
* queued work removal;
* running Attempts signaled;
* physical child abort result;
* abandonment;
* drain duration;
* late Completion rejection.

---

# 48. Cancellation Milestones

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

# 49. Retry Trace

Suggested:

```text
Attempt A1 Failed
        |
        v
Retry Evaluated
        |
        +--> RETRY_NOW
        +--> RETRY_LATER
        +--> DO_NOT_RETRY
        +--> RETRY_EXHAUSTED
        +--> RECOVERY_ESCALATION_REQUIRED
        |
        v
Attempt A2?
```

---

# 50. Retry Attributes

Possible:

```text
retry.decision
retry.reason
retry.timing
retry.delay_ms
retry.budget_remaining_class
retry.reuse_satisfied
```

Do NOT use:

```text
retry.provider_changed
retry.provider_fallback_total
```

as Retry semantics.

---

# 51. Recovery / Routing Trace

Alternative execution belongs to a separate trace branch.

Suggested:

```text
recovery.escalation
routing.selection
execution_binding.rebound
```

Possible bounded attributes:

```text
recovery.reason
recovery.outcome
routing.result
binding.changed
selection.duration_ms
```

Exact routing/provider details belong to AI/Provider observability contracts.

---

# 52. Stale / Obsolete Work

Stale/obsolete work remains visible for developers but quiet for users by default.

Record:

```text
original_execution_revision_state
current_execution_revision_state
work.type
attempt.number
execution.duration_ms
execution.cost_class
authority.reject_reason
candidate_created
publication_attempted
```

Technical success + stale rejection MUST NOT count as execution-binding failure.

---

# 53. Metrics Naming

Conceptual prefix:

```text
crai.runtime.<domain>.<measurement>
```

Examples:

```text
crai.runtime.execution_revision.created_total
crai.runtime.workitem.active_count
crai.runtime.attempt.duration_ms
crai.runtime.authority.validation_ms
crai.runtime.artifact.publication_ms
crai.runtime.business.acceptance_ms
crai.runtime.lease.hold_ms
crai.runtime.resource.draining_count
crai.runtime.presentation.commit_ms
```

Exact backend naming MAY differ.

Semantics MUST remain stable.

---

# 54. Metric Design Rules

Metrics MUST:

* use bounded cardinality;
* have stable names;
* stay low-overhead;
* separate logical and physical work;
* separate current and obsolete work;
* separate Queue wait and execution;
* separate physical outcome and authority outcome;
* separate Runtime publication and Business acceptance;
* separate Business acceptance and Presentation commit;
* separate logical and physical disposal;
* avoid raw identifiers.

---

# 55. Safe Metric Dimensions

Preferred bounded dimensions MAY include:

```text
owner_module
work_type
business_stage_class
execution_class
execution_binding_class
physical_outcome
authority_outcome
business_acceptance
cache_status
queue_class
cancellation_reason
resource_type
retention_kind
pressure_level
device_profile
```

---

# 56. Unsafe Metric Labels

Do NOT use:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
RuntimeArtifactId
ResourceId
LeaseId
ProviderRequestId
raw error message
content hash
raw source path
```

These MAY appear in trace/log/local snapshots when privacy policy permits.

---

# 57. ExecutionRevision Metrics

Recommended:

```text
execution_revision.created_total
execution_revision.current_total
execution_revision.superseded_total
execution_revision.cancelled_total
execution_revision.failed_total
execution_revision.active_count
execution_revision.lifetime_ms
execution_revision.useful_latency_ms
execution_revision.first_useful_result_ms
execution_revision.churn_rate
```

Avoid generic:

```text
execution_revision.committed_total
```

because Runtime has multiple downstream acceptance/commit boundaries.

---

# 58. WorkItem Metrics

Recommended:

```text
workitem.created_total
workitem.eligible_total
workitem.admitted_total
workitem.rejected_total
workitem.replaced_total
workitem.active_count
workitem.succeeded_total
workitem.failed_total
workitem.cancelled_total
workitem.abandoned_total
```

Do NOT use `workitem.stale_total` as canonical terminal-state metric.

---

# 59. Attempt Metrics

```text
attempt.started_total
attempt.completed_total
attempt.failed_total
attempt.cancelled_total
attempt.abandoned_total
attempt.late_completion_total
attempt.duration_ms
attempt.count_per_workitem
attempt.resource_wait_ms
attempt.lease_wait_ms
```

---

# 60. Authority Metrics

```text
authority.validation_total
authority.accepted_total
authority.rejected_total
authority.revoked_total
authority.validation_ms
authority.late_completion_rejected_total
authority.stale_rejected_total
authority.cancelled_rejected_total
authority.duplicate_rejected_total
```

Remove ambiguous:

```text
authority.commit_rejected_total
```

---

# 61. Runtime Artifact Publication Metrics

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

# 62. Business Acceptance Metrics

Where applicable:

```text
business.acceptance_total
business.accepted_total
business.rejected_total
business.acceptance_ms
business.recovery_requested_total
```

Dimension by bounded owner/module/result class.

---

# 63. Presentation Metrics

```text
presentation.prepare_ms
presentation.target_validation_ms
presentation.commit_ms
presentation.commit_rejected_total
presentation.dispatch_delay_ms
presentation.visible_replacement_total
```

UI framework metrics MAY remain under `ui.*`.

---

# 64. Queue Metrics

Recommended:

```text
queue.depth
queue.capacity
queue.utilization_ratio
queue.enqueue_total
queue.dispatch_total
queue.replaced_total
queue.removed_total
queue.technical_enqueue_failure_total
queue.wait_ms
queue.oldest_item_age_ms
queue.saturation_duration_ms
```

Avoid `queue.rejected_total` if it represents Scheduler admission rejection.

---

# 65. Scheduler Metrics

```text
scheduler.decision_total
scheduler.admit_total
scheduler.defer_total
scheduler.reject_total
scheduler.replace_total
scheduler.decision_ms
scheduler.current_execution_admission_ratio
scheduler.control_capacity_available
```

---

# 66. Cancellation Metrics

```text
cancellation.requested_total
cancellation.authority_revoke_ms
cancellation.queued_remove_ms
cancellation.acknowledged_total
cancellation.worker_ack_ms
cancellation.physical_abort_success_total
cancellation.physical_abort_failed_total
cancellation.abandoned_total
cancellation.drain_ms
cancellation.late_completion_total
```

---

# 67. Retry Metrics

```text
retry.evaluated_total
retry.approved_total
retry.skipped_total
retry.delayed_total
retry.admitted_total
retry.exhausted_total
retry.cancelled_total
retry.delay_ms
retry.recovery_ms
retry.concurrent_count
retry.reuse_avoided_total
retry.recovery_escalation_total
```

No fallback metric under `retry.*`.

---

# 68. Recovery / Routing Metrics

Where implemented:

```text
recovery.escalation_total
recovery.success_total
recovery.failed_total
recovery.duration_ms
routing.selection_total
routing.selection_ms
routing.binding_changed_total
```

Exact provider/AI dimensions belong to their architecture.

---

# 69. Execution Binding Metrics

Generic Runtime metrics MAY include:

```text
execution_binding.request_total
execution_binding.success_total
execution_binding.failure_total
execution_binding.timeout_total
execution_binding.rate_limited_total
execution_binding.cancelled_total
execution_binding.abandoned_total
execution_binding.inflight
execution_binding.duration_ms
execution_binding.queue_wait_ms
execution_binding.cold_start_ms
execution_binding.cost_estimate
```

---

# 70. Provider Metrics Boundary

Provider-specific metrics MAY additionally exist under Provider Management/Provider Runtime telemetry.

Runtime Observability MUST NOT make provider health state authoritative.

---

# 71. Cache Metrics

```text
cache.lookup_total
cache.hit_total
cache.miss_total
cache.validation_rejected_total
cache.compatibility_miss_total
cache.integrity_failure_total
cache.partition_miss_total
cache.promotion_total
cache.promotion_skipped_total
cache.eviction_total
cache.retained_bytes
cache.lookup_ms
cache.saved_execution_ms
cache.saved_execution_cost
cache.inflight_coalesced_total
```

---

# 72. Resource / Lease / Retention Metrics

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

# 73. Memory / Pressure Metrics

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
```

Admission rejection remains Scheduler-owned.

---

# 74. Runtime Control Metrics

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

Runtime Control delay may affect:

* cancellation;
* execution replacement;
* Completion acceptance;
* publication;
* downstream readiness.

It does not directly own Presentation commit.

---

# 75. UI Metrics

Possible framework/UI metrics:

```text
ui.command_ack_ms
ui.dispatch_delay_ms
ui.long_task_total
ui.long_task_ms
ui.frame_stall_total
ui.pending_update_count
```

Do NOT use:

```text
ui.authority_revalidation_ms
```

for Presentation target validation.

---

# 76. Error Metrics

Recommended:

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
business.rejected_total
presentation.failure_total
cleanup.failure_total
```

---

# 77. Structured Error Log Fields

Recommended:

```text
timestamp
severity
error.code
error.category
error.scope
error.retry_hint
error.recovery_hint
owner.module
business_stage.id?
work.type
execution_scope.id
execution_revision.id
workitem.id
attempt.id
execution_binding.class?
provider.id?
physical.outcome
authority.outcome
business.acceptance?
result.disposition
message
```

Technical message MUST be sanitized.

---

# 78. Runtime Events

Conceptual events MAY include:

```text
EXECUTION_SCOPE_OPENED
EXECUTION_SCOPE_CLOSED

EXECUTION_REVISION_CREATED
EXECUTION_REVISION_SUPERSEDED

WORKITEM_CREATED
ATTEMPT_STARTED
ATTEMPT_COMPLETED

AUTHORITY_REVOKED
AUTHORITY_REJECTED

CANDIDATE_CREATED
OWNERSHIP_TRANSFERRED
ARTIFACT_PUBLISHED

BUSINESS_RESULT_ACCEPTED
BUSINESS_RESULT_REJECTED

PRESENTATION_COMMITTED
PRESENTATION_COMMIT_REJECTED

LEASE_ACQUIRED
LEASE_RELEASED

RETENTION_ADDED
RETENTION_REMOVED

RESOURCE_LOGICALLY_DISPOSED
RESOURCE_PHYSICALLY_DISPOSED

EXECUTION_BINDING_DEGRADED
EXECUTION_SCOPE_FAILED
RUNTIME_FATAL
```

Final names follow Event Standard.

---

# 79. Event Boundary

`EXECUTION_BINDING_DEGRADED` is an observed Runtime condition.

Do NOT assume it directly means:

```text
canonical Provider Health state changed
```

unless Provider architecture explicitly defines that event contract.

---

# 80. Diagnostic Snapshot Model

Recommended:

```text
RuntimeDiagnosticSnapshot
├── Application
├── ExecutionScopes
├── ExecutionRevisions
├── Authority
├── WorkItems
├── Attempts
├── Queues
├── Scheduler
├── ExecutionBindings
├── Candidates
├── RuntimeArtifacts
├── Ownership
├── Visibility
├── Retention
├── Leases
├── Resources
├── ResourcePressure
├── Cache
├── BusinessAcceptance
├── Presentation
├── RecentErrors
└── RecentEvents
```

Snapshot remains best-effort.

---

# 81. Authority Snapshot

Recommended:

```text
application_execution_authority
active_execution_scope_count
current_execution_revision_by_scope
revoked_scope_count
pending_completion_validation
recent_authority_rejections
```

Remove:

```text
commit_authority_state
```

---

# 82. Work / Attempt Snapshot

```text
workitem.active_count
workitem.by_work_type
attempt.running_count
attempt.abandoned_count
attempt.pending_retry_count
oldest_attempt_age
current_execution_revision_attempts
obsolete_attempts
```

No payload.

---

# 83. Artifact / Ownership Snapshot

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

# 84. Visibility Snapshot

Optional:

```text
resource.published_count
resource.withdrawn_count
artifact.lookup_visible_count
```

because visibility and ownership are separate dimensions.

---

# 85. Lease / Retention Snapshot

```text
lease.active_count
lease.oldest_age
lease.waiting_count
lease.leak_suspect_count
retention.count_by_kind
retention.bytes_by_kind
disposal.blocked_by_lease
```

---

# 86. Resource Snapshot

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

# 87. Queue / Scheduler Snapshot

```text
queue.class
queue.capacity
queue.depth
queue.oldest_age
queue.current_execution_items
queue.obsolete_items
scheduler.last_decision
scheduler.control_capacity
scheduler.pressure_state
```

Queue does not determine currentness independently.

---

# 88. Execution Binding Snapshot

Recommended generic view:

```text
execution_binding.class
execution_binding.state_projection
execution_binding.inflight
execution_binding.abandoned_operations
execution_binding.recent_p50
execution_binding.recent_p95
execution_binding.failure_rate
execution_binding.rate_limit_pressure
execution_binding.last_error_code
```

Optional provider identity MAY be shown in development mode.

---

# 89. Business Acceptance Snapshot

Where applicable:

```text
business.pending_acceptance_count
business.recent_accept_total
business.recent_reject_total
business.last_rejection_code
```

No business payload.

---

# 90. Presentation Snapshot

```text
presentation.pending_commit_count
presentation.last_target_validation
presentation.recent_commit_total
presentation.recent_reject_total
ui.dispatch_delay
```

---

# 91. Development Diagnostic Views

Recommended development-only views:

* ExecutionRevision Timeline;
* WorkItem/Attempt Lineage;
* Authority View;
* Runtime Artifact Publication View;
* Business Acceptance View;
* Ownership View;
* Visibility View;
* Lease View;
* Retention View;
* Queue/Scheduler View;
* Execution Binding View;
* Resource Pressure View;
* Presentation View;
* Shutdown Timeline.

---

# 92. ExecutionRevision Timeline View

Example:

```text
ExecutionRevision 210
├── Planning                    4 ms
├── Reuse Evaluation           6 ms
├── Queue Wait                10 ms
├── Attempt Execution        420 ms
├── Authority Validation       2 ms
├── Ownership Transfer         1 ms
├── Publication                3 ms
├── Business Acceptance        5 ms
├── Presentation              42 ms
└── Visible Commit            16 ms
```

---

# 93. Obsolete ExecutionRevision Example

```text
ExecutionRevision 211
├── Attempt Execution        900 ms
├── Authority Validation       1 ms
└── REJECT_STALE
```

No Business acceptance or Presentation commit occurs.

---

# 94. Recent Event Ring Buffer

Development Runtime MAY maintain a bounded ring buffer of lightweight events.

Requirements:

* bounded;
* content-free;
* concurrency-safe;
* optional in production;
* not durable source of truth;
* no raw payload.

---

# 95. Structured Logging Rules

Structured logs MUST use:

* stable event name;
* machine-readable fields;
* correlation identifiers;
* bounded size;
* redaction;
* appropriate severity;
* rate limiting;
* deduplication.

---

# 96. Normal Events Are Not Errors

Do NOT use high severity for:

* expected cancellation;
* normal stale rejection;
* cache miss;
* queue replacement;
* normal recovery/routing change;
* normal ExecutionScope close.

---

# 97. Cardinality Control

Unsafe metric labels include:

* all runtime IDs;
* physical request IDs;
* raw errors;
* path;
* window title;
* content;
* content fingerprint.

IDs belong to trace/log/local diagnostics.

---

# 98. Privacy Principle

```text
No Reading Content by Default
```

Standard telemetry MUST NOT contain:

* screenshot;
* recognized/source text;
* translation;
* Prompt;
* full AI Context;
* source URL;
* page/window title;
* clipboard;
* provider body;
* secret;
* token;
* credential.

---

# 99. Content-Derived Metadata

Coarse bounded metadata MAY be allowed by policy, such as:

* dimension class;
* region-count bucket;
* text-length bucket;
* language code;
* confidence bucket;
* input-size class.

Content fingerprint:

* local by default;
* not a metric label;
* not exported by default;
* privacy-classified.

---

# 100. Telemetry Modes

Possible:

```text
OFF
LOCAL_ONLY
ANONYMOUS_OPERATIONAL
DEVELOPMENT_VERBOSE
```

---

# 101. OFF

Only essential in-memory Runtime counters/state required for correctness/health MAY remain.

---

# 102. LOCAL_ONLY

Content-free diagnostics stay on device.

---

# 103. ANONYMOUS_OPERATIONAL

Only bounded sanitized operational aggregate is exportable.

---

# 104. DEVELOPMENT_VERBOSE

Adds richer lifecycle traces and diagnostics.

Reading content remains disabled by default.

---

# 105. Diagnostic Consent

If enhanced diagnostics exist, they require explicit policy/consent.

Possible:

```text
STANDARD
ENHANCED_DIAGNOSTICS
CONTENT_DEBUG
```

MVP SHOULD disable `CONTENT_DEBUG` outside development.

---

# 106. Sampling

Prefer retaining:

* fatal/invariant;
* shutdown failure;
* authority conflict;
* Runtime Artifact publication failure;
* Business rejection anomaly;
* cleanup failure;
* resource leak;
* slow useful result;
* Retry/recovery;
* expensive stale work;
* execution-binding degradation.

Sample more aggressively:

* normal success;
* cache hit;
* unchanged source;
* routine cancellation;
* routine queue operation.

---

# 107. Telemetry Overhead

Observability MUST NOT:

* synchronously write disk on critical path;
* block network export;
* serialize large payload;
* use unbounded queues;
* trace every source frame;
* capture expensive stack for normal flow;
* block Runtime Control.

Telemetry queues MUST be bounded.

---

# 108. Telemetry Backpressure

When saturated:

1. preserve fatal/critical;

2. preserve invariant and important error;

3. preserve aggregate metrics;

4. drop verbose span detail;

5. drop repeated info/debug;

6. increment dropped-telemetry metric.

Runtime execution MUST NOT block.

---

# 109. Observability Failure

Examples:

* exporter unavailable;
* local sink failure;
* trace buffer full;
* Snapshot failure;
* telemetry configuration invalid.

Default:

```text
Record bounded local diagnostic if possible
        |
        v
Degrade Observability
        |
        v
Continue Runtime
```

It MUST NOT automatically fail an ExecutionScope.

---

# 110. Startup Observability

Track separately:

```text
runtime.initialize_ms
runtime_control.ready_ms
capture.initialize_ms?
execution_binding.initialize_ms
artifact_store.initialize_ms
cache.initialize_ms
model_runtime.load_ms?
ui.ready_ms
first_execution_revision_ms
first_useful_result_ms
```

Cold-start and steady-state distributions remain separate.

---

# 111. Shutdown Observability

Track milestones such as:

```text
shutdown.requested_at
admission.stopped_at
application_authority.revoked_at
execution_scopes.cancelled_at
queues.drained_at
attempts.drained_at
execution_bindings.unloaded_at
leases.released_at
resources.disposed_at
telemetry.flushed_at
shutdown.completed_at
```

---

# 112. Shutdown Metrics

```text
shutdown.duration_ms
shutdown.abandoned_attempt_count
shutdown.cleanup_failure_count
shutdown.remaining_lease_count
shutdown.remaining_resource_count
```

---

# 113. Health Model

Possible overall Runtime health projection:

```text
HEALTHY
DEGRADED
PARTIALLY_UNAVAILABLE
CRITICAL
SHUTTING_DOWN
```

Sub-health MAY include:

```text
Runtime Control
Capture Runtime
Execution Binding Runtime
Scheduler
Runtime Artifact Store
Resource Manager
Storage
Presentation/UI
Observability
```

Observability degradation alone SHOULD NOT make Runtime critical.

---

# 114. Alert Candidates

Useful alerts MAY include:

* fatal Runtime error;
* Runtime Control stall;
* sustained Queue saturation;
* high stale ratio;
* Useful Result Latency exceeded;
* high execution-binding failure rate;
* Retry exhaustion;
* Recovery escalation failure;
* sustained resource pressure;
* Lease leak;
* draining resource too old;
* Runtime Artifact publication failure rate;
* Business rejection anomaly;
* shutdown timeout.

Thresholds belong to Runtime Configuration/operations policy.

---

# 115. Alert Stability

Use where practical:

* moving window;
* minimum duration;
* trigger/recovery hysteresis;
* cooldown;
* deduplication;
* consecutive-sample threshold.

---

# 116. MVP Observability Model

Recommended:

```text
Runtime Observability
├── In-Memory Metrics
├── Structured Local Logs
├── ExecutionScope / ExecutionRevision / WorkItem / Attempt Trace
├── Bounded Recent Event Buffer
└── Runtime Diagnostic Snapshot
```

Remote export is optional.

---

# 117. MVP Required Correlation

Minimum:

```text
ApplicationInstanceId
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
OwnerModule
WorkType
```

Where applicable:

```text
BusinessExecutionPlanId
BusinessStageId
ExecutionBindingReference
RuntimeArtifactId
```

---

# 118. MVP Required Metrics

Minimum SHOULD include:

```text
execution_revision.created_total
execution_revision.superseded_total
execution_revision.failed_total
execution_revision.useful_latency_ms

workitem.created_total
workitem.admitted_total
workitem.failed_total
workitem.cancelled_total
workitem.abandoned_total

attempt.started_total
attempt.failed_total
attempt.abandoned_total
attempt.duration_ms

authority.validation_ms
authority.rejected_total
authority.stale_rejected_total

queue.depth
queue.wait_ms

execution_binding.duration_ms
execution_binding.failure_total
execution_binding.inflight

retry.approved_total
retry.exhausted_total
retry.recovery_escalation_total

cache.hit_total
cache.miss_total

candidate.rejected_total
artifact.published_total
artifact.active_count
artifact.active_bytes

business.accepted_total
business.rejected_total

lease.active_count
resource.draining_count
resource.disposal_failed_total

memory.process_bytes
resource.pressure_level

presentation.commit_ms
presentation.commit_rejected_total
```

---

# 119. MVP Required Logs

Structured logs SHOULD cover:

* startup failure;
* ExecutionScope startup/binding failure;
* WorkItem terminal failure;
* execution-binding initialization/auth failure;
* Retry exhaustion;
* recovery escalation failure;
* ownership transfer failure;
* Runtime Artifact publication failure;
* Business result rejection requiring attention;
* cleanup failure;
* Lease leak;
* invalid state transition;
* invariant violation;
* shutdown timeout.

---

# 120. MVP Required Trace

Representative successful trace:

```text
Business Intent / Observation
        |
        v
Business Plan
        |
        v
ExecutionRevision
        |
        v
Reuse Evaluation
        |
        v
WorkItem / Attempt
        |
        v
Execution Authority Validation
        |
        v
Ownership Transfer
        |
        v
Runtime Artifact Publication
        |
        v
Business Acceptance
        |
        v
Presentation
        |
        v
Visible Commit
```

Retry, Cancellation, Recovery and stale rejection appear as branches in the same causal trace where appropriate.

---

# 121. Testing Requirements

Tests SHOULD verify:

* correlation propagation;
* ExecutionScope/ExecutionRevision tracing;
* same WorkItem/new Attempt lineage;
* Retry vs Recovery/Fallback separation;
* authority accepted/rejected;
* Runtime Artifact publication success/failure;
* Business acceptance success/rejection;
* Presentation commit success/rejection;
* ownership transfer;
* Lease acquire/release/contention/leak;
* retention add/remove;
* orthogonal resource lifecycle observations;
* logical/physical disposal;
* stale disposition;
* cancellation not counted as Failure;
* stale not counted as WorkItem physical failure;
* Queue wait separated from execution;
* privacy redaction;
* bounded metric dimensions;
* bounded telemetry queue;
* non-blocking Snapshot;
* exporter failure not breaking Runtime;
* shutdown telemetry;
* no user content in standard signals.

---

# 122. Deterministic Tests

Use where practical:

* fake Clock;
* fake Execution Binding;
* manual Completion gate;
* manual authority gate;
* fake Runtime Artifact Store;
* fake Business acceptance gate;
* fake Lease manager;
* deterministic telemetry sink.

Assert timing boundaries such as:

```text
Queue Wait
Attempt Execution
Authority Validation
Ownership Transfer
Artifact Publication
Business Acceptance
Presentation Commit
Resource Drain
```

---

# 123. Endurance Tests

Verify:

* metric cardinality does not grow with IDs;
* trace/ring buffers remain bounded;
* logs remain bounded;
* telemetry Worker remains bounded;
* snapshots remain stable;
* Lease metrics reflect actual state;
* resource disposal metrics reflect actual lifecycle;
* no retention leak;
* draining resources remain bounded;
* observability overhead remains stable.

---

# 124. Architecture Invariants

1. Observability explains decisions, not only activity.

2. ExecutionScope/ExecutionRevision lifecycle is traceable.

3. WorkItem and Attempt identities remain distinct.

4. Retry creates another AttemptId.

5. Retry and alternative execution recovery remain distinct in telemetry.

6. Execution authority validation is observable.

7. Ownership transfer is observable.

8. Runtime Artifact publication is observable.

9. Business acceptance/rejection is observable where applicable.

10. Presentation commit/rejection is observable separately.

11. Runtime authority is not Presentation authority.

12. Candidate rejection is observable.

13. Lease lifecycle is observable.

14. Retention lifecycle is observable.

15. Resource ownership/visibility/retention/lease/disposal remain distinguishable.

16. Logical and physical disposal remain distinct.

17. Queue wait is separate from execution.

18. Cancellation is not counted as generic Failure.

19. Stale is an authority-rejection concept.

20. Stale is not canonical WorkItem terminal outcome.

21. Runtime Events are not automatically telemetry.

22. Event Bus does not depend on telemetry.

23. Standard telemetry contains no reading content.

24. Secrets are redacted before storage/export.

25. Metric cardinality remains bounded.

26. Runtime IDs are not aggregate labels.

27. Telemetry queue is bounded.

28. Telemetry failure does not break correctness.

29. Runtime Control is never blocked by export.

30. Snapshot contains no large payload.

31. Execution-binding degradation observation does not automatically change Provider Management health state.

32. Duplicate signals do not create duplicate logical outcome.

33. Fatal/invariant events receive elevated diagnostic retention.

34. Sampling preserves slow/failure/cleanup traces preferentially.

35. Observability overhead is itself measurable.

36. Cache/Storage/Runtime ownership is not conflated in telemetry.

37. Runtime Artifact publication does not imply Business acceptance.

38. Business acceptance does not imply Presentation commit.

39. Provider Fallback is not recorded as Retry strategy.

40. Presentation target validation does not masquerade as Runtime authority validation.

---

# 125. Recommended MVP

CRAI MVP SHOULD support:

* in-memory aggregate metrics;
* structured local logs;
* ExecutionScope/ExecutionRevision tracing;
* WorkItem/Attempt lineage;
* execution authority trace;
* Runtime Artifact publication trace;
* Business acceptance trace;
* Presentation trace;
* Retry trace;
* Recovery escalation trace;
* Queue/Scheduler metrics;
* execution-binding metrics;
* Cache metrics;
* Resource/Lease/Retention metrics;
* resource pressure metrics;
* bounded diagnostic Snapshot;
* bounded recent-event buffer;
* privacy-safe telemetry;
* startup/shutdown timelines.

MVP MAY defer:

* remote telemetry export;
* distributed tracing;
* persistent traces across restart;
* advanced tail-based sampling;
* provider-cost analytics;
* automatic support-bundle upload;
* GPU telemetry normalization across all platforms.

---

# 126. Open Decisions

The following remain open:

* OpenTelemetry adoption;
* trace persistence across restart;
* local log retention duration;
* support-bundle format;
* Runtime Snapshot export;
* anonymous operational telemetry in MVP;
* slow ExecutionRevision threshold;
* execution-binding cost estimate retention;
* lifecycle trace sampling rate;
* GPU telemetry portability;
* crash-recovery report;
* user-facing diagnostic metrics;
* Lease leak thresholds;
* allowed exported content-derived metadata;
* Business acceptance span conventions;
* Recovery/Routing telemetry contract;
* provider-specific telemetry layering.

---

# 127. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `ERROR_MODEL.md`
* `CACHE_POLICY.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `THREADING_MODEL.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `BOOT_SEQUENCE.md`
* `PROCESS_TOPOLOGY.md`

External:

* `../core/EVENT_BUS.md`
* `../ai/ROUTING.md`
* `../ai/FALLBACK.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`

---

# 128. Completion Criteria

`RUNTIME_OBSERVABILITY.md` is synchronized when:

* ExecutionScope/ExecutionRevision terminology is canonical;
* Business Execution Plan / WorkItem / Attempt trace remains explicit;
* physical outcome and authority outcome are separate;
* Runtime Artifact publication and Business acceptance are separate;
* Business acceptance and Presentation commit are separate;
* Retry and Recovery/Fallback telemetry are separate;
* stale is no longer a WorkItem terminal metric;
* authority metrics contain no generic commit-authority concept;
* execution-binding telemetry replaces Runtime dependence on ProviderProfile where appropriate;
* Provider health observations do not become canonical Provider state;
* Resource lifecycle is observed through orthogonal dimensions;
* ownership, visibility, retention and Lease remain distinct;
* privacy/cardinality/sampling rules remain explicit;
* telemetry failure does not affect Runtime correctness;
* MVP metrics/tests align with Runtime v2.

---

# 129. Summary

CRAI Runtime Observability uses:

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

Canonical Runtime correlation:

```text
Application
    |
    v
ExecutionScope
    |
    v
ExecutionRevision
    |
    v
WorkItem
    |
    v
Attempt
```

The useful-result observation chain is:

```text
Attempt Physically Completes
        |
        v
Execution Authority Validated
        |
        v
Runtime Artifact Published
        |
        v
Business Result Accepted
        |
        v
Presentation Target Validated
        |
        v
Visible Result Committed
```

Observability succeeds when CRAI can answer:

```text
what happened,
why it happened,
whether it still mattered,
which owner accepted or rejected it,
and which resource still keeps it alive.
```
