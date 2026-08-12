# AI Context

* **Document:** AI Architecture / Context
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI assembles, filters, prioritizes and packages contextual information for one AI operation.

AI Context exists to provide an AI model with the **minimum relevant information required to perform the requested capability correctly**.

Context Assembly MUST remain:

* provider-neutral,
* capability-aware,
* explicit,
* bounded,
* reproducible where required,
* policy-aware,
* privacy-aware,
* observable.

The Context architecture does NOT own canonical business truth.

It consumes already-authorized domain state, immutable snapshots, explicit operation inputs and controlled retrieval results.

---

# Core Principle

```text
Business / Domain State
        |
        v
Resolution / Snapshotting
        |
        v
Explicit Context Inputs
        |
        v
Context Assembly
        |
        v
AI Context Package
        |
        v
Prompt / Input Construction
```

Context Assembly transforms available context into an AI-consumable package.

It MUST NOT become a hidden universal business-state resolver.

---

# Scope

AI Context MAY support operations such as:

* Translation,
* Character inference,
* semantic validation,
* summarization,
* classification,
* language analysis,
* vision analysis,
* structured extraction.

Different capabilities require different context.

Therefore:

```text
AI Context
    !=
one universal context bundle
```

---

# Context Is Capability-Specific

Example:

```text
Translation
    may require
    source text
    target language
    GlossarySnapshot
    CharacterContextSnapshot
    nearby dialogue
```

while:

```text
Language Detection
    may require
    only source text
```

and:

```text
Vision Analysis
    may require
    image
    expected language hints
    layout hints
```

Context requirements MUST be defined per capability.

---

# Non-Goals

AI Context does NOT own:

* Project state,
* Session state,
* Character truth,
* Glossary truth,
* Profile truth,
* Translation history,
* Memory truth,
* OCR results,
* provider conversations,
* runtime stage history.

It references or consumes those resources where explicitly required.

---

# Design Principles

Context construction SHOULD be:

* explicit,
* deterministic for identical resolved inputs,
* provider-neutral,
* capability-specific,
* token/resource efficient,
* bounded,
* traceable,
* privacy-preserving,
* spoiler-aware where applicable,
* stable for durable execution.

---

# Context Pipeline

Recommended logical flow:

```text
Context Requirements
        |
        v
Resolve Context Sources
        |
        v
Authorize / Validate References
        |
        v
Materialize Required Context
        |
        v
Normalize
        |
        v
Deduplicate
        |
        v
Score / Prioritize
        |
        v
Budget
        |
        v
Compress / Reduce if required
        |
        v
Validate
        |
        v
AI Context Package
```

Not every operation requires every step.

---

# Context Requirements

Before collecting context, the operation SHOULD define what kinds of context are relevant.

Recommended:

```text
AIContextRequirements
├── requiredSources[]
├── optionalSources[]
├── maximumContextBudget?
├── historyPolicy?
├── characterPolicy?
├── glossaryPolicy?
├── memoryPolicy?
├── spoilerPolicy?
└── reductionPolicy?
```

Requirements SHOULD derive from:

* AI capability,
* Resolved Configuration,
* Context Profile,
* output contract,
* applicable policy.

---

# Explicit Context Sources

Preferred context sources include:

```text
AI Request Input
GlossarySnapshot
CharacterContextSnapshot
ResolvedConfigurationSnapshot
SessionContextSnapshot
OperationContextSnapshot
TextBlock Revisions
Translation Revisions
Content References
Memory Retrieval Results
Capability-Specific Snapshots
```

The Context Builder SHOULD consume explicit references rather than querying arbitrary mutable state.

---

# Mutable Source Boundary

Avoid:

```text
load current Glossary
load current Character
load latest Profile
load current Session state
```

during durable AI execution.

Prefer:

```text
GlossarySnapshot
CharacterContextSnapshot
exact Profile / Resolved Configuration snapshot
SessionContextSnapshot
exact TextBlock Revision
```

---

# Context Materialization

A Request MAY carry references rather than full content.

Context Assembly materializes those references into a bounded package.

Example:

```text
GlossarySnapshot ID
        |
        v
Selected terminology entries

CharacterContextSnapshot ID
        |
        v
Relevant character context

TextBlock Revision IDs
        |
        v
Source / nearby text
```

Materialization MUST preserve provenance.

---

# Context Package

Recommended:

```text
AIContextPackage
├── contextPackageId?
├── capabilityType
├── primaryInputContext
├── languageContext?
├── glossaryContext?
├── characterContext?
├── historyContext?
├── memoryContext?
├── sessionContext?
├── contentContext?
├── userInstructionContext?
├── policyContext?
├── provenance[]
├── reductionMetadata?
├── budgetMetadata?
└── contentHash?
```

Not every section is required.

---

# Primary Input Context

Primary input is the semantic material the model is expected to operate on.

Examples:

* current TextBlock Revision,
* selected source text,
* Image reference/materialization,
* structured data.

Primary input MUST remain distinguishable from auxiliary context.

---

# Primary Input vs Supporting Context

Critical distinction:

```text
Primary Input
    = content whose result is being produced

Supporting Context
    = information helping interpret the Primary Input
```

Example:

```text
Current dialogue
    = primary input

Previous dialogue
    = supporting context
```

This distinction helps prevent accidental output mapping errors.

---

# Language Context

Language Context MAY contain:

* source Language,
* target Language,
* Script,
* mixed-language analysis,
* writing-direction hints,
* operation-specific language rules.

All Language values MUST use canonical Language-domain representation.

Provider-specific language codes MUST NOT appear.

---

# Glossary Context

Glossary Context MUST derive from an immutable:

```text
GlossarySnapshot
```

or another explicit terminology snapshot.

Possible materialized fields:

```text
GlossaryContext
├── applicableEntries[]
├── sourceForms[]
├── targetForms[]
├── ruleTypes[]
├── authority?
└── conflictMetadata?
```

Context Assembly MUST NOT independently redefine Glossary precedence.

Glossary-domain resolution determines effective terminology.

---

# Character Context

Character Context MUST derive from:

```text
CharacterContextSnapshot
```

when durable character-dependent behavior matters.

Possible information:

* confirmed speaker,
* listener,
* character identity,
* relevant names,
* relationship,
* speech profile,
* address rules,
* spoiler-safe story facts.

Character Context MUST NOT own Translation terminology rules.

Preferred translated terminology belongs to Glossary context where terminology enforcement is required.

---

# Character vs Glossary Context

Example:

```text
Character Context:
    speaker = CH-001
    listener = CH-002
    relationship = master -> disciple
```

Glossary Context:

```text
师父 -> sư phụ
rule = TRANSLATE
```

The Context Builder may supply both.

It MUST NOT collapse them into one mutable "character glossary".

---

# Session Context

Session Context represents explicit working-context information relevant to an AI operation.

Examples:

* current logical location,
* temporary operation intent,
* spoiler boundary,
* selected user mode,
* temporary approved overrides,
* presentation/reading hints where relevant.

For durable operations, mutable Session state SHOULD be captured first into:

```text
SessionContextSnapshot
```

or another immutable operation-specific snapshot.

---

# Session Lifetime

Session context is NOT restricted to one active in-memory reading process.

A Session may be:

* persisted,
* paused,
* resumed,
* recovered,
* handed off between devices.

AI execution MUST NOT rely on process-local Session state.

---

# Session ID Boundary

```text
sessionId
```

is not sufficient semantic context.

If Session state affects output, the actual relevant values MUST be captured explicitly.

---

# Historical Context

Historical Context MAY contain:

* previous TextBlock Revisions,
* previous Translation Revisions,
* nearby dialogue,
* previous narration,
* Chapter summary,
* terminology usage,
* prior relevant decisions.

History MUST remain bounded.

---

# History Is Not Runtime History

AI Historical Context refers to content/business history.

It MUST NOT include runtime execution history such as:

* retry attempts,
* provider attempts,
* stage timeline,
* queue events.

Those belong to observability/runtime.

---

# History Policy

Context Profile MAY define:

```text
HistoryPolicy
├── previousBlocks?
├── previousTranslations?
├── maximumItems?
├── maximumDistance?
├── chapterBoundary?
├── summaryAllowed?
└── spoilerBoundary?
```

The Context Builder applies the resolved policy.

---

# Previous Translation Context

Previous Translation MAY help consistency.

However, historical translated text MUST reference exact Translation Revisions where reproducibility matters.

Avoid ambiguous:

```text
latest translation
```

during durable operations unless resolution occurs before Context Assembly.

---

# Memory Context

Memory is an optional explicit context source.

Possible inputs:

```text
MemoryRetrievalResult
MemorySnapshot
MemorySelectionResult
```

The Context Builder MUST NOT read arbitrary hidden Memory.

Memory use MUST be:

* authorized,
* explicitly requested or configured,
* scoped,
* bounded,
* provenance-aware.

---

# Memory vs Domain Truth

Memory is supporting context.

It MUST NOT silently override authoritative:

* Glossary,
* Character,
* Language,
* Profile,
* Project,
* user-confirmed domain state.

If Memory conflicts with authoritative context, the conflict SHOULD be surfaced or lower-authority Memory discarded.

---

# User Instructions

Explicit operation-specific user instructions MAY be included.

Examples:

* keep this term untranslated,
* use literal translation for this operation,
* summarize instead of translate,
* preserve ambiguity.

User instructions MUST still respect:

* mandatory Workspace Policy,
* safety constraints,
* domain authority,
* protected revisions.

Therefore:

```text
User Instruction
    !=
universally highest authority
```

---

# User Instruction Precedence

The previous global rule:

```text
explicit user instruction always wins
```

is too broad.

Recommended principle:

```text
Mandatory Policy / Protected Domain Authority
        >
Explicit User Operation Intent
        >
Lower-Authority Inferred Context
```

Exact precedence remains capability-specific.

---

# Project Context

Project MAY contribute explicit information such as:

* Project metadata,
* Project language defaults,
* Project-scoped resource selections,
* Project style intent.

Context Builder SHOULD receive already-resolved values or explicit references.

It SHOULD NOT become responsible for Project inheritance rules.

---

# Page Context

Page MAY be useful for image-oriented operations.

Examples:

* neighboring TextBlocks,
* layout reference,
* visual order,
* Page metadata.

Page MUST remain optional.

Text-native content MUST NOT require Page context.

---

# Content Context

Content Context MAY include:

```text
Book?
Chapter?
Page?
TextBlock?
Image?
Document section?
```

Only the relevant structural levels SHOULD be included.

---

# Conversation History

Provider conversation history MUST NOT be used as implicit canonical context.

If conversation-like history is required, it SHOULD be represented as provider-neutral content/history context.

```text
CRAI Historical Context
    !=
Provider Conversation Thread
```

---

# Plugin / Extension Context

Extensions MAY contribute context through explicit registered contracts.

Recommended:

```text
ExtensionContextItem
├── namespace
├── sourceReference
├── authority
├── scope
├── payload
├── contentHash?
└── expiresAt?
```

Plugin-provided context MUST NOT bypass:

* authorization,
* Workspace isolation,
* policy,
* context budget,
* domain authority.

---

# Context Authority

Every context source SHOULD have an authority classification where conflicts are possible.

Example:

```text
AUTHORITATIVE
CONFIRMED
EXPLICIT_USER_INTENT
CONFIGURED
DERIVED
INFERRED
MEMORY
EXTERNAL
UNKNOWN
```

Authority semantics MAY vary by context type.

---

# Authority Is Not Priority

Authority and contextual relevance are different.

Example:

```text
Project Glossary Entry
    high authority
    but irrelevant to current source text
```

should not consume context budget merely because it is authoritative.

Therefore selection considers both:

```text
Authority
+
Relevance
```

---

# Context Prioritization

There MUST NOT be one universal priority list for all AI operations.

Priority SHOULD be determined by:

* capability,
* context type,
* authority,
* relevance,
* scope specificity,
* temporal/content proximity,
* explicit user intent,
* spoiler policy,
* context budget.

---

# Context Score

A conceptual ranking MAY consider:

```text
ContextScore
    =
relevance
+
authority
+
scope specificity
+
recency / content proximity
+
operation importance
-
cost
-
privacy risk
```

The exact algorithm remains implementation-specific.

Deterministic rules SHOULD be preferred where authority semantics matter.

---

# Mandatory Context

Some context MUST NOT be dropped merely due to budget.

Examples MAY include:

* Primary Input,
* target Language,
* mandatory safety instructions,
* applicable locked terminology,
* required output schema,
* mandatory Policy constraints.

Such context SHOULD be classified:

```text
REQUIRED
```

---

# Optional Context

Examples:

* older dialogue,
* optional story summary,
* secondary Character notes,
* low-confidence Memory,
* distant chapter history.

Optional context MAY be reduced first.

---

# Context Budget

Context Assembly MUST respect resource limits.

Budget MAY include:

```text
maximumTokens
maximumCharacters
maximumItems
maximumImages
maximumBytes
providerContextLimit
reservedOutputBudget
```

Token is not the only possible unit.

---

# Provider-Neutral Budgeting

Context logic SHOULD express a provider-neutral budget where possible.

Route/model selection may later expose a concrete context-window limit.

The final budget may therefore be constrained by:

```text
Resolved Context Policy
+
Selected Model Capability
```

---

# Budget Timing

Initial context selection MAY occur before Route Planning using general operation limits.

After model selection, the pipeline MAY perform a final compatibility/budget check if exact model limits differ.

This MUST NOT silently change required semantic context.

If required context cannot fit, routing or operation policy may need to change.

---

# Context Reduction

When context exceeds budget, reduction MAY include:

1. remove duplicates,
2. remove irrelevant items,
3. drop expired context,
4. drop low-authority inferred context,
5. reduce distant history,
6. summarize eligible history,
7. compact equivalent metadata,
8. choose higher-relevance terminology/context.

Required context MUST remain protected.

---

# Truncation

Blind truncation SHOULD be avoided.

Bad:

```text
take first N tokens
```

Preferred:

```text
remove / compress by semantic policy
```

---

# Deduplication

Equivalent context SHOULD not be repeated.

Possible duplicate signals:

* same stable reference,
* same Revision,
* same Snapshot item,
* same content hash,
* same semantic terminology rule.

Deduplication MUST preserve authority/provenance.

---

# Merge

Context Merge combines compatible items.

Merge MUST NOT erase conflicting authority.

Example:

```text
two identical terminology entries
```

may be compacted.

But:

```text
two conflicting Character facts
```

must not be silently merged into one truth.

---

# Conflict

Context conflict MAY occur when sources disagree.

Examples:

* Memory conflicts with Glossary,
* inferred Character conflicts with confirmed Character,
* old Translation conflicts with new locked terminology,
* Project default conflicts with explicit allowed Session override.

Conflicts SHOULD be resolved by the owning domain/resolver where possible before Context Assembly.

Remaining conflicts MUST be explicit.

---

# Context Compression

Compression MAY reduce historical context.

Examples:

* summarize old dialogue,
* collapse repeated terminology,
* compact Character notes,
* replace multiple history items with approved summary.

Compression MUST preserve relevant semantic intent.

---

# Compression Provenance

A compressed item SHOULD retain:

```text
sourceReferences
compressionMethod
compressorVersion
inputHash
outputHash
```

where reproducibility matters.

---

# AI-Assisted Compression

AI-assisted context compression MAY be used.

If used for durable operations:

* its output SHOULD be immutable/versioned where material,
* provenance SHOULD be preserved,
* failure SHOULD not silently corrupt context,
* spoiler and privacy constraints still apply.

MVP MAY defer AI-assisted compression.

---

# Summary Context

Summaries SHOULD be treated as derived context, not canonical source truth.

A Chapter Summary MUST NOT replace exact source content where exact wording is required.

---

# Spoiler Safety

Context selection MUST respect story/reader knowledge boundaries.

For normal reading Translation:

```text
future Character identity
future relationship
future title
future death
future plot information
```

MUST NOT enter the Context Package before its allowed reveal boundary.

---

# Spoiler Context Sources

Potential spoiler-sensitive sources include:

* CharacterContextSnapshot,
* Character relationships,
* future Chapter summaries,
* Project-wide Memory,
* future Translation history,
* external wiki-like extensions.

Context Assembly MUST apply the configured spoiler policy.

---

# Privacy

Context may contain sensitive or copyrighted material.

Context Assembly SHOULD:

* minimize external-provider payload,
* exclude irrelevant content,
* exclude private notes unless required,
* respect Workspace Policy,
* honor local-only processing,
* avoid cross-Workspace retrieval.

---

# Tenant Isolation

Every context item MUST remain attributable to an allowed Workspace/domain scope.

Context from another Workspace MUST NOT be included unless an explicit sharing mechanism and authorization permit it.

---

# Sensitive Context Classification

Context items MAY carry sensitivity metadata.

Example:

```text
PUBLIC
INTERNAL
CONFIDENTIAL
RESTRICTED
LICENSED
PERSONAL
```

Policy may constrain whether each classification can leave local execution.

---

# Context Validation

Validation SHOULD include:

* schema validity,
* reference validity,
* Workspace scope compatibility,
* Language validity,
* required-context presence,
* context budget,
* authority consistency,
* spoiler boundary,
* policy compatibility,
* duplicate/conflict checks,
* content-integrity checks where available.

---

# Invalid Context

Invalid required context SHOULD fail the operation before Prompt Construction.

Invalid optional context MAY be:

* discarded,
* replaced by valid fallback context,
* surfaced as warning,

according to policy.

---

# Missing Context

Missing context is not always an error.

Examples:

```text
Character Context missing
```

may be acceptable for generic Translation.

But:

```text
required target Language missing
```

is not.

Required/optional semantics MUST be explicit.

---

# Context Degradation

A Context Package MAY be marked degraded when optional context is unavailable.

Possible metadata:

```text
ContextQuality
├── COMPLETE
├── DEGRADED
└── MINIMAL
```

The calling pipeline may:

* continue,
* use stronger model,
* request user action,
* reject,

according to operation policy.

---

# Determinism

For identical:

* context requirements,
* immutable input references,
* resolver versions,
* model-independent budget constraints,
* policy state,

Context Assembly SHOULD produce semantically equivalent Context Packages.

---

# Determinism Boundary

Determinism does NOT mean byte-identical output when:

* probabilistic summarization is used,
* external retrieval changes,
* runtime model constraints differ.

Such non-deterministic dependencies MUST be explicit and versioned where relevant.

---

# Context Hash

A finalized Context Package MAY expose:

```text
contextHash
```

derived from semantic context content and relevant ordering.

This MAY participate in:

* cache identity,
* reproducibility,
* debugging,
* request provenance.

---

# Context Item

Recommended generic representation:

```text
AIContextItem
├── itemId
├── contextType
├── sourceReference
├── sourceRevision?
├── authority
├── relevance?
├── scope
├── payload
├── language?
├── sensitivity?
├── required?
├── createdAt?
└── contentHash?
```

Not every implementation must persist this structure literally.

---

# Context Provenance

Every material context item SHOULD be traceable to its source.

Examples:

```text
Glossary Entry Revision
Character Revision
TextBlock Revision
Translation Revision
Memory Entry
Session Override
Project Configuration
```

Opaque unexplained context SHOULD be avoided.

---

# Context Ordering

Ordering SHOULD be deterministic where order affects prompt/input construction.

Recommended ordering may use:

* context type,
* authority,
* source order,
* content proximity,
* relevance score,
* stable tie-breaker.

Prompt Builder MAY later reorganize context into a model-facing representation.

---

# Context vs Prompt

Critical boundary:

```text
Context
    = structured semantic information

Prompt
    = model-facing instruction/input representation
```

Context MUST remain provider-neutral.

Prompt Builder determines how context is represented to a model.

---

# Context Package Is Not Prompt

Example Context:

```text
Glossary:
    灵力 -> linh lực

Speaker:
    CH-001

Relationship:
    master -> disciple

Target Language:
    vi
```

Prompt Builder may encode that as:

* messages,
* XML-like sections,
* JSON,
* tool schema,
* provider-native structure.

Context itself does not care.

---

# Context vs AI Request

`AIRequest` carries:

* capability,
* input references,
* context references,
* configuration references,
* output requirements.

Context Assembly materializes those references into:

```text
AIContextPackage
```

Therefore:

```text
AIRequest
    !=
AIContextPackage
```

---

# Context vs Memory

Memory is a possible retrieval source.

Context is the selected material actually supplied to the current operation.

```text
Memory
    large persistent/retrievable pool

Context
    bounded operation-specific selection
```

---

# Context vs Session

Session owns mutable working context.

AI Context is operation-specific.

Changing Session after Context Package finalization MUST NOT change that package.

---

# Context vs Domain Snapshots

GlossarySnapshot and CharacterContextSnapshot already contain domain-resolved truth.

Context Assembly may select/materialize their relevant content.

It MUST NOT rebuild their semantic resolution rules.

---

# Context vs Runtime Metadata

Runtime metadata such as:

* retry count,
* provider latency,
* worker ID,
* queue state,
* stage history,

MUST NOT be ordinary semantic Context.

Operational metadata belongs to observability/runtime.

---

# Observability

Context observability MAY record:

* item count,
* context types,
* estimated size,
* selected/dropped item counts,
* compression ratio,
* reduction reason,
* context build latency,
* final context hash,
* Context Quality.

---

# Sensitive Observability

Observability MUST NOT log raw context text by default.

Prefer:

```text
contextHash
itemCount
typeCount
sourceReferenceCount
size
```

---

# Context Selection Trace

For explainability/debugging, CRAI MAY retain:

```text
ContextSelectionTrace
├── candidateReference
├── selected
├── reason
├── relevance?
├── authority?
├── reductionReason?
└── policyReference?
```

This SHOULD avoid duplicating raw content.

---

# Failure Handling

Possible context failures include:

```text
CONTEXT_REFERENCE_INVALID
CONTEXT_REQUIRED_SOURCE_MISSING
CONTEXT_SCOPE_VIOLATION
CONTEXT_POLICY_DENIED
CONTEXT_SPOILER_VIOLATION
CONTEXT_CONFLICT_UNRESOLVED
CONTEXT_BUDGET_EXCEEDED
CONTEXT_COMPRESSION_FAILED
CONTEXT_MATERIALIZATION_FAILED
CONTEXT_LANGUAGE_INVALID
```

---

# Recovery

Possible recovery strategies:

* drop invalid optional context,
* use minimal context,
* reduce history,
* use deterministic compression,
* select another compatible route with larger context window,
* request user resolution,
* fail before provider execution.

Recovery MUST NOT silently discard required authoritative context.

---

# Architecture Invariants

1. Context is assembled before Prompt/Input Construction.

2. Context Assembly is provider-neutral.

3. AI Context is capability-specific.

4. There is no mandatory universal Context structure for every capability.

5. Context Builder MUST NOT become a universal business-state resolver.

6. Durable operations SHOULD consume explicit immutable or resolved context references.

7. Mutable current Glossary MUST NOT be read implicitly after durable context resolution.

8. Mutable current Character state MUST NOT be read implicitly after durable context resolution.

9. Mutable Session state affecting output SHOULD cross a snapshot boundary.

10. Session ID alone is not sufficient semantic context.

11. Context Assembly MUST preserve provenance.

12. Primary Input and Supporting Context remain distinct.

13. Glossary Context derives from Glossary-domain resolved state.

14. Character Context derives from Character-domain resolved state.

15. Character Context MUST NOT own Glossary Translation terminology rules.

16. Memory is optional supporting context, not canonical domain truth.

17. Memory participation MUST be explicit and bounded.

18. User instructions do not universally override mandatory policy or protected domain authority.

19. Context priority is capability-specific.

20. CRAI MUST NOT use one universal global priority list.

21. Context relevance and authority remain distinct concepts.

22. Required context MUST NOT be dropped merely because optional context consumes budget.

23. Blind truncation SHOULD be avoided.

24. Context reduction SHOULD follow semantic policy.

25. Context optimization MUST preserve required semantic intent.

26. Context conflict MUST NOT be silently merged when authority differs materially.

27. Context history remains separate from runtime execution history.

28. Provider conversation history MUST NOT become implicit canonical Context.

29. Page Context is optional.

30. Book Context is optional.

31. Text-native AI operations MUST NOT require Page.

32. All Language values use canonical Language representation.

33. Provider-specific Language codes MUST NOT enter Context.

34. Plugin/extension context MUST obey authorization and Workspace isolation.

35. Context MUST respect spoiler boundaries.

36. Context MUST respect Workspace/privacy policy.

37. Cross-Workspace context requires explicit authorized sharing.

38. Sensitive Context SHOULD be minimized before external execution.

39. Context validation occurs before Prompt generation where required.

40. Invalid required Context MUST NOT silently continue.

41. Missing optional Context MAY produce degraded execution.

42. Context degradation SHOULD be explicit.

43. Identical resolved inputs SHOULD produce semantically equivalent deterministic Context where deterministic components are used.

44. Probabilistic compression/retrieval MUST be explicit.

45. Context Package MAY expose a semantic hash.

46. Context ordering SHOULD be deterministic where order affects model input.

47. Context Package is not Prompt.

48. AI Request is not Context Package.

49. Memory is not Context Package.

50. Session is not Context Package.

51. Runtime telemetry MUST NOT become ordinary semantic Context.

52. Context observability MUST avoid logging raw sensitive content by default.

53. Context reduction and selection SHOULD be explainable.

54. Required authoritative context MUST NOT be dropped to satisfy cost alone.

55. Route Planning MAY need to choose a larger-context model when required context cannot fit.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* capability-specific Context requirements,
* TextBlock Revision context,
* canonical source/target Language,
* GlossarySnapshot context,
* CharacterContextSnapshot context,
* optional SessionContextSnapshot,
* previous TextBlock context,
* previous Translation Revision context,
* explicit user operation instructions,
* provider-neutral AIContextPackage,
* required vs optional context,
* deterministic deduplication,
* simple relevance ordering,
* explicit authority classification,
* simple content-distance priority,
* token/character budget,
* reserved output budget,
* deterministic context reduction,
* bounded dialogue history,
* spoiler-safe Character context,
* Workspace isolation,
* context validation,
* context hash,
* selection/drop metadata,
* context build observability.

MVP MAY defer:

* AI-assisted context compression,
* embedding-based retrieval,
* semantic relevance ranking,
* plugin context,
* complex Memory,
* adaptive context windows,
* automatic summary hierarchy,
* cross-Chapter semantic retrieval,
* graph-based Character retrieval,
* provider-specific context optimization,
* dynamic context learning,
* long-term context-selection analytics.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact `AIContextPackage` schema,
* whether Context Package is persisted,
* exact Context Item abstraction,
* whether context materialization occurs fully before routing,
* how final model context-window constraints feed back into Context Assembly,
* exact required/optional context classification,
* Context Profile schema,
* authority taxonomy,
* relevance-scoring model,
* context-distance model,
* history window size,
* Chapter-boundary policy,
* whether prior Translation is preferred over prior source text,
* summary retention model,
* AI-assisted compression,
* deterministic compression formats,
* Memory integration,
* retrieval architecture,
* extension context model,
* context hashing,
* Context Selection Trace retention,
* spoiler-boundary enforcement location,
* privacy/redaction integration,
* context classification,
* context item ordering,
* how conflicting user instructions and locked terminology are surfaced,
* degraded-context policy,
* route escalation when context does not fit,
* local-vs-cloud context-size differences.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `STAGES.md`
* `REQUEST.md`
* `RESPONSE.md`
* `PROMPTS.md`
* `MEMORY.md`
* `MODELS.md`
* `ROUTING.md`
* `STREAMING.md`
* `RETRY.md`
* `FALLBACK.md`
* `COST_CONTROL.md`
* `CACHE.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/LANGUAGE.md`
* `../domain/GLOSSARY.md`
* `../domain/CHARACTER.md`
* `../domain/PROFILE.md`
* `../domain/SESSION.md`
* `../domain/TRANSLATION.md`
* `../domain/WORKSPACE.md`

Modules:

* `../../02-modules/translation/`
* `../../02-modules/reading-session/`
* `../../02-modules/preferences/`
* `../../02-modules/provider-management/`

Runtime:

* `../runtime/BUSINESS_PIPELINE_ORCHESTRATION.md`
* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
