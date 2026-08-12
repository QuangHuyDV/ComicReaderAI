# Runtime Performance Model

* **Document:** Runtime Architecture / Performance Model
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI measures, budgets, protects and improves Runtime performance around useful current reading output.

CRAI is an interactive reading assistant.

Performance MUST NOT be evaluated only by:

* raw throughput;
* number of completed WorkItems;
* CPU utilization;
* provider median latency;
* isolated Business Module speed.

The primary question is:

```text
How quickly and predictably can CRAI deliver
a useful result that is still current,
execution-authorized,
business-valid where required,
and consumable by the user?
```

---

# 2. Scope

This document covers:

* Useful Result Latency;
* Time to First Useful Result;
* interaction responsiveness;
* observation latency;
* planning latency;
* WorkItem/Attempt latency;
* Scheduler admission;
* Queue wait;
* execution-authority validation;
* Runtime Artifact ownership transfer/publication;
* Business acceptance latency;
* Presentation commit latency;
* freshness;
* useful-work ratio;
* wasted work;
* Retry/recovery cost;
* cancellation efficiency;
* Resource Lease performance;
* resource lifecycle performance;
* cache/reuse value;
* provider/runtime execution performance;
* overload;
* graceful degradation;
* cold start;
* endurance;
* benchmarks;
* regression policy.

This document does NOT define:

* Business semantics;
* Provider selection;
* Fallback policy;
* exact Retry rules;
* quality thresholds;
* exact degradation policy;
* exact Scheduler algorithm;
* exact hardware requirements;
* provider pricing;
* implementation framework.

---

# 3. Performance Philosophy

CRAI prioritizes:

```text
Correct Current Result
        |
        v
Responsive Control / UI
        |
        v
Low Useful-Result Latency
        |
        v
Predictable Tail Latency
        |
        v
Stable Resource Usage
        |
        v
Predictable Recovery
        |
        v
Execution Cost Efficiency
        |
        v
Maximum Raw Throughput
```

Core rule:

```text
Optimize useful current output,
not maximum executed work.
```

---

# 4. Performance Is Not Authority

Performance optimization MUST NOT:

* bypass execution-authority validation;
* bypass Business result validation;
* bypass ownership transfer;
* bypass cancellation;
* bypass Privacy/Policy;
* bypass Artifact integrity;
* bypass Presentation target validation.

A faster invalid result is not a performance success.

---

# 5. Primary Performance Outcome

The primary end-to-end metric is:

```text
UsefulResultLatency
```

Conceptually:

```text
Current Source Becomes Processable
        |
        v
Business Intent / Plan Resolved
        |
        v
ExecutionRevision Created
        |
        v
Required Runtime Work
        |
        v
Execution Result Accepted
        |
        v
Runtime Artifact Published
        |
        v
Business Result Accepted
        |
        v
Presentation Ready
        |
        v
Visible Useful Result
```

Not every workflow requires every stage.

---

# 6. Useful Result

A result counts as useful only when all applicable conditions hold:

* still relevant to current business/user intent;
* execution authority accepted;
* required Runtime Artifact publication succeeded;
* required Business owner accepted semantics;
* required Presentation commit succeeded;
* result remains relevant when shown;
* output satisfies minimum usefulness contract.

---

# 7. Non-Useful Fast Results

The following do NOT count as useful success:

* stale Completion;
* rejected Runtime Artifact candidate;
* superseded ExecutionRevision result;
* Business-rejected output;
* Presentation commit rejected;
* result never consumed;
* output arriving after user moved away;
* output too incomplete to satisfy its declared partial/full result contract.

---

# 8. Performance Dimensions

Recommended:

```text
Performance
├── Responsiveness
├── Useful Latency
├── Freshness
├── Tail Predictability
├── Resource Stability
├── Execution Efficiency
├── Recovery
├── Cost Efficiency
└── Quality Preservation
```

---

# 9. Responsiveness

Measures how quickly CRAI reacts to:

* user interaction;
* cancellation;
* ExecutionRevision replacement;
* Runtime control operations;
* Presentation acknowledgment.

Responsiveness is distinct from full useful-result completion.

---

# 10. Useful Latency

Measures the time to produce a result that actually contributes to current user-visible value.

---

# 11. Freshness

Measures whether produced/visible output still corresponds to current execution/business intent.

---

# 12. Tail Predictability

P50 alone is insufficient.

Important paths SHOULD measure:

```text
P50
P90
P95
P99
```

where sample volume permits.

---

# 13. Resource Stability

Long-running CRAI operation should not progressively accumulate:

* memory;
* Runtime Artifacts;
* active Leases;
* Queue depth;
* draining resources;
* threads/contexts;
* provider requests;
* native/GPU resources.

---

# 14. Latency Categories

Recommended:

```text
InteractionLatency
ObservationLatency
PlanningLatency
ExecutionRevisionCreationLatency
WorkMaterializationLatency
AdmissionLatency
QueueWaitLatency
ExecutionStartLatency
AttemptExecutionLatency
ProviderExecutionLatency
CompletionDispatchLatency
AuthorityValidationLatency
OwnershipTransferLatency
ArtifactPublicationLatency
BusinessAcceptanceLatency
PresentationPreparationLatency
UiDispatchLatency
PresentationCommitLatency
UsefulResultLatency
RecoveryLatency
CleanupLatency
```

---

# 15. Interaction Latency

```text
User Action
    ->
Immediate UI/Application acknowledgment
```

This SHOULD remain very small regardless of slow provider/background execution.

---

# 16. Observation Latency

For capture/observation use cases:

```text
Source update
    ->
stable/processable source determination
```

Observation semantics remain owned by Capture/Observation architecture.

---

# 17. Planning Latency

```text
Validated Business Intent
    ->
BusinessExecutionPlan
```

This belongs to Business Pipeline Orchestration.

---

# 18. ExecutionRevision Creation Latency

```text
Accepted BusinessExecutionPlan
    ->
ExecutionRevision available
```

This is Runtime execution setup latency.

---

# 19. Work Materialization Latency

```text
Declared Business Stage runtime-ready
    ->
WorkItem / Attempt candidate available
```

---

# 20. Admission Latency

```text
Eligible candidate
    ->
Scheduler decision
```

---

# 21. Queue Wait

```text
ADMIT
    ->
dispatch
```

Queue wait MUST remain distinct from execution latency.

---

# 22. Attempt Execution Latency

```text
Attempt started
    ->
Completion reported
```

May include provider/native execution where applicable.

---

# 23. Authority Validation Latency

```text
Completion received
    ->
Runtime authority ACCEPT / REJECT
```

This SHOULD remain fast and control-path safe.

---

# 24. Ownership Transfer Latency

```text
Accepted candidate
    ->
Runtime Artifact Store owns candidate
```

---

# 25. Artifact Publication Latency

```text
ownership transfer accepted
    ->
RuntimeArtifactRef publicly available
```

---

# 26. Business Acceptance Latency

Where required:

```text
Accepted Runtime execution result
    ->
Owning Business Module accepts/rejects semantics
```

This MUST remain separate from Runtime authority validation.

---

# 27. Presentation Commit Latency

```text
Presentation commit-ready
    ->
visible Presentation state
```

Presentation target validation belongs to Presentation/Application.

---

# 28. Useful Result Latency

Conceptual:

```text
T_useful =
    T_observation?
  + T_planning
  + T_execution_revision
  + T_materialization
  + T_reuse_lookup?
  + T_admission
  + T_queue
  + T_attempt
  + T_authority_validation
  + T_ownership_transfer
  + T_artifact_publication
  + T_business_acceptance?
  + T_presentation
  + T_ui_dispatch
  + T_presentation_commit
```

Not every request uses every term.

---

# 29. Critical Path

Critical path is:

```text
the minimum dependency chain
required to create the current useful result
```

Runtime critical-path vocabulary SHOULD remain generic:

```text
Business Plan
    |
    v
ExecutionRevision
    |
    v
Required WorkItems
    |
    v
Accepted Runtime Results
    |
    v
Business-Accepted Result
    |
    v
Presentation
```

Do not hard-code OCR/Layout/provider-call internals into generic Runtime performance architecture.

---

# 30. Critical-Path Protection

Runtime SHOULD:

* preserve control capacity;
* prefer current eligible execution over obsolete execution;
* remove obsolete queued work quickly;
* revoke obsolete authority early;
* use bounded queues;
* use bounded concurrency;
* avoid UI/control blocking;
* limit provider/runtime saturation;
* reduce non-critical work under pressure;
* reuse compatible accepted results where valid.

---

# 31. Freshness Is Not Absolute Priority

Critical-path protection does NOT imply:

```text
current execution
    always outranks everything
```

Control operations such as:

* cancellation;
* shutdown;
* containment;
* critical lifecycle work

may outrank ordinary current Business work.

---

# 32. Provisional MVP Targets

Initial hypotheses MAY include:

| Operation                          |                                    Initial target |
| ---------------------------------- | ------------------------------------------------: |
| Immediate UI acknowledgment        |                                          < 100 ms |
| Runtime Control command handling   |                                   < 50 ms typical |
| Lightweight observation decision   |                                           < 50 ms |
| Scheduler decision                 |                                           < 50 ms |
| Authority validation               |                                   < 20 ms typical |
| Runtime Artifact publication       |                                   < 50 ms typical |
| Presentation commit after ready    |                                          < 100 ms |
| Cancellation authority propagation |                                          < 100 ms |
| Obsolete queued-work removal       |                                          < 100 ms |
| Warm reusable result path          |                          < 200 ms where realistic |
| Current useful result              | preferably around/below 2 s for interactive cases |

These are prototype hypotheses.

They are NOT product guarantees.

---

# 33. Target Review Rule

Targets MUST be revised using benchmark evidence.

A target SHOULD NOT become a correctness invariant.

---

# 34. Percentile Analysis

Important latency metrics SHOULD analyze:

```text
P50
P90
P95
P99
```

Tail causes MAY include:

* provider execution;
* Queue wait;
* Resource Lease contention;
* cold start;
* Retry;
* Runtime control contention;
* UI dispatch;
* Artifact publication;
* Business validation;
* cleanup/resource pressure.

---

# 35. WorkItem Timing Model

Possible timestamps:

```text
CreatedAt
EligibleAt
AdmittedAt
QueuedAt
DispatchedAt
AttemptStartedAt
ExternalExecutionStartedAt?
ExternalExecutionCompletedAt?
CompletionReportedAt
AuthorityValidatedAt
OwnershipTransferredAt
ArtifactPublishedAt
BusinessAcceptedAt?
PresentationReadyAt?
CommitRequestedAt?
PresentationCommittedAt?
CancellationRequestedAt?
LogicalDisposedAt?
PhysicalDisposedAt?
```

Not every WorkItem needs every timestamp.

---

# 36. Attempt Timing Breakdown

Attempt performance SHOULD distinguish:

```text
Queue Wait
Resource Wait
Lease Wait
Execution Time
Provider / Native Wait
Normalization Time
Completion Dispatch
Authority Validation
Attempt Cleanup
```

---

# 37. Work Outcome Funnel

Performance SHOULD distinguish:

```text
Materialized Work
Executed Work
Physically Completed Work
Execution-Accepted Work
Published Runtime Work
Business-Accepted Work
Presented Work
Useful Work
```

---

# 38. Materialized Work

Logical Runtime work was created.

This may still never execute.

---

# 39. Executed Work

At least one physical Attempt started.

---

# 40. Physically Completed Work

Attempt produced a physical terminal outcome.

---

# 41. Execution-Accepted Work

Runtime Control accepted the Completion under current execution authority.

---

# 42. Published Runtime Work

A Runtime Artifact/result became available through the Runtime publication boundary.

---

# 43. Business-Accepted Work

The owning Business Module accepted the result semantics where such acceptance is required.

---

# 44. Presented Work

Presentation/Application committed a result into current visible state.

---

# 45. Useful Work

The current user/use case actually benefits from the result.

---

# 46. Funnel Ratios

Useful ratios MAY include:

```text
Executed / Materialized
PhysicalCompleted / Executed
ExecutionAccepted / PhysicalCompleted
Published / ExecutionAccepted
BusinessAccepted / Published
Presented / BusinessAccepted
Useful / Presented
```

Not all pipelines require every denominator.

---

# 47. Useful Work Ratio

Recommended:

```text
UsefulWorkRatio =
    Useful Current Execution Cost
    /
    Total Executed Cost
```

Prefer cost/time-weighted ratios over simple WorkItem counts where feasible.

---

# 48. Wasted Work

Wasted execution MAY include:

* stale Attempt execution;
* canceled work finishing late;
* duplicate provider request;
* duplicate computation;
* Runtime Artifact published but never Business-accepted;
* Business result accepted but never presented;
* Presentation commit rejected;
* speculative result evicted before use;
* physical resource retained long after authority loss;
* Retry superseded before value;
* duplicate execution caused by poor coalescing.

Wasted work SHOULD be bounded and observable.

---

# 49. Freshness Metrics

Recommended:

```text
CurrentExecutionRevisionUsefulRatio
StaleCompletionRatio
AuthorityRejectionRatio
AverageObsoleteExecutionDuration
CancellationPropagationLatency
ExecutionRevisionChurn
VisibleExecutionRevisionLag
StableSourceToCurrentRevisionLag
```

---

# 50. Stale Is Not Physical Failure

Performance diagnostics SHOULD distinguish:

```text
Attempt completed quickly
but
Completion rejected stale
```

from genuine execution failure.

---

# 51. Authority Performance

Measure:

```text
AuthorityValidationLatency
DuplicateCompletionRejectionLatency
StaleRejectionLatency
CancellationAuthorityPropagationLatency
AuthorityConflictCount
LateResultRejectionCount
```

Presentation target validation is measured separately.

---

# 52. Runtime Artifact Publication Performance

Measure:

```text
CandidatePreparationLatency
OwnershipTransferLatency
ArtifactPublicationLatency
PublicationFailureCount
DuplicatePublicationRejectionCount
RejectedCandidateCleanupLatency
```

---

# 53. Business Acceptance Performance

Where applicable, measure:

```text
BusinessAcceptanceLatency
BusinessResultRejectionCount
BusinessRecoveryRequestCount
```

Performance Model does not define why a Business Module rejects a result.

---

# 54. Resource Lease Performance

Measure:

* acquisition delay;
* hold duration;
* contention;
* denied lease count;
* disposal blocked by lease;
* leaked lease count;
* lease lifetime by resource class.

Lease acquisition MUST remain bounded/cancelable where applicable.

---

# 55. Resource Lifecycle Performance

Measure:

```text
LogicalDisposal
    |
    v
Draining
    |
    v
PhysicalDisposal
```

Metrics:

```text
LogicalDisposalLatency
DrainingDuration
PhysicalDisposalLatency
CleanupRetryCount
CleanupFailureCount
ResourceLeakCount
NativeCleanupLatency
GpuCleanupLatency
ResourceHeldAfterAuthorityLoss
```

---

# 56. Unified Resource Pressure

Recommended categories:

```text
ResourcePressure
├── CPU
├── ManagedMemory
├── NativeMemory
├── GPU
├── ExecutionBinding
├── Queue
├── Lease
├── RuntimeArtifact
├── NativeHandle
├── TemporaryStorage
└── UI/Dispatcher
```

---

# 57. Pressure Levels

Recommended:

```text
NORMAL
ELEVATED
HIGH
CRITICAL
```

Performance Model measures pressure.

It does not itself mutate Business state.

---

# 58. Pressure Response Ownership

```text
Resource Manager
    measures resource state

Scheduler
    reduces admission

Runtime Control
    cancels/supersedes execution

Cache Policy
    releases retention

Provider Runtime
    adjusts/unloads runtime resources

Business/Application owner
    approves semantic/quality degradation where required
```

---

# 59. Queue Performance

Measure:

```text
depth by queue class
queue wait by WorkType/execution class
dispatch latency
replace count
obsolete removal count
saturation duration
current-execution ratio
control delay
dispatch failure
```

Queue does not own admission latency itself; Scheduler decision is measured separately.

---

# 60. Scheduler Performance

Measure:

```text
decision latency
ADMIT / DEFER / REJECT / REPLACE count
decision reason
fairness delay
current-eligible admission ratio
resource-pressure decision count
preemption recommendation latency
control-capacity availability
```

---

# 61. Capture / Observation Performance

CRAI-specific metrics MAY include:

* frame acquisition latency;
* callback delay;
* latest-value replacement count;
* dropped observation;
* stability decision latency;
* source fingerprint cost;
* capture CPU/GPU;
* ExecutionRevision churn caused by observation;
* no-change suppression ratio.

Goal is not processing every frame.

---

# 62. Business Module Performance

Every Business Module owns its semantic metrics.

Runtime MAY provide common dimensions such as:

```text
OwnerModule
WorkType
Operation
InputSizeClass
OutputSizeClass
ExecutionClass
ExecutionBindingClass
```

Runtime MUST NOT force every module into one stage taxonomy.

---

# 63. Provider Runtime Performance

Performance MAY observe resolved runtime execution dimensions such as:

```text
ProviderRuntime
ExecutionBinding
ModelDeployment?
Operation
ExecutionClass
Region?
ImplementationVersion
```

Use only dimensions available without exposing sensitive/high-cardinality data.

---

# 64. Provider Runtime Metrics

Possible:

* execution latency;
* remote queue latency;
* timeout;
* failure;
* rate-limit pressure;
* cold-start latency;
* payload-size class;
* estimated execution cost;
* cancellation support;
* abandoned physical duration;
* stale Completion ratio.

Fallback-selection metrics remain owned by Routing/Recovery observability.

---

# 65. Recovery Performance

Performance Model MAY measure:

```text
RecoveryEscalationLatency
NewBindingReadyLatency
UsefulResultRecoveryLatency
```

without deciding which Fallback route should be selected.

---

# 66. Cache / Reuse Performance

Measure more than hit rate:

```text
UsefulHitRatio
CompatibilityRejectCount
PolicyPartitionMiss
IntegrityFailure
PromotionCost
RetentionCost
EvictionCost
SavedUsefulLatency
SavedExecutionCost
InFlightCoalescingCount
DurableLookupLatency
```

---

# 67. Reuse Value

Conceptual:

```text
ReuseValue =
    AvoidedUsefulExecutionCost
  - LookupCost
  - CompatibilityValidationCost
  - RetentionCost
  - EvictionCost
```

---

# 68. Retry Performance

Measure:

```text
FirstAttemptLatency
RetryDelay
RetryAdmissionLatency
RetryQueueWait
RetryExecutionLatency
RetryRecoveryLatency
RetryBudgetExhaustion
ConcurrentRetryPressure
RetryCancelledByAuthorityChange
ReuseAvoidedRetryCount
```

Do not count Fallback selection as Runtime Retry performance.

---

# 69. Cancellation Performance

Measure:

```text
AuthorityRevocationLatency
QueuedRemovalLatency
WorkerAcknowledgmentLatency
PhysicalAbortLatency
GraceDuration
AbandonedAttemptCount
PostCancellationExecutionTime
ResourceDrainDuration
LateCompletionRejectionCost
```

---

# 70. UI / Presentation Performance

Measure:

* UI command acknowledgment;
* dispatcher delay;
* long UI task count;
* Presentation preparation latency;
* target validation latency;
* visible replacement latency;
* frame stutter;
* layout thrashing;
* repeated-loading duration.

Heavy processing remains outside UI Context.

---

# 71. Cold Start

Cold start SHOULD measure separately:

```text
ProcessStartLatency
ConfigurationReadyLatency
RuntimeCoreReadyLatency
ProviderRuntimeInitializationLatency
PluginActivationLatency?
LocalModelLoadLatency?
ApplicationReadyLatency
FirstUsefulResultLatency
```

Do not mix cold-start and steady-state distributions.

---

# 72. Provider Management Boundary

Do NOT measure:

```text
Provider Manager initialization
```

as a Runtime execution startup primitive.

Use:

```text
Provider Runtime Gateway / execution-binding initialization
```

where applicable.

---

# 73. Long-Running Stability

Endurance tests SHOULD verify:

* bounded managed/native/GPU memory;
* bounded Runtime Artifact count;
* bounded Lease count;
* bounded Queue depth;
* bounded thread/context count;
* bounded provider/runtime in-flight count;
* stable UI responsiveness;
* no accumulation of draining resources;
* cleanup continues to make progress;
* diagnostics remain bounded;
* Useful Result Latency does not drift materially.

---

# 74. Overload Definition

Runtime is overloaded when generated/admitted execution exceeds the system's ability to produce current useful output within acceptable resource/latency budgets.

Symptoms MAY include:

* rising Queue wait;
* increasing stale ratio;
* rising Useful Result Latency;
* growing memory/GPU pressure;
* Lease contention;
* provider/runtime saturation;
* draining accumulation;
* UI dispatch delays;
* cancellation occurring only after expensive execution;
* current useful work starvation.

---

# 75. Overload Response Principles

Performance Model does not directly execute response actions.

It defines goals:

1. preserve correctness;

2. preserve control path;

3. eliminate obsolete work early;

4. reduce non-critical admission;

5. reduce wasted execution;

6. free eligible low-value retention/resources;

7. preserve useful current work;

8. allow owner-approved quality degradation only when explicit.

---

# 76. Example Ownership of Overload Actions

```text
Stale/obsolete execution
    -> Runtime Control / Cancellation

Queue pressure
    -> Scheduler

Cache retention
    -> Cache Policy

Provider/model residency
    -> Provider Runtime

Capture pacing
    -> Capture owner

Input quality/resolution
    -> Business/Capability policy

Context-size reduction
    -> Translation/AI owner

UI simplification
    -> Presentation owner
```

Performance Model measures outcomes.

---

# 77. Graceful Degradation

Runtime MAY expose operational degradation levels such as:

```text
FULL
REDUCED
MINIMAL
CONTROL_ONLY
```

but exact semantic behavior belongs to owning components/policies.

---

# 78. Degradation Invariants

Any degradation MUST:

* preserve Runtime correctness;
* preserve Privacy/Security constraints;
* remain observable;
* be reversible where practical;
* stay within configured/owner-approved bounds;
* not silently change Business semantics.

---

# 79. Quality vs Performance

Performance changes MAY affect semantic quality.

Example:

```text
Lower input resolution
    ->
lower execution cost
    ->
possible Recognition degradation
```

or:

```text
Smaller AI context
    ->
lower cost/latency
    ->
possible consistency loss
```

The owning Business/AI capability determines whether degradation is permitted.

Performance Model only measures tradeoffs.

---

# 80. Performance Events

Possible normalized events:

```text
PerformancePressureChanged
PerformanceBudgetExceeded
WorkTypeSlow
ExecutionBindingSlow
QueueSaturated
StaleRatioHigh
DegradationEntered
DegradationExited
RuntimeColdStartDetected
RecoveryCompleted
AuthorityValidationSlow
ArtifactPublicationSlow
LeaseContentionHigh
ResourceDrainSlow
UsefulLatencyExceeded
```

Final names follow Event Standard.

---

# 81. Core Metrics

## End-to-End

```text
UsefulResultLatency
TimeToFirstUsefulResult
CurrentExecutionUsefulSuccessRatio
CurrentExecutionVisibleLatency
```

## Runtime

```text
WorkMaterializationLatency
SchedulerDecisionLatency
QueueWait
AttemptExecutionLatency
AuthorityValidationLatency
OwnershipTransferLatency
ArtifactPublicationLatency
BusinessAcceptanceLatency?
```

## Resources

```text
CPU
ManagedMemory
NativeMemory
GPU
Network
ProviderRuntimeInFlight
RuntimeArtifactBytes
LeaseCount
DrainingResourceCount
NativeHandleCount
WorkerUtilization
```

## User Experience

```text
UIAckLatency
UIDispatchLatency
UILongTask
PresentationReplacementLatency
LoadingDuration
StaleContentVisibility
```

---

# 82. Metric Dimensions

Prefer bounded low-cardinality dimensions:

```text
OwnerModule
WorkType
Operation
ExecutionClass
ExecutionBindingClass
CacheStatus
PhysicalOutcome
AuthorityOutcome
CancellationReason
DeviceProfile
ExecutionRevisionState
PressureLevel
```

Avoid raw IDs in aggregate metrics.

---

# 83. Trace Correlation

Use trace/log correlation for:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
```

These should generally not become metric labels.

---

# 84. End-to-End Trace

A representative trace MAY be:

```text
Observation / User Intent
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
WorkItem
        |
        v
Attempt
        |
        v
Completion
        |
        v
Runtime Authority Validation
        |
        v
Runtime Artifact Publication
        |
        v
Business Acceptance
        |
        v
Presentation Commit
```

---

# 85. Trace Spans

Useful spans MAY include:

* observation;
* planning;
* materialization;
* cache/reuse;
* Scheduler;
* Queue;
* Lease wait;
* Attempt execution;
* provider/native execution;
* cancellation;
* authority validation;
* ownership transfer;
* publication;
* Business acceptance;
* Presentation;
* resource disposal.

---

# 86. Benchmark Classes

Recommended:

```text
Microbenchmark
WorkType Benchmark
Authority Benchmark
Publication Benchmark
Lease Benchmark
Resource Lifecycle Benchmark
Scheduler/Queue Benchmark
End-to-End Benchmark
Stress Benchmark
Endurance Benchmark
Provider Runtime Benchmark
Cold-Start Benchmark
```

---

# 87. Benchmark Inputs

Representative CRAI benchmark corpus MAY include:

* simple comic page;
* dense comic page;
* Chinese vertical text;
* Chinese horizontal text;
* stylized text;
* low contrast;
* high-resolution image;
* rapid scroll;
* repeated source;
* partial viewport change;
* large document section;
* delayed remote provider;
* local model cold start;
* memory/GPU pressure.

---

# 88. Controlled Testing

Tests SHOULD control where possible:

* source input;
* source-change timing;
* ExecutionRevision creation timing;
* provider/runtime delay;
* Completion order;
* Cache state;
* Queue state;
* concurrency;
* cancellation timing;
* UI dispatch;
* resource pressure;
* Retry;
* cleanup delay;
* Business acceptance delay.

Use deterministic Clock/fake adapters/providers where practical.

---

# 89. Regression Policy

A performance regression exists when a change materially worsens one or more protected dimensions such as:

* Useful Result Latency;
* tail latency;
* stale ratio;
* authority-validation latency;
* Artifact publication latency;
* Business acceptance latency;
* Queue wait;
* CPU/memory/GPU use;
* Lease contention;
* provider execution count/cost;
* UI responsiveness;
* endurance stability;
* cleanup/drain latency.

Thresholds require measured baselines.

---

# 90. Optimization Workflow

```text
Measure
    |
    v
Find Critical Bottleneck
    |
    v
Form Hypothesis
    |
    v
Change One Variable
    |
    v
Benchmark
    |
    v
Compare:
    Useful Latency
    Resource Cost
    Quality
    Correctness
    |
    v
Keep / Revert
```

---

# 91. Premature Optimization Policy

MVP SHOULD avoid without evidence:

* complex custom Scheduler;
* custom allocator;
* aggressive pooling;
* unnecessary multi-process execution;
* distributed Cache;
* speculative execution;
* provider racing;
* fine-grained recomputation graph;
* adaptive routing without baseline;
* hardware-specific low-level tuning.

---

# 92. MVP Performance Strategy

Recommended:

```text
Protect Control Path
+
Prefer Current Eligible Work
+
Low Bounded Concurrency
+
Latest-Value Observation Where Declared
+
Compatible Result Reuse
+
Bounded Provider Runtime Execution
+
Atomic Runtime Artifact Publication
+
Atomic Presentation Replacement
```

---

# 93. MVP Primary Goals

1. UI remains responsive.

2. Runtime Control remains responsive.

3. Capture/Observation does not wait synchronously for downstream execution.

4. Obsolete queued work is removed quickly.

5. Late results cannot become useful output.

6. Compatible accepted results are reused.

7. Memory/native/GPU resource use stabilizes.

8. Provider execution stays bounded.

9. Performance telemetry reveals where latency occurs.

10. Authority/publication overhead stays small.

11. Resource draining does not accumulate.

12. Business acceptance cost is observable where relevant.

---

# 94. Protection Classes

Instead of one total priority ordering, Performance Model defines protected classes:

```text
CONTROL PATH
CURRENT USEFUL EXECUTION
RUNTIME ACCEPTANCE / PUBLICATION
PRESENTATION DELIVERY
BACKGROUND / MAINTENANCE
SPECULATIVE WORK
```

Scheduler defines actual admission ordering.

---

# 95. Control Path

Includes:

* cancellation;
* Runtime shutdown;
* ExecutionRevision replacement;
* Completion processing;
* fatal containment;
* critical lifecycle operations.

This path MUST retain capacity under load.

---

# 96. Current Useful Execution

Represents currently relevant execution expected to contribute to useful user-visible output.

It is strongly protected from obsolete/background work.

It is not an absolute global priority over Control.

---

# 97. MVP Concurrency Guidance

Initial conservative limits MAY resemble:

| Runtime execution class    | Initial conceptual concurrency |
| -------------------------- | -----------------------------: |
| Capture source             |                              1 |
| Observation serial context |                              1 |
| CPU-heavy execution        |                              1 |
| Remote/provider binding    |                              1 |
| GPU/native serial          |                              1 |
| Presentation commit        |                              1 |
| Maintenance                |                 1 low-priority |

Exact values belong to `RUNTIME_CONFIG.md`.

---

# 98. Performance Diagnostics View

Development diagnostics SHOULD expose at least:

```text
Current ExecutionScope
Current ExecutionRevision
Useful Result Latency
WorkItem Timing
Attempt Timing
Queue Depth / Wait
Provider Runtime In-Flight
Reuse Status
Authority Validation
Runtime Artifact Publication
Business Acceptance?
CPU / Memory / GPU
Lease Count
Draining Resources
Stale Completion
Cancellation
```

---

# 99. Example — Normal Execution

```text
Stable Current Source
        |
        v
Business Plan
        |
        v
ExecutionRevision
        |
        v
Attempt Executed
        |
        v
Completion
        |
        v
Runtime Authority Accepted
        |
        v
Runtime Artifact Published
        |
        v
Business Result Accepted
        |
        v
Presentation Committed
```

Useful latency spans the applicable full chain.

---

# 100. Example — Reuse Hit

```text
ReuseQuery
        |
        v
Compatible Accepted Result Found
        |
        v
Runtime Relevance Validated
        |
        v
Owning Contract Accepts Reuse
        |
        v
Presentation / Downstream Use
```

No new Attempt is created when execution is unnecessary.

---

# 101. Example — Rapid Scrolling

```text
ExecutionRevision A running
        |
        v
ExecutionRevision B becomes current
        |
        v
A authority revoked
        |
        v
A queued work removed
        |
        v
B receives admission preference
        |
        v
A late Completion rejected
```

Success means:

* Queue does not accumulate obsolete work;
* B begins quickly;
* control/UI remain responsive.

---

# 102. Example — Slow Provider Runtime

```text
ExecutionBinding latency rises
        |
        v
Runtime pressure projection rises
        |
        v
Scheduler reduces admission
        |
        v
Background work decreases
        |
        v
Current useful work remains protected
```

Routing/Recovery MAY independently choose another binding.

Performance Model does not.

---

# 103. Example — Resource Pressure

```text
Pressure = HIGH
        |
        +--> Scheduler reduces admission
        |
        +--> Cache Policy releases low-value retention
        |
        +--> Runtime Control drains obsolete execution
        |
        +--> Provider Runtime unloads eligible idle resources
        |
        v
Useful current work remains protected
```

---

# 104. Architecture Invariants

1. UI responsiveness is more important than raw throughput.

2. Current useful work is preferred over obsolete work.

3. Current execution preference is not absolute over Control.

4. Stale Completion does not count as useful throughput.

5. Queue and concurrency remain bounded.

6. Control path retains capacity.

7. Capture/Observation does not queue unbounded source updates.

8. Provider/runtime requests remain bounded.

9. Performance optimization does not bypass execution authority.

10. Performance optimization does not bypass ownership transfer.

11. Performance optimization does not bypass Business compatibility/acceptance.

12. Performance optimization does not bypass Privacy/Security.

13. Cache is optional for correctness.

14. Resource growth is bounded.

15. Tail latency is measured.

16. Queue wait and execution latency are separate.

17. Authority-validation latency is measurable.

18. Runtime Artifact publication latency is measurable.

19. Business acceptance latency is measurable where relevant.

20. Ownership-transfer latency is measurable.

21. Lease wait is bounded/observable.

22. Logical disposal is measurable.

23. Physical disposal is measurable.

24. Draining resources are observable.

25. Useful work excludes authority-rejected output.

26. Useful work excludes Business-rejected output.

27. Useful work excludes rejected visible commit.

28. Quality degradation is explicit and owner-approved.

29. Overload response begins before process instability.

30. Background work does not block critical path.

31. Resource pressure does not independently change Business semantics.

32. Provider median latency alone is insufficient for routing decisions.

33. Long-running stability is a performance requirement.

34. Aggregate metrics avoid raw execution IDs.

35. Performance telemetry contains no user content by default.

36. ExecutionScope/ExecutionRevision terminology is canonical.

37. Provider Management is not the owner of provider-runtime execution performance.

38. Fallback is not Runtime Retry performance.

39. Performance Model measures decisions; it does not become Scheduler/Recovery policy.

40. Runtime Artifact publication and Business acceptance remain separate performance boundaries.

---

# 105. Recommended MVP

CRAI MVP SHOULD support:

* Useful Result Latency;
* Time to First Useful Result;
* current ExecutionRevision freshness metrics;
* WorkItem/Attempt timing;
* Scheduler/Queue timing;
* execution-authority timing;
* Runtime Artifact ownership/publication timing;
* Business acceptance timing where applicable;
* Presentation latency;
* Retry/cancellation timing;
* cache reuse value;
* provider-runtime latency;
* managed/native/GPU resource metrics;
* Lease metrics;
* draining-resource metrics;
* cold-start measurement;
* endurance testing;
* P50/P95/P99 where sample count permits;
* content-safe traces.

MVP MAY defer:

* automated performance tuning;
* adaptive concurrency;
* predictive cost models;
* advanced power-mode tuning;
* automated quality degradation;
* distributed performance analysis;
* sophisticated multi-device hardware profiles.

---

# 106. Open Decisions

The following remain open:

* acceptable Useful Result Latency by use case;
* Time to First Useful Result target;
* minimum hardware profile;
* Capture frequency;
* observation stability delay;
* provider/runtime mix;
* local vs remote execution strategy;
* partial result MVP inclusion;
* cache memory budget;
* provider timeout;
* adaptive routing;
* performance/power profiles;
* overlay vs side-panel presentation budgets;
* Lease timeout policy;
* Runtime Artifact publication target;
* Business acceptance budget;
* endurance benchmark duration;
* representative benchmark corpus size.

---

# 107. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `BUSINESS_PIPELINE_ORCHESTRATION.md`
* `RUNTIME_COMPONENTS.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `CACHE_POLICY.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `THREADING_MODEL.md`
* `ERROR_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`
* `BOOT_SEQUENCE.md`
* `PROCESS_TOPOLOGY.md`

External:

* `../ai/ROUTING.md`
* `../ai/FALLBACK.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`

---

# 108. Completion Criteria

`PERFORMANCE_MODEL.md` is synchronized when:

* Useful Result Latency remains the primary end-to-end metric;
* ExecutionScope/ExecutionRevision terminology is canonical;
* Runtime authority acceptance and Business acceptance are separate;
* Runtime Artifact publication and Presentation commit remain separate;
* generic `commit` terminology is avoided;
* Work outcome funnel distinguishes physical, execution, business and user-visible value;
* stale/wasted work remains measurable;
* Scheduler/Queue/Retry/Cancellation ownership remains external;
* Fallback is not treated as Retry performance;
* Provider Runtime replaces Provider Manager in Runtime metrics;
* overload response ownership is explicit;
* degradation does not silently change Business semantics;
* Cache is measured by useful value, not raw hit rate;
* Lease/resource lifecycle performance remains first-class;
* long-running stability remains a performance requirement;
* metrics/traces remain privacy-safe.

---

# 109. Summary

CRAI Performance Model follows:

```text
Current Intent / Source
        |
        v
Bounded Planning + Runtime Work
        |
        v
Execution-Accepted Result
        |
        v
Published Runtime Artifact
        |
        v
Business-Accepted Result
        |
        v
Current Visible Result
```

The central principle is:

```text
Fast execution is not enough.

Useful performance means:
the right result,
still current,
accepted by the right owner,
delivered quickly,
without destabilizing the Runtime.
```
