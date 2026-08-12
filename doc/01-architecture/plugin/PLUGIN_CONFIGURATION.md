# Plugin Configuration

* **Document:** Plugin Architecture / Plugin Configuration
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how plugin-specific configuration is declared, resolved, validated, versioned, updated and delivered to CRAI plugin implementations.

Plugin Configuration provides a controlled boundary between:

```text
CRAI Configuration / Governance
        |
        v
Plugin-Specific Configuration
        |
        v
Resolved Plugin Configuration View
        |
        v
Plugin Runtime
```

The system allows plugins to receive only configuration relevant to their implementation responsibilities.

Plugin Configuration MUST NOT become a duplicate configuration system for unrelated CRAI concerns.

---

# Core Principle

```text
Configuration Sources
        |
        v
Configuration Resolution
        |
        v
Validation
        |
        v
Immutable Resolved View
        |
        v
Plugin Runtime
```

Plugins consume resolved configuration.

They do NOT discover or merge CRAI configuration themselves.

---

# Scope

Plugin Configuration covers:

* plugin-specific configuration schema,
* configuration namespaces,
* configuration scopes,
* default values,
* configuration resolution,
* overrides,
* immutable resolved views,
* validation,
* update classification,
* hot reload,
* restart-required changes,
* configuration references,
* persistence semantics,
* secret/credential references,
* configuration events,
* configuration diagnostics.

---

# Non-Goals

Plugin Configuration does NOT own:

* Workspace Policy,
* Project business settings,
* Profile semantics,
* AI Routing Policy,
* Retry Policy,
* Fallback Policy,
* Cache Policy,
* Provider credentials,
* Provider Configuration,
* Feature ownership outside plugin implementation,
* Storage technology,
* runtime scheduling.

---

# Design Principles

Plugin configuration SHOULD be:

* external to plugin implementation,
* scoped,
* explicit,
* versioned,
* schema-driven,
* immutable during one operation,
* least-privilege,
* secret-safe,
* independently persistent,
* reload-aware,
* traceable,
* implementation-independent.

---

# Configuration Ownership

Critical distinction:

```text
CRAI Canonical Configuration
    = owned by its architecture/module
```

```text
Plugin Configuration
    = implementation-specific settings
```

A plugin MUST NOT duplicate canonical CRAI configuration merely for convenience.

---

# Valid Plugin Configuration

Examples of legitimate plugin-specific configuration MAY include:

```text
implementation mode
plugin-local endpoint option
batch size
worker count
local cache size
implementation feature toggle
provider adapter tuning
temporary file limits
plugin-specific parser options
```

Only values owned by the plugin implementation belong here.

---

# Invalid Ownership Examples

Avoid placing these into opaque plugin configuration when they are already owned elsewhere:

```text
Retry Policy
AI Routing Policy
Model selection strategy
Workspace Language settings
Translation Profile
Global Cache Policy
Workspace budget
Safety Policy
Provider credentials
```

---

# Configuration Namespace

Each plugin MUST have an isolated configuration namespace.

Conceptually:

```text
pluginId
    |
    v
Plugin Configuration Namespace
```

Example:

```text
plugin.example.recognition
```

Namespace identity SHOULD use `pluginId`, not display name or filesystem path.

---

# Configuration Namespace Isolation

A plugin MUST NOT:

* mutate another plugin's namespace,
* read another plugin's private configuration,
* infer unrelated configuration through shared mutable objects.

---

# Configuration Schema

Each configurable plugin SHOULD declare a versioned schema.

Recommended:

```text
PluginConfigurationSchema
├── pluginId
├── schemaVersion
├── fields[]
├── defaults
├── validationRules[]
├── updateSemantics[]
└── compatibilityMetadata
```

---

# Configuration Field

Recommended:

```text
PluginConfigurationField
├── fieldId
├── valueType
├── required
├── defaultValue?
├── constraints?
├── sensitivity
├── updateMode
├── allowedScopes[]
└── description?
```

---

# Sensitivity

Possible sensitivity classes:

```text
PUBLIC
INTERNAL
SENSITIVE
SECRET_REFERENCE
```

Raw secret values SHOULD NOT normally use Plugin Configuration fields.

---

# Update Mode

Each field SHOULD declare one of:

```text
HOT_RELOADABLE
RESTART_REQUIRED
RELOAD_REQUIRED
IMMUTABLE_AFTER_INSTALL
READ_ONLY_DERIVED
```

This prevents ad-hoc runtime behavior.

---

# Configuration Sources

Plugin configuration MAY derive from several sources.

Possible sources:

```text
PLUGIN_DEFAULT
SYSTEM
WORKSPACE
PROJECT
PRINCIPAL
SESSION
OPERATION_OVERRIDE
ADMIN_OVERRIDE
```

Not every plugin or field supports every scope.

---

# Scope Model

Recommended:

```text
PluginConfigurationScope
├── scopeType
├── scopeId?
├── priority
└── persistenceClass
```

---

# Scope Is Explicit

A plugin configuration field SHOULD declare which scopes may override it.

Example:

```text
workerCount:
    SYSTEM
    WORKSPACE

debugLogging:
    SYSTEM
    PRINCIPAL

operationTimeoutHint:
    OPERATION_OVERRIDE
```

---

# No Universal Override Stack

CRAI SHOULD NOT assume every plugin always uses:

```text
Runtime
> User
> System
> Default
```

Instead:

```text
field schema
+
allowed scopes
+
configuration resolution policy
```

determine effective precedence.

---

# Default Resolution Order

Where no special policy exists, CRAI MAY use a default order such as:

```text
Operation Override
    >
Principal
    >
Project
    >
Workspace
    >
System
    >
Plugin Default
```

but this MUST remain configurable and field-aware.

---

# Administrative Override

Admin override MAY outrank ordinary scopes when policy explicitly allows it.

It MUST NOT silently bypass Security/Policy constraints.

---

# Configuration Resolution

Recommended flow:

```text
Plugin Schema
    +
Available Configuration Layers
        |
        v
Scope Filtering
        |
        v
Precedence Resolution
        |
        v
Validation
        |
        v
Resolved Plugin Configuration
```

---

# Resolved Configuration

Recommended:

```text
ResolvedPluginConfiguration
├── pluginId
├── pluginVersion?
├── schemaVersion
├── configurationRevision
├── values
├── sourceMap
├── contentHash
├── createdAt
└── scopeContext
```

---

# Source Map

`sourceMap` records where effective values came from.

Example:

```text
workerCount
    -> WORKSPACE

debugLogging
    -> PRINCIPAL

batchSize
    -> PLUGIN_DEFAULT
```

This improves diagnostics.

---

# Immutable Runtime View

A plugin receives:

```text
PluginConfigurationView
```

representing one resolved revision.

For the duration of an operation:

```text
configuration view MUST NOT mutate
```

---

# Operation Configuration Stability

Critical rule:

```text
Operation starts with Configuration Revision A
```

A concurrent configuration update to Revision B MUST NOT silently change that operation halfway through.

Future operations may use B.

---

# Runtime Plugin Configuration

A long-lived plugin runtime MAY receive configuration revision changes between operations.

Hot reload behavior depends on field update semantics.

---

# Configuration Revision

Every resolved or persisted material configuration change SHOULD produce a new:

```text
configurationRevision
```

or equivalent deterministic identity.

---

# Content Hash

A resolved configuration MAY expose:

```text
configurationHash
```

for:

* diagnostics,
* caching,
* provenance,
* operation reproducibility.

Sensitive raw values MUST NOT be reconstructable trivially from exposed hashes where that creates risk.

---

# Plugin Defaults

Plugin default values MAY be declared in the configuration schema or descriptor.

Defaults MUST be static/declarative.

Plugins SHOULD NOT execute arbitrary code to generate configuration defaults during Discovery.

---

# Dynamic Defaults

If a default depends on runtime state:

```text
READ_ONLY_DERIVED
```

or a separate Host-provided runtime value SHOULD be used.

Avoid hiding runtime discovery inside configuration resolution.

---

# Configuration Validation

Validation SHOULD include:

* schema validation,
* required fields,
* data types,
* value ranges,
* enum constraints,
* cross-field constraints,
* update-mode rules,
* allowed scope,
* compatibility constraints,
* reference validity where applicable.

---

# Validation Ownership

Plugin Configuration architecture validates generic schema semantics.

A plugin MAY provide implementation-specific validation through an explicit public validation contract.

The plugin MUST NOT receive invalid resolved configuration for normal activation.

---

# Plugin-Specific Validation

Example:

```text
local model path exists
batch size supported
endpoint mode compatible
```

MAY require plugin-specific validation.

This validation SHOULD remain side-effect-minimal.

---

# Provider Model Validation Boundary

Avoid generic Plugin Configuration validation rules such as:

```text
supported models
```

when model availability/selection belongs to AI Model/Routing architecture.

A plugin-specific adapter mode MAY validate its own implementation-specific model reference only when that reference is truly plugin-owned.

---

# Permission Validation Boundary

Permission grant validation belongs to Plugin Security.

Configuration MAY declare:

```text
requires permission X
```

but permission ownership remains separate.

---

# Invalid Configuration

Invalid configuration SHOULD produce:

```text
PLUGIN_CONFIGURATION_INVALID
```

with field-level diagnostics where safe.

A plugin requiring the invalid configuration MUST NOT become ACTIVE.

---

# Missing Required Configuration

Missing required plugin-owned configuration results in:

```text
configuration unresolved / invalid
```

not:

```text
Registry BLOCKED
```

unless Security/Admin explicitly blocks the plugin.

---

# Configuration Lifecycle

Recommended:

```text
Plugin Descriptor / Schema
        |
        v
Load Configuration Layers
        |
        v
Resolve
        |
        v
Validate
        |
        v
Create Revision
        |
        v
Inject Configuration View
        |
        v
Plugin Initialize / Activate
```

---

# Lifecycle Integration

Plugin Lifecycle `RESOLVED` SHOULD require any mandatory configuration prerequisites to be valid.

Conceptually:

```text
VALIDATED
    |
    v
Dependency + Configuration + Permission Resolution
    |
    v
RESOLVED
```

---

# Configuration During Initialization

Initialization receives an immutable resolved view.

Plugins MUST NOT:

* reread raw application configuration,
* independently merge overrides,
* fetch unrelated Workspace settings.

---

# Runtime Updates

A configuration update creates another revision.

Recommended:

```text
Configuration Revision A
        |
        v
Update Requested
        |
        v
Validate Candidate Revision B
        |
        v
Classify Update
        |
        +--> HOT_RELOAD
        |
        +--> RESTART_REQUIRED
        |
        +--> RELOAD_REQUIRED
        |
        +--> REJECT
```

---

# Update Classification

Recommended:

```text
PluginConfigurationChange
├── oldRevision
├── newRevision
├── changedFields[]
├── effectiveUpdateMode
└── reason?
```

---

# Hot Reload

A hot-reloadable configuration change MAY be applied to an ACTIVE plugin without restart.

Requirements:

* schema permits it,
* plugin declares support,
* update is atomic from plugin perspective,
* active operations preserve their original config snapshot where required.

---

# Hot Reload Contract

Recommended logical flow:

```text
Validated Revision B
        |
        v
PluginConfigurationUpdate
        |
        v
Plugin accepts/rejects
        |
        v
Runtime switches future operations to B
```

---

# Hot Reload Failure

If plugin rejects a hot update:

```text
Revision A remains active
```

unless policy says otherwise.

Configuration persistence of B and runtime activation of B are separate facts.

---

# Restart Required

If any changed field is:

```text
RESTART_REQUIRED
```

the new configuration MAY be persisted but does not become runtime-effective until a new plugin runtime instance starts.

---

# Reload Required

`RELOAD_REQUIRED` MAY mean:

```text
dispose + unload + create new runtime instance
```

rather than simple stop/start.

Exact semantics align with `PLUGIN_LIFECYCLE.md`.

---

# Immutable After Install

Fields such as package-level runtime mode MAY be:

```text
IMMUTABLE_AFTER_INSTALL
```

and require reinstall/upgrade rather than ordinary config update.

---

# Configuration Persistence

Configuration architecture owns:

* configuration semantics,
* revisions,
* resolution,
* validation.

Storage infrastructure provides persistence.

Therefore:

```text
Configuration
    owns meaning

Storage
    owns persistence implementation
```

---

# Persistence Classes

Possible:

```text
EPHEMERAL
SESSION
PERSISTENT
DERIVED
```

---

# Ephemeral Override

An ephemeral override:

* is not persisted,
* applies to an explicit scope,
* expires with that scope/operation.

---

# Session Scope

Session-level configuration MAY exist only when the plugin genuinely needs implementation behavior scoped to Session.

It MUST NOT duplicate canonical Session business state.

---

# Persistent Configuration

Persistent plugin-specific configuration SHOULD survive application restart.

It remains independent of plugin-private arbitrary storage.

---

# Configuration Storage

Possible physical storage:

* configuration service/database,
* local settings database,
* encrypted configuration store.

Exact technology belongs to infrastructure.

---

# Plugin Writes

Plugins MUST NOT directly persist their own configuration files as canonical configuration state.

They MAY request configuration updates through the public Configuration API when permitted.

---

# Configuration Update Request

Recommended:

```text
PluginConfigurationUpdateRequest
├── pluginId
├── expectedRevision?
├── targetScope
├── changes
├── reason?
├── principalContext?
└── correlationId?
```

---

# Optimistic Concurrency

Configuration writes SHOULD support revision checks.

Example:

```text
expectedRevision = 12
```

prevents silently overwriting revision 13.

---

# Configuration Conflict

Concurrent conflicting update SHOULD return:

```text
PLUGIN_CONFIGURATION_WRITE_CONFLICT
```

rather than last-write-wins by accident.

---

# Secrets

Raw secrets SHOULD NOT be ordinary Plugin Configuration values.

Examples:

```text
API Key
Access Token
Client Secret
Private Key
```

belong to Secret Management / Provider Management.

---

# Credential Reference

Plugin Configuration MAY contain:

```text
credentialReference
```

when a plugin-owned integration requires an approved credential binding.

---

# Secret Resolution

Preferred:

```text
Plugin
    |
    v
Credential / Provider Host Service
    |
    v
Secret Management
```

not:

```text
PluginConfiguration
    contains plaintext secret
```

---

# Raw Secret Access

If a plugin absolutely requires raw secret material:

* permission must explicitly allow it,
* scope must be minimal,
* secret must not be persisted back into plugin config,
* telemetry must redact it,
* access should be short-lived.

MVP SHOULD minimize this.

---

# Authentication Configuration

Non-secret authentication metadata MAY exist in plugin configuration.

Example:

```text
credentialReference
authenticationMode
accountProfileReference
```

Actual secret value remains external.

---

# Provider Configuration

For provider-related plugins, canonical:

```text
ProviderConfiguration
```

SHOULD remain owned by provider-management.

Plugin Configuration may reference it.

It MUST NOT duplicate:

```text
API credentials
provider account state
provider entitlement
model routing state
```

---

# Retry Configuration

Generic fields such as:

```text
Retry Count
```

SHOULD NOT normally be plugin configuration.

Retry belongs to runtime/recovery architecture.

A plugin MAY expose only low-level implementation-specific retry behavior if the host contract explicitly delegates that behavior and it does not conflict with global Retry semantics.

---

# Timeout Configuration

Timeout ownership must be explicit.

Possible layers:

```text
operation deadline
attempt timeout
plugin-internal connection timeout
```

Only implementation-specific internal timeout MAY belong to plugin config.

---

# Cache Configuration

Global AI/OCR Result Cache policy MUST NOT be duplicated into plugin config.

A plugin-local implementation cache MAY have settings such as:

```text
localCacheSize
localCacheTTL
```

when it is purely implementation-internal.

---

# Streaming Configuration

Whether business execution requires/prefer streaming belongs to Request/Routing semantics.

A plugin MAY have implementation-specific streaming buffer settings, but not own the global streaming requirement.

---

# Language Configuration

Canonical source/target Language belongs to Request/Profile/Domain semantics.

Plugin config MAY contain only implementation-specific language mapping behavior if truly needed.

Provider-specific language-code mapping SHOULD remain adapter-internal.

---

# Feature Flags

Plugin-specific feature flags are allowed only for implementation-specific behavior.

Avoid putting system-wide product features into plugin config.

Example acceptable:

```text
useExperimentalParser
```

Example not acceptable:

```text
enableTranslationFeatureGlobally
```

---

# Worker / Performance Settings

Implementation-specific settings MAY include:

```text
workerCount
batchSize
queueCapacity
localMemoryLimit
```

but Runtime resource policy may cap them.

Plugin config cannot override Host hard resource constraints.

---

# Host Constraint

Effective runtime configuration MAY be constrained by Host Policy.

Example:

```text
plugin requests workerCount = 16

Host maximum = 4
```

resolved runtime configuration may become:

```text
workerCount = 4
```

only if schema/policy explicitly defines clamping semantics.

Otherwise validation SHOULD reject the value.

---

# Configuration References

Registry SHOULD store only references such as:

```text
configurationReference
```

rather than copying complete plugin configuration into Registry entries.

---

# Configuration Reference

Recommended:

```text
PluginConfigurationReference
├── pluginId
├── scope
├── configurationRevision
└── resourceReference
```

---

# Configuration Events

Recommended:

```text
PluginConfigurationResolved
PluginConfigurationValidated
PluginConfigurationUpdateRequested
PluginConfigurationUpdated
PluginConfigurationUpdateRejected
PluginConfigurationActivationRequiredRestart
```

---

# Persistence Events

A generic:

```text
ConfigurationPersisted
```

MAY remain internal infrastructure telemetry rather than a public application event unless another consumer needs it.

---

# Event Payload

Recommended:

```text
pluginId
scope
oldRevision?
newRevision?
changedFieldIds[]
updateMode?
reasonCode?
correlationId?
occurredAt
```

Secret values MUST NOT appear.

---

# Events vs Audit

Routine resolved/reload events are telemetry/application events.

Material changes MAY require Audit:

* admin configuration change,
* sensitive endpoint change,
* credential reference changed,
* execution isolation changed.

---

# Configuration Diagnostics

Diagnostics MAY expose:

```text
pluginId
schemaVersion
configurationRevision
effective scopes
changed fields
validation codes
update mode
configuration hash
```

---

# Sensitive Diagnostics

Diagnostics MUST NOT expose:

* raw secrets,
* tokens,
* credential payloads,
* sensitive field values unless explicitly authorized.

---

# Failure Categories

Possible normalized failures:

```text
PLUGIN_CONFIGURATION_SCHEMA_INVALID
PLUGIN_CONFIGURATION_REQUIRED_VALUE_MISSING
PLUGIN_CONFIGURATION_TYPE_INVALID
PLUGIN_CONFIGURATION_VALUE_INVALID
PLUGIN_CONFIGURATION_SCOPE_NOT_ALLOWED
PLUGIN_CONFIGURATION_REFERENCE_INVALID
PLUGIN_CONFIGURATION_SECRET_REFERENCE_INVALID
PLUGIN_CONFIGURATION_VERSION_INCOMPATIBLE
PLUGIN_CONFIGURATION_UPDATE_NOT_SUPPORTED
PLUGIN_CONFIGURATION_RESTART_REQUIRED
PLUGIN_CONFIGURATION_WRITE_CONFLICT
PLUGIN_CONFIGURATION_VALIDATION_FAILED
PLUGIN_CONFIGURATION_STORAGE_UNAVAILABLE
PLUGIN_CONFIGURATION_PERMISSION_DENIED
```

---

# Failure Handling

Configuration failure SHOULD preserve the last known valid configuration revision.

A failed candidate revision MUST NOT partially replace active configuration.

---

# Atomicity

Runtime config activation SHOULD be atomic from the plugin's perspective.

Avoid:

```text
half fields from Revision A
half fields from Revision B
```

during one operation.

---

# Rollback

If a hot update fails after partial application:

```text
runtime should restore Revision A
```

where feasible.

Plugins supporting hot reload SHOULD define transactional/update rollback semantics.

---

# Configuration Versioning

Two version dimensions SHOULD remain separate:

```text
configuration schema version
configuration data revision
```

Schema version describes structure/meaning.

Revision identifies one concrete configuration state.

---

# Schema Migration

Plugin upgrade MAY require:

```text
config schema v1
    ->
config schema v2
```

Migration rules belong jointly to Plugin Configuration and Plugin Versioning.

Migration MUST NOT silently destroy unknown/unsupported values without policy.

---

# Plugin Removal

Plugin removal SHOULD define what happens to persistent plugin configuration.

Possible policy:

```text
RETAIN
DELETE
ARCHIVE
```

Ordinary unload MUST NOT delete persistent configuration.

---

# Reinstall

Reinstalling the same plugin ID MAY reuse retained configuration only if schema/version compatibility is valid.

---

# Workspace Isolation

Workspace-scoped configuration MUST NOT leak across Workspaces.

A shared plugin runtime MUST receive the correct resolved Workspace view for each relevant operation.

---

# Principal Isolation

Principal-specific configuration MUST preserve authorization.

A plugin MUST NOT access another principal's configuration.

---

# Shared Runtime

If one plugin instance serves multiple Workspaces:

```text
global mutable plugin configuration object
```

is unsafe.

Prefer:

```text
operation-scoped immutable configuration views
```

or clearly separated runtime scopes.

---

# Runtime-Level Configuration

Some plugin settings are runtime-instance-wide.

Example:

```text
workerCount
process isolation mode
```

If one shared runtime cannot support different values per Workspace, allowed configuration scope MUST reflect that limitation.

---

# Configuration and Dependency Resolution

A configuration change MAY alter dependencies.

Example:

```text
executionMode:
    local -> remote
```

may require different Host Services.

Such changes MUST trigger dependency re-resolution before activation.

---

# Configuration and Permissions

A configuration change MAY require additional permissions.

Example:

```text
enableNetworkMode = true
```

must not become effective until required permission grants are valid.

---

# Configuration and Lifecycle

Possible effects:

```text
HOT_RELOADABLE
    -> stay ACTIVE

RESTART_REQUIRED
    -> new runtime instance

RELOAD_REQUIRED
    -> unload + new runtime instance
```

Plugin Lifecycle owns transition execution.

---

# Configuration and Registry

Registry stores configuration references/status only.

Registry MUST NOT become canonical configuration storage.

---

# Configuration and Observability

Configuration observability SHOULD use revisions/hashes rather than raw values.

---

# Configuration and Cache

Cache identity MAY include:

```text
configurationRevision
configurationHash
```

only when the plugin configuration materially affects cached output semantics.

---

# Configuration and Reproducibility

For durable AI/Recognition execution where plugin config materially affects output, provenance SHOULD retain:

```text
pluginId
pluginVersion
configurationRevision/hash
```

as appropriate.

---

# Architecture Invariants

1. Plugin configuration is external to plugin implementation.

2. Plugins MUST NOT read arbitrary CRAI application configuration directly.

3. Every plugin has an isolated configuration namespace.

4. Plugin configuration MUST NOT duplicate canonical CRAI configuration without clear ownership.

5. Plugin Configuration does not own Workspace Policy.

6. Plugin Configuration does not own AI Routing Policy.

7. Plugin Configuration does not own Retry/Fallback Policy.

8. Plugin Configuration does not own global Cache Policy.

9. Plugin Configuration does not own Provider credentials.

10. Configuration fields SHOULD be schema-defined.

11. Configuration schema is versioned.

12. Configuration data revisions are distinct from schema versions.

13. Configuration scope MUST be explicit.

14. Not every field supports every configuration scope.

15. Precedence SHOULD be field/scope-aware.

16. Runtime overrides MUST NOT automatically outrank mandatory Security/Policy.

17. Plugins consume resolved configuration rather than resolving layers themselves.

18. Resolved configuration SHOULD be immutable for one operation.

19. Configuration updates MUST NOT silently alter an in-flight operation.

20. Configuration revisions SHOULD be traceable.

21. Invalid configuration MUST NOT become runtime-effective.

22. A failed configuration update preserves the last known valid active revision.

23. Configuration update activation SHOULD be atomic.

24. Hot-reloadable fields MUST be explicitly declared.

25. Restart-required fields MUST NOT be hot-applied silently.

26. Reload-required changes create a new runtime lifecycle instance as required.

27. Plugins MUST NOT directly persist canonical configuration state.

28. Storage implementation does not own Configuration semantics.

29. Registry stores configuration references, not full canonical configuration.

30. Raw secrets SHOULD NOT be ordinary Plugin Configuration values.

31. Credential references MAY be stored in configuration.

32. Secret resolution belongs to approved Secret/Provider Host Services.

33. Plugins MUST NOT log configuration secrets.

34. Plugin configuration events MUST NOT expose secrets.

35. Provider Configuration remains separate from Plugin Configuration.

36. Generic Retry Count SHOULD NOT be a default plugin configuration category.

37. Global model selection SHOULD NOT be a plugin configuration responsibility.

38. Canonical Language settings SHOULD NOT be duplicated into plugin configuration.

39. System-wide product feature flags SHOULD NOT be hidden in plugin configuration.

40. Implementation-specific performance settings MAY be plugin configuration.

41. Host hard resource limits override or reject incompatible plugin settings according to explicit policy.

42. Workspace-scoped configuration MUST preserve tenant isolation.

43. Principal-scoped configuration MUST preserve authorization.

44. Shared runtimes MUST NOT use unsafe global mutable configuration across tenants.

45. Configuration changes MAY trigger dependency re-resolution.

46. Configuration changes MAY trigger permission re-evaluation.

47. Configuration changes MAY trigger lifecycle restart/reload.

48. Configuration failure MUST NOT automatically mark Registry state BLOCKED.

49. Configuration diagnostics SHOULD expose revisions/hashes, not sensitive values.

50. Configuration writes SHOULD support concurrency control.

51. Configuration update conflicts MUST be explicit.

52. Ordinary unload MUST NOT delete persistent plugin configuration.

53. Plugin upgrade may require configuration schema migration.

54. Reinstall reuse of configuration requires compatibility validation.

55. Operation provenance MAY retain configuration identity when it materially affects output.

56. New plugin configuration fields SHOULD fit existing schema/scope/update semantics rather than bypassing the Configuration system.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* plugin configuration namespace,
* versioned configuration schema,
* plugin defaults,
* System scope,
* Workspace scope,
* Project scope where needed,
* Principal scope where needed,
* Operation Override,
* deterministic resolution,
* immutable ResolvedPluginConfiguration,
* configuration revision,
* configuration hash,
* source map,
* validation,
* HOT_RELOADABLE,
* RESTART_REQUIRED,
* RELOAD_REQUIRED,
* persistent configuration,
* ephemeral override,
* optimistic concurrency,
* configuration references in Registry,
* credential references,
* secure diagnostics,
* configuration events,
* lifecycle integration.

MVP SHOULD NOT place these directly into generic plugin config:

* raw API secrets,
* global Retry policy,
* AI model routing policy,
* global Cache policy,
* Workspace budget,
* global Safety policy.

MVP MAY defer:

* Session-scope plugin configuration,
* complex admin override rules,
* live transactional multi-plugin configuration updates,
* configuration inheritance between plugins,
* automatic schema migration,
* cross-device configuration sync,
* configuration marketplace presets,
* user-editable arbitrary plugin schema UI generation.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact configuration schema format,
* exact scope taxonomy,
* default precedence order,
* Project vs Workspace override rules,
* principal/user scope necessity,
* admin override semantics,
* `PluginConfigurationView` interface,
* resolved configuration persistence,
* configuration hash algorithm,
* configuration revision format,
* source-map retention,
* hot-reload API,
* rollback semantics,
* restart vs reload distinction,
* plugin validation hook,
* credential-reference type,
* provider-configuration reference integration,
* Storage implementation,
* optimistic concurrency mechanism,
* configuration event persistence,
* Audit requirements,
* schema migration format,
* removal retention defaults,
* reinstall behavior,
* shared-runtime multi-Workspace configuration model,
* config-driven dependency re-resolution.

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
* `PLUGIN_SECURITY.md`
* `PLUGIN_VERSIONING.md`

Architecture:

* `../domain/WORKSPACE.md`
* `../domain/PROJECT.md`
* `../domain/PROFILE.md`
* `../modules/OWNERSHIP_MAP.md`

AI:

* `../ai/ROUTING.md`
* `../ai/RETRY.md`
* `../ai/CACHE.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`
* `../../02-modules/storage/`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/logging/`
* `../../03-infrastructure/telemetry/`

Runtime:

* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RESOURCE_LIFECYCLE.md`
