# 03-infrastructure/resource-manager/CONTRACT.md

# Resource Manager Contract

## Purpose

Tài liệu này định nghĩa các interface, contract và quy tắc tương tác giữa Resource Manager với các module khác trong hệ thống.

Resource Manager là **điểm duy nhất** chịu trách nhiệm quản lý lifecycle của mọi shared resource trong CRAI.

---

# Core Principles

Resource Manager phải đảm bảo:

* Mỗi resource có định danh duy nhất.
* Resource chỉ được khởi tạo theo policy.
* Không tồn tại nhiều instance ngoài ý muốn.
* Lifecycle luôn xác định.
* Resource có thể được theo dõi (observable).
* Resource có thể được giải phóng an toàn.
* Resource không được sử dụng sau khi đã dispose.

---

# Resource Identity

Mỗi resource được xác định bởi:

```text
Resource ID
Resource Type
Version
Scope
Owner Module
```

Ví dụ:

```text
browser.default
ocr.primary
translator.openai
translator.gemini
renderer.overlay
http.default
gpu.main
cache.image
```

Resource ID phải là duy nhất trong phạm vi Runtime.

---

# Resource Scope

Resource Manager hỗ trợ các phạm vi sau:

| Scope       | Mô tả                            |
| ----------- | -------------------------------- |
| Application | Dùng chung toàn bộ ứng dụng      |
| Session     | Tồn tại trong một phiên làm việc |
| Task        | Chỉ tồn tại trong một tác vụ     |
| Request     | Chỉ phục vụ một request          |
| Worker      | Gắn với một worker cụ thể        |
| Module      | Chỉ dùng trong một module        |

---

# Resource Registration Contract

## register()

Đăng ký resource.

Input

```text
ResourceDescriptor
```

Output

```text
RegistrationResult
```

Điều kiện:

* Resource ID chưa tồn tại.
* Type hợp lệ.
* Lifecycle Policy hợp lệ.

Nếu trùng Resource ID:

* trả lỗi
* hoặc ghi đè nếu policy cho phép.

---

## unregister()

Xóa resource.

Điều kiện:

* Không còn client đang sử dụng.
* Không ở trạng thái Busy.
* Không còn dependency.

Nếu không đáp ứng điều kiện:

```text
RESOURCE_IN_USE
```

---

# Resource Lookup Contract

## resolve()

Lấy resource.

Input

```text
Resource ID
```

Output

```text
Resource Instance
```

Nếu chưa được tạo:

* Lazy Create
* hoặc trả lỗi

theo Resource Policy.

---

## exists()

Kiểm tra resource tồn tại.

Output

```text
true
false
```

---

## list()

Liệt kê toàn bộ resource.

Có thể lọc theo:

* Scope
* Type
* Module
* State
* Tag

---

# Lifecycle Contract

Resource phải đi theo lifecycle chuẩn:

```text
Registered
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

Không được phép:

```text
Ready
↓

Registered
```

hoặc

```text
Disposed
↓

Busy
```

---

# Acquire Contract

## acquire()

Yêu cầu sử dụng resource.

Input

```text
Resource ID
```

Output

```text
Lease
```

Acquire có thể:

* thành công
* timeout
* bị từ chối
* chờ pool

---

## release()

Giải phóng lease.

Input

```text
Lease
```

Sau release:

* giảm usage count
* trả về pool
* hoặc dispose

theo policy.

---

# Pool Contract

Nếu resource thuộc Pool:

Manager phải hỗ trợ:

* max size
* min idle
* max idle
* acquire timeout
* idle timeout
* lifetime
* auto expand
* auto shrink

Pool phải đảm bảo:

* không cấp cùng một instance cho hai client nếu resource không hỗ trợ chia sẻ đồng thời.
* không vượt quá giới hạn cấu hình.

---

# Dependency Contract

Resource có thể khai báo dependency.

Ví dụ:

```text
Translator

depends on

HTTP Client
Configuration
Logger
Telemetry
```

Manager phải:

* resolve dependency trước.
* khởi tạo đúng thứ tự.
* phát hiện dependency bị thiếu.
* phát hiện vòng lặp (circular dependency).

---

# Health Contract

Mỗi resource phải cung cấp trạng thái:

```text
Healthy
Busy
Idle
Restarting
Slow
Disconnected
Failed
Disposed
```

Resource Manager phải có khả năng:

* đọc trạng thái.
* phát hiện thay đổi.
* phát sinh event.
* chuyển sang quy trình recovery nếu cần.

---

# Recovery Contract

Khi resource lỗi:

Có thể áp dụng một trong các policy:

```text
Restart
Reconnect
Recreate
Ignore
Fail Fast
Manual
```

Policy được cấu hình theo từng resource.

---

# Monitoring Contract

Resource Manager phải cung cấp tối thiểu các chỉ số:

```text
Active Resources
Idle Resources
Busy Resources
Pool Size
Acquire Count
Release Count
Failure Count
Restart Count
Memory Usage
Average Lifetime
Average Acquire Time
```

Các metric này được Telemetry thu thập định kỳ.

---

# Thread Safety Contract

Mọi thao tác sau phải an toàn trong môi trường đa luồng:

* register
* unregister
* resolve
* acquire
* release
* dispose
* statistics

Không được xảy ra:

* race condition
* double initialization
* double dispose
* duplicate registration

---

# Event Contract

Resource Manager phát sinh các sự kiện:

```text
ResourceRegistered
ResourceInitialized
ResourceReady
ResourceAcquired
ResourceReleased
ResourceBusy
ResourceIdle
ResourceRestarted
ResourceRecovered
ResourceFailed
ResourceDisposed
PoolExpanded
PoolShrunk
HealthChanged
```

Chi tiết payload được định nghĩa trong `EVENTS.md`.

---

# Error Contract

Các lỗi chuẩn:

```text
RESOURCE_NOT_FOUND
RESOURCE_ALREADY_EXISTS
RESOURCE_INITIALIZATION_FAILED
RESOURCE_DISPOSE_FAILED
RESOURCE_BUSY
RESOURCE_IN_USE
RESOURCE_TIMEOUT
RESOURCE_POOL_EXHAUSTED
RESOURCE_DEPENDENCY_FAILED
RESOURCE_CIRCULAR_DEPENDENCY
RESOURCE_INVALID_SCOPE
RESOURCE_INVALID_STATE
RESOURCE_HEALTH_FAILED
RESOURCE_RECOVERY_FAILED
```

Chi tiết mã lỗi được định nghĩa trong `ERRORS.md`.

---

# Security Contract

Resource Manager không được:

* trả về resource đã dispose.
* trả về resource chưa khởi tạo.
* cho phép truy cập resource vượt phạm vi (scope).
* tự ý tạo resource không được đăng ký.
* ghi đè resource khi policy không cho phép.

---

# Performance Targets

| Metric                  | Target                                                       |
| ----------------------- | ------------------------------------------------------------ |
| Register Resource       | < 5 ms                                                       |
| Resolve Resource        | < 1 ms (cached)                                              |
| Acquire Resource        | < 2 ms (không chờ pool)                                      |
| Release Resource        | < 1 ms                                                       |
| Resource Initialization | Theo từng loại resource                                      |
| Dispose Resource        | Theo từng loại resource                                      |
| Pool Expansion          | Không chặn các resource đang hoạt động                       |
| Health Check            | Có thể chạy định kỳ mà không ảnh hưởng đáng kể tới hiệu năng |

---

# Compatibility

Resource Manager phải hỗ trợ quản lý thống nhất cho nhiều loại tài nguyên:

* Browser Instance
* Browser Context
* OCR Engine
* Translation Engine
* AI Model Session
* GPU Context
* CPU Worker
* Thread Pool
* HTTP Client
* WebSocket Client
* Event Bus Client
* Cache
* Font Cache
* Image Decoder
* Renderer
* Downloader
* Exporter
* Storage Connection

Việc bổ sung loại resource mới không được yêu cầu thay đổi contract hiện có (Open/Closed Principle).
