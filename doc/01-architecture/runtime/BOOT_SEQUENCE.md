# runtime/BOOT_SEQUENCE.md

# Runtime Boot Sequence

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa startup lifecycle của CRAI Application và thứ tự khởi tạo các Runtime Component.

Boot Sequence phải bảo đảm:

- dependency được thỏa mãn trước activation;
- component critical fail theo cách có kiểm soát;
- component optional có thể degrade;
- không nhận user/runtime command trước khi authority path sẵn sàng;
- không tạo WorkItem trước Scheduler, Queue và Runtime Control;
- không cấp Lease trước Resource Manager;
- không publish Artifact trước Artifact Store;
- partial initialization luôn có rollback/cleanup path;
- Application chỉ phát `APPLICATION_READY` khi Runtime đạt operational state.

---

## 2. Design Principles

1. Deterministic startup.
2. Dependency-aware initialization.
3. Fail-fast cho critical component.
4. Degraded mode cho optional component.
5. Parallel initialization chỉ khi dependency cho phép.
6. Observability càng sớm càng tốt.
7. No user processing before Runtime authority is ready.
8. No publication before ownership infrastructure is ready.
9. No active Reading Session is created during boot.
10. Rollback theo reverse dependency order.
11. Startup state observable.
12. Secrets không xuất hiện trong startup events/logs.
13. Storage failure không được silently ignored.
14. Safe Mode phải có đường vào rõ ràng.
15. Boot không phụ thuộc provider warmup thành công nếu provider là optional.

---

## 3. Boot State Machine

```text
PROCESS_STARTING
    ↓
BOOTSTRAP_LOADING
    ↓
CONFIGURING
    ↓
INFRASTRUCTURE_INITIALIZING
    ↓
RUNTIME_INITIALIZING
    ↓
UI_INITIALIZING
    ↓
READY
```

Failure states:

```text
DEGRADED
SAFE_MODE
BOOT_FAILED
SHUTTING_DOWN
```

`READY` chỉ đạt khi Runtime Control, Scheduler, Queue, Artifact Store và UI boundary đã operational.

---

## 4. High-Level Boot Flow

```text
Process Start
    ↓
Load Bootstrap Configuration
    ↓
Initialize Early Diagnostics
    ↓
Initialize Configuration Service
    ↓
Load / Merge / Validate Configuration
    ↓
Resolve Secret References
    ↓
Initialize Storage
    ↓
Create Runtime Container
    ↓
Initialize Event Bus
    ↓
Initialize Resource Manager
    ↓
Initialize Artifact Store
    ↓
Initialize Provider Manager
    ↓
Initialize Scheduler
    ↓
Initialize Work Queue and Execution Pools
    ↓
Initialize Runtime Control
    ↓
Initialize Session Manager
    ↓
Initialize Presentation Runtime
    ↓
Initialize UI Adapter
    ↓
Run Readiness Validation
    ↓
Publish APPLICATION_READY
```

---

## 5. Boot Dependency Graph

```text
Bootstrap Configuration
        ↓
Early Diagnostics
        ↓
Configuration Service
        ↓
Validated Configuration Snapshot
        ├── Storage
        ├── Secret Resolver
        └── Runtime Container
                ↓
            Event Bus
                ↓
        ┌───────┴────────┐
        ↓                ↓
Resource Manager     Provider Manager
        ↓                ↓
Artifact Store       Provider Registry
        └───────┬────────┘
                ↓
            Scheduler
                ↓
        Work Queue / Execution Pools
                ↓
          Runtime Control
                ↓
          Session Manager
                ↓
       Presentation Runtime
                ↓
            UI Adapter
```

---

## 6. Stage 1 — Process Start

Responsibilities:

- create `ApplicationInstanceId`;
- establish process-level cancellation/shutdown signal;
- capture startup timestamp;
- determine executable/runtime environment;
- prepare emergency stderr/fallback diagnostics.

No Runtime Component is active yet.

---

## 7. Stage 2 — Load Bootstrap Configuration

Bootstrap loader reads only values required before main config infrastructure:

```text
environment
profile
dataDirectory
configFile
secretStoreBackend
recoveryMode
startupLoggingDestination
safeModeFlag
```

Validation:

- syntax;
- required fields;
- supported profile;
- valid data directory policy;
- supported secret store type.

Failure:

- enter minimal boot diagnostics;
- preserve source configuration;
- start Safe Mode when supported;
- otherwise terminate safely.

---

## 8. Stage 3 — Initialize Early Diagnostics

Early diagnostics includes:

- minimal structured logging;
- startup trace;
- bounded startup event buffer;
- crash/failure marker;
- redaction rules;
- emergency sink fallback.

Early diagnostics must start before normal component initialization so later failures are observable.

It does not require full observability infrastructure yet.

---

## 9. Stage 4 — Initialize Configuration Service

Configuration Service initializes:

- parser;
- profile loader;
- persisted-config loader;
- environment/CLI override adapters;
- schema registry;
- migration registry;
- redaction;
- snapshot builder.

Then it performs:

```text
Load Defaults
    ↓
Load Runtime Profile
    ↓
Load Persisted Configuration
    ↓
Apply Overrides
    ↓
Migrate if Needed
    ↓
Schema Validation
    ↓
Semantic Validation
    ↓
Capability Prevalidation
    ↓
Create Immutable Configuration Snapshot
```

No normal Runtime Component starts before snapshot validation succeeds.

---

## 10. Stage 5 — Resolve Secret References

Secret Resolver initializes after bootstrap/config structure is known.

Responsibilities:

- open configured secure store;
- validate required credential references;
- avoid resolving optional secret eagerly unless needed;
- expose safe credential metadata;
- never log secret value.

Failure behavior:

- critical bootstrap secret failure → Safe Mode or boot failure;
- optional provider credential missing → provider disabled/degraded;
- unrelated local capability continues.

---

## 11. Stage 6 — Initialize Storage

Storage initialization includes:

- open configured backend;
- validate backend availability;
- validate storage schema;
- run supported migrations;
- load required configuration/preferences records;
- load safe recovery metadata;
- expose durable persistence capability.

Criticality depends on required use case:

- configuration persistence backend failure → critical;
- optional history unavailable → degraded;
- optional durable cache unavailable → degraded;
- corrupt state requiring recovery → Safe Mode possible.

Storage does not initialize Runtime Artifact Store.

---

## 12. Stage 7 — Create Runtime Container

Runtime Container registers:

- validated configuration snapshot;
- typed configuration views;
- Event Bus contract;
- Storage contract;
- Secret Resolver contract;
- Observability contract;
- clock/time provider;
- lifecycle registry;
- component factories.

Container creation must follow `MODULE_DEPENDENCY.md`.

No hidden service-locator access should be introduced for convenience.

---

## 13. Stage 8 — Initialize Event Bus

Event Bus initializes:

- event registry;
- subscribers;
- dispatch policy;
- bounded internal buffers;
- failure isolation;
- event-version validation.

Event dispatch may remain paused until required subscribers are registered.

Event Bus must not depend on remote telemetry.

---

## 14. Stage 9 — Initialize Resource Manager

Resource Manager initializes before Artifact Store and Worker execution.

Responsibilities:

- resource registry;
- Lease coordination;
- retention tracking;
- logical disposal coordination;
- physical cleanup coordination;
- pressure state;
- leak diagnostics;
- native/GPU cleanup adapters.

No shared Resource Lease may be issued before Resource Manager is ready.

---

## 15. Stage 10 — Initialize Artifact Store

Artifact Store initializes:

- Artifact registry;
- Candidate registration;
- ownership-transfer mechanism;
- atomic publication mechanism;
- Artifact lookup;
- Lease integration;
- cache-retention integration;
- disposal coordination.

Runtime memory cache may be initialized as part of Artifact Store integration.

Durable cache remains behind Storage.

---

## 16. Stage 11 — Initialize Provider Manager

Provider Manager initializes:

```text
Load Provider Registry
    ↓
Validate Provider Config
    ↓
Validate Capability Declarations
    ↓
Create Provider Adapters
    ↓
Run Health Checks
    ↓
Optional Warmup
    ↓
Publish Provider Availability
```

Provider initialization is capability-based, not hard-coded to OCR or Translation.

Provider states may become:

```text
HEALTHY
DEGRADED
UNAVAILABLE
PROBING
```

Optional provider failure must not abort unrelated Runtime capability.

---

## 17. Stage 12 — Initialize Scheduler

Scheduler initializes:

- admission policies;
- current-Revision preference;
- workload classes;
- provider/resource capacity views;
- retry admission rules;
- replacement/obsolete-work rules;
- pressure-aware decisions;
- control-capacity reservation.

Scheduler begins in:

```text
ADMISSION_CLOSED
```

It does not admit WorkItem until Runtime Control is ready.

---

## 18. Stage 13 — Initialize Work Queue and Execution Pools

Initialize:

- logical queue registry;
- bounded queue classes;
- dispatchers;
- CPU Execution Pool;
- optional GPU/native serial contexts;
- Provider I/O capacity bindings;
- maintenance execution context.

Queues start empty.

Dispatch remains paused until Runtime Control activation.

Work Queue does not own payload.

---

## 19. Stage 14 — Initialize Runtime Control

Runtime Control initializes:

- serialized command processing;
- authority registry;
- Revision registry;
- WorkItem state ownership;
- Attempt lineage;
- cancellation coordination;
- retry coordination;
- publication coordination;
- shutdown state;
- Scheduler/Queue integration.

Runtime Control is the final authority owner before admission opens.

Readiness checks:

- Resource Manager ready;
- Artifact Store ready;
- Scheduler ready;
- Queue ready;
- Event Bus ready;
- configuration snapshot active.

---

## 20. Stage 15 — Initialize Session Manager

Session Manager initializes:

- Session registry;
- Session factory;
- Session configuration derivation;
- source/capture adapter registry;
- no-active-session initial state.

Boot does not create a Reading Session automatically.

User action or explicit startup policy creates Session later.

---

## 21. Stage 16 — Initialize Presentation Runtime

Presentation Runtime initializes:

- presentation model factory;
- UI commit adapter;
- current/previous Presentation retention;
- UI authority revalidation;
- localization resources;
- typography/layout services;
- presentation diagnostics.

No presentation commit occurs before UI Adapter is ready.

---

## 22. Stage 17 — Initialize UI Adapter

UI Adapter initializes:

- application shell;
- Settings and Diagnostics views;
- theme/localization;
- Session controls;
- Presentation binding;
- Runtime command bridge;
- startup/degraded/safe-mode UI.

UI may appear before full readiness, but processing interaction remains disabled until readiness validation passes.

---

## 23. Stage 18 — Readiness Validation

Readiness Validator checks:

```text
Configuration Snapshot Active
Event Bus Ready
Resource Manager Ready
Artifact Store Ready
Scheduler Ready
Work Queue Ready
Runtime Control Ready
Session Manager Ready
Presentation Runtime Ready
UI Adapter Ready
Critical Storage Capability Ready
```

Optional degraded components are listed separately.

If checks pass:

```text
Open Scheduler Admission
    ↓
Enable Runtime Commands
    ↓
Enable User Processing Actions
    ↓
Publish APPLICATION_READY
```

---

## 24. Application Ready

`APPLICATION_READY` means:

- Runtime may accept user commands;
- Session may be created;
- WorkItem may be admitted;
- authority checks operational;
- Candidate publication safe;
- Resource Lease operational;
- UI commit path ready;
- critical diagnostics active.

It does not mean every optional provider is healthy.

---

## 25. Parallel Initialization

Safe parallelism is allowed only after dependencies are satisfied.

Possible parallel groups:

### Group A

After validated config:

- Storage open;
- Secret Resolver init;
- UI static assets preparation.

### Group B

After Runtime Container/Event Bus:

- Provider adapter construction;
- Resource Manager internal subcomponents;
- Presentation static services.

### Group C

After Resource Manager:

- Artifact Store;
- optional provider warmup;
- Execution Pool construction.

Never parallelize operations with unresolved ownership/dependency order.

---

## 26. Critical vs Optional Components

### Critical

Typical:

- Bootstrap Configuration;
- Configuration Service;
- Runtime Container;
- Event Bus;
- Resource Manager;
- Artifact Store;
- Scheduler;
- Work Queue;
- Runtime Control;
- UI Adapter;
- required Storage capability.

Failure result:

```text
Safe Mode or BOOT_FAILED
```

### Optional

Typical:

- optional provider;
- durable cache;
- history;
- remote telemetry exporter;
- experimental component;
- GPU acceleration.

Failure result:

```text
DEGRADED
```

---

## 27. Degraded Boot

Application may reach Ready in degraded mode when:

- local capability remains usable;
- critical authority/publication/resource path works;
- missing component is optional;
- UI clearly reports limitation;
- retry loop is not started indefinitely;
- diagnostics preserve cause.

Example:

```text
Remote Translator unavailable
    ↓
Local reading/capture/settings still operational
    ↓
APPLICATION_READY with provider degraded state
```

---

## 28. Safe Mode Boot

Safe Mode uses:

- built-in defaults;
- remote providers disabled;
- experimental features disabled;
- conservative resource limits;
- automatic capture disabled;
- durable cache/history disabled;
- diagnostics and configuration recovery enabled;
- authority/publication invariants still enforced.

Safe Mode is not a partially validated normal mode.

---

## 29. Boot Failure Handling

On failure:

```text
Stop Admission if Open
    ↓
Revoke Boot/Runtime Authority
    ↓
Cancel Pending Initialization
    ↓
Drain Started Execution Contexts
    ↓
Release Leases
    ↓
Dispose Initialized Components in Reverse Order
    ↓
Flush Bounded Diagnostics
    ↓
Persist Safe Failure Marker
    ↓
Enter Safe Mode or Exit
```

No partially initialized component should remain active.

---

## 30. Reverse Cleanup Order

Typical rollback order:

```text
UI Adapter
    ↓
Presentation Runtime
    ↓
Session Manager
    ↓
Runtime Control
    ↓
Work Queue / Execution Pools
    ↓
Scheduler
    ↓
Provider Manager
    ↓
Artifact Store
    ↓
Resource Manager
    ↓
Event Bus
    ↓
Runtime Container
    ↓
Storage
    ↓
Configuration Service
    ↓
Diagnostics Flush
```

Actual order follows initialized dependency graph.

---

## 31. Startup Events

Conceptual events:

```text
BOOT_STARTED
BOOTSTRAP_CONFIG_READY
EARLY_DIAGNOSTICS_READY
CONFIGURATION_READY
SECRET_RESOLVER_READY
STORAGE_READY
RUNTIME_CONTAINER_READY
EVENT_BUS_READY
RESOURCE_MANAGER_READY
ARTIFACT_STORE_READY
PROVIDER_MANAGER_READY
SCHEDULER_READY
WORK_QUEUE_READY
RUNTIME_CONTROL_READY
SESSION_MANAGER_READY
PRESENTATION_RUNTIME_READY
UI_READY
APPLICATION_DEGRADED
APPLICATION_READY
BOOT_FAILED
SAFE_MODE_ENTERED
```

Final event names follow Event Standard.

---

## 32. Startup Event Rules

Events must:

- be content-free;
- exclude secret values;
- include `ApplicationInstanceId`;
- include startup phase;
- include duration;
- include failure code when relevant;
- not block boot;
- not be required for correctness.

---

## 33. Startup Metrics

Track:

```text
boot.total_ms
boot.bootstrap_config_ms
boot.diagnostics_ms
boot.configuration_ms
boot.secret_resolver_ms
boot.storage_ms
boot.container_ms
boot.event_bus_ms
boot.resource_manager_ms
boot.artifact_store_ms
boot.provider_manager_ms
boot.scheduler_ms
boot.work_queue_ms
boot.runtime_control_ms
boot.presentation_ms
boot.ui_ms
boot.rollback_ms
boot.safe_mode_total
boot.failure_total
boot.degraded_total
```

Cold startup must be distinguished from provider/model warmup.

---

## 34. Startup Diagnostics Snapshot

Snapshot may include:

```text
ApplicationInstanceId
BootState
CurrentStage
CompletedStages
PendingStages
FailedStage
FailureCode
DegradedComponents
ActiveConfigurationSnapshotId
InitializedComponents
OpenResources
ActiveLeases
ElapsedTime
```

No payload or secret.

---

## 35. Startup Health Checks

Provider/resource health checks must be:

- bounded;
- timeout-controlled;
- optional where possible;
- non-destructive;
- privacy-safe;
- not dependent on real reading content.

Remote health check must not send captured content.

---

## 36. Boot and Configuration Activation

Startup activation differs runtime config update:

```text
Boot
    → activate one fully validated initial snapshot

Runtime Change
    → impact analysis + safe activation boundary
```

Initial boot must not expose partially merged config.

---

## 37. Boot and Storage Recovery

Storage recovery may:

- restore valid configuration backup;
- restore recovery marker;
- discard corrupt optional cache;
- enter Safe Mode;
- preserve corrupt data for diagnostics.

Storage recovery must not guess business data meaning.

---

## 38. Boot and Observability

Early diagnostics starts before full observability.

After full Observability initialization:

```text
Early Startup Buffer
    ↓
Sanitize
    ↓
Import into Runtime Observability
```

Import is best-effort.

---

## 39. Boot and Shutdown

If shutdown is requested during boot:

```text
Boot Cancellation Requested
    ↓
No New Stage Started
    ↓
Current Initialization Canceled if Safe
    ↓
Partial Components Rolled Back
    ↓
Diagnostics Flushed
    ↓
Process Exit
```

Application must not continue to `APPLICATION_READY`.

---

## 40. Startup Invariants

1. Boot order dependency-aware.
2. Every initialized component has cleanup path.
3. Diagnostics available before critical initialization.
4. Active config fully validated.
5. Secret value never appears in config snapshot.
6. Storage and Artifact Store are distinct.
7. Resource Manager starts before shared Lease.
8. Artifact Store starts before publication.
9. Scheduler starts before WorkItem admission.
10. Queue starts before dispatch.
11. Runtime Control starts before authority exists.
12. Runtime Control starts before admission opens.
13. Boot creates no active Reading Session.
14. UI processing actions disabled before ready.
15. Provider type not hard-coded into boot architecture.
16. Optional provider failure may degrade, not necessarily abort.
17. Critical component failure prevents Ready.
18. Rollback occurs in reverse dependency order.
19. Boot events do not control boot correctness.
20. Event Bus does not depend on telemetry exporter.
21. Admission opens only after readiness validation.
22. No Candidate publication before Artifact Store ready.
23. No Resource Lease before Resource Manager ready.
24. No WorkItem before Runtime Control ready.
25. Safe Mode retains authority/publication safety.
26. Boot shutdown cancels remaining stages.
27. Partial startup leaves no untracked resource.
28. Startup metrics contain no reading content.
29. ApplicationReady emitted once.
30. BootFailed and ApplicationReady are mutually exclusive.

---

## 41. Testing Requirements

Test:

- normal deterministic boot;
- optional provider degraded;
- missing credential;
- corrupt persisted configuration;
- unsupported schema;
- Storage unavailable;
- Storage migration failure;
- Resource Manager init failure;
- Artifact Store init failure;
- Scheduler init failure;
- Runtime Control init failure;
- UI init failure;
- Safe Mode entry;
- shutdown during boot;
- rollback order;
- parallel initialization dependency safety;
- no Session created on boot;
- admission closed until ready;
- no WorkItem before Runtime Control;
- no Lease before Resource Manager;
- startup event privacy;
- duplicate ready prevention;
- telemetry failure during boot.

---

## 42. MVP Boot Policy

MVP uses:

- one process;
- one Runtime Container;
- local Configuration Service;
- local Storage;
- local Event Bus;
- process-local Resource Manager;
- process-local Artifact Store;
- bounded execution pools;
- optional remote provider adapters;
- one Runtime Control context;
- one UI Adapter;
- no automatic Reading Session;
- no cloud boot dependency;
- no runtime plugin loading.

---

## 43. Open Questions

- UI shell có hiển thị trước full Ready không?
- Storage nào critical trong MVP?
- Provider warmup eager hay lazy?
- Local model load có nằm trên critical boot path không?
- Safe Mode tự động sau bao nhiêu lần failure?
- Early diagnostics được giữ bao lâu?
- Runtime Container implementation cụ thể?
- Readiness health check có timeout bao nhiêu?
- Provider registry có lazy instantiate không?
- Presentation assets có thể load song song đến mức nào?
- Boot recovery marker nằm ở Storage use case nào?

---

## 44. Related Documents

| Document | Relationship |
|---|---|
| `README.md` | Runtime overview |
| `RUNTIME_COMPONENTS.md` | Component dependency and ownership |
| `RUNTIME_CONFIG.md` | Bootstrap and active configuration |
| `PIPELINE_RUNTIME.md` | Runtime Control and authority |
| `SCHEDULER.md` | Admission opening |
| `WORK_QUEUE.md` | Queue/dispatcher initialization |
| `CANCELLATION.md` | Boot cancellation and shutdown |
| `MEMORY_MODEL.md` | Resource budgets |
| `RESOURCE_LIFECYCLE.md` | Rollback cleanup |
| `THREADING_MODEL.md` | Execution context lifecycle |
| `RUNTIME_OBSERVABILITY.md` | Startup metrics/events |
| `ERROR_MODEL.md` | Boot failure normalization |
| `../../modules/storage/README.md` | Storage startup/recovery |
| `../core/EVENT_BUS.md` | Startup event semantics |
| `../MODULE_DEPENDENCY.md` | Dependency graph |

---

## 45. Completion Criteria

`BOOT_SEQUENCE.md` được xem là đồng bộ khi:

- boot dùng Runtime v2 component graph;
- Configuration Service và immutable snapshot rõ;
- Resource Manager và Artifact Store có stage riêng;
- Provider Manager capability-based;
- Scheduler/Queue/Runtime Control order đúng;
- boot không tạo active Session;
- admission chỉ mở sau readiness;
- degraded/safe mode tách rõ;
- rollback reverse dependency;
- startup events/metrics/privacy đầy đủ;
- không hard-code OCR/Translation provider boot.

---

## 46. Summary

CRAI Boot Sequence:

```text
Bootstrap
    ↓
Observe
    ↓
Configure
    ↓
Persist/Recover
    ↓
Build Infrastructure
    ↓
Build Resource and Artifact Ownership
    ↓
Build Provider and Execution Infrastructure
    ↓
Build Runtime Authority
    ↓
Build Presentation/UI
    ↓
Validate Readiness
    ↓
Open Admission
```

Điểm chốt:

```text
No authority before Runtime Control.

No publication before Artifact Store.

No Lease before Resource Manager.

No WorkItem before Scheduler and Queue.

No user processing before APPLICATION_READY.
```
