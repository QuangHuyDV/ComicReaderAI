# AI Models

* **Document:** AI Architecture / Models
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the provider-neutral model abstraction used by CRAI AI architecture.

The Model layer describes:

* model identity,
* model family,
* deployment/runtime identity where required,
* declared capabilities,
* limits,
* compatibility,
* modality support,
* language support,
* cost characteristics,
* execution characteristics,
* lifecycle metadata.

Its purpose is to let Routing determine:

```text
Which currently available execution resource
can satisfy this AI operation?
```

without making CRAI business capabilities depend on concrete provider/model names.

---

# Core Principle

```text
AI Request Requirements
        |
        v
Routing
        |
        +--> Model Catalog
        +--> Provider Capabilities
        +--> Runtime Availability
        +--> Policy / Cost / Health
        |
        v
Route Plan
        |
        v
Model Runtime / Provider Adapter
```

The Model architecture describes execution resources.

It does NOT own route selection or model execution lifecycle.

---

# Scope

This document covers:

* model descriptors,
* model identities,
* capability declarations,
* modality support,
* context/output limits,
* model compatibility,
* model versioning,
* deployment/runtime references,
* health references,
* model availability metadata,
* cost metadata,
* lifecycle and deprecation.

It does NOT define:

* Route scoring,
* Retry,
* Fallback,
* provider credentials,
* provider API request shapes,
* runtime attempts,
* prompts,
* AI Requests,
* AI Responses,
* domain business logic.

---

# Non-Goals

The Model layer is NOT:

* Model Router,
* Provider Manager,
* runtime execution engine,
* model downloader,
* model server,
* provider SDK wrapper,
* business Profile,
* Translation configuration,
* Prompt Template.

---

# Design Principles

The Model architecture SHOULD be:

* provider-neutral,
* capability-driven,
* version-aware,
* runtime-selectable,
* explicit about limits,
* explicit about dynamic vs static metadata,
* extensible,
* observable,
* policy-compatible,
* local/cloud neutral.

---

# Architectural Separation

CRAI SHOULD distinguish:

```text
Model Descriptor
    relatively stable model metadata

Model Deployment
    executable configured instance

Model Runtime State
    dynamic availability / health

Model Execution Attempt
    one concrete invocation
```

These MUST NOT be collapsed into one object.

---

# Model Descriptor

A `ModelDescriptor` represents the provider-neutral description of an AI model or model offering.

Recommended:

```text
ModelDescriptor
├── modelId
├── providerId
├── modelFamily?
├── displayName
├── modelVersion?
├── lifecycleStatus
├── capabilities
├── modalities
├── limits
├── languageCapabilities?
├── executionCharacteristics?
├── pricingReference?
├── compatibility
├── releaseMetadata?
└── descriptorVersion
```

`ModelDescriptor` contains relatively stable metadata.

It MUST NOT contain active request state.

---

# Model ID

`modelId` is CRAI's canonical model-catalog identity.

It SHOULD be stable enough for:

* routing,
* configuration,
* observability,
* provenance,
* cache compatibility.

It MUST NOT require business capabilities to know a provider-native model string.

---

# Provider Model Identifier

Provider-native identifiers belong to provider configuration/adapters.

Example:

```text
CRAI modelId:
    model_001
```

may map internally to a provider identifier.

That mapping MUST NOT leak into Domain or generic capability contracts.

---

# Model Identity vs Model Family

CRAI SHOULD distinguish:

```text
Model Family
    = related model lineage

Model
    = routable model offering/version
```

Example conceptual relationship:

```text
ModelFamily
    |
    +--> Model A
    +--> Model B
    +--> Model C
```

Routing normally operates on routable model descriptors rather than broad family names.

---

# Model Version

Model versioning is complicated because providers differ in how precisely they expose model revisions.

CRAI SHOULD preserve:

```text
modelId
modelVersion?
providerVersionReference?
observedVersion?
```

where available.

Missing provider revision MUST remain distinguishable from a known immutable version.

---

# Version Stability

A provider alias may point to changing underlying behavior.

Therefore:

```text
provider alias
    !=
guaranteed immutable model revision
```

When exact reproducibility matters, CRAI SHOULD retain the strongest version/provenance information available.

---

# Model Deployment

A `ModelDeployment` represents one configured executable access path to a model.

Recommended:

```text
ModelDeployment
├── deploymentId
├── modelId
├── providerConfigurationId
├── executionMode
├── region?
├── endpointReference?
├── runtimeReference?
├── capabilityOverrides?
├── limitOverrides?
├── status
└── deploymentVersion
```

---

# Why Deployment Is Separate

The same logical model MAY be available through:

* several regions,
* several accounts,
* local runtime,
* cloud endpoint,
* managed deployment.

Example:

```text
Model X
├── Cloud Deployment SG
├── Cloud Deployment US
└── Local Deployment
```

Routing often selects a Deployment, not only a model name.

---

# Execution Modes

Possible values:

```text
LOCAL
CLOUD
SELF_HOSTED
MANAGED
HYBRID
```

Execution mode is an operational characteristic.

It MUST NOT become business-domain identity.

---

# Local Models

Local Model Deployments MAY use runtimes such as:

* Ollama,
* llama.cpp,
* vLLM,
* other local inference runtimes.

Those runtime names belong to infrastructure/deployment metadata.

The generic AI Pipeline SHOULD still consume the same provider-neutral contracts.

---

# Cloud Models

Cloud models are exposed through Provider Configurations.

Cloud capability does not automatically imply:

* permitted by Workspace Policy,
* available in every region,
* suitable for private data,
* low latency.

Routing must evaluate the actual deployment context.

---

# Specialized Models

A model MAY specialize in one or more capabilities.

Examples:

* Translation,
* classification,
* embedding,
* vision,
* OCR-related understanding,
* summarization.

CRAI SHOULD model specialization through capability declarations rather than one exclusive category.

A model MAY support several capabilities simultaneously.

---

# Model Capability

Capabilities describe what a model can technically support.

Recommended representation:

```text
ModelCapabilities
├── tasks[]
├── inputModalities[]
├── outputModalities[]
├── instructionCapabilities
├── structuredOutputCapabilities
├── streamingCapabilities
├── toolCapabilities
├── contextCapabilities
├── languageCapabilities?
└── executionCapabilities?
```

---

# Task Capabilities

Possible AI task capabilities:

```text
TEXT_GENERATION
TRANSLATION
CLASSIFICATION
SUMMARIZATION
LANGUAGE_DETECTION
SEMANTIC_VALIDATION
STRUCTURED_EXTRACTION
VISION_ANALYSIS
EMBEDDING
CHARACTER_INFERENCE
CUSTOM
```

This taxonomy SHOULD align with `REQUEST.md`.

---

# Capability Is Not Quality

Critical distinction:

```text
supports TRANSLATION
```

does NOT mean:

```text
high-quality Chinese -> Vietnamese Translation
```

Capability declares technical suitability.

Quality evaluation belongs to:

* evaluation,
* routing scoring,
* benchmarks,
* user feedback.

---

# Input Modalities

Possible modalities:

```text
TEXT
IMAGE
AUDIO
VIDEO
STRUCTURED_DATA
EMBEDDING_VECTOR
MULTIMODAL
```

A model MAY support several input modalities.

---

# Output Modalities

Possible outputs:

```text
TEXT
STRUCTURED_TEXT
STRUCTURED_OBJECT
EMBEDDING_VECTOR
AUDIO
IMAGE
CUSTOM
```

CRAI should declare actual supported output contracts independently from provider-native response shapes.

---

# Instruction Capabilities

Prompt Architecture may require certain instruction semantics.

Possible declarations:

```text
INSTRUCTION_FOLLOWING
INSTRUCTION_HIERARCHY
SEPARATE_GOVERNANCE_INSTRUCTIONS
MULTI_MESSAGE_INPUT
DATA_INSTRUCTION_SEPARATION
```

Provider Adapter determines the concrete representation.

---

# Instruction Compatibility

If a required CRAI instruction authority cannot be represented safely on a model/deployment:

```text
model/deployment is incompatible
```

Routing SHOULD NOT silently weaken the required semantics.

---

# Structured Output Capability

Structured-output support SHOULD be more precise than:

```text
STRUCTURED_OUTPUT = true
```

Possible levels:

```text
NONE
PROMPT_ONLY
JSON_MODE
SCHEMA_GUIDED
STRICT_SCHEMA
TOOL_SCHEMA
```

This enables accurate routing.

---

# Streaming Capability

Recommended representation:

```text
StreamingCapability
├── supported
├── incrementalText
├── incrementalStructuredOutput?
├── cancellationSupported?
├── usageDuringStream?
└── resumeSupported?
```

Streaming support is not always binary.

---

# Tool Capability

Possible declarations:

```text
TOOL_CALLING
MULTI_TOOL
PARALLEL_TOOL_CALLING
STRUCTURED_TOOL_ARGUMENTS
```

MVP may not require these capabilities.

---

# Vision Capability

Vision capability MAY declare:

```text
imageInputSupported
multipleImagesSupported
maximumImageCount?
imageSizeLimits?
regionInputSupported?
visualTextUnderstanding?
layoutUnderstanding?
```

These are capability declarations.

They MUST NOT imply that the AI Model owns CRAI Recognition domain semantics.

---

# Embedding Capability

Embedding models SHOULD declare:

```text
embeddingDimension?
maximumInputSize?
inputLanguageRanges?
normalizedVector?
distanceCompatibility?
```

Embedding output belongs to retrieval infrastructure where used.

---

# Context Capability

Recommended:

```text
ModelContextLimits
├── maximumInputUnits
├── maximumOutputUnits
├── maximumCombinedUnits?
├── unitType
├── reservedSystemOverhead?
└── knownLimitations[]
```

CRAI MUST NOT assume all context limits are measured identically.

---

# Context Units

Possible units MAY include:

```text
TOKEN
CHARACTER
BYTE
IMAGE
AUDIO_SECOND
CUSTOM
```

Token count may remain common for language models, but it is not a universal AI-resource unit.

---

# Maximum Output

Maximum output MUST remain distinct from context-window size.

Example:

```text
maxInput != maxOutput
```

Routing and Prompt/Input validation must evaluate both.

---

# Effective Limits

Effective runtime limit MAY differ from catalog limit due to:

* deployment configuration,
* provider account constraints,
* selected structured-output mode,
* multimodal input,
* provider changes.

Therefore:

```text
effective limit
    =
Model Descriptor
+
Deployment Overrides
+
Runtime Provider State
```

---

# Language Capability

Models MAY declare language support.

Recommended:

```text
ModelLanguageCapability
├── languageRange
├── capabilityType
├── supportLevel
├── direction?
├── qualityClass?
└── limitations?
```

Canonical Language/LanguageRange values MUST come from `LANGUAGE.md`.

---

# Language Support Is Capability-Specific

A model MAY:

* generate Vietnamese well,
* recognize Chinese,
* poorly translate Chinese to Vietnamese,
* support English embeddings only.

Therefore language support MUST NOT be modeled as one global list alone.

---

# Language Pair Capability

Translation routing MAY optionally use directional LanguagePair metadata.

Example:

```text
zh-Hans -> vi
```

This is more precise than:

```text
supports zh-Hans
supports vi
```

when reliable data exists.

---

# Language Capability Source

Language capability metadata MAY come from:

* provider declarations,
* CRAI benchmark,
* administrator configuration,
* observed evaluation.

The source SHOULD be explicit.

Runtime support data is not immutable Language-domain truth.

---

# Model Compatibility

A model/deployment MAY declare compatibility with:

* AI Request schema,
* AI Response schema,
* Prompt/Input contract,
* structured-output contract,
* streaming contract,
* tool contract,
* multimodal contract.

Compatibility SHOULD be explicit and version-aware.

---

# Model Requirements Matching

Routing compares:

```text
AIModelRequirements
        |
        v
Model Descriptor
+
Deployment
+
Runtime State
```

Examples:

```text
Request requires:
    TRANSLATION
    zh-Hans -> vi
    STRICT_SCHEMA
    context >= N
```

Only matching candidates proceed.

---

# Model Requirements Must Be Provider-Neutral

Business Request SHOULD say:

```text
structuredOutputRequired
qualityTier
localExecutionRequired
minimumContextSize
```

rather than:

```text
use provider X model Y
```

Exact model pinning remains an explicit special case.

---

# Exact Model Pinning

Exact model/deployment selection MAY be allowed for:

* diagnostics,
* experiments,
* benchmarking,
* reproducibility,
* explicit administration.

Pinning MUST NOT become the default business configuration mechanism.

---

# Model Catalog

A `ModelCatalog` provides queryable Model Descriptors.

Conceptually:

```text
ModelCatalog
├── Descriptor A
├── Descriptor B
└── Descriptor C
```

The Catalog is configuration/runtime metadata.

It is NOT a Domain aggregate.

---

# Model Catalog Responsibilities

Catalog MAY support:

* registration,
* update,
* deprecation,
* capability validation,
* lookup,
* discovery,
* version metadata.

It MUST NOT choose the final execution route.

---

# Model Registration

Registration verifies that a Model Descriptor is internally valid.

Possible checks:

* stable model ID,
* provider exists,
* capability schema valid,
* limits valid,
* Language metadata valid,
* deployment compatibility,
* version metadata valid.

Registration does NOT mean the model is currently healthy.

---

# Model Lifecycle

Recommended descriptor lifecycle:

```text
REGISTERED
    |
    v
ACTIVE
    |
    +--> DEPRECATED
    |
    +--> DISABLED
    |
    v
RETIRED
```

Possible initial state:

```text
DRAFT
```

when manual registration/review is required.

---

# Lifecycle Meaning

`REGISTERED`

Descriptor exists but is not necessarily routable.

`ACTIVE`

Model is eligible for routing when compatible and available.

`DEPRECATED`

Model remains available for historical/pinned use but SHOULD NOT be preferred for new operations.

`DISABLED`

Administrative policy prevents normal selection.

`RETIRED`

Model is no longer available for normal execution.

---

# What Is Not Model Lifecycle

The following MUST NOT be Model lifecycle states:

```text
SELECTED
EXECUTING
COMPLETED
FAILED
```

Those belong to:

```text
Model Execution Attempt
```

or AI runtime execution.

---

# Runtime Availability

Model/Deployment availability is dynamic runtime state.

Recommended:

```text
RuntimeAvailability
├── deploymentId
├── state
├── observedAt
├── expiresAt?
├── reason?
└── source
```

Possible states:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
MAINTENANCE
UNKNOWN
```

---

# Health

Health is a runtime observation.

It MUST remain separate from relatively stable Model Descriptor state.

Possible health dimensions:

```text
availability
latency
errorRate
rateLimitPressure
capacity
providerStatus
localResourcePressure
```

---

# Health Is Deployment-Specific

A model MAY be healthy through one Deployment and unavailable through another.

Example:

```text
Model X
├── Deployment SG -> AVAILABLE
└── Deployment US -> UNAVAILABLE
```

Therefore health SHOULD normally attach to Deployment/runtime path rather than only global `modelId`.

---

# Health Freshness

Health observations MUST have freshness semantics.

Avoid:

```text
health = HEALTHY
```

with no observation time.

Prefer:

```text
state: AVAILABLE
observedAt: ...
ttl: ...
```

Stale health SHOULD degrade to `UNKNOWN` according to policy.

---

# Provider Availability

Provider Management remains authoritative for provider configuration and provider-level runtime state.

Model architecture consumes or references that information.

It MUST NOT duplicate provider-management ownership.

---

# Model Execution Attempt

One execution attempt is runtime state.

Conceptually:

```text
ModelExecutionAttempt
├── attemptId
├── requestId
├── deploymentId
├── routePlanId
├── startedAt
├── completedAt?
├── status
├── usage
└── failure?
```

This belongs to runtime/execution architecture.

It is NOT part of Model Descriptor.

---

# Attempt Status

Possible runtime states:

```text
CREATED
STARTING
EXECUTING
STREAMING
COMPLETED
FAILED
CANCELLED
TIMED_OUT
```

These MUST NOT appear as Model lifecycle.

---

# Model Selection

Model selection belongs to Routing.

Routing MAY consider:

* required AI capability,
* input/output modality,
* Context limits,
* Language capability,
* structured-output requirement,
* streaming requirement,
* Policy,
* privacy,
* locality,
* cost,
* latency,
* quality,
* Deployment availability,
* health,
* quota/rate-limit pressure,
* user/admin preference.

---

# User Preference Boundary

User/Profile preference MAY influence routing.

Example:

```text
prefer local
prefer low cost
prefer high quality
```

Preference SHOULD NOT directly modify Model Descriptor.

---

# Cost Metadata

Model/Deployment MAY expose pricing metadata.

Recommended:

```text
ModelPricing
├── pricingReference
├── inputUnitCost?
├── outputUnitCost?
├── imageUnitCost?
├── requestCost?
├── currency?
├── effectiveFrom?
└── pricingVersion?
```

Pricing is dynamic external configuration.

It MUST NOT be assumed immutable.

---

# Cost Profile

A coarse derived cost class MAY exist:

```text
FREE
LOW
STANDARD
HIGH
PREMIUM
UNKNOWN
```

This is useful for routing but SHOULD NOT replace concrete pricing where cost calculations matter.

---

# Latency Metadata

Latency SHOULD normally come from observed runtime telemetry rather than static Model truth.

Catalog MAY hold a coarse expectation:

```text
INTERACTIVE
STANDARD
BATCH
UNKNOWN
```

Routing may combine this with current telemetry.

---

# Quality Metadata

Quality SHOULD NOT be a universal fixed scalar on Model Descriptor.

Quality is:

* task-specific,
* Language-specific,
* dataset-specific,
* version-specific.

Recommended evaluation representation:

```text
ModelEvaluation
├── modelId
├── deploymentId?
├── capabilityType
├── languagePair?
├── benchmarkId
├── score
├── evaluatorVersion
├── evaluatedAt
└── confidence?
```

Routing MAY consume evaluation projections.

---

# Model Benchmark Boundary

Model Catalog MAY reference benchmark/evaluation summaries.

Evaluation infrastructure owns benchmark execution and result history.

---

# Availability vs Entitlement

A model can be technically available but not usable by a Workspace.

Example:

```text
Deployment:
    AVAILABLE

Workspace Entitlement:
    not permitted
```

Routing must evaluate both.

Model availability MUST NOT imply authorization.

---

# Availability vs Policy

Likewise:

```text
Model:
    available

Workspace Policy:
    cloud processing forbidden
```

makes that route unusable.

Policy remains separate from model capability.

---

# Model Registry vs Provider Management

Recommended distinction:

```text
Model Catalog
    knows
    model/deployment descriptors
    capabilities
    limits
    compatibility
```

```text
Provider Management
    knows
    provider configuration
    credentials references
    provider capabilities
    provider health
```

```text
Routing
    combines both
```

---

# Provider Adapter

Provider Adapter owns provider-specific model mapping.

Conceptually:

```text
modelId / deploymentId
        |
        v
Provider Adapter
        |
        v
provider model identifier
```

Business capabilities never need the provider-native identifier.

---

# Local Runtime Adapter

Local model execution SHOULD use the same conceptual adapter boundary.

Example:

```text
ModelDeployment
    |
    v
Local Runtime Adapter
    |
    v
Ollama / llama.cpp / vLLM
```

Generic Routing should not branch throughout the codebase on each local runtime.

---

# Model Parameters

Provider-neutral model requirements and execution parameters MUST remain separate.

Possible generic parameters:

```text
outputRandomness?
maximumOutputUnits?
reasoningLevel?
```

only if CRAI can define stable provider-neutral semantics.

Provider-specific parameters remain adapter-level.

---

# Parameter Mapping

Recommended flow:

```text
Resolved Execution Intent
        |
        v
Route Plan
        |
        v
Provider-Neutral Execution Parameters
        |
        v
Provider Adapter
        |
        v
Provider-Specific Parameters
```

Mapping SHOULD be versioned where behavior materially affects output.

---

# Reproducibility

Durable AI-backed outputs SHOULD retain sufficient model execution provenance.

Possible references:

```text
modelId
modelVersion?
deploymentId
providerId
providerVersion?
parameterMappingVersion?
execution timestamp
```

Exact availability depends on provider guarantees.

---

# Reproducibility Limit

CRAI MUST NOT claim byte-for-byte reproducibility when the underlying provider/model does not guarantee it.

Instead CRAI SHOULD preserve:

```text
best available execution provenance
```

and distinguish:

```text
reproducible business inputs
```

from:

```text
deterministic model behavior
```

---

# Model Changes

A Model Descriptor change SHOULD classify whether it is:

```text
METADATA_ONLY
CAPABILITY_CHANGE
LIMIT_CHANGE
COMPATIBILITY_CHANGE
PRICING_CHANGE
DEPRECATION
PROVIDER_MAPPING_CHANGE
```

Not every metadata update invalidates cache or routing behavior.

---

# Cache Compatibility

Model identity MAY participate in AI execution cache identity.

Relevant material MAY include:

```text
modelId
modelVersion
deployment class
effective parameters
prompt/input hash
context hash
```

Exact cache rules belong to `CACHE.md`.

---

# Provider Aliases and Cache

If a provider alias can silently change underlying model behavior, cache policy SHOULD treat it conservatively.

A mutable alias MUST NOT be assumed equivalent to an immutable version.

---

# Registration Validation

Before activation, Model Descriptor SHOULD validate:

* capability taxonomy,
* modality combinations,
* limit consistency,
* Language ranges,
* structured-output levels,
* streaming declarations,
* compatibility references,
* deployment mappings,
* provider ownership.

---

# Capability Verification

Provider-declared capabilities MAY be inaccurate or incomplete.

CRAI MAY distinguish:

```text
DECLARED
VERIFIED
OBSERVED
EXPERIMENTAL
```

capability evidence.

---

# Capability Evidence

Recommended:

```text
CapabilityEvidence
├── source
├── status
├── verifiedAt?
├── benchmarkReference?
└── notes?
```

Routing policy MAY prefer verified capabilities for critical workloads.

---

# Capability Degradation

A model/deployment MAY temporarily lose effective capability.

Example:

```text
structured output endpoint unavailable
```

Dynamic capability degradation belongs to runtime availability/capability state.

It MUST NOT silently rewrite historical Model Descriptor metadata.

---

# Model Retirement

When a model is retired:

* existing historical provenance remains resolvable,
* exact pinned configurations may become non-executable,
* Routing must choose alternatives for new unpinned work,
* explicit fallback/migration may be required.

Historical AI/Translation artifacts remain valid records of what previously executed.

---

# Model Replacement

Replacement SHOULD NOT silently mean semantic equivalence.

Example:

```text
Model A retired
Model B recommended
```

does NOT imply:

```text
Model A == Model B
```

Route/default changes affect future operations only.

---

# Failure Categories

Model/catalog-related failures MAY include:

```text
MODEL_NOT_FOUND
MODEL_DESCRIPTOR_INVALID
MODEL_DISABLED
MODEL_RETIRED
MODEL_CAPABILITY_MISMATCH
MODEL_LANGUAGE_UNSUPPORTED
MODEL_MODALITY_UNSUPPORTED
MODEL_CONTEXT_LIMIT_EXCEEDED
MODEL_OUTPUT_LIMIT_EXCEEDED
MODEL_STRUCTURED_OUTPUT_UNSUPPORTED
MODEL_STREAMING_UNSUPPORTED
MODEL_TOOL_CAPABILITY_UNSUPPORTED
MODEL_DEPLOYMENT_NOT_FOUND
MODEL_DEPLOYMENT_UNAVAILABLE
MODEL_DEPLOYMENT_INCOMPATIBLE
MODEL_VERSION_UNRESOLVED
```

Provider transport/authentication failures belong to provider/runtime execution failures.

---

# Observability

Model-related observability MAY include:

* request count by model/deployment,
* success/failure rate,
* latency,
* first-token latency,
* streaming duration,
* input/output units,
* cost,
* route selection count,
* fallback count,
* capability mismatch count,
* health status,
* utilization where local.

---

# Observability Boundary

Observability data is runtime-derived.

It MUST NOT be written back into Model Descriptor as canonical static truth without an explicit aggregation/update workflow.

---

# Model Usage Analytics

Routing MAY consume derived performance projections such as:

```text
ModelPerformanceProjection
├── deploymentId
├── capabilityType
├── recentLatency
├── recentSuccessRate
├── recentCost
├── sampleWindow
└── observedAt
```

This is derived runtime data.

---

# Security and Privacy

Model metadata SHOULD NOT contain:

* provider API keys,
* raw credentials,
* user source content,
* prompts,
* model responses.

Credential references belong to Provider Configuration.

---

# Tenant Boundary

Model Catalog MAY be:

* system-wide,
* Workspace-filtered,
* deployment-specific.

A globally known model does NOT imply every Workspace may use it.

Workspace policy, provider configuration, entitlement and authorization determine usability.

---

# Extensibility

Adding a new model SHOULD require:

1. Model Descriptor,
2. compatible Deployment,
3. Provider/Runtime Adapter mapping,
4. capability validation,
5. routing availability.

It SHOULD NOT require redesigning the AI Pipeline.

---

# Architecture Invariants

1. AI business capabilities MUST NOT depend on concrete provider model identifiers.

2. Model architecture is provider-neutral.

3. Model selection belongs to Routing.

4. Model Catalog MUST NOT perform final route selection.

5. Provider Management remains separate from Model Catalog.

6. Provider-specific APIs remain behind Provider/Runtime Adapters.

7. Model Descriptor is separate from Model Deployment.

8. Model Descriptor is separate from Runtime Health.

9. Model Descriptor is separate from Model Execution Attempt.

10. `SELECTED`, `EXECUTING` and `COMPLETED` are NOT Model lifecycle states.

11. Model lifecycle describes registration/availability governance, not request execution.

12. Runtime availability MUST include freshness semantics.

13. Health SHOULD normally be Deployment-specific.

14. Stale Health MUST NOT be treated as current truth indefinitely.

15. Model capability declarations are structured.

16. Capabilities are capability-specific rather than provider-name-driven.

17. Capability support does not imply high quality.

18. Quality evaluation is task/language/version-specific.

19. Input Modalities and Output Modalities are explicit.

20. Structured Output capability SHOULD declare support level.

21. Streaming capability SHOULD be more precise than a single boolean where needed.

22. Tool capability remains distinct from general text-generation capability.

23. Context limits and output limits remain distinct.

24. Context/resource unit MUST NOT universally assume tokens.

25. Effective limits may depend on Deployment/runtime state.

26. Language capability uses canonical Language/LanguageRange values.

27. Provider-specific Language codes MUST NOT enter generic Model metadata.

28. Translation LanguagePair capability is directional when represented.

29. AI Request Model Requirements remain provider-neutral.

30. Exact Model pinning is exceptional and explicit.

31. Prompt architecture may declare required Model capabilities.

32. Routing MUST reject models that cannot preserve mandatory Prompt/Instruction semantics.

33. Availability does NOT imply Workspace authorization.

34. Availability does NOT imply Policy compatibility.

35. Entitlement remains separate from model availability.

36. Cost metadata and pricing may change independently from Model identity.

37. Latency observations belong primarily to runtime telemetry.

38. Model evaluation results remain derived/evaluation data.

39. Model Version and provider alias MUST NOT be assumed equivalent.

40. Historical execution SHOULD preserve strongest available model provenance.

41. CRAI MUST NOT claim deterministic model reproducibility without provider/runtime guarantees.

42. Model retirement MUST NOT corrupt historical artifacts.

43. Replacing a retired model affects future routing only unless explicit reprocessing occurs.

44. Model execution attempts are runtime-owned.

45. Retry and Fallback MUST NOT mutate Model Descriptor.

46. Model health changes MUST NOT mutate historical AI Response provenance.

47. Cache compatibility MAY depend on exact model/version semantics.

48. Credentials MUST NOT be stored in Model Descriptor.

49. Global Model visibility MUST NOT imply cross-Workspace usability.

50. New models SHOULD be addable without AI Pipeline redesign.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* stable CRAI `modelId`,
* `providerId`,
* Model Descriptor,
* Model Deployment,
* `LOCAL` and `CLOUD` execution,
* task capabilities,
* text input/output,
* optional image input,
* Translation,
* text generation,
* language detection,
* structured output,
* streaming,
* Context limits,
* maximum output limits,
* canonical Language support metadata,
* optional LanguagePair support,
* coarse cost class,
* pricing reference,
* descriptor lifecycle,
* Deployment availability,
* basic health observations,
* provider/model provenance,
* capability validation,
* Model Catalog lookup,
* Routing requirement matching,
* local runtime adapter,
* cloud provider adapter.

MVP MAY defer:

* audio/video modalities,
* tool calling,
* parallel tool calls,
* multimodal output,
* detailed benchmark infrastructure,
* automatic capability verification,
* adaptive quality scoring,
* dynamic model discovery,
* automatic provider-region optimization,
* advanced deployment capacity,
* speculative model execution,
* model ensembles,
* fine-tuned model management,
* user-hosted remote endpoints,
* complex model-family inheritance.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact `modelId` scheme,
* whether provider model aliases receive separate CRAI model IDs,
* ModelFamily representation,
* exact ModelDescriptor schema,
* whether ModelDeployment is in AI architecture or provider-management ownership,
* whether local model runtime is modeled as Provider or Deployment Runtime,
* exact capability taxonomy,
* exact modality taxonomy,
* structured-output support levels,
* instruction-capability taxonomy,
* Context unit abstraction,
* Language capability representation,
* LanguagePair quality metadata,
* capability evidence model,
* quality benchmark architecture,
* Model Evaluation ownership,
* pricing model,
* cost-class taxonomy,
* health TTL,
* Deployment availability model,
* provider rate-limit pressure representation,
* runtime-capacity representation,
* model alias reproducibility policy,
* model-version normalization,
* parameter mapping architecture,
* exact model provenance persisted in AIResponse,
* Model Catalog persistence,
* catalog refresh/discovery,
* model retirement migration,
* exact-model pinning permissions,
* local model download/installation ownership.

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
* `../domain/PROFILE.md`
* `../domain/WORKSPACE.md`
* `../domain/TRANSLATION.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`
* `../../02-modules/translation/`
* `../../02-modules/recognition/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
