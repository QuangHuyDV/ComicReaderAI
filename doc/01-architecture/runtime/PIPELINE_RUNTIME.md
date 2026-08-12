# Pipeline Runtime

* **Document:** Runtime Architecture / Pipeline Runtime
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI Runtime accepts an immutable `BusinessExecutionPlan` and executes it using controlled Runtime concepts:

* `ExecutionScope`;
* `ExecutionRevision`;
* `WorkItem`;
* `Attempt`;
* Runtime Artifacts;
* completion reporting;
* execution-authority validation;
* accepted execution outcome;
* stage-runtime readiness;
* cancellation;
* Retry;
* cleanup.

This document is the canonical source for Runtime execution vocabulary.

It does NOT define:

* business pipeline planning;
* Recognition/OCR semantics;
* Translation semantics;
* Presentation semantics;
* provider/model selection;
* Plugin implementation;
* UI framework;
* concrete process topology.

---

# 2. Architectural Position

```text
Business Request
        |
        v
Business Pipeline Orchestration
        |
        v
BusinessExecutionPlan
        |
        v
Pipeline Runtime
        |
        v
Runtime Control
        |
        v
Scheduler / Work Queue / Worker
        |
        v
Public Business Module Contract
        |
        v
Attempt Result
        |
        v
Execution Authority Validation
        |
        v
Accepted Execution Result
        |
        v
Owning Business Module
        |
        v
Business Validation / Commit
        |
        v
Declared Downstream Stage Readiness
```

Pipeline Runtime answers:

```text
How is a declared BusinessExecutionPlan
executed while preserving execution authority,
cancellation, Retry, stale protection,
resource safety and ownership boundaries?
```

---

# 3. Core Separation

CRAI distinguishes:

```text
Business Pipeline Orchestration
    -> decides WHAT business work exists

Pipeline Runtime
    -> executes the declared plan

Business Module
    -> decides WHAT each result means
       and whether it is business-valid
```

Runtime does not know implementation details such as:

* OCR algorithm;
* layout analysis;
* segmentation;
* Translation strategy;
* Prompt design;
* rendering algorithm.

Runtime understands only the public execution contracts.

---

# 4. Canonical Runtime Hierarchy

```text
ApplicationInstance
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

Optional business correlation may include:

```text
ReadingSessionId
ProjectId
DocumentId
RequestId
```

These identities do not replace Runtime identities.

---

# 5. ExecutionScope

An `ExecutionScope` is a Runtime execution boundary.

It may correspond to execution for:

* one Reading Session;
* one manual translation operation;
* one export operation;
* another application use case.

Recommended identity:

```text
ExecutionScopeId
```

ExecutionScope may own Runtime metadata such as:

* current ExecutionRevision;
* cancellation scope;
* runtime priority context;
* configuration reference;
* Runtime Artifact ownership;
* execution correlation.

---

# 6. ExecutionScope vs ReadingSession

Critical distinction:

```text
ReadingSession
    = business/domain concept

ExecutionScope
    = Runtime execution concept
```

A Reading Session MAY have an associated ExecutionScope.

Runtime MUST NOT redefine Reading Session business lifecycle.

---

# 7. ExecutionRevision

`ExecutionRevision` represents one immutable generation of execution intent inside an ExecutionScope.

Recommended:

```text
ExecutionRevision
├── executionRevisionId
├── executionScopeId
├── businessPlanId
├── planDefinitionVersion
├── runtimeConfigurationSnapshotId
├── sourceIdentityReference?
├── createdAt
├── authorityState
└── metadata
```

---

# 8. ExecutionRevision Is Not Domain Revision

```text
ExecutionRevision
    = runtime freshness / execution authority
```

It is NOT:

```text
TranslationRevision
CharacterRevision
ProfileRevision
```

Those belong to Domain/Business architecture.

---

# 9. ExecutionRevision Identity

ExecutionRevision identity is immutable.

If business intent changes:

```text
ExecutionRevision A
    remains unchanged

ExecutionRevision B
    is created
```

---

# 10. ExecutionRevision Lifecycle

Recommended:

```text
CREATED
    |
    v
CURRENT
    |
    +--> SUPERSEDED
    |
    +--> CANCELLED
    |
    v
DRAINING
    |
    v
DISPOSED
```

Failure of individual WorkItems does not necessarily make the whole ExecutionRevision invalid.

---

# 11. Current ExecutionRevision

An ExecutionScope SHOULD have at most one current ExecutionRevision for one mutually exclusive execution lineage.

Current means:

```text
eligible for current execution acceptance
```

It does NOT automatically mean:

```text
highest Scheduler priority in all cases
```

---

# 12. Scheduler Priority Boundary

Current work normally outranks obsolete work.

However Scheduler still considers:

* control priority;
* user-interactive priority;
* deadline;
* Retry class;
* background work;
* resource pressure;
* shutdown/drain state.

Therefore:

```text
CURRENT
    !=
absolute highest priority
```

---

# 13. Superseded ExecutionRevision

An ExecutionRevision becomes superseded when another accepted execution intent replaces it.

Possible causes:

* source changed;
* user intent changed;
* BusinessExecutionPlan replaced;
* required business configuration changed;
* reusable input became invalid.

A superseded revision may still have running Attempts while draining.

Its late results have no current execution authority.

---

# 14. WorkItem

A `WorkItem` is one logical Runtime unit created to execute a declared part of the BusinessExecutionPlan.

Recommended:

```text
WorkItem
├── workItemId
├── executionScopeId
├── executionRevisionId
├── businessStageId
├── workType
├── handlerReference
├── dependencyReferences[]
├── inputArtifactRefs[]
├── businessConfigurationReferences[]
├── runtimeConfigurationSnapshotId
├── priority
├── deadline?
├── cancellationScope
└── correlationContext
```

---

# 15. WorkItem Rules

1. WorkItem identity is stable.

2. Retry does not clone the WorkItem.

3. Retry creates another Attempt.

4. WorkItem contains no large payload.

5. WorkItem contains no raw secret.

6. WorkItem does not invoke downstream business work itself.

7. WorkItem does not commit Domain or UI state.

8. WorkItem may have several Attempt records.

9. WorkItem accepts at most one final logical execution outcome.

---

# 16. WorkItem Lifecycle

Recommended:

```text
CREATED
    |
    v
PENDING
    |
    v
ADMITTED
    |
    v
QUEUED
    |
    v
RUNNING
    |
    v
COMPLETION_REPORTED
    |
    v
OUTCOME_ACCEPTED
```

Possible terminal accepted logical outcomes:

```text
SUCCEEDED
FAILED
CANCELLED
ABANDONED
```

---

# 17. STALE Is Not Physical WorkItem Success/Failure

`STALE` SHOULD normally be represented as:

```text
Completion Rejection Reason
```

or:

```text
Execution Authority Rejection
```

rather than as a physical execution outcome.

Example:

```text
Attempt:
    COMPLETED

Completion:
    REJECT_STALE
```

---

# 18. Attempt

An `Attempt` is one physical execution attempt for one WorkItem.

Recommended:

```text
Attempt
├── attemptId
├── workItemId
├── executionScopeId
├── executionRevisionId
├── attemptNumber
├── executionBindingReference
├── runtimeConfigurationSnapshotId
├── startedAt
├── deadline?
└── executionContextReference
```

---

# 19. Attempt Lifecycle

```text
CREATED
    |
    v
STARTED
    |
    v
RUNNING
    |
    +--> COMPLETED
    |
    +--> FAILED
    |
    +--> CANCELLED
    |
    +--> ABANDONED
```

Attempt state is physical execution state.

---

# 20. Attempt Rules

* each Retry creates a new AttemptId;
* an old Attempt is never resumed;
* late Attempt completion cannot overwrite accepted WorkItem outcome;
* every Completion passes execution-authority validation;
* speculative execution is outside MVP;
* multiple Attempts may exist for one WorkItem;
* only one logical WorkItem outcome is accepted.

---

# 21. Execution Binding

An Attempt MAY reference:

```text
ExecutionBindingReference
```

Examples:

* built-in implementation;
* plugin capability provider;
* provider deployment;
* AI RoutePlan result;
* Recognition engine binding.

Runtime consumes this binding.

Runtime MUST NOT independently reinterpret business/provider policy.

---

# 22. Retry vs Fallback

Critical distinction:

```text
Retry
    = same WorkItem
      + compatible execution binding
      + new Attempt
```

```text
Fallback
    = different execution route/binding
      selected by owning routing/recovery architecture
```

Fallback MAY produce a new Attempt.

But:

```text
Pipeline Runtime
    does not choose the fallback
```

---

# 23. Provider Fallback Rule

Do NOT define:

```text
provider fallback
    = automatically another Runtime Attempt
```

as a Runtime-owned rule.

Correct flow:

```text
Attempt Failure
        |
        v
Owning Routing / Recovery Policy
        |
        v
new ExecutionBindingReference
        |
        v
Runtime creates another Attempt
```

---

# 24. Runtime Artifact

A Runtime Artifact is an immutable execution payload published into the Runtime Artifact Store.

Recommended:

```text
RuntimeArtifact
├── artifactId
├── artifactType
├── producerWorkItemId
├── producerAttemptId
├── executionRevisionId
├── semanticReference?
├── versionMetadata
├── ownershipMetadata
├── retentionMetadata
└── backingResourceReference
```

---

# 25. Runtime Artifact vs Domain Artifact

```text
RuntimeArtifact
    = execution payload
```

It is NOT automatically:

```text
TranslationRevision
GlossarySnapshot
CharacterRecord
canonical SourceDocument history
```

Those are Business/Domain-owned resources.

---

# 26. Completion

`Completion` is the report submitted after an Attempt physically ends.

Recommended:

```text
AttemptCompletion
├── executionScopeId
├── executionRevisionId
├── workItemId
├── attemptId
├── physicalOutcome
├── temporaryArtifactReference?
├── normalizedErrorReference?
├── timingMetadata
└── executionBindingReference
```

Completion does not mutate Runtime state by itself.

---

# 27. Completion vs Accepted Outcome

Critical distinction:

```text
Attempt physically finished
    !=
Completion accepted
```

and:

```text
Completion accepted by Runtime
    !=
Business result accepted by owning module
```

---

# 28. Execution Flow

Recommended:

```text
BusinessExecutionPlan Accepted
        |
        v
ExecutionScope Bound
        |
        v
ExecutionRevision Created
        |
        v
Declared Stage Dependencies Evaluated
        |
        v
WorkItem Materialized
        |
        v
Scheduler Admission
        |
        v
Bounded Work Queue
        |
        v
Worker Executes Attempt
        |
        v
Public Module / Adapter Contract
        |
        v
Temporary Result
        |
        v
Completion Reported
        |
        v
Execution Authority Validation
        |
        +--> Reject
        |
        +--> Accept Execution Result
                    |
                    v
             Runtime Artifact Publication
                    |
                    v
             Owning Business Module
                    |
                    v
             Business Validation / Commit
                    |
                    v
             Business Stage Satisfied
                    |
                    v
             Declared Downstream Stage Ready
```

---

# 29. Runtime Control Ownership

Runtime Control is the logical authority for execution-orchestration state.

It MAY own:

```text
current ExecutionRevision
WorkItem logical state
Attempt lineage
accepted execution outcome
execution authority
cancellation authority
Retry lineage
plan/execution replacement
runtime shutdown coordination
```

---

# 30. Runtime Control Is Not Owner of All Runtime State

Runtime Control does NOT own:

* Scheduler admission state;
* Queue position;
* physical Resource lifecycle;
* Artifact backing storage;
* Provider Health;
* Plugin lifecycle;
* Runtime Configuration source state;
* Business result correctness;
* Presentation local state;
* telemetry state.

---

# 31. Scheduler Interaction

Pipeline Runtime requests Scheduler admission.

Scheduler decides:

```text
ADMIT
DEFER
REJECT
REPLACE
```

Scheduler MUST NOT:

* change BusinessExecutionPlan;
* add/remove required Business Stages;
* create Retry decisions;
* create Fallback decisions;
* accept terminal WorkItem outcome;
* change ExecutionRevision authority.

---

# 32. Queue Interaction

After admission, Attempt execution may enter a bounded Work Queue.

Queue entry SHOULD contain lightweight references only:

```text
WorkItemRef
AttemptRef
Priority
DependencyRuntimeState
CancellationReference
ArtifactRefs
RuntimeConfigurationSnapshotId
Deadline?
```

---

# 33. Queue Forbidden Payloads

Queue MUST NOT contain:

* image buffers;
* full OCR text payload;
* full Translation output;
* provider SDK response objects;
* raw secrets;
* mutable Business objects.

---

# 34. Worker Execution

Worker owns physical Attempt execution.

Worker MAY:

* acquire Artifact leases;
* invoke public capability/module contract;
* cooperate with cancellation;
* create temporary output;
* normalize provider/module failure;
* submit Completion;
* release leases;
* cleanup temporary resources.

---

# 35. Worker Restrictions

Worker MUST NOT:

* mutate Runtime Control state;
* schedule downstream Business Stages;
* perform orchestration-level Retry;
* choose Fallback route;
* directly accept terminal outcome;
* directly commit Domain state;
* directly commit Presentation/UI;
* publish accepted Artifact before Runtime authority validation.

---

# 36. Business Module Invocation

Runtime invokes the owning Business Module through its public contract.

Example:

```text
Worker
    |
    v
Recognition Public Contract
    |
    v
Recognition Execution Result
```

Runtime does not know whether Recognition internally uses:

* OCR;
* Layout;
* Reading Order;
* AI model;
* Plugin implementation;
* local/native provider.

---

# 37. Business Module Result

A Business Module execution may return:

```text
execution result
```

but Runtime MUST distinguish:

```text
technical invocation succeeded
```

from:

```text
business result accepted
```

where the module contract requires semantic validation/commit.

---

# 38. Execution Authority Validation

Runtime Control validates whether a Completion is still eligible to influence current Runtime execution.

Possible inputs:

```text
ExecutionScope active?
ExecutionRevision current/eligible?
WorkItem already terminal?
Attempt lineage valid?
Cancellation revoked?
RuntimeConfigurationSnapshot compatible?
Result identity valid?
Artifact candidate intact?
Duplicate Completion?
Superseded?
```

---

# 39. Authority Decisions

Recommended:

```text
ACCEPT
REJECT_STALE
REJECT_CANCELLED
REJECT_DUPLICATE
REJECT_INVALID_STATE
REJECT_INTEGRITY
```

---

# 40. Authority Is Runtime Freshness

Execution authority answers:

```text
May this result still influence current execution?
```

It does NOT answer:

```text
Is the Translation semantically correct?
Is the Recognition result valid?
Should the UI display this representation?
```

---

# 41. Accepted Execution Result

When Runtime accepts a Completion:

```text
Completion
    |
    v
Authority Validation
    |
    v
AcceptedExecutionResult
```

The result may then be published as a Runtime Artifact.

---

# 42. Runtime Artifact Publication

Recommended:

```text
Temporary Output
        |
        v
Artifact Candidate
        |
        v
Execution Authority Validation
        |
        v
Atomic Runtime Artifact Publication
        |
        v
Accepted ArtifactRef
```

Published Runtime Artifacts are immutable.

---

# 43. Publication Boundary

Runtime Artifact publication means:

```text
execution payload accepted and retained
```

It does NOT automatically mean:

```text
Domain state committed
Presentation committed
Storage persisted
```

---

# 44. Business Validation / Commit

After Runtime acceptance, the owning Business Module decides whether the accepted execution result satisfies its business contract.

Recommended:

```text
AcceptedExecutionResult
        |
        v
Owning Business Module
        |
        +--> ACCEPT_BUSINESS_RESULT
        |
        +--> REJECT_BUSINESS_RESULT
        |
        +--> REQUEST_RECOVERY
```

Exact contract belongs to the owning module.

---

# 45. Stage Completion

A Business Stage becomes logically satisfied only after the required business output has been accepted according to its owner contract.

Critical rule:

```text
Worker returned
    !=
Stage complete
```

and:

```text
Runtime authority accepted
    !=
Stage business-valid
```

---

# 46. Downstream Stage Readiness

Correct progression:

```text
Business Stage Output Accepted
        |
        v
Runtime updates declared dependency state
        |
        v
Declared downstream BusinessStagePlan ready
        |
        v
Runtime Control materializes WorkItem(s)
        |
        v
Scheduler admission
```

---

# 47. Runtime May Advance Declared Graph

Pipeline Runtime MAY automatically advance through the already-accepted BusinessExecutionPlan.

It may determine:

```text
which declared stage has all dependencies satisfied
```

It MUST NOT invent a Business Stage not present in the plan.

---

# 48. Dynamic Business Decision

If an execution result requires another business decision not represented by the plan:

```text
Runtime
    |
    v
Application / Business Orchestrator
    |
    v
Replan
```

Runtime MUST NOT silently extend the graph.

---

# 49. Retry Boundary

Recommended:

```text
Attempt 1 Failed
        |
        v
Runtime verifies WorkItem still relevant
        |
        v
Retry Policy evaluates
        |
        +--> no retry
        |
        +--> retry allowed
                    |
                    v
               Attempt 2
                    |
                    v
             Scheduler Admission
```

---

# 50. Retry Identity

Retry preserves:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
```

and creates:

```text
new AttemptId
```

---

# 51. Retry Configuration

Retry execution uses the immutable Runtime Configuration identity applicable to the retry policy.

If Retry is allowed to use a newer Runtime snapshot, that activation rule MUST be explicit.

---

# 52. Cancellation Boundary

Cancellation begins by revoking execution authority.

Recommended:

```text
Cancellation Requested
        |
        v
Authority Revoked
        |
        v
Queued Work Removed
        |
        v
Running Attempts Signaled
        |
        v
Late Completion Rejected
        |
        v
Resources Drain
```

---

# 53. Cancellation Is Not Failure

```text
CANCELLED
    !=
FAILED
```

unless the owning business contract explicitly maps cancellation to another higher-level outcome.

---

# 54. Stale Completion

A Completion is stale when it belongs to execution no longer eligible to affect current output.

Examples:

* ExecutionRevision superseded;
* plan replaced;
* Attempt no longer valid;
* source identity changed;
* execution target changed;
* ExecutionScope ended.

---

# 55. Stale Is an Authority Outcome

Recommended:

```text
Attempt physical outcome:
    COMPLETED

Runtime authority decision:
    REJECT_STALE
```

This preserves the distinction between physical execution and logical acceptance.

---

# 56. Replacement Flow

```text
ExecutionRevision A CURRENT
        |
        v
Business intent changes
        |
        v
BusinessExecutionPlan B accepted
        |
        v
ExecutionRevision B created
        |
        v
A authority revoked
        |
        v
B becomes CURRENT
        |
        v
A late Completion
        |
        v
REJECT_STALE
```

---

# 57. Replan Mapping

Business Orchestration creates the new plan.

Pipeline Runtime decides how that plan maps to:

```text
ExecutionRevision
WorkItems
Attempts
```

Business Orchestrator MUST NOT create Runtime identities directly.

---

# 58. Partial Results

Partial result is allowed only when:

* BusinessExecutionPlan declares partial delivery;
* owning module supports partial output contract;
* identity is explicit;
* ordering is explicit;
* Runtime authority can be evaluated;
* downstream consumption is safe.

---

# 59. Partial Runtime Identity

Recommended:

```text
PartialExecutionResult
├── parentWorkItemId
├── attemptId
├── executionRevisionId
├── partialId
├── sequence/order
├── completionState
└── artifactReference
```

---

# 60. Partial Result Authority

Each partial output MUST pass authority validation.

A late partial from a superseded ExecutionRevision MUST NOT be accepted as current.

---

# 61. Presentation Boundary

Presentation commit is owned by Presentation/Application.

Pipeline Runtime may confirm:

```text
accepted execution result still has current authority
```

Presentation then decides:

```text
whether/how the result may be committed
to the current target/UI
```

---

# 62. Runtime Does Not Own UI Commit

Runtime Control MUST NOT be defined as the semantic owner of:

* target existence;
* layout validity;
* UI widget state;
* user-visible rendering correctness.

These belong to Presentation/Application.

---

# 63. Presentation Commit Flow

Recommended:

```text
Accepted Presentation Input Artifact
        |
        v
Presentation Module
        |
        v
Presentation Target Validation
        |
        v
UI/Application Commit
```

Runtime authority remains an input to that decision.

---

# 64. Concurrent ExecutionRevisions

Several ExecutionRevisions MAY coexist physically:

```text
ExecutionRevision 20 -> DRAINING
ExecutionRevision 21 -> CURRENT
ExecutionRevision 22 -> CREATED/PENDING
```

Only the current eligible revision may normally affect current execution output.

---

# 65. Concurrent WorkItems

Independent WorkItems may execute concurrently when:

* business dependencies satisfied;
* Scheduler admits them;
* resource budget available;
* provider/runtime concurrency permits;
* business ordering is preserved;
* cancellation scope active.

---

# 66. Backpressure

Pipeline Runtime MUST support bounded backpressure for:

* Queue capacity;
* worker saturation;
* provider saturation;
* memory pressure;
* Artifact pressure;
* slow downstream components.

Backpressure mechanisms belong to Scheduler, Queue and Resource policy.

Business Modules MUST NOT coordinate backpressure through private blocking dependencies.

---

# 67. Resource Cleanup

Logical authority loss and physical resource disposal are separate.

Recommended:

```text
ExecutionRevision loses authority
        |
        v
New work stops
        |
        v
Running Attempts drain/cancel
        |
        v
Artifact leases released
        |
        v
Retention eligibility evaluated
        |
        v
Physical disposal
```

---

# 68. ExecutionRevision Disposal

An ExecutionRevision may be disposed when:

* it is no longer current;
* no pending WorkItems remain;
* no active Attempts remain;
* no retained Runtime Artifact ownership remains;
* no active lease remains;
* required diagnostic retention expired;
* cleanup completed or transferred to cleanup-retry ownership.

---

# 69. WorkItem Terminal Outcome

One WorkItem accepts at most one logical terminal outcome.

Recommended:

```text
SUCCEEDED
FAILED
CANCELLED
ABANDONED
```

Authority rejections such as:

```text
STALE
DUPLICATE
INVALID_STATE
```

are tracked separately.

---

# 70. Attempt Outcome vs WorkItem Outcome

Example:

```text
Attempt 1:
    FAILED

Attempt 2:
    COMPLETED

WorkItem:
    SUCCEEDED
```

Another example:

```text
Attempt 1:
    COMPLETED

Authority:
    REJECT_STALE

WorkItem:
    CANCELLED / superseded according to execution lineage
```

---

# 71. Failure Model

Technical execution failures are normalized into Runtime Error contracts.

Failure remains separate from:

```text
CANCELLED
STALE rejection
ABANDONED
```

Detailed taxonomy belongs to `ERROR_MODEL.md`.

---

# 72. Runtime Completion State vs Business Failure

A WorkItem may technically succeed while the Business Module rejects the result as invalid.

That rejection MAY result in:

* WorkItem failure;
* another recovery WorkItem;
* Fallback request;
* replan;
* user-visible degradation.

Exact ownership depends on the Business contract.

---

# 73. Provider Runtime Boundary

Pipeline Runtime executes already-resolved execution bindings through public provider/capability runtime interfaces.

It does NOT own:

* Provider Configuration;
* provider credentials;
* provider selection policy;
* AI model routing;
* Plugin trust.

---

# 74. Plugin Boundary

A WorkItem MAY execute through a Plugin-provided capability.

Runtime depends on the capability contract, not plugin-private implementation APIs.

---

# 75. Configuration Boundary

Every WorkItem/Attempt MUST carry or reference immutable Runtime configuration identity.

Runtime does not copy full Workspace/Profile/Provider/Plugin configuration into execution objects.

---

# 76. Secrets

Raw secrets MUST NOT appear in:

* WorkItem;
* Attempt;
* Completion;
* Runtime Artifact metadata;
* Event Bus payload;
* Runtime telemetry.

Secret references/privileged Host Services are used where required.

---

# 77. Runtime State Ownership Summary

| State                             | Owner                      |
| --------------------------------- | -------------------------- |
| Current ExecutionRevision         | Runtime Control            |
| ExecutionRevision metadata        | Execution State Store      |
| WorkItem logical state            | Runtime Control            |
| Attempt physical execution        | Worker / execution context |
| Attempt lineage acceptance        | Runtime Control            |
| Scheduler admission               | Scheduler                  |
| Queue position                    | Work Queue                 |
| Runtime Artifact registry         | Runtime Artifact Store     |
| Physical resource lifecycle       | Resource Manager           |
| Business result correctness       | Business Module            |
| Presentation/UI state             | Presentation/Application   |
| Durable persistence               | Storage                    |
| Provider configuration/governance | Provider Management        |

---

# 78. Downstream Ownership Summary

```text
Business Orchestrator
    defines stage graph

Business Module
    accepts business output

Pipeline Runtime
    observes declared dependency satisfaction

Runtime Control
    materializes downstream WorkItems

Scheduler
    controls admission
```

---

# 79. Shutdown

Recommended:

```text
Stop New ExecutionScope Creation
        |
        v
Stop Scheduler Admission
        |
        v
Revoke / Quiesce Execution Authority
        |
        v
Remove Obsolete Queued Work
        |
        v
Signal Running Attempts
        |
        v
Drain / Cancel
        |
        v
Release Artifact Leases
        |
        v
Dispose ExecutionRevision State
        |
        v
Stop Workers / Provider Runtime
```

Exact ordering aligns with `BOOT_SEQUENCE.md` and `RESOURCE_LIFECYCLE.md`.

---

# 80. Observability

Pipeline Runtime SHOULD emit telemetry for:

* ExecutionScope lifecycle;
* ExecutionRevision lifecycle;
* WorkItem lifecycle;
* Attempt lifecycle;
* Queue wait;
* execution duration;
* authority rejection;
* stale Completion;
* Retry lineage;
* cancellation;
* Runtime Artifact publication;
* business-result acceptance latency;
* cleanup;
* wasted execution.

---

# 81. Correlation

Recommended correlation chain:

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

Business IDs may be attached separately.

---

# 82. Privacy

Runtime telemetry MUST NOT log reading content by default.

Do not log:

* screenshot;
* OCR/source text;
* translated text;
* Prompt;
* full AI Context;
* raw provider payload;
* secret.

---

# 83. Performance

Runtime performance SHOULD prioritize useful accepted output.

Possible metrics:

```text
Current ExecutionRevision Useful Latency
Useful Work Ratio
Stale Work Ratio
Rejected Completion Ratio
Wasted Execution Time
Queue Wait
Retry Cost
Business Acceptance Latency
```

---

# 84. Dependency Rules

1. Runtime Control does not depend on concrete provider implementation.

2. Scheduler does not modify BusinessExecutionPlan.

3. Worker does not mutate Runtime Control state.

4. Worker does not perform orchestration-level Retry.

5. Worker does not choose Fallback.

6. Business Module does not directly schedule downstream Runtime work.

7. Event Bus does not orchestrate the pipeline.

8. Runtime Artifact Store does not own Business semantics.

9. Storage does not manage execution authority.

10. UI does not call Worker directly.

11. Raw secrets do not travel through Runtime execution contracts.

12. Large payloads use ArtifactRefs/handles.

13. Process boundaries preserve execution semantics.

14. Completion passes authority validation before Runtime acceptance.

15. Accepted Runtime result may still require Business validation.

16. Late Attempt cannot overwrite accepted WorkItem outcome.

17. Resource cleanup must preserve active leases.

18. Runtime may only advance stages declared in the accepted plan.

19. Runtime does not invent business workflow.

20. Fallback selection belongs outside Pipeline Runtime.

---

# 85. Architecture Invariants

1. `ExecutionScope` is the Runtime execution scope.

2. `ExecutionRevision` is the Runtime freshness/authority generation.

3. ExecutionRevision is distinct from Domain revisions.

4. ReadingSession is distinct from ExecutionScope.

5. ExecutionRevision identity is immutable.

6. WorkItem identity is immutable.

7. Attempt identity is immutable.

8. Retry creates a new Attempt.

9. WorkItem accepts at most one final logical outcome.

10. Runtime Control is authority for execution-orchestration state.

11. Runtime Control is not owner of every Runtime component's state.

12. Scheduler owns admission.

13. Worker owns physical execution only.

14. Worker does not commit Business or UI state.

15. Completion is not accepted outcome.

16. Runtime authority acceptance is not Business correctness.

17. Business Module owns Business result validity.

18. Published Runtime Artifacts are immutable.

19. Runtime Artifact publication is not Domain commit.

20. Stale is an authority rejection concept.

21. Cancellation is not automatically Failure.

22. Retry and Fallback are separate.

23. Pipeline Runtime does not select another Provider/Model fallback.

24. Current ExecutionRevision does not automatically have absolute highest Scheduler priority.

25. Business plan defines downstream stage graph.

26. Runtime may advance declared stages only.

27. Runtime MUST NOT invent Business Stages.

28. Downstream readiness requires accepted business dependency output.

29. Worker completion alone does not satisfy a Business Stage.

30. Large payloads do not travel through Queue.

31. Raw secrets do not travel through WorkItem/Completion.

32. Runtime correctness does not depend on telemetry.

33. Resource authority loss does not imply immediate physical disposal.

34. Artifact disposal requires ownership/lease eligibility.

35. Storage and Runtime Artifact Store remain separate.

36. Presentation owns visible commit semantics.

37. Runtime authority may constrain Presentation commit but does not own UI semantics.

38. Shutdown stops admission before destructive cleanup.

---

# 86. Recommended MVP

CRAI MVP SHOULD support:

* ExecutionScope;
* ExecutionRevision;
* WorkItem;
* Attempt;
* immutable BusinessExecutionPlan input;
* stage dependency tracking;
* Scheduler admission;
* bounded Work Queue;
* Worker execution;
* Completion reporting;
* execution-authority validation;
* Runtime Artifact publication;
* Business-result handoff;
* downstream declared-stage readiness;
* same-binding Retry;
* cancellation;
* stale protection;
* Runtime Artifact leases;
* graceful cleanup;
* ExecutionRevision replacement.

MVP MAY defer:

* speculative execution;
* provider racing;
* distributed Runtime Control;
* distributed Work Queues;
* multi-process execution consensus;
* dynamic business plan mutation;
* autonomous Runtime planning;
* automatic Runtime-owned provider Fallback;
* persistent queue replay.

---

# 87. Open Decisions

The following remain open:

* exact ExecutionScope schema;
* exact ExecutionRevision schema;
* WorkItem schema;
* Attempt schema;
* Completion schema;
* AcceptedExecutionResult schema;
* stage-runtime readiness representation;
* WorkItem terminal outcome taxonomy;
* how Business Module acceptance is represented;
* whether Business result rejection reuses WorkItem or creates recovery WorkItem;
* Retry configuration snapshot behavior;
* Fallback-to-Attempt handoff contract;
* Runtime Artifact candidate/publication API;
* partial-result identity;
* ExecutionRevision replacement command;
* Execution State persistence;
* current revision per execution lineage;
* crash recovery;
* cleanup retention;
* Provider Runtime Gateway invocation contract.

---

# 88. Related Documents

Runtime:

* `BUSINESS_PIPELINE_ORCHESTRATION.md`
* `RUNTIME_COMPONENTS.md`
* `BOOT_SEQUENCE.md`
* `RUNTIME_CONFIG.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `CACHE_POLICY.md`
* `MEMORY_MODEL.md`
* `THREADING_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `PERFORMANCE_MODEL.md`
* `ERROR_MODEL.md`
* `RUNTIME_OBSERVABILITY.md`
* `PROCESS_TOPOLOGY.md`

External:

* `../ai/ROUTING.md`
* `../ai/RETRY.md`
* `../ai/FALLBACK.md`
* `../plugin/PLUGIN_SYSTEM.md`
* `../../02-modules/provider-management/`

---

# 89. Completion Criteria

`PIPELINE_RUNTIME.md` is synchronized when:

* Runtime vocabulary uses ExecutionScope/ExecutionRevision;
* Reading Session is not treated as Runtime ownership;
* Runtime Control ownership is narrow and explicit;
* WorkItem and Attempt remain distinct;
* Completion remains distinct from accepted outcome;
* Runtime authority remains distinct from Business correctness;
* STALE is separated from physical Attempt outcome;
* Retry creates new Attempt;
* Retry remains distinct from Fallback;
* provider fallback is not selected by Runtime;
* Runtime Artifact publication remains authority-gated;
* Business Module validation/commit occurs before downstream business-stage satisfaction;
* downstream work is created only for stages declared in the plan;
* Presentation commit remains Presentation-owned;
* Resource cleanup remains separate from logical authority loss.

---

# 90. Summary

Pipeline Runtime transforms an immutable BusinessExecutionPlan into controlled execution:

```text
BusinessExecutionPlan
        |
        v
ExecutionScope
        |
        v
ExecutionRevision
        |
        v
Declared Business Stage
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
Execution Authority Validation
        |
        v
Accepted Execution Result
        |
        v
Runtime Artifact
        |
        v
Business Module Validation / Commit
        |
        v
Declared Downstream Stage Ready
```

The central ownership model is:

```text
Business Orchestrator
    owns the declared business graph.

Runtime Control
    owns execution authority.

Scheduler
    owns admission.

Worker
    owns physical Attempt execution.

Business Module
    owns result meaning and correctness.

Runtime Artifact Store
    owns execution artifacts.

Presentation
    owns visible commit.

Storage
    owns durable persistence.
```
