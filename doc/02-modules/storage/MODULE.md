# Storage Module

- Module: Storage
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

The Storage Module is the single persistence layer of CRAI.

It provides a unified abstraction for storing, loading, updating and deleting application data without exposing implementation details such as SQLite, PostgreSQL, files, or cloud storage.

Storage owns persistence mechanics only. It never owns business rules.

---

# Responsibilities

The Storage Module is responsible for:

- Persisting application data.
- Loading persisted data.
- Managing transactions.
- Coordinating storage backends.
- Managing repository implementations.
- Supporting schema migration.
- Providing cache persistence.
- Managing object version metadata.
- Ensuring data integrity.
- Providing backup and restore capabilities.

---

# Out of Scope

The Storage Module is NOT responsible for:

- OCR execution.
- Translation.
- Text processing.
- Presentation rendering.
- Reading session orchestration.
- Preference validation.
- Business rule evaluation.
- AI decision making.

---

# Core Principles

## Single Persistence Layer

All persistent data flows through Storage.

---

## Business Logic Free

Storage stores data exactly as instructed.

It never interprets business meaning.

---

## Backend Independence

Consumers interact with repositories rather than database engines.

Supported backends may include:

- SQLite
- PostgreSQL
- In-Memory
- Local Files
- Cloud Object Storage

---

## Repository Pattern

Storage exposes repositories instead of database tables.

Typical repositories:

- PreferenceRepository
- SessionRepository
- OCRRepository
- TranslationRepository
- PresentationRepository
- ImageRepository
- AIMemoryRepository
- DiagnosticsRepository

---

# Owned Domain

Storage owns persistence for:

- Preferences
- Reading Sessions
- OCR Results
- Translation Cache
- Presentation Cache
- Images
- AI Memory
- Diagnostics
- Metadata

It does not own the semantics of these objects.

---

# Data Categories

## Preferences

Persistent global and source preferences.

---

## Reading Sessions

Reading progress, resume position and session metadata.

---

## OCR Results

Reusable OCR output indexed by image identity.

---

## Translation Cache

Translated text indexed by normalized input, provider and language.

---

## Presentation Cache

Cached rendering artifacts.

---

## Images

Captured images, crops, thumbnails and processed assets.

---

## AI Memory

Terminology, character memory and story context.

---

## Diagnostics

Logs, metrics and performance statistics.

---

# Transactions

Storage supports atomic transactions where required.

A transaction must:

- Commit completely.
- Roll back on failure.
- Preserve consistency.

---

# Data Integrity

Storage guarantees:

- Atomic writes.
- Consistent reads.
- Referential integrity where applicable.
- Version-aware persistence.

---

# Backend Architecture

```text
Application
      │
      ▼
Repositories
      │
      ▼
Storage Interface
      │
 ┌────┼────────────┐
 ▼    ▼            ▼
SQLite PostgreSQL InMemory
      │
      ▼
Cloud Storage
```

---

# Relationship with Preferences

Preferences owns validation and configuration semantics.

Storage only persists preference records.

---

# Relationship with Reading Session

Reading Session requests persistence of session state.

Storage never controls session lifecycle.

---

# Relationship with Translation

Translation requests cache lookup and persistence.

Storage never decides cache invalidation policy.

---

# Relationship with Diagnostics

Diagnostics records metrics through Storage.

Storage does not interpret telemetry.

---

# Event Ownership

Storage publishes events describing persistence lifecycle such as object creation, update, deletion and migration.

Detailed definitions are provided in EVENTS.md.

---

# State Ownership

Storage owns its operational lifecycle:

- Initializing
- Ready
- Transaction
- Migrating
- Recovering
- Shutdown

Detailed definitions are provided in STATES.md.

---

# Error Ownership

Storage owns persistence-related errors including:

- StorageUnavailable
- ObjectNotFound
- DuplicateKey
- TransactionFailed
- SerializationFailed
- MigrationFailed

Detailed definitions are provided in ERRORS.md.

---

# Design Principles

## Separation of Concerns

Persistence is isolated from business behavior.

---

## Replaceable Backends

Changing the storage engine must not affect business modules.

---

## Repository First

Consumers interact with repositories instead of physical databases.

---

## Consistency Before Performance

Correctness is prioritized over optimization.

---

## Version Awareness

Persistent data remains compatible across schema versions through migration.

---

# Architecture Invariants

1. Storage never executes business logic.
2. All persistent writes occur through Storage.
3. Repositories abstract physical storage.
4. Transactions are atomic.
5. Persistence backends are interchangeable.
6. Storage never validates business rules.
7. Storage never resolves preferences.
8. Storage never orchestrates processing modules.

---

# Related Documents

- CONTRACT.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
- REPOSITORIES.md
- SCHEMA.md
- CACHE.md
- MIGRATION.md
- BACKENDS.md
