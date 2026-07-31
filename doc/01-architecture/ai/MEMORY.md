# AI Memory

- **Document:** AI Architecture / Memory
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines the memory architecture used by the CRAI AI Pipeline.

The Memory subsystem preserves information across requests and reading sessions to improve translation consistency, reduce repeated reasoning, and provide long-term contextual awareness without coupling the pipeline to a specific AI provider.

---

# Design Principles

- Provider independent
- Layered memory model
- Context driven
- Persistent where appropriate
- Token efficient
- Observable
- Privacy first

---

# Memory Architecture

```text
                AI Pipeline
                     │
                     ▼
              Memory Manager
                     │
 ┌───────────┬────────────┬─────────────┬────────────┐
 ▼           ▼            ▼             ▼
Session   Character   Glossary    Long-term
Memory     Memory      Memory       Memory
```

The Memory Manager is responsible for loading, updating and persisting memory.

---

# Memory Types

## Session Memory

Short-lived information for the current reading session.

Examples:

- Current chapter
- Current page
- Active language
- Temporary terminology
- Recent dialogue

Destroyed when the session ends.

---

## Character Memory

Stores character-specific knowledge.

Examples:

- Name
- Aliases
- Gender
- Speech style
- Honorifics
- Relationships

Character memory should remain consistent across the project.

---

## Glossary Memory

Stores approved terminology.

Examples:

- Character names
- Locations
- Organizations
- Skills
- Items
- Technical terms

Glossary entries override generic translations.

---

## Long-Term Memory

Stores persistent knowledge.

Examples:

- User translation preferences
- Preferred writing style
- Confirmed terminology
- Project conventions

Loaded when relevant.

---

# Memory Lifecycle

```text
Create
   │
   ▼
Load
   │
   ▼
Read
   │
   ▼
Update
   │
   ▼
Validate
   │
   ▼
Persist
```

---

# Memory Sources

Memory may be created from:

- User input
- Translation results
- Confirmed glossary entries
- Manual edits
- Project configuration
- Plugin extensions

Only validated information should become persistent memory.

---

# Memory Retrieval

The Memory Manager retrieves only information relevant to the current request.

Selection factors include:

- Active project
- Current language
- Current chapter
- Character references
- Token budget

Irrelevant memory is excluded.

---

# Memory Updates

Memory updates may occur after:

- User correction
- Confirmed translation
- Character introduction
- Glossary modification
- Project import

Updates must be validated before persistence.

---

# Memory Persistence

Persistent memory is stored by the Storage module.

Possible storage backends:

- SQLite
- PostgreSQL
- Local files
- Cloud storage

The AI Pipeline never accesses storage directly.

---

# Memory Validation

Validation includes:

- Schema validation
- Duplicate detection
- Reference integrity
- Language consistency
- Version compatibility

Invalid memory is rejected.

---

# Observability

Metrics include:

- Memory hits
- Memory misses
- Retrieval latency
- Update count
- Persistence latency
- Memory size

Sensitive content should not be logged.

---

# Failure Handling

Possible failures:

- Storage unavailable
- Corrupted memory
- Duplicate entries
- Invalid schema
- Persistence failure

Recovery strategies:

- Continue without optional memory
- Rebuild indexes
- Restore previous snapshot
- Retry persistence

---

# Architecture Invariants

1. Memory is independent of any AI provider.
2. Session memory never persists beyond its lifecycle unless promoted.
3. Persistent memory is validated before storage.
4. Only relevant memory is injected into prompts.
5. Memory retrieval respects token budgets.
6. Storage responsibilities belong to the Storage module.
7. Memory updates are traceable and auditable.

---

# Related Documents

- README.md
- PIPELINE.md
- STAGES.md
- REQUEST.md
- RESPONSE.md
- PROMPTS.md
- CONTEXT.md
- MODELS.md
- ROUTING.md
- STREAMING.md
- RETRY.md
- FALLBACK.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
