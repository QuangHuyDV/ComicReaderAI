# CRAI Translation Context Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/translate/CONTEXT.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Architecture Owner:** Translation
> **Parent Architecture:** `TRANSLATION.md`
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-10

---

# 1. Purpose

Translation Context defines how CRAI assembles bounded, relevant, traceable information that helps Translation interpret source content accurately and consistently.

Context is necessary because a TranslationUnit should not always be interpreted in isolation.

Its meaning may depend on:

```text
neighboring source content

dialogue continuity

speaker identity

character information

Glossary terms

chapter/scene information

previous accepted translations

Translation Memory

user translation configuration
```

The central rule is:

```text
Translation Context
    = supporting semantic information
      assembled by Translation

Translation Context
    ≠ standalone pipeline layer
    ≠ separate architecture owner
    ≠ Runtime execution context
```

---

# 2. Architecture Ownership

Translation Context belongs to:

```text
Translation
```

It is a sub-concern of Translation Architecture.

Canonical ownership:

```text
Translation
├── TranslationUnit Construction
├── TranslationBatch Construction
├── Context Assembly
├── Provider Interaction
├── Translation Validation
└── TranslationArtifact
```

There is no standalone:

```text
Context Module
```

between Text Processing and Translation.

---

# 3. Canonical Position

Deprecated:

```text
Text Processing
    ↓
Segmentation
    ↓
Translation Context
    ↓
Translation
```

Current architecture:

```text
Published SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationUnit Construction
    ↓
TranslationBatch Construction
    ↓
Context Assembly
    ↓
Provider Interaction
    ↓
TranslationArtifact Candidate
```

---

# 4. Context Assembly Boundary

Context Assembly begins only after Translation has identified:

```text
what content is being translated
```

through:

```text
TranslationUnit
```

or:

```text
TranslationBatch
```

It then determines:

```text
what supporting information
helps interpret that target content
```

---

# 5. Target vs Context

Every Translation operation must distinguish:

```text
Translation Target
```

from:

```text
Supporting Context
```

Conceptually:

```text
TranslationUnit
    ↓
TARGET

Neighboring content
Character information
Glossary
Translation history
Knowledge
    ↓
CONTEXT
```

Only TARGET content is required to produce translated output.

---

# 6. Fundamental Invariant

Context must never accidentally become additional Translation target content.

For example:

```text
Target:
    Sentence B

Context:
    Sentence A
    Sentence C
```

Provider output should correspond to:

```text
Sentence B
```

not:

```text
translated A
translated B
translated C
```

unless all three were explicitly defined as Translation targets.

---

# 7. Context Assembly Responsibilities

Translation Context Assembly owns:

```text
context-source selection

neighbor selection

context relevance ranking

Glossary selection

character/speaker context selection

translation-history selection

Translation Memory retrieval integration

context-size budgeting

context truncation/compression policy

uncertainty preservation

context provenance

ContextSnapshot construction
```

---

# 8. Non-Responsibilities

Context Assembly does not own:

```text
SourceDocument construction

source-language segmentation

Reading Order authority

OCR

Recognition

persistent Glossary storage

persistent Preferences storage

character database ownership

Translation Memory storage authority

provider execution scheduling

Runtime retry

Runtime cancellation

Runtime timeout

Presentation layout
```

---

# 9. Context Sources

Translation Context may consume information from:

```text
SourceDocumentArtifact

TranslationArtifact

Reading Session

Glossary

Translation Memory

Knowledge / Character Data

Preferences-derived Translation configuration

Application-provided safe metadata
```

Every source must enter through an explicit contract.

---

# 10. SourceDocument Context

`SourceDocumentArtifact` is the primary source-semantic context authority.

It may provide:

```text
Section

Block

Paragraph

Sentence

Span

Annotation

Continuation

SourceReference

language metadata

source ordering

content roles
```

Translation must consume these structures rather than reconstructing source semantics independently.

---

# 11. Current Translation Target

The current target is represented by:

```text
TranslationUnit
```

or a compatible group of TranslationUnits inside:

```text
TranslationBatch
```

A TranslationUnit contains the semantic identity of content that must produce target-language output.

---

# 12. TranslationUnit Is Not Context

The TranslationUnit itself defines:

```text
what to translate
```

Context defines:

```text
what helps translate it
```

Therefore:

```text
TranslationUnit
    ≠
TranslationContext
```

---

# 13. Neighboring Source Context

Neighboring source content may help resolve:

```text
pronouns

subject omission

sentence continuation

dialogue flow

tone

scene continuity

reference resolution

name usage

terminology
```

---

# 14. Neighbor Selection

Neighbors should follow:

```text
canonical semantic source order
```

rather than arbitrary physical proximity.

For novels:

```text
Paragraph / Sentence order
```

normally dominates.

For comics:

```text
SourceDocument semantic order
```

derived from upstream Recognition/Text Processing semantics is used.

---

# 15. Reading Order Boundary

Translation Context does not reconstruct visual Reading Order.

Visual Reading Order authority remains upstream.

Conceptually:

```text
Recognition
    ↓
Reading Order
    ↓
SourceDocumentArtifact
    ↓
Translation
```

Translation consumes the resulting semantic ordering.

---

# 16. Physical Proximity

Physical proximity alone must not define context.

Two nearby text regions may belong to:

```text
different Panels

different conversations

different captions

different semantic groups
```

Source relationships and semantic ordering take precedence.

---

# 17. Neighbor Relationships

Context relevance may use explicit relationships such as:

```text
previous

next

same Paragraph

same Block

same Panel

same dialogue chain

continuation

same speaker

same scene

same Section
```

when such relationships are available.

---

# 18. Context Window

Context must be bounded.

A context window may constrain:

```text
previous semantic units

next semantic units

maximum characters

maximum tokens

maximum semantic nodes

maximum retrieved knowledge

maximum Translation history
```

---

# 19. Context Window Policy

Conceptually:

```text
ContextWindowPolicy
├── previousLimit
├── nextLimit
├── sourceCharacterBudget?
├── tokenBudget?
├── historyBudget?
├── knowledgeBudget?
└── totalBudget
```

Exact contract belongs to Translation module definitions.

---

# 20. Context Budget Is Provider-Aware

Different providers may impose different limits.

Translation may adapt context size according to:

```text
provider capability

model context window

TranslationBatch size

structured-output overhead

Translation strategy
```

without changing source semantic structure.

---

# 21. Target Preservation

Context-size reduction must never silently remove required Translation targets.

Reduction applies to:

```text
supporting context
```

before:

```text
Translation target
```

---

# 22. Context Selection Principle

Context should be selected because it is relevant.

Not because it merely exists.

Preferred principle:

```text
relevance
    >
availability
```

---

# 23. Context Priority

A default conceptual priority may be:

```text
Translation Target
    ↓
Direct Semantic Neighbors
    ↓
Continuation / Dialogue Relationships
    ↓
Speaker / Character Information
    ↓
Relevant Glossary
    ↓
Recent Compatible Translation History
    ↓
Relevant Translation Memory
    ↓
General Document / Chapter Metadata
```

Exact priority may vary by Translation strategy.

---

# 24. Context Relevance

Context relevance may depend on:

```text
semantic relationship

distance

same dialogue

same Paragraph

same Panel

same scene

shared entity

Glossary term match

speaker relationship

Translation strategy

manual pinning
```

---

# 25. Context Relevance Must Be Explainable

When practical, context selection should preserve enough provenance to explain:

```text
why was this context item included?
```

Possible reasons:

```text
previous_sentence

same_dialogue

speaker_context

glossary_match

manual_context

translation_history

memory_match
```

---

# 26. Document Context

General source metadata may include:

```text
document title

chapter title

section title

content type

source language

target language

genre

source identity
```

Only semantically useful metadata should be included.

---

# 27. Document Metadata Is Low Priority

General metadata usually has lower priority than:

```text
direct semantic neighbors

speaker information

relevant terminology
```

when context budget is constrained.

---

# 28. Character Context

Character context may include:

```text
canonical source name

preferred target name

aliases

known gender

titles

relationships

preferred pronouns

role

manual confirmations
```

---

# 29. Character Context Authority

Context Assembly does not invent authoritative character facts.

It consumes them from:

```text
Knowledge

Glossary

manual corrections

other explicit owner
```

depending on future architecture.

---

# 30. Character Uncertainty

Character information may be uncertain.

Example:

```text
speaker = unknown

speaker candidates =
    Character A 0.65
    Character B 0.35
```

Context must preserve that uncertainty.

---

# 31. No False Speaker Certainty

Forbidden:

```text
unknown speaker
    ↓
Context Builder guesses
    ↓
speaker = Character A
```

without retaining uncertainty/provenance.

---

# 32. Manual Speaker Confirmation

Explicit user confirmation has stronger authority than automatic inference for the compatible scope.

Conceptually:

```text
automatic inference
    ↓
manual confirmation
    ↓
confirmed speaker context
```

---

# 33. Glossary Context

Glossary entries provide terminology guidance.

Possible categories:

```text
character

place

organization

skill

item

cultivation level

title

technical term

genre-specific term
```

---

# 34. Glossary Selection

Do not attach the entire Glossary by default.

Select entries based on:

```text
target source text

neighbor source text

identified entities

current chapter/scene

explicit user rule

Translation strategy
```

---

# 35. Glossary Policy

Glossary entries may carry policies such as:

```text
required

preferred

advisory

contextual
```

Translation validation determines whether relevant constraints were respected.

---

# 36. Glossary Ownership

Context Assembly:

```text
selects
and
attaches
```

Glossary entries.

It does not own persistent Glossary storage.

---

# 37. Translation History

Previous accepted Translation may provide useful context for:

```text
terminology consistency

character voice

pronoun consistency

name consistency

dialogue continuity

narrative style
```

---

# 38. Translation History Source

Translation history should come from:

```text
Published TranslationArtifact
```

or another explicitly authoritative Translation source.

Do not use arbitrary provisional provider output as historical truth.

---

# 39. Translation History Priority

Preferred history is normally:

```text
same dialogue
    ↓
same scene
    ↓
same page/section
    ↓
same chapter
    ↓
older compatible context
```

depending on content type.

---

# 40. Translation History Compatibility

History should only be reused when compatible with relevant semantics such as:

```text
source identity

target language

Translation configuration

Glossary

manual corrections

content scope
```

---

# 41. Translation Memory

Translation Memory may provide reusable source-target examples.

It differs from immediate Translation history.

```text
Translation History
    = recent accepted contextual output

Translation Memory
    = reusable stored source-target knowledge
```

---

# 42. Translation Memory Is Not Truth

Memory matches are candidate context.

They must be validated for compatibility before inclusion.

---

# 43. Knowledge Context

Future Knowledge may provide:

```text
character information

relationships

world terminology

locations

organizations

story-specific facts

genre knowledge
```

Context Assembly consumes relevant knowledge.

It does not own the Knowledge domain.

---

# 44. User Translation Configuration

Translation consumes an immutable effective configuration derived from Preferences.

It may affect:

```text
literalness

naturalness

name handling

honorific handling

pronoun policy

SFX policy

terminology

custom instructions
```

---

# 45. Preferences Are Not Context History

Persistent Preferences and contextual Translation history are distinct.

```text
Preferences
    = user configuration authority

Translation History
    = previous accepted target-language semantics
```

---

# 46. ContextSnapshot

Provider execution should receive an immutable:

```text
TranslationContextSnapshot
```

or equivalent.

Conceptually:

```text
TranslationContextSnapshot
├── TargetRefs
├── NeighborContext[]
├── DocumentContext?
├── CharacterContext[]
├── GlossaryContext[]
├── TranslationHistory[]
├── TranslationMemoryContext[]
├── KnowledgeContext[]
├── TranslationConfigurationRef
├── Provenance
└── CompatibilityMetadata
```

Exact contract remains module-owned.

---

# 47. Snapshot Immutability

Once created for an execution Attempt:

```text
TranslationContextSnapshot
```

must not change underneath that Attempt.

If relevant inputs change:

```text
create new semantic plan/context snapshot
```

rather than mutating the old one.

---

# 48. Snapshot vs Runtime Attempt

These are different concepts:

```text
TranslationContextSnapshot
    = immutable Translation semantic input

Attempt
    = Runtime execution identity
```

One compatible ContextSnapshot may potentially be reused across execution Attempts.

---

# 49. Snapshot vs TranslationBatch

A TranslationBatch defines:

```text
which TranslationUnits
are translated together
```

ContextSnapshot defines:

```text
what supporting information
is supplied for that execution scope
```

---

# 50. Batch-Level Context

Some context may apply to the entire TranslationBatch:

```text
chapter title

Glossary subset

character roster

Translation strategy

general scene context
```

---

# 51. Unit-Level Context

Other context may be specific to one TranslationUnit:

```text
speaker

direct neighbors

continuation

specific Glossary entries

specific history

local ambiguity
```

The architecture may support both scopes.

---

# 52. Context Deduplication

Shared batch context should not need to be duplicated for every unit if the provider/request model can represent it safely.

This improves:

```text
token efficiency

cost

clarity

consistency
```

---

# 53. Context Assembly Pipeline

Conceptually:

```text
TranslationUnit / TranslationBatch
    ↓
Resolve Source Scope
    ↓
Collect Direct Semantic Context
    ↓
Collect Character / Speaker Context
    ↓
Select Relevant Glossary
    ↓
Select Compatible Translation History
    ↓
Retrieve Compatible Memory / Knowledge
    ↓
Apply Translation Configuration
    ↓
Rank Context
    ↓
Enforce Context Budget
    ↓
Build TranslationContextSnapshot
```

---

# 54. Context Assembly Does Not Produce TranslationRequest Authority

Deprecated:

```text
Context Layer
    ↓
TranslationRequest
    ↓
Translation Engine
```

Current:

```text
Translation
    ↓
TranslationUnit / Batch
    ↓
ContextSnapshot
    ↓
Provider Adapter Mapping
```

Provider request construction remains inside Translation/provider integration.

---

# 55. Provider Request Mapping

Provider adapters combine:

```text
TranslationBatch

TranslationContextSnapshot

Translation strategy

provider capability
```

into provider-native requests.

Context Architecture does not expose provider DTOs.

---

# 56. Target Isolation in Provider Requests

Provider mapping should make the distinction between:

```text
TARGET
```

and:

```text
CONTEXT
```

as explicit as provider capability allows.

Structured identifiers are preferred.

---

# 57. Context Leakage

Context leakage occurs when supporting information is incorrectly emitted as target output.

Examples:

```text
neighbor translated as target

Glossary description emitted in translation

character notes copied into target text

previous Translation repeated
```

Translation validation should detect obvious forms where possible.

---

# 58. Context Provenance

Every context item should remain traceable to its origin.

Possible provenance:

```text
SourceDocumentArtifact

SourceDocument node

TranslationArtifact

Glossary entry

Knowledge entity

Translation Memory entry

manual correction

Translation configuration
```

---

# 59. Typed Provenance

Prefer typed provenance such as:

```text
SourceDocumentArtifactId

TranslationArtifactId

GlossaryRevision

KnowledgeRevision

TranslationMemoryRevision

TranslationConfigurationRevision
```

rather than one universal:

```text
contextVersion
```

---

# 60. No Generic Context Version Authority

Deprecated as primary correctness authority:

```text
contextVersion: number
```

A generic counter cannot explain which semantic dependency changed.

Use typed revisions/identities where possible.

---

# 61. Context Fingerprint

A derived:

```text
ContextFingerprint
```

may still be useful for:

```text
cache lookup

diagnostics

deduplication

reproducibility
```

but it is derived metadata.

It is not the owner of semantic authority.

---

# 62. Context Fingerprint Inputs

A fingerprint may include relevant normalized identities such as:

```text
TranslationUnit content/identity

SourceDocumentArtifact identity

neighbor source identities

Glossary revision

Knowledge revision

Translation Memory revision

Translation configuration revision

Context Assembly algorithm version
```

---

# 63. Compatibility

Two ContextSnapshots may be considered equivalent only when all semantically relevant dependencies are compatible.

Byte equality is not required if semantic equivalence can be established safely.

---

# 64. Context Assembly Version

Context-selection algorithms may have an explicit:

```text
ContextAssemblyVersion
```

for:

```text
reproducibility

cache compatibility

diagnostics

migration
```

This is algorithm versioning, not Runtime authority.

---

# 65. Determinism

Given equivalent:

```text
TranslationUnit / Batch

SourceDocumentArtifact

Glossary snapshot

Knowledge snapshot

Translation history

Translation Memory snapshot

Translation configuration

Context Assembly version
```

deterministic Context Assembly rules should produce equivalent semantic context.

---

# 66. Runtime Scheduling Must Not Change Context Semantics

Equivalent work should not receive different semantic context merely because:

```text
queue order changed

worker changed

Attempt number changed

execution started later
```

unless an explicitly versioned dependency changed.

---

# 67. Uncertainty

Context may contain uncertain information such as:

```text
unknown speaker

uncertain character identity

ambiguous relationship

low-confidence Recognition source

uncertain source ordering

unresolved name
```

Uncertainty must remain explicit.

---

# 68. Confidence

Where meaningful, uncertain context may include:

```text
confidence

candidate values

source

inference method

manual confirmation status
```

---

# 69. Missing Optional Context

Missing optional context should not normally block Translation.

Examples:

```text
no character information
    → continue

no Glossary
    → continue

no previous Translation
    → continue

no next Sentence
    → continue

no Translation Memory
    → continue
```

---

# 70. Required Context

Some Translation strategies may define required contextual dependencies.

Example:

```text
mandatory terminology policy
```

If required context is unavailable or invalid, Translation may fail semantic validation/planning explicitly.

---

# 71. Invalid Target

Context Assembly cannot proceed meaningfully when required Translation target data is invalid.

Examples:

```text
missing TranslationUnit

empty required source content

invalid source references

incompatible SourceDocumentArtifact
```

Exact errors belong to Translation module contracts.

---

# 72. Manual Overrides

Manual semantic corrections may affect context through:

```text
confirmed speaker

preferred character name

Glossary override

pronoun choice

Translation correction

source correction
```

---

# 73. Manual Authority Is Scoped

Manual input has stronger authority only within its compatible scope.

Do not treat:

```text
one corrected pronoun
```

as universal character truth unless the correction explicitly establishes that rule.

---

# 74. Manual Provenance

Manual context-affecting changes should remain:

```text
traceable

reversible

scoped

versioned
```

---

# 75. Source Corrections

If source text or source relationships change:

```text
new SourceDocumentArtifact
```

may invalidate existing ContextSnapshots.

Translation must rebuild context when compatibility no longer holds.

---

# 76. Translation Corrections

A user-corrected Translation may become useful history/context for later TranslationUnits.

Only an authoritative accepted correction should be used.

---

# 77. Novel Context

Novel Translation commonly prioritizes:

```text
Paragraph continuity

Sentence continuity

narrative viewpoint

dialogue turns

character references

chapter terminology

previous accepted Translation
```

---

# 78. Novel Neighbor Window

Conceptually:

```text
Previous Paragraph(s)
        ↓
Current TranslationUnit
        ↓
Next Paragraph / Sentence(s)
```

The exact window depends on context budget and strategy.

---

# 79. Novel Dialogue

For dialogue-heavy novels, useful context may include:

```text
previous speaker

current speaker candidate

relationship between speakers

previous dialogue turn

narrative sentence between turns

established pronoun choices
```

---

# 80. Chinese Novel Context

Chinese-to-Vietnamese novel Translation especially benefits from context for:

```text
omitted subjects

pronouns

kinship terms

titles

cultivation terminology

character names

sect names

historical/fantasy titles

dialogue hierarchy
```

---

# 81. Chinese Pronoun Ambiguity

Source:

```text
他回来了。
```

Possible Vietnamese output depends on context.

Context may reveal:

```text
identity

gender

relationship

social hierarchy

speaker attitude

previous pronoun convention
```

Translation should not infer more certainty than available evidence supports.

---

# 82. Comic Context

Comic Translation commonly prioritizes:

```text
Panel relationships

Bubble relationships

dialogue order

speaker identity

nearby dialogue

caption role

visual-text role

scene continuity
```

---

# 83. Comic Source Geometry

Translation may indirectly reference geometry through:

```text
SourceReference
```

when useful.

It does not become geometry authority.

---

# 84. Comic Context Example

Conceptually:

```text
Panel 1
├── Bubble A
└── Bubble B

Panel 2
└── Bubble C
```

For Bubble B:

```text
Bubble A
same Panel
speaker context
dialogue relationship
```

may be more relevant than physically nearby Bubble C.

---

# 85. Cross-Bubble Continuation

If Text Processing has established:

```text
Bubble A
    continues into
Bubble B
```

Translation Context should preserve that relationship.

It should not rediscover continuation independently.

---

# 86. SFX Context

Sound effects may require context such as:

```text
content role

nearby action

Panel relationship

Translation strategy

Glossary/SFX terminology
```

but visual interpretation remains upstream unless explicitly provided.

---

# 87. Structured Text Context

Structured browser/novel/document text may not have geometry.

Context Assembly must work from:

```text
semantic source order

document hierarchy

source locators

Paragraph/Sentence relationships
```

without requiring OCR metadata.

---

# 88. Cross-Source Independence

Context Assembly must operate consistently whether SourceDocument originated from:

```text
OCR

DOM

plain text

clipboard

document parser
```

after canonical source semantics are available.

---

# 89. Reading Session Context

Reading Session may provide safe high-level context such as:

```text
current document

current chapter

reading position

active content identity
```

Translation does not consume the entire mutable Reading Session object directly.

---

# 90. Reading Session Authority

Reading Session owns:

```text
ReadingContext

ReadingContextRevision
```

Translation Context only consumes explicit compatible snapshots/references.

---

# 91. Runtime Context vs Translation Context

These must never be conflated.

```text
Runtime Execution Context
    = cancellation
      deadline
      tracing
      Attempt identity
      resource execution state

Translation Context
    = semantic information
      helping translation
```

---

# 92. RuntimeRevision

`RuntimeRevisionId` may participate in publication authority.

It should not be inserted into semantic context merely to influence Translation meaning.

---

# 93. Context and Attempts

Example:

```text
TranslationBatch B1
ContextSnapshot C1
    ↓
Attempt T1
    ↓ failure

TranslationBatch B1
ContextSnapshot C1
    ↓
Attempt T2
```

Reuse may be valid if semantic dependencies have not changed.

---

# 94. Context Change Between Attempts

If relevant semantics change between Attempts:

```text
Glossary G1
    ↓
Glossary G2
```

then:

```text
ContextSnapshot C1
```

may no longer be compatible.

A new semantic plan/context snapshot may be required.

---

# 95. Retry Boundary

Context Assembly does not schedule retries.

It may expose whether previous context remains compatible.

Runtime owns retry execution.

---

# 96. Cancellation Boundary

Cancellation does not mutate Translation Context semantics.

A cancelled Attempt may simply stop using its immutable ContextSnapshot.

---

# 97. Supersession

If newer source content supersedes current work:

```text
old ContextSnapshot
```

does not automatically become invalid as historical data, but it loses current publication relevance.

---

# 98. Cache

ContextSnapshots or derived context selections may be cached.

Cache compatibility must include all semantically relevant dependencies.

---

# 99. Context Cache Key

A conceptual cache key may include:

```text
TranslationUnit / Batch fingerprint

SourceDocumentArtifact fingerprint

Translation configuration revision

Glossary revision

Knowledge revision

Translation Memory revision

Context Assembly version
```

---

# 100. Cache Is Not Authority

A cached ContextSnapshot is only reusable when compatibility is proven.

Cache hit does not bypass:

```text
semantic compatibility

Runtime publication authority
```

---

# 101. Context Compression

Large context may require compression.

Possible future strategies:

```text
drop low-relevance context

deduplicate repeated metadata

summarize older context

compress character information

retrieve only matched terminology

retain recent direct dialogue verbatim
```

---

# 102. Compression Must Preserve Provenance

Compressed/summarized context should indicate:

```text
that it is derived

which sources contributed

which algorithm/version produced it
```

where relevant.

---

# 103. Compression Must Not Rewrite Target

Context compression applies only to supporting context.

Translation target content remains governed by TranslationUnit semantics.

---

# 104. Context Ranking

Future Context Assembly may rank candidate context using:

```text
rule-based relevance

semantic similarity

relationship strength

recency

manual priority

entity overlap
```

---

# 105. AI-Based Context Ranking

If AI is used for context ranking:

```text
ranking output
```

is advisory/derived unless explicitly promoted through architecture rules.

It must not silently rewrite source or character authority.

---

# 106. Semantic Retrieval

Future Translation may retrieve relevant information from earlier chapters or documents.

Retrieval results are:

```text
candidate context
```

not automatically semantic truth.

---

# 107. Retrieval Compatibility

Retrieved context should consider:

```text
same work/book

same character identity

same terminology domain

current chapter scope

Translation configuration

user corrections
```

---

# 108. Context Security

All source/context content must be treated as untrusted data.

Context must not be able to override:

```text
system Translation rules

privacy policy

security rules

provider restrictions

structured-output requirements
```

---

# 109. Prompt Injection Boundary

Source text may contain instructions such as:

```text
Ignore previous instructions...
```

This remains source content.

It must not become system-level Translation control.

---

# 110. Context Privacy

Context minimization is also a privacy requirement.

Send:

```text
only information required
for the current Translation
```

especially when using cloud providers.

---

# 111. Sensitive Context

Potentially sensitive context may include:

```text
private documents

authenticated web content

clipboard text

personal annotations

user corrections
```

Provider policy must determine whether such context may leave the device.

---

# 112. Local-Only Context

When Translation policy is:

```text
local-only
```

context must not be sent to cloud providers through fallback.

---

# 113. Logging

Raw Translation Context should not be logged by default.

Diagnostics should prefer:

```text
counts

sizes

identifiers

hashes/fingerprints

selection reasons

latency

warnings
```

over raw source text.

---

# 114. Diagnostics

Useful Context diagnostics include:

```text
context item count

neighbor count

Glossary matches

history items

memory matches

context size

context truncation

context compression

unknown speaker

uncertain context count

context cache hit

assembly latency
```

---

# 115. Context Selection Trace

Debug builds may expose a safe context-selection trace such as:

```text
included: previous_sentence
included: same_speaker
included: glossary_match
excluded: distant_chapter
excluded: budget_limit
```

without exposing secrets.

---

# 116. Performance

Context Assembly should remain bounded.

Avoid:

```text
scanning an entire novel

attaching an entire Glossary

attaching all character data

sending full Translation history

loading every previous chapter
```

for each TranslationUnit.

---

# 117. Indexes

Future performance optimization may use indexes for:

```text
source order

entity references

Glossary terms

character IDs

Translation history

Translation Memory

semantic retrieval
```

These are implementation concerns behind stable architecture contracts.

---

# 118. Interactive Priority

For interactive reading, Context Assembly should prioritize enough information for the current visible Translation rather than maximizing theoretical context completeness.

---

# 119. Context Assembly Latency

Context retrieval itself must not become the dominant interactive bottleneck.

Expensive retrieval should be:

```text
bounded

cached

optional

degradable
```

where safe.

---

# 120. Degradation

When optional context cannot be obtained within the useful execution window, Translation may degrade to less context.

Example:

```text
full context unavailable
    ↓
direct neighbors + Glossary
    ↓
Translation proceeds
```

provided required semantic constraints remain satisfied.

---

# 121. Degradation Must Be Observable

When significant context is omitted due to:

```text
budget

latency

missing dependency

privacy policy
```

Translation may expose a warning/diagnostic where useful.

---

# 122. Provider Independence

Translation Context semantics must remain provider-independent.

Do not define canonical context around:

```text
OpenAI messages

Gemini contents

Claude messages

DeepL glossary DTO

provider token objects
```

---

# 123. Provider Adapter Responsibility

Provider Adapter decides how canonical:

```text
TranslationContextSnapshot
```

is represented in the provider-native request.

---

# 124. Provider Context Capability

Providers may differ in:

```text
context window size

structured prompts

system/user message separation

Glossary support

JSON output

multi-item batching
```

Translation adapts mapping without changing canonical context semantics.

---

# 125. Testing — Core

Tests should verify:

```text
target/context isolation

neighbor ordering

bounded context

context provenance

deterministic selection

optional-context degradation

manual authority

uncertainty preservation

provider independence
```

---

# 126. Testing — Novels

Test:

```text
paragraph continuity

dialogue turns

omitted Chinese subjects

pronoun ambiguity

chapter terminology

previous Translation continuity
```

---

# 127. Testing — Comics

Test:

```text
Panel/Bubble relationships

reading-order-derived neighbors

cross-Bubble continuation

speaker uncertainty

physically-close but unrelated text

SFX context
```

---

# 128. Testing — Glossary

Test:

```text
relevant entry selection

unrelated entry exclusion

required terminology

manual override

Glossary revision change

large Glossary bounded retrieval
```

---

# 129. Testing — History

Test:

```text
same-dialogue history

same-scene history

incompatible history exclusion

manual corrected Translation reuse

provisional Translation exclusion
```

---

# 130. Testing — Context Budget

Test:

```text
small provider context limit

target preservation

priority-based trimming

history trimming

knowledge trimming

deterministic truncation
```

---

# 131. Testing — Stale Context

Test:

```text
SourceDocument changes

Glossary changes

Knowledge changes

Translation configuration changes

manual correction changes

Context Assembly version changes
```

and verify incompatible ContextSnapshots are not reused.

---

# 132. MVP Scope

MVP Context Assembly should support:

```text
TranslationUnit target

previous/next semantic neighbors

document/chapter identity

source/target language

Translation role

basic Glossary selection

effective Translation configuration

bounded context window

target/context isolation

context provenance

immutable ContextSnapshot
```

---

# 133. MVP Context Sources

Required initial sources:

```text
SourceDocumentArtifact

basic Glossary input

Translation configuration
```

Useful but optional initial support:

```text
recent Published Translation history
```

---

# 134. MVP Does Not Require

```text
automatic speaker detection

character relationship graph

cross-chapter semantic retrieval

AI context ranking

context summarization

long-term Translation Memory

automatic Glossary generation

learned user terminology

scene inference
```

---

# 135. Future Extensions

Possible future capabilities:

```text
automatic speaker inference

character relationship graph

scene-aware context

semantic retrieval

context ranking

context compression

cross-chapter Translation Memory

AI-assisted Glossary generation

character-specific context profiles

context quality diagnostics

adaptive context budgeting
```

---

# 136. Future Extensions Must Preserve Ownership

Future context capabilities must remain:

```text
Translation-owned semantic context assembly
```

unless architecture explicitly introduces another owner.

They must not recreate:

```text
Text Processing
    ↓
Context Layer
    ↓
Translation
```

---

# 137. Architecture Decision — Context Is Translation-Owned

There is no independent Translation Context pipeline stage.

Context Assembly is internal Translation architecture.

---

# 138. Architecture Decision — TranslationUnit Defines Target

Context Assembly does not decide arbitrary source segments to translate.

TranslationUnit construction defines Translation target semantics.

---

# 139. Architecture Decision — ContextSnapshot Is Immutable

Provider execution consumes immutable semantic context.

Mutable global state must not be read as Translation context during an Attempt.

---

# 140. Architecture Decision — Typed Provenance Replaces Generic contextVersion

A generic context counter is insufficient for correctness.

Use explicit semantic dependencies.

---

# 141. Architecture Decision — Reading Order Is Consumed

Context Assembly uses canonical source ordering.

It does not reconstruct visual Reading Order.

---

# 142. Architecture Decision — Context Is Bounded

More context is not automatically better.

Context must be:

```text
relevant

bounded

traceable

privacy-aware
```

---

# 143. Architecture Decision — Context Is Provider-Neutral

Canonical ContextSnapshot remains independent of provider request format.

---

# 144. Architecture Decision — Context May Degrade

Missing optional context does not automatically block Translation.

Required semantic constraints still must be satisfied.

---

# 145. Architecture Decision — Context Does Not Own Knowledge

Character, Glossary, Memory and other knowledge remain separately owned.

Context Assembly selects compatible information from them.

---

# 146. Architecture Decision — Context Does Not Own Runtime Execution

Context semantics are independent from:

```text
Attempt lifecycle

retry

cancellation

timeout

Scheduler
```

---

# 147. Architecture Invariants

1. Translation Context is owned by Translation.

2. Translation Context is not a standalone pipeline layer.

3. TranslationUnit defines target content.

4. Supporting context is distinct from target content.

5. Context must not accidentally become translated output.

6. SourceDocumentArtifact is the primary source-semantic context authority.

7. Context Assembly does not reconstruct SourceDocument.

8. Context Assembly does not reconstruct visual Reading Order.

9. Neighbor context follows canonical semantic source order.

10. Physical proximity alone is insufficient for context selection.

11. Context selection is relevance-based.

12. Context is bounded.

13. Translation targets are preserved during context reduction.

14. Context items remain traceable to their owners/sources.

15. ContextSnapshot is immutable for execution.

16. ContextSnapshot is not Runtime Attempt identity.

17. TranslationBatch is not ContextSnapshot.

18. Context Assembly remains provider-independent.

19. Provider adapters map canonical context to native requests.

20. Generic `contextVersion` is not primary semantic authority.

21. Typed semantic revisions/identities are preferred.

22. ContextFingerprint is derived metadata only.

23. Missing optional context does not normally block Translation.

24. Uncertain information remains uncertain.

25. Context Assembly does not fabricate speaker/character certainty.

26. Manual confirmations have stronger scoped authority than automatic inference.

27. Glossary is consumed, not owned.

28. Translation history should come from authoritative accepted Translation.

29. Translation Memory is candidate knowledge, not truth.

30. Persistent Preferences remain Preferences-owned.

31. Runtime retry does not mutate Context semantics.

32. Runtime cancellation does not mutate Context semantics.

33. Runtime scheduling does not define Context semantics.

34. Cache reuse requires semantic compatibility.

35. Cache is not authority.

36. Context minimization applies to cloud provider payloads.

37. Local-only policy applies to Context as well as Translation target text.

38. Raw Context should not be logged by default.

39. Structured and visual source paths use the same Context Architecture after SourceDocumentArtifact.

40. Context Assembly must not modify canonical source text.

---

# 148. Deprecated v1 Model

Deprecated:

```text
OCR Reading Order
    ↓
Text Model
    ↓
Segmentation
    ↓
Translation Context
    ↓
Translation
```

Deprecated responsibility:

```text
Context Layer
    selects segment to translate
```

Current:

```text
TranslationUnit Construction
    determines target
```

Deprecated:

```text
Context Layer
    produces TranslationRequest
```

Current:

```text
Translation
    builds TranslationBatch
    assembles ContextSnapshot
    maps both through Provider Adapter
```

Deprecated primary authority:

```text
requestId

segmentId

contextVersion
```

Current architecture uses typed semantic identities and Runtime execution authority.

---

# 149. Preserved v1 Strengths

The following concepts from v1 remain valid and important:

```text
target/context isolation

neighboring semantic context

document/chapter context

character context

Glossary context

Translation history

user Translation preferences

bounded context window

relevance-based selection

speaker uncertainty

manual authority

novel-specific context

comic-specific context

provider independence

deterministic construction

source traceability

optional-context degradation
```

---

# 150. Related Documents

```text
doc/01-architecture/translate/
├── TRANSLATION.md
└── CONTEXT.md

doc/01-architecture/text/
├── TEXT_MODEL.md
└── SEGMENTATION.md

doc/01-architecture/ocr/
└── READING_ORDER.md

doc/01-architecture/flows/
├── SCREEN_COMIC_FLOW.md
├── STRUCTURED_TEXT_FLOW.md
└── CONTENT_CHANGE_FLOW.md

doc/02-modules/
├── text-processing/
├── translation/
├── reading-session/
└── preferences/

doc/01-architecture/runtime/
```

---

# 151. Open Decisions

The following remain open:

```text
final TranslationContextSnapshot schema

batch-level vs unit-level context representation

context budget units

provider-aware budget calculation

character/knowledge owner

Glossary owner

Translation Memory owner

history retention window

cross-chapter retrieval

context ranking algorithm

context compression model

semantic retrieval architecture

speaker inference ownership

manual context pinning

context cache persistence

context fingerprint algorithm

context quality diagnostics
```

---

# 152. Completion Criteria

Context Architecture is synchronized when:

* Context is explicitly Translation-owned;
* no standalone Context pipeline layer remains;
* TranslationUnit defines target content;
* target and supporting context are explicitly separated;
* SourceDocumentArtifact is the primary source input;
* canonical semantic ordering replaces Context-owned Reading Order;
* Glossary/Knowledge/Memory are consumed rather than owned;
* previous authoritative Translation may be used as history;
* ContextSnapshot is immutable;
* generic `contextVersion` is no longer primary authority;
* typed provenance is used;
* Runtime Attempt/retry/cancellation semantics remain outside Context;
* provider request DTOs remain outside canonical Context;
* optional context can degrade safely;
* privacy/context minimization is explicit;
* novel and comic context requirements remain supported.

---

# 153. Summary

CRAI v1 treated Translation Context as:

```text
Segmentation
    ↓
Context Layer
    ↓
Translation Request
    ↓
Translation
```

CRAI v2 treats Context as:

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
TranslationContextSnapshot
    ↓
Provider Adapter
```

The essential distinction is:

```text
TranslationUnit
    = what must be translated

TranslationContextSnapshot
    = what helps interpret it

Attempt
    = how Runtime executes it
```

The central rule is:

```text
Context helps Translation
understand the target.

Context does not decide
source truth,

does not become
translation output,

and does not own
Runtime execution.
```
