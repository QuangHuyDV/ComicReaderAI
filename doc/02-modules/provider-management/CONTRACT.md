# Provider Management Contracts

> **Project:** CRAI  
> **Module:** Provider Management  
> **Document:** Public Contracts  
> **Path:** `02-modules/provider-management/CONTRACT.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-04  
> **Source of Truth:** `02-modules/provider-management/MODULE.md`

---

## 1. Purpose

This document defines the public contracts of the Provider Management module.

It specifies:

- public identifiers;
- provider and model definitions;
- normalized capabilities;
- provider requirements;
- eligibility and selection contracts;
- provider leases;
- execution handles;
- credential references;
- availability and health snapshots;
- rate-limit and circuit-breaker snapshots;
- local-model resource contracts;
- provider administration commands;
- provider-selection and lease commands;
- public queries;
- normalized usage and selection metadata;
- idempotency and concurrency rules;
- contract-level invariants.

This document does not define:

- Translation request or response payloads;
- Recognition request or response payloads;
- provider-native SDK models;
- raw credentials;
- Runtime work-item schemas;
- complete lifecycle transition tables;
- integration event envelopes;
- detailed error catalogs;
- persistence tables;
- adapter implementation classes.

Those concerns belong to capability modules, Runtime, `STATES.md`, `EVENTS.md`, `ERRORS.md`, or internal infrastructure implementations.

---

## 2. Contract Boundary

Provider Management accepts capability and policy requirements and returns a bounded provider-access result.

```text
ProviderRequirement
        ↓
RequestProviderSelection
        ↓
ProviderSelectionResult
        ↓
RequestProviderLease
        ↓
ProviderLease
        ↓
ExecutionHandle
```

The module does not accept a Translation job, Recognition job, or complete semantic provider payload as its generic public input.

Capability modules retain ownership of task-specific request and response contracts.

---

## 3. Contract Design Principles

### 3.1 Capability-first

Consumers describe required capabilities and constraints.

Provider identities are optional preferences or explicit user requirements, not the default architectural coupling.

### 3.2 Provider neutrality

Public contracts must not expose provider-native types such as:

```text
OpenAIClient
GeminiModel
ClaudeMessage
DeepLRequest
QwenRuntimeHandle
```

### 3.3 Credential isolation

Raw credentials never cross the public Provider Management boundary.

### 3.4 Lease-based access

Selection does not itself grant provider access.

Provider access requires a valid `ProviderLease` and associated `ExecutionHandle`.

### 3.5 Revision safety

Provider definitions, model metadata, capabilities, policies, and credential references used by a lease are immutable snapshots or revisioned references.

### 3.6 Runtime separation

Provider Management selects and grants access.

Runtime schedules and executes work.

### 3.7 Consumer semantic ownership

Translation, Recognition, and future capability modules own task semantics, output validation, and domain authority.

### 3.8 Conservative unknown handling

Unknown capability, health, limit, or policy information must not be interpreted as support.

### 3.9 Query authority

Events notify consumers that provider state changed.

Queries return authoritative current Provider Management state.

---

## 4. Contract Ownership Matrix

| Contract | Owner |
|---|---|
| `ProviderDefinition` | Provider Management |
| `ProviderModel` | Provider Management |
| `ProviderCapabilitySnapshot` | Provider Management |
| `ProviderRequirement` | Provider Management public boundary; populated by consumer |
| `ProviderSelectionResult` | Provider Management |
| `ProviderLease` | Provider Management |
| `ExecutionHandleReference` | Provider Management |
| `ProviderCredentialReference` | Provider Management public boundary |
| raw secret value | Secret infrastructure |
| `TranslationProviderRequest` | Translation |
| `RecognitionProviderRequest` | Recognition |
| `RuntimeWorkItem` | Runtime |
| `ContentRevision` | Reading Session |
| `TranslationResult` | Translation |
| `RecognitionResult` | Recognition |

Provider Management must not redefine contracts owned by another module.

---

# Part I — Identifiers and Revisions

## 5. Identifier Rules

Identifiers must:

- be opaque to callers;
- remain stable for the represented entity lifetime;
- not encode secrets;
- not encode provider-native authorization data;
- not depend on UI ordering;
- support distributed generation where necessary;
- not be reused for unrelated entities.

Concrete string format is an implementation decision.

---

## 6. ProviderId

Identifies one logical provider integration.

```text
ProviderId
```

A provider remains identifiable across configuration revisions, model catalog changes, temporary disablement, and archival.

---

## 7. ProviderRevision

Identifies one immutable revision of a provider definition.

```text
ProviderRevision
- monotonically increasing per ProviderId
```

A new revision is required when material provider configuration changes.

---

## 8. ProviderModelId

Identifies one normalized provider model or execution target.

```text
ProviderModelId
```

It is owned by CRAI and must not be replaced by a provider-native model name.

---

## 9. ProviderModelRevision

Identifies one immutable revision of normalized model metadata.

```text
ProviderModelRevision
```

---

## 10. ProviderSelectionRequestId

Identifies one provider-selection operation.

```text
ProviderSelectionRequestId
```

Equivalent selection requests may produce different request IDs while still resolving to the same provider path.

---

## 11. ProviderSelectionResultId

Identifies one immutable provider-selection decision.

```text
ProviderSelectionResultId
```

A result records the state and policy revisions used by the decision.

---

## 12. ProviderLeaseId

Identifies one bounded provider-access lease.

```text
ProviderLeaseId
```

A lease identity must never be reused after terminal release, expiry, rejection, or revocation.

---

## 13. ExecutionHandleId

Identifies one execution-handle instance associated with a lease.

```text
ExecutionHandleId
```

The identifier is safe to expose.

The underlying client, token, process, or provider-native handle is not.

---

## 14. ProviderCredentialReferenceId

Identifies an approved credential reference without revealing secret material.

```text
ProviderCredentialReferenceId
```

---

## 15. ProviderConfigurationRevision

Identifies the configuration snapshot used during selection or lease creation.

```text
ProviderConfigurationRevision
```

---

## 16. ProviderPolicyRevision

Identifies the provider-selection policy snapshot.

```text
ProviderPolicyRevision
```

---

## 17. ProviderHealthRevision

Identifies a monotonic normalized health snapshot revision.

```text
ProviderHealthRevision
```

---

## 18. ProviderAvailabilityRevision

Identifies a monotonic availability snapshot revision.

```text
ProviderAvailabilityRevision
```

---

## 19. ProviderRateLimitRevision

Identifies a normalized rate-limit snapshot revision.

```text
ProviderRateLimitRevision
```

---

## 20. ProviderCircuitRevision

Identifies a circuit-breaker state revision.

```text
ProviderCircuitRevision
```

---

## 21. LocalModelInstanceId

Identifies one loaded or loading local-model instance.

```text
LocalModelInstanceId
```

---

## 22. External Identifiers

Provider Management may consume identifiers owned by other modules:

```text
CommandId
CorrelationId
TraceId
ReadingSessionId
TranslationJobId
TranslationBatchId
RecognitionJobId
RuntimeWorkItemId
ConsumerOperationId
```

It must not redefine ownership of those identifiers.

---

# Part II — Common Metadata

## 23. CommandMetadata

Every mutating command should include:

```text
CommandMetadata {
    commandId
    requestedAt
    requestedBy
    correlationId
    traceContext
    idempotencyKey
}
```

---

## 24. ConsumerIdentity

Identifies the module requesting provider access.

```text
ConsumerIdentity {
    moduleId
    capabilityDomain
    operationType
    operationReference
}
```

Possible `capabilityDomain` values include:

```text
TRANSLATION
RECOGNITION
GENERIC_GENERATION
LANGUAGE_DETECTION
SUMMARIZATION
EMBEDDING
SPEECH
OTHER
```

Unknown values must be handled safely.

---

## 25. TraceContext

Carries distributed tracing information.

It must not contain:

- raw credentials;
- source content;
- translated content;
- provider-native request bodies.

---

# Part III — Provider Definition Contracts

## 26. ProviderDefinition

```text
ProviderDefinition {
    providerId
    providerRevision

    displayName
    providerClass
    providerKinds[]

    executionLocality
    lifecycleStatus
    enabled

    adapterBinding
    credentialRequirement
    configurationReference

    supportedRegions[]
    defaultRegion

    policyMetadata
    healthPolicyReference
    rateLimitPolicyReference
    lifecyclePolicyReference

    modelIds[]

    createdAt
    updatedAt
    archivedAt
}
```

---

## 27. ProviderClass

Possible values:

```text
REMOTE_API
LOCAL_MODEL
OPERATING_SYSTEM_SERVICE
LOCAL_PROCESS
CUSTOM_ADAPTER
UNKNOWN
```

---

## 28. ProviderKind

Possible values:

```text
TRANSLATION_PROVIDER
RECOGNITION_PROVIDER
GENERIC_LLM_PROVIDER
LOCAL_MODEL_PROVIDER
OPERATING_SYSTEM_PROVIDER
CUSTOM_PROVIDER
```

A provider may declare multiple kinds.

Provider kinds are descriptive and do not replace capability declarations.

---

## 29. ProviderLifecycleStatus

Possible values:

```text
DRAFT
ACTIVE
DISABLED
MAINTENANCE
DRAINING
ARCHIVED
```

Detailed lifecycle transitions belong in `STATES.md`.

---

## 30. ExecutionLocality

```text
LOCAL
REMOTE
HYBRID
UNKNOWN
```

`HYBRID` may describe an adapter that can route between local and remote execution targets.

---

## 31. AdapterBinding

```text
AdapterBinding {
    adapterId
    adapterVersion
    supportedPortTypes[]
    configurationSchemaVersion
}
```

It must not expose loaded client objects.

---

## 32. CredentialRequirement

```text
CredentialRequirement {
    required
    supportedCredentialTypes[]
    credentialReferenceId
    credentialScope
}
```

Possible credential types:

```text
API_KEY
OAUTH_CLIENT
OAUTH_USER
SERVICE_ACCOUNT
SIGNED_REQUEST
LOCAL_NONE
CUSTOM
```

Raw credential values are prohibited.

---

# Part IV — Provider Model Contracts

## 33. ProviderModel

```text
ProviderModel {
    providerModelId
    providerModelRevision
    providerId

    displayName
    modelClass
    lifecycleStatus

    providerNativeModelReference

    capabilitySnapshot
    limitSnapshot
    resourceRequirement
    costMetadata
    latencyMetadata
    qualityMetadata

    supportedRegions[]
    defaultRegion

    createdAt
    updatedAt
    deprecatedAt
}
```

`providerNativeModelReference` is opaque and must not be interpreted by consumers.

---

## 34. ModelClass

Possible values:

```text
MACHINE_TRANSLATION
LARGE_LANGUAGE_MODEL
OCR
DOCUMENT_AI
MULTIMODAL_MODEL
LANGUAGE_DETECTOR
EMBEDDING_MODEL
SPEECH_MODEL
CUSTOM
UNKNOWN
```

---

## 35. ModelLifecycleStatus

Possible values:

```text
ACTIVE
DEPRECATED
DISABLED
REMOVED
UNKNOWN
```

Deprecated models should not be selected for new work unless policy explicitly permits them.

---

# Part V — Capability Contracts

## 36. ProviderCapabilityCode

Initial capability codes:

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

The set is extensible.

---

## 37. CapabilitySupportLevel

```text
SUPPORTED
SUPPORTED_WITH_LIMITS
UNSUPPORTED
UNKNOWN
```

`UNKNOWN` must not satisfy a mandatory requirement.

---

## 38. ProviderCapability

```text
ProviderCapability {
    code
    supportLevel
    limitReference
    conditions[]
    evidence
}
```

---

## 39. ProviderCapabilitySnapshot

```text
ProviderCapabilitySnapshot {
    providerId
    providerRevision
    providerModelId
    providerModelRevision

    capabilities[]
    supportedLanguages[]
    supportedLanguagePairs[]
    supportedContentTypes[]
    supportedRegions[]

    capturedAt
    expiresAt
}
```

A snapshot is immutable.

---

## 40. LanguagePair

```text
LanguagePair {
    sourceLanguage
    targetLanguage
}
```

Language tags should be BCP 47 compatible.

---

## 41. ProviderLimitSnapshot

```text
ProviderLimitSnapshot {
    maximumInputCharacters
    maximumInputTokens
    maximumOutputCharacters
    maximumOutputTokens
    maximumContextTokens
    maximumSegments
    maximumImages
    maximumImageBytes
    maximumConcurrentExecutions
    timeoutBounds
    supportedImageFormats[]
}
```

All fields are optional unless required by the capability.

Unknown limits must be treated conservatively.

---

# Part VI — Provider Requirement Contracts

## 42. ProviderRequirement

```text
ProviderRequirement {
    requiredCapabilities[]
    preferredCapabilities[]

    sourceLanguage
    targetLanguage
    languagePair

    contentType
    executionLocalityRequirement
    privacyRequirement
    regionRequirement

    minimumContextCapacity
    minimumOutputCapacity
    expectedInputSize
    expectedOutputSize

    structuredOutputRequirement
    streamingPreference
    cancellationPreference

    providerConstraint
    modelConstraint

    qualityPreference
    latencyPreference
    costPreference

    resourceRequirement
}
```

The requirement is immutable for one selection request.

---

## 43. RequiredCapability

```text
RequiredCapability {
    code
    minimumSupportLevel
    minimumLimits
}
```

A candidate that does not satisfy a required capability is ineligible.

---

## 44. ProviderConstraint

```text
ProviderConstraint {
    mode
    requiredProviderId
    preferredProviderId
    allowedProviderIds[]
    excludedProviderIds[]
    fallbackAllowed
}
```

Possible modes:

```text
AUTOMATIC
PREFERRED
REQUIRED
ALLOW_LIST
```

---

## 45. ModelConstraint

```text
ModelConstraint {
    mode
    requiredModelId
    preferredModelId
    allowedModelIds[]
    excludedModelIds[]
    allowDeprecated
}
```

---

## 46. ExecutionLocalityRequirement

```text
ANY
PREFER_LOCAL
LOCAL_REQUIRED
PREFER_REMOTE
REMOTE_REQUIRED
```

A mandatory locality requirement is a hard constraint.

---

## 47. PrivacyRequirement

```text
PrivacyRequirement {
    mode
    remoteTransmissionAllowed
    providerLoggingAllowed
    persistenceAllowed
    allowedRegions[]
    prohibitedRegions[]
    dataClassification
}
```

Possible modes:

```text
REMOTE_ALLOWED
REMOTE_WITH_RESTRICTIONS
LOCAL_PREFERRED
LOCAL_REQUIRED
NO_PERSISTENCE
NO_PROVIDER_LOGGING
REGION_RESTRICTED
```

Mandatory privacy constraints must never be downgraded silently.

---

## 48. RegionRequirement

```text
RegionRequirement {
    requiredRegion
    allowedRegions[]
    prohibitedRegions[]
}
```

---

## 49. PreferenceLevel

```text
LOW
BALANCED
HIGH
REQUIRED
```

`REQUIRED` should be used only where the corresponding contract explicitly permits promotion from preference to hard constraint.

---

## 50. ResourceRequirement

```text
ResourceRequirement {
    cpuRequired
    gpuRequired
    minimumRamBytes
    minimumVramBytes
    maximumStartupDuration
    offlineRequired
}
```

Provider Management uses resource metadata for candidate eligibility.

Runtime remains the final authority for actual resource admission.

---

# Part VII — Provider Selection Contracts

## 51. RequestProviderSelectionCommand

```text
RequestProviderSelectionCommand {
    metadata

    providerSelectionRequestId
    consumerIdentity
    requirement

    expectedPriority
    expectedExecutionDuration
    expectedRequestSize

    policyReference
    authorityContext
}
```

This command requests a selection decision.

It does not request execution.

---

## 52. ProviderAuthorityContext

```text
ProviderAuthorityContext {
    consumerModule
    operationReference
    readingSessionId
    correlationId
    expiresAt
}
```

Only relevant fields are required.

Provider Management does not determine Reading Session authority.

---

## 53. ProviderSelectionDisposition

```text
SELECTED
NO_ELIGIBLE_PROVIDER
REJECTED
REUSED_EQUIVALENT_RESULT
```

---

## 54. ProviderSelectionResult

```text
ProviderSelectionResult {
    providerSelectionResultId
    providerSelectionRequestId

    disposition

    selectedProviderId
    selectedProviderRevision
    selectedProviderModelId
    selectedProviderModelRevision

    executionMode
    selectedRegion

    capabilitySnapshot
    limitSnapshot
    healthSnapshot
    availabilitySnapshot
    rateLimitSnapshot
    circuitSnapshot

    policyRevision
    configurationRevision

    selectionReason
    fallbackRank
    scoreSummary

    eligibleCandidateSummaries[]
    rejectedCandidateSummaries[]

    selectedAt
    expiresAt
}
```

A selection result is immutable.

It does not contain raw credentials or provider clients.

---

## 55. SelectionReason

Possible normalized values:

```text
REQUIRED_PROVIDER_MATCH
PREFERRED_PROVIDER_MATCH
REQUIRED_MODEL_MATCH
LOCALITY_REQUIRED
PRIVACY_REQUIRED
REGION_REQUIRED
CAPABILITY_BEST_FIT
LOWEST_EXPECTED_LATENCY
LOWEST_EXPECTED_COST
HIGHEST_EXPECTED_QUALITY
CURRENT_PROVIDER_UNAVAILABLE
RATE_LIMIT_AVOIDANCE
CIRCUIT_BREAKER_OPEN
LOCAL_MODEL_WARM
FALLBACK_SELECTION
ONLY_ELIGIBLE_CANDIDATE
POLICY_DEFAULT
UNKNOWN
```

---

## 56. CandidateSummary

```text
ProviderCandidateSummary {
    providerId
    providerModelId
    eligible
    eligibilityReasons[]
    rejectionReasons[]
    preferenceScoreSummary
    expectedLocality
    expectedRegion
}
```

The summary must not expose proprietary scoring internals or secrets.

---

## 57. ExplainProviderSelectionQuery

```text
ExplainProviderSelectionQuery {
    providerSelectionResultId
    includeEligibleCandidates
    includeRejectedCandidates
}
```

Returns normalized reasons and constraints used by the decision.

---

# Part VIII — Provider Lease Contracts

## 58. RequestProviderLeaseCommand

```text
RequestProviderLeaseCommand {
    metadata

    providerSelectionResultId
    consumerIdentity
    operationReference

    requestedCapabilityPort
    requestedLeaseDuration
    expectedStartBefore

    runtimeAdmissionReference
    authorityContext
}
```

A selection result must still be current and compatible when a lease is requested.

---

## 59. RequestedCapabilityPort

Possible values include:

```text
TRANSLATION_PROVIDER_PORT
RECOGNITION_PROVIDER_PORT
STRUCTURED_GENERATION_PORT
LANGUAGE_DETECTION_PORT
CUSTOM_PORT
```

The selected adapter must support the requested port.

---

## 60. ProviderLease

```text
ProviderLease {
    providerLeaseId

    providerSelectionResultId
    providerId
    providerRevision
    providerModelId
    providerModelRevision

    consumerIdentity
    operationReference
    capabilityPort

    credentialReferenceId
    providerConfigurationRevision
    providerPolicyRevision

    status

    grantedAt
    activatedAt
    expiresAt
    releasedAt
    revokedAt

    executionHandleReference
    resourceReservationReference

    revocationPolicy
    metadata
}
```

---

## 61. ProviderLeaseStatus

```text
REQUESTED
GRANTED
ACTIVE
RELEASED
EXPIRED
REVOKED
REJECTED
FAILED
```

Detailed transitions belong in `STATES.md`.

---

## 62. LeaseDisposition

```text
GRANTED
REUSED_COMPATIBLE_LEASE
REJECTED
SELECTION_EXPIRED
PROVIDER_UNAVAILABLE
RESOURCE_ADMISSION_REQUIRED
```

---

## 63. RequestProviderLeaseResult

```text
RequestProviderLeaseResult {
    commandId
    providerLeaseId
    disposition
    lease
    rejectionSummary
    grantedAt
}
```

---

## 64. ActivateProviderLeaseCommand

```text
ActivateProviderLeaseCommand {
    metadata
    providerLeaseId
    runtimeWorkItemId
    activatedAt
}
```

Activation records that provider access is now being used by admitted execution work.

---

## 65. ReleaseProviderLeaseCommand

```text
ReleaseProviderLeaseCommand {
    metadata
    providerLeaseId
    reason
    usageSummary
    executionOutcomeSummary
}
```

Repeated release must be idempotent.

---

## 66. ProviderLeaseReleaseReason

```text
EXECUTION_COMPLETED
EXECUTION_FAILED
EXECUTION_CANCELLED
CONSUMER_RELEASED
SELECTION_REPLACED
APPLICATION_SHUTDOWN
LEASE_UNUSED
OTHER
```

---

## 67. RevokeProviderLeaseCommand

```text
RevokeProviderLeaseCommand {
    metadata
    providerLeaseId
    reason
    effectiveAt
}
```

Possible reasons:

```text
SECURITY_POLICY_CHANGED
PRIVACY_POLICY_CHANGED
PROVIDER_DISABLED
CREDENTIAL_REVOKED
ADAPTER_INTEGRITY_FAILED
LOCAL_RESOURCE_UNSAFE
APPLICATION_SHUTDOWN
ADMINISTRATIVE
```

---

## 68. ExecutionHandleReference

```text
ExecutionHandleReference {
    executionHandleId
    providerLeaseId
    portType
    handleVersion
    expiresAt
}
```

The reference does not expose the underlying client object.

---

## 69. ExecutionHandle Contract

The concrete handle is an in-process or infrastructure capability boundary.

Conceptually:

```text
ExecutionHandle<TCapabilityPort> {
    executionHandleId
    providerLeaseId
    providerIdentity
    modelIdentity
    capabilitySnapshot
    resolvedLimits
    locality
    region
    capabilityPort
    cancellationSupport
    usageReportingSupport
}
```

The handle must not expose raw credentials.

---

## 70. Handle Validity Rule

A handle is valid only when:

- its lease exists;
- its lease is `GRANTED` or `ACTIVE`;
- the lease is not expired;
- the lease is not revoked;
- the requested capability port matches;
- the consumer identity matches;
- required Runtime admission has been satisfied where applicable.

---

# Part IX — Credential Reference Contracts

## 71. ProviderCredentialReference

```text
ProviderCredentialReference {
    providerCredentialReferenceId
    providerId
    credentialType
    scope
    revision
    availability
    expiresAt
    rotationRequired
    metadata
}
```

`metadata` must not contain secret values.

---

## 72. CredentialAvailability

```text
UNKNOWN
AVAILABLE
UNAVAILABLE
EXPIRED
REVOKED
ROTATION_REQUIRED
MISCONFIGURED
```

---

## 73. Credential Scope

```text
APPLICATION
USER
ORGANIZATION
DEVICE
SESSION
CUSTOM
```

---

## 74. RotateCredentialReferenceCommand

```text
RotateCredentialReferenceCommand {
    metadata
    providerId
    currentCredentialReferenceId
    newCredentialReferenceId
    activeLeasePolicy
}
```

Possible active-lease policies:

```text
ALLOW_EXISTING
DRAIN_EXISTING
REVOKE_EXISTING
```

The command changes references, not raw secret content.

---

# Part X — Availability and Health Contracts

## 75. ProviderAvailabilityState

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

---

## 76. ProviderAvailabilitySnapshot

```text
ProviderAvailabilitySnapshot {
    providerId
    providerModelId
    providerAvailabilityRevision
    state
    reasons[]
    eligibleForNewLeases
    observedAt
    validUntil
}
```

---

## 77. ProviderHealthState

```text
UNKNOWN
HEALTHY
DEGRADED
UNHEALTHY
PROBING
```

---

## 78. ProviderHealthSnapshot

```text
ProviderHealthSnapshot {
    providerId
    providerModelId
    providerHealthRevision
    state

    activeProbeStatus
    recentFailureRate
    recentTimeoutRate
    recentLatency
    recentValidationFailureRate

    credentialReady
    adapterReady
    localModelReady

    observedAt
    validUntil
    evidence[]
}
```

Semantic output validation failures must only affect health when classified as provider-relevant.

---

## 79. RefreshProviderHealthCommand

```text
RefreshProviderHealthCommand {
    metadata
    providerId
    providerModelId
    probeMode
}
```

Possible probe modes:

```text
PASSIVE_ONLY
ACTIVE_LIGHTWEIGHT
ACTIVE_FULL
```

---

# Part XI — Rate-Limit and Quota Contracts

## 80. ProviderRateLimitSnapshot

```text
ProviderRateLimitSnapshot {
    providerId
    providerModelId
    providerRateLimitRevision

    requestLimit
    requestsRemaining

    tokenLimit
    tokensRemaining

    characterLimit
    charactersRemaining

    concurrentLimit
    concurrentRemaining

    quotaWindow
    resetsAt
    retryAfter

    quotaExhausted
    observedAt
}
```

All fields are optional when unavailable.

---

## 81. ProviderQuotaDimension

Possible dimensions:

```text
REQUESTS
TOKENS
CHARACTERS
IMAGES
CONCURRENT_EXECUTIONS
DAILY_QUOTA
MONTHLY_QUOTA
ACCOUNT_BALANCE
PROVIDER_CREDITS
CUSTOM
```

---

## 82. RefreshProviderRateLimitCommand

```text
RefreshProviderRateLimitCommand {
    metadata
    providerId
    providerModelId
}
```

Provider-native headers remain adapter-internal.

---

# Part XII — Circuit Breaker Contracts

## 83. ProviderCircuitState

```text
CLOSED
OPEN
HALF_OPEN
```

---

## 84. ProviderCircuitSnapshot

```text
ProviderCircuitSnapshot {
    providerId
    providerModelId
    capabilityCode
    region
    credentialReferenceId

    providerCircuitRevision
    state

    failureCount
    successCount
    openedAt
    probeAllowedAt
    lastTransitionAt
    transitionReason
}
```

Only relevant scoping fields are required.

---

## 85. ResetProviderCircuitCommand

```text
ResetProviderCircuitCommand {
    metadata
    providerId
    providerModelId
    capabilityCode
    reason
}
```

Administrative reset does not guarantee provider health.

---

# Part XIII — Local Model Contracts

## 86. LocalModelResourceRequirement

```text
LocalModelResourceRequirement {
    minimumCpuCores
    minimumRamBytes
    gpuRequired
    supportedGpuKinds[]
    minimumVramBytes
    estimatedLoadDuration
    estimatedWarmupDuration
    maximumConcurrentExecutions
    unloadSupported
}
```

---

## 87. LocalModelResidencyPolicy

```text
ALWAYS_RESIDENT
SESSION_RESIDENT
ON_DEMAND
IDLE_TIMEOUT
MANUAL
```

---

## 88. LocalModelState

```text
NOT_INSTALLED
INSTALLED
VALIDATING
READY
LOAD_REQUESTED
LOADING
WARMING
LOADED
UNLOADING
FAILED
DISABLED
```

---

## 89. LocalModelSnapshot

```text
LocalModelSnapshot {
    providerModelId
    providerModelRevision
    localModelInstanceId

    state
    residencyPolicy
    resourceRequirement
    resourceAdmissionReference

    installedVersion
    fileIntegrityStatus
    runtimeCompatibilityStatus

    loadedAt
    lastUsedAt
    idleExpiresAt
    failureSummary
}
```

---

## 90. LoadLocalModelCommand

```text
LoadLocalModelCommand {
    metadata
    providerModelId
    residencyPolicy
    resourceAdmissionReference
    expectedUseBefore
}
```

Provider Management must not load the model without required Runtime or Resource admission.

---

## 91. UnloadLocalModelCommand

```text
UnloadLocalModelCommand {
    metadata
    providerModelId
    localModelInstanceId
    reason
    force
}
```

Force unload must follow active-lease safety policy.

---

# Part XIV — Provider Administration Commands

## 92. RegisterProviderCommand

```text
RegisterProviderCommand {
    metadata
    definition
    initialModels[]
    validationMode
}
```

Possible validation modes:

```text
STRICT
BEST_EFFORT
DEFERRED
```

---

## 93. RegisterProviderResult

```text
RegisterProviderResult {
    commandId
    providerId
    providerRevision
    disposition
    validationSummary
}
```

Possible dispositions:

```text
CREATED
REUSED_EQUIVALENT
CONFLICT
REJECTED
```

---

## 94. UpdateProviderCommand

```text
UpdateProviderCommand {
    metadata
    providerId
    expectedProviderRevision
    patch
    activeLeasePolicy
}
```

Updates create a new provider revision.

---

## 95. EnableProviderCommand

```text
EnableProviderCommand {
    metadata
    providerId
    expectedProviderRevision
}
```

---

## 96. DisableProviderCommand

```text
DisableProviderCommand {
    metadata
    providerId
    reason
    activeLeasePolicy
}
```

Possible active-lease policies:

```text
ALLOW_DRAIN
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE_PERIOD
```

---

## 97. ArchiveProviderCommand

```text
ArchiveProviderCommand {
    metadata
    providerId
    reason
}
```

Archival preserves historical identity.

---

## 98. RegisterProviderModelCommand

```text
RegisterProviderModelCommand {
    metadata
    providerId
    model
}
```

---

## 99. UpdateProviderModelCommand

```text
UpdateProviderModelCommand {
    metadata
    providerModelId
    expectedProviderModelRevision
    patch
}
```

---

## 100. DeprecateProviderModelCommand

```text
DeprecateProviderModelCommand {
    metadata
    providerModelId
    reason
    effectiveAt
}
```

---

## 101. RefreshProviderCapabilitiesCommand

```text
RefreshProviderCapabilitiesCommand {
    metadata
    providerId
    providerModelId
    refreshMode
}
```

Possible modes:

```text
CONFIGURATION_ONLY
ADAPTER_DISCOVERY
REMOTE_DISCOVERY
FULL
```

---

# Part XV — Query Contracts

## 102. Query Contract Set

Recommended queries:

```text
GetProvider
ListProviders
GetProviderModel
ListProviderModels
GetProviderCapabilities
ListEligibleProviders
GetProviderAvailability
GetProviderHealth
GetProviderRateLimit
GetProviderCircuit
GetProviderLease
ListActiveProviderLeases
GetExecutionHandleMetadata
GetLocalModelState
ExplainProviderSelection
GetProviderUsageSummary
```

Queries do not mutate state.

---

## 103. GetProviderQuery

```text
GetProviderQuery {
    providerId
    providerRevision
    includeModels
    includeOperationalState
}
```

When no revision is supplied, the current revision is returned.

---

## 104. ListProvidersQuery

```text
ListProvidersQuery {
    lifecycleStatuses[]
    providerKinds[]
    executionLocalities[]
    enabledOnly
    includeArchived
}
```

---

## 105. GetProviderModelQuery

```text
GetProviderModelQuery {
    providerModelId
    providerModelRevision
    includeOperationalState
}
```

---

## 106. ListProviderModelsQuery

```text
ListProviderModelsQuery {
    providerId
    modelClasses[]
    lifecycleStatuses[]
    capabilityCodes[]
    includeDeprecated
}
```

---

## 107. GetProviderCapabilitiesQuery

```text
GetProviderCapabilitiesQuery {
    providerId
    providerModelId
    capabilityCodes[]
    includeLimits
}
```

---

## 108. ListEligibleProvidersQuery

```text
ListEligibleProvidersQuery {
    consumerIdentity
    requirement
    includeRejectionReasons
}
```

This query evaluates current eligibility but does not create a lease.

---

## 109. GetProviderAvailabilityQuery

```text
GetProviderAvailabilityQuery {
    providerId
    providerModelId
}
```

---

## 110. GetProviderHealthQuery

```text
GetProviderHealthQuery {
    providerId
    providerModelId
}
```

---

## 111. GetProviderRateLimitQuery

```text
GetProviderRateLimitQuery {
    providerId
    providerModelId
}
```

---

## 112. GetProviderCircuitQuery

```text
GetProviderCircuitQuery {
    providerId
    providerModelId
    capabilityCode
    region
}
```

---

## 113. GetProviderLeaseQuery

```text
GetProviderLeaseQuery {
    providerLeaseId
    includeHandleMetadata
}
```

---

## 114. ListActiveProviderLeasesQuery

```text
ListActiveProviderLeasesQuery {
    providerId
    providerModelId
    consumerModule
    operationReference
}
```

---

## 115. GetExecutionHandleMetadataQuery

```text
GetExecutionHandleMetadataQuery {
    executionHandleId
}
```

Returns safe metadata only.

It must not return the provider-native client or credentials.

---

## 116. GetLocalModelStateQuery

```text
GetLocalModelStateQuery {
    providerModelId
    localModelInstanceId
}
```

---

## 117. GetProviderUsageSummaryQuery

```text
GetProviderUsageSummaryQuery {
    providerId
    providerModelId
    capabilityCode
    timeRange
    consumerModule
}
```

---

# Part XVI — Usage and Outcome Feedback

## 118. ProviderUsageSummary

```text
ProviderUsageSummary {
    requestCount
    successCount
    failureCount
    cancellationCount

    inputTokens
    outputTokens
    inputCharacters
    outputCharacters
    imageCount

    providerReported
    estimated

    monetaryCost
    currency

    executionDuration
    providerLatency
}
```

Estimated and provider-reported values must remain distinguishable.

---

## 119. ReportProviderExecutionOutcomeCommand

```text
ReportProviderExecutionOutcomeCommand {
    metadata

    providerLeaseId
    executionHandleId
    runtimeWorkItemId

    providerOutcome
    providerFailureClassification
    consumerValidationOutcome

    usageSummary
    latencySummary
    rateLimitObservation

    completedAt
}
```

This command reports normalized feedback.

It must not include complete provider response content.

---

## 120. ProviderExecutionOutcome

```text
SUCCESS
PROVIDER_FAILURE
ADAPTER_FAILURE
CREDENTIAL_FAILURE
RUNTIME_FAILURE
CONSUMER_VALIDATION_FAILURE
CANCELLED
SUPERSEDED
STALE
UNKNOWN
```

Only provider-relevant outcomes may affect health and circuit state.

---

## 121. ProviderFailureClassification

```text
UNAVAILABLE
TIMEOUT
RATE_LIMITED
AUTHENTICATION_FAILED
AUTHORIZATION_FAILED
QUOTA_EXCEEDED
CONNECTION_FAILED
MALFORMED_RESPONSE
PROVIDER_INTERNAL_ERROR
MODEL_UNAVAILABLE
CONTENT_REJECTED
REQUEST_TOO_LARGE
UNKNOWN
```

---

# Part XVII — Idempotency

## 122. Provider Registration Idempotency

Equivalent registration commands may:

```text
return existing ProviderId
```

or:

```text
reject as semantic duplicate
```

The behavior must be deterministic.

---

## 123. Selection Idempotency

Repeated equivalent selection commands with the same idempotency key may return the existing compatible selection result when:

- requirements are semantically equivalent;
- provider state revisions remain compatible;
- policy revisions remain compatible;
- the selection result has not expired.

---

## 124. Lease Idempotency

Repeated equivalent lease requests must not create uncontrolled duplicate leases.

A compatible active lease may be reused only when:

- consumer identity matches;
- operation reference matches;
- provider path matches;
- capability port matches;
- lease policy permits reuse;
- lease remains valid.

---

## 125. Release Idempotency

Releasing an already released lease must remain safe and must not recreate or reactivate resources.

---

## 126. Enable and Disable Idempotency

Repeated enable or disable commands must preserve the intended state without duplicating lifecycle side effects.

---

# Part XVIII — Concurrency and Revision Rules

## 127. Optimistic Concurrency

Mutating commands should include expected revisions where concurrent updates are possible.

Conceptual operation:

```text
transition(
    entityId,
    expectedRevision,
    nextRevision,
    mutation
)
```

---

## 128. Provider Update Conflict

A provider update must fail or be retried when the expected revision no longer matches current state.

Silent overwrite is forbidden.

---

## 129. Lease Race Safety

Lease creation must atomically verify:

- selected provider revision is still allowed;
- provider remains enabled;
- capability still matches;
- credential reference remains available;
- availability permits new leases;
- circuit is not open;
- rate-limit policy permits admission;
- local resource admission remains valid where required.

---

## 130. Disable Race Safety

When provider disable and lease creation race:

```text
Disable accepted before lease commit
    → lease must not be granted

Lease committed before disable
    → active lease follows configured drain or revocation policy
```

---

## 131. Credential Rotation Race Safety

New leases must use the current credential-reference revision.

Existing leases follow the selected rotation policy.

---

## 132. Local Model Load Race Safety

Only one compatible active load operation should exist per local model instance target unless parallel instances are explicitly supported.

---

# Part XIX — Validation Rules

## 133. Provider Definition Validation

A provider definition must be rejected when:

- no adapter binding is supplied;
- provider identity conflicts with an unrelated provider;
- execution locality is inconsistent;
- credential requirement is malformed;
- provider kinds are unsupported and no custom extension policy exists;
- referenced models belong to another provider;
- region policy is contradictory.

---

## 134. Provider Model Validation

A provider model must be rejected when:

- no provider exists;
- capability metadata is structurally invalid;
- declared limits are negative or contradictory;
- local resource requirements are impossible;
- lifecycle state is invalid;
- provider-native model reference is required but absent;
- capability and locality declarations conflict.

---

## 135. Provider Requirement Validation

A requirement must be rejected when:

- no required capability is supplied for a selection operation;
- required and excluded provider IDs conflict;
- required and excluded model IDs conflict;
- local required and remote required are both set;
- privacy constraints contradict locality constraints;
- required region is prohibited;
- minimum capacity values are invalid;
- expected sizes are negative;
- required capability has unknown minimum support semantics.

---

## 136. Selection Validation

A selection result must not be produced as `SELECTED` unless:

- every hard constraint is satisfied;
- the provider is enabled;
- the model is active or explicitly permitted deprecated;
- capabilities are current enough for policy;
- credentials are available when required;
- region policy is satisfied;
- availability permits selection;
- circuit state permits selection;
- rate-limit state permits selection or policy allows bounded waiting;
- local resource constraints can be considered admissible.

---

## 137. Lease Validation

A lease must not be granted unless:

- its selection result exists;
- the selection result has not expired;
- the selected provider path remains compatible;
- consumer identity matches;
- requested capability port is supported;
- credential reference remains available;
- provider is not disabled;
- active lease policy permits another lease;
- required Runtime admission reference is valid.

---

## 138. Execution Handle Validation

A handle must be rejected or revoked when:

- its lease is terminal;
- the consumer identity does not match;
- the capability port differs;
- the provider adapter revision is incompatible;
- the handle expired;
- security or privacy policy changed incompatibly.

---

# Part XX — Privacy and Security

## 139. Prohibited Public Fields

Public contracts must never contain:

```text
apiKey
accessToken
refreshToken
clientSecret
privateKey
authorizationHeader
rawCredential
providerNativeClient
rawProviderRequest
rawProviderResponse
```

---

## 140. Credential Reference Safety

Credential references may expose:

- identity;
- type;
- provider association;
- scope;
- revision;
- availability;
- expiry metadata.

They must not expose secret material.

---

## 141. Execution Handle Isolation

An execution handle must be bound to:

- one lease;
- one consumer identity;
- one capability port;
- one provider path;
- one policy snapshot.

Cross-consumer handle reuse is forbidden unless explicitly modeled through a new lease.

---

## 142. Privacy Constraint Rule

A provider path violating mandatory privacy, locality, or region constraints must be ineligible even when it is cheaper, faster, healthier, or higher quality.

---

## 143. Untrusted Provider Metadata

Provider-discovered model names, descriptions, and metadata are untrusted data.

They must not control:

- security policy;
- credential routing;
- adapter loading;
- command execution;
- event type;
- registry mutation without validation.

---

# Part XXI — Compatibility

## 144. Translation Compatibility

Translation may depend on:

```text
ProviderRequirement
ProviderSelectionResult
ProviderLease
ExecutionHandleReference
TranslationProviderPort
ProviderCapabilitySnapshot
ProviderLimitSnapshot
ProviderUsageSummary
```

Provider Management must not require Translation to understand provider SDK models.

---

## 145. Recognition Compatibility

Recognition may depend on:

```text
ProviderRequirement
ProviderSelectionResult
ProviderLease
ExecutionHandleReference
RecognitionProviderPort
ProviderCapabilitySnapshot
ProviderResourceMetadata
```

Provider Management must not redefine Recognition result semantics.

---

## 146. Runtime Compatibility

Runtime may consume:

```text
provider resource requirements
lease identity
execution handle reference
local model load requirement
provider concurrency limits
advisory retry-after
provider availability
```

Runtime owns scheduling, work-item lifecycle, and resource admission.

---

## 147. Secret Infrastructure Compatibility

Secret infrastructure resolves secret material from `ProviderCredentialReferenceId`.

Provider Management coordinates usage but does not expose the resolved value publicly.

---

## 148. Event Compatibility

Events derived from these contracts should prefer:

- stable identifiers;
- revisions;
- compact normalized state;
- selection references;
- lease references;
- capability and availability summaries;
- no raw credentials;
- no provider-native clients.

---

# Part XXII — Core Contract Invariants

## 149. Invariant 1 — Provider neutrality

Provider SDK and native payload types never become public Provider Management contracts.

## 150. Invariant 2 — Credential isolation

Raw credentials never appear in commands, results, queries, events, warnings, or logs.

## 151. Invariant 3 — Capability-first eligibility

Mandatory capability requirements must be satisfied before preference scoring.

## 152. Invariant 4 — Hard constraints dominate

Privacy, locality, region, capability, provider, and model requirements cannot be silently downgraded.

## 153. Invariant 5 — Selection is not execution authority

A selection result does not grant provider access.

## 154. Invariant 6 — Lease required

Provider access requires a valid provider lease.

## 155. Invariant 7 — Handle bounded by lease

An execution handle cannot outlive or exceed its lease.

## 156. Invariant 8 — Runtime separation

Provider Management does not own global scheduling, queueing, or work-item execution.

## 157. Invariant 9 — Consumer semantics remain external

Translation and Recognition retain ownership of task requests, validation, and results.

## 158. Invariant 10 — Revision traceability

Every lease records the relevant provider, model, policy, configuration, and credential-reference revisions.

## 159. Invariant 11 — Disabled providers receive no new leases

Provider disablement prevents new lease grants.

## 160. Invariant 12 — Unknown is not supported

Unknown capability or limit information does not satisfy mandatory requirements.

## 161. Invariant 13 — Provider health attribution is bounded

Unrelated domain, stale-authority, cancellation, and Presentation failures do not degrade provider health.

## 162. Invariant 14 — Archived identities remain resolvable

Archival does not destroy historical provider identity.

## 163. Invariant 15 — Local model resource admission

A local model cannot load without required Runtime or Resource admission.

## 164. Invariant 16 — Queries are authoritative

Events notify; queries return current Provider Management state.

---

# Part XXIII — Initial MVP Contract Surface

## 165. Required MVP Commands

```text
RegisterProvider
UpdateProvider
EnableProvider
DisableProvider
RegisterProviderModel
UpdateProviderModel
RequestProviderSelection
RequestProviderLease
ActivateProviderLease
ReleaseProviderLease
RevokeProviderLease
RefreshProviderHealth
RefreshProviderCapabilities
ReportProviderExecutionOutcome
LoadLocalModel
UnloadLocalModel
```

---

## 166. Required MVP Queries

```text
GetProvider
ListProviders
GetProviderModel
ListProviderModels
GetProviderCapabilities
ListEligibleProviders
GetProviderAvailability
GetProviderHealth
GetProviderLease
ListActiveProviderLeases
GetExecutionHandleMetadata
GetLocalModelState
ExplainProviderSelection
```

---

## 167. Required MVP Models

```text
ProviderDefinition
ProviderModel
ProviderCapabilitySnapshot
ProviderLimitSnapshot
ProviderRequirement
ProviderSelectionResult
ProviderLease
ExecutionHandleReference
ProviderCredentialReference
ProviderAvailabilitySnapshot
ProviderHealthSnapshot
ProviderRateLimitSnapshot
ProviderCircuitSnapshot
LocalModelSnapshot
ProviderUsageSummary
```

---

## 168. Required MVP Behaviors

The contracts must support:

- one remote translation-capable provider;
- secure credential references;
- capability-first selection;
- explicit provider requirement;
- provider enable and disable;
- provider and model revisions;
- mandatory privacy and locality constraints;
- basic cost, latency, and quality preferences;
- provider lease lifecycle;
- provider-neutral Translation execution handle;
- health and availability queries;
- normalized rate-limit state;
- basic circuit-breaker state;
- optional local-provider registration;
- local-model resource metadata;
- outcome feedback;
- fallback candidate discovery;
- historical provider traceability.

---

# Part XXIV — Deferred Contract Extensions

## 169. Deferred Extensions

The following may be added later without changing core ownership:

```text
Dynamic provider plugin installation
Remote model catalog synchronization
Distributed leases
Organization policies
User-shared provider configurations
Provider SLA contracts
Billing reconciliation
Advanced quota budgets
Hedged provider selection
Speculative provider paths
Automatic model downloading
Cross-device local-model coordination
Advanced benchmark evidence
Adaptive selection learning
Regional failover routing
```

Extensions must preserve credential isolation, lease boundaries, and consumer semantic ownership.

---

# Part XXV — Example Conceptual Flows

## 170. Translation Provider Selection

```text
Translation builds ProviderRequirement
    capability = TEXT_TRANSLATION
    sourceLanguage = zh-Hans
    targetLanguage = vi
    structuredOutput = required
    locality = PREFER_LOCAL
    privacy = REMOTE_WITH_RESTRICTIONS
        ↓
RequestProviderSelection
        ↓
Provider Management evaluates candidates
        ↓
ProviderSelectionResult
    selected provider/model
    capability snapshot
    normalized limits
    selection reason
```

---

## 171. Lease and Execution

```text
ProviderSelectionResult
        ↓
RequestProviderLease
        ↓
ProviderLease GRANTED
        ↓
Runtime admits Translation work
        ↓
ActivateProviderLease
        ↓
Translation worker uses TranslationProviderPort
        ↓
ReportProviderExecutionOutcome
        ↓
ReleaseProviderLease
```

---

## 172. Provider Disabled During Active Work

```text
ProviderLease ACTIVE
        ↓
DisableProvider(ALLOW_DRAIN)
        ↓
No new leases granted
        ↓
Existing lease completes
        ↓
Lease released
```

Security-driven disable may instead revoke the lease.

---

## 173. Local Model Flow

```text
ProviderRequirement requires LOCAL_EXECUTION
        ↓
Local provider selected
        ↓
Runtime resource admission succeeds
        ↓
LoadLocalModel
        ↓
LocalModel LOADED
        ↓
ProviderLease granted
        ↓
ExecutionHandle used
```

---

## 174. Fallback Candidate Flow

```text
Provider A attempt fails
        ↓
Translation decides fallback is allowed
        ↓
Provider Management reevaluates requirement
        ↓
Provider B selected as fallbackRank = 1
        ↓
New ProviderLease
        ↓
Runtime schedules replacement execution
```

---

# Part XXVI — Related Documents

## 175. Provider Management Documents

```text
02-modules/provider-management/MODULE.md
02-modules/provider-management/STATES.md
02-modules/provider-management/EVENTS.md
02-modules/provider-management/ERRORS.md
02-modules/provider-management/README.md
```

---

## 176. Architecture References

```text
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

---

## 177. Related Module References

```text
02-modules/translation/MODULE.md
02-modules/translation/CONTRACT.md
02-modules/recognition/MODULE.md
02-modules/reading-session/MODULE.md
02-modules/presentation/MODULE.md
```

---

# 178. Summary

The Provider Management public contract is centered on these distinct concepts:

```text
ProviderDefinition
    = configured provider identity and policy

ProviderModel
    = normalized model or execution target

ProviderCapabilitySnapshot
    = explicit supported behavior and limits

ProviderRequirement
    = consumer capability need and constraints

ProviderSelectionResult
    = immutable eligible provider decision

ProviderLease
    = bounded provider-access authority

ExecutionHandle
    = lease-bound capability port
```

The primary flow is:

```text
ProviderRequirement
        ↓
RequestProviderSelection
        ↓
ProviderSelectionResult
        ↓
RequestProviderLease
        ↓
ProviderLease
        ↓
ExecutionHandle
        ↓
Capability Module Execution
        ↓
Outcome Feedback
        ↓
Lease Release
```

These contracts ensure that CRAI can:

- remain independent from provider SDKs;
- select providers by capability and policy;
- protect credentials;
- support local and remote execution;
- enforce privacy and locality constraints;
- coordinate safely with Runtime;
- expose bounded provider access;
- preserve provider and model revision history;
- normalize health, availability, rate limits, and circuits;
- serve Translation, Recognition, and future capability modules.
