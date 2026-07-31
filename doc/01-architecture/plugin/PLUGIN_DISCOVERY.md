# Plugin Discovery

- Document: Plugin Architecture / Plugin Discovery
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines how the CRAI runtime discovers plugins before they are validated, registered and loaded.

Discovery is a read-only process. It identifies candidate plugins and collects metadata without executing plugin code.

---

# Design Principles

- Deterministic discovery
- No code execution during scanning
- Manifest-first design
- Platform independent
- Repeatable results
- Secure by default

---

# Discovery Pipeline

```text
Startup
   │
   ▼
Locate Plugin Directories
   │
   ▼
Scan Candidate Packages
   │
   ▼
Read Manifest
   │
   ▼
Basic Validation
   │
   ▼
Register Candidate
   │
   ▼
Plugin Registry
```

Only after discovery completes may the Plugin Manager begin validation and loading.

---

# Discovery Sources

Plugins may be discovered from:

- Built-in plugins
- Local plugin directory
- User plugin directory
- Enterprise-managed directory
- Future marketplace cache

The source determines installation policy, not runtime behavior.

---

# Recommended Directory Layout

```text
plugins/
├── capture/
├── ocr/
├── translation/
├── ai/
├── storage/
├── dictionary/
└── export/
```

Each plugin resides in its own directory.

---

# Plugin Package

A plugin package typically contains:

```text
translation.gemini/
├── manifest.json
├── plugin.*
├── resources/
├── localization/
└── README.md
```

The implementation file is platform-dependent.

---

# Manifest

The manifest is the entry point for discovery.

Typical fields include:

- Plugin ID
- Display Name
- Version
- Plugin API Version
- Category
- Capabilities
- Supported Platforms
- Dependencies
- Vendor

The manifest must be readable without executing plugin code.

---

# Discovery Rules

1. Scan configured plugin directories.
2. Ignore hidden or temporary files.
3. Ignore unsupported package formats.
4. Read manifest only.
5. Reject duplicate Plugin IDs.
6. Record candidate plugins in the Plugin Registry.

---

# Duplicate Handling

If multiple plugins share the same Plugin ID:

- Keep the highest-priority installation.
- Report the conflict to Diagnostics.
- Ignore lower-priority duplicates.

Priority is defined by installation policy.

---

# Invalid Plugins

A plugin is rejected during discovery if:

- Manifest is missing.
- Manifest cannot be parsed.
- Required metadata is absent.
- Plugin ID is invalid.
- Package structure is malformed.

Rejected plugins are never loaded.

---

# Discovery Events

| Event | Description |
|--------|-------------|
| DiscoveryStarted | Plugin scan started |
| PluginDiscovered | Candidate identified |
| PluginRejected | Candidate rejected |
| DiscoveryCompleted | Scan finished |

---

# Failure Handling

Discovery failures should:

- Continue scanning remaining plugins.
- Preserve valid candidates.
- Record diagnostics.
- Never stop the application unless configured as mandatory.

---

# Security Considerations

During discovery:

- Plugin code is never executed.
- Network access is not required.
- File writes are prohibited.
- Only manifests and metadata are read.

---

# Architecture Invariants

1. Discovery never executes plugin code.
2. Discovery is deterministic for identical inputs.
3. Every discovered plugin has a unique Plugin ID.
4. Only discovered plugins may enter validation.
5. Discovery is independent of plugin implementation language.
6. Registry updates occur only through the Plugin Manager.
7. Manifest data is treated as untrusted until validation completes.

---

# Related Documents

- README.md
- PLUGIN_SYSTEM.md
- PLUGIN_API.md
- PLUGIN_LIFECYCLE.md
- PLUGIN_REGISTRY.md
- PLUGIN_DEPENDENCY.md
- PLUGIN_CONFIGURATION.md
- PLUGIN_SECURITY.md
- PLUGIN_VERSIONING.md
