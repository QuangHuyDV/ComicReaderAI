# runtime/WORK_QUEUE.md

# Runtime Work Queue

**Status:** Draft  
**Version:** 2.0

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime lưu giữ tạm thời các `WorkItem` đã được Scheduler admit nhưng chưa bắt đầu execution.

Work Queue nằm giữa Scheduler và Worker Execution:

```text
Runtime Control
    ↓ creates eligible WorkItem
Scheduler
    ↓ admission decision
Work Queue
    ↓ dispatch
Worker Execution
```

Work Queue chỉ chịu trách nhiệm đối với **queued position và bounded buffering**.

Work Queue không:

- quyết định business workflow;
- quyết định priority policy;
- tạo retry;
- sở hữu terminal outcome;
- thực thi WorkItem;
- quản lý Artifact payload;
- thay Runtime Control hoặc Scheduler.

---

## 2. Core Responsibility

Work Queue có một trách nhiệm chính:

> Giữ các WorkItem đã được admission trong một bounded waiting structure cho đến khi chúng được dispatch, replaced hoặc removed.

Work Queue chịu trách nhiệm:

- lưu queued WorkItem reference;
- giữ queued order theo decision từ Scheduler;
- áp dụng bounded capacity;
- hỗ trợ replacement;
- hỗ trợ removal khi WorkItem mất eligibility;
- hỗ trợ dispatch atomically;
- bảo vệ lightweight queue item;
- cung cấp queue metrics;
- hỗ trợ drain khi pause hoặc shutdown.

---

## 3. Architectural Position

```text
BusinessExecutionPlan
        ↓
Runtime Control
        ↓
WorkItem
        ↓
Scheduler
        ↓
ADMIT / DEFER / REJECT / REPLACE
        ↓
Work Queue
        ↓
Worker Execution
```

Queue không được bypass Scheduler.

Worker không được lấy WorkItem chưa được admission.

---

## 4. Queue Philosophy

CRAI là interactive runtime.

Queue được tối ưu cho:

```text
Current useful work
```

không phải:

```text
Preserve every submitted task
```

Do đó Queue phải:

- bounded;
- revision-aware;
- replacement-aware;
- lightweight;
- observable;
- removable;
- không strict FIFO tuyệt đối.

---

## 5. Generic Queue Model

Work Queue là generic runtime infrastructure.

Runtime không định nghĩa queue theo capability cụ thể như:

```text
OCR Queue
Layout Queue
Translation Queue
Presentation Queue
```

Thay vào đó Queue làm việc với:

```text
WorkItem
WorkType
PriorityClass
CapabilityRequirements
ReplacementKey
```

Việc phân tách vật lý thành nhiều queue implementation có thể được quyết định về sau, nhưng public architecture vẫn phải generic.

---

## 6. Logical Queue Classes

Runtime có thể sử dụng các logical queue class:

```text
CONTROL
INTERACTIVE
BACKGROUND
MAINTENANCE
```

### 6.1 Control Queue

Dành cho:

- cancellation;
- pause;
- resume;
- shutdown;
- revision replacement;
- runtime control command.

Control capacity phải được bảo vệ.

### 6.2 Interactive Queue

Dành cho WorkItem phục vụ trực tiếp current user-visible output.

### 6.3 Background Queue

Dành cho prefetch, background analysis hoặc nonblocking enhancement.

### 6.4 Maintenance Queue

Dành cho bounded cleanup, diagnostics aggregation hoặc preload.

Logical queue class không đồng nghĩa dedicated thread hoặc dedicated process.

---

## 7. Queue Item

Queue lưu immutable lightweight item.

```text
QueuedWorkItem
├── WorkItemId
├── AttemptId
├── SessionId
├── RevisionId
├── BusinessStageId
├── WorkType
├── PriorityClass
├── CapabilityRequirements
├── InputArtifactRefs
├── ConfigurationVersion
├── ReplacementKey
├── CancellationScope
├── EnqueuedAt
├── Deadline
└── QueueMetadata
```

Queue Item không chứa:

- image buffer;
- OCR text payload;
- translated document;
- mutable provider DTO;
- secret;
- retry counter source of truth;
- business object mutable state.

---

## 8. Attempt Reference

Nếu Queue dispatch ở attempt level, mỗi queued item phải tham chiếu `AttemptId`.

Retry không mutate queued item cũ.

Luồng đúng:

```text
Attempt 1 failed
        ↓
Runtime Control / Retry Policy
        ↓
Attempt 2 created
        ↓
Scheduler admission
        ↓
New queued item
```

Attempt cũ không được quay lại Queue.

---

## 9. Queue Ownership

Ownership được tách rõ:

```text
Runtime Control
    → WorkItem logical state và authority

Scheduler
    → admission decision

Work Queue
    → queued position

Worker Execution
    → physical execution

Artifact Store
    → runtime artifacts
```

Queue không sở hữu WorkItem terminal state.

---

## 10. Queue Lifecycle

Queue lifecycle:

```text
ADMITTED
  ↓
ENQUEUED
  ↓
WAITING
  ↓
SELECTED
  ↓
DISPATCHED
```

Hoặc:

```text
WAITING
  ↓
REPLACED
```

Hoặc:

```text
WAITING
  ↓
REMOVED
```

Hoặc:

```text
WAITING
  ↓
DRAINED
```

Sau `DISPATCHED`, item không còn thuộc Queue.

Execution lifecycle thuộc `PIPELINE_RUNTIME.md`.

---

## 11. Queue State

Một queued item có thể ở:

```text
ENQUEUED
WAITING
SELECTED
DISPATCHED
REPLACED
REMOVED
DRAINED
```

Queue không sử dụng:

```text
RUNNING
SUCCEEDED
FAILED
CANCELED
STALE
```

vì đó là runtime execution state.

---

## 12. Bounded Capacity

Mọi queue đều phải bounded.

Capacity có thể cấu hình theo:

- logical queue class;
- session;
- WorkType;
- worker pool;
- provider class;
- resource class;
- global runtime limit.

Giá trị cụ thể thuộc `RUNTIME_CONFIG.md`.

---

## 13. Capacity Reservation

Control Queue phải có reserved capacity.

Interactive work cũng có thể có reserved capacity để background work không chiếm hết Queue.

Ví dụ conceptual:

```text
Total Queue Budget
├── Reserved Control Capacity
├── Reserved Interactive Capacity
└── Shared Background/Maintenance Capacity
```

---

## 14. Queue Admission

Queue chỉ nhận item khi Scheduler quyết định `ADMIT`.

Queue có thể từ chối technical enqueue nếu:

- capacity contract bị phá;
- queue đang stopping;
- duplicate queue identity;
- invalid serialization;
- internal integrity failure.

Technical enqueue failure phải được báo về Runtime Control.

Queue không tự chuyển sang `DEFER` hoặc `REJECT` policy.

---

## 15. Queue Ordering

Queue ordering phản ánh policy từ Scheduler.

Queue không tự quyết định priority.

Possible ordering inputs:

- PriorityClass;
- current revision;
- user interaction priority;
- deadline;
- queue age;
- fairness weight;
- replacement state.

Queue implementation có thể dùng heap, deque hoặc partitioned queue, nhưng semantics phải giữ nguyên.

---

## 16. Non-FIFO Behavior

Queue không strict FIFO.

Ví dụ:

```text
Revision 18 waiting
Revision 19 waiting
Revision 20 waiting
```

Khi current revision là 20, Scheduler có thể:

- replace older pending work;
- select Revision 20 trước;
- remove obsolete candidates.

Queue chỉ thực hiện quyết định đó.

---

## 17. Replacement

Replacement áp dụng cho pending queued work có cùng `ReplacementKey`.

```text
ReplacementKey
├── SessionId
├── TargetIdentity
├── WorkType
└── BusinessStageId
```

Default behavior:

```text
newer eligible queued item
    replaces
older pending item with same replacement key
```

Replacement phải atomic.

Replaced item không được dispatch sau khi replacement hoàn tất.

---

## 18. Duplicate Prevention

Queue phải phát hiện duplicate identity:

```text
WorkItemId + AttemptId
```

Một Attempt không được tồn tại nhiều lần trong cùng logical queue scope.

Duplicate enqueue phải:

- bị reject;
- được ghi nhận reason code;
- không tạo thêm execution.

---

## 19. Eligibility Loss

Queued item có thể mất eligibility khi:

- session inactive;
- revision superseded;
- cancellation authority revoked;
- BusinessExecutionPlan replaced;
- dependency invalidated;
- deadline expired;
- provider capability unavailable lâu dài;
- runtime stopping.

Queue không tự quyết định eligibility.

Runtime Control hoặc Scheduler cung cấp removal instruction.

---

## 20. Removal

Queued item có thể bị remove:

- trước dispatch;
- khi revision thay đổi;
- khi session dừng;
- khi cancellation được yêu cầu;
- khi item bị replace;
- khi Queue drain;
- khi internal integrity check thất bại.

Removal phải observable.

---

## 21. Dispatch

Dispatch phải atomic:

```text
SELECTED
    ↓
remove from queue ownership
    ↓
assign to Worker Execution
```

Không được có trạng thái item vừa thuộc Queue vừa thuộc Worker ownership không rõ ràng.

---

## 22. Dispatch Failure

Nếu Worker không nhận được item sau selection:

```text
SELECTED
    ↓ dispatch failure
```

Queue không tự retry execution.

Runtime Control và Scheduler quyết định:

- re-admit;
- defer;
- reject;
- create new Attempt khi phù hợp.

---

## 23. Queue Drain

Drain được dùng khi:

- Scheduler pause;
- session stop;
- runtime shutdown;
- worker pool restart;
- configuration transition.

Drain modes có thể gồm:

```text
REMOVE_NON_CONTROL
REMOVE_BACKGROUND
REMOVE_SESSION
REMOVE_REVISION
REMOVE_ALL
```

Control path phải tiếp tục hoạt động trong quá trình drain nếu có thể.

---

## 24. Pause Behavior

Khi Scheduler paused:

- domain WorkItem mới không được dispatch;
- queued item có thể được giữ hoặc drain theo policy;
- control item vẫn được xử lý;
- cancellation và shutdown vẫn hoạt động;
- queue metrics vẫn hoạt động.

---

## 25. Shutdown Behavior

```text
Stop new admission
        ↓
Stop normal dispatch
        ↓
Remove queued domain work
        ↓
Preserve control operations
        ↓
Drain queue metadata
        ↓
Dispose queue infrastructure
```

Queue không chờ running Attempt vì running Attempt đã thuộc Worker Execution.

---

## 26. Backpressure

Khi Queue đạt soft hoặc hard limit:

### Soft Limit

- defer background admission;
- increase replacement;
- reduce expensive WorkType admission;
- emit saturation telemetry.

### Hard Limit

- reject noncritical admission;
- protect control capacity;
- preserve current interactive work;
- notify Runtime Control.

Queue không được tăng memory vô hạn.

---

## 27. Upstream Coordination

Queue không trực tiếp pause Business Module hoặc upstream stage.

Luồng đúng:

```text
Queue pressure
    ↓
Queue telemetry
    ↓
Scheduler reduces admission
    ↓
Runtime Control creates less downstream work
```

Event Bus có thể thông báo pressure nhưng không điều phối flow.

---

## 28. Memory Model

Queue item phải nhẹ.

Large data nằm trong Artifact Store.

```text
QueuedWorkItem
    ↓ references
ArtifactRef
    ↓
Artifact Store
```

Queue không giữ Artifact Lease dài hạn trừ khi dispatch contract yêu cầu rất ngắn và rõ ràng.

Worker acquire lease trước khi đọc Artifact.

---

## 29. Artifact Reference Validation

Queue có thể kiểm tra technical shape của ArtifactRef.

Queue không xác nhận business validity hoặc artifact authority.

Validation cuối cùng thuộc Runtime Control và Artifact Store.

---

## 30. Cancellation Boundary

Queue hỗ trợ removal theo cancellation instruction.

Queue không sở hữu cancellation authority.

```text
Cancellation Requested
        ↓
Runtime Control revokes authority
        ↓
Queue removal instruction
        ↓
Queued item removed
```

Running Attempt không còn thuộc Queue.

---

## 31. Retry Boundary

Queue không biết retry policy.

Queue không lưu retry counter source of truth.

Retry flow:

```text
Attempt failed
        ↓
Retry Policy
        ↓
New Attempt
        ↓
Scheduler
        ↓
Queue
```

---

## 32. Scheduler Interaction

Scheduler:

- chọn candidate;
- quyết định `ADMIT`, `DEFER`, `REJECT`, `REPLACE`;
- cung cấp ordering metadata;
- chọn next dispatch candidate;
- phản ứng với queue pressure.

Queue:

- lưu item đã admit;
- expose pending state;
- thực hiện replacement/removal;
- dispatch atomically;
- báo metrics.

---

## 33. Worker Interaction

Worker Execution:

- nhận dispatched item;
- acquire Artifact Lease;
- execute Attempt;
- gửi Completion về Runtime Control.

Worker không thao tác trực tiếp queue nội bộ ngoài dispatch contract.

Worker không re-enqueue chính nó.

---

## 34. Queue Metrics

Queue nên cung cấp:

- current length;
- capacity;
- utilization ratio;
- enqueue count;
- dispatch count;
- replace count;
- remove count;
- drain count;
- duplicate rejection count;
- enqueue failure count;
- average wait time;
- P50/P90/P95/P99 wait time;
- queue saturation duration;
- current-revision queue ratio;
- background queue ratio;
- control capacity utilization.

Metrics không chứa user content.

---

## 35. Queue Events

Conceptual events:

```text
WORK_ENQUEUED
WORK_SELECTED
WORK_DISPATCHED
WORK_REPLACED
WORK_REMOVED
QUEUE_SOFT_LIMIT_REACHED
QUEUE_HARD_LIMIT_REACHED
QUEUE_DRAIN_STARTED
QUEUE_DRAIN_COMPLETED
QUEUE_INTEGRITY_FAILED
```

Tên cuối cùng phải tuân theo Event Standard.

Queue không phát terminal outcome cho WorkItem.

---

## 36. Integrity Rules

Queue phải đảm bảo:

- không duplicate Attempt identity;
- capacity không âm;
- item dispatched không còn pending;
- item replaced không thể dispatch;
- item removed không thể dispatch;
- queue state transition hợp lệ;
- ordering metadata không bị mutate sau enqueue;
- queue drain không mất control path trái policy.

---

## 37. Failure Isolation

Nếu một queue partition lỗi:

```text
Stop affected dispatch
        ↓
Preserve control path
        ↓
Notify Runtime Control and Scheduler
        ↓
Reject or drain affected queued items safely
        ↓
Emit diagnostics
```

Queue failure không được corrupt Artifact hoặc business data.

---

## 38. MVP Queue Model

MVP có thể dùng:

```text
Control Queue
Interactive Queue
Background Queue
Maintenance Queue
```

Mỗi queue:

- in-memory;
- bounded;
- process-local;
- typed;
- observable;
- replacement-aware.

Không cần external broker.

---

## 39. MVP Replacement Rule

```text
For the same SessionId + TargetIdentity + WorkType:
keep the newest eligible pending item.
```

Áp dụng chủ yếu cho interactive work thay đổi nhanh.

Không áp dụng mù quáng cho work có business ordering bắt buộc.

---

## 40. MVP Dispatch Rule

1. Dispatch control work trước.
2. Dispatch current interactive work tiếp theo.
3. Dispatch supporting work khi còn capacity.
4. Dispatch background work khi không ảnh hưởng interactive latency.
5. Maintenance chạy với bounded low priority.
6. Không dispatch item đã mất eligibility.

---

## 41. Example: Rapid Scrolling

```text
Revision 30 interactive item queued
Revision 31 arrives
Revision 30 replaced
Revision 32 arrives
Revision 31 replaced
Worker becomes available
Revision 32 dispatched
```

Queue không tự quyết định revision nào mới nhất; Scheduler cung cấp replacement decision.

---

## 42. Example: Queue Saturation

```text
Interactive Queue reaches soft limit
        ↓
Queue emits pressure metric
        ↓
Scheduler defers background work
        ↓
Older replaceable items removed
        ↓
Current-revision item remains eligible
```

---

## 43. Example: Cancellation

```text
Session stop requested
        ↓
Runtime Control revokes authority
        ↓
Queue receives REMOVE_SESSION
        ↓
All queued session items removed
        ↓
Running attempts handled by Cancellation Coordinator
```

---

## 44. Example: Retry

```text
Attempt 1 fails
        ↓
Runtime Control accepts failure
        ↓
Retry Policy approves retry
        ↓
Attempt 2 created
        ↓
Scheduler admits
        ↓
Attempt 2 queued
```

Queue không biết Attempt 2 là retry ngoài metadata tham chiếu.

---

## 45. Architecture Invariants

1. Queue chỉ sở hữu queued position.
2. Queue không sở hữu WorkItem terminal state.
3. Queue không quyết định admission policy.
4. Queue không tự retry.
5. Queue không tự cancel running Attempt.
6. Queue không thực thi WorkItem.
7. Queue không chứa large payload.
8. Queue không chứa secret.
9. Queue luôn bounded.
10. Control capacity được bảo vệ.
11. Replacement phải atomic.
12. Dispatched item không còn thuộc Queue.
13. Replaced hoặc removed item không được dispatch.
14. Retry tạo queued item mới cho Attempt mới.
15. Queue không phụ thuộc business capability cụ thể.
16. Queue không định nghĩa OCR/Translation-specific topology.
17. Queue pressure phải tạo backpressure.
18. Queue metrics không chứa user content.
19. Queue failure không phá runtime correctness.
20. Shutdown dừng admission trước drain.

---

## 46. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem và Attempt lifecycle |
| `SCHEDULER.md` | Admission, ordering và replacement decision |
| `RUNTIME_COMPONENTS.md` | Queue ownership |
| `CANCELLATION.md` | Queued-work removal |
| `RETRY_POLICY.md` | New Attempt creation |
| `MEMORY_MODEL.md` | Artifact Store và Lease |
| `RESOURCE_LIFECYCLE.md` | Ownership transfer và disposal |
| `THREADING_MODEL.md` | Worker dispatch boundary |
| `PERFORMANCE_MODEL.md` | Queue wait và backpressure |
| `RUNTIME_CONFIG.md` | Capacity và policy configuration |
| `RUNTIME_OBSERVABILITY.md` | Queue telemetry |

---

## 47. Completion Criteria

`WORK_QUEUE.md` được xem là đồng bộ khi:

- queue generic, không gắn OCR/Layout/Translation;
- queue chỉ sở hữu queued position;
- lifecycle kết thúc tại dispatch/removal;
- retry counter bị loại khỏi queue ownership;
- cancellation chỉ tạo removal instruction;
- Scheduler decision khớp `ADMIT`, `DEFER`, `REJECT`, `REPLACE`;
- replacement key được định nghĩa;
- queue bounded và lightweight;
- ArtifactRef thay large payload;
- control capacity được bảo vệ;
- metrics và integrity rules rõ ràng;
- startup, pause, drain và shutdown không mâu thuẫn runtime lifecycle.

---

## 48. Summary

Work Queue là bounded waiting infrastructure của CRAI Runtime.

```text
Runtime Control creates work.

Scheduler admits work.

Queue holds waiting position.

Worker executes attempts.

Runtime Control accepts outcomes.
```

Ranh giới cốt lõi:

```text
Queue stores references, not payloads.

Queue owns waiting, not execution.

Queue applies decisions, not policy.

Queue is generic runtime infrastructure.
```
