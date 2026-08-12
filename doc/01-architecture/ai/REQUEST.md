# AI Request

* **Document:** AI Architecture / Request
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the canonical provider-neutral request contract used to start one CRAI AI operation.

An `AIRequest` describes:

* what AI capability is requested,
* what semantic input should be processed,
* which immutable or explicit context applies,
* which output contract is expected,
* which execution requirements and constraints must be respected,
* how the operation is correlated and audited.

The AI Request represents **operation intent**.

It MUST NOT contain runtime execution history or provider-specific request payloads.

---

# Core Principle

Conceptually:

```text
Business Capability
        |
        v
Resolve Business Context
        |
        v
AI Request
        |
        v
AI Pipeline
        |
        v
AI Response
```

The AI Request is the boundary between:

```text
Business / Capability Intent
```

and:

```text
AI Execution Architecture
```

---

# Scope

An AI Request MAY be created by capabilities such as:

* Translation,
* Recognition,
* Language Detection,
* Character inference,
* semantic validation,
* classification,
* summarization,
* context analysis,
* optional text correction.

The Request contract MUST NOT assume that every AI operation originates from:

* Capture,
* OCR,
* Page content,
* Translation,
* user-visible UI.

---

# Non-Goals

The AI Request is NOT:

* a provider request,
* a prompt,
* a provider conversation,
* a runtime job,
* a retry attempt,
* a fallback plan,
* a queue message,
* a Session,
* a business-domain aggregate,
* a cache entry.

Those concepts may reference or derive from an AI Request.

---

# Design Principles

AI Request SHOULD be:

* provider-neutral,
* immutable during one operation,
* serializable,
* schema-versioned,
* traceable,
* capability-oriented,
* explicit,
* reproducible where required,
* safe for tenant isolation,
* extensible without provider leakage.

---

# Request Lifecycle

Recommended lifecycle:

```text
Business Intent
      |
      v
Domain / Application Resolution
      |
      v
Build AI Request
      |
      v
Request Validation
      |
      v
AI Pipeline Execution
```

The AI Request exists **before**:

* Prompt construction,
* Route Planning,
* Provider Request Adaptation,
* Model Execution.

---

# Request Structure

Recommended high-level structure:

```text
AIRequest
├── identity
├── capability
├── scope
├── input
├── contextReferences
├── configurationReferences
├── outputRequirements
├── modelRequirements
├── executionConstraints
├── costConstraints
├── privacyAndPolicyContext
├── correlation
└── extensions?
```

Not every capability requires every section.

---

# Request Identity

Recommended:

```text
AIRequestIdentity
├── requestId
├── schemaVersion
├── createdAt
└── logicalOperationId?
```

`requestId` identifies one immutable AI Request instance.

`logicalOperationId` MAY correlate several attempts or regenerated requests belonging to one higher-level business operation.

---

# Request ID vs Attempt ID

Critical distinction:

```text
requestId
    = semantic AI request identity

attemptId
    = one runtime execution attempt
```

Retry MUST NOT require mutating the Request with:

```text
retryCount
providerAttempt
```

Those belong to runtime execution state.

---

# Capability

Every AI Request MUST declare the requested AI capability.

Recommended conceptual structure:

```text
AICapabilityRequest
├── capabilityType
├── operationType?
├── capabilityVersion?
└── requirements?
```

Possible capability types MAY include:

```text
TRANSLATION
TEXT_GENERATION
CLASSIFICATION
LANGUAGE_DETECTION
SUMMARIZATION
SEMANTIC_VALIDATION
CHARACTER_INFERENCE
VISION_ANALYSIS
EMBEDDING
STRUCTURED_EXTRACTION
CUSTOM
```

The exact taxonomy is defined by AI capability/model architecture.

---

# Capability vs Business Operation

AI capability and business operation MUST remain separate.

Example:

```text
Business Operation:
    Translate TextBlock

AI Capability:
    TRANSLATION
```

or:

```text
Business Operation:
    Validate Character consistency

AI Capability:
    SEMANTIC_VALIDATION
```

The AI Pipeline does not become owner of the business operation.

---

# Scope

Every Request SHOULD carry enough ownership/correlation scope to enforce isolation and authorization.

Recommended:

```text
AIRequestScope
├── workspaceId
├── projectId?
├── sessionId?
├── principalReference?
└── businessOperationReference?
```

`workspaceId` SHOULD normally be required for Workspace-backed execution.

Local/anonymous execution MAY use another explicit isolation scope.

---

# Scope Is Not Input

Scope answers:

```text
Whose operation is this?
```

Input answers:

```text
What semantic content should the AI process?
```

These MUST remain separate.

---

# Input

`input` contains the semantic material that the requested AI capability operates on.

Recommended:

```text
AIInput
├── inputType
├── contentReference?
├── inlineContent?
├── contentRevision?
├── contentHash?
├── languageMetadata?
└── capabilityMetadata?
```

---

# Input Types

Possible canonical types include:

```text
TEXT
STRUCTURED_TEXT
TEXT_BLOCK
IMAGE
MULTIMODAL
STRUCTURED_DATA
EMBEDDING_INPUT
REFERENCE_SET
CUSTOM
```

The Request SHOULD NOT use implementation-specific input types such as:

```text
OCR_PROVIDER_BLOCK
OpenAIMessage
GeminiPart
```

---

# Referenced Input

Durable business operations SHOULD prefer immutable references when available.

Examples:

```text
TextBlock ID + Revision
Image ID + Version
Translation Revision
Content Snapshot ID
```

This improves:

* reproducibility,
* cache correctness,
* stale detection,
* auditability.

---

# Inline Input

Inline content MAY be used when:

* no durable domain resource exists,
* the operation is ephemeral,
* the content is small,
* privacy policy permits it.

Inline content MUST still have clear ownership and hashing semantics where cache/reproducibility requires them.

---

# Source Language

Source Language MAY be included when relevant.

It MUST use canonical Language values defined by the Language domain.

Provider-specific codes such as:

```text
zh_chs
en_US_PROVIDER
```

MUST NOT appear in canonical AI Request fields.

---

# Context References

Context SHOULD normally be represented through explicit immutable references.

Recommended:

```text
AIContextReferences
├── glossarySnapshotId?
├── characterContextSnapshotId?
├── sessionContextSnapshotId?
├── operationContextSnapshotId?
├── memorySnapshotReferences[]?
├── priorResultReferences[]?
└── capabilityContextReferences[]?
```

The exact set is capability-specific.

---

# Context Is Not Raw Mutable Domain State

AI Request MUST NOT rely on ambiguous references such as:

```text
current Glossary
current Character memory
current Project settings
latest Profile
```

for durable operations.

Prefer:

```text
GlossarySnapshot ID
CharacterContextSnapshot ID
exact Profile Revision
ResolvedConfigurationSnapshot ID
```

---

# Context Materialization

AI Request MAY contain references rather than all expanded context content.

Context Assembly later resolves:

```text
immutable references
        |
        v
AI Context Package
```

This avoids duplicating large context inside every Request.

---

# Session Context

`sessionId` alone MUST NOT be sufficient semantic context for a durable operation.

Session state is mutable.

If Session information materially affects output, the relevant state SHOULD be captured into:

```text
SessionContextSnapshot
```

or another immutable operation-specific snapshot.

---

# Memory Context

Memory participation MUST be explicit.

A Request MAY specify:

```text
memorySelectionReference
memorySnapshotReference
memoryPolicyReference
```

It MUST NOT instruct the AI Pipeline to read arbitrary hidden memory.

---

# Configuration References

Configuration intent SHOULD be represented through immutable/resolved references.

Recommended:

```text
AIConfigurationReferences
├── resolvedConfigurationSnapshotId?
├── resolvedProfileSnapshotIds[]?
├── policyRevisionReferences[]?
└── capabilityConfigurationReference?
```

For durable operations, `ResolvedConfigurationSnapshot` SHOULD normally be preferred when available.

---

# Profile Boundary

Request MUST NOT embed mutable Profile definitions.

Avoid:

```text
translationProfile:
    style: natural
    ...
```

when an immutable resolved configuration already exists.

Prefer:

```text
resolvedConfigurationSnapshotId
```

or exact Profile Revision references.

---

# Configuration Snapshot vs Execution Parameters

Resolved business configuration defines intent.

Concrete model parameters such as:

```text
temperature
top_p
provider-specific reasoning mode
```

MAY be derived later by:

* Route Planning,
* Model configuration mapping,
* Provider Adapter.

They SHOULD NOT appear in canonical business-level Request unless explicitly exposed as provider-neutral model requirements.

---

# Output Requirements

Every AI Request SHOULD describe the expected logical output contract.

Recommended:

```text
AIOutputRequirements
├── responseType
├── schemaReference?
├── language?
├── mappingRequirements?
├── completenessRequirements?
├── streamingAllowed?
├── partialOutputAllowed?
└── validationProfileReference?
```

---

# Response Type

Possible values MAY include:

```text
TEXT
STRUCTURED_TEXT
STRUCTURED_OBJECT
CLASSIFICATION
LANGUAGE_RESULT
MAPPING
EMBEDDING
MULTIMODAL_RESULT
CUSTOM
```

Provider response format MUST NOT define this identity.

---

# Structured Output

When structured output is required, Request SHOULD reference a provider-neutral schema.

Example:

```text
schemaReference:
    translation-block-map.v1
```

Provider adapters translate this into provider-specific schema mechanisms where supported.

---

# Output Mapping

For grouped operations, output requirements MAY declare deterministic mapping requirements.

Example:

```text
requiredKeys:
    block_1
    block_2
    block_3
```

This allows Response Validation to detect:

* missing output,
* duplicate mapping,
* unknown output keys.

---

# Model Requirements

`modelRequirements` describes capability requirements.

It MUST NOT directly select one provider/model unless the caller explicitly has authority to pin an execution resource.

Recommended:

```text
AIModelRequirements
├── requiredCapabilities[]
├── minimumContextSize?
├── multimodalRequired?
├── structuredOutputRequired?
├── streamingRequired?
├── toolCallingRequired?
├── localExecutionRequired?
├── localityPreference?
├── qualityTier?
└── modelClassConstraints?
```

---

# Preferred Model

The previous field:

```text
Preferred Model
```

SHOULD NOT be part of normal provider-neutral business Requests.

A caller MAY express:

```text
qualityTier
latencyPreference
localityPreference
capability requirement
```

Routing chooses a compatible model.

Exact model pinning MAY be allowed for:

* diagnostics,
* experiments,
* reproducibility tests,
* administrative workflows.

If supported, it MUST be explicit.

---

# Execution Constraints

Execution constraints express request-level boundaries.

Recommended:

```text
AIExecutionConstraints
├── deadline?
├── timeoutClass?
├── streamingMode?
├── cancellationReference?
├── maximumAttempts?
├── fallbackMode?
└── degradationPolicy?
```

These are constraints, not execution history.

---

# Runtime Policy Boundary

Request MAY state:

```text
maximumAttempts: 2
fallbackMode: ALLOWED
```

but MUST NOT store:

```text
retryCount: 1
providerAttempts:
    ...
```

The former is intent/constraint.

The latter is runtime history.

---

# Retry Strategy

Detailed retry algorithms SHOULD NOT be embedded in every Request.

Examples that belong to runtime policy:

* exponential backoff,
* jitter,
* retryable HTTP codes,
* provider-specific delay.

Request MAY reference:

```text
retryPolicyReference
```

or carry a simple bounded operation-level constraint.

---

# Fallback Constraint

Request MAY specify high-level fallback behavior such as:

```text
DISALLOW
ALLOW_COMPATIBLE_ROUTE
ALLOW_QUALITY_DEGRADATION
LOCAL_ONLY
```

Actual fallback selection remains owned by Route/Fallback orchestration.

---

# Deadline

A Request MAY contain an absolute deadline or deadline budget.

This is preferred over mutable per-stage timeout history.

Runtime derives stage/attempt deadlines from the operation constraint.

---

# Cost Constraints

Request MAY contain **operation-level** cost constraints.

Recommended:

```text
AICostConstraints
├── maximumEstimatedCost?
├── maximumInputUnits?
├── maximumOutputUnits?
├── maximumTotalUnits?
├── budgetPolicyReference?
└── costTier?
```

---

# Workspace Budget Boundary

The Request MUST NOT copy mutable Workspace-level fields such as:

```text
daily budget
monthly budget
current usage balance
```

Instead, Cost Control evaluates Workspace state and applicable Budget Policy.

Request MAY preserve:

```text
budgetPolicyRevisionId
```

when needed for reproducibility.

---

# Latency

Latency target SHOULD be treated as execution preference/constraint, not monetary budget.

Example:

```text
latencyClass:
    INTERACTIVE
```

or:

```text
deadline
```

rather than mixing latency into financial `Budget`.

---

# Privacy and Policy Context

The Request SHOULD carry or reference the policy context needed to prevent prohibited execution.

Recommended:

```text
AIPolicyContext
├── workspacePolicyRevisionId?
├── contentClassification?
├── externalProcessingAllowed?
├── allowedRegionConstraints[]?
├── sensitiveDataFlags[]?
└── authorizationDecisionReference?
```

Exact policy evaluation may occur in its dedicated stage.

---

# Policy Snapshot

For durable or audit-sensitive execution, exact applicable policy revision/reference SHOULD be preserved.

The Request MUST NOT depend only on:

```text
current Workspace policy
```

because it may change after execution begins.

---

# Authorization Boundary

AI Request scope MUST NOT itself grant authorization.

Authorization SHOULD already have been checked before Request creation and MAY be revalidated before external execution.

A stored Request MUST NOT be usable as a capability token.

---

# Correlation

Correlation fields support tracing and workflow linkage.

Recommended:

```text
AICorrelation
├── correlationId
├── causationId?
├── businessOperationId?
├── sessionId?
├── parentOperationId?
└── traceContextReference?
```

---

# Trace Context vs Trace History

Request MAY carry trace context.

It MUST NOT contain mutable trace history.

Correct:

```text
traceId
parentSpanId
```

Wrong:

```text
stageHistory[]
providerAttempts[]
retryCount
```

Those belong to runtime observability records.

---

# Observability Boundary

The Request may identify:

* correlation,
* ownership scope,
* business operation,
* schema version.

Telemetry owns:

* stage start/end,
* latency,
* retries,
* provider attempts,
* token usage,
* cache hit/miss,
* final cost.

---

# Immutability

Once Request Validation succeeds and execution begins, the AI Request MUST be treated as immutable.

If semantic input or configuration changes:

```text
create a new AI Request
```

Do NOT mutate the active Request.

---

# Request Revision

AI Request normally SHOULD NOT need in-place revisioning.

Each immutable semantic execution request receives a new:

```text
requestId
```

Schema version is independent from Request identity.

---

# Request Schema Version

Every Request MUST declare a schema version.

Example:

```text
ai-request.v2
```

Schema version identifies contract shape.

It MUST NOT identify:

* provider version,
* model version,
* business-domain revision.

---

# Request Validation

Validation SHOULD occur before expensive execution.

Checks MAY include:

* supported schema version,
* valid capability,
* valid Workspace scope,
* valid required input,
* resolvable immutable references,
* valid Language values,
* valid output requirements,
* valid model requirements,
* valid execution constraints,
* valid cost constraints,
* context-size feasibility,
* applicable policy presence,
* tenant isolation,
* cancellation state.

---

# Validation Layers

Request validation MAY occur in layers.

```text
Schema Validation
        |
        v
Semantic Validation
        |
        v
Reference Validation
        |
        v
Policy / Capability Validation
```

Not all capability availability checks need to occur during initial schema validation.

---

# Schema Validation

Verifies:

* required fields,
* allowed enums,
* data types,
* schema version,
* field structure.

---

# Semantic Validation

Verifies:

* capability/input compatibility,
* target Language requirements,
* output contract consistency,
* mutually compatible execution constraints.

---

# Reference Validation

Verifies referenced:

* TextBlock Revision,
* Image Version,
* Glossary Snapshot,
* Character Context Snapshot,
* Configuration Snapshot,
* policy revision.

Reference validation MUST respect Workspace scope.

---

# Capability Validation

Checks whether available runtime/provider capabilities can satisfy the Request.

A semantically valid Request MAY still fail capability resolution if no compatible route exists.

This distinction SHOULD remain visible.

---

# Provider Independence

Canonical Request fields MUST NOT contain:

* provider request object,
* provider headers,
* provider endpoint,
* provider API version,
* provider model code,
* provider retry codes,
* provider Language code,
* provider SDK types.

Those appear only after Route Planning and Provider Request Adaptation.

---

# Request Extensions

Extensions MAY be supported.

Recommended:

```text
extensions:
    namespace:
        ...
```

Requirements:

* namespace MUST be explicit,
* core semantics MUST remain stable,
* provider-specific extension MUST NOT become mandatory for core capability,
* unknown extension handling MUST be defined.

---

# Sensitive Content

Requests MAY reference sensitive source/context.

By default:

* raw content SHOULD NOT be logged,
* context SHOULD be minimized,
* secrets MUST NOT be embedded,
* provider credentials MUST NOT be serialized into Request,
* telemetry SHOULD prefer IDs/hashes.

---

# Serialization

AI Request SHOULD be serializable for:

* queueing,
* durable operation recovery,
* replay diagnostics,
* local/cloud boundary transfer.

Serialization MUST preserve semantic meaning.

Serialization format itself is infrastructure choice.

---

# Queue Boundary

A queue message MAY contain:

```text
requestId
```

or a serialized Request.

But:

```text
AI Request
    !=
Queue Message
```

Queue metadata such as:

* delivery count,
* visibility timeout,
* queue partition,

MUST remain outside canonical Request semantics.

---

# Idempotency

Request execution SHOULD support idempotency where applicable.

Possible identity material:

```text
requestId
logicalOperationId
semanticRequestHash
```

The exact strategy belongs to runtime/application architecture.

---

# Semantic Request Hash

A deterministic semantic hash MAY include:

* capability,
* input identity/hash,
* immutable context references,
* configuration references,
* output requirements,
* model requirements,
* semantic execution constraints.

It SHOULD exclude:

* trace IDs,
* creation timestamp,
* runtime attempt count.

---

# Cache Relationship

Cache identity MAY derive from semantic Request material.

However:

```text
Request ID
    !=
Cache Key
```

Two different Requests MAY be semantically cache-compatible.

One Request MAY also be intentionally non-cacheable.

---

# Retry Relationship

Retry SHOULD normally reuse the same immutable Request.

```text
AI Request
    |
    +--> Attempt 1
    +--> Attempt 2
    +--> Attempt 3
```

The Request does NOT increment `retryCount`.

Runtime attempt records do.

---

# Fallback Relationship

Fallback SHOULD normally preserve the same business Request while changing Route Plan.

```text
AI Request
       |
       +--> Route A / Attempt
       |
       +--> Route B / Attempt
```

If fallback changes semantic business intent, it is no longer merely fallback and may require a new Request.

---

# Streaming Relationship

Streaming preference/requirement MAY be declared in output/execution requirements.

Actual:

* stream ID,
* chunk sequence,
* partial buffer,

belong to execution runtime.

---

# Business Commit Boundary

Successful AI Response does not automatically equal committed business truth.

Example:

```text
AI Request
    |
    v
AI Response
    |
    v
Translation Module Validation / Commit
    |
    v
Translation Revision
```

The owning capability/domain controls durable commit.

---

# Example: Translation Request

```text
AIRequest
  identity:
    requestId: ai_req_001
    schemaVersion: ai-request.v2

  capability:
    capabilityType: TRANSLATION

  scope:
    workspaceId: ws_001
    projectId: project_001
    sessionId: session_001

  input:
    inputType: TEXT_BLOCK
    contentReference:
      textBlockId: block_100
      revision: 7

  contextReferences:
    glossarySnapshotId: glossary_snapshot_010
    characterContextSnapshotId: character_context_021

  configurationReferences:
    resolvedConfigurationSnapshotId: config_snapshot_042

  outputRequirements:
    responseType: STRUCTURED_TEXT
    language: vi

  modelRequirements:
    structuredOutputRequired: true

  executionConstraints:
    streamingMode: ALLOWED
    fallbackMode: ALLOW_COMPATIBLE_ROUTE

  costConstraints:
    costTier: STANDARD

  correlation:
    correlationId: corr_123
```

No provider/model API payload appears in this Request.

---

# Example: Language Detection Request

```text
AIRequest
  identity:
    requestId: ai_req_020
    schemaVersion: ai-request.v2

  capability:
    capabilityType: LANGUAGE_DETECTION

  scope:
    workspaceId: ws_001
    projectId: project_001

  input:
    inputType: TEXT
    inlineContent: "..."

  outputRequirements:
    responseType: LANGUAGE_RESULT

  modelRequirements:
    localExecutionRequired: false

  executionConstraints:
    streamingMode: DISABLED
```

This operation does not need:

* Glossary,
* Character context,
* Translation Profile,
* Page.

---

# Example: Vision Request

```text
AIRequest
  capability:
    capabilityType: VISION_ANALYSIS

  input:
    inputType: IMAGE
    contentReference:
      imageId: image_003
      imageVersion: 2

  outputRequirements:
    responseType: STRUCTURED_OBJECT
    schemaReference: visual-analysis.v1
```

Again, Capture is not part of the AI Request lifecycle.

---

# Failure Categories

Request-related failures MAY include:

```text
AI_REQUEST_SCHEMA_INVALID
AI_REQUEST_CAPABILITY_INVALID
AI_REQUEST_INPUT_MISSING
AI_REQUEST_INPUT_INVALID
AI_REQUEST_REFERENCE_INVALID
AI_REQUEST_SCOPE_INVALID
AI_REQUEST_CONTEXT_INVALID
AI_REQUEST_OUTPUT_CONTRACT_INVALID
AI_REQUEST_MODEL_REQUIREMENTS_INVALID
AI_REQUEST_EXECUTION_CONSTRAINT_INVALID
AI_REQUEST_COST_CONSTRAINT_INVALID
AI_REQUEST_POLICY_CONTEXT_MISSING
AI_REQUEST_CONTEXT_TOO_LARGE
AI_REQUEST_INPUT_TOO_LARGE
AI_REQUEST_CANCELLED
```

Routing/provider failures belong to later pipeline stages.

---

# Architecture Invariants

1. Every AI operation begins from a validated AI Request or equivalent versioned contract.

2. AI Request represents operation intent, not runtime history.

3. AI Request is provider-neutral.

4. AI Request is immutable after validated execution begins.

5. Changing semantic input creates a new Request.

6. Request schema version is separate from Request identity.

7. Request MUST NOT contain raw provider request payloads.

8. Request MUST NOT contain raw provider credentials.

9. Request MUST NOT contain provider response objects.

10. Request MUST NOT contain mutable provider-attempt history.

11. Request MUST NOT contain mutable Stage History.

12. Retry count is runtime state, not Request state.

13. Provider attempts are runtime state, not Request state.

14. Trace context MAY travel with Request; Trace history MUST NOT.

15. Request exists before Prompt/Input Construction.

16. Final Prompt MUST NOT be required as canonical Request input.

17. Context SHOULD use explicit immutable references where durable reproducibility matters.

18. Session ID alone MUST NOT represent durable semantic context.

19. Mutable Profile state MUST NOT be copied as historical configuration when immutable resolved configuration exists.

20. Dynamic Profile selections SHOULD resolve before durable AI execution where their values affect output.

21. Language values use canonical domain representation.

22. Provider-specific Language codes MUST NOT appear in canonical Request fields.

23. Output contract is provider-neutral.

24. Structured-output schema is provider-neutral.

25. Model Requirements express capabilities rather than normal provider/model identity.

26. Exact provider/model pinning is exceptional and explicit.

27. Execution constraints describe limits, not observed execution history.

28. Detailed retry algorithms belong to runtime/recovery policy.

29. Fallback intent is separate from fallback execution history.

30. Request-level cost constraints are separate from Workspace budget state.

31. Daily/monthly mutable budget balances MUST NOT be copied into every Request.

32. Workspace Policy affecting execution SHOULD be referenced by stable revision/context where required.

33. Request scope MUST NOT itself grant authorization.

34. Workspace/tenant isolation MUST be preserved across all referenced inputs.

35. Request MAY reference Memory only explicitly.

36. Request MUST NOT trigger arbitrary hidden Memory retrieval implicitly.

37. Queue metadata MUST NOT become canonical Request semantics.

38. Request ID is not automatically a cache key.

39. Semantic Request hash SHOULD exclude non-semantic trace/runtime fields.

40. Retry MAY reuse the same immutable Request.

41. Fallback MAY reuse the same immutable Request when semantic business intent is unchanged.

42. Streaming runtime state remains outside Request.

43. Successful AI execution does not automatically commit domain truth.

44. The calling capability owns business commit semantics.

45. Request serialization MUST preserve semantic meaning.

46. Sensitive source/context content SHOULD be excluded from ordinary logging.

47. Extensions MUST NOT introduce mandatory provider coupling into core Request semantics.

48. Not every AI Request originates from Capture or OCR.

49. Not every AI Request requires Context, Prompt-like text or Translation configuration.

50. Request validation MUST distinguish schema validity from runtime route availability.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* `requestId`,
* schema version,
* Workspace scope,
* optional Project/Session correlation,
* capability type,
* text input,
* TextBlock Revision input,
* Image input,
* canonical Language metadata,
* GlossarySnapshot reference,
* CharacterContextSnapshot reference,
* ResolvedConfigurationSnapshot reference,
* provider-neutral output type,
* optional structured schema reference,
* basic Model Requirements,
* streaming constraint,
* fallback constraint,
* operation-level cost tier/limit,
* Workspace Policy revision/reference,
* correlation ID,
* semantic Request hash,
* immutable Request serialization,
* Request Validation,
* tenant-safe reference validation.

MVP MAY defer:

* tool-calling requirements,
* multimodal compound input,
* many input sources in one Request,
* advanced extension namespaces,
* exact model pinning,
* complex degradation policies,
* detailed memory-selection contracts,
* advanced authorization-decision references,
* multiple policy-set composition,
* cross-Workspace safe cache identity,
* Request signing,
* replay authorization,
* complex offline replay.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact top-level Request schema,
* whether capability and operation type are separate enums,
* whether `workspaceId` is mandatory for local anonymous mode,
* exact content-reference union,
* whether inline source content is allowed for durable operations,
* maximum inline-content size,
* exact Context Reference structure,
* whether `OperationContextSnapshot` subsumes several other context references,
* whether `ResolvedConfigurationSnapshot` is mandatory for Translation,
* exact Model Requirements taxonomy,
* exact Quality Tier values,
* whether model pinning is permitted outside diagnostics,
* exact streaming enum,
* exact fallback enum,
* execution deadline representation,
* whether maximumAttempts belongs in Request or only Runtime Policy,
* cost-unit abstraction,
* exact Policy Context representation,
* authorization-decision retention,
* exact semantic Request hash,
* Request serialization format,
* Request schema migration strategy,
* extension namespace rules,
* sensitive inline-content handling,
* Request retention,
* Request replay semantics,
* idempotency-key relationship,
* queue serialization,
* offline Request persistence,
* whether AI Request becomes a persisted first-class runtime record.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `STAGES.md`
* `RESPONSE.md`
* `CONTEXT.md`
* `PROMPTS.md`
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
* `../domain/WORKSPACE.md`

Modules:

* `../../02-modules/translation/`
* `../../02-modules/recognition/`
* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`

Runtime:

* `../runtime/BUSINESS_PIPELINE_ORCHESTRATION.md`
* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
