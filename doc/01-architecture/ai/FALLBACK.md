# AI Fallback

* **Document:** AI Architecture / Fallback
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the fallback architecture for CRAI AI execution.

Fallback provides an alternative compatible execution path when the current Route Plan cannot or should not continue successfully.

Fallback exists to improve service continuity while preserving:

* business intent,
* mandatory capability requirements,
* Policy,
* Safety,
* privacy,
* output-contract correctness,
* tenant isolation,
* explicit degradation rules.

Fallback MAY change the execution route.

It MUST NOT silently change the semantic business request.

---

# Core Principle

```text
AI Request
    |
    v
Route Plan A
    |
    v
Execution Attempt
    |
    +--> Success
    |
    +--> Recoverable Failure
            |
            v
      Recovery Decision
            |
       +----+----+
       |         |
     Retry     Fallback
                 |
                 v
        Fallback Constraints
                 |
                 v
             Rerouting
                 |
                 v
           Route Plan B
                 |
                 v
          New Execution Attempt
```

Fallback does not mutate the failed Route Plan.

It results in another compatible Route Plan.

---

# Scope

Fallback MAY be used when:

* Retry is exhausted,
* retrying the current route is inappropriate,
* the current Deployment becomes unavailable,
* the selected Model becomes unavailable,
* current route health degrades beyond acceptable limits,
* route capability is no longer available,
* a provider-specific execution path fails persistently,
* an explicitly allowed degraded execution mode is required.

Fallback does NOT mean:

```text
try anything until some output appears
```

Every fallback route MUST remain compatible with the effective recovery constraints.

---

# Non-Goals

Fallback does NOT own:

* Model selection algorithms,
* Routing Policy,
* Provider Configuration,
* Model Catalog,
* Workspace Policy,
* Safety Policy,
* Cost Ledger,
* Retry algorithms,
* provider API execution,
* business-domain commit.

Routing remains responsible for selecting routes.

Fallback coordinates when and under which permitted constraints rerouting may occur.

---

# Fallback vs Retry

Critical distinction:

```text
Retry
    = another compatible attempt
      on the current Route Plan
```

```text
Fallback
    = another compatible execution path
      using a new Route Plan
```

Therefore:

```text
Retry
    preserves route identity
```

while:

```text
Fallback
    changes route identity
```

---

# Fallback vs Rerouting

Fallback determines:

```text
Why must another route be considered?

Which requirements remain mandatory?

Which soft/degradable constraints may change?
```

Routing determines:

```text
Which route satisfies those constraints?
```

Conceptually:

```text
Fallback Decision
        |
        v
Fallback Routing Request
        |
        v
Routing
        |
        v
New Route Plan
```

Fallback MUST NOT implement its own hidden model/provider selector.

---

# Fallback Architecture

Recommended components:

```text
Normalized Failure
        |
        v
Recovery Evaluation
        |
        v
Fallback Eligibility
        |
        v
Constraint Preservation
        |
        v
Allowed Degradation Evaluation
        |
        v
Fallback Routing Request
        |
        v
Routing
        |
        v
New Route Plan
        |
        v
Execution
```

Cross-cutting inputs include:

```text
Recovery Budget
Cancellation
Operation Deadline
Attempt History
Route History
Policy
Safety
Cost Constraints
```

---

# Fallback Trigger

Fallback MAY be considered after a normalized failure or route invalidation.

Typical triggers include:

```text
RETRY_EXHAUSTED
ROUTE_INVALID
DEPLOYMENT_UNAVAILABLE
MODEL_UNAVAILABLE
PROVIDER_UNAVAILABLE
CAPABILITY_TEMPORARILY_UNAVAILABLE
PERSISTENT_RATE_LIMIT
PERSISTENT_TIMEOUT
ROUTE_HEALTH_DEGRADED
ROUTE_COST_NO_LONGER_ALLOWED
```

The exact trigger MUST be explicit and observable.

---

# Retry Is Not Always Required First

Fallback MUST NOT require Retry to occur first in every case.

Example:

```text
Deployment:
    UNAVAILABLE
```

Retrying the same Deployment may have no value.

Recovery may choose:

```text
IMMEDIATE_FALLBACK
```

Likewise:

```text
Route Plan expired
Policy now invalidates route
Capability removed from Deployment
```

may justify rerouting without another same-route attempt.

---

# Recovery Decision

Recommended:

```text
RecoveryDecision
├── decision
├── normalizedFailure
├── currentRoutePlanId
├── retryEligible
├── fallbackEligible
├── reasonCode
├── recoveryBudgetReference?
└── evaluatedAt
```

Possible decisions:

```text
RETRY_CURRENT_ROUTE
FALLBACK
FAIL
CANCEL
WAIT
REQUEST_USER_ACTION
```

---

# Recovery Policy

The choice between Retry and Fallback SHOULD belong to a recovery policy/orchestrator.

It MAY consider:

* failure type,
* retryability,
* current health,
* remaining deadline,
* remaining cost,
* alternate-route availability,
* user latency preference,
* provider Retry-After,
* fallback degradation policy.

---

# Fallback Eligibility

Fallback SHOULD occur only if:

```text
operation not cancelled
AND
fallback permitted
AND
recovery budget remains
AND
mandatory Policy still permits execution
AND
Safety still permits execution
AND
another compatible route may exist
```

If not:

```text
STOP_FALLBACK
```

---

# Route History

Fallback MUST preserve route lineage.

Example:

```text
RoutePlan A
    |
    v
failure
    |
    v
RoutePlan B
    |
    v
failure
    |
    v
RoutePlan C
```

Each Route Plan remains immutable.

---

# Fallback Sequence

Runtime MAY represent:

```text
FallbackSequence
├── fallbackSequenceId
├── requestId
├── initialRoutePlanId
├── routePlanReferences[]
├── effectiveFallbackPolicyRevision
├── degradationHistory[]
├── finalOutcome
├── startedAt
└── endedAt?
```

Whether persisted is an implementation decision.

---

# Fallback Policy

A Fallback Policy defines what kinds of alternative execution are permitted.

Recommended:

```text
FallbackPolicy
├── fallbackPolicyId
├── revision
├── maximumFallbackRoutes
├── permittedRouteChanges[]
├── degradationRules[]
├── preserveRequirements[]
├── costRules
├── deadlineRules
├── localCloudRules?
└── stopConditions[]
```

---

# Fallback Policy Ownership

Fallback Policy belongs to recovery/runtime architecture.

Routing consumes fallback-adjusted requirements.

Routing MUST NOT own the fallback lifecycle.

---

# Fallback Constraints

A fallback routing request SHOULD explicitly separate:

```text
HARD
SOFT
DEGRADABLE
```

constraints.

---

# Hard Constraints

Hard constraints MUST survive every fallback.

Examples MAY include:

```text
required capability
required source/target Language semantics
required input modality
required output contract
mandatory structured-output level
required context
Policy
Safety
privacy
data residency
tenant isolation
authorization
```

A fallback route that violates any hard constraint MUST be rejected.

---

# Soft Constraints

Soft constraints affect ranking but MAY change without semantic degradation.

Examples:

```text
preferred provider
preferred model
preferred region
preferred latency class
streaming preferred
local preferred
```

They MAY be relaxed according to Routing/Fallback Policy.

---

# Degradable Constraints

Degradable constraints may be relaxed only when explicitly permitted.

Examples:

```text
quality tier
latency target
cost preference
optional context
streaming preference
optional metadata
```

Degradation MUST be visible.

---

# Constraint Provenance

Every important constraint SHOULD preserve its origin.

Examples:

```text
AI Request
Workspace Policy
Safety Policy
Routing Profile
Context Profile
Session Override
Operation Override
Cost Control
```

Fallback MUST NOT degrade a constraint merely because its authority is unknown.

---

# Capability Preservation

Fallback MUST preserve required AI capability.

Example:

```text
Request:
    TRANSLATION
```

Fallback candidates MUST still support:

```text
TRANSLATION
```

A generic text-generation model MAY be considered only if the Routing capability model explicitly says it satisfies the Translation requirement under current policy.

---

# Capability Is Structured

Fallback compatibility MAY include:

* task capability,
* Language pair,
* input modality,
* output modality,
* structured-output level,
* streaming requirement,
* context requirement,
* instruction capability,
* tool capability where needed.

It MUST NOT be reduced to a single boolean:

```text
supportsTranslation = true
```

when additional hard requirements exist.

---

# Context Preservation

Required semantic context MUST NOT be dropped merely to fit a fallback model.

Bad:

```text
required context does not fit
    ->
drop half the context
```

Preferred:

```text
required context does not fit
    ->
find larger compatible route
```

or:

```text
fail explicitly
```

---

# Optional Context Degradation

Optional context MAY be reduced if:

* Context Policy marks it degradable,
* operation semantics remain valid,
* degradation is recorded.

Context Assembly performs semantic reduction.

Fallback only authorizes the degradation class.

---

# Structured Output Preservation

If Request requires:

```text
STRICT_SCHEMA
```

fallback MUST NOT silently downgrade to:

```text
PROMPT_ONLY
```

unless the requirement was explicitly marked degradable.

---

# Streaming Preservation

If:

```text
streamingRequired = true
```

non-streaming candidates are incompatible.

If:

```text
streamingPreferred = true
```

fallback MAY disable streaming when policy allows.

---

# Quality Degradation

Quality MAY be degradable when explicitly allowed.

Example:

```text
PREMIUM
    ->
STANDARD
```

Fallback MUST record:

```text
degradation:
    QUALITY_TIER
```

It MUST NOT claim the fallback result used the original quality level.

---

# Latency Degradation

Fallback MAY accept slower execution when:

* deadline remains valid,
* user/operation policy permits,
* business intent remains unchanged.

Example:

```text
INTERACTIVE
    ->
STANDARD
```

---

# Cost Degradation

Fallback may sometimes use:

```text
more expensive route
```

when the operation cost budget permits it.

Or it may be required to use:

```text
cheaper route
```

after a budget-related trigger.

Cost behavior MUST be explicit.

---

# Local / Cloud Degradation

Switching between:

```text
LOCAL
CLOUD
```

is allowed only if Policy, privacy and Request constraints permit it.

Example:

```text
cloud unavailable
    ->
local
```

may be valid.

But:

```text
localOnly = true
    ->
cloud
```

is never a valid fallback.

---

# Provider Change

Changing provider is a common fallback route change.

It MUST occur through a new Route Plan.

Fallback MUST NOT directly construct:

```text
provider = B
```

without Routing validation.

---

# Model Change

Changing model likewise requires:

```text
new Route Plan
```

with complete capability validation.

---

# Deployment Change

Changing Deployment may be fallback even when Model remains identical.

Example:

```text
Model X / SG Deployment
    ->
Model X / US Deployment
```

Policy/data-residency constraints still apply.

---

# Region Change

Region changes MUST respect:

* data residency,
* privacy,
* latency constraints,
* provider availability.

Region MUST NOT be silently changed just because another endpoint exists.

---

# Offline Fallback

Local/offline execution is not a special universal last-resort tier.

It is simply another compatible Route Candidate.

Depending on policy:

```text
LOCAL_FIRST
```

may even make it the preferred route.

---

# No Universal Fallback Order

CRAI MUST NOT hard-code one universal order such as:

```text
same provider
then another provider
then cheaper model
then local
```

Candidate ordering belongs to Routing under the effective fallback constraints.

---

# Fallback Routing Request

Recommended:

```text
FallbackRoutingRequest
├── requestId
├── previousRoutePlanId
├── fallbackReason
├── excludedRouteIds[]
├── hardRequirements
├── softRequirements
├── degradableRequirements
├── permittedDegradations[]
├── recoveryCostConstraint
├── deadline
├── routeHistoryReference?
└── fallbackPolicyRevision
```

---

# Excluded Routes

Previously failed routes MAY be excluded from the next routing pass.

Example:

```text
Deployment A:
    hard failure
```

Routing SHOULD NOT immediately select Deployment A again unless recovery policy explicitly allows it.

---

# Exclusion Scope

Exclusion MAY apply to:

```text
exact Deployment
Model
Provider
Region
execution mode
```

depending on failure semantics.

A failure in one Deployment MUST NOT automatically blacklist every Deployment of the Model unless evidence supports that conclusion.

---

# Route Plan

Routing returns another immutable:

```text
RoutePlan
```

The new plan SHOULD reference:

```text
previousRoutePlanId
rerouteReason
```

where provenance requires it.

---

# Fallback Route Plan

Fallback route is not a special mutable variant of the original Route Plan.

It is a normal Route Plan produced under different explicit constraints.

---

# Fallback Budget

Fallback MUST be bounded.

Recommended:

```text
RecoveryBudget
├── remainingRoutes
├── remainingAttempts
├── remainingElapsedTime
├── remainingCost
├── remainingInputUnits?
├── remainingOutputUnits?
└── operationDeadline
```

---

# Budget Ownership

Fallback does NOT own Workspace:

* daily budget,
* monthly budget,
* Usage Ledger,
* billing state.

Cost Control provides applicable Recovery/Cost constraints.

---

# Maximum Fallback Routes

Fallback Policy SHOULD limit how many distinct route changes are allowed.

Example:

```text
maximumFallbackRoutes = 2
```

This prevents endless route cycling.

---

# Fallback Loop Prevention

Fallback loops are forbidden.

The recovery system SHOULD track attempted/excluded routes.

Example forbidden pattern:

```text
A
 ->
B
 ->
A
 ->
B
```

unless an explicit later recovery epoch makes those routes newly valid.

---

# Route Identity for Loop Detection

Loop detection SHOULD prefer:

```text
route semantic identity
```

such as:

```text
modelId
deploymentId
providerConfigurationId
executionMode
```

rather than raw provider request IDs.

---

# Retry Within Fallback Route

After Route Plan B is created:

```text
RoutePlan B
    |
    +--> Attempt 1
    +--> Attempt 2
```

Retry Policy MAY operate normally on Route B.

Thus:

```text
Fallback
    changes route

Retry
    may then retry that route
```

---

# Recovery Cycles

A recovery sequence MAY therefore look like:

```text
Route A
    Attempt 1
    Attempt 2

Fallback

Route B
    Attempt 1

Fallback

Route C
    Attempt 1
```

All steps remain bounded by the overall Recovery Budget.

---

# Fallback and Repair

Response Repair MAY occur before fallback.

Example:

```text
invalid structured output
    |
    +--> deterministic repair
    |
    +--> retry
    |
    +--> fallback to stronger schema-capable model
```

The order is recovery-policy-specific.

---

# Fallback and Cache

Before another expensive route executes, orchestration MAY check for a compatible cached result.

Fallback does not own Cache.

A cache result MUST still satisfy current semantic and Policy constraints.

---

# Fallback and Policy

Policy MUST be enforced for every fallback route.

A previous Policy approval for Route A MUST NOT automatically authorize Route B if relevant execution properties changed.

Examples:

* provider changed,
* region changed,
* local/cloud mode changed.

---

# Policy Re-Evaluation

Fallback SHOULD request Policy re-evaluation when route changes affect policy-sensitive attributes.

---

# Fallback and Safety

Safety constraints MUST survive fallback.

Fallback MUST NOT switch to a route that cannot enforce mandatory safety requirements.

---

# Safety Re-Evaluation

Safety MAY need re-evaluation when:

* model class changes,
* provider changes,
* execution modality changes,
* context representation changes materially.

Exact rules belong to `SAFETY.md`.

---

# Fallback and Authorization

Fallback MUST NOT use another provider/configuration merely because credentials exist.

The Workspace/principal must be authorized to use the alternative execution resource.

---

# Fallback and Entitlement

Alternative routes must satisfy entitlement.

Fallback MUST NOT silently upgrade into a paid/premium capability unavailable to the Workspace.

---

# Paid Fallback

User/Workspace intent MAY say:

```text
paidFallbackAllowed = false
```

Then paid alternatives are hard-excluded.

---

# User Preferences

Preferences MAY influence alternative ranking.

Examples:

* prefer local,
* avoid provider X,
* allow slower execution,
* avoid paid fallback.

They do not override hard constraints.

---

# User Interaction

Fallback SHOULD usually be automatic only when semantics remain within explicitly allowed bounds.

If fallback requires a material behavior change outside pre-approved degradation policy:

```text
request user action
```

rather than silently changing behavior.

---

# User-Visible Degradation

When fallback materially changes user experience, CRAI MAY surface:

* slower route,
* lower quality tier,
* streaming disabled,
* local model used,
* limited optional context.

Exact UI belongs to Presentation/Application layer.

---

# Fallback Result Provenance

Final AI Response SHOULD retain enough provenance to show that fallback occurred.

Possible:

```text
executionProvenance:
    fallbackUsed: true
    finalRoutePlanId: route_b
```

Detailed recovery history remains runtime-owned.

---

# Degradation Provenance

When semantics allow degraded execution:

```text
degradations[]
```

SHOULD record material changes.

Examples:

```text
QUALITY_TIER_REDUCED
STREAMING_DISABLED
OPTIONAL_CONTEXT_REDUCED
LATENCY_CLASS_RELAXED
```

---

# Fallback Decision

Recommended:

```text
FallbackDecision
├── decision
├── trigger
├── previousRoutePlanId
├── permittedDegradations[]
├── prohibitedDegradations[]
├── remainingFallbackRoutes
├── remainingDeadline?
├── remainingCostBudget?
├── reasonCode
└── evaluatedAt
```

Possible decisions:

```text
FALLBACK
NO_FALLBACK
FAIL
CANCEL
REQUEST_USER_ACTION
```

---

# Decision Explainability

Every automatic fallback SHOULD be explainable.

Example:

```text
trigger:
    DEPLOYMENT_UNAVAILABLE

decision:
    FALLBACK

preserved:
    TRANSLATION
    STRICT_SCHEMA
    LOCAL_ONLY

relaxed:
    QUALITY PREMIUM -> STANDARD
```

---

# Observability

Fallback observability SHOULD include:

* fallback count,
* fallback trigger,
* route transitions,
* excluded routes,
* selected alternative Route Plan,
* degradation types,
* additional latency,
* additional estimated/final cost,
* final success/failure,
* fallback-policy revision.

---

# Route Transition Metric

Useful metric:

```text
route_a -> route_b
```

SHOULD use internal IDs rather than sensitive provider payloads.

---

# Fallback Success Rate

Fallback success SHOULD be measured by trigger/capability.

Example:

```text
provider-timeout fallback success
```

is more useful than one global fallback percentage.

---

# Cost Attribution

Cost added by fallback SHOULD remain observable.

Cost Control/Usage remains authoritative for actual usage accounting.

---

# Sensitive Observability

Fallback diagnostics MUST NOT require:

* source text,
* Prompt content,
* Glossary context,
* Character context,
* credentials.

Prefer:

* IDs,
* capability flags,
* constraint categories,
* failure codes,
* route references.

---

# No Compatible Fallback

When Routing finds no compatible route under fallback constraints:

```text
FALLBACK_NO_COMPATIBLE_ROUTE
```

The recovery sequence MUST terminate or request user action.

Fallback MUST NOT select an incompatible candidate merely to avoid failure.

---

# Structured Failure

When recovery ends without a valid result, return a normalized:

```text
AIExecutionFailure
```

rather than an invalid semantic `AIResponse`.

---

# Failure Conditions

Fallback-specific failures MAY include:

```text
FALLBACK_DISABLED
FALLBACK_NOT_ELIGIBLE
FALLBACK_LIMIT_EXCEEDED
FALLBACK_DEADLINE_EXCEEDED
FALLBACK_COST_BUDGET_EXCEEDED
FALLBACK_CANCELLED
FALLBACK_NO_COMPATIBLE_ROUTE
FALLBACK_POLICY_DENIED
FALLBACK_SAFETY_DENIED
FALLBACK_ENTITLEMENT_MISSING
FALLBACK_HARD_CONSTRAINT_UNSATISFIED
FALLBACK_DEGRADATION_NOT_ALLOWED
FALLBACK_LOOP_DETECTED
FALLBACK_ROUTING_FAILED
FALLBACK_POLICY_INVALID
```

The original execution failure remains separately traceable.

---

# Determinism

Fallback decisions SHOULD be deterministic for identical:

* normalized failure,
* current Route Plan,
* Route history,
* effective Fallback Policy,
* Recovery Budget,
* Policy/Safety constraints,
* Routing inputs.

The same AI Request alone is NOT enough to guarantee the same fallback route.

---

# Dynamic Conditions

Fallback route may differ later because:

* health changed,
* pricing changed,
* provider availability changed,
* Policy changed,
* quota changed,
* Model Catalog changed.

This is legitimate when those inputs are explicit.

---

# Cancellation

Cancellation stops fallback.

After cancellation:

```text
NO NEW ROUTE
NO NEW ATTEMPT
```

unless an explicit user restart creates another operation/recovery sequence.

---

# Deadline

Fallback MUST respect the overall operation/recovery deadline.

If there is insufficient time for another viable route:

```text
STOP_FALLBACK
```

---

# Fallback vs Business Workflow

Fallback recovers AI execution.

It does NOT decide:

* Translation publication,
* OCR acceptance,
* Review decision,
* Session completion.

If no AI fallback succeeds, the calling capability determines business workflow behavior.

---

# Architecture Invariants

1. Fallback provides an alternative compatible execution path.

2. Fallback MAY change Model, Deployment, Provider, Region or execution mode only through a new Route Plan.

3. Fallback MUST NOT mutate the existing Route Plan.

4. Routing owns route selection.

5. Fallback MUST NOT implement a hidden secondary Router.

6. Retry and Fallback are separate recovery mechanisms.

7. Retry normally preserves Route Plan.

8. Fallback creates or requests a new Route Plan.

9. Fallback does NOT always require Retry exhaustion first.

10. Immediate fallback MAY occur when retrying the current route is inappropriate.

11. Recovery Policy decides Retry vs Fallback sequencing.

12. Fallback MUST preserve semantic AI Request intent.

13. Fallback MUST preserve hard capability requirements.

14. Fallback MUST preserve mandatory Policy constraints.

15. Fallback MUST preserve mandatory Safety constraints.

16. Fallback MUST preserve authorization and tenant isolation.

17. Fallback MUST preserve required output contracts.

18. Required context MUST NOT be silently dropped.

19. Required structured-output guarantees MUST NOT be silently weakened.

20. Required streaming MUST NOT be silently disabled.

21. Soft requirements MAY influence ranking.

22. Degradable requirements MAY be relaxed only when explicitly authorized.

23. Every material degradation SHOULD be recorded.

24. User preferences MUST NOT override hard constraints.

25. Preferred provider/model are not mandatory unless explicitly pinned.

26. There is no universal fixed fallback provider/model order.

27. Local/offline execution is an ordinary compatible route category.

28. Fallback selection MUST remain capability-driven.

29. Fallback MUST use canonical Language capability semantics.

30. Provider-specific Language identifiers MUST NOT enter generic Fallback logic.

31. Policy-sensitive route changes SHOULD be re-evaluated.

32. Safety-sensitive route changes SHOULD be re-evaluated.

33. Entitlement applies to fallback routes.

34. Fallback MUST NOT silently access unavailable premium capability.

35. Fallback MUST respect operation Cost constraints.

36. Fallback MUST NOT own daily/monthly Workspace budget state.

37. Fallback MUST be bounded by route/deadline/cost limits.

38. Fallback loops are prohibited.

39. Previously failed routes SHOULD be tracked/excluded according to failure semantics.

40. Failure of one Deployment does NOT automatically invalidate every Deployment of a Model.

41. A fallback Route MAY subsequently use ordinary Retry.

42. Retry attempts on a fallback Route remain distinct Attempt records.

43. Fallback and Repair remain separate mechanisms.

44. Fallback does not own Cache.

45. Cancellation stops further fallback.

46. Fallback MUST NOT continue beyond overall deadline.

47. No compatible fallback MUST return structured failure rather than an incompatible route.

48. AI execution failure is distinct from successful AIResponse.

49. Fallback decisions SHOULD be explainable.

50. Fallback decisions SHOULD be observable.

51. Fallback diagnostics SHOULD avoid sensitive content.

52. Historical execution provenance SHOULD preserve the final Route Plan and fallback usage.

53. Detailed route/attempt history remains runtime-owned.

54. Fallback decision determinism depends on complete recovery/routing inputs, not AI Request alone.

55. New models/providers can participate in fallback through ordinary Routing registration rather than fallback-specific code.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* explicit fallback enable/disable,
* normalized fallback triggers,
* immediate fallback for clearly invalid routes,
* fallback after Retry exhaustion,
* immutable Route Plan transitions,
* same-Model alternate Deployment,
* alternate Model,
* alternate Provider,
* local/cloud alternative where permitted,
* hard/soft/degradable constraint classification,
* required capability preservation,
* Language compatibility preservation,
* structured-output preservation,
* Context requirement preservation,
* Policy revalidation,
* Safety revalidation where necessary,
* Entitlement checks,
* Cost constraints,
* operation deadline,
* maximum fallback routes,
* loop prevention,
* failed-route exclusion,
* ranked rerouting,
* degradation provenance,
* FallbackDecision,
* structured no-fallback failure,
* fallback observability.

MVP MAY defer:

* adaptive fallback learning,
* automatic degradation optimization,
* user-interactive fallback negotiation,
* provider racing,
* parallel fallback,
* speculative secondary route,
* automatic benchmark-informed recovery,
* complex region migration,
* advanced circuit-breaker integration,
* resumable cross-provider streaming,
* dynamic semantic constraint relaxation,
* multi-stage agent fallback.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact `FallbackPolicy` schema,
* exact `FallbackDecision` schema,
* exact `FallbackRoutingRequest` schema,
* exact hard/soft/degradable constraint representation,
* default maximum fallback routes,
* whether Retry exhaustion is default before Fallback for common transient errors,
* immediate-fallback failure categories,
* failed-route exclusion TTL,
* route semantic identity for loop detection,
* whether same Model/different Deployment counts as fallback,
* whether simple region change always requires Policy re-evaluation,
* exact quality degradation tiers,
* whether optional Context reduction is allowed in MVP,
* streaming degradation semantics,
* strict-schema degradation rules,
* user notification thresholds,
* paid fallback defaults,
* local/cloud fallback preference,
* Recovery Budget ownership/model,
* relationship between RetryBudget and FallbackBudget,
* whether RecoveryPolicy receives a dedicated document,
* route-history persistence,
* FallbackSequence retention,
* final fallback provenance in AIResponse,
* cost attribution,
* integration with future Circuit Breaker,
* cache recheck placement,
* Repair-vs-Fallback ordering.

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
* `COST_CONTROL.md`
* `CACHE.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/PROFILE.md`
* `../domain/WORKSPACE.md`
* `../domain/SESSION.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/translation/`
* `../../02-modules/recognition/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`

Infrastructure:

* `../../03-infrastructure/scheduler/`
* `../../03-infrastructure/telemetry/`
