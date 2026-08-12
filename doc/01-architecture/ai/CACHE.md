# AI Cache

* **Document:** AI Architecture / Cache
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines caching architecture used by CRAI AI execution.

Caching reduces:

* latency,
* repeated provider execution,
* local compute,
* token/resource consumption,
* execution cost,

by safely reusing previously computed or derived artifacts.

Cache is always an optimization layer.

It MUST NOT become canonical business truth.

---

# Core Principle

```text
Semantic Inputs
      |
      v
Cache Identity
      |
      v
Cache Lookup
   /       \
 Hit       Miss
  |          |
  v          v
Validate   Execute
  |          |
  v          v
Return     Finalize
             |
             v
          Cache Write
```

A cache hit is valid only when the cached artifact remains semantically compatible with the current operation.

---

# Scope

AI caching MAY include:

* final AI execution results,
* Prompt/Input artifacts,
* Context materialization,
* token/input-size estimation,
* routing metadata,
* model/catalog metadata,
* derived summaries,
* selected immutable projections.

Different cache classes have different:

* identities,
* retention,
* isolation rules,
* invalidation semantics,
* ownership boundaries.

There is no single universal AI cache.

---

# Non-Goals

Cache does NOT own:

* Translation truth,
* Glossary truth,
* Character truth,
* Profile truth,
* Session state,
* Workspace Policy,
* Provider Configuration,
* AI Request identity,
* AI Response historical semantics,
* persistent domain storage.

---

# Cache Classes

Recommended cache classes:

```text
RESULT_CACHE
PROMPT_CACHE
CONTEXT_CACHE
ESTIMATION_CACHE
ROUTING_CACHE
MODEL_METADATA_CACHE
DERIVED_CONTENT_CACHE
```

Each class SHOULD define its own semantic contract.

---

# Result Cache

`RESULT_CACHE` stores finalized AI execution results that may be reused for another semantically compatible AI Request.

Example:

```text
AI Request semantics
    +
resolved Context
    +
execution semantics
        |
        v
AIResponse
```

Only finalized acceptable responses may enter ordinary Result Cache.

---

# Provisional Output Boundary

The following MUST NOT normally enter final Result Cache:

```text
AIResponseChunk
StreamAssemblyState
partial provider response
invalid candidate response
```

Streaming partial output is provisional.

Only finalized compatible `AIResponse` is eligible.

---

# Business Artifact Boundary

Result Cache MAY store an AIResponse or another execution artifact.

It MUST NOT become the canonical store for:

```text
TranslationRevision
GlossarySnapshot
CharacterRevision
```

Those remain owned by their domains.

A domain artifact MAY itself be retrieved through normal persistence/cache infrastructure, but that is separate from AI Result Cache semantics.

---

# Prompt Cache

Prompt Cache MAY store derived:

```text
Provider-Neutral Model Input
```

or PromptArtifact metadata.

Typical identity includes:

* Prompt Template version,
* Prompt Compiler version,
* instruction-set hash,
* Context hash,
* output-contract reference.

Prompt Cache MUST NOT own Profile or Context truth.

---

# Context Cache

Context Cache MAY store materialized or reduced `AIContextPackage`.

Its identity SHOULD depend on:

* immutable context source references,
* Context Profile/policy,
* reducer/compiler versions,
* context budget semantics.

It MUST NOT silently serve context after authoritative input references change.

---

# Estimation Cache

Estimation Cache MAY store inexpensive derived calculations such as:

* token estimate,
* character count,
* image-unit estimate,
* Prompt size estimate.

These caches normally have low semantic risk and may use aggressive eviction.

---

# Routing Cache

Routing Cache MAY briefly cache a valid RoutePlan or candidate ranking.

Because Routing depends on dynamic inputs, its identity/freshness MUST include or validate:

* health freshness,
* provider availability,
* pricing,
* Policy revision,
* Deployment catalog,
* Entitlement/quota state where relevant.

Routing Cache SHOULD normally have short TTL.

---

# Model Metadata Cache

Model metadata may be cached as a projection of Model Catalog / Provider Management data.

It MUST NOT become authoritative model/provider state.

Stale model metadata MUST be detectable.

---

# Derived Content Cache

Derived non-authoritative artifacts MAY include:

* deterministic summaries,
* normalized intermediate representations,
* context compression outputs.

These require explicit provenance and semantic identity.

---

# Cache Position

There is no one universal cache position in the AI Pipeline.

Possible examples:

```text
Before Routing
    semantic Result Cache
```

when result identity is model-independent under current policy.

```text
After Routing
    model-specific Result Cache
```

when exact model/deployment semantics affect compatibility.

```text
Before Prompt Construction
    Context Cache
```

```text
After Prompt Construction
    Prompt Cache
```

```text
Inside Routing
    short-lived Routing Cache
```

Cache placement depends on cache class.

---

# Cache Before Routing

A Result Cache MAY be queried before Routing only when the lookup does NOT require unresolved route-specific semantics.

Example:

```text
exact source
exact Context snapshots
exact resolved business configuration
output contract
accepted model-independence policy
```

If current cache policy says route/model identity matters, Routing must occur first.

---

# Cache After Routing

Lookup SHOULD occur after Routing when cache compatibility depends on:

* modelId,
* modelVersion,
* deployment class,
* effective model parameters,
* structured-output mode,
* Prompt specialization.

---

# Cache Key vs Cache Identity

Implementation may use a string/hash key.

Architecture defines the semantic:

```text
CacheIdentity
```

The semantic identity SHOULD be explicit before hashing.

---

# Result Cache Identity

Possible semantic inputs:

```text
capability
source content identity/hash
source revision
source Language
target Language
GlossarySnapshot
CharacterContextSnapshot
ResolvedConfigurationSnapshot
Context hash
Prompt/Input semantic hash
output contract
model identity/version when material
effective semantic model parameters
pipeline/compiler versions when material
cache policy revision
```

Not every field applies to every capability.

---

# Request ID Boundary

`requestId` SHOULD normally NOT be part of semantic Result Cache identity.

Two distinct Requests may be semantically equivalent.

Therefore:

```text
Request ID
    !=
Result Cache Key
```

---

# Session ID Boundary

`sessionId` SHOULD NOT automatically participate in reusable Result Cache identity.

If all Session-derived semantic inputs have already been frozen into immutable snapshots:

```text
Session ID
```

may be irrelevant to result semantics.

It MAY remain relevant for:

* authorization,
* temporary local caches,
* diagnostics.

---

# Project ID Boundary

`projectId` MUST NOT be included merely because the operation belongs to a Project.

Project identity belongs in CacheIdentity only when:

* Project-specific semantic state is not otherwise captured,
* privacy/authorization requires Project partitioning,
* cache policy deliberately disables cross-Project reuse.

Prefer exact semantic references over broad Project identity.

---

# Workspace Isolation

Workspace isolation is mandatory even when `workspaceId` is not part of the semantic hash.

Logical semantic identity and access/isolation identity are separate concerns.

A cache entry MAY have:

```text
semanticKey
+
isolationScope
```

---

# Isolation Scope

Recommended:

```text
CacheIsolationScope
├── workspaceId?
├── projectId?
├── principalId?
├── localDeviceId?
├── sharingClass
└── classification?
```

The exact scope depends on cache class.

---

# Cross-Workspace Reuse

Private cached content MUST NOT be reused across Workspaces by default.

Cross-Workspace reuse requires proof that:

* semantic inputs are content-addressed,
* result contains no private tenant-specific context,
* licensing/privacy permits reuse,
* authorization is independently rechecked,
* cache policy explicitly allows it.

MVP SHOULD disable this.

---

# Determinism

Caching does NOT require the underlying model to be mathematically deterministic.

A non-deterministic AI result MAY still be reusable if cache policy defines:

```text
reuse previously accepted result
for semantically equivalent execution identity
```

The important property is:

```text
semantic cache compatibility
```

not:

```text
model always produces the same bytes
```

---

# Deterministic Derived Cache

Some cache classes SHOULD still require deterministic computation.

Examples:

* token estimate,
* normalized Context serialization,
* exact Prompt compilation.

Their cache guarantees may be stronger than Result Cache.

---

# Semantic Compatibility

Before returning a cached artifact, CRAI SHOULD verify:

* compatible artifact type,
* compatible schema version,
* matching semantic key,
* valid isolation scope,
* required authorization,
* applicable freshness,
* no invalidation/tombstone,
* Policy compatibility,
* required model compatibility where applicable.

---

# Cache Entry

Recommended generic representation:

```text
CacheEntry
├── cacheEntryId
├── cacheClass
├── semanticKey
├── isolationScope
├── artifactType
├── artifactReferenceOrPayload
├── schemaVersion
├── compatibilityMetadata
├── createdAt
├── expiresAt?
├── lastAccessedAt?
├── sourceReferences[]
├── provenance
├── contentHash
└── status
```

Not every backend needs to persist this exact structure.

---

# Cache Entry Status

Possible states:

```text
VALID
STALE
INVALID
CORRUPTED
EXPIRED
```

Status is cache metadata.

It is not domain lifecycle.

---

# Cache Write Eligibility

Before writing a Result Cache entry, verify:

```text
AIResponse finalized
AND
response acceptable
AND
cache policy permits write
AND
sensitivity permits retention
AND
semantic identity complete
```

---

# Validation Before Write

Invalid or provisional AI responses MUST NOT enter ordinary Result Cache.

Degraded responses MAY be cached only if:

* degraded state is explicit,
* cache identity includes relevant degradation semantics,
* reuse policy allows it.

---

# Cached Result Validation

A cache hit MAY still undergo lightweight validation.

Examples:

* schema version,
* output type,
* mapping completeness,
* content hash,
* compatibility version.

A cached artifact MUST NOT bypass required authorization or Policy checks.

---

# Cache and AIResponse

A Result Cache hit SHOULD reconstruct or return a provider-neutral response contract compatible with normal live execution.

Example provenance:

```text
executionMode: CACHE
cacheEntryReference: ...
```

Semantic result shape remains unchanged.

---

# Cache and Execution Provenance

A cached AIResponse MAY retain provenance of the original generation.

It SHOULD also expose that current delivery came from cache.

These are different facts:

```text
original execution provenance
```

vs:

```text
current retrieval provenance
```

---

# Cache and Model Provenance

If model identity is semantically material, cache entry MUST preserve exact relevant model provenance.

If cache policy explicitly treats models as interchangeable for a result class, that policy MUST be versioned and explicit.

---

# Cache and RoutePlan

A Result Cache hit before Routing may mean no RoutePlan is needed for the current execution.

A Result Cache hit after Routing may reference the route-compatible identity used for lookup.

Cache architecture MUST NOT fabricate a provider execution attempt for a pure cache hit.

---

# Cache and Usage

Cache hits may reduce provider usage.

Usage/Cost accounting MAY record:

```text
cache retrieval
avoided provider execution estimate
```

But cache metrics are not authoritative billing state.

---

# Cache and Retry

During recovery, orchestration MAY recheck cache if semantically valid.

Retry itself does not own cache lookup.

A newly available cache hit MAY terminate recovery successfully.

---

# Cache and Fallback

Fallback MAY also cause a different cache identity to become eligible if route/model semantics change.

Cache MUST NOT hide material degradation differences.

---

# Cache and Streaming

Partial provider streams MUST NOT populate final Result Cache.

After stream finalization:

```text
AIResponse
```

may become cacheable.

A cached finalized response MAY later be delivered incrementally for UX, but this is delivery behavior, not provider streaming.

---

# Cache and Memory

Cache and Memory are different.

```text
Cache
    = reuse computation/artifact
```

```text
Memory
    = retrieve contextual knowledge
```

Memory MAY itself use infrastructure caches internally.

That does not merge the two semantic concepts.

---

# Cache and Persistent Storage

Cache MUST remain discardable.

Loss of Cache MUST NOT destroy canonical domain history.

If an artifact is required for business history:

```text
persist it in its owning storage
```

rather than relying solely on Cache.

---

# Cache Levels

Possible physical levels:

```text
PROCESS_MEMORY
LOCAL_DEVICE
LOCAL_PERSISTENT
WORKSPACE_SHARED
DISTRIBUTED
PROVIDER_SIDE
```

These are infrastructure placement choices.

They do not define semantic cache classes.

---

# Process Memory

Useful for:

* hot metadata,
* Prompt compilation,
* token estimates,
* short-lived routing data.

Process loss is expected.

---

# Local Device Cache

Useful for:

* offline reading,
* repeated local Translation,
* local model metadata,
* local Context artifacts.

It MUST preserve user/Workspace isolation.

---

# Distributed Cache

Useful for shared server execution.

Distributed cache MUST preserve:

* tenant isolation,
* version compatibility,
* concurrency safety,
* bounded retention.

---

# Provider-Side Cache

Some providers may expose:

* prompt caching,
* context caching,
* prefix caching.

These are provider optimizations.

They MUST remain behind Provider Adapter.

Canonical CRAI Cache identity MUST NOT depend solely on provider cache IDs.

---

# Provider Cache Boundary

Provider-side cache MAY reduce cost/latency without producing a CRAI Result Cache hit.

Therefore:

```text
Provider Prompt Cache
    !=
CRAI Result Cache
```

---

# TTL

TTL MAY be used for caches whose compatibility can expire over time.

Examples:

* health/routing metadata,
* pricing metadata,
* provider availability,
* temporary estimates.

Immutable semantic-result caches MAY rely more on exact versioned identity than TTL.

---

# Invalidation Philosophy

Prefer:

```text
new semantic inputs
    -> new CacheIdentity
```

over:

```text
global mutable invalidation
```

where possible.

---

# Snapshot-Based Invalidation

Example:

```text
GlossarySnapshot A
    -> Cache Key A

Glossary changes

GlossarySnapshot B
    -> Cache Key B
```

Cache Key A does not become semantically corrupted.

It may simply age out when unused.

---

# Profile Revision Change

Likewise:

```text
ResolvedConfigurationSnapshot X
```

and:

```text
ResolvedConfigurationSnapshot Y
```

produce different cache identities.

No broad “Profile changed, delete everything” invalidation is required.

---

# User Correction

A User correction SHOULD normally create new canonical source/Translation revision or configuration state.

New immutable identity prevents stale result reuse.

Explicit invalidation may still be useful for caches tied to mutable convenience projections.

---

# Explicit Invalidation

Explicit invalidation is still appropriate when:

* cached data is known corrupt,
* security/privacy requires removal,
* a provider alias was misclassified,
* a compatibility bug is discovered,
* policy retroactively forbids retained cache,
* mutable external metadata becomes unsafe.

---

# Cache Tombstone

High-risk invalidation MAY create a short-lived tombstone:

```text
CacheTombstone
├── semanticKey
├── reason
├── createdAt
└── expiresAt?
```

to prevent immediate repopulation from stale replicas.

MVP may defer this.

---

# Cache Versioning

Cache compatibility MAY depend on:

* AI Request schema,
* AI Response schema,
* Context Package version,
* Prompt Compiler version,
* model parameter mapper version,
* pipeline semantic version,
* cache policy revision.

Only versions that materially affect semantics SHOULD enter identity.

---

# Version Explosion

Avoid including every implementation version blindly.

Bad:

```text
all component versions
```

because this destroys reuse.

Prefer only versions that may change artifact semantics.

---

# Content Hashing

Cache identities SHOULD use normalized semantic hashes where practical.

Hashes MUST NOT be treated as authorization tokens.

---

# Hash Confidentiality

A hash of private content may still leak correlation information.

Sensitive hashes SHOULD be scoped/protected where necessary.

---

# Concurrency

Cache implementations SHOULD support safe concurrent:

* reads,
* writes,
* replacement,
* eviction.

Duplicate computation MAY be acceptable unless cost warrants request coalescing.

---

# Request Coalescing

For identical expensive semantic requests, CRAI MAY support:

```text
single-flight
```

behavior.

Conceptually:

```text
Request A ─┐
Request B ─┼--> one execution --> shared compatible result
Request C ─┘
```

This requires careful:

* isolation,
* cancellation semantics,
* authorization,
* result compatibility.

MVP MAY defer cross-request coalescing.

---

# Cache Stampede

Strategies MAY include:

* single-flight,
* jittered TTL,
* stale-while-revalidate for metadata caches,
* lock/lease,
* early refresh.

These are infrastructure concerns.

---

# Stale-While-Revalidate

Useful for metadata such as:

* model catalog,
* pricing,
* health projection.

It SHOULD NOT automatically be used for semantic AI Results where freshness is part of correctness.

---

# Cache Failure

Cache failure SHOULD normally degrade to live execution.

Possible failures:

```text
CACHE_UNAVAILABLE
CACHE_LOOKUP_FAILED
CACHE_ENTRY_CORRUPTED
CACHE_SERIALIZATION_FAILED
CACHE_VERSION_INCOMPATIBLE
CACHE_ISOLATION_VIOLATION
CACHE_POLICY_DENIED
CACHE_WRITE_FAILED
```

---

# Cache Failure Boundary

Cache failure MUST NOT normally become business-operation failure.

Exception:

If an operation explicitly requires:

```text
CACHE_ONLY
OFFLINE_CACHED_ONLY
```

then cache miss/failure may legitimately fail that operation mode.

---

# Cache-Only Mode

Future/offline workflows MAY request:

```text
cacheOnly = true
```

This MUST be explicit.

Ordinary AI execution SHOULD fall back to live execution on cache miss.

---

# Corruption

Corrupted cache entries MUST be:

* rejected,
* removed/quarantined,
* observed,
* regenerated where appropriate.

They MUST NOT be returned merely because a key matches.

---

# Security

Cache MUST NOT store:

* raw credentials,
* provider secrets,
* authentication tokens,

unless a dedicated secure credential cache explicitly owns those semantics.

Ordinary AI Cache must never become secret storage.

---

# Sensitive Result Storage

AI Result Cache may contain private or copyrighted content.

Policies SHOULD govern:

* encryption,
* local/cloud storage,
* TTL,
* Workspace isolation,
* deletion,
* external provider-side caching.

---

# Policy Changes

Policy MAY affect whether an existing cached artifact may still be served.

A cache hit MUST NOT bypass current authorization/Policy where those are evaluated at retrieval time.

Historical creation under old Policy is not automatically sufficient for current access.

---

# Data Residency

Distributed caches MUST comply with applicable Workspace data-residency constraints.

---

# Deletion

Workspace/Project/user deletion workflows SHOULD invalidate/remove applicable private caches.

Cache deletion MUST NOT be considered equivalent to deleting canonical domain resources.

---

# Observability

Recommended metrics:

* lookup count,
* hit count,
* miss count,
* hit rate,
* lookup latency,
* write latency,
* write failure count,
* corruption count,
* eviction count,
* invalidation count,
* cache size,
* avoided execution estimate,
* estimated cost savings,
* hit rate by cache class,
* hit rate by capability.

---

# Semantic Hit vs Physical Hit

Metrics SHOULD distinguish:

```text
physical key hit
```

from:

```text
semantic accepted hit
```

An entry may exist but fail compatibility validation.

---

# Cache Rejection Reasons

Useful reasons:

```text
VERSION_MISMATCH
POLICY_MISMATCH
ISOLATION_MISMATCH
MODEL_MISMATCH
SCHEMA_MISMATCH
STALE
CORRUPTED
DEGRADATION_MISMATCH
```

---

# Sensitive Observability

Cache logs SHOULD avoid:

* raw source,
* raw Prompt,
* raw AI Result,
* Glossary content,
* Character content.

Prefer:

```text
cacheEntryId
semanticKeyHash
cacheClass
artifactType
size
scope
```

---

# Architecture Invariants

1. Cache is an optimization layer.

2. Cache MUST NOT become canonical business truth.

3. There is no single universal AI Cache class.

4. Different cache classes have different identities and lifecycles.

5. Result Cache stores only finalized acceptable AI results.

6. Provisional streaming output MUST NOT enter ordinary Result Cache.

7. Invalid candidates MUST NOT enter ordinary Result Cache.

8. Cache lookup placement is cache-class-specific.

9. Cache lookup does NOT universally occur before Routing.

10. Cache lookup MAY occur before Routing when route identity is semantically irrelevant under policy.

11. Cache lookup SHOULD occur after Routing when model/route semantics affect compatibility.

12. Cache identity is semantic, not merely Request identity.

13. `requestId` SHOULD NOT normally be part of semantic Result Cache identity.

14. `sessionId` SHOULD NOT automatically prevent reusable cache hits.

15. `projectId` SHOULD NOT automatically prevent reusable cache hits.

16. Workspace isolation remains mandatory regardless of semantic cache-key composition.

17. Semantic identity and authorization/isolation identity are separate.

18. Cross-Workspace reuse is forbidden by default.

19. Cached results need semantic compatibility, not mathematical model determinism.

20. Deterministic derived caches MAY require stronger deterministic guarantees.

21. Exact immutable snapshots/revisions SHOULD participate in semantic cache identity where they affect output.

22. Mutable “current Glossary” MUST NOT be a cache-key dependency for durable execution.

23. Mutable “current Profile” MUST NOT be a cache-key dependency for durable execution.

24. New immutable semantic state SHOULD naturally produce new cache identities.

25. Broad invalidation SHOULD be avoided when immutable versioned identity is sufficient.

26. Explicit invalidation remains necessary for corruption/security/policy bugs.

27. Cache schema/version compatibility MUST be checked.

28. Only semantically material component versions SHOULD affect cache identity.

29. Cache keys/hashes MUST NOT grant authorization.

30. Cache hits MUST revalidate required access/isolation constraints.

31. Cache hits MUST NOT bypass applicable current Policy.

32. Cached result shape SHOULD remain compatible with normal AIResponse contracts.

33. Cache delivery provenance and original execution provenance remain distinct.

34. Provider-side prompt/context cache is separate from CRAI Result Cache.

35. Provider-specific cache identifiers MUST NOT become canonical CRAI cache identity.

36. Cache may be physically in-memory, local or distributed without changing semantic class.

37. Physical cache level is separate from semantic cache class.

38. Loss of Cache MUST NOT destroy required historical business data.

39. Memory and Cache remain separate concepts.

40. Routing Cache and Result Cache remain separate concepts.

41. Cache failure SHOULD normally degrade to live execution.

42. Explicit cache-only modes are exceptional.

43. Corrupted entries MUST NOT be served.

44. Cache write MUST respect sensitivity/retention Policy.

45. Private cache content MUST remain Workspace-isolated.

46. Cache storage MUST respect data residency.

47. Cache deletion does not equal domain-resource deletion.

48. Cache metrics SHOULD distinguish physical hits from semantically accepted hits.

49. Cache telemetry SHOULD avoid raw sensitive content.

50. Result Cache MAY reuse non-deterministic historical AI output only under explicit semantic reuse policy.

51. Retry does not own Cache.

52. Fallback does not own Cache.

53. Streaming does not own final Result Cache writes before finalization.

54. Cache may reduce provider execution but Usage/Cost infrastructure remains authoritative.

55. New cache implementations SHOULD be replaceable without changing domain/business contracts.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* Result Cache,
* Prompt/Input Cache,
* Context Cache,
* Estimation Cache,
* Model Metadata Cache,
* provider-neutral CacheIdentity,
* Workspace isolation,
* local persistent cache,
* in-memory cache,
* optional shared server cache,
* semantic hashes,
* exact GlossarySnapshot identity,
* exact CharacterContextSnapshot identity,
* ResolvedConfigurationSnapshot identity,
* Prompt/Input semantic hash,
* model/version identity when required,
* output-contract identity,
* TTL,
* size limits,
* explicit bypass,
* write eligibility,
* compatibility validation,
* corruption detection,
* cache hit/miss observability,
* avoided execution/cost estimate,
* partial-stream exclusion,
* live-execution fallback on ordinary cache failure.

MVP SHOULD disable:

* cross-Workspace semantic Result reuse,
* partial-result caching,
* provider-native cache identity as canonical identity,
* cache-only Translation by default.

MVP MAY defer:

* single-flight request coalescing,
* stale-while-revalidate,
* distributed cache leases,
* Cache Tombstones,
* cross-Project reuse optimization,
* cross-Workspace safe deduplication,
* advanced cache warming,
* semantic similarity cache,
* fuzzy cache matching,
* cache admission learning,
* adaptive TTL.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact cache-class taxonomy,
* exact `CacheIdentity` schema,
* whether Result Cache stores AIResponse or semantic Result separately,
* exact cache-before-routing conditions,
* whether model identity is mandatory for Translation Result Cache,
* exact Prompt Cache boundary,
* exact Context Cache persistence model,
* semantic hash normalization,
* Workspace isolation key strategy,
* whether Project ID participates in default isolation,
* local anonymous cache identity,
* cache TTL defaults,
* maximum cache sizes,
* result-cache retention,
* encrypted cache requirements,
* cache write policy for degraded responses,
* cache policy revisioning,
* current-Policy revalidation behavior,
* provider-side cache integration,
* token-estimation cache,
* Routing Cache ownership,
* stale-while-revalidate classes,
* cache coalescing/single-flight,
* offline cache-only mode,
* deletion propagation,
* cache metrics retention,
* whether cache entries retain original model provenance,
* exact cache compatibility after mutable provider aliases change.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `STAGES.md`
* `REQUEST.md`
* `RESPONSE.md`
* `CONTEXT.md`
* `MEMORY.md`
* `PROMPTS.md`
* `MODELS.md`
* `ROUTING.md`
* `STREAMING.md`
* `RETRY.md`
* `FALLBACK.md`
* `COST_CONTROL.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/GLOSSARY.md`
* `../domain/CHARACTER.md`
* `../domain/PROFILE.md`
* `../domain/SESSION.md`
* `../domain/TRANSLATION.md`
* `../domain/WORKSPACE.md`

Modules:

* `../../02-modules/translation/`
* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`

Infrastructure:

* `../../03-infrastructure/cache/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/logging/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
