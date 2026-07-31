# Storage Module States

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines the operational state model of the Storage Module.

The state model describes whether the Storage capability can safely accept persistence operations.

It does not describe:

- business object lifecycle;
- database connection lifecycle;
- repository lifecycle;
- individual query execution;
- physical backend state;
- worker execution;
- Runtime processing state.

Storage state represents the logical availability and safety of the persistence capability.

---

# 2. State Ownership

Storage owns only its capability lifecycle.

```text
StorageCapabilityState
```

Storage does not own the lifecycle of persisted business objects.

For example:

```text
ReadingSession
```

is owned by Reading Session.

```text
TranslationResult
```

is owned by Translation.

```text
PersistenceEntry
```

may have persistence status,

but its business state remains external to Storage.

---

# 3. State Principles

---

## 3.1 Single Capability State

Storage has exactly one active capability state at any moment.

```text
CurrentStorageState = One State
```

Operation-specific execution may occur concurrently,

but it must not create multiple conflicting capability states.

---

## 3.2 Deterministic Transitions

Given the same:

- current state;
- accepted transition trigger;
- persistence condition;

Storage must produce the same next state.

---

## 3.3 Explicit Availability

Every public operation must be accepted or rejected according to the current Storage state.

No operation may rely on hidden availability assumptions.

---

## 3.4 Consistency Before Availability

Storage must reject operations rather than expose persistence state whose consistency cannot be guaranteed.

---

## 3.5 Implementation Independence

Storage states never expose:

- SQLite state;
- PostgreSQL state;
- connection pool state;
- filesystem mount state;
- repository initialization state;
- database transaction handles.

Implementation states must be mapped into the logical Storage state model.

---

## 3.6 No Business Semantics

Storage state transitions never change business meaning.

They only describe persistence capability availability and safety.

---

# 4. State Model

```text
                  Initialize
                      │
                      ▼
               Uninitialized
                      │
                      │ InitializationRequested
                      ▼
                Initializing
                 │         │
                 │         └──────────────┐
                 │                        │
                 ▼                        ▼
               Ready                   Failed
          ┌──────┼─────────┐              │
          │      │         │              │
          ▼      ▼         ▼              │
      Degraded Migrating Recovering       │
          │      │         │              │
          │      │         │              │
          └──────┴────┬────┘              │
                      ▼                    │
                    Ready                  │
                      │                    │
                      │ ShutdownRequested  │
                      ▼                    │
                ShuttingDown ◄─────────────┘
                      │
                      ▼
                    Stopped
```

---

# 5. State Summary

| State | Meaning |
|---|---|
| Uninitialized | Storage has not started initialization |
| Initializing | Storage is validating and preparing persistence capability |
| Ready | Storage can safely accept all supported public operations |
| Degraded | Storage remains partially available with reduced guarantees or supported scope |
| Migrating | Storage is performing an exclusive schema evolution operation |
| Recovering | Storage is restoring a consistent persisted state |
| Failed | Storage cannot safely provide persistence operations |
| ShuttingDown | Storage is completing or rejecting remaining work before termination |
| Stopped | Storage has terminated and accepts no operations |

---

# 6. Uninitialized

## Meaning

Storage exists as a configured capability,

but initialization has not begun.

No persistence contract is available.

## Entry Conditions

Storage enters Uninitialized when:

- the application creates the Storage capability;
- no initialization attempt has started;
- a complete process restart creates a new capability instance.

## Allowed Operations

```text
InitializeStorage

InspectStaticConfiguration
```

## Rejected Operations

All persistence commands and queries are rejected.

Examples:

```text
PersistObject

ReplaceObject

LoadObject

FindObjects

CreateSnapshot

RestoreSnapshot
```

## Exit Conditions

```text
InitializationRequested
```

moves Storage to:

```text
Initializing
```

## Invariants

- No public persistence operation is accepted.
- No persisted state is changed.
- Storage availability is explicit.
- Uninitialized is not a failure state.

---

# 7. Initializing

## Meaning

Storage is preparing the persistence capability for safe use.

Initialization may include:

- validating configuration;
- validating persistence metadata;
- checking schema compatibility;
- verifying required integrity markers;
- detecting incomplete migration;
- detecting required recovery;
- preparing implementation adapters.

These are implementation activities behind a logical state.

## Allowed Operations

```text
ReadInitializationStatus

CancelInitialization

ShutdownStorage
```

Public persistence commands and queries remain unavailable.

## Exit Conditions

### Initialization Completed

```text
Initializing

↓

Ready
```

### Reduced Availability Detected

```text
Initializing

↓

Degraded
```

This transition is allowed only when Storage can still provide an explicitly declared safe subset of operations.

### Recovery Required

```text
Initializing

↓

Recovering
```

### Migration Required

```text
Initializing

↓

Migrating
```

### Initialization Failed

```text
Initializing

↓

Failed
```

### Shutdown Requested

```text
Initializing

↓

ShuttingDown
```

## Invariants

- No unsafe persistence operation is accepted.
- Initialization never exposes partially prepared state as Ready.
- Backend-specific initialization details remain hidden.
- Failed initialization does not silently create new persisted data.
- Required migration or recovery is handled explicitly.

---

# 8. Ready

## Meaning

Storage is fully available within its declared contract.

All supported persistence commands and queries may be accepted.

## Allowed Operations

Examples include:

```text
PersistObject

ReplaceObject

PersistObjectSet

ArchiveObject

RestoreArchivedObject

DeleteObject

ApplyRetentionInstruction

LoadObject

LoadObjectVersion

CheckObjectExistence

FindObjects

CountObjects

LoadPersistenceMetadata

CreateSnapshot

LoadSnapshot

RestoreSnapshot
```

Administrative operations may also be accepted when permitted.

## Possible Exits

```text
Ready → Degraded

Ready → Migrating

Ready → Recovering

Ready → Failed

Ready → ShuttingDown
```

## Invariants

- Every successful write produces a complete committed result.
- Every successful read returns a committed persisted representation.
- PersistenceVersion is authoritative.
- Business semantics remain external.
- Backend identity remains hidden.
- Storage never reports Ready when persistence consistency is uncertain.

---

# 9. Degraded

## Meaning

Storage remains safely usable,

but one or more capabilities are temporarily restricted.

Examples may include:

- read-only availability;
- snapshot operations unavailable;
- historical version queries unavailable;
- archival operations temporarily unavailable;
- one declared persistence scope unavailable;
- reduced redundancy while durability remains safe.

Degraded does not mean inconsistent.

Storage may enter Degraded only if the remaining supported operations still satisfy their public contracts.

## Required Degradation Declaration

Storage must expose a logical capability description such as:

```text
AvailableOperations

UnavailableOperations

AffectedScope

DegradationReason

DetectedAt
```

It must not expose backend-specific details.

## Allowed Operations

Only operations explicitly declared safe may be accepted.

Examples could include:

```text
LoadObject

CheckObjectExistence

LoadPersistenceMetadata
```

or a restricted subset of writes.

## Rejected Operations

Any operation whose guarantee cannot be preserved must be rejected.

The expected error is typically:

```text
StorageOperationUnavailable
```

or:

```text
StorageUnavailable
```

depending on scope.

## Exit Conditions

### Full Capability Restored

```text
Degraded

↓

Ready
```

### Recovery Required

```text
Degraded

↓

Recovering
```

### Degradation Becomes Unsafe

```text
Degraded

↓

Failed
```

### Shutdown Requested

```text
Degraded

↓

ShuttingDown
```

## Invariants

- Degraded never permits weakened hidden guarantees.
- Unsupported operations are explicitly rejected.
- Accepted operations retain their documented correctness.
- Degraded state never changes business meaning.
- Degraded availability must be observable.

---

# 10. Migrating

## Meaning

Storage is changing persisted representation between SchemaVersion values.

Migration is an exclusive capability state when normal operations cannot safely continue during the migration boundary.

Not every small representation conversion requires the global Migrating state.

This state is used only when capability-wide consistency requires exclusivity.

## Entry Conditions

Storage may enter Migrating when:

- current persisted schema is incompatible with the active contract;
- an explicit migration is requested;
- initialization detects mandatory schema evolution;
- recovery requires controlled migration.

## Allowed Operations

```text
ReadMigrationStatus

CancelMigration
```

Cancellation is allowed only before the migration reaches an irreversible contract boundary.

Other allowed operations must be explicitly declared safe.

## Rejected Operations

Normal persistence writes are rejected unless the migration contract explicitly supports them safely.

Reads may be rejected or restricted depending on migration scope.

## Exit Conditions

### Migration Completed

```text
Migrating

↓

Ready
```

### Migration Completed with Restricted Capability

```text
Migrating

↓

Degraded
```

### Migration Requires Recovery

```text
Migrating

↓

Recovering
```

### Migration Failed Safely

```text
Migrating

↓

Failed
```

Storage may enter Failed only when it cannot guarantee immediate safe operation.

### Shutdown Requested

```text
Migrating

↓

ShuttingDown
```

Shutdown may be delayed until the persistence boundary becomes safe.

## Invariants

- Source and target SchemaVersion values are explicit.
- Migration history is preserved.
- Migration never silently changes business meaning.
- A successful migration exposes a consistent target representation.
- A failed migration leaves a recoverable state where promised.
- Storage never reports Ready before migration validation succeeds.

---

# 11. Recovering

## Meaning

Storage is restoring a known consistent persistence state.

Recovery may use:

- snapshots;
- recovery points;
- migration checkpoints;
- persistence journals;
- backup representations;
- integrity metadata.

Recovery restores persistence state only.

It does not resume business or Runtime execution.

## Entry Conditions

Storage enters Recovering when:

- initialization detects incomplete persistence work;
- persistence consistency becomes uncertain;
- an explicit restore operation requires global exclusivity;
- migration cannot continue safely without restoration;
- integrity validation fails but recovery remains possible.

## Allowed Operations

```text
ReadRecoveryStatus

CancelRecovery
```

Cancellation is permitted only when the current recovery boundary remains safe.

Normal writes are rejected.

Reads are rejected unless explicitly declared safe against a stable restored scope.

## Exit Conditions

### Recovery Completed

```text
Recovering

↓

Ready
```

### Partial Capability Restored

```text
Recovering

↓

Degraded
```

### Recovery Failed

```text
Recovering

↓

Failed
```

### Shutdown Requested

```text
Recovering

↓

ShuttingDown
```

## Invariants

- Recovery is idempotent where required by contract.
- Committed operations must not be duplicated.
- Partially restored state is not exposed as normal active persistence.
- Recovery validation completes before Ready.
- Business modules must reevaluate recovered objects.
- Runtime work is not restarted by Storage.

---

# 12. Failed

## Meaning

Storage cannot safely provide persistence capability.

Failed means Storage cannot currently guarantee one or more fundamental properties such as:

- consistency;
- durability;
- integrity;
- deterministic retrieval;
- safe version handling.

Failed is not used for a single rejected command.

A normal command error does not automatically move the entire capability to Failed.

## Allowed Operations

```text
InspectFailure

RetryInitialization

BeginRecovery

ShutdownStorage
```

## Rejected Operations

All normal persistence commands and queries are rejected unless a strictly limited diagnostic query is explicitly declared safe.

## Exit Conditions

### Reinitialization Requested

```text
Failed

↓

Initializing
```

### Recovery Requested

```text
Failed

↓

Recovering
```

### Shutdown Requested

```text
Failed

↓

ShuttingDown
```

## Invariants

- No normal persistence write is accepted.
- Storage does not pretend to be partially available without entering Degraded.
- Existing persisted data is not mutated except by explicit recovery.
- Failure details are mapped to stable Storage error contracts.
- Backend-specific errors do not cross the public boundary.

---

# 13. ShuttingDown

## Meaning

Storage is terminating capability operation safely.

It may be:

- waiting for accepted atomic operations to reach a safe boundary;
- rejecting new operations;
- completing persistence metadata updates;
- closing implementation resources;
- preserving recovery markers.

## Entry Conditions

Storage may enter ShuttingDown from any non-terminal state.

## Operation Acceptance

New mutation commands are rejected.

Read operations may be rejected or allowed briefly according to shutdown policy.

No operation may be accepted if completion cannot be guaranteed before termination.

## Exit Conditions

```text
ShutdownCompleted

↓

Stopped
```

A shutdown failure does not return Storage to Ready automatically.

The capability remains in ShuttingDown or terminates with failure metadata for the next initialization.

## Invariants

- No new unsafe work is accepted.
- Accepted atomic operations are completed or rolled back safely.
- Required recovery metadata is preserved.
- Shutdown does not silently discard committed operations.
- ShuttingDown is not yet terminal.

---

# 14. Stopped

## Meaning

Storage capability has terminated.

No public persistence operation is available.

## Allowed Operations

None.

A new capability instance may be created by the application,

but the stopped instance does not restart.

## Invariants

- Stopped is terminal.
- No operations are accepted.
- No state transition originates from Stopped.
- Restart requires creation or initialization of a new Storage capability instance.

---

# 15. Operation State Versus Capability State

Most persistence operations do not change the global Storage capability state.

For example:

```text
Ready

PersistObject executes

Ready
```

The operation has its own outcome:

```text
Accepted

Executing

Succeeded
```

or:

```text
Accepted

Executing

Failed
```

but Storage remains Ready if the failure is isolated to that operation.

---

## 15.1 Operation Failure

Examples of isolated operation failures include:

```text
PersistenceEntryNotFound

PersistenceEntryAlreadyExists

PersistenceVersionConflict

InvalidPersistenceRequest

RetentionViolation

AccessDenied
```

These errors normally do not change the capability state.

---

## 15.2 Capability Failure

Examples that may change capability state include:

```text
PersistenceConsistencyViolation

UnrecoverableMetadataCorruption

CapabilityIntegrityFailure

GlobalStorageUnavailable
```

The exact transition depends on whether safe restricted operation remains possible.

```text
Ready → Degraded
```

or:

```text
Ready → Recovering
```

or:

```text
Ready → Failed
```

---

# 16. Atomic Operation Behavior

`PersistObjectSet` does not require a global Transaction state.

Atomic execution is an operation guarantee,

not necessarily a capability lifecycle state.

Conceptually:

```text
Storage State: Ready

Atomic Operation: Executing

Storage State: Ready
```

The physical implementation may use a transaction internally.

That detail is not part of the public state model.

---

## 16.1 Atomic Success

Every declared mutation becomes visible as one logical committed result.

---

## 16.2 Atomic Failure

No declared mutation becomes visible.

The previous consistent state remains authoritative.

---

## 16.3 Capability Impact

An atomic operation failure changes capability state only when Storage cannot guarantee that rollback or isolation succeeded.

For example:

```text
AtomicOperationFailed
```

with verified rollback:

```text
Ready → Ready
```

But:

```text
Persistence consistency uncertain
```

may require:

```text
Ready → Recovering
```

---

# 17. Snapshot Behavior

Creating or loading an ordinary snapshot does not necessarily change capability state.

```text
Ready

↓

CreateSnapshot

↓

Ready
```

A global restore may require:

```text
Ready

↓

Recovering

↓

Ready
```

The distinction depends on whether the operation temporarily makes normal persisted state unavailable or uncertain.

---

# 18. Migration Behavior

Migration may occur at different scopes.

---

## 18.1 Lazy Object Migration

An individual object may be transformed during read or replacement without changing global Storage state,

provided:

- the transformation is deterministic;
- consistency remains guaranteed;
- concurrent operations remain safe;
- contract compatibility is preserved.

Storage remains:

```text
Ready
```

---

## 18.2 Exclusive Migration

A capability-wide incompatible migration requires:

```text
Ready

↓

Migrating

↓

Ready
```

or another valid exit state.

Storage must never use global Migrating when exclusivity is unnecessary.

---

# 19. Recovery Behavior

Recovery is entered only when normal persistence cannot safely continue.

It is not a generic retry state.

For isolated command failures,

Storage returns an error and remains in the current safe capability state.

---

# 20. State Transition Table

| Current State | Trigger | Next State | Meaning |
|---|---|---|---|
| Uninitialized | InitializationRequested | Initializing | Capability preparation begins |
| Uninitialized | ShutdownRequested | ShuttingDown | Capability terminates before initialization |
| Initializing | InitializationCompleted | Ready | Full capability is available |
| Initializing | RestrictedCapabilityDetected | Degraded | Safe subset is available |
| Initializing | MigrationRequired | Migrating | Exclusive schema evolution is required |
| Initializing | RecoveryRequired | Recovering | Persisted consistency must be restored |
| Initializing | InitializationFailed | Failed | Safe operation cannot begin |
| Initializing | ShutdownRequested | ShuttingDown | Initialization stops safely |
| Ready | RestrictedCapabilityDetected | Degraded | Full capability is temporarily reduced |
| Ready | ExclusiveMigrationRequested | Migrating | Capability-wide migration begins |
| Ready | RecoveryRequired | Recovering | Persistence consistency must be restored |
| Ready | UnrecoverableCapabilityFailure | Failed | Safe operation is unavailable |
| Ready | ShutdownRequested | ShuttingDown | Safe termination begins |
| Degraded | FullCapabilityRestored | Ready | All declared operations become available |
| Degraded | RecoveryRequired | Recovering | Restricted capability is no longer sufficient |
| Degraded | CapabilityFailure | Failed | No safe supported scope remains |
| Degraded | ShutdownRequested | ShuttingDown | Safe termination begins |
| Migrating | MigrationCompleted | Ready | Target schema is fully available |
| Migrating | MigrationCompletedWithRestrictions | Degraded | Migration completes with reduced capability |
| Migrating | RecoveryRequired | Recovering | Migration requires restoration |
| Migrating | MigrationFailed | Failed | Safe normal operation cannot resume |
| Migrating | ShutdownRequested | ShuttingDown | Migration reaches a safe boundary before termination |
| Recovering | RecoveryCompleted | Ready | Full persistence capability is restored |
| Recovering | PartialRecoveryCompleted | Degraded | Safe restricted capability is restored |
| Recovering | RecoveryFailed | Failed | Persistence cannot be restored safely |
| Recovering | ShutdownRequested | ShuttingDown | Recovery reaches a safe boundary before termination |
| Failed | RetryInitialization | Initializing | Capability preparation is attempted again |
| Failed | RecoveryRequested | Recovering | Explicit recovery begins |
| Failed | ShutdownRequested | ShuttingDown | Failed capability terminates |
| ShuttingDown | ShutdownCompleted | Stopped | Capability termination completes |

---

# 21. Invalid Transitions

The following transitions are invalid.

```text
Stopped → Ready

Failed → Ready

Uninitialized → Ready

Migrating → Uninitialized

Recovering → Migrating

ShuttingDown → Ready
```

A target state may only be reached through its documented validation boundary.

For example:

```text
Failed

↓

Initializing

↓

Ready
```

or:

```text
Failed

↓

Recovering

↓

Ready
```

Storage must never skip required validation.

---

# 22. Transition Guard Rules

---

## 22.1 Entering Ready

Storage may enter Ready only when:

- required initialization completed;
- persistence metadata is consistent;
- mandatory migration completed;
- required recovery completed;
- supported operations can satisfy public contracts.

---

## 22.2 Entering Degraded

Storage may enter Degraded only when:

- the remaining safe capability is explicit;
- accepted operations preserve full documented guarantees;
- unsupported operations can be deterministically rejected;
- persistence consistency remains known.

---

## 22.3 Entering Migrating

Storage may enter Migrating only when:

- source SchemaVersion is known;
- target SchemaVersion is declared;
- migration scope is defined;
- migration strategy is valid;
- recovery or rollback protection exists where required.

---

## 22.4 Entering Recovering

Storage may enter Recovering only when:

- normal operations cannot safely continue;
- a recovery source or strategy exists;
- the recovery scope is explicit;
- active operations have reached a safe boundary.

---

## 22.5 Entering Failed

Storage enters Failed only when no safe supported capability remains.

A single object-level error is insufficient.

---

## 22.6 Entering Stopped

Storage enters Stopped only after:

- new operations are rejected;
- active atomic operations complete or roll back;
- required metadata is preserved;
- implementation resources terminate safely.

---

# 23. Command Acceptance Matrix

| Operation | Uninitialized | Initializing | Ready | Degraded | Migrating | Recovering | Failed | ShuttingDown | Stopped |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| InitializeStorage | Yes | No | No | No | No | No | Retry only | No | No |
| PersistObject | No | No | Yes | Conditional | No | No | No | No | No |
| ReplaceObject | No | No | Yes | Conditional | No | No | No | No | No |
| PersistObjectSet | No | No | Yes | Conditional | No | No | No | No | No |
| LoadObject | No | No | Yes | Conditional | Conditional | Conditional | No | Conditional | No |
| FindObjects | No | No | Yes | Conditional | Conditional | Conditional | No | No | No |
| CreateSnapshot | No | No | Yes | Conditional | No | No | No | No | No |
| RestoreSnapshot | No | No | Conditional | No | No | Recovery path | Recovery path | No | No |
| BeginMigration | No | No | Yes | Conditional | No | No | No | No | No |
| BeginRecovery | No | Conditional | Conditional | Conditional | Conditional | No | Yes | No | No |
| ShutdownStorage | Yes | Yes | Yes | Yes | Yes | Yes | Yes | No | No |

`Conditional` means availability must be explicitly declared by the active capability contract.

---

# 24. Error Mapping by State

---

## 24.1 Uninitialized

Rejected public operations return:

```text
StorageNotInitialized
```

---

## 24.2 Initializing

Rejected public operations return:

```text
StorageInitializing
```

---

## 24.3 Degraded

Unsupported operations return:

```text
StorageOperationUnavailable
```

The error must identify the logical unavailable capability,

not backend details.

---

## 24.4 Migrating

Rejected operations return:

```text
StorageMigrating
```

---

## 24.5 Recovering

Rejected operations return:

```text
StorageRecovering
```

---

## 24.6 Failed

Rejected operations return:

```text
StorageUnavailable
```

---

## 24.7 ShuttingDown

Rejected operations return:

```text
StorageShuttingDown
```

---

## 24.8 Stopped

Rejected operations return:

```text
StorageStopped
```

Detailed error definitions belong in `ERRORS.md`.

---

# 25. State Events

Every successful capability transition publishes a Storage state event.

Examples include:

```text
StorageInitializationStarted

StorageReady

StorageDegraded

StorageMigrationStarted

StorageRecoveryStarted

StorageFailed

StorageShutdownStarted

StorageStopped
```

State events describe completed or accepted capability facts.

Detailed definitions belong in `EVENTS.md`.

---

# 26. Observability

Every state transition should record:

```text
PreviousState

NextState

TransitionReason

CorrelationId

OccurredAt

CapabilityScope

Outcome
```

Optional fields may include:

```text
SchemaVersion

MigrationId

RecoveryPointId

DegradationScope
```

Observability must not expose:

- credentials;
- connection strings;
- physical paths;
- raw payloads;
- backend secrets.

---

# 27. State Metrics

Recommended capability metrics include:

```text
storage_state_transition_total

storage_ready_duration_seconds

storage_degraded_duration_seconds

storage_migrating_duration_seconds

storage_recovering_duration_seconds

storage_failed_duration_seconds

storage_initialization_failed_total

storage_recovery_failed_total

storage_migration_failed_total
```

These metrics describe logical Storage capability state.

Backend health metrics remain implementation-specific.

---

# 28. Architecture Invariants

The following invariants always apply.

1. Storage has exactly one active capability state.
2. Stopped is terminal.
3. Failed never transitions directly to Ready.
4. Ready is entered only after consistency validation.
5. Degraded operations retain their full documented guarantees.
6. Degraded never hides weakened consistency.
7. A single command failure does not automatically fail the capability.
8. Atomic operation execution does not require a global Transaction state.
9. Storage never exposes transaction handles in its lifecycle.
10. Migration changes persistence representation only.
11. Recovery restores persistence state only.
12. Recovery never resumes business workflows.
13. Storage state never represents business object state.
14. Backend-specific states never cross the public boundary.
15. Public writes are rejected whenever consistency cannot be guaranteed.
16. Persisted committed data is never exposed partially.
17. Capability transitions are deterministic.
18. Every transition is observable.
19. Shutdown preserves a safe persistence boundary.
20. No hidden state exists outside this state model.

---

# 29. Related Documents

| Document | Responsibility |
|---|---|
| README.md | Storage overview |
| MODULE.md | Ownership and architectural boundaries |
| CONTRACT.md | Public persistence contracts |
| STATES.md | Storage capability lifecycle |
| EVENTS.md | Persistence and capability events |
| ERRORS.md | Persistence failure model |
| MODELS.md | Persistence concepts and structures |
| MIGRATION.md | Schema evolution and migration |

---

# 30. Summary

The Storage state model describes whether CRAI persistence capability can safely operate.

The capability lifecycle is:

```text
Uninitialized

↓

Initializing

↓

Ready
```

with controlled transitions into:

```text
Degraded

Migrating

Recovering

Failed

ShuttingDown

Stopped
```

The state model deliberately excludes:

- repositories;
- backend engines;
- database transactions;
- individual object lifecycle;
- business workflow state;
- Runtime execution state.

This separation ensures that:

- capability availability is explicit;
- persistence consistency has priority;
- backend implementations remain replaceable;
- isolated operation failures do not corrupt global state;
- migration and recovery have clear boundaries;
- business modules remain independent from Storage lifecycle mechanics.

---

# End of Document