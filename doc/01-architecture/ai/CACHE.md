# AI Cache

- **Document:** AI Architecture / Cache
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines the caching architecture for the CRAI AI Pipeline.

The Cache subsystem reduces latency, provider requests and execution cost by reusing deterministic AI results when it is safe to do so.

---

# Design Principles

- Provider independent
- Deterministic
- Cache policy driven
- Cost efficient
- Observable
- Consistent
- Safe invalidation

---

# Architecture

```text
AI Request
     │
     ▼
Cache Key Builder
     │
     ▼
Cache Lookup
 ┌────┴────┐
 │ Hit     │ Miss
 ▼         ▼
Return   Execute AI
 Result      │
             ▼
      Response Validator
             │
             ▼
        Cache Writer
```

The pipeline checks the cache before routing and model execution.

---

# Cacheable Data

Typical cache entries include:

- Translation results
- OCR correction
- Prompt templates
- Model metadata
- Glossary snapshots
- Context summaries
- Token estimations

Only deterministic outputs should be cached.

---

# Cache Key

A cache key may include:

- Request hash
- Prompt version
- Context fingerprint
- Model capability
- Language pair
- Project identifier

Provider-specific identifiers should not be required.

---

# Cache Levels

Supported cache levels:

- In-memory
- Session cache
- Local persistent cache
- Shared distributed cache

Each level has independent eviction policies.

---

# Cache Lifecycle

```text
Create Key
    │
    ▼
Lookup
    │
 ┌──┴───┐
 │Hit   │Miss
 ▼      ▼
Return Execute
          │
          ▼
      Validate
          │
          ▼
        Store
```

---

# Cache Policies

Policies may define:

- Time-to-live (TTL)
- Maximum size
- Eviction strategy
- Refresh behavior
- Read-through
- Write-through
- Cache bypass

Policies are configurable.

---

# Invalidation

Cache entries may be invalidated when:

- Prompt version changes
- Glossary changes
- Context schema changes
- User corrections
- Project settings change
- TTL expires

Invalidation should target only affected entries.

---

# Consistency

The cache should guarantee:

- Valid response format
- Version compatibility
- Deterministic lookup
- Safe concurrent access

The cache is an optimization layer, never the source of truth.

---

# Observability

Metrics include:

- Cache hit rate
- Cache miss rate
- Lookup latency
- Write latency
- Eviction count
- Saved provider requests
- Estimated cost savings

---

# Failure Handling

Possible failures:

- Cache unavailable
- Corrupted entry
- Serialization failure
- Version mismatch
- Storage failure

Recovery strategies:

- Ignore cache
- Rebuild entry
- Remove invalid entry
- Continue with live execution

---

# Architecture Invariants

1. Cache lookup occurs before model execution.
2. Cached data must be version compatible.
3. Cache failures never block request execution.
4. Cache keys are deterministic.
5. Invalidation is explicit and traceable.
6. The cache never replaces persistent storage.
7. Cache metrics are exported to Diagnostics.

---

# Related Documents

- README.md
- PIPELINE.md
- REQUEST.md
- RESPONSE.md
- CONTEXT.md
- MEMORY.md
- MODELS.md
- ROUTING.md
- STREAMING.md
- RETRY.md
- FALLBACK.md
- COST_CONTROL.md
- SAFETY.md
- OBSERVABILITY.md
