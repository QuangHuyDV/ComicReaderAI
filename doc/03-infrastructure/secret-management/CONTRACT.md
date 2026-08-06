# Telemetry Contract

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Telemetry
> **Document:** Public Contracts
> **Path:** `03-infrastructure/telemetry/CONTRACT.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-06

---

# 1. Purpose

Tài liệu này định nghĩa toàn bộ **public contract**, **interface**, **DTO**, **value object** và **invariants** của module Telemetry.

Contract đảm bảo:

* mọi module phát telemetry theo cùng chuẩn;
* metrics, traces và health có lifecycle thống nhất;
* exporter có thể thay thế mà không ảnh hưởng producer;
* không có telemetry nào mang dữ liệu nhạy cảm;
* tương thích với OpenTelemetry nhưng không phụ thuộc trực tiếp.

---

# 2. Design Principles

## 2.1 Stable API

Producer chỉ làm việc với abstraction.

```text
Feature Module
      │
      ▼
Telemetry API
      │
      ▼
Implementation
```

---

## 2.2 Immutable Signals

Sau khi publish:

* Metric không sửa.
* Span đã End không sửa.
* Trace đã Complete không sửa.

---

## 2.3 Safe Metadata

Metadata chỉ được chứa:

```text
module
operation
duration
status
errorCode
resourceClass
provider
```

Không được chứa:

```text
OCR text
translated text
secret
token
password
prompt
image
payload
authorization
```

---

# 3. Public Interfaces

## 3.1 TelemetryService

```text
interface TelemetryService {

    Counter counter(name)

    Gauge gauge(name)

    Histogram histogram(name)

    Timer timer(name)

    Trace createTrace(...)

    Span createSpan(...)

    HealthReporter health()

    MetricsExporter exporter()

}
```

Là entry point của module.

---

## 3.2 Counter

```text
interface Counter {

    increment()

    increment(value)

}
```

Contract:

* chỉ tăng
* không giảm
* thread-safe

Ví dụ:

```text
translation_requests_total
```

---

## 3.3 Gauge

```text
interface Gauge {

    set(value)

    increase()

    decrease()

}
```

Ví dụ:

```text
gpu_usage

memory_usage

queue_depth
```

---

## 3.4 Histogram

```text
interface Histogram {

    observe(value)

}
```

Dùng cho:

* latency
* duration
* payload size
* queue wait

---

## 3.5 Timer

```text
interface Timer {

    start()

    stop()

}
```

Kết quả được ghi vào Histogram.

---

## 3.6 Trace

```text
interface Trace {

    traceId()

    rootSpan()

    end()

}
```

Một Trace có nhiều Span.

---

## 3.7 Span

```text
interface Span {

    spanId()

    parent()

    child()

    attribute()

    event()

    status()

    end()

}
```

Contract:

* parent không đổi
* end đúng một lần
* immutable sau End

---

## 3.8 HealthReporter

```text
interface HealthReporter {

    reportHealthy()

    reportDegraded()

    reportUnavailable()

}
```

---

## 3.9 MetricsExporter

```text
interface MetricsExporter {

    export()

    flush()

}
```

---

## 3.10 TraceExporter

```text
interface TraceExporter {

    export()

    flush()

}
```

---

# 4. Context Objects

## 4.1 TelemetryContext

```text
TelemetryContext {

    traceId

    spanId

    correlationId

    module

    operation

}
```

Không chứa:

* user content
* OCR result
* translated text

---

## 4.2 MetricLabel

```text
MetricLabel {

    key

    value

}
```

Giới hạn:

* cardinality thấp
* không chứa secret
* immutable

---

## 4.3 HealthSnapshot

```text
HealthSnapshot {

    module

    state

    timestamp

    details

}
```

`details` chỉ chứa metadata an toàn.

---

# 5. Metric Contracts

## Counter

Invariants:

```text
>= previous value
```

Không reset bằng API thường.

---

## Gauge

Cho phép:

```text
increase

decrease

set
```

---

## Histogram

Không trả về percentile trực tiếp.

Aggregation thực hiện nội bộ.

---

## Timer

```text
start()

↓

running

↓

stop()

↓

immutable
```

---

# 6. Trace Contracts

Một Trace:

```text
1 Root Span

↓

N Child Span
```

Không có nhiều Root Span.

---

## Span Hierarchy

```text
Root

↓

OCR

↓

Translate

↓

Layout

↓

Render
```

Không tạo vòng lặp.

---

## Span Status

```text
RUNNING

SUCCESS

FAILED

CANCELLED

TIMEOUT
```

Terminal:

```text
SUCCESS

FAILED

TIMEOUT

CANCELLED
```

---

# 7. Health Contracts

Health có ba mức:

```text
HEALTHY

DEGRADED

UNAVAILABLE
```

Health không phản ánh business state.

---

# 8. Export Contracts

Exporter phải:

* async
* bounded
* retry được
* flush được
* shutdown được

Exporter không được block producer.

---

# 9. Sampling Contracts

```text
Sampler {

    shouldSample(...)
}
```

Sampling chỉ ảnh hưởng telemetry.

Không ảnh hưởng business logic.

---

# 10. Aggregation Contracts

Aggregation hỗ trợ:

* Counter
* Histogram
* Gauge
* Trace summary

Aggregation không sửa dữ liệu gốc.

---

# 11. Resource Contracts

```text
ResourceSnapshot {

    cpu

    memory

    gpu

    disk

    network

}
```

Không chứa process memory dump.

---

# 12. Correlation Contracts

Correlation gồm:

```text
traceId

spanId

correlationId
```

Không dùng User ID làm correlation.

---

# 13. Runtime Contracts

Runtime Collector:

```text
collect()

↓

snapshot

↓

publish
```

Không can thiệp Runtime.

---

# 14. Security Contracts

Telemetry không được export:

* OCR result
* Translation result
* Image
* Prompt
* Secret
* Password
* Token
* Cookie
* Authorization Header

---

# 15. Performance Contracts

Tất cả API:

* thread-safe
* async-friendly
* low allocation
* bounded memory

---

# 16. Error Contracts

Producer không nhận exception nội bộ của exporter.

Chỉ nhận:

```text
accepted

ignored

sampled

dropped
```

Chi tiết exporter thuộc `ERRORS.md`.

---

# 17. Lifecycle Contracts

Telemetry:

```text
Initialize

↓

Ready

↓

Running

↓

Degraded

↓

Stopping

↓

Terminated
```

Không publish metric sau Terminated.

---

# 18. Compatibility

Contract tương thích với:

* OpenTelemetry
* Prometheus
* OTLP
* Jaeger
* Zipkin

Không khóa implementation.

---

# 19. Invariants

## Metrics

* immutable sau publish
* bounded labels
* không secret

## Trace

* một Root
* không cycle
* end một lần

## Span

* parent bất biến
* terminal một lần

## Export

* không block producer
* retry bounded

## Health

* chỉ phản ánh hạ tầng

---

# 20. Module Boundary

Telemetry sở hữu:

* Counter
* Gauge
* Histogram
* Timer
* Trace
* Span
* Health
* Exporter
* Sampler
* Aggregator

Không sở hữu:

* Logging
* Event Bus
* Storage
* OCR
* Translation

---

# 21. Related Documents

```text
MODULE.md
STATES.md
EVENTS.md
ERRORS.md
README.md
```

---

# 22. Summary

Contract của Telemetry xác định một API thống nhất cho toàn bộ hệ thống CRAI trong việc phát Metrics, Traces và Health Signals.

Mọi implementation phải đảm bảo:

* immutable signals;
* bounded memory;
* thread-safe;
* asynchronous;
* metadata an toàn;
* không rò rỉ dữ liệu nhạy cảm;
* exporter có thể thay thế mà không thay đổi producer;
* tương thích với các hệ sinh thái observability phổ biến.
