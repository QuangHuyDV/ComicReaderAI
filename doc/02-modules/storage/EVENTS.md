# Storage Module Events

- Module: Storage
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the events consumed and published by the Storage Module.

Storage communicates persistence lifecycle changes without exposing backend-specific implementation details.

---

# Event Principles

## Event-Driven Persistence

Storage reports persistence operations through immutable events.

---

## Immutable Events

Published events must never be modified after publication.

---

## Backend Independent

Events describe logical persistence operations rather than SQL, files or storage engines.

---

## Version Awareness

Every event includes:

- EventVersion
- ObjectVersion (when applicable)
- SchemaVersion
- Timestamp

---

# Event Naming Convention

Events use the past-tense convention.

Examples:

```text
ObjectStored
ObjectUpdated
ObjectDeleted
TransactionCommitted
MigrationCompleted
```

---

# Consumed Events

## ApplicationStarted

Purpose

Initialize storage backends and repositories.

---

## ApplicationShutdownRequested

Purpose

Flush pending writes and release resources.

---

## BackupRequested

Purpose

Create a storage backup.

---

## RestoreRequested

Purpose

Restore persisted data from backup.

---

## MigrationRequested

Purpose

Upgrade persisted schema.

---

# Published Events

## StorageReady

Purpose

Storage initialization completed successfully.

---

## ObjectStored

Purpose

A new object has been persisted.

---

## ObjectUpdated

Purpose

An existing object has been updated.

---

## ObjectDeleted

Purpose

An object has been removed.

---

## TransactionStarted

Purpose

A storage transaction has begun.

---

## TransactionCommitted

Purpose

All transaction changes were committed successfully.

---

## TransactionRolledBack

Purpose

Transaction aborted and no changes were persisted.

---

## StorageCompacted

Purpose

Backend maintenance completed.

---

## BackupCompleted

Purpose

Storage backup completed successfully.

---

## RestoreCompleted

Purpose

Storage restore completed successfully.

---

## MigrationCompleted

Purpose

Storage schema migration completed.

---

## StorageFailed

Purpose

An unrecoverable storage failure occurred.

---

# Event Ordering

Normal write sequence

```text
TransactionStarted
        ↓
ObjectStored / ObjectUpdated / ObjectDeleted
        ↓
TransactionCommitted
```

Migration sequence

```text
MigrationRequested
        ↓
MigrationCompleted
```

Backup sequence

```text
BackupRequested
        ↓
BackupCompleted
```

---

# Event Ordering Rules

1. TransactionStarted always precedes TransactionCommitted.
2. Object events occur within an active transaction when transactions are used.
3. TransactionRolledBack never follows TransactionCommitted for the same transaction.
4. MigrationCompleted is published only after a successful migration.
5. Failed operations must not publish success events.

---

# Event Idempotency

Consumers should ignore duplicate events identified by:

- EventId
- ObjectVersion
- TransactionId (when present)

---

# Event Delivery

Recommended guarantees:

- At-least-once delivery.
- Ordered within the same transaction.
- Immutable after publication.

---

# Event Failure Handling

If event publication fails:

- Persisted data remains authoritative.
- Event delivery may be retried.
- Duplicate delivery must be tolerated.

---

# Event Dependencies

```text
Business Module
        │
        ▼
Storage
        │
        ▼
Persistence Event
        │
        ▼
Diagnostics / Observability
```

Storage events do not carry business semantics.

---

# Architecture Invariants

1. Events describe logical persistence operations only.
2. Success events are published only after successful persistence.
3. Events are immutable.
4. Duplicate delivery must not corrupt state.
5. Storage events never expose backend-specific details.

---

# Future Events

Potential future events include:

- RepositoryRegistered
- RepositoryRemoved
- CachePersisted
- CacheEvicted
- StorageRecovered
- StorageHealthChanged

---

# Related Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- ERRORS.md
- README.md
- REPOSITORIES.md
- SCHEMA.md
- CACHE.md
- MIGRATION.md
- BACKENDS.md
