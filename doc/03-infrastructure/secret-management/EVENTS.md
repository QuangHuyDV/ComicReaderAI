# Secret Management Events

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Secret Management  
> **Document:** Integration Events  
> **Path:** `03-infrastructure/secret-management/EVENTS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-05  
> **Source of Truth:**
>
> - `03-infrastructure/secret-management/MODULE.md`
> - `03-infrastructure/secret-management/CONTRACT.md`
> - `03-infrastructure/secret-management/STATES.md`
> - `03-infrastructure/configuration/MODULE.md`
> - `03-infrastructure/configuration/CONTRACT.md`
> - `02-modules/provider-management/MODULE.md`
> - `02-modules/provider-management/CONTRACT.md`
> - `02-modules/provider-management/STATES.md`
> - `02-modules/provider-management/EVENTS.md`
> - `docs/architecture/EVENT_BUS.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`

---

## 1. Purpose

This document defines the integration events published and consumed by the Secret Management infrastructure module.

The events communicate facts that have already occurred, including:

- secret descriptor registration and lifecycle changes;
- secret revision activation, supersession, expiration, revocation, and deletion;
- normalized secret availability changes;
- secret lease lifecycle;
- secure backend lifecycle;
- backend lock, unlock, degradation, unavailability, and compromise;
- secret validation outcomes;
- rotation lifecycle and outcomes;
- migration lifecycle and outcomes;
- removal and deletion outcomes;
- user-action requirements;
- uncertain operations and reconciliation;
- security-policy enforcement;
- redaction and exposure prevention;
- safe administrative and observability signals.

These events allow Configuration, Provider Management, Runtime, Administration, Presentation, Observability, and approved infrastructure consumers to react without coupling to Secret Management internals.

This document does not define:

- commands;
- query contracts;
- raw secret transport;
- operating-system keychain callbacks;
- provider-native authentication callbacks;
- detailed state-transition validation;
- detailed error catalogs;
- persistence tables;
- UI wording;
- Runtime work events;
- Provider Management provider events;
- audit-log storage implementation.

---

## 2. Event Principles

### 2.1 Events represent facts

Event names use past-tense semantics.

Correct:

```text
SecretRegistered
SecretRevisionActivated
SecretAvailabilityChanged
SecretLeaseRevoked
SecretBackendLocked
SecretRotationCompleted
```

Incorrect:

```text
RegisterSecret
ActivateRevision
ResolveSecret
RevokeLease
UnlockBackend
RotateSecret
```

The incorrect forms express commands.

---

### 2.2 Events are immutable

After publication, an event must never be modified.

A correction or later transition requires a new event.

---

### 2.3 State commits before publication

Correct order:

```text
Validate transition
    ↓
Persist authoritative state
    ↓
Commit
    ↓
Publish safe event
```

An event must not claim a transition that was not committed.

---

### 2.4 Events never transport secret material

No Secret Management event may contain:

- raw API keys;
- access tokens;
- refresh tokens;
- passwords;
- client secrets;
- private keys;
- certificate private material;
- authorization headers;
- decrypted compound credentials;
- secret handles;
- secret leases containing handles;
- backend decrypted payloads;
- raw environment values;
- provider-native credential objects;
- material fingerprints derived unsafely from the secret;
- raw aliases when policy marks them sensitive.

Sensitive administrative inputs use direct trusted contracts, never Event Bus.

---

### 2.5 Events use safe identity

Events may include:

```text
secretId
safeReference
referenceId
secretRevision
secretLeaseId
secretBackendId
operationId
rotationId
migrationId
validationId
consumerId
providerId
```

Safe reference display must follow redaction policy.

---

### 2.6 Events are compact facts, not snapshots

Events should contain:

- stable identifiers;
- previous and current states when useful;
- revision information;
- reason codes;
- bounded safe metadata;
- references to queryable descriptors;
- timestamps;
- correlation context.

Events should not copy:

- full descriptors;
- full policy definitions;
- full validation evidence;
- full backend configuration;
- full operation histories;
- large audit trails.

---

### 2.7 Events may be delivered more than once

Consumers must be idempotent.

Recommended deduplication keys:

```text
eventId
```

or:

```text
entityId + stateVersion + eventType
```

A duplicate event must not create duplicate revocation, rotation, deletion, or user prompts.

---

### 2.8 Ordering is entity-scoped

Ordering should be preserved per relevant entity where practical:

```text
SecretId
SecretLeaseId
SecretBackendId
SecretRotationId
SecretMigrationId
```

Global ordering across all secrets is not required.

---

### 2.9 Events do not replace queries

Consumers needing current truth must query Secret Management.

Events describe what happened.

They are not guaranteed to contain the complete latest descriptor or policy.

---

### 2.10 Events do not grant authority

Receiving an event does not authorize:

- secret resolution;
- lease acquisition;
- secret enumeration;
- backend unlock;
- rotation;
- removal;
- export.

Authority remains governed by direct contracts and access policy.

---

### 2.11 Events are backend-neutral

Public and shared events must not expose:

- Windows Credential Manager objects;
- macOS Keychain objects;
- Linux Secret Service objects;
- external secret-manager SDK objects;
- platform-native error payloads;
- platform credential file locations.

Normalized backend type and state may be included.

---

### 2.12 Events are consumer-neutral

Secret events must not assume that every secret belongs to Translation.

The same model supports:

- Translation providers;
- Recognition providers;
- remote configuration;
- application authentication;
- future infrastructure consumers;
- session-only tokens;
- certificates and signing keys.

---

## 3. Event Visibility Classes

Secret Management events use explicit visibility.

```text
PUBLIC_INTERNAL
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
AUDIT_ONLY
LOCAL_COMPONENT_ONLY
```

### 3.1 PUBLIC_INTERNAL

Visible to approved application modules through the internal Event Bus.

Examples:

- safe availability changes;
- descriptor registration;
- revision activation;
- rotation completion;
- provider-relevant credential changes.

### 3.2 RESTRICTED_SECURITY

Visible only to explicitly authorized security, lifecycle, and administration subscribers.

Examples:

- backend compromise;
- exposure attempt blocked;
- access-policy violation;
- security revocation;
- suspicious consumer mismatch.

These events must not be broadcast on a general unrestricted topic.

### 3.3 OBSERVABILITY_ONLY

Operational signals consumed by metrics, tracing, or diagnostics.

Examples:

- lease count changes;
- backend initialization duration;
- validation latency class;
- candidate cleanup failure summary.

### 3.4 AUDIT_ONLY

Administrative facts requiring durable restricted audit retention.

Examples:

- secret registered by an actor;
- access policy changed;
- secret removed;
- export approved;
- provider-side revocation requested.

Audit-only events may use a dedicated restricted sink rather than the normal Event Bus.

### 3.5 LOCAL_COMPONENT_ONLY

High-frequency or highly sensitive lifecycle details used only inside Secret Management.

Examples:

- lease request entered evaluation;
- material candidate stored;
- backend handle opened;
- internal buffer cleanup completed.

These should normally remain internal callbacks or local telemetry.

---

## 4. Event Envelope

Secret Management uses the CRAI event envelope.

```text
SecretManagementEventEnvelope {
    eventId
    eventType
    eventVersion

    occurredAt
    publishedAt

    sourceModule = "secret-management"
    sourceComponent?

    correlationId
    causationId?
    operationId?

    applicationInstanceId
    processInstanceId?

    secretId?
    secretRevision?
    secretLeaseId?
    secretBackendId?
    rotationId?
    migrationId?
    validationId?

    sessionId?
    providerId?
    consumerId?

    visibility
    securityClassification

    payload
    metadata
}
```

---

## 5. Envelope Rules

### 5.1 Required fields

Every event requires:

```text
eventId
eventType
eventVersion
occurredAt
publishedAt
sourceModule
correlationId
applicationInstanceId
visibility
securityClassification
payload
```

### 5.2 Optional identity fields

Identity fields are included only when relevant and safe.

### 5.3 Metadata

Metadata must be:

- bounded;
- low-cardinality where used for metrics;
- redacted;
- free from secret material;
- serializable;
- documented when stable.

### 5.4 Security classification

Recommended values:

```text
INTERNAL
CONFIDENTIAL_METADATA
RESTRICTED_SECURITY
```

No event is allowed to carry raw secret material regardless of classification.

---

## 6. Event Type Naming

Canonical format:

```text
secret-management.<entity>.<past-tense-fact>
```

Examples:

```text
secret-management.secret.registered
secret-management.revision.activated
secret-management.availability.changed
secret-management.lease.revoked
secret-management.backend.locked
secret-management.rotation.completed
```

Event types are lowercase and hyphenated where necessary.

Conceptual class names use PascalCase.

---

## 7. Event Versioning

Each event type has an independent version.

```text
eventType: secret-management.rotation.completed
eventVersion: 1
```

Rules:

- additive optional fields may remain compatible;
- removing or changing field meaning requires a new version;
- secret-safety rules cannot be weakened by versioning;
- consumers must ignore unknown optional fields;
- consumers must reject unsupported incompatible major versions safely;
- persisted events retain original meaning.

---

## 8. Common Safe Payload Fields

Events may reuse:

```text
SecretIdentityPayload {
    secretId
    referenceId?
    safeReference?
    kind?
    scope?
    providerId?
    currentRevision?
}
```

```text
StateTransitionPayload {
    previousState
    currentState
    reasonCode
    stateVersion
    changedAt
}
```

```text
OperationIdentityPayload {
    operationId
    operationType
    status
    actorId?
}
```

`actorId` must use an internal safe identity.

---

# Part I — Descriptor Events

## 9. SecretRegistrationStarted

Published after a descriptor enters `REGISTERING`, when external consumers genuinely need to know that creation is in progress.

Event type:

```text
secret-management.secret.registration-started
```

Recommended visibility:

```text
AUDIT_ONLY or LOCAL_COMPONENT_ONLY
```

Payload:

```text
SecretRegistrationStartedPayload {
    secretId
    referenceId
    kind
    scope
    backendId?
    operationId
    startedAt
}
```

Must not include material input.

---

## 10. SecretRegistered

Published after initial descriptor and revision activation succeeds.

Event type:

```text
secret-management.secret.registered
```

Payload:

```text
SecretRegisteredPayload {
    secretId
    referenceId
    safeReference?
    kind
    scope
    backendId
    activeRevision
    persistenceMode
    providerId?
    registeredAt
}
```

Recommended visibility:

```text
PUBLIC_INTERNAL
AUDIT_ONLY copy where required
```

Expected consumers:

- Configuration diagnostics;
- Provider Management;
- Administration;
- Observability.

---

## 11. SecretRegistrationFailed

Published only as a safe operational event when registration failed after an accepted operation.

Event type:

```text
secret-management.secret.registration-failed
```

Payload:

```text
SecretRegistrationFailedPayload {
    operationId
    secretId?
    referenceId?
    normalizedErrorCode
    failureStage
    retryable
    userActionRequired
    failedAt
}
```

Recommended visibility:

```text
OBSERVABILITY_ONLY
AUDIT_ONLY when administrative
```

Detailed error data belongs in error handling, not the event.

---

## 12. SecretSuspended

Published after descriptor state becomes `SUSPENDED`.

Event type:

```text
secret-management.secret.suspended
```

Payload:

```text
SecretSuspendedPayload {
    secretId
    referenceId
    currentRevision?
    previousState
    currentState = SUSPENDED
    reasonCode
    activeLeasePolicy
    suspendedAt
}
```

Recommended visibility:

```text
PUBLIC_INTERNAL
```

Provider Management may stop selecting affected provider paths.

---

## 13. SecretReactivated

Published after explicit recovery returns a suspended or revoked descriptor to `ACTIVE`.

Event type:

```text
secret-management.secret.reactivated
```

Payload:

```text
SecretReactivatedPayload {
    secretId
    referenceId
    previousState
    currentState = ACTIVE
    activeRevision
    recoveryReason
    reactivatedAt
}
```

Reactivation must never imply reuse of revoked material unless policy explicitly verified reinstatement.

---

## 14. SecretRevoked

Published after the descriptor becomes `REVOKED`.

Event type:

```text
secret-management.secret.revoked
```

Payload:

```text
SecretRevokedPayload {
    secretId
    referenceId
    previousState
    currentState = REVOKED
    revokedRevision?
    reasonCode
    revocationClass
    activeLeasePolicy
    providerRevocationStatus?
    revokedAt
}
```

Possible revocation classes:

```text
ADMINISTRATIVE
SECURITY
PROVIDER_REPORTED
POLICY
EXPIRATION_ESCALATION
BACKEND_COMPROMISE
ACCOUNT_DISCONNECTED
```

Recommended visibility:

```text
PUBLIC_INTERNAL
RESTRICTED_SECURITY for security details
AUDIT_ONLY
```

---

## 15. SecretRemovalStarted

Published after descriptor state becomes `REMOVING`.

Event type:

```text
secret-management.secret.removal-started
```

Payload:

```text
SecretRemovalStartedPayload {
    secretId
    referenceId
    previousState
    currentState = REMOVING
    removalMode
    activeLeasePolicy
    providerRevocationRequested
    startedAt
}
```

Recommended visibility:

```text
AUDIT_ONLY
PUBLIC_INTERNAL when consumers must stop use
```

---

## 16. SecretRemoved

Published after descriptor reaches `REMOVED`.

Event type:

```text
secret-management.secret.removed
```

Payload:

```text
SecretRemovedPayload {
    secretId
    referenceId
    previousState
    currentState = REMOVED
    lastRevision
    removalAssurance
    providerRevocationStatus?
    descriptorRetained
    removedAt
}
```

Must not claim physical erasure beyond recorded assurance.

---

## 17. SecretTombstoned

Published after descriptor metadata is reduced to a tombstone.

Event type:

```text
secret-management.secret.tombstoned
```

Payload:

```text
SecretTombstonedPayload {
    secretId
    referenceHash?
    lastRevision
    removalAssurance
    tombstonedAt
    retentionExpiresAt?
}
```

Recommended visibility:

```text
AUDIT_ONLY
```

---

# Part II — Revision Events

## 18. SecretRevisionCandidateCreated

Published only when candidate lifecycle visibility is required.

Event type:

```text
secret-management.revision.candidate-created
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY or OBSERVABILITY_ONLY
```

Payload:

```text
SecretRevisionCandidateCreatedPayload {
    secretId
    candidateRevision
    operationId
    candidateSource
    createdAt
}
```

Must not include candidate material or material-derived fragments.

---

## 19. SecretRevisionReady

Published after a candidate enters `READY`.

Event type:

```text
secret-management.revision.ready
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
```

Payload:

```text
SecretRevisionReadyPayload {
    secretId
    candidateRevision
    validationStatus
    readyAt
}
```

---

## 20. SecretRevisionActivated

Published after a revision becomes authoritative.

Event type:

```text
secret-management.revision.activated
```

Payload:

```text
SecretRevisionActivatedPayload {
    secretId
    referenceId
    previousActiveRevision?
    currentActiveRevision
    activationReason
    backendId
    activatedAt
}
```

Recommended visibility:

```text
PUBLIC_INTERNAL
AUDIT_ONLY
```

Provider Management may rebuild credential-bound clients or create new leases against the new revision.

---

## 21. SecretRevisionSuperseded

Published after an older revision becomes `SUPERSEDED`.

Event type:

```text
secret-management.revision.superseded
```

Payload:

```text
SecretRevisionSupersededPayload {
    secretId
    supersededRevision
    successorRevision
    existingLeasePolicy
    supersededAt
}
```

No new lease may bind to the superseded revision.

---

## 22. SecretRevisionExpired

Published after a revision becomes `EXPIRED`.

Event type:

```text
secret-management.revision.expired
```

Payload:

```text
SecretRevisionExpiredPayload {
    secretId
    referenceId
    revision
    expiresAt
    renewable
    refreshPolicy?
    activeLeasePolicy
    detectedAt
}
```

Expected consumers:

- Provider Management;
- Administration;
- lifecycle automation;
- observability.

---

## 23. SecretRevisionRevoked

Published after a specific revision becomes `REVOKED`.

Event type:

```text
secret-management.revision.revoked
```

Payload:

```text
SecretRevisionRevokedPayload {
    secretId
    revision
    reasonCode
    activeLeasePolicy
    revokedAt
}
```

Recommended visibility:

```text
PUBLIC_INTERNAL
RESTRICTED_SECURITY where applicable
```

---

## 24. SecretRevisionInvalidated

Published after authoritative evidence marks a revision `INVALID`.

Event type:

```text
secret-management.revision.invalidated
```

Payload:

```text
SecretRevisionInvalidatedPayload {
    secretId
    revision
    validationId?
    invalidationClass
    normalizedReasonCode
    descriptorAction
    leaseAction
    invalidatedAt
}
```

Must not include raw validation evidence.

---

## 25. SecretRevisionDeletionStarted

Event type:

```text
secret-management.revision.deletion-started
```

Recommended visibility:

```text
AUDIT_ONLY or LOCAL_COMPONENT_ONLY
```

Payload:

```text
SecretRevisionDeletionStartedPayload {
    secretId
    revision
    backendId
    requestedAssurance
    startedAt
}
```

---

## 26. SecretRevisionDeleted

Event type:

```text
secret-management.revision.deleted
```

Payload:

```text
SecretRevisionDeletedPayload {
    secretId
    revision
    backendId
    effectiveAssurance
    verificationStatus
    deletedAt
}
```

Recommended visibility:

```text
AUDIT_ONLY
```

---

# Part III — Availability Events

## 27. SecretAvailabilityChanged

Published when normalized availability changes materially.

Event type:

```text
secret-management.availability.changed
```

Payload:

```text
SecretAvailabilityChangedPayload {
    secretId
    referenceId
    providerId?
    previousAvailability
    currentAvailability
    revision?
    backendId?
    reasonCode
    requiresUserAction
    retryAfter?
    changedAt
}
```

Possible values:

```text
UNKNOWN
AVAILABLE
UNAVAILABLE
MISSING
LOCKED
EXPIRED
REVOKED
INVALID
BACKEND_UNAVAILABLE
ACCESS_RESTRICTED
ROTATION_REQUIRED
USER_ACTION_REQUIRED
```

Must not include:

- secret values;
- token fragments;
- sensitive aliases;
- raw backend paths;
- provider-native authentication responses.

---

## 28. Availability Event Granularity

Availability events should be published only when:

- state changes;
- reason class changes materially;
- user-action requirement changes;
- retry-after changes enough to affect behavior;
- active revision changes and availability projection changes.

Do not publish on every resolution attempt.

---

## 29. Availability Event Consumers

Consumers may:

- stop selecting a provider credential path;
- update safe UI status;
- request user action;
- invalidate provider-client pools;
- trigger health reevaluation;
- update diagnostics.

Consumers must not:

- resolve material from the event;
- treat availability as access authority;
- infer that a backend contains a specific raw value;
- log sensitive reference aliases.

---

## 30. SecretUserActionRequired

Published when resolution or operation requires user interaction.

Event type:

```text
secret-management.user-action.required
```

Payload:

```text
SecretUserActionRequiredPayload {
    secretId?
    referenceId?
    operationId
    actionType
    reasonCode
    interactionOwner
    expiresAt?
    requestedAt
}
```

Possible action types:

```text
DEVICE_UNLOCK
BIOMETRIC
SYSTEM_PROMPT
APPLICATION_CONFIRMATION
EXTERNAL_AUTH_FLOW
CREDENTIAL_REENTRY
```

Recommended visibility:

```text
PUBLIC_INTERNAL
```

Presentation may react.

The event must not contain prompt secrets or provider authentication URLs containing sensitive parameters.

---

## 31. SecretUserActionResolved

Event type:

```text
secret-management.user-action.resolved
```

Payload:

```text
SecretUserActionResolvedPayload {
    operationId
    actionType
    outcome
    resolvedAt
}
```

Possible outcomes:

```text
COMPLETED
CANCELED
TIMED_OUT
FAILED
```

No user-entered secret value may be included.

---

# Part IV — Lease Events

## 32. Lease Event Visibility Rule

Lease events are sensitive operational metadata.

Default:

```text
SecretLeaseGranted
SecretLeaseActivated
SecretLeaseReleased
    → LOCAL_COMPONENT_ONLY or OBSERVABILITY_ONLY

SecretLeaseExpired
SecretLeaseRevoked
SecretLeaseAbandoned
    → restricted PUBLIC_INTERNAL when consumers must react
```

General modules should not subscribe to all lease activity.

---

## 33. SecretLeaseGranted

Event type:

```text
secret-management.lease.granted
```

Payload:

```text
SecretLeaseGrantedPayload {
    secretLeaseId
    secretId
    revision
    consumerId
    purposeCode
    grantedDuration
    expiresAt
    grantedAt
}
```

Must not include `SecretHandle`.

---

## 34. SecretLeaseActivated

Event type:

```text
secret-management.lease.activated
```

Payload:

```text
SecretLeaseActivatedPayload {
    secretLeaseId
    secretId
    revision
    consumerId
    purposeCode
    activatedAt
}
```

Recommended visibility:

```text
OBSERVABILITY_ONLY
```

---

## 35. SecretLeaseReleased

Event type:

```text
secret-management.lease.released
```

Payload:

```text
SecretLeaseReleasedPayload {
    secretLeaseId
    secretId
    revision
    consumerId
    releaseReason
    activeDuration?
    releasedAt
}
```

Repeated release must not create repeated semantic effects.

---

## 36. SecretLeaseExpired

Event type:

```text
secret-management.lease.expired
```

Payload:

```text
SecretLeaseExpiredPayload {
    secretLeaseId
    secretId
    revision
    consumerId
    purposeCode
    expiredAt
    externalOperationMayContinue
}
```

This event does not imply cancellation of an already accepted remote request.

---

## 37. SecretLeaseRevoked

Event type:

```text
secret-management.lease.revoked
```

Payload:

```text
SecretLeaseRevokedPayload {
    secretLeaseId
    secretId
    revision
    consumerId
    revocationReason
    effectiveAt
    externalOperationMayContinue
}
```

Expected consumers may:

- stop future handle use;
- dispose credential-bound clients;
- cancel provider operations where supported;
- update Runtime cleanup.

---

## 38. SecretLeaseRejected

Event type:

```text
secret-management.lease.rejected
```

Recommended visibility:

```text
OBSERVABILITY_ONLY
```

Payload:

```text
SecretLeaseRejectedPayload {
    operationId
    secretId?
    referenceId?
    consumerId
    purposeCode
    normalizedReasonCode
    userActionRequired
    rejectedAt
}
```

Do not expose hidden secret existence to unauthorized observability consumers.

---

## 39. SecretLeaseAbandoned

Event type:

```text
secret-management.lease.abandoned
```

Payload:

```text
SecretLeaseAbandonedPayload {
    secretLeaseId
    secretId
    revision
    consumerId
    abandonmentReason
    logicalAuthorityRemoved
    cleanupPending
    abandonedAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY or OBSERVABILITY_ONLY
```

---

# Part V — Backend Events

## 40. SecretBackendRegistered

Event type:

```text
secret-management.backend.registered
```

Payload:

```text
SecretBackendRegisteredPayload {
    secretBackendId
    backendType
    capabilitySummary
    registeredAt
}
```

Capability summary must not expose sensitive platform paths or encryption configuration.

---

## 41. SecretBackendInitializationStarted

Event type:

```text
secret-management.backend.initialization-started
```

Recommended visibility:

```text
OBSERVABILITY_ONLY
```

Payload:

```text
SecretBackendInitializationStartedPayload {
    secretBackendId
    backendType
    startedAt
}
```

---

## 42. SecretBackendAvailable

Event type:

```text
secret-management.backend.available
```

Payload:

```text
SecretBackendAvailablePayload {
    secretBackendId
    backendType
    previousState
    currentState = AVAILABLE
    recoveryReason?
    availableAt
}
```

---

## 43. SecretBackendLocked

Event type:

```text
secret-management.backend.locked
```

Payload:

```text
SecretBackendLockedPayload {
    secretBackendId
    backendType
    previousState
    currentState = LOCKED
    reasonCode
    userPresenceMode?
    affectedSecretCountClass?
    lockedAt
}
```

Use a count class or bounded count where disclosure is sensitive.

Must not enumerate secret aliases.

---

## 44. SecretBackendUnlocked

Event type:

```text
secret-management.backend.unlocked
```

Payload:

```text
SecretBackendUnlockedPayload {
    secretBackendId
    previousState = LOCKED
    currentState = AVAILABLE
    unlockMethodClass
    unlockedAt
}
```

Must not include biometric or credential details.

---

## 45. SecretBackendDegraded

Event type:

```text
secret-management.backend.degraded
```

Payload:

```text
SecretBackendDegradedPayload {
    secretBackendId
    backendType
    previousState
    currentState = DEGRADED
    degradedCapabilities[]
    reasonCode
    degradedAt
}
```

---

## 46. SecretBackendUnavailable

Event type:

```text
secret-management.backend.unavailable
```

Payload:

```text
SecretBackendUnavailablePayload {
    secretBackendId
    backendType
    previousState
    currentState = UNAVAILABLE
    normalizedReasonCode
    retryAfter?
    unavailableAt
}
```

---

## 47. SecretBackendCompromised

Event type:

```text
secret-management.backend.compromised
```

Payload:

```text
SecretBackendCompromisedPayload {
    secretBackendId
    backendType
    previousState
    currentState = COMPROMISED
    compromiseClass
    affectedScopeClass
    leaseAction
    descriptorAction
    detectedAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

Detailed evidence must remain in restricted security diagnostics.

---

## 48. SecretBackendShutdownStarted

Event type:

```text
secret-management.backend.shutdown-started
```

Recommended visibility:

```text
OBSERVABILITY_ONLY
```

Payload:

```text
SecretBackendShutdownStartedPayload {
    secretBackendId
    activeLeaseCountClass
    pendingOperationCountClass
    startedAt
}
```

---

## 49. SecretBackendTerminated

Event type:

```text
secret-management.backend.terminated
```

Payload:

```text
SecretBackendTerminatedPayload {
    secretBackendId
    terminationReason
    abandonedLeaseCountClass?
    terminatedAt
}
```

---

# Part VI — Validation Events

## 50. SecretValidationStarted

Event type:

```text
secret-management.validation.started
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY or OBSERVABILITY_ONLY
```

Payload:

```text
SecretValidationStartedPayload {
    validationId
    secretId
    revision
    validationMode
    providerId?
    startedAt
}
```

---

## 51. SecretValidationCompleted

Event type:

```text
secret-management.validation.completed
```

Payload:

```text
SecretValidationCompletedPayload {
    validationId
    secretId
    revision
    validationMode
    status
    providerId?
    expiresAt?
    renewable?
    safeReasonCode?
    checkedAt
}
```

Possible statuses:

```text
VALID
INVALID
UNKNOWN
EXPIRED
REVOKED
UNAVAILABLE
ACCESS_DENIED
VALIDATION_DEFERRED
```

Raw provider evidence is prohibited.

---

## 52. SecretValidationDeferred

Event type:

```text
secret-management.validation.deferred
```

Payload:

```text
SecretValidationDeferredPayload {
    validationId
    secretId
    revision
    deferReason
    userActionRequired
    retryAfter?
    deferredAt
}
```

---

## 53. SecretValidationFailed

Infrastructure-level validation failure.

Event type:

```text
secret-management.validation.failed
```

Payload:

```text
SecretValidationFailedPayload {
    validationId
    secretId
    revision
    validationMode
    normalizedErrorCode
    retryable
    failedAt
}
```

A failed validation operation does not automatically mean the secret is invalid.

---

# Part VII — Rotation Events

## 54. SecretRotationStarted

Published after rotation state enters an accepted running state.

Event type:

```text
secret-management.rotation.started
```

Payload:

```text
SecretRotationStartedPayload {
    rotationId
    operationId
    secretId
    currentRevision
    rotationMode
    activationMode
    existingLeasePolicy
    startedAt
}
```

Recommended visibility:

```text
PUBLIC_INTERNAL
AUDIT_ONLY
```

---

## 55. SecretRotationCandidateReady

Event type:

```text
secret-management.rotation.candidate-ready
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
```

Payload:

```text
SecretRotationCandidateReadyPayload {
    rotationId
    secretId
    candidateRevision
    validationStatus
    readyAt
}
```

---

## 56. SecretRotationActivated

Published after the new revision activates, before all lease-policy cleanup necessarily completes.

Event type:

```text
secret-management.rotation.activated
```

Payload:

```text
SecretRotationActivatedPayload {
    rotationId
    secretId
    previousRevision
    currentRevision
    existingLeasePolicy
    activatedAt
}
```

Provider Management may begin rebuilding clients against the new revision.

---

## 57. SecretRotationCompleted

Event type:

```text
secret-management.rotation.completed
```

Payload:

```text
SecretRotationCompletedPayload {
    rotationId
    operationId
    secretId
    previousRevision
    currentRevision
    activationState
    affectedLeaseCountClass?
    revokedLeaseCountClass?
    requiresConsumerRefresh
    completedAt
}
```

No material fingerprint or token fragment may be included.

---

## 58. SecretRotationPartiallyCompleted

Event type:

```text
secret-management.rotation.partially-completed
```

Payload:

```text
SecretRotationPartiallyCompletedPayload {
    rotationId
    secretId
    activeRevision
    incompleteStage
    normalizedWarningCode
    oldRevisionCleanupPending
    leaseCleanupPending
    reconciliationRequired
    occurredAt
}
```

The active revision field prevents consumers from assuming the entire rotation failed.

---

## 59. SecretRotationFailed

Event type:

```text
secret-management.rotation.failed
```

Payload:

```text
SecretRotationFailedPayload {
    rotationId
    operationId
    secretId
    retainedActiveRevision?
    failureStage
    normalizedErrorCode
    retryable
    candidateCleanupPending
    failedAt
}
```

A safe failure preserves the prior active revision.

---

## 60. SecretRotationCanceled

Event type:

```text
secret-management.rotation.canceled
```

Payload:

```text
SecretRotationCanceledPayload {
    rotationId
    secretId
    retainedActiveRevision
    cancellationStage
    candidateCleanupPending
    canceledAt
}
```

Use only when the outcome is known.

---

## 61. SecretRotationBecameUncertain

Event type:

```text
secret-management.rotation.became-uncertain
```

Payload:

```text
SecretRotationBecameUncertainPayload {
    rotationId
    secretId
    uncertaintyStage
    knownActiveRevision?
    candidateRevision?
    automaticRetryBlocked = true
    reconciliationRequired = true
    occurredAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY
PUBLIC_INTERNAL for lifecycle coordination
```

---

## 62. SecretRotationReconciled

Event type:

```text
secret-management.rotation.reconciled
```

Payload:

```text
SecretRotationReconciledPayload {
    rotationId
    secretId
    resolution
    activeRevision?
    cleanupRequired
    reconciledAt
}
```

Possible resolutions:

```text
ROTATION_CONFIRMED
ROTATION_NOT_APPLIED
CANDIDATE_ONLY
MANUAL_ACTION_REQUIRED
STILL_UNCERTAIN
```

---

# Part VIII — Migration Events

## 63. SecretMigrationStarted

Event type:

```text
secret-management.migration.started
```

Payload:

```text
SecretMigrationStartedPayload {
    migrationId
    operationId
    secretId
    revision
    sourceBackendId
    destinationBackendId
    existingLeasePolicy
    startedAt
}
```

Recommended visibility:

```text
AUDIT_ONLY
PUBLIC_INTERNAL when consumers must freeze access
```

---

## 64. SecretMigrationDestinationValidated

Event type:

```text
secret-management.migration.destination-validated
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
```

Payload:

```text
SecretMigrationDestinationValidatedPayload {
    migrationId
    secretId
    revision
    destinationBackendId
    validatedAt
}
```

---

## 65. SecretMigrationSwitched

Published after descriptor binding switches to the destination.

Event type:

```text
secret-management.migration.switched
```

Payload:

```text
SecretMigrationSwitchedPayload {
    migrationId
    secretId
    revision
    previousBackendId
    currentBackendId
    sourceCleanupPending
    switchedAt
}
```

---

## 66. SecretMigrationCompleted

Event type:

```text
secret-management.migration.completed
```

Payload:

```text
SecretMigrationCompletedPayload {
    migrationId
    operationId
    secretId
    revision
    sourceBackendId
    destinationBackendId
    sourceCleanupAssurance
    completedAt
}
```

---

## 67. SecretMigrationPartiallyCompleted

Event type:

```text
secret-management.migration.partially-completed
```

Payload:

```text
SecretMigrationPartiallyCompletedPayload {
    migrationId
    secretId
    activeBackendId
    incompleteStage
    sourceCleanupPending
    normalizedWarningCode
    reconciliationRequired
    occurredAt
}
```

---

## 68. SecretMigrationFailed

Event type:

```text
secret-management.migration.failed
```

Payload:

```text
SecretMigrationFailedPayload {
    migrationId
    secretId
    retainedBackendId?
    failureStage
    normalizedErrorCode
    retryable
    destinationCleanupPending
    failedAt
}
```

---

## 69. SecretMigrationBecameUncertain

Event type:

```text
secret-management.migration.became-uncertain
```

Payload:

```text
SecretMigrationBecameUncertainPayload {
    migrationId
    secretId
    uncertaintyStage
    knownBackendId?
    automaticRetryBlocked = true
    reconciliationRequired = true
    occurredAt
}
```

---

## 70. SecretMigrationReconciled

Event type:

```text
secret-management.migration.reconciled
```

Payload:

```text
SecretMigrationReconciledPayload {
    migrationId
    secretId
    activeBackendId?
    sourcePresent?
    destinationPresent?
    resolution
    cleanupRequired
    reconciledAt
}
```

Presence values must be safe booleans and must not reveal secret material.

---

# Part IX — Removal Events

## 71. SecretExternalRevocationRequested

Event type:

```text
secret-management.external-revocation.requested
```

Recommended visibility:

```text
AUDIT_ONLY
```

Payload:

```text
SecretExternalRevocationRequestedPayload {
    operationId
    secretId
    providerId?
    revision
    requestedAt
}
```

---

## 72. SecretExternalRevocationCompleted

Event type:

```text
secret-management.external-revocation.completed
```

Payload:

```text
SecretExternalRevocationCompletedPayload {
    operationId
    secretId
    providerId?
    revision
    status
    completedAt
}
```

Possible statuses:

```text
CONFIRMED
NOT_SUPPORTED
NOT_REQUIRED
```

---

## 73. SecretExternalRevocationFailed

Event type:

```text
secret-management.external-revocation.failed
```

Payload:

```text
SecretExternalRevocationFailedPayload {
    operationId
    secretId
    providerId?
    revision
    normalizedErrorCode
    uncertain
    retryable
    failedAt
}
```

---

## 74. SecretRemovalPartiallyCompleted

Event type:

```text
secret-management.secret.removal-partially-completed
```

Payload:

```text
SecretRemovalPartiallyCompletedPayload {
    operationId
    secretId
    descriptorState
    materialDeletionStatus
    externalRevocationStatus
    tombstoneStatus
    reconciliationRequired
    occurredAt
}
```

---

# Part X — Operation and Reconciliation Events

## 75. SecretOperationDeferred

Event type:

```text
secret-management.operation.deferred
```

Payload:

```text
SecretOperationDeferredPayload {
    operationId
    operationType
    secretId?
    deferReason
    userActionRequired
    retryAfter?
    deferredAt
}
```

---

## 76. SecretOperationBecameUncertain

Generic event for operations other than rotation or migration.

Event type:

```text
secret-management.operation.became-uncertain
```

Payload:

```text
SecretOperationBecameUncertainPayload {
    operationId
    operationType
    secretId?
    uncertaintyStage
    automaticRetryBlocked
    reconciliationRequired
    occurredAt
}
```

---

## 77. SecretReconciliationRequired

Event type:

```text
secret-management.reconciliation.required
```

Payload:

```text
SecretReconciliationRequiredPayload {
    operationId
    operationType
    secretId?
    reconciliationId
    reasonCode
    recommendedAction
    requiredAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 78. SecretReconciliationCompleted

Event type:

```text
secret-management.reconciliation.completed
```

Payload:

```text
SecretReconciliationCompletedPayload {
    reconciliationId
    operationId
    operationType
    secretId?
    resolution
    resultingDescriptorState?
    resultingRevision?
    resultingBackendId?
    completedAt
}
```

---

## 79. SecretManualActionRequired

Used when reconciliation cannot complete automatically.

Event type:

```text
secret-management.reconciliation.manual-action-required
```

Payload:

```text
SecretManualActionRequiredPayload {
    reconciliationId
    operationId
    secretId?
    reasonCode
    safeActionHints[]
    requiredAt
}
```

No secret value or unsafe provider URL may appear.

---

# Part XI — Security Events

## 80. SecretAccessDenied

Published only when needed for restricted security monitoring or audit.

Event type:

```text
secret-management.access.denied
```

Payload:

```text
SecretAccessDeniedPayload {
    operationId
    secretId?
    referenceId?
    consumerId
    purposeCode
    denialClass
    existenceHidden
    deniedAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY
```

Do not reveal secret existence when `existenceHidden = true`.

---

## 81. SecretConsumerMismatchDetected

Event type:

```text
secret-management.security.consumer-mismatch-detected
```

Payload:

```text
SecretConsumerMismatchDetectedPayload {
    operationId
    secretLeaseId?
    secretId?
    expectedConsumerId?
    actualConsumerId
    attemptedPurposeCode
    blocked
    detectedAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 82. SecretPurposeViolationDetected

Event type:

```text
secret-management.security.purpose-violation-detected
```

Payload:

```text
SecretPurposeViolationDetectedPayload {
    operationId
    secretLeaseId?
    secretId?
    consumerId
    grantedPurposeCode?
    attemptedPurposeCode
    blocked
    detectedAt
}
```

---

## 83. SecretExposureBlocked

Published when an operation attempted to expose material through a prohibited boundary and was blocked.

Event type:

```text
secret-management.security.exposure-blocked
```

Payload:

```text
SecretExposureBlockedPayload {
    operationId?
    secretId?
    consumerId?
    boundaryType
    findingClass
    blocked = true
    detectedAt
}
```

Possible boundary types:

```text
EVENT
LOG
TRACE
METRIC
DIAGNOSTIC
CONFIGURATION
UI
CLIPBOARD
FILE
EXCEPTION
SERIALIZATION
CHILD_PROCESS
```

Possible finding classes:

```text
KNOWN_SECRET_VALUE
AUTHORIZATION_HEADER
PRIVATE_KEY_BLOCK
TOKEN_PATTERN
PASSWORD_FIELD
SENSITIVE_QUERY_PARAMETER
UNSAFE_REFERENCE
UNKNOWN_HIGH_ENTROPY_VALUE
```

The event must not include the matched value.

---

## 84. SecretExportApproved

Event type:

```text
secret-management.export.approved
```

Recommended visibility:

```text
AUDIT_ONLY
```

Payload:

```text
SecretExportApprovedPayload {
    operationId
    secretId
    revision
    consumerId
    targetType
    targetIdentityClass
    purposeCode
    expiresAt?
    approvedAt
}
```

---

## 85. SecretExportBlocked

Event type:

```text
secret-management.export.blocked
```

Payload:

```text
SecretExportBlockedPayload {
    operationId
    secretId?
    consumerId
    targetType
    purposeCode
    denialClass
    blockedAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 86. SecretPolicyChanged

Published after access policy changes.

Event type:

```text
secret-management.policy.changed
```

Payload:

```text
SecretPolicyChangedPayload {
    secretId
    previousPolicyRevision
    currentPolicyRevision
    changeClass
    activeLeaseAction
    changedAt
}
```

No full policy or consumer list should be copied into the event by default.

---

# Part XII — Redaction Events

## 87. SecretRedactionFindingDetected

High-frequency findings should normally be aggregated.

Event type:

```text
secret-management.redaction.finding-detected
```

Recommended visibility:

```text
OBSERVABILITY_ONLY or RESTRICTED_SECURITY
```

Payload:

```text
SecretRedactionFindingDetectedPayload {
    operationId?
    boundaryType
    findingClass
    blocked
    sourceComponent
    detectedAt
}
```

No matched text may be included.

---

## 88. SecretRedactionFailureDetected

Published when redaction infrastructure itself fails.

Event type:

```text
secret-management.redaction.failure-detected
```

Payload:

```text
SecretRedactionFailureDetectedPayload {
    operationId?
    boundaryType
    failureClass
    outputBlocked
    detectedAt
}
```

Recommended visibility:

```text
RESTRICTED_SECURITY
```

Fail-safe behavior should normally block sensitive output.

---

# Part XIII — Consumed Events

## 89. Events Consumed by Secret Management

Secret Management may consume safe events from other modules.

It must not accept raw secret material through those events.

---

## 90. Configuration Revision Activated

Potential source event:

```text
configuration.snapshot.activated
```

Secret Management may react when:

- secret-reference syntax changes;
- access-policy references change;
- backend configuration changes;
- environment-reference policy changes;
- validation policy changes.

Configuration events contain references only.

They do not carry secret material.

---

## 91. Provider Authentication Outcome Reported

Potential source event:

```text
provider-management.credential.authentication-outcome-reported
```

Preferred integration remains a direct command when correctness depends on acceptance.

An event may carry a normalized fact:

```text
ProviderCredentialAuthenticationOutcomePayload {
    providerId
    credentialReferenceId
    credentialRevision
    outcomeClass
    occurredAt
}
```

Secret Management may update validation evidence or availability.

It must not infer global invalidity from one unrelated provider failure.

---

## 92. Provider Client Disposed

Potential source event:

```text
provider-management.client.disposed
```

Secret Management may use it to confirm that a credential-bound client no longer holds lease authority.

Lease release should still use the direct lease contract.

---

## 93. Runtime Attempt Canceled

Potential source event:

```text
runtime.attempt.canceled
```

Secret Management may trigger cleanup only when the event safely references a known lease or operation.

Direct cancellation propagation remains preferred for timely cleanup.

---

## 94. Application Shutdown Started

Potential source event:

```text
application.shutdown.started
```

Secret Management should:

- stop new leases;
- release or revoke active leases;
- cancel safe candidates;
- preserve uncertain operation records;
- clear memory caches;
- shut down backends.

---

## 95. Network Availability Changed

Potential source event:

```text
platform.network.availability-changed
```

May trigger:

- deferred validation;
- external backend recovery;
- refresh retry eligibility;
- reconciliation attempts.

Network restoration must not automatically retry non-idempotent uncertain rotation.

---

## 96. User Presence Completed

Potential direct result or restricted event:

```text
presentation.user-presence.completed
```

The payload must contain only:

- operation identity;
- outcome;
- trusted platform result reference.

It must not contain user-entered secret material.

Secret material entry uses a direct trusted channel.

---

# Part XIV — Consumer Guidance

## 97. Configuration Consumer Rules

Configuration may consume:

- `SecretRegistered`;
- `SecretRemoved`;
- `SecretAvailabilityChanged`;
- `SecretPolicyChanged`.

Configuration may update diagnostics or reference validation.

It must not persist material.

---

## 98. Provider Management Consumer Rules

Provider Management may consume:

- `SecretAvailabilityChanged`;
- `SecretRevisionActivated`;
- `SecretRevisionExpired`;
- `SecretRevisionRevoked`;
- `SecretRevoked`;
- `SecretLeaseRevoked`;
- `SecretRotationActivated`;
- `SecretRotationCompleted`;
- `SecretBackendCompromised`.

Provider Management may:

- stop selecting paths;
- rebuild clients;
- drain or revoke provider leases;
- reevaluate provider availability;
- request a new secret lease.

It must not resolve raw credentials from events.

---

## 99. Runtime Consumer Rules

Runtime may consume:

- lease revocation;
- backend compromise;
- operation uncertainty;
- application shutdown coordination.

Runtime may:

- cancel affected work;
- stop dispatch;
- trigger cleanup.

Runtime must not carry secret material in WorkItems.

---

## 100. Presentation Consumer Rules

Presentation may consume:

- `SecretUserActionRequired`;
- safe availability changes;
- safe operation outcomes;
- safe validation summaries.

Presentation must not receive:

- secret handles;
- raw backend objects;
- raw provider authentication payloads;
- secret values in events.

Secret entry occurs through a trusted direct form boundary.

---

## 101. Observability Consumer Rules

Observability may consume:

- normalized operation outcomes;
- durations;
- state transitions;
- counts;
- error codes;
- security finding classes.

It must not store:

- raw aliases where sensitive;
- account emails;
- material fingerprints;
- secret values;
- authorization data;
- full provider responses.

---

# Part XV — Publication and Delivery

## 102. Transactional Publication

Where durable state exists, publication should use an outbox or equivalent safe mechanism:

```text
Commit state + pending event record
    ↓
Publish event
    ↓
Mark event delivered
```

The Event Bus itself may remain in-process for MVP, but authoritative state must not depend on subscriber success.

---

## 103. Subscriber Failure

Subscriber failure must not roll back committed Secret Management state.

Failures should be:

- isolated;
- observable;
- retried only under Event Bus policy;
- prevented from exposing secret metadata;
- prevented from blocking security revocation.

Critical local security actions should not rely solely on asynchronous subscribers.

---

## 104. Delivery Semantics

MVP may use:

```text
in-process
typed events
asynchronous handlers
at-most-once or best-effort delivery
```

Consumers needing correctness must query current state after receiving an event.

Future durable infrastructure may support stronger delivery.

Event schemas must remain compatible with duplicate delivery.

---

## 105. Priority

Suggested priorities:

```text
CRITICAL
    backend compromised
    secret revoked for security
    lease revoked for security
    exposure blocked
    policy violation

HIGH
    active revision expired
    availability became revoked or invalid
    rotation became uncertain
    migration became uncertain

NORMAL
    secret registered
    revision activated
    rotation completed
    migration completed
    backend available

LOW
    observability-only lease lifecycle
    validation started
    candidate cleanup
```

Priority does not permit bypassing visibility restrictions.

---

## 106. Throttling

Throttle or aggregate:

- repeated backend unavailable checks;
- repeated lock observations;
- repeated access denials from the same safe identity;
- lease metrics;
- redaction findings;
- repeated availability recomputation with unchanged state;
- validation progress.

Never throttle away the first critical security transition.

---

## 107. Coalescing

Allowed:

```text
BackendUnavailable repeated 100 times
    → one state-change event + aggregated metric

Availability recomputed but unchanged
    → no event
```

Not allowed:

```text
SecretRevoked
SecretRevisionActivated
SecretRemoved
BackendCompromised
```

These distinct facts must remain explicit.

---

## 108. Event Retention

Retention depends on visibility:

```text
PUBLIC_INTERNAL
    short or bounded operational retention

OBSERVABILITY_ONLY
    telemetry retention policy

RESTRICTED_SECURITY
    restricted security retention

AUDIT_ONLY
    durable audit retention

LOCAL_COMPONENT_ONLY
    normally no durable retention
```

Retention stores only safe event payloads.

---

# Part XVI — Privacy and Security Validation

## 109. Pre-Publication Inspection

Every event must pass:

```text
Schema validation
    ↓
Visibility validation
    ↓
Sensitive-type rejection
    ↓
Redaction inspection
    ↓
Metadata cardinality check
    ↓
Publish
```

---

## 110. Sensitive Type Rejection

Serialization must reject payload fields typed as:

```text
SecretMaterialInput
SecretHandle
SecretLease with handle
SecureBuffer
PrivateKeyMaterial
AuthorizationObject
PlatformCredentialObject
DecryptedBackendEntry
```

---

## 111. Unsafe Field Names

Event schema review should reject or scrutinize fields such as:

```text
apiKey
accessToken
refreshToken
password
clientSecret
privateKey
authorization
rawCredential
secretValue
decryptedPayload
tokenBody
```

A normalized boolean or state field may use a related name only when clearly non-material, for example:

```text
accessTokenExpired = true
```

---

## 112. Reference Privacy

`safeReference` is optional.

When aliases are sensitive, use:

```text
referenceId
referenceHash
displayClass
```

instead of full canonical reference.

---

## 113. Account Metadata

Events should not expose full account emails, tenant names, or user identifiers by default.

Use safe hints only where explicitly approved.

---

# Part XVII — Idempotency and Race Handling

## 114. Duplicate Event Handling

Consumers should track:

```text
eventId
entity stateVersion
revision
lease terminal state
operation terminal state
```

Examples:

- duplicate `SecretLeaseRevoked` must not revoke twice;
- duplicate `SecretRevisionActivated` must not rebuild unlimited client pools;
- duplicate `SecretUserActionRequired` must not open repeated prompts;
- duplicate `SecretRemoved` must not trigger repeated provider revocation.

---

## 115. Out-of-Order Events

Consumers compare:

- state version;
- revision;
- occurredAt;
- entity identity.

Example:

```text
Revision R5 activated
then delayed R4 superseded event arrives
```

The consumer must not revert to R4.

---

## 116. Availability Race

Availability may change rapidly:

```text
LOCKED → AVAILABLE → BACKEND_UNAVAILABLE
```

Consumers should use the event as a hint and query current state before an important action.

---

## 117. Rotation Race

Possible event order by entity:

```text
SecretRotationStarted
SecretRevisionActivated
SecretRevisionSuperseded
SecretRotationCompleted
```

`SecretRevisionActivated` may precede cleanup completion.

Consumers must distinguish activation from full rotation completion.

---

## 118. Removal Race

`SecretRemoved` must not be published before new access is blocked and material-deletion outcome is known enough to report safe state.

A later `SecretRemovalPartiallyCompleted` or cleanup event may clarify external revocation or tombstone status.

---

# Part XVIII — Event Catalog Summary

## 119. Descriptor Events

```text
SecretRegistrationStarted
SecretRegistered
SecretRegistrationFailed
SecretSuspended
SecretReactivated
SecretRevoked
SecretRemovalStarted
SecretRemoved
SecretTombstoned
```

## 120. Revision Events

```text
SecretRevisionCandidateCreated
SecretRevisionReady
SecretRevisionActivated
SecretRevisionSuperseded
SecretRevisionExpired
SecretRevisionRevoked
SecretRevisionInvalidated
SecretRevisionDeletionStarted
SecretRevisionDeleted
```

## 121. Availability and User Action Events

```text
SecretAvailabilityChanged
SecretUserActionRequired
SecretUserActionResolved
```

## 122. Lease Events

```text
SecretLeaseGranted
SecretLeaseActivated
SecretLeaseReleased
SecretLeaseExpired
SecretLeaseRevoked
SecretLeaseRejected
SecretLeaseAbandoned
```

## 123. Backend Events

```text
SecretBackendRegistered
SecretBackendInitializationStarted
SecretBackendAvailable
SecretBackendLocked
SecretBackendUnlocked
SecretBackendDegraded
SecretBackendUnavailable
SecretBackendCompromised
SecretBackendShutdownStarted
SecretBackendTerminated
```

## 124. Validation Events

```text
SecretValidationStarted
SecretValidationCompleted
SecretValidationDeferred
SecretValidationFailed
```

## 125. Rotation Events

```text
SecretRotationStarted
SecretRotationCandidateReady
SecretRotationActivated
SecretRotationCompleted
SecretRotationPartiallyCompleted
SecretRotationFailed
SecretRotationCanceled
SecretRotationBecameUncertain
SecretRotationReconciled
```

## 126. Migration Events

```text
SecretMigrationStarted
SecretMigrationDestinationValidated
SecretMigrationSwitched
SecretMigrationCompleted
SecretMigrationPartiallyCompleted
SecretMigrationFailed
SecretMigrationBecameUncertain
SecretMigrationReconciled
```

## 127. Removal and External Revocation Events

```text
SecretExternalRevocationRequested
SecretExternalRevocationCompleted
SecretExternalRevocationFailed
SecretRemovalPartiallyCompleted
```

## 128. Operation and Reconciliation Events

```text
SecretOperationDeferred
SecretOperationBecameUncertain
SecretReconciliationRequired
SecretReconciliationCompleted
SecretManualActionRequired
```

## 129. Security and Redaction Events

```text
SecretAccessDenied
SecretConsumerMismatchDetected
SecretPurposeViolationDetected
SecretExposureBlocked
SecretExportApproved
SecretExportBlocked
SecretPolicyChanged
SecretRedactionFindingDetected
SecretRedactionFailureDetected
```

---

# Part XIX — MVP Event Boundary

## 130. Required MVP Events

The desktop MVP should implement:

```text
SecretRegistered
SecretRevisionActivated
SecretRevisionSuperseded
SecretRevisionExpired
SecretRevisionRevoked
SecretAvailabilityChanged
SecretLeaseRevoked
SecretLeaseExpired
SecretBackendAvailable
SecretBackendLocked
SecretBackendUnavailable
SecretBackendCompromised
SecretValidationCompleted
SecretRotationStarted
SecretRotationCompleted
SecretRotationFailed
SecretRotationBecameUncertain
SecretRevoked
SecretRemovalStarted
SecretRemoved
SecretUserActionRequired
SecretExposureBlocked
```

---

## 131. Optional MVP Operational Events

```text
SecretLeaseGranted
SecretLeaseActivated
SecretLeaseReleased
SecretValidationStarted
SecretBackendInitializationStarted
SecretRotationCandidateReady
SecretRevisionCandidateCreated
```

These may remain local telemetry.

---

## 132. Deferred Events

May be deferred:

```text
advanced external secret-manager events
hardware security module events
cross-device synchronization events
organization policy events
encrypted backup events
complex certificate lifecycle events
child-process transfer events
automatic provider provisioning events
```

---

# Part XX — Event Decisions

## 133. Decisions

### Decision 1 — No raw material in any event

Visibility class never permits secret material.

### Decision 2 — Facts publish after commit

State remains authoritative.

### Decision 3 — Availability is the main shared integration signal

Consumers use normalized availability rather than backend internals.

### Decision 4 — Lease events are restricted by default

High-frequency access metadata is not broadcast widely.

### Decision 5 — Security events use restricted channels

Compromise, mismatch, export, and exposure events are not general public events.

### Decision 6 — Audit and Event Bus may be separate

Durable administrative audit may use a dedicated restricted sink.

### Decision 7 — Activation and completion are distinct

Rotation or migration activation can occur before cleanup completes.

### Decision 8 — Uncertainty is explicit

Potentially committed operations publish uncertainty and block blind retry.

### Decision 9 — User-action events carry no user input

They contain only action class and operation identity.

### Decision 10 — References are redacted by policy

Full canonical references are optional, not automatic.

### Decision 11 — Provider Management receives normalized credential facts

It never receives material through events.

### Decision 12 — Events do not grant access

Every material access still requires direct policy evaluation and lease acquisition.

---

# Part XXI — Open Decisions

## 134. Visibility Decisions

Still to finalize:

- exact subscribers for lease events;
- whether `SecretRegistered` is general internal or administration-only;
- whether backend lock events are visible to Presentation directly;
- whether access-denied events use security sink only;
- exact audit/event dual-write strategy;
- provider-specific event routing.

---

## 135. Granularity Decisions

Still to finalize:

- validation progress events;
- rotation progress granularity;
- migration progress granularity;
- cleanup events;
- tombstone events;
- external revocation progress;
- backend capability-change events;
- active lease count events.

---

## 136. Retention Decisions

Still to finalize:

- audit retention;
- security-event retention;
- lease-event retention;
- local telemetry buffer size;
- event redaction review retention;
- reconciliation history retention.

---

## 137. Delivery Decisions

Still to finalize:

- MVP at-most-once versus local retry;
- persistent outbox timing;
- restricted security channel implementation;
- audit sink durability;
- subscriber authorization;
- event encryption at rest;
- cross-process event transport.

---

# Part XXII — Related Documents

## 138. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/configuration/MODULE.md
03-infrastructure/configuration/CONTRACT.md

03-infrastructure/secret-management/MODULE.md
03-infrastructure/secret-management/CONTRACT.md
03-infrastructure/secret-management/STATES.md

02-modules/provider-management/MODULE.md
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/STATES.md
02-modules/provider-management/EVENTS.md
```

Future Secret Management documents:

```text
03-infrastructure/secret-management/ERRORS.md
03-infrastructure/secret-management/README.md
```

---

## 139. Summary

Secret Management events communicate safe lifecycle facts while preserving the strict boundary around secret material.

The publication flow is:

```text
State transition validated
    ↓
Authoritative state committed
    ↓
Payload schema validated
    ↓
Sensitive types rejected
    ↓
Redaction inspection
    ↓
Visibility selected
    ↓
Event published
```

The principal shared integration flow is:

```text
Secret state changes
    ↓
SecretAvailabilityChanged
    ↓
Provider Management / Configuration / Presentation react
    ↓
Consumer queries current safe state
    ↓
Any actual access uses direct lease contract
```

The event model guarantees:

- events represent past facts;
- events are immutable;
- state commits before publication;
- raw secret material never enters Event Bus;
- events never grant access authority;
- availability is normalized;
- lease metadata is restricted;
- security signals use restricted visibility;
- audit may use a separate durable sink;
- activation and cleanup completion remain distinguishable;
- uncertain external outcomes are explicit;
- consumers are idempotent;
- safe identity and revision preserve traceability;
- backend-native details remain internal.

This document is the event source of truth for subsequent Secret Management errors and implementation documentation.
