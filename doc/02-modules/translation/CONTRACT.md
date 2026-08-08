# Translation Module Contract

> **Project:** CRAI
> **Module:** Translation
> **Path:** `02-modules/translation/CONTRACT.md`
> **Version:** 1.0.0
> **Status:** Architecture Draft
> **Source of Truth:** `MODULE.md`

---

# 1. Purpose

Tài liệu này định nghĩa public contract của Translation Module.

Contract bao phủ:

* Runtime-facing Attempt Input
* Runtime-facing Attempt Output
* Translation Intent
* SourceDocument selection
* Translation Profile
* Translation Unit
* Translation Plan
* Translation Batch
* Context Snapshot
* Knowledge / Terminology references
* Provider Policy
* provider-neutral execution contracts
* normalized provider output
* Translated Unit
* Candidate Translation Artifact
* Published Translation Artifact
* partial output
* translation variants
* warnings
* module errors
* RetryHint
* provider provenance
* semantic compatibility
* privacy/security
* producer obligations
* consumer obligations
* Runtime obligations
* contract evolution

Contract này không định nghĩa:

* Runtime WorkItem lifecycle
* Runtime Attempt lifecycle
* queue admission
* Scheduler behavior
* retry scheduling
* cancellation lifecycle
* stale-result authority
* Artifact publication lifecycle
* Provider SDK requests/responses
* Provider credentials
* persistent Knowledge schema
* Presentation layout

---

# 2. Contract Boundary

Canonical flow:

```text
SourceDocumentArtifact
        ↓
TranslationAttemptInput
        ↓
Translation Module
        ↓
TranslationPlan
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
TranslationAttemptOutput
        ↓
Runtime Authority Validation
        ↓
Artifact Store
        ↓
TranslationArtifact
```

---

# 3. Contract Design Principles

## 3.1 Provider Neutrality

Public contracts must not expose:

```text
OpenAIMessage
GeminiContent
ClaudeMessage
DeepLRequest
provider-native response objects
provider SDK enums
```

Provider-specific types remain inside Adapter boundary.

---

## 3.2 Stable Source Boundary

Translation begins from:

```text
SourceDocumentArtifact
```

not:

```text
raw OCR output
PreparedDocument
PreparedSegment
```

---

## 3.3 Translation Owns Translation Units

```text
SourceBlock
    ≠
TranslationUnit
```

Translation decides Translation Unit boundaries.

---

## 3.4 Explicit Alignment

Every TranslatedUnit must remain traceable through:

```text
TranslatedUnit
    ↓
TranslationUnit
    ↓
SourceBlockRef[]
    ↓
SourceDocumentArtifact
```

---

## 3.5 Batch Is Not Alignment

Batch membership must never become source/translation identity.

---

## 3.6 Runtime Separation

Translation does not own:

```text
Job lifecycle
Attempt lifecycle
Retry lifecycle
Cancellation lifecycle
Publication lifecycle
```

---

## 3.7 Immutable Candidate / Artifact

Candidate becomes immutable after module validation.

Published Artifact is immutable.

---

## 3.8 Explicit Partiality

Missing or failed Units are explicit.

No silent omission.

---

## 3.9 Source Content Is Untrusted

Source, context and glossary notes are data.

They are not trusted provider instructions.

---

# 4. Contract Version

```text
TranslationContractVersion
├── Major
├── Minor
└── Patch
```

Initial:

```text
1.0.0
```

Breaking semantic changes require new major version.

---

# 5. Shared Identifiers

Translation consumes shared IDs:

```text
SessionId?
RevisionId
WorkItemId
AttemptId

ArtifactId
CandidateArtifactId

ConfigurationSnapshotId
KnowledgeSnapshotId?
TraceId
```

Translation does not redefine their ownership.

---

# 6. Translation-Owned Identifiers

Translation owns semantic IDs:

```text
TranslationIntentId

TranslationPlanId

TranslationUnitId

TranslationBatchId

TranslatedUnitId

TranslationVariantId
```

No:

```text
TranslationJobId
TranslationAttemptId
```

is required in the core contract.

---

# 7. Translation Intent Identity

`TranslationIntentId` identifies immutable semantic translation intent.

Material dependencies may include:

```text
SourceDocument semantic identity

selected source scope

source language policy

target language

Translation Profile version

Knowledge Snapshot identity

Context Snapshot identity

Provider Policy semantic constraints

Translation Strategy version

Privacy Partition
```

Equivalent retries may preserve intent identity.

Material semantic change produces new intent identity.

---

# 8. Runtime Context

```text
TranslationRuntimeContext
├── ContractVersion
├── ApplicationInstanceId
├── SessionId?
├── RevisionId
├── WorkItemId
├── AttemptId
├── ConfigurationSnapshotId
├── TraceContext
└── CreatedAt
```

Runtime identity is trace/control context.

It does not become Translation domain lifecycle.

---

# 9. SourceDocument Artifact Reference

Primary input:

```text
SourceDocumentArtifactRef
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── ContentIdentity
├── PrivacyPartition
└── Metadata?
```

Expected:

```text
ArtifactType = SOURCE_DOCUMENT_ARTIFACT
```

---

# 10. SourceDocument Consumption Rules

Translation assumes SourceDocument contract defines:

* SourceBlock IDs
* source structure
* Raw/Normalized source semantics
* SourceBlockSequence
* excluded blocks
* language hints
* traceability
* completeness

Translation must not redefine them.

---

# 11. Translation Source Selection

```text
TranslationSourceSelection
├── Mode
├── SourceBlockIds[]
├── IncludeExcludedBlocks
└── SelectionMetadata?
```

Modes:

```text
ALL_TRANSLATABLE

EXPLICIT_BLOCKS

SEQUENCE_RANGE

VISIBLE_SCOPE

PROFILE_DEFAULT
```

`EXPLICIT_BLOCKS` requires valid `SourceBlockIds`.

---

# 12. Source Ordering

Translation follows:

```text
SourceDocument.BlockSequence
```

It must not infer comic Reading Order from geometry.

Command/request array order is not authoritative source order.

---

# 13. Translation Attempt Input

```text
TranslationAttemptInput
├── RuntimeContext
├── SourceDocumentArtifactRef
├── SourceSelection
├── TranslationIntent
├── ExecutionContextRef
├── CancellationContextRef
├── PrivacyContextRef
└── DiagnosticsContextRef?
```

---

# 14. Translation Intent

```text
TranslationIntent
├── TranslationIntentId
├── SourceLanguagePolicy
├── TargetLanguage
├── TranslationProfileRef
├── ProviderPolicy
├── TerminologyPolicy
├── ContextPolicy
├── PartialResultPolicy
├── VariantPolicy
└── Metadata?
```

Intent contains semantic requirements, not Runtime state.

---

# 15. Source Language Policy

```text
SourceLanguagePolicy
├── Mode
├── LanguageTag?
└── DetectionPolicy?
```

Modes:

```text
EXPLICIT

UPSTREAM_HINT

AUTO_DETECT
```

When `EXPLICIT`, `LanguageTag` is required.

---

# 16. Target Language

Target language is explicit.

```text
TargetLanguage
├── LanguageTag
└── LocalePreferences?
```

Recommended language-tag format:

```text
BCP-47 compatible
```

Architecture is language-pair independent.

---

# 17. Translation Profile Reference

```text
TranslationProfileRef
├── ProfileId
├── ProfileVersion
└── CustomProfileRef?
```

Initial profiles:

```text
NOVEL_NATURAL

COMIC_NATURAL

GENERAL_NATURAL

LITERAL

CUSTOM
```

---

# 18. Profile Boundary

Translation Profile may affect:

* naturalness
* literalness
* dialogue style
* pronouns
* honorifics
* terminology behavior
* compactness
* sound-effect behavior
* punctuation
* output style

It must not expose raw provider prompts as core contract.

---

# 19. Translation Plan

```text
TranslationPlan
├── PlanId
├── TranslationIntentId
├── SourceDocumentArtifactRef
├── SourceSelection
├── TranslationProfileRef
├── TranslationUnitRefs[]
├── TranslationBatchRefs[]
├── ContextSnapshotRef?
├── KnowledgeSnapshotRef?
├── ProviderRequirements
├── ValidationPolicy
├── PartialResultPolicy
├── CompatibilityPolicy
├── ConfigurationSnapshotId
└── PrivacyConstraints
```

Plan is immutable once `READY`.

---

# 20. Translation Unit

```text
TranslationUnit
├── TranslationUnitId
├── SourceBlockRefs[]
├── SourceTextView
├── SourceSequence
├── SourceLanguageHint?
├── TargetLanguage
├── StructuralType?
├── ContextRequirementRefs[]
├── TerminologyConstraintRefs[]
├── AlignmentMetadata
├── TranslationProfileRef
└── Metadata?
```

---

# 21. Translation Unit Rules

A Translation Unit must:

1. reference at least one SourceBlock
2. preserve SourceBlock identity
3. preserve deterministic source order
4. belong to one Translation Plan
5. have exactly one target language
6. remain traceable to SourceDocument
7. not include context-only blocks as target content unless selected
8. not contain provider-native schema

---

# 22. Translation Unit Mapping

Allowed:

```text
1 SourceBlock → 1 TranslationUnit
```

```text
N SourceBlocks → 1 TranslationUnit
```

Potentially:

```text
1 SourceBlock → N TranslationUnits
```

when required by size or semantic policy.

In split case, alignment metadata must preserve source ranges/identity.

---

# 23. Source Text View

```text
TranslationSourceTextView
├── TranslationUnitId
├── Text
├── SourceBlockRefs[]
├── SourceRanges?
├── PreserveMarkers[]
└── Metadata?
```

The source view is Translation-owned derived input.

It must not mutate SourceDocument.

---

# 24. Context Snapshot

```text
TranslationContextSnapshot
├── ContextSnapshotId
├── SourceDocumentArtifactRef
├── PreviousSourceRefs[]
├── FollowingSourceRefs[]
├── PreviousTranslationRefs[]
├── DialogueContextRefs[]
├── ParagraphContextRefs[]
├── ChapterSummaryRef?
├── CharacterContextRefs[]
├── AdditionalContextRefs[]
└── CreatedAt
```

Context may be embedded or referenced according to privacy/runtime architecture.

---

# 25. Context-Only Entry

```text
ContextEntry
├── ContextEntryId
├── Relationship
├── SourceRef?
├── TranslationRef?
├── ContentView?
└── Metadata?
```

Relationships:

```text
PREVIOUS

FOLLOWING

SAME_DIALOGUE_GROUP

SAME_PARAGRAPH

CHAPTER_CONTEXT

CHARACTER_CONTEXT

RELATED_REFERENCE
```

Context-only entries do not create TranslatedUnits.

---

# 26. Context Policy

```text
ContextPolicy
├── PreviousUnitLimit
├── FollowingUnitLimit
├── IncludeDialogueContext
├── IncludeParagraphContext
├── IncludeChapterSummary
├── IncludePriorTranslations
├── IncludeCharacterKnowledge
├── MaximumContextCharacters
├── MaximumEstimatedTokens
└── MissingContextBehavior
```

---

# 27. Missing Context Behavior

```text
USE_AVAILABLE_CONTEXT

CONTINUE_WITH_WARNING

FAIL
```

Interactive default recommendation:

```text
USE_AVAILABLE_CONTEXT
```

---

# 28. Knowledge Snapshot Reference

```text
KnowledgeSnapshotRef
├── KnowledgeSnapshotId
├── Revision?
├── ContentIdentity?
└── Metadata?
```

Translation does not own Knowledge persistence.

---

# 29. Terminology Policy

```text
TerminologyPolicy
├── KnowledgeSnapshotRef?
├── TermConstraints[]
├── NamePolicy
├── HonorificPolicy
├── TransliterationPolicy
└── ConflictPolicy
```

---

# 30. Term Constraint

```text
TermConstraint
├── ConstraintId
├── SourceTerm
├── TargetTerm
├── Strength
├── Scope
├── ScopeRefs[]
├── CaseSensitive?
└── Metadata?
```

Strength:

```text
LOCKED

PREFERRED

SUGGESTED

CONTEXTUAL
```

---

# 31. Terminology Scope

Possible scopes:

```text
GLOBAL

SERIES

CHAPTER

DOCUMENT

SOURCE_BLOCK

TRANSLATION_UNIT

CHARACTER
```

---

# 32. Conflict Policy

```text
FAIL

WARN_AND_CONTINUE

PREFER_LOCKED

PREFER_MOST_SPECIFIC_SCOPE
```

`LOCKED` must take precedence over weaker constraints.

---

# 33. Name Policy

```text
USE_KNOWLEDGE_MAPPING

PRESERVE_ORIGINAL

SINO_VIETNAMESE

PHONETIC_TRANSLITERATION

PROVIDER_DEFAULT
```

Knowledge mapping takes precedence when required by policy.

---

# 34. Honorific Policy

```text
CONTEXTUAL_VIETNAMESE

PRESERVE_SOURCE_STYLE

NEUTRAL

LITERAL

CUSTOM
```

---

# 35. Sound Effect Policy

```text
TRANSLATE

TRANSLITERATE

PRESERVE

SKIP

PRESENTATION_HANDLING
```

Applies when SourceBlock type indicates `SOUND_EFFECT`.

---

# 36. Provider Policy

```text
ProviderPolicy
├── Mode
├── PreferredProviderId?
├── AllowedProviderIds[]
├── ExcludedProviderIds[]
├── FallbackPolicy
├── LocalityRequirement
├── CostPreference
├── LatencyPreference
├── QualityPreference
└── ModelPolicy
```

---

# 37. Provider Mode

```text
AUTOMATIC

PREFERRED

REQUIRED

LOCAL_ONLY

REMOTE_ONLY
```

`REQUIRED` forbids fallback to another provider.

---

# 38. Locality Requirement

```text
ANY

PREFER_LOCAL

LOCAL_REQUIRED

REMOTE_ALLOWED
```

`LOCAL_REQUIRED` prohibits remote transmission.

---

# 39. Provider Preferences

```text
LOW

BALANCED

HIGH
```

Applicable to:

* cost
* latency
* quality

These express intent, not scheduling algorithm.

---

# 40. Model Policy

```text
ModelPolicy
├── PreferredModelClass?
├── RequiredCapabilities[]
├── MaximumContextSize?
├── DeterministicPreference?
└── OpaqueModelHint?
```

Provider-native model names are opaque hints/configuration, not architecture dependencies.

---

# 41. Provider Requirements

Resolved Translation Plan may declare:

```text
TranslationProviderRequirements
├── SourceLanguages[]
├── TargetLanguage
├── StructuredOutputRequired
├── BatchRequired
├── GlossarySupportRequired
├── StreamingRequired
├── LocalExecutionRequired
├── CancellationSupportPreferred
├── MaximumPayloadRequired?
├── ContextWindowRequired?
└── CapabilityTags[]
```

Provider Management resolves availability.

---

# 42. Fallback Policy

```text
FallbackPolicy
├── Enabled
├── MaximumFallbacks
└── EligibleFailureCategories[]
```

Fallback prohibited when:

* Provider mode = REQUIRED
* privacy disallows candidate provider
* fallback disabled
* no eligible provider
* semantic constraints cannot be preserved

---

# 43. Translation Batch

```text
TranslationBatch
├── TranslationBatchId
├── Sequence
├── TranslationUnitIds[]
├── ContextEntryRefs[]
├── TerminologyConstraintRefs[]
├── EstimatedCharacters?
├── EstimatedTokens?
├── ProviderRequirements
├── BatchPolicyMetadata
└── Metadata?
```

No Runtime Attempt IDs are stored as batch ownership.

---

# 44. Batch Rules

A valid Batch must:

1. contain ≥1 translatable TranslationUnit
2. contain no duplicate Unit IDs
3. preserve Unit source order
4. use one target language
5. respect semantic/provider limits
6. remain independently traceable
7. not use Batch sequence as TranslationUnit alignment identity

---

# 45. Batching Policy

```text
BatchingPolicy
├── Strategy
├── MaximumUnits?
├── MaximumCharacters?
├── MaximumEstimatedTokens?
├── PreserveDialogueGroups
├── PreserveParagraphs
├── PreserveDocumentBoundary
└── AllowSingleUnitBatch
```

Strategies:

```text
AUTOMATIC

LOW_LATENCY

BALANCED

MAXIMUM_CONTEXT

CUSTOM
```

---

# 46. Provider-Neutral Execution Request

Internal/public Adapter-boundary model:

```text
TranslationProviderRequest
├── RequestId
├── TranslationBatchId
├── TranslationUnits[]
├── ContextEntries[]
├── TerminologyConstraints[]
├── TranslationProfileRef
├── SourceLanguagePolicy
├── TargetLanguage
├── OutputSchemaRequirements
├── ProviderExecutionOptions
└── PrivacyConstraints
```

This model remains provider-neutral.

---

# 47. Provider Execution Options

```text
ProviderExecutionOptions
├── StructuredOutput
├── StreamingPreference
├── DeterministicPreference
├── MaximumOutputTokens?
├── CancellationTokenRef?
└── ExtensionOptions?
```

Runtime deadline remains external.

---

# 48. Provider Output Envelope

Normalized Adapter output:

```text
NormalizedTranslationProviderOutput
├── TranslationBatchId
├── ProviderRequestId?
├── UnitOutputs[]
├── ProviderExecutionMetadata
├── Usage?
├── Warnings[]
└── Metadata?
```

---

# 49. Provider Unit Output

```text
ProviderUnitOutput
├── TranslationUnitId
├── TargetText
├── ProviderConfidence?
├── ProviderWarnings[]
└── Metadata?
```

Unknown/missing Unit IDs are validation failures or degraded output according to policy.

---

# 50. Provider Execution Metadata

```text
ProviderExecutionMetadata
├── ProviderId
├── ProviderClass?
├── ModelIdentifier?
├── ProviderRequestId?
├── ExecutionRegion?
├── LocalExecution
├── RequestStartedAt?
├── ResponseReceivedAt?
├── Latency?
├── CachedByProvider?
├── StreamingUsed?
└── Usage?
```

No credentials or raw request/response.

---

# 51. Translation Usage

```text
TranslationUsage
├── InputCharacters?
├── OutputCharacters?
├── InputTokens?
├── OutputTokens?
├── TotalTokens?
├── ProviderReported
├── Estimated
├── MonetaryCost?
└── Currency?
```

Provider-reported and estimated values must be distinguishable.

---

# 52. Translated Unit

```text
TranslatedUnit
├── TranslatedUnitId
├── TranslationUnitId
├── SourceBlockRefs[]
├── SourceSequence
├── TargetLanguage
├── TranslatedText
├── Completion
├── Warnings[]
├── Confidence?
├── ProviderContribution?
├── AlignmentMetadata
└── Metadata?
```

---

# 53. Unit Completion

```text
COMPLETE

COMPLETE_WITH_WARNINGS

MISSING

FAILED
```

Do not include:

```text
CANCELLED

SUPERSEDED
```

as Translation-owned Unit semantic states.

Those are Runtime execution concerns.

---

# 54. Translated Text Rules

`TranslatedText` must:

1. be valid Unicode
2. correspond to its TranslationUnit
3. preserve required markers
4. contain no provider control syntax
5. contain no internal structured-output wrapper
6. obey locked terminology when required
7. be empty only when intentionally non-translatable or policy permits

---

# 55. Alignment Metadata

```text
TranslationAlignmentMetadata
├── TranslationUnitId
├── SourceBlockRefs[]
├── SourceRanges?
├── SourceSequenceRange
├── SplitGroupId?
├── MergeGroupId?
└── Metadata?
```

Alignment must survive batching and provider fallback.

---

# 56. Translation Confidence

```text
TranslationConfidence
├── Value?
├── Source
├── Scale?
└── Synthetic
```

Sources:

```text
PROVIDER

VALIDATOR

HEURISTIC

COMBINED
```

Confidence is optional.

---

# 57. Provider Contribution

```text
ProviderContribution
├── ProviderId
├── ModelIdentifier?
├── TranslationBatchId
├── ProviderRequestId?
└── ExecutionMetadataRef?
```

Runtime Attempt ID may appear in traceability metadata if useful, but it is not TranslationAttempt identity.

---

# 58. Translation Completeness

```text
TranslationCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

---

# 59. COMPLETE

All required TranslationUnits have acceptable TranslatedUnits.

---

# 60. PARTIAL

Some selected Units remain missing/failed but Candidate remains usable under policy.

Missing Units must be explicit.

---

# 61. EMPTY_VALID

Valid when no selected source content requires translation.

Examples:

* no translatable SourceBlocks
* all selected blocks excluded
* selected block policy = preserve without translation

---

# 62. UNKNOWN

Completeness cannot safely be determined.

No silent conversion.

---

# 63. Partial Result Policy

```text
PartialResultPolicy
├── Mode
├── MinimumCompletedUnits?
├── AllowFailedUnits
├── AllowMissingUnits
└── RequireIndependentAlignment
```

Modes:

```text
ATOMIC

ALLOW_PARTIAL

STREAM_VALIDATED_UNITS
```

This policy determines Candidate validity semantics.

It does not publish anything.

---

# 64. Streaming Boundary

Provider token stream:

```text
internal provider detail
```

Public/module-safe streaming prefers:

```text
validated completed TranslationUnit
```

Raw token stream must not become authoritative TranslationArtifact state.

---

# 65. Candidate Translation Artifact

```text
CandidateTranslationArtifact
├── CandidateArtifactId
├── ArtifactType
├── OwnerModule
├── ContractVersion
├── SourceDocumentArtifactRef
├── TranslationIntentId
├── TranslationProfileRef
├── TargetLanguage
├── TranslationUnits[]
├── TranslatedUnits[]
├── Completeness
├── MissingTranslationUnitIds[]
├── FailedTranslationUnitIds[]
├── Warnings[]
├── ProviderProvenance[]
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

---

# 66. Candidate Rules

Candidate is:

* non-authoritative
* not published
* immutable after validation
* Runtime-submitted
* source-traceable
* provider-neutral at public boundary
* cleaned if rejected

---

# 67. Candidate Validation

Validate:

* Candidate identity
* Artifact type
* owner module
* SourceDocument reference
* Translation Intent identity
* target language
* Translation Unit uniqueness
* Translation Unit → SourceBlock alignment
* TranslatedUnit uniqueness
* TranslatedUnit → TranslationUnit mapping
* Completeness
* missing/failed-unit consistency
* terminology constraints
* privacy
* provider provenance
* compatibility metadata
* traceability metadata
* no credentials
* no Runtime terminal state

---

# 68. Candidate Is Not Published Artifact

```text
Candidate VALID
    ≠
Runtime accepted
```

and:

```text
Runtime accepted
    ≠
active reading-session variant
```

These are separate concerns.

---

# 69. Translation Attempt Output

```text
TranslationAttemptOutput
├── CandidateArtifact?
├── ModuleWarnings[]
├── ModuleError?
├── RetryHint?
├── DiagnosticsRef?
└── CompletionMetadata
```

---

# 70. Completion Metadata

```text
TranslationCompletionMetadata
├── StartedAt
├── CompletedAt
├── OperationPhase
├── ProviderExecutionSummary?
├── CancellationObserved
└── Metadata?
```

This metadata does not define Runtime terminal status.

---

# 71. Published Translation Artifact

After Runtime acceptance and Artifact Store transfer:

```text
TranslationArtifact
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── SourceDocumentArtifactRef
├── TranslationIntentId
├── TranslationProfileRef
├── SourceLanguage?
├── TargetLanguage
├── TranslationUnits[]
├── TranslatedUnits[]
├── Completeness
├── Warnings[]
├── VariantMetadata
├── ProviderProvenance[]
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

---

# 72. Artifact Type

Recommended:

```text
TRANSLATION_ARTIFACT
```

---

# 73. Translation Variant

A Translation Artifact may represent one immutable variant.

```text
TranslationVariantMetadata
├── TranslationVariantId
├── VariantType
├── ParentVariantRef?
├── CreatedBy
├── CreatedAt
└── Metadata?
```

---

# 74. Variant Types

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

# 75. Variant Immutability

Published variant cannot be modified in place.

```text
Variant A
    ↓ correction/retranslation
Variant B
```

Parent reference may preserve lineage.

---

# 76. Variant Activation Boundary

Translation owns variant semantics.

It does **not automatically own**:

```text
which variant is currently active
for a Reading Session
```

Reading Session/User Preference/Application orchestration owns active selection unless architecture explicitly delegates it.

---

# 77. User Correction Contract

Recommended future/adjacent semantic command:

```text
CreateTranslationCorrection
├── BaseTranslationArtifactRef
├── Corrections[]
├── TranslationProfileRef?
├── ProposeKnowledgeUpdate
└── Metadata
```

Correction produces a new Candidate/Artifact variant.

It does not mutate old Artifact.

---

# 78. Translation Correction

```text
TranslationCorrection
├── TranslationUnitId
├── SourceBlockRefs[]
├── CorrectedText
├── CorrectionReason?
└── Notes?
```

Knowledge updates remain explicit and external.

---

# 79. Warning Contract

```text
TranslationWarning
├── WarningCode
├── Severity
├── OperationPhase
├── TranslationUnitIds[]
├── SourceBlockRefs[]
├── TranslationBatchId?
├── ProviderId?
├── MessageKey
├── Metadata?
└── RecordedAt
```

---

# 80. Warning Severity

```text
INFORMATION

NOTICE

DEGRADED
```

Fatal conditions are ModuleErrors.

---

# 81. Recommended Warning Codes

```text
MISSING_OPTIONAL_CONTEXT

LOW_TRANSLATION_CONFIDENCE

AMBIGUOUS_MEANING

TERMINOLOGY_CONFLICT

SOURCE_INCOMPLETE

SOURCE_LANGUAGE_UNCERTAIN

UNTRANSLATED_FRAGMENT

OUTPUT_LENGTH_ANOMALY

PROVIDER_FALLBACK_USED

PARTIAL_TRANSLATION

SOUND_EFFECT_PRESERVED

MIXED_LANGUAGE_CONTENT

PRONOUN_AMBIGUITY

AMBIGUOUS_SPEAKER_RELATIONSHIP
```

Cache reuse is not inherently a translation warning.

---

# 82. Module Error

```text
TranslationModuleError
├── ContractVersion
├── ErrorCode
├── Category
├── Severity
├── OperationPhase
├── MessageKey
├── RetryHint?
├── ProviderErrorRef?
├── AffectedTranslationUnitIds[]
├── AffectedBatchId?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

Detailed taxonomy belongs in `ERRORS.md`.

---

# 83. Retry Hint

```text
TranslationRetryHint
├── Retryability
├── SuggestedStrategies[]
├── ProviderFallbackAllowed
├── ReasonCode
└── Metadata?
```

Retryability:

```text
RETRYABLE

CONDITIONALLY_RETRYABLE

NON_RETRYABLE
```

---

# 84. Retry Strategies

```text
SAME_PROVIDER

ALTERNATIVE_PROVIDER

SMALLER_BATCH

REDUCE_CONTEXT

ALTERNATIVE_CONTEXT_POLICY

ALTERNATIVE_TRANSLATION_PROFILE

RESOURCE_WAIT

NO_RETRY
```

Runtime decides whether a new Attempt is created.

---

# 85. Provider Error Reference

```text
TranslationProviderErrorRef
├── ProviderId
├── ProviderErrorCode
├── ProviderCategory
├── Retryability
├── ProviderRequestId?
├── SanitizedMessageKey
├── DiagnosticsRef?
└── OccurredAt
```

No raw provider response or credential.

---

# 86. Operation Phase

Diagnostic enum:

```text
VALIDATING

PLANNING

BUILDING_UNITS

BUILDING_CONTEXT

RESOLVING_TERMINOLOGY

BUILDING_BATCHES

EXECUTING_PROVIDER

NORMALIZING_OUTPUT

VALIDATING_OUTPUT

ASSEMBLING_CANDIDATE

VALIDATING_CANDIDATE

FINALIZING
```

These are not Runtime Attempt states.

---

# 87. Provider Output Validation

Provider output may be rejected/degraded when:

* expected TranslationUnit ID missing
* unexpected ID returned
* duplicate output exists
* structured output malformed
* translated text unexpectedly empty
* provider control syntax leaked
* locked terminology violated
* source text remains untranslated beyond policy
* output length extreme
* target language implausible
* output belongs to wrong Batch/request
* source alignment cannot be preserved

---

# 88. Result Assembly Rules

Candidate assembly must:

1. order results by source/TranslationUnit sequence
2. preserve TranslationUnit identity
3. preserve SourceBlock refs
4. explicitly report missing Units
5. explicitly report failed Units
6. preserve warning associations
7. exclude context-only entries from output
8. preserve provider provenance
9. not silently drop selected Units

---

# 89. Provider Provenance

```text
TranslationProviderProvenance
├── ProviderId
├── ProviderClass?
├── ModelIdentifier?
├── AdapterVersion?
├── ExecutionLocation
├── TranslationBatchIds[]
├── UsageSummary?
└── Metadata?
```

Credentials forbidden.

---

# 90. Traceability Metadata

```text
TranslationTraceabilityMetadata
├── SourceDocumentArtifactRef
├── TranslationIntentId
├── TranslationPlanId
├── TranslationProfileRef
├── KnowledgeSnapshotRef?
├── ContextSnapshotRef?
├── ConfigurationSnapshotId
├── ProviderPolicyVersion?
└── TraceId?
```

---

# 91. Compatibility Metadata

```text
TranslationCompatibilityMetadata
├── SourceDocumentContentIdentity
├── SourceDocumentContractVersion
├── TranslationContractVersion
├── TargetLanguage
├── TranslationProfileVersion
├── TranslationStrategyVersion
├── SourceSelectionIdentity
├── KnowledgeSnapshotIdentity?
├── ContextIdentity?
├── TerminologyPolicyVersion?
├── ProviderPolicyVersion?
├── PrivacyPartition
└── SemanticDependencies[]
```

---

# 92. Semantic Compatibility

Two Translation Artifacts may be compatible only when relevant semantic dependencies match or are explicitly compatible.

Important dependencies:

* source semantic identity
* selected source scope
* target language
* Translation Profile
* terminology/Knowledge
* context
* source alignment
* contract version
* privacy partition

---

# 93. Provider Compatibility

Provider identity itself does not always invalidate semantic reuse.

Policy may allow:

```text
provider-independent reuse
```

when result semantics are compatible.

If intent explicitly requires Provider A, Provider identity becomes semantic dependency.

---

# 94. Compatibility vs Cache

Translation defines:

```text
semantic compatibility
```

Runtime Cache Policy decides:

```text
whether reuse occurs
```

Artifact Store decides:

```text
retention
```

Storage decides:

```text
durable persistence
```

---

# 95. Translation Statistics

Optional:

```text
TranslationStatistics
├── SelectedSourceBlockCount
├── TranslationUnitCount
├── TranslatedUnitCount
├── MissingUnitCount
├── FailedUnitCount
├── SourceCharacterCount
├── TranslatedCharacterCount
├── BatchCount
├── ProviderFallbackCount
├── ExecutionDuration?
└── Usage?
```

Do not include Runtime queue duration as Translation-owned metric.

---

# 96. Cancellation Contract

Translation consumes Runtime:

```text
CancellationContextRef
```

On cancellation observation:

* stop new batches
* request provider cancellation if supported
* stop optional processing
* avoid unauthorized Candidate submission
* cleanup resources
* return control to Runtime

Translation does not return canonical `CANCELLED` Artifact state.

---

# 97. Deadline Contract

Deadline belongs to Runtime ExecutionContext.

Translation may:

* observe remaining budget
* stop optional context expansion
* reduce new work when policy permits
* return partial Candidate when valid
* cleanup

Runtime owns terminal deadline outcome.

---

# 98. Stale Result Contract

Translation may perform defensive semantic consistency checks.

Final authority is Runtime.

```text
Candidate valid
    ↓
Runtime detects stale Revision
    ↓
Candidate rejected
```

This is not a TranslationModuleError.

---

# 99. No TranslationRevision Publication State

Legacy monotonic:

```text
TranslationRevision
```

used as mutable publication/current-result state is removed from core Translation contract.

Reasons:

* Artifact is immutable
* Runtime controls authority
* Reading Session/application controls active selection
* new semantic result = new Artifact/Variant

If UI needs a view revision, that revision belongs to its owning projection/session concern.

---

# 100. No Translation Job Query Contract

Removed core queries:

```text
GetTranslationJob

GetTranslationProgress

Get active TranslationAttempt
```

because Translation does not own a Job/Attempt lifecycle.

Runtime provides execution-state queries if needed.

---

# 101. Recommended Translation Artifact Queries

Artifact/read-model layer may expose:

```text
GetTranslationArtifact

ListTranslationVariants

GetTranslationVariant

FindCompatibleTranslation
```

Ownership of concrete query API should follow Artifact Store / application read-model architecture.

---

# 102. Retranslation Semantics

Retranslation means a new semantic Translation Intent.

Examples:

* change target language
* change Translation Profile
* change provider hard constraint
* change Knowledge Snapshot
* change Context Snapshot
* request literal/natural variant

This results in new Runtime work and eventually a new Candidate/Artifact.

No `RequestRetranslation` job command is required inside Translation core.

---

# 103. Invalidation Semantics

Existing immutable Artifact is not mutated because a newer result exists.

Invalidation/eligibility may be represented by:

* Runtime Cache Policy
* Artifact metadata
* Reading Session selection
* policy projection
* external administrative state

Translation should not maintain mutable `INVALIDATED` lifecycle inside immutable Artifact.

---

# 104. Privacy Context

```text
TranslationPrivacyContext
├── PrivacyMode
├── PrivacyPartition
├── RemoteExecutionAllowed
├── ProtectedDiagnosticsAllowed
├── PersistenceAllowed
└── ExportAllowed?
```

---

# 105. Privacy Modes

Recommended:

```text
STANDARD

LOCAL_ONLY

EPHEMERAL
```

`LOCAL_ONLY` forbids remote provider execution.

---

# 106. Sensitive Fields

Public normal metadata must never contain:

* provider API keys
* bearer tokens
* auth headers
* raw provider prompts
* provider secrets
* unrelated session data
* full raw provider responses

---

# 107. Source / Translation Content Handling

Source and translated text may appear in necessary Artifact/Provider request contracts.

They should not appear by default in:

* logs
* metrics
* event diagnostics
* error metadata
* provider-selection metadata

---

# 108. Untrusted Source Rule

```text
Source Content
Context Content
Glossary Notes
        ≠
Trusted Translation Instructions
```

Provider Adapter must keep:

```text
System Policy

Translation Profile

Terminology Policy

Context

Source Data
```

separated.

---

# 109. Prompt-Injection Safety

Source content must not be able to:

* override system translation policy
* request credentials
* activate application tools
* read unrelated session state
* change privacy policy
* alter Provider Policy
* reinterpret context as commands

---

# 110. Producer Obligations

Translation implementation must:

1. validate Attempt Input
2. resolve immutable SourceDocument Artifact
3. preserve SourceDocument immutability
4. create explicit Translation Intent
5. build deterministic semantic Plan where possible
6. create stable Translation Units
7. preserve source alignment
8. build provider-neutral Batches
9. enforce terminology constraints
10. enforce Privacy Context
11. normalize provider output
12. validate output
13. explicitly report missing Units
14. explicitly report partial output
15. assemble immutable Candidate
16. return RetryHint only
17. never schedule retry itself
18. never own terminal Runtime state
19. never publish authoritative Artifact directly
20. never leak provider credentials

---

# 111. Runtime Obligations

Runtime must:

1. create WorkItem/Attempt
2. provide Revision/authority identity
3. provide Execution Context
4. provide Cancellation Context
5. own deadline enforcement
6. own queue/Scheduler
7. own retry budget/backoff
8. own stale-result validation
9. own Attempt terminal state
10. accept/reject Candidate
11. coordinate cleanup
12. invoke Artifact Store on acceptance

---

# 112. Provider Management Obligations

Provider Management must:

* maintain provider registry
* manage lifecycle/health
* protect credentials
* expose capabilities
* expose availability
* provide safe execution handles
* manage reusable Provider resources

Translation does not duplicate these.

---

# 113. Artifact Store Obligations

Artifact Store must:

* receive accepted Candidate transfer
* assign ArtifactId
* publish atomically
* expose immutable Artifact lookup
* manage shared lifecycle
* reject invalid duplicate publication
* coordinate retention/leasing

---

# 114. Consumer Obligations

Consumers must:

1. treat TranslationArtifact immutable
2. preserve SourceBlock/TranslationUnit alignment
3. handle PARTIAL
4. handle EMPTY_VALID
5. not assume provider confidence exists
6. not use Batch IDs as alignment IDs
7. not depend on provider-native metadata
8. not infer Runtime success from Candidate validation
9. not mutate translated text in Artifact
10. create new correction/variant for edits

---

# 115. Presentation Obligations

Presentation may consume:

* TranslatedUnits
* SourceBlockRefs
* Source ordering
* target language
* warnings
* completeness
* upstream geometry lineage through SourceBlock

Presentation must not require:

* provider prompt
* Provider request
* Runtime retry history
* Translation Batch internals

---

# 116. Knowledge Compatibility

Translation consumes Knowledge by stable snapshot/reference.

It must not depend on Knowledge persistence tables/schema.

Knowledge updates are never silently committed from translation execution.

---

# 117. Idempotency

Semantic idempotency should derive from:

```text
SourceDocument semantic identity

SourceSelection

TargetLanguage

TranslationProfile

Knowledge Snapshot

Context Snapshot

Provider hard constraints

Translation Strategy version

Privacy Partition
```

Equivalent Runtime retries should not create semantically divergent Intent identities solely because `AttemptId` differs.

---

# 118. Contract Evolution

Backward-compatible:

* optional fields
* new warning codes
* new provider capability tags
* new Translation Profile
* new optional provenance
* new optional Variant type
* additive metadata

Breaking:

* changing TranslationUnit alignment semantics
* changing SourceDocument boundary
* removing required source lineage
* changing Candidate authority semantics
* changing Provider neutrality
* changing privacy guarantees
* moving Retry/Cancellation authority into Translation
* changing TranslationArtifact immutability

Requires major version.

---

# 119. Unknown Values

Consumers should:

* preserve unknown additive enum values when possible
* safely fall back
* not fabricate meaning
* reject unsupported major contract version

---

# 120. Example Attempt Input

```json
{
  "runtime_context": {
    "contract_version": "1.0.0",
    "revision_id": "revision_104",
    "work_item_id": "work_translation_104",
    "attempt_id": "attempt_01",
    "configuration_snapshot_id": "config_42"
  },
  "source_document_artifact_ref": {
    "artifact_id": "source_document_artifact_104",
    "artifact_type": "SOURCE_DOCUMENT_ARTIFACT",
    "contract_version": "1.0.0"
  },
  "source_selection": {
    "mode": "ALL_TRANSLATABLE",
    "include_excluded_blocks": false
  },
  "translation_intent": {
    "translation_intent_id": "intent_104_vi",
    "source_language_policy": {
      "mode": "UPSTREAM_HINT"
    },
    "target_language": {
      "language_tag": "vi"
    },
    "translation_profile_ref": {
      "profile_id": "COMIC_NATURAL",
      "profile_version": "1"
    },
    "partial_result_policy": {
      "mode": "ALLOW_PARTIAL"
    }
  }
}
```

---

# 121. Example Translation Unit

```json
{
  "translation_unit_id": "unit_01",
  "source_block_refs": [
    "block_01"
  ],
  "source_text_view": {
    "text": "你好！"
  },
  "source_sequence": 0,
  "target_language": "vi",
  "structural_type": "DIALOGUE",
  "translation_profile_ref": {
    "profile_id": "COMIC_NATURAL",
    "profile_version": "1"
  }
}
```

---

# 122. Example Translation Batch

```json
{
  "translation_batch_id": "batch_01",
  "sequence": 0,
  "translation_unit_ids": [
    "unit_01",
    "unit_02"
  ],
  "context_entry_refs": [
    "ctx_previous_01"
  ],
  "provider_requirements": {
    "target_language": "vi",
    "structured_output_required": true
  }
}
```

---

# 123. Example Translated Unit

```json
{
  "translated_unit_id": "translated_unit_01",
  "translation_unit_id": "unit_01",
  "source_block_refs": [
    "block_01"
  ],
  "source_sequence": 0,
  "target_language": "vi",
  "translated_text": "Xin chào!",
  "completion": "COMPLETE",
  "warnings": []
}
```

---

# 124. Example Candidate

```json
{
  "candidate_artifact_id": "candidate_translation_104",
  "artifact_type": "TRANSLATION_ARTIFACT",
  "owner_module": "translation",
  "contract_version": "1.0.0",
  "source_document_artifact_ref": {
    "artifact_id": "source_document_artifact_104"
  },
  "translation_intent_id": "intent_104_vi",
  "translation_profile_ref": {
    "profile_id": "COMIC_NATURAL",
    "profile_version": "1"
  },
  "target_language": "vi",
  "completeness": "COMPLETE",
  "missing_translation_unit_ids": [],
  "failed_translation_unit_ids": [],
  "warnings": []
}
```

---

# 125. Contract Testing — Input

Test:

* valid SourceDocument Artifact
* missing SourceDocument Artifact
* incompatible contract major
* invalid source selection
* missing target language
* invalid Translation Profile
* invalid Provider Policy
* invalid Privacy Context
* unavailable required Knowledge Snapshot
* impossible provider constraints

---

# 126. Contract Testing — Translation Units

Test:

* one SourceBlock → one Unit
* multiple SourceBlocks → one Unit
* split SourceBlock → multiple Units
* duplicate TranslationUnitId
* invalid SourceBlockRef
* source-order preservation
* context-only data not treated as target Unit

---

# 127. Contract Testing — Batches

Test:

* one Unit
* multiple Units
* duplicate Unit ID
* incompatible target languages
* provider size limit exceeded
* batch order differing from source alignment
* context-only entries
* fallback provider compatibility

---

# 128. Contract Testing — Provider Output

Test:

* valid structured output
* missing Unit
* duplicate Unit
* unknown Unit
* malformed provider output
* locked terminology violation
* source leakage
* empty output
* control syntax leakage
* target-language mismatch

---

# 129. Contract Testing — Candidate

Test:

* COMPLETE Candidate
* PARTIAL Candidate
* EMPTY_VALID Candidate
* missing Unit IDs explicit
* failed Unit IDs explicit
* invalid SourceBlock alignment
* missing CompatibilityMetadata
* missing TraceabilityMetadata
* credentials leakage
* Runtime-state leakage
* immutable after validation

---

# 130. Contract Testing — Runtime Boundary

Test:

```text
Candidate VALID
    → Runtime accepts
```

```text
Candidate VALID
    → Runtime rejects stale
```

```text
Cancellation observed
    → module stops
    → Runtime decides outcome
```

```text
RetryHint returned
    → Runtime creates new Attempt
```

---

# 131. Contract Testing — Variant

Test:

* immutable variant
* corrected variant creates new Artifact
* literal/natural variants coexist
* different provider variant
* parent variant lineage
* old Artifact remains unchanged
* active selection external to core Translation execution

---

# 132. Contract Testing — Privacy

Verify:

* credentials absent
* source text absent from normal errors
* translated text absent from normal events/logs
* remote provider blocked in LOCAL_ONLY
* prompt-injection separation maintained
* provider raw response does not cross boundary

---

# 133. Core Contract Invariants

1. Translation starts from SourceDocumentArtifact.

2. Translation does not consume raw OCR as canonical input.

3. Translation does not redefine SourceBlock.

4. Translation owns TranslationUnit.

5. TranslationUnit is distinct from SourceBlock.

6. TranslationUnit always preserves SourceBlock lineage.

7. Translation owns TranslationBatch semantics.

8. Batch is not alignment identity.

9. Translation does not own TranslationJob lifecycle.

10. Translation does not own TranslationAttempt lifecycle.

11. Runtime WorkItem owns logical execution work.

12. Runtime Attempt owns execution attempt identity.

13. Runtime owns retry execution.

14. Runtime owns cancellation authority.

15. Runtime owns deadline terminal behavior.

16. Runtime owns stale-result authority.

17. Translation creates Candidate only.

18. Candidate validation does not imply Runtime acceptance.

19. Artifact Store owns published Artifact lifecycle.

20. TranslationArtifact is immutable.

21. Translation variants are immutable.

22. Correction creates new variant.

23. Every TranslatedUnit maps to a TranslationUnit.

24. Every TranslationUnit maps to SourceBlock evidence.

25. Missing TranslationUnits are explicit.

26. Failed TranslationUnits are explicit.

27. Context-only entries never silently create outputs.

28. Translation follows SourceDocument order.

29. Translation never infers OCR Reading Order.

30. Provider-specific models remain inside adapters.

31. Provider credentials never cross public boundary.

32. Provider lifecycle belongs to Provider Management.

33. Provider fallback must respect Privacy Policy.

34. LOCAL_ONLY forbids remote providers.

35. Source/context/glossary text is untrusted data.

36. Source text cannot override system translation policy.

37. Locked terminology is explicit.

38. Mixed-language source is valid.

39. Provider confidence is optional.

40. Partial translation is explicit.

41. EMPTY_VALID is valid.

42. Translation defines semantic compatibility.

43. Runtime Cache Policy owns reuse decision.

44. Artifact Store owns retention.

45. Storage owns durable persistence.

46. Translation does not maintain monotonic publication revision state.

47. Reading Session/application state may choose active variant.

48. Translation does not mutate Knowledge silently.

49. Translation does not visually fit translated text.

50. Public contracts remain provider-neutral.

---

# 134. MVP Contract Surface

Required:

```text
TranslationAttemptInput

SourceDocumentArtifactRef

TranslationIntent

SourceSelection

TranslationProfileRef

ProviderPolicy

TerminologyPolicy

ContextPolicy

TranslationPlan

TranslationUnit

TranslationBatch

NormalizedTranslationProviderOutput

TranslatedUnit

CandidateTranslationArtifact

TranslationArtifact

TranslationWarning

TranslationModuleError

TranslationRetryHint
```

---

# 135. MVP Profiles

```text
COMIC_NATURAL

NOVEL_NATURAL

GENERAL_NATURAL

LITERAL
```

---

# 136. MVP Language Priority

Initial implementations may prioritize:

```text
zh-Hans → vi

zh-Hant → vi

en → vi
```

but this is implementation priority, not hardcoded contract limitation.

---

# 137. MVP Provider Features

Required minimum:

* provider-neutral Adapter
* batch execution
* stable Unit IDs
* structured/aligned output
* provider provenance
* retry/fallback hint support
* local/remote policy
* usage metadata where available

---

# 138. Deferred Extensions

Potential:

```text
AdvancedTranslationMemoryRef

MultiProviderComparison

EnsembleTranslationResult

TranslationQualityReport

IncrementalTranslationPatch

CrossChapterContextSnapshot

SpeakerIdentityRef

UserStyleProfileRef

TranslationEvaluationArtifact
```

Only add when concrete use cases require them.

---

# 139. Removed Legacy Contracts

Removed/re-owned:

```text
PreparedDocument
    → SourceDocumentArtifact

PreparedSegment
    → SourceBlock upstream
      + TranslationUnit downstream

TranslationJob
    → Runtime WorkItem

TranslationAttempt
    → Runtime Attempt

TranslatedSegment
    → TranslatedUnit

TranslationResultSnapshot
    → TranslationArtifact

TranslationRevision
    → removed from core publication lifecycle

CancelTranslationCommand
    → Runtime cancellation

RetryTranslationCommand
    → Runtime retry

GetTranslationJob
    → Runtime execution query

GetTranslationProgress
    → Runtime/telemetry query

InvalidateTranslation job state
    → Artifact/cache/session policy

SelectTranslationVariant
    → Reading Session / application selection concern
```

---

# 140. Related Documents

```text
02-modules/translation/README.md
02-modules/translation/MODULE.md
02-modules/translation/STATES.md
02-modules/translation/EVENTS.md
02-modules/translation/ERRORS.md

02-modules/text-processing/README.md
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md

02-modules/knowledge/
02-modules/provider-management/
02-modules/presentation/
02-modules/reading-session/

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

03-infrastructure/artifact-store/
03-infrastructure/resource-manager/
```

---

# 141. Summary

Translation public contract is centered on:

```text
SourceDocumentArtifact
    = stable source input

TranslationIntent
    = requested semantic translation

TranslationUnit
    = translation alignment unit

TranslationBatch
    = provider-execution planning unit

TranslatedUnit
    = aligned target-language unit

CandidateTranslationArtifact
    = module-valid non-authoritative result

TranslationArtifact
    = accepted immutable published result
```

Primary flow:

```text
SourceDocumentArtifact
        ↓
TranslationIntent
        ↓
TranslationPlan
        ↓
TranslationUnit[]
        ↓
TranslationBatch[]
        ↓
Provider Adapter
        ↓
TranslatedUnit[]
        ↓
CandidateTranslationArtifact
        ↓
Runtime
        ↓
TranslationArtifact
```

Ownership:

```text
Text Processing
    owns SourceDocument / SourceBlock.

Translation
    owns Translation Intent,
    Translation Plan,
    Translation Unit,
    Translation Batch
    and translated semantics.

Runtime
    owns WorkItem,
    Attempt,
    retry,
    cancellation,
    deadline
    and authority.

Provider Management
    owns Provider lifecycle,
    capabilities
    and credentials.

Artifact Store
    owns accepted Artifact lifecycle.

Knowledge
    owns persistent terminology/knowledge.

Reading Session / Application state
    owns active-reading-context selection.

Presentation
    owns visual layout.
```

The core rule is:

```text
SourceBlock stabilizes source structure.

TranslationUnit begins translation semantics.

Runtime decides whether execution still matters.

Artifact Store publishes the accepted immutable result.
```
