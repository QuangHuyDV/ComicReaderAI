# Storage Module States

- Module: Storage
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the internal state machine of the Storage Module.

Storage manages the lifecycle of persistence services, repositories, transactions, migrations and recovery while remaining independent of business logic.

---

# State Principles

## Single Active State

The Storage Module is in exactly one operational state at any time.

---

## Deterministic Transitions

The same state and input always produce the same transition.

---

## Backend Independence

State transitions describe logical storage behavior and never expose implementation-specific details.

---

## Transaction Safety

No committed data may be partially written.

---

# State Model

```text
          Initialize
               │
               ▼
         Initializing
               │
               ▼
             Ready
      ┌────────┼───────────┐
      ▼        ▼           ▼
 Transaction Migrating  Maintenance
      │        │           │
      └────────┴───────────┘
               │
               ▼
             Ready
               │
      ┌────────┴────────┐
      ▼                 ▼
   Recovering       Shutdown
      │
      ▼
    Ready / Failed
```

---

# State Summary

| State | Description |
|--------|-------------|
| Initializing | Loading repositories and storage backends |
| Ready | Accepting storage operations |
| Transaction | Executing atomic persistence operations |
| Migrating | Upgrading storage schema |
| Maintenance | Backup, restore or compaction |
| Recovering | Recovering after failure |
| Failed | Storage unavailable |
| Shutdown | Storage stopped |

---

# Initializing

## Meaning

Initialize repositories, storage engines and metadata.

## Exit Conditions

- Initialization completed
- Initialization failed

## Invariants

- Writes are rejected.
- Reads may be unavailable.

---

# Ready

## Meaning

Storage is fully operational.

## Allowed Operations

- Save
- Update
- Delete
- Query
- BeginTransaction
- Backup
- Restore
- CompactStorage

## Invariants

- Repository contracts available.
- Metadata synchronized.

---

# Transaction

## Meaning

Executing one or more atomic persistence operations.

## Exit Conditions

- Commit
- Rollback
- Failure

## Invariants

- Changes remain isolated until commit.
- Partial persistence is forbidden.

---

# Migrating

## Meaning

Schema migration is in progress.

## Exit Conditions

- Migration completed
- Migration failed

## Invariants

- Only migration operations are permitted.
- Schema consistency preserved.

---

# Maintenance

## Meaning

Administrative operations are executing.

Examples

- Backup
- Restore
- Compaction
- Verification

## Invariants

- Business semantics remain unchanged.

---

# Recovering

## Meaning

Recovering repositories after an unexpected failure.

## Exit Conditions

- Recovery succeeded
- Recovery failed

## Invariants

- Recovery is idempotent.
- Persisted data remains authoritative.

---

# Failed

## Meaning

Storage cannot safely continue.

## Allowed Operations

- RetryInitialization
- Shutdown

## Invariants

- Writes rejected.
- Existing data remains unchanged.

---

# Shutdown

## Meaning

Storage services have terminated.

## Invariants

- No further operations accepted.

---

# State Transition Table

| Current | Event | Next |
|---------|-------|------|
| Initializing | InitializationCompleted | Ready |
| Initializing | InitializationFailed | Failed |
| Ready | BeginTransaction | Transaction |
| Ready | MigrationRequested | Migrating |
| Ready | MaintenanceRequested | Maintenance |
| Ready | StorageFailure | Recovering |
| Transaction | CommitSucceeded | Ready |
| Transaction | RollbackSucceeded | Ready |
| Transaction | StorageFailure | Recovering |
| Migrating | MigrationCompleted | Ready |
| Migrating | MigrationFailed | Failed |
| Maintenance | MaintenanceCompleted | Ready |
| Maintenance | MaintenanceFailed | Failed |
| Recovering | RecoverySucceeded | Ready |
| Recovering | RecoveryFailed | Failed |
| Failed | RetryInitialization | Initializing |
| Failed | Shutdown | Shutdown |

---

# Transition Rules

## Atomic Commit

Committed transactions are durable.

---

## Rollback

Failed transactions leave persisted data unchanged.

---

## Recovery

Recovery must never duplicate committed writes.

---

## Migration

Migration either completes successfully or leaves the previous schema intact.

---

# Architecture Invariants

1. Storage is writable only in Ready and Transaction states.
2. Transactions are atomic.
3. Recovery never corrupts committed data.
4. Schema migration is exclusive.
5. Failed state rejects persistence operations.
6. Shutdown is terminal.
7. Storage lifecycle never contains business logic.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- EVENTS.md
- ERRORS.md
- README.md
- REPOSITORIES.md
- SCHEMA.md
- CACHE.md
- MIGRATION.md
- BACKENDS.md
