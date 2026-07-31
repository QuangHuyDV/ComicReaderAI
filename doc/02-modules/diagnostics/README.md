# Diagnostics Module

The Diagnostics Module provides observability for the entire CRAI architecture.

It passively collects logs, metrics, traces and health information so developers and users can understand application behavior, diagnose failures and monitor performance without affecting business logic.

---

# Responsibilities

The Diagnostics Module is responsible for:

- Collecting structured logs.
- Recording runtime metrics.
- Tracking traces.
- Monitoring application health.
- Aggregating error information.
- Exporting diagnostic data.
- Supporting profiling and performance analysis.
- Providing diagnostic summaries to other modules.

It is **not** responsible for:

- Business rule execution.
- OCR.
- Translation.
- UI rendering.
- Storage management.
- Reading Session orchestration.

---

# Position in Architecture

```text
        Application Modules
                │
                ▼
          Diagnostics Module
         ┌────────┼─────────┐
         ▼        ▼         ▼
       Logs    Metrics    Traces
         │        │         │
         └────────┼─────────┘
                  ▼
          Health & Exporters
                  │
      ┌───────────┼────────────┐
      ▼           ▼            ▼
   Console     Local File  OpenTelemetry
```

Diagnostics observes all modules but is not part of the business workflow.

---

# Responsibilities by Area

## Logging

Collects structured logs including:

- Timestamp
- Severity
- Component
- Message
- CorrelationId

Sensitive information must never be logged.

---

## Metrics

Records:

- Performance metrics
- Counters
- Gauges
- Latency
- Throughput

Metrics are independent of UI and storage implementations.

---

## Tracing

Tracks operations across modules using correlation identifiers.

Typical lifecycle:

```text
StartTrace
      ↓
Module Operations
      ↓
FinishTrace
```

---

## Health Monitoring

Provides health information such as:

- Healthy
- Degraded
- Unhealthy
- Unknown

Health evaluation never changes application behavior.

---

## Exporting

Supports exporting diagnostic information to different destinations without changing application code.

Potential exporters include:

- Console
- Local Files
- HTTP
- OpenTelemetry

---

# Interaction with Other Modules

Every module may publish diagnostic information.

The UI Adapter displays logs, metrics and health summaries.

Storage may persist diagnostic history.

Business modules remain independent from diagnostics implementations.

---

# Event Model

Typical consumed events:

- ErrorOccurred
- ReadingSessionChanged
- TranslationCompleted
- RecognitionCompleted
- StorageFailed

Typical published events:

- LogRecorded
- MetricUpdated
- TraceCompleted
- HealthStatusChanged
- DiagnosticsExported

---

# State Model

Typical internal states:

- Initializing
- Ready
- Collecting
- Monitoring
- Exporting
- Flushing
- Failed
- Shutdown

These states describe diagnostics lifecycle only.

---

# Error Model

Representative errors include:

- LoggerUnavailable
- MetricCollectionFailed
- TraceStorageFailed
- ExportFailed
- HealthCheckFailed
- InternalDiagnosticsError

Diagnostics failures never represent business failures.

---

# Design Principles

## Passive Observation

Diagnostics never changes business behavior.

---

## Low Overhead

Collection should have minimal runtime impact.

---

## Structured Data

Logs, metrics and traces use structured formats.

---

## Privacy First

Secrets and personal information are excluded from diagnostic output.

---

## Extensibility

Collectors and exporters are replaceable through public contracts.

---

# Related Documents

| Document | Description |
|----------|-------------|
| MODULE.md | Module responsibilities |
| CONTRACT.md | Public contracts |
| EVENTS.md | Event definitions |
| STATES.md | State machine |
| ERRORS.md | Error model |

---

# Summary

The Diagnostics Module provides a backend-independent observability layer for CRAI. It collects logs, metrics, traces and health information while remaining passive, extensible and isolated from business logic, enabling reliable monitoring, troubleshooting and performance analysis across the entire system.
