# AI Response

* **Document:** AI Architecture / Response
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the canonical provider-neutral response contract returned by one CRAI AI operation.

An `AIResponse` communicates:

* normalized semantic AI output,
* output-contract status,
* warnings and structured findings,
* execution provenance,
* optional usage summary,
* correlation information.

The AI Response is the boundary between:

```text
AI Execution Architecture
        |
        v
Provider-Neutral Result
        |
        v
Calling Capability
```

The AI Response itself is NOT durable business truth.

The calling capability decides whether and how the result becomes:

* Translation Revision,
* Recognition Result,
* Language Detection Result,
* Character inference,
* Validation Finding,
* another domain/application artifact.

---

# Core Principle

```text
Provider Response
        |
        v
Provider Response Adaptation
        |
        v
Provider-Neutral Response
        |
        v
Validation
        |
        v
Normalization / Finalization
        |
        v
AI Response
        |
        v
Calling Capability
```

The AI Pipeline normalizes execution output.

The owning capability commits business truth.

---

# Scope

AI Response MAY represent results for capabilities such as:

* Translation,
* Text Generation,
* Classification,
* Language Detection,
* Semantic Validation,
* Character Inference,
* Vision Analysis,
* Structured Extraction,
* Summarization,
* Embedding.

The response contract MUST NOT assume that every result is:

* translated text,
* Page-based,
* rendered,
* user-visible,
* persisted.

---

# Non-Goals

AI Response is NOT:

* raw provider response,
* provider SDK object,
* Translation Revision,
* OCR domain record,
* Presentation artifact,
* cache entry,
* runtime attempt history,
* telemetry record,
* provider conversation,
* review decision.

---

# Design Principles

AI Response SHOULD be:

* provider-neutral,
* structured,
* schema-versioned,
* serializable,
* traceable,
* capability-oriented,
* explicit about completeness,
* immutable after finalization,
* safe for downstream consumption,
* separable from runtime telemetry.

---

# Response Lifecycle

Recommended lifecycle:

```text
Model Execution
      |
      v
Raw Provider Response / Stream
      |
      v
Provider Response Adaptation
      |
      v
Provider-Neutral Candidate
      |
      v
Response Validation
      |
      +--> Repair / Retry / Fallback if required
      |
      v
Response Normalization
      |
      v
Result Finalization
      |
      v
AI Response
```

Rendering is NOT part of this lifecycle.

---

# Response Structure

Recommended high-level structure:

```text
AIResponse
├── identity
├── requestReference
├── capability
├── result
├── resultStatus
├── findings
├── warnings
├── executionProvenance
├── usageSummary?
├── correlation
└── extensions?
```

Semantic result and execution metadata MUST remain distinguishable.

---

# Response Identity

Recommended:

```text
AIResponseIdentity
├── responseId
├── schemaVersion
├── createdAt
└── finalizationVersion?
```

`responseId` identifies one finalized AI Response.

It MUST NOT be confused with:

```text
providerResponseId
attemptId
translationRevisionId
```

---

# Request Reference

Every AI Response MUST reference the AI Request that produced it.

Recommended:

```text
requestId
```

Optional:

```text
logicalOperationId
```

This preserves request/response traceability.

---

# Capability

Response SHOULD preserve the requested capability identity.

Example:

```text
capabilityType: TRANSLATION
```

This allows the response schema to remain explicit about result semantics.

---

# Result

`result` contains the normalized semantic output.

Examples:

```text
translated text
classification
language candidates
structured extraction
summary
semantic findings
vision analysis
embedding vector
```

Provider-specific response fields MUST NOT become part of the canonical semantic result unless normalized into CRAI-defined semantics.

---

# Result Types

Possible result types MAY include:

```text
TEXT
STRUCTURED_TEXT
STRUCTURED_OBJECT
CLASSIFICATION
LANGUAGE_RESULT
MAPPING
EMBEDDING
VISION_RESULT
VALIDATION_RESULT
CUSTOM
```

The exact taxonomy SHOULD align with `REQUEST.md`.

---

# Translation Result Example

Conceptually:

```text
TranslationAIResult
├── outputs[]
│   ├── mappingKey
│   └── translatedText
├── detectedSourceLanguage?
└── semanticWarnings[]
```

It is NOT yet:

```text
TranslationRevision
```

The Translation capability validates and publishes domain state separately.

---

# Structured Result

Structured output SHOULD use provider-neutral schemas.

Example:

```text
schemaId:
    translation-block-map.v1
```

The response MAY preserve:

```text
schemaVersion
```

used for validation.

Provider-native JSON schema representation MUST NOT leak into ordinary business logic.

---

# Result Status

Result completeness and usability SHOULD be explicit.

Recommended:

```text
AIResultStatus
├── state
├── completeness?
└── validationState?
```

Possible states:

```text
COMPLETE
PARTIAL
DEGRADED
INVALID
```

A finalized successful response SHOULD normally be:

```text
COMPLETE
```

or explicitly accepted:

```text
DEGRADED
```

---

# Partial Result

Partial output MUST be explicit.

```text
PARTIAL
```

MUST NOT silently masquerade as complete output.

The calling capability decides whether partial output:

* can be displayed provisionally,
* can be retried,
* can be repaired,
* may be persisted under an explicit partial-result policy.

---

# Degraded Result

A degraded result MAY still satisfy the minimum contract.

Examples:

* fallback model used,
* optional context omitted,
* lower quality tier used,
* some optional metadata unavailable.

Degradation MUST NOT violate:

* mandatory output requirements,
* policy,
* safety,
* source/output mapping.

---

# Response Validation

Response Validation checks whether the provider-neutral candidate satisfies the AI operation contract.

Possible validation includes:

* schema correctness,
* required fields,
* expected Language,
* output mapping,
* completeness,
* output length,
* structured-output validity,
* capability-specific constraints,
* safety findings,
* terminology requirements where part of AI-level contract.

---

# Validation Boundary

Not every business rule belongs inside generic AI Response Validation.

Example:

```text
AI layer:
    structured translation mapping valid

Translation domain/module:
    source revision still current
    protected user revision not overwritten
```

Generic AI validation MUST NOT absorb domain ownership.

---

# Validation Result

Recommended:

```text
AIResponseValidation
├── status
├── findings[]
├── validatorVersion
└── validatedAt
```

Possible status:

```text
VALID
VALID_WITH_WARNINGS
INVALID
```

---

# Findings

Structured findings SHOULD be used instead of embedding diagnostic meaning in free-form strings.

Recommended:

```text
AIResponseFinding
├── code
├── severity
├── category
├── fieldReference?
├── mappingKey?
├── messageReference?
└── metadata?
```

Possible severity:

```text
INFO
WARNING
ERROR
BLOCKING
```

---

# Findings vs Business Review

AI Response findings are execution/contract findings.

They are NOT automatically:

```text
Translation Review
OCR Review
Character Review
```

The relevant business capability MAY convert appropriate findings into durable Review/Validation artifacts.

---

# Warnings

Warnings indicate non-blocking response conditions.

Examples:

```text
CONTEXT_TRUNCATED
FALLBACK_ROUTE_USED
OPTIONAL_FIELD_MISSING
LOW_CONFIDENCE
DEGRADED_QUALITY
CACHED_RESULT_USED
```

Warnings MUST be machine-readable where behavior depends on them.

---

# Warning Semantics

A warning does NOT automatically mean the response is unusable.

However, specific warning types MAY trigger capability policy such as:

* request Review,
* avoid auto-commit,
* display user warning,
* retry with stronger route.

---

# Response Normalization

Normalization converts valid provider-neutral candidates into canonical CRAI result representation.

Possible operations:

* canonical field names,
* Language normalization,
* whitespace normalization,
* confidence normalization,
* mapping normalization,
* warning normalization,
* removal of provider-only formatting artifacts.

Normalization MUST NOT silently invent missing semantic data.

---

# Post-Processing Boundary

The AI layer MAY perform **AI-response normalization**.

It SHOULD NOT automatically perform all business-specific post-processing.

Examples:

```text
remove provider wrapper
normalize JSON fields
normalize confidence scale
```

belong to AI response normalization.

But:

```text
create Translation Revision
apply Glossary domain update
render translated bubble
```

belong outside AI Response.

---

# Business Transformation

Recommended flow:

```text
AI Response
    |
    v
Calling Capability
    |
    v
Capability-Specific Validation
    |
    v
Business Transformation
    |
    v
Domain Commit
```

---

# Provider Information

Provider provenance MAY be preserved as execution metadata.

Recommended:

```text
AIExecutionProvenance
├── providerReference?
├── modelReference?
├── modelVersion?
├── region?
├── routePlanReference?
├── attemptReference?
├── fallbackUsed?
└── executionMode?
```

Provider provenance MUST NOT control downstream business semantics directly.

---

# Provider Metadata Boundary

Provider metadata is useful for:

* diagnostics,
* reproducibility,
* evaluation,
* cost analysis.

It MUST NOT be used as a substitute for semantic result fields.

Bad:

```text
if provider == X:
    parse Translation differently
```

That provider-specific handling belongs before canonical AI Response finalization.

---

# Raw Provider Response

Raw provider response MUST NOT be embedded as normal canonical `result`.

It MAY be retained separately under diagnostic/retention policy.

If retained, it SHOULD use a reference:

```text
rawProviderResponseReference
```

rather than duplicating the payload in ordinary responses.

---

# Provider Response ID

A provider response/request identifier MAY be retained for diagnostics.

Example:

```text
providerRequestId
providerResponseId
```

These are execution provenance only.

---

# Usage Summary

Usage MAY accompany finalized responses where available.

Recommended:

```text
AIUsageSummary
├── inputUnits?
├── outputUnits?
├── totalUnits?
├── unitType?
├── estimatedCost?
├── finalCost?
└── usageReference?
```

Token terminology SHOULD NOT be assumed universal for every model type.

---

# Usage vs Result

Usage is execution metadata.

```text
semantic Result
    !=
Usage
```

A calling business capability SHOULD normally be able to consume the semantic result without understanding token accounting.

---

# Retry Count

`retryCount` SHOULD NOT be a primary canonical semantic field.

If useful for diagnostics, it MAY appear in summarized execution provenance:

```text
attemptCount
fallbackCount
```

The authoritative attempt history remains runtime-owned.

---

# Attempt History

Complete structures such as:

```text
RetryHistory
ProviderAttempts
StageHistory
```

MUST NOT be embedded into the semantic response contract.

They SHOULD be referenced through observability/execution records.

---

# Diagnostics

AI Response MAY expose a bounded diagnostic summary.

Recommended:

```text
AIResponseDiagnostics
├── validationSummary?
├── routeSummary?
├── cacheSummary?
├── degradationSummary?
└── diagnosticReference?
```

Detailed telemetry remains outside the Response.

---

# Diagnostics Boundary

Response MUST NOT become a complete copy of:

* trace timeline,
* stage execution history,
* retry graph,
* provider request logs,
* worker logs.

A reference is preferred for detailed diagnostics.

---

# Correlation

Recommended:

```text
AIResponseCorrelation
├── correlationId
├── causationId?
├── traceId?
└── businessOperationId?
```

This links the response to surrounding orchestration.

---

# Trace Context vs Trace Timeline

Response MAY include:

```text
traceId
```

for correlation.

It SHOULD NOT embed:

```text
Stage Timeline
Span Tree
Full Trace
```

Those belong to observability infrastructure.

---

# Streaming

Streaming produces provisional response chunks.

Conceptually:

```text
Raw Provider Stream
        |
        v
Provider-neutral Chunks
        |
        v
Incremental Assembly
        |
        v
Final Candidate
        |
        v
Validation
        |
        v
AI Response
```

---

# Streaming Chunk

Streaming chunks SHOULD use a separate contract.

Example:

```text
AIResponseChunk
├── requestId
├── streamId
├── sequence
├── chunkType
├── content
├── provisionalMetadata?
└── isFinal?
```

`AIResponseChunk` is NOT a finalized `AIResponse`.

---

# Streaming Finalization

The finalized AI Response SHOULD remain logically equivalent regardless of whether execution was:

```text
STREAMING
```

or:

```text
NON_STREAMING
```

when effective inputs and semantic output are equivalent.

---

# Cancellation

Cancelled executions normally do NOT produce a successful finalized AI Response.

They MAY produce a terminal execution result such as:

```text
AIExecutionOutcome
    status: CANCELLED
```

This SHOULD remain distinct from a successful semantic Response.

---

# Failure Response vs Execution Failure

Not every failed execution produces `AIResponse`.

Recommended distinction:

```text
AIResponse
    = finalized semantic output

AIExecutionFailure
    = operation failed to produce acceptable semantic output
```

This prevents malformed/provider failures from masquerading as legitimate Results.

---

# Execution Failure

Possible normalized categories:

```text
INVALID_REQUEST
POLICY_DENIED
SAFETY_DENIED
ROUTING_FAILED
PROVIDER_UNAVAILABLE
PROVIDER_RATE_LIMITED
PROVIDER_TIMEOUT
MODEL_ERROR
INVALID_PROVIDER_RESPONSE
RESPONSE_VALIDATION_FAILED
RESOURCE_LIMIT
CANCELLED
INTERNAL_ERROR
```

The canonical failure model SHOULD align with `PIPELINE.md`.

---

# Repair

Invalid candidate responses MAY enter repair/recovery before finalization.

```text
Candidate
    |
    v
Validation Failed
    |
    +--> Repair
    +--> Retry
    +--> Fallback
    |
    v
Revalidate
```

An invalid candidate MUST NOT become an ordinary finalized Response merely because recovery was attempted.

---

# Retry Relationship

One immutable AI Request MAY have several attempts.

```text
AI Request
    |
    +--> Attempt 1
    +--> Attempt 2
    +--> Attempt 3
            |
            v
       AI Response
```

The final Response MAY summarize execution history.

It does not own the authoritative attempt records.

---

# Fallback Relationship

A final Response MAY indicate:

```text
fallbackUsed: true
```

and reference the selected final route.

It MUST NOT require downstream business logic to reproduce the entire fallback decision graph.

---

# Cache Relationship

A compatible cache hit MAY produce an AI Response.

Response SHOULD indicate cache provenance where operationally useful.

Example:

```text
cache:
    hit: true
    cacheEntryReference: ...
```

Cache identity remains separate from Response identity.

---

# Cached Result Validation

A cached result MUST pass applicable semantic compatibility checks.

A cache hit MUST NOT bypass:

* tenant isolation,
* current authorization,
* required output contract,
* applicable safety constraints,
* compatibility checks.

---

# Response Immutability

Once finalized, an AI Response SHOULD be immutable.

If normalization, validation or semantic output changes materially:

```text
create another response
```

or another execution result.

Do not silently mutate a finalized Response that has already been consumed.

---

# Response Schema Version

Every AI Response MUST declare its schema version.

Example:

```text
ai-response.v2
```

This version defines CRAI contract shape.

It is independent from:

* AI Request schema version,
* provider API version,
* model version,
* business-domain revision.

---

# Provider Independence

Canonical Response MUST NOT expose provider-specific response schemas as required downstream fields.

Forbidden ordinary dependencies include:

```text
OpenAI choices[]
Gemini candidates[]
Anthropic content blocks
provider finish-reason enum
provider safety object
```

Provider adapters normalize these into CRAI contracts.

---

# Finish Reason

If semantic finish state matters, normalize it.

Possible CRAI values:

```text
COMPLETED
LENGTH_LIMIT
CONTENT_RESTRICTED
TOOL_REQUIRED
CANCELLED
ERROR
UNKNOWN
```

Raw provider finish-reason strings MAY be retained separately for diagnostics.

---

# Confidence

When AI results include confidence, CRAI SHOULD normalize it into capability-defined semantics.

Provider confidence values MUST NOT be assumed directly comparable across providers unless explicit calibration exists.

---

# Language Metadata

Any Language value exposed in semantic Result MUST use canonical Language domain representation.

Provider-specific language codes MUST NOT escape into canonical Response.

---

# Safety Findings

Safety findings MAY accompany a response.

Recommended:

```text
AISafetyFinding
├── category
├── severity
├── action
└── policyReference?
```

Provider-specific safety fields SHOULD be normalized before exposure.

---

# Business Rules Boundary

Generic AI Response Validation SHOULD NOT claim ownership of every business rule.

Examples:

```text
Translation:
    protected revision rule
```

```text
Character:
    spoiler boundary
```

```text
Glossary:
    terminology authority
```

These MAY be checked by the calling capability/domain after AI Response is produced.

---

# Presentation Boundary

AI Response does NOT render itself.

Conceptually:

```text
AI Response
    |
    v
Translation / Other Capability
    |
    v
Domain Artifact
    |
    v
Presentation
```

For provisional streaming display:

```text
AI Response Chunk
    |
    v
temporary Presentation
```

still does not transfer semantic ownership to Presentation.

---

# Translation Boundary

Example:

```text
AIResponse
    capability: TRANSLATION
    result:
        ...

        |
        v

Translation Capability
    validates source compatibility
    validates protected revisions
    resolves commit rules

        |
        v

TranslationRevision
```

Therefore:

```text
AIResponse
    !=
TranslationRevision
```

---

# Recognition Boundary

Likewise:

```text
AI Vision / Recognition Response
        |
        v
Recognition Capability
        |
        v
Recognition Result / TextBlock changes
```

AI Response MUST NOT directly mutate TextBlock or Image domain state.

---

# Validation Capability Boundary

AI may produce semantic validation suggestions.

The Validation/Review capability decides whether those become durable findings.

---

# Serialization

AI Response SHOULD be serializable for:

* queue/result transport,
* local/cloud boundaries,
* diagnostics,
* execution recovery,
* testing.

Serialization format is infrastructure choice.

---

# Sensitive Content

Response payloads may contain source-derived or translated sensitive content.

Default rules:

* do not log full Result,
* do not put full Result in ordinary telemetry,
* do not duplicate raw provider response unnecessarily,
* preserve Workspace isolation,
* use references/hashes in diagnostics where possible.

---

# Retention

AI Response retention MAY differ from business artifact retention.

For example:

```text
TranslationRevision
    long-lived

AIResponse
    medium/short-lived execution artifact

RawProviderResponse
    shorter diagnostic retention
```

Historical business truth MUST NOT depend solely on retention of raw AI Response when required information has been committed to the owning domain.

---

# Example: Translation AI Response

```text
AIResponse
  identity:
    responseId: ai_res_001
    schemaVersion: ai-response.v2

  requestReference:
    requestId: ai_req_001

  capability:
    capabilityType: TRANSLATION

  result:
    resultType: STRUCTURED_TEXT
    outputs:
      - mappingKey: block_100
        text: "..."

  resultStatus:
    state: COMPLETE
    validationState: VALID

  warnings:
    []

  executionProvenance:
    providerReference: provider_a
    modelReference: model_x
    fallbackUsed: false

  usageSummary:
    inputUnits: 920
    outputUnits: 174
    unitType: TOKEN

  correlation:
    correlationId: corr_123
```

This is not yet a Translation Revision.

---

# Example: Language Detection Response

```text
AIResponse
  capability:
    capabilityType: LANGUAGE_DETECTION

  result:
    resultType: LANGUAGE_RESULT

    primary:
      language: zh-Hans
      confidence: 0.94

    alternatives:
      - language: zh-Hant
        confidence: 0.05

  resultStatus:
    state: COMPLETE
    validationState: VALID
```

No rendering stage is required.

---

# Example: Cached Response

```text
AIResponse
  result:
    ...

  resultStatus:
    state: COMPLETE

  executionProvenance:
    executionMode: CACHE
    cacheEntryReference: cache_042
```

The semantic result contract remains the same.

---

# Error Conditions

Response-specific errors MAY include:

```text
AI_RESPONSE_SCHEMA_INVALID
AI_RESPONSE_RESULT_MISSING
AI_RESPONSE_RESULT_TYPE_INVALID
AI_RESPONSE_MAPPING_INVALID
AI_RESPONSE_LANGUAGE_INVALID
AI_RESPONSE_INCOMPLETE
AI_RESPONSE_VALIDATION_FAILED
AI_RESPONSE_NORMALIZATION_FAILED
AI_RESPONSE_SAFETY_INVALID
AI_RESPONSE_PROVIDER_ADAPTATION_FAILED
AI_RESPONSE_STREAM_INCOMPLETE
```

Provider transport/model failures SHOULD use execution failure categories rather than pretending to be valid responses.

---

# Architecture Invariants

1. AI Response is provider-neutral.

2. AI Response represents finalized AI-operation output, not raw provider response.

3. Raw provider response MUST be adapted before ordinary capability use.

4. Provider-specific response schemas MUST NOT escape as required business dependencies.

5. AI Response is not durable business truth by itself.

6. The calling capability owns business commit semantics.

7. AI Response is not Translation Revision.

8. AI Response is not Recognition domain state.

9. AI Response is not Presentation artifact.

10. Rendering is NOT part of the AI Response lifecycle.

11. Response semantic Result and execution metadata MUST remain distinguishable.

12. Usage is execution metadata, not semantic Result.

13. Diagnostics are not semantic Result.

14. Provider provenance is not semantic Result.

15. Full runtime Stage History MUST NOT be embedded as canonical semantic Response state.

16. Full Retry History MUST remain runtime-owned.

17. Full Provider Attempt history MUST remain runtime-owned.

18. Response MAY summarize attempt/fallback information.

19. Trace ID MAY accompany Response; complete trace timeline belongs to observability.

20. Finalized AI Response SHOULD be immutable.

21. AI Response schema is versioned.

22. Response schema version is independent from provider/model versions.

23. Semantic result types SHOULD align with provider-neutral Request output requirements.

24. Structured outputs MUST use CRAI-defined logical schemas.

25. Provider-specific Language codes MUST NOT escape into semantic Result.

26. Provider-specific confidence semantics MUST be normalized or clearly identified.

27. Response validation occurs before finalization when required.

28. Invalid candidate output MUST NOT masquerade as valid finalized Response.

29. Repair occurs before finalization.

30. Retry and Fallback may produce one later finalized Response.

31. One Request MAY have several attempts but normally one accepted final Response.

32. Partial streaming output is provisional.

33. AIResponseChunk is distinct from finalized AIResponse.

34. Streaming and non-streaming SHOULD converge to the same logical final contract.

35. Cancellation normally produces execution outcome/failure, not successful semantic Response.

36. Cache hits MUST satisfy normal semantic compatibility rules.

37. Cache provenance MUST NOT alter semantic Result meaning.

38. Business-specific validation remains owned by the calling domain/capability.

39. Generic AI validation MUST NOT absorb all domain business rules.

40. Translation protected-revision logic remains outside AI Response.

41. Character canonical truth remains outside AI Response.

42. Glossary canonical truth remains outside AI Response.

43. Presentation MUST NOT mutate canonical AI/domain semantic output.

44. Provider provenance MAY be retained for diagnostics/reproducibility.

45. Provider provenance MUST NOT control downstream semantic branching.

46. Sensitive Result content SHOULD NOT be logged by default.

47. Raw provider payload retention MUST follow explicit policy.

48. AI Response retention MAY differ from domain artifact retention.

49. Historical domain artifacts MUST remain meaningful without requiring mutable runtime telemetry.

50. Response serialization MUST preserve semantic meaning.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* `responseId`,
* response schema version,
* originating `requestId`,
* capability type,
* provider-neutral semantic Result,
* text result,
* structured-text result,
* Language-result type,
* structured-object result,
* deterministic output mapping,
* `COMPLETE`,
* `PARTIAL`,
* `DEGRADED`,
* validation state,
* structured warnings,
* basic findings,
* provider/model provenance,
* optional usage summary,
* correlation ID,
* provider-neutral finish state,
* immutable final Response,
* separate execution failure model,
* streaming chunk contract,
* cached-response provenance,
* sensitive-data-safe diagnostics.

MVP MAY defer:

* rich confidence-calibration metadata,
* complex multimodal Result unions,
* tool-call Result contracts,
* nested agent traces,
* response alternatives/ranking,
* advanced repair lineage,
* persisted full validation reports,
* provider raw-response references,
* complex safety findings,
* cross-provider ensemble Results,
* long-term AIResponse retention.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact top-level Response schema,
* exact Result union/types,
* whether response can contain several independent Results,
* exact `COMPLETE/PARTIAL/DEGRADED` semantics,
* whether `INVALID` is a finalized Response state or only an execution failure,
* exact Validation Finding schema,
* exact Warning taxonomy,
* exact execution-provenance schema,
* whether attempt count belongs directly in Response,
* usage-unit abstraction beyond tokens,
* final-cost availability,
* provider/model provenance retention,
* finish-reason normalization,
* confidence representation,
* streaming chunk schema,
* streaming finalization rules,
* cache-provenance fields,
* raw provider-response retention,
* AIResponse persistence policy,
* response expiration,
* relationship between AIResponse and runtime execution record,
* relationship between AIResponse and operation audit record,
* whether capability-specific response schemas wrap or extend the common envelope,
* extension namespace policy.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `STAGES.md`
* `REQUEST.md`
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
* `../domain/WORKSPACE.md`

Modules:

* `../../02-modules/translation/`
* `../../02-modules/recognition/`
* `../../02-modules/presentation/`
* `../../02-modules/provider-management/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
