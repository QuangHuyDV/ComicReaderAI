# 03-infrastructure/resource-manager/ERRORS.md

# Resource Manager Errors

## Purpose

Tài liệu này định nghĩa error taxonomy, error code, nguyên nhân, mức độ ảnh hưởng và chiến lược xử lý lỗi của `Resource Manager`.

Mục tiêu:

* chuẩn hóa lỗi giữa các module
* tránh throw lỗi implementation-specific ra ngoài contract
* phân biệt lỗi resource với lỗi manager
* hỗ trợ retry/recovery nhất quán
* giúp Logging và Telemetry phân loại đúng sự cố

---

# Error Principles

Resource Manager phải phân biệt rõ:

```text
Manager Error
Resource Error
Lease Error
Pool Error
Dependency Error
Scope Error
Health Error
Recovery Error
Lifecycle Error
```

Không sử dụng một error chung như:

```text
RESOURCE_ERROR
```

cho mọi trường hợp.

---

# Error Shape

Error chuẩn nên chứa tối thiểu:

```text
code
message
category
resourceId
generation
recoverable
retryable
timestamp
```

Có thể bổ sung:

```text
managerId
poolId
leaseId
scopeId
dependencyId
cause
metadata
```

---

# Public Error Contract

Các module khác không được phụ thuộc vào:

* exception type nội bộ
* stack trace cụ thể
* runtime-specific error
* OS error trực tiếp
* provider-specific error trực tiếp

Resource Manager phải normalize chúng thành error code chuẩn.

Ví dụ:

```text
ECONNRESET
```

có thể được map thành:

```text
RESOURCE_CONNECTION_LOST
```

---

# Part A — Manager Errors

## RESOURCE_MANAGER_NOT_READY

Manager chưa sẵn sàng phục vụ request.

Possible states:

```text
Created
Starting
```

Typical causes:

* startup chưa hoàn tất
* dependency bootstrap đang chạy

Retryable:

```text
Yes
```

nếu manager vẫn đang khởi động.

---

## RESOURCE_MANAGER_SHUTTING_DOWN

Request mới được gửi khi manager đang graceful shutdown.

State:

```text
ShuttingDown
```

Retryable:

```text
No
```

trong cùng lifecycle.

Caller nên cancel hoặc chuyển workflow sang shutdown path.

---

## RESOURCE_MANAGER_STOPPED

Manager đã dừng.

Retryable:

```text
No
```

trên manager instance hiện tại.

---

## RESOURCE_MANAGER_FAILED

Manager ở trạng thái `Failed`.

Đây là lỗi nghiêm trọng.

Typical causes:

* corrupted registry
* lifecycle invariant violation
* critical bootstrap failure
* dependency graph không hợp lệ
* internal state corruption

Severity:

```text
Critical
```

---

## RESOURCE_MANAGER_INTERNAL_ERROR

Lỗi nội bộ không thuộc taxonomy cụ thể hơn.

Chỉ sử dụng như fallback cuối cùng.

Không nên dùng để che lỗi có thể phân loại.

---

# Part B — Registration Errors

## RESOURCE_ALREADY_EXISTS

Resource ID đã được đăng ký.

Example:

```text
browser.default
```

đã tồn tại nhưng module tiếp tục register lại.

Retryable:

```text
No
```

trừ khi caller thay đổi ID hoặc policy cho phép replace.

---

## RESOURCE_NOT_REGISTERED

Resource chưa tồn tại trong registry.

Khác với resource instance chưa được initialize.

---

## RESOURCE_INVALID_DESCRIPTOR

Descriptor không hợp lệ.

Ví dụ:

* thiếu ID
* thiếu type
* scope không hợp lệ
* lifecycle policy sai
* factory không tồn tại
* dependency malformed

Retryable:

```text
No
```

cho tới khi configuration được sửa.

---

## RESOURCE_INVALID_ID

Resource ID không đúng convention hoặc chứa giá trị không hợp lệ.

---

## RESOURCE_REGISTRATION_REJECTED

Registration bị policy từ chối.

Ví dụ:

* dynamic registration bị tắt
* module không được phép đăng ký resource loại này
* manager đang shutdown

---

## RESOURCE_UNREGISTER_FAILED

Không thể unregister resource.

Typical causes:

* vẫn còn lease
* resource đang busy
* dependent vẫn tồn tại
* disposal thất bại

---

# Part C — Lookup / Resolve Errors

## RESOURCE_NOT_FOUND

Không tìm thấy resource theo ID/type yêu cầu.

---

## RESOURCE_RESOLVE_FAILED

Resource tồn tại trong registry nhưng manager không thể resolve usable instance.

Typical causes:

* initialization failed
* dependency failed
* invalid lifecycle state
* factory failed

---

## RESOURCE_TYPE_MISMATCH

Caller yêu cầu resource với type không tương thích với descriptor đã đăng ký.

Ví dụ:

```text
resourceId = translator.default

expected = TranslatorClient
actual   = BrowserContext
```

---

## RESOURCE_SCOPE_MISMATCH

Resource tồn tại nhưng không thuộc scope mà caller có quyền truy cập.

---

## RESOURCE_GENERATION_MISMATCH

Caller hoặc lease tham chiếu generation cũ.

Ví dụ:

```text
lease generation = 4
current generation = 5
```

Phải từ chối thao tác để tránh dùng instance đã bị thay thế.

---

# Part D — Initialization Errors

## RESOURCE_INITIALIZATION_FAILED

Initialization thất bại.

Generic wrapper cho lỗi init.

Nên đi kèm `cause`.

---

## RESOURCE_INITIALIZATION_TIMEOUT

Initialization vượt quá giới hạn thời gian.

Ví dụ:

* OCR model load quá lâu
* browser process không start
* external provider handshake treo

Retryable:

```text
Depends on policy
```

---

## RESOURCE_FACTORY_FAILED

Factory tạo resource bị lỗi.

---

## RESOURCE_FACTORY_RETURNED_NULL

Factory không trả instance hợp lệ.

Đây thường là programming/configuration error.

---

## RESOURCE_DOUBLE_INITIALIZATION

Initialization được gọi đồng thời hoặc lặp sai lifecycle.

Ví dụ:

```text
Initializing
→ initialize() again
```

Severity:

```text
Error
```

Có thể phản ánh race condition.

---

## RESOURCE_INITIALIZATION_CANCELLED

Initialization bị cancel.

Typical causes:

* shutdown
* scope closed
* task cancelled

Không nhất thiết là lỗi nghiêm trọng.

---

# Part E — Lifecycle Errors

## RESOURCE_INVALID_STATE

Operation không hợp lệ với lifecycle hiện tại.

Ví dụ:

```text
acquire()
while state = Failed
```

hoặc:

```text
resolve()
while state = Disposed
```

---

## RESOURCE_INVALID_TRANSITION

State transition không hợp lệ.

Ví dụ:

```text
Disposed → Ready
```

hoặc:

```text
Registered → Busy
```

Severity:

```text
High
```

Vì có thể cho thấy bug trong manager.

---

## RESOURCE_ALREADY_DISPOSED

Thao tác được thực hiện trên resource đã dispose.

---

## RESOURCE_DISPOSING

Resource đang cleanup và không thể nhận operation mới.

---

## RESOURCE_BUSY

Operation yêu cầu resource idle nhưng resource đang Busy.

Ví dụ:

* exclusive maintenance
* unregister
* forced policy update

---

## RESOURCE_IN_USE

Resource vẫn còn active lease hoặc dependent.

Khác với `RESOURCE_BUSY`:

`RESOURCE_BUSY` mô tả runtime state.

`RESOURCE_IN_USE` mô tả ownership/reference constraint.

---

# Part F — Acquire / Lease Errors

## RESOURCE_ACQUIRE_FAILED

Acquire thất bại nhưng không thuộc error cụ thể hơn.

---

## RESOURCE_ACQUIRE_TIMEOUT

Caller chờ resource vượt quá acquire timeout.

---

## RESOURCE_ACQUIRE_REJECTED

Acquire bị từ chối ngay.

Ví dụ:

* resource failed
* manager shutting down
* scope invalid
* access policy denied

---

## RESOURCE_LEASE_INVALID

Lease không hợp lệ.

Typical causes:

* malformed lease
* resource không tồn tại
* owner mismatch

---

## RESOURCE_LEASE_EXPIRED

Lease vượt thời hạn.

---

## RESOURCE_LEASE_ALREADY_RELEASED

Caller release cùng lease nhiều lần.

Đây có thể là dấu hiệu lifecycle misuse.

---

## RESOURCE_LEASE_OWNER_MISMATCH

Một owner cố release hoặc sử dụng lease thuộc owner khác.

---

## RESOURCE_LEASE_GENERATION_MISMATCH

Lease thuộc resource generation cũ.

---

## RESOURCE_LEASE_LIMIT_EXCEEDED

Owner hoặc scope đã vượt số lease cho phép.

Có thể dùng để bảo vệ:

* browser contexts
* OCR workers
* GPU sessions
* AI model sessions

---

# Part G — Pool Errors

## RESOURCE_POOL_NOT_FOUND

Không tìm thấy pool.

---

## RESOURCE_POOL_EXHAUSTED

Pool đạt `maxSize` và không còn instance khả dụng.

Retryable:

```text
Yes
```

nếu caller có thể chờ hoặc retry sau.

---

## RESOURCE_POOL_ACQUIRE_TIMEOUT

Đã chờ pool nhưng không nhận được resource trong thời gian cho phép.

---

## RESOURCE_POOL_EXPANSION_FAILED

Pool muốn tăng size nhưng không tạo được resource mới.

Typical causes:

* factory failure
* memory pressure
* GPU memory thiếu
* browser process limit
* provider limit

---

## RESOURCE_POOL_SHRINK_FAILED

Không thể dispose resource idle khi shrink.

---

## RESOURCE_POOL_INVALID_CONFIGURATION

Pool config không hợp lệ.

Ví dụ:

```text
minSize > maxSize
```

hoặc:

```text
maxSize <= 0
```

---

## RESOURCE_POOL_DRAINING

Acquire mới được gửi tới pool đang `Draining`.

---

## RESOURCE_POOL_DISPOSED

Operation được thực hiện trên pool đã dispose.

---

# Part H — Dependency Errors

## RESOURCE_DEPENDENCY_NOT_FOUND

Dependency được khai báo nhưng không tồn tại trong registry.

---

## RESOURCE_DEPENDENCY_FAILED

Dependency tồn tại nhưng không usable.

Ví dụ:

```text
Translator
depends on
HTTP Client

HTTP Client = Failed
```

Translator không thể initialize.

---

## RESOURCE_CIRCULAR_DEPENDENCY

Dependency graph có vòng lặp.

Ví dụ:

```text
A → B → C → A
```

Severity:

```text
Critical configuration error
```

---

## RESOURCE_DEPENDENCY_DISPOSED

Dependent cố sử dụng dependency đã dispose.

Đây thường là lỗi ordering.

---

## RESOURCE_DEPENDENCY_SCOPE_INVALID

Dependency có scope ngắn hơn dependent theo cách không an toàn.

Ví dụ:

```text
Application resource
depends on
Request resource
```

thường không hợp lệ.

---

# Part I — Health Errors

## RESOURCE_HEALTH_CHECK_FAILED

Health check không thể hoàn tất.

Khác với resource unhealthy.

Health check có thể fail vì chính cơ chế kiểm tra gặp lỗi.

---

## RESOURCE_UNHEALTHY

Resource được xác định không còn đáp ứng health contract.

---

## RESOURCE_UNAVAILABLE

Resource không thể phục vụ request.

Ví dụ:

* worker process chết
* provider unavailable
* browser crash
* GPU context lost

---

## RESOURCE_DEGRADED

Có thể trả dưới dạng warning/error tùy operation.

Resource vẫn usable nhưng hiệu năng hoặc độ tin cậy suy giảm.

---

# Part J — Recovery Errors

## RESOURCE_RECOVERY_FAILED

Recovery hoàn toàn thất bại.

---

## RESOURCE_RECOVERY_TIMEOUT

Recovery vượt thời gian cho phép.

---

## RESOURCE_RECOVERY_LIMIT_EXCEEDED

Đã vượt số lần retry/recovery tối đa.

Ví dụ:

```text
attempt = 5
maxAttempts = 5
```

---

## RESOURCE_RECOVERY_NOT_SUPPORTED

Policy yêu cầu recovery nhưng resource không hỗ trợ strategy đó.

---

## RESOURCE_RECOVERY_CONFLICT

Một recovery operation khác đang chạy trên cùng resource generation.

Manager không được chạy recovery đồng thời nhiều lần.

---

## RESOURCE_RECREATE_FAILED

Không thể tạo generation mới.

---

# Part K — Disposal Errors

## RESOURCE_DISPOSE_FAILED

Cleanup resource thất bại.

---

## RESOURCE_DISPOSE_TIMEOUT

Dispose vượt giới hạn thời gian.

---

## RESOURCE_DOUBLE_DISPOSE

Dispose được gọi lặp ngoài lifecycle cho phép.

Có thể phản ánh race condition hoặc ownership bug.

---

## RESOURCE_FORCE_DISPOSE_REQUIRED

Graceful dispose không thể hoàn tất và policy yêu cầu forced cleanup.

Đây có thể là internal control error/status tùy implementation.

---

# Part L — Scope Errors

## RESOURCE_SCOPE_NOT_FOUND

Scope không tồn tại.

---

## RESOURCE_SCOPE_CLOSED

Operation cố sử dụng scope đã đóng.

---

## RESOURCE_SCOPE_INVALID

Scope descriptor hoặc hierarchy không hợp lệ.

---

## RESOURCE_SCOPE_CLEANUP_FAILED

Một hoặc nhiều resource không cleanup được khi scope đóng.

---

## RESOURCE_SCOPE_HAS_ACTIVE_LEASES

Scope đang đóng nhưng vẫn có active lease.

Manager có thể:

* wait
* warn
* expire
* force release

tùy policy.

---

## RESOURCE_SCOPE_PARENT_CLOSED

Child scope cố hoạt động khi parent scope đã đóng.

---

# Part M — Pressure / Capacity Errors

## RESOURCE_MEMORY_LIMIT_EXCEEDED

Resource hoặc manager vượt memory policy.

---

## RESOURCE_GPU_MEMORY_LIMIT_EXCEEDED

GPU memory không đủ cho operation/resource mới.

---

## RESOURCE_CAPACITY_LIMIT_EXCEEDED

Đạt giới hạn resource chung.

Ví dụ:

* worker count
* browser count
* connection count

---

## RESOURCE_OS_LIMIT_REACHED

Chạm giới hạn hệ điều hành.

Ví dụ:

* file descriptor
* process limit
* thread limit

Không được expose raw OS error như public contract nếu có thể normalize.

---

# Part N — Leak Errors

## RESOURCE_LEAK_SUSPECTED

Resource/lease có dấu hiệu không được release đúng hạn.

Severity:

```text
Warning
```

---

## RESOURCE_LEAK_DETECTED

Manager xác định leak theo policy.

Severity:

```text
Error
```

hoặc:

```text
Critical
```

nếu làm cạn tài nguyên hệ thống.

---

# Part O — Policy Errors

## RESOURCE_POLICY_INVALID

Lifecycle/recovery/resource policy không hợp lệ.

---

## RESOURCE_POLICY_CHANGE_REJECTED

Runtime policy update bị từ chối.

Ví dụ:

* field immutable
* resource đang Busy
* update làm vi phạm invariant

---

## RESOURCE_POLICY_NOT_SUPPORTED

Implementation hiện tại không hỗ trợ policy yêu cầu.

---

# Error Severity

Recommended levels:

| Severity | Meaning                                |
| -------- | -------------------------------------- |
| Debug    | Diagnostic-only                        |
| Info     | Expected lifecycle condition           |
| Warning  | Có vấn đề nhưng hệ thống vẫn hoạt động |
| Error    | Operation/resource thất bại            |
| Critical | Manager/system integrity bị ảnh hưởng  |

---

# Recommended Severity Mapping

| Error                          | Severity        |
| ------------------------------ | --------------- |
| RESOURCE_NOT_FOUND             | Error           |
| RESOURCE_ACQUIRE_TIMEOUT       | Warning / Error |
| RESOURCE_POOL_EXHAUSTED        | Warning         |
| RESOURCE_LEASE_EXPIRED         | Warning         |
| RESOURCE_LEAK_SUSPECTED        | Warning         |
| RESOURCE_LEAK_DETECTED         | Error           |
| RESOURCE_INITIALIZATION_FAILED | Error           |
| RESOURCE_RECOVERY_FAILED       | Error           |
| RESOURCE_INVALID_TRANSITION    | Critical        |
| RESOURCE_MANAGER_FAILED        | Critical        |
| RESOURCE_CIRCULAR_DEPENDENCY   | Critical        |

Severity có thể được điều chỉnh theo context.

---

# Retry Classification

Không phải mọi error đều retry được.

## Normally Retryable

```text
RESOURCE_ACQUIRE_TIMEOUT
RESOURCE_POOL_EXHAUSTED
RESOURCE_INITIALIZATION_TIMEOUT
RESOURCE_UNAVAILABLE
RESOURCE_RECOVERY_TIMEOUT
```

tùy policy.

---

## Normally Non-Retryable

```text
RESOURCE_INVALID_DESCRIPTOR
RESOURCE_TYPE_MISMATCH
RESOURCE_INVALID_TRANSITION
RESOURCE_CIRCULAR_DEPENDENCY
RESOURCE_SCOPE_MISMATCH
RESOURCE_ALREADY_DISPOSED
RESOURCE_MANAGER_STOPPED
```

Retry mà không thay đổi điều kiện sẽ không giải quyết lỗi.

---

# Recovery Classification

## Recoverable

Có thể áp dụng:

```text
Reconnect
Restart
Reset
Recreate
Reload
```

Ví dụ:

```text
RESOURCE_UNAVAILABLE
RESOURCE_HEALTH_CHECK_FAILED
RESOURCE_CONNECTION_LOST
RESOURCE_INITIALIZATION_TIMEOUT
```

---

## Non-Recoverable

Ví dụ:

```text
RESOURCE_INVALID_DESCRIPTOR
RESOURCE_CIRCULAR_DEPENDENCY
RESOURCE_INVALID_TRANSITION
RESOURCE_TYPE_MISMATCH
```

Cần sửa configuration/code.

---

# Error Propagation Rules

Resource Manager phải:

1. nhận lỗi gốc
2. normalize
3. attach context
4. xác định retry/recovery
5. log
6. emit event phù hợp
7. trả public error cho caller

Ví dụ:

```text
Browser process
throws ECONNREFUSED
      ↓
Resource Manager
      ↓
RESOURCE_UNAVAILABLE
      ↓
ResourceFailed
      ↓
RecoveryStarted
```

---

# Error Cause Preservation

Không nên mất lỗi gốc.

Internal error có thể giữ:

```text
cause
```

để diagnostics.

Nhưng public API không cần expose toàn bộ stack trace.

---

# Error and Event Relationship

Không phải mọi error đều tạo event.

Ví dụ:

```text
RESOURCE_NOT_FOUND
```

từ một caller sai ID có thể chỉ log/debug.

Nhưng:

```text
RESOURCE_INITIALIZATION_FAILED
```

nên phát:

```text
ResourceInitializationFailed
ResourceFailed
```

---

# Recommended Event Mapping

| Error                          | Event                        |
| ------------------------------ | ---------------------------- |
| RESOURCE_INITIALIZATION_FAILED | ResourceInitializationFailed |
| RESOURCE_RECOVERY_FAILED       | ResourceRecoveryFailed       |
| RESOURCE_POOL_EXHAUSTED        | PoolExhausted                |
| RESOURCE_ACQUIRE_TIMEOUT       | ResourceAcquireTimedOut      |
| RESOURCE_LEASE_EXPIRED         | LeaseExpired                 |
| RESOURCE_LEAK_DETECTED         | ResourceLeakDetected         |
| RESOURCE_DISPOSE_FAILED        | ResourceDisposeFailed        |
| RESOURCE_DEPENDENCY_FAILED     | ResourceDependencyFailed     |
| RESOURCE_CIRCULAR_DEPENDENCY   | CircularDependencyDetected   |
| RESOURCE_MANAGER_FAILED        | ResourceManager.Failed       |

---

# Logging Rules

Log error phải chứa context tối thiểu:

```text
code
resourceId
generation
state
scope
operation
correlationId
```

Nếu liên quan lease:

```text
leaseId
owner
heldDuration
```

Nếu liên quan pool:

```text
poolId
size
busyCount
waitingCount
```

---

# Sensitive Data

Không được ghi vào error:

* access token
* API secret
* password
* session cookie
* raw Authorization header
* private provider credential

Có thể ghi:

```text
provider = openai
statusCode = 401
```

nhưng không ghi credential.

---

# Error Code Stability

Error code là một phần của public contract.

Không được đổi tên tùy tiện.

Ví dụ:

```text
RESOURCE_NOT_FOUND
```

không nên đổi thành:

```text
RESOURCE_MISSING
```

sau khi caller đã phụ thuộc vào code cũ.

---

# Error Versioning

Nếu semantics của error thay đổi đáng kể:

* giữ code cũ
* hoặc version contract
* hoặc thêm error mới

Không reuse cùng code cho ý nghĩa khác.

---

# CRAI Example — OCR Worker Failure

```text
OCR Worker
   ↓
process crash
   ↓
RESOURCE_UNAVAILABLE
   ↓
resource → Failed
   ↓
ResourceFailed event
   ↓
RecoveryStarted
   ↓
recreate worker
```

Nếu recreate thành công:

```text
generation 3 → 4
```

workflow OCR có thể retry.

---

# CRAI Example — OCR Pool Exhaustion

```text
All OCR workers Busy
        ↓
Pool maxSize reached
        ↓
RESOURCE_POOL_EXHAUSTED
        ↓
caller waits
        ↓
timeout
        ↓
RESOURCE_ACQUIRE_TIMEOUT
```

Hai lỗi này không giống nhau:

`POOL_EXHAUSTED` mô tả trạng thái pool.

`ACQUIRE_TIMEOUT` mô tả kết quả operation của caller.

---

# CRAI Example — Browser Context Leak

```text
Session closed
    ↓
BrowserContext lease vẫn Active
    ↓
RESOURCE_LEAK_SUSPECTED
    ↓
grace period
    ↓
lease vẫn tồn tại
    ↓
RESOURCE_LEAK_DETECTED
```

Manager có thể:

```text
force close context
```

nếu policy và loại resource cho phép.

---

# CRAI Example — Stale Lease

```text
OCR worker generation = 8
        ↓
worker crash
        ↓
recreated
generation = 9
        ↓
old lease tries release/use
        ↓
RESOURCE_LEASE_GENERATION_MISMATCH
```

Manager không được áp dụng old lease lên generation 9.

---

# Minimal Error Set for Initial Implementation

CRAI bản đầu chưa cần implement toàn bộ taxonomy.

Tối thiểu nên hỗ trợ:

```text
RESOURCE_NOT_FOUND
RESOURCE_ALREADY_EXISTS
RESOURCE_INVALID_DESCRIPTOR

RESOURCE_INITIALIZATION_FAILED
RESOURCE_INITIALIZATION_TIMEOUT

RESOURCE_INVALID_STATE
RESOURCE_INVALID_TRANSITION
RESOURCE_ALREADY_DISPOSED

RESOURCE_ACQUIRE_TIMEOUT
RESOURCE_LEASE_INVALID
RESOURCE_LEASE_ALREADY_RELEASED
RESOURCE_LEASE_GENERATION_MISMATCH

RESOURCE_POOL_EXHAUSTED

RESOURCE_DEPENDENCY_NOT_FOUND
RESOURCE_DEPENDENCY_FAILED
RESOURCE_CIRCULAR_DEPENDENCY

RESOURCE_UNAVAILABLE

RESOURCE_RECOVERY_FAILED

RESOURCE_DISPOSE_FAILED

RESOURCE_SCOPE_CLOSED

RESOURCE_MANAGER_NOT_READY
RESOURCE_MANAGER_SHUTTING_DOWN
RESOURCE_MANAGER_FAILED
```

Các lỗi nâng cao như:

```text
GPU memory
leak detection
adaptive pool
policy hot reload
OS limit
```

có thể bổ sung khi các capability tương ứng được triển khai.

---

# Summary

Error model của Resource Manager phải luôn trả lời được:

```text
What failed?
Why did it fail?
Which resource?
Which generation?
At which lifecycle state?
Can it be retried?
Can it be recovered?
What should happen next?
```

Cấu trúc chuẩn:

```text
ResourceError
├── identity
│   ├── resourceId
│   └── generation
│
├── classification
│   ├── code
│   ├── category
│   └── severity
│
├── behavior
│   ├── retryable
│   └── recoverable
│
└── diagnostics
    ├── message
    ├── cause
    └── metadata
```

Resource Manager không được để lỗi implementation-specific lan trực tiếp sang các module nghiệp vụ; mọi lỗi cần được normalize qua taxonomy này trước khi đi qua public contract.
