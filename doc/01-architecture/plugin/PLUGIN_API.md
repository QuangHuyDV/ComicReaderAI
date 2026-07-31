# Plugin API

- Document: Plugin Architecture / Plugin API
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the public API contract that every CRAI plugin must implement.

The Plugin API provides a stable, implementation-independent interface between the CRAI Core and all plugins.

---

# Design Principles

- Stable contracts
- Capability-based architecture
- Backward compatibility
- Lifecycle consistency
- Runtime independence
- No direct plugin-to-plugin dependencies

---

# Core Plugin Contract

Every plugin exposes the following logical interface:

```text
Plugin
├── Metadata
├── Capabilities
├── Configuration
├── Initialize()
├── Start()
├── Stop()
├── Health()
└── Dispose()
```

The concrete programming language or framework is implementation-specific.

---

# Metadata

Every plugin must declare:

- Plugin ID
- Display Name
- Version
- Plugin API Version
- Vendor
- Description
- License
- Supported Platforms

Example:

```text
id: translation.gemini
version: 1.2.0
api: v1
```

---

# Capability Contract

A plugin advertises one or more capabilities.

Examples:

```text
OCR
Translate
Capture
Summarize
Explain
DictionaryLookup
Export
Storage
```

The core selects plugins by capability rather than implementation name.

---

# Lifecycle API

## Initialize()

Prepare resources without starting background work.

Called once after validation.

---

## Start()

Begin normal operation.

Register event handlers and accept requests.

---

## Stop()

Finish in-flight work and release runtime resources.

---

## Dispose()

Release all remaining resources before unloading.

---

## Health()

Return the current health state:

- Healthy
- Degraded
- Unhealthy
- Unknown

---

# Configuration Contract

Plugins receive configuration through the Plugin Manager.

Configuration may include:

- API keys
- Timeouts
- Retry policy
- Model selection
- Cache options

Plugins must never read application configuration directly.

---

# Event Contract

Plugins may:

Consume:

- Public application events

Publish:

- Public plugin events

All communication uses the Event Bus.

---

# Service Contract

Plugins may request shared services through public contracts only.

Examples:

- Diagnostics
- Storage
- Scheduler
- Work Queue

Direct access to internal implementations is prohibited.

---

# Error Contract

Plugins return standardized errors.

Typical categories:

- ConfigurationError
- InitializationError
- RuntimeError
- CapabilityUnavailable
- Timeout
- AuthenticationFailed

Implementation-specific exceptions must not cross the API boundary.

---

# Version Compatibility

Each plugin declares:

- Plugin API version
- Minimum supported CRAI version
- Maximum supported CRAI version (optional)

The Plugin Manager validates compatibility before loading.

---

# Security Contract

Plugins execute with the minimum permissions required.

Access to:

- Network
- File System
- Clipboard
- Storage
- Runtime Secrets

is granted explicitly by the Plugin Manager.

---

# API Evolution

Rules:

1. Existing public contracts remain backward compatible within the same major API version.
2. Breaking changes require a new major Plugin API version.
3. Deprecated APIs remain available for a defined transition period.

---

# Architecture Invariants

1. Plugins implement the public Plugin API.
2. Plugins never call each other directly.
3. Capabilities are declared explicitly.
4. Lifecycle methods are deterministic.
5. Plugin Manager owns lifecycle orchestration.
6. Public APIs are implementation independent.
7. Errors crossing the API boundary are standardized.

---

# Related Documents

- README.md
- PLUGIN_SYSTEM.md
- PLUGIN_LIFECYCLE.md
- PLUGIN_REGISTRY.md
- PLUGIN_DISCOVERY.md
- PLUGIN_DEPENDENCY.md
- PLUGIN_CONFIGURATION.md
- PLUGIN_SECURITY.md
- PLUGIN_VERSIONING.md
