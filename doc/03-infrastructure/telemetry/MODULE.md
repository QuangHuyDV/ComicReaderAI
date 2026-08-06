# Telemetry Module

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Telemetry
> **Document:** Module Definition
> **Path:** `03-infrastructure/telemetry/MODULE.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-06

---

# 1. Purpose

Telemetry là hạ tầng **quan sát (Observability)** của CRAI.

Nhiệm vụ của Telemetry là thu thập, tổng hợp và xuất bản các tín hiệu (signals) phản ánh trạng thái vận hành của toàn bộ hệ thống mà **không chứa dữ liệu nghiệp vụ hoặc nội dung người dùng**.

Telemetry giúp trả lời các câu hỏi như:

* Hệ thống đang khỏe hay không?
* Module nào đang chậm?
* OCR mất bao lâu?
* Translation provider nào đang timeout?
* Cache hit rate là bao nhiêu?
* Queue đang backlog bao nhiêu?
* GPU có đang quá tải?
* Module nào tạo nhiều lỗi nhất?
* Workflow nào thường xuyên thất bại?

Telemetry không thay thế Logging và cũng không thay thế Event Bus.

---

# 2. Responsibilities

Telemetry chịu trách nhiệm:

* Metrics collection
* Distributed tracing
* Runtime statistics
* Performance measurement
* Latency measurement
* Resource usage
* Queue metrics
* Cache metrics
* Pipeline metrics
* Error metrics
* Health metrics
* Observability aggregation
* Metrics export
* Trace export
* Correlation support

---

# 3. Non-Goals

Telemetry không chịu trách nhiệm:

* ghi log

* lưu business data

* lưu OCR result

* lưu Translation result

* audit

* event delivery

* workflow orchestration

* analytics dashboard

* business reporting

* persistence của application data

---

# 4. Design Principles

## 4.1 Signal, not Data

Telemetry chỉ thu thập:

```text
latency

duration

counter

gauge

histogram

status

health

resource usage
```

Không thu thập:

```text
translated text

OCR text

comic image

novel content

API payload

secret

credential
```

---

## 4.2 Observability First

Mọi module đều có khả năng phát sinh telemetry.

Ví dụ:

```text
OCR

Translation

Image Processing

Cache

Storage

Networking

Presentation

Runtime

Scheduler

Secret Management

Logging
```

---

## 4.3 Low Overhead

Telemetry phải:

* lock-free khi có thể
* allocation tối thiểu
* async
* bounded memory
* sampling được

Telemetry không được trở thành bottleneck.

---

## 4.4 Safe by Default

Telemetry không được:

* chứa secret
* chứa password
* chứa token
* chứa cookie
* chứa OCR content
* chứa translated text
* chứa prompt
* chứa API response
* chứa Authorization header

---

## 4.5 Independent from Logging

Telemetry và Logging là hai module riêng biệt.

Logging ghi nhận:

```text
What happened?
```

Telemetry đo lường:

```text
How healthy?

How fast?

How often?

How much?
```

---

# 5. Signals

Telemetry quản lý ba nhóm tín hiệu chính.

## Metrics

Ví dụ:

```text
requests_total

translation_latency

ocr_duration

gpu_usage

cache_hit_rate

queue_depth

memory_usage
```

---

## Traces

Ví dụ:

```text
Open Reader

↓

Load Page

↓

OCR

↓

Translate

↓

Layout

↓

Render
```

---

## Runtime Health

Ví dụ:

```text
CPU

Memory

GPU

Disk

Queue

Thread Pool

Network

Module Health
```

---

# 6. Architecture Position

```text
Feature Modules
        │
        ▼
Telemetry API
        │
        ▼
Aggregation
        │
        ├──── Metrics
        │
        ├──── Traces
        │
        └──── Runtime Health
                │
                ▼
Exporters
```

Telemetry không được gọi ngược vào business logic.

---

# 7. Dependencies

Telemetry phụ thuộc vào:

* Runtime
* Configuration
* Clock
* Secret Management (để bảo vệ metadata nhạy cảm)

Có thể tích hợp với:

* Logging
* Event Bus

Nhưng không được phụ thuộc vòng.

---

# 8. Public Capabilities

Telemetry cung cấp:

```text
Counter

Gauge

Histogram

Timer

Span

Trace

Health Check

Exporter

Sampling

Correlation
```

---

# 9. Internal Components

Dự kiến module bao gồm:

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

Exporter

Health Collector

Runtime Collector

Resource Monitor

Telemetry Policy

Telemetry Buffer
```

---

# 10. Integration

Telemetry được sử dụng bởi:

```text
OCR

Translation

Image Pipeline

Networking

Storage

Scheduler

Presentation

Logging

Secret Management

Runtime
```

---

# 11. Security

Telemetry phải:

* loại bỏ dữ liệu nhạy cảm
* chỉ giữ metadata an toàn
* chuẩn hóa labels
* giới hạn cardinality
* không export secret
* không export prompt
* không export raw content

---

# 12. Performance

Telemetry phải hỗ trợ:

* async collection
* lock-free counter
* bounded queue
* batch export
* adaptive sampling
* aggregation
* low allocation
* low CPU overhead

---

# 13. Failure Policy

Nếu Telemetry lỗi:

* Application vẫn tiếp tục chạy.
* Logging vẫn hoạt động.
* OCR vẫn hoạt động.
* Translation vẫn hoạt động.
* Rendering vẫn hoạt động.

Telemetry là **best-effort infrastructure**.

---

# 14. Relationship with Other Modules

| Module            | Relationship                                     |
| ----------------- | ------------------------------------------------ |
| Logging           | Logging ghi sự kiện, Telemetry đo lường hệ thống |
| Event Bus         | Event Bus truyền sự kiện, Telemetry quan sát     |
| Runtime           | Runtime cung cấp trạng thái hệ thống             |
| Secret Management | Bảo đảm metadata không rò rỉ bí mật              |
| Configuration     | Điều khiển sampling, exporters, retention        |

---

# 15. Future Extensions

Có thể mở rộng:

* OpenTelemetry
* Prometheus
* OTLP
* Jaeger
* Zipkin
* Grafana
* Cloud exporters
* GPU profiling
* AI inference profiling
* Distributed tracing
* Adaptive telemetry sampling

---

# 16. Module Boundaries

Telemetry sở hữu:

* metrics
* traces
* spans
* health
* resource monitoring
* exporters
* telemetry aggregation

Telemetry không sở hữu:

* logs
* events
* OCR
* translation
* storage
* authentication
* business workflows

---

# 17. Success Criteria

Telemetry được xem là hoàn chỉnh khi:

* mọi module đều có thể publish metrics
* mọi workflow đều có trace
* mọi module đều có health state
* metrics có sampling
* traces có correlation
* exporters hoạt động độc lập
* không rò rỉ dữ liệu nhạy cảm
* overhead thấp
* không ảnh hưởng luồng nghiệp vụ

---

# 18. Related Documents

```text
03-infrastructure/telemetry/
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
├── ERRORS.md
└── README.md
```

Tài liệu kiến trúc liên quan:

```text
docs/architecture/STATE_MACHINE.md

docs/architecture/MODULE_DEPENDENCY.md

docs/architecture/DATA_FLOW.md

docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

docs/architecture/runtime/ERROR_MODEL.md

03-infrastructure/logging/

03-infrastructure/event-bus/

03-infrastructure/secret-management/
```

---

# 19. Summary

Telemetry là nền tảng Observability của CRAI.

Module chịu trách nhiệm thu thập **Metrics, Traces và Runtime Health** nhằm cung cấp khả năng quan sát toàn bộ hệ thống với chi phí thấp, không làm gián đoạn luồng xử lý và không lưu trữ dữ liệu nhạy cảm.

Telemetry hoạt động độc lập với Logging và Event Bus, hỗ trợ toàn bộ các module trong hệ thống thông qua cơ chế thu thập tín hiệu chuẩn hóa, sampling, aggregation và export, đồng thời tuân thủ các nguyên tắc **Safe by Default**, **Low Overhead** và **Best Effort**.
