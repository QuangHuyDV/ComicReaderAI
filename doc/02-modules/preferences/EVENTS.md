# Preferences Module Events

- Module: Preferences
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines all events consumed and published by the Preferences Module.

The Preferences Module owns the lifecycle of application preferences and notifies other modules whenever effective configuration changes.

---

# Event Principles

## Event-Driven Communication

The Preferences Module communicates configuration changes through immutable events.

---

## Immutable Events

Published events must never be modified after publication.

---

## Revision Awareness

Every event must include:

- PreferenceRevision
- SchemaVersion
- Timestamp

---

## Deterministic Resolution

Only validated preference changes may generate events.

Rejected changes never publish events.

---

# Event Naming Convention

Events use the past-tense convention.

Examples

```text
PreferenceChanged
PreferenceReset
PreferenceImported
EffectivePreferencesChanged
```

---

# Consumed Events

## StorageLoaded

Purpose

Load persisted preference data.

---

## SessionCreated

Purpose

Initialize session-specific preference overrides.

---

## SessionClosed

Purpose

Release session-specific preference overrides.

---

## ImportRequested

Purpose

Import a preference profile.

---

## MigrationRequested

Purpose

Upgrade stored preferences to a newer schema version.

---

# Published Events

## PreferenceChanged

Purpose

A preference value has been created or updated.

---

## PreferenceRemoved

Purpose

An explicit preference value has been removed.

---

## PreferenceReset

Purpose

One or more preferences have been reset to a lower-priority scope.

---

## EffectivePreferencesChanged

Purpose

The resolved configuration has changed.

Consumers should refresh cached configuration.

---

## PreferenceImported

Purpose

A preference profile has been imported successfully.

---

## PreferenceExported

Purpose

A preference profile has been exported successfully.

---

## PreferenceMigrationCompleted

Purpose

Preference schema migration completed successfully.

---

## PreferenceValidationFailed

Purpose

A requested preference update failed validation.

No configuration changes were applied.

---

# Event Ordering

Normal update sequence

```text
PreferenceChanged
        ↓
EffectivePreferencesChanged
```

Import sequence

```text
ImportRequested
        ↓
PreferenceImported
        ↓
EffectivePreferencesChanged
```

Migration sequence

```text
MigrationRequested
        ↓
PreferenceMigrationCompleted
        ↓
EffectivePreferencesChanged
```

---

# Event Ordering Rules

1. PreferenceChanged always precedes EffectivePreferencesChanged.
2. PreferenceReset always precedes EffectivePreferencesChanged.
3. PreferenceImported always precedes EffectivePreferencesChanged.
4. Validation failures never publish EffectivePreferencesChanged.
5. Every published event references the latest PreferenceRevision.

---

# Event Idempotency

Duplicate events must not produce duplicate configuration updates.

Events are considered identical when:

- EventId matches.
- PreferenceRevision matches.
- SchemaVersion matches.

---

# Event Versioning

Every event includes:

- EventVersion
- PreferenceRevision
- SchemaVersion

Consumers should ignore unknown optional fields.

---

# Event Delivery

Delivery guarantees:

- At-least-once delivery.
- Ordered by PreferenceRevision.
- Immutable after publication.

---

# Event Failure Handling

If an event cannot be processed:

- Reject invalid payloads.
- Ignore obsolete revisions.
- Retry transient delivery failures.
- Never partially apply preference updates.

---

# Event Dependencies

```text
PreferenceChanged
        ↓
EffectivePreferencesChanged
        ↓
Reading Session
        ↓
Processing Modules
```

---

# Architecture Invariants

1. Every event references one PreferenceRevision.
2. Events are immutable.
3. Only successful updates publish events.
4. EffectivePreferencesChanged represents a fully validated configuration.
5. Duplicate events never produce duplicate state changes.

---

# Future Events

Future versions may introduce:

- PreferenceProfileCreated
- PreferenceProfileDeleted
- SourceProfileActivated
- SourceProfileDeactivated
- PreferenceConflictDetected
- PreferenceConflictResolved

---

# Related Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- ERRORS.md
- README.md
