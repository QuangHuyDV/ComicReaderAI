# Runtime Retry Policy

> Project: CRAI
> Version: 0.1
> Status: Architecture Draft

---

# 1. Purpose

This document defines how CRAI retries failed work safely, consistently, and efficiently.

Retry exists to recover from transient failures without:

- duplicating work
- retry storms
- stale execution
- provider overload
- repeated permanent failures
- violating revision authority

Retry is coordinated by the Runtime Control layer rather than individual workers.

---

# 2. Goals

The retry system must:

- retry only meaningful work
- distinguish transient and permanent failures
- avoid retrying obsolete revisions
- support provider fallback
- bound retry cost
- preserve current-revision priority
- integrate with cancellation
- remain observable

---

# 3. Philosophy

Retry is a **new attempt**, never a continuation.

Every retry creates:

```text
New AttemptId
```

The previous attempt is terminal.

---

# 4. Retry Ownership

Retry decisions belong exclusively to:

```text
Runtime Control
    ↓
Retry Policy
```

Workers never retry themselves.

Provider adapters never retry themselves.

Scheduler never invents retries.

---

# 5. Retry Flow

```text
Work Failed
    ↓
Runtime validates failure
    ↓
Retry Policy evaluates
    ↓
Eligible?
    ├── No → Terminal
    └── Yes
          ↓
Create new Attempt
          ↓
Scheduler admits work
```

---

# 6. Retry Eligibility

Retry requires all conditions:

- retryable error
- current revision
- session active
- provider available
- retry budget available
- cancellation not requested

---

# 7. Retry Classes

```text
NONE

IMMEDIATE

DELAYED

AFTER_PROVIDER_CHANGE

AFTER_USER_ACTION
```

---

# 8. Retry Budget

Retry consumes budget.

Budget may exist per:

- WorkItem
- Revision
- Session
- Provider

Example:

```text
Revision Budget

Attempt1
Attempt2
Attempt3

Budget Exhausted
```

---

# 9. Maximum Attempts

MVP:

```text
OCR

2 attempts

Translation

3 attempts

Layout

1 attempt

Presentation

1 attempt
```

Exact values remain configurable.

---

# 10. Immediate Retry

Suitable for:

- temporary worker interruption
- temporary allocation failure
- short provider disconnect

Immediate retry should occur only once.

---

# 11. Delayed Retry

Suitable for:

- provider overload
- timeout
- temporary network issue

Delay should be cancelable.

---

# 12. Exponential Backoff

Repeated retries should increase delay.

Conceptually:

```text
Retry1

1x

Retry2

2x

Retry3

4x
```

Exact timing is implementation-specific.

---

# 13. Jitter

Random jitter prevents synchronized retries.

Useful when:

multiple sessions

or

multiple providers

experience identical failures.

---

# 14. Retry After

If provider supplies:

Retry-After

Runtime should respect it.

Retry Policy validates revision relevance before waiting.

---

# 15. Revision Validation

Before retry:

```text
Revision Current?
```

If not:

No retry.

---

# 16. Session Validation

Before retry:

```text
Session Active?
```

If not:

Cancel retry.

---

# 17. Attempt Identity

Each retry owns:

```text
Revision 50

Attempt1

Attempt2

Attempt3
```

Only latest authoritative attempt may commit.

---

# 18. Provider Retry

Provider retry is disabled by default.

Runtime creates a new provider request instead.

---

# 19. Provider Fallback

Retry Policy may switch provider.

Example:

```text
Primary timeout

↓

Fallback provider

↓

New Attempt
```

---

# 20. Retry and Cancellation

Cancellation immediately invalidates pending retries.

Running retries receive cancellation request.

---

# 21. Retry and Stale Results

Late result from:

Attempt1

must never overwrite:

Attempt3.

---

# 22. Retry and Cache

Retry should check cache again.

Previous attempt may have completed elsewhere.

---

# 23. Retry and Resource Cleanup

Previous attempt releases:

leases

temporary buffers

provider handles

before retry starts.

---

# 24. Retry and Performance

Retry counts toward:

provider cost

latency

resource usage

Metrics should distinguish:

Initial latency

Recovery latency

---

# 25. Retry Events

Examples:

retry.scheduled

retry.started

retry.completed

retry.skipped

retry.exhausted

retry.provider.changed

---

# 26. Metrics

Track:

retry count

retry success rate

retry latency

retry budget exhaustion

provider fallback rate

attempt count

---

# 27. Retry Storm Prevention

Prevent:

```text
Many failures

↓

Many retries

↓

Provider overload

↓

More failures
```

Methods:

bounded concurrency

backoff

budget

provider degradation

---

# 28. User Retry

Manual retry:

creates

New Attempt

not

resume old attempt.

---

# 29. Retry During Shutdown

Shutdown cancels:

scheduled retry

delayed retry

pending retry

No retry survives shutdown.

---

# 30. Retry Invariants

- Retry creates a new Attempt.
- Retry never resumes old work.
- Retry belongs to Runtime Control.
- Retry requires current revision.
- Retry respects retry budget.
- Retry never bypasses Scheduler.
- Retry checks cache again.
- Retry releases previous resources.
- Retry never revives canceled work.
- Retry never revives stale work.

---

# 31. Related Documents

- ERROR_MODEL.md
- PERFORMANCE_MODEL.md
- SCHEDULER.md
- CANCELLATION.md

---

# 32. Summary

Retry is a runtime decision.

Pipeline:

```text
Failure

↓

Retry Policy

↓

New Attempt

↓

Scheduler

↓

Execution
```

Retry never belongs to workers.

Retry always creates new authority.