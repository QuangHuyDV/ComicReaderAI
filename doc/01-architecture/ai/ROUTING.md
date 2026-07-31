# AI Routing

- **Document:** AI Architecture / Routing
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines how the CRAI AI Pipeline selects the most appropriate AI provider and model for each request.

The routing layer evaluates request requirements, model capabilities, runtime conditions and execution policies to make deterministic and cost-aware routing decisions.

---

# Design Principles

- Capability first
- Provider independent
- Policy driven
- Cost aware
- Health aware
- Deterministic
- Extensible

---

# Routing Architecture

```text
AI Request
     │
     ▼
Routing Engine
     │
     ├── Capability Matcher
     ├── Policy Evaluator
     ├── Cost Evaluator
     ├── Health Checker
     └── Model Selector
            │
            ▼
     Selected Provider
```

The Routing Engine never communicates directly with provider-specific APIs.

---

# Routing Inputs

The router evaluates:

- Required capability
- Input type
- Context size
- Token estimate
- User preferences
- Budget limits
- Latency target
- Streaming requirement
- Offline requirement
- Model availability

---

# Routing Policies

Typical policies include:

- Lowest Cost
- Highest Quality
- Lowest Latency
- Offline First
- Privacy First
- Balanced

Policies are configurable and independent of providers.

---

# Capability Matching

Models are matched by declared capabilities rather than provider names.

Examples:

- Translation
- Vision
- OCR Understanding
- Structured Output
- Streaming
- Tool Calling
- Long Context

Only compatible models proceed to evaluation.

---

# Selection Process

```text
Receive Request
      │
      ▼
Validate Capability
      │
      ▼
Filter Compatible Models
      │
      ▼
Evaluate Policy
      │
      ▼
Evaluate Health
      │
      ▼
Evaluate Budget
      │
      ▼
Select Model
```

---

# Budget Evaluation

Budget constraints may include:

- Maximum request cost
- Daily budget
- Monthly budget
- Token limits

Models exceeding policy limits are excluded.

---

# Health Evaluation

Routing considers runtime health:

- Healthy
- Degraded
- Unavailable
- Maintenance

Unavailable models are never selected.

---

# User Preferences

Optional preferences include:

- Preferred provider
- Preferred model
- Offline mode
- Maximum latency
- Preferred language

Preferences influence routing but do not override safety or compatibility rules.

---

# Fallback Routing

If the selected model fails:

```text
Primary Model
      │
      ▼
Retry
      │
      ▼
Alternative Model
      │
      ▼
Alternative Provider
      │
      ▼
Offline Model
```

Fallback behavior is defined separately in FALLBACK.md.

---

# Observability

Routing metrics include:

- Selected model
- Selected provider
- Decision latency
- Candidate count
- Rejected candidates
- Estimated cost
- Routing policy

Decision reasoning should be traceable without exposing sensitive data.

---

# Failure Handling

Possible routing failures:

- No compatible model
- Budget exceeded
- Capability unavailable
- All providers unhealthy
- Invalid policy

Recovery options:

- Relax non-critical preferences
- Use fallback routing
- Switch to offline execution
- Return structured error

---

# Architecture Invariants

1. Routing decisions are based on capabilities, not provider names.
2. Routing completes before model execution.
3. Provider-specific logic remains outside the pipeline.
4. Health and budget are evaluated before selection.
5. Routing policies are deterministic for identical inputs.
6. Fallback never bypasses compatibility validation.
7. Every routing decision is observable and traceable.

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
- STREAMING.md
- RETRY.md
- FALLBACK.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
