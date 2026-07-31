# runtime/RUNTIME_COMPONENTS.md

# Runtime Components

## Purpose

Tài liệu này mô tả toàn bộ các Runtime Component của CRAI.

Mục tiêu của tài liệu là xác định:

- Những thành phần tồn tại khi Runtime đang hoạt động.
- Trách nhiệm của từng thành phần.
- Ranh giới giữa các component.
- Quan hệ phụ thuộc giữa các component.
- Vòng đời (lifecycle) của từng component.

Tài liệu này **không mô tả implementation**, **không mô tả class**, và **không mô tả package**.

---

# Runtime Overview

```
                    CRAI Runtime

                 +------------------+
                 |    Bootstrap     |
                 +--------+---------+
                          |
                          v
              +-----------+------------+
              | Runtime Coordinator    |
              +-----------+------------+
                          |
     --------------------------------------------------------
     |          |           |           |                  |
     v          v           v           v                  v

 Configuration Event Bus  Scheduler  Session Manager  Diagnostics

                          |
                          v

                 Processing Pipeline

 Capture
    ↓
 Observation
    ↓
 Classification
    ↓
 Extraction
    ↓
 Understanding
    ↓
 Translation
    ↓
 Presentation
```

---

# Bootstrap

## Purpose

Khởi tạo toàn bộ Runtime.

## Responsibilities

- Load configuration.
- Validate configuration.
- Khởi tạo Runtime Coordinator.
- Tạo các singleton component.
- Chuẩn bị môi trường trước khi Runtime hoạt động.

## Lifetime

- Chỉ tồn tại trong giai đoạn startup.
- Sau khi Runtime khởi động thành công thì kết thúc.

---

# Runtime Coordinator

## Purpose

Là thành phần điều phối trung tâm của Runtime.

## Responsibilities

- Khởi động component.
- Dừng component.
- Restart component.
- Theo dõi health.
- Điều phối lifecycle.
- Quản lý shutdown.

## Notes

Không chứa business logic.

---

# Configuration Service

## Purpose

Quản lý toàn bộ Runtime Configuration.

## Responsibilities

- Load configuration.
- Validate configuration.
- Snapshot configuration.
- Runtime activation.
- Runtime update.

## Provides

- Read-only configuration view.
- Typed configuration.

---

# Event Bus

## Purpose

Trao đổi sự kiện giữa các component.

## Responsibilities

- Publish event.
- Subscribe event.
- Dispatch event.
- Event tracing.
- Event prioritization.

## Notes

Không xử lý business logic.

---

# Scheduler

## Purpose

Điều phối toàn bộ công việc bất đồng bộ.

## Responsibilities

- Queue management.
- Priority scheduling.
- Retry.
- Timeout.
- Cancellation.
- Concurrency control.

## Notes

Scheduler chỉ quản lý Job.

Scheduler không xử lý OCR hoặc Translation.

---

# Session Manager

## Purpose

Quản lý Reading Session.

## Responsibilities

- Create session.
- Destroy session.
- Pause.
- Resume.
- Stop.
- Session lifecycle.
- Generation tracking.

---

# Capture Runtime

## Purpose

Thu nhận dữ liệu đầu vào.

## Responsibilities

- Screen capture.
- Window capture.
- Browser capture.
- Region capture.

## Output

Frame

---

# Observation Runtime

## Purpose

Phân tích trạng thái của dữ liệu đầu vào.

## Responsibilities

- Detect stable frame.
- Detect scrolling.
- Detect page change.
- Duplicate detection.

## Output

ObservationResult

---

# Classification Runtime

## Purpose

Nhận diện loại nội dung.

## Responsibilities

- Detect content type.
- Detect language.
- Detect reading direction.

## Output

ContentType

---

# Extraction Runtime

## Purpose

Trích xuất nội dung.

## Responsibilities

- OCR.
- DOM extraction.
- Region detection.
- Bubble detection.

## Output

TextRegion

---

# Understanding Runtime

## Purpose

Chuẩn hóa dữ liệu văn bản.

## Responsibilities

- Normalize text.
- Merge sentence.
- Rebuild paragraph.
- Context construction.

## Output

TranslationUnit

---

# Translation Runtime

## Purpose

Dịch nội dung.

## Responsibilities

- Provider selection.
- Batch translation.
- Retry.
- Timeout.
- Translation execution.

## Output

TranslationResult

---

# Presentation Runtime

## Purpose

Hiển thị kết quả.

## Responsibilities

- Side Panel.
- Overlay.
- Typography.
- Synchronization.

## Output

Visible UI

---

# Storage Runtime

## Purpose

Quản lý dữ liệu lâu dài.

## Responsibilities

- Cache.
- Translation Memory.
- Glossary.
- Settings.
- Session persistence.

---

# Provider Registry

## Purpose

Quản lý Provider.

## Responsibilities

- Provider discovery.
- Capability lookup.
- Provider lifecycle.
- Fallback.

## Notes

Không trực tiếp thực hiện OCR hoặc Translation.

---

# Secret Manager

## Purpose

Quản lý thông tin nhạy cảm.

## Responsibilities

- API Key.
- OAuth Token.
- Secret lookup.
- Secure storage.

## Notes

Không trả về secret dạng plain text cho các component khác.

---

# Diagnostics Runtime

## Purpose

Thu thập thông tin vận hành.

## Responsibilities

- Logging.
- Metrics.
- Tracing.
- Timing.
- Health monitoring.

## Notes

Mặc định không ghi nội dung truyện hoặc dữ liệu OCR.

---

# Runtime Interaction

```
Capture
    ↓
Observation
    ↓
Scheduler
    ↓
Extraction
    ↓
Understanding
    ↓
Translation
    ↓
Presentation

          ↑

      Event Bus
```

---

# Runtime Dependency Rules

Mỗi Runtime Component chỉ phụ thuộc vào Contract.

Không phụ thuộc trực tiếp vào Implementation.

Ví dụ

```
Translation Runtime
        │
        ▼
TranslationProvider
```

Không phụ thuộc trực tiếp vào

```
OpenAIProvider

GeminiProvider

DeepLProvider
```

---

# Lifetime

## Long-lived Components

- Runtime Coordinator
- Configuration Service
- Event Bus
- Scheduler
- Diagnostics

---

## Session Components

Được tạo theo từng Reading Session.

- Capture Runtime
- Presentation Runtime
- Session Context

---

## Request Components

Được tạo theo từng Job.

- OCR Job
- Translation Job
- Observation Job

---

# Thread Ownership

Mỗi component tự quản lý thread và queue của mình.

Không component nào được phép thao tác trực tiếp queue nội bộ của component khác.

Trao đổi chỉ thông qua:

- Contract
- Event
- Scheduler

---

# Failure Isolation

Một component gặp lỗi không được làm Runtime dừng toàn bộ.

Ví dụ

```
OCR Runtime
      │
      ▼
Restart OCR

Translation vẫn hoạt động

Presentation vẫn hoạt động

Reading Session vẫn tiếp tục
```

---

# Component Summary

| Component | Responsibility |
|------------|----------------|
| Bootstrap | Startup |
| Runtime Coordinator | Lifecycle |
| Configuration Service | Runtime Configuration |
| Event Bus | Event Distribution |
| Scheduler | Job Scheduling |
| Session Manager | Reading Session |
| Capture Runtime | Capture Input |
| Observation Runtime | Detect Changes |
| Classification Runtime | Detect Content |
| Extraction Runtime | OCR / DOM |
| Understanding Runtime | Text Processing |
| Translation Runtime | Translation |
| Presentation Runtime | UI Rendering |
| Storage Runtime | Persistent Storage |
| Provider Registry | Provider Management |
| Secret Manager | Secret Management |
| Diagnostics Runtime | Logging & Metrics |

---

# Runtime Layer

```
+--------------------------------------+
|                 UI                   |
+--------------------------------------+

+--------------------------------------+
|        Runtime Coordinator           |
+--------------------------------------+

+--------------------------------------+
| Session | Scheduler | Event Bus      |
+--------------------------------------+

+--------------------------------------+
| Capture → Translation Pipeline       |
+--------------------------------------+

+--------------------------------------+
| Providers                            |
+--------------------------------------+

+--------------------------------------+
| Storage | Secrets | Diagnostics      |
+--------------------------------------+
```

---

# Completion Criteria

Runtime Architecture được xem là hoàn chỉnh khi:

- Mỗi component có đúng một trách nhiệm chính.
- Dependency không tạo vòng lặp.
- Lifecycle được định nghĩa đầy đủ.
- Runtime Configuration được quản lý tập trung.
- Event Flow được chuẩn hóa.
- Scheduler quản lý toàn bộ Job.
- Failure được cô lập theo từng component.
- Provider được quản lý tập trung.
- Session được quản lý tập trung.
- Các component chỉ giao tiếp thông qua Contract hoặc Event.