# Translation Module Events

> **Project:** CRAI
> **Module:** Translation
> **Path:** `02-modules/translation/EVENTS.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `STATES.md`

---

# 1. Purpose

Tài liệu này định nghĩa event boundary của Translation Module.

Translation events mô tả các fact/observation mà Translation thực sự sở hữu:

* module availability
* Translation Plan creation
* Translation Unit planning
* Translation Batch planning
* provider execution observations
* provider output observations
* translated-unit validation
* partial semantic progress
* Candidate validation
* Candidate submission
* immutable Translation Variant creation
* correction-derived Candidate creation
* Translation warnings/errors
* diagnostics and telemetry integration

Translation events không định nghĩa canonical lifecycle của:

* Runtime WorkItem
* Runtime Attempt
* Queue
* Scheduler
* retry
* cancellation
* supersession
* deadline
* terminal execution outcome
* Artifact publication
* Artifact retention
* active Reading Session variant

Core rule:

```text
Translation events describe
what Translation planned,
observed,
validated,
or produced.

They do not decide
whether the Runtime Attempt succeeded,
whether the work is still authoritative,
or whether a Translation Artifact is published.
```

---

# 2. Event Ownership

Translation may publish facts about:

```text
Module Availability

Translation Plan

Translation Units

Translation Batches

Provider Execution Observations

Provider Output Validation

Translated Units

Translation Completeness

Candidate Translation Artifacts

Translation Variant Creation

Translation Corrections

Translation Warnings

Translation Module Errors
```

Translation must not publish canonical lifecycle events declaring:

```text
WorkItem queued

Attempt started

Attempt completed

Attempt failed

Attempt canceled

Attempt superseded

Retry scheduled

Runtime deadline expired

Artifact published

Variant activated for Reading Session
```

---

# 3. Removed Legacy Event Families

The following legacy families are removed from Translation ownership:

```text
Translation Job Events

Translation Attempt Events

Translation Result Lifecycle Events

Retry Lifecycle Events

Cancellation Lifecycle Events

Supersession Events

Publication Lifecycle Events

Active Variant Lifecycle Events
```

Legacy examples removed:

```text
translation.job.created

translation.job.queued

translation.job.started

translation.attempt.started

translation.attempt.completed

translation.attempt.failed

translation.completed

translation.completed-with-warnings

translation.failed

translation.retry.scheduled

translation.cancellation.requested

translation.cancelled

translation.superseded

translation.invalidated

translation.variant.activated
```

These represented Runtime/application authority rather than Translation semantics.

---

# 4. Event Principles

## 4.1 Events Represent Facts

Event names describe observations/facts that already occurred.

Correct:

```text
translation.plan.created

translation.batch.planned

translation.provider.output-received

translation.candidate.validated

translation.candidate.submitted
```

Not commands:

```text
start_translation

retry_translation

cancel_translation

translate_document
```

---

# 5. Events Are Immutable

Published event instances are immutable.

Redelivery:

```text
same semantic event
    → same EventId
```

New fact:

```text
new EventId
```

---

# 6. Provider Neutrality

Public events must not expose:

* provider-native request
* provider-native response
* SDK objects
* raw prompt
* API key
* access token
* Authorization header
* secret model configuration

Normalized provider metadata may be included when useful.

---

# 7. Source and Translation Content Minimization

Normal events should not contain:

```text
full source text

full translated text

full SourceDocument

full TranslationArtifact

provider prompt

provider raw response

Knowledge database content
```

Prefer:

* IDs
* references
* counts
* warning codes
* error codes
* completeness
* durations
* usage summaries

---

# 8. Events Are Not Full Snapshots

Events should reference large semantic objects.

Examples:

```text
SourceDocumentArtifactRef

CandidateArtifactId

TranslationArtifactRef

KnowledgeSnapshotRef

ContextSnapshotRef
```

---

# 9. At-Least-Once Delivery

Consumers should assume:

```text
AtLeastOnce
```

unless Event Bus guarantees stronger semantics.

Consumers must tolerate:

* duplicates
* delayed delivery
* out-of-order delivery
* missing optional diagnostics

---

# 10. Event Ordering

Global ordering is not required.

Ordering may be meaningful within:

```text
AttemptId
```

or:

```text
TranslationPlanId
```

or:

```text
CandidateArtifactId
```

Semantic Translation Unit order must never derive from event arrival order.

---

# 11. Event Envelope

Translation uses shared CRAI Event Envelope.

Conceptually:

```text
EventEnvelope
├── EventId
├── EventName
├── EventVersion
├── OccurredAt
├── PublishedAt?
├── Producer
├── Subject
├── Correlation
├── Causation?
├── Sequence?
├── Privacy
├── Payload
└── Extensions?
```

Event Bus architecture remains authoritative for transport semantics.

---

# 12. Producer

Translation-produced events use:

```text
Producer.Module = translation
```

Optional:

```text
InstanceId

ModuleVersion
```

---

# 13. Event Subject

Possible subjects:

```text
TranslationModule

TranslationPlan

TranslationBatch

TranslationUnit

ProviderExecution

CandidateTranslationArtifact

TranslationVariant
```

Do not use:

```text
TranslationJob
```

as core domain subject.

---

# 14. Correlation Identity

Attempt-local events should carry:

```text
RevisionId

WorkItemId

AttemptId
```

and when relevant:

```text
TraceId

SourceDocumentArtifactId

TranslationIntentId

TranslationPlanId

TranslationBatchId

TranslationUnitId

CandidateArtifactId
```

---

# 15. No TranslationAttemptId

Translation event contracts do not introduce:

```text
TranslationAttemptId
```

Runtime:

```text
AttemptId
```

is canonical.

---

# 16. Source Correlation

Events affecting translated semantics should preserve:

```text
SourceDocumentArtifactId

SourceDocumentContentIdentity

TranslationIntentId

TargetLanguage
```

when needed.

This supports traceability without recreating a Job lifecycle.

---

# 17. Candidate Correlation

Candidate events include:

```text
RevisionId

WorkItemId

AttemptId

SourceDocumentArtifactId

TranslationIntentId

CandidateArtifactId
```

Optional:

```text
TranslationVariantId

TraceId
```

---

# 18. Sequence

Optional event sequencing may use:

```text
translation:<AttemptId>
```

or another Event Bus-approved stream.

Sequence is diagnostic.

It is not source of Runtime authority.

---

# 19. Optional Event Principle

Most Translation internal events are optional observations.

Consumer correctness must not require reception of every:

```text
unit planned

batch planned

provider started

provider output received
```

event.

The synchronous/local execution contract remains primary.

---

# 20. Event Categories

Recommended categories:

```text
1. Module Availability Events

2. Planning Events

3. Batch and Provider Observation Events

4. Translation Output Observation Events

5. Candidate Events

6. Variant / Correction Facts

7. Error / Warning Observations
```

---

# 21. Module Availability Events

Recommended:

```text
translation.module.available

translation.module.degraded

translation.module.unavailable

translation.module.draining

translation.module.stopped
```

These describe module availability only.

---

# 22. `translation.module.available`

Payload:

```text
TranslationModuleAvailable
├── SupportedContractVersions[]
├── SupportedProfiles[]
├── SupportedLanguages?
├── ConfigurationSnapshotId?
├── AvailableCapabilities[]
└── AvailableAt
```

---

# 23. `translation.module.degraded`

Payload:

```text
TranslationModuleDegraded
├── ReasonCodes[]
├── AvailableCapabilities[]
├── UnavailableCapabilities[]
├── EligibleProviderClasses?
├── ConfigurationSnapshotId?
└── DegradedAt
```

Possible reasons:

```text
OPTIONAL_CONTEXT_UNAVAILABLE

KNOWLEDGE_UNAVAILABLE

STREAMING_UNAVAILABLE

PREFERRED_PROVIDER_UNAVAILABLE

REMOTE_PROVIDER_UNAVAILABLE

LOCAL_PROVIDER_UNAVAILABLE

OPTIONAL_GLOSSARY_UNAVAILABLE
```

---

# 24. `translation.module.unavailable`

Payload:

```text
TranslationModuleUnavailable
├── ReasonCodes[]
├── RetryHint?
└── UnavailableAt
```

Runtime decides retry.

---

# 25. `translation.module.draining`

Payload:

```text
TranslationModuleDraining
├── ActiveAttemptCount?
├── ActiveProviderRequestCount?
├── Reason?
└── StartedAt
```

Does not cancel Runtime Attempts.

---

# 26. `translation.module.stopped`

Payload:

```text
TranslationModuleStopped
└── StoppedAt
```

---

# 27. Planning Events

Recommended optional planning events:

```text
translation.plan.created

translation.units.planned

translation.context.resolved

translation.terminology.resolved

translation.batch.planned
```

For MVP only some are necessary.

---

# 28. `translation.plan.created`

Published when:

```text
TranslationPlanState = READY
```

Payload:

```text
TranslationPlanCreated
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── TranslationIntentId
├── SourceDocumentArtifactId
├── TargetLanguage
├── TranslationProfileId
├── TranslationProfileVersion
├── KnowledgeSnapshotId?
├── ContextSnapshotId?
├── ConfigurationSnapshotId
├── PartialResultMode
├── PlannedUnitCount?
├── PlannedBatchCount?
└── CreatedAt
```

---

# 29. Plan Event Restrictions

Do not include:

* complete Translation Plan
* source text
* translated text
* provider credentials
* Runtime priority state
* retry budget
* cancellation state

---

# 30. `translation.units.planned`

Optional observation after Translation Unit planning succeeds.

Payload:

```text
TranslationUnitsPlanned
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── SourceDocumentArtifactId
├── TranslationUnitCount
├── SelectedSourceBlockCount
├── MergeCount?
├── SplitCount?
├── WarningCodes[]
└── PlannedAt
```

---

# 31. Translation Unit Event Granularity

Do not emit one public event per TranslationUnit by default.

Avoid:

```text
translation.unit.created
```

for thousands of Units unless a concrete consumer requires it.

Aggregate event preferred.

---

# 32. `translation.context.resolved`

Optional.

Payload:

```text
TranslationContextResolved
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── ContextSnapshotId?
├── ContextEntryCount
├── MissingOptionalContextCount
├── WarningCodes[]
└── ResolvedAt
```

No context content.

---

# 33. `translation.terminology.resolved`

Optional.

Payload:

```text
TranslationTerminologyResolved
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── KnowledgeSnapshotId?
├── ConstraintCount
├── LockedConstraintCount
├── ConflictCount
├── WarningCodes[]
└── ResolvedAt
```

No source/target term values in normal events.

---

# 34. `translation.batch.planned`

Optional.

Payload:

```text
TranslationBatchPlanned
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── TranslationBatchId
├── BatchSequence
├── TranslationUnitCount
├── EstimatedCharacters?
├── EstimatedTokens?
├── ProviderRequirementsSummary
└── PlannedAt
```

---

# 35. Batch Planned Semantics

This means:

```text
TranslationBatchState = READY
```

It does not mean:

```text
Runtime scheduled it

provider accepted it

provider is running
```

---

# 36. Provider Observation Events

Possible optional events:

```text
translation.provider.execution-requested

translation.provider.execution-observed

translation.provider.output-received

translation.provider.error-observed
```

They are observational.

---

# 37. `translation.provider.execution-requested`

Payload:

```text
TranslationProviderExecutionRequested
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── TranslationBatchId
├── ProviderId?
├── ProviderClass?
├── LocalExecution?
├── StreamingRequested?
└── RequestedAt
```

---

# 38. Execution Requested Is Not Started

```text
execution-requested
    ≠
provider running
```

Some providers offer no reliable running-state observation.

---

# 39. `translation.provider.execution-observed`

Optional diagnostic event.

Payload:

```text
TranslationProviderExecutionObserved
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationBatchId
├── ProviderId?
├── Observation
├── ProviderRequestId?
├── LocalExecution?
└── ObservedAt
```

Observation:

```text
ACCEPTED

RUNNING

STREAMING

CANCEL_REQUESTED

PHYSICALLY_FINISHED

UNKNOWN
```

---

# 40. Provider Observation Is Best Effort

Correctness must tolerate:

```text
execution-requested
    ↓
output-received
```

without a `RUNNING` event.

---

# 41. `translation.provider.output-received`

Published optionally after Adapter creates provider-neutral output.

Payload:

```text
TranslationProviderOutputReceived
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationBatchId
├── ProviderId?
├── ReturnedUnitCount
├── ProviderWarningCount?
├── UsageSummary?
├── LatencyMs?
└── ReceivedAt
```

---

# 42. Output Received Is Not Valid

Critical:

```text
provider.output-received
    ≠
TranslationBatch VALID
```

Output still requires validation.

---

# 43. `translation.provider.error-observed`

Optional diagnostic event.

Payload:

```text
TranslationProviderErrorObserved
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationBatchId
├── ProviderId?
├── ErrorCode
├── ErrorCategory
├── Retryability?
├── ProviderRequestId?
└── OccurredAt
```

No raw provider response.

---

# 44. No `TranslationAttemptFailed`

Provider/batch failure does not create a Translation Attempt lifecycle event.

Runtime Attempt remains authoritative.

---

# 45. Batch Validation Events

Recommended:

```text
translation.batch.validated

translation.batch.invalid
```

The second is optional diagnostic.

---

# 46. `translation.batch.validated`

Published after provider output has passed required Translation validation.

Payload:

```text
TranslationBatchValidated
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── TranslationBatchId
├── TranslationUnitCount
├── ValidatedUnitCount
├── WarningCount
├── ProviderId?
├── UsageSummary?
└── ValidatedAt
```

---

# 47. Batch Validation Requirement

`translation.batch.validated` must never be emitted merely because:

```text
HTTP 200

provider request completed

structured JSON parsed
```

Alignment/contract validation must pass.

This preserves one of the strongest principles from the previous model.

---

# 48. `translation.batch.invalid`

Optional diagnostic event.

Payload:

```text
TranslationBatchInvalid
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationBatchId
├── ValidationCode
├── AffectedTranslationUnitIds[]
├── RetryHint?
└── InvalidatedAt
```

Keep Unit list bounded or use a reference/count when large.

---

# 49. Translated Unit Observation

For low-latency progressive processing, Translation may expose:

```text
translation.units.validated
```

This replaces legacy `TranslationSegmentCompleted`.

---

# 50. `translation.units.validated`

Payload:

```text
TranslationUnitsValidated
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationPlanId
├── TranslationBatchId?
├── CandidateArtifactId?
├── UnitSummaries[]
├── CompletenessObservation?
└── ValidatedAt
```

---

# 51. Unit Summary

```text
TranslationUnitValidationSummary
├── TranslationUnitId
├── TranslatedUnitId
├── SourceBlockRefs[]
├── SourceSequence
├── Completion
└── WarningCodes[]
```

Do not embed translated text by default.

---

# 52. Why `TranslationSegmentCompleted` Is Removed

Legacy:

```text
PreparedSegment
    ↓
TranslatedSegment
```

no longer exists.

Current:

```text
SourceBlock[]
    ↓
TranslationUnit
    ↓
TranslatedUnit
```

Therefore event naming follows Translation Unit semantics.

---

# 53. Progressive Translation Events

Progressive processing may emit:

```text
translation.partial-candidate.validated
```

instead of publishing revisioned Translation Results directly.

---

# 54. `translation.partial-candidate.validated`

Optional.

Published when:

```text
CandidateValidationState = VALID
and
Completeness = PARTIAL
```

Payload:

```text
TranslationPartialCandidateValidated
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId
├── TranslationIntentId
├── SourceDocumentArtifactId
├── TargetLanguage
├── CompletedTranslationUnitCount
├── MissingTranslationUnitCount
├── FailedTranslationUnitCount
├── MissingTranslationUnitIds?
├── FailedTranslationUnitIds?
├── WarningSummary
└── ValidatedAt
```

---

# 55. Partial Candidate Is Not Published

```text
partial-candidate.validated
    ≠
Presentation may display
```

Runtime/application policy decides acceptance.

---

# 56. Partial Event Granularity

For large documents:

prefer:

```text
counts
+
bounded Unit ID subsets
+
Candidate reference
```

instead of complete lists.

---

# 57. Translation Candidate Events

Core events:

```text
translation.candidate.validated

translation.candidate.invalid

translation.candidate.submitted
```

---

# 58. `translation.candidate.validated`

Published when:

```text
TranslationCandidateValidationState = VALID
```

Payload:

```text
TranslationCandidateValidated
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId
├── SourceDocumentArtifactId
├── TranslationIntentId
├── TranslationVariantId?
├── TargetLanguage
├── TranslationProfileId
├── Completeness
├── TranslationUnitCount
├── TranslatedUnitCount
├── MissingUnitCount
├── FailedUnitCount
├── WarningSummary
├── ProviderProvenanceSummary
├── CompatibilitySummary
└── ValidatedAt
```

---

# 59. Candidate Validated Meaning

Means:

```text
Candidate satisfies
Translation-owned semantic contract.
```

Does not mean:

```text
Runtime Attempt succeeded

Candidate is current

Candidate is authoritative

Translation Artifact published

Reading Session selected this variant
```

---

# 60. `translation.candidate.invalid`

Optional diagnostic event.

Payload:

```text
TranslationCandidateInvalid
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId?
├── ValidationCode
├── ValidationCategory
├── OperationPhase
├── RetryHint?
└── InvalidatedAt
```

---

# 61. Invalid Candidate Rule

```text
Candidate INVALID
    ↛
candidate.submitted
```

---

# 62. `translation.candidate.submitted`

Published when a VALID Candidate crosses Translation → Runtime boundary.

Payload:

```text
TranslationCandidateSubmitted
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId
├── SourceDocumentArtifactId
├── TranslationIntentId
├── TranslationVariantId?
├── TargetLanguage
├── Completeness
└── SubmittedAt
```

---

# 63. Submission Is Not Acceptance

```text
candidate.submitted
    ≠
Candidate accepted
```

Runtime may classify:

```text
ACCEPTED

REJECTED_STALE

REJECTED_CANCELED

REJECTED_DUPLICATE

REJECTED_INVALID

REJECTED_AUTHORITY

REJECTED_RUNTIME_FAILURE
```

These events, if exposed, belong to Runtime.

---

# 64. No `translation.completed`

The old terminal event:

```text
translation.completed
```

is removed as a Translation-owned lifecycle event.

Reason:

```text
Translation local completion
    ≠
Runtime Attempt success
    ≠
Artifact publication
```

---

# 65. No `translation.completed-with-warnings`

Warnings belong to Candidate/Artifact metadata.

Use:

```text
candidate.validated
    Completeness = COMPLETE
    WarningCount > 0
```

rather than creating a second terminal lifecycle.

---

# 66. No `translation.failed`

Translation returns:

```text
TranslationModuleError
```

Runtime decides whether Attempt/WorkItem failed.

---

# 67. No `translation.cancelled`

Cancellation belongs to Runtime.

Translation may emit optional diagnostic:

```text
translation.cancellation.observed
```

---

# 68. `translation.cancellation.observed`

Payload:

```text
TranslationCancellationObserved
├── RevisionId
├── WorkItemId
├── AttemptId
├── OperationPhase
├── ActiveProviderRequestCount?
└── ObservedAt
```

Does not mean Runtime committed `CANCELED`.

---

# 69. Provider Cancellation Event

Optional:

```text
translation.provider.cancellation-requested
```

Payload:

```text
TranslationProviderCancellationRequested
├── RevisionId
├── WorkItemId
├── AttemptId
├── TranslationBatchId
├── ProviderId?
└── RequestedAt
```

Physical outcome remains best effort.

---

# 70. No `translation.retry.scheduled`

Translation emits:

```text
RetryHint
```

Runtime owns scheduling.

Optional diagnostic fact may describe recommendation:

```text
translation.retry-hint.produced
```

but this should usually stay in logs/telemetry.

---

# 71. Provider Fallback Semantics

Old:

```text
TranslationProviderFallbackSelected
```

implied Translation selected the next Attempt/provider.

New architecture should distinguish:

```text
Translation recommends fallback
        ↓
Runtime / Provider resolution
        ↓
new Attempt
```

If a provider was actually used after fallback, provenance records it.

---

# 72. Optional Fallback Observation

Possible:

```text
translation.provider.fallback-observed
```

when an Attempt executes using a fallback provider.

Payload:

```text
TranslationProviderFallbackObserved
├── AttemptId
├── PreviousProviderId?
├── CurrentProviderId
├── ReasonCode
└── ObservedAt
```

Not required for correctness.

---

# 73. Supersession

Translation publishes no:

```text
translation.superseded
```

Runtime owns stale/revision authority.

Example:

```text
Candidate VALID
    ↓
Runtime sees newer Revision
    ↓
REJECTED_STALE
```

No Translation state mutation required.

---

# 74. Invalidation

Translation core does not own mutable:

```text
translation.invalidated
```

for published Artifacts.

Artifact/application policy may publish invalidation events if that architecture supports them.

Translation may publish:

```text
candidate.invalid
```

only during Candidate validation.

---

# 75. Variant Events

Translation owns immutable variant creation semantics.

Recommended:

```text
translation.variant.created
```

It does not own:

```text
translation.variant.activated

translation.variant.deactivated
```

for Reading Session selection.

---

# 76. `translation.variant.created`

Published when a valid immutable translation variant is represented by a Candidate/Artifact identity.

Payload:

```text
TranslationVariantCreated
├── TranslationVariantId
├── CandidateArtifactId?
├── TranslationArtifactRef?
├── SourceDocumentArtifactId
├── TranslationIntentId
├── ParentVariantId?
├── VariantType
├── TargetLanguage
├── TranslationProfileId
├── CreatedBy
└── CreatedAt
```

---

# 77. Variant Types

Examples:

```text
PROVIDER_GENERATED

RETRANSLATED

LITERAL

NATURAL

USER_CORRECTED

SYSTEM_CORRECTED

IMPORTED
```

---

# 78. Variant Created Does Not Mean Active

```text
variant.created
    ≠
active for Reading Session
```

Selection belongs externally.

---

# 79. Translation Corrections

Corrections may create immutable variant facts.

Possible events:

```text
translation.correction.recorded

translation.correction.variant-created
```

Avoid command-like:

```text
correction.submitted
```

as a required domain event if submission ownership belongs to application/API layer.

---

# 80. `translation.correction.recorded`

Optional.

Payload:

```text
TranslationCorrectionRecorded
├── BaseTranslationArtifactId
├── BaseVariantId
├── AffectedTranslationUnitIds[]
├── AffectedSourceBlockRefs[]
├── CorrectedBy
├── KnowledgeProposalRequested
└── RecordedAt
```

No corrected text by default.

---

# 81. `translation.correction.variant-created`

Published after a new immutable corrected Candidate/Artifact variant exists.

Payload:

```text
TranslationCorrectionVariantCreated
├── BaseVariantId
├── CorrectedVariantId
├── CandidateArtifactId?
├── TranslationArtifactRef?
├── AffectedTranslationUnitCount
├── KnowledgeProposalRef?
└── CreatedAt
```

---

# 82. Knowledge Boundary

Correction events do not imply:

```text
global Knowledge updated
```

Knowledge owns terminology persistence/review.

---

# 83. Cache Events

Translation should not publish core:

```text
translation.cache.result-reused
```

because Runtime Cache Policy owns reuse decisions.

Runtime/cache telemetry may expose reuse.

Translation only defines semantic compatibility.

---

# 84. Configuration Observation

Optional:

```text
translation.configuration.observed
```

Payload:

```text
TranslationConfigurationObserved
├── ConfigurationSnapshotId
├── PreviousSnapshotId?
├── Supported
├── RequiresRestart?
└── ObservedAt
```

An active Attempt continues using its immutable snapshot.

---

# 85. Events Consumed by Translation

Translation may observe external events from:

```text
Runtime

Artifact Store

Reading Session

Knowledge

Provider Management

Text Processing
```

Exact names belong to owning modules.

---

# 86. SourceDocument Availability

Translation may react when an accepted:

```text
SourceDocumentArtifact
```

becomes available.

But automatic translation initiation belongs to orchestration/Runtime.

Translation must not turn upstream event directly into bypassed execution.

---

# 87. SourceDocument Revision Change

If source changes:

```text
Runtime
    → creates/revises WorkItem authority
```

Translation does not:

```text
mark old TranslationJob SUPERSEDED
```

Late Candidates are rejected by Runtime.

---

# 88. Reading Session Navigation

Reading Session navigation may affect:

* Runtime priority
* cancellation
* prefetch
* visible-content authority

Translation only observes resulting execution context.

It does not own Reading Session state transitions.

---

# 89. Knowledge Changes

Knowledge update does not mutate existing TranslationArtifacts.

New Translation Intent may reference new Knowledge Snapshot.

Existing variants remain immutable.

---

# 90. Provider Availability Changes

Provider Management owns provider availability events.

Translation may use updated availability when constructing future Plan/provider requirements.

Active immutable Plan must not silently change semantic policy mid-Attempt.

---

# 91. Event Ordering Within Concurrent Batches

Valid:

```text
Batch A request
Batch B request

Batch B output
Batch A output

Batch B validated
Batch A validated
```

Final ordering uses:

```text
TranslationUnit.SourceSequence
```

not event arrival.

---

# 92. No Terminal Event Ordering Rule

Legacy rule:

```text
exactly one:
Completed
Failed
Cancelled
Superseded
```

is removed from Translation events.

Runtime owns terminal events.

---

# 93. Late Provider Response

If provider output arrives after Runtime authority loss:

Translation may:

* record bounded diagnostic
* record usage
* cleanup provider resources

Translation must not assume authority is restored.

---

# 94. Event Delivery and Deduplication

Primary deduplication:

```text
EventId
```

Semantic keys when needed:

```text
AttemptId
+
EventName
+
TranslationBatchId?
+
CandidateArtifactId?
```

---

# 95. Candidate Submission Idempotency

For one Candidate:

```text
semantic submission count <= 1
```

Event redelivery may occur.

Duplicate event consumption must not create duplicate Artifact publication.

---

# 96. Event Loss

Loss of optional events must not change:

```text
Translation semantic output

Candidate validity

Runtime Attempt outcome
```

Core execution contract remains authoritative.

---

# 97. Event Publication Failure

Failure to publish optional:

```text
batch.planned

provider.output-received

units.validated
```

must not invalidate an otherwise valid Candidate.

---

# 98. Candidate Submission vs Candidate Event

Important:

```text
Submit Candidate to Runtime
```

is the correctness boundary.

```text
translation.candidate.submitted event
```

is observational.

Event publication failure must not trigger duplicate Candidate submission.

---

# 99. Event vs Telemetry

Use Event Bus when another module has a legitimate semantic reason to observe a fact.

Use Telemetry for:

* latency measurements
* provider token rate
* memory
* detailed retry diagnostics
* per-rule timings
* high-frequency streaming progress

---

# 100. Event vs Log

Good event:

```text
Candidate validated
```

Telemetry/log:

```text
provider emitted token 312
```

---

# 101. Progress Events

A generic:

```text
translation.progress.updated
```

should not be part of required Translation domain contract.

Runtime/UX progress can derive from:

* operation phase
* validated Unit counts
* Batch counts
* telemetry

If a progress projection is needed, it belongs to a read model/observability concern.

---

# 102. High-Frequency Event Rule

Do not emit:

* one event per provider token
* one event per character
* one event per prompt chunk
* one event per glossary lookup

Prefer phase/batch aggregate observations.

---

# 103. Privacy Metadata

Normal Translation events should indicate:

```text
contains_source_text = false

contains_translated_text = false

contains_provider_prompt = false

contains_credentials = false
```

according to shared privacy envelope.

---

# 104. Sensitive IDs

IDs may still be sensitive:

```text
SessionId

SourceDocumentArtifactId

TranslationArtifactId

KnowledgeSnapshotId
```

Use CRAI privacy classification.

---

# 105. Translated Text in Events

Default:

```text
do not embed translated text
```

For ultra-low-latency in-process delivery, a future private/non-persistent channel may carry bounded text if explicitly designed.

The public Event Bus contract must not depend on it.

---

# 106. Prompt Injection Safety

Provider/source output cannot control:

* EventName
* routing
* topic
* partition
* correlation IDs
* Provider ID metadata
* Runtime IDs
* Privacy classification

Trusted Translation code constructs metadata.

---

# 107. Error Observation Event

Optional:

```text
translation.operation-error.observed
```

Payload:

```text
TranslationOperationErrorObserved
├── RevisionId
├── WorkItemId
├── AttemptId
├── OperationPhase
├── ErrorCode
├── ErrorCategory
├── TranslationBatchId?
├── ProviderId?
├── Retryability?
└── OccurredAt
```

Diagnostic only.

---

# 108. Error Observation Is Not Failure Lifecycle

```text
operation-error.observed
    ≠
Runtime Attempt FAILED
```

Runtime evaluates the module result.

---

# 109. Warning Observation Event

Optional:

```text
translation.warning.recorded
```

Payload:

```text
TranslationWarningRecorded
├── RevisionId
├── WorkItemId
├── AttemptId
├── WarningCode
├── OperationPhase
├── TranslationUnitCount?
├── TranslationBatchId?
├── ProviderId?
└── RecordedAt
```

---

# 110. Warning Privacy

Do not include source/translated fragments in normal warning events.

Use:

```text
WarningCode

TranslationUnitId

SourceBlockRef
```

when necessary.

---

# 111. Event Versioning

Each event has independent:

```text
EventVersion
```

from:

```text
TranslationContractVersion

ModuleVersion

TranslationProfileVersion

ProviderVersion
```

---

# 112. Backward-Compatible Event Changes

May include:

* optional fields
* optional metrics
* new warning code
* new optional capability metadata
* additive provenance

---

# 113. Breaking Event Changes

Require major event version when:

* identifier ownership changes
* event semantic meaning changes
* required identity removed
* Candidate authority semantics change
* privacy guarantee weakens
* provider-native data becomes public

---

# 114. Unknown Fields

Consumers should:

* ignore unknown optional fields
* preserve when forwarding if appropriate
* handle unknown enums safely
* reject unsupported major version when required

---

# 115. Event Retention

Retention should be conservative for Translation events.

Consider:

* reading-history sensitivity
* provider usage metadata
* Session identifiers
* correction history
* Knowledge references
* privacy mode

Normal events should remain metadata-only.

---

# 116. MVP Required Events

Recommended minimum:

```text
translation.module.available

translation.module.degraded

translation.module.unavailable

translation.candidate.validated

translation.candidate.submitted
```

---

# 117. MVP Recommended Events

Useful:

```text
translation.plan.created

translation.batch.validated

translation.provider.error-observed

translation.partial-candidate.validated

translation.variant.created

translation.operation-error.observed
```

---

# 118. MVP Optional Events

Can defer:

```text
translation.units.planned

translation.context.resolved

translation.terminology.resolved

translation.batch.planned

translation.provider.execution-requested

translation.provider.execution-observed

translation.provider.output-received

translation.units.validated

translation.cancellation.observed

translation.provider.cancellation-requested

translation.correction.recorded

translation.correction.variant-created

translation.configuration.observed

translation.warning.recorded
```

---

# 119. Removed Legacy Events

Explicitly removed/re-owned:

```text
TranslationJobCreated
    → Runtime WorkItem creation

TranslationJobQueued
    → Runtime Queue

TranslationJobStarted
    → Runtime Attempt

TranslationProgressUpdated
    → Telemetry / progress projection

TranslationAttemptStarted
    → Runtime Attempt

TranslationAttemptCompleted
    → Runtime Attempt

TranslationAttemptFailed
    → Runtime Attempt

TranslationBatchStarted
    → Provider execution observation

TranslationBatchCompleted
    → translation.batch.validated

TranslationBatchFailed
    → provider/batch error observation

TranslationSegmentCompleted
    → translation.units.validated

TranslationSegmentsCompleted
    → translation.units.validated

TranslationPartialResultAvailable
    → translation.partial-candidate.validated

TranslationCompleted
    → Candidate + Runtime + Artifact Store

TranslationCompletedWithWarnings
    → Candidate warnings/completeness

TranslationFailed
    → Runtime terminal outcome

TranslationRetryScheduled
    → Runtime retry

TranslationCancellationRequested
    → Runtime cancellation

TranslationCancelled
    → Runtime terminal outcome

TranslationSuperseded
    → Runtime authority/stale handling

TranslationInvalidated
    → Artifact/application policy

TranslationVariantActivated
    → Reading Session/application state

TranslationVariantInvalidated
    → Artifact/application validity policy

TranslationCacheResultReused
    → Runtime Cache Policy
```

---

# 120. Canonical Event Flow

Successful semantic processing:

```text
Runtime Attempt
      ↓
Translation
      │
      ├── plan.created             [optional]
      │
      ├── units.planned            [optional]
      │
      ├── batch.planned            [optional]
      │
      ├── provider observations    [optional]
      │
      ├── batch.validated          [optional]
      │
      └── units.validated          [optional]
              ↓
      Assemble Candidate
              ↓
      candidate.validated
              ↓
      Submit Candidate to Runtime
              ↓
      candidate.submitted
              ↓
            Runtime
         ┌────┴────┐
         │         │
      ACCEPT     REJECT
         │
         ▼
    Artifact Store
         │
         ▼
 TranslationArtifact
```

---

# 121. Partial Candidate Flow

```text
Translation Units
    ├── VALID
    ├── VALID
    ├── FAILED
    └── MISSING
        ↓
Completeness = PARTIAL
        ↓
Candidate VALID
        ↓
partial-candidate.validated
        ↓
candidate.submitted
        ↓
Runtime policy
```

---

# 122. Provider Failure Flow

```text
Batch
    ↓
Provider request
    ↓
provider error
    ↓
provider.error-observed [optional]
    ↓
TranslationModuleError
    +
RetryHint
    ↓
Runtime
    ↓
Retry / Fail / Cancel
```

No Translation retry lifecycle event.

---

# 123. Cancellation Flow

```text
Runtime CancellationContext
        ↓
Translation observes
        ↓
cancellation.observed [optional]
        ↓
provider cancellation request [optional]
        ↓
cleanup
        ↓
return Runtime
```

No:

```text
translation.cancelled
```

terminal event.

---

# 124. Stale Candidate Flow

```text
Candidate VALID
        ↓
candidate.submitted
        ↓
Runtime detects newer Revision
        ↓
REJECTED_STALE
```

Translation does not publish `superseded`.

---

# 125. Variant Flow

```text
Candidate / Accepted Artifact
        ↓
Immutable Variant Identity
        ↓
translation.variant.created
```

Later:

```text
Reading Session
    ↓
selects variant
```

Translation does not publish activation as owner.

---

# 126. Correction Flow

```text
Existing TranslationArtifact
        ↓
Correction request
        ↓
Translation processing
        ↓
New corrected Candidate
        ↓
Runtime
        ↓
New TranslationArtifact Variant
        ↓
translation.correction.variant-created
```

Original remains immutable.

---

# 127. Consumer Guidance — Runtime

Runtime primarily consumes direct execution result, not Translation events.

Events may aid:

* observability
* distributed coordination
* diagnostics

Runtime authority must never rely solely on event arrival.

---

# 128. Consumer Guidance — Presentation

Presentation should primarily consume:

```text
accepted TranslationArtifact
```

through Reading Session/application authority.

It should not render directly because:

```text
translation.candidate.validated
```

was received.

---

# 129. Consumer Guidance — Reading Session

Reading Session should operate on accepted/published Translation Artifact references.

It may observe:

```text
translation.variant.created
```

but must independently validate compatibility/current source revision.

---

# 130. Consumer Guidance — Knowledge

Knowledge may observe correction-derived facts.

It must not automatically treat every user correction as global terminology truth.

---

# 131. Consumer Guidance — Provider Management

Provider Management should not infer provider health from one Translation module error.

It owns its own provider health/availability signals.

Translation may report normalized execution observations through approved telemetry.

---

# 132. Consumer Guidance — Observability

Observability may use:

```text
plan.created

batch.planned

provider.output-received

batch.validated

candidate.validated

candidate.submitted

operation-error.observed
```

Prefer metrics and IDs over content.

---

# 133. Testing — Event Envelope

Test:

* EventId unique
* same EventId on redelivery
* valid EventVersion
* producer = translation
* Attempt correlation present
* no credentials
* no full source/translated content
* optional fields forward-compatible

---

# 134. Testing — Planning Events

Test:

* plan.created only after READY
* Unit count accurate
* Batch count accurate
* context content absent
* terminology values absent
* source Artifact identity preserved

---

# 135. Testing — Provider Events

Test:

```text
execution-requested
    → output-received
```

without RUNNING.

Test:

```text
output-received
    ↛ batch.validated
```

until validation passes.

Test provider error sanitization.

---

# 136. Testing — Batch Validation

Test:

* valid provider response
* malformed response
* missing Unit
* duplicate Unit
* wrong Unit ID
* target-language mismatch
* terminology violation
* provider-control leakage
* source alignment failure

Only valid output produces:

```text
batch.validated
```

---

# 137. Testing — Candidate Events

Test:

```text
Candidate VALID
    → candidate.validated
```

```text
Candidate VALID
    → candidate.submitted
```

```text
Candidate INVALID
    ↛ candidate.submitted
```

```text
candidate.submitted
    ↛ Artifact publication assumption
```

---

# 138. Testing — Partial Candidate

Test:

```text
7 valid
2 failed
1 missing
```

produces:

```text
Completeness = PARTIAL
```

with explicit counts/references.

No final Translation completion event required.

---

# 139. Testing — Runtime Boundary

Test:

```text
Candidate VALID
    ↓
Runtime rejects stale
```

Translation emits no:

```text
completed

failed

superseded
```

terminal event.

---

# 140. Testing — Cancellation

Test:

* cancellation before provider request
* during provider execution
* after output received
* during Candidate validation
* before Candidate submission
* after Candidate submission

No Translation-owned `CANCELLED` terminal fact.

---

# 141. Testing — Duplicate Delivery

Duplicate:

```text
candidate.submitted event
```

must not:

* submit Candidate again
* publish duplicate Artifact
* duplicate UI overlay
* create duplicate variant

---

# 142. Testing — Ordering

Concurrent batches may deliver events out of order.

Final semantic ordering must use:

```text
TranslationUnit SourceSequence
```

not event sequence.

---

# 143. Testing — Privacy

Verify no event contains:

* source text by default
* translated text by default
* raw provider prompt
* raw provider response
* API key
* auth token
* Knowledge database
* source image

---

# 144. Property Tests

```text
candidate_submitted
implies Candidate was VALID
```

```text
candidate_validated
does not imply Runtime acceptance
```

```text
provider_output_received
does not imply batch_validated
```

```text
batch_validated
requires Translation output validation
```

```text
no Translation event
changes Runtime Attempt state
```

```text
no Translation event
grants publication authority
```

```text
no Translation event
selects Reading Session active variant
```

```text
duplicate event delivery
does not duplicate semantic output
```

```text
optional event loss
does not change Candidate semantics
```

---

# 145. Core Event Invariants

1. Events represent facts, not commands.
2. Events are immutable.
3. Events are provider-neutral.
4. Credentials never appear in events.
5. Source text is excluded by default.
6. Translated text is excluded by default.
7. Runtime Attempt identity is canonical.
8. TranslationJob identity does not exist.
9. TranslationAttempt identity does not exist.
10. Translation events do not define queue state.
11. Translation events do not define retry state.
12. Translation events do not define cancellation state.
13. Translation events do not define supersession state.
14. Translation events do not define terminal Runtime outcome.
15. Translation events do not publish Artifacts.
16. Candidate validation precedes Candidate submission.
17. Invalid Candidate cannot be validly submitted.
18. Candidate submission is not Runtime acceptance.
19. Provider output receipt is not Batch validation.
20. Batch validation requires Translation validation.
21. TranslationUnit order is independent of event arrival.
22. Partial completeness is explicit.
23. Missing Units are explicit.
24. Failed Units are explicit.
25. Variant creation is immutable.
26. Variant activation belongs outside Translation.
27. Knowledge updates are external.
28. Cache reuse belongs to Runtime policy.
29. Events tolerate duplicate delivery.
30. Optional event loss does not alter correctness.
31. Event history is not Translation state persistence.
32. Source content cannot control event metadata.
33. Provider output cannot control event routing.
34. Privacy classification is explicit.
35. Runtime authority always wins over late provider output.

---

# 146. Related Documents

```text
02-modules/translation/README.md
02-modules/translation/MODULE.md
02-modules/translation/CONTRACT.md
02-modules/translation/STATES.md
02-modules/translation/ERRORS.md

02-modules/text-processing/EVENTS.md
02-modules/text-processing/CONTRACT.md

02-modules/provider-management/
02-modules/knowledge/
02-modules/reading-session/
02-modules/presentation/

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

03-infrastructure/event-bus/
03-infrastructure/artifact-store/
03-infrastructure/resource-manager/
```

---

# 147. Summary

Translation event model is intentionally narrow:

```text
Module Availability

Translation Planning

Batch / Provider Observations

Validated Translation Units

Candidate Validation

Candidate Submission

Immutable Variant / Correction Facts
```

It does not create:

```text
Translation Job lifecycle

Translation Attempt lifecycle

Retry lifecycle

Cancellation lifecycle

Supersession lifecycle

Publication lifecycle

Active Variant lifecycle
```

Canonical semantic flow:

```text
SourceDocumentArtifact
        ↓
Translation Plan
        ↓
TranslationUnit[]
        ↓
TranslationBatch[]
        ↓
Provider Execution
        ↓
TranslatedUnit[]
        ↓
CandidateTranslationArtifact
        ↓
Runtime
        ↓
Artifact Store
        ↓
TranslationArtifact
```

Core rule:

```text
Translation events may announce
what Translation planned,
observed,
validated,
and produced.

Runtime decides
whether that execution still matters.

Artifact Store decides
what becomes a published Translation Artifact.

Reading Session decides
which compatible translation the reader is using.
```
