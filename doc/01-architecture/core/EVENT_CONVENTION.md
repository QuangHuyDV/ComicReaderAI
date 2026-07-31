# Event Naming Convention

## Purpose

Tài liệu này định nghĩa quy ước đặt tên và thiết kế Domain Event trong toàn bộ CRAI.

Mọi module phải tuân theo tài liệu này khi:

- tạo event mới;
- publish event;
- subscribe event;
- version event.

Mục tiêu là đảm bảo:

- Event nhất quán.
- Dễ đọc.
- Dễ tìm kiếm.
- Không phụ thuộc implementation.
- Dễ mở rộng.

---

# What is a Domain Event?

Domain Event mô tả một sự kiện **đã xảy ra**.

Event không phải:

- command;
- request;
- function call;
- callback.

Event chỉ mô tả sự thật.

Ví dụ:

```text
CaptureStarted
```

nghĩa là:

> Capture đã bắt đầu.

không phải:

> Hãy bắt đầu Capture.

---

# Event Naming Pattern

Mọi Event phải theo mẫu:

```text
<Noun><PastTenseVerb>
```

Ví dụ:

```text
CaptureStarted

CaptureCancelled

CaptureTimedOut

CaptureFailed

SourceActivated

PermissionRevoked

TranslationCompleted

BubbleDetected

TextRecognized
```

---

# Never Use

Không dùng:

```text
DoCapture

CaptureNow

CaptureRequest

CaptureProcess

CaptureWorkerDone

CaptureExecute

CaptureAction
```

vì đây không phải event.

---

# Event Tense

Luôn dùng:

```text
Past Tense
```

Ví dụ:

✔

```text
CaptureStarted
```

✘

```text
CaptureStart
```

✔

```text
TranslationCompleted
```

✘

```text
TranslationComplete
```

✔

```text
PermissionRevoked
```

✘

```text
PermissionRevoke
```

---

# Event Subject

Subject phải là Domain Object.

Ví dụ:

```text
Capture

Recognition

Translation

Bubble

Frame

Session

Source

Permission

Reading

Chapter

Page
```

Không dùng:

```text
Manager

Controller

Worker

Thread

Scheduler

Pipeline

Runtime
```

Implementation không được xuất hiện trong Event Name.

---

# Event Verb

Verb mô tả điều đã xảy ra.

Danh sách verb chuẩn:

```text
Started

Completed

Finished

Cancelled

Failed

TimedOut

Detected

Recognized

Created

Activated

Suspended

Resumed

Stopped

Removed

Revoked

Granted

Changed

Updated

Expired

Produced

Published

Merged

Validated

Loaded

Saved
```

Nếu có thể dùng verb đã tồn tại thì không tạo verb mới.

---

# Finished vs Completed

Quy ước:

Completed

=

Business hoàn thành.

Finished

=

Implementation kết thúc.

CRAI ưu tiên:

```text
Completed
```

Ví dụ:

```text
TranslationCompleted
```

không dùng

```text
TranslationFinished
```

---

# Ready vs Produced

Ví dụ:

```text
FrameReady
```

nói lên trạng thái.

Trong khi:

```text
FrameProduced
```

nói lên sự kiện.

Domain Event nên dùng:

```text
FrameProduced
```

---

# Changed

Chỉ dùng:

```text
ConfigurationChanged

LanguageChanged
```

Không dùng Changed nếu có verb cụ thể hơn.

Ví dụ:

```text
PermissionRevoked
```

tốt hơn

```text
PermissionChanged
```

---

# Event Payload

Event chỉ nên chứa:

- Identity.
- Metadata.
- Reference.

Không chứa:

- Raw image.
- Pixel buffer.
- Mutable object.
- Native pointer.
- Large binary.

---

# Event Version

Mỗi Event phải có:

```text
Version
```

Ví dụ:

```text
CaptureFrameProduced v1
```

Nếu payload thay đổi:

```text
CaptureFrameProduced v2
```

Không sửa payload của version cũ.

---

# Event Identity

Mỗi Event có:

```text
EventId

EventType

OccurredAt

CorrelationId

CausationId

TraceId
```

---

# Event Ordering

Ordering chỉ đảm bảo:

```text
Per Aggregate
```

Không đảm bảo Global Ordering.

---

# Event Ownership

Mỗi Event có đúng một Publisher.

Ví dụ:

```text
CaptureFrameProduced

↓

Capture Module
```

Recognition Module không được publish event này.

---

# Event Examples

Capture:

```text
CaptureStarted

CaptureFrameProduced

CaptureCompleted

CaptureCancelled

CaptureFailed
```

Recognition:

```text
RecognitionStarted

BubbleDetected

TextRecognized

RecognitionCompleted
```

Translation:

```text
TranslationStarted

TranslationCompleted

TranslationFailed
```

Presentation:

```text
OverlayRendered

TranslationDisplayed
```

Reading:

```text
ReadingSessionStarted

ReadingPaused

ReadingResumed

ReadingStopped

PageChanged

ChapterChanged
```

---

# Checklist

Một Event mới phải trả lời được:

- Đây có phải sự kiện đã xảy ra không?
- Có dùng Past Tense không?
- Subject có phải Domain Object không?
- Có chứa implementation không?
- Có trùng nghĩa với Event khác không?
- Payload có nhỏ không?
- Có owner duy nhất không?
- Có cần version không?