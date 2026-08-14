# Resource Manager

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Resource Manager

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
Application   → tồn tại trong toàn bộ runtime
Module        → thuộc lifecycle của một module
Session       → gắn với một phiên đọc
Task          → gắn với một tác vụ
Request       → ngắn hạn phục vụ một request cụ thể
Worker        → gắn với một worker
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

# Resource Lease

Consumer không sở hữu resource trực tiếp.

Nó nhận một `ResourceLease`:

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

# Resource Generation

Khi resource bị recreate, resource ID có thể giữ nguyên nhưng generation tăng.

Lease cũ của generation cũ không được áp dụng cho generation mới.

Generation tracking ngăn stale lease, stale event, stale reference.

---

# Health Monitoring

Lifecycle state và health state độc lập.

Health states:

```text
Unknown
Healthy
Degraded
Unhealthy
Unavailable
```

---

# Recovery

Recovery strategies:

```text
Reconnect
Restart
Reset
Recreate
Replace
Reload
```

Recovery policy phải giới hạn `maxAttempts`, `timeout`, `backoff` để tránh restart loop vô hạn.

---

# MVP Scope

MVP bao gồm:

* In-memory Resource Registry
* Application-scope và Session-scope resources
* Lazy và Eager initialization
* Resource Lease
* Pool (min/max size, acquire timeout)
* Health monitoring
* Basic recovery (recreate on failure)
* Generation tracking
* Graceful shutdown
* Logging và Telemetry integration

Chưa bao gồm:

* Distributed resource management
* Remote Worker Pool
* Cross-process GPU Pool
* AI Model Pool shared across processes

---

# Related Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- EVENTS.md
- ERRORS.md

Kiến trúc liên quan:

- `doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md`
- `doc/01-architecture/runtime/MEMORY_MODEL.md`
- `doc/01-architecture/runtime/PROCESS_TOPOLOGY.md`
