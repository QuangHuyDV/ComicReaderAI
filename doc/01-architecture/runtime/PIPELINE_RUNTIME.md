# runtime/PIPELINE_RUNTIME.md

# Runtime Execution Model

**Status:** Draft  
**Version:** 2.0

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime tiếp nhận một `BusinessExecutionPlan` và chuyển nó thành execution có kiểm soát bằng:

- `Revision`;
- `WorkItem`;
- `Attempt`;
- `Artifact`;
- authority validation;
- accepted terminal outcome.

Tài liệu này là nguồn định nghĩa chuẩn cho runtime vocabulary.

Nó không mô tả:

- OCR flow;
- Translation flow;
- Presentation flow;
- business stage implementation;
- provider implementation;
- UI framework;
- process topology cụ thể.

---

## 2. Architectural Position

```text
Business Request
        ↓
Business Pipeline Orchestration
        ↓
BusinessExecutionPlan
        ↓
Pipeline Runtime
        ↓
Runtime Control
        ↓
Scheduler / Work Queue / Worker
        ↓
Business Module Contract
        ↓
Artifact / Completion
        ↓
Authority Validation
        ↓
Accepted Outcome
```

Pipeline Runtime trả lời câu hỏi:

> Runtime thực thi một Business Execution Plan như thế nào mà vẫn bảo đảm authority, cancellation, retry, stale protection và resource safety?

---

## 3. Core Separation

CRAI phân biệt rõ:

```text
Business Pipeline Orchestration
    → quyết định business work nào cần thiết

Pipeline Runtime
    → quản lý execution state và authority

Business Module
    → quyết định correctness và semantics của result
```

Runtime không biết chi tiết OCR, layout, segmentation, translation strategy hoặc rendering algorithm.

Runtime chỉ biết:

```text
BusinessStagePlan
        ↓
WorkItem
        ↓
Attempt
        ↓
Completion
        ↓
Accepted Outcome
```

---

## 4. Runtime Vocabulary

Các thuật ngữ sau được định nghĩa chuẩn tại tài liệu này.

### 4.1 Session

Một runtime scope gắn với một Reading Session đang hoạt động.

`SessionId` là identity cấp cao để cô lập:

- current revision;
- cancellation scope;
- priority;
- runtime metadata;
- session-scoped resource ownership.

Session business state vẫn thuộc Reading Module.

### 4.2 Revision

`Revision` đại diện cho một phiên bản nội dung hoặc execution intent ổn định trong một session.

Revision là immutable về identity.

Revision có thể trở thành obsolete nhưng không bị mutate thành revision mới.

### 4.3 WorkItem

`WorkItem` là đơn vị công việc logic mà Runtime cần hoàn thành để thực hiện một phần của `BusinessExecutionPlan`.

WorkItem:

- có identity ổn định;
- có dependency;
- có priority;
- có input ArtifactRef;
- có thể có nhiều Attempt;
- chỉ chấp nhận một terminal outcome cuối cùng.

### 4.4 Attempt

`Attempt` là một lần thực thi vật lý của một WorkItem.

Mỗi retry tạo `AttemptId` mới.

Attempt cũ không được resume.

### 4.5 Artifact

`Artifact` là output immutable được publish vào Runtime Artifact Store.

Artifact có:

- `ArtifactId`;
- artifact type;
- producer WorkItem;
- producer Attempt;
- version metadata;
- ownership metadata;
- lease metadata;
- lifecycle state.

### 4.6 Completion

`Completion` là command hoặc notification do Worker gửi về Runtime Control sau khi Attempt kết thúc.

Completion chưa phải accepted outcome.

### 4.7 Terminal Outcome

Mỗi WorkItem chỉ chấp nhận một terminal outcome:

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

### 4.8 Authority

Authority là quyền logic để một result tiếp tục ảnh hưởng Runtime hoặc UI.

Một result có thể đúng về kỹ thuật nhưng không còn authority.

---

## 5. Runtime Execution Flow

```text
BusinessExecutionPlan accepted
        ↓
Runtime Control validates request
        ↓
Revision created
        ↓
BusinessStagePlan evaluated
        ↓
Logical WorkItem created
        ↓
Scheduler admission
        ↓
Work Queue
        ↓
Worker Execution
        ↓
Business Module Contract invoked
        ↓
Temporary Result produced
        ↓
Completion returned
        ↓
Runtime Control validates authority
        ↓
Artifact accepted and published
        ↓
WorkItem terminal outcome accepted
        ↓
Downstream WorkItem becomes eligible
        ↓
Presentation commit or revision completion
```

---

## 6. Runtime Control Ownership

Runtime Control là single logical writer đối với:

- runtime state;
- active session runtime metadata;
- current revision authority;
- WorkItem logical state;
- Attempt lineage;
- accepted terminal outcome;
- cancellation state;
- retry state;
- commit authority;
- shutdown state.

Worker, Scheduler, Event Bus và Provider Adapter không được mutate trực tiếp các state này.

---

## 7. Revision Model

### 7.1 Revision Identity

Mỗi Revision có tối thiểu:

```text
SessionId
RevisionId
CreatedAt
BusinessPlanId
BusinessPlanVersion
ConfigurationVersion
SourceIdentity
RevisionMetadata
```

### 7.2 Revision Lifecycle

```text
CREATED
  ↓
CURRENT
  ↓
SUPERSEDED
  ↓
DRAINING
  ↓
DISPOSED
```

Revision cũng có thể kết thúc bằng failure hoặc cancellation ở mức execution, nhưng authority vẫn được quản lý riêng.

### 7.3 Current Revision

Mỗi session chỉ có một current revision tại một thời điểm.

Current revision:

- có commit authority;
- có scheduling priority cao nhất;
- có quyền tạo downstream work;
- có thể supersede revision trước.

### 7.4 Superseded Revision

Revision cũ bị supersede khi:

- nội dung mới ổn định;
- source identity thay đổi;
- user intent thay đổi;
- BusinessExecutionPlan mới thay thế plan cũ;
- session configuration thay đổi theo cách làm invalid output cũ.

Superseded revision có thể vẫn còn running Attempt trong thời gian drain nhưng không còn commit authority.

---

## 8. WorkItem Model

### 8.1 WorkItem Identity

```text
SessionId
RevisionId
WorkItemId
BusinessStageId
WorkType
Priority
InputArtifactRefs
ConfigurationVersion
CancellationScope
```

### 8.2 WorkItem Lifecycle

```text
CREATED
  ↓
PENDING
  ↓
ADMITTED
  ↓
QUEUED
  ↓
RUNNING
  ↓
COMPLETION_REPORTED
  ↓
TERMINAL_ACCEPTED
```

Các terminal outcome:

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

### 8.3 WorkItem Rules

1. WorkItem là logical identity ổn định.
2. WorkItem không bị clone khi retry.
3. Retry tạo Attempt mới.
4. WorkItem chỉ có một accepted terminal outcome.
5. WorkItem không chứa large payload.
6. WorkItem không chứa secret.
7. WorkItem không tự schedule downstream work.
8. WorkItem không tự commit result.

---

## 9. Attempt Model

### 9.1 Attempt Identity

```text
SessionId
RevisionId
WorkItemId
AttemptId
AttemptNumber
ProviderSelectionRef
StartedAt
ExecutionContextRef
```

### 9.2 Attempt Lifecycle

```text
CREATED
  ↓
STARTED
  ↓
RUNNING
  ↓
COMPLETED | FAILED | CANCELED | ABANDONED
```

Attempt lifecycle khác WorkItem lifecycle.

### 9.3 Attempt Rules

- mỗi retry tạo Attempt mới;
- Attempt không được resume;
- Attempt cũ không overwrite accepted outcome;
- late completion phải được authority validation;
- provider fallback là một Attempt mới;
- speculative execution chưa thuộc MVP;
- một WorkItem có thể có nhiều Attempt nhưng chỉ một accepted outcome.

---

## 10. Scheduler Interaction

Pipeline Runtime tạo WorkItem và yêu cầu Scheduler admission.

Scheduler chỉ quyết định:

```text
ADMIT
DEFER
REJECT
REPLACE
```

Scheduler không:

- thay đổi BusinessExecutionPlan;
- tự tạo retry;
- tự tạo downstream WorkItem;
- xác nhận terminal outcome;
- commit Artifact;
- mutate revision authority.

---

## 11. Queue Interaction

Sau khi được admission, WorkItem có thể vào bounded Work Queue.

Queue chỉ lưu:

```text
WorkItemRef
AttemptRef
Priority
Dependency state
Cancellation reference
Artifact references
```

Queue không lưu:

- image buffer;
- text payload lớn;
- provider response;
- secret;
- mutable business object.

---

## 12. Worker Execution

Worker thực thi một Attempt.

Worker có trách nhiệm:

- nhận immutable execution input;
- acquire Artifact Lease;
- gọi public contract phù hợp;
- theo dõi cooperative cancellation;
- tạo temporary output;
- chuẩn hóa execution result;
- gửi Completion về Runtime Control;
- release lease;
- cleanup temporary resource.

Worker không được:

- tự retry;
- tạo downstream work;
- mutate Runtime Control state;
- commit UI;
- publish accepted Artifact trước authority validation;
- coi technical completion là accepted success.

---

## 13. Business Module Invocation

Runtime gọi Business Module thông qua public contract.

Ví dụ:

```text
Worker
    ↓
Recognition Contract
    ↓
Recognition Result
```

Runtime không biết Recognition Module thực hiện:

- OCR;
- layout analysis;
- reading order;
- bubble detection;
- model inference;
- provider selection nội bộ.

Tương tự, Runtime không biết Translation Module xử lý glossary, cache hay provider như thế nào.

---

## 14. Completion Model

Worker gửi Completion khi Attempt kết thúc.

Ví dụ:

```text
AttemptCompleted
AttemptFailed
AttemptCanceled
AttemptAbandoned
```

Completion phải chứa tối thiểu:

```text
SessionId
RevisionId
WorkItemId
AttemptId
ExecutionOutcome
TemporaryArtifactRef
ErrorRef
TimingMetadata
```

Completion không tự thay đổi WorkItem state.

---

## 15. Authority Validation

Runtime Control xác nhận Completion bằng cách kiểm tra:

- session còn active;
- revision còn authority;
- WorkItem chưa terminal;
- Attempt còn hợp lệ;
- cancellation state;
- configuration version;
- result identity;
- artifact integrity;
- duplicate completion;
- stale condition.

Kết quả validation:

```text
ACCEPT
REJECT_STALE
REJECT_CANCELED
REJECT_DUPLICATE
REJECT_INVALID_STATE
REJECT_INTEGRITY
```

---

## 16. Accepted Completion

Khi Completion được chấp nhận:

```text
Completion
    ↓
Authority Validation
    ↓
Artifact Publication
    ↓
WorkItem Terminal Outcome
    ↓
Downstream Eligibility
```

Chỉ Runtime Control mới được chấp nhận terminal outcome.

---

## 17. Artifact Publication

Worker có thể tạo temporary output.

Temporary output chỉ trở thành accepted Artifact sau validation.

```text
Temporary Output
        ↓
Register candidate
        ↓
Authority Validation
        ↓
Atomic publication
        ↓
Accepted ArtifactRef
```

Artifact đã publish là immutable.

---

## 18. Artifact Rejection

Artifact candidate bị loại khi:

- Attempt stale;
- cancellation đã revoke authority;
- WorkItem đã terminal;
- artifact corrupt;
- duplicate completion;
- configuration mismatch;
- source identity mismatch.

Rejected output phải được cleanup theo Resource Lifecycle.

---

## 19. Downstream Work Creation

Business Stage không tự gọi stage kế tiếp.

Luồng đúng:

```text
WorkItem succeeded
        ↓
Runtime Control updates dependency state
        ↓
Downstream BusinessStagePlan becomes ready
        ↓
Runtime Control creates downstream WorkItem
        ↓
Scheduler admission
```

---

## 20. Retry Boundary

Pipeline Runtime chỉ định nghĩa execution model cho retry.

```text
Attempt 1 failed
        ↓
Runtime Control validates relevance
        ↓
Retry Policy evaluates
        ↓
Attempt 2 created
        ↓
Scheduler admission
```

Retry policy chi tiết thuộc `RETRY_POLICY.md`.

---

## 21. Cancellation Boundary

Cancellation bắt đầu bằng authority revocation.

```text
Cancellation Requested
        ↓
Authority Revoked
        ↓
Queued Work Removed
        ↓
Running Attempt Signaled
        ↓
Late Completion Rejected
        ↓
Resource Drain
```

Chi tiết propagation thuộc `CANCELLATION.md`.

---

## 22. Stale Result

Một result là stale khi:

- revision không còn current;
- newer plan đã thay thế;
- Attempt không còn hợp lệ;
- source identity thay đổi;
- presentation target version thay đổi;
- session đã dừng.

Stale không đồng nghĩa technical failure.

---

## 23. Replacement Flow

```text
Revision A current
        ↓
Revision B created
        ↓
Revision A authority revoked
        ↓
Revision B becomes current
        ↓
Revision A late completion arrives
        ↓
Rejected as STALE
```

---

## 24. Partial Result

Partial result chỉ được chấp nhận khi BusinessExecutionPlan và owner contract cho phép.

Mỗi partial result phải có:

- identity;
- order;
- parent WorkItem;
- revision identity;
- authority metadata;
- completion semantics.

Partial result không được bypass authority validation.

---

## 25. Presentation Commit

Presentation commit là side effect có thể quan sát được.

Chỉ được phép khi:

- revision còn current;
- result còn authority;
- presentation target còn hợp lệ;
- UI context còn active;
- commit chưa bị thay thế;
- artifact integrity hợp lệ.

Worker không commit UI trực tiếp.

---

## 26. Failure Model

Execution failure phải được normalize thành RuntimeError.

Technical failure không đồng nghĩa:

```text
CANCELED
STALE
ABANDONED
```

Runtime Error Model chi tiết thuộc `ERROR_MODEL.md`.

---

## 27. Concurrent Revisions

Nhiều Revision có thể đồng thời tồn tại ở các trạng thái khác nhau:

```text
Revision 20 → DRAINING
Revision 21 → RUNNING
Revision 22 → CURRENT / PENDING
```

Chỉ current revision có commit authority.

---

## 28. Concurrent WorkItems

Các WorkItem độc lập có thể chạy song song nếu:

- dependency đã thỏa;
- Scheduler admission cho phép;
- resource budget đủ;
- provider concurrency cho phép;
- business ordering không bị phá;
- cancellation scope còn active.

---

## 29. Backpressure

Pipeline Runtime phải hỗ trợ backpressure khi:

- queue đầy;
- provider saturated;
- memory pressure;
- artifact count vượt budget;
- worker pool saturated;
- downstream chậm.

Backpressure được thực hiện qua Scheduler, Queue và Runtime Control, không qua Business Module tự ý chặn lẫn nhau.

---

## 30. Runtime Cleanup

Cleanup chỉ xảy ra khi resource đủ điều kiện.

```text
Revision loses authority
        ↓
Logical ownership removed
        ↓
Running Attempt drains
        ↓
Artifact Lease released
        ↓
Artifact no longer retained
        ↓
Physical disposal
```

Mất authority không đồng nghĩa dispose ngay.

---

## 31. Revision Disposal

Revision có thể dispose khi:

- không còn current;
- không còn pending WorkItem;
- không còn running Attempt;
- không còn accepted Artifact ownership cần giữ;
- không còn active lease;
- diagnostic retention đã hết;
- cleanup hoàn tất hoặc được chuyển sang retry cleanup.

---

## 32. Shutdown

```text
Stop new admission
        ↓
Revoke execution authority
        ↓
Remove queued work
        ↓
Signal running attempts
        ↓
Drain resources
        ↓
Release Artifact leases
        ↓
Dispose revision state
        ↓
Stop workers and providers
```

Shutdown chi tiết phải thống nhất với `BOOT_SEQUENCE.md` và `RESOURCE_LIFECYCLE.md`.

---

## 33. Observability

Pipeline Runtime phải phát telemetry cho:

- revision lifecycle;
- WorkItem lifecycle;
- Attempt lifecycle;
- queue wait;
- execution duration;
- authority rejection;
- stale completion;
- retry lineage;
- cancellation;
- artifact publication;
- cleanup;
- presentation commit;
- current-revision latency.

Không log user content mặc định.

---

## 34. Performance Model

Pipeline Runtime đo performance theo useful output của current revision.

Metric trọng tâm:

```text
Useful Translation Latency
Current Revision Commit Ratio
Stale Work Ratio
Useful Work Ratio
Wasted Execution Time
```

Chi tiết thuộc `PERFORMANCE_MODEL.md`.

---

## 35. Runtime State Summary

```text
Session
  └── Revision
        ├── WorkItem
        │     ├── Attempt 1
        │     ├── Attempt 2
        │     └── Accepted Outcome
        └── ArtifactRefs
```

---

## 36. State Ownership Summary

| State | Owner |
|---|---|
| Current revision | Runtime Control |
| Revision metadata | Revision Store |
| WorkItem logical state | Runtime Control |
| Attempt execution state | Worker execution + Runtime Control acceptance |
| Scheduling decision | Scheduler |
| Queue position | Work Queue |
| Artifact registry | Artifact Store |
| Physical resource lifecycle | Resource Manager |
| Business result correctness | Business Module |
| Durable persistence | Storage Module |
| UI local state | Presentation |

---

## 37. Dependency Rules

1. Runtime Control không phụ thuộc provider implementation.
2. Scheduler không thay đổi BusinessExecutionPlan.
3. Worker không mutate Runtime Control.
4. Worker không tự retry.
5. Business Module không tự schedule downstream work.
6. Event Bus không điều phối pipeline.
7. Artifact Store không sở hữu business data semantics.
8. Storage Module không quản lý runtime authority.
9. UI không gọi Worker trực tiếp.
10. Secret không đi qua WorkItem hoặc Completion.
11. Large payload chỉ truyền bằng ArtifactRef.
12. Process boundary không được thay đổi runtime semantics.
13. Completion phải được validation trước side effect.
14. Late Attempt không overwrite accepted outcome.
15. Cleanup không được phá active lease.

---

## 38. Runtime Invariants

1. Runtime Control là single logical writer.
2. Mỗi session chỉ có một current revision.
3. Current revision có priority cao nhất.
4. Revision identity là immutable.
5. WorkItem identity là immutable.
6. Attempt identity là immutable.
7. Mỗi WorkItem chỉ chấp nhận một terminal outcome.
8. Mỗi retry tạo Attempt mới.
9. Worker không commit.
10. Scheduler không đổi business semantics.
11. Business Module sở hữu result correctness.
12. Artifact đã publish là immutable.
13. Stale result không được commit.
14. Cancellation không mặc định là failure.
15. Failed, canceled hoặc stale output không được promote như success.
16. Artifact chỉ dispose khi không còn owner hoặc lease.
17. Large payload không đi qua queue.
18. Runtime correctness không phụ thuộc telemetry.
19. Shutdown dừng admission trước cleanup.
20. Storage và Artifact Store là hai boundary khác nhau.

---

## 39. Related Documents

| Document | Relationship |
|---|---|
| `BUSINESS_PIPELINE_ORCHESTRATION.md` | Tạo BusinessExecutionPlan |
| `RUNTIME_COMPONENTS.md` | Component ownership |
| `SCHEDULER.md` | Admission model |
| `WORK_QUEUE.md` | Queue lifecycle |
| `CANCELLATION.md` | Cancellation propagation |
| `RETRY_POLICY.md` | Retry eligibility |
| `ERROR_MODEL.md` | Runtime error và terminal outcome |
| `CACHE_POLICY.md` | Artifact reuse |
| `MEMORY_MODEL.md` | Revision Store, Artifact Store và Lease |
| `THREADING_MODEL.md` | Execution context |
| `RESOURCE_LIFECYCLE.md` | Ownership transfer và disposal |
| `PERFORMANCE_MODEL.md` | Useful-result performance |
| `RUNTIME_OBSERVABILITY.md` | Telemetry |
| `BOOT_SEQUENCE.md` | Startup và shutdown |
| `RUNTIME_CONFIG.md` | Configuration snapshot |

---

## 40. Completion Criteria

`PIPELINE_RUNTIME.md` được xem là hoàn chỉnh khi:

- runtime vocabulary chỉ được định nghĩa tại đây;
- BusinessExecutionPlan là input chính;
- Revision, WorkItem và Attempt được tách rõ;
- Runtime Control là authority owner;
- Scheduler chỉ sở hữu admission;
- Worker chỉ sở hữu execution;
- Completion khác accepted outcome;
- Artifact publication cần authority validation;
- retry tạo Attempt mới;
- stale được tách khỏi failure;
- cancellation bắt đầu bằng authority revocation;
- cleanup tách khỏi logical authority;
- Business Module không bị Runtime chiếm ownership;
- Storage và Artifact Store không bị nhầm;
- không còn business capability như OCR hoặc Translation được định nghĩa như runtime stage.

---

## 41. Summary

Pipeline Runtime biến một BusinessExecutionPlan thành execution có kiểm soát:

```text
BusinessExecutionPlan
        ↓
Revision
        ↓
WorkItem
        ↓
Attempt
        ↓
Completion
        ↓
Authority Validation
        ↓
Accepted Artifact
        ↓
Accepted Terminal Outcome
```

Ranh giới cốt lõi:

```text
Business Orchestrator decides what work is required.

Runtime Control owns authority.

Scheduler owns admission.

Workers own execution.

Business Modules own meaning.

Artifact Store owns runtime artifacts.

Storage owns durable persistence.
```
