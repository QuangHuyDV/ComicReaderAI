# AI Retry Policy

* **Document:** AI Architecture / Retry
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the retry architecture for CRAI AI execution.

Retry improves reliability by repeating a compatible execution attempt when a failure is expected to be transient and repetition can occur safely.

Retry MUST remain:

* provider-neutral,
* failure-aware,
* bounded,
* idempotency-aware,
* deadline-aware,
* cost-aware,
* cancellation-aware,
* observable.

Retry does NOT change the semantic business intent of an AI Request.

---

# Core Principle

```text
AI Request
    |
    v
Route Plan
    |
    v
Execution Attempt
    |
    +--> Success
    |
    +--> Failure
            |
            v
     Failure Classification
            |
            v
       Retry Decision
            |
       +----+----+
       |         |
     RETRY    STOP_RETRY
       |
       v
     Backoff
       |
       v
New Execution Attempt
on compatible Route
```

Retry repeats execution.

It does NOT automatically change the selected model/provider route.

---

# Scope

Retry MAY apply to transient failures occurring during:

* provider connection,
* model invocation,
* streamed execution,
* temporary infrastructure access,
* transient dependency access,
* provider rate limiting,
* temporary provider capacity failure.

Retry MAY also apply to selected non-provider stages when their failure is explicitly classified as transient and retry-safe.

---

# Non-Goals

Retry does NOT own:

* Route selection,
* fallback route selection,
* Policy rules,
* Safety rules,
* Provider Configuration,
* Model lifecycle,
* business-domain commit,
* queue scheduling,
* global operation recovery.

Retry also does NOT mean:

```text
try until something works
```

All retry behavior MUST be bounded.

---

# Retry vs Attempt

An `ExecutionAttempt` represents one concrete execution try.

Conceptually:

```text
AI Request
    |
    v
Route Plan
    |
    +--> Attempt 1
    +--> Attempt 2
    +--> Attempt 3
```

Retry creates another attempt.

It MUST NOT mutate the previous attempt.

---

# Retry vs AI Request

The semantic `AIRequest` normally remains unchanged across retries.

```text
requestId
    stable

attemptId
    new for each attempt
```

Retry MUST NOT mutate Request fields such as:

```text
retryCount
providerAttempts
stageHistory
```

Those belong to runtime attempt history.

---

# Retry vs Route Plan

Normal Retry SHOULD reuse the same compatible Route Plan.

```text
RoutePlan A
    |
    +--> Attempt 1
    +--> Attempt 2
```

If execution changes to:

* another model,
* another Deployment,
* another provider,
* another region where route semantics differ,

that is normally:

```text
Fallback / Rerouting
```

not ordinary Retry.

---

# Retry Architecture

Recommended components:

```text
Normalized Failure
        |
        v
Retry Eligibility
        |
        v
Retry Budget Evaluation
        |
        v
Retry Policy Evaluation
        |
        v
Backoff Scheduling
        |
        v
Execution Attempt
```

Cross-cutting inputs include:

```text
Cancellation
Deadline
Cost Constraints
Attempt History
Route Validity
```

---

# Failure Normalization

Retry decisions MUST operate on normalized CRAI failures rather than raw provider error codes.

Conceptually:

```text
Raw Provider / Runtime Error
        |
        v
Failure Normalization
        |
        v
Normalized Failure
        |
        v
Retry Policy
```

This prevents Retry architecture from becoming provider-specific.

---

# Normalized Failure

Recommended:

```text
NormalizedFailure
├── failureCategory
├── failureCode
├── retryabilityHint
├── sourceStage
├── providerReference?
├── deploymentReference?
├── attemptId?
├── retryAfter?
├── diagnosticReference?
└── occurredAt
```

Provider-specific raw details MAY remain in diagnostics.

---

# Retryability

Retryability SHOULD be one of:

```text
RETRYABLE
NON_RETRYABLE
CONDITIONAL
UNKNOWN
```

A normalized failure category MAY provide a default classification.

Retry Policy still evaluates operation-specific constraints.

---

# Retryable Failures

Typical retryable failures MAY include:

```text
NETWORK_INTERRUPTION
PROVIDER_TEMPORARILY_UNAVAILABLE
PROVIDER_RATE_LIMITED
PROVIDER_TIMEOUT
GATEWAY_TIMEOUT
CONNECTION_TIMEOUT
TRANSIENT_INFRASTRUCTURE_FAILURE
TEMPORARY_CAPACITY_EXHAUSTED
STREAM_INTERRUPTED
```

Retry is appropriate only when repetition is likely to succeed without semantic change.

---

# Conditionally Retryable Failures

Some failures require contextual evaluation.

Examples:

```text
DEPENDENCY_UNAVAILABLE
OUTPUT_STREAM_INTERRUPTED
PROVIDER_INTERNAL_ERROR
TEMPORARY_CREDENTIAL_RESOLUTION_FAILURE
CACHE_BACKEND_FAILURE
```

Whether Retry is appropriate depends on:

* stage,
* operation idempotency,
* deadline,
* route state,
* recovery policy.

---

# Non-Retryable Failures

Typical non-retryable failures include:

```text
INVALID_REQUEST
AUTHORIZATION_DENIED
POLICY_DENIED
SAFETY_DENIED
UNSUPPORTED_CAPABILITY
INVALID_OUTPUT_CONTRACT
INVALID_MODEL_REQUIREMENTS
PERMANENT_CREDENTIAL_FAILURE
USER_CANCELLED
BUSINESS_VALIDATION_FAILURE
```

These require:

* correction,
* another workflow,
* explicit fallback/rerouting,
* user/admin action,

rather than repeating the same attempt.

---

# Authentication Boundary

A permanent credential problem such as:

```text
invalid API key
revoked credential
```

is non-retryable.

A transient credential-resolution dependency failure MAY be retryable.

Therefore Retry MUST rely on normalized semantics rather than simply:

```text
HTTP 401 = never retry
```

---

# Validation Failure

Response validation failure is NOT universally retryable.

Examples:

```text
Malformed provider response
    may be retryable

Model consistently violates required schema
    may require fallback

Business-domain conflict
    must not be retried as model execution
```

Recovery policy determines the next action.

---

# Retry Eligibility

Before retrying, all of the following SHOULD be true:

```text
failure is retryable
AND
operation is retry-safe
AND
route remains compatible
AND
retry budget remains
AND
deadline remains
AND
cost constraint permits
AND
operation is not cancelled
```

Failure of any hard condition stops Retry.

---

# Idempotency

Retry MUST preserve business idempotency.

Critical rule:

```text
Repeating an attempt
must not duplicate committed business effects.
```

AI model execution SHOULD occur before durable business commit where possible.

---

# Business Commit Boundary

Recommended:

```text
AI Attempt
    |
    v
AI Response
    |
    v
Capability Validation
    |
    v
Domain Commit
```

If an execution attempt fails before business commit, Retry is usually safer.

If partial side effects already occurred, retry semantics require explicit idempotency handling.

---

# Provider Idempotency

Where providers support request-level idempotency keys, Provider Adapter MAY use them.

Provider-native idempotency mechanisms MUST remain adapter-specific.

Canonical CRAI retry identity remains provider-neutral.

---

# Idempotency Key

Runtime MAY derive an attempt/request idempotency key from:

```text
requestId
routePlanId
logicalOperationId
```

depending on provider and execution semantics.

The exact mechanism belongs to runtime/provider adapters.

---

# Retry Policy

A Retry Policy defines when and how a compatible attempt may be repeated.

Recommended:

```text
RetryPolicy
├── retryPolicyId
├── revision
├── maximumAttempts
├── maximumElapsedTime?
├── retryableCategories[]
├── conditionalRules[]
├── backoffStrategy
├── jitterPolicy?
├── respectRetryAfter
├── maximumRetryCost?
├── deadlinePolicy
└── stageRules?
```

---

# Retry Policy Ownership

Retry Policy belongs to Retry/Runtime architecture.

Routing MUST NOT own the Retry algorithm.

Routing MAY provide Route Plan characteristics that Retry Policy consumes.

---

# Retry Policy Resolution

Effective retry behavior MAY combine:

```text
Runtime Default
+
AI Request Execution Constraints
+
Resolved Configuration
+
Workspace Policy
+
Cost Constraints
+
Capability Policy
```

into:

```text
EffectiveRetryPolicy
```

The exact resolved policy SHOULD be immutable for one recovery sequence where reproducibility matters.

---

# Request Constraints

AI Request MAY express high-level constraints such as:

```text
maximumAttempts
deadline
fallbackAllowed
maximumEstimatedCost
```

It SHOULD NOT encode provider-specific backoff algorithms.

---

# Maximum Attempts

`maximumAttempts` SHOULD normally mean:

```text
total attempts including initial attempt
```

or the implementation MUST explicitly document another definition.

Ambiguous interpretation such as:

```text
3 retries
```

SHOULD be avoided.

---

# Retry Budget

Retry is constrained by multiple budgets.

Recommended:

```text
RetryBudget
├── remainingAttempts
├── remainingElapsedTime
├── remainingCost
├── remainingInputUnits?
├── remainingOutputUnits?
└── remainingOperationDeadline
```

---

# Budget Ownership

Retry computes whether another attempt fits within the provided limits.

It does NOT own:

* Workspace daily budget,
* monthly budget,
* Usage Ledger,
* Subscription entitlement.

Those belong to Cost Control/Usage architecture.

---

# Retry Exhaustion

When retry budget is exhausted:

```text
STOP_RETRY
```

This does NOT automatically mean:

```text
business operation permanently failed
```

The orchestrator MAY:

* invoke Fallback,
* reroute,
* return structured execution failure,
* request user action.

---

# Backoff Strategy

Supported strategies MAY include:

```text
NONE
FIXED
EXPONENTIAL
EXPONENTIAL_WITH_JITTER
SERVER_DIRECTED
ADAPTIVE
```

MVP SHOULD prefer:

```text
EXPONENTIAL_WITH_JITTER
```

for ordinary transient provider failures.

---

# Fixed Delay

Example:

```text
Attempt 1
    |
    v
delay 1s
    |
    v
Attempt 2
```

Useful for simple deterministic dependencies.

---

# Exponential Backoff

Conceptually:

```text
delay(n)
    =
baseDelay * multiplier^(n-1)
```

with an upper bound.

---

# Jitter

Jitter SHOULD be supported to reduce synchronized retry storms.

Common strategies MAY include:

* full jitter,
* equal jitter,
* decorrelated jitter.

Exact algorithm is runtime implementation detail.

---

# Server-Directed Retry

When a provider supplies an explicit retry delay such as:

```text
Retry-After
```

Retry Policy MAY respect it if:

* within deadline,
* within budget,
* Policy permits.

Provider Adapter normalizes provider-specific headers into a generic retry hint.

---

# Adaptive Retry

Adaptive Retry MAY consider:

* current provider health,
* rate-limit pressure,
* recent failure rate,
* queue pressure.

Advanced adaptive policies SHOULD be deferred until measurements justify them.

---

# Backoff Does Not Block Worker

Runtime SHOULD avoid holding scarce execution resources during long backoff periods.

Implementation MAY use:

* scheduler,
* delayed queue,
* timer service.

This is runtime infrastructure.

---

# Deadline

Retry MUST respect the overall operation deadline.

Example:

```text
Operation deadline:
    5 seconds

Remaining:
    600 ms

Next backoff:
    2 seconds
```

Result:

```text
STOP_RETRY
```

---

# Attempt Timeout vs Operation Deadline

These remain separate:

```text
Attempt Timeout
    = max duration of one attempt

Operation Deadline
    = max duration of entire operation/recovery sequence
```

Retry MUST account for both.

---

# Cancellation

Cancellation overrides Retry.

If operation is cancelled:

```text
NO NEW ATTEMPT
```

Backoff timers SHOULD be cancellable where possible.

---

# Route Validity

Before retrying, runtime SHOULD confirm the current Route Plan remains usable.

Possible checks:

* Route Plan not expired,
* Deployment not explicitly unavailable,
* Policy revalidation if required,
* credential configuration still usable.

---

# Route Becomes Invalid

If the current Route Plan is no longer valid:

```text
Retry stops
```

and orchestration MAY request:

```text
Fallback / Rerouting
```

Retry MUST NOT silently mutate Route Plan.

---

# Same Route

Normal Retry preserves:

```text
modelId
deploymentId
providerConfigurationId
executionMode
```

unless Retry Policy explicitly invokes a route revalidation step whose result creates a new Route Plan.

Once Route Plan changes, provenance MUST show rerouting.

---

# Model Parameters

Retry SHOULD normally preserve semantic model parameters.

Changing parameters that alter requested semantics is not ordinary Retry.

A purely transport/runtime parameter MAY change if it does not affect semantic intent.

---

# Streaming Retry

Streaming requires special care.

If failure occurs before any caller-visible output:

```text
normal retry
```

may be safe.

If provisional output was already streamed:

```text
restart / continuation policy
```

must explicitly define:

* whether previous chunks are discarded,
* whether new stream restarts from sequence 0,
* whether caller receives replacement notification.

---

# Partial Output

Partial AI output MUST NOT automatically be treated as committed business state.

This makes retry safer.

If partial content was displayed provisionally, Presentation/UI must distinguish it from committed output.

---

# Stream Resume

Provider-native stream resume MAY be supported in future.

It MUST NOT be assumed portable across providers.

MVP SHOULD prefer clean restart unless a provider-neutral resume contract is proven.

---

# Retry and Cache

Before repeating expensive model execution, orchestration MAY re-evaluate cache when semantically valid.

Retry itself does not own AI Result Cache.

A cache hit may terminate recovery successfully.

---

# Retry and Safety

Retry MUST NOT bypass Safety.

If Safety denied the operation:

```text
NO RETRY
```

If Safety requirements changed materially, execution must be revalidated before another attempt.

---

# Retry and Policy

Policy denial is non-retryable.

A policy change that later makes execution possible is a new authorization/routing situation, not simply blind retry.

---

# Retry and Cost

Every retry may increase cost.

Retry SHOULD evaluate:

```text
estimated next-attempt cost
```

against the remaining retry/operation budget.

Cost constraints MAY stop retry before maximum attempt count is reached.

---

# Retry and Rate Limits

Rate-limit failures SHOULD respect:

* provider retry hints,
* rate-limit pressure,
* deadline,
* fallback availability.

A long provider wait MAY make fallback preferable to retry.

That choice belongs to recovery orchestration.

---

# Retry and Provider Health

A retryable failure does not always imply retry is the best recovery.

Example:

```text
Deployment health:
    UNAVAILABLE
```

Then:

```text
STOP_RETRY_CURRENT_ROUTE
```

may be preferable.

Fallback may select another route.

---

# Retry vs Fallback

Critical distinction:

```text
Retry
    = repeat compatible execution on the current route
```

```text
Fallback
    = use an alternative compatible route or explicitly degraded mode
```

These MUST remain separate.

---

# Retry vs Rerouting

```text
Retry
    preserves Route Plan
```

```text
Rerouting
    creates a new Route Plan
```

Retry MAY trigger a request for rerouting after it stops.

It MUST NOT perform hidden rerouting itself.

---

# Retry vs Repair

```text
Repair
    = attempt to fix invalid response representation/result
```

```text
Retry
    = repeat execution
```

A validation failure MAY choose Repair before Retry.

They are independent recovery mechanisms.

---

# Failure Classifier

Failure classification SHOULD be provider-neutral.

It MAY consume:

* normalized provider errors,
* runtime errors,
* stage failures,
* timeout reason,
* cancellation state.

It outputs:

```text
NormalizedFailure
```

---

# Failure Classifier Ownership

Provider Adapter owns raw provider-error translation.

Retry architecture consumes normalized categories.

This prevents Retry Policy from containing:

```text
if HTTP status == ...
if provider error == ...
```

throughout generic code.

---

# Attempt Record

Recommended runtime structure:

```text
AIExecutionAttempt
├── attemptId
├── requestId
├── routePlanId
├── attemptNumber
├── startedAt
├── endedAt?
├── status
├── normalizedFailure?
├── usageReference?
├── providerRequestReference?
├── providerResponseReference?
└── diagnosticReference?
```

This belongs to runtime execution architecture.

---

# Retry Sequence

Runtime MAY expose:

```text
RetrySequence
├── retrySequenceId
├── requestId
├── initialRoutePlanId
├── effectiveRetryPolicyRevision
├── attemptReferences[]
├── finalRetryDecision
├── startedAt
└── endedAt?
```

Whether this is persisted is an implementation decision.

---

# Retry Decision

Recommended:

```text
RetryDecision
├── decision
├── failureCategory
├── reasonCode
├── nextDelay?
├── remainingAttempts
├── remainingDeadline?
├── remainingCostBudget?
└── evaluatedAt
```

Possible decisions:

```text
RETRY
STOP_RETRY
STOP_CANCELLED
STOP_ROUTE_INVALID
STOP_BUDGET_EXHAUSTED
STOP_DEADLINE_EXCEEDED
STOP_NON_RETRYABLE
```

---

# Retry Decision Is Explainable

Every retry should be explainable without inspecting raw provider payloads.

Example:

```text
decision:
    RETRY

reason:
    PROVIDER_RATE_LIMITED

delay:
    2.4s
```

---

# Observability

Retry observability SHOULD include:

* attempts per Request,
* retry count,
* retry latency,
* backoff duration,
* failure category,
* retry decision,
* retry success rate,
* cost attributable to retries,
* final recovery outcome,
* route invalidation during retry.

---

# Retry Count Semantics

Observability SHOULD clearly distinguish:

```text
attemptCount
```

from:

```text
retryCount
```

Example:

```text
attemptCount = 3
retryCount = 2
```

---

# Sensitive Observability

Retry logs MUST NOT require:

* raw Prompt,
* raw source content,
* raw provider response,
* credentials.

Prefer IDs and normalized failure codes.

---

# Failure Conditions

Retry-specific failures MAY include:

```text
RETRY_NON_RETRYABLE_FAILURE
RETRY_ATTEMPT_LIMIT_EXCEEDED
RETRY_DEADLINE_EXCEEDED
RETRY_COST_BUDGET_EXCEEDED
RETRY_CANCELLED
RETRY_ROUTE_INVALID
RETRY_BACKOFF_EXCEEDS_DEADLINE
RETRY_POLICY_INVALID
RETRY_IDEMPOTENCY_UNSAFE
RETRY_PROVIDER_HINT_INVALID
```

These describe why Retry stopped.

The original execution failure remains separately traceable.

---

# Recovery After Retry Stops

After Retry stops, orchestration MAY:

```text
invoke Fallback
request Rerouting
use compatible Cache
return AIExecutionFailure
request user action
```

Retry MUST NOT decide business workflow completion.

---

# Architecture Invariants

1. Retry repeats compatible execution attempts.

2. Retry normally preserves the same immutable AI Request.

3. Retry normally preserves the same Route Plan.

4. Changing Model/Deployment/Provider route is Fallback or Rerouting, not ordinary Retry.

5. Retry and Fallback are separate responsibilities.

6. Retry and Repair are separate responsibilities.

7. Retry decisions operate on normalized failures.

8. Provider-specific error mapping belongs to Provider Adapter.

9. Retry MUST NOT contain provider-specific API error logic in generic policy.

10. Only retryable or conditionally retryable failures may produce automatic retry.

11. Policy denial MUST NOT be retried automatically.

12. Safety denial MUST NOT be retried automatically.

13. User cancellation MUST NOT be retried.

14. Permanent credential failure MUST NOT be retried.

15. Invalid business requests MUST NOT be retried.

16. Retry MUST be bounded.

17. Retry limits include more than attempt count.

18. Retry MUST respect overall deadline.

19. Retry MUST respect cost constraints.

20. Retry MUST respect cancellation.

21. Retry MUST respect route validity.

22. Retry MUST preserve idempotency.

23. New retries create new Attempt IDs.

24. Retry MUST NOT mutate previous Attempt records.

25. Retry Count belongs to runtime/observability, not AI Request.

26. Provider Attempts belong to runtime/observability, not AI Request.

27. Retry Policy is separate from Routing Policy.

28. Routing MUST NOT own backoff algorithms.

29. Retry Policy MAY consume Route Plan characteristics.

30. Backoff SHOULD avoid synchronized request storms.

31. Provider retry hints MAY influence backoff after normalization.

32. Provider retry hints MUST NOT override hard operation deadlines.

33. Retry exhaustion means stop retrying, not necessarily fail the whole business workflow.

34. Orchestration MAY invoke Fallback after Retry stops.

35. Retry MUST NOT secretly reroute.

36. Rerouting creates a new Route Plan.

37. Route invalidation SHOULD stop retrying the current route.

38. Retry MUST NOT weaken hard capability requirements.

39. Retry MUST NOT change semantic model parameters merely to obtain success.

40. Streaming partial output remains provisional.

41. Streaming Retry MUST define restart semantics.

42. Partial streamed output MUST NOT automatically become committed domain truth.

43. Optional Cache reuse during recovery remains separate from Retry ownership.

44. Retry cost MUST be observable.

45. Retry decisions SHOULD be explainable.

46. Retry observability MUST distinguish attempts from retries.

47. Raw sensitive execution payloads MUST NOT be required for retry telemetry.

48. Retry does not own Workspace Usage Ledger or daily/monthly budgets.

49. Retry does not own Model Health.

50. Retry does not own Provider Health.

51. Retry does not own business-domain commit.

52. A retryable failure MAY still choose not to retry because deadline, cost, cancellation or health makes retry inappropriate.

53. Same semantic Request may have different effective Retry decisions under different explicit runtime constraints.

54. Retry Policy revisions SHOULD be traceable when materially affecting durable AI execution.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* normalized failure categories,
* retryability classification,
* same-Request retries,
* same-RoutePlan retries,
* unique Attempt IDs,
* maximum total attempts,
* overall operation deadline,
* per-attempt timeout,
* exponential backoff with jitter,
* provider Retry-After normalization,
* cancellation-aware backoff,
* retry cost budget,
* route-validity check,
* network failure retry,
* provider timeout retry,
* rate-limit retry,
* transient provider failure retry,
* non-retryable invalid Request,
* non-retryable Policy/Safety denial,
* Retry Decision records,
* retry observability,
* transition to Fallback after retry exhaustion.

MVP MAY defer:

* adaptive retry algorithms,
* circuit-breaker integration,
* resumable streaming,
* provider-specific advanced idempotency,
* dynamic retry-policy learning,
* queue-pressure-aware retry,
* automated optimal retry-vs-fallback prediction,
* complex dependency-specific retry trees,
* distributed retry coordination across multiple workers.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact normalized failure taxonomy,
* exact RetryPolicy schema,
* whether `maximumAttempts` includes initial attempt,
* default maximum attempts,
* default exponential-backoff parameters,
* jitter algorithm,
* maximum backoff,
* whether Provider Retry-After always takes precedence,
* exact operation-deadline representation,
* retry cost estimation,
* token/unit retry budget,
* route-validity recheck frequency,
* behavior when Health becomes `UNKNOWN`,
* Retry vs immediate Fallback thresholds,
* rate-limit-specific recovery strategy,
* streaming restart semantics,
* whether partial structured output may be resumed,
* provider idempotency-key mapping,
* RetrySequence persistence,
* RetryDecision retention,
* failure classifier ownership between AI/runtime/provider-management,
* whether Response Validation failure first invokes Repair or Retry,
* whether Context materialization failures use this Retry architecture,
* integration with future Circuit Breaker,
* delayed scheduler/backoff implementation,
* retry telemetry retention.

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
* `FALLBACK.md`
* `COST_CONTROL.md`
* `CACHE.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

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
* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/telemetry/`
