# AI Streaming

- **Document:** AI Architecture / Streaming
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines how the CRAI AI Pipeline processes streaming responses from AI providers.

Streaming enables incremental delivery of model output, reducing perceived latency while maintaining the same business logic, validation rules and rendering behavior as non-streaming requests.

---

# Design Principles

- Provider independent
- Incremental processing
- Non-blocking execution
- Deterministic assembly
- Graceful degradation
- Observable
- Cancelable

---

# Streaming Architecture

```text
AI Request
     │
     ▼
Model Execution
     │
     ▼
Streaming Adapter
     │
     ▼
Chunk Parser
     │
     ▼
Response Assembler
     │
     ▼
Incremental Validator
     │
     ▼
Renderer
     │
     ▼
User Interface
```

The Streaming Adapter normalizes provider-specific streaming protocols.

---

# Streaming Lifecycle

```text
Start
  │
  ▼
Open Stream
  │
  ▼
Receive Chunks
  │
  ▼
Parse
  │
  ▼
Assemble
  │
  ▼
Validate
  │
  ▼
Render
  │
  ▼
Complete
```

---

# Stream Chunks

Each chunk represents partial model output.

Typical contents:

- Delta text
- Structured tokens
- Finish reason
- Usage updates
- Provider metadata

Chunks are processed in arrival order.

---

# Chunk Parsing

Responsibilities:

- Decode provider payloads
- Normalize chunk format
- Detect stream completion
- Detect stream errors
- Preserve ordering

Provider-specific formats never leave this stage.

---

# Response Assembly

The assembler combines normalized chunks into a canonical AI Response.

Responsibilities:

- Merge text
- Preserve formatting
- Build structured output
- Track completion state

Partial responses remain available during assembly.

---

# Incremental Validation

Validation may occur during streaming.

Checks include:

- Output schema
- Safety rules
- Language consistency
- Size limits
- Structured output integrity

Critical validation failures terminate the stream.

---

# Rendering

The renderer supports incremental updates.

Capabilities include:

- Live translation updates
- Progressive paragraph rendering
- Partial UI refresh
- Completion notification

Rendering must tolerate incomplete responses.

---

# Cancellation

Streaming requests may be cancelled by:

- User action
- Timeout
- Provider failure
- Application shutdown

Cancellation should:

- Close provider connections
- Release resources
- Preserve diagnostics

---

# Recovery

Recovery strategies include:

- Stream retry
- Resume when supported
- Provider fallback
- Convert to non-streaming execution

Recovery policy is defined separately.

---

# Observability

Streaming metrics include:

- Time to first token
- Total duration
- Chunk count
- Average chunk latency
- Completion rate
- Cancellation rate
- Retry count

These metrics are published to Diagnostics.

---

# Failure Handling

Possible failures:

- Connection interrupted
- Malformed chunk
- Provider timeout
- Validation failure
- Renderer failure

Recovery may include:

- Retry stream
- Switch provider
- Abort gracefully
- Return partial response when allowed

---

# Architecture Invariants

1. Streaming is transparent to business logic.
2. Provider-specific stream formats are normalized.
3. Chunk order is preserved.
4. Rendering consumes validated data only.
5. Streams can be cancelled safely.
6. Streaming and non-streaming produce equivalent final responses.
7. Every stream emits diagnostics and metrics.

---

# Related Documents

- README.md
- PIPELINE.md
- STAGES.md
- REQUEST.md
- RESPONSE.md
- PROMPTS.md
- CONTEXT.md
- MEMORY.md
- MODELS.md
- ROUTING.md
- RETRY.md
- FALLBACK.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
