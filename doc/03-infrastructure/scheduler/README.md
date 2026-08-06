# Scheduler

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Scheduler

## Purpose

Scheduler là hạ tầng chịu trách nhiệm lập lịch, tạo Job, điều phối thực thi và quản lý vòng đời của các công việc bất đồng bộ trong CRAI.

Scheduler **không** thực hiện nghiệp vụ (OCR, Translation, Storage...). Nó chỉ quyết định **khi nào**, **bao nhiêu**, **ở đâu** và **theo chính sách nào** một Job được chạy.

---

# Responsibilities

Scheduler chịu trách nhiệm:

- quản lý Task Definition
- quản lý Schedule
- Cron / Interval / One-shot / Event Trigger
- tạo Job và Attempt
- Queue admission
- Dispatch tới Worker
- Retry
- Timeout
- Cancellation
- Concurrency limit
- Resource reservation
- Dependency scheduling
- Shutdown & Drain
- Recovery (khi hỗ trợ persistence)

Không chịu trách nhiệm:

- business logic
- OCR
- Translation
- Image Processing
- Logging
- Telemetry
- Event Bus

---

# Public APIs

- RegisterTask()
- UpdateTask()
- RemoveTask()
- RegisterSchedule()
- RemoveSchedule()
- TriggerNow()
- CancelJob()
- PauseSchedule()
- ResumeSchedule()

---

# Internal Components

- Scheduler Core
- Trigger Engine
- Job Builder
- Queue
- Dispatcher
- Worker Manager
- Retry Manager
- Timeout Manager
- Resource Manager
- Recovery Manager

---

# Lifecycle

```text
Initialize
    ↓
Load configuration
    ↓
Register tasks
    ↓
Register schedules
    ↓
Running
    ↓
Dispatch / Retry / Timeout
    ↓
Drain
    ↓
Shutdown
```

---

# State Model

Các state chính được định nghĩa tại:

- STATES.md

Bao gồm:

- Scheduler
- Task
- Schedule
- Trigger
- Occurrence
- Job
- Attempt
- Worker
- Queue
- Recovery
- Shutdown

---

# Event Model

Xem:

- EVENTS.md

Scheduler phát các nhóm sự kiện:

- lifecycle
- task
- schedule
- trigger
- occurrence
- queue
- dispatch
- worker
- retry
- timeout
- cancellation
- recovery
- shutdown

---

# Error Model

Xem:

- ERRORS.md

Nguyên tắc:

- Attempt Failure ≠ Job Failure
- Timeout ≠ Cancellation
- Abandonment là trạng thái riêng
- Không bao giờ log payload thô
- Không ngụ ý exactly-once

---

# Integration

Scheduler tích hợp với:

- Event Bus
- Logging
- Telemetry
- Secret Management
- Runtime Queue
- Resource Manager

Business module chỉ đăng ký Task và Schedule.

---

# Data Flow

```text
Business Module
      │
      ▼
Register Task
      │
      ▼
Scheduler
      │
      ▼
Trigger
      │
      ▼
Job
      │
      ▼
Queue
      │
      ▼
Dispatcher
      │
      ▼
Worker
      │
      ▼
Completion / Retry / Timeout
```

---

# Design Decisions

- Scheduler chỉ quản lý điều phối.
- Job immutable sau khi tạo.
- Attempt là đơn vị thực thi.
- Retry tạo Attempt mới.
- Timeout kết thúc authority logic.
- Late completion chỉ dùng cho reconciliation.
- Queue và Resource là hai khái niệm riêng.
- Retry luôn bị giới hạn.
- Shutdown luôn bounded.

---

# MVP

MVP bao gồm:

- In-memory Scheduler
- Cron / Interval / One-shot
- Manual Trigger
- Retry cơ bản
- Timeout
- Cancellation
- Queue
- Worker Pool
- Logging
- Telemetry
- Event Bus integration

Chưa bao gồm:

- Durable persistence
- Distributed scheduler
- Leader election
- Cross-node lease
- HA recovery

---

# Related Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- EVENTS.md
- ERRORS.md

Kiến trúc liên quan:

- docs/architecture/STATE_MACHINE.md
- docs/architecture/DATA_FLOW.md
- docs/architecture/MODULE_DEPENDENCY.md

---

# Summary

Scheduler là trung tâm điều phối thực thi của CRAI.

Nó chịu trách nhiệm chuyển đổi từ **Task → Schedule → Job → Attempt**, đảm bảo việc thực thi được giới hạn tài nguyên, có retry, timeout, cancellation, khả năng quan sát (observability) và khả năng mở rộng, đồng thời tách biệt hoàn toàn khỏi business logic.
