# Capture Module

## Purpose

Capture Module chịu trách nhiệm thu nhận dữ liệu đầu vào từ nguồn nội dung mà người dùng đang đọc.

Module chuyển dữ liệu từ nguồn bên ngoài thành một biểu diễn đầu vào thống nhất để các bước xử lý tiếp theo có thể sử dụng.

Capture không xác định nội dung có thay đổi hay không, không nhận diện văn bản và không thực hiện dịch thuật.

---

## Product Context

Trong CRAI, người dùng không chủ động tải từng ảnh lên để dịch.

Người dùng bắt đầu một Reading Session và tiếp tục đọc nội dung trên:

- Trình duyệt.
- Cửa sổ ứng dụng.
- Vùng màn hình.
- Ảnh hoặc tài liệu cục bộ.

Capture Module cung cấp dữ liệu đầu vào liên tục hoặc theo yêu cầu cho Reading Session mà không làm gián đoạn trải nghiệm đọc.

---

## Responsibilities

Capture Module chịu trách nhiệm:

- Kết nối với nguồn nội dung.
- Kiểm tra nguồn có khả dụng hay không.
- Thu nhận frame từ màn hình hoặc cửa sổ.
- Thu nhận nội dung từ vùng màn hình được chọn.
- Nhận dữ liệu từ browser connector khi được hỗ trợ.
- Nhận ảnh hoặc tài liệu cục bộ khi được hỗ trợ.
- Chuẩn hóa kết quả thành Capture Frame.
- Gắn metadata nguồn vào frame.
- Quản lý lifecycle của capture source.
- Tôn trọng cancellation và resource limits.
- Giải phóng tài nguyên khi session kết thúc.

---

## Non-Responsibilities

Capture Module không chịu trách nhiệm:

- Xác định frame có ổn định hay không.
- Phát hiện cuộn trang.
- Phát hiện đổi trang hoặc đổi chapter.
- So sánh frame trùng lặp.
- Phân loại truyện tranh hoặc tiểu thuyết.
- Phát hiện khung thoại.
- Nhận diện bố cục.
- OCR.
- Trích xuất DOM thành nội dung đọc.
- Chuẩn hóa văn bản.
- Dịch nội dung.
- Hiển thị bản dịch.
- Quản lý Reading Session.
- Lưu lịch sử đọc lâu dài.

Các nhiệm vụ này thuộc về module hoặc runtime component khác.

---

## Module Boundary

```text
Reading Session
      |
      | Capture request
      v
+-------------------------+
|     Capture Module      |
|-------------------------|
| Source connection       |
| Permission validation   |
| Frame acquisition       |
| Input normalization     |
| Resource cleanup        |
+-------------------------+
      |
      | CaptureFrame
      v
Recognition Pipeline
```

Capture Module chỉ quản lý quá trình thu nhận dữ liệu.

Việc quyết định frame có cần được xử lý tiếp hay không nằm ngoài module này.

---

## Supported Source Types

### Screen Region

Thu nhận nội dung từ một vùng màn hình do người dùng hoặc hệ thống xác định.

Phù hợp với:

- Website truyện tranh.
- Website tiểu thuyết.
- Ứng dụng đọc truyện.
- Trình đọc PDF.
- Nội dung không có connector chuyên dụng.

---

### Application Window

Thu nhận nội dung từ một cửa sổ ứng dụng cụ thể.

Phù hợp khi CRAI cần:

- Theo dõi đúng cửa sổ đọc.
- Hạn chế chụp nội dung ngoài phạm vi.
- Giữ vùng capture ổn định khi cửa sổ di chuyển.

---

### Display

Thu nhận toàn bộ một màn hình.

Nguồn này chỉ nên dùng khi:

- Không thể xác định cửa sổ.
- Người dùng chủ động lựa chọn.
- Runtime cần hỗ trợ chế độ capture thủ công.

Display capture không nên là lựa chọn mặc định vì phạm vi dữ liệu quá rộng.

---

### Browser Connector

Nhận dữ liệu từ browser extension hoặc browser integration.

Browser Connector có thể cung cấp:

- Screenshot của vùng đọc.
- Vị trí viewport.
- Kích thước trang.
- DOM snapshot.
- Text nodes.
- Thông tin scroll.

Capture Module chỉ tiếp nhận và chuẩn hóa dữ liệu.

Việc phân tích DOM hoặc nội dung văn bản thuộc Recognition Module.

---

### Local Input

Nhận dữ liệu cục bộ như:

- Ảnh.
- Thư mục ảnh.
- Tệp tài liệu được hỗ trợ.
- Clipboard image.

Local Input là nguồn bổ sung, không phải trải nghiệm chính của CRAI MVP.

---

## Capture Modes

### On-Demand Capture

Capture được thực hiện khi có yêu cầu cụ thể.

Ví dụ:

- Người dùng bấm dịch.
- Reading Session yêu cầu frame đầu tiên.
- Runtime yêu cầu kiểm tra lại vùng đọc.

---

### Continuous Capture

Capture được thực hiện theo chu kỳ trong khi Reading Session hoạt động.

Continuous Capture phải tuân thủ:

- Capture interval.
- Resource budget.
- Session state.
- Backpressure.
- Cancellation.
- Application visibility.

Capture Module không tự quyết định frame nào cần OCR.

---

### Event-Triggered Capture

Capture được thực hiện sau một tín hiệu từ connector hoặc runtime.

Ví dụ:

- Viewport thay đổi.
- Browser phát hiện scroll kết thúc.
- Cửa sổ đọc được kích hoạt lại.
- Người dùng chuyển trang.

Capture Module tiếp nhận trigger nhưng không sở hữu logic xác định page change.

---

## Primary Inputs

Capture Module có thể nhận:

```text
CaptureRequest
CaptureSourceDescriptor
CaptureRegion
CapturePolicy
SessionContext
RuntimeContext
CancellationContext
ConfigSnapshotId
```

Các contract chính thức sẽ được định nghĩa trong:

```text
doc/03-contracts/
```

---

## Primary Outputs

Capture Module tạo ra:

```text
CaptureFrame
CaptureSourceStatus
CaptureFailure
CaptureCapability
```

### CaptureFrame

CaptureFrame đại diện cho một đơn vị dữ liệu đầu vào được thu nhận tại một thời điểm.

CaptureFrame có thể chứa:

- Pixel data.
- Image reference.
- DOM snapshot reference.
- Source metadata.
- Capture timestamp.
- Source dimensions.
- Capture region.
- Coordinate space.
- Runtime context identifiers.

CaptureFrame không chứa kết quả OCR hoặc bản dịch.

---

## Source Identity

Mỗi source đang hoạt động phải có một định danh ổn định:

```text
CaptureSourceId
```

Định danh này được dùng để:

- Liên kết frame với nguồn.
- Theo dõi lifecycle.
- Phân biệt nhiều cửa sổ hoặc màn hình.
- Hủy công việc thuộc source cũ.
- Phát hiện source đã bị thay thế.

Một Reading Session có thể thay đổi source, nhưng tại một thời điểm chỉ nên có một primary capture source.

---

## Coordinate Space

Capture Module phải mô tả rõ hệ tọa độ của dữ liệu đầu ra.

Ví dụ:

```text
Display coordinates
Window coordinates
Content coordinates
Viewport coordinates
Capture-frame coordinates
```

Mỗi CaptureFrame phải có đủ metadata để module phía sau có thể chuyển đổi tọa độ.

Capture Module không được giả định rằng:

- Tọa độ màn hình bằng tọa độ ảnh.
- Tỷ lệ DPI luôn là `1.0`.
- Cửa sổ luôn ở cùng vị trí.
- Browser zoom luôn là `100%`.

---

## Privacy Model

Capture là module có rủi ro riêng tư cao nhất vì nó có thể tiếp cận nội dung trên màn hình người dùng.

Các quy tắc bắt buộc:

- Chỉ capture source mà người dùng đã chọn hoặc cho phép.
- Không âm thầm chuyển sang cửa sổ khác.
- Không capture ngoài vùng được cấu hình.
- Không lưu raw frame mặc định.
- Không đưa raw frame vào log.
- Không gửi frame tới remote provider nếu chưa được phép.
- Phải giải phóng frame khi không còn cần thiết.
- Phải dừng capture khi Reading Session kết thúc.
- Phải biểu thị rõ khi continuous capture đang hoạt động.

Mặc định của CRAI là:

```text
Raw capture data is memory-only.
```

---

## Lifecycle

```text
Uninitialized
      |
      v
Initializing
      |
      v
Ready
      |
      v
Capturing
      |
      +-------> Suspended
      |             |
      |             v
      +--------- Capturing
      |
      v
Stopping
      |
      v
Stopped
```

### Uninitialized

Source chưa được tạo.

### Initializing

Đang kiểm tra source, quyền truy cập và capability.

### Ready

Source đã sẵn sàng nhưng chưa thu nhận frame.

### Capturing

Source đang xử lý capture request hoặc continuous capture.

### Suspended

Capture tạm dừng nhưng source vẫn được giữ.

### Stopping

Đang hủy công việc và giải phóng tài nguyên.

### Stopped

Source không còn hoạt động.

Chi tiết state transition sẽ được mô tả trong:

```text
capture/STATES.md
```

---

## Session Relationship

Capture Module không sở hữu Reading Session.

Reading Session cung cấp context cần thiết cho Capture Module.

Mỗi frame phải liên kết tối thiểu với:

```text
RuntimeId
SessionId
GenerationId
CaptureSourceId
ConfigSnapshotId
```

Khi Generation thay đổi, kết quả capture thuộc Generation cũ có thể bị loại bỏ.

---

## Runtime Relationship

Capture Module phối hợp với:

### Session Manager

Nhận trạng thái session và lifecycle signal.

### Scheduler

Gửi hoặc nhận Capture Job.

### Event Bus

Phát lifecycle event và capture result event.

### Configuration Service

Nhận typed configuration snapshot.

### Resource Manager

Tuân thủ giới hạn memory, concurrency và capture frequency.

### Diagnostics Runtime

Ghi nhận timing, failure và health metadata.

---

## Dependencies

Capture Module có thể phụ thuộc vào:

- Runtime contracts.
- Session context contracts.
- Capture provider contracts.
- Scheduler contracts.
- Event contracts.
- Configuration contracts.
- Diagnostics contracts.

Capture Module không được phụ thuộc trực tiếp vào:

- Recognition implementation.
- OCR implementation.
- Translation implementation.
- Presentation implementation.
- Provider-specific SDK từ public module boundary.
- Storage implementation.

---

## Provider Relationship

Capture Module sử dụng Capture Provider thông qua abstraction.

```text
Capture Module
      |
      v
CaptureProvider
      |
      +-- ScreenCaptureProvider
      +-- WindowCaptureProvider
      +-- BrowserCaptureProvider
      +-- LocalInputProvider
```

Capture Module không phụ thuộc trực tiếp vào implementation cụ thể.

Thiết kế provider chi tiết nằm trong:

```text
doc/04-providers/
```

---

## Scheduling

Capture work phải được Scheduler quản lý khi hoạt động bất đồng bộ.

Capture Job có thể mang:

```text
JobId
SessionId
GenerationId
CaptureSourceId
Priority
Deadline
CancellationScope
ConfigSnapshotId
```

Capture Module không tự tạo một hệ thống worker riêng bên ngoài Runtime Scheduler.

---

## Backpressure

Capture không được tạo frame nhanh hơn khả năng xử lý của pipeline trong thời gian dài.

Khi pipeline bị chậm, hệ thống có thể:

- Bỏ qua capture tick mới.
- Thay thế frame cũ bằng frame mới nhất.
- Giảm capture frequency.
- Tạm dừng continuous capture.
- Ưu tiên user-triggered capture.

Đối với trải nghiệm đọc, frame mới nhất thường có giá trị cao hơn hàng loạt frame cũ chưa xử lý.

Vì vậy Capture Queue nên ưu tiên chiến lược:

```text
Latest relevant frame wins.
```

thay vì xử lý mọi frame theo FIFO tuyệt đối.

---

## Cancellation

Capture phải hỗ trợ cancellation trong các trường hợp:

- Reading Session dừng.
- Session Generation thay đổi.
- Source bị thay thế.
- Người dùng đổi vùng capture.
- Cửa sổ nguồn bị đóng.
- Runtime shutdown.
- Capture request vượt deadline.

Sau cancellation, kết quả đến muộn không được tiếp tục đi vào pipeline.

---

## Error Categories

Capture error có thể được phân loại thành:

```text
SourceUnavailable
PermissionDenied
SourceLost
InvalidRegion
CaptureTimeout
UnsupportedSource
ProviderUnavailable
ResourceLimitExceeded
Cancelled
InternalCaptureFailure
```

Capture Module phải chuyển lỗi provider-specific thành error model chung.

---

## Failure Recovery

Tùy loại lỗi, Capture Module có thể:

- Retry có giới hạn.
- Yêu cầu người dùng cấp lại quyền.
- Chờ source xuất hiện lại.
- Chuyển về suspended state.
- Yêu cầu chọn source mới.
- Dừng source hiện tại.
- Báo lỗi không thể phục hồi cho Session Manager.

Không tự động chuyển sang capture toàn màn hình để thay thế source bị lỗi.

---

## Performance Expectations

Capture phải ưu tiên:

- Độ trễ thấp.
- Không gây giật giao diện.
- Không chiếm dụng CPU/GPU quá mức.
- Không giữ nhiều raw frame trong bộ nhớ.
- Không capture khi không có nhu cầu.
- Không tạo backlog frame cũ.

Các giới hạn cụ thể được lấy từ Runtime Configuration và Performance Model.

---

## Configuration

Capture configuration có thể gồm:

```text
Default source type
Capture mode
Capture interval
Maximum frame size
Maximum concurrent captures
Preferred image format
Region policy
DPI handling
Permission policy
Raw frame retention
Capture timeout
Backpressure strategy
```

Capture Module chỉ sử dụng typed configuration view được cấp bởi Configuration Service.

Module không tự đọc file cấu hình.

---

## Observability

Capture Module có thể ghi nhận:

- Capture request count.
- Capture success count.
- Capture failure count.
- Capture duration.
- Frame dimensions.
- Frame memory size.
- Dropped frame count.
- Source reconnect count.
- Permission failure count.
- Active source count.

Không được ghi:

- Raw image.
- Nội dung màn hình.
- OCR text.
- DOM content nhạy cảm.
- Credential.

---

## MVP Scope

Capture Module trong MVP ưu tiên:

- Chọn vùng màn hình.
- Capture vùng màn hình.
- Theo dõi một source tại một thời điểm.
- On-demand capture.
- Continuous capture với tần suất giới hạn.
- Memory-only raw frame.
- Cancellation theo session và generation.
- Source availability detection.
- Basic permission handling.

---

## Deferred Scope

Các khả năng có thể để sau MVP:

- Browser extension integration.
- DOM snapshot capture.
- Multi-window capture.
- Multi-monitor coordinated capture.
- Automatic reader-area detection.
- GPU-native frame sharing.
- Video stream capture.
- Mobile device capture.
- Remote device capture.
- PDF renderer integration.
- Automatic source switching.

---

## Design Principles

Capture Module phải tuân thủ:

1. Capture only acquires data.
2. Capture does not interpret content.
3. Source access must be explicit.
4. Raw data is memory-only by default.
5. Every frame belongs to a session generation.
6. New content is usually more valuable than stale frames.
7. Provider-specific details must remain behind contracts.
8. Capture failure must not terminate the entire Runtime.
9. Cleanup must be deterministic.
10. Capture must not interrupt the reading experience.

---

## Related Documents

```text
doc/00-project/USER_JOURNEY.md

doc/01-architecture/core/CAPABILITY_MAP.md
doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/flows/SCREEN_COMIC_FLOW.md

doc/01-architecture/modules/MODULE_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/RUNTIME_COMPONENTS.md
doc/01-architecture/runtime/RUNTIME_CONTEXT.md
doc/01-architecture/runtime/RUNTIME_CONFIG.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/SCHEDULER.md
doc/01-architecture/runtime/WORK_QUEUE.md
```

---

## Module Documents

Capture Module sẽ được mô tả chi tiết qua:

```text
capture/
├── README.md
├── MODULE.md
├── API.md
├── EVENTS.md
└── STATES.md
```

### README.md

Tổng quan, phạm vi và vị trí của module trong hệ thống.

### MODULE.md

Thiết kế nội bộ, component, lifecycle và dependency chi tiết.

### API.md

Command, query và public contract của module.

### EVENTS.md

Event được publish và subscribe.

### STATES.md

State machine của capture source và capture operation.

---

## Completion Criteria

Capture Module được xem là thiết kế đầy đủ khi:

- Boundary với Recognition được xác định rõ.
- Các loại capture source được định nghĩa.
- CaptureFrame contract được xác định.
- Coordinate space được mô tả rõ.
- Privacy policy được áp dụng.
- Lifecycle của source được định nghĩa.
- Cancellation propagation được xác định.
- Backpressure strategy được xác định.
- Error và recovery behavior được xác định.
- Provider boundary được xác định.
- MVP scope được khóa.