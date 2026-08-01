# runtime/RESOURCE_LIFECYCLE.md

# Runtime Resource Lifecycle

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime tạo, đăng ký, chuyển giao, publish, retain, lease, release, drain và dispose runtime resource.

Tài liệu này là nguồn chuẩn cho các khái niệm:

```text
Ownership
Retention
Lease
Logical Disposal
Physical Disposal
```

Resource lifecycle phải bảo đảm:

- mọi resource có owner rõ ràng;
- sharing không làm mất ownership boundary;
- retention khác ownership;
- authority khác ownership;
- active lease chặn physical disposal;
- canceled hoặc stale resource không được revive;
- cleanup failure không khôi phục authority;
- resource lifetime luôn bounded và observable.

---

## 2. Core Model

CRAI dùng mô hình:

```text
Create
    ↓
Register (optional)
    ↓
Ownership Transfer (optional)
    ↓
Publish (optional)
    ↓
Retain / Acquire Lease (optional)
    ↓
Use
    ↓
Release Lease / Retention
    ↓
Logical Disposal
    ↓
Draining
    ↓
Physical Disposal
```

Không phải resource nào cũng đi qua mọi bước.

Temporary buffer có thể chỉ:

```text
Create
    ↓
Use
    ↓
Physical Disposal
```

Shared Artifact thường đi qua toàn bộ lifecycle.

---

## 3. Architectural Position

```text
Runtime Control
    → owns execution authority

Artifact Store
    → owns accepted Artifact payload

Cache Policy
    → owns reuse/retention policy

Resource Manager
    → coordinates physical lifecycle

Worker Execution
    → owns Attempt-local resource

Provider Manager
    → owns provider-lifetime resource

Storage
    → owns durable persistence mechanics
```

Không component nào được chiếm ownership ngoài boundary của mình.

---

## 4. Core Principles

1. Mỗi resource có một payload owner tại một thời điểm.
2. Một resource có thể có nhiều retention owner.
3. Lease không cấp payload ownership.
4. Publication không tự chuyển ownership.
5. Authority không đồng nghĩa ownership.
6. Retention không đồng nghĩa ownership.
7. Logical disposal luôn xảy ra trước physical disposal.
8. Physical disposal chỉ xảy ra khi không còn owner, retention hoặc lease.
9. Worker không dispose shared resource.
10. Cache promotion không copy payload mặc định.
11. Resource đã physical disposal không được resurrect.
12. Draining resource phải observable.
13. Cleanup failure không restore authority.
14. Native/GPU resource cần explicit lifecycle.
15. Resource lifecycle không phụ thuộc GC timing.

---

## 5. Resource Categories

Resource được phân loại theo lifetime và sharing semantics.

### 5.1 Runtime-Global

Ví dụ:

- worker pool;
- provider client;
- configuration registry;
- shared model;
- Event Bus infrastructure.

### 5.2 Session-Scoped

Ví dụ:

- session runtime metadata;
- source observation state;
- session presentation reference;
- session provider preference.

### 5.3 Revision-Scoped

Ví dụ:

- Revision metadata;
- Revision retention;
- accepted ArtifactRef;
- Revision resource accounting.

### 5.4 Attempt-Local

Ví dụ:

- temporary buffer;
- provider request body;
- intermediate tensor;
- temporary file;
- child process handle.

### 5.5 Shared

Ví dụ:

- accepted Artifact;
- shared model;
- reusable provider client;
- UI presentation Artifact.

### 5.6 External

Ví dụ:

- HTTP request;
- GPU context;
- capture surface;
- native handle;
- child process;
- memory mapping.

---

## 6. Resource State Machine

Canonical lifecycle state:

```text
CREATED
  ↓
REGISTERED
  ↓
PUBLISHED
  ↓
ACTIVE
  ↓
LOGICALLY_DISPOSED
  ↓
DRAINING
  ↓
PHYSICALLY_DISPOSED
```

Optional states:

```text
TRANSFER_PENDING
INVALID
DISPOSAL_FAILED
```

Không phải resource nào cũng đi qua `REGISTERED` hoặc `PUBLISHED`.

---

## 7. Ownership State

Payload ownership có thể chuyển:

```text
CREATOR_OWNED
    ↓
TRANSFER_PENDING
    ↓
RUNTIME_COMPONENT_OWNED
    ↓
NO_OWNER
    ↓
PHYSICALLY_DISPOSED
```

Ownership transfer phải atomic hoặc có rollback rõ ràng.

---

## 8. Visibility State

Visibility tách khỏi ownership.

```text
PRIVATE
    ↓
PUBLISHED
    ↓
WITHDRAWN
```

Ví dụ:

- Attempt-local buffer luôn private.
- Candidate Artifact private cho đến khi validation.
- Accepted Artifact được publish bởi Artifact Store.
- Logical disposal có thể withdraw Artifact khỏi new lookup.

---

## 9. Candidate Resource

Worker output ban đầu là candidate:

```text
Temporary Output
    ↓
Candidate Resource
    ↓
Validation
    ↓
Accepted Resource
```

Candidate Resource:

- chưa reusable;
- chưa có commit authority;
- chưa được cache;
- vẫn thuộc producer hoặc transfer-pending owner;
- phải cleanup nếu validation fail.

---

## 10. Publication Boundary

Publication nghĩa là resource được component khác nhìn thấy qua contract.

Publication không có nghĩa:

- ownership transfer;
- cache promotion;
- authority grant;
- durable persistence.

Các hành vi đó phải diễn ra riêng.

---

## 11. Ownership vs Authority

```text
Artifact Store
    → owns payload

Runtime Control
    → owns authority
```

Artifact có thể tồn tại về vật lý nhưng không còn authority.

Ví dụ stale Artifact:

- payload vẫn tồn tại;
- owner vẫn là Artifact Store;
- commit authority đã mất;
- cache promotion bị từ chối theo policy.

---

## 12. Ownership vs Retention

```text
Artifact Store
    → payload owner

Revision
Cache
UI
Diagnostics
    → retention owners
```

Retention owner chỉ ngăn disposal.

Retention owner không được mutate hoặc dispose payload trực tiếp.

---

## 13. Resource Lease

`ResourceLease` là quyền sử dụng tạm thời, không phải ownership.

Possible lease types:

```text
ArtifactLease
GpuResourceLease
NativeHandleLease
ProviderResourceLease
CaptureSurfaceLease
```

Lease phải:

- scoped;
- có identity;
- có owner;
- có acquisition time;
- releasable;
- observable;
- bounded hoặc leak-detectable.

---

## 14. Lease Timeline

```text
Acquire Lease
    ↓
Use Resource
    ↓
Release Lease
    ↓
Disposal Eligibility Re-evaluated
```

Lease expiration không tự dispose resource.

---

## 15. Retention Timeline

```text
Revision Retention
    ↓
Cache Retention Added
    ↓
Revision Retention Removed
    ↓
UI Retention Removed
    ↓
Cache Retention Removed
    ↓
No Retention
    ↓
Physical Disposal Eligible
```

Lease và retention là hai hệ thống độc lập.

---

## 16. Creation

Creator sở hữu resource ngay khi creation thành công.

Creator phải:

- register cleanup path;
- biết resource type;
- biết expected lifetime;
- biết transfer target nếu có;
- xử lý failure giữa create và register.

---

## 17. Registration

Large hoặc shared resource nên được register trước khi share.

Registration gán:

- ResourceId;
- type;
- owner;
- retention metadata;
- size/cost estimate;
- backing resource;
- integrity state;
- lifecycle state.

---

## 18. Ownership Transfer

Ví dụ accepted Artifact:

```text
Worker owns candidate output
        ↓
Artifact Store registers transfer
        ↓
Runtime Control validates authority
        ↓
Artifact Store accepts ownership
        ↓
Worker releases creator ownership
```

Nếu transfer fail:

- creator vẫn owner;
- candidate phải cleanup;
- không được xuất hiện half-published.

---

## 19. Shared Resource

Shared resource luôn có:

- one payload owner;
- zero hoặc nhiều retention owner;
- zero hoặc nhiều active lease;
- immutable shared state khi là Artifact;
- explicit disposal coordinator.

Shared Business Module Artifact không hard-code OCR/Layout-specific lifecycle.

---

## 20. Attempt-Local Resource

Attempt-local resource:

- do Worker hoặc Provider Adapter sở hữu;
- không publish mặc định;
- không có shared retention;
- release khi Attempt terminal;
- có thể drain nếu provider không cancel được;
- không được giữ bởi Event Bus.

---

## 21. Artifact Lifecycle

```text
Candidate Created
    ↓
Registered
    ↓
Validated
    ↓
Ownership Transferred to Artifact Store
    ↓
Published
    ↓
Retained / Leased
    ↓
Logical Disposal
    ↓
Physical Disposal
```

Artifact đã publish là immutable.

---

## 22. Cache Promotion

Cache promotion thêm retention owner:

```text
Accepted Artifact
    ↓
Cache Policy approves
    ↓
Cache Retention Added
    ↓
Payload unchanged
```

Cache không trở thành payload owner.

---

## 23. Cache Eviction

Cache eviction:

```text
Remove Cache Retention
    ↓
Re-evaluate Disposal Eligibility
```

Nếu Revision, UI hoặc Lease còn, payload vẫn sống.

---

## 24. Revision Lifecycle Interaction

Khi Revision bị superseded:

```text
Authority Revoked
    ↓
Queued Work Removed
    ↓
Running Attempts Draining
    ↓
Revision Retention Released
    ↓
Artifact Disposal Re-evaluated
```

Revision metadata và Artifact payload có lifecycle riêng.

---

## 25. Session Shutdown

```text
Runtime Control revokes Session authority
        ↓
Child cancellation propagated
        ↓
Queued Work removed
        ↓
Running Attempts drain
        ↓
UI retention released
        ↓
Session retention released
        ↓
Physical disposal when eligible
```

---

## 26. Application Shutdown

```text
Stop new admission
        ↓
Revoke application authority
        ↓
Cancel sessions
        ↓
Drain queue and workers
        ↓
Release leases
        ↓
Release retention owners
        ↓
Dispose providers and runtime components
        ↓
Flush bounded diagnostics
```

Chi tiết order phải thống nhất với `BOOT_SEQUENCE.md`.

---

## 27. Provider Resource Lifecycle

Provider Manager sở hữu provider-lifetime resource.

```text
CREATED
  ↓
INITIALIZING
  ↓
READY
  ↔
IDLE
  ↓
DRAINING
  ↓
UNLOADING
  ↓
DISPOSED
```

Per-request resource vẫn thuộc Attempt/Adapter.

---

## 28. Local Model Lifecycle

```text
UNLOADED
  ↓
LOADING
  ↓
READY
  ↔
IDLE
  ↓
UNLOADING
  ↓
DISPOSED
```

Model load/unload phải xét:

- active lease;
- provider request;
- GPU memory;
- resource pressure;
- shutdown state.

---

## 29. Worker Boundary

Worker lifecycle chi tiết thuộc `THREADING_MODEL.md`.

Tại tài liệu này chỉ chốt:

```text
Worker owns Attempt-local resource.
Worker may hold Lease.
Worker never disposes shared Artifact.
Worker releases all local ownership on terminal completion.
```

---

## 30. Native Resource Lifecycle

Native/GPU/OS resource:

- cần explicit owner;
- có creation failure path;
- có thread/process affinity khi cần;
- không phụ thuộc GC finalizer;
- có disposal timeout;
- leak phải observable;
- có fallback khi disposal fail.

---

## 31. Resource Dependency Graph

```text
Session
    │
    ▼
Revision
    │
    ├── Shared Artifact
    ├── Presentation Retention
    └── WorkItem
           │
           ▼
        Attempt
           ├── Temporary Buffer
           ├── Provider Request
           ├── GPU Lease
           └── Artifact Lease
```

Disposal phải tôn trọng graph nhưng không hard-code pipeline stage order.

---

## 32. Disposal Eligibility

Resource chỉ eligible khi:

```text
No Runtime Authority Requirement
No Payload Owner Requirement
No Retention Owner
No Active Lease
No UI Use
No Provider Use
No Required Diagnostics Retention
```

Sau đó mới được physical disposal.

---

## 33. Logical Disposal

Logical disposal:

- withdraw khỏi new lookup;
- deny new lease;
- release retention khi policy cho phép;
- mark draining;
- không cho new publication;
- không cho resurrection.

Resource có thể vẫn tồn tại vật lý.

---

## 34. Draining

Draining nghĩa là:

- authority hoặc retention chính đã mất;
- physical use vẫn còn;
- cleanup đang diễn ra;
- resource vẫn accounting;
- no new work may acquire it unless explicit exception.

Draining resource phải có timeout hoặc leak detection.

---

## 35. Physical Disposal

Physical disposal do Artifact Store hoặc Resource Manager điều phối.

Physical disposal phải:

- idempotent khi khả thi;
- không chạy khi lease còn;
- tôn trọng thread/process affinity;
- report success/failure;
- remove backing resource;
- mark state terminal.

---

## 36. Disposal Order

Không hard-code Recognition → Translation → Presentation.

Generic order:

```text
Stop New Use
    ↓
Release Attempt-Local Resource
    ↓
Release UI/Cache/Revision Retention
    ↓
Release Leases
    ↓
Dispose Shared Artifact Backing
    ↓
Dispose Revision/Session Metadata
    ↓
Dispose Runtime-Global Resource
```

Dependency-specific cleanup theo Resource Graph.

---

## 37. Resource Resurrection

Sau `PHYSICALLY_DISPOSED`:

```text
ResourceId cannot become active again.
```

Reuse cần create/register resource mới.

Resurrection attempt là invariant violation.

---

## 38. Resource Version

Identity, version và compatibility tách biệt:

```text
ResourceId
ResourceVersion
CompatibilityMetadata
```

Version thay đổi không tự đổi identity semantics.

Business Module định nghĩa compatibility.

---

## 39. Cleanup Failure

Cleanup failure tạo normalized `RuntimeError`.

```text
Primary Outcome
    +
Cleanup Error
```

Rules:

- không restore authority;
- không restore ownership;
- không revive resource;
- retry cleanup chỉ theo safe policy;
- diagnostics phải giữ primary và cleanup error;
- repeated failure có thể degrade Resource Manager/Provider.

---

## 40. Cleanup Retry Boundary

Cleanup retry khác WorkItem retry.

Cleanup retry:

- không tạo business WorkItem;
- không thay Attempt lineage;
- không grant authority;
- chỉ cố hoàn tất physical cleanup;
- phải bounded;
- có backoff nếu cần.

---

## 41. Lifecycle Events

Conceptual events:

```text
RESOURCE_CREATED
RESOURCE_REGISTERED
RESOURCE_TRANSFER_STARTED
RESOURCE_TRANSFER_COMPLETED
RESOURCE_PUBLISHED
RETENTION_ADDED
RETENTION_REMOVED
LEASE_ACQUIRED
LEASE_RELEASED
RESOURCE_LOGICALLY_DISPOSED
RESOURCE_DRAINING
RESOURCE_PHYSICALLY_DISPOSED
RESOURCE_DISPOSAL_FAILED
RESOURCE_LEAK_DETECTED
```

Tên cuối tuân theo Event Standard.

---

## 42. Event Payload

```text
ResourceId
ResourceType
Owner
PreviousOwner
RetentionClass
LeaseId
Scope
SessionId
RevisionId
WorkItemId
AttemptId
OccurredAt
ReasonCode
```

Không chứa payload.

---

## 43. Metrics

Theo dõi:

- active resource count;
- resource count by type;
- ownership transfer count;
- retention owner count;
- active lease count;
- lease lifetime;
- logical disposal count;
- physical disposal count;
- draining resource count;
- disposal latency;
- disposal blocked by lease;
- cleanup retry count;
- cleanup failure count;
- leaked resource count;
- native handle count;
- GPU resource count;
- provider resource count.

---

## 44. Leak Detection

Leak indicators:

- lease vượt expected lifetime;
- resource draining quá lâu;
- owner đã dispose nhưng payload còn;
- provider request không release;
- native handle tăng liên tục;
- GPU allocation không giảm;
- Session close nhưng retention còn;
- UI giữ old Revision vô hạn.

---

## 45. Privacy

Resource lifecycle telemetry không chứa raw content.

Sensitive resource:

- release sớm;
- clear buffer khi policy yêu cầu;
- không persist ngoài Storage policy;
- không giữ trong debug retention mặc định;
- không xuất hiện trong event payload.

---

## 46. Failure Isolation

Nếu disposal một resource fail:

- resource khác vẫn cleanup;
- ownership graph không corrupt;
- no new authority;
- no new lease;
- failed resource giữ trạng thái observable;
- Runtime có thể continue nếu safety còn giữ được.

---

## 47. MVP Policy

MVP yêu cầu:

- one payload owner;
- explicit retention owner;
- Artifact Store ownership;
- Worker Attempt-local ownership;
- Resource Lease;
- logical/physical disposal split;
- draining state;
- bounded cleanup retry;
- no resurrection;
- no complex general-purpose graph engine;
- process-local lifecycle registry;
- explicit native/GPU cleanup khi có.

---

## 48. Example: Artifact Publication

```text
Worker creates candidate
        ↓
Artifact Store registers
        ↓
Runtime Control validates
        ↓
Ownership transfer completes
        ↓
Artifact published
        ↓
Worker releases local ownership
```

---

## 49. Example: Cache Promotion

```text
Accepted Artifact
        ↓
Cache Policy approves
        ↓
Cache retention added
        ↓
Payload owner remains Artifact Store
```

---

## 50. Example: Revision Cancellation

```text
Revision authority revoked
        ↓
No new lease
        ↓
Running Attempt drains
        ↓
Revision retention removed
        ↓
Last lease released
        ↓
Artifact physically disposed
```

---

## 51. Example: Disposal Blocked by Lease

```text
Logical disposal requested
        ↓
Active Artifact Lease exists
        ↓
Resource enters DRAINING
        ↓
Lease released
        ↓
Physical disposal proceeds
```

---

## 52. Example: Provider Request Abandoned

```text
Authority revoked
        ↓
Abort unsupported
        ↓
Attempt becomes abandoned
        ↓
Provider request remains physical
        ↓
Provider resource stays accounted
        ↓
Late completion arrives
        ↓
Result rejected and resource disposed
```

---

## 53. Example: Cleanup Failure

```text
Physical disposal invoked
        ↓
Native API fails
        ↓
RESOURCE_DISPOSAL_FAILED
        ↓
Resource remains non-active and draining
        ↓
Bounded cleanup retry
```

---

## 54. Architecture Invariants

1. Mỗi payload có một owner.
2. Retention owner có thể nhiều.
3. Lease không cấp ownership.
4. Publication không cấp ownership.
5. Authority khác ownership.
6. Ownership khác retention.
7. Logical disposal trước physical disposal.
8. Resource đang lease không physical dispose.
9. Cache promotion không đổi payload owner.
10. Cache promotion không copy payload mặc định.
11. Worker chỉ dispose Attempt-local resource.
12. Worker không dispose shared Artifact.
13. Artifact Store sở hữu accepted Artifact payload.
14. Runtime Control sở hữu authority.
15. Draining resource observable.
16. Resource đã dispose không resurrect.
17. Ownership transfer atomic hoặc rollback-safe.
18. Candidate không reusable trước acceptance.
19. Cleanup failure không restore authority.
20. Cleanup failure không restore ownership.
21. Native/GPU resource có explicit lifecycle.
22. Resource event không chứa payload.
23. Retention release không phá active lease.
24. Session shutdown revoke authority trước disposal.
25. Application shutdown dừng admission trước cleanup.
26. Physical disposal do coordinator thực hiện.
27. Resource accounting bounded.
28. Lease lifetime leak-detectable.
29. Storage không quản lý runtime lease.
30. Artifact Store không thay Storage.

---

## 55. Testing Requirements

Test phải bao phủ:

- create/register/publish;
- ownership transfer success;
- ownership transfer rollback;
- double publish;
- duplicate registration;
- lease acquire/release;
- dispose with active lease;
- late lease release;
- cache promotion;
- cache eviction;
- Revision drain;
- Session shutdown;
- Application shutdown;
- provider unload;
- native disposal failure;
- GPU cleanup;
- abandoned provider request;
- double dispose;
- resurrection prevention;
- cleanup retry;
- leak detection;
- event privacy;
- shared Artifact reuse.

---

## 56. Open Questions

- Resource Manager có standalone component trong MVP không?
- Ownership registry dùng reference count, owner set hay lease table?
- Lease timeout là hard hay diagnostic-only?
- Native disposal retry bao nhiêu lần?
- Provider resource isolation bằng process hay thread?
- GPU resource có cần dedicated manager không?
- UI retention boundary cụ thể nằm ở Presentation hay Runtime?
- Durable Artifact materialization có lifecycle adapter riêng không?
- Cleanup task chạy trên execution context nào?
- Leak threshold theo resource type là bao nhiêu?

---

## 57. Related Documents

| Document | Relationship |
|---|---|
| `MEMORY_MODEL.md` | Memory/resource ownership and budgets |
| `CACHE_POLICY.md` | Retention promotion and eviction |
| `PIPELINE_RUNTIME.md` | Revision, WorkItem, Attempt and authority |
| `RUNTIME_COMPONENTS.md` | Artifact Store and Resource Manager |
| `CANCELLATION.md` | Authority revocation and drain |
| `RETRY_POLICY.md` | Attempt-local cleanup |
| `THREADING_MODEL.md` | Affinity and execution context |
| `ERROR_MODEL.md` | Cleanup RuntimeError |
| `RUNTIME_CONFIG.md` | Cleanup timeout and limits |
| `RUNTIME_OBSERVABILITY.md` | Lifecycle metrics and events |
| `BOOT_SEQUENCE.md` | Startup/shutdown ordering |
| `../../modules/storage/README.md` | Durable persistence boundary |

---

## 58. Completion Criteria

`RESOURCE_LIFECYCLE.md` được xem là đồng bộ khi:

- ownership là khái niệm trung tâm;
- payload ownership tách retention ownership;
- Resource Lease tổng quát;
- candidate/published/accepted resource tách rõ;
- authority tách ownership;
- logical/physical disposal tách rõ;
- draining state được định nghĩa;
- cache promotion không chuyển payload owner;
- disposal eligibility rõ;
- no resurrection invariant tồn tại;
- events, metrics, testing và MVP policy đầy đủ;
- không hard-code OCR/Layout/Translation disposal order.

---

## 59. Summary

CRAI Runtime Resource Lifecycle dùng mô hình:

```text
Create
    ↓
Own
    ↓
Transfer / Publish
    ↓
Retain / Lease
    ↓
Use
    ↓
Release
    ↓
Logical Disposal
    ↓
Drain
    ↓
Physical Disposal
```

Ranh giới cốt lõi:

```text
Ownership controls responsibility.

Retention controls lifetime.

Lease protects active use.

Authority controls whether a result may matter.

Disposal occurs only when all four allow it.
```
