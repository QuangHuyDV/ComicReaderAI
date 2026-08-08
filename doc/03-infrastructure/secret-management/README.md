# 03-infrastructure/resource-manager/README.md

# Resource Manager

## Overview

`Resource Manager` là module hạ tầng chịu trách nhiệm quản lý vòng đời, quyền sử dụng, tình trạng và việc giải phóng các shared resource trong CRAI.

Module này không thực hiện OCR, Translation, Rendering hay Browser Automation. Nó quản lý những tài nguyên mà các capability đó cần để hoạt động ổn định.

Các resource điển hình gồm:

```text
Browser Process
Browser Context
OCR Engine
OCR Worker
Translation Client
AI Model Session
GPU Context
HTTP Client
WebSocket Client
Worker Pool
Image Decoder
Font Cache
Temporary Cache
Storage Connection
```

Resource Manager đóng vai trò như một runtime resource orchestrator:

```text
Module
   │
   ▼
Resource Manager
   │
   ├── Registry
   ├── Lifecycle
   ├── Dependency Resolution
   ├── Lease
   ├── Pool
   ├── Health
   ├── Recovery
   └── Cleanup
```

---

# Why CRAI Needs Resource Manager

CRAI có nhiều tài nguyên có chi phí khởi tạo hoặc vận hành cao.

Ví dụ:

```text
OCR Model
Browser Process
GPU Session
AI Model
Image Worker
Translation Provider
```

Nếu mỗi module tự quản lý các resource này sẽ dễ dẫn tới:

* tạo duplicate resource
* load cùng model nhiều lần
* tiêu tốn RAM/VRAM
* browser process bị bỏ quên
* worker không được shutdown
* race condition khi initialize
* khó thực hiện graceful shutdown
* khó retry khi resource crash
* khó theo dõi resource leak

Resource Manager tạo một lifecycle thống nhất cho toàn hệ thống.

---

# Position in Architecture

Resource Manager thuộc:

```text
03-infrastructure/
```

và nằm giữa runtime infrastructure với các capability sử dụng resource.

```text
Presentation
OCR
Translation
Browser
Automation
Downloader
AI
        │
        ▼
Resource Manager
        │
        ├── Configuration
        ├── Event Bus
        ├── Logging
        ├── Telemetry
        └── Scheduler
```

Resource Manager không thay thế Dependency Injection container.

DI giải quyết:

```text
Which object depends on which object?
```

Resource Manager giải quyết:

```text
When should this expensive/shared resource exist?

Who is using it?

Is it healthy?

When can it be reused?

When should it be recreated?

When must it be disposed?
```

---

# Core Responsibilities

Resource Manager chịu trách nhiệm cho:

* resource registration
* resource lookup
* lazy initialization
* eager initialization
* lifecycle management
* dependency resolution
* resource sharing
* resource pooling
* lease management
* health tracking
* failure recovery
* resource recreation
* generation tracking
* scope management
* leak detection
* graceful shutdown
* resource statistics

---

# Resource Registry

Mỗi resource được đăng ký thông qua một descriptor.

Ví dụ:

```text
resourceId       = ocr.primary
resourceType     = OCRWorker
scope            = Application
ownerModule      = OCR
lifecyclePolicy  = Lazy
recoveryPolicy   = Recreate
```

Resource ID phải duy nhất trong phạm vi registry tương ứng.

Ví dụ convention:

```text
browser.default
browser.session

ocr.primary
ocr.worker

translator.default
translator.chinese

http.default

gpu.primary

cache.image
```

---

# Resource Descriptor

Một descriptor có thể chứa:

```text
ResourceDescriptor
├── id
├── type
├── scope
├── owner
├── factory
├── dependencies
├── lifecyclePolicy
├── recoveryPolicy
├── healthPolicy
├── poolPolicy
└── tags
```

Descriptor mô tả **cách quản lý resource**, không phải resource instance.

---

# Resource Scope

CRAI hỗ trợ các scope chính:

```text
Application
Module
Session
Task
Request
Worker
```

---

## Application Scope

Tồn tại trong toàn bộ runtime.

Ví dụ:

```text
HTTP Client
Shared OCR Engine
Global Translation Client
Telemetry Exporter
```

---

## Module Scope

Chỉ thuộc lifecycle của một module.

Ví dụ:

```text
OCR internal worker registry
Browser internal process manager
```

---

## Session Scope

Gắn với một phiên đọc.

Ví dụ:

```text
Browser Context
Translation Session
Image Cache
Overlay State
```

---

## Task Scope

Gắn với một tác vụ.

Ví dụ:

```text
OCR inference context
Image decode buffer
Temporary render resource
```

---

## Request Scope

Resource rất ngắn hạn phục vụ một request cụ thể.

Chỉ nên dùng khi thực sự cần thiết vì resource creation thường có overhead.

---

## Worker Scope

Gắn với một worker.

Ví dụ:

```text
OCR model binding
GPU execution context
browser worker context
```

---

# Lifecycle

Resource lifecycle chuẩn:

```text
Registered
    ↓
Initializing
    ↓
Ready
    ↓
Idle ⇄ Busy
    │
    ▼
Disposing
    ↓
Disposed
```

Failure flow:

```text
Ready / Idle / Busy
        ↓
      Failed
        ↓
    Recovering
        ↓
      Ready
```

Lifecycle chi tiết được định nghĩa trong:

```text
STATES.md
```

---

# Lazy Initialization

Resource không nhất thiết phải được tạo khi ứng dụng start.

Ví dụ:

```text
Chinese OCR Model
```

chỉ cần load khi người dùng thực sự mở nội dung tiếng Trung.

Flow:

```text
Registered
    │
    ▼
First Resolve / Acquire
    │
    ▼
Initializing
    │
    ▼
Ready
```

Lazy initialization giúp giảm:

* startup time
* RAM
* VRAM
* process count

---

# Eager Initialization

Resource quan trọng có thể được initialize ngay khi runtime start.

Ví dụ:

```text
Logger
Core HTTP Client
Essential Worker Pool
```

Sử dụng khi:

* resource chắc chắn sẽ được dùng
* initialization failure cần được phát hiện sớm
* startup readiness phụ thuộc resource đó

---

# Resolve vs Acquire

Hai operation không nên được hiểu giống nhau.

## Resolve

Dùng để lấy reference tới shared/shareable resource.

Ví dụ:

```text
HTTP Client
Telemetry Client
Configuration-backed Service
```

Flow:

```text
resolve("http.default")
        ↓
shared instance
```

---

## Acquire

Dùng khi quyền sử dụng resource cần được quản lý.

Ví dụ:

```text
OCR Worker
Browser Context
GPU Session
Pooled Decoder
```

Flow:

```text
acquire("ocr.worker")
        ↓
ResourceLease
        ↓
use
        ↓
release()
```

---

# Resource Lease

Consumer không sở hữu resource trực tiếp.

Nó nhận một:

```text
ResourceLease
```

Ví dụ conceptual API:

```text
lease = acquire("ocr.worker")

worker = lease.resource

result = worker.process(image)

release(lease)
```

Lease giúp Resource Manager theo dõi:

* ai đang dùng resource
* dùng bao lâu
* resource có thể dispose hay chưa
* resource có bị leak hay không

---

# Lease Safety

Lease phải tuân thủ:

```text
Acquire once
Release once
```

Không được:

```text
release()
release()
```

hoặc tiếp tục sử dụng sau:

```text
Released
```

Resource Manager có thể phát hiện:

```text
RESOURCE_LEASE_ALREADY_RELEASED
RESOURCE_LEASE_EXPIRED
RESOURCE_LEASE_GENERATION_MISMATCH
```

---

# Resource Generation

Khi resource bị recreate, resource ID có thể giữ nguyên nhưng instance đã thay đổi.

Ví dụ:

```text
ocr.primary
generation = 12
```

worker crash.

Sau recovery:

```text
ocr.primary
generation = 13
```

Lease cũ của generation 12 không được áp dụng cho generation 13.

Generation tracking ngăn:

* stale lease
* stale event
* stale reference
* accidental release of replacement resource

---

# Dependency Resolution

Resource có thể phụ thuộc resource khác.

Ví dụ:

```text
Translator
    │
    ├── HTTP Client
    ├── Configuration
    └── Telemetry
```

Startup order:

```text
Configuration
Telemetry
HTTP Client
Translator
```

Shutdown order phải đảo ngược:

```text
Translator
HTTP Client
Telemetry
Configuration
```

Resource Manager phải phát hiện:

```text
A → B → C → A
```

và trả:

```text
RESOURCE_CIRCULAR_DEPENDENCY
```

---

# Resource Pool

Một số resource thích hợp để quản lý dạng pool.

Ví dụ:

```text
OCR Worker
Image Decoder
Browser Context
AI Session
```

Pool có thể cấu hình:

```text
minSize
maxSize
minIdle
maxIdle
acquireTimeout
idleTimeout
maxLifetime
```

Flow:

```text
Acquire
   │
   ├── Idle resource exists
   │       ↓
   │     Lease
   │
   └── No idle resource
           │
           ├── pool < maxSize
           │      ↓
           │    expand
           │
           └── pool == maxSize
                  ↓
                wait
                  ↓
              timeout
```

---

# Exclusive Resources

Một số resource không thể chia sẻ đồng thời.

Ví dụ:

```text
single OCR worker
mutable browser context
exclusive GPU session
```

Contract:

```text
activeLeaseCount <= 1
```

---

# Shareable Resources

Một số resource có thể dùng đồng thời.

Ví dụ:

```text
HTTP Client
read-only configuration
thread-safe translation client
```

Resource Manager vẫn quản lý lifecycle nhưng không nhất thiết chuyển qua exclusive `Busy` semantics.

---

# Health Monitoring

Lifecycle state và health state độc lập.

Ví dụ:

```text
Lifecycle = Ready
Health    = Degraded
```

Health states:

```text
Unknown
Healthy
Degraded
Unhealthy
Unavailable
```

Health check có thể dựa trên:

* heartbeat
* ping
* request latency
* process state
* queue depth
* memory usage
* error rate
* provider connectivity

---

# Recovery

Resource Manager có thể áp dụng recovery strategy.

```text
Reconnect
Restart
Reset
Recreate
Replace
Reload
```

Ví dụ:

```text
Browser Process
     ↓ crash
Failed
     ↓
Recovering
     ↓
Recreate Browser Process
     ↓
generation + 1
     ↓
Ready
```

Recovery policy phải giới hạn:

```text
maxAttempts
timeout
backoff
```

để tránh restart loop vô hạn.

---

# Failure Isolation

Một resource lỗi không mặc định làm Resource Manager lỗi.

Ví dụ:

```text
translator.provider-a → Failed
```

không đồng nghĩa:

```text
Resource Manager → Failed
```

Manager chỉ nên fail khi:

* registry không còn đáng tin cậy
* lifecycle invariant bị phá vỡ nghiêm trọng
* critical dependency không thể hoạt động
* manager internal state bị corruption

---

# Memory Pressure

CRAI có thể xử lý nhiều ảnh, OCR buffer và model nên memory pressure là một vấn đề thực tế.

Resource Manager có thể quan sát:

```text
RAM usage
VRAM usage
cache size
pool size
worker count
image buffers
```

Khi vượt policy:

```text
Memory Pressure
      ↓
dispose idle resource
      ↓
shrink pool
      ↓
clear expendable cache
      ↓
emit telemetry
```

Không được tự ý dispose resource đang có active lease nếu resource không hỗ trợ safe reclaim.

---

# Resource Leak Detection

Các resource có nguy cơ leak:

```text
Browser Context
Stream
File Handle
OCR Worker
Image Buffer
GPU Allocation
Native Model Session
```

Một lease giữ quá lâu có thể tạo:

```text
ResourceLeakSuspected
```

Nếu vượt policy/grace period:

```text
ResourceLeakDetected
```

Detection không đồng nghĩa luôn có thể force release.

---

# Scope Cleanup

Khi scope kết thúc, Resource Manager chịu trách nhiệm cleanup tài nguyên thuộc scope đó.

Ví dụ Reading Session:

```text
Session Start
     │
     ├── Browser Context
     ├── Translation Session
     ├── Image Cache
     └── Overlay State
     │
     ▼
Session End
     │
     ▼
Stop new acquire
     │
     ▼
Release leases
     │
     ▼
Dispose resources
     │
     ▼
Scope Closed
```

---

# Graceful Shutdown

Khi application shutdown:

```text
Resource Manager
     │
     ▼
ShuttingDown
     │
     ▼
Reject new acquire
     │
     ▼
Wait active leases
     │
     ▼
Drain pools
     │
     ▼
Dispose dependents
     │
     ▼
Dispose dependencies
     │
     ▼
Stopped
```

Dispose order phải dựa trên dependency graph.

---

# Events

Resource Manager publish các event lifecycle quan trọng.

Ví dụ:

```text
ResourceRegistered
ResourceInitializationStarted
ResourceReady

ResourceAcquired
ResourceReleased

ResourceFailed

ResourceRecoveryStarted
ResourceRecoverySucceeded
ResourceRecoveryFailed

ResourceDisposed

PoolExpanded
PoolExhausted

LeaseExpired

ResourceHealthChanged
```

Chi tiết:

```text
EVENTS.md
```

---

# Errors

Error được normalize thành taxonomy chung.

Ví dụ:

```text
RESOURCE_NOT_FOUND
RESOURCE_ALREADY_EXISTS

RESOURCE_INITIALIZATION_FAILED

RESOURCE_INVALID_STATE

RESOURCE_ACQUIRE_TIMEOUT

RESOURCE_POOL_EXHAUSTED

RESOURCE_DEPENDENCY_FAILED

RESOURCE_CIRCULAR_DEPENDENCY

RESOURCE_UNAVAILABLE

RESOURCE_RECOVERY_FAILED

RESOURCE_DISPOSE_FAILED
```

Chi tiết:

```text
ERRORS.md
```

---

# Logging

Resource Manager dùng Logging cho diagnostics.

Ví dụ:

```text
resource initialized
resource initialization failed
lease expired
pool exhausted
recovery started
recovery failed
dispose timeout
```

Resource Manager không được dùng Event Bus thay thế Logging.

---

# Telemetry

Các metric quan trọng có thể gồm:

```text
resource_count
resource_active_count
resource_failed_count

resource_acquire_total
resource_release_total
resource_acquire_duration

resource_initialization_duration
resource_lifetime

resource_recovery_total
resource_recovery_failure_total

resource_pool_size
resource_pool_busy
resource_pool_waiting

resource_leases_active
resource_lease_duration

resource_memory_usage
```

Không bắt buộc tạo mọi metric thông qua Event Bus.

---

# Scheduler Integration

Scheduler có thể hỗ trợ các background maintenance task như:

```text
health checks
idle cleanup
lease expiration scan
pool shrink
statistics snapshot
```

Resource Manager vẫn là owner của lifecycle decision.

Scheduler chỉ cung cấp khả năng scheduling.

---

# Configuration Integration

Configuration có thể định nghĩa policy.

Ví dụ conceptual configuration:

```text
resources:
  ocr:
    lazy: true

    pool:
      minSize: 1
      maxSize: 4
      acquireTimeout: 5000

    recovery:
      strategy: recreate
      maxAttempts: 3
```

Resource Manager đọc policy nhưng không trực tiếp chịu trách nhiệm load configuration source.

---

# Event Bus Integration

Event Bus được dùng để publish lifecycle facts.

Ví dụ:

```text
ResourceManager.ResourceFailed
ResourceManager.ResourceRecoverySucceeded
```

Resource Manager không phụ thuộc consumer cụ thể.

---

# Dependency Direction

Không nên có:

```text
OCR
  ↓
directly creates ResourceManager internals
```

Thay vào đó:

```text
OCR
  ↓
ResourceManager Contract
```

Tương tự:

```text
Browser
Translation
Presentation
AI
```

chỉ sử dụng public contract.

---

# Conceptual API

Implementation cụ thể có thể khác theo ngôn ngữ/runtime.

Core API nên tương đương:

```text
register(descriptor)

unregister(resourceId)

exists(resourceId)

resolve(resourceId)

acquire(resourceId)

release(lease)

createScope(type)

closeScope(scopeId)

getState(resourceId)

getHealth(resourceId)

getStatistics()

shutdown()
```

---

# Register Example

```text
register({
    id: "ocr.worker",
    type: OCRWorker,
    scope: Application,

    lifecycle: Lazy,

    pool: {
        minSize: 1,
        maxSize: 4
    },

    recovery: {
        strategy: Recreate,
        maxAttempts: 3
    }
})
```

---

# Acquire Example

Conceptual flow:

```text
lease = acquire("ocr.worker")

try:
    result = lease.resource.recognize(image)
finally:
    release(lease)
```

`finally` semantics là quan trọng để tránh leak.

Ngôn ngữ hỗ trợ RAII/context manager nên ưu tiên automatic release.

---

# Scoped Usage Example

Một reading session:

```text
session = createScope(Session)

browser = acquire(
    "browser.context",
    scope = session
)

translator = resolve(
    "translator.default"
)

...

closeScope(session)
```

Khi scope đóng, Resource Manager xử lý cleanup resource thuộc session.

---

# OCR Flow in CRAI

```text
Page Image
    │
    ▼
OCR Capability
    │
    ▼
Acquire OCR Worker
    │
    ▼
Resource Manager
    │
    ├── idle worker?
    │
    │     Yes
    │      ↓
    │     Lease
    │
    └── No
          ↓
      expand / wait
          ↓
        Lease
    │
    ▼
OCR Processing
    │
    ▼
Release Lease
```

OCR module không cần biết:

* worker được tạo lúc nào
* worker nằm trong pool ra sao
* worker từng crash chưa
* generation hiện tại bao nhiêu

Các chi tiết đó thuộc Resource Manager.

---

# OCR Recovery Example

```text
OCR Worker #3
     │
     ▼
Process Crash
     │
     ▼
Health = Unavailable
     │
     ▼
State = Failed
     │
     ▼
Recovery Started
     │
     ▼
Recreate
     │
     ▼
Generation 3 → 4
     │
     ▼
Ready
```

OCR workflow có thể retry theo policy ở tầng workflow/capability.

Resource Manager không tự quyết định retry toàn bộ business task.

---

# Browser Example

Browser Process nên có scope dài hơn Browser Context.

Ví dụ:

```text
Browser Process
scope = Application

Browser Context
scope = Session
```

Flow:

```text
Application Start
      │
      ▼
Browser Process
      │
      ├── Session A → Context A
      ├── Session B → Context B
      └── Session C → Context C
```

Khi Session A đóng:

```text
Context A → Disposed
```

Browser Process vẫn tồn tại.

Điều này tiết kiệm chi phí start browser process liên tục.

---

# GPU Example

Nếu CRAI sử dụng local AI/OCR model trên GPU:

```text
GPU Device
   │
   ▼
GPU Resource Pool
   │
   ├── OCR Session
   ├── AI Session
   └── Image Processing
```

Resource Manager có thể dùng policy để giới hạn VRAM consumption.

Initial implementation chưa cần distributed GPU scheduling.

---

# Translation Example

Remote translation provider thường không cần pool nặng như OCR worker.

Có thể đăng ký:

```text
translator.default
scope = Application
shareable = true
```

Resource có thể phụ thuộc:

```text
HTTP Client
Secret Management
Configuration
Telemetry
```

---

# What Resource Manager Does Not Do

Resource Manager không:

```text
perform OCR

translate text

detect text regions

render overlays

download pages

crawl websites

store user library

schedule business workflows

select translation provider

decide OCR retry semantics

implement Event Bus

implement Logging
```

Nó chỉ quản lý resource cần thiết để các chức năng đó thực hiện công việc.

---

# Avoiding Overengineering

Resource Manager có thể trở nên rất phức tạp nếu implement toàn bộ capability ngay từ đầu.

CRAI nên triển khai theo giai đoạn.

---

# Phase 1 — Core

Ưu tiên:

```text
Registry
Lazy Initialization
Lifecycle
Resolve
Acquire / Release
Basic Lease
Scope
Dependency Resolution
Graceful Shutdown
Basic Errors
Basic Events
```

Đây là mức cần thiết cho MVP.

---

# Phase 2 — Reliability

Bổ sung:

```text
Health Check
Recovery
Generation Tracking
Pool
Acquire Timeout
Lease Expiration
Statistics
```

---

# Phase 3 — Adaptive Runtime

Khi CRAI thực sự cần:

```text
Memory Pressure Handling
GPU Pressure Handling
Leak Detection
Adaptive Pool Scaling
Dynamic Policy Reload
Advanced Diagnostics
```

---

# Phase 4 — Optional Future

Chỉ xem xét nếu kiến trúc phát triển tới mức cần thiết:

```text
Remote Worker Pool
Distributed Resource Management
Multi-device GPU Pool
Resource Migration
Cross-process Resource Coordination
```

Không phải mục tiêu ban đầu.

---

# Recommended MVP Resources

Ở phiên bản CRAI đầu tiên, những resource đáng để Resource Manager quản lý gồm:

```text
Browser Process
Browser Context

OCR Engine
OCR Worker

Translation Client

HTTP Client

Image Worker

Session Cache
```

Các object nhỏ, rẻ và stateless không cần biến thành managed resource.

---

# Rule of Thumb

Một object nên trở thành managed resource nếu có ít nhất một trong các đặc điểm:

```text
expensive to create

must be shared

must be pooled

owns external connection

owns process/thread

owns native resource

uses significant RAM/VRAM

requires graceful cleanup

can fail independently

needs health/recovery

must respect scope lifetime
```

Nếu không có các đặc điểm trên, dependency injection thông thường thường là đủ.

---

# File Map

```text
resource-manager/
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
├── ERRORS.md
└── README.md
```

---

## MODULE.md

Định nghĩa:

* mục tiêu module
* trách nhiệm
* phạm vi
* dependency
* design principles

---

## CONTRACT.md

Định nghĩa:

* public contract
* register/resolve/acquire/release
* scope
* pool
* dependency
* health
* monitoring

---

## STATES.md

Định nghĩa:

* Resource Manager lifecycle
* Managed Resource lifecycle
* Health state
* Lease state
* Pool state
* transition invariant
* generation

---

## EVENTS.md

Định nghĩa:

* lifecycle events
* recovery events
* lease events
* pool events
* scope events
* leak/pressure events

---

## ERRORS.md

Định nghĩa:

* error taxonomy
* error codes
* retry classification
* recovery classification
* severity
* event mapping

---

# Design Invariants

Resource Manager implementation phải luôn đảm bảo:

```text
1. Disposed resource cannot be acquired.

2. Resource initialization cannot run twice concurrently.

3. Resource disposal cannot run twice concurrently.

4. Exclusive resource cannot have multiple active leases.

5. Resource generation must change after recreation.

6. Old lease cannot operate on new generation.

7. Dependency must initialize before dependent.

8. Dependent must dispose before dependency.

9. Failed resource cannot be handed to a new consumer.

10. Closing scope cannot accept new scoped resource acquire.

11. Pool cannot exceed maxSize.

12. Resource lifecycle state has one authoritative owner.
```

Authority đó là:

```text
Resource Manager
```

---

# CRAI Runtime Example

Một runtime đơn giản:

```text
CRAI Application
      │
      ▼
Resource Manager
      │
      ├── HTTP Client
      │
      ├── Browser Process
      │      │
      │      └── Reading Session
      │             └── Browser Context
      │
      ├── OCR Pool
      │      ├── Worker #1
      │      └── Worker #2
      │
      ├── Translation Client
      │
      └── Image Worker Pool
```

Page flow:

```text
Web Page
   │
   ▼
Browser Context
   │
   ▼
Image Capture
   │
   ▼
OCR Worker Lease
   │
   ▼
OCR
   │
   ▼
Release OCR Worker
   │
   ▼
Translation Client
   │
   ▼
Presentation / Overlay
```

Resource Manager chỉ xuất hiện ở những nơi lifecycle/resource ownership cần được kiểm soát.

---

# Summary

Resource Manager là hạ tầng trung tâm cho việc quản lý các runtime resource có chi phí cao hoặc lifecycle phức tạp trong CRAI.

Nó cung cấp một lớp thống nhất cho:

```text
Registration
     ↓
Initialization
     ↓
Resolution / Acquisition
     ↓
Usage
     ↓
Health
     ↓
Recovery
     ↓
Release
     ↓
Disposal
```

Thiết kế quan trọng nhất của module là:

```text
Resource identity
+
Scope
+
Lifecycle state
+
Lease
+
Generation
+
Dependency
```

Các capability cao hơn không cần biết cách resource được tạo, pool, recreate hay cleanup. Chúng chỉ làm việc thông qua contract của Resource Manager.

Implementation ban đầu nên ưu tiên lifecycle correctness và cleanup reliability trước khi bổ sung các cơ chế nâng cao như adaptive scaling, GPU pressure hay distributed resource management.
