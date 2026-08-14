# Resource Manager Contract

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Resource Manager
> **Document:** Contract
> **Path:** `03-infrastructure/resource-manager/CONTRACT.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines the public interface of the Resource Manager: the operations it exposes, the types it uses, and the rules callers must follow.

---

## 2. Core Principles

The Resource Manager contract guarantees:

- every resource has a unique, stable identity;
- consumers access resources only through a `ResourceLease`;
- a lease must be acquired before use and released after use;
- stale leases from a previous resource generation are rejected;
- health state is tracked independently of lifecycle state;
- recovery is bounded and observable;
- graceful shutdown disposes all resources in safe order.

---

## 3. Resource Identity

```text
ResourceId = {domain}.{type}.{qualifier}

Examples:
    ocr.primary
    ocr.worker
    browser.default
    browser.session
    translator.default
    translator.chinese
    http.default
    gpu.primary
    cache.image
```

Resource IDs are case-insensitive and must be unique within the registry.

---

## 4. Operations

### 4.1 Register

```text
Register(descriptor: ResourceDescriptor)
    → void
    | ResourceAlreadyRegisteredError
    | ResourceDescriptorInvalidError
    | ResourceCircularDependencyError
```

Registers a resource descriptor before use.

Must be called during application startup, before `Initialize()`.

### 4.2 Initialize

```text
Initialize()
    → void
    | ResourceInitializationFailedError
```

Initializes all resources with `Eager` lifecycle policy, in dependency order.

### 4.3 Resolve

```text
Resolve(resourceId: ResourceId)
    → ResourceReference
    | ResourceNotFoundError
    | ResourceUnavailableError
```

Returns a shared reference to a shareable resource.

Use `Resolve` for resources that do not require exclusive access (e.g., HTTP Client, Logger).

### 4.4 Acquire

```text
Acquire(resourceId: ResourceId, options: AcquireOptions)
    → ResourceLease
    | ResourceNotFoundError
    | ResourceUnavailableError
    | ResourcePoolExhaustedError
    | ResourceAcquireTimeoutError
```

Acquires exclusive or pooled access to a resource.

Returns a `ResourceLease` that must be released when done.

### 4.5 Release

```text
Release(lease: ResourceLease)
    → void
    | ResourceLeaseAlreadyReleasedError
    | ResourceLeaseGenerationMismatchError
```

Returns the resource to the pool or marks it as idle.

### 4.6 Dispose

```text
Dispose(resourceId: ResourceId)
    → void
    | ResourceNotFoundError
    | ResourceStillInUseError
```

Explicitly disposes a resource and removes it from the registry.

Must not be called while active leases exist unless forced.

### 4.7 GetHealth

```text
GetHealth(resourceId: ResourceId)
    → ResourceHealthSnapshot
    | ResourceNotFoundError
```

Returns the current health state of a resource without acquiring it.

### 4.8 Shutdown

```text
Shutdown(options: ShutdownOptions)
    → void
```

Initiates graceful shutdown: stops new acquisitions, waits for active leases, disposes all resources in reverse dependency order.

---

## 5. Types

### 5.1 ResourceDescriptor

```text
ResourceDescriptor {
    id              ResourceId
    type            string (resource type name)
    scope           Application | Module | Session | Task | Request | Worker
    owner           ModuleIdentity
    factory         ResourceFactory reference
    dependencies    list of ResourceId
    lifecyclePolicy Lazy | Eager | OnDemand
    recoveryPolicy  Recreate | Reconnect | Restart | Fail
    healthPolicy    HealthCheckConfig (optional)
    poolPolicy      PoolConfig (optional)
    tags            map<string, string>
}
```

### 5.2 ResourceLease

```text
ResourceLease {
    leaseId         UUID
    resourceId      ResourceId
    generation      integer
    resource        typed reference
    acquiredAt      timestamp
    expiresAt       optional timestamp
}
```

### 5.3 ResourceHealthSnapshot

```text
ResourceHealthSnapshot {
    resourceId
    lifecycleState  Registered | Initializing | Ready | Idle | Busy | Disposing | Disposed | Failed | Recovering
    healthState     Unknown | Healthy | Degraded | Unhealthy | Unavailable
    generation      integer
    activeLeases    integer
    lastCheckedAt   timestamp
}
```

### 5.4 AcquireOptions

```text
AcquireOptions {
    timeout         optional duration
    priority        optional priority hint
}
```

### 5.5 ShutdownOptions

```text
ShutdownOptions {
    gracePeriod     duration (max wait for active leases)
    force           boolean (abandon unreleased leases after grace period)
}
```

---

## 6. Lease Safety Rules

1. A lease must be acquired before the resource is used.
2. A lease must be released exactly once.
3. A resource must not be used after its lease is released.
4. A lease from a previous generation is invalid after resource recreation.
5. Unreleased leases within the configured timeout are treated as potential leaks.

---

## 7. Error Codes

Full error taxonomy is in `ERRORS.md`.

Summary:

| Error Code | Meaning |
|---|---|
| `RESOURCE_NOT_FOUND` | ResourceId does not exist in the registry |
| `RESOURCE_ALREADY_REGISTERED` | ResourceId is already registered |
| `RESOURCE_DESCRIPTOR_INVALID` | Descriptor is missing required fields or has invalid values |
| `RESOURCE_CIRCULAR_DEPENDENCY` | Dependency graph contains a cycle |
| `RESOURCE_INITIALIZATION_FAILED` | Resource could not be initialized |
| `RESOURCE_UNAVAILABLE` | Resource exists but is not in a usable state |
| `RESOURCE_POOL_EXHAUSTED` | All pool instances are in use and pool is at max capacity |
| `RESOURCE_ACQUIRE_TIMEOUT` | Acquire waited longer than the configured timeout |
| `RESOURCE_STILL_IN_USE` | Dispose was called while active leases exist |
| `RESOURCE_LEASE_ALREADY_RELEASED` | Release was called on an already-released lease |
| `RESOURCE_LEASE_GENERATION_MISMATCH` | Lease generation does not match current resource generation |
