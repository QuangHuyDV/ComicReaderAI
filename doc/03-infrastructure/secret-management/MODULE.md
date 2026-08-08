# 03-infrastructure/resource-manager/MODULE.md

# Module: Resource Manager

## Purpose

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

# Responsibilities

## Resource Registration

Cho phép module khác đăng ký resource.

Ví dụ:

* OCR Engine
* Translator Client
* Browser Instance
* Image Cache
* Font Cache
* HTTP Client
* WebSocket Client
* GPU Context
* Database Connection
* Thread Pool
* Worker Pool

---

## Resource Discovery

Cho phép module khác lấy resource đã được đăng ký.

Ví dụ

Presentation cần:

```
Renderer
```

Resource Manager trả về instance hiện có.

Nếu chưa tồn tại:

* tạo mới
* hoặc báo lỗi
* hoặc trả Lazy Resource

tùy policy.

---

## Lifecycle Management

Quản lý toàn bộ vòng đời resource.

```
Created
↓

Initializing
↓

Ready
↓

Busy
↓

Idle
↓

Disposing
↓

Disposed
```

Resource Manager đảm bảo:

* init đúng lúc
* dispose đúng lúc
* không dispose resource đang sử dụng
* không tạo duplicate

---

## Shared Resource Management

Một số resource cần dùng chung.

Ví dụ

```
HTTP Client

GPU Context

Translation Engine

Playwright Browser

Logger
```

Không nên tạo mới mỗi lần sử dụng.

Resource Manager sẽ quản lý singleton hoặc pool tùy loại resource.

---

## Resource Pool

Một số resource có chi phí tạo cao.

Ví dụ

```
OCR Worker

Translator Worker

Image Decoder

Playwright Context

AI Model Session
```

Resource Manager hỗ trợ:

* acquire
* release
* timeout
* maximum size
* minimum idle
* auto expand
* auto shrink

---

## Resource Health Monitoring

Theo dõi tình trạng resource.

Ví dụ

```
Healthy

Busy

Slow

Disconnected

Restarting

Failed
```

Nếu resource lỗi:

* restart
* recreate
* remove khỏi pool
* notify Telemetry

---

## Dependency Resolution

Một số resource phụ thuộc resource khác.

Ví dụ

```
Translator

↓

HTTP Client

↓

Configuration

↓

Logger
```

Resource Manager đảm bảo dependency được khởi tạo đúng thứ tự.

---

## Graceful Shutdown

Khi ứng dụng đóng:

```
Stop Scheduler

↓

Stop Workers

↓

Flush Cache

↓

Close Browser

↓

Close HTTP

↓

Release GPU

↓

Dispose Logger
```

Không để resource bị mất dữ liệu.

---

## Memory Protection

Theo dõi:

* memory usage
* object count
* cache size
* image buffer
* OCR buffer
* translation buffer

Khi vượt ngưỡng:

* release cache
* shrink pool
* GC hint
* emit warning

---

## Leak Detection

Phát hiện:

* resource không release
* worker bị treo
* browser context quên đóng
* stream chưa dispose
* file handle chưa close

---

## Resource Statistics

Thu thập:

* active resources
* idle resources
* pool size
* acquire count
* release count
* average lifetime
* memory usage
* failure count

Telemetry sẽ sử dụng các thống kê này.

---

# Out of Scope

Không chịu trách nhiệm:

* OCR
* Translation
* Rendering
* Download
* Storage
* Scheduler
* Event Bus
* Logging
* Telemetry

Module chỉ quản lý lifecycle của các thành phần trên.

---

# Public Responsibilities

Resource Manager cung cấp:

* register resource
* unregister resource
* acquire resource
* release resource
* resolve dependency
* initialize
* dispose
* monitor health
* collect statistics

---

# Dependencies

Required

* Configuration
* Logging
* Event Bus
* Telemetry

Optional

* Scheduler
* Cache
* Storage

---

# Used By

Hầu như toàn bộ hệ thống:

* Presentation
* OCR
* Translation
* Overlay
* Automation
* Browser
* Downloader
* Cache
* AI
* Export
* Synchronization

---

# Design Principles

## Lazy Initialization

Chỉ tạo resource khi cần.

---

## Deterministic Lifecycle

Lifecycle rõ ràng.

Không tồn tại resource ở trạng thái không xác định.

---

## Reusable Resources

Ưu tiên tái sử dụng.

Giảm:

* startup time
* memory
* CPU
* GPU

---

## Isolation

Resource lỗi không làm hỏng resource khác.

---

## Fail Fast

Khởi tạo thất bại phải báo ngay.

Không tiếp tục với resource không hợp lệ.

---

## Graceful Recovery

Có thể:

* recreate
* restart
* reconnect

mà không cần khởi động lại toàn bộ ứng dụng.

---

## Observable

Mọi thay đổi lifecycle đều sinh metric và event để Logging và Telemetry theo dõi.

---

# Typical Flow

```
Application Start
        │
        ▼
Register Resources
        │
        ▼
Resolve Dependencies
        │
        ▼
Initialize Resources
        │
        ▼
Ready
        │
        ▼
Acquire / Release
        │
        ▼
Health Monitoring
        │
        ▼
Graceful Shutdown
        │
        ▼
Dispose All Resources
```

---

# Future Extensions

Có thể mở rộng thêm:

* Dynamic Resource Plugin
* Distributed Resource Manager
* Remote Worker Pool
* GPU Pool
* AI Model Pool
* Multi-browser Pool
* Adaptive Pool Scaling
* Resource Priority Scheduling
* Memory Pressure Response
* Automatic Resource Migration
