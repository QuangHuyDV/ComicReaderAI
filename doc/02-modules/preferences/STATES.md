# Preferences Module States

- Module: Preferences
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the internal state machine of the Preferences Module.

The Preferences Module manages the lifecycle of configuration data and ensures that only validated, consistent preferences are exposed to other modules.

---

# State Principles

## Single Active State

The module may exist in only one state at any moment.

---

## Deterministic Transitions

The same input and current state must always produce the same next state.

---

## Atomic Updates

Preference updates are applied atomically.

A failed update leaves the previous configuration unchanged.

---

## Immutable Revisions

Every successful update creates a new Preference Revision.

Previous revisions remain immutable.

---

# State Model

```text
          Initialize
               │
               ▼
           Loading
               │
               ▼
             Ready
          ┌────┼─────┐
          │    │     │
          ▼    ▼     ▼
      Updating Migrating Exporting
          │      │        │
          └──────┴────────┘
                 │
                 ▼
               Ready
                 │
         ┌───────┴────────┐
         ▼                ▼
      Failed          Shutdown
```

---

# State Summary

| State | Description |
|--------|-------------|
| Loading | Loading definitions and persisted preferences |
| Ready | Preferences available for use |
| Updating | Applying validated changes |
| Migrating | Migrating schema versions |
| Exporting | Importing or exporting preference data |
| Failed | Internal failure |
| Shutdown | Module terminated |

---

# Loading

## Meaning

Preference definitions and persisted values are being loaded.

## Allowed Inputs

- StorageLoaded
- InitializationFailed

## Exit Conditions

- Loading completed
- Failure detected

## Invariants

- EffectivePreferences unavailable.
- Updates are rejected.

---

# Ready

## Meaning

The module is fully operational.

## Allowed Inputs

- SetPreference
- RemovePreference
- ResetCategory
- ResetScope
- ImportPreferences
- ExportPreferences
- MigrateSchema

## Exit Conditions

- Update requested
- Migration requested
- Export requested
- Internal failure

## Invariants

- EffectivePreferences is valid.
- Preference definitions are immutable.
- Consumers may query configuration.

---

# Updating

## Meaning

Validated preference changes are being applied.

## Allowed Inputs

None.

## Exit Conditions

- Update committed
- Validation failed
- Internal failure

## Invariants

- Updates are atomic.
- Previous revision remains active until commit.
- New revision is unpublished until validation succeeds.

---

# Migrating

## Meaning

Preference schema migration is in progress.

## Allowed Inputs

None.

## Exit Conditions

- Migration completed
- Migration failed

## Invariants

- Schema consistency preserved.
- Partial migration is not allowed.

---

# Exporting

## Meaning

Preference import or export is executing.

## Allowed Inputs

None.

## Exit Conditions

- Operation completed
- Operation failed

## Invariants

- Active configuration remains unchanged.
- Sensitive values are excluded from exports.

---

# Failed

## Meaning

An unrecoverable internal failure occurred.

## Allowed Inputs

- RetryInitialization
- Shutdown

## Exit Conditions

- Successful recovery
- Shutdown

## Invariants

- EffectivePreferences remain read-only.
- No updates are accepted.

---

# Shutdown

## Meaning

The module has been terminated.

## Allowed Inputs

None.

## Exit Conditions

None.

## Invariants

- No further processing occurs.

---

# State Transition Table

| Current State | Event | Next State |
|---------------|-------|------------|
| Loading | StorageLoaded | Ready |
| Loading | InitializationFailed | Failed |
| Ready | SetPreference | Updating |
| Ready | RemovePreference | Updating |
| Ready | ResetCategory | Updating |
| Ready | ResetScope | Updating |
| Ready | ImportPreferences | Exporting |
| Ready | ExportPreferences | Exporting |
| Ready | MigrateSchema | Migrating |
| Ready | InternalFailure | Failed |
| Updating | CommitSucceeded | Ready |
| Updating | CommitFailed | Ready |
| Updating | InternalFailure | Failed |
| Migrating | MigrationCompleted | Ready |
| Migrating | MigrationFailed | Failed |
| Exporting | OperationCompleted | Ready |
| Exporting | OperationFailed | Ready |
| Failed | RetryInitialization | Loading |
| Failed | Shutdown | Shutdown |

---

# Transition Rules

## Validation

Every update is validated before commit.

---

## Revision Creation

Every successful update creates exactly one new Preference Revision.

---

## EffectivePreferences

EffectivePreferences are recalculated only after a successful update.

---

## Import

Imported preferences must be validated before becoming active.

---

## Migration

Migration either succeeds completely or leaves the previous schema unchanged.

---

# Architecture Invariants

1. EffectivePreferences are always valid in Ready state.
2. Only one update operation executes at a time.
3. Failed updates never modify active configuration.
4. Successful updates create a new Preference Revision.
5. Preference definitions remain immutable at runtime.
6. Scope resolution remains deterministic.
7. Sensitive values are never exposed during state transitions.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- EVENTS.md
- ERRORS.md
- README.md
