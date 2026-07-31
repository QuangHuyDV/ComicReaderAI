# Storage Module Migration

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Document: MIGRATION.md
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines how the Storage Module evolves persisted representations while preserving:

- persistence integrity;
- persistence identity;
- version correctness;
- recoverability;
- compatibility;
- business ownership boundaries.

Migration allows Storage-owned persistence structures and externally owned persisted representations to evolve across application versions.

Migration changes persistence representation.

It does not automatically change business meaning.

---

# 2. Migration Boundary

Storage migration may transform:

- PersistenceEntry representation;
- PersistenceMetadata representation;
- PersistenceSnapshot representation;
- migration records;
- recovery metadata;
- retention metadata;
- declared serialized payload structure;
- physical persistence representation.

Storage migration must not independently transform:

- Reading Session business state;
- translation meaning;
- recognition meaning;
- presentation semantics;
- user preferences semantics;
- workflow intent;
- Runtime execution state.

Any transformation that changes business meaning must be defined and owned by the relevant business module.

---

# 3. Migration Ownership

---

## 3.1 Storage-Owned Migration

Storage owns migration of:

```text
PersistenceMetadata

PersistenceVersion metadata

Snapshot manifests

Retention records

Archival records

Recovery records

Migration history
```

Storage may define these migrations independently because it owns these concepts.

---

## 3.2 Module-Owned Payload Migration

A business module owns migration of its persisted payload semantics.

For example:

```text
ReadingSession payload migration
```

is defined by Reading Session.

```text
TranslationResult payload migration
```

is defined by Translation.

Storage may execute the supplied transformation,

but it does not author or infer its semantic meaning.

---

## 3.3 Implementation Migration

Moving data between Storage implementations is an implementation migration.

Examples include:

```text
Local persistence → Remote persistence

Single-node persistence → Distributed persistence

Filesystem-backed representation → Database-backed representation
```

Public architecture does not identify concrete technologies.

The migration must preserve the same Storage contract.

---

# 4. Migration Principles

---

## 4.1 Meaning Preservation

Migration must preserve business meaning unless an explicit business-owned semantic transformation is supplied.

Storage must never silently reinterpret data.

---

## 4.2 Deterministic Transformation

Given the same:

- source representation;
- source SchemaVersion;
- migration definition;
- target SchemaVersion;

the migration must produce the same logical target result.

---

## 4.3 Explicit Version Boundaries

Every migration declares:

```text
SourceSchemaVersion

TargetSchemaVersion
```

A migration must not infer target version from application build number alone.

---

## 4.4 Forward Safety

Unsupported future SchemaVersion values must be rejected.

Storage must not attempt best-effort interpretation of unknown future data.

---

## 4.5 Backward Compatibility

Where supported,

newer Storage versions should read or migrate older persisted representations.

Compatibility support must be explicit.

---

## 4.6 Idempotent Execution

Re-running the same migration against the same already migrated scope must not produce a different logical result.

---

## 4.7 Recoverability

Migration must preserve a safe recovery path whenever the migration mode promises recoverability.

---

## 4.8 No Partial Visibility

Unvalidated target representation must not become visible as authoritative persisted state.

---

## 4.9 Implementation Independence

Migration contracts must not expose:

- SQL migration syntax;
- database engine names;
- table names;
- repository names;
- filesystem commands;
- driver-specific behavior.

---

## 4.10 Observable Progress

Long-running migrations must expose logical progress without revealing physical backend details.

---

# 5. Version Model

Storage uses independent version dimensions.

```text
StorageContractVersion

SchemaVersion

MigrationDefinitionVersion

MigrationRecordVersion

BusinessRevision
```

---

## 5.1 StorageContractVersion

Identifies the version of the public Storage contract.

Example:

```text
2.0.0
```

This version describes public behavior,

not persisted object format.

---

## 5.2 SchemaVersion

Identifies the persisted representation format.

```text
SchemaVersion
```

SchemaVersion may apply to:

- one object type;
- one persistence namespace;
- one Storage-owned model;
- an application persistence boundary.

---

## 5.3 MigrationDefinitionVersion

Identifies the version of the migration transformation definition.

This allows migration logic to evolve while preserving auditability.

---

## 5.4 MigrationRecordVersion

Identifies the schema version of the MigrationRecord itself.

---

## 5.5 BusinessRevision

BusinessRevision belongs to the originating module.

```text
BusinessRevision != SchemaVersion
```

and:

```text
BusinessRevision != PersistenceVersion
```

---

# 6. Migration Identity

Every migration has a stable MigrationId.

```text
MigrationId
```

A migration identity must uniquely represent:

- migration scope;
- source version;
- target version;
- migration definition.

The same MigrationId must never refer to different transformation logic.

---

# 7. Migration Scope

MigrationScope defines the persistence boundary affected by migration.

```text
MigrationScope

├── ScopeType
└── ScopeSelector
```

Possible scope types include:

```text
SingleObject

ObjectSet

ObjectType

ModuleNamespace

StorageOwnedMetadata

SnapshotSet

ApplicationPersistenceBoundary
```

---

## 7.1 SingleObject

Migrates one PersistenceEntry.

---

## 7.2 ObjectSet

Migrates a declared bounded set of PersistenceEntry objects.

---

## 7.3 ObjectType

Migrates all persisted representations for one declared ObjectType.

---

## 7.4 ModuleNamespace

Migrates a persistence namespace owned by one module.

---

## 7.5 StorageOwnedMetadata

Migrates models owned directly by Storage.

---

## 7.6 ApplicationPersistenceBoundary

Migrates the complete declared persistence boundary.

This normally requires exclusive migration.

---

# 8. Migration Modes

Storage supports three logical migration modes.

```text
Lazy

Eager

Exclusive
```

---

## 8.1 Lazy Migration

Lazy migration occurs when an object is accessed.

Example:

```text
LoadObject

↓

Old SchemaVersion detected

↓

Object representation migrated

↓

Object returned or replaced
```

### Suitable For

- independent object representations;
- deterministic transformations;
- low-risk schema changes;
- migrations that do not require global coordination.

### Capability State

Storage may remain:

```text
Ready
```

### Requirements

- concurrent access remains safe;
- target representation is validated;
- stale writes are prevented;
- failed migration does not corrupt source representation.

---

## 8.2 Eager Migration

Eager migration processes a declared scope before normal use of that scope.

### Suitable For

- bounded object sets;
- planned application upgrades;
- transformations that should complete proactively;
- migrations where mixed versions are undesirable but manageable.

### Capability State

Storage may remain:

```text
Ready
```

or:

```text
Degraded
```

depending on affected scope.

---

## 8.3 Exclusive Migration

Exclusive migration temporarily restricts normal Storage operations.

### Suitable For

- incompatible capability-wide changes;
- global metadata restructuring;
- migrations requiring consistent global ordering;
- migrations where mixed SchemaVersion values are unsafe.

### Capability State

```text
Ready

↓

Migrating

↓

Ready / Degraded / Recovering / Failed
```

---

# 9. Migration Types

---

## 9.1 Payload Representation Migration

Changes the serialized representation of an application-owned payload.

Examples:

- adding an optional field;
- renaming a persisted field;
- splitting a serialized structure;
- combining previously separate fields;
- changing encoding;
- changing normalization format.

Business modules own semantic definitions.

Storage executes the declared transformation.

---

## 9.2 Storage Metadata Migration

Changes Storage-owned metadata representation.

Examples:

- adding a persistence state field;
- changing retention metadata structure;
- adding snapshot integrity metadata;
- changing idempotency record format.

Storage owns these transformations.

---

## 9.3 Snapshot Migration

Changes the representation of:

- PersistenceSnapshot;
- SnapshotManifest;
- snapshot metadata;
- snapshot integrity records.

A migrated snapshot must be revalidated before use.

---

## 9.4 Recovery Metadata Migration

Changes:

- RecoveryPoint representation;
- RecoveryRecord representation;
- migration checkpoints;
- recovery validation metadata.

Recovery metadata migration must not invalidate required recovery paths silently.

---

## 9.5 Implementation Migration

Moves logically equivalent persisted data between Storage implementations.

Implementation migration must preserve:

- PersistenceKey;
- PersistenceId where contractually required;
- PersistenceVersion;
- SchemaVersion;
- retention state;
- archival state;
- deletion state;
- migration history;
- recovery metadata;
- idempotency behavior.

---

## 9.6 Contract Metadata Migration

Updates persisted metadata required to support a newer Storage contract version.

Contract metadata migration must not change business payload meaning.

---

# 10. Migration Definition

Every migration uses an explicit MigrationDefinition.

```text
MigrationDefinition

├── MigrationId
├── MigrationDefinitionVersion
├── MigrationScope
├── SourceSchemaVersion
├── TargetSchemaVersion
├── MigrationMode
├── Preconditions
├── Transformation
├── ValidationRules
├── RecoveryStrategy
├── CompatibilityPolicy
└── OwnershipMetadata
```

---

## 10.1 Preconditions

Preconditions may include:

- source version match;
- required metadata presence;
- required migration dependency completion;
- sufficient capability support;
- valid recovery point;
- no conflicting active migration.

---

## 10.2 Transformation

Transformation defines how source representation becomes target representation.

It must be:

- deterministic;
- versioned;
- testable;
- auditable;
- safe to retry.

---

## 10.3 ValidationRules

ValidationRules define how the target representation is verified.

They may include:

- structural consistency;
- identity preservation;
- version consistency;
- integrity validation;
- required field presence;
- declared reference consistency;
- retention metadata preservation.

---

## 10.4 RecoveryStrategy

RecoveryStrategy defines how Storage returns to a safe persistence boundary after failure.

Possible strategies include:

```text
RollbackToPreviousRepresentation

RestoreFromSnapshot

ResumeFromCheckpoint

RebuildFromAuthoritativeSource

ManualRecoveryRequired
```

---

## 10.5 OwnershipMetadata

OwnershipMetadata identifies who owns the transformation meaning.

Example:

```text
OwnerModule = ReadingSession
```

for a business payload migration.

Example:

```text
OwnerModule = Storage
```

for PersistenceMetadata migration.

---

# 11. Migration Planning

Before execution,

Storage creates or validates a MigrationPlan.

```text
MigrationPlan

├── MigrationId
├── OrderedSteps
├── AffectedScope
├── EstimatedObjectCount
├── RequiredCapabilityState
├── RecoveryPointRequirement
├── CompatibilityWindow
└── ValidationPlan
```

---

## 11.1 Dependency Ordering

Migration steps execute according to explicit dependencies.

Migration order must not rely solely on filenames or discovery order.

---

## 11.2 Version Path

A migration path may contain multiple steps.

Example:

```text
SchemaVersion 1

↓

SchemaVersion 2

↓

SchemaVersion 3
```

Storage must not skip intermediate transformations unless a direct migration is explicitly defined.

---

## 11.3 Missing Migration Path

When no valid path exists,

Storage returns:

```text
MigrationPathUnavailable
```

---

# 12. Migration Flow

```text
Migration Requested or Required
            │
            ▼
Validate Migration Definition
            │
            ▼
Determine Migration Scope
            │
            ▼
Check Source SchemaVersion
            │
            ▼
Resolve Migration Path
            │
            ▼
Validate Preconditions
            │
            ▼
Create or Validate Recovery Point
            │
            ▼
Enter Required Capability State
            │
            ▼
Execute Transformation
            │
            ▼
Validate Target Representation
            │
      ┌─────┴─────┐
      │           │
      ▼           ▼
   Success      Failure
      │           │
      ▼           ▼
Commit Target   Restore or
Representation Recover
      │           │
      ▼           ▼
Record         Record
Completion     Failure
      │
      ▼
Publish Migration Outcome
```

---

# 13. Migration Preconditions

Migration may begin only when:

1. MigrationDefinition is known.
2. SourceSchemaVersion matches the declared source.
3. TargetSchemaVersion is supported.
4. Migration scope is explicit.
5. Required dependencies have completed.
6. No conflicting migration is active.
7. Required recovery protection exists.
8. Ownership of semantic transformation is explicit.
9. Current Storage state allows migration.
10. Required access authorization is present.

---

# 14. Migration Execution

---

## 14.1 Step Execution

Each migration step must:

- read a known source representation;
- apply a deterministic transformation;
- produce a declared target representation;
- validate the target;
- record progress where required.

---

## 14.2 PersistenceVersion Behavior

A successful persisted representation migration advances PersistenceVersion when the authoritative representation changes.

```text
PreviousPersistenceVersion

↓

Migration

↓

PersistenceVersion
```

PersistenceVersion advancement remains owned by Storage.

---

## 14.3 SchemaVersion Behavior

A successful migration changes:

```text
SourceSchemaVersion

↓

TargetSchemaVersion
```

SchemaVersion must not be updated before target validation succeeds.

---

## 14.4 Source Preservation

The source representation must remain recoverable until the migration crosses its declared safe commitment boundary.

---

## 14.5 Mixed Version Operation

Mixed SchemaVersion values may exist only when explicitly supported.

Examples:

- lazy object migration;
- staged namespace migration;
- compatibility window.

Storage must reject mixed-version operation when the active contract cannot guarantee safe behavior.

---

# 15. Atomicity Model

Not every migration must be globally atomic.

Atomicity is defined by migration scope.

---

## 15.1 Single-Object Atomicity

For SingleObject migration:

```text
Old representation remains authoritative
```

until:

```text
New representation is fully validated and committed
```

---

## 15.2 Object-Set Atomicity

For an atomic ObjectSet migration:

either:

```text
All declared objects migrate
```

or:

```text
No declared object becomes visible in target form
```

---

## 15.3 Incremental Migration

Incremental migration may commit completed partitions independently.

This is allowed only when:

- each partition is independently consistent;
- migration checkpoints are durable;
- mixed-version compatibility is explicit;
- failure does not invalidate completed partitions;
- recovery can resume deterministically.

Incremental migration is not globally atomic.

The contract must state this clearly.

---

## 15.4 Exclusive Global Migration

A capability-wide migration may require one global atomic boundary.

If the implementation cannot guarantee it,

the migration must use:

- checkpointed phases;
- recovery protection;
- explicit unavailability;
- validated staged commitment.

Storage must not claim global atomicity when only phased recoverability is provided.

---

# 16. Migration Validation

Validation occurs before target representation becomes authoritative.

---

## 16.1 Structural Validation

Validates:

- required fields;
- model structure;
- representation format;
- declared type compatibility.

---

## 16.2 Identity Validation

Validates preservation of:

```text
PersistenceKey

ObjectType

ObjectId

PersistenceId
```

where required.

---

## 16.3 Version Validation

Validates:

- SourceSchemaVersion;
- TargetSchemaVersion;
- PersistenceVersion advancement;
- migration dependency versions;
- migration record version.

---

## 16.4 Integrity Validation

Validates:

- payload integrity;
- metadata integrity;
- snapshot manifest integrity;
- recovery metadata integrity;
- migration checkpoint integrity.

---

## 16.5 Retention Validation

Validates that migration preserves:

- RetainUntil;
- LegalHold;
- ArchiveAfter;
- DeleteAfter;
- DeletionMode;
- deletion guarantees.

---

## 16.6 Reference Validation

Where declared logical references exist,

migration validates that representation changes do not create inconsistent persistence references.

This does not imply physical foreign keys.

---

## 16.7 Semantic Validation

Business semantic validation is performed only through rules supplied by the owning module.

Storage does not invent semantic validation.

---

# 17. Migration Commitment

A migration is committed only after:

1. transformation completed;
2. target representation passed required validation;
3. required metadata was written;
4. migration history was recorded or atomically prepared;
5. recovery boundary remained valid;
6. target SchemaVersion became authoritative.

After commitment,

Storage may publish:

```text
StorageMigrationCompleted
```

or:

```text
ObjectRepresentationMigrated
```

depending on scope.

---

# 18. Migration Failure

Migration failure may occur during:

- planning;
- transformation;
- validation;
- commitment;
- progress recording;
- recovery preparation.

---

## 18.1 Pre-Execution Failure

If failure occurs before transformation begins:

- no persisted representation changes;
- no rollback is required;
- migration is recorded as rejected or failed where appropriate.

---

## 18.2 Transformation Failure

If transformation fails:

- target representation must not become authoritative;
- source representation remains authoritative where possible;
- recovery strategy is applied.

---

## 18.3 Validation Failure

If target validation fails:

- target SchemaVersion must not be committed;
- `StorageMigrationCompleted` must not be published;
- Storage returns `MigrationValidationFailed`.

---

## 18.4 Commitment Failure

If commitment outcome is known not to have occurred:

- source remains authoritative;
- migration may be retried according to policy.

If commitment outcome is unknown:

- Storage must enter or request recovery;
- blind retry is forbidden;
- OperationId and migration records must be inspected.

---

# 19. Rollback and Recovery

---

## 19.1 Rollback

Rollback restores the previous authoritative representation before the migration commitment boundary.

Rollback may be used when:

- transformation fails;
- validation fails;
- commitment fails before becoming authoritative;
- cancellation occurs at a safe boundary.

---

## 19.2 Recovery

Recovery is required when:

- rollback cannot be confirmed;
- commitment outcome is unknown;
- migration checkpoint integrity is uncertain;
- source representation is no longer directly restorable;
- partial staged migration must resume.

---

## 19.3 Rollback Is Not Always Possible

For large or staged migrations,

automatic rollback may be unsafe or impractical.

In such cases MigrationDefinition must declare:

```text
ResumeFromCheckpoint
```

or another explicit recovery strategy.

Storage must not promise rollback universally.

---

## 19.4 Recovery State

Capability-wide recovery uses:

```text
Migrating

↓

Recovering
```

Storage returns to Ready only after recovery validation succeeds.

---

# 20. Migration Checkpoints

MigrationCheckpoint records a safe progress boundary.

```text
MigrationCheckpoint

├── CheckpointId
├── MigrationId
├── CompletedScope
├── RemainingScope
├── LastCommittedUnit
├── SourceSchemaVersion
├── TargetSchemaVersion
├── IntegrityMetadata
└── CreatedAt
```

---

## 20.1 Checkpoint Rules

A checkpoint must:

- represent committed progress only;
- be durable before later migration work proceeds;
- support deterministic resume;
- preserve migration ordering;
- pass integrity validation.

---

## 20.2 Resume

Resuming from a checkpoint must not repeat already committed logical effects.

The same MigrationId remains in use.

---

# 21. Migration Cancellation

Cancellation is allowed only at a safe migration boundary.

---

## 21.1 Safe Cancellation

Safe cancellation may occur when:

- no transformation has begun;
- current partition is complete;
- source remains authoritative;
- checkpoint is valid;
- no commitment is in progress.

---

## 21.2 Unsafe Cancellation

When cancellation would violate consistency,

Storage returns:

```text
MigrationCancellationUnsafe
```

Storage continues until a safe boundary is reached.

---

## 21.3 Cancellation Outcome

Cancellation must be recorded in MigrationRecord.

Cancellation does not imply rollback unless explicitly performed.

---

# 22. Migration Record

Every migration attempt produces or updates a MigrationRecord.

```text
MigrationRecord

├── MigrationId
├── MigrationDefinitionVersion
├── MigrationScope
├── SourceSchemaVersion
├── TargetSchemaVersion
├── MigrationMode
├── MigrationStatus
├── StartedAt
├── CompletedAt
├── CurrentCheckpoint
├── MigratedObjectCount
├── FailedObjectCount
├── ValidationResult
├── RecoveryStrategy
├── OwnerModule
└── CorrelationId
```

---

## 22.1 MigrationStatus

Possible values:

```text
Pending

Running

Completed

Failed

Cancelled

RecoveryRequired
```

---

## 22.2 History Immutability

Completed migration history must not be rewritten.

Corrections require a new migration record or corrective migration.

---

## 22.3 Sensitive Data

MigrationRecord must not store:

- raw payload contents;
- credentials;
- connection strings;
- backend secrets;
- physical paths.

---

# 23. Migration Ordering

---

## 23.1 Explicit Dependencies

Migration definitions may declare dependencies.

Example:

```text
Migration B depends on Migration A
```

Storage must ensure A completes before B begins.

---

## 23.2 Ascending Version Paths

Schema migrations generally execute from lower to higher SchemaVersion values.

However,

ordering must be based on declared paths,

not numeric comparison alone.

---

## 23.3 No Duplicate Completion

A completed MigrationId must not execute again against the same committed scope.

Re-execution must return the existing logical outcome where idempotency records remain available.

---

## 23.4 Parallel Migration

Migrations may run in parallel only when scopes are explicitly non-conflicting.

Parallel execution must not weaken:

- ordering;
- version consistency;
- recovery;
- integrity guarantees.

---

# 24. Compatibility Policy

---

## 24.1 Supported Read Versions

Each Storage contract profile declares which SchemaVersion values it can:

- read directly;
- read after lazy migration;
- migrate eagerly;
- reject.

---

## 24.2 Supported Write Version

Storage should normally write one authoritative target SchemaVersion per declared scope.

Mixed write versions require an explicit compatibility contract.

---

## 24.3 Future Versions

A representation with an unsupported future SchemaVersion returns:

```text
UnsupportedSchemaVersion
```

Storage must not downgrade or reinterpret it automatically.

---

## 24.4 Compatibility Window

A migration may define a temporary compatibility window in which:

- old readers remain supported;
- new readers are supported;
- mixed representations are allowed.

The window must have an explicit end condition.

---

## 24.5 Previous Version Support

Support must not be described only as:

```text
Current Version

Previous Version
```

Compatibility is declared by actual SchemaVersion ranges and migration paths.

---

# 25. Implementation Migration

Implementation migration changes where or how data is physically persisted.

It must remain invisible to business modules.

---

## 25.1 Required Preservation

Implementation migration must preserve:

```text
PersistenceKey

PersistenceId

PersistenceVersion

SchemaVersion

PersistenceState

RetentionInstruction

ArchivalRecord

DeletionRecord

Snapshot relationships

Migration history

Recovery metadata

Idempotency records
```

where these concepts are included in the migration scope.

---

## 25.2 Dual-Write Migration

Temporary dual-write may be used internally.

It must not be exposed as a public architecture requirement.

If used,

the implementation must define:

- authoritative source;
- consistency verification;
- cutover boundary;
- divergence handling;
- rollback or recovery strategy.

---

## 25.3 Cutover

Cutover may occur only after:

- target integrity validation;
- version consistency validation;
- migration completion recording;
- recovery strategy validation;
- authoritative implementation selection.

---

## 25.4 Business Transparency

Business modules must not need to know which physical implementation is active.

---

# 26. Online Migration

Online migration allows some persistence operations while migration proceeds.

It is permitted only when:

- supported operations are explicit;
- mixed-version behavior is defined;
- stale writes remain detectable;
- target and source representations stay consistent;
- recovery remains possible.

Storage may enter:

```text
Degraded
```

rather than Migrating when only part of the capability is restricted.

---

# 27. Background Migration

Background migration is an execution strategy,

not a separate contract type.

It may be used for:

- eager object conversion;
- old snapshot conversion;
- metadata normalization;
- staged implementation migration.

Background migration must:

- expose progress;
- support bounded resource use;
- preserve operation correctness;
- avoid unbounded interference with foreground persistence;
- remain restartable.

---

# 28. Migration Idempotency

---

## 28.1 Migration Identity

The same MigrationId and migration definition must produce one logical migration result.

---

## 28.2 Repeated Execution

If a completed migration is requested again:

```text
Return existing completion result
```

or:

```text
Report already completed
```

No transformation is reapplied.

---

## 28.3 Definition Conflict

If the same MigrationId is supplied with different transformation content:

```text
MigrationDefinitionConflict
```

The migration must not start.

---

## 28.4 Checkpoint Resume

Resume must continue from the last valid checkpoint without duplicating already committed transformations.

---

# 29. Migration Events

Migration events are defined in `EVENTS.md`.

Primary events include:

```text
StorageMigrationStarted

ObjectRepresentationMigrated

StorageMigrationCompleted

StorageMigrationFailed
```

---

## 29.1 Publication Rules

`StorageMigrationStarted` is published after migration execution is accepted.

`ObjectRepresentationMigrated` is published only after one object target representation is committed.

`StorageMigrationCompleted` is published only after full scope validation succeeds.

`StorageMigrationFailed` is published for architecturally significant migration failure.

---

# 30. Migration Errors

Migration errors are defined in `ERRORS.md`.

Primary errors include:

```text
MigrationRequired

MigrationFailed

MigrationPathUnavailable

MigrationValidationFailed

MigrationCancellationUnsafe

UnsupportedSchemaVersion
```

Additional related errors may include:

```text
PersistenceConsistencyViolation

RecoveryFailed

SnapshotInvalid

OperationIdConflict
```

---

# 31. State Relationships

Migration behavior aligns with `STATES.md`.

---

## 31.1 Lazy Migration

```text
Ready → Ready
```

for isolated safe migration.

---

## 31.2 Restricted Online Migration

```text
Ready → Degraded → Ready
```

---

## 31.3 Exclusive Migration

```text
Ready → Migrating → Ready
```

---

## 31.4 Migration Failure Requiring Recovery

```text
Migrating → Recovering
```

---

## 31.5 Unrecoverable Migration Failure

```text
Migrating → Failed
```

---

# 32. Security and Privacy

Migration must preserve all applicable security requirements.

---

## 32.1 Access Control

Only authorized migration operations may execute.

---

## 32.2 Encryption

Migration must not silently weaken encryption requirements.

When target implementation cannot meet required encryption policy:

```text
EncryptionRequirementUnsupported
```

---

## 32.3 Retention

Migration must preserve active retention constraints.

---

## 32.4 Protected Payloads

Migration logs and records must not contain complete protected payloads by default.

---

## 32.5 Temporary Representations

Temporary migration representations must receive protection equivalent to authoritative persisted data.

They must be removed or invalidated after safe completion.

---

# 33. Observability

Every migration should expose logical observability fields.

```text
MigrationId

MigrationScope

MigrationMode

SourceSchemaVersion

TargetSchemaVersion

MigrationStatus

CurrentCheckpoint

MigratedObjectCount

FailedObjectCount

StartedAt

CompletedAt

CorrelationId
```

Physical backend details remain internal.

---

# 34. Migration Metrics

Recommended metrics include:

```text
storage_migration_started_total

storage_migration_completed_total

storage_migration_failed_total

storage_migration_object_total

storage_migration_object_failed_total

storage_migration_duration_seconds

storage_migration_checkpoint_total

storage_migration_resume_total

storage_migration_validation_failed_total

storage_migration_recovery_required_total
```

Metric labels should remain bounded.

MigrationId and ObjectId should not be used as high-cardinality labels.

---

# 35. Testing Requirements

Every migration definition should be tested for:

- source version detection;
- deterministic transformation;
- idempotent re-execution;
- target validation;
- source preservation;
- rollback behavior where promised;
- checkpoint resume;
- unknown future version rejection;
- mixed-version compatibility where supported;
- retention preservation;
- identity preservation;
- failure injection;
- recovery behavior.

---

## 35.1 Representative Data

Tests should include:

- minimum valid representation;
- maximum supported representation;
- missing optional fields;
- legacy field combinations;
- archived entries;
- logically deleted entries;
- retained entries;
- snapshot-contained entries;
- corrupted source data;
- duplicate migration requests.

---

## 35.2 Business-Owned Tests

When payload migration has business meaning,

the owning module must provide semantic validation tests.

Storage tests alone are insufficient.

---

# 36. Deployment Rules

Before deploying a migration:

1. MigrationDefinition is versioned.
2. Source and target SchemaVersion values are explicit.
3. Migration path is tested.
4. Recovery strategy is tested.
5. Compatibility window is documented.
6. required capability state is known.
7. expected duration and scope are estimated.
8. observability is enabled.
9. retention and security behavior are validated.
10. rollback or resume boundaries are known.

---

# 37. Removed Legacy Concepts

The following legacy concepts are removed from the public migration architecture:

```text
RepositoryVersion

RepositoryMigration

Repository-specific migration plans

Backend names in public migration contracts

Repository consistency validation

SCHEMA.md dependency
```

Internal implementations may organize migration code using repositories or technology-specific scripts.

Those details do not define the Storage capability contract.

---

# 38. Architecture Invariants

The following invariants always apply.

1. Migration changes persistence representation only.
2. Storage never silently changes business meaning.
3. Business semantic migration is owned by the relevant business module.
4. Every migration has a stable MigrationId.
5. Every migration declares source and target SchemaVersion values.
6. Migration definitions are deterministic.
7. Migration execution is idempotent.
8. Completed migration history is immutable.
9. Target representation is not authoritative before validation.
10. Unsupported future versions are rejected.
11. PersistenceKey identity is preserved unless an explicit owner-approved identity migration exists.
12. PersistenceVersion changes only through Storage.
13. Retention constraints survive migration.
14. Migration does not weaken security guarantees silently.
15. Temporary migration data receives equivalent protection.
16. Lazy migration does not require a global Migrating state.
17. Exclusive migration is used only when consistency requires it.
18. Mixed SchemaVersion operation must be explicit.
19. Incremental migration must use durable checkpoints.
20. Checkpoint resume never duplicates committed effects.
21. Rollback is promised only when technically guaranteed.
22. Unknown commitment outcomes require recovery.
23. Migration failure never publishes completion events.
24. Implementation migration remains transparent to business modules.
25. Backend-specific details never cross the public boundary.
26. Repository concepts never define public migration ownership.
27. Migration records exclude raw business payloads by default.
28. Recovery restores persistence consistency before Ready.
29. Migration and recovery remain separate lifecycle concepts.
30. All migration behavior aligns with `CONTRACT.md`, `MODELS.md`, `STATES.md`, `EVENTS.md`, and `ERRORS.md`.

---

# 39. Related Documents

| Document | Responsibility |
|---|---|
| README.md | Storage overview |
| MODULE.md | Storage ownership and architecture boundary |
| CONTRACT.md | Public persistence and migration contracts |
| STATES.md | Migrating and Recovering capability states |
| EVENTS.md | Migration lifecycle events |
| ERRORS.md | Migration and recovery errors |
| MODELS.md | MigrationRecord, MigrationCheckpoint and SchemaVersion models |
| MIGRATION.md | Migration strategy and guarantees |

---

# 40. Summary

Storage migration evolves persistence representations without transferring business ownership to Storage.

The migration architecture supports:

- lazy migration;
- eager migration;
- exclusive migration;
- incremental migration;
- online migration;
- implementation migration;
- checkpointed recovery;
- explicit compatibility windows.

Every migration must define:

- identity;
- ownership;
- scope;
- source version;
- target version;
- transformation;
- validation;
- recovery strategy;
- compatibility behavior.

Storage owns persistence evolution.

Business modules own semantic evolution.

This separation ensures that persisted data can evolve safely while Storage implementations and CRAI modules remain independently replaceable.

---

# End of Document