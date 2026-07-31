# Preferences Module

The Preferences Module is the central configuration authority of CRAI.

It provides a single, validated and versioned source of configuration for every configurable aspect of the application. Rather than allowing each module to maintain its own settings, all runtime behavior is driven by preferences resolved through this module.

The module separates **configuration** from **execution**. It defines *what* the application should do, while other modules decide *when* and *how* to execute those behaviors.

---

# Responsibilities

The Preferences Module is responsible for:

- Managing application preferences.
- Defining supported preference keys.
- Validating preference values.
- Resolving effective configuration.
- Managing preference scopes.
- Managing preference revisions.
- Publishing preference change events.
- Supporting schema migration.
- Supporting preference import and export.

It is not responsible for:

- OCR execution
- Translation
- Screen capture
- Presentation rendering
- Reading session orchestration
- Data persistence implementation

---

# Position in Architecture

```text
                 User
                   │
                   ▼
              UI Adapter
                   │
                   ▼
             Preferences
                   │
      ┌────────────┼────────────┐
      ▼            ▼            ▼
 Reading      Translation   Presentation
 Session          Engine        Engine
      │
      ▼
Capture → Recognition → Text Processing → Translation → Presentation
```

Preferences acts as a shared configuration service for the entire architecture.

---

# Preference Scopes

Configuration is resolved using four scopes.

```text
Session
    ↓
Source
    ↓
Global
    ↓
Default
```

- **Default** — Built into the application.
- **Global** — Applies to the entire application.
- **Source** — Applies to a specific website, document or provider.
- **Session** — Temporary overrides for the active reading session.

The highest-priority available value becomes the effective value.

---

# Preference Categories

The module manages configuration in several logical categories.

## Reading

Examples:

- Source Language
- Target Language
- Reading Direction
- Auto Capture
- Auto Translate
- Reading Mode

---

## Recognition

Examples:

- OCR Engine
- Recognition Strategy
- Confidence Threshold
- Vertical Text Detection
- GPU Usage

---

## Text Processing

Examples:

- Text Normalization
- Reading Order
- Paragraph Reconstruction
- Sentence Segmentation
- Mixed Language Handling

---

## Translation

Examples:

- Translation Provider
- Translation Model
- Translation Style
- Glossary
- Prompt Template
- Timeout
- Retry Policy

---

## Presentation

Examples:

- Presentation Mode
- Overlay Mode
- Side Panel
- Font
- Font Size
- Bubble Layout
- Dual Language Mode

---

## Performance

Examples:

- Cache Size
- Maximum Parallel Jobs
- Image Resolution
- Memory Budget
- Background Processing

---

## AI

Examples:

- Story Context
- Character Memory
- Translation Tone
- Terminology Lock
- Prompt Strategy

---

# Effective Preferences

Other modules do not resolve configuration themselves.

Instead, they consume a fully validated configuration called:

```text
EffectivePreferences
```

It is produced by merging:

```text
Application Defaults
        ↓
Global Preferences
        ↓
Source Preferences
        ↓
Session Overrides
```

EffectivePreferences is immutable after publication.

---

# Revision Management

Every successful preference update creates a new Preference Revision.

Consumers may compare revisions to determine whether cached configuration is still valid.

---

# Validation

Before a preference becomes active it is validated for:

- Key existence
- Data type
- Value range
- Enumeration values
- Scope compatibility
- Cross-field compatibility
- Schema compatibility

Invalid updates are rejected without modifying the active configuration.

---

# Interaction with Reading Session

Reading Session consumes EffectivePreferences to configure the processing pipeline.

When preferences change, the Preferences Module publishes the change and its impact classification.

Reading Session decides whether to:

- Continue processing
- Refresh Presentation
- Restart a processing stage
- Restart the pipeline
- Restart the session

Preferences never orchestrates processing.

---

# Interaction with Storage

Storage persists preference data.

Preferences owns:

- Validation
- Resolution
- Schema
- Revision management

Storage owns:

- Saving
- Loading
- Physical persistence

---

# Interaction with UI Adapter

The UI Adapter may:

- Display available preferences.
- Show validation errors.
- Edit preference values.
- Reset preferences.
- Import or export preference profiles.

Business rules remain inside the Preferences Module.

---

# Event Model

The module publishes events describing the preference lifecycle.

Typical events include:

- PreferenceChanged
- PreferenceRemoved
- PreferenceReset
- EffectivePreferencesChanged
- PreferenceImported
- PreferenceMigrationCompleted

Other modules react to these events instead of polling configuration.

---

# State Management

The internal lifecycle consists of states such as:

- Loading
- Ready
- Updating
- Migrating
- Exporting
- Failed

These states are internal to the module and independent of the Reading Session lifecycle.

---

# Error Handling

Typical error categories include:

- Validation errors
- Scope errors
- Schema errors
- Migration errors
- Import/export errors
- Internal failures

Failed updates never partially modify the active configuration.

---

# Design Principles

## Single Source of Truth

All configurable behavior originates from the Preferences Module.

---

## Scope-Based Resolution

More specific scopes override less specific scopes.

---

## Atomic Updates

Configuration changes are committed completely or not at all.

---

## Deterministic Resolution

The same inputs always produce the same effective configuration.

---

## Separation of Concerns

Preferences defines configuration.

Other modules execute behavior.

---

## Backward Compatibility

Schema evolution must preserve compatibility through migration or fallback rules.

---

# Performance Goals

The module should:

- Resolve configuration efficiently.
- Cache validated EffectivePreferences.
- Avoid unnecessary recalculation.
- Publish only meaningful changes.
- Scale to multiple source profiles and concurrent sessions.

---

# Related Documents

| Document | Description |
|----------|-------------|
| MODULE.md | Responsibilities and architecture |
| CONTRACT.md | Public contracts |
| EVENTS.md | Published and consumed events |
| STATES.md | Internal state machine |
| ERRORS.md | Error definitions |

---

# Summary

The Preferences Module is the configuration backbone of CRAI.

It centralizes application settings, validates configuration, resolves effective preferences across multiple scopes, manages revisions, and provides every other module with a consistent, deterministic and versioned configuration model.
