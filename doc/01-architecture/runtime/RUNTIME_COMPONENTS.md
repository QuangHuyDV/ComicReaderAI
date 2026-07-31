# runtime/RUNTIME_COMPONENTS.md

# Runtime Components

## 1. Purpose

Tài liệu này xác định các **logical runtime component** của CRAI, trách nhiệm của từng component, ownership đối với runtime state, quan hệ phụ thuộc và lifecycle khi ứng dụng đang hoạt động.

Tài liệu này đóng vai trò là bản đồ tổng hợp của Runtime Architecture.

Tài liệu này không:

- mô tả class, package hoặc framework cụ thể;
- thay thế tài liệu chi tiết của từng runtime concern;
- biến các Business Module thành Runtime Component;
- quyết định process topology hoặc công nghệ triển khai;
- mô tả provider implementation cụ thể.

---

## 2. Runtime Component Definition

Trong CRAI, **Runtime Component** là một đơn vị trách nhiệm logic tham gia trực tiếp vào việc:

- khởi động và dừng runtime;
- quản lý runtime state;
- điều phối revision và WorkItem;
- kiểm soát scheduling, cancellation và retry;
- quản lý artifact và resource;
- thực thi công việc;
- xác nhận quyền commit kết quả;
- thu thập observability.

Runtime Component không đồng nghĩa với:

- Business Module;
- source-code module;
- class;
- thread;
- process;
- service độc lập.

Một Runtime Component có thể được triển khai bằng một hoặc nhiều object, thread, queue hoặc process tùy Technology Stack và Process Topology về sau.

---

## 3. Runtime Architecture Principles

Runtime Architecture tuân theo các nguyên tắc sau:

1. `Runtime Control` là **single logical writer** đối với runtime state và authority.
2. `Scheduler` là nơi duy nhất quyết định WorkItem nào được admission.
3. Worker không tự tạo downstream work.
4. Worker và Provider Adapter không tự retry.
5. Queue, worker concurrency và provider concurrency luôn bounded.
6. Large payload chỉ được trao đổi bằng `ArtifactRef`.
7. Artifact là immutable sau khi publish.
8. Mỗi WorkItem chỉ có một terminal outcome được chấp nhận.
9. Mỗi lần retry tạo một `AttemptId` mới.
10. Stale result không có commit authority.
11. Cancellation là control flow hợp lệ, không mặc định là failure.
12. Runtime Component chỉ giao tiếp qua contract, command, event hoặc artifact reference.
13. Runtime Architecture không sở hữu business semantics của Capture, Recognition, Translation, Presentation hoặc Storage.
14. Telemetry failure không được làm sai runtime correctness.
15. Shutdown phải dừng admission trước khi cleanup resource.

---

## 4. Runtime Overview

```text
Application Bootstrap
        |
        v
Runtime Configuration
        |
        v
Runtime Control
        |
        +--------------------+----------------------+-------------------+
        |                    |                      |                   |
        v                    v                      v                   v
Session Runtime          Scheduler           Cancellation         Observability
        |                    |                 Coordinator
        |                    |
        |                    v
        |               Work Queues
        |                    |
        |                    v
        |              Worker Execution
        |                    |
        |                    v
        |              Provider Manager
        |
        +--------------------+----------------------+
                             |
                             v
                    Revision / Artifact State
                             |
             +---------------+----------------+
             |                                |
             v                                v
       Revision Store                   Artifact Store
                                              |
                                              v
                                      Resource Manager
```

Các Business Module tham gia pipeline thông qua public contract và worker execution, nhưng không trở thành Runtime Component chỉ vì chúng có stage được thực thi.

---

## 5. Component Groups

Runtime Component được tổ chức thành sáu nhóm trách nhiệm:

```text
Runtime Foundation
Runtime Control
Execution
State and Data
Integration
Observability
```

---

# 6. Runtime Foundation

## 6.1 Application Bootstrap

### Purpose

Chuẩn bị môi trường và khởi tạo Runtime theo boot sequence đã xác định.

### Responsibilities

- khởi tạo dependency cần thiết;
- load và validate configuration ban đầu;
- tạo Runtime Configuration snapshot;
- wiring Runtime Component;
- đăng ký provider và adapter;
- khởi tạo Event Bus và Observability;
- khởi động Runtime Control;
- chuyển runtime sang trạng thái sẵn sàng;
- xử lý startup failure theo boot policy.

### Ownership

Application Bootstrap chỉ sở hữu resource trong giai đoạn khởi tạo cho đến khi ownership được chuyển giao rõ ràng cho component tương ứng.

### Lifetime

```text
APPLICATION_STARTING
        ↓
BOOTSTRAPPING
        ↓
RUNTIME_READY | STARTUP_FAILED
```

Sau khi runtime sẵn sàng, Bootstrap không tiếp tục điều phối runtime operation.

### Related Document

- `BOOT_SEQUENCE.md`
- `RUNTIME_CONFIG.md`

---

## 6.2 Runtime Configuration

### Purpose

Cung cấp configuration hợp lệ, typed, versioned và nhất quán cho Runtime.

### Responsibilities

- load configuration từ các nguồn được phép;
- áp dụng application default;
- validate configuration;
- tạo immutable configuration snapshot;
- phát hành configuration version;
- quản lý activation boundary;
- xác định hot-reload boundary;
- từ chối configuration không hợp lệ;
- cung cấp secret reference thay vì phát tán secret trực tiếp.

### Ownership

Runtime Configuration sở hữu:

- active runtime configuration version;
- configuration snapshot;
- validation result;
- configuration activation state.

### Rules

- WorkItem phải tham chiếu configuration version cần thiết.
- Configuration đang dùng bởi một WorkItem không bị mutate.
- Secret value không được đưa vào event hoặc telemetry.
- Business Module vẫn sở hữu business configuration của chính nó; Runtime Configuration chỉ quản lý cách snapshot và cấp phát configuration cho runtime execution.

### Related Document

- `RUNTIME_CONFIG.md`

---

# 7. Runtime Control

## 7.1 Runtime Control

### Purpose

Là authority trung tâm đối với runtime state, revision authority, WorkItem lifecycle và việc chấp nhận terminal outcome.

### Responsibilities

- quản lý runtime lifecycle;
- quản lý session runtime state;
- tiếp nhận command từ Application và worker;
- tạo và chuyển trạng thái revision;
- tạo logical WorkItem;
- yêu cầu Scheduler admission;
- chấp nhận hoặc từ chối completion;
- xác định stale result;
- tạo downstream work sau khi upstream result hợp lệ;
- áp dụng cancellation và retry decision;
- cho phép hoặc từ chối presentation commit;
- điều phối graceful shutdown.

### Ownership

Runtime Control là single logical writer đối với:

- active runtime state;
- session runtime state;
- current revision authority;
- WorkItem logical state;
- accepted terminal outcome;
- retry lineage;
- commit authority;
- runtime shutdown state.

### Non-responsibilities

Runtime Control không:

- thực thi OCR hoặc Translation;
- chứa provider implementation;
- giữ large payload;
- tự quản lý worker thread;
- thực hiện persistence nghiệp vụ;
- thay thế Business Module hoặc Pipeline Orchestrator ở mức business workflow.

### Related Documents

- `PIPELINE_RUNTIME.md`
- `PIPELINE_ORCHESTRATION.md`
- `CANCELLATION.md`
- `RETRY_POLICY.md`

---

## 7.2 Session Runtime

### Purpose

Biểu diễn trạng thái runtime của một Reading Session đang hoạt động.

### Responsibilities

- duy trì liên kết giữa `SessionId` và current revision;
- tiếp nhận pause, resume và stop authority;
- quản lý session-scoped runtime resource;
- giữ session configuration snapshot reference;
- cô lập WorkItem, queue priority và cancellation scope theo session;
- hỗ trợ session drain và cleanup.

### Ownership

Session Runtime sở hữu runtime metadata của session, không sở hữu business data của Reading Module.

### Distinction

```text
Reading Module
    → sở hữu business meaning và business state của Reading Session

Session Runtime
    → sở hữu execution state cần để session được vận hành
```

### Lifetime

```text
CREATED
  ↓
ACTIVE
  ↔
PAUSED
  ↓
STOPPING
  ↓
STOPPED
```

---

## 7.3 Authority Validator

### Purpose

Xác nhận một result hoặc side effect còn quyền được chấp nhận tại thời điểm commit.

Authority Validator có thể là trách nhiệm nội bộ của Runtime Control trong MVP, chưa bắt buộc là standalone component.

### Validation Inputs

- `SessionId`;
- `RevisionId`;
- `WorkItemId`;
- `AttemptId`;
- current revision;
- terminal state;
- cancellation state;
- configuration version;
- target presentation version khi cần.

### Decisions

```text
ACCEPT
REJECT_STALE
REJECT_CANCELED
REJECT_DUPLICATE
REJECT_INVALID_STATE
```

### Rules

- Worker completion không đồng nghĩa result được chấp nhận.
- Artifact publication và presentation commit đều phải đi qua authority validation phù hợp.
- Validation phải xảy ra trước mọi side effect có thể quan sát được.

---

# 8. Execution Components

## 8.1 Scheduler

### Purpose

Quyết định WorkItem nào được admission, defer, replace hoặc reject.

### Responsibilities

- xét dependency readiness;
- xét revision authority;
- xét queue capacity;
- xét priority;
- xét worker concurrency;
- xét provider concurrency;
- xét resource budget;
- ưu tiên current revision;
- loại bỏ obsolete queued work;
- admission retry attempt;
- giữ capacity cho control và cancellation;
- tạo backpressure khi overload.

### Decisions

```text
ADMIT
DEFER
REJECT
REPLACE
```

### Ownership

Scheduler sở hữu scheduling decision và admission state, nhưng không sở hữu business result hoặc WorkItem terminal outcome.

### Non-responsibilities

Scheduler không:

- thực thi WorkItem;
- tự tạo retry;
- tự quyết định business stage kế tiếp;
- mutate runtime authority;
- giữ artifact payload.

### Related Documents

- `SCHEDULER.md`
- `WORK_QUEUE.md`
- `PERFORMANCE_MODEL.md`

---

## 8.2 Work Queues

### Purpose

Giữ các WorkItem đã được admission hoặc đang chờ execution theo bounded capacity.

### Responsibilities

- lưu immutable WorkItem reference;
- duy trì priority order;
- hỗ trợ current-revision preference;
- loại obsolete queued work;
- hỗ trợ latest-work replacement khi phù hợp;
- cung cấp queue metrics;
- áp dụng bounded capacity;
- hỗ trợ drain khi shutdown.

### Queue Content

Queue chỉ chứa dữ liệu nhẹ:

```text
SessionId
RevisionId
WorkItemId
AttemptId
Stage
Priority
InputArtifactRefs
ConfigurationVersion
CancellationContext
```

Queue không chứa:

- image buffer;
- OCR payload lớn;
- translated document;
- mutable provider DTO;
- secret.

### Ownership

Queue sở hữu vị trí chờ execution, không sở hữu logical WorkItem state hoặc Artifact.

### Related Document

- `WORK_QUEUE.md`

---

## 8.3 Worker Execution

### Purpose

Thực thi một Attempt của WorkItem đã được Scheduler cấp.

### Responsibilities

- nhận immutable execution input;
- acquire Artifact Lease;
- gọi Business Module contract hoặc Provider Adapter phù hợp;
- cooperative cancellation;
- tạo temporary output;
- chuẩn hóa execution outcome;
- publish completion command về Runtime Control;
- release lease và temporary resource.

### Worker Rules

Worker không được:

- mutate Runtime Control state trực tiếp;
- tự tạo downstream WorkItem;
- tự retry;
- commit UI trực tiếp;
- promote artifact thành accepted output khi chưa được validation;
- giữ resource sau terminal cleanup;
- tự coi completion là success được chấp nhận.

### Completion Output

```text
AttemptCompleted
AttemptFailed
AttemptCanceled
AttemptAbandoned
```

Runtime Control quyết định terminal outcome logic cuối cùng, bao gồm `STALE`.

### Related Documents

- `PIPELINE_RUNTIME.md`
- `THREADING_MODEL.md`
- `ERROR_MODEL.md`

---

## 8.4 Cancellation Coordinator

### Purpose

Áp dụng cancellation theo đúng scope và propagation order.

Cancellation Coordinator có thể là trách nhiệm nội bộ của Runtime Control trong MVP.

### Responsibilities

- tiếp nhận cancellation request;
- revoke authority;
- loại queued work;
- signal running attempt;
- hủy delayed retry;
- theo dõi drain;
- ngăn canceled work được hồi sinh;
- chuẩn hóa cancellation outcome.

### Cancellation Scopes

```text
APPLICATION
SESSION
REVISION
WORK_ITEM
ATTEMPT
```

### Related Document

- `CANCELLATION.md`

---

## 8.5 Retry Coordinator

### Purpose

Đánh giá và tạo retry attempt mới khi failure còn phù hợp với current runtime state.

Retry Coordinator có thể là trách nhiệm nội bộ của Runtime Control trong MVP.

### Responsibilities

- phân loại failure;
- kiểm tra relevance;
- kiểm tra retry budget;
- áp dụng backoff và jitter;
- tôn trọng provider `Retry-After`;
- hủy delayed retry khi stale hoặc canceled;
- tạo `AttemptId` mới;
- yêu cầu Scheduler admission;
- hỗ trợ provider fallback như một new attempt.

### Rules

- Attempt cũ không được resume.
- Retry không làm thay đổi logical WorkItem identity.
- Worker và Provider Adapter không tự retry.
- Retry phải kiểm tra cache hoặc existing accepted artifact lại khi phù hợp.

### Related Document

- `RETRY_POLICY.md`

---

# 9. State and Data Components

## 9.1 Revision Store

### Purpose

Lưu runtime metadata của các revision đang hoạt động hoặc đang drain.

### Responsibilities

- tạo revision metadata;
- lưu revision state;
- xác định current revision theo session;
- giữ revision-to-WorkItem relation;
- giữ input/output ArtifactRef;
- hỗ trợ authority validation;
- hỗ trợ revision supersession;
- loại revision metadata khi đủ điều kiện cleanup.

### Ownership

Revision Store sở hữu runtime metadata, không sở hữu nội dung artifact vật lý.

### Rules

- Runtime Control là writer duy nhất.
- Worker chỉ đọc snapshot hoặc reference được cấp.
- Revision cũ có thể còn tồn tại để drain resource nhưng không còn authority.

### Related Documents

- `PIPELINE_RUNTIME.md`
- `MEMORY_MODEL.md`

---

## 9.2 Artifact Store

### Purpose

Quản lý artifact immutable được tạo và sử dụng trong Runtime.

### Responsibilities

- register artifact;
- atomic publication;
- cấp `ArtifactId` và `ArtifactRef`;
- lưu artifact metadata;
- cung cấp artifact lookup;
- quản lý cache ownership khi có;
- phối hợp Artifact Lease;
- xác định artifact eligibility for disposal;
- hỗ trợ memory hoặc temporary backing store tùy implementation.

### Artifact Examples

```text
SourceImageArtifact
StructuredTextArtifact
RecognitionArtifact
SourceDocumentArtifact
TranslationArtifact
PresentationArtifact
```

Tên artifact cụ thể do contract kiến trúc tương ứng định nghĩa.

### Distinction from Storage Module

```text
Artifact Store
    → runtime object lifecycle, immutable artifacts, temporary reuse

Storage Module
    → durable persistence capability, snapshots, retention, recovery,
      schema evolution và persistence contract
```

Artifact Store không phải repository nghiệp vụ và không mặc định là durable storage.

### Related Documents

- `CACHE_POLICY.md`
- `MEMORY_MODEL.md`
- `RESOURCE_LIFECYCLE.md`

---

## 9.3 Resource Manager

### Purpose

Quản lý lifecycle vật lý của resource được Runtime sử dụng.

Resource Manager hiện là logical responsibility, chưa bắt buộc thành standalone module trong MVP.

### Responsibilities

- resource registration;
- ownership transfer;
- lease tracking;
- disposal eligibility check;
- physical disposal;
- cleanup retry;
- shutdown cleanup ordering;
- phát hiện resource leak.

### Resource Types

- memory buffer;
- temporary file;
- native handle;
- GPU resource;
- provider connection;
- process handle;
- artifact backing storage.

### Rules

- Mất logical authority không đồng nghĩa có thể dispose ngay.
- Artifact chỉ được dispose khi không còn owner hoặc lease.
- Cleanup failure phải observable.
- Resource không được tồn tại vô hạn chỉ vì cleanup từng thất bại.

### Related Documents

- `RESOURCE_LIFECYCLE.md`
- `MEMORY_MODEL.md`

---

# 10. Integration Components

## 10.1 Event Bus

### Purpose

Phân phối typed event giữa các thành phần mà không chuyển ownership hoặc trực tiếp điều phối pipeline.

### Responsibilities

- publish;
- subscribe;
- dispatch;
- ordering theo scope đã định;
- subscriber failure isolation;
- event tracing;
- duplicate/stale metadata support;
- bounded progress event handling.

### Rules

Event Bus không:

- thay Runtime Control;
- thay Scheduler;
- thay Query Interface;
- tự động tạo chuỗi pipeline;
- chứa secret hoặc large payload;
- mutate state owner trực tiếp.

### Related Document

- `../core/EVENT_BUS.md`
- `../core/EVENT_STANDARD.md` nếu tồn tại trong cấu trúc dự án

---

## 10.2 Provider Manager

### Purpose

Quản lý provider capability, provider instance và runtime availability.

### Responsibilities

- provider registration;
- capability lookup;
- lifecycle;
- health state;
- concurrency limit;
- rate-limit state;
- provider selection input;
- provider fallback availability;
- mapping provider instance với secret reference;
- shutdown và cleanup.

### Rules

Provider Manager không:

- chứa business policy dịch hoặc OCR;
- trực tiếp quyết định retry;
- phát tán raw provider DTO;
- trả secret value cho component không có quyền;
- tự commit provider result.

Provider selection về mặt business thuộc module hoặc policy sở hữu use case; Provider Manager cung cấp runtime availability và execution access.

### Related Documents

- `RUNTIME_CONFIG.md`
- `RETRY_POLICY.md`
- `ERROR_MODEL.md`
- Provider Architecture trong giai đoạn sau nếu có

---

## 10.3 Secret Access Boundary

### Purpose

Cấp quyền sử dụng secret mà không phát tán secret qua runtime contract.

Secret Access Boundary thường thuộc Infrastructure hoặc Platform, không nhất thiết là Runtime Component độc lập.

### Responsibilities

- resolve secret reference;
- enforce access scope;
- sử dụng secure storage;
- hỗ trợ credential rotation;
- redaction trong logs và errors;
- không expose plain-text secret vượt quá adapter cần sử dụng.

### Rules

- Event không chứa secret.
- WorkItem không chứa secret.
- Configuration snapshot chỉ chứa secret reference.
- Provider Adapter chỉ nhận quyền truy cập tối thiểu cần thiết.

---

# 11. Observability Component

## 11.1 Runtime Observability

### Purpose

Cung cấp khả năng quan sát Runtime mà không làm thay đổi correctness hoặc làm lộ user content.

### Responsibilities

- metrics;
- traces;
- structured logs;
- runtime events;
- bounded recent-event buffer;
- diagnostic snapshots;
- queue and scheduler telemetry;
- revision and WorkItem timeline;
- provider health telemetry;
- resource leak indicators;
- startup và shutdown diagnostics.

### Correlation Model

```text
ApplicationInstanceId
    ↓
SessionId
    ↓
RevisionId
    ↓
WorkItemId
    ↓
AttemptId
```

### Privacy Rule

```text
No Content by Default
```

Standard telemetry không chứa:

- screenshot;
- OCR text;
- source text;
- translated text;
- prompt;
- source URL;
- window title;
- credential;
- provider request body.

### Rules

- Telemetry export không được block Runtime Control.
- Telemetry failure không được thay đổi terminal outcome.
- Progress event phải được throttle.
- Diagnostic content có user data chỉ được tạo trong chế độ explicit và phải được redaction.

### Related Document

- `RUNTIME_OBSERVABILITY.md`
- `PERFORMANCE_MODEL.md`
- `ERROR_MODEL.md`

---

# 12. Business Module Boundary

Runtime không được tạo các component kiểu:

```text
Capture Runtime
Recognition Runtime
Translation Runtime
Presentation Runtime
Storage Runtime
```

chỉ để phản chiếu tên Business Module.

Business Module sở hữu:

- business state;
- business rule;
- business contract;
- business data;
- business result semantics.

Runtime sở hữu:

- execution state;
- scheduling;
- attempt;
- authority;
- cancellation;
- retry;
- artifact lifecycle;
- runtime observability.

Quan hệ đúng:

```text
Runtime Worker
    ↓ invokes public contract
Business Module
    ↓ produces result
Runtime Control
    ↓ validates authority
Artifact Store / Presentation Commit
```

---

# 13. Runtime Interaction Model

```text
Application Command
        ↓
Runtime Control
        ↓ creates logical work
Scheduler
        ↓ admits
Work Queue
        ↓ dispatches
Worker Execution
        ↓ invokes module/provider contract
Temporary Result
        ↓ completion command
Runtime Control
        ↓ validates authority
Accepted Artifact Publication
        ↓
Downstream Scheduling or Presentation Commit
```

Event Bus có thể phát notification ở các bước phù hợp nhưng không sở hữu flow trên.

---

# 14. Component Ownership Summary

| Component | Primary ownership |
|---|---|
| Application Bootstrap | Startup sequence và initial ownership transfer |
| Runtime Configuration | Active configuration snapshot và version |
| Runtime Control | Runtime state, revision authority, WorkItem logical state, accepted outcome |
| Session Runtime | Session-scoped execution metadata |
| Authority Validator | Commit-authority decision |
| Scheduler | Admission decision |
| Work Queues | Bounded queued-work position |
| Worker Execution | Physical execution của một Attempt |
| Cancellation Coordinator | Cancellation propagation |
| Retry Coordinator | Retry decision và creation của new Attempt |
| Revision Store | Revision runtime metadata |
| Artifact Store | Artifact registry, metadata và runtime retention |
| Resource Manager | Physical resource lifecycle |
| Event Bus | Event distribution |
| Provider Manager | Provider runtime availability và lifecycle |
| Secret Access Boundary | Controlled secret resolution |
| Runtime Observability | Runtime telemetry và diagnostic state |

---

# 15. Logical Responsibility vs Standalone Component

Không phải mọi mục trong tài liệu này đều phải trở thành component độc lập trong MVP.

## Likely Standalone Runtime Components

- Runtime Control;
- Runtime Configuration;
- Scheduler;
- Work Queues;
- Revision Store;
- Artifact Store;
- Worker Execution;
- Event Bus;
- Runtime Observability;
- Provider Manager.

## May Remain Internal Responsibilities in MVP

- Session Runtime;
- Authority Validator;
- Cancellation Coordinator;
- Retry Coordinator;
- Resource Manager;
- Secret Access Boundary.

Việc tách thành implementation component riêng chỉ được quyết định sau khi Technology Stack và Process Topology được chốt.

---

# 16. Lifetime Model

## 16.1 Application Lifetime

```text
Bootstrap
Runtime Configuration
Runtime Control
Scheduler
Work Queues
Revision Store
Artifact Store
Provider Manager
Event Bus
Runtime Observability
```

## 16.2 Session Lifetime

```text
Session Runtime
Session cancellation scope
Current revision metadata
Session-scoped artifact ownership
Session configuration reference
```

## 16.3 Revision Lifetime

```text
Revision metadata
Logical WorkItems
Accepted ArtifactRefs
Revision authority
Revision cancellation scope
```

## 16.4 Attempt Lifetime

```text
AttemptId
Worker execution context
Artifact leases
Provider request
Temporary resources
Execution outcome
```

Resource physical lifetime có thể dài hơn logical Attempt lifetime trong thời gian drain hoặc cleanup.

---

# 17. Threading and Process Boundary

Runtime Component không tự động sở hữu dedicated thread.

Execution context logic gồm:

```text
UI Context
Runtime Control Context
Observation/Capture Context
CPU Worker Pool
Provider I/O Context
GPU Context
Optional Isolated Process
```

Quy tắc:

- Runtime Control là single logical writer, không nhất thiết là một OS thread riêng.
- Worker pool không mutate Runtime Control state.
- Queue không bị component ngoài thao tác trực tiếp.
- Provider callback chỉ gửi completion command.
- UI commit diễn ra trên UI Context sau authority validation.
- Process boundary được xác định trong `PROCESS_TOPOLOGY.md`, không phải tài liệu này.

### Related Document

- `THREADING_MODEL.md`

---

# 18. Failure Isolation

Failure được cô lập theo scope:

```text
Attempt Failure
    ↓
WorkItem Failure or Retry Decision
    ↓
Revision Degradation or Failure
    ↓
Session remains active when recoverable
```

Một provider hoặc worker failure không mặc định làm toàn Runtime dừng.

Runtime chỉ chuyển sang fatal shutdown khi:

- invariant cốt lõi bị phá;
- Runtime Control không còn đáng tin cậy;
- Artifact/Resource ownership không thể bảo toàn;
- configuration hoặc security boundary gây trạng thái không an toàn;
- tiếp tục chạy có nguy cơ làm sai dữ liệu hoặc side effect.

### Related Document

- `ERROR_MODEL.md`

---

# 19. Startup and Shutdown

## Startup Order

Startup order được định nghĩa bởi `BOOT_SEQUENCE.md`, nhìn chung phải đảm bảo:

```text
Configuration
    ↓
Observability foundation
    ↓
Storage/Infrastructure dependencies required for runtime
    ↓
Event Bus
    ↓
Artifact and Revision state
    ↓
Provider Manager
    ↓
Scheduler and queues
    ↓
Runtime Control activation
    ↓
Accept new session
```

## Shutdown Order

```text
Stop new admission
    ↓
Revoke authority / cancel active work
    ↓
Remove queued work
    ↓
Drain running attempts
    ↓
Release leases
    ↓
Dispose session and revision resources
    ↓
Stop providers and workers
    ↓
Flush bounded diagnostics
    ↓
Dispose runtime infrastructure
```

Shutdown chi tiết phải thống nhất giữa:

- `BOOT_SEQUENCE.md`;
- `CANCELLATION.md`;
- `RESOURCE_LIFECYCLE.md`;
- `THREADING_MODEL.md`;
- `RUNTIME_OBSERVABILITY.md`.

---

# 20. Dependency Rules

1. Runtime Component chỉ phụ thuộc public contract hoặc runtime abstraction.
2. Runtime Control không phụ thuộc provider implementation.
3. Worker không import Runtime Control implementation.
4. Scheduler không gọi Business Module trực tiếp.
5. Event Bus không mutate state owner.
6. Artifact Store không sở hữu business persistence policy.
7. Storage Module không quản lý runtime queue hoặc revision authority.
8. Provider Adapter không tự retry.
9. UI không gọi worker hoặc provider trực tiếp.
10. Secret không vượt qua boundary dưới dạng event payload hoặc WorkItem field.
11. Component ngang hàng không thao tác queue, state hoặc resource nội bộ của nhau.
12. Mọi ownership transfer phải explicit.
13. Deep import vào implementation nội bộ bị cấm.
14. Process boundary không được làm thay đổi contract semantics.
15. Runtime correctness không phụ thuộc telemetry availability.

---

# 21. Runtime Invariants

1. Runtime Control là single logical writer.
2. Current revision có quyền ưu tiên cao nhất.
3. Scheduler là nơi duy nhất quyết định admission.
4. Worker không tạo downstream work.
5. Worker và Provider Adapter không tự retry.
6. Queue và concurrency luôn bounded.
7. Large payload chỉ truyền qua ArtifactRef.
8. Artifact đã publish là immutable.
9. Mỗi WorkItem chỉ chấp nhận một terminal outcome.
10. Retry tạo AttemptId mới.
11. Late attempt không overwrite accepted outcome mới hơn.
12. Stale result không được commit.
13. Cancellation không mặc định là failure.
14. Artifact chỉ dispose khi không còn owner hoặc lease.
15. Runtime state và physical resource lifetime được quản lý tách biệt.
16. Business Module ownership không bị Runtime chiếm đoạt.
17. Storage Module và Artifact Store là hai boundary khác nhau.
18. UI không bị block bởi worker, provider hoặc telemetry.
19. Shutdown dừng admission trước cleanup.
20. Telemetry failure không phá runtime correctness.

---

# 22. Related Documents

| Document | Relationship |
|---|---|
| `BOOT_SEQUENCE.md` | Startup, activation và shutdown sequence |
| `RUNTIME_CONFIG.md` | Configuration ownership và snapshot |
| `PIPELINE_ORCHESTRATION.md` | Quyền điều phối stage và business workflow boundary |
| `PIPELINE_RUNTIME.md` | Revision, WorkItem, Attempt và completion flow |
| `SCHEDULER.md` | Admission policy |
| `WORK_QUEUE.md` | Bounded queue và queued-work lifecycle |
| `CANCELLATION.md` | Cancellation scope và propagation |
| `RETRY_POLICY.md` | Retry eligibility và new Attempt |
| `CACHE_POLICY.md` | Artifact reuse và cache promotion |
| `MEMORY_MODEL.md` | Revision Store, Artifact Store và Artifact Lease |
| `THREADING_MODEL.md` | Execution context và concurrency boundary |
| `RESOURCE_LIFECYCLE.md` | Ownership transfer, lease và disposal |
| `PERFORMANCE_MODEL.md` | Useful-result performance và overload policy |
| `ERROR_MODEL.md` | RuntimeError và terminal outcome |
| `RUNTIME_OBSERVABILITY.md` | Metrics, traces, logs và diagnostics |

---

# 23. Completion Criteria

`RUNTIME_COMPONENTS.md` được xem là đồng bộ khi:

- mọi Runtime Component có ownership rõ ràng;
- Runtime Component không trùng với Business Module;
- Runtime Control, Scheduler và Worker có boundary riêng;
- Revision, WorkItem và Attempt được sử dụng nhất quán;
- Artifact Store được phân biệt rõ với Storage Module;
- retry và cancellation thuộc Runtime Control policy;
- queue, concurrency và resource đều bounded;
- lifecycle application, session, revision và attempt được mô tả;
- startup và shutdown không mâu thuẫn với tài liệu chi tiết;
- logical responsibility và standalone implementation được phân biệt;
- dependency không tạo vòng;
- terminology thống nhất với toàn bộ thư mục `runtime/`.

---

# 24. Summary

CRAI Runtime được tổ chức quanh một nguyên tắc trung tâm:

```text
Runtime Control owns authority.
Scheduler owns admission.
Workers own execution.
Artifact Store owns runtime artifacts.
Resource Manager owns physical lifecycle.
Business Modules own business semantics.
Storage owns durable persistence capability.
```

Tài liệu này là bản đồ component cấp cao. Mọi hành vi chi tiết phải được định nghĩa trong tài liệu runtime chuyên biệt tương ứng và không được làm sai các ownership boundary nêu trên.
