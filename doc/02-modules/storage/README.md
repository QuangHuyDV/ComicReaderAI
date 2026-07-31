# Storage Module

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Overview

The Storage Module is the persistence capability of CRAI.

It provides implementation-independent mechanisms for preserving and retrieving application state without exposing databases, filesystems, browser storage or object stores to business modules.

Storage owns persistence behavior.

It does not own the business meaning of the data it stores.

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

---

# Why Storage Exists

CRAI produces and maintains data across multiple capabilities, including:

- Reading Session
- OCR
- Translation
- Presentation
- Preferences
- AI memory and context
- Diagnostics
- Application configuration

These modules must be able to persist state without depending on a physical storage technology.

Storage provides that boundary.

It ensures that:

- business modules remain independent from persistence engines;
- persistence behavior is explicit and consistent;
- stored representations can evolve safely;
- failures are reported through stable contracts;
- recovery does not become mixed with business execution.

---

# Ownership Boundary

Storage never becomes the owner of the application objects it persists.

Examples:

```text
ReadingSession      → owned by Reading Session
TranslationResult   → owned by Translation
OCRResult           → owned by OCR
UserPreference      → owned by Preferences
PresentationState   → owned by Presentation
```

The originating module owns:

- business identity;
- business state;
- business validation;
- business lifecycle;
- business invariants;
- retention intent;
- cache invalidation policy.

Storage owns:

- durable persistence;
- persistence identity;
- persistence versions;
- persistence metadata;
- atomic persistence behavior;
- snapshots and recovery points;
- archival and deletion execution;
- schema compatibility;
- persistence failure reporting.

Persistence does not transfer business ownership.

---

# Core Responsibilities

The Storage Module is responsible for:

- persisting application-owned data;
- loading persisted data;
- preserving persistence identity;
- maintaining persistence versions;
- supporting atomic persistence operations;
- maintaining persistence metadata;
- creating and restoring snapshots;
- applying externally defined retention instructions;
- supporting archival and deletion;
- detecting persistence conflicts;
- supporting schema evolution;
- publishing persistence lifecycle events;
- reporting persistence failures through stable contracts;
- hiding implementation-specific storage details.

Storage protects persistence consistency.

It does not determine whether stored data is business-correct.

---

# Out of Scope

Storage is not responsible for:

- creating business objects;
- validating business rules;
- controlling business lifecycles;
- interpreting persisted content;
- determining when application processing should run;
- coordinating Runtime execution;
- executing OCR;
- translating content;
- rendering presentation output;
- selecting AI behavior;
- resolving user preferences;
- deciding cache invalidation policy;
- defining retention policy;
- defining application workflows;
- exposing backend-specific behavior to business modules.

Storage applies explicit persistence instructions.

It does not invent business decisions.

---

# Core Persistence Concepts

Storage defines a small set of persistence-specific concepts.

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

## PersistenceEntry

Represents one durably stored application object.

It identifies persisted content without interpreting its business meaning.

## PersistenceVersion

Identifies a specific persisted representation of an object.

It supports:

- stale-write detection;
- optimistic concurrency;
- migration;
- recovery;
- historical compatibility.

A PersistenceVersion is not the same as a business revision.

## PersistenceSnapshot

Represents a consistent captured persistence state that may be used for backup, migration protection or recovery.

## PersistenceMetadata

Contains persistence-specific information such as:

- object type;
- object identifier;
- schema version;
- persistence version;
- timestamps;
- integrity metadata;
- storage status.

## RetentionInstruction

Describes how long persisted data should remain available.

The instruction originates outside Storage.

Storage only enforces it.

## ArchivalRecord

Represents persisted data that has moved out of active use while remaining available under an applicable retention policy.

## RecoveryPoint

Identifies a persistence state that may be restored safely after failure or migration.

Detailed structures are defined in `MODELS.md`.

---

# Persistence Flow

```text
Business Object
      │
      ▼
Persistence Request
      │
      ▼
Contract Validation
      │
      ▼
Version / Conflict Check
      │
      ▼
Atomic Persistence Operation
      │
      ▼
Persistence Metadata Update
      │
      ▼
Persistence Outcome Event
```

Storage validates persistence structure and contract requirements.

The originating module remains responsible for validating business meaning before requesting persistence.

---

# Consistency Model

Storage guarantees persistence consistency within the limits of its public contract.

Depending on the operation, this may include:

- atomic writes;
- complete rollback;
- deterministic retrieval;
- stale-write detection;
- version-aware replacement;
- snapshot integrity;
- durable deletion markers;
- declared referential consistency.

A persistence operation must either:

```text
Complete Fully
```

or:

```text
Leave the Previous Consistent State Unchanged
```

Partially committed persistence state is not allowed where atomicity is promised.

---

# Versioning and Conflict Handling

Mutable persisted representations use explicit persistence versions.

When a write is based on an obsolete PersistenceVersion, Storage reports a conflict.

The owning module decides whether to:

- reload;
- merge;
- retry;
- discard;
- create a new business operation.

Storage detects persistence conflicts.

It does not resolve business conflicts.

---

# Retention, Archival and Deletion

Retention policy is defined by the owning module or system policy.

Storage executes the supplied instruction.

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

Not every object must pass through every stage.

Deletion may be:

- logical;
- physical;
- secure when supported by a specific implementation.

Implementation-specific deletion guarantees remain outside the abstract Storage contract.

---

# Snapshot and Recovery

Storage supports recovery through snapshots and recovery points.

Recovery may restore:

- persisted application state;
- configuration state;
- indexed metadata;
- persistence relationships;
- version metadata.

Recovery ends when persisted data has been restored consistently.

It does not automatically resume business execution.

Example:

```text
Storage restores ReadingSession data
                │
                ▼
Reading Session evaluates restored session state
                │
                ▼
Runtime decides whether execution should resume
```

---

# Schema Evolution

Persisted representations may evolve independently from business behavior.

Storage supports:

- schema version metadata;
- compatibility validation;
- forward migration;
- controlled fallback;
- snapshot protection;
- migration history;
- rollback on failed migration.

Schema migration changes persistence representation.

It must never silently change business meaning.

Detailed migration behavior is defined in `MIGRATION.md`.

---

# Implementation Independence

Storage contracts are independent from physical persistence technologies.

Possible implementations may use:

- relational databases;
- document databases;
- key-value stores;
- browser storage;
- local files;
- object storage;
- in-memory persistence.

These are implementation choices.

They are not part of the business-facing Storage architecture.

Business modules must not:

- access physical backends directly;
- depend on database-specific types;
- handle backend-specific transactions;
- interpret backend-specific failures;
- assume a particular serialization format.

Backend-specific errors must be mapped to stable Storage errors.

---

# Event Model

Storage publishes completed persistence outcomes.

Representative events include:

- ObjectPersisted
- ObjectReplaced
- ObjectDeleted
- ObjectArchived
- SnapshotCreated
- SnapshotRestored
- MigrationCompleted
- StorageFailed

Storage events describe persistence outcomes.

They must not describe business decisions.

Detailed definitions are provided in `EVENTS.md`.

---

# State Model

Storage owns its operational lifecycle.

Representative states include:

- Uninitialized
- Initializing
- Ready
- Degraded
- Migrating
- Recovering
- Unavailable
- ShuttingDown
- Stopped

These are Storage capability states.

They are not states of persisted business objects.

Detailed definitions are provided in `STATES.md`.

---

# Error Model

Storage owns persistence-related failures.

Representative errors include:

- StorageUnavailable
- PersistenceEntryNotFound
- PersistenceVersionConflict
- AtomicOperationFailed
- SerializationFailed
- SnapshotInvalid
- MigrationFailed
- RetentionViolation
- RecoveryFailed
- PersistenceConsistencyViolation

Backend-specific errors must never escape directly through public Storage contracts.

Detailed definitions are provided in `ERRORS.md`.

---

# Design Principles

## Business Ownership Remains External

Storage never becomes the owner of persisted business objects.

## Persistence Is Explicit

Application state is persisted only through explicit Storage contracts.

Hidden persistence side effects are not allowed.

## Business Logic Free

Storage does not interpret business meaning.

## Implementation Independence

Business modules never depend on a physical storage engine.

## Version Awareness

Mutable persisted representations support deterministic version handling.

## Consistency Before Optimization

Persistence correctness takes precedence over performance optimization.

## Recoverability

Storage preserves enough metadata to support safe recovery where required.

## Observable Failure

Persistence failures are explicit.

Storage must never silently lose or partially persist data.

---

# Architecture Invariants

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

# Related Documents

| Document | Responsibility |
|----------|----------------|
| `MODULE.md` | Ownership, responsibilities and architectural boundaries |
| `CONTRACT.md` | Public persistence commands, queries and guarantees |
| `MODELS.md` | Persistence concepts and data structures |
| `STATES.md` | Storage capability lifecycle |
| `EVENTS.md` | Persistence lifecycle events |
| `ERRORS.md` | Persistence failure model |
| `MIGRATION.md` | Schema evolution and compatibility |

Implementation-specific backend documentation remains outside the core Storage specification.

---

# Summary

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

Business modules own meaning and correctness.

Storage owns durability and persistence consistency.

This boundary keeps CRAI independent from storage technologies while preserving explicit ownership, stable failure contracts and safe schema evolution.

---

# End of Document
