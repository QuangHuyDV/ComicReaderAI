# Plugin Architecture

* **Architecture Area:** Plugin System
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The CRAI Plugin Architecture defines a controlled extension mechanism for adding or replacing selected implementation capabilities without changing CRAI business ownership.

Plugins MAY provide implementations for extension points such as:

* Capture sources,
* Recognition/OCR engines,
* Translation execution,
* AI/provider adapters,
* Dictionary/reference sources,
* Export targets,
* optional Storage adapters,
* future extension capabilities.

Plugins extend CRAI.

They do NOT replace CRAI modules, domains or business semantics.

---

# Core Principle

```text
CRAI Module / Capability
        |
        v
Public Extension Contract
        |
        v
Capability Provider
        |
        +--> Built-In Implementation
        |
        +--> Plugin Implementation
```

The owning CRAI module defines:

```text
what the capability means
```

The plugin defines:

```text
how one implementation provides it
```

---

# Critical Boundary

CRAI distinguishes:

```text
Module
    = business/application ownership boundary
```

from:

```text
Plugin
    = extension implementation mechanism
```

Therefore:

```text
Plugin
    !=
Module
```

and:

```text
Plugin
    !=
Domain
```

---

# Plugins Are Optional

CRAI MUST NOT require every internal implementation to be packaged as a plugin.

Possible implementation forms include:

```text
Built-In Implementation
Plugin Implementation
Infrastructure Adapter
Platform Adapter
Provider Adapter
```

The simplest appropriate mechanism SHOULD be used.

---

# Architecture Goals

The Plugin Architecture aims to provide:

* stable public extension contracts,
* implementation replaceability,
* capability-based composition,
* deterministic discovery,
* controlled lifecycle,
* dependency validation,
* scoped configuration,
* least-privilege execution,
* version compatibility,
* runtime isolation,
* safe third-party integration,
* observability,
* long-term extensibility.

---

# High-Level Architecture

```text
                     CRAI Host
                        |
                        v
                 Plugin Manager
                        |
        +---------------+---------------+
        |               |               |
        v               v               v
    Discovery        Registry       Lifecycle
        |               |               |
        +---------------+---------------+
                        |
                        v
                Capability Binding
                        |
            +-----------+-----------+
            |                       |
            v                       v
       Built-In                  Plugin
       Provider                 Provider
```

Cross-cutting concerns:

```text
Plugin API
Dependency Resolution
Configuration
Security
Versioning
Telemetry
Host Services
```

---

# CRAI Host

The CRAI Host is the trusted application environment in which plugins execute or connect.

The Host exposes only approved:

* Public Contracts,
* Capability Interfaces,
* Host Services,
* Events,
* Configuration Views,
* Security Context.

Plugins MUST NOT depend on private CRAI implementation details.

---

# Plugin Manager

The Plugin Manager coordinates:

* plugin lifecycle,
* activation eligibility,
* dependency/lifecycle orchestration,
* capability binding registration,
* enable/disable reactions,
* shutdown,
* runtime instance coordination.

It does NOT own every plugin-related concern.

---

# Plugin Manager Is Not

The Plugin Manager MUST NOT become:

```text
Universal Service Locator
Universal Capability Selector
Message Broker
Secret Store
Configuration Store
Health System
Telemetry Backend
```

Other architectures retain their own responsibilities.

---

# Plugin API

The Plugin API defines the public Host/plugin boundary.

It includes:

```text
Base Plugin Contract
Lifecycle Contract
Capability Contracts
Host Services
Configuration Contract
Permission Contract
Event Contract
Error Contract
Observability Contract
```

The Base Plugin Contract SHOULD remain minimal.

Business-specific methods belong to capability contracts.

---

# Capability Contracts

A plugin advertises public capability implementations.

Examples:

```text
capture.source
recognition.engine
translation.execution
ai.execution-provider
dictionary.source
export.target
storage.adapter
```

Capability identity MUST remain distinct from plugin identity.

---

# Capability Provider

Recommended mental model:

```text
Capability
    |
    +--> Built-In Provider
    +--> Plugin Provider
    +--> External Adapter
```

The relevant module/runtime selects among eligible providers.

Plugin Manager MUST NOT become the universal business selector.

---

# Discovery

Discovery locates candidate plugin artifacts and reads static metadata.

It MUST NOT execute plugin code.

Recommended flow:

```text
Discovery Source
      |
      v
Candidate Artifact
      |
      v
Manifest / Descriptor Candidate
      |
      v
Structural Checks
      |
      v
Discovery Result
```

---

# Discovery Boundary

A discovered plugin is NOT automatically:

```text
valid
compatible
trusted
enabled
registered
loaded
active
```

Those are later decisions.

---

# Registry

Plugin Registry stores canonical plugin registry metadata such as:

* plugin identity,
* descriptor,
* plugin version,
* capability declarations,
* dependency declarations,
* enablement state,
* compatibility state,
* configuration references,
* permission references,
* provenance.

---

# Registry Boundary

Registry does NOT own authoritative:

* runtime health,
* active request count,
* latency,
* error rate,
* worker state,
* runtime process state.

These belong to Runtime/Observability/Health projections.

---

# Registry vs Runtime

```text
Registry
    = what is known / configured
```

```text
Runtime
    = what is currently executing
```

These MUST remain separate.

---

# Enablement

Recommended administrative state:

```text
ENABLED
DISABLED
BLOCKED
```

`ENABLED` means:

```text
eligible for activation
```

not:

```text
currently ACTIVE
```

---

# Compatibility

Compatibility is a separate dimension.

Possible:

```text
COMPATIBLE
INCOMPATIBLE_API
INCOMPATIBLE_CAPABILITY
INCOMPATIBLE_DEPENDENCY
INCOMPATIBLE_PLATFORM
INCOMPATIBLE_RUNTIME
INCOMPATIBLE_CONFIGURATION
```

A plugin may be enabled but incompatible.

---

# Plugin Lifecycle

Recommended runtime lifecycle:

```text
DISCOVERED
    |
    v
VALIDATED
    |
    v
RESOLVED
    |
    v
LOADING
    |
    v
LOADED
    |
    v
INITIALIZING
    |
    v
INITIALIZED
    |
    v
ACTIVATING
    |
    v
ACTIVE
    |
    v
QUIESCING
    |
    v
STOPPING
    |
    v
STOPPED
    |
    v
DISPOSING
    |
    v
DISPOSED
    |
    v
UNLOADING
    |
    v
UNLOADED
```

Failure MAY occur at lifecycle stages:

```text
FAILED
```

---

# ACTIVE

`ACTIVE` is the canonical normal operational state.

A plugin is eligible for normal capability invocation only after successful activation.

---

# QUIESCING

Before shutdown:

```text
stop new work first
```

then:

```text
drain or cancel existing work
```

Only after that should destructive cleanup begin.

---

# Runtime Instance

Every loaded runtime SHOULD have:

```text
runtimeInstanceId
```

separate from:

```text
pluginId
pluginVersion
```

---

# Restart

Restart does NOT move the same disposed instance backward.

Preferred:

```text
Runtime Instance A
    -> UNLOADED

Runtime Instance B
    -> LOADING
    -> ...
    -> ACTIVE
```

---

# Dependency Architecture

Plugins MAY declare:

* required plugin dependencies,
* optional plugin dependencies,
* required capability dependencies,
* optional capability dependencies,
* Host Service dependencies,
* platform/runtime requirements.

---

# Capability-First Dependency

Prefer:

```text
requires capability X
```

over:

```text
requires plugin Y
```

when any compatible provider can satisfy the dependency.

---

# Dependency Resolution

Dependency Resolver answers:

```text
Can this plugin's prerequisites be satisfied?
```

It returns:

```text
DependencyResolution
```

and does NOT load plugins.

---

# RESOLVED

A plugin reaches lifecycle `RESOLVED` only when required prerequisites are satisfied.

Missing dependency means:

```text
UNRESOLVED
```

not automatically:

```text
BLOCKED
```

---

# Dependency Binding vs Business Routing

```text
Dependency Binding
    = activation/runtime prerequisite
```

```text
Business Routing
    = per-operation provider selection
```

They MUST remain separate.

---

# Configuration

Plugin Configuration contains only plugin-owned implementation settings.

Examples:

```text
workerCount
batchSize
implementation mode
local cache size
adapter-specific behavior
```

---

# Configuration Must Not Duplicate

Plugin Configuration SHOULD NOT become a duplicate home for:

```text
Workspace Policy
AI Routing Policy
Retry Policy
Fallback Policy
Global Cache Policy
Translation Profile
Canonical Language settings
Provider credentials
Workspace budgets
```

---

# Resolved Configuration

Plugins consume an immutable:

```text
ResolvedPluginConfiguration
```

or:

```text
PluginConfigurationView
```

for one operation/runtime revision.

Plugins do NOT merge raw configuration layers themselves.

---

# Configuration Scope

Possible scopes MAY include:

```text
SYSTEM
WORKSPACE
PROJECT
PRINCIPAL
SESSION
OPERATION_OVERRIDE
```

Not every field supports every scope.

---

# Configuration Update Modes

Fields SHOULD declare:

```text
HOT_RELOADABLE
RESTART_REQUIRED
RELOAD_REQUIRED
IMMUTABLE_AFTER_INSTALL
READ_ONLY_DERIVED
```

---

# Security

Plugin Security protects CRAI through:

* trust evaluation,
* explicit permissions,
* Host Service enforcement,
* isolation,
* package integrity,
* Workspace isolation,
* secret minimization,
* revocation,
* containment.

---

# Trust vs Permission

```text
Trust
    = confidence in artifact/provenance
```

```text
Permission
    = authority to access a resource/action
```

Trust MUST NOT automatically grant unrestricted permissions.

---

# Trust Levels

Possible conceptual classes:

```text
BUILT_IN
VERIFIED
USER_APPROVED
UNTRUSTED
DEVELOPMENT
BLOCKED
```

Exact policy remains configurable.

---

# Permissions

Possible permissions MAY include:

```text
NETWORK
FILE_READ
FILE_WRITE
CLIPBOARD_READ
CLIPBOARD_WRITE
SCREEN_CAPTURE
STORAGE_READ
STORAGE_WRITE
EVENT_PUBLISH
EVENT_SUBSCRIBE
SECRET_REFERENCE_USE
LOCAL_PROCESS_EXECUTION
LOCAL_MODEL_EXECUTION
```

---

# Capability Is Not Permission

These are capabilities:

```text
Recognition
Translation
AI
Dictionary
Export
```

They MUST NOT be modeled as generic security permissions.

---

# Host Service Boundary

Preferred:

```text
Plugin
   |
   v
Host Service
   |
   v
Permission Check
   |
   v
Sensitive Resource
```

Sensitive resources SHOULD NOT be exposed directly.

---

# Secrets

Canonical secrets belong to:

* Secret Management,
* Provider Management.

Plugin Configuration SHOULD contain only references where possible.

Preferred:

```text
credentialReference
    |
    v
Credential Broker / Provider Adapter
    |
    v
Secret Store
```

---

# Raw Secret Access

Raw secret access SHOULD be exceptional and explicitly permissioned.

Plugins MUST NOT persist or log raw secrets.

---

# Workspace Isolation

Plugins MUST preserve tenant boundaries.

A shared plugin runtime MUST NOT mix Workspace data.

---

# Principal Authorization

Where user/principal permissions matter, authorization context must propagate through capability calls.

---

# Execution Isolation

Plugins MAY run:

```text
IN_PROCESS
OUT_OF_PROCESS
SANDBOXED
REMOTE
```

depending on:

* trust,
* permissions,
* capability,
* performance,
* platform.

---

# In-Process Limitation

In-process plugins cannot provide perfect fault/security isolation.

CRAI MUST NOT claim otherwise.

Higher-risk plugins SHOULD prefer stronger isolation where practical.

---

# Versioning

Plugin architecture distinguishes:

```text
PluginVersion
PluginApiVersion
CapabilityContractVersion
ConfigurationSchemaVersion
DependencyVersion
CRAIVersion
PlatformVersion
RuntimeVersion
```

No single version defines all compatibility.

---

# Plugin Version

```text
pluginVersion
    = implementation release
```

It MAY follow Semantic Versioning.

---

# Plugin API Version

```text
pluginApiVersion
    = generic Host ↔ Plugin contract
```

It evolves independently from plugin release version.

---

# Capability Version

```text
capabilityContractVersion
    = extension capability contract
```

Example:

```text
recognition.engine/v1
recognition.engine/v2
```

---

# Configuration Schema Version

```text
configurationSchemaVersion
    = configuration structure/meaning
```

It is distinct from configuration data revision.

---

# CRAI Version

Supported CRAI application version is a coarse compatibility hint.

Exact Plugin API/capability/dependency/platform checks are more authoritative.

---

# Compatibility Evaluation

Recommended:

```text
Artifact Metadata
    |
    v
Plugin API Compatibility
    |
    v
Capability Compatibility
    |
    v
Dependency Compatibility
    |
    v
Configuration Compatibility
    |
    v
Platform / Runtime Compatibility
    |
    v
Security Requirements
    |
    v
Compatibility Result
```

---

# Upgrade

Plugin upgrade creates:

```text
new plugin version
+
new runtime instance
```

It MUST NOT mutate the existing runtime instance into another version.

---

# Rollback

Rollback means:

```text
previous compatible artifact
+
compatible configuration/data
+
NEW runtime instance
```

It does NOT mean restoring old in-memory runtime objects.

---

# Plugin Upgrade Ownership

Upgrade spans several concerns:

```text
Installer
    installs artifact

Versioning
    evaluates compatibility

Security
    reevaluates trust/permissions

Configuration
    handles configuration migration

Plugin Manager
    coordinates lifecycle transition
```

No single component owns the entire upgrade workflow.

---

# Failure Isolation

Plugin failures SHOULD be contained to the affected plugin/runtime scope where technically possible.

A plugin failure SHOULD NOT corrupt unrelated CRAI state.

---

# Failure Boundary

For out-of-process plugins:

```text
plugin crash
    MUST NOT
crash Host process
```

For in-process plugins, isolation guarantees are weaker.

---

# Capability Failure

A failed plugin may cause only its provided capabilities to become unavailable.

The owning module/runtime may select another provider when available.

---

# Observability

Plugin runtime SHOULD emit structured telemetry for:

* discovery,
* validation,
* compatibility,
* dependency resolution,
* load/init/activation,
* lifecycle transitions,
* capability registration,
* permission denial,
* security findings,
* plugin crash,
* configuration update,
* upgrade/rollback.

---

# Audit

Material actions MAY require Audit:

* plugin installed/removed,
* enabled/disabled,
* blocked/unblocked,
* sensitive permission granted/revoked,
* untrusted plugin approved,
* plugin upgraded/downgraded,
* rollback,
* security override.

Ordinary capability calls and lifecycle telemetry SHOULD NOT automatically become durable Audit records.

---

# Event Bus

Plugins MAY publish and subscribe only through approved public event contracts.

Event Bus is appropriate for:

```text
asynchronous notifications
```

It is NOT the required mechanism for every plugin interaction.

---

# Synchronous Interaction

Synchronous capability invocation SHOULD use typed public contracts.

Example:

```text
Capability Request
    |
    v
Capability Provider
    |
    v
Capability Response
```

---

# Direct Plugin Communication

Plugins MUST NOT depend on another plugin's private API.

Allowed:

```text
Plugin A
    |
    v
Public Capability X
    |
    v
Provider B
```

Caller depends on Capability X, not Provider B identity.

---

# Storage

Plugins needing private persistence use approved Storage Host Services.

They MUST NOT access CRAI internal databases directly.

---

# Plugin Data

Plugin-private data remains distinct from canonical CRAI domain resources.

Unloading/removing a plugin MUST NOT erase canonical CRAI business truth.

---

# Extension Areas

Initial extension surfaces MAY include:

```text
Capture Source
Recognition Engine
Translation Execution
AI Provider / Adapter
Dictionary / Reference Source
Export Target
Optional Storage Adapter
```

These are extension surfaces, not new domain owners.

---

# Capture Extension

Capture plugins provide capture implementations.

Capture module owns capture semantics.

---

# Recognition Extension

Recognition plugins provide OCR/detection/layout implementations.

Recognition module owns recognition semantics and canonical TextBlock integration.

---

# Translation Extension

Translation plugins provide translation execution implementations.

Translation module owns Translation lifecycle and revisions.

---

# AI Extension

AI plugins provide provider/runtime adapters or AI execution capabilities.

AI Routing chooses eligible execution routes.

Business modules MUST NOT depend on concrete provider/plugin identity.

---

# Dictionary Extension

Dictionary plugins may provide reference data.

They MUST NOT automatically become canonical Glossary truth.

---

# Export Extension

Export plugins provide output formatting/target integrations.

Export must not mutate canonical source/Translation state.

---

# Storage Extension

Storage plugins MAY provide optional infrastructure adapters only where CRAI exposes an explicit storage extension contract.

Storage architecture retains persistence semantics.

---

# Document Structure

```text
01-architecture/plugin/
│
├── README.md
├── PLUGIN_SYSTEM.md
├── PLUGIN_API.md
├── PLUGIN_REGISTRY.md
├── PLUGIN_DISCOVERY.md
├── PLUGIN_LIFECYCLE.md
├── PLUGIN_DEPENDENCY.md
├── PLUGIN_CONFIGURATION.md
├── PLUGIN_SECURITY.md
└── PLUGIN_VERSIONING.md
```

---

# Document Overview

| Document                  | Purpose                                                        |
| ------------------------- | -------------------------------------------------------------- |
| `README.md`               | Plugin architecture overview and ownership boundaries          |
| `PLUGIN_SYSTEM.md`        | Overall plugin extension model and Host/plugin boundary        |
| `PLUGIN_API.md`           | Public Plugin API, capability contracts and Host Services      |
| `PLUGIN_REGISTRY.md`      | Canonical plugin registry metadata and capability declarations |
| `PLUGIN_DISCOVERY.md`     | Safe artifact/manifest discovery without code execution        |
| `PLUGIN_LIFECYCLE.md`     | Runtime plugin lifecycle and shutdown semantics                |
| `PLUGIN_DEPENDENCY.md`    | Dependency declaration, resolution and ordering                |
| `PLUGIN_CONFIGURATION.md` | Plugin-specific configuration resolution and updates           |
| `PLUGIN_SECURITY.md`      | Trust, permissions, isolation and security containment         |
| `PLUGIN_VERSIONING.md`    | Plugin/API/capability/config versioning and compatibility      |

---

# Recommended Reading Order

Recommended order:

```text
1. README.md
2. PLUGIN_SYSTEM.md
3. PLUGIN_API.md
4. PLUGIN_REGISTRY.md
5. PLUGIN_DISCOVERY.md
6. PLUGIN_LIFECYCLE.md
7. PLUGIN_DEPENDENCY.md
8. PLUGIN_CONFIGURATION.md
9. PLUGIN_SECURITY.md
10. PLUGIN_VERSIONING.md
```

---

# Reading Logic

```text
Architecture Boundary
        |
        v
Extension Contracts
        |
        v
Known Plugin Metadata
        |
        v
Discovery
        |
        v
Runtime Lifecycle
        |
        v
Dependency Resolution
        |
        v
Configuration
        |
        v
Security
        |
        v
Version Compatibility
```

---

# Integration with CRAI Modules

Plugins integrate with CRAI modules only through public extension contracts.

Relevant modules MAY include:

```text
Capture
Recognition
Translation
Provider Management
Storage
Presentation / Export
```

---

# Plugin vs Provider Management

Provider Management owns:

* Provider Configuration,
* credential references,
* provider enablement,
* provider availability,
* provider policy.

Provider plugins provide implementation support.

They MUST NOT duplicate provider ownership.

---

# Plugin vs AI Routing

Plugin Registry may expose AI execution providers.

AI Routing decides:

```text
which route/provider/model handles an operation
```

Plugin architecture does not replace AI Routing.

---

# Plugin vs Runtime

Runtime owns:

* workers,
* scheduling,
* queues,
* process execution,
* cancellation,
* resource lifecycle mechanics.

Plugin Lifecycle defines plugin-specific runtime semantics implemented using Runtime.

---

# Plugin vs Configuration Infrastructure

Plugin Configuration defines meaning/resolution.

Configuration infrastructure provides persistence and resolution mechanics.

---

# Plugin vs Storage Infrastructure

Storage provides persistence.

Plugin architecture does not own Storage technology.

---

# Plugin vs Security Infrastructure

Plugin Security defines trust/permission semantics.

Infrastructure provides:

* sandboxing,
* signature verification,
* secret management,
* resource enforcement.

---

# Plugin vs Observability

Plugin architecture emits normalized telemetry.

Observability infrastructure owns collection/storage/aggregation.

---

# Plugin vs Event Bus

Event Bus transports approved asynchronous events.

Plugin Manager is not the Event Bus.

---

# Architecture Invariants

1. CRAI modules/domains remain owners of business semantics.

2. Plugins are extension implementations, not module/domain owners.

3. CRAI Core MUST NOT depend on concrete plugin implementations.

4. Plugins depend only on approved public contracts and Host Services.

5. Not every CRAI implementation must be a plugin.

6. Built-in and plugin implementations MAY coexist.

7. Plugin is not Module.

8. Plugin is not Domain.

9. Plugins declare capabilities explicitly.

10. Capability identity is distinct from plugin identity.

11. Capability contracts SHOULD be preferred over implementation-specific dependencies.

12. Plugin Manager coordinates lifecycle transitions.

13. Plugin Manager MUST NOT become a universal service locator.

14. Plugin Manager MUST NOT become a message broker.

15. Plugin Manager MUST NOT become a universal capability selector.

16. Plugin Registry is authoritative for registry metadata, not all runtime state.

17. Registry lifecycle/admin metadata and runtime Health/Statistics remain distinct.

18. Discovery MUST NOT execute plugin code.

19. Discovery produces candidates, not canonical activation.

20. Validation occurs before loading.

21. Required dependencies must resolve before activation.

22. `RESOLVED` is distinct from `VALIDATED`.

23. ACTIVE is the canonical normal runtime state.

24. New capability work must stop before destructive shutdown.

25. Restart creates a new runtime instance.

26. Plugin runtime identity is distinct from plugin identity/version.

27. Dependency Resolver does not load plugins.

28. Dependency Resolver does not perform business routing.

29. Plugin configuration is implementation-specific and scoped.

30. Plugins consume resolved immutable configuration views.

31. Plugin Configuration MUST NOT duplicate canonical CRAI policies without explicit ownership.

32. Raw secrets SHOULD NOT live in ordinary plugin configuration.

33. Sensitive resources require explicit permission grants.

34. Trust and permission are distinct.

35. Plugin activation does not imply unrestricted authority.

36. Host Services SHOULD enforce permissions at resource boundaries.

37. Workspace isolation MUST be preserved across plugin calls.

38. Plugins MUST NOT access CRAI internal databases directly.

39. Plugins MUST NOT depend on another plugin's private implementation APIs.

40. Event Bus is not the only plugin communication mechanism.

41. Synchronous capability calls SHOULD use typed contracts.

42. Plugin Version, Plugin API Version and Capability Contract Version are distinct.

43. Configuration Schema Version is distinct from configuration revision.

44. CRAI application version alone MUST NOT determine compatibility.

45. Upgrade creates a new runtime instance.

46. Rollback does not restore disposed in-memory runtime state.

47. New permissions introduced by upgrades MUST NOT be silently granted.

48. Plugin failures SHOULD be isolated according to the execution-isolation model.

49. In-process plugins have weaker containment guarantees.

50. Plugin telemetry MUST avoid secrets/private values by default.

51. Material plugin/security/configuration changes MAY require Audit.

52. Plugin runtime events are not automatically Domain Events.

53. Plugin-private persistent data remains distinct from canonical CRAI domain truth.

54. Unloading/removing a plugin MUST NOT delete canonical CRAI business data.

55. New extension capabilities SHOULD integrate through public contracts rather than core architectural rewrites.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* stable plugin identity,
* static Plugin Descriptor,
* Plugin API version,
* capability declarations,
* capability contract versions,
* Plugin Registry,
* Plugin Discovery,
* descriptor validation,
* enable/disable/block states,
* compatibility evaluation,
* dependency resolution,
* lifecycle activation/shutdown,
* runtimeInstanceId,
* built-in + plugin capability providers,
* scoped plugin configuration,
* configuration revisions,
* explicit permissions,
* basic trust classification,
* controlled Network/File/Clipboard/Storage Host Services,
* credential references,
* Workspace isolation,
* plugin errors,
* plugin diagnostics,
* manual plugin upgrade,
* safe rollback where possible.

MVP SHOULD prioritize:

```text
trusted / controlled plugins
stable contracts
correct ownership boundaries
simple lifecycle
safe configuration
least privilege
```

over a fully open plugin marketplace.

---

# Deferred Capabilities

CRAI SHOULD initially defer:

* unrestricted third-party plugin marketplace,
* automatic marketplace installation,
* arbitrary untrusted in-process plugins,
* full OS sandboxing across all platforms,
* remote plugins,
* hot upgrade,
* zero-downtime plugin replacement,
* multiple active versions,
* sophisticated dependency SAT solving,
* dynamic runtime rebinding,
* complex cross-plugin service graphs,
* automatic plugin updates,
* distributed plugin registry.

---

# Open Architecture Questions

The following SHOULD remain explicit until implementation/prototype validation:

* exact Plugin Descriptor schema,
* plugin package format,
* plugin ID naming scheme,
* Base Plugin interface,
* Host Service injection,
* capability ID/version scheme,
* Registry persistence,
* Registry vs Runtime Capability Index separation,
* discovery source layout,
* runtime isolation default,
* out-of-process IPC,
* lifecycle enum,
* dependency resolver implementation,
* configuration schema format,
* configuration scope precedence,
* permission taxonomy,
* permission-grant storage,
* trust model,
* signature policy,
* credential broker interface,
* Workspace-isolated runtime instances,
* plugin-private storage namespace,
* Plugin API version negotiation,
* capability-version negotiation,
* upgrade orchestration,
* configuration migration,
* plugin-private data migration,
* rollback policy,
* future Marketplace architecture.

---

# Related Architecture

Architecture:

* `../core/`
* `../domain/`
* `../modules/`
* `../runtime/`

AI:

* `../ai/`

Modules:

* `../../02-modules/capture/`
* `../../02-modules/recognition/`
* `../../02-modules/translation/`
* `../../02-modules/provider-management/`
* `../../02-modules/storage/`
* `../../02-modules/presentation/`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/logging/`
* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/telemetry/`

---

# Canonical Mental Model

The Plugin Architecture can be summarized as:

```text
CRAI Capability
      |
      v
Public Extension Contract
      |
      v
Capability Provider Set
      |
      +--> Built-In Provider
      |
      +--> Plugin Provider
              |
              v
        Plugin Lifecycle
              |
              v
        Host Service Boundary
              |
              v
        Controlled Resources
```

Supporting concerns:

```text
Discovery
Registry
Dependency Resolution
Configuration
Security
Versioning
Observability
```

---

# Final Principle

The core Plugin Architecture boundary is:

```text
CRAI owns meaning

Plugins provide implementations

Public contracts connect them
```

The Plugin System exists to make CRAI extensible without transferring business ownership to plugin implementations.
