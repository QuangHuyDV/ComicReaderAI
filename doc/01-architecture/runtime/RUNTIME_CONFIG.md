# CRAI Runtime Configuration

> **Project:** CRAI
> **Document:** Runtime Configuration Architecture
> **Path:** `runtime/RUNTIME_CONFIG.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-21

---

## 1. Purpose

This document defines how CRAI loads, validates, exposes, changes, and persists runtime configuration.

Runtime configuration controls how an installed CRAI application behaves without requiring source-code changes or rebuilding the application.

Examples include:

* source and target languages;
* OCR and translation provider selection;
* provider timeout and retry limits;
* capture frequency;
* stable-frame detection thresholds;
* worker concurrency;
* cache limits;
* presentation mode;
* logging level;
* privacy settings;
* experimental feature flags.

This document does not define the final user-interface layout for settings.

It also does not define provider-specific implementation details. Provider-specific fields must remain isolated behind provider configuration contracts.

---

## 2. Related Documents

Runtime configuration must remain consistent with:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES_RULE.md
.meta/MODULES.md

docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
docs/architecture/CAPABILITY_MAP.md
```

The main relationships are:

```text
RUNTIME_CONFIG
    ↓
controls runtime policies

STATE_MACHINE
    ↓
determines when a configuration change may take effect

EVENT_BUS
    ↓
distributes validated configuration changes

MODULE_DEPENDENCY
    ↓
restricts which modules may read which configuration sections

DATA_FLOW
    ↓
shows how configuration affects processing behavior
```

---

## 3. Configuration Goals

The CRAI configuration system should provide:

1. predictable startup behavior;
2. safe default values;
3. schema-based validation;
4. clear separation between user preferences and system policies;
5. secure handling of credentials;
6. module-scoped configuration access;
7. controlled runtime updates;
8. support for temporary session overrides;
9. compatibility across configuration versions;
10. diagnostics without exposing secrets.

---

## 4. Non-Goals

The initial configuration system will not provide:

* arbitrary user-authored scripts;
* dynamic code execution;
* unrestricted provider plugins;
* remote cloud-controlled configuration;
* organization-wide policy management;
* automatic synchronization across devices;
* direct configuration mutation by arbitrary modules;
* storage of credentials as plain text;
* immediate hot reload for every setting.

These capabilities may be reconsidered after the desktop MVP is validated.

---

## 5. Configuration Principles

### 5.1 Configuration Is Data

Configuration must be represented as structured data.

It must not contain executable application code.

Invalid configuration must not be interpreted partially unless the schema explicitly supports partial overrides.

---

### 5.2 Defaults Must Be Usable

CRAI should be able to start with built-in defaults even when no user configuration file exists.

Provider-dependent capabilities may remain unavailable until a valid provider is configured.

A missing provider credential must not prevent unrelated local capabilities from starting.

---

### 5.3 Validation Before Activation

Configuration must be validated before it becomes active.

The active runtime configuration must always satisfy the current configuration schema.

Modules must never observe an intermediate or partially merged configuration state.

---

### 5.4 Configuration Must Be Scoped

Modules should receive only the configuration sections they own or require.

For example:

```text
Capture Module
    → capture, observation and resource limits

OCR Module
    → OCR provider and OCR processing policies

Translation Module
    → translation provider and translation policies

Presentation Module
    → side-panel, overlay and typography preferences
```

A module must not read the complete configuration object merely for convenience.

---

### 5.5 Secrets Are References

Sensitive credentials should not be stored directly in normal configuration files.

Configuration should contain a reference to secure credential storage.

Example:

```yaml
translation:
  provider: openai
  credentialRef: secret://translation/openai-primary
```

The referenced secret may be stored through:

* operating-system credential storage;
* an encrypted local secret store;
* a development-only environment variable;
* a secure provider credential service in a future version.

---

### 5.6 Explicit Restart Semantics

Every configuration field must define one of these activation modes:

| Mode                | Meaning                                                  |
| ------------------- | -------------------------------------------------------- |
| Immediate           | May be applied to new work without restarting a session. |
| Session restart     | Requires the active reading session to stop and restart. |
| Module restart      | Requires recreation of one or more runtime modules.      |
| Application restart | Takes effect only after CRAI restarts.                   |
| Immutable           | Cannot be changed through normal runtime settings.       |

The configuration system must not imply that all settings can be hot-reloaded safely.

---

## 6. Configuration Categories

CRAI configuration is divided into six categories.

```text
Runtime Configuration
├── Bootstrap Configuration
├── Application Configuration
├── User Preferences
├── Session Configuration
├── Provider Configuration
└── Diagnostic Configuration
```

---

## 7. Bootstrap Configuration

Bootstrap configuration contains values required before the main runtime container is created.

Examples:

* application environment;
* application data directory;
* configuration file location;
* secure storage backend;
* runtime profile;
* crash recovery behavior;
* startup logging destination.

Example:

```yaml
bootstrap:
  environment: production
  profile: desktop-default
  dataDirectory: auto
  configFile: auto
  secretStore: operating-system
  recoveryMode: safe
```

Bootstrap values usually require an application restart.

Bootstrap configuration must remain small because it is loaded before the normal configuration infrastructure is available.

---

## 8. Application Configuration

Application configuration defines relatively stable system-wide behavior.

Examples:

* enabled modules;
* queue capacity;
* worker limits;
* cache policies;
* provider registry;
* event-bus limits;
* storage backends;
* feature flags.

Example:

```yaml
application:
  modules:
    capture: true
    observation: true
    ocr: true
    translation: true
    presentation: true

  runtime:
    maxWorkerCount: 4
    maxQueuedJobs: 64
    shutdownTimeoutMs: 5000
```

Application configuration is normally persisted between application runs.

---

## 9. User Preferences

User preferences represent user-facing choices that do not fundamentally redefine the application architecture.

Examples:

* source language;
* target language;
* preferred reading mode;
* translation style;
* font size;
* side-panel location;
* overlay opacity;
* automatic processing preference;
* notification preference.

Example:

```yaml
preferences:
  language:
    source: auto
    target: vi

  reading:
    contentMode: auto
    translationStyle: natural
    autoProcessStableContent: true

  presentation:
    mode: side-panel
    fontSize: 18
    lineHeight: 1.6
    showSourceText: true
    showRegionNumbers: true
```

User preferences should be editable through the application settings interface.

---

## 10. Session Configuration

Session configuration applies only to one active reading session.

It may be derived from:

* persisted user preferences;
* detected content properties;
* user actions;
* source-specific metadata;
* temporary overrides.

Examples:

* currently selected capture region;
* selected source window;
* detected source language;
* active provider override;
* current comic reading direction;
* temporary privacy mode;
* temporary quality profile.

Example:

```yaml
session:
  sessionId: generated
  sourceType: screen-region
  sourceLanguage: zh-Hans
  targetLanguage: vi
  readingDirection: top-to-bottom
  privacyMode: temporary
  providerProfile: balanced
```

Session configuration must not automatically overwrite persisted user preferences.

At session termination, session-only values are discarded unless a field is explicitly marked as persistable.

---

## 11. Provider Configuration

Provider configuration defines how CRAI selects and invokes OCR, translation, detection, or optional AI implementations.

Provider configuration must be divided into:

```text
Provider Selection
Provider Capability Requirements
Provider Execution Policy
Provider-Specific Options
Credential Reference
```

Example:

```yaml
providers:
  ocr:
    primary: paddle-ocr-local
    fallback: null

    requirements:
      languages:
        - zh-Hans
        - en
      verticalText: preferred
      regionDetection: required

    execution:
      timeoutMs: 10000
      maxRetries: 1
      concurrency: 2

    instances:
      paddle-ocr-local:
        type: local
        credentialRef: null
        options:
          useGpu: auto
          modelProfile: chinese-comic

  translation:
    primary: remote-translation-primary
    fallback: remote-translation-secondary

    requirements:
      targetLanguages:
        - vi
      batchTranslation: required
      cancellation: preferred

    execution:
      timeoutMs: 20000
      maxRetries: 1
      concurrency: 2

    instances:
      remote-translation-primary:
        type: remote
        credentialRef: secret://translation/primary
        options:
          model: configured-provider-model
```

Provider-specific options must be validated by the owning provider adapter.

Core modules must not interpret arbitrary provider-specific fields.

---

## 12. Diagnostic Configuration

Diagnostic configuration controls observability and development behavior.

Examples:

* log level;
* metrics collection;
* processing trace retention;
* debug overlay;
* event tracing;
* configuration dump;
* performance timing;
* provider request diagnostics.

Example:

```yaml
diagnostics:
  logLevel: info
  structuredLogging: true
  performanceTiming: true
  processingTrace: true
  eventTrace: false
  debugOverlay: false
  includeSourceContentInLogs: false
```

Raw source text, captured images, translated text, provider tokens, and credential values must not appear in standard logs by default.

---

## 13. Recommended Top-Level Schema

The initial configuration should follow this conceptual structure:

```yaml
schemaVersion: 1

bootstrap:
  environment: production
  profile: desktop-default

application:
  runtime: {}
  modules: {}
  scheduling: {}
  cache: {}
  storage: {}
  security: {}
  features: {}

preferences:
  language: {}
  reading: {}
  capture: {}
  presentation: {}
  accessibility: {}

providers:
  ocr: {}
  translation: {}
  detection: {}

diagnostics:
  logging: {}
  tracing: {}
  metrics: {}
  debug: {}
```

Session configuration should be stored separately from the persisted application configuration:

```yaml
session:
  sessionId: generated
  source: {}
  language: {}
  reading: {}
  providers: {}
  presentation: {}
  privacy: {}
```

---

## 14. Example Initial MVP Configuration

```yaml
schemaVersion: 1

bootstrap:
  environment: production
  profile: desktop-mvp
  dataDirectory: auto
  secretStore: operating-system
  recoveryMode: safe

application:
  modules:
    capture: true
    observation: true
    classification: true
    extraction: true
    textUnderstanding: true
    translation: true
    presentation: true
    knowledge: true
    diagnostics: true

  runtime:
    maxWorkerCount: auto
    maxQueuedJobs: 64
    shutdownTimeoutMs: 5000

  scheduling:
    visibleWorkPriority: 100
    retryWorkPriority: 50
    backgroundWorkPriority: 10
    cancelObsoleteWork: true

  cache:
    ocr:
      enabled: true
      maxEntries: 500
      retentionHours: 24

    translation:
      enabled: true
      maxEntries: 2000
      retentionHours: 168

    rawCapture:
      persistToDisk: false

  storage:
    engine: local
    historyEnabled: false
    glossaryEnabled: true
    correctionMemoryEnabled: true

  security:
    redactSensitiveLogs: true
    allowRemoteImageProcessing: false
    allowRemoteTextProcessing: true

  features:
    screenRegionCapture: true
    windowCapture: true
    sidePanel: true
    simpleOverlay: false
    browserConnector: false
    importedLibrary: false
    cloudSync: false
    runtimePlugins: false

preferences:
  language:
    source: auto
    target: vi
    preferredSources:
      - zh-Hans
      - zh-Hant
      - en

  reading:
    contentMode: auto
    translationStyle: natural
    autoStartProcessing: false
    autoProcessStableContent: true

  capture:
    frameIntervalMs: 250
    stableDurationMs: 450
    minimumChangeRatio: 0.03
    duplicateDetection: true
    captureCursor: false

  presentation:
    mode: side-panel
    sidePanelPosition: right
    fontSize: 18
    lineHeight: 1.6
    showSourceText: true
    showRegionNumbers: true
    preserveRegionOrder: true

providers:
  ocr:
    primary: local-ocr
    fallback: null

    execution:
      timeoutMs: 10000
      maxRetries: 1
      concurrency: 2

    instances:
      local-ocr:
        type: local
        credentialRef: null
        options:
          device: auto
          verticalText: true

  translation:
    primary: primary-translator
    fallback: null

    execution:
      timeoutMs: 20000
      maxRetries: 1
      concurrency: 2
      batchSize: 12

    instances:
      primary-translator:
        type: remote
        credentialRef: secret://translation/primary
        options:
          contextMode: neighboring-regions

diagnostics:
  logging:
    level: info
    structured: true
    includeSourceContent: false
    includeTranslatedContent: false

  tracing:
    processingTrace: true
    eventTrace: false
    retentionHours: 24

  metrics:
    stageTiming: true
    queueMetrics: true
    providerMetrics: true

  debug:
    overlay: false
    regionBounds: false
    readingOrder: false
```

This example is illustrative. Exact values must be verified through prototypes.

---

## 15. Configuration Sources

CRAI may receive configuration from several sources.

Recommended precedence, from lowest to highest:

```text
Built-in Defaults
    ↓
Runtime Profile
    ↓
Persisted Application Configuration
    ↓
Environment Overrides
    ↓
Command-Line Overrides
    ↓
Session Overrides
    ↓
Temporary User Actions
```

The higher-precedence source overrides only the explicitly supplied fields.

---

## 16. Built-In Defaults

Built-in defaults are distributed with the application.

Their purpose is to guarantee a valid baseline configuration.

Built-in defaults must:

* satisfy the current schema;
* avoid requiring credentials;
* avoid remote processing by accident;
* use conservative resource limits;
* disable unfinished features;
* prevent permanent raw capture storage;
* permit CRAI to open its settings and diagnostics screens.

Built-in defaults are immutable at runtime.

---

## 17. Runtime Profiles

A runtime profile is a named configuration layer.

Possible profiles include:

```text
desktop-mvp
desktop-low-resource
desktop-balanced
desktop-high-quality
development
test
benchmark
```

Profiles provide coherent groups of defaults.

They must not contain credentials.

Example:

```yaml
profile: desktop-low-resource

application:
  runtime:
    maxWorkerCount: 2

providers:
  ocr:
    execution:
      concurrency: 1

  translation:
    execution:
      concurrency: 1

preferences:
  capture:
    frameIntervalMs: 500
```

Users may select a profile, but explicit user preferences should override profile defaults.

---

## 18. Environment Variables

Environment variables may be supported for:

* development;
* testing;
* CI;
* portable deployments;
* secure credential references;
* advanced troubleshooting.

Recommended naming convention:

```text
CRAI_<SECTION>_<FIELD>
```

Examples:

```text
CRAI_BOOTSTRAP_ENVIRONMENT=development
CRAI_DIAGNOSTICS_LOG_LEVEL=debug
CRAI_RUNTIME_MAX_WORKER_COUNT=4
CRAI_TRANSLATION_CREDENTIAL_REF=env://CRAI_TRANSLATION_API_KEY
```

Environment variables should not become the primary configuration mechanism for normal desktop users.

Environment variable values must pass through the same validation process as file-based configuration.

---

## 19. Command-Line Overrides

Command-line arguments may override selected bootstrap or diagnostic values.

Examples:

```text
crai --profile development
crai --log-level debug
crai --safe-mode
crai --disable-remote-providers
crai --config /path/to/config.yaml
```

Command-line arguments must not expose provider secrets in process listings or shell history.

Secret values should be supplied through secure references rather than direct command-line parameters.

---

## 20. Configuration Merge Rules

Configuration layers should be merged deterministically.

### 20.1 Scalar Values

Higher-precedence scalar values replace lower-precedence values.

```yaml
# Lower layer
presentation:
  fontSize: 16

# Higher layer
presentation:
  fontSize: 18

# Effective value
presentation:
  fontSize: 18
```

---

### 20.2 Objects

Objects are merged recursively unless the schema marks the object as replace-only.

---

### 20.3 Arrays

Arrays must not be merged implicitly.

Each array field must define one explicit strategy:

| Strategy            | Meaning                                       |
| ------------------- | --------------------------------------------- |
| Replace             | The higher layer replaces the complete array. |
| Append              | New values are appended.                      |
| Unique append       | New unique values are appended.               |
| Merge by identifier | Entries are merged using a stable identifier. |

Default array behavior should be `Replace`.

Ambiguous array merging can create unsafe provider or module configurations.

---

### 20.4 Null Values

The schema must distinguish between:

```text
field absent
field explicitly null
field assigned a concrete value
```

A `null` value may mean:

* disable the optional value;
* remove an inherited value;
* use automatic selection.

Its meaning must be defined per field.

---

## 21. Configuration Loading Flow

```text
Application starts
    ↓
Bootstrap loader reads bootstrap inputs
    ↓
Built-in defaults are loaded
    ↓
Selected runtime profile is loaded
    ↓
Persisted configuration is loaded
    ↓
Environment overrides are applied
    ↓
Command-line overrides are applied
    ↓
Configuration schema validation runs
    ↓
Semantic validation runs
    ↓
Secret references are checked
    ↓
Effective configuration is frozen
    ↓
Runtime modules are created
```

The application must not initialize normal processing modules before configuration validation succeeds.

---

## 22. Validation Levels

Configuration validation should occur in several stages.

### 22.1 Syntax Validation

Checks whether configuration data can be parsed.

Examples:

* malformed YAML or JSON;
* invalid encoding;
* duplicated keys where unsupported;
* unexpected document structure.

---

### 22.2 Schema Validation

Checks field names, types, ranges, and required values.

Examples:

* `fontSize` must be a positive number;
* `timeoutMs` must be within a supported range;
* `targetLanguage` must be a supported language code;
* unknown top-level sections are rejected or reported.

---

### 22.3 Semantic Validation

Checks relationships between fields.

Examples:

```text
remote provider selected
    → credential reference must exist

overlay mode enabled
    → presentation module must be enabled

continuous capture enabled
    → observation module must be enabled

translation fallback selected
    → fallback provider must be registered

history disabled
    → history retention settings are ignored or rejected
```

---

### 22.4 Capability Validation

Checks whether the selected provider can satisfy required capabilities.

Example:

```text
Session requires vertical Chinese text
    ↓
Selected OCR provider reports no vertical-text support
    ↓
Configuration warning or provider rejection
```

Capability validation may occur during startup or when a session is created.

---

### 22.5 Runtime Validation

Some settings can only be verified when runtime resources are available.

Examples:

* selected GPU backend is unavailable;
* capture permission is denied;
* secure storage cannot be opened;
* provider endpoint is unreachable;
* configured model is not installed.

Runtime validation failures should produce structured diagnostics.

They must not silently alter configuration unless an explicit fallback policy exists.

---

## 23. Invalid Configuration Handling

Invalid persisted configuration must not cause uncontrolled startup failure.

Recommended behavior:

```text
Load persisted configuration
    ↓
Validation fails
    ↓
Record a sanitized diagnostic
    ↓
Preserve invalid configuration for recovery
    ↓
Start in configuration-safe mode
    ↓
Use built-in defaults for non-sensitive fields
    ↓
Disable affected providers or features
    ↓
Show configuration errors to the user
```

CRAI must not overwrite the invalid configuration automatically before the user can inspect or recover it.

---

## 24. Safe Mode

Safe mode starts CRAI with minimal capabilities.

Safe mode should:

* use built-in defaults;
* disable remote providers;
* disable experimental features;
* disable automatic continuous capture;
* avoid loading third-party provider extensions;
* use conservative resource limits;
* expose configuration recovery and diagnostics.

Safe mode may be triggered by:

* explicit command-line argument;
* repeated startup failure;
* configuration migration failure;
* module initialization failure;
* corrupted persistent state.

---

## 25. Runtime Configuration Access

Modules must access configuration through typed configuration views.

Conceptual interfaces:

```text
RuntimeConfigService
├── getBootstrapConfig()
├── getCaptureConfig()
├── getObservationConfig()
├── getOcrConfig()
├── getTranslationConfig()
├── getPresentationConfig()
├── getStorageConfig()
├── getSecurityConfig()
└── getDiagnosticsConfig()
```

A module must not:

* parse the configuration file directly;
* read environment variables directly;
* access another module's configuration section;
* mutate the effective configuration object;
* resolve arbitrary secrets without authorization.

---

## 26. Immutable Configuration Snapshots

The active configuration should be represented as an immutable snapshot.

Example:

```text
EffectiveConfigurationSnapshot
├── snapshotId
├── schemaVersion
├── createdAt
├── sourceVersions
├── validatedConfiguration
└── sanitizedSummary
```

Every session should retain the identifier of the configuration snapshot under which it started.

This allows diagnostics to answer:

* which provider was selected;
* which timeout policy was active;
* which capture threshold was used;
* whether the configuration changed during processing.

Secrets must not be copied into trace snapshots.

---

## 27. Runtime Configuration Changes

A user may change configuration while CRAI is running.

The change flow should be:

```text
User submits proposed changes
    ↓
Configuration service creates a candidate snapshot
    ↓
Schema validation runs
    ↓
Semantic validation runs
    ↓
Affected modules are calculated
    ↓
Activation requirements are calculated
    ↓
User is informed of restart implications
    ↓
Configuration is persisted
    ↓
Change is activated at the allowed boundary
    ↓
Configuration events are published
```

The existing active snapshot must remain unchanged until validation succeeds.

---

## 28. Configuration Change Events

Configuration events should be distributed through the event bus.

Suggested event names:

```text
runtime.config.change-requested
runtime.config.validation-succeeded
runtime.config.validation-failed
runtime.config.persisted
runtime.config.activated
runtime.config.activation-deferred
runtime.config.rollback-completed
```

Example event:

```yaml
eventType: runtime.config.activated
eventVersion: 1
eventId: generated
occurredAt: timestamp

payload:
  previousSnapshotId: config-snapshot-41
  activeSnapshotId: config-snapshot-42

  changedSections:
    - preferences.presentation
    - diagnostics.logging

  activation:
    mode: immediate

metadata:
  correlationId: generated
  causationId: generated
```

Configuration events must not contain credential values.

---

## 29. Immediate Changes

Immediate settings may affect newly created work while existing work continues with its original configuration snapshot.

Examples may include:

* UI font size;
* side-panel visibility;
* normal logging level;
* whether source text is shown;
* whether region numbers are displayed.

Immediate changes must not mutate data already owned by active jobs.

---

## 30. Session-Restart Changes

Some settings require the active reading session to restart.

Examples:

* selected capture source;
* source language override;
* reading direction;
* session privacy mode;
* OCR provider;
* translation provider;
* content-processing mode.

Recommended behavior:

```text
Persist new configuration
    ↓
Mark change as pending
    ↓
Allow current session to continue unchanged
    ↓
Apply new configuration to the next session
```

Alternatively, the user may explicitly stop and recreate the current session.

---

## 31. Module-Restart Changes

Some settings require recreating one or more modules.

Examples:

* OCR model device selection;
* provider endpoint;
* worker pool size;
* secure storage backend;
* capture backend;
* local model path.

Module restart must follow the runtime dependency graph.

Example:

```text
OCR provider changes
    ↓
Stop accepting new OCR jobs
    ↓
Cancel or drain current OCR jobs
    ↓
Dispose current OCR provider
    ↓
Create replacement provider
    ↓
Run health and capability checks
    ↓
Resume OCR scheduling
```

A failed module restart should trigger rollback when possible.

---

## 32. Application-Restart Changes

Application restart may be required for:

* application data directory;
* bootstrap profile;
* native capture backend;
* graphics backend;
* secure storage implementation;
* event-bus implementation;
* application-wide experimental runtime features.

The settings interface must clearly report:

```text
Saved
Takes effect after application restart
```

---

## 33. Configuration and State Machine

Configuration activation must respect application and session states.

Example constraints:

| Runtime State  | Allowed Changes                                                    |
| -------------- | ------------------------------------------------------------------ |
| Starting       | Bootstrap loading only.                                            |
| Ready          | Most validated changes may be prepared.                            |
| Session active | Immediate changes and deferred session changes.                    |
| Processing     | No destructive provider replacement without cancellation or drain. |
| Paused         | Selected session changes may be applied safely.                    |
| Stopping       | No new configuration activation.                                   |
| Failed         | Recovery and safe-mode configuration only.                         |

The configuration service may reject or defer changes that conflict with the current state.

---

## 34. Configuration and Work Items

Every asynchronous work item should reference the configuration snapshot used to create it.

Example:

```yaml
job:
  jobId: translation-job-1024
  sessionId: reading-session-17
  configSnapshotId: config-snapshot-42
```

A configuration change must not unexpectedly alter a job already in progress.

New jobs may use the new snapshot after activation.

---

## 35. Configuration and Cancellation

When a configuration change invalidates active work, the runtime must determine whether to:

* allow the work to finish;
* cancel the work;
* discard its output;
* restart the processing stage;
* defer activation.

Example:

```text
Translation provider changes
    ↓
Active translation request uses old snapshot
    ↓
Request may finish
    ↓
Result is accepted only if session generation remains current
```

Provider replacement alone must not bypass normal stale-result protection.

---

## 36. Configuration Persistence

Configuration persistence should use an atomic-write strategy.

Recommended flow:

```text
Serialize validated configuration
    ↓
Write temporary file
    ↓
Flush file contents
    ↓
Validate serialized result
    ↓
Atomically replace active configuration file
    ↓
Retain bounded backup
```

A crash during persistence must not leave the only configuration copy partially written.

---

## 37. Configuration Storage Layout

Conceptual local layout:

```text
<CRAI_DATA_DIR>/
├── config/
│   ├── application.yaml
│   ├── application.yaml.backup
│   ├── profiles/
│   └── migrations/
│
├── secrets/
│   └── managed-by-secure-store
│
├── state/
│   ├── recovery.json
│   └── pending-config.json
│
└── logs/
```

The exact operating-system paths will be decided by the desktop platform implementation.

Secret storage must not be treated as a normal subdirectory when an operating-system credential store is available.

---

## 38. Secrets and Credential Resolution

Credential resolution should be performed through a dedicated secret service.

Conceptual interface:

```text
SecretResolver
├── resolve(reference)
├── exists(reference)
├── store(alias, secret)
├── remove(reference)
└── describe(reference)
```

`describe` may return safe metadata such as:

* provider alias;
* creation time;
* last validation time;
* credential source.

It must never return the secret value for display.

---

## 39. Supported Secret Reference Forms

Possible reference forms:

```text
secret://translation/primary
os-keychain://crai/translation/primary
env://CRAI_TRANSLATION_API_KEY
memory://session/provider-token
```

Recommended policy:

| Reference        |   Production | Development |            Persistence |
| ---------------- | -----------: | ----------: | ---------------------: |
| `secret://`      |          Yes |         Yes |     Secure local store |
| `os-keychain://` |          Yes |         Yes | Operating-system store |
| `env://`         |      Limited |         Yes |               External |
| `memory://`      | Session only |         Yes |                     No |

Plain values such as the following should be rejected in normal persisted configuration:

```yaml
apiKey: sk-example-secret
```

---

## 40. Configuration Redaction

Any configuration output used for logs, diagnostics, bug reports, or UI previews must pass through redaction.

Example:

```yaml
credentialRef: secret://translation/primary
credentialStatus: configured
credentialValue: "[REDACTED]"
```

Redaction should also cover:

* authorization headers;
* API tokens;
* cookies;
* signed URLs;
* private endpoints containing embedded credentials;
* raw imported content where privacy mode forbids disclosure.

---

## 41. Privacy-Related Configuration

Privacy settings require conservative defaults.

Recommended fields:

```yaml
security:
  allowRemoteTextProcessing: true
  allowRemoteImageProcessing: false
  persistRawCapture: false
  persistOcrText: false
  persistTranslationCache: true
  includeContentInDiagnostics: false
  clearTemporaryDataOnExit: true
```

When a remote provider is selected, CRAI should make the data transfer behavior visible to the user.

A provider configuration must not silently override global privacy restrictions.

---

## 42. Resource Configuration

Runtime resource limits should be configurable but bounded.

Suggested fields:

```yaml
application:
  runtime:
    maxWorkerCount: auto
    maximumMemoryMb: auto
    maxQueuedJobs: 64

  network:
    maxConcurrentRequests: 4

  capture:
    maxFramesPerSecond: 4

  cache:
    maximumDiskMb: 512
```

User-configured values must be constrained to safe supported ranges.

`auto` should mean that CRAI selects a value based on device capability and runtime profile.

---

## 43. Capture Configuration

Suggested capture settings:

```yaml
preferences:
  capture:
    mode: selected-region
    frameIntervalMs: 250
    captureCursor: false
    includeWindowShadow: false
    scaleMode: native
```

Suggested observation settings:

```yaml
application:
  observation:
    stableDurationMs: 450
    minimumChangeRatio: 0.03
    duplicateDetection: true
    scrollDebounceMs: 300
    ignoreCursorChanges: true
    ignoreSmallAnimations: true
```

Exact thresholds must be validated through representative comic-reading tests.

---

## 44. OCR Configuration

Suggested OCR settings:

```yaml
providers:
  ocr:
    primary: local-ocr

    execution:
      timeoutMs: 10000
      maxRetries: 1
      concurrency: 2

    policy:
      detectRegions: true
      recognizeVerticalText: true
      preserveBoundingBoxes: true
      minimumConfidence: 0.45
      lowConfidenceBehavior: include-with-warning
```

OCR confidence thresholds should not silently delete text unless the configured policy explicitly requires it.

---

## 45. Translation Configuration

Suggested translation settings:

```yaml
providers:
  translation:
    primary: primary-translator

    execution:
      timeoutMs: 20000
      maxRetries: 1
      concurrency: 2
      batchSize: 12

    policy:
      contextMode: neighboring-regions
      preserveSegmentAlignment: true
      glossaryEnabled: true
      translationMemoryEnabled: true
      fallbackOnTimeout: true
      fallbackOnRateLimit: true
      fallbackOnInvalidRequest: false
```

Fallback behavior must distinguish recoverable and non-recoverable failures.

A configuration error must not cause repeated fallback loops.

---

## 46. Presentation Configuration

Text reading and image reading have different presentation requirements.

Suggested shared settings:

```yaml
preferences:
  presentation:
    mode: side-panel
    fontFamily: system
    fontSize: 18
    lineHeight: 1.6
    showSourceText: true
    showTranslation: true
```

Suggested comic-specific settings:

```yaml
preferences:
  presentation:
    comic:
      showRegionNumbers: true
      highlightActiveRegion: true
      preserveReadingOrder: true
      overlayEnabled: false
      overlayOpacity: 0.90
      maximumOverlayFontReduction: 0.25
```

Suggested novel-specific settings:

```yaml
preferences:
  presentation:
    novel:
      paragraphWidth: comfortable
      paragraphSpacing: 1.0
      dialogueIndentation: true
      preserveOriginalParagraphs: true
```

Comic and novel presentation policies should not be forced into one undifferentiated field set.

---

## 47. Feature Flags

Feature flags allow incomplete or experimental functionality to remain isolated.

Example:

```yaml
application:
  features:
    simpleOverlay: false
    browserConnector: false
    browserDomObservation: false
    translatedLibrary: false
    cloudSync: false
    localTranslationModel: false
```

Feature flags must not replace architectural module boundaries.

A feature flag may control whether a capability is exposed, but disabled code must still obey dependency and security rules.

---

## 48. Feature Flag Categories

Recommended categories:

| Category     | Purpose                                   |
| ------------ | ----------------------------------------- |
| Stable       | Production capability, normally enabled.  |
| Optional     | Supported feature selected by the user.   |
| Experimental | Under validation and disabled by default. |
| Internal     | Development or diagnostics only.          |
| Removed      | No longer accepted by current schema.     |

Experimental flags should display an explicit warning where they affect stored data, privacy, or provider cost.

---

## 49. Configuration Versioning

The root configuration must include a schema version.

```yaml
schemaVersion: 1
```

Schema versioning supports:

* field renames;
* field relocation;
* new required defaults;
* removed settings;
* provider option changes;
* changed validation rules.

The schema version is not the application version.

---

## 50. Configuration Migration

When CRAI opens an older configuration:

```text
Read schema version
    ↓
Find supported migration path
    ↓
Create a backup
    ↓
Migrate in memory
    ↓
Validate migrated configuration
    ↓
Persist new version atomically
    ↓
Record migration diagnostics
```

Migration must be deterministic.

A migration must not require access to provider secrets beyond checking that references remain valid.

---

## 51. Unsupported Future Configuration

When an older CRAI version encounters a newer unsupported schema:

```text
Detected schema version > supported version
    ↓
Do not overwrite the configuration
    ↓
Start in safe mode or stop configuration activation
    ↓
Inform the user that a newer CRAI version is required
```

The application must not guess how to interpret unknown security or provider fields.

---

## 52. Unknown Fields

Recommended initial policy:

```text
Unknown top-level field
    → validation error

Unknown core section field
    → validation error

Unknown provider-specific option
    → delegated to provider schema

Unknown experimental field
    → warning or error according to feature schema
```

Strict handling helps detect misspelled settings.

For forward compatibility, migration tools may preserve unknown fields separately, but runtime modules must not use them.

---

## 53. Configuration Rollback

CRAI should retain a bounded number of valid configuration snapshots or backups.

Rollback may occur when:

* module initialization fails;
* provider validation fails;
* a new configuration prevents session creation;
* the application crashes repeatedly after a change;
* the user explicitly selects a previous configuration.

Rollback flow:

```text
Activation fails
    ↓
Affected modules are stopped
    ↓
Previous valid snapshot is restored
    ↓
Modules are recreated
    ↓
Rollback event is published
    ↓
Failure diagnostics are preserved
```

Secrets referenced by the previous snapshot must still exist for full rollback to succeed.

---

## 54. Pending Configuration

A configuration may be valid but not yet active.

Example:

```yaml
pendingConfiguration:
  snapshotId: config-snapshot-43
  requiredActivation: application-restart
  changedSections:
    - bootstrap.secretStore
```

The settings UI should distinguish:

* current active value;
* saved pending value;
* required action;
* validation status.

---

## 55. Configuration Diagnostics

Configuration diagnostics should report:

* active schema version;
* active snapshot ID;
* selected profile;
* loaded configuration sources;
* ignored or overridden fields;
* validation warnings;
* disabled modules;
* unavailable providers;
* pending changes;
* restart requirements;
* redacted provider configuration;
* latest migration result.

Example:

```yaml
configurationStatus:
  schemaVersion: 1
  activeSnapshotId: config-snapshot-42
  profile: desktop-mvp
  valid: true
  pendingRestart: false

  providers:
    ocr:
      selected: local-ocr
      available: true

    translation:
      selected: primary-translator
      available: false
      reason: credential-reference-not-found
```

---

## 56. Configuration Error Model

Suggested error codes:

```text
CONFIG_PARSE_FAILED
CONFIG_SCHEMA_UNSUPPORTED
CONFIG_FIELD_UNKNOWN
CONFIG_FIELD_REQUIRED
CONFIG_VALUE_INVALID
CONFIG_RELATION_INVALID
CONFIG_PROVIDER_NOT_REGISTERED
CONFIG_PROVIDER_CAPABILITY_MISSING
CONFIG_SECRET_REFERENCE_INVALID
CONFIG_SECRET_NOT_FOUND
CONFIG_PERSIST_FAILED
CONFIG_MIGRATION_FAILED
CONFIG_ACTIVATION_FAILED
CONFIG_RESTART_REQUIRED
CONFIG_ROLLBACK_FAILED
```

Errors should include:

* error code;
* affected field path;
* sanitized message;
* recoverability;
* recommended user action;
* correlation ID.

---

## 57. Testing Requirements

The configuration system should have automated tests for:

### 57.1 Default Configuration

* defaults satisfy the schema;
* defaults start without credentials;
* unfinished features are disabled;
* sensitive persistence is disabled by default.

### 57.2 Merge Behavior

* precedence is deterministic;
* nested object merge is correct;
* arrays follow their declared strategy;
* `null` behavior is correct;
* session overrides do not mutate persisted preferences.

### 57.3 Validation

* invalid types are rejected;
* out-of-range values are rejected;
* conflicting settings are rejected;
* unavailable capabilities are reported;
* secret values are never included in validation output.

### 57.4 Persistence

* writes are atomic;
* corrupted files are recoverable;
* backups are bounded;
* failed persistence leaves the old valid file intact.

### 57.5 Migration

* each supported version migrates correctly;
* migrations are repeatable;
* unsupported future versions are preserved;
* migration failure enters safe mode.

### 57.6 Runtime Activation

* immediate changes affect only permitted behavior;
* session changes remain pending when required;
* module restart follows dependency order;
* activation failure performs rollback;
* active jobs retain their configuration snapshot.

### 57.7 Security

* credentials are stored only through secure references;
* diagnostics redact secrets;
* command-line output does not expose secrets;
* configuration export excludes secret values;
* remote provider settings cannot bypass privacy policy.

---

## 58. Initial MVP Decisions

For the first CRAI desktop MVP:

1. use one persisted local configuration file;
2. use built-in defaults plus user overrides;
3. store secrets through an operating-system credential store where available;
4. use typed module configuration views;
5. validate configuration before runtime activation;
6. use immutable effective configuration snapshots;
7. permit immediate UI preference updates;
8. require session restart for provider or source changes;
9. require application restart for bootstrap changes;
10. disable cloud-controlled configuration;
11. disable runtime provider plugin loading;
12. default raw screen captures to memory-only;
13. default diagnostics to exclude source and translated content;
14. retain one or more recent valid configuration backups;
15. support safe mode after configuration failure.

---

## 59. Deferred Decisions

The following decisions remain open:

* YAML versus JSON as the persisted human-readable format;
* exact operating-system secret storage implementation;
* whether environment overrides are supported in production builds;
* how many valid backups should be retained;
* whether provider health checks run at startup or on first use;
* whether provider changes should drain or cancel active requests;
* whether session configurations can be exported;
* whether users may create custom runtime profiles;
* whether configuration files may be imported from another device;
* whether different websites or series may have dedicated preference profiles;
* whether adaptive performance tuning may update temporary configuration automatically.

These decisions should be based on implementation prototypes and user-testing evidence.

---

## 60. Recommended Configuration Ownership

Suggested ownership boundaries:

| Configuration Section      | Owning Runtime Area           |
| -------------------------- | ----------------------------- |
| `bootstrap`                | Application Bootstrap         |
| `application.runtime`      | Runtime Coordinator           |
| `application.scheduling`   | Scheduler                     |
| `application.cache`        | Cache Manager                 |
| `application.storage`      | Storage Module                |
| `application.security`     | Security and Privacy          |
| `application.features`     | Feature Registry              |
| `preferences.language`     | Reading Session               |
| `preferences.capture`      | Capture and Observation       |
| `preferences.reading`      | Reading Session               |
| `preferences.presentation` | Presentation                  |
| `providers.ocr`            | OCR Provider Registry         |
| `providers.translation`    | Translation Provider Registry |
| `diagnostics`              | Diagnostics Module            |

The configuration service owns loading, merging, validation, persistence, snapshotting, and activation coordination.

It does not own the business interpretation of every setting.

---

## 61. Module Dependency Rules

The runtime configuration module may depend on:

* schema definitions;
* configuration storage abstraction;
* secret-reference abstraction;
* diagnostics abstraction;
* runtime lifecycle contracts.

Feature modules may depend on typed configuration contracts.

Feature modules must not depend on:

* the concrete configuration file parser;
* the physical configuration file path;
* environment variable readers;
* command-line parsers;
* configuration migration internals;
* secret storage implementation.

Conceptual dependency direction:

```text
Feature Module
    ↓
Typed Configuration Contract
    ↑
Runtime Configuration Service
    ↓
Parser / Persistence / Migration / Secret References
```

---

## 62. Data Flow Summary

```text
Configuration Sources
    ↓
Configuration Loader
    ↓
Layer Merger
    ↓
Schema Validator
    ↓
Semantic Validator
    ↓
Capability Validator
    ↓
Immutable Configuration Snapshot
    ↓
Typed Module Views
    ↓
Runtime Modules
```

Runtime update flow:

```text
Settings UI
    ↓
Configuration Change Request
    ↓
Candidate Snapshot
    ↓
Validation
    ↓
Impact Analysis
    ↓
Persistence
    ↓
Activation Boundary
    ↓
Configuration Event
    ↓
Affected Runtime Modules
```

---

## 63. Security Summary

The initial security rules are:

```text
No plain-text provider credentials in normal configuration

No secrets in logs, events, traces or exports

No remote processing without an explicit permitted policy

No permanent raw capture storage by default

No partial activation of invalid configuration

No arbitrary module access to the complete configuration

No automatic overwrite of corrupted user configuration

No execution of code from configuration
```

---

## 64. Completion Criteria

This architecture document is considered ready for implementation planning when:

* configuration categories are accepted;
* precedence rules are accepted;
* activation modes are accepted;
* module ownership is accepted;
* secret references are accepted;
* initial schema fields are sufficient for the MVP;
* configuration change events align with `EVENT_BUS.md`;
* restart behavior aligns with `STATE_MACHINE.md`;
* configuration dependencies align with `MODULE_DEPENDENCY.md`;
* loading and activation flows align with `DATA_FLOW.md`.

---

## 65. Next Recommended Runtime Documents

After this document, the recommended order is:

```text
runtime/RUNTIME_CONFIG.md
    ↓
runtime/RUNTIME_CONTEXT.md
    ↓
runtime/RUNTIME_LIFECYCLE.md
    ↓
runtime/WORK_SCHEDULER.md
    ↓
runtime/CANCELLATION_MODEL.md
    ↓
runtime/RESOURCE_LIMITS.md
    ↓
runtime/ERROR_RECOVERY.md
```

The immediate next document should be:

```text
runtime/RUNTIME_CONTEXT.md
```

It should define the runtime objects and identifiers shared across modules, including:

* application runtime ID;
* reading session context;
* processing generation;
* configuration snapshot ID;
* correlation and causation IDs;
* cancellation scope;
* active source context;
* provider execution context;
* diagnostic trace context.
