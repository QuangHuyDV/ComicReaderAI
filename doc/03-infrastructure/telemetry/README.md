# Telemetry

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Telemetry
> **Document:** README
> **Path:** `03-infrastructure/telemetry/README.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-06

---

# 1. Overview

Telemetry là nền tảng **Observability** của CRAI.

Module chịu trách nhiệm thu thập, tổng hợp và xuất bản các tín hiệu vận hành của toàn bộ hệ thống, giúp trả lời các câu hỏi:

* Hệ thống có đang hoạt động bình thường không?
* Module nào đang chậm?
* OCR mất bao lâu?
* Translation provider nào có tỷ lệ lỗi cao?
* Queue có đang bị backlog?
* GPU và CPU đang sử dụng bao nhiêu?
* Cache có hoạt động hiệu quả không?
* Workflow nào thường xuyên thất bại?

Telemetry chỉ quan tâm đến **tín hiệu vận hành (Operational Signals)**, không quan tâm đến dữ liệu nghiệp vụ.

---

# 2. Goals

Telemetry hướng đến các mục tiêu:

* Chuẩn hóa observability cho toàn bộ CRAI.
* Thu thập metrics theo thời gian thực.
* Theo dõi distributed traces.
* Giám sát tài nguyên hệ thống.
* Cung cấp health status cho từng module.
* Hỗ trợ debugging và performance tuning.
* Hỗ trợ monitoring và alerting.
* Hoạt động với chi phí thấp và không ảnh hưởng đến nghiệp vụ.

---

# 3. Non-Goals

Telemetry **không** chịu trách nhiệm:

* Logging.
* Event delivery.
* Audit.
* Business analytics.
* Business reporting.
* OCR.
* Translation.
* Image processing.
* Persistence của dữ liệu nghiệp vụ.
* Dashboard UI.

---

# 4. Core Concepts

Telemetry bao gồm ba nhóm tín hiệu chính.

## 4.1 Metrics

Đo lường giá trị định lượng.

Ví dụ:

```text
translation_requests_total

translation_duration_ms

ocr_duration_ms

cache_hit_rate

queue_depth

memory_usage_bytes

gpu_usage_percent
```

---

## 4.2 Traces

Theo dõi vòng đời của một workflow.

Ví dụ:

```text
Open Reader
      │
      ▼
Load Image
      │
      ▼
OCR
      │
      ▼
Translation
      │
      ▼
Layout
      │
      ▼
Rendering
```

---

## 4.3 Health

Theo dõi trạng thái của module.

Ví dụ:

```text
HEALTHY

DEGRADED

UNAVAILABLE
```

Health phản ánh **khả năng hoạt động của hạ tầng**, không phản ánh kết quả nghiệp vụ.

---

# 5. High-Level Architecture

```text
Feature Modules
        │
        ▼
Telemetry API
        │
        ▼
Registry
        │
        ▼
Aggregation
        │
 ┌──────┼─────────┐
 ▼      ▼         ▼
Metrics Traces Health
        │
        ▼
Exporters
        │
        ▼
Monitoring Backend
```

Producer chỉ giao tiếp với **Telemetry API**.

---

# 6. Responsibilities

Module chịu trách nhiệm:

* Metrics Registry
* Counter
* Gauge
* Histogram
* Timer
* Trace
* Span
* Health Reporting
* Runtime Collection
* Resource Collection
* Aggregation
* Sampling
* Correlation
* Export
* Flush
* Recovery

---

# 7. Internal Components

```text
Telemetry API
Metrics Registry
Counter Manager
Gauge Manager
Histogram Manager
Timer Manager
Trace Manager
Span Manager
Correlation Manager
Sampler
Aggregation Engine
Runtime Collector
Resource Collector
Health Reporter
Export Queue
Exporter
```

Mỗi thành phần có lifecycle và state machine riêng.

---

# 8. Integration

Telemetry tích hợp với:

| Module            | Vai trò                                                |
| ----------------- | ------------------------------------------------------ |
| Runtime           | Thu thập trạng thái hệ thống                           |
| Logging           | Ghi nhận lỗi của Telemetry theo cơ chế guarded logging |
| Configuration     | Điều khiển sampling, exporter và policy                |
| Secret Management | Kiểm tra metadata nhạy cảm                             |
| Event Bus         | Nhận lifecycle/configuration events nếu cần            |
| Storage           | Đọc thông tin tài nguyên lưu trữ                       |

Telemetry không được tạo dependency vòng với các module này.

---

# 9. Security Model

Telemetry chỉ thu thập **metadata an toàn**.

Được phép:

* module
* operation
* duration
* latency
* status
* errorCode
* resourceClass

Không được phép:

* OCR text
* translated text
* comic image
* novel content
* prompt
* API payload
* password
* token
* cookie
* authorization header
* secret
* private key

Nếu phát hiện dữ liệu không an toàn, Telemetry phải từ chối ghi nhận và phát sinh lỗi phù hợp theo `ERRORS.md`.

---

# 10. Performance Model

Telemetry phải đảm bảo:

* Async processing.
* Low allocation.
* Lock-free khi phù hợp.
* Bounded queue.
* Batch export.
* Sampling.
* Aggregation.
* Minimal CPU overhead.
* Không block producer.

Telemetry được thiết kế theo mô hình **best-effort**, không làm chậm luồng xử lý chính.

---

# 11. Lifecycle

```text
UNINITIALIZED
      │
      ▼
INITIALIZING
      │
      ▼
READY
      │
      ▼
RUNNING
      │
 ┌────┴────┐
 ▼         ▼
DEGRADED STOPPING
             │
             ▼
         FLUSHING
             │
             ▼
        TERMINATED
```

Lifecycle chi tiết được định nghĩa trong `STATES.md`.

---

# 12. Failure Model

Một số nguyên tắc chính:

* Exporter lỗi không làm dừng Producer.
* Collector lỗi không làm dừng Runtime.
* Queue đầy không block vô hạn.
* Aggregation lỗi có thể retry.
* Shutdown luôn bounded.
* Recovery thực hiện độc lập cho từng thành phần.

Telemetry có thể chuyển sang trạng thái **DEGRADED** nhưng vẫn tiếp tục hoạt động khi còn khả năng.

---

# 13. Relationship with Logging

Logging và Telemetry bổ sung cho nhau.

| Logging                    | Telemetry                               |
| -------------------------- | --------------------------------------- |
| Ghi nhận điều gì đã xảy ra | Đo lường hệ thống hoạt động như thế nào |
| Structured log             | Metrics / Traces / Health               |
| Điều tra sự cố             | Quan sát xu hướng và hiệu năng          |
| Event-oriented             | Signal-oriented                         |

Logging không thay thế Telemetry và ngược lại.

---

# 14. Relationship with Event Bus

Event Bus truyền tải sự kiện.

Telemetry quan sát hệ thống.

Telemetry có thể nhận lifecycle event từ Event Bus nhưng không dùng Event Bus làm kênh truyền metrics theo thời gian thực.

---

# 15. Relationship with Secret Management

Secret Management giúp:

* phát hiện metadata nhạy cảm;
* ngăn telemetry export dữ liệu bí mật;
* bảo đảm observability không trở thành nguồn rò rỉ thông tin.

---

# 16. Observability Pipeline

```text
Producer
    │
    ▼
Telemetry API
    │
    ▼
Registry
    │
    ▼
Aggregation
    │
    ▼
Sampling
    │
    ▼
Export Queue
    │
    ▼
Exporter
    │
    ▼
Monitoring Backend
```

Pipeline luôn theo hướng một chiều.

---

# 17. Observability Principles

Telemetry tuân thủ các nguyên tắc:

* Safe by Default.
* Immutable Signals.
* Low Cardinality.
* Bounded Memory.
* Correlation First.
* Best Effort.
* Fail Safe.
* Async by Default.

---

# 18. MVP Scope

MVP bắt buộc hỗ trợ:

* Counter
* Gauge
* Histogram
* Timer
* Trace
* Span
* Health
* Runtime Collector
* Resource Collector
* Metrics Export
* Trace Export
* Sampling
* Correlation

Các tính năng có thể bổ sung sau:

* OpenTelemetry SDK.
* OTLP Exporter.
* Prometheus Exporter.
* Jaeger.
* Zipkin.
* Grafana integration.
* Adaptive Sampling.
* AI Inference Profiling.
* GPU Profiling.

---

# 19. Related Documents

## Trong module

```text
MODULE.md
CONTRACT.md
STATES.md
EVENTS.md
ERRORS.md
README.md
```

## Kiến trúc chung

```text
docs/architecture/STATE_MACHINE.md
docs/architecture/DATA_FLOW.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md
docs/architecture/runtime/ERROR_MODEL.md
```

## Module liên quan

```text
03-infrastructure/logging/
03-infrastructure/event-bus/
03-infrastructure/secret-management/
03-infrastructure/configuration/
```

---

# 20. Module Completion Checklist

Một implementation của Telemetry được xem là hoàn chỉnh khi:

* Có Metrics Registry.
* Có Counter/Gauge/Histogram/Timer.
* Có Trace và Span.
* Có Correlation.
* Có Runtime Collector.
* Có Resource Collector.
* Có Health Reporter.
* Có Sampling.
* Có Aggregation.
* Có Export Queue.
* Có Exporter.
* Có Flush và Shutdown.
* Có Recovery.
* Có đầy đủ State, Events và Errors.
* Không rò rỉ dữ liệu nhạy cảm.

---

# 21. Summary

Telemetry là nền tảng Observability của CRAI, chịu trách nhiệm thu thập **Metrics**, **Traces** và **Health Signals** để phản ánh trạng thái vận hành của toàn bộ hệ thống.

Module hoạt động độc lập với Logging và Event Bus, tuân thủ các nguyên tắc **Safe by Default**, **Best Effort**, **Immutable Signals** và **Low Overhead**, đồng thời cung cấp một API thống nhất để mọi module trong CRAI có thể quan sát và đo lường hiệu năng mà không ảnh hưởng đến luồng xử lý nghiệp vụ.

README này là điểm khởi đầu của module Telemetry. Mọi chi tiết triển khai, state machine, contract, event và error được định nghĩa trong các tài liệu chuyên biệt còn lại của module.
