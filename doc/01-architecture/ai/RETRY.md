
# AI Retry Policy

- **Document:** AI Architecture / Retry
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines the retry strategy for the CRAI AI Pipeline.

The Retry subsystem improves reliability by automatically reattempting recoverable failures while preventing unnecessary requests, duplicated work and excessive cost.

---

# Design Principles

- Provider independent
- Failure aware
- Idempotent
- Policy driven
- Cost conscious
- Observable
- Safe by default

---

# Retry Architecture

```text
AI Request
    │
    ▼
Execution
    │
    ▼
Failure Classifier
    │
    ▼
Retry Policy Engine
    │
 ┌──┴─────────────┐
 │Retry           │No Retry
 ▼                ▼
Backoff       Return Error
 │
 ▼
Model Execution
```

---

# Retry Lifecycle

```text
Request
   │
   ▼
Execute
   │
   ▼
Failure?
   │
 ┌─┴─────────┐
 │Yes        │No
 ▼           ▼
Classify   Success
 │
 ▼
Retry Policy
 │
 ▼
Backoff
 │
 ▼
Retry
```

---

# Retryable Failures

Typical retryable failures include:

- Network interruption
- Temporary provider outage
- HTTP 429 rate limiting
- Gateway timeout
- Connection timeout
- Transient infrastructure errors

These failures are expected to recover without changing the request.

---

# Non-Retryable Failures

Requests should not be retried for:

- Invalid request
- Authentication failure
- Authorization failure
- Unsupported capability
- Invalid prompt
- Invalid output schema
- User cancellation

These require correction rather than repetition.

---

# Retry Policies

Supported policies may include:

- Fixed delay
- Exponential backoff
- Exponential backoff with jitter
- Adaptive retry
- Provider-specific override

The routing layer selects the appropriate policy.

---

# Backoff Strategy

Typical retry flow:

```text
Attempt 1
    │
    ▼
Delay
    │
    ▼
Attempt 2
    │
    ▼
Longer Delay
    │
    ▼
Attempt 3
```

Backoff prevents request storms during provider instability.

---

# Retry Limits

Retry configuration may define:

- Maximum attempts
- Maximum elapsed time
- Maximum cost
- Maximum token budget

Requests exceeding limits fail fast.

---

# Idempotency

Retries must preserve request identity.

Requirements:

- Same logical request
- No duplicated side effects
- Stable request metadata
- Trace continuity

---

# Interaction with Routing

If retry conditions are exhausted:

1. Retry policy ends.
2. Routing may select another compatible model.
3. Fallback policy may be invoked.

Retry itself never changes provider selection.

---

# Observability

Metrics include:

- Retry count
- Retry latency
- Retry success rate
- Failure classification
- Backoff duration
- Final outcome

All retries are traceable.

---

# Failure Handling

Possible retry failures:

- Retry limit exceeded
- Budget exceeded
- Repeated timeout
- Permanent provider failure

Recovery options:

- Invoke fallback
- Switch provider
- Switch model
- Return structured error

---

# Architecture Invariants

1. Retries are applied only to retryable failures.
2. Retry decisions are policy driven.
3. Requests remain idempotent across retries.
4. Retry limits are always enforced.
5. Retry does not bypass routing or safety validation.
6. Every retry is observable and traceable.
7. Retry and fallback are separate responsibilities.

---

# Related Documents

- README.md
- PIPELINE.md
- REQUEST.md
- RESPONSE.md
- MODELS.md
- ROUTING.md
- STREAMING.md
- FALLBACK.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
