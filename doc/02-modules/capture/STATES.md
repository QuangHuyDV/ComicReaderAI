# Capture States

> **Project:** CRAI
> **Module:** `capture`
> **Path:** `doc/02-modules/capture/STATES.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the Capture-owned state model.

It specifies:

```text
CaptureSource lifecycle
CaptureOperation local phases
Candidate Capture lifecycle
Capture health
state transitions
transition guards
source replacement
permission loss
local resource cleanup
Runtime cancellation cooperation
recovery
state invariants
```

This state model describes Capture-owned state only.

It does not define:

```text
RuntimeRevision lifecycle
WorkItem lifecycle
Attempt lifecycle
Scheduler queue state
Runtime retry state
Runtime cancellation authority
Runtime deadline state
Artifact publication state
Recognition state
Reading Session state
UI lifecycle
```

---

# 2. State Ownership

Capture owns:

```text
CaptureSourceState
CaptureOperationPhase
CandidateCaptureState
CaptureHealthState
SourceVersion
Capture-local temporary resource state
```

Runtime owns:

```text
RuntimeRevision
WorkItemState
AttemptState
Queue state
Scheduler state
Cancellation authority
Retry lifecycle
Deadline enforcement
Completion acceptance
```

Artifact Store owns:

```text
accepted CapturedFrameArtifact lifecycle
Artifact lease state
Artifact retention
Artifact disposal
```

Capture Provider owns:

```text
native provider resource state
native stream mechanics
platform capture callbacks
platform permission integration
```

---

# 3. State Machine Overview

Capture v2 uses three primary Capture-owned state domains:

```text
Capture
├── CaptureSource State Machine
├── CaptureOperation Local Phase
└── CaptureHealth State Machine
```

A small Candidate lifecycle exists inside one CaptureOperation but does not require a globally addressable state machine.

There is no Capture-owned:

```text
Continuous Capture Session State Machine
Runtime Job State Machine
Capture Retry State Machine
Generation Authority State Machine
```

---

# 4. Why Runtime States Were Removed

The previous model included Capture states such as:

```text
Queued
Cancelled
TimedOut
```

with semantics tied directly to:

```text
Work Queue
Scheduler
Runtime deadline
Runtime cancellation
```

Runtime v2 already owns these concepts.

Capture may observe Runtime context, but it must not maintain a competing lifecycle.

Therefore:

```text
Queued
```

is removed from Capture state ownership.

Runtime cancellation is represented as an observed condition, not a Capture-owned terminal authority state.

Runtime deadline expiration is a Runtime outcome, although Capture may still classify provider-specific timeout failures.

---

# 5. CaptureSource State Machine

CaptureSource lifecycle represents whether one logical capture source can currently serve Capture operations.

States:

```text
UNINITIALIZED
INITIALIZING
READY
SUSPENDED
UNAVAILABLE
STOPPING
STOPPED
```

---

# 6. `UNINITIALIZED`

## Meaning

Logical CaptureSource identity exists or is being requested, but Capture has not established a usable provider source.

Typical characteristics:

```text
no active provider source lease
no validated capabilities
no usable capture operation
```

## Allowed Next States

```text
INITIALIZING
STOPPED
```

---

# 7. `INITIALIZING`

## Meaning

Capture is establishing the logical source through the provider boundary.

Possible actions:

```text
validate source descriptor
validate normalized permission requirements
resolve provider capabilities
acquire provider source reference
validate source scope
establish SourceVersion
```

Capture core does not directly manipulate native handles.

## Allowed Next States

```text
READY
UNAVAILABLE
STOPPING
```

---

# 8. `READY`

## Meaning

The CaptureSource is valid and may accept Capture operations.

Important:

```text
READY does not mean
a Runtime Capture Attempt is currently running.
```

The previous `Active` source state is removed as unnecessary in v2.

Execution activity belongs to Runtime/operation scope, not source lifecycle.

## Allowed Next States

```text
SUSPENDED
UNAVAILABLE
INITIALIZING
STOPPING
```

`READY → INITIALIZING` may be used for explicit source reconfiguration/reacquisition when SourceVersion changes.

---

# 9. Why `ACTIVE` Was Removed

The previous source lifecycle used:

```text
READY
  ↓
ACTIVE
```

where `ACTIVE` meant the source was currently serving Capture operations.

That couples CaptureSource lifecycle to Runtime execution activity.

A source may be:

```text
READY
```

while:

* zero Capture operations are running;
* one Runtime Attempt is invoking Capture;
* several sequential Attempts use the source over time.

Therefore operation activity is tracked separately.

---

# 10. `SUSPENDED`

## Meaning

CaptureSource still logically exists but new Capture operations must not begin.

Possible causes:

```text
temporary user suspension
temporary permission limitation
application policy
provider temporary pause
```

Provider-native resources may be retained or released depending on provider/platform capability.

## Allowed Next States

```text
READY
UNAVAILABLE
STOPPING
```

---

# 11. `UNAVAILABLE`

## Meaning

Capture cannot currently use the source.

Examples:

```text
required permission unavailable
provider unavailable
native source disappeared
display disconnected
browser connector unavailable
source reference invalidated
```

This state does not mean Reading Session is invalid.

It also does not mean Runtime failed globally.

## Allowed Next States

```text
INITIALIZING
STOPPING
```

A source must normally re-establish provider state before returning to READY.

---

# 12. `STOPPING`

## Meaning

Capture is logically closing the CaptureSource.

Actions may include:

```text
reject new Capture invocations
invalidate Capture-local source leases
close provider source reference
release Capture-owned temporary resources
stop provider-native stream if one exists
```

Capture may signal or observe Runtime cancellation context, but it does not directly transition Runtime Attempts to `Cancelled`.

## Allowed Next State

```text
STOPPED
```

---

# 13. `STOPPED`

## Meaning

The CaptureSource is terminal.

Characteristics:

```text
not usable
no new Capture operations accepted
Capture-owned provider references released
```

The same logical source may later be represented by a new CaptureSource instance.

`STOPPED → READY` is invalid.

---

# 14. CaptureSource Diagram

```text
UNINITIALIZED
      ↓
INITIALIZING
   ┌──┼─────────┐
   ↓  ↓         ↓
READY UNAVAILABLE STOPPING
  │       │         ↓
  ↓       ↓       STOPPED
SUSPENDED │
  │       │
  └──→ INITIALIZING
```

Simplified normal flow:

```text
UNINITIALIZED
      ↓
INITIALIZING
      ↓
READY
  ↕
SUSPENDED
      ↓
STOPPING
      ↓
STOPPED
```

---

# 15. CaptureSource Valid Transitions

| From            | To             |
| --------------- | -------------- |
| `UNINITIALIZED` | `INITIALIZING` |
| `UNINITIALIZED` | `STOPPED`      |
| `INITIALIZING`  | `READY`        |
| `INITIALIZING`  | `UNAVAILABLE`  |
| `INITIALIZING`  | `STOPPING`     |
| `READY`         | `SUSPENDED`    |
| `READY`         | `UNAVAILABLE`  |
| `READY`         | `INITIALIZING` |
| `READY`         | `STOPPING`     |
| `SUSPENDED`     | `READY`        |
| `SUSPENDED`     | `UNAVAILABLE`  |
| `SUSPENDED`     | `STOPPING`     |
| `UNAVAILABLE`   | `INITIALIZING` |
| `UNAVAILABLE`   | `STOPPING`     |
| `STOPPING`      | `STOPPED`      |

---

# 16. SourceVersion

`SourceVersion` is a Capture-owned semantic version.

It may advance when:

```text
region changes
source descriptor changes
provider-native source is re-established incompatibly
capabilities change materially
logical source association changes
```

SourceVersion is not a lifecycle state.

It is also not Runtime authority.

---

# 17. Source Replacement

Source replacement follows:

```text
CaptureSource S / version N / READY
        ↓
ReplaceCaptureSource
        ↓
validate replacement
        ↓
source temporarily stops accepting new work
        ↓
provider resource replaced
        ↓
SourceVersion N+1
        ↓
READY
```

An implementation may use:

```text
READY → INITIALIZING → READY
```

for this transition.

---

# 18. Old Source Results

A Capture Candidate may reference:

```text
CaptureSourceId = S
SourceVersion = N
```

while current source is:

```text
S / version N+1
```

Capture may reject the Candidate as source-incompatible.

This is different from Runtime authority rejection.

Both checks may independently apply.

---

# 19. CaptureOperation Local Phase

CaptureOperation has Capture-local processing phases.

Recommended phases:

```text
VALIDATING
ACQUIRING
NORMALIZING
VALIDATING_CANDIDATE
COMPLETING
FINISHED
```

Optional local end outcomes:

```text
REJECTED
FAILED
ABORTED_LOCAL
```

These are not Runtime Attempt terminal states.

---

# 20. Why This Is a Phase Model

Runtime already owns the externally meaningful execution lifecycle.

Therefore CaptureOperation state should primarily answer:

> Which Capture-owned processing stage is currently executing?

It should not duplicate:

```text
Queued
Retrying
AttemptCancelled
AttemptTimedOut
```

---

# 21. `VALIDATING`

Capture validates:

```text
CaptureInvocation
CaptureSource
SourceVersion
requested region
capabilities
configuration
privacy scope
```

No provider acquisition has begun yet.

Possible next phases:

```text
ACQUIRING
REJECTED
ABORTED_LOCAL
```

---

# 22. `ACQUIRING`

CaptureProvider is acquiring source data.

Capture may:

* invoke provider;
* wait for bounded provider completion;
* observe Runtime cancellation;
* receive provider-specific failure.

Possible next phases:

```text
NORMALIZING
FAILED
ABORTED_LOCAL
```

---

# 23. `NORMALIZING`

A raw provider result exists.

Capture converts it into normalized Candidate semantics.

Possible work:

```text
validate raw dimensions
normalize pixel representation
normalize orientation
normalize source geometry
normalize scale metadata
create temporary frame representation
```

Possible next phases:

```text
VALIDATING_CANDIDATE
FAILED
ABORTED_LOCAL
```

---

# 24. `VALIDATING_CANDIDATE`

Capture validates Candidate-specific invariants.

Examples:

```text
source identity
SourceVersion
dimensions
pixel format
buffer bounds
coordinate space
privacy scope
temporary resource lifetime
```

Possible next phases:

```text
COMPLETING
REJECTED
FAILED
ABORTED_LOCAL
```

---

# 25. `COMPLETING`

Capture constructs:

```text
CaptureCompletion
+
CandidateCaptureResult
```

and transfers the Candidate to the Runtime completion boundary.

Capture has not published an accepted Artifact.

Possible next phase:

```text
FINISHED
```

---

# 26. `FINISHED`

Capture-owned processing has completed.

This means only:

```text
Capture returned a completion outcome
to Runtime.
```

It does not mean:

```text
Runtime Attempt succeeded
Candidate was accepted
CapturedFrameArtifact was published
Recognition started
```

---

# 27. `REJECTED`

Capture rejected the request or Candidate for a deterministic Capture-owned semantic reason.

Examples:

```text
invalid region
unsupported capability
SourceVersion conflict
privacy scope violation
invalid Candidate geometry
```

This is not Runtime Attempt failure.

---

# 28. `FAILED`

Capture encountered a Capture-owned processing failure.

Examples:

```text
provider failure
normalization failure
temporary resource failure
invalid raw provider result
```

Runtime decides how this maps to Attempt outcome/retry.

---

# 29. `ABORTED_LOCAL`

Capture stopped useful local processing because continuing no longer made sense.

Typical causes:

```text
Runtime cancellation observed
CaptureSource entered STOPPING
provider stream terminated
local operation superseded by source replacement
```

This does not mean:

```text
Runtime Attempt state = Cancelled
```

Runtime remains authoritative.

---

# 30. CaptureOperation Phase Diagram

```text
VALIDATING
   │
   ├──────→ REJECTED
   │
   └──────→ ABORTED_LOCAL
   ↓
ACQUIRING
   │
   ├──────→ FAILED
   └──────→ ABORTED_LOCAL
   ↓
NORMALIZING
   │
   ├──────→ FAILED
   └──────→ ABORTED_LOCAL
   ↓
VALIDATING_CANDIDATE
   │
   ├──────→ REJECTED
   ├──────→ FAILED
   └──────→ ABORTED_LOCAL
   ↓
COMPLETING
   ↓
FINISHED
```

---

# 31. Removed `QUEUED`

CaptureOperation does not own:

```text
QUEUED
```

because queue state belongs to Runtime Work Queue/Scheduler.

By the time Capture is invoked, Runtime already decided that the Attempt may execute.

---

# 32. Removed Runtime `CANCELLED`

CaptureOperation does not use `CANCELLED` as an authoritative Runtime terminal state.

Instead:

```text
Runtime cancellation observed
        ↓
Capture may stop local work
        ↓
ABORTED_LOCAL
        ↓
Runtime determines Attempt terminal state
```

---

# 33. Removed Runtime `TIMED_OUT`

Runtime deadline expiration belongs to Runtime.

Capture may still encounter:

```text
ProviderTimeout
```

as a Capture-specific provider failure.

Do not conflate:

```text
ProviderTimeout
```

with:

```text
Runtime Attempt deadline exceeded
```

---

# 34. CandidateCaptureState

Candidate Capture state is internal and small.

Conceptually:

```text
BUILDING
VALID
TRANSFERRED
DISCARDED
```

It does not need to be a public standalone state machine for MVP.

---

# 35. Candidate `BUILDING`

Capture is constructing normalized Candidate data.

Candidate is not immutable yet.

---

# 36. Candidate `VALID`

Capture validation succeeded.

Candidate becomes immutable for transfer to Runtime.

It is still not an accepted Artifact.

---

# 37. Candidate `TRANSFERRED`

Candidate ownership/lifetime responsibility has reached the Runtime completion boundary according to the temporary resource contract.

Runtime may subsequently:

```text
accept
or
reject
```

the completion.

---

# 38. Candidate `DISCARDED`

Candidate temporary resources were released because:

```text
Capture rejected it
Capture aborted locally
Runtime rejected completion
temporary lease expired
```

A discarded Candidate cannot later become an accepted Artifact.

---

# 39. Artifact Publication Is Not a Capture State

After Runtime authority acceptance:

```text
CandidateCaptureResult
        ↓
Runtime
        ↓
Artifact publication
        ↓
CapturedFrameArtifact
```

The resulting Artifact lifecycle does not transition CaptureOperation into another state.

Capture processing has already finished.

---

# 40. Capture Health State Machine

Capture health represents whether Capture capability is generally usable.

States:

```text
HEALTHY
DEGRADED
UNAVAILABLE
RECOVERING
STOPPED
```

---

# 41. `HEALTHY`

Capture capability is operating normally.

Typical conditions:

```text
provider reachable
required source operations succeed
latency within expected Capture budget
local buffering bounded
```

---

# 42. `DEGRADED`

Capture remains usable but quality or reliability is reduced.

Examples:

```text
provider latency increased
local samples dropped
temporary source instability
provider reconnect frequency increased
local resource pressure
```

A rise in Runtime retry count is not itself a Capture-owned health metric unless correlated externally.

---

# 43. `UNAVAILABLE`

Capture capability cannot currently serve required operations.

Examples:

```text
provider unavailable
permission unavailable
all required sources unavailable
platform capture capability lost
```

---

# 44. `RECOVERING`

Capture is attempting Capture-owned recovery such as:

```text
reacquire provider
recreate provider source
refresh capabilities
wait for externally restored permission
```

Capture recovery must not create hidden Runtime retry loops.

---

# 45. `STOPPED`

Capture capability has been shut down.

No CaptureSource operation should be initiated.

---

# 46. Health Transitions

Typical:

```text
HEALTHY → DEGRADED
DEGRADED → HEALTHY
DEGRADED → UNAVAILABLE
UNAVAILABLE → RECOVERING
RECOVERING → HEALTHY
RECOVERING → DEGRADED
RECOVERING → UNAVAILABLE
* → STOPPED
```

Health transitions are Capture-owned observations.

---

# 47. Health Does Not Drive Runtime State Directly

Invalid:

```text
CaptureHealth = UNAVAILABLE
    ↓
Runtime Attempts automatically mutated by Capture
```

Correct:

```text
CaptureHealth = UNAVAILABLE
    ↓
fact/query exposed
    ↓
Runtime/Application may react through their own policies
```

---

# 48. Cross-State Rule — Starting Capture

A Capture operation may proceed into provider acquisition only when:

```text
CaptureSourceState = READY
```

unless a provider-specific internal rule explicitly allows another state.

---

# 49. Cross-State Rule — Source Suspension

If source transitions:

```text
READY → SUSPENDED
```

new Capture operations must not begin.

An operation already acquiring may:

* complete if policy/provider permits;
* observe cancellation;
* abort locally;
* later have completion rejected by Runtime.

Capture does not automatically mark Runtime Attempt canceled.

---

# 50. Cross-State Rule — Source Stopping

When:

```text
CaptureSource → STOPPING
```

Capture must:

```text
reject new acquisitions
invalidate source-local provider leases
request/perform provider-local shutdown
allow or stop existing local operations according to cancellation/provider capability
```

Existing Runtime Attempts remain Runtime-owned.

---

# 51. Cross-State Rule — SourceVersion Change

If an operation was created for:

```text
SourceVersion N
```

and the source becomes:

```text
SourceVersion N+1
```

before Candidate validation, Capture must check whether the result is still semantically compatible.

If incompatible:

```text
REJECTED
or
ABORTED_LOCAL
```

No Runtime-generation mechanism is required.

---

# 52. Cross-State Rule — Health Unavailable

When Capture health is:

```text
UNAVAILABLE
```

new operations may be rejected with a Capture-specific unavailable failure.

Runtime decides whether to retry later.

---

# 53. Permission Revocation

Permission loss is represented through CaptureSource/Health state.

Typical flow:

```text
READY
  ↓
permission revoked
  ↓
UNAVAILABLE
```

or, when temporary suspension semantics are appropriate:

```text
READY
  ↓
SUSPENDED
```

The exact choice depends on whether the source can be resumed without provider reacquisition.

---

# 54. Permission Restoration

If source requires re-establishment:

```text
UNAVAILABLE
    ↓
INITIALIZING
    ↓
READY
```

If platform/provider can safely resume existing source:

```text
SUSPENDED
    ↓
READY
```

---

# 55. Source Lost

Typical:

```text
READY
  ↓
UNAVAILABLE
  ↓
INITIALIZING
```

if recovery is possible.

Or:

```text
READY
  ↓
STOPPING
  ↓
STOPPED
```

when logical source is permanently removed.

Reading Session does not decide Capture source recreation directly.

Application/orchestration owns the cross-module decision.

---

# 56. Runtime Cancellation

Runtime cancellation flow:

```text
Runtime cancellation becomes observable
        ↓
Capture operation checkpoint
        ↓
stop provider/local work when possible
        ↓
ABORTED_LOCAL or no Candidate
        ↓
Runtime determines Attempt outcome
```

No Capture-owned Runtime cancellation state exists.

---

# 57. Runtime Revision Supersession

Capture does not subscribe to `Generation Change` and transition operations itself.

Preferred flow:

```text
Runtime establishes newer authority
        ↓
old Capture Attempt may receive cancellation
        ↓
Capture cooperates
        ↓
late Candidate reaches Runtime
        ↓
Runtime rejects authority if obsolete
```

---

# 58. Removal of Generation Change State Rule

The old rule:

```text
Reading Generation changes
    ↓
Running Operation → Cancelled
```

is removed.

Reason:

```text
ReadingContextRevision
RuntimeRevisionId
SourceVersion
```

are separate domains.

Capture must not infer Runtime authority from Reading Session revision changes.

---

# 59. Runtime Shutdown

Runtime/Application shutdown may cause Capture shutdown through explicit lifecycle integration.

Capture-side behavior:

```text
stop accepting new Capture operations
        ↓
CaptureSources → STOPPING
        ↓
release Capture-owned resources
        ↓
CaptureSources → STOPPED
        ↓
CaptureHealth → STOPPED
```

Runtime controls WorkItem/Attempt cancellation separately.

---

# 60. No Global Continuous Capture State Machine

Capture v2 does not own:

```text
ContinuousCaptureSession
```

with states:

```text
Created
Starting
Running
Paused
Stopping
Stopped
```

Repeated sampling belongs to Runtime orchestration/scheduling.

---

# 61. Runtime-Scheduled Repeated Capture

Preferred:

```text
Runtime policy
    ↓
Capture WorkItem 1
    ↓
Capture operation

Runtime policy
    ↓
Capture WorkItem 2
    ↓
Capture operation
```

Each Capture invocation remains bounded.

---

# 62. Provider-Native Continuous Stream

If provider requires a long-lived stream, it is provider/source mechanics.

Possible internal provider states:

```text
OPENING
OPEN
PAUSED
CLOSING
CLOSED
```

These are not required as Capture public module states.

They remain behind CaptureProvider unless architecture later requires exposure.

---

# 63. Provider Stream and Runtime Authority

A native stream may physically continue while Runtime authority changes.

Therefore:

```text
provider stream open
≠
Capture result accepted
```

Every Candidate result still requires Runtime completion validation.

---

# 64. Timeout Behavior

Two timeout domains must remain separate.

## Provider Timeout

CaptureProvider failed to return within provider-specific bounded behavior.

This may produce:

```text
Capture FAILED
error = ProviderTimeout
```

## Runtime Deadline

Runtime Attempt deadline expired.

Capture may observe cancellation/deadline context and stop locally.

Runtime owns the terminal execution outcome.

---

# 65. Late Result

Late provider result behavior:

```text
provider returns after local cancellation/deadline observation
        ↓
Capture checks local operation/cancellation state
        ↓
discard before Candidate when possible
```

If Candidate is still produced:

```text
Runtime authority validation
```

remains final protection.

---

# 66. Illegal Capture States

Invalid:

```text
CaptureSource = STOPPED
CaptureOperation ACQUIRING against that source
```

unless the operation already held a valid isolated provider snapshot and policy explicitly permits completion.

Default MVP policy:

```text
do not permit new acquisition
```

---

# 67. Illegal Source State

Invalid:

```text
CaptureSource = READY
required provider reference absent
```

If provider reference is unavailable:

```text
UNAVAILABLE
or
INITIALIZING
```

must be used.

---

# 68. Illegal Candidate State

Invalid:

```text
CandidateCaptureResult = VALID
but normalized frame representation missing
```

---

# 69. Illegal Artifact Assumption

Invalid:

```text
CaptureOperation FINISHED
    ⇒
CapturedFrameArtifact exists
```

Runtime may reject Candidate completion.

Therefore Artifact existence cannot be inferred from CaptureOperation phase.

---

# 70. State/Event Relationship

Capture events describe Capture-owned facts only.

Typical mappings:

| Transition                      | Possible Event             |
| ------------------------------- | -------------------------- |
| `INITIALIZING → READY`          | `CaptureSourceReady`       |
| `READY → SUSPENDED`             | `CaptureSourceSuspended`   |
| `READY/SUSPENDED → UNAVAILABLE` | `CaptureSourceUnavailable` |
| source version changes          | `CaptureSourceChanged`     |
| `STOPPING → STOPPED`            | `CaptureSourceStopped`     |
| health changes                  | `CaptureHealthChanged`     |

Capture operation terminal facts should normally return through Runtime completion rather than a competing success/failure Event Bus lifecycle.

---

# 71. Removed Operation Events

Capture v2 should not rely on:

```text
CaptureCompleted
CaptureCancelled
CaptureTimeout
CaptureFrameReady
```

as an independent execution lifecycle.

Reasons:

```text
Capture completion
    → Runtime processing completion

Cancellation
    → Runtime-owned

Timeout
    → provider failure or Runtime-owned deadline

Frame readiness
    → Candidate / Artifact boundary
```

Detailed event policy belongs to `EVENTS.md`.

---

# 72. Event Timing

For CaptureSource/Health facts:

```text
state transition
    ↓
state committed
    ↓
event publication
```

Success event must not precede the state change.

---

# 73. Source Transition Idempotency

Repeated commands should converge safely.

Examples:

```text
Suspend already SUSPENDED
Remove already STOPPED
```

should normally return no-op/idempotent result.

They should not generate unnecessary SourceVersion changes.

---

# 74. Source Mutation Concurrency

CaptureSource mutation should use SourceVersion optimistic concurrency where appropriate.

Example:

```text
Current SourceVersion = 8

Command A expects 8
Command B expects 8

B commits version 9

A reaches commit
    ↓
SourceVersionConflict
```

This does not affect RuntimeRevisionId.

---

# 75. Operation Concurrency

Capture may enforce:

```text
maximumConcurrentAcquisitionPerSource = 1
```

for MVP.

If another Runtime Attempt invokes Capture simultaneously:

* Capture may reject locally;
* Runtime may queue beforehand;
* provider capability may later allow more concurrency.

Capture does not own the global queue.

---

# 76. Temporary Resource Lifecycle

Capture-owned temporary resources include:

```text
provider temporary buffer
raw provider result
Candidate frame buffer
source-local sample buffer
provider source lease
```

They must have deterministic release.

---

# 77. Temporary Resource Release

Release occurs on:

```text
local rejection
Capture failure
ABORTED_LOCAL
Candidate discard
Runtime authority rejection notification/lease release
source removal
shutdown
temporary lease expiry
```

Accepted Artifact resources follow Artifact Store lifetime rules.

---

# 78. Previous Accepted Artifact

Capture does not own or maintain:

```text
current CapturedFrameArtifact
```

as source state.

Artifact availability/reuse belongs to Runtime Artifact Store/Business Pipeline Orchestration.

---

# 79. Health Recovery

Capture-owned recovery may include:

```text
provider reconnect
source reinitialization
capability refresh
provider resource recreation
```

It must not:

```text
schedule Runtime retry
create WorkItems
change RuntimeRevision
```

---

# 80. Health and Permission Waiting

`RECOVERING` may include waiting for externally restored permission only when this does not create repeated prompt loops.

Capture itself must not repeatedly trigger user permission prompts.

---

# 81. State Snapshot

Capture may expose diagnostic snapshots.

Example:

```text
CaptureSourceStateSnapshot
├── captureSourceId
├── sourceVersion
├── sourceState
├── capabilitySummary
├── healthState
├── activeLocalOperationCount
├── recentIssueCode?
└── observedAt
```

No native handle is exposed.

---

# 82. Operation Diagnostic Snapshot

```text
CaptureOperationDiagnostic
├── operationId
├── phase
├── captureSourceId
├── sourceVersion
├── runtimeRevisionId?
├── workItemId?
├── attemptId?
├── cancellationObserved
├── providerName?
├── durationMs?
└── issueCode?
```

This is diagnostic only.

It does not redefine Runtime Attempt state.

---

# 83. Persistence

CaptureSource lifecycle does not require durable persistence for MVP.

Persisted source preferences/descriptors may be restored through Application/Storage.

Restored data must be validated before source enters READY.

---

# 84. Restoration

Typical source restoration:

```text
stored descriptor
    ↓
Create/Restore CaptureSource
    ↓
UNINITIALIZED
    ↓
INITIALIZING
    ↓
READY
```

Native handles are never restored directly from persisted state.

---

# 85. Testing — CaptureSource

Tests MUST cover:

```text
UNINITIALIZED → INITIALIZING
INITIALIZING → READY
INITIALIZING → UNAVAILABLE
READY ↔ SUSPENDED
READY → UNAVAILABLE
UNAVAILABLE → INITIALIZING
READY → INITIALIZING → READY on replacement
* → STOPPING → STOPPED
invalid STOPPED transitions
```

---

# 86. Testing — CaptureOperation

Tests MUST cover:

```text
VALIDATING → ACQUIRING
ACQUIRING → NORMALIZING
NORMALIZING → VALIDATING_CANDIDATE
VALIDATING_CANDIDATE → COMPLETING
COMPLETING → FINISHED
validation rejection
provider failure
normalization failure
local cancellation observation
SourceVersion conflict
```

---

# 87. Testing — Runtime Ownership

Tests MUST verify Capture does not maintain:

```text
Queued
Runtime Cancelled
Runtime TimedOut
Retrying
Scheduler state
```

as Capture-owned lifecycle.

---

# 88. Testing — Runtime Supersession

Test:

```text
Capture operation running
Runtime authority superseded
provider returns late
Runtime rejects completion
```

Capture state remains internally consistent and Candidate resources are released.

---

# 89. Testing — Source Replacement

Test race:

```text
CaptureSource version 10
operation acquiring
source replaced → version 11
old provider result arrives
```

Ensure:

* semantic source compatibility checked;
* incompatible Candidate rejected;
* no stale SourceVersion Candidate published.

---

# 90. Testing — Continuous Provider Stream

If supported:

* stream opens;
* samples are bounded;
* obsolete samples are discarded;
* stream closes exactly once;
* Runtime cancellation does not require Capture-owned continuous-session scheduler state;
* provider stream does not imply Artifact publication.

---

# 91. Testing — Permission

Tests MUST verify:

* permission revoked causes correct source state;
* no full-display fallback;
* restoration path works;
* no prompt loop;
* current Reading Session lifecycle is not modified directly.

---

# 92. Testing — Health

Test:

```text
HEALTHY → DEGRADED
DEGRADED → HEALTHY
DEGRADED → UNAVAILABLE
UNAVAILABLE → RECOVERING
RECOVERING → HEALTHY
* → STOPPED
```

---

# 93. Testing — Resources

Verify:

* raw buffer released once;
* rejected Candidate released;
* source stop releases provider reference;
* accepted Artifact not disposed by Capture;
* temporary source buffers bounded;
* late provider callback after STOPPED is safely ignored/released.

---

# 94. Removed v1 State Concepts

The following concepts are removed from Capture-owned state:

```text
CaptureOperation.Queued
CaptureOperation.Cancelled
CaptureOperation.TimedOut

ContinuousCaptureSession
├── Created
├── Starting
├── Running
├── Paused
├── Stopping
└── Stopped

Reading Generation change
    → Capture operation cancellation authority
```

Replacement:

```text
Runtime
    → Queue / Attempt / Cancel / Deadline / repeated scheduling

Capture
    → local Capture processing phases

SourceVersion
    → CaptureSource semantic compatibility
```

---

# 95. Architecture Invariants

1. CaptureSource lifecycle is Capture-owned.

2. CaptureOperation phases are Capture-owned.

3. Capture Health is Capture-owned.

4. Runtime WorkItem lifecycle is not Capture-owned.

5. Runtime Attempt lifecycle is not Capture-owned.

6. Scheduler queue state is not Capture-owned.

7. Runtime cancellation authority is not Capture-owned.

8. Runtime timeout/deadline state is not Capture-owned.

9. Runtime retry lifecycle is not Capture-owned.

10. Continuous sampling scheduling is not Capture-owned.

11. Provider-native stream mechanics may remain internal.

12. SourceVersion is Capture-owned.

13. SourceVersion is not Runtime authority.

14. ReadingContextRevision is not Capture authority.

15. CandidateCaptureResult is not an accepted Artifact.

16. CaptureOperation FINISHED does not imply Artifact publication.

17. Accepted Artifact lifecycle is external.

18. Candidate validation occurs before transfer to Runtime.

19. Runtime authority validation remains final acceptance protection.

20. A CaptureSource must be READY before new acquisition.

21. SUSPENDED sources reject new acquisition.

22. STOPPED sources never return to READY.

23. Source mutation is logically serialized.

24. Temporary resources are released deterministically.

25. Accepted Artifact resources are not disposed by Capture.

26. Permission loss does not mutate Reading Session directly.

27. Health changes do not mutate Runtime state directly.

28. Runtime shutdown and Capture shutdown remain separate ownership domains.

29. Operation success/failure events do not create a competing Runtime lifecycle.

30. State diagnostics do not expose native handles or raw frame content.

---

# 96. Related Documents

```text
doc/02-modules/capture/MODULE.md
doc/02-modules/capture/CONTRACT.md
doc/02-modules/capture/EVENTS.md
doc/02-modules/capture/ERRORS.md
doc/02-modules/capture/README.md

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/MEMORY_MODEL.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md
```

---

# 97. Completion Criteria

This specification is synchronized when:

* only Capture-owned state remains;
* CaptureSource no longer uses `Active` to represent Runtime execution;
* CaptureOperation no longer owns `Queued`;
* CaptureOperation no longer owns Runtime `Cancelled`;
* CaptureOperation no longer owns Runtime deadline `TimedOut`;
* continuous scheduling state machine is removed;
* SourceVersion replaces Generation-based source compatibility;
* Reading Generation no longer directly cancels Capture;
* Runtime authority remains external;
* Candidate Capture state remains distinct from accepted Artifact;
* source replacement semantics are deterministic;
* permission recovery is explicit;
* health is Capture-specific;
* temporary resource cleanup is deterministic;
* tests cover Runtime ownership boundaries.

---

# 98. Summary

Capture v2 state is:

```text
CaptureSourceState
+
CaptureOperationPhase
+
CaptureHealthState
```

CaptureSource:

```text
UNINITIALIZED
      ↓
INITIALIZING
      ↓
READY
  ↕
SUSPENDED

READY / SUSPENDED
      ↓
UNAVAILABLE
      ↓
INITIALIZING

*
↓
STOPPING
↓
STOPPED
```

CaptureOperation:

```text
VALIDATING
    ↓
ACQUIRING
    ↓
NORMALIZING
    ↓
VALIDATING_CANDIDATE
    ↓
COMPLETING
    ↓
FINISHED
```

with Capture-local outcomes:

```text
REJECTED
FAILED
ABORTED_LOCAL
```

Runtime remains responsible for:

```text
Queued
Running Attempt
Cancelled Attempt
Timed-out Attempt
Retrying
Superseded
```

The central invariant is:

```text
Capture state describes
source availability
and Capture-local processing.

Runtime state describes
execution authority and lifecycle.

Those state machines must not be merged.
```
