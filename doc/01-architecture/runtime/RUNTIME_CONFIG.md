# Runtime Configuration

* **Document:** Runtime Architecture / Runtime Configuration
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines the configuration semantics owned by CRAI Runtime.

Runtime Configuration controls execution behavior such as:

* execution capacity,
* Scheduler admission,
* Work Queue limits,
* Retry execution,
* cancellation timing,
* Runtime Artifact retention,
* Resource/Lease limits,
* shutdown behavior,
* Runtime diagnostics,
* process/runtime execution options.

Runtime Configuration consumes resolved inputs from other CRAI configuration owners.

It MUST NOT become the canonical configuration system for:

* Workspace,
* Project,
* Profile,
* Reading Session,
* Translation,
* Recognition,
* Presentation,
* Provider Management,
* Plugin Configuration,
* AI Routing,
* Safety/Privacy Policy.

---

# 2. Core Principle

```text
Canonical Configuration Owners
        |
        v
Configuration Resolution
        |
        v
Runtime-Relevant Projection
        |
        v
RuntimeConfigurationSnapshot
        |
        v
Runtime Components
```

Runtime Components consume immutable resolved snapshots.

They MUST NOT independently read or merge arbitrary CRAI configuration.

---

# 3. Ownership Boundary

Critical distinction:

```text
Canonical Configuration
    = owned by the relevant CRAI architecture/module
```

```text
Runtime Configuration
    = execution-specific projection
```

Examples:

```text
Workspace privacy policy
    -> Workspace/Governance

Translation style
    -> Profile / Translation

Provider credentials
    -> Provider Management / Secret Management

Plugin settings
    -> Plugin Configuration

AI model routing
    -> AI Routing

Runtime worker count
    -> Runtime Configuration
```

---

# 4. Runtime Configuration Responsibilities

Runtime Configuration owns semantics for:

```text
execution
scheduler
queues
retry
cancellation
resource limits
leases
runtime-artifact retention
runtime cache implementation limits
shutdown
runtime diagnostics
process/runtime execution
```

---

# 5. Non-Responsibilities

Runtime Configuration does NOT own:

```text
sourceLanguage
targetLanguage
translationStyle
readingDirection
captureMode
presentationMode
provider preference
AI model preference
Workspace privacy policy
Safety policy
Glossary policy
Character policy
Session lifecycle
Plugin-specific config
Provider credentials
```

These MAY influence a Runtime projection after their owning architecture resolves them.

---

# 6. Runtime Configuration Layers

Recommended layers:

```text
BootstrapRuntimeConfiguration
RuntimeSystemConfiguration
RuntimeEnvironmentProjection
RuntimeOperationOverrides
```

Not every value supports every layer.

---

# 7. Bootstrap Runtime Configuration

Bootstrap configuration contains only information needed before full configuration infrastructure exists.

Possible:

```text
environment
runtimeProfile
dataDirectory
configurationLocation
secretStoreBackend
recoveryMode
safeModeFlag
earlyDiagnosticsDestination
pluginDiscoveryRoots?
```

Bootstrap configuration MUST remain small.

---

# 8. Runtime System Configuration

Runtime System Configuration defines application-instance execution limits and behavior.

Examples:

```text
worker capacity
queue capacity
Scheduler policy
Retry limits
cancellation grace
resource budgets
shutdown timeout
diagnostics
```

---

# 9. Runtime Environment Projection

Some values are derived from current host capabilities.

Examples:

```text
available CPU
available memory
GPU capability
process isolation support
OS/runtime constraints
provider/runtime availability
```

These values are runtime observations/projections, not persisted user preference.

---

# 10. Runtime Operation Overrides

An operation MAY carry bounded execution overrides such as:

```text
deadline
priority
execution class
resource hint
```

when the relevant contract permits it.

Operation override MUST NOT bypass:

* Security,
* Workspace Policy,
* hard Runtime safety limits,
* capability constraints.

---

# 11. Immutable Runtime Configuration Snapshot

Recommended:

```text
RuntimeConfigurationSnapshot
├── runtimeConfigurationSnapshotId
├── schemaVersion
├── configurationRevision
├── createdAt
├── runtimeSettings
├── environmentProjectionReference?
├── policyReferences[]
├── sourceReferences[]
├── contentHash
└── activationMetadata
```

---

# 12. Snapshot Content

The snapshot SHOULD contain only Runtime-relevant resolved values.

It SHOULD NOT contain an entire copy of:

```text
Workspace
Project
Profile
Provider Configuration
Plugin Configuration
Reading Session
Presentation Configuration
```

---

# 13. Immutable Execution Rule

Once an Attempt starts:

```text
Attempt
    -> RuntimeConfigurationSnapshot A
```

that Attempt continues with A.

Activation of Snapshot B MUST NOT mutate the in-flight Attempt.

---

# 14. Runtime Identity

Asynchronous Runtime execution SHOULD preserve:

```text
ApplicationInstanceId
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
RuntimeConfigurationSnapshotId
```

Optional business correlation MAY include:

```text
sessionId
projectId
requestId
correlationId
```

These identities MUST NOT be conflated.

---

# 15. ExecutionRevision Terminology

Runtime MUST use:

```text
ExecutionRevision
ExecutionRevisionId
```

rather than ambiguous:

```text
Revision
RevisionId
```

because CRAI Domain contains independent revisions such as TranslationRevision.

---

# 16. Configuration Schema

Recommended top-level Runtime schema:

```yaml
runtime:
  execution: {}
  scheduler: {}
  queues: {}
  retry: {}
  cancellation: {}
  resources: {}
  leases: {}
  artifacts: {}
  cache: {}
  shutdown: {}
  diagnostics: {}
```

Optional implementation-specific sections MAY exist when Runtime owns them.

---

# 17. What Is Deliberately Absent

The Runtime schema SHOULD NOT directly contain:

```yaml
preferences:
providers:
session:
presentation:
translation:
recognition:
workspace:
project:
profile:
```

Those belong to their owning configuration architectures.

---

# 18. Execution Configuration

Example:

```yaml
runtime:
  execution:
    maxCpuWorkers: auto
    maxGpuExecutions: 1
    maxNativeSerialExecutions: 1
    maxExternalIoExecutions: 4
```

---

# 19. Auto Values

`auto` means:

```text
derive within safe Runtime limits
from current environment profile
```

It MUST NOT mean arbitrary adaptive behavior without explicit bounds.

---

# 20. Scheduler Configuration

Example:

```yaml
runtime:
  scheduler:
    priority:
      control: 1000
      interactive: 100
      retry: 50
      background: 10
      maintenance: 1

    admission:
      rejectAtCriticalPressure: true
      preserveControlCapacity: true

    replacement:
      removeObsoleteQueuedWork: true
```

---

# 21. Scheduler Boundary

Scheduler Configuration controls:

```text
Runtime admission behavior
```

It does NOT:

* grant execution authority;
* decide business workflow;
* choose AI model/provider;
* define Domain priority semantics.

---

# 22. Work Queue Configuration

Example:

```yaml
runtime:
  queues:
    control:
      capacity: 128
      overflow: reject

    interactive:
      capacity: 64
      overflow: replace-obsolete

    background:
      capacity: 32
      overflow: reject
```

Queue capacities MUST remain bounded.

---

# 23. Queue Correctness

The following are architecture rules, not optional config toggles:

```text
queued work is lightweight
large payloads use ArtifactRef
cancelled/stale work is not accepted as current
```

---

# 24. Retry Configuration

Retry remains a Runtime execution policy.

Example:

```yaml
runtime:
  retry:
    enabled: true

    attempts:
      maxAttemptsPerWorkItem: 2
      maxConcurrentRetries: 2
      maxDelayedRetries: 16

    backoff:
      initialDelayMs: 500
      maximumDelayMs: 5000
      multiplier: 2.0
      jitter: true

    budgets:
      perExecutionRevision: 8
      globalRuntime: 64
```

---

# 25. Retry vs Fallback

Runtime Retry MUST NOT contain:

```text
providerFallback
modelFallback
routeFallback
```

as Retry strategies.

Critical rule:

```text
Retry
    = same logical WorkItem
      + compatible execution binding
      + new Attempt
```

```text
Fallback
    = new execution binding/route
      chosen by the owning recovery/routing architecture
```

---

# 26. Retry-After

Runtime Retry MAY respect normalized:

```text
RetryAfter
```

from provider/runtime execution.

Provider-specific headers MUST remain normalized behind adapters.

---

# 27. Cancellation Configuration

Example:

```yaml
runtime:
  cancellation:
    grace:
      defaultMs: 1000
      externalIoMs: 2000
      shutdownMs: 5000

    drain:
      removeQueuedCancelledWork: true
```

---

# 28. Cancellation Invariants

These SHOULD NOT be configurable off:

```text
cancelled execution loses acceptance authority
new cancelled work is not admitted
cancelled stale completion cannot overwrite current state
```

---

# 29. Authority Is Not User Configuration

Runtime authority correctness MUST NOT be represented as optional settings such as:

```yaml
acceptStaleCompletion: true
requireAuthorityValidation: false
```

These are invalid production concepts.

---

# 30. Hard Runtime Invariants

The following are architecture invariants:

```text
stale result cannot become current
duplicate logical completion cannot overwrite accepted result
cancelled result cannot regain authority
Worker cannot grant itself authority
```

They MUST NOT be disabled by normal configuration.

---

# 31. Runtime Artifact Configuration

Runtime MAY configure operational Artifact limits.

Example:

```yaml
runtime:
  artifacts:
    memoryBudgetMb: auto
    temporaryRetentionMs: 30000
    cleanupBatchSize: 64
```

---

# 32. Artifact Publication Is Not a Toggle

Architecture invariants such as:

```text
immutable published Artifact
explicit ownership transfer
atomic publication
accepted execution authority
```

MUST NOT be configurable off.

---

# 33. Runtime Artifact vs Domain Commit

Runtime Config may control Runtime Artifact retention.

It MUST NOT control:

```text
whether TranslationRevision becomes canonical
whether Character state is committed
whether Glossary truth changes
```

Those decisions belong to Business Modules.

---

# 34. Resource Configuration

Example:

```yaml
runtime:
  resources:
    managedMemoryMb: auto
    nativeMemoryMb: auto
    gpuMemoryMb: auto
    artifactMemoryMb: auto

    pressure:
      elevatedRatio: 0.70
      highRatio: 0.85
      criticalRatio: 0.95
      hysteresisRatio: 0.05
```

---

# 35. Resource Safety

User/config values MUST be bounded by Runtime hard limits.

Configuration MUST NOT permit:

```text
negative limits
unbounded queues
unbounded worker pools
unsafe GPU allocation
```

---

# 36. Lease Configuration

Example:

```yaml
runtime:
  leases:
    leakDiagnostics:
      warningAfterMs: 10000
      criticalAfterMs: 60000

    shutdown:
      waitForRelease: true
      timeoutMs: 3000
```

---

# 37. Lease Invariants

The following are architecture rules:

```text
shared resource requires explicit ownership/lease semantics
disposed resource cannot issue new valid lease
physical cleanup waits for eligibility where required
```

They MUST NOT be optional production toggles.

---

# 38. Runtime Cache Configuration

Runtime MAY configure implementation-level cache/resource limits.

Example:

```yaml
runtime:
  cache:
    memory:
      enabled: true
      maximumEntries: 2000
      maximumMemoryMb: auto
```

---

# 39. Cache Policy Boundary

Canonical cache compatibility and semantic reuse remain owned by `CACHE_POLICY.md` and the owning capability architecture.

Runtime Config does NOT define:

```text
whether Translation result A is semantically reusable for request B
```

---

# 40. Storage Boundary

Runtime Configuration MAY reference runtime-required persistence capabilities.

It MUST NOT contain business storage feature switches such as:

```text
glossaryEnabled
correctionMemoryEnabled
translationHistoryEnabled
```

Those belong to owning modules/features.

---

# 41. Provider Management Boundary

Canonical Provider Configuration is NOT Runtime Configuration.

Provider Management owns:

```text
provider registration
provider enablement
ProviderConfiguration
credential references
provider policy
provider metadata
```

---

# 42. Runtime Provider Projection

Runtime MAY consume a provider execution projection such as:

```text
deployment/binding reference
concurrency limit
attempt timeout
execution class
runtime isolation mode
```

after the owning provider/routing architecture has resolved it.

---

# 43. Provider Selection

Runtime Config MUST NOT directly define:

```yaml
primary: provider-a
fallback: provider-b
```

for business execution.

Selection belongs to:

```text
AI Routing
Recognition selection policy
Translation/provider-selection architecture
```

as appropriate.

---

# 44. AI Model Selection

Runtime Configuration MUST NOT contain canonical:

```text
model preference
AI provider preference
RoutePlan
Fallback Model
```

Those belong to AI architecture.

---

# 45. Plugin Configuration Boundary

Plugin-specific configuration belongs to:

```text
Plugin Configuration
```

Runtime Config MAY consume only runtime consequences such as:

```text
process isolation requirement
worker capacity
runtime resource requirements
```

---

# 46. Workspace / Project / Profile Boundary

Runtime Configuration MUST NOT duplicate:

```text
Workspace Policy
Project configuration
Profile processing intent
```

Resolved policy constraints MAY produce Runtime limits.

Example:

```text
Workspace disallows remote execution
    ->
Runtime receives no eligible remote execution binding
```

rather than:

```text
runtime.allowRemote = false
```

being a second policy source.

---

# 47. Privacy Boundary

Privacy policy is authoritative outside Runtime.

Runtime MUST enforce received privacy constraints.

Runtime Config MAY contain implementation safeguards such as:

```text
temporary cleanup interval
diagnostic content disabled
```

but MUST NOT override the governing privacy policy.

---

# 48. Presentation Boundary

Runtime Configuration does NOT own:

```text
font size
line height
side panel mode
show source text
```

These are Presentation/Preferences concerns.

Runtime MAY receive only performance/runtime consequences if applicable.

---

# 49. Capture Boundary

Runtime Configuration does NOT own:

```text
capture mode
capture cursor
capture interval
screen-region semantics
```

Capture owns these settings.

Runtime may own only scheduling/resource limits used while executing Capture work.

---

# 50. Reading Session Boundary

Session Configuration is not Runtime Configuration.

A Reading Session MAY resolve business settings into execution inputs.

Runtime receives:

```text
ExecutionScope
configuration references
operation constraints
```

not ownership of Session configuration.

---

# 51. Diagnostic Configuration

Runtime MAY own runtime-specific diagnostic settings such as:

```yaml
runtime:
  diagnostics:
    logLevel: info
    tracingEnabled: true
    runtimeSnapshotEnabled: true
```

---

# 52. Diagnostic Privacy

Runtime diagnostics MUST NOT enable raw reading content merely through an ordinary Runtime setting.

Sensitive diagnostic mode requires the appropriate Privacy/Security authorization.

---

# 53. Configuration Sources

Runtime Configuration SHOULD consume resolved configuration layers rather than define a universal CRAI precedence stack.

Possible Runtime-owned sources:

```text
Built-In Runtime Defaults
Runtime Profile
Persisted Runtime Configuration
Environment / CLI Overrides
Authorized Runtime Operation Override
```

---

# 54. Business Configuration Sources

Workspace/Project/Profile/Plugin/Provider configuration precedence belongs to those configuration owners.

Runtime MUST NOT merge them independently.

---

# 55. Merge Rules

Runtime-owned configuration MAY define standard merge semantics:

```text
Scalar:
    replace

Object:
    recursive or replace-only by schema

Array:
    REPLACE
    APPEND
    UNIQUE_APPEND
    MERGE_BY_ID

Value:
    ABSENT
    EXPLICIT_NULL
    CONCRETE_VALUE
```

Rules MUST be schema-defined.

---

# 56. Validation Levels

Recommended:

```text
Syntax Validation
        |
        v
Schema Validation
        |
        v
Runtime Semantic Validation
        |
        v
Environment Validation
        |
        v
Activation Validation
```

---

# 57. Runtime Semantic Validation

Examples:

```text
queue capacity <= supported maximum

worker count compatible with Runtime profile

critical pressure > high pressure

Retry budget non-negative

shutdown deadline >= required minimum

Artifact memory budget <= Runtime resource envelope
```

---

# 58. What Runtime Validation Must Not Do

Runtime validation MUST NOT reimplement:

```text
AI model compatibility
Workspace privacy policy
Plugin trust
Provider credential validity
Translation Profile semantics
```

It consumes results/references from those owners.

---

# 59. Environment Validation

Runtime MAY validate:

```text
memory availability
CPU availability
GPU/runtime availability
filesystem runtime path
process isolation support
required runtime service presence
```

---

# 60. Activation Modes

Recommended Runtime activation modes:

```text
IMMEDIATE_RUNTIME
NEW_ATTEMPT_ONLY
NEW_WORK_ITEM_ONLY
EXECUTION_SCOPE_RESTART
COMPONENT_RESTART
APPLICATION_RESTART
IMMUTABLE
```

---

# 61. IMMEDIATE_RUNTIME

Used only for settings whose update cannot alter execution semantics of in-flight work.

Examples:

```text
runtime log level
diagnostic sampling
```

---

# 62. NEW_ATTEMPT_ONLY

New Attempts use the new snapshot.

Existing Attempt keeps the previous snapshot.

Use carefully because one WorkItem may then contain Attempts with different Runtime config provenance.

---

# 63. NEW_WORK_ITEM_ONLY

New WorkItems use the new snapshot.

Existing WorkItems/Attempts preserve their existing snapshot.

This SHOULD be preferred for many execution-policy changes.

---

# 64. EXECUTION_SCOPE_RESTART

A change requires creation of another Execution Scope/business execution boundary.

Use only where the owning workflow explicitly supports restart.

Runtime MUST NOT redefine Reading Session lifecycle to implement this.

---

# 65. COMPONENT_RESTART

Requires:

```text
stop admission
quiesce affected component
drain/cancel work
dispose runtime instance
create replacement
validate
activate new snapshot
resume admission
```

---

# 66. APPLICATION_RESTART

Examples:

```text
data directory
process topology
secret-store implementation
core Event Bus implementation
```

---

# 67. IMMUTABLE

An immutable setting cannot be changed by ordinary Runtime configuration update.

---

# 68. Activation Impact Analysis

A candidate change SHOULD determine:

```text
affected Runtime components
affected Execution Scopes
affected WorkItems
affected Attempts
admission impact
resource impact
restart requirement
drain requirement
rollback strategy
```

---

# 69. Impact Analysis Boundary

Impact Analysis SHOULD NOT make Business Module decisions such as:

```text
restart Reading Session automatically
change Translation Profile
change provider preference
```

It reports runtime consequences to the owning application/orchestrator.

---

# 70. Configuration Change Flow

Recommended:

```text
Proposed Runtime Change
        |
        v
Build Candidate Snapshot
        |
        v
Validate
        |
        v
Impact Analysis
        |
        v
Persist Candidate State
        |
        v
Prepare Activation
        |
        v
Freeze Affected Admission if Required
        |
        v
Drain / Replace Runtime Components
        |
        v
Activate Snapshot
        |
        v
Resume Admission
```

---

# 71. Activation Atomicity

A candidate Runtime snapshot MUST NOT become partially active.

From an affected component's perspective:

```text
old valid snapshot
or
new valid snapshot
```

not a mixture.

---

# 72. Existing Attempts

Existing Attempts MUST retain their original configuration identity.

They MUST NOT observe an in-place mutable configuration object.

---

# 73. Persistence Ownership

Configuration architecture owns:

```text
meaning
schema
revision
activation
```

Storage provides:

```text
persistence implementation
```

Therefore:

```text
Configuration
    owns semantic state

Storage
    stores it
```

---

# 74. Configuration Persistence

Persistence SHOULD support:

* atomic write;
* revision identity;
* bounded backup;
* recovery;
* unsupported-version protection.

---

# 75. Secret Handling

Normal Runtime configuration stores only:

```text
SecretReference
CredentialReference
```

where a runtime-owned concern genuinely needs such a reference.

Raw secret values MUST NOT be persisted.

---

# 76. Secret Resolution

Runtime components SHOULD NOT resolve arbitrary secrets.

Preferred:

```text
Execution Adapter
    |
    v
Credential / Secret Host Boundary
```

when privileged execution requires it.

---

# 77. Versioning

Distinguish:

```text
RuntimeConfigurationSchemaVersion
RuntimeConfigurationRevision
RuntimeConfigurationSnapshotId
```

These are separate identities.

---

# 78. Migration

Schema migration SHOULD be:

* deterministic;
* explicit;
* validated before persistence;
* non-destructive to unsupported future versions;
* rollback-aware.

---

# 79. Rollback

Runtime configuration rollback means:

```text
reactivate previous compatible Runtime configuration revision
```

It does NOT automatically rollback:

* Domain state;
* Provider Configuration;
* Plugin Configuration;
* Storage schema;
* Reading Session business state.

---

# 80. Rollback Flow

Recommended:

```text
Activation Failed
        |
        v
Stop Affected Admission
        |
        v
Drain Candidate Runtime
        |
        v
Restore Previous Runtime Snapshot
        |
        v
Recreate Affected Runtime Components
        |
        v
Validate
        |
        v
Resume Admission
```

---

# 81. Safe Mode Runtime Configuration

Safe Mode MAY use a dedicated validated Runtime profile such as:

```text
conservative worker limits
remote execution unavailable
third-party plugin activation restricted
diagnostics enabled
temporary retention minimized
```

---

# 82. Safe Mode Boundary

Safe Mode MUST NOT weaken:

* stale-result rejection;
* cancellation authority;
* ownership transfer;
* Artifact immutability;
* Workspace isolation;
* permission enforcement.

---

# 83. Typed Runtime Views

Recommended:

```text
RuntimeConfigurationService
├── getExecutionConfig()
├── getSchedulerConfig()
├── getQueueConfig()
├── getRetryConfig()
├── getCancellationConfig()
├── getResourceConfig()
├── getLeaseConfig()
├── getArtifactConfig()
├── getRuntimeCacheConfig()
├── getShutdownConfig()
└── getRuntimeDiagnosticsConfig()
```

---

# 84. What Is Removed from Runtime Config Service

It SHOULD NOT expose generic ownership APIs such as:

```text
getProviderProfile()
getPresentationConfig()
getUserPreference()
getSessionConfig()
getTranslationConfig()
```

Those belong elsewhere.

---

# 85. Runtime Config Consumers

| Runtime Concern                     | Configuration Owner          |
| ----------------------------------- | ---------------------------- |
| Worker/execution limits             | Runtime Execution            |
| Scheduler                           | Scheduler                    |
| Work Queue                          | Queue                        |
| Retry                               | Retry Policy                 |
| Cancellation                        | Cancellation                 |
| Runtime Artifact operational limits | Runtime Artifact Store       |
| Resource budgets                    | Resource Manager             |
| Lease diagnostics/timeouts          | Resource Lifecycle           |
| Runtime Cache limits                | Cache Runtime implementation |
| Shutdown                            | Runtime Control / Bootstrap  |
| Runtime diagnostics                 | Runtime Observability        |

---

# 86. External Configuration Owners

| Concern                   | Canonical Owner        |
| ------------------------- | ---------------------- |
| Workspace Policy          | Workspace / Governance |
| Project settings          | Project                |
| Reading Session semantics | Reading Session        |
| Translation Profile       | Profile / Translation  |
| Recognition settings      | Recognition            |
| Presentation settings     | Presentation           |
| Provider Configuration    | Provider Management    |
| Plugin Configuration      | Plugin Architecture    |
| AI Routing                | AI Architecture        |
| Safety Policy             | Safety / Governance    |
| Secrets                   | Secret Management      |

---

# 87. Configuration Events

Recommended Runtime events:

```text
RuntimeConfigurationValidationSucceeded
RuntimeConfigurationValidationFailed
RuntimeConfigurationActivationStarted
RuntimeConfigurationActivated
RuntimeConfigurationActivationDeferred
RuntimeConfigurationActivationFailed
RuntimeConfigurationRollbackStarted
RuntimeConfigurationRollbackCompleted
```

---

# 88. Event Boundary

Runtime configuration events describe Runtime configuration state.

They are NOT Domain Events.

---

# 89. Audit Boundary

Material administrative configuration changes MAY require Audit.

Routine activation telemetry does not automatically become durable Audit.

---

# 90. Observability

Runtime Configuration diagnostics SHOULD expose:

```text
schema version
active revision
pending revision
snapshot ID
source references
changed Runtime fields
activation mode
affected Runtime components
drain/restart requirements
rollback result
```

---

# 91. Privacy

Diagnostics MUST NOT expose:

* secret values;
* reading content;
* raw Prompt/Context;
* arbitrary Business configuration content.

---

# 92. Error Categories

Possible:

```text
RUNTIME_CONFIG_PARSE_FAILED
RUNTIME_CONFIG_SCHEMA_UNSUPPORTED
RUNTIME_CONFIG_FIELD_UNKNOWN
RUNTIME_CONFIG_FIELD_REQUIRED
RUNTIME_CONFIG_VALUE_INVALID
RUNTIME_CONFIG_RELATION_INVALID
RUNTIME_CONFIG_ENVIRONMENT_UNSUPPORTED
RUNTIME_CONFIG_ACTIVATION_DEFERRED
RUNTIME_CONFIG_ACTIVATION_FAILED
RUNTIME_CONFIG_DRAIN_FAILED
RUNTIME_CONFIG_ROLLBACK_FAILED
RUNTIME_CONFIG_RESTART_REQUIRED
RUNTIME_CONFIG_WRITE_CONFLICT
RUNTIME_CONFIG_PERSISTENCE_FAILED
RUNTIME_CONFIG_MIGRATION_FAILED
```

Provider/plugin/business-specific failures SHOULD retain their owning taxonomy.

---

# 93. Runtime Configuration Invariants

1. Active Runtime Configuration is immutable.

2. Invalid Runtime Configuration does not activate.

3. Existing Attempts are not mutated by configuration changes.

4. Runtime execution retains RuntimeConfigurationSnapshot identity.

5. Runtime Configuration does not grant execution authority.

6. Runtime correctness invariants cannot be disabled by normal configuration.

7. Stale-result rejection cannot be configured off.

8. Cancellation authority cannot be configured off.

9. Artifact ownership transfer cannot be configured off.

10. Runtime Artifact immutability cannot be configured off.

11. Queue/concurrency remain bounded.

12. Retry and Fallback remain separate.

13. Runtime Retry does not select another provider/model route.

14. Runtime Configuration does not own Provider Configuration.

15. Runtime Configuration does not own Plugin Configuration.

16. Runtime Configuration does not own Workspace Policy.

17. Runtime Configuration does not own Project/Profile semantics.

18. Runtime Configuration does not own Reading Session business state.

19. Runtime Configuration does not own Presentation settings.

20. Runtime Configuration does not own source/target Language.

21. Runtime Configuration does not own AI Routing.

22. Raw secrets are absent from persisted normal Runtime configuration.

23. Runtime Components consume typed views only.

24. Runtime Components do not parse arbitrary config sources independently.

25. Runtime merge rules apply only to Runtime-owned configuration.

26. Runtime activation is explicit.

27. Partial activation is forbidden.

28. Component restart follows dependency/lifecycle rules.

29. Configuration rollback does not imply Business/Domain rollback.

30. Unsupported future schema is never overwritten blindly.

31. Runtime persistence is atomic where supported.

32. Migration semantics are deterministic.

33. Runtime configuration events contain no secret or reading content.

34. Runtime diagnostics use sanitized snapshots.

35. Configuration changes during shutdown do not activate.

36. ExecutionRevision and Domain revisions remain distinct.

37. Runtime Configuration Snapshot and Plugin Configuration Revision remain distinct.

38. Runtime Configuration Snapshot and Provider Configuration remain distinct.

39. Runtime Safety limits outrank ordinary Runtime overrides.

40. External authoritative Policy cannot be weakened by Runtime config.

---

# 94. Recommended MVP

CRAI MVP SHOULD support:

* one Runtime configuration schema;
* built-in Runtime defaults;
* persisted local Runtime settings;
* immutable Runtime Configuration snapshots;
* Runtime configuration revisions;
* typed Runtime views;
* Scheduler configuration;
* bounded Queue configuration;
* same-binding Retry configuration;
* cancellation grace configuration;
* resource budgets;
* lease diagnostics;
* Runtime Artifact limits;
* Runtime cache limits;
* shutdown timing;
* local Runtime diagnostics;
* atomic persistence;
* bounded backups;
* Safe Mode Runtime profile.

MVP SHOULD NOT place into Runtime Configuration:

```text
Provider selection
AI model selection
Translation style
source/target Language
Reading preferences
Presentation preferences
Plugin configuration
Workspace privacy policy
raw credentials
```

MVP MAY defer:

* remote Runtime configuration;
* live distributed configuration;
* complex adaptive Runtime tuning;
* per-Workspace Runtime worker pools;
* per-principal Runtime settings;
* distributed snapshot activation;
* automatic component replacement.

---

# 95. Open Decisions

The following remain open:

* YAML vs JSON;
* exact Runtime schema;
* schema-version strategy;
* RuntimeConfigurationSnapshot schema;
* revision format;
* configuration hash algorithm;
* Runtime profile representation;
* Runtime-owned source precedence;
* environment override policy;
* operation override policy;
* activation-mode taxonomy;
* NEW_ATTEMPT vs NEW_WORK_ITEM default;
* Runtime component restart mechanism;
* runtime configuration persistence backend;
* rollback retention;
* resource auto-sizing algorithm;
* hard Runtime safety limits;
* queue defaults;
* Retry defaults;
* Safe Mode Runtime limits;
* diagnostics defaults.

---

# 96. Related Documents

Runtime:

* `README.md`
* `RUNTIME_COMPONENTS.md`
* `BOOT_SEQUENCE.md`
* `PIPELINE_ORCHESTRATION.md`
* `PIPELINE_RUNTIME.md`
* `SCHEDULER.md`
* `WORK_QUEUE.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`
* `CACHE_POLICY.md`
* `MEMORY_MODEL.md`
* `THREADING_MODEL.md`
* `RESOURCE_LIFECYCLE.md`
* `PERFORMANCE_MODEL.md`
* `ERROR_MODEL.md`
* `RUNTIME_OBSERVABILITY.md`
* `PROCESS_TOPOLOGY.md`

Architecture:

* `../domain/WORKSPACE.md`
* `../domain/PROJECT.md`
* `../domain/PROFILE.md`
* `../ai/ROUTING.md`
* `../ai/RETRY.md`
* `../plugin/PLUGIN_CONFIGURATION.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/reading-session/`
* `../../02-modules/recognition/`
* `../../02-modules/translation/`
* `../../02-modules/presentation/`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/storage/`

---

# 97. Completion Criteria

`RUNTIME_CONFIG.md` is synchronized when:

* Runtime Configuration ownership is narrow and explicit;
* business/provider/plugin configuration is external;
* immutable Runtime snapshots remain;
* ExecutionRevision terminology is used;
* Scheduler/Queue/Retry/Cancellation configuration is Runtime-owned;
* Retry no longer contains Fallback;
* correctness invariants are not ordinary config toggles;
* Artifact/Lease safety rules remain architectural invariants;
* Provider selection is removed;
* Presentation/User Preferences/Session configuration are removed;
* raw secrets remain outside snapshots;
* activation/rollback semantics preserve in-flight execution;
* Runtime Config does not become a second CRAI configuration architecture.

---

# 98. Summary

CRAI Runtime Configuration follows:

```text
Authoritative Configuration Owners
        |
        v
Resolved Constraints / Inputs
        |
        v
Runtime Projection
        |
        v
Validate
        |
        v
Immutable RuntimeConfigurationSnapshot
        |
        v
Activate at Explicit Boundary
        |
        v
Typed Runtime Views
```

The central boundary is:

```text
Runtime Configuration
    controls execution mechanics.

It does not own business intent.
```
