# runtime/RETRY_POLICY.md

# Runtime Retry Policy

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime đánh giá và tạo một `Attempt` mới cho cùng một `WorkItem` khi physical execution trước đó không còn phù hợp hoặc không hoàn thành thành công.

Retry không tạo lại logical work.

```text
Same WorkItemId
    +
New AttemptId
```

Retry tồn tại để phục hồi transient execution failure mà không:

- tạo duplicate logical work;
- vượt retry budget;
- revive stale revision;
- revive canceled scope;
- gây retry storm;
- overload provider;
- phá authority model;
- bypass Scheduler;
- ghi đè accepted outcome cũ hoặc mới hơn.

---

## 2. Architectural Position

```text
Attempt ends
    ↓
Runtime Control validates outcome
    ↓
Retry Policy evaluates
    ↓
Retry Strategy selected
    ↓
New AttemptId created
    ↓
Scheduler admission
    ↓
Worker Execution
```

Retry Policy không:

- tạo BusinessExecutionPlan;
- tạo WorkItem mới khi logical work không đổi;
- chọn provider implementation;
- lookup cache trực tiếp;
- thực thi Attempt;
- commit Artifact;
- commit UI;
- sở hữu WorkItem terminal outcome;
- bypass Runtime Control hoặc Scheduler.

---

## 3. Core Principle

Retry là **physical attempt replacement**, không phải logical work recreation.

```text
WorkItem
    ├── Attempt 1
    ├── Attempt 2
    └── Attempt 3
```

WorkItem giữ nguyên identity.

Mỗi Attempt có identity và lifecycle độc lập.

---

## 4. Retry Ownership

Retry ownership được tách rõ:

```text
Runtime Control
    → validates relevance and authority

Retry Policy
    → decides retry eligibility and strategy

Provider Selection Policy
    → proposes provider candidate

Provider Manager
    → reports availability and capability

Scheduler
    → decides admission

Worker
    → executes new Attempt
```

Worker và Provider Adapter không tự retry.

---

## 5. Retry Vocabulary

### 5.1 Retry Evaluation

Quá trình đánh giá một outcome có đủ điều kiện tạo Attempt mới hay không.

### 5.2 Retry Strategy

Cách Retry được thực hiện:

```text
IMMEDIATE
DELAYED
RETRY_AFTER
PROVIDER_FALLBACK
WAIT_FOR_RESOURCE
```

### 5.3 Retry Budget

Giới hạn số lần và chi phí Retry.

### 5.4 Attempt Lineage

Chuỗi Attempt thuộc cùng một WorkItem.

### 5.5 Delayed Retry

Retry được lên lịch sau một khoảng delay cancelable.

### 5.6 Manual Re-execution

Request mới từ người dùng hoặc Application. Đây không mặc định là automatic retry.

---

## 6. Retry Trigger

Retry evaluation có thể bắt đầu khi:

- Attempt failed;
- provider timeout;
- provider switch được yêu cầu;
- worker execution bị gián đoạn;
- temporary resource không sẵn sàng;
- provider degraded;
- recoverable process restart;
- controlled execution abandonment;
- explicit runtime recovery action.

Không phải mọi trigger đều là technical failure.

---

## 7. Retry Eligibility

Retry chỉ được phép khi tất cả điều kiện sau thỏa mãn:

- WorkItem vẫn tồn tại;
- WorkItem chưa có accepted terminal outcome;
- session còn active;
- revision còn authority;
- BusinessExecutionPlan còn hiệu lực;
- cancellation chưa revoke scope;
- retry budget còn;
- deadline chưa hết;
- configuration version còn hợp lệ;
- required input ArtifactRef còn hợp lệ;
- error hoặc trigger được policy cho phép;
- runtime chưa stopping;
- privacy mode cho phép execution path;
- provider/resource capacity có thể được đáp ứng;
- Attempt lineage chưa bị supersede bởi accepted outcome khác.

---

## 8. Retry Decision

Retry Policy tạo một trong các decision:

```text
RETRY_NOW
RETRY_LATER
RETRY_WITH_FALLBACK
WAIT_FOR_RESOURCE
DO_NOT_RETRY
RETRY_EXHAUSTED
```

Decision phải có reason code.

---

## 9. Retry Reason Codes

Ví dụ:

```text
TRANSIENT_NETWORK_FAILURE
PROVIDER_TIMEOUT
PROVIDER_RATE_LIMITED
WORKER_INTERRUPTED
TEMPORARY_RESOURCE_UNAVAILABLE
PROVIDER_SWITCH_REQUESTED
PROVIDER_UNHEALTHY
RETRY_BUDGET_EXHAUSTED
REVISION_NOT_CURRENT
SESSION_INACTIVE
CANCELLATION_REQUESTED
DEADLINE_EXCEEDED
NON_RETRYABLE_ERROR
RUNTIME_STOPPING
PRIVACY_POLICY_BLOCKED
DEPENDENCY_INVALID
```

---

## 10. Attempt Lineage

Mỗi WorkItem có một lineage:

```text
WorkItemId = W1

AttemptId = A1
AttemptId = A2
AttemptId = A3
```

Rules:

1. WorkItemId không đổi.
2. AttemptId luôn mới.
3. AttemptNumber tăng đơn điệu.
4. Attempt cũ là terminal.
5. Attempt cũ không resume.
6. Late Completion phải qua authority validation.
7. Chỉ một WorkItem terminal outcome được chấp nhận.
8. Attempt lineage phải observable.

---

## 11. Retry Flow

```text
Attempt outcome reported
        ↓
Runtime Control validates identity
        ↓
Check session and revision authority
        ↓
Check accepted terminal outcome
        ↓
Retry Policy evaluates
        ↓
Check budget and deadline
        ↓
Re-evaluate Artifact reuse
        ↓
Select retry strategy
        ↓
Create new AttemptId
        ↓
Scheduler admission
```

---

## 12. Retry Classes

Automatic Retry Policy sử dụng:

```text
NONE
IMMEDIATE
DELAYED
PROVIDER_FALLBACK
RESOURCE_WAIT
```

`AFTER_USER_ACTION` không phải automatic retry class.

User action tạo manual re-execution hoặc business request mới.

---

## 13. Immediate Retry

Immediate retry chỉ dùng khi:

- failure có khả năng biến mất ngay;
- retry không làm tăng resource pressure;
- provider chưa degraded;
- side effect không bị duplicate;
- budget cho phép;
- chỉ một immediate retry trong cùng lineage branch;
- current revision vẫn có user value.

Không nên immediate retry khi:

- memory allocation đang fail do pressure;
- provider rate-limit vẫn còn;
- network outage chưa thay đổi;
- input deterministic invalid;
- configuration sai;
- credential invalid.

---

## 14. Delayed Retry

Delayed retry dùng cho:

- temporary network issue;
- provider overload;
- transient timeout;
- provider recovery;
- bounded resource wait.

Delay phải:

- cancelable;
- gắn CancellationContextRef;
- bounded;
- observable;
- check authority lại khi timer hết;
- không giữ resource lớn trong thời gian chờ.

---

## 15. Exponential Backoff

Repeated transient retry có thể dùng exponential backoff:

```text
baseDelay × growthFactor^attemptIndex
```

Backoff phải:

- có upper bound;
- phù hợp interactive latency;
- cancelable;
- không áp dụng mù quáng cho current user-visible work;
- không kéo dài vượt deadline.

---

## 16. Jitter

Jitter giúp tránh synchronized retry.

Jitter phải:

- bounded;
- deterministic trong test;
- không làm vượt deadline;
- không che mất `Retry-After`;
- có thể tắt trong MVP single-instance nếu chưa cần.

---

## 17. Retry-After

Nếu provider cung cấp `Retry-After`:

- Runtime nên tôn trọng nếu còn business value;
- authority phải được kiểm tra trước khi chờ;
- deadline phải được so sánh;
- provider capacity vẫn được tính đúng;
- cancellation phải hủy delayed retry;
- fallback có thể được ưu tiên nếu policy cho phép.

---

## 18. Retry Budget

Budget có thể tồn tại theo:

```text
WORK_ITEM
REVISION
SESSION
PROVIDER
GLOBAL_RUNTIME
```

Retry chỉ được tạo khi tất cả budget liên quan còn.

---

## 19. Attempt Count Budget

Giới hạn số Attempt trên một WorkItem.

Ví dụ conceptual:

```text
maxAttemptsPerWorkItem
```

Giá trị cụ thể nằm trong `RUNTIME_CONFIG.md`.

Không hard-code theo OCR, Layout hoặc Translation tại tài liệu này.

---

## 20. Concurrent Retry Budget

Ngoài max attempt count, Runtime cần giới hạn số retry đang chờ hoặc đang chạy:

```text
maxConcurrentRetries
maxDelayedRetries
maxProviderRetries
maxSessionRetries
```

Điều này ngăn nhiều WorkItem cùng retry làm overload Runtime.

---

## 21. Retry Cost Budget

Retry có thể tiêu thụ:

- provider cost;
- network quota;
- GPU time;
- CPU time;
- memory;
- user-visible latency.

Policy có thể từ chối Retry khi expected recovery value thấp hơn cost.

---

## 22. Deadline Boundary

Retry không được tạo khi:

```text
current time + expected retry delay + expected execution time
    >
useful result deadline
```

Exact estimation là implementation concern, nhưng architecture phải hỗ trợ deadline-aware decision.

---

## 23. Revision Validation

Trước khi tạo Attempt mới:

- revision phải còn current hoặc explicitly eligible;
- revision không bị superseded;
- revision authority không bị revoke;
- target presentation vẫn còn phù hợp.

Nếu không:

```text
DO_NOT_RETRY
```

---

## 24. Session Validation

Retry bị từ chối khi:

- session inactive;
- session stopping;
- session canceled;
- session configuration không còn tương thích;
- user intent đã đổi.

---

## 25. Cancellation Boundary

Cancellation hủy:

- pending retry evaluation;
- delayed retry timer;
- queued retry Attempt;
- resource-wait retry;
- provider fallback pending.

Cancellation không được để retry hồi sinh WorkItem.

---

## 26. Retry and Stale Result

Late Completion bị reject vì mất authority, không chỉ vì Attempt mới hơn tồn tại.

```text
Attempt 1 completes late
        ↓
Runtime Control validates
        ↓
Authority missing
        ↓
Rejected
```

Attempt 2 không tự động có commit authority chỉ vì mới hơn.

---

## 27. Cache and Artifact Re-evaluation

Retry Policy không lookup cache trực tiếp.

Luồng:

```text
Retry candidate
        ↓
Runtime Control / Cache Policy
        ↓
Artifact Store lookup
        ↓
Accepted reusable Artifact exists?
```

Nếu có Artifact hợp lệ:

- không cần tạo Attempt mới;
- WorkItem có thể được hoàn thành bằng accepted Artifact flow.

---

## 28. Provider Fallback

Retry Policy chỉ quyết định:

```text
fallback allowed?
```

Provider Selection Policy quyết định candidate.

Provider Manager cung cấp:

- availability;
- capability;
- health;
- concurrency;
- privacy eligibility.

Runtime Control tạo Attempt mới với provider reference mới.

---

## 29. Provider Fallback Rules

Fallback chỉ hợp lệ khi:

- business semantics tương thích;
- output contract tương thích;
- privacy mode cho phép;
- provider capability phù hợp;
- configuration cho phép;
- cost policy cho phép;
- deadline còn đủ;
- provider health hợp lệ.

---

## 30. Provider Adapter Boundary

Provider Adapter không:

- tự retry;
- tự fallback;
- tự tăng AttemptId;
- tự quyết định budget;
- tự commit result.

Adapter chỉ thực thi một Attempt được cấp.

---

## 31. Resource Boundary

Attempt cũ phải release resource khi có thể.

Tuy nhiên, physical cleanup có thể chưa hoàn tất với non-cancelable provider.

Retry eligibility và physical cleanup completeness là hai khái niệm khác nhau.

Attempt mới chỉ được admit khi:

- resource budget thực tế đủ;
- provider capacity thực tế đủ;
- shared resource không bị unsafe reuse.

---

## 32. Abandoned Attempt

Attempt bị `ABANDONED` có thể vẫn giữ physical resource.

Retry chỉ được tạo khi policy xác nhận:

- new Attempt không vi phạm capacity;
- provider slot thực tế còn;
- duplicate side effect được kiểm soát;
- late result từ abandoned Attempt sẽ bị reject.

---

## 33. Idempotency

Retry chỉ an toàn khi execution side effect:

- idempotent;
- deduplicated;
- hoặc được bảo vệ bằng request identity.

Provider request có thể cần:

```text
WorkItemId
AttemptId
IdempotencyKey
```

Exact mechanism thuộc provider architecture.

---

## 34. Retry Storm Prevention

Sử dụng:

- bounded attempt count;
- concurrent retry budget;
- exponential backoff;
- jitter;
- provider degradation state;
- global runtime budget;
- circuit-breaker-like admission policy nếu cần;
- cancellation;
- current-revision priority;
- delayed retry deduplication.

---

## 35. Retry Deduplication

Trong cùng WorkItem:

- không tạo nhiều pending Retry cho cùng failed Attempt;
- một retry evaluation chỉ được accept một lần;
- delayed timer phải có unique identity;
- duplicate retry signal phải bị ignore.

---

## 36. Manual Re-execution

Manual re-execution là Application request mới.

Nó có thể:

- giữ cùng WorkItem nếu chỉ thay Attempt execution detail;
- tạo BusinessExecutionPlan mới nếu business intent đổi;
- tạo Revision mới nếu source/configuration thay đổi.

Không nên tự động gọi mọi manual action là retry.

---

## 37. Provider Switch

Provider switch có thể là:

### Attempt Replacement

Nếu logical work và business input không đổi:

```text
same WorkItemId
new AttemptId
new ProviderRef
```

### Business Replan

Nếu output semantics hoặc profile thay đổi:

```text
new BusinessExecutionPlan
possibly new Revision
```

---

## 38. Retry During Shutdown

Khi shutdown bắt đầu:

- không tạo Retry mới;
- delayed retry bị cancel;
- pending resource wait bị cancel;
- retry timer bị dispose;
- queued retry Attempt bị remove;
- running Attempt theo Cancellation Policy.

Không retry nào sống qua Runtime shutdown.

---

## 39. Retry Events

Conceptual events:

```text
RETRY_EVALUATED
RETRY_APPROVED
RETRY_SKIPPED
RETRY_DELAYED
RETRY_ADMITTED
RETRY_EXHAUSTED
RETRY_CANCELED
RETRY_DUPLICATE_REJECTED
PROVIDER_FALLBACK_ALLOWED
PROVIDER_FALLBACK_SELECTED
```

Tên cuối phải tuân theo Event Standard.

---

## 40. Event Payload

Retry event có thể chứa:

```text
EventId
OccurredAt
SessionId
RevisionId
WorkItemId
PreviousAttemptId
NewAttemptId
AttemptNumber
RetryDecision
RetryStrategy
ReasonCode
Delay
ProviderRef
BudgetSnapshotRef
```

Không chứa:

- source text;
- translated text;
- screenshot;
- prompt;
- secret;
- raw provider payload.

---

## 41. Metrics

Theo dõi:

- retry evaluation count;
- retry approved count;
- retry skipped count;
- retry exhausted count;
- retry canceled count;
- retry success ratio;
- attempt count per WorkItem;
- retry latency;
- recovery latency;
- delayed retry count;
- provider fallback count;
- global concurrent retry count;
- retry budget exhaustion;
- retry storm prevention activation;
- abandoned-attempt overlap count.

---

## 42. Performance Accounting

Phân biệt:

```text
Initial Attempt Latency
Retry Delay
Retry Queue Wait
Retry Execution Latency
Recovery Latency
Total Useful Result Latency
```

Retry không được che queue wait hoặc provider wait trong một metric duy nhất.

---

## 43. Failure Classification Boundary

Retry Policy nhận normalized `RuntimeError` hoặc recovery trigger.

Retry Policy không tự parse raw provider error.

Error classification thuộc `ERROR_MODEL.md`.

---

## 44. MVP Retry Policy

### 44.1 Required Strategies

```text
NONE
IMMEDIATE
DELAYED
PROVIDER_FALLBACK
```

### 44.2 Required Rules

1. Same WorkItemId.
2. New AttemptId.
3. Runtime Control owns evaluation.
4. Retry Policy owns eligibility and strategy.
5. Scheduler owns admission.
6. Worker and Provider Adapter never retry themselves.
7. Revision must remain current.
8. Session must remain active.
9. Cancellation invalidates retry.
10. Budget must remain.
11. Cache/Artifact reuse is re-evaluated.
12. Delayed retry is cancelable.
13. Shutdown cancels all pending retry.
14. Provider fallback must respect privacy and capability.
15. Retry never restores authority automatically.

---

## 45. MVP Budget Guidance

Exact values nằm trong `RUNTIME_CONFIG.md`.

Conceptual defaults:

```text
interactive immediate retry: at most 1
interactive total attempts: small bounded number
background retry: larger delay, still bounded
global concurrent retry: very small
provider retry: limited by provider health
```

Không hard-code theo OCR, Layout, Translation hoặc Presentation.

---

## 46. Example: Transient Provider Timeout

```text
Attempt 1 times out
        ↓
Runtime Control validates relevance
        ↓
Retry Policy selects DELAYED
        ↓
Delay timer created
        ↓
Authority checked again
        ↓
Attempt 2 created
        ↓
Scheduler admission
```

---

## 47. Example: Provider Fallback

```text
Attempt 1 on Provider A fails
        ↓
Fallback allowed
        ↓
Provider Selection chooses Provider B
        ↓
Attempt 2 created
        ↓
Scheduler admission
```

WorkItemId giữ nguyên.

---

## 48. Example: Revision Becomes Stale

```text
Attempt 1 fails
        ↓
Newer revision becomes current
        ↓
Retry evaluation runs
        ↓
Revision authority missing
        ↓
DO_NOT_RETRY
```

---

## 49. Example: Cache Satisfies Retry

```text
Attempt 1 fails
        ↓
Retry candidate created
        ↓
Artifact Store re-evaluated
        ↓
Reusable accepted Artifact found
        ↓
No new Attempt required
```

---

## 50. Example: Abandoned Provider Request

```text
Attempt 1 abandoned but provider still running
        ↓
Retry Policy considers fallback
        ↓
Provider capacity checked
        ↓
Capacity unavailable
        ↓
WAIT_FOR_RESOURCE or DO_NOT_RETRY
```

---

## 51. Architecture Invariants

1. Retry không thay đổi WorkItemId.
2. Retry luôn tạo AttemptId mới.
3. Attempt cũ không resume.
4. Runtime Control sở hữu retry authority.
5. Retry Policy không sở hữu provider selection.
6. Scheduler không tạo Retry.
7. Worker không tự retry.
8. Provider Adapter không tự retry.
9. Retry không bypass Scheduler.
10. Retry không bypass cancellation.
11. Retry không bypass authority validation.
12. Retry không tạo business work mới.
13. Retry không tự commit Artifact hoặc UI.
14. Retry không revive stale revision.
15. Retry không revive canceled scope.
16. Retry budget luôn bounded.
17. Delayed retry luôn cancelable.
18. Retry timer là runtime resource phải cleanup.
19. Provider fallback tạo Attempt mới.
20. Cache lookup không thuộc Retry Policy.
21. Abandoned Attempt có thể vẫn giữ resource.
22. Retry không giả định physical cleanup đã hoàn tất.
23. Manual re-execution không mặc định là automatic retry.
24. Shutdown hủy toàn bộ pending retry.
25. Retry events không chứa user content.

---

## 52. Testing Requirements

Test phải bao phủ:

- immediate retry;
- delayed retry;
- retry-after;
- jitter deterministic;
- budget exhausted;
- global retry budget;
- session inactive;
- revision stale;
- cancellation trước timer;
- cancellation sau timer nhưng trước admission;
- duplicate retry signal;
- provider fallback;
- privacy-blocked fallback;
- cache satisfies retry;
- abandoned provider holds capacity;
- shutdown cancels retry;
- same WorkItemId/new AttemptId;
- late previous Attempt completion;
- resource wait;
- manual re-execution boundary.

---

## 53. Open Questions

- Budget cụ thể theo WorkType là bao nhiêu?
- Có cần circuit breaker riêng không?
- Khi nào provider fallback tốt hơn delayed retry?
- Có cần adaptive backoff theo observed latency không?
- Abandoned provider request được accounting vào retry budget thế nào?
- Manual retry UI tạo Attempt mới hay business replan?
- Retry cost budget có cần theo tiền thật không?
- Idempotency key chuẩn hóa ở đâu?
- Có cần persistent retry sau application restart không? MVP: không.

---

## 54. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem, Attempt và authority |
| `RUNTIME_COMPONENTS.md` | Retry Coordinator ownership |
| `SCHEDULER.md` | Admission |
| `WORK_QUEUE.md` | Queued retry Attempt |
| `CANCELLATION.md` | Retry invalidation |
| `ERROR_MODEL.md` | RuntimeError và retry classification |
| `CACHE_POLICY.md` | Artifact reuse |
| `MEMORY_MODEL.md` | Artifact and resource state |
| `RESOURCE_LIFECYCLE.md` | Attempt cleanup |
| `PERFORMANCE_MODEL.md` | Recovery latency |
| `RUNTIME_CONFIG.md` | Budget, delay và attempts |
| `RUNTIME_OBSERVABILITY.md` | Retry metrics and traces |

---

## 55. Completion Criteria

`RETRY_POLICY.md` được xem là đồng bộ khi:

- retry được mô tả là Attempt replacement;
- same WorkItemId/new AttemptId được chốt;
- authority vẫn thuộc Runtime Control;
- Retry Policy không chọn provider implementation;
- Scheduler chỉ làm admission;
- cache lookup tách khỏi Retry Policy;
- automatic retry và manual re-execution được phân biệt;
- retry budget có WorkItem, Revision, Session, Provider và Global scope;
- concurrent retry budget được mô tả;
- abandoned Attempt và resource capacity được xử lý đúng;
- event, metric và MVP policy thống nhất;
- không hard-code OCR/Translation/Layout attempt count.

---

## 56. Summary

Retry trong CRAI quản lý Attempt lineage:

```text
WorkItem
    ↓
Attempt 1
    ↓
Retry Evaluation
    ↓
Attempt 2
    ↓
Scheduler Admission
    ↓
Execution
```

Ranh giới cốt lõi:

```text
Retry keeps logical work unchanged.

Retry creates a new physical Attempt.

Runtime Control decides authority.

Scheduler decides admission.

Workers only execute.
```
