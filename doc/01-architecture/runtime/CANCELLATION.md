# runtime/CANCELLATION.md

# Runtime Cancellation

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime thu hồi authority, dừng queued work, yêu cầu running execution dừng an toàn, xử lý provider không thể hủy, và ngăn late result ảnh hưởng current revision.

Cancellation là một runtime control capability.

Mục tiêu của cancellation không chỉ là “dừng công việc”, mà trước hết là:

```text
Revoke authority immediately.
Stop execution when safe.
Reject late results unconditionally.
Drain resources correctly.
```

---

## 2. Architectural Position

```text
Cancellation Request
        ↓
Runtime Control
        ↓
Authority Revoked
        ↓
Queued Work Removal
        ↓
Running Attempt Signaled
        ↓
Provider Abort / Cooperative Stop / Abandon
        ↓
Resource Drain
        ↓
Cancellation Outcome Accepted
```

Runtime Control là source of truth cho cancellation authority.

Scheduler, Work Queue, Worker và Provider Adapter chỉ thực hiện phần trách nhiệm tương ứng.

---

## 3. Core Principles

1. Cancellation bắt đầu bằng authority revocation.
2. Logical cancellation xảy ra trước physical stop.
3. Physical execution có thể dừng chậm hoặc không dừng được.
4. Late Completion luôn phải qua authority validation.
5. Cancellation không mặc định là failure.
6. Cancellation không tự tạo retry.
7. Hard thread termination bị cấm trong primary process.
8. Cleanup ownership phải explicit.
9. Cancellation wait phải bounded.
10. Provider capacity thực tế phải được phản ánh trung thực.
11. Canceled work không được tạo downstream work.
12. Cancellation events không chứa user content.

---

## 4. Cancellation Goals

Cancellation phải:

- loại obsolete work sớm;
- bảo vệ current revision;
- giải phóng queue capacity;
- giảm wasted CPU, GPU, memory và network;
- không làm block UI;
- không làm provider request sống vô hạn không theo dõi;
- không revive work đã mất authority;
- không làm sai terminal outcome;
- hỗ trợ deterministic testing;
- hỗ trợ shutdown an toàn.

---

## 5. Canonical Cancellation Scopes

Runtime v2 sử dụng các scope chuẩn:

```text
APPLICATION
SESSION
REVISION
WORK_ITEM
ATTEMPT
PROVIDER_REQUEST
```

### 5.1 Application Scope

Thu hồi authority toàn Runtime.

Ví dụ:

- application shutdown;
- fatal invariant failure;
- controlled restart.

### 5.2 Session Scope

Thu hồi authority của một Reading Session.

Ví dụ:

- người dùng đóng session;
- session trở thành invalid;
- session stop command.

### 5.3 Revision Scope

Thu hồi authority của một Revision.

Ví dụ:

- newer revision xuất hiện;
- source identity thay đổi;
- BusinessExecutionPlan mới thay thế plan cũ.

### 5.4 WorkItem Scope

Thu hồi authority của một logical WorkItem.

Ví dụ:

- optional work không còn cần thiết;
- business dependency invalidated.

### 5.5 Attempt Scope

Thu hồi authority của một physical execution attempt.

Ví dụ:

- provider switch;
- timeout;
- attempt-specific preemption.

### 5.6 Provider Request Scope

Yêu cầu dừng một local hoặc remote provider operation.

---

## 6. Scope Hierarchy

```text
APPLICATION
    ↓
SESSION
    ↓
REVISION
    ↓
WORK_ITEM
    ↓
ATTEMPT
    ↓
PROVIDER_REQUEST
```

Parent cancellation ảnh hưởng toàn bộ child scope.

Child cancellation không tự động cancel parent.

Ví dụ:

```text
Cancel ATTEMPT
    ≠
Cancel REVISION
```

Escalation phải là policy decision rõ ràng của Runtime Control.

---

## 7. Removed Legacy Scopes

Các scope sau không còn là cancellation scope chuẩn:

```text
SCHEDULER
PIPELINE
STAGE
```

Lý do:

- Scheduler có lifecycle riêng, không phải execution authority scope.
- Pipeline đã được biểu diễn bằng Revision và BusinessExecutionPlan.
- Runtime không còn sử dụng stage như execution identity chuẩn.

Nếu một Business Stage bị thay thế, Runtime Control revoke WorkItem hoặc Attempt liên quan.

---

## 8. Cancellation Context

Architecture sử dụng khái niệm tổng quát:

```text
CancellationContext
├── Scope
├── ScopeId
├── ParentContextRef
├── IsCancellationRequested
├── ReasonCode
├── RequestedAt
├── RequestedBy
├── GraceDeadline
└── Metadata
```

`CancellationToken` có thể là implementation cụ thể, nhưng không phải architectural requirement.

---

## 9. Cancellation Reference

WorkItem và Attempt chỉ mang lightweight reference:

```text
CancellationScope
CancellationContextRef
```

Không được nhúng mutable cancellation implementation hoặc provider handle trực tiếp vào public WorkItem contract.

---

## 10. Cancellation Reasons

Reason code phải stable.

Ví dụ:

```text
APPLICATION_SHUTDOWN
SESSION_CLOSED
USER_STOPPED
NEWER_REVISION_AVAILABLE
SOURCE_CHANGED
WORK_SUPERSEDED
ATTEMPT_REPLACED
PROVIDER_SWITCHED
PROVIDER_TIMEOUT
DEADLINE_EXCEEDED
RESOURCE_PRESSURE
DEPENDENCY_INVALIDATED
RUNTIME_STOPPING
MANUAL_CANCEL
```

Reason code phục vụ:

- metrics;
- tests;
- diagnostics;
- UI mapping;
- policy analysis.

---

## 11. Cancellation Progression

Cancellation progression không định nghĩa lại WorkItem lifecycle.

```text
ACTIVE
  ↓
AUTHORITY_REVOKED
  ↓
SIGNALING
  ↓
DRAINING
  ↓
ACKNOWLEDGED
```

Hoặc:

```text
DRAINING
  ↓
ABANDONED
```

Canonical WorkItem terminal outcome vẫn được định nghĩa tại `PIPELINE_RUNTIME.md`.

---

## 12. Authority Revocation

Đây là bước bắt buộc đầu tiên.

Khi cancellation được chấp nhận, Runtime Control phải:

- revoke commit authority;
- revoke downstream scheduling authority;
- đánh dấu WorkItem/Attempt không còn eligible;
- ngăn accepted Artifact publication;
- ngăn UI commit;
- ngăn canceled scope được revive.

Authority revocation phải xảy ra ngay cả khi physical cancellation chưa bắt đầu.

---

## 13. Three-Layer Protection

### Layer 1 — Prevent Execution

Nếu work chưa chạy:

```text
Authority revoked
    ↓
Queued item removed or invalidated
```

### Layer 2 — Stop Execution Cooperatively

Nếu Attempt đang chạy:

```text
Cancellation signaled
    ↓
Worker reaches checkpoint
    ↓
Execution stops safely
```

### Layer 3 — Reject Late Result

Nếu không thể dừng:

```text
Attempt completes late
    ↓
Authority validation fails
    ↓
Result rejected
```

Layer 3 luôn bắt buộc.

---

## 14. Queued Work Removal

Luồng chuẩn:

```text
Runtime Control revokes authority
        ↓
Work Queue receives removal instruction
        ↓
Queued item removed or logically invalidated
```

Queue không tự quyết định terminal outcome.

Queue có thể sử dụng:

- physical removal;
- logical invalidation.

MVP có thể dùng logical invalidation nếu queue nhỏ và bounded.

---

## 15. Dequeue Validation

Ngay trước dispatch phải xác nhận:

- session active;
- revision eligible;
- WorkItem chưa bị revoke;
- Attempt còn hợp lệ;
- dependency còn ready;
- deadline còn hợp lệ;
- runtime chưa stopping.

Nếu invalid:

```text
Do not dispatch.
Notify Runtime Control.
```

Queue hoặc Scheduler không tự đánh dấu WorkItem `CANCELED`.

---

## 16. Running Attempt Cancellation

```text
Authority revoked
    ↓
Cancellation signal delivered
    ↓
Worker observes context
    ↓
Worker stops at safe checkpoint
    ↓
Temporary resources released
    ↓
Completion reported
```

Worker phải phản hồi cancellation trong bounded time khi implementation cho phép.

---

## 17. Generic Cancellation Checkpoints

Worker nên kiểm tra cancellation:

- trước expensive execution;
- trước provider invocation;
- sau provider invocation;
- giữa bounded batches;
- trước large allocation;
- trước candidate Artifact creation;
- trước Completion;
- trước observable side effect;
- trước UI dispatch;
- ngay trước UI commit.

Checkpoint chi tiết thuộc Business Module hoặc Provider Adapter tương ứng.

---

## 18. Business Module Boundary

Runtime không hard-code checkpoint theo OCR, Layout, Translation hoặc Presentation.

Business Module phải khai báo hoặc triển khai checkpoint phù hợp với:

- cost;
- batch boundary;
- provider behavior;
- resource ownership;
- output atomicity.

Runtime chỉ yêu cầu cooperative cancellation contract.

---

## 19. Provider Cancellation Categories

### 19.1 Fully Cancelable

Provider hỗ trợ abort ngay.

```text
Signal cancellation
    ↓
Abort provider request
    ↓
Provider acknowledges
    ↓
Capacity released
```

### 19.2 Cooperatively Cancelable

Provider chỉ dừng tại checkpoint.

```text
Set cancellation state
    ↓
Provider reaches checkpoint
    ↓
Stops safely
```

### 19.3 Non-Cancelable

Provider không thể dừng an toàn.

```text
Authority revoked
    ↓
Request marked abandoned
    ↓
Runtime stops waiting
    ↓
Physical request may continue
    ↓
Late result rejected
```

---

## 20. Abandoned Execution

`ABANDONED` nghĩa là:

> Runtime không còn chờ execution, nhưng chưa xác nhận physical work đã kết thúc.

Abandoned execution:

- không có commit authority;
- không được schedule downstream work;
- vẫn phải được theo dõi;
- vẫn có thể giữ provider capacity;
- vẫn cần cleanup khi late completion đến;
- không được giả định đã giải phóng billing hoặc resource.

---

## 21. Provider Capacity Truthfulness

Nếu provider request vẫn chạy:

- provider slot vẫn được tính occupied khi thực tế còn occupied;
- concurrency không được tăng giả tạo;
- billing risk vẫn được ghi nhận;
- late response vẫn phải được consume hoặc discard an toàn.

Runtime không được coi logical detach là physical release.

---

## 22. Cancellation Grace Period

Cancellation wait phải bounded.

```text
Cancellation requested
        ↓
Wait for cooperative stop
        ↓
Grace deadline exceeded
        ↓
Mark Attempt ABANDONED
        ↓
Runtime stops waiting
        ↓
Continue asynchronous tracking
```

Grace period cụ thể thuộc `RUNTIME_CONFIG.md`.

---

## 23. Hard Termination Policy

Hard termination bị cấm trong primary process.

Không được:

- kill arbitrary thread;
- dispose shared memory đang được dùng;
- interrupt unmanaged operation không có safety guarantee;
- terminate worker mà không có ownership cleanup.

Hard termination chỉ có thể xem xét trong isolated child process với process-restart policy rõ ràng.

---

## 24. Completion After Cancellation

Worker vẫn phải gửi Completion khi có thể.

Completion có thể là:

```text
AttemptCanceled
AttemptAbandoned
AttemptCompletedLate
AttemptFailedDuringCancellation
```

Runtime Control quyết định accepted terminal outcome.

---

## 25. Authority Validation

Mọi Completion sau cancellation phải kiểm tra:

- SessionId;
- RevisionId;
- WorkItemId;
- AttemptId;
- current authority;
- cancellation state;
- duplicate state;
- accepted outcome state;
- Artifact integrity.

Possible result:

```text
REJECT_CANCELED
REJECT_STALE
REJECT_DUPLICATE
REJECT_INVALID_STATE
```

---

## 26. Cancellation vs Stale

Cancellation và stale khác nhau.

### Canceled

Authority bị revoke bởi explicit control decision.

### Stale

Result không còn phù hợp với current runtime state.

Một Attempt có thể bị cancellation request và completion của nó cuối cùng bị phân loại `STALE` hoặc `CANCELED` tùy Runtime Control acceptance rules.

Canonical terminal semantics thuộc `PIPELINE_RUNTIME.md` và `ERROR_MODEL.md`.

---

## 27. Cancellation vs Failure

Cancellation không phải error mặc định.

```text
Technical Failure
    ≠
Cancellation
    ≠
Stale Result
    ≠
Abandoned Execution
```

Cleanup failure có thể tạo diagnostics error nhưng không phục hồi authority.

---

## 28. Cancellation and Retry

Cancellation không tự tạo retry.

Luồng chuẩn:

```text
Attempt canceled or failed
        ↓
Runtime Control evaluates relevance
        ↓
Retry Policy evaluates
        ↓
New AttemptId created
        ↓
Scheduler admission
```

Quy tắc:

- same WorkItemId;
- new AttemptId;
- cancellation vì newer revision thì không retry;
- Attempt cũ không được resume;
- Provider switch có thể tạo Attempt mới;
- retry budget không thuộc Cancellation component.

---

## 29. Queued Retry Work

Nếu delayed retry đang pending và scope bị canceled:

```text
Authority revoked
    ↓
Delayed retry canceled
    ↓
No new Attempt admitted
```

Cancellation phải ngăn delayed retry hồi sinh WorkItem đã mất authority.

---

## 30. Resource Drain

Sau authority revocation:

```text
Stop new work
    ↓
Cancel child operations
    ↓
Release temporary resources
    ↓
Release or detach provider resources
    ↓
Release Artifact Leases
    ↓
Report completion
```

Logical cancellation có thể hoàn tất trước physical drain.

---

## 31. Cleanup Ownership

| Resource | Default owner |
|---|---|
| Attempt temporary resource | Worker Execution |
| Provider request | Provider Adapter |
| Candidate Artifact | Producer cho đến ownership transfer |
| Accepted Artifact | Artifact Store |
| Artifact Lease | Lease holder |
| Queued position | Work Queue |
| Revision metadata | Revision Store |
| Physical backing resource | Resource Manager |
| UI dispatch handle | Presentation boundary |

Ownership transfer phải explicit.

---

## 32. Cleanup Rules

Cleanup phải:

- idempotent khi thực tế cho phép;
- không revive canceled work;
- không phục hồi commit authority;
- không dispose resource còn lease;
- observable khi thất bại;
- bounded theo retry/cleanup policy;
- không block UI lâu dài.

---

## 33. Child Operation Registration

Child operation phải register trước khi execution bắt đầu.

```text
Create child context
    ↓
Link to parent scope
    ↓
Register ownership
    ↓
Start child operation
```

Điều này ngăn child operation thoát khỏi parent cancellation.

---

## 34. Downward Propagation

Cancellation propagates xuống child scope.

```text
SESSION
    ↓
REVISION
    ↓
WORK_ITEM
    ↓
ATTEMPT
    ↓
PROVIDER_REQUEST
```

Propagation lên parent không tự động.

---

## 35. Cancellation Race Conditions

### Completion and Cancellation Simultaneously

Resolution:

```text
Authority validation decides.
```

### Revision Changes During Dispatch

Resolution:

```text
Validate immediately before Worker assignment.
```

### UI Dispatch Already Queued

Resolution:

```text
Validate again on UI context before commit.
```

### Provider Response After Abandonment

Resolution:

```text
Reject result and release late resources.
```

### Retry Timer Fires During Cancellation

Resolution:

```text
Check authority before creating or admitting new Attempt.
```

---

## 36. UI Boundary

UI phản ánh logical cancellation ngay.

Khi cancellation được chấp nhận:

- loading state của scope cũ dừng;
- current valid content có thể vẫn hiển thị;
- stale error không thay thế newer content;
- UI không chờ provider cleanup;
- UI commit cũ bị authority validation chặn.

---

## 37. User-Initiated Cancellation

Luồng:

```text
User intent changes
        ↓
Application updates business intent
        ↓
Runtime Control revokes authority
        ↓
UI updates promptly
        ↓
Physical cancellation and cleanup continue
```

Ví dụ:

- stop;
- close session;
- change region;
- switch mode;
- retranslate;
- switch provider.

---

## 38. Automatic Cancellation

Runtime có thể tự cancel khi:

- newer revision xuất hiện;
- session inactive;
- deadline hết;
- critical resource pressure;
- provider unhealthy;
- dependency invalidated;
- BusinessExecutionPlan replaced;
- runtime stopping.

Automatic cancellation luôn cần reason code.

---

## 39. Cancellation Events

Conceptual events:

```text
CANCELLATION_REQUESTED
AUTHORITY_REVOKED
CANCELLATION_PROPAGATED
CANCELLATION_ACKNOWLEDGED
CANCELLATION_TIMED_OUT
ATTEMPT_ABANDONED
QUEUED_WORK_REMOVED
LATE_RESULT_REJECTED
```

Tên cuối phải tuân theo Event Standard.

---

## 40. Event Payload

Cancellation event có thể chứa:

```text
EventId
OccurredAt
Scope
ScopeId
SessionId
RevisionId
WorkItemId
AttemptId
ReasonCode
RequestedBy
RequestedAt
AcknowledgedAt
GraceDeadline
Outcome
```

Không chứa:

- screenshot;
- source text;
- OCR text;
- translated text;
- prompt;
- secret;
- provider request body.

---

## 41. Metrics

Theo dõi:

- cancellation request count theo scope;
- authority revocation latency;
- queued removal latency;
- worker acknowledgment latency;
- cancellation completion latency;
- abandoned Attempt count;
- provider abort success ratio;
- late Completion count;
- late result rejection count;
- cleanup failure count;
- reason-code distribution;
- drain duration;
- provider capacity held after logical cancellation.

---

## 42. Logging

Log cancellation phải chứa identity và state metadata, không chứa user content.

Nên ghi:

- scope;
- reason;
- SessionId;
- RevisionId;
- WorkItemId;
- AttemptId;
- authority revoked time;
- acknowledgment time;
- drain state;
- terminal outcome.

---

## 43. Cancellation Failure

Cancellation operation có thể thất bại vật lý.

Ví dụ:

- provider abort throws;
- worker không acknowledge;
- temporary resource release lỗi;
- child operation không dừng;
- process không phản hồi.

Khi đó:

- authority vẫn bị revoke;
- work không được revive;
- diagnostics được phát;
- Resource Manager tiếp tục cleanup;
- Runtime có thể mark Attempt abandoned;
- current revision correctness vẫn được bảo vệ.

---

## 44. Shutdown Integration

Application shutdown sử dụng Application scope.

```text
Stop new admission
        ↓
Revoke application authority
        ↓
Remove queued work
        ↓
Signal running attempts
        ↓
Wait bounded grace period
        ↓
Mark remaining work abandoned
        ↓
Drain and dispose resources
```

Shutdown chi tiết phải thống nhất với `BOOT_SEQUENCE.md`.

---

## 45. MVP Cancellation Policy

### Required Scopes

```text
APPLICATION
SESSION
REVISION
WORK_ITEM
ATTEMPT
PROVIDER_REQUEST
```

### Required Rules

1. Mọi WorkItem có CancellationContextRef.
2. Session close revoke toàn bộ child authority.
3. New revision revoke revision cũ.
4. Queued obsolete work bị remove hoặc invalidate.
5. Running Attempt nhận cooperative signal.
6. Worker check trước và sau expensive boundary.
7. Provider abort được dùng khi hỗ trợ.
8. Non-cancelable request trở thành abandoned.
9. Late Completion luôn validation.
10. UI commit validation lại trên UI context.
11. Delayed retry bị cancel cùng scope.
12. Hard termination không dùng trong primary process.

---

## 46. MVP Sequence

```text
New Revision Created
        ↓
Previous Revision Authority Revoked
        ↓
Queued Work Removed
        ↓
Running Attempts Signaled
        ↓
New Revision Work Admitted
        ↓
Late Old Results Rejected
        ↓
Old Resources Drained
```

---

## 47. Example: Rapid Content Change

```text
Revision 70 Attempt running
        ↓
Revision 71 becomes current
        ↓
Revision 70 authority revoked
        ↓
Queued Revision 70 work removed
        ↓
Running Attempt signaled
        ↓
Revision 71 receives priority
```

Nếu Attempt cũ kết thúc muộn:

```text
Completion arrives
    ↓
Authority validation fails
    ↓
Result rejected
```

---

## 48. Example: Provider Switch

```text
Attempt using Provider A
        ↓
User switches provider
        ↓
Attempt authority revoked
        ↓
Provider A abort requested
        ↓
New AttemptId created for Provider B
        ↓
Scheduler admission
```

WorkItemId giữ nguyên nếu logical work không đổi.

---

## 49. Example: Non-Cancelable Provider

```text
Provider request running
        ↓
Cancellation requested
        ↓
Authority revoked
        ↓
Abort unsupported
        ↓
Attempt marked ABANDONED
        ↓
Provider capacity remains occupied
        ↓
Late response rejected and cleaned
```

---

## 50. Example: Session Close

```text
Session close
    ↓
Session authority revoked
    ↓
All revision/work/attempt scopes canceled
    ↓
Queued work removed
    ↓
Running work signaled
    ↓
UI detached
    ↓
Resources disposed when safe
```

---

## 51. Example: Cancellation Timeout

```text
Cancellation signal sent
        ↓
Grace deadline exceeded
        ↓
Runtime stops waiting
        ↓
Attempt becomes ABANDONED
        ↓
Late execution remains tracked
        ↓
Cleanup continues asynchronously
```

---

## 52. Architecture Invariants

1. Authority revocation xảy ra trước physical stop.
2. Parent cancellation ảnh hưởng child scope.
3. Child cancellation không tự cancel parent.
4. Cancellation không tự retry.
5. Canceled scope không commit.
6. Late Completion luôn validation.
7. Mỗi retry tạo AttemptId mới.
8. WorkItemId giữ nguyên khi logical work không đổi.
9. Hard termination bị cấm trong primary process.
10. Grace period luôn bounded.
11. Non-cancelable execution có thể trở thành abandoned.
12. Abandoned không đồng nghĩa resource đã được release.
13. Cleanup ownership explicit.
14. Cleanup failure không phục hồi authority.
15. Queue không sở hữu cancellation authority.
16. Scheduler không sở hữu cancellation outcome.
17. Worker không tự quyết định terminal outcome.
18. UI phản ánh logical cancellation, không chờ physical stop.
19. Event và log không chứa user content.
20. Cancellation semantics không phụ thuộc implementation token cụ thể.

---

## 53. Testing Requirements

Test phải bao phủ:

- cancel trước admission;
- cancel khi queued;
- cancel giữa dispatch và execution;
- cancel running Attempt;
- simultaneous completion và cancellation;
- session cancellation;
- revision replacement;
- provider abort supported;
- provider abort unsupported;
- cancellation timeout;
- late Completion;
- retry timer bị cancel;
- provider switch tạo Attempt mới;
- cleanup idempotency;
- child scope inheritance;
- UI commit validation;
- shutdown cancellation;
- resource still held after logical cancellation.

Tests dùng deterministic fake worker và provider.

---

## 54. Open Questions

- Grace period mặc định theo execution class là bao lâu?
- Local AI có cần isolated process không?
- Provider nào hỗ trợ abort thực sự?
- Abandoned provider slot được accounting thế nào?
- UI có giữ previous valid content đến khi replacement sẵn sàng không?
- Memory pressure ở mức nào được phép cancel current work?
- Partial output sau cancellation có bao giờ được giữ không?
- Cleanup retry budget là bao nhiêu?

Các câu hỏi này không chặn MVP architecture.

---

## 55. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | Authority, WorkItem, Attempt và terminal outcome |
| `RUNTIME_COMPONENTS.md` | Cancellation Coordinator ownership |
| `SCHEDULER.md` | Admission và preemption recommendation |
| `WORK_QUEUE.md` | Queued-work removal |
| `RETRY_POLICY.md` | Retry sau failure/cancellation |
| `ERROR_MODEL.md` | Canceled, stale, abandoned và failure |
| `MEMORY_MODEL.md` | Artifact Lease và revision retention |
| `RESOURCE_LIFECYCLE.md` | Drain và physical disposal |
| `THREADING_MODEL.md` | Cooperative cancellation context |
| `RUNTIME_CONFIG.md` | Grace period và timeout |
| `RUNTIME_OBSERVABILITY.md` | Metrics, events và logs |
| `BOOT_SEQUENCE.md` | Shutdown integration |

---

## 56. Completion Criteria

`CANCELLATION.md` được xem là đồng bộ khi:

- cancellation bắt đầu bằng authority revocation;
- scope chuẩn chỉ còn application, session, revision, WorkItem, Attempt và provider request;
- token không còn là architectural requirement;
- WorkItem lifecycle không bị định nghĩa lại;
- queued removal khớp Work Queue mới;
- checkpoint generic, không hard-code OCR/Translation;
- abandoned được tách rõ khỏi canceled;
- retry dùng same WorkItemId và new AttemptId;
- cleanup ownership tổng quát;
- UI không chờ physical stop;
- late result luôn bị authority validation;
- events, metrics và MVP policy nhất quán.

---

## 57. Summary

Cancellation trong CRAI không được hiểu đơn giản là gửi một tín hiệu dừng.

```text
Cancellation Request
        ↓
Authority Revoked
        ↓
Queued Work Removed
        ↓
Running Attempt Signaled
        ↓
Execution Stops or Becomes Abandoned
        ↓
Late Result Rejected
        ↓
Resources Drained Safely
```

Ranh giới cốt lõi:

```text
Cancellation protects correctness first.

Physical stopping protects efficiency second.
