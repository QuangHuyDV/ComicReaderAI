# Recognition Module States

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `02-modules/recognition/STATES.md`
> **Version:** 1.1
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `01-architecture/ocr/`

---

# 1. Purpose

Tài liệu này định nghĩa state model mà Recognition Module thực sự sở hữu.

Recognition state model bao phủ:

* Recognition capability availability
* Recognition Plan lifecycle
* Attempt-local operation phases
* Candidate Recognition Artifact validation
* Recognition completeness
* Provider execution observations
* cancellation/deadline observations
* resource cleanup observations
* external Runtime disposition observations
* invalid transitions
* concurrency constraints
* recovery
* state invariants

Recognition state model không định nghĩa canonical lifecycle của:

* WorkItem
* Attempt
* Provider Manager
* Scheduler
* Work Queue
* Runtime Retry Policy
* Runtime cancellation authority
* Artifact publication
* Artifact retention
* Resource disposal
* OCR Quality semantics
* Reading Session
* Text Processing
* Translation
* Presentation

---

# 2. State Ownership

Recognition owns:

```text
RecognitionAvailabilityState
RecognitionPlanState
RecognitionOperationPhase
CandidateValidationState
RecognitionCompleteness
ProviderExecutionObservation
CleanupObservation
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
OCRQualityState
ReadingOrderState
SessionState
```

Recognition may observe external snapshots without becoming their source of truth.

---

# 3. State Model Overview

```text
Recognition Module
├── Recognition Availability
├── Attempt-Local Recognition Plan
├── Attempt-Local Operation Phase
├── Attempt-Local Candidate Validation
├── Recognition Completeness
└── External Observations
    ├── Provider Availability Snapshot
    ├── Provider Execution Observation
    ├── Cancellation Context
    ├── Deadline Context
    ├── Resource Cleanup Observation
    ├── Quality Report Reference
    └── Runtime Candidate Disposition
```

Không tồn tại:

```text
Recognition Request Registry
Recognition Attempt Registry
Recognition Result Registry
Recognition Cancellation Registry
Recognition Retry Registry
Recognition Quality Registry
```

---

# 4. State Principles

## 4.1 Explicit Transitions

Mọi Recognition-owned state transition phải explicit và testable.

---

## 4.2 Attempt-Local State

Các state sau là Attempt-local:

* Recognition Plan
* Operation Phase
* Candidate Validation
* Provider Execution Observation
* Cleanup Observation

Chúng không sống lâu hơn execution context.

---

## 4.3 No Runtime Authority

Recognition state không quyết định:

* Attempt success/failure
* current Revision
* cancellation terminal outcome
* retry
* publication
* Artifact authority
* downstream scheduling

---

## 4.4 Candidate Is Not Published Artifact

```text
Candidate VALID
    ≠
Published Recognition Artifact
```

`VALID` chỉ có nghĩa Candidate hợp lệ theo module contract.

---

## 4.5 Operation Phase Is Diagnostic

Operation Phase:

* hỗ trợ metrics
* hỗ trợ cancellation checkpoint
* hỗ trợ diagnostics
* hỗ trợ cleanup
* hỗ trợ error localization

Nó không phải Runtime lifecycle.

---

## 4.6 OCR Stage State Is External

Recognition orchestrates OCR Architecture.

Nó không sở hữu state machines riêng của:

* Preprocessing
* Detection
* Recognition
* Text Direction
* Layout
* Postprocessing
* Quality
* Reading Order

---

# 5. Recognition Availability State

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

Availability mô tả khả năng của **Recognition Module boundary** nhận execution.

Nó không phải Provider lifecycle.

---

# 6. UNINITIALIZED

Recognition module chưa active.

Characteristics:

* contract chưa active
* Recognition Profile registry chưa ready
* Plan builder chưa ready
* Candidate validator chưa ready

Allowed:

```text
UNINITIALIZED
    → INITIALIZING
    → STOPPED
```

---

# 7. INITIALIZING

Recognition đang chuẩn bị module-owned structures:

* contract validation
* Recognition Profile registry
* Recognition Plan builder
* capability requirement builder
* Candidate validator
* compatibility policy
* diagnostics schema

Recognition không initialize:

* Scheduler
* Work Queue
* Provider Manager
* Resource Manager
* Artifact Store
* cancellation registry

Allowed next:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
DRAINING
```

---

# 8. AVAILABLE

Recognition có thể nhận operation hợp lệ.

Requirements:

```text
contract_valid
plan_builder_ready
candidate_validator_ready
runtime_dependencies_ready
at_least_one_satisfiable_capability_path
```

Allowed next:

```text
DEGRADED
UNAVAILABLE
DRAINING
```

---

# 9. DEGRADED

Recognition vẫn usable nhưng một số capability path không khả dụng.

Ví dụ:

* vertical-text capability unavailable
* GPU path unavailable
* remote path disabled
* only one Provider path available
* optional Quality/ReadingOrder output unavailable
* resource pressure giới hạn profile

Module chỉ nhận request có requirements vẫn satisfiable.

Allowed next:

```text
AVAILABLE
UNAVAILABLE
DRAINING
```

---

# 10. UNAVAILABLE

Không tồn tại usable Recognition capability path.

Ví dụ:

* no eligible OCR Provider
* module contract invalid
* Candidate validator unavailable
* required Runtime dependency unavailable

Recognition không tự reject WorkItem.

Nó trả module guard/error để Runtime xử lý.

Allowed:

```text
INITIALIZING
DEGRADED
AVAILABLE
DRAINING
```

---

# 11. DRAINING

Recognition:

* không nhận execution mới
* cho phép work hiện tại cleanup/finish
* không tạo new expensive work
* tiếp tục cooperative cancellation
* release Attempt-local resources

Allowed next:

```text
STOPPED
```

---

# 12. STOPPED

Recognition module-owned structures đã release.

Không còn:

* active Recognition Plan
* active Candidate validation
* active module-owned resources

Restart:

```text
STOPPED
    → INITIALIZING
```

---

# 13. Availability Transition

```text
UNINITIALIZED
      ↓
INITIALIZING
   ┌──┼────────────┐
   ↓  ↓            ↓
AVAILABLE ←→ DEGRADED ←→ UNAVAILABLE
   \        |          /
            ↓
         DRAINING
            ↓
          STOPPED
```

---

# 14. Provider Availability Snapshot

Recognition có thể consume external snapshot:

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

Provider Manager là owner.

Recognition chỉ dùng snapshot để build Plan/capability requirements.

---

# 15. Provider Eligibility Guard

Một capability path usable khi:

```text
provider available
AND
required capabilities satisfied
AND
privacy constraints satisfied
AND
input limits satisfied
AND
execution class compatible
```

Provider selection authority không thuộc Recognition.

---

# 16. Recognition Plan State

```text
RecognitionPlanState
├── NOT_CREATED
├── BUILDING
├── VALIDATING
├── READY
└── INVALID
```

Plan State là Attempt-local.

---

# 17. NOT_CREATED

Chưa có Recognition Plan.

```text
NOT_CREATED
    → BUILDING
```

---

# 18. BUILDING

Plan được xây từ:

* RecognitionAttemptInput
* Recognition Profile
* OCR Profile reference
* Capability Requirements
* Privacy Context
* Configuration Snapshot
* Provider Availability Snapshot
* Runtime resource constraints

Allowed:

```text
BUILDING
    → VALIDATING
    → INVALID
```

---

# 19. VALIDATING

Plan validation kiểm tra:

* operation supported
* Recognition Profile valid
* OCR Profile resolvable
* capability requirements coherent
* privacy compatible
* execution path satisfiable
* required output references supported
* resource estimate bounded

Allowed:

```text
READY
INVALID
```

---

# 20. READY

Plan immutable và executable.

Fixed:

* operation
* profile
* OCR Profile reference
* capability requirements
* compatibility policy
* configuration versions
* privacy constraints

Không quay lại `BUILDING`.

---

# 21. INVALID

Plan không executable.

Ví dụ:

* unsupported profile
* impossible capability requirements
* privacy conflict
* unavailable OCR capability path
* incompatible configuration

Terminal cho Plan instance đó.

Runtime vẫn sở hữu Attempt outcome.

---

# 22. Plan Transition

```text
NOT_CREATED
    ↓
BUILDING
    ↓
VALIDATING
   ┌┴──────┐
   ↓       ↓
 READY   INVALID
```

---

# 23. Plan Invariants

1. Một Attempt có tối đa một active Recognition Plan.
2. READY Plan immutable.
3. INVALID Plan không execute.
4. Plan không chứa credential.
5. Plan không own Provider lifecycle.
6. Plan giữ Configuration Snapshot identity.
7. Plan không weaken Privacy policy.
8. Plan không grant Runtime authority.
9. Plan reference capability contract, không Provider SDK object.
10. Plan không schedule retry.

---

# 24. Recognition Operation Phase

```text
RecognitionOperationPhase
├── NOT_STARTED
├── VALIDATING
├── PLANNING
├── ACQUIRING_INPUT
├── EXECUTING_OCR
├── ASSEMBLING_CANDIDATE
├── VALIDATING_CANDIDATE
├── FINALIZING
└── FINISHED
```

Đây là thay đổi quan trọng so với bản cũ.

Recognition Module không còn encode:

```text
PREPARING
DETECTING
RECOGNIZING
NORMALIZING
MAPPING_COORDINATES
RESOLVING_READING_ORDER
```

thành module state.

Những bước đó thuộc OCR Architecture.

---

# 25. Primary Operation Path

```text
NOT_STARTED
    ↓
VALIDATING
    ↓
PLANNING
    ↓
ACQUIRING_INPUT
    ↓
EXECUTING_OCR
    ↓
ASSEMBLING_CANDIDATE
    ↓
VALIDATING_CANDIDATE
    ↓
FINALIZING
    ↓
FINISHED
```

---

# 26. EXECUTING_OCR

`EXECUTING_OCR` đại diện cho việc Recognition Module đang điều phối canonical OCR flow.

Bên trong có thể xảy ra:

```text
Preprocessing
Detection
Recognition
Text Direction
Layout
Postprocessing
Quality
Reading Order
```

nhưng những stage này không trở thành Recognition Module states.

Detailed stage progress có thể xuất hiện trong diagnostics/trace.

---

# 27. Phase Entry Rules

Trước expensive phase:

1. check CancellationContext
2. check Deadline
3. validate prerequisites
4. ensure Resource Lease
5. ensure Plan = READY
6. enforce Privacy Context
7. avoid new work when DRAINING

---

# 28. Phase Exit Rules

Trước khi rời phase:

1. validate output
2. record duration
3. observe cancellation/deadline
4. release unnecessary Attempt-local resources
5. preserve traceability
6. normalize warnings/errors
7. determine next phase explicitly

---

# 29. Phase Skipping

Một phase chỉ skip khi Plan cho phép.

Ví dụ:

```text
ACQUIRING_INPUT may be trivial
    → already leased execution context

EXECUTING_OCR may short-circuit
    → compatible result reused by Runtime policy
```

Silent skipping không được phép.

---

# 30. Phase Failure

Bất kỳ phase nào có thể tạo:

```text
RecognitionModuleError
```

Local flow:

```text
Current Phase
    ↓
FINALIZING
    ↓
FINISHED
```

`FINISHED` không đồng nghĩa Runtime `SUCCEEDED`.

---

# 31. Cancellation During Phase

```text
Current Phase
    ↓
Cancellation Observed
    ↓
Stop New Expensive Local Work
    ↓
Request Provider Cancellation if supported
    ↓
Cleanup
    ↓
FINALIZING
    ↓
FINISHED
```

Runtime quyết định Attempt outcome.

---

# 32. Candidate Validation State

```text
CandidateValidationState
├── NOT_CREATED
├── ASSEMBLING
├── VALIDATING
├── VALID
├── INVALID
└── SUBMITTED_TO_RUNTIME
```

Đây là Recognition-owned state machine chính của Candidate.

---

# 33. NOT_CREATED

```text
NOT_CREATED
    → ASSEMBLING
```

---

# 34. ASSEMBLING

Recognition tạo Candidate từ module outputs/references.

Candidate assembly bao gồm:

* CandidateArtifactId
* InputArtifactRef
* ContentIdentity
* OCRDocumentRef
* optional ReadingOrderResultRef
* optional QualityReportRef
* ProviderProvenance
* warnings
* completeness
* compatibility metadata
* traceability metadata
* integrity metadata

Allowed:

```text
VALIDATING
INVALID
```

---

# 35. VALIDATING

Validation chỉ kiểm tra module-level contract:

* Candidate identity
* ArtifactType
* OwnerModule
* InputArtifactRef
* ContentIdentity
* OCRDocumentRef valid
* optional output refs compatible
* ProviderProvenance
* Completeness
* CompatibilityMetadata
* privacy-safe metadata
* no SDK object
* no Runtime state
* no credentials

Recognition không validate lại:

* Region geometry
* Line hierarchy
* Reading Graph
* Quality Score internals

Các semantics đó thuộc artifact owner tương ứng.

Allowed:

```text
VALID
INVALID
```

---

# 36. VALID

Candidate:

* module-contract valid
* immutable
* provider-independent
* traceable
* ready for Runtime submission

```text
VALID
    → SUBMITTED_TO_RUNTIME
```

---

# 37. INVALID

Candidate vi phạm module contract.

Ví dụ:

* missing OCRDocumentRef
* invalid Artifact identity
* incompatible optional artifact refs
* invalid completeness
* missing ProviderProvenance
* invalid compatibility metadata
* SDK object leak
* privacy violation

Terminal cho Candidate instance.

---

# 38. SUBMITTED_TO_RUNTIME

Candidate đã vượt Recognition boundary.

Sau đó:

* Recognition không mutate
* Runtime validate authority
* Runtime accept/reject
* Artifact Store có thể receive ownership
* rejected Candidate cleanup

Không quay lại `ASSEMBLING`.

---

# 39. Candidate Transition

```text
NOT_CREATED
    ↓
ASSEMBLING
    ↓
VALIDATING
   ┌┴─────────┐
   ↓          ↓
 VALID      INVALID
   ↓
SUBMITTED_TO_RUNTIME
```

---

# 40. External Candidate Disposition

Runtime có thể trả:

```text
RuntimeCandidateDisposition
├── ACCEPTED
├── REJECTED_STALE
├── REJECTED_CANCELED
├── REJECTED_DUPLICATE
├── REJECTED_INVALID
└── REJECTED_RUNTIME_FAILURE
```

Đây là external Runtime state.

Không phải `CandidateValidationState`.

---

# 41. Candidate Ownership Boundary

```text
ASSEMBLING / VALIDATING / VALID
    → Recognition producer ownership

SUBMITTED_TO_RUNTIME
    → transfer pending

ACCEPTED
    → Artifact Store ownership

REJECTED_*
    → cleanup
```

---

# 42. Recognition Completeness

```text
RecognitionCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

Completeness là Artifact metadata.

Không phải execution state.

---

# 43. COMPLETE

Expected module-level OCR output available.

---

# 44. PARTIAL

Một phần output usable.

Phải explicit.

---

# 45. EMPTY_VALID

OCR execution hợp lệ nhưng không phát hiện readable source text.

Không phải failure.

---

# 46. UNKNOWN

Không đủ information để xác định completeness.

---

# 47. Quality Observation

Recognition Module **không còn sở hữu `RecognitionQualityState`**.

Quality semantics thuộc:

```text
01-architecture/ocr/QUALITY.md
```

Recognition chỉ có thể observe/reference:

```text
QualityReportRef
QualityGrade?
QualitySummary?
```

để:

* quyết định Candidate requirements theo Plan
* tạo warning
* cung cấp diagnostics

Recognition không mutate Quality state.

---

# 48. Quality vs Candidate Validation

Candidate validity và OCR quality khác nhau.

```text
Candidate VALID
    = module contract valid

Quality Poor
    = OCR result may be low quality
```

Một Candidate có thể:

```text
VALID
+
QualityReport Grade = Poor
```

Runtime/Policy mới quyết định có publish/use hay retry.

---

# 49. Provider Execution Observation

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

Observation này không authoritative.

Provider Manager owns Provider lifecycle/health.

Runtime owns Attempt outcome.

---

# 50. Provider Observation Flow

```text
NOT_STARTED
    ↓
STARTING
    ↓
RUNNING
   ┌┴──────────────┐
   ↓               ↓
OUTPUT_RECEIVED  ERROR_RECEIVED
   └───────┬───────┘
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

Late physical completion không cấp authority.

---

# 51. Duplicate Provider Callback

Rules:

1. normalize/deduplicate theo Provider request identity
2. duplicate callback không restart phase
3. duplicate không tạo second Candidate
4. duplicate không đổi Runtime outcome
5. duplicate resources cleanup idempotently
6. diagnostics record duplication

---

# 52. Cancellation Observation

Recognition có thể observe:

```text
NOT_REQUESTED
REQUESTED
ACKNOWLEDGED_BY_MODULE
PROVIDER_CANCEL_REQUESTED
PROVIDER_CANCEL_UNSUPPORTED
```

Đây không phải canonical cancellation state.

---

# 53. Cancellation Checkpoints

Required checkpoints:

* before Plan execution
* before Input Lease acquisition
* before EXECUTING_OCR
* between bounded OCR work when supported
* after OCR completion
* before Candidate assembly
* before Candidate submission

---

# 54. Non-Cancelable Provider

```text
Cancellation Requested
    ↓
Provider Cannot Stop
    ↓
Recognition Stops New Local Work
    ↓
Runtime May Revoke Authority
    ↓
Provider Physically Finishes
    ↓
Late Output Ignored / Rejected
    ↓
Cleanup
```

Recognition không chờ vô hạn.

---

# 55. Deadline Observation

Deadline thuộc Runtime Execution Context.

Recognition chỉ observe:

```text
DeadlineAvailable
RemainingBudget
DeadlineExceeded
```

Trước expensive work:

```text
remaining_budget >= required_budget
```

nếu không, Recognition trả normalized module result/error.

Runtime quyết định terminal outcome.

---

# 56. Resource Interaction

Recognition sử dụng:

```text
Input Artifact Lease
Attempt-Local Buffers
Provider Request Resource
Candidate Resource
```

Recognition không own:

```text
Published Artifact Retention
Cache Retention
Physical Artifact Disposal
Provider Model Lifetime
```

---

# 57. Resource Flow

```text
Acquire Input Lease
    ↓
Use Immutable Input
    ↓
Create Attempt-Local Resources
    ↓
Execute OCR
    ↓
Create Candidate
    ↓
Submit or Cleanup
    ↓
Release Attempt-Local Resources
    ↓
Release Input Lease
```

Canonical resource lifecycle thuộc Resource Manager/Runtime.

---

# 58. Cleanup Observation

```text
CleanupObservation
├── NOT_REQUIRED
├── PENDING
├── RUNNING
├── COMPLETED
└── FAILED
```

Observation này chỉ phục vụ local execution/diagnostics.

Không thay Resource Lifecycle state.

---

# 59. Runtime Disposition Observation

Recognition có thể observe:

```text
AttemptDisposition
├── ACCEPTED
├── FAILED
├── CANCELED
├── ABANDONED
├── REJECTED_STALE
└── REJECTED_DUPLICATE
```

Use only for:

* cleanup
* diagnostics
* late output handling

Recognition không mutate disposition.

---

# 60. Invalid Transitions

Forbidden examples:

```text
STOPPED → AVAILABLE without INITIALIZING

DRAINING → AVAILABLE

Plan READY → BUILDING

Plan INVALID → READY

Candidate VALID → ASSEMBLING

Candidate INVALID → VALID

Candidate SUBMITTED_TO_RUNTIME → ASSEMBLING

Operation FINISHED → EXECUTING_OCR

Provider OUTPUT_RECEIVED → RUNNING
```

---

# 61. Invalid Transition Handling

Khi transition invalid:

1. reject transition
2. preserve current state
3. record contract violation
4. avoid authority implication
5. cleanup duplicate resource if necessary
6. emit safe diagnostics
7. avoid application-wide crash unless corruption unrecoverable

---

# 62. Concurrency Rules

1. One Attempt owns one Recognition execution context.
2. Plan transitions serialized logically.
3. Operation phase transitions serialized logically.
4. Candidate transitions serialized logically.
5. Provider callbacks normalized before state mutation.
6. Duplicate callbacks cannot create duplicate Candidate.
7. Candidate submitted at most once.
8. Attempt-local cleanup idempotent.
9. Input Lease released once.
10. Recognition never races Runtime authority ownership.
11. Region-level OCR concurrency is handled within OCR/Runtime execution policy.
12. Provider concurrency controlled externally.
13. DRAINING blocks new execution.
14. Runtime disposition may arrive after local FINISHED.

---

# 63. Diagnostic Transition Record

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

Không chứa raw image/full OCR content.

---

# 64. Recognition-Owned Transition Triggers

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

OCR-stage event semantics không được redefine ở đây.

---

# 65. States and Events

Recognition diagnostic facts có thể correspond:

```text
Plan READY
    → RECOGNITION_PLAN_CREATED

EXECUTING_OCR completed
    → RECOGNITION_OCR_COMPLETED

Candidate VALID
    → RECOGNITION_CANDIDATE_VALIDATED

Candidate SUBMITTED_TO_RUNTIME
    → RECOGNITION_CANDIDATE_CREATED
```

Facts:

* do not grant authority
* do not define Attempt outcome
* do not create downstream work directly
* optional for correctness

---

# 66. Recovery

Recognition-owned active state là ephemeral.

Sau crash, không restore trực tiếp:

```text
Recognition Plan
Operation Phase
Candidate Assembly State
Provider Execution Observation
Attempt-Local Resources
Cleanup Observation
```

Runtime có thể:

* mark Attempt interrupted
* create new Attempt
* reacquire input Artifact
* rebuild Plan
* rerun Recognition

---

# 67. Candidate Recovery

Candidate chưa được accepted:

* không published
* không tự resurrect
* có thể cleanup
* không publish sau restart trừ khi explicit idempotent transfer design tồn tại

Published Artifact recovery thuộc Artifact Store/Storage.

---

# 68. Provider Recovery

Provider recovery thuộc Provider Manager.

Recognition chỉ nhận snapshot mới.

Recognition không transition Provider health state.

---

# 69. State Persistence

MVP không persist active Recognition state.

Ephemeral:

```text
Recognition Plan
Operation Phase
Provider Observation
Candidate Assembly State
Cancellation Observation
Cleanup Observation
```

Potentially persistent artifacts/diagnostics thuộc owner khác.

---

# 70. MVP State Model

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
├── EXECUTING_OCR
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

# 71. MVP Simplification

Minimal control flow:

```text
Build Plan
    ↓
Execute OCR
    ↓
Assemble Candidate
    ↓
Validate Candidate
    ↓
Submit to Runtime
    ↓
Cleanup
```

Không tạo lifecycle song song với Runtime.

---

# 72. When to Expand State Detail

Chỉ mở rộng khi thật sự cần:

* streaming OCR
* long-page chunks
* multi-provider composition
* partial Candidate
* complex cleanup
* process isolation
* distributed execution
* fine-grained diagnostics

Nếu cần stage detail, ưu tiên:

```text
OCR diagnostic stage record
```

thay vì thêm Recognition Module state machine.

---

# 73. Availability Invariants

1. AVAILABLE requires satisfiable capability path.
2. DRAINING accepts no new execution.
3. STOPPED has no active module-owned execution state.
4. UNAVAILABLE does not create normal execution Plan.
5. Availability is not determined by one Attempt result.
6. Provider health remains external.
7. Degradation is explicit.
8. Privacy constraints cannot be weakened.

---

# 74. Plan Invariants

1. At most one active Plan per Attempt.
2. READY Plan immutable.
3. INVALID Plan never executes.
4. Plan contains no credentials.
5. Plan preserves Configuration Snapshot identity.
6. Plan uses capability contracts, not SDK objects.
7. Plan does not alter Runtime priority.
8. Plan does not alter Runtime deadline.
9. Plan does not grant authority.
10. Plan does not schedule retry.

---

# 75. Phase Invariants

1. One current local phase.
2. Phase progression explicit.
3. Phase skip must be Plan-defined.
4. Expensive work checks cancellation/deadline.
5. Outputs validated before dependency usage.
6. FINISHED does not imply Runtime success.
7. Phase failure produces normalized module output/error.
8. Phase state remains Attempt-local.
9. OCR stage semantics remain outside Recognition state ownership.

---

# 76. Candidate Invariants

1. One Candidate instance has one validation state.
2. Candidate ID unique.
3. Candidate immutable after VALID.
4. INVALID Candidate never submitted as valid.
5. SUBMITTED Candidate never mutated.
6. Candidate has no Runtime terminal state.
7. Candidate references a valid OCRDocument.
8. Optional ReadingOrderResult/QualityReport references are compatible.
9. Candidate contains no credentials.
10. Candidate contains no Provider SDK object.
11. Rejected Candidate cleanup required.
12. Submission does not imply publication.

---

# 77. Completeness Invariants

1. EMPTY_VALID is valid module output.
2. PARTIAL is explicit.
3. UNKNOWN is not silently interpreted.
4. Completeness is Artifact metadata.
5. Completeness does not define Runtime terminal outcome.

---

# 78. External-State Invariants

1. Runtime owns WorkItem state.
2. Runtime owns Attempt state.
3. Runtime owns cancellation authority.
4. Runtime Retry Policy owns retry.
5. Provider Manager owns Provider lifecycle.
6. OCR Quality owns quality semantics.
7. Artifact Store owns publication.
8. Resource Manager owns physical resource lifecycle.
9. Cache Policy owns retention/reuse decision.
10. Recognition never maintains parallel Runtime registries.
11. Recognition never commits terminal Runtime outcome.

---

# 79. Testing Requirements

## Availability

* initialize AVAILABLE
* initialize DEGRADED
* initialize UNAVAILABLE
* drain rejects new execution
* stop clears module-owned state
* restart requires INITIALIZING

## Plan

* valid Plan
* invalid capability requirements
* privacy conflict
* READY immutable
* INVALID cannot execute
* configuration snapshot preserved

## Operation

* normal OCR path
* combined provider path
* selected-region path
* empty-valid path
* cancellation checkpoints
* deadline before OCR execution
* module error path
* FINISHED without Runtime success implication

## Candidate

* valid Candidate references OCRDocument
* invalid OCRDocumentRef
* incompatible ReadingOrderResultRef
* incompatible QualityReportRef
* missing compatibility metadata
* immutable after VALID
* submit once
* reject and cleanup

## Provider Observation

* normal output
* Provider error
* duplicate callback
* supported cancellation
* unsupported cancellation
* late callback
* physical completion after Runtime abandonment

## Concurrency

* duplicate Candidate submission
* callback vs cancellation
* cleanup vs late callback
* drain vs Candidate assembly
* Runtime rejection after Candidate VALID
* Input Lease released once

---

# 80. Property Tests

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
FINISHED does not imply published Artifact
```

```text
all Attempt-local resources are eventually released
```

```text
Recognition never changes Runtime Attempt state
```

```text
Recognition never mutates Provider Manager state
```

```text
Recognition never mutates Quality Report state
```

---

# 81. Recommended MVP Decisions

```text
Availability state remains explicit.

DEGRADED remains explicit.

Operation phases remain diagnostic.

OCR stages are not Recognition module states.

Candidate validation remains explicit.

RecognitionQualityState is removed.

QualityReport is externally referenced.

No active Recognition state persists.

Provider execution observation stays trace/local.

Runtime owns every terminal outcome.

Candidate transfer begins on submit.
```

---

# 82. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/EVENTS.md
doc/02-modules/recognition/ERRORS.md

doc/01-architecture/ocr/PIPELINE.md
doc/01-architecture/ocr/PREPROCESS.md
doc/01-architecture/ocr/DETECTION.md
doc/01-architecture/ocr/RECOGNITION.md
doc/01-architecture/ocr/TEXT_DIRECTION.md
doc/01-architecture/ocr/LAYOUT.md
doc/01-architecture/ocr/POSTPROCESS.md
doc/01-architecture/ocr/QUALITY.md
doc/01-architecture/ocr/READING_ORDER.md
doc/01-architecture/ocr/PROVIDERS.md

doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/CACHE_POLICY.md
```

---

# 83. Summary

Recognition state model chỉ tập trung vào state Recognition thực sự sở hữu:

```text
Recognition Availability
        ↓
Recognition Plan
        ↓
Recognition Operation Phase
        ↓
Candidate Validation
        ↓
Submitted to Runtime
```

OCR Architecture sở hữu:

```text
Detection
Recognition semantics
Text Direction
Layout
OCR Document
Quality
Reading Order
Provider semantics
```

Runtime sở hữu:

```text
WorkItem
Attempt
Authority
Cancellation
Retry
Publication Decision
```

Provider Manager sở hữu:

```text
Provider Lifecycle
Provider Health
Provider Capacity
```

Artifact Store sở hữu:

```text
Published Recognition Artifact
```

Boundary quan trọng nhất:

```text
Recognition may create a valid Candidate.

Quality may report that OCR is good or poor.

Only Runtime decides whether that Candidate matters.

Only Artifact Store owns the accepted published Artifact.
```
