# AI Response

- **Document:** AI Architecture / Response
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines the canonical AI Response produced by the CRAI AI Pipeline.

The AI Response provides a provider-independent representation of model output that can be validated, transformed and rendered consistently regardless of the underlying AI service.

---

# Design Principles

- Provider independent
- Structured output
- Immutable after validation
- Traceable
- Serializable
- Extensible

---

# Response Lifecycle

```text
Model Execution
      │
      ▼
Raw Provider Response
      │
      ▼
Normalization
      │
      ▼
Validation
      │
      ▼
Post Processing
      │
      ▼
Rendering
```

---

# Response Structure

```text
AI Response
├── Metadata
├── Result
├── Usage
├── Diagnostics
├── Warnings
├── Trace Information
└── Provider Information
```

---

# Metadata

Metadata identifies the response.

Typical fields:

- Request ID
- Response ID
- Timestamp
- Pipeline Version
- Schema Version

---

# Result

The normalized AI output.

Examples:

- Translated text
- OCR correction
- Summary
- Explanation
- Structured JSON

The Result section is independent of provider-specific formats.

---

# Usage

Execution statistics may include:

- Input Tokens
- Output Tokens
- Total Tokens
- Estimated Cost
- Latency
- Retry Count

These values support budgeting and observability.

---

# Diagnostics

Runtime diagnostics include:

- Validation status
- Processing stages
- Retry history
- Provider attempts
- Processing duration

---

# Warnings

Warnings indicate non-fatal issues.

Examples:

- Context truncated
- Budget limit approached
- Partial response
- Fallback provider used
- Low OCR confidence

Warnings do not invalidate the response.

---

# Trace Information

Trace data supports distributed diagnostics.

Examples:

- Trace ID
- Span ID
- Parent Span
- Stage Timeline
- Correlation ID

---

# Provider Information

Provider metadata may include:

- Provider Name
- Model Name
- Model Version
- Region
- Response Format

Provider information is informational and must not affect downstream business logic.

---

# Response Validation

Validation verifies:

- Schema compliance
- Required fields
- JSON structure
- Language consistency
- Safety policy
- Business rules

Invalid responses are rejected or repaired before rendering.

---

# Response Flow

```text
Provider
     │
     ▼
Normalize
     │
     ▼
Validate
     │
     ▼
Post Process
     │
     ▼
Cache
     │
     ▼
Renderer
```

---

# Failure Handling

Possible failures:

- Invalid response format
- Empty response
- Malformed JSON
- Safety violation
- Unsupported language
- Provider error

Recovery strategies include:

- Retry
- Response repair
- Provider fallback
- User-visible error

---

# Architecture Invariants

1. Every provider response is normalized before use.
2. Rendering consumes only validated responses.
3. Provider-specific formats never escape the pipeline.
4. Usage metrics accompany every completed response when available.
5. Trace information remains linked to the originating request.
6. Post-processing never modifies provider metadata.
7. Response schemas are versioned for compatibility.

---

# Related Documents

- README.md
- PIPELINE.md
- STAGES.md
- REQUEST.md
- PROMPTS.md
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
