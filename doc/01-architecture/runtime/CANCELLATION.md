# Runtime Cancellation

* **Document:** Runtime Architecture / Cancellation
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI Runtime revokes execution authority, removes queued work, requests running execution to stop safely, handles non-cancelable physical operations, rejects late results and drains resources.

Cancellation is a Runtime control capability.

Its first purpose is correctness:

```text
Revoke execution authority immediately.
```

Its second purpose is efficiency:

```text
Stop physical execution when safe and practical.
```

The canonical principle is:

```text
Logical cancellation first.

Physical stopping second.

Late-result rejection always.
```

---

# 2. Architectural Position

```text
Cancellation Request
        |
        v
Runtime Control
        |
        v
Execution Authority Revoked
        |
        v
Queued Work Removal
        |
        v
Running Attempts Signaled
        |
        v
Child Physical Operations Signaled
        |
        v
Stop / Abort / Abandon
        |
        v
Resource Drain
        |
        v
Completion / Cleanup Tracking
```

Runtime Control is the authority owner for cancellation state affecting Runtime execution.

Other components execute their own cancellation responsibilities.

---

# 3. Core Principles

1. Cancellation begins with execution-authority revocation.

2. Logical cancellation occurs before physical stop.

3. Physical execution MAY stop immediately, later or not at all.

4. Every late Completion passes execution-authority validation.

5. Cancellation is not Failure by default.

6. Cancellation does not automatically create Retry.

7. Cancellation does not choose Fallback.

8. Hard arbitrary thread termination is forbidden in the primary process.

9. Cleanup ownership is explicit.

10. Cancellation wait is bounded.

11. Logical cancellation does not imply physical resource release.

12. Canceled execution cannot create accepted downstream work.

13. Cancellation telemetry contains no user content by default.

14. Runtime cancellation does not own Business/Presentation commit semantics.

---

# 4. Cancellation Goals

Cancellation SHOULD:

* remove obsolete pending work quickly;
* protect current ExecutionRevision;
* free Queue capacity;
* reduce wasted CPU/GPU/network execution;
* avoid blocking interactive UI;
* keep non-cancelable operations tracked;
* prevent revoked work from regaining authority;
* preserve WorkItem outcome correctness;
* preserve resource ownership;
* support deterministic testing;
* support graceful shutdown.

---

# 5. Canonical Cancellation Scopes

Recommended Runtime scopes:

```text
APPLICATION
EXECUTION_SCOPE
EXECUTION_REVISION
WORK_ITEM
ATTEMPT
```

These are the canonical execution-authority scopes.

---

# 6. APPLICATION

Application cancellation affects the entire Runtime execution tree.

Typical causes:

* application shutdown;
* fatal Runtime invariant violation;
* controlled application restart.

---

# 7. EXECUTION_SCOPE

ExecutionScope cancellation revokes execution associated with one Runtime execution scope.

Possible business correlation:

```text
ReadingSession
Manual Translation Operation
Export Operation
```

but the canonical Runtime scope remains:

```text
ExecutionScopeId
```

---

# 8. EXECUTION_REVISION

ExecutionRevision cancellation/supersession revokes one generation of execution authority.

Typical causes:

* newer ExecutionRevision accepted;
* BusinessExecutionPlan replaced;
* execution source identity replaced;
* execution intent materially changed.

---

# 9. WORK_ITEM

WorkItem cancellation revokes one logical unit of Runtime work.

Examples:

* optional work no longer needed;
* logical dependency invalidated;
* explicit operation cancellation.

---

# 10. ATTEMPT

Attempt cancellation targets one physical execution attempt.

Examples:

* timeout;
* attempt preemption;
* execution binding abandoned;
* recovery requires stopping current Attempt.

---

# 11. Physical Child Operations

An Attempt MAY own one or more physical child operations such as:

```text
provider request
native operation
GPU task
subprocess invocation
stream
file operation
```

These are not required to be canonical Runtime authority scopes.

They SHOULD instead be registered beneath the owning Attempt through:

```text
CancellationChildOperation
```

or equivalent runtime ownership.

---

# 12. Why Provider Request Is Not Canonical Scope

A single Attempt MAY issue:

```text
0..N provider/native/child operations
```

Therefore the Runtime hierarchy SHOULD NOT assume:

```text
Attempt
    -> exactly one ProviderRequest
```

Physical operations remain child cancellation contexts.

---

# 13. Scope Hierarchy

Canonical hierarchy:

```text
APPLICATION
    |
    v
EXECUTION_SCOPE
    |
    v
EXECUTION_REVISION
    |
    v
WORK_ITEM
    |
    v
ATTEMPT
```

Physical child operations hang beneath the Attempt as registered children.

---

# 14. Propagation

Parent cancellation propagates downward.

Example:

```text
Cancel EXECUTION_REVISION

    ->
all child WorkItems
    ->
all child Attempts
    ->
registered physical operations
```

Child cancellation does not automatically cancel its parent.

---

# 15. Escalation

Upward escalation MUST be an explicit Runtime/Recovery decision.

Example:

```text
Cancel ATTEMPT
    !=
Cancel EXECUTION_REVISION
```

---

# 16. Non-Canonical Legacy Scopes

The following SHOULD NOT be canonical cancellation authority scopes:

```text
SESSION
REVISION
SCHEDULER
PIPELINE
BUSINESS_STAGE
PROVIDER_REQUEST
```

Clarification:

```text
SESSION
    may be Business correlation

REVISION
    replaced by ExecutionRevision

PROVIDER_REQUEST
    physical child operation

SCHEDULER
    has Scheduler lifecycle

BUSINESS_STAGE
    belongs to BusinessExecutionPlan semantics
```

---

# 17. Cancellation Context

Recommended:

```text
CancellationContext
├── cancellationContextId
├── scopeType
├── scopeId
├── parentContextReference?
├── state
├── reasonCode
├── requestedAt
├── requestedBy
├── graceDeadline?
└── metadata
```

---

# 18. Cancellation Context State

Possible:

```text
ACTIVE
CANCELLATION_REQUESTED
AUTHORITY_REVOKED
SIGNALING
DRAINING
ACKNOWLEDGED
ABANDONED
COMPLETED
```

This state machine describes cancellation coordination.

It does NOT redefine WorkItem/Attempt lifecycle.

---

# 19. Cancellation Reference

WorkItems and Attempts SHOULD carry lightweight references such as:

```text
CancellationContextRef
```

They MUST NOT embed:

* mutable cancellation implementation;
* provider SDK handle;
* arbitrary thread handle;
* secret;
* raw child-resource reference.

---

# 20. Cancellation Token

`CancellationToken` MAY be an implementation mechanism.

It is NOT an architecture-level requirement.

Architecture depends on cancellation semantics, not one programming-language primitive.

---

# 21. Stable Reason Codes

Recommended reason codes:

```text
APPLICATION_SHUTDOWN
EXECUTION_SCOPE_CLOSED
EXECUTION_REVISION_SUPERSEDED
BUSINESS_PLAN_REPLACED
SOURCE_REPLACED
WORK_SUPERSEDED
ATTEMPT_TIMEOUT
ATTEMPT_PREEMPTED
EXECUTION_BINDING_REPLACED
DEADLINE_EXCEEDED
RESOURCE_PRESSURE
DEPENDENCY_INVALIDATED
SECURITY_CONTAINMENT
RUNTIME_STOPPING
USER_CANCELLED
MANUAL_CANCEL
```

---

# 22. Reason Code Boundary

Reason codes describe why cancellation was requested.

They MUST NOT silently encode:

* Retry decision;
* Fallback decision;
* Business result failure;
* Provider selection decision.

---

# 23. Cancellation Progression

Recommended:

```text
ACTIVE
    |
    v
CANCELLATION_REQUESTED
    |
    v
AUTHORITY_REVOKED
    |
    v
SIGNALING
    |
    v
DRAINING
    |
    +--> ACKNOWLEDGED
    |
    +--> ABANDONED
```

---

# 24. Authority Revocation

Authority revocation is the mandatory first correctness step.

When cancellation is accepted, Runtime Control MUST:

* mark affected Runtime execution no longer eligible;
* prevent new Scheduler admission for affected execution;
* invalidate queued execution eligibility;
* prevent new Attempts under revoked scope;
* reject future Completion acceptance;
* prevent accepted Runtime Artifact publication from revoked execution;
* cancel delayed Retry creation;
* prevent revoked execution from regaining authority.

---

# 25. Runtime Authority vs Business Commit

Critical distinction:

```text
Runtime execution authority
    = may this execution result influence current Runtime?
```

```text
Business commit authority
    = may owning Business Module accept/commit this result?
```

```text
Presentation commit
    = may Presentation/Application show this result now?
```

Cancellation revokes Runtime execution authority.

Business/Presentation owners consume that state when making their own commit decisions.

---

# 26. Three-Layer Protection

## Layer 1 — Prevent Execution

If work has not begun:

```text
Authority revoked
    |
    v
Queued work removed / invalidated
```

---

## Layer 2 — Stop Physical Work

If execution is running:

```text
Cancellation signaled
    |
    v
Worker / Adapter checks cancellation
    |
    v
Stop safely where supported
```

---

## Layer 3 — Reject Late Completion

If work cannot stop:

```text
Physical execution continues
    |
    v
Completion arrives
    |
    v
Authority validation
    |
    v
REJECT_CANCELLED / REJECT_STALE
```

Layer 3 is mandatory.

---

# 27. Queued Work Removal

Correct flow:

```text
Runtime Control revokes authority
        |
        v
QueueRemovalInstruction
        |
        v
Work Queue removes/invalidate matching queued items
```

Queue does not decide cancellation authority.

Queue does not assign terminal WorkItem outcomes.

---

# 28. Queue Eligibility Boundary

Queue MUST NOT independently evaluate:

```text
ExecutionScope active?
ExecutionRevision current?
business dependency valid?
```

Those semantics come from Runtime Control/Scheduler.

Queue MAY verify only the supplied removal/dispatch contract.

---

# 29. Pre-Dispatch Validation

Before Worker ownership transfer, Scheduler/Runtime Control SHOULD revalidate execution eligibility.

Possible checks:

```text
ExecutionScope eligible
ExecutionRevision eligible
WorkItem eligible
Attempt eligible
cancellation authority valid
deadline valid
Runtime admission open
execution requirement still satisfiable
```

Queue performs the atomic dispatch operation.

---

# 30. Running Attempt Cancellation

```text
Execution Authority Revoked
        |
        v
Cancellation Signal
        |
        v
Worker / Execution Adapter Observes Signal
        |
        v
Safe Cancellation Checkpoint
        |
        v
Attempt Stops / Abandons
        |
        v
Completion Reported
```

---

# 31. Cooperative Cancellation

Worker and adapters SHOULD support cooperative cancellation.

Useful checkpoints MAY include:

* before expensive work;
* before provider invocation;
* after provider invocation;
* between bounded batches;
* before large allocation;
* before Runtime Artifact candidate creation;
* before Completion report;
* before irreversible external side effect where contract permits.

---

# 32. Business Module Boundary

Runtime MUST NOT hard-code cancellation checkpoints by:

```text
OCR
Translation
Layout
Presentation
```

Each owner/adapter determines safe checkpoints for its implementation.

Runtime requires only the public cooperative-cancellation contract.

---

# 33. Presentation Boundary

Runtime SHOULD NOT define:

```text
before UI commit
```

as a Worker cancellation checkpoint owned by Runtime.

Instead:

```text
Runtime exposes current authority state

Presentation/Application
    revalidates execution relevance
    before visible commit
```

---

# 34. Physical Cancellation Categories

Physical child operations may be:

```text
FULLY_CANCELABLE
COOPERATIVELY_CANCELABLE
NON_CANCELABLE
PROCESS_TERMINABLE
```

---

# 35. Fully Cancelable

Example:

```text
Signal cancellation
    |
    v
Abort physical operation
    |
    v
Capacity released
    |
    v
Completion/acknowledgment
```

---

# 36. Cooperatively Cancelable

```text
Cancellation requested
    |
    v
Operation reaches checkpoint
    |
    v
Stops safely
```

---

# 37. Non-Cancelable

```text
Execution authority revoked
        |
        v
Runtime stops awaiting logical result
        |
        v
Attempt/child operation becomes abandoned
        |
        v
Physical work may continue
        |
        v
Late Completion rejected
```

---

# 38. ABANDONED

`ABANDONED` means:

```text
Runtime no longer waits for physical completion
but physical work may still exist
```

It does NOT mean:

```text
resource released
provider slot released
billing stopped
process ended
```

---

# 39. Abandoned Attempt vs Child Operation

An Attempt MAY be considered abandoned when Runtime no longer waits for its meaningful completion.

Physical children MAY continue to be tracked separately until they terminate and release resources.

---

# 40. Capacity Truthfulness

If a physical provider/process operation still runs:

* actual provider slot remains occupied;
* concurrency capacity remains consumed;
* billing risk remains possible;
* resources remain tracked;
* late output must be safely consumed/discarded;
* no artificial capacity increase is allowed.

---

# 41. Logical Detach Is Not Resource Release

Critical invariant:

```text
Logical cancellation
    !=
physical release
```

---

# 42. Grace Period

Cancellation wait MUST be bounded.

```text
Cancellation requested
        |
        v
Wait for cooperative stop
        |
        v
Grace deadline exceeded
        |
        v
Mark logical execution abandoned
        |
        v
Stop waiting
        |
        v
Continue resource tracking
```

Exact durations belong to `RUNTIME_CONFIG.md`.

---

# 43. Hard Termination

Hard arbitrary termination is forbidden in the primary process.

Forbidden examples:

* killing arbitrary thread;
* disposing shared memory still leased;
* interrupting unmanaged code without safety guarantee;
* destroying a shared worker without ownership cleanup.

---

# 44. Isolated Process Termination

A child plugin/provider process MAY be terminated when:

* process isolation exists;
* lifecycle/ownership permits termination;
* resources can be contained;
* process restart policy exists;
* diagnostic state is recorded.

This remains an explicit containment action.

---

# 45. Completion After Cancellation

Worker/Adapter SHOULD still report Completion where possible.

Examples:

```text
AttemptCancelled
AttemptAbandoned
AttemptFailed
AttemptCompleted
```

Late/current classification belongs to Runtime authority validation.

---

# 46. Physical Outcome vs Authority Outcome

Example:

```text
Attempt physical outcome:
    COMPLETED

Authority outcome:
    REJECT_CANCELLED
```

or:

```text
Attempt physical outcome:
    COMPLETED

Authority outcome:
    REJECT_STALE
```

This preserves the distinction defined in `PIPELINE_RUNTIME.md`.

---

# 47. Authority Validation

Every Completion after cancellation MUST validate at least:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
current execution authority
cancellation state
accepted WorkItem outcome
duplicate state
Artifact candidate integrity
```

---

# 48. Authority Decisions

Possible:

```text
ACCEPT
REJECT_CANCELLED
REJECT_STALE
REJECT_DUPLICATE
REJECT_INVALID_STATE
REJECT_INTEGRITY
```

---

# 49. Cancellation vs Stale

Cancellation and stale are related but distinct.

```text
CANCELLED
    = explicit execution-authority revocation
```

```text
STALE
    = result no longer relevant/current
```

An Attempt may receive a cancellation request and later have its Completion rejected as either according to authoritative state.

---

# 50. Cancellation vs Failure

```text
Technical Failure
    !=
Cancellation
    !=
Stale Authority Rejection
    !=
Abandoned Physical Execution
```

Cleanup failure is another operational failure and does not restore execution authority.

---

# 51. Cancellation vs Retry

Cancellation does not create Retry.

Correct:

```text
Attempt ends
        |
        v
Runtime Control checks current relevance
        |
        v
Retry Policy evaluates
        |
        v
new AttemptId
        |
        v
Scheduler admission
```

---

# 52. Retry Rule

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

when logical work remains unchanged.

---

# 53. Cancellation Prevents Retry Resurrection

If a delayed Retry is pending and parent authority is revoked:

```text
cancel delayed retry
    |
    v
do not create/admit new Attempt
```

---

# 54. Cancellation vs Fallback

Cancellation MUST NOT select:

```text
another Provider
another Model
another RoutePlan
```

Correct:

```text
old Attempt cancelled/revoked

Routing / Recovery
    selects another execution binding

Pipeline Runtime
    creates another Attempt if appropriate
```

---

# 55. Provider Switch

User/provider-policy switch SHOULD be modeled as:

```text
Current Attempt
    |
    v
execution authority revoked / cancellation requested
```

independently from:

```text
Routing
    chooses new execution binding
```

New Attempt creation occurs only after that new binding is available.

---

# 56. Resource Drain

After cancellation:

```text
Stop New Child Work
        |
        v
Signal Children
        |
        v
Release Temporary Attempt Resources
        |
        v
Release/Detach Physical Operations
        |
        v
Release Artifact Leases
        |
        v
Report Completion/Cleanup
```

Logical cancellation MAY complete before physical resource drain.

---

# 57. Cleanup Ownership

Recommended ownership:

| Resource                        | Default owner              |
| ------------------------------- | -------------------------- |
| Attempt temporary resource      | Worker / Attempt owner     |
| Provider/native child operation | Execution Adapter          |
| Artifact candidate              | Producer until publication |
| Published Runtime Artifact      | Runtime Artifact Store     |
| Artifact Lease                  | Lease holder               |
| Queued position                 | Work Queue                 |
| ExecutionRevision metadata      | Execution State Store      |
| Physical backing resource       | Resource Manager           |
| Presentation/UI handle          | Presentation/Application   |

---

# 58. Cleanup Rules

Cleanup SHOULD be:

* idempotent where practical;
* authority-neutral;
* observable;
* bounded;
* lease-aware;
* safe under duplicate cleanup requests.

Cleanup MUST NOT:

* revive canceled execution;
* restore Runtime authority;
* dispose active leased resources;
* mutate Business results.

---

# 59. Child Operation Registration

A physical child operation SHOULD register before it starts.

```text
Create Child Operation
        |
        v
Link To Parent Attempt Cancellation Context
        |
        v
Register Ownership / Cleanup
        |
        v
Start Physical Operation
```

This prevents child work from escaping cancellation ownership.

---

# 60. Child Operation Record

Recommended:

```text
CancellationChildOperation
├── childOperationId
├── parentAttemptId
├── cancellationContextRef
├── operationType
├── cancelCapability
├── ownershipReference
├── startedAt
└── physicalState
```

---

# 61. Downward Propagation

```text
APPLICATION
    |
    v
EXECUTION_SCOPE
    |
    v
EXECUTION_REVISION
    |
    v
WORK_ITEM
    |
    v
ATTEMPT
    |
    v
REGISTERED CHILD OPERATIONS
```

Upward propagation is not automatic.

---

# 62. Cancellation Race — Completion vs Cancellation

If Completion and cancellation happen concurrently:

```text
Runtime Control serialized authority validation
```

determines whether Completion is accepted or rejected.

---

# 63. Cancellation Race — Revision Replacement During Dispatch

If ExecutionRevision changes during dispatch:

```text
revalidate eligibility
immediately before Worker ownership transfer
```

Then final Completion still undergoes authority validation.

---

# 64. Cancellation Race — Presentation Dispatch Queued

If Presentation work has already crossed into Presentation/UI context:

```text
Presentation/Application
    revalidates current execution relevance
    before visible commit
```

Runtime does not directly own that UI commit.

---

# 65. Cancellation Race — Late Physical Response

If an abandoned physical operation later completes:

```text
consume/discard response safely
        |
        v
release physical resource
        |
        v
reject revoked result
```

---

# 66. Cancellation Race — Retry Timer

Before delayed Retry creates another Attempt:

```text
check current execution authority
```

If revoked:

```text
discard Retry timer
```

---

# 67. User-Initiated Cancellation

Recommended:

```text
User Intent Changes
        |
        v
Application / Business Owner Updates Intent
        |
        v
Cancellation / Replacement Request
        |
        v
Runtime Control Revokes Execution Authority
        |
        v
UI Responds Promptly
        |
        v
Physical Cleanup Continues
```

---

# 68. Business Intent Examples

Possible user actions:

* stop;
* close Reading Session;
* change region;
* switch reading mode;
* request retranslation;
* change execution preference.

Application/Business architecture decides what new intent means.

Runtime executes the resulting cancellation/replacement.

---

# 69. Automatic Cancellation Requests

Runtime cancellation MAY be requested because of:

* newer ExecutionRevision;
* ExecutionScope termination;
* deadline expiration;
* critical Runtime resource pressure;
* dependency invalidation;
* BusinessExecutionPlan replacement;
* Runtime shutdown;
* security containment;
* recovery/routing decision.

---

# 70. Health Boundary

Provider/Plugin health degradation itself does NOT grant Cancellation authority.

Recommended:

```text
Health / Recovery / Routing Policy
        |
        v
contain / rebind / cancel decision
        |
        v
Cancellation Request
```

Cancellation architecture executes the request.

---

# 71. Resource Pressure Boundary

Scheduler/Resource policy MAY recommend or request cancellation under critical pressure.

Cancellation component does not invent Resource policy.

---

# 72. Events

Possible normalized events:

```text
CancellationRequested
ExecutionAuthorityRevoked
CancellationPropagated
CancellationAcknowledged
CancellationGraceExpired
AttemptAbandoned
QueuedWorkRemovalRequested
LateCompletionRejected
PhysicalChildCancellationFailed
```

---

# 73. Event Payload

Recommended:

```text
eventId
occurredAt
scopeType
scopeId
executionScopeId?
executionRevisionId?
workItemId?
attemptId?
reasonCode
requestedBy
requestedAt
graceDeadline?
outcome?
correlationId?
```

---

# 74. Event Privacy

Cancellation events MUST NOT contain:

* screenshot;
* source/OCR text;
* translated text;
* Prompt;
* AI Context;
* raw provider request;
* secret.

---

# 75. Logging

Cancellation logs SHOULD include only execution/control metadata such as:

```text
scope
reason
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
authorityRevokedAt
acknowledgedAt
graceDeadline
drain state
physical child state
```

---

# 76. Metrics

Recommended:

```text
cancellation request count by scope
authority-revocation latency
queue-removal latency
worker acknowledgment latency
cancellation logical-completion latency
abandoned Attempt count
physical abort success ratio
late Completion count
late-result rejection count
cleanup failure count
reason-code distribution
resource drain duration
capacity retained after logical cancellation
```

---

# 77. Cancellation Failure

Physical cancellation MAY fail.

Examples:

* provider abort fails;
* worker does not acknowledge;
* child operation ignores signal;
* resource cleanup fails;
* isolated process does not stop.

The correctness rule remains:

```text
execution authority stays revoked
```

Failure to stop physically MUST NOT restore logical execution authority.

---

# 78. Cancellation Failure Handling

Possible:

```text
record diagnostics
mark Attempt/child abandoned
continue resource tracking
request stronger containment
eventually terminate isolated process if policy allows
```

---

# 79. Shutdown Integration

Application shutdown uses:

```text
APPLICATION
```

scope.

Recommended:

```text
Stop New Admission
        |
        v
Revoke Application Execution Authority
        |
        v
Remove Queued Work
        |
        v
Signal Running Attempts
        |
        v
Signal Physical Child Operations
        |
        v
Wait Bounded Grace
        |
        v
Mark Remaining Work Abandoned
        |
        v
Drain Resources
```

---

# 80. Scheduler Boundary

Scheduler does NOT own cancellation authority.

Scheduler MAY:

* stop admitting canceled work;
* apply eligibility projection;
* recommend preemption;
* react to cancellation signals.

---

# 81. Work Queue Boundary

Work Queue does NOT own cancellation authority.

It receives explicit:

```text
QueueRemovalInstruction
```

and atomically removes/invalidate queued work.

---

# 82. Worker Boundary

Worker MUST:

* observe cancellation context;
* stop safely where possible;
* cleanup owned temporary resources;
* report physical outcome.

Worker MUST NOT determine canonical WorkItem terminal outcome.

---

# 83. Provider Runtime Boundary

Provider Runtime Gateway / adapters MUST:

* expose cancellation capability accurately;
* attempt supported abort;
* maintain truthful physical capacity state;
* return normalized physical outcome;
* cleanup late results safely.

They MUST NOT silently claim cancellation completed when physical work remains active.

---

# 84. Runtime Artifact Boundary

A canceled/revoked Attempt MUST NOT publish an accepted Runtime Artifact.

Temporary/candidate Artifact cleanup remains producer/resource-owner responsibility.

Already-published immutable Artifacts follow normal retention/lease rules.

---

# 85. Presentation Boundary

Presentation/Application should reflect logical cancellation promptly.

Example:

* loading state may stop;
* previous accepted content may remain;
* canceled output does not replace newer content;
* Presentation does not wait for remote cleanup.

Exact visual behavior belongs to Presentation.

---

# 86. Domain Boundary

Cancellation does NOT automatically roll back already committed Domain state.

If business rollback is required, it is a separate owning-module operation.

---

# 87. Persistence Boundary

Cancellation of runtime execution does NOT automatically delete durable data.

Storage retention/deletion follows owning Business/Storage policy.

---

# 88. Architecture Invariants

1. Cancellation begins with Runtime execution-authority revocation.

2. Logical cancellation occurs before physical stop.

3. Physical stop is best-effort within safe execution guarantees.

4. Late Completion always passes authority validation.

5. Cancellation is not Failure by default.

6. Stale is distinct from Cancellation.

7. Abandoned is distinct from Cancellation acknowledgment.

8. Abandoned does not imply physical resource release.

9. Grace period is bounded.

10. Parent cancellation propagates downward.

11. Child cancellation does not automatically cancel parent.

12. Canonical scopes are Application, ExecutionScope, ExecutionRevision, WorkItem and Attempt.

13. Physical provider/native operations are child operations, not mandatory canonical authority scopes.

14. ReadingSession is not a Runtime cancellation scope.

15. ExecutionRevision is distinct from Domain revisions.

16. Runtime Control owns cancellation authority.

17. Scheduler does not own cancellation authority.

18. Work Queue does not own cancellation authority.

19. Worker does not determine canonical WorkItem outcome.

20. Cancellation does not create Retry automatically.

21. Cancellation does not select Fallback.

22. Provider switch and Attempt cancellation are separate decisions.

23. New execution binding is supplied by Routing/Recovery.

24. Delayed Retry cannot resurrect revoked work.

25. Canceled execution cannot produce accepted Runtime Artifact publication.

26. Runtime cancellation does not own Business result correctness.

27. Runtime cancellation does not own Presentation/UI commit semantics.

28. Cleanup failure never restores execution authority.

29. Hard arbitrary thread termination is forbidden in primary process.

30. Isolated process termination requires explicit containment policy.

31. Logical detach does not imply provider/resource capacity release.

32. Resource accounting remains truthful after abandonment.

33. Physical child operations must remain trackable after logical abandonment.

34. Queue removal follows explicit instruction.

35. Queue does not independently infer execution eligibility.

36. Pre-dispatch authority validation occurs outside Queue policy.

37. Business/Presentation commits consume Runtime authority but remain owner-controlled.

38. Cancellation telemetry contains no user content by default.

39. Cancellation correctness does not depend on telemetry.

40. Shutdown uses Application-scope cancellation semantics.

---

# 89. Recommended MVP

CRAI MVP SHOULD support:

* APPLICATION scope;
* EXECUTION_SCOPE scope;
* EXECUTION_REVISION scope;
* WORK_ITEM scope;
* ATTEMPT scope;
* hierarchical cancellation contexts;
* lightweight CancellationContextRef;
* immediate execution-authority revocation;
* queued-work removal;
* cooperative Worker cancellation;
* provider/native child operation registration;
* cancelable/cooperative/non-cancelable classification;
* bounded grace period;
* ABANDONED tracking;
* late Completion rejection;
* truthful resource/capacity tracking;
* delayed Retry cancellation;
* graceful shutdown integration;
* content-free cancellation telemetry.

MVP SHOULD NOT require:

* provider request as canonical Runtime scope;
* hard primary-process thread termination;
* physical cancellation guarantees for all providers;
* cancellation-triggered automatic Fallback.

---

# 90. Open Decisions

The following remain open:

* exact CancellationContext schema;
* cancellation-context storage;
* parent-child implementation;
* grace period defaults by execution class;
* child-operation registration interface;
* physical cancellation capability taxonomy;
* abandoned resource tracking;
* process termination policy;
* cancellation acknowledgment semantics;
* WorkItem outcome after abandoned Attempt;
* UI cancellation presentation behavior;
* cancellation under memory pressure;
* partial output after cancellation;
* cleanup Retry budget;
* recovery action after physical cancellation failure.

---

# 91. Testing Requirements

Tests SHOULD include:

* cancel before admission;
* cancel while queued;
* cancel between selection and Worker ownership transfer;
* cancel running Attempt;
* simultaneous Completion and cancellation;
* ExecutionScope cancellation;
* ExecutionRevision supersession;
* WorkItem cancellation;
* Attempt cancellation;
* child operation abort supported;
* child operation abort unsupported;
* cancellation grace expiration;
* late Completion;
* Retry timer canceled;
* Fallback decision separate from cancellation;
* provider switch with new binding;
* cleanup idempotency;
* parent-child propagation;
* Presentation revalidation;
* shutdown cancellation;
* resource still occupied after logical abandonment;
* duplicate cancellation request;
* isolated process termination;
* queue removal race.

---

# 92. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `RETRY_POLICY.md`
* `ERROR_MODEL.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `THREADING_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`
* `BOOT_SEQUENCE.md`

External:

* `../ai/FALLBACK.md`
* `../ai/RETRY.md`
* `../plugin/PLUGIN_LIFECYCLE.md`
* `../../02-modules/provider-management/`
* `../../02-modules/presentation/`

---

# 93. Completion Criteria

`CANCELLATION.md` is synchronized when:

* cancellation begins with Runtime authority revocation;
* canonical scopes use ExecutionScope/ExecutionRevision terminology;
* provider/native operation becomes a child operation rather than required canonical scope;
* CancellationContext remains implementation-independent;
* WorkItem lifecycle is not redefined;
* Queue removal matches Work Queue ownership;
* Queue does not independently validate business/runtime authority;
* cooperative cancellation stays generic;
* ABANDONED remains distinct from CANCELLED;
* Retry and Fallback remain separate;
* Provider switch does not make Cancellation choose another Provider;
* Business/Presentation commit ownership stays external;
* late results always undergo Runtime authority validation;
* resource/capacity state remains truthful;
* events/logs remain content-free.

---

# 94. Summary

CRAI Runtime Cancellation follows:

```text
Cancellation Request
        |
        v
Execution Authority Revoked
        |
        v
Prevent New Execution
        |
        v
Remove Queued Work
        |
        v
Signal Running Attempts
        |
        v
Signal Physical Child Operations
        |
        +--> Stop
        |
        +--> Abandon
        |
        v
Reject Late Completion
        |
        v
Drain Physical Resources Safely
```

The central rule is:

```text
Cancellation protects correctness
by revoking Runtime authority first.

Physical stopping only reduces wasted work
and releases resources when safely possible.
```
