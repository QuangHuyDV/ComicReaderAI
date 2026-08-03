# Recognition Module Events

> Project: CRAI  
> Module: Recognition  
> Path: `doc/02-modules/recognition/EVENTS.md`  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa event contract của Recognition Module theo Runtime v2.

Nó đặc tả:

- event ownership;
- event categories;
- shared event envelope;
- Runtime correlation context;
- Recognition domain facts;
- Recognition progress facts;
- warning/error facts;
- diagnostic facts;
- Candidate event boundary;
- Runtime event boundary;
- Provider Manager event boundary;
- consumed-event policy;
- ordering;
- delivery và idempotency;
- privacy;
- publication failure behavior;
- consumer expectations;
- MVP event set;
- testing;
- invariants.

Tài liệu này chỉ định nghĩa event-driven communication.

Command/data contracts nằm trong:

```text
doc/02-modules/recognition/CONTRACT.md
```

Recognition state ownership nằm trong:

```text
doc/02-modules/recognition/STATES.md
```

---

## 2. Event Role of Recognition

Recognition là processing module được Runtime invoke trong một Attempt.

```text
Runtime Attempt
    ↓
Recognition Execution
    ↓
Optional Recognition Facts
    ↓
Candidate Submitted to Runtime
```

Recognition events:

- report facts đã xảy ra;
- hỗ trợ diagnostics, observability và optional progress;
- không xác định Attempt terminal outcome;
- không cấp authority;
- không publish authoritative Artifact;
- không trigger downstream work trực tiếp;
- không thay Attempt Completion contract.

Không có Recognition event nào bắt buộc cho correctness.

---

## 3. Event Ownership

Recognition owns:

```text
Recognition domain facts
Recognition progress facts
Recognition warning/error facts
Recognition diagnostic facts
```

Recognition does not own:

```text
ATTEMPT_STARTED
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
ATTEMPT_ABANDONED
AUTHORITY_REJECTED
ARTIFACT_PUBLISHED
PROVIDER_READY
PROVIDER_DEGRADED
PROVIDER_UNAVAILABLE
CONFIG_ACTIVATED
SESSION_STOPPED
APPLICATION_SHUTDOWN_REQUESTED
```

Các event trên thuộc Runtime hoặc component owner tương ứng.

---

## 4. Event Design Principles

### 4.1 Events Represent Facts

Correct:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_CANDIDATE_VALIDATED
```

Avoid:

```text
RECOGNITION_CREATE_PLAN
RECOGNITION_DETECT_REGIONS
RECOGNITION_VALIDATE_CANDIDATE
```

### 4.2 Events Are Optional for Correctness

Recognition execution phải thành công ngay cả khi Event Bus unavailable.

Candidate submission đi qua Attempt Completion, không qua event.

### 4.3 Events Are Immutable

Published event không được mutate.

Correction hoặc additional fact cần event mới.

### 4.4 Events Are Safe to Duplicate

Every event has unique `EventId`.

Consumers deduplicate by EventId.

### 4.5 Events Are Not Globally Ordered

Ordering chỉ causal/best-effort trong một Attempt hoặc Revision partition.

### 4.6 Events Do Not Grant Authority

Event occurrence không chứng minh:

- Revision còn current;
- Attempt được accepted;
- Candidate được published;
- downstream work được phép chạy.

### 4.7 Large Payloads Travel by Reference

Event Bus không carry:

- image bytes;
- Candidate payload;
- Recognition Artifact payload;
- provider raw response;
- diagnostic image;
- full recognized text.

### 4.8 Privacy Is Contractual

Normal Recognition events chứa operational metadata only.

---

## 5. Event Naming

Canonical architectural names use:

```text
UPPER_SNAKE_CASE
```

Format:

```text
RECOGNITION_<FACT>
```

Examples:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_PREPARATION_COMPLETED
RECOGNITION_REGIONS_DETECTED
```

Transport adapters may map to topics such as:

```text
recognition.plan_created
```

Canonical event identity remains the architectural name.

---

## 6. Event Categories

```text
Recognition Events
├── Domain Facts
├── Progress Facts
├── Warning/Error Facts
└── Diagnostic Facts
```

No Recognition-owned lifecycle category exists.

---

## 7. Shared Event Envelope

```text
RecognitionEventEnvelope<T>
├── EventId
├── EventName
├── ContractVersion
├── ProducerModule
├── OccurredAt
├── TraceContext
├── RuntimeContext
├── PrivacyClassification
├── PartitionKey?
├── SequenceNumber?
└── Payload
```

---

## 8. Envelope Fields

### EventId

```text
EventId = opaque string
```

Requirements:

1. unique within retention window;
2. immutable;
3. not reused;
4. safe for deduplication;
5. not interpreted for business meaning.

### EventName

Canonical Recognition event name.

### ContractVersion

```text
1.0.0
```

Consumers reject unsupported major version.

### ProducerModule

```text
recognition
```

Provider adapters do not publish Recognition public facts under provider-specific namespaces.

### OccurredAt

UTC timestamp.

### PrivacyClassification

```text
OPERATIONAL_METADATA
PROTECTED_DIAGNOSTIC_REFERENCE
```

Default:

```text
OPERATIONAL_METADATA
```

---

## 9. Trace Context

```text
TraceContext
├── TraceId
├── ParentSpanId?
├── CorrelationId?
└── Baggage?
```

Baggage must not contain:

- image data;
- recognized text;
- credential;
- provider request body;
- private source path.

---

## 10. Recognition Event Runtime Context

```text
RecognitionEventContext
├── ApplicationInstanceId
├── SessionId?
├── RevisionId
├── WorkItemId
├── AttemptId
├── CandidateArtifactId?
├── ArtifactId?
├── InputArtifactId?
├── ProviderId?
├── ConfigurationSnapshotId
├── RecognitionOperation
├── RecognitionProfile
├── OperationPhase?
└── BusinessStageId?
```

### Rules

1. RevisionId, WorkItemId, AttemptId required.
2. SessionId optional for standalone image.
3. CandidateArtifactId only after Candidate exists.
4. ArtifactId appears only in externally sourced Runtime/Artifact event, not ordinary Recognition Candidate fact.
5. ProviderId optional before provider execution.
6. No RequestId lifecycle identity is created by Recognition.
7. No retry attempt integer; AttemptId is authoritative identity.
8. No priority or queue class in Recognition event context.

---

## 11. Partition Key

Recommended:

```text
AttemptId
```

for Attempt-local facts.

Alternative:

```text
RevisionId
```

for Revision-scoped diagnostics.

Selection must follow Event Convention.

---

## 12. Sequence Number

Optional monotonic sequence per partition.

Example:

```text
RECOGNITION_PLAN_CREATED                  1
RECOGNITION_PREPARATION_COMPLETED         2
RECOGNITION_REGIONS_DETECTED              3
RECOGNITION_CANDIDATE_VALIDATED           4
```

Consumers must still tolerate:

- duplicates;
- delays;
- missing optional facts;
- out-of-order delivery.

---

## 13. Domain Facts

Core Recognition domain facts:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_CANDIDATE_VALIDATED
RECOGNITION_CANDIDATE_SUBMITTED
```

These describe semantic milestones only.

---

## 14. `RECOGNITION_PLAN_CREATED`

### Meaning

A valid immutable Recognition Plan has been built for an Attempt.

### Payload

```text
RecognitionPlanCreatedEvent
├── PlanId
├── Strategy
├── RecognitionOperation
├── RecognitionProfile
├── CapabilityRequirementSummary
├── PreparationProfileId?
├── CoordinatePolicyVersion
├── ReadingOrderPolicyVersion
├── QualityPolicyVersion
├── PrivacyMode
├── ExecutionClasses[]
├── EstimatedResourceCost?
└── CreatedAt
```

### Rules

1. emitted after Plan state becomes READY;
2. no provider credential;
3. provider selection may still be external/pending;
4. no image reference;
5. no raw Recognition options containing sensitive fields;
6. optional for correctness;
7. does not mean Attempt has started provider execution.

---

## 15. `RECOGNITION_CANDIDATE_VALIDATED`

### Meaning

A Candidate Recognition Artifact passed Recognition semantic validation.

### Payload

```text
RecognitionCandidateValidatedEvent
├── CandidateArtifactId
├── ArtifactType
├── Completeness
├── QualityLevel
├── RegionCount
├── LineCount
├── CharacterCount
├── WarningCount
├── ProviderProvenanceSummary
├── CompatibilityMetadataVersion
├── ValidationDurationMs
└── ValidatedAt
```

### Rules

1. no Candidate payload;
2. no full recognized text;
3. Candidate is not authoritative;
4. Candidate is not published;
5. event does not grant ownership transfer;
6. event does not trigger Text Processing;
7. Runtime may later reject Candidate;
8. empty-valid Candidate is allowed.

---

## 16. `RECOGNITION_CANDIDATE_SUBMITTED`

### Meaning

Recognition submitted a valid Candidate through Attempt Completion to Runtime.

### Payload

```text
RecognitionCandidateSubmittedEvent
├── CandidateArtifactId
├── ArtifactType
├── SubmissionMode
├── SubmittedAt
└── DiagnosticsRef?
```

```text
SubmissionMode
├── ATTEMPT_COMPLETION
└── DIAGNOSTIC_EVALUATION
```

### Rules

1. submission is not publication;
2. no ArtifactId yet;
3. no downstream orchestration;
4. Candidate must not be mutated afterward;
5. Runtime disposition may be accepted or rejected;
6. optional fact only.

---

## 17. Progress Facts

```text
RECOGNITION_PREPARATION_COMPLETED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_REGION_PROCESSED
RECOGNITION_PROVIDER_OUTPUT_NORMALIZED
RECOGNITION_READING_ORDER_RESOLVED
```

Progress facts:

- optional;
- best-effort;
- sampleable;
- not required for correctness;
- not authoritative;
- not terminal;
- not downstream triggers.

---

## 18. `RECOGNITION_PREPARATION_COMPLETED`

### Payload

```text
RecognitionPreparationCompletedEvent
├── PreparationProfileId?
├── OperationCount
├── SourceWidth
├── SourceHeight
├── ProcessedWidth
├── ProcessedHeight
├── GeometryChanged
├── DurationMs
└── CompletedAt
```

### Rules

1. no processed image;
2. no temporary path;
3. no provider secret;
4. geometry-changing operations tracked internally;
5. may be omitted under sampling.

---

## 19. `RECOGNITION_REGIONS_DETECTED`

### Payload

```text
RecognitionRegionsDetectedEvent
├── DetectedRegionCount
├── LowConfidenceRegionCount
├── OrientationSummary
├── DetectionProviderId?
├── DetectionDurationMs
└── DetectedAt
```

Trusted in-process extension may include bounded region summaries:

```text
RegionSummary
├── RegionId
├── Geometry
├── Orientation
└── ConfidenceLevel
```

No recognized text.

---

## 20. `RECOGNITION_REGION_PROCESSED`

### Meaning

One bounded region operation completed.

### Payload

```text
RecognitionRegionProcessedEvent
├── RegionId
├── RegionIndex
├── TotalRegionCount
├── ConfidenceLevel
├── CharacterCount
├── DurationMs
└── ProcessedAt
```

### Rules

1. bounded/sampled frequency;
2. no text content;
3. may arrive out of spatial order;
4. not a partial Artifact;
5. consumers do not start Translation from it;
6. may be lost.

---

## 21. `RECOGNITION_PROVIDER_OUTPUT_NORMALIZED`

### Payload

```text
RecognitionProviderOutputNormalizedEvent
├── ProviderId
├── ProviderRequestId?
├── RegionCount
├── LineCount
├── ConfidenceAvailable
├── GeometrySourceSummary
├── WarningCount
├── NormalizationDurationMs
└── NormalizedAt
```

ProviderRequestId should remain trace-only when cardinality/privacy policy requires.

---

## 22. `RECOGNITION_READING_ORDER_RESOLVED`

### Payload

```text
RecognitionReadingOrderResolvedEvent
├── OrderedRegionCount
├── ReadingDirection
├── OrderSource
├── ConfidenceLevel
├── WarningCount
├── DurationMs
└── ResolvedAt
```

### Rules

1. full order remains in Candidate/Artifact;
2. event is informational;
3. uncertain order explicit;
4. not a downstream trigger.

---

## 23. Warning and Error Facts

```text
RECOGNITION_WARNING_RECORDED
RECOGNITION_MODULE_ERROR_RECORDED
RECOGNITION_CANCELLATION_OBSERVED
RECOGNITION_DEADLINE_OBSERVED
```

These facts do not define Runtime terminal outcomes.

---

## 24. `RECOGNITION_WARNING_RECORDED`

### Payload

```text
RecognitionWarningRecordedEvent
├── WarningCode
├── Severity
├── OperationPhase
├── RegionId?
├── ProviderId?
├── Metadata?
└── RecordedAt
```

Rules:

- no full text;
- no provider raw response;
- duplicates may be aggregated;
- warning is not failure.

---

## 25. `RECOGNITION_MODULE_ERROR_RECORDED`

### Meaning

Recognition produced a normalized module error.

### Payload

```text
RecognitionModuleErrorRecordedEvent
├── ErrorCode
├── OperationPhase
├── Retryability
├── SuggestedStrategies[]
├── ProviderErrorCode?
├── ProviderId?
├── DiagnosticsRef?
└── RecordedAt
```

### Rules

1. not a terminal event;
2. Runtime may retry, cancel, abandon or fail Attempt;
3. no credential;
4. no full provider response;
5. no raw recognized text;
6. does not emit `recognition.failed`.

---

## 26. `RECOGNITION_CANCELLATION_OBSERVED`

### Payload

```text
RecognitionCancellationObservedEvent
├── OperationPhase
├── ProviderCancellationRequested
├── ProviderCancellationSupported
├── LocalWorkStopped
├── ObservedAt
└── Metadata?
```

This fact means Recognition observed CancellationContext.

It does not mean Runtime committed `ATTEMPT_CANCELED`.

---

## 27. `RECOGNITION_DEADLINE_OBSERVED`

### Payload

```text
RecognitionDeadlineObservedEvent
├── OperationPhase
├── DeadlineSource
├── RemainingBudgetMs?
├── Expired
├── ProviderTimeoutObserved
└── ObservedAt
```

Runtime owns final timeout disposition.

---

## 28. Diagnostic Facts

```text
RECOGNITION_DIAGNOSTIC_RECORDED
RECOGNITION_BENCHMARK_SAMPLE_RECORDED
RECOGNITION_INVARIANT_VIOLATION_RECORDED
```

Diagnostic facts:

- development/protected only;
- not normal workflow inputs;
- may contain references, not payloads;
- bounded retention;
- access controlled.

---

## 29. Candidate Event Boundary

Recognition Candidate facts describe only:

```text
Candidate assembled
Candidate validated
Candidate submitted
```

They do not describe:

```text
Candidate accepted
Ownership transferred
Artifact published
Artifact available
Artifact retained
Artifact evicted
```

Those belong to Runtime/Artifact Store/Resource Manager.

---

## 30. Runtime Event Boundary

Runtime owns canonical execution events:

```text
WORKITEM_CREATED
ATTEMPT_STARTED
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
ATTEMPT_ABANDONED
AUTHORITY_ACCEPTED
AUTHORITY_REJECTED
LATE_COMPLETION_REJECTED
```

Recognition must not duplicate them with module-specific terminal lifecycle events.

---

## 31. Artifact Store Event Boundary

Artifact Store owns:

```text
OWNERSHIP_TRANSFER_REQUESTED
OWNERSHIP_TRANSFERRED
ARTIFACT_PUBLISHED
ARTIFACT_PUBLICATION_REJECTED
RESOURCE_LOGICALLY_DISPOSED
RESOURCE_PHYSICALLY_DISPOSED
```

Text Processing starts from orchestrated published Artifact availability, not Recognition Candidate events.

---

## 32. Provider Manager Event Boundary

Provider Manager owns:

```text
PROVIDER_REGISTERED
PROVIDER_READY
PROVIDER_DEGRADED
PROVIDER_UNAVAILABLE
PROVIDER_CONFIGURATION_CHANGED
PROVIDER_DRAINING
PROVIDER_STOPPED
```

Recognition only observes provider snapshot/capability through contracts.

It does not publish:

```text
recognition.provider_ready
recognition.provider_degraded
recognition.provider_unavailable
```

---

## 33. Consumed Event Policy

Recognition should not subscribe directly to broad workflow events.

Recognition does not directly consume:

```text
source.image_imported
observation.stable_frame_ready
recognition.requested
recognition.cancellation_requested
session.stopped
source.closed
application.shutdown_requested
configuration.recognition_changed
```

Runtime Control, Business Pipeline Orchestration, Configuration Service hoặc component owner handles these facts.

Recognition receives explicit execution contracts:

```text
RecognitionAttemptInput
ExecutionContextRef
CancellationContextRef
PrivacyContextRef
ConfigurationSnapshotId
ProviderAvailabilitySnapshot
```

---

## 34. No Hidden Orchestration

Recognition Event handlers must not:

- create WorkItem;
- create Attempt;
- cancel Session;
- start retry;
- select current Revision;
- publish Artifact;
- trigger Text Processing;
- mutate Provider Manager;
- activate Configuration;
- stop Runtime.

Event subscription must never become hidden business pipeline orchestration.

---

## 35. Ordering

Preferred causal order for one Attempt:

```text
RECOGNITION_PLAN_CREATED
    ↓
RECOGNITION_PREPARATION_COMPLETED
    ↓
RECOGNITION_REGIONS_DETECTED
    ↓
RECOGNITION_PROVIDER_OUTPUT_NORMALIZED
    ↓
RECOGNITION_READING_ORDER_RESOLVED
    ↓
RECOGNITION_CANDIDATE_VALIDATED
    ↓
RECOGNITION_CANDIDATE_SUBMITTED
```

But:

- phases may be skipped;
- optional facts may be absent;
- delivery may be delayed;
- transport may duplicate;
- Runtime correctness must not depend on this sequence.

---

## 36. Delivery Guarantees

Assume at-least-once delivery when Event Bus persists/retries.

Possible effects:

- duplicate facts;
- delayed facts;
- missing sampled progress facts;
- out-of-order facts;
- fact arrives after Attempt terminal event;
- Candidate fact arrives after Runtime rejected Completion.

Consumers must handle safely.

---

## 37. Consumer Deduplication

Consumers maintain:

```text
ProcessedEvent
├── EventId
├── HandlerId
└── ProcessedAt
```

Duplicate EventId must not:

- update UI twice;
- create WorkItem;
- append duplicate diagnostic row;
- increment business counter twice.

Aggregate metrics may deduplicate or use idempotent counters according to telemetry design.

---

## 38. Late Facts

A Recognition fact may arrive after Runtime terminal outcome.

Example:

```text
ATTEMPT_CANCELED
    ↓
delayed RECOGNITION_REGIONS_DETECTED
```

Consumer behavior:

- keep for diagnostics if useful;
- do not change Runtime state;
- do not trigger downstream processing;
- do not publish Artifact;
- mark late when trace context allows.

---

## 39. Stale Completion

Staleness is not decided by Recognition event consumer.

Canonical flow:

```text
Attempt Completion
    ↓
Runtime Authority Validation
    ↓
Accepted or Rejected Stale
```

Recognition facts preserve:

```text
RevisionId
WorkItemId
AttemptId
```

Runtime owns stale rejection.

---

## 40. Privacy Classification

Normal Recognition facts:

```text
OPERATIONAL_METADATA
```

Allowed:

- Runtime IDs;
- provider ID;
- counts;
- duration;
- warning/error code;
- confidence level;
- quality/completeness;
- CandidateArtifactId;
- sanitized diagnostics reference.

Forbidden:

```text
image_bytes
image_base64
raw_image_path
complete_raw_text
complete_surface_text
provider_api_key
provider_access_token
authorization_header
provider_full_response
temporary_file_credentials
translated_text
user_glossary_content
```

---

## 41. Geometry in Events

Geometry may be included only when:

- bounded;
- required by a concrete trusted consumer;
- privacy classification allows;
- event channel is approved;
- no text content accompanies it.

Default public facts use counts/summaries only.

---

## 42. Remote Provider Disclosure

When remote provider is used, safe facts may include:

```text
ProviderId
ExecutionLocation = REMOTE_SERVICE
PrivacyClassification
RemoteExecutionUsed = true
```

No image/text payload included.

Remote provider use is primarily recorded in Candidate provenance and Runtime Observability.

---

## 43. Diagnostic Event Security

Protected diagnostics may reference:

- processed image Artifact;
- provider raw-response Artifact;
- region visualization Artifact;
- benchmark sample;
- comparison report.

Requirements:

- explicit diagnostic mode;
- authorization;
- secure Artifact reference;
- bounded retention;
- redaction;
- auditability;
- no direct unrestricted file path.

Normal modules must not consume protected diagnostic stream.

---

## 44. Event Publication Failure

If a Recognition fact cannot be published:

```text
Record bounded local diagnostic if possible
    ↓
Retry/drop according to Event Bus policy
    ↓
Continue Recognition correctness path
```

Rules:

1. do not rerun Recognition;
2. do not fail Attempt solely due to optional fact failure;
3. do not block Runtime Control;
4. do not block Candidate submission;
5. reuse EventId for idempotent retry;
6. increment dropped-event metric when applicable.

Artifact publication failure is separate and belongs to Artifact Store.

---

## 45. Event Bus Unavailable

Recognition execution must continue if:

- Attempt contract is in-process/direct;
- Candidate can be submitted through Runtime Completion;
- correctness does not require events.

When Event Bus unavailable:

- suppress optional facts;
- preserve critical diagnostics locally if bounded;
- report observability degradation;
- do not invent terminal event fallback.

---

## 46. Consumer Expectations

### Diagnostics

May consume all Recognition facts.

Uses:

- phase timing;
- region count;
- warning/error rate;
- quality/completeness distribution;
- provider normalization behavior;
- Candidate validation;
- cancellation observation;
- late fact detection.

Diagnostics must not infer semantic OCR quality from operational success alone.

### Presentation

May consume safe progress summaries for optional developer/progress UI.

Must not:

- treat progress fact as authoritative result;
- start Translation;
- display full recognized text from Event Bus;
- couple to provider details.

### Text Processing

Must not consume Recognition facts for correctness.

Text Processing receives published `RecognitionArtifact` through Runtime orchestration.

### Session / Business Orchestration

Consumes Runtime lifecycle and authority events, not Recognition terminal events.

### Provider Manager

Does not consume Recognition facts to change provider lifecycle automatically unless an explicit Runtime health policy exists outside this module.

---

## 47. Event Contract Examples

### Plan Created

```json
{
  "event_id": "evt_rec_plan_001",
  "event_name": "RECOGNITION_PLAN_CREATED",
  "contract_version": "1.0.0",
  "producer_module": "recognition",
  "occurred_at": "2026-08-03T01:15:42.190Z",
  "privacy_classification": "OPERATIONAL_METADATA",
  "partition_key": "attempt_01",
  "sequence_number": 1,
  "trace_context": {
    "trace_id": "trace_01"
  },
  "runtime_context": {
    "application_instance_id": "app_01",
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_recognition_104",
    "attempt_id": "attempt_01",
    "configuration_snapshot_id": "config_42",
    "recognition_operation": "RECOGNIZE_IMAGE",
    "recognition_profile": "COMIC_PAGE",
    "operation_phase": "PLANNING"
  },
  "payload": {
    "plan_id": "plan_rec_104_01",
    "strategy": "COMBINED_RECOGNITION",
    "recognition_operation": "RECOGNIZE_IMAGE",
    "recognition_profile": "COMIC_PAGE",
    "preparation_profile_id": "comic_default_v1",
    "coordinate_policy_version": "1",
    "reading_order_policy_version": "comic_mixed_v1",
    "quality_policy_version": "interactive_v1",
    "privacy_mode": "LOCAL_ONLY",
    "execution_classes": ["GPU", "CPU"],
    "created_at": "2026-08-03T01:15:42.190Z"
  }
}
```

### Candidate Validated

```json
{
  "event_id": "evt_rec_candidate_valid_001",
  "event_name": "RECOGNITION_CANDIDATE_VALIDATED",
  "contract_version": "1.0.0",
  "producer_module": "recognition",
  "occurred_at": "2026-08-03T01:15:42.871Z",
  "privacy_classification": "OPERATIONAL_METADATA",
  "partition_key": "attempt_01",
  "sequence_number": 5,
  "trace_context": {
    "trace_id": "trace_01"
  },
  "runtime_context": {
    "application_instance_id": "app_01",
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_recognition_104",
    "attempt_id": "attempt_01",
    "candidate_artifact_id": "candidate_recognition_104",
    "provider_id": "local_recognition_01",
    "configuration_snapshot_id": "config_42",
    "recognition_operation": "RECOGNIZE_IMAGE",
    "recognition_profile": "COMIC_PAGE",
    "operation_phase": "VALIDATING_CANDIDATE"
  },
  "payload": {
    "candidate_artifact_id": "candidate_recognition_104",
    "artifact_type": "RECOGNITION_ARTIFACT",
    "completeness": "COMPLETE",
    "quality_level": "DEGRADED",
    "region_count": 12,
    "line_count": 18,
    "character_count": 143,
    "warning_count": 1,
    "provider_provenance_summary": {
      "provider_id": "local_recognition_01",
      "execution_location": "LOCAL_PROCESS",
      "execution_class": "GPU"
    },
    "compatibility_metadata_version": "1",
    "validation_duration_ms": 3,
    "validated_at": "2026-08-03T01:15:42.871Z"
  }
}
```

---

## 48. MVP Event Set

MVP correctness requires no Recognition-specific event.

Recommended optional MVP facts:

```text
RECOGNITION_CANDIDATE_VALIDATED
RECOGNITION_WARNING_RECORDED
RECOGNITION_MODULE_ERROR_RECORDED
```

Development-only optional facts:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_PREPARATION_COMPLETED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_READING_ORDER_RESOLVED
```

Do not add an event without a concrete consumer.

---

## 49. Deferred Event Extensions

Potential future facts:

```text
RECOGNITION_PARTIAL_CANDIDATE_VALIDATED
RECOGNITION_LONG_PAGE_CHUNK_PROCESSED
RECOGNITION_PROVIDER_COMPARISON_RECORDED
RECOGNITION_QUALITY_EVALUATED
RECOGNITION_STREAM_SEGMENT_RECEIVED
RECOGNITION_MANUAL_REVIEW_REQUESTED
```

Deferred because they introduce:

- streaming semantics;
- partial Artifact lifecycle;
- extra retention;
- consumer complexity;
- stronger ordering requirements.

---

## 50. Testing Requirements

### Envelope

- unique EventId;
- supported ContractVersion;
- UTC timestamp;
- producer_module = recognition;
- Runtime context complete;
- no credential;
- privacy classification present.

### Domain Facts

- Plan fact after READY Plan;
- Candidate valid fact after semantic validation;
- Candidate submit fact only once;
- empty-valid Candidate summary;
- partial Candidate summary;
- no authoritative publication implication.

### Progress Facts

- optional facts may be omitted;
- duplicate delivery safe;
- out-of-order delivery safe;
- bounded region facts;
- no text content;
- no image content.

### Warning/Error Facts

- warning separate from error;
- module error not terminal event;
- cancellation observed not Attempt canceled;
- deadline observed not terminal state;
- retry hint sanitized.

### Boundaries

- no Recognition terminal lifecycle event;
- no provider lifecycle event;
- no WorkItem creation from fact;
- no Text Processing trigger;
- no Artifact publication from fact.

### Privacy

- no image bytes;
- no complete OCR text;
- no raw provider response;
- no credentials;
- no temporary sensitive path;
- remote use disclosed safely.

### Failure

- Event Bus unavailable does not fail Recognition;
- event retry uses same EventId;
- dropped fact recorded;
- Candidate submission unaffected.

---

## 51. Event Invariants

1. Recognition facts are immutable.
2. Every fact has unique EventId.
3. Every fact declares ContractVersion.
4. Every fact contains RevisionId, WorkItemId and AttemptId.
5. Recognition emits no terminal Attempt event.
6. Recognition emits no authoritative Artifact publication event.
7. Recognition emits no provider lifecycle event.
8. Recognition facts do not create WorkItem.
9. Recognition facts do not trigger retry.
10. Recognition facts do not cancel Session.
11. Recognition facts do not grant authority.
12. Candidate validated does not mean published.
13. Candidate submitted does not mean accepted.
14. Progress facts are optional.
15. Consumers do not depend on progress facts for correctness.
16. Duplicate delivery does not duplicate business work.
17. Global ordering is not assumed.
18. Late facts do not change Runtime state.
19. Raw image never appears in normal facts.
20. Complete recognized text never appears in normal facts.
21. Provider credentials never appear.
22. Provider SDK types never appear.
23. Event publication failure does not rerun Recognition.
24. Event publication failure does not block Candidate submission.
25. Remote execution is disclosed safely.
26. Geometry payload is bounded and opt-in.
27. Warning is not failure.
28. Module error fact is not terminal outcome.
29. Cancellation observed is not cancellation authority.
30. Text Processing consumes published Artifact, not Recognition event.
31. ArtifactRef appears only when external owner has published it.
32. CandidateArtifactId may appear before publication.
33. Recognition does not consume broad workflow events directly.
34. Runtime correctness is Event Bus independent.
35. Diagnostic events have stricter access and retention.
36. Transport topic naming does not replace canonical event identity.
37. Unknown event minor additions are handled safely.
38. Unsupported major version is rejected.
39. Event metadata remains bounded.
40. No hidden orchestration through subscriptions.

---

## 52. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/STATES.md
doc/02-modules/recognition/ERRORS.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RETRY_POLICY.md

doc/01-architecture/ocr/PIPELINE.md
doc/01-architecture/ocr/QUALITY.md
doc/01-architecture/ocr/PROVIDERS.md
```

---

## 53. Summary

Recognition events now communicate optional module facts:

```text
Plan Created
    ↓
Progress Facts
    ↓
Candidate Validated
    ↓
Candidate Submitted
```

They do not communicate authoritative execution lifecycle.

Runtime owns:

```text
Attempt Outcome
Authority
Cancellation
Retry
```

Artifact Store owns:

```text
Ownership Transfer
Publication
Artifact Availability
```

Provider Manager owns:

```text
Provider Lifecycle
Provider Health
Provider Capacity
```

The central rule is:

```text
Recognition events explain what Recognition observed or produced.

They never decide whether the work still matters,
and they never publish the authoritative result.
```
