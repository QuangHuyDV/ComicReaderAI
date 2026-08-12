# Plugin Versioning

* **Document:** Plugin Architecture / Plugin Versioning
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI plugin versions, public contract versions, compatibility rules, upgrades, migrations, deprecations and rollbacks are represented and evaluated.

Versioning exists to allow:

* independent plugin evolution,
* stable public Plugin APIs,
* independently versioned capability contracts,
* safe configuration evolution,
* deterministic dependency resolution,
* compatible upgrades,
* explicit breaking changes,
* safe rollback where supported.

Versioning MUST distinguish implementation release identity from public contract identity.

---

# Core Principle

```text
Plugin Artifact
    |
    v
Plugin Version
    +
Plugin API Compatibility
    +
Capability Contract Compatibility
    +
Configuration Schema Compatibility
    +
Dependency Compatibility
    +
Platform / Runtime Compatibility
        |
        v
Compatibility Evaluation
        |
        +--> COMPATIBLE
        |
        +--> INCOMPATIBLE
```

No single version number represents all compatibility semantics.

---

# Scope

Plugin Versioning covers:

* plugin release version,
* Plugin API version/range,
* capability contract versions/ranges,
* configuration schema versions,
* dependency version ranges,
* CRAI application compatibility hints,
* platform/runtime compatibility,
* upgrade compatibility,
* migration compatibility,
* deprecation,
* rollback,
* release channels,
* compatibility diagnostics.

---

# Non-Goals

Versioning does NOT own:

* plugin installation,
* plugin loading,
* lifecycle execution,
* dependency-resolution algorithm,
* configuration persistence,
* Security trust,
* package download,
* Registry persistence,
* Marketplace implementation.

---

# Version Dimensions

CRAI SHOULD distinguish at least:

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

Provider-native API versions MAY also exist but remain external/provider-specific.

---

# Plugin Version

`pluginVersion` identifies one release of one plugin implementation.

Recommended:

```text
pluginId:
    plugin.example

pluginVersion:
    2.3.1
```

---

# Plugin Version Semantics

Plugin Version MAY follow Semantic Versioning:

```text
MAJOR.MINOR.PATCH
```

Typical interpretation:

```text
MAJOR
    materially incompatible plugin release

MINOR
    backward-compatible plugin feature release

PATCH
    backward-compatible fixes
```

But Plugin Version alone MUST NOT define compatibility with Host public contracts.

---

# Plugin Version vs Plugin API Version

Critical distinction:

```text
pluginVersion
    = implementation release
```

```text
pluginApiVersion
    = Host/plugin base public contract
```

Example:

```text
Plugin Foo 2.3.1
    supports Plugin API >=2.0 <3.0
```

---

# Plugin Version MAJOR Does Not Automatically Mean Plugin API Break

A plugin MAY release:

```text
2.0.0
```

because of:

* internal redesign,
* configuration changes,
* removed plugin-specific feature,

while still supporting:

```text
Plugin API v2
```

Likewise a new Plugin API major does not force every plugin to increment its own major in the same pattern.

---

# Plugin API Version

Plugin API version identifies the generic Host/plugin contract defined by `PLUGIN_API.md`.

Examples of affected concepts:

* base Plugin contract,
* lifecycle hooks,
* Host Service injection semantics,
* capability registration protocol,
* error/cancellation conventions.

---

# Plugin API Range

Plugins SHOULD declare a supported Plugin API range.

Example conceptually:

```text
pluginApi:
    >=2.0 <3.0
```

Exact syntax MAY reuse the common version-range grammar.

---

# Plugin API Compatibility

Before activation, CRAI MUST select a mutually compatible Plugin API version.

If no supported version exists:

```text
INCOMPATIBLE_API
```

---

# Plugin API Version Negotiation

Where multiple compatible versions exist:

```text
Host supported:
    2.0
    2.1

Plugin supported:
    >=2.0 <3.0
```

Host SHOULD deterministically select the highest compatible stable contract unless policy specifies otherwise.

---

# Capability Contract Version

Each public extension capability is versioned independently.

Examples:

```text
recognition.engine/v1
recognition.engine/v2

translation.execution/v1
ai.execution/v2
```

---

# Why Capability Version Is Separate

Changing:

```text
RecognitionEngine request/response contract
```

does not necessarily require changing:

```text
Base Plugin API
```

Therefore:

```text
CapabilityContractVersion
    !=
PluginApiVersion
```

---

# Capability Declaration

A plugin MAY declare:

```text
capabilities:
    recognition.engine:
        supportedVersions:
            >=1 <3
```

Exact descriptor syntax remains open.

---

# Multi-Version Capability Support

A plugin MAY support several capability-contract versions simultaneously.

Example:

```text
recognition.engine/v1
recognition.engine/v2
```

This can ease migration.

---

# Capability Compatibility

A plugin is eligible only if every required capability contract used by the Host can be negotiated to a compatible version.

---

# Configuration Schema Version

Plugin configuration schema has its own version.

Example:

```text
configSchemaVersion:
    3
```

This describes:

```text
configuration structure and semantics
```

not:

```text
concrete configuration revision
```

---

# Configuration Schema vs Configuration Revision

Critical distinction:

```text
schemaVersion
    = structure/meaning
```

```text
configurationRevision
    = one concrete state
```

---

# Configuration Schema Compatibility

Plugin upgrade MAY support:

```text
config schema v2
    -> v3
```

through migration.

Unsupported schema transition MUST prevent unsafe activation.

---

# Configuration Migration

Recommended:

```text
ConfigurationMigration
├── fromSchemaVersion
├── toSchemaVersion
├── migrationId
├── reversible
├── dataLossRisk?
└── migratorVersion
```

---

# Migration Is Not Automatic Compatibility

A plugin supporting schema v3 does NOT imply it can safely consume schema v2.

It must either:

* support v2 directly,
* provide migration,
* reject activation.

---

# Dependency Versions

Plugin dependencies MAY specify:

```text
exact version
minimum version
maximum version
version range
```

Capability dependencies specify capability-contract version ranges rather than plugin implementation versions where possible.

---

# Dependency Example

Preferred:

```text
requiresCapability:
    recognition.language-hint >=1 <2
```

rather than:

```text
requiresPlugin:
    ocr.helper ^3.0.0
```

unless concrete plugin coupling is intentional.

---

# Version Range

CRAI SHOULD standardize one canonical version-range grammar.

Possible expressions:

```text
=1.2.3
>=1.2.0
>=1.2 <2.0
^2.0.0
```

Exact syntax remains open.

---

# Deterministic Range Evaluation

Given:

* version,
* version range,
* range parser version,

compatibility evaluation MUST be deterministic.

---

# Pre-Release Versions

Pre-release identifiers MAY follow SemVer:

```text
2.0.0-alpha.1
2.0.0-beta.2
2.0.0-rc.1
```

Pre-release versions SHOULD NOT satisfy stable ranges by default unless explicitly allowed.

---

# Build Metadata

Build metadata MAY be retained:

```text
2.0.0+build.42
```

It SHOULD NOT affect SemVer compatibility ordering.

---

# CRAI Application Version

A plugin MAY declare:

```text
supportedCRAIRange
```

Example:

```text
>=1.5 <2.0
```

This is useful for:

* packaging,
* Marketplace filtering,
* coarse compatibility,
* application-level feature assumptions.

---

# CRAI Version Is Not the Primary Contract

Critical rule:

```text
Supported CRAI Version
    !=
complete compatibility proof
```

Exact Plugin API/capability/platform/dependency checks remain authoritative.

---

# Compatibility Dimensions

Recommended compatibility evaluation includes:

```text
Plugin API
Capability Contracts
Plugin Dependencies
Host Services
Configuration Schema
Platform
Architecture
Runtime
Security Requirements
CRAI Application Range
```

Not every plugin uses every dimension.

---

# Compatibility Result

Recommended:

```text
PluginCompatibilityResult
├── pluginId
├── pluginVersion
├── status
├── negotiatedPluginApiVersion?
├── capabilityNegotiations[]
├── dependencyCompatibility[]
├── configurationCompatibility?
├── platformCompatibility
├── runtimeCompatibility
├── craiRangeCompatibility?
├── warnings[]
├── failures[]
├── evaluatorVersion
└── evaluatedAt
```

---

# Compatibility Status

Possible:

```text
COMPATIBLE
INCOMPATIBLE_API
INCOMPATIBLE_CAPABILITY
INCOMPATIBLE_DEPENDENCY
INCOMPATIBLE_CONFIGURATION
INCOMPATIBLE_PLATFORM
INCOMPATIBLE_RUNTIME
INCOMPATIBLE_CRAI_VERSION
INCOMPATIBLE_SECURITY_REQUIREMENTS
UNKNOWN
```

---

# Compatibility Evaluation Boundary

Compatibility Evaluator computes compatibility.

Registry stores the result/projection.

Plugin Lifecycle consumes it.

Plugin Manager coordinates lifecycle based on it.

---

# Before Loading

Required compatibility SHOULD be evaluated before implementation loading where static metadata allows.

This preserves the existing architectural principle:

```text
incompatible plugin
    MUST NOT run
```

---

# Runtime Compatibility Check

Some compatibility conditions can only be confirmed at runtime.

Examples:

* native library availability,
* GPU runtime,
* local model runtime.

These MAY be checked during dependency resolution/initialization.

Static incompatibility MUST still fail earlier where possible.

---

# Platform Compatibility

Plugins MAY declare:

```text
operating system
CPU architecture
runtime ABI
GPU/runtime requirements
```

Example:

```text
linux-x86_64
windows-x64
arm64
```

---

# Host Service Version

Host Services SHOULD also be versioned when their public contract evolves.

Example:

```text
network-service/v1
storage-service/v2
```

A plugin's Host Service dependency may specify a compatible contract version.

---

# Version Resolution vs Dependency Resolution

Versioning defines:

```text
compatibility rules
```

Dependency Resolver uses them to determine:

```text
which dependencies can satisfy requirements
```

The two concerns remain separate.

---

# Upgrade

Plugin upgrade means replacing one installed plugin release with another.

Example:

```text
plugin.foo 1.5.0
    ->
plugin.foo 2.0.0
```

Upgrade is distinct from:

```text
restart
reload
configuration update
```

---

# Upgrade Preconditions

Before an upgrade becomes active, CRAI SHOULD verify:

```text
new artifact integrity
new plugin identity/version
Plugin API compatibility
capability compatibility
dependency compatibility
configuration compatibility
permissions/trust
platform/runtime compatibility
```

---

# Upgrade Flow

Recommended:

```text
Current Version
      |
      v
Discover / Install Candidate
      |
      v
Validate Artifact
      |
      v
Evaluate Compatibility
      |
      v
Evaluate Security / Permissions
      |
      v
Prepare Configuration Migration
      |
      v
Quiesce Current Runtime
      |
      v
Activate New Runtime Instance
      |
      +--> Success
      |
      +--> Failure -> Rollback Policy
```

Exact package-install mechanics remain separate.

---

# Upgrade Does Not Reuse Runtime Instance

Upgrade MUST create:

```text
new plugin runtime instance
```

The old runtime instance MUST NOT mutate itself into the new plugin version.

---

# Upgrade Identity

After successful upgrade:

```text
pluginId
    same

pluginVersion
    new

runtimeInstanceId
    new
```

---

# Side-by-Side Validation

Where platform/runtime permits, CRAI MAY validate/load the new version before quiescing the old version.

MVP MAY defer zero-downtime replacement.

---

# Configuration Migration

Upgrade MAY require configuration migration.

Migration SHOULD occur against a copied/candidate configuration state before it becomes active.

---

# Data Migration

Plugins may also own private persistent data requiring migration.

This is distinct from configuration migration.

---

# Plugin-Private Data Migration

Recommended:

```text
PluginDataMigration
├── fromDataVersion
├── toDataVersion
├── migrationId
├── reversible
└── migrationToolVersion
```

Plugin-private data migration MUST NOT modify CRAI canonical domain schemas directly.

---

# Canonical Domain Data

A plugin upgrade MUST NOT define migrations for canonical CRAI domain truth unless the owning module explicitly delegates that extension contract.

---

# Permission Re-Evaluation

Upgrade MUST re-evaluate requested permissions.

If new version asks for:

```text
additional sensitive permission
```

existing approval MUST NOT automatically imply approval.

---

# Trust Re-Evaluation

Artifact identity changes on upgrade.

Trust/integrity SHOULD therefore be reevaluated.

---

# Upgrade Success

Upgrade is complete only when the new runtime instance reaches required lifecycle readiness, typically:

```text
ACTIVE
```

and applicable migrations are committed.

---

# Upgrade Failure

If candidate upgrade fails before destructive commit:

```text
retain current version
```

where possible.

---

# Rollback

Rollback reactivates a previously compatible plugin release after a failed or rejected upgrade.

---

# Rollback Does Not Restore Runtime Object State

Critical rule:

```text
Rollback
    !=
restore old in-memory runtime instance
```

Preferred:

```text
create new runtime instance
of previous plugin version
```

---

# Rollback Scope

Rollback MAY restore:

* previous plugin artifact/version,
* previous compatible configuration revision,
* previous plugin-private data version,

when reversible.

It MUST NOT claim to restore transient runtime state automatically.

---

# Runtime State

Examples of runtime state that normally SHOULD NOT be restored blindly:

```text
open network connections
active requests
worker thread state
in-memory caches
provider session objects
```

---

# Rollback Preconditions

Rollback requires:

* previous artifact still available,
* previous config/data compatible,
* dependencies still satisfiable,
* permissions/trust still valid,
* security policy allows it.

---

# Migration Rollback

Migration must declare whether it is reversible.

If:

```text
reversible = false
```

automatic rollback to an older version MAY be unsafe or impossible.

---

# Forward-Only Migration

Some plugin-private data migrations MAY be:

```text
FORWARD_ONLY
```

In that case the upgrade system MUST warn/block rollback before destructive migration.

---

# Rollback Result

Recommended:

```text
PluginRollbackResult
├── fromVersion
├── toVersion
├── configurationRevision?
├── dataVersion?
├── success
├── warnings[]
└── failure?
```

---

# Upgrade Coordination Ownership

Plugin Manager coordinates runtime lifecycle transitions.

Installer/Package Manager owns artifact installation/removal.

Versioning owns compatibility rules.

Configuration owns config migration semantics.

Security owns trust/permission reevaluation.

No single component owns the entire upgrade process.

---

# Deprecation

Public Plugin API and Capability Contracts MAY be deprecated.

Deprecation SHOULD include:

```text
deprecatedSince
replacement?
removalTarget?
migrationGuidance
```

---

# Deprecated Contract

Deprecated contracts SHOULD:

* remain functional during a defined transition period where feasible,
* emit diagnostics,
* have documented replacement.

---

# Removal

Breaking removal SHOULD occur only in an explicit incompatible contract version.

Example:

```text
Plugin API v2
    deprecated feature

Plugin API v3
    feature removed
```

---

# Capability Deprecation

Capability contracts have their own deprecation lifecycle.

A capability may be deprecated independently from Base Plugin API.

---

# Plugin Release Deprecation

An individual plugin version MAY also be marked:

```text
DEPRECATED
UNSUPPORTED
REVOKED
```

through repository/security metadata.

This is separate from API deprecation.

---

# Release Channels

Possible release channels:

```text
STABLE
BETA
ALPHA
DEVELOPMENT
ENTERPRISE
CUSTOM
```

Release channel MAY influence update policy.

It MUST NOT override compatibility/security checks.

---

# Update Policy

Possible update modes:

```text
MANUAL
NOTIFY
AUTO_PATCH
AUTO_MINOR
AUTO_COMPATIBLE
ADMIN_MANAGED
```

MVP MAY prefer manual/admin-controlled updates.

---

# Automatic Upgrade

Automatic upgrade MUST still perform:

* artifact validation,
* compatibility,
* migration validation,
* permission/trust checks.

There is no:

```text
auto-update
    -> bypass validation
```

path.

---

# Marketplace / Repository Metadata

Future repositories MAY publish:

```text
pluginId
pluginVersion
Plugin API range
capability contract versions
CRAI application range
platforms
release channel
artifact hash
signature
dependencies
configuration schema version
deprecation status
```

---

# Repository Metadata Is Advisory Until Verified

Repository metadata MUST be validated against the downloaded artifact/manifest.

---

# Install Compatibility

Compatibility MAY be evaluated:

```text
before download
before install
before activation
```

at different confidence levels.

Final activation requires Host-local validation.

---

# Version Conflict

Version conflict occurs when no compatible set satisfies required constraints.

Possible:

```text
PLUGIN_VERSION_CONFLICT
CAPABILITY_VERSION_CONFLICT
DEPENDENCY_VERSION_CONFLICT
HOST_SERVICE_VERSION_CONFLICT
CONFIGURATION_SCHEMA_CONFLICT
```

---

# Conflict Does Not Mean Plugin Blocked

A version conflict normally produces:

```text
INCOMPATIBLE / UNRESOLVED
```

not:

```text
BLOCKED
```

unless Security/Admin policy also blocks the plugin.

---

# Compatibility Cache

Compatibility results MAY be cached.

Cache identity SHOULD include:

```text
plugin artifact identity
Host Plugin API versions
capability contract versions
platform/runtime snapshot
dependency snapshot
configuration schema
evaluator version
```

---

# Compatibility Cache Invalidation

Re-evaluate when:

* plugin artifact changes,
* Host API changes,
* capability contract support changes,
* dependency registry changes,
* platform/runtime changes,
* configuration schema changes,
* Security requirement changes.

---

# Compatibility Provenance

Registry/diagnostics SHOULD preserve enough data to explain:

```text
why plugin version X is or is not compatible
```

---

# Versioning Events

Recommended events:

```text
PluginCompatibilityEvaluated
PluginVersionRegistered
PluginUpgradeRequested
PluginUpgradeStarted
PluginUpgradeCompleted
PluginUpgradeFailed
PluginRollbackRequested
PluginRollbackStarted
PluginRollbackCompleted
PluginRollbackFailed
PluginContractDeprecated
PluginVersionConflictDetected
```

---

# Event Boundary

Versioning events are platform/runtime events.

They are NOT Domain Events.

---

# Audit

Material actions MAY require Audit:

* plugin upgraded,
* plugin downgraded,
* rollback performed,
* incompatible version manually forced/attempted,
* migration performed,
* release channel changed.

---

# Failure Categories

Possible normalized failures:

```text
PLUGIN_VERSION_INVALID
PLUGIN_VERSION_RANGE_INVALID
PLUGIN_API_VERSION_UNSUPPORTED
PLUGIN_CAPABILITY_VERSION_UNSUPPORTED
PLUGIN_CRAI_VERSION_UNSUPPORTED
PLUGIN_PLATFORM_VERSION_UNSUPPORTED
PLUGIN_RUNTIME_VERSION_UNSUPPORTED
PLUGIN_DEPENDENCY_VERSION_CONFLICT
PLUGIN_HOST_SERVICE_VERSION_UNSUPPORTED
PLUGIN_CONFIGURATION_SCHEMA_INCOMPATIBLE
PLUGIN_UPGRADE_VALIDATION_FAILED
PLUGIN_UPGRADE_MIGRATION_FAILED
PLUGIN_UPGRADE_ACTIVATION_FAILED
PLUGIN_ROLLBACK_UNAVAILABLE
PLUGIN_ROLLBACK_MIGRATION_IRREVERSIBLE
PLUGIN_ROLLBACK_ACTIVATION_FAILED
PLUGIN_VERSION_NEGOTIATION_FAILED
```

---

# Determinism

For identical:

* artifact metadata,
* Host-supported contracts,
* dependency state,
* configuration schema,
* platform/runtime state,
* evaluator version,

compatibility evaluation SHOULD be deterministic.

---

# Architecture Invariants

1. Every plugin artifact declares a plugin version.

2. Plugin Version is distinct from Plugin API Version.

3. Plugin API Version is distinct from Capability Contract Version.

4. Capability Contract Version is distinct from Configuration Schema Version.

5. Configuration Schema Version is distinct from Configuration Revision.

6. CRAI application version is not sufficient by itself to prove plugin compatibility.

7. Public contract compatibility SHOULD be evaluated explicitly.

8. Plugin Version MAY use SemVer.

9. Plugin major version does not automatically define Plugin API compatibility.

10. Breaking Base Plugin API changes require explicit Plugin API version evolution.

11. Breaking capability-contract changes require capability-version evolution.

12. Capability contracts MAY evolve independently from Base Plugin API.

13. Plugins MAY support multiple compatible capability versions.

14. Version ranges MUST be parsed/evaluated deterministically.

15. Pre-release compatibility MUST be explicit.

16. Build metadata MUST NOT alter SemVer compatibility ordering.

17. Compatibility SHOULD be checked before plugin code runs where possible.

18. Static incompatibility MUST prevent activation.

19. Some runtime compatibility checks MAY occur later during dependency resolution/initialization.

20. Dependency resolution consumes Versioning rules.

21. Versioning does not own dependency-resolution algorithm.

22. Compatibility state is separate from Registry enablement/block state.

23. Version conflict MUST NOT automatically mark a plugin BLOCKED.

24. Plugin upgrade is distinct from restart.

25. Plugin upgrade creates a new runtime instance.

26. Runtime instance identity MUST change across upgrade.

27. Plugin Manager does not own the entire upgrade process.

28. Installer owns artifact installation.

29. Security owns trust/permission reevaluation.

30. Configuration owns configuration migration semantics.

31. Plugin-private data migration is distinct from configuration migration.

32. Plugin upgrade MUST NOT mutate canonical CRAI domain data without owning-module contract.

33. Upgrade MUST NOT bypass compatibility validation.

34. Upgrade MUST NOT bypass Security evaluation.

35. New permissions requested by an upgrade MUST NOT be granted silently.

36. Artifact trust/integrity SHOULD be reevaluated on upgrade.

37. Rollback creates a new runtime instance of the previous version.

38. Rollback MUST NOT claim to restore disposed in-memory runtime state.

39. Rollback requires previous artifact/config/data compatibility.

40. Irreversible migration may prevent rollback.

41. Migration reversibility MUST be explicit where rollback is promised.

42. Deprecated contracts SHOULD provide replacement guidance.

43. Breaking contract removal requires explicit incompatible version evolution.

44. Release channel does not bypass compatibility/security.

45. Automatic updates still require validation.

46. Repository metadata MUST be verified against local artifact metadata.

47. Final activation compatibility MUST be evaluated locally.

48. Compatibility failures SHOULD use normalized error codes.

49. Compatibility results SHOULD preserve provenance.

50. Compatibility cache MUST be freshness/version aware.

51. Versioning events are not Domain Events.

52. Material upgrade/rollback actions MAY require Audit.

53. New plugin versions SHOULD remain independently releasable from CRAI Core where public contracts permit it.

54. New capability contract versions SHOULD not require unrelated plugin API breaking changes.

55. Removing a plugin version MUST NOT erase canonical CRAI domain truth.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* SemVer plugin versions,
* Plugin API major/minor versioning,
* Plugin API compatible ranges,
* capability contract versions,
* capability version ranges,
* configuration schema version,
* configuration migration declaration,
* dependency version ranges,
* CRAI application compatibility hint,
* platform compatibility,
* deterministic compatibility evaluation,
* version conflict detection,
* manual plugin upgrade,
* config compatibility check,
* permission/trust reevaluation,
* rollback to previous artifact where safe,
* deprecation metadata,
* version diagnostics,
* versioning events.

MVP SHOULD NOT assume:

* CRAI application version alone determines compatibility,
* plugin MAJOR alone determines public API compatibility,
* rollback can restore in-memory runtime state,
* every upgrade is automatically reversible.

MVP MAY defer:

* automatic plugin updates,
* multiple simultaneously active versions,
* zero-downtime upgrades,
* side-by-side activation,
* complex private-data migrations,
* automatic rollback,
* repository release channels,
* pre-release auto-selection,
* generated compatibility matrices,
* SAT-style multi-version resolution.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* canonical SemVer library/rules,
* version-range syntax,
* Plugin API version format,
* capability-contract version format,
* configuration schema version format,
* Host Service versioning,
* CRAI compatibility-range semantics,
* pre-release selection rules,
* compatibility evaluator ownership component,
* negotiated Plugin API algorithm,
* negotiated capability-version algorithm,
* compatibility-result persistence,
* compatibility cache,
* upgrade orchestration owner,
* package installer integration,
* configuration migration API,
* plugin-private data migration API,
* migration rollback semantics,
* rollback artifact retention,
* automatic rollback policy,
* release-channel model,
* plugin update policy,
* repository metadata contract,
* deprecation transition period,
* unsupported-version policy,
* compatibility diagnostics schema.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_SYSTEM.md`
* `PLUGIN_API.md`
* `PLUGIN_REGISTRY.md`
* `PLUGIN_DISCOVERY.md`
* `PLUGIN_LIFECYCLE.md`
* `PLUGIN_DEPENDENCY.md`
* `PLUGIN_CONFIGURATION.md`
* `PLUGIN_SECURITY.md`

Architecture:

* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/telemetry/`

Runtime:

* `../runtime/RESOURCE_LIFECYCLE.md`
* `../runtime/RUNTIME_COMPONENTS.md`
