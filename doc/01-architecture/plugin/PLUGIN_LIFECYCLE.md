# Plugin Lifecycle

- Document: Plugin Architecture / Plugin Lifecycle
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the complete lifecycle of a plugin within the CRAI Plugin System.

A deterministic lifecycle ensures that every plugin is discovered, validated, initialized, executed and unloaded in a consistent and predictable manner.

The Plugin Manager is solely responsible for orchestrating the lifecycle.

---

# Design Principles

- Deterministic lifecycle
- Single lifecycle owner
- Safe resource management
- Failure isolation
- Graceful shutdown
- Idempotent transitions

---

# Lifecycle Overview

```text
Not Installed
      │
      ▼
Discovered
      │
      ▼
Validated
      │
      ▼
Loaded
      │
      ▼
Initialized
      │
      ▼
Started
      │
      ▼
Running
      │
      ▼
Stopping
      │
      ▼
Stopped
      │
      ▼
Disposed
      │
      ▼
Unloaded
```

Each state transition is managed exclusively by the Plugin Manager.

---

# Lifecycle States

## Not Installed

The plugin is unavailable to the runtime.

No metadata is registered.

---

## Discovered

The Plugin Manager detects the plugin package.

Typical actions:

- Locate manifest
- Read metadata
- Register candidate plugin

The plugin code is not executed.

---

## Validated

The plugin manifest and compatibility are verified.

Validation includes:

- Plugin API version
- Required fields
- Dependency checks
- Capability declarations
- Digital signature (optional)
- Supported platform

Failure prevents loading.

---

## Loaded

The plugin binary or implementation is loaded into memory.

No background work is started.

---

## Initialized

The plugin allocates required resources.

Typical actions:

- Read configuration
- Create clients
- Allocate caches
- Prepare internal state

The plugin must not process requests yet.

---

## Started

The plugin begins normal operation.

Typical actions:

- Subscribe to events
- Register capabilities
- Accept requests
- Start background workers (if required)

---

## Running

The plugin is operational.

The Plugin Manager may:

- Route requests
- Monitor health
- Collect diagnostics

---

## Stopping

The Plugin Manager requests graceful shutdown.

The plugin should:

- Stop accepting new work
- Finish active tasks
- Unsubscribe from events
- Flush pending state

---

## Stopped

The plugin has completed execution.

Resources remain allocated until disposal.

---

## Disposed

The plugin releases all remaining resources.

Typical actions:

- Close connections
- Release memory
- Dispose caches
- Release handles

---

## Unloaded

The plugin is removed from the runtime.

The Plugin Manager unregisters:

- Metadata
- Capabilities
- Health information

---

# Lifecycle Events

Typical events:

| Event | Description |
|--------|-------------|
| PluginDiscovered | Plugin detected |
| PluginValidated | Validation succeeded |
| PluginLoaded | Plugin loaded into memory |
| PluginInitialized | Initialization completed |
| PluginStarted | Plugin entered running state |
| PluginStopped | Plugin stopped |
| PluginDisposed | Resources released |
| PluginUnloaded | Plugin removed |

---

# Failure Handling

Possible failure points:

- Discovery
- Validation
- Loading
- Initialization
- Startup
- Shutdown

Rules:

- Failures remain isolated.
- Failed plugins never affect other plugins.
- Diagnostics records every lifecycle failure.
- Recoverable failures may be retried.

---

# Restart Flow

A plugin restart follows:

```text
Running
    │
    ▼
Stopping
    ▼
Stopped
    ▼
Disposed
    ▼
Loaded
    ▼
Initialized
    ▼
Started
    ▼
Running
```

The restart process follows the same validation and lifecycle rules.

---

# Lifecycle Invariants

1. The Plugin Manager exclusively controls lifecycle transitions.
2. Plugins never transition directly between arbitrary states.
3. Initialization occurs exactly once per load cycle.
4. Running plugins have completed validation and initialization.
5. Resources are released before unloading.
6. Lifecycle transitions are deterministic and idempotent.
7. Plugin failures never compromise the CRAI Core.

---

# Related Documents

- README.md
- PLUGIN_SYSTEM.md
- PLUGIN_API.md
- PLUGIN_REGISTRY.md
- PLUGIN_DISCOVERY.md
- PLUGIN_DEPENDENCY.md
- PLUGIN_CONFIGURATION.md
- PLUGIN_SECURITY.md
- PLUGIN_VERSIONING.md
