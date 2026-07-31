# Storage Repositories

- Module: Storage
- Document: REPOSITORIES.md
- Version: 1.0.0
- Status: Draft

---

# Purpose

This document defines the logical repositories owned by the Storage Module.

A repository represents a logical persistence boundary for one type of data.
Repositories abstract physical storage engines and expose backend-independent operations.

Business modules communicate only with repositories, never with SQLite, PostgreSQL, files or cloud APIs directly.

---

# Design Principles

## Repository per Aggregate

Each repository owns one logical data domain.

## Backend Independent

Repository contracts remain identical regardless of backend.

## Business Logic Free

Repositories store and retrieve data only.

They never:

- Translate text
- Run OCR
- Resolve preferences
- Execute AI
- Orchestrate workflows

---

# Repository Hierarchy

```text
Storage
│
├── PreferenceRepository
├── SessionRepository
├── OCRRepository
├── TranslationRepository
├── PresentationRepository
├── ImageRepository
├── AIMemoryRepository
├── DiagnosticsRepository
└── MetadataRepository
```

---

# PreferenceRepository

## Owns

- Global Preferences
- Source Preferences
- Preference Profiles
- Preference Revisions

## Used By

- Preferences Module

---

# SessionRepository

## Owns

- Reading Sessions
- Resume Position
- Current Chapter
- Scroll Position
- Session Metadata

## Used By

- Reading Session

---

# OCRRepository

## Owns

- OCR Results
- OCR Metadata
- OCR Cache
- Recognition Revision

## Used By

- Recognition

---

# TranslationRepository

## Owns

- Translation Cache
- Translation Metadata
- Provider Information
- Translation Revision

## Used By

- Translation

---

# PresentationRepository

## Owns

- Render Cache
- Bubble Layout Cache
- Presentation Metadata

## Used By

- Presentation

---

# ImageRepository

## Owns

- Screenshots
- Cropped Images
- Processed Images
- Thumbnails

## Used By

- Capture
- Presentation

---

# AIMemoryRepository

## Owns

- Character Memory
- Story Memory
- Terminology
- Prompt Context

## Used By

- Translation
- Future AI Modules

---

# DiagnosticsRepository

## Owns

- Performance Metrics
- Logs
- Crash Reports
- Latency Statistics

## Used By

- Diagnostics

---

# MetadataRepository

## Owns

Shared metadata including:

- Schema Versions
- Repository Versions
- Migration History
- Storage Health
- Backend Information

---

# Common Repository Operations

Every repository should support the logical operations below where applicable:

- Save
- Get
- Update
- Delete
- Exists
- Query
- Count
- BatchSave

Not every repository must implement every operation if it is not meaningful.

---

# Repository Dependencies

```text
Business Module
        │
        ▼
Repository
        │
        ▼
Storage Interface
        │
        ▼
Backend
```

Business modules never bypass repositories.

---

# Repository Ownership Rules

1. Every persisted object belongs to exactly one repository.
2. Repositories never share ownership of the same aggregate.
3. Cross-repository coordination is handled by Storage transactions.
4. Business modules must not access backend implementations directly.
5. Repository interfaces remain stable across backend changes.

---

# Future Repositories

Potential future repositories include:

- DictionaryRepository
- PluginRepository
- DownloadRepository
- UserProfileRepository
- AnalyticsRepository

---

# Related Documents

- MODULE.md
- CONTRACT.md
- SCHEMA.md
- CACHE.md
- MIGRATION.md
- BACKENDS.md
