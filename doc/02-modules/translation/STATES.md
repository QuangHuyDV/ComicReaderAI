# Translation Module States

> **Project:** CRAI
> **Module:** Translation
> **Path:** `02-modules/translation/STATES.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`

---

# 1. Purpose

Tài liệu này định nghĩa state model mà Translation Module thực sự sở hữu.

Translation state model bao phủ:

* module availability
* Translation Plan lifecycle
* Translation Unit planning
* Translation Batch local lifecycle
* Provider Execution observations
* provider output validation
* Candidate Translation Artifact validation
* Translation completeness
* cancellation/deadline observations
* cleanup observations
* Runtime Candidate disposition observations
* concurrency rules
* recovery
* state invariants

Translation không định nghĩa canonical lifecycle của:

* Runtime WorkItem
* Runtime Attempt
* Scheduler
* Work Queue
* retry
* cancellation
* supersession
* authority
* Artifact publication
* Artifact retention
* Reading Session active variant
* Provider lifecycle

---

# 2. State Ownership

Translation owns:

```text
TranslationModuleAvailabilityState

TranslationPlanState

TranslationUnitPlanningState

TranslationBatchState

ProviderExecutionObservation

ProviderOutputValidationState

TranslationCandidateValidationState

TranslationCompleteness

CleanupObservation
```

Translation does not own:

```text
TranslationJobState

TranslationAttemptState

WorkItemState

AttemptState

QueueState

SchedulerState

RetryState

CancellationState

SupersessionState

PublicationState

ArtifactLifecycleState

ActiveVariantState

ProviderLifecycleState
```

External states may be observed but are not mutated by Translation.

---

# 3. State Model Overview

```text
Translation Module
│
├── Module Availability
│
├── Translation Plan
│
├── Translation Unit Planning
│
├── Translation Batch
│
│   └── Provider Execution Observation
│
├── Provider Output Validation
│
├── Candidate Validation
├── Translation Completeness
│
└── External Observations
    ├── Runtime Cancellation
    ├── Runtime Deadline
    ├── Runtime Authority
    ├── Candidate Disposition
    └── Cleanup
```

There is no module-owned:

```text
TranslationJob Registry

TranslationAttempt Registry

Translation Retry Registry

Translation Supersession Registry

Translation Publication Registry
```

---

# 4. State Principles

## 4.1 Runtime Attempt Is the Execution Boundary

Translation executes inside:

```text
Runtime WorkItem
    ↓
Runtime Attempt
    ↓
Translation Module
```

Translation does not create another Attempt lifecycle.

---

## 4.2 Semantic State vs Runtime State

Translation-owned state describes:

```text
what translation work has been planned

what batch is being processed

what provider output has been observed

whether translated output is semantically valid
```

Runtime-owned state describes:

```text
whether execution is queued

whether execution is running

whether it should retry

whether it was cancelled

whether it became stale

whether the Attempt succeeded
```

---

## 4.3 Candidate Is Not Published Artifact

```text
Candidate VALID
    ≠
TranslationArtifact published
```

Candidate validation means only:

```text
Translation contract-valid
```

---

## 4.4 Provider Response Is Not Translation Success

```text
HTTP 200
or
provider response received
    ≠
valid translated output
```

Provider output must pass normalization and validation.

---

## 4.5 Partiality Is Semantic Metadata

```text
PARTIAL
```

is Translation completeness.

It is not Runtime lifecycle state.

---

## 4.6 Variant Activity Is External

Translation Artifact may describe immutable variant semantics.

But:

```text
ACTIVE

INACTIVE
```

for the current Reading Session are not Translation execution states.

---

# 5. Translation Module Availability

```text
TranslationModuleAvailabilityState
├── UNINITIALIZED
├── INITIALIZING
├── AVAILABLE
├── DEGRADED
├── UNAVAILABLE
├── DRAINING
└── STOPPED
```

---

# 6. UNINITIALIZED

Translation module has not initialized required module-owned components.

Not ready:

* Translation Profiles
* Unit planner
* Batch planner
* validators
* Provider Adapter registry references
* terminology integration
* context builder

Allowed:

```text
UNINITIALIZED
    → INITIALIZING
    → STOPPED
```

---

# 7. INITIALIZING

Module prepares:

* Translation Profile definitions
* Translation Unit planner
* Batch planner
* context builder
* terminology resolver
* output validators
* Candidate validator
* Provider capability mappings

Translation does not initialize:

* Runtime Queue
* Runtime Scheduler
* provider credentials
* Provider lifecycle
* Artifact Store
* Reading Session

Allowed:

```text
INITIALIZING
    → AVAILABLE
    → DEGRADED
    → UNAVAILABLE
    → DRAINING
```

---

# 8. AVAILABLE

Translation can execute supported Translation Plans.

Requirements:

```text
contract_valid

profile_registry_ready

unit_planner_ready

batch_planner_ready

candidate_validator_ready

provider_boundary_ready

required_dependencies_available
```

Allowed:

```text
AVAILABLE
    → DEGRADED
    → UNAVAILABLE
    → DRAINING
```

---

# 9. DEGRADED

Module remains usable with reduced optional capability.

Examples:

* advanced context unavailable
* optional Knowledge unavailable
* preferred provider unavailable
* streaming unavailable
* optional glossary unavailable
* only local provider available
* only fallback provider available

Allowed:

```text
DEGRADED
    → AVAILABLE
    → UNAVAILABLE
    → DRAINING
```

Degradation must never violate hard privacy/provider constraints.

---

# 10. UNAVAILABLE

Translation cannot satisfy required contract.

Examples:

* no eligible provider path
* Translation Profile registry invalid
* Candidate validator unavailable
* required source contract incompatible

Runtime decides WorkItem outcome.

Allowed:

```text
UNAVAILABLE
    → INITIALIZING
    → DEGRADED
    → AVAILABLE
    → DRAINING
```

---

# 11. DRAINING

Module:

* accepts no new module execution
* starts no optional provider work
* allows active Attempt-local cleanup
* cooperatively responds to cancellation
* releases Provider handles/resources

Allowed:

```text
DRAINING
    → STOPPED
```

---

# 12. STOPPED

No active module-owned execution resources remain.

Restart:

```text
STOPPED
    → INITIALIZING
```

---

# 13. Availability Diagram

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

# 14. Translation Plan State

```text
TranslationPlanState
├── NOT_CREATED
├── BUILDING
├── VALIDATING
├── READY
└── INVALID
```

Plan is Attempt-local.

---

# 15. NOT_CREATED

No Translation Plan exists.

```text
NOT_CREATED
    → BUILDING
```

---

# 16. BUILDING

Translation resolves:

* SourceDocumentArtifact
* Source Selection
* Translation Intent
* Translation Profile
* Context Policy
* Knowledge Snapshot
* Terminology Policy
* Provider Policy
* Privacy Context
* Partial Result Policy
* Configuration Snapshot

Allowed:

```text
BUILDING
    → VALIDATING

BUILDING
    → INVALID
```

---

# 17. VALIDATING

Checks:

* SourceDocument compatible
* target language present
* Translation Profile supported
* Provider Policy coherent
* Privacy Context compatible
* source selection valid
* required Knowledge available
* terminology constraints coherent
* context requirements satisfiable
* partial-result policy coherent

Allowed:

```text
VALIDATING
    → READY

VALIDATING
    → INVALID
```

---

# 18. READY

Translation Plan is immutable and executable.

Fixed:

* Translation Intent
* source semantic identity
* source selection
* target language
* Translation Profile
* context policy
* Knowledge identity
* terminology policy
* Provider Policy
* Privacy constraints
* Configuration Snapshot

No return to `BUILDING`.

---

# 19. INVALID

Plan cannot execute.

Examples:

* unsupported target language
* impossible provider constraint
* contradictory privacy/provider policy
* required Knowledge unavailable
* incompatible SourceDocument
* invalid Translation Profile

Terminal for this Plan instance.

Runtime still owns Attempt disposition.

---

# 20. Translation Plan Diagram

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

# 21. Translation Plan Invariants

1. One Attempt has at most one active Translation Plan.
2. READY Plan is immutable.
3. INVALID Plan never executes.
4. Plan references one SourceDocument semantic identity.
5. Plan has one target language.
6. Plan preserves Privacy Context.
7. Plan does not contain Runtime retry state.
8. Plan does not contain Runtime queue state.
9. Plan does not own authority.
10. Plan does not publish Artifact.

---

# 22. Translation Unit Planning State

```text
TranslationUnitPlanningState
├── NOT_STARTED
├── SELECTING_SOURCE
├── BUILDING_UNITS
├── VALIDATING_UNITS
├── READY
└── INVALID
```

---

# 23. SELECTING_SOURCE

Translation resolves SourceBlocks included by Source Selection.

Checks:

* selected blocks exist
* excluded block handling
* source sequence
* language hints
* structural types
* source traceability

---

# 24. BUILDING_UNITS

Translation constructs:

```text
SourceBlock[]
    ↓
TranslationUnit[]
```

Possible mappings:

```text
1 → 1

N → 1

1 → N
```

according to Translation Profile and semantic constraints.

---

# 25. VALIDATING_UNITS

Checks:

* unique TranslationUnitId
* valid SourceBlock refs
* deterministic order
* no lost source selection
* no unintended duplicates
* target language consistency
* traceability
* context-only content separated
* split/merge alignment metadata valid

---

# 26. Translation Units READY

All Translation Units satisfy contract.

They become immutable for the current Plan.

---

# 27. Translation Units INVALID

Unit planning cannot satisfy semantic/alignment contract.

Examples:

* source mapping lost
* duplicate Unit identity
* source split cannot be traced
* incompatible target-language grouping
* selected source content silently omitted

---

# 28. Unit Planning Diagram

```text
NOT_STARTED
    ↓
SELECTING_SOURCE
    ↓
BUILDING_UNITS
    ↓
VALIDATING_UNITS
   ┌────┴────┐
   ↓         ↓
 READY     INVALID
```

---

# 29. Translation Batch State

TranslationBatch remains a Translation-owned semantic/provider planning object.

```text
TranslationBatchState
├── CREATED
├── READY
├── EXECUTION_REQUESTED
├── OUTPUT_RECEIVED
├── VALIDATING_OUTPUT
├── VALID
└── INVALID
```

Important:

```text
TranslationBatchState
    ≠
Runtime Attempt State
```

---

# 30. Batch CREATED

Batch membership exists.

Fixed:

* TranslationBatchId
* TranslationUnitIds
* Unit sequence
* context refs
* terminology refs
* provider capability requirements

Provider execution may not yet be prepared.

---

# 31. Batch READY

Batch satisfies pre-execution requirements.

Checks:

* Unit membership valid
* provider limits resolvable
* context available
* terminology resolved
* Privacy Context satisfied
* eligible Provider path exists

---

# 32. EXECUTION_REQUESTED

Translation has submitted provider-neutral execution through the Provider boundary.

This means:

```text
provider execution requested
```

not:

```text
provider is definitely running
```

Physical execution belongs to Provider Management/provider implementation.

---

# 33. OUTPUT_RECEIVED

Provider Adapter returned normalized provider output.

Still not accepted Translation output.

Required next:

```text
VALIDATING_OUTPUT
```

---

# 34. VALIDATING_OUTPUT

Checks:

* expected Unit IDs
* missing Units
* duplicate Units
* unexpected Units
* structural parse
* target-language plausibility
* terminology constraints
* output length
* source leakage
* provider control leakage
* source alignment

---

# 35. Batch VALID

Provider output has passed required Translation validation for this Batch.

Validated TranslatedUnits may contribute to Candidate assembly.

---

# 36. Batch INVALID

Current Batch execution output cannot contribute as valid Translation output.

Examples:

* malformed provider result
* impossible alignment
* locked terminology violation under strict policy
* provider returned unrelated Unit IDs
* provider response violates Privacy/contract boundary

`INVALID` does not itself schedule retry.

Translation may return a RetryHint.

---

# 37. Batch Diagram

```text
CREATED
    ↓
READY
    ↓
EXECUTION_REQUESTED
    ↓
OUTPUT_RECEIVED
    ↓
VALIDATING_OUTPUT
   ┌────┴────┐
   ↓         ↓
 VALID     INVALID
```

---

# 38. Why `RUNNING` Is Removed From Batch State

Old model used:

```text
READY
  ↓
RUNNING
  ↓
VALIDATING
```

But actual provider execution may occur:

* in-process
* remote HTTP
* local model worker
* provider pool
* another process

Translation cannot always authoritatively know physical provider state.

Therefore state is separated into:

```text
TranslationBatch:
    EXECUTION_REQUESTED

ProviderExecutionObservation:
    REQUESTED / RUNNING / ...
```

---

# 39. Provider Execution Observation

```text
ProviderExecutionObservation
├── NOT_REQUESTED
├── REQUESTED
├── ACCEPTED
├── RUNNING
├── STREAMING
├── OUTPUT_RECEIVED
├── ERROR_RECEIVED
├── CANCEL_REQUESTED
├── PHYSICALLY_FINISHED
└── UNKNOWN
```

This is observational.

It is not provider lifecycle ownership.

---

# 40. NOT_REQUESTED

No provider request made.

---

# 41. REQUESTED

Translation submitted provider execution request.

---

# 42. ACCEPTED

Provider boundary acknowledged request.

It does not imply useful work has started.

---

# 43. RUNNING

Provider reports active processing when such information exists.

Optional.

---

# 44. STREAMING

Provider is emitting partial/token output.

Streaming provider output is not automatically public Translation output.

---

# 45. OUTPUT_RECEIVED

A response/output arrived.

Translation must normalize and validate it.

---

# 46. ERROR_RECEIVED

Provider Adapter returned normalized provider failure.

Translation may construct:

```text
TranslationModuleError

RetryHint
```

Runtime owns next execution decision.

---

# 47. CANCEL_REQUESTED

Physical provider cancellation has been requested.

This does not mean Runtime Attempt is `CANCELED`.

---

# 48. PHYSICALLY_FINISHED

Provider physical execution has ended.

It does not imply:

```text
Batch VALID

Candidate VALID

Attempt SUCCEEDED
```

---

# 49. UNKNOWN

Provider execution state cannot be reliably determined.

This is valid for some remote providers.

---

# 50. Provider Observation Rule

Correctness must not require reliable intermediate Provider state.

Architecture must tolerate:

```text
REQUESTED
    ↓
OUTPUT_RECEIVED
```

with no observable:

```text
ACCEPTED
RUNNING
```

---

# 51. Provider Output Validation State

Optional explicit validator state:

```text
ProviderOutputValidationState
├── NOT_STARTED
├── VALIDATING
├── VALID
├── VALID_WITH_WARNINGS
└── INVALID
```

---

# 52. VALID_WITH_WARNINGS

Output remains usable but contains warnings.

Examples:

* ambiguous terminology
* low confidence
* output-length anomaly
* source-language fragment preserved
* provider fallback used

This may still contribute to Candidate.

---

# 53. Translation Completeness

```text
TranslationCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

Completeness is Candidate/Artifact semantic metadata.

Not an execution lifecycle.

---

# 54. COMPLETE

All required Translation Units have valid translated output.

---

# 55. PARTIAL

At least one required Unit has valid output and at least one remains missing/failed.

Requirements:

* valid completed Units explicit
* missing Units explicit
* failed Units explicit
* alignment preserved
* PartialResultPolicy allows output

---

# 56. EMPTY_VALID

No selected content requires translated output.

Examples:

* SourceDocument has no translatable blocks
* selected blocks all use PRESERVE/SKIP policy
* empty source selection valid by policy

Not a failure.

---

# 57. UNKNOWN

Completeness cannot safely be determined.

Must remain explicit.

---

# 58. No PARTIALLY_COMPLETED Lifecycle State

Old:

```text
TranslationJob.PARTIALLY_COMPLETED
```

is removed.

New:

```text
Candidate.Completeness = PARTIAL
```

This separates semantic completeness from Runtime execution lifecycle.

---

# 59. Candidate Validation State

```text
TranslationCandidateValidationState
├── NOT_CREATED
├── ASSEMBLING
├── VALIDATING
├── VALID
├── INVALID
└── SUBMITTED_TO_RUNTIME
```

---

# 60. Candidate NOT_CREATED

No Candidate exists.

```text
NOT_CREATED
    → ASSEMBLING
```

---

# 61. Candidate ASSEMBLING

Collect:

```text
CandidateArtifactId

SourceDocumentArtifactRef

TranslationIntentId

TranslationProfileRef

TranslationUnits[]

TranslatedUnits[]

Completeness

MissingTranslationUnitIds[]

FailedTranslationUnitIds[]

Warnings[]

ProviderProvenance[]

CompatibilityMetadata

TraceabilityMetadata

IntegrityMetadata
```

---

# 62. Candidate VALIDATING

Checks:

* unique Candidate identity
* SourceDocument reference
* Translation Intent
* Translation Unit integrity
* TranslatedUnit mapping
* target language
* completeness
* missing/failed Unit consistency
* terminology policy
* source traceability
* Provider provenance
* Compatibility metadata
* Privacy Context
* no credentials
* no Runtime state

---

# 63. Candidate VALID

Candidate satisfies Translation-owned contract.

It is:

* immutable
* traceable
* provider-neutral
* non-authoritative
* ready for Runtime submission

---

# 64. Candidate INVALID

Candidate violates Translation contract.

Examples:

* missing Unit alignment
* invalid completeness
* duplicated TranslatedUnit
* wrong target language
* lost SourceBlock refs
* credential leakage
* provider control content leaked
* privacy violation

Cannot be submitted as valid.

---

# 65. SUBMITTED_TO_RUNTIME

Candidate crossed Translation → Runtime boundary.

After this:

* no Candidate mutation
* Runtime validates authority
* Runtime may accept/reject
* Artifact Store may receive accepted Candidate
* rejected Candidate cleaned according to Resource policy

---

# 66. Candidate Diagram

```text
NOT_CREATED
    ↓
ASSEMBLING
    ↓
VALIDATING
   ┌────┴────┐
   ↓         ↓
 VALID     INVALID
   ↓
SUBMITTED_TO_RUNTIME
```

---

# 67. Runtime Candidate Disposition

External observation:

```text
RuntimeCandidateDisposition
├── ACCEPTED
├── REJECTED_STALE
├── REJECTED_CANCELED
├── REJECTED_DUPLICATE
├── REJECTED_INVALID
├── REJECTED_AUTHORITY
└── REJECTED_RUNTIME_FAILURE
```

This is not Translation Candidate state.

---

# 68. Candidate Ownership Boundary

```text
ASSEMBLING
VALIDATING
VALID
    → Translation producer ownership

SUBMITTED_TO_RUNTIME
    → transfer pending

ACCEPTED
    → Artifact Store ownership

REJECTED_*
    → cleanup
```

---

# 69. Translation Artifact Has No Execution State Machine

Published:

```text
TranslationArtifact
```

is immutable semantic data.

It does not transition:

```text
AVAILABLE
    → NON_AUTHORITATIVE
    → INVALIDATED
```

inside Translation.

External systems may track:

* usability
* retention
* compatibility
* active selection
* administrative invalidation

without mutating the Artifact.

---

# 70. No TranslationResult State Machine

Removed legacy:

```text
ASSEMBLING

PARTIAL

FINALIZING

AVAILABLE

AVAILABLE_WITH_WARNINGS

NON_AUTHORITATIVE

INVALIDATED
```

Replacement:

```text
CandidateValidationState

TranslationCompleteness

Warnings

RuntimeCandidateDisposition
```

---

# 71. Why `AVAILABLE` Is Removed

A Translation Candidate being valid does not determine its authority.

```text
Candidate VALID
    ↓
Runtime ACCEPT
    ↓
Artifact Store publishes
```

Only after this does a published TranslationArtifact exist.

---

# 72. Why `NON_AUTHORITATIVE` Is Removed

Authority belongs to Runtime/application revision semantics.

A TranslationArtifact may remain semantically valid even if it is no longer active/current.

Therefore:

```text
not current
    ≠
invalid artifact
```

---

# 73. Translation Variant State

Immutable Translation Variant does not require lifecycle states:

```text
CREATED
AVAILABLE
ACTIVE
INACTIVE
INVALIDATED
```

Variant is semantic identity/provenance.

Example:

```text
TranslationArtifact A
    variant = NATURAL

TranslationArtifact B
    variant = LITERAL
```

Both can coexist.

---

# 74. Active Variant Boundary

Current selection:

```text
ACTIVE / INACTIVE
```

belongs to:

* Reading Session
* User Preference
* Presentation/Application projection

not Translation execution.

---

# 75. Correction State Boundary

Correction creates:

```text
Old TranslationArtifact
        ↓
Correction
        ↓
New Candidate
        ↓
New TranslationArtifact Variant
```

Old Artifact remains immutable.

---

# 76. Cancellation Observation

Runtime owns canonical CancellationContext.

Translation may observe:

```text
NOT_REQUESTED

REQUESTED

ACKNOWLEDGED_BY_MODULE

PROVIDER_CANCEL_REQUESTED

LOCAL_WORK_STOPPING

LOCAL_WORK_STOPPED
```

These are observations only.

---

# 77. Cancellation Flow

```text
Current Translation Operation
        ↓
Cancellation Requested
        ↓
Stop Creating New Batches
        ↓
Request Provider Cancellation
        ↓
Ignore Unsafe Late Output
        ↓
Cleanup
        ↓
Return Runtime
```

Runtime decides:

```text
CANCELED

ABANDONED

FAILED
```

---

# 78. Cancellation Checkpoints

Check:

* before Plan building
* before Unit planning
* before context expansion
* before Batch construction
* before each provider request
* between provider requests
* before Candidate assembly
* before Candidate submission

---

# 79. Physical Cancellation

Provider cancellation may be:

```text
SUPPORTED

BEST_EFFORT

UNSUPPORTED
```

Physical cancellation result never determines Runtime authority.

---

# 80. Late Provider Result

Example:

```text
Provider running
      ↓
Runtime cancellation/authority loss
      ↓
provider finishes late
      ↓
output arrives
```

Translation must:

* avoid unsafe Candidate submission
* discard or clean output
* optionally record bounded diagnostics

Late completion does not restore authority.

---

# 81. Supersession Boundary

There is no Translation-owned:

```text
SUPERSEDED
```

state.

Runtime owns Revision/authority.

Example:

```text
SourceDocument A
    ↓
Translation running

SourceDocument B becomes current
    ↓
Runtime revokes authority for A
    ↓
late Candidate A
    ↓
REJECTED_STALE
```

---

# 82. Retranslation

Changing:

* target language
* Translation Profile
* Knowledge Snapshot
* Context Identity
* semantic Provider constraints

creates new semantic Translation Intent and Runtime work.

Old Translation state is not mutated to `SUPERSEDED`.

---

# 83. Retry Boundary

No Translation state:

```text
RETRY_SCHEDULED
```

exists.

Translation may return:

```text
RetryHint
```

Runtime decides:

```text
new Attempt?
backoff?
provider fallback?
no retry?
```

---

# 84. Provider Fallback

Example:

```text
Attempt N
    ↓
Provider A failure
    ↓
Translation RetryHint:
ALTERNATIVE_PROVIDER
    ↓
Runtime chooses retry
    ↓
Attempt N+1
    ↓
Provider B
```

Translation does not reopen the old Attempt.

---

# 85. Batch Retry Semantics

A Batch object belongs to one Plan/Attempt-local execution context.

When retry semantics require different:

* provider constraints
* batch membership
* batch size
* context size

a new Batch identity should be constructed.

Do not reset:

```text
Batch INVALID
    → READY
```

---

# 86. Same Batch Semantic Retry

If architecture allows reusing identical Batch semantics in a new Runtime Attempt:

```text
BatchSemanticIdentity
```

may remain equivalent.

But Attempt-local Batch state is recreated.

No mutable long-lived Batch lifecycle is resumed.

---

# 87. Deadline Observation

Runtime owns deadline.

Translation observes:

```text
DeadlineAvailable

RemainingBudget

DeadlineExceeded
```

Possible local behavior:

* stop optional context expansion
* avoid starting new Batch
* request Provider cancellation
* assemble PARTIAL Candidate if allowed
* cleanup

Runtime decides terminal outcome.

---

# 88. Partial Candidate on Deadline

If:

```text
some Units VALID
+
PartialResultPolicy = ALLOW_PARTIAL
```

Translation may assemble:

```text
Completeness = PARTIAL
```

before returning.

This does not imply Runtime will accept it.

---

# 89. Streaming Observation

Provider token streaming state:

```text
ProviderExecutionObservation = STREAMING
```

does not mean Translation completeness changed.

Only structurally validated translated Units affect Candidate completeness.

---

# 90. Incremental Unit Completion

Translation may internally observe:

```text
TranslationUnitResultObservation
├── PENDING
├── OUTPUT_RECEIVED
├── VALID
├── VALID_WITH_WARNINGS
├── MISSING
└── INVALID
```

This is optional Attempt-local diagnostic state.

---

# 91. Unit Result State Is Not Persistent Business State

Do not persist a global registry of:

```text
TranslationUnitResultObservation
```

for core lifecycle.

Published Artifact ultimately carries semantic translated results.

---

# 92. Cleanup Observation

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

# 93. Cleanup Scope

May include:

* Provider handle release
* temporary source/context buffers
* Batch buffers
* streaming buffers
* output parser state
* Candidate assembly buffers
* Artifact leases

---

# 94. Runtime Disposition Observation

Translation may observe:

```text
AttemptDisposition
├── ACCEPTED
├── FAILED
├── CANCELED
├── ABANDONED
├── REJECTED_STALE
└── REJECTED_DUPLICATE
```

for:

* diagnostics
* cleanup
* late-result handling

It does not mutate disposition.

---

# 95. No QUEUED State

Translation does not own:

```text
QUEUED
```

Queue belongs to Runtime Work Queue/Scheduler.

---

# 96. No RUNNING Attempt State

Translation may have an Operation Phase or Provider Execution Observation.

It does not own:

```text
TranslationAttempt.RUNNING
```

Runtime Attempt does.

---

# 97. No FAILED Translation Job State

Translation returns:

```text
TranslationModuleError
```

Runtime chooses Attempt terminal state.

---

# 98. No COMPLETED Translation Job State

Translation local work may finish.

That only means:

```text
Translation execution returned
```

not:

```text
WorkItem completed
Artifact published
Reading Session updated
```

---

# 99. No CANCELLED Translation Job State

Cancellation is Runtime-owned.

Translation only cooperates.

---

# 100. No SUPERSEDED Translation Job State

Staleness/authority is Runtime-owned.

Translation Candidate may remain semantically valid but no longer relevant.

---

# 101. No INVALIDATED Translation Job State

Immutable Artifact/result should not be mutated into lifecycle invalidation.

Administrative validity belongs to Artifact/application policy.

---

# 102. Local Operation Phase

Recommended diagnostic phase:

```text
TranslationOperationPhase
├── NOT_STARTED
├── VALIDATING
├── BUILDING_PLAN
├── BUILDING_UNITS
├── BUILDING_CONTEXT
├── RESOLVING_TERMINOLOGY
├── BUILDING_BATCHES
├── EXECUTING_PROVIDER
├── NORMALIZING_OUTPUT
├── VALIDATING_OUTPUT
├── ASSEMBLING_CANDIDATE
├── VALIDATING_CANDIDATE
├── FINALIZING
└── FINISHED
```

---

# 103. Operation Phase Rule

Operation Phase is:

* Attempt-local
* diagnostic
* useful for cancellation checkpoints
* useful for latency metrics
* useful for error localization

It is not Runtime execution lifecycle.

---

# 104. FINISHED

```text
TranslationOperationPhase = FINISHED
```

means local Translation execution ended.

It does not mean:

```text
Runtime Attempt = SUCCEEDED
```

---

# 105. Invalid Availability Transitions

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

---

# 106. Invalid Plan Transitions

Forbidden:

```text
READY
    → BUILDING
```

```text
INVALID
    → READY
```

---

# 107. Invalid Unit Planning Transitions

Forbidden:

```text
READY
    → BUILDING_UNITS
```

```text
INVALID
    → READY
```

within same planning instance.

---

# 108. Invalid Batch Transitions

Forbidden:

```text
VALID
    → EXECUTION_REQUESTED
```

```text
INVALID
    → READY
```

```text
OUTPUT_RECEIVED
    → EXECUTION_REQUESTED
```

Retry requires new Attempt-local execution context or new Batch instance.

---

# 109. Invalid Candidate Transitions

Forbidden:

```text
VALID
    → ASSEMBLING
```

```text
INVALID
    → VALID
```

```text
SUBMITTED_TO_RUNTIME
    → VALIDATING
```

---

# 110. Invalid Transition Handling

When invalid transition occurs:

1. reject transition
2. preserve current state
3. record invariant violation
4. do not infer Runtime state
5. avoid duplicate Provider execution
6. cleanup duplicate resources
7. return normalized Translation error when necessary

---

# 111. Concurrency Rules

1. One Runtime Attempt owns one Translation execution context.
2. One active immutable Translation Plan per Attempt.
3. Unit planning transitions logically serialized.
4. Multiple independent Batches may execute concurrently.
5. Batch completion order does not determine semantic Unit order.
6. Candidate assembly is logically serialized.
7. Candidate submission occurs at most once.
8. Provider request identity remains unique.
9. Cancellation may race with Provider response.
10. Runtime authority always wins against late Translation output.
11. Cleanup is idempotent.
12. Provider limits must be respected.

---

# 112. Safe Parallelism

Usually safe:

* independent Batch execution
* context preprocessing
* terminology lookup
* output validation per Batch
* usage aggregation

Needs deterministic coordination:

* Translation Unit construction
* final Unit sequence
* Candidate assembly
* missing Unit calculation
* source alignment merge
* variant identity

---

# 113. Concurrent Batches

Example:

```text
Batch A
    ProviderExecution = RUNNING

Batch B
    ProviderExecution = OUTPUT_RECEIVED

Batch C
    State = VALID
```

Runtime Attempt remains the only canonical execution lifecycle.

---

# 114. Provider Limit Concurrency

Concurrency must not bypass:

* rate limit
* max connection
* local model capacity
* Provider Management lease budget
* privacy restrictions

---

# 115. Candidate Assembly Concurrency

Candidate must be assembled from a stable snapshot of:

```text
VALID Batch results
+
known missing Units
+
known failed Units
```

No concurrent mutation after Candidate validation starts.

---

# 116. Deterministic Planning

Equivalent:

```text
SourceDocument

Translation Intent

Translation Profile

Knowledge Snapshot

Context Snapshot

Provider Policy

Configuration Snapshot
```

should create semantically equivalent:

```text
Translation Plan

Translation Unit boundaries

Batch planning
```

subject to explicitly allowed provider/runtime variation.

---

# 117. Provider Nondeterminism

Provider output may differ between equivalent calls.

This does not invalidate deterministic planning.

Record differences through:

```text
ProviderProvenance

Candidate identity

Artifact identity
```

not mutable state.

---

# 118. Recovery

Translation active state is Attempt-local.

After crash do not restore directly:

```text
Translation Plan active state

Unit Planning state

Batch execution state

Provider execution observation

Candidate assembly state

Cleanup observation
```

Runtime decides whether to create a new Attempt.

---

# 119. Recovery Flow

```text
Runtime Attempt interrupted
        ↓
Runtime records interruption
        ↓
Retry policy evaluates
        ↓
new Attempt if allowed
        ↓
reacquire SourceDocument Artifact
        ↓
new Translation Plan
        ↓
new provider execution
```

---

# 120. Candidate Recovery

Unaccepted Candidate:

* is non-authoritative
* is not automatically resurrected
* should be cleaned if abandoned

Published TranslationArtifact recovery belongs to Artifact Store/Storage.

---

# 121. State Persistence

Core active Translation states are ephemeral.

Do not persist as canonical business lifecycle:

```text
Translation Plan active state

Unit Planning state

Batch state

Provider Execution observation

Candidate Validation state

Operation Phase
```

Persistent semantic output may include:

```text
TranslationArtifact
```

through Artifact Store/Storage.

---

# 122. No Event-Sourced Translation Job

Text Processing/Translation events must not be replayed to reconstruct:

```text
TranslationJob

TranslationAttempt

active Translation execution lifecycle
```

because these entities no longer exist as Translation-owned lifecycle.

---

# 123. Event Relationship

Optional observations may map to local state:

```text
Plan READY
    → translation.plan_created
```

```text
Batch READY
    → translation.batch_planned
```

```text
Provider OUTPUT_RECEIVED
    → translation.provider_output_received
```

```text
Candidate VALID
    → translation.candidate_validated
```

```text
Candidate SUBMITTED
    → translation.candidate_submitted
```

Events do not grant authority.

---

# 124. Events Must Not Create State Authority

Receiving:

```text
translation.candidate_validated
```

does not mean:

```text
Artifact published
```

Receiving:

```text
translation.provider_output_received
```

does not mean:

```text
Batch valid
```

---

# 125. Partial Result Event Semantics

If Translation emits:

```text
translation.partial_candidate_validated
```

it means:

```text
Candidate.Completeness = PARTIAL
and
Candidate satisfies Translation contract
```

It does not mean Presentation may display it.

Runtime/application policy still decides.

---

# 126. Variant Events

Translation may emit immutable variant creation facts.

Example:

```text
translation.variant_created
```

But it should not emit:

```text
variant_activated
variant_deactivated
```

unless active-selection ownership is explicitly moved into Translation.

---

# 127. Error Relationship

Any local state may produce:

```text
TranslationModuleError
```

Examples:

```text
BUILDING_PLAN
    → configuration error

BUILDING_UNITS
    → alignment error

EXECUTING_PROVIDER
    → provider failure

VALIDATING_OUTPUT
    → malformed output

VALIDATING_CANDIDATE
    → Candidate contract failure
```

Runtime maps these to execution disposition.

---

# 128. Retry Hint Relationship

Error may include:

```text
RetryHint
```

Examples:

```text
provider unavailable
    → ALTERNATIVE_PROVIDER
```

```text
batch too large
    → SMALLER_BATCH
```

```text
context too large
    → REDUCE_CONTEXT
```

No state transition to `RETRY_SCHEDULED`.

---

# 129. MVP Availability States

Required:

```text
UNINITIALIZED

INITIALIZING

AVAILABLE

DEGRADED

UNAVAILABLE

DRAINING

STOPPED
```

---

# 130. MVP Plan States

Required:

```text
NOT_CREATED

BUILDING

VALIDATING

READY

INVALID
```

---

# 131. MVP Unit Planning States

Required:

```text
NOT_STARTED

SELECTING_SOURCE

BUILDING_UNITS

VALIDATING_UNITS

READY

INVALID
```

---

# 132. MVP Batch States

Required:

```text
CREATED

READY

EXECUTION_REQUESTED

OUTPUT_RECEIVED

VALIDATING_OUTPUT

VALID

INVALID
```

This replaces legacy:

```text
CREATED
READY
RUNNING
VALIDATING
COMPLETED
FAILED
CANCELLED
SUPERSEDED
```

---

# 133. MVP Provider Observations

Recommended:

```text
NOT_REQUESTED

REQUESTED

RUNNING

OUTPUT_RECEIVED

ERROR_RECEIVED

CANCEL_REQUESTED

PHYSICALLY_FINISHED

UNKNOWN
```

`ACCEPTED` and `STREAMING` may be optional.

---

# 134. MVP Candidate States

Required:

```text
NOT_CREATED

ASSEMBLING

VALIDATING

VALID

INVALID

SUBMITTED_TO_RUNTIME
```

---

# 135. MVP Completeness

Required:

```text
COMPLETE

PARTIAL

EMPTY_VALID

UNKNOWN
```

---

# 136. MVP Operation Flow

```text
Build Translation Plan
        ↓
Build Translation Units
        ↓
Build Context
        ↓
Resolve Terminology
        ↓
Build Batches
        ↓
Execute Providers
        ↓
Validate Outputs
        ↓
Assemble Candidate
        ↓
Validate Candidate
        ↓
Submit Runtime
        ↓
Cleanup
```

No second execution lifecycle is created.

---

# 137. Removed Legacy Job States

Removed:

```text
CREATED

QUEUED

RUNNING

PARTIALLY_COMPLETED

RETRY_SCHEDULED

CANCELLATION_REQUESTED

COMPLETED

COMPLETED_WITH_WARNINGS

FAILED

CANCELLED

SUPERSEDED

INVALIDATED
```

as `TranslationJobState`.

Reasons:

* WorkItem lifecycle belongs to Runtime
* retry belongs to Runtime
* cancellation belongs to Runtime
* authority belongs to Runtime
* invalidation belongs to Artifact/application policy
* completeness belongs to Candidate metadata

---

# 138. Removed Translation Attempt States

Removed:

```text
TranslationAttempt.CREATED

PREPARING

RUNNING

PARTIALLY_COMPLETED

COMPLETED

FAILED

CANCELLED

SUPERSEDED
```

Runtime Attempt replaces them.

---

# 139. Removed Translation Result States

Removed:

```text
ASSEMBLING

PARTIAL

FINALIZING

AVAILABLE

AVAILABLE_WITH_WARNINGS

NON_AUTHORITATIVE

INVALIDATED
```

Replacement responsibilities:

```text
ASSEMBLING / FINALIZING
    → Candidate local phase

PARTIAL
    → TranslationCompleteness

AVAILABLE
    → published Artifact existence

AVAILABLE_WITH_WARNINGS
    → Artifact warnings

NON_AUTHORITATIVE
    → Runtime/application authority

INVALIDATED
    → Artifact/application policy
```

---

# 140. Removed Variant Lifecycle

Removed:

```text
CREATED

AVAILABLE

ACTIVE

INACTIVE

INVALIDATED
```

from Translation execution.

Translation Variant is immutable semantic identity.

Reading Session/application owns selection.

---

# 141. Removed Retry State Rules

Legacy model:

```text
Attempt FAILED
    ↓
Job RETRY_SCHEDULED
    ↓
Attempt CREATED
```

Current:

```text
TranslationModuleError
    +
RetryHint
        ↓
Runtime Retry Policy
        ↓
new Runtime Attempt
```

---

# 142. Removed Progressive Publication State Machine

Legacy:

```text
RUNNING
    ↓
PARTIALLY_COMPLETED
    ↓
progressive publication
```

Current:

```text
Validated TranslatedUnits
        ↓
Candidate
Completeness = PARTIAL
        ↓
Runtime
        ↓
Artifact policy
```

---

# 143. Removed Stale Result Transition

Legacy Translation checked:

```text
Job / Attempt / Revision / active replacement
```

and moved output to `NON_AUTHORITATIVE`.

Current:

```text
Candidate
    ↓
Runtime authority check
    ↓
ACCEPT / REJECT_STALE
```

---

# 144. Removed Durable Job State Persistence

Legacy required durable state/event consistency for TranslationJob/Result/Variant.

Current state persistence authority lives with:

```text
Runtime
    → execution lifecycle

Artifact Store
    → published Artifact

Reading Session
    → current selection
```

Translation-local state remains ephemeral.

---

# 145. State Invariants — Availability

1. AVAILABLE means required Translation components ready.
2. DRAINING accepts no new work.
3. STOPPED has no active Translation execution.
4. DEGRADED cannot violate hard privacy constraints.
5. UNAVAILABLE cannot create executable Plan.

---

# 146. State Invariants — Plan

1. One active Plan per Runtime Attempt.
2. READY Plan immutable.
3. INVALID Plan never executes.
4. Target language fixed.
5. Translation Profile fixed.
6. Source semantic identity fixed.
7. Knowledge/Context identity fixed when material.
8. Privacy constraints cannot weaken.
9. Plan has no Runtime lifecycle state.
10. Plan cannot publish.

---

# 147. State Invariants — Translation Units

1. Unit IDs unique.
2. Every Unit maps to SourceBlock evidence.
3. Unit order remains deterministic.
4. Context-only content is not a TranslationUnit target.
5. Units become immutable after READY.
6. SourceBlock splitting preserves range lineage.
7. SourceBlock merging preserves all refs.
8. No source content silently omitted.

---

# 148. State Invariants — Batch

1. Batch contains at least one TranslationUnit.
2. Unit IDs not duplicated in one Batch.
3. Batch target language consistent.
4. Batch membership immutable after READY.
5. OUTPUT_RECEIVED is not VALID.
6. Provider response receipt is not success.
7. VALID requires output validation.
8. INVALID does not transition back to READY.
9. Batch state is Attempt-local.
10. Batch identity is not source alignment identity.

---

# 149. State Invariants — Provider Observation

1. Provider observation is best effort.
2. Provider state may be UNKNOWN.
3. Translation correctness does not require provider RUNNING observation.
4. PHYSICALLY_FINISHED does not imply Candidate success.
5. CANCEL_REQUESTED does not imply physical cancellation succeeded.
6. Provider late output never restores Runtime authority.

---

# 150. State Invariants — Candidate

1. One Candidate has one validation state.
2. Candidate ID unique.
3. Candidate immutable after VALID.
4. INVALID Candidate cannot be validly submitted.
5. SUBMITTED Candidate cannot be mutated.
6. Candidate maps all TranslatedUnits to TranslationUnits.
7. Candidate preserves SourceBlock lineage.
8. Candidate contains no credentials.
9. Candidate contains no Runtime terminal state.
10. Candidate submission does not equal publication.

---

# 151. State Invariants — Completeness

1. COMPLETE requires all required Units accounted for.
2. PARTIAL has explicit missing/failed Units.
3. EMPTY_VALID is valid.
4. UNKNOWN remains explicit.
5. Completeness does not determine Runtime outcome.
6. Warning count does not automatically change completeness.

---

# 152. External Ownership Invariants

1. Runtime owns WorkItem.
2. Runtime owns Attempt.
3. Runtime owns queueing.
4. Runtime owns retry.
5. Runtime owns cancellation.
6. Runtime owns deadline.
7. Runtime owns stale-result authority.
8. Artifact Store owns publication.
9. Provider Management owns Provider lifecycle.
10. Reading Session/application owns active variant selection.
11. Storage owns durable persistence.
12. Translation maintains no parallel lifecycle registry.

---

# 153. Testing — Availability

Test:

* initialization success
* degraded provider availability
* no eligible provider
* DRAINING rejects new work
* STOPPED clears local resources
* restart requires INITIALIZING

---

# 154. Testing — Translation Plan

Test:

* valid Plan
* incompatible SourceDocument
* invalid Translation Profile
* missing target language
* impossible Provider Policy
* Local-only with no local provider
* required Knowledge missing
* invalid terminology constraints
* READY immutable

---

# 155. Testing — Unit Planning

Test:

* 1 SourceBlock → 1 Unit
* N SourceBlocks → 1 Unit
* 1 SourceBlock → N Units
* invalid SourceBlock reference
* duplicate Unit ID
* source order
* context-only exclusion
* traceability
* no silent omissions

---

# 156. Testing — Batch

Test:

* single Unit Batch
* multiple Unit Batch
* duplicate Unit
* oversized Batch
* output received then VALID
* output received then INVALID
* invalid provider Unit IDs
* wrong target language
* locked terminology violation
* batch immutable after READY

---

# 157. Testing — Provider Observation

Test:

```text
REQUESTED
    → OUTPUT_RECEIVED
without RUNNING
```

```text
RUNNING
    → CANCEL_REQUESTED
    → PHYSICALLY_FINISHED
```

```text
RUNNING
    → ERROR_RECEIVED
```

```text
UNKNOWN
    → OUTPUT_RECEIVED
```

---

# 158. Testing — Partial Translation

Test:

```text
10 TranslationUnits

7 VALID
2 FAILED
1 MISSING
```

with:

```text
Completeness = PARTIAL
```

and explicit failed/missing IDs.

---

# 159. Testing — EMPTY_VALID

Test:

```text
no translatable SourceBlocks
```

produces valid Candidate:

```text
Completeness = EMPTY_VALID
```

not ModuleError.

---

# 160. Testing — Candidate

Test:

* COMPLETE Candidate
* PARTIAL Candidate
* EMPTY_VALID Candidate
* duplicate TranslatedUnit
* missing TranslationUnit ref
* missing SourceBlock lineage
* provider credential leakage
* missing Compatibility metadata
* missing Traceability metadata
* immutable after VALID
* submit once

---

# 161. Testing — Runtime Boundary

Test:

```text
Candidate VALID
    ↓
Runtime accepts
```

```text
Candidate VALID
    ↓
Runtime rejects stale
```

```text
Candidate VALID
    ↓
Runtime rejects canceled
```

```text
Module error
    ↓
RetryHint
    ↓
Runtime creates new Attempt
```

---

# 162. Testing — Cancellation

Test cancellation:

* before Plan
* during Unit planning
* before Provider request
* during Provider execution
* during output validation
* before Candidate submission
* after Candidate submission

Verify Translation never owns final cancellation disposition.

---

# 163. Testing — Late Provider Output

Test:

```text
provider request active
    ↓
Runtime authority revoked
    ↓
provider response arrives
```

Verify:

* no authority restoration
* no unsafe Candidate publication
* cleanup succeeds

---

# 164. Testing — Variants

Test:

* Natural Artifact
* Literal Artifact
* corrected Artifact
* alternative-provider Artifact
* parent lineage
* no mutable ACTIVE/INACTIVE state inside Translation Artifact

---

# 165. Property Tests

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
Batch VALID requires output validation
```

```text
all TranslatedUnits
map to TranslationUnits
```

```text
all TranslationUnits
map to SourceBlocks
```

```text
PARTIAL Candidate
lists all missing/failed Units
```

```text
Provider physical completion
does not imply Runtime success
```

```text
Translation never changes
Runtime Attempt state
```

---

# 166. Recommended MVP Decisions

```text
Keep Module Availability explicit.

Keep Translation Plan explicit.

Keep Translation Unit planning explicit.

Keep Translation Batch as semantic/provider planning state.

Keep Provider Execution as observation only.

Keep Candidate validation explicit.

Keep Translation completeness as metadata.

Remove TranslationJob lifecycle.

Remove TranslationAttempt lifecycle.

Remove TranslationResult lifecycle.

Remove mutable TranslationRevision.

Remove ACTIVE/INACTIVE variant lifecycle.

Remove RETRY_SCHEDULED.

Remove CANCELLATION_REQUESTED as Translation state.

Remove SUPERSEDED as Translation state.

Remove publication lifecycle.

Runtime owns every execution terminal outcome.

Artifact Store owns accepted Artifact lifecycle.

Reading Session/application owns active variant selection.
```

---

# 167. Related Documents

```text
02-modules/translation/README.md
02-modules/translation/MODULE.md
02-modules/translation/CONTRACT.md
02-modules/translation/EVENTS.md
02-modules/translation/ERRORS.md

02-modules/text-processing/README.md
02-modules/text-processing/CONTRACT.md

02-modules/provider-management/
02-modules/knowledge/
02-modules/reading-session/
02-modules/presentation/

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

03-infrastructure/artifact-store/
03-infrastructure/resource-manager/
```

---

# 168. Summary

Translation state model now focuses only on Translation-owned semantic/local state:

```text
Module Availability
        ↓
Translation Plan
        ↓
Translation Unit Planning
        ↓
Translation Batch
        ↓
Provider Execution Observation
        ↓
Provider Output Validation
        ↓
Candidate Validation
        ↓
Submitted to Runtime
```

Semantic data flow:

```text
SourceDocumentArtifact
        ↓
TranslationUnit[]
        ↓
TranslationBatch[]
        ↓
TranslatedUnit[]
        ↓
CandidateTranslationArtifact
```

Runtime owns:

```text
WorkItem

Attempt

Queue

Scheduling

Retry

Cancellation

Deadline

Authority

Terminal Outcome
```

Provider Management owns:

```text
Provider lifecycle

Provider health

Provider availability

Credentials

Reusable provider resources
```

Artifact Store owns:

```text
accepted TranslationArtifact lifecycle
```

Reading Session/Application owns:

```text
active Translation variant selection
```

Core rule:

```text
Translation owns
how source content becomes translated semantic output.

Runtime owns
whether execution still matters.

Provider Management owns
provider resources and lifecycle.

Artifact Store owns
what becomes a published Artifact.

Reading Session owns
what the reader is currently using.
```
