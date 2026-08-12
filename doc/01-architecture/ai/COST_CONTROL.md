# AI Cost Control

* **Document:** AI Architecture / Cost Control
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI estimates, constrains, attributes and controls the cost of AI execution.

Cost Control helps CRAI achieve:

* predictable spending,
* bounded execution,
* provider independence,
* cost-aware routing,
* explicit budget enforcement,
* explainable optimization,
* accurate usage attribution.

Cost Control does NOT directly execute models or own Routing.

---

# Core Principle

```text
AI Request
    +
Resolved Execution Requirements
        |
        v
Cost Estimation
        |
        v
Budget / Quota Evaluation
        |
        v
Cost Constraints
        |
        v
Routing / Recovery
        |
        v
Execution
        |
        v
Actual Usage
        |
        v
Cost Reconciliation
```

Cost Control provides constraints and accounting.

Other components decide how to execute within those constraints.

---

# Scope

Cost Control covers:

* provider-independent cost units,
* pricing metadata consumption,
* pre-route estimation,
* post-route refined estimation,
* operation cost limits,
* Workspace/Project budget evaluation,
* quota checks,
* cost reservations,
* actual usage attribution,
* actual-cost reconciliation,
* retry/fallback cost budgets,
* cost observability,
* cost-related decisions.

---

# Non-Goals

Cost Control does NOT own:

* Routing candidate selection,
* Context reduction implementation,
* Prompt construction,
* Retry algorithms,
* Fallback selection,
* Model Catalog,
* Provider Configuration,
* billing-provider APIs,
* Workspace lifecycle,
* business-domain commit.

---

# Cost Concepts

CRAI SHOULD distinguish:

```text
Pricing
Estimate
Budget
Quota
Reservation
Usage
Actual Cost
Cost Decision
```

These are related but different concepts.

---

# Pricing

Pricing describes the cost rules for using an execution resource.

Recommended:

```text
PricingSnapshot
├── pricingSnapshotId
├── modelId?
├── deploymentId?
├── providerId
├── currency
├── effectiveFrom
├── effectiveTo?
├── inputPricing[]
├── outputPricing[]
├── modalityPricing[]
├── requestFees[]
├── minimumBillingRules[]
└── pricingVersion
```

Pricing is external/dynamic metadata.

---

# Pricing Ownership

Pricing SHOULD come from:

* Provider Management,
* administrator configuration,
* billing integration,
* provider metadata sync.

Cost Control consumes pricing.

It MUST NOT hard-code provider prices.

---

# Pricing Freshness

Pricing MUST have freshness/effective-date semantics.

An estimate SHOULD preserve which pricing version/snapshot was used.

---

# Provider Independence

Canonical Cost Control MUST NOT depend on provider-specific fields such as:

```text
input_token_price_openai
gemini_cached_token_rate
provider_X_unit
```

Provider adapters normalize provider billing dimensions into CRAI cost components.

---

# Cost Components

Possible normalized components include:

```text
INPUT_TEXT_UNITS
OUTPUT_TEXT_UNITS
IMAGE_INPUT
AUDIO_INPUT
VIDEO_INPUT
EMBEDDING_INPUT
TOOL_EXECUTION
REQUEST_FEE
CACHE_WRITE
CACHE_READ
LOCAL_COMPUTE
CUSTOM
```

Not every execution uses every component.

---

# Cost Units

CRAI MUST NOT assume all AI cost is token-based.

Possible units include:

```text
TOKEN
CHARACTER
BYTE
IMAGE
PIXEL
AUDIO_SECOND
VIDEO_SECOND
REQUEST
COMPUTE_SECOND
CUSTOM
```

---

# Token Awareness

Token estimation remains important for many language models.

But:

```text
Cost Control
    !=
Token Control only
```

---

# Cost Estimate

A `CostEstimate` predicts the expected cost of a possible execution.

Recommended:

```text
CostEstimate
├── estimateId
├── requestId
├── routePlanId?
├── pricingSnapshotId?
├── estimatedInputUsage
├── estimatedOutputUsage
├── estimatedCost
├── currency
├── confidence?
├── estimationMethod
├── estimatorVersion
└── createdAt
```

---

# Pre-Route Estimate

Before Routing, CRAI MAY compute a provider-neutral preliminary estimate.

Possible inputs:

* source size,
* Context size,
* expected output size,
* modality count,
* cost tier,
* generic pricing envelope.

This estimate supports:

* early budget rejection,
* candidate filtering,
* operation sizing.

---

# Post-Route Estimate

After Routing selects a RoutePlan, CRAI SHOULD refine cost when exact route characteristics matter.

Inputs MAY include:

* selected model,
* deployment,
* provider pricing,
* exact tokenization,
* structured-output overhead,
* image-processing fees,
* provider-specific billing rules.

---

# Estimation Pipeline

Conceptually:

```text
Request / Context
      |
      v
Pre-Route Estimate
      |
      v
Routing
      |
      v
RoutePlan
      |
      v
Refined Estimate
      |
      v
Reservation / Execution Decision
```

Not every execution requires both stages.

---

# Estimate Is Not Actual Cost

Critical rule:

```text
Estimated Cost
    !=
Actual Cost
```

Estimates guide decisions.

Actual accounting follows execution/provider usage.

---

# Estimation Confidence

Estimate confidence MAY be:

```text
HIGH
MEDIUM
LOW
UNKNOWN
```

Low-confidence estimates MAY require larger safety margin.

---

# Output Estimation

Expected output usage may derive from:

* Request output limit,
* historical capability data,
* Prompt/Profile settings,
* model defaults,
* user-requested maximum.

It MUST remain bounded by execution constraints.

---

# Safety Margin

Cost estimation MAY include a configurable:

```text
costSafetyMargin
```

or:

```text
usageSafetyMargin
```

to prevent optimistic budget overrun.

---

# Budget

Budget represents allowed spending over a defined scope and period.

Recommended:

```text
BudgetPolicy
├── budgetPolicyId
├── scope
├── amount
├── currency
├── period
├── enforcementMode
├── warningThresholds[]
├── reservationPolicy?
└── revision
```

---

# Budget Scopes

Possible scopes:

```text
OPERATION
SESSION
PROJECT
WORKSPACE
PRINCIPAL
CUSTOM
```

Organization-level budget may map to Workspace in current CRAI architecture unless a separate organization domain is introduced.

---

# Budget Period

Possible periods:

```text
NONE
OPERATION
SESSION
DAILY
WEEKLY
MONTHLY
CUSTOM
```

---

# Budget Ownership

Budget configuration belongs to Workspace/Governance/Usage policy.

Cost Control evaluates it.

AI Request MAY carry operation-level limits or stable budget-policy references.

---

# Operation Cost Constraint

Recommended:

```text
AICostConstraints
├── maximumEstimatedCost?
├── maximumFinalCost?
├── costTier?
├── allowPaidFallback?
├── budgetPolicyReference?
└── currency?
```

This is operation intent/constraint.

It is NOT current Workspace usage balance.

---

# Current Budget State

Mutable budget consumption SHOULD remain in Usage/Cost infrastructure.

Example:

```text
BudgetState
├── budgetPolicyId
├── consumed
├── reserved
├── remaining
├── periodStart
├── periodEnd
└── version
```

---

# Budget Decision

Recommended:

```text
CostDecision
├── decision
├── estimatedCost
├── remainingBudget?
├── reservationRequired
├── maximumAllowedCost?
├── reasonCodes[]
├── budgetPolicyRevision?
├── quotaDecisionReferences[]
└── evaluatedAt
```

Possible decisions:

```text
ALLOW
ALLOW_WITH_WARNING
ALLOW_WITH_CONSTRAINTS
REQUIRE_RESERVATION
DENY
```

---

# Budget Exceeded

When estimated cost exceeds a hard budget:

```text
DENY
```

or:

```text
ALLOW_WITH_CONSTRAINTS
```

only if an explicit degradation/optimization policy exists.

Cost Control MUST NOT silently exceed hard budget.

---

# Warning Threshold

Budget MAY define soft warning thresholds such as:

```text
80%
90%
```

These SHOULD NOT automatically block execution unless policy says so.

---

# Quota

Quota constrains resource consumption.

Examples:

```text
maximum requests
maximum input units
maximum output units
maximum total units
maximum spend
maximum concurrent executions
maximum premium-route executions
```

---

# Quota vs Budget

```text
Budget
    = monetary/cost allowance
```

```text
Quota
    = resource/capability allowance
```

They MUST remain distinct.

---

# Entitlement vs Quota

Entitlement answers:

```text
May this Workspace use this capability?
```

Quota answers:

```text
How much of it may be consumed?
```

---

# Quota Decision

Recommended:

```text
QuotaDecision
├── quotaId
├── decision
├── requestedAmount
├── remainingAmount?
├── resetAt?
└── reasonCode?
```

---

# Reservation

For expensive or concurrent operations, CRAI MAY reserve expected budget/quota before execution.

Conceptually:

```text
Estimate
    |
    v
Reservation
    |
    +--> Execution
    |
    +--> Release
```

---

# Cost Reservation

Recommended:

```text
CostReservation
├── reservationId
├── workspaceId
├── projectId?
├── requestId
├── routePlanId?
├── reservedAmount
├── currency
├── status
├── expiresAt
├── createdAt
└── version
```

---

# Reservation Status

Possible:

```text
RESERVED
CONSUMED
RELEASED
EXPIRED
ADJUSTED
FAILED
```

---

# Why Reservation Exists

Without reservation:

```text
Request A checks remaining budget = 10
Request B checks remaining budget = 10
```

both may proceed and overspend.

Reservation reduces race conditions.

---

# Reservation Boundary

Reservation belongs to Cost/Usage infrastructure.

Routing, Retry and Fallback consume remaining Recovery/Cost budget.

They MUST NOT directly mutate BudgetState.

---

# Retry Cost Budget

Retry MAY receive:

```text
remainingRetryCost
```

or a generic:

```text
RecoveryBudget
```

Cost Control remains authoritative for actual/reserved cost state.

---

# Fallback Cost Budget

Fallback MAY be allowed to:

* use a more expensive compatible route,
* use only cheaper alternatives,
* prohibit paid fallback.

These are explicit constraints.

---

# Recovery Budget

Recommended conceptual structure:

```text
RecoveryBudget
├── maximumAdditionalCost?
├── remainingCost?
├── remainingAttempts?
├── remainingRoutes?
├── deadline?
└── costPolicyReference?
```

Retry/Fallback consume this budget.

---

# Cost Optimization

Cost Optimization means changing **permitted execution choices** while preserving required semantics.

Possible cost-driven strategies include:

```text
prefer cheaper compatible route
reduce optional Context
reduce optional metadata
disable optional streaming
use Cache
use local execution
reduce degradable quality tier
```

Only explicit degradable/soft aspects may change.

---

# Optimization Ownership

Cost Control decides:

```text
what cost constraint or optimization target applies
```

Other components execute it:

```text
Context Assembly
    reduces optional Context

Routing
    selects cheaper compatible route

Fallback
    permits/records degradation

Cache
    reuses prior computation
```

Cost Control MUST NOT perform those transformations directly.

---

# Context Optimization

If cost policy requires smaller input:

```text
Cost Control
    ->
Context Constraint
    ->
Context Assembly
```

Context Assembly remains authoritative for semantic context reduction.

---

# Model Optimization

If cost policy prefers cheaper execution:

```text
Cost Control
    ->
Cost Constraint
    ->
Routing
```

Routing selects the route.

Cost Control MUST NOT directly select Model/provider.

---

# Offline / Local Preference

`OFFLINE_PREFERRED` is primarily a Routing/Privacy preference.

Cost Control MAY contribute:

```text
prefer zero provider spend
```

but MUST NOT own local/cloud semantics.

---

# Quality First

`QUALITY_FIRST` is primarily a Routing Policy.

Cost Control MAY say:

```text
maximumCost = ...
```

while Routing balances quality within that bound.

---

# Cost Policies

Cost-specific policy intents SHOULD focus on cost semantics.

Examples:

```text
HARD_COST_CAP
TARGET_COST
LOWEST_COST_WITHIN_REQUIREMENTS
WARN_ABOVE_THRESHOLD
UNMETERED_PREFERRED
NO_PAID_FALLBACK
```

Broader execution strategies belong to Routing Policy.

---

# Hard vs Soft Cost Constraint

Examples:

```text
maximumFinalCost
    HARD
```

```text
prefer low cost
    SOFT
```

```text
quality tier may reduce to stay under cost
    DEGRADABLE
```

---

# Functional Correctness

Cost optimization MUST NOT violate hard semantic requirements.

Cost savings MUST NOT override:

* Safety,
* Policy,
* required output contract,
* required context,
* required Language semantics,
* tenant isolation.

---

# Safety Priority

Cost Control MUST NOT bypass Safety.

Conceptually:

```text
Safety / Policy hard constraints
    >
Cost optimization
```

---

# Cache

A compatible cache hit may reduce cost to near-zero provider execution.

Cost Control MAY account for:

```text
estimated avoided cost
```

Cache remains separate architecture.

---

# Provider-Side Cache

Provider prompt/context caching may affect actual price.

Provider Adapter/Pricing normalization SHOULD expose this through usage/pricing metadata.

Cost Control does not depend on provider-specific cache APIs.

---

# Actual Usage

After execution, CRAI SHOULD collect normalized Usage.

Recommended:

```text
AIUsage
├── usageId
├── workspaceId
├── projectId?
├── requestId
├── responseId?
├── routePlanId?
├── attemptId?
├── providerId?
├── modelId?
├── deploymentId?
├── components[]
├── occurredAt
└── correlationId
```

---

# Usage Component

Recommended:

```text
UsageComponent
├── type
├── quantity
├── unit
├── pricingReference?
├── estimatedCost?
└── actualCost?
```

---

# Usage Ownership

Usage Ledger SHOULD be authoritative for actual resource consumption.

Cost Control evaluates/aggregates Usage.

AIResponse MAY expose a usage summary, but SHOULD NOT become the authoritative ledger.

---

# Attempt-Level Usage

Each model attempt MAY incur cost.

Therefore:

```text
Request cost
    =
sum of attempt costs
+
other billable components
```

not merely final successful attempt cost.

---

# Failed Attempt Cost

Failed/retried/fallback attempts MAY still incur provider cost.

Usage accounting MUST retain them.

---

# Streaming Usage

Cancelled or partial streams MAY still incur cost.

Streaming architecture provides usage signals.

Cost/Usage infrastructure reconciles them.

---

# Cache Usage

Pure cache hit SHOULD NOT fabricate provider usage.

It MAY record:

```text
CACHE_READ
```

or avoided-cost analytics separately.

---

# Local Execution Cost

Local execution may have:

```text
provider monetary cost = 0
```

but still consume:

* CPU,
* GPU,
* electricity,
* device resources.

MVP MAY model local monetary cost coarsely or as zero.

---

# Actual Cost

Actual cost SHOULD be calculated from:

```text
normalized usage
+
applicable pricing
```

where provider final billed amount is unavailable.

If provider returns authoritative billed cost, CRAI MAY retain it separately.

---

# Cost Reconciliation

Recommended:

```text
Estimate
    |
    v
Reservation
    |
    v
Execution
    |
    v
Actual Usage
    |
    v
Actual Cost
    |
    v
Reservation Adjustment
```

---

# Reservation Adjustment

Examples:

```text
reserved 0.10
actual 0.07
    -> release 0.03
```

```text
reserved 0.10
actual 0.12
    -> consume 0.10 + overage handling
```

Exact overage policy must be explicit.

---

# Final Cost Unavailable

Some providers may not provide immediate final cost.

CRAI MAY maintain:

```text
ESTIMATED
PROVISIONAL
FINAL
RECONCILED
```

cost states.

---

# Cost Record Status

Recommended:

```text
CostRecordStatus
├── ESTIMATED
├── RESERVED
├── PROVISIONAL
├── FINAL
└── RECONCILED
```

These are accounting states.

---

# Currency

Cost records MUST preserve currency.

Currency conversion SHOULD be explicit.

Do NOT silently sum:

```text
USD + VND
```

without normalized conversion.

---

# FX Conversion

If CRAI supports multiple currencies:

```text
FX rate reference
FX timestamp
```

SHOULD be retained for converted reporting.

MVP MAY use one configured billing currency.

---

# Pricing Changes

Pricing changes affect future estimates.

They MUST NOT silently rewrite historical actual-cost records.

---

# Model Alias Changes

If a mutable model alias changes underlying pricing, estimates MUST use the current resolved pricing snapshot.

Historical Usage retains historical route/pricing provenance.

---

# Unknown Pricing

Unknown pricing does not always mean execution must fail.

Policy may define:

```text
DENY_UNKNOWN_COST
ALLOW_WITH_WARNING
ALLOW_UNMETERED
ALLOW_LOCAL_ONLY
```

---

# Cost Estimation Failure

If hard budget enforcement requires reliable estimate and estimation fails:

```text
fail closed
```

may be necessary.

If execution is local/unmetered, policy MAY continue.

---

# Quota Failure

Quota violation SHOULD return structured failure.

Fallback MAY select a route using another permitted quota class if policy allows.

---

# Budget Concurrency

Budget/Reservation updates SHOULD use concurrency control.

Possible:

* optimistic version,
* atomic decrement,
* transactional reservation.

Implementation belongs to infrastructure.

---

# Budget Period Reset

Period reset MUST be deterministic and timezone-aware where needed.

Canonical accounting timestamps SHOULD remain UTC.

Workspace timezone MAY affect reporting periods if configured.

---

# Cost Control and Routing

Routing consumes:

```text
CostConstraints
CostDecision
PricingSnapshot
EstimatedRouteCost
```

Routing does NOT own mutable budget balance.

---

# Cost Control and Retry

Retry consumes remaining:

```text
cost budget
```

and expected next-attempt cost.

Retry MUST stop when another attempt violates hard cost constraints.

---

# Cost Control and Fallback

Fallback consumes:

```text
RecoveryBudget
```

and allowed degradation/cost rules.

Fallback MUST NOT silently choose paid execution when prohibited.

---

# Cost Control and Safety

Safety hard constraints always remain authoritative.

Cheaper unsafe execution is not a valid optimization.

---

# Cost Control and Context

Cost Control may constrain maximum affordable input/output size.

Context owns semantic reduction.

Required context MUST NOT be dropped solely for cost.

---

# Cost Control and Streaming

Streaming MAY:

* reduce perceived latency,
* increase partial billed usage on cancellation,
* alter output generation quantity.

Cost evaluation MAY consider streaming mode where pricing materially differs.

---

# Cost Control and Cache

Cache MAY reduce cost.

Cost analytics MAY report:

```text
avoided estimated cost
```

but avoided cost is not actual spend.

---

# Cost Control and Model Catalog

Model Catalog MAY expose coarse cost class and pricing references.

Cost Control uses pricing snapshots for actual estimate/accounting.

---

# Cost Control and Provider Management

Provider Management owns provider configuration and provider pricing acquisition where implemented.

Cost Control normalizes and evaluates pricing.

---

# Cost Control and Billing

AI Cost Control and external Billing are distinct.

```text
Cost Control
    = internal execution economics
```

```text
Billing
    = charging/invoicing/subscription
```

CRAI MAY use cost records as Billing inputs.

---

# Cost Control and Audit

Material decisions MAY be auditable:

* request rejected for budget,
* premium fallback allowed,
* manual budget override,
* budget changed,
* quota changed.

Audit persistence remains separate.

---

# Cost Observability

Recommended metrics:

* pre-route estimated cost,
* refined estimated cost,
* actual cost,
* estimate error,
* input/output usage,
* retry cost,
* fallback cost,
* cache avoided-cost estimate,
* budget utilization,
* reservation utilization,
* quota rejection count,
* budget rejection count,
* unknown-pricing count,
* optimization count.

---

# Estimate Accuracy

Useful metric:

```text
estimateError
    =
actualCost - estimatedCost
```

or normalized percentage error.

This helps improve estimators.

---

# Savings

Cost savings SHOULD be defined carefully.

Possible:

```text
estimated baseline cost
-
actual cost
```

Baseline methodology MUST be explicit.

---

# Sensitive Observability

Cost telemetry SHOULD avoid raw source/prompt content.

IDs, quantities and pricing references are normally sufficient.

---

# Failure Conditions

Possible stable failures:

```text
COST_PRICING_UNKNOWN
COST_ESTIMATION_FAILED
COST_ESTIMATE_LIMIT_EXCEEDED
COST_BUDGET_EXCEEDED
COST_QUOTA_EXCEEDED
COST_RESERVATION_FAILED
COST_RESERVATION_CONFLICT
COST_RESERVATION_EXPIRED
COST_CURRENCY_UNSUPPORTED
COST_RECONCILIATION_FAILED
COST_USAGE_MISSING
COST_PRICING_VERSION_INVALID
COST_POLICY_INVALID
COST_OPERATION_NOT_ALLOWED
```

---

# Recovery

Possible cost-directed recovery outcomes:

```text
REDUCE_OPTIONAL_CONTEXT
REQUEST_CHEAPER_ROUTE
DISABLE_OPTIONAL_FEATURE
REQUIRE_LOCAL_ROUTE
ALLOW_WITH_WARNING
REQUEST_USER_ACTION
DENY
```

Cost Control does not directly perform these actions.

---

# Architecture Invariants

1. Cost Control is provider-neutral.

2. Provider pricing MUST NOT be hard-coded into business logic.

3. Pricing, Estimate, Budget, Quota, Reservation and Usage are separate concepts.

4. Estimated Cost is separate from Actual Cost.

5. Pricing changes MUST NOT rewrite historical cost records.

6. Cost Control does not own Routing.

7. Cost Control does not directly select models/providers.

8. Cost Control does not own Context reduction.

9. Cost Control does not own Retry algorithms.

10. Cost Control does not own Fallback route selection.

11. Cost Control produces constraints/decisions consumed by Routing/Recovery.

12. Cost may be estimated before Routing.

13. Cost SHOULD be refined after Routing when route-specific pricing matters.

14. Token estimation does NOT universally have to precede provider selection.

15. Cost Control MUST NOT assume tokens are the only billable unit.

16. Model/Deployment pricing should be versioned or freshness-aware.

17. Unknown pricing behavior MUST be policy-defined.

18. Hard budget limits MUST NOT be silently exceeded.

19. Soft cost targets MAY influence Routing.

20. Routing Policy and Cost Policy are distinct.

21. `QUALITY_FIRST` is not owned solely by Cost Control.

22. `OFFLINE_PREFERRED` is not owned solely by Cost Control.

23. Cost optimization MUST preserve hard business requirements.

24. Cost optimization MUST preserve Safety.

25. Cost optimization MUST preserve Workspace Policy.

26. Cost optimization MUST preserve tenant isolation.

27. Required Context MUST NOT be dropped solely to reduce cost.

28. Optional Context MAY be reduced only through explicit Context policy.

29. Retry cost MUST be bounded.

30. Fallback cost MUST be bounded.

31. Paid fallback MUST be explicit when policy requires it.

32. Every billable attempt SHOULD be represented in Usage.

33. Failed attempts MAY still incur cost.

34. Cancelled streams MAY still incur cost.

35. Cache hits MUST NOT fabricate provider usage.

36. AIResponse usage summary is not the authoritative Usage Ledger.

37. Usage Ledger should be authoritative for actual execution consumption.

38. Reservations MAY be required for concurrent expensive execution.

39. Reservation prevents common budget race conditions.

40. Reservation state is separate from AI Request state.

41. Current Workspace budget balance MUST NOT be copied into every AI Request.

42. Request MAY carry operation-level Cost Constraints or stable budget-policy references.

43. Cost Decisions SHOULD be explainable.

44. Cost decisions SHOULD preserve applicable policy/pricing provenance.

45. Budget and Quota are distinct.

46. Entitlement and Quota are distinct.

47. Currency MUST be explicit.

48. Multi-currency aggregation requires explicit FX conversion.

49. Cost reconciliation SHOULD adjust reservations against actual usage.

50. Historical actual cost MUST remain auditable.

51. Cost observability MUST avoid sensitive source content.

52. Cache avoided cost is analytics, not actual spend.

53. Local execution may be unmetered monetarily but still consume compute resources.

54. Cost failures SHOULD use stable normalized error codes.

55. New providers/models SHOULD integrate through normalized pricing/cost metadata rather than Cost Control redesign.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* provider-neutral pricing metadata,
* pricing version/effective date,
* token input/output pricing,
* image pricing where applicable,
* pre-route estimate,
* post-route refined estimate,
* per-operation cost limit,
* Workspace monthly budget,
* optional Project budget,
* basic quota,
* basic CostDecision,
* simple CostReservation,
* Usage records,
* attempt-level Usage,
* actual-cost calculation,
* Retry cost accounting,
* Fallback cost accounting,
* cache avoided-cost estimate,
* cost observability,
* hard budget rejection,
* unknown-pricing policy,
* one configured reporting currency.

MVP MAY defer:

* complex multi-currency accounting,
* FX conversion,
* Session budgets,
* Principal budgets,
* weekly/custom budgets,
* advanced quota reservation,
* adaptive cost forecasting,
* provider-bill reconciliation,
* detailed local electricity/compute costing,
* complex billing integration,
* dynamic cost arbitrage,
* cost anomaly detection,
* predictive budget alerts,
* automated budget optimization.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact `PricingSnapshot` schema,
* normalized Cost Unit taxonomy,
* exact pre-route estimator,
* exact post-route estimator,
* estimation confidence model,
* output estimation algorithm,
* safety-margin defaults,
* exact BudgetPolicy schema,
* whether Project budget is in MVP,
* whether Session budget is useful,
* monthly-budget reset timezone,
* CostReservation necessity for MVP,
* reservation TTL,
* reservation concurrency implementation,
* overage behavior,
* exact Usage Ledger ownership,
* actual provider-cost ingestion,
* unknown-pricing defaults,
* one-vs-multi currency model,
* RecoveryBudget structure,
* relationship between RetryBudget/FallbackBudget/CostReservation,
* cost policy vs Routing Policy schema,
* cost-driven optional Context reduction,
* paid fallback UX,
* local compute costing,
* cache avoided-cost baseline,
* estimate error thresholds,
* pricing-sync ownership,
* model alias pricing changes,
* external Billing integration.

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
* `CACHE.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/WORKSPACE.md`
* `../domain/PROJECT.md`
* `../domain/SESSION.md`
* `../domain/PROFILE.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`
* `../../02-modules/translation/`

Infrastructure:

* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/logging/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
