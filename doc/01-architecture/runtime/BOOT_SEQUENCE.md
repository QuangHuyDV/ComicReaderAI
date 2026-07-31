# Boot Sequence

- Document: Runtime / Boot Sequence
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the startup lifecycle of the CRAI application.

A deterministic boot sequence ensures that every runtime component is initialized in the correct order, dependencies are satisfied, failures are handled consistently and the application only becomes available when it reaches a fully operational state.

---

# Design Principles

- Deterministic startup.
- Dependency-aware initialization.
- Fail-fast for critical components.
- Parallel initialization where safe.
- Event-driven notifications.
- Graceful rollback on failure.

---

# Boot Flow

```text
Process Start
      │
      ▼
Load Runtime Configuration
      │
      ▼
Initialize Diagnostics
      │
      ▼
Initialize Storage
      │
      ▼
Load Preferences
      │
      ▼
Initialize Translation / OCR Providers
      │
      ▼
Initialize Event Bus
      │
      ▼
Initialize Scheduler
      │
      ▼
Initialize Work Queue
      │
      ▼
Initialize Reading Session
      │
      ▼
Initialize UI Adapter
      │
      ▼
Publish ApplicationReady
      │
      ▼
Ready
```

---

# Initialization Stages

## 1. Load Runtime Configuration

- Read configuration.
- Validate required values.
- Resolve environment overrides.

Failure:
- Abort startup.

---

## 2. Initialize Diagnostics

- Logging
- Metrics
- Tracing

Diagnostics should start first so subsequent failures are observable.

---

## 3. Initialize Storage

- Open backend.
- Validate schema.
- Run pending migrations.
- Warm essential caches.

Failure:
- Abort startup.

---

## 4. Load Preferences

- Load persisted preferences.
- Apply defaults when missing.
- Validate compatibility.

---

## 5. Initialize Providers

Initialize configured:

- OCR providers
- Translation providers
- AI providers

Unavailable providers are marked as degraded when alternatives exist.

---

## 6. Initialize Event Bus

- Register event handlers.
- Register subscribers.
- Enable event dispatching.

---

## 7. Initialize Scheduler

Start background schedulers for:

- Cleanup
- Retry
- Health checks
- Cache maintenance

---

## 8. Initialize Work Queue

- Create worker pools.
- Allocate queues.
- Start workers.

---

## 9. Initialize Reading Session

Prepare runtime state without starting any reading task.

---

## 10. Initialize UI Adapter

- Build initial ViewModels.
- Load themes.
- Load localization.
- Display startup UI.

---

## 11. Application Ready

Publish:

- ApplicationStarted
- ApplicationReady

Accept user interaction.

---

# Parallel Initialization

These stages may execute concurrently after Storage and Preferences are ready:

- Provider initialization
- Scheduler initialization
- Work Queue initialization

Shared dependencies must remain synchronized.

---

# Failure Handling

Critical failures:

- Configuration
- Storage
- Schema migration

Result:

- Abort startup.
- Publish diagnostic information.
- Exit gracefully.

Recoverable failures:

- Optional providers
- Optional exporters

Result:

- Continue in degraded mode.

---

# Shutdown After Boot Failure

If startup fails after partial initialization:

1. Stop worker threads.
2. Flush diagnostics.
3. Close storage.
4. Release resources.
5. Exit process.

---

# Startup Events

| Event | Description |
|--------|-------------|
| ApplicationStarting | Startup initiated |
| DiagnosticsReady | Diagnostics initialized |
| StorageReady | Storage initialized |
| PreferencesLoaded | Preferences available |
| ProvidersReady | Providers initialized |
| RuntimeReady | Runtime components initialized |
| UIReady | UI initialized |
| ApplicationReady | Application ready for use |

---

# Startup Invariants

1. Storage is initialized before preferences are loaded.
2. Diagnostics starts before every other runtime component.
3. Event Bus starts before runtime event processing.
4. User interaction is disabled until ApplicationReady.
5. Boot order is deterministic.
6. Critical failures abort startup safely.
7. Partial startup is cleaned up before exit.

---

# Related Documents

- README.md
- RUNTIME_COMPONENTS.md
- RUNTIME_CONFIG.md
- PIPELINE_RUNTIME.md
- RESOURCE_LIFECYCLE.md
- WORK_QUEUE.md
- SCHEDULER.md
