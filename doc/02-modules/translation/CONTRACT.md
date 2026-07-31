# Translation Contracts

> **Project:** CRAI
> **Module:** Translation
> **Document:** Public Contracts
> **Path:** `modules/translation/CONTRACT.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-25
> **Source of Truth:** `modules/translation/MODULE.md`

---

## 1. Purpose

This document defines the public contracts of the Translation module.

It specifies:

* public commands;
* command responses;
* identifiers;
* input references;
* translation configuration;
* translation jobs;
* translation attempts;
* translation batches;
* translated segments;
* translation variants;
* partial results;
* warnings;
* normalized provider metadata;
* query models;
* contract-level invariants.

This document does not define:

* provider-specific requests;
* provider-specific responses;
* event envelopes;
* detailed error catalogs;
* lifecycle transition tables;
* persistence schemas;
* implementation classes;
* database tables.

Those concerns belong to other documents or internal implementations.

---

## 2. Contract Boundary

The Translation module accepts prepared content and produces aligned translated content.

```text
PreparedDocument
      ↓
StartTranslation
      ↓
TranslationJob
      ↓
TranslationAttempt[]
      ↓
TranslationBatch[]
      ↓
TranslatedSegment[]
      ↓
TranslationResult
```

The module does not accept raw OCR output as its canonical input.

The canonical input must already have passed through Text Processing.

---

## 3. Contract Design Principles

All public contracts defined here must follow these principles.

### 3.1 Provider neutrality

Public contracts must not expose provider-native models such as:

```text
OpenAIMessage
GeminiContent
ClaudeMessage
DeepLRequest
```

Provider-specific data remains inside provider adapters.

### 3.2 Immutable source identity

Every translation job targets one immutable prepared content revision.

### 3.3 Segment-level alignment

Every published translated segment must map to one prepared source segment.

### 3.4 Batch-oriented execution

A provider request may translate multiple prepared segments together.

### 3.5 Attempt history preservation

Retries and provider fallbacks create new attempts within the same logical job.

### 3.6 Revision-safe publication

Cancelled, superseded, or stale work must never become authoritative.

### 3.7 Optional partial completion

A job may expose validated partial results when its publication policy permits.

### 3.8 Stable public contracts

Changing translation providers must not require changes to calling modules.

---

## 4. Contract Categories

The public contract surface is divided into:

```text
Identifiers
Commands
Command Results
Source References
Configuration
Job Models
Attempt Models
Batch Models
Result Models
Variant Models
Warning Models
Query Models
Shared Metadata
```

---

# Part I — Identifiers

## 5. Identifier Rules

Identifiers must:

* be opaque to callers;
* remain stable for the lifetime of the represented entity;
* not encode provider credentials;
* not depend on display ordering;
* not be reused for unrelated entities;
* support distributed generation where necessary.

The concrete string format is an implementation decision.

Examples in this document are conceptual.

---

## 6. TranslationJobId

Identifies one logical translation request.

```text
TranslationJobId
```

A job remains the same across:

* automatic retries;
* provider fallback;
* retryable provider failures;
* batch-level re-execution.

A new job is required when the logical translation intent changes materially.

Examples include:

* a different target language;
* a different source revision;
* a different translation profile;
* a manual retranslation request;
* a different terminology snapshot;
* a newly requested translation variant.

---

## 7. TranslationAttemptId

Identifies one execution attempt within a translation job.

```text
TranslationAttemptId
```

Each attempt belongs to exactly one `TranslationJobId`.

```text
TranslationJob
      ├── Attempt 1
      ├── Attempt 2
      └── Attempt 3
```

A new attempt may be created when:

* a provider request times out;
* a retryable failure occurs;
* output validation fails;
* a fallback provider is selected;
* failed batches are retried.

---

## 8. TranslationBatchId

Identifies one provider execution batch.

```text
TranslationBatchId
```

Each batch belongs to exactly one attempt.

A batch contains one or more prepared segments.

---

## 9. TranslationResultId

Identifies one assembled translation result.

```text
TranslationResultId
```

A job may produce:

* partial result revisions;
* one final result;
* multiple immutable variants over time.

A result identifier must not be reused after the result content changes.

---

## 10. TranslationVariantId

Identifies one immutable translation variant.

```text
TranslationVariantId
```

Examples:

* natural translation;
* literal translation;
* alternate provider result;
* glossary-updated translation;
* user-corrected translation.

---

## 11. TranslatedSegmentId

Identifies one translated segment instance.

```text
TranslatedSegmentId
```

A translated segment must reference one source `PreparedSegmentId`.

Different variants may contain different translated segment identifiers for the same source segment.

---

## 12. External Identifiers

Translation consumes identifiers owned by other modules.

Expected external identifiers include:

```text
ReadingSessionId
PreparedDocumentId
PreparedSegmentId
ContentRevision
KnowledgeSnapshotId
GlossaryRevision
ContextRevision
TraceId
CorrelationId
```

Translation must not redefine their ownership.

---

# Part II — Public Commands

## 13. Public Command Set

The initial public command set is:

```text
StartTranslation
CancelTranslation
RetryTranslation
RequestRetranslation
InvalidateTranslation
SelectTranslationVariant
SubmitTranslationCorrection
```

Not every deployment must expose every command through a public API.

The contracts define module-level intent.

---

## 14. Common Command Metadata

Every mutating command should include:

```text
CommandId
RequestedAt
RequestedBy
CorrelationId
TraceContext
IdempotencyKey
```

Conceptual model:

```text
CommandMetadata {
    commandId
    requestedAt
    requestedBy
    correlationId
    traceContext
    idempotencyKey
}
```

### 14.1 CommandId

Uniquely identifies the command submission.

### 14.2 RequestedAt

Timestamp at which the caller issued the command.

### 14.3 RequestedBy

Opaque actor reference.

Possible actor types:

```text
USER
SYSTEM
READING_SESSION
PREFETCH_SCHEDULER
ADMINISTRATOR
```

### 14.4 CorrelationId

Associates work across module boundaries.

### 14.5 TraceContext

Supports distributed tracing.

### 14.6 IdempotencyKey

Allows equivalent repeated submissions to resolve to the same logical operation where applicable.

---

# Part III — Start Translation

## 15. StartTranslationCommand

Requests a new logical translation job.

```text
StartTranslationCommand {
    metadata

    source
    configuration
    context
    knowledge
    executionPolicy
    publicationPolicy
    priority
}
```

---

## 16. TranslationSource

Identifies the prepared source content to translate.

```text
TranslationSource {
    readingSessionId

    preparedDocumentId
    contentRevision

    segmentSelection

    sourceLanguage

    contentProfile
}
```

### Required fields

```text
preparedDocumentId
contentRevision
segmentSelection
```

### Optional fields

```text
readingSessionId
sourceLanguage
contentProfile
```

The module may resolve full prepared content through an approved Text Processing query contract.

---

## 17. Prepared Document Reference

The preferred inter-module contract is reference-based:

```text
PreparedDocumentReference {
    preparedDocumentId
    contentRevision
}
```

Translation should not require callers to copy the entire prepared document into every command.

However, an embedded immutable snapshot may be supported for:

* offline execution;
* isolated testing;
* cross-process transport;
* deployments without shared prepared-content storage.

The command must use one source mode unambiguously:

```text
REFERENCE
SNAPSHOT
```

---

## 18. Prepared Document Snapshot

When snapshot mode is used:

```text
PreparedDocumentSnapshot {
    preparedDocumentId
    contentRevision

    sourceLanguage
    contentProfile

    segments[]

    structureMetadata
    sourceMetadata
}
```

The snapshot must be immutable for the lifetime of the job.

---

## 19. Prepared Segment Contract

Translation consumes a prepared segment contract owned primarily by Text Processing.

Minimum expected shape:

```text
PreparedSegment {
    preparedSegmentId

    sequence
    sourceText

    segmentType

    paragraphId
    dialogueGroupId
    regionId

    languageHint
    contextReferences[]

    flags[]
}
```

### 19.1 preparedSegmentId

Stable alignment identity.

### 19.2 sequence

Source ordering established upstream.

Translation must not infer comic reading order from visual coordinates.

### 19.3 sourceText

Normalized text ready for translation.

### 19.4 segmentType

Possible values may include:

```text
PARAGRAPH
DIALOGUE
NARRATION
CAPTION
TITLE
SOUND_EFFECT
FOOTNOTE
UNKNOWN
```

### 19.5 structural references

Optional values:

```text
paragraphId
dialogueGroupId
regionId
```

### 19.6 languageHint

Optional per-segment language hint.

### 19.7 flags

Possible upstream flags:

```text
OCR_DERIVED
LOW_EXTRACTION_CONFIDENCE
POSSIBLY_INCOMPLETE
PRESERVE_WHITESPACE
PRESERVE_LINE_BREAKS
DO_NOT_TRANSLATE
```

Exact upstream definitions remain owned by Text Processing.

---

## 20. SegmentSelection

Specifies which prepared segments are active translation targets.

```text
SegmentSelection {
    mode
    preparedSegmentIds[]
}
```

Possible modes:

```text
ALL
EXPLICIT
VISIBLE_ONLY
RANGE
```

For `EXPLICIT`, `preparedSegmentIds` is required.

Selection ordering follows the prepared document sequence, not command array order.

---

## 21. StartTranslationResult

The command returns an acknowledgement, not necessarily the completed translation.

```text
StartTranslationResult {
    commandId

    translationJobId

    disposition
    acceptedAt

    existingJobId

    initialStatus
}
```

Possible dispositions:

```text
CREATED
REUSED_ACTIVE_JOB
REUSED_COMPLETED_RESULT
REJECTED
```

### CREATED

A new logical translation job was created.

### REUSED_ACTIVE_JOB

An equivalent active job already exists.

### REUSED_COMPLETED_RESULT

A compatible completed result or cache entry was reused.

### REJECTED

The command failed validation before job creation.

Detailed failures belong in `ERRORS.md`.

---

# Part IV — Translation Configuration

## 22. TranslationConfiguration

Represents the semantic translation intent.

```text
TranslationConfiguration {
    sourceLanguage
    targetLanguage

    translationProfile

    providerPolicy
    terminologyPolicy
    contextPolicy
    outputPolicy

    configurationVersion
}
```

A configuration snapshot is immutable after job creation.

---

## 23. LanguageTag

Languages should use application-approved BCP 47 compatible tags.

Examples:

```text
zh
zh-Hans
zh-Hant
en
vi
ja
```

The architecture must support both:

* document-level language;
* optional segment-level hints.

---

## 24. Source Language

Possible source-language modes:

```text
EXPLICIT
UPSTREAM_DETECTED
AUTO_DETECT
```

Conceptual contract:

```text
SourceLanguageConfiguration {
    mode
    languageTag
    detectionPolicy
}
```

When `mode = EXPLICIT`, `languageTag` is required.

---

## 25. Target Language

Target language must be explicit.

```text
TargetLanguageConfiguration {
    languageTag
    localePreferences
}
```

Initial CRAI priority:

```text
zh-Hans → vi
en → vi
```

These pairs must not be hardcoded into the contract.

---

## 26. TranslationProfile

Initial profile identifiers:

```text
NOVEL_NATURAL
COMIC_NATURAL
GENERAL_NATURAL
LITERAL
CUSTOM
```

Conceptual model:

```text
TranslationProfile {
    profileId
    profileRevision
    customInstructionsReference
}
```

Public contracts should reference profile intent or a stored profile.

They must not expose provider prompt templates.

---

## 27. Profile Semantics

### NOVEL_NATURAL

Prioritizes:

* narrative continuity;
* natural Vietnamese;
* paragraph flow;
* consistent names;
* context-sensitive pronouns.

### COMIC_NATURAL

Prioritizes:

* short natural dialogue;
* speaker consistency;
* bubble-level alignment;
* concise output;
* neighboring-dialogue context.

### GENERAL_NATURAL

Provides general readable translation without genre-specific behavior.

### LITERAL

Prioritizes source structure and direct meaning over natural phrasing.

### CUSTOM

Uses an application-managed custom profile.

---

# Part V — Provider Policy

## 28. ProviderPolicy

Expresses provider-selection intent.

```text
ProviderPolicy {
    mode

    preferredProviderId
    allowedProviderIds[]
    excludedProviderIds[]

    fallbackPolicy

    localityRequirement
    costPreference
    latencyPreference
    qualityPreference

    modelPolicy
}
```

---

## 29. Provider Selection Mode

Possible values:

```text
AUTOMATIC
PREFERRED
REQUIRED
LOCAL_ONLY
REMOTE_ONLY
```

### AUTOMATIC

Translation selects an eligible provider.

### PREFERRED

Translation should prefer one provider but may use fallback.

### REQUIRED

Only the specified provider is allowed.

### LOCAL_ONLY

Remote transmission is prohibited.

### REMOTE_ONLY

Only configured remote providers are eligible.

---

## 30. FallbackPolicy

```text
FallbackPolicy {
    enabled
    maximumFallbacks
    eligibleFailureCategories[]
}
```

Fallback must not occur when:

* the provider mode is `REQUIRED`;
* privacy policy prohibits the fallback provider;
* fallback is disabled;
* no eligible provider exists.

---

## 31. Provider Preferences

Preference values express intent rather than strict scheduling algorithms.

Possible values:

```text
LOW
BALANCED
HIGH
```

Applicable preferences:

```text
costPreference
latencyPreference
qualityPreference
```

The provider-selection algorithm remains internal.

---

## 32. Locality Requirement

Possible values:

```text
ANY
PREFER_LOCAL
LOCAL_REQUIRED
REMOTE_ALLOWED
```

`LOCAL_REQUIRED` prohibits remote transmission.

---

## 33. ModelPolicy

```text
ModelPolicy {
    preferredModelClass
    requiredCapabilities[]
    maximumContextSize
    deterministicPreference
}
```

Public callers should use capability or class requirements where possible.

Provider-native model names should be accepted only as opaque configuration values, never as core architecture dependencies.

---

# Part VI — Terminology Policy

## 34. TerminologyPolicy

```text
TerminologyPolicy {
    knowledgeSnapshotId
    glossaryRevision

    termConstraints[]

    namePolicy
    honorificPolicy
    transliterationPolicy

    conflictPolicy
}
```

Translation consumes terminology snapshots.

It does not own glossary storage.

---

## 35. TermConstraint

```text
TermConstraint {
    sourceTerm
    targetTerm

    strength
    scope

    caseSensitive
    notes
}
```

Possible strengths:

```text
LOCKED
PREFERRED
SUGGESTED
CONTEXTUAL
```

Possible scopes:

```text
GLOBAL
SERIES
CHAPTER
DOCUMENT
SEGMENT
CHARACTER
```

---

## 36. NamePolicy

Possible values:

```text
USE_KNOWLEDGE_MAPPING
PRESERVE_ORIGINAL
SINO_VIETNAMESE
PHONETIC_TRANSLITERATION
PROVIDER_DEFAULT
```

Knowledge mappings take precedence when policy requires them.

---

## 37. HonorificPolicy

Possible values:

```text
CONTEXTUAL_VIETNAMESE
PRESERVE_SOURCE_STYLE
NEUTRAL
LITERAL
CUSTOM
```

---

## 38. Terminology Conflict Policy

Possible values:

```text
FAIL
WARN_AND_CONTINUE
PREFER_LOCKED
PREFER_MOST_SPECIFIC_SCOPE
```

`LOCKED` terms must take precedence over weaker constraints.

---

# Part VII — Context Policy

## 39. TranslationContext

Contains context supplied for translation quality.

```text
TranslationContext {
    contextRevision

    previousSegments[]
    followingSegments[]

    priorTranslations[]
    chapterContext
    characterContext[]

    additionalContextReferences[]
}
```

Context may be embedded as an immutable snapshot or referenced by revision.

---

## 40. Context Policy

```text
ContextPolicy {
    previousSegmentLimit
    followingSegmentLimit

    includeDialogueGroup
    includeParagraphContext
    includeChapterSummary
    includePriorTranslations
    includeCharacterKnowledge

    maximumContextCharacters
    maximumEstimatedContextTokens

    missingContextBehavior
}
```

---

## 41. Missing Context Behavior

Possible values:

```text
CONTINUE_WITH_WARNING
FAIL
USE_AVAILABLE_CONTEXT
```

The default should normally be:

```text
USE_AVAILABLE_CONTEXT
```

for interactive reading.

---

## 42. Context-Only Segments

Context-only segments improve translation but do not produce new public translated segments.

```text
ContextSegment {
    preparedSegmentId
    sourceText

    translatedText
    relationship
    sequence
}
```

Possible relationships:

```text
PREVIOUS
FOLLOWING
SAME_DIALOGUE_GROUP
SAME_PARAGRAPH
RELATED_REFERENCE
```

---

# Part VIII — Execution Policy

## 43. TranslationExecutionPolicy

```text
TranslationExecutionPolicy {
    timeoutPolicy
    retryPolicy
    batchingPolicy
    cachePolicy
    concurrencyPolicy

    allowPartialExecution
}
```

---

## 44. TimeoutPolicy

```text
TimeoutPolicy {
    jobTimeout
    attemptTimeout
    batchTimeout
}
```

Timeout values may be expressed as durations.

A job timeout must not be shorter than mandatory batch execution requirements.

---

## 45. RetryPolicy

```text
RetryPolicy {
    maximumAttempts
    retryableCategories[]
    backoffStrategy
    retryFailedBatchesOnly
}
```

Possible backoff strategies:

```text
NONE
FIXED
EXPONENTIAL
PROVIDER_RECOMMENDED
```

Retries create new `TranslationAttemptId` values.

They do not create a new logical job unless translation intent changes.

---

## 46. BatchingPolicy

```text
BatchingPolicy {
    strategy

    maximumSegments
    maximumCharacters
    maximumEstimatedTokens

    preserveDialogueGroups
    preserveParagraphs
    preservePageBoundary

    allowSingleSegmentBatch
}
```

Possible strategies:

```text
AUTOMATIC
LOW_LATENCY
BALANCED
MAXIMUM_CONTEXT
CUSTOM
```

Batching is an internal Translation responsibility.

Caller values are constraints or preferences, not exact batch construction instructions.

---

## 47. CachePolicy

```text
CachePolicy {
    mode
    maximumAge
    allowProviderIndependentReuse
    allowCrossSessionReuse
}
```

Possible modes:

```text
DISABLED
READ_ONLY
READ_WRITE
REFRESH
```

A cache hit must still satisfy revision and alignment safety.

---

## 48. ConcurrencyPolicy

```text
ConcurrencyPolicy {
    maximumConcurrentBatches
    providerConcurrencyLimit
}
```

Batch completion order must not alter final segment order.

---

# Part IX — Publication Policy

## 49. TranslationPublicationPolicy

```text
TranslationPublicationPolicy {
    mode

    publishPartialSegments
    minimumCompletedSegmentCount
    requireSourceRevisionCurrent

    allowWarnings
    activateFinalVariant
}
```

Possible modes:

```text
ATOMIC
PROGRESSIVE
FINAL_ONLY
```

### ATOMIC

The result is published only when the selected source set is complete.

### PROGRESSIVE

Validated translated segments may be published incrementally.

### FINAL_ONLY

Internal partial results may exist, but only the final result is exposed as authoritative.

---

## 50. Priority

Possible priorities:

```text
INTERACTIVE
VISIBLE
PREFETCH
BACKGROUND
```

Conceptual contract:

```text
TranslationPriority {
    level
    expiresAt
}
```

Visible interactive requests should normally outrank prefetch work.

---

# Part X — Translation Job

## 51. TranslationJob

```text
TranslationJob {
    translationJobId

    sourceIdentity
    configurationSnapshot

    contextIdentity
    knowledgeIdentity

    executionPolicy
    publicationPolicy
    priority

    status

    activeAttemptId
    attemptIds[]

    activeResultId
    activeVariantId

    progress

    createdAt
    startedAt
    completedAt

    cancellation
    supersession
}
```

Exact lifecycle values belong in `STATES.md`.

---

## 52. SourceIdentity

```text
TranslationSourceIdentity {
    readingSessionId

    preparedDocumentId
    contentRevision

    selectedPreparedSegmentIds[]
    sourceContentHash
}
```

`sourceContentHash` may be used for cache and integrity checks.

It must not replace the canonical document and revision identity.

---

## 53. Configuration Snapshot

The job records an immutable configuration snapshot or stable snapshot identity.

```text
TranslationConfigurationSnapshot {
    configuration
    resolvedProfileRevision
    resolvedPolicyRevision
}
```

Changes to user settings after job creation do not mutate the existing job.

---

## 54. Context Identity

```text
TranslationContextIdentity {
    contextRevision
    contextHash
}
```

This allows delayed and cached results to be evaluated safely.

---

## 55. Knowledge Identity

```text
TranslationKnowledgeIdentity {
    knowledgeSnapshotId
    glossaryRevision
}
```

A changed glossary does not mutate an existing job.

It may trigger invalidation or a new retranslation job.

---

## 56. Job Progress

```text
TranslationProgress {
    totalSegmentCount
    completedSegmentCount
    failedSegmentCount
    pendingSegmentCount

    totalBatchCount
    completedBatchCount
    failedBatchCount

    percentage
}
```

`percentage` is optional and informational.

Counts are authoritative.

---

# Part XI — Translation Attempt

## 57. TranslationAttempt

```text
TranslationAttempt {
    translationAttemptId
    translationJobId

    attemptNumber
    reason

    providerSelection
    status

    batchIds[]

    startedAt
    completedAt

    normalizedFailure
    usage
}
```

---

## 58. Attempt Reason

Possible values:

```text
INITIAL
AUTOMATIC_RETRY
PROVIDER_FALLBACK
BATCH_RETRY
VALIDATION_RETRY
MANUAL_RETRY
```

A manual retry may remain in the same job only when it preserves the same translation intent.

A manual request that changes profile, provider constraints, context, or terminology should create a new job.

---

## 59. Provider Selection Metadata

```text
ProviderSelectionMetadata {
    providerId
    providerClass
    modelIdentifier

    selectionReason
    fallbackIndex
}
```

Provider identifiers are opaque.

Provider credentials and raw configuration must never appear here.

---

# Part XII — Translation Batch

## 60. TranslationBatch

```text
TranslationBatch {
    translationBatchId
    translationAttemptId

    sequence

    preparedSegmentIds[]
    contextSegmentIds[]

    status

    providerExecutionMetadata

    startedAt
    completedAt

    translatedSegmentIds[]
    normalizedFailure
}
```

---

## 61. Batch Rules

A valid batch must:

* contain at least one translatable prepared segment;
* contain no duplicate prepared segment IDs;
* preserve upstream sequence information;
* belong to one attempt;
* not mix incompatible target languages;
* not exceed resolved provider limits;
* remain independently traceable.

---

## 62. Batch Sequence

Batch sequence represents assembly order within an attempt.

It must not be used as a substitute for prepared segment sequence.

---

## 63. Batch Context

A batch may contain:

```text
translatable segments
context-only segments
terminology constraints
profile instructions
```

Only translatable segments produce public translated segments.

---

# Part XIII — Provider Execution Metadata

## 64. ProviderExecutionMetadata

Normalized provider execution information:

```text
ProviderExecutionMetadata {
    providerId
    modelIdentifier

    providerRequestId

    executionRegion
    localExecution

    requestStartedAt
    responseReceivedAt
    latency

    cachedByProvider
    streamingUsed

    usage
}
```

Raw provider request and response payloads are not public contracts.

---

## 65. TranslationUsage

```text
TranslationUsage {
    inputCharacters
    outputCharacters

    inputTokens
    outputTokens
    totalTokens

    providerReported
    estimated

    monetaryCost
    currency
}
```

All usage fields are optional except when required by deployment policy.

Estimated usage must be distinguishable from provider-reported usage.

---

# Part XIV — Translation Results

## 66. TranslationResult

```text
TranslationResult {
    translationResultId
    translationJobId

    resultRevision

    completion

    translatedSegments[]

    missingPreparedSegmentIds[]
    failedPreparedSegmentIds[]

    warnings[]

    activeVariantId

    sourceIdentity
    configurationIdentity
    contextIdentity
    knowledgeIdentity

    statistics

    createdAt
    finalizedAt
}
```

---

## 67. Result Revision

Partial or progressive results may produce increasing result revisions.

```text
resultRevision = monotonically increasing within one job
```

A later revision must not contain an older source identity.

---

## 68. Translation Completion

```text
TranslationCompletion {
    status
    complete
    authoritative
}
```

Possible completion statuses:

```text
COMPLETED
COMPLETED_WITH_WARNINGS
PARTIAL
FAILED
CANCELLED
SUPERSEDED
INVALIDATED
```

Detailed lifecycle meaning belongs in `STATES.md`.

---

## 69. Authoritative Result

A result may be marked authoritative only when:

* its source revision remains current according to publication policy;
* the job is not cancelled;
* the job is not superseded;
* required validation passed;
* its variant is active;
* publication policy permits its completion level.

---

## 70. TranslatedSegment

```text
TranslatedSegment {
    translatedSegmentId

    preparedSegmentId
    sourceSequence

    translatedText
    targetLanguage

    variantId

    completion
    warnings[]

    confidence

    providerContribution

    createdAt
}
```

---

## 71. Segment Completion

Possible values:

```text
COMPLETE
COMPLETE_WITH_WARNINGS
MISSING
FAILED
CANCELLED
SUPERSEDED
```

Only complete or warning-bearing complete segments should normally be published to Presentation.

---

## 72. Translated Text Rules

`translatedText` must:

* be valid Unicode text;
* correspond to exactly one prepared segment;
* not contain provider control syntax;
* not contain internal structural wrappers;
* preserve required source markers where policy demands;
* be empty only when the source is intentionally non-translatable.

An intentionally untranslated segment must include an explanatory flag or warning.

---

## 73. Segment Alignment

The required relationship is:

```text
TranslatedSegment.preparedSegmentId
        →
PreparedSegment.preparedSegmentId
```

Each active variant may contain at most one authoritative translated segment per prepared segment.

---

## 74. Segment Confidence

```text
TranslationConfidence {
    value
    source
    scale
}
```

Possible confidence sources:

```text
PROVIDER
VALIDATOR
HEURISTIC
COMBINED
```

Confidence is optional.

A missing confidence value does not make a translation invalid.

---

## 75. Provider Contribution

```text
ProviderContribution {
    providerId
    modelIdentifier
    translationAttemptId
    translationBatchId
}
```

This supports diagnostics without exposing provider payloads.

---

# Part XV — Translation Variants

## 76. TranslationVariant

```text
TranslationVariant {
    translationVariantId
    translationJobId

    variantType

    profileIdentity
    providerIdentity

    translatedSegmentIds[]

    status

    createdBy
    createdAt

    parentVariantId
}
```

---

## 77. Variant Type

Possible values:

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

## 78. Variant Immutability

A published variant must not be modified in place.

Changes create a new variant.

```text
Variant A
    ↓ correction
Variant B
```

`parentVariantId` may preserve lineage.

---

## 79. Active Variant

Only one variant should normally be active for a given:

```text
reading context
+
prepared source revision
+
target language
```

Selecting a variant changes activation state.

It does not delete other variants.

---

# Part XVI — Cancellation

## 80. CancelTranslationCommand

```text
CancelTranslationCommand {
    metadata

    translationJobId

    reason
    scope
}
```

Possible scopes:

```text
ENTIRE_JOB
ACTIVE_ATTEMPT
PENDING_BATCHES
```

Logical cancellation of the whole job prevents authoritative publication.

---

## 81. Cancellation Reason

Possible values:

```text
USER_REQUESTED
SOURCE_CHANGED
NAVIGATION_CHANGED
SESSION_CLOSED
TARGET_LANGUAGE_CHANGED
NEW_JOB_REPLACED_OLD
RESOURCE_LIMIT
SYSTEM_SHUTDOWN
OTHER
```

---

## 82. CancelTranslationResult

```text
CancelTranslationResult {
    commandId
    translationJobId

    disposition
    cancellationRequestedAt
}
```

Possible dispositions:

```text
CANCELLATION_ACCEPTED
ALREADY_TERMINAL
JOB_NOT_FOUND
REJECTED
```

---

# Part XVII — Retry

## 83. RetryTranslationCommand

Requests another attempt for the same logical job.

```text
RetryTranslationCommand {
    metadata

    translationJobId

    scope
    failedBatchIds[]

    providerOverride
    retryReason
}
```

Possible scopes:

```text
FAILED_BATCHES
ACTIVE_ATTEMPT
ENTIRE_JOB
```

A provider override must remain compatible with the existing job configuration.

If it changes translation intent materially, callers must use `RequestRetranslationCommand`.

---

## 84. RetryTranslationResult

```text
RetryTranslationResult {
    commandId
    translationJobId

    translationAttemptId

    disposition
}
```

Possible dispositions:

```text
ATTEMPT_CREATED
RETRY_NOT_ALLOWED
JOB_ALREADY_ACTIVE
JOB_NOT_FOUND
REJECTED
```

---

# Part XVIII — Retranslation

## 85. RequestRetranslationCommand

Creates a new logical translation job derived from prior work.

```text
RequestRetranslationCommand {
    metadata

    sourceTranslationJobId
    sourceVariantId

    source

    newConfiguration
    newContext
    newKnowledge

    reason

    activateWhenCompleted
}
```

---

## 86. Retranslation Reason

Possible values:

```text
USER_DISSATISFIED
CHANGE_PROFILE
CHANGE_PROVIDER_POLICY
CHANGE_TARGET_LANGUAGE
GLOSSARY_UPDATED
CONTEXT_UPDATED
REQUEST_LITERAL_VARIANT
REQUEST_NATURAL_VARIANT
OTHER
```

---

## 87. RequestRetranslationResult

```text
RequestRetranslationResult {
    commandId

    sourceTranslationJobId
    newTranslationJobId

    disposition
}
```

---

# Part XIX — Invalidation

## 88. InvalidateTranslationCommand

Marks existing translation output as no longer eligible for authoritative use.

```text
InvalidateTranslationCommand {
    metadata

    translationJobId
    translationVariantId

    reason
    invalidationScope
}
```

Possible scopes:

```text
JOB
VARIANT
SEGMENTS
CACHE_ENTRY
```

For `SEGMENTS`, explicit segment IDs are required.

---

## 89. Invalidation Reason

Possible values:

```text
SOURCE_REVISION_CHANGED
ALIGNMENT_CHANGED
GLOSSARY_CHANGED
CONTEXT_CHANGED
QUALITY_REJECTED
SECURITY_POLICY
PRIVACY_POLICY
MANUAL_ADMINISTRATIVE
OTHER
```

Invalidation does not necessarily delete historical data.

---

# Part XX — Variant Selection

## 90. SelectTranslationVariantCommand

```text
SelectTranslationVariantCommand {
    metadata

    translationJobId
    translationVariantId

    readingSessionId
}
```

The variant must be compatible with the active source revision and target language.

---

## 91. SelectTranslationVariantResult

```text
SelectTranslationVariantResult {
    commandId

    translationJobId
    translationVariantId

    disposition
    activatedAt
}
```

---

# Part XXI — User Correction

## 92. SubmitTranslationCorrectionCommand

Creates a corrected translation variant.

```text
SubmitTranslationCorrectionCommand {
    metadata

    translationJobId
    baseVariantId

    corrections[]

    activateWhenCreated
    proposeKnowledgeUpdate
}
```

---

## 93. SegmentCorrection

```text
SegmentCorrection {
    preparedSegmentId

    correctedText

    correctionReason
    notes
}
```

A correction must target an existing prepared segment in the job source identity.

---

## 94. Correction Result

```text
SubmitTranslationCorrectionResult {
    commandId

    translationJobId
    translationVariantId

    correctedPreparedSegmentIds[]

    knowledgeProposalId

    disposition
}
```

A proposed Knowledge update is optional and belongs to the Knowledge workflow.

Translation must not silently change global terminology.

---

# Part XXII — Warnings

## 95. TranslationWarning

```text
TranslationWarning {
    code
    category

    severity
    message

    translationJobId
    translationAttemptId
    translationBatchId

    preparedSegmentIds[]

    metadata
}
```

Warnings are structured machine-readable outcomes.

The human-readable message is supplementary.

---

## 96. Warning Severity

Possible values:

```text
INFO
NOTICE
DEGRADED
```

Fatal conditions are errors, not warnings.

---

## 97. Initial Warning Categories

```text
MISSING_CONTEXT
LOW_CONFIDENCE
AMBIGUOUS_MEANING
TERMINOLOGY_CONFLICT
SOURCE_INCOMPLETE
SOURCE_LANGUAGE_UNCERTAIN
UNTRANSLATED_FRAGMENT
OUTPUT_LENGTH_ANOMALY
PROVIDER_FALLBACK_USED
PARTIAL_RESULT
SOUND_EFFECT_PRESERVED
MIXED_LANGUAGE_CONTENT
PRONOUN_AMBIGUITY
CACHE_RESULT_REUSED
```

The complete warning catalog belongs in `ERRORS.md`.

---

# Part XXIII — Statistics

## 98. TranslationStatistics

```text
TranslationStatistics {
    selectedSegmentCount
    translatedSegmentCount
    missingSegmentCount
    failedSegmentCount

    sourceCharacterCount
    translatedCharacterCount

    attemptCount
    batchCount
    retryCount
    fallbackCount

    cacheHit

    queueDuration
    executionDuration
    totalDuration

    usage
}
```

Statistics are informational and must not determine source alignment.

---

# Part XXIV — Queries

## 99. Query Contract Set

Recommended query contracts:

```text
GetTranslationJob
GetTranslationResult
GetActiveTranslation
ListTranslationVariants
GetTranslationProgress
```

Queries do not change translation state.

---

## 100. GetTranslationJobQuery

```text
GetTranslationJobQuery {
    translationJobId

    includeAttempts
    includeBatches
}
```

Returns:

```text
GetTranslationJobResult {
    job
    attempts[]
    batches[]
}
```

---

## 101. GetTranslationResultQuery

```text
GetTranslationResultQuery {
    translationJobId

    resultRevision
    variantId

    includeWarnings
    includeStatistics
}
```

When no revision or variant is supplied, the active authoritative result is returned.

---

## 102. GetActiveTranslationQuery

```text
GetActiveTranslationQuery {
    readingSessionId

    preparedDocumentId
    contentRevision

    targetLanguage
}
```

Returns the currently active compatible translation variant, when available.

---

## 103. ListTranslationVariantsQuery

```text
ListTranslationVariantsQuery {
    translationJobId

    includeInvalidated
}
```

Returns immutable variant summaries.

---

## 104. GetTranslationProgressQuery

```text
GetTranslationProgressQuery {
    translationJobId
}
```

Returns:

```text
TranslationProgress
Current job status
Active attempt identity
Partial-result availability
```

---

# Part XXV — Idempotency

## 105. Start Command Idempotency

Equivalent `StartTranslationCommand` submissions may resolve to the same active job when all relevant semantic inputs match.

Relevant inputs may include:

```text
preparedDocumentId
contentRevision
selected segment identity
source language
target language
translation profile revision
terminology revision
context revision
provider policy
publication policy
```

The caller-provided `IdempotencyKey` may additionally force deduplication within an implementation-defined time window.

---

## 106. Retry Idempotency

Repeated equivalent retry commands must not create uncontrolled concurrent attempts.

A retry may be rejected or resolved to an already active attempt.

---

## 107. Cancellation Idempotency

Cancelling an already cancelled or terminal job must not recreate work or change historical results.

---

# Part XXVI — Stale-Result Safety

## 108. Publication Identity

Before a result becomes authoritative, the module must verify at least:

```text
ReadingSessionId
PreparedDocumentId
ContentRevision
TranslationJobId
TranslationAttemptId
```

Where relevant, it should also verify:

```text
TargetLanguage
ContextRevision
GlossaryRevision
ActiveVariantId
```

---

## 109. Stale Result Rule

A result is stale when it no longer matches the authoritative source or translation intent.

A stale result may be retained for diagnostics.

It must not overwrite current work.

---

# Part XXVII — Validation Rules

## 110. Command Validation

`StartTranslationCommand` must be rejected when:

* no prepared source is provided;
* no target language is provided;
* selected segments do not exist;
* selected segments contain duplicate identities;
* content revision is unavailable;
* provider policy is impossible to satisfy;
* local-only policy has no eligible local provider;
* required terminology data is unavailable;
* publication policy is internally inconsistent.

---

## 111. Provider Output Validation

Provider output must be rejected or degraded when:

* required segment identities are missing;
* unexpected segment identities are returned;
* duplicate outputs exist for one source segment;
* output cannot be structurally parsed;
* segment order cannot be reconstructed;
* target text is empty without justification;
* provider control content leaks into output;
* output violates locked terminology;
* result belongs to another attempt or batch.

Detailed handling belongs in `ERRORS.md`.

---

## 112. Result Assembly Rules

Final result assembly must:

* order translated segments using source sequence;
* preserve source identifiers;
* report missing segments explicitly;
* report failed segments explicitly;
* preserve warning associations;
* not silently drop selected segments;
* not treat context-only segments as translated output.

---

# Part XXVIII — Privacy and Security

## 113. Sensitive Fields

Public contracts must never contain:

* provider API keys;
* provider bearer tokens;
* provider secret configuration;
* raw authorization headers;
* unredacted credential errors;
* raw internal prompts;
* unrelated reading-session data.

---

## 114. Source Content Handling

Raw source and translated text may appear in necessary result contracts.

They should not appear by default in:

* logs;
* telemetry;
* error metadata;
* provider-selection metadata;
* event diagnostics.

---

## 115. Untrusted Source Rule

Source text, context text, and glossary notes are untrusted data.

They must not be interpreted as trusted system commands.

```text
Source content
    ≠
Translation policy
```

Provider adapters are responsible for preserving this separation.

---

# Part XXIX — Compatibility

## 116. Text Processing Compatibility

Translation depends on stable definitions from:

```text
modules/text-processing/CONTRACTS.md
```

or the corresponding canonical contract file.

Translation must not redefine:

* prepared document ownership;
* prepared segment ownership;
* source ordering;
* extraction confidence semantics;
* normalized source lifecycle.

---

## 117. Presentation Compatibility

Presentation may depend on:

```text
PreparedSegmentId
TranslatedSegmentId
TranslatedText
SourceSequence
TranslationVariantId
Completion
Warnings
ContentRevision
```

Presentation must not need provider-specific metadata to display translated content.

---

## 118. Knowledge Compatibility

Knowledge provides revisioned terminology and contextual knowledge.

Translation consumes:

```text
KnowledgeSnapshotId
GlossaryRevision
TermConstraint[]
CharacterContext[]
```

Translation must not depend on the Knowledge module’s persistence schema.

---

## 119. Event Compatibility

Events derived from these contracts must not expose larger mutable objects unnecessarily.

`EVENTS.md` should prefer:

* stable identifiers;
* state snapshots;
* result revision references;
* compact progress;
* normalized warnings.

---

# Part XXX — Core Contract Invariants

## 120. Invariant 1 — Prepared input only

Translation starts from prepared content, not canonical raw OCR.

## 121. Invariant 2 — Stable alignment

Every published translated segment references one prepared segment.

## 122. Invariant 3 — No duplicate active alignment

One active variant cannot contain multiple authoritative translated segments for the same prepared segment.

## 123. Invariant 4 — Batch is not alignment

Batch membership must never replace segment identity.

## 124. Invariant 5 — Job and attempt are distinct

Retrying provider execution creates an attempt, not automatically a new logical job.

## 125. Invariant 6 — Intent changes create new jobs

Changes to source revision, target language, translation profile, or other material semantic input require a new job.

## 126. Invariant 7 — Immutable variants

Published translation variants are never changed in place.

## 127. Invariant 8 — Provider isolation

Provider-specific requests and responses remain internal.

## 128. Invariant 9 — Cancellation blocks authority

Cancelled work cannot become authoritative.

## 129. Invariant 10 — Supersession blocks authority

Superseded work cannot overwrite newer work.

## 130. Invariant 11 — Missing segments are explicit

A result must not silently omit selected source segments.

## 131. Invariant 12 — Context is not output

Context-only segments do not create public translated segments.

## 132. Invariant 13 — Source order is upstream-owned

Translation preserves upstream sequence and does not infer comic reading order.

## 133. Invariant 14 — Credentials remain private

Provider credentials never appear in public contracts, results, warnings, or events.

---

# Part XXXI — Initial MVP Contract Surface

## 134. Required MVP Commands

```text
StartTranslation
CancelTranslation
RetryTranslation
RequestRetranslation
```

## 135. Required MVP Models

```text
TranslationSource
TranslationConfiguration
ProviderPolicy
TerminologyPolicy
ContextPolicy
TranslationExecutionPolicy
TranslationPublicationPolicy

TranslationJob
TranslationAttempt
TranslationBatch

TranslationResult
TranslatedSegment
TranslationWarning
TranslationStatistics
```

## 136. Required MVP Behaviors

The contracts must support:

* Chinese-to-Vietnamese translation;
* English-to-Vietnamese translation;
* prepared novel text;
* OCR-derived prepared comic segments;
* multi-segment batches;
* at least one provider adapter;
* basic terminology constraints;
* retry attempts;
* provider fallback;
* cancellation;
* partial completion;
* stale-result rejection;
* immutable variants;
* cache-aware execution.

---

# Part XXXII — Deferred Contract Extensions

The following may be added later without changing core ownership:

```text
Translation quality evaluation
Provider comparison results
Automatic glossary proposals
Speaker relationship inference
Translation memory matches
Distributed worker leases
Long-running chapter translation
User rating contracts
Cost-budget contracts
Advanced prefetch contracts
Collaborative correction review
```

Extensions must preserve existing identifiers and alignment rules.

---

# Part XXXIII — Example Conceptual Flow

## 137. Starting a Comic Translation

```text
StartTranslationCommand
    source:
        preparedDocumentId = comic-page-42
        contentRevision = 7
        segmentSelection = ALL

    configuration:
        sourceLanguage = zh-Hans
        targetLanguage = vi
        translationProfile = COMIC_NATURAL

    publicationPolicy:
        mode = PROGRESSIVE

    priority:
        VISIBLE
```

Translation creates:

```text
TranslationJob
    └── TranslationAttempt 1
            ├── TranslationBatch 1
            │       ├── Bubble 1
            │       ├── Bubble 2
            │       └── Bubble 3
            │
            └── TranslationBatch 2
                    ├── Bubble 4
                    └── Narration 1
```

The result preserves:

```text
Bubble 1 → TranslatedSegment 1
Bubble 2 → TranslatedSegment 2
Bubble 3 → TranslatedSegment 3
Bubble 4 → TranslatedSegment 4
Narration 1 → TranslatedSegment 5
```

---

## 138. Provider Retry Example

```text
TranslationJob A
      ↓
Attempt 1
      ↓
Provider timeout
      ↓
Attempt 2
      ↓
Fallback provider
      ↓
Completed result
```

The same `TranslationJobId` is preserved.

Each execution receives a different `TranslationAttemptId`.

---

## 139. Retranslation Example

The user changes from natural translation to literal translation.

```text
TranslationJob A
    profile = COMIC_NATURAL

RequestRetranslation
    profile = LITERAL

TranslationJob B
    profile = LITERAL
```

This creates a new logical job because the translation intent changed.

---

## 140. Stale Result Example

```text
Document revision 7
      ↓
TranslationJob A starts

Document changes to revision 8
      ↓
TranslationJob B starts

TranslationJob A completes late
      ↓
Result retained as non-authoritative
      ↓
Must not replace TranslationJob B
```

---

# Part XXXIV — Related Documents

```text
modules/translation/MODULE.md
modules/translation/EVENTS.md
modules/translation/ERRORS.md
modules/translation/STATES.md
modules/translation/README.md
```

Architecture references:

```text
docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Upstream references:

```text
modules/text-processing/MODULE.md
modules/text-processing/CONTRACTS.md
```

Future integration references:

```text
modules/knowledge/MODULE.md
modules/presentation/MODULE.md
modules/provider-management/MODULE.md
modules/reading-session/MODULE.md
```

---

# 141. Summary

The Translation public contract is centered on six distinct concepts:

```text
PreparedSegment
    = source alignment unit

TranslationBatch
    = provider execution unit

TranslationAttempt
    = one execution attempt

TranslationJob
    = one logical translation intent

TranslatedSegment
    = aligned translated output

TranslationVariant
    = immutable translation version
```

The primary flow is:

```text
StartTranslationCommand
        ↓
TranslationJob
        ↓
TranslationAttempt
        ↓
TranslationBatch[]
        ↓
Provider adapters
        ↓
TranslatedSegment[]
        ↓
TranslationResult
        ↓
Active TranslationVariant
```

These contracts ensure that CRAI can:

* translate novels and comics;
* use multiple providers;
* retry safely;
* preserve contextual quality;
* support progressive output;
* reject stale results;
* retain alternative translations;
* remain independent from provider APIs;
* preserve exact source-to-translation alignment.
