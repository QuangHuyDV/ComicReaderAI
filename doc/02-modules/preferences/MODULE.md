# Preferences Module

- Module: Preferences
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

The Preferences Module is the central configuration authority of CRAI.

It stores, resolves, validates, and distributes user-defined behavior for reading, recognition, text processing, translation, presentation, performance, and AI-assisted features.

The module provides a single source of truth for configurable system behavior.

It does not implement the behavior controlled by those preferences.

---

# Responsibilities

The Preferences Module is responsible for:

- Defining supported preference keys and value types.
- Storing global preferences.
- Storing source-specific preferences.
- Managing session-specific preference overrides.
- Resolving the effective preference set for a reading session.
- Validating preference values.
- Applying default values.
- Publishing preference change events.
- Preserving compatibility across preference schema versions.
- Preventing invalid configurations from reaching other modules.
- Supporting preference import and export.
- Protecting sensitive configuration values.

---

# Out of Scope

The Preferences Module is not responsible for:

- Rendering a settings interface.
- Executing OCR.
- Translating text.
- Capturing content.
- Rendering translated content.
- Orchestrating the processing pipeline.
- Managing reading session lifecycle.
- Persisting translation history.
- Storing captured images.
- Selecting a provider at runtime based on availability.
- Managing provider credentials directly.

The user interface may edit preferences, but the Preferences Module owns their validation and resolution.

The Reading Session Module consumes resolved preferences and decides when configuration changes require pipeline restart.

---

# Owned Domain

The module owns the following concepts:

- PreferenceDefinition
- PreferenceKey
- PreferenceValue
- PreferenceScope
- PreferenceSet
- PreferenceOverride
- EffectivePreferences
- PreferenceSchema
- PreferenceRevision
- SourcePreferenceProfile
- SessionPreferenceProfile

No other module may directly modify these concepts.

---

# Preference Scopes

Preferences may exist at multiple scopes.

Supported scopes are:

- Default
- Global
- Source
- Session

The effective value is resolved using the following priority:

```text
Session
    ↓
Source
    ↓
Global
    ↓
Default
```

A higher-priority scope overrides a lower-priority scope.

---

# Default Preferences

Default preferences are defined by the application.

They provide valid fallback values when the user has not configured a preference.

Default preferences:

- Are versioned.
- Are read-only at runtime.
- Must always form a valid configuration.
- May change between application versions through migration.

---

# Global Preferences

Global preferences apply across the entire application unless overridden by a more specific scope.

Examples include:

- Default source language.
- Default target language.
- Preferred OCR engine.
- Preferred translation provider.
- Default presentation mode.
- Performance limits.
- Theme-independent reading behavior.

---

# Source Preferences

Source preferences apply to a specific content source.

A source may represent:

- A website domain.
- A document.
- A local folder.
- An application.
- A content provider.
- A user-defined source profile.

Examples:

```text
Source: manhuagui.com
Source Language: Chinese
Presentation Mode: Overlay
OCR Strategy: Comic
```

```text
Source: syosetu.com
Source Language: Japanese
Presentation Mode: Side Panel
Text Mode: Novel
```

Source preferences override global preferences.

---

# Session Preferences

Session preferences apply only to one active reading session.

They are intended for temporary changes.

Examples include:

- Temporarily changing translation provider.
- Temporarily disabling automatic translation.
- Selecting another presentation mode.
- Changing source language for one chapter.
- Testing another OCR strategy.

Session preferences override source and global preferences.

Session preferences must not modify persistent global or source configuration unless explicitly promoted.

---

# Preference Categories

The module manages preferences in the following categories.

## Reading Preferences

Reading preferences control general reading behavior.

Examples:

- Source language.
- Target language.
- Automatic language detection.
- Reading direction.
- Automatic capture.
- Automatic translation.
- Automatic next-page behavior.
- Remember last reading position.
- Content type.
- Reading mode.

---

## Recognition Preferences

Recognition preferences control OCR and visual text recognition behavior.

Examples:

- OCR engine.
- Recognition strategy.
- Minimum confidence.
- Vertical text detection.
- Text orientation detection.
- Region detection.
- Line merging.
- Noise reduction.
- GPU usage.
- Recognition timeout.

---

## Text Processing Preferences

Text Processing preferences control normalization and segmentation behavior.

Examples:

- Unicode normalization.
- Whitespace normalization.
- Paragraph reconstruction.
- Reading-order strategy.
- Sentence segmentation.
- Mixed-language handling.
- Punctuation restoration.
- Maximum segment size.
- Source formatting preservation.

---

## Translation Preferences

Translation preferences control translation behavior.

Examples:

- Translation provider.
- Translation model.
- Translation style.
- Formality.
- Temperature.
- Context size.
- Maximum request size.
- Timeout.
- Retry policy.
- Glossary.
- Prompt template.
- Proper-name handling.
- Honorific handling.
- Terminology locking.

---

## Presentation Preferences

Presentation preferences control how translated content is displayed.

Examples:

- Presentation mode.
- Overlay mode.
- Side panel mode.
- Tooltip mode.
- Dual-language mode.
- Font family.
- Font size.
- Line height.
- Text alignment.
- Text color.
- Background opacity.
- Bubble fitting.
- Overflow behavior.
- Maximum line count.
- Automatic font resizing.

---

## Performance Preferences

Performance preferences control resource usage.

Examples:

- Maximum parallel jobs.
- Maximum image resolution.
- Cache size.
- Lazy processing.
- Preload behavior.
- Battery saver mode.
- GPU usage.
- Memory budget.
- Processing timeout.
- Background processing.

---

## AI Preferences

AI preferences control context-aware processing.

Examples:

- Use surrounding context.
- Use previous-page context.
- Use character memory.
- Use story memory.
- Translation tone.
- Genre profile.
- Terminology memory.
- Glossary enforcement.
- Prompt strategy.
- Context retention limits.

---

# Preference Definition

Every supported preference is described by a PreferenceDefinition.

A PreferenceDefinition contains:

- Preference key.
- Category.
- Data type.
- Default value.
- Allowed scopes.
- Validation rules.
- Whether the value is sensitive.
- Whether a change requires pipeline restart.
- Whether a change affects cache validity.
- Schema version.
- Deprecation status.

Preference definitions are controlled by the application and are not directly editable by users.

---

# Preference Key

Every preference has a stable key.

Example keys:

```text
reading.source_language
reading.target_language
recognition.engine
recognition.minimum_confidence
translation.provider
translation.style
presentation.mode
presentation.font_size
performance.maximum_parallel_jobs
ai.use_story_context
```

Preference keys must remain stable after publication.

Renaming a key requires a migration rule.

---

# Preference Value

A preference value must conform to its definition.

Supported value types may include:

- Boolean
- Integer
- Decimal
- String
- Enumeration
- Duration
- Size
- List
- Map
- Structured object

Unknown or invalid values must never become part of EffectivePreferences.

---

# Effective Preferences

EffectivePreferences is the fully resolved configuration consumed by other modules.

It is built by merging:

```text
Application Defaults
        ↓
Global Preferences
        ↓
Source Preferences
        ↓
Session Overrides
```

EffectivePreferences must:

- Be complete.
- Be valid.
- Be immutable.
- Include its schema version.
- Include its preference revision.
- Identify the scopes used during resolution.
- Contain no unresolved values.

Other modules should consume EffectivePreferences rather than resolving preference scopes independently.

---

# Preference Resolution

Preference resolution is deterministic.

Given the same:

- Preference schema.
- Global preference set.
- Source preference set.
- Session overrides.

the module must always produce the same EffectivePreferences.

Resolution must not depend on event ordering or hidden runtime state.

---

# Revision Ownership

The Preferences Module owns PreferenceRevision.

Every successful preference change increments the relevant revision.

A resolved preference set must include:

- Global revision.
- Source revision when applicable.
- Session revision when applicable.
- Effective revision.

Consumers use revisions to determine whether cached configuration remains valid.

---

# Validation

The module validates preferences before accepting them.

Validation may include:

- Type validation.
- Range validation.
- Enumeration validation.
- Required-value validation.
- Cross-field validation.
- Scope validation.
- Compatibility validation.
- Security validation.

Examples:

- Font size must be greater than zero.
- Minimum confidence must be between zero and one.
- Source and target language must not conflict with unsupported providers.
- GPU-only recognition mode requires GPU support.
- A preference may be disallowed at session scope.

Invalid changes must be rejected atomically.

---

# Change Classification

Every preference change is classified by its system impact.

Supported impact levels include:

- NoRuntimeEffect
- PresentationRefresh
- StageRestart
- PipelineRestart
- SessionRestart
- ApplicationRestart

Examples:

```text
presentation.font_size
Impact: PresentationRefresh
```

```text
translation.provider
Impact: StageRestart
```

```text
recognition.engine
Impact: PipelineRestart
```

The Preferences Module classifies impact.

The Reading Session Module decides when and how the affected work is restarted.

---

# Cache Impact

Preference definitions must state whether a change invalidates cached results.

Examples:

- Font size does not invalidate translation cache.
- Translation provider may invalidate translation cache.
- OCR engine may invalidate recognition and downstream caches.
- Target language invalidates translation and presentation caches.
- Presentation mode invalidates only presentation output.

The Preferences Module reports cache impact but does not manage caches.

---

# Sensitive Preferences

Some preferences may reference sensitive configuration.

Examples:

- Provider credential reference.
- Private endpoint reference.
- Local model path.
- Organization identifier.

The module must not expose secret values through:

- Events.
- Logs.
- Diagnostics.
- Export files.
- Error messages.

Secrets should be stored by a dedicated secure credential mechanism.

Preferences should contain only secure references where possible.

---

# Import and Export

The module may support preference import and export.

Exported preferences must:

- Include schema version.
- Exclude sensitive values.
- Preserve scope information.
- Be validated before import.
- Support migration when possible.

Import must be atomic.

An invalid import must not partially update existing preferences.

---

# Reset Behavior

The module may reset preferences at different scopes.

Supported reset operations may include:

- Reset one preference.
- Reset one category.
- Reset one source profile.
- Reset session overrides.
- Reset all global preferences.

Reset removes the explicit value at the selected scope.

The effective value then falls back to the next available scope.

---

# Dependency Direction

The Preferences Module may be consumed by:

- Reading Session.
- Capture.
- Recognition.
- Text Processing.
- Translation.
- Presentation.
- UI Adapter.
- Storage.
- Diagnostics.

Processing modules should receive resolved preferences through their operation contracts.

They should not directly mutate preferences.

---

# Relationship with Reading Session

Reading Session uses EffectivePreferences to configure pipeline execution.

When preferences change, Preferences publishes the change and its impact classification.

Reading Session determines whether to:

- Continue without restart.
- Refresh Presentation.
- Retry one stage.
- Restart the pipeline.
- Restart the session.

Preferences does not orchestrate pipeline execution.

---

# Relationship with Storage

Preferences owns preference semantics and validation.

Storage owns persistence mechanisms.

Preferences may request Storage to:

- Load preference records.
- Save preference records.
- Delete preference records.
- Store schema migration results.

Storage must not interpret preference meaning.

---

# Relationship with UI Adapter

UI Adapter may:

- Query preference definitions.
- Display current values.
- Submit preference changes.
- Display validation errors.
- Reset preferences.

UI Adapter must not implement preference resolution or validation rules independently.

---

# Event Ownership

The Preferences Module owns events describing preference lifecycle.

Typical events include:

- PreferenceChanged.
- PreferenceReset.
- PreferenceProfileCreated.
- PreferenceProfileDeleted.
- EffectivePreferencesChanged.
- PreferenceMigrationCompleted.
- PreferenceImportCompleted.

Detailed event definitions are provided in `EVENTS.md`.

---

# State Ownership

The Preferences Module owns only its internal configuration lifecycle.

Typical internal states may include:

- Uninitialized.
- Loading.
- Ready.
- Updating.
- Migrating.
- Failed.

It does not own reading session or processing stage states.

Detailed state definitions are provided in `STATES.md`.

---

# Error Ownership

The Preferences Module owns errors related to:

- Invalid preference keys.
- Invalid values.
- Unsupported scopes.
- Resolution failures.
- Schema incompatibility.
- Migration failures.
- Import and export failures.
- Persistence coordination failures.
- Internal invariant violations.

Detailed error definitions are provided in `ERRORS.md`.

---

# Design Principles

## Single Source of Truth

All configurable application behavior must resolve through the Preferences Module.

---

## Scope-Based Overrides

More specific scopes override less specific scopes.

---

## Immutable Effective Configuration

Resolved EffectivePreferences cannot be modified after publication.

---

## Atomic Updates

A preference update either succeeds completely or leaves the previous state unchanged.

---

## Deterministic Resolution

The same preference inputs always produce the same effective configuration.

---

## Separation of Configuration and Execution

Preferences defines configuration.

Other modules execute behavior.

---

## Backward Compatibility

Preference schema changes must define migration or fallback behavior.

---

## Privacy by Default

Sensitive values must never be exposed through ordinary preference contracts.

---

# Performance Goals

The module should:

- Resolve preferences without blocking the processing pipeline.
- Cache validated effective configurations.
- Avoid recomputing unchanged scopes.
- Publish only meaningful changes.
- Support source-specific profiles efficiently.
- Support session overrides with minimal overhead.

---

# Architecture Invariants

The Preferences Module must guarantee:

1. Every supported preference has exactly one stable key.
2. Every accepted value conforms to its PreferenceDefinition.
3. EffectivePreferences is complete and valid.
4. Scope resolution follows Session, Source, Global, Default priority.
5. Invalid updates never partially modify stored preferences.
6. EffectivePreferences is immutable after publication.
7. Every successful change produces a new PreferenceRevision.
8. Sensitive values are never exposed through public events or logs.
9. Other modules do not resolve preference scopes independently.
10. Preferences never orchestrates processing modules.
11. Storage does not interpret preference semantics.
12. Deprecated preferences remain readable until migration or removal is explicitly defined.

---

# Related Documents

- CONTRACT.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
