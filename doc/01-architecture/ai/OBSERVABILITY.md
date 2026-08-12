# AI Observability

* **Document:** AI Architecture / Observability
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the observability architecture for CRAI AI execution.

AI Observability provides operational visibility into:

* AI Requests,
* pipeline stages,
* Routing,
* Model execution,
* Streaming,
* Retry,
* Fallback,
* Cache,
* Safety,
* Cost-related execution signals,
* failures and degradation.

Observability enables:

* troubleshooting,
* performance analysis,
* reliability monitoring,
* capacity planning,
* route optimization,
* operational diagnostics.

Observability MUST remain:

* provider-neutral,
* structured,
* privacy-aware,
* low-overhead,
* correlation-friendly,
* failure-tolerant,
* replaceable.

---

# Core Principle

```text
AI Execution
    |
    v
Telemetry Emission
    |
    +--> Logs
    +--> Metrics
    +--> Traces
    +--> Runtime Events
    |
    v
Telemetry Processing
    |
    v
Derived Projections
    |
    +--> Dashboards
    +--> Alerts
    +--> Health
    +--> Diagnostics
```

Audit and Usage/Cost accounting are related but separate concerns.

---

# Scope

AI Observability covers:

* structured logging,
* metrics,
* distributed tracing,
* execution correlation,
* stage observability,
* route decision telemetry,
* retry/fallback telemetry,
* streaming telemetry,
* cache telemetry,
* safety telemetry,
* cost/usage projections,
* runtime health derivation,
* alerts,
* diagnostics.

---

# Non-Goals

AI Observability does NOT own:

* AI Request semantics,
* AI Response semantics,
* Workspace Policy,
* Safety Policy,
* Usage Ledger,
* Budget state,
* Audit business semantics,
* Provider Configuration,
* Model Catalog,
* Retry Policy,
* Fallback Policy,
* Routing Policy,
* domain business truth.

---

# Observability vs Telemetry

`Telemetry` is emitted operational evidence.

Examples:

```text
request started
stage duration
provider timeout
cache hit
retry attempted
stream cancelled
```

`Observability` is the architecture that allows operators and systems to understand behavior from telemetry.

---

# Observability Signals

Primary signals:

```text
Logs
Metrics
Traces
Runtime Events
```

Derived signals MAY include:

```text
Health
Alerts
Diagnostics
Performance Projections
```

---

# Observability vs Audit

Critical distinction:

```text
Observability
    = how the system behaved
```

```text
Audit
    = what materially happened,
      who/what caused it,
      and why it matters for accountability
```

Not every observable event belongs in Audit.

---

# Observability vs Usage

```text
Telemetry:
    model output units observed
```

may feed:

```text
Usage Ledger:
    authoritative usage record
```

But telemetry itself is not the authoritative Usage Ledger.

---

# Observability vs Cost

Estimated/actual cost MAY be exported as metrics.

The authoritative cost record remains owned by Cost/Usage architecture.

---

# Observability vs Health

Health is a derived operational projection.

Example:

```text
latency
error rate
availability
rate-limit pressure
```

may produce:

```text
DeploymentHealth
```

Routing may consume that projection.

Observability supplies evidence.

Health projection logic owns the derived state.

---

# Architecture

Recommended:

```text
AI Pipeline
    |
    +--> Structured Logs
    +--> Metrics
    +--> Trace Spans
    +--> Runtime Events
    |
    v
Telemetry Collection
    |
    v
Processing / Aggregation
    |
    +--> Diagnostics
    +--> Dashboards
    +--> Alerts
    +--> Health Projections
    +--> Performance Projections
```

---

# AI Operation Correlation

Every AI operation SHOULD have stable correlation identifiers.

Recommended:

```text
requestId
correlationId
traceId
businessOperationId?
sessionId?
projectId?
workspaceId
```

Not every telemetry record requires every identifier.

---

# Request ID

`requestId` identifies one immutable AI Request.

It SHOULD remain stable across:

* Retry attempts,
* Fallback routes,
* Streaming attempts.

---

# Attempt ID

Each model execution attempt SHOULD have:

```text
attemptId
```

This distinguishes:

```text
one AI Request
    |
    +--> Attempt 1
    +--> Attempt 2
    +--> Attempt 3
```

---

# Route Plan ID

Routing telemetry SHOULD preserve:

```text
routePlanId
```

Fallback creates another RoutePlan and therefore another RoutePlan ID.

---

# Stream ID

Streaming telemetry SHOULD use:

```text
streamId
```

separately from:

* Request ID,
* Attempt ID,
* Response ID.

---

# Response ID

A finalized AIResponse SHOULD have:

```text
responseId
```

A failed operation may have Attempt/Stream telemetry without any Response ID.

---

# Trace ID

`traceId` links execution spans.

A Trace MAY include:

```text
AI Request Validation
Context Assembly
Prompt/Input Construction
Safety Evaluation
Routing
Cache Evaluation
Provider Request Adaptation
Model Execution
Streaming
Response Adaptation
Response Validation
Finalization
```

Only stages actually executed should appear.

---

# AI Trace Boundary

AI Observability MUST NOT assume the full CRAI workflow is:

```text
Capture
OCR
...
Rendering
```

Those capabilities may belong to a broader system trace.

The AI trace represents one AI operation.

---

# Parent Trace

An AI operation MAY be a child of a broader business/runtime trace.

Example:

```text
Reading Translation Flow
    |
    +--> Capture span
    +--> Recognition span
    +--> Translation AI operation trace
    +--> Presentation span
```

This preserves cross-module observability without making AI own those modules.

---

# Structured Logging

AI logs SHOULD be structured.

Recommended common fields:

```text
timestamp
severity
component
stageId?
workspaceId?
projectId?
requestId?
attemptId?
routePlanId?
streamId?
responseId?
traceId?
correlationId?
failureCode?
duration?
```

---

# Log Content

Logs SHOULD describe:

* state transition,
* failure category,
* decision reason,
* resource reference,
* duration,
* normalized metadata.

Logs SHOULD NOT require raw semantic content.

---

# Sensitive Logging

The following MUST NOT be logged by default:

```text
raw source text
full Prompt
AI Context Package content
Glossary contents
Character context
Memory contents
raw AIResponse content
raw provider response
credentials
API tokens
private keys
```

---

# Logging by Reference

Prefer:

```text
textBlockId
contextHash
promptHash
responseId
contentHash
diagnosticReference
```

instead of raw payloads.

---

# Redaction

If operational logging may contain sensitive values, redaction/masking MUST occur before telemetry leaves the emitting boundary where practical.

---

# Raw Diagnostic Mode

Temporary raw payload diagnostics MAY exist only under explicit controlled policy.

Requirements SHOULD include:

* restricted access,
* limited duration,
* Workspace authorization,
* strong retention limits,
* audit of enablement where appropriate.

---

# Metrics

Metrics SHOULD represent aggregated numeric operational behavior.

Typical AI metrics include:

```text
request_count
request_success_count
request_failure_count

stage_duration
route_selection_latency
provider_latency

attempt_count
retry_count
fallback_count

cache_hit
cache_miss

stream_count
stream_completion
stream_cancellation
stream_time_to_first_output

safety_allow
safety_deny

estimated_cost_projection
actual_cost_projection

input_usage_projection
output_usage_projection
```

---

# Metrics Are Projections

Metrics derived from Usage or Cost MUST NOT replace authoritative accounting.

Metrics may be:

* sampled,
* aggregated,
* delayed,
* dropped.

Therefore they are unsuitable as authoritative budget state.

---

# Labels / Dimensions

Metrics MAY be aggregated by:

```text
capability
provider
model
deployment
executionMode
workspace
project
failureCategory
cacheClass
routingPolicy
```

High-cardinality labels MUST be used carefully.

---

# High Cardinality

The following SHOULD NOT automatically become metric labels:

```text
requestId
traceId
streamId
userId
raw Project IDs at massive scale
```

They belong better in logs/traces.

---

# Provider Dimension

Provider/model dimensions are useful operationally.

Business capabilities MUST NOT depend on those observability dimensions.

---

# Distributed Tracing

Tracing SHOULD describe execution causality and latency.

Recommended logical structure:

```text
AI Operation
    |
    +--> Request Validation
    +--> Context Assembly
    +--> Prompt/Input Construction
    +--> Safety Evaluation
    +--> Routing
    +--> Cache
    +--> Execution Attempt
    |       |
    |       +--> Provider Adaptation
    |       +--> Model Execution
    |       +--> Streaming?
    |
    +--> Response Validation
    +--> Finalization
```

---

# Retry Trace

Retry MAY produce sibling Attempt spans.

Example:

```text
AI Operation
    |
    +--> Attempt 1 [timeout]
    |
    +--> backoff
    |
    +--> Attempt 2 [success]
```

Retry does not require a new root trace.

---

# Fallback Trace

Fallback SHOULD show RoutePlan transition.

Example:

```text
RoutePlan A
    |
    +--> Attempt 1 [unavailable]
    |
    v
Fallback Decision
    |
    v
RoutePlan B
    |
    +--> Attempt 1 [success]
```

---

# Streaming Trace

Streaming tracing SHOULD capture meaningful milestones rather than every token.

Recommended:

```text
stream-open
first-semantic-chunk
stream-finalization
stream-complete
```

Per-token spans SHOULD be avoided.

---

# Context Trace

Context observability MAY expose:

```text
candidateCount
selectedCount
droppedCount
contextSize
contextHash
reductionCount
buildDuration
```

It SHOULD NOT expose raw Context content.

---

# Prompt Trace

Prompt observability MAY expose:

```text
templateId
templateVersion
compilerVersion
promptHash
inputSize
estimatedUnits
buildDuration
```

It MUST NOT log full Prompt by default.

---

# Routing Trace

Routing SHOULD expose bounded diagnostic information such as:

```text
candidateCount
filteredCandidateCount
selectedRoute
routingPolicyRevision
decisionHash
rejectionReasonCounts
```

Detailed candidate traces may use diagnostics storage rather than metrics.

---

# Retry Observability

Retry telemetry SHOULD include:

```text
attemptNumber
retryDecision
failureCategory
backoffDuration
remainingDeadline
remainingCostBudget?
```

---

# Fallback Observability

Fallback telemetry SHOULD include:

```text
fallbackTrigger
previousRoutePlanId
newRoutePlanId
degradations[]
excludedRouteCount
```

---

# Cache Observability

Cache telemetry SHOULD distinguish:

```text
physicalHit
semanticAcceptedHit
semanticRejectedHit
miss
write
writeFailure
corruption
```

A physical entry found but rejected for incompatibility is NOT a semantic hit.

---

# Safety Observability

Safety telemetry MAY include:

```text
decision
findingCategory
severity
requiredControls
evaluatorVersion
policyRevision
evaluationDuration
```

Raw sensitive input SHOULD remain excluded.

---

# Cost Observability

Cost-related telemetry MAY include:

```text
preRouteEstimate
refinedEstimate
actualCostProjection
estimateError
reservationOutcome
retryCost
fallbackCost
avoidedCostEstimate
```

Authoritative accounting remains outside Observability.

---

# Health Monitoring

Health projections MAY derive from telemetry.

Possible subjects:

```text
Provider
Model Deployment
Cache
Storage
Queue
Scheduler
Local Runtime
```

---

# Deployment Health

For AI Routing, Deployment health is especially important.

Possible signals:

```text
availability
success rate
error rate
latency
rate-limit pressure
capacity
```

Health SHOULD include observation freshness.

---

# Health Status

Possible:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
MAINTENANCE
UNKNOWN
```

The precise Health model SHOULD remain aligned with `MODELS.md` and `ROUTING.md`.

---

# Health Ownership

Observability collects evidence.

A Health Projection component computes current health state.

Routing consumes the Health Projection.

Routing MUST NOT infer health independently from arbitrary logs.

---

# Health Feedback Loop

Conceptually:

```text
Execution
    |
    v
Telemetry
    |
    v
Health Projection
    |
    v
Routing
```

This is allowed.

It does NOT mean telemetry instrumentation itself changes execution semantics.

---

# Feedback Stability

Routing/health feedback SHOULD avoid unstable oscillation.

Example:

```text
one transient timeout
    ->
immediate global model ban
```

SHOULD normally be avoided.

Health windows/hysteresis MAY be required.

---

# Audit

Audit is a separate accountability channel.

Audit records SHOULD capture material actions such as:

```text
Workspace Policy changed
Provider permission changed
Budget manually overridden
Safety restriction overridden
User correction committed to canonical state
Configuration changed
Sensitive diagnostic mode enabled
Cross-Workspace sharing changed
```

---

# What Is Usually Not Audit

Ordinary runtime events such as:

```text
retry attempt
cache hit
normal route selection
stream chunk
normal provider request
```

SHOULD normally remain telemetry.

They MAY become audit-relevant only under a specific governance requirement.

---

# Retry/Fallback Audit

Retry/Fallback decisions MAY be auditable when they cause a material controlled change such as:

```text
paid fallback invoked
local-only policy override attempted
quality degradation requiring consent
provider restriction overridden
```

Otherwise they remain telemetry.

---

# Audit Immutability

Audit records SHOULD be append-only.

Correction SHOULD create another audit record rather than silently rewriting history.

---

# Audit Content

Audit SHOULD prefer:

```text
actor/reference
action
resourceReference
reasonCode
policyRevision
correlationId
timestamp
```

over raw content.

---

# Audit Storage Boundary

Observability does NOT own Audit persistence.

Audit events MAY flow to dedicated audit infrastructure.

---

# Alerts

Alerts are derived operational notifications.

Possible conditions:

```text
provider unavailable
deployment error spike
routing no-route spike
retry spike
fallback spike
stream failure spike
cache corruption
cost anomaly
budget threshold
safety deny spike
telemetry pipeline degradation
```

---

# Alert Actionability

Alerts SHOULD include:

* affected component,
* severity,
* relevant time window,
* diagnostic links/references,
* recommended investigation context.

They SHOULD avoid dumping raw user content.

---

# Alert Deduplication

Repeated equivalent alerts SHOULD be grouped/deduplicated where practical.

---

# Diagnostics

Diagnostics provides bounded structured information for troubleshooting.

It MAY include:

```text
route decision trace
retry sequence
fallback sequence
context-selection trace
prompt metadata
stream execution record
normalized failure chain
```

---

# Diagnostics vs Logs

Logs are event records.

Diagnostics MAY assemble correlated evidence into a higher-level troubleshooting view.

---

# Diagnostics vs Audit

Diagnostics may be deleted/rotated as operational data.

Audit retention follows accountability requirements.

---

# Observability Lifecycle

Recommended:

```text
Emit
  |
  v
Collect
  |
  v
Normalize
  |
  v
Process
  |
  +--> Store
  +--> Aggregate
  |
  v
Analyze
  |
  +--> Dashboard
  +--> Alert
  +--> Health Projection
  +--> Diagnostics
```

---

# Telemetry Normalization

Provider-specific telemetry SHOULD be normalized before generic observability consumers depend on it.

Examples:

```text
provider latency
provider token usage
provider finish metadata
provider error category
```

---

# OpenTelemetry-Like Semantics

CRAI MAY adopt standard telemetry conventions where useful.

The architecture SHOULD NOT depend on one telemetry vendor.

---

# Observability Backends

Possible implementations may include:

* OpenTelemetry,
* Prometheus-compatible metrics,
* structured logs,
* distributed trace backends,
* local diagnostics storage.

Backend choice belongs to infrastructure.

---

# Observability Failure

Observability failures SHOULD normally NOT fail AI execution.

Possible failures:

```text
TELEMETRY_EXPORT_FAILED
LOG_EXPORT_FAILED
METRIC_EXPORT_FAILED
TRACE_EXPORT_FAILED
DIAGNOSTIC_STORAGE_FAILED
```

---

# Degraded Observability

If a backend is unavailable:

```text
continue execution
```

where safe.

Runtime MAY:

* buffer,
* sample,
* drop non-critical telemetry,
* mark telemetry degraded.

---

# Exceptions to Non-Blocking Observability

Certain accountability controls MAY be required by Policy.

Example:

```text
operation requires durable audit
```

and Audit storage is unavailable.

In that case Policy MAY fail closed.

This is an Audit/Policy requirement, not ordinary telemetry behavior.

---

# Buffering

Local buffering MAY protect against transient telemetry backend outages.

Buffers MUST be bounded.

---

# Dropping Telemetry

When overloaded, the system SHOULD prefer dropping:

```text
high-volume debug telemetry
```

before:

```text
critical error/security/accountability events
```

subject to policy.

---

# Sampling

Tracing/log sampling MAY reduce overhead.

Sampling rules MUST preserve sufficient visibility into:

* failures,
* Safety denials,
* severe latency,
* recovery loops,
* anomalous cost.

---

# Performance Overhead

Observability instrumentation SHOULD have bounded execution overhead.

Instrumentation MUST NOT materially distort latency-sensitive AI operations.

---

# Semantic Non-Interference

Critical principle:

```text
Instrumentation
    MUST NOT
change semantic AI output
```

Examples of forbidden coupling:

```text
logging enabled
    -> different Prompt

tracing disabled
    -> different Context

metrics backend unavailable
    -> different Translation semantics
```

---

# Derived Operational Feedback

Derived telemetry projections MAY influence execution.

Examples:

```text
Health Projection
    -> Routing

Cost Projection
    -> Routing/Cost Control

Capacity Projection
    -> Scheduler
```

This does NOT violate semantic non-interference because the feedback is an explicit architecture input, not an instrumentation side effect.

---

# Correlation Boundaries

Correlation identifiers SHOULD remain separate by meaning:

```text
requestId
    semantic AI request

attemptId
    execution attempt

routePlanId
    route decision

streamId
    streaming execution

responseId
    finalized AI response

traceId
    telemetry trace

correlationId
    broader workflow linkage
```

They MUST NOT be conflated.

---

# Session Correlation

`sessionId` MAY aid debugging.

It MUST NOT be required for every AI operation.

---

# User Correlation

User/principal identifiers SHOULD only appear where needed and permitted.

Prefer pseudonymous/stable references over personal information.

---

# Workspace Correlation

Workspace scope SHOULD normally be available for tenant-safe operational analysis.

Cross-Workspace diagnostics MUST preserve access control.

---

# Retention

Telemetry types SHOULD have distinct retention.

Example:

```text
high-volume debug logs
    short

metrics
    medium

traces
    short/medium

diagnostic records
    controlled

audit
    independent policy
```

---

# Raw Payload Retention

Raw AI payload retention SHOULD be disabled by default.

---

# Deletion

Telemetry containing user/Workspace-sensitive information SHOULD participate in applicable deletion/privacy workflows.

Aggregated anonymous metrics may have different retention rules.

---

# Access Control

Operational telemetry MUST respect role-based access.

Operators SHOULD NOT automatically have unrestricted access to raw Workspace content.

---

# Observability Security

Observability backends may themselves become sensitive systems.

Protect:

* telemetry credentials,
* trace exports,
* diagnostic payloads,
* dashboards,
* cross-tenant queries.

---

# Observability Events

Possible normalized events:

```text
AIOperationStarted
AIStageStarted
AIStageCompleted
AIStageFailed

AIRouteSelected

AIAttemptStarted
AIAttemptCompleted
AIAttemptFailed

AIRetryScheduled
AIFallbackSelected

AIStreamStarted
AIStreamCompleted
AIStreamFailed
AIStreamCancelled

AICacheHit
AICacheMiss

AISafetyEvaluated

AIOperationCompleted
AIOperationFailed
AIOperationCancelled
```

High-volume events MAY remain internal telemetry rather than durable Event Bus events.

---

# Event Bus Boundary

Observability events MUST NOT automatically become business/domain events.

Example:

```text
AIAttemptFailed
    = runtime telemetry
```

not:

```text
domain business fact
```

unless another owning module explicitly publishes such a business event.

---

# Event Volume

Per-token/per-chunk events SHOULD generally remain in stream telemetry rather than global Event Bus.

---

# Failure Taxonomy

Observability SHOULD use normalized failure categories from the owning AI components.

It SHOULD NOT invent a competing error taxonomy.

Examples:

```text
ROUTING_NO_COMPATIBLE_ROUTE
PROVIDER_TIMEOUT
RETRY_LIMIT_EXCEEDED
FALLBACK_NO_COMPATIBLE_ROUTE
STREAM_IDLE_TIMEOUT
SAFETY_POLICY_DENIED
CACHE_ENTRY_CORRUPTED
COST_BUDGET_EXCEEDED
```

---

# Failure Chains

Diagnostics MAY retain causal chains.

Example:

```text
Attempt 1:
    PROVIDER_TIMEOUT

Retry:
    scheduled

Attempt 2:
    PROVIDER_TIMEOUT

Fallback:
    RoutePlan B

Attempt 3:
    success
```

This improves debugging without redefining failure ownership.

---

# Architecture Invariants

1. AI Observability is provider-neutral.

2. Observability provides operational visibility; it does not own business truth.

3. Logs, Metrics, Traces and Runtime Events are primary telemetry signals.

4. Health is a derived operational projection, not raw telemetry itself.

5. Audit is distinct from ordinary observability telemetry.

6. Usage Ledger is distinct from telemetry.

7. Cost accounting is distinct from metrics.

8. AI Observability MUST NOT assume the complete CRAI business pipeline is the AI pipeline.

9. AI traces represent AI operations and MAY nest inside broader workflow traces.

10. Every AI Request SHOULD have stable correlation identity.

11. Attempts MUST have identities distinct from Request IDs.

12. RoutePlan IDs MUST remain distinct from Attempt IDs.

13. Stream IDs MUST remain distinct from Response IDs.

14. Finalized Response ID is separate from Trace ID.

15. Structured logging SHOULD be used.

16. Sensitive AI content MUST NOT be logged by default.

17. Full Prompt MUST NOT be logged by default.

18. Full Context MUST NOT be logged by default.

19. Raw provider Response MUST NOT be logged by default.

20. Secrets MUST NEVER enter ordinary telemetry.

21. Metrics SHOULD avoid uncontrolled high-cardinality labels.

22. Request/Trace IDs SHOULD normally remain in logs/traces rather than metric labels.

23. Cost/Usage metrics are projections, not authoritative accounting.

24. Routing telemetry MUST NOT mutate Routing decisions.

25. Retry telemetry MUST distinguish attempts from retries.

26. Fallback telemetry SHOULD preserve route transitions.

27. Cache telemetry SHOULD distinguish physical hit from semantic accepted hit.

28. Streaming telemetry SHOULD avoid per-token tracing by default.

29. Safety telemetry MUST avoid sensitive payloads by default.

30. Health projections SHOULD include freshness semantics.

31. Routing MAY consume Health projections.

32. Telemetry instrumentation itself MUST NOT change semantic AI behavior.

33. Derived operational projections MAY explicitly influence later execution decisions.

34. Audit events SHOULD be append-only where accountability requires it.

35. Not every Retry/Fallback event belongs to Audit.

36. Material Policy/Safety/configuration changes MAY require Audit.

37. Audit storage remains outside ordinary Observability ownership.

38. Observability failures SHOULD normally not block AI execution.

39. Policy-required Audit failure MAY fail closed independently.

40. Telemetry buffering MUST be bounded.

41. Non-critical telemetry MAY be dropped under pressure.

42. Error/security/accountability telemetry SHOULD receive higher retention priority.

43. Sampling MUST NOT systematically hide important failures.

44. Observability instrumentation SHOULD have bounded performance overhead.

45. Provider-specific telemetry SHOULD be normalized before generic consumption.

46. Observability backend choice MUST remain replaceable.

47. Telemetry retention SHOULD be signal-specific.

48. Raw payload retention SHOULD be disabled by default.

49. Telemetry access MUST preserve Workspace isolation.

50. Cross-Workspace diagnostic access requires explicit authorization.

51. Observability events are not automatically Domain Events.

52. High-volume streaming events SHOULD NOT automatically enter the durable Event Bus.

53. Observability SHOULD reuse normalized failure codes from owning components.

54. Diagnostics MAY assemble failure chains without owning failure semantics.

55. New providers/models SHOULD integrate through normalized telemetry without redesigning business capabilities.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* structured logs,
* metrics,
* distributed traces,
* request correlation,
* Request ID,
* Attempt ID,
* RoutePlan ID,
* Stream ID,
* Response ID,
* Trace ID,
* Correlation ID,
* stage latency,
* Routing metrics,
* provider/model/deployment metrics,
* Retry metrics,
* Fallback metrics,
* Cache metrics,
* Streaming metrics,
* Safety metrics,
* Cost/Usage projections,
* Deployment Health projections,
* basic alerts,
* structured diagnostics,
* privacy-safe telemetry,
* bounded local buffering,
* telemetry-backend failure tolerance,
* append-only material Audit events through separate audit infrastructure.

MVP SHOULD NOT log:

* raw Prompt,
* raw Context,
* raw source text,
* full AIResponse,
* raw provider response,
* secrets.

MVP MAY defer:

* advanced profiling,
* continuous performance profiling,
* adaptive sampling,
* distributed trace tail sampling,
* automated anomaly detection,
* predictive alerts,
* complex health scoring,
* long-term route-quality analytics,
* advanced diagnostic replay,
* automated root-cause analysis.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact telemetry field conventions,
* OpenTelemetry adoption,
* Trace/span naming,
* correlation propagation mechanism,
* metric naming convention,
* metric cardinality limits,
* log levels,
* telemetry sampling,
* trace sampling,
* telemetry retention,
* local buffering limits,
* backpressure/drop policy,
* diagnostic record schema,
* Health Projection ownership,
* Health scoring algorithm,
* health observation TTL,
* performance-projection ownership,
* Audit event schema,
* Audit infrastructure ownership,
* Policy-required audit-failure behavior,
* sensitive diagnostic mode,
* raw payload diagnostic retention,
* user/principal correlation policy,
* Workspace diagnostic permissions,
* alert thresholds,
* alert routing,
* anomaly detection,
* cost/usage metric reconciliation,
* telemetry Event Bus integration,
* broader system-trace composition across Capture/Recognition/Translation/Presentation.

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
* `COST_CONTROL.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/translation/`
* `../../02-modules/recognition/`
* `../../02-modules/presentation/`

Infrastructure:

* `../../03-infrastructure/logging/`
* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/storage/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RETRY_POLICY.md`
