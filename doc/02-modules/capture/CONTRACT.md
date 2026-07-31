# Capture Module API

## Purpose

Tài liệu này định nghĩa **Public API** của Capture Module.

Public API là tập hợp các contract mà các module khác được phép sử dụng để tương tác với Capture Module.

Mục tiêu của tài liệu:

- Định nghĩa capability của Capture Module.
- Định nghĩa command và query.
- Định nghĩa request/response contract.
- Định nghĩa quy tắc bất đồng bộ.
- Không mô tả implementation.

---

# API Principles

Capture Module chỉ expose những API cần thiết.

Module khác **không được** truy cập:

- Source Manager
- Frame Acquirer
- Frame Normalizer
- Permission Controller
- Frame Lifecycle Manager

Tất cả đều phải đi qua Public API.

---

# Public Services

Capture Module cung cấp các service sau:

```text
CaptureService

CaptureSourceService

CaptureCapabilityService

CaptureHealthService
```

---

# CaptureService

## Purpose

Service chính để thực hiện Capture.

---

## Commands

### Capture

```text
Capture(
    CaptureRequest
)

→ CaptureResult
```

Thực hiện một lần capture.

Có thể là:

- synchronous abstraction
- asynchronous implementation

Runtime quyết định execution model.

---

### StartContinuousCapture

```text
StartContinuousCapture(
    ContinuousCaptureRequest
)

→ CaptureSessionHandle
```

Bắt đầu continuous capture.

Không thực hiện OCR.

Không thực hiện Translation.

---

### StopContinuousCapture

```text
StopContinuousCapture(
    CaptureSessionHandle
)
```

Dừng continuous capture.

---

### SuspendCapture

```text
SuspendCapture(
    CaptureSessionHandle
)
```

Tạm dừng.

---

### ResumeCapture

```text
ResumeCapture(
    CaptureSessionHandle
)
```

Tiếp tục.

---

### CancelCapture

```text
CancelCapture(
    CaptureOperationId
)
```

Hủy một capture operation.

Không hủy Reading Session.

---

# CaptureSourceService

## Purpose

Quản lý Capture Source.

---

## Commands

### CreateSource

```text
CreateSource(
    CaptureSourceDescriptor
)

→ CaptureSourceId
```

---

### ReplaceSource

```text
ReplaceSource(
    CaptureSourceId
)
```

---

### RemoveSource

```text
RemoveSource(
    CaptureSourceId
)
```

---

### ActivateSource

```text
ActivateSource(
    CaptureSourceId
)
```

---

### DeactivateSource

```text
DeactivateSource(
    CaptureSourceId
)
```

---

# CaptureCapabilityService

## Purpose

Tra cứu capability của source.

---

### GetCapabilities

```text
GetCapabilities(
    CaptureSourceId
)

→ CaptureCapability
```

Ví dụ:

```text
SupportsRegionCapture

SupportsContinuousCapture

SupportsDomSnapshot

SupportsWindowCapture
```

---

# CaptureHealthService

## Purpose

Theo dõi tình trạng Capture Module.

---

### GetHealth

```text
GetHealth()

→ CaptureHealth
```

---

### GetStatistics

```text
GetStatistics()

→ CaptureStatistics
```

---

# Request Models

## CaptureRequest

```text
CaptureRequest

SessionId

GenerationId

CaptureSourceId

CaptureMode

CaptureRegion

Priority

Deadline

CancellationScope

ConfigSnapshotId
```

---

## ContinuousCaptureRequest

```text
ContinuousCaptureRequest

SessionId

CaptureSourceId

Interval

Priority

CapturePolicy

CancellationScope

ConfigSnapshotId
```

---

## CaptureSourceDescriptor

Ví dụ:

```text
ScreenRegion

ApplicationWindow

Display

BrowserConnector

LocalInput
```

Có thể chứa:

```text
SourceType

Region

WindowHandle

DisplayId

ConnectorId

Metadata
```

---

# Response Models

## CaptureResult

```text
CaptureResult

OperationId

CaptureFrame

Duration

Status

Warnings
```

---

## CaptureSessionHandle

```text
CaptureSessionHandle

HandleId

SessionId

SourceId

State
```

---

## CaptureCapability

```text
SupportsContinuousCapture

SupportsRegionCapture

SupportsWindowCapture

SupportsDisplayCapture

SupportsDomSnapshot

SupportsCursorExclusion

SupportsMultipleDisplays

SupportsOccludedWindow
```

---

## CaptureHealth

```text
Healthy

Degraded

Recovering

Unavailable

Stopped
```

---

## CaptureStatistics

Ví dụ:

```text
TotalCaptures

DroppedFrames

CaptureLatency

FailureRate

RetryCount

ReconnectCount
```

---

# Execution Model

Capture API không đảm bảo synchronous.

Implementation có thể:

```text
Sync

Async

Coroutine

Future

Promise

Task

Job
```

Public contract phải giữ nguyên.

---

# Thread Safety

Capture API phải thread-safe.

Các command có thể được gọi đồng thời từ nhiều thread.

Capture Module chịu trách nhiệm:

- synchronization
- locking
- state consistency

Caller không cần lock.

---

# Idempotency

Các command sau nên idempotent.

```text
StopContinuousCapture

DeactivateSource

RemoveSource

SuspendCapture
```

Nếu gọi nhiều lần phải cho kết quả giống nhau.

---

# Non-Idempotent Commands

```text
Capture

CreateSource

StartContinuousCapture
```

Mỗi lần gọi tạo operation mới.

---

# Cancellation

Mọi command có thể chạy lâu đều phải hỗ trợ:

```text
CancellationScope

Deadline

Timeout
```

Late result phải bị bỏ.

---

# Error Model

Capture API không trả native error.

Chỉ trả error chuẩn.

Ví dụ:

```text
PermissionDenied

SourceLost

CaptureTimeout

InvalidRegion

UnsupportedSource

Cancelled

ProviderUnavailable

InternalError
```

---

# Versioning

Public API phải compatible.

Không thay đổi:

- command name
- semantic
- ownership

Nếu cần thay đổi lớn:

```text
CaptureServiceV2
```

hoặc versioned contract.

---

# Security

API không trả:

- API Key
- Native Handle
- Provider Secret
- OS-specific object

---

# Performance Expectations

Capture command nên:

- latency thấp
- không block UI thread
- không giữ frame lâu
- không tạo backlog

---

# Usage Examples

## One-shot Capture

```text
Reading Module

↓

CaptureService.Capture()

↓

CaptureResult

↓

Recognition Module
```

---

## Continuous Capture

```text
Reading Module

↓

StartContinuousCapture()

↓

Capture Session

↓

CaptureFrame

↓

Recognition Module
```

---

## Source Replacement

```text
DeactivateSource()

↓

RemoveSource()

↓

CreateSource()

↓

ActivateSource()

↓

ResumeCapture()
```

---

# API Constraints

Capture API không được:

- Thực hiện OCR.
- Trả TranslationResult.
- Trả TextRegion.
- Trả Bubble.
- Trả DOM đã phân tích.
- Truy cập trực tiếp Runtime internals.
- Phụ thuộc provider implementation.

---

# Extension Points

Có thể mở rộng sau này:

```text
Multi-source capture

Video capture

GPU frame

Remote capture

PDF renderer

Browser live DOM

Clipboard watcher
```

Các extension phải giữ tương thích với Public API hiện có.

---

# Related Documents

```text
capture/README.md
capture/MODULE.md
capture/EVENTS.md
capture/STATES.md

doc/03-contracts/
doc/04-providers/

doc/01-architecture/runtime/RUNTIME_CONTEXT.md
doc/01-architecture/runtime/SCHEDULER.md
doc/01-architecture/runtime/CANCELLATION.md
```

---

# Completion Criteria

Capture API được xem là hoàn chỉnh khi:

- Public surface nhỏ và ổn định.
- Không rò rỉ implementation.
- Hỗ trợ cancellation.
- Hỗ trợ thread-safe.
- Có command và query rõ ràng.
- Không phụ thuộc provider cụ thể.
- Có thể mock để kiểm thử.
- Phù hợp với Runtime Architecture.