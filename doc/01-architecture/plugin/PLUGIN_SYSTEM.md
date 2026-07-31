# Plugin System

- Document: Plugin Architecture / Plugin System
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

The Plugin System enables CRAI to extend its capabilities without modifying the core application.

All integrations with OCR engines, translation providers, AI models, capture mechanisms, storage backends and future extensions are implemented as plugins.

The core application depends on plugin contracts rather than concrete implementations.

---

# Design Goals

- Extensible architecture
- Loose coupling
- Stable public contracts
- Runtime discovery
- Safe loading and unloading
- Version compatibility
- Capability-based selection

---

# High-Level Architecture

```text
                    CRAI Core
                         │
                         ▼
                 Plugin Manager
        ┌──────────┼──────────┐
        ▼          ▼          ▼
 Plugin Registry Loader   Capability Index
        │
        ▼
 ┌─────────────────────────────────────┐
 │ OCR Plugins                         │
 │ Translation Plugins                 │
 │ AI Plugins                          │
 │ Capture Plugins                     │
 │ Storage Plugins                     │
 │ Dictionary Plugins                  │
 │ Export Plugins                      │
 └─────────────────────────────────────┘
```

The Plugin Manager is the only runtime component responsible for plugin lifecycle management.

---

# Supported Plugin Categories

## Capture Plugins

Acquire images from supported sources.

Examples:

- Browser Extension
- Desktop Overlay
- Screen Capture
- Android
- iOS

---

## OCR Plugins

Extract text from images.

Examples:

- PaddleOCR
- Tesseract
- EasyOCR
- Windows OCR
- Apple Vision

---

## Translation Plugins

Translate recognized text.

Examples:

- Gemini
- GPT
- Claude
- DeepL
- Google Translate

---

## AI Plugins

Provide AI-powered capabilities such as:

- Summarization
- Explanation
- Conversation
- Context analysis

---

## Storage Plugins

Implement persistence backends.

Examples:

- SQLite
- PostgreSQL
- Cloud Storage

---

## Dictionary Plugins

Provide glossary, terminology and custom dictionaries.

---

## Export Plugins

Export translated content to supported formats.

---

# Core Components

## Plugin Manager

Coordinates the complete plugin lifecycle.

Responsibilities:

- Discovery
- Validation
- Loading
- Initialization
- Health monitoring
- Shutdown

---

## Plugin Registry

Maintains metadata for:

- Installed plugins
- Enabled plugins
- Disabled plugins
- Versions
- Dependencies
- Capabilities

---

## Plugin Loader

Loads plugin implementations using the public Plugin API.

The loader hides platform-specific loading mechanisms.

---

## Capability Index

Maps capabilities to available plugins.

Example:

```text
Translate
 ├── Gemini
 ├── GPT
 └── Google Translate

OCR
 ├── PaddleOCR
 └── Tesseract
```

The application selects plugins by capability rather than by implementation name.

---

# Runtime Flow

```text
Application Startup
        │
        ▼
Discover Plugins
        │
        ▼
Validate Manifest
        │
        ▼
Resolve Dependencies
        │
        ▼
Load Plugin
        │
        ▼
Initialize
        │
        ▼
Register Capabilities
        │
        ▼
Running
```

---

# Plugin Communication

Plugins never communicate directly.

All interactions occur through:

- Public Contracts
- Event Bus
- Plugin Manager

This prevents tight coupling between plugins.

---

# Failure Isolation

A plugin failure should:

- Not crash the application.
- Not corrupt other plugins.
- Be reported to Diagnostics.
- Allow graceful degradation when possible.

---

# Version Compatibility

Each plugin declares:

- Plugin API version
- Plugin version
- Supported CRAI version
- Required capabilities
- Optional dependencies

Compatibility is verified before loading.

---

# Security Principles

Plugins operate with the minimum permissions required.

The Plugin Manager controls access to:

- Network
- File System
- Clipboard
- Storage
- Runtime Configuration

Sensitive resources are never exposed directly.

---

# Architecture Invariants

1. CRAI Core never depends on plugin implementations.
2. Plugins depend only on public contracts.
3. Plugins are selected by capability, not by name.
4. Plugin lifecycle is managed exclusively by the Plugin Manager.
5. Plugin failures remain isolated.
6. Public APIs remain backward compatible whenever possible.
7. All plugin communication uses contracts or events.

---

# Related Documents

- README.md
- PLUGIN_API.md
- PLUGIN_LIFECYCLE.md
- PLUGIN_REGISTRY.md
- PLUGIN_DISCOVERY.md
- PLUGIN_DEPENDENCY.md
- PLUGIN_CONFIGURATION.md
- PLUGIN_SECURITY.md
- PLUGIN_VERSIONING.md
