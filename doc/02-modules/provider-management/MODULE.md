# Provider Management Module

> **Project:** CRAI  
> **Module:** Provider Management  
> **Document:** Module Definition  
> **Path:** `02-modules/provider-management/MODULE.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-04

---

## 1. Purpose

The Provider Management module manages the provider-side infrastructure used by CRAI capabilities such as Translation, Recognition, and future AI-assisted processing modules.

Its primary responsibility is to provide a stable, provider-neutral boundary for:

- provider registration;
- provider discovery;
- model discovery;
- capability description;
- provider eligibility;
- provider selection;
- provider availability;
- provider health;
- provider client lifecycle;
- credential resolution;
- local-model lifecycle;
- rate-limit state;
- circuit-breaker state;
- provider leasing;
- execution-handle creation;
- normalized provider metadata.

Provider Management does not own the semantic work performed by consuming modules.

It does not translate text, recognize text, summarize content, reconstruct reading order, or render results.

Its central role is:

```text
Capability Requirement
        ↓
Provider Eligibility
        ↓
Provider Selection
        ↓
Provider Lease
        ↓
Execution Handle
```

---

## 2. Architectural Position

Provider Management is a shared infrastructure-facing application module.

It is used by capability modules that require external or local execution providers.

```text
Translation ──────────────┐
Recognition ──────────────┤
Future AI Capabilities ───┤
                          ▼
                Provider Management
                          │
                          ▼
              Provider Adapters / Models
```

Provider Management sits between domain capability modules and provider-specific infrastructure.

```text
Capability Module
        ↓
Provider Requirement
        ↓
Provider Management
        ↓
Provider Adapter
        ↓
Remote API or Local Model
```

Provider Management must remain independent from the internal domain model of Translation, Recognition, or any future consumer.

---

## 3. Module Goal

The module must ensure that consuming modules can request execution capability without depending directly on:

- provider SDKs;
- provider endpoint formats;
- provider authentication mechanisms;
- raw credentials;
- provider-native model names;
- provider-native error payloads;
- client construction;
- connection pooling;
- local model loading;
- provider health implementations;
- rate-limit implementations;
- circuit-breaker implementations.

The desired consumer interaction is:

```text
Consumer expresses required capability
        ↓
Provider Management resolves eligible provider
        ↓
Provider Management grants bounded execution access
        ↓
Consumer uses a provider-neutral execution contract
```

The primary optimization target is not merely selecting the fastest provider.

The primary optimization target is:

```text
safe and policy-compatible provider access
```

This includes:

- capability compatibility;
- privacy compatibility;
- locality compatibility;
- model availability;
- credential availability;
- health status;
- rate-limit status;
- cost constraints;
- latency preference;
- quality preference;
- lifecycle safety;
- resource safety.

---

## 4. Core Architectural Principle

Provider Management follows a capability-first model.

Consumers should request:

```text
Required capability
Required language pair
Required execution mode
Required context capacity
Required privacy level
Required streaming behavior
Preferred latency
Preferred cost
Preferred quality
```

Consumers should not need to hardcode provider identities by default.

Provider-specific selection may still be explicitly requested by user policy, but the general architecture must not require provider-first coupling.

---

## 5. Responsibilities

Provider Management owns the following responsibilities.

### 5.1 Provider registry

The module maintains the logical registry of configured providers.

A provider definition may describe:

- provider identity;
- provider class;
- provider kind;
- execution locality;
- enabled state;
- supported capabilities;
- model catalog;
- adapter binding;
- credential reference requirements;
- region support;
- policy metadata;
- lifecycle policy;
- health-check policy;
- rate-limit policy.

The registry must not expose raw credential values.

### 5.2 Model registry

The module maintains normalized metadata for provider models.

A model definition may include:

- model identity;
- provider identity;
- model class;
- execution type;
- supported tasks;
- supported languages;
- supported language pairs;
- context capacity;
- output limits;
- structured-output support;
- streaming support;
- cancellation support;
- glossary support;
- deterministic-control support;
- estimated latency class;
- estimated cost class;
- local resource requirements;
- availability state.

Provider-native model identifiers may be stored internally as opaque adapter configuration.

They must not become architectural dependencies of consuming modules.

### 5.3 Capability modeling

Provider Management owns the normalized provider capability model.

Capabilities may include:

```text
TEXT_TRANSLATION
IMAGE_TEXT_RECOGNITION
DOCUMENT_RECOGNITION
STRUCTURED_OUTPUT
STREAMING
SEGMENT_BATCHING
GLOSSARY_CONSTRAINTS
LANGUAGE_DETECTION
LOCAL_EXECUTION
REMOTE_EXECUTION
CANCELLATION
TOKEN_USAGE_REPORTING
CHARACTER_USAGE_REPORTING
DETERMINISTIC_PARAMETERS
LARGE_CONTEXT
GPU_EXECUTION
OFFLINE_EXECUTION
```

Capabilities must be explicit.

The architecture must not assume that all providers support the same behavior.

### 5.4 Provider eligibility

The module determines whether a provider or model is eligible for a request.

Eligibility may consider:

- requested capability;
- language support;
- content type;
- locality requirement;
- privacy policy;
- model availability;
- provider enabled state;
- provider health;
- credential availability;
- region restrictions;
- rate-limit state;
- circuit-breaker state;
- local resource availability;
- request size;
- context requirement;
- output requirement;
- consumer constraints.

Eligibility is a hard filter.

A provider that violates mandatory constraints must not be selected.

### 5.5 Provider selection

The module selects among eligible providers and models.

Selection may consider:

- explicit provider requirement;
- explicit model requirement;
- preferred provider;
- preferred model class;
- quality preference;
- latency preference;
- cost preference;
- locality preference;
- historical health;
- recent failure rate;
- rate-limit pressure;
- local resource pressure;
- warm-model availability;
- request size;
- expected output size;
- capability fit;
- fallback position.

The exact scoring algorithm remains internal.

Public contracts express intent and constraints, not the scoring implementation.

### 5.6 Provider lease management

Provider Management grants bounded access to a selected provider through a `ProviderLease`.

A lease represents temporary authority to use:

- one provider;
- one model or execution target;
- one credential resolution path;
- one client or model instance;
- one bounded capability set;
- one policy snapshot;
- one resource allocation context.

A lease is not a translation job, recognition job, or runtime work item.

The lease exists only to control provider access and lifecycle.

### 5.7 Execution handle creation

Provider Management exposes a provider-neutral `ExecutionHandle` or capability-specific execution port.

The handle may provide access to:

- provider identity;
- model identity;
- normalized capabilities;
- resolved limits;
- locality;
- execution metadata;
- provider adapter interface;
- cancellation support;
- usage-reporting support;
- lease identity.

The handle must not expose:

- raw API keys;
- bearer tokens;
- raw authorization headers;
- secret file paths;
- provider-native client internals;
- mutable global provider state.

### 5.8 Provider client lifecycle

The module owns provider client lifecycle, including:

- client creation;
- client reuse;
- connection pooling;
- client disposal;
- idle cleanup;
- health-aware reuse;
- configuration change replacement;
- credential-rotation replacement;
- provider shutdown;
- local process lifecycle where applicable.

Consumers must not construct provider SDK clients directly.

### 5.9 Credential resolution boundary

Provider Management resolves approved credential references for provider adapters.

It may integrate with:

- operating-system secret storage;
- encrypted application configuration;
- environment-backed secrets;
- external secret managers;
- user-configured secure storage;
- provider-specific authentication services.

Raw credentials remain inside approved infrastructure boundaries.

Provider Management owns credential resolution coordination, but persistent secret storage may belong to a dedicated Secret or Configuration infrastructure component.

### 5.10 Local model lifecycle

For local providers, the module may manage:

- model registration;
- model installation metadata;
- model file validation;
- load state;
- unload state;
- warm-up state;
- runtime compatibility;
- CPU requirements;
- GPU requirements;
- memory estimates;
- residency policy;
- idle eviction;
- model version;
- model health.

The module does not own global CPU, GPU, or memory scheduling.

Resource admission remains coordinated with Runtime or Resource Management.

### 5.11 Availability and health

Provider Management maintains normalized availability and health state.

Availability answers:

```text
Can this provider currently be considered for new work?
```

Health answers:

```text
What is the current operational condition of this provider?
```

Possible signals include:

- configured;
- enabled;
- credential-ready;
- reachable;
- degraded;
- unavailable;
- rate-limited;
- circuit-open;
- loading;
- draining;
- maintenance;
- resource-constrained.

A provider may be enabled but not currently available.

### 5.12 Rate-limit state

The module may maintain normalized rate-limit metadata such as:

- requests remaining;
- tokens remaining;
- characters remaining;
- reset time;
- retry-after;
- concurrent request limit;
- provider-defined quota window;
- local concurrency capacity.

Provider-native rate-limit headers remain adapter-internal.

Normalized values may affect provider eligibility and selection.

### 5.13 Circuit breaker

Provider Management may own circuit-breaker state per:

- provider;
- model;
- capability;
- region;
- credential identity;
- execution target.

The circuit breaker prevents repeated use of a currently failing provider path.

Possible states:

```text
CLOSED
OPEN
HALF_OPEN
```

Circuit-breaker logic is operational protection.

It must not change semantic consumer requirements.

### 5.14 Provider usage metadata

The module normalizes provider execution metadata needed for:

- cost analysis;
- provider comparison;
- capacity planning;
- failure analysis;
- selection feedback;
- health calculation;
- usage reporting.

Provider Management may aggregate normalized metadata but must avoid retaining source or result content unnecessarily.

### 5.15 Fallback infrastructure

The module supports selection of alternate eligible provider paths.

Provider Management owns:

- fallback candidate discovery;
- provider compatibility checks;
- fallback ranking;
- capability compatibility;
- credential readiness;
- locality validation;
- health validation.

The consuming capability module owns the semantic decision that fallback is allowed for its current work.

Runtime owns when the replacement execution is scheduled.

---

## 6. Non-Responsibilities

Provider Management does not own the following responsibilities.

### 6.1 Translation semantics

It does not own Translation jobs, batches, attempts, profiles, context, glossary semantics, translated segments, result assembly, variants, or Translation authority.

### 6.2 Recognition semantics

It does not own OCR jobs, image preprocessing, text-region detection, reading-order inference, Recognition result assembly, confidence, or source-region alignment.

### 6.3 Runtime scheduling

It does not own global scheduling, work queues, worker allocation, retry timers, backoff scheduling, priority enforcement, cancellation propagation, generic timeout enforcement, global concurrency admission, CPU scheduling, GPU scheduling, or memory-pressure scheduling.

Provider Management provides constraints and provider resource information to Runtime.

### 6.4 Domain retry decisions

Provider Management does not decide whether a Translation or Recognition operation should be retried semantically.

It may report provider failure category, retry-after, availability, fallback candidates, and circuit-breaker state.

The consuming module decides whether the domain operation remains retryable.

Runtime enforces retry timing and attempt budgets.

### 6.5 Reading-session authority

It does not own current reading content, source revision, chapter, visible page, navigation, session lifecycle, or acceptance of Translation and Recognition results.

### 6.6 Presentation

It does not own fonts, layout, overlay geometry, panel rendering, presentation mode, visual fallback, or viewport state.

### 6.7 Persistent knowledge

It does not own glossaries, character dictionaries, terminology, aliases, translation memory, or user corrections.

### 6.8 Provider task payload semantics

Provider Management does not construct semantic Translation prompts or Recognition instructions by itself.

Capability modules own semantic task requests.

Provider adapters may convert provider-neutral execution requests into provider-specific payloads.

### 6.9 Raw secret persistence

Provider Management must not become an unstructured credential database.

Persistent secret storage belongs to an approved secure secret-management boundary.

---

## 7. Core Domain Concepts

```text
ProviderDefinition
ProviderModel
ProviderCapability
ProviderRequirement
ProviderSelectionRequest
ProviderSelectionResult
ProviderLease
ExecutionHandle
ProviderAvailability
ProviderHealth
ProviderRateLimitState
ProviderCircuitState
ProviderCredentialReference
LocalModelState
```

These concepts must not be treated as interchangeable.

---

## 8. Provider Definition

A `ProviderDefinition` represents one configured provider integration.

A provider definition may contain multiple models and execution targets.

The definition is logical configuration, not an active client instance.

---

## 9. Provider Model

A `ProviderModel` represents one normalized model or execution option exposed by a provider.

Model metadata is immutable or revisioned.

A provider may update its model catalog without changing consumer contracts.

---

## 10. Provider Capability

A `ProviderCapability` describes what a provider model can support.

Conceptual support levels:

```text
SUPPORTED
SUPPORTED_WITH_LIMITS
UNSUPPORTED
UNKNOWN
```

Capability limits may include maximum input size, context size, output size, language support, image formats, region support, cancellation behavior, and streaming behavior.

---

## 11. Provider Requirement

A `ProviderRequirement` expresses what a consumer needs.

It may include:

```text
capability
sourceLanguage
targetLanguage
contentType
executionLocality
privacyClass
minimumContextCapacity
minimumOutputCapacity
structuredOutputRequired
streamingPreference
cancellationPreference
qualityPreference
latencyPreference
costPreference
requiredProviderId
preferredProviderId
requiredModelId
allowedProviderIds
excludedProviderIds
requiredRegion
allowedRegions
```

Mandatory constraints and preferences must be distinguishable.

---

## 12. Provider Selection Request

A selection request combines:

```text
ProviderRequirement
+
Consumer policy
+
Current provider state
+
Current resource state
```

The request must include enough identity for traceability without embedding domain payload content.

---

## 13. Provider Selection Result

Conceptually:

```text
ProviderSelectionResult {
    providerId
    modelId
    executionMode
    capabilitySnapshot
    resolvedLimits
    selectionReason
    fallbackRank
    healthSnapshot
    policySnapshot
}
```

A selection result is not yet execution authority.

Execution authority is granted through a lease.

---

## 14. Provider Lease

A `ProviderLease` represents bounded temporary authority to access a selected provider path.

Conceptually:

```text
ProviderLease {
    providerLeaseId
    providerId
    modelId
    capability
    consumerModule
    operationReference
    grantedAt
    expiresAt
    status
    executionHandleReference
}
```

A lease may be granted, active, released, expired, revoked, or failed.

A lease does not guarantee successful provider execution.

---

## 15. Execution Handle

An `ExecutionHandle` is the consumer-facing provider access boundary.

The handle may expose a capability-specific port such as:

```text
TranslationProviderPort
RecognitionProviderPort
GenericStructuredGenerationPort
```

The handle must be provider-neutral, lease-bound, policy-bound, revocable, releasable, and free from raw credentials.

---

## 16. Lease Versus Runtime Work

```text
ProviderLease
    = permission and access to provider resources

RuntimeWorkItem
    = scheduled execution unit
```

Normal interaction:

```text
Capability module prepares work
        ↓
Provider Management grants lease
        ↓
Runtime schedules execution
        ↓
Worker uses execution handle
        ↓
Lease released
```

---

## 17. Provider Adapter Boundary

Each provider adapter is responsible for:

- client integration;
- authentication application;
- request serialization;
- response parsing;
- provider-native cancellation;
- provider error capture;
- rate-limit extraction;
- usage extraction;
- model metadata reporting;
- health probing;
- capability reporting.

Provider adapters must not decide Translation intent, Recognition semantics, Reading Session authority, domain retry eligibility, final provider selection policy, Presentation behavior, or glossary persistence.

---

## 18. Consumer Adapter Boundary

Capability modules may define capability-specific provider ports.

Provider Management owns adapter discovery, adapter lifecycle, model and capability lookup, lease creation, credential resolution, and execution-handle delivery.

This keeps Provider Management generic without forcing every task into one universal payload.

---

## 19. Provider Selection Policy

Selection policy must separate hard constraints from preferences.

Hard constraints may include required locality, region, provider, model, capability, structured output, language support, and credential availability.

Preferences may include low cost, low latency, high quality, local execution, warm model, and deterministic behavior.

Preferences never override hard constraints.

---

## 20. Explicit Provider Selection

When a required provider or model cannot be satisfied:

- Provider Management must not silently select another provider;
- selection must fail;
- fallback remains disabled unless caller policy explicitly permits it.

---

## 21. Local and Remote Providers

Provider Management supports both:

```text
LOCAL_PROVIDER
REMOTE_PROVIDER
```

Remote providers may require credentials, region policy, network availability, rate limits, quota, and remote privacy approval.

Local providers may require model installation, file validation, CPU/GPU compatibility, memory availability, model loading, runtime availability, and local concurrency capacity.

---

## 22. Provider Availability

Possible availability states:

```text
UNKNOWN
AVAILABLE
DEGRADED
UNAVAILABLE
DISABLED
MAINTENANCE
LOADING
DRAINING
RATE_LIMITED
CIRCUIT_OPEN
RESOURCE_CONSTRAINED
CREDENTIAL_UNAVAILABLE
```

Availability provides a normalized eligibility summary.

---

## 23. Provider Health

Provider health may be derived from active probes, passive execution outcomes, connection failures, timeout rate, malformed-response rate, latency, local-model status, credential validity, and rate-limit pressure.

Health must not be inferred from one unrelated semantic failure.

---

## 24. Rate Limits and Quotas

Provider Management normalizes rate-limit and quota dimensions such as requests, tokens, characters, images, concurrent executions, daily quota, monthly quota, balance, and provider credits.

Rate-limit state may exclude a provider temporarily, lower ranking, set advisory retry-after, trigger fallback selection, prevent new leases, or allow existing leases to drain.

---

## 25. Circuit Breaker

```text
CLOSED
    ↓ repeated eligible failures
OPEN
    ↓ cooldown or probe
HALF_OPEN
    ↓ success
CLOSED
```

The exact thresholds belong to operational policy.

---

## 26. Provider Lease Lifecycle

```text
REQUESTED
    ↓
GRANTED
    ↓
ACTIVE
    ↓
RELEASED
```

Alternative terminal paths:

```text
REQUESTED → REJECTED
GRANTED → EXPIRED
GRANTED → REVOKED
ACTIVE → EXPIRED
ACTIVE → REVOKED
```

Detailed state contracts belong in `STATES.md`.

---

## 27. Lease Expiration

A lease may expire because execution never begins, lifetime is exceeded, provider configuration changes, credentials rotate, provider is disabled, local model shuts down, resource pressure forces revocation, consumer fails to release, or application shutdown begins.

Lease expiration prevents new operations through the handle but does not necessarily terminate an already accepted remote request.

---

## 28. Lease Revocation

Provider Management may revoke a lease when security policy changes, provider is disabled, credentials are revoked, provider path becomes prohibited, local resource becomes unsafe, adapter integrity fails, or application shutdown begins.

The consuming module and Runtime still own the corresponding domain and work-item state transitions.

---

## 29. Credential Model

Public contracts use `ProviderCredentialReference`, never raw secret fields.

Credential references may contain identity, type, provider association, user/application scope, revision, and availability state.

Actual secret values remain inaccessible to consumers.

---

## 30. Credential Rotation

Credential rotation may allow an existing lease to continue, revoke it, direct new leases to the new revision, replace client pools, and trigger health reevaluation.

Provider Management records enough revision information for traceability.

---

## 31. Provider Configuration

Provider configuration may include enabled state, endpoint, locality, model catalog, credential reference, region, timeout defaults, health-check policy, rate-limit policy, local model path, residency policy, cost metadata, privacy classification, user visibility, and experimental flag.

Configuration snapshots used for selection and lease creation must be immutable or revisioned.

---

## 32. Configuration Change Safety

A configuration change must not silently mutate the meaning of an active lease.

Possible responses include allowing drain, marking lease draining, revoking lease, creating a new configuration revision, rebuilding client pools, reloading local models, or preventing new leases until validation succeeds.

---

## 33. Local Model Residency

```text
ALWAYS_RESIDENT
SESSION_RESIDENT
ON_DEMAND
IDLE_TIMEOUT
MANUAL
```

Provider Management owns residency policy and local-model lifecycle.

Runtime or Resource Management owns admission under actual CPU, GPU, and memory constraints.

---

## 34. Provider Resource Metadata

Provider Management exposes normalized metadata such as estimated RAM/VRAM, CPU/GPU requirements, startup and warm-up duration, maximum concurrency, per-execution memory, unload support, and device compatibility.

Runtime uses this information for resource-aware admission.

---

## 35. Provider Selection and Runtime

```text
Provider Management decides:
    which provider path is eligible and preferred

Runtime decides:
    when work executes and whether resources are admitted
```

Provider Management owns candidate discovery, capability matching, policy validation, ranking, lease creation, and execution-handle delivery.

Runtime owns queue admission, priority scheduling, worker assignment, resource admission, execution timeout, cancellation propagation, retry timing, backpressure, and execution terminal outcome.

---

## 36. Provider Selection and Translation

Translation supplies translation-specific requirements such as language pair, profile requirements, context capacity, structured output, glossary support, streaming preference, locality, privacy, cost, latency, and quality.

Provider Management returns a compatible provider path.

Translation still owns batch content, semantic request, output validation, alignment, retry eligibility, fallback permission, and result assembly.

---

## 37. Provider Selection and Recognition

Recognition may supply image-recognition capability, image formats, language hints, vertical-text support, layout support, local preference, GPU availability, confidence support, and geometry support.

Recognition owns image preprocessing, request semantics, region and text contracts, output normalization, reading-order behavior, and authority.

---

## 38. Provider Selection and Future Modules

Future consumers may include summarization, language detection, classification, quality evaluation, speech recognition, speech synthesis, semantic search, embeddings, and correction suggestions.

Provider Management must not hardcode Translation as its primary domain.

---

## 39. Provider Result Feedback

Consumers or Runtime may report normalized outcomes such as success, timeout, rate limit, unavailable, authentication failure, malformed response, validation failure, cancellation, stale result, latency, and usage.

Provider Management uses only provider-relevant feedback for health, rate-limit, circuit-breaker, and selection metrics.

---

## 40. Failure Attribution

Failures must distinguish:

```text
Provider failure
Adapter failure
Credential failure
Runtime failure
Consumer semantic validation failure
Source-content failure
Policy rejection
```

Provider health must not degrade because source content was invalid, Translation alignment failed, Reading Session rejected stale output, Presentation failed, or the user cancelled normally.

---

## 41. Cache Interaction

Provider Management may cache provider definitions, capability snapshots, model catalogs, health snapshots, rate-limit state, client instances, local-model instances, and credential-resolution metadata.

It does not own Translation or Recognition result caches.

---

## 42. Persistence

Persistent storage may be used for provider definitions, models, capabilities, configuration revisions, health history, usage history, lease metadata, local-model installation state, circuit state, and administrative enablement state.

Ephemeral state may include active clients, model process handles, credential values, connection pools, probes, and active rate-limit windows.

Raw credentials must not be stored in ordinary Provider Management persistence.

---

## 43. Security

The module must ensure:

- credentials never appear in public contracts, events, logs, or selection results;
- execution handles cannot access unrelated capabilities;
- one consumer cannot use another consumer's lease;
- provider-native responses cannot mutate registry state;
- untrusted metadata cannot alter security policy;
- remote execution follows privacy and region policy;
- local model files are validated before loading;
- adapter plugins are explicitly trusted and registered.

---

## 44. Privacy

Possible privacy modes:

```text
REMOTE_ALLOWED
REMOTE_WITH_RESTRICTIONS
LOCAL_PREFERRED
LOCAL_REQUIRED
NO_PERSISTENCE
NO_PROVIDER_LOGGING
REGION_RESTRICTED
```

Mandatory privacy requirements are hard constraints and must never be downgraded silently.

---

## 45. Observability

Recommended metrics include provider counts, availability, degraded state, selection count/failure, leases granted/rejected/expired/revoked, latency, failure rate, rate-limit occurrence, circuit transitions, credential failures, local-model load duration, active clients/models, usage, and estimated cost.

Observability must avoid content leakage.

---

## 46. Event Interaction

Provider Management may publish events such as:

- ProviderRegistered;
- ProviderUpdated;
- ProviderEnabled;
- ProviderDisabled;
- ProviderAvailabilityChanged;
- ProviderHealthChanged;
- ProviderModelRegistered;
- ProviderModelUpdated;
- ProviderCapabilityChanged;
- ProviderLeaseGranted;
- ProviderLeaseReleased;
- ProviderLeaseExpired;
- ProviderLeaseRevoked;
- ProviderRateLimitChanged;
- ProviderCircuitOpened;
- ProviderCircuitHalfOpened;
- ProviderCircuitClosed;
- LocalModelLoadStarted;
- LocalModelLoaded;
- LocalModelLoadFailed;
- LocalModelUnloaded;
- ProviderCredentialAvailabilityChanged.

Exact event contracts belong in `EVENTS.md`.

---

## 47. Command Interaction

Expected command categories include register/update/enable/disable provider, register/update model, request selection, request/release/revoke lease, refresh capabilities/health, reset circuit, load/unload local model, and rotate credential reference.

Exact public contracts belong in `CONTRACT.md`.

---

## 48. Query Interaction

Expected queries include get/list providers, eligible providers, models, capabilities, availability, health, rate-limit state, circuit state, active leases, local-model state, selection explanation, and usage summaries.

Queries must not expose credentials or provider-native client objects.

---

## 49. State Ownership

Provider Management owns state for:

```text
ProviderDefinition
ProviderModel
ProviderAvailability
ProviderHealth
ProviderLease
ProviderCircuit
ProviderRateLimitState
LocalModelLifecycle
ProviderClientLifecycle
CredentialAvailabilityReference
```

It does not own state for TranslationJob, RecognitionJob, RuntimeWorkItem, ReadingSession, Presentation, KnowledgeSnapshot, TranslationResult, or RecognitionResult.

---

## 50. Initial Provider Categories

```text
TRANSLATION_PROVIDER
RECOGNITION_PROVIDER
GENERIC_LLM_PROVIDER
LOCAL_MODEL_PROVIDER
OPERATING_SYSTEM_PROVIDER
CUSTOM_PROVIDER
```

A single provider may support more than one category.

Provider category does not replace explicit capability definitions.

---

## 51. Provider Identity

Conceptual identifiers include:

```text
ProviderId
ProviderRevision
ProviderModelId
ProviderModelRevision
ProviderLeaseId
ProviderClientInstanceId
LocalModelInstanceId
ProviderCredentialReferenceId
ProviderConfigurationRevision
```

Provider-native IDs may be recorded separately but must not replace CRAI identities.

---

## 52. Revision Model

Provider Management uses revisioned immutable snapshots for provider definitions, model metadata, capability declarations, selection policy, credential references, provider configuration, and health observations when historical traceability is required.

An active lease records the relevant revisions that influenced selection and access.

---

## 53. Idempotency

Repeated equivalent commands must not create uncontrolled duplicates.

Equivalent lease requests may return an existing compatible lease. Repeated release and disable commands remain safely idempotent.

---

## 54. Concurrency

The module must safely handle concurrent provider updates, model refresh, lease requests/releases, credential rotation, health updates, rate-limit updates, circuit transitions, model load/unload races, and provider disable during active execution.

State transitions must use optimistic concurrency or equivalent protection.

---

## 55. Provider Disable Semantics

Disabling a provider prevents new selection and leases.

Active-lease policy:

```text
ALLOW_DRAIN
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE_PERIOD
```

Default should normally be `ALLOW_DRAIN`, except for security, privacy, or credential revocation.

---

## 56. Provider Removal Semantics

Recommended lifecycle:

```text
Provider enabled
    ↓
Provider disabled
    ↓
Provider archived
```

Hard deletion should be rare.

Historical records may continue referencing archived provider identities.

---

## 57. Model Deprecation

```text
ACTIVE
DEPRECATED
DISABLED
REMOVED
```

Deprecated models remain identifiable but should not be selected for new work unless explicitly permitted.

---

## 58. Provider Fallback Boundary

Provider Management may return fallback candidates.

Translation or Recognition decides whether fallback is semantically allowed. Runtime schedules replacement execution. Provider Management validates and leases the fallback path.

---

## 59. Provider Choice Explainability

Possible normalized reasons:

```text
REQUIRED_PROVIDER_MATCH
PREFERRED_PROVIDER_MATCH
LOCALITY_REQUIRED
PRIVACY_REQUIRED
CAPABILITY_BEST_FIT
LOWEST_EXPECTED_LATENCY
LOWEST_EXPECTED_COST
HIGHEST_EXPECTED_QUALITY
CURRENT_PROVIDER_UNAVAILABLE
RATE_LIMIT_AVOIDANCE
CIRCUIT_BREAKER_OPEN
LOCAL_MODEL_WARM
FALLBACK_SELECTION
```

Explainability must not reveal secrets or sensitive scoring internals.

---

## 60. Cost Metadata

Provider Management may maintain normalized cost per request, token, character, image, or execution second, plus subscription/quota class and unknown-cost state.

Estimated and provider-reported values must remain distinguishable.

---

## 61. Latency Metadata

Latency metadata may include connection latency, startup, warm-up, execution, percentiles, expected latency class, time-to-first-result, and completion time.

Estimates influence selection but do not guarantee actual time.

---

## 62. Quality Metadata

Quality metadata may include manually assigned tier, benchmark score, language-pair score, task-specific score, validation success rate, user preference, and unknown quality.

Capability modules remain responsible for semantic output validation.

---

## 63. Administrative Control

Administrative actions may enable, disable, archive, refresh health/catalog, reset circuit, invalidate capability metadata, revoke leases, unload models, rotate credential references, or place providers in maintenance.

Administrative actions must be auditable.

---

## 64. Initial MVP Scope

The MVP should support provider and model registry, explicit capabilities, enable/disable, local and remote definitions, credential references, selection, mandatory constraints, basic ranking, provider lease, execution handle, availability, health, rate-limit state, basic circuit breaker, local-model state metadata, adapter registry, normalized errors, lifecycle events, usage metadata, Translation integration, and Recognition integration boundary.

At least one remote Translation-capable provider and one provider-neutral Translation execution port should be supported.

---

## 65. Deferred Capabilities

Deferred capabilities include automatic remote catalogs, plugin marketplace, advanced benchmarking, automatic quality scoring, adaptive optimization, hedged execution, distributed leases, cross-device model management, automatic model download, GPU placement, quota budgeting, organization policy, billing reconciliation, regional routing, SLA enforcement, and automatic credential provisioning.

---

## 66. Core Invariants

1. Capability modules do not depend directly on provider SDK types.
2. Raw credentials never cross the Provider Management boundary.
3. Selection never violates mandatory privacy, locality, region, or capability constraints.
4. A provider lease is not a domain job and is not a Runtime work item.
5. Consumers cannot use an execution handle after its lease becomes invalid.
6. Disabling a provider prevents new leases.
7. Provider-native errors are normalized before leaving adapters.
8. Provider health is not degraded by unrelated domain or Presentation failures.
9. Provider Management chooses eligible provider paths; Runtime chooses execution timing.
10. Translation, Recognition, and future consumers retain semantic request and result ownership.
11. Capability declarations without explicit limits are treated conservatively.
12. Configuration changes do not rewrite historical lease or execution metadata.
13. Local model loading requires successful Runtime or Resource admission.
14. Provider output is untrusted data and cannot mutate registry or security policy.
15. Provider archival does not destroy historical identity.
16. Selection explanations remain normalized and free from secrets.

---

## 67. Key Architectural Decisions

1. Provider Management is shared across Translation, Recognition, and future capabilities.
2. Selection is capability-first.
3. Provider-specific payloads remain inside adapters.
4. Public contracts use credential references only.
5. Provider access is lease-based.
6. Runtime owns execution scheduling.
7. Capability modules own semantics.
8. Local and remote providers share one selection architecture.
9. Health and eligibility remain distinct.
10. Historical provider identities remain stable through revisions.

---

## 68. Open Decisions

### Contract decisions

- exact command and query names;
- exact lease request contract;
- exact execution-handle contract;
- capability representation;
- mandatory-versus-preference representation;
- selection explanation contract;
- credential-reference contract;
- local-model resource contract;
- provider usage contract.

### State decisions

- exact ProviderDefinition, ProviderModel, ProviderLease, LocalModel, ProviderHealth, Availability, and Circuit states;
- active-lease behavior when disabling a provider.

### Event decisions

- public versus internal lease events;
- health/rate-limit throttling;
- local-model event granularity;
- credential availability privacy;
- provider-selection event visibility.

### Error decisions

- selection, lease, credential, local-model, capability, configuration, circuit, and client-lifecycle errors.

### Policy decisions

- default ranking;
- health and circuit thresholds;
- lease duration;
- disable drain policy;
- local-model residency;
- health-check and capability-refresh intervals.

---

## 69. Documentation Order

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
EVENTS.md
    ↓
ERRORS.md
    ↓
README.md
```

---

## 70. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Runtime references:

```text
docs/architecture/runtime/PIPELINE_RUNTIME.md
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/MEMORY_MODEL.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/RUNTIME_CONFIG.md
docs/architecture/runtime/RUNTIME_COMPONENTS.md
```

Related modules:

```text
02-modules/translation/
02-modules/recognition/
02-modules/reading-session/
02-modules/presentation/
```

Future Provider Management documents:

```text
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/STATES.md
02-modules/provider-management/EVENTS.md
02-modules/provider-management/ERRORS.md
02-modules/provider-management/README.md
```

---

## 71. Summary

Provider Management is the shared CRAI module responsible for selecting, preparing, leasing, and maintaining provider execution paths.

```text
ProviderRequirement
        ↓
Provider Eligibility
        ↓
Provider Selection
        ↓
ProviderLease
        ↓
ExecutionHandle
        ↓
Capability Module Execution
```

The module is capability-first, provider-neutral, credential-safe, lease-based, health-aware, rate-limit-aware, circuit-breaker-aware, local-and-remote capable, Runtime-compatible, and reusable across capability modules.

It deliberately excludes Translation and Recognition semantics, Runtime scheduling, Reading Session authority, Presentation, Knowledge persistence, domain result assembly, and direct credential exposure.

This document is the architectural source of truth for all subsequent Provider Management contracts, states, events, errors, and implementation documentation.
