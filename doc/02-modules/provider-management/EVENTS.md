# Provider Management Events

> **Project:** CRAI  
> **Module:** Provider Management  
> **Document:** Integration Events  
> **Path:** `02-modules/provider-management/EVENTS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-04  
> **Source of Truth:**
>
> - `02-modules/provider-management/MODULE.md`
> - `02-modules/provider-management/CONTRACT.md`
> - `02-modules/provider-management/STATES.md`

---

## 1. Purpose

This document defines the integration events published and consumed by the Provider Management module.

The events communicate facts that have already occurred, including:

- provider registration and configuration changes;
- provider enablement, disablement, and archival;
- provider model lifecycle;
- provider capability changes;
- provider selection outcomes;
- provider lease lifecycle;
- provider availability changes;
- provider health changes;
- rate-limit and quota changes;
- circuit-breaker transitions;
- credential availability changes;
- provider client lifecycle;
- local model installation, loading, unloading, and failure;
- normalized provider usage and outcome feedback;
- administrative provider controls.

These events allow Translation, Recognition, Runtime, Observability, Administration, and future capability modules to react without directly coupling themselves to Provider Management internals.

This document does not define:

- commands;
- query contracts;
- provider-native callbacks;
- provider SDK events;
- detailed state transition tables;
- detailed error catalogs;
- persistence tables;
- Runtime worker events;
- Translation domain events;
- Recognition domain events.

---

## 2. Event Principles

### 2.1 Events represent facts

Event names use past-tense semantics.

Correct:

```text
ProviderRegistered
ProviderEnabled
ProviderLeaseGranted
ProviderHealthChanged
ProviderCircuitOpened
LocalModelLoaded
```

Incorrect:

```text
RegisterProvider
EnableProvider
GrantLease
CheckProviderHealth
LoadLocalModel
```

The incorrect forms express commands.

---

### 2.2 Events are immutable

After publication, an event must never be modified.

Corrections or later changes require another event.

---

### 2.3 Events are provider-neutral

Public Provider Management events must not expose:

- provider-native request payloads;
- provider-native response payloads;
- SDK objects;
- raw authentication headers;
- API keys;
- access tokens;
- client secrets;
- private keys;
- credential file paths;
- provider-native retry objects;
- unredacted provider-native error bodies.

Normalized provider identifiers, model identifiers, capability snapshots, usage summaries, health observations, and selection reasons may be included when necessary.

---

### 2.4 Events are capability-neutral

Provider Management events must not assume that every provider serves Translation.

The same event model must support:

- Translation;
- Recognition;
- generic structured generation;
- future AI capabilities;
- local models;
- operating-system providers;
- custom provider types.

---

### 2.5 Events preserve traceability

Events must contain enough identity information to associate them with the relevant provider-management entity.

Examples:

```text
ProviderId
ProviderModelId
ProviderLeaseId
ProviderCircuitId
LocalModelInstanceId
ProviderConfigurationRevision
```

Lease and selection events should additionally identify the consumer module and operation reference when policy permits.

---

### 2.6 Events are not full entity snapshots by default

Events should contain:

- stable identifiers;
- state changes;
- compact state or revision information;
- references to larger queryable objects;
- information required by expected consumers.

Large model catalogs, full provider configurations, raw health evidence, or secret material must not be copied into every event.

---

### 2.7 Events may be delivered more than once

Consumers must assume at-least-once delivery unless the Event Bus architecture explicitly provides stronger guarantees.

Consumers must be idempotent.

---

### 2.8 Event order is scoped

Consumers must not assume global ordering.

Ordering should normally be guaranteed only within an explicit entity stream, such as:

```text
ProviderId
ProviderLeaseId
LocalModelInstanceId
ProviderCircuitId
```

---

### 2.9 Events do not replace queries

Events notify consumers that provider-management state changed.

Queries remain authoritative for retrieving current Provider Management state.

---

### 2.10 Provider state remains owner-controlled

Translation, Recognition, Runtime, and Presentation must not infer or publish Provider Management state transitions on behalf of Provider Management.

They may send commands or outcome feedback.

Provider Management publishes the resulting provider-state facts.

---

## 3. Event Categories

Provider Management publishes these main event categories:

```text
Provider Definition Events
Provider Model Events
Provider Capability Events
Provider Selection Events
Provider Lease Events
Availability Events
Health Events
Rate Limit and Quota Events
Circuit Breaker Events
Credential Availability Events
Provider Client Events
Local Model Events
Usage and Outcome Events
Administrative Events
```

Not every deployment must publish every operational event publicly.

---

## 4. Event Ownership Matrix

| Event group | Owner | Notes |
|---|---|---|
| Provider registration and configuration | Provider Management | Logical provider lifecycle |
| Provider model lifecycle | Provider Management | Model metadata and selection eligibility |
| Provider capability changes | Provider Management | Normalized capability snapshots |
| Provider selection decisions | Provider Management | May be internal by default |
| Provider lease lifecycle | Provider Management | Provider-access authority |
| Provider availability | Provider Management | Derived normalized state |
| Provider health | Provider Management | Provider-relevant operational evidence |
| Provider circuit breaker | Provider Management | Operational protection state |
| Credential availability | Provider Management / Secret boundary | Never expose secret material |
| Local model lifecycle | Provider Management | Runtime resource admission remains external |
| Runtime work events | Runtime | Not redefined here |
| Translation events | Translation | Not redefined here |
| Recognition events | Recognition | Not redefined here |
| Reading Session events | Reading Session | Not redefined here |
| Presentation events | Presentation | Not redefined here |

---

## 5. Public Event Set

The initial Provider Management event set is:

```text
ProviderRegistered
ProviderUpdated
ProviderEnabled
ProviderDisabled
ProviderArchived

ProviderModelRegistered
ProviderModelActivated
ProviderModelDeprecated
ProviderModelDisabled
ProviderModelRemoved

ProviderCapabilityChanged
ProviderModelCatalogRefreshed

ProviderSelectionCompleted
ProviderSelectionRejected

ProviderLeaseRequested
ProviderLeaseGranted
ProviderLeaseActivated
ProviderLeaseReleaseRequested
ProviderLeaseReleased
ProviderLeaseExpired
ProviderLeaseRevoked
ProviderLeaseRejected
ProviderLeaseFailed

ProviderAvailabilityChanged
ProviderHealthChanged

ProviderRateLimitChanged
ProviderQuotaChanged

ProviderCircuitOpened
ProviderCircuitHalfOpened
ProviderCircuitClosed

ProviderCredentialAvailabilityChanged

ProviderClientCreated
ProviderClientReplaced
ProviderClientDisposed

LocalModelRegistered
LocalModelInstallStarted
LocalModelInstalled
LocalModelValidationStarted
LocalModelValidated
LocalModelLoadStarted
LocalModelLoaded
LocalModelBecameBusy
LocalModelBecameReady
LocalModelUnloadStarted
LocalModelUnloaded
LocalModelFailed
LocalModelRemoved

ProviderUsageRecorded
ProviderOutcomeFeedbackAccepted
```

The required, recommended, and optional visibility of these events is defined later.

---

# Part I — Event Envelope

## 6. ProviderManagementEventEnvelope

Every public event uses the common CRAI event envelope.

Conceptual shape:

```text
ProviderManagementEventEnvelope<TPayload> {
    eventId
    eventType
    eventVersion

    occurredAt
    publishedAt

    producer
    subject

    correlationId
    causationId
    traceContext

    partitionKey
    sequence

    payload
}
```

---

## 7. eventId

Uniquely identifies one event instance.

Consumers use it for:

- deduplication;
- audit tracing;
- replay safety;
- support diagnostics;
- idempotent projection updates.

An event-delivery retry reuses the same `eventId`.

A new domain fact uses a new `eventId`.

---

## 8. eventType

Recommended naming convention:

```text
provider-management.<entity>.<fact>
```

Examples:

```text
provider-management.provider.registered
provider-management.provider.enabled
provider-management.model.activated
provider-management.lease.granted
provider-management.health.changed
provider-management.circuit.opened
provider-management.local-model.loaded
```

---

## 9. eventVersion

Identifies the payload schema version.

Example:

```text
1
```

Provider-native API or SDK versions must not determine the CRAI event version.

A backward-incompatible payload change requires a new event version.

---

## 10. occurredAt

The timestamp when the domain fact occurred.

This may differ from publication time.

---

## 11. publishedAt

The timestamp when the event was submitted to the Event Bus.

---

## 12. producer

Conceptual shape:

```text
producer {
    module = "provider-management"
    instanceId
}
```

`instanceId` may be optional outside distributed deployments.

---

## 13. subject

Identifies the primary Provider Management entity represented by the event.

Examples:

```text
subject {
    type = "provider"
    id = ProviderId
}
```

```text
subject {
    type = "provider-model"
    id = ProviderModelId
}
```

```text
subject {
    type = "provider-lease"
    id = ProviderLeaseId
}
```

---

## 14. correlationId

Associates events with a larger application flow.

Examples:

- selecting a provider for one Translation batch;
- loading a local model for Recognition;
- rotating provider credentials;
- disabling a provider during maintenance;
- recovering a provider after an outage.

---

## 15. causationId

Identifies the command or event that directly caused the event.

Examples:

```text
RegisterProviderCommand.commandId
RequestProviderLeaseCommand.commandId
ProviderOutcomeFeedback.eventId
CredentialAvailabilityChanged.eventId
RuntimeResourceAdmissionGranted.eventId
```

---

## 16. traceContext

Carries distributed tracing information.

It must not contain secrets or provider content.

---

## 17. partitionKey

Recommended partition keys depend on the entity.

### Provider lifecycle events

```text
partitionKey = ProviderId
```

### Provider model events

```text
partitionKey = ProviderId
```

or:

```text
partitionKey = ProviderModelId
```

The deployment must choose one consistent ordering model.

### Lease events

```text
partitionKey = ProviderLeaseId
```

or the parent `ProviderId` when provider-wide ordering is more important.

### Circuit events

```text
partitionKey = ProviderCircuitId
```

### Local model events

```text
partitionKey = LocalModelInstanceId
```

---

## 18. sequence

A monotonically increasing sequence within the selected event stream.

Example:

```text
sequence = 1, 2, 3, ...
```

Sequence numbering is not global.

Consumers must track the stream identity associated with the sequence.

---

# Part II — Common Payload Contracts

## 19. ProviderEventIdentity

Common provider identity fields:

```text
ProviderEventIdentity {
    providerId
    providerRevision
    providerConfigurationRevision
}
```

Only relevant fields are required.

---

## 20. ProviderModelEventIdentity

```text
ProviderModelEventIdentity {
    providerId
    providerModelId
    providerModelRevision
}
```

---

## 21. ProviderLeaseEventIdentity

```text
ProviderLeaseEventIdentity {
    providerLeaseId

    providerId
    providerModelId

    consumerModule
    operationReference

    capability
}
```

`operationReference` is opaque and must not expose source content.

---

## 22. ProviderStateChange

```text
ProviderStateChange<TState> {
    previousState
    currentState
    stateRevision
    reasonCode
}
```

`reasonCode` must be normalized and free from provider-native secrets.

---

## 23. Compact Capability Summary

```text
ProviderCapabilitySummary {
    capability
    supportLevel
    keyLimits
    locality
}
```

Large capability documents should be referenced through:

```text
capabilitySnapshotId
```

---

## 24. Compact Selection Explanation

```text
ProviderSelectionExplanationSummary {
    primaryReason
    hardConstraintsSatisfied[]
    preferenceReasons[]
    excludedCandidateCount
}
```

Detailed scoring remains queryable through an approved explain-selection query.

---

## 25. Compact Health Summary

```text
ProviderHealthSummary {
    state
    healthRevision
    evidenceWindowStartedAt
    evidenceWindowEndedAt
    observedLatencyClass
    recentFailureClass
}
```

Raw provider responses and full evidence samples must not be embedded.

---

## 26. Compact Availability Summary

```text
ProviderAvailabilitySummary {
    state
    availabilityRevision
    reasonCodes[]
    retryAfter
    eligibleForNewSelection
    eligibleForNewLease
}
```

---

## 27. Compact Rate Limit Summary

```text
ProviderRateLimitSummary {
    scope
    constrained
    retryAfter
    resetAt
    remainingClass
}
```

Exact remaining token or character counts may be included only when safe and useful.

---

## 28. Compact Usage Summary

```text
ProviderUsageSummary {
    requestCount
    inputCharacters
    outputCharacters
    inputTokens
    outputTokens
    imageCount
    executionDuration
    estimatedCost
    currency
    providerReported
    estimated
}
```

All usage fields may be optional depending on provider support.

---

# Part III — Provider Definition Events

## 29. ProviderRegistered

Published after a new provider definition is persisted.

Event type:

```text
provider-management.provider.registered
```

Payload:

```text
ProviderRegisteredPayload {
    providerId
    providerRevision
    providerConfigurationRevision

    providerClass
    providerKind
    executionLocality

    registeredBy
    registeredAt
}
```

Meaning:

- provider identity exists;
- provider is not necessarily enabled;
- provider is not necessarily available;
- credential and model readiness may remain unresolved.

Expected consumers:

- Administration;
- Observability;
- Provider configuration UI;
- audit components.

Must not include:

- raw credentials;
- full endpoint secrets;
- private adapter configuration;
- provider SDK objects.

---

## 30. ProviderUpdated

Published after a provider definition or configuration revision changes.

Event type:

```text
provider-management.provider.updated
```

Payload:

```text
ProviderUpdatedPayload {
    providerId

    previousProviderRevision
    currentProviderRevision

    previousConfigurationRevision
    currentConfigurationRevision

    changedFieldGroups[]
    activeLeasePolicy

    updatedBy
    updatedAt
}
```

Possible `changedFieldGroups`:

```text
BASIC_METADATA
ENDPOINT
REGION
CREDENTIAL_REFERENCE
MODEL_CATALOG
CAPABILITY_POLICY
HEALTH_POLICY
RATE_LIMIT_POLICY
LOCAL_MODEL_POLICY
PRIVACY_POLICY
LIFECYCLE_POLICY
```

The event should not include full old and new secret-bearing configurations.

---

## 31. ProviderEnabled

Published after the provider enters `ENABLED`.

Event type:

```text
provider-management.provider.enabled
```

Payload:

```text
ProviderEnabledPayload {
    providerId
    providerRevision
    providerConfigurationRevision

    previousState
    currentState = ENABLED

    enabledBy
    enabledAt
}
```

Meaning:

The provider may participate in eligibility evaluation.

It does not imply `AVAILABLE`.

---

## 32. ProviderDisabled

Published after the provider enters `DISABLED`.

Event type:

```text
provider-management.provider.disabled
```

Payload:

```text
ProviderDisabledPayload {
    providerId
    providerRevision

    previousState
    currentState = DISABLED

    disablePolicy
    activeLeaseCount
    affectedLeaseIdsReference

    reasonCode

    disabledBy
    disabledAt
}
```

Possible disable policies:

```text
ALLOW_DRAIN
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE_PERIOD
```

Large affected lease lists should be referenced rather than embedded.

---

## 33. ProviderArchived

Published after the provider enters terminal `ARCHIVED`.

Event type:

```text
provider-management.provider.archived
```

Payload:

```text
ProviderArchivedPayload {
    providerId
    providerRevision

    previousState
    currentState = ARCHIVED

    historicalDataRetained
    archivedBy
    archivedAt
}
```

The provider identity remains usable for historical references.

---

# Part IV — Provider Model Events

## 34. ProviderModelRegistered

Published after model metadata is persisted.

Event type:

```text
provider-management.model.registered
```

Payload:

```text
ProviderModelRegisteredPayload {
    providerId
    providerModelId
    providerModelRevision

    modelClass
    executionType
    capabilitySnapshotId

    registeredAt
}
```

---

## 35. ProviderModelActivated

Published after the model enters `ACTIVE`.

Event type:

```text
provider-management.model.activated
```

Payload:

```text
ProviderModelActivatedPayload {
    providerId
    providerModelId
    providerModelRevision

    previousState
    currentState = ACTIVE

    activatedBy
    activatedAt
}
```

`ACTIVE` does not guarantee provider availability or local residency.

---

## 36. ProviderModelDeprecated

Published after the model enters `DEPRECATED`.

Event type:

```text
provider-management.model.deprecated
```

Payload:

```text
ProviderModelDeprecatedPayload {
    providerId
    providerModelId
    providerModelRevision

    replacementModelId
    newLeasePolicy
    reasonCode

    deprecatedAt
}
```

Possible `newLeasePolicy`:

```text
DISALLOW_NEW
ALLOW_EXPLICIT_ONLY
ALLOW_UNTIL_DATE
ALLOW_WHEN_NO_REPLACEMENT
```

---

## 37. ProviderModelDisabled

Published after the model enters `DISABLED`.

Event type:

```text
provider-management.model.disabled
```

Payload:

```text
ProviderModelDisabledPayload {
    providerId
    providerModelId
    providerModelRevision

    activeLeasePolicy
    reasonCode

    disabledAt
}
```

---

## 38. ProviderModelRemoved

Published after the model enters terminal `REMOVED`.

Event type:

```text
provider-management.model.removed
```

Payload:

```text
ProviderModelRemovedPayload {
    providerId
    providerModelId
    providerModelRevision

    historicalIdentityRetained
    removedAt
}
```

---

# Part V — Capability and Catalog Events

## 39. ProviderCapabilityChanged

Published after normalized capability metadata changes.

Event type:

```text
provider-management.capability.changed
```

Payload:

```text
ProviderCapabilityChangedPayload {
    providerId
    providerModelId

    previousCapabilitySnapshotId
    currentCapabilitySnapshotId

    changedCapabilities[]
    changeSource

    changedAt
}
```

Possible sources:

```text
ADMINISTRATIVE
CATALOG_REFRESH
ADAPTER_DISCOVERY
LOCAL_MODEL_VALIDATION
PROVIDER_ANNOUNCEMENT
CAPABILITY_TEST
```

Consumers should re-query current capabilities before using the provider path.

---

## 40. ProviderModelCatalogRefreshed

Published after model catalog refresh completes.

Event type:

```text
provider-management.model-catalog.refreshed
```

Payload:

```text
ProviderModelCatalogRefreshedPayload {
    providerId

    previousCatalogRevision
    currentCatalogRevision

    addedModelCount
    updatedModelCount
    deprecatedModelCount
    removedModelCount

    refreshSource
    refreshedAt
}
```

Full model catalogs must be retrieved through queries.

---

# Part VI — Provider Selection Events

## 41. ProviderSelectionCompleted

Published when Provider Management completes a selection decision.

Event type:

```text
provider-management.selection.completed
```

Payload:

```text
ProviderSelectionCompletedPayload {
    providerSelectionId

    consumerModule
    operationReference
    capability

    selectedProviderId
    selectedProviderModelId

    selectionExplanation
    fallbackRank

    requirementRevision
    policySnapshotId

    selectedAt
}
```

Visibility:

- internal by default;
- public when audit, administration, or distributed coordination requires it;
- useful for observability and explainability.

The event must not contain semantic task payload content.

---

## 42. ProviderSelectionRejected

Published when no provider path satisfies the selection request.

Event type:

```text
provider-management.selection.rejected
```

Payload:

```text
ProviderSelectionRejectedPayload {
    providerSelectionId

    consumerModule
    operationReference
    capability

    rejectionCode
    hardConstraintFailures[]
    candidateCount
    excludedCandidateCount

    rejectedAt
}
```

This event does not mean the consumer domain operation has failed finally.

The consumer may:

- adjust policy;
- choose another mode;
- wait;
- request a new selection later.

---

# Part VII — Provider Lease Events

## 43. ProviderLeaseRequested

Published after a lease request is accepted for processing.

Event type:

```text
provider-management.lease.requested
```

Payload:

```text
ProviderLeaseRequestedPayload {
    providerLeaseId
    providerSelectionId

    providerId
    providerModelId

    consumerModule
    operationReference
    capability

    requestedAt
    requestExpiresAt
}
```

Visibility:

```text
internal by default
```

This event is useful for distributed lease coordinators or detailed audit.

---

## 44. ProviderLeaseGranted

Published after the lease enters `GRANTED` and its execution handle is retrievable.

Event type:

```text
provider-management.lease.granted
```

Payload:

```text
ProviderLeaseGrantedPayload {
    providerLeaseId

    providerId
    providerModelId

    consumerModule
    operationReference
    capability

    capabilitySnapshotId
    policySnapshotId
    providerConfigurationRevision

    executionHandleReference

    grantedAt
    expiresAt
}
```

Critical rule:

`ProviderLeaseGranted` must not be published unless the lease and handle reference are durable enough to be queried or resolved.

The event must not contain:

- raw credentials;
- client objects;
- provider-native SDK handles;
- secret-bearing endpoint configuration.

---

## 45. ProviderLeaseActivated

Published after actual provider use begins through the lease.

Event type:

```text
provider-management.lease.activated
```

Payload:

```text
ProviderLeaseActivatedPayload {
    providerLeaseId

    providerId
    providerModelId

    consumerModule
    operationReference

    runtimeWorkReference
    activatedAt
}
```

Recommended visibility:

```text
internal by default
```

Runtime remains owner of `runtimeWorkReference`.

---

## 46. ProviderLeaseReleaseRequested

Published after normal release is requested and the lease enters `RELEASE_REQUESTED`.

Event type:

```text
provider-management.lease.release-requested
```

Payload:

```text
ProviderLeaseReleaseRequestedPayload {
    providerLeaseId

    releaseReason
    activeExecutionKnown
    requestedAt
}
```

---

## 47. ProviderLeaseReleased

Published after the lease enters terminal `RELEASED`.

Event type:

```text
provider-management.lease.released
```

Payload:

```text
ProviderLeaseReleasedPayload {
    providerLeaseId

    providerId
    providerModelId

    consumerModule
    operationReference

    usageSummary
    releasedAt
}
```

Meaning:

- no new provider operation may begin through the lease;
- pooled provider resources may remain alive;
- local model residency may remain unchanged.

---

## 48. ProviderLeaseExpired

Published after the lease enters terminal `EXPIRED`.

Event type:

```text
provider-management.lease.expired
```

Payload:

```text
ProviderLeaseExpiredPayload {
    providerLeaseId

    previousState
    activeExecutionKnown

    expirationReason
    expiredAt
}
```

Possible reasons:

```text
REQUEST_DEADLINE_EXPIRED
EXECUTION_NOT_STARTED
LEASE_LIFETIME_EXCEEDED
CONSUMER_DID_NOT_RELEASE
GRACE_PERIOD_EXPIRED
CONFIGURATION_POLICY_INVALIDATED
```

Expiration invalidates the handle for new use.

Physical execution cleanup may remain pending.

---

## 49. ProviderLeaseRevoked

Published after the lease enters terminal `REVOKED`.

Event type:

```text
provider-management.lease.revoked
```

Payload:

```text
ProviderLeaseRevokedPayload {
    providerLeaseId

    providerId
    providerModelId

    revocationReason
    activeExecutionKnown
    cancellationRequested

    revokedBy
    revokedAt
}
```

Possible reasons:

```text
PROVIDER_DISABLED
MODEL_DISABLED
CREDENTIAL_REVOKED
SECURITY_POLICY_CHANGED
PRIVACY_POLICY_CHANGED
REGION_BECAME_PROHIBITED
ADAPTER_INTEGRITY_FAILURE
LOCAL_RESOURCE_UNSAFE
ADMINISTRATIVE
APPLICATION_SHUTDOWN
```

Provider Management does not claim that physical provider execution stopped immediately.

---

## 50. ProviderLeaseRejected

Published after a valid lease request is denied and the lease enters `REJECTED`.

Event type:

```text
provider-management.lease.rejected
```

Payload:

```text
ProviderLeaseRejectedPayload {
    providerLeaseId

    providerId
    providerModelId

    rejectionCode
    eligibilitySnapshotId
    retryAfter

    rejectedAt
}
```

---

## 51. ProviderLeaseFailed

Published after lease creation or management fails unexpectedly.

Event type:

```text
provider-management.lease.failed
```

Payload:

```text
ProviderLeaseFailedPayload {
    providerLeaseId

    providerId
    providerModelId

    failureSummary
    previousState

    failedAt
}
```

The complete failure contract belongs in `ERRORS.md`.

---

# Part VIII — Availability Events

## 52. ProviderAvailabilityChanged

Published when normalized provider availability changes materially.

Event type:

```text
provider-management.availability.changed
```

Payload:

```text
ProviderAvailabilityChangedPayload {
    providerId
    providerModelId
    availabilityScope

    previousAvailability
    currentAvailability

    availabilityRevision
    reasonCodes[]

    eligibleForNewSelection
    eligibleForNewLease

    retryAfter
    changedAt
}
```

Possible scopes:

```text
PROVIDER
MODEL
CAPABILITY
REGION
CREDENTIAL
LOCAL_MODEL_INSTANCE
```

---

## 53. Availability Event Throttling

Availability may change rapidly because of:

- rate limits;
- resource pressure;
- short-lived health evidence;
- local model loading;
- concurrent capacity.

Implementations should publish only meaningful transitions.

They should not emit one event for every request completion.

Recommended publication triggers:

- availability state changes;
- selection eligibility changes;
- new lease eligibility changes;
- retry-after changes materially;
- maintenance or draining begins or ends;
- resource constraint crosses a configured threshold.

---

# Part IX — Health Events

## 54. ProviderHealthChanged

Published when normalized provider health changes.

Event type:

```text
provider-management.health.changed
```

Payload:

```text
ProviderHealthChangedPayload {
    providerId
    providerModelId
    healthScope

    previousHealth
    currentHealth

    healthRevision
    evidenceSummary

    changedAt
}
```

Possible scopes:

```text
PROVIDER
MODEL
CAPABILITY
REGION
ENDPOINT
LOCAL_MODEL_INSTANCE
```

---

## 55. Health Evidence Safety

Health events may include:

- normalized failure classes;
- latency class;
- evidence count;
- evidence window;
- probe status;
- success ratio class.

They must not include:

- raw source content;
- translated text;
- recognized text;
- raw provider response bodies;
- secret-bearing endpoint details;
- stack traces.

---

## 56. Provider Health Recovery

A transition from:

```text
UNHEALTHY → DEGRADED
UNHEALTHY → HEALTHY
DEGRADED → HEALTHY
```

is represented through `ProviderHealthChanged`.

A separate `ProviderHealthRecovered` event is optional and should not duplicate the same fact unless a specific consumer requires it.

---

# Part X — Rate Limit and Quota Events

## 57. ProviderRateLimitChanged

Published when normalized rate-limit state changes materially.

Event type:

```text
provider-management.rate-limit.changed
```

Payload:

```text
ProviderRateLimitChangedPayload {
    providerId
    providerModelId
    credentialReferenceId
    region

    rateLimitScope

    previousConstrained
    currentConstrained

    retryAfter
    resetAt
    remainingClass

    changedAt
}
```

Possible rate-limit scopes:

```text
REQUESTS
TOKENS
CHARACTERS
IMAGES
CONCURRENT_EXECUTIONS
PROVIDER_DEFINED
```

Exact credential values are never included.

---

## 58. ProviderQuotaChanged

Published when quota or account capacity changes materially.

Event type:

```text
provider-management.quota.changed
```

Payload:

```text
ProviderQuotaChangedPayload {
    providerId
    credentialReferenceId

    quotaScope
    previousQuotaClass
    currentQuotaClass

    exhausted
    resetAt

    changedAt
}
```

Possible quota scopes:

```text
DAILY
MONTHLY
ACCOUNT_BALANCE
PROVIDER_CREDITS
LOCAL_CAPACITY
```

---

## 59. Rate Event Visibility

Rate and quota events are:

- recommended for Provider Management;
- useful to Runtime, selection, and observability;
- potentially sensitive.

They should normally expose normalized classes rather than exact account-level billing details.

---

# Part XI — Circuit Breaker Events

## 60. ProviderCircuitOpened

Published after a circuit enters `OPEN`.

Event type:

```text
provider-management.circuit.opened
```

Payload:

```text
ProviderCircuitOpenedPayload {
    providerCircuitId

    providerId
    providerModelId
    capability
    region
    credentialReferenceId

    previousState
    currentState = OPEN

    triggerCode
    evidenceSummary
    openedAt
    eligibleProbeAfter
}
```

Opening one scoped circuit must not be interpreted as disabling the provider definition globally unless the circuit scope is provider-wide.

---

## 61. ProviderCircuitHalfOpened

Published after a circuit enters `HALF_OPEN`.

Event type:

```text
provider-management.circuit.half-opened
```

Payload:

```text
ProviderCircuitHalfOpenedPayload {
    providerCircuitId

    providerId
    providerModelId

    probeLimit
    probeCapability
    enteredAt
}
```

Only controlled probe traffic should use this path.

---

## 62. ProviderCircuitClosed

Published after a circuit returns to `CLOSED`.

Event type:

```text
provider-management.circuit.closed
```

Payload:

```text
ProviderCircuitClosedPayload {
    providerCircuitId

    providerId
    providerModelId

    closeReason
    probeOutcomeSummary

    closedAt
}
```

Possible reasons:

```text
PROBE_SUCCEEDED
ADMINISTRATIVE_RESET
TRUSTED_HEALTH_RECOVERY
CONFIGURATION_REPLACED
```

---

# Part XII — Credential Availability Events

## 63. ProviderCredentialAvailabilityChanged

Published when the normalized availability of a provider credential reference changes.

Event type:

```text
provider-management.credential.availability-changed
```

Payload:

```text
ProviderCredentialAvailabilityChangedPayload {
    providerCredentialReferenceId

    providerId
    credentialScope

    previousAvailability
    currentAvailability

    credentialRevision
    reasonCode

    changedAt
}
```

Possible availability values:

```text
UNKNOWN
AVAILABLE
UNAVAILABLE
EXPIRED
REVOKED
LOCKED
REFRESHING
```

Must not include:

- secret values;
- authorization headers;
- token fragments;
- secret-manager paths when sensitive;
- provider-native credential payloads.

---

## 64. Credential Event Consumer Rule

Consumers may use this event to:

- stop selecting affected provider paths;
- revoke or drain leases according to policy;
- update availability projections;
- notify administration.

Consumers must not attempt to resolve raw credentials from the event.

---

# Part XIII — Provider Client Events

## 65. ProviderClientCreated

Operational event published after a provider client instance becomes usable.

Event type:

```text
provider-management.client.created
```

Payload:

```text
ProviderClientCreatedPayload {
    providerClientInstanceId

    providerId
    providerConfigurationRevision
    credentialReferenceId

    clientClass
    createdAt
}
```

Recommended visibility:

```text
internal or observability-only
```

---

## 66. ProviderClientReplaced

Published when one provider client instance is replaced by another.

Event type:

```text
provider-management.client.replaced
```

Payload:

```text
ProviderClientReplacedPayload {
    providerId

    previousClientInstanceId
    currentClientInstanceId

    replacementReason
    drainingPreviousClient

    replacedAt
}
```

Possible reasons:

```text
CONFIGURATION_CHANGED
CREDENTIAL_ROTATED
CLIENT_UNHEALTHY
ENDPOINT_CHANGED
ADAPTER_UPDATED
ADMINISTRATIVE
```

---

## 67. ProviderClientDisposed

Published after a client instance is disposed.

Event type:

```text
provider-management.client.disposed
```

Payload:

```text
ProviderClientDisposedPayload {
    providerClientInstanceId
    providerId

    activeLeaseCountAtDisposal
    disposalReason

    disposedAt
}
```

This event should not expose SDK objects or network details.

---

# Part XIV — Local Model Events

## 68. LocalModelRegistered

Published after a local model definition is registered.

Event type:

```text
provider-management.local-model.registered
```

Payload:

```text
LocalModelRegisteredPayload {
    providerId
    providerModelId
    localModelInstanceId

    modelRevision
    installationState
    registeredAt
}
```

---

## 69. LocalModelInstallStarted

Published after state enters `INSTALLING`.

Event type:

```text
provider-management.local-model.install-started
```

Payload:

```text
LocalModelInstallStartedPayload {
    providerId
    providerModelId
    localModelInstanceId

    installationSource
    expectedSizeClass
    startedAt
}
```

Model download URLs, credentials, and private storage paths should not be exposed.

---

## 70. LocalModelInstalled

Published after installation completes.

Event type:

```text
provider-management.local-model.installed
```

Payload:

```text
LocalModelInstalledPayload {
    providerId
    providerModelId
    localModelInstanceId

    modelRevision
    integrityStatus
    installedAt
}
```

`INSTALLED` does not imply `READY`.

---

## 71. LocalModelValidationStarted

Published after state enters `VALIDATING`.

Event type:

```text
provider-management.local-model.validation-started
```

Payload:

```text
LocalModelValidationStartedPayload {
    providerId
    providerModelId
    localModelInstanceId

    validationProfile
    startedAt
}
```

---

## 72. LocalModelValidated

Published after validation succeeds.

Event type:

```text
provider-management.local-model.validated
```

Payload:

```text
LocalModelValidatedPayload {
    providerId
    providerModelId
    localModelInstanceId

    modelRevision
    compatibilitySummary
    validatedAt
}
```

This event may correspond to transition back to `INSTALLED` or immediate progression toward `LOADING`.

---

## 73. LocalModelLoadStarted

Published after state enters `LOADING`.

Event type:

```text
provider-management.local-model.load-started
```

Payload:

```text
LocalModelLoadStartedPayload {
    providerId
    providerModelId
    localModelInstanceId

    runtimeResourceAdmissionId
    targetDeviceClass
    startedAt
}
```

Runtime or Resource Management owns the referenced admission state.

---

## 74. LocalModelLoaded

Published after state enters `READY`.

Event type:

```text
provider-management.local-model.loaded
```

Payload:

```text
LocalModelLoadedPayload {
    providerId
    providerModelId
    localModelInstanceId

    modelRevision
    deviceClass
    resourceSummary
    capabilitySnapshotId

    loadedAt
}
```

`READY` does not automatically mean the provider is eligible if another hard constraint blocks it.

---

## 75. LocalModelBecameBusy

Published when model capacity becomes materially constrained and state enters `BUSY`.

Event type:

```text
provider-management.local-model.became-busy
```

Payload:

```text
LocalModelBecameBusyPayload {
    localModelInstanceId

    activeExecutionCount
    capacityClass
    busyReason

    becameBusyAt
}
```

Recommended visibility:

```text
internal or observability-only
```

---

## 76. LocalModelBecameReady

Published when a busy local model returns to `READY`.

Event type:

```text
provider-management.local-model.became-ready
```

Payload:

```text
LocalModelBecameReadyPayload {
    localModelInstanceId

    activeExecutionCount
    capacityClass

    becameReadyAt
}
```

---

## 77. LocalModelUnloadStarted

Published after state enters `UNLOADING`.

Event type:

```text
provider-management.local-model.unload-started
```

Payload:

```text
LocalModelUnloadStartedPayload {
    localModelInstanceId

    unloadReason
    activeLeaseCount
    drainPolicy

    startedAt
}
```

---

## 78. LocalModelUnloaded

Published after state enters `UNLOADED`.

Event type:

```text
provider-management.local-model.unloaded
```

Payload:

```text
LocalModelUnloadedPayload {
    localModelInstanceId

    releasedResourceSummary
    unloadedAt
}
```

The model may remain installed.

---

## 79. LocalModelFailed

Published after the local model enters `FAILED`.

Event type:

```text
provider-management.local-model.failed
```

Payload:

```text
LocalModelFailedPayload {
    providerId
    providerModelId
    localModelInstanceId

    previousState
    failureSummary
    recoveryActions[]

    failedAt
}
```

Raw runtime stack traces and private file paths remain internal.

---

## 80. LocalModelRemoved

Published after state enters terminal `REMOVED`.

Event type:

```text
provider-management.local-model.removed
```

Payload:

```text
LocalModelRemovedPayload {
    providerId
    providerModelId
    localModelInstanceId

    modelFilesRemoved
    historicalIdentityRetained

    removedAt
}
```

---

# Part XV — Usage and Outcome Events

## 81. ProviderUsageRecorded

Published after normalized provider usage is recorded.

Event type:

```text
provider-management.usage.recorded
```

Payload:

```text
ProviderUsageRecordedPayload {
    providerUsageRecordId

    providerId
    providerModelId
    providerLeaseId

    consumerModule
    operationReference
    capability

    usageSummary

    recordedAt
}
```

Visibility:

- observability and cost analysis;
- internal by default when high volume;
- never include source or result content.

---

## 82. ProviderOutcomeFeedbackAccepted

Published after Provider Management accepts normalized provider-relevant execution feedback.

Event type:

```text
provider-management.outcome-feedback.accepted
```

Payload:

```text
ProviderOutcomeFeedbackAcceptedPayload {
    providerOutcomeFeedbackId

    providerId
    providerModelId
    providerLeaseId

    consumerModule
    operationReference

    outcomeClass
    providerRelevant
    healthEvidenceApplied
    circuitEvidenceApplied
    usageApplied

    acceptedAt
}
```

This event confirms that feedback entered Provider Management.

It does not guarantee a health or circuit transition.

---

## 83. Outcome Classes

Possible normalized outcome classes:

```text
SUCCESS
TIMEOUT
CONNECTION_FAILURE
PROVIDER_INTERNAL_FAILURE
RATE_LIMITED
AUTHENTICATION_FAILURE
MALFORMED_RESPONSE
CAPABILITY_MISMATCH
ADAPTER_FAILURE
LOCAL_MODEL_FAILURE
CANCELLED
CONSUMER_VALIDATION_FAILURE
STALE_RESULT_REJECTED
PRESENTATION_FAILURE
UNKNOWN
```

Only provider-relevant outcomes should influence health or circuit state.

---

# Part XVI — Events Consumed by Provider Management

## 84. Upstream Events

Provider Management may consume events from other modules and infrastructure.

Expected categories include:

```text
RuntimeResourceAdmissionGranted
RuntimeResourceAdmissionDenied
RuntimeExecutionStarted
RuntimeExecutionCompleted
RuntimeExecutionFailed
RuntimeExecutionCancelled

SecretCredentialAvailabilityChanged
SecretCredentialRotated
SecretCredentialRevoked

TranslationProviderOutcomeReported
RecognitionProviderOutcomeReported

ApplicationShutdownStarted
ApplicationShutdownCompleted

NetworkAvailabilityChanged
DeviceResourcePressureChanged
```

Exact event names depend on the owning modules.

Provider Management must not redefine them.

---

## 85. Runtime Resource Admission

When Runtime grants resource admission for a local model:

- Provider Management may enter `LOADING`;
- it may bind an admitted resource context;
- it may create or activate a local model instance.

When Runtime denies admission:

- local model health does not automatically become unhealthy;
- availability may become `RESOURCE_CONSTRAINED`;
- selection may choose another provider;
- pending load requests may remain rejected or deferred.

---

## 86. Credential Rotation

On credential rotation:

- new leases should use the new credential revision;
- provider clients may be replaced;
- existing leases may drain or be revoked;
- provider availability may be recomputed;
- credential availability events may be published.

Raw secret material must never enter Provider Management events.

---

## 87. Translation Outcome Feedback

Translation may report:

- provider timeout;
- provider rate limit;
- malformed provider response;
- provider authentication failure;
- successful usage;
- adapter failure;
- translation semantic validation failure;
- stale-result rejection.

Provider Management must distinguish provider-relevant evidence from Translation-only failures.

---

## 88. Recognition Outcome Feedback

Recognition follows the same rule.

Provider Management must not treat:

- invalid image region;
- recognition reading-order failure;
- upstream source corruption;
- stale result;
- Presentation failure

as provider-health evidence unless a provider-relevant cause is proven.

---

## 89. Application Shutdown

Application shutdown may cause:

- providers to enter `DRAINING`;
- new lease requests to be rejected;
- active leases to release or revoke;
- local models to unload;
- provider clients to dispose;
- final lifecycle events to publish.

Runtime owns process and worker shutdown.

---

# Part XVII — Event Ordering

## 90. Provider Stream Ordering

Provider lifecycle events should use:

```text
partitionKey = ProviderId
```

Example:

```text
1 ProviderRegistered
2 ProviderUpdated
3 ProviderEnabled
4 ProviderAvailabilityChanged
5 ProviderDisabled
6 ProviderArchived
```

---

## 91. Lease Stream Ordering

Lease events should preserve order within one lease stream.

Example:

```text
1 ProviderLeaseRequested
2 ProviderLeaseGranted
3 ProviderLeaseActivated
4 ProviderLeaseReleaseRequested
5 ProviderLeaseReleased
```

Alternative terminal sequence:

```text
1 ProviderLeaseRequested
2 ProviderLeaseGranted
3 ProviderLeaseRevoked
```

A lease must not emit incompatible terminal outcomes.

---

## 92. Local Model Stream Ordering

Example:

```text
1 LocalModelRegistered
2 LocalModelInstallStarted
3 LocalModelInstalled
4 LocalModelValidationStarted
5 LocalModelValidated
6 LocalModelLoadStarted
7 LocalModelLoaded
8 LocalModelUnloadStarted
9 LocalModelUnloaded
```

Failure may interrupt any applicable step.

---

## 93. Concurrent Provider Events

Different entities may update concurrently.

Valid arrival order may be:

```text
ProviderHealthChanged
ProviderRateLimitChanged
ProviderAvailabilityChanged
```

or:

```text
ProviderRateLimitChanged
ProviderHealthChanged
ProviderAvailabilityChanged
```

Consumers must use revisions and query current state rather than deriving all state from arrival order alone.

---

## 94. State Revision Rule

Events that update a projection should include the relevant monotonic state revision.

Examples:

```text
providerStateRevision
providerModelStateRevision
providerLeaseStateRevision
availabilityRevision
healthRevision
circuitRevision
localModelStateRevision
```

An older revision must not overwrite a newer projection.

---

# Part XVIII — Delivery and Idempotency

## 95. At-Least-Once Delivery

Consumers must handle duplicate delivery.

Recommended deduplication key:

```text
eventId
```

For aggregate projections, also track:

```text
subject.id
stateRevision
sequence
```

---

## 96. Duplicate Event Handling

Receiving the same event twice must not:

- enable a provider twice;
- revoke a lease twice;
- increment active lease counts twice;
- reopen a circuit twice;
- load a local model twice;
- dispose a provider client twice;
- double-count usage;
- duplicate administrative notifications.

---

## 97. Out-of-Order Handling

When an older state revision arrives:

- ignore it for current projections;
- retain it for audit when required;
- do not roll back current state.

When a sequence gap is detected, consumers may:

- wait briefly;
- query current state;
- rebuild from authoritative query models;
- request event replay when supported.

---

## 98. Consumer Recovery

A consumer recovering after downtime should query:

```text
GetProvider
GetProviderModel
GetProviderAvailability
GetProviderHealth
GetProviderCircuit
GetProviderLease
ListActiveProviderLeases
GetLocalModelState
```

Events provide change notifications.

Queries provide authoritative current state.

---

# Part XIX — Event Payload Size

## 99. Payload Size Principle

Provider Management events should remain compact.

Large data should be referenced using:

```text
ProviderId
ProviderModelId
ProviderLeaseId
capabilitySnapshotId
providerConfigurationRevision
catalogRevision
usageRecordId
healthEvidenceReference
```

rather than embedded repeatedly.

---

## 100. Prohibited Payload Content

Events must not contain:

- API keys;
- access tokens;
- refresh tokens;
- client secrets;
- private keys;
- authorization headers;
- raw provider prompts;
- source reading content;
- translated text;
- recognized text;
- full provider responses;
- private local model paths;
- secret-manager internal locations;
- provider SDK objects;
- full stack traces;
- unrelated reading-session data.

---

# Part XX — Privacy and Security

## 101. Content Minimization

Provider Management events should normally contain no user reading content.

Preferred data:

```text
identifiers
revisions
states
reason codes
counts
durations
capabilities
resource classes
usage summaries
selection explanations
```

---

## 102. Credential Safety

Credential events expose only normalized reference identity and availability.

They must never expose secret material.

---

## 103. Selection Privacy

Selection events must not reveal:

- protected policy internals;
- secret provider scores;
- raw cost-account details;
- provider credentials;
- content-based selection features that reveal private text.

Normalized explainability is allowed.

---

## 104. Local Model Safety

Local model events must avoid exposing:

- sensitive file paths;
- user-specific directories;
- private model-download credentials;
- untrusted model metadata as trusted event fields.

All event metadata is created by trusted Provider Management code.

---

# Part XXI — Event Versioning

## 105. Backward-Compatible Changes

Examples:

- adding an optional field;
- adding an optional normalized reason code;
- adding a new optional capability;
- adding optional metadata;
- adding an enum value when consumers tolerate unknown values.

These may retain the same event version when governance permits.

---

## 106. Breaking Changes

Examples:

- removing a required field;
- changing identifier ownership;
- changing event meaning;
- changing partition-key semantics;
- replacing a provider-neutral field with provider-native data;
- exposing previously secret data;
- changing a state transition fact into an intent.

These require a new event version.

---

## 107. Unknown Fields and Enum Values

Consumers should:

- ignore unknown optional fields;
- preserve unknown values where possible;
- map unknown enum values to `UNKNOWN`;
- avoid failing the whole stream.

---

# Part XXII — Subscription Guidance

## 108. Translation Module

Translation may primarily consume:

```text
ProviderAvailabilityChanged
ProviderRateLimitChanged
ProviderCircuitOpened
ProviderCircuitClosed
ProviderLeaseGranted
ProviderLeaseRevoked
ProviderLeaseExpired
ProviderCredentialAvailabilityChanged
ProviderModelDeprecated
ProviderModelDisabled
```

Translation must still query current provider state when making authoritative decisions.

Translation does not own Provider Management state.

---

## 109. Recognition Module

Recognition may consume the same core events, particularly:

```text
ProviderAvailabilityChanged
ProviderLeaseGranted
ProviderLeaseRevoked
LocalModelLoaded
LocalModelUnloaded
LocalModelFailed
ProviderCapabilityChanged
```

---

## 110. Runtime

Runtime may consume:

```text
ProviderLeaseGranted
ProviderLeaseRevoked
ProviderLeaseExpired
ProviderAvailabilityChanged
ProviderRateLimitChanged
LocalModelLoaded
LocalModelUnloaded
LocalModelFailed
ProviderStartedDraining
```

Runtime owns execution scheduling and resource admission.

---

## 111. Observability

Observability may consume:

```text
ProviderRegistered
ProviderEnabled
ProviderDisabled
ProviderArchived

ProviderSelectionCompleted
ProviderSelectionRejected

ProviderLeaseGranted
ProviderLeaseReleased
ProviderLeaseExpired
ProviderLeaseRevoked
ProviderLeaseFailed

ProviderAvailabilityChanged
ProviderHealthChanged
ProviderRateLimitChanged
ProviderQuotaChanged

ProviderCircuitOpened
ProviderCircuitHalfOpened
ProviderCircuitClosed

LocalModelLoadStarted
LocalModelLoaded
LocalModelFailed
LocalModelUnloaded

ProviderUsageRecorded
```

Observability should prefer identifiers and metrics over content.

---

## 112. Administration

Administration may consume:

```text
ProviderRegistered
ProviderUpdated
ProviderEnabled
ProviderDisabled
ProviderArchived

ProviderModelRegistered
ProviderModelDeprecated
ProviderModelDisabled
ProviderModelRemoved

ProviderCredentialAvailabilityChanged
ProviderHealthChanged
ProviderCircuitOpened
LocalModelFailed
```

---

# Part XXIII — Event Publication Matrix

## 113. Required MVP Events

```text
ProviderRegistered
ProviderUpdated
ProviderEnabled
ProviderDisabled
ProviderArchived

ProviderModelRegistered
ProviderModelActivated
ProviderModelDeprecated
ProviderModelDisabled

ProviderLeaseGranted
ProviderLeaseReleased
ProviderLeaseExpired
ProviderLeaseRevoked
ProviderLeaseRejected
ProviderLeaseFailed

ProviderAvailabilityChanged
ProviderHealthChanged

ProviderCircuitOpened
ProviderCircuitHalfOpened
ProviderCircuitClosed

ProviderCredentialAvailabilityChanged

LocalModelLoadStarted
LocalModelLoaded
LocalModelUnloaded
LocalModelFailed
```

---

## 114. Recommended Events

```text
ProviderCapabilityChanged
ProviderModelCatalogRefreshed

ProviderSelectionCompleted
ProviderSelectionRejected

ProviderRateLimitChanged
ProviderQuotaChanged

LocalModelRegistered
LocalModelInstalled
LocalModelValidated
LocalModelUnloadStarted

ProviderUsageRecorded
ProviderOutcomeFeedbackAccepted
```

---

## 115. Optional Operational Events

```text
ProviderLeaseRequested
ProviderLeaseActivated
ProviderLeaseReleaseRequested

ProviderClientCreated
ProviderClientReplaced
ProviderClientDisposed

LocalModelInstallStarted
LocalModelValidationStarted
LocalModelBecameBusy
LocalModelBecameReady
LocalModelRemoved
```

High-volume operational events should remain internal unless a concrete consumer requires them.

---

# Part XXIV — Event Flow Examples

## 116. Register and Enable Remote Provider

```text
ProviderRegistered
    ↓
ProviderModelRegistered
    ↓
ProviderCapabilityChanged
    ↓
ProviderEnabled
    ↓
ProviderHealthChanged: UNKNOWN → HEALTHY
    ↓
ProviderAvailabilityChanged: UNKNOWN → AVAILABLE
```

---

## 117. Successful Provider Selection and Lease

```text
ProviderSelectionCompleted
    ↓
ProviderLeaseRequested
    ↓
ProviderLeaseGranted
    ↓
ProviderLeaseActivated
    ↓
ProviderUsageRecorded
    ↓
ProviderLeaseReleased
```

`ProviderLeaseRequested` and `ProviderLeaseActivated` may remain internal.

---

## 118. Selection Rejected

```text
ProviderSelectionRejected
```

Possible reasons:

- no capability match;
- privacy restriction;
- locality restriction;
- provider disabled;
- circuit open;
- credential unavailable;
- no eligible model.

The consumer may issue a different request later.

---

## 119. Provider Becomes Rate Limited

```text
ProviderRateLimitChanged
    ↓
ProviderAvailabilityChanged: AVAILABLE → RATE_LIMITED
    ↓
new selection prefers fallback provider
```

The provider definition remains `ENABLED`.

---

## 120. Circuit Opens After Failures

```text
ProviderOutcomeFeedbackAccepted
    ↓
ProviderHealthChanged: HEALTHY → DEGRADED
    ↓
ProviderCircuitOpened
    ↓
ProviderAvailabilityChanged: DEGRADED → CIRCUIT_OPEN
```

After cooldown:

```text
ProviderCircuitHalfOpened
    ↓ probe success
ProviderCircuitClosed
    ↓
ProviderHealthChanged
    ↓
ProviderAvailabilityChanged
```

---

## 121. Provider Disabled With Drain

```text
ProviderDisabled
    disablePolicy = ALLOW_DRAIN
    ↓
ProviderAvailabilityChanged: AVAILABLE → DRAINING
    ↓
active leases continue
    ↓
ProviderLeaseReleased
    ↓
ProviderAvailabilityChanged: DRAINING → DISABLED
```

No new lease may be granted after `ProviderDisabled`.

---

## 122. Provider Disabled With Immediate Revocation

```text
ProviderDisabled
    disablePolicy = REVOKE_IMMEDIATELY
    ↓
ProviderLeaseRevoked
    ↓
Runtime receives cancellation signal
    ↓
ProviderAvailabilityChanged → DISABLED
```

Physical cancellation may remain best-effort.

---

## 123. Local Model Load

```text
LocalModelRegistered
    ↓
LocalModelInstallStarted
    ↓
LocalModelInstalled
    ↓
LocalModelValidationStarted
    ↓
LocalModelValidated
    ↓
RuntimeResourceAdmissionGranted
    ↓
LocalModelLoadStarted
    ↓
LocalModelLoaded
    ↓
ProviderAvailabilityChanged → AVAILABLE
```

---

## 124. Local Model Load Failure

```text
LocalModelLoadStarted
    ↓
LocalModelFailed
    ↓
ProviderHealthChanged
    ↓
ProviderAvailabilityChanged → UNAVAILABLE
```

A different local model or remote provider may still remain eligible.

---

## 125. Credential Revoked

```text
SecretCredentialRevoked
    ↓
ProviderCredentialAvailabilityChanged → REVOKED
    ↓
ProviderAvailabilityChanged → CREDENTIAL_UNAVAILABLE
    ↓
ProviderLeaseRevoked
```

Lease revocation behavior depends on policy and credential semantics.

---

## 126. Configuration Rotation

```text
ProviderUpdated
    ↓
ProviderClientCreated
    ↓
ProviderClientReplaced
    ↓
old client drains
    ↓
ProviderClientDisposed
```

Historical leases retain the configuration revision they used.

---

# Part XXV — State Consistency Rules

## 127. Event After Durable State

A public event is published only after its corresponding state transition is durable enough to query.

Avoid:

```text
publish ProviderLeaseGranted
    ↓
lease persistence fails
```

Preferred:

```text
persist lease GRANTED
persist handle reference
    ↓
publish ProviderLeaseGranted
```

---

## 128. No Phantom Provider

`ProviderRegistered` must not be published unless `GetProvider` can retrieve the provider.

---

## 129. No Phantom Model

`ProviderModelRegistered` must not be published unless the model is queryable.

---

## 130. No Phantom Lease

`ProviderLeaseGranted` must not be published unless:

- lease state is `GRANTED`;
- provider and model identity are persisted;
- handle reference is resolvable;
- policy and capability snapshots are recorded.

---

## 131. No Premature Local Model Loaded

`LocalModelLoaded` must not be published until the model is actually in `READY`.

File installation alone is insufficient.

---

## 132. No False Health Attribution

A domain validation failure must not publish `ProviderHealthChanged` unless Provider Management has accepted it as provider-relevant evidence.

---

## 133. One Lease Terminal Outcome

One lease cannot be simultaneously:

```text
RELEASED
EXPIRED
REVOKED
REJECTED
FAILED
```

Later audit events may describe the terminal outcome but must not create another incompatible lease terminal state.

---

# Part XXVI — Core Event Invariants

## 134. Invariant 1 — Facts only

Events describe completed provider-management facts.

---

## 135. Invariant 2 — Immutable events

Published events are never edited.

---

## 136. Invariant 3 — Credential isolation

Raw credentials never appear in Provider Management events.

---

## 137. Invariant 4 — Provider-neutral payloads

Provider-native SDK objects and payloads remain internal.

---

## 138. Invariant 5 — Stable provider identity

Provider disablement, archival, model removal, and lease termination do not destroy historical identity.

---

## 139. Invariant 6 — Lease references remain retrievable

A published lease reference must resolve through Provider Management queries.

---

## 140. Invariant 7 — Runtime ownership

Provider Management events do not redefine Runtime work state.

---

## 141. Invariant 8 — Capability-module ownership

Provider Management events do not redefine Translation or Recognition domain state.

---

## 142. Invariant 9 — Availability is normalized

Availability events summarize current provider eligibility but do not replace source state revisions.

---

## 143. Invariant 10 — Health evidence is provider-relevant

Unrelated domain or Presentation failures do not affect provider health events.

---

## 144. Invariant 11 — Idempotent consumers

Duplicate event delivery must not duplicate business effects.

---

## 145. Invariant 12 — Revision-safe projections

Older state revisions cannot overwrite newer projections.

---

## 146. Invariant 13 — Compact payloads

Large catalogs, configurations, evidence, and usage detail remain queryable by reference.

---

## 147. Invariant 14 — Local model events respect Runtime admission

`LocalModelLoadStarted` requires approved resource admission under normal execution.

---

## 148. Invariant 15 — Provider disable blocks new leases

After `ProviderDisabled`, no new `ProviderLeaseGranted` may be published for that provider.

---

# Part XXVII — Open Decisions

## 149. Selection Event Visibility

Choose whether `ProviderSelectionCompleted` is:

```text
PUBLIC
OBSERVABILITY_ONLY
INTERNAL
```

Recommended MVP default:

```text
OBSERVABILITY_ONLY
```

---

## 150. Lease Activation Event

Recommended MVP behavior:

```text
ProviderLeaseActivated
    → internal by default
```

`ProviderLeaseGranted` and terminal lease events remain public.

---

## 151. Health Event Throttling

The exact throttling policy remains open.

Recommended baseline:

- publish only on state transition;
- publish when health revision changes materially;
- do not publish per execution;
- coalesce rapid evidence updates.

---

## 152. Availability Event Throttling

Recommended baseline:

- publish on eligibility change;
- publish on lease eligibility change;
- publish on meaningful retry-after change;
- avoid rapid oscillation through hysteresis.

---

## 153. Rate-Limit Detail

Decide whether events expose:

- exact remaining units;
- normalized remaining class;
- retry-after only.

Recommended default:

```text
normalized remaining class
+
retry-after
```

---

## 154. Usage Event Granularity

Possible options:

```text
PER_EXECUTION
PER_LEASE
PER_PROVIDER_INTERVAL
AGGREGATED_ONLY
```

Recommended MVP:

```text
PER_LEASE
+
periodic aggregate
```

---

## 155. Credential Availability Visibility

Credential availability events may be sensitive.

Recommended visibility:

```text
Provider Management
Runtime
Administration
Observability with redaction
```

Not all capability modules require direct subscription.

---

## 156. Local Model Busy Event

Decide whether `LocalModelBecameBusy` represents:

- any active execution;
- concurrency limit reached;
- material capacity constraint.

Recommended meaning:

```text
material capacity constraint
```

---

# Part XXVIII — Related Documents

```text
02-modules/provider-management/MODULE.md
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/STATES.md
02-modules/provider-management/ERRORS.md
02-modules/provider-management/README.md
```

Architecture references:

```text
docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Runtime references:

```text
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Related module references:

```text
02-modules/translation/EVENTS.md
02-modules/translation/CONTRACT.md
02-modules/recognition/EVENTS.md
02-modules/reading-session/EVENTS.md
02-modules/presentation/EVENTS.md
```

---

## 157. Summary

Provider Management publishes events for:

```text
Provider lifecycle
Provider model lifecycle
Capability changes
Provider selection
Provider lease lifecycle
Availability
Health
Rate limits and quotas
Circuit breakers
Credential availability
Provider clients
Local models
Usage and outcome feedback
```

The core access flow is:

```text
ProviderSelectionCompleted
    ↓
ProviderLeaseGranted
    ↓
ProviderLeaseActivated
    ↓
ProviderUsageRecorded
    ↓
ProviderLeaseReleased
```

Operational protection flows include:

```text
ProviderHealthChanged
ProviderRateLimitChanged
ProviderCircuitOpened
ProviderAvailabilityChanged
ProviderLeaseRevoked
```

Local model flow:

```text
LocalModelRegistered
    ↓
LocalModelInstalled
    ↓
LocalModelValidated
    ↓
LocalModelLoadStarted
    ↓
LocalModelLoaded
    ↓
LocalModelUnloaded
```

Every public event must remain:

- immutable;
- provider-neutral;
- capability-neutral;
- credential-safe;
- compact by default;
- revision-aware;
- idempotently consumable;
- query-backed;
- traceable to stable Provider Management identities.

Events notify consumers that Provider Management state changed.

Queries remain authoritative for retrieving current provider, model, lease, availability, health, circuit, and local-model state.
