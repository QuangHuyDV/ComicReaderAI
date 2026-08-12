# Business Pipeline Orchestration

* **Document:** Runtime Architecture / Business Pipeline Orchestration
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI transforms a validated business intent into an immutable logical execution plan.

Business Pipeline Orchestration answers:

```text
For this use case,
what business work is required,
in what logical dependency order,
and what business outputs are expected?
```

Its output is:

```text
BusinessExecutionPlan
```

Business Pipeline Orchestration does NOT define:

* Work Queue implementation;
* Scheduler admission;
* Worker execution;
* physical Retry;
* cancellation mechanics;
* resource allocation;
* provider invocation;
* Runtime Artifact storage.

Those belong to Pipeline Runtime and other Runtime architecture documents.

---

# 2. Core Separation

CRAI distinguishes three layers:

```text
Business Architecture
        |
        v
Business Pipeline Orchestration
        |
        v
Pipeline Runtime
```

---

## 2.1 Business Architecture

Defines:

```text
what capabilities/modules exist
what each capability means
who owns each business result
```

Examples:

```text
Capture
Recognition
Text Processing
Translation
Presentation
Reading
Storage
```

---

## 2.2 Business Pipeline Orchestration

Defines:

```text
which Business Stages are required
which inputs/outputs connect them
which dependencies exist
which stages may be skipped
which outputs may be reused
which partial boundaries are allowed
```

Result:

```text
BusinessExecutionPlan
```

---

## 2.3 Pipeline Runtime

Defines:

```text
how the BusinessExecutionPlan is executed
```

using concepts such as:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
ArtifactRef
RuntimeConfigurationSnapshotId
```

---

# 3. Architectural Position

```text
User / System Intent
        |
        v
Application Use Case
        |
        v
Resolved Business Constraints
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
Scheduler / Worker Execution
        |
        v
Public Business Module Contracts
```

Business Pipeline Orchestration sits between:

```text
Application Use Case
```

and:

```text
Pipeline Runtime
```

It is neither Runtime Control nor Scheduler.

---

# 4. Core Principle

```text
Business Orchestrator
    decides WHAT business work is required.

Pipeline Runtime
    decides HOW declared work is executed.

Business Modules
    decide WHAT each result means
    and whether it is business-valid.
```

---

# 5. Responsibilities

Business Pipeline Orchestration owns:

* Business Request interpretation;
* Pipeline Variant selection;
* minimal Business Stage selection;
* Business Stage dependency graph;
* required input declaration;
* expected output declaration;
* stage optionality;
* logical conditional-stage rules;
* logical parallelism declaration;
* partial-delivery semantics;
* business priority declaration;
* reusable-output eligibility declaration;
* presentation/output intent;
* immutable BusinessExecutionPlan creation;
* planning diagnostics;
* replanning when business intent changes.

---

# 6. Non-Responsibilities

Business Pipeline Orchestration does NOT:

* execute Capture;
* execute Recognition;
* execute Translation;
* execute Presentation;
* call concrete providers;
* choose worker/thread/process;
* own Runtime queues;
* perform Scheduler admission;
* create Attempt identities;
* perform physical Retry;
* perform Fallback routing;
* propagate Runtime cancellation;
* enforce Runtime timeout;
* manage provider concurrency;
* store Runtime Artifact payloads;
* validate stale Attempt results;
* accept Runtime terminal outcomes;
* mutate Domain state;
* directly commit Presentation/UI state;
* persist durable business data itself;
* replace Event Bus;
* replace Runtime Control.

---

# 7. Business Stage

A `BusinessStage` is one logical step in a business workflow.

It corresponds to:

```text
a public responsibility
owned by a Business Module
```

or another explicitly defined application use-case boundary.

Examples:

```text
Acquire Content
Recognize Content
Build Source Document
Translate Source Document
Prepare Presentation
Present Result
Persist Requested Result
```

---

# 8. Business Stage Is Not Internal Capability

The following are NOT automatically separate Business Stages:

```text
OCR
Region Detection
Reading Order
Layout Detection
Segmentation
Normalization
Provider Call
Prompt Construction
Cache Lookup
GPU Execution
```

These may be implementation capabilities inside a Business Stage.

---

# 9. Stage Ownership

Every Business Stage MUST have one primary business owner.

Example:

| Business Stage                        | Primary Owner   |
| ------------------------------------- | --------------- |
| Acquire structured/visual source      | Capture         |
| Recognize visual content              | Recognition     |
| Build translation-ready source        | Text Processing |
| Produce translation                   | Translation     |
| Prepare presentation representation   | Presentation    |
| Present/commit presentation result    | Presentation    |
| Manage reading intent/session meaning | Reading         |
| Persist requested durable result      | Storage         |

The Business Orchestrator does not inherit stage ownership.

---

# 10. Stage Ownership Rule

```text
Orchestrator
    owns dependency planning.

Business Module
    owns stage semantics.
```

The Orchestrator MUST NOT redefine:

```text
what RecognitionResult means
what TranslationResult means
what PresentationModel means
```

---

# 11. Business Request

Planning begins from a:

```text
BusinessRequest
```

Recommended conceptual structure:

```text
BusinessRequest
├── requestId
├── requestType
├── businessScopeReference
├── sourceIntent
├── requestedOutput
├── languageIntent?
├── presentationIntent?
├── privacyIntentReference?
├── userPriority?
├── reusableInputReferences[]
└── requestMetadata
```

---

# 12. Business Request Boundary

BusinessRequest describes business intent.

It MUST NOT contain Runtime implementation details such as:

```text
WorkerId
QueueId
ThreadId
AttemptId
process identity
provider connection
```

---

# 13. Execution Scope Correlation

A BusinessRequest MAY reference:

```text
ReadingSessionId
ProjectId
DocumentId
```

where relevant.

These are business correlation identities.

They MUST NOT replace the Runtime concept:

```text
ExecutionScopeId
```

---

# 14. Resolved Business Constraints

Before or during planning, authoritative owners MAY provide resolved constraints.

Recommended:

```text
ResolvedBusinessConstraints
├── privacyConstraints
├── policyConstraints
├── capabilityConstraints
├── languageConstraints
├── persistenceConstraints
├── presentationConstraints
└── references/version identities
```

---

# 15. Constraint Ownership

Business Pipeline Orchestration consumes resolved constraints.

It MUST NOT become the authoritative owner of:

* Workspace Policy;
* Privacy Policy;
* Provider Policy;
* AI Safety;
* Plugin Security;
* Provider credentials.

---

# 16. Pipeline Variant

A `PipelineVariant` represents a predefined logical workflow family.

Initial variants MAY include:

```text
TEXT_READING
IMAGE_READING
CLIPBOARD_TEXT
CLIPBOARD_IMAGE
MANUAL_IMAGE_TRANSLATION
RETRANSLATION
PRESENTATION_REFRESH
RESTORED_READING_SESSION
EXPORT
```

---

# 17. Variant Boundary

Pipeline Variant is NOT:

```text
provider profile
model profile
Runtime execution class
Plugin type
process topology
```

---

# 18. Business Execution Plan

Planning produces an immutable:

```text
BusinessExecutionPlan
```

Recommended:

```text
BusinessExecutionPlan
├── planId
├── planDefinitionVersion
├── plannerVersion?
├── requestReference
├── pipelineVariant
├── sourceIntent
├── requestedOutput
├── resolvedConstraintReferences[]
├── stages[]
├── dependencies[]
├── reusableOutputDeclarations[]
├── partialDeliveryPolicy?
├── businessPriorityPolicy?
├── presentationIntent?
└── metadata
```

---

# 19. Plan Identity

`planId` identifies one immutable planned result.

If replanning occurs:

```text
Plan A
    remains immutable

Plan B
    receives another planId
```

---

# 20. Plan Definition Version

`planDefinitionVersion` identifies the logical planning contract/template used to build the plan.

It may change when:

* stage graph semantics change;
* required input contracts change;
* output contracts change;
* conditional rules change;
* partial-delivery semantics change;
* planning ownership boundary changes.

---

# 21. Planner Version

`plannerVersion` MAY identify the implementation/release that created the plan.

It is operational provenance.

It MUST NOT replace business plan semantics.

---

# 22. Plan Does Not Contain Runtime State

BusinessExecutionPlan MUST NOT contain:

```text
Worker
Queue Position
Attempt Count
Cancellation Token implementation
Provider connection
mutable Artifact payload
Runtime terminal state
Runtime lease
thread/process identity
```

---

# 23. Business Stage Plan

Recommended:

```text
BusinessStagePlan
├── stageId
├── stageType
├── ownerModule
├── contractReference
├── requiredInputs[]
├── expectedOutputs[]
├── dependencies[]
├── optional
├── condition?
├── reuseEligibilityRef?
├── partialOutputPolicy?
├── businessPriority?
└── configurationReferences[]
```

---

# 24. Stage Contract

Every stage SHOULD reference a public business contract.

Example:

```text
RecognizeContent
    ->
Recognition Public Contract
```

Avoid:

```text
RecognizeContent
    ->
PaddleOCR implementation
```

---

# 25. Stage Graph

A BusinessExecutionPlan is normally a Directed Acyclic Graph.

```text
Stage A
   |
   v
Stage B
  / \
 v   v
C   D
```

Rules:

1. dependencies are explicit;

2. required graph is acyclic;

3. a stage does not invoke the next stage itself;

4. stages do not depend on another stage's private implementation;

5. inputs/outputs use declared contracts;

6. downstream stages do not mutate upstream immutable outputs.

---

# 26. Logical Dependency vs Runtime Readiness

Business dependency:

```text
Stage B requires Output A
```

does NOT directly imply:

```text
run B immediately after A physically completes
```

Pipeline Runtime must first establish that the declared dependency is satisfied and the result remains execution-current.

---

# 27. Runtime May Materialize Declared Ready Stages

Once the plan is accepted, Pipeline Runtime MAY determine:

```text
which already-declared stage
has all explicit dependencies satisfied
```

and materialize WorkItems for that stage.

This is NOT new business planning.

---

# 28. Runtime Must Not Invent Stages

Runtime MUST NOT infer:

```text
Translation succeeded
    therefore run Presentation
```

unless Presentation already exists in the accepted BusinessExecutionPlan with satisfied dependencies.

Critical distinction:

```text
Business Orchestrator
    defines the graph.

Runtime
    advances through the declared graph.
```

---

# 29. Dynamic Business Decisions

If execution produces information that requires a new business decision not represented by the current plan:

```text
execution result
    ->
Application / Business Orchestrator
    ->
replan
```

Runtime MUST NOT silently extend the plan.

---

# 30. Text Reading Pipeline

When structured text already exists:

```text
Acquire Structured Text
        |
        v
Build Source Document
        |
        v
Translate Source Document
        |
        v
Prepare Presentation
        |
        v
Present Result
```

Recognition SHOULD NOT be introduced unnecessarily.

---

# 31. Image Reading Pipeline

For visual content:

```text
Acquire Visual Source
        |
        v
Recognize Content
        |
        v
Build Source Document
        |
        v
Translate Source Document
        |
        v
Prepare Presentation
        |
        v
Present Result
```

Recognition MAY internally use:

* OCR;
* region detection;
* layout;
* reading order;
* traceability mapping.

They do not automatically become separate Business Stages.

---

# 32. Clipboard Text

```text
Acquire Clipboard Text
        |
        v
Build Source Document
        |
        v
Translate Source Document
        |
        v
Prepare Presentation
```

Recognition is not required.

---

# 33. Clipboard Image

```text
Acquire Clipboard Image
        |
        v
Recognize Content
        |
        v
Build Source Document
        |
        v
Translate Source Document
        |
        v
Prepare Presentation
```

---

# 34. Manual Image Translation

```text
Acquire Selected Image
        |
        v
Recognize Content
        |
        v
Build Source Document
        |
        v
Translate Source Document
        |
        v
Prepare Presentation
```

Characteristics MAY include:

* user-selected source;
* no mandatory continuous observation loop;
* output retained until dismissed;
* no automatic new visual source generation.

---

# 35. Retranslation

When the Source Document remains business-valid:

```text
Existing Source Document
        |
        v
Translate Source Document
        |
        v
Prepare Presentation
        |
        v
Present Result
```

Possible causes:

* target Language change;
* Translation Profile change;
* GlossarySnapshot change;
* explicit retranslation request;
* owning translation policy change.

---

# 36. Provider Change Boundary

A provider/model change does NOT necessarily require another Business Stage graph.

Example:

```text
Translate Source Document
```

remains the same Business Stage while AI/provider routing may choose another execution route.

---

# 37. Presentation Refresh

When Translation output remains valid:

```text
Existing Translation Result
        |
        v
Prepare Presentation
        |
        v
Present Result
```

Translation MUST NOT run again solely because visual presentation settings changed.

---

# 38. Export

```text
Accepted Business Result
        |
        v
Prepare Export Representation
        |
        v
Deliver / Persist Export
```

Export is an explicit use case.

It is not automatic persistence of all reading content.

---

# 39. Minimal Pipeline Selection

The Orchestrator SHOULD select the smallest valid Business Stage graph capable of producing the requested output.

Example:

```text
font/layout refresh

Required:
    Presentation

Not required:
    Capture
    Recognition
    Text Processing
    Translation
```

---

# 40. Another Minimal Example

```text
Glossary changed

Required:
    Translation
    Presentation

Potentially reusable:
    Source Document
```

---

# 41. Input Availability

A stage may appear in a plan only if every required input:

* already exists;
* may be produced by an upstream stage;
* or is eligible for business reuse.

---

# 42. Reuse Declaration

The Business Orchestrator MAY declare:

```text
this type of previous business result
is eligible to satisfy this input
```

Examples:

```text
Recognized Content
Source Document
Translation Result
Presentation Model
```

---

# 43. Reuse Eligibility vs Cache Hit

Critical distinction:

```text
Business Orchestration
    declares semantic reuse eligibility
```

```text
Cache / Runtime
    determines whether a compatible reusable result
    is actually available
```

---

# 44. Business Validity Ownership

The Orchestrator MUST NOT independently invent compatibility rules for a result owned by another module.

Example:

```text
Translation module
    defines which input/version changes
    invalidate a Translation Result
```

Planning consumes that validity/compatibility contract.

---

# 45. Runtime Artifact Lookup

The Business Orchestrator MUST NOT directly inspect mutable Runtime Artifact Store state during physical execution.

It may plan against:

```text
known reusable business references
validity projections
cache eligibility contracts
```

---

# 46. Optional Stage

A stage is optional only if omission still permits a valid useful requested result.

Optional MUST NOT mean:

```text
ignore failure of required business work
```

---

# 47. Conditional Stage

A stage MAY be conditionally included based on business inputs or validated business metadata.

Examples:

```text
structured text exists
    -> no Recognition

visual-only source
    -> Recognition required

TranslationResult valid
    -> Translation may be omitted
```

---

# 48. Runtime State Must Not Define Business Conditions

Planning conditions MUST NOT depend on uncontrolled mutable Runtime details such as:

```text
current queue length
current Worker ID
temporary provider connection
thread availability
```

Those affect execution, not business graph semantics.

---

# 49. Parallelizable Business Branches

The plan MAY declare logically independent branches.

Example:

```text
Document
   |
   +--> Visible Section
   |
   +--> Nearby Section
```

or:

```text
Page Collection
   |
   +--> Page A
   +--> Page B
   +--> Page C
```

---

# 50. Logical Parallelism Is Not Physical Concurrency

```text
parallelizable
    !=
execute concurrently
```

Scheduler/Runtime decides actual admission based on resources and priority.

---

# 51. Business Ordering

Business output order is defined by the owning business semantics.

Possible ordering metadata:

```text
documentOrder
pageOrder
regionOrder
segmentOrder
presentationOrder
```

Scheduler MUST NOT infer these semantics.

---

# 52. Business Priority

The plan MAY declare business-relative priority such as:

```text
VISIBLE_CONTENT
NEARBY_CONTENT
CURRENT_DOCUMENT_BACKGROUND
PREFETCH
MAINTENANCE
```

Scheduler maps these to runtime admission policy.

---

# 53. Priority Boundary

Business Orchestrator declares:

```text
relative business importance
```

Scheduler controls:

```text
actual runtime admission
```

---

# 54. Partial Delivery

The plan MAY allow partial output only when:

* owning module defines a partial contract;
* partial output has explicit identity;
* ordering is defined;
* Runtime authority can be evaluated independently;
* Presentation can safely consume it;
* partial meaning is not misleading.

---

# 55. Partial Boundary Examples

Possible:

```text
paragraph
comic region
page
document chunk
```

The owning module defines valid boundaries.

---

# 56. Incremental Plan

Example:

```text
Recognized Document
        |
        v
Source Chunks
   |
   +--> Chunk 1 -> Translation
   +--> Chunk 2 -> Translation
   +--> Chunk 3 -> Translation
```

Runtime may materialize one or more WorkItems per stage/chunk.

---

# 57. Stage Count vs WorkItem Count

Critical rule:

```text
1 Business Stage
    may become
1..N WorkItems
```

and:

```text
N Business Stages
    do not imply
exactly N WorkItems
```

---

# 58. Configuration References

BusinessExecutionPlan MAY reference immutable business configuration/version identities such as:

```text
TranslationProfileRevision
GlossarySnapshotId
RecognitionProfileReference
PresentationProfileRevision
ResolvedPolicyReference
```

---

# 59. Runtime Configuration Boundary

The plan SHOULD NOT embed Runtime mechanics such as:

```text
queue capacity
worker count
retry attempt count
GPU pool size
```

Pipeline Runtime receives Runtime Configuration separately.

---

# 60. Provider Configuration Boundary

Plan MUST NOT contain raw mutable Provider Configuration.

It MAY carry capability requirements or routing intent owned by the business/AI contract when needed.

---

# 61. Secrets

BusinessExecutionPlan MUST NOT contain:

```text
API key
OAuth token
client secret
private key
authorization header
```

---

# 62. Privacy Boundary

Business planning MUST respect resolved privacy/policy constraints.

Example:

```text
LOCAL_ONLY
    ->
do not create a business path
whose only valid implementation
requires prohibited remote processing
```

---

# 63. Privacy Ownership

The Business Orchestrator consumes:

```text
ResolvedPolicyConstraints
```

It does not become the authoritative Policy owner.

---

# 64. EPHEMERAL Behavior

If policy requires ephemeral processing:

```text
durable persistence stage
```

MUST NOT be added implicitly.

Explicit user-requested persistence may still require separate policy validation.

---

# 65. Storage Boundary

Storage is not a mandatory stage of every plan.

Storage appears only when the use case explicitly requires durable persistence.

---

# 66. Runtime Artifact Store Is Not Storage Stage

```text
Runtime Artifact Store
    = execution payload lifecycle
```

```text
Storage
    = durable persistence capability
```

The former MUST NOT appear as a Business Stage.

---

# 67. Interaction with Pipeline Runtime

Business Orchestration submits:

```text
BusinessExecutionPlan
```

Pipeline Runtime then owns execution mechanics including:

* Execution Scope binding;
* Execution Revision creation;
* WorkItem materialization;
* Attempt creation;
* Scheduler admission;
* queueing;
* worker execution;
* cancellation;
* Retry;
* timeout;
* execution-authority validation;
* Runtime Artifact publication;
* physical cleanup.

---

# 68. Pipeline Runtime Does Not Change Plan Meaning

Pipeline Runtime MUST NOT:

* remove mandatory stages;
* insert arbitrary business stages;
* alter stage ownership;
* change expected business output;
* reinterpret business priority;
* change business ordering.

---

# 69. Stage Runtime Readiness

Pipeline Runtime MAY calculate:

```text
DECLARED_STAGE_READY
```

when:

* all declared business dependencies are satisfied;
* required accepted input references exist;
* current ExecutionRevision still has authority;
* Runtime admission remains possible.

This is a Runtime decision over an already-defined graph.

---

# 70. Runtime Control Interaction

Preferred:

```text
BusinessExecutionPlan
        |
        v
Runtime Control
        |
        v
ExecutionRevision
        |
        v
Stage Readiness
        |
        v
WorkItem materialization
```

---

# 71. Plan Acceptance

Runtime Control MAY reject execution of a plan when:

* request/execution scope is obsolete;
* plan contract version unsupported;
* immutable references are unavailable;
* resolved constraints cannot be represented safely;
* required Runtime capabilities are unavailable;
* Runtime is shutting down;
* plan violates Runtime contract invariants.

---

# 72. Runtime Does Not Re-evaluate Business Policy

Runtime SHOULD NOT independently reinterpret:

```text
whether privacy policy means X
whether TranslationProfile is valid
whether Workspace permits feature Y
```

Those should already be resolved by authoritative owners.

Runtime may verify that the provided resolved constraint/reference remains valid/current.

---

# 73. Scheduler Interaction

Business Orchestrator MUST NOT directly submit work to Scheduler.

Correct flow:

```text
BusinessExecutionPlan
        |
        v
Runtime Control
        |
        v
WorkItem
        |
        v
Scheduler
```

---

# 74. Business Module Interaction

Each Business Stage invokes only public owner contracts.

Forbidden plan references include:

* concrete provider implementation;
* plugin-private interface;
* internal package;
* raw database model;
* mutable private module state;
* UI implementation detail.

---

# 75. Result Acceptance Boundary

Physical success:

```text
Attempt SUCCEEDED
```

is not automatically:

```text
Business Stage succeeded
```

Recommended:

```text
Attempt Result
      |
      v
Runtime Authority Validation
      |
      v
Accepted Execution Result
      |
      v
Owning Business Module
      |
      v
Business Validation / Commit
```

---

# 76. Business Stage Completion

A Business Stage is logically satisfied only when its owning contract says its required output is valid.

Runtime execution success alone does not redefine business correctness.

---

# 77. Downstream Stage Boundary

A downstream declared stage MAY become runtime-ready only after:

```text
upstream accepted business output
```

satisfies the dependency contract.

Not merely after:

```text
upstream Worker returned
```

---

# 78. Event Bus

Business Orchestration MAY emit plan lifecycle notifications such as:

```text
BusinessPlanCreated
BusinessPlanRejected
BusinessPlanReplaced
BusinessPlanNotRequired
```

---

# 79. Runtime Stage Events

Events such as:

```text
StageRuntimeReady
StageWorkStarted
StageWorkCompleted
```

belong to Pipeline Runtime/Observability rather than Business Orchestration.

---

# 80. Event Bus Is Not Orchestrator

An Event MUST NOT implicitly execute the next stage.

Business plan dependency + Runtime authority remain the source of execution progression.

---

# 81. Replanning

Replanning occurs when business intent or business validity assumptions materially change.

Examples:

* source type changed;
* target Language changed;
* Presentation intent changed;
* privacy/policy constraint changed;
* requested output changed;
* previous reusable business result became invalid;
* use case changed.

---

# 82. Replanning Produces Another Plan

```text
Plan A
    remains immutable

Business Intent changes

Plan B
    created
```

---

# 83. Plan Replacement

Recommended:

```text
Plan A accepted
        |
        v
Business intent changes
        |
        v
Plan B created
        |
        v
Plan Replacement Request
        |
        v
Runtime Control
        |
        v
Execution authority updated
```

---

# 84. Orchestrator Does Not Cancel Workers

The Business Orchestrator requests replacement/replan semantics.

Runtime controls:

* WorkItem cancellation;
* Attempt cancellation;
* queue removal;
* ExecutionRevision supersession.

---

# 85. ExecutionRevision on Replan

A new accepted plan MAY result in:

```text
new ExecutionRevision
```

or another Runtime execution structure.

The exact mapping belongs to `PIPELINE_RUNTIME.md`.

Business Orchestration MUST NOT directly assign Runtime execution identity.

---

# 86. Planning Error

Planning errors include:

```text
PIPELINE_VARIANT_UNSUPPORTED
BUSINESS_INPUT_MISSING
BUSINESS_STAGE_OWNER_MISSING
BUSINESS_STAGE_GRAPH_CYCLE
REQUESTED_OUTPUT_UNSUPPORTED
RESOLVED_CONSTRAINT_UNSATISFIABLE
BUSINESS_PLAN_INVALID
```

---

# 87. Execution Error

Examples:

```text
provider timeout
Worker failure
resource exhaustion
Attempt cancellation
stale execution result
```

These belong to Runtime Error Model.

---

# 88. Planning Result

Planning may return:

```text
PLAN_CREATED
PLAN_NOT_REQUIRED
PLAN_REJECTED
PLAN_UNSUPPORTED
```

---

# 89. PLAN_NOT_REQUIRED

`PLAN_NOT_REQUIRED` means:

```text
requested business output already exists
and remains business-valid
```

with no required additional Business Stage.

It is NOT a WorkItem terminal outcome.

---

# 90. Planning Observability

Recommended content-free metrics:

```text
plan creation count
plan variant
stage count
optional stage count
replan count
planning latency
reuse eligibility count
partial-delivery count
planning rejection code
```

---

# 91. Privacy

Planning telemetry MUST NOT contain by default:

* source text;
* OCR text;
* translated text;
* screenshot;
* Prompt;
* AI Context;
* raw source URL;
* credentials.

---

# 92. Conceptual Image Reading Example

```text
BusinessRequest
    requestType = IMAGE_READING
    requestedOutput = TRANSLATED_READING_VIEW

        |
        v

Business Pipeline Orchestration

        |
        v

BusinessExecutionPlan

    Acquire Visual Source
        |
        v
    Recognize Content
        |
        v
    Build Source Document
        |
        v
    Translate Source Document
        |
        v
    Prepare Presentation
        |
        v
    Present Result

        |
        v

Pipeline Runtime

    ExecutionRevision
        |
        +--> Capture WorkItem(s)
        +--> Recognition WorkItem(s)
        +--> Text Processing WorkItem(s)
        +--> Translation WorkItem(s)
        +--> Presentation WorkItem(s)
```

---

# 93. Text Reading Example

```text
BusinessRequest
    requestType = TEXT_READING

        |
        v

BusinessExecutionPlan

    Acquire Structured Text
        |
        v
    Build Source Document
        |
        v
    Translate Source Document
        |
        v
    Prepare Presentation
        |
        v
    Present Result
```

No Recognition stage is required.

---

# 94. Presentation Refresh Example

```text
BusinessRequest
    requestType = PRESENTATION_REFRESH

        |
        v

Existing Translation Result
        |
        v
Prepare Presentation
        |
        v
Present Result
```

Translation does not rerun.

---

# 95. Glossary Change Example

```text
BusinessRequest
    requestType = RETRANSLATION
    glossarySnapshot = newer

        |
        v

Existing Source Document
        |
        v
Translate Source Document
        |
        v
Prepare Presentation
        |
        v
Present Result
```

Source Document reuse requires validity compatibility.

---

# 96. Dependency Rules

1. Business Orchestrator depends on public business contracts.

2. Business Orchestrator does not depend on Scheduler implementation.

3. Business Orchestrator does not call Worker.

4. Business Orchestrator does not own Runtime Artifact Store.

5. Business Stage does not deep-import another stage implementation.

6. Required Business Stage graph is acyclic.

7. Every Business Stage has one primary owner.

8. Required inputs/outputs cross explicit contracts.

9. Provider DTOs do not appear in BusinessExecutionPlan.

10. Raw secrets do not appear in BusinessExecutionPlan.

11. Storage appears only for explicitly durable use cases.

12. Event Bus does not orchestrate stage progression.

13. Runtime Control does not change business graph semantics.

14. Scheduler does not reinterpret business priority.

15. Replanning creates another immutable plan.

16. Runtime may advance only through stages already declared by the plan.

17. Runtime does not invent downstream Business Stages.

18. Business result validity remains owned by the owning Business Module.

---

# 97. Architecture Invariants

1. Every BusinessExecutionPlan has a stable `planId`.

2. Plans are immutable after creation.

3. Replanning creates a new plan.

4. Plan Definition Version is separate from plan instance identity.

5. Business Stage has one primary owner.

6. Business Stage is not automatically an internal capability.

7. Plan contains dependency logic, not physical execution state.

8. WorkItem and Attempt belong to Pipeline Runtime.

9. ExecutionScope and ExecutionRevision belong to Runtime.

10. ReadingSessionId is not a replacement for ExecutionScopeId.

11. Business Orchestrator does not own Queue/Scheduler.

12. Business Orchestrator does not perform physical Retry.

13. Business Orchestrator does not perform Fallback Routing.

14. Stage does not self-trigger downstream stage.

15. Runtime may activate only declared stages.

16. Runtime MUST NOT invent a business stage.

17. Runtime execution MUST NOT change business semantics.

18. Downstream stage readiness requires accepted dependency output, not merely Worker completion.

19. Stage output is not mutated by downstream stages.

20. Minimal valid pipeline SHOULD be selected.

21. Structured text SHOULD bypass unnecessary Recognition.

22. Partial delivery requires explicit owner-defined semantics.

23. Logical parallelism does not require physical concurrency.

24. Scheduler does not define business ordering.

25. Reuse eligibility and cache hit are distinct.

26. Business validity rules remain owned by Business Modules.

27. Provider changes do not automatically change Business Stage graph.

28. AI Routing is not Business Pipeline Orchestration.

29. Runtime Retry is not Business Pipeline Orchestration.

30. Storage is not a mandatory stage.

31. Runtime Artifact Store is not Storage.

32. Business Pipeline Orchestration consumes resolved Policy constraints but does not own Policy.

33. Plan metadata contains no user content by default.

34. Planning Error and Execution Error remain distinct.

35. Plan lifecycle events do not execute stages.

36. Runtime Control may reject execution-contract violations but does not reinterpret business policy.

37. Accepted execution result still requires owner-module business validation where defined.

38. Physical Attempt success does not automatically imply business-stage completion.

---

# 98. Recommended MVP

CRAI MVP SHOULD support:

* TEXT_READING;
* IMAGE_READING;
* MANUAL_IMAGE_TRANSLATION;
* RETRANSLATION;
* PRESENTATION_REFRESH;
* immutable BusinessExecutionPlan;
* explicit Business Stage ownership;
* DAG dependencies;
* minimal pipeline selection;
* reusable-output eligibility;
* visible-first business priority;
* partial delivery for explicitly supported contracts;
* plan replacement/replanning;
* resolved privacy constraints;
* runtime plan acceptance;
* stage-to-WorkItem materialization.

MVP MAY defer:

* highly dynamic runtime-generated business graphs;
* cyclic/iterative business workflow;
* speculative alternative plans;
* autonomous AI-driven planning;
* runtime plan mutation;
* distributed orchestration;
* generic workflow language;
* user-authored pipelines.

---

# 99. Open Decisions

The following remain open:

* exact BusinessRequest schema;
* exact BusinessExecutionPlan schema;
* exact BusinessStagePlan schema;
* Pipeline Variant taxonomy;
* plan-definition versioning format;
* planner provenance format;
* conditional-expression format;
* stage readiness representation;
* business priority mapping;
* reusable-output declaration contract;
* validity-query mechanism;
* partial delivery contract;
* plan replacement command;
* mapping from accepted plan to ExecutionRevision;
* runtime stage-completion representation;
* dynamic replan trigger rules;
* application-use-case ownership.

---

# 100. Related Documents

Runtime:

* `RUNTIME_COMPONENTS.md`
* `BOOT_SEQUENCE.md`
* `RUNTIME_CONFIG.md`
* `PIPELINE_RUNTIME.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `CACHE_POLICY.md`
* `MEMORY_MODEL.md`
* `ERROR_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `RUNTIME_OBSERVABILITY.md`

Architecture:

* `../core/DATA_FLOW.md`
* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`

Modules:

* `../../02-modules/capture/`
* `../../02-modules/recognition/`
* `../../02-modules/text-processing/`
* `../../02-modules/translation/`
* `../../02-modules/presentation/`
* `../../02-modules/reading-session/`
* `../../02-modules/storage/`

---

# 101. Completion Criteria

This document is synchronized when:

* Business Orchestration and Pipeline Runtime are clearly separate;
* BusinessStage and internal capability remain distinct;
* every BusinessStage has explicit ownership;
* plan is immutable;
* Runtime identities use ExecutionScope/ExecutionRevision terminology;
* Runtime may advance declared graph but cannot invent business work;
* reuse eligibility is distinct from cache availability;
* business validity remains module-owned;
* Privacy/Policy ownership remains external;
* Provider/AI routing remains external;
* planning errors remain separate from execution errors;
* plan replacement creates a new plan;
* physical Attempt success does not automatically equal business success.

---

# 102. Summary

Business Pipeline Orchestration follows:

```text
Business Intent
        |
        v
Resolved Business Constraints
        |
        v
Pipeline Variant Selection
        |
        v
Business Stage Graph
        |
        v
Immutable BusinessExecutionPlan
        |
        v
Pipeline Runtime
```

The central ownership model is:

```text
Business Orchestrator
    decides what logical business work exists.

Pipeline Runtime
    executes the declared plan.

Business Modules
    decide what results mean.

Runtime Control
    owns execution authority.

Scheduler
    owns admission.
```

Runtime may progress through the accepted plan.

Runtime may never invent the business plan.
