# Telemetry States

> **Project:** CRAI
> **Layer:** Infrastructure
> **Module:** Telemetry
> **Document:** State Machines
> **Path:** `03-infrastructure/telemetry/STATES.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-06

---

# 1. Purpose

Tài liệu này định nghĩa toàn bộ **state machine** của module Telemetry.

State machine đảm bảo:

* lifecycle nhất quán;
* không có trạng thái mơ hồ;
* mọi transition đều có điều kiện rõ ràng;
* shutdown có giới hạn thời gian;
* exporter không làm block producer;
* metrics và traces luôn immutable sau khi hoàn thành.

---

# 2. Design Principles

## 2.1 Explicit State

Mọi component đều có state rõ ràng.

Ví dụ:

```text
INITIALIZING
READY
RUNNING
DEGRADED
STOPPING
TERMINATED
```

Không sử dụng implicit state.

---

## 2.2 Single Active State

Một đối tượng chỉ được ở **một state** tại một thời điểm.

Ví dụ:

```text
RUNNING
```

không thể đồng thời là:

```text
STOPPING
```

---

## 2.3 Terminal State

Terminal state không được transition trở lại.

Ví dụ:

```text
TERMINATED
```

không được quay về:

```text
RUNNING
```

---

## 2.4 Immutable Completion

Sau khi hoàn thành:

* Metric không đổi.
* Span không đổi.
* Trace không đổi.
* Export Session không đổi.

---

# 3. Telemetry Lifecycle

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
 ┌──────┴─────────┐
 ▼                ▼
DEGRADED      STOPPING
 │                │
 ▼                ▼
RUNNING      FLUSHING
                  │
                  ▼
            TERMINATED
```

---

## State Meaning

### UNINITIALIZED

Module chưa được tạo.

---

### INITIALIZING

Khởi tạo:

* registry
* exporters
* collectors
* sampler
* aggregation

---

### READY

Đã sẵn sàng.

Chưa bắt đầu thu telemetry.

---

### RUNNING

Hoạt động bình thường.

---

### DEGRADED

Một hoặc nhiều exporter/collector lỗi.

Producer vẫn tiếp tục publish.

---

### STOPPING

Không nhận telemetry mới.

---

### FLUSHING

Đẩy dữ liệu còn lại.

---

### TERMINATED

Đã dừng hoàn toàn.

---

# 4. Metric State

```text
CREATED
    │
    ▼
REGISTERED
    │
    ▼
ACTIVE
    │
    ▼
FROZEN
```

---

### CREATED

Metric mới tạo.

---

### REGISTERED

Đăng ký vào Registry.

---

### ACTIVE

Được cập nhật.

---

### FROZEN

Không còn ghi.

Terminal.

---

# 5. Counter State

```text
REGISTERED
      │
      ▼
ACTIVE
      │
      ▼
FROZEN
```

Counter chỉ tăng.

---

# 6. Gauge State

```text
REGISTERED
      │
      ▼
ACTIVE
      │
      ▼
FROZEN
```

Gauge cho phép tăng giảm.

---

# 7. Histogram State

```text
REGISTERED
      │
      ▼
COLLECTING
      │
      ▼
FROZEN
```

Histogram không sửa bucket sau FROZEN.

---

# 8. Timer State

```text
CREATED
    │
    ▼
RUNNING
    │
    ▼
STOPPED
```

STOPPED là terminal.

---

# 9. Trace State

```text
CREATED
    │
    ▼
ACTIVE
    │
    ▼
COMPLETED
```

Không được quay lại ACTIVE.

---

# 10. Span State

```text
CREATED
    │
    ▼
RUNNING
    │
    ▼
SUCCESS
FAILED
TIMEOUT
CANCELLED
```

Bốn state cuối là terminal.

---

# 11. Span Hierarchy State

```text
ROOT_CREATED
       │
       ▼
CHILDREN_RUNNING
       │
       ▼
ALL_COMPLETED
```

Root không hoàn thành khi còn Child đang RUNNING.

---

# 12. Trace Aggregation State

```text
WAITING
    │
    ▼
COLLECTING
    │
    ▼
READY_TO_EXPORT
    │
    ▼
EXPORTED
```

---

# 13. Metric Registry State

```text
INITIALIZING
      │
      ▼
READY
      │
      ▼
LOCKED
      │
      ▼
TERMINATED
```

LOCKED khi shutdown.

---

# 14. Aggregation Engine

```text
INITIALIZING
      │
      ▼
RUNNING
      │
 ┌────┴────┐
 ▼         ▼
PAUSED   DEGRADED
 │         │
 └────┬────┘
      ▼
STOPPING
      │
      ▼
TERMINATED
```

---

# 15. Sampler

```text
INITIALIZING
      │
      ▼
ACTIVE
      │
      ▼
DISABLED
```

DISABLED vẫn cho phép publish.

---

# 16. Runtime Collector

```text
INITIALIZING
      │
      ▼
COLLECTING
      │
      ▼
PAUSED
      │
      ▼
STOPPED
```

---

# 17. Resource Collector

```text
STARTING
    │
    ▼
RUNNING
    │
 ┌──┴──┐
 ▼     ▼
SLOW DEGRADED
 │     │
 └──┬──┘
    ▼
STOPPED
```

---

# 18. Health Reporter

```text
INITIALIZING
      │
      ▼
HEALTHY
      │
 ┌────┴────┐
 ▼         ▼
DEGRADED UNAVAILABLE
 │         │
 └────┬────┘
      ▼
STOPPED
```

---

# 19. Export Queue

```text
EMPTY
  │
  ▼
BUFFERING
  │
  ▼
READY
  │
  ▼
EXPORTING
```

Có thể quay lại BUFFERING.

---

# 20. Export Session

```text
CREATED
    │
    ▼
RUNNING
    │
 ┌──┼────────────┐
 ▼  ▼            ▼
SUCCESS FAILED TIMEOUT
```

Ba state cuối là terminal.

---

# 21. Metrics Exporter

```text
INITIALIZING
      │
      ▼
READY
      │
      ▼
EXPORTING
      │
 ┌────┴─────┐
 ▼          ▼
READY    DEGRADED
             │
             ▼
        TERMINATED
```

---

# 22. Trace Exporter

```text
INITIALIZING
      │
      ▼
READY
      │
      ▼
EXPORTING
      │
 ┌────┴─────┐
 ▼          ▼
READY    DEGRADED
             │
             ▼
        TERMINATED
```

---

# 23. Flush Operation

```text
CREATED
    │
    ▼
RUNNING
    │
 ┌──┼─────────────┐
 ▼  ▼             ▼
SUCCESS PARTIAL TIMEOUT
```

Terminal:

```text
SUCCESS
PARTIAL
TIMEOUT
```

---

# 24. Shutdown State

```text
RUNNING
    │
    ▼
STOPPING
    │
    ▼
FLUSHING
    │
    ▼
TERMINATED
```

Shutdown phải bounded.

---

# 25. Correlation Context

```text
CREATED
    │
    ▼
ACTIVE
    │
    ▼
CLOSED
```

Không reopen.

---

# 26. Sampling Decision

```text
UNKNOWN
   │
   ▼
SAMPLED
DROPPED
```

Hai state cuối là terminal.

---

# 27. Health Snapshot

```text
COLLECTING
      │
      ▼
READY
      │
      ▼
EXPORTED
```

---

# 28. Runtime Snapshot

```text
CREATED
    │
    ▼
POPULATED
    │
    ▼
FROZEN
```

Immutable sau FROZEN.

---

# 29. Export Buffer

```text
EMPTY
 │
 ▼
BUFFERING
 │
 ▼
FULL
 │
 ▼
DRAINING
 │
 ▼
EMPTY
```

---

# 30. Failure Recovery

Nếu exporter lỗi:

```text
READY
   │
   ▼
DEGRADED
   │
   ▼
READY
```

Không cần restart toàn module.

---

# 31. Invariants

## Telemetry

* chỉ một lifecycle state.

## Trace

* một Root Span.

## Span

* end đúng một lần.

## Metric

* immutable sau Freeze.

## Export

* session terminal không reopen.

## Shutdown

* không publish sau TERMINATED.

---

# 32. Illegal Transitions

Không hợp lệ:

```text
TERMINATED
    │
    ▼
RUNNING
```

```text
SUCCESS
    │
    ▼
RUNNING
```

```text
COMPLETED
    │
    ▼
ACTIVE
```

```text
FROZEN
    │
    ▼
ACTIVE
```

---

# 33. State Ownership

Telemetry sở hữu state của:

* Metrics
* Counters
* Gauges
* Histograms
* Timers
* Traces
* Spans
* Registry
* Aggregation
* Sampling
* Health
* Export Queue
* Exporter
* Runtime Collector
* Resource Collector

Không sở hữu state của:

* Logging
* Event Bus
* OCR
* Translation
* Storage

---

# 34. Related Documents

```text
MODULE.md
CONTRACT.md
EVENTS.md
ERRORS.md
README.md
```

---

# 35. Summary

State machine của Telemetry đảm bảo:

* lifecycle rõ ràng;
* metrics, traces và spans có trạng thái bất biến sau khi hoàn thành;
* exporter và collector có khả năng suy giảm độc lập;
* shutdown luôn có giới hạn;
* producer không bị block bởi exporter;
* toàn bộ observability pipeline hoạt động theo mô hình deterministic, bounded và fail-safe.
