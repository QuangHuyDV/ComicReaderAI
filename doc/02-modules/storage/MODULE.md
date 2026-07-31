# Storage Module

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

The Storage Module is the persistence capability of CRAI.

It provides durable retention and retrieval of application data through implementation-independent persistence contracts.

Storage defines how data is:

- persisted;
- loaded;
- versioned;
- retained;
- archived;
- deleted;
- recovered.

Storage does not define what persisted data means.

Business meaning remains owned by the module that created the data.

---

# 2. Architectural Role

Storage separates application ownership from persistence implementation.

```text
Business Module
      │
      │ Persistence Contract
      ▼
    Storage
      │
      │ Storage Adapter
      ▼
Persistence Implementation
```

The business module owns:

- object meaning;
- object identity;
- business lifecycle;
- validation rules;
- business correctness.

Storage owns:

- durable persistence;
- persistence versioning;
- persistence consistency;
- retention enforcement;
- persistence metadata;
- recovery support.

The persistence implementation owns:

- physical data format;
- database engine integration;
- filesystem access;
- serialization mechanism;
- backend-specific optimization.

---

# 3. Responsibilities

The Storage Module is responsible for:

- persisting application-owned data;
- loading persisted data;
- preserving persistence identity;
- maintaining persistence versions;
- supporting atomic persistence operations;
- maintaining persistence metadata;
- creating and restoring persistence snapshots;
- applying retention instructions;
- supporting archival and deletion;
- detecting persistence conflicts;
- exposing implementation-independent contracts;
- publishing persistence lifecycle events;
- reporting persistence failures.

Storage protects the durability of application state.

It does not determine whether that state is business-valid.

---

# 4. Out of Scope

Storage is not responsible for:

- creating business objects;
- validating business rules;
- controlling business lifecycles;
- interpreting object semantics;
- deciding when processing should run;
- coordinating Runtime execution;
- executing OCR;
- translating content;
- rendering presentation output;
- selecting AI behavior;
- resolving user preferences;
- determining cache invalidation policy;
- deciding data retention policy;
- defining application workflows.

Storage applies persistence instructions.

It does not invent them.

---

# 5. Ownership Model

Storage never owns the business objects it persists.

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
UserPreference
```

is owned by Preferences.

Storage may persist those objects,

but persistence does not transfer ownership.

---

## 5.1 Business Ownership

The originating module owns:

```text
Business Identity

Business State

Business Validation

Business Lifecycle

Business Invariants
```

---

## 5.2 Storage Ownership

Storage owns persistence-specific concepts:

```text
PersistenceEntry

PersistenceVersion

PersistenceSnapshot

PersistenceMetadata

RetentionInstruction

ArchivalRecord

RecoveryPoint
```

These concepts describe persistence state,

not business state.

---

# 6. Core Concepts

Storage defines a small set of persistence concepts.

```text
Storage

├── PersistenceEntry
├── PersistenceVersion
├── PersistenceSnapshot
├── PersistenceMetadata
├── RetentionInstruction
├── ArchivalRecord
└── RecoveryPoint
```

---

## 6.1 Persistence Entry

A PersistenceEntry represents one durably stored application object.

It contains enough metadata to identify and retrieve persisted content.

A PersistenceEntry does not interpret the content it stores.

---

## 6.2 Persistence Version

A PersistenceVersion identifies a specific persisted representation of an object.

It supports:

- optimistic concurrency;
- stale-write detection;
- migration;
- recovery;
- historical compatibility.

Persistence version is distinct from business revision.

For example:

```text
ContentRevision
```

is a business concept.

```text
PersistenceVersion
```

is a storage concept.

The two must never be treated as interchangeable.

---

## 6.3 Persistence Snapshot

A PersistenceSnapshot is a stable persistence representation captured at a specific point.

Snapshots may support:

- application recovery;
- offline continuation;
- synchronization;
- backup;
- migration safety.

A snapshot does not define the business truth of an object.

It preserves a persisted representation of that truth.

---

## 6.4 Persistence Metadata

PersistenceMetadata describes how an object is stored.

It may include:

```text
PersistenceId

ObjectType

ObjectId

PersistenceVersion

SchemaVersion

CreatedAt

UpdatedAt

ArchivedAt

RetentionClass
```

Metadata must not contain hidden business rules.

---

## 6.5 Retention Instruction

A RetentionInstruction describes how long persisted data should remain available.

Storage applies retention instructions supplied by the owning module or system policy.

Storage does not independently decide the business value of retained data.

---

## 6.6 Archival Record

An ArchivalRecord indicates that persisted data has been moved from active persistence into archival persistence.

Archived data remains historically available according to policy,

but may no longer participate in normal application operations.

---

## 6.7 Recovery Point

A RecoveryPoint identifies a persistence state that can be restored safely.

Recovery restores persistence state.

It does not bypass business validation after restoration.

---

# 7. Persistence Contract

Application modules interact with Storage through persistence contracts.

Conceptually:

```text
Persist

Load

Replace

Delete

Archive

CreateSnapshot

RestoreSnapshot
```

The exact public contract is defined in `CONTRACT.md`.

Storage contracts describe required persistence behavior.

They do not expose:

- database tables;
- SQL statements;
- filesystem paths;
- database drivers;
- backend connection details;
- infrastructure credentials.

---

# 8. Data Transparency

Storage treats application payloads as semantically opaque.

This means Storage may understand:

```text
ObjectType

ObjectId

PersistenceVersion

SchemaVersion

StorageMetadata
```

but must not interpret:

```text
ReadingSession state

Translation quality

OCR confidence

Presentation layout

User preference meaning
```

Storage may validate persistence structure.

It must not validate business meaning.

---

# 9. Persistence Consistency

Storage guarantees persistence consistency within the limits of its public contract.

Consistency may include:

- atomic writes;
- complete rollback;
- stale-write detection;
- version-aware replacement;
- snapshot integrity;
- deterministic object retrieval;
- durable deletion markers.

Storage does not guarantee that an object is business-correct.

That guarantee belongs to the object owner.

---

## 9.1 Atomic Operations

A persistence operation must either:

```text
Complete Fully
```

or:

```text
Leave the Previous Consistent State Unchanged
```

Partially committed persistence state is not allowed.

---

## 9.2 Version Conflicts

Storage rejects writes based on obsolete PersistenceVersion values.

The originating module must decide whether to:

- reload;
- merge;
- retry;
- discard;
- create a new business operation.

Storage reports the conflict.

It does not resolve business conflicts.

---

## 9.3 Referential Consistency

Storage may preserve declared persistence relationships.

However, the business meaning of those relationships belongs to the originating modules.

Storage must not create undocumented business coupling between persisted objects.

---

# 10. Retention and Deletion

Retention policy is defined outside Storage.

Storage executes the supplied retention instructions.

Conceptually:

```text
Active
  │
  ▼
Retained
  │
  ▼
Archived
  │
  ▼
Deleted
```

Not every stored object must follow every stage.

The owning module or system policy determines the appropriate lifecycle.

---

## 10.1 Logical Deletion

Logical deletion marks data as unavailable without immediately removing its physical representation.

It may be used for:

- recovery windows;
- synchronization safety;
- auditability;
- delayed cleanup.

---

## 10.2 Physical Deletion

Physical deletion removes persisted data from active storage.

Storage must not perform physical deletion before applicable retention rules permit it.

---

## 10.3 Secure Deletion

Where required by policy,

Storage implementations may provide stronger deletion guarantees.

Such guarantees belong to implementation-specific documentation.

They are not assumed by the abstract Storage contract.

---

# 11. Snapshot and Recovery

Storage supports persistence recovery through snapshots and recovery points.

A recovery operation may restore:

- application state;
- configuration state;
- indexed metadata;
- persistence relationships.

Recovery does not automatically reactivate business workflows.

After recovery,

the owning module must reevaluate restored business state.

---

## 11.1 Recovery Boundary

Storage recovery ends when persisted data has been restored consistently.

Any action after that boundary belongs to the relevant business or Runtime module.

For example:

```text
Storage restores ReadingSession data.

Reading Session evaluates whether the Session may resume.

Runtime decides whether execution must restart.
```

---

# 12. Schema Evolution

Persisted representation may evolve independently from business behavior.

Storage supports schema evolution through:

- schema version metadata;
- compatibility validation;
- forward migration;
- controlled fallback;
- snapshot protection;
- migration history.

Detailed migration behavior is defined in `MIGRATION.md`.

Storage migration changes persistence representation.

It must not silently change business meaning.

---

# 13. Implementation Independence

Storage contracts are independent from physical persistence technologies.

Possible implementations may use:

- relational databases;
- document databases;
- key-value stores;
- local files;
- browser storage;
- object storage;
- in-memory persistence.

These are implementation choices.

They are not part of the Storage business-facing architecture.

Changing the persistence implementation must not require changes to business modules.

---

# 14. Relationship with Reading Session

Reading Session owns:

- ReadingSession;
- ReadingContext;
- ContentRevision;
- ProcessingIntent;
- SessionConfiguration.

Storage may persist representations of these objects.

The relationship is:

```text
Reading Session
      │
      │ Persistence Request
      ▼
    Storage
      │
      │ Persistence Result
      ▼
Reading Session
```

Reading Session owns lifecycle decisions.

Storage owns durability.

Storage never:

- activates a ReadingSession;
- completes a ReadingSession;
- supersedes a ContentRevision;
- creates a ProcessingIntent;
- interprets SessionConfiguration.

---

# 15. Relationship with Runtime

Runtime may use Storage for execution-related persistence.

Examples may include:

- resumable execution metadata;
- checkpoint data;
- task recovery state;
- temporary execution artifacts.

Storage does not:

- schedule Runtime work;
- select workers;
- retry execution;
- cancel tasks;
- evaluate processing results.

Runtime owns execution.

Storage owns persistence.

---

# 16. Relationship with Processing Modules

Recognition, Translation, and Presentation may persist their own module-owned objects through Storage.

For example:

```text
Recognition owns RecognitionResult.

Translation owns TranslationResult.

Presentation owns PresentationArtifact.
```

Storage stores those objects without assuming ownership.

Storage must not create direct semantic dependencies between processing modules.

---

# 17. Relationship with Cache

Cache and durable persistence are different concerns.

```text
Cache
```

optimizes access.

```text
Storage
```

provides durability.

A cache entry may be persisted through Storage,

but Storage does not determine:

- whether caching is appropriate;
- when a cache entry becomes stale;
- whether a cached value may be reused;
- how cache keys represent business meaning.

Those decisions belong to the cache owner.

---

# 18. Events

Storage publishes persistence facts.

Examples include:

```text
ObjectPersisted

ObjectReplaced

ObjectDeleted

ObjectArchived

SnapshotCreated

SnapshotRestored

MigrationCompleted
```

Storage events describe completed persistence outcomes.

They must not describe business decisions.

Detailed event definitions are provided in `EVENTS.md`.

---

# 19. State Ownership

Storage owns its own persistence lifecycle.

Possible capability states may include:

```text
Uninitialized

Initializing

Ready

Degraded

Migrating

Recovering

Unavailable

ShuttingDown

Stopped
```

These are Storage operational states.

They are not business object states.

Detailed state definitions are provided in `STATES.md`.

---

# 20. Error Ownership

Storage owns persistence-related errors.

Examples include:

```text
StorageUnavailable

PersistenceEntryNotFound

PersistenceVersionConflict

AtomicOperationFailed

SerializationFailed

SnapshotInvalid

MigrationFailed

RetentionViolation

RecoveryFailed

PersistenceConsistencyViolation
```

Storage does not expose backend-specific failures directly to business modules.

For example:

```text
SQLiteBusy

PostgreSQLConnectionError

FilesystemPermissionDenied
```

must be mapped to stable Storage error contracts.

Detailed error definitions are provided in `ERRORS.md`.

---

# 21. Observability

Storage observability describes persistence behavior.

Recommended signals include:

```text
persistence_operation_total

persistence_operation_failed_total

persistence_version_conflict_total

snapshot_created_total

snapshot_restore_failed_total

migration_completed_total

migration_failed_total

storage_consistency_violation_total
```

Implementation metrics may additionally measure:

- backend latency;
- connection count;
- disk usage;
- query duration;
- cache hit rate.

Those implementation metrics must remain outside the public Storage contract.

---

# 22. Design Principles

---

## 22.1 Business Ownership Remains External

Storage never becomes the owner of persisted business objects.

Persistence does not imply business ownership.

---

## 22.2 Persistence Is Explicit

Application state is persisted only through explicit Storage contracts.

Hidden persistence side effects are not allowed.

---

## 22.3 Business Logic Free

Storage does not interpret business meaning.

It applies persistence instructions deterministically.

---

## 22.4 Implementation Independence

Business modules never depend on a physical storage engine.

---

## 22.5 Version Awareness

All mutable persisted representations must support deterministic version handling.

---

## 22.6 Consistency Before Optimization

Persistence correctness takes precedence over performance optimization.

---

## 22.7 Recoverability

Storage design must preserve enough metadata to support safe recovery where required.

---

## 22.8 Observable Failure

Persistence failures must be explicit.

Storage must never silently lose or partially persist data.

---

# 23. Architecture Invariants

The following invariants always apply.

1. Storage never owns business objects.
2. Storage never creates business meaning.
3. Storage never validates business rules.
4. Storage never controls business lifecycle.
5. Storage never coordinates Runtime execution.
6. Storage persists only through explicit contracts.
7. Persistence operations are atomic where promised by contract.
8. PersistenceVersion is distinct from business revision.
9. Backend-specific errors never escape public Storage contracts.
10. Persistence implementations remain replaceable.
11. Storage never performs undocumented semantic transformations.
12. Schema migration never silently changes business meaning.
13. Retention instructions originate outside Storage.
14. Recovery restores persistence state, not business execution.
15. Persisted history is never rewritten without an explicit versioned operation.

---

# 24. Related Documents

| Document | Responsibility |
|----------|----------------|
| README.md | Storage overview |
| MODULE.md | Ownership and architectural boundaries |
| CONTRACT.md | Public persistence contracts |
| STATES.md | Storage capability lifecycle |
| EVENTS.md | Persistence lifecycle events |
| ERRORS.md | Persistence failure model |
| MODELS.md | Persistence concepts and data structures |
| MIGRATION.md | Schema evolution and compatibility |

Implementation-specific backend documentation must remain outside the core Storage specification.

---

# 25. Summary

Storage is the persistence capability of CRAI.

It provides implementation-independent mechanisms for:

- durable persistence;
- retrieval;
- versioning;
- snapshots;
- retention;
- archival;
- deletion;
- recovery;
- schema evolution.

Storage does not own the application objects it persists.

Business modules own meaning and correctness.

Storage owns durability and persistence consistency.

This boundary ensures that:

- business modules remain independent from databases;
- persistence implementations remain replaceable;
- business ownership remains explicit;
- storage failures have stable contracts;
- schema evolution can occur safely;
- recovery responsibilities remain clearly separated.

---

# End of Document