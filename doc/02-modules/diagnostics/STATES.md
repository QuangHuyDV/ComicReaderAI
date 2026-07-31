# Diagnostics Module States

- Module: Diagnostics
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the internal state machine of the Diagnostics Module.

The Diagnostics Module passively observes the application, collects logs, metrics, traces and health information, and exports diagnostic data without influencing business execution.

---

# State Principles

## Passive Operation

Diagnostics never changes application behavior.

---

## Deterministic Transitions

The same input always produces the same state transition.

---

## Non-Blocking

Diagnostic failures must not block application execution.

---

## Append-Only Collection

Collected diagnostic data is appended until retention or export policies apply.

---

# State Model

```text
          Initialize
               │
               ▼
         Initializing
               │
               ▼
             Ready
      ┌────────┼──────────┐
      ▼        ▼          ▼
 Collecting Exporting Monitoring
      └────────┼──────────┘
               ▼
            Flushing
               │
               ▼
             Ready
        ┌──────┴──────┐
        ▼             ▼
      Failed       Shutdown
```

---

# State Summary

| State | Description |
|--------|-------------|
| Initializing | Preparing collectors and exporters |
| Ready | Waiting for diagnostic events |
| Collecting | Recording logs, metrics and traces |
| Monitoring | Evaluating health and runtime status |
| Exporting | Sending diagnostic data to exporters |
| Flushing | Persisting buffered diagnostic data |
| Failed | Diagnostics unavailable |
| Shutdown | Diagnostics terminated |

---

# Initializing

Loads collectors, exporters and runtime configuration.

Exit:
- InitializationCompleted
- InitializationFailed

---

# Ready

Diagnostics is operational.

Allowed operations:

- RecordLog
- RecordMetric
- StartTrace
- ReportError
- UpdateHealthStatus

---

# Collecting

Recording diagnostic information.

Invariants:

- Business execution continues uninterrupted.
- Collection is append-only.

---

# Monitoring

Evaluating application health and performance.

Produces health summaries and metrics.

---

# Exporting

Exporting accumulated diagnostic information.

Exit:
- ExportCompleted
- ExportFailed

---

# Flushing

Persist pending buffers before shutdown or on explicit request.

---

# Failed

Diagnostics cannot reliably collect or export data.

Application execution continues.

Allowed operations:

- RetryInitialization
- Shutdown

---

# Shutdown

Diagnostic services have stopped.

No further collection occurs.

---

# State Transition Table

| Current | Event | Next |
|---------|-------|------|
| Initializing | InitializationCompleted | Ready |
| Initializing | InitializationFailed | Failed |
| Ready | DiagnosticEventReceived | Collecting |
| Ready | HealthEvaluationRequested | Monitoring |
| Ready | ExportRequested | Exporting |
| Collecting | CollectionCompleted | Ready |
| Monitoring | EvaluationCompleted | Ready |
| Exporting | ExportCompleted | Ready |
| Exporting | ExportFailed | Failed |
| Ready | FlushRequested | Flushing |
| Flushing | FlushCompleted | Ready |
| Failed | RetryInitialization | Initializing |
| Failed | Shutdown | Shutdown |

---

# Transition Rules

- Collection never modifies business state.
- Monitoring evaluates observations only.
- Exporting is isolated from collectors.
- Flush preserves pending records before shutdown.

---

# Architecture Invariants

1. Diagnostics is passive.
2. Collection failures never terminate the application.
3. Export failures never corrupt collected data.
4. State transitions are deterministic.
5. Sensitive information is never exposed.
6. Shutdown is terminal.
7. All interactions use public contracts.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- EVENTS.md
- ERRORS.md
- README.md
