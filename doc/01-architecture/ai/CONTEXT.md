# AI Context

- **Document:** AI Architecture / Context
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines how contextual information is collected, prioritized and supplied to the AI Pipeline.

The Context Builder improves translation quality and consistency by assembling only the information relevant to the current request while remaining independent of any AI provider.

---

# Design Principles

- Context is relevant
- Provider independent
- Deterministic construction
- Incremental enrichment
- Token efficient
- Versioned
- Observable

---

# Context Pipeline

```text
Input
   │
   ▼
Collect Sources
   │
   ▼
Normalize
   │
   ▼
Prioritize
   │
   ▼
Merge
   │
   ▼
Token Budget Check
   │
   ▼
Context Package
```

---

# Context Sources

The Context Builder may combine information from:

- Current page
- Previous pages
- OCR results
- Reading session
- Character memory
- Glossary
- User preferences
- Project settings
- Conversation history
- Plugin-provided context

Every source is optional.

---

# Context Structure

```text
Context
├── Session
├── Document
├── Characters
├── Glossary
├── History
├── User Preferences
├── Runtime Metadata
└── Plugin Extensions
```

---

# Context Prioritization

Typical priority (highest first):

1. Current page
2. Explicit user instructions
3. Character glossary
4. Recent dialogue
5. Previous pages
6. Long-term memory
7. Project defaults

Lower-priority information may be discarded when token limits are reached.

---

# Session Context

Session context contains transient information such as:

- Current chapter
- Current page
- Reading direction
- Active language
- Translation mode

Session context exists only for the active reading session.

---

# Character Context

Character information may include:

- Name
- Aliases
- Gender
- Speaking style
- Relationships
- Preferred translations

This improves consistency across chapters.

---

# Glossary Context

Glossary entries include:

- Character names
- Locations
- Organizations
- Skills
- Items
- Domain-specific terminology

Glossary entries override generic translations where applicable.

---

# Historical Context

Historical context may include:

- Previous dialogue
- Previous narration
- Previous translations
- Recently used terminology

History is limited by configurable size and token budgets.

---

# Context Optimization

The Context Builder may:

- Remove duplicate entries
- Compress history
- Drop low-priority context
- Merge equivalent terms
- Summarize older information

Optimization must preserve semantic meaning.

---

# Token Budget

Before prompt generation, the Context Builder estimates token usage.

If limits are exceeded, context is reduced according to priority rules rather than truncating arbitrarily.

---

# Validation

Context validation includes:

- Schema validation
- Reference integrity
- Size limits
- Token estimation
- Language consistency

Invalid context is rejected before prompt generation.

---

# Observability

Metrics may include:

- Context size
- Token estimate
- Number of sources
- Build latency
- Compression ratio

Sensitive context should never be logged in plain text.

---

# Failure Handling

Possible failures include:

- Missing context source
- Corrupted memory
- Invalid glossary
- Token budget exceeded
- Context merge conflict

Recovery strategies include:

- Ignore unavailable sources
- Reduce context
- Rebuild context package
- Continue with minimal context

---

# Architecture Invariants

1. Context is assembled before prompt generation.
2. Context construction is deterministic for identical inputs.
3. Context remains provider independent.
4. Explicit user instructions always take precedence over inferred context.
5. Token budgets are enforced before model execution.
6. Context optimization preserves semantic intent.
7. Sensitive information is excluded unless explicitly required.

---

# Related Documents

- README.md
- PIPELINE.md
- STAGES.md
- REQUEST.md
- RESPONSE.md
- PROMPTS.md
- MEMORY.md
- MODELS.md
- ROUTING.md
- STREAMING.md
- RETRY.md
- FALLBACK.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
