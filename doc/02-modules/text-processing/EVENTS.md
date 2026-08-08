# Text Processing Module Events

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `02-modules/text-processing/EVENTS.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `STATES.md`

---

# 1. Purpose

Tài liệu này định nghĩa event boundary của Text Processing Module.

Text Processing events được dùng để mô tả:

* module availability
* module configuration observations
* Attempt-local processing milestones
* SourceDocument construction observations
* Candidate validation
* Candidate submission
* diagnostics and telemetry

Text Processing events không định nghĩa canonical lifecycle của:

* WorkItem
* Attempt
* Queue
* Scheduler
* Retry
* Cancellation
* Supersession
* Artifact publication
* Translation
* Reading Session

Core rule:

```text
Text Processing events describe
what Text Processing observed or produced.

They do not decide
whether the Attempt succeeded,
whether the work is still authoritative,
or whether an Artifact is published.
```

---

# 2. Event Ownership

Text Processing may produce events about:

```text
Module Availability

Processing Plan

Attempt-Local Operation Phase

SourceDocument Construction

Traceability Validation

Candidate Validation

Candidate Submission
```

Text Processing does not own events declaring:

```text
WorkItem queued

Attempt started

Attempt succeeded

Attempt failed

Attempt canceled

Attempt superseded

Retry scheduled

Artifact published

Translation requested
```

Those belong to their owning modules.

---

# 3. Event Boundary

Events must remain lightweight.

Events should carry:

* identifiers
* state observations
* Candidate identifiers
* Artifact references
* warning summaries
* error summaries
* metrics
* correlation metadata
* compatibility metadata
* privacy metadata

Events should not carry:

* complete Recognition Artifact
* complete OCRDocument
* complete SourceDocument
* RawText
* NormalizedText
* image bytes
* Translation Units
* translated text
* provider credentials

Large semantic objects must be referenced rather than embedded.

---

# 4. Event Categories

Text Processing events are divided into:

```text
Module Availability Events

Configuration Observation Events

Attempt-Local Observation Events

Candidate Events
```

Optional diagnostic events may exist within these categories.

---

# 5. Event Naming

Recommended namespace:

```text
text_processing.<event_name>
```

Core module availability events:

```text
text_processing.module_available

text_processing.module_degraded

text_processing.module_unavailable

text_processing.module_draining

text_processing.module_stopped
```

Core Attempt-local observations:

```text
text_processing.plan_created

text_processing.input_adapted

text_processing.normalization_completed

text_processing.reconstruction_completed

text_processing.grouping_completed

text_processing.classification_completed

text_processing.document_built

text_processing.traceability_validated
```

Core Candidate events:

```text
text_processing.candidate_validated

text_processing.candidate_invalid

text_processing.candidate_submitted
```

Optional configuration observation:

```text
text_processing.configuration_observed
```

---

# 6. Removed Lifecycle Events

The following legacy lifecycle events are no longer canonical Text Processing events:

```text
text_processing.requested

text_processing.started

text_processing.completed

text_processing.failed

text_processing.cancellation_requested

text_processing.cancelled
```

Reason:

```text
WorkItem / Attempt lifecycle
belongs to Runtime.
```

Text Processing must not maintain a parallel execution lifecycle through events.

---

# 7. No Terminal Event Ownership

Text Processing does not guarantee:

```text
exactly one of:

completed

failed

cancelled
```

for each execution.

Instead:

```text
Runtime Attempt
    ↓
invokes Text Processing
    ↓
Text Processing performs local work
    ↓
returns Candidate or Module Error
    ↓
Runtime decides Attempt outcome
```

Therefore:

```text
Text Processing FINISHED
    ≠
Attempt SUCCEEDED
```

and:

```text
Candidate submitted
    ≠
Artifact published
```

---

# 8. Standard Event Envelope

All Text Processing events should use the shared CRAI Event Envelope.

Conceptual structure:

```text
EventEnvelope
├── EventId
├── EventName
├── EventVersion
├── OccurredAt
├── Producer
├── Subject
├── Correlation
├── Causation?
├── Sequence?
├── Delivery?
├── Privacy
├── Payload
└── Extensions?
```

The shared Event Bus specification remains authoritative for transport semantics.

---

# 9. Event ID

```text
EventId
```

must:

* identify one semantic event publication
* support deduplication
* remain stable during redelivery
* not be reused for unrelated events

Redelivery:

```text
same semantic event
    → same EventId
```

New semantic observation:

```text
new EventId
```

---

# 10. Event Version

```text
EventVersion
```

versions the schema of one event type.

Recommended:

```text
MAJOR.MINOR.PATCH
```

Independent from:

```text
ModuleVersion

ContractVersion

ProcessingProfileVersion

ProcessingStrategyVersion

ConfigurationSnapshotVersion
```

---

# 11. Producer

Text Processing-produced events use:

```text
Producer.Module = text-processing
```

Conceptually:

```text
EventProducer
├── Module
├── InstanceId?
└── Version?
```

---

# 12. Event Subject

Subjects should identify the entity being observed.

Examples:

```text
ProcessingPlan

TextProcessingAttemptExecution

CandidateSourceDocumentArtifact

TextProcessingModule

TextProcessingConfiguration
```

Avoid introducing:

```text
TextProcessingJob
```

as a new domain entity.

---

# 13. Correlation Model

Relevant identifiers:

```text
TraceId

RevisionId

WorkItemId

AttemptId

RecognitionArtifactId

CandidateArtifactId?

SourceId?

SessionId?
```

Not every event requires every identifier.

---

# 14. Required Attempt Correlation

Attempt-local events should contain:

```text
RevisionId

WorkItemId

AttemptId
```

and when available:

```text
TraceId

RecognitionArtifactId
```

This allows events to correlate with Runtime without creating module-owned lifecycle identity.

---

# 15. Candidate Correlation

Candidate events should additionally include:

```text
CandidateArtifactId
```

and lineage:

```text
RecognitionArtifactId
```

Example:

```text
RevisionId
    ↓
WorkItemId
    ↓
AttemptId
    ↓
RecognitionArtifactId
    ↓
CandidateArtifactId
```

---

# 16. Causation

When supported by the shared Event Bus:

```text
CausationEventId
```

may identify the event that triggered the current observation.

However, Text Processing correctness must not depend on every internal phase being represented as an event.

---

# 17. Event Ordering

Global ordering is not required.

Ordering may be meaningful within:

```text
AttemptId
```

or:

```text
CandidateArtifactId
```

Consumers must tolerate:

* duplicate delivery
* delayed delivery
* missing optional observations
* out-of-order diagnostic events

---

# 18. Event Sequence

Optional:

```text
Sequence
├── StreamId
└── Number
```

Recommended stream:

```text
text-processing:<AttemptId>
```

Sequence is primarily diagnostic.

It must not become the source of Runtime authority.

---

# 19. Optional Event Principle

Most Text Processing events are observational.

Therefore:

```text
No consumer may require
all internal Text Processing events
for correctness.
```

A valid execution may emit only:

```text
Candidate submitted
```

plus shared Runtime telemetry.

---

# 20. Module Availability Events

Module availability events describe the module-owned availability state from `STATES.md`.

They do not describe Attempt state.

---

# 21. `text_processing.module_available`

Published when Text Processing can execute supported Plans.

Payload:

```text
TextProcessingModuleAvailable
├── ModuleInstanceId?
├── SupportedContractVersions[]
├── SupportedProfiles[]
├── ConfigurationSnapshotId?
├── AvailableAt
└── Capabilities?
```

---

# 22. `text_processing.module_degraded`

Published when Text Processing remains usable but some optional capabilities are unavailable.

Payload:

```text
TextProcessingModuleDegraded
├── ModuleInstanceId?
├── ReasonCodes[]
├── AvailableProfiles[]
├── UnavailableCapabilities[]
├── ConfigurationSnapshotId?
└── DegradedAt
```

Examples:

```text
OPTIONAL_CLASSIFIER_UNAVAILABLE

ADVANCED_GROUPING_DISABLED

DIAGNOSTICS_REDUCED

RESOURCE_PRESSURE
```

---

# 23. `text_processing.module_unavailable`

Published when Text Processing cannot satisfy its required contract.

Payload:

```text
TextProcessingModuleUnavailable
├── ModuleInstanceId?
├── ReasonCodes[]
├── RetryHint?
└── UnavailableAt
```

`RetryHint` is advisory only.

Runtime decides retry behavior.

---

# 24. `text_processing.module_draining`

Published when module enters DRAINING.

Payload:

```text
TextProcessingModuleDraining
├── ModuleInstanceId?
├── ActiveAttemptCount?
├── Reason?
└── StartedAt
```

The event does not cancel Runtime Attempts.

Runtime/shutdown coordination owns cancellation authority.

---

# 25. `text_processing.module_stopped`

Published after module-owned active resources have been released.

Payload:

```text
TextProcessingModuleStopped
├── ModuleInstanceId?
└── StoppedAt
```

Avoid request-level completion statistics as canonical state.

---

# 26. Configuration Observation

Text Processing may observe shared configuration changes.

Recommended event:

```text
text_processing.configuration_observed
```

This means:

```text
Text Processing has observed
a configuration snapshot
```

not:

```text
all active Attempts were mutated
```

---

# 27. Configuration Snapshot Rule

An active Attempt uses one immutable effective Configuration Snapshot.

Therefore:

```text
Attempt N
    → Configuration Snapshot A

configuration changes
    ↓

Attempt N remains on A

future Attempt
    → may use Snapshot B
```

---

# 28. Configuration Observation Payload

```text
TextProcessingConfigurationObserved
├── ConfigurationSnapshotId
├── PreviousSnapshotId?
├── Supported
├── Compatibility?
├── RequiresRestart?
└── ObservedAt
```

---

# 29. Processing Plan Event

Optional:

```text
text_processing.plan_created
```

Published after:

```text
ProcessingPlanState = READY
```

Payload:

```text
TextProcessingPlanCreated
├── RevisionId
├── WorkItemId
├── AttemptId
├── RecognitionArtifactId
├── ProcessingProfileId
├── ProcessingProfileVersion
├── ConfigurationSnapshotId
├── StrategyVersion?
├── EnabledOperations[]
└── CreatedAt
```

---

# 30. Plan Event Restrictions

Do not include:

* full Processing Plan
* source text
* credentials
* Runtime priority mutations
* Runtime retry state
* Runtime cancellation state

---

# 31. `text_processing.input_adapted`

Optional observation after Recognition Artifact input is converted to the internal processing representation.

Payload:

```text
TextProcessingInputAdapted
├── RevisionId
├── WorkItemId
├── AttemptId
├── RecognitionArtifactId
├── InputRegionCount?
├── InputLineCount?
├── CoordinateSpace?
├── UpstreamWarningCount?
├── DurationMs?
└── OccurredAt
```

No OCR text is included.

---

# 32. No `order_resolved` Event

Legacy:

```text
text_processing.order_resolved
```

is removed.

Reason:

```text
Canonical OCR Reading Order
belongs to Recognition / OCR architecture.
```

Text Processing may reconstruct source structure using reading-order evidence, but does not declare a replacement canonical OCR Reading Order.

---

# 33. `text_processing.normalization_completed`

Optional observation.

Payload:

```text
TextProcessingNormalizationCompleted
├── RevisionId
├── WorkItemId
├── AttemptId
├── NodeCount
├── ChangedNodeCount
├── ChangeCount?
├── WarningCodes[]
├── DurationMs?
└── OccurredAt
```

No RawText or NormalizedText values.

---

# 34. `text_processing.reconstruction_completed`

Published optionally after structural reconstruction.

Replaces legacy:

```text
lines_reconstructed
```

because reconstruction may operate above line level.

Payload:

```text
TextProcessingReconstructionCompleted
├── RevisionId
├── WorkItemId
├── AttemptId
├── InputNodeCount?
├── OutputStructureCount?
├── JoinCount?
├── PreservedSeparateCount?
├── AmbiguousDecisionCount?
├── WarningCodes[]
├── DurationMs?
└── OccurredAt
```

---

# 35. Reconstruction Event Semantics

The event means:

```text
local reconstruction phase completed
```

It does not mean:

```text
SourceDocument valid
```

or:

```text
Attempt succeeded
```

---

# 36. `text_processing.grouping_completed`

Optional observation after logical source grouping.

Payload:

```text
TextProcessingGroupingCompleted
├── RevisionId
├── WorkItemId
├── AttemptId
├── InputStructureCount?
├── OutputGroupCount?
├── MergeCount?
├── PreservedSeparateCount?
├── AmbiguousGroupCount?
├── DurationMs?
└── OccurredAt
```

---

# 37. Grouping Uncertainty

Grouping uncertainty should be represented through metadata:

```text
AmbiguousGroupCount

WarningCodes
```

not through failure lifecycle events.

---

# 38. `text_processing.classification_completed`

Optional observation after source block classification.

Payload:

```text
TextProcessingClassificationCompleted
├── RevisionId
├── WorkItemId
├── AttemptId
├── BlockCount
├── ClassifiedCount
├── UnknownCount
├── LowConfidenceCount?
├── TypeCounts?
├── DurationMs?
└── OccurredAt
```

`TypeCounts` contains only type names and counts.

---

# 39. Unknown Classification

```text
BlockType = UNKNOWN
```

is valid.

Therefore:

```text
UnknownCount > 0
```

does not imply failure.

---

# 40. `text_processing.document_built`

Published optionally when a SourceDocument Candidate has been constructed.

Payload:

```text
TextProcessingDocumentBuilt
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId?
├── DocumentId
├── DocumentType?
├── BlockCount
├── RootBlockCount
├── ExcludedBlockCount?
├── Completeness
├── WarningCodes[]
├── DurationMs?
└── OccurredAt
```

---

# 41. Document Built Is Not Publication

```text
text_processing.document_built
```

means only:

```text
SourceDocument object assembled
```

It does not mean:

```text
Candidate valid

Runtime accepted

Artifact published

Translation may begin
```

---

# 42. `text_processing.traceability_validated`

Published optionally after SourceDocument traceability validation succeeds.

Payload:

```text
TextProcessingTraceabilityValidated
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId?
├── DocumentId
├── SourceNodeCount?
├── ReferencedSourceNodeCount?
├── UnresolvedSourceNodeCount?
├── CoverageRatio?
├── DurationMs?
└── OccurredAt
```

---

# 43. Traceability Failure

If traceability validation fails:

```text
no traceability_validated event
```

Text Processing returns:

```text
TextProcessingModuleError
```

to Runtime.

Runtime determines Attempt outcome.

Text Processing does not publish:

```text
text_processing.failed
```

as canonical terminal state.

---

# 44. Candidate Events

Candidate events are the most important Text Processing-owned event family.

They describe:

```text
Candidate SourceDocument Artifact
```

before Runtime/Artifact Store publication.

---

# 45. Candidate Event Flow

Conceptually:

```text
SourceDocument built
        ↓
Candidate assembled
        ↓
Candidate validated
        ↓
candidate_validated
        ↓
Candidate submitted to Runtime
        ↓
candidate_submitted
        ↓
Runtime decides disposition
```

---

# 46. `text_processing.candidate_validated`

Published when:

```text
CandidateValidationState = VALID
```

Payload:

```text
TextProcessingCandidateValidated
├── RevisionId
├── WorkItemId
├── AttemptId
├── RecognitionArtifactId
├── CandidateArtifactId
├── DocumentId
├── ArtifactType
├── ContractVersion
├── Completeness
├── WarningSummary
├── CompatibilitySummary
├── IntegrityMetadata?
├── ValidatedAt
└── TraceId?
```

---

# 47. Candidate Validated Semantics

This event means:

```text
Candidate satisfies
Text Processing-owned contract validation.
```

It does not mean:

```text
Attempt succeeded

Candidate is authoritative

Candidate is current

Artifact is published

Artifact is durable

Translation should start
```

---

# 48. Candidate Warning Summary

Recommended:

```text
WarningSummary
├── Total
├── Codes[]
└── HighestSeverity?
```

Do not include warning messages containing source text.

---

# 49. Candidate Compatibility Summary

Conceptual:

```text
CompatibilitySummary
├── ArtifactSchemaVersion
├── TextProcessingContractVersion
├── ProcessingProfileVersion
├── ProcessingStrategyVersion?
└── ConfigurationSnapshotId
```

---

# 50. `text_processing.candidate_invalid`

Optional diagnostic event.

Published when Candidate assembly reaches:

```text
CandidateValidationState = INVALID
```

Payload:

```text
TextProcessingCandidateInvalid
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId?
├── ValidationCode
├── ValidationCategory
├── Stage
├── RetryHint?
├── InvalidatedAt
└── TraceId?
```

This event is diagnostic.

Runtime outcome remains authoritative.

---

# 51. Candidate Invalid Privacy

Do not include:

* SourceDocument
* RawText
* NormalizedText
* stack trace
* credentials
* sensitive source fragments

Use stable validation codes.

---

# 52. `text_processing.candidate_submitted`

Published when a VALID Candidate crosses the Text Processing → Runtime boundary.

Payload:

```text
TextProcessingCandidateSubmitted
├── RevisionId
├── WorkItemId
├── AttemptId
├── RecognitionArtifactId
├── CandidateArtifactId
├── ArtifactType
├── ContractVersion
├── Completeness
├── SubmittedAt
└── TraceId?
```

---

# 53. Candidate Submission Rule

Only:

```text
CandidateValidationState = VALID
```

may produce:

```text
candidate_submitted
```

Forbidden:

```text
INVALID
    → candidate_submitted
```

---

# 54. Submission Is Not Acceptance

Critical distinction:

```text
candidate_submitted
    ≠
candidate accepted
```

After submission Runtime may decide:

```text
ACCEPTED

REJECTED_STALE

REJECTED_CANCELED

REJECTED_DUPLICATE

REJECTED_INVALID

REJECTED_RUNTIME_FAILURE
```

These are Runtime dispositions.

---

# 55. Artifact Publication Boundary

Text Processing must not publish:

```text
text_processing.artifact_published
```

unless architecture later explicitly transfers publication ownership.

Current boundary:

```text
Text Processing
    ↓
Candidate
    ↓
Runtime
    ↓
Artifact Store
    ↓
Published SourceDocument Artifact
```

---

# 56. Translation Trigger Boundary

Text Processing must not directly declare:

```text
translation requested
```

or:

```text
SourceDocument ready for translation
```

based solely on Candidate validation.

Translation may begin only from an accepted/published Artifact according to Runtime orchestration.

---

# 57. Runtime Events Observed by Text Processing

Text Processing may observe external Runtime facts such as:

```text
Attempt cancellation requested

Attempt deadline changed/exceeded

Attempt authority revoked

Candidate accepted

Candidate rejected

Runtime shutdown
```

Exact event names belong to Runtime documentation.

This file does not redefine them.

---

# 58. Cancellation

Text Processing does not consume:

```text
text_processing.cancellation_requested
```

as its own canonical command.

Instead cancellation is provided through Runtime-owned:

```text
CancellationContext
```

or equivalent Runtime mechanism.

---

# 59. Cancellation Observations

Text Processing may emit diagnostics such as:

```text
text_processing.cancellation_observed
```

if useful.

This is optional.

Payload:

```text
TextProcessingCancellationObserved
├── RevisionId
├── WorkItemId
├── AttemptId
├── OperationPhase
├── ObservedAt
└── TraceId?
```

It does not mean Runtime has committed `CANCELED`.

---

# 60. No `text_processing.cancelled`

Text Processing does not emit a canonical:

```text
text_processing.cancelled
```

terminal event.

Local behavior:

```text
CancellationContext requested
        ↓
Text Processing stops new expensive work
        ↓
cleanup
        ↓
return control to Runtime
        ↓
Runtime decides terminal disposition
```

---

# 61. Deadline

Deadline belongs to Runtime.

Text Processing may observe:

```text
RemainingBudget

DeadlineExceeded
```

but must not create a separate module-owned timeout lifecycle.

Optional diagnostic:

```text
text_processing.deadline_observed
```

should be used only if telemetry requires it.

---

# 62. Supersession

Text Processing does not own supersession.

Therefore legacy fields such as:

```text
supersedes_request_id

supersession_key

is_current_at_publication
```

do not belong to Text Processing-owned lifecycle logic.

Runtime owns:

```text
Revision relevance

Attempt authority

staleness

supersession
```

---

# 63. Stale Candidate

Possible flow:

```text
Candidate VALID
    ↓
candidate_submitted
    ↓
Runtime detects stale Revision
    ↓
REJECTED_STALE
```

Text Processing does not rewrite Candidate state to:

```text
SUPERSEDED
```

---

# 64. Retry

Text Processing does not publish:

```text
retry_requested

retry_started

retry_scheduled
```

as canonical module lifecycle events.

Text Processing may return:

```text
RetryHint
```

inside a normalized Module Error.

Runtime owns retry policy.

---

# 65. Retry Creates New Attempt

When Runtime retries:

```text
Attempt N
    ↓ failure
Runtime retry policy
    ↓
Attempt N+1
```

Text Processing event correlation therefore uses the new:

```text
AttemptId
```

No Text Processing event stream is reopened.

---

# 66. Duplicate Delivery

Event transport should assume:

```text
AtLeastOnce
```

unless the Event Bus guarantees otherwise.

Consumers must tolerate:

* duplicate events
* late events
* out-of-order observations

---

# 67. Consumer Deduplication

Primary key:

```text
EventId
```

When semantic deduplication is needed:

```text
AttemptId
+
EventName
+
CandidateArtifactId?
```

may be used.

---

# 68. Candidate Submission Idempotency

For one Candidate:

```text
semantic candidate submission count <= 1
```

Redelivery of the same event is allowed.

Duplicate delivery must not cause duplicate Artifact publication.

---

# 69. Runtime Deduplication

Runtime should use:

```text
CandidateArtifactId

AttemptId

RevisionId
```

when evaluating Candidate submission.

Exact Runtime idempotency rules belong to Runtime documentation.

---

# 70. Event Privacy

Default Text Processing events should be metadata-only.

Recommended:

```text
contains_source_text = false

contains_image_data = false

contains_translated_text = false
```

---

# 71. Sensitive Identifiers

Even metadata may be sensitive.

Examples:

```text
SessionId

SourceId

RevisionId

ArtifactId
```

Therefore events should use the shared CRAI privacy classification.

---

# 72. Source Text Prohibition

Normal events must not contain:

```text
RawText

NormalizedText

OCR text snippets

SourceBlock text

Translation text
```

Even error and warning events should use codes instead of source fragments.

---

# 73. Error Event Principle

Text Processing errors are primarily returned through the module execution contract.

Events may mirror errors for diagnostics.

They must not become the authoritative failure lifecycle.

Optional:

```text
text_processing.operation_error_observed
```

Payload:

```text
TextProcessingOperationErrorObserved
├── RevisionId
├── WorkItemId
├── AttemptId
├── OperationPhase
├── ErrorCode
├── ErrorCategory
├── RetryHint?
├── OccurredAt
└── TraceId?
```

---

# 74. No Stack Traces in Events

Events must not contain:

* stack traces
* arbitrary exception serialization
* credentials
* filesystem secrets
* full provider responses
* source content

Detailed diagnostics belong to Logging/Telemetry according to privacy policy.

---

# 75. Progress Events

Progress events are optional.

Consumers must remain correct if none are emitted.

Recommended production policy:

```text
No per-character events

No per-block events

No per-rule events

No high-frequency percentage events

Prefer phase-level aggregate observations
```

---

# 76. Progress Percentage

A generic:

```text
0.0 → 1.0
```

percentage is not required.

Text Processing operations may vary significantly by:

* page structure
* OCR node count
* Processing Profile
* optional operations

Phase observations are usually more meaningful.

---

# 77. Metrics

Events may carry bounded aggregate metrics:

```text
DurationMs

InputNodeCount

OutputBlockCount

JoinCount

GroupCount

UnknownClassificationCount

WarningCount
```

Metrics must not expose source content.

---

# 78. Event vs Telemetry

Use Event Bus when:

```text
another component may reasonably observe
a semantic module fact
```

Use Telemetry when:

```text
the information exists only
for monitoring/performance/debugging
```

Avoid turning every metric into a domain event.

---

# 79. Event vs Log

Example:

```text
Candidate validated
```

may justify an event.

Example:

```text
normalization rule #17 took 0.4 ms
```

belongs to telemetry/logging.

---

# 80. Event vs Contract Return

The execution contract remains the primary synchronous/local communication boundary.

Example:

```text
Text Processing returns Candidate
```

does not require another module to wait for:

```text
candidate_validated event
```

Events are not a replacement for direct Runtime execution semantics.

---

# 81. Module Availability Transition Events

Recommended mapping:

```text
AVAILABLE
    → module_available

DEGRADED
    → module_degraded

UNAVAILABLE
    → module_unavailable

DRAINING
    → module_draining

STOPPED
    → module_stopped
```

No event is required for every internal transition if no observer needs it.

---

# 82. Processing Phase Mapping

Recommended optional mapping:

```text
ADAPTING_INPUT
    → input_adapted

NORMALIZING
    → normalization_completed

RECONSTRUCTING
    → reconstruction_completed

GROUPING
    → grouping_completed

CLASSIFYING
    → classification_completed

BUILDING_DOCUMENT
    → document_built

VALIDATING_TRACEABILITY
    → traceability_validated
```

No event for:

```text
FINISHED
```

is required.

Runtime already owns Attempt completion.

---

# 83. Candidate State Mapping

```text
VALID
    → candidate_validated

INVALID
    → candidate_invalid [optional]

SUBMITTED_TO_RUNTIME
    → candidate_submitted
```

Candidate acceptance is external.

---

# 84. Empty-Valid Candidate

Example:

```text
Completeness = EMPTY_VALID
```

may still produce:

```text
candidate_validated

candidate_submitted
```

Empty semantic output is not automatically failure.

---

# 85. Partial Candidate

Example:

```text
Completeness = PARTIAL
```

may produce:

```text
candidate_validated
```

when the Text Processing Contract permits partial output.

Runtime still decides whether the Candidate is acceptable for the WorkItem.

---

# 86. Event Compatibility

Consumers must evaluate:

```text
EventName

EventVersion
```

Unknown optional fields should normally be ignored.

Unknown major versions may be rejected or routed according to shared Event Bus policy.

---

# 87. Candidate Compatibility

Candidate events may include compatibility summaries but do not replace Artifact-level compatibility metadata.

Artifact metadata remains authoritative.

---

# 88. Event Schema Evolution

Backward-compatible changes may include:

* optional metadata fields
* optional metrics
* new warning codes
* new extension fields

Breaking changes include:

* changing semantic meaning
* removing required identity fields
* changing identifier ownership
* changing Candidate event semantics

Breaking changes require major version update.

---

# 89. Event Delivery Failure

Failure to publish an optional diagnostic event must not invalidate an otherwise valid Candidate.

Example:

```text
normalization_completed publication failed
    ↓
Candidate processing may continue
```

depending on Event Bus policy.

---

# 90. Candidate Submission Event Failure

Important distinction:

```text
Candidate submission to Runtime
```

and:

```text
candidate_submitted event publication
```

are not the same operation.

Runtime submission is the correctness boundary.

The event is observational.

Therefore event publication failure must not cause duplicate Candidate submission.

---

# 91. Outbox

If reliable event publication is required, use shared Event Bus/outbox infrastructure.

Text Processing should not implement a private event durability subsystem.

---

# 92. Recovery

After Text Processing process crash:

* Attempt-local phase events do not reconstruct the Attempt
* Processing Plan is not restored from event history
* Candidate assembly is not resumed from event history
* Runtime determines whether a new Attempt is created

Events are not Text Processing event sourcing.

---

# 93. Event Sourcing Boundary

Text Processing is not event-sourced by default.

Do not reconstruct:

```text
ProcessingPlan

OperationPhase

WorkingNodes

CandidateAssembly
```

by replaying Text Processing events.

---

# 94. Testing — Availability Events

Test:

* AVAILABLE emits correct observation
* DEGRADED carries bounded reasons
* UNAVAILABLE exposes no source content
* DRAINING does not imply Attempt cancellation
* STOPPED does not claim Runtime terminal outcomes

---

# 95. Testing — Processing Events

Test:

* plan_created contains Attempt correlation
* input_adapted contains no OCR text
* normalization event contains counts only
* reconstruction event does not redefine Reading Order
* grouping uncertainty remains metadata
* UNKNOWN classification remains valid
* document_built does not imply Candidate validity
* traceability_validated emitted only after validation success

---

# 96. Testing — Candidate Events

Test:

```text
VALID
    → candidate_validated
```

```text
VALID
    → candidate_submitted
```

```text
INVALID
    ↛ candidate_submitted
```

```text
candidate_submitted
    ↛ artifact published
```

```text
candidate_submitted
    ↛ attempt succeeded
```

---

# 97. Testing — Runtime Boundary

Test:

```text
Candidate VALID
    ↓
Runtime rejects stale
```

Text Processing must not emit `completed`.

Test:

```text
Cancellation observed
    ↓
local cleanup
    ↓
Runtime decides CANCELED
```

Text Processing must not own terminal cancellation.

Test:

```text
Candidate submitted
    ↓
Runtime rejects duplicate
```

Candidate remains immutable.

---

# 98. Testing — Event Delivery

Test:

* duplicate delivery
* late delivery
* out-of-order phase events
* missing optional progress events
* event publication failure
* Candidate submission event redelivery
* unsupported event version
* unknown optional fields

---

# 99. Property Tests

```text
candidate_submitted
implies Candidate was VALID
```

```text
candidate_submitted
does not imply Runtime acceptance
```

```text
no Text Processing event
changes Runtime Attempt state
```

```text
no normal Text Processing event
contains source text
```

```text
optional event loss
does not change semantic Candidate
```

```text
duplicate event delivery
does not create duplicate Candidate
```

```text
Text Processing events
never grant publication authority
```

---

# 100. Recommended MVP Events

For MVP, keep the event surface small.

Recommended:

```text
text_processing.module_available

text_processing.module_degraded

text_processing.module_unavailable

text_processing.candidate_validated

text_processing.candidate_submitted
```

Optional diagnostics:

```text
text_processing.document_built

text_processing.traceability_validated

text_processing.operation_error_observed
```

Everything else can initially remain Telemetry.

---

# 101. Events Deferred Beyond MVP

Do not require initially:

```text
plan_created

input_adapted

normalization_completed

reconstruction_completed

grouping_completed

classification_completed

cancellation_observed

deadline_observed

configuration_observed
```

Add only when a real consumer or operational requirement exists.

---

# 102. Removed Legacy Concepts

The following concepts from the previous event model are intentionally removed:

```text
TextProcessingRequest lifecycle

request_id as Text Processing lifecycle identity

processing_id as terminal processing identity

text_processing.requested

text_processing.started

text_processing.completed

text_processing.failed

text_processing.cancelled

text_processing.cancellation_requested

terminal-event ownership

module-owned terminal race

module-owned supersession

module-owned retry scheduling

result registry ownership

result publication ownership

translation trigger ownership

order_resolved as canonical Reading Order
```

---

# 103. Ownership Summary

```text
Runtime owns:

WorkItem lifecycle

Attempt lifecycle

Authority

Cancellation

Deadline

Supersession

Retry

Terminal outcome
```

```text
Recognition owns:

Recognition Artifact

OCRDocument

Canonical OCR Reading Order
```

```text
Text Processing owns:

Processing Plan

Source reconstruction

SourceDocument construction

Candidate validation

Candidate submission
```

```text
Artifact Store owns:

Accepted Artifact publication

Artifact identity after acceptance

Artifact lifecycle
```

```text
Translation owns:

Translation Plan

Translation Units

Translation Context

Translated Content
```

---

# 104. Canonical Event Flow

```text
Runtime Attempt
      │
      ▼
Text Processing
      │
      ├── [optional observations]
      │
      ▼
Build SourceDocument
      │
      ▼
Validate Traceability
      │
      ▼
Build Candidate
      │
      ▼
Validate Candidate
      │
      ├── candidate_validated
      │
      ▼
Submit Candidate to Runtime
      │
      └── candidate_submitted
              │
              ▼
            Runtime
              │
      ┌───────┴────────┐
      │                │
   ACCEPT           REJECT
      │
      ▼
Artifact Store
      │
      ▼
Published SourceDocument Artifact
```

---

# 105. Core Invariants

1. Text Processing events never grant Runtime authority.
2. Text Processing events never publish Artifacts.
3. Text Processing events never define Attempt terminal state.
4. Candidate submission is not Candidate acceptance.
5. Candidate validation is not Artifact publication.
6. Cancellation belongs to Runtime.
7. Supersession belongs to Runtime.
8. Retry belongs to Runtime.
9. Canonical OCR Reading Order belongs upstream.
10. Translation does not start from an unaccepted Candidate.
11. Normal events contain no source text.
12. Optional events are not required for correctness.
13. Duplicate events are safe.
14. Event loss must not mutate semantic processing output.
15. Event history is not Text Processing state persistence.

---

# 106. Related Documents

```text
02-modules/text-processing/README.md
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md
02-modules/text-processing/STATES.md
02-modules/text-processing/ERRORS.md

02-modules/recognition/CONTRACT.md
02-modules/recognition/STATES.md
02-modules/recognition/EVENTS.md

01-architecture/ocr/READING_ORDER.md
01-architecture/ocr/POSTPROCESS.md
01-architecture/ocr/QUALITY.md

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

03-infrastructure/event-bus/
03-infrastructure/artifact-store/
03-infrastructure/resource-manager/

02-modules/translation/
```

---

# 107. Summary

Text Processing event model is intentionally narrow:

```text
Module Availability

Attempt-Local Observations

Candidate Validation

Candidate Submission
```

It does not create:

```text
TextProcessingJob lifecycle

Request terminal lifecycle

Cancellation lifecycle

Retry lifecycle

Supersession lifecycle

Publication lifecycle
```

Canonical boundary:

```text
Recognition Artifact
        ↓
Text Processing
        ↓
SourceDocument
        ↓
Candidate
        ↓
Runtime
        ↓
Artifact Store
```

Core rule:

```text
Text Processing may announce
what it observed and what it produced.

Runtime decides
whether that work still matters.

Artifact Store decides
what becomes a published Artifact.

Translation operates
only after the appropriate accepted Artifact exists.
```
