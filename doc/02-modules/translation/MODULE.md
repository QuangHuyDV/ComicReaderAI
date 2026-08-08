# Translation Module Specification

> **Project:** CRAI
> **Module:** Translation
> **Path:** `02-modules/translation/MODULE.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Primary Input:** `SourceDocumentArtifact`
> **Primary Output:** `TranslationArtifact`

---

# 1. Module Definition

Translation là Core Business Processing Module chịu trách nhiệm chuyển stable source content thành target-language content trong khi bảo toàn:

* semantic meaning
* source alignment
* document structure
* terminology consistency
* contextual coherence
* source traceability
* provider independence
* revision safety
* privacy policy

Canonical transformation:

```text id="a0z3v4"
SourceDocumentArtifact
        ↓
Translation Planning
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
Runtime Authority Validation
        ↓
TranslationArtifact
```

Translation không chịu trách nhiệm:

* source acquisition
* OCR
* source normalization
* source reconstruction
* visual layout
* Runtime execution lifecycle
* Artifact publication lifecycle

---

# 2. Architectural Position

```text id="y28sn4"
Source
    ↓
Recognition         image-originated path
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
```

Text-originated content có thể đi qua Text Processing mà không cần Recognition.

Translation không consume raw OCR output trực tiếp.

---

# 3. Module Identity

```text id="r20fjd"
Module ID:
    translation

Module Type:
    Core Business Processing Module

Primary Domain:
    Source-to-Target Language Transformation

Execution Model:
    Runtime WorkItem / Attempt

Primary Input:
    SourceDocumentArtifactRef

Primary Candidate Output:
    CandidateTranslationArtifact

Published Output:
    TranslationArtifact

Execution Authority:
    Runtime

Provider Lifecycle Owner:
    Provider Management
```

---

# 4. Primary Goal

Translation phải tạo target-language output đủ tốt cho continuous reading.

Primary optimization target:

```text id="6zvnt6"
continuous reading quality
```

Điều này bao gồm:

* coherent paragraph flow
* natural dialogue
* consistent names
* consistent terminology
* suitable pronouns
* reasonable honorifics
* preserved structural alignment
* bounded visible latency

Translation không tối ưu chỉ cho từng câu độc lập.

---

# 5. Core Responsibilities

Translation sở hữu:

* Translation Intent semantics
* Translation Profile
* Translation Plan
* Translation Unit construction
* Translation Batch construction
* context construction
* terminology constraint construction
* provider capability requirements
* provider-neutral execution requests
* provider output normalization
* translated-unit alignment
* translation-specific validation
* Translation Artifact construction
* translation warnings/errors
* semantic compatibility
* partial-result semantics
* variant semantics
* module diagnostics
* retry/fallback recommendations

---

# 6. Explicit Non-Responsibilities

Translation không sở hữu:

* content acquisition
* browser integration
* screenshot capture
* OCR
* text-region detection
* OCR Reading Order
* source normalization
* source reconstruction
* SourceDocument construction
* WorkItem lifecycle
* Attempt lifecycle
* Scheduler
* Work Queue
* Runtime retry execution
* Runtime cancellation authority
* stale-result authority
* Artifact publication
* Artifact retention
* Provider registry
* Provider health
* Provider credential storage
* glossary persistence
* Reading Session lifecycle
* Presentation layout
* overlay rendering
* font fitting
* UI state

---

# 7. Upstream Contract

Translation consumes:

```text id="nlja2c"
SourceDocumentArtifact
```

produced by Text Processing.

It may contain/reference:

```text id="xxh25k"
SourceDocument
SourceBlock[]
SourceBlockSequence
ExcludedBlocks
LanguageHints
TraceabilityMetadata
CompatibilityMetadata
```

Translation does not consume legacy:

```text id="35h7vu"
PreparedDocument
PreparedSegment
```

as canonical public inputs.

---

# 8. SourceDocument Boundary

Text Processing answers:

```text id="1o5yjz"
How should source content
be represented structurally?
```

Translation answers:

```text id="g1bgus"
How should that stable source structure
be translated into the target language?
```

Translation must not mutate SourceDocument.

---

# 9. Translation Unit Ownership

Translation owns:

```text id="cv78qs"
TranslationUnit
```

A Translation Unit is the smallest semantic alignment unit that Translation chooses to manage as one logical translation result.

It may derive from:

```text id="ny6puv"
one SourceBlock
```

or:

```text id="rwr3y7"
multiple SourceBlocks
```

depending on:

* target language
* source structure
* paragraph continuity
* dialogue relationships
* context strategy
* provider limits
* Translation Profile
* batching policy

---

# 10. Translation Unit Is Not SourceBlock

```text id="9q3vf5"
SourceBlock
    = source reconstruction unit

TranslationUnit
    = translation semantic/alignment unit
```

Possible mapping:

```text id="qdg01x"
SourceBlock A
    ↓
TranslationUnit 1
```

or:

```text id="p5o78r"
SourceBlock A
SourceBlock B
SourceBlock C
    ↓
TranslationUnit 1
```

or:

```text id="27fqpp"
SourceBlock A
    ↓
TranslationUnit 1
TranslationUnit 2
```

Splitting one SourceBlock should be used only when required by Translation semantics/provider constraints while preserving traceability.

---

# 11. Translation Unit Contract

Conceptually:

```text id="gy6yan"
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
├── TranslationProfileRef
├── AlignmentMetadata
└── Metadata
```

Exact public schema belongs in `CONTRACT.md`.

---

# 12. Translation Intent

Translation Intent describes what translation the caller wants.

Conceptually:

```text id="12721h"
TranslationIntent
├── SourceDocumentArtifactRef
├── TargetLanguage
├── TranslationProfileRef
├── KnowledgeSnapshotRef?
├── ContextPolicyRef?
├── ProviderPolicyRef?
├── PartialResultPolicy
├── PrivacyContextRef
└── Metadata?
```

Translation Intent is semantic input.

It is not Runtime WorkItem state.

---

# 13. Translation Profile

Initial profiles may include:

```text id="70us1h"
NOVEL_NATURAL

COMIC_NATURAL

GENERAL_NATURAL

LITERAL

CUSTOM
```

Profile may influence:

* naturalness
* literalness
* dialogue tone
* honorific handling
* name preservation
* explanatory behavior
* output compactness
* punctuation policy
* sound-effect behavior
* pronoun policy

Profile must not expose provider-specific prompts as public contract.

---

# 14. Translation Plan

Before execution Translation creates immutable:

```text id="h7bgnq"
TranslationPlan
```

Conceptually:

```text id="mlns4x"
TranslationPlan
├── PlanId
├── SourceDocumentArtifactRef
├── TranslationIntent
├── TranslationProfile
├── TranslationUnits[]
├── TranslationBatches[]
├── ContextPlan
├── TerminologyPlan
├── ProviderRequirements
├── ValidationPolicy
├── PartialResultPolicy
├── CompatibilityPolicy
├── ConfigurationSnapshotId
└── PrivacyConstraints
```

Plan becomes immutable within one Runtime Attempt.

---

# 15. Main Processing Flow

```text id="ocmi60"
SourceDocumentArtifact
        ↓
Validate Module Input
        ↓
Resolve Translation Intent
        ↓
Build Translation Plan
        ↓
Build Translation Units
        ↓
Resolve Context
        ↓
Resolve Terminology Constraints
        ↓
Build Translation Batches
        ↓
Resolve Provider Requirements
        ↓
Execute Provider Requests
        ↓
Normalize Provider Output
        ↓
Validate Translated Units
        ↓
Assemble Translation Artifact Candidate
        ↓
Submit Candidate to Runtime
```

---

# 16. Translation Batch

Translation owns:

```text id="fsglyf"
TranslationBatch
```

Batch is the primary provider-execution planning unit.

```text id="xxrg6n"
TranslationUnit[]
        ↓
TranslationBatch
        ↓
Provider-neutral Execution Request
```

Batching exists to balance:

* quality
* context
* provider limits
* cost
* latency
* failure isolation
* source alignment

---

# 17. Why Batch Translation Exists

Translating every SourceBlock/Unit independently can cause:

* lost dialogue context
* inconsistent names
* inconsistent pronouns
* excessive provider calls
* poor paragraph flow
* higher cost
* network overhead
* inconsistent terminology

Therefore:

```text id="ynmmyz"
multiple TranslationUnits
        ↓
one TranslationBatch
        ↓
multiple aligned TranslatedUnits
```

Batch support is architectural, even if MVP starts with small batches.

---

# 18. Batch Boundary Inputs

Batch planning may consider:

* TranslationUnit sequence
* paragraph boundaries
* dialogue groups
* comic page boundaries
* chapter boundaries
* provider token limits
* provider character limits
* provider context window
* latency budget
* cost budget
* Translation Profile
* context dependencies
* terminology dependencies
* privacy constraints

---

# 19. Batch Isolation

Batches should be independently retryable where possible.

Failure of Batch A should not automatically invalidate Batch B unless:

* cross-batch consistency contract requires it
* provider result is document-atomic
* policy requires atomic completion
* source alignment would become unsafe

---

# 20. Context Model

Translation quality depends on supporting context.

Context may include:

* preceding SourceBlocks
* following SourceBlocks
* previous Translation Units
* previous accepted translations
* paragraph context
* dialogue-group context
* page context
* chapter context
* chapter summary
* character names
* relationship hints
* glossary terms
* terminology preferences
* Reading Session-provided context

---

# 21. Context Roles

Translation distinguishes:

```text id="ndwv37"
Translatable Content
```

from:

```text id="eh096q"
Context-Only Content
```

Example:

```text id="h1za7j"
Previous translated dialogue
    → Context Only

Current source dialogue
    → Translatable
```

Context-only input must not accidentally produce new translated output.

---

# 22. Context Sources

Context must enter through explicit snapshots/references.

Examples:

```text id="q6ft17"
SourceDocumentRef

AdjacentSourceDocumentRef

PreviousTranslationArtifactRef

KnowledgeSnapshotRef

ChapterContextSnapshotRef

ReadingContextSnapshotRef
```

Translation must not silently query mutable global application state during provider execution.

---

# 23. Context Revision Safety

Context can change translation output.

Therefore semantic compatibility may depend on:

* SourceDocument identity
* Context Snapshot identity
* Knowledge Snapshot revision
* Translation Profile version
* target language
* Provider Policy version
* Translation contract version

A context change does not automatically require retranslation.

Runtime/cache policy decides when new work is needed.

---

# 24. Knowledge Integration

Translation consumes Knowledge references/snapshots containing:

* names
* aliases
* relationships
* preferred transliterations
* locations
* terminology
* genre-specific terms
* cultivation terms
* honorific policy
* user corrections
* previously approved mappings

Translation does not own Knowledge persistence.

---

# 25. Terminology Constraints

Recommended semantic levels:

```text id="ay91n6"
LOCKED

PREFERRED

SUGGESTED

CONTEXTUAL
```

Meaning:

```text id="q81ia3"
LOCKED
    must be preserved/applied

PREFERRED
    should normally be applied

SUGGESTED
    may be used when context fits

CONTEXTUAL
    may vary with scene/speaker/context
```

Provider adapters convert these constraints into provider-specific forms internally.

---

# 26. Names and Transliteration

Translation may support policies:

* preserve original
* established Vietnamese form
* Sino-Vietnamese reading
* phonetic transliteration
* glossary-defined name
* preserve Latin form

Knowledge supplies mappings.

Translation applies them.

---

# 27. Pronouns and Honorifics

For Chinese → Vietnamese, Translation may need context around:

* relationship
* age
* status
* rank
* gender
* formality
* family role
* cultivation hierarchy

Translation must not assume a fixed one-to-one pronoun mapping.

When context insufficient:

* choose neutral form
* preserve ambiguity
* follow Translation Profile
* attach warning when useful

---

# 28. Novel Translation

Novel translation prioritizes:

* paragraph continuity
* narrative voice
* pronoun consistency
* name consistency
* sentence rhythm
* readable Vietnamese
* adjacent-paragraph context

Typical:

```text id="od2aaq"
SourceBlocks
    ↓
TranslationUnits
    ↓
Context-rich Batches
    ↓
TranslatedUnits
```

Novel profiles may use larger context windows.

---

# 29. Comic Translation

Comic translation prioritizes:

* bubble/source alignment
* short natural dialogue
* speaker consistency
* pronoun consistency
* nearby-dialogue context
* compact target output
* preservation of source-block relationships

Translation does not visually fit text into bubbles.

Presentation owns visual fitting.

---

# 30. Sound Effects

For `SOUND_EFFECT` SourceBlocks, Translation Profile may decide:

```text id="67n9mb"
TRANSLATE

TRANSLITERATE

PRESERVE

SKIP

PRESENTATION_HANDLING
```

Translation does not detect sound-effect regions.

It consumes upstream `SourceBlockType`.

---

# 31. Language Handling

Translation should support:

* explicit source language
* source language hint
* unknown source language with detection policy
* mixed-language SourceDocument
* explicit target language

Initial CRAI priorities may include:

```text id="7vke67"
Chinese → Vietnamese

English → Vietnamese
```

but architecture must remain language-pair independent.

---

# 32. Mixed-Language Content

Mixed-language source is valid.

Examples:

* Chinese dialogue with English names
* Japanese SFX with Chinese dialogue
* English skill names
* Latin character names
* numbers/symbols

Translation may apply per-Unit language hints where required.

---

# 33. Provider-Neutral Architecture

Public Translation contract must not contain provider SDK types such as:

```text id="51cadj"
OpenAIMessage

GeminiContent

ClaudeBlock

DeepLRequest
```

Canonical flow:

```text id="yixuid"
TranslationBatch
        ↓
Provider-neutral Request
        ↓
Provider Adapter
        ↓
Provider-specific Request
```

Provider-specific request/response formats remain behind Adapter boundary.

---

# 34. Provider Management Boundary

Provider Management owns:

* provider registry
* Provider lifecycle
* health
* availability
* credentials
* connection reuse
* model residency
* reusable provider resources
* provider capability descriptors

Translation consumes:

```text id="t361l6"
ProviderDescriptor

ProviderCapabilities

ProviderLease / ProviderHandle

Credential-safe Execution Boundary
```

Translation does not recreate Provider Manager.

---

# 35. Provider Capability Requirements

Translation may require capabilities such as:

* source language support
* target language support
* structured output
* batch support
* glossary support
* local execution
* streaming
* deterministic parameters
* usage reporting
* context-window size
* maximum payload
* cancellation support
* model class

Translation declares requirements.

Provider Management resolves available providers.

---

# 36. Provider Selection Policy

Translation may define semantic/provider preference requirements.

Selection may consider:

* user preference
* source language
* target language
* content profile
* expected quality
* latency
* cost
* privacy
* local-only requirement
* payload size
* required capabilities
* recent provider failure
* Provider Policy

Exact infrastructure/provider health mechanics stay outside Translation.

---

# 37. Provider Fallback

Translation may return fallback recommendations.

Example:

```text id="qw0n65"
preferred provider failed
        ↓
Translation Retry/Fallback Hint
        ↓
Runtime / Provider Selection
        ↓
alternative Provider Attempt
```

Fallback must never silently violate:

* local-only
* user-locked provider
* cost limit
* privacy policy
* model restrictions

---

# 38. Local and Remote Providers

Translation architecture supports:

```text id="ab7m8x"
LOCAL_PROVIDER

REMOTE_PROVIDER
```

Same logical provider boundary applies.

Execution location should be visible in provenance/policy metadata when relevant.

---

# 39. Provider Request Construction

Provider Adapter may construct:

* prompts
* provider messages
* glossary payload
* structured output schema
* model parameters
* timeout configuration
* provider-specific token limits

These do not escape Provider Adapter as public Translation models.

---

# 40. Prompt Injection Boundary

Source content is untrusted data.

For LLM-backed translation:

```text id="pu4k1s"
source text
    ≠
trusted instruction
```

Adapter must separate:

```text id="vutpjt"
System Translation Policy

Translation Profile

Terminology Constraints

Context

Source Content
```

Source text must not be allowed to request:

* access to unrelated application state
* credential disclosure
* system prompt changes
* policy override
* external tool usage outside approved execution scope

---

# 41. Provider Output Normalization

Provider-native output must be converted into CRAI-neutral models.

Conceptually:

```text id="bsz1g5"
Provider Response
      ↓
Provider Adapter
      ↓
Normalized Provider Translation Output
      ↓
Translation Validation
```

Public module contracts must never require provider-native response parsing.

---

# 42. Translated Unit

Canonical Translation-owned semantic result unit:

```text id="ssj44d"
TranslatedUnit
├── TranslatedUnitId
├── TranslationUnitId
├── SourceBlockRefs[]
├── TargetLanguage
├── TranslatedText
├── AlignmentMetadata
├── TerminologyMetadata?
├── Warnings[]
├── ProviderProvenance?
└── Metadata
```

Exact schema belongs in `CONTRACT.md`.

---

# 43. Structural Alignment

Every TranslatedUnit must remain traceable to:

```text id="y04a4d"
TranslationUnit
        ↓
SourceBlockRefs[]
        ↓
SourceDocument
```

This supports:

* Presentation
* selective retry
* variant comparison
* user correction
* source/translation side-by-side views
* partial results

---

# 44. Result Validation

Translation must validate normalized provider output before Candidate assembly.

Validation may include:

* expected TranslationUnit presence
* output identifier preservation
* source alignment
* target-language plausibility
* empty result
* duplicate result
* malformed structured output
* source-language leakage
* terminology constraints
* length anomalies
* missing Units
* duplicated Units
* forbidden metadata

Validation does not guarantee linguistic correctness.

---

# 45. Translation Quality Signals

Translation quality is multidimensional.

Possible signals:

* completeness
* alignment correctness
* terminology consistency
* target-language fluency
* source-language leakage
* structural validity
* length anomaly
* provider confidence
* user correction history

No single provider confidence score is sufficient to represent objective quality.

---

# 46. Confidence

Provider confidence is optional.

Rules:

* absence is valid
* provider-native values require normalization
* synthetic confidence must be marked synthetic
* confidence semantics must be explicit
* no universal numeric confidence is required

---

# 47. Candidate Translation Artifact

Translation does not publish directly.

It creates:

```text id="rbia8q"
CandidateTranslationArtifact
```

Conceptually:

```text id="3wuxmx"
CandidateTranslationArtifact
├── CandidateArtifactId
├── ArtifactType
├── OwnerModule
├── ContractVersion
├── SourceDocumentArtifactRef
├── TranslationIntentIdentity
├── TranslationProfileRef
├── TargetLanguage
├── TranslationUnits[]
├── TranslatedUnits[]
├── Completeness
├── MissingUnitRefs[]
├── Warnings[]
├── ProviderProvenance
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

---

# 48. Candidate Boundary

Candidate is:

* module-valid
* immutable after validation
* non-authoritative
* not yet published
* subject to Runtime authority validation
* cleaned if rejected

Translation determines:

```text id="w1i6ub"
is this Candidate semantically valid?
```

Runtime determines:

```text id="r0ebza"
does this Candidate still matter?
```

---

# 49. Published Translation Artifact

After Runtime acceptance:

```text id="rzxtua"
TranslationArtifact
├── ArtifactId
├── SourceDocumentArtifactRef
├── TranslationIntentIdentity
├── TranslationProfileRef
├── SourceLanguage?
├── TargetLanguage
├── TranslationUnits[]
├── TranslatedUnits[]
├── Completeness
├── Warnings[]
├── ProviderProvenance
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Artifact is immutable.

---

# 50. Translation Artifact vs Translation Plan

```text id="r22gl5"
TranslationPlan
    = execution planning

TranslationArtifact
    = semantic translated result
```

Do not persist execution-only planning state into Artifact unless needed for provenance.

---

# 51. Completeness

Recommended:

```text id="8mjvck"
COMPLETE

PARTIAL

EMPTY_VALID

UNKNOWN
```

---

# 52. EMPTY_VALID

Valid when SourceDocument contains no translatable content under current Translation Intent.

Example:

```text id="e62kf9"
SourceDocument has only excluded blocks
or
Translation Intent selects nothing
```

No failure required.

---

# 53. PARTIAL

Partial Translation Candidate may exist when:

* some Units succeed
* some Units fail
* streaming has completed some Units
* provider omitted Units
* Runtime deadline/cancellation arrives after usable output
* batch execution partially succeeds

Partial output must identify:

* completed Units
* missing Units
* failed scopes
* warnings
* source alignment

---

# 54. Partial Publication Boundary

Translation may mark Candidate:

```text id="653a2l"
Completeness = PARTIAL
```

but Translation does not decide canonical publication.

Runtime/Artifact policy decides whether partial Candidate is acceptable.

---

# 55. Streaming

Streaming must remain compatible with alignment.

Preferred public semantic streaming:

```text id="rwtl41"
Completed TranslationUnit
        ↓
Validated Partial Candidate Update
```

Raw provider token streaming should remain internal to Provider Adapter until content becomes structurally safe.

---

# 56. Translation Variants

A SourceDocument may have multiple immutable translation variants.

Examples:

```text id="lesfr2"
natural Vietnamese

literal Vietnamese

alternative provider

updated glossary

manual correction
```

Each variant should be represented as an immutable Translation Artifact or equivalent immutable variant object.

Do not overwrite old translation content in place.

---

# 57. Variant Identity

Conceptually variant semantics may include:

```text id="hb4vlt"
VariantId

SourceDocumentArtifactRef

TargetLanguage

TranslationProfileRef

KnowledgeSnapshotRef?

ProviderProvenance

TranslationIntentIdentity
```

Active variant selection is not automatically owned by Translation execution state.

Reading Session/User Preference/Application state may decide which variant is active.

---

# 58. User Corrections

A corrected translation should not mutate an existing immutable Translation Artifact.

Recommended:

```text id="on7y5s"
TranslationArtifact
        ↓
Correction / New Variant
        ↓
New Translation Artifact Variant
```

Knowledge updates, if any, are separate and explicit.

---

# 59. Semantic Compatibility

Translation defines semantic compatibility for reuse.

Possible dependencies:

```text id="c2prsr"
SourceDocument ContentIdentity

SourceDocument Contract Version

Translation Contract Version

TargetLanguage

Translation Profile Version

Knowledge Snapshot Version

Context Identity

Terminology Policy Version

Provider Policy Version

Translation Strategy Version

Privacy Partition
```

---

# 60. Cache Boundary

Translation owns:

```text id="bgngv0"
semantic compatibility
```

Runtime Cache Policy owns:

```text id="5kaf1i"
whether reuse occurs
```

Artifact Store owns:

```text id="uqarbz"
retained runtime Artifact lifecycle
```

Storage owns:

```text id="cl84zr"
durable persistence
```

Translation does not create a separate persistent cache subsystem.

---

# 61. Cache Safety

A Translation Artifact should not be reused when relevant semantic dependencies changed.

Examples:

* source content changed
* source alignment changed
* target language changed
* Translation Profile changed materially
* Knowledge Snapshot changed materially
* required terminology changed
* Context Identity changed
* privacy partition incompatible
* output contract incompatible

---

# 62. Runtime Boundary

Translation executes inside Runtime WorkItem/Attempt.

```text id="s6etpl"
Runtime WorkItem
      ↓
Runtime Attempt
      ↓
Translation Module
      ↓
Translation Plan
      ↓
Provider Execution
      ↓
Candidate Translation Artifact
      ↓
Attempt Completion
      ↓
Runtime Authority Validation
      ↓
Artifact Store
```

---

# 63. No TranslationJob Lifecycle

Legacy:

```text id="b5ekxm"
TranslationJob
```

as a second execution lifecycle is removed from the core architecture.

Runtime WorkItem already represents logical work.

Runtime Attempt already represents physical execution attempt.

Translation should not duplicate them with:

```text id="xszs1w"
TranslationJobState

TranslationAttemptState
```

---

# 64. Translation Intent vs WorkItem

These are different concepts.

```text id="wtrw3y"
TranslationIntent
    = semantic request

Runtime WorkItem
    = execution/control object
```

One Intent may produce new WorkItems over time due:

* retry
* manual retranslation
* profile change
* provider change
* new context
* new Knowledge Snapshot

Translation does not need a second Job state machine to represent this.

---

# 65. Runtime Attempt vs Translation Batch

A Translation Batch is semantic/provider planning.

A Runtime Attempt is execution control.

```text id="jliba7"
TranslationBatch
    ≠
Runtime Attempt
```

Runtime may execute one or several Batches according to execution architecture.

---

# 66. Retry Boundary

Translation may return:

```text id="tpcnyu"
RetryHint
```

or fallback recommendation.

Translation may decide retry is semantically allowed.

Runtime owns:

* retry budget
* backoff
* scheduling
* WorkItem/Attempt creation
* resource admission
* queueing

---

# 67. Retry Hint

Possible:

```text id="z2t6bw"
SAME_PROVIDER

ALTERNATIVE_PROVIDER

SMALLER_BATCH

ALTERNATIVE_CONTEXT_POLICY

ALTERNATIVE_TRANSLATION_PROFILE

RESOURCE_WAIT

NO_RETRY
```

No new Attempt is created by Translation itself.

---

# 68. Cancellation Boundary

Translation consumes Runtime Cancellation Context.

Cancellation means:

```text id="cx0pya"
stop creating new useful work
and
do not submit invalid/unauthorized Candidate
```

Translation does not own canonical cancellation state.

Provider physical cancellation should be requested when supported.

---

# 69. Late Provider Completion

Provider may complete after authority is lost.

```text id="wojlh1"
Runtime revokes authority
        ↓
Provider continues
        ↓
Provider returns late
        ↓
Translation may normalize/cleanup if necessary
        ↓
Candidate rejected / not submitted
```

Late completion never restores authority.

---

# 70. Supersession Boundary

Translation does not own a canonical `SUPERSEDED` state.

Supersession/staleness belongs to Runtime authority/revision model.

Examples:

* newer SourceDocument
* changed target language
* new Translation Intent
* changed Knowledge Snapshot
* changed context
* newer manual request

should result in new semantic work/compatibility identity.

Runtime prevents stale Candidate publication.

---

# 71. State Ownership

Translation may own local semantic state such as:

```text id="axkfb4"
Module Availability

Translation Plan State

Translation Unit Planning State

Batch Planning State

Provider Execution Observation

Candidate Validation State

Translation Completeness
```

Exact state model belongs to `STATES.md`.

It does not own:

```text id="eq24zv"
WorkItemState

AttemptState

RetryState

CancellationState

SupersessionState

ArtifactPublicationState
```

---

# 72. Provider Execution Observation

Translation may observe:

```text id="s1151j"
NOT_STARTED

REQUESTED

RUNNING

OUTPUT_RECEIVED

ERROR_RECEIVED

CANCEL_REQUESTED

PHYSICALLY_FINISHED
```

This is diagnostic/local.

Provider Management remains owner of Provider lifecycle.

---

# 73. Event Boundary

Translation events should describe module facts, not duplicate Runtime lifecycle.

Possible:

```text id="bwqnco"
TRANSLATION_PLAN_CREATED

TRANSLATION_BATCH_PLANNED

TRANSLATION_PROVIDER_OUTPUT_RECEIVED

TRANSLATION_CANDIDATE_VALIDATED

TRANSLATION_CANDIDATE_SUBMITTED

TRANSLATION_WARNING_RECORDED

TRANSLATION_MODULE_ERROR_RECORDED
```

Do not create authoritative aliases:

```text id="a5v2hq"
TRANSLATION_COMPLETED

TRANSLATION_FAILED

TRANSLATION_CANCELED

TRANSLATION_SUPERSEDED
```

for Runtime terminal state.

---

# 74. Error Boundary

Translation owns module-level semantic errors such as:

```text id="xnd4vx"
TRANSLATION_INPUT_INVALID

TRANSLATION_PLAN_INVALID

TRANSLATION_UNIT_PLANNING_FAILED

TRANSLATION_BATCH_BUILD_FAILED

TRANSLATION_CONTEXT_UNAVAILABLE

TRANSLATION_TERMINOLOGY_INVALID

TRANSLATION_PROVIDER_EXECUTION_FAILED

TRANSLATION_PROVIDER_OUTPUT_INVALID

TRANSLATION_ALIGNMENT_FAILED

TRANSLATION_VALIDATION_FAILED

TRANSLATION_CANDIDATE_INVALID

TRANSLATION_PRIVACY_VIOLATION

TRANSLATION_INTERNAL_ERROR
```

Provider-native errors must remain normalized/referenced.

Runtime errors remain Runtime-owned.

---

# 75. Warning Boundary

Possible warnings:

```text id="7di46m"
MISSING_OPTIONAL_CONTEXT

PARTIAL_TRANSLATION

TERMINOLOGY_CONFLICT

PROVIDER_FALLBACK_USED

SOURCE_APPEARS_INCOMPLETE

TARGET_OUTPUT_LENGTH_ANOMALY

UNTRANSLATED_SOURCE_FRAGMENT

AMBIGUOUS_PRONOUN

AMBIGUOUS_SPEAKER_RELATIONSHIP

LOW_TRANSLATION_CONFIDENCE
```

Warnings do not automatically invalidate Candidate.

---

# 76. Security and Privacy

Translation input may be sensitive.

Rules:

1. Raw source content not logged by default.
2. Translated text not logged by default.
3. Provider credentials never appear in Translation Artifact.
4. Provider credentials never appear in events.
5. Provider raw response does not cross public boundary.
6. Remote translation requires compatible Privacy Context.
7. Local-only policy must disable remote provider.
8. Provider logging policy must be explicit.
9. Protected diagnostics require explicit authorization.
10. Source text remains untrusted input.

---

# 77. Observability

Useful Translation metrics:

```text id="uumn61"
translation.plan_ms

translation.unit_count

translation.batch_count

translation.batch_size

translation.provider_latency_ms

translation.normalization_ms

translation.validation_ms

translation.total_ms

translation.partial_total

translation.warning_total

translation.error_total

translation.fallback_total

translation.provider_usage
```

Runtime lifecycle metrics remain Runtime-owned.

---

# 78. Cost Control

Translation may express semantic cost constraints:

* max remote cost
* max tokens
* max characters
* allowed providers
* fallback cost policy
* preferred local provider
* cache-first preference
* visible-content priority
* bounded prefetch policy

Runtime/Provider Management enforce execution/resource mechanics according to their contracts.

---

# 79. Latency Strategy

CRAI prioritizes uninterrupted reading.

Translation may improve latency via:

* appropriate batching
* semantic cache reuse
* visible content first
* bounded prefetch
* concurrent batches when safe
* local providers
* fast-provider fallback
* partial Candidates
* segment-completion streaming

Latency optimization must preserve alignment and authority safety.

---

# 80. Prefetch Boundary

Reading Session/Business Pipeline determines what upcoming content may be useful.

Translation receives semantic translation work for those artifacts.

Translation may mark:

```text id="p1vbbg"
INTERACTIVE

VISIBLE

PREFETCH

BACKGROUND
```

as execution intent.

Runtime decides actual scheduling priority.

---

# 81. Concurrency

Translation may plan independent batches concurrently.

Runtime owns actual concurrency admission.

Concurrency must preserve:

* TranslationUnit identity
* source ordering
* alignment
* batch independence
* terminology policy
* Candidate assembly determinism
* provider limits
* privacy constraints

Batch completion order must not determine final Unit order.

---

# 82. Ordering

Source order comes from SourceDocument.

Translation does not infer comic Reading Order from geometry.

Translation may:

```text id="iziy66"
consume SourceBlockSequence
        ↓
construct TranslationUnit order
        ↓
preserve explicit mapping
```

TranslatedUnit order must remain source-traceable.

---

# 83. Determinism

Translation may not always be perfectly deterministic because providers can be nondeterministic.

However semantic planning should be deterministic for equivalent:

```text id="ln5pyd"
SourceDocument

Translation Intent

Translation Profile

Context Snapshot

Knowledge Snapshot

Provider Policy

Configuration Snapshot
```

Provider nondeterminism must be represented through provenance rather than hidden.

---

# 84. Translation Provenance

Candidate/Artifact may record:

```text id="ei4dbn"
ProviderId

ProviderVersion

AdapterVersion

ModelId?

ModelVersion?

ExecutionLocation

TranslationProfileVersion

KnowledgeSnapshotVersion?

ContextIdentity?

ProviderPolicyVersion
```

No credentials.

---

# 85. User Corrections and Knowledge

Translation may create a new corrected variant.

Knowledge may optionally learn/update persistent terminology.

But:

```text id="hj1g8j"
user correction
    ≠
automatic global Knowledge mutation
```

Knowledge changes require explicit boundary/action.

---

# 86. Presentation Boundary

Presentation consumes Translation Artifact.

Presentation may require:

* SourceBlock alignment
* translated text
* Unit ordering
* Source geometry references through upstream lineage
* warnings relevant to display
* completeness

Presentation does not need:

* provider prompts
* provider raw response
* retry history
* batch execution internals

---

# 87. Text Processing Compatibility

Translation expects SourceDocument to provide:

* stable SourceBlock IDs
* Raw/Normalized source semantics
* SourceBlockSequence
* structural metadata
* language hints
* SourceDocument identity
* traceability
* exclusions

Translation must not reconstruct OCR geometry or source layout from scratch.

---

# 88. Provider Management Compatibility

Provider Management supplies:

* provider descriptors
* capabilities
* availability
* health
* leases/handles
* credentials through secure boundary

Translation supplies:

* required capabilities
* source/target language requirements
* privacy requirements
* Provider Policy
* execution request semantics

---

# 89. Runtime Compatibility

Runtime supplies:

* WorkItemId
* AttemptId
* RevisionId
* Execution Context
* Cancellation Context
* Deadline
* resource budget
* authority boundary

Translation returns:

* CandidateTranslationArtifact
* warnings
* module error
* RetryHint
* diagnostics

---

# 90. MVP Scope

MVP Translation should support:

* `SourceDocumentArtifact` input
* SourceBlock-based Translation Unit planning
* Chinese → Vietnamese
* English → Vietnamese
* explicit target language
* `COMIC_NATURAL`
* `NOVEL_NATURAL`
* `GENERAL_NATURAL`
* multi-Unit batches
* Provider abstraction
* at least one provider adapter
* context support
* terminology snapshot/reference
* stable source alignment
* provider-output normalization
* output validation
* logical cancellation observation
* retry/fallback hints
* Candidate Translation Artifact
* partial Candidate support
* semantic compatibility
* local/remote privacy policy
* prompt-injection protection

---

# 91. MVP Translation Unit Rules

For MVP:

```text id="q6mlxc"
Comic:
    usually one SourceBlock
    → one TranslationUnit

Novel:
    one or multiple paragraph SourceBlocks
    → one TranslationUnit
    when context/size permits
```

Translation Unit construction should initially remain conservative.

---

# 92. MVP Batch Rules

MVP batch planning may use:

* maximum Units
* maximum characters
* estimated token budget
* paragraph/dialogue boundaries
* SourceDocument boundaries
* provider context limit

No advanced learned batching required.

---

# 93. MVP Partial Policy

MVP may allow:

```text id="1nmlxt"
PARTIAL
```

for independent comic bubbles/paragraphs when alignment remains valid.

Atomic mode may still be required for specific profiles.

---

# 94. Deferred Capabilities

Can defer:

* provider quality ranking
* multiple-provider ensemble
* translation memory search
* advanced offline model distribution
* token-level public streaming
* automatic glossary learning
* advanced speaker inference
* full-novel pronoun optimization
* voting across variants
* learned quality scoring
* distributed Translation
* long-term personalization

---

# 95. Open Decisions

Still open:

* exact TranslationUnit schema
* exact TranslationArtifact schema
* Batch-to-Runtime execution mapping
* source splitting policy
* multi-SourceBlock Unit rules
* context snapshot schema
* Knowledge Snapshot schema
* public variant model
* partial Candidate granularity
* Translation quality signals
* Translation profile versioning
* provider-policy compatibility
* streaming public contract
* sound-effect default
* default batch size
* default context range

These decisions must preserve ownership boundaries defined here.

---

# 96. Architecture Invariants

1. Translation consumes `SourceDocumentArtifact`.

2. Translation does not consume raw OCR as canonical input.

3. Translation does not mutate SourceDocument.

4. Translation owns TranslationUnit.

5. Translation Unit is distinct from SourceBlock.

6. Translation owns TranslationBatch semantics.

7. Translation does not own Runtime WorkItem.

8. Translation does not own Runtime Attempt.

9. Translation does not create a parallel TranslationJob lifecycle.

10. Translation does not create a parallel TranslationAttempt lifecycle.

11. Runtime owns retry execution.

12. Runtime owns cancellation authority.

13. Runtime owns stale-result authority.

14. Runtime owns terminal Attempt outcome.

15. Translation creates Candidate Artifact only.

16. Artifact Store owns accepted published Artifact lifecycle.

17. Provider Management owns Provider lifecycle.

18. Provider SDK types never cross public Translation boundary.

19. Provider credentials never appear in Translation Artifact.

20. Source content is untrusted data.

21. Source content must not override Translation system policy.

22. Every TranslatedUnit maps to one or more TranslationUnits.

23. Every TranslationUnit maps to one or more SourceBlocks.

24. Source alignment is never lost.

25. Translation does not infer OCR Reading Order.

26. Source ordering derives from SourceDocument.

27. Translation Profile is provider-neutral.

28. TranslationBatch is provider-neutral.

29. Translation result validation precedes Candidate validation.

30. Empty translation may be valid.

31. Partial output is explicit.

32. Missing context may be warning rather than failure.

33. Provider confidence is optional.

34. Semantic compatibility is explicit.

35. Cache reuse belongs to Runtime policy.

36. Durable persistence belongs to Storage.

37. Late provider result never regains authority.

38. Candidate validation does not imply publication.

39. Variant history is immutable.

40. Manual correction does not mutate old Translation Artifact.

41. Knowledge persistence is external.

42. Presentation owns visual fitting.

43. Text Processing owns source reconstruction.

44. Translation owns target-language semantic transformation.

45. Normal logs contain no full source/translated content.

---

# 97. Related Documents

```text id="1s73ot"
02-modules/translation/README.md
02-modules/translation/MODULE.md
02-modules/translation/CONTRACT.md
02-modules/translation/STATES.md
02-modules/translation/EVENTS.md
02-modules/translation/ERRORS.md

02-modules/text-processing/README.md
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md

02-modules/provider-management/
02-modules/knowledge/
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

# 98. Summary

Translation transforms:

```text id="jltpgr"
SourceDocumentArtifact
        ↓
Translation Intent
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
TranslationArtifact
```

Ownership boundary:

```text id="bdsv83"
Text Processing
    owns SourceDocument and SourceBlock.

Translation
    owns Translation Intent,
    Translation Plan,
    Translation Unit,
    Translation Batch,
    translated semantic output.

Provider Management
    owns Provider lifecycle,
    availability and credentials.

Runtime
    owns WorkItem,
    Attempt,
    retry,
    cancellation
    and authority.

Artifact Store
    owns accepted Artifact lifecycle.

Knowledge
    owns persistent terminology and knowledge.

Presentation
    owns visual layout and rendering.
```

The key rule is:

```text id="v7fnm7"
SourceBlock is where source structure stabilizes.

TranslationUnit is where translation semantics begin.

Runtime decides whether execution still matters.

Artifact Store publishes the accepted result.
```
