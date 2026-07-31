# Storage Logical Schema

- Module: Storage
- Document: SCHEMA.md
- Version: 1.0.0
- Status: Draft

---

# Purpose

This document defines the logical data schema of the Storage Module.

The schema describes **what data exists**, **how it is logically organized**, and **which repository owns it**.

It intentionally does **not** define SQL tables, indexes, ORM models, or backend-specific structures.

---

# Design Principles

## Logical First

This document describes logical entities rather than physical database layouts.

---

## Backend Independent

The logical schema must remain stable regardless of whether the backend is:

- SQLite
- PostgreSQL
- In-Memory
- Local Files
- Cloud Object Storage

---

## Single Ownership

Every entity belongs to exactly one repository.

---

# Schema Overview

```text
Storage
│
├── Preferences
├── Reading Sessions
├── OCR Results
├── Translation Cache
├── Presentation Cache
├── Images
├── AI Memory
├── Diagnostics
└── Metadata
```

---

# Preferences

Repository

- PreferenceRepository

Logical Entity

```text
Preference
├── Key
├── Value
├── Scope
├── Revision
├── UpdatedAt
└── SchemaVersion
```

---

# Reading Sessions

Repository

- SessionRepository

Logical Entity

```text
ReadingSession
├── SessionId
├── Source
├── Chapter
├── Page
├── ScrollPosition
├── CreatedAt
├── UpdatedAt
└── Metadata
```

---

# OCR Results

Repository

- OCRRepository

Logical Entity

```text
OCRResult
├── ImageHash
├── Regions
├── TextBlocks
├── Confidence
├── OCRRevision
└── CreatedAt
```

Primary identity is ImageHash.

---

# Translation Cache

Repository

- TranslationRepository

Logical Entity

```text
TranslationCache
├── CacheKey
├── SourceLanguage
├── TargetLanguage
├── Provider
├── Model
├── SourceText
├── TranslatedText
├── CreatedAt
└── Revision
```

Suggested logical cache key:

```text
Hash(
    SourceText +
    SourceLanguage +
    TargetLanguage +
    Provider +
    Model
)
```

---

# Presentation Cache

Repository

- PresentationRepository

Logical Entity

```text
PresentationCache
├── PresentationKey
├── Layout
├── Style
├── Font
├── RenderData
├── Revision
└── CreatedAt
```

---

# Images

Repository

- ImageRepository

Logical Entity

```text
ImageAsset
├── ImageId
├── Hash
├── Width
├── Height
├── Format
├── StorageLocation
├── CreatedAt
└── Metadata
```

---

# AI Memory

Repository

- AIMemoryRepository

Logical Entity

```text
MemoryEntry
├── MemoryId
├── Category
├── Context
├── Content
├── Revision
├── CreatedAt
└── UpdatedAt
```

---

# Diagnostics

Repository

- DiagnosticsRepository

Logical Entity

```text
DiagnosticRecord
├── RecordId
├── Category
├── Severity
├── Timestamp
├── Payload
└── Metadata
```

---

# Metadata

Repository

- MetadataRepository

Logical Entity

```text
StorageMetadata
├── StorageVersion
├── SchemaVersion
├── Backend
├── MigrationRevision
├── CreatedAt
└── UpdatedAt
```

---

# Relationships

```text
Reading Session
        │
        ▼
Translation Cache

Reading Session
        │
        ▼
Presentation Cache

OCR Result
        │
        ▼
Translation Cache

Preferences
        │
        ▼
Reading Session
```

Relationships are logical references rather than physical foreign keys.

---

# Versioning

Every persisted entity should include:

- SchemaVersion
- CreatedAt
- UpdatedAt (when applicable)
- Revision (when applicable)

---

# Identity Rules

1. Every entity has one stable identifier.
2. Cache entities may use deterministic hash keys.
3. Identifiers remain immutable.
4. Repository ownership never changes.

---

# Future Schema

Potential future entities:

- UserProfile
- PluginConfiguration
- DownloadTask
- DictionaryEntry
- AnalyticsSnapshot

---

# Related Documents

- MODULE.md
- CONTRACT.md
- REPOSITORIES.md
- CACHE.md
- MIGRATION.md
- BACKENDS.md
