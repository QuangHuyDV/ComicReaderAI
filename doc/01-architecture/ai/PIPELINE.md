# AI Pipeline

* **Document:** AI Architecture / Pipeline
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The AI Pipeline defines the provider-neutral execution architecture for an AI-powered operation in CRAI.

It describes how CRAI transforms:

```text
Resolved Business Intent
        +
Immutable / Explicit Context
        +
AI Capability Requirements
        |
        v
AI Request
        |
        v
Provider / Model Execution
        |
        v
Validated AI Response
```

while supporting:

* provider independence,
* deterministic orchestration,
* reproducibility,
* structured contracts,
* routing,
* retry,
* fallback,
* caching,
* streaming,
* safety,
* cost control,
* observability.

The AI Pipeline does NOT define CRAI's complete business-processing pipeline.

---

# Scope

The AI Pipeline begins when a CRAI capability requires AI execution.

Examples include:

* translation,
* language detection,
* context inference,
* character inference,
* text correction,
* summarization,
* classification,
* validation,
* optional OCR-related AI processing.

The pipeline ends when a normalized, validated AI result is returned to the calling capability.

---

# Non-Goals

The AI Pipeline does NOT own:

* screen capture,
* source acquisition,
* OCR orchestration,
* document hierarchy,
* TextBlock ownership,
* Translation business history,
* Session lifecycle,
* Presentation rendering,
* UI delivery,
* global business-pipeline orchestration,
* provider credentials,
* runtime worker scheduling.

Those responsibilities belong to their respective domains, modules, runtime or infrastructure layers.

---

# Architectural Boundary

CRAI's broader business flow may resemble:

```text
Capture
    |
    v
Recognition
    |
    v
Text Processing
    |
    v
Context / Configuration Resolution
    |
    v
Translation
    |
    v
Presentation
```

AI may participate in one or more of those capabilities.

Therefore:

```text
Business Pipeline
    !=
AI Pipeline
```

Instead:

```text
Business Capability
        |
        v
AI Operation
        |
        v
AI Pipeline
        |
        v
Normalized AI Result
        |
        v
Business Capability
```

---

# Design Goals

The AI Pipeline SHOULD provide:

* provider neutrality,
* model neutrality,
* explicit inputs,
* structured outputs,
* deterministic orchestration,
* stage isolation,
* reusable execution stages,
* reproducibility,
* observable execution,
* cost-aware routing,
* policy-aware routing,
* failure recovery,
* fallback,
* caching,
* streaming where appropriate,
* cancellation,
* bounded resource use,
* safe handling of sensitive context.

---

# Core Principle

The AI Pipeline executes AI intent.

It does not own business truth.

```text
Domain / Capability
        |
        | business intent
        | immutable references
        | constraints
        v
AI Pipeline
        |
        | normalized result
        v
Domain / Capability
```

The calling capability decides how the returned result affects business state.

---

# Canonical AI Operation Flow

A typical AI operation follows:

```text
AI Operation Request
        |
        v
Request Validation
        |
        v
Context Assembly
        |
        v
Prompt / Input Construction
        |
        v
Policy & Safety Evaluation
        |
        v
Route Planning
        |
        v
Cache Evaluation
        |
        v
Provider Request Construction
        |
        v
Model Execution
        |
        v
Response Parsing
        |
        v
Response Validation
        |
        v
Normalization
        |
        v
AI Operation Response
```

Not every operation requires every stage.

---

# Optional Stages

Stages MAY be skipped where they are not applicable.

Examples:

```text
Embedding request
    may not require Prompt Builder

Local deterministic model
    may not require provider routing

Non-streaming operation
    does not require streaming assembly

Non-cacheable operation
    skips cache lookup

Structured classification
    may use schema validation directly
```

Therefore the AI Pipeline is:

```text
Composable
```

rather than:

```text
One fixed sequence for every AI request
```

---

# Stage Categories

AI stages can be grouped into:

```text
Preparation
    Request Validation
    Context Assembly
    Input Construction

Governance
    Policy Evaluation
    Safety Evaluation
    Cost Constraints

Planning
    Route Planning
    Cache Evaluation

Execution
    Provider Request Construction
    Model Execution
    Streaming

Interpretation
    Response Parsing
    Response Validation
    Normalization

Recovery
    Retry
    Fallback
```

---

# Request Validation

Request Validation verifies that the operation contains sufficient and valid intent.

Possible checks include:

* capability type,
* required Language values,
* required context references,
* output contract,
* policy constraints,
* model requirements,
* streaming requirements,
* token/input limits,
* cancellation state.

Invalid requests SHOULD fail before provider execution where possible.

---

# Context Assembly

Context Assembly constructs the effective AI-facing context from already-authorized sources.

Possible inputs include:

* source content,
* source Language,
* target Language,
* Glossary Snapshot,
* Character Context Snapshot,
* Resolved Profile Snapshot,
* Resolved Configuration Snapshot,
* Session-derived operation context,
* prior bounded AI context,
* capability-specific metadata.

Context Assembly MUST NOT silently read arbitrary mutable state.

---

# Immutable Context Boundary

For durable operations:

```text
Mutable Business State
        |
        v
Domain / Application Resolution
        |
        v
Immutable / Explicit Context
        |
        v
AI Pipeline
```

The AI Pipeline SHOULD consume resolved context rather than independently resolving canonical business truth.

---

# Context Minimization

Only context necessary for the AI operation SHOULD be included.

This reduces:

* token usage,
* latency,
* cost,
* privacy exposure,
* prompt instability,
* provider data exposure.

More context is not automatically better context.

---

# Prompt / Input Construction

Prompt Builder transforms operation intent and context into model-consumable input.

Depending on model type, this MAY produce:

* messages,
* structured instructions,
* JSON schema,
* multimodal input,
* embedding input,
* classification input,
* tool definitions.

Prompt construction MUST remain separate from business-domain objects.

---

# Prompt Boundary

Domain concepts SHOULD NOT store concrete provider prompts as canonical business truth.

Conceptually:

```text
Profile / Context / Intent
        |
        v
Prompt Builder
        |
        v
Provider-neutral Prompt Representation
        |
        v
Provider Adapter
        |
        v
Provider-specific Request
```

---

# Policy Evaluation

Before external execution, applicable policy MUST be evaluated.

Examples:

* cloud processing allowed,
* provider allowed,
* model class allowed,
* data residency allowed,
* content classification permitted,
* external data sharing allowed,
* human provider review permitted.

Policy denial MUST prevent prohibited provider execution.

---

# Safety Evaluation

Safety checks MAY occur:

* before provider execution,
* during streamed execution,
* after response generation.

Safety policy MAY be capability-specific.

Safety SHOULD NOT be implemented as one hard-coded universal prompt.

---

# Cost Constraints

Cost controls MAY constrain:

* provider,
* model,
* maximum input size,
* maximum output tokens,
* retry count,
* fallback depth,
* parallel execution,
* cache policy.

Cost control contributes constraints.

It does not directly own model execution.

---

# Route Planning

Routing selects an execution plan.

Conceptually:

```text
Capability Requirements
        +
Model Requirements
        +
Workspace Policy
        +
Provider Availability
        +
Cost Constraints
        +
Latency Preference
        +
Quality Requirements
        |
        v
Route Plan
```

A Route Plan MAY include:

* provider,
* model,
* execution mode,
* fallback candidates,
* timeout,
* streaming mode,
* provider region,
* model parameters.

---

# Routing Boundary

Routing MUST use provider abstractions.

Business capabilities MUST NOT contain logic such as:

```text
if provider == "X"
```

Provider-specific behavior belongs to adapters/provider infrastructure.

---

# Route Reproducibility

Durable AI operations SHOULD record enough execution metadata to explain what actually ran.

Examples:

* selected provider reference,
* model identifier,
* model revision where available,
* route-plan revision,
* effective model parameters,
* fallback path,
* execution timestamp.

This execution metadata is separate from canonical business identity.

---

# Cache Evaluation

Cache MAY be evaluated before model execution.

A cache key SHOULD reflect semantic inputs that materially affect output.

Potential inputs include:

* capability,
* source-content fingerprint,
* prompt/input fingerprint,
* context snapshot references,
* model identity,
* effective parameters,
* relevant policy/configuration revision.

---

# Cache Safety

Cache reuse MUST NOT violate:

* tenant isolation,
* authorization,
* privacy,
* provider policy,
* content classification,
* freshness requirements,
* semantic correctness.

Cross-Workspace reuse is forbidden unless explicitly proven safe by cache policy.

---

# Provider Request Construction

After route selection:

```text
Provider-neutral AI Request
        |
        v
Provider Adapter
        |
        v
Provider-specific Request
```

Provider adapters own:

* provider API schema,
* provider model names,
* provider Language mappings,
* authentication headers,
* provider-specific parameters,
* SDK behavior.

---

# Model Execution

Model Execution performs the external or local AI inference.

Execution MAY be:

```text
LOCAL
CLOUD
HYBRID
```

and MAY support:

* synchronous response,
* streaming response,
* structured output,
* tool invocation,
* multimodal input.

Model Execution MUST NOT directly commit domain business state.

---

# Provider Credentials

The AI Pipeline MUST NOT own raw provider credentials.

Conceptually:

```text
Route Plan
    |
    v
Provider Configuration Reference
    |
    v
Secret Infrastructure
```

Credentials are resolved only at the infrastructure boundary.

---

# Response Parsing

Provider responses MUST be converted into provider-neutral representations before reaching higher-level capability logic.

```text
Raw Provider Response
        |
        v
Provider Adapter / Parser
        |
        v
Normalized AI Response
```

Calling capabilities SHOULD NOT parse provider-specific response formats.

---

# Response Validation

Validation MAY verify:

* expected schema,
* required fields,
* Language constraints,
* structural completeness,
* output length,
* terminology constraints,
* confidence requirements,
* safety constraints,
* capability-specific invariants.

Validation failure MAY trigger:

* repair,
* retry,
* fallback,
* rejection.

---

# Normalization

Normalization converts valid AI output into the canonical response contract expected by the calling capability.

Normalization MAY include:

* whitespace normalization,
* structured-field normalization,
* provider metadata extraction,
* confidence normalization,
* warning normalization.

Normalization MUST NOT silently invent missing business truth.

---

# Business Post-Processing Boundary

Business-specific transformations SHOULD normally remain in the calling capability.

For example:

```text
AI Translation Result
        |
        v
Translation Capability
        |
        v
Translation Revision
```

The AI Pipeline returns an AI result.

The Translation domain/module decides how that result becomes durable Translation history.

---

# Streaming

Streaming is an execution mode, not a separate business pipeline.

Conceptually:

```text
Model Execution
        |
        v
Provider Stream
        |
        v
Provider-neutral Chunks
        |
        v
Incremental Parser
        |
        v
Incremental Validation
        |
        v
Caller
```

---

# Streaming Invariant

Streaming MUST NOT change final business semantics.

A streamed and non-streamed execution with equivalent effective inputs SHOULD produce responses conforming to the same logical output contract.

---

# Partial Streaming State

Partial streamed output is provisional.

It MUST NOT automatically become durable domain truth.

```text
Partial AI Output
    !=
Committed Translation Revision
```

The calling capability determines when output is sufficiently complete and valid to commit.

---

# Cancellation

AI operations SHOULD support cooperative cancellation.

Cancellation MAY occur:

* before routing,
* before provider execution,
* during streaming,
* between retries,
* before fallback.

Cancellation SHOULD prevent unnecessary further provider cost where technically possible.

---

# Timeout

Timeout policy MAY exist at multiple levels:

```text
Operation Timeout
Provider Attempt Timeout
Streaming Idle Timeout
Retry Budget
Fallback Budget
```

Timeout configuration belongs to runtime/execution policy.

The AI Pipeline respects those constraints.

---

# Retry

Retry applies to a failed execution attempt.

Retry SHOULD consider:

* error classification,
* idempotency,
* provider semantics,
* retry budget,
* cost budget,
* latency budget,
* cancellation.

Retry MUST NOT be unconditional.

---

# Fallback

Fallback changes the execution route after failure or unacceptable result.

Possible fallback changes:

* model,
* provider,
* region,
* execution mode,
* quality tier.

Fallback SHOULD preserve the same business intent.

---

# Retry vs Fallback

```text
Retry
    = repeat compatible execution attempt

Fallback
    = select an alternative execution route
```

They MUST remain separate concepts.

---

# Repair

Structured-output failures MAY support a repair step.

Conceptually:

```text
Invalid AI Response
        |
        v
Repair Strategy
        |
        +--> Local deterministic repair
        |
        +--> AI-assisted repair
        |
        +--> Retry
        |
        +--> Fallback
        |
        v
Validation
```

Repair MUST NOT conceal material semantic corruption.

---

# Recovery

Recovery policy MAY combine:

* retry,
* fallback,
* cache reuse,
* structured repair,
* resumable streaming where supported.

Recovery policy is independent from individual stage implementation.

---

# Stage Contracts

Every stage MUST communicate through explicit contracts.

Conceptually:

```text
StageInput
    |
    v
Stage
    |
    v
StageOutput
```

Stages MUST NOT rely on shared mutable state for semantic communication.

---

# Statelessness

Stages SHOULD remain stateless where practical.

Persistent information SHOULD be obtained through explicit dependencies such as:

* Memory,
* Storage,
* Cache,
* Configuration,
* Policy,
* Provider Management.

Hidden process-local state MUST NOT materially alter durable output.

---

# Stage Isolation

A stage SHOULD be replaceable without requiring unrelated stages to change.

Examples:

* replace Prompt Builder,
* replace Router,
* replace model provider,
* replace cache implementation,
* replace response validator.

Replacement MUST preserve public contracts and semantic expectations.

---

# Stage Composition

Stages SHOULD be composed by orchestration.

Stages SHOULD NOT directly discover and invoke arbitrary next stages.

Preferred:

```text
Orchestrator
    |
    +--> Stage A
    +--> Stage B
    +--> Stage C
```

Avoid:

```text
Stage A
    |
    v
Stage B
    |
    v
Stage C
```

when those dependencies are hidden inside stage implementations.

---

# AI Pipeline Orchestrator

The AI Pipeline Orchestrator coordinates AI stages.

It MAY be responsible for:

* stage ordering,
* optional-stage selection,
* cancellation propagation,
* timeout propagation,
* retry coordination,
* fallback coordination,
* streaming coordination,
* observability correlation.

It MUST NOT become owner of domain business truth.

---

# Pipeline State

Runtime MAY track execution state such as:

```text
CREATED
PREPARING
ROUTING
EXECUTING
STREAMING
VALIDATING
RECOVERING
COMPLETED
FAILED
CANCELLED
```

These are execution states.

They MUST NOT replace domain lifecycle states.

---

# Observability

Every AI operation SHOULD have a stable correlation identity.

Observability MAY record:

* operation start/end,
* stage latency,
* provider latency,
* selected route,
* model identity,
* token/input usage,
* output usage,
* estimated cost,
* final cost where available,
* cache hit/miss,
* retry count,
* fallback count,
* validation failures,
* safety decisions,
* cancellation,
* failure classification.

---

# Observability Boundary

Telemetry MUST NOT automatically record:

* full prompts,
* raw source content,
* raw provider responses,
* credentials,
* sensitive Character context,
* private Glossary contents.

Sensitive payload logging requires explicit policy.

---

# Usage vs Telemetry

AI usage and operational telemetry are different.

```text
Usage
    = resource consumption / attribution

Telemetry
    = operational behavior
```

Example:

```text
12,000 model tokens
```

may contribute to usage.

```text
provider latency = 840 ms
```

is telemetry.

---

# Failure Classification

AI failures SHOULD use stable categories such as:

```text
INVALID_REQUEST
POLICY_DENIED
SAFETY_DENIED
ROUTING_FAILED
PROVIDER_UNAVAILABLE
PROVIDER_RATE_LIMITED
PROVIDER_TIMEOUT
MODEL_ERROR
INVALID_RESPONSE
VALIDATION_FAILED
CACHE_ERROR
CANCELLED
RESOURCE_LIMIT
INTERNAL_ERROR
```

Provider-specific errors SHOULD be normalized before reaching capability logic.

---

# Failure Isolation

A stage failure SHOULD NOT corrupt unrelated pipeline state.

Failure does NOT necessarily imply the whole business workflow must fail.

The calling capability/runtime determines whether to:

* retry,
* fallback,
* degrade,
* pause,
* request user action,
* fail the operation.

---

# Degraded Execution

Where allowed, the pipeline MAY produce a degraded but valid result.

Examples:

* cheaper fallback model,
* local model instead of cloud,
* no streaming,
* reduced optional context,
* cached compatible result.

Degradation MUST remain within:

* business intent,
* policy,
* safety,
* output contract.

---

# Extensibility

New AI stages MAY be added when they provide reusable AI execution behavior.

Examples:

* semantic context compression,
* tool planning,
* structured-output repair,
* confidence estimation,
* redaction,
* prompt optimization,
* result ranking.

New stages MUST NOT absorb unrelated business-domain ownership.

---

# AI Pipeline vs Business Pipeline

This distinction is critical.

```text
CRAI Business Pipeline

Capture
    |
Recognition
    |
Text Processing
    |
Translation
    |
Presentation
```

may invoke:

```text
AI Pipeline
```

zero, one or multiple times.

Example:

```text
Recognition
    |
    +--> AI Pipeline
          visual text recognition

Translation
    |
    +--> AI Pipeline
          translation

Validation
    |
    +--> AI Pipeline
          semantic validation
```

Therefore AI execution is a reusable architectural capability inside CRAI workflows.

---

# AI Pipeline vs Runtime Pipeline

The AI Pipeline defines AI execution semantics.

Runtime defines:

* workers,
* queues,
* scheduling,
* thread/process execution,
* resource allocation,
* persistence of execution checkpoints.

Conceptually:

```text
AI Pipeline
    = what AI execution stages mean

Runtime Pipeline
    = how those stages are executed operationally
```

---

# AI Pipeline vs Provider Management

AI Pipeline asks:

```text
Which execution route satisfies this operation?
```

Provider Management owns:

* provider registration,
* provider availability,
* provider capabilities,
* provider configuration,
* credential references,
* health state.

AI routing consumes Provider Management information.

---

# AI Pipeline vs Memory

Memory provides explicitly requested/retrieved context.

The AI Pipeline MAY consume Memory results.

It MUST NOT treat arbitrary hidden Memory as implicit business truth.

Memory participation SHOULD be:

* explicit,
* bounded,
* authorized,
* observable where appropriate.

---

# AI Pipeline vs Cache

Cache is an optimization layer.

A cache hit MUST be semantically equivalent to acceptable execution under the current cache policy.

Cache MUST NOT redefine business truth.

---

# AI Pipeline vs Safety

Safety defines constraints and evaluations.

The AI Pipeline invokes safety checks at appropriate execution boundaries.

Safety SHOULD remain independently evolvable.

---

# AI Pipeline vs Cost Control

Cost Control provides:

* budgets,
* thresholds,
* route constraints,
* retry/fallback budgets.

The AI Pipeline consumes those constraints.

Cost Control does not own provider execution.

---

# Architecture Invariants

1. AI Pipeline is not CRAI's complete business-processing pipeline.

2. AI Pipeline executes one AI operation on behalf of a calling capability.

3. A business workflow MAY invoke the AI Pipeline zero, one or multiple times.

4. Not every AI operation must execute the same stages.

5. AI stages are composable.

6. Stage communication uses explicit contracts.

7. Stages MUST NOT use shared mutable state for semantic communication.

8. Stages SHOULD remain stateless where practical.

9. AI Pipeline MUST remain provider-neutral.

10. Provider-specific behavior belongs to provider adapters/infrastructure.

11. Business capabilities MUST NOT branch directly on provider-specific implementation details.

12. AI Pipeline MUST NOT own canonical domain business truth.

13. Durable business state is committed by the owning domain/capability.

14. AI Pipeline SHOULD consume explicit/resolved business context.

15. AI Pipeline MUST NOT silently resolve arbitrary mutable canonical business state.

16. Mutable context affecting durable output SHOULD cross an immutable resolution boundary before AI execution.

17. Prompt construction is separate from domain business objects.

18. Concrete provider prompts MUST NOT become canonical domain truth.

19. Provider-specific request construction occurs only after routing.

20. Provider credentials remain outside AI Pipeline state.

21. Raw provider responses MUST NOT reach ordinary business capabilities directly.

22. Provider responses are parsed into provider-neutral contracts.

23. Response validation occurs before durable business commit where validation is required.

24. Partial streamed output is provisional.

25. Streaming MUST NOT change the logical output contract.

26. Cancellation SHOULD propagate through the pipeline.

27. Retry MUST be bounded and error-aware.

28. Retry and Fallback are separate concepts.

29. Fallback MUST preserve business intent.

30. Recovery MUST respect policy, safety, cost and cancellation constraints.

31. Cache reuse MUST preserve semantic correctness.

32. Cache reuse MUST preserve tenant isolation.

33. Cross-Workspace cache reuse requires explicit safety guarantees.

34. Routing considers capability requirements and applicable constraints.

35. Workspace/provider policy MUST be evaluated before prohibited external execution.

36. Safety MAY be evaluated at multiple pipeline boundaries.

37. Cost constraints MAY affect routing and recovery.

38. Route execution SHOULD be explainable.

39. Durable AI-backed outputs SHOULD retain sufficient execution metadata for provenance/debugging.

40. Provider errors SHOULD be normalized before reaching calling capabilities.

41. Stage failures SHOULD be isolated.

42. A stage failure does not automatically define business-workflow failure semantics.

43. Pipeline orchestration is separate from individual stage implementation.

44. Stages SHOULD NOT hide arbitrary next-stage invocation.

45. Runtime execution state is separate from domain lifecycle state.

46. Usage and Telemetry remain distinct.

47. Observability MUST NOT leak sensitive content by default.

48. Memory participation MUST be explicit and bounded.

49. Cache is an optimization, not business truth.

50. Safety is a constraint/evaluation concern, not provider execution.

51. Cost Control constrains execution but does not own execution.

52. Provider Management owns provider configuration/availability information.

53. Runtime owns workers, queues and scheduling.

54. Business Pipeline orchestration remains outside AI Pipeline ownership.

55. New AI stages MUST preserve public contracts and domain boundaries.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* provider-neutral AI Operation Request,
* provider-neutral AI Operation Response,
* request validation,
* context assembly,
* prompt/input construction,
* basic policy evaluation,
* route planning,
* one local provider path,
* one cloud provider path,
* provider adapters,
* non-streaming execution,
* optional streaming for supported models,
* structured response parsing,
* response validation,
* normalization,
* cancellation,
* operation timeout,
* bounded retry,
* provider/model fallback,
* Workspace-safe caching,
* token/usage measurement,
* estimated cost measurement,
* stage-level observability,
* normalized failure categories,
* correlation IDs,
* secure credential references,
* immutable/resolved context consumption for durable Translation operations.

MVP MAY defer:

* multi-provider parallel racing,
* speculative execution,
* automatic model benchmarking,
* semantic context compression,
* AI-assisted repair loops,
* tool-calling agents,
* multi-agent orchestration,
* dynamic prompt optimization,
* advanced confidence ranking,
* cross-Workspace cache reuse,
* provider-region optimization,
* adaptive cost/quality learning,
* automatic model fine-tuning,
* complex multimodal routing,
* resumable provider streams.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact AI Operation Request schema,
* exact AI Operation Response schema,
* whether `OperationContextSnapshot` is shared or capability-specific,
* exact stage-interface abstraction,
* whether stages are represented as runtime components or logical architecture only,
* exact Prompt intermediate representation,
* whether Prompt Builder produces one provider-neutral format,
* exact structured-output schema mechanism,
* exact routing score model,
* provider capability discovery mechanism,
* model capability taxonomy,
* model-version provenance guarantees,
* exact cache-key composition,
* cache TTL policy,
* negative caching,
* streaming validation strategy,
* partial-output repair behavior,
* retry budgets,
* fallback depth,
* timeout hierarchy,
* cancellation behavior for providers without cancellation APIs,
* policy-evaluation integration point,
* safety-evaluation integration points,
* context-size budgeting,
* context truncation strategy,
* Memory retrieval integration,
* sensitive-context redaction,
* cost-estimation accuracy,
* final-cost reconciliation,
* provider-health integration,
* route-plan persistence,
* degraded-mode behavior,
* local/cloud preference semantics,
* whether OCR AI execution uses this exact pipeline abstraction,
* whether embeddings use a specialized AI sub-pipeline,
* whether future tool-calling/agent execution extends this pipeline or receives a separate architecture.

---

# Related Documents

AI Architecture:

* `README.md`
* `STAGES.md`
* `REQUEST.md`
* `RESPONSE.md`
* `CONTEXT.md`
* `PROMPTS.md`
* `MODELS.md`
* `ROUTING.md`
* `RETRY.md`
* `FALLBACK.md`
* `STREAMING.md`
* `CACHE.md`
* `SAFETY.md`
* `COST_CONTROL.md`
* `OBSERVABILITY.md`
* `MEMORY.md`

Domain:

* `../domain/LANGUAGE.md`
* `../domain/GLOSSARY.md`
* `../domain/CHARACTER.md`
* `../domain/PROFILE.md`
* `../domain/SESSION.md`
* `../domain/TRANSLATION.md`
* `../domain/WORKSPACE.md`

Architecture:

* `../core/CAPABILITY_MAP.md`
* `../core/DATA_FLOW.md`
* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`

Runtime:

* `../runtime/BUSINESS_PIPELINE_ORCHESTRATION.md`
* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
