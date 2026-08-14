# Resource Manager Module

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Resource Manager
> **Document:** Module Architecture
> **Path:** `03-infrastructure/resource-manager/MODULE.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-14
> **Source of Truth:**
>
> - `doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md`
> - `doc/01-architecture/runtime/MEMORY_MODEL.md`
> - `doc/01-architecture/runtime/PROCESS_TOPOLOGY.md`
> - `doc/03-infrastructure/configuration/MODULE.md`
> - `doc/03-infrastructure/event-bus/MODULE.md`
> - `doc/03-infrastructure/logging/MODULE.md`
> - `doc/03-infrastructure/telemetry/MODULE.md`

---

## 1. Purpose

Resource Manager chịu trách nhiệm quản lý toàn bộ tài nguyên (resource) mà CRAI sử dụng trong suốt vòng đời của ứng dụng.

Khác với Configuration hay Scheduler, module này **không thực hiện nghiệp vụ** mà đóng vai trò là tầng điều phối và quản lý tài nguyên hệ thống nhằm đảm bảo:

* Khởi tạo đúng thứ tự.
* Chia sẻ resource dùng chung.
* Quản lý lifecycle.
* Giải phóng tài nguyên đúng lúc.
* Theo dõi tình trạng hoạt động.
* Ngăn rò rỉ tài nguyên (resource leak).
* Hỗ trợ graceful shutdown.

Đây là một trong những module hạ tầng cốt lõi mà hầu như mọi module khác đều phụ thuộc.

---

## 2. Module Goal

The module must provide a consistent resource management model for shared resources such as:

- OCR Engine instances;
- Browser Process and Context;
- Translation Client;
- AI Model Session;
- GPU Context;
- HTTP Client and WebSocket Client;
- Worker Pool;
- Image Decoder;
- Font Cache;
- Storage Connection.

The primary optimization target is:

```text
stable shared resource lifecycle
without resource leaks, duplicate initialization, or uncontrolled failure cascades
```

---

## 3. Architectural Position

```text
Business Module (e.g., Recognition, Translation)
    ↓ acquire("resource-id")
Resource Manager
    ├── Registry
    ├── Lifecycle Manager
    ├── Pool Manager
    ├── Lease Manager
    ├── Health Monitor
    └── Recovery Manager
        ↓ delegates creation to
Factory / Adapter
    ↓ returns resource instance
```

Composition Root owns:

- Resource Manager construction;
- adapter and factory registration;
- startup initialization order;
- shutdown disposal order.

---

## 4. Terminology

### 4.1 Resource

A shared, managed object with a lifecycle.

Examples:

```text
ocr.primary       → OCR Engine
browser.default   → Browser Process
gpu.primary       → GPU Context
http.default      → HTTP Client
```

### 4.2 ResourceDescriptor

Describes **how** a resource is managed:

```text
ResourceDescriptor {
    id
    type
    scope            Application | Module | Session | Task | Request | Worker
    owner
    factory
    dependencies
    lifecyclePolicy  Lazy | Eager | OnDemand
    recoveryPolicy   Recreate | Reconnect | Restart | Fail
    healthPolicy
    poolPolicy
    tags
}
```

### 4.3 ResourceLease

A scoped handle returned by `acquire()`.

The consumer uses the lease to access the resource and must call `release()` when done.

```text
ResourceLease {
    leaseId
    resourceId
    generation
    resource         (typed reference)
    acquiredAt
    expiresAt
}
```

### 4.4 ResourceGeneration

An integer counter incremented each time a resource is recreated.

Stale leases from a previous generation are rejected.

### 4.5 ResourcePool

A bounded collection of interchangeable resource instances.

```text
ResourcePool {
    minSize
    maxSize
    minIdle
    maxIdle
    acquireTimeout
    idleTimeout
    maxLifetime
}
```

---

## 5. Responsibilities

### 5.1 Resource Registration

Registers a `ResourceDescriptor` before the resource is needed.

### 5.2 Resource Lookup

Returns an existing resource instance or initializes one on first request.

### 5.3 Lifecycle Management

Manages the full lifecycle:

```text
Registered → Initializing → Ready → Idle ⇄ Busy → Disposing → Disposed
```

### 5.4 Shared Resource Management

Ensures that resources that should be shared (e.g., HTTP Client, GPU Context) are not duplicated.

### 5.5 Resource Pooling

Manages a bounded pool of instances for resources such as OCR workers and Image Decoders.

Pool operations: `acquire`, `release`, `expand`, `shrink`, `timeout`.

### 5.6 Lease Management

Issues `ResourceLease` on `acquire()` and tracks lease holders.

Enforces: acquire once, release once.

Detects: lease not released, lease released twice, lease used after release.

### 5.7 Health Monitoring

Tracks health state independently of lifecycle state:

```text
Unknown → Healthy → Degraded → Unhealthy → Unavailable
```

Health check sources: heartbeat, ping, error rate, latency, queue depth, memory.

### 5.8 Recovery

Applies recovery strategy on resource failure:

```text
Reconnect | Restart | Reset | Recreate | Replace | Reload
```

Recovery must be bounded: `maxAttempts`, `timeout`, `backoff`.

Increments `ResourceGeneration` after successful recreation.

### 5.9 Dependency Resolution

Ensures initialization order respects resource dependencies.

Detects circular dependencies and reports `RESOURCE_CIRCULAR_DEPENDENCY`.

Shutdown order must be the reverse of initialization order.

### 5.10 Leak Detection

Detects resources that are acquired but not released within expected timeframes.

Emits `ResourceLeakDetected` event.

### 5.11 Graceful Shutdown

On shutdown:

1. Stop new acquisitions.
2. Wait for active leases to be released (bounded timeout).
3. Dispose resources in dependency-reverse order.
4. Log any resources that could not be disposed cleanly.

### 5.12 Resource Statistics

Tracks: active resources, idle resources, pool size, acquire count, release count, average lifetime, memory usage, failure count, recovery count.

---

## 6. Non-Responsibilities

Resource Manager does not own:

- OCR logic;
- Translation logic;
- Rendering logic;
- Scheduling policy (Scheduler owns that);
- Event transport (Event Bus owns that);
- Business workflow decisions.

---

## 7. Design Principles

### Lazy Initialization

Create resources only when first needed unless explicitly configured as eager.

### Deterministic Lifecycle

No resource may exist in an undefined state.

### Reusable Resources

Prefer reuse over recreation. Reduce: startup time, memory, CPU, GPU.

### Isolation

A single resource failure must not fail the Resource Manager or other resources.

### Fail Fast

Initialization failure must be reported immediately.

### Graceful Recovery

Recovery must be possible without restarting the entire application.

### Observable

All lifecycle transitions emit metrics and events for Logging and Telemetry.

---

## 8. Typical Flow

```text
Application Start
        │
        ▼
Register Resources (descriptors + factories)
        │
        ▼
Resolve Dependencies (topological order)
        │
        ▼
Initialize Eager Resources
        │
        ▼
Ready
        │
   ┌────┤ Business Module
   │    ▼
   │ acquire(resourceId)
   │    ↓
   │ ResourceLease
   │    ↓
   │ use resource
   │    ↓
   │ release(lease)
   └────┤
        │
        ▼
Graceful Shutdown
        │
        ▼
Dispose All Resources (reverse dependency order)
```

---

## 9. Dependencies

Required:

- Configuration (load pool sizes, timeouts, recovery policies)
- Logging (structured lifecycle and health logs)
- Event Bus (publish lifecycle and health events)
- Telemetry (resource metrics)

Optional:

- Scheduler (schedule health checks, leak detection sweeps)

---

## 10. Used By

- Recognition / OCR (OCR Engine, OCR Worker)
- Translation (Translation Client)
- Presentation (Renderer, Font Cache)
- Provider Management (HTTP Client, provider connections)
- AI (AI Model Session, GPU Context)
- Storage (Storage Connection)
- UI Adapter (UI rendering resources)
