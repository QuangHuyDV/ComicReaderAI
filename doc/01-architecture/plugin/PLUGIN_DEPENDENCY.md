# Plugin Dependency

- Document: Plugin Architecture / Plugin Dependency
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines how dependencies between plugins are declared, validated and resolved within the CRAI Plugin System.

The dependency model ensures that plugins are loaded in a deterministic order while preventing circular dependencies and incompatible runtime configurations.

---

# Design Principles

- Explicit dependencies
- Deterministic resolution
- No hidden runtime dependencies
- Version-aware compatibility
- Failure isolation
- Capability-first architecture

---

# Dependency Model

Each plugin may declare:

- Required Dependencies
- Optional Dependencies
- Required Capabilities
- Supported Plugin API Version
- Supported CRAI Version

Dependencies are declared in the plugin manifest and validated before loading.

---

# Dependency Types

## Required Dependency

A required dependency must be present and compatible.

If unavailable:

- Plugin loading fails.
- Plugin remains disabled.

Example:

```text
Translation Plugin
    │
    ▼
HTTP Client Plugin
```

---

## Optional Dependency

Optional dependencies enhance functionality but are not required.

If unavailable:

- Plugin continues with reduced functionality.

---

## Capability Dependency

Instead of depending on a specific plugin, a plugin may depend on a capability.

Example:

```text
Needs:
Translate

Satisfied by:

Gemini
GPT
Google Translate
```

The Plugin Manager selects the most appropriate provider.

---

# Dependency Graph

Example:

```text
Application
      │
      ▼
Plugin Manager
      │
 ┌────┴─────┐
 ▼          ▼
OCR      Translation
 │            │
 ▼            ▼
Storage   Diagnostics
```

The dependency graph is directed and acyclic.

---

# Dependency Resolution

Resolution process:

```text
Read Manifest
      │
      ▼
Collect Dependencies
      │
      ▼
Validate Versions
      │
      ▼
Resolve Capabilities
      │
      ▼
Topological Sort
      │
      ▼
Load Plugins
```

Plugins are initialized according to dependency order.

---

# Version Compatibility

Each dependency may specify:

- Minimum Version
- Maximum Version (optional)
- Compatible Plugin API Version

The Plugin Manager rejects incompatible combinations before loading.

---

# Circular Dependencies

Circular dependencies are prohibited.

Example (invalid):

```text
Plugin A
   │
   ▼
Plugin B
   │
   ▼
Plugin A
```

Detected cycles prevent all affected plugins from loading.

---

# Missing Dependencies

If a required dependency cannot be resolved:

- The plugin is marked as Blocked.
- Diagnostics records the failure.
- Dependent plugins are not started.

Optional dependencies are ignored with a warning.

---

# Runtime Changes

When a dependency becomes unavailable at runtime:

- Health status is updated.
- Dependent plugins enter a degraded or stopped state according to their policy.
- Plugin Manager may attempt recovery or restart.

---

# Dependency Events

| Event | Description |
|--------|-------------|
| DependencyResolved | Dependencies resolved successfully |
| DependencyMissing | Required dependency unavailable |
| DependencyConflict | Version or capability conflict detected |
| DependencyCycleDetected | Circular dependency detected |

---

# Architecture Invariants

1. Dependencies are declared explicitly in the manifest.
2. Dependency resolution completes before plugin initialization.
3. Circular dependencies are never permitted.
4. Plugins are loaded in dependency order.
5. Capability dependencies are preferred over implementation-specific dependencies.
6. Version compatibility is verified before loading.
7. Dependency failures never compromise the CRAI Core.

---

# Related Documents

- README.md
- PLUGIN_SYSTEM.md
- PLUGIN_API.md
- PLUGIN_LIFECYCLE.md
- PLUGIN_REGISTRY.md
- PLUGIN_DISCOVERY.md
- PLUGIN_CONFIGURATION.md
- PLUGIN_SECURITY.md
- PLUGIN_VERSIONING.md
