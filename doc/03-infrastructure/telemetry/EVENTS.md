# Telemetry Events

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Telemetry
> **Document:** Internal Events
> **Path:** `03-infrastructure/telemetry/EVENTS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-06

---

# 1. Purpose

Tài liệu này định nghĩa toàn bộ **Telemetry Events** của CRAI.

Các event mô tả:

* lifecycle của Telemetry;
* metrics lifecycle;
* trace/span lifecycle;
* runtime & resource collection;
* aggregation;
* sampling;
* exporter;
* health reporting;
* flush và shutdown;
* degradation và recovery.

Các event trong tài liệu này là **internal infrastructure events**, không phải business events.

---

# 2. Design Principles

Telemetry Events phải tuân theo các nguyên tắc:

* immutable;
* idempotent khi cần;
* ordered trong cùng một entity;
* không chứa dữ liệu người dùng;
* không chứa OCR/Translation content;
* không chứa secret hoặc credential;
* correlation được bằng `traceId`, `spanId`, `correlationId`.

---

# 3. Event Naming

Định dạng:

```text
Telemetry<Entity><PastTense>
```

Ví dụ:

```text
TelemetryInitialized
MetricRegistered
CounterIncremented
SpanStarted
SpanCompleted
ExporterDegraded
HealthChanged
```

---

# 4. Event Categories

Telemetry chia thành các nhóm:

```text
Lifecycle
Metrics
Trace
Span
Aggregation
Sampling
Runtime
Resources
Health
Export
Flush
Shutdown
Recovery
Configuration
Diagnostics
```

---

# 5. Lifecycle Events

## Published

```text
TelemetryInitializing
TelemetryInitialized
TelemetryReady
TelemetryStarted
TelemetryRunning
TelemetryDegraded
TelemetryRecovered
TelemetryStopping
TelemetryFlushing
TelemetryTerminated
```

---

## Consumed

```text
ApplicationStarting
ApplicationStopping
RuntimeReady
RuntimeShutdownRequested
ConfigurationReloaded
```

---

# 6. Registry Events

```text
MetricRegistered
MetricRegistrationRejected
MetricFrozen
MetricRemoved
RegistryLocked
RegistryUnlocked
RegistryTerminated
```

---

# 7. Counter Events

```text
CounterCreated
CounterIncremented
CounterOverflowPrevented
CounterFrozen
```

Counter không phát event khi đọc giá trị.

---

# 8. Gauge Events

```text
GaugeCreated
GaugeValueChanged
GaugeReset
GaugeFrozen
```

---

# 9. Histogram Events

```text
HistogramCreated
HistogramObserved
HistogramBucketUpdated
HistogramSnapshotCreated
HistogramFrozen
```

Không publish bucket detail ra ngoài module.

---

# 10. Timer Events

```text
TimerStarted
TimerStopped
TimerCancelled
TimerTimeoutObserved
```

Timer kết thúc sẽ tạo observation cho Histogram.

---

# 11. Trace Events

```text
TraceCreated
TraceActivated
TraceCompleted
TraceCancelled
TraceExportQueued
TraceExported
```

Một Trace chỉ có một `TraceCompleted`.

---

# 12. Span Events

```text
SpanCreated
SpanStarted
SpanAttributeAdded
SpanEventRecorded
SpanStatusChanged
SpanCompleted
SpanFailed
SpanCancelled
SpanTimedOut
```

Sau terminal state không phát thêm event.

---

# 13. Correlation Events

```text
CorrelationCreated
CorrelationAttached
CorrelationDetached
CorrelationClosed
```

Không phát User ID.

---

# 14. Aggregation Events

```text
AggregationStarted
AggregationPaused
AggregationResumed
AggregationCompleted
AggregationFailed
AggregationRecovered
```

Aggregation không sửa dữ liệu nguồn.

---

# 15. Sampling Events

```text
SamplingDecisionCreated
MetricSampled
MetricDropped
TraceSampled
TraceDropped
SamplingPolicyChanged
```

Sampling chỉ ảnh hưởng Telemetry.

---

# 16. Runtime Collector Events

```text
RuntimeCollectionStarted
RuntimeSnapshotCollected
RuntimeCollectionDelayed
RuntimeCollectionFailed
RuntimeCollectionRecovered
RuntimeCollectorStopped
```

---

# 17. Resource Collector Events

```text
ResourceCollectionStarted
CpuUsageCollected
MemoryUsageCollected
GpuUsageCollected
DiskUsageCollected
NetworkUsageCollected
ResourceSnapshotCreated
ResourceCollectorDegraded
ResourceCollectorRecovered
```

Chỉ chứa số liệu tổng hợp.

---

# 18. Health Events

```text
HealthReported
HealthChanged
HealthDegraded
HealthRecovered
HealthUnavailable
HealthSnapshotPublished
```

Health phản ánh trạng thái hạ tầng, không phản ánh nghiệp vụ.

---

# 19. Export Queue Events

```text
ExportQueued
ExportDequeued
ExportBufferFull
ExportBufferRecovered
ExportBatchCreated
ExportBatchDiscarded
```

---

# 20. Metrics Export Events

```text
MetricsExportStarted
MetricsExportSucceeded
MetricsExportPartiallySucceeded
MetricsExportFailed
MetricsExportTimedOut
MetricsExporterDegraded
MetricsExporterRecovered
```

---

# 21. Trace Export Events

```text
TraceExportStarted
TraceExportSucceeded
TraceExportPartiallySucceeded
TraceExportFailed
TraceExportTimedOut
TraceExporterDegraded
TraceExporterRecovered
```

---

# 22. Flush Events

```text
FlushRequested
FlushStarted
FlushCompleted
FlushPartiallyCompleted
FlushTimedOut
FlushCancelled
```

Flush không đảm bảo durability tuyệt đối.

---

# 23. Shutdown Events

```text
ShutdownRequested
ShutdownStarted
ShutdownDrainCompleted
ShutdownFlushCompleted
ShutdownCompleted
ShutdownTimedOut
```

---

# 24. Recovery Events

```text
TelemetryRecoveryStarted
ExporterRecoveryStarted
CollectorRecoveryStarted
RecoverySucceeded
RecoveryFailed
```

Recovery phải bounded.

---

# 25. Configuration Events

```text
TelemetryConfigurationLoaded
TelemetryConfigurationReloaded
TelemetryConfigurationRejected
SamplingConfigurationChanged
ExporterConfigurationChanged
```

---

# 26. Diagnostics Events

```text
DiagnosticsRequested
DiagnosticsSnapshotCreated
DiagnosticsExportRequested
DiagnosticsExportCompleted
DiagnosticsExportRejected
```

Không export dữ liệu nhạy cảm.

---

# 27. Observability Events

Telemetry có thể phát:

```text
ObservabilityPipelineDegraded
ObservabilityPipelineRecovered
ObservabilityCapacityWarning
ObservabilityBackpressureDetected
```

Đây là tín hiệu cho Runtime hoặc Logging.

---

# 28. Integration Events

Telemetry **consume** các sự kiện từ:

```text
Runtime
Configuration
Logging
Event Bus
Secret Management
Storage
```

Telemetry **publish** sự kiện cho:

```text
Runtime
Logging
Diagnostics
Monitoring
```

Không publish business event.

---

# 29. Event Ordering

Trong cùng một entity:

```text
MetricRegistered
    ↓
CounterIncremented
    ↓
MetricFrozen
```

```text
SpanCreated
    ↓
SpanStarted
    ↓
SpanCompleted
```

Thứ tự giữa các entity độc lập không được đảm bảo.

---

# 30. Event Correlation

Mọi event có thể mang:

```text
telemetryId
traceId
spanId
correlationId
timestamp
module
```

Không mang:

```text
userContent
translatedText
ocrResult
imageData
secret
token
password
```

---

# 31. Event Invariants

* Event không thay đổi sau khi publish.
* Không publish duplicate terminal event.
* Không publish sau `TelemetryTerminated`.
* Không tạo vòng lặp giữa Telemetry và Logging.
* Không phát event chỉ vì exporter đọc dữ liệu.

---

# 32. Failure Semantics

Ví dụ:

```text
MetricsExportStarted
        ↓
MetricsExportFailed
        ↓
ExporterRecoveryStarted
        ↓
MetricsExporterRecovered
```

Hoặc:

```text
TraceExportStarted
        ↓
TraceExportTimedOut
```

Timeout không đồng nghĩa export thất bại hoàn toàn.

---

# 33. Security Rules

Event không được chứa:

* OCR text
* Translation text
* Prompt
* API payload
* Authorization Header
* Cookie
* Password
* Token
* Secret
* Private Key

Chỉ được chứa metadata an toàn.

---

# 34. Related Documents

```text
MODULE.md
CONTRACT.md
STATES.md
ERRORS.md
README.md
```

---

# 35. Summary

Telemetry Events chuẩn hóa toàn bộ vòng đời của Metrics, Traces, Health, Collectors và Exporters.

Các event chỉ phản ánh **tình trạng quan sát của hệ thống**, không phản ánh dữ liệu nghiệp vụ. Mọi event đều immutable, không chứa thông tin nhạy cảm, hỗ trợ correlation và đảm bảo khả năng mở rộng sang các backend observability như OpenTelemetry hoặc OTLP mà không làm thay đổi contract của các module sử dụng Telemetry.
