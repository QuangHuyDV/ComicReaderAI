# Scheduler Module

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Scheduler  
> **Document:** Module Architecture  
> **Path:** `03-infrastructure/scheduler/MODULE.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/runtime/WORK_QUEUE.md`
> - `docs/architecture/runtime/SCHEDULER.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/configuration/MODULE.md`
> - `03-infrastructure/event-bus/MODULE.md`
> - `03-infrastructure/logging/MODULE.md`
> - `03-infrastructure/telemetry/MODULE.md`

---

## 1. Purpose

The Scheduler module provides CRAI with a shared infrastructure for scheduling, dispatching, and supervising background work.

Although the module is named `scheduler`, its architectural role is broader than cron execution.

It acts as a:

```text
Background Execution Runtime
```

responsible for:

- immediate jobs;
- delayed jobs;
- recurring jobs;
- interval schedules;
- cron schedules;
- retry scheduling;
- timeout supervision;
- cancellation;
- priority;
- concurrency control;
- dependency constraints;
- resource-aware execution;
- graceful shutdown;
- optional restart recovery.

Scheduler coordinates when and under what execution policy a job may run.

It does not own the business meaning of that job.

---

## 2. Module Goal

The module must provide a consistent execution model for background work such as:

- log rotation;
- log retention cleanup;
- telemetry collection;
- telemetry export;
- secret-expiration checks;
- secret rotation checks;
- cache cleanup;
- cache refresh;
- temporary-file cleanup;
- provider health checks;
- provider retry;
- background OCR;
- background translation;
- metadata synchronization;
- update checks;
- plugin discovery;
- periodic maintenance.

The primary optimization target is:

```text
predictable background execution
without blocking interactive reading workflows
```

---

## 3. Architectural Position

```text
Owning Module
    ↓ registers TaskDefinition
Scheduler
    ↓ evaluates Trigger
    ↓ creates JobInstance
    ↓ checks dependencies, capacity, policy, and resources
Execution Queue
    ↓
Dispatcher
    ↓
Worker
    ↓
JobResult
    ↓
Owning Module / Event Bus / Diagnostics
```

Composition Root owns:

- Scheduler construction;
- worker registration;
- persistence-adapter wiring;
- startup order;
- shutdown order.

---

## 4. Terminology

### 4.1 Task Definition

A `TaskDefinition` describes reusable background work.

Example:

```text
LoggingRetentionCleanup
TelemetryResourceCollection
ProviderHealthCheck
```

A task definition is not a specific execution.

### 4.2 Trigger

A `Trigger` defines when a task should produce a job.

Examples:

```text
IMMEDIATE
DELAYED
INTERVAL
CRON
MANUAL
EVENT
```

### 4.3 Job Instance

A `JobInstance` is one scheduled execution of a task.

Example:

```text
TaskDefinition:
    LoggingRetentionCleanup

JobInstance:
    run-2026-08-06T02:00:00Z
```

### 4.4 Job Attempt

A `JobAttempt` is one execution attempt of a job.

Retries create new attempts under the same job identity.

### 4.5 Worker

A `Worker` executes the job-specific callback or handler.

### 4.6 Job Result

A `JobResult` reports:

```text
SUCCEEDED
FAILED
RETRY_REQUESTED
CANCELED
TIMED_OUT
ABANDONED
SKIPPED
```

### 4.7 Schedule

A `Schedule` is a durable or in-memory rule that produces future jobs.

---

## 5. Responsibilities

### 5.1 Task registration

Scheduler registers validated task definitions containing:

- task identity;
- owner module;
- handler identity;
- trigger;
- priority;
- timeout;
- retry policy;
- concurrency policy;
- resource requirements;
- persistence mode;
- overlap policy;
- shutdown policy.

### 5.2 Trigger evaluation

Scheduler evaluates time-based and externally initiated triggers.

Supported trigger classes:

```text
IMMEDIATE
DELAYED
INTERVAL
CRON
MANUAL
EVENT
```

Future:

```text
CONDITIONAL
IDLE_TIME
NETWORK_AVAILABLE
RESOURCE_THRESHOLD
```

### 5.3 Job creation

When a trigger fires, Scheduler creates a `JobInstance`.

Job creation must be deterministic and duplicate-aware.

### 5.4 Queue admission

Scheduler admits eligible jobs into bounded execution queues.

Admission considers:

- priority;
- capacity;
- deadline;
- owner;
- resource class;
- concurrency group;
- shutdown state.

### 5.5 Dispatch

Scheduler dispatches ready jobs to registered workers.

### 5.6 Retry scheduling

Scheduler applies a registered retry policy after a retryable failure.

It owns retry timing.

The worker owns failure classification input.

### 5.7 Timeout supervision

Scheduler owns logical execution deadlines.

It may request cancellation when timeout occurs.

It must distinguish:

```text
TIMED_OUT
CANCELED
ABANDONED
FAILED
```

### 5.8 Cancellation

Scheduler propagates cooperative cancellation to running attempts.

Cancellation may originate from:

- user action;
- owning module;
- shutdown;
- superseded work;
- dependency failure;
- timeout;
- resource policy.

### 5.9 Priority scheduling

Scheduler supports priority classes such as:

```text
CRITICAL
HIGH
NORMAL
LOW
BACKGROUND
MAINTENANCE
```

### 5.10 Concurrency control

Scheduler limits:

- global concurrent jobs;
- concurrent jobs per task;
- concurrent jobs per owner module;
- concurrent jobs per resource class;
- concurrent jobs per key.

### 5.11 Dependency constraints

Scheduler may delay a job until declared prerequisites are satisfied.

Dependencies must be explicit.

Scheduler must not infer business workflow dependencies from event names.

### 5.12 Resource-aware scheduling

Scheduler may consider resource classes:

```text
CPU
GPU
DISK_IO
NETWORK
MEMORY
UI_SENSITIVE
PROVIDER_QUOTA
```

### 5.13 Overlap control

Recurring jobs require an overlap policy:

```text
ALLOW
SKIP_NEW
QUEUE_ONE
QUEUE_ALL_BOUNDED
CANCEL_PREVIOUS
REPLACE_PENDING
```

### 5.14 Misfire handling

When a recurring execution was missed, Scheduler applies:

```text
SKIP
RUN_ONCE_NOW
RUN_ALL_BOUNDED
RESCHEDULE_FROM_NOW
RESUME_NEXT_OCCURRENCE
```

### 5.15 Execution tracking

Scheduler tracks:

- task;
- job;
- attempt;
- trigger;
- queue state;
- worker state;
- timing;
- outcome;
- retry;
- cancellation;
- timeout;
- abandonment.

### 5.16 Persistence boundary

Scheduler may optionally persist:

- task definitions;
- schedules;
- pending jobs;
- retry state;
- execution receipts.

Persistence is optional for the MVP.

### 5.17 Graceful shutdown

Scheduler:

- stops creating normal new jobs;
- stops queue admission;
- allows selected jobs to complete;
- cancels remaining work;
- abandons unresponsive workers after deadline;
- persists recoverable state when configured.

---

## 6. Non-Responsibilities

Scheduler does not own:

### 6.1 Business workflows

Scheduler does not decide:

- when OCR should logically follow image detection;
- when translation should follow OCR;
- when rendering should follow translation;
- whether a document is complete.

Those decisions belong to orchestration or owning modules.

### 6.2 Business state

Scheduler tracks execution state, not domain state.

### 6.3 Job implementation

Scheduler does not implement:

- OCR;
- translation;
- cache eviction logic;
- provider calls;
- file cleanup logic;
- secret rotation logic.

### 6.4 Event transport

Event Bus transports facts.

Scheduler may consume trigger events, but does not replace Event Bus.

### 6.5 General Runtime work queue

Interactive and pipeline work already governed by the Runtime architecture must not be silently duplicated inside Scheduler.

Scheduler is intended primarily for:

- delayed;
- recurring;
- maintenance;
- retry;
- low-priority;
- resource-conditioned;
- recoverable background work.

### 6.6 Logging and Telemetry

Scheduler emits safe logs and telemetry but does not own those systems.

### 6.7 Distributed scheduling

Distributed coordination is deferred.

---

## 7. Scheduler Versus Runtime

CRAI already has Runtime concepts for active pipeline execution.

The boundary is:

```text
Runtime
    → immediate application work
    → pipeline execution
    → interactive work
    → work-item lifecycle

Scheduler
    → delayed work
    → recurring work
    → maintenance
    → retry timing
    → background execution
    → restart-recoverable jobs
```

Some jobs may dispatch into Runtime.

Example:

```text
Scheduler trigger fires
    ↓
Scheduler creates background translation job
    ↓
Owning module submits Runtime work item
    ↓
Runtime executes translation pipeline
```

Scheduler must not duplicate Runtime's internal pipeline orchestration.

---

## 8. Scheduler Versus Event Bus

Event Bus:

```text
communicates that a fact occurred
```

Scheduler:

```text
determines when a background execution becomes eligible
```

Valid event-triggered flow:

```text
ConfigurationActivated event
    ↓
Scheduler trigger adapter
    ↓
Job created
    ↓
Worker executes maintenance task
```

Invalid flow:

```text
Every event automatically becomes a scheduled job
```

Event triggers must be explicitly registered.

---

## 9. Scheduler Versus Cron

Cron is only one trigger type.

```text
Scheduler
├── Immediate
├── Delayed
├── Interval
├── Cron
├── Manual
├── Event-triggered
├── Retry-triggered
└── Future conditional triggers
```

---

## 10. Core Components

```text
Scheduler API
Task Registry
Task Definition Validator
Schedule Registry
Trigger Engine
Clock Adapter
Job Factory
Job Registry
Priority Queue
Delayed Queue
Retry Queue
Dependency Resolver
Concurrency Controller
Resource Controller
Dispatcher
Worker Registry
Execution Supervisor
Timeout Supervisor
Cancellation Manager
Misfire Handler
Persistence Adapter
Recovery Manager
Scheduler Diagnostics
```

---

## 11. Task Definition

Conceptual structure:

```text
TaskDefinition {
    taskId
    ownerModule
    handlerId

    trigger
    priority

    timeoutPolicy
    retryPolicy
    concurrencyPolicy
    overlapPolicy
    misfirePolicy
    resourcePolicy
    shutdownPolicy

    persistenceMode
    enabled
}
```

Detailed contracts belong in `CONTRACT.md`.

---

## 12. Trigger Types

### 12.1 Immediate

Creates a job as soon as admission permits.

### 12.2 Delayed

Creates or releases a job after a delay or target time.

### 12.3 Interval

Runs repeatedly at a fixed interval.

Two interval semantics should be distinguished:

```text
FIXED_RATE
FIXED_DELAY
```

`FIXED_RATE` is based on scheduled time.

`FIXED_DELAY` is based on completion time.

### 12.4 Cron

Runs according to an explicit cron expression and timezone.

### 12.5 Manual

Runs only when explicitly requested.

### 12.6 Event-triggered

Creates a job in response to an authorized Event Bus event.

### 12.7 Retry-triggered

Creates a new attempt after retry delay.

---

## 13. Time Semantics

Scheduler must distinguish:

```text
wall clock
monotonic clock
timezone
```

Use wall clock for:

- cron;
- calendar schedules;
- user-visible run time.

Use monotonic time for:

- delays;
- timeout;
- duration;
- backoff.

Clock changes must not cause unbounded duplicate execution.

---

## 14. Timezone

Cron schedules require an explicit timezone.

Default application timezone may be used only when the schedule contract permits it.

A timezone change must not silently rewrite historical executions.

---

## 15. Priority Model

Recommended priority classes:

```text
CRITICAL
HIGH
NORMAL
LOW
BACKGROUND
MAINTENANCE
```

Examples:

```text
CRITICAL
    → required security expiration check

HIGH
    → provider recovery check needed by active work

NORMAL
    → requested background translation

LOW
    → metadata refresh

BACKGROUND
    → cache warming

MAINTENANCE
    → log retention cleanup
```

Priority does not override:

- resource safety;
- authorization;
- bounded capacity;
- dependency;
- concurrency limits;
- shutdown rules.

---

## 16. Queue Model

Scheduler should use bounded logical queues.

Potential queue classes:

```text
READY
DELAYED
RETRY
RESOURCE_WAIT
DEPENDENCY_WAIT
```

The implementation may use one underlying priority structure, but the lifecycle distinctions remain explicit.

---

## 17. Queue Ordering

Default ordering may consider:

```text
effectivePriority
deadline
scheduledAt
enqueueSequence
fairnessGroup
```

Global FIFO is not required.

Same-key ordering may be configured when jobs mutate one shared resource.

---

## 18. Fairness

Scheduler should avoid starvation.

Fairness may operate across:

- owner module;
- task;
- resource class;
- priority class;
- concurrency group.

CRITICAL work may preempt admission preference, but running work is not forcibly preempted unless its policy supports cancellation.

---

## 19. Deadline

A job may have:

```text
scheduledAt
notBefore
startDeadline
executionTimeout
completionDeadline
```

These must not be treated as the same concept.

---

## 20. Retry Policy

Retry policy may include:

```text
maximumAttempts
retryableErrorClasses
backoffStrategy
initialDelay
maximumDelay
jitter
retryBudget
```

Backoff strategies:

```text
FIXED
LINEAR
EXPONENTIAL
EXPONENTIAL_WITH_JITTER
CUSTOM_REGISTERED
```

Retry must be bounded.

---

## 21. Retry Ownership

The worker reports:

```text
failure classification
retry recommendation
safe error code
```

Scheduler decides:

```text
whether retry is allowed
when the next attempt may run
```

---

## 22. Idempotency

Retryable jobs should declare an idempotency policy.

Possible modes:

```text
IDEMPOTENT
IDEMPOTENCY_KEY_REQUIRED
AT_MOST_ONCE
RECONCILIATION_REQUIRED
```

Scheduler must not promise exactly-once execution.

---

## 23. Delivery and Execution Guarantee

MVP in-memory guarantee:

```text
best effort
at-most-once admission per in-memory attempt
```

With future durable persistence:

```text
at-least-once execution may occur
```

Consumers must remain duplicate-aware.

---

## 24. Timeout

Timeout ends the logical authority of an attempt.

```text
RUNNING
    ↓ timeout
TIMED_OUT
```

The worker receives cancellation.

If physical work continues beyond grace:

```text
ABANDONED
```

Late completion must not overwrite the terminal Scheduler outcome.

---

## 25. Cancellation

Cancellation is cooperative.

Possible cancellation scopes:

```text
JOB
TASK
OWNER_MODULE
CONCURRENCY_KEY
RESOURCE_CLASS
SCHEDULER_SHUTDOWN
```

A canceled recurring schedule may either:

- stop only the current attempt;
- pause future occurrences;
- disable the schedule;

depending on request type.

---

## 26. Dependency Model

Dependencies should be execution-oriented.

Supported examples:

```text
job A completed successfully
resource became available
module reached ready state
configuration revision active
```

Business workflow dependencies should remain outside Scheduler.

Dependency cycles must be rejected.

---

## 27. Concurrency Policy

Conceptual policy:

```text
ConcurrencyPolicy {
    globalLimit?
    perTaskLimit?
    perOwnerLimit?
    perKeyLimit?
    keySelector?
}
```

Examples:

```text
OCR jobs:
    maximum 1 GPU job

Translation jobs:
    maximum 4 network jobs

Log rotation:
    maximum 1 per sink

Secret rotation:
    maximum 1 per secret
```

---

## 28. Resource Policy

Conceptual requirements:

```text
ResourceRequirement {
    resourceClass
    units
    exclusive
    minimumAvailability?
}
```

Resource classes may include:

```text
CPU
GPU
DISK_IO
NETWORK
MEMORY
UI_SENSITIVE
PROVIDER_QUOTA
```

---

## 29. Resource-Aware Examples

```text
OCR job
    requires GPU = 1

Translation job
    requires NETWORK = 1
    requires provider quota

Cache cleanup
    requires DISK_IO
    priority = MAINTENANCE

Resource collector
    priority = BACKGROUND
    skip under pressure
```

---

## 30. Interactive Protection

Scheduler must protect the reading experience.

Policies may:

- reduce background concurrency while user is interacting;
- pause maintenance under high CPU/GPU pressure;
- delay disk-heavy cleanup;
- preserve interactive Runtime capacity;
- resume work when idle.

Scheduler should not infer user state directly.

It consumes a safe resource or interaction signal from the owning infrastructure.

---

## 31. Overlap Policy

Recurring work must define what happens when the previous run is still active.

```text
ALLOW
SKIP_NEW
QUEUE_ONE
QUEUE_ALL_BOUNDED
CANCEL_PREVIOUS
REPLACE_PENDING
```

Recommended defaults:

```text
maintenance cleanup
    → SKIP_NEW

resource collection
    → SKIP_NEW

telemetry export
    → QUEUE_ONE

provider health check
    → SKIP_NEW

background translation
    → explicit per-task policy
```

---

## 32. Misfire Policy

A misfire occurs when a scheduled occurrence was not evaluated or executed on time.

Possible reasons:

- application suspended;
- application stopped;
- device sleeping;
- Scheduler unavailable;
- resource constraint;
- persistence recovery.

Policies:

```text
SKIP
RUN_ONCE_NOW
RUN_ALL_BOUNDED
RESCHEDULE_FROM_NOW
RESUME_NEXT_OCCURRENCE
```

---

## 33. Worker Contract

Workers should:

- accept immutable job input;
- report safe progress when enabled;
- honor cancellation;
- avoid blocking Scheduler threads;
- classify failures safely;
- not mutate Scheduler state;
- return one terminal result;
- be duplicate-aware when retryable or durable.

---

## 34. Worker Isolation

One worker failure must not:

- stop the dispatcher;
- corrupt unrelated jobs;
- block shutdown indefinitely;
- expose raw exceptions;
- consume unbounded resources.

---

## 35. Progress

Job progress is optional and non-authoritative.

It may be:

- throttled;
- coalesced;
- sampled;
- dropped.

Terminal outcomes must never be dropped.

---

## 36. Scheduler Lifecycle

Conceptual lifecycle:

```text
CREATED
    ↓
INITIALIZING
    ↓
READY
    ↓
RUNNING
    ↓
DEGRADED
    ↓
QUIESCING
    ↓
DRAINING
    ↓
STOPPING
    ↓
TERMINATED
```

Failure state:

```text
FAILED
```

Detailed state machines belong in `STATES.md`.

---

## 37. Task Lifecycle

Conceptual lifecycle:

```text
DRAFT
    ↓
VALIDATING
    ↓
REGISTERED
    ↓
ENABLED
    ↓
PAUSED
    ↓
DISABLED
    ↓
REMOVED
```

---

## 38. Job Lifecycle

Conceptual lifecycle:

```text
CREATED
    ↓
SCHEDULED
    ↓
WAITING
    ↓
READY
    ↓
DISPATCHED
    ↓
RUNNING
    ↓
terminal outcome
```

Terminal outcomes:

```text
SUCCEEDED
FAILED
CANCELED
TIMED_OUT
ABANDONED
SKIPPED
EXPIRED
```

---

## 39. Attempt Lifecycle

Conceptual lifecycle:

```text
CREATED
    ↓
STARTING
    ↓
RUNNING
    ↓
SUCCEEDED / FAILED / CANCELED / TIMED_OUT / ABANDONED
```

A retry creates a new attempt identity.

---

## 40. Scheduler Health

Scheduler health may be:

```text
HEALTHY
DEGRADED
UNAVAILABLE
```

Possible degraded reasons:

- optional persistence unavailable;
- one worker unavailable;
- queue pressure;
- clock warning;
- one resource monitor unavailable;
- retry backlog;
- event-trigger adapter unavailable.

---

## 41. Failure Philosophy

Scheduler is important infrastructure, but job failures remain isolated.

Rules:

- one failed job does not fail Scheduler;
- one failed worker does not stop unrelated workers;
- one failed optional trigger does not stop all schedules;
- persistence failure affects only durability-dependent jobs when possible;
- unsafe state or queue corruption may fail Scheduler;
- shutdown always remains bounded.

---

## 42. Backpressure

Possible backpressure actions:

```text
reject low-priority new jobs
delay recurring jobs
coalesce replaceable maintenance jobs
skip misfired low-value jobs
reduce concurrency
pause resource-heavy jobs
use bounded admission wait
```

Scheduler must not allow unbounded job accumulation.

---

## 43. Persistence Modes

Potential modes:

```text
IN_MEMORY
DURABLE_SCHEDULE
DURABLE_PENDING_JOB
DURABLE_EXECUTION
```

MVP should begin with:

```text
IN_MEMORY
```

Potentially durable early exceptions:

- security expiration schedules;
- critical cleanup markers;
- application update checks;

only if product requirements justify them.

---

## 44. Recovery

Future restart recovery may:

- reload enabled schedules;
- recreate future occurrences;
- detect missed occurrences;
- apply misfire policy;
- reload retryable pending jobs;
- reconcile uncertain running jobs.

A job that was `RUNNING` during process crash becomes:

```text
UNKNOWN / INTERRUPTED
```

It must not be assumed successful.

---

## 45. Scheduler Startup Order

Recommended startup order:

```text
Configuration
    ↓
Logging
    ↓
Telemetry
    ↓
Event Bus
    ↓
Resource monitors
    ↓
Scheduler
    ↓
Feature task registration
    ↓
Scheduler RUNNING
```

Exact order may depend on Composition Root.

---

## 46. Scheduler Shutdown Order

Recommended shutdown:

```text
Stop feature job creation
    ↓
Scheduler QUIESCING
    ↓
Stop recurring triggers
    ↓
Drain selected jobs
    ↓
Cancel remaining attempts
    ↓
Persist recoverable state
    ↓
Scheduler TERMINATED
    ↓
Logging final flush later
```

Scheduler should stop before Logging.

---

## 47. Event Bus Interaction

Scheduler may publish safe events for:

- task registration;
- schedule activation;
- job creation;
- job readiness;
- job started;
- job completed;
- retry scheduled;
- timeout;
- cancellation;
- abandonment;
- queue pressure;
- Scheduler degradation.

Scheduler must not publish raw job input.

---

## 48. Logging Interaction

Allowed fields:

```text
taskId
jobId
attemptId
ownerModule
triggerType
priority
state
normalizedErrorCode
durationClass
queueWaitDuration
retryCount
resourceClass
```

Prohibited:

```text
raw job payload
OCR text
translation text
image data
provider response
secret
credential
authorization data
```

---

## 49. Telemetry Interaction

Recommended metrics:

```text
scheduler_jobs_created_total
scheduler_jobs_started_total
scheduler_jobs_completed_total
scheduler_jobs_failed_total
scheduler_jobs_retried_total
scheduler_jobs_timed_out_total
scheduler_jobs_abandoned_total
scheduler_queue_depth
scheduler_queue_wait_duration
scheduler_execution_duration
scheduler_active_jobs
scheduler_misfires_total
scheduler_resource_wait_total
```

---

## 50. Configuration Interaction

Configuration controls:

- queue capacities;
- worker concurrency;
- retry defaults;
- timeout defaults;
- scheduling timezone;
- resource thresholds;
- shutdown deadline;
- persistence mode;
- diagnostic limits.

Task-specific business policy remains owned by the registering module.

---

## 51. Security

Scheduler must reject:

- secret-bearing job payloads;
- raw credential objects;
- authorization headers;
- unbounded arbitrary object graphs;
- raw user content unless an explicit task contract permits a safe reference;
- unauthorized task registration;
- unauthorized manual execution;
- unsafe event-trigger subscriptions.

Large inputs use references:

```text
ArtifactId
DocumentId
PageId
ChapterId
ProviderRequestId
```

---

## 52. Authorization

Scheduler should identify:

```text
task owner
registration authority
manual trigger authority
cancellation authority
administrative authority
```

One module must not cancel or replace another module's jobs unless explicitly authorized.

---

## 53. Idempotency Key

Jobs may carry an idempotency key such as:

```text
taskId + logicalTarget + revision + occurrence
```

The key must not contain secret or user content.

---

## 54. Duplicate Handling

Duplicate jobs may be:

```text
REJECTED
COALESCED
REPLACED
LINKED_TO_EXISTING
QUEUED_SEPARATELY
```

depending on task policy.

---

## 55. Job Input

Job input should be:

- typed;
- immutable;
- bounded;
- serializable when durable;
- safe;
- reference-based for large data.

---

## 56. Job Output

Scheduler should retain only a bounded execution summary.

Full business results belong to the owning module or storage.

---

## 57. Diagnostics

Scheduler diagnostics may expose:

- lifecycle;
- registered task count;
- active schedule count;
- queue depth;
- delayed job count;
- retry backlog;
- running jobs;
- blocked jobs;
- worker health;
- resource availability;
- recent normalized failures;
- next scheduled occurrences.

Diagnostics must not expose job payloads by default.

---

## 58. Testing Support

The module should provide:

```text
ManualClock
TestScheduler
InlineWorker
RecordingWorker
FaultInjectingWorker
InMemoryTaskRegistry
InMemoryJobStore
DeterministicTriggerEngine
ManualResourceController
```

---

## 59. Required Tests

### Registration

- duplicate task;
- invalid task;
- unauthorized owner;
- unbounded timeout;
- invalid retry;
- dependency cycle;
- unsupported trigger.

### Trigger

- immediate;
- delayed;
- fixed-rate;
- fixed-delay;
- cron;
- timezone;
- clock change;
- event trigger;
- misfire.

### Queue

- priority;
- fairness;
- capacity;
- low-priority rejection;
- critical admission;
- starvation prevention.

### Execution

- success;
- failure;
- retry;
- timeout;
- cancellation;
- abandonment;
- late completion;
- duplicate result.

### Concurrency

- per-task limit;
- per-key serialization;
- global limit;
- resource-class limit;
- overlap policy.

### Resource

- GPU unavailable;
- provider quota unavailable;
- interactive protection;
- resource recovery.

### Shutdown

- recurring triggers stop;
- drain selected work;
- cancel remainder;
- abandon unresponsive worker;
- bounded termination.

### Recovery

- missed schedule;
- retry restoration;
- uncertain running job;
- duplicate prevention.

---

## 60. Core Invariants

1. Scheduler coordinates execution, not business meaning.
2. A task definition is not a job instance.
3. A job instance may have multiple attempts.
4. One attempt has exactly one terminal outcome.
5. Late completion cannot overwrite timeout or abandonment.
6. Retry is bounded.
7. Queues are bounded.
8. Shutdown is bounded.
9. Dependencies are explicit.
10. Dependency cycles are rejected.
11. Resource limits are explicit.
12. Priority does not override safety.
13. Running jobs are not implicitly preempted.
14. Recurring jobs define overlap policy.
15. Missed schedules define misfire policy.
16. Scheduler does not promise exactly-once.
17. Durable execution may produce duplicates.
18. Job inputs are typed, bounded, and safe.
19. Large content uses references.
20. Secret material is prohibited.
21. Worker failures are isolated.
22. Scheduler failure does not rewrite owning-module state.
23. Business results remain outside Scheduler.
24. Event-triggered jobs require explicit registration.
25. Scheduler and Runtime boundaries remain distinct.

---

## 61. Key Architectural Decisions

### Decision 1 — Keep the name `scheduler`

The folder remains:

```text
03-infrastructure/scheduler/
```

The architecture defines it as a Background Execution Runtime.

### Decision 2 — Separate Task, Job, and Attempt

```text
TaskDefinition
    ↓
JobInstance
    ↓
JobAttempt
```

### Decision 3 — Scheduler is not business orchestration

Feature and pipeline decisions remain outside Scheduler.

### Decision 4 — Runtime boundary remains explicit

Scheduler handles delayed, recurring, retry, and maintenance work.

### Decision 5 — Bounded queues

No unbounded job accumulation.

### Decision 6 — Retry is policy-driven

Workers classify failures; Scheduler owns retry timing.

### Decision 7 — Resource-aware execution

CPU, GPU, network, disk, and provider quota may limit dispatch.

### Decision 8 — Local in-memory MVP

Durability and distributed scheduling are deferred.

### Decision 9 — Exactly-once is not promised

Consumers must support idempotency where required.

### Decision 10 — Shutdown is bounded

Unresponsive jobs become abandoned after policy deadline.

### Decision 11 — Overlap and misfire are explicit

Recurring work cannot rely on implicit behavior.

### Decision 12 — Job payloads remain safe and bounded

Large data uses references.

---

## 62. MVP Scope

The MVP should support:

```text
task registration
manual execution
immediate execution
delayed execution
interval execution
basic cron execution
priority
bounded ready queue
bounded delayed queue
global concurrency
per-task concurrency
per-key serialization
timeout
cancellation
bounded retry
fixed and exponential backoff
overlap policy
misfire policy
worker registry
execution tracking
safe logging
telemetry
bounded shutdown
in-memory diagnostics
manual clock for tests
```

---

## 63. Deferred Capabilities

Deferred:

```text
durable job persistence
distributed scheduler
leader election
multi-process workers
remote workers
workflow DAG engine
calendar exclusions
business-day calendars
conditional triggers
machine-idle triggers
network-aware triggers
battery-aware scheduling
advanced preemption
cross-device scheduling
exactly-once execution
```

---

## 64. Open Decisions

### Contract decisions

- exact `TaskDefinition`;
- exact `JobInstance`;
- exact `JobAttempt`;
- worker interface;
- trigger interface;
- retry policy;
- overlap policy;
- misfire policy;
- resource policy;
- persistence adapter;
- diagnostics contract.

### State decisions

- Scheduler lifecycle;
- task lifecycle;
- schedule lifecycle;
- trigger lifecycle;
- job lifecycle;
- attempt lifecycle;
- queue lifecycle;
- worker lifecycle;
- resource reservation lifecycle;
- retry lifecycle;
- shutdown lifecycle;
- recovery lifecycle.

### Event decisions

- Scheduler started;
- task registered;
- schedule activated;
- trigger fired;
- job created;
- job queued;
- job started;
- job completed;
- job failed;
- retry scheduled;
- job timed out;
- job abandoned;
- queue pressure;
- resource blocked;
- Scheduler degraded.

### Error decisions

- invalid task;
- duplicate task;
- invalid cron;
- dependency cycle;
- queue full;
- worker unavailable;
- timeout;
- retry exhausted;
- resource unavailable;
- persistence unavailable;
- shutdown timeout;
- recovery uncertainty.

### Policy decisions

- default concurrency;
- default timeout;
- default retry;
- queue capacities;
- priority aging;
- fairness;
- shutdown drain policy;
- interactive protection thresholds;
- cron timezone;
- misfire defaults.

### Implementation decisions

- queue primitives;
- timer wheel versus priority heap;
- cron parser;
- worker execution primitive;
- cancellation mechanism;
- resource-controller integration;
- persistence technology;
- startup reconciliation.

---

## 65. Documentation Order

Recommended order:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
EVENTS.md
    ↓
ERRORS.md
    ↓
README.md
```

`CONTRACT.md` should next define:

- `Scheduler`;
- `TaskDefinition`;
- `ScheduleDefinition`;
- `Trigger`;
- `JobInstance`;
- `JobAttempt`;
- `JobInput`;
- `JobResult`;
- `Worker`;
- `WorkerRegistry`;
- `RetryPolicy`;
- `BackoffPolicy`;
- `TimeoutPolicy`;
- `ConcurrencyPolicy`;
- `OverlapPolicy`;
- `MisfirePolicy`;
- `ResourcePolicy`;
- `JobStore`;
- lifecycle controls;
- diagnostics queries.

---

## 66. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/configuration/
03-infrastructure/event-bus/
03-infrastructure/logging/
03-infrastructure/telemetry/
03-infrastructure/secret-management/
```

Future Scheduler documents:

```text
03-infrastructure/scheduler/CONTRACT.md
03-infrastructure/scheduler/STATES.md
03-infrastructure/scheduler/EVENTS.md
03-infrastructure/scheduler/ERRORS.md
03-infrastructure/scheduler/README.md
```

---

## 67. Summary

Scheduler is CRAI's shared Background Execution Runtime.

The main conceptual flow is:

```text
TaskDefinition
    ↓
Trigger
    ↓
JobInstance
    ↓
Queue admission
    ↓
Resource and concurrency checks
    ↓
Worker dispatch
    ↓
JobAttempt
    ↓
Result / Retry / Timeout / Cancellation
```

The module deliberately separates:

```text
Task
Job
Attempt
Trigger
Worker
Result
```

The MVP favors:

```text
local
in-memory
bounded
priority-aware
resource-aware
cancellable
retry-capable
predictable
```

The architecture guarantees:

- Scheduler does not own business workflow meaning;
- Runtime and Scheduler remain distinct;
- queues and retries are bounded;
- overlap and misfire are explicit;
- worker failures are isolated;
- timeout and abandonment are terminal;
- shutdown is bounded;
- secret and large payloads are prohibited;
- exactly-once execution is not promised;
- future persistence can extend the module without changing task semantics.

This document is the architectural source of truth for subsequent Scheduler contracts, states, events, errors, and implementation documentation.
