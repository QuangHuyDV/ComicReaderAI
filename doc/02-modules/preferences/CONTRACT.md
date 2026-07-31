# Preferences Module Contract

- Module: Preferences
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the public contract of the Preferences Module.

The Preferences Module is the authoritative provider of application configuration. It validates, resolves and publishes effective preferences for all other modules.

---

# Public Commands

## SetPreference

Purpose

Create or update a preference value.

Required Data

- Preference Key
- Preference Value
- Preference Scope

Result

- Preference Revision incremented
- Effective preferences recalculated

---

## RemovePreference

Purpose

Remove an explicit preference from a scope.

Result

The effective value falls back to the next lower-priority scope.

---

## ResetCategory

Purpose

Reset all preferences in a category.

Examples

- Reading
- Recognition
- Translation
- Presentation
- Performance
- AI

---

## ResetScope

Purpose

Remove every preference in a scope.

Supported Scopes

- Session
- Source
- Global

---

## ImportPreferences

Purpose

Import a preference set.

Result

Imported values are validated before being applied.

---

## ExportPreferences

Purpose

Export the current preference set.

Sensitive values must never be exported.

---

# Public Queries

## GetPreference

Returns the resolved value of a single preference.

---

## GetPreferenceDefinition

Returns

- Key
- Type
- Category
- Default Value
- Validation Rules
- Allowed Scopes
- Restart Impact

---

## GetEffectivePreferences

Returns the complete resolved configuration used by the application.

---

## ListPreferenceDefinitions

Returns every supported preference definition.

---

## ListSourceProfiles

Returns all source-specific preference profiles.

---

# Consumed Events

| Event | Purpose |
|--------|---------|
| SessionCreated | Create session overrides when needed |
| SessionClosed | Remove session overrides |
| StorageLoaded | Load persisted preferences |
| ImportCompleted | Activate imported preferences |

---

# Published Events

| Event | Purpose |
|--------|---------|
| PreferenceChanged | Preference updated |
| PreferenceRemoved | Explicit value removed |
| PreferenceReset | Preference reset |
| EffectivePreferencesChanged | Effective configuration changed |
| PreferenceImported | Import completed |
| PreferenceExported | Export completed |
| PreferenceMigrationCompleted | Schema migration completed |

---

# Data Contracts

## PreferenceDefinition

Contains

- Key
- Category
- Data Type
- Default Value
- Allowed Scopes
- Validation Rules
- Restart Impact
- Cache Impact
- Schema Version

---

## PreferenceValue

Contains

- Key
- Value
- Scope
- Revision
- UpdatedAt

---

## EffectivePreferences

Contains

- Preference Revision
- Schema Version
- Fully Resolved Values
- Resolution Metadata

---

# Scope Resolution Contract

Preferences are resolved using the following priority:

```text
Session
    ↓
Source
    ↓
Global
    ↓
Default
```

Every resolved value originates from exactly one scope.

---

# Validation Contract

Before accepting an update the module validates:

- Preference key
- Data type
- Allowed range
- Enumeration value
- Scope
- Cross-field compatibility
- Schema compatibility

Invalid updates are rejected atomically.

---

# Change Impact Contract

Every preference definition declares one impact level.

Supported impacts:

- NoRuntimeEffect
- PresentationRefresh
- StageRestart
- PipelineRestart
- SessionRestart
- ApplicationRestart

The Preferences Module classifies the impact.

Reading Session decides how to apply it.

---

# Cache Contract

Preference definitions declare cache impact.

Examples

- Font size does not invalidate translation cache.
- OCR engine invalidates recognition cache.
- Target language invalidates translation cache.
- Presentation mode invalidates presentation cache.

---

# Revision Contract

Every successful change creates a new Preference Revision.

Revisions are monotonically increasing.

Consumers may use revisions to detect stale configuration.

---

# Version Contract

Every contract includes:

- Contract Version
- Preference Schema Version
- Preference Revision

Older schema versions should remain readable through migration whenever possible.

---

# Error Contract

Commands may return:

- Validation Error
- Scope Error
- Schema Error
- Import Error
- Export Error
- Migration Error
- Internal Error

Detailed definitions are provided in `ERRORS.md`.

---

# Security Contract

The module must never expose:

- API keys
- Authentication tokens
- Secrets
- Provider credentials

Public contracts expose only safe configuration values or secure references.

---

# Architecture Invariants

1. Every preference key is unique.
2. Every accepted value satisfies its definition.
3. EffectivePreferences is complete.
4. Scope resolution is deterministic.
5. Successful updates create a new Preference Revision.
6. Invalid updates never partially modify stored preferences.
7. Sensitive values are never published.
8. Other modules consume resolved preferences instead of resolving scopes themselves.

---

# Related Documents

- MODULE.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
