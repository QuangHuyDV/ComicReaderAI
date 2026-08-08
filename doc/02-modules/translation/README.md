# Translation Module

> **Project:** CRAI
> **Module:** Translation
> **Path:** `02-modules/translation/README.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `STATES.md`, `EVENTS.md`, `ERRORS.md`

---

# 1. Overview

Translation is the CRAI module responsible for converting a stable semantic source representation into aligned translated semantic output.

Its core responsibility is:

```text
SourceDocumentArtifact
        ↓
Translation Intent
        ↓
Translation Plan
        ↓
Translation Units
        ↓
Context / Terminology
        ↓
Translation Batches
        ↓
Provider Execution
        ↓
Validated Translated Units
        ↓
Translation Candidate
```

Translation is responsible for the **meaning transformation**.

It is not responsible for the generic lifecycle of executing that transformation.

That distinction is fundamental to the CRAI architecture.

---

# 2. Language Priorities

Initial language priorities are:

```text
Chinese → Vietnamese

English → Vietnamese
```

The architecture remains language-neutral.

Core contracts must not hardcode these language pairs.

Future combinations may include:

```text
Japanese → Vietnamese

Korean → Vietnamese

Chinese → English

English → Chinese

or other supported pairs
```

without redesigning the Translation domain.

---

# 3. Module Position

Translation operates after source acquisition, recognition and semantic text preparation.

```text
Source
    ↓
Observation
    ↓
Recognition
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
Runtime Candidate Handling
    ↓
Artifact Store
    ↓
Reading Session
    ↓
Presentation
```

Translation does not consume canonical raw acquisition data directly.

It does not own:

```text
raw browser DOM

screenshots

source images

raw OCR output

raw extraction output

visual regions

canonical reading-order detection
```

Those concerns belong upstream.

---

# 4. Core Architectural Boundary

The most important architectural rule is:

```text
Translation owns semantic translation.

Runtime owns execution lifecycle.
```

Translation decides:

```text
what should be translated

how source blocks become Translation Units

what context is needed

what terminology constraints apply

how Units should be batched

what provider capabilities are required

how provider output is interpreted

whether translated output is semantically valid

whether source alignment is valid

whether a Translation Candidate is valid

whether retry may be semantically useful
```

Runtime decides:

```text
when work executes

where work executes

queue admission

worker assignment

execution Attempt lifecycle

retry timing

backoff

execution budgets

deadlines

cancellation

supersession

stale-result authority

terminal execution outcome
```

---

# 5. Core Flow

The logical Translation flow is:

```text
SourceDocumentArtifact
        ↓
TranslationAttemptInput
        ↓
Translation Plan
        ↓
TranslationUnit[]
        ↓
TranslationBatch[]
        ↓
Provider-neutral execution request
        ↓
Provider Adapter
        ↓
Provider-neutral output
        ↓
TranslatedUnit[]
        ↓
Validation
        ↓
Alignment
        ↓
Translation Candidate
        ↓
Runtime
```

Runtime may then:

```text
accept

retry

reject stale

cancel

abandon

publish
```

depending on Runtime policy and current authority.

---

# 6. Translation Is Not a Job System

Translation does not define:

```text
TranslationJob

TranslationAttempt
```

as its own execution lifecycle entities.

CRAI already has generic Runtime abstractions:

```text
WorkItem

Attempt

Revision
```

Translation provides the semantic work executed by those abstractions.

Therefore:

```text
TranslationIntent
    ≠ WorkItem

TranslationPlan
    ≠ Attempt

TranslationBatch
    ≠ Runtime Attempt
```

---

# 7. SourceDocumentArtifact

Translation receives a stable semantic source Artifact produced upstream.

Conceptually:

```text
SourceDocumentArtifact
├── ArtifactId
├── ContentIdentity
├── ContractVersion
├── SourceLanguage?
├── SourceBlocks[]
├── Sequence
├── SemanticMetadata
├── Provenance
└── PrivacyMetadata
```

Translation treats this Artifact as immutable input.

It must never modify upstream source content.

---

# 8. SourceBlock

`SourceBlock` is the stable semantic source unit supplied by the upstream text-processing boundary.

Examples:

```text
novel paragraph

dialogue line

comic speech bubble

narration box

caption

sound effect

web text block
```

Translation preserves traceability back to SourceBlocks.

---

# 9. TranslationUnit

`TranslationUnit` is the Translation-owned semantic translation unit.

```text
TranslationUnit
    = unit sent through translation semantics
      while preserving source lineage
```

A TranslationUnit may correspond to:

```text
1 SourceBlock → 1 TranslationUnit
```

but the architecture also supports:

```text
N SourceBlocks → 1 TranslationUnit
```

and controlled:

```text
1 SourceBlock → N TranslationUnits
```

when traceability remains deterministic.

---

# 10. SourceBlock vs TranslationUnit

These concepts must not be confused.

```text
SourceBlock
    = upstream semantic source identity
```

```text
TranslationUnit
    = Translation-owned semantic execution unit
```

Translation may restructure source content for better translation quality.

It may not destroy source traceability.

---

# 11. TranslationIntent

`TranslationIntent` describes what translation the caller wants.

Typical fields include:

```text
SourceLanguage?

TargetLanguage

TranslationProfile

SourceSelection

ProviderPolicy

ContextPolicy

TerminologyPolicy

PartialResultPolicy

PrivacyContext
```

A TranslationIntent describes semantic requirements.

It does not describe Runtime lifecycle.

---

# 12. TranslationPlan

Translation resolves Intent and source input into an immutable Translation Plan.

Conceptually:

```text
TranslationPlan
├── TranslationPlanId
├── SourceDocumentRef
├── TranslationIntentId
├── TranslationProfile
├── SourceLanguage
├── TargetLanguage
├── TranslationUnits[]
├── ContextPlan
├── TerminologyPlan
├── ProviderRequirements
├── BatchPolicy
├── PartialResultPolicy
├── PrivacyContext
└── ConfigurationSnapshot
```

Once ready for execution, the Plan is immutable.

---

# 13. TranslationBatch

A TranslationBatch is the provider execution grouping owned by Translation.

```text
TranslationBatch
    = one provider-facing group
      of Translation Units
```

Batching may improve:

* dialogue continuity;
* narrative continuity;
* terminology consistency;
* contextual consistency;
* provider efficiency;
* latency;
* cost.

Batch membership never replaces TranslationUnit identity.

---

# 14. TranslationBatch vs Runtime Attempt

A Batch describes:

```text
what Translation Units
should be translated together
```

A Runtime Attempt describes:

```text
one execution attempt
of work
```

Therefore:

```text
one TranslationBatch
```

may participate in more than one Runtime Attempt through retry.

Translation does not maintain an internal `TranslationAttempt` lifecycle.

---

# 15. TranslatedUnit

Provider output is normalized into Translation-owned translated units.

Conceptually:

```text
TranslatedUnit
├── TranslationUnitId
├── SourceBlockRefs[]
├── TargetLanguage
├── TranslatedContent
├── ProviderProvenance
├── ValidationMetadata
├── TerminologyMetadata
└── Warnings[]
```

A TranslatedUnit is not trusted merely because the provider returned it.

It must pass Translation validation.

---

# 16. Translation Candidate

The output of Translation is a Candidate.

```text
TranslationCandidate
├── CandidateArtifactId
├── SourceDocumentRef
├── TranslationIntentId
├── TranslationPlanId
├── TargetLanguage
├── TranslatedUnits[]
├── Completeness
├── MissingTranslationUnitIds[]
├── FailedTranslationUnitIds[]
├── Warnings[]
├── TraceabilityMetadata
└── ProviderProvenance
```

A Candidate means:

```text
Translation believes
this semantic output is valid.
```

It does not mean:

```text
the output is still authoritative

the Runtime still wants it

the Artifact has been published

the reader currently sees it
```

---

# 17. Candidate Completeness

Canonical completeness values include:

```text
COMPLETE

PARTIAL

EMPTY_VALID
```

### COMPLETE

All required Translation Units are represented.

### PARTIAL

Some required or requested Units could not be produced, but policy allows usable partial output.

### EMPTY_VALID

There was legitimately nothing requiring translation.

No selected Unit may disappear silently.

---

# 18. Candidate vs Artifact

Translation produces:

```text
Candidate
```

not authoritative durable publication.

Conceptually:

```text
Translation
    ↓
Candidate
    ↓
Runtime
    ↓
Artifact Store
    ↓
Translation Artifact
```

Artifact Store owns durable publication and persistence.

---

# 19. Candidate Authority

A valid Candidate may still be rejected by Runtime.

Example:

```text
Attempt starts
    ↓
Translation executes
    ↓
Candidate becomes VALID
    ↓
newer Revision becomes authoritative
    ↓
Runtime receives Candidate
    ↓
REJECTED_STALE
```

This is not a Translation failure.

Translation answered the semantic question correctly.

The answer simply no longer matters to the active Runtime Revision.

---

# 20. Text and Image Translation

CRAI does not require separate Translation engines for text and image content.

The pipelines converge before Translation.

### Text source

```text
Web page / document
        ↓
Source
        ↓
Observation
        ↓
Text Processing
        ↓
SourceDocumentArtifact
        ↓
Translation
```

### Comic / image source

```text
Image / comic page
        ↓
Observation
        ↓
Recognition
        ↓
Text Processing
        ↓
SourceDocumentArtifact
        ↓
Translation
```

Translation receives the same semantic source contract.

---

# 21. Novel Translation

Typical priorities:

* paragraph continuity;
* narrative flow;
* consistent names;
* pronoun consistency;
* long-range context;
* natural Vietnamese prose;
* terminology stability.

Recommended initial profile:

```text
NOVEL_NATURAL
```

---

# 22. Comic Translation

Typical priorities:

* speech-bubble alignment;
* concise dialogue;
* speaker consistency;
* neighboring-dialogue context;
* sound-effect handling;
* terminology consistency;
* reasonable output length.

Recommended initial profile:

```text
COMIC_NATURAL
```

Translation does not perform visual text fitting.

---

# 23. Presentation Boundary

Presentation owns:

```text
font selection

font size

line breaking

bubble fitting

overlay placement

image composition

visual overflow handling

reader-facing rendering
```

Translation may provide semantic hints such as:

```text
output length

content type

speaker identity

sound-effect classification
```

but must not perform layout.

---

# 24. Translation Profiles

Initial profile identifiers:

```text
NOVEL_NATURAL

COMIC_NATURAL

GENERAL_NATURAL

LITERAL

CUSTOM
```

Profiles describe translation semantics.

They must not expose provider-native prompt templates through public contracts.

---

# 25. Context Model

Translation distinguishes:

```text
content being translated
```

from:

```text
context used to improve translation
```

Possible context includes:

* preceding SourceBlocks;
* following SourceBlocks;
* dialogue groups;
* paragraph groups;
* chapter summaries;
* previous translations;
* character information;
* names and aliases;
* relationships;
* terminology;
* reading-session context.

Context must not accidentally create translated output unless explicitly selected.

---

# 26. Context Snapshot

Translation should resolve execution context into immutable references or snapshots.

Conceptually:

```text
ContextSnapshot
├── ContextSnapshotId
├── SourceContext[]
├── CharacterContext[]
├── PreviousTranslationContext[]
├── ReadingContext?
└── Provenance
```

This improves reproducibility and debugging.

---

# 27. Knowledge Integration

Knowledge owns persistent knowledge such as:

```text
glossary

characters

aliases

relationships

terminology history

learned terminology
```

Translation consumes Knowledge through immutable snapshots or revisioned references.

Translation may use:

```text
KnowledgeSnapshotId

TermConstraint[]

CharacterContext[]

RelationshipContext[]
```

Translation does not mutate Knowledge directly.

---

# 28. Terminology Constraints

Translation may apply constraints such as:

```text
LOCKED

PREFERRED

OPTIONAL
```

Example:

```text
SourceTerm:
    林凡

TargetTerm:
    Lâm Phàm

Constraint:
    LOCKED
```

A locked term must not be silently ignored.

---

# 29. Provider Architecture

Translation uses provider-neutral execution contracts.

```text
TranslationBatch
        ↓
Provider-neutral request
        ↓
Provider Adapter
        ↓
Provider-specific execution
        ↓
Provider-neutral response
        ↓
Translation validation
```

Provider-specific SDK types must remain behind adapter boundaries.

---

# 30. Provider Management Boundary

Provider Management owns:

```text
provider registration

provider enablement

credentials

credential refresh

provider lifecycle

provider health

capability discovery

local-model residency
```

Translation owns:

```text
translation-specific provider requirements

provider suitability for the Plan

fallback eligibility

provider-neutral request construction

provider output interpretation
```

---

# 31. Supported Provider Types

Architecture supports:

```text
REMOTE_PROVIDER

LOCAL_PROVIDER
```

Possible providers may include:

```text
OpenAI

Gemini

Claude

DeepL

local translation models

future providers
```

The Translation domain must remain independent of any specific provider.

---

# 32. Provider Policies

Possible policies include:

```text
AUTOMATIC

PREFERRED

REQUIRED

LOCAL_ONLY

REMOTE_ONLY
```

Provider selection must obey:

```text
Translation Plan

Provider capabilities

Privacy Context

Runtime resource availability
```

---

# 33. Local Translation

Local execution may be useful for:

* privacy;
* offline reading;
* cost reduction;
* restricted content;
* poor network conditions;
* user-controlled models.

Local provider lifecycle remains outside Translation.

---

# 34. Retry Model

Translation does not execute retries itself.

Translation may return:

```text
TranslationRetryHint
```

Examples:

```text
RETRYABLE

CONDITIONALLY_RETRYABLE

NON_RETRYABLE
```

Possible strategies:

```text
SAME_PROVIDER

ALTERNATIVE_PROVIDER

SMALLER_BATCH

REDUCE_CONTEXT

ALTERNATIVE_TRANSLATION_PROFILE

RESOURCE_WAIT

USE_LOCAL_PROVIDER
```

Runtime decides whether a new Attempt is actually created.

---

# 35. Retry Example

```text
Runtime Attempt 1
        ↓
Translation
        ↓
Provider unavailable
        ↓
TranslationRetryHint
    ALTERNATIVE_PROVIDER
        ↓
Runtime Retry Policy
        ↓
Runtime Attempt 2
        ↓
Translation
```

Translation does not mutate failed local state back into an active state.

---

# 36. Retranslation

Retranslation is a semantic request, not a Runtime retry.

Examples:

```text
COMIC_NATURAL
    ↓
user requests LITERAL
```

or:

```text
old terminology
    ↓
updated terminology
```

or:

```text
Chinese → Vietnamese
    ↓
Chinese → English
```

These produce new semantic translation intent/plan rather than merely retrying failed execution.

---

# 37. Partial Translation

Translation supports valid partial output when policy allows.

Example:

```text
10 Translation Units

7 valid

2 failed

1 missing
```

Candidate:

```text
Completeness = PARTIAL
```

and explicitly records:

```text
FailedTranslationUnitIds

MissingTranslationUnitIds
```

No requested Unit may disappear silently.

---

# 38. Progressive Translation

Progressive reading is supported through Candidate fragments or validated Translation progress facts.

This is especially useful for:

```text
comic bubbles

visible page regions

interactive reading

low-latency overlays
```

Progressive delivery does not weaken validation or alignment requirements.

---

# 39. Streaming

Provider token streaming is an internal adapter concern.

Preferred semantic public granularity is:

```text
validated TranslationUnit
```

or:

```text
validated group of TranslationUnits
```

Conceptually:

```text
Provider token stream
        ↓
Adapter
        ↓
Unit assembly
        ↓
Validation
        ↓
TranslatedUnit
        ↓
semantic progress
```

Consumers should not depend directly on provider token streams.

---

# 40. Cancellation

Cancellation belongs to Runtime.

Translation may observe:

```text
CancellationContext
```

or equivalent execution context.

Translation should stop unnecessary work when practical.

However:

```text
provider physical cancellation
```

is best-effort infrastructure behavior.

The authoritative cancellation state belongs to Runtime.

---

# 41. Supersession and Stale Work

Translation does not maintain a `SUPERSEDED` Translation lifecycle.

Runtime determines whether work is still authoritative.

Example:

```text
Revision A
    ↓
translation running

Revision B becomes current

Revision A Candidate arrives
    ↓
Runtime authority check
    ↓
REJECTED_STALE
```

The Candidate may still be semantically valid for Revision A.

---

# 42. Translation Variants

Translation supports immutable semantic alternatives.

Examples:

```text
natural translation

literal translation

fallback-provider translation

user-corrected translation

terminology-updated translation
```

A published immutable variant is never edited in place.

A correction creates a new variant.

---

# 43. Active Variant Boundary

Translation may construct variants.

Translation does not own:

```text
which variant is currently active
for a reader
```

Reading Session/application state owns active selection.

Therefore Translation does not maintain:

```text
ACTIVE

INACTIVE
```

as execution lifecycle states.

---

# 44. State Model

Translation maintains only Translation-local semantic states.

Main entities:

```text
Translation Plan

Translation Batch

Translation Candidate
```

Typical Plan flow:

```text
BUILDING
    ↓
READY
```

Typical Batch flow:

```text
PLANNED
    ↓
READY
    ↓
EXECUTING
    ↓
VALIDATING
    ↓
VALID
```

Failure path:

```text
any valid execution state
    ↓
INVALID
```

Candidate flow:

```text
ASSEMBLING
    ↓
VALIDATING
    ↓
VALID
```

or:

```text
ASSEMBLING / VALIDATING
    ↓
INVALID
```

See:

```text
02-modules/translation/STATES.md
```

---

# 45. Runtime States Are External

Translation does not define:

```text
QUEUED

RETRY_SCHEDULED

CANCELLATION_REQUESTED

CANCELLED

SUPERSEDED

ABANDONED

STALE
```

as Translation domain states.

Those belong to Runtime lifecycle.

---

# 46. Events

Translation events communicate semantic Translation facts.

Examples:

```text
TranslationPlanReady

TranslationBatchPlanned

TranslationBatchValidated

TranslationBatchInvalid

TranslationCandidateAssembled

TranslationCandidateValidated

TranslationCandidateInvalid

TranslationPartialCandidateAvailable

TranslationProviderFallbackRecommended
```

Events are:

* immutable;
* provider-neutral;
* compact;
* traceable;
* privacy-safe;
* duplicate-delivery tolerant where required.

See:

```text
02-modules/translation/EVENTS.md
```

---

# 47. Runtime Events Are Not Translation Events

Translation does not publish canonical events such as:

```text
TranslationQueued

TranslationRetryScheduled

TranslationCancelled

TranslationSuperseded

TranslationJobFailed
```

when those represent Runtime lifecycle.

Runtime publishes its own execution facts.

---

# 48. Errors

Translation normalizes semantic failures into provider-neutral error codes.

Examples:

```text
TRN-INPUT-002
SOURCE_DOCUMENT_UNAVAILABLE

TRN-PLAN-003
PROVIDER_POLICY_UNSATISFIABLE

TRN-CTX-003
CONTEXT_TOO_LARGE

TRN-TERM-003
LOCKED_TERMINOLOGY_VIOLATED

TRN-PROV-005
PROVIDER_RATE_LIMITED

TRN-OUT-002
PROVIDER_OUTPUT_MALFORMED

TRN-ALIGN-001
TRANSLATION_ALIGNMENT_FAILED

TRN-CAND-002
CANDIDATE_INVALID
```

See:

```text
02-modules/translation/ERRORS.md
```

---

# 49. Warnings

Warnings describe usable degraded outcomes.

Examples:

```text
MISSING_OPTIONAL_CONTEXT

CONTEXT_TRUNCATED

LOW_TRANSLATION_CONFIDENCE

PRONOUN_AMBIGUITY

PROVIDER_FALLBACK_USED

PARTIAL_TRANSLATION

OUTPUT_LENGTH_ANOMALY
```

Warnings do not automatically invalidate Candidate output.

---

# 50. Error Ownership

Translation owns failures concerning:

```text
Translation input semantics

Plan construction

Translation Unit construction

Context construction

Terminology application

Batch planning

provider-boundary execution semantics

provider output normalization

output validation

alignment

Candidate assembly

Candidate validation

translation privacy

translation security

Translation-local invariants
```

---

# 51. External Error Ownership

Runtime owns:

```text
queue failures

scheduler failures

Attempt deadline

retry exhaustion

cancellation lifecycle

worker crashes

stale-result rejection
```

Provider Management owns:

```text
provider registry

credentials

credential refresh

provider lifecycle

provider health
```

Artifact infrastructure owns:

```text
publication

durable persistence

retention

cache mechanics
```

---

# 52. Cache Strategy

Translation does not own cache infrastructure.

Semantic reuse may still exist.

A reusable Translation Artifact must be compatible with at least:

```text
source content identity

SourceDocument semantic identity

selected source content

source language

target language

Translation Profile

terminology context

translation context

provider requirements

privacy partition
```

Runtime Cache Policy determines reuse mechanics.

Artifact Store owns stored Artifact access.

---

# 53. Cache Failure

Infrastructure cache failure should normally not become Translation failure.

Conceptually:

```text
Cache unavailable
        ↓
Runtime / infrastructure policy
        ↓
normal Translation execution
```

Translation only validates whether reused semantic output is compatible.

---

# 54. Privacy and Security

Translation content may be private.

Public contracts, events and logs must not expose:

* API keys;
* access tokens;
* authorization headers;
* raw provider prompts;
* full provider responses;
* unnecessary source text;
* unnecessary translated text;
* private Knowledge contents.

---

# 55. Source Content Is Untrusted

```text
source content
    ≠
system instruction
```

Instruction-like text inside a novel, comic or webpage remains source data.

It must not control:

```text
provider selection

state transitions

event types

Candidate identity

routing metadata

security policy
```

---

# 56. Provider Output Is Untrusted

Provider output must not directly control:

```text
Translation state

Runtime state

event type

provider selection

Candidate identity

SourceBlock identity

routing

privacy policy
```

Provider output passes through:

```text
parse
    ↓
normalize
    ↓
validate
    ↓
align
    ↓
Candidate assembly
```

---

# 57. Observability

Translation-owned metrics may include:

```text
translation.plan.total

translation.batch.total

translation.candidate.total

translation.partial.total

translation.error.total

translation.warning.total

translation.provider_fallback_total

translation.provider_output_invalid_total

translation.alignment_failure_total

translation.candidate_invalid_total

translation.input_units

translation.output_units

translation.estimated_cost
```

---

# 58. Runtime Observability Boundary

Translation should not redefine Runtime metrics such as:

```text
queue duration

scheduler delay

Attempt retry count

Attempt deadline count

worker utilization

cancellation count
```

Those belong to Runtime observability.

Translation may correlate with them through:

```text
RevisionId

WorkItemId

AttemptId

TraceId
```

---

# 59. Module Dependencies

Required upstream semantic dependency:

```text
Text Processing
```

Supporting dependencies:

```text
Runtime

Provider Management

Knowledge

Artifact Store

Resource Manager

Event Bus

Logging

Telemetry
```

Contextual integration:

```text
Reading Session
```

Downstream semantic consumers:

```text
Reading Session

Presentation

Knowledge review workflows
```

---

# 60. Dependency Flow

Primary semantic flow:

```text
Text Processing
        ↓
SourceDocumentArtifact
        ↓
Translation
        ↓
Translation Candidate
        ↓
Runtime / Artifact Store
        ↓
Reading Session
        ↓
Presentation
```

Supporting flow:

```text
Knowledge ───────────────► Translation

Provider Management ────► Translation

Runtime ◄───────────────► Translation

Artifact Store ◄────────► Runtime

Reading Session ─────────► Translation Context
```

---

# 61. Public Contract Surface

The Translation contract exposes semantic operations rather than Job management.

Representative operations include:

```text
Translate

RequestRetranslation

SubmitTranslationCorrection
```

Supporting semantic queries may include:

```text
GetTranslationPlan

GetTranslationCandidate

GetTranslationVariant

ListTranslationVariants
```

Exact canonical surface is defined by:

```text
02-modules/translation/CONTRACT.md
```

README must not override CONTRACT.md.

---

# 62. Runtime Correlation

Translation operations may carry Runtime context:

```text
RevisionId

WorkItemId

AttemptId

TraceId

CancellationContext

DeadlineContext
```

These identifiers provide correlation.

They do not make Translation the owner of Runtime lifecycle.

---

# 63. Recommended Implementation Components

Possible conceptual components:

```text
TranslationIntentResolver

TranslationPlanBuilder

TranslationUnitPlanner

TranslationBatchPlanner

TranslationContextBuilder

TranslationTerminologyResolver

TranslationProviderRequirementResolver

TranslationProviderSelector

TranslationProviderAdapterRegistry

TranslationOutputParser

TranslationOutputValidator

TranslationAlignmentValidator

TranslationCandidateAssembler

TranslationCandidateValidator

TranslationVariantFactory

TranslationRetryHintEvaluator

TranslationErrorNormalizer

TranslationEventPublisher
```

These are conceptual responsibilities.

They are not mandatory class names.

---

# 64. Components That Should Not Exist as Translation Owners

Avoid introducing Translation-private infrastructure such as:

```text
TranslationScheduler

TranslationWorkerPool

TranslationRetryScheduler

TranslationAttemptManager

TranslationQueue

TranslationCancellationManager

TranslationArtifactPublisher

TranslationCacheManager
```

unless they are thin adapters to the corresponding platform/runtime capability.

They must not create parallel ownership.

---

# 65. Suggested Internal Folder Structure

A possible implementation structure:

```text
02-modules/translation/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
├── ERRORS.md
│
├── domain/
│   ├── intent/
│   ├── plan/
│   ├── unit/
│   ├── batch/
│   ├── candidate/
│   ├── variant/
│   ├── context/
│   ├── terminology/
│   ├── policies/
│   └── errors/
│
├── application/
│   ├── translation/
│   ├── retranslation/
│   ├── correction/
│   └── services/
│
├── providers/
│   ├── contracts/
│   ├── adapters/
│   └── selection/
│
├── validation/
├── alignment/
├── events/
└── observability/
```

This layout is illustrative.

---

# 66. MVP Scope

Translation MVP should support:

* Chinese → Vietnamese;
* English → Vietnamese;
* novel SourceDocuments;
* OCR-derived comic SourceDocuments;
* provider abstraction;
* at least one remote provider;
* multiple Translation Units per Batch;
* translation profiles;
* basic context;
* basic terminology constraints;
* provider fallback recommendations;
* Runtime retry integration;
* Runtime cancellation integration;
* partial Candidates;
* progressive comic translation;
* Candidate assembly;
* stale Candidate rejection by Runtime;
* immutable Translation variants;
* semantic Artifact reuse compatibility;
* normalized errors;
* warnings;
* semantic events;
* observability metadata.

---

# 67. MVP Does Not Require

The initial Translation implementation does not require:

* its own scheduler;
* its own Work Queue;
* its own worker pool;
* its own retry engine;
* its own cancellation engine;
* its own Artifact Store;
* its own Cache infrastructure;
* its own Provider credential store;
* active Translation variant management;
* visual text fitting.

---

# 68. Deferred Capabilities

Deferred capabilities include:

* simultaneous provider comparison;
* automatic quality scoring;
* Translation Memory;
* automatic glossary learning;
* automatic name discovery;
* advanced speaker inference;
* long-range novel memory;
* collaborative correction review;
* speculative parallel execution;
* advanced local-model management;
* provider benchmarking;
* automatic provider-quality routing;
* advanced prefetch translation.

All future capabilities must preserve source traceability.

---

# 69. Core Invariants

Translation must always preserve these rules.

1. Translation consumes stable semantic source content.

2. Translation does not consume canonical raw OCR as its primary source contract.

3. SourceDocumentArtifact is immutable.

4. SourceBlock identity belongs upstream.

5. TranslationUnit identity belongs to Translation.

6. Every TranslatedUnit maps to a valid TranslationUnit.

7. Every TranslationUnit preserves SourceBlock lineage.

8. Translation may restructure source content but never lose traceability.

9. TranslationBatch is a provider execution grouping.

10. TranslationBatch is not a Runtime Attempt.

11. Runtime owns Attempt lifecycle.

12. Runtime owns queue lifecycle.

13. Runtime owns retry execution.

14. Runtime owns cancellation.

15. Runtime owns execution deadlines.

16. Runtime owns stale-result authority.

17. Translation only provides RetryHint.

18. Provider-specific SDK types remain behind adapters.

19. Provider output is untrusted.

20. Provider success does not imply valid Translation output.

21. Output passes parsing before acceptance.

22. Output passes semantic validation before acceptance.

23. Output passes alignment validation before acceptance.

24. Unknown provider Unit IDs are never guessed.

25. Missing Units remain explicit.

26. Failed Units remain explicit.

27. Partial output is explicit.

28. EMPTY_VALID is valid.

29. Candidate VALID does not mean authoritative.

30. Candidate VALID does not mean published.

31. Artifact Store owns durable publication.

32. Translation does not own Cache infrastructure.

33. Cache reuse requires semantic compatibility.

34. Translation variants are immutable.

35. Reading Session owns active variant selection.

36. Knowledge owns persistent terminology/character knowledge.

37. Translation consumes immutable Knowledge context.

38. Provider Management owns provider lifecycle.

39. Provider Management owns credentials.

40. Privacy rules cannot be bypassed by provider selection.

41. LOCAL_ONLY cannot execute remotely.

42. Source content is untrusted data.

43. Provider output cannot control metadata or lifecycle.

44. Public errors are provider-neutral.

45. Public events are provider-neutral.

46. Credentials never enter public contracts.

47. Raw source/translated content is minimized in logs.

48. Translation-local states do not duplicate Runtime lifecycle.

49. Retry does not mutate failed Translation state backward.

50. README.md must not introduce rules contradicting canonical Translation documents.

---

# 70. Documentation Map

Translation documentation consists of:

```text
02-modules/translation/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md
```

---

# 71. README.md

Purpose:

```text
entry point

architectural overview

developer orientation

AI-agent orientation
```

Answers:

* What is Translation?
* Where does it sit?
* What are the main concepts?
* What does Translation own?
* What does Runtime own?
* What should I read next?

README is not the final authority for detailed contracts.

---

# 72. MODULE.md

Defines:

```text
architectural ownership

module boundary

responsibilities

non-responsibilities

cross-module relationships

architectural invariants
```

Use it when asking:

```text
Who owns this concern?
```

---

# 73. CONTRACT.md

Defines:

```text
public semantic inputs

public semantic outputs

Translation Intent

Translation Plan

Translation Units

Translation Batches

Candidate contracts

provider-neutral contracts

variant/correction contracts
```

Use it when asking:

```text
What data crosses the Translation boundary?
```

---

# 74. STATES.md

Defines Translation-local semantic state machines.

Use it when asking:

```text
What state may Plan, Batch or Candidate enter?
```

It does not redefine Runtime lifecycle.

---

# 75. EVENTS.md

Defines Translation-owned semantic facts.

Use it when asking:

```text
What Translation facts may be published?
```

It does not redefine Runtime lifecycle events.

---

# 76. ERRORS.md

Defines:

```text
Translation semantic errors

provider-boundary normalization

output validation errors

alignment errors

Candidate errors

privacy/security errors

warnings

RetryHint
```

Use it when asking:

```text
What failed inside Translation semantics?
```

---

# 77. Recommended Reading Order

For a new developer or AI agent:

```text
README.md
    ↓
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
EVENTS.md
    ↓
ERRORS.md
```

For architectural review:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
EVENTS.md
    ↓
ERRORS.md
    ↓
README.md consistency check
```

For implementation work:

```text
CONTRACT.md
    ↓
STATES.md
    ↓
ERRORS.md
    ↓
EVENTS.md
    ↓
MODULE.md invariants
```

---

# 78. Decision Authority

When Translation documentation appears inconsistent, use:

```text
Project-wide architecture rules
        ↓
Translation MODULE.md
        ↓
Translation CONTRACT.md
        ↓
Translation STATES.md
        ↓
Translation EVENTS.md
        ↓
Translation ERRORS.md
        ↓
Translation README.md
```

README summarizes the module.

It must not create contradictory architecture.

---

# 79. Cross-Module Authority

When ownership is unclear:

```text
semantic translation
    → Translation

execution lifecycle
    → Runtime

provider lifecycle
    → Provider Management

durable Artifact
    → Artifact Store

persistent terminology / character knowledge
    → Knowledge

reader-active selection
    → Reading Session

visual rendering
    → Presentation
```

---

# 80. Architecture Completion Status

Translation documentation set:

```text
[x] MODULE.md

[x] CONTRACT.md

[x] STATES.md

[x] EVENTS.md

[x] ERRORS.md

[x] README.md
```

The Translation architecture is sufficiently defined for:

* cross-module review;
* Provider Management integration;
* Runtime integration;
* Artifact Store integration;
* Knowledge integration;
* sequence-diagram creation;
* MVP technical planning;
* domain-model implementation;
* provider-adapter design;
* test planning.

It is not yet a complete implementation specification.

---

# 81. Next Architecture Work

After Translation, the next work should verify surrounding boundaries rather than expand Translation further.

Priority checks:

```text
Provider Management
    ↔ Translation provider requirements

Runtime
    ↔ Translation execution contract

Artifact Store
    ↔ Candidate publication

Knowledge
    ↔ Context / terminology snapshots

Reading Session
    ↔ active Translation selection

Presentation
    ↔ translated semantic Artifact consumption
```

Any inconsistency should be fixed at the owning boundary rather than by expanding Translation ownership.

---

# 82. Related Documents

```text
02-modules/translation/MODULE.md
02-modules/translation/CONTRACT.md
02-modules/translation/STATES.md
02-modules/translation/EVENTS.md
02-modules/translation/ERRORS.md

02-modules/text-processing/
02-modules/provider-management/
02-modules/knowledge/
02-modules/reading-session/
02-modules/presentation/

01-architecture/runtime/PIPELINE_RUNTIME.md
01-architecture/runtime/WORK_QUEUE.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

03-infrastructure/artifact-store/
03-infrastructure/resource-manager/
03-infrastructure/event-bus/
03-infrastructure/logging/
03-infrastructure/telemetry/
03-infrastructure/scheduler/
```

---

# 83. Quick Reference

```text
Input Artifact:
    SourceDocumentArtifact

Upstream semantic unit:
    SourceBlock

Translation semantic unit:
    TranslationUnit

Translation intent:
    TranslationIntent

Resolved semantic execution:
    TranslationPlan

Provider execution grouping:
    TranslationBatch

Validated translated unit:
    TranslatedUnit

Translation output:
    TranslationCandidate

Durable output:
    Translation Artifact

Immutable alternative:
    TranslationVariant

Execution work:
    Runtime WorkItem

Execution attempt:
    Runtime Attempt
```

---

# 84. Primary Flow

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
Validation
        ↓
Alignment
        ↓
TranslationCandidate
        ↓
Runtime
        ↓
Artifact Store
```

---

# 85. Retry Flow

```text
Runtime Attempt
        ↓
Translation
        ↓
retryable semantic/provider failure
        ↓
TranslationRetryHint
        ↓
Runtime Retry Policy
        ↓
new Runtime Attempt
```

---

# 86. Cancellation Flow

```text
Runtime
    ↓
Cancellation requested
    ↓
Translation observes cancellation
    ↓
unnecessary work stops where practical
    ↓
Runtime owns final cancellation outcome
```

---

# 87. Stale Candidate Flow

```text
TranslationCandidate VALID
        ↓
Runtime authority check
        ↓
current Revision?
    ├── yes → continue
    └── no  → REJECTED_STALE
```

No Translation failure is required.

---

# 88. Partial Flow

```text
TranslationUnits
    ├── valid
    ├── valid
    ├── failed
    └── missing
        ↓
PartialResultPolicy
        ↓
TranslationCandidate
Completeness = PARTIAL
```

Failed and missing Unit identities remain explicit.

---

# 89. Correction Flow

```text
Existing Translation Artifact
        ↓
Correction request
        ↓
Translation validates correction
        ↓
new immutable TranslationVariant
```

Reading Session decides whether that variant becomes active.

---

# 90. Summary

Translation transforms:

```text
stable semantic source
```

into:

```text
validated
aligned
provider-neutral
translated semantic output
```

Its core domain is:

```text
SourceDocumentArtifact
        ↓
TranslationIntent
        ↓
TranslationPlan
        ↓
TranslationUnit
        ↓
TranslationBatch
        ↓
Provider
        ↓
TranslatedUnit
        ↓
TranslationCandidate
```

The most important boundaries are:

```text
SourceBlock
    = upstream semantic source identity

TranslationUnit
    = Translation-owned semantic translation unit

TranslationBatch
    = provider execution grouping

Runtime Attempt
    = execution lifecycle attempt

TranslationCandidate
    = semantically valid Translation output

Translation Artifact
    = durable published output

TranslationVariant
    = immutable semantic alternative
```

And the central architecture rule is:

```text
Translation owns
what translated output means
and whether that output is valid.

Runtime owns
whether execution continues
and whether the result still matters.

Provider Management owns
the providers used to execute translation.

Artifact Store owns
durable publication.

Reading Session owns
which translation the reader currently uses.

Presentation owns
how translated content is rendered.
```
