# Runtime Boot Sequence

* **Document:** Runtime Architecture / Boot Sequence
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines the startup, readiness, degraded-start and rollback lifecycle of the CRAI application runtime.

Boot must ensure that:

* required dependencies are available before activation;
* Runtime execution authority exists before work is admitted;
* required ownership/storage/resource boundaries exist before publication;
* plugins/providers are activated only after validation and resolution;
* critical failures stop readiness safely;
* optional failures may produce degraded operation;
* partial initialization always has a cleanup path;
* application processing is enabled only when its required readiness profile is satisfied.

Boot coordinates startup.

It does NOT redefine ownership of the components it initializes.

---

# 2. Core Boot Principle

```text
Process Start
    |
    v
Bootstrap Configuration
    |
    v
Early Diagnostics
    |
    v
Configuration / Security Foundation
    |
    v
Infrastructure Foundation
    |
    v
Plugin / Provider Resolution
    |
    v
Runtime State / Resource Foundation
    |
    v
Scheduling / Execution Foundation
    |
    v
Runtime Authority
    |
    v
RUNTIME_READY
    |
    v
Application / UI Initialization
    |
    v
APPLICATION_READY
```

Critical distinction:

```text
RUNTIME_READY
    !=
APPLICATION_READY
```

---

# 3. Design Principles

1. Startup is dependency-aware.

2. Startup transition semantics are deterministic.

3. Critical dependencies fail fast.

4. Optional dependencies may degrade.

5. Independent initialization MAY execute concurrently.

6. Early diagnostics SHOULD start before critical component initialization.

7. No Runtime WorkItem is admitted before Runtime authority is ready.

8. No Runtime Artifact publication occurs before Artifact ownership infrastructure is ready.

9. No shared Resource Lease is issued before Resource lifecycle infrastructure is ready.

10. Boot MUST NOT create a Reading Session unless explicit application startup policy requests one after readiness.

11. Runtime initialization MUST NOT mirror Business Module names.

12. Partial initialization MUST have rollback/cleanup.

13. Cleanup follows reverse ownership/dependency order.

14. Raw secret values MUST NOT enter startup events or normal boot configuration snapshots.

15. Provider/plugin optional failures MUST NOT abort unrelated capabilities.

16. Safe Mode has an explicit entry path.

17. Boot telemetry MUST NOT become required for correctness.

18. `APPLICATION_READY` is emitted at most once per application instance.

---

# 4. Readiness Layers

CRAI SHOULD distinguish several readiness levels.

```text
PROCESS_READY
CONFIGURATION_READY
INFRASTRUCTURE_READY
RUNTIME_READY
APPLICATION_READY
```

Optional:

```text
DEGRADED
SAFE_MODE
```

These are operational states, not Business Module states.

---

# 5. Boot State Machine

Recommended:

```text
PROCESS_STARTING
        |
        v
BOOTSTRAP_LOADING
        |
        v
CONFIGURING
        |
        v
INFRASTRUCTURE_INITIALIZING
        |
        v
EXTENSIONS_RESOLVING
        |
        v
RUNTIME_INITIALIZING
        |
        v
RUNTIME_READY
        |
        v
APPLICATION_INITIALIZING
        |
        v
APPLICATION_READY
```

Failure/degraded states MAY include:

```text
DEGRADED
SAFE_MODE
BOOT_FAILED
SHUTTING_DOWN
```

---

# 6. PROCESS_STARTING

Responsibilities:

* create `ApplicationInstanceId`;
* create host/runtime epoch identity where required;
* establish process shutdown/cancellation signal;
* capture startup timestamp;
* identify platform/runtime environment;
* initialize emergency diagnostics sink.

No Runtime execution component is active yet.

---

# 7. Bootstrap Configuration

Bootstrap Configuration contains only values required to create the real configuration/infrastructure system.

Examples:

```text
environment
runtimeProfile
dataDirectory
configurationLocation
secretStoreBackend
recoveryMode
safeModeFlag
earlyDiagnosticDestination
pluginDiscoveryRoots?
```

It MUST remain minimal.

---

# 8. Bootstrap Validation

Validate only bootstrap-critical information:

* syntax;
* supported runtime profile;
* data-directory policy;
* supported configuration source;
* supported secret-store backend;
* safe-mode options;
* permitted plugin discovery roots where needed.

Failure MAY:

```text
Enter Safe Mode
or
BOOT_FAILED
```

according to policy.

---

# 9. Early Diagnostics

Early Diagnostics SHOULD provide:

* minimal structured logging;
* startup trace;
* bounded startup buffer;
* failure marker;
* redaction rules;
* emergency fallback sink.

It MUST NOT require:

* remote telemetry;
* provider availability;
* full application Storage.

---

# 10. Configuration Foundation

Initialize configuration infrastructure capable of resolving the initial runtime/application configuration.

Recommended:

```text
Load Defaults
    |
    v
Load Persisted Configuration
    |
    v
Apply Approved Overrides
    |
    v
Migrate if Required
    |
    v
Schema Validation
    |
    v
Semantic Validation
    |
    v
Create Immutable Configuration Snapshot
```

---

# 11. Configuration Ownership Boundary

Boot consumes:

```text
Resolved Configuration
```

It does not own configuration semantics.

Business/module configuration remains owned by the relevant architecture.

Runtime receives only the Runtime-relevant immutable projection/reference.

---

# 12. Secret / Credential Foundation

Boot SHOULD initialize the Secret/Credential access infrastructure.

It SHOULD NOT eagerly resolve every raw secret.

Preferred:

```text
Configuration
    contains SecretReference
        |
        v
Credential / Secret Boundary
        |
        v
privileged Adapter resolves when needed
```

---

# 13. Secret Reference Validation

Boot MAY validate:

* referenced secret store exists;
* required credential reference exists;
* access mechanism is operational;
* required security policy can be evaluated.

It SHOULD avoid exposing secret bytes.

---

# 14. Optional Credentials

A missing credential for an optional remote integration SHOULD normally produce:

```text
integration unavailable/degraded
```

rather than global boot failure.

---

# 15. Storage / Persistence Foundation

Initialize durable infrastructure required for boot.

Possible actions:

* open configured persistence backend;
* validate storage schema;
* run supported infrastructure migrations;
* load configuration/administrative state;
* load Registry state;
* load safe recovery metadata.

Storage MUST NOT infer or repair business semantics without owning-module rules.

---

# 16. Critical Storage

Storage criticality MUST be capability/use-case aware.

Examples:

```text
required Registry/config persistence unavailable
    -> critical

optional history unavailable
    -> degraded

optional durable cache unavailable
    -> degraded
```

---

# 17. Runtime Wiring / Container

Create the explicit runtime dependency graph.

Possible registered contracts:

```text
Runtime Configuration Snapshot
Clock
Event Bus
Storage
Telemetry
Secret/Credential Boundary
Plugin Registry
Plugin Discovery
Plugin Security
Plugin Lifecycle services
Execution State Store
Artifact Store
Runtime factories
```

A runtime container MUST NOT become an unrestricted global service locator.

---

# 18. Event Bus

Initialize Event Bus before components that require asynchronous public/runtime events.

Event Bus MAY start dispatch in a controlled/paused state.

It MUST NOT depend on remote telemetry.

---

# 19. Plugin Registry / Discovery Foundation

If plugins are enabled in the deployment, boot MAY initialize:

```text
Plugin Registry
Plugin Discovery
Compatibility Evaluator
Security / Permission Evaluation
Dependency Resolver
```

Discovery MUST NOT execute plugin code.

---

# 20. Plugin Boot Flow

Recommended:

```text
Load Registry State
        |
        v
Discover Installed/Built-In Candidates
        |
        v
Validate Descriptor / Artifact
        |
        v
Compatibility Evaluation
        |
        v
Security / Permission Evaluation
        |
        v
Dependency Resolution
        |
        v
Eligible Plugin Set
```

No activation has occurred yet.

---

# 21. Plugin Activation

Only plugins required/eager for current application runtime SHOULD be activated during boot.

Others MAY remain:

```text
ENABLED + RESOLVED + UNLOADED
```

for lazy activation.

---

# 22. Plugin Lifecycle Boundary

Boot requests lifecycle actions.

Plugin Manager coordinates:

```text
RESOLVED
    ->
LOADING
    ->
INITIALIZED
    ->
ACTIVE
```

according to `PLUGIN_LIFECYCLE.md`.

Boot MUST NOT implement plugin lifecycle logic independently.

---

# 23. Built-In Implementations

Built-in capability providers MAY participate in the same capability registry/binding model without pretending to be third-party plugins.

---

# 24. Resource Manager

Initialize Runtime physical resource management before shared leases are possible.

Responsibilities MAY include:

* resource registration;
* lease tracking;
* ownership transfer;
* disposal eligibility;
* cleanup coordination;
* leak diagnostics;
* resource-pressure projection.

---

# 25. Runtime Artifact Store

Initialize Runtime Artifact infrastructure after required resource lifecycle foundations.

Responsibilities MAY include:

* Artifact registration;
* Artifact references;
* atomic publication;
* Artifact lookup;
* lease integration;
* runtime retention;
* disposal coordination.

Runtime Artifact Store MUST NOT own durable Domain persistence.

---

# 26. Execution State Store

Initialize storage/projection required for:

```text
ExecutionScope
ExecutionRevision
WorkItem
Attempt lineage
accepted execution outcome
```

This is Runtime execution metadata.

It MUST remain distinct from Domain revisions/history.

---

# 27. Provider Management Boundary

Canonical Provider Management MAY already have been initialized as an Application Module/infrastructure dependency.

Boot consumes its validated projections.

Runtime MUST NOT recreate Provider Management semantics.

---

# 28. Provider Runtime Gateway

Initialize executable provider/adapter runtime access.

Recommended:

```text
Provider / Plugin Capability Registry
        |
        v
Executable Binding Resolution
        |
        v
Provider Runtime Gateway
```

---

# 29. Provider Runtime Gateway Responsibilities During Boot

Boot-time initialization MAY:

* construct required adapters;
* connect executable bindings;
* establish runtime concurrency controls;
* expose initial availability observations;
* perform bounded optional warmup.

It MUST NOT:

* choose AI models;
* choose Translation/Recognition provider for business requests;
* own Provider Configuration;
* own credentials;
* own canonical Provider Policy.

---

# 30. Provider Health

Boot-time provider probes are observations.

They MAY feed:

```text
Health Projection
```

but Runtime Gateway MUST NOT redefine canonical health architecture.

---

# 31. Provider Warmup

Provider/model warmup SHOULD normally be:

```text
optional
bounded
timeout-controlled
privacy-safe
```

unless a required local execution path cannot operate without it.

---

# 32. Scheduler Initialization

Scheduler initializes:

* admission policy;
* priorities;
* capacity constraints;
* ExecutionRevision freshness inputs;
* retry admission;
* replacement policy;
* resource-pressure inputs.

Initial state:

```text
ADMISSION_CLOSED
```

---

# 33. Work Queues / Execution Pools

Initialize:

* bounded queues;
* dispatchers;
* CPU execution pools;
* Provider I/O execution contexts;
* optional GPU/native contexts;
* maintenance/control execution capacity.

Dispatch MUST remain closed/paused until Runtime authority is ready.

---

# 34. Runtime Control

Runtime Control initializes after its required execution dependencies.

It SHOULD initialize:

* execution-command processing;
* Execution Scope state;
* Execution Revision authority;
* WorkItem logical state;
* Attempt lineage;
* cancellation coordination;
* Retry coordination;
* accepted execution outcome state;
* shutdown coordination;
* Scheduler/Queue integration.

---

# 35. Runtime Control Boundary

Runtime Control is NOT:

```text
owner of all runtime state
```

It is authority for:

```text
execution orchestration state
```

Scheduler, Configuration, Resource Manager, Plugin Runtime and other components retain their own state ownership.

---

# 36. Runtime Core Readiness

`RUNTIME_READY` MAY be reached when required Runtime core components are operational.

Typical checks:

```text
Runtime Configuration Snapshot Active
Event Bus Ready
Execution State Store Ready
Resource Manager Ready
Runtime Artifact Store Ready
Scheduler Ready
Work Queues Ready
Worker Execution Capacity Ready
Runtime Control Ready
Required Provider/Capability Runtime Available
```

---

# 37. Opening Runtime Admission

Preferred:

```text
Runtime Core Validation
        |
        v
RUNTIME_READY
        |
        v
Open Scheduler Admission
```

However no Application work should arrive until an application-facing command boundary is enabled.

---

# 38. Application Initialization

After `RUNTIME_READY`, application/module/UI concerns MAY initialize.

Examples:

* Reading Module application services;
* Presentation Module adapters;
* UI shell;
* user settings views;
* diagnostics UI;
* application command bridge.

These are not Runtime Components merely because boot initializes them.

---

# 39. Reading Session Boundary

Boot MUST NOT automatically create a Reading Session as part of Runtime initialization.

A Reading Session is created through:

* explicit user action;
* explicit resume policy;
* explicit application startup workflow.

Reading owns its business lifecycle.

---

# 40. Presentation Boundary

Boot MAY initialize Presentation infrastructure/application bindings.

It MUST NOT define a Runtime-owned:

```text
Presentation Runtime
```

solely to mirror the Presentation Module.

---

# 41. UI Initialization

UI MAY initialize before or after `RUNTIME_READY` depending on product UX.

Processing actions MUST remain unavailable until required readiness checks pass.

---

# 42. Application Readiness

`APPLICATION_READY` means the application's required interaction path is usable.

Possible requirements:

```text
RUNTIME_READY
Application command bridge ready
required Business Modules ready
Presentation/UI boundary ready where required
critical Storage capabilities ready
```

---

# 43. APPLICATION_READY Does Not Mean Everything Is Healthy

Optional components MAY remain:

```text
DEGRADED
UNAVAILABLE
LAZY
```

while the application is still usable.

---

# 44. Readiness Profiles

CRAI MAY eventually support capability-specific readiness.

Example:

```text
READING_READY
LOCAL_TRANSLATION_READY
REMOTE_AI_READY
```

MVP MAY use a simpler global Application readiness plus degraded capabilities.

---

# 45. Parallel Initialization

Independent startup branches MAY run concurrently.

Example after validated configuration:

```text
Storage Foundation
Secret/Credential Foundation
Plugin Descriptor Discovery
UI Static Asset Preparation
```

provided no ownership/dependency edge is violated.

---

# 46. Parallelism Rule

```text
dependency before concurrency
```

Parallel startup MUST NOT hide unresolved ordering requirements.

---

# 47. Critical Component

A component/dependency is critical only if required to preserve the selected application readiness profile.

Typical Runtime-core critical dependencies MAY include:

* valid configuration;
* required Runtime state;
* Resource Manager;
* Runtime Artifact Store;
* Scheduler;
* Queue/Worker capacity;
* Runtime Control;
* required persistence boundary.

---

# 48. Optional Components

Examples MAY include:

* remote AI provider;
* optional OCR engine;
* history;
* durable cache;
* remote telemetry exporter;
* GPU acceleration;
* optional plugin.

---

# 49. Degraded Boot

Application MAY become ready in degraded mode if:

* runtime correctness invariants remain valid;
* missing dependency is optional;
* at least one supported useful capability remains available;
* limitation is exposed to application/UI;
* cause is observable;
* no unbounded startup Retry occurs.

---

# 50. Degraded Example

```text
Remote AI Provider Unavailable
        |
        v
Local Recognition + Reading Still Available
        |
        v
APPLICATION_READY
        +
Remote Capability DEGRADED
```

---

# 51. Safe Mode

Safe Mode is a separately validated restricted operating profile.

Possible properties:

```text
remote providers disabled
third-party plugins disabled
experimental features disabled
conservative resource limits
automatic capture disabled
diagnostics enabled
configuration recovery enabled
```

Exact Safe Mode behavior is product/runtime policy.

---

# 52. Safe Mode Invariants

Safe Mode MUST preserve:

* authorization;
* Runtime execution authority;
* publication ownership;
* resource lifecycle;
* Workspace isolation;
* secret safety.

It MUST NOT mean:

```text
ignore validation and continue anyway
```

---

# 53. Boot Failure Handling

Recommended:

```text
Failure Detected
        |
        v
Prevent New Activation / Admission
        |
        v
Cancel Pending Boot Operations
        |
        v
Quiesce Already Activated Components
        |
        v
Drain / Cancel Active Startup Work
        |
        v
Release Leases
        |
        v
Dispose in Reverse Dependency Order
        |
        v
Flush Bounded Critical Diagnostics
        |
        v
Persist Safe Failure Marker
        |
        +--> SAFE_MODE
        |
        +--> PROCESS EXIT
```

---

# 54. Reverse Cleanup

Cleanup is based on the actual activated dependency graph.

It MUST NOT rely only on a hard-coded textual list.

Conceptually:

```text
Application/UI Bindings
        |
        v
Runtime Control
        |
        v
Queues / Workers / Scheduler
        |
        v
Provider / Plugin Runtime Bindings
        |
        v
Runtime Artifact / Execution State
        |
        v
Resource Manager
        |
        v
Event Bus
        |
        v
Infrastructure
        |
        v
Configuration / Persistence
```

Exact order depends on ownership.

---

# 55. Plugin Cleanup

Activated plugins SHOULD be quiesced/stopped/disposed according to Plugin Lifecycle before their runtime resources disappear.

Dependency shutdown order SHOULD be respected.

---

# 56. Provider Cleanup

Provider Runtime Gateway SHOULD stop accepting new invocations before adapters/processes are disposed.

Raw provider configuration/credentials remain owned elsewhere.

---

# 57. Shutdown During Boot

If application shutdown occurs during boot:

```text
Boot Cancellation
        |
        v
Stop Starting New Stages
        |
        v
Cancel Safe In-Progress Initialization
        |
        v
Rollback Activated Components
        |
        v
Flush Bounded Diagnostics
        |
        v
Exit
```

`APPLICATION_READY` MUST NOT subsequently be emitted.

---

# 58. Startup Events

Possible normalized events:

```text
BootStarted
BootstrapConfigurationReady
EarlyDiagnosticsReady
ConfigurationReady
InfrastructureReady
PluginDiscoveryCompleted
PluginResolutionCompleted
RuntimeArtifactStoreReady
ExecutionStateStoreReady
ProviderRuntimeReady
SchedulerReady
RuntimeControlReady
RuntimeReady
ApplicationDegraded
ApplicationReady
SafeModeEntered
BootFailed
```

Exact names follow Event Standard.

---

# 59. Startup Event Boundary

Startup events describe boot/runtime state.

They are NOT Domain Events.

---

# 60. Startup Event Rules

Startup events SHOULD:

* contain no raw user content;
* contain no secret values;
* include `ApplicationInstanceId`;
* include phase/stage;
* include normalized status;
* include duration when useful;
* include failure code;
* remain non-blocking for ordinary telemetry.

---

# 61. Audit Boundary

Routine startup stage completion is telemetry.

Material administrative/security actions MAY require Audit, such as:

* Safe Mode forced by administrator;
* untrusted plugin approved;
* security block overridden;
* migration forced;
* incompatible component override attempted.

---

# 62. Startup Metrics

Useful metrics MAY include:

```text
boot.total_ms
boot.configuration_ms
boot.storage_ms
boot.plugin_discovery_ms
boot.plugin_resolution_ms
boot.runtime_state_ms
boot.artifact_store_ms
boot.provider_runtime_ms
boot.scheduler_ms
boot.runtime_control_ms
boot.application_init_ms
boot.rollback_ms
boot.failure_total
boot.degraded_total
boot.safe_mode_total
```

---

# 63. Startup Diagnostics

Recommended snapshot:

```text
ApplicationInstanceId
BootState
CurrentStage
CompletedStages
PendingStages
FailedStage
FailureCode
DegradedCapabilities
ActiveConfigurationRevision
InitializedComponents
ActivatedPlugins
OpenResources
ActiveLeases
ElapsedTime
```

No raw payload or secret.

---

# 64. Startup Health Probes

Boot health probes MUST be:

* bounded;
* timeout-controlled;
* non-destructive;
* privacy-safe;
* optional where possible.

They MUST NOT require real user reading content.

---

# 65. Configuration Activation

Boot activates one fully validated initial Runtime Configuration Snapshot.

Runtime configuration update later uses:

```text
impact analysis
+
activation boundary
```

rather than rerunning the entire boot sequence.

---

# 66. Registry / Plugin Reconciliation

Boot MAY reconcile:

```text
Plugin Discovery Snapshot
        |
        v
Registry Snapshot
```

before activating plugins.

Discovery MUST NOT directly activate plugins.

---

# 67. Crash / Previous Runtime State

Previous persisted:

```text
ACTIVE
```

runtime/plugin state MUST NOT be trusted as proof of current activity after process restart.

New runtime instances must be constructed.

---

# 68. ExecutionRevision State Recovery

If Runtime execution state is restored after crash:

* stale previous attempts MUST NOT regain authority;
* resource leases from prior process epoch MUST be reconciled;
* accepted durable business results remain owned by their business/storage architecture.

Exact recovery semantics belong to detailed Runtime documents.

---

# 69. Startup Invariants

1. Startup is dependency-aware.

2. Every initialized component has a cleanup path.

3. Early diagnostics exist before critical initialization.

4. Initial configuration is fully validated before Runtime activation.

5. Raw secret values do not enter normal Runtime configuration snapshots.

6. Secret references and secret values remain distinct.

7. Storage and Runtime Artifact Store are separate.

8. Resource Manager is ready before shared leases.

9. Runtime Artifact Store is ready before Runtime Artifact publication.

10. Execution State Store is ready before Runtime authority becomes active.

11. Scheduler/Queues are initialized before admission opens.

12. Runtime Control is ready before Runtime execution authority is used.

13. Runtime Control is not the owner of every Runtime state.

14. Boot does not create a Reading Session by default.

15. Boot does not create mirror Runtime components for Business Modules.

16. Provider Runtime Gateway is separate from Provider Management.

17. Provider/model business selection is not performed by boot.

18. Optional provider/plugin failure may degrade rather than abort boot.

19. Required plugin dependencies resolve before plugin activation.

20. Plugin Discovery executes no plugin implementation code.

21. Enabled plugin does not imply active plugin.

22. Plugin runtime instances are newly constructed after process restart.

23. Scheduler admission stays closed until Runtime readiness.

24. Application processing stays disabled until Application readiness.

25. `RUNTIME_READY` and `APPLICATION_READY` are distinct.

26. Runtime Artifact publication and Domain commit are distinct.

27. Boot events do not determine boot correctness.

28. Telemetry exporter failure does not automatically fail boot.

29. Rollback follows actual dependency/ownership order.

30. Partial startup leaves no untracked owned resource where technically possible.

31. Startup telemetry contains no reading content by default.

32. Safe Mode still enforces authority/security/resource invariants.

33. Boot cancellation prevents later ApplicationReady.

34. `ApplicationReady` is emitted at most once.

35. `BootFailed` and successful `ApplicationReady` are mutually exclusive for one boot attempt.

---

# 70. Testing Requirements

Boot tests SHOULD include:

* normal boot;
* deterministic dependency ordering;
* independent-stage parallelism;
* invalid bootstrap config;
* invalid main configuration;
* configuration migration failure;
* missing optional credential;
* missing required credential reference;
* Storage unavailable;
* Registry unavailable;
* plugin descriptor invalid;
* plugin incompatible;
* plugin required dependency unresolved;
* optional plugin failure;
* provider runtime unavailable;
* Resource Manager failure;
* Runtime Artifact Store failure;
* Execution State Store failure;
* Scheduler failure;
* Runtime Control failure;
* degraded startup;
* Safe Mode;
* shutdown during boot;
* reverse cleanup;
* no Reading Session created automatically;
* admission remains closed before RuntimeReady;
* no Artifact publication before Artifact Store;
* no Lease before Resource Manager;
* plugin Discovery executes no plugin code;
* stale previous runtime state is not trusted;
* startup telemetry privacy;
* duplicate-ready prevention;
* telemetry backend failure.

---

# 71. MVP Boot Policy

CRAI MVP SHOULD support:

* one application process;
* local Configuration infrastructure;
* local persistence;
* local Event Bus;
* process-local Runtime Control;
* process-local Scheduler/Queues;
* process-local Resource Manager;
* process-local Runtime Artifact Store;
* process-local Execution State Store;
* bounded Worker execution;
* one Provider Runtime Gateway;
* built-in capability providers;
* installed trusted plugin discovery/activation where configured;
* lazy optional providers/plugins;
* optional remote provider adapters;
* one UI/Application shell;
* no automatic Reading Session;
* no mandatory cloud dependency.

MVP MAY defer:

* hot plugin discovery;
* hot plugin loading/unloading;
* remote plugin execution;
* distributed boot;
* multi-process Runtime authority;
* distributed Scheduler;
* automatic plugin marketplace installation;
* zero-downtime component replacement.

---

# 72. Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact boot state enum;
* exact RuntimeReady definition;
* exact ApplicationReady definition;
* UI shell before RuntimeReady;
* application-module initialization ordering;
* which persistence capabilities are critical;
* Safe Mode policy;
* repeated boot-failure threshold;
* plugin eager vs lazy activation;
* provider eager vs lazy adapter construction;
* provider/model warmup policy;
* local model boot criticality;
* Execution State recovery;
* Registry reconciliation ownership;
* Plugin Manager startup timing;
* early diagnostics retention;
* runtime wiring/container implementation;
* readiness timeout;
* capability-specific readiness;
* parallel initialization groups;
* rollback failure behavior;
* shutdown timeout during boot.

---

# 73. Related Documents

Runtime:

* `README.md`
* `RUNTIME_COMPONENTS.md`
* `RUNTIME_CONFIG.md`
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

Plugin:

* `../plugin/PLUGIN_SYSTEM.md`
* `../plugin/PLUGIN_DISCOVERY.md`
* `../plugin/PLUGIN_REGISTRY.md`
* `../plugin/PLUGIN_LIFECYCLE.md`
* `../plugin/PLUGIN_DEPENDENCY.md`
* `../plugin/PLUGIN_SECURITY.md`

AI:

* `../ai/MODELS.md`
* `../ai/ROUTING.md`
* `../ai/OBSERVABILITY.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/reading-session/`
* `../../02-modules/presentation/`
* `../../02-modules/storage/`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/telemetry/`

---

# 74. Summary

The CRAI boot sequence follows this ownership order:

```text
Configure
    |
    v
Establish Infrastructure
    |
    v
Resolve Extensions
    |
    v
Establish Runtime State / Resource Ownership
    |
    v
Establish Execution Capacity
    |
    v
Establish Runtime Authority
    |
    v
RUNTIME_READY
    |
    v
Establish Application Interaction
    |
    v
APPLICATION_READY
```

The critical rules are:

```text
No execution authority before Runtime Control.

No admission before Runtime readiness.

No Runtime Artifact publication before Artifact Store.

No shared Lease before Resource Manager.

No plugin activation before validation/resolution.

No raw secrets through Runtime contracts.

No Business Module ownership inside Runtime boot.

No user processing before Application readiness.
```
