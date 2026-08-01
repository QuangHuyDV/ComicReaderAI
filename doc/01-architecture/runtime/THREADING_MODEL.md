# runtime/THREADING_MODEL.md

# Runtime Threading Model

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI phân phối WorkItem và Attempt qua các logical execution context, worker, asynchronous operation và optional isolated process mà vẫn giữ UI responsiveness, authority correctness và bounded resource usage.

`Execution Context` là abstraction logic.

```text
Execution Context
    ≠ OS Thread
    ≠ Thread Pool Worker
    ≠ Coroutine
    ≠ Task
    ≠ Process
```

Implementation có thể map một hoặc nhiều logical context lên physical thread, event loop, task scheduler, GPU queue hoặc process khi semantics vẫn được giữ.

---

## 2. Scope

Tài liệu này bao phủ:

- logical execution context;
- context ownership và lifecycle;
- UI boundary;
- Runtime Control context;
- Scheduler execution;
- capture/observation context;
- execution resource pool;
- CPU/GPU/native/remote execution;
- provider callbacks;
- thread/process affinity;
- immutable transfer;
- Resource Lease;
- synchronization;
- event dispatch;
- cancellation;
- shutdown;
- process isolation;
- metrics;
- MVP policy.

Không định nghĩa:

- exact thread API;
- framework dispatcher syntax;
- Scheduler priority policy;
- memory budget values;
- provider SDK internals;
- Business Module algorithm;
- process topology cuối cùng.

---

## 3. Core Principles

1. Execution Context là logical abstraction.
2. Physical thread là implementation detail.
3. Core Runtime state có một logical writer.
4. Runtime Control sở hữu execution authority.
5. Worker không mutate Runtime state trực tiếp.
6. Worker không sở hữu shared payload.
7. Shared payload immutable sau publication.
8. Resource Lease không cấp ownership.
9. Provider callback không cấp authority.
10. Physical completion order không quyết định logical acceptance.
11. UI context không chạy heavy domain work.
12. Control path không cạnh tranh trực tiếp với heavy work.
13. CPU, GPU, provider và process concurrency luôn bounded.
14. Blocking wait bị cấm trên UI và Runtime Control context.
15. Event subscriber chạy trên declared context.
16. Cancellation revoke authority trước physical drain.
17. Shutdown dừng admission trước cleanup.
18. Thread affinity phải được khai báo.
19. Publication phải atomic.
20. Threading correctness không phụ thuộc cache availability.

---

## 4. Architectural Position

```text
Runtime Control
    ↓ creates eligible WorkItem / Attempt
Scheduler
    ↓ admits
Execution Resource Pool
    ↓ dispatches
Worker Execution
    ↓ invokes Business Module or Provider Adapter
Candidate Completion
    ↓
Runtime Control
    ↓ validates authority
Artifact Store
    ↓ accepts ownership and publishes
Presentation
    ↓ commits on UI Context
```

Threading Model không sở hữu business workflow hoặc terminal outcome.

---

## 5. Execution Contexts

CRAI định nghĩa các logical context:

```text
Application Runtime
├── UI Execution Context
├── Runtime Control Context
├── Capture Context
├── Observation Context
├── CPU Execution Pool
├── Provider I/O Context
├── Optional GPU Context
├── Maintenance Context
└── Optional Isolated Process Context
```

Một implementation có thể merge context khi:

- affinity không bị phá;
- control path vẫn được bảo vệ;
- concurrency vẫn bounded;
- observability vẫn phân biệt được logical context.

---

## 6. Execution Context Ownership

| Execution Context | Logical owner |
|---|---|
| UI Context | Presentation boundary |
| Runtime Control Context | Runtime Control |
| Capture Context | Capture Module runtime adapter |
| Observation Context | Capture/Recognition observation adapter |
| CPU Execution Pool | Worker Execution manager |
| Provider I/O Context | Provider Manager / Adapter |
| GPU Context | Provider Manager hoặc GPU coordinator |
| Maintenance Context | Runtime infrastructure |
| Isolated Process Context | Process supervisor |

Ownership phải rõ để lifecycle và shutdown không mơ hồ.

---

## 7. Execution Context Lifecycle

```text
CREATED
  ↓
INITIALIZED
  ↓
RUNNING
  ↔
PAUSED
  ↓
DRAINING
  ↓
STOPPED
  ↓
DISPOSED
```

Không phải context nào cũng hỗ trợ `PAUSED`.

Context disposal phải tôn trọng active Attempt, Lease và thread/process affinity.

---

## 8. UI Execution Context

UI Context là context duy nhất được mutate UI state.

Nó sở hữu:

- UI control state;
- presentation commit;
- loading/error display;
- capture-region selection UI;
- framework-bound visual resource;
- UI-local lifecycle.

UI Context không được:

- chạy Recognition/Translation;
- gọi provider đồng bộ;
- block chờ Runtime;
- thực hiện large image processing;
- giữ Runtime lock;
- dispose shared Artifact;
- mutate Runtime Control state.

---

## 9. UI Command Flow

```text
User Action
    ↓
UI validates local input
    ↓
UI submits Runtime Command
    ↓
UI updates immediate interaction state
    ↓
Runtime handles asynchronously
    ↓
Validated result/event returned
    ↓
UI applies update on UI Context
```

UI không chờ toàn pipeline hoàn tất đồng bộ.

---

## 10. UI Commit Boundary

```text
Presentation Candidate Ready
        ↓
Runtime Control validates authority
        ↓
Commit request dispatched to UI Context
        ↓
UI Context validates again
        ↓
Atomic Presentation replacement
```

Second validation bắt buộc vì active Revision có thể thay đổi trong lúc UI queue chờ.

---

## 11. Runtime Control Context

Runtime Control Context là authority owner cho mutable Runtime state:

- Session runtime metadata;
- current Revision;
- WorkItem logical state;
- Attempt lineage;
- cancellation authority;
- accepted terminal outcome;
- commit authority;
- shutdown state.

Context này phải:

- nhanh;
- serialized;
- deterministic;
- non-blocking;
- không chạy heavy domain work.

---

## 12. Single Logical Writer

Core mutable Runtime state có một logical writer.

Các context khác:

- đọc immutable snapshot;
- gửi command;
- gửi Completion;
- không mutate shared Runtime object trực tiếp.

Single writer không bắt buộc một dedicated OS thread.

---

## 13. Runtime Command Queue

Conceptual commands:

```text
START_SESSION
STOP_SESSION
UPDATE_SOURCE
SUBMIT_OBSERVATION
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
PROVIDER_HEALTH_CHANGED
RESOURCE_PRESSURE_CHANGED
REQUEST_PRESENTATION_COMMIT
```

Command chỉ chứa lightweight metadata và ArtifactRef.

---

## 14. Runtime Control Restrictions

Runtime Control không:

- chạy provider call;
- chờ Worker;
- chờ UI dispatch;
- xử lý full image;
- build large presentation;
- block cancellation cleanup;
- hold broad lock;
- perform durable I/O synchronously.

---

## 15. Scheduler Execution Context

MVP có thể chạy Scheduler trong Runtime Control Context nếu scheduling decision ngắn và bounded.

Scheduler có thể tách context riêng khi:

- candidate set lớn;
- multi-session complexity tăng;
- contention đo được;
- policy computation trở nên đắt.

Scheduler context tách riêng vẫn không được mutate authority trực tiếp.

---

## 16. WorkItem Execution Mapping

WorkItem không “chạy trên thread” theo nghĩa kiến trúc.

```text
WorkItem
    ↓
Scheduler Admission
    ↓
Execution Context Selection
    ↓
Worker Assignment
    ↓
Physical Thread / Task / Event Loop / Process
```

Runtime contract không phụ thuộc physical mapping.

---

## 17. Execution Resource Pool

`Execution Resource Pool` là abstraction chung.

Có thể gồm:

```text
CPU Worker Pool
GPU Queue
Dedicated Native Thread
Provider Async Capacity
Process Pool
```

Pool phải:

- bounded;
- capability-aware;
- pressure-aware;
- observable;
- cancelable khi platform hỗ trợ.

---

## 18. Capture Context

Capture có thể cần context riêng vì:

- OS callback;
- thread affinity;
- GPU-backed surface;
- timing sensitivity;
- source-specific lifecycle.

Capture không phụ thuộc Completion của Recognition hoặc Translation.

---

## 19. Capture Backpressure

Capture không queue mọi frame.

```text
Frame A pending
Frame B arrives → replaces A
Frame C arrives → replaces B
```

Latest-value behavior được ưu tiên cho observation input.

---

## 20. Observation Context

Observation có thể gồm:

- change detection;
- stability detection;
- fingerprint preparation;
- candidate Revision input.

Mỗi capture source nên có serial observation semantics.

Intermediate input có thể replace, nhưng ordering decision không được chạy song song không kiểm soát.

---

## 21. CPU-Bound Work

CPU-heavy execution phải ngoài UI và Runtime Control contexts.

Ví dụ tổng quát:

- image processing;
- local model inference;
- normalization;
- structured document processing;
- presentation model construction.

Concurrency không hard-code theo OCR/Layout stage.

---

## 22. CPU Execution Pool

CPU Pool phải bounded để tránh:

- thread explosion;
- oversubscription;
- temporary-memory spike;
- control-path starvation;
- excessive context switching.

MVP bắt đầu với low concurrency.

---

## 23. Capability Limits

Pool capacity và capability concurrency là hai khái niệm riêng.

```text
CPU Pool Capacity = N
WorkType concurrency limits = bounded per policy
```

Scheduler thực thi capability-aware admission.

---

## 24. Work Granularity

Work phải:

- đủ lớn để scheduling overhead thấp;
- đủ nhỏ để cancellation và fairness hiệu quả;
- không tạo một task cho mỗi ký tự;
- không tạo một uninterruptible task cho toàn bộ document.

Granularity cụ thể thuộc Business Module và WorkType contract.

---

## 25. I/O-Bound Work

I/O nên dùng async API khi có:

- remote provider;
- Storage;
- telemetry;
- document loading;
- provider authentication.

Async không đồng nghĩa unlimited.

Concurrency, socket, billing và callback đều phải bounded.

---

## 26. Provider I/O Context

```text
WorkItem
    ↓
Provider Adapter
    ↓
Asynchronous Request
    ↓
Callback
    ↓
Normalize Result
    ↓
Runtime Command
```

Callback không mutate Runtime state.

Callback không grant authority.

Callback không giữ Runtime implementation object lâu dài.

---

## 27. Provider Callback Boundary

Provider callback phải:

1. capture minimal request identity;
2. normalize provider output/error;
3. release request-local resource khi phù hợp;
4. submit Completion;
5. không advance downstream work;
6. không update UI;
7. không tự retry.

---

## 28. Provider Execution Declaration

Provider phải khai báo:

```text
ExecutionClass
Affinity
MaximumConcurrency
CancellationSupport
ProcessIsolation
MemoryCostHint
GpuCostHint
BlockingBehavior
```

Possible `ExecutionClass`:

```text
CPU
GPU
REMOTE_IO
NATIVE_SERIAL
PROCESS
HYBRID
```

---

## 29. Execution Affinity

`ExecutionAffinity` tổng quát hơn thread affinity.

Có thể là:

```text
ANY
SPECIFIC_THREAD
SPECIFIC_EVENT_LOOP
SPECIFIC_PROCESS
SPECIFIC_GPU_QUEUE
SERIAL_CONTEXT
UI_CONTEXT
```

Affinity phải explicit trước execution.

---

## 30. GPU Context

GPU execution có thể cần:

- dedicated command queue;
- serial model inference;
- explicit synchronization;
- GPU memory budget;
- UI rendering protection.

Parallel GPU execution không mặc định tốt hơn.

---

## 31. Process Isolation

Isolated process phù hợp cho:

- unstable native library;
- large model;
- hard-cancel requirement;
- third-party plugin;
- high-memory import.

Process chỉ giao tiếp qua adapter/command contract.

Nó không mutate Runtime state trực tiếp.

---

## 32. Cross-Process Payload

Large payload không serialize lặp lại.

Possible mechanism:

- shared memory;
- memory-mapped file;
- temporary file;
- shared surface;
- Artifact handle.

Process topology cụ thể thuộc tài liệu riêng.

---

## 33. Worker Ownership

Worker sở hữu:

- Attempt-local resource;
- acquired Resource Lease;
- provider request handle;
- candidate output trước transfer.

Worker không sở hữu:

- shared payload;
- Session/Revision authority;
- Cache Policy;
- downstream scheduling;
- UI state;
- accepted Artifact sau ownership transfer.

---

## 34. Worker Execution Contract

```text
Receive immutable Attempt input
    ↓
Validate CancellationContext
    ↓
Acquire Resource Leases
    ↓
Validate inputs
    ↓
Execute bounded operation
    ↓
Check cancellation
    ↓
Create Candidate Artifact
    ↓
Submit Completion
    ↓
Release local resources and leases
```

Runtime Control quyết định acceptance.

---

## 35. Candidate Publication Flow

```text
Worker Completion
    ↓
Candidate Artifact
    ↓
Runtime Control authority validation
    ↓
Artifact Store ownership transfer
    ↓
Atomic publication
```

Publication không xảy ra trực tiếp từ Worker.

---

## 36. Shared Mutable State

Forbidden by default:

- multiple workers mutate one payload;
- callback mutates Session;
- UI và Runtime share mutable collection;
- Worker changes Queue priority;
- Worker updates Revision graph.

Mutable builder chỉ local cho một Worker cho đến finalize.

---

## 37. Cross-Context Data

Data crossing context phải immutable hoặc treated-as-immutable:

```text
RuntimeCommand
WorkItem
AttemptInput
ArtifactRef
Completion
RuntimeEvent
PresentationModel
```

---

## 38. Synchronization Strategy

Ưu tiên:

1. explicit ownership;
2. single logical writer;
3. serialized command processing;
4. immutable data;
5. Resource Lease;
6. bounded concurrent structure;
7. narrow lock;
8. broader locking chỉ khi bất khả kháng.

---

## 39. Lock Rules

Lock nếu cần phải:

- scope nhỏ;
- hold ngắn;
- không qua provider call;
- không qua UI dispatch;
- không qua heavy processing;
- không qua shutdown wait;
- tránh nested locks.

Kiến trúc ưu tiên tránh lock ordering phức tạp.

---

## 40. Blocking Policy

Blocking bị cấm trong:

- UI Context;
- Runtime Control Context;
- capture callback;
- provider callback;
- Event Bus dispatch loop.

Dedicated Worker có thể block nếu:

- API inherently synchronous;
- capacity được accounting;
- timeout bounded;
- cancellation policy rõ;
- control/UI không bị ảnh hưởng.

---

## 41. Sync-over-Async

Không dùng pattern tương đương:

```text
Async().Wait()
Async().Result
```

trên UI/Control path.

Async call chain nên giữ async end-to-end khi thực tế cho phép.

---

## 42. Event Dispatch

```text
Publisher
    ↓
Event Dispatcher
    ↓
Declared Execution Context
    ↓
Subscriber
```

Publisher không execute arbitrary subscriber logic đồng bộ.

Subscriber muốn thay Runtime state phải gửi Command.

---

## 43. Event Ordering

Ordering chỉ guarantee theo scope explicit.

Per-session serialized stream có thể giữ causal order.

Không guarantee total ordering toàn hệ thống.

Consumer phải dùng identity và timestamp.

---

## 44. Event Reentrancy

State transition phải hoàn tất trước khi event-triggered command được xử lý.

```text
Transition completes
    ↓
Event queued
    ↓
Subscriber runs
    ↓
New command queued
```

---

## 45. Cancellation and Contexts

```text
Cancellation Request
    ↓
Runtime Control revokes authority
    ↓
Queued work removed
    ↓
Running context signaled
    ↓
Attempt drains or becomes abandoned
    ↓
Resources released
```

Caller không block vô hạn chờ physical stop.

---

## 46. Cancellation Checkpoints

Generic checkpoints:

- before expensive acquisition;
- before heavy execution;
- between bounded batches;
- after external call;
- before Candidate creation;
- before Completion;
- before UI dispatch;
- before UI commit.

---

## 47. Timers

Timer callback phải ngắn.

Dùng cho:

- capture pacing;
- stability timing;
- provider timeout;
- retry delay;
- idle unload;
- metrics sampling.

Timer callback gửi Command, không chạy full work.

---

## 48. Retry Delay

Delayed retry không giữ blocked thread.

```text
Cancelable delay registered
    ↓
Delay completes
    ↓
Authority revalidated
    ↓
Retry-ready command submitted
```

---

## 49. Control Path Protection

Control path phải luôn có execution capacity cho:

- cancellation;
- stop;
- Revision replacement;
- Completion processing;
- provider timeout;
- shutdown;
- UI state signal.

Heavy work không được chiếm toàn bộ capacity mà control path cần.

---

## 50. Workload Classes

```text
CONTROL
UI
CAPTURE
OBSERVATION
CPU_LIGHT
CPU_HEAVY
NETWORK_IO
GPU
NATIVE_SERIAL
PROCESS
MAINTENANCE
```

Scheduler dùng class cho admission và capacity, không hard-code Business Module internals.

---

## 51. Background Work

Background work phải:

- low concurrency;
- yield to interactive;
- stop dưới pressure;
- cancelable;
- không giữ UI-affine resource;
- không block shutdown.

---

## 52. Artifact Store Concurrency

Artifact Store phải:

- thread-safe;
- ownership-safe;
- lease-safe;
- publication-safe;
- disposal-safe.

Payload immutable sau publication.

Consumers chỉ thấy:

```text
not available
```

hoặc:

```text
complete accepted Artifact
```

---

## 53. Revision Registry Concurrency

Revision Registry có một logical writer qua Runtime Control.

Worker chỉ yêu cầu snapshot/Lease.

Worker không mutate Revision relation trực tiếp.

---

## 54. Atomic Presentation Replacement

Presentation commit phải atomic từ góc nhìn UI.

Progressive rendering nếu có cần consistency model riêng.

---

## 55. Race Prevention

Các race chính:

- Revision superseded khi Attempt hoàn tất;
- cache eviction khi Lease active;
- UI closed khi commit queued;
- provider switched khi callback tới;
- Session close khi Capture callback chạy;
- logical disposal khi native use chưa xong.

Giải pháp:

- authority validation;
- immutable payload;
- Lease;
- serialized Runtime Control;
- context-affinity;
- atomic publication.

---

## 56. Deadlock Prevention

1. UI không chờ Runtime đồng bộ.
2. Runtime Control không chờ UI.
3. Không hold lock qua provider.
4. Không hold Artifact Store lock chờ Worker.
5. Không nested lock nếu tránh được.
6. External operation có timeout.
7. Shutdown async và bounded.
8. Event subscriber không reenter transition trực tiếp.

---

## 57. Error Across Contexts

Worker normalize execution result thành Completion:

```text
ATTEMPT_COMPLETED
ATTEMPT_FAILED
ATTEMPT_CANCELED
ATTEMPT_ABANDONED
```

Completion chứa:

- SessionId;
- RevisionId;
- WorkItemId;
- AttemptId;
- BusinessStageId;
- WorkType;
- RuntimeErrorRef;
- timing metadata.

Runtime Control quyết định accepted outcome.

---

## 58. Unhandled Worker Failure

Unhandled failure phải:

- release Attempt-local resource;
- release Lease;
- notify Runtime Control;
- mark Worker/provider health khi cần;
- không crash toàn app nếu isolation vẫn an toàn;
- trigger process isolation consideration nếu native failure lặp.

---

## 59. Shutdown

```text
Stop New Admission
    ↓
Revoke Authority
    ↓
Cancel Queued Work
    ↓
Signal Running Contexts
    ↓
Bounded Drain
    ↓
Mark Remaining Attempts Abandoned
    ↓
Release Leases
    ↓
Dispose Contexts and Resources
```

Order chi tiết thuộc `BOOT_SEQUENCE.md`.

---

## 60. Structured Concurrency

Logical scope:

```text
Session
    ↓
Revision
    ↓
WorkItem
    ↓
Attempt
```

Child execution phải gắn owner scope.

Khi parent kết thúc:

- child nhận cancellation;
- child không được sống không tracking;
- non-cancelable child thành abandoned.

Architecture không bắt buộc framework cụ thể.

---

## 61. Metrics

Theo dõi:

- UI dispatch delay;
- Runtime command queue length;
- Runtime Control processing delay;
- control-path delay;
- capture callback delay;
- observation latency;
- execution context utilization;
- Worker utilization;
- CPU/GPU saturation;
- provider in-flight;
- queue wait;
- Lease wait;
- publication delay;
- authority validation delay;
- cancellation acknowledgment;
- event dispatch delay;
- blocked Worker count;
- active thread count khi đo được;
- process restart count.

---

## 62. MVP Execution Contexts

```text
1 UI Context
1 Runtime Control Context
1 Capture/Observation Serial Context
1 Bounded CPU Execution Pool
Asynchronous Provider I/O
Optional serial GPU/native context
```

Capture và Observation có thể merge trong MVP nếu lightweight.

---

## 63. MVP Rules

1. UI chỉ mutate UI.
2. Heavy work không chạy trên UI.
3. Runtime state có one logical writer.
4. Workers nhận immutable input.
5. Workers trả Completion và Candidate output.
6. Workers không schedule downstream work.
7. Provider callback chỉ gửi Completion.
8. Concurrency bounded theo workload class.
9. Capture latest-value.
10. UI commit validation hai lần.
11. Cancellation không block caller vô hạn.
12. Không unbounded thread/task creation.
13. Worker không sở hữu shared payload.
14. Resource Lease dùng cho shared resource.
15. Shutdown dừng admission trước drain.

---

## 64. Example: Generic Execution

```text
Runtime Control creates WorkItem
        ↓
Scheduler admits
        ↓
Execution Context selected
        ↓
Worker executes Attempt
        ↓
Candidate Artifact produced
        ↓
Completion submitted
        ↓
Runtime Control validates
        ↓
Artifact Store accepts ownership
        ↓
Artifact published
```

---

## 65. Example: Late Provider Callback

```text
Provider callback arrives
        ↓
Adapter normalizes result
        ↓
Completion submitted
        ↓
Runtime Control detects stale authority
        ↓
Result rejected
```

Callback never updates UI or Runtime state directly.

---

## 66. Example: UI Closed Before Commit

```text
Commit queued to UI
    ↓
Session closes
    ↓
UI callback executes
    ↓
Second authority validation fails
    ↓
Presentation candidate released
```

---

## 67. Example: Cache Eviction During Read

```text
Worker holds Artifact Lease
    ↓
Cache retention removed
    ↓
Payload remains
    ↓
Worker releases Lease
    ↓
Physical disposal becomes eligible
```

---

## 68. Example: Native Serial Provider

```text
WorkItem admitted
    ↓
Dedicated serial context
    ↓
Synchronous native call
    ↓
Completion normalized
    ↓
Runtime Command
```

No Runtime lock is held during native execution.

---

## 69. Architecture Invariants

1. Execution Context là logical.
2. Physical thread là implementation detail.
3. Context ownership explicit.
4. Context lifecycle bounded.
5. UI only mutates UI.
6. Runtime Control owns authority.
7. Core Runtime state has one logical writer.
8. Heavy work never blocks Runtime Control.
9. Control path always protected.
10. Worker never owns shared payload.
11. Worker owns only Attempt-local resource and Lease.
12. Lease never grants ownership.
13. Provider callback never grants authority.
14. Callback never mutates Runtime directly.
15. Publication never transfers ownership implicitly.
16. Ownership transfer explicit before accepted publication.
17. Physical completion order never determines logical outcome.
18. Execution order never implies commit order.
19. Shared payload immutable.
20. CPU/GPU/provider/process concurrency bounded.
21. Thread/process affinity explicit.
22. Locks not held across external or heavy work.
23. Event subscriber does not block publisher indefinitely.
24. Subscriber changes Runtime only through Command.
25. UI commit revalidates authority.
26. Cancellation revoke authority before drain.
27. Delayed retry uses non-blocking timer.
28. Shutdown stops admission before cleanup.
29. Artifact Store is ownership/lease/publication safe.
30. Threading correctness independent of cache.

---

## 70. Testing Requirements

Test phải bao phủ:

- UI responsiveness during slow execution;
- control commands under saturation;
- callback out-of-order;
- Revision switch during UI dispatch;
- Session close during Capture callback;
- eviction with active Lease;
- cancellation during CPU/GPU/provider work;
- Worker failure before publication;
- duplicate Completion;
- event subscriber queues Command;
- shutdown during active work;
- affinity enforcement;
- no unbounded task creation;
- context drain;
- process callback isolation;
- authority rejection after late completion;
- ownership transfer before publication.

---

## 71. Open Questions

- Desktop framework và UI dispatcher nào?
- Capture API có affinity gì?
- Capture/Observation có merge được không?
- Provider nào sync/native/GPU?
- Default CPU pool size?
- GPU coordinator có cần trong MVP không?
- Local model chạy thread hay process?
- Event Bus đảm bảo ordering theo scope thế nào?
- Structured concurrency support của stack?
- Presentation build chạy Worker hay UI một phần?
- Context lifecycle registry nằm ở đâu?

---

## 72. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem, Attempt, Completion, authority |
| `RUNTIME_COMPONENTS.md` | Runtime Control and Worker ownership |
| `SCHEDULER.md` | Admission and workload class |
| `WORK_QUEUE.md` | Dispatch boundary |
| `CANCELLATION.md` | Authority revocation and cooperative stop |
| `RETRY_POLICY.md` | Delayed retry and new Attempt |
| `MEMORY_MODEL.md` | Resource Lease and budgets |
| `RESOURCE_LIFECYCLE.md` | Ownership transfer and disposal |
| `CACHE_POLICY.md` | Retention and active Lease |
| `ERROR_MODEL.md` | Completion error normalization |
| `PERFORMANCE_MODEL.md` | Context saturation and latency |
| `RUNTIME_CONFIG.md` | Pool and concurrency limits |
| `RUNTIME_OBSERVABILITY.md` | Execution metrics |
| `BOOT_SEQUENCE.md` | Context startup/shutdown |
| `../core/EVENT_BUS.md` | Event dispatch semantics |

---

## 73. Completion Criteria

`THREADING_MODEL.md` được xem là đồng bộ khi:

- Execution Context được tách physical thread;
- context ownership và lifecycle rõ;
- Runtime Control là authority owner;
- Worker không sở hữu shared payload;
- Resource Lease thay Artifact-only view;
- Candidate → validation → transfer → publication rõ;
- Provider callback không mutate/grant authority;
- Execution Affinity tổng quát;
- Event subscriber chạy trên declared context;
- control path protected;
- shutdown/cancellation khớp Runtime v2;
- Stage-specific vocabulary bị loại khỏi architecture core;
- invariants và testing đầy đủ.

---

## 74. Summary

CRAI sử dụng một số ít logical execution context rõ ràng:

```text
UI Context
+
Serialized Runtime Control
+
Responsive Capture/Observation
+
Bounded Execution Resource Pools
+
Bounded Provider I/O
+
Immutable Artifact Exchange
```

Ranh giới cốt lõi:

```text
Execution Context defines where work may run.

Runtime Control defines whether work still matters.

Worker performs physical execution.

Artifact Store owns accepted shared payload.

UI commits only after authority validation.
```
