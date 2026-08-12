# Plugin API

* **Document:** Plugin Architecture / Plugin API
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the public contracts used between the CRAI Host and plugin implementations.

The Plugin API provides stable, versioned and implementation-independent boundaries for:

* plugin identity,
* lifecycle coordination,
* capability declaration,
* capability invocation,
* Host Services,
* configuration access,
* permissions,
* events,
* cancellation,
* errors,
* observability,
* compatibility.

The Plugin API enables CRAI to use plugin implementations without exposing internal application structures.

---

# Core Principle

```text
CRAI Module / Runtime
        |
        v
Public Capability Contract
        |
        v
Plugin API Boundary
        |
        v
Plugin Implementation
```

The Plugin API defines:

```text
how CRAI and plugins interact
```

It does NOT define:

```text
which module owns the business semantics
```

---

# API Layers

The Plugin API SHOULD be separated into several logical layers:

```text
Plugin API
├── Base Plugin Contract
├── Lifecycle Contract
├── Capability Contracts
├── Host Service Contracts
├── Configuration Contract
├── Permission Contract
├── Event Contract
├── Error Contract
└── Observability Contract
```

Not every plugin requires every optional contract.

---

# Base Plugin Contract

Every plugin MUST expose a minimal base contract.

Recommended:

```text
Plugin
├── descriptor()
├── capabilities()
└── lifecycle hooks as supported
```

The base contract SHOULD remain small.

Business-specific methods MUST NOT be added to the generic Plugin interface.

---

# Why the Base Contract Is Minimal

Avoid:

```text
Plugin.translate()
Plugin.recognize()
Plugin.capture()
Plugin.export()
```

because different plugin types provide different capabilities.

Instead:

```text
Plugin
    declares capability implementations
```

and capability-specific contracts define invocation semantics.

---

# Plugin Descriptor

Every plugin MUST expose or be associated with a validated:

```text
PluginDescriptor
```

Recommended metadata includes:

```text
pluginId
pluginVersion
pluginApiVersion
displayName?
publisher?
description?
license?
supportedPlatforms[]
capabilities[]
requiredPermissions[]
requiredDependencies[]
optionalDependencies[]
configurationSchemaReference?
executionModel?
integrityMetadata?
```

The canonical descriptor format is defined by Plugin Registry/Discovery architecture.

---

# Descriptor vs Runtime State

Plugin Descriptor contains declarative metadata.

It MUST NOT contain mutable runtime state such as:

```text
current health
active request count
last failure
worker state
```

Those belong to runtime/diagnostics.

---

# Plugin Identity

`pluginId` is the stable canonical identity.

It MUST NOT be inferred from:

* display name,
* filename,
* package path,
* provider display name.

---

# Plugin API Version

Each plugin MUST declare which Plugin API contract range it supports.

Example conceptually:

```text
pluginApi:
    min: 2.0
    maxExclusive: 3.0
```

Exact version syntax is defined by `PLUGIN_VERSIONING.md`.

---

# Capability Contracts

A plugin advertises one or more public capabilities.

Examples MAY include:

```text
CaptureSource
RecognitionEngine
TranslationExecution
AIExecutionProvider
DictionarySource
ExportTarget
StorageAdapter
```

These are extension contracts.

They are NOT domain ownership categories.

---

# Capability Declaration

Recommended:

```text
PluginCapabilityDeclaration
├── capabilityId
├── capabilityVersion
├── implementationVersion?
├── metadata
├── requirements?
└── compatibility?
```

---

# Capability ID

Capability ID represents a public extension contract.

Example:

```text
recognition.engine
```

rather than:

```text
paddleocr
```

Implementation identity remains separate.

---

# Capability Invocation

Capability invocation SHOULD use typed provider-neutral contracts.

Conceptually:

```text
Capability Request
        |
        v
Plugin Capability Interface
        |
        v
Plugin Implementation
        |
        v
Capability Response
```

---

# Capability Request

A capability request SHOULD carry only information required by that capability.

It MAY include:

```text
requestId
workspaceScope
operationContext
input
configurationReference
deadline
cancellationContext
correlationContext
```

The exact fields are defined by each capability contract.

---

# Capability Response

Capability responses SHOULD be provider-neutral and normalized.

Implementation-specific SDK objects MUST NOT cross the Plugin API boundary.

---

# Capability Contract Ownership

Capability contracts SHOULD be defined by the owning CRAI architecture/module.

Examples:

```text
Recognition
    defines Recognition engine contract
```

```text
AI architecture
    defines AI provider/model execution contracts
```

```text
Capture
    defines Capture source contract
```

Plugin architecture provides the extension mechanism.

---

# Plugin API Must Not Duplicate Module Contracts

Avoid defining parallel plugin-only semantics such as:

```text
PluginTranslationResult
```

when the owning Translation/AI architecture already defines a suitable public execution contract.

Reuse or adapt canonical public contracts where appropriate.

---

# Built-In Compatibility

Where practical, built-in implementations SHOULD be able to implement the same capability interface as plugins.

Conceptually:

```text
RecognitionEngine
├── BuiltInRecognizer
├── PaddleOCRPlugin
└── TesseractPlugin
```

This avoids separate business paths.

---

# Lifecycle Contract

Lifecycle API coordinates plugin runtime state.

Recommended lifecycle hooks MAY include:

```text
prepare()
start()
stop()
dispose()
```

or equivalent semantics.

Not every execution model requires all hooks.

---

# Lifecycle Optionality

A stateless plugin MAY require only:

```text
prepare
dispose
```

while a plugin with workers/subscriptions MAY require:

```text
prepare
start
stop
dispose
```

Therefore the Plugin API SHOULD NOT assume every plugin runs background work.

---

# Prepare / Initialize

Preparation SHOULD:

* validate runtime dependencies,
* establish bounded resources,
* validate scoped configuration,
* prepare capability implementation.

It MUST NOT begin accepting normal capability requests before activation.

---

# Start / Activate

Start MAY:

* register subscriptions,
* start required workers,
* open long-lived resources,
* mark capability provider ready.

Only plugins whose execution model requires activation need this hook.

---

# Stop

Stop SHOULD:

* stop accepting new work,
* coordinate draining/cancellation,
* stop background activity,
* prepare for unload.

Stop SHOULD NOT silently discard in-flight work without explicit runtime policy.

---

# Dispose

Dispose releases remaining runtime resources.

After successful dispose:

```text
plugin instance must not accept further capability calls
```

---

# Lifecycle Idempotency

Lifecycle hooks SHOULD be safe against duplicate runtime invocation where practical.

Example:

```text
stop()
stop()
```

should not corrupt plugin state.

Exact idempotency requirements are defined in `PLUGIN_LIFECYCLE.md`.

---

# Lifecycle Determinism

The previous invariant:

```text
Lifecycle methods are deterministic
```

is too broad.

Network/provider initialization may be non-deterministic.

The correct requirement is:

```text
Lifecycle transition semantics are deterministic
```

for the same current lifecycle state and normalized outcome.

---

# Health Contract

Health SHOULD NOT be mandatory on every plugin's generic API.

A plugin MAY expose:

```text
HealthProbe
```

or capability/runtime health signals when meaningful.

---

# Health Probe

Recommended:

```text
PluginHealthProbeResult
├── state
├── observedAt
├── components[]
├── reasonCodes[]
└── expiresAt?
```

Possible normalized states:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
UNKNOWN
```

Health vocabulary SHOULD align with wider runtime/model architecture where practical.

---

# Health Boundary

Health probe output is an observation.

The authoritative health projection may be produced by runtime/telemetry infrastructure.

Plugin Manager MUST NOT infer all health semantics itself.

---

# Configuration Contract

Plugins receive configuration through a scoped Host-provided configuration interface.

Recommended:

```text
PluginConfigurationView
├── pluginId
├── scope
├── configurationRevision
├── values
└── schemaVersion
```

---

# Configuration Is Scoped

A plugin MUST NOT receive the entire CRAI application configuration.

It SHOULD receive only:

```text
configuration relevant to its declared capability
```

---

# Direct Configuration Access

Plugins MUST NOT:

* read internal application configuration files directly,
* access arbitrary environment variables,
* mutate internal runtime configuration objects,
* query unrelated Workspace settings.

---

# Configuration Values

Plugin configuration MAY contain ordinary plugin-specific values such as:

* endpoint preference,
* plugin feature toggles,
* plugin-local limits,
* implementation behavior,
* non-sensitive adapter options.

---

# Secrets Boundary

Raw secrets SHOULD NOT be included in normal Plugin Configuration.

Avoid:

```text
apiKey: plaintext
```

Prefer:

```text
credentialReference
```

or a permission-aware Host Service that performs the privileged operation.

---

# Provider Configuration Boundary

Provider plugins SHOULD consume approved Provider Configuration references/contracts.

They MUST NOT create a second independent credential/provider-management model.

---

# Retry Configuration Boundary

Generic Retry Policy MUST NOT be copied into arbitrary plugin configuration.

Runtime owns Retry orchestration.

Plugins MAY receive execution-scoped data such as:

```text
attempt deadline
provider retry hint handling contract
```

when required.

---

# Model Selection Boundary

Plugins MUST NOT normally receive:

```text
choose model automatically
```

as opaque private configuration when model selection belongs to AI Routing.

A selected model/deployment reference MAY be passed to the capability invocation after Routing.

---

# Cache Boundary

Plugins MUST NOT independently redefine CRAI Result Cache semantics.

Provider-native caches MAY be used internally behind explicit adapter contracts.

---

# Host Services

Plugins MAY request approved Host Services.

Recommended initial Host Services:

```text
Logging
Telemetry
Configuration
Storage
Event Publishing
Event Subscription
Network Client
Secret / Credential Broker
Clock
Scheduler
Temporary Files
Cancellation
```

Not every plugin receives every service.

---

# Host Service Access

Host Services SHOULD be provided through explicit interfaces.

Avoid exposing:

```text
ApplicationContainer
InternalServiceRegistry
GlobalRuntime
DatabaseConnection
```

---

# Service Discovery

Plugins SHOULD receive required Host Services during construction/initialization.

They SHOULD NOT perform unrestricted runtime lookup from a global service locator.

---

# Host Service Capability

Recommended:

```text
HostServiceHandle
├── serviceId
├── contractVersion
├── grantedScope
└── permissionContext
```

Implementation details MAY differ.

---

# Storage Service

Plugins needing persistence SHOULD use an approved Storage Host Service.

They MUST NOT:

* open CRAI internal database tables directly,
* assume underlying database technology,
* modify another plugin's namespace.

---

# Plugin Storage Namespace

Plugin-private data SHOULD be scoped using:

```text
pluginId
+
workspace/project scope where applicable
```

Canonical business resources remain outside plugin-private storage.

---

# Network Service

Network access SHOULD use a controlled Host Network Service where feasible.

It MAY enforce:

* allowed hosts,
* protocols,
* timeouts,
* proxy configuration,
* telemetry,
* policy.

---

# Clipboard Service

Clipboard operations SHOULD use a permission-aware Host Service.

Read and write access SHOULD be distinct when practical.

---

# File Service

File access SHOULD use scoped handles or approved paths.

Plugins MUST NOT assume unrestricted filesystem access.

---

# Secret / Credential Service

Plugins SHOULD use a credential broker or opaque reference.

Preferred:

```text
Plugin
    requests approved operation
        |
        v
Credential-aware Host / Provider Adapter
```

rather than:

```text
Plugin receives raw secret
```

Raw secret exposure MAY exist only where unavoidable and explicitly permitted.

---

# Scheduler Service

Plugins MAY request scheduling services for plugin-owned background work.

Plugins MUST NOT create unmanaged global schedulers when Host scheduling exists.

---

# Cancellation Service

Long-running capability calls MUST support Host-propagated cancellation where applicable.

---

# Permission Contract

A plugin declares required permissions.

The Host determines granted permissions.

Conceptually:

```text
Declared Permission
        |
        v
Policy / User / Admin Evaluation
        |
        v
Permission Grant
        |
        v
Host Service Enforcement
```

---

# Permission Declaration

Possible permissions MAY include:

```text
NETWORK
FILE_READ
FILE_WRITE
CLIPBOARD_READ
CLIPBOARD_WRITE
STORAGE_READ
STORAGE_WRITE
EVENT_PUBLISH
EVENT_SUBSCRIBE
SECRET_REFERENCE_USE
SCREEN_CAPTURE
LOCAL_MODEL_EXECUTION
```

Exact taxonomy belongs to `PLUGIN_SECURITY.md`.

---

# Permission Grant

Recommended:

```text
PluginPermissionGrant
├── pluginId
├── permissionId
├── scope
├── constraints?
├── grantedBy?
├── policyReference?
├── expiresAt?
└── revision
```

---

# Plugin Manager Boundary

Plugin Manager coordinates permission evaluation during activation.

It is NOT necessarily the component that directly performs every access check.

Sensitive Host Services SHOULD enforce permission grants themselves.

---

# Event Contract

Plugins MAY interact asynchronously through public Event Bus contracts.

Event Bus is appropriate for:

* notifications,
* lifecycle-independent signals,
* asynchronous integration,
* eventual processing.

---

# Event Bus Is Not the Only Communication Mechanism

Synchronous capability invocation SHOULD use capability/service contracts.

Therefore:

```text
Capability call
    -> direct typed contract
```

while:

```text
Asynchronous notification
    -> Event Bus
```

---

# Event Publication

Plugins MAY publish only approved public events.

Recommended event metadata:

```text
eventType
eventVersion
pluginId
capabilityId?
correlationId?
workspaceScope?
timestamp
payload
```

---

# Event Subscription

Plugins MAY subscribe only to event contracts permitted by:

* declared capability,
* permission grant,
* Workspace/security policy.

---

# Event Privacy

Event payloads MUST preserve:

* Workspace isolation,
* sensitivity constraints,
* public contract boundaries.

---

# Plugin Events vs Domain Events

A plugin runtime event is NOT automatically a Domain Event.

Examples:

```text
PluginProviderRateLimited
```

may be runtime telemetry/event.

Only an owning domain/module may define canonical Domain Events.

---

# Direct Plugin-to-Plugin Communication

Plugins MUST NOT call another plugin's private/internal APIs.

Allowed:

```text
Plugin A
    ->
public Capability X
    ->
Capability Provider B
```

The caller SHOULD depend on Capability X, not Plugin B identity.

---

# Plugin Dependency Invocation

When Plugin A declares a capability dependency:

```text
requires recognition.language-hint
```

the Host resolves a compatible provider.

The dependency need not be another plugin; it may be built-in.

---

# Error Contract

All errors crossing the Plugin API boundary MUST use normalized public contracts.

Implementation-specific:

* exceptions,
* stack traces,
* SDK errors,
* provider errors,

MUST NOT escape as required caller semantics.

---

# Plugin Error

Recommended:

```text
PluginError
├── code
├── category
├── messageReference?
├── retryability?
├── capabilityId?
├── pluginId
├── operationId?
├── diagnosticReference?
└── metadata?
```

---

# Error Categories

Possible generic categories:

```text
CONFIGURATION_INVALID
INITIALIZATION_FAILED
CAPABILITY_UNAVAILABLE
DEPENDENCY_UNAVAILABLE
PERMISSION_DENIED
TIMEOUT
CANCELLED
RESOURCE_EXHAUSTED
EXTERNAL_SERVICE_FAILURE
PROTOCOL_ERROR
INTERNAL_PLUGIN_ERROR
VERSION_INCOMPATIBLE
```

Capability-specific contracts MAY define additional normalized failures.

---

# Authentication Failure Boundary

A raw provider:

```text
401
invalid_api_key
```

SHOULD normally be normalized by the provider/plugin implementation.

Generic callers SHOULD consume something like:

```text
CREDENTIAL_INVALID
```

or an owning provider-management error category.

---

# Retryability

Plugin errors MAY provide a normalized:

```text
retryabilityHint
```

The plugin does NOT decide global Retry orchestration.

Runtime Retry Policy makes the final decision.

---

# Cancellation

Cancellation MUST be distinguishable from failure.

Recommended:

```text
CANCELLED
```

rather than generic `RuntimeError`.

---

# Diagnostics

Detailed implementation errors SHOULD be available through restricted diagnostics when needed.

They SHOULD NOT be embedded in normal user-facing error messages.

---

# Observability Contract

Plugins SHOULD use Host-provided logging and telemetry contracts where possible.

This preserves:

* correlation,
* Workspace isolation,
* field conventions,
* sensitive-data rules.

---

# Correlation

Capability calls SHOULD propagate relevant identifiers such as:

```text
requestId?
operationId?
correlationId
traceContext?
workspaceId
```

The exact identifiers depend on capability.

---

# Logging

Plugins MUST NOT log by default:

* secrets,
* raw credentials,
* unrestricted source content,
* private configuration,
* full Prompt/Context unless explicitly authorized.

---

# Metrics

Plugins MAY emit normalized metrics such as:

* invocation count,
* latency,
* error count,
* provider latency,
* resource usage.

Metric names/dimensions SHOULD follow host conventions.

---

# Trace

Host trace context SHOULD be propagated into plugin execution where possible.

A plugin MAY create child spans.

It MUST NOT create an unrelated root trace for every internal operation unless necessary.

---

# API Threading / Concurrency

The Plugin API SHOULD define concurrency expectations per capability.

A capability MAY declare:

```text
CONCURRENT
SERIALIZED
SINGLE_FLIGHT
IMPLEMENTATION_DEFINED
```

or equivalent metadata.

---

# Thread Safety

Plugins MUST NOT be assumed thread-safe unless the relevant contract says so.

The Host SHOULD respect declared concurrency semantics.

---

# Reentrancy

Reentrant invocation requirements MUST be explicit for capabilities where callbacks/events may cause nested execution.

---

# Deadlines

Capability calls SHOULD carry an explicit deadline or timeout context where meaningful.

The plugin MUST NOT silently replace the operation deadline with an unrelated longer timeout.

---

# Resource Limits

Host MAY impose:

* memory limits,
* execution time limits,
* concurrency limits,
* queue limits,
* output-size limits.

Plugins MUST respect normalized resource constraints.

---

# API Version Compatibility

Compatibility SHOULD consider at least:

```text
Plugin API version
Capability contract version
Plugin version
CRAI compatibility range
Platform
```

---

# Host Version Boundary

The previous model of only:

```text
minimum CRAI version
maximum CRAI version
```

MAY be retained as one compatibility hint.

However capability/API contract compatibility SHOULD be primary.

Application version alone is often too coarse.

---

# API Evolution

Rules:

1. Backward-compatible changes SHOULD remain within the current major Plugin API version.

2. Breaking Base Plugin API changes require a new major Plugin API version.

3. Breaking capability-contract changes require new capability contract versions.

4. Deprecated contracts SHOULD remain available for a documented transition period where feasible.

5. A plugin MUST NOT be activated against an incompatible contract.

---

# Capability Versioning

Capability contracts are versioned independently.

Example:

```text
recognition.engine/v1
recognition.engine/v2
```

A plugin MAY support several compatible versions.

---

# Version Negotiation

Host and plugin MAY negotiate the highest mutually supported public contract version.

Negotiation MUST be deterministic.

---

# Compatibility Failure

Incompatible plugins SHOULD remain:

```text
DISCOVERED / VALIDATED
```

with a structured compatibility failure.

They MUST NOT be partially activated.

---

# API Extensions

Optional API extensions MAY be introduced using explicit capability/feature negotiation.

Unknown optional extensions MUST NOT break otherwise compatible plugins.

---

# Execution Isolation Boundary

The same logical Plugin API SHOULD be usable across:

```text
IN_PROCESS
OUT_OF_PROCESS
SANDBOXED
REMOTE
```

execution models where practical.

---

# Serialization Boundary

When plugins run across a process/network boundary, capability contracts MUST be serializable.

In-process implementations SHOULD preserve equivalent logical semantics.

---

# Remote Failure

Transport failure to an out-of-process/remote plugin MUST be normalized separately from capability business failure where useful.

---

# Host Shutdown

Plugin API MUST support coordinated host shutdown.

Outstanding calls SHOULD be:

* drained,
* cancelled,
* or failed predictably

according to runtime policy.

---

# Capability Unregistration

After plugin deactivation:

```text
its capabilities MUST NOT receive new invocations
```

Capability registry/binding must be updated before unsafe unload.

---

# Backward Compatibility Boundary

“Backward compatible whenever possible” remains a goal, not permission to maintain ambiguous semantics forever.

Deprecated contracts SHOULD have:

```text
deprecationVersion
replacement
removalTarget?
migrationGuidance
```

where practical.

---

# Security Contract

The Plugin API MUST enforce least privilege through explicit permissions and Host Service boundaries.

Plugins MUST NOT receive broad internal application access merely because they implement a valid capability.

---

# Security Context

Capability invocation MAY include an opaque:

```text
securityContext
```

or equivalent authorized scope.

Plugins MUST NOT fabricate or elevate it.

---

# Workspace Isolation

Workspace scope MUST propagate through plugin calls that access Workspace-owned data.

A shared plugin instance MUST NOT mix data between Workspaces.

---

# Principal Authorization

Where user/principal permissions matter, authorization context MUST remain preserved across the Plugin API boundary.

---

# API Boundary Data Ownership

Objects crossing the API boundary SHOULD have clear ownership/lifetime semantics.

Plugins MUST NOT retain mutable references to Host-owned internal objects.

Prefer:

* immutable values,
* copied DTOs,
* opaque handles,
* versioned references.

---

# Mutable Shared State

Shared mutable memory between Host and plugin SHOULD be avoided.

It makes:

* unload,
* isolation,
* concurrency,
* process separation,
* versioning

much harder.

---

# File / Binary Payloads

Large payloads MAY use:

* opaque handles,
* stream interfaces,
* temporary-file references,
* shared-buffer abstractions.

The API SHOULD avoid mandatory full-memory copies where performance matters.

---

# Streaming Capability Contracts

Capability contracts MAY support streaming.

Streaming interfaces SHOULD define:

* stream identity,
* chunk ordering,
* cancellation,
* completion,
* errors,
* backpressure.

Where AI streaming is involved, semantics SHOULD align with `../ai/STREAMING.md`.

---

# Capability Selection Boundary

Plugin API exposes implementations.

It does NOT define which implementation is selected.

Examples:

```text
AI
    -> AI Routing
```

```text
Recognition
    -> Recognition selection policy
```

Plugin Manager MUST NOT become the universal selector.

---

# Capability Availability

Capability availability MAY differ from plugin lifecycle.

Example:

```text
Plugin ACTIVE
RecognitionCapability AVAILABLE
DictionaryCapability DEGRADED
```

Capability status SHOULD therefore be queryable/observable separately where required.

---

# Architecture Invariants

1. Plugin API is public, versioned and implementation-independent.

2. CRAI Core/modules MUST NOT depend on concrete plugin implementation APIs.

3. Base Plugin Contract SHOULD remain minimal.

4. Business-specific methods belong to capability contracts, not the generic Plugin interface.

5. Plugins declare capabilities explicitly.

6. Capability contracts are versioned independently from Plugin API version.

7. Capability identity is distinct from plugin identity.

8. Built-in and plugin implementations MAY implement the same public capability contract.

9. Plugin API does not transfer business ownership from CRAI modules/domains to plugins.

10. Plugin lifecycle is coordinated by Plugin Manager.

11. Not every plugin is required to run background work.

12. Lifecycle hooks MAY vary by declared execution model.

13. Lifecycle transition semantics SHOULD be deterministic.

14. Health is not mandatory on the generic Base Plugin Contract.

15. Health observation is distinct from authoritative runtime health projection.

16. Plugins receive scoped configuration only.

17. Plugins MUST NOT read arbitrary application configuration directly.

18. Raw secrets SHOULD NOT be included in ordinary Plugin Configuration.

19. Provider Configuration ownership remains outside arbitrary plugin-private state.

20. Retry Policy remains owned by runtime/recovery architecture.

21. Model selection remains owned by the relevant selection/routing architecture.

22. Cache semantics remain owned by Cache architecture.

23. Host Services MUST be accessed through public contracts.

24. Global internal service-locator access is forbidden.

25. Sensitive Host Services MUST enforce permission grants.

26. Plugin Manager does not need to perform every resource access check itself.

27. Requested permission does not imply granted permission.

28. Plugins operate with least privilege.

29. Raw CRAI internal database access is forbidden.

30. Unrestricted filesystem/network/clipboard access is forbidden by default.

31. Event Bus is used for asynchronous events, not every plugin interaction.

32. Synchronous capability calls SHOULD use typed public contracts.

33. Plugins MUST NOT depend on another plugin's private implementation APIs.

34. Capability dependencies SHOULD be preferred over concrete plugin dependencies.

35. Plugin runtime events are not automatically Domain Events.

36. Errors crossing Plugin API boundaries MUST be normalized.

37. Implementation-specific exceptions MUST NOT escape as required caller semantics.

38. Retryability hint does not transfer Retry ownership to the plugin.

39. Cancellation is distinct from generic failure.

40. Capability calls SHOULD propagate correlation context.

41. Plugin telemetry SHOULD use Host conventions.

42. Sensitive plugin telemetry MUST be minimized.

43. Concurrency/thread-safety expectations MUST be explicit where relevant.

44. Deadlines/cancellation MUST propagate through long-running capability calls.

45. Plugin API contracts SHOULD work across different execution-isolation models where practical.

46. Cross-process implementations MUST preserve the same logical contract.

47. Shared mutable Host/plugin state SHOULD be avoided.

48. Workspace isolation MUST be preserved across Plugin API calls.

49. Principal authorization context MUST be preserved where required.

50. Plugins MUST NOT fabricate or elevate security context.

51. Large payload handling SHOULD support efficient opaque/streaming representations where needed.

52. Capability unregistration MUST occur before unsafe unload.

53. Incompatible plugins MUST NOT be partially activated.

54. Breaking API changes require explicit version evolution.

55. Deprecated contracts SHOULD have clear migration paths.

56. Plugin API does not own capability-provider selection.

57. Capability availability may differ from overall plugin lifecycle state.

58. Adding a new plugin implementation SHOULD require implementing public contracts, not modifying business modules.

59. Removing a plugin MUST NOT erase canonical domain truth.

60. Public API evolution MUST preserve provider/runtime independence.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* minimal Base Plugin Contract,
* Plugin Descriptor,
* Plugin API version,
* capability IDs,
* capability contract versions,
* capability declaration,
* synchronous capability invocation,
* asynchronous public events,
* prepare/start/stop/dispose lifecycle where needed,
* scoped Plugin Configuration,
* Host Logging Service,
* Host Telemetry Service,
* Host Storage Service,
* controlled Network Service,
* controlled File Service,
* Clipboard Service,
* cancellation propagation,
* deadline propagation,
* standardized PluginError,
* capability-unavailable errors,
* permission-denied errors,
* Workspace scope,
* correlation context,
* basic health probe where supported,
* version compatibility validation,
* built-in/plugin contract parity.

MVP SHOULD NOT expose:

* raw application container,
* arbitrary database connections,
* unrestricted filesystem,
* unrestricted network by default,
* raw secrets by default,
* arbitrary application configuration.

MVP MAY defer:

* remote Plugin API transport,
* generic RPC protocol,
* full sandbox RPC,
* tool-like dynamic capability invocation,
* complex capability negotiation,
* advanced streaming contracts beyond required extensions,
* principal-specific permission grants,
* hot API upgrade,
* cross-process zero-copy buffers,
* arbitrary plugin-defined Host Services.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact Base Plugin interface,
* whether `prepare/start/stop/dispose` names are retained,
* whether lifecycle hooks are async,
* exact PluginDescriptor representation,
* capability ID naming,
* capability-version format,
* capability negotiation protocol,
* whether built-in implementations use the same runtime registry,
* Host Service injection mechanism,
* Configuration View schema,
* secret/credential broker interface,
* network-host restriction model,
* file-handle abstraction,
* plugin storage namespace contract,
* Event Bus contract,
* event permission model,
* normalized PluginError schema,
* error-code ownership between plugin and capability,
* retryability-hint semantics,
* health probe interface,
* health state taxonomy,
* concurrency metadata,
* streaming capability interface,
* cancellation token/context representation,
* deadline representation,
* security-context representation,
* large-payload transport,
* out-of-process serialization format,
* IPC transport,
* API feature negotiation,
* deprecation metadata,
* API compatibility testing,
* generated SDK/interface artifacts.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_SYSTEM.md`
* `PLUGIN_REGISTRY.md`
* `PLUGIN_DISCOVERY.md`
* `PLUGIN_LIFECYCLE.md`
* `PLUGIN_DEPENDENCY.md`
* `PLUGIN_CONFIGURATION.md`
* `PLUGIN_SECURITY.md`
* `PLUGIN_VERSIONING.md`

Architecture:

* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`
* `../core/EVENT_BUS.md`

AI:

* `../ai/MODELS.md`
* `../ai/ROUTING.md`
* `../ai/STREAMING.md`
* `../ai/RETRY.md`
* `../ai/CACHE.md`

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
* `../../03-infrastructure/storage/`

Runtime:

* `../runtime/CANCELLATION.md`
* `../runtime/RESOURCE_LIFECYCLE.md`
* `../runtime/RUNTIME_COMPONENTS.md`
