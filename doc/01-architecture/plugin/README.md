# Plugin Architecture

- **Module:** Plugin System
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

The Plugin Architecture enables CRAI to be extended without modifying the core application.

Plugins provide optional capabilities such as OCR, Translation, AI, Dictionary, Export and Storage while remaining isolated from the CRAI Core.

The architecture is capability-driven, version-aware and designed for long-term extensibility.

---

# Design Goals

- Modular architecture
- Stable public APIs
- Capability-based composition
- Runtime discovery
- Deterministic lifecycle
- Secure execution
- Version compatibility
- Independent deployment

---

# Architecture Overview

```text
                 CRAI Core
                      │
                      ▼
               Plugin Manager
                      │
      ┌───────────────┼───────────────┐
      ▼               ▼               ▼
 Plugin Registry   Lifecycle      Configuration
      │               │               │
      └───────┬───────┴───────┬───────┘
              ▼               ▼
         Capability Index   Security
              │
      ┌───────┼────────────────────────────┐
      ▼       ▼        ▼        ▼          ▼
    OCR   Translation  AI   Dictionary   Export
```

---

# Core Components

| Component | Responsibility |
|-----------|----------------|
| Plugin Manager | Coordinates discovery, loading, lifecycle and permissions |
| Plugin Registry | Stores plugin metadata and runtime state |
| Plugin API | Stable contract between CRAI Core and plugins |
| Discovery | Finds candidate plugins without executing code |
| Dependency Resolver | Resolves plugin and capability dependencies |
| Configuration | Provides validated plugin configuration |
| Security | Enforces permissions and protects the runtime |
| Versioning | Ensures compatibility across releases |

---

# Plugin Lifecycle

```text
Discover
    │
    ▼
Validate
    │
    ▼
Register
    │
    ▼
Load
    │
    ▼
Initialize
    │
    ▼
Start
    │
    ▼
Running
    │
    ▼
Stop
    │
    ▼
Dispose
    │
    ▼
Unload
```

---

# Supported Plugin Categories

- Capture
- OCR
- Translation
- AI Provider
- Dictionary
- Export
- Storage
- Utility

New categories may be introduced without changing the core architecture.

---

# Design Principles

- Plugins communicate through public contracts.
- Plugin implementations are isolated.
- Capabilities are preferred over implementation-specific dependencies.
- The Plugin Manager owns lifecycle transitions.
- The Registry is the single source of truth for plugin metadata.
- Configuration is external and validated.
- Security follows the principle of least privilege.

---

# Document Structure

| Document | Description |
|----------|-------------|
| PLUGIN_SYSTEM.md | Overall plugin architecture |
| PLUGIN_API.md | Public plugin contract |
| PLUGIN_LIFECYCLE.md | Plugin lifecycle management |
| PLUGIN_REGISTRY.md | Plugin metadata registry |
| PLUGIN_DISCOVERY.md | Plugin discovery process |
| PLUGIN_DEPENDENCY.md | Dependency resolution |
| PLUGIN_CONFIGURATION.md | Configuration model |
| PLUGIN_SECURITY.md | Security architecture |
| PLUGIN_VERSIONING.md | Compatibility and versioning |

---

# Related Architecture

- Event Bus
- Runtime
- Storage
- Diagnostics
- Provider
- UI Adapter

Plugins integrate with these modules only through documented public interfaces.

---

# Architecture Invariants

1. CRAI Core never depends on a concrete plugin implementation.
2. Every plugin implements the public Plugin API.
3. Plugins are discovered before they are validated.
4. Validation completes before loading.
5. Configuration is validated before initialization.
6. Plugins execute with explicit permissions only.
7. Capability-based resolution is preferred over direct implementation references.
8. Plugin failures never compromise the CRAI Core.
9. The Plugin Manager is the only component that controls plugin lifecycle.
10. The Plugin Registry remains the authoritative source of plugin metadata.
