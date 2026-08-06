# Secret Management Contract

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Secret Management  
> **Document:** Public and Internal Contracts  
> **Path:** `03-infrastructure/secret-management/CONTRACT.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-05  
> **Source of Truth:**
>
> - `03-infrastructure/secret-management/MODULE.md`
> - `03-infrastructure/configuration/MODULE.md`
> - `03-infrastructure/configuration/CONTRACT.md`
> - `02-modules/provider-management/MODULE.md`
> - `02-modules/provider-management/CONTRACT.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/RESOURCE_LIFECYCLE.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`

---

## 1. Purpose

This document defines the contracts exposed and consumed by the Secret Management infrastructure module.

It specifies:

- secret reference contracts;
- secret descriptor contracts;
- secret creation and replacement commands;
- secret resolution contracts;
- secret lease contracts;
- secret availability queries;
- secret validation contracts;
- secret rotation contracts;
- secret revocation and deletion contracts;
- secret backend registration contracts;
- access policy and consumer identity;
- redaction and safe diagnostics;
- concurrency and idempotency expectations;
- persistence and serialization boundaries;
- contract versioning;
- cross-module ownership boundaries.

This document does not define:

- concrete operating-system keychain APIs;
- encryption algorithms or key derivation parameters;
- provider-native authentication payloads;
- UI forms;
- Provider Management selection policy;
- Configuration source precedence;
- Runtime work scheduling;
- detailed state machines;
- event schemas;
- detailed error catalogs;
- database tables;
- implementation classes.

Detailed lifecycles belong in `STATES.md`.

Integration events belong in `EVENTS.md`.

Normalized failures belong in `ERRORS.md`.

---

## 2. Contract Goals

Secret Management contracts must:

1. prevent raw secret values from crossing public architecture boundaries;
2. make secret identity stable and explicit;
3. separate secret identity from secret material;
4. separate secret metadata from secret material;
5. restrict resolution to authorized consumers;
6. make access purpose-scoped and time-bounded;
7. support operating-system secure storage in the desktop MVP;
8. support memory-only and environment-backed secret sources where policy permits;
9. support future external secret-manager backends;
10. support rotation without silently mutating historical meaning;
11. support safe revocation;
12. support deterministic redaction;
13. provide machine-readable availability without exposing secret values;
14. preserve traceability without leaking sensitive material;
15. keep contracts backend-neutral;
16. remain serializable except for explicitly non-serializable secret handles;
17. prevent accidental secret inclusion in logs, events, snapshots, exceptions, and diagnostics.

---

## 3. Architectural Boundary

```text
Configuration
    stores SecretReference
            │
            ▼
Secret Management
    validates identity and policy
    stores or locates secret material
    resolves approved access
    returns bounded SecretLease / SecretHandle
            │
            ▼
Approved Internal Consumer
    Provider Management
    Authentication Adapter
    Remote Configuration Adapter
    Other explicitly authorized infrastructure
```

Raw secret material must remain inside approved infrastructure boundaries.

Normal feature, application, presentation, event, telemetry, and persistence contracts must use references or safe descriptors only.

---

## 4. Contract Classification

Secret Management defines four contract classes.

### 4.1 Public safe contracts

Safe to serialize and expose across internal module boundaries:

- `SecretReference`;
- `SecretDescriptor`;
- `SecretAvailability`;
- `SecretCapability`;
- `SecretRevisionReference`;
- `SecretValidationSummary`;
- `SecretAccessPolicySummary`;
- `SecretOperationReceipt`;
- normalized error references.

These contracts never contain secret material.

### 4.2 Administrative command contracts

Used by trusted application services or settings flows:

- register secret;
- replace secret;
- rotate secret;
- revoke secret;
- remove secret;
- update metadata;
- validate secret;
- migrate backend.

Command payloads that carry secret material are sensitive transport objects and must never be logged, persisted as ordinary command history, or published through Event Bus.

### 4.3 Internal resolution contracts

Used only by approved infrastructure consumers:

- resolve secret;
- acquire secret lease;
- use secret handle;
- release secret lease;
- refresh renewable credential;
- resolve compound credential.

These contracts are not general-purpose application APIs.

### 4.4 Backend adapter contracts

Implemented by secure-storage adapters:

- store material;
- retrieve material;
- replace material;
- delete material;
- inspect safe metadata;
- test availability;
- enumerate safe entries where supported;
- lock and unlock backend;
- migrate entry.

---

## 5. Core Identifiers

```text
SecretId
SecretReferenceId
SecretRevision
SecretLeaseId
SecretBackendId
SecretConsumerId
SecretAccessPolicyId
SecretOperationId
SecretValidationId
SecretRotationId
CorrelationId
CausationId
ApplicationInstanceId
SessionId?
ProviderId?
```

Rules:

- identifiers must be opaque;
- identifiers must not embed raw secret values;
- identifiers should not reveal user names, tokens, account numbers, or provider keys;
- stable identifiers may remain visible in logs when policy allows;
- secret revisions are monotonic within one `SecretId`;
- a new secret identity requires a new `SecretId`;
- a replacement of material for the same logical secret creates a new revision.

---

## 6. SecretReference

`SecretReference` is the normal cross-module representation of a secret dependency.

```text
SecretReference {
    referenceId
    scheme
    namespace
    alias
    expectedKind?
    expectedProviderId?
    expectedAccountId?
    minimumRevision?
    accessPolicyId?
}
```

Example forms:

```text
secret://translation/primary
os-keychain://crai/translation/primary
env://CRAI_TRANSLATION_API_KEY
memory://session/provider-token
```

### 6.1 Required properties

A reference must:

- be parseable without resolving secret material;
- identify an approved scheme;
- identify one namespace and alias;
- be safe to serialize;
- be safe to include in configuration after redaction policy;
- never contain secret material in query strings, fragments, user info, or path segments.

### 6.2 Forbidden reference forms

```text
secret://translation/sk-actual-key
https://user:password@example.com
env://API_KEY?fallback=raw-secret
memory://token/<actual-token>
```

### 6.3 Reference equality

Reference equality is based on normalized identity fields, not display formatting.

```text
normalize(referenceA) == normalize(referenceB)
```

must indicate the same logical lookup target.

Case sensitivity is scheme-defined.

---

## 7. Secret Kind

`SecretKind` describes the semantic shape of secret material.

```text
API_KEY
ACCESS_TOKEN
REFRESH_TOKEN
PASSWORD
CLIENT_SECRET
PRIVATE_KEY
CERTIFICATE
SIGNING_KEY
ENCRYPTION_KEY
SESSION_TOKEN
COMPOUND_CREDENTIAL
OPAQUE_SECRET
```

`SecretKind` is safe metadata.

It must not imply that consumers may inspect or export the material.

---

## 8. Secret Scope

```text
APPLICATION
USER
PROFILE
PROVIDER
ACCOUNT
SESSION
PROCESS
DEVICE
INSTALLATION
```

Scope affects:

- persistence;
- visibility;
- backend selection;
- lease duration;
- cleanup;
- sharing;
- migration;
- availability.

A session-scoped secret must not silently become application-persistent.

A process-scoped secret must not survive process termination.

---

## 9. SecretDescriptor

`SecretDescriptor` exposes safe metadata.

```text
SecretDescriptor {
    secretId
    canonicalReference
    kind
    scope
    backendId
    namespace
    alias

    providerId?
    accountHint?
    displayLabel?

    currentRevision
    availability
    persistenceMode
    exportPolicy
    rotationPolicy

    createdAt?
    updatedAt?
    expiresAt?
    lastValidatedAt?
    lastUsedAt?

    renewable
    rotatable
    removable
    userManaged
    systemManaged

    tags[]
    metadata
}
```

### 9.1 Safe metadata only

Allowed examples:

```text
providerId
accountHint = "user@…"
lastFour = "A91F"
expiresAt
backendType
credentialKind
rotationSupported
```

Forbidden examples:

```text
rawValue
fullToken
authorizationHeader
privateKeyPem
password
refreshToken
decryptedPayload
backendEncryptionKey
```

### 9.2 Optional timestamps

Backends that cannot safely or reliably provide a timestamp may return `null`.

Absence of metadata must not force material resolution.

---

## 10. SecretAvailability

```text
SecretAvailability {
    reference
    state
    checkedAt
    revision?
    backendId?
    reasonCode?
    retryAfter?
    requiresUserAction
    actionHints[]
}
```

Possible states:

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
ACCESS_DENIED
ROTATION_REQUIRED
USER_ACTION_REQUIRED
```

Availability is not secret material.

`AVAILABLE` means resolution may be attempted by an authorized consumer.

It does not guarantee that a remote provider will accept the credential.

---

## 11. SecretRevisionReference

```text
SecretRevisionReference {
    secretId
    revision
    fingerprint?
    activatedAt
    supersededAt?
}
```

`fingerprint` must be a one-way, non-reversible identifier created specifically for revision comparison.

It must not be:

- the raw secret;
- a reversible encoding;
- an unsalted password hash used for authentication;
- a provider token prefix that materially weakens secrecy.

A fingerprint may be omitted when backend policy does not permit it.

---

## 12. Consumer Identity

Every resolution request requires an authenticated internal consumer identity.

```text
SecretConsumerIdentity {
    consumerId
    moduleId
    componentId?
    applicationInstanceId
    processId?
    sessionId?
    providerId?
    capability?
    trustLevel
}
```

The caller must not self-assert arbitrary trust.

Consumer identity is established by Composition Root, process boundary, dependency injection, signed inter-process channel, or another approved host mechanism.

Presentation code must not construct privileged consumer identities.

---

## 13. Secret Access Purpose

```text
SecretAccessPurpose {
    operationType
    providerId?
    capability?
    targetEndpointClass?
    sessionId?
    workItemId?
    attemptId?
    requestedDuration?
    justificationCode
}
```

Examples:

```text
CREATE_PROVIDER_CLIENT
SIGN_PROVIDER_REQUEST
REFRESH_ACCESS_TOKEN
AUTHENTICATE_REMOTE_CONFIGURATION
VALIDATE_CREDENTIAL
MIGRATE_SECRET
```

Generic purposes such as `READ_SECRET` should be rejected unless explicitly reserved for trusted maintenance tooling.

---

## 14. Access Policy

```text
SecretAccessPolicy {
    accessPolicyId
    allowedConsumers[]
    allowedPurposes[]
    allowedProviders[]
    allowedCapabilities[]
    allowedScopes[]
    allowedBackends[]

    maximumLeaseDuration
    maximumConcurrentLeases?
    renewable
    exportAllowed
    backgroundUseAllowed
    userPresenceRequired
    networkUseAllowed
    processIsolationRequired

    createdRevision
}
```

Policy evaluation uses deny-by-default semantics.

A missing permission means access is denied.

Preferences must never override a mandatory security restriction.

---

## 15. Access Decision

```text
SecretAccessDecision {
    decisionId
    allowed
    denialReason?
    effectivePolicyRevision
    grantedPurpose?
    grantedDuration?
    requiresUserPresence
    requiresBackendUnlock
    auditReference?
}
```

The decision must not expose secret material.

Denied decisions may be logged with safe identifiers and reason codes.

---

## 16. ResolveSecretRequest

```text
ResolveSecretRequest {
    operationId
    reference
    consumerIdentity
    purpose
    expectedKind?
    minimumRevision?
    requestedLeaseDuration?
    correlationId
    causationId?
}
```

Validation order:

```text
Parse reference
    ↓
Locate descriptor
    ↓
Validate consumer identity
    ↓
Evaluate access policy
    ↓
Validate kind and revision
    ↓
Check backend availability
    ↓
Resolve material internally
    ↓
Create bounded lease or handle
```

A resolution request must never be published through the Event Bus.

---

## 17. Resolution Result

A successful resolution returns a handle, not an ordinary serialized value.

```text
ResolveSecretResult {
    secretLease
    descriptorSnapshot
    accessDecision
}
```

`ResolveSecretResult` must be treated as sensitive even though the descriptor and decision are safe.

It must not be:

- persisted;
- cached by general-purpose cache;
- serialized into events;
- included in exception messages;
- exposed to UI;
- copied into diagnostic snapshots.

---

## 18. SecretLease

`SecretLease` grants bounded authority to use one secret revision.

```text
SecretLease {
    leaseId
    secretId
    revision
    consumerId
    purpose
    issuedAt
    expiresAt
    state
    handle
}
```

Possible lease states are defined in `STATES.md`.

A lease:

- is non-transferable;
- is bound to one consumer;
- is bound to one purpose;
- is bound to one secret revision;
- has a bounded lifetime;
- must be released;
- may be revoked;
- must not expose raw material through normal inspection;
- must not be serializable as a whole.

---

## 19. SecretHandle

`SecretHandle` is an opaque internal capability.

Conceptual operations may include:

```text
SecretHandle
├── withBytes(callback)
├── withUtf8(callback)
├── sign(input, algorithm)
├── createAuthorization(adapter)
├── createClientCredential(adapter)
├── exportForApprovedProcess(channel)
└── close()
```

Preferred usage is operation-oriented:

```text
secretHandle.sign(payload)
```

rather than material-oriented:

```text
secretHandle.getRawValue()
```

A raw-value callback may exist only where an adapter cannot operate otherwise.

The callback must:

- be synchronous or tightly bounded;
- prevent retention where practical;
- avoid copying;
- clear temporary buffers where practical;
- prohibit logging;
- prohibit returning the value from the callback;
- run only after policy approval.

---

## 20. SecretMaterialInput

Administrative store and replace commands may carry `SecretMaterialInput`.

```text
SecretMaterialInput {
    kind
    encoding
    sensitivePayload
    compoundParts?
}
```

This object is:

- non-serializable by default;
- never logged;
- never included in command history;
- never included in telemetry;
- never published as an event;
- cleared after use where practical;
- accepted only through trusted UI-to-host or administrative boundaries.

Supported encodings may include:

```text
UTF8
BYTES
PEM
PKCS8
PKCS12
JSON_COMPOUND
PROVIDER_NATIVE_OPAQUE
```

Encoding support is backend- and kind-specific.

---

## 21. RegisterSecretCommand

```text
RegisterSecretCommand {
    operationId
    desiredReference
    kind
    scope
    backendPreference?
    material
    metadata
    accessPolicy
    persistenceMode
    rotationPolicy?
    idempotencyKey
    actor
    correlationId
}
```

Rules:

- the desired reference must not already point to a different secret unless replace semantics are explicit;
- the backend must support the requested scope and persistence;
- access policy must be valid before storage;
- material validation occurs before activation where possible;
- failures must not leave an active descriptor without usable material;
- retries with the same idempotency key and equivalent input must not create duplicates.

---

## 22. RegisterSecretResult

```text
RegisterSecretResult {
    operationReceipt
    descriptor
    revisionReference
    validationSummary?
}
```

The result contains no secret material.

---

## 23. ReplaceSecretCommand

```text
ReplaceSecretCommand {
    operationId
    reference
    expectedCurrentRevision?
    newMaterial
    newMetadata?
    validationMode
    activationMode
    existingLeasePolicy
    idempotencyKey
    actor
    correlationId
}
```

Possible activation modes:

```text
IMMEDIATE
AFTER_VALIDATION
NEW_LEASES_ONLY
SCHEDULED
MANUAL_COMMIT
```

Possible existing lease policies:

```text
ALLOW_TO_EXPIRE
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE
BACKEND_DEFINED
```

Replacement creates a new revision.

It must not mutate the historical revision in place.

---

## 24. RotateSecretCommand

```text
RotateSecretCommand {
    operationId
    reference
    rotationMode
    expectedRevision?
    suppliedMaterial?
    providerRotationContext?
    validationMode
    activationMode
    existingLeasePolicy
    idempotencyKey
    actor
    correlationId
}
```

Possible rotation modes:

```text
USER_SUPPLIED
BACKEND_GENERATED
PROVIDER_GENERATED
REFRESH_TOKEN_FLOW
CERTIFICATE_REISSUE
KEY_PAIR_REGENERATION
```

Rotation coordination may involve Provider Management or an authentication adapter.

Secret Management remains owner of stored secret material and revision identity.

---

## 25. Rotation Result

```text
SecretRotationResult {
    rotationId
    oldRevision
    newRevision
    activationState
    validationSummary
    affectedLeaseCount?
    revokedLeaseCount?
    requiresConsumerRefresh
    operationReceipt
}
```

No raw old or new value may appear in the result.

---

## 26. RevokeSecretCommand

```text
RevokeSecretCommand {
    operationId
    reference
    expectedRevision?
    reasonCode
    leasePolicy
    providerRevocationRequested
    actor
    correlationId
}
```

Revocation means the secret must no longer be used for new access.

Revocation is distinct from deletion.

A revoked descriptor may remain for audit, diagnostics, or recovery policy.

---

## 27. RemoveSecretCommand

```text
RemoveSecretCommand {
    operationId
    reference
    expectedRevision?
    removalMode
    activeLeasePolicy
    removeDescriptor
    actor
    correlationId
}
```

Possible removal modes:

```text
LOGICAL_REMOVE
DELETE_MATERIAL
CRYPTOGRAPHIC_ERASURE
BACKEND_DEFINED_SECURE_DELETE
```

The contract must not claim guaranteed physical erasure when the backend cannot guarantee it.

The result must report the effective deletion assurance level.

---

## 28. SecretOperationReceipt

```text
SecretOperationReceipt {
    operationId
    operationType
    secretId?
    reference?
    acceptedAt
    completedAt?
    status
    resultingRevision?
    backendId?
    assuranceLevel?
    warnings[]
}
```

Possible statuses:

```text
ACCEPTED
COMPLETED
PARTIALLY_COMPLETED
REJECTED
FAILED
DEFERRED
USER_ACTION_REQUIRED
```

Receipts are safe only when warning and metadata fields follow redaction rules.

---

## 29. ValidateSecretCommand

```text
ValidateSecretCommand {
    validationId
    reference
    validationMode
    consumerIdentity
    purpose
    networkPermission
    providerId?
    endpointClass?
    timeout?
    correlationId
}
```

Possible validation modes:

```text
REFERENCE_ONLY
BACKEND_ACCESS
STRUCTURAL
LOCAL_CRYPTOGRAPHIC
PROVIDER_AUTHENTICATION
PROVIDER_CAPABILITY
EXPIRATION_ONLY
```

Provider authentication validation may create an external request.

It must obey privacy, network, provider, cost, and rate-limit policy.

---

## 30. SecretValidationSummary

```text
SecretValidationSummary {
    validationId
    reference
    revision?
    status
    checkedAt
    validationMode
    providerId?
    expiresAt?
    renewable?
    reasonCode?
    warnings[]
    safeEvidence
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

Safe evidence may include:

```text
provider responded with authentication success
certificate chain valid until timestamp
reference resolved from backend
token expiration metadata parsed
```

It must not include:

```text
raw response headers
authorization header
token body
private key
full provider error body
```

---

## 31. DescribeSecretQuery

```text
DescribeSecretQuery {
    reference
    includeAvailability
    includePolicySummary
    includeValidationSummary
    callerIdentity
}
```

Result:

```text
DescribeSecretResult {
    descriptor
    availability?
    accessPolicySummary?
    validationSummary?
}
```

Description never resolves secret material.

---

## 32. ListSecretsQuery

```text
ListSecretsQuery {
    namespace?
    providerId?
    kind?
    scope?
    availability?
    backendId?
    tags?
    page
    callerIdentity
}
```

Result:

```text
ListSecretsResult {
    descriptors[]
    nextPageToken?
}
```

Enumeration is administrative and permission-controlled.

A consumer authorized to resolve one reference is not automatically authorized to enumerate all secrets.

---

## 33. ExistsSecretQuery

```text
ExistsSecretQuery {
    reference
    callerIdentity
}
```

Result:

```text
ExistsSecretResult {
    existence
    availability?
}
```

Possible existence values:

```text
EXISTS
DOES_NOT_EXIST
UNKNOWN
HIDDEN_BY_POLICY
```

`HIDDEN_BY_POLICY` prevents enumeration through existence probing.

---

## 34. Secret Backend Contract

```text
SecretBackend {
    backendId
    backendType
    capabilities()

    store(request)
    replace(request)
    resolve(request)
    delete(request)
    describe(request)
    exists(request)
    validateAccess(request)
    lock()
    unlock(context)
    migrate(request)
}
```

Backend adapters must not expose platform-native secret objects beyond the adapter boundary.

---

## 35. Backend Capabilities

```text
SecretBackendCapabilities {
    persistent
    memoryOnly
    userScoped
    machineScoped
    sessionScoped

    supportsAtomicReplace
    supportsVersioning
    supportsExpirationMetadata
    supportsEnumeration
    supportsSecureDelete
    supportsUserPresence
    supportsHardwareProtection
    supportsExport
    supportsImport
    supportsLocking
    supportsMigration

    supportedKinds[]
    maximumValueSize?
}
```

Capability metadata is safe.

Claims such as hardware protection or secure deletion must reflect actual backend guarantees.

---

## 36. Backend Types

Potential backend types:

```text
OS_KEYCHAIN
OS_CREDENTIAL_MANAGER
ENCRYPTED_APPLICATION_STORE
ENVIRONMENT
MEMORY
FILE_REFERENCE
EXTERNAL_SECRET_MANAGER
HARDWARE_SECURITY_MODULE
CUSTOM
```

MVP preference:

```text
Production desktop persistent secrets
    → operating-system secure store

Session-only tokens
    → memory backend

Development injection
    → environment backend under explicit policy
```

Ordinary plaintext configuration files are not an approved persistent secret backend.

---

## 37. Backend Selection Request

```text
SelectSecretBackendRequest {
    kind
    scope
    persistenceMode
    requiredCapabilities[]
    preferredBackendId?
    platformContext
    policy
}
```

Result:

```text
SelectSecretBackendResult {
    selectedBackendId
    capabilitySnapshot
    selectionReason
    warnings[]
}
```

Selection must fail when mandatory capabilities cannot be satisfied.

It must not silently downgrade a persistent secure secret to plaintext storage.

---

## 38. Compound Credentials

Some provider credentials consist of multiple related values.

```text
CompoundSecretDescriptor {
    secretId
    kind = COMPOUND_CREDENTIAL
    parts[] {
        partName
        partKind
        required
        exposurePolicy
    }
}
```

Examples:

```text
clientId + clientSecret
accessToken + refreshToken
certificate + privateKey
username + password
```

Public descriptors may expose part names and kinds.

They never expose part values.

Atomicity:

- registration should activate all required parts together;
- replacement must not produce a partially active compound credential;
- resolution returns one compound handle;
- rotation may update only selected parts when the provider protocol allows it;
- revision identity must describe the effective compound set.

---

## 39. Renewable Credentials

```text
RefreshSecretRequest {
    operationId
    reference
    expectedRevision
    consumerIdentity
    refreshPurpose
    activationMode
    correlationId
}
```

Refresh may use:

- refresh token;
- client credentials;
- provider SDK;
- operating-system broker;
- user interaction;
- external identity provider.

Refresh results in a new revision when effective secret material changes.

Secret Management stores the new material.

Provider Management or Authentication Adapter may coordinate provider-specific refresh protocol.

---

## 40. User Presence

Some backends require:

- biometric confirmation;
- device unlock;
- password re-entry;
- system credential prompt;
- browser authentication.

```text
UserPresenceRequirement {
    required
    mode
    reasonCode
    timeout?
    interactionOwner
}
```

Possible modes:

```text
NONE
DEVICE_UNLOCK
BIOMETRIC
SYSTEM_PROMPT
APPLICATION_CONFIRMATION
EXTERNAL_AUTH_FLOW
```

Secret Management reports the requirement.

Presentation owns user interaction.

Secret Management must not bypass a backend-required prompt.

---

## 41. Secret Export

Export is denied by default.

```text
ExportSecretRequest {
    operationId
    reference
    targetType
    targetIdentity
    purpose
    consumerIdentity
    userConfirmation?
    correlationId
}
```

Allowed target types may include:

```text
APPROVED_CHILD_PROCESS
APPROVED_PROVIDER_SDK
APPROVED_OS_API
ENCRYPTED_BACKUP
```

General clipboard, log, UI text, Event Bus, ordinary file, and diagnostic export are prohibited.

Export must require explicit backend and access-policy support.

---

## 42. Child Process Secret Transfer

When a worker process requires a secret:

```text
Parent resolves approved access
    ↓
Create authenticated IPC channel
    ↓
Bind transfer to child identity and purpose
    ↓
Transfer minimal material or operation capability
    ↓
Child acknowledges
    ↓
Expire transfer context
```

The contract must define:

```text
ChildSecretTransferRequest {
    leaseId
    childProcessIdentity
    purpose
    channelBinding
    expiresAt
}
```

Command-line arguments and inherited plaintext environment variables should not be used for persistent production secrets.

---

## 43. Redaction Contract

```text
SecretRedactor {
    redactReference(reference)
    redactDescriptor(descriptor)
    redactText(text, context)
    sanitizeMetadata(metadata)
    inspectObject(object)
}
```

Redaction results:

```text
RedactionResult {
    sanitizedValue
    findings[]
    blocked
}
```

Possible findings:

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

A redactor is a defense-in-depth mechanism.

It does not permit callers to handle raw secret values casually.

---

## 44. Safe Reference Redaction

A reference may itself reveal sensitive organization or account information.

Example:

```text
secret://provider/customer-name-prod-admin
```

Safe display may become:

```text
secret://provider/***
```

or:

```text
Provider credential · primary account
```

The descriptor controls safe labels.

The raw alias should not automatically appear in telemetry.

---

## 45. Logging Contract

Allowed logging fields:

```text
operationId
secretId
referenceHash
backendId
consumerId
purposeCode
decision
revision
leaseId
duration
resultCode
```

Prohibited logging fields:

```text
secret material
authorization header
raw alias when sensitive
full environment value
private key
password
refresh token
provider-native credential object
decrypted backend payload
```

Debug mode does not disable these prohibitions.

---

## 46. Event Boundary

Secret material must never be published through Event Bus.

Events may include:

```text
secretId
safeReference
revision
availabilityState
rotationState
leaseId
consumerId
reasonCode
occurredAt
```

Events must not include:

```text
raw material
material input
secret handle
backend decrypted payload
authorization header
provider-native credential
private key data
password
token body
```

Commands carrying `SecretMaterialInput` are direct trusted calls, not Event Bus messages.

---

## 47. Configuration Boundary

Configuration stores:

```text
SecretReference
SecretReferenceId
expectedKind
expectedProviderId
minimumRevision?
```

Configuration does not store:

```text
raw API key
password
access token
refresh token
private key
client secret
decrypted credential payload
```

Configuration may validate reference syntax.

Secret Management validates existence, availability, access, and resolution.

---

## 48. Provider Management Boundary

Provider Management may:

- request credential availability;
- request a bounded secret lease;
- bind a secret lease to a provider client;
- react to rotation or revocation;
- report authentication outcomes;
- request validation;
- expose safe credential readiness.

Provider Management must not:

- persist raw secret material;
- expose raw credentials through `ExecutionHandle`;
- return secret handles to Translation or Recognition;
- publish secret values;
- construct arbitrary references outside approved configuration;
- bypass Secret Management policy.

Preferred flow:

```text
Provider lease requested
    ↓
Provider Management validates provider path
    ↓
Secret Management evaluates credential access
    ↓
Provider adapter receives bounded secret capability
    ↓
Provider client created
    ↓
Raw material remains infrastructure-internal
```

---

## 49. Runtime Boundary

Runtime may carry:

```text
SecretLeaseId
CredentialAvailabilityReference
ProviderLeaseId
safe revision metadata
```

Runtime must not carry:

```text
SecretMaterialInput
SecretHandle
raw token
password
private key
authorization header
```

Secret leases should be acquired as late as practical and released as early as practical.

Cancellation must trigger lease release or revocation according to policy.

---

## 50. Lease Acquisition Contract

```text
AcquireSecretLeaseRequest {
    operationId
    reference
    consumerIdentity
    purpose
    requestedDuration
    expectedRevision?
    correlationId
}
```

Result:

```text
AcquireSecretLeaseResult {
    lease
    descriptorSnapshot
    accessDecision
}
```

The lease is granted only after:

- reference validation;
- identity validation;
- policy evaluation;
- backend availability check;
- revision validation;
- material resolution;
- lease-capacity admission.

---

## 51. Lease Renewal Contract

```text
RenewSecretLeaseRequest {
    leaseId
    consumerIdentity
    additionalDuration
    purpose
    correlationId
}
```

Renewal must re-evaluate:

- policy;
- secret revision;
- revocation;
- expiration;
- backend state;
- consumer identity;
- maximum lifetime.

A lease cannot be renewed after terminal revocation.

---

## 52. Lease Release Contract

```text
ReleaseSecretLeaseCommand {
    leaseId
    consumerIdentity
    reasonCode
    correlationId
}
```

Release must be idempotent.

Repeated release of the same lease must not expose internal material or create a new error unless a security mismatch is detected.

---

## 53. Lease Revocation Contract

```text
RevokeSecretLeaseCommand {
    leaseId
    reasonCode
    effectiveAt
    gracePeriod?
    actor
    correlationId
}
```

Revocation may invalidate future handle operations.

It may not be able to cancel a remote request that already copied credential material into an external protocol exchange.

The consuming module remains responsible for its own work-state consequences.

---

## 54. Authentication Outcome Feedback

```text
ReportSecretUseOutcomeCommand {
    operationId
    secretId
    revision
    leaseId
    consumerId
    providerId?
    outcome
    normalizedReason?
    occurredAt
    correlationId
}
```

Possible outcomes:

```text
ACCEPTED
AUTHENTICATION_REJECTED
AUTHORIZATION_REJECTED
EXPIRED_REPORTED
REVOKED_REPORTED
RATE_LIMITED
PROVIDER_UNAVAILABLE
UNKNOWN_FAILURE
```

Feedback must not contain raw provider error bodies or request headers.

Secret Management may update validation metadata or availability.

Provider Management owns provider-health interpretation.

---

## 55. Idempotency

Mutating commands require `operationId` and should support `idempotencyKey`.

Equivalent retries must not:

- create duplicate secrets;
- create multiple revisions;
- repeat provider-side rotation unnecessarily;
- duplicate deletion;
- duplicate lease revocation;
- overwrite a newer revision.

Same idempotency key with different semantic input must be rejected.

---

## 56. Optimistic Concurrency

Mutating commands should use:

```text
expectedCurrentRevision
```

when updating an existing secret.

If the current revision differs:

```text
Expected R4
Actual R5
    → reject with revision conflict
```

A stale administrative command must not overwrite a newer credential.

---

## 57. Atomicity

Operations must preserve these invariants:

### Registration

```text
Descriptor active
    ⇔
Required material durably available
```

### Replacement

```text
Old revision active
    ↓ validate candidate
New revision committed atomically
    ↓
Old revision superseded
```

### Compound credential

All required parts activate together.

### Migration

At least one valid copy remains until the destination is validated and activation succeeds.

---

## 58. Backend Migration Contract

```text
MigrateSecretCommand {
    operationId
    reference
    sourceBackendId
    destinationBackendId
    expectedRevision
    activationMode
    sourceCleanupMode
    actor
    correlationId
}
```

Recommended flow:

```text
Resolve source internally
    ↓
Store destination candidate
    ↓
Validate destination candidate
    ↓
Activate destination reference
    ↓
Drain or revoke affected leases
    ↓
Cleanup source according to assurance policy
```

Migration must never expose material to the caller.

---

## 59. Backup and Recovery Contract

Secret backup is optional and backend-dependent.

```text
SecretBackupPolicy {
    enabled
    targetType
    encryptionRequirement
    userConfirmationRequired
    recoveryAuthority
    retention
}
```

Recovery contracts must distinguish:

```text
metadata recovery
secret material recovery
provider credential re-entry
provider credential reissuance
```

The system must not claim recoverability when only descriptors are backed up.

---

## 60. Serialization Rules

Safe and serializable:

- references;
- descriptors;
- availability;
- revision references;
- validation summaries;
- policy summaries;
- operation receipts;
- normalized errors.

Not serializable:

- raw secret material;
- `SecretMaterialInput`;
- `SecretHandle`;
- full `SecretLease`;
- decrypted backend payload;
- platform-native credential object;
- temporary plaintext buffer;
- authorization object containing a credential.

Serialization libraries should reject sensitive types by default.

---

## 61. Equality and Hashing

`SecretReference` may support normalized equality.

`SecretDescriptor` equality should not imply equal material.

`SecretHandle`, `SecretLease`, and material objects must not implement value-based equality over secret bytes.

Hash codes must never be derived directly from raw secret material for general use.

---

## 62. Caching Rules

General application cache must not cache secret material.

Permitted caches:

```text
safe descriptor cache
availability cache
policy decision cache with short TTL
backend capability cache
```

Sensitive internal caching may exist only when:

- owned by Secret Management;
- bounded;
- memory-only unless secure backend semantics apply;
- cleared on revocation, rotation, shutdown, lock, or policy change;
- not visible to general cache inspection.

---

## 63. Memory Handling

Implementations should:

- minimize plaintext lifetime;
- minimize copies;
- use mutable buffers where clearing is meaningful;
- avoid immutable language strings for long-lived sensitive values where practical;
- clear temporary buffers where practical;
- avoid memory dumps containing secrets;
- avoid crash reporting of secret-bearing objects;
- prevent debugger display in production where practical.

These are best-effort protections.

Managed runtimes cannot always guarantee immediate physical memory erasure.

Contracts must not claim stronger guarantees than the runtime provides.

---

## 64. Time Semantics

All contracts use an injected monotonic and wall clock abstraction where appropriate.

Wall-clock timestamps:

```text
createdAt
updatedAt
expiresAt
activatedAt
revokedAt
```

Monotonic duration is preferred for:

```text
lease lifetime
operation timeout
unlock timeout
validation timeout
grace period
```

Clock changes must not silently extend a bounded lease beyond policy.

---

## 65. Cancellation

Sensitive operations accept cancellation where possible.

Cancellation rules:

- cancellation must not publish partial secret material;
- canceled registration must not activate an incomplete entry;
- canceled replacement keeps the prior valid revision active;
- canceled migration keeps the source valid;
- canceled validation does not mark a secret invalid unless evidence already proves invalidity;
- cancellation of a remote provider rotation may result in an uncertain outcome;
- uncertain outcomes require reconciliation before retry.

---

## 66. Partial and Uncertain Outcomes

Secret operations may become uncertain when an external provider or backend completed an action but the local acknowledgement was lost.

```text
SecretOperationUncertainty {
    operationId
    operationType
    reference
    state = RECONCILIATION_REQUIRED
    safeEvidence
    recommendedAction
}
```

The system must not blindly repeat non-idempotent provider rotation.

Reconciliation may query:

- provider credential status;
- backend revision;
- destination entry existence;
- active reference revision;
- lease state.

---

## 67. Error Contract Shape

Detailed codes belong in `ERRORS.md`.

Shared shape:

```text
SecretManagementError {
    errorId
    code
    category
    scope
    severity
    retryable
    userActionRequired
    safeMessage
    recoveryActions[]
    reference?
    secretId?
    revision?
    leaseId?
    backendId?
    correlationId
    occurredAt
    metadata
}
```

The error must not contain raw secret-manager exceptions when they may expose sensitive values.

---

## 68. Error Categories

```text
REFERENCE
VALIDATION
ACCESS_POLICY
CONSUMER_IDENTITY
BACKEND
BACKEND_LOCK
MATERIAL
REGISTRATION
REPLACEMENT
ROTATION
REVOCATION
REMOVAL
LEASE
MIGRATION
REFRESH
USER_PRESENCE
SERIALIZATION
REDACTION
SECURITY
CONCURRENCY
PERSISTENCE
INTERNAL
```

---

## 69. Recovery Actions

```text
RETRY
RETRY_AFTER
UNLOCK_SECRET_STORE
REENTER_SECRET
ROTATE_SECRET
REFRESH_CREDENTIAL
SELECT_DIFFERENT_REFERENCE
SELECT_DIFFERENT_BACKEND
REQUEST_USER_PRESENCE
CHECK_SYSTEM_CREDENTIAL_STORE
CHECK_ENVIRONMENT_CONFIGURATION
UPDATE_ACCESS_POLICY
RESTART_APPLICATION
RECONCILE_PROVIDER_STATE
CONTACT_SUPPORT
NONE
```

Recovery actions are recommendations.

They are not automatic commands.

---

## 70. Security Violations

The following must be treated as contract or security violations:

- raw secret placed in an event;
- raw secret placed in log metadata;
- Presentation requesting secret resolution directly;
- consumer identity mismatch;
- lease used by another consumer;
- handle used for another purpose;
- handle used after revocation;
- secret exported to clipboard;
- plaintext secret persisted in ordinary configuration;
- backend downgrade to insecure storage without explicit approved policy;
- secret included in exception string;
- secret material serialized;
- secret material returned through diagnostics.

---

## 71. Diagnostics Contract

Safe diagnostics may expose:

```text
backend registered
backend locked
number of descriptors
availability counts
rotation-required count
expired count
active lease count
failed operation count
redaction finding count
last successful backend check
```

Diagnostics must not expose:

```text
secret values
full aliases when sensitive
authorization payloads
private keys
passwords
refresh tokens
decrypted compound credentials
raw environment values
```

A diagnostics export must pass redaction and object inspection.

---

## 72. Metrics Contract

Permitted metrics:

```text
secret_resolution_total
secret_resolution_failure_total
secret_resolution_duration
secret_lease_active
secret_lease_duration
secret_rotation_total
secret_rotation_failure_total
secret_validation_total
secret_backend_availability
secret_backend_unlock_required_total
secret_access_denied_total
secret_redaction_block_total
secret_expired_total
```

Metric labels must remain low-cardinality and non-sensitive.

Do not use:

```text
raw alias
token prefix
account email
full provider account id
secret value hash as a high-cardinality label
```

---

## 73. Tracing Contract

Trace spans may include:

```text
operation type
backend type
consumer module
provider id when safe
decision
result code
revision
lease duration
```

Trace spans must not include secret material.

Sensitive operations may use restricted trace detail even when general diagnostic mode is enabled.

---

## 74. Testing Contracts

Implementations must provide test doubles that preserve security semantics.

```text
SecretManagementTestDouble {
    registerSafeFixture(reference, fixtureId)
    configureAvailability(reference, state)
    configureResolutionFailure(reference, errorCode)
    inspectSafeOperations()
}
```

Tests should avoid real production credentials.

Test fixtures must be clearly non-production and isolated.

Snapshot tests must ensure sensitive types cannot serialize.

---

## 75. Required Contract Tests

### Reference

- valid schemes;
- invalid schemes;
- normalization;
- forbidden embedded material;
- case rules;
- namespace isolation.

### Registration

- successful atomic registration;
- duplicate idempotent registration;
- idempotency conflict;
- backend capability mismatch;
- canceled registration;
- failed validation keeps no active incomplete entry.

### Resolution

- authorized consumer;
- denied consumer;
- purpose mismatch;
- kind mismatch;
- revision mismatch;
- missing secret;
- locked backend;
- expired secret;
- revoked secret;
- no serialization of result.

### Lease

- bounded lifetime;
- consumer binding;
- purpose binding;
- idempotent release;
- renewal policy;
- revocation;
- rotation interaction;
- shutdown cleanup.

### Rotation

- new revision;
- old revision preserved historically;
- existing lease policies;
- failed candidate validation;
- uncertain provider outcome;
- stale expected revision conflict.

### Redaction

- known values;
- authorization headers;
- PEM blocks;
- environment dumps;
- nested objects;
- exception messages;
- diagnostics export;
- false-positive handling without exposing input.

### Boundary

- no Event Bus material;
- no Configuration material;
- no Presentation resolution;
- no Runtime material;
- no provider execution-handle exposure.

---

## 76. MVP Contract Boundary

The initial desktop MVP should support:

```text
SecretReference
SecretDescriptor
SecretAvailability
RegisterSecret
ReplaceSecret
RemoveSecret
ResolveSecret
AcquireSecretLease
ReleaseSecretLease
ValidateSecret
OS secure-store backend
Memory backend
Environment reference backend
Redaction
Safe diagnostics
Provider Management integration
```

The MVP may defer:

```text
external cloud secret manager
hardware security module
multi-device synchronization
encrypted backup and recovery
automatic provider-side key generation
complex certificate lifecycle
cross-process capability forwarding
organization-wide policy service
remote administrative API
multi-user desktop sharing
```

Deferred capabilities must not be blocked by reference, backend, policy, revision, and lease abstractions.

---

## 77. Contract Decisions

### Decision 1 — References are public; material is not

All normal module boundaries use `SecretReference` or safe descriptors.

### Decision 2 — Resolution returns capability, not ordinary value

The preferred result is a bounded `SecretHandle` inside a `SecretLease`.

### Decision 3 — Access is deny by default

Consumer, purpose, scope, provider, and duration must be authorized.

### Decision 4 — Rotation creates a revision

Existing material is never silently mutated under the same revision.

### Decision 5 — Leases are consumer- and purpose-bound

They are non-transferable and time-bounded.

### Decision 6 — Event Bus never transports secret material

Sensitive administrative calls use direct trusted interfaces.

### Decision 7 — Configuration stores only references

Secret Management owns existence, storage, resolution, and lifecycle.

### Decision 8 — Provider Management coordinates use

Provider Management may acquire credential access for adapters but does not own persistent secret material.

### Decision 9 — OS secure storage is preferred for desktop persistence

Plain files are not the normal production backend.

### Decision 10 — Export is denied by default

Only explicitly approved targets and policies may receive material.

### Decision 11 — Backend guarantees are explicit

Secure deletion, hardware protection, and user-presence claims must reflect reality.

### Decision 12 — Redaction is mandatory defense in depth

It does not replace proper boundary design.

---

## 78. Open Decisions

The following remain for later documents or implementation selection.

### State decisions

- exact descriptor lifecycle;
- exact secret revision lifecycle;
- lease lifecycle;
- backend lock lifecycle;
- rotation candidate lifecycle;
- deletion tombstone lifecycle;
- migration lifecycle.

### Event decisions

- event granularity for availability changes;
- rotation event visibility;
- lease event visibility;
- backend lock events;
- user-action-required events;
- whether administrative audit events use a separate restricted sink.

### Error decisions

- exact error codes;
- severity mapping;
- locked-backend retry policy;
- uncertain provider-rotation errors;
- redaction failure behavior;
- secure-delete assurance warnings.

### Platform decisions

- Windows Credential Manager implementation;
- macOS Keychain implementation;
- Linux Secret Service / keyring implementation;
- fallback behavior when no secure store exists;
- application unlock UX;
- process isolation model.

### Policy decisions

- default lease duration;
- maximum concurrent leases;
- whether environment references are allowed in production;
- default existing-lease behavior on rotation;
- secret descriptor retention after deletion;
- audit retention;
- user-presence requirements;
- automatic validation cadence;
- automatic refresh policy;
- backend migration policy.

---

## 79. Recommended Documentation Order

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

`STATES.md` should next define:

- `SecretDescriptorState`;
- `SecretRevisionState`;
- `SecretLeaseState`;
- `SecretBackendState`;
- `SecretRotationState`;
- `SecretMigrationState`;
- valid transitions;
- rotation and revocation effects on active leases;
- backend lock and unlock behavior;
- deletion and tombstone behavior.

---

## 80. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/configuration/MODULE.md
03-infrastructure/configuration/CONTRACT.md

02-modules/provider-management/MODULE.md
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/STATES.md
02-modules/provider-management/EVENTS.md
02-modules/provider-management/ERRORS.md
```

Future Secret Management documents:

```text
03-infrastructure/secret-management/STATES.md
03-infrastructure/secret-management/EVENTS.md
03-infrastructure/secret-management/ERRORS.md
03-infrastructure/secret-management/README.md
```

---

## 81. Summary

Secret Management exposes safe identity, metadata, availability, policy, validation, revision, and operation contracts while keeping raw secret material inside approved infrastructure boundaries.

Its central access flow is:

```text
SecretReference
    ↓
Consumer Identity + Purpose
    ↓
Policy Evaluation
    ↓
Backend Resolution
    ↓
Bounded SecretLease
    ↓
Opaque SecretHandle
    ↓
Approved Operation
    ↓
Release
```

Its central mutation flow is:

```text
Sensitive Material Input
    ↓
Validate
    ↓
Store Candidate
    ↓
Activate New Revision
    ↓
Apply Existing-Lease Policy
    ↓
Publish Safe Metadata Only
```

The contract deliberately prevents:

- raw secret values in Configuration;
- raw secret values in Event Bus;
- raw secret values in ordinary Runtime work;
- raw secret values in Presentation;
- raw secret values in logs, traces, metrics, diagnostics, and exceptions;
- unrestricted material export;
- silent revision mutation;
- unauthorized or purpose-free resolution;
- insecure backend downgrade.

This document is the contract source of truth for subsequent Secret Management states, events, errors, and implementation documentation.
