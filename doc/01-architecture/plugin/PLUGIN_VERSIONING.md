# Plugin Versioning

- **Document:** Plugin Architecture / Plugin Versioning
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines how plugin versions, compatibility rules and upgrade policies are managed within the CRAI Plugin System.

The objective is to ensure that plugins remain compatible with the CRAI Core and with one another while allowing independent evolution over time.

---

# Design Principles

- Semantic Versioning (SemVer)
- Backward compatibility where practical
- Explicit compatibility declarations
- Independent plugin releases
- Deterministic version resolution
- Safe upgrades and rollbacks

---

# Version Model

Each plugin declares:

- Plugin Version
- Plugin API Version
- Minimum Supported CRAI Version
- Maximum Supported CRAI Version (optional)

Example:

```text
Plugin Version:      2.3.1
Plugin API Version:  1.2
CRAI Version:        >=1.5.0 <2.0.0
```

---

# Semantic Versioning

Plugins follow Semantic Versioning.

```text
MAJOR.MINOR.PATCH
```

- **MAJOR** – Breaking API changes.
- **MINOR** – Backward-compatible features.
- **PATCH** – Bug fixes and internal improvements.

---

# Compatibility Matrix

Compatibility is evaluated before loading.

| Component | Verified Against |
|-----------|------------------|
| Plugin | CRAI Core Version |
| Plugin | Plugin API Version |
| Plugin | Required Dependencies |
| Plugin | Capability Contracts |

A plugin is loaded only if all compatibility checks succeed.

---

# Plugin API Version

The Plugin API has its own version independent of plugin versions.

Example:

```text
Plugin API v1
├── Initialize()
├── Start()
├── Stop()
└── Dispose()

Plugin API v2
├── Initialize()
├── Start()
├── Pause()
├── Resume()
└── Dispose()
```

Plugins must implement the API version they declare.

---

# Upgrade Policy

Upgrade sequence:

```text
Current Plugin
      │
      ▼
Compatibility Check
      │
      ▼
Backup Configuration
      │
      ▼
Install New Version
      │
      ▼
Validate
      │
      ▼
Start Plugin
```

If validation fails, rollback may be performed.

---

# Rollback Policy

Rollback restores:

- Previous plugin version
- Previous configuration (if required)
- Previous runtime state where applicable

Rollback is managed by the Plugin Manager.

---

# Dependency Version Resolution

Dependency declarations may specify:

- Exact Version
- Minimum Version
- Version Range

Example:

```text
storage.sqlite >=1.4.0
translation.api ^2.0.0
ocr.engine 3.1.2
```

Version conflicts prevent plugin startup.

---

# Deprecation Policy

Deprecated APIs should:

- Remain available during a transition period.
- Generate diagnostics warnings.
- Be documented with replacement guidance.

Removal occurs only in a future major Plugin API version.

---

# Marketplace Compatibility

Future plugin repositories may publish:

- Supported CRAI versions
- Plugin API versions
- Supported platforms
- Release channel
- Digital signature

The Plugin Manager verifies compatibility before installation.

---

# Versioning Events

| Event | Description |
|--------|-------------|
| PluginVersionValidated | Version compatibility verified |
| PluginUpgradeStarted | Upgrade initiated |
| PluginUpgradeCompleted | Upgrade completed |
| PluginRollbackStarted | Rollback initiated |
| PluginRollbackCompleted | Rollback completed |
| VersionConflictDetected | Incompatible versions detected |

---

# Failure Handling

Version-related failures include:

- Unsupported CRAI version
- Unsupported Plugin API version
- Dependency version conflict
- Invalid version format
- Failed upgrade
- Failed rollback

Failures are reported to Diagnostics and prevent incompatible plugins from running.

---

# Architecture Invariants

1. Every plugin declares its own version.
2. Plugin API versioning is independent of plugin versioning.
3. Compatibility is verified before plugin loading.
4. Version conflicts prevent runtime activation.
5. Upgrades never bypass validation.
6. Rollback restores a previously valid state whenever possible.
7. Version management is coordinated exclusively by the Plugin Manager.

---

# Related Documents

- README.md
- PLUGIN_SYSTEM.md
- PLUGIN_API.md
- PLUGIN_LIFECYCLE.md
- PLUGIN_REGISTRY.md
- PLUGIN_DISCOVERY.md
- PLUGIN_DEPENDENCY.md
- PLUGIN_CONFIGURATION.md
- PLUGIN_SECURITY.md
