# Diagnostics Module Contract

- Module: Diagnostics
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the public contract of the Diagnostics Module.

The Diagnostics Module provides a stable interface for collecting, querying and exporting diagnostic information, including logs, metrics, traces, health reports and error summaries, without exposing implementation details.

---

# Public Commands

## RecordLog

Record a structured log entry.

---

## RecordMetric

Record or update a metric.

---

## StartTrace

Start a distributed or local trace.

---

## FinishTrace

Complete an active trace.

---

## ReportError

Record an application error.

---

## UpdateHealthStatus

Update the health status of a component.

---

## ExportDiagnostics

Export logs, metrics and traces using configured exporters.

---

## Flush

Force pending diagnostic data to be persisted or exported.

---

# Public Queries

## GetHealthStatus

Returns current application health.

---

## GetMetrics

Returns available metrics.

---

## GetRecentLogs

Returns recent log entries.

---

## GetTrace

Returns trace information by identifier.

---

## GetErrorSummary

Returns aggregated error information.

---

# Log Contract

Each log entry should include:

- Timestamp
- Severity
- Component
- Message
- CorrelationId (when available)

Sensitive information must be excluded.

---

# Metric Contract

Metrics should be:

- Immutable after publication
- Timestamped
- Aggregatable
- Independent of UI or storage implementation

---

# Health Contract

Health states include:

- Healthy
- Degraded
- Unhealthy
- Unknown

---

# Consumed Events

| Event | Purpose |
|--------|---------|
| ErrorOccurred | Record application failures |
| ReadingSessionChanged | Update runtime metrics |
| StorageFailed | Record storage failures |
| TranslationCompleted | Update translation metrics |
| RecognitionCompleted | Update OCR metrics |

---

# Published Events

| Event | Purpose |
|--------|---------|
| LogRecorded | Structured log created |
| MetricUpdated | Metric changed |
| TraceCompleted | Trace finalized |
| HealthStatusChanged | Health status updated |
| DiagnosticsExported | Export completed |

---

# Export Contract

Supported export targets may include:

- Local Files
- Console
- HTTP Endpoint
- OpenTelemetry
- Future monitoring systems

The public contract remains unchanged regardless of exporter.

---

# Security Contract

Diagnostics must never expose:

- Secrets
- Access tokens
- API keys
- Personal information
- Raw credential data

---

# Error Contract

Operations may return:

- LoggerUnavailable
- MetricCollectionFailed
- TraceStorageFailed
- ExportFailed
- HealthCheckFailed

Detailed definitions are provided in ERRORS.md.

---

# Architecture Invariants

1. Diagnostics never modifies business state.
2. Collection failures never stop application execution.
3. Diagnostic records are append-only unless retention policies apply.
4. Public contracts remain backend independent.
5. Sensitive information is never exposed.
6. Correlation IDs are preserved when available.
7. Export implementations remain replaceable.

---

# Related Documents

- MODULE.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
