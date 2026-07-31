# Capture Module Design

## Purpose

Tài liệu này mô tả thiết kế nội bộ của Capture Module trong CRAI.

Capture Module chịu trách nhiệm kết nối với nguồn nội dung, thu nhận dữ liệu đầu vào và chuẩn hóa dữ liệu đó thành `CaptureFrame` để chuyển sang Recognition Module.

Tài liệu này tập trung vào:

- Thành phần nội bộ của module.
- Trách nhiệm của từng thành phần.
- Luồng xử lý capture.
- Quyền sở hữu dữ liệu và tài nguyên.
- Quan hệ với Runtime.
- Quan hệ với Capture Provider.
- Boundary giữa Capture và các module khác.

Tài liệu này không định nghĩa chi tiết public API, event contract hoặc state transition đầy đủ. Các nội dung đó được mô tả trong:

```text
API.md
EVENTS.md
STATES.md
```

---

# Module Responsibility

Capture Module chịu trách nhiệm:

- Khởi tạo capture source.
- Xác thực source có thể sử dụng.
- Quản lý quyền truy cập nguồn.
- Thực hiện capture theo yêu cầu.
- Thực hiện capture liên tục khi được cấu hình.
- Chuẩn hóa dữ liệu đầu vào.
- Gắn runtime context và source metadata.
- Kiểm soát frame lifetime.
- Xử lý backpressure.
- Hủy capture khi context không còn hợp lệ.
- Giải phóng source và tài nguyên liên quan.

Capture Module không chịu trách nhiệm:

- Phát hiện nội dung có thay đổi.
- Phát hiện cuộn trang.
- Phát hiện đổi trang.
- OCR.
- Phân tích bố cục.
- Phát hiện khung thoại.
- Chuẩn hóa văn bản.
- Dịch nội dung.
- Hiển thị kết quả.
- Lưu lịch sử đọc lâu dài.
- Quản lý lifecycle của Reading Session.

---

# Module Boundary

```text
Reading Module
      |
      | source selection
      | capture policy
      | session context
      v
+----------------------------------+
|          Capture Module          |
|----------------------------------|
| Capture Coordinator              |
| Source Manager                   |
| Permission Controller            |
| Capture Policy Engine            |
| Frame Acquirer                   |
| Frame Normalizer                 |
| Frame Lifecycle Manager          |
| Capture Health Monitor           |
+----------------------------------+
      |
      | CaptureFrame
      v
Recognition Module
```

Capture Module nhận yêu cầu từ Reading Module hoặc Runtime.

Capture Module trả về dữ liệu thô đã được chuẩn hóa về mặt định dạng và metadata.

Capture Module không diễn giải nội dung bên trong dữ liệu.

---

# Internal Components

## Capture Coordinator

### Purpose

Điều phối toàn bộ hoạt động của Capture Module.

### Responsibilities

- Tiếp nhận capture command.
- Xác định source đang hoạt động.
- Kiểm tra session và generation.
- Chọn capture policy.
- Tạo capture operation.
- Gửi work tới Scheduler.
- Nhận kết quả từ Frame Acquirer.
- Chuyển kết quả qua Frame Normalizer.
- Đăng ký frame với Frame Lifecycle Manager.
- Trả kết quả hoặc phát event.
- Điều phối cancellation.
- Điều phối shutdown.

### Ownership

Capture Coordinator sở hữu:

- Active capture operations.
- Quan hệ giữa operation và source.
- Quan hệ giữa operation và session context.
- Trạng thái điều phối cấp module.

Capture Coordinator không sở hữu:

- Native capture handle.
- Provider implementation.
- Raw frame lâu dài.
- Reading Session.

---

## Source Manager

### Purpose

Quản lý lifecycle của các capture source.

### Responsibilities

- Tạo source instance.
- Khởi tạo source.
- Kiểm tra source availability.
- Theo dõi source identity.
- Giữ primary source của session.
- Thay thế source.
- Suspend source.
- Resume source.
- Stop source.
- Giải phóng source.

### Source Types

Source Manager có thể quản lý:

```text
ScreenRegionSource
ApplicationWindowSource
DisplaySource
BrowserConnectorSource
LocalInputSource
```

### Source Identity

Mỗi source phải có:

```text
CaptureSourceId
SourceType
SourceDescriptor
SourceVersion
SourceState
```

`SourceVersion` tăng khi source được tái tạo hoặc cấu hình source thay đổi đáng kể.

Kết quả từ source version cũ không được tiếp tục đi vào pipeline.

---

## Permission Controller

### Purpose

Quản lý quyền truy cập nguồn capture.

### Responsibilities

- Kiểm tra quyền capture màn hình.
- Yêu cầu quyền khi cần.
- Theo dõi permission state.
- Phát hiện quyền bị thu hồi.
- Chuyển lỗi nền tảng sang error model chung.
- Ngăn capture khi quyền không hợp lệ.

### Rules

- Không tự động mở rộng phạm vi capture.
- Không tự động chuyển sang full display.
- Không tiếp tục capture khi quyền đã bị thu hồi.
- Không yêu cầu quyền nhiều lần liên tục.
- Permission prompt phải gắn với hành động rõ ràng của người dùng.

---

## Capture Policy Engine

### Purpose

Quyết định cách capture được thực hiện trong phạm vi policy đã cấu hình.

### Responsibilities

- Chọn capture mode.
- Xác định capture interval.
- Áp dụng deadline.
- Áp dụng priority.
- Áp dụng backpressure strategy.
- Xác định frame replacement policy.
- Điều chỉnh tần suất theo runtime pressure.
- Tôn trọng trạng thái session.

### Inputs

```text
CapturePolicy
RuntimeResourceState
SessionState
SourceCapability
QueuePressure
ConfigSnapshot
```

### Outputs

```text
EffectiveCapturePolicy
```

### Notes

Capture Policy Engine không xác định nội dung đã thay đổi hay chưa.

Nó chỉ quyết định khi nào và bằng cách nào nên yêu cầu thêm dữ liệu.

---

## Frame Acquirer

### Purpose

Thực hiện thao tác thu nhận dữ liệu thực tế từ provider.

### Responsibilities

- Gọi Capture Provider.
- Áp dụng region.
- Áp dụng timeout.
- Nhận raw provider result.
- Kiểm tra dữ liệu tối thiểu.
- Chuyển provider failure thành capture failure.
- Tôn trọng cancellation.
- Trả dữ liệu cho Frame Normalizer.

### Inputs

```text
CaptureOperation
CaptureSourceHandle
EffectiveCapturePolicy
CancellationContext
```

### Outputs

```text
RawCaptureResult
```

### Rules

- Không retry vô hạn.
- Không giữ raw result sau khi đã bàn giao.
- Không gọi trực tiếp Recognition.
- Không tự ghi raw data xuống storage.
- Không tạo worker pool riêng ngoài Scheduler.

---

## Frame Normalizer

### Purpose

Chuẩn hóa dữ liệu từ provider thành `CaptureFrame`.

### Responsibilities

- Chuẩn hóa pixel format.
- Chuẩn hóa dimensions.
- Chuẩn hóa orientation.
- Gắn coordinate metadata.
- Gắn DPI scale.
- Gắn source metadata.
- Gắn timestamp.
- Gắn runtime context.
- Gắn config snapshot.
- Kiểm tra frame validity.

### Input

```text
RawCaptureResult
CaptureOperationContext
SourceMetadata
```

### Output

```text
CaptureFrame
```

### Normalization Rules

CaptureFrame phải mô tả rõ:

```text
Frame width
Frame height
Pixel format
Image orientation
Capture region
Coordinate space
DPI scale
Source dimensions
Capture timestamp
Source identity
Session identity
Generation identity
```

Frame Normalizer không:

- Resize tùy ý để phục vụ OCR.
- Enhance ảnh.
- Sharpen ảnh.
- Denoise ảnh.
- Crop theo bubble.
- Detect text region.

Các thao tác đó thuộc Recognition Module hoặc provider chuyên biệt phía sau.

---

## Frame Lifecycle Manager

### Purpose

Quản lý vòng đời và quyền sở hữu bộ nhớ của CaptureFrame.

### Responsibilities

- Đăng ký frame.
- Theo dõi frame owner.
- Theo dõi reference.
- Áp dụng retention policy.
- Giải phóng raw buffer.
- Loại bỏ stale frame.
- Giới hạn số frame tồn tại đồng thời.
- Hủy frame thuộc generation cũ.
- Hủy frame khi session kết thúc.

### Default Policy

```text
Raw frame is memory-only.
```

Frame phải được giải phóng khi:

- Không còn consumer.
- Hết retention window.
- Bị thay thế bởi frame mới.
- Generation thay đổi.
- Session dừng.
- Runtime shutdown.
- Resource pressure vượt ngưỡng.

### Ownership Rule

Tại mỗi thời điểm phải xác định được một owner rõ ràng của frame.

Không được truyền raw pointer hoặc mutable buffer tự do giữa các module.

---

## Capture Health Monitor

### Purpose

Theo dõi tình trạng hoạt động của Capture Module và source.

### Responsibilities

- Theo dõi capture latency.
- Theo dõi failure rate.
- Theo dõi source availability.
- Theo dõi dropped frame.
- Theo dõi reconnect attempts.
- Theo dõi permission failures.
- Theo dõi memory pressure liên quan tới frame.
- Cung cấp health status cho Runtime Coordinator.

### Health States

```text
Healthy
Degraded
Unavailable
Recovering
Stopped
```

### Notes

Health Monitor không tự restart module.

Nó cung cấp tín hiệu để Runtime Coordinator hoặc Source Manager thực hiện recovery.

---

# Supporting Components

## Capture Operation Factory

Tạo `CaptureOperation` từ request và runtime context.

Operation phải chứa tối thiểu:

```text
OperationId
JobId
RuntimeId
SessionId
GenerationId
CaptureSourceId
SourceVersion
ConfigSnapshotId
Priority
Deadline
CancellationScope
RequestedRegion
CaptureMode
```

---

## Source Capability Resolver

Xác định khả năng của source và provider.

Ví dụ:

```text
SupportsRegionCapture
SupportsWindowCapture
SupportsContinuousCapture
SupportsCursorExclusion
SupportsOccludedWindowCapture
SupportsDpiMetadata
SupportsDomSnapshot
SupportsEventTrigger
```

Capability phải được kiểm tra trước khi áp dụng policy.

---

## Capture Result Validator

Kiểm tra kết quả capture ở mức tối thiểu:

- Kích thước hợp lệ.
- Buffer tồn tại.
- Format được hỗ trợ.
- Source identity khớp.
- Source version khớp.
- Session generation còn hiệu lực.
- Kết quả chưa quá deadline.
- Operation chưa bị cancel.

Validator không đánh giá nội dung hình ảnh.

---

# Public Module Surface

Capture Module chỉ nên expose một public surface nhỏ.

Khái niệm dự kiến:

```text
CaptureService
CaptureSourceService
CaptureCapabilityQuery
CaptureHealthQuery
```

Public API chi tiết được định nghĩa trong:

```text
API.md
```

Các component nội bộ như:

```text
FrameAcquirer
FrameNormalizer
SourceManager
FrameLifecycleManager
```

không được truy cập trực tiếp từ module khác.

---

# Main Processing Flow

## On-Demand Capture Flow

```text
Capture Request
      |
      v
Capture Coordinator
      |
      v
Validate Session Context
      |
      v
Resolve Active Source
      |
      v
Resolve Effective Policy
      |
      v
Create Capture Operation
      |
      v
Submit Capture Job
      |
      v
Frame Acquirer
      |
      v
Capture Provider
      |
      v
Raw Capture Result
      |
      v
Frame Normalizer
      |
      v
Capture Result Validator
      |
      v
Frame Lifecycle Manager
      |
      v
CaptureFrame
```

---

## Continuous Capture Flow

```text
Continuous Capture Started
      |
      v
Capture Policy Engine
      |
      v
Schedule Capture Tick
      |
      v
Check Session State
      |
      v
Check Queue Pressure
      |
      +---- overloaded ----> skip or delay tick
      |
      v
Create Capture Operation
      |
      v
Acquire Frame
      |
      v
Normalize Frame
      |
      v
Replace Stale Pending Frame
      |
      v
Publish Latest CaptureFrame
      |
      v
Schedule Next Tick
```

Continuous capture phải dừng khi:

```text
Session paused
Session stopped
Generation changed
Source lost
Permission revoked
Runtime shutting down
Resource limit exceeded
```

---

## Source Replacement Flow

```text
Replace Source Requested
      |
      v
Suspend Current Capture
      |
      v
Cancel Active Operations
      |
      v
Increment Session Generation
      |
      v
Stop Old Source
      |
      v
Create New Source
      |
      v
Validate Permission
      |
      v
Initialize New Source
      |
      v
Set Primary Source
      |
      v
Resume Capture
```

Kết quả từ source cũ đến sau thời điểm replacement phải bị loại bỏ.

---

# Backpressure Design

Capture Module không nên duy trì một backlog dài các frame.

Đối với trải nghiệm đọc, dữ liệu hiện tại thường quan trọng hơn dữ liệu cũ.

Chiến lược mặc định:

```text
Latest relevant frame wins.
```

## Backpressure Actions

Khi pipeline bị chậm, Capture Module có thể:

- Không tạo capture operation mới.
- Bỏ qua capture tick.
- Thay thế pending frame cũ.
- Giảm capture frequency.
- Tạm dừng continuous capture.
- Chỉ giữ user-triggered request.
- Giảm độ phân giải nếu policy cho phép.

## Queue Recommendation

Capture Queue nên tách ít nhất:

```text
UserTriggeredQueue
ContinuousCaptureQueue
RecoveryQueue
```

Priority đề xuất:

```text
User-triggered capture
    >
Source recovery capture
    >
Continuous capture
```

---

# Concurrency Model

## Source Concurrency

Mỗi source mặc định chỉ có một active capture operation.

```text
maxConcurrentCapturePerSource = 1
```

Điều này tránh:

- Tranh chấp native handle.
- Capture dư thừa.
- Frame reorder.
- Tăng memory không cần thiết.

Provider có thể khai báo hỗ trợ concurrency lớn hơn, nhưng không nên là mặc định.

---

## Session Concurrency

MVP nên giới hạn:

```text
one primary active source per reading session
```

Một session có thể có pending source trong lúc chuyển đổi, nhưng chỉ một source được phép phát frame chính thức.

---

## Frame Concurrency

Frame phải được xem là immutable sau khi normalization hoàn tất.

Consumer không được thay đổi trực tiếp buffer hoặc metadata.

Nếu cần preprocessing, Recognition Module phải tạo representation mới hoặc sử dụng copy-on-write theo Memory Model.

---

# Threading Model

Capture Module không sở hữu global thread pool.

Các capture operation chạy thông qua Runtime Scheduler.

Provider adapter có thể sử dụng:

- Native callback.
- OS capture thread.
- Async runtime.
- Dedicated provider thread.

Nhưng các chi tiết đó phải bị che sau `CaptureProvider`.

Public module behavior phải nhất quán bất kể provider dùng threading model nào.

---

# Runtime Context

Mỗi capture operation phải mang runtime context đầy đủ.

```text
RuntimeId
SessionId
GenerationId
JobId
OperationId
CaptureSourceId
SourceVersion
ConfigSnapshotId
CorrelationId
CausationId
TraceContext
CancellationScope
```

Các identity này được dùng để:

- Tracing.
- Cancellation.
- Stale result rejection.
- Error correlation.
- Session isolation.
- Config consistency.

---

# Configuration

Capture Module nhận typed configuration từ Configuration Service.

Ví dụ cấu hình:

```yaml
capture:
  defaultMode: continuous
  defaultSourceType: screen-region
  intervalMs: 500
  timeoutMs: 1500
  maxConcurrentPerSource: 1
  maxActiveFrames: 3

  backpressure:
    strategy: latest-relevant-frame
    skipWhenBusy: true
    reduceFrequencyWhenDegraded: true

  frame:
    preferredPixelFormat: rgba8
    maxWidth: 4096
    maxHeight: 4096
    rawRetentionMs: 3000
    memoryOnly: true

  privacy:
    allowFullDisplayCapture: false
    persistRawFrames: false
    includeCursor: false
```

Capture Module không tự đọc YAML, JSON hoặc environment variable.

---

# Error Handling

## Error Translation

Provider-specific error phải được chuyển thành error chung.

Ví dụ:

```text
NativeAccessDenied
    ->
PermissionDenied
```

```text
WindowHandleInvalid
    ->
SourceLost
```

```text
ProviderDeadlineExceeded
    ->
CaptureTimeout
```

---

## Recoverable Errors

Có thể recovery:

```text
SourceTemporarilyUnavailable
CaptureTimeout
TransientProviderFailure
SourceOccluded
ResourcePressure
```

Hành vi có thể gồm:

- Retry có giới hạn.
- Backoff.
- Suspend.
- Reconnect.
- Giảm tần suất.
- Yêu cầu người dùng chọn lại source.

---

## Non-Recoverable Errors

Không nên retry tự động:

```text
PermissionDenied
UnsupportedSource
InvalidRegion
InvalidConfiguration
SourcePermanentlyClosed
PrivacyPolicyViolation
```

---

## Retry Ownership

Capture Module quyết định lỗi capture nào có thể retry.

Scheduler thực hiện delay, deadline và retry scheduling.

Capture Provider không tự retry vô hạn bên trong adapter.

---

# Cancellation Design

Cancellation có thể đến từ:

```text
Runtime shutdown
Session stop
Session pause
Generation change
Source replacement
User cancellation
Deadline exceeded
Resource pressure
Provider shutdown
```

## Propagation

```text
Cancellation Scope
      |
      v
Capture Coordinator
      |
      v
Capture Operation
      |
      v
Scheduler Job
      |
      v
Frame Acquirer
      |
      v
Capture Provider
```

Khi provider không hỗ trợ hard cancellation, kết quả trả về sau đó vẫn phải bị validator loại bỏ.

---

# Resource Ownership

## Native Source Handle

Owner:

```text
Source Manager
```

## Active Capture Operation

Owner:

```text
Capture Coordinator
```

## Raw Provider Result

Owner:

```text
Frame Acquirer
```

sau đó chuyển quyền cho:

```text
Frame Normalizer
```

## Normalized CaptureFrame

Owner ban đầu:

```text
Frame Lifecycle Manager
```

Sau đó consumer nhận reference theo contract.

## Provider Instance

Owner:

```text
Provider Registry
```

Capture Module chỉ sử dụng provider lease hoặc provider reference.

---

# Privacy and Security

Capture Module phải thực hiện các quy tắc sau:

1. Chỉ capture source được cho phép.
2. Không tự động mở rộng capture region.
3. Không capture cửa sổ khác khi source biến mất.
4. Không lưu raw frame mặc định.
5. Không log nội dung frame.
6. Không gửi frame ra ngoài module nếu contract không yêu cầu.
7. Không gửi remote provider nếu policy không cho phép.
8. Không giữ frame lâu hơn retention policy.
9. Phải hủy frame khi session hoặc generation hết hiệu lực.
10. Phải hiển thị trạng thái continuous capture ở UI.

---

# Diagnostics

Capture Module được phép ghi:

```text
OperationId
SessionId
GenerationId
CaptureSourceId
SourceType
Frame dimensions
Frame byte size
Capture duration
Queue wait duration
Dropped frame count
Retry count
Error category
Provider name
Health state
```

Không được ghi:

```text
Raw frame bytes
Screenshot
DOM content
Recognized text
Translated text
Secret
Access token
Full window title nếu có dữ liệu nhạy cảm
```

---

# Health Model

## Healthy

- Source available.
- Permission valid.
- Capture success rate bình thường.
- Latency trong budget.
- Không có memory pressure đáng kể.

## Degraded

- Capture chậm.
- Có dropped frame.
- Retry tăng.
- Source không ổn định.
- Runtime đang giảm frequency.

## Unavailable

- Source mất.
- Permission bị thu hồi.
- Provider không hoạt động.
- Không thể tạo frame.

## Recovering

- Đang reconnect.
- Đang yêu cầu lại quyền.
- Đang tái tạo source.

## Stopped

- Source đã dừng hoặc module đang shutdown.

---

# Dependency Rules

Capture Module được phép phụ thuộc vào:

```text
Runtime Context Contracts
Scheduler Contracts
Event Contracts
Configuration Contracts
Diagnostics Contracts
Provider Contracts
Common Error Contracts
```

Capture Module không được phụ thuộc trực tiếp vào:

```text
Recognition Module implementation
Translation Module
Presentation Module
Storage implementation
OCR provider implementation
Translation provider implementation
Desktop UI implementation
Browser SDK implementation
```

Platform-specific capture code phải nằm sau Capture Provider boundary.

---

# Interaction with Reading Module

Reading Module chịu trách nhiệm:

- Tạo Reading Session.
- Chọn reading mode.
- Chọn hoặc yêu cầu source.
- Pause/resume session.
- Quyết định khi session kết thúc.
- Quản lý page/chapter semantics.

Capture Module chịu trách nhiệm:

- Biến source đã chọn thành dữ liệu capture.
- Giữ source hoạt động trong session.
- Dừng capture theo lifecycle signal.

Capture Module không tự tạo Reading Session.

---

# Interaction with Recognition Module

Capture Module cung cấp:

```text
CaptureFrame
```

Recognition Module quyết định:

- Frame có ổn định hay không.
- Frame có trùng hay không.
- Có cần OCR hay không.
- Vùng nào cần nhận diện.
- Nội dung thuộc loại nào.
- Preprocessing nào cần áp dụng.

Capture Module không gọi trực tiếp OCR.

---

# Interaction with Presentation Module

Presentation Module có thể cung cấp capture region selection UI.

Tuy nhiên:

- Presentation sở hữu UI.
- Capture sở hữu source descriptor và region validation.
- Capture không render selection overlay.
- Presentation không thao tác trực tiếp native capture handle.

---

# Interaction with Storage

Capture Module không lưu raw frame mặc định.

Có thể sử dụng storage thông qua contract cho:

- Persist source preference.
- Persist region metadata.
- Persist non-sensitive capture settings.

Raw frame persistence chỉ được phép khi:

- Có feature rõ ràng.
- Người dùng chủ động bật.
- Privacy policy cho phép.
- Retention được xác định.
- Dữ liệu được bảo vệ phù hợp.

---

# Provider Boundary

```text
Capture Module
      |
      v
CaptureProvider Contract
      |
      +-- Desktop Screen Provider
      +-- Desktop Window Provider
      +-- Browser Connector Provider
      +-- Local File Provider
```

Capture Provider chịu trách nhiệm:

- Giao tiếp nền tảng.
- Native API.
- Provider-specific source handle.
- Provider-specific result.
- Provider capability.

Capture Module chịu trách nhiệm:

- Policy.
- Session integration.
- Scheduling.
- Normalization.
- Lifecycle coordination.
- Error translation.
- Backpressure.
- Frame ownership.

---

# Internal Component Dependency

```text
Capture Coordinator
    |
    +--> Source Manager
    |
    +--> Permission Controller
    |
    +--> Capture Policy Engine
    |
    +--> Capture Operation Factory
    |
    +--> Frame Acquirer
    |       |
    |       +--> CaptureProvider
    |
    +--> Frame Normalizer
    |
    +--> Capture Result Validator
    |
    +--> Frame Lifecycle Manager
    |
    +--> Capture Health Monitor
```

Dependency phải đi theo một chiều.

Các component cấp thấp không được gọi ngược Capture Coordinator bằng direct reference.

Kết quả hoặc trạng thái bất đồng bộ phải trả qua:

- Callback contract.
- Future/Promise.
- Event.
- Scheduler completion signal.

---

# Suggested Internal Layout

Cấu trúc dưới đây chỉ mang tính khái niệm, không khóa ngôn ngữ hoặc framework:

```text
capture/
├── application/
│   ├── capture-coordinator
│   ├── capture-operation-factory
│   └── capture-policy-engine
│
├── domain/
│   ├── capture-source
│   ├── capture-operation
│   ├── capture-frame
│   ├── capture-policy
│   └── capture-errors
│
├── services/
│   ├── source-manager
│   ├── permission-controller
│   ├── frame-acquirer
│   ├── frame-normalizer
│   ├── frame-validator
│   ├── frame-lifecycle-manager
│   └── capture-health-monitor
│
├── contracts/
│   ├── capture-service
│   ├── capture-provider
│   └── capture-events
│
└── infrastructure/
    └── provider-adapters
```

Provider adapter cụ thể có thể được đặt ngoài module nếu project structure sau này quy định provider nằm trong `04-providers` hoặc package riêng.

---

# MVP Internal Scope

MVP cần các component sau:

```text
Capture Coordinator
Source Manager
Permission Controller
Capture Policy Engine
Frame Acquirer
Frame Normalizer
Capture Result Validator
Frame Lifecycle Manager
Capture Health Monitor
```

MVP chỉ cần hỗ trợ đầy đủ:

```text
Screen Region Source
On-Demand Capture
Limited Continuous Capture
Single Active Source
Single Active Capture Per Source
Memory-Only Frame
Latest-Frame Backpressure
Session Cancellation
Generation Validation
```

---

# Deferred Internal Scope

Sau MVP có thể bổ sung:

```text
Automatic Source Discovery
Automatic Reading Area Detection
Multi-Source Coordinator
GPU Frame Pool
Zero-Copy Frame Sharing
Adaptive Resolution Controller
Browser DOM Capture Adapter
PDF Render Source
Remote Capture Source
Capture Recording
Frame Replay
Multi-Monitor Coordination
```

Các khả năng này không được làm phức tạp MVP boundary.

---

# Invariants

Capture Module phải luôn giữ các invariant sau:

1. Mỗi active source có một `CaptureSourceId`.
2. Mỗi capture result thuộc đúng một source version.
3. Mỗi frame thuộc đúng một session generation.
4. Frame bị cancel không được chuyển tiếp.
5. Frame từ source cũ không được chuyển tiếp.
6. Raw frame không được persist mặc định.
7. Mỗi native source handle có đúng một owner.
8. Capture không tự diễn giải nội dung.
9. Capture không tự thay đổi source ngoài ý định người dùng.
10. Capture backlog không được tăng không giới hạn.
11. Provider-specific error không được rò rỉ qua public boundary.
12. Cleanup phải hoàn thành khi source dừng.

---

# Testing Expectations

Thiết kế module phải cho phép kiểm thử:

## Unit Tests

- Capture policy resolution.
- Operation creation.
- Source version validation.
- Generation validation.
- Frame metadata normalization.
- Backpressure decision.
- Error translation.
- Cancellation handling.
- Retention decision.
- Health state calculation.

## Integration Tests

- Capture Coordinator với fake provider.
- Source replacement.
- Permission revoked.
- Source lost.
- Timeout.
- Provider failure.
- Continuous capture.
- Queue pressure.
- Session stop.
- Runtime shutdown.

## Stress Tests

- Capture tần suất cao.
- Frame kích thước lớn.
- Source thay đổi liên tục.
- Cancellation liên tục.
- Provider trả kết quả muộn.
- Memory pressure.
- Pipeline xử lý chậm.

## Privacy Tests

- Không ghi raw frame vào log.
- Không persist raw frame mặc định.
- Không capture ngoài region.
- Không fallback sang full display.
- Frame được giải phóng sau session.

---

# Completion Criteria

Thiết kế nội bộ của Capture Module được xem là hoàn chỉnh khi:

- Mọi component nội bộ có trách nhiệm rõ ràng.
- Public boundary nhỏ và ổn định.
- Source lifecycle có một owner duy nhất.
- Frame ownership được xác định.
- Capture operation mang đầy đủ runtime context.
- Backpressure không tạo backlog vô hạn.
- Cancellation truyền được tới provider.
- Late result bị loại bỏ.
- Provider-specific detail không rò rỉ.
- Privacy rule được thực thi ở cấp module.
- MVP không phụ thuộc vào browser connector.
- Recognition không bị trộn vào Capture.
- Thiết kế có thể kiểm thử bằng fake provider.

---

# Related Documents

```text
doc/02-modules/capture/README.md
doc/02-modules/capture/API.md
doc/02-modules/capture/EVENTS.md
doc/02-modules/capture/STATES.md

doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/MODULE_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/RUNTIME_COMPONENTS.md
doc/01-architecture/runtime/RUNTIME_CONTEXT.md
doc/01-architecture/runtime/RUNTIME_CONFIG.md
doc/01-architecture/runtime/SCHEDULER.md
doc/01-architecture/runtime/WORK_QUEUE.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/MEMORY_MODEL.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/ERROR_MODEL.md

doc/03-contracts/
doc/04-providers/
```

---

# Open Decisions

Các quyết định sau chưa cần khóa ở giai đoạn thiết kế module:

## Frame Representation

Chưa quyết định representation vật lý của frame:

```text
Owned byte buffer
Shared memory
Native texture
GPU surface
Reference-counted image
```

Public contract chỉ nên yêu cầu một abstraction ổn định, không khóa implementation cụ thể.

---

## Pixel Format

Chưa quyết định pixel format nội bộ mặc định:

```text
RGBA8
BGRA8
RGB8
Native provider format
```

MVP nên ưu tiên format dễ tích hợp và dễ kiểm thử hơn tối ưu zero-copy quá sớm.

---

## Continuous Capture Interval

Chưa khóa capture interval mặc định.

Giá trị thực tế cần được xác định qua benchmark dựa trên:

- CPU usage.
- GPU usage.
- Capture latency.
- OCR latency.
- Tốc độ cuộn trang.
- Trải nghiệm người dùng.

---

## Frame Sharing Strategy

Chưa quyết định giữa:

```text
Copy per consumer
Reference counting
Immutable shared frame
Copy-on-write
```

Quyết định này phải tuân theo `MEMORY_MODEL.md`.

---

## Browser Connector Boundary

Chưa quyết định Browser Connector sẽ:

- Là một Capture Provider.
- Là một application-level connector.
- Hay là một source adapter kết hợp giữa Capture và Recognition.

MVP không phụ thuộc vào quyết định này.

---

# Architectural Risks

## Capture Scope Expansion

Capture có nguy cơ bị mở rộng để chứa:

- Duplicate detection.
- Image preprocessing.
- Text-region detection.
- Scroll detection.
- Page-change detection.

Các chức năng này phải được giữ ngoài Capture Module.

---

## Platform Leakage

Native type như window handle, display handle hoặc OS-specific error có thể rò rỉ qua public API.

Mọi dữ liệu platform-specific phải được bao bọc trong provider contract hoặc opaque descriptor.

---

## Excessive Frame Retention

Giữ quá nhiều frame có thể gây:

- Memory pressure.
- UI lag.
- OCR backlog.
- Dữ liệu cũ tiếp tục được xử lý.
- Rủi ro riêng tư.

Frame Lifecycle Manager phải áp dụng giới hạn cứng.

---

## Hidden Provider Retry

Provider tự retry bên trong có thể phá vỡ:

- Deadline.
- Cancellation.
- Scheduler policy.
- Observability.
- Retry budget.

Retry phải được điều phối ở cấp module và runtime.

---

## Full-Screen Privacy Fallback

Không được tự động chuyển sang full-display capture khi window hoặc region source thất bại.

Fallback này chỉ hợp lệ khi người dùng chủ động xác nhận.

---

## Source Identity Instability

Một cửa sổ có thể:

- Đóng rồi mở lại.
- Đổi native handle.
- Đổi kích thước.
- Di chuyển sang display khác.
- Thay đổi DPI.

`CaptureSourceId` và `SourceVersion` phải giúp phân biệt source logic với native handle hiện tại.

---

# Design Trade-offs

## Freshness over Completeness

Capture ưu tiên frame mới và phù hợp hơn việc xử lý đầy đủ mọi frame.

```text
Freshness > frame completeness
```

Điều này phù hợp với Reading Session vì người dùng thường cần bản dịch của nội dung đang nhìn thấy, không phải nội dung đã cuộn qua.

---

## Simplicity over Zero-Copy

MVP ưu tiên:

- Ownership rõ ràng.
- Dễ kiểm thử.
- Dễ giải phóng bộ nhớ.
- Ít phụ thuộc nền tảng.

Zero-copy và GPU-native sharing chỉ nên được triển khai khi benchmark chứng minh cần thiết.

---

## Explicit Source over Automatic Discovery

MVP ưu tiên người dùng chọn source rõ ràng.

Automatic source discovery có thể cải thiện trải nghiệm sau này nhưng làm tăng:

- Rủi ro capture nhầm.
- Complexity.
- Platform-specific behavior.
- Privacy concerns.

---

## Central Scheduler over Private Workers

Capture sử dụng Runtime Scheduler thay vì duy trì worker system riêng.

Điều này giúp thống nhất:

- Priority.
- Cancellation.
- Deadline.
- Retry.
- Resource budget.
- Observability.

---

# Traceability

Các yêu cầu của Capture Module phải truy ngược được tới kiến trúc cấp cao.

| Capture requirement | Architecture source |
|---|---|
| Capture chỉ thu nhận dữ liệu | `MODULE_MAP.md` |
| Raw frame memory-only | `MEMORY_MODEL.md` |
| Không tạo backlog vô hạn | `WORK_QUEUE.md` |
| Latest relevant frame wins | `PIPELINE_RUNTIME.md` |
| Job mang Generation ID | `RUNTIME_CONTEXT.md` |
| Late result bị loại bỏ | `CANCELLATION.md` |
| Provider được thay thế | `MODULE_DEPENDENCY.md` |
| Không ghi nội dung nhạy cảm | `RUNTIME_OBSERVABILITY.md` |
| Config là typed snapshot | `RUNTIME_CONFIG.md` |
| Source cleanup có owner | `RESOURCE_LIFECYCLE.md` |

Khi tài liệu kiến trúc nguồn thay đổi, Capture Module phải được rà soát lại.

---

# Change Impact

Các thay đổi sau được xem là thay đổi lớn đối với Capture Module:

- Thêm một source type mới.
- Thay đổi ownership của `CaptureFrame`.
- Cho phép nhiều primary source trong một session.
- Thay đổi coordinate-space model.
- Cho phép persist raw frame mặc định.
- Thay đổi backpressure strategy mặc định.
- Cho phép provider-specific type đi qua public API.
- Di chuyển preprocessing vào Capture Module.
- Thay đổi quan hệ giữa Capture và Reading Module.

Những thay đổi này phải được kiểm tra với:

```text
MODULE_DEPENDENCY.md
DATA_FLOW.md
RUNTIME_CONTEXT.md
MEMORY_MODEL.md
CANCELLATION.md
RESOURCE_LIFECYCLE.md
```

---

# Review Checklist

Trước khi chấp nhận một thay đổi trong Capture Module, cần kiểm tra:

## Boundary

- Thay đổi có thực sự thuộc trách nhiệm thu nhận dữ liệu không?
- Có đưa Recognition logic vào Capture không?
- Có làm public API phụ thuộc nền tảng không?

## Context

- Operation có đầy đủ Session ID và Generation ID không?
- Source ID và Source Version có được kiểm tra không?
- Config Snapshot có được giữ nhất quán không?

## Resource

- Native resource có owner rõ ràng không?
- Raw frame được giải phóng ở đâu?
- Có khả năng tạo backlog không giới hạn không?

## Cancellation

- Operation có thể bị hủy không?
- Late result có bị loại bỏ không?
- Provider không hỗ trợ hard cancellation được xử lý thế nào?

## Privacy

- Có capture ngoài phạm vi người dùng chọn không?
- Có lưu hoặc log raw frame không?
- Có gửi frame tới remote service không?

## Recovery

- Lỗi nào có thể retry?
- Retry do thành phần nào điều phối?
- Source mất có dẫn tới fallback không an toàn không?

## Testing

- Có thể kiểm thử bằng fake provider không?
- Có test source replacement không?
- Có test generation change không?
- Có test memory release không?

---

# Document Status

```text
Status: Draft
Architecture level: Module Design
Implementation binding: None
MVP applicability: Required
```

Tài liệu chuyển sang trạng thái `Accepted` khi:

- `API.md` hoàn thành.
- `EVENTS.md` hoàn thành.
- `STATES.md` hoàn thành.
- Các contract chính được đối chiếu với `doc/03-contracts/`.
- Capture Provider boundary được đối chiếu với `doc/04-providers/`.
- Không còn xung đột với Runtime Architecture.

---

# Next Documents

Thứ tự hoàn thiện Capture Module:

```text
1. README.md       Completed
2. MODULE.md       Completed
3. API.md          Next
4. EVENTS.md
5. STATES.md
```

Tài liệu tiếp theo:

```text
doc/02-modules/capture/API.md
```