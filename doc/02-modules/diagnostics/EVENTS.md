# Diagnostics Module Events

- Module: Diagnostics
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the events consumed and published by the Diagnostics Module.

The Diagnostics Module observes application behavior by collecting logs, metrics, traces and health information without affecting business execution.

---

# Event Principles

## Passive Observation

Events describe observed system activity.

Diagnostics never changes business state.

---

## Immutable Events

Published events are immutable after publication.

---

## Correlation

Whenever possible, events include a CorrelationId to associate related operations across modules.

---

## Structured Payloads

Diagnostic events should use structured, machine-readable formats.

---

# Event Naming Convention

Events use the past-tense convention.

Examples

```text
LogRecorded
MetricUpdated
TraceCompleted
HealthStatusChanged
DiagnosticsExported
```

---

# Consumed Events

## ErrorOccurred

Purpose

Record unexpected application errors.

---

## ReadingSessionChanged

Purpose

Update runtime activity metrics.

---

## TranslationCompleted

Purpose

Record translation performance metrics.

---

## RecognitionCompleted

Purpose

Record OCR execution metrics.

---

## PresentationUpdated

Purpose

Measure rendering performance.

---

## StorageReady

Purpose

Update storage health status.

---

## StorageFailed

Purpose

Record storage failures and health degradation.

---

## ApplicationStarted

Purpose

Initialize runtime diagnostics.

---

## ApplicationShutdownRequested

Purpose

Flush pending diagnostic information.

---

# Published Events

## LogRecorded

Purpose

A structured log entry has been created.

---

## MetricUpdated

Purpose

A metric value has changed.

---

## TraceStarted

Purpose

A trace has begun.

---

## TraceCompleted

Purpose

A trace has completed.

---

## ErrorReported

Purpose

An application error has been recorded.

---

## HealthStatusChanged

Purpose

Application health state has changed.

---

## DiagnosticsExported

Purpose

Diagnostic information has been successfully exported.

---

## DiagnosticsFlushed

Purpose

Pending diagnostic data has been persisted or exported.

---

# Event Ordering

Typical trace lifecycle

```text
TraceStarted
      ↓
MetricUpdated
      ↓
TraceCompleted
```

Typical error reporting

```text
ErrorOccurred
      ↓
ErrorReported
      ↓
LogRecorded
```

---

# Event Ordering Rules

1. TraceStarted precedes TraceCompleted.
2. ErrorReported follows ErrorOccurred.
3. HealthStatusChanged occurs only after evaluation.
4. DiagnosticsExported occurs after export succeeds.
5. Events represent completed observations.

---

# Event Idempotency

Duplicate events should not create duplicate diagnostic records.

Consumers may identify duplicates using:

- EventId
- CorrelationId
- Timestamp

---

# Event Delivery

Recommended guarantees:

- At-least-once delivery
- Ordered within a trace
- Immutable after publication

---

# Failure Handling

If event processing fails:

- Preserve business execution.
- Retry export operations when appropriate.
- Buffer transient failures.
- Never interrupt application workflow.

---

# Architecture Invariants

1. Diagnostics events never modify business state.
2. Events are append-only observations.
3. Correlation IDs are preserved when available.
4. Sensitive information is never included.
5. Collection failures never stop the application.

---

# Future Events

Potential future events include:

- ProfileCaptured
- RetentionCompleted
- ExportRetryScheduled
- AlertTriggered
- CollectorRegistered

---

# Related Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- ERRORS.md
- README.md
