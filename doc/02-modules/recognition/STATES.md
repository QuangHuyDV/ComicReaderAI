# Recognition Module States

> Project: CRAI  
> Module: Recognition  
> Path: `doc/02-modules/recognition/STATES.md`  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa state model mà Recognition Module thực sự sở hữu.

Recognition state model bao phủ:

- Recognition capability availability;
- Recognition Plan lifecycle;
- Recognition operation phases;
- Candidate Recognition Artifact validation;
- Recognition quality;
- Recognition completeness;
- provider-execution observations;
- cancellation checkpoint behavior;
- deadline behavior;
- resource-state interaction;
- external Runtime disposition observation;
- invalid transitions;
- concurrency constraints;
- diagnostics;
- recovery;
- MVP state model;
- state invariants.

Tài liệu này không định nghĩa canonical lifecycle của:

- WorkItem;
- Attempt;
- Provider Manager;
- Scheduler;
- Work Queue;
- Retry Policy;
- Runtime cancellation authority;
- Artifact publication;
- Artifact retention;
- Resource disposal;
- Reading Session;
- Translation;
- Presentation.

Các lifecycle đó thuộc Runtime hoặc module owner tương ứng.

---

## 2. State Ownership

Recognition owns:

```text
RecognitionAvailabilityState
RecognitionPlanState
RecognitionOperationPhase
CandidateValidationState
RecognitionQualityState
RecognitionCompleteness
ProviderExecutionObservation
```

Recognition does not own:

```text
WorkItemState
AttemptState
QueueState
SchedulerState
ProviderLifecycleState
ProviderHealthState
CancellationState
RetryState
PublicationState
ArtifactLifecycleState
RetentionState
StorageState
SessionState
```

Recognition có thể đọc external state snapshot nhưng không trở thành source of truth.

---

## 3. State Model Overview

```text
Recognition Module
├── Recognition Availability
├── Attempt-Local Recognition Plan
├── Attempt-Local Operation Phase
├── Attempt-Local Candidate Validation
├── Recognition Quality
├── Recognition Completeness
└── External State Observations
    ├── Provider Availability Snapshot
    ├── Cancellation Context
    ├── Deadline Context
    ├── Resource Pressure
    └── Runtime Candidate Disposition
```

Không tồn tại:

```text
Recognition Request Registry
Recognition Attempt Registry
Recognition Result Registry
Recognition Cancellation Registry
Recognition Retry Registry
```

Runtime v2 đã sở hữu các lifecycle đó.

---

## 4. State Machine Principles

### 4.1 Explicit Transitions

Mọi Recognition-owned state transition phải explicit và testable.

### 4.2 Attempt-Local State

Plan, operation phase và Candidate validation là Attempt-local.

Chúng không sống lâu hơn Attempt execution context.

### 4.3 No Runtime Authority

Recognition state không quyết định:

- Attempt success;
- current Revision;
- cancellation terminal outcome;
- retry;
- publication;
- Artifact availability.

### 4.4 Candidate Is Not Published Artifact

```text
Candidate VALID
    ≠
Published Recognition Artifact
```

Sau submission, Runtime và Artifact Store xử lý ownership transfer/publication.

### 4.5 Operation Phase Is Diagnostic

Operation phase không phải Runtime state machine.

Nó chỉ hỗ trợ:

- metrics;
- cancellation checkpoint;
- error localization;
- resource cleanup;
- debugging.

### 4.6 Provider State Is External

Recognition không quản lý provider lifecycle.

Nó chỉ quan sát capability/availability snapshot từ Provider Manager.

---

## 5. Recognition Availability State

```text
RecognitionAvailabilityState
├── UNINITIALIZED
├── INITIALIZING
├── AVAILABLE
├── DEGRADED
├── UNAVAILABLE
├── DRAINING
└── STOPPED
```

Availability state mô tả Recognition capability boundary, không phải provider lifecycle.

---

## 6. `UNINITIALIZED`

Recognition capability chưa được đăng ký đầy đủ trong Runtime.

Characteristics:

- contract chưa active;
- profile registry chưa ready;
- quality/normalization policy chưa ready;
- module không nhận execution.

Allowed next states:

```text
INITIALIZING
STOPPED
```

---

## 7. `INITIALIZING`

Recognition đang chuẩn bị module-owned structures:

- contract validation;
- Recognition Profile registry;
- Recognition Plan builders;
- normalization rules;
- quality policy;
- provider capability requirements;
- module diagnostics.

Recognition không initialize:

- Scheduler;
- Work Queue;
- Provider Manager;
- cancellation registry;
- Artifact Store.

Allowed next states:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
DRAINING
```

---

## 8. `AVAILABLE`

Recognition có thể thực thi các operation được support.

Requirements:

- module contract valid;
- Recognition Plan builder ready;
- at least one provider-capability path externally available;
- Candidate validation available;
- required Runtime dependencies ready.

Allowed next states:

```text
DEGRADED
UNAVAILABLE
DRAINING
```

---

## 9. `DEGRADED`

Recognition vẫn usable nhưng capability hoặc quality bị hạn chế.

Examples:

- vertical text unavailable;
- only CPU execution available;
- line geometry unavailable;
- only one provider capability path usable;
- partial quality policy active;
- remote path disabled;
- resource pressure requires reduced profile.

Recognition may execute only requests whose requirements remain satisfiable.

Allowed next states:

```text
AVAILABLE
UNAVAILABLE
DRAINING
```

---

## 10. `UNAVAILABLE`

Recognition không thể đáp ứng useful execution.

Examples:

- no eligible Recognition provider capability;
- module contract invalid;
- required image primitive unavailable;
- Candidate validation unavailable;
- critical Runtime dependency unavailable.

Recognition does not reject WorkItem directly.

Runtime receives a capability-unavailable response/guard result.

Allowed next states:

```text
INITIALIZING
DEGRADED
AVAILABLE
DRAINING
```

---

## 11. `DRAINING`

Recognition không nhận new execution nhưng Attempt-local work đang cleanup hoặc finish.

Behavior:

- no new execution;
- existing execution observes cancellation/deadline;
- Attempt-local resources released;
- Candidate submission may be denied by Runtime authority;
- module-owned structures remain until drain ends.

Allowed next state:

```text
STOPPED
```

---

## 12. `STOPPED`

Recognition module-owned runtime structures đã released.

Characteristics:

- no new execution;
- no active module-owned Attempt-local state;
- no pending Candidate validation;
- no module-owned temporary resources.

Allowed next state:

```text
INITIALIZING
```

chỉ qua new startup sequence.

---

## 13. Availability Transition Diagram

```text
UNINITIALIZED
      ↓
INITIALIZING
  ┌────┼──────────┐
  ↓    ↓          ↓
AVAILABLE DEGRADED UNAVAILABLE
   ↕       ↕         ↕
   └───────┴─────────┘
            ↓
         DRAINING
            ↓
          STOPPED
```

---

## 14. Availability Guards

Transition to `AVAILABLE` requires:

```text
contract_valid
plan_builder_ready
candidate_validator_ready
runtime_dependencies_ready
at_least_one_capability_path_available
```

Transition to `DEGRADED` allowed when:

```text
core_execution_possible
but
one_or_more_declared_capabilities_unavailable
```

Transition to `UNAVAILABLE` when:

```text
no_satisfiable_recognition_capability_path
```

---

## 15. External Provider Availability Snapshot

Recognition may read:

```text
ProviderAvailabilitySnapshot
├── ProviderId
├── Availability
├── Capabilities
├── ExecutionClasses
├── CapacityAvailable
├── PrivacyClassification
├── DegradedCapabilities[]
└── SnapshotVersion
```

```text
ProviderAvailability
├── HEALTHY
├── DEGRADED
├── UNAVAILABLE
└── DRAINING
```

This is external state owned by Provider Manager.

Recognition must not mutate it.

---

## 16. Provider Eligibility Guard

A provider path is eligible only when:

```text
availability ∈ {HEALTHY, DEGRADED}
capabilities satisfy RecognitionCapabilityRequirements
privacy policy satisfied
image limits satisfied
execution class compatible
capacity available or Runtime may wait
provider not draining
```

Recognition may build capability requirements.

Provider selection authority remains external.

---

## 17. Recognition Plan State

```text
RecognitionPlanState
├── NOT_CREATED
├── BUILDING
├── VALIDATING
├── READY
└── INVALID
```

Plan state is Attempt-local.

---

## 18. `NOT_CREATED`

No Recognition Plan exists.

Allowed next state:

```text
BUILDING
```

---

## 19. `BUILDING`

Recognition derives plan from:

- RecognitionAttemptInput;
- Recognition Profile;
- Capability Requirements;
- privacy context;
- configuration snapshot;
- provider availability snapshot;
- resource constraints.

Allowed next states:

```text
VALIDATING
INVALID
```

---

## 20. `VALIDATING`

Plan is checked for:

- supported operation;
- valid profile;
- coherent image preparation;
- satisfiable capability requirements;
- valid coordinate strategy;
- valid quality policy;
- privacy compatibility;
- executable strategy;
- bounded resource estimate.

Allowed next states:

```text
READY
INVALID
```

---

## 21. `READY`

Plan is immutable and may be executed.

Properties:

- strategy fixed for this Attempt;
- policy versions fixed;
- capability requirements fixed;
- coordinate transform policy fixed;
- quality policy fixed.

No transition back to `BUILDING`.

---

## 22. `INVALID`

Plan cannot be executed.

Examples:

- impossible capability combination;
- unsupported profile;
- privacy conflict;
- invalid preparation chain;
- unsupported geometry requirement;
- no executable strategy.

`INVALID` is terminal for that plan instance.

Runtime still owns Attempt terminal outcome.

---

## 23. Plan Transition Diagram

```text
NOT_CREATED
    ↓
BUILDING
    ↓
VALIDATING
   ┌┴───────┐
   ↓        ↓
 READY    INVALID
```

---

## 24. Plan Invariants

1. One Attempt has at most one active Recognition Plan.
2. READY plan immutable.
3. INVALID plan never executes.
4. Plan does not contain credential values.
5. Plan does not own provider lifecycle.
6. Plan preserves configuration snapshot identity.
7. Plan cannot weaken privacy.
8. Plan cannot grant Runtime authority.
9. Plan can reference capability requirements, not provider SDK objects.
10. Plan version remains stable during Attempt.

---

## 25. Recognition Operation Phase

```text
RecognitionOperationPhase
├── NOT_STARTED
├── VALIDATING
├── PLANNING
├── ACQUIRING_INPUT
├── PREPARING
├── DETECTING
├── RECOGNIZING
├── NORMALIZING
├── MAPPING_COORDINATES
├── RESOLVING_READING_ORDER
├── ASSEMBLING_CANDIDATE
├── VALIDATING_CANDIDATE
├── FINALIZING
└── FINISHED
```

Phase describes current module operation.

It is not a terminal lifecycle.

---

## 26. Primary Operation Path

```text
NOT_STARTED
    ↓
VALIDATING
    ↓
PLANNING
    ↓
ACQUIRING_INPUT
    ↓
PREPARING
    ↓
DETECTING
    ↓
RECOGNIZING
    ↓
NORMALIZING
    ↓
MAPPING_COORDINATES
    ↓
RESOLVING_READING_ORDER
    ↓
ASSEMBLING_CANDIDATE
    ↓
VALIDATING_CANDIDATE
    ↓
FINALIZING
    ↓
FINISHED
```

Some phases may be skipped according to plan.

---

## 27. Combined Recognition Path

```text
VALIDATING
    ↓
PLANNING
    ↓
ACQUIRING_INPUT
    ↓
PREPARING
    ↓
RECOGNIZING
    ↓
NORMALIZING
    ↓
MAPPING_COORDINATES
    ↓
RESOLVING_READING_ORDER
    ↓
ASSEMBLING_CANDIDATE
    ↓
VALIDATING_CANDIDATE
    ↓
FINALIZING
```

`DETECTING` is skipped because provider performs combined recognition.

---

## 28. Single-Region Path

```text
VALIDATING
    ↓
PLANNING
    ↓
ACQUIRING_INPUT
    ↓
PREPARING
    ↓
RECOGNIZING
    ↓
NORMALIZING
    ↓
MAPPING_COORDINATES
    ↓
ASSEMBLING_CANDIDATE
    ↓
VALIDATING_CANDIDATE
    ↓
FINALIZING
```

Reading-order phase may be skipped when one region exists.

---

## 29. Empty-Valid Path

```text
DETECTING or RECOGNIZING
    ↓
NORMALIZING
    ↓
ASSEMBLING_CANDIDATE
    ↓
VALIDATING_CANDIDATE
    ↓
FINALIZING
```

Candidate:

```text
Completeness = EMPTY_VALID
Regions = []
ReadingOrder = []
Warning = NO_READABLE_TEXT_DETECTED
```

This is not module failure.

---

## 30. Phase Entry Rules

Before entering an expensive phase:

1. check CancellationContext;
2. check ExecutionContext deadline;
3. validate prerequisite output;
4. verify required Resource Lease;
5. verify plan remains READY;
6. record phase start;
7. verify privacy constraints;
8. avoid new work when module is DRAINING.

---

## 31. Phase Exit Rules

Before leaving a phase:

1. validate phase output;
2. record duration;
3. check cancellation/deadline;
4. release no-longer-needed Attempt-local resource;
5. preserve traceability;
6. determine next phase explicitly;
7. normalize warnings/errors;
8. update diagnostic phase only after output is safe.

---

## 32. Phase Skipping Rules

A phase may be skipped only when plan explicitly states it.

Examples:

```text
DETECTING skipped
    → combined provider or single-region input

PREPARING skipped
    → provider accepts source view directly

RESOLVING_READING_ORDER skipped
    → no regions or explicit valid order already present
```

Silent phase skipping is forbidden.

---

## 33. Phase Failure

Any phase may produce:

```text
RecognitionModuleError
```

The phase then moves to:

```text
FINALIZING
    ↓
FINISHED
```

Recognition does not transition to Runtime `FAILED`.

Runtime receives module error through Attempt Completion.

---

## 34. Cancellation During Phase

When cancellation observed:

```text
Current Phase
    ↓
Stop Starting New Expensive Work
    ↓
Request Provider Cancellation if Supported
    ↓
Release Attempt-Local Resources
    ↓
FINALIZING
    ↓
FINISHED
```

Recognition reports cancellation observed.

Runtime decides Attempt outcome.

---

## 35. Candidate Validation State

```text
CandidateValidationState
├── NOT_CREATED
├── ASSEMBLING
├── VALIDATING
├── VALID
├── INVALID
└── SUBMITTED_TO_RUNTIME
```

This is the main Recognition-owned Candidate state machine.

---

## 36. `NOT_CREATED`

No Candidate exists.

Allowed next state:

```text
ASSEMBLING
```

---

## 37. `ASSEMBLING`

Recognition builds Candidate from normalized outputs.

Activities:

- assign CandidateArtifactId;
- attach InputArtifactRef;
- attach provider provenance;
- attach source coordinate space;
- attach regions/lines;
- attach ReadingOrder;
- attach warnings;
- attach quality/completeness;
- attach compatibility metadata;
- attach integrity metadata.

Allowed next states:

```text
VALIDATING
INVALID
```

---

## 38. `VALIDATING`

Candidate validation checks:

- identity;
- Artifact type;
- owner module;
- region/line ID uniqueness;
- line-region references;
- reading-order references;
- geometry bounds;
- confidence ranges;
- completeness consistency;
- provider provenance;
- transform chain;
- compatibility metadata;
- privacy-safe metadata;
- no provider SDK object;
- no Runtime Attempt status.

Allowed next states:

```text
VALID
INVALID
```

---

## 39. `VALID`

Candidate passed Recognition semantic validation.

Properties:

- immutable;
- source-space geometry valid;
- provider-independent;
- content-safe metadata;
- ready for Runtime submission.

Allowed next state:

```text
SUBMITTED_TO_RUNTIME
```

---

## 40. `INVALID`

Candidate cannot be submitted as valid Recognition output.

Examples:

- invalid geometry;
- dangling RegionId;
- duplicate LineId;
- impossible completeness;
- missing provider provenance;
- invalid compatibility metadata;
- provider SDK object leaked;
- privacy violation.

`INVALID` is terminal for that Candidate instance.

Recognition returns module error.

---

## 41. `SUBMITTED_TO_RUNTIME`

Candidate has crossed Recognition boundary.

After this transition:

- Recognition does not own publication;
- Recognition does not mutate Candidate;
- Runtime may accept or reject;
- Artifact Store may receive ownership;
- rejected Candidate follows cleanup path.

No transition back to `ASSEMBLING`.

---

## 42. Candidate Transition Diagram

```text
NOT_CREATED
    ↓
ASSEMBLING
    ↓
VALIDATING
   ┌┴────────┐
   ↓         ↓
 VALID     INVALID
   ↓
SUBMITTED_TO_RUNTIME
```

---

## 43. External Candidate Disposition

After submission, Recognition may observe:

```text
RuntimeCandidateDisposition
├── ACCEPTED
├── REJECTED_STALE
├── REJECTED_CANCELED
├── REJECTED_DUPLICATE
├── REJECTED_INVALID
└── REJECTED_RUNTIME_FAILURE
```

This is external Runtime state.

Recognition must not treat it as CandidateValidationState.

---

## 44. Candidate Ownership Boundary

```text
ASSEMBLING / VALIDATING / VALID
    → Recognition-side producer ownership

SUBMITTED_TO_RUNTIME
    → transfer pending

ACCEPTED
    → Artifact Store ownership

REJECTED_*
    → Candidate cleanup required
```

Recognition never owns published Artifact payload.

---

## 45. Recognition Quality State

```text
RecognitionQualityState
├── UNKNOWN
├── ACCEPTABLE
├── DEGRADED
└── UNUSABLE
```

### UNKNOWN

Insufficient quality information.

### ACCEPTABLE

Output meets configured quality policy.

### DEGRADED

Output usable with warnings.

### UNUSABLE

Output cannot satisfy Recognition semantic requirements.

---

## 46. Quality Transition Rules

Quality is derived, not freely mutated.

```text
Provider Output
    ↓
Normalization
    ↓
Quality Evaluation
    ↓
UNKNOWN / ACCEPTABLE / DEGRADED / UNUSABLE
```

Quality may be recalculated during Candidate validation.

A Candidate with `UNUSABLE` quality normally becomes `INVALID` unless explicit diagnostic operation allows otherwise.

---

## 47. Recognition Completeness

```text
RecognitionCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

Completeness is Artifact metadata, not execution status.

### COMPLETE

Expected Recognition output produced.

### PARTIAL

Some usable regions produced, some unavailable.

### EMPTY_VALID

No readable text detected successfully.

### UNKNOWN

Completeness cannot be determined.

---

## 48. Quality and Completeness Matrix

| Completeness | Possible Quality |
|---|---|
| COMPLETE | ACCEPTABLE, DEGRADED |
| PARTIAL | DEGRADED, UNUSABLE |
| EMPTY_VALID | ACCEPTABLE, DEGRADED |
| UNKNOWN | UNKNOWN, DEGRADED, UNUSABLE |

`EMPTY_VALID` is not automatically degraded.

---

## 49. Provider Execution Observation

```text
ProviderExecutionObservation
├── NOT_STARTED
├── STARTING
├── RUNNING
├── OUTPUT_RECEIVED
├── ERROR_RECEIVED
├── CANCELLATION_REQUESTED
└── PHYSICALLY_FINISHED
```

This is observational, not authoritative.

Provider Manager owns lifecycle/health.

Runtime owns Attempt outcome.

---

## 50. Provider Observation Flow

Typical:

```text
NOT_STARTED
    ↓
STARTING
    ↓
RUNNING
   ┌┴─────────────┐
   ↓              ↓
OUTPUT_RECEIVED ERROR_RECEIVED
   └──────┬───────┘
          ↓
PHYSICALLY_FINISHED
```

Cancellation:

```text
RUNNING
    ↓
CANCELLATION_REQUESTED
    ↓
PHYSICALLY_FINISHED
```

Provider may still produce late callback after cancellation request.

---

## 51. Duplicate Provider Callback

Provider callback must be deduplicated by adapter/request identity.

Rules:

1. first normalized output/error is retained for module processing;
2. duplicate callback does not restart phase;
3. duplicate callback does not create second Candidate;
4. duplicate callback does not change Runtime outcome;
5. duplicate callback resources are released safely;
6. diagnostics record duplication.

---

## 52. Cancellation Behavior

Recognition reads external CancellationContext.

Possible observations:

```text
NOT_REQUESTED
REQUESTED
ACKNOWLEDGED_BY_MODULE
PROVIDER_CANCEL_REQUESTED
PROVIDER_CANCEL_UNSUPPORTED
```

Recognition does not own canonical cancellation state.

---

## 53. Cancellation Checkpoints

Required checkpoints:

- before Plan execution;
- before Input Lease acquisition;
- before image preparation;
- after image preparation;
- before provider execution;
- between bounded region batches;
- after provider completion;
- before coordinate mapping;
- before Candidate assembly;
- before Candidate submission.

---

## 54. Non-Cancelable Provider

```text
Cancellation Requested
    ↓
Provider Cannot Stop
    ↓
Recognition Stops New Local Work
    ↓
Runtime May Mark Attempt Abandoned
    ↓
Provider Finishes Physically
    ↓
Late Output Rejected
    ↓
Resources Released
```

Recognition must not wait indefinitely for physical provider completion.

---

## 55. Deadline Behavior

Deadline belongs to Runtime ExecutionContext.

Recognition only observes:

```text
DeadlineAvailable
RemainingBudget
DeadlineExceeded
```

Before expensive phase:

```text
remaining_budget >= phase_minimum_budget
```

If not:

- do not start phase;
- return normalized module/provider timeout information;
- allow Runtime to decide failed/canceled outcome.

---

## 56. Timeout Sources

Possible external sources:

```text
ATTEMPT_DEADLINE
PROVIDER_TIMEOUT
RESOURCE_WAIT_TIMEOUT
SHUTDOWN_DEADLINE
```

Queue timeout is not Recognition-owned.

---

## 57. Resource Interaction

Recognition uses:

```text
Input Artifact Lease
Attempt-Local Buffers
Provider Request Resource
Candidate Artifact Resource
```

Recognition does not own:

```text
Published Artifact Retention
Cache Retention
Physical Artifact Disposal
Provider Model Lifetime
```

---

## 58. Resource-State Interaction

```text
Acquire Input Lease
    ↓
Use Immutable Input
    ↓
Create Attempt-Local Resources
    ↓
Execute Recognition
    ↓
Create Candidate
    ↓
Submit or Cleanup
    ↓
Release Attempt-Local Resources
    ↓
Release Input Lease
```

Resource Manager owns canonical resource lifecycle.

---

## 59. Resource Cleanup State

Recognition may track Attempt-local cleanup observation:

```text
CleanupObservation
├── NOT_REQUIRED
├── PENDING
├── RUNNING
├── COMPLETED
└── FAILED
```

This does not replace Resource Lifecycle state.

Cleanup failure is returned/recorded through Runtime Error Model.

---

## 60. Runtime Disposition Observation

Recognition may receive/observe:

```text
AttemptDisposition
├── ACCEPTED
├── FAILED
├── CANCELED
├── ABANDONED
├── REJECTED_STALE
└── REJECTED_DUPLICATE
```

Recognition cannot mutate this disposition.

It may use it only for:

- cleanup;
- diagnostics;
- provider late-output handling.

---

## 61. Invalid Transitions

Forbidden:

```text
Availability STOPPED → AVAILABLE without INITIALIZING
Availability DRAINING → AVAILABLE
Plan READY → BUILDING
Plan INVALID → READY
Candidate VALID → ASSEMBLING
Candidate INVALID → VALID
Candidate SUBMITTED_TO_RUNTIME → ASSEMBLING
Operation FINISHED → RECOGNIZING
Quality UNUSABLE → ACCEPTABLE without reevaluation
Provider OUTPUT_RECEIVED → RUNNING
```

---

## 62. Invalid Transition Handling

When attempted:

1. reject transition;
2. preserve current state;
3. record contract violation;
4. avoid Candidate publication implication;
5. release duplicate resources if needed;
6. surface diagnostics;
7. do not crash whole application unless corruption is unrecoverable.

---

## 63. Concurrency Rules

1. One Attempt owns one Recognition execution context.
2. Plan transitions serialized logically.
3. Operation phase transitions serialized logically.
4. Candidate validation transitions serialized logically.
5. Provider callbacks normalized before phase mutation.
6. Duplicate callback cannot create duplicate Candidate.
7. Candidate submission occurs at most once.
8. Attempt-local resource release is idempotent.
9. Input Lease released exactly once.
10. Recognition never races Runtime authority mutation.
11. Recognition never owns terminal CAS.
12. Region-level concurrency bounded.
13. Provider concurrency controlled externally.
14. Shutdown prevents new execution.
15. External disposition may arrive after local FINISHED.

---

## 64. Diagnostic Transition Record

```text
RecognitionStateTransition
├── RevisionId
├── WorkItemId
├── AttemptId
├── StateCategory
├── PreviousState
├── NextState
├── Trigger
├── OccurredAt
├── ProviderId?
├── OperationPhase?
├── TraceId
└── Metadata?
```

No raw image or full recognized text.

---

## 65. Transition Triggers

Recognition-owned triggers:

```text
MODULE_INITIALIZE_REQUESTED
MODULE_INITIALIZED
MODULE_DEGRADED
MODULE_UNAVAILABLE
MODULE_DRAIN_REQUESTED
MODULE_STOPPED

PLAN_BUILD_STARTED
PLAN_BUILT
PLAN_VALID
PLAN_INVALID

PHASE_ENTERED
PHASE_COMPLETED
PHASE_SKIPPED
PHASE_ERROR
CANCELLATION_OBSERVED
DEADLINE_OBSERVED

CANDIDATE_ASSEMBLY_STARTED
CANDIDATE_ASSEMBLED
CANDIDATE_VALID
CANDIDATE_INVALID
CANDIDATE_SUBMITTED
```

Runtime terminal triggers are not Recognition-owned.

---

## 66. State and Events

Recognition-specific diagnostic facts may correspond to:

```text
Plan READY
    → RECOGNITION_PLAN_CREATED

PREPARING finished
    → RECOGNITION_PREPARATION_COMPLETED

DETECTING finished
    → RECOGNITION_REGIONS_DETECTED

NORMALIZING finished
    → RECOGNITION_PROVIDER_OUTPUT_NORMALIZED

RESOLVING_READING_ORDER finished
    → RECOGNITION_READING_ORDER_RESOLVED

Candidate VALID
    → RECOGNITION_CANDIDATE_VALIDATED

Candidate SUBMITTED_TO_RUNTIME
    → RECOGNITION_CANDIDATE_CREATED
```

These facts:

- do not grant authority;
- do not define Attempt outcome;
- do not trigger downstream work directly;
- are optional for correctness.

---

## 67. Recovery

Recognition-owned active state is ephemeral.

After process crash:

```text
Plan state
Operation phase
Candidate assembly state
Provider execution observation
Attempt-local resources
```

are not restored directly.

Runtime may:

- mark Attempt interrupted;
- create new Attempt;
- reacquire input Artifact;
- rebuild Recognition Plan;
- re-execute Recognition.

---

## 68. Candidate Recovery

Candidate not yet accepted by Artifact Store:

- is not published;
- should not be resurrected automatically;
- may be cleaned by recovery process;
- must not generate publication event after restart without explicit idempotent transfer design.

Published Artifact recovery belongs to Artifact Store/Storage.

---

## 69. Provider Recovery

Provider model/client recovery belongs to Provider Manager.

Recognition only receives updated availability snapshot.

Recognition does not transition provider from unavailable to ready.

---

## 70. State Persistence

MVP persists none of the active Recognition-owned state.

Ephemeral:

```text
Recognition Plan
Operation Phase
Attempt-Local Buffers
Provider Request Observation
Candidate Assembly State
Cancellation Observation
Cleanup Observation
```

Potentially persistent objects:

```text
Published Recognition Artifact
Benchmark Results
Sanitized Diagnostics
Provider Configuration Versions
```

Their persistence owner is not Recognition state machine.

---

## 71. MVP State Model

Required:

```text
RecognitionAvailabilityState
├── UNINITIALIZED
├── INITIALIZING
├── AVAILABLE
├── DEGRADED
├── UNAVAILABLE
├── DRAINING
└── STOPPED
```

```text
RecognitionPlanState
├── NOT_CREATED
├── BUILDING
├── VALIDATING
├── READY
└── INVALID
```

```text
RecognitionOperationPhase
├── VALIDATING
├── PLANNING
├── ACQUIRING_INPUT
├── PREPARING
├── DETECTING
├── RECOGNIZING
├── NORMALIZING
├── MAPPING_COORDINATES
├── RESOLVING_READING_ORDER
├── ASSEMBLING_CANDIDATE
├── VALIDATING_CANDIDATE
├── FINALIZING
└── FINISHED
```

```text
CandidateValidationState
├── NOT_CREATED
├── ASSEMBLING
├── VALIDATING
├── VALID
├── INVALID
└── SUBMITTED_TO_RUNTIME
```

---

## 72. MVP Simplification

Implementation may store operation phase as diagnostic enum only.

It must not create a second Runtime lifecycle.

Minimal control flow:

```text
Plan
    ↓
Execute Phases
    ↓
Validate Candidate
    ↓
Submit to Runtime
    ↓
Cleanup
```

---

## 73. When to Expand State Detail

Expand only when needed for:

- stage-specific cancellation;
- independent detector/recognizer;
- long-page chunking;
- provider streaming;
- partial Candidate;
- complex resource cleanup;
- distributed provider execution;
- advanced diagnostics;
- benchmark comparison;
- process isolation.

Expansion must remain Attempt-local.

---

## 74. Availability Invariants

1. AVAILABLE requires at least one satisfiable capability path.
2. DRAINING accepts no new execution.
3. STOPPED has no module-owned active state.
4. UNAVAILABLE does not create Recognition Plan for normal execution.
5. Availability does not depend on one Attempt result.
6. Provider state is not owned by Recognition.
7. Capability degradation is explicit.
8. Privacy policy cannot be weakened by degraded mode.

---

## 75. Plan Invariants

1. Every execution uses zero or one Plan.
2. READY Plan immutable.
3. INVALID Plan never executes.
4. Plan preserves config snapshot identity.
5. Plan contains no credentials.
6. Plan references capabilities, not provider SDK types.
7. Plan cannot alter Runtime priority.
8. Plan cannot alter Runtime deadline.
9. Plan cannot grant authority.
10. Plan cannot schedule retry.

---

## 76. Phase Invariants

1. One current phase per Recognition execution.
2. Phase transition order explicit.
3. Skipped phase documented by Plan.
4. Expensive phase checks cancellation/deadline.
5. Phase output validated before dependent phase.
6. Geometry-changing phase updates transform chain.
7. Semantic text correction forbidden.
8. FINISHED does not imply Runtime success.
9. Phase failure returns module error.
10. Phase state remains Attempt-local.

---

## 77. Candidate Invariants

1. One Candidate instance has one validation state.
2. Candidate ID unique.
3. Candidate immutable after VALID.
4. INVALID Candidate never submitted as valid.
5. SUBMITTED Candidate not mutated.
6. Candidate does not include Runtime terminal status.
7. Candidate geometry source-space valid.
8. Candidate ReadingOrder references existing regions.
9. Candidate contains no credential.
10. Candidate contains no provider SDK object.
11. Candidate rejection triggers cleanup.
12. Candidate submission does not imply publication.

---

## 78. Quality and Completeness Invariants

1. UNKNOWN confidence is not zero.
2. Warning is not failure.
3. EMPTY_VALID is valid success candidate.
4. PARTIAL explicit.
5. UNUSABLE normally invalidates Candidate.
6. Quality derived from configured policy.
7. Provider confidence normalized before use.
8. Quality does not claim translation correctness.
9. Completeness is Artifact metadata.
10. User corrections do not mutate quality state of original Artifact.

---

## 79. External-State Invariants

1. Runtime owns WorkItem state.
2. Runtime owns Attempt state.
3. Runtime owns cancellation authority.
4. Runtime Retry Policy owns retry.
5. Provider Manager owns provider lifecycle.
6. Artifact Store owns publication.
7. Resource Manager owns physical resource lifecycle.
8. Cache Policy owns retention.
9. Recognition never maintains parallel request registry.
10. Recognition never commits terminal outcome.

---

## 80. Testing Requirements

### Availability

- initialize to AVAILABLE;
- initialize to DEGRADED;
- initialize to UNAVAILABLE;
- drain rejects new execution;
- stop clears module-owned state;
- reinitialize from STOPPED.

### Plan

- valid Plan path;
- invalid capability combination;
- privacy conflict;
- immutable READY Plan;
- INVALID cannot execute;
- config snapshot preserved.

### Operation Phases

- combined path;
- composed path;
- single-region path;
- empty-valid path;
- skipped phase recorded;
- cancellation at every checkpoint;
- deadline before expensive phase;
- phase error;
- phase output validation;
- transform update.

### Candidate

- valid Candidate;
- invalid geometry;
- duplicate RegionId;
- invalid ReadingOrder;
- incomplete compatibility metadata;
- Candidate immutable after VALID;
- submit once;
- reject and cleanup.

### Provider Observation

- normal output;
- provider error;
- duplicate callback;
- cancellation supported;
- cancellation unsupported;
- late callback;
- physical completion after Runtime abandonment.

### Concurrency

- duplicate Candidate submission;
- provider callback vs cancellation;
- cleanup vs late callback;
- module drain vs Candidate assembly;
- external Runtime rejection after local VALID;
- input Lease released once.

---

## 81. Property Tests

```text
candidate_submission_count <= 1
```

```text
READY plan never returns to BUILDING
```

```text
VALID candidate never returns to ASSEMBLING
```

```text
public geometry always fits source coordinate space
```

```text
FINISHED does not imply published Artifact
```

```text
all acquired Attempt-local resources are released
```

```text
Recognition never changes Runtime Attempt state
```

```text
Recognition never mutates Provider Manager state
```

---

## 82. Open Decisions

- Is availability state a distinct runtime object or derived capability view?
- Should `DEGRADED` be one state or capability flags only?
- Should `VALIDATING_CANDIDATE` remain a phase and state simultaneously?
- Do partial Candidates need substate?
- Should provider execution observation be persisted in trace only?
- Should quality reevaluation happen during Text Processing?
- Should Plan expose selected provider ID or only capability path reference?
- How is long-page chunk state represented?
- Do streaming providers require `PARTIAL_OUTPUT_RECEIVED` observation?
- When does Candidate ownership transfer begin exactly?
- Which cleanup failures invalidate module availability?
- Is `DRAINING` controlled directly by Runtime Container?

---

## 83. Recommended MVP Decisions

```text
Availability state is explicit.
DEGRADED is retained.
Operation phases are diagnostic enums.
Candidate validation is explicit.
No active Recognition state persists.
Provider execution observations stay in trace/snapshot.
Provider attempts do not overlap in MVP.
Partial Candidate support is optional.
Runtime owns all terminal outcomes.
Candidate transfer begins on submit.
```

---

## 84. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/EVENTS.md
doc/02-modules/recognition/ERRORS.md

doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/THREADING_MODEL.md

doc/01-architecture/ocr/PIPELINE.md
doc/01-architecture/ocr/PREPROCESS.md
doc/01-architecture/ocr/DETECTION.md
doc/01-architecture/ocr/RECOGNITION.md
doc/01-architecture/ocr/READING_ORDER.md
doc/01-architecture/ocr/QUALITY.md
doc/01-architecture/ocr/PROVIDERS.md
```

---

## 85. Summary

Recognition state model now focuses only on state that Recognition actually owns:

```text
Recognition Availability
        ↓
Recognition Plan
        ↓
Recognition Operation Phases
        ↓
Candidate Validation
        ↓
Submitted to Runtime
```

Runtime owns:

```text
WorkItem
Attempt
Authority
Cancellation
Retry
Publication
Artifact Lifecycle
```

Provider Manager owns:

```text
Provider Lifecycle
Provider Health
Provider Capacity
```

The key boundary is:

```text
Recognition may produce a valid Candidate.

Only Runtime can decide whether that Candidate matters.

Only Artifact Store can publish and own the accepted Artifact.
```
