# runtime/MEMORY_MODEL.md

# Runtime Memory Model

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime quản lý ownership, lifetime, retention, memory usage và physical disposal của runtime resources.

Memory là một phần của resource lifecycle, không phải toàn bộ resource model.

Runtime resource có thể gồm:

```text
Memory Resource
Artifact Resource
Native Resource
GPU Resource
Provider Resource
Operating-System Handle
Temporary File or Mapping
```

Tài liệu này tập trung vào:

- bounded runtime memory;
- immutable Artifact;
- lightweight reference;
- explicit ownership;
- scoped lease;
- logical disposal;
- physical disposal;
- memory/resource pressure;
- attempt-local và shared resource;
- deterministic-enough cleanup;
- privacy-safe retention.

---

## 2. Core Philosophy

CRAI tuân theo nguyên tắc:

```text
Resource ownership determines lifetime.

Memory usage is a consequence of lifetime.
```

Mọi large resource phải có:

- logical owner;
- retention class;
- lifetime boundary;
- size or cost estimate;
- release condition;
- disposal owner;
- observability metadata.

Large payload phải được chia sẻ qua immutable reference hoặc scoped lease thay vì copy qua queue, event hoặc component boundary.

---

## 3. Architectural Position

```text
Session
  ↓
Revision
  ↓
WorkItem
  ↓
Attempt
  ↓
Runtime Resources
    ├── Attempt-local Resource
    ├── Shared Artifact
    ├── Provider Resource
    ├── Native/GPU Resource
    └── UI Resource
```

Ownership và lifetime phải được tách khỏi business meaning.

Business Module sở hữu semantic meaning của data.

Runtime sở hữu execution-time resource lifecycle.

Storage sở hữu durable persistence capability, không phải runtime memory ownership.

---

## 4. Scope

Tài liệu này bao phủ:

- runtime resource categories;
- session/revision/attempt lifetime;
- Revision Registry;
- Artifact Store;
- Resource Lease;
- retention tracking;
- ownership transfer;
- logical và physical disposal;
- memory budget;
- GPU/native resource;
- provider lifetime resource;
- queue payload policy;
- cache retention;
- memory pressure;
- resource leak;
- metrics và diagnostics;
- MVP policy.

Không định nghĩa:

- persistent schema;
- disk cache format;
- provider SDK internals;
- GC implementation;
- exact image-processing algorithm;
- process topology;
- Storage migration/retention policy.

---

## 5. Runtime Resource Categories

Runtime resource được chia theo lifetime và sharing semantics.

### 5.1 Attempt-Local Resource

Chỉ tồn tại trong một Attempt.

Ví dụ:

- temporary buffer;
- request builder;
- image tile;
- provider response body;
- intermediate tensor;
- temporary geometry graph.

### 5.2 Shared Runtime Artifact

Immutable data được nhiều component hoặc Attempt tham chiếu.

Ví dụ:

- Source Image Artifact;
- Recognition Artifact;
- Source Document Artifact;
- Translation Artifact;
- Presentation Artifact.

### 5.3 Session-Scoped Resource

Tồn tại trong một active session.

Ví dụ:

- session configuration snapshot reference;
- current presentation reference;
- session glossary index;
- source observation state.

### 5.4 Runtime-Global Resource

Tồn tại theo application/runtime lifetime.

Ví dụ:

- provider client;
- reusable worker pool;
- configuration snapshot registry;
- shared model handle;
- bounded buffer pool.

### 5.5 External Resource

Do external system hoặc OS giữ.

Ví dụ:

- HTTP request;
- process;
- window capture handle;
- GPU context;
- file mapping;
- native model context.

---

## 6. Resource Classification Table

| Resource | Default owner | Lifetime | Shared |
|---|---|---|---|
| Candidate Artifact | Producer Attempt | Attempt until transfer | No |
| Accepted Artifact | Artifact Store | Retention/Lease governed | Yes |
| Temporary Buffer | Worker Execution | Attempt | No |
| Queue Metadata | Work Queue | Queued position | No |
| Revision Metadata | Revision Store | Revision | Yes |
| Provider Request | Provider Adapter | Attempt/request | No |
| Provider Client | Provider Manager | Provider/runtime | Yes |
| GPU Context | Provider Manager or Resource Manager | Provider/runtime | Maybe |
| Artifact Lease | Lease holder | Scoped operation | No |
| UI Presentation Reference | Presentation | Display lifetime | Yes |
| Persistent Snapshot | Storage | Durable policy | Yes, outside runtime memory |

---

## 7. Control Memory

Control memory phải nhỏ và bounded.

Có thể chứa:

- SessionId;
- RevisionId;
- WorkItemId;
- AttemptId;
- CancellationContextRef;
- scheduler metadata;
- queue metadata;
- state snapshots;
- event envelope;
- metrics counter.

Không chứa:

- screenshot;
- large text payload;
- model tensor;
- provider body;
- secret;
- complete Artifact payload.

---

## 8. Session Memory

Session memory chứa runtime metadata cần để vận hành một session:

- current RevisionRef;
- source/capture descriptor;
- configuration snapshot reference;
- active presentation reference;
- session cancellation context;
- session-scoped retained ArtifactRef;
- provider/runtime preference reference.

Reading Module vẫn sở hữu business state của Reading Session.

Session close revoke runtime ownership, nhưng physical disposal chờ active lease và drain.

---

## 9. Revision Memory

Revision là runtime ownership boundary cho execution intent hiện tại.

Conceptual model:

```text
Revision
├── Revision Metadata
├── BusinessPlanRef
├── Input ArtifactRefs
├── Accepted Output ArtifactRefs
├── WorkItemRefs
├── Resource Accounting
└── Drain State
```

Revision không chứa mutable large payload trực tiếp.

---

## 10. Revision Lifetime

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

Khi superseded:

- commit authority mất ngay;
- new lease có thể bị từ chối theo policy;
- queued work bị remove;
- running Attempt được drain/cancel;
- physical payload chưa chắc được free ngay.

---

## 11. Runtime Revision Registry

`Revision Store` được hiểu là Runtime Revision Registry.

Nó quản lý:

- Revision identity;
- current/superseded state;
- Revision-to-WorkItem relation;
- Revision-to-ArtifactRef relation;
- resource accounting metadata;
- disposal eligibility.

Nó không phải durable Storage.

Nó không sở hữu Artifact payload vật lý.

---

## 12. Artifact Store

Artifact Store quản lý immutable runtime Artifact.

Trách nhiệm:

- register candidate;
- atomic publication;
- Artifact identity;
- metadata;
- retention owner;
- lease tracking;
- size estimate;
- lookup;
- cache retention;
- disposal eligibility;
- backing resource reference.

Artifact Store không quyết định scheduling hoặc business semantics.

---

## 13. Artifact Model

```text
Artifact
├── ArtifactId
├── ArtifactType
├── ContentIdentity
├── ProducerWorkItemId
├── ProducerAttemptId
├── ProducerVersion
├── CreatedAt
├── SizeEstimate
├── RetentionClass
├── RetentionOwners
├── BackingResourceRef
└── IntegrityMetadata
```

Exact structure là implementation-specific.

---

## 14. Artifact Immutability

Artifact đã publish không được mutate.

Thay đổi tạo Artifact mới.

```text
Artifact v1
    ↓ correction or transformation
Artifact v2
```

Immutability hỗ trợ:

- safe sharing;
- deterministic cache key;
- stale validation;
- reduced locking;
- retry isolation;
- traceability.

---

## 15. Artifact Identity vs Physical Payload

Nhiều Revision có thể tham chiếu cùng Artifact.

Nhiều Artifact metadata cũng có thể dùng chung backing payload nếu identity và compatibility cho phép.

```text
Revision A ─┐
            ├── ArtifactRef X → one physical payload
Revision B ─┘
```

Architecture không bắt buộc payload duplication.

---

## 16. Lightweight WorkItem Reference

WorkItem và Queue chỉ mang metadata nhẹ:

```text
SessionId
RevisionId
WorkItemId
AttemptId
BusinessStageId
WorkType
InputArtifactRefs
RequestedOutputType
ConfigurationVersion
ExecutionContextRef
CancellationContextRef
```

Không mang:

- full image;
- full Source Document;
- provider response;
- mutable business object;
- secret.

---

## 17. Ownership Model

Mọi resource phải có một logical owner tại mỗi thời điểm.

Owner có thể là:

- Application Bootstrap;
- Runtime Component;
- Session Runtime;
- Revision Registry;
- Artifact Store;
- Worker Execution;
- Provider Manager;
- Provider Adapter;
- Presentation;
- Resource Manager;
- Cache retention policy;
- Storage boundary.

Ownership transfer phải explicit.

---

## 18. Ownership Transfer

Ví dụ:

```text
Worker creates temporary output
        ↓
Candidate Artifact registered
        ↓
Runtime Control validates authority
        ↓
Artifact Store accepts ownership
        ↓
Worker releases producer ownership
```

Sau transfer, producer không được dispose backing payload độc lập.

---

## 19. Retention Tracking

Architecture yêu cầu retention explicit, nhưng không bắt buộc reference counting.

Implementation có thể dùng:

- reference count;
- lease table;
- owner set;
- pin count;
- generation token;
- managed reference;
- explicit handle.

Yêu cầu duy nhất:

```text
A resource cannot be physically disposed
while a valid owner or lease still exists.
```

---

## 20. Resource Lease

`ResourceLease` là abstraction tổng quát.

Các dạng có thể gồm:

```text
ArtifactLease
GpuResourceLease
NativeHandleLease
ProviderResourceLease
CaptureSurfaceLease
```

Lease phải:

- scoped;
- immutable/read-only mặc định;
- có owner;
- có acquisition time;
- có release path;
- cancel-safe;
- observable;
- không được giữ vô hạn không phát hiện.

---

## 21. Lease Acquisition

```text
Resolve ResourceRef
    ↓
Validate resource state
    ↓
Acquire Lease
    ↓
Use resource
    ↓
Release Lease
```

Nếu logical disposal đã bắt đầu, new lease có thể bị deny.

---

## 22. Logical Disposal

Logical disposal nghĩa là resource không còn hợp lệ cho new runtime work.

Actions:

- remove active index;
- deny new lease;
- release retention owner;
- revoke commit authority nếu liên quan;
- mark pending physical disposal.

Logical disposal không đồng nghĩa memory đã free.

---

## 23. Physical Disposal

Physical disposal chỉ xảy ra khi:

- no retention owner;
- no active lease;
- no UI ownership;
- no provider/native usage;
- cleanup policy cho phép;
- required diagnostics retention hết.

```text
Logical disposal
    ↓
Drain
    ↓
Lease count = 0
    ↓
Physical disposal
```

---

## 24. Retention Classes

```text
EPHEMERAL
ATTEMPT_SCOPED
REVISION_SCOPED
SESSION_SCOPED
CACHE_ELIGIBLE
APPLICATION_SCOPED
EXTERNAL_LIFETIME
```

Retention class mô tả intended maximum lifetime, không thay ownership.

---

## 25. Attempt-Local Resource

Attempt-local resource:

- do Worker hoặc Provider Adapter sở hữu;
- không publish trước validation;
- release khi Attempt kết thúc;
- không được retained bởi event handler;
- không tự chuyển thành shared Artifact;
- có bounded size/cost.

---

## 26. Shared Artifact Retention

Shared Artifact có thể có nhiều retention owner:

```text
Revision retention
Cache retention
UI retention
Diagnostic retention
```

Physical payload chỉ cần tồn tại một lần nếu implementation hỗ trợ.

---

## 27. Cache Promotion

Cache promotion là thay đổi retention ownership, không phải copy payload.

```text
Revision-scoped Artifact
        ↓ validation
Cache Policy approves
        ↓
Cache retention owner added
        ↓
Payload unchanged
```

Failed, canceled, stale, abandoned hoặc unvalidated output không promote trong MVP.

---

## 28. Cache Eviction

Cache eviction nghĩa là:

```text
Remove cache retention owner
```

Không nghĩa là:

```text
Free payload immediately
```

Payload còn nếu Revision, UI hoặc Lease vẫn giữ.

---

## 29. Artifact Store vs Storage

```text
Artifact Store
    → runtime immutable artifact lifecycle

Storage
    → durable persistence, versioning, retention, recovery
```

Artifact Store không mặc định durable.

Storage không quản lý active lease, queue hoặc revision authority.

---

## 30. Large Payload Policy

Payload được coi là large nếu copy có ảnh hưởng đáng kể đến:

- latency;
- RAM;
- GPU transfer;
- GC/allocation pressure;
- serialization cost;
- IPC cost.

Large payload không được copy mặc định.

---

## 31. Image Resource

Image pipeline có thể tạo nhiều representation:

```text
Capture Surface
CPU Buffer
Preprocessed View
Model Tensor
Preview Surface
```

Mỗi representation phải có:

- owner;
- lifetime;
- size estimate;
- release point;
- sharing policy;
- backing resource type.

---

## 32. Image Copy Policy

Copy chỉ được phép khi cần cho:

- format conversion;
- immutable snapshot;
- process boundary;
- GPU/CPU transfer;
- API ownership contract;
- provider encoding;
- thread-affinity safety.

Copy vì convenience bị tránh.

---

## 33. Frame Retention

MVP chỉ giữ bounded frame set:

- latest observed frame;
- previous comparison frame;
- current stable source Artifact;
- small bounded draining set;
- optional currently displayed previous presentation data.

Không giữ unbounded frame history.

---

## 34. Frame Deduplication

Nếu content identity tương thích:

```text
New frame
    ↓
Fingerprint matches accepted Artifact
    ↓
Reuse ArtifactRef
```

Revision metadata mới vẫn có thể được tạo nếu timeline yêu cầu.

---

## 35. Recognition Resource

Recognition Module có thể dùng:

- preprocessing buffer;
- model tensor;
- region structure;
- provider response;
- normalized recognition Artifact.

Temporary resource phải release sau khi accepted candidate được tạo hoặc Attempt kết thúc.

Runtime không hard-code OCR/Layout internals.

---

## 36. Translation Resource

Translation execution có thể dùng:

- Source Document reference;
- bounded context;
- provider request buffer;
- provider response;
- glossary snapshot reference;
- normalized Translation Artifact.

Raw provider request/response không retained mặc định.

---

## 37. Presentation Resource

Presentation có thể sở hữu:

- Presentation ArtifactRef;
- text/layout model;
- render surface;
- font layout cache;
- UI dispatch handle.

Chỉ current hoặc explicitly retained previous presentation được giữ.

---

## 38. Previous Presentation Retention

UI có thể giữ previous valid presentation đến khi replacement ready.

```text
Old presentation visible
    ↓
New revision processing
    ↓
New presentation committed
    ↓
Old UI retention released
```

UI phải phân biệt displayed revision và processing revision.

---

## 39. Provider Lifetime Resource

Provider Manager có thể sở hữu:

- client;
- connection pool;
- loaded model;
- tokenizer;
- native context;
- GPU context;
- reusable buffer pool.

Đây là provider-lifetime resource, không phải Attempt-local resource.

---

## 40. Provider Request Resource

Per-request resource gồm:

- encoded request;
- prompt/request body;
- response body;
- temporary tensor;
- request handle;
- timeout/cancellation handle.

Phải release sau completion/cancellation/abandonment khi physical execution thực sự cho phép.

---

## 41. Local Model Residency

Possible policy:

```text
ALWAYS_RESIDENT
SESSION_RESIDENT
ON_DEMAND
IDLE_TIMEOUT
```

Provider Manager và Runtime Configuration sở hữu policy.

Memory Model chỉ yêu cầu:

- estimated cost;
- explicit load/unload;
- pressure-aware admission;
- no unsafe speculative load.

---

## 42. GPU Resource

GPU resource quản lý riêng với RAM.

Ví dụ:

- capture surface;
- tensor;
- model allocation;
- UI texture;
- shared graphics handle.

GPU resource cần explicit disposal khi platform yêu cầu.

GC không được coi là guarantee cho timely GPU cleanup.

---

## 43. Native Resource

Native resource có thể gồm:

- image handle;
- OCR/model context;
- window capture handle;
- memory mapping;
- file handle;
- process handle;
- IPC handle.

Managed wrapper phải có explicit lifecycle contract.

---

## 44. Buffer Pooling

Pooling chỉ dùng sau profiling.

Risks:

- oversized retention;
- stale private content;
- use-after-return;
- cross-thread misuse;
- hidden global memory.

Pool phải bounded, observable và có clear owner.

---

## 45. Buffer Pool Rules

1. Rented buffer có một temporary owner.
2. Không return khi còn reference/lease.
3. Sau return, buffer invalid ngay.
4. Sensitive data được clear khi policy yêu cầu.
5. Oversized buffer có thể discard.
6. Pool capacity bounded.
7. Pool pressure observable.
8. Pool không trở thành hidden durable cache.

---

## 46. Context Memory

Translation context phải bounded.

Có thể dùng:

- current unit;
- nearby unit;
- bounded glossary;
- bounded recent-name context;
- short rolling summary.

Không truyền complete reading history mặc định.

---

## 47. Context Budget

Context limit có thể theo:

- unit count;
- character count;
- token estimate;
- byte size;
- memory class.

Exact value thuộc Translation configuration.

---

## 48. Runtime Resource Budget

Runtime dùng budget tổng quát:

```text
Runtime Resource Budget
├── Managed Memory Budget
├── Native Memory Budget
├── GPU Budget
├── Artifact Budget
├── Lease Budget
├── Provider Resource Budget
├── UI Resource Budget
└── Diagnostics Budget
```

Budget là control limit, không nhất thiết physical partition.

---

## 49. Resource Pressure Levels

```text
NORMAL
ELEVATED
HIGH
CRITICAL
```

Transition nên có hysteresis.

---

## 50. Resource Pressure Signal

Memory/Resource Manager chỉ phát signal và accounting.

Luồng đúng:

```text
Resource pressure detected
        ↓
Budget state updated
        ↓
Scheduler reduces admission
        ↓
Runtime Control coordinates cancellation/cleanup
```

Memory component không tự quyết định business failure.

---

## 51. Pressure Response Order

1. stop speculative work;
2. stop cache warming;
3. release expired cache retention;
4. evict low-value cache retention;
5. dispose obsolete Revision;
6. stop background admission;
7. reduce expensive concurrency;
8. unload idle provider resource;
9. reject non-critical admission;
10. fail current processing safely nếu invariant không thể giữ.

Control path và current useful work được bảo vệ cao nhất.

---

## 52. Admission Cost Hint

WorkItem có thể cung cấp:

```text
MemoryCostHint
GpuCostHint
NativeResourceHint
ArtifactCostHint
```

Hint có thể là:

```text
SMALL
MEDIUM
LARGE
UNKNOWN
```

hoặc estimated range.

Hint không phải guarantee.

---

## 53. Memory and Cancellation

Cancellation revoke logical value ngay, nhưng physical memory có thể còn.

```text
Authority revoked
    ↓
Attempt draining
    ↓
Lease released
    ↓
Physical resource disposed
```

Draining resource phải được accounting riêng.

---

## 54. Draining Resource

Draining resource thuộc work đã mất authority nhưng chưa cleanup vật lý.

Metrics nên phân biệt:

- active;
- cached;
- draining;
- provider-resident;
- UI-retained;
- diagnostics-retained.

---

## 55. Memory and Retry

Retry giữ same shared input Artifact, nhưng release Attempt-local resource.

```text
Attempt 1 ends
    ↓
Release Attempt-local resources
    ↓
Keep compatible shared ArtifactRefs
    ↓
Attempt 2 created
```

Non-cancelable provider resource có thể vẫn draining; admission phải xét resource truth.

---

## 56. Queue Memory

Queue chỉ giữ lightweight metadata và ArtifactRef.

Queue không acquire long-lived payload ownership.

Queue không giữ lease suốt thời gian pending trừ khi có explicit policy rất ngắn và bounded.

---

## 57. Event Memory

Event chỉ mang lightweight identity và reference.

Preferred:

```text
ATTEMPT_COMPLETED
├── SessionId
├── RevisionId
├── WorkItemId
├── AttemptId
└── ArtifactRef
```

Không nhúng full payload.

---

## 58. Diagnostics Memory

Standard diagnostics không giữ:

- screenshot;
- source text;
- translated text;
- raw prompt;
- provider body;
- secret.

Debug content chỉ khi:

- explicit enable;
- bounded;
- short-lived;
- redacted;
- privacy-aware.

---

## 59. Resource Leak

Leak có thể gồm:

- managed memory leak;
- lease leak;
- native handle leak;
- GPU resource leak;
- provider session leak;
- event subscription leak;
- stale UI retention;
- unbounded context;
- queue metadata retention;
- process/IPC handle leak.

---

## 60. Disposal Eligibility

Revision/resource đủ điều kiện disposal khi:

- no current authority;
- no pending queue ownership;
- no running Attempt use;
- no active lease;
- no UI retention;
- no cache retention;
- no required diagnostics retention;
- physical cleanup safe.

---

## 61. Disposal Coordination

Resource Manager hoặc Artifact Store điều phối physical disposal.

Worker chỉ:

- release local resource;
- release lease;
- report cleanup outcome.

Worker không dispose shared Artifact độc lập.

---

## 62. Automatic Memory Management

Automatic memory management có thể thu hồi ordinary object.

Architecture không dựa vào nó cho timely cleanup của:

- native resource;
- GPU resource;
- file mapping;
- process handle;
- pooled buffer;
- capture surface;
- provider handle.

---

## 63. Metrics

Runtime nên đo:

- total process memory;
- managed heap estimate;
- native memory estimate;
- GPU memory estimate;
- active Artifact memory;
- cache retention memory;
- Attempt-local memory;
- provider-resident memory;
- draining memory;
- UI-retained memory;
- Artifact count;
- Artifact reference count;
- active lease count;
- lease lifetime;
- native handle count;
- GPU resource count;
- queue metadata memory;
- disposal latency;
- resource admission reject count;
- pressure state.

---

## 64. Size Accounting

Large Artifact cần estimated size đủ để:

- admission;
- eviction;
- diagnostics;
- profiling;
- capacity planning.

Không yêu cầu perfect byte accounting trong MVP.

---

## 65. Resource Diagnostics

Diagnostics cần trả lời:

- resource type nào lớn nhất;
- Revision nào retained;
- owner nào giữ retention;
- lease nào quá hạn;
- provider nào giữ memory;
- bao nhiêu resource đang draining;
- resource nào vượt lifetime;
- cache/UI/diagnostics giữ bao nhiêu.

---

## 66. Privacy

Sensitive resource chỉ tồn tại khi cần.

Runtime phải:

- không tự ghi Artifact xuống disk;
- tránh crash dump chứa content nếu configurable;
- clear pooled buffer khi cần;
- không gửi raw payload qua telemetry;
- release source image sớm;
- tách durable persistence sang Storage policy.

---

## 67. MVP Resource Policy

MVP sử dụng:

- process-local in-memory Artifact Store;
- bounded queue;
- low worker concurrency;
- current Revision;
- optional previous displayed Revision;
- small draining set;
- bounded memory cache;
- explicit native/GPU cleanup;
- no implicit persistent Artifact cache;
- no custom pooling trước profiling.

---

## 68. MVP Retention Guidance

| Resource | MVP retention |
|---|---|
| Observation frame | latest + previous |
| Current stable source | 1 current |
| Previous displayed presentation | at most 1 |
| Draining Revision | small bounded count |
| Shared Artifact | bounded by owner/lease/cache |
| Background Artifact | disabled or strict bounded |
| Debug content | disabled by default |
| Local model | one large model class at a time unless profiled |
| Provider response | Attempt-local only |

---

## 69. Example: Normal Execution

```text
Source Artifact accepted
    ↓
Worker acquires Artifact Lease
    ↓
Attempt-local buffers created
    ↓
Candidate output produced
    ↓
Authority validated
    ↓
Artifact Store accepts ownership
    ↓
Worker releases lease and temporary resources
```

---

## 70. Example: Rapid Revision Replacement

```text
Revision A current
    ↓
Revision B created
    ↓
Revision A authority revoked
    ↓
New leases denied where appropriate
    ↓
Running Attempt drains
    ↓
Lease released
    ↓
Revision A retention removed
    ↓
Physical disposal
```

---

## 71. Example: Shared Artifact

```text
Revision A ─┐
            ├── same ArtifactRef
Revision B ─┘
```

Payload không duplicate.

---

## 72. Example: Cache Eviction with Active Lease

```text
Cache retention removed
    ↓
Worker lease still active
    ↓
Payload remains
    ↓
Lease released
    ↓
No owner remains
    ↓
Physical disposal
```

---

## 73. Example: Critical Resource Pressure

```text
Pressure = CRITICAL
    ↓
Scheduler stops non-critical admission
    ↓
Cache retention reduced
    ↓
Obsolete Revision canceled/drained
    ↓
Idle provider resource unloaded
    ↓
Current work preserved if safe
```

---

## 74. Architecture Invariants

1. Large payload không đi qua Queue.
2. Published Artifact immutable.
3. Mọi large resource có logical owner.
4. Ownership transfer explicit.
5. Retention tracking explicit.
6. Resource Lease scoped và observable.
7. Physical disposal chờ owner/lease hết.
8. Logical disposal tách physical disposal.
9. Scheduler không sở hữu payload.
10. Work Queue không sở hữu payload.
11. Worker chỉ sở hữu Attempt-local resource trừ khi transfer.
12. Revision không sở hữu cache retention.
13. Cache promotion không copy payload mặc định.
14. Cache eviction không invalidate active lease.
15. Artifact Store khác Storage.
16. Native/GPU resource có explicit lifecycle.
17. GC không đảm bảo timely native cleanup.
18. Resource budget bounded.
19. Resource pressure chỉ tạo signal; Scheduler quyết định admission.
20. Cancellation revoke authority trước disposal.
21. Retry giữ shared input, release Attempt-local resource.
22. Draining resource được accounting.
23. UI retention bounded.
24. Runtime Revision history bounded.
25. Diagnostics không giữ user content mặc định.
26. Resource leak observable.
27. Lease lifetime không được unbounded không phát hiện.
28. Provider capacity phản ánh physical reality.
29. Resource cleanup failure không revive work.
30. Runtime vẫn correct khi toàn bộ cache bị evict.

---

## 75. Testing Requirements

Test phải bao phủ:

- repeated source update;
- rapid Revision replacement;
- active lease during disposal;
- cache promotion without copy;
- cache eviction with active lease;
- cancellation with draining provider;
- retry same shared Artifact;
- session close;
- previous presentation release;
- native handle cleanup;
- GPU cleanup;
- provider model load/unload;
- queue payload lightweight;
- lease leak detection;
- resource pressure transition;
- bounded Revision retention;
- Artifact reuse;
- cleanup idempotency;
- privacy retention;
- long-running memory stabilization.

---

## 76. Profiling Requirements

Profile:

- source Artifact size;
- copy count;
- peak Attempt-local memory;
- provider resident memory;
- GPU memory;
- context size;
- presentation surface memory;
- cancellation drain latency;
- lease lifetime;
- cache hit/retention;
- long-session resource trend;
- native handle trend.

---

## 77. Open Questions

- Runtime stack quản lý native resource thế nào?
- Capture surface CPU hay GPU-backed?
- Artifact Store dùng lease table hay managed reference?
- Default RAM/GPU budget là bao nhiêu?
- Previous presentation giữ trong trường hợp nào?
- Local model nào vào MVP?
- Provider worker có chạy isolated process không?
- Large document import dùng streaming thế nào?
- Resource lease timeout có cần hard limit không?
- Artifact backing store có memory mapping không?
- Device minimum RAM/GPU là bao nhiêu?

---

## 78. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | Revision, WorkItem, Attempt và Artifact |
| `RUNTIME_COMPONENTS.md` | Artifact Store, Revision Store, Resource Manager |
| `WORK_QUEUE.md` | Lightweight queued reference |
| `SCHEDULER.md` | Resource-aware admission |
| `CANCELLATION.md` | Authority revocation và drain |
| `RETRY_POLICY.md` | Attempt-local cleanup và shared input |
| `CACHE_POLICY.md` | Retention promotion/eviction |
| `RESOURCE_LIFECYCLE.md` | Ownership transfer và physical disposal |
| `THREADING_MODEL.md` | Thread/process resource affinity |
| `PERFORMANCE_MODEL.md` | Resource pressure và useful latency |
| `RUNTIME_CONFIG.md` | Budget và limits |
| `RUNTIME_OBSERVABILITY.md` | Resource metrics |
| `../../modules/storage/README.md` | Durable persistence boundary |

---

## 79. Completion Criteria

`MEMORY_MODEL.md` được xem là đồng bộ khi:

- memory được đặt trong resource ownership/lifetime model;
- Runtime Revision Registry và Artifact Store tách rõ;
- Storage boundary rõ;
- Resource Lease tổng quát hơn Artifact Lease;
- Stage vocabulary được loại bỏ khỏi runtime reference;
- WorkItem dùng ArtifactRef;
- retention tracking không bắt buộc reference count;
- logical/physical disposal tách rõ;
- cache promotion không copy payload;
- resource budget có RAM/GPU/native/Artifact/Lease;
- Scheduler sở hữu admission;
- retry/cancellation/drain khớp Runtime v2;
- leak, metrics và MVP policy đầy đủ.

---

## 80. Summary

CRAI sử dụng resource-oriented memory model:

```text
Explicit Owner
    ↓
Defined Lifetime
    ↓
Immutable Artifact or Scoped Resource
    ↓
Lightweight Reference
    ↓
Bounded Retention
    ↓
Logical Disposal
    ↓
Physical Disposal When Safe
```

Ranh giới cốt lõi:

```text
Ownership determines lifetime.

Lifetime determines memory pressure.

Leases protect active use.

Retention changes do not require payload copies.

Storage remains a separate durable persistence capability.
```
