# Storage Module Events

- Module: Storage
- Identifier: storage
- Layer: Persistence Capability
- Version: 2.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# 1. Purpose

This document defines the events published and consumed by the Storage Module.

Storage events communicate facts about:

- persistence outcomes;
- persistence version changes;
- snapshots;
- archival;
- deletion;
- retention;
- migration;
- recovery;
- capability state transitions.

Storage events describe persistence facts only.

They do not describe:

- business decisions;
- business object lifecycle;
- Runtime execution;
- database operations;
- repository operations;
- physical backend behavior.

---

# 2. Event Ownership

Storage owns events whose primary meaning is persistence.

Examples include:

```text
ObjectPersisted

ObjectReplaced

SnapshotCreated

StorageReady

StorageRecoveryCompleted
```

Storage does not own events whose primary meaning belongs to another module.

For example:

```text
ReadingSessionStarted
```

belongs to Reading Session.

```text
TranslationCompleted
```

belongs to Translation.

```text
RecognitionCompleted
```

belongs to Recognition.

Storage may persist data resulting from those events,

but it must not republish them as Storage-owned business events.

---

# 3. Event Categories

Storage events are divided into four categories.

```text
Storage Events

├── Persistence Outcome Events
├── Administrative Outcome Events
├── Capability State Events
└── Failure Events
```

---

## 3.1 Persistence Outcome Events

These events describe successful changes to persisted state.

Examples:

```text
ObjectPersisted

ObjectReplaced

ObjectArchived

ObjectDeleted
```

---

## 3.2 Administrative Outcome Events

These events describe completed persistence administration.

Examples:

```text
SnapshotCreated

SnapshotRestored

MigrationCompleted

StorageRecoveryCompleted
```

---

## 3.3 Capability State Events

These events describe Storage capability transitions.

Examples:

```text
StorageReady

StorageDegraded

StorageFailed

StorageStopped
```

---

## 3.4 Failure Events

These events describe significant Storage failures that are useful outside the direct command response.

Examples:

```text
StorageMigrationFailed

StorageRecoveryFailed

StorageConsistencyViolationDetected
```

Routine command validation failures do not necessarily require published events.

---

# 4. Event Principles

---

## 4.1 Events Describe Facts

Published Storage events describe something that has already occurred.

They must not represent commands or requests.

Correct:

```text
ObjectPersisted
```

Incorrect:

```text
PersistObjectRequested
```

Requests belong to command contracts.

Facts belong to events.

---

## 4.2 Past-Tense Naming

Storage events use completed-action or established-state naming.

Examples:

```text
ObjectPersisted

SnapshotCreated

StorageReady

MigrationCompleted
```

---

## 4.3 Immutable Events

A published event must never be modified.

Corrections require a new event.

---

## 4.4 Backend Independence

Events must not expose:

- SQL statements;
- table names;
- database engines;
- repository names;
- filesystem paths;
- object storage buckets;
- connection identifiers;
- driver failures;
- physical transaction identifiers.

---

## 4.5 Business Semantic Neutrality

Storage events may identify the persisted object type,

but must not interpret its business meaning.

For example:

```text
ObjectType = ReadingSession
```

is allowed.

The event must not claim:

```text
The reading session is valid.

The reading session should resume.

The user completed reading.
```

---

## 4.6 Version Awareness

Object mutation events include the relevant:

```text
PersistenceVersion

SchemaVersion
```

where applicable.

Storage events must never use PersistenceVersion as a substitute for business revision.

---

## 4.7 Success After Commitment

A success event may be published only after the corresponding persistence outcome is committed.

Storage must never publish a success event for:

- an uncommitted write;
- a rolled-back atomic operation;
- an incomplete migration;
- an unvalidated recovery;
- a partial snapshot restoration.

---

## 4.8 Duplicate-Tolerant Delivery

Consumers must tolerate duplicate delivery.

Every event contains an EventId.

Events associated with an idempotent command should also include OperationId.

---

## 4.9 Explicit Ordering Scope

Ordering is guaranteed only within a declared ordering scope.

Storage does not promise global ordering across unrelated objects or operations.

---

# 5. Event Envelope

Every Storage event uses a common envelope.

```text
StorageEvent

├── EventId
├── EventType
├── EventVersion
├── OccurredAt
├── PublishedAt
├── CorrelationId
├── CausationId
├── OperationId
├── StorageInstanceId
├── Subject
├── Payload
└── Metadata
```

---

## 5.1 EventId

A globally unique identifier for the event.

```text
EventId
```

Consumers use EventId for duplicate detection.

---

## 5.2 EventType

Identifies the logical event.

Examples:

```text
storage.object.persisted

storage.snapshot.created

storage.capability.ready
```

---

## 5.3 EventVersion

Identifies the schema version of the event itself.

```text
EventVersion
```

EventVersion is independent from:

```text
PersistenceVersion

SchemaVersion

BusinessRevision
```

---

## 5.4 OccurredAt

The time when the described persistence fact became true.

---

## 5.5 PublishedAt

The time when the event became available to consumers.

`PublishedAt` may be later than `OccurredAt`.

---

## 5.6 CorrelationId

Links the event to a wider application workflow.

Storage records it without interpreting the workflow.

---

## 5.7 CausationId

Identifies the command or event that directly caused this event.

---

## 5.8 OperationId

Identifies the persistence operation associated with the event.

It is especially important for:

- idempotent commands;
- duplicate detection;
- audit correlation;
- atomic operation grouping.

---

## 5.9 StorageInstanceId

Identifies the logical Storage capability instance that emitted the event.

It must not reveal physical backend identity.

---

## 5.10 Subject

Subject identifies the main persistence entity associated with the event.

Examples:

```text
PersistenceKey

SnapshotId

MigrationId

RecoveryPointId

StorageCapability
```

---

## 5.11 Metadata

Metadata may contain declared non-sensitive context.

It must not contain:

- raw application payloads by default;
- credentials;
- encryption keys;
- backend secrets;
- connection strings;
- physical paths.

---

# 6. Consumed Inputs

Storage primarily consumes commands through `CONTRACT.md`.

It must not treat every command as an integration event.

Possible external events may be consumed only when event-driven integration is explicitly required.

---

## 6.1 ApplicationShutdownRequested

### Owner

Application Lifecycle.

### Purpose

Requests safe termination of Storage.

### Result

Storage may transition to:

```text
ShuttingDown
```

and eventually publish:

```text
StorageStopped
```

### Rule

Storage must not assume that process termination may occur before accepted atomic operations reach a safe boundary.

---

## 6.2 SecurityPolicyChanged

### Owner

Security or Configuration.

### Purpose

Indicates that persistence security policy has changed.

Storage reevaluates applicable persistence controls.

### Rule

Storage does not define the policy.

It only applies supported persistence-level requirements.

---

## 6.3 RetentionPolicyChanged

### Owner

Policy or owning business module.

### Purpose

Indicates that externally owned retention rules have changed.

### Rule

This event does not itself delete or archive data unless it contains an explicit authorized retention instruction.

---

## 6.4 StorageConfigurationChanged

### Owner

Configuration.

### Purpose

Indicates that logical Storage configuration has changed.

### Rule

Storage must reject unsupported live configuration changes rather than silently weaken guarantees.

---

# 7. Persistence Outcome Events

---

## 7.1 ObjectPersisted

### Event Type

```text
storage.object.persisted
```

### Meaning

A new persistence entry has been created successfully.

### Payload

```text
ObjectPersisted

├── PersistenceId
├── PersistenceKey
│   ├── ObjectType
│   └── ObjectId
├── PersistenceVersion
├── SchemaVersion
├── CreatedAt
└── RetentionClass
```

### Publication Rule

Published only after the new object is durably committed.

### Does Not Mean

- the object is business-valid;
- the business workflow started;
- downstream processing should automatically run.

---

## 7.2 ObjectReplaced

### Event Type

```text
storage.object.replaced
```

### Meaning

An existing persisted representation has been replaced successfully.

### Payload

```text
ObjectReplaced

├── PersistenceId
├── PersistenceKey
├── PreviousPersistenceVersion
├── PersistenceVersion
├── SchemaVersion
└── UpdatedAt
```

### Publication Rule

Published only after the replacement becomes authoritative.

### Invariant

```text
PreviousPersistenceVersion != PersistenceVersion
```

---

## 7.3 ObjectSetPersisted

### Event Type

```text
storage.object-set.persisted
```

### Meaning

Every operation in an atomic persistence set completed successfully.

### Payload

```text
ObjectSetPersisted

├── AtomicOperationId
├── Results
│   ├── PersistenceKey
│   ├── OutcomeType
│   └── PersistenceVersion
├── OperationCount
└── CompletedAt
```

### Publication Rule

Published only after every declared operation is committed.

No event is published for partial success because partial success is forbidden.

### Object-Level Events

Storage may publish individual object events in addition to `ObjectSetPersisted`.

When it does:

- all events must share the same `AtomicOperationId`;
- no object-level success event may become visible before atomic commitment;
- consumers must not infer partial commitment from delivery order.

---

## 7.4 ObjectArchived

### Event Type

```text
storage.object.archived
```

### Meaning

An active persistence entry has entered archival state.

### Payload

```text
ObjectArchived

├── PersistenceId
├── PersistenceKey
├── PreviousPersistenceVersion
├── PersistenceVersion
├── ArchiveReason
└── ArchivedAt
```

### Does Not Mean

- the business object is completed;
- the business object is deleted;
- physical storage has been removed.

---

## 7.5 ArchivedObjectRestored

### Event Type

```text
storage.object.archive-restored
```

### Meaning

An archived persistence entry has returned to active persistence.

### Payload

```text
ArchivedObjectRestored

├── PersistenceId
├── PersistenceKey
├── PreviousPersistenceVersion
├── PersistenceVersion
└── RestoredAt
```

### Rule

Owning modules must reevaluate restored business state.

Storage restoration does not imply business reactivation.

---

## 7.6 ObjectLogicallyDeleted

### Event Type

```text
storage.object.logically-deleted
```

### Meaning

A persistence entry has been marked unavailable to normal active-object queries.

### Payload

```text
ObjectLogicallyDeleted

├── PersistenceId
├── PersistenceKey
├── PreviousPersistenceVersion
├── PersistenceVersion
├── DeletionReason
└── DeletedAt
```

### Rule

The physical representation may still exist according to retention policy.

---

## 7.7 ObjectPhysicallyDeleted

### Event Type

```text
storage.object.physically-deleted
```

### Meaning

The active persistence representation has been removed according to the declared implementation guarantee.

### Payload

```text
ObjectPhysicallyDeleted

├── PersistenceId
├── PersistenceKey
├── FinalPersistenceVersion
├── DeletionReason
├── DeletionGuarantee
└── DeletedAt
```

### Rule

The event must not imply secure erasure unless `DeletionGuarantee` explicitly declares it.

---

## 7.8 RetentionInstructionApplied

### Event Type

```text
storage.retention-instruction.applied
```

### Meaning

A retention instruction has been attached to or updated for a persistence entry.

### Payload

```text
RetentionInstructionApplied

├── PersistenceId
├── PersistenceKey
├── PreviousPersistenceVersion
├── PersistenceVersion
├── RetentionClass
├── RetainUntil
├── ArchiveAfter
├── DeleteAfter
├── LegalHold
└── AppliedAt
```

### Rule

The event describes application of the instruction.

It does not claim that retention policy was authored by Storage.

---

# 8. Snapshot Events

---

## 8.1 SnapshotCreated

### Event Type

```text
storage.snapshot.created
```

### Meaning

A consistent persistence snapshot has been created.

### Payload

```text
SnapshotCreated

├── SnapshotId
├── SnapshotScope
├── ConsistencyLevel
├── IncludedObjectCount
├── IncludedPersistenceVersions
└── CreatedAt
```

### Publication Rule

Published only after snapshot integrity is validated.

---

## 8.2 SnapshotRestored

### Event Type

```text
storage.snapshot.restored
```

### Meaning

A snapshot has been restored into a consistent persistence state.

### Payload

```text
SnapshotRestored

├── SnapshotId
├── RestoreMode
├── RestoredScope
├── RestoredObjectCount
├── ConflictPolicy
└── RestoredAt
```

### Does Not Mean

- business workflows resumed;
- Runtime tasks restarted;
- restored business objects are automatically valid for use.

---

## 8.3 SnapshotInvalidated

### Event Type

```text
storage.snapshot.invalidated
```

### Meaning

A previously known snapshot can no longer be used safely.

### Payload

```text
SnapshotInvalidated

├── SnapshotId
├── InvalidationReason
└── InvalidatedAt
```

### Rule

Invalidation must never silently remove required recovery information without policy authorization.

---

# 9. Migration Events

---

## 9.1 StorageMigrationStarted

### Event Type

```text
storage.migration.started
```

### Meaning

An exclusive or declared Storage migration has begun.

### Payload

```text
StorageMigrationStarted

├── MigrationId
├── MigrationScope
├── SourceSchemaVersion
├── TargetSchemaVersion
├── MigrationMode
└── StartedAt
```

### State Relationship

This event normally corresponds to entry into:

```text
Migrating
```

For lazy object migration that does not change capability state,

a more narrowly scoped migration event may be used.

---

## 9.2 ObjectRepresentationMigrated

### Event Type

```text
storage.object-representation.migrated
```

### Meaning

One persisted object representation has been migrated to a new SchemaVersion.

### Payload

```text
ObjectRepresentationMigrated

├── MigrationId
├── PersistenceId
├── PersistenceKey
├── PreviousSchemaVersion
├── SchemaVersion
├── PreviousPersistenceVersion
├── PersistenceVersion
└── MigratedAt
```

### Rule

This event describes representation migration only.

It must not claim that business meaning changed.

---

## 9.3 StorageMigrationCompleted

### Event Type

```text
storage.migration.completed
```

### Meaning

The declared migration scope has completed and passed validation.

### Payload

```text
StorageMigrationCompleted

├── MigrationId
├── MigrationScope
├── SourceSchemaVersion
├── TargetSchemaVersion
├── MigratedObjectCount
├── ValidationResult
└── CompletedAt
```

### Publication Rule

Published only after target representation consistency is confirmed.

---

## 9.4 StorageMigrationFailed

### Event Type

```text
storage.migration.failed
```

### Meaning

A migration could not complete successfully.

### Payload

```text
StorageMigrationFailed

├── MigrationId
├── MigrationScope
├── SourceSchemaVersion
├── TargetSchemaVersion
├── FailureCode
├── Recoverability
└── FailedAt
```

### Rule

The payload must not expose backend-specific failure details.

---

# 10. Recovery Events

---

## 10.1 StorageRecoveryStarted

### Event Type

```text
storage.recovery.started
```

### Meaning

Storage has begun restoring a consistent persistence state.

### Payload

```text
StorageRecoveryStarted

├── RecoveryId
├── RecoveryScope
├── RecoverySource
├── RecoveryReason
└── StartedAt
```

### State Relationship

This event corresponds to entry into:

```text
Recovering
```

---

## 10.2 StorageRecoveryCompleted

### Event Type

```text
storage.recovery.completed
```

### Meaning

Recovery completed and the restored persistence scope passed integrity validation.

### Payload

```text
StorageRecoveryCompleted

├── RecoveryId
├── RecoveryScope
├── RecoverySource
├── RestoredObjectCount
├── ValidationResult
└── CompletedAt
```

### Does Not Mean

- business state was validated;
- Reading Sessions resumed;
- Runtime execution restarted;
- processing pipelines continued.

---

## 10.3 StorageRecoveryPartiallyCompleted

### Event Type

```text
storage.recovery.partially-completed
```

### Meaning

A safe but restricted persistence capability has been restored.

### Payload

```text
StorageRecoveryPartiallyCompleted

├── RecoveryId
├── RestoredScope
├── UnavailableScope
├── AvailableOperations
├── UnavailableOperations
└── CompletedAt
```

### State Relationship

This event normally corresponds to:

```text
Recovering

↓

Degraded
```

---

## 10.4 StorageRecoveryFailed

### Event Type

```text
storage.recovery.failed
```

### Meaning

Storage could not restore a consistent persistence state.

### Payload

```text
StorageRecoveryFailed

├── RecoveryId
├── RecoveryScope
├── FailureCode
├── Recoverability
└── FailedAt
```

### State Relationship

This event normally corresponds to:

```text
Recovering

↓

Failed
```

---

# 11. Capability State Events

---

## 11.1 StorageInitializationStarted

### Event Type

```text
storage.capability.initialization-started
```

### Meaning

Storage capability initialization has begun.

### State Transition

```text
Uninitialized

↓

Initializing
```

---

## 11.2 StorageReady

### Event Type

```text
storage.capability.ready
```

### Meaning

Storage has passed required validation and can provide its full declared contract.

### Payload

```text
StorageReady

├── PreviousState
├── SupportedContractVersion
├── ActiveSchemaVersion
└── ReadyAt
```

### Rule

This event must not be published while persistence consistency remains uncertain.

---

## 11.3 StorageDegraded

### Event Type

```text
storage.capability.degraded
```

### Meaning

Storage remains safely available with an explicitly reduced capability.

### Payload

```text
StorageDegraded

├── PreviousState
├── AvailableOperations
├── UnavailableOperations
├── AffectedScope
├── ReasonCode
└── DegradedAt
```

### Rule

The event must state the logical restriction.

It must not expose backend failure details.

---

## 11.4 StorageCapabilityRestored

### Event Type

```text
storage.capability.restored
```

### Meaning

Storage has returned from Degraded to full Ready capability.

### Payload

```text
StorageCapabilityRestored

├── PreviousState
├── RestoredOperations
├── RestoredScope
└── RestoredAt
```

### State Transition

```text
Degraded

↓

Ready
```

---

## 11.5 StorageFailed

### Event Type

```text
storage.capability.failed
```

### Meaning

Storage can no longer provide a safe supported persistence capability.

### Payload

```text
StorageFailed

├── PreviousState
├── FailureCode
├── AffectedScope
├── Recoverability
└── FailedAt
```

### Rule

A routine object-level error must not publish `StorageFailed`.

This event is reserved for capability-level failure.

---

## 11.6 StorageShutdownStarted

### Event Type

```text
storage.capability.shutdown-started
```

### Meaning

Storage has begun safe capability termination.

### State Transition

```text
Any Non-Terminal State

↓

ShuttingDown
```

---

## 11.7 StorageStopped

### Event Type

```text
storage.capability.stopped
```

### Meaning

Storage capability has terminated and no longer accepts operations.

### State Transition

```text
ShuttingDown

↓

Stopped
```

---

# 12. Consistency and Integrity Events

---

## 12.1 StorageConsistencyViolationDetected

### Event Type

```text
storage.consistency-violation.detected
```

### Meaning

Storage detected that a persistence consistency guarantee may have been violated.

### Payload

```text
StorageConsistencyViolationDetected

├── ViolationId
├── AffectedScope
├── ConsistencyGuarantee
├── DetectionSource
├── Recoverability
└── DetectedAt
```

### Rule

Sensitive implementation details must remain internal.

### State Effect

Depending on scope:

```text
Ready → Degraded
```

or:

```text
Ready → Recovering
```

or:

```text
Ready → Failed
```

---

## 12.2 PersistenceIntegrityValidated

### Event Type

```text
storage.integrity.validated
```

### Meaning

A declared persistence scope passed integrity validation.

### Payload

```text
PersistenceIntegrityValidated

├── ValidationId
├── ValidationScope
├── ValidationMethod
├── ValidatedObjectCount
└── ValidatedAt
```

This event may support migration, recovery, or administrative verification.

---

# 13. Events Not Published

Storage must not publish success events for failed commands.

Examples:

```text
PersistObject failed

→ No ObjectPersisted
```

```text
ReplaceObject failed because of PersistenceVersionConflict

→ No ObjectReplaced
```

```text
PersistObjectSet rolled back

→ No ObjectSetPersisted
```

```text
Migration failed before target validation

→ No StorageMigrationCompleted
```

Failure may still be returned directly through the command contract.

A failure event is published only when it has architectural or integration value.

---

# 14. Event Ordering

Storage guarantees ordering only within declared scopes.

---

## 14.1 Single Object Ordering

Events for the same PersistenceKey must preserve committed PersistenceVersion order.

Example:

```text
ObjectPersisted
PersistenceVersion = 1

↓

ObjectReplaced
PersistenceVersion = 2

↓

ObjectArchived
PersistenceVersion = 3
```

A consumer must never observe version 3 before version 2 within the same ordered event stream.

---

## 14.2 Atomic Operation Ordering

For an atomic object set:

```text
Object-level outcome events

↓

ObjectSetPersisted
```

may be delivered in this logical order where individual events are published.

However, every event must be created only after atomic commitment.

No consumer-visible event may imply partial commitment.

---

## 14.3 Capability Transition Ordering

State events preserve transition order.

Example:

```text
StorageInitializationStarted

↓

StorageReady
```

A later state event must not be published before its preceding accepted transition.

---

## 14.4 Migration Ordering

```text
StorageMigrationStarted

↓

ObjectRepresentationMigrated
    zero or more times

↓

PersistenceIntegrityValidated

↓

StorageMigrationCompleted
```

On failure:

```text
StorageMigrationStarted

↓

StorageMigrationFailed
```

`StorageMigrationCompleted` must not follow `StorageMigrationFailed` for the same MigrationId.

---

## 14.5 Recovery Ordering

```text
StorageRecoveryStarted

↓

PersistenceIntegrityValidated

↓

StorageRecoveryCompleted
```

or:

```text
StorageRecoveryStarted

↓

StorageRecoveryFailed
```

---

## 14.6 No Global Ordering Guarantee

Storage does not guarantee total ordering across:

- unrelated PersistenceKeys;
- independent operations;
- separate Storage scopes;
- unrelated event categories.

Consumers must not rely on global event sequence unless an explicit stream contract provides it.

---

# 15. Event Idempotency

---

## 15.1 Event Identity

Duplicate events are identified primarily by:

```text
EventId
```

Consumers must not treat redelivery as a new persistence outcome.

---

## 15.2 Operation Correlation

Events created by an idempotent command include:

```text
OperationId
```

Multiple deliveries of an event with the same EventId and OperationId represent the same event.

---

## 15.3 Version-Based Protection

For object mutation events,

consumers may additionally use:

```text
PersistenceKey

PersistenceVersion
```

to avoid applying an obsolete event.

PersistenceVersion is a secondary consistency signal,

not a replacement for EventId.

---

## 15.4 Conflicting Duplicate

The same EventId must never refer to different event content.

Detection of conflicting content requires:

```text
EventIdentityConflict
```

and must be treated as an integrity violation.

---

# 16. Event Delivery

---

## 16.1 Delivery Guarantee

The default recommended delivery guarantee is:

```text
At Least Once
```

Consumers must tolerate duplicates.

---

## 16.2 Event Publication Reliability

Storage must preserve the relationship between persistence commitment and event publication.

A recommended pattern is:

```text
Commit Persistence Outcome

and

Record Event for Delivery

within one reliable boundary
```

The implementation may use an outbox or another reliable mechanism.

The public architecture does not require a specific pattern.

---

## 16.3 Delayed Publication

An event may be delivered after its persistence outcome becomes visible.

Consumers must use:

```text
OccurredAt
```

rather than delivery time to understand when the fact became true.

---

## 16.4 Publication Retry

Event publication may be retried.

Retries must preserve:

```text
EventId

EventType

EventVersion

Payload
```

---

## 16.5 Delivery Failure

Failure to deliver an event does not invalidate already committed persistence.

Persisted state remains authoritative.

However, Storage must retain enough publication state to retry according to the configured delivery contract.

---

# 17. Event Consumer Rules

Consumers of Storage events must:

1. Treat events as immutable facts.
2. Tolerate duplicate delivery.
3. Validate EventVersion.
4. Ignore unsupported optional fields safely.
5. Process object events in PersistenceVersion order where ordering matters.
6. Never infer business validity from persistence success.
7. Never infer workflow continuation from snapshot or recovery events.
8. Never depend on backend-specific metadata.
9. Never use delivery time as the persistence occurrence time.
10. Handle delayed events safely.

---

# 18. Event Schema Evolution

---

## 18.1 EventVersion

Every event type has an explicit EventVersion.

Example:

```text
EventType = storage.object.persisted

EventVersion = 1
```

---

## 18.2 Compatible Changes

Compatible changes may include:

- adding optional fields;
- adding optional metadata;
- adding enum values when consumers support unknown-value handling.

---

## 18.3 Breaking Changes

Breaking changes require a new major EventVersion.

Examples include:

- removing required fields;
- changing field meaning;
- changing identity semantics;
- changing ordering semantics;
- changing success meaning.

---

## 18.4 Persistence Schema Independence

EventVersion is independent from SchemaVersion.

```text
EventVersion != SchemaVersion
```

Changing persisted representation does not necessarily require changing the event schema.

---

# 19. Privacy and Security

Storage events must follow data-minimization principles.

---

## 19.1 Payload Exclusion

Storage events must not contain complete persisted application payloads by default.

Events should carry:

- identity;
- persistence version;
- schema version;
- outcome metadata;
- safe operational context.

---

## 19.2 Sensitive Data

Events must never expose:

- credentials;
- private keys;
- access tokens;
- database connection strings;
- encryption secrets;
- raw protected content;
- physical storage paths.

---

## 19.3 Object Identifiers

Where ObjectId is sensitive,

an approved redacted or indirect identifier may be used.

The event must still preserve reliable correlation semantics.

---

## 19.4 Authorization

Receiving a Storage event does not automatically authorize access to the persisted payload.

Payload retrieval remains subject to Storage access rules.

---

# 20. Observability

Every published event should produce structured observability data.

Recommended fields include:

```text
EventId

EventType

EventVersion

OperationId

CorrelationId

SubjectType

ObjectType

Outcome

OccurredAt

PublishedAt

DeliveryAttempt
```

Observability must not duplicate protected payload data.

---

# 21. Event Metrics

Recommended logical metrics include:

```text
storage_event_published_total

storage_event_publication_failed_total

storage_event_delivery_retry_total

storage_event_duplicate_detected_total

storage_event_identity_conflict_total

storage_object_persisted_event_total

storage_object_replaced_event_total

storage_snapshot_created_event_total

storage_migration_completed_event_total

storage_recovery_completed_event_total

storage_capability_state_event_total
```

Backend-specific transport metrics remain implementation details.

---

# 22. Event-to-State Relationship

Events report state transitions defined in `STATES.md`.

They do not independently control state.

Examples:

```text
State transition succeeds:

Initializing → Ready

Then:

StorageReady is published
```

```text
State transition succeeds:

Ready → Recovering

Then:

StorageRecoveryStarted is published
```

Failure to deliver the event does not reverse the already completed state transition.

---

# 23. Event-to-Command Relationship

Commands request behavior.

Events report completed facts.

```text
PersistObject
    │
    ▼
Storage validates and commits
    │
    ▼
ObjectPersisted
```

```text
CreateSnapshot
    │
    ▼
Storage creates and validates snapshot
    │
    ▼
SnapshotCreated
```

A command may fail without producing a public event.

---

# 24. Event-to-Error Relationship

Errors are returned through command and query contracts.

Events are used when the failure itself is an important architectural fact.

Example:

```text
ReplaceObject

↓

PersistenceVersionConflict
```

Normally:

```text
No public event required
```

But:

```text
Storage detects capability-wide consistency violation

↓

StorageConsistencyViolationDetected
```

and potentially:

```text
StorageRecoveryStarted
```

---

# 25. Architecture Invariants

The following invariants always apply.

1. Storage events describe persistence facts only.
2. Storage events never own business semantics.
3. Published events are immutable.
4. Success events are published only after committed outcomes.
5. Rolled-back operations publish no success events.
6. Every event has a unique EventId.
7. Object mutation events carry PersistenceVersion.
8. PersistenceVersion never replaces business revision.
9. EventVersion is independent from SchemaVersion.
10. Backend-specific details never cross the event boundary.
11. Repository-specific details never cross the event boundary.
12. Duplicate delivery must not create duplicate business effects.
13. Ordering is guaranteed only within declared scopes.
14. Global event ordering is never assumed.
15. Storage events do not automatically resume workflows.
16. Snapshot restoration events do not imply business reactivation.
17. Recovery events do not imply Runtime restart.
18. Capability failure events represent capability-level failure only.
19. Routine validation failures do not require failure events.
20. Event publication failure does not invalidate committed persistence.
21. Event retries preserve the original EventId and content.
22. Storage events exclude complete application payloads by default.
23. Commands and events remain separate architectural concepts.
24. State events report completed or accepted state transitions.
25. Event evolution remains versioned and backward-compatible within a major version.

---

# 26. Related Documents

| Document | Responsibility |
|---|---|
| README.md | Storage overview |
| MODULE.md | Ownership and architectural boundaries |
| CONTRACT.md | Commands, queries and persistence guarantees |
| STATES.md | Storage capability lifecycle |
| EVENTS.md | Persistence and capability events |
| ERRORS.md | Persistence failure model |
| MODELS.md | Persistence concepts and event-related structures |
| MIGRATION.md | Schema evolution and migration rules |

---

# 27. Summary

Storage events communicate immutable facts about persistence outcomes and Storage capability state.

They cover:

- object persistence;
- object replacement;
- atomic object sets;
- archival;
- deletion;
- retention;
- snapshots;
- migration;
- recovery;
- capability transitions;
- consistency violations.

Storage events never communicate:

- business decisions;
- business correctness;
- workflow continuation;
- Runtime execution;
- backend implementation details;
- repository mechanics.

Commands request persistence behavior.

Events report completed persistence facts.

Errors report failed operations.

States describe Storage capability availability.

Keeping these concepts separate ensures that CRAI can evolve persistence implementations without coupling business modules to Storage internals.

---

# End of Document