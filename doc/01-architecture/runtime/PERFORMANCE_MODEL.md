# runtime/PERFORMANCE_MODEL.md

# Runtime Performance Model

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime đánh giá, ngân sách hóa, bảo vệ, đo lường và cải thiện hiệu năng theo trải nghiệm đọc thực tế.

CRAI là interactive reading assistant.

Hiệu năng không được đánh giá chỉ bằng:

- raw throughput;
- số WorkItem hoàn thành;
- CPU utilization;
- provider median latency;
- tốc độ của một Business Module riêng lẻ.

Câu hỏi chính là:

> CRAI có thể trình bày kết quả hữu ích, đúng authority và còn liên quan tới nội dung người dùng đang đọc nhanh và ổn định đến mức nào?

---

## 2. Scope

Tài liệu này bao phủ:

- useful-result latency;
- time to first useful result;
- interaction latency;
- observation latency;
- queue wait;
- WorkItem và Attempt latency;
- authority-validation latency;
- ownership-transfer latency;
- Artifact publication latency;
- UI commit latency;
- freshness;
- useful-work ratio;
- resource pressure;
- cache reuse value;
- Resource Lease performance;
- logical/physical disposal performance;
- provider performance;
- overload;
- graceful degradation;
- benchmark;
- regression;
- MVP targets.

Không định nghĩa:

- provider pricing cuối cùng;
- exact hardware requirements;
- exact retry rules;
- exact error taxonomy;
- persistent schema;
- implementation language/framework;
- Business Module algorithm chi tiết.

---

## 3. Performance Philosophy

CRAI tuân theo:

```text
Correct Current Result
    ↓
Responsive UI
    ↓
Low Useful-Result Latency
    ↓
Stable Resource Usage
    ↓
Predictable Recovery
    ↓
Provider Cost Efficiency
    ↓
Maximum Throughput
```

Nguyên tắc cốt lõi:

```text
Optimize for current useful output,
not maximum executed work.
```

Một hệ thống xử lý nhiều obsolete WorkItem không được coi là performant.

---

## 4. Primary Performance Outcome

Metric chính:

```text
Useful Result Latency
```

Được đo từ khi content hiện tại đủ ổn định để xử lý đến khi result hợp lệ cho current Revision được commit cho người dùng.

```text
Stable Content
    ↓
Revision Created
    ↓
Required WorkItems Completed
    ↓
Candidate Artifact Validated
    ↓
Artifact Published
    ↓
Presentation Committed
```

Không tính là useful result nếu:

- Revision đã obsolete;
- authority validation fail;
- Artifact không được publish;
- UI commit bị reject;
- user đã chuyển khỏi content;
- result quá thiếu để hỗ trợ đọc.

---

## 5. Performance Dimensions

```text
Performance
├── Responsiveness
├── Useful Latency
├── Freshness
├── Predictability
├── Stability
├── Resource Efficiency
├── Recovery
├── Cost Efficiency
└── Quality Preservation
```

### Responsiveness

Runtime phản hồi user/control command nhanh đến đâu.

### Useful Latency

Bao lâu để tạo output thực sự có ích.

### Freshness

Output còn đại diện cho current content hay không.

### Predictability

P50/P95/P99 có ổn định không.

### Stability

Runtime có giữ bounded resource trong long session không.

### Resource Efficiency

CPU, GPU, memory, provider, queue, lease và Artifact được dùng hiệu quả không.

### Recovery

Runtime phục hồi khỏi overload/failure/cancellation nhanh đến đâu.

---

## 6. Latency Categories

### Interaction Latency

User command → immediate UI acknowledgment.

### Observation Latency

Frame/source update → stable-content decision.

### Planning Latency

Business request → BusinessExecutionPlan.

### Work Creation Latency

Plan accepted → WorkItem ready.

### Admission Latency

WorkItem ready → Scheduler decision.

### Queue Wait

Admitted → dispatched.

### Attempt Execution Latency

Attempt start → Completion reported.

### Authority Validation Latency

Completion received → accepted/rejected.

### Ownership Transfer Latency

Candidate accepted → Artifact Store ownership acquired.

### Publication Latency

Ownership accepted → Artifact publicly available.

### Commit Latency

Presentation ready → UI visible.

### Useful Result Latency

Stable current content → valid current visible result.

### Recovery Latency

Failure/cancellation/pressure → normal useful processing restored.

---

## 7. End-to-End Latency Model

Conceptual formula:

```text
T_useful =
    T_observation
  + T_revision
  + T_planning
  + T_work_creation
  + T_reuse_lookup
  + T_admission
  + T_queue
  + T_attempt_execution
  + T_authority_validation
  + T_ownership_transfer
  + T_publication
  + T_presentation
  + T_ui_dispatch
  + T_commit
```

Không phải mọi execution đều có mọi thành phần.

Một reusable Artifact có thể loại bỏ:

- Attempt creation;
- queue wait;
- provider execution;
- parts of presentation work.

---

## 8. Critical Path

Critical path là chuỗi tối thiểu tạo current useful output.

```text
Stable Content
    ↓
Revision
    ↓
Required WorkItems
    ↓
Accepted Artifacts
    ↓
Presentation
    ↓
UI Commit
```

Business Module internals như OCR, segmentation hoặc provider call không được hard-code thành Runtime critical-path vocabulary.

---

## 9. Critical-Path Protection

Runtime phải:

- ưu tiên current Revision;
- loại obsolete queued work;
- revoke authority sớm;
- giữ control-path capacity;
- giới hạn provider concurrency;
- giới hạn resource usage;
- tránh UI blocking;
- dùng bounded queue;
- giảm background/speculative work dưới pressure;
- ưu tiên reusable accepted Artifact.

---

## 10. Timing Targets

Provisional MVP targets:

| Operation | Initial target |
|---|---:|
| Immediate UI acknowledgment | under 100 ms |
| Runtime control command handling | under 50 ms typical |
| Observation lightweight decision | under 50 ms |
| Current WorkItem admission | under 50 ms |
| Authority validation | under 20 ms typical |
| Candidate publication | under 50 ms typical |
| UI commit after ready | under 100 ms |
| Cancellation authority propagation | under 100 ms |
| Obsolete queued-work removal | under 100 ms |
| Cached/reused presentation | under 200 ms |
| Current useful result | preferably under 2 seconds |

Các số này là hypothesis ban đầu, không phải product guarantee.

---

## 11. Percentile Evaluation

Metric quan trọng phải đo:

```text
P50
P90
P95
P99
```

Average không đủ.

Tail latency cần được phân tích theo cause:

- provider;
- queue;
- resource pressure;
- cold start;
- retry;
- lock/contention;
- lease wait;
- UI dispatch;
- publication delay;
- cleanup pressure.

---

## 12. WorkItem Timing Model

Conceptual timestamps:

```text
CreatedAt
EligibleAt
AdmittedAt
QueuedAt
DispatchedAt
AttemptStartedAt
ProviderRequestedAt
ProviderCompletedAt
CompletionReportedAt
AuthorityValidatedAt
OwnershipTransferredAt
PublishedAt
CommitRequestedAt
CommittedAt
CanceledAt
LogicalDisposedAt
PhysicalDisposedAt
```

Không phải WorkItem nào cũng cần mọi timestamp.

---

## 13. Attempt Timing

Mỗi Attempt phải tách:

```text
Queue Wait
Resource Wait
Lease Wait
Execution Time
Provider Wait
Normalization Time
Completion Dispatch
Authority Validation
Cleanup Time
```

Điều này tránh nhầm execution chậm với admission hoặc resource wait.

---

## 14. Useful Work Model

Phân biệt:

```text
Executed Work
Completed Work
Accepted Work
Published Work
Committed Work
Useful Work
```

### Executed Work

Attempt đã chạy.

### Completed Work

Worker đã báo Completion.

### Accepted Work

Runtime Control chấp nhận outcome.

### Published Work

Artifact đã publish.

### Committed Work

Presentation đã commit.

### Useful Work

Current user thực sự hưởng lợi.

---

## 15. Useful Work Ratio

```text
Useful Work Ratio =
Useful Current Work
/
Total Executed Work
```

Ngoài ra đo:

```text
Accepted / Completed
Published / Accepted
Committed / Published
Useful / Committed
```

Các ratio này giúp tìm đúng điểm waste.

---

## 16. Wasted Work

Bao gồm:

- stale Attempt;
- canceled work hoàn thành muộn;
- duplicate provider request;
- duplicate computation;
- Artifact accepted nhưng không dùng;
- Artifact publish nhưng không commit;
- presentation commit bị reject;
- speculative work evicted trước use;
- resource held sau authority loss.

Wasted work phải bounded và observable.

---

## 17. Freshness

Metric:

- current Revision commit ratio;
- stale Completion ratio;
- stale Artifact rejection;
- average obsolete execution duration;
- cancellation propagation delay;
- revision churn;
- displayed Revision lag;
- latest stable content lag.

Một stale result nhanh vẫn là kết quả kém.

---

## 18. Authority Performance

Đo:

- authority-validation latency;
- duplicate Completion rejection latency;
- stale rejection latency;
- cancellation authority propagation;
- commit revalidation latency;
- authority conflict count;
- rejected late result count.

Authority validation phải nhanh và không block control path.

---

## 19. Publication Performance

Đo riêng:

```text
Candidate Created
    ↓
Validation
    ↓
Ownership Transfer
    ↓
Publication
```

Metrics:

- candidate validation time;
- ownership-transfer latency;
- publication latency;
- publication failure count;
- duplicate publication rejection;
- candidate cleanup after rejection.

---

## 20. Resource Lease Performance

Metrics:

- Lease acquisition delay;
- Lease hold time;
- Lease contention;
- denied Lease count;
- disposal blocked by Lease;
- leaked Lease count;
- Lease lifetime by resource type.

Lease không được chờ vô hạn.

---

## 21. Resource Lifecycle Performance

Đo:

```text
Logical Disposal
    ↓
Draining
    ↓
Physical Disposal
```

Metrics:

- logical-disposal latency;
- draining duration;
- physical-disposal latency;
- cleanup retry count;
- disposal failure;
- resource leak;
- native/GPU cleanup latency;
- resource still held after authority loss.

---

## 22. Resource Pressure

Unified pressure model:

```text
Resource Pressure
├── CPU
├── Memory
├── GPU
├── Provider
├── Queue
├── Lease
├── Artifact
├── Native Handle
└── UI
```

Levels:

```text
NORMAL
ELEVATED
HIGH
CRITICAL
```

Pressure signal phải dẫn tới Scheduler/Runtime Control action, không tự thay đổi business state.

---

## 23. Queue Performance

Đo:

- depth theo logical queue class;
- queue wait theo WorkType;
- admission latency;
- replace count;
- obsolete removal;
- hard/soft limit duration;
- current-revision queue ratio;
- control queue delay;
- background starvation có chủ đích;
- dispatch failure.

Không dùng Stage làm dimension bắt buộc.

---

## 24. Scheduler Performance

Đo:

- decision latency;
- admit/defer/reject/replace count;
- decision reason;
- fairness delay;
- current-revision admission ratio;
- resource-pressure decisions;
- preemption recommendation delay;
- control capacity availability.

---

## 25. Capture and Observation Performance

Đo:

- frame acquisition latency;
- callback delay;
- replacement count;
- dropped observation;
- stability detection latency;
- source fingerprint cost;
- capture CPU/GPU;
- revision churn;
- no-change suppression.

Mục tiêu không phải xử lý mọi frame.

---

## 26. Business Module Performance

Mỗi Business Module tự khai báo metrics semantic của mình.

Runtime chỉ yêu cầu common dimensions:

```text
OwnerModule
WorkType
Operation
InputSizeClass
OutputSizeClass
ExecutionClass
ProviderProfile
```

Module-specific example có thể gồm Recognition, Translation hoặc Presentation, nhưng không trở thành Runtime stage taxonomy.

---

## 27. Provider Performance

Đo theo:

```text
Provider
ProviderProfile
Model
Operation
ExecutionClass
Region
Version
```

Metrics:

- request latency;
- provider queue;
- timeout;
- failure;
- rate limit;
- cold start;
- payload size;
- cost estimate;
- cancellation support;
- abandoned request duration;
- stale completion ratio;
- fallback recovery.

---

## 28. Cache and Reuse Performance

Không chỉ đo hit/miss.

Đo:

- useful hit ratio;
- validation reject;
- compatibility miss;
- integrity failure;
- promotion cost;
- retention cost;
- eviction cost;
- saved useful latency;
- saved provider cost;
- in-flight coalescing;
- durable lookup latency;
- privacy partition miss.

Conceptual value:

```text
Reuse Value =
Avoided Useful Cost
-
Lookup Cost
-
Validation Cost
-
Retention Cost
-
Eviction Cost
```

---

## 29. Retry Performance

Đo:

- first Attempt latency;
- retry delay;
- retry queue wait;
- retry execution;
- recovery latency;
- duplicate provider cost;
- retry budget exhaustion;
- concurrent retry pressure;
- retry canceled by newer authority;
- cache satisfying retry.

---

## 30. Cancellation Performance

Đo:

- authority revoke latency;
- queued removal;
- Worker acknowledgment;
- provider abort;
- grace period;
- abandoned count;
- post-cancel execution time;
- resource drain duration;
- late Completion reject cost.

---

## 31. UI Performance

Đo:

- UI command acknowledgment;
- dispatcher delay;
- long task;
- presentation replacement;
- frame stutter;
- layout thrashing;
- repeated loading duration;
- atomic commit latency;
- commit revalidation.

Heavy processing không chạy trên UI Context.

---

## 32. Cold Start

Đo riêng:

- application startup;
- Runtime Control initialization;
- Capture initialization;
- Provider Manager initialization;
- local model load;
- first Artifact Store use;
- first useful result.

Không trộn cold start với steady state.

---

## 33. Long-Session Stability

Test dài phải xác nhận:

- memory bounded;
- Artifact count bounded;
- Lease count bounded;
- queue bounded;
- thread/context count bounded;
- provider health ổn định;
- UI responsiveness không giảm;
- draining resource không tích tụ;
- cleanup vẫn hoạt động;
- diagnostics bounded;
- useful latency không trôi dần.

---

## 34. Overload Definition

Runtime overloaded khi incoming/generated work vượt khả năng tạo current useful result trong budget.

Symptoms:

- queue growth;
- rising stale ratio;
- rising useful latency;
- memory/GPU growth;
- Lease contention;
- provider saturation;
- draining accumulation;
- UI commit delay;
- cancellation after expensive execution;
- current-revision starvation.

---

## 35. Overload Response Order

1. reject stale result;
2. remove obsolete queued work;
3. revoke obsolete authority;
4. cancel obsolete running work;
5. stop speculative work;
6. stop background work;
7. reduce capture/observation rate;
8. reduce batch size;
9. reduce concurrency;
10. evict low-value retention;
11. unload idle resource;
12. reduce quality only when explicitly permitted;
13. delay/reject noncritical work.

Current Revision và control path được bảo vệ.

---

## 36. Graceful Degradation

Levels:

```text
FULL
REDUCED
MINIMAL
CONTROL_ONLY
```

Degradation có thể thay:

- capture rate;
- input resolution;
- context size;
- number of visible regions;
- provider mode;
- background work;
- model residency.

Mọi degradation phải:

- preserve correctness;
- observable;
- reversible;
- configuration-controlled;
- không bypass privacy.

---

## 37. Quality and Performance

Optimization phải đo cả quality.

Ví dụ:

```text
Lower input resolution
    → lower latency
    → possible recognition loss
```

```text
Smaller context
    → lower provider cost
    → possible consistency loss
```

Không chấp nhận performance gain nếu quality xuống dưới product threshold.

---

## 38. Performance Events

Conceptual events:

```text
PERFORMANCE_PRESSURE_CHANGED
PERFORMANCE_BUDGET_EXCEEDED
WORKTYPE_SLOW
PROVIDER_SLOW
QUEUE_SATURATED
STALE_RATIO_HIGH
DEGRADATION_ENTERED
DEGRADATION_EXITED
MODEL_COLD_START
RECOVERY_COMPLETED
AUTHORITY_VALIDATION_SLOW
PUBLICATION_SLOW
LEASE_CONTENTION_HIGH
RESOURCE_DRAIN_SLOW
```

Tên cuối theo Event Standard.

---

## 39. Core Metrics

### End-to-End

- useful-result latency;
- time to first useful result;
- current-revision commit latency;
- current-revision useful success ratio.

### Runtime

- WorkItem creation latency;
- admission latency;
- queue wait;
- Attempt execution;
- authority validation;
- ownership transfer;
- publication;
- commit.

### Resources

- CPU;
- memory;
- GPU;
- network;
- provider in-flight;
- Artifact bytes;
- Lease count;
- draining resource;
- native handle;
- Worker utilization.

### User Experience

- UI dispatch;
- UI long task;
- presentation replacement;
- loading duration;
- stale-content visibility.

---

## 40. Metric Dimensions

Preferred low-cardinality dimensions:

```text
OwnerModule
WorkType
Operation
Provider
ProviderProfile
ExecutionClass
CacheStatus
TerminalOutcome
CancellationReason
DeviceProfile
RevisionState
PressureLevel
```

Raw SessionId/RevisionId/WorkItemId/AttemptId dùng trong trace/log, không dùng bừa trong aggregate metric.

---

## 41. Tracing

Một Revision trace nên nối:

```text
Observation
    ↓
Revision Creation
    ↓
Business Plan
    ↓
Reuse Evaluation
    ↓
WorkItem
    ↓
Attempt
    ↓
Completion
    ↓
Authority Validation
    ↓
Ownership Transfer
    ↓
Artifact Publication
    ↓
Presentation Commit
```

Span nên có:

- queue wait;
- Lease wait;
- execution;
- provider;
- cache/reuse;
- cancellation;
- disposition;
- publication;
- freshness.

---

## 42. Benchmark Classes

### Microbenchmark

Một operation nhỏ.

### WorkType Benchmark

Một WorkType với realistic input.

### Authority Benchmark

Validation throughput và latency.

### Publication Benchmark

Candidate → accepted publication.

### Lease Benchmark

Acquire/release/contention.

### Resource Lifecycle Benchmark

Logical disposal → physical disposal.

### End-to-End Benchmark

Stable content → visible useful result.

### Stress Benchmark

Overload behavior.

### Endurance Benchmark

Long-session stability.

### Provider Benchmark

Latency, variability, cost và cancellation.

---

## 43. Benchmark Inputs

Representative set nên có:

- simple comic page;
- dense page;
- Chinese vertical text;
- Chinese horizontal text;
- stylized font;
- low contrast;
- high resolution;
- rapid scroll;
- repeated content;
- partial viewport change;
- large document section;
- provider delay;
- resource pressure.

---

## 44. Controlled Testing

Control:

- source input;
- observation timing;
- Revision creation;
- provider delay;
- completion order;
- cache state;
- queue state;
- concurrency;
- cancellation timing;
- UI dispatch;
- resource pressure;
- retry;
- cleanup delay.

Dùng fake provider/worker/clock khi có thể.

---

## 45. Regression Policy

Regression nếu materially worsen:

- useful latency;
- tail latency;
- stale ratio;
- authority validation;
- publication latency;
- queue wait;
- CPU/memory/GPU;
- Lease contention;
- provider count/cost;
- UI responsiveness;
- long-session stability;
- cleanup latency.

Threshold đặt sau khi có baseline.

---

## 46. Optimization Workflow

```text
Measure
    ↓
Identify Critical Bottleneck
    ↓
Form Hypothesis
    ↓
Change One Variable
    ↓
Benchmark
    ↓
Compare Quality + Resource Cost
    ↓
Keep or Revert
```

Không thêm complexity vì suy đoán.

---

## 47. Premature Optimization Policy

MVP tránh:

- custom scheduler phức tạp;
- custom allocator;
- aggressive pooling;
- multi-process pipeline không cần thiết;
- distributed cache;
- speculative execution;
- aggressive parallel provider calls;
- fine-grained recomputation graph;
- adaptive routing chưa có baseline;
- NUMA-specific optimization.

---

## 48. MVP Performance Policy

```text
Current Revision First
+
Low Bounded Concurrency
+
Latest-Value Observation
+
Memory Artifact Reuse
+
Bounded Provider Requests
+
Atomic Publication
+
Atomic UI Commit
```

Primary goals:

1. UI không freeze.
2. Capture không chờ domain work.
3. Obsolete queued work removed nhanh.
4. Late results không commit.
5. Compatible Artifact được reuse.
6. Memory và resource stabilizes.
7. Provider concurrency bounded.
8. Performance telemetry chỉ ra latency nằm ở đâu.
9. Authority và publication overhead nhỏ.
10. Cleanup không tích tụ.

---

## 49. MVP Priority Order

```text
Runtime Control
    ↓
Cancellation / Revision Replacement
    ↓
Current Required Work
    ↓
Authority Validation
    ↓
Publication
    ↓
Current Presentation Commit
    ↓
Cache Maintenance
    ↓
Diagnostics
    ↓
Speculative Work
```

---

## 50. MVP Concurrency

Không hard-code theo OCR/Layout architecture.

Conceptual initial limits:

| Workload class | Initial concurrency |
|---|---:|
| Capture source | 1 |
| Observation serial context | 1 |
| CPU-heavy WorkType | 1 |
| Provider profile | 1 |
| GPU/native serial provider | 1 |
| Presentation commit | 1 |
| Maintenance | 1 low-priority |

Exact value thuộc `RUNTIME_CONFIG.md`.

---

## 51. MVP Performance Dashboard

Nên hiển thị:

```text
Current Revision
Current Runtime State
Useful Result Latency
WorkItem Timing
Attempt Timing
Queue Depth
Provider In-Flight
Reuse Status
Authority Validation
Publication Delay
CPU / Memory / GPU
Lease Count
Draining Resource
Stale Completion
Cancellation
```

Có thể chỉ là development diagnostics.

---

## 52. Example: Normal Execution

```text
Stable content
    ↓  observation
Revision created
    ↓  planning/admission
Attempt executed
    ↓  Completion
Authority validated
    ↓  ownership transfer
Artifact published
    ↓  presentation commit
```

Useful latency tính toàn bộ chuỗi.

---

## 53. Example: Reuse Hit

```text
ReuseQuery
    ↓
Compatible Artifact found
    ↓
Authority validated
    ↓
Presentation reused/built
    ↓
UI committed
```

Không tạo new Attempt nếu không cần.

---

## 54. Example: Rapid Scrolling

```text
Revision A running
    ↓
Revision B current
    ↓
A authority revoked
    ↓
A queued work removed
    ↓
B admitted
    ↓
A late Completion rejected
```

Success nghĩa là queue không tăng và B bắt đầu nhanh.

---

## 55. Example: Slow Provider

```text
Provider latency increases
    ↓
Provider pressure rises
    ↓
Admission reduced
    ↓
Background work stopped
    ↓
Current work preserved
```

UI và control path vẫn responsive.

---

## 56. Example: Resource Pressure

```text
Pressure = HIGH
    ↓
Scheduler reduces admission
    ↓
Low-value retention evicted
    ↓
Obsolete Revision drained
    ↓
Idle provider resource unloaded
    ↓
Current useful work preserved
```

---

## 57. Architecture Invariants

1. UI responsiveness ưu tiên hơn raw throughput.
2. Current useful work ưu tiên obsolete work.
3. Stale Completion không tính useful throughput.
4. Queue và concurrency bounded.
5. Control path luôn có capacity.
6. Capture không queue mọi frame.
7. Provider requests bounded.
8. Performance optimization không bypass authority.
9. Performance optimization không bypass ownership.
10. Performance optimization không bypass compatibility.
11. Cache optional cho correctness.
12. Memory/resource growth bounded.
13. Tail latency được đo.
14. Queue wait và execution tách riêng.
15. Authority-validation latency measurable.
16. Publication latency measurable.
17. Ownership-transfer latency measurable.
18. Lease wait bounded và measurable.
19. Logical disposal measurable.
20. Physical disposal measurable.
21. Draining resource observable.
22. Useful work excludes rejected publication.
23. Useful work excludes stale commit.
24. Quality degradation explicit.
25. Overload được phản ứng trước process instability.
26. Background work không block critical path.
27. Resource pressure không tự thay business semantics.
28. Provider median không đủ để chọn provider.
29. Long-session stability là performance requirement.
30. Metrics không chứa user content.

---

## 58. Testing Requirements

Test:

- normal current result;
- reuse hit;
- rapid scrolling;
- slow provider;
- provider timeout;
- repeated cancellation;
- high WorkItem count;
- high-resolution source;
- CPU saturation;
- GPU contention;
- memory pressure;
- Lease contention;
- publication delay;
- authority validation under duplicate Completion;
- UI dispatch delay;
- long session;
- cache eviction during active Lease;
- cold start;
- warm steady state;
- shutdown during slow work;
- cleanup delay;
- abandoned provider request;
- degradation transitions.

---

## 59. Open Questions

- Minimum supported hardware?
- Acceptable useful-result latency?
- Capture rate?
- Stability delay?
- Main Recognition/Translation providers?
- Local vs remote execution?
- Partial result cần trong MVP không?
- Cache budget?
- Provider timeout?
- Adaptive routing?
- Performance/power mode?
- Side panel và overlay có budget khác nhau không?
- Lease timeout policy?
- Publication target?
- Long-session benchmark duration?

---

## 60. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem, Attempt, authority, publication |
| `SCHEDULER.md` | Admission and pressure response |
| `WORK_QUEUE.md` | Queue timing |
| `CANCELLATION.md` | Cancellation efficiency |
| `RETRY_POLICY.md` | Recovery latency |
| `ERROR_MODEL.md` | Failure performance |
| `CACHE_POLICY.md` | Reuse value |
| `MEMORY_MODEL.md` | Resource pressure and budgets |
| `RESOURCE_LIFECYCLE.md` | Ownership, Lease and disposal |
| `THREADING_MODEL.md` | Context utilization |
| `RUNTIME_CONFIG.md` | Performance limits |
| `RUNTIME_OBSERVABILITY.md` | Metrics, logs and traces |
| `BOOT_SEQUENCE.md` | Cold start and shutdown |
| `BUSINESS_PIPELINE_ORCHESTRATION.md` | Critical business work |

---

## 61. Completion Criteria

`PERFORMANCE_MODEL.md` được xem là đồng bộ khi:

- Stage-centric vocabulary được thay bằng WorkItem/Attempt/Artifact;
- useful-result latency là metric chính;
- authority, ownership transfer và publication có metric riêng;
- Lease và Resource Lifecycle performance được đo;
- useful work phân tách executed/accepted/published/committed;
- Resource Pressure unified;
- cache đo reuse value, không chỉ hit rate;
- benchmarks có authority/publication/lease/lifecycle;
- MVP concurrency không hard-code OCR/Layout;
- events, metrics, invariants và tests khớp Runtime v2.

---

## 62. Summary

CRAI đánh giá hiệu năng bằng output hiện tại có ích:

```text
Stable Current Content
    ↓
Bounded Runtime Work
    ↓
Accepted Candidate
    ↓
Published Artifact
    ↓
Current Visible Result
```

Các ưu tiên chính:

```text
Responsiveness
+
Freshness
+
Useful-Result Latency
+
Bounded Resource Usage
+
Predictable Recovery
+
Quality Preservation
```

Runtime không được coi là nhanh nếu chỉ hoàn thành nhiều work không còn authority hoặc không bao giờ được người dùng nhìn thấy.
