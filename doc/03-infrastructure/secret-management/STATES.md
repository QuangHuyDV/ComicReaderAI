# 03-infrastructure/resource-manager/STATES.md

# Resource Manager States

## Purpose

Tài liệu này định nghĩa state machine cho:

* chính `Resource Manager`
* từng `Managed Resource`
* trạng thái sức khỏe của resource
* trạng thái lease khi resource được acquire
* các quy tắc chuyển trạng thái hợp lệ

Mục tiêu là tránh tình trạng lifecycle mơ hồ, double initialization, double dispose, sử dụng resource sau khi đã dispose hoặc recovery không kiểm soát.

---

# Part A — Resource Manager Lifecycle

## Manager States

```text
Created
  ↓
Starting
  ↓
Ready
  ↓
Running
  ↓
ShuttingDown
  ↓
Stopped
```

Có thể chuyển sang:

```text
Failed
```

từ các trạng thái lifecycle đang hoạt động nếu xảy ra lỗi nghiêm trọng.

---

## Created

Resource Manager object đã tồn tại nhưng chưa thực hiện bootstrap.

Tại trạng thái này:

* chưa load registry
* chưa resolve dependency
* chưa initialize resource
* chưa chạy health monitoring
* chưa nhận acquire request

Allowed transitions:

```text
Created → Starting
Created → Failed
```

---

## Starting

Resource Manager đang khởi động.

Các thao tác điển hình:

* load configuration
* khởi tạo registry
* đăng ký built-in resources
* kiểm tra descriptor
* xây dependency graph
* kiểm tra circular dependency
* initialize eager resources
* chuẩn bị pool
* đăng ký health monitoring

Không nên phục vụ acquire request thông thường trong giai đoạn này.

Allowed transitions:

```text
Starting → Ready
Starting → Failed
```

---

## Ready

Resource Manager đã hoàn thành bootstrap và sẵn sàng phục vụ hệ thống.

Tại đây:

* registry hợp lệ
* dependency graph hợp lệ
* eager resources đã khởi tạo
* lazy resources có thể được tạo khi cần
* acquire/release được phép

Allowed transitions:

```text
Ready → Running
Ready → ShuttingDown
Ready → Failed
```

`Ready` có thể là trạng thái rất ngắn nếu hệ thống tự động chuyển sang `Running`.

---

## Running

Trạng thái vận hành bình thường.

Resource Manager có thể:

* register dynamic resource nếu policy cho phép
* resolve
* acquire
* release
* initialize lazy resources
* expand/shrink pool
* health check
* recovery
* collect statistics
* dispose session/task scoped resources

Allowed transitions:

```text
Running → ShuttingDown
Running → Failed
```

---

## ShuttingDown

Hệ thống đang thực hiện graceful shutdown.

Resource Manager phải:

1. từ chối acquire mới nếu policy yêu cầu
2. chờ lease đang hoạt động kết thúc trong giới hạn timeout
3. dừng health monitor
4. dừng pool expansion
5. dispose resource theo dependency order ngược
6. flush resource cần flush
7. đóng connection, process, worker, browser và model session
8. giải phóng native/GPU resource

Allowed transitions:

```text
ShuttingDown → Stopped
ShuttingDown → Failed
```

Không được chuyển trở lại:

```text
ShuttingDown → Running
```

trong cùng một manager instance.

---

## Stopped

Resource Manager đã đóng hoàn toàn.

Tại đây:

* không resolve mới
* không acquire
* không register
* tất cả resource thuộc lifecycle của manager phải đã dispose hoặc được đánh dấu failed-to-dispose

Terminal state.

```text
Stopped → ∅
```

Nếu cần khởi động lại Resource Manager, phải tạo lifecycle mới.

---

## Failed

Resource Manager gặp lỗi nghiêm trọng khiến không thể đảm bảo contract.

Ví dụ:

* registry corruption
* dependency graph invalid
* critical resource startup failure
* internal lifecycle invariant violation
* shutdown failure nghiêm trọng

`Failed` không đồng nghĩa mọi lỗi resource đều làm manager fail.

Một resource đơn lẻ bị lỗi thường chỉ chuyển resource đó sang `Failed`.

Manager chỉ vào `Failed` khi không thể tiếp tục đảm bảo tính đúng đắn của toàn hệ thống.

---

# Manager Transition Table

| From         | To           | Allowed |
| ------------ | ------------ | ------: |
| Created      | Starting     |     Yes |
| Starting     | Ready        |     Yes |
| Ready        | Running      |     Yes |
| Ready        | ShuttingDown |     Yes |
| Running      | ShuttingDown |     Yes |
| ShuttingDown | Stopped      |     Yes |
| Created      | Failed       |     Yes |
| Starting     | Failed       |     Yes |
| Ready        | Failed       |     Yes |
| Running      | Failed       |     Yes |
| ShuttingDown | Failed       |     Yes |
| Stopped      | Running      |      No |
| ShuttingDown | Running      |      No |
| Failed       | Running      |      No |

---

# Part B — Managed Resource Lifecycle

## Resource States

Lifecycle chuẩn:

```text
Registered
    ↓
Initializing
    ↓
Ready
    ↓
Idle ⇄ Busy
    ↓
Disposing
    ↓
Disposed
```

Failure path:

```text
Initializing
    ↓
Failed

Ready / Idle / Busy
    ↓
Failed
```

Recovery path:

```text
Failed
  ↓
Recovering
  ↓
Ready
```

hoặc:

```text
Failed
  ↓
Disposing
  ↓
Disposed
```

---

# Registered

Resource descriptor đã được đăng ký nhưng resource instance chưa sẵn sàng.

Có thể là:

* eager resource đang chờ startup
* lazy resource chưa được yêu cầu
* pool resource chưa cần tạo instance

Thông tin phải có:

* resource id
* type
* scope
* owner
* lifecycle policy
* dependency
* recovery policy

Allowed transitions:

```text
Registered → Initializing
Registered → Disposing
Registered → Failed
```

`Registered → Disposing` dùng khi unregister một lazy resource chưa từng initialize.

---

# Initializing

Resource đang được tạo hoặc chuẩn bị sử dụng.

Ví dụ:

* tạo HTTP client
* load OCR model
* mở browser process
* mở database connection
* allocate GPU context
* initialize translation provider
* start worker

Trong trạng thái này:

* không được trả resource cho consumer
* initialization phải idempotent hoặc được manager bảo vệ khỏi chạy lặp
* chỉ một initialization operation được phép tồn tại trên cùng resource instance

Allowed transitions:

```text
Initializing → Ready
Initializing → Failed
Initializing → Disposing
```

`Initializing → Disposing` chỉ dùng khi startup bị cancel/shutdown.

---

# Ready

Resource đã khởi tạo thành công.

Đối với shared concurrent resource:

```text
Ready
```

có thể duy trì trong khi nhiều consumer sử dụng đồng thời.

Đối với exclusive/pool resource:

```text
Ready → Idle
```

sau khi initialization hoàn tất.

Allowed transitions:

```text
Ready → Idle
Ready → Busy
Ready → Failed
Ready → Disposing
```

---

# Idle

Resource sẵn sàng nhưng hiện không có active lease.

Thường áp dụng cho:

* pooled worker
* browser context
* OCR worker
* translator worker
* image decoder
* database connection pool item

Allowed transitions:

```text
Idle → Busy
Idle → Failed
Idle → Disposing
```

Idle resource có thể bị dispose do:

* idle timeout
* pool shrink
* memory pressure
* scope end
* shutdown

---

# Busy

Resource đang được sử dụng.

Thông thường:

```text
activeLeaseCount > 0
```

Đối với exclusive resource:

```text
activeLeaseCount = 1
```

Đối với shareable concurrent resource:

```text
activeLeaseCount >= 1
```

Allowed transitions:

```text
Busy → Idle
Busy → Ready
Busy → Failed
Busy → Disposing
```

`Busy → Disposing` chỉ hợp lệ trong các tình huống cưỡng chế như:

* shutdown timeout
* unrecoverable failure
* process crash

Manager phải ưu tiên chờ release trước khi dispose.

---

# Failed

Resource không còn đảm bảo khả năng sử dụng.

Nguyên nhân có thể gồm:

* initialization failure
* disconnected
* process crash
* health check failure
* native resource error
* provider unavailable
* corrupted state
* timeout nghiêm trọng

Resource ở `Failed` không được cấp cho consumer mới.

Allowed transitions:

```text
Failed → Recovering
Failed → Disposing
```

Trong một số policy:

```text
Failed → Failed
```

có thể xảy ra khi retry thất bại, nhưng đây là state persistence chứ không phải transition logic mới.

---

# Recovering

Resource Manager đang cố phục hồi resource.

Recovery action có thể là:

* reconnect
* restart
* reset
* recreate
* replace instance
* reload model

Trong thời gian này:

* resource không được acquire mới
* existing lease phải được xử lý theo policy
* health state phải phản ánh recovery

Allowed transitions:

```text
Recovering → Ready
Recovering → Idle
Recovering → Failed
Recovering → Disposing
```

---

# Disposing

Resource đang được giải phóng.

Các thao tác có thể gồm:

* flush
* close
* disconnect
* stop worker
* terminate process
* release native memory
* release GPU memory
* close browser
* unload model

Trong trạng thái này:

* không được acquire mới
* resolve không được trả instance này như usable resource
* dispose phải chống double invocation

Allowed transitions:

```text
Disposing → Disposed
Disposing → Failed
```

Nếu dispose gặp lỗi, manager vẫn phải đánh dấu resource không còn usable.

---

# Disposed

Resource đã kết thúc lifecycle.

Terminal state:

```text
Disposed → ∅
```

Một resource instance đã `Disposed` không được tái sử dụng.

Nếu cần resource cùng ID sau đó, Resource Manager phải:

* tạo instance lifecycle mới
* tăng generation nếu implementation hỗ trợ generation tracking

Ví dụ:

```text
browser.default#1 → Disposed

browser.default#2 → Initializing
```

---

# Resource Transition Table

| From         | To           | Allowed |
| ------------ | ------------ | ------: |
| Registered   | Initializing |     Yes |
| Registered   | Disposing    |     Yes |
| Initializing | Ready        |     Yes |
| Initializing | Failed       |     Yes |
| Ready        | Idle         |     Yes |
| Ready        | Busy         |     Yes |
| Idle         | Busy         |     Yes |
| Busy         | Idle         |     Yes |
| Ready        | Failed       |     Yes |
| Idle         | Failed       |     Yes |
| Busy         | Failed       |     Yes |
| Failed       | Recovering   |     Yes |
| Recovering   | Ready        |     Yes |
| Recovering   | Idle         |     Yes |
| Recovering   | Failed       |     Yes |
| Registered   | Disposed     |      No |
| Initializing | Busy         |      No |
| Failed       | Busy         |      No |
| Disposed     | Ready        |      No |
| Disposed     | Initializing |      No |

---

# Part C — Health State

Lifecycle state và health state là hai khái niệm khác nhau.

Ví dụ:

```text
Lifecycle = Ready
Health    = Degraded
```

là hoàn toàn hợp lệ.

Không nên dùng lifecycle state để thay thế health state.

---

## Health States

```text
Unknown
Healthy
Degraded
Unhealthy
Unavailable
```

---

## Unknown

Chưa đủ thông tin để xác định health.

Thường xảy ra:

* vừa đăng ký
* vừa initialize
* health check chưa chạy

---

## Healthy

Resource hoạt động bình thường.

---

## Degraded

Resource vẫn usable nhưng có dấu hiệu suy giảm.

Ví dụ:

* latency cao
* memory tăng bất thường
* provider phản hồi chậm
* worker queue gần đầy
* OCR inference chậm
* browser sử dụng memory cao

Manager có thể tiếp tục cấp resource nhưng cần emit warning/metric.

---

## Unhealthy

Resource đang hoạt động không đúng contract.

Ví dụ:

* health check thất bại
* request liên tục lỗi
* internal process không phản hồi

Thông thường phải trigger recovery.

---

## Unavailable

Resource không thể phục vụ.

Ví dụ:

* external service mất kết nối
* worker process chết
* GPU context bị mất
* browser crash

Có thể tương ứng lifecycle:

```text
Failed
```

hoặc:

```text
Recovering
```

---

# Health Transition Example

```text
Unknown
   ↓
Healthy
   ↓
Degraded
   ↓
Unhealthy
   ↓
Unavailable
```

Recovery có thể đưa health về:

```text
Unavailable
     ↓
Unknown
     ↓
Healthy
```

---

# Part D — Lease Lifecycle

Resource được acquire không nên trả ownership trực tiếp cho consumer.

Consumer nhận một:

```text
ResourceLease
```

---

## Lease States

```text
Created
  ↓
Active
  ↓
Releasing
  ↓
Released
```

Failure path:

```text
Active
  ↓
Expired
```

---

## Created

Lease đã được tạo nhưng chưa hoàn tất acquire transaction.

---

## Active

Consumer được phép sử dụng resource.

Lease phải chứa tối thiểu:

```text
leaseId
resourceId
resourceGeneration
acquiredAt
owner
```

Có thể thêm:

```text
expiresAt
taskId
sessionId
```

---

## Releasing

Release operation đã bắt đầu.

Trạng thái này ngăn double release.

---

## Released

Lease đã hoàn tất.

Terminal state.

Consumer không được tiếp tục sử dụng resource thông qua lease này.

---

## Expired

Lease vượt quá thời gian sử dụng cho phép.

Manager có thể:

* emit warning
* mark possible leak
* request cancellation
* force reclaim nếu resource hỗ trợ

Không phải mọi resource đều cho phép force reclaim.

---

# Lease Invariants

Một lease phải đảm bảo:

```text
Acquire once
Release once
```

Không hợp lệ:

```text
Released → Active
```

hoặc:

```text
release(releasedLease)
```

Nếu xảy ra phải được ghi nhận như lifecycle misuse.

---

# Part E — Scope End Behavior

Khi một scope kết thúc:

## Request Scope

Dispose resource thuộc request ngay sau request.

```text
Request End
    ↓
Release leases
    ↓
Dispose resources
```

---

## Task Scope

Khi task hoàn thành hoặc cancel:

```text
Task Completed / Cancelled
          ↓
Release resources
          ↓
Dispose task resources
```

---

## Session Scope

Ví dụ người dùng đóng một phiên đọc truyện:

```text
Reading Session End
        ↓
Close browser context
Clear temporary OCR state
Release translation session
Dispose session cache
```

---

## Application Scope

Chỉ dispose khi Resource Manager shutdown.

---

# Part F — Pool State

Pool có state riêng ở cấp aggregate.

```text
Created
  ↓
Starting
  ↓
Ready
  ↓
Draining
  ↓
Disposed
```

---

## Ready Pool

Pool có thể:

* acquire
* release
* expand
* shrink

Các instance bên trong vẫn dùng resource lifecycle riêng.

---

## Draining

Không nhận acquire mới.

Đang chờ resource đang Busy được release.

Dùng khi:

* shutdown
* pool recreation
* configuration reload

```text
Ready
  ↓
Draining
  ↓
Disposed
```

---

# Part G — Shutdown Ordering

Khi shutdown, resource phải được dispose theo thứ tự dependency ngược.

Ví dụ:

```text
Translator
   ↓ depends on
HTTP Client
   ↓ depends on
Telemetry
```

Startup:

```text
Telemetry
HTTP Client
Translator
```

Shutdown:

```text
Translator
HTTP Client
Telemetry
```

Nguyên tắc:

```text
Initialize dependencies first.

Dispose dependents first.
```

---

# Part H — State Invariants

Resource Manager phải luôn duy trì các invariant sau.

## Invariant 1

`Disposed` resource không bao giờ usable.

```text
state == Disposed
→ acquire forbidden
```

---

## Invariant 2

Resource chưa `Ready`, `Idle` hoặc trạng thái shareable tương đương không được cấp cho consumer.

---

## Invariant 3

Một exclusive resource không có nhiều hơn một active lease.

```text
exclusive == true

→ activeLeaseCount <= 1
```

---

## Invariant 4

Pool không vượt quá `maxSize`.

---

## Invariant 5

Resource chỉ có một initialization operation tại một thời điểm.

---

## Invariant 6

Resource chỉ có một dispose operation tại một thời điểm.

---

## Invariant 7

Recovery không chạy song song nhiều lần trên cùng generation.

---

## Invariant 8

Dependency đã disposed không được tiếp tục phục vụ dependent.

---

## Invariant 9

Lease phải tham chiếu đúng resource generation.

Điều này ngăn trường hợp:

```text
lease cũ
   ↓
resource crash
   ↓
resource recreate
   ↓
lease cũ vô tình thao tác trên instance mới
```

---

# Part I — Generation

Mỗi lần resource được recreate nên tăng:

```text
generation
```

Ví dụ:

```text
ocr.primary
generation = 1

→ crash

ocr.primary
generation = 2
```

Lease thuộc generation cũ phải trở thành invalid.

Generation tracking đặc biệt quan trọng với:

* browser process
* browser context
* GPU context
* OCR model session
* AI model session
* native worker

---

# Part J — CRAI Example

Một phiên đọc manga có thể tạo:

```text
BrowserContext
OCRWorker
TranslationSession
ImageCache
OverlayRenderer
```

Lifecycle:

```text
Start Reading Session
        │
        ▼
Create Session Scope
        │
        ├── BrowserContext → Ready
        ├── OCRWorker → Idle
        ├── TranslationSession → Ready
        ├── ImageCache → Ready
        └── OverlayRenderer → Ready
        │
        ▼
Page Detected
        │
        ▼
OCRWorker
Idle → Busy → Idle
        │
        ▼
Translation
        │
        ▼
Overlay Render
        │
        ▼
Next Page
```

Khi người dùng đóng phiên:

```text
Session End
    │
    ▼
Stop new acquire
    │
    ▼
Release active leases
    │
    ▼
Dispose BrowserContext
Dispose OCR session resources
Dispose TranslationSession
Dispose ImageCache
Dispose OverlayRenderer
```

Application-scoped OCR engine hoặc shared model có thể vẫn tồn tại để phiên sau tái sử dụng.

---

# State Ownership

Resource Manager là authority duy nhất đối với lifecycle state.

Resource implementation có thể báo:

```text
health
internal status
capability status
```

nhưng không được tự ý thay đổi lifecycle state ngoài contract của Resource Manager.

Điều này tránh việc lifecycle bị phân tán giữa nhiều module.

---

# Observability

Mọi transition quan trọng phải có thể quan sát được thông qua:

* Event Bus
* Logging
* Telemetry

Tối thiểu cần ghi nhận:

```text
previousState
newState
resourceId
generation
timestamp
reason
duration
```

Không bắt buộc emit event cho mọi internal transition cực nhỏ nếu gây quá nhiều overhead, nhưng các transition quan trọng phải observable.

---

# Summary State Model

```text
RESOURCE MANAGER

Created
   ↓
Starting
   ↓
Ready
   ↓
Running
   ↓
ShuttingDown
   ↓
Stopped
```

```text
RESOURCE

Registered
    ↓
Initializing
    ↓
Ready
    ↓
Idle ⇄ Busy
    │
    ├────────────→ Failed
    │                ↓
    │            Recovering
    │                ↓
    └────────────── Ready

Ready / Idle / Failed
        ↓
    Disposing
        ↓
     Disposed
```

```text
HEALTH

Unknown
   ↓
Healthy
   ↕
Degraded
   ↕
Unhealthy
   ↕
Unavailable
```

Lifecycle State, Health State và Lease State phải được quản lý độc lập nhưng có quan hệ rõ ràng với nhau.
