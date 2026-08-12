# AI Architecture

* **Architecture Area:** AI
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This directory defines the provider-neutral AI execution architecture used by CRAI capabilities.

The AI architecture describes how CRAI:

```text id="0cjlav"
Business / Capability Intent
        +
Resolved Context
        +
Execution Constraints
        |
        v
AI Request
        |
        v
AI Execution Architecture
        |
        v
Provider-Neutral AI Response
```

while supporting:

* multiple AI providers,
* local and cloud execution,
* model/deployment routing,
* prompt/input construction,
* context assembly,
* memory retrieval,
* streaming,
* retry,
* fallback,
* caching,
* safety,
* cost control,
* observability.

The AI architecture does NOT own CRAI's complete business-processing pipeline.

---

# Core Boundary

CRAI distinguishes:

```text id="6e12c6"
Business Capability
    = what CRAI is trying to accomplish
```

from:

```text id="xhy899"
AI Execution
    = how an AI-capable execution resource
      is used to help accomplish it
```

Examples:

```text id="9hcs4g"
Translation
Recognition
Language Detection
Character Inference
Semantic Validation
```

MAY invoke the AI architecture.

They do not become part of the AI architecture merely because AI is used.

---

# AI Architecture Is Not the Business Pipeline

A broader CRAI flow MAY look like:

```text id="s21imc"
Capture
    |
    v
Recognition
    |
    v
Text Processing
    |
    v
Translation
    |
    v
Presentation
```

AI MAY participate in one or more of these capabilities.

Therefore:

```text id="agax6c"
Business Pipeline
    !=
AI Pipeline
```

---

# Canonical AI Operation

A typical AI operation is:

```text id="3qmxgq"
AI Request
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
Policy / Safety Constraints
    |
    v
Routing
    |
    v
Cache Evaluation
    |
    v
Provider Request Adaptation
    |
    v
Model Execution
    |
    v
Provider Response Adaptation
    |
    v
Response Validation
    |
    v
Response Normalization
    |
    v
AI Response
```

Not every operation requires every stage.

---

# Composable Execution

The AI Pipeline is composable.

Examples:

```text id="eedh01"
Translation

Request
 -> Context
 -> Prompt
 -> Routing
 -> Execution
 -> Validation
 -> Response
```

```text id="99a88o"
Language Detection

Request
 -> Routing
 -> Execution
 -> Response
```

```text id="1q2jjj"
Embedding

Request
 -> Routing
 -> Execution
 -> Embedding Result
```

There is no mandatory universal stage sequence for every AI capability.

---

# Responsibilities

The AI architecture defines:

* provider-neutral AI Request contracts,
* provider-neutral AI Response contracts,
* AI stage semantics,
* Context assembly,
* Memory retrieval integration,
* Prompt/Input construction,
* model capability abstraction,
* model/deployment routing,
* provider adaptation boundaries,
* streaming semantics,
* Retry semantics,
* Fallback semantics,
* Cache semantics,
* Safety constraints,
* Cost constraints/accounting integration,
* AI observability.

---

# Non-Responsibilities

The AI architecture does NOT own:

* Capture,
* OCR/Recognition business ownership,
* TextBlock canonical truth,
* Translation history,
* Glossary truth,
* Character truth,
* Profile truth,
* Session lifecycle,
* Presentation rendering,
* Workspace Policy truth,
* Provider credentials,
* runtime worker scheduling,
* persistent domain storage,
* user-interface behavior.

---

# Design Principles

CRAI AI architecture follows these principles:

* Provider Neutral
* Capability Driven
* Explicit Contracts
* Composable Stages
* Immutable / Explicit Context
* Capability-Based Routing
* Policy-Aware
* Safety-Aware
* Cost-Aware
* Recovery-Aware
* Observable
* Privacy-Aware
* Independently Replaceable
* Local / Cloud Neutral
* Technology Independent

---

# Provider Neutrality

Business capabilities MUST NOT depend directly on concrete provider APIs.

Avoid:

```text id="q10nd4"
if provider == X
```

inside Translation, Recognition or other business capabilities.

Provider-specific behavior belongs behind:

```text id="8tlfrb"
Provider Adapter
```

---

# Provider Boundary

Conceptually:

```text id="30piv7"
Provider-Neutral AI Input
        |
        v
Provider Adapter
        |
        v
Provider-Specific Request
        |
        v
Provider
        |
        v
Raw Provider Response
        |
        v
Provider Adapter
        |
        v
Provider-Neutral AI Response
```

---

# AI Request Boundary

`AIRequest` represents semantic AI operation intent.

It MAY contain:

* capability,
* input references,
* Context references,
* resolved configuration references,
* output requirements,
* model requirements,
* execution constraints,
* cost constraints,
* correlation.

It MUST NOT contain:

* provider request payload,
* runtime Retry history,
* Provider Attempt history,
* mutable Stage History,
* credentials.

---

# AI Response Boundary

`AIResponse` represents finalized provider-neutral AI execution output.

It is NOT automatically:

```text id="wcvldg"
TranslationRevision
Recognition Result
Presentation Artifact
```

The calling capability decides how the AI result affects business state.

---

# Business Commit Boundary

Recommended:

```text id="ufztpy"
AI Response
    |
    v
Calling Capability
    |
    v
Capability / Domain Validation
    |
    v
Business Commit
```

AI execution MUST NOT directly become canonical domain truth without the owning capability's rules.

---

# Context

AI Context is an operation-specific bounded selection of semantic information.

Possible sources include:

* immutable TextBlock Revision,
* GlossarySnapshot,
* CharacterContextSnapshot,
* ResolvedConfigurationSnapshot,
* SessionContextSnapshot,
* previous Translation Revision,
* explicit Memory Retrieval Result.

Context Assembly MUST NOT become a universal mutable business-state resolver.

---

# Context Boundary

Preferred:

```text id="v0z041"
Domain / Application Resolution
        |
        v
Immutable / Explicit Inputs
        |
        v
Context Assembly
```

Avoid:

```text id="txf9zp"
Context Builder
    secretly reads
    current Glossary
    latest Character
    mutable Session
    latest Profile
```

during durable execution.

---

# Memory

AI Memory provides optional retrievable supporting knowledge.

Memory MAY include:

* summaries,
* observations,
* retrieval notes,
* correction patterns,
* temporary AI working context.

Memory does NOT own:

* Character truth,
* Glossary truth,
* Profile definitions,
* Session resume state,
* Translation history.

---

# Memory Flow

```text id="o0pbdu"
Memory Store
    |
    v
Memory Retrieval
    |
    v
Retrieval Result
    |
    v
Context Assembly
```

Memory MUST NOT be injected implicitly into every AI operation.

---

# Prompt / Input Construction

Prompt/Input Construction converts:

```text id="i2xx5g"
AI Request Intent
+
AI Context Package
+
Output Contract
```

into a provider-neutral model-facing representation.

Prompt is a derived execution artifact.

It is NOT canonical business truth.

---

# Instruction Semantics

CRAI SHOULD use semantic instruction categories such as:

```text id="bcqj9b"
Governance
Capability
Operation
Context
Output Contract
Data
```

rather than treating provider roles such as:

```text id="14r9yw"
system
developer
user
```

as canonical architecture concepts.

Provider Adapter performs role mapping where required.

---

# Models

Model architecture defines:

* Model Descriptor,
* Model Deployment,
* capabilities,
* modalities,
* limits,
* Language support,
* structured-output support,
* streaming support,
* lifecycle metadata.

It does NOT own:

* routing decisions,
* provider health,
* execution-attempt lifecycle.

---

# Model Separation

```text id="kg663i"
Model Descriptor
    stable capability metadata

Model Deployment
    executable configured path

Runtime Health
    dynamic operational state

Execution Attempt
    one concrete invocation
```

These concepts MUST remain distinct.

---

# Routing

Routing selects an executable RoutePlan.

It evaluates:

* capability compatibility,
* Model requirements,
* Deployment availability,
* Policy constraints,
* Safety constraints,
* cost constraints,
* health,
* Language capability,
* context limits,
* quality/latency preferences.

Routing does NOT execute the model.

---

# RoutePlan

Conceptually:

```text id="37grz9"
RoutePlan
├── modelId
├── deploymentId
├── providerId
├── providerConfigurationId
├── executionMode
├── region?
├── streamingMode
├── effectiveParameters?
└── alternateRoutes[]
```

Provider-native request payloads MUST NOT appear in RoutePlan.

---

# Retry

Retry means:

```text id="romhq5"
same semantic Request
+
normally same RoutePlan
+
new Execution Attempt
```

Retry is appropriate for compatible transient execution failures.

Retry MUST NOT silently reroute.

---

# Fallback

Fallback means:

```text id="5ovx4d"
same semantic Request
+
new compatible RoutePlan
```

Fallback MAY change:

* Deployment,
* Model,
* Provider,
* Region,
* local/cloud execution mode,

only through Routing.

---

# Retry vs Fallback

```text id="za447d"
Retry
    = try compatible route again
```

```text id="5g8vry"
Fallback
    = choose another compatible route
```

They MUST remain separate.

---

# Recovery

Recovery orchestration MAY choose among:

```text id="gpgean"
Repair
Retry
Fallback
Cache
Fail
Request User Action
```

No individual recovery mechanism owns the entire recovery workflow.

---

# Streaming

Streaming is an execution mode.

It produces:

```text id="s0r63j"
AIResponseChunk
        |
        v
StreamAssemblyState
        |
        v
AIResponseCandidate
        |
        v
Final Validation
        |
        v
AIResponse
```

Partial stream output is provisional.

---

# Streaming Boundary

```text id="lre6wd"
AIResponseChunk
    !=
AIResponse
```

and:

```text id="9nlcwd"
Partial Translation
    !=
TranslationRevision
```

Presentation MAY render provisional output but does not gain semantic ownership.

---

# Cache

AI Cache is an optimization layer.

Possible cache classes include:

```text id="hswv7d"
Result Cache
Prompt Cache
Context Cache
Routing Cache
Model Metadata Cache
Estimation Cache
```

There is no single universal cache position.

---

# Cache Boundary

Cache MUST NOT become canonical business storage.

Loss of Cache MUST NOT destroy:

* Translation history,
* Glossary snapshots,
* Character history,
* Profile revisions.

---

# Safety

Safety defines constraints on whether and how AI execution may occur.

Safety covers:

* instruction-authority boundaries,
* prompt-injection resistance,
* sensitive-data exposure,
* privacy,
* external-processing restrictions,
* output safety,
* tool/action risk.

Safety does NOT replace generic Request/Response validation.

---

# Safety and Policy

Workspace/Governance owns authoritative Policy.

Safety consumes/apply safety-relevant constraints.

Routing enforces applicable execution constraints.

---

# Cost Control

Cost Control distinguishes:

```text id="d4lfj9"
Pricing
Estimate
Budget
Quota
Reservation
Usage
Actual Cost
```

It produces constraints and decisions.

It does NOT directly select models or modify Context.

---

# Cost Flow

```text id="eh63np"
Pre-Route Estimate
        |
        v
Cost Constraints
        |
        v
Routing
        |
        v
Refined Estimate
        |
        v
Reservation / Execution
        |
        v
Actual Usage
        |
        v
Reconciliation
```

---

# Observability

AI Observability provides:

* Logs,
* Metrics,
* Traces,
* Runtime Events,
* Diagnostics.

Derived telemetry may support:

* Health,
* Alerts,
* Performance projections.

---

# Observability Boundary

Observability is separate from:

```text id="xgt6mt"
Audit
Usage Ledger
Cost accounting
Domain events
```

Those concepts may share infrastructure but MUST remain semantically distinct.

---

# Sensitive Telemetry

By default, AI telemetry MUST NOT record:

* raw source content,
* full Prompt,
* full Context,
* private Glossary content,
* Character context,
* full AIResponse,
* raw provider response,
* credentials.

Prefer:

* IDs,
* hashes,
* sizes,
* failure codes,
* versions,
* durations.

---

# High-Level Architecture

Canonical high-level architecture:

```text id="km3kkn"
Calling Capability
        |
        v
AI Request
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
Safety / Policy Constraints
        |
        v
Routing
        |
        v
Cache Evaluation
        |
        v
Provider Request Adaptation
        |
        v
Model Execution
        |
        v
Streaming? / Response Adaptation
        |
        v
Response Validation
        |
        v
Response Finalization
        |
        v
AI Response
        |
        v
Calling Capability
```

---

# Cross-Cutting Concerns

Cross-cutting AI execution concerns include:

```text id="6gkpvv"
Memory Retrieval
Safety
Cost Constraints
Retry
Fallback
Cache
Observability
Cancellation
Policy
```

Cross-cutting does NOT imply shared ownership.

Each concern retains its own semantics.

---

# Directory Structure

```text id="oq1m82"
01-architecture/ai/
│
├── README.md
├── PIPELINE.md
├── STAGES.md
├── REQUEST.md
├── RESPONSE.md
├── CONTEXT.md
├── MEMORY.md
├── PROMPTS.md
├── MODELS.md
├── ROUTING.md
├── RETRY.md
├── FALLBACK.md
├── STREAMING.md
├── CACHE.md
├── SAFETY.md
├── COST_CONTROL.md
└── OBSERVABILITY.md
```

---

# Document Overview

| Document           | Purpose                                                        |
| ------------------ | -------------------------------------------------------------- |
| `README.md`        | AI architecture overview and ownership boundaries              |
| `PIPELINE.md`      | Composable AI-operation execution flow                         |
| `STAGES.md`        | Logical AI execution stages and stage contracts                |
| `REQUEST.md`       | Provider-neutral AI operation request contract                 |
| `RESPONSE.md`      | Provider-neutral finalized AI response contract                |
| `CONTEXT.md`       | Context requirements, selection, materialization and budgeting |
| `MEMORY.md`        | Optional retrievable AI-supporting memory architecture         |
| `PROMPTS.md`       | Provider-neutral Prompt/Input construction                     |
| `MODELS.md`        | Model Descriptor, Deployment and capability abstraction        |
| `ROUTING.md`       | RoutePlan selection from compatible execution candidates       |
| `RETRY.md`         | Same-route compatible retry semantics                          |
| `FALLBACK.md`      | Alternative-route recovery semantics                           |
| `STREAMING.md`     | Incremental provider-neutral AI response delivery              |
| `CACHE.md`         | AI execution and derived-artifact caching semantics            |
| `SAFETY.md`        | AI execution safety constraints and evaluations                |
| `COST_CONTROL.md`  | Pricing, estimates, budgets, usage and cost constraints        |
| `OBSERVABILITY.md` | AI logs, metrics, tracing and operational diagnostics          |

---

# Recommended Reading Order

Recommended order:

```text id="7a2n4q"
1. README.md

2. PIPELINE.md
3. STAGES.md

4. REQUEST.md
5. RESPONSE.md

6. CONTEXT.md
7. MEMORY.md
8. PROMPTS.md

9. MODELS.md
10. ROUTING.md

11. RETRY.md
12. FALLBACK.md
13. STREAMING.md

14. CACHE.md
15. SAFETY.md
16. COST_CONTROL.md
17. OBSERVABILITY.md
```

---

# Reading Logic

The order moves from:

```text id="zibnr4"
AI architecture boundary
        |
        v
execution stages
        |
        v
request / response contracts
        |
        v
context and model input
        |
        v
execution resources and routing
        |
        v
recovery / streaming
        |
        v
cross-cutting optimization,
safety, cost and observability
```

---

# Integration Points

AI architecture integrates with several CRAI areas.

---

# Translation

Translation MAY invoke AI for translation generation.

AI returns:

```text id="q4q3f4"
AIResponse
```

Translation owns:

```text id="trg6fb"
Translation
TranslationRevision
```

---

# Recognition

Recognition MAY invoke AI for:

* visual text understanding,
* OCR assistance,
* layout inference.

AI does NOT own canonical Recognition/TextBlock truth.

---

# Presentation

Presentation MAY consume:

* provisional stream output,
* committed Translation artifacts.

Presentation is not part of the AI pipeline.

---

# Session

Session may contribute explicit temporary operation context.

AI execution MUST NOT directly depend on mutable Session state once durable operation inputs have been resolved.

---

# Glossary

Glossary provides:

```text id="zq9vxb"
GlossarySnapshot
```

AI Context may consume it.

AI MUST NOT mutate Glossary truth.

---

# Character

Character provides:

```text id="xt652b"
CharacterContextSnapshot
```

AI may consume it for interpretation/inference.

AI MUST NOT silently redefine Character truth.

---

# Profile

Profile provides reusable processing intent.

Resolved Profile/configuration semantics may influence:

* Context,
* Prompt,
* Routing,
* output requirements.

Concrete Prompt strings/model/provider choices MUST NOT become Profile identity.

---

# Workspace

Workspace contributes:

* policy,
* provider availability,
* defaults,
* budget,
* privacy constraints,
* tenant scope.

AI does not own Workspace governance.

---

# Provider Management

Provider Management owns:

* provider registration,
* Provider Configuration,
* credential references,
* provider availability,
* provider capabilities.

AI Routing consumes Provider Management projections.

---

# Runtime

Runtime owns:

* workers,
* scheduling,
* queues,
* process execution,
* delayed Retry,
* cancellation mechanisms,
* execution checkpoints.

AI architecture defines semantic execution contracts that runtime implements.

---

# Infrastructure

Infrastructure may provide:

* Provider Adapters,
* storage,
* Cache,
* telemetry,
* logging,
* secrets,
* scheduling,
* networking.

Infrastructure MUST NOT redefine AI semantic contracts.

---

# AI vs Domain

Domain owns business truth.

AI architecture owns execution semantics.

Example:

```text id="l5awis"
TranslationRevision
    = Domain

AIResponse
    = AI execution artifact
```

---

# AI vs Modules

Modules expose application/capability behavior.

A Module MAY invoke several AI architecture concerns.

Example:

```text id="194f0l"
Translation Module
    |
    +--> Context
    +--> AI Request
    +--> Routing
    +--> Streaming
    +--> Response
```

The AI architecture does not replace Module ownership.

---

# AI vs Runtime

AI architecture says:

```text id="9x0uvu"
what an AI execution stage means
```

Runtime says:

```text id="3yk8de"
how that stage actually runs
```

---

# AI vs Provider

AI architecture is provider-neutral.

Provider adapters map canonical contracts to provider-specific protocols.

---

# AI vs Presentation

AI may emit provisional or finalized AI output.

Presentation decides:

* visual layout,
* overlay,
* rich text,
* user interaction.

Rendering is not an AI stage.

---

# AI vs Audit

AI execution emits telemetry and may emit material audit candidates.

Audit architecture determines:

* what must be durably audited,
* retention,
* accountability semantics.

---

# AI vs Usage / Cost

AI execution produces usage signals.

Usage/Cost architecture owns:

* accounting,
* budgets,
* reservations,
* actual cost.

Metrics are only operational projections.

---

# Architecture Goals

CRAI AI architecture aims for:

* provider independence,
* high-quality AI-assisted Translation,
* support for Chinese/English/Vietnamese-oriented workflows,
* low perceived latency,
* bounded cost,
* reliable recovery,
* safe private-content processing,
* local/offline capability,
* scalable execution,
* explainable Routing,
* reproducible durable inputs,
* independently replaceable components.

---

# Architecture Invariants

1. CRAI AI architecture is provider-neutral.

2. AI architecture does not own the complete CRAI business-processing pipeline.

3. Capture is not a universal AI stage.

4. Recognition/OCR is not a universal AI stage.

5. Rendering/Presentation is not an AI stage.

6. AI operations are initiated by calling capabilities.

7. AI Pipeline stages are composable.

8. Not every AI operation uses every stage.

9. AI Requests use standardized provider-neutral contracts.

10. AI Responses use standardized provider-neutral contracts.

11. AI Request represents operation intent rather than execution history.

12. AI Response represents AI execution output rather than committed domain truth.

13. Calling capabilities own business commit semantics.

14. Context Assembly consumes explicit/resolved context.

15. Context Assembly MUST NOT become a hidden universal mutable-state resolver.

16. Memory is supporting knowledge, not canonical domain truth.

17. Memory participation MUST be explicit.

18. Prompt/Input Construction is derived execution representation.

19. Provider-specific message roles are not canonical CRAI instruction semantics.

20. Models are selected by capabilities/constraints rather than provider-name business logic.

21. Model Descriptor, Deployment, Health and Execution Attempt are distinct.

22. Routing selects RoutePlan.

23. Routing does not execute models.

24. Retry normally preserves RoutePlan.

25. Fallback produces another RoutePlan through Routing.

26. Retry and Fallback are separate recovery mechanisms.

27. Streaming output is provisional until finalization.

28. Partial streaming output MUST NOT automatically become business truth.

29. Streaming and non-streaming converge to equivalent logical AIResponse contracts.

30. Cache is an optimization layer.

31. Cache MUST NOT become canonical business storage.

32. Cache placement depends on cache class.

33. Safety constrains execution but does not replace generic validation.

34. Safety DENY MUST NOT be bypassed by Retry or Fallback.

35. Cost Control constrains/account execution but does not directly select routes.

36. Pricing, Estimate, Budget, Quota, Reservation and Usage are separate concepts.

37. Observability is operational telemetry, not business truth.

38. Audit is distinct from telemetry.

39. Usage/Cost accounting is distinct from observability metrics.

40. Health is a derived runtime projection.

41. Routing MAY consume explicit Health projections.

42. Instrumentation MUST NOT change semantic AI behavior.

43. Sensitive Prompt/Context/content MUST NOT be logged by default.

44. Provider credentials MUST remain outside AI semantic contracts.

45. Provider-specific request/response formats MUST remain behind adapters.

46. Domain Language identity MUST remain provider-neutral.

47. AI operations SHOULD preserve sufficient provenance for diagnostics/reproducibility.

48. Historical business artifacts MUST NOT depend on mutable AI runtime state.

49. New providers/models SHOULD be integrable without redesigning business capabilities.

50. Runtime/infrastructure implementations MUST remain replaceable without redefining AI semantic contracts.

---

# Recommended MVP

CRAI MVP AI architecture SHOULD support:

* Translation AI operation,
* Language Detection operation where needed,
* provider-neutral Request/Response,
* Context Assembly,
* GlossarySnapshot context,
* CharacterContextSnapshot context,
* Prompt/Input construction,
* Model Descriptor,
* Model Deployment,
* one local model path,
* one cloud model path,
* capability-based Routing,
* RoutePlan,
* basic Streaming,
* Retry,
* Fallback,
* Result Cache,
* Context/Prompt Cache where useful,
* basic Safety controls,
* cost estimation,
* Workspace budget evaluation,
* attempt-level Usage,
* structured Logs/Metrics/Traces,
* tenant-safe execution.

MVP SHOULD prioritize:

```text id="79ihqs"
Correct boundaries
Explicit contracts
Chinese -> Vietnamese Translation quality
Low perceived latency
Privacy
Cost predictability
Local/cloud flexibility
```

over advanced autonomous AI features.

---

# Deferred AI Capabilities

CRAI SHOULD initially defer:

* autonomous agents,
* multi-agent orchestration,
* unrestricted tool calling,
* automatic long-term AI learning,
* provider racing,
* model ensembles,
* speculative execution,
* autonomous Prompt optimization,
* adaptive Routing learning,
* cross-Workspace Memory,
* cross-Workspace AI Result reuse,
* complex multimodal generation,
* advanced model fine-tuning.

---

# Open Architecture Questions

The following SHOULD remain explicit until implementation/prototype validation:

* exact AI Request schema,
* exact AI Response schema,
* exact Context Package schema,
* exact Prompt/Input intermediate representation,
* exact Model capability taxonomy,
* Model Deployment ownership boundary,
* Provider Adapter interface,
* exact RoutePlan schema,
* Routing Policy representation,
* Recovery Policy architecture,
* Retry/Fallback budget composition,
* Streaming chunk contract,
* Cache identity,
* Safety Decision schema,
* Cost/Usage ownership implementation,
* Usage Ledger location,
* Health Projection ownership,
* Audit infrastructure ownership,
* exact local model runtime architecture,
* provider capability discovery,
* provider/model evaluation framework,
* whether Memory becomes a dedicated module,
* whether AI execution records are persisted,
* exact relationship between AI Pipeline and runtime Pipeline.

---

# Related Architecture

Domain:

```text id="eqgl6q"
../domain/
```

Modules:

```text id="eoaikc"
../../02-modules/
```

Runtime:

```text id="21wegc"
../runtime/
```

Infrastructure:

```text id="h72ius"
../../03-infrastructure/
```

Important related areas include:

* `../domain/WORKSPACE.md`
* `../domain/PROJECT.md`
* `../domain/LANGUAGE.md`
* `../domain/GLOSSARY.md`
* `../domain/CHARACTER.md`
* `../domain/PROFILE.md`
* `../domain/SESSION.md`
* `../domain/TRANSLATION.md`
* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/BUSINESS_PIPELINE_ORCHESTRATION.md`
* `../../02-modules/provider-management/`
* `../../02-modules/translation/`
* `../../02-modules/recognition/`
* `../../02-modules/presentation/`

---

# Canonical Mental Model

The CRAI AI architecture can be summarized as:

```text id="nz7rwz"
Calling Capability
        |
        v
AI Request
        |
        v
Resolved Context
        |
        v
Prompt / Input
        |
        v
Safety + Cost + Policy Constraints
        |
        v
Routing
        |
        v
RoutePlan
        |
        v
Execution
        |
        v
AI Response
        |
        v
Calling Capability
        |
        v
Domain Commit
```

Supporting concerns:

```text id="0u6jug"
Memory
Cache
Retry
Fallback
Streaming
Observability
```

---

# Final Principle

The central boundary is:

```text id="dj045x"
Business Meaning
        |
        v
Explicit AI Intent
        |
        v
Provider-Neutral AI Execution
        |
        v
Normalized AI Result
        |
        v
Owning Business Capability
```

AI exists to support CRAI capabilities.

It does not replace their ownership.
