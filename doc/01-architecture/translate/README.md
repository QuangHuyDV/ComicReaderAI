# CRAI Translation Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/translate/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Architecture Owner:** Translation
> **Public Input:** `SourceDocumentArtifact`
> **Public Output:** `TranslationArtifact`
> **Runtime Model:** Runtime v2 aligned
> **Last Updated:** 2026-08-10

---

# 1. Purpose

The Translation Architecture defines how CRAI transforms canonical source-language semantic content into target-language semantic content.

It answers:

```text
What source content should be translated?

How is that content grouped into TranslationUnits?

How are TranslationBatches constructed?

What supporting context should be provided?

How are Translation providers abstracted?

How is provider output normalized and validated?

How is Translation semantic authority committed?
```

The Translation Architecture belongs to:

```text
Translation
```

---

# 2. Central Architecture Rule

Translation owns:

```text
TranslationUnit

TranslationBatch

Translation Context Assembly

Translation strategy

Translation provider semantics

Translation validation

Translation corrections

TranslationArtifact
```

Runtime owns:

```text
WorkItem

Attempt

retry execution

cancellation

deadline

scheduling

supersession

resource control
```

Therefore:

```text
Translation
    owns meaning

Runtime
    owns execution
```

---

# 3. Architecture Position

Canonical upstream paths converge before Translation.

Visual source:

```text
Screen / Image
    ↓
Capture
    ↓
Recognition
    ↓
RecognitionArtifact
    ↓
Text Processing
    ↓
SourceDocumentArtifact
```

Structured source:

```text
DOM / Structured Text
Plain Text
Clipboard
Document Text Layer
        ↓
Text Processing
        ↓
SourceDocumentArtifact
```

Then:

```text
Published SourceDocumentArtifact
    ↓
Translation
    ↓
Published TranslationArtifact
    ↓
Presentation
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

It does not own:

```text
Capture

Recognition

SourceDocument reconstruction

Presentation semantics

native UI rendering
```

---

# 5. Current Document Structure

The Translation Architecture currently consists of:

```text
01-architecture/translate/
├── README.md
├── TRANSLATION.md
└── CONTEXT.md
```

No separate Presentation architecture document is required in this folder.

---

# 6. TRANSLATION.md

`TRANSLATION.md` defines the main Translation semantic architecture.

It answers:

```text
How does CRAI translate
SourceDocument content
into TranslationArtifact?
```

It owns architecture-level definitions for:

```text
TranslationUnit

TranslationBatch

Translation strategy

Translation planning

provider abstraction

provider suitability

provider request semantics

provider response normalization

source-target alignment

Translation validation

Translation corrections

Translation Memory integration

TranslationArtifact
```

---

# 7. CONTEXT.md

`CONTEXT.md` defines Translation-owned context assembly.

It answers:

```text
What supporting semantic information
helps translate a TranslationUnit accurately?
```

It covers:

```text
neighboring source context

dialogue continuity

speaker/character information

Glossary context

Translation history

Translation Memory

Knowledge context

context relevance

context budgeting

context provenance

TranslationContextSnapshot
```

---

# 8. Context Is Not a Separate Pipeline Layer

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

Current:

```text
SourceDocumentArtifact
    ↓
Translation
    ├── TranslationUnit Construction
    ├── TranslationBatch Construction
    ├── Context Assembly
    ├── Provider Interaction
    └── TranslationArtifact
```

`CONTEXT.md` therefore describes a Translation sub-concern, not an independent module.

---

# 9. TranslationUnit

`TranslationUnit` is the canonical Translation-owned semantic unit.

It represents:

```text
what source semantic content
is being translated together
```

A TranslationUnit may map:

```text
1 source Sentence
    ↓
1 TranslationUnit
```

or:

```text
multiple source Sentences
    ↓
1 TranslationUnit
```

or:

```text
1 source Sentence
    ↓
multiple derived TranslationUnits
```

when Translation semantics require it.

---

# 10. TranslationUnit vs Text Segmentation

Text Processing determines:

```text
source-language structure
```

such as:

```text
Paragraph

Sentence

Span

Continuation
```

Translation determines:

```text
TranslationUnit
```

Therefore:

```text
Sentence
    ≠
TranslationUnit
```

---

# 11. No Generic Segment Authority

Deprecated architecture used:

```text
Segment
```

as a universal unit crossing Text Processing, Translation and Presentation.

Current architecture uses typed owners:

```text
Text Processing
    → Paragraph / Sentence / Span

Translation
    → TranslationUnit

Presentation
    → PresentationItem
```

Do not recreate one global `Segment` abstraction.

---

# 12. TranslationBatch

`TranslationBatch` groups compatible TranslationUnits for provider interaction.

It may consider:

```text
semantic coherence

language pair

Translation strategy

provider capability

context needs

structured-output capability

privacy constraints

request limits
```

---

# 13. TranslationBatch vs Runtime WorkItem

These are different:

```text
TranslationBatch
    = Translation semantic/provider grouping

WorkItem
    = Runtime executable work
```

One WorkItem may execute one TranslationBatch or another mapping defined by integration.

The identities must not be conflated.

---

# 14. TranslationBatch vs Attempt

```text
TranslationBatch
    ≠
Attempt
```

A batch may be executed by:

```text
Attempt A1
```

and later:

```text
Attempt A2
```

without changing the semantic purpose of the batch.

---

# 15. Translation Context

Translation Context contains:

```text
information useful for understanding
the Translation target
```

It is distinct from:

```text
content that must produce target output
```

Canonical distinction:

```text
TranslationUnit
    = TARGET

TranslationContextSnapshot
    = SUPPORTING CONTEXT
```

---

# 16. Target vs Context Isolation

Provider interaction must preserve:

```text
TARGET
```

vs:

```text
CONTEXT
```

Supporting context must not accidentally become translated output.

Examples of context:

```text
previous Sentence

next Sentence

same dialogue

speaker information

Glossary terms

previous accepted Translation

chapter metadata
```

---

# 17. TranslationContextSnapshot

Translation provider execution should use an immutable:

```text
TranslationContextSnapshot
```

or equivalent.

It may include:

```text
direct neighbors

character/speaker context

Glossary subset

Translation history

Translation Memory

Knowledge

Translation configuration

typed provenance
```

---

# 18. Context Snapshot vs Runtime Context

Never conflate:

```text
TranslationContextSnapshot
```

with:

```text
Runtime execution context
```

Runtime execution context may contain:

```text
cancellation

deadline

Attempt identity

tracing
```

Translation Context contains semantic information.

---

# 19. Typed Context Provenance

Avoid one generic:

```text
contextVersion
```

as the primary correctness model.

Prefer typed provenance such as:

```text
SourceDocumentArtifactId

TranslationArtifactId

GlossaryRevision

KnowledgeRevision

TranslationMemoryRevision

TranslationConfigurationRevision
```

---

# 20. Provider Abstraction

Translation is provider-independent.

Canonical relationship:

```text
Translation
    ↓
TranslationProviderPort
    ↓
Provider Adapter
    ↓
Provider API / Local Engine
```

Providers may include:

```text
cloud AI

local AI

machine translation API

offline engine

user-configured provider
```

---

# 21. Provider DTO Isolation

Provider-native objects must remain inside adapters.

Do not expose canonical contracts containing:

```text
OpenAI response objects

DeepL DTOs

Google Translation DTOs

local model tensors

SDK exception classes
```

---

# 22. Provider Capability

Provider capability may describe:

```text
supported languages

maximum input size

structured output

batch support

streaming

Glossary support

local/cloud mode

privacy characteristics

cancellation support
```

---

# 23. Provider Suitability

Translation determines whether a provider is semantically compatible with:

```text
language pair

Translation strategy

privacy requirements

structured-output needs

local-only policy

content role
```

Actual execution remains Runtime/provider-management controlled.

---

# 24. Provider Selection Boundary

Conceptually:

```text
Translation
    → semantic suitability

Provider Management
    → available eligible providers

Runtime
    → execution Attempt
```

The exact split remains synchronized with provider/runtime architecture.

---

# 25. Local-Only Invariant

If Translation configuration requires:

```text
local-only
```

execution must never silently fall back to a cloud provider.

This constraint applies to:

```text
Translation target

Translation Context
```

---

# 26. Translation Planning

Translation may construct a semantic:

```text
TranslationPlan
```

containing:

```text
TranslationUnits

TranslationBatches

Translation strategy

provider requirements

ContextSnapshots

validation requirements
```

---

# 27. TranslationPlan vs RuntimeRevision

```text
TranslationPlan
    = semantic Translation intent

RuntimeRevision
    = execution authority
```

TranslationPlan does not own:

```text
queue state

Attempt lifecycle

retry timers

cancellation state
```

---

# 28. Provider Request Mapping

Provider adapters map:

```text
TranslationBatch
+
TranslationContextSnapshot
+
Translation strategy
```

to provider-native requests.

Canonical Translation semantics remain provider-neutral.

---

# 29. Structured Output

Structured provider output is preferred where available.

Example:

```text
TranslationUnitId
    ↓
TranslatedText
```

Identifier-based mapping is preferable to positional parsing.

---

# 30. Free-Form Provider Output

If a provider only returns free-form text:

```text
Provider Output
    ↓
Controlled Parser
    ↓
Canonical Provider Result
```

Validation requirements are stricter.

---

# 31. Provider Output Is Provisional

Provider success does not automatically create Translation authority.

Canonical flow:

```text
Provider Result
    ↓
Normalization
    ↓
Translation Validation
    ↓
TranslationArtifact Candidate
```

---

# 32. Translation Validation

Before Candidate creation, Translation should validate:

```text
expected TranslationUnits exist

no duplicate unit IDs

no unexpected unit IDs

translated text structurally valid

context not leaked into target output

mapping intact

required terminology policy satisfied or warned

output sanity acceptable
```

---

# 33. TranslationArtifact

`TranslationArtifact` is the immutable public semantic output of Translation.

It may contain:

```text
ArtifactId

SourceDocumentArtifactRef

source language

target language

translated units

source-target alignment

Translation strategy

warnings

provider provenance

configuration provenance

correction provenance
```

Exact schema belongs to:

```text
02-modules/translation/CONTRACT.md
```

---

# 34. Source-Target Alignment

TranslationArtifact must retain traceability to source semantic content.

Conceptually:

```text
SourceDocument Nodes
        ↕
TranslationUnit
        ↕
Translated Output
```

Alignment may support:

```text
1:1

1:N

N:1

N:M
```

---

# 35. Translation Does Not Modify Source Truth

Translation must not mutate:

```text
SourceDocumentArtifact
```

If source text is wrong:

```text
source correction
    ↓
Text Processing owner
```

not Translation mutation.

---

# 36. Translation Correction

User-corrected target text belongs to Translation semantics.

Canonical flow:

```text
TranslationArtifact T1
    ↓
user correction
    ↓
new Translation semantic revision
    ↓
TranslationArtifact T2
```

Published TranslationArtifact remains immutable.

---

# 37. Correction Authority

Confirmed user Translation correction has stronger scoped authority than older automatic Translation for compatible source content.

It remains valid until:

```text
explicitly changed

explicitly removed

source becomes incompatible
```

---

# 38. Source Correction vs Translation Correction

These are separate:

```text
Source correction
    → Text Processing/source semantics

Translation correction
    → Translation semantics
```

A source correction may invalidate a previous Translation correction.

---

# 39. Glossary

Translation may consume Glossary information.

Glossary is contextual terminology input.

It is not:

```text
post-Translation global string replacement
```

---

# 40. Glossary Policies

Possible policies:

```text
required

preferred

advisory

contextual
```

Translation validation may surface violations.

---

# 41. Translation Memory

Translation Memory stores reusable source-target knowledge.

It may support:

```text
repeated dialogue

recurring terms

known titles

previous passages

interface text
```

---

# 42. Translation Memory Is Not Authority

A Translation Memory entry is candidate reusable knowledge.

It requires compatibility with:

```text
current source

language pair

Translation configuration

Glossary

context

semantic role
```

---

# 43. Translation History

Recent accepted Translation may be used as context for:

```text
pronoun consistency

name consistency

dialogue continuity

character voice

terminology consistency
```

Only accepted/published Translation should normally become authoritative history.

---

# 44. Knowledge Context

Future Knowledge may provide:

```text
characters

relationships

locations

organizations

story terminology

world facts
```

Translation consumes relevant Knowledge through explicit contracts.

Translation does not necessarily own Knowledge storage.

---

# 45. Chinese → Vietnamese Priority

Initial CRAI Translation Architecture prioritizes Chinese → Vietnamese.

Important domain concerns include:

```text
Simplified Chinese

Traditional Chinese

names

Sino-Vietnamese terminology

pronouns

social titles

cultivation levels

sects

techniques

items

historical/fantasy titles

idioms

internet slang

omitted subjects

gender ambiguity
```

---

# 46. Chinese Name Consistency

Names should remain consistent across compatible:

```text
dialogue

Paragraphs

chapters

Panels

sessions
```

when adequate Glossary/Knowledge/history exists.

---

# 47. Chinese Pronoun Ambiguity

Chinese frequently omits information required for natural Vietnamese pronouns.

Translation may use:

```text
neighbor context

speaker hints

character information

previous Translation

Glossary
```

but must not fabricate certainty where evidence is insufficient.

---

# 48. Simplified vs Traditional Chinese

Translation must recognize source script metadata such as:

```text
zh-Hans

zh-Hant
```

without expecting Text Processing to convert one script into the other during generic normalization.

---

# 49. Novel Translation

Novel strategy may prioritize:

```text
Paragraph continuity

narrative viewpoint

dialogue continuity

character voice

terminology consistency

natural Vietnamese flow

bounded chapter context
```

---

# 50. Novel Context

Useful novel context may include:

```text
previous Paragraphs

next Sentence

current scene

chapter title

speaker candidates

Glossary

previous accepted Translation
```

Context remains bounded.

---

# 51. Long-Form Translation

Long chapters should support:

```text
incremental TranslationUnit construction

bounded TranslationBatches

bounded context

current-reading priority

limited prefetch
```

Whole-book eager Translation is not required.

---

# 52. Comic Translation

Comic strategy may prioritize:

```text
dialogue tone

brevity

character voice

Bubble relationships

Panel/source order

caption clarity

SFX policy

Presentation constraints
```

---

# 53. Presentation Constraints

Presentation may provide advisory semantic constraints such as:

```text
prefer concise target wording

preferred target-length range

content role
```

These constraints do not allow Translation to remove essential meaning.

---

# 54. Translation vs Presentation

Translation owns:

```text
target-language meaning
```

Presentation owns:

```text
how that meaning is arranged
for reading
```

Canonical boundary:

```text
TranslationArtifact
    ↓
Presentation
```

---

# 55. No PRESENTATION.md in Translation Architecture

The previous architecture placed:

```text
01-architecture/translate/PRESENTATION.md
```

inside the Translation folder.

That model is retired.

Presentation is defined by:

```text
02-modules/presentation/
```

including:

```text
MODULE.md

CONTRACT.md

STATES.md

EVENTS.md

ERRORS.md

README.md
```

---

# 56. Why Presentation Is Not Here

Presentation has its own semantic ownership:

```text
PresentationSnapshot

RenderPlan

PresentationItem

PresentationMode

presentation layout/readability
```

Those are not Translation concerns.

Keeping a separate Presentation architecture authority inside `translate/` would duplicate module ownership.

---

# 57. Runtime Execution

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

Translation does not own Runtime Attempt lifecycle.

---

# 58. Retry

Translation may classify an error as:

```text
retryable

non-retryable

provider-incompatible

semantic-invalid

configuration-invalid
```

Runtime decides whether another Attempt is created.

---

# 59. Retry Creates New Attempt

Canonical:

```text
Attempt A1
    ↓
failure
    ↓
Runtime Retry Policy
    ↓
Attempt A2
```

Do not model retry as a Translation-owned `attemptCount` state machine.

---

# 60. Cancellation

Translation/provider adapters cooperate with Runtime cancellation.

They may:

```text
stop work

cancel provider call

discard provisional data
```

where supported.

They do not own Runtime cancellation authority.

---

# 61. Deadline / Timeout

Translation/provider capabilities may describe execution constraints.

Runtime owns effective:

```text
deadline

timeout lifecycle
```

---

# 62. Supersession

New source/current content may supersede old Translation execution.

Example:

```text
RuntimeRevision R10
    ↓
Translation running
    ↓
R11 becomes current
    ↓
R10 Translation returns
```

R10 result cannot become current authority.

---

# 63. Candidate → Published

Canonical authority path:

```text
Attempt
    ↓
Provider Output
    ↓
Translation Validation
    ↓
TranslationArtifact Candidate
    ↓
Runtime / current authority validation
    ↓
Published TranslationArtifact
```

---

# 64. Provider Success ≠ Publication

A provider can successfully translate obsolete content.

Therefore:

```text
provider success
    ≠
current Translation authority
```

---

# 65. Partial Translation

Partial provider success may occur.

Example:

```text
10 TranslationUnits
    ↓
9 successful
1 failed
```

Whether partial TranslationArtifact publication is allowed must be explicit in Translation contracts.

---

# 66. Partial Translation Is Semantic

If supported, partial TranslationArtifact must explicitly expose:

```text
translated units

missing units

failed units

completeness

warnings
```

Presentation must not infer partial state from Runtime internals.

---

# 67. Streaming

Provider streaming is an execution capability.

Raw stream chunks are:

```text
provisional provider output
```

not automatically Published Translation semantics.

---

# 68. Provisional Translation

A provisional Translation model may be added only through an explicit Translation contract.

Presentation/UI must not treat raw provider tokens as authoritative target text.

---

# 69. Parallel Translation

Independent TranslationUnits/Batches may run concurrently where semantic dependencies permit.

Final semantic order follows source/TranslationUnit order.

It does not follow execution completion order.

---

# 70. Sequential Translation

Some Translation dependencies may require ordered execution.

Examples:

```text
pronoun-sensitive dialogue

terminology established by previous units

connected conversation

progressive contextual Translation
```

Translation may declare semantic dependencies.

Runtime executes them.

---

# 71. Provider Fallback

Provider fallback may involve:

```text
Translation
    → classify suitability/failure

Provider Management
    → identify eligible alternatives

Runtime
    → execute next Attempt
```

Provider fallback is not hidden mutation of the same Attempt.

---

# 72. Provider Fallback Provenance

Example:

```text
Attempt A1
Provider X

Attempt A2
Provider Y
```

The change must remain observable.

---

# 73. Provider Constraints

Fallback must preserve:

```text
language support

privacy policy

local-only policy

explicit provider restrictions

Translation strategy compatibility
```

---

# 74. Cache

Translation output may be cached.

Compatibility may depend on:

```text
source semantic identity

TranslationUnit identity/content

source language

target language

Translation strategy

Glossary revision

Translation configuration revision

Context compatibility

provider/model profile where required

Translation engine/prompt version
```

---

# 75. Cache Is Not Authority

Cache hit requires:

```text
semantic compatibility
+
current authority validation
```

before becoming current Published Translation.

---

# 76. Manual Corrections vs Cache

Manual user Translation corrections should not be treated as ordinary provider cache entries.

They have stronger semantic authority and distinct provenance.

---

# 77. Translation Versioning

Distinguish:

```text
TranslationArtifact schema version

Translation semantic revision

Translation configuration revision

Glossary revision

Knowledge revision

provider/model version

RuntimeRevisionId
```

Do not collapse these into one generic `translationVersion`.

---

# 78. Immutability

Published TranslationArtifact is immutable.

Changes such as:

```text
retranslation

manual correction

strategy change

Glossary change

accepted improved Translation
```

produce a new semantic result.

---

# 79. Events

Exact Translation events belong to:

```text
02-modules/translation/EVENTS.md
```

Architecture documents do not create a second event catalog.

---

# 80. Event Bus Rule

Event Bus reports committed facts.

It must not orchestrate:

```text
TranslationRequested

RetryRequested

FallbackRequested

TranslationBatchStarted
```

as hidden execution commands.

---

# 81. Errors

Exact Translation errors belong to:

```text
02-modules/translation/ERRORS.md
```

Architecture-level documents describe semantic failure categories only.

---

# 82. Runtime Error Separation

Do not redefine Translation equivalents of:

```text
AttemptCancelled

AttemptTimedOut

RetryExhausted

WorkItemSuperseded
```

Those remain Runtime-owned.

---

# 83. Diagnostics

Useful Translation observations include:

```text
TranslationUnit count

TranslationBatch count

context size

provider latency

validation warnings

provider fallback

cache reuse

late-result rejection

manual correction frequency

Glossary warnings

cost/token estimates
```

---

# 84. Privacy

Translation may process:

```text
private documents

authenticated web text

clipboard content

copyrighted reading material

personal corrections
```

Default principles:

```text
send minimum required target/context

avoid raw-content logs

respect local-only mode

protect credentials

bound retained context

support deletion/retention policy
```

---

# 85. Prompt Injection Boundary

Source and context content are untrusted data.

They must not override:

```text
system Translation instructions

privacy policy

security constraints

provider restrictions

structured-output schema
```

---

# 86. Credentials

Provider credentials must never enter:

```text
SourceDocumentArtifact

TranslationArtifact

TranslationUnit text

Translation Context semantic data

Event payload

Presentation state
```

They remain in secret/provider management.

---

# 87. Performance

Interactive Translation should prioritize:

```text
current visible/requested content

bounded batches

bounded context

cancellation responsiveness

limited concurrency

cache reuse

safe prefetch
```

---

# 88. Current Content Priority

Conceptually:

```text
current requested Translation
    ↓
explicit retranslation/correction
    ↓
near-future prefetch
    ↓
background Translation
```

Runtime owns actual scheduling.

---

# 89. Prefetch

Translation may semantically plan speculative near-future work.

Prefetch result is not current authority until relevant source content becomes current.

---

# 90. Source-Path Independence

After:

```text
SourceDocumentArtifact
```

Translation should not care whether source came from:

```text
OCR

DOM

plain text

clipboard

document parser
```

except through explicit semantic metadata.

---

# 91. Structured Text Flow

```text
Structured Source
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
```

No OCR is required.

---

# 92. Screen Comic Flow

```text
CaptureArtifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
```

Translation does not consume OCR provider internals.

---

# 93. Reading Session Boundary

Reading Session owns:

```text
SessionId

ReadingContext

ReadingContextRevision

reading lifecycle
```

Translation consumes compatible session-derived configuration/context references where required.

It does not own reading-session state.

---

# 94. Preferences Boundary

Persistent Translation preferences belong to:

```text
Preferences
```

Translation consumes an immutable effective configuration snapshot.

---

# 95. Translation Configuration

Possible settings:

```text
literal / balanced / natural

name policy

honorific policy

pronoun policy

SFX policy

custom terminology

custom Translation instructions

provider preference

privacy/local-only policy
```

---

# 96. Configuration Does Not Equal Context

Distinguish:

```text
Translation Configuration
    = how Translation should behave

Translation Context
    = information helping Translation
      understand current target
```

---

# 97. Current Folder Reading Order

Recommended:

```text
1. README.md

2. TRANSLATION.md

3. CONTEXT.md
```

---

# 98. Why This Order

`README.md` establishes:

```text
scope

ownership

boundaries

document relationships
```

Then:

```text
TRANSLATION.md
```

defines the complete Translation semantic lifecycle.

Finally:

```text
CONTEXT.md
```

deepens one important Translation sub-concern:

```text
context assembly
```

---

# 99. Files Intentionally Not Added

Current folder intentionally does not contain:

```text
PRESENTATION.md

PROVIDERS.md

RETRY.md

CANCELLATION.md

BATCHING.md

MEMORY.md

GLOSSARY.md
```

unless future complexity justifies a separate architecture document.

---

# 100. Why No PRESENTATION.md

Presentation already has its semantic module authority in:

```text
02-modules/presentation/
```

Keeping another Presentation authority under Translation would duplicate ownership.

---

# 101. Why No RETRY.md

Retry belongs to Runtime architecture.

Translation only classifies failure semantics.

---

# 102. Why No CANCELLATION.md

Cancellation mechanics belong to Runtime.

Translation/provider adapters only cooperate with cancellation.

---

# 103. Why No PROVIDERS.md Yet

Provider-specific architecture already belongs to:

```text
Provider Management

provider adapters

Translation provider ports
```

A Translation-local `PROVIDERS.md` should be created only if Translation-specific provider suitability becomes large enough to require separate architecture treatment.

---

# 104. Why No BATCHING.md Yet

TranslationBatch semantics are currently manageable inside:

```text
TRANSLATION.md
```

Create a separate file only if batching develops independent complexity such as:

```text
multi-level batching

adaptive batching

provider-specific planning matrices

complex batch dependency graph

separate compatibility/versioning model
```

---

# 105. Why No MEMORY.md Yet

Translation Memory ownership is still an open architecture decision.

It may become:

```text
Translation subdomain
```

or:

```text
Knowledge capability/module
```

Do not prematurely assign ownership through file placement.

---

# 106. Why No GLOSSARY.md Yet

Glossary ownership also remains broader than one Translation architecture file.

Translation consumes Glossary semantics but persistent Glossary ownership may belong elsewhere.

---

# 107. Architecture Invariants

1. Translation consumes Published SourceDocumentArtifact.

2. Translation owns TranslationUnit.

3. Translation owns TranslationBatch.

4. Translation owns Translation Context Assembly.

5. Translation owns TranslationArtifact semantics.

6. Text Processing does not own TranslationUnit.

7. Presentation does not own Translation semantics.

8. Runtime owns WorkItem.

9. Runtime owns Attempt.

10. Runtime owns retry execution.

11. Runtime owns cancellation mechanics.

12. Runtime owns deadlines and scheduling.

13. Runtime owns supersession.

14. Translation classifies semantic/provider failures.

15. Retry creates a new Attempt.

16. TranslationContextSnapshot is semantic context, not Runtime context.

17. TARGET and CONTEXT are separate.

18. Context must not become unexpected target output.

19. TranslationBatch is not WorkItem.

20. TranslationBatch is not Attempt.

21. Provider DTOs remain adapter-private.

22. Provider credentials never enter semantic Artifacts.

23. Local-only policy cannot silently fall back to cloud.

24. TranslationUnit remains traceable to SourceDocument semantics.

25. Generic architecture-wide Segment is deprecated.

26. Parallel execution does not change semantic order.

27. Provider completion does not imply publication.

28. Candidate TranslationArtifact requires semantic validation.

29. Candidate publication requires current authority.

30. Stale Translation cannot overwrite newer source authority.

31. Published TranslationArtifact is immutable.

32. Manual Translation correction creates new semantic authority.

33. Translation does not mutate SourceDocument truth.

34. Translation Memory is reusable candidate knowledge, not automatic truth.

35. Glossary is contextual terminology input, not blind post-processing.

36. Cache does not bypass semantic compatibility.

37. Event Bus does not orchestrate Translation execution.

38. Exact events belong to Translation EVENTS.md.

39. Exact errors belong to Translation ERRORS.md.

40. Presentation is outside the Translation architecture folder.

41. Translation remains source-path independent after SourceDocumentArtifact.

---

# 108. Deprecated Architecture

Deprecated:

```text
Segmentation
    ↓
Translation Context
    ↓
Translation
    ↓
Presentation
```

as independent pipeline layers.

Current:

```text
SourceDocumentArtifact
    ↓
Translation
    ├── TranslationUnit
    ├── TranslationBatch
    ├── Context Assembly
    └── TranslationArtifact
        ↓
Presentation module
```

---

# 109. Deprecated Generic Segment Model

Deprecated:

```text
Text Segment
    ↓
Translation Segment
    ↓
Presentation Segment
```

Current:

```text
SourceDocument Node
    ↓
TranslationUnit
    ↓
PresentationItem
```

Each concept belongs to its semantic owner.

---

# 110. Deprecated Translation-Owned Runtime State

Deprecated:

```text
Translation
    owns retry count

Translation
    owns retry backoff

Translation
    owns cancellation lifecycle

Translation
    owns timeout lifecycle
```

Current:

```text
Translation
    classifies

Runtime
    executes
```

---

# 111. Deprecated Generic Revision Model

Avoid one generic:

```text
requestRevision

contextVersion

translationRevision
```

as the universal correctness model.

Use typed authority such as:

```text
SourceDocumentArtifactId

TranslationArtifact lineage

Translation configuration revision

Glossary revision

RuntimeRevisionId
```

---

# 112. Preserved Architecture Strengths

The Translation architecture intentionally preserves:

```text
provider independence

Translation strategy

TranslationBatch

target/context isolation

structured output

identifier-based mapping

provider response validation

Chinese → Vietnamese specialization

novel/comic strategies

Glossary integration

Translation Memory

manual corrections

cache compatibility

provider privacy constraints

local-only enforcement

partial result awareness

source-target traceability

prompt/source security separation
```

---

# 113. Relationship to Text Architecture

Text Architecture defines:

```text
SourceDocumentArtifact

source-language semantic structure

Paragraph / Sentence / Span

source mapping

segmentation
```

Translation consumes that semantic structure.

It does not redefine it.

---

# 114. Relationship to Presentation

Presentation consumes:

```text
Published TranslationArtifact
```

and owns:

```text
PresentationSnapshot

RenderPlan

PresentationItem

PresentationMode

presentation layout/readability
```

Presentation specifications live in:

```text
02-modules/presentation/
```

---

# 115. Relationship to Runtime

Runtime owns execution mechanics.

Translation may provide Runtime with:

```text
semantic dependencies

provider requirements

priority hints

cost hints

failure classifications
```

but does not own the scheduler.

---

# 116. Relationship to Provider Management

Provider Management may own:

```text
provider registration

provider availability

provider health

provider configuration

credential association
```

Translation determines whether a provider is semantically suitable for the current Translation requirements.

---

# 117. Relationship to Reading Session

Reading Session may provide:

```text
current document/chapter identity

session-scoped overrides

safe reading context

current source authority
```

Translation consumes compatible snapshots/references.

---

# 118. Relationship to Preferences

Preferences owns persistent user choices.

Translation consumes effective immutable Translation configuration.

---

# 119. Relationship to Diagnostics

Diagnostics may observe:

```text
Translation latency

provider selection

fallback

validation failure

context size

cache behavior

late-result rejection

manual correction rate
```

It does not control Translation semantics.

---

# 120. MVP Translation Scope

Recommended MVP:

```text
Chinese → Vietnamese

SourceDocumentArtifact input

TranslationUnit construction

TranslationBatch construction

basic bounded Context Assembly

one usable Translation provider

provider abstraction

stable source-target mapping

structured output where possible

semantic validation

Runtime cancellation cooperation

Runtime failure classification

stale-result protection

basic Translation cache

manual Translation correction
```

---

# 121. MVP Context Scope

MVP context should support:

```text
direct source neighbors

chapter/document identity

basic Glossary

Translation configuration

target/context isolation

bounded context

context provenance
```

Recent accepted Translation history is useful but may remain optional.

---

# 122. MVP Does Not Require

```text
automatic provider benchmarking

advanced Translation Memory

character relationship graph

multi-provider consensus

automatic Glossary learning

cross-book retrieval

AI context ranking

adaptive provider routing

objective literary quality scoring

automatic Translation alternatives
```

---

# 123. Future Extensions

Potential future capabilities:

```text
multi-provider routing

local-first hybrid Translation

advanced Translation Memory

automatic Glossary suggestions

character-specific Translation profiles

semantic retrieval

cross-chapter context

Translation alternatives

AI-assisted revision

adaptive batching

cost-aware routing

quality estimation

offline model packages
```

---

# 124. Future Extensions Must Preserve Boundaries

Future work must preserve:

```text
SourceDocumentArtifact input

TranslationUnit ownership

Translation Context ownership

TranslationArtifact output

Runtime Attempt ownership

provider adapter isolation

Candidate → Published authority
```

unless architecture explicitly changes.

---

# 125. Open Decisions

The following remain open:

```text
final TranslationUnit schema

final TranslationArtifact schema

TranslationUnit identity strategy

TranslationPlan schema/lifetime

partial TranslationArtifact publication model

provisional/streaming semantic model

Glossary owner

Translation Memory owner

Knowledge owner

character-context model

provider-selection authority split

provider fallback policy

context budget model

context cache model

Translation cache compatibility model

manual correction persistence scope

cross-chapter context strategy

prompt/profile versioning

Translation quality-estimation model
```

---

# 126. Completion Criteria

The Translation Architecture set is synchronized when:

* README identifies Translation as owner;
* SourceDocumentArtifact is the canonical input;
* TranslationArtifact is the canonical output;
* Translation owns TranslationUnit;
* Translation owns TranslationBatch;
* Translation owns Context Assembly;
* Context is not a standalone pipeline layer;
* generic Segment authority is removed;
* Runtime owns WorkItem/Attempt/retry/cancellation;
* provider adapters remain isolated;
* TARGET and CONTEXT remain separated;
* TranslationArtifact publication follows Candidate → authority validation;
* stale Translation cannot overwrite newer source authority;
* manual corrections remain first-class semantic data;
* Presentation is no longer defined inside `translate/`;
* folder contains only current Translation architecture documents.

---

# 127. Summary

The Translation Architecture currently consists of:

```text
README.md
    → scope, ownership and boundaries

TRANSLATION.md
    → complete Translation semantic architecture

CONTEXT.md
    → Translation-owned context assembly
```

Canonical flow:

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
Provider Interaction
        ↓
Translation Validation
        ↓
TranslationArtifact Candidate
        ↓
Runtime Authority Validation
        ↓
Published TranslationArtifact
        ↓
Presentation
```

The essential ownership model is:

```text
Text Processing
    owns source-language structure

Translation
    owns target-language meaning

Runtime
    owns execution

Presentation
    owns readable semantic presentation
```

The central Translation rule is:

```text
Translation decides
what source content means
in the target language.

Context helps Translation
understand that content.

Runtime decides
how the work executes.

Provider success alone
does not create authority.
```
