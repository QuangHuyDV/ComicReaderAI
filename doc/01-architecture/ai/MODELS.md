# AI Models

- **Document:** AI Architecture / Models
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines the model abstraction used by the CRAI AI Pipeline.

The model layer hides provider-specific implementations behind a stable contract so the pipeline can work consistently with cloud, local and future AI models.

---

# Design Principles

- Provider independent
- Capability driven
- Model abstraction
- Runtime selectable
- Extensible
- Observable
- Version aware

---

# Architecture

```text
AI Pipeline
     │
     ▼
 Model Router
     │
     ▼
Model Registry
     │
 ┌───┼───────────────┐
 ▼   ▼       ▼       ▼
GPT Gemini Claude Ollama
                ▼
           Future Models
```

The pipeline communicates only with the Model Router.

---

# Model Categories

## Cloud Models

Examples:

- OpenAI GPT
- Google Gemini
- Anthropic Claude
- DeepSeek
- Qwen API

Suitable for high-quality inference.

---

## Local Models

Examples:

- Ollama
- llama.cpp
- vLLM
- Local GGUF models

Suitable for offline or privacy-sensitive workloads.

---

## Specialized Models

Examples:

- OCR correction
- Translation
- Summarization
- Explanation
- Vision models

Selected according to capability.

---

# Model Capabilities

Each model advertises supported capabilities.

Examples:

- Text Generation
- Translation
- Vision
- OCR Understanding
- Streaming
- Structured Output
- Tool Calling
- Long Context

Capabilities are used for routing instead of provider names.

---

# Model Metadata

Typical metadata includes:

- Model ID
- Display Name
- Provider
- Version
- Context Window
- Maximum Output Tokens
- Supported Languages
- Capability List
- Cost Profile
- Availability

---

# Model Selection

Selection factors include:

- Required capability
- Context length
- Cost budget
- Latency target
- User preference
- Offline requirement
- Health status

The routing layer makes the final selection.

---

# Model Lifecycle

```text
Registered
    │
    ▼
Available
    │
    ▼
Selected
    │
    ▼
Executing
    │
    ▼
Completed
```

Unavailable models remain registered but are not selected.

---

# Model Health

Health states:

- Healthy
- Degraded
- Unavailable
- Maintenance

Routing should avoid unhealthy models when alternatives exist.

---

# Model Versioning

Each model maintains:

- Provider Version
- Capability Version
- Compatibility Information

Changes must not break the public AI contracts.

---

# Observability

Metrics include:

- Request Count
- Success Rate
- Error Rate
- Latency
- Token Usage
- Estimated Cost
- Streaming Duration

Metrics are consumed by Diagnostics.

---

# Failure Handling

Possible failures:

- Provider unavailable
- Timeout
- Rate limiting
- Invalid model
- Capability mismatch
- Authentication failure

Recovery options:

- Retry
- Alternative model
- Alternative provider
- Offline model
- User notification

---

# Architecture Invariants

1. The AI Pipeline never depends on a concrete model.
2. Models are selected by capability and policy.
3. Provider-specific APIs are hidden behind abstraction layers.
4. Model health influences routing decisions.
5. Model metadata is centrally managed.
6. Capability declarations are validated before registration.
7. Adding a new model does not require pipeline redesign.

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
- ROUTING.md
- STREAMING.md
- RETRY.md
- FALLBACK.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
