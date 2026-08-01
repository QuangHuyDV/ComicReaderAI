# runtime/SCHEDULER.md

# Runtime Scheduler

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Scheduler đánh giá, ưu tiên và đưa ra admission decision cho `WorkItem`.

Scheduler nằm giữa Runtime Control và Worker Execution:

```text
Runtime Control
    ↓ submits eligible WorkItem
Scheduler
    ↓ admission decision
Work Queue / Worker Execution
```

Scheduler không sở hữu business workflow, retry policy, terminal outcome hoặc runtime authority.

---

## 2. Core Responsibility

Scheduler chịu trách nhiệm duy nhất ở cấp kiến trúc:

> Quyết định WorkItem nào được phép tiến gần hơn tới execution, vào thời điểm nào và dưới resource budget nào.

Scheduler chịu trách nhiệm:

- đánh giá candidate WorkItem;
- kiểm tra admission condition;
- ưu tiên current-revision work;
- áp dụng priority class;
- giới hạn concurrency;
- match worker capability;
- áp dụng fairness;
- phản ứng với resource pressure;
- bảo vệ control path và UI responsiveness;
- loại bỏ hoặc thay thế queued work không còn giá trị;
- phát scheduling decision có reason code.

Scheduler không:

- tạo BusinessExecutionPlan;
- thay đổi business dependency;
- tạo WorkItem;
- tạo retry Attempt;
- quyết định terminal outcome;
- xác nhận stale result cuối cùng;
- tự cancel running Attempt;
- tự lookup cache;
- chọn provider theo business policy;
- commit Artifact;
- commit UI;
- sở hữu durable persistence;
- giải phóng mọi resource vật lý.

---

## 3. Architectural Position

```text
BusinessExecutionPlan
        ↓
Runtime Control
        ↓ creates WorkItem
Scheduler
        ↓
ADMIT / DEFER / REJECT / REPLACE
        ↓
Work Queue
        ↓
Worker Execution
```

Scheduler không được bypass Runtime Control.

---

## 4. Scheduling Philosophy

CRAI là ứng dụng đọc tương tác, không phải batch-processing system.

Scheduler tối ưu cho:

```text
Useful Current-Revision Output
```

thay vì:

```text
Completion of Every Submitted WorkItem
```

Nguyên tắc trung tâm:

```text
Current Revision First
Bounded Concurrency
Drop Obsolete Pending Work
Protect Control Path
Reject Stale Results at Runtime Control
```

---

## 5. Scheduler Inputs

Scheduler nhận read-only scheduling inputs từ các runtime component.

### 5.1 Runtime Control

Cung cấp:

- active session state;
- current revision;
- WorkItem eligibility;
- cancellation state;
- shutdown state;
- priority elevation từ user action;
- dependency readiness.

### 5.2 Revision Store

Cung cấp:

- revision identity;
- revision age;
- current/superseded state;
- revision metadata;
- resource linkage cần cho admission.

Revision Store không cấp authority; authority thuộc Runtime Control.

### 5.3 Work Queue

Cung cấp:

- pending WorkItem;
- queue capacity;
- queue saturation;
- queued-at time;
- replacement candidates.

### 5.4 Worker Execution

Cung cấp:

- available worker slots;
- worker capability;
- worker health;
- execution-context availability;
- worker-pool utilization.

### 5.5 Provider Manager

Cung cấp:

- provider availability;
- provider capability;
- provider concurrency state;
- provider health;
- rate-limit state.

Scheduler sử dụng các dữ liệu này để admission, không tự chọn provider theo business policy.

### 5.6 Resource Monitor

Cung cấp:

- CPU pressure;
- memory pressure;
- GPU pressure;
- network availability;
- artifact pressure;
- temporary-storage pressure.

### 5.7 Runtime Configuration

Cung cấp immutable configuration snapshot cho:

- queue capacity;
- concurrency limit;
- priority policy;
- fairness;
- resource budget;
- admission threshold;
- preemption policy.

---

## 6. Scheduling Unit

Đơn vị scheduling là `WorkItem`.

Canonical `WorkItem` được định nghĩa tại `PIPELINE_RUNTIME.md`.

Scheduler chỉ sử dụng scheduling metadata cần thiết:

```text
WorkItem
├── WorkItemId
├── SessionId
├── RevisionId
├── BusinessStageId
├── WorkType
├── PriorityClass
├── CreatedAt
├── Deadline
├── CostHint
├── CapabilityRequirements
├── InputArtifactRefs
├── ConfigurationVersion
└── CancellationScope
```

Scheduler không đọc hoặc mutate business payload.

Large payload chỉ được truy cập bằng `ArtifactRef`.

---

## 7. Scheduling Decision

Scheduler chỉ tạo một trong bốn admission decision:

| Decision | Meaning |
|---|---|
| `ADMIT` | Cho phép WorkItem tiến vào queue hoặc execution slot |
| `DEFER` | Giữ WorkItem ở trạng thái pending để đánh giá lại |
| `REJECT` | Từ chối admission vì WorkItem không còn hoặc chưa bao giờ hợp lệ |
| `REPLACE` | Thay queued WorkItem cũ bằng WorkItem mới có giá trị cao hơn |

Scheduler không tạo các decision sau:

```text
RETRY
FAIL
SUCCEED
CANCEL
CACHE_HIT
```

Các quyết định đó thuộc Runtime Control, Retry Policy, Cache Policy hoặc Authority Validation.

---

## 8. Decision Reason Codes

Mỗi decision phải có reason code.

Ví dụ:

```text
CURRENT_REVISION
NEWER_REVISION_AVAILABLE
SESSION_INACTIVE
REVISION_SUPERSEDED
DEPENDENCY_NOT_READY
QUEUE_CAPACITY_AVAILABLE
QUEUE_CAPACITY_EXCEEDED
REPLACED_BY_NEWER_WORK
NO_COMPATIBLE_WORKER
CONCURRENCY_LIMIT
RESOURCE_BUDGET_EXCEEDED
PROVIDER_UNAVAILABLE
PROVIDER_SATURATED
DEADLINE_EXCEEDED
RUNTIME_PAUSED
RUNTIME_STOPPING
CONTROL_CAPACITY_RESERVED
```

Reason code phục vụ:

- diagnostics;
- metrics;
- deterministic testing;
- scheduler tuning;
- explanation.

---

## 9. Priority Classes

CRAI sử dụng một tập priority class nhỏ và ổn định.

### 9.1 Interactive Critical

Work trực tiếp cần để tạo output cho nội dung hiện tại người dùng đang xem.

### 9.2 Interactive Supporting

Work cải thiện trải nghiệm hiện tại nhưng không chặn output chính.

### 9.3 Background

Work hữu ích nhưng không cần cho tương tác hiện tại.

### 9.4 Maintenance

Cleanup, metrics aggregation, bounded diagnostics hoặc preloading.

Thứ tự mặc định:

```text
INTERACTIVE_CRITICAL
    >
INTERACTIVE_SUPPORTING
    >
BACKGROUND
    >
MAINTENANCE
```

---

## 10. Priority Inputs

Priority decision có thể xét:

```text
Priority Class
Current Revision
User Visibility
User Interaction
Business Priority
Deadline Pressure
Queue Age
Cost Hint
Resource Cost
Provider Availability
Obsolescence Risk
Fairness Weight
```

Priority score cụ thể là implementation detail, nhưng phải:

- deterministic;
- explainable;
- bounded;
- observable;
- testable.

---

## 11. Current Revision Preference

Current revision là tín hiệu scheduling mạnh nhất.

Default rule:

```text
WorkItem.RevisionId == Session.CurrentRevisionId
    → eligible for current-revision priority
```

WorkItem của revision cũ:

- thường bị `REJECT`;
- có thể bị `REPLACE` khi vẫn đang queued;
- chỉ được tiếp tục nếu Runtime Control vẫn đánh dấu eligible;
- không bao giờ được Scheduler tự cấp commit authority.

Scheduler không phải authority validator cuối cùng.

---

## 12. Obsolete Work Elimination

Obsolete work nên bị loại càng sớm càng tốt.

Các điểm kiểm tra:

### Before Admission

Không admit WorkItem đã mất eligibility.

### While Pending

Từ chối hoặc replace WorkItem khi revision mới xuất hiện.

### Before Queue Dispatch

Đánh giá lại current revision, cancellation state và resource budget.

### Before Worker Assignment

Xác nhận worker compatibility và Runtime Control eligibility.

### After Execution

Không thuộc Scheduler. Runtime Control thực hiện authority validation.

---

## 13. Admission Preconditions

Trước khi `ADMIT`, Scheduler phải xác nhận:

- Runtime đang cho phép admission;
- session còn active;
- revision còn eligible;
- dependency đã ready;
- cancellation chưa revoke work;
- input ArtifactRef tồn tại theo metadata được cấp;
- queue capacity còn;
- compatible worker hoặc execution pool tồn tại;
- concurrency budget còn;
- resource budget còn;
- provider runtime state phù hợp khi cần;
- deadline chưa hết;
- control capacity vẫn được bảo vệ.

---

## 14. Worker Capability Matching

WorkItem có thể khai báo `CapabilityRequirements`.

Ví dụ:

```text
CapabilityRequirements
├── WorkType
├── ExecutionClass
├── CPU/GPU Requirement
├── Local/Remote Eligibility
├── Language Capability
├── Model Capability
├── Memory Class
└── Provider Capability
```

Scheduler chỉ admit hoặc dispatch tới worker tương thích.

Scheduler không biết implementation chi tiết bên trong Business Module.

---

## 15. Resource Classes

Các resource class logic có thể gồm:

```text
CONTROL
UI
CPU_LIGHT
CPU_HEAVY
GPU
NETWORK
IO
```

Scheduler phải ngăn một resource class làm starve:

- control command;
- cancellation;
- UI commit;
- observation/capture control;
- shutdown;
- diagnostics tối thiểu cần thiết.

---

## 16. Concurrency Control

Concurrency phải bounded.

Giới hạn có thể áp dụng theo:

- global Runtime;
- session;
- WorkType;
- worker pool;
- provider;
- execution class;
- GPU context;
- network provider;
- resource class.

Không hard-code concurrency theo capability nội bộ như OCR hoặc Layout tại tài liệu này.

Giá trị cụ thể thuộc `RUNTIME_CONFIG.md`.

---

## 17. Admission Budget

Admission decision phải xét tổng hợp:

```text
Queue Budget
Worker Budget
Provider Budget
CPU Budget
GPU Budget
Memory Budget
Artifact Budget
Temporary Storage Budget
Control Capacity Reserve
```

Không có một budget đơn lẻ nào được coi là đủ.

---

## 18. Control Capacity

Runtime phải giữ capacity cho:

- stop;
- pause;
- resume;
- cancellation;
- shutdown;
- revision replacement;
- user-triggered replan;
- fatal error handling.

Control command không được xếp sau domain work dài hạn trong cùng bottleneck mà không có capacity reserve.

---

## 19. Scheduling Cycle

Một scheduling cycle điển hình:

```text
Runtime signal received
        ↓
Collect eligible candidates
        ↓
Remove invalid candidates
        ↓
Evaluate replacement opportunities
        ↓
Compute priority
        ↓
Match capability
        ↓
Check budgets
        ↓
Produce admission decision
        ↓
Emit decision telemetry
```

Scheduling signal có thể gồm:

- WorkItem created;
- queue changed;
- worker available;
- revision changed;
- session state changed;
- provider state changed;
- resource pressure changed;
- configuration activated;
- Runtime paused/resumed;
- shutdown started.

Scheduler nên event-driven.

---

## 20. Queue Replacement

`REPLACE` dùng khi WorkItem mới có cùng logical replacement key nhưng giá trị cao hơn.

Ví dụ replacement key có thể dựa trên:

```text
SessionId
BusinessStageId
WorkType
TargetIdentity
```

Default latest-value behavior:

```text
Keep newest eligible pending WorkItem
Replace older pending WorkItem with same replacement key
```

Không replace running Attempt trực tiếp.

Running work chỉ có thể mất authority hoặc nhận cooperative cancellation từ Runtime Control.

---

## 21. Preemption Boundary

Scheduler không tự cancel running Attempt.

Scheduler có thể phát recommendation hoặc admission pressure signal:

```text
PREEMPTION_RECOMMENDED
SCARCE_CAPACITY_BLOCKED
OBSOLETE_RUNNING_WORK
```

Runtime Control và Cancellation Coordinator quyết định cancellation.

Default MVP behavior:

```text
If obsolete running work blocks a scarce worker
AND current interactive work is waiting
THEN recommend preemption.
```

Nếu cancellation không khả thi, Scheduler vẫn ưu tiên current work khi capacity xuất hiện.

---

## 22. Fairness

Scheduler phải ngăn một session hoặc WorkType chiếm toàn bộ capacity vô hạn.

Fairness có thể xét:

```text
Foreground Session Weight
Visible Secondary Session Weight
Background Session Weight
WorkType Share
Provider Share
Queue Age
```

MVP có thể chỉ hỗ trợ một foreground interactive session, nhưng architecture không được khóa cứng giả định đó.

---

## 23. User Interaction Priority

User-triggered work có thể được Runtime Control nâng priority.

Ví dụ:

- manual retranslation;
- manual correction;
- change presentation mode;
- session stop;
- region change;
- source change.

Scheduler không tự suy luận business intent từ payload.

Nó chỉ sử dụng priority metadata đã được Runtime Control xác nhận.

---

## 24. Deadline Handling

Deadline là scheduling metadata, không phải terminal outcome.

Khi deadline hết, Scheduler có thể:

- `REJECT` nếu chưa admit;
- `DEFER` không còn phù hợp;
- phát deadline signal cho Runtime Control;
- từ chối admission vào provider chậm.

Scheduler không tự đánh dấu WorkItem `FAILED`.

---

## 25. Provider Boundary

Provider Manager sở hữu:

- registration;
- health;
- availability;
- rate-limit state;
- capability;
- runtime lifecycle.

Scheduler chỉ sử dụng provider runtime state để admission.

Scheduler không:

- chọn provider theo business meaning;
- quyết định fallback;
- tạo retry;
- đọc credential;
- normalize provider error.

---

## 26. Cache Boundary

Scheduler không lookup cache trực tiếp.

Luồng đúng:

```text
Runtime Control / Cache Policy
        ↓ determines reusable result
WorkItem may become unnecessary
        ↓
Scheduler receives only remaining eligible work
```

Nếu accepted Artifact đã tồn tại, Runtime Control có thể không tạo WorkItem hoặc rút WorkItem khỏi eligibility.

`CACHE_HIT` không phải Scheduler decision.

---

## 27. Retry Boundary

Retry Policy và Runtime Control quyết định có tạo Attempt mới hay không.

Luồng đúng:

```text
Attempt failed
        ↓
Runtime Control validates relevance
        ↓
Retry Policy evaluates
        ↓
New Attempt created
        ↓
Scheduler evaluates admission
```

Scheduler chỉ admit Attempt mới.

Scheduler không:

- phân loại retryable error;
- đếm retry budget như source of truth;
- tạo AttemptId;
- quyết định provider fallback.

---

## 28. Partial Result Boundary

Partial result support phải được BusinessExecutionPlan và owner module cho phép.

Scheduler chỉ có thể admit các WorkItem liên quan đến partial path.

Scheduler không quyết định:

- partial correctness;
- partial commit;
- presentation semantics;
- logical order.

---

## 29. Backpressure

Khi downstream capacity hết, Scheduler giảm admission thay vì cho queue tăng vô hạn.

Possible actions:

1. reject obsolete pending work;
2. replace older pending work;
3. defer background work;
4. reduce admission cho expensive WorkType;
5. protect control and current-revision work;
6. stop speculative or prefetch work;
7. emit resource-pressure decision.

Backpressure không được block control path.

---

## 30. Memory Pressure

Khi memory pressure tăng:

1. ngừng admit maintenance/background work;
2. reject obsolete pending work;
3. reduce expensive concurrency;
4. preserve current interactive work;
5. yêu cầu Runtime Control xem xét cancellation hoặc cleanup;
6. reject non-critical new work khi vượt hard budget.

Scheduler không tự dispose Artifact.

---

## 31. Provider Pressure

Khi provider degraded hoặc saturated:

```text
Provider state degraded
        ↓
Scheduler reduces admission
        ↓
Background/speculative work deferred
        ↓
Current interactive work prioritized
        ↓
Runtime Control decides fallback or failure
```

Scheduler không flood provider bằng retry.

---

## 32. Scheduler Lifecycle

```text
STOPPED
  ↓
STARTING
  ↓
RUNNING
  ↔
PAUSED
  ↓
STOPPING
  ↓
STOPPED
```

`PAUSED` nghĩa là:

- không admit domain work mới;
- control path còn hoạt động;
- cancellation và cleanup vẫn hoạt động;
- telemetry tối thiểu vẫn hoạt động.

---

## 33. Scheduler State Ownership

Scheduler sở hữu:

- internal policy state;
- admission decision state;
- fairness counters;
- bounded scheduling metadata;
- scheduler lifecycle;
- decision telemetry metadata.

Scheduler không sở hữu:

- WorkItem terminal state;
- Revision authority;
- cancellation authority;
- Artifact lifecycle;
- provider business selection;
- retry lineage source of truth.

---

## 34. Scheduling Events

Các event cuối cùng phải tuân theo Event Standard.

Conceptual events:

```text
SCHEDULER_STARTED
SCHEDULER_PAUSED
SCHEDULER_RESUMED
SCHEDULER_STOPPED

WORK_ADMITTED
WORK_DEFERRED
WORK_REJECTED
WORK_REPLACED

CAPACITY_PRESSURE_DETECTED
PREEMPTION_RECOMMENDED
PROVIDER_CAPACITY_DEGRADED
```

Scheduler không phát:

```text
WORK_SUCCEEDED
WORK_FAILED
WORK_RETRIED
WORK_CANCELED
```

như thể Scheduler sở hữu terminal outcome.

---

## 35. Metrics

Scheduler nên cung cấp:

- admission decision count;
- defer count;
- reject count;
- replace count;
- pending candidate count;
- queue saturation;
- decision latency;
- queue wait time;
- worker utilization input;
- provider saturation input;
- resource-pressure decisions;
- current-revision admission ratio;
- background deferral ratio;
- fairness wait time;
- preemption recommendation count.

Metrics không chứa user content.

---

## 36. Determinism and Testability

Với cùng:

- candidate WorkItem set;
- Runtime Control snapshot;
- revision metadata;
- queue state;
- worker availability;
- provider state;
- resource state;
- configuration snapshot;

Scheduler phải tạo cùng decision.

Randomness chỉ được dùng khi:

- intentional;
- seeded;
- observable;
- testable.

Scheduler test không cần chạy real provider hoặc Business Module.

---

## 37. Failure Isolation

Nếu Scheduler gặp internal failure:

```text
Stop new admission
        ↓
Preserve control path
        ↓
Notify Runtime Control
        ↓
Reject or hold pending work safely
        ↓
Emit fatal scheduler diagnostics
        ↓
Allow controlled runtime shutdown or restart
```

Scheduler failure không được mutate business data hoặc accepted Artifact.

---

## 38. MVP Scheduling Policy

### 38.1 Assumptions

- một foreground interactive session;
- một current revision;
- bounded Work Queue;
- bounded worker pools;
- cooperative cancellation khi implementation hỗ trợ;
- explicit provider capacity;
- current-revision-first policy.

### 38.2 Decision Rules

1. Reject work khi Runtime không nhận admission.
2. Reject work từ inactive session.
3. Reject work không còn eligible.
4. Replace older pending work có cùng replacement key.
5. Bảo vệ control capacity.
6. Ưu tiên current revision.
7. Ưu tiên interactive critical.
8. Tôn trọng worker capability.
9. Tôn trọng queue, concurrency và resource budget.
10. Admit WorkItem có giá trị cao nhất trong số candidate hợp lệ.

### 38.3 Latest-Value Rule

```text
For the same session, target and WorkType:
keep the newest eligible pending WorkItem.
```

---

## 39. Example: Rapid Scrolling

```text
Revision 30 WorkItem running

User scrolls

Revision 31 created
Runtime Control revokes Revision 30 authority

Revision 31 WorkItem becomes pending

User scrolls again

Revision 32 created
Revision 31 pending WorkItem replaced
Scheduler recommends preemption for obsolete running work
Revision 32 receives next scarce slot
```

Runtime Control, không phải Scheduler, reject late completion của Revision 30.

---

## 40. Example: Provider Saturation

```text
Provider capacity exhausted
        ↓
Scheduler defers background provider work
        ↓
Current interactive work remains highest priority
        ↓
Provider Manager reports prolonged degradation
        ↓
Runtime Control / Retry Policy decides fallback
```

---

## 41. Example: Memory Pressure

```text
Memory pressure detected
        ↓
Maintenance admission stopped
        ↓
Background pending work rejected
        ↓
Expensive concurrency reduced
        ↓
Current-revision work protected
        ↓
Runtime Control coordinates cancellation/cleanup if required
```

---

## 42. Example: Replacement

```text
Pending WorkItem A
    Session = S1
    WorkType = PRESENTATION_BUILD
    Target = current viewport

New WorkItem B arrives
    same replacement key
    newer RevisionId

Scheduler decision:
    REPLACE A with B
```

---

## 43. Architecture Invariants

1. Scheduler chỉ sở hữu admission decision.
2. Scheduler không tạo WorkItem.
3. Scheduler không tạo Attempt.
4. Scheduler không quyết định retry.
5. Scheduler không quyết định terminal outcome.
6. Scheduler không commit Artifact hoặc UI.
7. Scheduler không thay đổi BusinessExecutionPlan.
8. Scheduler không đọc business payload.
9. Queue và concurrency luôn bounded.
10. Control path không bị starve.
11. Current revision được ưu tiên.
12. Worker chỉ nhận compatible work.
13. Provider business selection không thuộc Scheduler.
14. Cache lookup không thuộc Scheduler.
15. Stale validation cuối cùng không thuộc Scheduler.
16. Admission decision phải observable.
17. Decision phải có reason code.
18. Resource pressure làm giảm admission, không làm queue tăng vô hạn.
19. Scheduler failure không phá runtime correctness.
20. Telemetry failure không thay đổi decision semantics.

---

## 44. Open Questions

Các câu hỏi để lại cho implementation:

- một Scheduler hay nhiều policy partition trên cùng Scheduler;
- local và remote worker có dùng capacity pool riêng không;
- cost hint được ước lượng tĩnh hay động;
- fairness đa session dùng weighted round-robin hay policy khác;
- adaptive concurrency có cần thiết sau profiling không;
- GPU memory budget được đo và reserve thế nào;
- preemption recommendation có cần remaining-cost estimate không;
- queue replacement key cụ thể theo từng WorkType là gì.

Những câu hỏi này không chặn MVP.

---

## 45. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem, Attempt, authority và terminal outcome |
| `RUNTIME_COMPONENTS.md` | Scheduler component ownership |
| `BUSINESS_PIPELINE_ORCHESTRATION.md` | BusinessExecutionPlan và BusinessStagePlan |
| `WORK_QUEUE.md` | Bounded queued-work lifecycle |
| `CANCELLATION.md` | Cancellation authority và propagation |
| `RETRY_POLICY.md` | Retry decision và new Attempt |
| `CACHE_POLICY.md` | Reuse trước execution |
| `MEMORY_MODEL.md` | Revision, Artifact và Lease |
| `THREADING_MODEL.md` | Worker execution context |
| `RESOURCE_LIFECYCLE.md` | Resource ownership và disposal |
| `PERFORMANCE_MODEL.md` | Useful-result latency và overload |
| `RUNTIME_CONFIG.md` | Scheduling configuration |
| `RUNTIME_OBSERVABILITY.md` | Scheduler metrics và traces |

---

## 46. Completion Criteria

`SCHEDULER.md` được xem là đồng bộ khi:

- chỉ còn bốn decision `ADMIT`, `DEFER`, `REJECT`, `REPLACE`;
- retry và terminal outcome đã được loại khỏi ownership;
- cache lookup không còn thuộc Scheduler;
- provider selection được tách khỏi Scheduler;
- Runtime Control là nguồn authority;
- WorkItem vocabulary khớp `PIPELINE_RUNTIME.md`;
- không hard-code capability nội bộ làm scheduling architecture;
- queue và concurrency bounded;
- control path được bảo vệ;
- decision deterministic, observable và có reason code;
- MVP policy đơn giản và triển khai được.

---

## 47. Summary

Scheduler là admission decision engine của CRAI Runtime.

```text
Runtime Control creates eligible work.

Scheduler decides admission.

Work Queue holds admitted work.

Workers execute attempts.

Runtime Control accepts outcomes.
```

MVP Scheduler nên duy trì:

```text
Current Revision First
+
Bounded Capacity
+
Explicit Admission
+
Replace Obsolete Pending Work
+
Protect Control Path
```
