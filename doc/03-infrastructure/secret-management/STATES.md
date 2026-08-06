# Secret Management States

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Secret Management  
> **Document:** State Machines  
> **Path:** `03-infrastructure/secret-management/STATES.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-05  
> **Source of Truth:**
>
> - `03-infrastructure/secret-management/MODULE.md`
> - `03-infrastructure/secret-management/CONTRACT.md`
> - `03-infrastructure/configuration/MODULE.md`
> - `03-infrastructure/configuration/CONTRACT.md`
> - `02-modules/provider-management/MODULE.md`
> - `02-modules/provider-management/CONTRACT.md`
> - `02-modules/provider-management/STATES.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/RESOURCE_LIFECYCLE.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`

---

## 1. Purpose

This document defines the lifecycle states and valid transitions owned by the Secret Management infrastructure module.

It covers:

- secret descriptor lifecycle;
- secret revision lifecycle;
- secret availability;
- secret lease lifecycle;
- secure backend lifecycle;
- backend lock and unlock behavior;
- secret registration and replacement candidates;
- secret rotation lifecycle;
- secret migration lifecycle;
- secret validation lifecycle;
- secret removal and tombstone behavior;
- operation uncertainty and reconciliation;
- active lease behavior during rotation, revocation, removal, migration, shutdown, and backend failure;
- state ownership;
- command-to-state mapping;
- concurrency and persistence rules;
- invalid transitions;
- cross-module boundaries.

This document does not define:

- command payload schemas;
- query payload schemas;
- event payload schemas;
- detailed error codes;
- encryption algorithms;
- operating-system keychain APIs;
- provider-native authentication states;
- Provider Management provider states;
- Runtime work-item states;
- UI states;
- database schemas;
- implementation classes.

---

## 2. State Ownership

Secret Management owns lifecycle state for:

```text
SecretDescriptor
SecretRevision
SecretAvailability
SecretLease
SecretBackend
SecretCandidate
SecretRotation
SecretMigration
SecretValidation
SecretRemoval
SecretOperation
SecretReconciliation
```

Secret Management does not own lifecycle state for:

```text
ConfigurationSnapshot
ProviderDefinition
ProviderModel
ProviderLease
ProviderClient
RuntimeWorkItem
RuntimeAttempt
TranslationJob
RecognitionJob
ReadingSession
Presentation
UserInterfacePrompt
ExternalProviderAccount
OperatingSystemUserSession
```

External state may trigger or constrain a Secret Management transition, but ownership remains with the originating module.

Examples:

- an operating-system keychain lock affects `SecretBackendState`;
- a provider authentication rejection affects `SecretAvailabilityState` and validation metadata;
- Provider Management may react to a secret revocation;
- Runtime cancellation may trigger lease release;
- Presentation may complete user presence interaction;
- none of these consumers may mutate Secret Management state directly.

---

## 3. State Ownership Matrix

| State machine | Owner | Notes |
|---|---|---|
| `SecretDescriptorState` | Secret Management | Logical secret identity lifecycle |
| `SecretRevisionState` | Secret Management | Immutable material revision lifecycle |
| `SecretAvailabilityState` | Secret Management | Normalized resolvability and usability summary |
| `SecretLeaseState` | Secret Management | Temporary access authority |
| `SecretBackendState` | Secret Management boundary | Normalized secure-store operational state |
| `SecretCandidateState` | Secret Management | Pre-activation material candidate |
| `SecretRotationState` | Secret Management | Rotation orchestration lifecycle |
| `SecretMigrationState` | Secret Management | Backend migration lifecycle |
| `SecretValidationState` | Secret Management | Validation execution lifecycle |
| `SecretRemovalState` | Secret Management | Logical removal and material deletion lifecycle |
| `SecretOperationState` | Secret Management | Administrative operation lifecycle |
| `SecretReconciliationState` | Secret Management | Resolution of uncertain external outcomes |
| `ProviderLeaseState` | Provider Management | Provider access lifecycle |
| `RuntimeWorkState` | Runtime | Scheduling and execution |
| `ConfigurationSnapshotState` | Configuration | Configuration publication lifecycle |
| `UserPresenceState` | Presentation / Platform | User interaction and platform prompt |

---

## 4. State Machine Separation

Secret Management must not use one global state enumeration.

Each entity has an independent lifecycle:

```text
SecretDescriptorState
SecretRevisionState
SecretAvailabilityState
SecretLeaseState
SecretBackendState
SecretCandidateState
SecretRotationState
SecretMigrationState
SecretValidationState
SecretRemovalState
SecretOperationState
SecretReconciliationState
```

This separation is required because:

- a descriptor may remain `ACTIVE` while its backend is `LOCKED`;
- a secret may be `AVAILABLE` while an old revision is `SUPERSEDED`;
- a descriptor may be `REVOKED` while historical metadata remains queryable;
- a rotation may be `VALIDATING` while the current revision remains active;
- a migration may fail while the source revision remains active;
- a lease may remain `ACTIVE` while the descriptor enters `ROTATING`;
- a backend may be `DEGRADED` without making every secret unavailable;
- a secret may be logically removed while physical deletion is still pending;
- a provider may reject one credential use without proving the secret is globally invalid;
- an operation may be uncertain while the underlying secret remains protected.

---

## 5. State Principles

### 5.1 State represents current lifecycle truth

```text
State
    = current accepted lifecycle condition

Event
    = immutable fact that a transition occurred
```

State and event meaning must not be conflated.

### 5.2 Transitions are explicit

Entities must not jump between unrelated states without a defined transition.

### 5.3 Secret identity is stable

`SecretId` remains stable across:

- material replacement;
- rotation;
- validation;
- backend migration;
- temporary unavailability;
- backend locking;
- expiration;
- revocation;
- logical removal;
- tombstone retention.

A new logical secret requires a new `SecretId`.

### 5.4 Revisions are immutable

Once a revision is committed, its material meaning never changes.

Replacement or rotation creates a new revision.

### 5.5 Availability is derived

`SecretAvailabilityState` summarizes whether an approved resolution may currently succeed.

It is derived from:

- descriptor state;
- active revision state;
- backend state;
- expiration;
- revocation;
- access policy;
- validation evidence;
- user presence;
- external provider feedback.

It is not the sole source of truth.

### 5.6 Lease state controls access authority

A lease grants bounded authority to use one revision for one consumer and purpose.

Descriptor availability alone does not grant access.

### 5.7 Logical state and physical execution may diverge

A lease may be logically revoked before:

- a remote request finishes;
- a child process releases copied material;
- a provider client is disposed;
- a backend handle is physically closed.

The logical state controls future authority.

### 5.8 Terminal does not mean erased

Terminal state preserves identity and safe metadata unless retention policy explicitly removes it.

### 5.9 Backend lock does not delete secrets

A locked backend makes resolution unavailable.

It does not remove descriptors or revisions.

### 5.10 Security transitions take precedence

Revocation, access denial, backend compromise, or policy violation may invalidate access immediately even when normal policy would allow graceful drain.

---

# Part I — Secret Descriptor State Machine

## 6. SecretDescriptorState

Canonical states:

```text
REGISTERING
ACTIVE
ROTATING
MIGRATING
SUSPENDED
REVOKED
REMOVING
REMOVED
TOMBSTONED
```

Primary flow:

```text
REGISTERING
    ↓
ACTIVE
    ↓
ROTATING / MIGRATING / SUSPENDED
    ↓
ACTIVE
    ↓
REVOKED
    ↓
REMOVING
    ↓
REMOVED
    ↓
TOMBSTONED
```

Alternative registration path:

```text
REGISTERING → REMOVED
```

when registration fails before activation and no active identity should remain.

---

## 7. REGISTERING

The logical secret identity is being created.

At this point:

- a `SecretId` may already be reserved;
- a desired reference is validated;
- access policy is validated;
- backend selection is in progress or complete;
- candidate material may exist;
- no active revision exists yet;
- normal resolution is prohibited;
- safe administrative inspection may show registration in progress.

Valid outgoing transitions:

```text
REGISTERING → ACTIVE
REGISTERING → REMOVING
REGISTERING → REMOVED
```

`REGISTERING → ACTIVE` requires:

- a valid descriptor;
- a valid access policy;
- an activated revision;
- a committed backend entry or approved non-persistent source;
- no identity conflict;
- no unresolved atomicity failure.

---

## 8. ACTIVE

The descriptor is valid for normal use subject to:

- availability;
- access policy;
- revision state;
- backend state;
- expiration;
- consumer authorization;
- purpose authorization.

`ACTIVE` does not guarantee every resolution succeeds.

Valid outgoing transitions:

```text
ACTIVE → ROTATING
ACTIVE → MIGRATING
ACTIVE → SUSPENDED
ACTIVE → REVOKED
ACTIVE → REMOVING
```

---

## 9. ROTATING

A replacement revision is being prepared.

Properties:

- the current active revision remains authoritative unless policy says otherwise;
- a candidate revision may exist;
- new leases may continue using the current revision;
- new leases may be temporarily frozen;
- current leases may drain;
- security rotation may revoke current leases;
- descriptor identity remains stable.

Valid outgoing transitions:

```text
ROTATING → ACTIVE
ROTATING → SUSPENDED
ROTATING → REVOKED
ROTATING → REMOVING
```

The descriptor returns to `ACTIVE` when:

- the new revision activates successfully; or
- rotation fails safely and the previous revision remains valid.

Rotation failure alone must not revoke a valid current revision unless evidence proves it unsafe.

---

## 10. MIGRATING

The active secret is being moved between backends or storage representations.

Properties:

- the source remains authoritative until destination validation and activation succeed;
- normal access may continue through the source;
- new lease admission may be frozen under strict policy;
- material must not be exposed to the migration caller;
- destination candidate is not active until commit.

Valid outgoing transitions:

```text
MIGRATING → ACTIVE
MIGRATING → SUSPENDED
MIGRATING → REVOKED
MIGRATING → REMOVING
```

A failed migration should return the descriptor to `ACTIVE` when the source remains valid.

---

## 11. SUSPENDED

The descriptor remains registered but normal resolution is temporarily prohibited.

Possible causes:

- administrative suspension;
- unresolved policy conflict;
- backend unavailable for an extended period;
- user presence required but unavailable;
- validation pending under strict policy;
- suspected compromise;
- migration or rotation reconciliation;
- application safe mode;
- compliance restriction.

Existing leases follow explicit policy:

```text
ALLOW_DRAIN
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE
```

Valid outgoing transitions:

```text
SUSPENDED → ACTIVE
SUSPENDED → ROTATING
SUSPENDED → MIGRATING
SUSPENDED → REVOKED
SUSPENDED → REMOVING
```

---

## 12. REVOKED

The logical secret must not be used for new access.

Properties:

- no new lease may be granted;
- active leases are revoked or drained only when policy explicitly permits;
- safe descriptor metadata remains queryable;
- historical revisions remain identifiable;
- provider-side revocation may be complete, pending, unsupported, or unknown;
- revocation is not deletion.

Valid outgoing transitions:

```text
REVOKED → ROTATING
REVOKED → REMOVING
REVOKED → ACTIVE
```

`REVOKED → ACTIVE` is permitted only through explicit administrative recovery with:

- a new valid revision or verified reinstatement;
- policy approval;
- concurrency protection;
- audit trace.

Silent un-revocation is prohibited.

---

## 13. REMOVING

Logical removal or material deletion is in progress.

Properties:

- no new leases;
- active leases follow removal policy;
- backend deletion may be pending;
- provider-side revocation may be pending;
- descriptor remains queryable for operation tracking;
- physical erasure assurance may be unknown until completion.

Valid outgoing transitions:

```text
REMOVING → REMOVED
REMOVING → REVOKED
REMOVING → SUSPENDED
```

Returning to `ACTIVE` is normally prohibited after material deletion begins.

Recovery should create a new revision or new descriptor rather than pretending removal never began.

---

## 14. REMOVED

The descriptor is no longer available for normal use.

Properties:

- no active revision;
- no new lease;
- safe historical metadata may remain;
- material deletion outcome is recorded;
- references resolve as removed or missing according to visibility policy;
- retention policy may later create a tombstone.

Valid outgoing transitions:

```text
REMOVED → TOMBSTONED
```

Reactivation should normally create a new descriptor or require a controlled restore operation that creates a new revision and audit trail.

---

## 15. TOMBSTONED

A minimal record remains to prevent identity reuse, support audit, or explain historical references.

A tombstone may retain:

```text
secretId
safe reference hash
removal time
last revision number
backend identifier
removal assurance
reason code
retention expiry
```

A tombstone must not retain secret material.

`TOMBSTONED` is terminal.

Hard deletion of the tombstone is a retention operation, not a normal lifecycle transition.

---

## 16. Descriptor Transition Table

| Current | Command or condition | Next | Notes |
|---|---|---|---|
| `REGISTERING` | registration committed | `ACTIVE` | Initial revision active |
| `REGISTERING` | registration aborted | `REMOVED` | No usable secret created |
| `ACTIVE` | rotate accepted | `ROTATING` | Current revision may remain active |
| `ACTIVE` | migrate accepted | `MIGRATING` | Source remains authoritative |
| `ACTIVE` | suspend | `SUSPENDED` | New lease denied |
| `ACTIVE` | revoke | `REVOKED` | Security authority removed |
| `ACTIVE` | remove | `REMOVING` | Deletion workflow begins |
| `ROTATING` | new revision activated | `ACTIVE` | Revision changes |
| `ROTATING` | failed safely | `ACTIVE` | Old revision retained |
| `ROTATING` | uncertain or unsafe | `SUSPENDED` | Reconciliation needed |
| `MIGRATING` | destination activated | `ACTIVE` | Backend binding changes |
| `MIGRATING` | failed safely | `ACTIVE` | Source retained |
| `MIGRATING` | source uncertain | `SUSPENDED` | Reconciliation needed |
| `SUSPENDED` | condition resolved | `ACTIVE` | Explicit transition |
| `SUSPENDED` | security revoke | `REVOKED` | No drain by default |
| `REVOKED` | replacement recovery | `ROTATING` | New revision path |
| `REVOKED` | remove | `REMOVING` | Cleanup path |
| `REMOVING` | deletion completed | `REMOVED` | Assurance recorded |
| `REMOVED` | retention compaction | `TOMBSTONED` | Minimal metadata only |

---

# Part II — Secret Revision State Machine

## 17. SecretRevisionState

Canonical states:

```text
CANDIDATE
VALIDATING
READY
ACTIVE
SUPERSEDED
EXPIRED
REVOKED
INVALID
DELETION_PENDING
DELETED
```

Primary lifecycle:

```text
CANDIDATE
    ↓
VALIDATING
    ↓
READY
    ↓
ACTIVE
    ↓
SUPERSEDED
    ↓
DELETION_PENDING
    ↓
DELETED
```

Alternative terminal paths:

```text
CANDIDATE → INVALID
VALIDATING → INVALID
READY → INVALID
ACTIVE → EXPIRED
ACTIVE → REVOKED
SUPERSEDED → REVOKED
EXPIRED → DELETION_PENDING
REVOKED → DELETION_PENDING
INVALID → DELETION_PENDING
```

---

## 18. CANDIDATE

Material exists in a pre-activation context.

It may be:

- held in memory;
- stored in a non-active backend slot;
- represented by a provider-generated pending credential;
- awaiting structural validation;
- awaiting user confirmation;
- awaiting policy approval.

A candidate:

- is not resolvable for normal consumers;
- is not the active revision;
- must have bounded lifetime;
- must be cleaned up on failure;
- must not appear in ordinary configuration or events.

Valid outgoing transitions:

```text
CANDIDATE → VALIDATING
CANDIDATE → READY
CANDIDATE → INVALID
CANDIDATE → DELETION_PENDING
```

Direct `CANDIDATE → READY` is allowed only when validation policy permits no additional check.

---

## 19. VALIDATING

The candidate is undergoing one or more checks:

- structural validation;
- backend access validation;
- cryptographic validation;
- expiration parsing;
- provider authentication;
- provider capability validation;
- compound-part completeness;
- policy compatibility.

Normal resolution remains prohibited.

Valid outgoing transitions:

```text
VALIDATING → READY
VALIDATING → INVALID
VALIDATING → CANDIDATE
VALIDATING → DELETION_PENDING
```

`VALIDATING → CANDIDATE` may occur when validation is deferred or requires user action.

---

## 20. READY

The revision has passed required checks and may be activated atomically.

Properties:

- material is stored or otherwise resolvable;
- descriptor binding is validated;
- access policy is compatible;
- it is still not authoritative;
- no normal lease may bind to it until activation.

Valid outgoing transitions:

```text
READY → ACTIVE
READY → INVALID
READY → DELETION_PENDING
```

---

## 21. ACTIVE

The revision is the descriptor's authoritative revision for new leases.

Invariants:

- only one revision is normally `ACTIVE` per descriptor;
- active revision identity is immutable;
- new leases record the revision;
- old leases do not silently change revision;
- the backend binding is committed;
- availability may still be non-available because of backend or policy state.

Valid outgoing transitions:

```text
ACTIVE → SUPERSEDED
ACTIVE → EXPIRED
ACTIVE → REVOKED
ACTIVE → INVALID
ACTIVE → DELETION_PENDING
```

`ACTIVE → INVALID` requires strong evidence such as corrupted material or failed integrity validation.

---

## 22. SUPERSEDED

A newer revision is active.

Properties:

- no new lease should normally use it;
- existing leases may continue under rotation policy;
- historical identity remains;
- backend material may be retained temporarily for drain or rollback;
- it is not automatically deleted.

Valid outgoing transitions:

```text
SUPERSEDED → EXPIRED
SUPERSEDED → REVOKED
SUPERSEDED → DELETION_PENDING
```

Returning to `ACTIVE` should occur only through an explicit rollback operation that creates a new revision whose content is based on the historical revision.

Revision numbers never move backward.

---

## 23. EXPIRED

The material is no longer valid because its validity period ended.

Properties:

- no new lease;
- active leases are revoked or fail on next use;
- renewable credentials may trigger refresh;
- expiration is not deletion;
- safe expiration metadata remains queryable.

Valid outgoing transitions:

```text
EXPIRED → DELETION_PENDING
```

A refresh creates a new revision.

It does not reactivate the expired revision.

---

## 24. REVOKED

Use of the revision is prohibited.

Possible causes:

- user revocation;
- provider revocation;
- suspected compromise;
- policy change;
- backend compromise;
- account disconnect;
- administrative action.

Properties:

- no new lease;
- active revision leases are revoked;
- historical metadata remains;
- provider-side status may be confirmed or uncertain.

Valid outgoing transitions:

```text
REVOKED → DELETION_PENDING
```

---

## 25. INVALID

The revision cannot be safely used.

Possible causes:

- malformed material;
- missing compound part;
- cryptographic failure;
- backend corruption;
- provider authentication failure under strict validation;
- unsupported encoding;
- identity mismatch;
- integrity failure.

An invalid revision is never activated.

If invalidity is discovered after activation:

- the descriptor becomes `SUSPENDED` or `REVOKED`;
- leases are revoked according to severity;
- replacement or rotation is required.

Valid outgoing transitions:

```text
INVALID → DELETION_PENDING
```

---

## 26. DELETION_PENDING

Material deletion has been requested but is not yet confirmed.

Possible reasons:

- backend operation pending;
- active lease drain;
- external provider revocation pending;
- backend offline;
- operating-system prompt required;
- secure-delete workflow deferred.

No lease may be granted.

Valid outgoing transitions:

```text
DELETION_PENDING → DELETED
DELETION_PENDING → REVOKED
```

`DELETION_PENDING → REVOKED` represents deletion failure while use remains prohibited.

---

## 27. DELETED

Secret material is no longer available through the managed backend.

The deletion record must include an assurance level such as:

```text
LOGICAL_ONLY
BACKEND_CONFIRMED
CRYPTOGRAPHIC_ERASURE
PHYSICAL_ERASURE_NOT_GUARANTEED
EXTERNAL_SOURCE_REMOVED
UNKNOWN
```

`DELETED` is terminal.

The state does not claim stronger erasure than the backend can prove.

---

## 28. Revision Activation Invariant

```text
At most one ACTIVE revision per SecretId.
```

Activation flow:

```text
Current revision R4 = ACTIVE
Candidate R5 = READY
        ↓ atomic commit
R5 = ACTIVE
R4 = SUPERSEDED
```

No observable state may expose both revisions as active for new leases.

Existing R4 leases may continue only under explicit policy.

---

# Part III — Secret Availability State

## 29. SecretAvailabilityState

Canonical states:

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

Availability is derived and may change without changing descriptor state.

---

## 30. UNKNOWN

The system lacks enough current evidence.

Possible causes:

- descriptor just loaded;
- backend not checked;
- validation never performed;
- external source status unknown;
- stale availability cache;
- application recovery in progress.

Policy should treat `UNKNOWN` conservatively.

---

## 31. AVAILABLE

The secret can be considered for an authorized resolution attempt.

Requirements normally include:

- descriptor `ACTIVE`;
- active revision exists;
- active revision not expired, revoked, invalid, or deleted;
- backend accessible;
- policy permits the consumer and purpose;
- no mandatory user action;
- no strict validation failure.

`AVAILABLE` does not guarantee provider authentication success.

---

## 32. UNAVAILABLE

The secret exists but cannot currently be resolved or safely used.

This is a generic normalized state when no more specific public state should be exposed.

Possible causes:

- temporary resolution failure;
- internal adapter failure;
- temporary policy evaluation issue;
- unresolved recovery.

---

## 33. MISSING

The reference does not map to an active descriptor or external source.

Visibility policy may return `UNKNOWN` or hidden existence instead of `MISSING` to unauthorized callers.

---

## 34. LOCKED

The relevant backend requires unlock or user presence.

Properties:

- descriptor and revision remain valid;
- no material is returned;
- retry may succeed after unlock;
- the backend state is normally `LOCKED`;
- this state is not a credential failure.

---

## 35. EXPIRED

The active revision expired.

A renewable credential may transition through refresh and create a new active revision.

---

## 36. REVOKED

The descriptor or active revision is revoked.

No new lease may be granted.

---

## 37. INVALID

The active revision failed validation or integrity requirements.

It may require replacement, rotation, or user re-entry.

---

## 38. BACKEND_UNAVAILABLE

The secure backend cannot currently serve requests.

Possible causes:

- operating-system service unavailable;
- external manager unreachable;
- encrypted store damaged;
- device profile unavailable;
- permission loss;
- adapter failure.

Descriptor state does not automatically change.

---

## 39. ACCESS_RESTRICTED

The secret exists and may be valid, but the requesting caller is not authorized.

This state may be returned only when policy allows revealing that the secret exists.

Otherwise the result should be hidden.

---

## 40. ROTATION_REQUIRED

Policy requires replacement before normal use.

Possible causes:

- age threshold;
- provider warning;
- suspected exposure;
- algorithm or certificate policy;
- administrative requirement;
- account lifecycle change.

Policy determines whether limited existing access may drain.

---

## 41. USER_ACTION_REQUIRED

Resolution requires user action such as:

- device unlock;
- biometric confirmation;
- system prompt;
- password re-entry;
- external login;
- credential re-entry.

Presentation owns the interaction.

Secret Management owns the resulting availability transition.

---

## 42. Availability Derivation

Conceptual priority:

```text
Descriptor removed or missing
    → MISSING

Descriptor or revision revoked
    → REVOKED

Revision expired
    → EXPIRED

Revision invalid
    → INVALID

Backend locked
    → LOCKED

Backend unavailable
    → BACKEND_UNAVAILABLE

User action required
    → USER_ACTION_REQUIRED

Rotation mandatory
    → ROTATION_REQUIRED

Caller denied
    → ACCESS_RESTRICTED or hidden

All mandatory conditions satisfied
    → AVAILABLE

Insufficient evidence
    → UNKNOWN

Other temporary failure
    → UNAVAILABLE
```

Security-specific states take precedence over generic `UNAVAILABLE`.

---

# Part IV — Secret Lease State Machine

## 43. SecretLeaseState

Canonical states:

```text
REQUESTED
EVALUATING
GRANTED
ACTIVE
RELEASING
RELEASED
EXPIRED
REVOKED
REJECTED
ABANDONED
```

Primary flow:

```text
REQUESTED
    ↓
EVALUATING
    ↓
GRANTED
    ↓
ACTIVE
    ↓
RELEASING
    ↓
RELEASED
```

Alternative terminal paths:

```text
REQUESTED → REJECTED
EVALUATING → REJECTED
GRANTED → EXPIRED
GRANTED → REVOKED
ACTIVE → EXPIRED
ACTIVE → REVOKED
ACTIVE → ABANDONED
RELEASING → ABANDONED
```

---

## 44. REQUESTED

A consumer requested bounded secret access.

At this point:

- request identity exists;
- no authority has been granted;
- no handle is usable;
- consumer, purpose, policy, backend, and revision checks are pending.

Valid outgoing transitions:

```text
REQUESTED → EVALUATING
REQUESTED → REJECTED
```

---

## 45. EVALUATING

Secret Management is evaluating:

- reference;
- descriptor;
- revision;
- consumer identity;
- access purpose;
- access policy;
- backend availability;
- lease capacity;
- requested duration;
- user presence requirements.

Valid outgoing transitions:

```text
EVALUATING → GRANTED
EVALUATING → REJECTED
```

Material resolution may happen only after policy allows it.

---

## 46. GRANTED

Authority was granted and a handle exists, but the consumer has not yet begun an approved operation.

Properties:

- lease lifetime has started;
- revision is fixed;
- consumer and purpose are fixed;
- the handle is non-transferable;
- normal use may transition to `ACTIVE`;
- cancellation before use should release promptly.

Valid outgoing transitions:

```text
GRANTED → ACTIVE
GRANTED → RELEASING
GRANTED → EXPIRED
GRANTED → REVOKED
```

---

## 47. ACTIVE

The lease is currently being used by an approved consumer operation.

Properties:

- the handle may perform only granted operations;
- every use validates lease authority;
- no revision rebinding;
- no consumer transfer;
- lease expiration and revocation remain enforceable;
- remote protocols may already have received derived authentication data.

Valid outgoing transitions:

```text
ACTIVE → RELEASING
ACTIVE → EXPIRED
ACTIVE → REVOKED
ACTIVE → ABANDONED
```

---

## 48. RELEASING

Logical release was requested and physical cleanup is in progress.

Cleanup may include:

- closing backend handle;
- clearing temporary buffers;
- detaching provider client;
- invalidating child-process transfer;
- decrementing lease counters;
- removing internal cache entry.

Valid outgoing transitions:

```text
RELEASING → RELEASED
RELEASING → ABANDONED
```

No new use is permitted.

---

## 49. RELEASED

The consumer completed normal release.

Properties:

- handle is unusable;
- temporary material is cleared where practical;
- lease is terminal;
- repeated release is idempotent.

`RELEASED` is terminal.

---

## 50. EXPIRED

The lease lifetime ended.

Properties:

- new handle operations are rejected;
- physical cleanup begins;
- an already accepted external request may continue;
- renewal must occur before terminal expiration when policy supports it.

`EXPIRED` is terminal for the same lease.

A new lease is required.

---

## 51. REVOKED

Authority was explicitly removed before normal release.

Possible causes:

- descriptor or revision revocation;
- security policy change;
- consumer mismatch;
- purpose violation;
- backend compromise;
- application shutdown;
- provider credential rotation with immediate revocation;
- administrative action.

No future handle operation is valid.

`REVOKED` is terminal.

---

## 52. REJECTED

The request never received authority.

Possible causes:

- secret unavailable;
- access denied;
- invalid consumer;
- invalid purpose;
- revision mismatch;
- lease capacity exceeded;
- backend locked;
- user action unavailable;
- descriptor revoked or removed.

No material may have crossed the approved boundary.

`REJECTED` is terminal.

---

## 53. ABANDONED

Secret Management stopped waiting for complete physical cleanup or acknowledgement.

Examples:

- child process disappeared;
- provider SDK did not close;
- backend handle close timed out;
- external process transfer acknowledgement was lost;
- application shutdown exceeded deadline.

Logical authority is already removed.

`ABANDONED` must not permit reuse.

Late cleanup may still occur.

---

## 54. Lease Terminal States

```text
RELEASED
EXPIRED
REVOKED
REJECTED
ABANDONED
```

A terminal lease:

- cannot become active again;
- cannot change revision;
- cannot change consumer;
- cannot change purpose;
- cannot be renewed;
- may remain queryable as safe metadata.

---

## 55. Rotation and Active Leases

Possible policies:

```text
ALLOW_TO_EXPIRE
REVOKE_IMMEDIATELY
REVOKE_AFTER_GRACE
BACKEND_DEFINED
```

### ALLOW_TO_EXPIRE

```text
R4 lease ACTIVE
R5 becomes ACTIVE revision
R4 becomes SUPERSEDED
R4 lease continues
R4 lease later RELEASED or EXPIRED
```

### REVOKE_IMMEDIATELY

```text
R5 activation
    ↓
All R4 leases → REVOKED
```

Use for:

- compromise;
- explicit revocation;
- critical policy change;
- invalid old material.

### REVOKE_AFTER_GRACE

```text
R5 activation
    ↓
R4 leases remain temporarily ACTIVE
    ↓ grace deadline
Remaining R4 leases → REVOKED
```

No new R4 lease may be granted after R5 activation.

---

## 56. Removal and Active Leases

Removal policy must be explicit.

Default secure flow:

```text
Descriptor → REMOVING
    ↓
No new leases
    ↓
Revoke or drain active leases
    ↓
Delete material
    ↓
Descriptor → REMOVED
```

Material must not be deleted while a backend requires it for an authorized draining lease unless policy explicitly prioritizes immediate security revocation.

---

# Part V — Secret Backend State Machine

## 57. SecretBackendState

Canonical states:

```text
UNREGISTERED
REGISTERED
INITIALIZING
AVAILABLE
LOCKED
DEGRADED
UNAVAILABLE
MIGRATING
COMPROMISED
SHUTTING_DOWN
TERMINATED
```

Primary flow:

```text
UNREGISTERED
    ↓
REGISTERED
    ↓
INITIALIZING
    ↓
AVAILABLE
    ↓
SHUTTING_DOWN
    ↓
TERMINATED
```

Operational alternatives:

```text
AVAILABLE ↔ LOCKED
AVAILABLE ↔ DEGRADED
AVAILABLE ↔ UNAVAILABLE
AVAILABLE → MIGRATING → AVAILABLE
ANY ACTIVE STATE → COMPROMISED
```

---

## 58. UNREGISTERED

The backend is unknown to the application.

No operation is permitted.

---

## 59. REGISTERED

The backend adapter and capability metadata are registered.

Initialization has not completed.

Valid outgoing transitions:

```text
REGISTERED → INITIALIZING
REGISTERED → TERMINATED
```

---

## 60. INITIALIZING

The backend is checking:

- platform availability;
- permissions;
- account context;
- store existence;
- schema or metadata compatibility;
- encryption prerequisites;
- lock state;
- migration requirements.

Valid outgoing transitions:

```text
INITIALIZING → AVAILABLE
INITIALIZING → LOCKED
INITIALIZING → DEGRADED
INITIALIZING → UNAVAILABLE
INITIALIZING → COMPROMISED
```

---

## 61. AVAILABLE

The backend may accept supported operations.

Availability remains subject to:

- per-secret access policy;
- user presence;
- backend capabilities;
- value size;
- secret kind;
- scope;
- current permissions.

Valid outgoing transitions:

```text
AVAILABLE → LOCKED
AVAILABLE → DEGRADED
AVAILABLE → UNAVAILABLE
AVAILABLE → MIGRATING
AVAILABLE → COMPROMISED
AVAILABLE → SHUTTING_DOWN
```

---

## 62. LOCKED

The backend exists but requires unlock or user presence.

Properties:

- descriptors may remain queryable from safe metadata cache;
- material resolution is prohibited;
- store or delete may also be prohibited;
- unlock attempts must be bounded;
- failed unlock must not reveal secret existence beyond policy.

Valid outgoing transitions:

```text
LOCKED → AVAILABLE
LOCKED → UNAVAILABLE
LOCKED → COMPROMISED
LOCKED → SHUTTING_DOWN
```

---

## 63. DEGRADED

The backend remains partially usable.

Possible causes:

- slow response;
- partial capability loss;
- enumeration unavailable;
- secure delete unavailable;
- metadata stale;
- external service intermittent;
- backup unavailable;
- lock-state polling failure.

Policy determines which operations remain allowed.

Valid outgoing transitions:

```text
DEGRADED → AVAILABLE
DEGRADED → UNAVAILABLE
DEGRADED → LOCKED
DEGRADED → COMPROMISED
DEGRADED → SHUTTING_DOWN
```

---

## 64. UNAVAILABLE

The backend cannot currently perform required operations.

Possible causes:

- service not running;
- network unreachable;
- permission denied;
- platform unsupported;
- device profile missing;
- adapter initialization failure;
- external manager outage.

Descriptors remain logically registered.

Valid outgoing transitions:

```text
UNAVAILABLE → INITIALIZING
UNAVAILABLE → AVAILABLE
UNAVAILABLE → LOCKED
UNAVAILABLE → COMPROMISED
UNAVAILABLE → SHUTTING_DOWN
```

---

## 65. MIGRATING

Backend-level metadata or storage format is being migrated.

Properties:

- normal writes may be frozen;
- reads may continue under policy;
- per-secret migration states remain independent;
- migration must preserve at least one valid copy;
- failures may return backend to `DEGRADED` or `UNAVAILABLE`.

Valid outgoing transitions:

```text
MIGRATING → AVAILABLE
MIGRATING → DEGRADED
MIGRATING → UNAVAILABLE
MIGRATING → COMPROMISED
```

---

## 66. COMPROMISED

The backend is suspected or confirmed unsafe.

Immediate effects:

- no new resolution;
- active leases are revoked under security policy;
- affected descriptors become `SUSPENDED` or `REVOKED`;
- migration or rotation may be required;
- diagnostics are restricted;
- raw failure details remain protected.

Valid outgoing transitions:

```text
COMPROMISED → SHUTTING_DOWN
COMPROMISED → TERMINATED
COMPROMISED → INITIALIZING
```

Recovery through `INITIALIZING` requires explicit remediation and policy approval.

---

## 67. SHUTTING_DOWN

The backend is draining and closing.

Properties:

- no new operations;
- active leases are released or revoked;
- internal caches are cleared;
- pending writes are finalized or failed safely;
- handles close within bounded time.

Valid outgoing transitions:

```text
SHUTTING_DOWN → TERMINATED
```

---

## 68. TERMINATED

The backend adapter is no longer operational in the application instance.

`TERMINATED` is terminal for that backend instance.

A new application instance may register a new backend instance with the same logical backend identity.

---

# Part VI — Secret Candidate State Machine

## 69. SecretCandidateState

Canonical states:

```text
CREATED
MATERIAL_RECEIVED
STORED
VALIDATING
READY
ACTIVATING
ACTIVATED
REJECTED
CLEANUP_PENDING
CLEANED
```

Flow:

```text
CREATED
    ↓
MATERIAL_RECEIVED
    ↓
STORED
    ↓
VALIDATING
    ↓
READY
    ↓
ACTIVATING
    ↓
ACTIVATED
```

Failure path:

```text
ANY NON-TERMINAL
    ↓
REJECTED
    ↓
CLEANUP_PENDING
    ↓
CLEANED
```

---

## 70. Candidate Atomicity

A candidate must never become visible as active before:

- material is durably available where required;
- descriptor binding is valid;
- access policy is committed;
- required validation succeeds;
- revision number is reserved safely;
- activation transaction succeeds.

A failed candidate must not replace the last known good revision.

---

# Part VII — Secret Rotation State Machine

## 71. SecretRotationState

Canonical states:

```text
REQUESTED
PREPARING
GENERATING
STORING_CANDIDATE
VALIDATING
READY_TO_ACTIVATE
ACTIVATING
APPLYING_LEASE_POLICY
COMPLETED
FAILED
CANCELED
UNCERTAIN
RECONCILING
```

Primary flow:

```text
REQUESTED
    ↓
PREPARING
    ↓
GENERATING
    ↓
STORING_CANDIDATE
    ↓
VALIDATING
    ↓
READY_TO_ACTIVATE
    ↓
ACTIVATING
    ↓
APPLYING_LEASE_POLICY
    ↓
COMPLETED
```

---

## 72. REQUESTED

Rotation command accepted for evaluation.

No candidate exists yet.

Valid outgoing transitions:

```text
REQUESTED → PREPARING
REQUESTED → FAILED
REQUESTED → CANCELED
```

---

## 73. PREPARING

Secret Management validates:

- descriptor state;
- expected revision;
- rotation policy;
- backend capabilities;
- provider coordination requirements;
- user presence;
- existing lease policy;
- idempotency;
- concurrency.

Valid outgoing transitions:

```text
PREPARING → GENERATING
PREPARING → STORING_CANDIDATE
PREPARING → FAILED
PREPARING → CANCELED
```

`STORING_CANDIDATE` is used when material was supplied by the user.

---

## 74. GENERATING

New material is generated through:

- backend generation;
- provider API;
- refresh-token flow;
- certificate reissue;
- key pair generation;
- external identity flow.

Valid outgoing transitions:

```text
GENERATING → STORING_CANDIDATE
GENERATING → FAILED
GENERATING → UNCERTAIN
GENERATING → CANCELED
```

A lost provider acknowledgement may create `UNCERTAIN`.

---

## 75. STORING_CANDIDATE

The candidate revision is stored without replacing the active revision.

Valid outgoing transitions:

```text
STORING_CANDIDATE → VALIDATING
STORING_CANDIDATE → FAILED
STORING_CANDIDATE → UNCERTAIN
```

---

## 76. VALIDATING

Required candidate validation runs.

Valid outgoing transitions:

```text
VALIDATING → READY_TO_ACTIVATE
VALIDATING → FAILED
VALIDATING → CANCELED
VALIDATING → UNCERTAIN
```

---

## 77. READY_TO_ACTIVATE

The candidate passed validation.

The active revision remains unchanged.

Valid outgoing transitions:

```text
READY_TO_ACTIVATE → ACTIVATING
READY_TO_ACTIVATE → CANCELED
READY_TO_ACTIVATE → FAILED
```

---

## 78. ACTIVATING

Secret Management atomically:

- marks the candidate revision active;
- marks the old revision superseded;
- updates descriptor binding;
- updates backend reference if required;
- commits safe metadata.

Valid outgoing transitions:

```text
ACTIVATING → APPLYING_LEASE_POLICY
ACTIVATING → FAILED
ACTIVATING → UNCERTAIN
```

If activation outcome is uncertain, no blind retry is permitted until reconciliation.

---

## 79. APPLYING_LEASE_POLICY

The selected policy is applied to old-revision leases.

Possible actions:

- allow drain;
- schedule grace revocation;
- revoke immediately;
- rebuild provider clients;
- invalidate internal caches.

Valid outgoing transitions:

```text
APPLYING_LEASE_POLICY → COMPLETED
APPLYING_LEASE_POLICY → FAILED
APPLYING_LEASE_POLICY → UNCERTAIN
```

The new revision may already be active even if cleanup partially fails.

The result may be `PARTIALLY_COMPLETED` at the operation level.

---

## 80. COMPLETED

Rotation completed with a known active revision and known lease-policy result.

`COMPLETED` is terminal.

---

## 81. FAILED

Rotation did not activate a new revision.

The previous active revision remains authoritative unless independently unsafe.

Candidate cleanup must occur.

`FAILED` is terminal.

---

## 82. CANCELED

Rotation stopped before an irreversible external or activation step completed.

Cancellation is terminal only when the final outcome is known.

If provider-side generation may have completed, use `UNCERTAIN`, not `CANCELED`.

---

## 83. UNCERTAIN

The system cannot determine whether an external or backend action completed.

Examples:

- provider rotated key but response was lost;
- destination backend write timed out after commit;
- activation transaction acknowledgement was lost;
- child authentication broker disappeared.

Valid outgoing transitions:

```text
UNCERTAIN → RECONCILING
```

No automatic non-idempotent retry is allowed.

---

## 84. RECONCILING

Secret Management compares:

- provider credential status;
- backend revision;
- active descriptor revision;
- candidate existence;
- validation evidence;
- active lease bindings;
- idempotency records.

Valid outgoing transitions:

```text
RECONCILING → COMPLETED
RECONCILING → FAILED
RECONCILING → UNCERTAIN
```

Repeated unresolved reconciliation may suspend the descriptor.

---

# Part VIII — Secret Migration State Machine

## 85. SecretMigrationState

Canonical states:

```text
REQUESTED
VALIDATING_SOURCE
PREPARING_DESTINATION
COPYING
VALIDATING_DESTINATION
READY_TO_SWITCH
SWITCHING
DRAINING_SOURCE
CLEANING_SOURCE
COMPLETED
FAILED
CANCELED
UNCERTAIN
RECONCILING
```

Primary flow:

```text
REQUESTED
    ↓
VALIDATING_SOURCE
    ↓
PREPARING_DESTINATION
    ↓
COPYING
    ↓
VALIDATING_DESTINATION
    ↓
READY_TO_SWITCH
    ↓
SWITCHING
    ↓
DRAINING_SOURCE
    ↓
CLEANING_SOURCE
    ↓
COMPLETED
```

---

## 86. Migration Safety Invariant

```text
At least one valid resolvable copy remains
until destination activation is confirmed.
```

The source must not be deleted before:

- destination material is stored;
- destination validation succeeds;
- descriptor binding switches atomically;
- new resolution uses the destination;
- existing source-bound leases are handled.

---

## 87. VALIDATING_SOURCE

Checks:

- source descriptor and revision;
- source backend access;
- expected revision;
- export or internal transfer capability;
- migration policy;
- active lease state.

Failure leaves the source active.

---

## 88. PREPARING_DESTINATION

Checks:

- destination backend available;
- capability compatibility;
- scope support;
- secret kind support;
- size limits;
- user presence;
- atomic replace capability;
- policy compatibility.

---

## 89. COPYING

Material transfers internally between approved backends.

The caller never receives material.

Failure must preserve the source.

---

## 90. VALIDATING_DESTINATION

The destination candidate is checked before activation.

A failed destination is cleaned up.

The source remains active.

---

## 91. READY_TO_SWITCH

Destination is valid but not yet authoritative.

Valid outgoing transitions:

```text
READY_TO_SWITCH → SWITCHING
READY_TO_SWITCH → CANCELED
READY_TO_SWITCH → FAILED
```

---

## 92. SWITCHING

Descriptor and active revision backend binding switch atomically.

Possible result:

- new leases use destination;
- old source-bound leases drain;
- source remains until cleanup policy permits deletion.

---

## 93. DRAINING_SOURCE

No new source-bound leases are granted.

Existing leases:

- drain;
- revoke immediately; or
- revoke after grace.

---

## 94. CLEANING_SOURCE

The source copy is deleted according to requested assurance.

Failure may yield a partially completed migration:

```text
Destination active
Source cleanup failed
```

This must not roll the descriptor back automatically.

---

## 95. Migration Terminal and Uncertain States

`COMPLETED`, `FAILED`, and `CANCELED` follow the same certainty rules as rotation.

`UNCERTAIN` requires `RECONCILING`.

Reconciliation determines:

- active backend;
- destination content;
- source content;
- descriptor binding;
- lease bindings;
- cleanup status.

---

# Part IX — Secret Validation State Machine

## 96. SecretValidationState

Canonical states:

```text
REQUESTED
CHECKING_REFERENCE
CHECKING_BACKEND
CHECKING_STRUCTURE
CHECKING_PROVIDER
VALID
INVALID
UNKNOWN
DEFERRED
CANCELED
FAILED
```

The exact steps depend on validation mode.

---

## 97. Validation Semantics

Validation state describes one validation operation.

It does not replace descriptor, revision, or availability state.

Examples:

- a provider check may fail because of network outage and result in `UNKNOWN`;
- a structural check may result in `INVALID`;
- a canceled check must not mark the revision invalid;
- a previous `VALID` result may become stale.

---

## 98. VALID

Required checks succeeded at `checkedAt`.

`VALID` is evidence, not a permanent guarantee.

The summary records:

- revision;
- validation mode;
- provider;
- expiration metadata;
- safe evidence;
- validation timestamp.

---

## 99. INVALID

Evidence proves the revision is unusable for the checked requirement.

Effects depend on severity:

```text
Candidate invalid
    → candidate rejected

Active revision invalid
    → descriptor SUSPENDED or REVOKED
    → active leases revoked when necessary
```

---

## 100. UNKNOWN

The system cannot determine validity.

Possible causes:

- network unavailable;
- provider unavailable;
- backend unavailable;
- unsupported validation;
- inconclusive response;
- stale evidence.

`UNKNOWN` is not equivalent to `INVALID`.

---

## 101. DEFERRED

Validation requires:

- user action;
- scheduled provider call;
- network permission;
- cost approval;
- rate-limit recovery;
- external process.

The secret may remain available only when policy permits.

---

## 102. CANCELED and FAILED

`CANCELED` means the operation was stopped intentionally.

`FAILED` means the validation infrastructure failed unexpectedly.

Neither state alone proves secret invalidity.

---

# Part X — Secret Removal State Machine

## 103. SecretRemovalState

Canonical states:

```text
REQUESTED
BLOCKING_NEW_ACCESS
DRAINING_LEASES
REVOKING_EXTERNAL
DELETING_MATERIAL
VERIFYING_DELETION
RETAINING_TOMBSTONE
COMPLETED
PARTIALLY_COMPLETED
FAILED
UNCERTAIN
RECONCILING
```

---

## 104. Removal Flow

```text
REQUESTED
    ↓
BLOCKING_NEW_ACCESS
    ↓
DRAINING_LEASES
    ↓
REVOKING_EXTERNAL
    ↓
DELETING_MATERIAL
    ↓
VERIFYING_DELETION
    ↓
RETAINING_TOMBSTONE
    ↓
COMPLETED
```

Optional steps may be skipped when unsupported or unnecessary.

---

## 105. BLOCKING_NEW_ACCESS

Descriptor enters `REMOVING`.

No new lease may be granted.

This transition must occur before material deletion.

---

## 106. DRAINING_LEASES

Lease policy is applied.

Security removal should normally revoke immediately.

Routine cleanup may allow bounded drain.

---

## 107. REVOKING_EXTERNAL

When requested and supported, Secret Management coordinates provider-side revocation.

Outcomes:

```text
CONFIRMED
NOT_SUPPORTED
FAILED
UNCERTAIN
NOT_REQUESTED
```

External revocation is separate from local deletion.

---

## 108. DELETING_MATERIAL

The backend deletion operation executes.

No public result may claim physical erasure beyond backend guarantees.

---

## 109. VERIFYING_DELETION

Verification may include:

- backend entry absent;
- active reference no longer resolves;
- encrypted blob key destroyed;
- external source missing;
- safe metadata updated.

Verification must not re-expose material.

---

## 110. PARTIALLY_COMPLETED

Examples:

- local deletion succeeded but provider revocation failed;
- destination active but source cleanup failed;
- material removed but tombstone persistence failed;
- one compound part removed while another external part is uncertain.

The operation requires explicit recovery guidance.

The descriptor remains `REMOVED`, `REVOKED`, or `SUSPENDED` according to the safe outcome.

---

# Part XI — Secret Operation and Reconciliation

## 111. SecretOperationState

Canonical states:

```text
ACCEPTED
RUNNING
WAITING_FOR_USER
WAITING_FOR_EXTERNAL
DEFERRED
COMPLETED
PARTIALLY_COMPLETED
REJECTED
FAILED
CANCELED
UNCERTAIN
```

This is a general administrative operation state.

It does not replace entity-specific state machines.

---

## 112. WAITING_FOR_USER

The operation needs:

- device unlock;
- biometric confirmation;
- system credential prompt;
- credential re-entry;
- application confirmation;
- external authentication.

Presentation displays the prompt.

Secret Management resumes only from a trusted result.

---

## 113. WAITING_FOR_EXTERNAL

The operation is waiting for:

- provider API;
- operating-system service;
- external secret manager;
- child process;
- identity provider;
- remote validation.

Timeout may lead to `FAILED` or `UNCERTAIN` depending on whether the external action may have committed.

---

## 114. SecretReconciliationState

Canonical states:

```text
NOT_REQUIRED
REQUIRED
INSPECTING
RESOLVED_SUCCESS
RESOLVED_FAILURE
UNRESOLVED
MANUAL_ACTION_REQUIRED
```

Reconciliation is required when repeating an operation could:

- create another provider credential;
- revoke the wrong credential;
- overwrite a newer revision;
- lose the only valid copy;
- delete the wrong backend entry;
- expose inconsistent lease authority.

---

## 115. Reconciliation Invariant

```text
Do not retry a potentially committed non-idempotent operation
until its actual outcome is known or safely isolated.
```

---

# Part XII — Cross-State Rules

## 116. Descriptor and Revision Relationship

| Descriptor | Allowed active revision condition |
|---|---|
| `REGISTERING` | No active revision |
| `ACTIVE` | Exactly one active revision |
| `ROTATING` | One current active revision plus optional candidate |
| `MIGRATING` | One active revision, source authoritative until switch |
| `SUSPENDED` | Active revision may exist but no new lease |
| `REVOKED` | No usable active revision |
| `REMOVING` | No new use; revisions deletion-pending or revoked |
| `REMOVED` | No active revision |
| `TOMBSTONED` | No material revision retained |

---

## 117. Descriptor and Availability Relationship

Examples:

```text
Descriptor ACTIVE + Backend AVAILABLE + Revision ACTIVE
    → Availability may be AVAILABLE

Descriptor ACTIVE + Backend LOCKED
    → Availability LOCKED

Descriptor ACTIVE + Revision EXPIRED
    → Availability EXPIRED

Descriptor SUSPENDED
    → Availability UNAVAILABLE or USER_ACTION_REQUIRED

Descriptor REVOKED
    → Availability REVOKED

Descriptor REMOVED
    → Availability MISSING
```

---

## 118. Backend and Lease Relationship

When backend becomes:

### LOCKED

- existing operation-oriented handles may continue only if they do not require another backend read;
- new resolution is denied;
- policy may revoke handles that depend on backend presence.

### UNAVAILABLE

- new resolution denied;
- active leases may continue if material is already safely bound;
- failures must not expose backend internals.

### COMPROMISED

- new resolution denied;
- active leases revoked;
- affected descriptors suspended or revoked;
- rotation or migration required.

### SHUTTING_DOWN

- no new lease;
- active leases drain or revoke;
- cleanup bounded.

---

## 119. Provider Management Interaction

Secret Management exposes normalized availability.

Provider Management may derive:

```text
CredentialAvailabilityState
```

from Secret Management state.

Mapping example:

| Secret availability | Provider credential state |
|---|---|
| `AVAILABLE` | `AVAILABLE` |
| `LOCKED` | `CREDENTIAL_UNAVAILABLE` or user action required |
| `EXPIRED` | `CREDENTIAL_UNAVAILABLE` |
| `REVOKED` | `CREDENTIAL_UNAVAILABLE` |
| `INVALID` | `CREDENTIAL_UNAVAILABLE` |
| `BACKEND_UNAVAILABLE` | `CREDENTIAL_UNAVAILABLE` |
| `UNKNOWN` | `UNKNOWN` |

Provider Management must not mutate secret state.

Authentication feedback is submitted through a command.

---

## 120. Runtime Cancellation Interaction

Runtime cancellation may request lease release.

```text
Runtime Attempt canceled
    ↓
Provider operation canceled where supported
    ↓
Secret lease → RELEASING
    ↓
Secret lease → RELEASED
```

If physical cleanup cannot be confirmed:

```text
Secret lease → ABANDONED
```

Cancellation does not revoke the descriptor or revision.

---

## 121. Application Shutdown Interaction

Recommended order:

```text
Stop new work
    ↓
Stop new secret leases
    ↓
Release or revoke active leases
    ↓
Cancel candidate operations
    ↓
Finish or mark uncertain rotations/migrations
    ↓
Clear sensitive memory caches
    ↓
Shutdown backends
    ↓
Backend TERMINATED
```

Shutdown must not silently mark uncertain external rotation as failed.

---

# Part XIII — Invalid Transitions

## 122. Invalid Descriptor Transitions

Examples:

```text
REMOVED → ACTIVE
TOMBSTONED → ACTIVE
REVOKED → ACTIVE without explicit recovery
REGISTERING → ROTATING
REMOVING → ROTATING
```

These require a new controlled operation or new identity.

---

## 123. Invalid Revision Transitions

Examples:

```text
SUPERSEDED → ACTIVE directly
EXPIRED → ACTIVE
REVOKED → ACTIVE
DELETED → ACTIVE
INVALID → ACTIVE
ACTIVE R4 material mutated in place
```

Rollback creates a new revision.

---

## 124. Invalid Lease Transitions

Examples:

```text
RELEASED → ACTIVE
EXPIRED → ACTIVE
REVOKED → ACTIVE
REJECTED → GRANTED
ABANDONED → ACTIVE
GRANTED revision changed
ACTIVE consumer changed
ACTIVE purpose changed
```

A new lease is required.

---

## 125. Invalid Backend Transitions

Examples:

```text
TERMINATED → AVAILABLE
COMPROMISED → AVAILABLE without remediation
UNREGISTERED → AVAILABLE
LOCKED → MIGRATING without unlock when material access is required
```

---

## 126. Invalid Rotation and Migration Transitions

Examples:

```text
FAILED → ACTIVATING
CANCELED → COMPLETED
UNCERTAIN → automatic retry
COPYING → source deletion
VALIDATING_DESTINATION → source cleanup
READY_TO_SWITCH → source deleted before switch
```

---

# Part XIV — Concurrency and Persistence

## 127. Single Logical Writer

Secret Management is the single logical writer for:

- descriptor state;
- revision state;
- lease state;
- rotation state;
- migration state;
- removal state;
- normalized availability;
- backend state snapshots.

Backend adapters report facts.

Consumers do not write lifecycle state directly.

---

## 128. Optimistic Concurrency

Mutating operations should validate:

```text
expectedDescriptorVersion
expectedCurrentRevision
expectedBackendBindingRevision
expectedLeaseVersion
```

A stale operation must not overwrite newer state.

---

## 129. State Transition Record

Every accepted transition should record safe metadata:

```text
entityType
entityId
fromState
toState
reasonCode
operationId
actorId
correlationId
causationId?
occurredAt
stateVersion
```

No record may contain secret material.

---

## 130. Persistence Ordering

For durable state:

```text
Validate transition
    ↓
Persist material candidate if needed
    ↓
Persist new lifecycle state atomically
    ↓
Commit
    ↓
Publish safe event
```

Events must not be published before the authoritative state commit.

If event publication fails, state remains authoritative and publication may be retried according to Event Bus policy.

---

## 131. Crash Recovery

On startup, Secret Management must detect incomplete states such as:

```text
REGISTERING
ROTATING
MIGRATING
REMOVING
ACTIVATING
SWITCHING
DELETION_PENDING
UNCERTAIN
RELEASING
```

Recovery rules:

- never assume an external operation failed merely because the process crashed;
- never activate an unvalidated candidate;
- preserve the last known good revision;
- reconcile uncertain provider or backend outcomes;
- expire orphaned leases;
- clear memory-only material;
- mark unavailable references appropriately;
- complete safe idempotent cleanup.

---

## 132. Orphaned Lease Recovery

After restart:

```text
Persistent lease metadata found
    ↓
No live consumer identity
    ↓
Lease → EXPIRED or ABANDONED
    ↓
Cleanup attempted
```

Secret material handles themselves must not be restored from ordinary persistence.

A new resolution is required.

---

## 133. Memory Backend Recovery

Memory-backed secrets do not survive process termination.

After restart:

- descriptor may become `SUSPENDED`, `REMOVED`, or remain configuration-only depending on policy;
- availability becomes `MISSING` or `UNAVAILABLE`;
- active memory revision cannot be reconstructed;
- the user or provider flow must supply a new revision.

---

# Part XV — Command-to-State Mapping

## 134. RegisterSecret

```text
Descriptor: none → REGISTERING → ACTIVE
Candidate: CREATED → ... → ACTIVATED
Revision: CANDIDATE → READY → ACTIVE
Operation: ACCEPTED → RUNNING → COMPLETED
```

Failure:

```text
Candidate → REJECTED → CLEANED
Descriptor → REMOVED or no persistent record
Operation → FAILED / REJECTED
```

---

## 135. ReplaceSecret

```text
Descriptor ACTIVE → ROTATING
Rotation REQUESTED → ... → COMPLETED
New revision CANDIDATE → ACTIVE
Old revision ACTIVE → SUPERSEDED
Descriptor ROTATING → ACTIVE
```

---

## 136. RotateSecret

Same state shape as replacement, with optional provider generation and reconciliation.

---

## 137. RevokeSecret

```text
Descriptor ACTIVE / SUSPENDED → REVOKED
Active revision ACTIVE → REVOKED
Active leases → REVOKED
Availability → REVOKED
```

---

## 138. RemoveSecret

```text
Descriptor → REMOVING
Removal → REQUESTED ... COMPLETED
Revisions → DELETION_PENDING → DELETED
Descriptor → REMOVED → TOMBSTONED
Availability → MISSING
```

---

## 139. ResolveSecret / AcquireLease

```text
Lease REQUESTED
    ↓ EVALUATING
    ↓ GRANTED
    ↓ ACTIVE
    ↓ RELEASING
    ↓ RELEASED
```

No descriptor state change is required for normal resolution.

Safe usage metadata may update.

---

## 140. ValidateSecret

```text
Validation REQUESTED
    ↓ checking stages
    ↓ VALID / INVALID / UNKNOWN / DEFERRED
```

Only authoritative evidence may affect revision or availability state.

---

## 141. MigrateSecret

```text
Descriptor ACTIVE → MIGRATING
Migration REQUESTED → ... → COMPLETED
Revision backend binding switches
Descriptor MIGRATING → ACTIVE
```

Failure before switch returns to `ACTIVE`.

Uncertain switch leads to `SUSPENDED` plus reconciliation.

---

# Part XVI — Event-to-State Mapping

## 142. Event Principle

Events report accepted facts after state transition.

Examples:

```text
SecretRegistered
SecretDescriptorActivated
SecretRevisionActivated
SecretRotationStarted
SecretRotationCompleted
SecretRevoked
SecretRemovalStarted
SecretRemoved
SecretAvailabilityChanged
SecretLeaseGranted
SecretLeaseActivated
SecretLeaseReleased
SecretLeaseExpired
SecretLeaseRevoked
SecretBackendLocked
SecretBackendAvailable
SecretBackendCompromised
SecretMigrationCompleted
SecretValidationCompleted
SecretOperationBecameUncertain
SecretReconciliationRequired
```

Detailed event contracts belong in `EVENTS.md`.

---

## 143. State Transition Before Event

Correct:

```text
Validate
    ↓
Persist state transition
    ↓
Commit
    ↓
Publish event
```

Incorrect:

```text
Publish event
    ↓
Attempt state transition
```

---

# Part XVII — Security Invariants

## 144. No Material in State Records

Lifecycle state records may contain:

- IDs;
- revisions;
- safe references;
- timestamps;
- reason codes;
- backend IDs;
- policy IDs;
- operation IDs;
- safe status metadata.

They must never contain:

- raw secret;
- authorization header;
- password;
- access token;
- refresh token;
- private key;
- decrypted compound credential;
- backend encryption key;
- secret handle;
- temporary plaintext buffer.

---

## 145. Lease Authority Invariant

A handle is usable only when:

```text
Lease state = GRANTED or ACTIVE
AND
consumer identity matches
AND
purpose matches
AND
revision remains permitted
AND
lease not expired
AND
lease not revoked
```

---

## 146. Rotation Authority Invariant

After a new revision activates:

```text
No new lease may bind to the superseded revision.
```

---

## 147. Revocation Invariant

After descriptor or revision revocation:

```text
No new material access may be granted.
```

Late physical execution does not restore authority.

---

## 148. Removal Invariant

Material deletion must not occur before new access is blocked.

---

## 149. Backend Compromise Invariant

A compromised backend cannot return to normal availability without explicit remediation and reinitialization.

---

# Part XVIII — MVP State Boundary

## 150. Required MVP State Machines

The desktop MVP must implement:

```text
SecretDescriptorState
SecretRevisionState
SecretAvailabilityState
SecretLeaseState
SecretBackendState
SecretCandidateState
SecretValidationState
SecretOperationState
```

The MVP should implement basic:

```text
SecretRotationState
SecretRemovalState
```

The MVP may implement simplified:

```text
SecretMigrationState
SecretReconciliationState
```

Simplification must not remove:

- immutable revision identity;
- lease terminal states;
- backend lock state;
- uncertainty handling;
- safe candidate activation;
- revocation;
- deletion assurance distinction.

---

## 151. MVP Backend States

Required:

```text
OS secure-store backend:
    INITIALIZING
    AVAILABLE
    LOCKED
    UNAVAILABLE
    SHUTTING_DOWN
    TERMINATED

Memory backend:
    INITIALIZING
    AVAILABLE
    SHUTTING_DOWN
    TERMINATED

Environment backend:
    INITIALIZING
    AVAILABLE
    UNAVAILABLE
    TERMINATED
```

---

# Part XIX — State Decisions

## 152. Decisions

### Decision 1 — Independent state machines

Descriptor, revision, availability, lease, backend, rotation, migration, validation, removal, and operation states remain separate.

### Decision 2 — One active revision

A descriptor normally has exactly one active revision.

### Decision 3 — Rotation never mutates a revision

Rotation creates a new revision and supersedes the old revision.

### Decision 4 — Availability is derived

Availability summarizes resolvability but does not replace source state.

### Decision 5 — Leases are terminal once closed

Released, expired, revoked, rejected, and abandoned leases never reactivate.

### Decision 6 — Backend lock preserves identity

Locking changes availability, not descriptor identity.

### Decision 7 — Revocation differs from removal

Revocation blocks use; removal deletes or detaches material.

### Decision 8 — Removal differs from erasure guarantee

Deletion state records the actual assurance supported by the backend.

### Decision 9 — Uncertain is explicit

Potentially committed external operations enter `UNCERTAIN` and require reconciliation.

### Decision 10 — Last known good remains authoritative

A failed rotation or migration does not replace a valid current revision.

### Decision 11 — Security may bypass graceful drain

Compromise or explicit revocation may revoke active leases immediately.

### Decision 12 — State persistence contains no secret material

Only safe metadata is persisted in lifecycle state.

---

# Part XX — Open Decisions

## 153. Policy Decisions

Still to finalize:

- default lease duration;
- maximum lease duration;
- default lease policy during routine rotation;
- default lease policy during security rotation;
- descriptor suspension thresholds;
- availability cache TTL;
- validation evidence TTL;
- automatic refresh timing;
- grace period duration;
- backend recovery retry cadence;
- tombstone retention;
- superseded revision retention;
- source cleanup behavior after migration;
- orphaned lease terminal mapping;
- environment secret behavior after restart.

---

## 154. Platform Decisions

Still to finalize:

- exact Windows lock-state mapping;
- exact macOS Keychain prompt behavior;
- exact Linux Secret Service lock mapping;
- safe fallback when no platform store exists;
- whether OS prompt cancellation maps to `LOCKED`, `USER_ACTION_REQUIRED`, or operation `CANCELED`;
- process identity binding;
- child-process lease transfer;
- platform-specific secure delete assurance.

---

## 155. Event Decisions

To define in `EVENTS.md`:

- public versus restricted events;
- lease event visibility;
- backend lock event visibility;
- rotation progress granularity;
- migration progress granularity;
- validation event throttling;
- tombstone events;
- reconciliation events;
- audit sink separation;
- provider-facing credential availability events.

---

## 156. Error Decisions

To define in `ERRORS.md`:

- invalid transition codes;
- revision conflict codes;
- backend lock errors;
- uncertain operation errors;
- reconciliation errors;
- deletion assurance warnings;
- active lease cleanup failures;
- candidate cleanup errors;
- provider rotation mismatch;
- migration source/destination errors;
- security violation severity.

---

# Part XXI — Related Documents

## 157. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/CAPABILITY_MAP.md
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

02-modules/provider-management/MODULE.md
02-modules/provider-management/CONTRACT.md
02-modules/provider-management/STATES.md
```

Future Secret Management documents:

```text
03-infrastructure/secret-management/EVENTS.md
03-infrastructure/secret-management/ERRORS.md
03-infrastructure/secret-management/README.md
```

---

## 158. Summary

Secret Management uses independent state machines for logical identity, immutable material revisions, availability, temporary access authority, backend operations, candidate activation, rotation, migration, validation, removal, and reconciliation.

The core descriptor lifecycle is:

```text
REGISTERING
    ↓
ACTIVE
    ↓
ROTATING / MIGRATING / SUSPENDED
    ↓
ACTIVE
    ↓
REVOKED
    ↓
REMOVING
    ↓
REMOVED
    ↓
TOMBSTONED
```

The core revision lifecycle is:

```text
CANDIDATE
    ↓
VALIDATING
    ↓
READY
    ↓
ACTIVE
    ↓
SUPERSEDED / EXPIRED / REVOKED
    ↓
DELETION_PENDING
    ↓
DELETED
```

The core lease lifecycle is:

```text
REQUESTED
    ↓
EVALUATING
    ↓
GRANTED
    ↓
ACTIVE
    ↓
RELEASING
    ↓
RELEASED
```

The architecture preserves these invariants:

- one logical writer;
- one active revision per descriptor;
- immutable revisions;
- deny-by-default access;
- consumer- and purpose-bound leases;
- no lease reactivation;
- no secret material in state records;
- failed candidates do not replace last known good material;
- revocation blocks new access immediately;
- backend lock does not delete identity;
- uncertain external outcomes require reconciliation;
- removal does not overstate physical erasure guarantees.

This document is the state-machine source of truth for subsequent Secret Management events, errors, and implementation documentation.
