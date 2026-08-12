# Plugin Registry

* **Document:** Plugin Architecture / Plugin Registry
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Plugin Registry stores and exposes the canonical registry metadata for plugins known to CRAI.

It provides a stable catalog of:

* plugin identity,
* plugin descriptors,
* declared capabilities,
* declared dependencies,
* compatibility state,
* enablement state,
* installation/discovery provenance,
* configuration references,
* permission references,
* lifecycle registration references.

The Registry MUST NOT become the authoritative owner of every dynamic runtime concern associated with plugins.

---

# Core Principle

```text
Plugin Package / Built-in Extension
        |
        v
Discovery
        |
        v
Validation
        |
        v
Plugin Registry
        |
        +--> Capability Index
        +--> Dependency Resolver
        +--> Plugin Manager
        +--> Configuration
        +--> Security / Permissions
```

The Registry answers:

```text
What plugin is known?
What does it declare?
Is it enabled and compatible?
```

It does NOT answer every runtime question such as:

```text
Is the plugin currently healthy?
How many requests is it processing?
What is its current latency?
```

---

# Scope

The Plugin Registry owns persistent or authoritative registry metadata such as:

* Plugin ID,
* descriptor version,
* plugin version,
* Plugin API version,
* capability declarations,
* dependency declarations,
* compatibility classification,
* enablement state,
* block state,
* installation/discovery provenance,
* configuration reference,
* permission-grant references,
* lifecycle registration metadata.

---

# Non-Goals

The Registry does NOT own:

* plugin package execution,
* plugin loading,
* capability invocation,
* runtime workers,
* active request count,
* telemetry,
* runtime health computation,
* dependency-resolution algorithm,
* capability-provider selection,
* plugin configuration contents,
* provider credentials,
* plugin-private business data.

---

# Design Principles

The Plugin Registry SHOULD be:

* authoritative for registry metadata,
* implementation-independent,
* capability-oriented,
* version-aware,
* lifecycle-aware,
* fast to query,
* deterministic,
* integrity-preserving,
* runtime-independent where possible,
* safe for concurrent access,
* observable.

---

# Registry vs Runtime State

Critical distinction:

```text
Registry State
    = durable/declarative plugin metadata
```

```text
Runtime State
    = current execution state
```

Example:

```text
Registry:
    plugin enabled
    plugin compatible
    capability declared
```

while:

```text
Runtime:
    plugin process active
    capability unavailable
    provider degraded
```

These MUST remain separable.

---

# Registry Model

Recommended:

```text
PluginRegistryEntry
├── pluginId
├── descriptor
├── registration
├── installationState
├── enablementState
├── compatibilityState
├── capabilityDeclarations[]
├── dependencyDeclarations[]
├── configurationReference?
├── permissionGrantReferences[]
├── trustReference?
├── lifecycleReference?
├── provenance
├── createdAt
├── updatedAt
└── registryRevision
```

Dynamic health/runtime statistics SHOULD NOT live directly inside the authoritative registry entry.

---

# Plugin Descriptor

The Registry stores the validated Plugin Descriptor.

Recommended fields MAY include:

```text
PluginDescriptor
├── pluginId
├── pluginVersion
├── pluginApiVersion
├── displayName?
├── publisher?
├── description?
├── license?
├── supportedPlatforms[]
├── supportedCRAIRange?
├── capabilities[]
├── requiredDependencies[]
├── optionalDependencies[]
├── requiredPermissions[]
├── configurationSchemaReference?
├── executionModel?
└── integrityMetadata?
```

---

# Descriptor Immutability

A validated descriptor SHOULD be treated as immutable for that exact plugin artifact/version.

If plugin metadata materially changes:

```text
new artifact / descriptor revision
```

SHOULD be registered rather than silently mutating historical descriptor identity.

---

# Registry Entry Mutability

Not every field is immutable.

Examples of mutable registry state:

```text
enablementState
blockState
compatibilityState
configurationReference
permissionGrantReferences
```

These MAY change independently from immutable descriptor metadata.

---

# Plugin Identity

`pluginId` is the stable plugin identity.

It MUST remain distinct from:

```text
pluginVersion
descriptorRevision
installationId
runtimeInstanceId
capabilityProviderId
```

---

# Plugin Version

One plugin ID MAY have several installed/known versions.

Possible model:

```text
plugin.example
├── 1.0.0
├── 1.1.0
└── 2.0.0
```

The Registry MUST NOT assume only one version can ever exist historically.

---

# Installed Version

CRAI MAY choose to allow only one active installed version per plugin ID in MVP.

That is an installation policy.

It is NOT a semantic requirement of registry architecture.

---

# Registration

Registration creates or updates Registry knowledge from a validated plugin artifact or built-in extension descriptor.

Recommended flow:

```text
Discovery Result
      |
      v
Validation
      |
      v
Registry Registration
```

Registration MUST NOT execute plugin code.

---

# Registration Preconditions

Before registration, the Registry SHOULD receive:

* validated descriptor,
* artifact identity,
* discovery provenance,
* integrity result where required.

The Registry SHOULD NOT itself load the plugin merely to discover missing metadata.

---

# Registration Result

Recommended:

```text
PluginRegistrationResult
├── pluginId
├── pluginVersion
├── registrationStatus
├── registryRevision
├── warnings[]
└── conflicts[]
```

---

# Registration Conflict

Possible conflicts include:

```text
PLUGIN_ID_CONFLICT
PLUGIN_VERSION_CONFLICT
DESCRIPTOR_CONFLICT
ARTIFACT_IDENTITY_CONFLICT
```

Conflicts MUST preserve existing Registry integrity.

---

# Discovery Provenance

Registry SHOULD retain how the plugin became known.

Examples:

```text
BUILT_IN
LOCAL_DIRECTORY
PACKAGE_INSTALL
APPLICATION_BUNDLE
DEVELOPMENT_PATH
FUTURE_REPOSITORY
```

This MAY influence trust/security decisions.

---

# Installation State

Registry MAY track installation-related state separately from lifecycle.

Possible:

```text
KNOWN
INSTALLED
REMOVED
MISSING
```

Exact installation semantics depend on future package-management architecture.

---

# Enablement State

Recommended:

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
currently active
```

---

# Disabled

A disabled plugin:

* remains registered,
* remains discoverable,
* MUST NOT become eligible for normal activation or capability selection.

---

# Blocked

`BLOCKED` indicates activation is administratively/security prohibited.

Possible causes:

* trust failure,
* revoked publisher,
* policy denial,
* known vulnerability,
* manual administrative block.

---

# Incompatible Is Not Enablement

The previous model placed:

```text
INCOMPATIBLE
```

beside Enabled/Disabled/Blocked.

Compatibility SHOULD be a separate dimension.

Example:

```text
enablement:
    ENABLED

compatibility:
    INCOMPATIBLE
```

This preserves the reason the plugin cannot activate.

---

# Compatibility State

Recommended:

```text
UNKNOWN
COMPATIBLE
INCOMPATIBLE_API
INCOMPATIBLE_CAPABILITY
INCOMPATIBLE_PLATFORM
INCOMPATIBLE_CRAI_VERSION
INCOMPATIBLE_DEPENDENCY
```

Exact compatibility model belongs to `PLUGIN_VERSIONING.md`.

---

# Compatibility Evidence

Compatibility SHOULD retain relevant evidence such as:

```text
checkedAt
hostApiVersion
platform
dependencySnapshot?
checkerVersion
```

when useful.

---

# Capability Declaration

Registry stores plugin-declared capabilities.

Recommended:

```text
RegisteredCapabilityDeclaration
├── pluginId
├── pluginVersion
├── capabilityId
├── capabilityVersion
├── implementationVersion?
├── metadata
├── requirements?
└── registrationStatus
```

---

# Capability Identity

Capability ID represents a public extension contract.

Example:

```text
recognition.engine
translation.execution
capture.source
```

not:

```text
paddle
gemini
tesseract
```

---

# Capability Index

The Registry MAY maintain an index:

```text
Capability ID
    |
    +--> Capability Provider Declaration A
    +--> Capability Provider Declaration B
    +--> Capability Provider Declaration C
```

Example:

```text
recognition.engine
├── built-in recognizer
├── plugin ocr.paddle
└── plugin ocr.tesseract
```

---

# Capability Index vs Capability Runtime Registry

Critical distinction:

```text
Registry Capability Index
    = declared/known providers
```

```text
Runtime Capability Registry
    = currently bound/active providers
```

These MAY be separate projections.

---

# Declared Does Not Mean Available

A registered plugin MAY declare:

```text
translation.execution
```

while currently:

```text
plugin not loaded
provider unavailable
permission denied
capability degraded
```

Therefore:

```text
declared capability
    !=
runtime available capability
```

---

# Capability Provider Identity

Where needed, Registry MAY expose a stable provider record:

```text
CapabilityProviderRecord
├── capabilityProviderId
├── capabilityId
├── pluginId?
├── implementationKind
├── implementationVersion
└── descriptorReference
```

Possible implementation kinds:

```text
PLUGIN
BUILT_IN
EXTERNAL_ADAPTER
```

---

# Built-In Providers

If built-in implementations participate in the same capability-selection mechanism, they MAY appear in a generic Capability Provider catalog.

They do not necessarily need to masquerade as plugins.

This decision remains open.

---

# Dependency Declarations

Registry stores declarations such as:

```text
requiredPluginDependencies[]
optionalPluginDependencies[]
requiredCapabilityDependencies[]
optionalCapabilityDependencies[]
```

---

# Capability Dependencies Preferred

Where possible:

```text
requires capability X
```

SHOULD be preferred over:

```text
requires plugin Y
```

because capability-based dependencies reduce implementation coupling.

---

# Dependency Graph

Registry MAY expose enough data to construct a dependency graph.

Conceptually:

```text
Registry
    |
    v
Dependency Declarations
    |
    v
Dependency Resolver
```

---

# Dependency Resolution Boundary

The Registry MUST NOT own the complete dependency-resolution algorithm.

It supplies:

* dependency declarations,
* installed versions,
* compatibility metadata,
* enablement state.

`PLUGIN_DEPENDENCY.md` defines resolution semantics.

---

# Dependency Resolution Result

A separate resolver MAY produce:

```text
DependencyResolution
├── pluginId
├── resolvedDependencies[]
├── unresolvedDependencies[]
├── conflicts[]
├── cycles[]
└── resolutionVersion
```

The Registry MAY retain a reference/cache of that result.

---

# Configuration Reference

Registry MAY store:

```text
configurationReference
```

for the effective plugin configuration resource.

It MUST NOT store raw arbitrary configuration values unless explicitly part of Registry semantics.

---

# Secrets

Registry MUST NOT store:

* plaintext API keys,
* tokens,
* passwords,
* private keys.

It MAY store approved opaque credential/configuration references.

---

# Permission References

Registry MAY retain:

```text
permissionGrantReferences[]
```

or a permission summary.

Canonical permission-grant semantics belong to Plugin Security/permission infrastructure.

---

# Trust Reference

Registry MAY store plugin trust classification/reference.

Example:

```text
SIGNED_TRUSTED
USER_INSTALLED
DEVELOPMENT
```

Trust evaluation belongs to Security.

---

# Lifecycle Reference

Registry MAY retain the current coarse lifecycle state or reference needed for coordination.

However lifecycle history and runtime execution details SHOULD remain runtime/lifecycle-owned.

---

# Lifecycle State

Recommended high-level lifecycle states aligned with `PLUGIN_SYSTEM.md`:

```text
DISCOVERED
VALIDATED
RESOLVED
LOADED
INITIALIZED
ACTIVE
DISABLED
FAILED
STOPPING
UNLOADED
```

Exact transition rules belong to `PLUGIN_LIFECYCLE.md`.

---

# Registry Lifecycle State Boundary

The Registry MAY persist the latest lifecycle projection for recovery/diagnostics.

It MUST NOT become the engine that performs lifecycle transitions.

Plugin Manager coordinates lifecycle.

---

# Lifecycle History

Detailed lifecycle transition history SHOULD normally be stored as:

* telemetry,
* runtime execution records,
* Audit where materially required.

Do not turn the Registry entry into an ever-growing event history.

---

# Runtime Instance

A plugin may have a runtime instance identity such as:

```text
runtimeInstanceId
```

This belongs to runtime state.

It SHOULD NOT become Plugin identity.

---

# Multiple Runtime Instances

Architecture SHOULD NOT assume one plugin descriptor equals one runtime process forever.

Future execution may allow:

* multiple worker processes,
* isolated Workspace instances,
* restarted runtime instances.

Registry metadata must remain independent.

---

# Health

Health MUST remain separate from authoritative Registry metadata.

Possible runtime projection:

```text
PluginHealthProjection
├── pluginId
├── runtimeInstanceId?
├── state
├── observedAt
├── expiresAt?
├── reasonCodes[]
└── source
```

---

# Health State

Possible:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
UNKNOWN
```

The vocabulary SHOULD align with wider runtime/AI health semantics where appropriate.

---

# Health Source

Health MAY derive from:

* plugin health probe,
* runtime process state,
* dependency health,
* telemetry,
* provider health.

Therefore the old model:

```text
Registry health
    updated only from Plugin.Health()
```

is too restrictive.

---

# Runtime Statistics

Runtime statistics MUST NOT be authoritative Registry fields.

Examples:

```text
activeRequests
errorRate
latency
memoryUsage
CPU usage
```

belong to Observability/runtime metrics.

Registry MAY expose links/projections for diagnostics.

---

# Lookup API

Registry SHOULD support deterministic read-only queries.

Examples:

```text
findPlugin(pluginId)
findPluginVersion(pluginId, version)
listPlugins()
listEnabledPlugins()
listBlockedPlugins()
listCompatiblePlugins()
findCapabilityDeclarations(capabilityId)
listDependencies(pluginId)
getConfigurationReference(pluginId)
getPermissionReferences(pluginId)
```

---

# Lookup by Category

A generic:

```text
find by category
```

SHOULD NOT be a core architectural requirement unless Plugin Descriptor retains a useful classification taxonomy.

Capability lookup is generally preferred.

---

# Lookup by Runtime State

Queries such as:

```text
list running plugins
```

may require a runtime projection rather than the pure Registry store.

The API SHOULD make that distinction explicit.

---

# Registry Query Result

Recommended query results SHOULD return immutable snapshots/DTOs.

Avoid returning mutable internal Registry objects.

---

# Registry Snapshot

For deterministic dependency resolution or diagnostics, CRAI MAY expose:

```text
PluginRegistrySnapshot
├── registryRevision
├── entries[]
├── createdAt
└── contentHash?
```

This allows consumers to reason against one consistent Registry view.

---

# Registry Revision

Every material registry mutation SHOULD increment or otherwise identify:

```text
registryRevision
```

This helps:

* concurrency,
* diagnostics,
* dependency resolution,
* cache invalidation.

---

# Concurrent Mutations

Registry mutations SHOULD use concurrency-safe semantics.

Possible mechanisms:

* optimistic versioning,
* transactions,
* serialized mutation command path.

Implementation belongs to infrastructure.

---

# Registry Writes

Registry writes MAY include:

```text
register
remove
enable
disable
block
unblock
update compatibility projection
update configuration reference
update permission reference
```

Lifecycle state updates MAY be performed through Plugin Manager-owned commands.

---

# Write Authority

The previous invariant:

```text
Only Plugin Manager modifies Registry
```

is too broad.

Preferred rule:

```text
Only authorized Registry mutation paths
may modify Registry state
```

Different owners MAY legitimately update their own projections/references:

```text
Plugin Manager
Security
Configuration
Installer / Package Manager
Compatibility Evaluator
```

depending on final implementation.

---

# Command Boundary

Registry SHOULD expose explicit mutation commands rather than generic:

```text
updatePlugin(anything)
```

Examples:

```text
RegisterPlugin
SetPluginEnabled
SetPluginBlocked
UpdateCompatibilityState
SetConfigurationReference
SetPermissionGrantReferences
RemovePlugin
```

This preserves ownership.

---

# Remove Plugin

Removing a plugin from Registry MUST NOT delete canonical domain data created while the plugin was installed.

---

# Removal Preconditions

Removal MAY require:

* plugin inactive,
* no active capability invocation,
* dependency validation,
* configuration/data retention decision.

Package removal semantics belong to installer/lifecycle architecture.

---

# Missing Artifact

Registry MAY retain knowledge of a plugin whose artifact disappeared unexpectedly.

Possible state:

```text
installationState: MISSING
```

This is useful for diagnostics instead of silently deleting metadata.

---

# Registry Events

Registry MAY publish events for material registry-state changes.

Recommended events:

```text
PluginRegistered
PluginRemoved
PluginEnablementChanged
PluginBlockStateChanged
PluginCompatibilityChanged
PluginDescriptorVersionRegistered
PluginConfigurationReferenceChanged
PluginPermissionReferenceChanged
```

---

# Event Boundary

Do NOT emit `PluginUpdated` for every kind of mutation.

Specific event types improve:

* ownership,
* consumers,
* auditability,
* replay semantics.

---

# Registry Event Payload

Recommended:

```text
pluginId
pluginVersion?
previousState?
newState?
registryRevision
reasonCode?
correlationId?
occurredAt
```

Avoid embedding full plugin configuration/secrets.

---

# Registry Events vs Lifecycle Events

Registry events describe Registry state changes.

Lifecycle events describe runtime transitions.

They are related but distinct.

Example:

```text
PluginEnabled
    = registry/administrative state
```

```text
PluginActivated
    = lifecycle/runtime state
```

---

# Registry Events vs Domain Events

Registry events are platform/application events.

They are NOT business Domain Events such as Translation or Character events.

---

# Failure Handling

Registry failures MUST preserve existing consistency.

Possible failures:

```text
PLUGIN_REGISTRY_DUPLICATE_ID
PLUGIN_REGISTRY_VERSION_CONFLICT
PLUGIN_REGISTRY_INVALID_DESCRIPTOR
PLUGIN_REGISTRY_WRITE_CONFLICT
PLUGIN_REGISTRY_NOT_FOUND
PLUGIN_REGISTRY_INTEGRITY_ERROR
PLUGIN_REGISTRY_STORAGE_UNAVAILABLE
PLUGIN_REGISTRY_PERMISSION_DENIED
```

---

# Duplicate Registration

Duplicate registration is not always an error.

Cases SHOULD distinguish:

```text
same pluginId + same version + same artifact
    -> idempotent registration possible
```

versus:

```text
same pluginId + same version + different artifact
    -> integrity conflict
```

---

# Idempotent Registration

Registration SHOULD be idempotent where identical artifact identity is supplied repeatedly.

This simplifies repeated discovery scans.

---

# Invalid Descriptor

Invalid descriptors MUST NOT become active Registry entries.

Discovery MAY retain diagnostic knowledge separately.

---

# Registry Storage Failure

If Registry storage is unavailable:

* existing in-memory snapshot MAY continue for safe read operations where policy permits,
* mutations SHOULD fail explicitly,
* plugin loading/activation MAY fail closed when durable consistency is required.

Exact recovery belongs to runtime/storage architecture.

---

# Registry Recovery

Registry MUST NOT recover by loading plugin implementations.

Recovery MAY include:

* reload persistent Registry state,
* rebuild from validated installed descriptors,
* reconcile known packages,
* restore previous consistent revision.

---

# Rebuildability

Where practical, Registry SHOULD be rebuildable from:

```text
installed plugin artifacts
+
validated descriptors
+
persistent administrative state
```

Administrative state such as permission grants or enablement may require separate durable storage.

---

# Persistence

Registry persistence MAY use:

* relational storage,
* local database,
* document storage,
* durable configuration storage.

Architecture MUST remain storage-technology independent.

---

# Registry Cache

A fast in-memory Registry index MAY exist.

It MUST remain a projection/cache of authoritative Registry state.

---

# Startup

Typical startup:

```text
Load Registry State
      |
      v
Discover Plugin Artifacts
      |
      v
Validate
      |
      v
Reconcile
      |
      v
Publish Registry Snapshot
```

Discovery and lifecycle activation remain separate.

---

# Reconciliation

Reconciliation compares:

```text
persistent Registry
```

with:

```text
currently discoverable artifacts
```

Possible outcomes:

```text
UNCHANGED
NEW
MISSING
VERSION_CHANGED
ARTIFACT_CHANGED
INVALID
```

---

# Runtime Activation Boundary

Registry entry being:

```text
ENABLED + COMPATIBLE
```

means:

```text
eligible for lifecycle resolution
```

not:

```text
already active
```

---

# Capability Selection Boundary

Registry exposes candidate capability declarations.

It does NOT choose which provider handles a business request.

Example:

```text
AIExecutionProvider candidates
    ->
AI Routing
```

```text
RecognitionEngine candidates
    ->
Recognition selection policy
```

---

# Registry and Plugin Manager

Plugin Manager consumes Registry data to:

* determine eligible plugins,
* resolve lifecycle operations,
* bind initialized capabilities,
* update lifecycle projections.

Registry does not invoke Plugin Manager behavior itself.

---

# Registry and Discovery

Discovery finds plugin artifacts/descriptors.

Registry stores validated known state.

```text
Discovery
    !=
Registry
```

---

# Registry and Dependency Resolver

Registry stores dependency declarations.

Resolver computes dependency solution.

```text
Dependency data
    !=
Dependency algorithm
```

---

# Registry and Configuration

Registry stores configuration references.

Configuration architecture owns configuration content, schema resolution and overrides.

---

# Registry and Security

Registry may store trust/permission references.

Security owns trust evaluation and permission-grant semantics.

---

# Registry and Observability

Registry metadata SHOULD be observable.

Runtime metrics/health MUST remain outside authoritative Registry state.

---

# Registry and Audit

Material registry mutations MAY require Audit:

* plugin registered/removed,
* enabled/disabled,
* blocked/unblocked,
* trust-related change,
* permission-related change.

Audit persistence remains separate.

---

# Registry and Versioning

Versioning architecture determines:

* Plugin API compatibility,
* capability-version compatibility,
* host compatibility,
* deprecation.

Registry stores resulting metadata/state.

---

# Security

Registry read/write APIs MUST respect authorization.

Untrusted plugins MUST NOT modify their own Registry entry directly.

---

# Plugin Self-Description

A plugin MAY expose its descriptor.

But canonical Registry state MUST come from Host validation.

The plugin MUST NOT be able to mark itself:

```text
trusted
enabled
compatible
```

without Host-controlled evaluation.

---

# Sensitive Registry Data

Registry SHOULD NOT contain:

* raw credentials,
* secret values,
* private plugin operational payloads.

---

# Architecture Invariants

1. Plugin Registry is authoritative for canonical plugin registry metadata.

2. Registry is not authoritative for all plugin runtime state.

3. Registry MUST NOT execute plugin code.

4. Registry MUST NOT load plugins.

5. Plugin Descriptor metadata is validated before canonical registration.

6. Descriptor metadata for an exact artifact/version SHOULD be immutable.

7. Mutable administrative state remains separate from immutable descriptor metadata.

8. Plugin identity is distinct from plugin version.

9. Plugin identity is distinct from runtime instance identity.

10. Registry MAY retain multiple historical/known plugin versions.

11. Enabled means eligible for activation, not currently active.

12. Disabled plugins remain discoverable.

13. Blocked plugins MUST NOT activate normally.

14. Compatibility is separate from enablement.

15. `INCOMPATIBLE` MUST NOT be modeled merely as an Enablement state.

16. Capability declarations are indexed independently from plugin display names.

17. Capability ID represents a public extension contract.

18. Declared capability does not imply runtime capability availability.

19. Capability Registry/Index and Runtime Capability Binding may be separate projections.

20. Registry MUST NOT become the universal capability selector.

21. Dependency declarations are Registry data.

22. Dependency-resolution algorithm belongs outside Registry.

23. Capability dependency SHOULD be preferred over concrete-plugin dependency where practical.

24. Registry MAY store configuration references but SHOULD NOT own full configuration semantics.

25. Registry MUST NOT store plaintext secrets.

26. Registry MAY store permission/trust references but does not own Security semantics.

27. Lifecycle state and health are separate.

28. Runtime statistics MUST NOT be authoritative Registry fields.

29. Health is a runtime projection.

30. Health MAY derive from more than Plugin HealthProbe alone.

31. Registry lookup operations SHOULD be read-only and deterministic.

32. Registry query results SHOULD not expose mutable internal objects.

33. Registry revisioning SHOULD identify material mutations.

34. Registry mutations MUST be concurrency safe.

35. Authorized mutation paths may involve more than Plugin Manager alone.

36. Untrusted plugins MUST NOT directly mutate Registry state.

37. Registry mutation APIs SHOULD be command-specific.

38. Registration SHOULD be idempotent for identical artifacts.

39. Same ID/version with different artifact identity is an integrity conflict.

40. Registry failures MUST preserve prior consistent state.

41. Registry MUST NOT recover failures by executing plugins.

42. Registry may be rebuildable from validated artifacts plus durable administrative state.

43. Registry storage implementation is infrastructure-specific.

44. Registry Cache is a projection, not canonical truth.

45. Registry events describe Registry mutations, not every runtime lifecycle transition.

46. Lifecycle events and Registry events remain distinct.

47. Registry events are not automatically Domain Events.

48. Removing a plugin MUST NOT delete canonical CRAI domain truth.

49. Missing plugin artifacts SHOULD be diagnosable rather than silently forgotten.

50. Registry access MUST preserve Workspace/security boundaries where applicable.

51. Plugin self-description MUST NOT grant trust/enablement/compatibility.

52. Capability selection remains owned by the relevant module/routing architecture.

53. Registry and Discovery are separate concerns.

54. Registry and Configuration are separate concerns.

55. Registry and Security are separate concerns.

56. Registry and Observability are separate concerns.

57. Registry and Dependency Resolver are separate concerns.

58. Registry and Plugin Manager are separate concerns.

59. Registry state SHOULD be sufficient for deterministic capability/dependency discovery without executing plugin code.

60. New plugin implementations SHOULD be registerable without changing business-domain architecture.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* stable `pluginId`,
* plugin version,
* validated Plugin Descriptor,
* Registry Entry,
* persistent registration,
* descriptor immutability,
* enable/disable state,
* blocked state,
* compatibility state,
* capability declarations,
* capability index,
* required/optional dependencies,
* configuration reference,
* permission-grant references,
* discovery provenance,
* plugin lookup,
* capability lookup,
* deterministic Registry revision,
* idempotent registration,
* duplicate/conflict detection,
* registry events,
* safe removal,
* registry reconciliation during startup,
* in-memory Registry index/cache.

MVP SHOULD NOT store as canonical Registry fields:

* runtime statistics,
* raw health telemetry,
* raw secrets,
* plugin-private business data.

MVP MAY defer:

* multiple simultaneously installed active versions,
* remote Registry synchronization,
* distributed Registry,
* signed registry snapshots,
* complex package repository state,
* cross-device plugin registry synchronization,
* Registry event sourcing,
* advanced Registry rollback,
* multi-process consensus.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact `PluginRegistryEntry` schema,
* exact Registry persistence model,
* whether multiple plugin versions may be installed simultaneously,
* installation-state taxonomy,
* exact enablement/block model,
* compatibility-state taxonomy,
* capability-provider identity model,
* built-in provider representation,
* runtime capability registry separation,
* lifecycle-state persistence depth,
* Registry revision mechanism,
* Registry snapshot format,
* configuration-reference ownership,
* permission-reference ownership,
* trust-reference representation,
* discovery provenance schema,
* artifact identity/hash format,
* idempotent registration behavior,
* registry reconciliation strategy,
* missing-artifact policy,
* mutation authorization model,
* startup recovery behavior,
* registry event schema,
* registry cache invalidation,
* Registry rebuild procedure,
* removal dependency checks,
* Registry Audit requirements.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_SYSTEM.md`
* `PLUGIN_API.md`
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
* `../ai/OBSERVABILITY.md`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/secret-management/`

Runtime:

* `../runtime/RUNTIME_COMPONENTS.md`
* `../runtime/RESOURCE_LIFECYCLE.md`
