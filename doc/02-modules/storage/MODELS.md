# Storage Module Models

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Document: MODELS.md
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines the logical models owned or used by the Storage Module.

It describes:

- Storage-owned persistence concepts;
- public persistence value objects;
- persistence metadata;
- snapshot and recovery models;
- retention models;
- migration models;
- relationships between Storage-owned concepts.

This document does not define:

- business entities;
- database tables;
- ORM models;
- repository classes;
- SQL schemas;
- indexes;
- filesystem layouts;
- backend-specific records;
- serialization implementation.

---

# 2. Model Boundary

Storage models are divided into two groups.

```text
Storage Models

├── Storage-Owned Models
└── Contract Value Objects
```

---

## 2.1 Storage-Owned Models

Storage owns the lifecycle and correctness of these concepts.

Examples:

```text
PersistenceEntry

PersistenceVersion

PersistenceMetadata

PersistenceSnapshot

RetentionInstruction

ArchivalRecord

RecoveryPoint

MigrationRecord
```

---

## 2.2 Contract Value Objects

These objects are exchanged through Storage contracts.

Examples:

```text
PersistenceKey

PersistencePayload

ExpectedPersistenceVersion

OperationId

QuerySpecification
```

They support persistence behavior,

but they do not transfer ownership of business meaning to Storage.

---

# 3. Ownership Rules

---

## 3.1 Business Objects Remain External

Storage may persist objects such as:

```text
ReadingSession

ContentRevision

RecognitionResult

TranslationResult

PresentationArtifact
```

but Storage does not own those models.

Their structure and business lifecycle belong to their originating modules.

Storage receives only a persistence representation.

---

## 3.2 No Domain Registry

Storage does not maintain a hard-coded catalog of CRAI business entities.

Adding a new business module must not require adding a new Storage-owned model.

---

## 3.3 Persistence Representation Ownership

Storage owns:

- persistence identity;
- persistence version;
- persistence state;
- persistence metadata;
- snapshot identity;
- retention enforcement state;
- migration history;
- recovery metadata.

Storage does not own the business payload inside a persistence entry.

---

# 4. Model Overview

```text
PersistenceEntry
│
├── PersistenceId
├── PersistenceKey
├── PersistencePayload
├── PersistenceMetadata
├── PersistenceState
└── PersistenceVersion

PersistenceSnapshot
│
├── SnapshotId
├── SnapshotScope
├── SnapshotManifest
└── SnapshotMetadata

RetentionInstruction
│
├── RetentionClass
├── RetainUntil
├── ArchiveAfter
├── DeleteAfter
├── DeletionMode
└── LegalHold

ArchivalRecord
│
├── PersistenceKey
├── ArchiveReason
├── ArchivedAt
└── ArchivedVersion

RecoveryPoint
│
├── RecoveryPointId
├── RecoveryScope
├── RecoverySource
└── ValidationMetadata

MigrationRecord
│
├── MigrationId
├── MigrationScope
├── SourceSchemaVersion
├── TargetSchemaVersion
└── MigrationStatus
```

---

# 5. PersistenceKey

PersistenceKey identifies one application-owned object within Storage.

```text
PersistenceKey

├── ObjectType
└── ObjectId
```

---

## 5.1 ObjectType

ObjectType identifies the logical category supplied by the owning module.

Examples may include:

```text
ReadingSession

ContentRevision

TranslationResult
```

Storage treats ObjectType as an opaque declared identifier.

It does not infer business behavior from it.

---

## 5.2 ObjectId

ObjectId identifies the business object within its ObjectType.

ObjectId is:

- supplied by the owning module;
- stable for the object lifetime;
- immutable after creation;
- independent from PersistenceId.

---

## 5.3 Identity Rule

The combination below identifies one persisted application object:

```text
ObjectType + ObjectId
```

Two objects with the same ObjectId but different ObjectType values are distinct.

---

# 6. PersistenceId

PersistenceId identifies the Storage-managed persistence representation.

```text
PersistenceId
```

It is assigned and managed only by Storage.

---

## 6.1 Purpose

PersistenceId supports:

- internal persistence correlation;
- snapshots;
- archival;
- migration;
- recovery;
- diagnostics.

---

## 6.2 Separation from ObjectId

```text
PersistenceId != ObjectId
```

ObjectId belongs to the originating module.

PersistenceId belongs to Storage.

A consumer must not use PersistenceId as a replacement for business identity.

---

## 6.3 Stability

PersistenceId remains stable while the same logical persistence entry continues to exist.

A physical backend replacement must not require changing PersistenceId at the public contract boundary.

---

# 7. PersistencePayload

PersistencePayload contains the application-owned representation supplied for persistence.

```text
PersistencePayload

├── Content
├── ContentType
├── SchemaVersion
└── PayloadMetadata
```

---

## 7.1 Content

Content contains the serialized business representation.

Storage treats Content as semantically opaque.

It may validate:

- presence;
- size limits;
- declared encoding;
- integrity;
- schema compatibility.

It must not validate business correctness.

---

## 7.2 ContentType

ContentType declares the logical representation format.

Examples may include:

```text
application/json

application/cbor

application/octet-stream
```

Specific supported formats are implementation or contract-profile decisions.

---

## 7.3 SchemaVersion

SchemaVersion identifies the version of the persisted representation.

```text
SchemaVersion
```

It supports:

- compatibility checks;
- migration;
- recovery;
- representation evolution.

SchemaVersion does not represent business lifecycle state.

---

## 7.4 PayloadMetadata

PayloadMetadata may contain declared non-sensitive attributes required for persistence handling.

Examples:

```text
Compression

Encoding

IntegrityAlgorithm

DeclaredIndexes
```

PayloadMetadata must not contain backend-specific details.

---

# 8. PersistenceEntry

PersistenceEntry is the primary Storage-owned logical record.

```text
PersistenceEntry

├── PersistenceId
├── PersistenceKey
├── PersistencePayload
├── PersistenceVersion
├── PersistenceState
└── PersistenceMetadata
```

---

## 8.1 Meaning

A PersistenceEntry represents one application-owned object as persisted by Storage.

It does not become the business object itself.

---

## 8.2 Ownership

Storage owns:

- entry identity;
- persistence version;
- persistence state;
- persistence metadata;
- representation integrity.

The originating module owns:

- payload meaning;
- business lifecycle;
- business correctness;
- business revision.

---

## 8.3 Invariant

One active PersistenceKey maps to at most one authoritative active PersistenceEntry.

---

# 9. PersistenceVersion

PersistenceVersion identifies the authoritative persisted version of a PersistenceEntry.

```text
PersistenceVersion
```

---

## 9.1 Ownership

PersistenceVersion is assigned only by Storage.

Consumers treat it as opaque and read-only.

---

## 9.2 Advancement

PersistenceVersion advances after successful persisted mutations such as:

- replacement;
- archival;
- restoration;
- logical deletion;
- versioned retention changes;
- representation migration.

---

## 9.3 Separation from Business Revision

```text
PersistenceVersion != BusinessRevision
```

For example:

```text
PersistenceVersion != ContentRevision
```

Business revisions remain inside the owning module.

---

## 9.4 Ordering

PersistenceVersion values define committed ordering for one PersistenceEntry.

They do not define global ordering across different entries.

---

# 10. ExpectedPersistenceVersion

ExpectedPersistenceVersion is a command value object used for concurrency control.

```text
ExpectedPersistenceVersion

├── Mode
└── Version
```

Possible modes:

```text
None

Exact

Any
```

---

## 10.1 None

The caller expects that no active persistence entry exists.

Typical use:

```text
PersistObject
```

---

## 10.2 Exact

The caller expects the current PersistenceVersion to equal the supplied value.

Typical use:

```text
ReplaceObject

ArchiveObject

DeleteObject
```

---

## 10.3 Any

The caller allows mutation regardless of the current persisted version.

Use of `Any` must be explicit.

It should be restricted because it disables stale-write protection.

---

# 11. PersistenceState

PersistenceState describes the logical persistence status of one entry.

```text
PersistenceState
```

Possible values are:

```text
Active

Archived

LogicallyDeleted
```

Physical deletion removes the active PersistenceEntry from normal availability rather than introducing a permanent active state.

---

## 11.1 Active

The entry is available through normal persistence queries.

---

## 11.2 Archived

The entry remains persisted but is excluded from normal active-object queries.

---

## 11.3 LogicallyDeleted

The entry is marked unavailable to normal access while retained according to policy.

---

## 11.4 Business State Separation

PersistenceState does not represent:

- Reading Session state;
- translation state;
- OCR processing state;
- presentation state;
- Runtime task state.

---

# 12. PersistenceMetadata

PersistenceMetadata contains Storage-owned metadata for one PersistenceEntry.

```text
PersistenceMetadata

├── PersistenceId
├── PersistenceKey
├── PersistenceVersion
├── SchemaVersion
├── PersistenceState
├── CreatedAt
├── UpdatedAt
├── ArchivedAt
├── DeletedAt
├── RetentionClass
├── IntegrityMetadata
└── ExtensionMetadata
```

---

## 12.1 CreatedAt

The time the PersistenceEntry was first committed.

---

## 12.2 UpdatedAt

The time the authoritative persisted representation last changed.

---

## 12.3 ArchivedAt

The time the entry entered Archived state.

Absent when not archived.

---

## 12.4 DeletedAt

The time logical or physical deletion was recorded.

Absent when not deleted.

---

## 12.5 RetentionClass

A logical retention classification supplied by policy.

Storage applies it without inventing business policy.

---

## 12.6 IntegrityMetadata

IntegrityMetadata describes logical integrity validation information.

```text
IntegrityMetadata

├── IntegrityAlgorithm
├── IntegrityValue
├── VerifiedAt
└── VerificationStatus
```

Physical checksum implementation remains backend-specific.

---

## 12.7 ExtensionMetadata

ExtensionMetadata allows compatible addition of non-sensitive persistence attributes.

It must not be used to bypass explicit contract modeling.

---

# 13. OperationId

OperationId identifies one logical mutation command.

```text
OperationId
```

---

## 13.1 Purpose

OperationId supports:

- idempotency;
- duplicate detection;
- retry safety;
- event correlation;
- diagnostics.

---

## 13.2 Stability

A retry of the same logical mutation must reuse the same OperationId.

A different logical mutation must use a new OperationId.

---

# 14. IdempotencyRecord

IdempotencyRecord stores the authoritative result associated with an OperationId.

```text
IdempotencyRecord

├── OperationId
├── OperationType
├── RequestFingerprint
├── Outcome
├── Subject
├── CreatedAt
└── ExpiresAt
```

---

## 14.1 RequestFingerprint

RequestFingerprint identifies the logical command content.

Reusing an OperationId with a different fingerprint produces:

```text
OperationIdConflict
```

---

## 14.2 Outcome

Outcome records the original logical result.

Possible forms include:

```text
Succeeded

Rejected

FailedWithKnownOutcome
```

---

## 14.3 Expiration

Idempotency retention duration is defined by implementation or deployment policy.

The public contract must not imply indefinite duplicate detection unless explicitly configured.

---

# 15. RetentionInstruction

RetentionInstruction describes persistence retention requirements supplied from outside Storage.

```text
RetentionInstruction

├── RetentionClass
├── RetainUntil
├── ArchiveAfter
├── DeleteAfter
├── DeletionMode
├── LegalHold
└── InstructionMetadata
```

---

## 15.1 RetentionClass

A policy-defined logical category.

Examples may include:

```text
Temporary

SessionScoped

LongTerm

AuditRequired
```

The exact values belong to policy configuration.

---

## 15.2 RetainUntil

The earliest time before which the entry must not be deleted.

---

## 15.3 ArchiveAfter

The time after which archival is allowed or requested.

---

## 15.4 DeleteAfter

The time after which deletion is allowed or requested.

Expiration does not automatically imply immediate deletion unless the instruction explicitly defines automatic enforcement.

---

## 15.5 DeletionMode

Possible values:

```text
Logical

Physical

Secure
```

`Secure` may be accepted only when the implementation explicitly supports the required guarantee.

---

## 15.6 LegalHold

When true,

destructive retention actions are prohibited until the hold is removed.

---

# 16. ArchivalRecord

ArchivalRecord describes the archival fact for one PersistenceEntry.

```text
ArchivalRecord

├── ArchiveId
├── PersistenceId
├── PersistenceKey
├── PreviousPersistenceVersion
├── ArchivedPersistenceVersion
├── ArchiveReason
├── ArchivedAt
└── RetentionInstruction
```

---

## 16.1 Meaning

ArchivalRecord represents persistence archival history.

It does not mean that the business object is complete or inactive.

---

## 16.2 Restore Relationship

Restoring an archived entry creates a new authoritative PersistenceVersion.

The archival record remains historical metadata where retention permits.

---

# 17. DeletionRecord

DeletionRecord describes a completed logical or physical deletion operation.

```text
DeletionRecord

├── DeletionId
├── PersistenceId
├── PersistenceKey
├── FinalPersistenceVersion
├── DeletionMode
├── DeletionGuarantee
├── DeletionReason
├── DeletedAt
└── RetentionReference
```

---

## 17.1 DeletionGuarantee

DeletionGuarantee declares what was actually guaranteed.

Possible values may include:

```text
LogicalUnavailability

PhysicalRepresentationRemoved

SecureErasureConfirmed
```

Storage must not claim a stronger guarantee than the active implementation provides.

---

## 17.2 Historical Metadata

Deletion metadata may remain after physical payload removal when required for:

- audit;
- idempotency;
- recovery protection;
- policy enforcement.

Such metadata must not expose deleted payload content.

---

# 18. PersistenceSnapshot

PersistenceSnapshot represents a stable persistence boundary.

```text
PersistenceSnapshot

├── SnapshotId
├── SnapshotScope
├── SnapshotManifest
├── SnapshotMetadata
└── SnapshotState
```

---

## 18.1 SnapshotId

Uniquely identifies the snapshot.

---

## 18.2 SnapshotScope

Defines the persistence boundary represented by the snapshot.

```text
SnapshotScope

├── ScopeType
└── ScopeSelector
```

Possible scope types include:

```text
SingleObject

ObjectSet

ObjectType

ModuleNamespace

ApplicationPersistenceBoundary
```

---

## 18.3 SnapshotManifest

SnapshotManifest declares the entries and versions included.

```text
SnapshotManifest

├── Entries
│   ├── PersistenceKey
│   ├── PersistenceId
│   ├── PersistenceVersion
│   └── SchemaVersion
├── EntryCount
└── ManifestIntegrity
```

---

## 18.4 SnapshotMetadata

```text
SnapshotMetadata

├── CreatedAt
├── CreatedByOperationId
├── ConsistencyLevel
├── SnapshotReason
├── IntegrityMetadata
└── RetentionInstruction
```

---

## 18.5 SnapshotState

Possible values include:

```text
Creating

Valid

Invalid

Expired

Deleted
```

These are snapshot record states,

not global Storage capability states.

---

## 18.6 Business Execution Separation

Restoring a PersistenceSnapshot does not resume:

- Runtime work;
- Reading Sessions;
- translation pipelines;
- presentation execution.

Owning modules reevaluate restored business objects.

---

# 19. RecoveryPoint

RecoveryPoint identifies a validated source from which persistence state may be restored.

```text
RecoveryPoint

├── RecoveryPointId
├── RecoveryScope
├── RecoverySource
├── SourceReference
├── CreatedAt
├── ValidationMetadata
└── RecoveryCompatibility
```

---

## 19.1 RecoverySource

Possible logical source types include:

```text
Snapshot

BackupRepresentation

PersistenceJournal

MigrationCheckpoint
```

These values describe logical recovery sources,

not backend technology.

---

## 19.2 ValidationMetadata

```text
ValidationMetadata

├── ValidationStatus
├── ValidatedAt
├── IntegrityMetadata
└── ValidationVersion
```

A RecoveryPoint must not be treated as usable until validation succeeds.

---

## 19.3 RecoveryCompatibility

RecoveryCompatibility declares the schema and contract boundaries supported by the recovery point.

---

# 20. RecoveryRecord

RecoveryRecord describes one recovery attempt.

```text
RecoveryRecord

├── RecoveryId
├── RecoveryPointId
├── RecoveryScope
├── RecoveryMode
├── StartedAt
├── CompletedAt
├── RecoveryStatus
├── RestoredEntries
├── FailedEntries
└── ValidationResult
```

---

## 20.1 RecoveryStatus

Possible values:

```text
Pending

Running

Completed

PartiallyCompleted

Failed

Cancelled
```

These are recovery-operation states,

not global capability states.

---

## 20.2 ValidationResult

A recovery cannot be marked Completed until persistence integrity validation succeeds.

Business validation remains external.

---

# 21. MigrationRecord

MigrationRecord describes one persistence schema migration.

```text
MigrationRecord

├── MigrationId
├── MigrationScope
├── SourceSchemaVersion
├── TargetSchemaVersion
├── MigrationMode
├── MigrationStatus
├── StartedAt
├── CompletedAt
├── MigrationCheckpoint
└── ValidationResult
```

---

## 21.1 MigrationScope

Defines the affected persistence boundary.

Possible scopes include:

```text
SingleObject

ObjectType

Namespace

ApplicationPersistenceBoundary
```

---

## 21.2 MigrationMode

Possible values:

```text
Lazy

Eager

Exclusive
```

---

## 21.3 MigrationStatus

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

## 21.4 MigrationCheckpoint

MigrationCheckpoint records a safe restart or recovery boundary.

```text
MigrationCheckpoint

├── CheckpointId
├── MigrationId
├── CompletedScope
├── RemainingScope
├── CreatedAt
└── IntegrityMetadata
```

---

## 21.5 Meaning Preservation

MigrationRecord describes representation evolution.

It must not claim business semantic transformation unless explicitly defined and owned by the relevant business module.

---

# 22. QuerySpecification

QuerySpecification defines a bounded query against declared persistence metadata.

```text
QuerySpecification

├── Conditions
├── Ordering
├── Limit
└── DeclaredIndexes
```

---

## 22.1 Conditions

Supported conditions may reference:

```text
ObjectType

ObjectId

PersistenceState

PersistenceVersion

SchemaVersion

CreatedAt

UpdatedAt

ArchivedAt

RetentionClass

DeclaredIndex
```

Storage must not interpret arbitrary business payload fields unless they are explicitly declared as persistence indexes.

---

## 22.2 Ordering

Ordering may use declared persistence metadata fields.

Global deterministic ordering must not be assumed without an explicit sort definition.

---

## 22.3 Limit

Queries must support bounded result size.

Unbounded object loading should not be part of the default public contract.

---

## 22.4 DeclaredIndexes

A business module may supply safe declared index values for persistence queries.

Storage owns index persistence behavior,

but the originating module owns the meaning of the indexed value.

---

# 23. PageRequest

PageRequest describes bounded result pagination.

```text
PageRequest

├── PageSize
├── ContinuationToken
└── Direction
```

ContinuationToken must be treated as opaque by consumers.

It must not expose physical backend offsets or keys.

---

# 24. PageInformation

PageInformation describes the returned page.

```text
PageInformation

├── ItemCount
├── HasMore
├── NextContinuationToken
└── PreviousContinuationToken
```

Token availability depends on supported query direction.

---

# 25. CapabilityDescriptor

CapabilityDescriptor describes the logical features currently available from Storage.

```text
CapabilityDescriptor

├── ContractVersion
├── SupportedOperations
├── UnsupportedOperations
├── SupportedConsistencyLevels
├── SupportedDeletionGuarantees
├── SupportedSnapshotScopes
├── SupportedRetentionCapabilities
└── CapabilityState
```

---

## 25.1 Purpose

CapabilityDescriptor supports:

- degraded-state reporting;
- compatibility validation;
- operational diagnostics;
- safe feature detection.

---

## 25.2 Backend Independence

CapabilityDescriptor must not expose:

- database brand;
- filesystem type;
- cloud provider;
- driver version;
- physical deployment layout.

---

# 26. DegradationDescriptor

DegradationDescriptor describes reduced Storage availability.

```text
DegradationDescriptor

├── AvailableOperations
├── UnavailableOperations
├── AffectedScope
├── ReasonCode
├── DetectedAt
└── ExpectedRecoveryCondition
```

It corresponds to the global `Degraded` state defined in `STATES.md`.

---

# 27. Model Relationships

```text
PersistenceKey
      │
      ▼
PersistenceEntry
      │
      ├──────────────► PersistenceMetadata
      │
      ├──────────────► PersistencePayload
      │
      ├──────────────► RetentionInstruction
      │
      ├──────────────► ArchivalRecord
      │
      └──────────────► DeletionRecord

PersistenceSnapshot
      │
      ▼
SnapshotManifest
      │
      ▼
PersistenceEntry References

RecoveryPoint
      │
      ▼
PersistenceSnapshot / Recovery Source
      │
      ▼
RecoveryRecord

MigrationRecord
      │
      ▼
MigrationCheckpoint
      │
      ▼
PersistenceEntry Scope
```

Relationships are logical.

They do not imply:

- foreign keys;
- join tables;
- object references;
- repository boundaries;
- physical co-location.

---

# 28. Model Versioning

---

## 28.1 Contract Model Version

Public model structures evolve with the Storage contract version.

---

## 28.2 Event and Error Independence

```text
ContractModelVersion

EventVersion

ErrorVersion

SchemaVersion
```

are independent version dimensions.

A change in one does not automatically require changes in the others.

---

## 28.3 Persistence Schema Version

SchemaVersion belongs to the persisted representation.

It must not be used as the version of the Storage public contract itself.

---

## 28.4 Backward-Compatible Changes

Compatible model evolution may include:

- adding optional fields;
- adding optional metadata;
- adding enum values when unknown values are safely supported.

---

## 28.5 Breaking Changes

Breaking changes include:

- changing identity semantics;
- removing required fields;
- changing field meaning;
- changing version behavior;
- weakening persistence guarantees.

Breaking changes require a new major contract version.

---

# 29. Validation Rules

Storage validates model structure only.

Examples include:

- required field presence;
- identifier format;
- supported ContentType;
- valid version value;
- retention field consistency;
- bounded query size;
- supported snapshot scope.

Storage does not validate:

- whether a ReadingSession may be completed;
- whether translation output is correct;
- whether OCR confidence is acceptable;
- whether presentation layout is valid;
- whether business metadata is meaningful.

---

# 30. Serialization Rules

Public models are logical structures.

This document does not require a specific encoding.

Possible implementations may use:

- JSON;
- CBOR;
- Protocol Buffers;
- binary encoding;
- native in-memory structures.

Serialization choice must not change public model meaning.

---

# 31. Privacy and Security

Storage models must minimize protected data exposure.

---

## 31.1 Payload Isolation

Complete business payloads should not appear in:

- events;
- errors;
- logs;
- metrics;
- capability descriptors.

---

## 31.2 Metadata Safety

Metadata must not contain:

- credentials;
- access tokens;
- encryption keys;
- connection strings;
- physical backend paths;
- implementation secrets.

---

## 31.3 Identifier Protection

Sensitive ObjectId values may require:

- redaction;
- hashing;
- indirect reference;
- access-controlled visibility.

The chosen protection must preserve required correlation behavior.

---

# 32. Models Not Owned by Storage

The following models must not be defined as Storage-owned entities:

```text
Preference

ReadingSession

ContentRevision

OCRResult

RecognitionResult

TranslationResult

TranslationCache

PresentationCache

ImageAsset

MemoryEntry

DiagnosticRecord

UserProfile

PluginConfiguration

DownloadTask

DictionaryEntry
```

These belong to their corresponding modules.

Storage may persist their representations through generic persistence contracts.

---

# 33. Removed Legacy Concepts

The following concepts from the previous `SCHEMA.md` are intentionally removed from the Storage model boundary:

```text
PreferenceRepository

SessionRepository

OCRRepository

TranslationRepository

PresentationRepository

ImageRepository

AIMemoryRepository

DiagnosticsRepository

MetadataRepository
```

They are not required by the public architecture.

An implementation may internally use repositories,

but that pattern must not define Storage ownership or public models.

---

# 34. Architecture Invariants

The following invariants always apply.

1. Storage owns persistence models only.
2. Storage never owns business entities.
3. PersistenceEntry is not the business object itself.
4. PersistenceId never replaces ObjectId.
5. PersistenceKey is composed of ObjectType and ObjectId.
6. One active PersistenceKey maps to at most one authoritative PersistenceEntry.
7. PersistenceVersion is managed only by Storage.
8. Business revision is managed only by the owning module.
9. PersistenceState never represents business lifecycle state.
10. SchemaVersion represents persisted format only.
11. Storage validates model structure but not business meaning.
12. QuerySpecification uses declared persistence metadata only.
13. Backend-specific records never cross the model boundary.
14. Repository classes are not public Storage models.
15. Snapshot restoration never resumes business execution.
16. Recovery models restore persistence state only.
17. Migration models describe representation evolution only.
18. Retention policy originates outside Storage.
19. DeletionGuarantee never exceeds implementation capability.
20. Public model evolution remains versioned.
21. Complete business payloads are excluded from diagnostics by default.
22. Logical relationships never imply physical database relationships.
23. Adding a new business module does not require a new Storage-owned entity.
24. Storage implementations remain replaceable.
25. All models align with `CONTRACT.md`, `STATES.md`, `EVENTS.md`, and `ERRORS.md`.

---

# 35. Related Documents

| Document | Responsibility |
|---|---|
| README.md | Storage overview |
| MODULE.md | Ownership and architectural boundaries |
| CONTRACT.md | Public operations using these models |
| STATES.md | Storage capability lifecycle |
| EVENTS.md | Events built from persistence models |
| ERRORS.md | Errors associated with model validation and persistence |
| MODELS.md | Storage-owned logical models |
| MIGRATION.md | Representation evolution and migration records |

---

# 36. Summary

`MODELS.md` replaces the former `SCHEMA.md`.

The renamed document no longer describes a centralized schema for every CRAI business entity.

Instead, it defines only:

- Storage-owned persistence models;
- public persistence value objects;
- metadata structures;
- snapshots;
- retention;
- archival;
- deletion;
- migration;
- recovery;
- query structures.

Business modules remain responsible for their own models.

Storage persists their representations without becoming their owner.

This preserves:

- single ownership;
- business-module independence;
- backend independence;
- implementation replaceability;
- stable persistence contracts.

---

# End of Document