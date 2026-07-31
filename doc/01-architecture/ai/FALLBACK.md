# AI Fallback

- **Document:** AI Architecture / Fallback
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines the fallback strategy for the CRAI AI Pipeline.

Fallback provides an alternative execution path when the primary execution cannot complete successfully, ensuring service continuity while preserving correctness, safety and predictable behavior.

---

# Design Principles

- Provider independent
- Capability preserving
- Policy driven
- Graceful degradation
- Cost aware
- Observable
- Deterministic

---

# Architecture

```text
Primary Execution
        │
        ▼
Success?
   │          │
 Yes         No
  │           ▼
Return    Fallback Engine
               │
      ┌────────┼────────┐
      ▼        ▼        ▼
 Alternate  Alternate  Offline
   Model     Provider   Model
               │
               ▼
        Structured Error
```

The Fallback Engine never bypasses routing, safety or validation.

---

# Fallback Triggers

Fallback may be triggered by:

- Retry policy exhausted
- Provider unavailable
- Model unavailable
- Capability temporarily unavailable
- Budget policy allows alternative execution
- Infrastructure failure

---

# Fallback Levels

Typical priority:

1. Equivalent model (same provider)
2. Equivalent provider
3. Lower-tier compatible model
4. Local/offline model
5. Structured failure response

Each level must satisfy the required capability.

---

# Capability Preservation

Fallback candidates must support the requested capability, such as:

- Translation
- Vision
- OCR understanding
- Structured output
- Streaming
- Long context

Unsupported candidates are rejected.

---

# Selection Flow

```text
Primary Failure
      │
      ▼
Validate Retry Exhausted
      │
      ▼
Find Compatible Candidates
      │
      ▼
Evaluate Policy
      │
      ▼
Select Alternative
      │
      ▼
Execute
```

---

# Quality Degradation

Fallback may reduce:

- Response speed
- Context window
- Output quality
- Streaming support

Behavioral correctness and safety must not be reduced.

---

# Budget Awareness

Fallback selection respects:

- Maximum request cost
- Remaining budget
- Token limits
- Offline preference

Expensive alternatives may be skipped when policy forbids them.

---

# User Preferences

Optional preferences include:

- Prefer local execution
- Prefer specific provider
- Disable paid fallback
- Allow slower execution

Preferences influence but do not override compatibility or safety.

---

# Interaction with Retry

Execution order:

```text
Execute
   │
   ▼
Retry Policy
   │
   ▼
Fallback Engine
   │
   ▼
Alternative Execution
```

Fallback begins only after retry policy has completed.

---

# Observability

Metrics include:

- Fallback count
- Fallback reason
- Selected alternative
- Success rate
- Additional latency
- Additional cost

Every fallback decision should be traceable.

---

# Failure Handling

If no compatible fallback exists:

- Return structured error
- Preserve diagnostics
- Record failure reason
- Avoid repeated fallback loops

---

# Architecture Invariants

1. Fallback never bypasses retry policy.
2. Every fallback candidate satisfies the requested capability.
3. Safety validation always executes before alternative execution.
4. Fallback decisions are deterministic under identical conditions.
5. Fallback loops are prohibited.
6. All fallback decisions are observable.
7. Structured errors are returned when no valid alternative exists.

---

# Related Documents

- README.md
- PIPELINE.md
- REQUEST.md
- RESPONSE.md
- MODELS.md
- ROUTING.md
- STREAMING.md
- RETRY.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
