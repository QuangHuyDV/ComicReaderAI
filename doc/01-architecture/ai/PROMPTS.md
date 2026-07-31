# Prompt Architecture

- **Document:** AI Architecture / Prompts
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines how prompts are constructed within the CRAI AI Pipeline.

Prompt construction is independent from any specific AI provider and produces deterministic, structured prompts that maximize translation quality while minimizing cost and ambiguity.

The Prompt Builder is responsible for assembling the final prompt.

---

# Design Principles

- Provider independent
- Template driven
- Context aware
- Deterministic composition
- Versioned prompts
- Reusable components
- Observable generation

---

# Prompt Pipeline

```text
Input
   │
   ▼
Context Builder
   │
   ▼
Template Selection
   │
   ▼
Prompt Composition
   │
   ▼
Validation
   │
   ▼
Final Prompt
```

---

# Prompt Structure

```text
Prompt
├── System Prompt
├── Developer Prompt
├── User Prompt
├── Context
├── Glossary
├── Formatting Rules
├── Output Schema
└── Metadata
```

Each section has a dedicated purpose and should remain logically separated.

---

# System Prompt

Defines the permanent behavior of the model.

Examples:

- Translator role
- Safety rules
- Output language
- General translation principles

System prompts are managed by CRAI and are not editable during normal execution.

---

# Developer Prompt

Defines application-specific behavior.

Examples:

- Comic translation rules
- Novel translation rules
- OCR correction strategy
- Layout preservation
- JSON output requirements

Developer prompts are version-controlled with the application.

---

# User Prompt

Represents the user's immediate request.

Examples:

- Translate this page
- Explain this paragraph
- Summarize the chapter
- Keep honorifics unchanged

User prompts never override system safety rules.

---

# Context Injection

Context improves translation quality.

Sources may include:

- Previous pages
- Character profiles
- Dialogue history
- Reading direction
- User preferences
- Project settings

Context is injected only when relevant.

---

# Glossary

Glossary ensures terminology consistency.

Entries may include:

- Character names
- Place names
- Organizations
- Skills
- Items
- Technical terms

Glossary entries have higher priority than general translation.

---

# Formatting Rules

Formatting instructions specify how results should be returned.

Examples:

- JSON schema
- Markdown
- Plain text
- Rich text
- Subtitle format

Formatting rules are independent of model implementation.

---

# Output Schema

The expected response structure.

Examples:

- TranslationResult
- SummaryResult
- OCRCorrectionResult

Responses are validated against the declared schema.

---

# Prompt Versioning

Every prompt template has:

- Template ID
- Template Version
- Compatible Pipeline Version
- Compatible Model Capabilities

Versioning enables safe evolution without breaking existing workflows.

---

# Prompt Optimization

The Prompt Builder may optimize prompts by:

- Removing duplicate context
- Compressing history
- Truncating low-priority information
- Reordering context
- Eliminating unused instructions

Optimization must preserve semantic intent.

---

# Validation

Before execution, prompts are validated for:

- Required sections
- Context size
- Token estimation
- Output schema
- Safety compliance

Invalid prompts never reach the model.

---

# Observability

Prompt generation publishes metrics including:

- Prompt size
- Estimated tokens
- Context size
- Generation time
- Template version
- Optimization actions

Prompt content itself should not be logged unless explicitly permitted.

---

# Failure Handling

Possible failures include:

- Missing template
- Invalid schema
- Context overflow
- Token limit exceeded
- Unsupported output format

Recovery may include:

- Prompt simplification
- Context reduction
- Alternative template selection
- Request rejection

---

# Architecture Invariants

1. Prompt construction is deterministic for identical inputs.
2. Prompts are provider independent.
3. System prompts always have the highest priority.
4. User instructions cannot bypass safety policies.
5. Context injection is explicit and traceable.
6. Prompt templates are versioned.
7. Every prompt is validated before model execution.

---

# Related Documents

- README.md
- PIPELINE.md
- STAGES.md
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
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
