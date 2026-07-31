# Storage Module Contract

- Module: Storage
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the public contract of the Storage Module.

Storage provides a backend-independent persistence interface for all CRAI modules. Consumers interact with repositories and storage operations rather than databases, files, or object stores.

---

# Public Commands

## Save

Persist a new object.

Result

- Object stored
- Object version assigned
- Storage metadata updated

---

## Update

Persist changes to an existing object.

Result

- Previous version replaced
- Object revision updated

---

## Delete

Remove an object.

Deletion may be:

- Soft delete
- Hard delete

depending on repository policy.

---

## BatchSave

Persist multiple objects atomically.

---

## BeginTransaction

Open a storage transaction.

---

## CommitTransaction

Commit all pending changes.

---

## RollbackTransaction

Discard all pending changes.

---

## CompactStorage

Perform backend-specific maintenance without changing logical data.

---

# Public Queries

## Get

Retrieve one object by identifier.

---

## Exists

Check whether an object exists.

---

## Query

Retrieve objects matching repository criteria.

---

## Count

Return the number of matching objects.

---

## ListRepositories

Return all available repositories.

---

## GetBackendInformation

Return backend capabilities including:

- Backend type
- Version
- Transaction support
- Migration version

---

# Repository Contracts

Storage exposes repositories rather than database tables.

Typical repositories include:

- PreferenceRepository
- SessionRepository
- OCRRepository
- TranslationRepository
- PresentationRepository
- ImageRepository
- AIMemoryRepository
- DiagnosticsRepository

Repositories own persistence access only.

---

# Transaction Contract

Transactions guarantee:

- Atomicity
- Consistency
- Isolation (backend dependent)
- Durability (persistent backends)

Nested transactions are implementation dependent.

---

# Version Contract

Persisted objects may include:

- ObjectVersion
- SchemaVersion
- CreatedAt
- UpdatedAt

Consumers must treat version metadata as read-only.

---

# Cache Contract

Storage persists cache data but does not decide cache invalidation.

Cache ownership belongs to the corresponding business module.

---

# Migration Contract

Storage supports schema migration.

Migration guarantees:

- Version detection
- Atomic migration
- Rollback on failure
- Data preservation whenever possible

---

# Backend Contract

Supported backend capabilities may include:

- SQLite
- PostgreSQL
- InMemory
- Local Files
- Cloud Object Storage

Business modules must not depend on a specific backend.

---

# Security Contract

Storage must protect persisted data through:

- Access control
- Encryption where required
- Integrity verification
- Secure deletion where supported

Sensitive information must never be exposed through repository contracts.

---

# Error Contract

Operations may return:

- StorageUnavailable
- ObjectNotFound
- DuplicateKey
- SerializationFailed
- TransactionFailed
- MigrationFailed
- PermissionDenied
- BackendFailure

Detailed definitions are available in ERRORS.md.

---

# Performance Contract

Storage should:

- Minimize unnecessary writes.
- Support efficient reads.
- Support batched operations.
- Allow streaming for large objects.
- Avoid blocking unrelated repositories.

---

# Architecture Invariants

1. Storage never executes business logic.
2. Repository contracts are backend independent.
3. Successful transactions are atomic.
4. Failed transactions leave persisted state unchanged.
5. Every persisted object belongs to exactly one repository.
6. Storage never validates business semantics.
7. Consumers never access physical backends directly.
8. Version metadata is managed only by Storage.

---

# Related Documents

- MODULE.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
- REPOSITORIES.md
- SCHEMA.md
- CACHE.md
- MIGRATION.md
- BACKENDS.md
