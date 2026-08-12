# Plugin Dependency

* **Document:** Plugin Architecture / Plugin Dependency
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI plugin dependencies are declared, validated, resolved and tracked.

The dependency architecture ensures that plugins activate only when required prerequisites are satisfied while preserving:

* explicit contracts,
* deterministic resolution,
* capability-first coupling,
* version compatibility,
* lifecycle correctness,
* failure isolation.

Dependency resolution determines whether a plugin is eligible to progress into the `RESOLVED` lifecycle state.

It does NOT execute plugin lifecycle transitions itself.

---

# Core Principle

```text
Plugin Descriptor
      |
      v
Dependency Declarations
      |
      v
Dependency Resolver
      |
      v
DependencyResolution
      |
      +--> RESOLVED
      |
      +--> UNRESOLVED
      |
      v
Plugin Lifecycle
```

The Resolver answers:

```text
Can this plugin's declared prerequisites be satisfied?
```

It does NOT answer:

```text
Which provider should handle a business request?
```

---

# Scope

Plugin Dependency architecture covers:

* dependency declaration,
* dependency types,
* version constraints,
* capability dependencies,
* concrete plugin dependencies,
* Host Service dependencies,
* optional dependencies,
* dependency resolution,
* conflict detection,
* cycle detection,
* startup ordering,
* shutdown ordering,
* runtime dependency loss,
* dependency diagnostics.

---

# Non-Goals

Dependency architecture does NOT own:

* plugin loading,
* plugin activation,
* capability-provider business routing,
* AI Routing,
* Recognition selection,
* Provider Management,
* Host Service implementation,
* plugin health,
* restart policy,
* Registry enablement state.

---

# Design Principles

Plugin dependencies SHOULD be:

* explicit,
* versioned,
* minimal,
* capability-first,
* implementation-independent where possible,
* deterministic,
* cycle-safe,
* lifecycle-aware,
* observable.

---

# Dependency Categories

Recommended categories:

```text
PLUGIN
CAPABILITY
HOST_SERVICE
PLATFORM
RUNTIME
OPTIONAL
```

A dependency may also carry:

```text
REQUIRED
OPTIONAL
```

criticality.

---

# Dependency Declaration

Recommended:

```text
PluginDependencyDeclaration
├── dependencyId
├── dependencyType
├── target
├── required
├── versionRange?
├── capabilityVersionRange?
├── bindingPolicy?
├── activationRequirement?
└── metadata?
```

---

# Required Dependency

A required dependency must be satisfied before the plugin may become `RESOLVED`.

If not satisfied:

```text
Plugin lifecycle eligibility:
    UNRESOLVED
```

The plugin MUST NOT activate.

---

# Required Dependency Does Not Mean Blocked

Critical distinction:

```text
Dependency unresolved
    !=
Registry BLOCKED
```

`BLOCKED` represents administrative/security prohibition.

A dependency failure is a runtime/compatibility eligibility problem.

---

# Optional Dependency

An optional dependency enhances functionality but is not required for activation.

If unavailable:

```text
plugin MAY still activate
```

with:

```text
reduced capability
degraded optional feature
```

where explicitly supported.

---

# Optional Dependency Declaration

Optional dependencies SHOULD declare what feature/capability is affected.

Avoid opaque behavior such as:

```text
optional plugin missing
    -> silently change arbitrary functionality
```

---

# Plugin Dependency

A concrete Plugin Dependency references a specific plugin identity.

Example:

```text
requires:
    pluginId = example.codec
```

This SHOULD be used only when the dependent plugin genuinely requires that implementation or plugin-owned protocol.

---

# Concrete Plugin Dependency Is Exceptional

Prefer:

```text
requires capability X
```

instead of:

```text
requires plugin Y
```

when any compatible implementation can satisfy the need.

---

# Valid Concrete Dependency Examples

Concrete plugin dependency MAY be appropriate when:

* plugin extension protocol is implementation-specific,
* plugin package is split into coordinated subplugins,
* shared private data format requires exact companion plugin,
* migration/compatibility contract is plugin-specific.

Such coupling SHOULD remain explicit.

---

# Capability Dependency

A Capability Dependency references a public capability contract.

Example:

```text
requires:
    recognition.language-hint/v1
```

Possible providers MAY include:

```text
built-in implementation
plugin A
plugin B
```

The dependent plugin does not need to know which implementation provides it.

---

# Capability Dependency Resolution

The Resolver determines:

```text
Is there at least one compatible eligible capability provider?
```

It MAY bind a provider according to dependency-binding policy.

It MUST NOT become a universal business request Router.

---

# Dependency Binding vs Business Selection

Critical distinction:

```text
Dependency Binding
    = satisfy plugin activation/runtime prerequisite
```

```text
Business Capability Selection
    = choose implementation per operation
```

Example:

```text
Plugin needs Logging Host Service
    -> dependency binding

Translation request needs AI model
    -> AI Routing
```

---

# Dynamic Capability Dependency

Some plugins may require:

```text
at least one provider of capability X
```

rather than a permanently fixed provider.

In that case, Dependency Resolution MAY validate availability without creating a fixed implementation binding.

---

# Fixed Capability Binding

A fixed capability binding MAY be used when:

* plugin maintains long-lived dependency state,
* background worker requires one stable provider,
* protocol negotiation occurs during initialization.

This behavior MUST be explicit.

---

# Host Service Dependency

Plugins SHOULD use Host Service dependencies for platform/runtime services.

Examples:

```text
logging
telemetry
network-client
storage
scheduler
clock
temporary-files
credential-broker
```

Do NOT model these as arbitrary helper plugins unless they are truly plugin-provided capabilities.

---

# HTTP Client Example

Avoid:

```text
Translation Plugin
    -> HTTP Client Plugin
```

Prefer:

```text
Translation Plugin
    -> Host Network Service
```

unless CRAI intentionally defines HTTP transport as a pluggable public capability.

---

# Platform Dependency

A plugin MAY require:

```text
operating system
architecture
GPU capability
specific runtime
```

Example:

```text
platform:
    linux-x86_64

runtime:
    CUDA >= N
```

These are environment dependencies, not plugin dependencies.

---

# Runtime Dependency

Runtime dependencies MAY include:

* local model runtime,
* browser integration runtime,
* GPU runtime,
* native library runtime.

They SHOULD be normalized as environment prerequisites.

---

# Provider Dependency

A plugin SHOULD NOT normally depend on a concrete external provider through Plugin Dependency architecture if Provider Management already owns that relationship.

Instead:

```text
plugin capability
    consumes ProviderConfiguration / Provider Adapter contract
```

where appropriate.

---

# Dependency Manifest

Plugin Descriptor MAY declare:

```text
dependencies:
    requiredPlugins[]
    optionalPlugins[]
    requiredCapabilities[]
    optionalCapabilities[]
    requiredHostServices[]
    optionalHostServices[]
    platformRequirements[]
    runtimeRequirements[]
```

Exact schema remains open.

---

# Dependency Version Range

Dependencies SHOULD support version ranges.

Conceptually:

```text
>=1.2.0 <2.0.0
```

or another standardized range format.

Exact syntax is defined by `PLUGIN_VERSIONING.md`.

---

# Plugin Version vs Capability Version

These MUST remain separate.

Example:

```text
plugin:
    plugin.foo 3.1.0

capability:
    recognition.engine/v2
```

A plugin version does not imply one capability-contract version.

---

# Plugin API Dependency

A plugin's supported Plugin API range is not a normal dependency edge to another plugin.

It is Host compatibility metadata.

---

# CRAI Version Compatibility

Supported CRAI application version MAY remain a compatibility hint.

Public API/capability compatibility SHOULD remain more precise than application version alone.

---

# Dependency Graph

The dependency graph represents required ordering relationships.

Nodes MAY include:

```text
plugin runtime
capability binding
Host Service
environment prerequisite
```

However topological plugin ordering is only meaningful for dependencies that require activation ordering.

---

# Plugin Activation Graph

Recommended graph for plugin lifecycle ordering:

```text
Plugin A
    depends on
Plugin B
```

creates:

```text
B ACTIVE
    before
A ACTIVE
```

when the dependency requires runtime activation.

---

# Capability Dependency Graph

A capability dependency MAY create an edge to:

```text
Capability X
```

rather than directly to Plugin B.

Provider binding can later map that capability to a compatible provider.

---

# Host Service Dependencies Are Roots

Host Services normally act as runtime roots.

They SHOULD NOT participate in plugin cycles.

---

# Optional Edges

Optional dependencies SHOULD NOT prevent resolution when absent.

They MAY affect feature availability and diagnostics.

---

# Dependency Resolution Input

Recommended:

```text
PluginDependencyResolutionRequest
├── pluginId
├── pluginVersion
├── registrySnapshot
├── capabilityProviderSnapshot
├── hostServiceSnapshot
├── platformSnapshot
├── runtimeSnapshot
├── permissionSnapshot?
├── compatibilitySnapshot
└── resolverVersion
```

---

# Dependency Resolution Result

Recommended:

```text
PluginDependencyResolution
├── resolutionId
├── pluginId
├── pluginVersion
├── status
├── resolvedDependencies[]
├── unresolvedDependencies[]
├── optionalUnavailable[]
├── conflicts[]
├── cycles[]
├── activationOrder?
├── shutdownOrder?
├── warnings[]
├── resolverVersion
└── createdAt
```

---

# Resolution Status

Possible:

```text
RESOLVED
UNRESOLVED
CONFLICTED
CYCLIC
INCOMPATIBLE
```

---

# Resolution Is Immutable

A stored resolution result SHOULD represent one exact dependency snapshot.

If Registry/runtime conditions change:

```text
create/recompute resolution
```

rather than silently mutating historical resolution provenance.

---

# Resolution Freshness

Some dependencies are dynamic.

Therefore a Resolution MAY have:

```text
expiresAt?
dynamicDependencyReferences[]
```

or require revalidation before activation.

---

# Resolution Pipeline

Recommended:

```text
Read Dependency Declarations
        |
        v
Validate Dependency Syntax
        |
        v
Resolve Host / Platform Requirements
        |
        v
Resolve Required Plugin Dependencies
        |
        v
Resolve Capability Dependencies
        |
        v
Validate Version Constraints
        |
        v
Detect Conflicts
        |
        v
Detect Cycles
        |
        v
Build Activation Ordering
        |
        v
DependencyResolution
```

Lifecycle loading occurs after this result.

---

# Resolver Does Not Load Plugins

Critical rule:

```text
Dependency Resolver
    MUST NOT
load or initialize plugins
```

It produces an eligibility/ordering result.

Plugin Lifecycle consumes it.

---

# Resolver Does Not Select Business Providers

The Resolver MUST NOT implement:

```text
AI model selection
Translation provider selection
Recognition engine routing
```

Those remain owned by their respective architectures.

---

# Capability Candidate Filtering

For dependency purposes, the Resolver MAY filter capability providers using:

* capability version,
* plugin enablement,
* compatibility,
* permissions,
* lifecycle eligibility.

It SHOULD NOT score them by business quality/cost/latency unless the dependency contract explicitly requires a stable binding policy.

---

# Dependency Binding Policy

Possible binding policies MAY include:

```text
ANY_COMPATIBLE
PREFERRED_PROVIDER
FIXED_PROVIDER
HOST_DEFAULT
DYNAMIC
```

These apply only to dependency satisfaction.

---

# Fixed Provider Dependency

If a plugin explicitly requires one exact capability provider:

```text
FIXED_PROVIDER
```

the dependency becomes implementation-coupled.

This SHOULD be exceptional and documented.

---

# Deterministic Resolution

For identical:

* dependency declarations,
* Registry Snapshot,
* Capability Provider Snapshot,
* Host Service Snapshot,
* platform/runtime state,
* resolver version,

resolution SHOULD be deterministic.

---

# Stable Tie-Break

When multiple equivalent providers satisfy a dependency, deterministic binding SHOULD use a stable tie-break rule.

Avoid random dependency binding.

---

# Cycle Detection

Required dependency cycles are forbidden.

Example:

```text
Plugin A
    -> Plugin B
    -> Plugin A
```

results in:

```text
CYCLIC
```

Affected plugins MUST NOT activate.

---

# Capability Cycle

Cycles may also occur indirectly:

```text
Plugin A requires Capability X
Capability X bound to Plugin B

Plugin B requires Capability Y
Capability Y bound to Plugin A
```

The Resolver MUST detect cycles after binding decisions where fixed bindings create such edges.

---

# Optional Cycles

Optional dependency cycles require explicit semantics.

MVP SHOULD avoid resolving optional cyclic activation dependencies.

A safe policy is to ignore optional edges for mandatory activation ordering unless explicitly required.

---

# Self Dependency

A plugin MUST NOT declare itself as a required concrete plugin dependency.

---

# Capability Self-Satisfaction

A plugin MAY potentially provide a capability it also references.

This is only valid if the dependency does not require the capability before plugin initialization/activation.

Otherwise it forms an invalid lifecycle cycle.

---

# Dependency Conflict

Possible conflicts:

```text
VERSION_CONFLICT
CAPABILITY_VERSION_CONFLICT
MULTIPLE_FIXED_BINDINGS
PLATFORM_CONFLICT
PERMISSION_CONFLICT
RUNTIME_CONFLICT
PROVIDER_BINDING_CONFLICT
```

---

# Version Conflict

Example:

```text
Plugin A requires Plugin C <2

Plugin B requires Plugin C >=2
```

If only one incompatible C instance may be active:

```text
DEPENDENCY_VERSION_CONFLICT
```

---

# Multiple Versions

If future plugin isolation permits multiple versions simultaneously, some version conflicts may become resolvable.

MVP SHOULD not assume this capability.

---

# Missing Required Dependency

If required dependency is missing:

```text
resolution:
    UNRESOLVED
```

The plugin remains known/registered but is not eligible to activate.

---

# Missing Optional Dependency

If optional dependency is missing:

```text
resolution:
    RESOLVED
warnings:
    OPTIONAL_DEPENDENCY_UNAVAILABLE
```

when remaining requirements are satisfied.

---

# Dependency Failure vs Registry State

Dependency failure MUST NOT automatically change:

```text
enablement = BLOCKED
```

or:

```text
compatibility = INCOMPATIBLE
```

unless the actual cause belongs to those dimensions.

---

# Lifecycle Integration

Recommended:

```text
VALIDATED
    |
    v
Dependency Resolution
    |
    +--> RESOLVED
    |       |
    |       v
    |     LOADING
    |
    +--> UNRESOLVED
            |
            v
      remain non-active
```

---

# Loading Order

Plugins that have required runtime plugin dependencies SHOULD load/activate in dependency order.

However:

```text
resolved dependency
    !=
all plugins must load sequentially
```

Independent plugins MAY proceed concurrently.

---

# Activation Order

Activation ordering is more semantically important than raw binary load order.

Example:

```text
Plugin B capability
must be ACTIVE
before Plugin A starts using it
```

---

# Load Before Dependency Active

Some execution models MAY load a dependent plugin before its runtime dependency becomes ACTIVE, provided it cannot initialize/use that dependency prematurely.

MVP SHOULD prefer simpler dependency-aware ordering.

---

# Shutdown Order

Required dependents SHOULD normally stop before dependencies.

If:

```text
A requires B
```

then shutdown normally follows:

```text
A
    before
B
```

---

# Shutdown Graph

Shutdown order is generally the reverse of required activation ordering.

Optional dependencies MAY not require strict shutdown ordering.

---

# Runtime Dependency Availability

Dependency existence and runtime availability are separate.

Example:

```text
Registry:
    Plugin B installed

Runtime:
    Plugin B capability unavailable
```

An ACTIVE dependent may need degradation/recovery.

---

# Runtime Required Dependency Loss

When a required runtime dependency becomes unavailable, policy MAY choose:

```text
DEGRADE_CAPABILITY
QUIESCE_CAPABILITY
QUIESCE_PLUGIN
STOP_PLUGIN
WAIT_FOR_RECOVERY
REQUEST_REBIND
```

The dependency layer reports the condition.

Lifecycle/recovery policy performs the action.

---

# Runtime Optional Dependency Loss

Optional dependency loss SHOULD normally:

```text
degrade only affected optional functionality
```

not stop the entire plugin.

---

# Runtime Rebinding

A dynamic capability dependency MAY be rebound to another compatible provider.

Example:

```text
Capability X:
    Provider A unavailable
        ->
    Provider B
```

This is valid only if the dependency contract supports dynamic rebinding.

---

# Rebinding vs Business Routing

Dynamic dependency rebinding remains a plugin-runtime prerequisite concern.

It MUST NOT be confused with per-request AI/Recognition routing.

---

# Binding Lifetime

Possible binding lifetimes:

```text
ACTIVATION
RUNTIME_INSTANCE
OPERATION
DYNAMIC
```

Plugin dependencies SHOULD usually use:

```text
ACTIVATION
or
RUNTIME_INSTANCE
```

Per-operation business selection belongs elsewhere.

---

# Dependency Health

Dependency health is a runtime observation.

Resolver may consume current availability but MUST NOT own Health state.

---

# Dependency Revalidation

Revalidation MAY occur:

* before activation,
* before restart,
* after dependency change,
* after configuration change,
* after permission change,
* after plugin upgrade.

---

# Registry Change

A Registry mutation affecting a dependency MAY invalidate prior resolution.

Examples:

* dependency disabled,
* version removed,
* capability provider changed,
* permission revoked.

---

# Configuration Change

Dependency-relevant configuration MAY require resolution refresh.

Example:

```text
plugin switches from local runtime
to external provider mode
```

---

# Permission Dependency

If a plugin requires a Host Service whose permission is denied:

```text
required dependency unresolved
```

or a structured security-resolution failure occurs.

The plugin MUST NOT bypass the Host Service.

---

# Secret Reference Dependency

A plugin MAY require the presence of an approved credential reference.

Dependency resolution SHOULD verify availability/reference validity where policy permits.

It SHOULD NOT retrieve/expose raw secret values.

---

# Lazy Loading

Capability/provider candidates needed only for optional features MAY remain unloaded until required.

Dependency resolution SHOULD support lazy activation where safe.

---

# Lazy Dependency

A dependency MAY be declared:

```text
activationRequirement:
    LAZY
```

meaning plugin activation does not require it until a specific feature is invoked.

MVP MAY defer this complexity.

---

# Dependency Events

Recommended events:

```text
PluginDependenciesResolved
PluginDependencyResolutionFailed
PluginDependencyUnavailable
PluginDependencyRecovered
PluginDependencyRebound
PluginDependencyCycleDetected
PluginDependencyConflictDetected
```

---

# Event Boundary

These are plugin/runtime architecture events.

They are NOT automatically Domain Events.

---

# Dependency Diagnostics

Diagnostics SHOULD include:

```text
pluginId
pluginVersion
dependencyId
dependencyType
target
required
resolutionStatus
selectedBinding?
reasonCode
resolverVersion
```

---

# Sensitive Diagnostics

Dependency diagnostics MUST NOT contain:

* secrets,
* raw credentials,
* private configuration values.

---

# Failure Codes

Possible normalized failures:

```text
PLUGIN_DEPENDENCY_REQUIRED_MISSING
PLUGIN_DEPENDENCY_VERSION_CONFLICT
PLUGIN_DEPENDENCY_CAPABILITY_MISSING
PLUGIN_DEPENDENCY_CAPABILITY_VERSION_CONFLICT
PLUGIN_DEPENDENCY_HOST_SERVICE_MISSING
PLUGIN_DEPENDENCY_PLATFORM_UNSUPPORTED
PLUGIN_DEPENDENCY_RUNTIME_UNAVAILABLE
PLUGIN_DEPENDENCY_PERMISSION_DENIED
PLUGIN_DEPENDENCY_CYCLE
PLUGIN_DEPENDENCY_SELF_REFERENCE
PLUGIN_DEPENDENCY_BINDING_CONFLICT
PLUGIN_DEPENDENCY_RESOLUTION_STALE
PLUGIN_DEPENDENCY_INTERNAL_ERROR
```

---

# Failure Isolation

Dependency failure SHOULD prevent only affected plugin/capabilities from activation where possible.

It MUST NOT corrupt unrelated Registry state or active plugins.

---

# Resolution Cache

Dependency resolution MAY be cached.

Cache identity SHOULD include relevant:

```text
registry revision
capability provider revision
host-service revision
platform/runtime snapshot
resolver version
```

Stale resolution MUST NOT be reused indefinitely.

---

# Dependency Resolver Boundary

Recommended:

```text
Registry
    supplies declarations

Capability Index
    supplies providers

Host
    supplies services/environment

Dependency Resolver
    computes resolution

Lifecycle
    consumes result
```

---

# Architecture Invariants

1. Plugin dependencies MUST be explicit.

2. Required dependencies MUST be satisfied before activation.

3. Dependency resolution is separate from lifecycle execution.

4. Dependency Resolver MUST NOT load plugins.

5. Dependency Resolver MUST NOT initialize plugins.

6. Dependency Resolver MUST NOT execute business capability requests.

7. Capability dependencies SHOULD be preferred over concrete plugin dependencies.

8. Concrete plugin dependencies SHOULD be exceptional.

9. Host/runtime services SHOULD normally use Host Service dependencies rather than helper-plugin coupling.

10. `pluginId` dependency and `capabilityId` dependency are distinct.

11. Plugin version and capability-contract version are distinct.

12. Plugin API compatibility is Host compatibility, not an ordinary plugin dependency edge.

13. Missing required dependency produces unresolved lifecycle eligibility.

14. Missing required dependency MUST NOT automatically mark Registry state BLOCKED.

15. Optional dependency absence MUST NOT automatically prevent activation.

16. Optional dependency behavior SHOULD be explicit.

17. Dependency resolution SHOULD be deterministic for identical explicit inputs.

18. Stable tie-breaking SHOULD be used for equivalent dependency providers.

19. Dependency Resolver does not own AI Routing.

20. Dependency Resolver does not own Recognition/Translation business provider selection.

21. Dependency binding and per-operation routing are distinct concepts.

22. Dependency cycles among required activation dependencies are forbidden.

23. Indirect capability-binding cycles MUST be detectable.

24. Self-dependency is forbidden where it creates an activation prerequisite cycle.

25. Version constraints MUST be checked before activation.

26. Capability-version constraints MUST be checked before activation.

27. Dependency conflicts MUST fail explicitly.

28. Resolution result SHOULD be traceable/versioned.

29. Dynamic dependency state may require revalidation.

30. Registry declaration does not imply runtime dependency availability.

31. Required runtime dependency loss requires explicit recovery/lifecycle policy.

32. Optional runtime dependency loss SHOULD normally degrade only optional functionality.

33. Dynamic rebinding is allowed only when declared contract permits it.

34. Rebinding MUST preserve compatibility constraints.

35. Activation order SHOULD respect required dependency edges.

36. Independent plugins MAY initialize concurrently.

37. Shutdown order SHOULD normally reverse required dependency ordering.

38. Resolver does not own Health state.

39. Resolver MAY consume Health/availability projections.

40. Permission denial may make a required Host Service dependency unresolved.

41. Dependency resolution MUST NOT expose raw secrets.

42. Dependency diagnostics MUST avoid sensitive configuration.

43. Dependency events are not automatically Domain Events.

44. Resolution cache MUST be freshness-aware.

45. Dependency failure SHOULD remain isolated to affected plugins/capabilities.

46. Plugin restart SHOULD revalidate required dynamic dependencies.

47. Plugin upgrade requires dependency resolution against the new version.

48. Registry changes MAY invalidate prior dependency resolution.

49. Configuration/permission changes MAY invalidate dependency resolution.

50. Dependency architecture MUST NOT become a universal service locator.

51. Capability binding SHOULD use public contracts.

52. Private plugin APIs MUST NOT be used to satisfy generic capability dependencies.

53. Built-in providers MAY satisfy capability dependencies.

54. Plugin providers MAY satisfy capability dependencies.

55. New dependency types SHOULD integrate through normalized resolver contracts.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* required plugin dependency,
* optional plugin dependency,
* required capability dependency,
* optional capability dependency,
* required Host Service dependency,
* plugin version ranges,
* capability-contract version ranges,
* deterministic dependency resolution,
* capability-first resolution,
* cycle detection,
* version conflict detection,
* explicit `DependencyResolution`,
* `RESOLVED` / `UNRESOLVED`,
* activation ordering,
* reverse shutdown ordering,
* optional dependency warnings,
* basic runtime dependency-loss signal,
* resolution diagnostics,
* normalized dependency errors.

MVP SHOULD prefer:

```text
Host Services
+
Capability Dependencies
```

over extensive concrete plugin-to-plugin dependency graphs.

MVP MAY defer:

* multiple simultaneous plugin versions,
* sophisticated SAT dependency solving,
* lazy dependencies,
* dynamic rebinding,
* runtime capability-provider migration,
* optional cyclic dependency handling,
* distributed dependency resolution,
* remote dependency providers,
* complex platform feature expressions.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact dependency descriptor schema,
* version-range syntax,
* capability-version range syntax,
* dependency ID scheme,
* binding-policy taxonomy,
* fixed vs dynamic capability binding,
* whether `RESOLVED` is persisted,
* resolution TTL/freshness,
* exact capability-provider eligibility inputs,
* dependency resolver ownership component,
* capability-provider snapshot representation,
* Host Service dependency catalog,
* platform/runtime requirement schema,
* permission dependency representation,
* credential-reference availability checks,
* activation ordering algorithm,
* startup parallelism,
* shutdown ordering algorithm,
* optional-edge treatment,
* cycle detection across capability bindings,
* version-conflict strategy,
* multiple-version isolation support,
* runtime dependency-loss handling,
* dependency rebinding,
* dependency event persistence,
* resolution cache.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_SYSTEM.md`
* `PLUGIN_API.md`
* `PLUGIN_REGISTRY.md`
* `PLUGIN_DISCOVERY.md`
* `PLUGIN_LIFECYCLE.md`
* `PLUGIN_CONFIGURATION.md`
* `PLUGIN_SECURITY.md`
* `PLUGIN_VERSIONING.md`

Architecture:

* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`

AI:

* `../ai/ROUTING.md`
* `../ai/MODELS.md`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/telemetry/`

Runtime:

* `../runtime/RESOURCE_LIFECYCLE.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RUNTIME_COMPONENTS.md`
