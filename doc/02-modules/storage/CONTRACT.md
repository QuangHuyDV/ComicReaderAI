# Storage Module Contract

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines the public contract of the Storage Module.

Storage provides implementation-independent persistence operations for CRAI modules.

The contract defines:

- accepted persistence requests;
- persistence identities;
- version behavior;
- consistency guarantees;
- snapshot behavior;
- retention behavior;
- recovery behavior;
- persistence outcomes;
- stable error contracts.

The contract does not define:

- repository patterns;
- database tables;
- SQL statements;
- physical file layouts;
- database engines;
- storage drivers;
- backend maintenance procedures.

Consumers depend on persistence behavior,

not persistence implementation.

---

# 2. Contract Principles

All Storage contracts follow the principles below.

---

## 2.1 Explicit Persistence

Persistence occurs only through an explicit Storage command.

Storage must not persist application state through hidden side effects.

---

## 2.2 External Business Ownership

The originating module retains ownership of every persisted business object.

Storage owns only the persistence representation and persistence metadata.

---

## 2.3 Semantic Transparency

Storage treats payloads as semantically opaque.

It may validate persistence structure,

but it must not evaluate business meaning.

---

## 2.4 Implementation Independence

Public contracts never expose:

- backend identity;
- database capability;
- driver behavior;
- physical location;
- implementation pattern.

---

## 2.5 Deterministic Outcomes

Given the same:

- command;
- expected persistence version;
- current persisted state;

Storage must produce the same logical outcome.

---

## 2.6 Explicit Failure

Persistence failures must be returned through stable Storage errors.

Storage must never silently lose, partially persist, or ignore requested data.

---

# 3. Contract Boundary

The Storage contract begins when a consumer submits a valid persistence operation.

It ends when Storage returns one of the following:

```text
Persistence Result

Persistence Error
```

Storage does not decide what business action follows the result.

For example:

```text
Storage returns PersistenceVersionConflict.

Reading Session decides whether to reload,
retry,
or abandon the business operation.
```

---

# 4. Shared Types

The following types are used across Storage commands and queries.

---

## 4.1 PersistenceKey

A PersistenceKey uniquely identifies one persisted application object.

```text
PersistenceKey

├── ObjectType
└── ObjectId
```

### ObjectType

Identifies the owning object category.

Examples may include:

```text
ReadingSession

ContentRevision

TranslationResult
```

ObjectType is supplied by the owning module.

Storage does not interpret its business meaning.

### ObjectId

Identifies the application object within its ObjectType.

ObjectId is owned by the originating module.

---

## 4.2 PersistenceId

PersistenceId uniquely identifies the persisted representation managed by Storage.

```text
PersistenceId
```

It is assigned and managed by Storage.

PersistenceId must not replace the business ObjectId.

---

## 4.3 PersistencePayload

PersistencePayload contains the serialized representation supplied by the owning module.

```text
PersistencePayload

├── Content
├── ContentType
└── SchemaVersion
```

Storage must not change payload meaning.

Serialization format may be agreed through contract metadata,

but physical encoding remains implementation-specific.

---

## 4.4 PersistenceVersion

PersistenceVersion identifies the current persisted version of an object.

```text
PersistenceVersion
```

It is:

- assigned by Storage;
- monotonically advanced;
- read-only to consumers;
- used for stale-write detection.

PersistenceVersion is not a business revision.

```text
PersistenceVersion != ContentRevision
```

---

## 4.5 SchemaVersion

SchemaVersion identifies the persisted representation format.

```text
SchemaVersion
```

It supports:

- compatibility validation;
- migration;
- recovery;
- representation evolution.

SchemaVersion does not represent business state.

---

## 4.6 PersistenceMetadata

PersistenceMetadata contains Storage-owned information.

```text
PersistenceMetadata

├── PersistenceId
├── PersistenceKey
├── PersistenceVersion
├── SchemaVersion
├── CreatedAt
├── UpdatedAt
├── ArchivedAt
├── DeletedAt
└── RetentionClass
```

Fields may be absent where not applicable.

---

## 4.7 ExpectedPersistenceVersion

ExpectedPersistenceVersion allows a consumer to declare the persisted version it expects to replace.

Possible values include:

```text
None

Exact Version

Any Version
```

### None

The consumer expects no object to exist.

### Exact Version

The consumer expects the stored version to match a specific PersistenceVersion.

### Any Version

The consumer accepts replacement regardless of current version.

Use of `Any Version` should be restricted because it weakens stale-write protection.

---

## 4.8 OperationId

OperationId uniquely identifies a persistence command.

```text
OperationId
```

It may be used for:

- idempotency;
- tracing;
- duplicate detection;
- audit correlation.

---

## 4.9 CorrelationId

CorrelationId links a Storage operation to a wider application workflow.

Storage records it for observability.

It does not interpret workflow meaning.

---

# 5. Persistence Commands

Storage exposes commands that request changes to persisted state.

Commands describe desired persistence outcomes.

They do not expose transaction handles or backend operations.

---

## 5.1 PersistObject

PersistObject creates a new persisted object.

### Request

```text
PersistObject

├── OperationId
├── CorrelationId
├── PersistenceKey
├── PersistencePayload
├── ExpectedPersistenceVersion = None
├── RetentionInstruction
└── Metadata
```

### Preconditions

- PersistenceKey is valid.
- PersistencePayload is structurally valid.
- No active persisted object exists for the same PersistenceKey.
- SchemaVersion is supported or migratable.

### Success Result

```text
ObjectPersisted

├── PersistenceId
├── PersistenceKey
├── PersistenceVersion
├── SchemaVersion
├── CreatedAt
└── OperationId
```

### Failure Outcomes

```text
PersistenceEntryAlreadyExists

InvalidPersistenceRequest

UnsupportedSchemaVersion

SerializationFailed

StorageUnavailable

InternalStorageFailure
```

---

## 5.2 ReplaceObject

ReplaceObject replaces the persisted representation of an existing object.

### Request

```text
ReplaceObject

├── OperationId
├── CorrelationId
├── PersistenceKey
├── PersistencePayload
├── ExpectedPersistenceVersion
├── RetentionInstruction
└── Metadata
```

### Preconditions

- The persisted object exists.
- ExpectedPersistenceVersion matches the current persisted version unless `Any Version` is explicitly permitted.
- The payload can be stored safely.

### Success Result

```text
ObjectReplaced

├── PersistenceId
├── PreviousPersistenceVersion
├── PersistenceVersion
├── SchemaVersion
├── UpdatedAt
└── OperationId
```

### Failure Outcomes

```text
PersistenceEntryNotFound

PersistenceVersionConflict

UnsupportedSchemaVersion

SerializationFailed

StorageUnavailable

InternalStorageFailure
```

---

## 5.3 PersistObjectSet

PersistObjectSet applies multiple persistence changes atomically.

### Request

```text
PersistObjectSet

├── OperationId
├── CorrelationId
└── Operations
    ├── PersistObject
    ├── ReplaceObject
    ├── ArchiveObject
    └── DeleteObject
```

### Success Guarantee

Either:

```text
All declared persistence changes become visible.
```

or:

```text
No declared persistence change becomes visible.
```

### Success Result

```text
ObjectSetPersisted

├── OperationId
├── Results
└── CompletedAt
```

### Failure Outcomes

```text
AtomicOperationFailed

PersistenceVersionConflict

PersistenceEntryNotFound

PersistenceEntryAlreadyExists

RetentionViolation

StorageUnavailable

InternalStorageFailure
```

Storage must not expose a partially committed result.

---

## 5.4 ArchiveObject

ArchiveObject moves an active persisted object into archival state.

### Request

```text
ArchiveObject

├── OperationId
├── CorrelationId
├── PersistenceKey
├── ExpectedPersistenceVersion
└── ArchiveReason
```

### Success Result

```text
ObjectArchived

├── PersistenceId
├── PersistenceKey
├── PersistenceVersion
├── ArchivedAt
└── OperationId
```

### Contract Rules

- Archived objects are excluded from normal active-object queries unless explicitly requested.
- Archival does not imply physical deletion.
- Archival must preserve required historical metadata.
- Storage does not determine whether archival is business-appropriate.

---

## 5.5 RestoreArchivedObject

RestoreArchivedObject returns an archived persisted object to active persistence.

### Request

```text
RestoreArchivedObject

├── OperationId
├── CorrelationId
├── PersistenceKey
└── ExpectedPersistenceVersion
```

### Success Result

```text
ArchivedObjectRestored

├── PersistenceId
├── PersistenceKey
├── PersistenceVersion
├── RestoredAt
└── OperationId
```

The owning module must reevaluate the restored business object before using it.

---

## 5.6 DeleteObject

DeleteObject marks or removes a persisted object according to an explicit deletion mode.

### Request

```text
DeleteObject

├── OperationId
├── CorrelationId
├── PersistenceKey
├── ExpectedPersistenceVersion
├── DeletionMode
└── DeletionReason
```

### DeletionMode

```text
Logical

Physical
```

### Logical Deletion

Logical deletion makes an object unavailable to normal reads while preserving its persisted representation for a defined period.

### Physical Deletion

Physical deletion removes the persisted representation according to implementation capability and policy.

### Success Result

```text
ObjectDeleted

├── PersistenceId
├── PersistenceKey
├── DeletionMode
├── DeletedAt
└── OperationId
```

### Failure Outcomes

```text
PersistenceEntryNotFound

PersistenceVersionConflict

RetentionViolation

DeletionNotSupported

StorageUnavailable

InternalStorageFailure
```

---

## 5.7 ApplyRetentionInstruction

ApplyRetentionInstruction updates persistence retention metadata.

### Request

```text
ApplyRetentionInstruction

├── OperationId
├── CorrelationId
├── PersistenceKey
├── ExpectedPersistenceVersion
└── RetentionInstruction
```

### Success Result

```text
RetentionInstructionApplied

├── PersistenceId
├── PersistenceVersion
├── RetentionInstruction
├── AppliedAt
└── OperationId
```

Storage applies the instruction.

It does not determine the underlying business policy.

---

# 6. Persistence Queries

Queries retrieve persisted state without modifying it.

Queries must not produce hidden writes.

---

## 6.1 LoadObject

LoadObject retrieves the active persisted representation associated with a PersistenceKey.

### Request

```text
LoadObject

├── CorrelationId
├── PersistenceKey
└── ReadOptions
```

### Success Result

```text
PersistedObject

├── PersistenceMetadata
└── PersistencePayload
```

### Failure Outcomes

```text
PersistenceEntryNotFound

PersistenceEntryArchived

PersistenceEntryDeleted

DeserializationFailed

StorageUnavailable

InternalStorageFailure
```

---

## 6.2 LoadObjectVersion

LoadObjectVersion retrieves a specific persisted version where historical version access is supported.

### Request

```text
LoadObjectVersion

├── CorrelationId
├── PersistenceKey
└── PersistenceVersion
```

### Success Result

```text
PersistedObjectVersion

├── PersistenceMetadata
└── PersistencePayload
```

Historical version access is optional unless explicitly required by the owning module contract.

---

## 6.3 CheckObjectExistence

CheckObjectExistence determines whether a persisted object currently exists.

### Request

```text
CheckObjectExistence

├── CorrelationId
├── PersistenceKey
└── ExistenceOptions
```

### Success Result

```text
ObjectExistence

├── Exists
├── PersistenceState
├── PersistenceVersion
└── CheckedAt
```

Possible PersistenceState values include:

```text
Active

Archived

LogicallyDeleted

Absent
```

---

## 6.4 FindObjects

FindObjects retrieves persisted objects matching a bounded persistence query.

### Request

```text
FindObjects

├── CorrelationId
├── ObjectType
├── QuerySpecification
├── PageRequest
└── ReadOptions
```

### QuerySpecification

QuerySpecification may reference declared persistence metadata.

It must not require Storage to understand arbitrary business semantics.

Supported query fields may include:

```text
ObjectId

ObjectType

PersistenceVersion

SchemaVersion

CreatedAt

UpdatedAt

ArchivedAt

RetentionClass

DeclaredIndex
```

### Success Result

```text
PersistedObjectPage

├── Items
├── PageInformation
└── QueryCompletedAt
```

---

## 6.5 CountObjects

CountObjects returns the number of persisted objects matching a supported persistence query.

### Request

```text
CountObjects

├── CorrelationId
├── ObjectType
└── QuerySpecification
```

### Success Result

```text
ObjectCount

├── Count
└── CountedAt
```

---

## 6.6 LoadPersistenceMetadata

LoadPersistenceMetadata retrieves Storage-owned metadata without loading the complete payload.

### Request

```text
LoadPersistenceMetadata

├── CorrelationId
└── PersistenceKey
```

### Success Result

```text
PersistenceMetadata
```

This query is intended for:

- version checks;
- existence validation;
- retention evaluation;
- lightweight synchronization;
- diagnostics.

---

# 7. Atomic Operation Contract

Consumers request atomic behavior through `PersistObjectSet`.

They do not manage:

```text
BeginTransaction

CommitTransaction

RollbackTransaction
```

These mechanics remain internal to Storage.

---

## 7.1 Atomicity Guarantee

For an accepted atomic request:

```text
Success
```

means every declared change is committed.

```text
Failure
```

means none of the declared changes becomes visible.

---

## 7.2 Failure Isolation

An atomic operation failure must preserve the previously committed consistent state.

Storage must not return success while background completion remains uncertain.

---

## 7.3 Unsupported Atomic Scope

Some implementations may not support atomicity across all persistence classes.

In that case Storage must reject the request before applying any change.

It must return:

```text
AtomicScopeUnsupported
```

Storage must not silently downgrade atomicity.

---

# 8. Version Contract

PersistenceVersion is managed exclusively by Storage.

---

## 8.1 Initial Version

A newly persisted object receives an initial PersistenceVersion.

The actual encoding is implementation-independent.

Consumers must treat the value as opaque.

---

## 8.2 Version Advancement

Every successful mutation of a persisted representation advances PersistenceVersion.

Mutations include:

- replacement;
- archival;
- restoration;
- logical deletion;
- retention metadata changes where contractually versioned.

---

## 8.3 Stale Write Detection

When ExpectedPersistenceVersion does not match the current version,

Storage returns:

```text
PersistenceVersionConflict
```

No mutation may occur.

---

## 8.4 Business Revision Separation

Storage must never infer business revision from PersistenceVersion.

The owning module may persist a business revision inside the payload,

but the two values remain independently owned.

---

## 8.5 Version Immutability

Previously assigned PersistenceVersion values must never be reassigned to different persisted content.

---

# 9. Snapshot Contract

Storage may support stable persistence snapshots.

---

## 9.1 CreateSnapshot

### Request

```text
CreateSnapshot

├── OperationId
├── CorrelationId
├── SnapshotScope
├── SnapshotReason
└── Metadata
```

### SnapshotScope

A snapshot scope may identify:

- one persisted object;
- a declared object set;
- a module-owned persistence namespace;
- an application persistence boundary.

### Success Result

```text
SnapshotCreated

├── SnapshotId
├── SnapshotScope
├── CreatedAt
├── IncludedPersistenceVersions
└── OperationId
```

---

## 9.2 LoadSnapshot

LoadSnapshot retrieves snapshot metadata or content.

```text
LoadSnapshot

├── CorrelationId
└── SnapshotId
```

Success returns:

```text
PersistenceSnapshot
```

---

## 9.3 RestoreSnapshot

### Request

```text
RestoreSnapshot

├── OperationId
├── CorrelationId
├── SnapshotId
├── RestoreMode
└── ConflictPolicy
```

### Success Result

```text
SnapshotRestored

├── SnapshotId
├── RestoredAt
├── RestoredObjects
└── OperationId
```

### Contract Boundary

Snapshot restoration restores persistence state.

It does not automatically resume:

- Reading Sessions;
- Runtime tasks;
- processing pipelines;
- user workflows.

Owning modules must reevaluate restored objects.

---

## 9.4 Snapshot Consistency

A successful snapshot must represent a declared consistent persistence boundary.

Storage must reject snapshot creation if it cannot satisfy the requested consistency level.

---

# 10. Retention Contract

Retention instructions originate outside Storage.

Storage validates and applies them as persistence metadata.

---

## 10.1 RetentionInstruction

A RetentionInstruction may contain:

```text
RetentionClass

RetainUntil

ArchiveAfter

DeleteAfter

DeletionMode

LegalHold
```

Not every field is required.

---

## 10.2 Retention Enforcement

Storage must prevent operations that violate active retention constraints.

Examples include:

- physical deletion before `RetainUntil`;
- deletion while `LegalHold` is active;
- archival before required active retention has ended.

---

## 10.3 Retention Expiration

Retention expiration does not necessarily cause immediate deletion.

It indicates that deletion or archival is permitted according to external policy.

Storage must not invent a deletion decision unless the contract explicitly supplies an automatic retention instruction.

---

# 11. Archival and Deletion Contract

Archival and deletion are distinct persistence operations.

```text
Archive != Delete
```

---

## 11.1 Archival Guarantee

Archived data remains retrievable through explicit archival queries while retention policy permits.

---

## 11.2 Logical Deletion Guarantee

Logically deleted data must not appear in normal active-object queries.

---

## 11.3 Physical Deletion Guarantee

Physical deletion guarantees only the abstraction promised by the active implementation and deployment policy.

Secure erasure must not be implied unless explicitly supported and declared.

---

## 11.4 Deletion Idempotency

Repeating the same DeleteObject command with the same OperationId must not produce duplicate deletion effects.

---

# 12. Recovery Contract

Recovery restores persisted consistency after failure or data loss.

---

## 12.1 Recovery Inputs

Recovery may use:

- snapshots;
- recovery points;
- persistence logs;
- backup representations;
- migration checkpoints.

---

## 12.2 Recovery Outcome

A successful recovery returns:

```text
StorageRecovered

├── RecoveryPointId
├── RestoredScope
├── RestoredAt
└── ValidationResult
```

---

## 12.3 Recovery Validation

Storage must validate persistence integrity before reporting successful recovery.

Business validation remains the responsibility of owning modules.

---

## 12.4 Recovery Failure

When recovery cannot guarantee consistent persisted state,

Storage must return:

```text
RecoveryFailed
```

It must not expose partially restored state as normal active persistence.

---

# 13. Migration Contract

Migration changes persisted representation between SchemaVersion values.

Detailed migration rules are defined in `MIGRATION.md`.

---

## 13.1 Migration Guarantees

An accepted migration must provide:

- source version detection;
- target version declaration;
- compatibility validation;
- deterministic transformation;
- migration history;
- failure reporting;
- rollback or protected recovery where promised.

---

## 13.2 Business Meaning Preservation

Migration must not silently change business meaning.

Any semantic transformation must be:

- explicitly defined;
- owned by the relevant business module;
- versioned;
- auditable.

---

## 13.3 Migration Atomicity

Where migration is declared atomic,

failure must leave the previous valid representation recoverable.

Storage must not report a migrated version until required migration steps complete successfully.

---

## 13.4 Unsupported Version

When a persisted representation cannot be loaded or migrated safely,

Storage returns:

```text
UnsupportedSchemaVersion
```

---

# 14. Security Contract

Storage protects persisted data within the guarantees declared by the active implementation and deployment policy.

---

## 14.1 Access Enforcement

Storage must reject persistence operations not authorized by the applicable access context.

The resulting error is:

```text
AccessDenied
```

---

## 14.2 Integrity Protection

Storage must detect corrupted or inconsistent persisted representations where integrity validation is supported by contract.

---

## 14.3 Sensitive Metadata

Storage must not expose:

- credentials;
- encryption keys;
- connection strings;
- physical storage paths;
- backend secrets.

---

## 14.4 Encryption

Encryption requirements are supplied by security or deployment policy.

The abstract Storage contract does not assume that every implementation provides identical encryption guarantees.

---

## 14.5 Secure Deletion

Secure deletion is an optional implementation capability.

It must be explicitly declared before consumers rely on it.

---

# 15. Error Contract

All public Storage operations return stable errors.

Backend-specific failures must be mapped before crossing the public boundary.

---

## 15.1 Validation Errors

```text
InvalidPersistenceRequest

InvalidPersistenceKey

InvalidPersistencePayload

InvalidRetentionInstruction

UnsupportedOperation
```

---

## 15.2 Entry Errors

```text
PersistenceEntryNotFound

PersistenceEntryAlreadyExists

PersistenceEntryArchived

PersistenceEntryDeleted
```

---

## 15.3 Version Errors

```text
PersistenceVersionConflict

UnsupportedSchemaVersion
```

---

## 15.4 Atomic Operation Errors

```text
AtomicOperationFailed

AtomicScopeUnsupported
```

---

## 15.5 Serialization Errors

```text
SerializationFailed

DeserializationFailed
```

---

## 15.6 Snapshot Errors

```text
SnapshotNotFound

SnapshotInvalid

SnapshotConflict

SnapshotRestoreFailed
```

---

## 15.7 Retention and Deletion Errors

```text
RetentionViolation

DeletionNotSupported
```

---

## 15.8 Recovery and Migration Errors

```text
RecoveryFailed

MigrationFailed
```

---

## 15.9 Availability and Security Errors

```text
StorageUnavailable

AccessDenied
```

---

## 15.10 Consistency and Internal Errors

```text
PersistenceConsistencyViolation

InternalStorageFailure
```

Detailed definitions are provided in `ERRORS.md`.

---

# 16. Idempotency

Mutation commands should support idempotency through OperationId.

---

## 16.1 Duplicate Command

When Storage receives the same OperationId with the same command content,

it must return the original logical outcome where that outcome remains available.

It must not apply the mutation again.

---

## 16.2 Operation Conflict

When the same OperationId is reused with different command content,

Storage must reject the request.

The error is:

```text
OperationIdConflict
```

---

## 16.3 Idempotency Scope

The duration and persistence of idempotency records must be declared by implementation policy.

Storage must not promise indefinite duplicate detection unless explicitly configured.

---

# 17. Concurrency

Storage supports concurrent consumers through persistence version checks and atomicity guarantees.

---

## 17.1 Concurrent Replacement

When two consumers attempt to replace the same persisted object using the same ExpectedPersistenceVersion,

at most one may succeed.

The other must receive:

```text
PersistenceVersionConflict
```

---

## 17.2 Read Consistency

A successful read must return one complete committed persisted version.

Storage must not return a partially written representation.

---

## 17.3 Conflict Resolution

Storage detects persistence conflicts.

It does not resolve business conflicts.

Conflict resolution remains with the owning module.

---

# 18. Observability

Every Storage operation should support structured observability.

---

## 18.1 Required Context

Operations should record:

```text
OperationId

CorrelationId

OperationType

ObjectType

ObjectId

PersistenceVersion

Outcome

OccurredAt
```

---

## 18.2 Sensitive Payloads

Observability records must not include complete application payloads by default.

Payload logging requires an explicit diagnostics policy.

---

## 18.3 Public Metrics

Recommended logical metrics include:

```text
storage_operation_total

storage_operation_failed_total

storage_version_conflict_total

storage_atomic_operation_failed_total

storage_snapshot_created_total

storage_snapshot_restore_failed_total

storage_migration_failed_total

storage_recovery_failed_total

storage_consistency_violation_total
```

Backend-specific operational metrics remain implementation details.

---

# 19. Compatibility

Storage contracts must remain stable across implementation changes.

---

## 19.1 Backward Compatibility

Existing consumers must not require modification when:

- a database engine changes;
- persistence moves from local to remote;
- serialization implementation changes;
- indexing strategy changes;
- repository or DAO patterns change internally.

---

## 19.2 Contract Evolution

New optional fields may be introduced when older consumers can safely ignore them.

Breaking changes require a new contract version.

---

## 19.3 Error Stability

Published error names and meanings must remain stable within the same major contract version.

---

## 19.4 Payload Compatibility

The owning module defines valid business payload evolution.

Storage manages representation compatibility through SchemaVersion and migration contracts.

---

# 20. Architecture Invariants

The following invariants always apply.

1. Storage never owns persisted business objects.
2. Storage never interprets business semantics.
3. Storage never validates business rules.
4. Persistence occurs only through explicit commands.
5. Every persisted object has an explicit PersistenceKey.
6. PersistenceId never replaces business ObjectId.
7. PersistenceVersion is managed only by Storage.
8. Business revisions are managed only by owning modules.
9. Backend-specific failures never cross the public contract.
10. Consumers never manage backend transaction handles.
11. Successful atomic operations commit every declared change.
12. Failed atomic operations expose no partial result.
13. Storage never silently downgrades requested consistency.
14. Migration never silently changes business meaning.
15. Retention policy originates outside Storage.
16. Recovery restores persistence state, not business execution.
17. Queries never create hidden persistence side effects.
18. Idempotent commands never apply the same logical mutation twice.
19. Physical implementation remains replaceable.
20. Public contracts remain independent from Repository, DAO, CQRS, or database-specific patterns.

---

# 21. Related Documents

| Document | Responsibility |
|----------|----------------|
| README.md | Storage overview |
| MODULE.md | Ownership and architectural boundaries |
| CONTRACT.md | Public persistence contracts |
| STATES.md | Storage capability lifecycle |
| EVENTS.md | Persistence lifecycle events |
| ERRORS.md | Persistence failure model |
| MODELS.md | Persistence concepts and structures |
| MIGRATION.md | Schema evolution and migration |

---

# 22. Summary

The Storage contract defines how CRAI modules persist and retrieve application-owned data without depending on physical persistence technology.

It provides contracts for:

- object persistence;
- object replacement;
- atomic persistence sets;
- retrieval;
- version control;
- snapshots;
- retention;
- archival;
- deletion;
- recovery;
- migration;
- concurrency;
- idempotency.

Storage owns persistence behavior and persistence metadata.

Originating modules retain ownership of:

- business identity;
- business state;
- business lifecycle;
- business correctness.

This separation ensures that Storage implementations can evolve without changing the application business model.

---

# End of Document