# Plugin System

* **Document:** Plugin Architecture / Plugin System
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The CRAI Plugin System defines a controlled extension mechanism for adding or replacing selected implementation capabilities without modifying CRAI core business architecture.

Plugins MAY provide implementations for extension points such as:

* Recognition/OCR engines,
* AI/provider integrations,
* Translation engines,
* Capture adapters,
* Export formats,
* Dictionary/reference integrations,
* optional Storage adapters,
* future extension capabilities.

The Plugin System enables:

* extensibility,
* implementation replaceability,
* capability discovery,
* controlled third-party integration,
* versioned public contracts,
* permission isolation,
* runtime lifecycle management.

Plugins extend CRAI.

They do NOT replace CRAI module/domain ownership.

---

# Core Principle

```text
CRAI Capability / Module
        |
        v
Public Extension Contract
        |
        v
Plugin Capability Binding
        |
        v
Plugin Implementation
```

The owning CRAI module defines:

```text
business semantics
```

The plugin provides:

```text
an implementation of an allowed extension contract
```

---

# Critical Boundary

Plugin architecture MUST distinguish:

```text
Capability Ownership
    = CRAI modules/domains
```

from:

```text
Capability Implementation
    = core implementation or plugin implementation
```

Example:

```text
Recognition Module
    owns recognition semantics

PaddleOCR Plugin
    provides one recognition implementation
```

Likewise:

```text
Translation Module
    owns Translation workflow

DeepL Plugin
    provides one translation execution capability
```

---

# Plugins Are Optional Extension Mechanisms

CRAI MUST NOT require every internal implementation to be packaged as a plugin.

Possible implementation forms include:

```text
Built-in implementation
Plugin implementation
Infrastructure adapter
Platform adapter
Provider adapter
```

The architecture SHOULD use the simplest form appropriate to the component.

---

# Non-Goals

The Plugin System does NOT own:

* Translation business truth,
* Recognition business truth,
* Capture lifecycle,
* Presentation semantics,
* Storage domain semantics,
* Provider credentials,
* Workspace Policy,
* AI Routing,
* Event Bus semantics,
* runtime Scheduler,
* canonical configuration truth.

---

# Design Goals

The Plugin System SHOULD provide:

* extensible architecture,
* loose coupling,
* stable public contracts,
* explicit capability registration,
* deterministic discovery,
* controlled loading,
* safe initialization/shutdown,
* dependency validation,
* version compatibility,
* permission isolation,
* failure isolation,
* runtime replaceability where safe,
* observability,
* platform independence.

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
          +-------------+-------------+
          |                           |
          v                           v
   Built-in Provider            Plugin Provider
```

Cross-cutting concerns:

```text
Permissions
Security
Configuration
Diagnostics
Telemetry
Version Compatibility
```

---

# CRAI Host

The CRAI Host represents the trusted application environment in which plugins execute or to which isolated plugins connect.

The Host exposes only approved:

```text
Host Services
Public Contracts
Capability Interfaces
Events
Configuration Views
```

Plugins MUST NOT depend on internal implementation details.

---

# Plugin Manager

The Plugin Manager coordinates plugin lifecycle and capability binding.

Primary responsibilities:

* trigger discovery,
* validate plugin descriptors,
* coordinate dependency resolution,
* coordinate loading,
* create plugin instances,
* initialize plugins,
* register capabilities,
* enable/disable plugins,
* coordinate shutdown,
* remove capability bindings,
* publish lifecycle signals,
* expose plugin status.

The Plugin Manager MUST NOT become the owner of unrelated business semantics.

---

# Plugin Manager Is Not a Service Locator

Business code SHOULD NOT use:

```text
PluginManager.get("DeepL")
```

or:

```text
PluginManager.get("PaddleOCR")
```

as the normal integration mechanism.

Preferred:

```text
RecognitionCapability
TranslationExecutionCapability
CaptureSourceCapability
```

resolved through explicit capability contracts.

---

# Plugin Manager Is Not a Message Broker

Plugin Manager MAY:

* resolve dependencies,
* bind capability references,
* coordinate lifecycle.

It SHOULD NOT mediate every runtime message between plugins.

Runtime communication SHOULD use:

* public service contracts,
* capability bindings,
* Event Bus where asynchronous communication is appropriate.

---

# Plugin Descriptor

Every plugin MUST expose a machine-readable descriptor/manifest.

Recommended:

```text
PluginDescriptor
├── pluginId
├── pluginVersion
├── pluginApiVersion
├── displayName?
├── publisher?
├── entryPoint
├── capabilities[]
├── requiredPermissions[]
├── requiredDependencies[]
├── optionalDependencies[]
├── supportedPlatforms[]
├── supportedCRAIRange?
├── configurationSchemaReference?
├── executionModel?
└── integrityMetadata?
```

Exact schema is defined by related Plugin documents.

---

# Plugin Identity

Every plugin MUST have a stable:

```text
pluginId
```

Plugin identity MUST NOT be inferred from:

* display name,
* provider name,
* filename,
* installation path.

---

# Plugin Version

Plugin version identifies the implementation release.

It remains distinct from:

```text
Plugin API version
Capability contract version
CRAI application version
Provider API version
```

---

# Extension Points

CRAI exposes explicit extension points.

An extension point defines:

```text
what capability may be extended
what contract must be implemented
what permissions may be required
what lifecycle semantics apply
```

---

# Extension Point vs Plugin Category

CRAI SHOULD prefer:

```text
Capability / Extension Contract
```

over hard-coded plugin categories.

Example:

```text
RecognitionEngineCapability
```

may be implemented by:

* PaddleOCR,
* Tesseract,
* Windows OCR,
* Apple Vision,
* future engines.

---

# Recommended Extension Areas

Initial CRAI extension areas MAY include:

```text
Capture Source
Recognition Engine
Translation Execution
AI Provider / Model Adapter
Dictionary / Reference Source
Export Target
Optional Storage Adapter
```

These are extensibility surfaces.

They are NOT new business-domain owners.

---

# Capture Extensions

A Capture plugin MAY provide access to a source such as:

* browser surface,
* desktop screen,
* application window,
* mobile capture source,
* platform-specific capture API.

The Capture module remains authoritative for capture workflow semantics.

---

# Recognition Extensions

Recognition plugins MAY expose:

```text
text detection
OCR recognition
layout recognition
reading-order support
language/script hints
```

depending on the public Recognition extension contract.

The Recognition module owns normalization and domain integration.

---

# Translation Extensions

Translation plugins MAY provide translation execution through:

* local models,
* external translation services,
* AI models,
* specialized translation engines.

The Translation module owns Translation lifecycle and revision semantics.

---

# AI Extensions

AI extensions MAY expose provider/runtime capabilities used by AI architecture.

Examples:

* text generation,
* structured output,
* multimodal execution,
* embeddings,
* classification.

AI Routing selects capabilities through provider-neutral metadata.

Business modules MUST NOT depend on the plugin's concrete provider name.

---

# Dictionary / Reference Extensions

Dictionary/reference plugins MAY provide external reference data.

They MUST NOT automatically become canonical Glossary truth.

Imported or proposed terminology still passes through Glossary-domain rules.

---

# Export Extensions

Export plugins MAY provide output targets such as:

* file formats,
* clipboard formats,
* external applications,
* document exporters.

Export implementations MUST NOT modify canonical source/Translation state merely for formatting.

---

# Storage Extensions

Optional Storage adapters MAY exist when CRAI intentionally exposes a storage extension point.

However:

```text
Plugin Storage Adapter
    !=
Storage Module ownership
```

The Storage module/infrastructure retains:

* persistence semantics,
* migration rules,
* transaction boundaries,
* canonical storage contracts.

---

# Plugin Registry

The Plugin Registry records plugin installation/runtime metadata.

It MAY track:

```text
installed plugins
enabled state
disabled state
plugin versions
capability declarations
dependency metadata
permission grants
compatibility status
lifecycle status
```

---

# Registry Is Not Plugin Storage

Registry metadata describes plugin availability.

It MUST NOT become the canonical storage for arbitrary plugin business data.

Plugin-owned persistent data must use approved Storage contracts.

---

# Plugin Loader

The Plugin Loader performs implementation loading.

Its responsibilities MAY include:

* resolving entry point,
* loading package/library/process,
* establishing isolation boundary,
* constructing adapter/proxy,
* returning a load result.

The Loader hides platform-specific mechanisms.

---

# Loader Boundary

The Loader MUST NOT:

* register business capabilities directly,
* resolve Workspace business policy,
* choose preferred plugins,
* mutate canonical plugin configuration.

Those decisions belong to other components.

---

# Plugin Discovery

Discovery identifies installed or available plugin descriptors.

Possible discovery sources:

* known plugin directories,
* package manifests,
* installed application bundles,
* registered built-in extensions,
* future plugin repositories.

Discovery MUST NOT automatically execute discovered code.

---

# Discovery Boundary

Conceptually:

```text
Discover Descriptor
        |
        v
Validate Descriptor
        |
        v
Compatibility Check
        |
        v
Permission / Trust Evaluation
        |
        v
Load
```

Discovery itself MUST remain side-effect-minimal.

---

# Capability Registration

After successful initialization, a plugin MAY register one or more capabilities.

Conceptually:

```text
Plugin
   |
   v
Capability Declaration
   |
   v
Capability Registry / Index
```

Only validated capabilities become eligible for use.

---

# Capability Index

Capability Index maps:

```text
Capability Requirement
        |
        v
Eligible Capability Providers
```

Example:

```text
RecognitionEngine
├── BuiltInRecognizer
├── PaddleOCR Plugin
└── Tesseract Plugin
```

The index SHOULD use stable capability contracts rather than provider display names.

---

# Capability Provider

Recommended representation:

```text
CapabilityProvider
├── capabilityId
├── capabilityVersion
├── providerKind
├── providerReference
├── pluginId?
├── implementationVersion
├── capabilityMetadata
├── availability
└── priorityMetadata?
```

A provider may be:

```text
BUILT_IN
PLUGIN
EXTERNAL_ADAPTER
```

---

# Capability Selection

The Plugin System exposes eligible implementations.

The owning module/runtime decides which provider to use according to its architecture.

Examples:

```text
AI model/provider
    -> AI Routing
```

```text
Recognition engine
    -> Recognition selection policy
```

Plugin Manager MUST NOT become the universal selection engine.

---

# Plugin Lifecycle

Recommended logical lifecycle:

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
LOADED
    |
    v
INITIALIZED
    |
    v
ACTIVE
    |
    +--> DISABLED
    |
    +--> FAILED
    |
    v
STOPPING
    |
    v
UNLOADED
```

Exact lifecycle semantics are defined by `PLUGIN_LIFECYCLE.md`.

---

# Lifecycle Ownership

Plugin Manager coordinates lifecycle transitions.

Individual plugins MUST NOT directly mark themselves ACTIVE in canonical Plugin Registry state.

They may report readiness/failure signals.

---

# Lifecycle vs Health

Plugin lifecycle and plugin health are separate concepts.

Example:

```text
Lifecycle:
    ACTIVE

Health:
    DEGRADED
```

A loaded plugin may remain active while one external dependency is degraded.

---

# Health Boundary

Plugin Manager MAY expose health probes or status references.

Health collection/projection MAY be performed through Diagnostics/Telemetry infrastructure.

Plugin Manager MUST NOT become the sole owner of all operational health semantics.

---

# Plugin Dependencies

Plugins MAY declare:

```text
required plugin dependencies
optional plugin dependencies
required capability dependencies
```

Capability dependency SHOULD be preferred when an implementation does not require a specific plugin identity.

---

# Plugin-to-Plugin Dependency

Avoid:

```text
plugin A requires plugin B
```

when the real requirement is:

```text
plugin A requires capability X
```

Capability dependencies reduce implementation coupling.

---

# Direct Plugin Communication

Plugins MUST NOT depend directly on another plugin's private implementation APIs.

Allowed communication mechanisms include:

```text
public capability contract
host service contract
Event Bus
explicit capability dependency
```

---

# Public Contracts

All plugin-facing contracts MUST be versioned and documented.

Plugins MUST NOT import or depend on private/internal CRAI implementation objects unless explicitly declared part of the Plugin API.

---

# Host Services

CRAI MAY expose controlled Host Services to plugins.

Examples:

```text
Logging Service
Telemetry Service
Configuration View
Storage API
Event Publisher
HTTP/Network Client
Secret Reference Resolver
Temporary File Service
Clock
Scheduler API
```

Not every plugin receives every Host Service.

---

# Least Privilege

A plugin receives only the services and permissions required by its declared capabilities.

---

# Permission Model

Plugins SHOULD declare required permissions.

Possible permissions include:

```text
NETWORK
FILE_READ
FILE_WRITE
SCREEN_CAPTURE
CLIPBOARD_READ
CLIPBOARD_WRITE
STORAGE_READ
STORAGE_WRITE
EVENT_PUBLISH
EVENT_SUBSCRIBE
SECRET_REFERENCE_USE
LOCAL_MODEL_EXECUTION
```

Exact taxonomy is defined by `PLUGIN_SECURITY.md`.

---

# Permission Grant

Requested permission does NOT automatically imply granted permission.

Conceptually:

```text
Plugin requests
        |
        v
Policy / User / Admin Evaluation
        |
        v
Permission Grant
```

---

# Permission Broker

Sensitive Host Services SHOULD enforce permission grants.

Conceptually:

```text
Plugin
   |
   v
Host Service Proxy
   |
   v
Permission Check
   |
   v
Sensitive Resource
```

This is preferable to exposing raw operating-system resources directly.

---

# Network Access

A plugin requiring network access SHOULD receive an approved network capability/client rather than unrestricted network authority where feasible.

---

# File System Access

Plugins SHOULD NOT receive unrestricted filesystem access by default.

Preferred:

* scoped directories,
* virtual file handles,
* temporary file services,
* explicit user-selected paths.

---

# Clipboard Access

Clipboard access is sensitive and SHOULD be explicit.

Read and write permissions SHOULD remain separate where practical.

---

# Storage Access

Plugins SHOULD use public Storage contracts.

Direct access to CRAI's internal database files/tables is forbidden.

---

# Runtime Configuration

Plugins MAY receive a scoped configuration view.

They MUST NOT mutate arbitrary runtime configuration through internal objects.

---

# Secret Access

Raw provider/application secrets SHOULD NOT be exposed directly.

Prefer:

```text
SecretReference
    |
    v
Approved Host/Provider Adapter
```

Exact semantics belong to Secret Management and Plugin Security architecture.

---

# Trust Model

CRAI SHOULD distinguish plugin trust levels.

Possible conceptual levels:

```text
BUILT_IN_TRUSTED
SIGNED_TRUSTED
USER_INSTALLED
UNTRUSTED
DEVELOPMENT
```

Trust level MAY affect:

* allowed permissions,
* isolation mode,
* diagnostics,
* auto-loading,
* update behavior.

---

# Trust Is Not Capability

A highly trusted plugin still MUST declare capabilities and permissions.

Trust MUST NOT implicitly grant unrestricted authority.

---

# Execution Isolation

Plugins MAY execute:

```text
IN_PROCESS
OUT_OF_PROCESS
SANDBOXED
REMOTE
```

depending on:

* platform,
* trust,
* capability,
* performance,
* security requirements.

---

# In-Process Plugins

In-process execution may provide low overhead.

It also has weaker fault/security isolation.

Only sufficiently trusted plugins SHOULD be eligible where the platform cannot enforce meaningful isolation.

---

# Out-of-Process Plugins

Out-of-process execution provides stronger:

* crash isolation,
* permission isolation,
* lifecycle control.

It incurs IPC/runtime overhead.

---

# Remote Plugins

Future remote plugins MAY expose capability contracts over approved RPC/network boundaries.

They MUST still appear to CRAI through provider-neutral Plugin API contracts.

---

# Failure Isolation

Plugin failure SHOULD NOT corrupt unrelated CRAI state.

Possible failures include:

```text
load failure
initialization failure
capability failure
process crash
dependency failure
permission denial
external provider failure
```

---

# Failure Handling

On plugin failure CRAI SHOULD:

* normalize failure,
* mark relevant capability unavailable/degraded,
* release resources,
* remove invalid capability bindings,
* emit diagnostics,
* allow alternate implementation selection where available.

---

# Failure Is Not Automatically Application Failure

A non-essential plugin failure SHOULD normally allow CRAI to continue.

If the plugin provides a capability required by the active operation and no alternative exists, that operation MAY fail gracefully.

---

# Plugin Crash Boundary

An out-of-process plugin crash MUST NOT crash the host process.

An in-process plugin cannot always provide the same guarantee; architecture and trust policy must acknowledge this limitation.

---

# Configuration

Plugin configuration SHOULD be scoped by:

```text
pluginId
pluginVersion / compatible range
Workspace?
Project?
Principal?
```

depending on the configuration type.

Canonical rules are defined by `PLUGIN_CONFIGURATION.md`.

---

# Configuration Ownership

Plugin-specific configuration belongs to Plugin Configuration architecture.

Workspace/Project business configuration MUST NOT be duplicated into opaque plugin config when canonical CRAI configuration already exists.

---

# Plugin Configuration vs Provider Configuration

A provider plugin MAY reference:

```text
ProviderConfiguration
```

but it SHOULD NOT duplicate credentials/provider ownership into arbitrary plugin-private state.

---

# Version Compatibility

Compatibility SHOULD consider:

```text
Plugin API version
Capability contract version
Plugin version
CRAI compatibility range
Dependency versions
Platform support
```

Compatibility is checked before activation.

---

# Backward Compatibility

Public Plugin APIs SHOULD preserve backward compatibility where practical.

Breaking changes MUST require explicit version evolution.

---

# Version Negotiation

Where supported:

```text
Plugin
    declares supported contract range

Host
    selects compatible contract version
```

Silent incompatible loading is forbidden.

---

# Plugin Upgrade

Plugin upgrade MUST preserve explicit lifecycle semantics.

Conceptually:

```text
old version
    |
    v
compatibility check
    |
    v
configuration/data migration?
    |
    v
new version activation
```

Upgrade details belong to Versioning/Configuration documents.

---

# Runtime Discovery

Runtime discovery MAY be supported.

However:

```text
discovered
    !=
trusted
    !=
enabled
    !=
loaded
    !=
active
```

These states MUST remain separate.

---

# Hot Loading

Hot loading/unloading MAY be supported only when:

* platform permits it,
* plugin contract supports it,
* no unsafe active dependency exists,
* resources can be released safely.

MVP MAY defer arbitrary hot unloading.

---

# Unloading

Before unload:

* stop accepting new work,
* drain/cancel active operations according to policy,
* unregister capabilities,
* release Host Service leases,
* shutdown plugin,
* remove runtime resources.

---

# Capability Availability

A plugin can be ACTIVE while one capability is unavailable.

Capability availability SHOULD therefore be independently observable where needed.

---

# Event Bus

Plugins MAY publish/subscribe to allowed events through public Event Bus contracts.

They MUST NOT:

* publish arbitrary internal events,
* subscribe to private event streams,
* impersonate another module.

Event permissions MAY be capability-scoped.

---

# Event Ownership

Plugin-emitted events SHOULD identify:

```text
pluginId
capability
event contract version
correlation context
```

where appropriate.

A plugin event does NOT automatically become a Domain Event.

---

# Plugin API

Plugin API defines:

* lifecycle entry points,
* Host Services,
* capability registration,
* configuration access,
* error contracts,
* event interfaces,
* permission-aware APIs.

Exact contracts belong to `PLUGIN_API.md`.

---

# Registry Persistence

Plugin Registry metadata MAY be persisted.

Loss of Registry cache/projection MUST NOT destroy plugin package files or canonical business data.

---

# Plugin Data

Plugins needing durable private data MUST use approved storage namespaces/contracts.

They MUST NOT create hidden unmanaged persistence outside defined policy where avoidable.

---

# Data Ownership

Plugin-private operational data MUST remain distinguishable from CRAI canonical domain resources.

---

# Security

Plugin security MUST consider:

* plugin authenticity,
* integrity,
* provenance,
* permission grants,
* isolation,
* secrets,
* network/file access,
* update integrity,
* dependency risk,
* cross-Workspace isolation.

Detailed rules belong to `PLUGIN_SECURITY.md`.

---

# Authenticity

Plugins MAY be:

* built-in,
* signed,
* user-installed,
* development-only.

Signature/trust requirements SHOULD depend on distribution model.

---

# Integrity

Plugin package integrity SHOULD be validated before load where possible.

Possible mechanisms:

* package hash,
* digital signature,
* trusted repository metadata.

---

# Workspace Isolation

A plugin MUST NOT access another Workspace's data merely because the same plugin process serves several Workspaces.

Host Services MUST enforce scope.

---

# Principal Isolation

Where principal/user permissions differ, plugin calls MUST preserve principal authorization context as required.

---

# Security Failure

Permission denial SHOULD produce a structured plugin/security error.

It MUST NOT be bypassed by direct access to internal resources.

---

# Observability

Plugin runtime SHOULD emit structured telemetry such as:

```text
plugin discovery
validation result
load duration
initialization duration
shutdown duration
capability registration
capability failure
dependency failure
permission denial
plugin crash
resource usage
```

---

# Sensitive Observability

Plugin telemetry MUST NOT expose:

* secrets,
* arbitrary user content,
* private configuration values,
* raw provider credentials.

---

# Plugin Diagnostics

Diagnostics MAY expose:

```text
pluginId
pluginVersion
lifecycle state
capabilities
dependency state
permission state
health summary
normalized failures
```

---

# Audit

Material plugin actions MAY require Audit.

Examples:

* plugin installed,
* plugin removed,
* plugin enabled/disabled,
* sensitive permission granted,
* plugin trust level changed,
* untrusted plugin approved,
* plugin updated.

Ordinary capability calls SHOULD remain telemetry unless policy requires otherwise.

---

# Runtime Flow

Typical startup flow:

```text
Application Startup
        |
        v
Discover Plugin Descriptors
        |
        v
Validate Descriptor
        |
        v
Verify Compatibility
        |
        v
Evaluate Trust / Permissions
        |
        v
Resolve Dependencies
        |
        v
Load Plugin
        |
        v
Initialize Plugin
        |
        v
Register Capabilities
        |
        v
Activate Plugin
```

Not every plugin must be loaded during initial application startup.

Lazy activation MAY be supported.

---

# Lazy Loading

A compatible enabled plugin MAY be loaded only when one of its capabilities is needed.

This may improve:

* startup time,
* memory usage,
* isolation,
* resource consumption.

---

# Lazy Loading Boundary

Capability discovery MUST still be possible without executing plugin code wherever feasible.

---

# Shutdown Flow

```text
Shutdown Requested
        |
        v
Stop New Capability Requests
        |
        v
Drain / Cancel Active Operations
        |
        v
Unregister Capabilities
        |
        v
Shutdown Plugin
        |
        v
Unload Runtime
```

---

# Plugin Replacement

A plugin implementation MAY be replaced by another implementation if both satisfy the same required capability contract.

Example:

```text
PaddleOCR
    ->
Tesseract
```

The owning Recognition capability remains unchanged.

---

# Built-In vs Plugin Replacement

Built-in and plugin implementations SHOULD be able to participate in the same capability-selection model where useful.

Example:

```text
RecognitionEngine
├── Built-in Engine
├── Plugin A
└── Plugin B
```

---

# Plugin System vs Module System

Critical distinction:

```text
Module
    = CRAI application capability / ownership boundary
```

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

A plugin may implement contracts exposed by one or more modules.

---

# Plugin System vs Provider Management

Provider Management owns:

* provider configuration,
* provider availability,
* credential references,
* provider policy.

A provider plugin may expose implementation support.

It MUST NOT become the owner of provider configuration semantics.

---

# Plugin System vs Infrastructure

Infrastructure provides:

* loader mechanisms,
* process isolation,
* IPC,
* filesystem abstractions,
* network clients,
* signature verification,
* secrets,
* logging/telemetry.

Plugin architecture defines semantic extension contracts.

---

# Plugin System vs Event Bus

Event Bus transports approved events.

Plugin System determines which event contracts/plugins may use.

Plugin Manager is not the Event Bus.

---

# Plugin System vs Security

Plugin Security defines trust, permissions and isolation policies.

Plugin Manager enforces/respects those decisions during lifecycle.

---

# Plugin System vs Configuration

Plugin System identifies plugin configuration ownership.

Configuration infrastructure provides persistence/resolution mechanisms.

---

# Plugin System vs Runtime

Runtime owns:

* scheduling,
* workers,
* queues,
* cancellation,
* process/thread execution.

Plugin lifecycle may use Runtime facilities.

Plugin Manager does not replace Runtime.

---

# Architecture Invariants

1. CRAI modules/domains remain owners of business semantics.

2. Plugins provide extension implementations, not business-domain ownership.

3. CRAI Core MUST NOT depend on concrete plugin implementations.

4. Plugins depend only on approved public contracts and Host Services.

5. Not every internal implementation must be a plugin.

6. Built-in and plugin implementations MAY coexist.

7. Plugin is not Module.

8. Plugin is not Domain.

9. Plugin identity is stable and independent from display name/path.

10. Plugins declare capabilities explicitly.

11. Capability contracts SHOULD be preferred over implementation-name dependencies.

12. Plugin Manager coordinates plugin lifecycle.

13. Plugin Manager MUST NOT become a universal service locator.

14. Plugin Manager MUST NOT become a message broker.

15. Plugin Manager MUST NOT become a universal capability selector.

16. Owning modules/runtime select implementations according to their architecture.

17. Discovery MUST NOT automatically execute plugin code.

18. Discovered does not mean trusted.

19. Trusted does not mean enabled.

20. Enabled does not mean loaded.

21. Loaded does not mean active.

22. Plugin lifecycle and health are separate concepts.

23. Plugins MUST NOT directly depend on another plugin's private APIs.

24. Plugin-to-plugin interaction uses public capability contracts or approved events.

25. Required capability dependencies SHOULD be preferred over concrete plugin dependencies where possible.

26. Plugin Registry stores plugin metadata, not arbitrary canonical business data.

27. Plugin Loader hides platform-specific loading mechanisms.

28. Loader MUST NOT own business selection or configuration semantics.

29. Sensitive resources MUST be exposed through controlled Host Services/permission boundaries.

30. Plugins MUST operate with least privilege.

31. Requested permissions MUST NOT automatically be granted.

32. Raw secrets SHOULD NOT be exposed to plugins by default.

33. Direct access to CRAI internal database/schema is forbidden.

34. File/network/clipboard access SHOULD be scoped.

35. Plugin trust and plugin capability are distinct concepts.

36. Higher trust MUST NOT imply unrestricted permissions.

37. Execution isolation SHOULD reflect trust and risk.

38. Plugin failure SHOULD remain isolated where technically possible.

39. Out-of-process plugin crashes MUST NOT crash the host process.

40. In-process plugins cannot guarantee equivalent crash isolation and require stronger trust.

41. Plugin failure MUST NOT corrupt unrelated plugin state.

42. A plugin failure MAY make only its provided capabilities unavailable.

43. Public Plugin APIs are versioned.

44. Capability contracts are versioned.

45. Compatibility MUST be verified before activation.

46. Breaking contract changes require explicit version evolution.

47. Plugins MUST NOT silently bypass version incompatibility.

48. Plugin configuration MUST remain scoped.

49. Plugin-private configuration MUST NOT duplicate canonical CRAI configuration unnecessarily.

50. Plugin-private data MUST use approved persistence mechanisms.

51. Plugin event access MUST be permission/contract controlled.

52. Plugin-emitted runtime events are not automatically Domain Events.

53. Workspace isolation MUST be enforced for plugin calls/data.

54. Principal authorization MUST be preserved where required.

55. Plugin telemetry MUST avoid sensitive values by default.

56. Material plugin-management changes MAY require Audit.

57. Hot loading/unloading is optional, not an architectural requirement for MVP.

58. Capability discovery SHOULD be possible without arbitrary plugin-code execution where feasible.

59. New plugin types SHOULD integrate through extension contracts rather than core business rewrites.

60. Removing the Plugin System MUST NOT erase canonical domain truth.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* stable `pluginId`,
* Plugin Descriptor,
* Plugin Registry,
* Plugin Discovery,
* descriptor validation,
* Plugin API version,
* plugin version,
* capability declarations,
* capability registration,
* dependency validation,
* enable/disable,
* load,
* initialize,
* activate,
* shutdown,
* unload where safe,
* built-in + plugin capability providers,
* Recognition/OCR plugin extension,
* Translation/AI provider extension,
* basic Export extension,
* scoped plugin configuration,
* permission declarations,
* Network permission,
* File permission,
* Clipboard permission,
* Storage Host Service,
* Logging/Telemetry Host Service,
* structured plugin errors,
* basic failure isolation,
* plugin diagnostics,
* version compatibility.

MVP SHOULD prefer:

```text
trusted / controlled plugins
```

over a fully open public plugin marketplace.

MVP MAY defer:

* arbitrary third-party untrusted plugins,
* full OS sandbox,
* remote plugins,
* automatic plugin marketplace installation,
* cryptographic signing enforcement,
* hot upgrade,
* arbitrary hot unload,
* complex dependency solving,
* cross-plugin service graphs,
* plugin process pooling,
* user-contributed extension SDK distribution,
* advanced trust scoring.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact Plugin Descriptor schema,
* exact Plugin API contract,
* plugin ID naming scheme,
* capability ID/version scheme,
* Plugin Registry persistence,
* whether built-in implementations appear as pseudo-plugins or generic CapabilityProviders,
* default execution isolation,
* whether MVP plugins run in-process,
* out-of-process IPC protocol,
* plugin package format,
* discovery directories,
* lazy loading,
* hot loading,
* hot unloading,
* trust levels,
* signature requirements,
* publisher identity,
* permission taxonomy,
* permission grant UI,
* Workspace-specific permission grants,
* principal-specific permissions,
* network-host allowlists,
* filesystem scope model,
* plugin data namespace,
* configuration migration,
* plugin update mechanism,
* capability dependency resolver,
* dependency cycle handling,
* health-projection ownership,
* provider-plugin relationship with Provider Management,
* Storage extension policy,
* Event Bus permission model,
* audit retention,
* plugin repository/marketplace architecture.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_API.md`
* `PLUGIN_REGISTRY.md`
* `PLUGIN_DISCOVERY.md`
* `PLUGIN_LIFECYCLE.md`
* `PLUGIN_DEPENDENCY.md`
* `PLUGIN_CONFIGURATION.md`
* `PLUGIN_SECURITY.md`
* `PLUGIN_VERSIONING.md`

Architecture:

* `../modules/MODULE_MAP.md`
* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`
* `../core/CAPABILITY_MAP.md`
* `../core/EVENT_BUS.md`

AI:

* `../ai/MODELS.md`
* `../ai/ROUTING.md`
* `../ai/SAFETY.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/recognition/`
* `../../02-modules/translation/`
* `../../02-modules/capture/`
* `../../02-modules/storage/`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/logging/`
* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/telemetry/`

Runtime:

* `../runtime/RUNTIME_COMPONENTS.md`
* `../runtime/RESOURCE_LIFECYCLE.md`
* `../runtime/CANCELLATION.md`
