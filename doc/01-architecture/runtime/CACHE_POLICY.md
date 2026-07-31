# Runtime Cache Policy

> Project: CRAI
> Version: 0.1
> Status: Architecture Draft

---

# 1. Purpose

This document defines how runtime data is cached, reused, validated, expired, and protected.

Caching exists to avoid repeating expensive work while preserving correctness.

The cache is part of the runtime execution pipeline rather than a standalone storage system.

---

# 2. Goals

The cache system must:

- minimize repeated computation
- reduce OCR cost
- reduce translation cost
- preserve correctness
- avoid stale reuse
- support multiple providers
- support provider upgrades
- support glossary changes
- support model changes
- support cancellation
- support bounded memory usage

---

# 3. Cache Philosophy

Cache is a performance optimization.

It must never become the source of truth.

The source of truth always remains:

Revision

↓

Pipeline

↓

Runtime

Cache may disappear at any time.

The runtime must continue functioning correctly.

---

# 4. Cache Layers

CRAI defines multiple cache layers.

```text
Revision Cache

↓

OCR Cache

↓

Layout Cache

↓

Translation Cache

↓

Presentation Cache
```

Each layer owns only its own artifacts.

---

# 5. Revision Cache

Stores immutable revision metadata.

Examples

- checksum
- image fingerprint
- capture timestamp

Never stores mutable runtime state.

---

# 6. OCR Cache

Stores completed OCR results.

Key contains:

Image Fingerprint

OCR Provider

OCR Version

Language

Preprocessing Version

Output:

OCR Result

---

# 7. Layout Cache

Stores:

- detected regions
- reading order
- semantic blocks

Depends on:

OCR Result

Layout Algorithm Version

---

# 8. Translation Cache

Stores translated Translation Units.

Depends on:

Source Text

Provider

Model

Prompt Version

Glossary Version

Language Pair

Context Version

Output:

Translation Result

---

# 9. Presentation Cache

Stores UI-ready Presentation Models.

Depends on:

Presentation Version

Translation Result

Theme

Reader Mode

---

# 10. Cache Keys

Every cache entry must use deterministic keys.

Example:

```text
Translation

↓

SHA256(

Source Text

+

Language Pair

+

Provider

+

Model Version

+

Prompt Version

+

Glossary Version

)
```

Changing any dependency invalidates the cache.

---

# 11. Cache Lookup

Every expensive runtime stage follows:

```text
Need Result

↓

Cache Lookup

↓

Hit?

↓

Yes

↓

Reuse

↓

No

↓

Compute

↓

Store
```

---

# 12. Cache Validation

Before using cache:

Validate:

Provider Version

Model Version

Algorithm Version

Language

Revision Compatibility

Checksum

If any validation fails:

Treat as Cache Miss.

---

# 13. Cache Ownership

Each stage owns its own cache.

OCR owns OCR cache.

Translation owns Translation cache.

Presentation owns Presentation cache.

No stage writes another stage's cache.

---

# 14. Cache Lifetime

Runtime cache is temporary.

Suggested lifetimes:

Revision Cache

Current Session

OCR Cache

Current Session

Translation Cache

Multiple Sessions

Presentation Cache

Current Session

Persistent cache policy belongs to Storage.

---

# 15. Memory Cache vs Persistent Cache

Memory Cache

Fast

Lost on restart

Persistent Cache

Disk

Survives restart

Runtime documents describe only Memory Cache.

Persistent cache belongs to Storage documentation.

---

# 16. Cache Population

Only successful results may populate cache.

Never cache:

Canceled

Failed

Partial

Corrupted

Unknown State

---

# 17. Cache Eviction

Eviction may occur because of:

Memory Pressure

Session End

LRU

TTL

Manual Clear

Provider Upgrade

Model Upgrade

---

# 18. Cache and Cancellation

Canceled work must never populate cache.

Late stale results must never overwrite valid cache entries.

---

# 19. Cache Metrics

Expose:

Hit Rate

Miss Rate

Eviction Count

Memory Usage

Average Lookup Time

Average Insert Time

---

# 20. Security

Cache must never contain:

API Keys

Provider Secrets

User Credentials

Private Tokens

Sensitive runtime information.

---

# 21. MVP Policy

For MVP implement:

Revision Cache

OCR Cache

Translation Cache

Memory-only

LRU eviction

No persistent cache

---

# 22. Related Documents

- MEMORY_MODEL.md
- RESOURCE_LIFECYCLE.md
- PERFORMANCE_MODEL.md
- DATA_FLOW.md
- PIPELINE_RUNTIME.md

---

# 23. Summary

Every runtime stage should check cache before computation.

Cache correctness is more important than cache hit rate.

The runtime must always be able to function correctly without any cached data.