# Logging

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Logging
> **Document:** README
> **Path:** `03-infrastructure/logging/README.md`

# 1. Purpose

Logging là hạ tầng ghi nhận toàn bộ hoạt động của CRAI theo mô hình **structured, safe, bounded và observable**.

Mục tiêu:

- Thu thập log chuẩn hóa.
- Không làm rò rỉ secret hoặc dữ liệu người dùng.
- Hỗ trợ debug và diagnostics.
- Hỗ trợ audit.
- Hoạt động ổn định ngay cả khi hệ thống suy giảm.

---

# 2. Responsibilities

Module chịu trách nhiệm:

- Structured logging
- Context & scope management
- Exception normalization
- Safety inspection
- Redaction
- Log admission
- Buffer management
- Sink routing
- Batch writing
- File rotation
- Retention
- Compression
- Diagnostics query & export
- Audit logging
- Bootstrap logger
- Emergency logger

---

# 3. Non-goals

Module này không:

- thay thế Event Bus
- thay thế Telemetry
- lưu business data
- làm search engine
- làm analytics
- làm audit authority

---

# 4. High-level Pipeline

```text
Producer
    ↓
Validation
    ↓
Safety Inspection
    ↓
Redaction
    ↓
Admission
    ↓
Buffer
    ↓
Sink Routing
    ↓
Batch Write
    ↓
File / Console / Remote Sink
```

---

# 5. Lifecycle

```text
BOOTSTRAPPING
    ↓
INITIALIZING
    ↓
READY
    ↓
RUNNING
    ↓
DEGRADED
    ↓
QUIESCING
    ↓
DRAINING
    ↓
FLUSHING
    ↓
STOPPING
    ↓
TERMINATED
```

---

# 6. Related Components

Logging phối hợp với:

- Configuration
- Secret Management
- Event Bus
- Telemetry
- Runtime
- Storage

---

# 7. Documents

| Document | Purpose |
|----------|---------|
| MODULE.md | Module overview |
| CONTRACT.md | Public contracts |
| STATES.md | State machines |
| EVENTS.md | Integration events |
| ERRORS.md | Error model |

---

# 8. Security Principles

- Không log secret.
- Không log raw OCR.
- Không log translated text.
- Không log raw provider payload.
- Không log password/token/key.
- Fail Closed khi không chứng minh được an toàn.
- Restricted record không được route sang sink yếu hơn.
- Audit tách biệt với ordinary logging.

---

# 9. Performance Principles

- Async pipeline.
- Bounded buffer.
- Batch write.
- Sampling.
- Duplicate suppression.
- File rotation.
- Retention cleanup.

---

# 10. MVP

Bắt buộc:

- Structured logging
- Safety inspection
- Redaction
- Local file sink
- Console sink
- Rotation
- Retention
- Diagnostics export
- Audit sink
- Emergency logger

Có thể bổ sung sau:

- Remote sink
- Cloud logging
- Search/index
- Tamper-evident audit
- Encrypted archive

---

# 11. Architecture Summary

Logging đảm bảo:

- Structured logs.
- Immutable records.
- Safe redaction.
- Bounded memory.
- Reliable shutdown.
- Guarded self-reporting.
- Tách biệt giữa admission và persistence.
- Tách biệt giữa logging và audit.

Toàn bộ hành vi chi tiết được định nghĩa trong:

- MODULE.md
- CONTRACT.md
- STATES.md
- EVENTS.md
- ERRORS.md

README là tài liệu tổng quan và entry point của module Logging.
