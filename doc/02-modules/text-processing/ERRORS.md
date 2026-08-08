# Text Processing Module Errors

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `02-modules/text-processing/ERRORS.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `STATES.md`, `EVENTS.md`

---

# 1. Purpose

Tài liệu này định nghĩa error và warning contract mà Text Processing Module thực sự sở hữu.

Text Processing Error Model chịu trách nhiệm:

* stable module error codes
* module-level error categories
* `TextProcessingModuleError`
* `TextProcessingWarning`
* `RetryHint`
* upstream error references
* input adaptation failures
* normalization failures
* reconstruction failures
* grouping failures
* classification failures
* SourceDocument construction failures
* traceability failures
* Candidate validation failures
* Text Processing-local resource failures
* module invariant failures
* privacy violations
* logging and observability rules
* compatibility
* testing
* architecture invariants

Text Processing Error Model không định nghĩa lại failure semantics của:

* OCR Detection
* OCR Recognition
* OCR Reading Order
* OCR Quality
* Recognition Module
* Runtime
* Scheduler
* Work Queue
* Artifact Store
* Storage
* Translation

---

# 2. Error Ownership

Text Processing owns:

```text
TextProcessingModuleError

TextProcessingWarning

RetryHint

Input Adaptation Errors

Normalization Errors

Reconstruction Errors

Grouping Errors

Classification Errors

SourceDocument Build Errors

Traceability Errors

Candidate Errors

Text Processing-Local Resource Errors

Text Processing State Errors

Privacy Errors

Internal Module Errors
```

Text Processing does not own:

```text
OCRDetectionError

OCRRecognitionError

OCRReadingOrderError

OCRQualityError

RecognitionModuleError

WorkItemError

AttemptError

SchedulerError

QueueError

ArtifactPublicationError

StorageError

TranslationError
```

External failures are preserved through references.

---

# 3. Error Architecture

```text
Upstream / External Failure
        ↓
ExternalErrorRef
        ↓
Text Processing Context
        ↓
TextProcessingModuleError?
        ↓
RetryHint?
        ↓
Runtime
        ↓
Runtime Error Normalization
        ↓
Retry / Fail / Cancel / Abandon
```

Text Processing should not redefine an upstream semantic error when the owner contract already provides enough information.

---

# 4. Error Principles

## 4.1 Stable Error Codes

Consumers depend on:

```text
ErrorCode
```

not:

* implementation exception
* stack trace
* arbitrary error text
* source text
* provider-specific exception

---

## 4.2 Preserve Upstream Artifacts

Failures must not mutate:

* Recognition Artifact
* OCR Document
* Reading Order Result
* Quality Report

Text Processing produces new derived structures only.

---

## 4.3 Preserve Raw Source

A processing failure must never overwrite upstream `RawText`.

---

## 4.4 Warning vs Error

```text
Warning
    = processing degraded
      but a contract-valid Candidate may still exist
```

```text
ModuleError
    = Text Processing cannot produce
      a contract-valid Candidate
      for the current Attempt
```

---

## 4.5 Conservative Recovery

When uncertainty can be safely represented:

```text
degrade
instead of fail
```

Examples:

```text
uncertain reconstruction
    → preserve separate
```

```text
uncertain grouping
    → preserve separate
```

```text
uncertain classification
    → UNKNOWN
```

---

## 4.6 Runtime Owns Disposition

Text Processing reports what failed.

Runtime decides:

```text
Retry
Fail
Cancel
Abandon
Reject Stale
```

---

## 4.7 Privacy

Errors must never expose:

* page images
* RawText
* NormalizedText
* browser content
* translated text
* credentials
* authorization headers
* full diagnostics payload

---

# 5. TextProcessingModuleError

```text
TextProcessingModuleError
├── ContractVersion
├── ErrorCode
├── SymbolicName
├── Category
├── Severity
├── OperationPhase
├── MessageKey
├── RetryHint?
├── UpstreamErrorRef?
├── AffectedScopeRef?
├── CandidateArtifactId?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

---

# 6. Error Contract Rules

1. ErrorCode stable within one major contract version.

2. SymbolicName stable.

3. MessageKey suitable for localization.

4. Raw source text never embedded.

5. Runtime state not embedded.

6. RetryHint advisory only.

7. Metadata bounded.

8. Upstream semantics preserved.

9. Candidate identity may be included only when Candidate exists.

10. OperationPhase uses Text Processing-owned phases.

---

# 7. Stable Error Code Format

Canonical:

```text
TXT-<CATEGORY>-<NUMBER>
```

Recommended examples:

```text
TXT-INPUT-001
TXT-PLAN-001
TXT-ADAPT-001
TXT-NORM-001
TXT-RECON-001
TXT-GROUP-001
TXT-CLASS-001
TXT-DOC-001
TXT-TRACE-001
TXT-CAND-001
TXT-RES-001
TXT-STATE-001
TXT-PRIV-001
TXT-INT-001
```

---

# 8. Error Categories

| Prefix  | Category                          |
| ------- | --------------------------------- |
| `INPUT` | Attempt input / upstream artifact |
| `PLAN`  | Processing Plan                   |
| `ADAPT` | Recognition Artifact adaptation   |
| `NORM`  | Text normalization                |
| `RECON` | Source reconstruction             |
| `GROUP` | Source grouping                   |
| `CLASS` | Block classification              |
| `DOC`   | SourceDocument construction       |
| `TRACE` | Source traceability               |
| `CAND`  | Candidate assembly / validation   |
| `RES`   | Text Processing-local resources   |
| `STATE` | Module-owned state invariants     |
| `PRIV`  | Privacy boundary                  |
| `INT`   | Internal module failure           |

Legacy categories removed:

```text
SEG
LANG
FORMAT
```

because their old meanings no longer match current ownership.

---

# 9. Severity

```text
TextProcessingErrorSeverity
├── INFORMATION
├── WARNING
├── ERROR
└── CRITICAL
```

## INFORMATION

Expected diagnostic condition.

Usually represented as warning/diagnostic rather than ModuleError.

---

## WARNING

Degraded but usable.

Prefer `TextProcessingWarning`.

---

## ERROR

Current Attempt cannot produce valid Candidate without changed conditions.

---

## CRITICAL

Module invariant, privacy, or structural guarantee has been violated.

Runtime/Container decides whether module degradation/restart is necessary.

---

# 10. Retry Hint

```text
TextProcessingRetryHint
├── Retryability
├── SuggestedStrategies[]
├── ReasonCode
└── Metadata?
```

```text
Retryability
├── RETRYABLE
├── CONDITIONALLY_RETRYABLE
└── NON_RETRYABLE
```

Suggested strategies:

```text
SAME_PROFILE

CONSERVATIVE_PROFILE

DISABLE_OPTIONAL_GROUPING

DISABLE_OPTIONAL_CLASSIFICATION

FLAT_STRUCTURE

RESOURCE_WAIT

NO_RETRY
```

Text Processing never creates retry Attempts itself.

---

# 11. TextProcessingWarning

```text
TextProcessingWarning
├── WarningCode
├── Severity
├── OperationPhase
├── MessageKey
├── SourceScopeRef?
├── EvidenceRefs[]
├── Metadata?
└── RecordedAt
```

Warnings may coexist with a valid Candidate.

---

# 12. Recommended Module Warning Codes

```text
NO_PROCESSABLE_TEXT

PARTIAL_SOURCE_DOCUMENT

NORMALIZATION_SKIPPED

NORMALIZATION_DEGRADED

RECONSTRUCTION_UNCERTAIN

GROUPING_UNCERTAIN

CLASSIFICATION_UNCERTAIN

STRUCTURE_FLATTENED

OPTIONAL_READING_ORDER_UNAVAILABLE

OPTIONAL_QUALITY_REPORT_UNAVAILABLE

BLOCK_EXCLUSION_UNCERTAIN

UPSTREAM_WARNING_PRESERVED
```

---

# 13. No Processable Text

Canonical result:

```text
Completeness = EMPTY_VALID
```

with:

```text
NO_PROCESSABLE_TEXT
```

This is not an error.

Legacy:

```text
TXT-NORM-001 EmptyText
```

should no longer be treated as a processing failure.

---

# 14. Reconstruction Uncertainty

When join evidence is weak:

```text
RECONSTRUCTION_UNCERTAIN
```

and:

```text
preserve separate structures
```

rather than throwing a ModuleError.

---

# 15. Grouping Uncertainty

When group relationship is ambiguous:

```text
GROUPING_UNCERTAIN
```

with conservative fallback:

```text
PRESERVE_SEPARATE
```

---

# 16. Classification Uncertainty

When classification confidence is insufficient:

```text
CLASSIFICATION_UNCERTAIN
```

and:

```text
BlockType = UNKNOWN
```

`UNKNOWN` is valid.

---

# 17. Input Errors

## TXT-INPUT-001 — TEXT_PROCESSING_INPUT_INVALID

Attempt Input malformed or internally inconsistent.

Examples:

* RuntimeContext missing
* RecognitionArtifactRef missing
* ProcessingProfileRef missing
* malformed ProcessingOptions
* invalid PrivacyContextRef

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

until caller corrects input.

---

## TXT-INPUT-002 — RECOGNITION_ARTIFACT_UNAVAILABLE

Recognition Artifact cannot be resolved or leased.

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

## TXT-INPUT-003 — RECOGNITION_ARTIFACT_INCOMPATIBLE

Recognition Artifact exists but is incompatible with required contract.

Examples:

* unsupported contract major
* incompatible content identity
* incompatible privacy partition
* missing required semantic lineage

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

unless a compatible upstream Artifact becomes available.

---

## TXT-INPUT-004 — OCR_DOCUMENT_REFERENCE_UNAVAILABLE

Required `OCRDocumentRef` cannot be resolved.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## TXT-INPUT-005 — TEXT_PROCESSING_CONTRACT_VERSION_UNSUPPORTED

Unsupported Text Processing major version.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

---

# 18. Processing Plan Errors

## TXT-PLAN-001 — PROCESSING_PLAN_INVALID

Processing Plan cannot become `READY`.

Examples:

* invalid Profile
* contradictory Options
* impossible requested structure mode
* unsupported configuration combination

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## TXT-PLAN-002 — PROCESSING_PROFILE_UNSUPPORTED

Requested Processing Profile unsupported.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

unless a fallback profile is explicitly permitted.

---

## TXT-PLAN-003 — PROCESSING_CONFIGURATION_INCOMPATIBLE

Configuration Snapshot or Rule Set incompatible with current contract/input.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 19. Input Adaptation Errors

## TXT-ADAPT-001 — SOURCE_ADAPTATION_FAILED

Recognition Artifact cannot be adapted into `ProcessingInputDocument`.

Examples:

* required upstream reference inconsistent
* source identity mismatch
* required OCR entity reference unresolved
* invalid canonical upstream artifact relationship

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## TXT-ADAPT-002 — SOURCE_IDENTITY_CONFLICT

Resolved upstream artifacts do not describe the same semantic source.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

until upstream inputs change.

---

## TXT-ADAPT-003 — REQUIRED_SOURCE_ORDER_UNAVAILABLE

Processing Profile requires canonical Reading Order, but required result is unavailable.

Use only when Reading Order is mandatory.

If optional:

```text
OPTIONAL_READING_ORDER_UNAVAILABLE
```

warning should be used instead.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 20. Upstream Error Reference

Text Processing should preserve upstream semantic failures.

```text
UpstreamErrorRef
├── Owner
├── ErrorCode
├── ErrorContractVersion
├── ScopeRef?
├── Retryability?
├── DiagnosticsRef?
└── Metadata?
```

Possible owners:

```text
RECOGNITION

OCR_POSTPROCESS

OCR_READING_ORDER

OCR_QUALITY

ARTIFACT_STORE

RESOURCE_MANAGER
```

Text Processing does not change upstream ErrorCode meaning.

---

# 21. Normalization Errors

## TXT-NORM-001 — NORMALIZATION_FAILED

Deterministic normalization cannot safely produce a normalized representation.

Examples:

* normalization rule invariant failure
* corrupt internal scalar representation
* invalid rule output

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Suggested fallback:

```text
CONSERVATIVE_PROFILE
```

when allowed.

---

## TXT-NORM-002 — NORMALIZATION_RULE_INVALID

Configured normalization rule is malformed or violates Text Processing invariants.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

until configuration changes.

---

## TXT-NORM-003 — NORMALIZED_TEXT_NOT_TRACEABLE

NormalizedText cannot be attributed to RawText according to required traceability policy.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for unchanged implementation/rules.

---

# 22. Unicode Handling

Legacy errors such as:

```text
InvalidUnicode

UnsupportedEncoding
```

should not automatically become module failures.

At this boundary, upstream Artifact text should already be a valid canonical string representation.

If malformed canonical text enters Text Processing:

```text
TXT-ADAPT-001
```

or:

```text
TXT-NORM-001
```

should be used depending on where violation is detected.

---

# 23. Reconstruction Errors

## TXT-RECON-001 — RECONSTRUCTION_FAILED

Source reconstruction cannot produce valid source structures.

Use only when conservative fallback cannot preserve a valid document.

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
CONSERVATIVE_PROFILE
```

---

## TXT-RECON-002 — RECONSTRUCTION_INVARIANT_VIOLATION

Reconstruction produces impossible structural state.

Examples:

* one source node assigned incompatibly
* reconstruction reference graph corrupted
* output references non-existent input evidence

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for same implementation/config.

---

## TXT-RECON-003 — RECONSTRUCTION_POLICY_UNSUPPORTED

Requested reconstruction policy cannot be executed.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

unless policy/profile changes.

---

# 24. Removed Segmentation Ownership

Legacy errors:

```text
SegmentationFailed

SegmentTooLarge

InvalidSegment
```

are removed from Text Processing.

Reason:

```text
TranslationUnit segmentation
belongs to Translation.
```

Text Processing creates:

```text
SourceDocument

SourceBlock
```

not translation-ready segments.

---

# 25. Removed ReadingOrderConflict Ownership

Legacy:

```text
TXT-SEG-002 ReadingOrderConflict
```

is removed.

Canonical OCR Reading Order belongs to OCR Architecture.

Text Processing may:

* consume canonical Reading Order
* derive SourceBlockSequence

but does not determine canonical page order from scratch.

When required upstream Reading Order is missing:

```text
TXT-ADAPT-003
```

may be used.

---

# 26. Grouping Errors

## TXT-GROUP-001 — GROUPING_FAILED

Logical source grouping cannot produce a valid structure and conservative separation cannot recover.

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
CONSERVATIVE_PROFILE
```

---

## TXT-GROUP-002 — GROUPING_RULE_INVALID

Configured grouping rule is invalid.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

until rule/config changes.

---

## TXT-GROUP-003 — GROUPING_INVARIANT_VIOLATION

Grouping result violates structural guarantees.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

---

# 27. Classification Errors

Classification uncertainty should not normally be an error.

Use:

```text
CLASSIFICATION_UNCERTAIN
```

and:

```text
BlockType = UNKNOWN
```

instead.

---

## TXT-CLASS-001 — CLASSIFICATION_FAILED

Classifier infrastructure/logic cannot produce any valid classification output and Profile requires classification.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Possible fallback:

```text
DISABLE_OPTIONAL_CLASSIFICATION
```

when classification is optional.

---

## TXT-CLASS-002 — CLASSIFICATION_RULE_INVALID

Classification rule configuration invalid.

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

until config changes.

---

# 28. Removed Language Errors

Legacy errors:

```text
LanguageMismatch

UnsupportedLanguage

MixedLanguageContent
```

are not default Text Processing errors anymore.

Text Processing should preserve language uncertainty/hints.

Mixed-language content is valid source content.

Translation decides:

* target language handling
* segmentation by language
* provider language support

Text Processing may emit a source-language warning only if a Processing Profile specifically requires it.

---

# 29. SourceDocument Build Errors

## TXT-DOC-001 — SOURCE_DOCUMENT_BUILD_FAILED

SourceDocument cannot be constructed from processed structures.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## TXT-DOC-002 — SOURCE_DOCUMENT_INVALID

Constructed SourceDocument violates contract.

Examples:

* duplicate BlockId
* missing root block
* invalid child reference
* cyclic hierarchy
* invalid BlockSequence reference
* invalid ExcludedBlock reference

Severity:

```text
ERROR
```

or:

```text
CRITICAL
```

when module invariant is broken.

---

## TXT-DOC-003 — SOURCE_DOCUMENT_HIERARCHY_CYCLE

SourceBlock hierarchy contains a cycle.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for same implementation/configuration.

---

## TXT-DOC-004 — SOURCE_BLOCK_SEQUENCE_INVALID

SourceBlockSequence violates contract.

Examples:

* references missing block
* invalid index
* contradictory source derivation
* unintended duplicate

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Possible fallback:

```text
FLAT_STRUCTURE
```

only when Profile allows it.

---

# 30. Formatting Errors Removed

Legacy:

```text
InvalidStructure

FormattingFailed

InvalidWhitespace
```

are too broad and overlap several current concerns.

They are replaced by precise categories:

```text
NORM

RECON

GROUP

DOC

TRACE
```

Text Processing does not attempt grammar or sentence correctness validation by default.

For example:

```text
unmatched quotation
```

is source content, not automatically an error.

---

# 31. Traceability Errors

## TXT-TRACE-001 — TRACEABILITY_VALIDATION_FAILED

SourceDocument does not satisfy required source lineage guarantees.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## TXT-TRACE-002 — SOURCE_EVIDENCE_MISSING

A textual SourceBlock lacks required OCR source evidence.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

unless source data changes.

---

## TXT-TRACE-003 — NORMALIZATION_EVIDENCE_MISSING

NormalizedText lacks required RawText/NormalizationChange lineage.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

---

## TXT-TRACE-004 — SOURCE_REFERENCE_UNRESOLVED

One or more required source references cannot be resolved.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 32. Candidate Errors

## TXT-CAND-001 — CANDIDATE_ASSEMBLY_FAILED

CandidateSourceDocumentArtifact cannot be assembled.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## TXT-CAND-002 — CANDIDATE_INVALID

Candidate failed Text Processing module-level validation.

Examples:

* invalid SourceDocument
* missing RecognitionArtifactRef
* missing CompatibilityMetadata
* missing TraceabilityMetadata
* invalid Completeness
* translated content leaked into Candidate
* Runtime state leaked into Candidate

Severity:

```text
ERROR
```

or `CRITICAL` for invariant violation.

---

## TXT-CAND-003 — CANDIDATE_PRIVACY_VIOLATION

Candidate contains forbidden content or violates PrivacyContext.

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

## TXT-CAND-004 — CANDIDATE_SUBMISSION_FAILED

Candidate cannot cross module → Runtime boundary because local serialization/contract transfer preparation failed.

Not used for:

* stale rejection
* Runtime authority rejection
* Artifact Store publication failure

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 33. Resource Errors

## TXT-RES-001 — RESOURCE_EXHAUSTED

Attempt-local Text Processing resource budget exceeded.

Examples:

* working-node memory
* reconstruction buffer
* document assembly buffer

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

or conservative profile when applicable.

---

## TXT-RES-002 — ARTIFACT_LEASE_FAILED

Recognition Artifact/OCR reference lease cannot be acquired or maintained.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

## TXT-RES-003 — LOCAL_CLEANUP_FAILED

Attempt-local resources cannot be cleaned safely.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for same Attempt.

Resource Manager/Runtime may degrade the component.

---

# 34. Timeout Ownership

Legacy:

```text
TXT-RES-002 Timeout
```

is removed as a Text Processing-owned timeout.

Deadline belongs to Runtime.

Text Processing may:

* observe deadline
* stop optional work
* return a ModuleError/RetryHint when local operation cannot continue

But canonical timeout/Attempt outcome belongs to Runtime.

---

# 35. QueueOverflow Ownership

Legacy:

```text
TXT-RES-003 QueueOverflow
```

is removed.

Queue ownership belongs to Runtime Work Queue/Scheduler.

Text Processing begins only after Runtime admits the Attempt.

---

# 36. State Errors

## TXT-STATE-001 — STATE_INVARIANT_VIOLATION

Text Processing-owned state transition violates `STATES.md`.

Applies to:

* module availability
* Processing Plan
* Operation Phase
* Candidate Validation

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

for same Attempt.

---

## TXT-STATE-002 — DUPLICATE_CANDIDATE_SUBMISSION

Same Candidate is semantically submitted more than once by module logic.

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

---

# 37. Removed Legacy State Errors

Legacy errors:

```text
ProcessingAlreadyRunning

ProcessingNotStarted
```

are removed.

Reason:

```text
WorkItem / Attempt execution lifecycle
belongs to Runtime.
```

Text Processing does not maintain a global request registry whose lifecycle must be queried.

---

# 38. Privacy Errors

## TXT-PRIV-001 — PRIVACY_CONTEXT_CONFLICT

Requested processing behavior conflicts with PrivacyContext.

Examples:

* protected diagnostics requested without authorization
* persistent diagnostic data in EPHEMERAL mode
* incompatible privacy partition

Severity:

```text
ERROR
```

Retry:

```text
NON_RETRYABLE
```

until request/policy changes.

---

## TXT-PRIV-002 — SOURCE_CONTENT_EXPOSURE_DETECTED

Forbidden source content is about to cross a normal log/event/error boundary.

Examples:

* RawText in Event payload
* NormalizedText in normal log
* source snippet in error message
* protected content in unrestricted diagnostics

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

---

# 39. Internal Errors

## TXT-INT-001 — INTERNAL_FAILURE

Unexpected Text Processing implementation failure.

Severity:

```text
CRITICAL
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

Runtime decides whether a new Attempt helps.

---

## TXT-INT-002 — INVARIANT_VIOLATION

Architecture invariant violated.

Examples:

* RawText overwritten
* SourceDocument contains TranslationUnit
* Text Processing redefines OCR Reading Order
* Candidate mutated after VALID
* Text Processing assumes Runtime authority
* SourceBlock created without source evidence
* translated text written into SourceDocument

Severity:

```text
CRITICAL
```

Retry:

```text
NON_RETRYABLE
```

until implementation/config corrected.

---

## TXT-INT-003 — REFERENCE_NORMALIZATION_FAILED

Module cannot safely normalize an external Artifact/Error reference into its public contract.

Severity:

```text
ERROR
```

Retry:

```text
CONDITIONALLY_RETRYABLE
```

---

# 40. AtomicCommitFailed Removed

Legacy:

```text
TXT-INT-003 AtomicCommitFailed
```

is removed.

Text Processing no longer owns:

```text
result commit

terminal commit

Artifact publication
```

Current boundary:

```text
Candidate
    ↓
Runtime
    ↓
Artifact Store
```

Publication/ownership-transfer failures belong to Runtime/Artifact Store.

---

# 41. Errors Not Owned by Text Processing

Do not create Text Processing-specific aliases for:

```text
QueueOverflow

SchedulerAdmissionRejected

AttemptCanceled

AttemptAbandoned

RuntimeDeadlineExpired

RevisionStale

RecognitionArtifactPublicationFailed

OCRReadingOrderCycle

OCRQualityAssessmentFailed

ArtifactOwnershipTransferFailed

ArtifactPublicationFailed

CacheEvictionFailed

StorageWriteFailed

TranslationProviderUnavailable
```

Reference canonical owner error instead.

---

# 42. Cancellation Boundary

Cancellation is not normally a `TextProcessingModuleError`.

Text Processing observes Runtime-provided:

```text
CancellationContext
```

Then:

* stops new expensive work
* cleans local resources
* avoids invalid Candidate submission
* returns control to Runtime

Runtime decides terminal disposition.

---

# 43. Supersession / Stale Boundary

A valid Candidate may become irrelevant.

Example:

```text
Candidate VALID
    ↓
Runtime detects stale Revision
    ↓
REJECTED_STALE
```

This is not:

```text
TextProcessingModuleError
```

Text Processing must not invent:

```text
TXT-...-Superseded
```

unless future architecture explicitly transfers ownership.

---

# 44. Runtime Deadline Boundary

Deadline expiration does not automatically map to a Text Processing error.

Depending on point of observation, module may:

* stop optional work
* return partial Candidate if contract allows
* return appropriate local operation error
* cleanup and return control

Runtime owns final timeout outcome.

---

# 45. Error to Runtime Disposition

```text
TextProcessingModuleError
        ↓
Runtime Error Normalization
        ↓
Retry Policy
Cancellation Policy
Authority Validation
        ↓
Attempt Disposition
```

Possible external outcomes:

```text
FAILED

CANCELED

ABANDONED

RETRY_SCHEDULED

REJECTED_STALE
```

Text Processing does not directly map one error to one terminal state.

---

# 46. Error and Candidate Relationship

```text
Valid Candidate
    → no ModuleError
    → warnings allowed
```

```text
Invalid Candidate
    → ModuleError
    → no valid submission
```

```text
Valid Candidate
    → Runtime rejects stale
    → no Text Processing error
```

```text
EMPTY_VALID Candidate
    → warning allowed
    → no error required
```

---

# 47. Error and SourceDocument Relationship

A SourceDocument may remain valid when:

* classification uncertain
* grouping uncertain but preserved separate
* optional Reading Order unavailable
* optional Quality Report unavailable
* no text exists
* hierarchy falls back to flat

Errors should be reserved for contract-invalid or unrecoverable processing states.

---

# 48. Logging Contract

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

RecognitionArtifactId?

CandidateArtifactId?

ProcessingProfileId?

ConfigurationSnapshotId

UpstreamErrorCode?

Retryability

TraceId

OccurredAt
```

Forbidden:

```text
RawText

NormalizedText

SourceDocument content

image bytes

browser content

translated text

credentials

authorization headers

stack traces in normal structured logs
```

---

# 49. Metrics

Recognition-owned metrics are not duplicated.

Text Processing-owned error metrics may include:

```text
text_processing.error.total

text_processing.error.by_code

text_processing.error.by_category

text_processing.error.by_phase

text_processing.warning.total

text_processing.warning.by_code

text_processing.normalization_failure_total

text_processing.reconstruction_failure_total

text_processing.grouping_failure_total

text_processing.document_invalid_total

text_processing.traceability_failure_total

text_processing.candidate_invalid_total

text_processing.resource_exhausted_total

text_processing.invariant_violation_total
```

---

# 50. Metrics Not Owned by Text Processing

Do not redefine:

```text
queue_overflow_total

scheduler_rejection_total

runtime_timeout_total

recognition_error_total

ocr_reading_order_error_total

quality_grade_distribution

artifact_publication_failure_total

translation_error_total
```

---

# 51. Error Observability

Every module error should be traceable to:

```text
RevisionId

WorkItemId

AttemptId

OperationPhase

ConfigurationSnapshotId

ProcessingProfileId?

RecognitionArtifactId?

TraceId
```

High-cardinality IDs belong in logs/traces, not normal metric labels.

---

# 52. Diagnostic Reference

Detailed diagnostics should use:

```text
DiagnosticsRef
```

rather than embedding protected content.

Protected diagnostics require:

* explicit mode
* authorization
* bounded retention
* redaction
* secure storage
* auditability

---

# 53. Contract Evolution

Backward-compatible changes:

* add new error code
* add warning code
* add optional Metadata
* add RetryStrategy
* add optional diagnostics reference
* clarify semantics

Breaking changes:

* change existing code meaning
* materially change severity
* materially change retryability
* remove/rename stable code
* change owner boundary
* change privacy guarantee

Breaking changes require major version update.

---

# 54. Unknown Codes

Consumers must:

* preserve unknown code
* use known Category if available
* not crash
* not fabricate behavior
* reject unsupported contract major version when required

---

# 55. Testing — Error Contract

Verify:

* every ErrorCode unique
* every code has one symbolic meaning
* category matches prefix
* Severity valid
* RetryHint valid
* MessageKey present
* no Runtime terminal state
* no source content
* no Translation-specific error ownership
* no OCR Reading Order redefinition

---

# 56. Testing — Warning vs Error

Test:

```text
empty source
    → EMPTY_VALID
      + NO_PROCESSABLE_TEXT
```

```text
classification uncertain
    → UNKNOWN
      + warning
```

```text
grouping uncertain
    → preserve separate
      + warning
```

```text
traceability impossible
    → ModuleError
```

```text
invalid Candidate
    → ModuleError
```

---

# 57. Testing — Input

Test:

* missing RecognitionArtifactRef
* unavailable Artifact
* unsupported contract
* missing OCRDocumentRef
* invalid Processing Profile
* incompatible PrivacyContext
* source identity conflict

---

# 58. Testing — Reconstruction

Test:

* safe reconstruction success
* ambiguous reconstruction fallback
* reconstruction invariant violation
* unsupported reconstruction policy
* same input/rules deterministic output

---

# 59. Testing — Grouping

Test:

* grouping succeeds
* grouping ambiguous → preserve separate
* invalid grouping rule
* structural invariant violation

---

# 60. Testing — Classification

Test:

* confident classification
* unknown classification
* optional classifier unavailable
* required classification failure
* invalid rule configuration

---

# 61. Testing — SourceDocument

Test:

* duplicate BlockId
* missing RootBlock
* cyclic hierarchy
* invalid BlockSequence reference
* invalid excluded block reference
* SourceBlock without source evidence
* NormalizedText without RawText lineage
* valid flat fallback

---

# 62. Testing — Candidate

Test:

* valid Candidate
* missing RecognitionArtifactRef
* invalid SourceDocument
* translated text leakage
* Runtime state leakage
* missing CompatibilityMetadata
* missing TraceabilityMetadata
* duplicate Candidate submission

---

# 63. Testing — Runtime Boundary

Test:

```text
valid Candidate
    → stale Runtime rejection
    → no module error
```

```text
cancellation
    → no module-owned Cancelled terminal error
```

```text
deadline expiry
    → Runtime owns terminal outcome
```

```text
retry
    → Runtime creates new Attempt
```

---

# 64. Error Invariants

1. Every Text Processing-owned failure has stable ErrorCode.

2. Every ErrorCode maps to one semantic meaning.

3. Recognition/OCR errors retain their original owner.

4. Runtime errors retain their original owner.

5. Artifact Store publication errors are not Text Processing errors.

6. Translation errors are not Text Processing errors.

7. RawText never appears in normal error payload.

8. NormalizedText never appears in normal error payload.

9. Warning differs from ModuleError.

10. Empty content is not failure.

11. Classification uncertainty is not failure.

12. Grouping uncertainty is not failure when conservative fallback exists.

13. Reconstruction uncertainty is not failure when preservation is possible.

14. Text Processing does not redefine canonical Reading Order errors.

15. Text Processing does not own queue errors.

16. Text Processing does not own timeout lifecycle.

17. Text Processing does not own cancellation outcome.

18. Text Processing does not own supersession.

19. Text Processing does not own retry execution.

20. Text Processing does not own publication errors.

21. Candidate validation failure prevents valid submission.

22. Stale Candidate rejection is not module failure.

23. SourceDocument failure never mutates Recognition Artifact.

24. Raw source evidence remains preserved after failure.

25. RetryHint is advisory.

26. UpstreamErrorRef preserves owner semantics.

27. Metadata remains bounded.

28. OperationPhase uses Text Processing phases only.

29. Contract is versioned.

30. Unknown codes handled safely.

31. Privacy violations are explicit.

32. Candidate rejection triggers cleanup.

33. Traceability failure is never silently ignored.

34. TranslationUnit segmentation failures cannot exist in this module.

35. Mixed-language source content is not automatically error.

36. Unmatched source punctuation is not automatically error.

37. SourceDocument hierarchy cycles are invalid.

38. Invalid SourceBlockSequence is explicit.

39. Source content is never fabricated to recover from failure.

40. Runtime decides what happens next.

---

# 65. MVP Error Set

Required MVP:

```text
TXT-INPUT-001
TEXT_PROCESSING_INPUT_INVALID

TXT-INPUT-002
RECOGNITION_ARTIFACT_UNAVAILABLE

TXT-INPUT-003
RECOGNITION_ARTIFACT_INCOMPATIBLE

TXT-INPUT-004
OCR_DOCUMENT_REFERENCE_UNAVAILABLE


TXT-PLAN-001
PROCESSING_PLAN_INVALID

TXT-PLAN-002
PROCESSING_PROFILE_UNSUPPORTED


TXT-ADAPT-001
SOURCE_ADAPTATION_FAILED

TXT-ADAPT-002
SOURCE_IDENTITY_CONFLICT


TXT-NORM-001
NORMALIZATION_FAILED


TXT-RECON-001
RECONSTRUCTION_FAILED


TXT-GROUP-001
GROUPING_FAILED


TXT-DOC-001
SOURCE_DOCUMENT_BUILD_FAILED

TXT-DOC-002
SOURCE_DOCUMENT_INVALID


TXT-TRACE-001
TRACEABILITY_VALIDATION_FAILED

TXT-TRACE-002
SOURCE_EVIDENCE_MISSING


TXT-CAND-001
CANDIDATE_ASSEMBLY_FAILED

TXT-CAND-002
CANDIDATE_INVALID


TXT-RES-001
RESOURCE_EXHAUSTED

TXT-RES-002
ARTIFACT_LEASE_FAILED


TXT-STATE-001
STATE_INVARIANT_VIOLATION


TXT-PRIV-001
PRIVACY_CONTEXT_CONFLICT


TXT-INT-001
INTERNAL_FAILURE

TXT-INT-002
INVARIANT_VIOLATION
```

---

# 66. MVP Warning Set

Required:

```text
NO_PROCESSABLE_TEXT

PARTIAL_SOURCE_DOCUMENT

RECONSTRUCTION_UNCERTAIN

GROUPING_UNCERTAIN

CLASSIFICATION_UNCERTAIN

STRUCTURE_FLATTENED
```

Recommended:

```text
OPTIONAL_READING_ORDER_UNAVAILABLE

OPTIONAL_QUALITY_REPORT_UNAVAILABLE

BLOCK_EXCLUSION_UNCERTAIN

UPSTREAM_WARNING_PRESERVED
```

---

# 67. Removed Legacy Errors

The following legacy errors are intentionally removed or re-owned:

```text
MissingOCRResult
    → RecognitionArtifact / OCRDocument reference error

InvalidOCRResult
    → upstream Artifact incompatibility

UnsupportedOCRVersion
    → upstream contract incompatibility

EmptyText
    → EMPTY_VALID + warning

SegmentationFailed
    → Translation concern when referring to Translation Units

ReadingOrderConflict
    → OCR Reading Order concern

SegmentTooLarge
    → Translation concern

InvalidSegment
    → Translation concern

LanguageMismatch
    → normally hint/warning, not module failure

UnsupportedLanguage
    → Translation/provider concern in most cases

MixedLanguageContent
    → valid source content

InvalidStructure
    → replaced by precise DOC/TRACE errors

FormattingFailed
    → replaced by NORM/RECON/DOC concerns

ProcessingAlreadyRunning
    → Runtime concern

ProcessingNotStarted
    → Runtime concern

Timeout
    → Runtime deadline concern

QueueOverflow
    → Runtime Queue/Scheduler concern

AtomicCommitFailed
    → Runtime / Artifact Store concern
```

---

# 68. Completion Criteria

This error contract is complete when:

* all module-owned failures have stable codes
* warnings are separated from failures
* SourceDocument errors are explicit
* traceability failures are explicit
* Candidate errors are explicit
* Translation segmentation errors are absent
* OCR Reading Order errors remain upstream-owned
* Queue/timeout/publication errors remain external
* Runtime disposition remains external
* RetryHint remains advisory
* privacy rules are enforced
* error ownership is testable
* backward compatibility is defined

---

# 69. Related Documents

```text
02-modules/text-processing/README.md
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md
02-modules/text-processing/STATES.md
02-modules/text-processing/EVENTS.md

02-modules/recognition/CONTRACT.md
02-modules/recognition/ERRORS.md

01-architecture/ocr/POSTPROCESS.md
01-architecture/ocr/QUALITY.md
01-architecture/ocr/READING_ORDER.md

01-architecture/runtime/ERROR_MODEL.md
01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

03-infrastructure/artifact-store/
03-infrastructure/resource-manager/

02-modules/translation/
```

---

# 70. Summary

Text Processing error model covers only failures inside the Text Processing semantic boundary:

```text
Recognition Artifact
        ↓
Input Adaptation
        ↓
Normalization
        ↓
Reconstruction
        ↓
Grouping
        ↓
Classification
        ↓
SourceDocument Construction
        ↓
Traceability Validation
        ↓
Candidate Assembly
```

Text Processing owns:

```text
Source reconstruction errors

SourceDocument errors

Traceability errors

Candidate errors

Module warnings

Retry hints
```

Runtime owns:

```text
Queue errors

Attempt lifecycle

Deadline outcome

Cancellation outcome

Retry execution

Stale-result outcome
```

OCR Architecture owns:

```text
OCR Reading Order errors

OCR Quality errors

OCR structural errors
```

Artifact Store owns:

```text
Publication errors

Ownership-transfer errors
```

Translation owns:

```text
Translation Unit segmentation errors

Target-language errors

Translation provider errors

Translated-result errors
```

Core rule:

```text
Text Processing owns failures
while reconstructing a stable SourceDocument.

It does not own failures
of OCR, Runtime, Artifact publication,
or Translation.
```
