# Resource Manager States

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Resource Manager
> **Document:** State Model
> **Path:** `03-infrastructure/resource-manager/STATES.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines the state machines for:

- the Resource Manager module itself;
- individual managed resources;
- resource health;
- resource leases.

---

## 2. Resource Manager Module States

```text
Uninitialized
    ↓ Initialize()
Initializing
    ↓ eager resources ready, dependency order resolved
Ready
    ↓ Shutdown()
ShuttingDown
    ↓ all leases released or abandoned, all resources disposed
Shutdown

Initializing → InitializationFailed  (if critical eager resource fails)
```

| State | Meaning |
|---|---|
| `Uninitialized` | Module not yet started; registration accepted, acquisition rejected |
| `Initializing` | Eager resources being initialized in dependency order |
| `Ready` | Accepting all operations |
| `ShuttingDown` | No new acquisitions; waiting for active leases; disposing resources |
| `Shutdown` | Module stopped; all resources disposed |
| `InitializationFailed` | A critical eager resource failed and recovery is not configured |

---

## 3. Individual Resource States

```text
Registered
    ↓ (Eager: at Initialize() | Lazy: at first Resolve/Acquire)
Initializing
    ↓ factory returns instance
Ready
    ↓ lease acquired
Busy
    ↓ lease released
Idle
    ↓ idleTimeout exceeded or explicit Dispose
Disposing
    ↓ disposal complete
Disposed
```

Failure path:

```text
Initializing / Ready / Idle / Busy
        ↓ factory error | health check failure | crash
      Failed
        ↓ recoveryPolicy = Recreate | Reconnect | Restart
    Recovering
        ↓ success → generation + 1
      Ready
        ↓ maxAttempts exceeded
      PermanentlyFailed
```

| State | Meaning |
|---|---|
| `Registered` | Descriptor registered; resource not yet created |
| `Initializing` | Factory is creating the resource instance |
| `Ready` | Resource is available for Resolve or Acquire |
| `Idle` | Resource is in pool but not currently leased |
| `Busy` | Resource has one or more active leases |
| `Disposing` | Dispose in progress |
| `Disposed` | Resource has been disposed and removed |
| `Failed` | Resource failed; recovery may be attempted |
| `Recovering` | Recovery strategy in progress |
| `PermanentlyFailed` | Recovery attempts exhausted; resource is unavailable |

---

## 4. Resource Health States

Health state is tracked independently of lifecycle state.

```text
Unknown
    ↓ first health check passes
Healthy
    ↓ latency elevated | error rate rising
Degraded
    ↓ critical check fails
Unhealthy
    ↓ resource unreachable
Unavailable

Unhealthy / Unavailable → Recovering (resource manager initiates recovery)
Recovering              → Healthy (if recovery succeeds)
Recovering              → PermanentlyFailed (if maxAttempts exceeded)
```

| State | Meaning |
|---|---|
| `Unknown` | No health check has been performed yet |
| `Healthy` | Resource is operating within normal parameters |
| `Degraded` | Resource is operating but with elevated latency or error rate |
| `Unhealthy` | Resource is failing health checks |
| `Unavailable` | Resource is completely unreachable |

---

## 5. Resource Lease States

```text
Acquired
    ↓ release() called
Released

Acquired
    ↓ idleTimeout exceeded without release
Expired (potential leak)
```

| State | Meaning |
|---|---|
| `Acquired` | Lease is active; consumer holds the resource |
| `Released` | Lease has been released; resource returned to pool or idle |
| `Expired` | Lease was not released within timeout; treated as a potential leak |

---

## 6. Transition Rules

- Resource must not be disposed while leases are `Acquired` (unless force shutdown).
- A resource in `PermanentlyFailed` state must be explicitly re-registered to be used again.
- `Busy` and `Idle` are equivalent from a health perspective; health monitoring continues in both.
- A resource returning from `Recovering` must increment its generation before accepting new leases.
- Leases with a generation mismatch must be rejected with `RESOURCE_LEASE_GENERATION_MISMATCH`.
- Resource Manager in `ShuttingDown` state must not accept new `Acquire` calls; `Resolve` calls for already-initialized resources may be permitted until `Shutdown`.
