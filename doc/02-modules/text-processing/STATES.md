# Text Processing Module States

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `02-modules/text-processing/STATES.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`

---

# 1. Purpose

Tài liệu này định nghĩa state model mà Text Processing Module thực sự sở hữu.

Text Processing state model bao phủ:

* module availability
* Processing Plan lifecycle
* Attempt-local operation phases
* Candidate SourceDocument validation
* SourceDocument completeness
* cancellation/deadline observations
* cleanup observations
* Runtime Candidate disposition observations
* concurrency rules
* recovery
* state invariants

Text Processing không định nghĩa canonical lifecycle của:

* WorkItem
* Attempt
* Scheduler
* Work Queue
* retry
* Runtime cancellation
* supersession
* authority
* Artifact publication
* Artifact retention
* Storage
* Translation
* Reading Session

---

# 2. State Ownership

Text Processing owns:

```text
TextProcessingAvailabilityState

ProcessingPlanState

TextProcessingOperationPhase

CandidateValidationState

TextProcessingCompleteness

CleanupObservation
```

Text Processing does not own:

```text
TextProcessingJobState

WorkItemState

AttemptState

QueueState

SchedulerState

RetryState

CancellationState

SupersessionState

PublicationState

ArtifactLifecycleState

TranslationState

SessionState
```

External states may be observed but are not mutated by Text Processing.

---

# 3. State Model Overview

```text
Text Processing Module
│
├── Module Availability
│
├── Attempt-Local Processing Plan
│
├── Attempt-Local Operation Phase
│
├── Candidate Validation
│
├── SourceDocument Completeness
│
└── External Observations
    ├── Cancellation Context
    ├── Deadline Context
    ├── Runtime Candidate Disposition
    └── Cleanup Observation
```

Không tồn tại module-owned:

```text
TextProcessingJob Registry

TextProcessing Retry Registry

TextProcessing Cancellation Registry

TextProcessing Supersession Registry
```

---

# 4. State Principles

## 4.1 Attempt-Local State

Các state sau chỉ tồn tại trong một Runtime Attempt:

* Processing Plan
* Operation Phase
* Candidate Validation
* Cleanup Observation

Chúng không phải durable business state.

---

## 4.2 Runtime Owns Lifecycle

Runtime sở hữu:

```text
Queued

Running

Succeeded

Failed

Canceled

Abandoned

Stale
```

Text Processing không tạo parallel lifecycle.

---

## 4.3 Candidate Is Not Published Artifact

```text
Candidate VALID
    ≠
SourceDocument published
```

`VALID` chỉ có nghĩa Candidate hợp lệ theo Text Processing Contract.

---

## 4.4 Operation Phase Is Diagnostic

Operation Phase dùng cho:

* diagnostics
* metrics
* cancellation checkpoints
* error localization
* resource cleanup

Nó không phải Runtime Attempt state.

---

## 4.5 SourceDocument Semantics Are Separate

SourceDocument completeness hoặc warning không quyết định Runtime terminal outcome.

Ví dụ:

```text
Completeness = EMPTY_VALID
```

vẫn có thể là Candidate hợp lệ.

---

# 5. Text Processing Availability State

```text
TextProcessingAvailabilityState
├── UNINITIALIZED
├── INITIALIZING
├── AVAILABLE
├── DEGRADED
├── UNAVAILABLE
├── DRAINING
└── STOPPED
```

Availability mô tả khả năng của module boundary nhận execution.

---

# 6. UNINITIALIZED

Module chưa active.

Module-owned structures chưa sẵn sàng:

* Processing Profile registry
* rule registry
* input adapter
* SourceDocument validator
* Candidate validator
* compatibility evaluator

Allowed:

```text
UNINITIALIZED
    → INITIALIZING
    → STOPPED
```

---

# 7. INITIALIZING

Module chuẩn bị:

* Processing Profile definitions
* processing rules
* validators
* SourceDocument builder
* compatibility semantics
* diagnostics schema

Module không initialize:

* Scheduler
* Work Queue
* Runtime cancellation
* Artifact Store
* Storage
* Translation

Allowed next:

```text
AVAILABLE

DEGRADED

UNAVAILABLE

DRAINING
```

---

# 8. AVAILABLE

Module có thể xử lý Attempt hợp lệ.

Requirements:

```text
contract_valid

profile_registry_ready

rule_registry_ready

input_adapter_ready

document_validator_ready

candidate_validator_ready

runtime_dependencies_available
```

Allowed:

```text
DEGRADED

UNAVAILABLE

DRAINING
```

---

# 9. DEGRADED

Module vẫn usable nhưng một số optional capability không khả dụng.

Examples:

* optional classification unavailable
* advanced grouping disabled
* hierarchical reconstruction unavailable
* diagnostics reduced
* resource pressure forces conservative profile

Text Processing nên ưu tiên degraded-but-safe behavior.

Allowed:

```text
AVAILABLE

UNAVAILABLE

DRAINING
```

---

# 10. UNAVAILABLE

Module không thể thực hiện contract-valid processing.

Examples:

* rule/profile registry invalid
* critical dependency unavailable
* Candidate validator unavailable
* SourceDocument builder unavailable

Runtime quyết định WorkItem outcome.

Allowed:

```text
INITIALIZING

DEGRADED

AVAILABLE

DRAINING
```

---

# 11. DRAINING

Module:

* không nhận execution mới
* không bắt đầu optional expensive work
* cho active Attempt hoàn thành hoặc cleanup
* tiếp tục cooperative cancellation
* release Attempt-local resources

Allowed:

```text
DRAINING
    → STOPPED
```

---

# 12. STOPPED

Không còn active module-owned resources/state.

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
   ┌──┼─────────────┐
   ↓  ↓             ↓
AVAILABLE ←→ DEGRADED ←→ UNAVAILABLE
   \        |          /
            ↓
         DRAINING
            ↓
          STOPPED
```

---

# 14. Processing Plan State

```text
ProcessingPlanState
├── NOT_CREATED
├── BUILDING
├── VALIDATING
├── READY
└── INVALID
```

Plan State là Attempt-local.

---

# 15. NOT_CREATED

Chưa có Processing Plan.

```text
NOT_CREATED
    → BUILDING
```

---

# 16. BUILDING

Plan được tạo từ:

* TextProcessingAttemptInput
* RecognitionArtifactRef
* ProcessingProfile
* ProcessingOptions
* SourceContext
* Configuration Snapshot
* Privacy Context
* Runtime resource constraints

Allowed:

```text
BUILDING
    → VALIDATING

BUILDING
    → INVALID
```

---

# 17. VALIDATING

Plan validation kiểm tra:

* Recognition Artifact compatible
* OCRDocumentRef resolvable
* Processing Profile supported
* Processing Options coherent
* privacy compatible
* requested structure mode supported
* required operations available

Allowed:

```text
READY

INVALID
```

---

# 18. READY

Plan immutable và executable.

Fixed:

* Processing Profile
* Processing Options
* structure mode
* rule versions
* compatibility policy
* privacy constraints
* Configuration Snapshot

Không quay lại `BUILDING`.

---

# 19. INVALID

Plan không thể execute.

Examples:

* unsupported profile
* conflicting options
* incompatible upstream Artifact
* unresolved required reference
* privacy conflict

Terminal cho Plan instance.

Runtime vẫn quyết định Attempt disposition.

---

# 20. Processing Plan Transition

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

# 21. Processing Plan Invariants

1. Một Attempt có tối đa một active Plan.
2. READY Plan immutable.
3. INVALID Plan không execute.
4. Plan không chứa Translation configuration.
5. Plan không chứa Runtime retry state.
6. Plan không chứa queue priority mutation.
7. Plan giữ Configuration Snapshot identity.
8. Plan không weaken Privacy Context.
9. Plan không grant authority.
10. Plan không publish Artifact.

---

# 22. Text Processing Operation Phase

```text
TextProcessingOperationPhase
├── NOT_STARTED
├── VALIDATING
├── ADAPTING_INPUT
├── NORMALIZING
├── RECONSTRUCTING
├── GROUPING
├── CLASSIFYING
├── BUILDING_DOCUMENT
├── VALIDATING_TRACEABILITY
├── ASSEMBLING_CANDIDATE
├── VALIDATING_CANDIDATE
├── FINALIZING
└── FINISHED
```

Đây là module-local processing flow.

---

# 23. Primary Operation Path

```text
NOT_STARTED
    ↓
VALIDATING
    ↓
ADAPTING_INPUT
    ↓
NORMALIZING
    ↓
RECONSTRUCTING
    ↓
GROUPING
    ↓
CLASSIFYING
    ↓
BUILDING_DOCUMENT
    ↓
VALIDATING_TRACEABILITY
    ↓
ASSEMBLING_CANDIDATE
    ↓
VALIDATING_CANDIDATE
    ↓
FINALIZING
    ↓
FINISHED
```

Optional stages có thể skip theo Plan.

---

# 24. VALIDATING

Checks:

* Attempt Input contract
* RecognitionArtifactRef
* required OCR references
* Processing Profile
* Processing Options
* Privacy Context
* source identity

Không revalidate toàn bộ OCR internals.

---

# 25. ADAPTING_INPUT

Resolve:

```text
RecognitionArtifact
      ↓
OCRDocumentRef
ReadingOrderResultRef?
QualityReportRef?
      ↓
ProcessingInputDocument
```

Không mutate upstream Artifact.

---

# 26. NORMALIZING

Thực hiện deterministic surface normalization.

Không semantic rewrite.

Outputs remain linked to RawText.

---

# 27. RECONSTRUCTING

Có thể:

* join wrapped lines
* preserve separate fragments
* identify paragraph boundaries
* reconstruct textual groups

Không redefine canonical OCR Reading Order.

---

# 28. GROUPING

Group reconstructed structures thành logical source groups.

Uncertainty fallback:

```text
PRESERVE_SEPARATE
```

---

# 29. CLASSIFYING

Assign source-oriented block roles.

Examples:

```text
DIALOGUE
NARRATION
PARAGRAPH
CAPTION
UNKNOWN
```

Unknown hợp lệ.

---

# 30. BUILDING_DOCUMENT

Construct:

```text
SourceDocument

SourceBlocks

RootBlockIds

BlockSequence

ExcludedBlocks
```

No Translation Units.

---

# 31. VALIDATING_TRACEABILITY

Checks:

* every textual block has OCR source evidence
* RawText provenance exists
* NormalizedText derivation valid
* hierarchy acyclic
* BlockSequence references valid
* source identity consistent

Failure here có thể invalidate Candidate.

---

# 32. ASSEMBLING_CANDIDATE

Create:

```text
CandidateSourceDocumentArtifact
```

with:

* SourceDocument
* completeness
* warnings
* compatibility metadata
* traceability metadata
* integrity metadata

---

# 33. VALIDATING_CANDIDATE

Module-level validation only.

Checks:

* Candidate identity
* Artifact type
* owner module
* SourceDocument contract
* compatibility metadata
* traceability metadata
* completeness
* privacy
* no Translation-specific fields
* no Runtime terminal state

---

# 34. FINALIZING

Perform:

* diagnostics summary
* completion metadata
* resource cleanup preparation
* Candidate submission preparation

No Artifact publication.

---

# 35. FINISHED

Local Text Processing execution ended.

```text
FINISHED
    ≠
Runtime SUCCEEDED
```

Runtime may still:

* accept Candidate
* reject stale
* reject canceled
* reject duplicate
* fail completion handling

---

# 36. Phase Skipping

Skip allowed only when Plan permits.

Examples:

```text
EnableNormalization = false
    → skip NORMALIZING
```

```text
EnableGrouping = false
    → skip GROUPING
```

```text
EnableClassification = false
    → skip CLASSIFYING
```

Skipped optional work must not break SourceDocument validity.

---

# 37. Phase Entry Rules

Before expensive work:

1. check CancellationContext
2. check Runtime deadline
3. validate prerequisites
4. ensure required leases/resources
5. Plan must be READY
6. enforce privacy
7. avoid optional expensive work while DRAINING

---

# 38. Phase Exit Rules

Before moving phase:

1. validate phase output
2. record duration
3. observe cancellation/deadline
4. release no-longer-needed resources
5. preserve traceability
6. normalize warnings/errors
7. determine next phase explicitly

---

# 39. Phase Failure

Any phase may return:

```text
TextProcessingModuleError
```

Local flow:

```text
Current Phase
    ↓
FINALIZING
    ↓
FINISHED
```

Runtime decides terminal outcome.

---

# 40. Candidate Validation State

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

# 41. Candidate NOT_CREATED

```text
NOT_CREATED
    → ASSEMBLING
```

---

# 42. Candidate ASSEMBLING

Build:

```text
CandidateArtifactId

RecognitionArtifactRef

SourceDocument

Completeness

Warnings

CompatibilityMetadata

TraceabilityMetadata

IntegrityMetadata
```

Allowed:

```text
VALIDATING

INVALID
```

---

# 43. Candidate VALIDATING

Checks:

* Candidate fields
* SourceDocument contract
* traceability
* privacy
* Artifact type
* ownership
* compatibility
* no translated content
* no Runtime state

Allowed:

```text
VALID

INVALID
```

---

# 44. Candidate VALID

Candidate is:

* module-contract valid
* immutable
* traceable
* translation-independent
* ready for Runtime submission

Allowed:

```text
VALID
    → SUBMITTED_TO_RUNTIME
```

---

# 45. Candidate INVALID

Candidate violates Text Processing Contract.

Examples:

* invalid SourceDocument
* missing source evidence
* cyclic hierarchy
* missing compatibility metadata
* translated content leaked in
* privacy violation

Terminal for Candidate instance.

---

# 46. SUBMITTED_TO_RUNTIME

Candidate crossed module boundary.

After submission:

* no mutation
* Runtime validates authority
* Runtime accepts/rejects
* Artifact Store may receive ownership
* rejected Candidate cleaned up

---

# 47. Candidate Transition

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

# 48. Runtime Candidate Disposition

External Runtime observation:

```text
RuntimeCandidateDisposition
├── ACCEPTED
├── REJECTED_STALE
├── REJECTED_CANCELED
├── REJECTED_DUPLICATE
├── REJECTED_INVALID
└── REJECTED_RUNTIME_FAILURE
```

Không phải CandidateValidationState.

---

# 49. Candidate Ownership Boundary

```text
ASSEMBLING / VALIDATING / VALID
    → Text Processing producer ownership

SUBMITTED_TO_RUNTIME
    → transfer pending

ACCEPTED
    → Artifact Store ownership

REJECTED_*
    → cleanup
```

---

# 50. Text Processing Completeness

```text
TextProcessingCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

Completeness là Artifact metadata.

Không phải execution state.

---

# 51. COMPLETE

Required source content represented successfully.

---

# 52. PARTIAL

Một phần source content usable.

Requirements:

* traceability preserved
* missing/failed scope explicit
* warning recorded
* Candidate contract valid

---

# 53. EMPTY_VALID

Valid processing produced no processable source text.

Example:

```text
SourceDocument.Blocks = []

Completeness = EMPTY_VALID
```

Không phải failure.

---

# 54. UNKNOWN

Không đủ evidence để xác định completeness.

Không silently convert thành COMPLETE/PARTIAL.

---

# 55. Warning State Is Not Needed

Text Processing không cần một:

```text
WarningState
```

Warnings là immutable observations attached to:

* processing diagnostics
* SourceDocument
* Candidate Artifact

A Candidate may be:

```text
VALID
+
warnings.length > 0
```

---

# 56. Reconstruction Uncertainty

Reconstruction uncertainty không phải failure state.

Possible behavior:

```text
uncertain join
    ↓
preserve separate
    ↓
warning
```

---

# 57. Grouping Uncertainty

```text
grouping uncertain
    ↓
PRESERVE_SEPARATE
```

Candidate vẫn có thể VALID.

---

# 58. Classification Uncertainty

```text
classification uncertain
    ↓
BlockType = UNKNOWN
```

Không tạo FAILED state.

---

# 59. Structure Uncertainty

Nếu hierarchy không đủ evidence:

```text
HIERARCHICAL
    ↓ fallback
FLAT
```

khi Processing Profile cho phép.

---

# 60. Cancellation Observation

Text Processing observe Runtime CancellationContext.

Possible local observations:

```text
NOT_REQUESTED

REQUESTED

ACKNOWLEDGED_BY_MODULE

LOCAL_WORK_STOPPING

LOCAL_WORK_STOPPED
```

Đây không phải canonical Runtime cancellation state.

---

# 61. Cancellation Checkpoints

Recommended:

* before input adaptation
* before normalization
* between bounded reconstruction work
* before grouping/classification
* before SourceDocument assembly
* before Candidate assembly
* before Candidate submission

---

# 62. Cancellation Flow

```text
Current Phase
    ↓
Cancellation Requested
    ↓
Stop New Expensive Work
    ↓
Cleanup Attempt-Local Resources
    ↓
FINALIZING
    ↓
FINISHED
```

Runtime decides:

```text
CANCELED

ABANDONED

FAILED
```

---

# 63. Supersession Boundary

Text Processing does **not** own:

```text
SUPERSEDED
```

as local terminal state.

Supersession is Runtime authority/relevance.

Typical flow:

```text
Revision N processing
        ↓
Revision N+1 becomes authoritative
        ↓
Runtime revokes N authority
        ↓
Text Processing observes cancellation/staleness
        ↓
Candidate N rejected if produced
```

---

# 64. Deadline Observation

Deadline belongs to Runtime.

Text Processing may observe:

```text
DeadlineAvailable

RemainingBudget

DeadlineExceeded
```

Before optional expensive work:

```text
remaining_budget
    must be sufficient
```

otherwise module may degrade or stop according to Plan.

---

# 65. Retry Boundary

There is no module-owned:

```text
RETRYING
```

state.

Text Processing may return:

```text
RetryHint
```

Runtime decides new Attempt.

New retry = new Attempt identity.

Existing Plan/Phase/Candidate state is never reset.

---

# 66. No Queue State

There is no:

```text
QUEUED
```

Text Processing state.

Queue/Scheduler belong to Runtime.

Text Processing begins only when Runtime executes the Attempt.

---

# 67. No COMPLETED / FAILED Module Terminal States

There is no module-owned domain lifecycle:

```text
COMPLETED

FAILED

CANCELLED

SUPERSEDED
```

`FINISHED` means only local execution ended.

Outcome belongs to Runtime Attempt.

---

# 68. No BUILDING_CONTEXT State

Legacy `BUILDING_CONTEXT` is removed.

Reason:

Text Processing no longer builds Translation Units or translation-context packages.

Source structural context belongs in SourceDocument structure.

Translation-specific contextualization belongs to Translation.

---

# 69. No Translation Segment State

Text Processing does not maintain:

```text
PreparedSegmentState

TranslationUnitState

ContextPackageState
```

because these are not Text Processing-owned outputs.

---

# 70. Cleanup Observation

```text
CleanupObservation
├── NOT_REQUIRED
├── PENDING
├── RUNNING
├── COMPLETED
└── FAILED
```

Observation only.

Physical resource lifecycle belongs to Runtime/Resource Manager.

---

# 71. Resource Flow

```text
Acquire Recognition Artifact Lease
        ↓
Resolve OCR References
        ↓
Build Attempt-Local Structures
        ↓
Build Candidate
        ↓
Submit or Cleanup
        ↓
Release Attempt-Local Resources
        ↓
Release Leases
```

---

# 72. Runtime Disposition Observation

Text Processing may observe:

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
* late-output handling

Text Processing does not mutate disposition.

---

# 73. Invalid Transitions

Forbidden:

```text
STOPPED
    → AVAILABLE
without INITIALIZING
```

```text
DRAINING
    → AVAILABLE
```

```text
Plan READY
    → BUILDING
```

```text
Plan INVALID
    → READY
```

```text
Candidate VALID
    → ASSEMBLING
```

```text
Candidate INVALID
    → VALID
```

```text
Candidate SUBMITTED_TO_RUNTIME
    → VALIDATING
```

```text
FINISHED
    → NORMALIZING
```

---

# 74. Invalid Transition Handling

When invalid transition occurs:

1. reject transition
2. preserve current state
3. record invariant violation
4. avoid Runtime authority implication
5. cleanup duplicate resources if needed
6. emit bounded diagnostics
7. do not crash entire application unless corruption is unrecoverable

---

# 75. Concurrency Rules

1. One Attempt owns one Text Processing execution context.
2. Plan transitions logically serialized.
3. Operation phase transitions logically serialized.
4. Candidate transitions logically serialized.
5. Candidate submitted at most once.
6. Attempt-local cleanup idempotent.
7. Artifact leases released once.
8. Parallel node processing must preserve deterministic output.
9. Ordering-sensitive reconstruction remains deterministic.
10. DRAINING blocks new Attempts.
11. Runtime disposition may arrive after local FINISHED.
12. Module never races Runtime authority ownership.

---

# 76. Safe Parallelism

Safe examples:

* normalize independent TextNodes
* compute reconstruction evidence
* compute grouping evidence
* classify independent SourceGroups
* calculate traceability metadata

Unsafe without deterministic coordination:

* assigning final BlockSequence
* conflicting merge decisions
* hierarchy construction
* Candidate finalization

---

# 77. Determinism State Rule

Given equivalent:

```text
Recognition semantic input

Processing Profile

Processing Options

Rule versions

Processing Strategy version
```

module state path may vary internally due concurrency, but final semantic SourceDocument must remain equivalent.

---

# 78. State Transition Diagnostics

Conceptual record:

```text
TextProcessingStateTransition
├── RevisionId
├── WorkItemId
├── AttemptId
├── StateCategory
├── PreviousState
├── NextState
├── Trigger
├── OccurredAt
├── OperationPhase?
├── TraceId
└── Metadata?
```

No full source text.

---

# 79. Recognition-Owned Transition Triggers

Examples:

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


CANDIDATE_ASSEMBLY_STARTED

CANDIDATE_ASSEMBLED

CANDIDATE_VALID

CANDIDATE_INVALID

CANDIDATE_SUBMITTED
```

---

# 80. State and Event Relationship

Possible optional facts:

```text
Plan READY
    → TEXT_PROCESSING_PLAN_CREATED
```

```text
BUILDING_DOCUMENT completed
    → TEXT_PROCESSING_DOCUMENT_BUILT
```

```text
Candidate VALID
    → TEXT_PROCESSING_CANDIDATE_VALIDATED
```

```text
Candidate SUBMITTED
    → TEXT_PROCESSING_CANDIDATE_SUBMITTED
```

Events do not grant authority.

---

# 81. Recovery

Text Processing-owned active state is ephemeral.

After crash, do not restore directly:

```text
Processing Plan

Operation Phase

Candidate Assembly State

Cleanup Observation

Attempt-Local Working Nodes
```

Runtime may create new Attempt.

---

# 82. Recovery Flow

```text
Attempt interrupted
      ↓
Runtime records interruption
      ↓
New Attempt created if policy allows
      ↓
Recognition Artifact reacquired
      ↓
New Processing Plan
      ↓
Text Processing reruns
```

---

# 83. Candidate Recovery

Unaccepted Candidate:

* not published
* not authoritative
* must not resurrect automatically
* cleanup when abandoned

Published SourceDocument recovery belongs to Artifact Store/Storage.

---

# 84. State Persistence

MVP does not persist active Text Processing state.

Ephemeral:

```text
Processing Plan

Operation Phase

Working Nodes

Reconstruction Decisions

Candidate Validation State

Cleanup Observation
```

Potentially persistent output:

```text
Published SourceDocument Artifact
```

through Artifact Store/Storage policy.

---

# 85. No TextProcessingJob Persistence

Legacy model assumed durable/reconstructable:

```text
TextProcessingJob
```

with:

```text
CREATED

QUEUED

COMPLETED

FAILED
```

This model is removed.

Runtime WorkItem/Attempt already provides execution lifecycle.

---

# 86. MVP State Model

Required module availability:

```text
UNINITIALIZED

INITIALIZING

AVAILABLE

DEGRADED

UNAVAILABLE

DRAINING

STOPPED
```

Required Plan states:

```text
NOT_CREATED

BUILDING

VALIDATING

READY

INVALID
```

Required phases:

```text
VALIDATING

ADAPTING_INPUT

NORMALIZING

RECONSTRUCTING

GROUPING

CLASSIFYING

BUILDING_DOCUMENT

VALIDATING_TRACEABILITY

ASSEMBLING_CANDIDATE

VALIDATING_CANDIDATE

FINALIZING

FINISHED
```

Required Candidate states:

```text
NOT_CREATED

ASSEMBLING

VALIDATING

VALID

INVALID

SUBMITTED_TO_RUNTIME
```

---

# 87. MVP Simplification

Canonical module control flow:

```text
Build Plan
    ↓
Adapt Recognition Artifact
    ↓
Normalize
    ↓
Reconstruct
    ↓
Group
    ↓
Classify
    ↓
Build SourceDocument
    ↓
Validate Traceability
    ↓
Build Candidate
    ↓
Submit Runtime
    ↓
Cleanup
```

No second Runtime lifecycle is created.

---

# 88. When to Expand State Detail

Only expand when concrete requirements justify it:

* long-page incremental processing
* multi-page SourceDocument
* chunked reconstruction
* streaming structural updates
* user correction integration
* structured browser adapters
* distributed execution

Prefer diagnostics before adding public state.

---

# 89. Availability Invariants

1. AVAILABLE means required module-owned components ready.
2. DRAINING accepts no new Attempts.
3. STOPPED has no active module-owned execution.
4. UNAVAILABLE does not create executable Plan.
5. DEGRADED remains usable for supported profiles.
6. Availability does not equal Runtime health.
7. Availability does not depend on one Attempt result.
8. Privacy constraints cannot be weakened.

---

# 90. Plan Invariants

1. One active Plan per Attempt.
2. READY Plan immutable.
3. INVALID Plan never executes.
4. Plan has no Translation-specific policy.
5. Plan has no credentials.
6. Plan preserves Configuration Snapshot.
7. Plan does not modify Runtime priority.
8. Plan does not own deadline.
9. Plan does not grant authority.
10. Plan does not schedule retry.

---

# 91. Phase Invariants

1. One current local phase.
2. Phase progression explicit.
3. Optional phase skip must be Plan-defined.
4. Expensive phases check cancellation/deadline.
5. Phase output validated before next dependency.
6. FINISHED does not imply Runtime success.
7. Phase failure produces normalized module error.
8. Phase state remains Attempt-local.
9. No Translation context-building phase exists.
10. No canonical Reading Order resolution phase exists.

---

# 92. Candidate Invariants

1. One Candidate instance has one validation state.
2. Candidate ID unique.
3. Candidate immutable after VALID.
4. INVALID Candidate never submitted as valid.
5. SUBMITTED Candidate never mutated.
6. Candidate contains valid SourceDocument.
7. Candidate preserves Recognition Artifact lineage.
8. Candidate contains no Translation Unit.
9. Candidate contains no Runtime state.
10. Candidate contains no credentials.
11. Candidate rejection requires cleanup.
12. Candidate submission does not mean publication.

---

# 93. Completeness Invariants

1. EMPTY_VALID is successful semantic output.
2. PARTIAL is explicit.
3. UNKNOWN is preserved.
4. Completeness is Artifact metadata.
5. Completeness does not define Runtime outcome.
6. Warning count does not change completeness automatically.

---

# 94. Reconstruction Invariants

1. RawText never overwritten.
2. NormalizedText remains traceable.
3. Text Processing does not invent semantic source text.
4. Uncertain merges preserve separation.
5. Canonical OCR Reading Order is not redefined.
6. BlockSequence derives from reconstruction plus source-order evidence.
7. Unknown classification is valid.
8. Exclusion never deletes source evidence.
9. Hierarchy must be acyclic.
10. Every textual SourceBlock has source lineage.

---

# 95. External-State Invariants

1. Runtime owns WorkItem.
2. Runtime owns Attempt.
3. Runtime owns queueing.
4. Runtime owns cancellation authority.
5. Runtime owns supersession/staleness.
6. Runtime owns retry.
7. Runtime owns terminal outcome.
8. Artifact Store owns publication.
9. Resource Manager owns physical resource lifecycle.
10. Storage owns durable persistence.
11. Translation owns Translation Units and translated output.
12. Text Processing maintains no parallel lifecycle registries.

---

# 96. Testing Requirements — Availability

Test:

* initialize AVAILABLE
* initialize DEGRADED
* initialize UNAVAILABLE
* DRAINING rejects new work
* STOPPED clears module state
* restart requires INITIALIZING

---

# 97. Testing Requirements — Plan

Test:

* valid Plan
* invalid Processing Profile
* invalid Processing Options
* incompatible Recognition Artifact
* privacy conflict
* READY immutable
* INVALID cannot execute
* Configuration Snapshot preserved

---

# 98. Testing Requirements — Operation

Test:

* normal Comic Page flow
* Comic Region flow
* Novel Page flow
* Generic conservative flow
* optional normalization skipped
* optional classification skipped
* empty-valid flow
* cancellation during reconstruction
* deadline during grouping
* traceability failure
* FINISHED without Runtime success implication

---

# 99. Testing Requirements — Candidate

Test:

* valid Candidate
* invalid SourceDocument
* missing OCR lineage
* cyclic hierarchy
* invalid BlockSequence
* translated content leakage
* missing compatibility metadata
* immutable after VALID
* Candidate submit once
* rejected Candidate cleanup

---

# 100. Testing Requirements — Runtime Boundary

Test:

```text
Candidate VALID
    +
Revision stale
    ↓
Runtime rejects
```

```text
Cancellation requested
    ↓
module stops local work
    ↓
Runtime decides CANCELED
```

```text
module FINISHED
    ↓
Runtime completion fails
    ↓
no publication
```

```text
retry
    ↓
new Attempt
    ↓
old Plan never reused as active state
```

---

# 101. Property Tests

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
all Attempt-local resources
are eventually released
```

```text
every textual SourceBlock
has source evidence
```

```text
SourceDocument hierarchy
is acyclic
```

```text
Text Processing never changes
Runtime Attempt state
```

---

# 102. Recommended MVP Decisions

```text
Module Availability remains explicit.

Processing Plan remains explicit.

Operation phases remain diagnostic/local.

No TextProcessingJob state machine.

No QUEUED state.

No COMPLETED / FAILED / CANCELLED / SUPERSEDED states.

No BUILDING_CONTEXT state.

No Translation Unit lifecycle.

Candidate validation remains explicit.

Completeness remains Artifact metadata.

Runtime owns every terminal outcome.

Candidate transfer begins on submission.

Active module state remains ephemeral.
```

---

# 103. Related Documents

```text
02-modules/text-processing/README.md
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md
02-modules/text-processing/EVENTS.md
02-modules/text-processing/ERRORS.md

02-modules/recognition/CONTRACT.md
02-modules/recognition/STATES.md

01-architecture/ocr/POSTPROCESS.md
01-architecture/ocr/QUALITY.md
01-architecture/ocr/READING_ORDER.md

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

02-modules/translation/
```

---

# 104. Summary

Text Processing state model focuses only on module-owned state:

```text
Module Availability
        ↓
Processing Plan
        ↓
Operation Phase
        ↓
Candidate Validation
        ↓
Submitted to Runtime
```

Text Processing semantic output:

```text
Recognition Artifact
        ↓
Source Reconstruction
        ↓
SourceDocument
        ↓
Candidate SourceDocument Artifact
```

Runtime owns:

```text
Queue

WorkItem

Attempt

Authority

Cancellation

Supersession

Retry

Terminal Outcome
```

Artifact Store owns:

```text
Accepted SourceDocument Artifact
```

Translation owns:

```text
Translation Plan

Translation Units

Translation Context

Translated Content
```

Core rule:

```text
Text Processing owns
how SourceDocument is reconstructed.

Runtime owns
whether the work still matters.

Translation owns
how the SourceDocument is translated.
```
