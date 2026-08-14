# Resource Manager Events

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Resource Manager
> **Document:** Events
> **Path:** `03-infrastructure/resource-manager/EVENTS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14

---

## 1. Purpose

This document defines events emitted by the Resource Manager module.

Events reflect:

- lifecycle of the Resource Manager itself
- lifecycle of each managed resource
- health state changes
- acquire / release lease activity
- pool behavior
- recovery
- scope cleanup
- shutdown
- resource pressure and leak detection

---

## 2. Event Conventions

All Resource Manager events follow the CRAI Event Convention:

- past-tense naming
- no large data payloads (identifiers and metadata only)
- published via Event Bus infrastructure

---

## 3. Module Lifecycle Events

### ResourceManagerInitialized

Published when the module completes initialization.

```text
ResourceManagerInitialized {
    timestamp
    eagerResourceCount   integer
}
```

### ResourceManagerShutdown

Published when the module has completed graceful shutdown.

```text
ResourceManagerShutdown {
    timestamp
    disposedCount    integer
    abandonedCount   integer
}
```

### ResourceManagerInitializationFailed

Published when a critical eager resource prevents startup.

```text
ResourceManagerInitializationFailed {
    timestamp
    failedResourceId
    reason
}
```

---

## 4. Resource Lifecycle Events

### ResourceRegistered

Published when a resource descriptor is successfully registered.

```text
ResourceRegistered {
    resourceId
    resourceType
    scope
    lifecyclePolicy
    timestamp
}
```

### ResourceInitialized

Published when a resource instance is successfully created.

```text
ResourceInitialized {
    resourceId
    generation      integer
    initDuration    duration
    timestamp
}
```

### ResourceInitializationFailed

Published when a resource factory fails.

```text
ResourceInitializationFailed {
    resourceId
    generation      integer
    attempt         integer
    reason
    willRetry       boolean
    timestamp
}
```

### ResourceDisposed

Published when a resource has been disposed.

```text
ResourceDisposed {
    resourceId
    generation      integer
    lifetime        duration
    timestamp
}
```

---

## 5. Lease Events

### ResourceLeaseAcquired

Published when a consumer acquires a lease.

```text
ResourceLeaseAcquired {
    leaseId
    resourceId
    generation      integer
    consumerId      ModuleIdentity
    timestamp
}
```

### ResourceLeaseReleased

Published when a consumer releases a lease.

```text
ResourceLeaseReleased {
    leaseId
    resourceId
    generation      integer
    leaseDuration   duration
    timestamp
}
```

### ResourceLeakDetected

Published when a lease has not been released within the configured timeout.

```text
ResourceLeakDetected {
    leaseId
    resourceId
    generation      integer
    acquiredAt      timestamp
    leakThreshold   duration
    timestamp
}
```

---

## 6. Health Events

### ResourceHealthChanged

Published when a resource health state changes.

```text
ResourceHealthChanged {
    resourceId
    previousHealth  Unknown | Healthy | Degraded | Unhealthy | Unavailable
    currentHealth   Unknown | Healthy | Degraded | Unhealthy | Unavailable
    timestamp
}
```

---

## 7. Recovery Events

### ResourceRecoveryStarted

Published when Resource Manager begins a recovery attempt.

```text
ResourceRecoveryStarted {
    resourceId
    previousGeneration  integer
    attempt             integer
    strategy            Recreate | Reconnect | Restart | Reset | Replace | Reload
    timestamp
}
```

### ResourceRecoverySucceeded

Published when a resource has been successfully recovered.

```text
ResourceRecoverySucceeded {
    resourceId
    newGeneration   integer
    attempt         integer
    timestamp
}
```

### ResourceRecoveryFailed

Published when all recovery attempts have been exhausted.

```text
ResourceRecoveryFailed {
    resourceId
    totalAttempts   integer
    reason
    isPermanent     boolean
    timestamp
}
```

---

## 8. Pool Events

### ResourcePoolExpanded

Published when the pool grows to serve a new acquire request.

```text
ResourcePoolExpanded {
    resourceId
    previousSize    integer
    newSize         integer
    timestamp
}
```

### ResourcePoolShrunk

Published when idle instances are disposed to reduce memory pressure.

```text
ResourcePoolShrunk {
    resourceId
    previousSize    integer
    newSize         integer
    disposedCount   integer
    timestamp
}
```

### ResourcePoolExhausted

Published when a pool has reached max capacity and an acquire request must wait.

```text
ResourcePoolExhausted {
    resourceId
    currentSize     integer
    maxSize         integer
    waitingCount    integer
    timestamp
}
```
