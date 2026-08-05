# Configuration Infrastructure Module

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Configuration  
> **Document:** Module Definition  
> **Path:** `03-infrastructure/configuration/MODULE.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-04

---

## 1. Purpose

The Configuration infrastructure module provides a centralized, typed, versioned, validated, and observable mechanism for loading and distributing application configuration across CRAI.

Its primary responsibilities are:

- configuration source discovery;
- configuration loading;
- configuration precedence;
- configuration merging;
- typed binding;
- schema validation;
- environment resolution;
- configuration snapshots;
- configuration revisioning;
- runtime-safe refresh;
- change notification;
- module-scoped configuration;
- feature configuration;
- provider configuration references;
- persistence and serialization boundaries;
- redaction and safe observability;
- configuration fallback;
- invalid configuration handling.

Configuration is shared infrastructure.

It does not own the business meaning of every setting.

Each consuming module owns the semantic interpretation and domain validation of its own configuration section.

The central flow is:

```text
Configuration Sources
        ↓
Load
        ↓
Normalize
        ↓
Merge by Precedence
        ↓
Validate
        ↓
Create Immutable Snapshot
        ↓
Publish Revision
        ↓
Consumers Read or Refresh
```

---

## 2. Architectural Position

Configuration sits below application and domain modules.

```text
Application Modules
Infrastructure Modules
Runtime
Provider Management
Translation
Recognition
Presentation
Reading Session
        │
        ▼
Configuration Infrastructure
        │
        ├── Default Files
        ├── User Configuration
        ├── Environment Variables
        ├── Command-Line Arguments
        ├── Remote Configuration
        └── Runtime Overrides
```

Configuration provides values and immutable snapshots.

Consumers remain responsible for their own behavior.

---

## 3. Module Goal

The module must ensure that CRAI configuration is:

- explicit;
- typed;
- validated;
- versioned;
- deterministic;
- inspectable;
- reloadable where safe;
- immutable once published as a snapshot;
- isolated by module;
- free from accidental secret exposure;
- portable across desktop and future deployment modes.

The intended consumer interaction is:

```text
Consumer requests configuration section
        ↓
Configuration returns immutable typed snapshot
        ↓
Consumer validates domain-specific semantics
        ↓
Consumer applies supported changes
```

Consumers must not read arbitrary files or environment variables directly once the Configuration module exists.

---

## 4. Core Principles

### 4.1 Configuration is data

Configuration values are data, not executable code.

Configuration must not:

- execute arbitrary scripts;
- instantiate arbitrary classes;
- load untrusted plugins implicitly;
- alter module ownership;
- bypass security policy;
- inject runtime logic.

---

### 4.2 Immutable snapshots

Every published configuration view is immutable.

A refresh creates a new snapshot and a new revision.

```text
ConfigurationSnapshot R17
        ↓ change detected
ConfigurationSnapshot R18
```

Consumers may continue using the old snapshot until they accept the new one.

---

### 4.3 Typed access

Consumers should request typed configuration sections.

Preferred:

```text
TranslationConfiguration
ProviderManagementConfiguration
RuntimeConfiguration
PresentationConfiguration
```

Avoid:

```text
config["some.path.with.string"]
```

String-key access may exist internally or for diagnostics but should not be the primary contract.

---

### 4.4 Deterministic precedence

When the same key appears in multiple sources, precedence must be explicit and stable.

A recommended precedence is:

```text
Built-in Defaults
        ↓ overridden by
Application Configuration File
        ↓ overridden by
User Configuration File
        ↓ overridden by
Environment Variables
        ↓ overridden by
Command-Line Arguments
        ↓ overridden by
Approved Runtime Overrides
```

Secret values are resolved through Secret Management rather than stored directly in ordinary configuration.

---

### 4.5 Module ownership

Configuration owns configuration mechanics.

Each module owns:

- its configuration schema;
- semantic rules;
- domain defaults where appropriate;
- supported reload behavior;
- restart requirements;
- compatibility behavior.

Configuration must not invent business defaults for another module.

---

### 4.6 Safe refresh

Not every setting may be applied live.

Configuration must classify changes as:

```text
LIVE_APPLICABLE
REQUIRES_COMPONENT_RESTART
REQUIRES_APPLICATION_RESTART
REJECTED
```

The owning consumer decides how to apply a supported change.

---

## 5. Responsibilities

Configuration owns the following responsibilities.

### 5.1 Configuration source registry

The module maintains a registry of approved configuration sources.

Possible source types:

```text
BUILT_IN_DEFAULTS
APPLICATION_FILE
USER_FILE
ENVIRONMENT_VARIABLES
COMMAND_LINE
REMOTE_CONFIGURATION
RUNTIME_OVERRIDE
TEST_CONFIGURATION
```

Each source declares:

- identity;
- source type;
- precedence;
- availability;
- format;
- reload support;
- trust level;
- scope;
- error policy.

---

### 5.2 Source loading

Configuration loads supported source formats.

Possible initial formats:

```text
YAML
JSON
TOML
ENVIRONMENT
COMMAND_LINE
```

Format support must be explicit.

Unknown or unsupported formats must fail safely.

---

### 5.3 Configuration normalization

Source-specific data is normalized into a provider-independent internal representation.

Normalization includes:

- key-path normalization;
- primitive type normalization;
- collection normalization;
- null handling;
- source metadata;
- origin tracking;
- path normalization where applicable;
- value redaction classification.

---

### 5.4 Configuration merging

The module merges sources by deterministic precedence.

The merge process must preserve origin metadata.

Conceptual output:

```text
ResolvedValue {
    key
    value
    winningSource
    overriddenSources[]
}
```

Merge rules for objects, arrays, and scalars must be explicit.

---

### 5.5 Typed binding

Configuration binds normalized values into typed module configuration contracts.

Examples:

```text
RuntimeConfiguration
TranslationConfiguration
ProviderManagementConfiguration
PresentationConfiguration
```

Binding failures produce normalized configuration errors.

---

### 5.6 Schema validation

The module validates structural configuration rules such as:

- required fields;
- type compatibility;
- enum membership;
- numeric ranges;
- collection shape;
- unknown fields;
- duplicate keys;
- invalid nullability;
- unsupported schema version.

Domain-specific semantic validation remains owned by the consuming module.

---

### 5.7 Configuration snapshot

The module creates immutable configuration snapshots.

A snapshot includes:

- snapshot identity;
- revision;
- source revisions;
- schema versions;
- load timestamp;
- resolved module sections;
- origin metadata;
- redaction metadata;
- validation status;
- compatibility status.

---

### 5.8 Configuration revisioning

Every accepted configuration change creates a new monotonically ordered revision.

Conceptual identities:

```text
ConfigurationSnapshotId
ConfigurationRevision
ConfigurationSourceId
ConfigurationSourceRevision
ConfigurationSectionId
ConfigurationSectionRevision
```

Revisions support:

- stale update protection;
- change detection;
- audit;
- rollback;
- reproducibility;
- support diagnostics.

---

### 5.9 Module-scoped access

Consumers request only their configuration scope.

Examples:

```text
configuration.getSection("translation")
configuration.getSection("provider-management")
configuration.getSection("runtime")
```

A module must not require access to unrelated sections.

---

### 5.10 Change detection

The module detects changes through approved mechanisms such as:

- file watcher;
- explicit reload command;
- remote source revision update;
- administrative override;
- environment restart;
- test injection.

File watching is optional and platform-dependent.

---

### 5.11 Change classification

Configuration compares old and new snapshots and produces a change set.

Conceptual structure:

```text
ConfigurationChangeSet {
    previousRevision
    currentRevision
    changedSections[]
    changedKeys[]
    addedKeys[]
    removedKeys[]
    changeClassification
}
```

Secret values must not appear in change payloads.

---

### 5.12 Refresh coordination

Configuration coordinates refresh notification.

It does not force consumers to apply every change.

Consumers may:

- accept immediately;
- defer;
- reject;
- require restart;
- continue with previous snapshot;
- enter degraded mode.

---

### 5.13 Runtime overrides

The module may support approved runtime overrides.

Runtime overrides must be:

- explicit;
- scoped;
- auditable;
- revisioned;
- reversible;
- validated;
- non-secret;
- restricted by policy.

Temporary overrides must not silently mutate persisted user configuration.

---

### 5.14 Default configuration

Built-in defaults provide a valid baseline where possible.

Defaults must be:

- documented;
- deterministic;
- versioned with the application;
- safe;
- minimal;
- free from credentials;
- suitable for first-run behavior.

A module may require explicit user configuration when no safe default exists.

---

### 5.15 Configuration diagnostics

The module provides safe diagnostics such as:

- loaded source list;
- source precedence;
- active revision;
- failed source list;
- changed section list;
- restart-required settings;
- unresolved configuration references;
- redacted effective configuration.

Diagnostics must not expose secrets.

---

### 5.16 Rollback support

The module may support rollback to a previously accepted snapshot.

Rollback must create a new revision.

```text
R18 active
    ↓ rollback to content of R16
R19 created
```

Revision numbers must never move backward.

---

## 6. Non-Responsibilities

Configuration does not own the following responsibilities.

### 6.1 Secret storage

It does not persist raw:

- API keys;
- access tokens;
- refresh tokens;
- passwords;
- private keys;
- client secrets.

Configuration stores only approved secret references.

Secret Management owns secret storage and resolution.

---

### 6.2 Domain policy

Configuration does not decide:

- which translation provider is best;
- whether a Translation result is authoritative;
- how OCR works;
- how Runtime schedules jobs;
- how Presentation lays out text;
- how Reading Session navigates;
- how Provider Health is calculated.

It only supplies validated configuration values.

---

### 6.3 Runtime execution

Configuration does not own:

- workers;
- queues;
- scheduling;
- retry timers;
- cancellation propagation;
- resource admission;
- thread management;
- process management.

Runtime consumes Runtime configuration.

---

### 6.4 Application lifecycle

Configuration may indicate restart requirements, but it does not own application restart orchestration.

Lifecycle or Application Host owns restart and shutdown.

---

### 6.5 Filesystem implementation

Configuration may load files through a filesystem abstraction.

It does not own generic file access, file permissions, or filesystem watching infrastructure.

---

### 6.6 Remote configuration transport

Configuration may consume a remote configuration source.

Networking owns transport.

Authentication may use Secret Management.

---

### 6.7 User interface

Configuration does not own settings screens, forms, visual validation, or user interaction.

UI consumes configuration metadata and commands.

---

### 6.8 Feature business semantics

Feature toggles may be stored by Configuration, but the owning module decides what the feature changes semantically.

---

## 7. Core Domain Concepts

The module distinguishes:

```text
ConfigurationSource
ConfigurationSchema
ConfigurationSection
ConfigurationSnapshot
ConfigurationRevision
ConfigurationChangeSet
ConfigurationOverride
ConfigurationBinding
ConfigurationValidationResult
ConfigurationOrigin
ConfigurationCompatibilityResult
```

These concepts must not be treated as interchangeable.

---

## 8. Configuration Source

A `ConfigurationSource` represents one approved origin of configuration data.

Conceptual structure:

```text
ConfigurationSource {
    configurationSourceId
    sourceType
    precedence
    scope
    format
    reloadMode
    trustLevel
    locationReference
    enabled
}
```

`locationReference` must not expose secrets.

---

## 9. Configuration Schema

A `ConfigurationSchema` defines structural expectations for one configuration section.

Conceptual structure:

```text
ConfigurationSchema {
    schemaId
    moduleId
    schemaVersion
    requiredFields[]
    optionalFields[]
    typeRules[]
    validationRules[]
    unknownFieldPolicy
}
```

Schemas should be versioned independently when necessary.

---

## 10. Configuration Section

A section represents configuration owned by one module or infrastructure capability.

Examples:

```text
application
runtime
translation
provider-management
presentation
recognition
logging
metrics
tracing
cache
storage
```

Section ownership must be explicit.

---

## 11. Configuration Snapshot

A `ConfigurationSnapshot` is the immutable effective configuration at one revision.

Conceptual structure:

```text
ConfigurationSnapshot {
    configurationSnapshotId
    configurationRevision

    createdAt
    sourceRevisions[]
    schemaVersions[]

    sections[]
    validationStatus
    compatibilityStatus

    redactionMetadata
}
```

A snapshot must remain reproducible from recorded sources where retention policy permits.

---

## 12. Configuration Change Set

A change set compares two accepted snapshots.

```text
ConfigurationChangeSet {
    previousSnapshotId
    currentSnapshotId

    changedSections[]
    changedKeys[]
    addedKeys[]
    removedKeys[]

    liveApplicableKeys[]
    componentRestartKeys[]
    applicationRestartKeys[]
    rejectedKeys[]
}
```

Values should not be embedded by default.

---

## 13. Configuration Override

A runtime override is a bounded value replacement.

```text
ConfigurationOverride {
    configurationOverrideId
    targetSection
    targetKey
    valueReference
    scope
    reason
    createdBy
    createdAt
    expiresAt
}
```

Overrides must not contain raw secrets.

---

## 14. Configuration Origin

Each effective value should preserve origin metadata.

```text
ConfigurationOrigin {
    sourceId
    sourceRevision
    sourceType
    originalKey
    precedence
}
```

This enables diagnostics such as:

```text
runtime.maxWorkers came from USER_FILE
and overrode APPLICATION_FILE
```

---

## 15. Configuration Precedence

Recommended default precedence:

```text
1. Built-in Defaults
2. Application Configuration File
3. User Configuration File
4. Environment Variables
5. Command-Line Arguments
6. Approved Runtime Overrides
```

Higher-numbered sources override lower-numbered sources.

Remote configuration placement must be explicitly decided.

It may be inserted between user configuration and environment variables, or treated as a separate policy-controlled source.

---

## 16. Merge Semantics

### Scalars

Higher-precedence scalar replaces lower-precedence scalar.

### Objects

Objects merge recursively unless the schema declares replace semantics.

### Arrays

Array behavior must be schema-defined:

```text
REPLACE
APPEND
UNION
MERGE_BY_IDENTITY
```

Default array behavior should be:

```text
REPLACE
```

because implicit concatenation is difficult to reason about.

### Null values

Null behavior must be explicit:

```text
SET_NULL
REMOVE_VALUE
IGNORE_NULL
INVALID
```

---

## 17. Unknown Field Policy

Possible policies:

```text
REJECT
WARN
IGNORE
PRESERVE
```

Recommended defaults:

```text
Production:
    WARN or REJECT for owned sections

Forward-compatible external metadata:
    PRESERVE or IGNORE
```

The policy belongs to each schema.

---

## 18. Environment Variables

Environment-variable mapping must be deterministic.

Example convention:

```text
CRAI_RUNTIME_MAX_WORKERS
    ↓
runtime.maxWorkers
```

Mapping rules must define:

- prefix;
- separator;
- case normalization;
- arrays;
- booleans;
- nulls;
- escaped values.

Environment variables must not be used as an uncontrolled secret database.

Secret values should still be referenced through Secret Management where possible.

---

## 19. Command-Line Configuration

Command-line arguments may override selected configuration values.

Command-line support should be limited to:

- startup diagnostics;
- profile selection;
- configuration file location;
- safe operational overrides;
- development or testing options.

Sensitive values should not be supplied directly because command-line values may appear in process listings or logs.

---

## 20. Configuration Profiles

The module may support named profiles such as:

```text
development
test
production
desktop
offline
diagnostic
```

Profiles select or extend source sets.

Profiles must not create hidden business behavior.

The active profile must be observable.

---

## 21. Configuration and Secrets

Ordinary configuration uses secret references.

Example:

```text
providerCredentialReferenceId = "credential-openai-default"
```

not:

```text
apiKey = "..."
```

Configuration validates the reference format.

Secret Management validates availability and resolves secret material.

---

## 22. Configuration and Provider Management

Provider Management consumes configuration for:

- provider definitions;
- provider adapter binding;
- model metadata;
- provider enablement defaults;
- credential references;
- region policy;
- local model paths;
- health policy;
- circuit policy;
- lease defaults.

Configuration does not own Provider state.

A configuration change may request a Provider Management update, but Provider Management performs its own validation and state transition.

---

## 23. Configuration and Runtime

Runtime consumes configuration for:

- worker limits;
- queue limits;
- retry policy defaults;
- timeout defaults;
- resource thresholds;
- scheduler tuning;
- cancellation grace periods;
- observability controls.

Configuration does not mutate active Runtime state directly.

Runtime decides whether a change is live-applicable.

---

## 24. Configuration and Translation

Translation consumes configuration for:

- default target language;
- translation profiles;
- batching limits;
- context defaults;
- publication defaults;
- fallback policy defaults;
- cache behavior;
- feature toggles.

Translation owns semantic validation.

---

## 25. Configuration and Presentation

Presentation consumes configuration for:

- default presentation mode;
- typography defaults;
- layout limits;
- overlay behavior;
- accessibility preferences;
- fallback display behavior.

Presentation owns rendering semantics.

---

## 26. Configuration and Recognition

Recognition consumes configuration for:

- OCR provider preference;
- recognition thresholds;
- preprocessing limits;
- language hints;
- local/remote policy;
- confidence thresholds.

Recognition owns recognition semantics.

---

## 27. Configuration Loading Lifecycle

Conceptual lifecycle:

```text
DISCOVERING
    ↓
LOADING
    ↓
NORMALIZING
    ↓
MERGING
    ↓
BINDING
    ↓
VALIDATING
    ↓
PUBLISHED
```

Failure paths:

```text
LOAD_FAILED
NORMALIZATION_FAILED
MERGE_FAILED
BINDING_FAILED
VALIDATION_FAILED
```

Detailed state definitions belong in `STATES.md` if the module requires a dedicated state document.

---

## 28. Startup Behavior

At startup:

1. discover approved sources;
2. load sources by precedence;
3. normalize values;
4. merge values;
5. bind typed sections;
6. validate schemas;
7. validate cross-section structural rules;
8. create immutable snapshot;
9. publish active revision;
10. notify consumers.

If required configuration is invalid, startup behavior may be:

```text
FAIL_FAST
START_DEGRADED
USE_LAST_KNOWN_GOOD
USE_SAFE_DEFAULTS
```

Policy must be explicit.

---

## 29. Last Known Good Configuration

The module may persist a last-known-good snapshot.

It may be used when:

- user file is corrupted;
- remote configuration is unavailable;
- a refreshed configuration fails validation;
- source loading fails temporarily.

Use of last-known-good must be observable.

It must not silently override security-critical changes.

---

## 30. Reload Behavior

A reload creates a new candidate snapshot.

```text
Active Snapshot
        ↓
Load Candidate
        ↓
Validate Candidate
        ↓
Compare
        ↓
Publish or Reject
```

The active snapshot remains unchanged until the candidate is accepted.

---

## 31. Candidate Configuration

A candidate snapshot is not yet authoritative.

It may be:

```text
VALID
INVALID
INCOMPATIBLE
REQUIRES_RESTART
PARTIALLY_APPLICABLE
```

Consumers must not read a candidate through the normal active configuration query.

---

## 32. Consumer Acceptance

For live changes, consumers may report:

```text
ACCEPTED
DEFERRED
REQUIRES_COMPONENT_RESTART
REQUIRES_APPLICATION_RESTART
REJECTED
```

Configuration may track acceptance status per consumer.

It must not pretend that publication means every component already applied the change.

---

## 33. Partial Application

Partial application must be handled carefully.

Possible strategies:

```text
ATOMIC_GLOBAL
ATOMIC_PER_SECTION
BEST_EFFORT
RESTART_REQUIRED
```

Recommended default:

```text
ATOMIC_PER_SECTION
```

A module section should not be partially applied unless its schema explicitly supports it.

---

## 34. Restart Requirements

Configuration changes may require:

```text
NONE
COMPONENT_RESTART
APPLICATION_RESTART
```

Restart requirements should be declared by the owning module.

Configuration aggregates and reports them.

---

## 35. Feature Flags

Configuration may store feature flags.

A feature flag should include:

```text
FeatureFlag {
    flagId
    enabled
    scope
    rolloutPolicy
    ownerModule
    revision
}
```

Advanced experimentation and rollout may belong to a future feature-management subsystem.

For MVP, feature flags should remain simple and deterministic.

---

## 36. Configuration Validation Layers

Validation should occur in layers.

### Layer 1 — Parse validation

Can the source be parsed?

### Layer 2 — Structural validation

Does the data match the schema?

### Layer 3 — Cross-field validation

Are fields internally consistent?

### Layer 4 — Cross-section validation

Are dependent sections compatible?

### Layer 5 — Consumer semantic validation

Can the owning module use the configuration?

Configuration owns Layers 1–4 structurally.

Consumers own domain-specific Layer 5 validation.

---

## 37. Cross-Section Validation

Examples:

```text
provider-management.localOnly = true
+
network.remoteProviderEnabled = true
```

may be structurally valid but semantically conflicting under application policy.

Cross-section validation must remain limited to infrastructure-level compatibility.

It must not absorb all domain policy.

---

## 38. Schema Versioning

Each configuration section may have a schema version.

Example:

```text
translation:
    schemaVersion: 2
```

The module may support:

- current version;
- selected previous versions;
- explicit migrations;
- rejection of unsupported versions.

Silent interpretation changes are forbidden.

---

## 39. Configuration Migration

A configuration migration transforms older schema versions.

Migrations must be:

- deterministic;
- versioned;
- testable;
- reversible where practical;
- observable;
- free from secret exposure.

Migration does not directly overwrite user files unless explicitly approved.

---

## 40. Configuration Compatibility

Compatibility may be classified as:

```text
COMPATIBLE
COMPATIBLE_WITH_WARNINGS
REQUIRES_MIGRATION
REQUIRES_RESTART
INCOMPATIBLE
```

---

## 41. Configuration Persistence

The module may persist:

- accepted snapshots;
- source metadata;
- revisions;
- last-known-good snapshot;
- runtime overrides;
- migration history;
- acceptance status;
- audit metadata.

It must not persist raw secrets.

---

## 42. Configuration File Locations

Location policy must support platform portability.

Possible categories:

```text
APPLICATION_DEFAULTS
SYSTEM_CONFIGURATION
USER_CONFIGURATION
WORKSPACE_CONFIGURATION
SESSION_CONFIGURATION
```

Actual paths belong to platform-specific infrastructure.

Modules must not hardcode operating-system paths.

---

## 43. Security

Configuration must enforce:

- approved sources only;
- source trust levels;
- path validation;
- file ownership and permission checks where available;
- no arbitrary code execution;
- no raw secret exposure;
- safe diagnostics;
- schema validation;
- protected runtime overrides;
- audit for administrative changes;
- redacted events and logs.

---

## 44. Privacy

Configuration values may reveal user preferences or provider choices.

Observability should minimize:

- full configuration dumps;
- file paths;
- user-specific directories;
- provider-account metadata;
- reading preferences;
- local model locations.

Only redacted effective configuration should be exposed diagnostically.

---

## 45. Observability

Recommended metrics:

```text
configuration_load_count
configuration_load_failure_count
configuration_reload_count
configuration_reload_failure_count
configuration_validation_failure_count
configuration_snapshot_publish_count
configuration_revision
configuration_source_count
configuration_override_count
configuration_rollback_count
configuration_restart_required_count
configuration_consumer_rejection_count
configuration_last_known_good_usage_count
```

Recommended safe logs:

- source identity;
- source type;
- revision;
- section name;
- validation code;
- change classification;
- restart requirement.

---

## 46. Events

Configuration may publish:

```text
ConfigurationSourceRegistered
ConfigurationSourceLoaded
ConfigurationSourceLoadFailed
ConfigurationCandidateCreated
ConfigurationValidationFailed
ConfigurationSnapshotPublished
ConfigurationChanged
ConfigurationSectionChanged
ConfigurationReloadRejected
ConfigurationRollbackCompleted
ConfigurationOverrideCreated
ConfigurationOverrideExpired
ConfigurationRestartRequired
ConfigurationConsumerAcceptanceChanged
```

Exact event contracts belong in `EVENTS.md`.

---

## 47. Commands

Expected commands:

```text
RegisterConfigurationSource
EnableConfigurationSource
DisableConfigurationSource
ReloadConfiguration
ValidateConfiguration
CreateConfigurationOverride
RemoveConfigurationOverride
RollbackConfiguration
AcceptConfigurationRevision
RejectConfigurationRevision
MigrateConfiguration
```

Exact command contracts belong in `CONTRACT.md`.

---

## 48. Queries

Expected queries:

```text
GetActiveConfigurationSnapshot
GetConfigurationSection
GetConfigurationValue
GetConfigurationOrigin
ListConfigurationSources
GetConfigurationRevision
CompareConfigurationRevisions
GetConfigurationChangeSet
GetConfigurationValidationResult
GetRestartRequirements
GetLastKnownGoodConfiguration
ListConfigurationOverrides
```

Queries must apply redaction policy.

---

## 49. Configuration Authority

The active published configuration snapshot is authoritative for configuration values.

However:

```text
Active Configuration
    ≠ every consumer has already applied it
```

Consumer application status is tracked separately.

---

## 50. Failure Handling

Failure policy depends on stage.

### Source load failure

Possible behavior:

- ignore optional source;
- fail required source;
- use previous source revision;
- use last-known-good snapshot.

### Validation failure

The candidate is rejected.

The active snapshot remains unchanged.

### Consumer rejection

The configuration may remain published but require restart, or publication may be rolled back according to application policy.

---

## 51. Configuration Rollback

Rollback targets accepted historical content.

```text
Active R20
    ↓ rollback request to R17 content
Candidate created
    ↓ validation
New active R21
```

Rollback does not reactivate the old revision identity.

---

## 52. Concurrency

The module must handle:

- concurrent reload requests;
- file change during load;
- runtime override races;
- rollback racing with reload;
- source enable/disable races;
- consumer acceptance updates;
- remote source revision races;
- stale candidate publication.

Optimistic concurrency or equivalent revision checks are required.

---

## 53. Idempotency

Equivalent commands must behave deterministically.

Examples:

```text
Reload same unchanged sources
    → no new revision or explicit no-change result

Create same override with same idempotency key
    → return existing override

Remove already removed override
    → remain safely removed

Accept already accepted revision
    → idempotent success
```

---

## 54. Caching

Configuration may cache:

- parsed source data;
- schema documents;
- typed bindings;
- origin maps;
- redacted diagnostics;
- section snapshots.

The active immutable snapshot itself is safe to share.

Cache invalidation occurs on source or schema revision change.

---

## 55. Performance

Configuration loading is not expected to be a high-frequency hot path.

Priorities are:

1. correctness;
2. determinism;
3. safety;
4. debuggability;
5. reasonable startup and reload latency.

Reads from active snapshots should be fast and lock-light.

---

## 56. Initial MVP Scope

The MVP Configuration module should support:

- built-in defaults;
- application configuration file;
- user configuration file;
- environment-variable overrides;
- command-line startup overrides;
- deterministic precedence;
- YAML or JSON;
- typed section binding;
- schema validation;
- immutable snapshots;
- revisioning;
- change detection by explicit reload;
- redacted diagnostics;
- last-known-good snapshot;
- module-scoped queries;
- configuration change events;
- restart requirement reporting;
- secret references;
- integration with Runtime;
- integration with Provider Management;
- integration with Translation and Presentation.

---

## 57. Deferred Capabilities

Deferred capabilities may include:

- remote configuration service;
- live file watching on every platform;
- distributed configuration;
- organization-level policies;
- multi-user configuration synchronization;
- advanced feature rollout;
- percentage-based feature flags;
- configuration marketplace;
- cloud backup;
- encrypted configuration sections;
- visual schema-driven settings UI;
- automatic configuration repair;
- cross-device profile synchronization;
- configuration policy language;
- dynamic plugin-defined schemas.

Deferred features must preserve immutable snapshot and ownership rules.

---

## 58. Core Invariants

### Invariant 1

Consumers do not read raw configuration sources directly.

### Invariant 2

Every active configuration view is immutable.

### Invariant 3

Every accepted change creates a new revision.

### Invariant 4

Invalid candidates never replace the active snapshot.

### Invariant 5

Source precedence is deterministic.

### Invariant 6

Raw secrets never appear in ordinary configuration snapshots.

### Invariant 7

Configuration owns mechanics; consumers own domain semantics.

### Invariant 8

Publishing a snapshot does not imply every consumer applied it.

### Invariant 9

Older source or candidate revisions cannot overwrite newer active state.

### Invariant 10

Rollback creates a new revision.

### Invariant 11

Runtime overrides are explicit, scoped, auditable, and reversible.

### Invariant 12

Configuration diagnostics are redacted.

### Invariant 13

Unknown fields follow schema policy.

### Invariant 14

Restart requirements are explicit.

### Invariant 15

Module configuration sections are isolated by ownership.

### Invariant 16

Configuration values cannot execute arbitrary code.

---

## 59. Key Architectural Decisions

### Decision 1 — Centralized mechanics

All modules use the shared Configuration infrastructure.

### Decision 2 — Typed sections

Typed module sections are the primary access model.

### Decision 3 — Immutable snapshots

Published configuration is immutable and revisioned.

### Decision 4 — Deterministic precedence

Source precedence is explicit and stable.

### Decision 5 — Secrets by reference

Raw secrets remain in Secret Management.

### Decision 6 — Consumer-owned semantics

Each module validates the business meaning of its configuration.

### Decision 7 — Safe reload

Changes are classified by live applicability and restart needs.

### Decision 8 — Candidate before publication

New configuration is validated before replacing active state.

### Decision 9 — Last known good

The module may preserve a safe last-known-good snapshot.

### Decision 10 — Rollback creates a new revision

Historical revision identity remains immutable.

---

## 60. Open Decisions

The following must be finalized in later documents.

### Contract decisions

- exact source registration contract;
- exact snapshot contract;
- exact section query contract;
- exact override contract;
- exact origin contract;
- exact consumer acceptance contract;
- exact redaction metadata.

### State decisions

- configuration source states;
- candidate states;
- snapshot states;
- override states;
- consumer acceptance states;
- migration states.

### Event decisions

- source-load event visibility;
- per-key versus per-section change events;
- reload event granularity;
- consumer acceptance event visibility;
- file watcher event handling.

### Error decisions

- parse error taxonomy;
- schema error taxonomy;
- binding error taxonomy;
- source precedence conflict errors;
- migration errors;
- rollback errors;
- redaction errors.

### Policy decisions

- remote configuration precedence;
- unknown field default;
- last-known-good retention;
- file watch debounce;
- maximum snapshot history;
- restart aggregation;
- partial application policy;
- default configuration format.

---

## 61. Documentation Order

Recommended order:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
EVENTS.md
    ↓
ERRORS.md
    ↓
README.md
```

Not every infrastructure module requires equally complex `STATES.md` or `EVENTS.md`, but Configuration benefits from both because it has candidate, snapshot, source, override, and consumer-acceptance lifecycles.

---

## 62. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
```

Runtime references:

```text
docs/architecture/runtime/RUNTIME_CONFIG.md
docs/architecture/runtime/RUNTIME_COMPONENTS.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Related modules:

```text
02-modules/provider-management/MODULE.md
02-modules/translation/MODULE.md
02-modules/recognition/MODULE.md
02-modules/presentation/MODULE.md
02-modules/reading-session/MODULE.md
```

Future infrastructure references:

```text
03-infrastructure/configuration/CONTRACT.md
03-infrastructure/configuration/STATES.md
03-infrastructure/configuration/EVENTS.md
03-infrastructure/configuration/ERRORS.md
03-infrastructure/configuration/README.md

03-infrastructure/secret-management/MODULE.md
03-infrastructure/event-bus/MODULE.md
03-infrastructure/storage/MODULE.md
03-infrastructure/logging/MODULE.md
03-infrastructure/metrics/MODULE.md
03-infrastructure/tracing/MODULE.md
```

---

## 63. Summary

Configuration is the shared CRAI infrastructure responsible for loading, merging, validating, versioning, publishing, and safely refreshing application configuration.

Its central flow is:

```text
Sources
    ↓
Normalize
    ↓
Merge
    ↓
Bind
    ↓
Validate
    ↓
Immutable Snapshot
    ↓
Revisioned Publication
    ↓
Consumer Acceptance
```

The module is:

- centralized;
- typed;
- deterministic;
- revisioned;
- immutable;
- reload-aware;
- secret-safe;
- module-scoped;
- observable;
- rollback-capable.

It deliberately excludes:

- raw secret storage;
- domain policy ownership;
- Runtime scheduling;
- application restart orchestration;
- UI;
- generic filesystem and network implementation.

This document is the architectural source of truth for subsequent Configuration contracts, states, events, errors, and implementation documentation.
