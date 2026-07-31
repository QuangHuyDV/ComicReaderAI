# Capture Module States

## Purpose

Tài liệu này định nghĩa toàn bộ State Machine của Capture Module.

Capture Module không chỉ quản lý việc thu nhận dữ liệu mà còn quản lý nhiều thực thể (entity) với vòng đời độc lập.

Mỗi entity có một State Machine riêng.

Tài liệu này định nghĩa:

- State.
- Transition.
- Trigger.
- Guard condition.
- Terminal state.
- Invalid transition.
- Recovery behavior.

Không mô tả implementation.

---

# State Machines

Capture Module bao gồm bốn State Machine chính:

```text
Capture Source State Machine

Capture Operation State Machine

Continuous Capture Session State Machine

Capture Module Health State Machine
```

Mỗi State Machine hoạt động độc lập nhưng có thể ảnh hưởng lẫn nhau thông qua Event.

---

# 1. Capture Source State Machine

## Purpose

Quản lý lifecycle của một Capture Source.

Ví dụ:

- Screen Region
- Application Window
- Browser Connector
- Display
- Local Input

---

## States

```text
Uninitialized

↓

Initializing

↓

Ready

↓

Active

↓

Suspended

↓

Stopping

↓

Stopped
```

---

## State Descriptions

### Uninitialized

Source chưa được tạo.

Không có native resource.

---

### Initializing

Đang:

- tạo source
- kiểm tra permission
- kiểm tra capability
- mở native handle

---

### Ready

Source hợp lệ.

Chưa có Capture Operation.

---

### Active

Source đang phục vụ capture.

Có thể có nhiều operation theo thời gian nhưng chỉ một operation active tại một thời điểm (MVP).

---

### Suspended

Source vẫn tồn tại.

Không thực hiện capture mới.

Native handle vẫn được giữ nếu platform cho phép.

---

### Stopping

Đang:

- cancel operation
- release resource
- cleanup

---

### Stopped

Source không còn tồn tại.

Không thể reuse.

---

## Transitions

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
Active
      /   \
     /     \
Suspended  Stopping
     |        |
     +--------+
        |
        v
      Stopped
```

---

## Valid Transitions

| From | To |
|-------|----|
| Uninitialized | Initializing |
| Initializing | Ready |
| Ready | Active |
| Active | Suspended |
| Suspended | Active |
| Active | Stopping |
| Ready | Stopping |
| Suspended | Stopping |
| Stopping | Stopped |

---

## Invalid

Ví dụ:

```text
Stopped

↓

Ready
```

Không hợp lệ.

Muốn dùng lại phải tạo source mới.

---

# 2. Capture Operation State Machine

## Purpose

Mỗi lần Capture() tạo một Capture Operation.

Operation có lifecycle riêng.

---

## States

```text
Created

↓

Queued

↓

Running

↓

Normalizing

↓

Completed
```

Có thể rẽ sang:

```text
Cancelled

Failed

TimedOut
```

---

## Diagram

```text
Created
    |
    v
Queued
    |
    v
Running
    |
    v
Normalizing
    |
    v
Completed

Running
   |
   +------> Failed
   |
   +------> TimedOut
   |
   +------> Cancelled
```

---

## State Descriptions

### Created

Operation vừa được tạo.

Chưa vào Scheduler.

---

### Queued

Đã vào Work Queue.

Chờ Scheduler.

---

### Running

Provider đang capture.

---

### Normalizing

Đã có RawCaptureResult.

Đang tạo CaptureFrame.

---

### Completed

CaptureFrame hợp lệ.

FrameReady event đã publish.

---

### Failed

Không thể tạo CaptureFrame.

---

### TimedOut

Deadline hết hạn.

---

### Cancelled

Operation bị huỷ.

Late result phải bị loại bỏ.

---

## Terminal States

```text
Completed

Cancelled

Failed

TimedOut
```

Sau khi vào Terminal State không được chuyển tiếp.

---

# 3. Continuous Capture Session State Machine

## Purpose

Quản lý vòng đời của continuous capture.

---

## States

```text
Created

↓

Starting

↓

Running

↓

Paused

↓

Running

↓

Stopping

↓

Stopped
```

---

## State Descriptions

### Created

Session chưa bắt đầu.

---

### Starting

Đang:

- validate source
- validate permission
- tạo scheduler task

---

### Running

Capture theo interval.

---

### Paused

Không tạo operation mới.

Operation đang chạy được phép hoàn thành.

---

### Stopping

Đang:

- cancel scheduler
- stop capture tick
- cleanup

---

### Stopped

Continuous Capture kết thúc.

---

# 4. Capture Health State Machine

## Purpose

Theo dõi tình trạng Capture Module.

---

## States

```text
Healthy

↓

Degraded

↓

Recovering

↓

Healthy
```

hoặc

```text
Healthy

↓

Unavailable

↓

Recovering
```

---

## State Descriptions

### Healthy

Hoạt động bình thường.

---

### Degraded

Ví dụ:

- capture chậm
- nhiều dropped frame
- retry tăng

---

### Recovering

Đang:

- reconnect
- recreate source
- chờ permission

---

### Unavailable

Không thể capture.

Ví dụ:

- mất permission
- provider crash

---

### Stopped

Module shutdown.

---

# Cross State Rules

Một Capture Operation chỉ được Running khi:

```text
Capture Source == Active
```

Nếu Source chuyển sang:

```text
Stopping
```

mọi Operation phải:

```text
Cancelled
```

---

Continuous Capture chỉ được Running khi:

```text
Capture Source == Active
```

---

Nếu Module Health:

```text
Unavailable
```

Continuous Capture phải dừng tạo Operation mới.

---

# State Invariants

## Source

Một Source chỉ có một State tại một thời điểm.

---

## Operation

Một Operation chỉ có một Terminal State.

---

## Session

Một Reading Session chỉ có một Active Source (MVP).

---

## Continuous Capture

Một Session chỉ có một Continuous Capture Session.

---

# Recovery

## Permission Revoked

```text
Active

↓

Suspended
```

Sau khi permission được cấp lại:

```text
Suspended

↓

Active
```

---

## Source Lost

```text
Active

↓

Stopping

↓

Stopped
```

Reading Module quyết định tạo Source mới.

---

## Runtime Shutdown

Tất cả State Machine phải tiến về:

```text
Stopped
```

---

# Generation Change

Khi Reading Generation thay đổi:

```text
Running Operation

↓

Cancelled
```

Sau đó:

```text
Source

↓

Ready

↓

Active
```

Operation mới sẽ được tạo theo Generation mới.

---

# Timeout Behaviour

Nếu Operation timeout:

```text
Running

↓

TimedOut
```

Late Result:

```text
Discard
```

không được:

```text
TimedOut

↓

Completed
```

---

# Illegal States

Các trạng thái sau không được phép tồn tại:

```text
Source = Active

Operation = Running

Permission = Revoked
```

---

```text
Operation = Completed

Frame = Not Produced
```

---

```text
Source = Stopped

Operation = Running
```

---

# State Ownership

| State Machine | Owner |
|--------------|-------|
| Capture Source | Source Manager |
| Capture Operation | Capture Coordinator |
| Continuous Capture | Capture Coordinator |
| Health | Capture Health Monitor |

Không component nào khác được thay đổi state trực tiếp.

---

# Event Relationship

| Transition | Event |
|------------|-------|
| Ready → Active | CaptureSourceActivated |
| Active → Suspended | CaptureSourceSuspended |
| Running → Completed | CaptureCompleted |
| Running → Cancelled | CaptureCancelled |
| Running → Failed | CaptureFailed |
| Running → TimedOut | CaptureTimeout |
| Normalizing → Completed | CaptureFrameReady + CaptureCompleted |

State transition phải xảy ra trước khi event tương ứng được publish.

---

# Testing

Các trường hợp cần kiểm thử:

- Source lifecycle đầy đủ.
- Operation lifecycle đầy đủ.
- Continuous Capture pause/resume.
- Permission revoke.
- Generation change.
- Runtime shutdown.
- Timeout.
- Source replacement.
- Double cancel.
- Double stop.
- Late result.
- Invalid transition.

---

# Completion Criteria

State Model được xem là hoàn chỉnh khi:

- Mỗi entity có State Machine riêng.
- Transition được định nghĩa rõ.
- Terminal state được xác định.
- Invalid transition được mô tả.
- Recovery được mô tả.
- Quan hệ giữa các State Machine rõ ràng.
- Event và State nhất quán.
- Có thể kiểm thử độc lập từng State Machine.

---

# Related Documents

```text
capture/README.md
capture/MODULE.md
capture/CONTRACT.md
capture/EVENTS.md

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/runtime/CANCELLATION.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/RUNTIME_CONTEXT.md
```