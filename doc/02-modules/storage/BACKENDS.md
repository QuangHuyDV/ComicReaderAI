# Storage Backends

- Module: Storage
- Document: BACKENDS.md
- Version: 1.0.0
- Status: Draft

---

# Purpose

This document defines the storage backend architecture supported by the Storage Module.

A backend is a physical persistence implementation. Business modules interact only with repositories and the Storage interface, never with backend-specific APIs.

---

# Design Principles

## Backend Independence

The Storage interface remains identical regardless of backend implementation.

---

## Replaceable Implementations

Changing the backend must not require changes in business modules.

---

## Capability Driven

Each backend advertises its capabilities instead of exposing implementation details.

---

# Backend Architecture

```text
Business Modules
        │
        ▼
Repositories
        │
        ▼
Storage Interface
        │
 ┌──────┼───────────────┬──────────────┬──────────────┐
 ▼      ▼               ▼              ▼
SQLite PostgreSQL   In-Memory     Local Files
                                           │
                                           ▼
                                  Cloud Object Storage
```

---

# Supported Backends

## SQLite

Recommended for:

- Local development
- Single-user desktop application
- Offline mode

Advantages:

- Zero configuration
- Lightweight
- ACID transactions
- Embedded database

Limitations:

- Limited concurrent writes
- Not intended for distributed deployments

---

## PostgreSQL

Recommended for:

- Multi-user environments
- Large datasets
- Server deployments

Advantages:

- Strong consistency
- Advanced indexing
- High concurrency
- Mature tooling

---

## In-Memory

Recommended for:

- Unit tests
- Benchmarks
- Temporary runtime data

Advantages:

- Extremely fast
- No disk access
- Easy reset

Limitations:

- Volatile
- No persistence after shutdown

---

## Local Files

Recommended for:

- Images
- Large binary assets
- Backups
- Exported data

Advantages:

- Simple storage
- Efficient for large files

Limitations:

- File management required
- Platform-specific behavior

---

## Cloud Object Storage

Examples:

- Amazon S3
- Azure Blob Storage
- Google Cloud Storage
- MinIO

Recommended for:

- Large image collections
- Backups
- Shared assets

Advantages:

- High durability
- Horizontal scalability
- Remote access

Limitations:

- Network latency
- Additional operational cost

---

# Backend Selection

Typical deployment strategies:

| Environment | Backend |
|-------------|---------|
| Development | SQLite |
| Testing | In-Memory |
| Desktop Production | SQLite |
| Server Production | PostgreSQL |
| Large Binary Assets | Local Files / Cloud Object Storage |

---

# Backend Capabilities

Typical capability flags:

- Transactions
- Batch Operations
- Streaming
- Versioning
- Backup
- Restore
- Compression
- Encryption

Business modules should rely on capabilities instead of backend names.

---

# Backend Switching

Storage should support backend replacement without changing:

- Repository interfaces
- Business modules
- Processing pipeline
- Preference resolution

Only the Storage implementation changes.

---

# Failure Handling

If a backend becomes unavailable:

- Reject affected operations
- Preserve committed data
- Attempt recovery when supported
- Publish storage failure events

---

# Future Backends

Potential future implementations:

- Redis
- RocksDB
- LMDB
- Distributed SQL databases
- Hybrid local/cloud storage

---

# Architecture Invariants

1. Business modules never communicate directly with storage engines.
2. Repository interfaces remain backend independent.
3. Backend replacement must not affect business logic.
4. Capabilities are preferred over backend-specific behavior.
5. Persisted data remains logically consistent across supported backends.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- REPOSITORIES.md
- SCHEMA.md
- CACHE.md
- MIGRATION.md
