# Recognition Module Errors

> Project: CRAI  
> Module: Recognition  
> Path: `doc/02-modules/recognition/ERRORS.md`  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa error và warning contract của Recognition Module theo Runtime v2.

Nó đặc tả:

- error ownership;
- stable error-code format;
- RecognitionModuleError;
- Warning contract;
- RetryHint;
- ProviderErrorRef;
- severity;
- input/image/capability/preparation/detection/recognition/coordinate/ordering/candidate/resource/internal errors;
- cancellation và deadline boundary;
- Runtime disposition boundary;
- logging;
- metrics;
- privacy;
- compatibility;
- testing;
- invariants.

Recognition chịu trách nhiệm nhận dạng text và spatial structure từ image-based input.

Recognition không chịu trách nhiệm:

- Capture;
- Observation;
- WorkItem/Attempt lifecycle;
- Scheduler;
- Queue;
- Runtime retry;
- cancellation authority;
- Provider lifecycle;
- Artifact publication;
- durable persistence;
- Translation;
- Presentation.

---

## 2. Error Ownership

Recognition owns:

```text
RecognitionModuleError
RecognitionWarning
RetryHint
ProviderError normalization
Candidate validation errors
Recognition semantic error codes
```

Recognition does not own:

```text
Attempt terminal outcome
Retry scheduling
Cancellation terminal outcome
Queue overflow policy
Scheduler admission failure
Provider lifecycle failure
Model lifecycle ownership
Artifact publication failure
Artifact retention failure
Storage failure
Runtime shutdown failure
```

Recognition may reference external errors through normalized references.

---

## 3. Error Principles

### 3.1 Stable Error Codes

Consumers rely on stable error codes, not implementation exceptions.

### 3.2 Recognition Never Guesses

When output is uncertain:

- preserve uncertainty;
- emit warning;
- lower quality state;
- produce RetryHint when useful;
- never silently fabricate text or geometry.

### 3.3 Preserve Input

Recognition failure never mutates source image or published input Artifact.

### 3.4 Error Is Not Warning

A warning describes degraded-but-usable output.

An error means Recognition cannot produce a semantically valid Candidate for the requested operation.

### 3.5 Runtime Owns Disposition

Recognition identifies and normalizes failure.

Runtime decides:

- fail Attempt;
- retry;
- fallback;
- cancel;
- abandon;
- reject stale output.

### 3.6 Privacy

Errors must never expose:

- image bytes;
- full recognized text;
- browser content;
- personal information;
- credentials;
- authorization header;
- full provider response;
- sensitive temporary path.

### 3.7 Candidate Boundary

Candidate assembly/validation errors belong to Recognition.

Ownership-transfer/publication errors belong to Runtime/Artifact Store.

---

## 4. Recognition Module Error Contract

```text
RecognitionModuleError
├── ContractVersion
├── ErrorCode
├── SymbolicName
├── Category
├── Severity
├── OperationPhase
├── MessageKey
├── RetryHint?
├── ProviderErrorRef?
├── AffectedRegionId?
├── CandidateArtifactId?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

### Rules

1. ErrorCode stable within major version.
2. SymbolicName human-readable and stable.
3. MessageKey suitable for localization.
4. Provider SDK exception never crosses boundary.
5. RetryHint advisory only.
6. Metadata bounded and sanitized.
7. Full text/image forbidden.
8. OperationPhase identifies failure location.
9. Error does not embed Runtime terminal state.
10. Error does not publish Artifact.

---

## 5. Stable Error-Code Format

```text
REC-<CATEGORY>-<NUMBER>
```

Examples:

```text
REC-INPUT-001
REC-IMAGE-002
REC-DETECT-001
REC-REC-001
REC-CAND-002
REC-INT-001
```

Each code maps to one symbolic name.

Example:

```text
REC-REC-001
    ↔ RECOGNITION_TEXT_FAILED
```

Code meaning must not change silently.

---

## 6. Error Categories

| Prefix | Category |
|---|---|
| `INPUT` | Attempt input validation |
| `IMAGE` | Image validation |
| `CAP` | Capability resolution |
| `PREP` | Image preparation |
| `DETECT` | Region detection |
| `REC` | Text recognition |
| `COORD` | Coordinate mapping |
| `ORDER` | Reading order |
| `CAND` | Candidate assembly/validation |
| `RES` | Recognition-local resource |
| `PROV` | Provider-output normalization |
| `STATE` | Recognition-owned state invariant |
| `INT` | Internal invariant/failure |

Queue, Scheduler, publication và Provider lifecycle không dùng Recognition category.

---

## 7. Severity

```text
RecognitionErrorSeverity
├── INFORMATION
├── WARNING
├── ERROR
└── CRITICAL
```

### INFORMATION

Expected technical condition recorded as error-like diagnostic, rarely used for ModuleError.

### WARNING

Recoverable/degraded condition only when no valid Candidate can be produced under current policy.

Most degraded-but-usable conditions should use `RecognitionWarning`, not ModuleError.

### ERROR

Attempt cannot produce valid Candidate.

### CRITICAL

Recognition invariant/corruption boundary violated.

Critical does not automatically mean whole module enters failed state.

Runtime/Container decides degradation, drain or restart.

---

## 8. Retry Hint Contract

```text
RetryHint
├── Retryability
├── SuggestedStrategies[]
├── SuggestedDelayMs?
├── AlternativeProviderAllowed
├── AlternativePreparationAllowed
├── RegionOnlyAllowed
└── ReasonCode
```

```text
Retryability
├── RETRYABLE
├── CONDITIONALLY_RETRYABLE
└── NON_RETRYABLE
```

```text
RetryStrategy
├── SAME_PROVIDER
├── ALTERNATIVE_PROVIDER
├── ALTERNATIVE_PREPARATION
├── REGION_ONLY
├── RESOURCE_WAIT
└── NO_RETRY
```

Recognition does not:

- create retry Attempt;
- choose retry time;
- consume retry budget;
- choose final fallback;
- bypass authority validation.

---

## 9. Warning Contract

```text
RecognitionWarning
├── WarningCode
├── Severity
├── OperationPhase
├── MessageKey
├── RegionId?
├── ProviderId?
├── Metadata?
└── RecordedAt
```

Warnings:

- can coexist with valid Candidate;
- do not create failure state;
- do not trigger retry automatically;
- contribute to QualityMetadata;
- remain separate from ModuleError.

---

## 10. Standard Warning Codes

```text
NO_READABLE_TEXT_DETECTED
LOW_IMAGE_QUALITY
LOW_DETECTION_CONFIDENCE
LOW_RECOGNITION_CONFIDENCE
READING_ORDER_UNCERTAIN
REGION_GEOMETRY_INFERRED
LINE_GEOMETRY_UNAVAILABLE
OVERLAPPING_REGIONS_SUPPRESSED
DUPLICATE_REGION_SUPPRESSED
PROVIDER_CONFIDENCE_UNAVAILABLE
IMAGE_UPSCALED
IMAGE_DOWNSCALED
IMAGE_ROTATED
REMOTE_PROVIDER_USED
FALLBACK_PROVIDER_USED
PARTIAL_RECOGNITION
PREPARATION_FALLBACK_USED
MIXED_ORIENTATION_DETECTED
MIXED_LANGUAGE_DETECTED
OUTPUT_TRUNCATED
```

---

## 11. Warning vs Error Rules

### No readable text

```text
Completeness = EMPTY_VALID
Warning = NO_READABLE_TEXT_DETECTED
```

Not error.

### Low confidence

```text
Warning = LOW_RECOGNITION_CONFIDENCE
Quality = DEGRADED
```

Error only when quality policy concludes no valid Candidate can be produced.

### Reading order uncertain

Normally warning.

Error only when requested contract requires valid explicit order and no acceptable fallback exists.

### Overlapping regions

Normally normalize/suppress + warning.

Error only when geometry becomes unusable.

### Image too small

May be warning when upscaling path exists.

Error when no supported preparation can satisfy minimum input requirements.

---

## 12. Input Errors

### `REC-INPUT-001 — RECOGNITION_INPUT_INVALID`

Meaning:

RecognitionAttemptInput is malformed or internally inconsistent.

Examples:

- missing Runtime identity;
- missing operation;
- missing privacy context;
- conflicting options;
- impossible capability requirement.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

unless caller corrects input.

---

### `REC-INPUT-002 — RECOGNITION_ARTIFACT_UNAVAILABLE`

Meaning:

Input ArtifactRef cannot be resolved or leased.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested strategies:

```text
RESOURCE_WAIT
NO_RETRY
```

depending on reason.

---

### `REC-INPUT-003 — RECOGNITION_ARTIFACT_TYPE_UNSUPPORTED`

Meaning:

Input Artifact type is not supported by Recognition.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

### `REC-INPUT-004 — RECOGNITION_COORDINATE_SPACE_INVALID`

Meaning:

Source CoordinateSpace is malformed or inconsistent with image metadata.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

### `REC-INPUT-005 — RECOGNITION_REGION_INVALID`

Meaning:

Requested RegionSelection is invalid or outside source coordinate space.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Correction required.

---

### `REC-INPUT-006 — RECOGNITION_CONTRACT_VERSION_UNSUPPORTED`

Meaning:

Recognition Attempt uses unsupported major contract version.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

## 13. Image Errors

### `REC-IMAGE-001 — RECOGNITION_IMAGE_INVALID`

Meaning:

Image metadata or decoded content is invalid.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested strategy:

```text
ALTERNATIVE_PREPARATION
```

when decode/preparation path may differ.

---

### `REC-IMAGE-002 — RECOGNITION_IMAGE_FORMAT_UNSUPPORTED`

Meaning:

Image format is unsupported by all eligible Recognition paths.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

unless upstream converts format.

---

### `REC-IMAGE-003 — RECOGNITION_IMAGE_TOO_LARGE`

Meaning:

Image exceeds safe Recognition processing limits.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested strategies:

```text
REGION_ONLY
ALTERNATIVE_PREPARATION
```

---

### `REC-IMAGE-004 — RECOGNITION_IMAGE_TOO_SMALL`

Meaning:

Image does not contain enough resolution for requested quality and no acceptable upscale path exists.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Do not use this error when Candidate can still be produced with `LOW_IMAGE_QUALITY`.

---

### `REC-IMAGE-005 — RECOGNITION_IMAGE_CONTENT_UNUSABLE`

Meaning:

Image is technically valid but unusable for Recognition.

Examples:

- fully transparent;
- entirely blank under configured policy;
- unrecoverably corrupted visual content.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

Do not confuse with no readable text.

---

## 14. Capability Errors

### `REC-CAP-001 — RECOGNITION_CAPABILITY_UNAVAILABLE`

Meaning:

No eligible provider/capability path satisfies the Recognition requirements.

Examples:

- vertical text required but unavailable;
- local-only required but only remote providers exist;
- language unsupported;
- required line geometry unavailable.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested strategies:

```text
ALTERNATIVE_PROVIDER
RESOURCE_WAIT
```

---

### `REC-CAP-002 — RECOGNITION_LANGUAGE_UNSUPPORTED`

Meaning:

Required language cannot be processed by eligible providers.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

unless requirements/config change.

---

### `REC-CAP-003 — RECOGNITION_SCRIPT_UNSUPPORTED`

Meaning:

Required script capability unavailable.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

### `REC-CAP-004 — RECOGNITION_ORIENTATION_UNSUPPORTED`

Meaning:

Required orientation cannot be processed under current capability policy.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PREPARATION
ALTERNATIVE_PROVIDER
```

---

### `REC-CAP-005 — RECOGNITION_PRIVACY_CONFLICT`

Meaning:

Requested execution path violates PrivacyContext.

Examples:

- local-only but selected path is remote;
- EPHEMERAL mode conflicts with required persistence;
- protected diagnostics not authorized.

Severity:

```text
CRITICAL
```

for attempted policy violation; otherwise `ERROR`.

Retry:

```text
NON_RETRYABLE
```

until policy/path changes.

---

## 15. Preparation Errors

### `REC-PREP-001 — RECOGNITION_PREPARATION_FAILED`

Meaning:

Image preparation pipeline cannot produce valid provider input.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PREPARATION
ALTERNATIVE_PROVIDER
```

---

### `REC-PREP-002 — RECOGNITION_TRANSFORM_INVALID`

Meaning:

Preparation transform chain is invalid or non-invertible where inverse mapping is required.

Severity:

```text
CRITICAL
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PREPARATION
```

---

### `REC-PREP-003 — RECOGNITION_PREPARATION_OUTPUT_INVALID`

Meaning:

Prepared image dimensions, format or metadata violate provider/Recognition contract.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## 16. Detection Errors

### `REC-DETECT-001 — RECOGNITION_DETECTION_FAILED`

Meaning:

Text-region detection failed to produce a valid output.

Severity:

```text
ERROR
```

Retry:

```text
RETRYABLE
```

Suggested:

```text
SAME_PROVIDER
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPARATION
```

---

### `REC-DETECT-002 — RECOGNITION_DETECTION_OUTPUT_INVALID`

Meaning:

Detector output is malformed or cannot be normalized.

Examples:

- invalid geometry;
- duplicate invalid IDs;
- out-of-bounds regions;
- unsupported coordinate semantics.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
```

---

### `REC-DETECT-003 — RECOGNITION_REGION_SEGMENTATION_FAILED`

Meaning:

Composed strategy cannot create usable Recognition regions.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPARATION
```

---

## 17. Text Recognition Errors

### `REC-REC-001 — RECOGNITION_TEXT_FAILED`

Meaning:

Text-recognition operation failed.

Severity:

```text
ERROR
```

Retry:

```text
RETRYABLE
```

depending on provider error.

Suggested:

```text
SAME_PROVIDER
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPARATION
```

---

### `REC-REC-002 — RECOGNITION_TEXT_OUTPUT_INVALID`

Meaning:

Recognized text output is malformed or violates normalization contract.

Examples:

- invalid encoding;
- impossible line references;
- unsupported provider output shape;
- non-normalizable text structure.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
```

---

### `REC-REC-003 — RECOGNITION_OUTPUT_UNUSABLE`

Meaning:

Provider produced output, but Recognition quality policy considers it unusable and no degraded Candidate is allowed.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPARATION
REGION_ONLY
```

---

### `REC-REC-004 — RECOGNITION_PROVIDER_TIMEOUT`

Meaning:

Recognition provider exceeded provider-specific execution timeout.

Severity:

```text
ERROR
```

Retry:

```text
RETRYABLE
```

Suggested:

```text
SAME_PROVIDER
ALTERNATIVE_PROVIDER
```

Runtime still owns Attempt deadline/terminal disposition.

---

## 18. Coordinate Errors

### `REC-COORD-001 — RECOGNITION_COORDINATE_MAPPING_FAILED`

Meaning:

Processed-space geometry cannot be mapped safely to source space.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PREPARATION
```

---

### `REC-COORD-002 — RECOGNITION_GEOMETRY_OUT_OF_BOUNDS`

Meaning:

Normalized public geometry falls outside source CoordinateSpace beyond allowed tolerance.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

### `REC-COORD-003 — RECOGNITION_GEOMETRY_INVALID`

Meaning:

Geometry is empty, malformed or inconsistent.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## 19. Reading-Order Errors

### `REC-ORDER-001 — RECOGNITION_READING_ORDER_FAILED`

Meaning:

Recognition cannot produce required valid reading order.

Severity:

```text
ERROR
```

only when valid reading order is required by contract.

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
ALTERNATIVE_PREPARATION
```

When uncertain order is usable:

```text
Warning = READING_ORDER_UNCERTAIN
```

not error.

---

### `REC-ORDER-002 — RECOGNITION_READING_ORDER_INVALID`

Meaning:

ReadingOrder entries reference missing/duplicate regions or invalid indices.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## 20. Candidate Errors

### `REC-CAND-001 — RECOGNITION_CANDIDATE_ASSEMBLY_FAILED`

Meaning:

Recognition cannot assemble Candidate Recognition Artifact.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

This is not Artifact publication failure.

---

### `REC-CAND-002 — RECOGNITION_CANDIDATE_INVALID`

Meaning:

Candidate failed Recognition semantic validation.

Examples:

- duplicate RegionId;
- invalid ReadingOrder reference;
- invalid geometry;
- missing provider provenance;
- invalid completeness;
- missing compatibility metadata;
- SDK object leaked into Metadata.

Severity:

```text
ERROR
```

or `CRITICAL` for invariant corruption.

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

### `REC-CAND-003 — RECOGNITION_CANDIDATE_PRIVACY_VIOLATION`

Meaning:

Candidate contains forbidden/sensitive data.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

Candidate must not be submitted.

---

### `REC-CAND-004 — RECOGNITION_CANDIDATE_SUBMISSION_FAILED`

Meaning:

Recognition cannot hand Candidate to Runtime Completion boundary because of local contract/serialization failure.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Do not use for Runtime authority rejection or Artifact Store publication failure.

---

## 21. Recognition-Local Resource Errors

### `REC-RES-001 — RECOGNITION_RESOURCE_EXHAUSTED`

Meaning:

Recognition cannot allocate required Attempt-local resources within provided budget.

Examples:

- preparation buffer allocation;
- region batch allocation;
- local native working memory.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
RESOURCE_WAIT
ALTERNATIVE_PREPARATION
REGION_ONLY
```

---

### `REC-RES-002 — RECOGNITION_INPUT_LEASE_FAILED`

Meaning:

Input Artifact Lease cannot be acquired or becomes invalid before use.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
RESOURCE_WAIT
```

---

### `REC-RES-003 — RECOGNITION_LOCAL_RESOURCE_CLEANUP_FAILED`

Meaning:

Attempt-local Recognition resource cannot be released cleanly.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for same Attempt.

Runtime/Resource Manager may degrade or drain component.

---

## 22. Errors Not Owned by Recognition

Do not define Recognition codes for:

```text
QueueOverflow
SchedulerAdmissionRejected
WorkItemCanceled
AttemptAbandoned
RuntimeDeadlineExpired
ProviderModelInitializationFailed
ProviderHealthUnavailable
GPUGloballyUnavailable
ArtifactPublicationFailed
OwnershipTransferFailed
CacheEvictionFailed
StorageWriteFailed
ApplicationShutdownFailed
```

These must use canonical errors from owning component.

Recognition may receive/reference them through:

```text
RuntimeErrorRef
ProviderErrorRef
ResourceErrorRef
```

---

## 23. Provider Error Reference

```text
ProviderErrorRef
├── ProviderId
├── ProviderErrorCode
├── ProviderCategory
├── Retryability
├── SanitizedMessageKey
├── ProviderRequestId?
├── DiagnosticsRef?
└── OccurredAt
```

Rules:

1. no raw exception;
2. no credential;
3. no full response;
4. no prompt/image/text payload;
5. provider code remains traceable;
6. Recognition maps it to semantic module error where needed;
7. Provider Manager remains owner of provider health/lifecycle.

---

## 24. Provider Output Errors

### `REC-PROV-001 — RECOGNITION_PROVIDER_OUTPUT_INVALID`

Meaning:

Provider returned data that cannot be normalized into Recognition contract.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
```

---

### `REC-PROV-002 — RECOGNITION_PROVIDER_CAPABILITY_MISMATCH`

Meaning:

Selected provider behavior does not match declared capability.

Examples:

- line geometry required but omitted;
- vertical text advertised but unsupported;
- local-only guarantee violated;
- confidence format differs from declared contract.

Severity:

```text
CRITICAL
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
```

Provider Manager should receive health/capability diagnostic.

---

### `REC-PROV-003 — RECOGNITION_PROVIDER_PROTOCOL_FAILED`

Meaning:

Provider request/response protocol fails within Recognition adapter.

Severity:

```text
ERROR
```

Retry:

```text
RETRYABLE
```

depending on ProviderErrorRef.

---

## 25. Recognition-Owned State Errors

### `REC-STATE-001 — RECOGNITION_STATE_INVARIANT_VIOLATION`

Meaning:

Recognition-owned Plan, phase or Candidate state transition violates `STATES.md`.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for same Attempt.

Possible Runtime response:

- fail Attempt;
- degrade module;
- drain component;
- collect diagnostics.

Recognition does not transition itself to a canonical `FAILED` state.

---

### `REC-STATE-002 — RECOGNITION_DUPLICATE_CANDIDATE_SUBMISSION`

Meaning:

Same Candidate is submitted more than once from Recognition execution.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

---

## 26. Internal Errors

### `REC-INT-001 — RECOGNITION_INTERNAL_ERROR`

Meaning:

Unexpected Recognition implementation failure.

Severity:

```text
CRITICAL
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

A new Attempt may succeed, but Runtime decides.

---

### `REC-INT-002 — RECOGNITION_INVARIANT_VIOLATION`

Meaning:

Recognition domain invariant is violated.

Examples:

- source mapping lost;
- raw text overwritten;
- provider SDK object escapes boundary;
- Candidate mutated after validation;
- local-only path attempts remote execution.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

until implementation/configuration corrected.

---

### `REC-INT-003 — RECOGNITION_NORMALIZATION_FAILED`

Meaning:

Provider-independent normalization fails unexpectedly.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested:

```text
ALTERNATIVE_PROVIDER
```

---

## 27. Removed Legacy Errors

The following legacy errors are removed or moved:

```text
RecognitionAlreadyRunning
    → removed; bounded concurrent Attempts are Runtime-managed

RecognitionNotStarted
    → removed; no request-oriented singleton lifecycle

QueueOverflow
    → Work Queue owner

GPUUnavailable
    → Provider Manager / Resource Manager

ModelUnavailable
ModelInitializationFailed
UnsupportedModel
ModelExecutionFailed
    → Provider Manager / ProviderErrorRef

AtomicCommitFailed
    → Artifact Store publication/ownership error

Timeout
    → split into provider timeout or Runtime deadline

NoTextDetected
    → EMPTY_VALID + warning

LowConfidence
    → warning/quality state

ReadingOrderUnknown
    → warning unless strict contract fails

OverlappingRegions
    → warning when safely normalized
```

---

## 28. Cancellation Boundary

Cancellation is not a Recognition ModuleError by default.

Recognition may observe:

```text
CancellationContext.IsCancellationRequested = true
```

Then:

- stop starting new expensive work;
- request provider cancellation when supported;
- release Attempt-local resources;
- return cancellation observation;
- avoid Candidate submission.

Runtime decides:

```text
ATTEMPT_CANCELED
ATTEMPT_ABANDONED
ATTEMPT_FAILED
```

Do not emit a Recognition terminal cancellation error.

---

## 29. Deadline Boundary

Deadline belongs to Runtime ExecutionContext.

Recognition can produce:

### Provider timeout

```text
REC-REC-004 — RECOGNITION_PROVIDER_TIMEOUT
```

### Phase cannot start before deadline

Recognition returns safe module/deadline observation or RuntimeErrorRef according to runtime contract.

It does not:

- own global timeout;
- cancel WorkItem lineage;
- decide retry;
- convert every deadline into Recognition error.

---

## 30. Error-to-Runtime Disposition

```text
RecognitionModuleError
        ↓
Runtime Error Normalization
        ↓
Retry Policy / Cancellation / Authority Evaluation
        ↓
Attempt Disposition
```

Possible Runtime dispositions:

```text
ATTEMPT_FAILED
ATTEMPT_CANCELED
ATTEMPT_ABANDONED
RETRY_SCHEDULED
FALLBACK_ATTEMPT_CREATED
REJECTED_STALE
```

Recognition does not map directly to a terminal state.

---

## 31. Error and Candidate Relationship

```text
Valid Candidate
    → no ModuleError
    → warnings allowed

Invalid Candidate
    → RecognitionModuleError
    → Candidate not submitted

Cancellation observed before Candidate
    → no Candidate
    → Runtime decides outcome

Runtime rejects valid Candidate as stale
    → no RecognitionModuleError
```

Stale rejection is not Recognition failure.

---

## 32. Logging Contract

Safe structured fields:

```text
ErrorCode
SymbolicName
Category
Severity
OperationPhase
ApplicationInstanceId
SessionId?
RevisionId
WorkItemId
AttemptId
InputArtifactId?
CandidateArtifactId?
ProviderId?
ProviderErrorCode?
Retryability
TraceId
DurationMs?
OccurredAt
```

Forbidden:

```text
image_bytes
image_base64
recognized_text
surface_text
browser_content
personal_information
provider_api_key
provider_access_token
authorization_header
full_provider_response
sensitive_file_path
```

---

## 33. Metrics

Recognition-owned metrics:

```text
recognition.error.total
recognition.error.by_code
recognition.error.by_category
recognition.error.by_phase
recognition.error.critical_total
recognition.retry_hint.total
recognition.warning.total
recognition.warning.by_code
recognition.empty_valid.total
recognition.partial_total
recognition.low_confidence_warning_total
recognition.candidate_invalid_total
recognition.provider_output_invalid_total
recognition.invariant_violation_total
recognition.cleanup_failure_total
```

Not Recognition-owned:

```text
queue_overflow_total
scheduler_rejection_total
provider_health_state
gpu_unavailable_total
artifact_publication_failure_total
runtime_deadline_total
```

---

## 34. Error Observability

Every ModuleError should be traceable to:

```text
RevisionId
WorkItemId
AttemptId
OperationPhase
ConfigurationSnapshotId
ProviderId?
TraceId
```

Correlation IDs stay in trace/log, not high-cardinality metric labels.

---

## 35. Privacy

Error metadata must be content-free by default.

Protected diagnostics may contain Artifact references only when:

- explicit diagnostic mode;
- authorization;
- secure storage;
- bounded retention;
- redaction;
- audit trail.

Never embed full content directly in error contract.

---

## 36. Error Contract Evolution

### Backward-compatible

Allowed:

- new error codes;
- new optional Metadata keys;
- new RetryStrategy values with safe unknown handling;
- new warning codes;
- clarification without semantic change.

### Breaking

Requires major version:

- changing existing error meaning;
- changing retryability semantics;
- changing severity semantics materially;
- removing/renaming stable code;
- changing privacy guarantee;
- changing error ownership boundary.

Unknown codes:

- preserve raw stable code;
- map category when possible;
- treat unknown severity safely;
- do not crash;
- reject unsupported major version.

---

## 37. Testing Requirements

### Contract

- every code unique;
- every code maps one symbolic name;
- category matches code prefix;
- severity valid;
- RetryHint valid;
- MessageKey present;
- no Runtime terminal state embedded.

### Warning vs Error

- no text → EMPTY_VALID warning;
- low confidence → warning when Candidate usable;
- uncertain order → warning when usable;
- overlapping regions → warning when normalized;
- unusable output → ModuleError.

### Input/Image

- missing ArtifactRef;
- unavailable Artifact;
- unsupported type;
- invalid CoordinateSpace;
- invalid Region;
- unsupported format;
- too-large image;
- too-small unusable image.

### Capability

- unsupported language;
- unsupported vertical text;
- local-only conflict;
- no eligible provider;
- line-geometry requirement unavailable.

### Pipeline

- preparation failure;
- detection failure;
- text recognition failure;
- provider timeout;
- output normalization failure;
- coordinate mapping failure;
- reading-order failure;
- Candidate assembly/validation failure.

### Ownership Boundaries

- Queue overflow not mapped to Recognition code;
- Provider initialization failure remains ProviderErrorRef;
- Artifact publication failure not mapped to Candidate error;
- stale rejection not mapped to Recognition failure;
- cancellation not converted to ModuleError by default.

### Privacy

- no image;
- no full text;
- no credentials;
- no raw provider response;
- safe diagnostics reference only.

### Runtime Integration

- RetryHint evaluated externally;
- new Attempt uses new AttemptId;
- cancellation can lead to abandoned Attempt;
- valid Candidate rejected stale without Recognition error;
- critical invariant can degrade/drain module externally.

---

## 38. Error Invariants

1. Every Recognition failure has stable ErrorCode.
2. Every code maps to one symbolic meaning.
3. Provider SDK exceptions never cross boundary.
4. Raw image never appears in error.
5. Full recognized text never appears in error.
6. Credential never appears in error.
7. Warning is separate from ModuleError.
8. No-text is not error.
9. Low confidence is not automatically error.
10. Uncertain reading order is not automatically error.
11. Recognition never decides retry.
12. Recognition never creates retry Attempt.
13. Recognition never owns cancellation terminal outcome.
14. Recognition never owns Queue overflow.
15. Recognition never owns Provider lifecycle.
16. Recognition never owns Artifact publication error.
17. Recognition never owns Storage error.
18. Stale rejection is not Recognition error.
19. Candidate validation error prevents submission.
20. Candidate publication failure is not Candidate validation error.
21. Input Artifact remains unchanged after failure.
22. Attempt-local resources release on failure.
23. RetryHint is advisory.
24. Critical error does not automatically create module FAILED state.
25. Runtime decides Attempt disposition.
26. ProviderErrorRef is sanitized.
27. Metadata is bounded.
28. OperationPhase is explicit.
29. Error contract is versioned.
30. Unknown codes are handled safely.
31. Local-only violations are critical.
32. Invalid Candidate is never submitted as valid.
33. Cleanup failure is observable.
34. Recognition output is never silently fabricated.
35. Error handling preserves traceability.

---

## 39. MVP Error Set

Required MVP errors:

```text
REC-INPUT-001 RECOGNITION_INPUT_INVALID
REC-INPUT-002 RECOGNITION_ARTIFACT_UNAVAILABLE
REC-INPUT-003 RECOGNITION_ARTIFACT_TYPE_UNSUPPORTED
REC-INPUT-004 RECOGNITION_COORDINATE_SPACE_INVALID
REC-INPUT-005 RECOGNITION_REGION_INVALID

REC-IMAGE-001 RECOGNITION_IMAGE_INVALID
REC-IMAGE-002 RECOGNITION_IMAGE_FORMAT_UNSUPPORTED
REC-IMAGE-003 RECOGNITION_IMAGE_TOO_LARGE

REC-CAP-001 RECOGNITION_CAPABILITY_UNAVAILABLE
REC-CAP-002 RECOGNITION_LANGUAGE_UNSUPPORTED
REC-CAP-005 RECOGNITION_PRIVACY_CONFLICT

REC-PREP-001 RECOGNITION_PREPARATION_FAILED
REC-DETECT-001 RECOGNITION_DETECTION_FAILED
REC-REC-001 RECOGNITION_TEXT_FAILED
REC-REC-002 RECOGNITION_TEXT_OUTPUT_INVALID
REC-REC-004 RECOGNITION_PROVIDER_TIMEOUT

REC-COORD-001 RECOGNITION_COORDINATE_MAPPING_FAILED
REC-ORDER-001 RECOGNITION_READING_ORDER_FAILED

REC-CAND-001 RECOGNITION_CANDIDATE_ASSEMBLY_FAILED
REC-CAND-002 RECOGNITION_CANDIDATE_INVALID

REC-RES-001 RECOGNITION_RESOURCE_EXHAUSTED
REC-PROV-001 RECOGNITION_PROVIDER_OUTPUT_INVALID

REC-STATE-001 RECOGNITION_STATE_INVARIANT_VIOLATION
REC-INT-001 RECOGNITION_INTERNAL_ERROR
REC-INT-002 RECOGNITION_INVARIANT_VIOLATION
```

Required MVP warnings:

```text
NO_READABLE_TEXT_DETECTED
LOW_IMAGE_QUALITY
LOW_DETECTION_CONFIDENCE
LOW_RECOGNITION_CONFIDENCE
READING_ORDER_UNCERTAIN
REGION_GEOMETRY_INFERRED
LINE_GEOMETRY_UNAVAILABLE
IMAGE_UPSCALED
IMAGE_DOWNSCALED
REMOTE_PROVIDER_USED
PARTIAL_RECOGNITION
```

---

## 40. Completion Criteria

This document is complete when:

- every Recognition semantic failure has stable code;
- warnings and errors are separate;
- RetryHint is advisory;
- Provider errors are referenced and sanitized;
- Runtime ownership boundaries are explicit;
- no Recognition code duplicates Queue/Scheduler/Provider lifecycle/Artifact Store errors;
- privacy is enforced;
- Candidate validation errors are explicit;
- stale/cancellation/publication outcomes are correctly externalized;
- contracts remain backward-compatible within major version;
- tests cover domain and ownership boundaries.

---

## 41. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/STATES.md
doc/02-modules/recognition/EVENTS.md

doc/01-architecture/runtime/ERROR_MODEL.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md

doc/01-architecture/ocr/PIPELINE.md
doc/01-architecture/ocr/QUALITY.md
doc/01-architecture/ocr/PROVIDERS.md
```

---

## 42. Summary

Recognition Errors now follow:

```text
Recognition detects semantic failure
        ↓
RecognitionModuleError + RetryHint
        ↓
Runtime normalizes and evaluates
        ↓
Runtime decides Attempt disposition
```

Recognition owns:

```text
Warnings
Module Errors
Retry Hints
Provider Error Mapping
Candidate Validation Errors
```

Runtime and other components own:

```text
Retry Scheduling
Cancellation Outcome
Queue and Admission Errors
Provider Lifecycle Errors
Publication Errors
Storage Errors
Terminal Attempt State
```

The central boundary is:

```text
Recognition explains why its semantic work could not produce a valid Candidate.

Runtime decides what the system does next.
```
