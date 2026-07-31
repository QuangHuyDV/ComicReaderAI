# AI Cost Control

- **Document:** AI Architecture / Cost Control
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines how the CRAI AI Pipeline controls execution cost while maintaining translation quality, predictable performance and provider independence.

Cost Control manages token consumption, provider pricing, execution budgets and optimization policies before and during model execution.

---

# Design Principles

- Provider independent
- Budget driven
- Token aware
- Predictable
- Policy based
- Observable
- Extensible

---

# Architecture

```text
AI Request
     │
     ▼
Cost Estimator
     │
     ▼
Budget Evaluator
     │
     ▼
Optimization Engine
     │
     ▼
Execution Decision
     │
 ┌───┼─────────────┐
 ▼   ▼             ▼
Run Optimize     Reject
```

The Cost Control layer evaluates every request before model execution.

---

# Cost Components

Execution cost may include:

- Input tokens
- Output tokens
- Context size
- Vision processing
- OCR processing
- Tool execution
- Provider pricing

All costs are normalized into a provider-independent model.

---

# Budget Types

Supported budgets include:

- Per request budget
- Session budget
- Daily budget
- Monthly budget
- Project budget
- Organization budget

Budgets are configurable by policy.

---

# Token Budget

The pipeline estimates:

- Prompt tokens
- Context tokens
- Expected output tokens
- Safety margin

Requests exceeding limits are optimized before execution.

---

# Optimization Strategies

When budgets are exceeded, the system may:

- Compress context
- Summarize history
- Remove duplicate information
- Reduce optional metadata
- Select a lower-cost compatible model
- Disable non-essential features

Optimization must preserve functional correctness.

---

# Pricing Model

Each registered model exposes pricing metadata such as:

- Input token price
- Output token price
- Vision price
- Minimum billing unit
- Currency
- Effective date

The pipeline never hardcodes provider pricing.

---

# Policy Evaluation

Example policies:

- Lowest Cost
- Cost Cap
- Best Value
- Quality First
- Offline Preferred
- User Defined

Policies are evaluated before routing completes.

---

# Quotas

Optional quotas include:

- Maximum requests
- Maximum tokens
- Maximum spend
- Concurrent executions

Quota violations return structured errors.

---

# Monitoring

Metrics include:

- Estimated cost
- Actual cost
- Token usage
- Budget utilization
- Optimization count
- Cost savings
- Rejected requests

These metrics are exported to Diagnostics.

---

# Failure Handling

Possible failures:

- Budget exceeded
- Unknown pricing
- Token estimation failure
- Quota exceeded
- Optimization failure

Recovery strategies:

- Optimize request
- Select cheaper compatible model
- Use offline execution
- Reject with structured error

---

# Architecture Invariants

1. Every request is evaluated before execution.
2. Token estimation precedes provider selection.
3. Budget policies are deterministic.
4. Cost optimization never bypasses safety validation.
5. Provider pricing is externalized from business logic.
6. Actual cost is recorded after execution.
7. Cost metrics are observable and auditable.

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
- FALLBACK.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
