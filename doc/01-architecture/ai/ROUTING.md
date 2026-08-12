# AI Routing

* **Document:** AI Architecture / Routing
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI selects a compatible and appropriate AI execution route for one AI operation.

Routing evaluates:

* AI capability requirements,
* Model requirements,
* Model and Deployment capabilities,
* applicable Policy decisions,
* Cost constraints,
* runtime availability,
* health,
* locality/privacy requirements,
* quality preferences,
* latency preferences,
* user/administrative preferences,

and produces a provider-neutral:

```text
RoutePlan
```

Routing decides:

```text
Which currently allowed execution path
should CRAI attempt?
```

It does NOT execute the model.

---

# Core Principle

```text
AI Request
    +
Resolved Execution Requirements
    +
Model Catalog
    +
Deployment Catalog
    +
Provider Availability
    +
Policy Decision
    +
Cost Constraints
    +
Runtime Health
        |
        v
Routing
        |
        v
Route Plan
        |
        v
Provider Request Adaptation
        |
        v
Model Execution
```

Routing combines inputs owned by other architectural concerns.

It MUST NOT absorb their ownership.

---

# Scope

Routing is responsible for:

* candidate discovery,
* compatibility filtering,
* hard-constraint evaluation,
* preference scoring,
* deterministic ranking,
* route selection,
* route-plan construction,
* alternate candidate planning,
* decision explainability.

Routing is NOT responsible for:

* provider API execution,
* provider credentials,
* Policy rule ownership,
* Cost ledger ownership,
* health-check execution,
* retry execution,
* fallback execution,
* queue scheduling,
* model lifecycle,
* AI business-domain commit.

---

# Non-Goals

Routing does NOT own:

* AI Request semantics,
* Prompt construction,
* Model Catalog truth,
* Provider Configuration,
* Workspace Policy,
* Usage Ledger,
* Budget balances,
* runtime attempt history,
* Translation truth,
* provider SDK logic.

---

# Design Principles

Routing SHOULD be:

* capability-first,
* provider-neutral,
* constraint-driven,
* policy-aware,
* cost-aware,
* health-aware,
* locality-aware,
* explainable,
* deterministic for identical routing inputs,
* extensible,
* fail-closed for hard constraints.

---

# Routing Architecture

Recommended:

```text
AI Request / Model Requirements
        |
        v
Candidate Discovery
        |
        v
Hard Constraint Filtering
        |
        v
Candidate Enrichment
        |
        v
Preference Scoring
        |
        v
Deterministic Ranking
        |
        v
Route Plan Construction
```

External inputs MAY come from:

```text
Model Catalog
Deployment Catalog
Provider Management
Policy Evaluation
Cost Control
Runtime Health
Evaluation/Quality Projections
```

---

# Routing Inputs

Recommended conceptual input:

```text
RoutingInput
├── requestReference
├── capabilityRequirements
├── modelRequirements
├── contextRequirements
├── outputRequirements
├── executionConstraints
├── policyDecision
├── costConstraints
├── localityConstraints
├── privacyConstraints
├── userPreferences?
├── administrativePreferences?
├── modelCatalogSnapshot
├── deploymentSnapshot
├── providerAvailabilitySnapshot
├── healthSnapshot
├── qualityProjectionSnapshot?
├── pricingSnapshot?
└── routingPolicyRevision
```

Not every implementation must persist all snapshots.

But routing decisions SHOULD be explainable from explicit inputs.

---

# Routing Input vs AI Request

AI Request contains semantic operation intent.

Routing additionally needs dynamic execution information.

Therefore:

```text
AIRequest
    !=
RoutingInput
```

The same Request MAY produce different Route Plans at different times because runtime conditions changed.

---

# Candidate Discovery

Candidate Discovery finds routable Model Deployments potentially capable of satisfying the Request.

Conceptually:

```text
Model Catalog
    +
Deployment Catalog
        |
        v
Candidate Deployments
```

Discovery SHOULD NOT yet apply preference scoring.

---

# Candidate Identity

Routing SHOULD operate on a concrete routable execution identity such as:

```text
RouteCandidate
├── modelId
├── deploymentId
├── providerId
├── providerConfigurationId
├── executionMode
├── region?
└── capabilitySnapshot
```

Selecting only a provider is insufficient.

---

# Model vs Deployment Selection

A Model describes capability.

A Deployment describes an executable path.

Routing normally needs:

```text
Model
+
Deployment
```

Example:

```text
Model X
    Deployment SG
    Deployment US
    Local Deployment
```

These may differ in:

* availability,
* policy compatibility,
* latency,
* cost,
* region,
* health.

---

# Hard Constraints

Hard constraints MUST be evaluated before preference scoring.

Examples:

* required capability,
* input modality,
* output modality,
* minimum Context size,
* maximum output requirement,
* structured-output level,
* streaming requirement,
* Language compatibility,
* local-only requirement,
* provider prohibition,
* data residency,
* privacy restriction,
* entitlement,
* deployment availability.

A candidate violating a hard constraint MUST NOT proceed.

---

# Capability Matching

Capability matching compares:

```text
AIModelRequirements
        |
        v
ModelDescriptor
+
ModelDeployment
+
Runtime Capability State
```

Examples include:

* TRANSLATION,
* VISION_ANALYSIS,
* STRUCTURED_EXTRACTION,
* LANGUAGE_DETECTION,
* STRICT_SCHEMA,
* STREAMING,
* IMAGE_INPUT,
* instruction-hierarchy support.

---

# Capability vs Provider

Capability matching MUST NOT be expressed as:

```text
if provider == X
```

Instead:

```text
candidate.capabilities satisfies requirements
```

Provider identity may still matter later for:

* Policy,
* pricing,
* health,
* preference,

but not as a substitute for capability semantics.

---

# Language Matching

When capability is Language-sensitive, routing MAY evaluate:

* source Language,
* target Language,
* LanguageRange,
* LanguagePair,
* Script,
* provider/model support evidence.

Canonical Language values MUST come from `LANGUAGE.md`.

Provider-specific language codes MUST remain inside adapters.

---

# Language Pair Routing

For Translation, directional metadata MAY be used:

```text
zh-Hans -> vi
```

where available.

Fallback from exact LanguagePair to broader Language capability MUST follow an explicit routing policy.

---

# Context Compatibility

Candidate filtering MUST evaluate whether required context fits.

Relevant constraints MAY include:

* input Context size,
* Prompt overhead,
* reserved output capacity,
* image count,
* modality-specific limits.

Routing SHOULD consider:

```text
required semantic context
```

rather than blindly selecting a small-context model and forcing Prompt Builder to drop required data.

---

# Prompt / Instruction Compatibility

Routing MUST consider Prompt Architecture requirements.

Examples:

```text
requires:
    STRICT_SCHEMA
    instruction separation
    multimodal input
```

If a model/deployment cannot safely represent required semantics, it is incompatible.

Routing MUST NOT rely on Prompt Builder to silently weaken requirements.

---

# Policy Decision

Routing consumes an already-evaluated or explicitly evaluable Policy Decision.

Recommended:

```text
PolicyDecision
├── decision
├── allowedProviderIds?
├── deniedProviderIds?
├── allowedRegions?
├── requiredExecutionMode?
├── requiredDataResidency?
├── prohibitedCapabilities?
└── policyRevisionReference
```

---

# Policy Ownership

Routing MUST NOT become the canonical Workspace Policy engine.

Policy architecture/domain owns:

* rules,
* constraints,
* revisions,
* approval semantics.

Routing applies the resulting constraints to candidates.

---

# Policy Denial

If Policy denies the operation entirely:

```text
DENY
```

Routing MUST NOT attempt to find a workaround.

Fallback MUST NOT bypass Policy.

---

# Safety Boundary

Routing MAY consume Safety constraints where Safety affects model/provider eligibility.

Examples:

* provider unsuitable for required safety controls,
* local-only processing required,
* model lacks required safety capability.

Routing does NOT own Safety Policy.

---

# Cost Constraints

Routing consumes operation-level Cost Constraints.

Examples:

```text
maximumEstimatedCost
costTier
budgetClass
retryBudget
fallbackBudget
```

Routing SHOULD NOT read or mutate the authoritative Usage Ledger directly.

---

# Budget Boundary

Workspace daily/monthly budget state belongs to Cost Control.

Recommended:

```text
Usage / Budget State
        |
        v
Cost Control
        |
        v
Cost Constraints / Decision
        |
        v
Routing
```

Routing may estimate candidate cost against those constraints.

---

# Cost Estimation

For each candidate:

```text
EstimatedRouteCost
```

MAY be derived from:

* estimated input units,
* estimated output units,
* model/deployment pricing,
* request fees,
* modality fees.

Cost estimation is approximate until execution completes.

---

# Pricing Freshness

Pricing may change.

Routing SHOULD know which pricing version/snapshot was used when cost materially affects selection.

---

# Health Input

Routing consumes runtime health information.

It SHOULD NOT perform provider health ownership itself.

Recommended:

```text
DeploymentHealth
├── deploymentId
├── state
├── observedAt
├── expiresAt?
├── latencyProjection?
├── errorRateProjection?
├── rateLimitPressure?
└── capacitySignal?
```

---

# Health States

Possible high-level states:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
MAINTENANCE
UNKNOWN
```

These align with `MODELS.md`.

---

# Health Freshness

A health observation is valid only within its freshness window.

Routing MUST NOT treat stale:

```text
AVAILABLE
```

as permanently healthy.

Expired observations SHOULD become:

```text
UNKNOWN
```

or trigger an explicit refresh policy.

---

# Unavailable Candidate

A Deployment explicitly known as:

```text
UNAVAILABLE
```

MUST NOT be selected for a normal primary route.

Whether `UNKNOWN` is usable depends on Routing Policy.

---

# Degraded Candidate

A `DEGRADED` route MAY remain eligible when:

* no healthier candidate exists,
* Policy allows it,
* latency/error thresholds remain acceptable,
* business requirements can still be met.

The decision SHOULD be visible in Route Plan warnings.

---

# Availability vs Health

These concepts may differ.

```text
Availability
    = can this route currently accept execution?
```

```text
Health
    = how well is it behaving?
```

A route may be available but degraded.

---

# Entitlement

Routing MUST respect entitlement.

A technically compatible Model may still be unavailable to the Workspace due to plan/deployment restrictions.

Entitlement failure is a hard constraint unless an explicit upgrade workflow exists.

---

# Quota

Quota MAY constrain candidate selection.

Example:

```text
cloud premium quota exhausted
```

Routing may choose another allowed candidate.

Quota is owned by Usage/Entitlement infrastructure.

---

# Privacy and Locality

Routing MAY evaluate:

* local-only requirement,
* cloud allowed,
* provider region,
* Workspace data residency,
* content classification,
* external-processing restrictions.

Privacy constraints are hard constraints when mandatory.

---

# User Preferences

User/Profile preferences MAY include:

* prefer local,
* prefer low cost,
* prefer low latency,
* prefer high quality,
* preferred provider,
* preferred model,
* preferred execution mode.

Preferences influence ranking.

They MUST NOT bypass hard constraints.

---

# Preferred Provider

A preferred provider is a soft preference unless configuration explicitly pins it.

Example:

```text
preferredProvider = X
```

means:

```text
rank X higher when compatible
```

not:

```text
use X even if Policy or capability forbids it
```

---

# Preferred Model

Likewise, preferred Model is normally a soft preference.

Exact Model pinning MUST be explicit.

---

# Exact Route Pinning

For diagnostics, benchmarks or reproducibility, Routing MAY accept:

```text
PinnedRoute
├── modelId
├── deploymentId?
└── providerConfigurationId?
```

Even pinned routes MUST satisfy mandatory:

* Policy,
* safety,
* authorization,
* capability constraints,

unless a controlled administrative diagnostic policy explicitly states otherwise.

---

# Routing Policy

A Routing Policy defines how compatible candidates are ranked.

Possible policy intents:

```text
BALANCED
LOWEST_COST
LOWEST_LATENCY
HIGHEST_QUALITY
OFFLINE_FIRST
PRIVACY_FIRST
LOCAL_FIRST
QUALITY_WITHIN_BUDGET
CUSTOM
```

These are ranking strategies.

They MUST NOT redefine hard constraints.

---

# Routing Policy vs Profile

Routing Profile MAY express user/business routing preference.

Routing Policy represents the algorithm/rules used to evaluate it.

Conceptually:

```text
RoutingProfile
    = desired intent

RoutingPolicy
    = how candidates are scored/selected
```

They SHOULD remain separate.

---

# Candidate Enrichment

After hard filtering, candidates MAY be enriched with:

* estimated cost,
* latency projection,
* quality projection,
* health,
* capacity,
* locality,
* preference match.

These values feed scoring.

---

# Quality Evaluation

Quality SHOULD be capability-specific.

Example:

```text
translation quality for zh-Hans -> vi
```

is more relevant than a global:

```text
quality = 9
```

Routing MAY consume benchmark/evaluation projections.

It SHOULD NOT own evaluation history.

---

# Latency Evaluation

Latency scoring MAY use:

* recent deployment telemetry,
* coarse catalog expectations,
* region proximity,
* queue pressure,
* local hardware state.

Latency estimates are dynamic projections.

---

# Preference Scoring

After hard constraints pass, Routing MAY compute:

```text
CandidateScore
```

using weighted factors.

Conceptually:

```text
Score
    =
qualityWeight * quality
+
latencyWeight * latency
+
costWeight * cost
+
privacyWeight * locality
+
preferenceWeight * userPreference
+
healthWeight * health
```

The actual scoring model remains configurable.

---

# Hard Constraints vs Score

A candidate MUST NOT receive a high score to compensate for violating a hard constraint.

Correct:

```text
Filter
    first

Score
    second
```

---

# Deterministic Ranking

For identical:

* candidate set,
* capability metadata,
* Policy Decision,
* Cost Constraints,
* health snapshot,
* pricing snapshot,
* preference input,
* Routing Policy revision,

candidate ordering SHOULD be deterministic.

---

# Dynamic Routing

The same AI Request MAY legitimately produce another Route Plan later because:

* health changed,
* provider availability changed,
* pricing changed,
* quota changed,
* Policy changed,
* deployment changed.

Therefore:

```text
Same Request
    !=
always same Route Plan
```

---

# Stable Tie-Breaker

Candidate ranking SHOULD have a deterministic final tie-breaker.

Possible:

```text
deploymentId
```

or another stable canonical key.

Avoid random selection unless explicitly required.

---

# Load Balancing Boundary

Runtime/provider infrastructure MAY apply load balancing among semantically equivalent Deployment instances.

If load balancing can materially change provider/model semantics, it MUST remain visible to Routing/provenance.

Pure infrastructure instance balancing need not become Route Plan semantics.

---

# Route Candidate

Recommended:

```text
RouteCandidate
├── candidateId
├── modelId
├── deploymentId
├── providerId
├── providerConfigurationId
├── executionMode
├── region?
├── capabilityMatch
├── policyMatch
├── estimatedCost?
├── latencyProjection?
├── qualityProjection?
├── healthState
├── preferenceScore?
├── totalScore?
├── rejectionReasons[]
└── warnings[]
```

Rejected candidates MAY be retained only in diagnostic trace rather than the final Route Plan.

---

# Route Plan

Routing produces:

```text
RoutePlan
├── routePlanId
├── requestId
├── selectedRoute
├── alternateRoutes[]
├── routingPolicyRevision
├── decisionInputs
├── hardConstraintsApplied[]
├── preferenceSummary
├── estimatedCost?
├── warnings[]
├── decisionHash
├── createdAt
└── expiresAt?
```

---

# Selected Route

Recommended:

```text
SelectedRoute
├── modelId
├── deploymentId
├── providerId
├── providerConfigurationId
├── executionMode
├── region?
├── streamingMode
├── effectiveModelParameters?
├── timeoutClass?
└── adapterReference?
```

Provider-native request payload is NOT part of Route Plan.

---

# Route Plan Is Immutable

Once an execution attempt begins from a Route Plan:

```text
that Route Plan MUST NOT mutate
```

If routing is reevaluated:

```text
create a new Route Plan
```

This is important for Retry/Fallback provenance.

---

# Route Plan Expiry

Because Routing uses dynamic inputs, a Route Plan MAY have a freshness/expiry boundary.

Example:

```text
expiresAt
```

A queued operation that begins much later MAY require route revalidation.

---

# Route Revalidation

Before provider execution, runtime MAY verify:

* Deployment still available,
* Policy still permits execution if required,
* credentials/config still valid,
* route not expired.

Material changes MAY trigger rerouting.

---

# Rerouting

Rerouting creates a new Route Plan.

It MUST preserve:

```text
requestId
```

while recording:

```text
previousRoutePlanId
rerouteReason
```

where useful.

---

# Primary Route

The selected best candidate becomes the primary route.

Selection SHOULD be based on:

1. mandatory compatibility,
2. Policy,
3. availability,
4. Routing Policy scoring,
5. stable tie-breaker.

---

# Alternate Routes

Routing MAY return a ranked set of alternate compatible routes.

These can support later fallback.

Example:

```text
primary:
    Deployment A

alternates:
    Deployment B
    Deployment C
```

---

# Alternate Routes Are Not Fallback Execution

Critical distinction:

```text
Routing
    provides eligible alternatives
```

```text
Fallback
    decides when to move to another route
```

Routing MUST NOT execute the fallback chain.

---

# Retry Boundary

Retry normally reuses the same Route Plan when the route remains valid.

Whether a retry stays on the same route belongs to Retry Policy.

Routing MAY be invoked again when retry policy requests rerouting.

---

# Retry Is Not Routing Lifecycle

The Router MUST NOT maintain:

```text
retryCount
attemptCount
```

as canonical routing state.

Those belong to runtime attempts.

---

# Fallback Boundary

Fallback MAY request:

```text
next compatible alternate route
```

or:

```text
reroute under degraded constraints
```

according to `FALLBACK.md`.

Fallback MUST still pass Routing hard constraints.

---

# Degradation

A fallback policy MAY explicitly allow relaxed soft requirements.

Examples:

* quality tier lowered,
* streaming disabled,
* local route preferred,
* latency target relaxed.

Hard requirements MUST NOT be silently relaxed.

---

# Relaxable Constraint

Routing SHOULD distinguish:

```text
HARD
SOFT
DEGRADABLE
```

requirements where appropriate.

Example:

```text
structured output required
    HARD

streaming preferred
    SOFT

quality tier PREMIUM
    DEGRADABLE if policy allows
```

---

# Constraint Provenance

Constraints SHOULD preserve provenance.

Examples:

```text
Workspace Policy
AI Request
Routing Profile
Session Override
Operation Override
Safety
Cost Control
```

This enables explainable decisions.

---

# Decision Trace

Routing SHOULD support a bounded:

```text
RoutingDecisionTrace
├── candidateReference
├── accepted
├── rejectionReasons[]
├── constraintMatches[]
├── scoreComponents?
└── finalRank?
```

This is diagnostics/provenance.

It SHOULD NOT contain sensitive payload content.

---

# Decision Hash

A Route Plan MAY include:

```text
decisionHash
```

derived from normalized Routing inputs and selected candidate.

This supports:

* diagnostics,
* reproducibility,
* cache of routing decisions where safe,
* audit.

---

# Routing Cache

Routing decisions MAY be cached briefly when dynamic inputs are still valid.

A Routing Cache MUST consider freshness of:

* health,
* pricing,
* provider availability,
* Policy,
* deployment catalog.

Routing cache is separate from AI result cache.

---

# Routing Cache Boundary

A cached Route Plan MUST NOT be reused after its dynamic dependencies expire.

---

# Provider Adapter Boundary

Routing knows:

* provider identity,
* provider configuration reference,
* Deployment,
* Model capabilities.

Routing does NOT know:

* provider HTTP fields,
* SDK method names,
* provider request schema,
* authentication headers,
* provider-native Language codes.

Those remain in Provider Adapter.

---

# Provider Management Boundary

Provider Management owns:

* provider registration,
* Provider Configuration,
* credential references,
* provider capabilities,
* provider-level availability,
* provider health signals.

Routing consumes these projections/references.

---

# Model Catalog Boundary

Model Catalog owns:

* Model Descriptor,
* Deployment descriptor,
* capability metadata,
* model lifecycle metadata.

Routing consumes Catalog data.

It does not mutate Catalog state.

---

# Cost Control Boundary

Cost Control owns:

* budgets,
* usage state,
* quota,
* reservations,
* budget decisions.

Routing consumes cost constraints and estimates candidate costs.

---

# Observability Boundary

Observability owns:

* route decision latency,
* attempt execution metrics,
* real latency,
* real usage,
* real cost,
* health measurements.

Routing MAY consume derived projections.

It MUST NOT become authoritative telemetry storage.

---

# Routing Observability

Recommended metrics:

* routing request count,
* routing latency,
* candidate count,
* filtered candidate count,
* rejection reason count,
* selected model,
* selected Deployment,
* selected provider,
* estimated cost,
* health state,
* routing policy,
* reroute count,
* no-route count.

---

# Sensitive Data

Routing trace MUST NOT require:

* raw Prompt,
* source text,
* Character context,
* Glossary content,
* credentials.

It should operate primarily on:

* capabilities,
* IDs,
* constraints,
* sizes,
* hashes,
* projections.

---

# Routing Failures

Possible stable routing failures:

```text
ROUTING_NO_COMPATIBLE_ROUTE
ROUTING_CAPABILITY_UNAVAILABLE
ROUTING_MODEL_REQUIREMENT_UNSATISFIED
ROUTING_LANGUAGE_UNSUPPORTED
ROUTING_CONTEXT_LIMIT_UNSATISFIED
ROUTING_POLICY_DENIED
ROUTING_PRIVACY_CONSTRAINT_UNSATISFIED
ROUTING_REGION_UNAVAILABLE
ROUTING_ENTITLEMENT_MISSING
ROUTING_QUOTA_UNAVAILABLE
ROUTING_COST_LIMIT_EXCEEDED
ROUTING_ALL_DEPLOYMENTS_UNAVAILABLE
ROUTING_HEALTH_DATA_INVALID
ROUTING_POLICY_INVALID
ROUTING_CATALOG_UNAVAILABLE
ROUTING_ROUTE_EXPIRED
ROUTING_PINNED_ROUTE_INVALID
```

Provider execution failures belong to later stages.

---

# Failure Recovery

Routing failure MAY lead to:

* user-visible structured failure,
* requested constraint relaxation,
* alternative operation mode,
* local-only/manual mode,
* delayed retry after runtime conditions change.

Routing MUST NOT silently violate hard constraints merely to produce a route.

---

# No Compatible Route

When no candidate satisfies hard requirements, return:

```text
ROUTING_NO_COMPATIBLE_ROUTE
```

with machine-readable reasons.

Do NOT select the "closest" incompatible model unless an explicit degradation policy authorizes the changed requirements.

---

# Offline Routing

Offline/local execution is simply another routing constraint/candidate category.

Avoid special parallel architecture such as:

```text
normal routing
+
offline routing
```

Prefer one routing system with:

```text
executionMode = LOCAL
```

and corresponding constraints.

---

# Local Hardware

Local routes MAY depend on runtime resource availability such as:

* RAM,
* VRAM,
* CPU capability,
* GPU capability,
* model loaded state.

These inputs belong to Runtime/Provider Management health projections.

---

# Routing and Streaming

If Request requires:

```text
streamingRequired = true
```

non-streaming candidates are incompatible.

If streaming is only preferred, Routing Policy may score streaming-capable candidates higher.

---

# Routing and Structured Output

If exact schema output is mandatory, candidates must meet the required structured-output level.

Example:

```text
required:
    STRICT_SCHEMA
```

Candidates with only:

```text
PROMPT_ONLY
```

must be rejected unless degradation policy explicitly permits weaker semantics.

---

# Routing and Safety

If Safety architecture requires a specific execution property, it becomes a hard constraint.

Examples:

* local only,
* approved provider class,
* provider with required content controls.

Fallback MUST preserve it.

---

# Routing and Cache

AI Result Cache may produce a valid response before Model Execution.

Where cache is evaluated before Routing, no route may be required for a cache hit.

Where cache policy requires model-equivalent identity, Routing/model metadata MAY participate in cache lookup.

Exact ordering belongs to `CACHE.md` and Pipeline orchestration.

---

# Routing and Prompt Limits

Prompt/Input Construction MAY later discover actual serialized input exceeds the selected route's effective limits.

Recovery MAY:

1. rerun Context Reduction,
2. choose another route,
3. fail.

Prompt Builder MUST NOT silently drop required context.

---

# Routing and Model Retirement

Deprecated models SHOULD normally receive lower preference.

Retired models MUST normally be excluded.

Pinned historical/debug operations MAY return a structured unavailability error rather than silently substituting another model.

---

# Routing and Model Alias

Mutable provider aliases SHOULD be treated conservatively where reproducibility matters.

Routing MAY prefer exact model versions for reproducibility-sensitive workloads.

---

# Routing and Reproducibility

Routing cannot guarantee that future execution selects the same route unless exact dynamic inputs are preserved/pinned.

For historical provenance, durable execution SHOULD preserve:

```text
routePlanId
modelId
deploymentId
providerId
routingPolicyRevision
relevant dynamic-input references
```

---

# Architecture Invariants

1. Routing selects AI execution routes; it does not execute models.

2. Routing is provider-neutral.

3. Routing operates on capabilities and constraints rather than provider API details.

4. Routing normally selects a Model Deployment, not merely a provider.

5. Routing output is a Route Plan.

6. Route Plan is separate from Provider Request.

7. Model Catalog and Routing are separate concerns.

8. Provider Management and Routing are separate concerns.

9. Policy ownership remains outside Routing.

10. Cost/Budget ownership remains outside Routing.

11. Health-check ownership remains outside Routing.

12. Routing consumes explicit Policy/Cost/Health inputs or projections.

13. Hard constraints are evaluated before preference scoring.

14. A candidate violating a hard constraint MUST NOT be selected.

15. Capability support MUST be checked before ranking.

16. Provider preference MUST NOT substitute for capability matching.

17. Language matching uses canonical Language semantics.

18. Provider-specific Language codes MUST NOT enter Routing contracts.

19. Context/output limits MUST be evaluated before selection.

20. Required Prompt/Instruction capabilities MUST be respected.

21. Routing MUST NOT select a model that requires weakening mandatory instruction semantics.

22. Workspace Policy denial MUST NOT be bypassed.

23. Safety restrictions MUST NOT be bypassed.

24. Entitlement and availability are separate.

25. Availability and Policy compatibility are separate.

26. Health is dynamic and freshness-aware.

27. Stale health MUST NOT be treated as indefinitely current.

28. Cost estimation uses constraints; Routing does not own mutable daily/monthly budget state.

29. User preferences influence ranking but do not override hard constraints.

30. Preferred provider/model are soft preferences unless explicitly pinned.

31. Exact route pinning is exceptional and explicit.

32. Routing Policy defines ranking behavior, not hard Policy authorization.

33. Candidate ranking SHOULD be deterministic for identical complete routing inputs.

34. Same AI Request MAY produce different routes when dynamic inputs differ.

35. Stable tie-breaking SHOULD be deterministic.

36. Route Plan is immutable once used for an attempt.

37. Rerouting creates a new Route Plan.

38. Route Plan MAY expire because dynamic inputs become stale.

39. Alternate routes MAY be included for recovery.

40. Alternate routes are not automatically executed by Routing.

41. Retry and Routing are separate concerns.

42. Fallback and Routing are separate concerns.

43. Fallback MAY consume alternate routes or request rerouting.

44. Fallback MUST revalidate hard compatibility constraints.

45. Hard requirements MUST NOT be silently degraded.

46. Degradable/soft requirements MUST be explicit.

47. Route decisions SHOULD be explainable.

48. Decision traces SHOULD avoid sensitive content.

49. Provider-specific request construction occurs after Routing.

50. Provider credentials MUST NOT appear in Route Plan.

51. Routing MUST NOT mutate Model Catalog.

52. Routing MUST NOT mutate Provider Health state.

53. Routing observability is separate from runtime execution telemetry.

54. A no-route result MUST be explicit rather than selecting an incompatible candidate.

55. Local/offline execution uses the same routing abstraction.

56. Historical durable AI-backed outputs SHOULD preserve actual route provenance.

57. Routing reproducibility requires preserving complete relevant routing inputs, not AI Request alone.

58. New providers/models SHOULD be routable through Catalog/Deployment/Adapter registration without changing business capabilities.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* Route Plan,
* Model Deployment selection,
* local and cloud candidates,
* task capability matching,
* text modality,
* optional image modality,
* source/target Language compatibility,
* Context-window requirement,
* maximum output requirement,
* basic structured-output requirement,
* basic streaming requirement,
* Workspace Policy constraints,
* local/cloud privacy constraint,
* provider allow/deny constraint,
* region constraint,
* Entitlement check,
* Cost Constraint,
* estimated cost,
* deployment availability,
* basic health state with freshness,
* `BALANCED`,
* `LOWEST_COST`,
* `LOWEST_LATENCY`,
* `QUALITY_WITHIN_BUDGET`,
* `OFFLINE_FIRST`,
* basic user preference,
* deterministic ranking,
* stable tie-break,
* primary route,
* ranked alternate routes,
* immutable Route Plan,
* Route Plan provenance,
* routing decision trace,
* structured no-route errors,
* rerouting after stale/unavailable route.

MVP MAY defer:

* complex weighted quality models,
* real-time capacity-aware routing,
* provider racing,
* multi-provider parallel execution,
* adaptive routing learning,
* automatic benchmark-based scoring,
* advanced quota reservation,
* cross-region optimization,
* dynamic provider arbitrage,
* model ensembles,
* speculative routing,
* complex SLA contracts,
* per-user learned route preferences,
* routing-marketplace policies.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact `RoutingInput` schema,
* exact `RouteCandidate` schema,
* exact `RoutePlan` schema,
* whether Route Plan is persisted,
* Route Plan expiry/TTL,
* whether route revalidation always occurs before execution,
* exact hard/soft/degradable constraint model,
* Routing Policy representation,
* scoring formula,
* scoring normalization,
* quality-projection ownership,
* latency projection window,
* health freshness TTL,
* behavior for `UNKNOWN` health,
* rate-limit pressure input,
* local capacity modeling,
* exact LanguagePair matching fallback,
* exact structured-output compatibility levels,
* exact Prompt capability matching,
* cost-estimation accuracy,
* pricing snapshot retention,
* entitlement integration,
* quota integration,
* whether Budget reservation happens before or after Route selection,
* preferred-provider semantics,
* exact-model pinning authorization,
* alternate-route count,
* whether alternates are persisted or recomputed,
* routing decision-cache strategy,
* Decision Trace retention,
* Decision Hash composition,
* route provenance persisted in AIResponse,
* route behavior after Model deprecation,
* reproducibility-sensitive route policy,
* cache-before-routing vs routing-before-cache ordering.

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
* `STREAMING.md`
* `RETRY.md`
* `FALLBACK.md`
* `COST_CONTROL.md`
* `CACHE.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/LANGUAGE.md`
* `../domain/PROFILE.md`
* `../domain/WORKSPACE.md`
* `../domain/SESSION.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`
* `../../02-modules/translation/`
* `../../02-modules/recognition/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
