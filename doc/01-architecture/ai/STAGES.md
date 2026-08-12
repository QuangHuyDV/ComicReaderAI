# AI Pipeline Stages

* **Document:** AI Architecture / Pipeline Stages
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the logical processing stages that may participate in a CRAI AI operation.

Each stage has:

* one clear primary responsibility,
* explicit input/output contracts,
* provider-neutral semantics where applicable,
* isolated failure behavior,
* observable execution.

AI stages are **composable**.

Not every AI operation executes every stage.

The stage model MUST remain distinct from CRAI's broader business-processing flow.

---

# Architectural Scope

AI stages operate inside:

```text
AI Operation
```

They do NOT define CRAI's complete end-to-end workflow.

For example:

```text
Capture
OCR / Recognition
Text Processing
Presentation
```

are independent capabilities/modules.

They MAY invoke AI stages where appropriate, but they are not themselves universal AI Pipeline stages.

---

# Stage Philosophy

A stage represents one reusable logical transformation in AI execution.

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

A stage SHOULD:

* have one primary responsibility,
* expose explicit contracts,
* avoid hidden shared state,
* be independently replaceable,
* emit structured observability,
* be orchestrated externally.

A stage SHOULD NOT decide the complete business workflow.

---

# Stage Categories

AI Pipeline stages are grouped into six categories:

```text
Preparation
Governance
Planning
Execution
Interpretation
Recovery
```

Conceptually:

```text
AI Operation Request
        |
        v
Preparation
        |
        v
Governance
        |
        v
Planning
        |
        v
Execution
        |
        v
Interpretation
        |
        v
AI Operation Response

Recovery
    may intercept selected failures
```

---

# Canonical Stage Set

Recommended logical stages:

```text
Request Validation
Context Assembly
Input / Prompt Construction
Policy Evaluation
Safety Evaluation
Cost Evaluation
Route Planning
Cache Evaluation
Provider Request Adaptation
Model Execution
Stream Processing
Provider Response Adaptation
Response Validation
Response Normalization
Result Finalization
```

Recovery stages MAY include:

```text
Repair
Retry Coordination
Fallback Coordination
```

These stages are composable rather than mandatory.

---

# Optionality

An AI operation MAY skip stages that are not relevant.

Examples:

```text
Embedding
    may skip Prompt Construction

Local deterministic classifier
    may skip provider routing

Non-streaming request
    skips Stream Processing

Non-cacheable operation
    skips Cache Evaluation

Internal model
    may use a simplified Provider Adapter
```

Therefore:

```text
AI Pipeline Stage Set
    !=
Fixed Stage Sequence
```

---

# 1. Request Validation

## Purpose

Validate the AI operation before expensive or external execution begins.

---

## Responsibilities

Possible checks include:

* valid capability type,
* valid request identity,
* required fields,
* valid Language values,
* valid source references,
* valid expected output contract,
* configuration availability,
* input-size limits,
* cancellation state,
* required policy context,
* required snapshot references.

---

## Input

```text
AI Operation Request
```

---

## Output

```text
Validated AI Operation Request
```

or structured validation failure.

---

## Boundary

Request Validation validates AI-operation requirements.

It MUST NOT independently mutate business-domain state.

---

# 2. Context Assembly

## Purpose

Assemble the explicit context required by the AI operation.

---

## Possible Inputs

* source TextBlock Revision,
* source text snapshot,
* source Language,
* target Language,
* GlossarySnapshot,
* CharacterContextSnapshot,
* ResolvedProfileSnapshot,
* ResolvedConfigurationSnapshot,
* Session-derived operation context,
* bounded Memory results,
* capability-specific metadata.

---

## Responsibilities

Context Assembly MAY:

* select already-authorized context,
* combine immutable references,
* enforce context budgets,
* remove irrelevant context,
* order contextual information,
* preserve provenance.

---

## Output

```text
AI Context Package
```

---

## Boundary

Context Assembly MUST NOT silently read arbitrary mutable domain state.

Domain/Application resolvers remain authoritative for canonical business truth.

---

# 3. Input / Prompt Construction

## Purpose

Convert validated AI intent and context into a provider-neutral model input representation.

---

## Possible Output Forms

* message sequence,
* structured instruction object,
* multimodal input,
* embedding input,
* classification input,
* JSON-schema request,
* tool-definition request.

---

## Responsibilities

* organize instructions,
* apply reusable prompt templates,
* inject contextual references,
* define expected output structure,
* define model-facing constraints,
* preserve separation between instructions and source data.

---

## Output

```text
Provider-Neutral AI Input
```

---

## Boundary

Prompt construction MUST NOT become canonical business truth.

Business-domain entities SHOULD NOT store final provider prompts.

---

# 4. Policy Evaluation

## Purpose

Evaluate mandatory governance constraints before execution.

---

## Possible Checks

* cloud processing allowed,
* provider allowed,
* model class allowed,
* provider region allowed,
* content classification permitted,
* external sharing permitted,
* data residency satisfied,
* human-provider review permitted.

---

## Output

```text
Policy Decision
```

Examples:

```text
ALLOW
DENY
ALLOW_WITH_CONSTRAINTS
REQUIRE_LOCAL_PROCESSING
```

---

## Boundary

Policy Evaluation determines what execution is permitted.

It does NOT select the concrete provider by itself.

---

# 5. Safety Evaluation

## Purpose

Apply capability-appropriate safety constraints.

---

## Possible Evaluation Points

Safety MAY occur:

```text
before model execution
during streamed output
after complete response
```

---

## Responsibilities

Possible concerns include:

* unsafe request classification,
* prohibited transformation,
* sensitive-data handling,
* output restrictions,
* provider-specific safety compatibility.

---

## Output

```text
Safety Decision
```

or structured findings.

---

## Boundary

Safety MUST NOT be implemented solely as one universal hard-coded prompt.

---

# 6. Cost Evaluation

## Purpose

Translate applicable cost/budget intent into execution constraints.

---

## Possible Inputs

* Workspace budget policy,
* Routing Profile,
* operation cost limit,
* model cost metadata,
* token estimate,
* retry budget,
* fallback budget.

---

## Output

```text
Cost Constraints
```

Examples:

* maximum estimated cost,
* maximum output tokens,
* permitted quality tiers,
* permitted retry depth,
* permitted fallback depth.

---

## Boundary

Cost Evaluation constrains execution.

It does NOT invoke models.

---

# 7. Route Planning

## Purpose

Choose a compatible execution route.

---

## Inputs

Possible inputs:

```text
Capability Requirements
Model Requirements
Policy Decision
Safety Constraints
Cost Constraints
Provider Availability
Provider Capabilities
Latency Preference
Quality Preference
Locality Preference
```

---

## Responsibilities

Route Planning MAY select:

* provider,
* model,
* region,
* execution mode,
* streaming mode,
* timeout class,
* fallback candidates,
* model parameters.

---

## Output

```text
Route Plan
```

---

## Boundary

Routing knows provider capabilities and abstract provider identities.

It MUST NOT contain provider API payload logic.

---

# 8. Cache Evaluation

## Purpose

Determine whether a semantically compatible cached result may be used.

---

## Inputs

Possible cache identity components:

* capability,
* source fingerprint,
* AI Input fingerprint,
* Context fingerprint,
* Resolved Configuration fingerprint,
* model identity,
* model parameters,
* relevant policy state,
* pipeline version.

---

## Responsibilities

Cache Evaluation MAY:

* derive cache key,
* verify tenant isolation,
* verify semantic compatibility,
* evaluate freshness,
* return compatible cached result.

---

## Output

Either:

```text
Cache Hit
```

or:

```text
Cache Miss
```

---

## Boundary

Cache is an optimization.

A cache hit MUST satisfy the same semantic contract required from live execution.

---

# 9. Provider Request Adaptation

## Purpose

Convert provider-neutral AI input and Route Plan into a provider-specific request.

---

## Responsibilities

Provider Adapter MAY handle:

* provider request schema,
* provider model identifier,
* provider-specific parameters,
* message conversion,
* tool schema conversion,
* Language-code mapping,
* multimodal encoding,
* authentication references,
* streaming options.

---

## Input

```text
Provider-Neutral AI Input
+
Route Plan
```

---

## Output

```text
Provider-Specific Request
```

---

## Boundary

Provider-specific request logic MUST remain inside provider adapters.

Route Planning MUST NOT construct provider API payloads.

---

# 10. Model Execution

## Purpose

Execute the selected AI request.

---

## Responsibilities

Model Execution MAY:

* invoke local model,
* invoke cloud provider,
* send request,
* receive response,
* open stream,
* obey cancellation,
* obey timeout,
* collect execution metadata.

---

## Input

```text
Provider-Specific Request
```

---

## Output

Either:

```text
Raw Provider Response
```

or:

```text
Raw Provider Stream
```

---

## Boundary

Model Execution MUST NOT commit durable business state.

---

# 11. Stream Processing

## Purpose

Convert raw streamed provider output into provider-neutral incremental output.

---

## Responsibilities

* parse provider chunks,
* normalize chunk structure,
* detect stream completion,
* detect stream errors,
* preserve ordering,
* perform incremental validation where supported,
* propagate cancellation.

---

## Input

```text
Raw Provider Stream
```

---

## Output

```text
Provider-Neutral Response Chunks
```

---

## Boundary

Partial stream content is provisional.

It MUST NOT automatically become durable business truth.

---

# 12. Provider Response Adaptation

## Purpose

Convert raw provider output into a provider-neutral response representation.

---

## Responsibilities

* parse provider schema,
* extract generated content,
* extract structured output,
* extract finish reason,
* normalize token/usage metadata,
* normalize provider warnings,
* normalize tool results where applicable.

---

## Input

```text
Raw Provider Response
```

---

## Output

```text
Provider-Neutral AI Response
```

---

## Boundary

Raw provider response MUST NOT pass directly into ordinary business capabilities.

---

# 13. Response Validation

## Purpose

Verify that the AI response satisfies the operation contract.

---

## Validation MAY Include

* required fields,
* output schema,
* JSON/schema correctness,
* expected Language,
* structural completeness,
* source/output mapping,
* safety requirements,
* terminology requirements,
* confidence constraints,
* output-size constraints.

---

## Input

```text
Provider-Neutral AI Response
```

---

## Output

```text
Validated AI Response
```

or structured validation failure.

---

# 14. Response Normalization

## Purpose

Normalize a valid response into the canonical AI-operation result contract.

---

## Responsibilities

Possible normalization:

* whitespace normalization,
* canonical field naming,
* Language normalization,
* confidence normalization,
* warning normalization,
* structured mapping normalization,
* removal of provider-only metadata from semantic output.

---

## Output

```text
Normalized AI Result
```

---

## Boundary

Normalization MUST NOT silently invent missing semantic content.

---

# 15. Result Finalization

## Purpose

Construct the final AI Operation Response returned to the calling capability.

---

## Result MAY Include

```text
semantic result
warnings
quality metadata
execution provenance
usage metadata
cache metadata
route metadata
validation metadata
```

---

## Output

```text
AI Operation Response
```

---

## Boundary

Result Finalization returns a capability-facing result.

The calling domain/module decides whether and how to commit durable business state.

---

# Recovery Stages

Recovery is coordinated across stage failures.

Recovery stages MUST remain separate from normal semantic stages.

---

# Repair

## Purpose

Repair structurally invalid or incomplete AI output when safe.

Possible strategies:

```text
Deterministic Local Repair
AI-Assisted Repair
Reformat Request
Schema Repair
```

Repair MUST NOT hide semantic corruption.

---

# Retry Coordination

## Purpose

Repeat a compatible execution attempt after a retryable failure.

---

## Inputs

* normalized failure,
* retry policy,
* remaining budget,
* cancellation state,
* Route Plan,
* attempt history.

---

## Output

Either:

```text
Retry Attempt
```

or:

```text
Retry Exhausted
```

---

## Boundary

Individual stages SHOULD NOT independently implement unbounded retry loops.

Retry is coordinated by the pipeline orchestrator/runtime.

---

# Fallback Coordination

## Purpose

Select an alternative execution route after failure or unacceptable output.

Fallback MAY change:

* provider,
* model,
* region,
* local/cloud mode,
* quality tier.

Fallback MUST preserve the operation's business intent.

---

# Retry vs Fallback

```text
Retry
    = another compatible attempt

Fallback
    = alternative execution route
```

They MUST remain distinct.

---

# Stage Contract

Every stage SHOULD expose a logical contract containing:

```text
Stage Input
Stage Output
Stage Failure
Stage Metadata
```

Where applicable:

```text
Cancellation Context
Deadline / Budget Context
Correlation Context
```

---

# Stage Input

Stage Input SHOULD be immutable for the lifetime of one stage invocation.

A stage MUST NOT mutate another stage's input object through shared references.

---

# Stage Output

Stage Output MUST be explicit.

A stage MUST NOT communicate semantic results by modifying hidden shared runtime state.

---

# Stage Failure

Failure SHOULD contain normalized information such as:

```text
failureCategory
retryability
stageId
providerReference?
attemptId?
messageCode
diagnosticReference?
```

Raw provider errors SHOULD be normalized before propagating beyond provider boundaries.

---

# Stage Metadata

Stage metadata MAY include:

* start time,
* end time,
* duration,
* attempt number,
* cache status,
* route reference,
* usage summary,
* correlation identifiers.

Metadata MUST NOT be mixed into semantic output unless part of the public response contract.

---

# Stage IDs

Stages SHOULD have stable architecture identifiers.

Examples:

```text
request-validation
context-assembly
input-construction
policy-evaluation
route-planning
model-execution
response-validation
response-normalization
```

Stage IDs SHOULD remain provider-neutral.

---

# Statelessness

Stages SHOULD remain stateless where practical.

A stage MAY consume explicit dependencies such as:

```text
Memory
Cache
Provider Management
Policy
Configuration
Storage
```

However, hidden process-local state MUST NOT materially alter reproducible output.

---

# Shared Mutable State

Stages MUST NOT communicate using:

* global mutable objects,
* hidden singleton state,
* shared mutable request buffers,
* implicit provider-session state.

Explicit references and contracts are required.

---

# Provider State

Provider-specific transient state MAY exist inside Provider Adapter or execution runtime.

It MUST NOT escape as shared semantic stage state.

---

# Stage Composition

Pipeline Orchestrator decides:

* which stages execute,
* in what order,
* which stages are skipped,
* how failures are recovered,
* when fallback occurs.

Stages SHOULD NOT arbitrarily choose their successor.

---

# Deterministic Ordering

For one resolved execution plan, stage ordering MUST be deterministic.

This does NOT mean every AI operation uses one universal fixed ordering.

Example:

```text
Translation Operation A

Validation
Context
Prompt
Safety
Route
Execute
Validate
Normalize
```

while:

```text
Embedding Operation B

Validation
Context
Route
Execute
Normalize
```

Both are deterministic.

They simply use different stage graphs.

---

# Stage Graph

A pipeline MAY be represented as a directed acyclic stage graph.

Example:

```text
                +--> Policy Evaluation ----+
                |                          |
Request --------+--> Safety Evaluation ----+--> Route Planning
Validation      |                          |
                +--> Cost Evaluation ------+
```

Simple operations MAY use a linear graph.

Parallel pre-execution checks MAY run concurrently when their dependencies allow it.

---

# Stage Dependency

Dependencies MUST be explicit.

Example:

```text
Provider Request Adaptation
    requires
Route Plan
+
Provider-Neutral AI Input
```

Hidden stage dependency is forbidden.

---

# Parallel Stages

Stages MAY run in parallel when they do not depend on each other's output.

Possible examples:

```text
Policy Evaluation
Safety Evaluation
Cost Estimation
```

depending on the operation.

Parallelism is runtime behavior.

The stage dependency graph defines whether it is semantically valid.

---

# Cancellation

Cancellation is a cross-cutting execution context.

Every long-running stage SHOULD:

* observe cancellation,
* stop promptly where possible,
* avoid starting unnecessary downstream work,
* report cancellation consistently.

---

# Timeout

Stages MAY receive deadlines.

However, timeout policy is owned by orchestration/runtime.

A stage SHOULD NOT invent independent timeout behavior that conflicts with the operation budget.

---

# Retry Boundary

Not every stage is retryable.

Examples:

```text
Pure Request Validation
    normally not retried

Provider Execution
    may be retried

Context Assembly
    may be retried only if dependency failure is transient

Policy Denial
    must not be retried automatically
```

Retryability comes from normalized failure and recovery policy.

---

# Fallback Boundary

Fallback typically applies to route/execution-compatible failures.

Examples:

```text
Provider unavailable
Model unavailable
Rate limit
Unsupported capability
```

It SHOULD NOT be used to bypass:

```text
Policy denial
Safety denial
Invalid business request
```

---

# Observability

Every stage SHOULD produce structured observability.

Typical metadata:

* stage start,
* stage completion,
* duration,
* status,
* failure category,
* route/provider where relevant,
* usage where relevant,
* cache status,
* retry attempt,
* correlation ID.

---

# Sensitive Data

Observability MUST NOT log by default:

* raw prompts,
* source text,
* Translation output,
* Character context,
* private Glossary terms,
* credentials,
* raw provider responses.

---

# Metrics

Not every metric applies to every stage.

Examples:

```text
Model Execution
    token usage
    provider latency
    cost

Context Assembly
    context size
    selected context count

Cache Evaluation
    hit / miss

Response Validation
    finding count
```

Therefore stages SHOULD emit stage-appropriate metrics rather than one forced universal metric set.

---

# Diagnostics

Diagnostics MAY include:

* normalized error category,
* warning codes,
* validation findings,
* route explanation,
* cache explanation,
* fallback reason.

Diagnostics SHOULD prefer identifiers and hashes over raw content.

---

# Stage Replacement

A stage implementation SHOULD be replaceable if it preserves:

* input contract,
* output contract,
* failure semantics,
* semantic guarantees.

Example:

```text
Prompt Builder v1
    ->
Prompt Builder v2
```

MUST NOT require changes to unrelated business domains.

---

# Versioning

Stage implementations MAY be versioned.

Possible metadata:

```text
stageId
stageVersion
contractVersion
implementationVersion
```

Durable AI-backed outputs MAY preserve relevant stage/pipeline versions for reproducibility when necessary.

---

# Stage vs Module

Stage and CRAI Module are different concepts.

Example:

```text
AI Stage:
    Context Assembly
```

may use:

```text
Module:
    preferences
    provider-management
    translation
```

Likewise:

```text
Module:
    translation
```

may invoke several AI stages.

Therefore:

```text
Stage
    !=
Module
```

---

# Stage vs Domain

A stage is execution architecture.

A Domain owns semantic business truth.

Example:

```text
Response Normalization
    stage

Translation Revision
    domain artifact
```

The normalization stage MUST NOT become owner of Translation history.

---

# Stage vs Business Capability

Business capability may invoke one or more AI stages.

For example:

```text
Translation Capability
    |
    v
AI Pipeline
    |
    v
Normalized AI Translation Result
    |
    v
Translation Domain Commit
```

The AI stages support the capability.

They do not replace it.

---

# Removed From AI Stage Set

The following were present in the previous fixed pipeline model but are no longer universal AI stages:

```text
Capture
OCR
Layout Analysis
Text Normalization
Rendering
```

They belong to broader CRAI capabilities.

---

# Capture Boundary

Capture belongs to the Capture capability/module.

Capture MAY later invoke AI for specialized tasks, but:

```text
Capture
    !=
AI Stage
```

---

# OCR Boundary

OCR / Recognition belongs to Recognition/OCR architecture.

OCR MAY invoke AI/model execution internally.

It MUST NOT be treated as one universal stage in every AI operation.

---

# Layout Boundary

Layout Analysis belongs to recognition/layout or text-processing capability.

An AI model MAY implement one layout algorithm.

That does not make Layout Analysis a universal AI Pipeline stage.

---

# Text Normalization Boundary

Canonical source-text normalization belongs primarily to Text Processing / TextBlock preparation.

AI-specific input normalization MAY still occur inside AI stages.

These two meanings MUST NOT be conflated.

---

# Rendering Boundary

Rendering belongs to Presentation.

AI Pipeline MAY stream provisional output to Presentation.

Presentation does not become part of AI execution ownership.

---

# Architecture Invariants

1. AI stages operate inside an AI Operation.

2. AI stages do NOT define CRAI's complete business pipeline.

3. Capture is not a universal AI stage.

4. OCR is not a universal AI stage.

5. Layout Analysis is not a universal AI stage.

6. canonical Text Normalization is not a universal AI stage.

7. Rendering is not a universal AI stage.

8. AI stages are composable.

9. Not every AI operation executes every stage.

10. For one resolved stage graph, execution ordering is deterministic.

11. Each stage has one clear primary responsibility.

12. Stages communicate through explicit contracts.

13. Stage Inputs SHOULD be immutable during invocation.

14. Stages MUST NOT communicate semantic state through shared mutable objects.

15. Hidden stage dependencies are forbidden.

16. Stage dependencies SHOULD be explicit.

17. Stages SHOULD be independently replaceable.

18. Provider-specific API logic belongs to Provider Adapters.

19. Routing MUST NOT construct provider-specific payloads.

20. Provider-specific response parsing belongs to Provider Response Adaptation.

21. Raw provider responses MUST NOT reach ordinary business capabilities.

22. Model Execution MUST NOT commit durable business state.

23. Domain/Application layers resolve canonical business context before durable AI execution where required.

24. Context Assembly MUST NOT silently invent canonical business truth.

25. Prompt/Input Construction MUST remain separate from domain business objects.

26. Policy Evaluation MUST occur before prohibited external execution.

27. Safety Evaluation MAY occur at multiple execution boundaries.

28. Cost Evaluation constrains execution but does not execute models.

29. Cache is an optimization.

30. Cache reuse MUST satisfy normal semantic validation.

31. Streaming output is provisional until final validation/commit.

32. Retry is orchestrated; individual stages MUST NOT hide unbounded retry loops.

33. Not every failure is retryable.

34. Fallback and Retry are different mechanisms.

35. Fallback MUST NOT bypass mandatory Policy or Safety denial.

36. Recovery remains separate from normal stage semantics.

37. Cancellation is a cross-cutting execution context.

38. Timeout policy is owned by orchestration/runtime.

39. Observability is required for meaningful stage execution.

40. Metrics are stage-specific.

41. Observability MUST avoid sensitive content by default.

42. Stage failure categories SHOULD be normalized.

43. Stage execution state is runtime state, not domain lifecycle.

44. Stage replacement MUST preserve public semantic contracts.

45. Stage implementations MAY be versioned independently.

46. Stage is not Module.

47. Stage is not Domain.

48. AI Pipeline stages support business capabilities but do not replace them.

49. Durable outputs MAY retain relevant pipeline/stage execution provenance.

50. New stages MUST preserve provider neutrality and domain ownership boundaries.

---

# Recommended MVP Stage Set

For the initial CRAI MVP, the default AI Translation operation SHOULD support:

```text
Request Validation
Context Assembly
Input / Prompt Construction
Policy Evaluation
Route Planning
Provider Request Adaptation
Model Execution
Provider Response Adaptation
Response Validation
Response Normalization
Result Finalization
```

Optional MVP stages:

```text
Cache Evaluation
Safety Evaluation
Cost Evaluation
Stream Processing
Retry Coordination
Fallback Coordination
```

MVP does NOT require:

* generic AI agent planning,
* tool-calling orchestration,
* multi-agent stages,
* semantic response ranking,
* model racing,
* AI-assisted repair,
* dynamic prompt optimization,
* advanced context compression,
* cross-provider ensemble execution.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact Stage contract interface,
* exact Stage ID naming convention,
* whether stages are first-class runtime components,
* linear pipeline vs DAG runtime representation,
* which pre-execution checks may run in parallel,
* exact Context Package structure,
* exact Prompt/Input intermediate representation,
* exact Policy Decision contract,
* exact Safety Decision contract,
* Cost Constraint model,
* Route Plan schema,
* Provider Request Adapter interface,
* Provider Response Adapter interface,
* streaming chunk contract,
* Response Validation contract,
* Response Normalization contract,
* Result Finalization contract,
* repair-stage requirements,
* retry orchestration ownership,
* fallback orchestration ownership,
* cancellation propagation mechanism,
* timeout/deadline representation,
* stage-version persistence,
* stage observability event schema,
* provider execution metadata retention,
* whether embeddings use the same stage graph,
* whether multimodal recognition uses a specialized graph,
* whether future tool calling extends this stage model.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `REQUEST.md`
* `RESPONSE.md`
* `PROMPTS.md`
* `CONTEXT.md`
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

Modules:

* `../../02-modules/capture/`
* `../../02-modules/recognition/`
* `../../02-modules/text-processing/`
* `../../02-modules/translation/`
* `../../02-modules/presentation/`
* `../../02-modules/provider-management/`

Runtime:

* `../runtime/BUSINESS_PIPELINE_ORCHESTRATION.md`
* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
