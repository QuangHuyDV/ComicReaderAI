# Capture Module Events

## Purpose

Tài liệu này định nghĩa toàn bộ Event Contract của Capture Module.

Capture Module hoạt động trong kiến trúc Event-Driven, vì vậy các module khác không nên phụ thuộc trực tiếp vào implementation của Capture.

Mọi thay đổi trạng thái hoặc kết quả quan trọng đều được phát ra dưới dạng Event.

Tài liệu này định nghĩa:

- Event được publish.
- Event được subscribe.
- Payload của từng event.
- Thứ tự phát event.
- Delivery guarantee.
- Event ordering.
- Event ownership.

---

# Event Principles

Capture Module sử dụng event để:

- Thông báo trạng thái.
- Đồng bộ Runtime.
- Khởi động pipeline tiếp theo.
- Quan sát hệ thống.
- Thực hiện tracing.

Event không được sử dụng để:

- Trả dữ liệu lớn.
- Truyền raw image.
- Truyền mutable object.
- Điều khiển trực tiếp module khác.

---

# Event Categories

Capture Module có các nhóm event sau:

```text
Lifecycle Events

Capture Events

Source Events

Health Events

Failure Events
```

---

# Published Events

Capture Module publish các event sau.

```text
CaptureStarted

CaptureCompleted

CaptureCancelled

CaptureTimeout

CaptureFailed

CaptureFrameReady

CaptureSourceCreated

CaptureSourceActivated

CaptureSourceSuspended

CaptureSourceResumed

CaptureSourceRemoved

CaptureSourceLost

CapturePermissionGranted

CapturePermissionRevoked

CaptureHealthChanged
```

---

# Subscribed Events

Capture Module subscribe các event sau.

```text
ReadingSessionStarted

ReadingSessionPaused

ReadingSessionResumed

ReadingSessionStopped

ReadingGenerationChanged

RuntimeShutdown

ConfigurationChanged
```

Capture Module không nên subscribe trực tiếp event từ Recognition hoặc Translation.

---

# Lifecycle Events

## CaptureStarted

### Purpose

Thông báo một Capture Operation đã bắt đầu.

### Payload

```text
OperationId

SessionId

GenerationId

CaptureSourceId

CaptureMode

Timestamp
```

### Published When

Sau khi Capture Coordinator tạo operation thành công.

---

## CaptureCompleted

### Purpose

Thông báo operation hoàn thành.

### Payload

```text
OperationId

SessionId

GenerationId

CaptureSourceId

Duration

Timestamp
```

CaptureCompleted không mang CaptureFrame.

Frame sẽ được publish bằng event riêng.

---

## CaptureCancelled

### Payload

```text
OperationId

Reason

Timestamp
```

Reason có thể là:

```text
UserCancelled

GenerationChanged

SessionStopped

DeadlineExceeded

Shutdown

SourceReplaced
```

---

## CaptureTimeout

### Payload

```text
OperationId

Timeout

Timestamp
```

---

# Capture Events

## CaptureFrameReady

Đây là event quan trọng nhất của Capture Module.

### Purpose

Thông báo một CaptureFrame đã sẵn sàng.

Recognition Module sẽ subscribe event này.

### Payload

```text
FrameId

OperationId

SessionId

GenerationId

CaptureSourceId

CaptureFrameReference

Timestamp
```

Không truyền raw pixel.

Chỉ truyền reference.

---

## CaptureFailed

### Payload

```text
OperationId

ErrorCode

Recoverable

Timestamp
```

Recoverable giúp Runtime quyết định retry.

---

# Source Events

## CaptureSourceCreated

```text
CaptureSourceId

SourceType

Timestamp
```

---

## CaptureSourceActivated

```text
CaptureSourceId

Timestamp
```

---

## CaptureSourceSuspended

```text
CaptureSourceId

Reason

Timestamp
```

---

## CaptureSourceResumed

```text
CaptureSourceId

Timestamp
```

---

## CaptureSourceRemoved

```text
CaptureSourceId

Timestamp
```

---

## CaptureSourceLost

### Purpose

Thông báo source không còn khả dụng.

Ví dụ:

- Window đóng.
- Browser disconnect.
- Display bị ngắt.
- Region không hợp lệ.

Payload

```text
CaptureSourceId

Reason

Recoverable

Timestamp
```

---

# Permission Events

## CapturePermissionGranted

```text
PermissionType

Timestamp
```

---

## CapturePermissionRevoked

```text
PermissionType

Reason

Timestamp
```

Sau event này Capture Module phải dừng operation mới.

---

# Health Events

## CaptureHealthChanged

### Payload

```text
OldState

NewState

Timestamp
```

Health State:

```text
Healthy

Degraded

Recovering

Unavailable

Stopped
```

---

# Event Ordering

Một Capture Operation bình thường có thứ tự:

```text
CaptureStarted

↓

CaptureFrameReady

↓

CaptureCompleted
```

Nếu lỗi:

```text
CaptureStarted

↓

CaptureFailed
```

Nếu timeout:

```text
CaptureStarted

↓

CaptureTimeout

↓

CaptureFailed
```

Nếu bị cancel:

```text
CaptureStarted

↓

CaptureCancelled
```

---

# Continuous Capture Ordering

```text
CaptureStarted

↓

CaptureFrameReady

↓

CaptureCompleted

↓

CaptureStarted

↓

CaptureFrameReady

↓

CaptureCompleted

...
```

Mỗi operation độc lập.

---

# Source Replacement Ordering

```text
CaptureSourceSuspended

↓

CaptureCancelled

↓

CaptureSourceRemoved

↓

CaptureSourceCreated

↓

CaptureSourceActivated
```

Generation mới sẽ bắt đầu sau chuỗi event này.

---

# Delivery Guarantee

Mặc định:

```text
At Least Once
```

Do đó consumer phải xử lý duplicate event.

Không được giả định event chỉ xuất hiện một lần.

---

# Ordering Guarantee

Capture Module chỉ đảm bảo:

```text
Per Operation Ordering
```

Không đảm bảo:

```text
Global Ordering
```

Hai operation khác nhau có thể hoàn thành theo thứ tự bất kỳ.

---

# Event Identity

Mỗi event phải có:

```text
EventId

EventType

Timestamp

CorrelationId

CausationId

TraceId
```

Nếu liên quan tới operation:

```text
OperationId
```

Nếu liên quan tới frame:

```text
FrameId
```

---

# Event Ownership

Capture Module là owner duy nhất của:

```text
CaptureStarted

CaptureCompleted

CaptureFrameReady

CaptureCancelled

CaptureTimeout

CaptureFailed

CaptureSource*

CapturePermission*

CaptureHealthChanged
```

Không module nào khác được publish các event này.

---

# Event Versioning

Mỗi event cần version.

Ví dụ:

```text
CaptureFrameReady v1
```

Nếu payload thay đổi:

```text
CaptureFrameReady v2
```

Không thay đổi payload của version cũ.

---

# Event Size

Event phải nhỏ.

Không chứa:

- Raw image
- Screenshot
- Pixel buffer
- OCR result
- Translation result

Chỉ chứa:

- Identifier
- Metadata
- Reference

---

# Event Lifetime

Capture event có thời gian sống ngắn.

Ví dụ:

```text
CaptureStarted

↓

CaptureCompleted

↓

Discard
```

Frame Reference chỉ hợp lệ trong retention window.

Consumer phải lấy frame trước khi hết hạn.

---

# Retry Events

Capture Module không publish event:

```text
CaptureRetrying
```

Retry là implementation detail.

Consumer chỉ quan tâm:

```text
CaptureStarted

CaptureCompleted

CaptureFailed
```

---

# Event Security

Event không được chứa:

- Raw image.
- Secret.
- Token.
- Native handle.
- Memory address.
- Provider object.

---

# Event Flow

```text
Reading Module

↓

Capture Request

↓

Capture Module

↓

CaptureStarted

↓

CaptureFrameReady

↓

Recognition Module

↓

CaptureCompleted
```

---

# Integration

Recognition Module subscribe:

```text
CaptureFrameReady
```

Diagnostics subscribe:

```text
CaptureStarted

CaptureCompleted

CaptureFailed

CaptureTimeout

CaptureHealthChanged
```

Reading Module subscribe:

```text
CaptureSourceLost

CapturePermissionRevoked
```

Runtime Coordinator subscribe:

```text
CaptureHealthChanged

CaptureFailed
```

---

# Testing

Cần kiểm thử:

- Event đúng thứ tự.
- Duplicate event.
- Late event.
- Cancel event.
- Timeout event.
- Source replacement.
- Generation change.
- Retry không lộ ra ngoài.
- FrameReady luôn trước Completed.

---

# Completion Criteria

Capture Event Contract được xem là hoàn chỉnh khi:

- Publish event xác định rõ.
- Subscribe event xác định rõ.
- Payload ổn định.
- Ordering được mô tả.
- Delivery guarantee được xác định.
- Event ownership rõ ràng.
- Không rò rỉ implementation.
- Chỉ truyền metadata và reference.
- Phù hợp với Event Bus Architecture.

---

# Related Documents

```text
capture/README.md
capture/MODULE.md
capture/CONTRACT.md
capture/STATES.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/runtime/RUNTIME_CONTEXT.md
doc/01-architecture/runtime/CANCELLATION.md
```