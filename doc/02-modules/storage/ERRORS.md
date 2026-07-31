# Storage Module Errors

- Module: Storage
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the error model of the Storage Module.

The Storage Module owns errors related to persistence, repositories, transactions, migrations, backend availability and data integrity.

Storage errors describe infrastructure failures only and never business rule violations.

---

# Error Principles

## Business Logic Independent

Storage reports persistence failures without interpreting application behavior.

---

## Stable Error Codes

Each error has a stable identifier that remains compatible across versions.

---

## Recoverability

Errors are classified as either:

- Recoverable
- Non-Recoverable

---

## Atomic Failure

Failed operations never leave partially committed data.

---

# Error Categories

## Availability Errors

### StorageUnavailable

The storage service is temporarily unavailable.

Recovery:

- Retry later.
- Switch backend if supported.

---

### BackendUnavailable

The configured backend cannot be reached.

Recovery:

- Reconnect.
- Initialize another backend.

---

## Repository Errors

### RepositoryNotFound

Requested repository does not exist.

---

### ObjectNotFound

Requested object cannot be located.

---

### DuplicateKey

An object with the same identifier already exists.

---

### InvalidIdentifier

The supplied object identifier is malformed.

---

## Transaction Errors

### TransactionAlreadyActive

A transaction is already running.

---

### TransactionNotActive

No active transaction exists.

---

### TransactionFailed

Transaction could not be committed.

Recovery:

- Roll back.
- Retry if appropriate.

---

### RollbackFailed

Rollback operation failed.

This error requires recovery.

---

## Serialization Errors

### SerializationFailed

Object could not be serialized.

---

### DeserializationFailed

Persisted object could not be reconstructed.

---

### SchemaMismatch

Persisted object does not match the expected schema.

---

## Migration Errors

### MigrationRequired

Stored schema version is outdated.

---

### MigrationFailed

Schema migration failed.

No partial migration is permitted.

---

### UnsupportedSchemaVersion

The schema version is unsupported.

---

## Persistence Errors

### WriteFailed

Object could not be written.

---

### ReadFailed

Object could not be read.

---

### DeleteFailed

Object could not be deleted.

---

### UpdateFailed

Object update failed.

---

## Integrity Errors

### IntegrityViolation

Persisted data violates integrity constraints.

---

### VersionConflict

Object version differs from expected version.

---

### ChecksumMismatch

Stored checksum validation failed.

---

## Security Errors

### PermissionDenied

Caller lacks required storage permission.

---

### EncryptionFailed

Encryption or decryption operation failed.

---

### SecureEraseFailed

Secure deletion could not be completed.

---

## Maintenance Errors

### BackupFailed

Backup operation failed.

---

### RestoreFailed

Restore operation failed.

---

### CompactionFailed

Storage compaction failed.

---

## Internal Errors

### InternalStorageError

Unexpected internal storage failure.

---

### BackendFailure

Underlying backend reported an unrecoverable failure.

---

# Error Severity

| Severity | Description |
|----------|-------------|
| Info | Informational only |
| Warning | Recoverable issue |
| Error | Operation failed |
| Critical | Storage integrity at risk |

---

# Recovery Strategy

Recoverable errors:

- StorageUnavailable
- BackendUnavailable
- TransactionFailed
- WriteFailed
- ReadFailed
- BackupFailed

Non-Recoverable errors:

- IntegrityViolation
- UnsupportedSchemaVersion
- BackendFailure
- InternalStorageError

---

# Error Reporting

Each error should include:

- ErrorCode
- Message
- Timestamp
- Repository
- Backend
- Operation
- CorrelationId (if available)

Sensitive information must never be exposed.

---

# Architecture Invariants

1. Storage errors never describe business failures.
2. Failed writes never leave partial data.
3. Migration failures never partially upgrade schema.
4. Duplicate keys never overwrite existing data automatically.
5. Internal backend details remain hidden from consumers.
6. Stable error codes are preserved across versions.
7. Recoverable errors provide deterministic recovery guidance.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- EVENTS.md
- STATES.md
- README.md
- REPOSITORIES.md
- SCHEMA.md
- CACHE.md
- MIGRATION.md
- BACKENDS.md
