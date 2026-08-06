# Telemetry Errors

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Telemetry
> **Document:** Errors and Warnings
> **Path:** `03-infrastructure/telemetry/ERRORS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-06

---

# 1. Purpose

Tài liệu này định nghĩa toàn bộ **lỗi (Errors)** và **cảnh báo (Warnings)** của module Telemetry.

Mục tiêu:

* Chuẩn hóa mọi lỗi trong quá trình thu thập telemetry.
* Phân biệt lỗi của Telemetry với lỗi nghiệp vụ.
* Không làm rò rỉ dữ liệu nhạy cảm.
* Hỗ trợ retry và recovery một cách nhất quán.
* Cho phép thay đổi implementation mà không thay đổi hợp đồng (contract).

Telemetry là **best-effort infrastructure**, vì vậy phần lớn lỗi của Telemetry **không được làm gián đoạn luồng nghiệp vụ**.

---

# 2. Error Design Principles

Mọi lỗi của Telemetry phải tuân theo các nguyên tắc:

* Không chứa dữ liệu người dùng.
* Không chứa OCR result.
* Không chứa Translation result.
* Không chứa Prompt.
* Không chứa Secret.
* Không chứa Password, Token hoặc Credential.
* Có khả năng correlation.
* Có thể phân loại theo category.
* Có thể xác định khả năng retry.
* Có khả năng export an toàn.

---

# 3. Canonical Error Model

```text
TelemetryError {

    errorId

    code

    category

    scope

    severity

    retryClass

    recoverability

    safeMessage

    correlationId

    traceId

    spanId

    occurredAt

    metadata

}
```

---

# 4. Severity

```text
NOTICE
WARNING
ERROR
CRITICAL
FATAL
```

Ý nghĩa:

* **NOTICE**: hành vi mong đợi hoặc cần điều chỉnh.
* **WARNING**: hệ thống vẫn hoạt động nhưng bị suy giảm.
* **ERROR**: thao tác hiện tại thất bại.
* **CRITICAL**: ảnh hưởng đến khả năng quan sát.
* **FATAL**: Telemetry không còn hoạt động.

---

# 5. Retry Classes

```text
NEVER

TRANSIENT

AFTER_CONFIGURATION_CHANGE

AFTER_RECOVERY

AFTER_RESTART

UNKNOWN
```

---

# 6. Error Categories

```text
REGISTRY

METRIC

COUNTER

GAUGE

HISTOGRAM

TIMER

TRACE

SPAN

SAMPLING

AGGREGATION

EXPORT

EXPORTER

HEALTH

COLLECTOR

RUNTIME

RESOURCE

CONFIGURATION

SHUTDOWN

SECURITY

INTERNAL
```

---

# 7. Registry Errors

```text
TELEMETRY_REGISTRY_NOT_INITIALIZED

TELEMETRY_REGISTRY_LOCKED

TELEMETRY_METRIC_ALREADY_REGISTERED

TELEMETRY_METRIC_NOT_FOUND

TELEMETRY_METRIC_NAME_INVALID

TELEMETRY_METRIC_TYPE_CONFLICT
```

---

# 8. Counter Errors

```text
TELEMETRY_COUNTER_OVERFLOW

TELEMETRY_COUNTER_FROZEN

TELEMETRY_COUNTER_INCREMENT_INVALID
```

Counter không được giảm.

---

# 9. Gauge Errors

```text
TELEMETRY_GAUGE_FROZEN

TELEMETRY_GAUGE_VALUE_INVALID
```

---

# 10. Histogram Errors

```text
TELEMETRY_HISTOGRAM_BUCKET_INVALID

TELEMETRY_HISTOGRAM_OBSERVATION_INVALID

TELEMETRY_HISTOGRAM_FROZEN
```

---

# 11. Timer Errors

```text
TELEMETRY_TIMER_ALREADY_STARTED

TELEMETRY_TIMER_NOT_RUNNING

TELEMETRY_TIMER_ALREADY_STOPPED
```

---

# 12. Trace Errors

```text
TELEMETRY_TRACE_ALREADY_COMPLETED

TELEMETRY_TRACE_PARENT_INVALID

TELEMETRY_TRACE_NOT_FOUND

TELEMETRY_TRACE_CORRUPTED
```

---

# 13. Span Errors

```text
TELEMETRY_SPAN_ALREADY_COMPLETED

TELEMETRY_SPAN_PARENT_INVALID

TELEMETRY_SPAN_HIERARCHY_INVALID

TELEMETRY_SPAN_STATUS_INVALID
```

---

# 14. Correlation Errors

```text
TELEMETRY_CORRELATION_MISSING

TELEMETRY_CORRELATION_CONFLICT

TELEMETRY_CORRELATION_INVALID
```

---

# 15. Sampling Errors

```text
TELEMETRY_SAMPLING_CONFIGURATION_INVALID

TELEMETRY_SAMPLING_POLICY_CONFLICT

TELEMETRY_SAMPLING_ENGINE_UNAVAILABLE
```

Sampling lỗi không được ảnh hưởng business.

---

# 16. Aggregation Errors

```text
TELEMETRY_AGGREGATION_FAILED

TELEMETRY_AGGREGATION_CORRUPTED

TELEMETRY_AGGREGATION_TIMEOUT
```

Aggregation có thể retry.

---

# 17. Runtime Collector Errors

```text
TELEMETRY_RUNTIME_COLLECTOR_UNAVAILABLE

TELEMETRY_RUNTIME_COLLECTION_TIMEOUT

TELEMETRY_RUNTIME_COLLECTION_FAILED
```

---

# 18. Resource Collector Errors

```text
TELEMETRY_RESOURCE_COLLECTOR_UNAVAILABLE

TELEMETRY_RESOURCE_SNAPSHOT_INVALID

TELEMETRY_RESOURCE_COLLECTION_TIMEOUT
```

---

# 19. Health Errors

```text
TELEMETRY_HEALTH_REPORT_INVALID

TELEMETRY_HEALTH_STATE_CONFLICT

TELEMETRY_HEALTH_COLLECTOR_FAILED
```

---

# 20. Export Queue Errors

```text
TELEMETRY_EXPORT_QUEUE_FULL

TELEMETRY_EXPORT_QUEUE_CORRUPTED

TELEMETRY_EXPORT_QUEUE_TIMEOUT
```

Queue đầy không được block producer vô hạn.

---

# 21. Metrics Export Errors

```text
TELEMETRY_METRICS_EXPORT_FAILED

TELEMETRY_METRICS_EXPORT_TIMEOUT

TELEMETRY_METRICS_EXPORT_PARTIAL

TELEMETRY_METRICS_EXPORT_REJECTED
```

---

# 22. Trace Export Errors

```text
TELEMETRY_TRACE_EXPORT_FAILED

TELEMETRY_TRACE_EXPORT_TIMEOUT

TELEMETRY_TRACE_EXPORT_PARTIAL

TELEMETRY_TRACE_EXPORT_REJECTED
```

---

# 23. Exporter Errors

```text
TELEMETRY_EXPORTER_NOT_INITIALIZED

TELEMETRY_EXPORTER_UNAVAILABLE

TELEMETRY_EXPORTER_CONFIGURATION_INVALID

TELEMETRY_EXPORTER_DEGRADED
```

Exporter có thể recover độc lập.

---

# 24. Configuration Errors

```text
TELEMETRY_CONFIGURATION_INVALID

TELEMETRY_CONFIGURATION_CONFLICT

TELEMETRY_CONFIGURATION_RELOAD_FAILED
```

---

# 25. Shutdown Errors

```text
TELEMETRY_FLUSH_TIMEOUT

TELEMETRY_SHUTDOWN_TIMEOUT

TELEMETRY_TERMINATION_FAILED
```

Shutdown luôn bounded.

---

# 26. Security Errors

```text
TELEMETRY_SECRET_DETECTED

TELEMETRY_UNSAFE_METADATA

TELEMETRY_PRIVATE_CONTENT_DETECTED

TELEMETRY_LABEL_CARDINALITY_EXCEEDED
```

Các lỗi này phải fail-safe.

---

# 27. Internal Errors

```text
TELEMETRY_INVALID_STATE_TRANSITION

TELEMETRY_STATE_CORRUPTED

TELEMETRY_INTERNAL_INVARIANT_BROKEN

TELEMETRY_UNEXPECTED_EXCEPTION
```

---

# 28. Warning Model

```text
TelemetryWarning {

    warningId

    code

    scope

    safeMessage

    metadata

}
```

---

# 29. Standard Warnings

```text
TELEMETRY_WARNING_EXPORT_DELAYED

TELEMETRY_WARNING_QUEUE_PRESSURE

TELEMETRY_WARNING_HIGH_CARDINALITY

TELEMETRY_WARNING_PARTIAL_EXPORT

TELEMETRY_WARNING_DEGRADED_EXPORTER

TELEMETRY_WARNING_RUNTIME_DELAY

TELEMETRY_WARNING_RESOURCE_DELAY
```

Warnings không làm dừng Telemetry.

---

# 30. Retry Policy

Có thể retry:

* Aggregation.
* Export.
* Collector.
* Runtime Snapshot.
* Resource Snapshot.

Không retry:

* Invalid configuration.
* Invalid metric name.
* Invalid span hierarchy.
* Security violation.

---

# 31. Recovery Policy

Recovery bao gồm:

* restart exporter;
* restart collector;
* reload configuration;
* rebuild aggregation cache;
* flush queue.

Recovery không được làm mất tính nhất quán của metric.

---

# 32. Failure Semantics

Nếu:

```text
Exporter
    ↓
FAILED
```

thì:

```text
Telemetry
    ↓
DEGRADED
```

không phải:

```text
TERMINATED
```

Chỉ khi hầu hết thành phần cốt lõi đều thất bại mới chuyển sang trạng thái FATAL.

---

# 33. Error Correlation

Mọi lỗi có thể mang:

```text
correlationId

traceId

spanId

module

timestamp
```

Không mang:

* userId
* OCR text
* translated text
* image
* prompt
* secret

---

# 34. Security Rules

Không được log:

* Secret.
* Password.
* Token.
* Authorization Header.
* OCR result.
* Translation result.
* Prompt.
* Payload.

Nếu phát hiện phải chuyển thành lỗi:

```text
TELEMETRY_SECRET_DETECTED
```

hoặc:

```text
TELEMETRY_UNSAFE_METADATA
```

---

# 35. State Implications

Ví dụ:

```text
EXPORT_FAILED
```

↓

```text
Exporter = DEGRADED
```

↓

```text
Telemetry = DEGRADED
```

Hoặc:

```text
CONFIGURATION_INVALID
```

↓

```text
Exporter không khởi tạo
```

↓

```text
TelemetryReady = false
```

---

# 36. Testing Requirements

Phải kiểm thử:

* duplicate metric;
* invalid metric name;
* invalid span hierarchy;
* exporter timeout;
* queue full;
* aggregation timeout;
* runtime timeout;
* resource timeout;
* configuration conflict;
* exporter recovery;
* secret detection;
* cardinality limit;
* shutdown timeout;
* flush timeout.

---

# 37. MVP Errors

Bắt buộc hỗ trợ:

```text
TELEMETRY_REGISTRY_NOT_INITIALIZED

TELEMETRY_METRIC_ALREADY_REGISTERED

TELEMETRY_COUNTER_OVERFLOW

TELEMETRY_TRACE_ALREADY_COMPLETED

TELEMETRY_SPAN_ALREADY_COMPLETED

TELEMETRY_EXPORT_QUEUE_FULL

TELEMETRY_METRICS_EXPORT_FAILED

TELEMETRY_TRACE_EXPORT_FAILED

TELEMETRY_EXPORTER_UNAVAILABLE

TELEMETRY_CONFIGURATION_INVALID

TELEMETRY_FLUSH_TIMEOUT

TELEMETRY_SECRET_DETECTED

TELEMETRY_INVALID_STATE_TRANSITION
```

---

# 38. Related Documents

```text
MODULE.md

CONTRACT.md

STATES.md

EVENTS.md

README.md
```

---

# 39. Summary

Telemetry Errors chuẩn hóa toàn bộ lỗi phát sinh trong quá trình thu thập, tổng hợp và xuất telemetry của CRAI.

Các lỗi được phân loại theo từng thành phần như Registry, Metrics, Trace, Exporter, Collector và Health. Tất cả đều tuân thủ nguyên tắc **safe metadata**, **best-effort**, **bounded recovery** và **không ảnh hưởng trực tiếp đến luồng nghiệp vụ**, giúp hệ thống duy trì khả năng quan sát ngay cả khi một phần của hạ tầng telemetry bị suy giảm.
