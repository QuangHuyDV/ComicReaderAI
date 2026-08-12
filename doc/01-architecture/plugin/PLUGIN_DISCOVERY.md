# Plugin Discovery

* **Document:** Plugin Architecture / Plugin Discovery
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI discovers potential plugin artifacts before they enter validation, compatibility evaluation, trust evaluation, registration or loading.

Discovery is a read-mostly identification process.

Its responsibilities are to:

* locate candidate plugin artifacts,
* read discovery-safe metadata,
* identify Plugin Descriptor candidates,
* record provenance,
* identify obvious structural problems,
* return deterministic discovery results.

Discovery MUST NOT execute plugin implementation code.

---

# Core Principle

```text
Configured Discovery Sources
        |
        v
Locate Candidate Artifacts
        |
        v
Read Discovery-Safe Metadata
        |
        v
Create Discovery Candidates
        |
        v
Structural Checks
        |
        v
Discovery Result
        |
        v
Validation / Compatibility / Security
        |
        v
Registry Registration
```

Discovery answers:

```text
What plugin artifacts appear to exist?
```

It does NOT answer:

```text
Is this plugin trusted?
Is it compatible?
Should it be enabled?
Should it be loaded?
```

---

# Scope

Plugin Discovery covers:

* discovery sources,
* package/artifact enumeration,
* manifest reading,
* descriptor-candidate extraction,
* discovery provenance,
* basic structural checks,
* duplicate candidate detection,
* deterministic ordering,
* discovery diagnostics,
* discovery events.

---

# Non-Goals

Discovery does NOT own:

* canonical Registry registration,
* Plugin API compatibility validation,
* capability-contract validation,
* trust evaluation,
* signature approval,
* permission grants,
* dependency resolution,
* enable/disable state,
* plugin loading,
* plugin initialization,
* package installation,
* runtime activation,
* provider health.

---

# Design Principles

Plugin Discovery SHOULD be:

* read-only where practical,
* deterministic,
* manifest-first,
* implementation-language independent,
* platform-aware,
* package-format aware,
* secure by default,
* repeatable,
* side-effect-minimal,
* provenance-preserving,
* tolerant of invalid candidates.

---

# Discovery Pipeline

Recommended:

```text
Discovery Trigger
      |
      v
Resolve Discovery Sources
      |
      v
Enumerate Candidate Artifacts
      |
      v
Read Manifest / Descriptor Metadata
      |
      v
Create PluginDiscoveryCandidate
      |
      v
Structural Discovery Checks
      |
      v
Normalize Candidate
      |
      v
Collect Discovery Result
```

After discovery:

```text
Discovery Result
      |
      v
Descriptor Validation
      |
      v
Compatibility Evaluation
      |
      v
Trust / Security Evaluation
      |
      v
Registry Registration / Reconciliation
```

---

# Discovery Result

Recommended:

```text
PluginDiscoveryResult
├── discoveryRunId
├── sourceSnapshots[]
├── candidates[]
├── rejectedArtifacts[]
├── conflicts[]
├── warnings[]
├── startedAt
├── completedAt
└── discoveryVersion
```

---

# Discovery Candidate

Recommended:

```text
PluginDiscoveryCandidate
├── candidateId
├── artifactReference
├── artifactIdentity?
├── descriptorCandidate
├── discoverySource
├── sourcePriority?
├── discoveredAt
├── structuralStatus
├── warnings[]
└── contentHash?
```

A Discovery Candidate is NOT yet a canonical Plugin Registry Entry.

---

# Candidate Identity

`candidateId` identifies one discovered artifact occurrence.

It is distinct from:

```text
pluginId
pluginVersion
installationId
registryEntryId
runtimeInstanceId
```

This allows Discovery to represent conflicting or duplicate artifacts safely.

---

# Artifact Identity

Where practical, Discovery SHOULD capture an artifact identity such as:

* content hash,
* package digest,
* bundle identity,
* canonical path plus metadata,
* signed package identity.

Artifact identity helps detect:

```text
same plugin ID/version
but different package content
```

without executing code.

---

# Discovery Sources

Plugins MAY be discovered from sources such as:

```text
BUILT_IN
SYSTEM_DIRECTORY
USER_DIRECTORY
ENTERPRISE_DIRECTORY
DEVELOPMENT_DIRECTORY
PACKAGE_INSTALLATION
APPLICATION_BUNDLE
REPOSITORY_CACHE
CUSTOM
```

Not every CRAI deployment supports every source.

---

# Discovery Source

Recommended:

```text
PluginDiscoverySource
├── sourceId
├── sourceType
├── locationReference?
├── priority?
├── trustHint?
├── enabled
└── configurationRevision?
```

Discovery source priority is installation/discovery metadata.

It MUST NOT determine business capability selection.

---

# Built-In Extensions

Built-in implementations MAY participate in discovery through:

```text
BUILT_IN
```

descriptor registration.

They do not require filesystem scanning.

---

# Directory Discovery

Directory-based discovery SHOULD operate only on configured roots.

Plugins MUST NOT cause CRAI to recursively inspect arbitrary user/system directories.

---

# Directory Layout

CRAI SHOULD NOT require top-level directories such as:

```text
capture/
ocr/
translation/
ai/
storage/
```

as canonical plugin categories.

Preferred generic layout:

```text
plugins/
├── plugin-a/
├── plugin-b/
├── plugin-c/
└── ...
```

Capabilities are declared in the descriptor.

---

# Optional Organizational Directories

Implementations MAY organize packages physically by category for convenience.

Example:

```text
plugins/
├── recognition/
├── providers/
└── export/
```

But physical directory grouping MUST NOT define canonical capability semantics.

---

# Plugin Package

A plugin package MAY contain:

```text
plugin.example/
├── manifest.json
├── implementation/
├── resources/
├── localization/
├── migrations/
└── README.md
```

Exact package layout is package-format specific.

---

# Package Format

Discovery MUST NOT assume one implementation binary format.

Possible implementations may be:

* native library,
* managed library,
* executable process,
* script package,
* local service bundle,
* remote adapter descriptor.

The manifest/descriptor boundary remains common.

---

# Manifest

The manifest is the primary discovery-safe metadata source.

It MUST be readable without executing plugin implementation code.

---

# Manifest Candidate Fields

Typical fields MAY include:

```text
pluginId
pluginVersion
pluginApiVersion
displayName
publisher
capabilities[]
dependencies[]
requiredPermissions[]
supportedPlatforms[]
supportedCRAIRange?
entryPoint
configurationSchemaReference?
executionModel?
integrityMetadata?
```

Discovery only parses these fields.

Semantic validation occurs later.

---

# Manifest Is Untrusted

All manifest data MUST initially be treated as untrusted.

A manifest claiming:

```text
trusted = true
compatible = true
permission = ALL
```

MUST NOT cause those states to be granted.

---

# Manifest Parser Safety

Manifest parsing SHOULD:

* enforce input-size bounds,
* avoid executable deserialization,
* reject unsupported encodings,
* avoid entity/network expansion,
* enforce parser resource limits.

---

# Discovery-Safe Metadata

Discovery MAY read limited static files required to identify the package.

Examples:

* manifest,
* package metadata,
* signature metadata,
* static descriptor,
* checksums.

Discovery MUST NOT load implementation libraries to query metadata.

---

# No Code Execution

Forbidden during Discovery:

```text
load dynamic library
import plugin module
run plugin executable
instantiate plugin class
call descriptor() from plugin code
invoke initialization hook
execute package script
```

---

# Side Effects

Discovery SHOULD avoid side effects.

By default it MUST NOT:

* modify plugin files,
* write plugin configuration,
* start processes,
* open provider connections,
* access credentials,
* initialize databases,
* activate capabilities.

---

# Discovery Temporary State

Discovery MAY create bounded internal runtime state such as:

* directory enumeration buffers,
* candidate objects,
* diagnostics,
* hashes.

This does not violate read-only semantic behavior.

---

# Network Access

Normal local discovery SHOULD NOT require network access.

Future repository discovery MAY use network access through a separate repository/package-management path.

It SHOULD NOT silently turn local plugin scanning into remote code lookup.

---

# Repository Cache

A locally available marketplace/repository cache MAY be scanned as a source.

Remote marketplace search/install belongs outside ordinary runtime Discovery.

---

# Structural Checks

Discovery MAY perform only basic checks required to create a candidate safely.

Examples:

```text
manifest exists
manifest readable
manifest parseable
package root valid
artifact type recognizable
basic required discovery fields present
candidate path within allowed root
```

---

# Structural Check Boundary

Discovery SHOULD NOT decide final:

```text
Plugin API compatibility
Capability compatibility
Dependency satisfiability
Trust
Permission approval
Signature trust
Publisher trust
```

These belong to later stages.

---

# Descriptor Validation Boundary

Example:

```text
pluginApiVersion syntax parseable
    -> Discovery structural check

pluginApiVersion supported by Host
    -> Compatibility validation
```

Likewise:

```text
capabilities field is an array
    -> Discovery structural check

declared capability version is supported
    -> Capability validation
```

---

# Invalid Candidate

Discovery SHOULD distinguish different outcomes.

Possible:

```text
DISCOVERED
STRUCTURALLY_INVALID
UNREADABLE
UNSUPPORTED_FORMAT
OUTSIDE_ALLOWED_SOURCE
QUARANTINED
```

A structurally invalid artifact is not a canonical registered plugin.

---

# Rejected Artifact

Recommended:

```text
RejectedPluginArtifact
├── artifactReference
├── discoverySource
├── reasonCode
├── diagnosticReference?
└── discoveredAt
```

---

# Unsupported Package

An unsupported package format SHOULD normally be ignored or recorded diagnostically.

It MUST NOT be loaded speculatively.

---

# Malformed Package

Malformed plugin packages MUST NOT proceed to semantic validation/loading.

---

# Discovery Continues

A malformed candidate SHOULD NOT normally stop discovery of other candidates.

---

# Duplicate Candidates

Discovery MUST NOT simply reject every repeated `pluginId`.

Multiple candidates may legitimately represent:

```text
same plugin ID
different versions
```

or:

```text
same plugin/version
same identical artifact from mirrored source
```

or a real integrity conflict.

---

# Duplicate Classification

Recommended:

```text
CandidateDuplicateClassification
├── SAME_ARTIFACT
├── SAME_ID_DIFFERENT_VERSION
├── SAME_ID_VERSION_DIFFERENT_ARTIFACT
├── SHADOWED_SOURCE
└── UNKNOWN_CONFLICT
```

---

# Same ID Different Version

Example:

```text
plugin.example / 1.0.0
plugin.example / 2.0.0
```

This is NOT inherently a Discovery error.

Installation/Registry policy determines which versions may coexist.

---

# Same ID and Version, Same Artifact

Repeated discovery of the same artifact SHOULD be idempotent.

Example:

```text
same package visible through two equivalent paths
```

may be deduplicated by artifact identity.

---

# Same ID and Version, Different Artifact

This is a serious integrity conflict.

Discovery SHOULD report:

```text
PLUGIN_DISCOVERY_ARTIFACT_CONFLICT
```

It SHOULD NOT arbitrarily trust the highest-priority candidate without later policy/security evaluation.

---

# Source Priority

Source priority MAY be used as an input to Registry reconciliation or installation policy.

Discovery MUST retain all relevant conflicting candidates/provenance long enough for safe resolution.

---

# Shadowing

A higher-priority installation MAY shadow another candidate for activation.

Shadowing is not equivalent to deleting/forgetting the lower-priority candidate.

Diagnostics SHOULD preserve the conflict.

---

# Deterministic Ordering

For identical:

* configured sources,
* source contents,
* discovery configuration,
* discovery implementation version,

candidate ordering SHOULD be deterministic.

---

# Stable Ordering

Recommended tie-breakers MAY include:

```text
source priority
normalized pluginId
pluginVersion
canonical artifact reference
artifact hash
```

---

# Filesystem Ordering

Discovery MUST NOT depend on nondeterministic filesystem enumeration order.

Results SHOULD be explicitly sorted.

---

# Platform Filtering

Discovery MAY identify whether a package appears relevant to the current platform.

But final platform compatibility belongs to validation.

Example:

```text
supportedPlatforms:
    linux
```

on Windows MAY be marked:

```text
candidate platform mismatch
```

without executing code.

---

# Platform Independence

Discovery architecture MUST remain independent from plugin implementation language/runtime.

Platform-specific scanning mechanisms may exist behind Discovery adapters.

---

# Discovery Adapter

Possible:

```text
PluginDiscoveryAdapter
├── FileSystemDiscoveryAdapter
├── BuiltInDiscoveryAdapter
├── PackageManagerDiscoveryAdapter
└── RepositoryCacheDiscoveryAdapter
```

Adapters produce normalized Discovery Candidates.

---

# Discovery Adapter Boundary

Adapters know:

* how to enumerate one source type,
* how to read source metadata.

They MUST NOT:

* activate plugins,
* grant permissions,
* register capabilities,
* select providers.

---

# Discovery Configuration

Discovery configuration MAY define:

```text
enabled sources
allowed roots
ignored patterns
package formats
maximum scan depth
maximum package count
maximum manifest size
source priorities
```

---

# Configuration Ownership

Discovery consumes resolved configuration.

It MUST NOT own Workspace/Project business configuration.

---

# Ignore Rules

Discovery MAY ignore:

* hidden temporary files,
* editor swap files,
* incomplete downloads,
* known non-plugin artifacts,
* quarantine directories.

Ignore rules SHOULD be deterministic.

---

# Symlink Handling

Filesystem discovery MUST define symlink behavior.

Possible safe policy:

```text
do not follow symlinks outside configured discovery root
```

This prevents directory traversal and accidental broad scans.

---

# Path Traversal

Manifest/package paths MUST NOT allow traversal outside the plugin package root.

---

# Package Bomb Protection

Discovery SHOULD bound:

* archive size,
* manifest size,
* file count,
* nesting depth,

when scanning archived packages.

---

# Signature Metadata

Discovery MAY read static signature metadata.

It MUST NOT decide trust solely from the plugin's own claims.

Signature verification/trust belongs to Plugin Security or package-validation architecture.

---

# Integrity Hashing

Discovery MAY calculate package/content hashes.

This is allowed because hashing reads artifact bytes without executing them.

---

# Hash Cost

Hashing very large packages MAY be deferred or bounded.

Discovery MAY first use cheaper metadata and perform full integrity hashing during validation/install reconciliation.

---

# Discovery Events

Possible normalized events:

```text
PluginDiscoveryStarted
PluginCandidateDiscovered
PluginCandidateRejected
PluginDiscoveryConflictDetected
PluginDiscoveryCompleted
```

---

# Event Semantics

`PluginCandidateDiscovered` means:

```text
candidate artifact identified
```

not:

```text
plugin registered
trusted
enabled
loaded
```

---

# Discovery Event Payload

Recommended:

```text
discoveryRunId
candidateId?
pluginId?
pluginVersion?
sourceId
status
reasonCode?
correlationId?
occurredAt
```

Avoid embedding full manifests where unnecessary.

---

# Event Bus Boundary

High-volume scan diagnostics need not become durable global Event Bus events.

Detailed scan records MAY remain local telemetry/diagnostics.

---

# Diagnostics

Discovery diagnostics MAY include:

```text
source
artifact reference
candidate ID
plugin ID/version if parseable
failure code
artifact hash
manifest size
scan duration
```

---

# Sensitive Diagnostics

Discovery logs SHOULD NOT expose:

* secrets embedded incorrectly in manifests,
* arbitrary package contents,
* user file contents outside plugin metadata.

If a manifest contains secrets unexpectedly, diagnostics SHOULD redact them.

---

# Failure Handling

Discovery failures SHOULD be isolated per source/artifact where possible.

Examples:

```text
source inaccessible
manifest unreadable
parser failure
artifact conflict
resource limit exceeded
permission denied
```

---

# Source Failure

Failure to scan one optional discovery source SHOULD NOT normally prevent scanning others.

---

# Mandatory Source

A deployment MAY mark a discovery source mandatory.

If that source cannot be scanned safely, application startup MAY fail according to configuration.

This is deployment/runtime policy, not the default Plugin Discovery rule.

---

# Discovery Failure Codes

Possible stable failures:

```text
PLUGIN_DISCOVERY_SOURCE_UNAVAILABLE
PLUGIN_DISCOVERY_ACCESS_DENIED
PLUGIN_DISCOVERY_MANIFEST_MISSING
PLUGIN_DISCOVERY_MANIFEST_UNREADABLE
PLUGIN_DISCOVERY_MANIFEST_INVALID
PLUGIN_DISCOVERY_PACKAGE_MALFORMED
PLUGIN_DISCOVERY_PACKAGE_UNSUPPORTED
PLUGIN_DISCOVERY_ARTIFACT_CONFLICT
PLUGIN_DISCOVERY_RESOURCE_LIMIT
PLUGIN_DISCOVERY_PATH_VIOLATION
PLUGIN_DISCOVERY_INTERNAL_ERROR
```

---

# Recovery

Discovery recovery MAY include:

* continue with remaining artifacts,
* rescan source,
* retry transient filesystem access,
* ignore unsupported artifacts,
* quarantine malformed artifacts,
* rebuild source snapshot.

Discovery MUST NOT recover by executing plugin code.

---

# Discovery Snapshot

For reproducibility/reconciliation, CRAI MAY expose:

```text
PluginDiscoverySnapshot
├── discoveryRunId
├── sources[]
├── candidates[]
├── rejectedArtifacts[]
├── conflicts[]
├── createdAt
└── contentHash?
```

---

# Discovery Snapshot vs Registry Snapshot

```text
DiscoverySnapshot
    = what artifacts were observed
```

```text
RegistrySnapshot
    = what plugins are canonically known/registered
```

They MUST remain distinct.

---

# Registry Integration

Recommended:

```text
Discovery Result
        |
        v
Validation
        |
        v
Compatibility / Security
        |
        v
Registry Reconciliation
```

Discovery MUST NOT directly write canonical Registry entries.

---

# Registry Reconciliation

Registry reconciliation MAY compare:

```text
Discovery Snapshot
```

with:

```text
Registry Snapshot
```

to identify:

```text
NEW
UNCHANGED
UPDATED_VERSION
MISSING
ARTIFACT_CHANGED
CONFLICT
INVALID
```

---

# Plugin Manager Integration

Plugin Manager MAY trigger or consume Discovery.

It does NOT need to perform filesystem scanning itself.

Discovery SHOULD remain a separable service/component.

---

# Package Installer Boundary

Plugin Installation and Discovery are separate.

```text
Installer
    places/removes package artifacts

Discovery
    observes package artifacts
```

Discovery MUST NOT silently install packages.

---

# Marketplace Boundary

Future Marketplace/Repository architecture may:

* search remote plugins,
* download packages,
* verify repository metadata.

Normal runtime Discovery begins from locally available/installable artifacts or explicit repository-cache descriptors.

---

# Security

Discovery is part of the untrusted-code boundary.

Therefore:

* no implementation code execution,
* bounded parsing,
* bounded file access,
* path validation,
* package integrity checks where appropriate,
* no raw secrets access,
* no permission grants.

---

# Untrusted Manifest

Manifest content MUST NOT:

* change Host discovery roots,
* request immediate code execution,
* modify Registry state,
* auto-enable plugin,
* self-grant permissions,
* trigger network requests during parsing.

---

# Workspace Boundary

Plugin discovery is normally application/runtime-level rather than Workspace-level.

Workspace-specific enablement/configuration MAY be evaluated later.

If per-Workspace plugin installation is introduced, source scope MUST be explicit.

---

# Observability

Recommended metrics:

```text
discovery_run_count
discovery_duration
sources_scanned
artifacts_scanned
candidates_found
candidates_rejected
conflicts_found
manifest_parse_failures
resource_limit_rejections
```

---

# Architecture Invariants

1. Discovery never executes plugin implementation code.

2. Discovery is manifest/descriptor-first.

3. Discovery is side-effect-minimal.

4. Discovery does not load plugins.

5. Discovery does not initialize plugins.

6. Discovery does not activate capabilities.

7. Discovery does not grant permissions.

8. Discovery does not determine trust.

9. Discovery does not perform final API compatibility approval.

10. Discovery does not perform dependency resolution.

11. Discovery does not select capability providers.

12. Discovery produces candidates, not canonical Registry Entries.

13. Canonical Registry registration occurs after required validation.

14. Manifest data is untrusted until Host validation completes.

15. A discovered plugin ID does not imply plugin validity.

16. Duplicate Plugin IDs are not automatically invalid.

17. Same Plugin ID with different versions MAY be legitimate.

18. Same ID/version with different artifact identity is an integrity conflict.

19. Discovery SHOULD preserve conflict provenance instead of blindly discarding candidates.

20. Source priority MAY influence later reconciliation but does not establish trust.

21. Physical directory category MUST NOT define canonical capability category.

22. Capabilities are declared in descriptors.

23. Discovery architecture is implementation-language independent.

24. Discovery results SHOULD be deterministic for identical explicit inputs.

25. Filesystem enumeration order MUST NOT determine final candidate ordering.

26. Discovery source failures SHOULD be isolated where possible.

27. Invalid candidate failure SHOULD NOT normally stop scanning other candidates.

28. Discovery MUST remain bounded against malformed/hostile packages.

29. Path traversal outside approved roots is forbidden.

30. Symlink behavior MUST be explicit.

31. Package/manifest resource limits SHOULD be enforced.

32. Discovery MAY read static integrity/signature metadata.

33. Trust evaluation remains outside Discovery.

34. Discovery MAY calculate artifact hashes without executing code.

35. Discovery events represent candidate discovery, not plugin activation.

36. Discovery telemetry MUST avoid exposing sensitive package contents.

37. Discovery MUST NOT recover failures by executing plugins.

38. Discovery Snapshot and Registry Snapshot are separate concepts.

39. Discovery and Installation are separate concerns.

40. Discovery and Marketplace search/download are separate concerns.

41. Plugin Manager MAY coordinate Discovery but need not implement scanning.

42. Discovery MUST NOT write plugin configuration.

43. Discovery MUST NOT access provider credentials.

44. Discovery MUST NOT start network/provider sessions for plugin validation.

45. Only locally/configurationally authorized discovery sources may be scanned.

46. Discovery MUST NOT scan arbitrary filesystem locations.

47. Untrusted manifests cannot auto-enable or self-authorize plugins.

48. Discovery output SHOULD retain enough provenance for Registry reconciliation.

49. New package/source types SHOULD integrate through Discovery adapters.

50. Adding a new plugin capability does not require changing Discovery logic when descriptor contracts remain compatible.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* built-in discovery,
* one system/local plugin directory,
* one user plugin directory,
* manifest-first scanning,
* JSON or another single static manifest format,
* generic plugin package directories,
* deterministic source ordering,
* deterministic candidate ordering,
* required manifest-field checks,
* candidate IDs,
* artifact references,
* source provenance,
* basic artifact hashing,
* duplicate classification,
* same-ID/version artifact conflict detection,
* invalid-candidate isolation,
* scan resource limits,
* symlink/path safety,
* discovery diagnostics,
* Discovery Result,
* Registry reconciliation handoff.

MVP SHOULD NOT:

* execute plugin code during discovery,
* perform remote marketplace search during normal startup,
* auto-install missing plugins,
* auto-enable discovered plugins,
* grant permissions from manifest declarations,
* discard conflicting artifacts silently.

MVP MAY defer:

* archive package formats,
* signed-package verification,
* remote repository discovery,
* enterprise policy directories,
* hot filesystem watching,
* incremental discovery,
* per-Workspace installation,
* complex package metadata,
* remote plugin descriptors,
* distributed discovery.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact `PluginDiscoveryCandidate` schema,
* exact Discovery Result schema,
* manifest format,
* manifest file name,
* artifact-hash algorithm,
* whether full package hashing occurs during Discovery or Validation,
* canonical artifact-reference format,
* built-in extension discovery representation,
* discovery source priority semantics,
* system/user plugin directories,
* generic directory layout,
* ignored-file patterns,
* maximum manifest size,
* maximum package size,
* maximum scan depth,
* maximum candidates per source,
* symlink policy,
* archive handling,
* candidate duplicate algorithm,
* shadowing semantics,
* startup rescan strategy,
* Discovery Snapshot persistence,
* Registry reconciliation ownership,
* filesystem watch/hot discovery,
* quarantine mechanism,
* repository-cache integration,
* package-installer integration,
* discovery event persistence.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_SYSTEM.md`
* `PLUGIN_API.md`
* `PLUGIN_REGISTRY.md`
* `PLUGIN_LIFECYCLE.md`
* `PLUGIN_DEPENDENCY.md`
* `PLUGIN_CONFIGURATION.md`
* `PLUGIN_SECURITY.md`
* `PLUGIN_VERSIONING.md`

Architecture:

* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`

Infrastructure:

* `../../03-infrastructure/configuration/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/logging/`
* `../../03-infrastructure/telemetry/`

Runtime:

* `../runtime/RUNTIME_COMPONENTS.md`
* `../runtime/RESOURCE_LIFECYCLE.md`
