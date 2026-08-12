# CRAI Translation Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/translate/TRANSLATION.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Architecture Owner:** Translation
> **Public Input:** `SourceDocumentArtifact`
> **Public Output:** `TranslationArtifact`
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-10

---

# 1. Purpose

The Translation Architecture defines how CRAI converts canonical source-language semantic content into target-language semantic content.

Translation consumes:

```text
Published SourceDocumentArtifact
```

and owns the transformation into:

```text
TranslationUnit
    ↓
TranslationBatch
    ↓
Translation Context Assembly
    ↓
Translation Candidate
    ↓
TranslationArtifact
```

The architecture must support:

```text
AI-based translation

traditional machine translation

local translation

cloud translation

hybrid provider strategies
```

without depending on one specific provider.

---

# 2. Central Architecture Rule

Translation owns:

```text
translation semantics
```

Runtime owns:

```text
translation execution
```

Therefore:

```text
Translation
    owns TranslationUnit
    owns TranslationBatch
    owns context assembly
    owns provider suitability semantics
    owns TranslationArtifact

Runtime
    owns WorkItem
    owns Attempt
    owns retry execution
    owns cancellation
    owns timeout/deadline
    owns supersession
```

---

# 3. Position in CRAI

Canonical source flow:

```text
RecognitionArtifact
       \
        \
         ↓
      Text Processing
         ↑
        /
       /
Structured Source
```

Text Processing publishes:

```text
SourceDocumentArtifact
```

Then:

```text
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
    ↓
PresentationArtifact
```

---

# 4. Translation Boundary

Translation begins at:

```text
Published SourceDocumentArtifact
```

Translation ends at:

```text
Published TranslationArtifact
```

Translation does not own:

```text
Capture

Recognition

SourceDocument reconstruction

Presentation layout

native UI rendering
```

---

# 5. Responsibilities

Translation owns:

```text
TranslationUnit construction

TranslationBatch construction

translation context assembly

translation strategy

translation instructions

provider suitability

provider request semantics

provider response normalization

source-target alignment

translation validation

translation warnings

translation corrections

TranslationArtifact semantics
```

---

# 6. Runtime-Related Non-Responsibilities

Translation does not own:

```text
RuntimeRevision

WorkItem lifecycle

Attempt lifecycle

global retry policy

retry scheduling

cancellation lifecycle

deadline enforcement

queueing

Scheduler behavior

Runtime backpressure

supersession
```

Translation cooperates with these mechanisms through Runtime contracts.

---

# 7. Other Non-Responsibilities

Translation does not own:

```text
screen capture

text detection

OCR

Reading Order authority

SourceDocument construction

native Presentation rendering

persistent general user preferences

physical storage implementation

Event Bus transport
```

---

# 8. Public Input

Translation consumes:

```text
SourceDocumentArtifact
```

not:

```text
raw OCR result

OCR Document

DOM Node

HTMLElement

provider-specific Recognition DTO

generic Text Segment array detached from provenance
```

---

# 9. SourceDocument Consumption

Translation may consume source structures such as:

```text
Section

Block

Paragraph

Sentence

Span

Annotation

Continuation

SourceReference

Language metadata
```

according to its TranslationUnit construction strategy.

---

# 10. TranslationUnit

`TranslationUnit` is the primary semantic unit owned by Translation.

Conceptually:

```text
TranslationUnit
├── TranslationUnitId
├── SourceReferences
├── SourceText
├── SourceLanguage
├── TranslationRole
├── SourceOrder
├── ContextReferences
├── Strategy
├── Constraints
└── Metadata
```

Exact contract belongs to the Translation module.

---

# 11. TranslationUnit vs Text Sentence

A Text Processing Sentence is:

```text
source-language linguistic structure
```

A TranslationUnit is:

```text
source content selected and packaged
for one translation-semantic operation
```

Therefore:

```text
Sentence
    ≠
TranslationUnit
```

---

# 12. One Sentence → Multiple TranslationUnits

Possible when:

```text
provider constraints require safe splitting

source contains independent translatable spans

different Translation strategies apply

only selected content should be translated
```

The relationship must preserve source references.

---

# 13. Multiple Sentences → One TranslationUnit

Possible when:

```text
dialogue continuity matters

contextual translation requires grouping

short related sentences belong together

a semantic unit spans multiple source nodes
```

Again, source identities remain traceable.

---

# 14. TranslationUnit Identity

Every TranslationUnit must have a stable semantic identity within the applicable source/configuration lineage.

Do not derive identity solely from:

```text
array position

provider request index

AttemptId

WorkItemId

RuntimeRevisionId

memory address
```

---

# 15. TranslationUnit Source Mapping

Every TranslationUnit must remain traceable to:

```text
SourceDocumentArtifact

SourceDocument Node(s)

source ranges
```

where applicable.

Translation must never detach translated output from its source semantic identity.

---

# 16. TranslationUnit Ordering

TranslationUnit order should preserve logical source order where order matters.

Parallel execution must not redefine semantic ordering.

---

# 17. Translation Role

Possible translation roles may include:

```text
novel_prose

dialogue

narration

caption

title

sound_effect

sign

annotation

generic
```

Exact enum belongs to Translation contracts.

---

# 18. Translation Strategy

Translation behavior may vary by role/content type.

Examples:

```text
novel

comic_dialogue

comic_caption

sound_effect

title

literal

natural

generic
```

Strategies configure Translation semantics.

They must not mutate SourceDocument truth.

---

# 19. Strategy Concerns

A Translation strategy may influence:

```text
tone

literalness

brevity

honorific treatment

name handling

terminology policy

pronoun behavior

SFX handling

output formatting constraints
```

---

# 20. Strategy vs Presentation

A strategy may request concise target wording.

It must not own:

```text
font size

bubble resizing

overlay placement

line wrapping

native rendering
```

Those belong to Presentation/UI.

---

# 21. TranslationBatch

`TranslationBatch` groups compatible TranslationUnits for efficient provider interaction.

Conceptually:

```text
TranslationBatch
├── TranslationBatchId
├── TranslationUnits[]
├── ContextSnapshot
├── TranslationStrategy
├── LanguagePair
├── ProviderRequirements?
├── BatchOrder
└── Semantic Constraints
```

---

# 22. TranslationBatch Is Translation-Owned

Text Processing does not create TranslationBatch.

Runtime does not define Translation semantic batching.

Translation creates batches according to:

```text
semantic coherence

provider capabilities

language pair

strategy

context needs

configured Translation policy
```

---

# 23. TranslationBatch vs Runtime WorkItem

These must remain distinct.

```text
TranslationBatch
    = Translation semantic/provider grouping

WorkItem
    = Runtime logical executable work
```

One WorkItem may process:

```text
one TranslationBatch
```

or another execution mapping defined by Runtime integration.

The concepts are not interchangeable.

---

# 24. TranslationBatch vs Attempt

```text
TranslationBatch
    ≠
Attempt
```

A TranslationBatch may be executed by:

```text
Attempt T1
```

and, after retry/fallback:

```text
Attempt T2
```

without changing the TranslationBatch's semantic purpose.

---

# 25. Batch Compatibility

TranslationUnits may be grouped when compatible in terms of:

```text
source language

target language

strategy

context scope

privacy policy

provider capability

structured-output requirement

user translation configuration
```

---

# 26. Batch Size

Batch sizing should balance:

```text
semantic coherence

context continuity

provider limits

response reliability

latency

cost

cancellation responsiveness
```

Translation owns the semantic/provider-aware batching policy.

Runtime owns execution scheduling.

---

# 27. Provider Limits

Provider capability may constrain:

```text
maximum characters

maximum tokens

maximum items

structured output support

batch support
```

Translation adapts TranslationBatch construction accordingly.

This does not change source-language segmentation authority.

---

# 28. Safe Unit Splitting

If one TranslationUnit exceeds provider constraints, Translation may create safe derived subunits when necessary.

Requirements:

```text
preserve original TranslationUnit relationship

preserve source mapping

record derived/split provenance

reconstruct one coherent translated semantic result
```

---

# 29. Translation Context

Translation owns Translation-specific context assembly.

Context may include:

```text
neighbor source nodes

previous TranslationUnits

previous translated output where allowed

character information

glossary entries

terminology

chapter metadata

speaker hints

style profile

user translation preferences
```

---

# 30. Context Is Not Target

The provider request must distinguish:

```text
Content To Translate
```

from:

```text
Context For Understanding
```

Supporting context must not be returned as additional translated target content.

---

# 31. Context Source

Context may come from:

```text
SourceDocumentArtifact

previous TranslationArtifact

Glossary

Translation Memory

Knowledge

Reading Session context

Application-provided safe context
```

through explicit contracts.

---

# 32. Context Snapshot

Provider execution should use an immutable:

```text
TranslationContextSnapshot
```

or equivalent.

A Translation Attempt should not read mutable global context during execution.

---

# 33. Context Versioning

Context compatibility may depend on:

```text
SourceDocumentArtifact identity

GlossaryRevision

TranslationMemoryRevision

character/knowledge revision

Translation configuration revision
```

Avoid one generic:

```text
contextVersion
```

when typed revisions are available.

---

# 34. No Standalone Context Authority

The old architecture placed:

```text
Translation Context
    ↓
Translation
```

as though an upstream Context layer owned the final prepared translation context.

Current architecture:

```text
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationUnit
    ↓
Context Assembly
```

Translation owns this concern.

---

# 35. Translation Planning

Translation may create a semantic:

```text
TranslationPlan
```

describing how current source content should be translated.

It may determine:

```text
TranslationUnits

TranslationBatches

strategy

provider suitability requirements

context snapshots

validation strategy

structured-output expectations
```

---

# 36. TranslationPlan Is Not RuntimeRevision

```text
TranslationPlan
    = Translation semantic plan

RuntimeRevision
    = current Runtime execution authority
```

TranslationPlan must not own:

```text
global queue state

Attempt lifecycle

retry timer

cancellation state
```

---

# 37. TranslationPlan Determinism

Given equivalent:

```text
SourceDocumentArtifact

Translation configuration

Glossary/Knowledge snapshots

provider capability set
```

planning should be deterministic where deterministic rules apply.

Provider health/availability may create controlled differences where explicitly included in planning inputs.

---

# 38. Provider Abstraction

Translation providers expose a provider-neutral Translation contract.

Conceptually:

```text
Translation
    ↓
TranslationProviderPort
    ↓
Provider Adapter
    ↓
Provider API / Local Engine
```

---

# 39. Provider Implementations

Possible provider types:

```text
local model

cloud AI model

machine translation API

user-configured AI service

offline translation engine
```

Provider-specific behavior remains inside adapters.

---

# 40. Provider DTO Isolation

Never expose provider-native types through canonical Translation contracts.

Examples:

```text
OpenAI response objects

DeepL DTOs

Google Translation response

local model tensor/result structures
```

Adapters normalize them first.

---

# 41. Provider Capabilities

Provider capability information may include:

```text
supported languages

structured output

streaming

glossary support

batch support

cancellation support

maximum input size

privacy characteristics

local/cloud mode
```

---

# 42. Provider Suitability

Translation determines semantic suitability requirements.

Possible inputs:

```text
language pair

Translation strategy

privacy requirement

content role

structured-output need

request size

quality requirement

local-only requirement
```

---

# 43. Provider Selection Boundary

Cross-provider discovery/availability may belong to Provider Management.

Translation owns:

```text
whether a provider is semantically suitable
```

Runtime/provider policy owns:

```text
which provider execution Attempt occurs
```

according to the integrated architecture.

---

# 44. Explicit User Provider Selection

An explicit user provider choice may constrain eligible providers.

If the provider is incompatible:

```text
do not silently violate the requested policy
```

Application may surface an error or fallback option.

---

# 45. Local-Only Policy

A local-only request must never silently execute against a cloud provider.

This invariant applies regardless of retry or fallback.

---

# 46. Privacy-Constrained Provider Selection

Provider eligibility must consider:

```text
whether source content may leave device

whether contextual data may be sent

whether the provider satisfies configured privacy constraints
```

---

# 47. Provider Request Mapping

Provider adapters transform:

```text
TranslationBatch
+
Context Snapshot
+
Translation Rules
```

into provider-native requests.

---

# 48. Provider Request Contents

A request may contain:

```text
target units

source language

target language

Translation strategy

supporting context

Glossary entries

character information

structured-output schema

system-controlled instructions
```

---

# 49. Prompt / Instruction Separation

For AI providers, distinguish:

```text
system-controlled translation rules
```

from:

```text
untrusted source content
```

Source text must not be allowed to override:

```text
privacy rules

provider configuration

output schema

target boundaries

security constraints
```

---

# 50. Structured Output

Structured provider output is preferred when available.

Conceptually:

```text
TranslationUnitId
    ↓
TranslatedText
```

Identifier-based mapping is more reliable than positional/free-form parsing.

---

# 51. Structured Response Requirements

Responses should preserve:

```text
TranslationUnitId

translated content

unit-level warnings/status where applicable
```

Context-only content must not become target output.

---

# 52. Free-Form Response

When a provider cannot return structured output:

```text
Provider Adapter
    ↓
Controlled Parser
    ↓
Canonical Provider Result
```

Validation requirements become stricter.

---

# 53. Response Parsing

Provider adapter parsing should verify:

```text
response shape

expected TranslationUnit identities

output count

duplicate results

missing results

unexpected context output

empty translations

provider-specific failure markers
```

---

# 54. Unknown Output

Unknown or ambiguous provider output must not be silently published.

It should result in:

```text
validation failure

degraded result

retryable classification
```

according to Translation contracts.

---

# 55. Canonical Provider Result

Provider adapter output should be provider-neutral.

Conceptually:

```text
CanonicalProviderTranslationResult
├── UnitResults[]
├── ProviderInfo
├── Usage?
├── Warnings[]
└── SafeMetadata
```

This remains an internal Translation/provider boundary.

---

# 56. Translation Candidate

After semantic normalization and validation, Translation produces:

```text
TranslationArtifact Candidate
```

A Candidate is not current authoritative Translation output.

---

# 57. TranslationArtifact

`TranslationArtifact` is the immutable public semantic result owned by Translation.

Conceptually it may contain:

```text
ArtifactId

SourceDocumentArtifactRef

source language

target language

translated units

source-target alignment

Translation strategy

semantic warnings

provider provenance

configuration provenance

correction provenance
```

Exact schema belongs to the Translation module contract.

---

# 58. Translated Unit

A translated unit may contain:

```text
TranslationUnitId

SourceReferences

SourceText reference/snapshot

TranslatedText

TranslationRole

SourceOrder

Status

Warnings

Provenance
```

---

# 59. Source-Target Mapping

Translated content must preserve alignment with source semantic content.

Preferred mapping:

```text
TranslationUnitId
```

plus source references.

Avoid relying solely on:

```text
array position
```

---

# 60. One-to-Many Alignment

Translation may map:

```text
one source semantic unit
    ↓
multiple target fragments
```

where the target language requires structural expansion.

The TranslationArtifact should preserve semantic relationship explicitly.

---

# 61. Many-to-One Alignment

Translation may combine:

```text
multiple source Sentences
    ↓
one coherent target unit
```

when the TranslationUnit intentionally groups them.

Source traceability must remain intact.

---

# 62. Order Preservation

Final translated semantic ordering must follow source/TranslationUnit order.

Never use provider completion order as semantic output order.

---

# 63. Result Validation

Before creating a valid Candidate, Translation should validate:

```text
all required target units accounted for

returned identities known

no duplicate identities

translated content valid

source/context not accidentally emitted

required terminology constraints considered

mapping integrity

output size sanity

language/output sanity where useful
```

---

# 64. Validation Is Not Literary Truth

Translation validation detects structural and obvious semantic failures.

It must not pretend to produce an objective literary quality score without a defined measurement method.

---

# 65. Source Equality

Target text may legitimately equal source text when:

```text
proper name

number

symbol

punctuation

already-target-language content

intentional preservation
```

Equality is not automatically a failure.

---

# 66. Quality Signals

Non-authoritative quality signals may include:

```text
missing glossary term

unexpected source equality

suspicious expansion

suspicious shortening

mixed-language output

unresolved name

provider parser fallback

high correction frequency
```

These may generate warnings.

---

# 67. Uncertainty

Translation must preserve uncertainty when it cannot confidently resolve:

```text
speaker

gender

pronoun

name

title

ambiguous terminology
```

Avoid fabricating certainty merely to produce fluent target text.

---

# 68. Chinese → Vietnamese Focus

Initial CRAI translation priorities include Chinese-to-Vietnamese content.

Important concerns include:

```text
Simplified Chinese

Traditional Chinese

character names

Sino-Vietnamese terminology

pronouns

kinship/social titles

cultivation levels

martial arts techniques

sect/organization names

historical titles

idioms

internet slang

omitted subjects

context-dependent gender
```

---

# 69. Chinese Name Consistency

Names should remain consistent across:

```text
paragraphs

dialogue turns

chapters

comic panels
```

when adequate context/Glossary/Translation Memory exists.

Uncertain names should not be silently normalized into unstable variants.

---

# 70. Chinese Pronouns

Chinese frequently omits information needed for natural Vietnamese pronouns.

Translation may use:

```text
neighbor context

speaker hints

character knowledge

previous Translation

user terminology
```

but should preserve uncertainty when evidence remains insufficient.

---

# 71. Titles and Social Relations

Vietnamese target output may require contextual forms such as:

```text
huynh / đệ / sư huynh / sư tỷ

cha / phụ thân

ngài / hắn / y / cô ấy

bệ hạ / điện hạ

trưởng lão
```

Choice should be context-sensitive rather than a simple word replacement.

---

# 72. Cultivation / Fantasy Terminology

Translation should use glossary/knowledge-aware terminology for recurring concepts such as:

```text
cultivation levels

sects

techniques

artifacts

realms

ranks
```

Consistency may be more important than isolated literal translation.

---

# 73. Novel Translation

Novel Translation should prioritize:

```text
paragraph continuity

narrative viewpoint

character voice

dialogue relationships

terminology consistency

natural Vietnamese flow

source paragraph identity
```

---

# 74. Novel Context

Useful novel context may include:

```text
previous Paragraphs

current scene

speaker candidates

chapter title

Glossary

previous TranslationUnits

character information
```

Context should remain bounded.

---

# 75. Long Chapters

Long chapters should support:

```text
incremental TranslationUnit construction

bounded TranslationBatches

current-reading priority

limited prefetch
```

Do not require translation of an entire book/chapter before useful output appears.

---

# 76. Comic Translation

Comic Translation should prioritize:

```text
dialogue tone

brevity

character voice

Bubble relationships

Panel/source order

caption clarity

SFX policy

available presentation constraints
```

---

# 77. Presentation Constraint Input

Translation may receive semantic constraints such as:

```text
prefer concise output

desired target length range

content role
```

when explicitly supported.

It must not manipulate UI geometry directly.

---

# 78. Meaning Preservation vs Fit

A shorter target translation should not remove essential semantic content merely to fit an overlay.

When content cannot fit safely:

```text
Translation
    may expose warning/alternative

Presentation
    handles layout degradation/fallback
```

---

# 79. SFX Translation

Possible SFX strategies:

```text
preserve

translate

transliterate

translate + explain

hide from target output
```

The selected behavior is Translation policy.

Presentation determines how the chosen semantic result is shown.

---

# 80. Translation Preferences

Persistent user preferences belong to:

```text
Preferences
```

Translation consumes an effective immutable configuration snapshot.

---

# 81. Translation Configuration

Possible semantics include:

```text
literal / balanced / natural

preserve names

preserve honorifics

translate SFX

Vietnamese pronoun policy

custom terminology

custom Translation instructions
```

---

# 82. Free-Form Instructions

Free-form custom instructions may be supported.

They should be treated as:

```text
controlled optional extension
```

not the only source of Translation behavior.

Structured configuration is preferred for stable semantics.

---

# 83. Configuration Provenance

Translation output should record semantically relevant configuration identity/revision sufficient for:

```text
reproducibility

cache compatibility

correction comparison

diagnostics
```

---

# 84. Glossary

Translation may consume Glossary entries.

Glossary is:

```text
contextual terminology authority/input
```

not a string-replacement pass after Translation.

---

# 85. Glossary Enforcement

Possible policies:

```text
required

preferred

advisory

contextual
```

Translation validation may warn when required terminology is violated.

---

# 86. Translation Memory

Translation Memory stores reusable source-target semantic pairs.

It may assist:

```text
repeated dialogue

recurring titles

known terminology

previously translated passages

interface text
```

---

# 87. Translation Memory Is Candidate Knowledge

A Translation Memory entry is not automatically current truth.

Reuse requires compatibility with:

```text
current source

current context

Glossary

Translation configuration

language pair

semantic role
```

---

# 88. Translation Memory Ownership

Whether Translation Memory becomes:

```text
Translation subdomain
```

or:

```text
future Knowledge module
```

remains an architecture decision.

Translation consumes it through an explicit contract.

---

# 89. User Translation Corrections

Users may modify translated output.

A correction must create a new semantic Translation revision/result.

Do not mutate provider output silently in place.

---

# 90. User Authority

Explicit confirmed user translation should take precedence over automatic output for the compatible semantic scope until:

```text
user removes it

source becomes incompatible

correction is explicitly replaced
```

---

# 91. Translation Correction Provenance

A correction should record:

```text
target TranslationUnit/source scope

previous translation

corrected translation

actor

timestamp

base Artifact/revision

reason/status
```

---

# 92. Source Correction vs Translation Correction

These are distinct.

```text
Source correction
    → Text Processing / source semantics

Translation correction
    → Translation semantics
```

A source correction may invalidate a Translation correction if its source compatibility no longer holds.

---

# 93. Cache

Translation results may be cached.

Cache compatibility may depend on:

```text
SourceDocument semantic fingerprint

TranslationUnit identity/content

source language

target language

Translation strategy

Glossary revision

Translation configuration revision

context compatibility

provider/model profile where required

Translation engine/version
```

---

# 94. Provider-Agnostic Cache

Some cache entries may be reusable regardless of provider.

Others may intentionally include provider/model in compatibility for:

```text
reproducibility

quality preference

explicit provider choice
```

This is Translation cache policy.

---

# 95. Manual Correction Cache Separation

Confirmed manual Translation corrections should not be treated as ordinary provider cache entries.

They have stronger user authority and distinct provenance.

---

# 96. Cache Is Not Authority

A Translation cache hit still requires:

```text
semantic compatibility
+
current Runtime authority
```

before becoming current Published Translation output.

---

# 97. Runtime Execution

Runtime executes Translation work.

Conceptually:

```text
Translation WorkItem
    ↓
Attempt
    ↓
Translation operation
    ↓
TranslationArtifact Candidate
```

---

# 98. Attempt

Attempt belongs to Runtime.

One semantic Translation operation may experience:

```text
Attempt T1
    ↓ failure
Attempt T2
    ↓ success
```

without Translation inventing its own execution-attempt authority.

---

# 99. Retry Classification

Translation may classify failures as:

```text
retryable

non-retryable

provider-specific

semantic-invalid

configuration-invalid

provider-incompatible
```

Runtime decides whether another Attempt is created.

---

# 100. Retry Execution

Deprecated:

```text
Translation
    retries with exponential backoff
```

Current:

```text
Translation Attempt
    ↓
error classification
    ↓
Runtime Retry Policy
    ↓
new Attempt
```

---

# 101. Retry Identity

A retry creates:

```text
new AttemptId
```

It does not merely increment a Translation-owned:

```text
attemptCount
```

as execution authority.

Attempt count may still appear as derived provenance/diagnostic metadata.

---

# 102. Timeout

Translation may provide:

```text
provider expected latency

provider-supported timeout constraints

semantic execution hints
```

Runtime owns the effective deadline/timeout behavior.

---

# 103. Cancellation

Translation must cooperate with Runtime cancellation.

Provider adapter should cancel/abort provider work when supported.

Translation does not commit Runtime cancellation state.

---

# 104. Cancellation Propagation

Conceptually:

```text
Runtime Attempt cancelled
    ↓
Translation execution context observes cancellation
    ↓
Provider Adapter receives cancellation signal
```

---

# 105. Cancelled Provider Response

A provider may still return after cancellation.

The response may be parsed/observed as needed, but it must not regain current publication authority.

---

# 106. Supersession

Newer content may supersede old Translation execution.

Example:

```text
RuntimeRevision R10
Translation Attempt running
    ↓
new content
    ↓
RuntimeRevision R11 current
```

R10 output cannot become current under R11.

---

# 107. Stale Result Protection

Do not rely on generic:

```text
requestRevision
segmentRevision
contextVersion
```

alone.

Use typed authority/provenance such as:

```text
SessionId where relevant

ReadingContextRevision where relevant

RuntimeRevisionId

SourceDocumentArtifactId

TranslationUnit identity

configuration/knowledge revisions
```

according to the contract.

---

# 108. Candidate Publication

Canonical flow:

```text
Attempt completes
    ↓
Provider result normalized
    ↓
Translation semantic validation
    ↓
TranslationArtifact Candidate
    ↓
Runtime/current-authority validation
    ↓
Published TranslationArtifact
```

---

# 109. Execution Success ≠ Publication

A provider may return a perfectly valid translation after its work has become obsolete.

Therefore:

```text
provider success
    ≠
current Translation authority
```

---

# 110. Partial Results

Provider execution may return partial semantic success.

Examples:

```text
9 TranslationUnits succeeded
1 failed
```

Translation must represent which units are valid.

---

# 111. Partial Publication Is Explicit

Whether successful units may become a Published partial TranslationArtifact is a Translation contract decision.

Do not infer partial publication simply because some provider results succeeded.

---

# 112. Partial TranslationArtifact

If supported, partial artifacts must explicitly identify:

```text
translated units

missing units

failed units

completeness

warnings

source alignment
```

Presentation must be able to distinguish them safely.

---

# 113. Retry of Failed Partial Units

If partial publication is allowed:

```text
failed TranslationUnits
```

may produce new Runtime work.

Retry execution still uses new Attempts.

---

# 114. Streaming

Provider streaming is an execution capability.

Streaming chunks are:

```text
provisional provider output
```

not automatically TranslationArtifact authority.

---

# 115. Streaming Assembly

Translation determines how stream chunks contribute to:

```text
provisional unit output
```

and when a semantic Candidate becomes valid.

---

# 116. Streaming to Presentation

Presentation may show provisional Translation only if CRAI explicitly defines:

```text
provisional Translation semantic contract
```

Do not expose raw provider token streams directly as authoritative translated text.

---

# 117. Sequential Translation

Some Translation semantic dependencies may favor sequential execution.

Examples:

```text
pronoun-sensitive dialogue

terminology established by previous units

long connected conversation

progressive context generation
```

TranslationPlan may declare these semantic dependencies.

Runtime executes them.

---

# 118. Parallel Translation

Independent TranslationUnits/Batches may execute concurrently where:

```text
semantic dependency allows

provider supports it

Runtime admits resources
```

Final semantic output order remains independent of completion order.

---

# 119. Runtime Scheduling Boundary

Translation may express:

```text
dependencies

priority hints

cost hints

provider requirements
```

through contracts.

Runtime owns actual:

```text
queue placement

concurrency

resource admission

execution timing
```

---

# 120. Provider Fallback

Provider fallback involves multiple authorities.

```text
Translation
    → semantic suitability / error classification

Provider Management
    → eligible provider availability

Runtime
    → next Attempt
```

---

# 121. Fallback Is Not Hidden Retry

Changing provider should be explicit in Attempt provenance.

Example:

```text
Attempt T1
Provider A

Attempt T2
Provider B
```

---

# 122. Fallback Constraints

Fallback must preserve:

```text
language compatibility

Translation strategy support

privacy policy

local-only policy

explicit user provider restrictions
```

---

# 123. Provider Provenance

TranslationArtifact may record semantic execution provenance such as:

```text
provider ID

model/engine ID

fallback used?

strategy

Translation configuration revision
```

Attempt-level timing/retry details remain primarily Runtime/Diagnostics provenance.

---

# 124. Usage

Provider usage may include:

```text
input characters

output characters

input tokens

output tokens

estimated cost

confirmed cost where available
```

Usage metadata is optional.

Translation success must not depend on providers exposing usage data.

---

# 125. Cost

Cost may influence:

```text
provider suitability

batching

prefetch decisions

Application/Runtime policy
```

but Translation quality/correctness boundaries remain unchanged.

---

# 126. Cost Metadata

Estimated cost must be identified as estimated unless confirmed by the provider.

Do not mix billing estimates with authoritative provider invoices.

---

# 127. Error Ownership

Exact Translation errors belong to:

```text
02-modules/translation/ERRORS.md
```

This architecture file defines conceptual categories only.

---

# 128. Translation Error Categories

Examples:

```text
invalid Translation input

empty source scope

unsupported language pair

no semantically compatible provider

provider response invalid

TranslationUnit mapping failure

Translation validation failure

Glossary constraint failure

Translation configuration invalid
```

---

# 129. Runtime Error Separation

Do not redefine Translation-owned equivalents of:

```text
Attempt timed out

Attempt cancelled

retry exhausted

Runtime superseded
```

Those remain Runtime semantics.

Translation errors may remain causal information attached to Runtime execution outcomes.

---

# 130. Provider Error Normalization

Provider-native errors should be normalized into:

```text
Translation/provider semantic classifications
```

without leaking:

```text
SDK exception classes

raw secrets

raw HTTP objects
```

across the module boundary.

---

# 131. Authentication Failure

Provider authentication failure is usually:

```text
non-retryable for the same credential/configuration
```

Translation/provider management classifies it.

Runtime determines execution consequence.

---

# 132. Rate Limiting

Provider rate limiting may be:

```text
retryable
```

subject to:

```text
Retry-After

current usefulness

deadline

provider alternatives

cost policy
```

Runtime owns actual retry scheduling.

---

# 133. Oversized Request

If a TranslationBatch exceeds a provider hard limit:

```text
Translation
```

may:

```text
rebuild safer batches

choose another compatible provider

return semantic/configuration failure
```

Do not ask Runtime to split linguistic source content arbitrarily.

---

# 134. Events

Exact Translation events belong to:

```text
02-modules/translation/EVENTS.md
```

This architecture document does not define a competing event catalog.

---

# 135. Deprecated Translation Execution Events

Do not use Event Bus as execution lifecycle with:

```text
TranslationRequested

TranslationPlanningStarted

TranslationBatchStarted

TranslationRetryScheduled

TranslationCancelled
```

for orchestration.

Commands/Runtime contracts control execution.

---

# 136. Valid Translation Facts

A committed module fact might conceptually describe:

```text
TranslationArtifactPublished

TranslationCorrectionApplied
```

if such events are defined by Translation module contracts.

The exact event names remain module-owned.

---

# 137. Event Bus Is Not Retry/Fallback Control

Forbidden:

```text
TranslationFailed
    ↓
RetryRequested event
```

or:

```text
ProviderFailed
    ↓
FallbackRequested event
```

as execution authority.

Runtime/provider contracts handle those decisions explicitly.

---

# 138. Progress

Execution progress should primarily be represented through:

```text
Runtime observability

Application projection

provider stream/progress adapters
```

rather than a large global Event Bus progress stream.

---

# 139. Diagnostics

Useful Translation diagnostics include:

```text
TranslationUnit count

TranslationBatch count

provider latency

validation warnings

provider fallback

cache reuse

partial result rate

late result rejection

manual correction frequency

Glossary violation warnings

estimated token/cost usage
```

---

# 140. Metrics

Possible measurements:

```text
Translation latency

time to first provisional output

time to Published TranslationArtifact

provider failure rate

semantic validation failure rate

cache hit rate

fallback frequency

TranslationUnit size distribution

batch size distribution

correction rate
```

Exact metric names belong to Diagnostics/Telemetry.

---

# 141. Privacy

Translation content may contain:

```text
private reading content

copyrighted material

personal documents

clipboard text

authenticated-site text
```

Default principles:

```text
send only required text/context

avoid raw text logging

respect local-only mode

expose cloud-provider use

minimize context

protect credentials

support cache deletion/retention policy
```

---

# 142. Provider Payload Minimization

Translation provider should receive:

```text
target source content

only context required for translation
```

It should not receive:

```text
entire screenshot

unrelated chapter history

browser cookies

credentials

full ReadingContext
```

unless a separately defined capability explicitly requires it.

---

# 143. Security

All source/context text is untrusted input.

AI provider requests must ensure source content cannot override:

```text
system Translation instructions

privacy rules

security constraints

structured output contract

TranslationUnit boundaries
```

---

# 144. Provider Credentials

Credentials belong to:

```text
Secret Management / Provider Adapter
```

and never enter:

```text
SourceDocumentArtifact

TranslationUnit semantic text

TranslationArtifact

Event payload

ViewModel
```

---

# 145. Performance

Interactive Translation should prioritize:

```text
current visible content

bounded batch size

cancellation responsiveness

cache reuse

limited provider concurrency

safe streaming where supported
```

---

# 146. Current vs Prefetch

Suggested semantic priority:

```text
current requested content
    ↓
explicit user retranslation
    ↓
near-future prefetch
    ↓
background content
```

Runtime determines actual scheduling.

---

# 147. Prefetch

Translation may plan speculative units for likely near-future reading.

Prefetch output is not current presentation authority until the relevant source becomes current.

---

# 148. Long Novel Performance

For long novels:

```text
bounded current reading window

bounded context window

bounded TranslationBatch

bounded prefetch
```

should be preferred over whole-book eager Translation.

---

# 149. Comics Performance

For comics:

```text
current Bubble/Panel content
```

should be prioritized.

Rapid scroll/page change should supersede obsolete Translation work.

---

# 150. Deterministic Semantics

Given equivalent:

```text
SourceDocumentArtifact

Translation configuration

Glossary/Knowledge inputs

provider capability assumptions
```

Translation planning should be deterministic where rules are deterministic.

Provider generation itself may remain nondeterministic.

---

# 151. Reproducibility

Useful provenance may include:

```text
provider

model

strategy

Glossary revision

Translation configuration revision

Translation engine/prompt version
```

for comparing outputs and reproducing behavior where possible.

---

# 152. Model Temperature / Sampling

For AI providers, generation settings are provider-adapter/configuration concerns.

If they materially affect semantic reproducibility/cache compatibility, their normalized profile/version should be included in Translation provenance.

---

# 153. Provider Prompt Version

Prompt/system-instruction version may affect Translation semantics.

When relevant it should participate in:

```text
reproducibility

cache compatibility

diagnostics
```

without leaking the raw internal prompt through public Artifacts.

---

# 154. Presentation Compatibility

TranslationArtifact must expose enough semantic information for Presentation to determine:

```text
target text

source alignment

content role

warnings

partial availability

semantic alternatives where supported
```

---

# 155. Presentation Must Not Re-Translate

Presentation must not:

```text
rewrite target semantics

perform provider Translation

resolve terminology independently
```

It may perform presentation-safe fitting/degradation without becoming Translation authority.

---

# 156. Translation Alternatives

Future Translation may expose:

```text
primary translation

alternative translations

literal gloss

uncertainty alternatives
```

This requires explicit TranslationArtifact semantics.

Presentation/UI may choose how alternatives are shown.

---

# 157. Translation Versioning

Distinguish:

```text
TranslationArtifact schema version

Translation semantic revision

Translation configuration revision

Glossary revision

provider/model version

RuntimeRevisionId
```

These are not one universal `translationVersion`.

---

# 158. Semantic Revision

A new Translation semantic revision may be created when:

```text
translated text changes

manual correction is applied

Translation strategy changes

retranslation produces a newly accepted semantic result
```

Exact revision model belongs to Translation contracts.

---

# 159. RuntimeRevision Independence

A new RuntimeRevision does not necessarily mean Translation semantics changed.

RuntimeRevision represents execution authority.

Translation revision represents target-language semantic authority.

---

# 160. TranslationArtifact Immutability

A Published TranslationArtifact is immutable.

Do not mutate it when:

```text
user corrects Translation

provider retranslates

Glossary changes

new strategy is selected
```

Produce a new semantic Candidate/Artifact revision.

---

# 161. Artifact Provenance

TranslationArtifact should preserve enough information to determine:

```text
which SourceDocumentArtifact produced it

which Translation configuration applied

which semantic source units were translated

which provider/model produced automatic output

which user corrections were applied
```

---

# 162. Stale Source Correction

Example:

```text
SourceDocumentArtifact D1
    ↓
Translation Attempt
    ↓
user corrects source
    ↓
SourceDocumentArtifact D2 current
    ↓
old D1 Translation returns
```

The D1 Translation Candidate must not replace current D2 translation authority.

---

# 163. Stale Context

Even when source text is identical, a Candidate may be incompatible because:

```text
Glossary changed

character knowledge changed

Translation configuration changed

manual override changed
```

Compatibility checks must use relevant typed provenance.

---

# 164. Structured Text Flow

For novels/web text:

```text
Structured Source
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
```

Capture/Recognition are skipped.

Translation behavior remains identical downstream.

---

# 165. Screen Comic Flow

For comic images:

```text
CaptureArtifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
Translation
```

Translation does not need provider-specific OCR information.

---

# 166. Cross-Source Consistency

Translation contracts should remain the same whether the SourceDocument originated from:

```text
OCR

DOM

plain text

clipboard

document parser
```

except for optional source-specific semantic metadata.

---

# 167. Testing Strategy

Unit/integration coverage should include:

```text
Chinese → Vietnamese novel

Chinese → Vietnamese comic dialogue

Traditional Chinese

mixed-language input

names/titles

pronoun ambiguity

Glossary constraints

structured provider output

free-form provider output

partial provider result

manual correction

provider fallback

stale result

cache reuse

local-only policy
```

---

# 168. TranslationUnit Tests

Verify:

```text
stable source mapping

correct source order

multi-Sentence grouping

safe splitting

context isolation

target/context separation

identity stability
```

---

# 169. Batch Tests

Verify:

```text
compatible grouping

provider-size constraints

order preservation

different strategy separation

context preservation

batch regeneration when provider changes
```

---

# 170. Provider Adapter Tests

Verify:

```text
native request mapping

structured response parsing

missing TranslationUnit IDs

duplicate IDs

unexpected output

provider errors

cancellation cooperation

secret redaction
```

---

# 171. Validation Tests

Verify:

```text
missing target

extra target

empty output

context leakage

source-target swap

Glossary warning

mixed-language anomalies

partial result semantics
```

---

# 172. Runtime Integration Tests

Verify:

```text
Attempt failure classification

Runtime retry → new Attempt

provider fallback → new Attempt

cancellation

supersession

late result rejection

Candidate → Published authority
```

---

# 173. Correction Tests

Verify:

```text
provider Translation remains immutable

manual correction creates new semantic revision

manual correction precedence

source change invalidates incompatible correction

reset restores automatic authority according to policy
```

---

# 174. MVP Scope

Recommended MVP Translation support:

```text
Chinese → Vietnamese

provider-independent Translation contracts

one usable local or cloud provider

TranslationUnit construction

basic TranslationBatch construction

bounded context assembly

stable source-target identity

structured-output validation where possible

Runtime cancellation cooperation

Runtime retry classification

stale-result protection

basic Translation cache

manual Translation editing
```

---

# 175. MVP Does Not Require

```text
automatic provider benchmarking

advanced Translation Memory

objective quality scoring

character relationship inference

multi-provider consensus

automatic Glossary learning

cross-book retrieval

adaptive AI provider routing

automatic translation alternatives
```

---

# 176. Future Extensions

Possible future capabilities:

```text
multiple provider routing

local-first hybrid Translation

provider quality comparison

semantic Translation Memory

style-preserving Translation

character-specific language profiles

automatic Glossary suggestions

translation alternatives

AI-assisted revision

quality estimation

cross-chapter terminology retrieval

adaptive batching

cost-aware provider selection

offline model packages
```

---

# 177. Future Features Must Preserve Core Boundaries

Extensions must not invalidate:

```text
SourceDocumentArtifact input

TranslationUnit ownership

TranslationArtifact output

provider adapter isolation

Runtime Attempt ownership

Candidate → Published authority
```

---

# 178. Architecture Decision — Translation Owns TranslationUnit

Text Processing supplies source-language semantic structure.

Translation decides which source structures become translation-semantic units.

---

# 179. Architecture Decision — Translation Owns Context Assembly

There is no upstream standalone Translation Context authority.

Translation builds context from owner-approved inputs.

---

# 180. Architecture Decision — TranslationBatch Is Semantic/Provider Grouping

TranslationBatch belongs to Translation.

Runtime WorkItem/Attempt remains execution identity.

---

# 181. Architecture Decision — Runtime Owns Retry

Translation classifies failures.

Runtime creates the next Attempt.

---

# 182. Architecture Decision — Runtime Owns Cancellation

Translation/provider adapters cooperate with cancellation.

They do not commit Runtime cancellation state.

---

# 183. Architecture Decision — Runtime Owns Timeout/Deadline

Translation may expose provider-specific execution constraints.

Runtime owns effective execution deadline.

---

# 184. Architecture Decision — Provider Output Is Provisional

Provider success creates provider output.

It does not automatically create Published Translation authority.

---

# 185. Architecture Decision — Validation Precedes Candidate Publication

Provider output must pass Translation semantic normalization/validation before becoming a valid TranslationArtifact Candidate.

---

# 186. Architecture Decision — Publication Requires Runtime Authority

A semantically valid Candidate may still be stale.

Current execution authority must be validated before publication.

---

# 187. Architecture Decision — Translation Does Not Own UI Fit

Translation may produce concise semantic output or warnings.

Presentation owns layout/fitting consequences.

---

# 188. Architecture Decision — User Corrections Are Semantic Authority

Confirmed manual Translation corrections are first-class semantic data, not ephemeral UI edits.

---

# 189. Architecture Decision — Translation Memory Is Not Truth

Translation Memory is candidate reusable knowledge requiring compatibility validation.

---

# 190. Architecture Invariants

1. Translation consumes Published SourceDocumentArtifact.

2. Translation owns TranslationUnit.

3. Translation owns TranslationBatch.

4. Translation owns Translation context assembly.

5. Translation owns TranslationArtifact semantics.

6. Text Processing does not create TranslationUnit.

7. Runtime owns WorkItem.

8. Runtime owns Attempt.

9. Runtime owns retry execution.

10. Runtime owns cancellation mechanics.

11. Runtime owns timeout/deadline mechanics.

12. Runtime owns supersession.

13. Translation classifies semantic/provider failures.

14. Retry creates a new Attempt.

15. Provider fallback is represented through new execution provenance.

16. Provider DTOs remain inside adapters.

17. Provider credentials never enter semantic Artifacts.

18. Supporting context is distinct from target content.

19. TranslationUnit identity remains traceable to SourceDocument.

20. Parallel execution does not change semantic order.

21. Provider completion does not imply publication.

22. Candidate TranslationArtifact requires semantic validation.

23. Candidate publication requires current Runtime authority.

24. Stale Translation cannot replace newer source authority.

25. Published TranslationArtifact is immutable.

26. Manual corrections create new semantic authority/revision.

27. Translation Memory reuse requires semantic compatibility.

28. Cache hit does not bypass authority validation.

29. Local-only policy cannot silently fall back to cloud.

30. Translation does not modify SourceDocument truth.

31. Translation does not perform Presentation rendering.

32. Presentation constraints must not erase essential meaning.

33. Event Bus does not orchestrate retry/fallback/Translation execution.

34. Exact module events belong to `translation/EVENTS.md`.

35. Exact module errors belong to `translation/ERRORS.md`.

36. Translation remains source-path independent after SourceDocumentArtifact.

---

# 191. Deprecated v1 Concepts

Deprecated as current authority:

```text
Translation Context
    → standalone upstream layer
```

Deprecated:

```text
TranslationRequest.requestRevision
Segment.sourceRevision
ContextVersion
```

as generic primary authority identities.

Deprecated:

```text
Translation
    owns retry/backoff
    owns maxAttempts
    owns timeout lifecycle
    owns cancellation lifecycle
```

Deprecated Event Bus execution lifecycle:

```text
TranslationRequested

TranslationPlanningStarted

TranslationBatchStarted

TranslationRetryScheduled

TranslationCancelled
```

Deprecated:

```text
segment
    = architecture-wide Translation input
prepared upstream by Segmentation
```

Current canonical semantic input is:

```text
SourceDocumentArtifact
```

---

# 192. Preserved v1 Strengths

The following v1 concepts remain valuable:

```text
provider independence

Translation strategy

TranslationBatch

target/context isolation

structured provider output

identifier-based mapping

response validation

partial result awareness

stale result protection

provider privacy constraints

local-only enforcement

Chinese → Vietnamese specialization

novel/comic strategy differences

Translation quality warnings

manual Translation corrections

Translation Memory as candidate knowledge

cache compatibility

provider usage/cost metadata

prompt/source security separation
```

---

# 193. Related Documents

```text
doc/01-architecture/core/
├── DATA_FLOW.md
├── STATE_MACHINE.md
└── EVENT_CONVENTION.md

doc/01-architecture/text/
├── TEXT_MODEL.md
└── SEGMENTATION.md

doc/01-architecture/translate/
├── TRANSLATION.md
└── CONTEXT.md

doc/01-architecture/flows/
├── SCREEN_COMIC_FLOW.md
├── STRUCTURED_TEXT_FLOW.md
└── CONTENT_CHANGE_FLOW.md

doc/02-modules/
├── text-processing/
├── translation/
└── presentation/

doc/01-architecture/runtime/
├── PIPELINE_RUNTIME.md
├── RETRY_POLICY.md
├── CANCELLATION.md
├── SCHEDULER.md
└── WORK_QUEUE.md
```

---

# 194. Open Decisions

The following remain open:

```text
final TranslationUnit schema

final TranslationArtifact schema

TranslationUnit identity strategy

partial TranslationArtifact publication model

streaming semantic model

TranslationPlan persistence/lifetime

Translation Memory ownership

Glossary/Knowledge module boundary

character-context model

provider-selection authority split

translation-alternative semantics

manual correction persistence scope

prompt/profile versioning model

cache compatibility model

cross-chapter context strategy

source-context vs translated-context balance

provider fallback selection policy

semantic quality-estimation model
```

---

# 195. Completion Criteria

This Translation Architecture is synchronized when:

* SourceDocumentArtifact is the canonical input;
* Translation Context is no longer an upstream owner;
* Translation owns TranslationUnit;
* Translation owns TranslationBatch;
* Translation owns context assembly;
* Runtime owns WorkItem/Attempt execution;
* retry/backoff is removed from Translation authority;
* cancellation is Runtime-owned;
* timeout/deadline is Runtime-owned;
* provider fallback is modeled through Attempt execution;
* typed Artifact/Runtime provenance replaces generic request revision authority;
* provider output is normalized and validated before Candidate creation;
* Candidate → Published authority matches Runtime v2;
* stale Translation cannot overwrite newer content;
* manual corrections remain first-class semantic data;
* provider adapters remain isolated;
* Chinese→Vietnamese domain considerations remain preserved;
* Translation and Presentation responsibilities remain separate.

---

# 196. Summary

CRAI v1 broadly modeled:

```text
Segmentation
    ↓
Translation Context
    ↓
Translation Request
    ↓
Translation Plan
    ↓
Provider
    ↓
Translated Segments
```

and Translation itself owned much of:

```text
retry
timeout
cancellation
fallback execution
```

CRAI v2 uses:

```text
Published SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationUnit
    ↓
TranslationBatch
    ↓
Context Assembly
    ↓
Translation Semantic Plan
```

Runtime executes:

```text
Translation WorkItem
    ↓
Attempt
    ↓
Provider Adapter
```

Translation then produces:

```text
Provider Output
    ↓
Semantic Validation
    ↓
TranslationArtifact Candidate
    ↓
Runtime Authority Validation
    ↓
Published TranslationArtifact
```

The central rule is:

```text
Translation decides
what and how content means
to be translated.

Runtime decides
how that work executes.

Provider success
does not automatically
create Translation authority.
```
