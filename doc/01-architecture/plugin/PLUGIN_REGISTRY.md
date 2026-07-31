# Plugin Registry

- Document: Plugin Architecture / Plugin Registry
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

The Plugin Registry is the authoritative catalog of every plugin known to the CRAI runtime.

It maintains plugin metadata, capabilities, lifecycle state, dependencies and runtime status without containing plugin implementations.

The Plugin Registry is managed exclusively by the Plugin Manager.

---

# Design Principles

- Single source of truth
- Runtime independent
- Immutable metadata after registration
- Capability-based lookup
- Fast discovery
- Consistent lifecycle tracking

---

# Registry Responsibilities

The Plugin Registry is responsible for:

- Registering discovered plugins.
- Storing plugin metadata.
- Tracking lifecycle state.
- Indexing capabilities.
- Recording dependencies.
- Maintaining enable/disable status.
- Exposing lookup APIs.

The registry never loads or executes plugins.

---

# Registry Model

```text
Plugin Registry
├── Plugin Metadata
├── Lifecycle State
├── Capabilities
├── Dependencies
├── Configuration Reference
├── Health Status
└── Runtime Statistics
```

---

# Stored Metadata

Each registered plugin includes:

- Plugin ID
- Display Name
- Version
- Plugin API Version
- Vendor
- Category
- Description
- Supported Platforms

Metadata is populated during discovery and validation.

---

# Lifecycle Information

The registry tracks the current lifecycle state:

- Discovered
- Validated
- Loaded
- Initialized
- Started
- Running
- Stopped
- Disposed
- Unloaded

State transitions are updated only by the Plugin Manager.

---

# Capability Index

Capabilities are indexed independently from plugin identity.

Example:

```text
Translate
 ├── translation.gemini
 ├── translation.gpt
 └── translation.google

OCR
 ├── ocr.paddle
 └── ocr.tesseract
```

Consumers request capabilities instead of specific plugins.

---

# Dependency Graph

The registry records:

- Required dependencies
- Optional dependencies
- Plugin API version
- Minimum CRAI version

Dependency resolution is performed before loading.

---

# Enable / Disable State

Each plugin has one of:

- Enabled
- Disabled
- Blocked
- Incompatible

Disabled plugins remain registered but are unavailable for runtime selection.

---

# Health Information

Runtime health is tracked separately:

- Healthy
- Degraded
- Unhealthy
- Unknown

Health information is updated from the plugin Health() API.

---

# Lookup Operations

Typical registry queries include:

- Find by Plugin ID
- Find by Capability
- Find by Category
- List Enabled Plugins
- List Running Plugins
- List Incompatible Plugins
- Resolve Dependencies

---

# Registry Events

Published events may include:

| Event | Description |
|--------|-------------|
| PluginRegistered | Plugin added to registry |
| PluginUpdated | Metadata updated |
| PluginEnabled | Plugin enabled |
| PluginDisabled | Plugin disabled |
| PluginRemoved | Plugin removed from registry |

---

# Failure Handling

Registry failures should:

- Prevent duplicate registration.
- Reject invalid metadata.
- Preserve existing registry integrity.
- Report failures to Diagnostics.

The registry never attempts to recover by loading plugins directly.

---

# Architecture Invariants

1. The Plugin Registry is the single source of truth for plugin metadata.
2. Only the Plugin Manager modifies registry contents.
3. Metadata remains immutable after successful registration except for runtime state.
4. Capabilities are indexed independently from implementations.
5. Disabled plugins remain discoverable but are not selectable.
6. Registry operations never execute plugin code.
7. Registry data is consistent with the plugin lifecycle.

---

# Related Documents

- README.md
- PLUGIN_SYSTEM.md
- PLUGIN_API.md
- PLUGIN_LIFECYCLE.md
- PLUGIN_DISCOVERY.md
- PLUGIN_DEPENDENCY.md
- PLUGIN_CONFIGURATION.md
- PLUGIN_SECURITY.md
- PLUGIN_VERSIONING.md
