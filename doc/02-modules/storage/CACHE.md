# Storage Cache Strategy

- Module: Storage
- Document: CACHE.md
- Version: 1.0.0
- Status: Draft

---

# Purpose

This document defines the cache architecture of the Storage Module.

Storage provides persistence mechanisms for cache data while cache ownership, invalidation rules and business semantics remain the responsibility of the corresponding business modules.

---

# Design Principles

## Cache Is an Optimization

Cache improves performance but is never the authoritative source of truth.

---

## Backend Independent

Cache implementations may use:

- In-Memory
- SQLite
- PostgreSQL
- Redis (future)
- Local Files
- Cloud Storage

Business modules remain unaware of the backend.

---

## Module Ownership

Storage persists cache objects.

Business modules decide:

- When to cache
- When to invalidate
- Cache lifetime
- Cache keys

---

# Cache Layers

```text
Application
      │
      ▼
Memory Cache
      │
      ▼
Persistent Cache
      │
      ▼
Authoritative Data Source
```

Memory cache is optional and volatile.

Persistent cache survives application restarts.

---

# Cache Types

## OCR Cache

Repository

- OCRRepository

Cache Key

- ImageHash
- OCR Engine
- OCR Revision

Cached Value

- OCR Result
- Recognition Metadata

---

## Translation Cache

Repository

- TranslationRepository

Cache Key

- Source Text
- Source Language
- Target Language
- Provider
- Model

Cached Value

- Translated Text
- Translation Metadata

---

## Presentation Cache

Repository

- PresentationRepository

Cache Key

- Presentation Mode
- Layout
- Style
- Translation Revision

Cached Value

- Render Layout
- Bubble Geometry
- Presentation Metadata

---

## Image Cache

Repository

- ImageRepository

Cache Key

- Image Hash
- Processing Revision

Cached Value

- Cropped Image
- Thumbnail
- Processed Asset

---

# Cache Key Rules

Cache keys should be:

- Deterministic
- Immutable
- Collision resistant
- Independent of backend

Hash-based keys are recommended for large payloads.

---

# Cache Invalidation

Storage executes invalidation requests but does not determine invalidation policy.

Typical invalidation triggers:

- OCR engine changed
- Translation provider changed
- Target language changed
- Presentation mode changed
- Object deleted
- Schema migration

---

# Cache Lifetime

Storage supports both:

- Session cache
- Persistent cache

Expiration policy is repository specific.

Possible strategies:

- No expiration
- Time-to-live (TTL)
- Revision based
- Manual invalidation

---

# Cache Consistency

Requirements:

1. Cached data must correspond to the current revision.
2. Invalid cache entries must never overwrite authoritative data.
3. Cache misses are acceptable.
4. Stale cache entries may be discarded safely.

---

# Cache Eviction

Possible eviction strategies:

- Least Recently Used (LRU)
- Least Frequently Used (LFU)
- Time-based
- Size-based
- Manual

Strategy selection depends on backend implementation.

---

# Cache Warm-Up

Optional warm-up may preload:

- Frequently translated text
- Recent reading sessions
- Frequently accessed images
- OCR results

Warm-up must not block application startup.

---

# Cache Metrics

Storage may expose:

- Hit Rate
- Miss Rate
- Eviction Count
- Cache Size
- Average Lookup Time
- Average Write Time

Diagnostics consumes these metrics.

---

# Architecture Invariants

1. Cache never replaces authoritative storage.
2. Cache keys are deterministic.
3. Cache invalidation is initiated by business modules.
4. Persistent cache survives restart when supported.
5. Cache failures never corrupt authoritative data.
6. Cache implementation is backend independent.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- REPOSITORIES.md
- SCHEMA.md
- MIGRATION.md
- BACKENDS.md
