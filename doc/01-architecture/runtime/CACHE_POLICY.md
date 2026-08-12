# Runtime Cache Policy

* **Document:** Runtime Architecture / Cache Policy
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines how CRAI Runtime may reuse and retain previously accepted execution results to reduce repeated computation, latency and provider/resource cost without changing business semantics or execution authority.

Cache is an optional optimization.

It MAY provide:

* execution-result reuse;
* Runtime Artifact retention;
* provider-cost reduction;
* latency reduction;
* duplicate-computation reduction.

Cache is NOT:

* a source of truth;
* a Business Module;
* a Business validity owner;
* a Scheduler decision;
* a WorkItem terminal outcome;
* Runtime execution authority;
* durable Storage by default;
* a Policy/Governance owner.

---

# 2. Core Principle

```text
Business Module
    defines semantic compatibility
        |
        v
Policy / Privacy Owner
    defines allowed reuse scope
        |
        v
Cache Policy
    applies reuse / retention mechanics
        |
        v
Runtime Artifact Store / Durable Cache
        |
        v
Runtime Control
    validates current execution relevance
```

The system MUST remain correct when:

```text
Cache = Empty
```

---

# 3. Architectural Position

Recommended reuse flow:

```text
Logical Work Identified
        |
        v
Owner-Defined Reuse Requirements
        |
        v
ReuseQuery
        |
        v
Runtime / Durable Candidate Lookup
        |
        v
Semantic Compatibility Validation
        |
        v
Policy / Partition Validation
        |
        v
Integrity / Availability Validation
        |
        v
Runtime Authority Validation
        |
        v
Reusable Result Accepted
        |
        +--> satisfy WorkItem without new Attempt
        |
        +--> miss -> Scheduler path
```

---

# 4. Ownership

| Concern                          | Owner                     |
| -------------------------------- | ------------------------- |
| Business result meaning          | Business Module           |
| Semantic compatibility           | Business Module           |
| Semantic dependency declaration  | Business Module           |
| Privacy/policy reuse constraints | Policy / Governance owner |
| Cache reuse mechanics            | Cache Policy              |
| Cache retention policy           | Cache Policy              |
| Runtime Artifact lifecycle       | Runtime Artifact Store    |
| Physical resource lifecycle      | Resource Manager          |
| Durable persistence mechanics    | Storage                   |
| Current execution relevance      | Runtime Control           |
| Scheduler admission after miss   | Scheduler                 |

---

# 5. Cache Policy Boundary

Cache Policy MAY decide:

* whether cache lookup is enabled;
* which retention class is allowed;
* lookup order;
* bounded retention;
* eviction;
* expiration;
* coalescing mechanics;
* durable-cache eligibility mechanics;
* promotion mechanics.

Cache Policy MUST NOT independently define:

* Translation semantic compatibility;
* Recognition semantic compatibility;
* Presentation semantic compatibility;
* Provider equivalence;
* Language equivalence;
* Workspace Privacy meaning;
* Domain correctness.

---

# 6. Cache Principles

1. Cache is optional.

2. Runtime correctness MUST NOT depend on cache presence.

3. Cache is not source of truth.

4. Cache key is deterministic.

5. Reuse requires explicit semantic compatibility.

6. ExecutionRevisionId is not a semantic reuse identity.

7. Cache hit is not Scheduler admission.

8. Cache hit is not WorkItem terminal outcome.

9. Worker does not decide cache reuse.

10. Technical execution success does not imply cache eligibility.

11. Business acceptance is normally required before promotion.

12. Failed/Canceled/Stale/Abandoned output is not promoted by default.

13. Cache retention is bounded.

14. Eviction does not invalidate active leases.

15. Durable cache uses Storage boundary.

16. Cache contains no raw secret.

17. Policy partitions are respected.

18. Cache operation failure normally degrades to miss.

19. Cache does not alter Business semantics.

20. Cache coalescing does not merge WorkItem authority.

---

# 7. Runtime Vocabulary

Canonical Runtime identities:

```text
ApplicationInstanceId
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
```

Cache reuse identities are separate.

---

# 8. Execution Identity vs Reuse Identity

```text
ExecutionRevisionId
    = runtime freshness / authority identity
```

```text
ContentIdentity
    = semantic input identity
```

```text
ReuseIdentity
    = semantic reuse identity
```

They MUST NOT be conflated.

---

# 9. Content Identity

`ContentIdentity` represents the semantic identity of relevant business input.

It SHOULD be:

* deterministic;
* privacy-safe;
* stable enough for reuse;
* independent from one Attempt;
* independent from one ExecutionRevision where semantics permit.

---

# 10. Content Identity Examples

Possible implementations MAY derive identity from:

* normalized source structure;
* source-document identity;
* immutable Domain resource revision;
* content hash;
* owner-defined semantic identity.

Exact construction belongs to the owning Business Module contract.

---

# 11. Reuse Across Execution

An accepted result MAY potentially be reused across:

```text
multiple Attempts
multiple WorkItems
multiple ExecutionRevisions
multiple ExecutionScopes
multiple application runs
```

only when semantic compatibility and policy permit.

---

# 12. Reuse Query

Recommended:

```text
ReuseQuery
├── ownerModule
├── resultType
├── contentIdentity
├── outputContractVersion
├── semanticDependencyFingerprint
├── reusePartitionReference
├── requestedReuseScope
├── requestedRetentionClass?
└── correlationMetadata?
```

---

# 13. Semantic Dependency Fingerprint

Instead of hard-coding:

```text
ProviderProfileVersion
LanguageProfile
GlossaryVersion
PromptVersion
ModelVersion
```

into generic Cache architecture, the owning module SHOULD produce:

```text
SemanticDependencyFingerprint
```

representing all dependencies that materially affect result compatibility.

---

# 14. Why Fingerprint Is Owner-Defined

Different results have different semantic dependencies.

Example:

```text
Translation Result
    may depend on:
        source identity
        target language
        translation profile
        glossary
        model/provider semantics
```

while:

```text
Recognition Result
    may depend on:
        image identity
        recognition configuration
        model/version
```

Cache Policy MUST NOT encode these rules globally.

---

# 15. Semantic Dependency Descriptor

Where diagnostics require explainability, owner MAY expose:

```text
SemanticDependencyDescriptor
├── fingerprint
├── dependencyKinds[]
├── contractVersion
└── compatibilityVersion
```

Values SHOULD avoid exposing sensitive content.

---

# 16. Cache Key

Recommended conceptual composition:

```text
CacheKey = Hash(
    OwnerModule
    + ResultType
    + ContentIdentity
    + OutputContractVersion
    + SemanticDependencyFingerprint
    + ReusePartitionId
)
```

---

# 17. Cache Key Boundary

Cache Key MUST NOT rely on:

```text
ExecutionRevisionId
AttemptId
WorkerId
QueueId
```

unless a deliberately local reuse scope requires execution identity.

---

# 18. Cache Key Privacy

Cache keys MUST NOT expose raw:

* source content;
* translated content;
* Prompt;
* URL;
* credential;
* private path.

Hash preimages SHOULD remain inaccessible through ordinary diagnostics.

---

# 19. Reuse Candidate

A `ReuseCandidate` is a previously retained result that MAY satisfy a current logical work request.

Recommended:

```text
ReuseCandidate
├── cacheEntryId
├── resultReference
├── resultType
├── ownerModule
├── outputContractVersion
├── semanticDependencyFingerprint
├── reusePartitionReference
├── producerMetadata
├── integrityMetadata
├── retentionMetadata
└── provenanceReference?
```

---

# 20. Cache Entry

Recommended:

```text
CacheEntry
├── cacheEntryId
├── cacheKey
├── resultReference
├── resultType
├── ownerModule
├── outputContractVersion
├── semanticDependencyFingerprint
├── reusePartitionReference
├── producerVersion?
├── createdAt
├── lastAccessedAt?
├── expiresAt?
├── sizeEstimate?
├── retentionClass
├── validationState
└── integrityMetadata
```

Cache Entry SHOULD NOT duplicate payload.

---

# 21. Result Reference

A Cache Entry MAY reference:

```text
RuntimeArtifactRef
DurableCacheRecordRef
BusinessResultRef
```

depending on architecture and retention class.

The reference type MUST remain explicit.

---

# 22. Runtime Artifact vs Business Result

Critical distinction:

```text
RuntimeArtifact
    = execution payload
```

```text
Business Result
    = owner-accepted semantic result
```

A Runtime Artifact is not automatically eligible for semantic reuse.

---

# 23. Promotion Preconditions

By default, promotion SHOULD require:

```text
Runtime execution result accepted
        |
        v
Owning Business Module accepts result
        |
        v
Owner declares result cache-eligible
        |
        v
Policy permits retention/reuse
        |
        v
Cache Policy promotes
```

---

# 24. Runtime Acceptance Is Not Enough

Critical rule:

```text
Runtime authority accepted
    !=
Business result cache eligible
```

Runtime Control proves that the result was current.

Business Module proves that the result is semantically valid.

---

# 25. Promotion

Promotion means:

```text
add cache retention/reuse ownership
```

to an already accepted result.

Promotion SHOULD NOT copy large payload by default.

---

# 26. Promotion Result

Recommended:

```text
CachePromotionResult
├── cacheEntryId
├── resultReference
├── retentionClass
├── reuseScope
├── expiresAt?
└── policyReference?
```

---

# 27. Technical Success vs Promotion

```text
Attempt SUCCEEDED
    !=
cache promotion
```

Possible non-promotable successful result:

* insufficient compatibility metadata;
* policy forbids retention;
* result nondeterministic and owner disallows reuse;
* result only valid for current operation;
* retention budget exhausted;
* business owner marks result non-cacheable.

---

# 28. Reuse Validation

Recommended dimensions:

```text
Identity
Semantic Compatibility
Contract Compatibility
Policy / Partition
Integrity
Availability
Runtime Relevance
```

Each dimension has a distinct owner.

---

# 29. Identity Validation

Checks that candidate input identity matches current logical work according to owner-defined rules.

---

# 30. Semantic Compatibility Validation

Business Module or an owner-provided compatibility contract decides:

```text
Does this result remain semantically valid?
```

Cache Policy MUST NOT recreate those rules.

---

# 31. Contract Compatibility

Output contract/version must be consumable by the current Business Module contract.

---

# 32. Policy / Partition Validation

Authoritative Policy/Governance rules determine whether candidate may cross:

* Workspace;
* Project;
* principal/user;
* privacy class;
* local/remote provenance;
* security boundary.

Cache Policy applies the resolved result.

---

# 33. Integrity Validation

Checks:

* referenced payload exists;
* metadata matches;
* checksum/hash valid where available;
* durable record not corrupt;
* Artifact reference structurally valid.

---

# 34. Runtime Relevance

Runtime Control confirms:

```text
Does the current WorkItem
still need this result?
```

This is execution relevance, not semantic compatibility.

---

# 35. Reuse Acceptance

A cache candidate becomes reusable only when all required validations pass.

Recommended:

```text
REUSE_ACCEPTED
REUSE_MISS
REUSE_REJECTED_COMPATIBILITY
REUSE_REJECTED_POLICY
REUSE_REJECTED_INTEGRITY
REUSE_REJECTED_AUTHORITY
REUSE_UNAVAILABLE
```

---

# 36. Cache Hit Semantics

A cache hit means:

```text
compatible reusable result found
```

It does NOT mean:

```text
WorkItem terminal state changed
Scheduler admitted/rejected work
Business result committed
Presentation committed
```

---

# 37. Satisfying Work from Reuse

Recommended:

```text
Reusable Result
        |
        v
Runtime Control validates WorkItem relevance
        |
        v
Owning Business Contract accepts reuse
        |
        v
WorkItem logical result may be satisfied
```

Exact WorkItem completion mechanism belongs to `PIPELINE_RUNTIME.md`.

---

# 38. Reuse Scope

Recommended generic scopes:

```text
EXECUTION_REVISION
EXECUTION_SCOPE
APPLICATION_RUNTIME
DURABLE
```

---

# 39. EXECUTION_REVISION

Reuse limited to one ExecutionRevision.

Useful for highly execution-local result classes.

---

# 40. EXECUTION_SCOPE

Reuse allowed across ExecutionRevisions within one ExecutionScope.

---

# 41. APPLICATION_RUNTIME

Reuse allowed across ExecutionScopes in the same application process/runtime when policy permits.

---

# 42. DURABLE

Reuse may survive application restart through Storage.

Durable reuse requires stronger provenance/compatibility validation.

---

# 43. Reuse Scope Is Not Permission

A result marked:

```text
APPLICATION_RUNTIME
```

does not automatically mean every Workspace/principal may access it.

Policy partition remains authoritative.

---

# 44. Reuse Partition

Generic Cache architecture SHOULD use:

```text
ReusePartitionReference
```

rather than hard-coding privacy dimensions.

---

# 45. Reuse Partition Ownership

Policy/Governance may derive partition from:

* Workspace;
* principal;
* privacy mode;
* sensitivity;
* local/remote policy;
* provenance constraints.

Cache receives a resolved partition/reference.

---

# 46. Cross-Partition Reuse

Default:

```text
different partition
    -> no reuse
```

unless authoritative policy explicitly permits compatibility.

---

# 47. EPHEMERAL-Like Policy

When resolved policy prohibits durable retention:

* no durable promotion;
* runtime retention remains minimal;
* cleanup receives elevated priority;
* reuse scope may be constrained.

Cache Policy applies this resolved restriction.

It does not own the meaning of EPHEMERAL policy itself.

---

# 48. Runtime Memory Cache

Runtime memory cache provides volatile process-local retention.

Characteristics:

* bounded;
* fast lookup;
* RuntimeArtifactRef/result-reference based;
* lease-aware;
* pressure-aware;
* lost on process restart.

---

# 49. Artifact Store Boundary

Runtime Artifact Store owns:

* Artifact registry;
* backing-resource references;
* leases;
* Runtime retention;
* physical lifecycle coordination.

Cache Policy owns:

* whether extra reuse retention should exist;
* when that retention should be removed.

---

# 50. Cache Retention vs Artifact Lifetime

Critical distinction:

```text
Cache retention removed
    !=
Artifact physically destroyed
```

An Artifact may remain alive because of:

* active owner;
* active lease;
* other retention;
* Business reference.

---

# 51. Durable Cache

Durable cache is a Storage-backed optimization.

Recommended:

```text
Cache Policy
    approves durable retention
        |
        v
Storage
    persists durable cache representation
```

Storage does not define semantic compatibility.

---

# 52. Durable Cache Record

Recommended:

```text
DurableCacheRecord
├── cacheKey
├── resultDescriptor
├── outputContractVersion
├── semanticDependencyFingerprint
├── reusePartitionReference
├── persistenceVersion
├── retentionMetadata
├── provenanceMetadata
├── createdAt
├── expiresAt?
└── integrityMetadata
```

---

# 53. Durable Materialization

A durable record loaded after restart is only:

```text
ReuseCandidate
```

It does NOT automatically gain Runtime authority.

---

# 54. Durable Reuse Validation

After materialization, validate again:

* contract compatibility;
* semantic dependencies;
* partition/policy;
* integrity;
* availability;
* current Runtime relevance.

---

# 55. Eviction

Eviction removes cache retention because of:

* pressure;
* budget;
* low usefulness;
* LRU/LFU/weighted policy;
* scope end;
* runtime shutdown;
* retention expiry.

Eviction does NOT mean semantic invalidity.

---

# 56. Invalidation

Invalidation means candidate is no longer semantically/structurally eligible.

Possible causes:

* integrity failure;
* unsupported output contract;
* owner compatibility rule changed;
* policy changed;
* explicit Business correction invalidates result;
* security removal requirement.

---

# 57. Expiration

Expiration is time-based.

Possible:

```text
TTL
Idle TTL
ExecutionScope lifetime
Application lifetime
Durable retention window
```

Expiration prevents reuse.

Physical disposal still follows ownership/lease rules.

---

# 58. Removal

Removal may be requested for:

* user clear;
* privacy action;
* account/workspace removal;
* administrative cleanup;
* corruption remediation;
* security incident.

Durable removal follows Storage deletion policy.

---

# 59. Eviction vs Invalidation vs Expiration vs Removal

```text
Eviction
    = retention/value decision

Invalidation
    = compatibility/integrity decision

Expiration
    = temporal decision

Removal
    = explicit deletion action
```

These MUST remain distinct.

---

# 60. Partial Results

Unvalidated partial output MUST NOT be cached.

Validated partial output MAY be cacheable only if:

* owner defines stable partial-result contract;
* identity is stable;
* ordering metadata exists;
* semantic compatibility exists;
* policy permits;
* cache promotion is explicit.

MVP SHOULD default to no partial promotion.

---

# 61. Warning-Bearing Results

A business-accepted result with warnings MAY be reusable when:

* warnings are preserved;
* warnings do not invalidate semantics;
* owner allows reuse;
* policy allows retention.

Warning alone does not automatically prohibit reuse.

---

# 62. Canceled Results

Canceled execution output SHOULD NOT be promoted by default.

MVP:

```text
Cancellation
    -> no promotion
```

---

# 63. Stale Results

Stale execution output SHOULD NOT be promoted by default.

MVP:

```text
REJECT_STALE
    -> no promotion
```

Future optimization may consider stale-but-semantically-valid results only through an explicit separate policy.

---

# 64. Failed / Abandoned Results

Failed or abandoned execution MUST NOT be stored as successful reusable results.

Negative-result caching is a separate future architecture.

---

# 65. Retry Interaction

Before creating another Retry Attempt, Runtime MAY request reuse evaluation.

```text
Retry Candidate
        |
        v
Runtime Control
        |
        v
Reuse Evaluation
        |
        +--> reusable result
        |       -> no new Attempt needed
        |
        +--> miss
                -> Retry continues
```

Retry Policy does not perform cache lookup.

---

# 66. Scheduler Interaction

Cache lookup SHOULD normally occur before Scheduler admission when practical.

Cache hit is not a Scheduler decision.

If reuse fails:

```text
Runtime Control
    -> Scheduler
```

continues normal execution admission.

---

# 67. Worker Boundary

Worker MUST NOT independently:

* lookup semantic cache;
* select reusable result;
* promote result;
* invalidate business cache entries.

Worker may interact with low-level implementation-local caches only when those are hidden behind the owning capability contract and do not violate Runtime semantics.

---

# 68. In-Flight Reuse

Cache architecture MAY support coalescing duplicate expensive execution.

This is distinct from ordinary cache hit.

---

# 69. In-Flight Reuse Model

```text
WorkItem A
    produces ReuseKey K

WorkItem B
    also needs K
```

Possible behavior:

```text
A executes

B waits for accepted result from A
```

provided B keeps its own:

* WorkItem identity;
* cancellation scope;
* authority;
* deadline;
* business context.

---

# 70. Producer Is Not Shared WorkItem

Critical rule:

```text
In-flight coalescing
    !=
merge WorkItems
```

Producer remains one WorkItem.

Observers remain independent WorkItems.

---

# 71. Coalescing Ownership

Recommended:

```text
InFlightReuseCoordinator
```

MAY coordinate:

* producer registration;
* observer registration;
* bounded waiting;
* accepted-result publication;
* observer detachment.

This MAY remain internal to Cache Runtime implementation.

---

# 72. In-Flight Cancellation

Canceling observer B MUST NOT automatically cancel producer A if:

* A is still independently required;
* another observer still depends on A.

---

# 73. Producer Failure

Producer failure MUST NOT automatically mark observers failed.

Observers may:

* continue normal execution;
* retry lookup;
* create their own Attempt;
* receive recovery decision.

---

# 74. Accepted Result Requirement

Only a result accepted through required Runtime and Business boundaries becomes eligible to satisfy observers.

Physical producer Completion alone is insufficient.

---

# 75. Coalescing Wait

Observer wait MUST be:

* bounded;
* cancelable;
* deadline-aware;
* authority-aware.

---

# 76. Stampede Prevention

Possible mechanisms:

* in-flight coalescing;
* bounded lookup;
* per-key coordination;
* Scheduler admission;
* bounded provider/execution concurrency.

Do NOT use one global cache lock.

---

# 77. Negative Cache

MVP SHOULD NOT support negative-result caching.

Future negative cache MUST use a distinct type/contract.

It MUST NOT masquerade as successful reusable Artifact/result.

---

# 78. Cache Failure

Normal cache failure degrades to miss.

```text
Cache Lookup Failure
        |
        v
Diagnostics
        |
        v
Treat As Miss
        |
        v
Continue Normal Execution
```

---

# 79. Cache Integrity Failure

Integrity corruption MAY require:

* candidate invalidation;
* Artifact quarantine;
* Storage diagnostics;
* security diagnostics where appropriate.

It still MUST NOT create false success.

---

# 80. Durable Cache Failure

If durable cache is unavailable:

* runtime memory reuse MAY remain available;
* normal execution continues;
* WorkItem does not fail merely because durable cache failed;
* durable promotion pauses.

---

# 81. Cache Configuration

Runtime Configuration MAY control operational limits such as:

```text
memory capacity
maximum entries
retention TTL
lookup timeout
eviction thresholds
```

It MUST NOT define Business semantic compatibility.

---

# 82. Semantic Configuration Identity

Where Business configuration affects result semantics, the owning module includes that dependency in:

```text
SemanticDependencyFingerprint
```

Cache does not independently inspect full configuration objects.

---

# 83. Provider Boundary

Whether Provider identity affects semantic reuse is an owning-module/routing contract decision.

Cache architecture MUST NOT globally assume:

```text
different Provider
    -> incompatible
```

or:

```text
different Provider
    -> compatible
```

---

# 84. Model Boundary

Likewise, whether AI/OCR model identity affects result compatibility is owner-defined.

Cache only receives the resulting semantic fingerprint.

---

# 85. Language Boundary

Language pair/profile affects cache identity only when the owner declares it semantic.

Generic Cache Policy does not understand language semantics.

---

# 86. Presentation Cache Boundary

Presentation results MAY have reuse semantics different from Translation/Recognition.

Presentation owner defines their dependencies.

Runtime Cache must not hard-code:

```text
font
layout
theme
```

globally.

---

# 87. Source Document Boundary

Source Document reuse follows Text/Domain ownership rules.

Cache Policy does not decide whether two source representations are semantically identical.

---

# 88. Observability Events

Recommended:

```text
CacheLookupStarted
CacheHit
CacheMiss
CacheCandidateRejected
CacheEntryPromoted
CacheEntryEvicted
CacheEntryInvalidated
CacheEntryExpired
CacheEntryRemoved
CacheLookupFailed
DurableCacheDegraded
InFlightReuseJoined
InFlightReuseDetached
```

---

# 89. Event Payload

Recommended metadata:

```text
cacheEntryId?
resultType
ownerModule
reuseScope
reusePartitionReference
decision
reasonCode
sizeEstimate?
timing
contractVersion
semanticFingerprintReference?
```

Do not emit raw cache-key preimage.

---

# 90. Metrics

Recommended:

```text
hit count
miss count
useful hit ratio
validation rejection count
semantic incompatibility count
policy partition miss count
integrity failure count
promotion count
promotion skipped count
eviction count
invalidation count
expiration count
retained bytes
lookup latency
durable lookup latency
saved useful latency
saved provider/resource cost
in-flight coalescing count
cache operation failure count
```

---

# 91. Useful Metrics

Raw hit rate is not sufficient.

Prefer:

```text
Useful Hit Ratio
Saved Useful Latency
Saved Execution Cost
Current Execution Reuse Ratio
Invalid Reuse Prevention Count
```

---

# 92. Privacy

Cache telemetry MUST NOT contain:

* source text;
* translated text;
* screenshot;
* Prompt;
* AI Context;
* raw URL;
* credentials;
* key preimage.

---

# 93. Cache Security

Cache MUST NOT contain:

* API keys;
* OAuth tokens;
* access tokens;
* private keys;
* resolved credential values;
* unrestricted private diagnostics.

---

# 94. Process Restart

Runtime-memory cache disappears at process restart.

Durable cache records must be revalidated.

No durable record automatically restores:

```text
ExecutionRevision authority
WorkItem outcome
Runtime Artifact ownership
```

---

# 95. Cache and ExecutionRevision

An Artifact/result produced under an old ExecutionRevision MAY still be reusable later.

Therefore:

```text
ExecutionRevision authority
    !=
semantic reuse compatibility
```

---

# 96. Cache and ExecutionScope

Cross-ExecutionScope reuse is allowed only when:

* semantic compatibility allows;
* reuse scope allows;
* policy partition permits;
* security/tenant isolation permits.

---

# 97. Cache and Domain History

Cache MUST NOT replace Domain history.

Example:

```text
TranslationRevision history
```

remains Domain-owned even if equivalent Translation output is cached.

---

# 98. Cache and Durable Storage

Durable cache is disposable optimization data.

Canonical persisted business truth remains separate.

Deleting durable cache MUST NOT delete canonical Domain resources.

---

# 99. Cache and Resource Lifecycle

Eviction removes retention.

Resource Manager determines physical disposal eligibility.

Recommended:

```text
Eviction
    |
    v
Cache Retention Released
    |
    v
Artifact still leased?
    |
    +--> yes -> remain physically alive
    |
    +--> no  -> disposal eligible
```

---

# 100. Architecture Invariants

1. Cache is not source of truth.

2. Runtime remains correct when cache is empty.

3. Cache key is deterministic.

4. ExecutionRevisionId is not semantic reuse identity.

5. ExecutionScopeId is not semantic reuse identity.

6. Business Module owns semantic compatibility.

7. Business Module owns semantic dependency declaration.

8. Policy/Governance owns cross-scope privacy/security reuse constraints.

9. Cache Policy owns reuse/retention mechanics.

10. Runtime Artifact Store owns runtime payload lifecycle.

11. Storage owns durable persistence mechanics.

12. Runtime Control owns current execution relevance.

13. Scheduler does not decide cache hit.

14. Worker does not perform semantic cache reuse.

15. Cache hit is not terminal WorkItem outcome.

16. Technical success does not imply cache eligibility.

17. Runtime authority acceptance alone does not imply cache eligibility.

18. Business-accepted result is normally required for promotion.

19. Failed/Canceled/Stale/Abandoned outputs do not promote by default.

20. Unvalidated partial output does not promote.

21. Cache promotion does not copy payload by default.

22. Eviction does not invalidate active lease.

23. Eviction and invalidation are distinct.

24. Expiration and invalidation are distinct.

25. Explicit removal is distinct from eviction.

26. Durable cache always uses Storage boundary.

27. Reuse partition is policy-derived.

28. Cross-partition reuse is denied by default.

29. Cache contains no secrets.

30. Cache-key diagnostics do not expose preimages.

31. Lookup failure normally degrades to miss.

32. In-flight reuse does not merge WorkItem identity.

33. Observer cancellation does not automatically cancel producer.

34. Producer physical Completion alone does not satisfy observers.

35. Only accepted compatible result satisfies reuse.

36. SemanticDependencyFingerprint is owner-defined.

37. Cache Policy does not hard-code Provider/Language/Model semantics.

38. Version changes do not require immediate deletion of older entries.

39. Cache retention is always bounded.

40. Runtime cache and canonical Domain persistence remain separate.

---

# 101. Recommended MVP

CRAI MVP SHOULD support:

* process-local memory cache;
* bounded Runtime Artifact retention;
* deterministic CacheKey;
* ContentIdentity;
* SemanticDependencyFingerprint;
* ReusePartitionReference;
* EXECUTION_REVISION scope;
* EXECUTION_SCOPE scope;
* APPLICATION_RUNTIME scope;
* no durable cache by default;
* LRU/weighted LRU;
* no negative cache;
* no partial promotion by default;
* no Failed/Canceled/Stale/Abandoned promotion;
* business-accepted-result promotion;
* policy-partition validation;
* cache failure -> miss;
* Retry pre-check integration;
* basic content-free cache telemetry.

MVP MAY defer:

* in-flight coalescing;
* durable cache;
* cross-application reuse;
* cost-aware eviction;
* adaptive retention;
* negative-result cache;
* stale-but-semantic-result salvage;
* cross-device cache.

---

# 102. MVP Result Types

MVP MAY support reuse for owner-defined result contracts such as:

```text
Captured/Source Result
Recognition Result
Source Document
Translation Result
Presentation Result
```

Exact types belong to module contracts.

Do NOT create architecture layers named:

```text
OCR Cache
Layout Cache
Translation Cache
```

unless those owners explicitly expose such reusable result contracts.

---

# 103. MVP Retention

Conceptual defaults:

| Scope               | Retention                                   |
| ------------------- | ------------------------------------------- |
| ExecutionRevision   | Until revision cleanup or pressure eviction |
| ExecutionScope      | Until scope cleanup or pressure eviction    |
| Application Runtime | Bounded LRU                                 |
| Durable             | Disabled by default                         |
| Debug/private       | Disabled by default                         |

Exact values belong to `RUNTIME_CONFIG.md`.

---

# 104. Example — Runtime Hit

```text
WorkItem Needs Recognition Result
        |
        v
ReuseQuery
        |
        v
Candidate Found
        |
        v
Semantic Compatibility Valid
        |
        v
Policy Partition Valid
        |
        v
Integrity Valid
        |
        v
Runtime Relevance Valid
        |
        v
Reuse Accepted
        |
        v
No New Attempt Needed
```

---

# 105. Example — Semantic Fingerprint Mismatch

```text
Translation Result Found
        |
        v
SemanticDependencyFingerprint differs
        |
        v
Reuse Rejected
        |
        v
Cache Miss
        |
        v
Normal Execution
```

Cache does not need to know whether the difference was caused by Glossary, model, Profile or another semantic dependency.

---

# 106. Example — Eviction with Active Lease

```text
Eviction selects Cache Entry
        |
        v
Cache Retention Removed
        |
        v
Artifact Lease Still Active
        |
        v
Payload Remains
        |
        v
Lease Released
        |
        v
Physical Disposal Eligible
```

---

# 107. Example — Durable Reuse

```text
Memory Miss
        |
        v
Storage-backed Cache Lookup
        |
        v
Durable Record Found
        |
        v
Materialize ReuseCandidate
        |
        v
Revalidate Contract / Semantic Fingerprint
        |
        v
Validate Partition / Integrity
        |
        v
Runtime Control checks relevance
        |
        v
Reuse Accepted
```

Durable presence alone never grants runtime authority.

---

# 108. Example — Partition Miss

```text
Candidate Found
        |
        v
Candidate ReusePartition = P1

Current request ReusePartition = P2
        |
        v
No explicit cross-partition authorization
        |
        v
Reuse Rejected
```

---

# 109. Example — Retry Avoided by Reuse

```text
Attempt A1 fails
        |
        v
Runtime Control considers another Attempt
        |
        v
Reuse evaluation
        |
        v
Compatible accepted result exists
        |
        v
No new Attempt created
```

Retry Policy itself performs no lookup.

---

# 110. Example — In-Flight Coalescing

```text
WorkItem A needs key K
WorkItem B needs key K

A becomes producer
B becomes observer

A completes
        |
        v
Runtime + Business acceptance
        |
        v
Reusable result published
        |
        v
B independently validates authority
        |
        v
B may use result
```

A and B retain independent WorkItem identity.

---

# 111. Testing Requirements

Tests SHOULD include:

* memory hit;
* memory miss;
* deterministic key;
* semantic fingerprint mismatch;
* output contract mismatch;
* partition mismatch;
* integrity failure;
* Runtime relevance rejection;
* cache disabled;
* all entries evicted;
* promotion after Business acceptance;
* rejection before Business acceptance;
* Failed/Canceled/Stale/Abandoned promotion rejection;
* partial-result rejection;
* warning-bearing accepted result;
* eviction with active lease;
* durable cache unavailable;
* durable candidate incompatible;
* Retry avoided by reuse;
* ExecutionRevision cross-reuse;
* ExecutionScope cross-reuse;
* cross-partition denial;
* process restart;
* cache lookup failure degradation;
* telemetry privacy;
* optional in-flight coalescing;
* observer cancellation;
* producer failure.

---

# 112. Open Decisions

The following remain open:

* CacheKey hash algorithm;
* ContentIdentity representation;
* SemanticDependencyFingerprint format;
* ReusePartition representation;
* compatibility-query contract;
* owner-module cache-eligibility API;
* RuntimeArtifactRef vs BusinessResultRef representation;
* LRU vs weighted LRU;
* reuse-scope defaults;
* durable-cache support;
* durable-cache encryption;
* durable retention window;
* cross-ExecutionScope reuse defaults;
* in-flight coalescing MVP inclusion;
* coalescing coordinator implementation;
* useful-value scoring;
* stale-but-semantic-result future policy;
* negative-result caching.

---

# 113. Related Documents

Runtime:

* `PIPELINE_RUNTIME.md`
* `RUNTIME_COMPONENTS.md`
* `SCHEDULER.md`
* `RETRY_POLICY.md`
* `CANCELLATION.md`
* `MEMORY_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `PERFORMANCE_MODEL.md`
* `RUNTIME_CONFIG.md`
* `RUNTIME_OBSERVABILITY.md`

External:

* `../domain/`
* `../ai/CACHE.md`
* `../../02-modules/recognition/`
* `../../02-modules/translation/`
* `../../02-modules/presentation/`
* `../../02-modules/storage/`

---

# 114. Completion Criteria

`CACHE_POLICY.md` is synchronized when:

* Cache remains an optional optimization;
* Cache is never source of truth;
* ExecutionScope/ExecutionRevision terminology is canonical;
* execution identity and reuse identity are separate;
* Business Module owns semantic compatibility;
* owner-defined SemanticDependencyFingerprint replaces hard-coded generic dependencies;
* Policy/Governance owns reuse partition semantics;
* Cache Policy owns mechanics rather than Business meaning;
* Runtime authority remains Runtime Control-owned;
* promotion normally requires Business-accepted result;
* Cache hit remains distinct from terminal WorkItem outcome;
* Runtime Artifact Store remains distinct from Cache Policy;
* durable cache remains Storage-backed;
* eviction/invalidation/expiration/removal remain distinct;
* Retry can re-evaluate reuse without Retry Policy owning cache lookup;
* in-flight coalescing does not merge WorkItems;
* no raw secrets/content appear in cache metadata/telemetry.

---

# 115. Summary

CRAI Cache Policy follows:

```text
Business Result Semantics
        |
        v
Owner-Defined Compatibility
        |
        v
Policy-Derived Reuse Scope
        |
        v
Cache Reuse Evaluation
        |
        v
Candidate Lookup
        |
        v
Validation
        |
        v
Runtime Relevance
        |
        v
Reusable Result
```

The central rule is:

```text
Cache reuses accepted meaning.

Cache does not define meaning.
```
