# Event Bus

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Event Bus  
> **Path:** `03-infrastructure/event-bus/README.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06

---

## 1. Overview

Event Bus is the shared CRAI infrastructure for safe, typed, immutable, asynchronous communication between modules.

It transports facts that have already occurred.

It does not:

- own domain state;
- replace queries;
- route business commands by default;
- orchestrate pipeline stages;
- transport large artifacts;
- transport secret material;
- provide exactly-once guarantees.

The Event Bus exists to reduce direct coupling while preserving clear ownership and predictable runtime behavior.

---

## 2. MVP Architecture

The CRAI MVP uses:

```text
In-process
In-memory
Typed events
Asynchronous handlers
At-most-once delivery
Bounded queues
Scoped ordering
Subscriber isolation
```

The MVP does not use:

```text
Kafka
RabbitMQ
Redis Pub/Sub
NATS
External message broker
Distributed consumer groups
Durable replay
Exactly-once delivery
```

Future durable adapters may add outbox-based and at-least-once delivery without changing the semantic meaning of events.

---

## 3. Core Flow

```text
Module commits state
    ↓
Module creates immutable event
    ↓
Event Bus validates envelope
    ↓
Publisher authorization checked
    ↓
Payload safety inspected
    ↓
Ordering lane selected
    ↓
Bounded queue admission
    ↓
Event routed to eligible subscribers
    ↓
Handlers execute in isolation
    ↓
Delivery outcomes observed safely
```

---

## 4. Responsibilities

Event Bus owns:

- event publication;
- event subscription;
- event-type registry;
- publisher authorization;
- subscriber authorization;
- visibility and security routing;
- version compatibility;
- queue admission;
- scoped ordering;
- asynchronous dispatch;
- subscriber failure isolation;
- handler timeout;
- backpressure;
- progress throttling and coalescing;
- bounded shutdown drain;
- safe delivery observability;
- future durable-adapter boundary.

---

## 5. Non-Responsibilities

Event Bus does not own:

- domain event meaning;
- module state transitions;
- business retry policy;
- provider selection;
- pipeline orchestration;
- queries;
- large file transfer;
- secret storage;
- audit retention implementation;
- external broker technology.

Correct orchestration:

```text
Stage completed
    ↓
Event published
    ↓
Pipeline Orchestrator evaluates state
    ↓
Explicit next command issued
```

Incorrect orchestration:

```text
Stage completed
    ↓
Subscriber silently starts next stage
```

---

## 6. Event Contract

Every event uses a typed immutable envelope.

```text
EventEnvelope<TPayload> {
    eventId
    eventType
    eventVersion
    category

    occurredAt
    publishedAt

    sourceModule
    publisherId

    correlationId
    causationId?

    applicationInstanceId

    sessionId?
    pipelineId?
    taskId?
    workItemId?
    attemptId?
    entityId?
    contentRevision?

    ordering
    priority
    visibility
    securityClassification

    payload
    metadata
}
```

Canonical event name:

```text
<module>.<entity>.<past-tense-fact>
```

Examples:

```text
configuration.snapshot.activated
secret-management.secret.revoked
provider-management.lease.granted
translation.job.completed
application.shutdown.started
```

---

## 7. Event Categories

```text
DOMAIN
INTEGRATION
RESULT
PROGRESS
SYSTEM
SECURITY
AUDIT
OBSERVABILITY
```

Commands are intentionally excluded from the normal Event Bus category model.

---

## 8. Priority

```text
CRITICAL
HIGH
NORMAL
LOW
BACKGROUND
```

Priority affects queue and dispatch preference.

It does not override:

- security;
- visibility;
- same-lane ordering;
- bounded capacity;
- event semantics.

---

## 9. Visibility

```text
PUBLIC_INTERNAL
MODULE_INTERNAL
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
AUDIT_ONLY
LOCAL_COMPONENT_ONLY
```

Restricted events are routed only to explicitly authorized subscribers.

No visibility class permits secret material.

---

## 10. Ordering

Ordering is scoped, not global.

Typical ordering keys:

```text
application
session
pipeline
task
work item
entity
secret
provider
configuration
custom key
```

Example:

```text
session:<SessionId>
pipeline:<PipelineId>
secret:<SecretId>
provider:<ProviderId>
configuration:global
```

For the same subscription and ordering key:

```text
Event N must terminate
before Event N+1 begins
```

when the subscription uses serial or ordered-by-key execution.

Events in different lanes may execute concurrently.

---

## 11. Publication Semantics

Default publication mode:

```text
ENQUEUE_CONFIRMED
```

A successful receipt means:

```text
Envelope accepted
Security checks passed
Queue admission succeeded
```

It does not mean:

```text
All subscribers handled the event
Downstream state changed
Business work succeeded
Durable delivery occurred
Exactly-once processing occurred
```

Publication and delivery are separate lifecycles.

---

## 12. Delivery Semantics

Typical delivery lifecycle:

```text
CREATED
    ↓
SCHEDULED
    ↓
WAITING_FOR_LANE
    ↓
STARTING
    ↓
RUNNING
    ↓
HANDLED
```

Other terminal outcomes:

```text
IGNORED_NOT_RELEVANT
IGNORED_STALE
DUPLICATE
REJECTED_UNSUPPORTED_VERSION
FAILED
TIMED_OUT
CANCELED
ABANDONED
```

A terminal delivery never becomes active again.

Late physical completion after `TIMED_OUT` or `ABANDONED` is non-authoritative.

---

## 13. Queue and Backpressure

All queues are bounded.

Possible overflow policies:

```text
REJECT_NEW
DROP_LOWEST_PRIORITY
DROP_OLDEST_PROGRESS
COALESCE_PROGRESS
BLOCK_PUBLISHER_BOUNDED
ESCALATE
```

Recommended defaults:

```text
DOMAIN / INTEGRATION / RESULT
    → reject or bounded wait

PROGRESS
    → coalesce or drop older progress

OBSERVABILITY
    → sample or drop

SECURITY
    → reserved capacity and escalation

AUDIT
    → dedicated reliable sink
```

---

## 14. Progress Events

Progress events are:

- non-authoritative;
- high-frequency;
- replaceable;
- throttleable;
- coalescible.

Example:

```text
Progress 31%
Progress 32%
Progress 33%
```

may become:

```text
Progress 33%
```

The following must never be coalesced away:

```text
completion
failure
cancellation
revocation
removal
security compromise
configuration activation
revision activation
application shutdown
audit facts
```

---

## 15. Subscriber Isolation

One failing subscriber must not block unrelated subscribers.

Possible subscriber states:

```text
ACTIVE
PAUSED
DEGRADED
DISABLED
DRAINING
DISPOSED
```

Possible health states:

```text
UNKNOWN
HEALTHY
SLOW
FAILING
DEGRADED
CIRCUIT_OPEN
RECOVERING
UNHEALTHY
DISABLED
```

Handlers must have:

- bounded timeout;
- bounded concurrency;
- safe exception normalization;
- cooperative cancellation where possible.

---

## 16. Event Bus Lifecycle

```text
CREATED
    ↓
INITIALIZING
    ↓
READY
    ↓
RUNNING
    ↓
QUIESCING
    ↓
DRAINING
    ↓
STOPPING
    ↓
TERMINATED
```

Alternative states:

```text
DEGRADED
FAILED
```

Normal shutdown:

```text
Stop normal publication
    ↓
Allow critical system/security events
    ↓
Drain bounded queues
    ↓
Cancel remaining handlers
    ↓
Mark unconfirmed handlers abandoned
    ↓
Dispose subscriptions
    ↓
Terminate
```

Shutdown must never wait indefinitely.

---

## 17. Security Rules

The Event Bus must reject:

```text
SecretHandle
SecretMaterialInput
Authorization headers
Passwords
Access tokens
Refresh tokens
Private keys
Decrypted credentials
Raw environment values
Provider SDK clients
Platform credential objects
Raw images
Raw documents
Mutable UI objects
Unredacted exceptions
```

Large content must use references:

```text
ArtifactId
DocumentId
ResultId
SnapshotReference
ConfigurationRevision
```

---

## 18. Error Model

Major error groups:

```text
ENVELOPE
EVENT_TYPE
REGISTRY
PUBLISHER_AUTHORIZATION
SUBSCRIBER_AUTHORIZATION
SUBSCRIPTION
VERSION
ROUTING
ORDERING
QUEUE
BACKPRESSURE
PUBLICATION
DELIVERY
HANDLER
SUBSCRIBER_HEALTH
DISPATCHER
DRAIN
SHUTDOWN
SECURITY
PAYLOAD_SAFETY
EVENT_LOOP
DIAGNOSTICS
OUTBOX
DURABLE_ADAPTER
INTERNAL
```

Important distinctions:

```text
Publication Error
    ≠ Delivery Error
    ≠ Handler Error
    ≠ Queue Pressure
    ≠ Cancellation
    ≠ Timeout
    ≠ Abandonment
```

---

## 19. Self-Events

Event Bus publishes safe self-events for:

- lifecycle;
- registry;
- event-type registration;
- subscriptions;
- subscriber health;
- queue pressure;
- publication rejection;
- delivery failure;
- timeout;
- drain;
- security blocks;
- future durable adapters.

Self-events:

- never copy the original payload;
- use non-recursive reporting;
- may be sampled when high-volume;
- must not sample away critical security or terminal failures.

---

## 20. Observability

Safe metrics include:

```text
event_bus_published_total
event_bus_rejected_total
event_bus_dropped_total
event_bus_coalesced_total
event_bus_delivery_failed_total
event_bus_handler_timeout_total
event_bus_queue_depth
event_bus_queue_wait_duration
event_bus_handler_duration
event_bus_subscriber_degraded
event_bus_security_blocks_total
event_bus_shutdown_abandoned_total
```

Logs and traces may contain:

```text
eventId
eventType
eventVersion
sourceModule
subscriberId
subscriptionId
deliveryId
laneId
priority
visibility
normalizedErrorCode
correlationId
```

They must not contain event payloads by default.

---

## 21. Integration Boundaries

### Configuration

Publishes committed configuration snapshot facts.

Consumers query Configuration for current state.

### Secret Management

Uses restricted routing for safe security and availability facts.

Secret material remains prohibited.

### Provider Management

Publishes normalized provider, health, availability, and lease facts.

Provider-native clients and credentials remain internal.

### Runtime

Publishes work lifecycle facts.

Runtime remains owner of work state.

### Presentation

Consumes safe progress, status, completion, and user-action facts.

UI work must marshal to the UI scheduler.

### Logging and Telemetry

Consume safe envelope and outcome metadata.

Recursive telemetry loops must be prevented.

---

## 22. MVP Scope

Required:

```text
typed envelope
typed publication
typed subscription
event registry
publisher authorization
subscriber authorization
asynchronous dispatch
at-most-once delivery
bounded queues
per-session ordering
optional per-entity ordering
priority
visibility
security classification
handler timeout
subscriber isolation
progress throttling
progress coalescing
safe metrics
safe tracing
bounded shutdown
payload-size validation
sensitive-type rejection
```

Deferred:

```text
transactional outbox
durable delivery
at-least-once adapter
external broker
distributed partitions
consumer groups
durable replay
dead-letter storage
cross-process bus
cross-device replication
event sourcing
exactly-once claims
```

---

## 23. Core Invariants

1. Events are immutable facts.
2. State commits before publication.
3. Event Bus does not own domain state.
4. Event Bus does not replace queries.
5. Event Bus does not orchestrate pipelines.
6. Commands do not use the normal event path by default.
7. Queues are bounded.
8. Ordering is scoped.
9. Subscriber failures are isolated.
10. Progress may be coalesced.
11. Terminal facts are never coalesced away.
12. Secret material is prohibited.
13. Large payloads use references.
14. Publication and delivery remain separate.
15. Consumers remain duplicate-aware.
16. Late completion cannot rewrite terminal delivery state.
17. Shutdown is bounded.
18. MVP delivery is at-most-once.
19. Future durable delivery may be at-least-once.
20. Exactly-once is not promised.

---

## 24. Module Documents

```text
03-infrastructure/event-bus/
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
├── ERRORS.md
└── README.md
```

Document roles:

```text
MODULE.md
    architecture, ownership, boundaries and decisions

CONTRACT.md
    publisher, subscriber, envelope, queue and delivery contracts

STATES.md
    bus, subscription, lane, publication and delivery lifecycles

EVENTS.md
    Event Bus self-events

ERRORS.md
    normalized failures, warnings and recovery guidance

README.md
    module overview and navigation
```

---

## 25. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/PIPELINE_RUNTIME.md
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/configuration/EVENTS.md
03-infrastructure/secret-management/EVENTS.md
02-modules/provider-management/EVENTS.md
```

---

## 26. Summary

Event Bus is the CRAI infrastructure boundary for safe asynchronous event delivery.

Its primary model is:

```text
Committed fact
    ↓
Typed immutable event
    ↓
Validation and security inspection
    ↓
Bounded ordered queue
    ↓
Isolated subscribers
    ↓
Consumer-owned reaction
```

The MVP deliberately favors:

```text
simplicity
bounded memory
clear ownership
safe payloads
predictable shutdown
```

over distributed-broker complexity.

The complete module documentation is the source of truth for Event Bus implementation.
