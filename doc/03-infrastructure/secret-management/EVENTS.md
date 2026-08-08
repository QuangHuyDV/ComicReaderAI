# 03-infrastructure/resource-manager/EVENTS.md

# Resource Manager Events

## Purpose

Tài liệu này định nghĩa các event do `Resource Manager` phát sinh trong CRAI.

Các event phản ánh:

* lifecycle của Resource Manager
* lifecycle của từng managed resource
* thay đổi health
* acquire / release lease
* pool behavior
* recovery
* scope cleanup
* shutdown
* resource pressure và leak detection

Mục tiêu là giúp các module như:

* Logging
* Telemetry
* Scheduler
* Presentation
* OCR
* Translation
* Browser
* Automation

có thể quan sát Resource Manager mà không cần phụ thuộc trực tiếp vào implementation nội bộ.

---

# Event Principles

## Events Are Facts

Event mô tả điều **đã xảy ra**.

Ví dụ:

```text
ResourceInitialized
ResourceFailed
ResourceReleased
```

Không dùng event như command.

Không nên có:

```text
InitializeResource
RestartResource
ReleaseResource
```

Các hành động này phải được thực hiện thông qua contract/API phù hợp.

---

# Event Naming

Tên event sử dụng dạng:

```text
ResourceManager.<EventName>
```

Ví dụ:

```text
ResourceManager.ResourceRegistered
ResourceManager.ResourceStateChanged
ResourceManager.ResourceFailed
ResourceManager.LeaseExpired
```

Tên event ở cấp manager:

```text
ResourceManager.Started
ResourceManager.ShutdownStarted
ResourceManager.Stopped
```

---

# Event Envelope

Mọi event phải sử dụng envelope chuẩn của CRAI Event Bus.

Ví dụ:

```text
eventId
eventType
eventVersion
timestamp
source
correlationId
causationId
payload
metadata
```

---

# Common Resource Payload

Các event liên quan resource nên có tối thiểu:

```text
resourceId
resourceType
scope
ownerModule
generation
```

Có thể bổ sung:

```text
tags
sessionId
taskId
requestId
```

nếu phù hợp với scope.

---

# Common Transition Payload

Các event lifecycle transition nên có:

```text
resourceId
generation
previousState
newState
reason
timestamp
```

Có thể thêm:

```text
transitionDurationMs
trigger
```

---

# Part A — Resource Manager Lifecycle Events

## ResourceManager.Starting

Phát khi Resource Manager bắt đầu bootstrap.

Payload:

```text
managerId
previousState = Created
newState = Starting
timestamp
```

---

## ResourceManager.Ready

Phát khi Resource Manager hoàn tất bootstrap và registry đã hợp lệ.

Payload:

```text
managerId
resourceCount
poolCount
eagerResourceCount
lazyResourceCount
startupDurationMs
timestamp
```

---

## ResourceManager.Started

Phát khi manager chuyển sang trạng thái `Running`.

Payload:

```text
managerId
resourceCount
timestamp
```

---

## ResourceManager.ShutdownStarted

Phát khi graceful shutdown bắt đầu.

Payload:

```text
managerId
activeResourceCount
activeLeaseCount
shutdownTimeoutMs
reason
timestamp
```

---

## ResourceManager.Stopped

Phát khi Resource Manager shutdown hoàn tất.

Payload:

```text
managerId
disposedResourceCount
failedDisposeCount
shutdownDurationMs
timestamp
```

---

## ResourceManager.Failed

Phát khi manager gặp lỗi nghiêm trọng và không thể tiếp tục đảm bảo contract.

Payload:

```text
managerId
previousState
errorCode
errorMessage
recoverable
timestamp
```

Event này chỉ dành cho lỗi ở cấp manager.

Một resource đơn lẻ lỗi phải sử dụng:

```text
ResourceManager.ResourceFailed
```

---

# Part B — Resource Registration Events

## ResourceManager.ResourceRegistered

Phát sau khi descriptor được thêm thành công vào registry.

Payload:

```text
resourceId
resourceType
scope
ownerModule
generation
lifecyclePolicy
recoveryPolicy
dependencies
timestamp
```

---

## ResourceManager.ResourceUnregistered

Phát sau khi resource descriptor bị loại khỏi registry.

Payload:

```text
resourceId
resourceType
scope
generation
reason
timestamp
```

---

## ResourceManager.ResourceRegistrationRejected

Phát khi registration bị từ chối.

Ví dụ:

```text
duplicate resource id
invalid scope
invalid descriptor
invalid dependency
```

Payload:

```text
resourceId
resourceType
errorCode
reason
timestamp
```

Event này chủ yếu phục vụ diagnostics.

---

# Part C — Resource Lifecycle Events

## ResourceManager.ResourceInitializationStarted

Phát trước khi bắt đầu initialize resource.

Payload:

```text
resourceId
resourceType
generation
scope
trigger
timestamp
```

`trigger` có thể là:

```text
startup
lazy-resolve
acquire
pool-expand
recovery
manual
```

---

## ResourceManager.ResourceInitialized

Phát khi initialization thành công.

Payload:

```text
resourceId
resourceType
generation
initializationDurationMs
timestamp
```

---

## ResourceManager.ResourceReady

Phát khi resource đã usable theo contract.

Payload:

```text
resourceId
resourceType
generation
scope
timestamp
```

---

## ResourceManager.ResourceStateChanged

Generic lifecycle transition event.

Payload:

```text
resourceId
resourceType
generation
previousState
newState
reason
timestamp
```

Ví dụ:

```text
Idle → Busy
Busy → Idle
Ready → Failed
Failed → Recovering
```

Không nhất thiết mọi transition đều phải có event chuyên biệt nếu `ResourceStateChanged` đã đủ.

---

## ResourceManager.ResourceBusy

Có thể phát khi resource chuyển:

```text
Idle → Busy
```

Payload:

```text
resourceId
generation
activeLeaseCount
timestamp
```

Event này là optional convenience event.

---

## ResourceManager.ResourceIdle

Phát khi resource không còn active lease và quay về idle.

Payload:

```text
resourceId
generation
idleSince
timestamp
```

---

## ResourceManager.ResourceDisposing

Phát trước khi resource bắt đầu cleanup.

Payload:

```text
resourceId
generation
reason
activeLeaseCount
timestamp
```

---

## ResourceManager.ResourceDisposed

Phát khi lifecycle của một resource generation kết thúc.

Payload:

```text
resourceId
resourceType
generation
reason
lifetimeMs
disposeDurationMs
timestamp
```

---

# Part D — Failure Events

## ResourceManager.ResourceFailed

Phát khi resource chuyển sang `Failed`.

Payload:

```text
resourceId
resourceType
generation
previousState
errorCode
errorMessage
failureCategory
recoverable
timestamp
```

`failureCategory` có thể là:

```text
initialization
health
connection
timeout
process
memory
gpu
provider
internal
unknown
```

---

## ResourceManager.ResourceInitializationFailed

Event chuyên biệt cho initialization failure.

Payload:

```text
resourceId
resourceType
generation
errorCode
errorMessage
initializationDurationMs
retryAllowed
timestamp
```

---

## ResourceManager.ResourceDisposeFailed

Phát khi cleanup gặp lỗi.

Payload:

```text
resourceId
generation
errorCode
errorMessage
forcedCleanupAttempted
timestamp
```

Resource sau event này vẫn phải được coi là unusable.

---

# Part E — Health Events

## ResourceManager.ResourceHealthChanged

Phát khi health state thay đổi.

Payload:

```text
resourceId
resourceType
generation
previousHealth
newHealth
reason
timestamp
```

Ví dụ:

```text
Healthy → Degraded
Degraded → Unhealthy
Unavailable → Healthy
```

---

## ResourceManager.ResourceHealthCheckFailed

Phát khi một health check không thực hiện được hoặc trả kết quả lỗi.

Payload:

```text
resourceId
generation
checkType
errorCode
errorMessage
consecutiveFailures
timestamp
```

---

## ResourceManager.ResourceDegraded

Optional convenience event khi resource chuyển sang:

```text
Degraded
```

Payload có thể bao gồm:

```text
resourceId
generation
reason
latencyMs
memoryUsage
queueDepth
timestamp
```

---

## ResourceManager.ResourceUnavailable

Phát khi resource không thể phục vụ consumer.

Payload:

```text
resourceId
generation
reason
timestamp
```

---

# Part F — Recovery Events

## ResourceManager.ResourceRecoveryStarted

Phát khi manager bắt đầu recovery.

Payload:

```text
resourceId
resourceType
generation
strategy
attempt
maxAttempts
trigger
timestamp
```

`strategy`:

```text
restart
reconnect
recreate
reset
replace
reload
```

---

## ResourceManager.ResourceRecoverySucceeded

Phát khi recovery thành công.

Payload:

```text
resourceId
previousGeneration
generation
strategy
attempt
recoveryDurationMs
timestamp
```

Nếu recreate resource:

```text
previousGeneration != generation
```

---

## ResourceManager.ResourceRecoveryFailed

Phát khi recovery attempt thất bại.

Payload:

```text
resourceId
generation
strategy
attempt
errorCode
errorMessage
retryRemaining
timestamp
```

---

## ResourceManager.ResourceRecreated

Phát khi manager thay instance resource bằng generation mới.

Payload:

```text
resourceId
previousGeneration
newGeneration
reason
timestamp
```

Ví dụ:

```text
browser.default
generation 4 → 5
```

---

# Part G — Lease Events

## ResourceManager.ResourceAcquired

Phát khi acquire thành công.

Payload:

```text
leaseId
resourceId
resourceType
resourceGeneration
scope
owner
acquiredAt
waitDurationMs
timestamp
```

Không nên chứa thông tin nhạy cảm của resource instance.

---

## ResourceManager.ResourceReleased

Phát khi lease được release thành công.

Payload:

```text
leaseId
resourceId
resourceGeneration
owner
heldDurationMs
releasedAt
timestamp
```

---

## ResourceManager.ResourceAcquireTimedOut

Phát khi acquire chờ quá giới hạn.

Payload:

```text
resourceId
resourceType
timeoutMs
waitDurationMs
poolSize
busyCount
waitingCount
timestamp
```

---

## ResourceManager.ResourceAcquireRejected

Phát khi acquire bị từ chối.

Ví dụ:

```text
resource failed
resource disposed
manager shutting down
scope invalid
```

Payload:

```text
resourceId
errorCode
reason
timestamp
```

---

## ResourceManager.LeaseExpired

Phát khi lease vượt thời gian sử dụng cho phép.

Payload:

```text
leaseId
resourceId
resourceGeneration
owner
acquiredAt
expiresAt
heldDurationMs
timestamp
```

Event này có thể được dùng để phát hiện resource leak.

---

## ResourceManager.LeaseForceReleased

Chỉ phát nếu implementation hỗ trợ force reclaim.

Payload:

```text
leaseId
resourceId
resourceGeneration
reason
heldDurationMs
timestamp
```

Phải dùng thận trọng vì không phải resource nào cũng an toàn khi force release.

---

# Part H — Resource Pool Events

## ResourceManager.PoolCreated

Payload:

```text
poolId
resourceType
minSize
maxSize
timestamp
```

---

## ResourceManager.PoolExpanded

Phát khi pool tăng capacity.

Payload:

```text
poolId
resourceType
previousSize
newSize
reason
timestamp
```

Ví dụ reason:

```text
high-demand
startup
manual
adaptive-scaling
```

---

## ResourceManager.PoolShrunk

Payload:

```text
poolId
resourceType
previousSize
newSize
reason
releasedResourceCount
timestamp
```

---

## ResourceManager.PoolExhausted

Phát khi không còn resource available và pool không thể expand.

Payload:

```text
poolId
resourceType
currentSize
maxSize
busyCount
waitingCount
timestamp
```

---

## ResourceManager.PoolDraining

Phát khi pool không nhận acquire mới và đang chờ active lease kết thúc.

Payload:

```text
poolId
activeLeaseCount
reason
timestamp
```

---

## ResourceManager.PoolDisposed

Payload:

```text
poolId
disposedResourceCount
failedDisposeCount
timestamp
```

---

# Part I — Dependency Events

## ResourceManager.ResourceDependencyResolved

Optional diagnostics event.

Payload:

```text
resourceId
dependencyId
dependencyGeneration
timestamp
```

Không nên bật mặc định nếu event volume quá lớn.

---

## ResourceManager.ResourceDependencyFailed

Phát khi dependency khiến resource không thể initialize hoặc continue.

Payload:

```text
resourceId
dependencyId
dependencyState
errorCode
timestamp
```

---

## ResourceManager.CircularDependencyDetected

Phát khi dependency graph phát hiện vòng lặp.

Payload:

```text
resources
dependencyPath
timestamp
```

Ví dụ:

```text
A → B → C → A
```

Đây thường là configuration/development error nghiêm trọng.

---

# Part J — Scope Events

## ResourceManager.ScopeCreated

Payload:

```text
scopeId
scopeType
parentScopeId
owner
timestamp
```

Scope type:

```text
Application
Session
Task
Request
Worker
Module
```

---

## ResourceManager.ScopeClosing

Phát trước khi cleanup scope.

Payload:

```text
scopeId
scopeType
resourceCount
activeLeaseCount
reason
timestamp
```

---

## ResourceManager.ScopeClosed

Payload:

```text
scopeId
scopeType
disposedResourceCount
releasedLeaseCount
cleanupDurationMs
timestamp
```

---

## ResourceManager.ScopeCleanupFailed

Payload:

```text
scopeId
scopeType
failedResources
errorCount
timestamp
```

---

# Part K — Resource Pressure Events

## ResourceManager.MemoryPressureDetected

Phát khi Resource Manager phát hiện áp lực memory.

Payload:

```text
currentUsage
threshold
resourceCount
cacheUsage
timestamp
```

Có thể trigger:

```text
pool shrink
cache cleanup
idle resource disposal
```

---

## ResourceManager.GpuPressureDetected

Nếu CRAI sử dụng GPU:

Payload:

```text
deviceId
usedMemory
availableMemory
threshold
activeResources
timestamp
```

---

## ResourceManager.ResourcePressureRelieved

Phát khi pressure quay về mức bình thường.

Payload:

```text
pressureType
previousUsage
currentUsage
actionsPerformed
timestamp
```

---

# Part L — Leak Detection Events

## ResourceManager.ResourceLeakSuspected

Phát khi manager nghi ngờ resource không được release đúng hạn.

Payload:

```text
resourceId
generation
leaseId
owner
heldDurationMs
expectedMaxDurationMs
timestamp
```

Đây chỉ là cảnh báo.

Không đồng nghĩa leak đã được xác nhận.

---

## ResourceManager.ResourceLeakDetected

Phát khi đã đủ điều kiện xác định leak.

Payload:

```text
resourceId
generation
leaseId
owner
detectedBy
heldDurationMs
timestamp
```

`detectedBy` có thể là:

```text
lease-timeout
scope-close
shutdown
reference-tracking
health-monitor
```

---

# Part M — Configuration Events

## ResourceManager.ResourcePolicyChanged

Nếu hệ thống hỗ trợ runtime configuration reload.

Payload:

```text
resourceId
changedFields
previousPolicyVersion
newPolicyVersion
timestamp
```

Không phải mọi policy đều được phép thay đổi runtime.

---

## ResourceManager.PoolPolicyChanged

Payload:

```text
poolId
changedFields
previousConfiguration
newConfiguration
timestamp
```

Các dữ liệu lớn có thể chỉ ghi changed field thay vì full config.

---

# Event Versioning

Mỗi event phải có version.

Ví dụ:

```text
ResourceManager.ResourceFailed.v1
```

hoặc envelope:

```text
eventType = ResourceManager.ResourceFailed
eventVersion = 1
```

Ưu tiên cách thứ hai để giữ tên event ổn định.

---

# Backward Compatibility

Khi thêm field mới:

```text
v1
```

có thể giữ nguyên nếu field là optional.

Nếu thay đổi:

* semantics
* field type
* required field
* payload structure

phải tăng version.

---

# Event Delivery Semantics

Resource Manager không được giả định event luôn được xử lý exactly once.

Consumer phải chịu được:

```text
at-least-once
duplicate event
out-of-order event
```

nếu Event Bus backend có các đặc tính đó.

Event phải chứa đủ identity:

```text
eventId
resourceId
generation
timestamp
```

để consumer xử lý idempotent.

---

# Ordering

Các event của cùng một resource nên duy trì logical ordering khi có thể.

Ví dụ:

```text
ResourceInitializationStarted
        ↓
ResourceInitialized
        ↓
ResourceReady
```

Không nên quan sát:

```text
ResourceReady
```

trước:

```text
ResourceInitialized
```

trong cùng một generation.

---

# Generation Awareness

Consumer phải sử dụng:

```text
resourceGeneration
```

khi state phụ thuộc vào instance cụ thể.

Ví dụ:

```text
ResourceFailed
generation = 3

ResourceRecreated
generation = 4
```

Nếu event cũ của generation 3 đến trễ, consumer không được dùng nó để đánh dấu generation 4 là Failed.

---

# Correlation

Các event phát sinh từ cùng một workflow nên giữ:

```text
correlationId
```

Ví dụ:

```text
OCR task
    ↓
Acquire OCR worker
    ↓
OCR worker crash
    ↓
Recovery
    ↓
Recreate
    ↓
Retry OCR
```

Toàn bộ chuỗi có thể sử dụng cùng correlationId.

---

# Sensitive Data Rules

Event không được chứa:

* API key
* access token
* secret
* cookie nhạy cảm
* raw authentication header
* private provider credentials
* toàn bộ nội dung OCR/translation nếu không cần thiết

Event Resource Manager chủ yếu mô tả metadata và lifecycle.

---

# Logging Relationship

Event và log không phải cùng một thứ.

Event:

```text
ResourceFailed
```

được phát cho hệ thống.

Logging có thể ghi:

```text
OCR worker crashed while processing task.
```

Không nên dùng Event Bus như logging sink.

---

# Telemetry Relationship

Telemetry có thể subscribe hoặc nhận metric trực tiếp từ các event:

```text
ResourceAcquired
→ acquire_count

ResourceAcquireTimedOut
→ acquire_timeout_count

ResourceRecoverySucceeded
→ recovery_success_count

PoolExhausted
→ pool_exhausted_count
```

Không bắt buộc mọi metric đều phải được tạo từ event.

Các metric tần suất cao có thể được ghi trực tiếp để tránh event overhead.

---

# High-Frequency Event Policy

Một số event có thể xảy ra rất thường xuyên:

```text
ResourceAcquired
ResourceReleased
ResourceBusy
ResourceIdle
```

Trong production, implementation có thể hỗ trợ:

```text
Full
Sampled
MetricsOnly
Disabled
```

cho observability level.

Không được làm thay đổi lifecycle behavior.

---

# Critical Events

Những event sau nên luôn được bật:

```text
ResourceManager.Failed

ResourceManager.ResourceFailed
ResourceManager.ResourceInitializationFailed
ResourceManager.ResourceRecoveryFailed

ResourceManager.ResourceLeakDetected

ResourceManager.PoolExhausted

ResourceManager.ShutdownStarted
ResourceManager.Stopped
```

---

# Recommended Minimal Event Set

Nếu implementation ban đầu cần giữ đơn giản, tối thiểu nên có:

```text
ResourceManager.Ready
ResourceManager.ShutdownStarted
ResourceManager.Stopped

ResourceManager.ResourceRegistered
ResourceManager.ResourceStateChanged
ResourceManager.ResourceHealthChanged
ResourceManager.ResourceFailed
ResourceManager.ResourceDisposed

ResourceManager.ResourceAcquired
ResourceManager.ResourceReleased
ResourceManager.ResourceAcquireTimedOut

ResourceManager.ResourceRecoveryStarted
ResourceManager.ResourceRecoverySucceeded
ResourceManager.ResourceRecoveryFailed

ResourceManager.PoolExhausted

ResourceManager.LeaseExpired
```

Các event chuyên biệt khác có thể bổ sung khi implementation trưởng thành hơn.

---

# CRAI Example — OCR Worker Crash

Luồng:

```text
OCR task
   │
   ▼
ResourceAcquired
   │
   ▼
OCR Worker Busy
   │
   ▼
Worker Crash
   │
   ▼
ResourceHealthChanged
Healthy → Unavailable
   │
   ▼
ResourceFailed
   │
   ▼
ResourceRecoveryStarted
strategy = recreate
   │
   ▼
ResourceRecreated
generation 7 → 8
   │
   ▼
ResourceRecoverySucceeded
   │
   ▼
ResourceReady
```

Task layer có thể quyết định retry OCR mà không cần biết chi tiết cách Resource Manager recreate worker.

---

# CRAI Example — Browser Session

Khi bắt đầu đọc truyện:

```text
ScopeCreated
scopeType = Session
    │
    ▼
ResourceRegistered
browser.context
    │
    ▼
ResourceInitializationStarted
    │
    ▼
ResourceInitialized
    │
    ▼
ResourceReady
```

Khi người dùng đóng phiên:

```text
ScopeClosing
    │
    ▼
ResourceDisposing
    │
    ▼
ResourceDisposed
    │
    ▼
ScopeClosed
```

---

# CRAI Example — OCR Pool Exhaustion

```text
OCR requests increase
      │
      ▼
OCR workers Busy
      │
      ▼
PoolExpanded
      │
      ▼
maxSize reached
      │
      ▼
new request waits
      │
      ▼
PoolExhausted
      │
      ▼
ResourceAcquireTimedOut
```

Telemetry có thể sử dụng chuỗi event này để xác định:

```text
OCR pool too small

hoặc

OCR processing too slow
```

---

# Event Ownership

Resource Manager là producer chính của các event trong namespace:

```text
ResourceManager.*
```

Managed resource không nên tự publish event lifecycle dưới namespace này.

Resource có thể báo internal status cho manager.

Manager chịu trách nhiệm:

```text
validate
transition
record
publish
```

Điều này đảm bảo state machine và event luôn nhất quán.

---

# Summary

Resource Manager event model được chia thành:

```text
Manager Lifecycle
        │
Resource Registration
        │
Resource Lifecycle
        │
Health
        │
Recovery
        │
Lease
        │
Pool
        │
Dependency
        │
Scope
        │
Pressure
        │
Leak Detection
```

Các event quan trọng phải luôn mang:

```text
resourceId
generation
timestamp
```

và khi là transition:

```text
previousState
newState
reason
```

`generation` là thành phần bắt buộc trong các luồng có thể recreate resource để tránh event cũ làm sai trạng thái của instance mới.
