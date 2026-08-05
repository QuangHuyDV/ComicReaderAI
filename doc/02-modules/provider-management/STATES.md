# Provider Management States

> **Project:** CRAI  
> **Module:** Provider Management  
> **Document:** State Machines  
> **Path:** `02-modules/provider-management/STATES.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-04  
> **Source of Truth:**
>
> - `02-modules/provider-management/MODULE.md`
> - `02-modules/provider-management/CONTRACT.md`

---

## 1. Purpose

This document defines the lifecycle states and valid state transitions owned by the Provider Management module.

It covers:

- provider definition lifecycle;
- provider model lifecycle;
- provider lease lifecycle;
- provider availability;
- provider health;
- provider circuit-breaker lifecycle;
- local model lifecycle;
- provider disable and archive behavior;
- active lease handling;
- state relationships;
- command-to-state mapping;
- event-to-state mapping;
- concurrency and persistence rules;
- invalid transitions;
- cross-module ownership boundaries.

This document does not define:

- command payload schemas;
- query payload schemas;
- event payload schemas;
- detailed error catalogs;
- provider-native states;
- Runtime worker states;
- Translation job states;
- Recognition job states;
- persistence tables;
- implementation classes.

---

## 2. State Ownership

Provider Management owns lifecycle state for:

```text
ProviderDefinition
ProviderModel
ProviderLease
ProviderAvailability
ProviderHealth
ProviderCircuit
LocalModel
ProviderClientLifecycle
CredentialAvailabilityReference
```

Provider Management does not own lifecycle state for:

```text
RuntimeWorkItem
RuntimeWorker
RuntimeQueue
TranslationJob
TranslationBatch
TranslationAttempt
RecognitionJob
ReadingSession
Presentation
KnowledgeSnapshot
TranslationResult
RecognitionResult
```

External state may influence Provider Management transitions but remains owned by the originating module.

---

## 3. State Ownership Matrix

| State machine | Owner | Notes |
|---|---|---|
| `ProviderDefinitionState` | Provider Management | Administrative provider lifecycle |
| `ProviderModelState` | Provider Management | Logical model lifecycle |
| `ProviderLeaseState` | Provider Management | Temporary provider-access authority |
| `ProviderAvailabilityState` | Provider Management | Normalized eligibility summary |
| `ProviderHealthState` | Provider Management | Operational health condition |
| `ProviderCircuitState` | Provider Management | Circuit-breaker protection |
| `LocalModelState` | Provider Management | Local-model installation and residency lifecycle |
| `ProviderClientState` | Provider Management | Internal client lifecycle |
| `CredentialAvailabilityState` | Provider Management or Secret boundary | Only normalized availability is exposed |
| `RuntimeWorkState` | Runtime | Execution scheduling and worker lifecycle |
| `TranslationJobState` | Translation | Translation semantic lifecycle |
| `RecognitionJobState` | Recognition | Recognition semantic lifecycle |
| `ReadingSessionState` | Reading Session | Current content authority |
| `PresentationState` | Presentation | Logical presentation lifecycle |

Provider Management may observe Runtime and consumer-module states.

It must not mutate them directly.

---

## 4. State Machine Separation

Provider Management does not use one global state enumeration.

Each entity has an independent lifecycle:

```text
ProviderDefinitionState
ProviderModelState
ProviderLeaseState
ProviderAvailabilityState
ProviderHealthState
ProviderCircuitState
LocalModelState
```

This separation is required because:

- a provider may be enabled while temporarily unavailable;
- a model may be active while its local instance is unloaded;
- a provider may be healthy while rate-limited;
- a circuit may be open while the provider definition remains enabled;
- a lease may remain active while the provider enters draining mode;
- a local model may be ready while Runtime denies new resource admission;
- a provider may be archived while historical lease records remain queryable.

---

## 5. State Principles

### 5.1 State represents current domain truth

```text
State
    = current lifecycle condition

Event
    = historical fact that a transition occurred
```

State and event semantics must not be conflated.

### 5.2 Transitions are explicit

Entities must not jump between unrelated states without a defined transition.

### 5.3 Terminal does not mean deleted

Terminal states preserve historical identity unless retention policy explicitly removes records.

### 5.4 Provider state does not replace Runtime state

Provider Management states describe provider access, eligibility, health, and lifecycle.

Runtime states describe scheduling, admission, and physical work execution.

### 5.5 Availability is derived

`ProviderAvailabilityState` is a normalized summary derived from several independent facts.

It is not the sole source of truth for provider configuration, health, circuit, credentials, or local resources.

### 5.6 Lease state controls access authority

A lease grants bounded provider access.

A Runtime work item may exist without an active provider operation, and a granted lease may exist before Runtime execution begins.

### 5.7 Historical identities are stable

Provider, model, lease, and configuration identities must remain traceable after disablement, deprecation, release, expiration, revocation, or archival.

---

# Part I — Provider Definition State Machine

## 6. ProviderDefinitionState

Canonical states:

```text
REGISTERED
ENABLED
DISABLED
ARCHIVED
```

---

## 7. REGISTERED

The provider definition exists but is not yet eligible for normal selection.

At this point:

- `ProviderId` exists;
- initial configuration is persisted;
- adapter binding may still require validation;
- credential availability may be unresolved;
- models may still be unregistered;
- no new lease should be granted unless an explicit testing policy permits it.

Valid outgoing transitions:

```text
REGISTERED → ENABLED
REGISTERED → DISABLED
REGISTERED → ARCHIVED
```

Typical entry causes:

```text
RegisterProvider accepted
ImportProviderDefinition accepted
Administrative restore into registered state
```

Expected event:

```text
ProviderRegistered
```

---

## 8. ENABLED

The provider is administratively permitted to participate in eligibility evaluation and selection.

`ENABLED` does not guarantee:

- availability;
- healthy status;
- credentials;
- eligible models;
- free quota;
- closed circuit;
- Runtime resources.

Valid outgoing transitions:

```text
ENABLED → DISABLED
ENABLED → ARCHIVED
```

Expected events may include:

```text
ProviderEnabled
ProviderConfigurationActivated
```

New leases may be granted only if all additional eligibility checks pass.

---

## 9. DISABLED

The provider is administratively excluded from new selection and new leases.

At this point:

- no new lease may be granted;
- no new provider selection result may identify it as eligible;
- existing leases follow the configured disable policy;
- historical data remains queryable;
- models remain identifiable;
- health and usage history may continue to be retained.

Valid outgoing transitions:

```text
DISABLED → ENABLED
DISABLED → ARCHIVED
```

Expected event:

```text
ProviderDisabled
```

A provider may be disabled because of:

- administrative choice;
- invalid configuration;
- credential unavailability;
- security policy;
- maintenance;
- deprecation;
- migration.

---

## 10. ARCHIVED

The provider is retained only for historical identity and audit.

At this point:

- it cannot participate in selection;
- it cannot receive new leases;
- its models cannot be selected;
- historical results may still reference it;
- usage and audit records remain queryable;
- provider-native resources may already be removed.

`ARCHIVED` is terminal.

No normal outgoing transition is permitted.

Restoring an archived integration should create a new provider definition or follow an explicit administrative migration process.

---

## 11. Provider Definition Diagram

```text
REGISTERED
    ├──► ENABLED
    ├──► DISABLED
    └──► ARCHIVED

ENABLED
    ├──► DISABLED
    └──► ARCHIVED

DISABLED
    ├──► ENABLED
    └──► ARCHIVED

ARCHIVED
    └──► terminal
```

---

# Part II — Provider Model State Machine

## 12. ProviderModelState

Canonical states:

```text
REGISTERED
ACTIVE
DEPRECATED
DISABLED
REMOVED
```

---

## 13. Model REGISTERED

The model definition exists but is not yet eligible for normal selection.

Possible reasons:

- model metadata incomplete;
- capability validation pending;
- provider catalog refresh not finalized;
- local model files not validated;
- administrative approval pending.

Valid outgoing transitions:

```text
REGISTERED → ACTIVE
REGISTERED → DISABLED
REGISTERED → REMOVED
```

---

## 14. Model ACTIVE

The model may participate in eligibility checks when its provider and operational state permit.

`ACTIVE` does not guarantee:

- provider enabled state;
- provider availability;
- local model residency;
- credential availability;
- Runtime resource admission.

Valid outgoing transitions:

```text
ACTIVE → DEPRECATED
ACTIVE → DISABLED
ACTIVE → REMOVED
```

---

## 15. Model DEPRECATED

The model remains identifiable and may remain usable under explicit policy, but it should not normally be selected for new work.

A deprecated model may be allowed when:

- explicitly required;
- no replacement exists;
- backward compatibility requires it;
- migration policy permits it;
- an existing lease is draining.

Valid outgoing transitions:

```text
DEPRECATED → ACTIVE
DEPRECATED → DISABLED
DEPRECATED → REMOVED
```

Returning to `ACTIVE` requires explicit administrative action.

---

## 16. Model DISABLED

The model is unavailable for new selection.

Its provider may remain enabled.

Valid outgoing transitions:

```text
DISABLED → ACTIVE
DISABLED → DEPRECATED
DISABLED → REMOVED
```

Existing leases follow lease and provider-disable policy.

---

## 17. Model REMOVED

The model is no longer available as an active execution target.

Historical records may still reference its identity.

`REMOVED` is terminal.

No new lease may reference a removed model.

---

## 18. Provider Model Diagram

```text
REGISTERED
    ├──► ACTIVE
    ├──► DISABLED
    └──► REMOVED

ACTIVE
    ├──► DEPRECATED
    ├──► DISABLED
    └──► REMOVED

DEPRECATED
    ├──► ACTIVE
    ├──► DISABLED
    └──► REMOVED

DISABLED
    ├──► ACTIVE
    ├──► DEPRECATED
    └──► REMOVED

REMOVED
    └──► terminal
```

---

# Part III — Provider Lease State Machine

## 19. ProviderLeaseState

Canonical states:

```text
REQUESTED
GRANTED
ACTIVE
RELEASE_REQUESTED
RELEASED
EXPIRED
REVOKED
REJECTED
FAILED
```

---

## 20. Lease REQUESTED

A consumer requested bounded provider access.

At this point:

- lease identity may already exist;
- provider selection may be referenced;
- eligibility is being verified;
- current provider state is being checked;
- credential and client readiness may be checked;
- no execution handle is usable yet.

Valid outgoing transitions:

```text
REQUESTED → GRANTED
REQUESTED → REJECTED
REQUESTED → FAILED
REQUESTED → EXPIRED
```

`EXPIRED` is allowed when the lease request itself has a request deadline.

Expected event may include:

```text
ProviderLeaseRequested
```

This event may remain internal.

---

## 21. Lease GRANTED

Provider access has been authorized and an execution handle is available or resolvable.

At this point:

- provider and model are fixed;
- policy snapshot is fixed;
- capability snapshot is fixed;
- relevant configuration revisions are recorded;
- lease expiration is defined;
- execution may not yet have started;
- Runtime admission may still be pending.

Valid outgoing transitions:

```text
GRANTED → ACTIVE
GRANTED → RELEASE_REQUESTED
GRANTED → RELEASED
GRANTED → EXPIRED
GRANTED → REVOKED
GRANTED → FAILED
```

Expected event:

```text
ProviderLeaseGranted
```

---

## 22. Lease ACTIVE

The execution handle is currently being used for provider access.

`ACTIVE` does not mean:

- the provider request will succeed;
- Runtime work is complete;
- the consumer operation is authoritative;
- the domain job will complete.

Valid outgoing transitions:

```text
ACTIVE → RELEASE_REQUESTED
ACTIVE → RELEASED
ACTIVE → EXPIRED
ACTIVE → REVOKED
ACTIVE → FAILED
```

Expected event may include:

```text
ProviderLeaseActivated
```

This event may remain internal for MVP.

---

## 23. Lease RELEASE_REQUESTED

Normal lease release has been requested, but local cleanup may still be in progress.

At this point:

- no new provider operations may begin through the lease;
- active provider execution may be allowed to finish or may be cancelled according to policy;
- the handle is logically closing;
- client or model references may still be held temporarily.

Valid outgoing transitions:

```text
RELEASE_REQUESTED → RELEASED
RELEASE_REQUESTED → EXPIRED
RELEASE_REQUESTED → REVOKED
RELEASE_REQUESTED → FAILED
```

---

## 24. Lease RELEASED

The lease ended normally.

At this point:

- no execution handle use is permitted;
- provider resources may be returned to pools;
- local model residency may remain unaffected;
- historical lease metadata remains queryable.

`RELEASED` is terminal.

Expected event:

```text
ProviderLeaseReleased
```

---

## 25. Lease EXPIRED

The lease lifetime ended before normal release completed.

Possible causes:

- execution never began;
- consumer failed to release;
- execution exceeded lease duration;
- grace period ended;
- provider configuration revision invalidated the lease under policy.

At this point:

- no new operation may begin;
- active physical execution may still require Runtime cancellation or adapter cleanup;
- the handle is no longer valid for consumer use.

`EXPIRED` is terminal.

Expected event:

```text
ProviderLeaseExpired
```

---

## 26. Lease REVOKED

Provider Management withdrew the lease before normal completion.

Possible causes:

- provider disabled under immediate-revoke policy;
- credentials revoked;
- security policy changed;
- provider region became prohibited;
- adapter integrity failed;
- local resource became unsafe;
- application shutdown;
- administrative revoke command.

At this point:

- the execution handle is logically invalid;
- no new operation may start;
- Runtime and the consumer must be notified;
- physical cancellation remains best-effort and belongs to Runtime or adapter execution.

`REVOKED` is terminal.

Expected event:

```text
ProviderLeaseRevoked
```

---

## 27. Lease REJECTED

The lease request was validly processed but access was not granted.

Possible reasons:

- provider no longer eligible;
- provider disabled;
- model disabled;
- circuit open;
- credential unavailable;
- no local resources;
- lease limit exceeded;
- privacy policy conflict;
- selection result stale.

`REJECTED` is terminal.

Expected event may include:

```text
ProviderLeaseRejected
```

---

## 28. Lease FAILED

Lease creation or lifecycle management failed unexpectedly.

Examples:

- execution handle creation failed;
- internal client binding failed;
- lease persistence failed;
- credential resolution failed unexpectedly;
- local model binding failed;
- consistency check failed.

`FAILED` is terminal.

Expected event:

```text
ProviderLeaseFailed
```

Detailed normalized failures belong in `ERRORS.md`.

---

## 29. Provider Lease Diagram

```text
REQUESTED
    ├──► GRANTED
    ├──► REJECTED
    ├──► FAILED
    └──► EXPIRED

GRANTED
    ├──► ACTIVE
    ├──► RELEASE_REQUESTED
    ├──► RELEASED
    ├──► EXPIRED
    ├──► REVOKED
    └──► FAILED

ACTIVE
    ├──► RELEASE_REQUESTED
    ├──► RELEASED
    ├──► EXPIRED
    ├──► REVOKED
    └──► FAILED

RELEASE_REQUESTED
    ├──► RELEASED
    ├──► EXPIRED
    ├──► REVOKED
    └──► FAILED
```

Terminal states:

```text
RELEASED
EXPIRED
REVOKED
REJECTED
FAILED
```

---

# Part IV — Provider Availability State Machine

## 30. ProviderAvailabilityState

Canonical normalized availability states:

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

Availability is a derived eligibility summary.

It may be recomputed without changing provider definition state.

---

## 31. Availability UNKNOWN

Provider Management lacks enough current information to determine availability.

Possible causes:

- provider just registered;
- health not yet checked;
- model catalog unresolved;
- credential status unknown;
- local model state unknown;
- stale operational snapshot.

`UNKNOWN` should normally be treated conservatively.

Eligibility policy may:

- exclude the provider;
- permit an explicit probe;
- permit only diagnostic lease requests;
- lower ranking.

---

## 32. Availability AVAILABLE

The provider is currently eligible for normal consideration, subject to request-specific capability and policy checks.

`AVAILABLE` requires at least:

- provider enabled;
- model eligible;
- required credentials available where applicable;
- circuit not open;
- operational health acceptable;
- no mandatory maintenance;
- no hard local resource block.

It does not guarantee selection or execution success.

---

## 33. Availability DEGRADED

The provider remains usable but has reduced operational quality.

Possible reasons:

- elevated latency;
- intermittent failures;
- reduced capacity;
- partial model catalog;
- non-critical health issues;
- near-quota condition;
- degraded local device.

Selection may lower its ranking.

Hard requirements may still permit it.

---

## 34. Availability UNAVAILABLE

The provider cannot currently accept new work.

Possible reasons:

- unreachable endpoint;
- provider-wide outage;
- local process unavailable;
- no eligible model;
- failed health probe under strict policy;
- provider internal failure.

Existing leases may drain or be revoked according to policy.

---

## 35. Availability DISABLED

The provider definition is administratively disabled.

This availability state is derived from:

```text
ProviderDefinitionState = DISABLED
```

No new selection or lease is permitted.

---

## 36. Availability MAINTENANCE

The provider is intentionally unavailable or restricted for maintenance.

Maintenance may be:

- administrative;
- provider-announced;
- local model update;
- adapter migration;
- credential rotation;
- model catalog refresh.

Lease behavior depends on drain policy.

---

## 37. Availability LOADING

A required local model or provider resource is preparing.

The provider path is not yet ready for normal execution.

Selection may:

- wait;
- choose another provider;
- return a pending path only when supported;
- trigger local-model loading.

---

## 38. Availability DRAINING

No new leases should be granted, but existing leases may finish.

Possible causes:

- provider disable with drain policy;
- model deprecation;
- configuration replacement;
- application shutdown;
- client pool replacement;
- local model unload preparation.

---

## 39. Availability RATE_LIMITED

New provider work is temporarily constrained by rate or quota limits.

This state may contain:

- retry-after;
- remaining quota;
- reset time;
- constrained capability;
- constrained model;
- constrained credential scope.

A fallback provider may still be selected.

---

## 40. Availability CIRCUIT_OPEN

The relevant provider path is blocked by an open circuit breaker.

The provider definition remains enabled unless separately disabled.

New normal leases are prohibited for the affected circuit scope.

Controlled half-open probes may be allowed.

---

## 41. Availability RESOURCE_CONSTRAINED

A local or managed provider path lacks sufficient admitted resources.

Possible causes:

- insufficient RAM;
- insufficient VRAM;
- GPU unavailable;
- concurrency capacity exhausted;
- local process limit;
- Runtime admission denied.

Provider Management reports resource requirements.

Runtime or Resource Management owns actual resource admission.

---

## 42. Availability CREDENTIAL_UNAVAILABLE

The provider path requires credentials that cannot currently be resolved or used.

Possible causes:

- credential reference missing;
- credential expired;
- secret manager unavailable;
- user credential locked;
- credential revision revoked.

Raw secret details must not be exposed.

---

## 43. Availability Derivation

Availability may be derived conceptually from:

```text
ProviderDefinitionState
+
ProviderModelState
+
ProviderHealthState
+
ProviderCircuitState
+
CredentialAvailability
+
RateLimitState
+
LocalModelState
+
RuntimeResourceSignal
+
MaintenancePolicy
```

Precedence should be explicit.

A suggested high-level precedence is:

```text
ARCHIVED / DISABLED
    ↓
SECURITY OR CREDENTIAL BLOCK
    ↓
MAINTENANCE / DRAINING
    ↓
CIRCUIT OPEN
    ↓
UNAVAILABLE
    ↓
RESOURCE CONSTRAINED
    ↓
RATE LIMITED
    ↓
LOADING
    ↓
DEGRADED
    ↓
AVAILABLE
```

Exact derivation belongs to policy implementation.

---

# Part V — Provider Health State Machine

## 44. ProviderHealthState

Canonical states:

```text
UNKNOWN
HEALTHY
DEGRADED
UNHEALTHY
```

Health should remain simpler than availability.

---

## 45. Health UNKNOWN

Not enough evidence exists to evaluate health.

Valid outgoing transitions:

```text
UNKNOWN → HEALTHY
UNKNOWN → DEGRADED
UNKNOWN → UNHEALTHY
```

---

## 46. Health HEALTHY

Provider-relevant evidence indicates normal operation.

Possible evidence:

- successful active probe;
- recent successful executions;
- acceptable latency;
- valid credentials;
- normal malformed-response rate;
- stable local process.

Valid outgoing transitions:

```text
HEALTHY → DEGRADED
HEALTHY → UNHEALTHY
HEALTHY → UNKNOWN
```

---

## 47. Health DEGRADED

The provider remains operational but exhibits concerning signals.

Possible signals:

- elevated timeout rate;
- intermittent connection errors;
- increased malformed responses;
- high latency;
- local resource pressure;
- partial regional outage;
- repeated but recoverable provider failures.

Valid outgoing transitions:

```text
DEGRADED → HEALTHY
DEGRADED → UNHEALTHY
DEGRADED → UNKNOWN
```

---

## 48. Health UNHEALTHY

Provider-relevant evidence indicates the path should not normally receive new work.

Possible causes:

- repeated connection failure;
- repeated provider internal failure;
- failed health probes;
- corrupted local model;
- unusable adapter state;
- deterministic provider-wide failure.

Valid outgoing transitions:

```text
UNHEALTHY → DEGRADED
UNHEALTHY → HEALTHY
UNHEALTHY → UNKNOWN
```

Recovery requires new evidence.

---

## 49. Health Attribution Rule

The following must not directly degrade provider health:

- invalid Translation source;
- Translation alignment defect;
- Recognition source-region defect;
- Reading Session stale-result rejection;
- Presentation render failure;
- user cancellation;
- unsupported consumer configuration that eligibility checks should reject;
- domain validation failure proven unrelated to provider behavior.

Only provider-relevant evidence may affect health.

---

## 50. Provider Health Diagram

```text
UNKNOWN
    ├──► HEALTHY
    ├──► DEGRADED
    └──► UNHEALTHY

HEALTHY
    ├──► DEGRADED
    ├──► UNHEALTHY
    └──► UNKNOWN

DEGRADED
    ├──► HEALTHY
    ├──► UNHEALTHY
    └──► UNKNOWN

UNHEALTHY
    ├──► DEGRADED
    ├──► HEALTHY
    └──► UNKNOWN
```

---

# Part VI — Provider Circuit State Machine

## 51. ProviderCircuitState

Canonical states:

```text
CLOSED
OPEN
HALF_OPEN
```

---

## 52. Circuit CLOSED

Normal provider selection and lease behavior is allowed.

Failure observations may increment circuit metrics.

Valid outgoing transition:

```text
CLOSED → OPEN
```

Possible trigger:

- configured failure threshold reached;
- deterministic severe provider failure;
- administrative open command.

---

## 53. Circuit OPEN

Normal selection and leases are blocked for the circuit scope.

At this point:

- new normal leases are rejected;
- availability may become `CIRCUIT_OPEN`;
- fallback candidates may be considered;
- cooldown timing belongs to operational policy;
- existing leases follow revocation or drain policy.

Valid outgoing transitions:

```text
OPEN → HALF_OPEN
OPEN → CLOSED
```

Direct `OPEN → CLOSED` should require explicit administrative reset or trusted health evidence.

---

## 54. Circuit HALF_OPEN

A limited number of controlled probe executions may be allowed.

Normal provider traffic remains restricted.

Valid outgoing transitions:

```text
HALF_OPEN → CLOSED
HALF_OPEN → OPEN
```

Success returns the circuit to `CLOSED`.

Failure returns it to `OPEN`.

---

## 55. Circuit Scope

Circuit state may be scoped by:

```text
ProviderId
ProviderModelId
Capability
Region
CredentialReferenceId
Endpoint
LocalModelInstanceId
```

Opening one scoped circuit must not automatically block unrelated provider paths unless policy explicitly links them.

---

## 56. Circuit Diagram

```text
CLOSED
    ↓ threshold reached
OPEN
    ↓ cooldown or controlled probe
HALF_OPEN
    ├── success ──► CLOSED
    └── failure ──► OPEN
```

---

# Part VII — Local Model State Machine

## 57. LocalModelState

Canonical states:

```text
UNREGISTERED
REGISTERED
INSTALLING
INSTALLED
VALIDATING
LOADING
READY
BUSY
UNLOADING
UNLOADED
FAILED
REMOVED
```

Not every deployment must use every state.

---

## 58. Local Model UNREGISTERED

No Provider Management model definition currently exists.

This state may remain conceptual rather than persisted.

Valid transition:

```text
UNREGISTERED → REGISTERED
```

---

## 59. Local Model REGISTERED

The logical local model definition exists, but model files may not yet be installed.

Valid outgoing transitions:

```text
REGISTERED → INSTALLING
REGISTERED → INSTALLED
REGISTERED → REMOVED
```

Direct transition to `INSTALLED` is allowed when files already exist and are adopted.

---

## 60. Local Model INSTALLING

Model files or required runtime components are being installed.

Valid outgoing transitions:

```text
INSTALLING → INSTALLED
INSTALLING → FAILED
INSTALLING → REMOVED
```

Installation progress may be tracked separately.

---

## 61. Local Model INSTALLED

Required files exist, but integrity or runtime compatibility may not yet be validated.

Valid outgoing transitions:

```text
INSTALLED → VALIDATING
INSTALLED → LOADING
INSTALLED → REMOVED
INSTALLED → FAILED
```

Direct loading is permitted only when validation is already trusted and recorded.

---

## 62. Local Model VALIDATING

Provider Management is verifying:

- file integrity;
- model metadata;
- runtime compatibility;
- device compatibility;
- required tokenizer or auxiliary files;
- adapter compatibility;
- security policy.

Valid outgoing transitions:

```text
VALIDATING → INSTALLED
VALIDATING → LOADING
VALIDATING → FAILED
VALIDATING → REMOVED
```

Returning to `INSTALLED` means validation succeeded but loading was not requested.

---

## 63. Local Model LOADING

The model is being loaded into an execution runtime.

Preconditions include:

- model installed;
- model validated;
- ProviderDefinition enabled;
- ProviderModel active or explicitly permitted;
- Runtime or Resource admission granted.

Valid outgoing transitions:

```text
LOADING → READY
LOADING → FAILED
LOADING → UNLOADING
```

Expected event:

```text
LocalModelLoadStarted
```

---

## 64. Local Model READY

The model is loaded and capable of accepting work, subject to Runtime admission and lease rules.

`READY` does not guarantee provider availability when:

- circuit is open;
- credentials or auxiliary service are unavailable;
- provider disabled;
- concurrency exhausted;
- Runtime denies execution resources.

Valid outgoing transitions:

```text
READY → BUSY
READY → UNLOADING
READY → FAILED
```

Expected event:

```text
LocalModelLoaded
```

---

## 65. Local Model BUSY

The loaded model currently has one or more active executions or has reached the configured busy threshold.

`BUSY` may still allow more work if capacity remains.

Valid outgoing transitions:

```text
BUSY → READY
BUSY → UNLOADING
BUSY → FAILED
```

A transition to `UNLOADING` should normally wait for active leases to drain unless immediate shutdown is required.

---

## 66. Local Model UNLOADING

The model is releasing resident resources.

At this point:

- no new lease should bind to the instance;
- existing execution follows drain or revoke policy;
- Runtime owns physical resource cleanup scheduling;
- Provider Management owns lifecycle intent and state.

Valid outgoing transitions:

```text
UNLOADING → UNLOADED
UNLOADING → FAILED
```

---

## 67. Local Model UNLOADED

The model remains installed but is no longer resident.

Valid outgoing transitions:

```text
UNLOADED → LOADING
UNLOADED → VALIDATING
UNLOADED → REMOVED
UNLOADED → FAILED
```

---

## 68. Local Model FAILED

The local model cannot currently continue its intended lifecycle.

Possible causes:

- install failure;
- corrupted files;
- load failure;
- incompatible runtime;
- insufficient resources after admission;
- process crash;
- unload failure;
- adapter failure.

Valid outgoing transitions:

```text
FAILED → INSTALLING
FAILED → VALIDATING
FAILED → LOADING
FAILED → UNLOADING
FAILED → REMOVED
```

Recovery requires explicit action or policy.

---

## 69. Local Model REMOVED

The local model files or definition are no longer available for execution.

Historical identity may remain.

`REMOVED` is terminal for the current local model identity.

---

## 70. Local Model Diagram

```text
REGISTERED
    ↓
INSTALLING
    ↓
INSTALLED
    ↓
VALIDATING
    ↓
LOADING
    ↓
READY
    ↕
BUSY
    ↓
UNLOADING
    ↓
UNLOADED
    └──► LOADING
```

Failure path:

```text
INSTALLING
INSTALLED
VALIDATING
LOADING
READY
BUSY
UNLOADING
UNLOADED
    └──► FAILED
```

Removal path:

```text
REGISTERED
INSTALLING
INSTALLED
VALIDATING
UNLOADED
FAILED
    └──► REMOVED
```

---

# Part VIII — Entity State Relationships

## 71. Provider and Model Relationship

A model may be `ACTIVE` while its provider is `DISABLED`.

However, it cannot be selected in that condition.

Eligibility requires:

```text
ProviderDefinitionState = ENABLED
+
ProviderModelState = ACTIVE
```

or explicit policy allowing a deprecated model.

---

## 72. Provider and Lease Relationship

A new lease may be granted only when:

```text
ProviderDefinitionState = ENABLED
```

and the selected provider path is eligible.

No new lease may be granted when provider state is:

```text
REGISTERED
DISABLED
ARCHIVED
```

---

## 73. Model and Lease Relationship

A new lease normally requires:

```text
ProviderModelState = ACTIVE
```

A `DEPRECATED` model may be leased only under explicit policy.

No new lease may reference:

```text
DISABLED
REMOVED
```

---

## 74. Lease and Runtime Relationship

Conceptually:

```text
ProviderLease.GRANTED
    ↓
Runtime admission succeeds
    ↓
ProviderLease.ACTIVE
```

Runtime admission failure does not automatically mean the provider is unhealthy.

The lease may:

- remain granted until expiration;
- be released;
- be rejected before grant;
- fail due to handle creation;
- be revoked according to policy.

---

## 75. Availability and Definition Relationship

Recommended mapping:

```text
ProviderDefinition.DISABLED
    → Availability.DISABLED

ProviderDefinition.ARCHIVED
    → no selectable availability
```

An archived provider should not be represented as normally operational.

---

## 76. Availability and Circuit Relationship

```text
ProviderCircuit.OPEN
    → Availability.CIRCUIT_OPEN
```

for the affected provider path.

`HALF_OPEN` may allow controlled probe eligibility.

---

## 77. Availability and Health Relationship

Suggested mapping:

```text
Health.HEALTHY
    → may support Availability.AVAILABLE

Health.DEGRADED
    → may support Availability.DEGRADED

Health.UNHEALTHY
    → normally supports Availability.UNAVAILABLE
```

Other state dimensions may override this mapping.

---

## 78. Availability and Local Model Relationship

Examples:

```text
LocalModel.LOADING
    → Availability.LOADING

LocalModel.READY
    → may support Availability.AVAILABLE

LocalModel.BUSY
    → AVAILABLE, DEGRADED, or RESOURCE_CONSTRAINED

LocalModel.FAILED
    → Availability.UNAVAILABLE

LocalModel.UNLOADED
    → Availability.LOADING or UNAVAILABLE
```

depending on residency and on-demand loading policy.

---

## 79. Lease and Provider Disable Relationship

Provider disable policy may be:

```text
ALLOW_DRAIN
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE_PERIOD
```

### ALLOW_DRAIN

```text
Provider ENABLED → DISABLED
Active leases remain ACTIVE
No new leases granted
```

### REVOKE_IMMEDIATELY

```text
Provider ENABLED → DISABLED
Active leases → REVOKED
```

### REVOKE_AFTER_GRACE_PERIOD

```text
Provider ENABLED → DISABLED
Active leases remain ACTIVE temporarily
Grace period expires
Remaining leases → REVOKED
```

---

# Part IX — Command-to-State Mapping

## 80. RegisterProvider

Accepted command creates:

```text
ProviderDefinition = REGISTERED
```

It must not automatically imply availability.

---

## 81. EnableProvider

Valid target states:

```text
REGISTERED
DISABLED
```

Transition:

```text
REGISTERED → ENABLED
DISABLED → ENABLED
```

Enabling must validate required configuration according to policy.

---

## 82. DisableProvider

Valid target states:

```text
REGISTERED
ENABLED
```

Transition:

```text
REGISTERED → DISABLED
ENABLED → DISABLED
```

Active leases follow disable policy.

---

## 83. ArchiveProvider

Valid target states:

```text
REGISTERED
ENABLED
DISABLED
```

Preferred administrative flow:

```text
ENABLED → DISABLED → ARCHIVED
```

Direct archival may be allowed when no active leases exist.

---

## 84. RegisterProviderModel

Creates:

```text
ProviderModel = REGISTERED
```

---

## 85. ActivateProviderModel

Valid transitions:

```text
REGISTERED → ACTIVE
DEPRECATED → ACTIVE
DISABLED → ACTIVE
```

Activation must validate model metadata and provider association.

---

## 86. DeprecateProviderModel

Valid transition:

```text
ACTIVE → DEPRECATED
```

Existing leases may drain.

---

## 87. DisableProviderModel

Valid transitions:

```text
REGISTERED → DISABLED
ACTIVE → DISABLED
DEPRECATED → DISABLED
```

---

## 88. RemoveProviderModel

Valid transitions:

```text
REGISTERED → REMOVED
ACTIVE → REMOVED
DEPRECATED → REMOVED
DISABLED → REMOVED
```

Removal with active leases should normally be rejected or converted into disable-and-drain.

---

## 89. RequestProviderLease

Normal flow:

```text
REQUESTED
    ↓ eligibility succeeds
GRANTED
```

Failure flows:

```text
REQUESTED → REJECTED
REQUESTED → FAILED
REQUESTED → EXPIRED
```

---

## 90. ActivateProviderLease

Valid transition:

```text
GRANTED → ACTIVE
```

Activation should occur when provider use actually begins.

---

## 91. ReleaseProviderLease

Valid transitions:

```text
GRANTED → RELEASE_REQUESTED → RELEASED
ACTIVE → RELEASE_REQUESTED → RELEASED
```

Atomic release may permit:

```text
GRANTED → RELEASED
ACTIVE → RELEASED
```

---

## 92. RevokeProviderLease

Valid transitions:

```text
GRANTED → REVOKED
ACTIVE → REVOKED
RELEASE_REQUESTED → REVOKED
```

---

## 93. RefreshProviderHealth

May transition:

```text
UNKNOWN ↔ HEALTHY
UNKNOWN ↔ DEGRADED
UNKNOWN ↔ UNHEALTHY
HEALTHY ↔ DEGRADED
HEALTHY ↔ UNHEALTHY
DEGRADED ↔ UNHEALTHY
```

Health changes require provider-relevant evidence.

---

## 94. ResetProviderCircuit

Typical administrative transitions:

```text
OPEN → HALF_OPEN
OPEN → CLOSED
HALF_OPEN → CLOSED
```

Direct closing should be auditable.

---

## 95. LoadLocalModel

Typical flow:

```text
INSTALLED or UNLOADED
    ↓
VALIDATING when required
    ↓
LOADING
    ↓
READY
```

Runtime or Resource admission is required before physical loading.

---

## 96. UnloadLocalModel

Typical flow:

```text
READY or BUSY
    ↓
UNLOADING
    ↓
UNLOADED
```

`BUSY → UNLOADING` requires drain or revoke policy.

---

# Part X — Event-to-State Mapping

## 97. Provider Definition Events

| Event | Expected state after event |
|---|---|
| `ProviderRegistered` | `REGISTERED` |
| `ProviderEnabled` | `ENABLED` |
| `ProviderDisabled` | `DISABLED` |
| `ProviderArchived` | `ARCHIVED` |

---

## 98. Provider Model Events

| Event | Expected state after event |
|---|---|
| `ProviderModelRegistered` | `REGISTERED` |
| `ProviderModelActivated` | `ACTIVE` |
| `ProviderModelDeprecated` | `DEPRECATED` |
| `ProviderModelDisabled` | `DISABLED` |
| `ProviderModelRemoved` | `REMOVED` |

---

## 99. Provider Lease Events

| Event | Expected state after event |
|---|---|
| `ProviderLeaseRequested` | `REQUESTED` |
| `ProviderLeaseGranted` | `GRANTED` |
| `ProviderLeaseActivated` | `ACTIVE` |
| `ProviderLeaseReleaseRequested` | `RELEASE_REQUESTED` |
| `ProviderLeaseReleased` | `RELEASED` |
| `ProviderLeaseExpired` | `EXPIRED` |
| `ProviderLeaseRevoked` | `REVOKED` |
| `ProviderLeaseRejected` | `REJECTED` |
| `ProviderLeaseFailed` | `FAILED` |

Some request and activation events may remain internal.

---

## 100. Availability Events

| Event | Expected state after event |
|---|---|
| `ProviderAvailabilityChanged` | payload availability state |
| `ProviderEnteredMaintenance` | `MAINTENANCE` |
| `ProviderStartedDraining` | `DRAINING` |
| `ProviderRateLimited` | `RATE_LIMITED` |
| `ProviderResourceConstrained` | `RESOURCE_CONSTRAINED` |
| `ProviderCredentialUnavailable` | `CREDENTIAL_UNAVAILABLE` |

---

## 101. Health Events

| Event | Expected state after event |
|---|---|
| `ProviderHealthChanged` | payload health state |
| `ProviderHealthRecovered` | `HEALTHY` or `DEGRADED` |
| `ProviderHealthBecameUnhealthy` | `UNHEALTHY` |

---

## 102. Circuit Events

| Event | Expected state after event |
|---|---|
| `ProviderCircuitOpened` | `OPEN` |
| `ProviderCircuitHalfOpened` | `HALF_OPEN` |
| `ProviderCircuitClosed` | `CLOSED` |

---

## 103. Local Model Events

| Event | Expected state after event |
|---|---|
| `LocalModelRegistered` | `REGISTERED` |
| `LocalModelInstallStarted` | `INSTALLING` |
| `LocalModelInstalled` | `INSTALLED` |
| `LocalModelValidationStarted` | `VALIDATING` |
| `LocalModelLoadStarted` | `LOADING` |
| `LocalModelLoaded` | `READY` |
| `LocalModelBecameBusy` | `BUSY` |
| `LocalModelUnloadStarted` | `UNLOADING` |
| `LocalModelUnloaded` | `UNLOADED` |
| `LocalModelFailed` | `FAILED` |
| `LocalModelRemoved` | `REMOVED` |

---

# Part XI — State Persistence

## 104. Durable Transition Rule

A public event must be published only after its corresponding state transition is durable enough to be queried.

Example:

```text
persist ProviderDefinition = ENABLED
    ↓
publish ProviderEnabled
```

Another example:

```text
persist ProviderLease = GRANTED
persist execution handle reference
    ↓
publish ProviderLeaseGranted
```

---

## 105. Transactional Consistency

Preferred mechanisms include:

- transactional outbox;
- atomic aggregate persistence;
- durable event log;
- equivalent reliable state-and-event storage.

The system must avoid:

```text
event published
state update failed
```

and:

```text
lease granted event published
execution handle not retrievable
```

---

## 106. Optimistic Concurrency

Transitions should validate expected state and revision.

Conceptual operation:

```text
transition(
    entityId,
    expectedState,
    nextState,
    expectedStateRevision
)
```

This prevents:

- provider disable racing with lease grant;
- duplicate lease activation;
- release racing with revoke;
- model activation racing with removal;
- local model load racing with unload;
- circuit close racing with new failure threshold;
- stale health observations overwriting newer health state.

---

## 107. State Revisions

Each stateful entity should maintain an internal monotonic revision.

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

State revision supports:

- optimistic locking;
- event ordering;
- duplicate command handling;
- read-model synchronization;
- stale observation rejection.

State revision is distinct from provider definition revision and model metadata revision.

---

# Part XII — Invalid Transitions

## 108. Provider Definition Invalid Transitions

Forbidden:

```text
ARCHIVED → ENABLED
ARCHIVED → DISABLED
ARCHIVED → REGISTERED
```

Restoration requires explicit migration or a new provider identity.

---

## 109. Provider Model Invalid Transitions

Forbidden:

```text
REMOVED → ACTIVE
REMOVED → DEPRECATED
REMOVED → DISABLED
REMOVED → REGISTERED
```

---

## 110. Provider Lease Invalid Transitions

Forbidden:

```text
RELEASED → ACTIVE
EXPIRED → ACTIVE
REVOKED → ACTIVE
REJECTED → GRANTED
FAILED → ACTIVE

RELEASED → GRANTED
EXPIRED → GRANTED
REVOKED → GRANTED
```

A new lease request is required.

---

## 111. Circuit Invalid Transitions

Forbidden without explicit administrative semantics:

```text
CLOSED → HALF_OPEN
```

`HALF_OPEN` is entered from `OPEN`.

---

## 112. Local Model Invalid Transitions

Forbidden:

```text
REMOVED → READY
REMOVED → LOADING
REMOVED → INSTALLED
```

A new local model identity or installation record is required.

Direct transitions should also be prohibited when prerequisites are absent:

```text
REGISTERED → READY
UNLOADED → READY
FAILED → READY
```

---

# Part XIII — Transition Tables

## 113. Provider Definition Transition Table

| Current state | Trigger | Next state |
|---|---|---|
| `REGISTERED` | enable accepted | `ENABLED` |
| `REGISTERED` | disable accepted | `DISABLED` |
| `REGISTERED` | archive accepted | `ARCHIVED` |
| `ENABLED` | disable accepted | `DISABLED` |
| `ENABLED` | archive accepted | `ARCHIVED` |
| `DISABLED` | enable accepted | `ENABLED` |
| `DISABLED` | archive accepted | `ARCHIVED` |

---

## 114. Provider Model Transition Table

| Current state | Trigger | Next state |
|---|---|---|
| `REGISTERED` | activation succeeds | `ACTIVE` |
| `REGISTERED` | disabled | `DISABLED` |
| `REGISTERED` | removed | `REMOVED` |
| `ACTIVE` | deprecated | `DEPRECATED` |
| `ACTIVE` | disabled | `DISABLED` |
| `ACTIVE` | removed | `REMOVED` |
| `DEPRECATED` | reactivated | `ACTIVE` |
| `DEPRECATED` | disabled | `DISABLED` |
| `DEPRECATED` | removed | `REMOVED` |
| `DISABLED` | activated | `ACTIVE` |
| `DISABLED` | marked deprecated | `DEPRECATED` |
| `DISABLED` | removed | `REMOVED` |

---

## 115. Provider Lease Transition Table

| Current state | Trigger | Next state |
|---|---|---|
| `REQUESTED` | eligibility and handle preparation succeed | `GRANTED` |
| `REQUESTED` | policy or eligibility rejects request | `REJECTED` |
| `REQUESTED` | unexpected lease creation failure | `FAILED` |
| `REQUESTED` | request deadline expires | `EXPIRED` |
| `GRANTED` | execution begins | `ACTIVE` |
| `GRANTED` | release requested | `RELEASE_REQUESTED` |
| `GRANTED` | released atomically | `RELEASED` |
| `GRANTED` | lease deadline expires | `EXPIRED` |
| `GRANTED` | revocation accepted | `REVOKED` |
| `GRANTED` | handle lifecycle fails | `FAILED` |
| `ACTIVE` | release requested | `RELEASE_REQUESTED` |
| `ACTIVE` | released atomically | `RELEASED` |
| `ACTIVE` | lease deadline expires | `EXPIRED` |
| `ACTIVE` | revocation accepted | `REVOKED` |
| `ACTIVE` | lease lifecycle fails | `FAILED` |
| `RELEASE_REQUESTED` | cleanup succeeds | `RELEASED` |
| `RELEASE_REQUESTED` | deadline expires | `EXPIRED` |
| `RELEASE_REQUESTED` | revocation overrides release | `REVOKED` |
| `RELEASE_REQUESTED` | cleanup fails | `FAILED` |

---

## 116. Provider Health Transition Table

| Current state | Trigger | Next state |
|---|---|---|
| `UNKNOWN` | healthy evidence | `HEALTHY` |
| `UNKNOWN` | degraded evidence | `DEGRADED` |
| `UNKNOWN` | unhealthy evidence | `UNHEALTHY` |
| `HEALTHY` | degradation threshold reached | `DEGRADED` |
| `HEALTHY` | severe failure evidence | `UNHEALTHY` |
| `HEALTHY` | evidence expires | `UNKNOWN` |
| `DEGRADED` | recovery evidence | `HEALTHY` |
| `DEGRADED` | failure threshold reached | `UNHEALTHY` |
| `DEGRADED` | evidence expires | `UNKNOWN` |
| `UNHEALTHY` | partial recovery | `DEGRADED` |
| `UNHEALTHY` | verified recovery | `HEALTHY` |
| `UNHEALTHY` | evidence reset | `UNKNOWN` |

---

## 117. Provider Circuit Transition Table

| Current state | Trigger | Next state |
|---|---|---|
| `CLOSED` | failure threshold reached | `OPEN` |
| `CLOSED` | administrative open | `OPEN` |
| `OPEN` | cooldown allows probe | `HALF_OPEN` |
| `OPEN` | administrative trusted reset | `CLOSED` |
| `HALF_OPEN` | probe succeeds | `CLOSED` |
| `HALF_OPEN` | probe fails | `OPEN` |

---

## 118. Local Model Transition Table

| Current state | Trigger | Next state |
|---|---|---|
| `REGISTERED` | installation begins | `INSTALLING` |
| `REGISTERED` | existing files adopted | `INSTALLED` |
| `REGISTERED` | removed | `REMOVED` |
| `INSTALLING` | installation succeeds | `INSTALLED` |
| `INSTALLING` | installation fails | `FAILED` |
| `INSTALLING` | removed | `REMOVED` |
| `INSTALLED` | validation begins | `VALIDATING` |
| `INSTALLED` | trusted loading begins | `LOADING` |
| `INSTALLED` | failure detected | `FAILED` |
| `INSTALLED` | removed | `REMOVED` |
| `VALIDATING` | validation succeeds without load | `INSTALLED` |
| `VALIDATING` | validation succeeds and load begins | `LOADING` |
| `VALIDATING` | validation fails | `FAILED` |
| `VALIDATING` | removed | `REMOVED` |
| `LOADING` | load succeeds | `READY` |
| `LOADING` | load fails | `FAILED` |
| `LOADING` | unload requested | `UNLOADING` |
| `READY` | execution pressure reaches busy state | `BUSY` |
| `READY` | unload begins | `UNLOADING` |
| `READY` | model fails | `FAILED` |
| `BUSY` | active work falls below busy threshold | `READY` |
| `BUSY` | unload begins under policy | `UNLOADING` |
| `BUSY` | model fails | `FAILED` |
| `UNLOADING` | unload succeeds | `UNLOADED` |
| `UNLOADING` | unload fails | `FAILED` |
| `UNLOADED` | loading begins | `LOADING` |
| `UNLOADED` | revalidation begins | `VALIDATING` |
| `UNLOADED` | removed | `REMOVED` |
| `UNLOADED` | failure detected | `FAILED` |
| `FAILED` | reinstall begins | `INSTALLING` |
| `FAILED` | validation begins | `VALIDATING` |
| `FAILED` | reload begins | `LOADING` |
| `FAILED` | cleanup begins | `UNLOADING` |
| `FAILED` | removed | `REMOVED` |

---

# Part XIV — Cross-Module State Rules

## 119. Runtime Boundary

Provider Management may request or observe Runtime actions, but it does not own:

```text
Runtime queue state
Runtime worker state
Runtime work retry state
Runtime cancellation propagation state
Runtime resource admission state
```

Example:

```text
ProviderLease.GRANTED
+
Runtime admission denied
```

does not imply:

```text
ProviderHealth.UNHEALTHY
```

---

## 120. Translation Boundary

Translation may report normalized provider outcome feedback.

Translation must not directly transition:

```text
ProviderHealthState
ProviderCircuitState
ProviderAvailabilityState
ProviderDefinitionState
ProviderLeaseState
```

Provider Management evaluates feedback and performs its own transition.

---

## 121. Recognition Boundary

Recognition follows the same rule as Translation.

Recognition results or validation failures do not directly mutate provider state.

Only normalized provider-relevant evidence may influence health or circuit decisions.

---

## 122. Credential Boundary

Credential availability may influence:

```text
ProviderAvailability = CREDENTIAL_UNAVAILABLE
Lease REQUESTED → REJECTED
Lease GRANTED or ACTIVE → REVOKED
```

depending on policy.

Raw credential lifecycle remains inside the approved Secret boundary.

---

## 123. Reading Session Boundary

Reading Session state must not influence provider health directly.

Navigation or session closure may cause consumer work cancellation and lease release.

They do not imply provider failure.

---

# Part XV — State Invariants

## 124. Invariant 1 — Enabled provider requirement

A new lease may be granted only when:

```text
ProviderDefinitionState = ENABLED
```

---

## 125. Invariant 2 — Eligible model requirement

A new lease must reference an eligible model.

Normally:

```text
ProviderModelState = ACTIVE
```

A deprecated model requires explicit policy.

---

## 126. Invariant 3 — Lease terminality

A terminal lease never returns to `GRANTED` or `ACTIVE`.

Terminal lease states:

```text
RELEASED
EXPIRED
REVOKED
REJECTED
FAILED
```

---

## 127. Invariant 4 — Archived provider terminality

An archived provider cannot be enabled or selected.

---

## 128. Invariant 5 — Removed model terminality

A removed model cannot be activated or leased.

---

## 129. Invariant 6 — Availability is not ownership

Availability summarizes eligibility.

It does not replace definition, health, circuit, credential, or Runtime state.

---

## 130. Invariant 7 — Circuit independence

Opening a circuit does not mutate `ProviderDefinitionState`.

---

## 131. Invariant 8 — Health attribution

Consumer semantic failures and Presentation failures do not directly degrade provider health.

---

## 132. Invariant 9 — Runtime scheduling independence

Provider Management does not transition Runtime work state.

---

## 133. Invariant 10 — Local resource admission

A local model cannot enter `LOADING` through normal execution unless required Runtime or Resource admission succeeds.

---

## 134. Invariant 11 — Handle lifetime

An execution handle cannot be used after its lease becomes:

```text
RELEASED
EXPIRED
REVOKED
REJECTED
FAILED
```

---

## 135. Invariant 12 — Disable blocks new leases

Provider disablement prevents all new leases immediately after the disabling transition becomes authoritative.

---

## 136. Invariant 13 — Historical traceability

Provider disablement, archival, model removal, lease termination, and local model removal do not erase historical identity.

---

## 137. Invariant 14 — State durability before events

Public lifecycle events must reference durable, queryable state.

---

## 138. Invariant 15 — Revision-safe updates

Older health, availability, capability, and lifecycle observations must not overwrite newer state revisions.

---

## 139. Invariant 16 — Credential secrecy

No state snapshot contains raw credentials.

---

# Part XVI — MVP State Scope

## 140. Required MVP State Machines

The MVP requires:

```text
ProviderDefinitionState
ProviderModelState
ProviderLeaseState
ProviderAvailabilityState
ProviderHealthState
ProviderCircuitState
LocalModelState
```

---

## 141. Required MVP Behaviors

The MVP state model must support:

- provider registration;
- provider enable and disable;
- provider archival;
- model registration and activation;
- model deprecation and disablement;
- provider lease grant;
- lease activation;
- normal lease release;
- lease expiration;
- lease revocation;
- provider availability changes;
- provider health degradation and recovery;
- circuit open, half-open, and close;
- local model load and unload;
- local model failure;
- provider disable with active leases;
- concurrency-safe transitions;
- event publication after durable state.

---

# Part XVII — Open Decisions

## 142. Provider Disable Default

Choose the default:

```text
ALLOW_DRAIN
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE_PERIOD
```

Recommended default:

```text
ALLOW_DRAIN
```

Security and credential revocation override the default.

---

## 143. Lease Activation Visibility

Decide whether `ProviderLeaseActivated` is:

- public;
- internal;
- omitted in favor of query state.

Recommended MVP behavior:

```text
internal by default
```

---

## 144. Availability Persistence

Decide whether availability is:

- persisted as a derived projection;
- recomputed on query;
- both persisted and periodically reconciled.

Recommended approach:

```text
persist normalized availability projection
+
retain source state revisions
```

---

## 145. Local Model BUSY State

Decide whether `BUSY` means:

- at least one active execution;
- concurrency limit reached;
- resource threshold reached.

Recommended meaning:

```text
capacity is materially constrained
```

Active execution count should remain separate metadata.

---

## 146. Health Evidence Expiration

Define when old evidence causes:

```text
HEALTHY / DEGRADED / UNHEALTHY
    → UNKNOWN
```

This belongs to operational policy.

---

## 147. Lease Failure Versus Rejection

Recommended distinction:

```text
REJECTED
    = valid decision not to grant access

FAILED
    = unexpected failure while creating or managing access
```

---

# Part XVIII — Related Documents

```text
02-modules/provider-management/MODULE.md
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/EVENTS.md
02-modules/provider-management/ERRORS.md
02-modules/provider-management/README.md
```

Architecture references:

```text
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Runtime references:

```text
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/MEMORY_MODEL.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/RUNTIME_CONFIG.md
```

Related module references:

```text
02-modules/translation/STATES.md
02-modules/translation/CONTRACT.md
02-modules/recognition/STATES.md
02-modules/reading-session/STATES.md
```

---

## 148. Summary

Provider Management maintains independent state machines for:

```text
ProviderDefinition
ProviderModel
ProviderLease
ProviderAvailability
ProviderHealth
ProviderCircuit
LocalModel
```

The central access lifecycle is:

```text
ProviderRequirement
    ↓
Provider Selection
    ↓
Lease REQUESTED
    ↓
Lease GRANTED
    ↓
Runtime execution begins
    ↓
Lease ACTIVE
    ↓
Lease RELEASED
```

Alternative terminal lease outcomes are:

```text
REJECTED
FAILED
EXPIRED
REVOKED
```

The most important ownership rules are:

```text
Provider Management
    owns provider access and lifecycle state

Runtime
    owns scheduling and physical work execution

Translation and Recognition
    own semantic task state

Reading Session
    owns current content authority
```

This document is the state-machine source of truth for subsequent Provider Management events, errors, and implementation behavior.
