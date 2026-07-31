# Storage Module

The Storage Module is the persistence layer of the CRAI architecture.

It provides a unified abstraction for storing and retrieving application data while hiding the underlying storage technology from the rest of the system. Business modules interact only with repository contracts and never communicate directly with databases or files.

---

# Responsibilities

The Storage Module is responsible for:

- Persisting application data.
- Providing repository interfaces.
- Managing transactions.
- Managing schema versions.
- Handling data migrations.
- Managing caches.
- Coordinating backup and restore operations.
- Supporting multiple storage backends.
- Preserving data integrity.

It is **not** responsible for:

- Business rule evaluation.
- OCR execution.
- Translation.
- Reading Session orchestration.
- UI rendering.
- AI processing.

---

# Position in Architecture

```text
Application Modules
        │
        ▼
 Repository Contracts
        │
        ▼
   Storage Module
        │
 ┌──────┼─────────────┐
 ▼      ▼             ▼
SQLite PostgreSQL  In-Memory
        │
        ▼
 Local Files / Cloud Storage
```

All persistence flows through the Storage Module.

---

# Repository Model

The Storage Module exposes repositories such as:

- PreferenceRepository
- SessionRepository
- OCRRepository
- TranslationRepository
- PresentationRepository
- ImageRepository
- AIMemoryRepository
- DiagnosticsRepository
- MetadataRepository

Repositories hide implementation details and provide a stable persistence API.

---

# Schema Management

The Storage Module owns the logical data schema.

It supports:

- Schema versioning
- Compatibility validation
- Incremental migrations
- Rollback strategies
- Metadata management

Schema definitions remain independent of any database engine.

---

# Cache Architecture

The module supports multiple cache layers:

```text
Memory Cache
      ↓
Persistent Cache
      ↓
Authoritative Storage
```

Typical cache types include:

- OCR Cache
- Translation Cache
- Presentation Cache
- Image Cache

---

# Transactions

The Storage Module provides:

- Atomic operations
- Commit
- Rollback
- Batch persistence
- Consistency guarantees

Business modules never manage backend transactions directly.

---

# Supported Backends

Potential storage backends include:

- SQLite
- PostgreSQL
- In-Memory
- Local Files
- Cloud Object Storage

Applications can switch backend implementations without changing business logic.

---

# Event Model

Typical consumed events:

- BackupRequested
- RestoreRequested
- MigrationRequested

Typical published events:

- StorageReady
- ObjectStored
- ObjectUpdated
- TransactionCommitted
- BackupCompleted
- MigrationCompleted
- StorageFailed

---

# State Model

Typical internal states:

- Initializing
- Ready
- Transaction
- Migrating
- Maintenance
- Recovering
- Failed
- Shutdown

---

# Error Model

Representative errors include:

- StorageUnavailable
- RepositoryNotFound
- TransactionFailed
- MigrationFailed
- SerializationFailed
- IntegrityViolation
- BackupFailed
- InternalStorageError

---

# Design Principles

## Backend Independence

Storage consumers depend on contracts, not implementations.

## Repository Pattern

Repositories own persistence access.

## Transaction Safety

Operations are atomic whenever possible.

## Schema Evolution

Data structures evolve through versioned migrations.

## Data Integrity

Consistency is maintained across supported backends.

---

# Related Documents

| Document | Description |
|----------|-------------|
| MODULE.md | Module responsibilities |
| CONTRACT.md | Public storage contracts |
| EVENTS.md | Storage events |
| STATES.md | State machine |
| ERRORS.md | Error model |
| REPOSITORIES.md | Repository architecture |
| SCHEMA.md | Logical schema |
| CACHE.md | Cache architecture |
| MIGRATION.md | Migration strategy |
| BACKENDS.md | Backend abstraction |

---

# Summary

The Storage Module provides a backend-independent persistence layer built around repositories, transactions, schema evolution and cache management. It enables every CRAI module to store and retrieve data consistently while remaining isolated from database technologies and storage implementations.
