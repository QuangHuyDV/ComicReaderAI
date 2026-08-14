# Resource Manager Errors

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Resource Manager
> **Document:** Errors
> **Path:** `03-infrastructure/resource-manager/ERRORS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines the error taxonomy, error codes, causes, severity levels, and handling strategies for the Resource Manager module.

---

## 2. Error Principles

- All errors are CRAI-typed; factory or backend exceptions are wrapped at the adapter boundary.
- A single resource failure must not propagate as a Resource Manager failure unless the manager's own invariants are broken.
- Errors are classified by recoverability.
- Lease errors must never silently allow use of a disposed or stale resource.

---

## 3. Error Taxonomy

### 3.1 Registration Errors

#### RESOURCE_ALREADY_REGISTERED

```text
Code:       RESOURCE_ALREADY_REGISTERED
Severity:   ERROR
Cause:      A descriptor with the same ResourceId was already registered.
Recovery:   Verify startup registration order; each resource may only be registered once.
```

#### RESOURCE_DESCRIPTOR_INVALID

```text
Code:       RESOURCE_DESCRIPTOR_INVALID
Severity:   ERROR
Cause:      The descriptor is missing required fields or has conflicting policies.
Recovery:   Correct the descriptor before registration.
```

#### RESOURCE_CIRCULAR_DEPENDENCY

```text
Code:       RESOURCE_CIRCULAR_DEPENDENCY
Severity:   CRITICAL
Cause:      The dependency graph contains a cycle (A → B → C → A).
Recovery:   Redesign the resource dependency structure.
```

---

### 3.2 Initialization Errors

#### RESOURCE_INITIALIZATION_FAILED

```text
Code:       RESOURCE_INITIALIZATION_FAILED
Severity:   ERROR
Cause:      The resource factory could not produce a valid instance.
Recovery:   Retry if recoveryPolicy permits; otherwise the resource enters Failed state.
```

---

### 3.3 Acquisition Errors

#### RESOURCE_NOT_FOUND

```text
Code:       RESOURCE_NOT_FOUND
Severity:   ERROR
Cause:      The requested ResourceId is not registered.
Recovery:   Verify the resource ID and ensure registration occurred before acquisition.
```

#### RESOURCE_UNAVAILABLE

```text
Code:       RESOURCE_UNAVAILABLE
Severity:   ERROR
Cause:      The resource is registered but in a state that does not permit acquisition (Failed, Recovering, Disposed, ShuttingDown).
Recovery:   Wait for recovery or use an alternative resource.
```

#### RESOURCE_POOL_EXHAUSTED

```text
Code:       RESOURCE_POOL_EXHAUSTED
Severity:   WARNING
Cause:      All pool instances are leased and the pool is at max capacity.
Recovery:   Wait for a lease to be released; consider increasing pool maxSize.
```

#### RESOURCE_ACQUIRE_TIMEOUT

```text
Code:       RESOURCE_ACQUIRE_TIMEOUT
Severity:   WARNING
Cause:      The acquire request waited longer than the configured timeout without obtaining a lease.
Recovery:   Retry with backoff or report degraded capability to the caller.
```

---

### 3.4 Lease Errors

#### RESOURCE_LEASE_ALREADY_RELEASED

```text
Code:       RESOURCE_LEASE_ALREADY_RELEASED
Severity:   ERROR
Cause:      Release was called on a lease that was already released.
Recovery:   Fix the caller to track lease state correctly.
```

#### RESOURCE_LEASE_GENERATION_MISMATCH

```text
Code:       RESOURCE_LEASE_GENERATION_MISMATCH
Severity:   ERROR
Cause:      The lease generation does not match the current resource generation (resource was recreated after the lease was issued).
Recovery:   Discard the stale result; re-acquire a fresh lease.
```

#### RESOURCE_LEASE_EXPIRED

```text
Code:       RESOURCE_LEASE_EXPIRED
Severity:   WARNING
Cause:      The lease was not released within the configured expiry timeout.
Recovery:   Release the lease immediately; investigate the caller for blocking behavior.
```

---

### 3.5 Disposal Errors

#### RESOURCE_STILL_IN_USE

```text
Code:       RESOURCE_STILL_IN_USE
Severity:   ERROR
Cause:      Dispose was called while active leases exist.
Recovery:   Wait for leases to be released before disposing, or use force shutdown.
```

#### RESOURCE_DISPOSAL_FAILED

```text
Code:       RESOURCE_DISPOSAL_FAILED
Severity:   WARNING
Cause:      The resource factory's dispose method threw an error.
Recovery:   Log and continue shutdown; do not block shutdown on non-critical disposal failures.
```

---

### 3.6 Module Errors

#### RESOURCE_MANAGER_NOT_READY

```text
Code:       RESOURCE_MANAGER_NOT_READY
Severity:   CRITICAL
Cause:      An operation was attempted before the module completed initialization.
Recovery:   Wait for ResourceManagerInitialized event.
```

#### RESOURCE_MANAGER_SHUTTING_DOWN

```text
Code:       RESOURCE_MANAGER_SHUTTING_DOWN
Severity:   WARNING
Cause:      An acquire was attempted while the module is in ShuttingDown state.
Recovery:   The caller should not attempt new acquisitions during shutdown.
```

---

## 4. Error Handling Summary

| Error Code | Caller Action |
|---|---|
| `RESOURCE_NOT_FOUND` | Verify resource ID; check startup registration |
| `RESOURCE_UNAVAILABLE` | Retry after health recovery; use fallback |
| `RESOURCE_POOL_EXHAUSTED` | Wait and retry; consider pool size increase |
| `RESOURCE_ACQUIRE_TIMEOUT` | Retry with backoff; report degraded state |
| `RESOURCE_LEASE_ALREADY_RELEASED` | Fix caller to release exactly once |
| `RESOURCE_LEASE_GENERATION_MISMATCH` | Discard result; re-acquire fresh lease |
| `RESOURCE_INITIALIZATION_FAILED` | Retry if configured; else mark resource unavailable |
| `RESOURCE_CIRCULAR_DEPENDENCY` | Redesign resource dependency graph |
| `RESOURCE_MANAGER_NOT_READY` | Wait for initialization event |
