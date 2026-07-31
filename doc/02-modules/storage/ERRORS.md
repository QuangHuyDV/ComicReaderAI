# Storage Module Errors

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines the public error model of the Storage Module.

Storage errors describe failures related to:

- persistence requests;
- persistence identity;
- persistence versions;
- atomic persistence operations;
- serialization;
- snapshots;
- retention;
- archival;
- deletion;
- migration;
- recovery;
- capability availability;
- persistence consistency;
- access enforcement.

Storage errors describe persistence failures only.

They do not describe:

- business validation failures;
- business lifecycle violations;
- Runtime execution failures;
- OCR failures;
- translation failures;
- presentation failures;
- backend-specific implementation details.

---

# 2. Error Ownership

Storage owns an error when the primary failure concerns persistence behavior.

Examples:

```text
PersistenceEntryNotFound

PersistenceVersionConflict

SnapshotInvalid

StorageUnavailable
```

Storage does not own errors whose primary meaning belongs to another module.

For example:

```text
ReadingSessionAlreadyCompleted
```

belongs to Reading Session.

```text
TranslationLanguageUnsupported
```

belongs to Translation.

```text
RecognitionRegionInvalid
```

belongs to Recognition.

Storage may fail to persist these module-owned objects,

but it must not reinterpret their business failures as Storage errors.

---

# 3. Error Principles

---

## 3.1 Business Semantic Independence

Storage errors never evaluate application meaning.

Storage may report:

```text
PersistenceEntryNotFound
```

It must not report:

```text
ReadingSessionShouldNotExist
```

---

## 3.2 Stable Error Codes

Every public error has a stable ErrorCode.

An ErrorCode must retain the same meaning throughout a major contract version.

Message text may evolve.

Error meaning must not.

---

## 3.3 Backend Independence

Public errors must not expose:

- database engine names;
- SQL error codes;
- table names;
- repository names;
- connection strings;
- filesystem paths;
- object storage provider details;
- physical transaction identifiers;
- driver exception types.

Implementation failures must be mapped into stable Storage errors.

---

## 3.4 Explicit Recoverability

Each error declares a recoverability classification.

Possible values are:

```text
Retryable

ConditionallyRetryable

RequiresCallerAction

RequiresRecovery

NonRetryable
```

Recoverability describes persistence behavior only.

It does not decide whether the business operation should be retried.

---

## 3.5 Atomic Failure

When an operation promises atomicity,

failure must not expose a partially committed result.

If atomicity can no longer be guaranteed,

Storage must return a consistency-level error and move capability state appropriately.

---

## 3.6 No Silent Degradation

Storage must never convert a requested guarantee into a weaker guarantee without returning an explicit error.

For example:

```text
Atomic request unsupported

→ AtomicScopeUnsupported
```

Storage must not silently perform the operations non-atomically.

---

## 3.7 Deterministic Classification

The same logical failure must map to the same public ErrorCode regardless of physical implementation.

---

## 3.8 Safe Error Disclosure

Errors must provide enough information for safe handling without exposing sensitive or implementation-specific details.

---

# 4. Error Envelope

Every public Storage error uses a common structure.

```text
StorageError

├── ErrorId
├── ErrorCode
├── ErrorVersion
├── Message
├── Category
├── Severity
├── Recoverability
├── OperationType
├── OperationId
├── CorrelationId
├── Subject
├── StorageState
├── RetryAdvice
├── Details
├── CauseReference
└── OccurredAt
```

---

## 4.1 ErrorId

A unique identifier for one error occurrence.

```text
ErrorId
```

ErrorId supports:

- tracing;
- diagnostics;
- support correlation;
- duplicate reporting analysis.

---

## 4.2 ErrorCode

A stable machine-readable identifier.

Example:

```text
STORAGE_PERSISTENCE_VERSION_CONFLICT
```

Consumers must branch on ErrorCode,

not human-readable Message text.

---

## 4.3 ErrorVersion

Identifies the schema version of the error representation.

```text
ErrorVersion
```

ErrorVersion is independent from:

```text
EventVersion

PersistenceVersion

SchemaVersion
```

---

## 4.4 Message

A concise human-readable explanation.

Message text must not contain sensitive implementation details.

---

## 4.5 Category

Identifies the logical error category.

Possible values include:

```text
Validation

Identity

Entry

Version

Atomicity

Serialization

Snapshot

Retention

Deletion

Migration

Recovery

Availability

CapabilityState

Access

Consistency

Internal
```

---

## 4.6 Severity

Possible severity values are:

```text
Info

Warning

Error

Critical
```

Severity describes operational importance,

not recoverability.

---

## 4.7 Recoverability

Possible values are:

```text
Retryable

ConditionallyRetryable

RequiresCallerAction

RequiresRecovery

NonRetryable
```

---

## 4.8 OperationType

Identifies the public operation that failed.

Examples:

```text
PersistObject

ReplaceObject

PersistObjectSet

LoadObject

CreateSnapshot

RestoreSnapshot
```

---

## 4.9 OperationId

Identifies the idempotent persistence command where available.

Queries may omit OperationId.

---

## 4.10 CorrelationId

Links the error to a wider application workflow.

Storage records it without interpreting the workflow.

---

## 4.11 Subject

Identifies the logical persistence subject.

Examples:

```text
PersistenceKey

SnapshotId

MigrationId

RecoveryId

StorageCapability
```

Subject must not reveal physical implementation details.

---

## 4.12 StorageState

Identifies the logical Storage capability state when the error occurred.

Examples:

```text
Ready

Degraded

Migrating

Recovering

Failed
```

---

## 4.13 RetryAdvice

Provides bounded guidance such as:

```text
DoNotRetry

RetryWithBackoff

ReloadBeforeRetry

RetryAfterRecovery

CorrectRequestBeforeRetry

WaitForStateChange
```

RetryAdvice is advisory.

The owning module decides whether retry is business-appropriate.

---

## 4.14 Details

Contains structured, safe diagnostic context.

Examples:

```text
ExpectedPersistenceVersion

ActualPersistenceVersion

UnsupportedSchemaVersion

UnavailableOperation

RetentionConstraint
```

Details must not include raw backend exceptions.

---

## 4.15 CauseReference

An internal or diagnostics-safe reference to the underlying cause.

It must not expose confidential implementation data to normal consumers.

---

## 4.16 OccurredAt

The time when the failure became known.

---

# 5. Error Code Naming

Public ErrorCodes follow this structure:

```text
STORAGE_<CATEGORY>_<ERROR>
```

Examples:

```text
STORAGE_ENTRY_NOT_FOUND

STORAGE_VERSION_CONFLICT

STORAGE_SNAPSHOT_INVALID

STORAGE_CAPABILITY_UNAVAILABLE
```

Error names in architecture documents may use PascalCase.

Example:

```text
PersistenceEntryNotFound
```

Machine-readable form:

```text
STORAGE_ENTRY_NOT_FOUND
```

---

# 6. Validation Errors

Validation errors indicate that a persistence request is structurally invalid.

They do not represent business validation.

---

## 6.1 InvalidPersistenceRequest

### Error Code

```text
STORAGE_VALIDATION_INVALID_REQUEST
```

### Meaning

The persistence request does not satisfy the required contract structure.

### Examples

- required field missing;
- unsupported option combination;
- invalid operation structure;
- malformed atomic operation set.

### Recoverability

```text
RequiresCallerAction
```

### Retry Advice

```text
CorrectRequestBeforeRetry
```

### State Effect

No Storage capability state transition.

---

## 6.2 InvalidPersistenceKey

### Error Code

```text
STORAGE_VALIDATION_INVALID_PERSISTENCE_KEY
```

### Meaning

PersistenceKey is missing, malformed, or structurally unsupported.

### Recoverability

```text
RequiresCallerAction
```

### Does Not Mean

The business ObjectId is invalid according to business rules.

Storage validates key structure only.

---

## 6.3 InvalidPersistencePayload

### Error Code

```text
STORAGE_VALIDATION_INVALID_PAYLOAD
```

### Meaning

PersistencePayload cannot be accepted because its persistence structure is invalid.

Examples:

- payload absent where required;
- invalid ContentType declaration;
- malformed envelope;
- inconsistent SchemaVersion metadata.

### Recoverability

```text
RequiresCallerAction
```

---

## 6.4 InvalidRetentionInstruction

### Error Code

```text
STORAGE_VALIDATION_INVALID_RETENTION_INSTRUCTION
```

### Meaning

The supplied retention instruction is structurally invalid or internally contradictory.

Examples:

```text
DeleteAfter < ArchiveAfter

RetainUntil > DeleteAfter

unsupported deletion mode
```

### Recoverability

```text
RequiresCallerAction
```

---

## 6.5 UnsupportedOperation

### Error Code

```text
STORAGE_VALIDATION_UNSUPPORTED_OPERATION
```

### Meaning

The requested operation is not supported by the active Storage contract.

### Recoverability

```text
NonRetryable
```

Retry may become valid only after configuration or contract capability changes.

---

# 7. Persistence Entry Errors

---

## 7.1 PersistenceEntryNotFound

### Error Code

```text
STORAGE_ENTRY_NOT_FOUND
```

### Meaning

No matching persistence entry exists for the requested PersistenceKey.

### Typical Operations

```text
LoadObject

ReplaceObject

ArchiveObject

DeleteObject
```

### Recoverability

```text
RequiresCallerAction
```

### State Effect

No capability state transition.

### Does Not Mean

The business object must not exist.

It means Storage cannot locate the requested persisted representation.

---

## 7.2 PersistenceEntryAlreadyExists

### Error Code

```text
STORAGE_ENTRY_ALREADY_EXISTS
```

### Meaning

A create operation was requested for a PersistenceKey that already has an active persisted representation.

### Typical Operation

```text
PersistObject
```

### Recoverability

```text
RequiresCallerAction
```

### Retry Advice

The caller may:

- load the current representation;
- use ReplaceObject with an expected version;
- select a different business identity.

Storage does not choose among these actions.

---

## 7.3 PersistenceEntryArchived

### Error Code

```text
STORAGE_ENTRY_ARCHIVED
```

### Meaning

The requested entry exists but is archived and excluded from the requested active-object operation.

### Recoverability

```text
RequiresCallerAction
```

### Possible Caller Actions

- request archival access;
- restore the archived entry;
- stop the operation.

---

## 7.4 PersistenceEntryDeleted

### Error Code

```text
STORAGE_ENTRY_DELETED
```

### Meaning

The requested entry has been logically deleted and is unavailable to the requested operation.

### Recoverability

```text
RequiresCallerAction
```

The existence of a retained physical representation does not make normal access valid.

---

# 8. Persistence Version Errors

---

## 8.1 PersistenceVersionConflict

### Error Code

```text
STORAGE_VERSION_CONFLICT
```

### Meaning

ExpectedPersistenceVersion does not match the current authoritative PersistenceVersion.

### Details

```text
ExpectedPersistenceVersion

ActualPersistenceVersion

PersistenceKey
```

### Recoverability

```text
ConditionallyRetryable
```

### Retry Advice

```text
ReloadBeforeRetry
```

### State Effect

No capability state transition.

### Invariant

No mutation occurs.

---

## 8.2 UnsupportedSchemaVersion

### Error Code

```text
STORAGE_SCHEMA_VERSION_UNSUPPORTED
```

### Meaning

The persisted representation or supplied payload uses a SchemaVersion that Storage cannot safely read, write, or migrate.

### Recoverability

```text
RequiresCallerAction
```

or:

```text
RequiresRecovery
```

depending on scope.

### State Effect

For one isolated object:

```text
Ready → Ready
```

For capability-wide incompatibility:

```text
Initializing → Migrating
```

or:

```text
Initializing → Failed
```

---

## 8.3 SchemaCompatibilityViolation

### Error Code

```text
STORAGE_SCHEMA_COMPATIBILITY_VIOLATION
```

### Meaning

A requested representation change violates declared schema compatibility rules.

### Recoverability

```text
RequiresCallerAction
```

### Rule

Storage must not attempt a lossy or undefined transformation silently.

---

# 9. Atomic Operation Errors

---

## 9.1 AtomicOperationFailed

### Error Code

```text
STORAGE_ATOMIC_OPERATION_FAILED
```

### Meaning

An atomic persistence set could not be committed.

### Guarantee

No declared mutation becomes visible.

### Recoverability

```text
ConditionallyRetryable
```

### Retry Advice

Depends on the underlying public cause:

- correct invalid operation;
- reload after version conflict;
- retry after temporary availability failure.

### State Effect

Normally none.

If rollback or consistency cannot be verified,

Storage must return `PersistenceConsistencyViolation` instead or in addition through capability diagnostics.

---

## 9.2 AtomicScopeUnsupported

### Error Code

```text
STORAGE_ATOMIC_SCOPE_UNSUPPORTED
```

### Meaning

Storage cannot provide atomicity for the requested operation scope.

### Guarantee

The request is rejected before any declared mutation is applied.

### Recoverability

```text
RequiresCallerAction
```

### Rule

Storage must never silently downgrade the request to non-atomic execution.

---

## 9.3 AtomicOperationConflict

### Error Code

```text
STORAGE_ATOMIC_OPERATION_CONFLICT
```

### Meaning

One or more operations within an atomic set conflict with current persisted state.

### Details

May include a bounded list of:

```text
PersistenceKey

ConflictType

ExpectedPersistenceVersion

ActualPersistenceVersion
```

### Recoverability

```text
ConditionallyRetryable
```

---

# 10. Serialization Errors

---

## 10.1 SerializationFailed

### Error Code

```text
STORAGE_SERIALIZATION_FAILED
```

### Meaning

The supplied persistence representation could not be converted into the physical persistence form.

### Recoverability

```text
RequiresCallerAction
```

or:

```text
ConditionallyRetryable
```

depending on whether the cause is payload-specific or temporarily implementation-related.

### Rule

No partial write may remain visible.

---

## 10.2 DeserializationFailed

### Error Code

```text
STORAGE_DESERIALIZATION_FAILED
```

### Meaning

A persisted representation could not be reconstructed into the declared PersistencePayload form.

### Recoverability

```text
RequiresRecovery
```

for corrupted persisted data,

or:

```text
RequiresCallerAction
```

for unsupported requested representation.

### Possible State Effect

For isolated data:

```text
Ready → Ready
```

For systemic failure:

```text
Ready → Degraded
```

or:

```text
Ready → Recovering
```

---

## 10.3 PayloadIntegrityViolation

### Error Code

```text
STORAGE_PAYLOAD_INTEGRITY_VIOLATION
```

### Meaning

Persisted payload integrity validation failed.

Examples may include:

- checksum mismatch;
- truncated representation;
- invalid integrity marker;
- unauthorized representation modification.

### Recoverability

```text
RequiresRecovery
```

### State Effect

Depends on affected scope.

---

# 11. Snapshot Errors

---

## 11.1 SnapshotNotFound

### Error Code

```text
STORAGE_SNAPSHOT_NOT_FOUND
```

### Meaning

The requested SnapshotId does not identify an available snapshot.

### Recoverability

```text
RequiresCallerAction
```

---

## 11.2 SnapshotInvalid

### Error Code

```text
STORAGE_SNAPSHOT_INVALID
```

### Meaning

The snapshot exists but fails integrity, compatibility, or completeness validation.

### Recoverability

```text
RequiresRecovery
```

or:

```text
RequiresCallerAction
```

### Rule

An invalid snapshot must not be restored.

---

## 11.3 SnapshotConflict

### Error Code

```text
STORAGE_SNAPSHOT_CONFLICT
```

### Meaning

Restoring the snapshot would conflict with current persistence state under the declared ConflictPolicy.

### Recoverability

```text
RequiresCallerAction
```

### State Effect

No restoration occurs.

---

## 11.4 SnapshotCreationFailed

### Error Code

```text
STORAGE_SNAPSHOT_CREATION_FAILED
```

### Meaning

A consistent snapshot could not be created.

### Recoverability

```text
ConditionallyRetryable
```

### Guarantee

No incomplete snapshot is exposed as valid.

---

## 11.5 SnapshotRestoreFailed

### Error Code

```text
STORAGE_SNAPSHOT_RESTORE_FAILED
```

### Meaning

Snapshot restoration did not complete successfully.

### Recoverability

```text
RequiresRecovery
```

### Guarantee

Storage must not expose partially restored state as normal active persistence.

### Possible State Effect

```text
Recovering → Failed
```

or:

```text
Recovering → Degraded
```

when a safe restricted scope remains.

---

# 12. Retention Errors

---

## 12.1 RetentionViolation

### Error Code

```text
STORAGE_RETENTION_VIOLATION
```

### Meaning

The requested persistence operation violates an active retention constraint.

Examples:

- physical deletion before RetainUntil;
- deletion while LegalHold is active;
- archival before declared active retention ends;
- unsupported weakening of retention guarantees.

### Recoverability

```text
RequiresCallerAction
```

### State Effect

No capability state transition.

---

## 12.2 RetentionInstructionConflict

### Error Code

```text
STORAGE_RETENTION_INSTRUCTION_CONFLICT
```

### Meaning

A new retention instruction conflicts with the currently enforceable instruction or policy boundary.

### Recoverability

```text
RequiresCallerAction
```

---

## 12.3 RetentionCapabilityUnsupported

### Error Code

```text
STORAGE_RETENTION_CAPABILITY_UNSUPPORTED
```

### Meaning

The active Storage implementation cannot provide a required retention guarantee.

### Recoverability

```text
NonRetryable
```

until capability configuration or implementation changes.

### Rule

Storage must not silently accept an unenforceable instruction.

---

# 13. Archival and Deletion Errors

---

## 13.1 ArchivalNotSupported

### Error Code

```text
STORAGE_ARCHIVAL_NOT_SUPPORTED
```

### Meaning

The active persistence capability does not support the requested archival operation or scope.

### Recoverability

```text
NonRetryable
```

until capability support changes.

---

## 13.2 ArchiveRestoreConflict

### Error Code

```text
STORAGE_ARCHIVE_RESTORE_CONFLICT
```

### Meaning

An archived entry cannot be restored because an active conflicting representation exists or version conditions are not satisfied.

### Recoverability

```text
RequiresCallerAction
```

---

## 13.3 DeletionNotSupported

### Error Code

```text
STORAGE_DELETION_NOT_SUPPORTED
```

### Meaning

The requested deletion mode or guarantee is unsupported.

Examples:

- physical deletion unsupported;
- secure deletion guarantee unavailable;
- deletion of the requested persistence class prohibited.

### Recoverability

```text
NonRetryable
```

until capability or policy changes.

---

## 13.4 PhysicalDeletionFailed

### Error Code

```text
STORAGE_PHYSICAL_DELETION_FAILED
```

### Meaning

Storage could not complete the promised physical deletion operation.

### Recoverability

```text
ConditionallyRetryable
```

or:

```text
RequiresRecovery
```

depending on whether persistence state remains known.

### Rule

Storage must not report deletion success when the promised guarantee was not met.

---

# 14. Migration Errors

---

## 14.1 MigrationRequired

### Error Code

```text
STORAGE_MIGRATION_REQUIRED
```

### Meaning

The current persisted representation requires migration before the requested capability can operate safely.

### Recoverability

```text
RequiresCallerAction
```

or:

```text
RequiresRecovery
```

depending on migration orchestration.

### State Effect

May cause:

```text
Initializing → Migrating
```

or:

```text
Ready → Migrating
```

for an exclusive migration.

---

## 14.2 MigrationFailed

### Error Code

```text
STORAGE_MIGRATION_FAILED
```

### Meaning

The declared migration did not complete or pass validation.

### Recoverability

```text
RequiresRecovery
```

### Guarantee

Storage must not expose an unvalidated target representation as authoritative.

### State Effect

Possible transitions:

```text
Migrating → Recovering

Migrating → Failed
```

---

## 14.3 MigrationPathUnavailable

### Error Code

```text
STORAGE_MIGRATION_PATH_UNAVAILABLE
```

### Meaning

No supported deterministic migration path exists between source and target SchemaVersion values.

### Recoverability

```text
NonRetryable
```

until a valid migration definition is supplied.

---

## 14.4 MigrationValidationFailed

### Error Code

```text
STORAGE_MIGRATION_VALIDATION_FAILED
```

### Meaning

Migration transformation completed,

but the target representation did not satisfy required persistence validation.

### Recoverability

```text
RequiresRecovery
```

### Rule

`StorageMigrationCompleted` must not be published.

---

## 14.5 MigrationCancellationUnsafe

### Error Code

```text
STORAGE_MIGRATION_CANCELLATION_UNSAFE
```

### Meaning

Migration cancellation was requested after the operation crossed a boundary where immediate cancellation could violate consistency.

### Recoverability

```text
RequiresCallerAction
```

Storage must continue to a safe boundary before stopping.

---

# 15. Recovery Errors

---

## 15.1 RecoveryFailed

### Error Code

```text
STORAGE_RECOVERY_FAILED
```

### Meaning

Storage could not restore a consistent persisted state.

### Recoverability

```text
RequiresRecovery
```

or:

```text
NonRetryable
```

when no valid recovery strategy remains.

### State Effect

```text
Recovering → Failed
```

---

## 15.2 RecoverySourceUnavailable

### Error Code

```text
STORAGE_RECOVERY_SOURCE_UNAVAILABLE
```

### Meaning

The declared snapshot, recovery point, backup representation, or journal required for recovery is unavailable.

### Recoverability

```text
ConditionallyRetryable
```

or:

```text
RequiresCallerAction
```

---

## 15.3 RecoverySourceInvalid

### Error Code

```text
STORAGE_RECOVERY_SOURCE_INVALID
```

### Meaning

The selected recovery source fails integrity or compatibility validation.

### Recoverability

```text
RequiresCallerAction
```

A different recovery source is required.

---

## 15.4 RecoveryConflict

### Error Code

```text
STORAGE_RECOVERY_CONFLICT
```

### Meaning

Recovery cannot proceed under the selected restore or conflict policy.

### Recoverability

```text
RequiresCallerAction
```

---

## 15.5 RecoveryValidationFailed

### Error Code

```text
STORAGE_RECOVERY_VALIDATION_FAILED
```

### Meaning

Restored persistence state did not pass required integrity validation.

### Recoverability

```text
RequiresRecovery
```

### Rule

Storage must not transition to Ready.

---

## 15.6 RecoveryCancellationUnsafe

### Error Code

```text
STORAGE_RECOVERY_CANCELLATION_UNSAFE
```

### Meaning

Recovery cannot stop immediately without risking an inconsistent persistence boundary.

### Recoverability

```text
RequiresCallerAction
```

---

# 16. Availability Errors

---

## 16.1 StorageUnavailable

### Error Code

```text
STORAGE_CAPABILITY_UNAVAILABLE
```

### Meaning

Storage cannot currently provide the requested persistence capability safely.

### Typical States

```text
Failed

Recovering

Unavailable capability scope
```

### Recoverability

```text
Retryable
```

or:

```text
RequiresRecovery
```

depending on RetryAdvice.

### Retry Advice

Possible values:

```text
RetryWithBackoff

RetryAfterRecovery

WaitForStateChange

DoNotRetry
```

### Rule

The error must not tell consumers to switch database or backend.

Implementation selection remains outside the public contract.

---

## 16.2 StorageOperationUnavailable

### Error Code

```text
STORAGE_OPERATION_UNAVAILABLE
```

### Meaning

Storage remains available,

but the requested operation or scope is unavailable in the current capability state.

### Typical State

```text
Degraded
```

### Details

```text
UnavailableOperation

AffectedScope

AvailableAlternatives
```

### Recoverability

```text
ConditionallyRetryable
```

---

## 16.3 StorageTimeout

### Error Code

```text
STORAGE_OPERATION_TIMEOUT
```

### Meaning

The operation did not complete within the declared persistence boundary.

### Recoverability

```text
ConditionallyRetryable
```

### Critical Rule

Timeout does not automatically mean the operation failed before commitment.

The error must indicate one of:

```text
OutcomeKnownNotCommitted

OutcomeKnownCommitted

OutcomeUnknown
```

When outcome is unknown,

the caller must use OperationId or query authoritative state before retrying.

---

## 16.4 StorageCapacityExceeded

### Error Code

```text
STORAGE_CAPACITY_EXCEEDED
```

### Meaning

Storage cannot accept the requested persistence operation because a declared capacity limit has been reached.

### Recoverability

```text
ConditionallyRetryable
```

or:

```text
RequiresCallerAction
```

### Rule

Storage must not silently discard or truncate payloads.

---

# 17. Capability State Errors

These errors indicate that an operation is invalid because of the current Storage capability state.

---

## 17.1 StorageNotInitialized

### Error Code

```text
STORAGE_STATE_NOT_INITIALIZED
```

### Meaning

The operation was requested while Storage is Uninitialized.

### Recoverability

```text
ConditionallyRetryable
```

### Retry Advice

```text
WaitForStateChange
```

---

## 17.2 StorageInitializing

### Error Code

```text
STORAGE_STATE_INITIALIZING
```

### Meaning

The operation was requested while Storage initialization is incomplete.

### Recoverability

```text
Retryable
```

---

## 17.3 StorageMigrating

### Error Code

```text
STORAGE_STATE_MIGRATING
```

### Meaning

The requested operation is unavailable during an exclusive migration.

### Recoverability

```text
Retryable
```

### Retry Advice

```text
WaitForStateChange
```

---

## 17.4 StorageRecovering

### Error Code

```text
STORAGE_STATE_RECOVERING
```

### Meaning

The requested operation is unavailable while Storage restores consistent persistence state.

### Recoverability

```text
Retryable
```

---

## 17.5 StorageShuttingDown

### Error Code

```text
STORAGE_STATE_SHUTTING_DOWN
```

### Meaning

Storage has begun safe termination and no longer accepts the requested operation.

### Recoverability

```text
NonRetryable
```

for the current capability instance.

---

## 17.6 StorageStopped

### Error Code

```text
STORAGE_STATE_STOPPED
```

### Meaning

The operation was requested against a terminal Storage capability instance.

### Recoverability

```text
NonRetryable
```

A new capability instance is required.

---

# 18. Access and Security Errors

---

## 18.1 AccessDenied

### Error Code

```text
STORAGE_ACCESS_DENIED
```

### Meaning

The active access context is not authorized to perform the requested persistence operation.

### Recoverability

```text
RequiresCallerAction
```

### Rule

The error must not reveal whether protected data exists unless the access policy permits that disclosure.

---

## 18.2 IntegrityProtectionUnavailable

### Error Code

```text
STORAGE_INTEGRITY_PROTECTION_UNAVAILABLE
```

### Meaning

A required persistence integrity guarantee cannot be provided.

### Recoverability

```text
NonRetryable
```

until capability configuration changes.

### Rule

Storage must reject the operation rather than persist without the required guarantee.

---

## 18.3 EncryptionRequirementUnsupported

### Error Code

```text
STORAGE_ENCRYPTION_REQUIREMENT_UNSUPPORTED
```

### Meaning

The active implementation cannot satisfy the required persistence encryption policy.

### Recoverability

```text
NonRetryable
```

until deployment or capability configuration changes.

---

## 18.4 SecureDeletionRequirementUnsupported

### Error Code

```text
STORAGE_SECURE_DELETION_UNSUPPORTED
```

### Meaning

The requested secure deletion guarantee cannot be provided.

### Recoverability

```text
NonRetryable
```

### Rule

Storage must not report ordinary physical deletion as secure erasure.

---

# 19. Consistency Errors

---

## 19.1 PersistenceConsistencyViolation

### Error Code

```text
STORAGE_CONSISTENCY_VIOLATION
```

### Meaning

Storage detected that a declared persistence consistency guarantee has been or may have been violated.

Examples:

- atomic outcome uncertain;
- conflicting authoritative versions;
- partial state exposure detected;
- recovery boundary invalid;
- committed metadata and payload disagree.

### Severity

```text
Critical
```

### Recoverability

```text
RequiresRecovery
```

### State Effect

Normally:

```text
Ready → Recovering
```

or:

```text
Ready → Failed
```

A restricted unaffected scope may allow:

```text
Ready → Degraded
```

---

## 19.2 PersistenceIdentityConflict

### Error Code

```text
STORAGE_IDENTITY_CONFLICT
```

### Meaning

Persistence identity metadata resolves inconsistently.

Examples:

- one PersistenceId maps to multiple incompatible PersistenceKeys;
- one active key maps to conflicting authoritative entries.

### Severity

```text
Critical
```

### Recoverability

```text
RequiresRecovery
```

---

## 19.3 PersistenceMetadataCorrupted

### Error Code

```text
STORAGE_METADATA_CORRUPTED
```

### Meaning

Storage-owned metadata fails structural or integrity validation.

### Recoverability

```text
RequiresRecovery
```

### Possible State Effect

```text
Initializing → Recovering

Ready → Recovering

Ready → Failed
```

---

## 19.4 EventIdentityConflict

### Error Code

```text
STORAGE_EVENT_IDENTITY_CONFLICT
```

### Meaning

The same EventId has been associated with conflicting event content.

### Severity

```text
Critical
```

### Recoverability

```text
RequiresRecovery
```

or operational investigation.

### Rule

Storage must not publish the conflicting event as a valid duplicate.

---

# 20. Idempotency Errors

---

## 20.1 OperationIdConflict

### Error Code

```text
STORAGE_OPERATION_ID_CONFLICT
```

### Meaning

An OperationId was reused with command content different from the original command.

### Recoverability

```text
RequiresCallerAction
```

### Guarantee

The conflicting command is not applied.

---

## 20.2 IdempotencyRecordUnavailable

### Error Code

```text
STORAGE_IDEMPOTENCY_RECORD_UNAVAILABLE
```

### Meaning

Storage cannot verify whether the supplied OperationId has already been processed.

### Recoverability

```text
ConditionallyRetryable
```

or:

```text
RequiresRecovery
```

depending on scope.

### Rule

Storage must not blindly repeat a mutation when duplicate safety is required.

---

# 21. Internal Errors

---

## 21.1 InternalStorageFailure

### Error Code

```text
STORAGE_INTERNAL_FAILURE
```

### Meaning

An unexpected internal persistence failure occurred that cannot be represented by a more specific public error.

### Recoverability

```text
ConditionallyRetryable
```

or:

```text
RequiresRecovery
```

depending on RetryAdvice.

### Rule

This is a last-resort public mapping.

Specific errors must be preferred whenever possible.

### Disclosure

The public error must not expose:

- exception stack traces;
- database messages;
- driver error codes;
- physical resource identifiers;
- internal credentials.

---

## 21.2 StorageInvariantViolation

### Error Code

```text
STORAGE_INTERNAL_INVARIANT_VIOLATION
```

### Meaning

Storage detected that one of its own architecture or runtime invariants was violated.

### Severity

```text
Critical
```

### Recoverability

```text
RequiresRecovery
```

### Possible State Effect

```text
Any Active State → Failed
```

or:

```text
Any Active State → Recovering
```

---

# 22. Error Severity

| Severity | Meaning |
|---|---|
| Info | No operation failure; informational condition only |
| Warning | Operation or capability is restricted but consistency remains safe |
| Error | One requested operation failed |
| Critical | Persistence integrity or capability safety is at risk |

Severity and recoverability are independent.

Example:

```text
PersistenceVersionConflict

Severity = Error

Recoverability = ConditionallyRetryable
```

Example:

```text
PersistenceConsistencyViolation

Severity = Critical

Recoverability = RequiresRecovery
```

---

# 23. Recoverability Model

---

## 23.1 Retryable

The same logical request may succeed later without changing its business or persistence content.

Examples:

```text
StorageInitializing

StorageRecovering

temporary StorageUnavailable
```

Retry should use bounded backoff.

---

## 23.2 ConditionallyRetryable

Retry is safe only after a declared condition is satisfied.

Examples:

```text
PersistenceVersionConflict

StorageTimeout

AtomicOperationFailed
```

The condition may require:

- reloading authoritative state;
- checking OperationId;
- waiting for capability recovery;
- resolving a conflict.

---

## 23.3 RequiresCallerAction

The caller must change the request or choose a different application action.

Examples:

```text
InvalidPersistenceRequest

PersistenceEntryAlreadyExists

RetentionViolation

AccessDenied
```

---

## 23.4 RequiresRecovery

Storage consistency or integrity must be restored before normal operation can continue safely.

Examples:

```text
PersistenceConsistencyViolation

RecoveryValidationFailed

PersistenceMetadataCorrupted
```

---

## 23.5 NonRetryable

The request cannot succeed under the current contract or capability configuration.

Examples:

```text
UnsupportedOperation

MigrationPathUnavailable

StorageStopped

SecureDeletionRequirementUnsupported
```

---

# 24. Retry Rules

---

## 24.1 Retry Requires Idempotency Awareness

Mutation commands must not be retried blindly.

The caller should reuse the original:

```text
OperationId
```

when retrying the same logical command.

---

## 24.2 Version Conflict Retry

After:

```text
PersistenceVersionConflict
```

the caller must load current persisted state before issuing a replacement with a new ExpectedPersistenceVersion.

---

## 24.3 Timeout Retry

After:

```text
StorageTimeout
```

the caller must inspect the declared outcome status.

For:

```text
OutcomeUnknown
```

the caller must query by OperationId or authoritative PersistenceKey before retrying.

---

## 24.4 Capability State Retry

For state errors such as:

```text
StorageInitializing

StorageMigrating

StorageRecovering
```

the caller should wait for a state change rather than repeatedly issuing immediate requests.

---

## 24.5 Nonretryable Errors

Errors classified as NonRetryable must not be retried unchanged.

---

# 25. Error-to-State Relationship

A command error does not automatically change Storage capability state.

---

## 25.1 Isolated Operation Errors

These normally leave Storage in its current safe state:

```text
InvalidPersistenceRequest

PersistenceEntryNotFound

PersistenceEntryAlreadyExists

PersistenceVersionConflict

RetentionViolation

AccessDenied
```

Example:

```text
Ready

↓

ReplaceObject returns PersistenceVersionConflict

↓

Ready
```

---

## 25.2 Capability-Level Errors

These may change Storage state:

```text
PersistenceConsistencyViolation

PersistenceMetadataCorrupted

StorageInvariantViolation

RecoveryFailed

MigrationFailed
```

Possible transitions:

```text
Ready → Degraded

Ready → Recovering

Ready → Failed
```

---

## 25.3 Failed State Boundary

Storage enters Failed only when no safe supported persistence capability remains.

An individual write failure is insufficient.

---

# 26. Error-to-Event Relationship

Errors are returned directly through operation contracts.

Events are published only when the failure is an architecturally significant fact.

Example:

```text
ReplaceObject

↓

PersistenceVersionConflict
```

Normally no public failure event is required.

Example:

```text
PersistenceConsistencyViolation
```

may produce:

```text
StorageConsistencyViolationDetected
```

and then:

```text
StorageRecoveryStarted
```

A failed operation must never publish a corresponding success event.

---

# 27. Error Mapping

Implementation-specific failures must be mapped into public errors.

Examples:

```text
Database unique constraint failure

↓

PersistenceEntryAlreadyExists
```

```text
Driver connection failure

↓

StorageUnavailable
```

```text
Serialization library exception

↓

SerializationFailed
```

```text
Checksum implementation mismatch

↓

PayloadIntegrityViolation
```

The public mapping must be based on logical meaning,

not implementation vocabulary.

---

# 28. Unknown Outcomes

Some failures may occur after Storage has attempted a mutation but before the caller receives a definitive result.

These cases must be represented explicitly.

---

## 28.1 OutcomeKnownNotCommitted

Storage confirms that no mutation became visible.

A retry may be safe according to normal rules.

---

## 28.2 OutcomeKnownCommitted

Storage confirms the mutation committed even though response delivery failed.

The caller must not issue the command again with a new OperationId.

---

## 28.3 OutcomeUnknown

Storage cannot currently determine whether commitment occurred.

The caller must not blindly retry.

It must first use:

- OperationId lookup;
- PersistenceKey loading;
- recovery status;
- capability diagnostics.

Storage should enter Recovering when unknown outcome indicates possible consistency risk.

---

# 29. Error Reporting and Observability

Every error occurrence should record:

```text
ErrorId

ErrorCode

ErrorVersion

Category

Severity

Recoverability

OperationType

OperationId

CorrelationId

SubjectType

StorageState

OccurredAt

OutcomeStatus
```

Internal diagnostics may additionally record implementation-specific causes.

Those details must remain behind the public boundary.

---

# 30. Sensitive Information Rules

Public Storage errors must never expose:

- credentials;
- access tokens;
- encryption keys;
- connection strings;
- database hostnames;
- database names;
- table names;
- raw SQL;
- filesystem paths;
- object storage bucket names;
- raw persisted payloads;
- protected business content;
- full internal stack traces.

Sensitive ObjectId values should be redacted or represented indirectly when policy requires.

---

# 31. Error Metrics

Recommended logical metrics include:

```text
storage_error_total

storage_validation_error_total

storage_entry_not_found_total

storage_version_conflict_total

storage_atomic_operation_failed_total

storage_serialization_failed_total

storage_snapshot_failed_total

storage_retention_violation_total

storage_migration_failed_total

storage_recovery_failed_total

storage_unavailable_total

storage_consistency_violation_total

storage_internal_failure_total
```

Metrics should include bounded labels such as:

```text
ErrorCode

OperationType

StorageState

Recoverability
```

High-cardinality identifiers such as ObjectId and ErrorId should not be used as metric labels.

---

# 32. Compatibility

---

## 32.1 Stable Meaning

An existing ErrorCode must not change meaning within the same major contract version.

---

## 32.2 New Errors

New ErrorCodes may be added when consumers safely support unknown error handling.

---

## 32.3 Deprecated Errors

Deprecated ErrorCodes must remain documented until the next breaking contract version.

---

## 32.4 Message Compatibility

Consumers must never depend on exact Message text.

---

## 32.5 Category Compatibility

Changing an error to a different category, severity, or recoverability classification may be breaking when consumers depend on that behavior.

Such changes require explicit version review.

---

# 33. Error Handling Rules for Consumers

Consumers must:

1. Branch on ErrorCode rather than Message.
2. Respect Recoverability and RetryAdvice.
3. Reuse OperationId when retrying the same logical mutation.
4. Reload authoritative state after PersistenceVersionConflict.
5. Never infer business meaning from a Storage error.
6. Never expose internal error details directly to end users.
7. Treat unknown Critical errors as unsafe.
8. Avoid immediate repeated retries during migration or recovery.
9. Verify outcome before retrying unknown-result mutations.
10. Preserve CorrelationId when propagating failures across module boundaries.

---

# 34. Architecture Invariants

The following invariants always apply.

1. Storage errors describe persistence failures only.
2. Storage errors never describe business rule violations.
3. Public error codes remain backend-independent.
4. Repository-specific errors never cross the public boundary.
5. Database-specific errors never cross the public boundary.
6. Failed atomic operations expose no partial result.
7. PersistenceVersionConflict never applies a mutation.
8. Duplicate create operations never overwrite existing data automatically.
9. Unsupported guarantees are rejected explicitly.
10. Storage never silently weakens atomicity, retention, encryption, or deletion guarantees.
11. Routine command errors do not automatically fail the Storage capability.
12. Capability failure is reserved for unsafe persistence conditions.
13. Error recoverability is explicit.
14. Retried mutations preserve OperationId.
15. Unknown mutation outcomes are never treated as confirmed failures.
16. Migration failures never expose an unvalidated target representation.
17. Recovery failures never expose partially restored state as normal persistence.
18. Sensitive implementation details never appear in public errors.
19. ErrorCode meaning remains stable within a major version.
20. Message text is never used as a machine contract.
21. Errors do not publish success events.
22. Backend replacement does not change logical error meaning.
23. Storage state errors correspond to the state model in `STATES.md`.
24. Error names align with operations defined in `CONTRACT.md`.
25. Capability-level failures align with events defined in `EVENTS.md`.

---

# 35. Related Documents

| Document | Responsibility |
|---|---|
| README.md | Storage overview |
| MODULE.md | Ownership and architectural boundaries |
| CONTRACT.md | Persistence commands, queries and guarantees |
| STATES.md | Storage capability lifecycle |
| EVENTS.md | Persistence and capability events |
| ERRORS.md | Public persistence error model |
| MODELS.md | Persistence concepts and structures |
| MIGRATION.md | Schema evolution and migration failures |

---

# 36. Summary

The Storage error model defines stable, implementation-independent failures for CRAI persistence operations.

It covers:

- invalid persistence requests;
- persistence entry conflicts;
- version conflicts;
- atomic operation failures;
- serialization failures;
- snapshot failures;
- retention violations;
- archival and deletion failures;
- migration failures;
- recovery failures;
- capability availability;
- capability state restrictions;
- access enforcement;
- persistence consistency;
- internal Storage failures.

Storage errors never represent:

- business validation;
- business lifecycle;
- Runtime execution;
- backend implementation details;
- repository mechanics.

Errors report failed operations.

Events report completed facts.

States describe capability availability.

Contracts define accepted behavior.

This separation keeps persistence failure handling stable even when Storage implementation changes.

---

# End of Document