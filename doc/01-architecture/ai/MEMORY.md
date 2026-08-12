# AI Memory

* **Document:** AI Architecture / Memory
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the provider-neutral memory architecture used by CRAI AI capabilities.

AI Memory provides reusable contextual knowledge across AI operations when that knowledge is:

* useful beyond one immediate request,
* not already canonical business truth,
* explicitly retrievable,
* bounded,
* provenance-aware,
* policy-compliant.

Memory may help:

* preserve contextual continuity,
* reduce repeated reasoning,
* retrieve prior relevant information,
* support summarization,
* support long-running reading workflows,
* improve context efficiency.

AI Memory MUST NOT become a duplicate owner of canonical CRAI domain state.

---

# Core Principle

```text
Canonical Domain Truth
        |
        +--> explicit references / projections
        |
        v
Derived / Learned Memory
        |
        v
Memory Retrieval
        |
        v
Context Selection
        |
        v
AI Context Package
```

Memory supports AI execution.

It does not replace domain ownership.

---

# Critical Boundary

The following are NOT AI Memory merely because AI may use them:

```text
Character identity
Glossary Entries
Profile Revisions
Project configuration
Session resume state
Translation Revisions
Language configuration
Workspace Policy
```

These remain canonical domain/application state.

Memory MAY reference or derive summaries from them.

It MUST NOT become their authoritative owner.

---

# AI Memory vs Domain Truth

Examples:

```text
CharacterRevision
    = authoritative Character truth
```

while:

```text
Character Summary Memory
    = derived compact context
```

Likewise:

```text
GlossarySnapshot
    = authoritative resolved terminology
```

while:

```text
Terminology Usage Memory
    = derived information about prior usage
```

and:

```text
ProfileRevision
    = authoritative processing intent
```

while:

```text
Preference Pattern Memory
    = learned suggestion
```

The second category MUST NOT silently override the first.

---

# Scope

AI Memory MAY support:

* Translation context,
* long-range story continuity,
* prior dialogue retrieval,
* contextual summarization,
* prior decision retrieval,
* user correction patterns,
* recurring terminology usage,
* prior AI observations,
* retrieval augmentation.

Different AI capabilities MAY use different Memory sources.

---

# Non-Goals

AI Memory does NOT own:

* Session lifecycle,
* Character canonical facts,
* Glossary terminology rules,
* Profile definitions,
* User Preferences,
* Project settings,
* Translation history,
* Workspace policy,
* authorization,
* provider conversation state,
* cache entries,
* runtime attempt history.

---

# Design Principles

Memory SHOULD be:

* provider-independent,
* explicit,
* provenance-aware,
* authority-aware,
* scoped,
* privacy-first,
* bounded,
* retrievable,
* versioned where necessary,
* observable,
* disposable when derived,
* rebuildable where possible.

---

# Memory Architecture

Recommended conceptual architecture:

```text
Canonical Sources
       |
       v
Memory Producers
       |
       v
Memory Store
       |
       v
Memory Retrieval
       |
       v
Memory Selection Result
       |
       v
Context Assembly
       |
       v
AI Context Package
```

The AI Pipeline MUST NOT directly query arbitrary storage.

---

# Memory Manager Boundary

A Memory capability/service MAY coordinate:

* memory creation,
* retrieval,
* validation,
* retention,
* deletion,
* indexing.

It MUST NOT own the canonical business resources from which Memory may be derived.

---

# Memory Types

Recommended high-level types:

```text
EPHEMERAL_WORKING
SUMMARY
OBSERVATION
USAGE_PATTERN
CORRECTION_PATTERN
RETRIEVAL_NOTE
DERIVED_KNOWLEDGE
EXTERNAL_REFERENCE
CUSTOM
```

These types describe AI-supporting knowledge.

They intentionally exclude canonical Character/Glossary/Profile state.

---

# Ephemeral Working Memory

Ephemeral Working Memory holds short-lived AI context between related operations.

Examples:

* recent dialogue references,
* temporary reasoning summary,
* previous operation result reference,
* current scene summary,
* temporary unresolved ambiguity.

It MAY outlive one provider call.

It SHOULD normally remain bounded to:

* one Session,
* one Chapter,
* one operation chain,
* one explicit context scope.

---

# Session Boundary

Session working state and AI Working Memory are different.

```text
Session
    owns resumable user working state
```

```text
AI Working Memory
    owns temporary AI-supporting context
```

Example:

```text
Session resume position
    !=
AI memory
```

A Session MAY reference AI Memory.

AI Memory MUST NOT become the canonical Session state store.

---

# Session Lifecycle

Memory scoped to a Session MAY expire when the Session ends.

However:

```text
Session-scoped
    !=
always physically deleted immediately
```

Retention follows Memory policy.

Promotion to another durable memory scope MUST be explicit.

---

# Summary Memory

Summary Memory stores compact derived representations of larger source context.

Examples:

* Chapter summary,
* scene summary,
* dialogue summary,
* Character interaction summary.

Summary is derived context.

It MUST NOT replace exact canonical source content.

---

# Summary Provenance

Recommended:

```text
SummaryMemory
├── memoryId
├── scope
├── sourceReferences[]
├── sourceRevisions[]
├── summaryText
├── summaryType
├── summarizerReference?
├── summarizerVersion?
├── createdAt
└── contentHash
```

Where reproducibility matters, source revisions MUST be preserved.

---

# Observation Memory

Observation Memory stores non-authoritative inferred information.

Examples:

* possible recurring motif,
* suspected speaker pattern,
* likely Character association,
* likely terminology relationship,
* story-context observation.

Observation Memory MUST remain distinguishable from confirmed domain truth.

---

# Observation Authority

Typical authority:

```text
INFERRED
DERIVED
EXTERNAL
UNKNOWN
```

Observation Memory MUST NOT silently become:

```text
CONFIRMED
AUTHORITATIVE
```

without an explicit promotion/review workflow.

---

# Usage Pattern Memory

Usage Pattern Memory captures recurring observed behavior.

Examples:

* user repeatedly chooses literal mode,
* user tends to preserve English skill names,
* certain terminology repeatedly corrected.

These are signals.

They are NOT automatically:

* Profile revisions,
* Preferences,
* Glossary Entries.

---

# Correction Pattern Memory

User corrections MAY produce Memory describing a pattern.

Example:

```text
User repeatedly changes:
    "linh khí"
to:
    "linh lực"
```

Memory MAY record this pattern.

It MUST NOT automatically mutate:

```text
Glossary
```

Promotion SHOULD produce:

```text
GlossaryCandidate
```

or another explicit proposal.

---

# Retrieval Note

A Retrieval Note is a lightweight remembered reference.

Example:

```text
Earlier Chapter introduced Character CH-009
```

It SHOULD point back to canonical source references where possible.

---

# Derived Knowledge

Derived Knowledge represents AI-generated supporting knowledge.

Examples:

* inferred scene relationship,
* compact story state summary,
* unresolved ambiguity note.

Derived Knowledge MUST retain:

* provenance,
* confidence where relevant,
* scope,
* expiry/relevance boundaries.

---

# External Reference Memory

Memory MAY point to approved external knowledge.

Example:

```text
external glossary/reference source
publisher note
user-provided reference
```

External references MUST obey:

* Workspace policy,
* authorization,
* privacy,
* provenance requirements.

---

# Memory Record

Recommended generic representation:

```text
AIMemory
├── memoryId
├── memoryType
├── workspaceId
├── projectId?
├── sessionId?
├── scope
├── sourceReferences[]
├── sourceRevisions[]
├── content
├── language?
├── authority
├── confidence?
├── relevanceMetadata?
├── sensitivity?
├── createdAt
├── expiresAt?
├── retentionPolicy?
├── createdBy
└── contentHash
```

Not every implementation needs one physical table for all types.

---

# Memory Scope

Possible scopes:

```text
OPERATION
SESSION
CHAPTER
BOOK
PROJECT
WORKSPACE
USER
CUSTOM
```

Not every Memory Type may use every scope.

Scope MUST be explicit.

---

# Memory Ownership

Memory ownership answers:

```text
Who may access this Memory?
```

It does not answer:

```text
Which business domain owns the underlying truth?
```

Example:

```text
Project-scoped Character Summary Memory
```

does NOT mean Memory owns Character truth.

---

# Memory Sources

Memory MAY be created from:

* canonical domain resources,
* user input,
* user corrections,
* Translation results,
* summaries,
* AI observations,
* imported contextual notes,
* extension-provided data.

Source MUST remain traceable where practical.

---

# Canonical Source Reference

Preferred:

```text
sourceReferences:
    CharacterRevision CH-001/r7
    TextBlock B-009/r3
```

Avoid:

```text
memory says...
```

with no provenance.

---

# Memory Creation

Memory creation SHOULD follow:

```text
Source
   |
   v
Memory Candidate / Derivation
   |
   v
Validation
   |
   v
Memory Record
```

Higher-risk learned Memory SHOULD require stronger validation.

---

# Memory Validation

Validation MAY include:

* schema validity,
* source-reference validity,
* Workspace scope,
* language consistency,
* duplication,
* authority classification,
* sensitivity classification,
* retention policy,
* source-revision compatibility.

Invalid Memory MUST NOT enter normal retrieval.

---

# Memory Authority

Possible authority levels:

```text
AUTHORITATIVE_REFERENCE
CONFIRMED_DERIVED
EXPLICIT_USER_NOTE
DERIVED
INFERRED
EXTERNAL
UNKNOWN
```

`AUTHORITATIVE_REFERENCE` means the Memory points to authoritative canonical data.

It does NOT make the duplicated Memory payload itself canonical truth.

---

# Authority Precedence

When Memory conflicts with canonical domain state:

```text
Canonical Domain Truth
    >
Memory
```

unless the Memory is itself merely a reference to newer authoritative state.

Memory MUST NOT silently override:

* locked Glossary,
* approved Character facts,
* exact Profile Revision,
* Workspace Policy,
* confirmed source Language.

---

# Memory Retrieval

Memory Retrieval selects relevant memory for one explicit request.

Recommended:

```text
MemoryRetrievalRequest
├── workspaceId
├── projectId?
├── sessionId?
├── capabilityType
├── sourceReferences[]
├── languageContext?
├── query?
├── memoryTypes[]
├── scopeConstraints[]
├── maximumItems?
├── budget?
├── spoilerBoundary?
└── policyReference?
```

---

# Retrieval Result

Recommended:

```text
MemoryRetrievalResult
├── retrievalId
├── queryFingerprint?
├── items[]
├── selectedCount
├── omittedCount
├── retrievalPolicyRevision
├── createdAt
└── resultHash?
```

Each result item SHOULD preserve:

* memoryId,
* score,
* authority,
* scope,
* source references.

---

# Retrieval Is Explicit

Critical rule:

```text
AI Pipeline
    MUST NOT
silently retrieve arbitrary Memory
```

Memory participation should be triggered by:

* AI Request,
* Context Profile,
* explicit capability policy,
* operation configuration.

---

# Retrieval Relevance

Selection MAY consider:

* current source references,
* Project,
* Chapter,
* Characters,
* Language,
* terminology,
* semantic similarity,
* content proximity,
* Session,
* time,
* Memory type,
* authority.

There is no universal ranking formula.

---

# Retrieval Score

A conceptual score MAY include:

```text
relevance
+
authority
+
scope proximity
+
content proximity
+
recency
-
privacy risk
-
staleness
```

The exact algorithm is implementation-specific.

---

# Memory Budget

Retrieval SHOULD respect:

* item limit,
* context budget,
* token/character budget,
* privacy budget,
* provider limit.

Memory SHOULD normally be lower priority than required authoritative context.

---

# Memory Selection Boundary

Memory Retrieval returns candidates.

Context Assembly decides which selected Memory items actually enter:

```text
AIContextPackage
```

Therefore:

```text
Memory Retrieval Result
    !=
AI Context Package
```

---

# Memory vs Context

```text
Memory
    persistent/retrievable pool
```

```text
Context
    operation-specific bounded selection
```

Memory may be large.

Context must be intentionally small enough for one operation.

---

# Memory vs Cache

Memory and Cache are different.

```text
Memory
    contextual knowledge
```

```text
Cache
    reusable execution/result optimization
```

Cache answers:

```text
Can we reuse a prior computation?
```

Memory answers:

```text
What prior knowledge may help this computation?
```

---

# Memory vs Translation History

Translation Revisions are canonical durable output.

They MUST NOT be replaced by Memory copies.

Memory MAY reference selected Translation Revisions for context.

---

# Memory vs Glossary

Glossary owns terminology rules.

Memory MAY contain:

* terminology usage observation,
* repeated correction pattern,
* unresolved terminology note.

Memory MUST NOT duplicate approved Glossary truth as its authoritative source.

---

# Memory vs Character

Character owns:

* identity,
* approved Character facts,
* Relationships.

Memory MAY contain:

* derived Character summary,
* interaction summary,
* inference,
* observation.

Memory MUST NOT become the canonical Character registry.

---

# Memory vs Profile

Profile owns reusable processing intent.

Memory MAY observe:

```text
user repeatedly prefers Natural translation
```

but this MUST NOT silently mutate:

```text
TranslationProfile
```

Instead it MAY create:

```text
ProfileCandidate
```

or recommendation.

---

# Memory vs User Preference

User Preference is explicit configuration.

Memory MAY infer a likely preference.

Inference MUST remain a suggestion until explicitly promoted or policy permits auto-adaptation.

---

# Memory vs Project Configuration

Project configuration is canonical Project state.

Memory MAY summarize or reference it.

It MUST NOT become a shadow Project configuration store.

---

# Promotion

Memory promotion converts a learned/derived item into a canonical-domain proposal.

Examples:

```text
Correction Pattern Memory
    ->
GlossaryCandidate
```

```text
Character Observation Memory
    ->
CharacterObservation / CharacterCandidate
```

```text
Preference Pattern Memory
    ->
ProfileCandidate
```

Promotion MUST be explicit.

---

# Promotion Does Not Mean Automatic Approval

Promotion creates a candidate/proposal.

It does NOT automatically create approved canonical truth.

Normal owning-domain workflow still applies.

---

# Memory Update

Memory records SHOULD normally be immutable or append-oriented once materially consumed.

Changes MAY create:

```text
new Memory Revision
```

or a new Memory record linked to the previous one.

Silent in-place semantic rewriting SHOULD be avoided where provenance matters.

---

# Memory Revision

Where needed:

```text
AIMemoryRevision
├── memoryRevisionId
├── memoryId
├── parentRevisionId?
├── content
├── sourceReferences[]
├── authority
├── confidence?
├── createdAt
└── contentHash
```

Not every short-lived memory requires full revision history.

---

# Staleness

Memory MAY become stale when its source changes.

Examples:

```text
Chapter summary based on old TextBlocks
Character observation contradicted by confirmed Character Revision
derived terminology note superseded by approved Glossary Entry
```

Stale Memory MUST NOT automatically be retrieved as current truth.

---

# Staleness Metadata

Possible:

```text
VALID
STALE
SUPERSEDED
INVALIDATED
EXPIRED
```

These describe Memory usability.

They are not the lifecycle of the canonical source domain.

---

# Expiration

Memory MAY expire by:

* Session end,
* Chapter transition,
* age,
* source invalidation,
* explicit TTL,
* manual cleanup,
* policy.

Expiration SHOULD be part of Memory policy.

---

# Retention

Possible retention:

```text
Ephemeral Working Memory
    short

Summary Memory
    medium/long

Correction Pattern Memory
    medium

Observation Memory
    policy-controlled

External Reference Memory
    according to source/policy
```

Retention MUST respect privacy and licensing.

---

# Persistence

Persistent Memory MAY be stored using infrastructure such as:

* relational storage,
* document storage,
* local storage,
* vector indexes,
* search indexes.

The architecture MUST NOT depend on SQLite/PostgreSQL/file implementation choices.

Storage implementation belongs to infrastructure.

---

# Storage Boundary

Preferred architecture:

```text
Memory Capability
        |
        v
Memory Repository Contract
        |
        v
Storage Infrastructure
```

The AI Pipeline MUST NOT access database/files directly.

---

# Indexes

Memory Retrieval MAY use:

* full-text index,
* vector index,
* graph index,
* recency index,
* scope index.

Indexes are derived infrastructure.

They MUST be rebuildable from canonical Memory records where possible.

---

# Vector Embeddings

Embedding vectors are derived retrieval data.

They SHOULD preserve:

```text
memoryId
memoryRevisionId?
embeddingModel
embeddingVersion
contentHash
createdAt
```

Embedding vectors MUST NOT become Memory semantic identity.

---

# Provider Independence

Memory MUST NOT depend on:

* provider conversation IDs,
* provider memory APIs,
* provider assistant threads,
* provider vector-store identity.

Provider facilities MAY implement a temporary adapter.

Canonical Memory identity remains CRAI-owned.

---

# Provider Conversation Memory

Provider-native conversation history MUST NOT automatically become CRAI Memory.

If useful content is extracted, it MUST cross a normalization/provenance boundary first.

---

# Conversation History

CRAI MAY preserve conversation-like content as Memory.

It MUST remain provider-neutral and explicitly scoped.

Example:

```text
DialogueSummaryMemory
```

rather than:

```text
OpenAIThreadMemory
```

---

# Spoiler Safety

Memory retrieval MUST respect reader/story boundaries.

A Project-level memory containing future knowledge MUST NOT automatically enter a Translation request for an earlier Chapter.

Memory SHOULD include sufficient scope metadata to enforce this.

---

# Story Scope

Memory MAY include:

```text
validFromChapter?
validToChapter?
revealBoundary?
readerProgressBoundary?
```

where story progression matters.

---

# Privacy

Memory may contain sensitive user/content information.

Requirements:

* minimize persistence,
* minimize cross-provider exposure,
* prevent cross-Workspace leakage,
* honor local-only policy,
* support deletion,
* respect provider-retention policy,
* avoid logging raw Memory content.

---

# Cross-Workspace Isolation

Private Memory MUST NOT be retrieved across Workspaces without explicit sharing and authorization.

Workspace ID SHOULD participate in storage/index isolation.

---

# Cross-Workspace Learning

Private Memory MUST NOT silently improve another Workspace.

Shared/global learned Memory requires explicit:

* opt-in,
* provenance,
* privacy policy,
* authority model.

---

# Sensitive Memory

Memory MAY be classified:

```text
PUBLIC
INTERNAL
CONFIDENTIAL
RESTRICTED
PERSONAL
LICENSED
```

Retrieval and external-provider use may depend on classification.

---

# Deletion

Deletion MUST account for:

* Memory records,
* indexes,
* embeddings,
* replicated storage,
* derived summaries.

Deleting Memory MUST NOT delete canonical source-domain resources.

---

# Right to Forget

When applicable, CRAI SHOULD support deleting user-derived Memory while preserving canonical resources that have independent legitimate retention.

Exact compliance policy belongs to privacy/governance architecture.

---

# Audit

Material Memory operations MAY be auditable.

Examples:

* creation,
* promotion,
* deletion,
* authority change,
* cross-Workspace share,
* sensitive retrieval.

Audit SHOULD avoid raw Memory content.

---

# Observability

Memory observability MAY include:

* retrieval count,
* hit/miss,
* selected items,
* discarded items,
* retrieval latency,
* index latency,
* Memory size,
* stale-item count,
* promotion count,
* deletion count.

---

# Observability Boundary

Observability MUST NOT log raw Memory content by default.

Prefer:

```text
memoryId
memoryType
scope
contentHash
size
retrievalScore
```

---

# Failure Handling

Possible failures include:

```text
MEMORY_NOT_FOUND
MEMORY_REFERENCE_INVALID
MEMORY_SCHEMA_INVALID
MEMORY_SOURCE_INVALID
MEMORY_SCOPE_VIOLATION
MEMORY_POLICY_DENIED
MEMORY_STALE
MEMORY_EXPIRED
MEMORY_RETRIEVAL_FAILED
MEMORY_INDEX_UNAVAILABLE
MEMORY_PERSISTENCE_FAILED
MEMORY_PROMOTION_CONFLICT
MEMORY_CROSS_WORKSPACE_DENIED
```

---

# Degraded Operation

Memory SHOULD normally be optional unless an operation explicitly requires it.

If optional Memory is unavailable:

```text
continue without Memory
```

may be valid.

If required Memory cannot be retrieved:

```text
fail
```

or request another policy-defined route.

---

# Recovery

Possible strategies:

* continue without optional Memory,
* rebuild index,
* retrieve by deterministic reference,
* restore previous Memory Revision,
* retry persistence,
* mark Memory unavailable,
* rederive Memory from source.

Recovery MUST NOT fabricate canonical truth.

---

# Memory Lifecycle

Recommended conceptual lifecycle:

```text
CREATED
    |
    v
ACTIVE
    |
    +--> STALE
    |
    +--> SUPERSEDED
    |
    +--> EXPIRED
    |
    +--> INVALIDATED
    |
    v
ARCHIVED
```

Temporary Memory MAY skip persistent lifecycle states.

---

# Lifecycle vs Source Domain

Memory lifecycle describes usability of the Memory record.

It MUST NOT mirror canonical source lifecycle.

Example:

```text
Character remains ACTIVE
```

while:

```text
old Character Summary Memory becomes STALE
```

---

# Memory Snapshot

Where operation reproducibility requires exact Memory inputs, retrieval output MAY be frozen into:

```text
MemorySnapshot
```

Recommended:

```text
MemorySnapshot
├── snapshotId
├── memoryReferences[]
├── memoryRevisionReferences[]
├── retrievalPolicyRevision
├── scope
├── createdAt
└── contentHash
```

---

# Memory Snapshot vs Retrieval Result

```text
MemoryRetrievalResult
    = runtime selection output
```

```text
MemorySnapshot
    = immutable retained selection for reproducibility
```

Not every retrieval requires a persisted Snapshot.

---

# Context Integration

Recommended flow:

```text
Memory Store
     |
     v
Memory Retrieval
     |
     v
Memory Retrieval Result
     |
     v
Context Assembly
     |
     v
AI Context Package
```

For durable operations requiring reproducibility:

```text
Memory Retrieval
     |
     v
MemorySnapshot
     |
     v
Context Assembly
```

---

# Prompt Boundary

Only Context Assembly / Prompt Construction should inject Memory content into model input.

Memory subsystem MUST NOT directly build provider prompts.

---

# Update Boundary

The AI Pipeline MAY produce signals that suggest Memory updates.

It MUST NOT silently persist learned Memory without explicit Memory workflow/policy.

Recommended:

```text
AI Result
    |
    v
Memory Candidate
    |
    v
Validation / Policy
    |
    v
Memory Persistence
```

---

# Automatic Learning

Automatic Memory creation SHOULD be conservative.

MVP SHOULD prefer:

* explicit summaries,
* explicit retrieval notes,
* user-approved learning,
* deterministic derived Memory.

Aggressive autonomous learning SHOULD be deferred.

---

# Architecture Invariants

1. AI Memory is provider-independent.

2. AI Memory is not canonical domain truth.

3. Character canonical facts MUST remain in Character domain.

4. Glossary terminology truth MUST remain in Glossary domain.

5. Profile definitions MUST remain in Profile domain.

6. User Preferences MUST remain explicit preference state.

7. Project configuration MUST remain in Project domain.

8. Session resume state MUST remain in Session domain.

9. Translation history MUST remain in Translation domain.

10. Memory MAY reference canonical domain resources.

11. Memory MAY store derived summaries/observations of canonical resources.

12. Derived Memory MUST preserve provenance where practical.

13. Memory authority MUST remain distinguishable from canonical authority.

14. Memory MUST NOT silently override authoritative domain state.

15. Memory participation in AI execution MUST be explicit.

16. AI Pipeline MUST NOT retrieve arbitrary hidden Memory implicitly.

17. Memory Retrieval Result is separate from AI Context Package.

18. Context Assembly decides which retrieved Memory enters the operation Context.

19. Memory is separate from Cache.

20. Memory is separate from provider conversation history.

21. Provider-native memory identity MUST NOT become CRAI Memory identity.

22. Session-scoped Memory is separate from Session state.

23. Memory promotion to canonical domains MUST be explicit.

24. Promotion MUST follow the owning domain's validation/review rules.

25. Correction patterns MUST NOT automatically mutate Glossary.

26. Preference patterns MUST NOT automatically mutate Profile/User Preferences.

27. Character observations MUST NOT automatically mutate Character truth.

28. Memory MAY be immutable or revisioned where provenance matters.

29. Stale Memory MUST NOT masquerade as current authoritative context.

30. Memory lifecycle is independent from canonical source lifecycle.

31. Memory retrieval MUST respect Workspace scope.

32. Memory retrieval MUST respect spoiler/story boundaries.

33. Private Memory MUST NOT leak across Workspaces.

34. Cross-Workspace learning is disabled by default.

35. Sensitive Memory MUST respect Workspace/privacy policy.

36. Raw Memory content SHOULD NOT be logged.

37. Memory indexes are derived and SHOULD be rebuildable.

38. Embeddings are retrieval artifacts, not Memory identity.

39. AI Pipeline MUST NOT directly access Memory storage infrastructure.

40. Memory storage implementation remains infrastructure-specific.

41. Optional Memory failure MAY degrade gracefully.

42. Required Memory failure MUST be explicit.

43. Memory deletion MUST NOT cascade to canonical source data.

44. Memory Snapshot MAY be used when exact retrieval state affects durable output.

45. Memory Snapshot is separate from ordinary retrieval result.

46. Automatic learning SHOULD be conservative.

47. AI Result MUST NOT silently persist learned Memory without policy/workflow.

48. Memory updates SHOULD be traceable and auditable when materially important.

49. Memory context must remain bounded.

50. More Memory does not automatically mean better Context.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* provider-neutral Memory IDs,
* Workspace/Project scope,
* optional Session scope,
* Ephemeral Working Memory,
* Chapter/scene Summary Memory,
* explicit Retrieval Note,
* basic Observation Memory,
* source references,
* source revisions,
* provenance,
* authority,
* optional confidence,
* expiration,
* basic staleness,
* deterministic retrieval by scope/reference,
* bounded recent-history retrieval,
* optional full-text retrieval,
* Memory Retrieval Result,
* optional MemorySnapshot for durable Translation,
* Context integration,
* Workspace isolation,
* spoiler boundaries,
* deletion,
* safe observability,
* explicit promotion into candidates.

MVP MAY defer:

* vector embeddings,
* semantic retrieval,
* AI-driven autonomous Memory,
* long-term User preference learning,
* cross-Project semantic retrieval,
* cross-Workspace Memory,
* graph Memory,
* provider-native Memory integrations,
* advanced Memory consolidation,
* automatic contradiction detection,
* adaptive relevance learning,
* long-term personal AI Memory.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* whether AI Memory is a dedicated module,
* whether Memory records are always persisted,
* exact Memory Type taxonomy,
* exact Memory authority taxonomy,
* Memory revision model,
* whether Session Working Memory duplicates any Session projection,
* Summary Memory generation policy,
* summary regeneration on source revision changes,
* Story Scope representation,
* spoiler-boundary enforcement location,
* whether Translation history retrieval uses Memory or direct Translation queries,
* exact Retrieval Request schema,
* exact Retrieval Result schema,
* whether MemorySnapshot is required for Translation,
* retrieval ranking,
* full-text index implementation,
* vector retrieval timing,
* embedding model policy,
* Memory TTL defaults,
* deletion semantics,
* Memory Candidate workflow,
* automatic correction-pattern learning,
* user opt-in for learning,
* cross-device Memory synchronization,
* local/cloud Memory partition,
* sensitive Memory classification,
* privacy export/delete workflows,
* audit retention,
* context budget allocation to Memory,
* interaction between Memory and future Knowledge Base domain.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `STAGES.md`
* `REQUEST.md`
* `RESPONSE.md`
* `CONTEXT.md`
* `PROMPTS.md`
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

* `../domain/PROJECT.md`
* `../domain/GLOSSARY.md`
* `../domain/CHARACTER.md`
* `../domain/PROFILE.md`
* `../domain/SESSION.md`
* `../domain/TRANSLATION.md`
* `../domain/WORKSPACE.md`

Modules:

* `../../02-modules/preferences/`
* `../../02-modules/translation/`
* `../../02-modules/reading-session/`
* `../../02-modules/provider-management/`

Infrastructure:

* `../../03-infrastructure/storage/`
* `../../03-infrastructure/cache/`
* `../../03-infrastructure/logging/`
* `../../03-infrastructure/telemetry/`
