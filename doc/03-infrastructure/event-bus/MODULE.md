# Event Bus Module

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Event Bus  
> **Document:** Module Architecture  
> **Path:** `03-infrastructure/event-bus/MODULE.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `docs/architecture/EVENT_BUS.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/PIPELINE_RUNTIME.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/configuration/EVENTS.md`
> - `03-infrastructure/secret-management/EVENTS.md`
> - `02-modules/provider-management/EVENTS.md`

---

## 1. Purpose

The Event Bus module provides the shared infrastructure used by CRAI modules to publish and subscribe to immutable typed events.

It enables modules to communicate facts without:

- directly depending on concrete implementations;
- calling every downstream consumer synchronously;
- sharing mutable internal state;
- exposing provider-native or platform-native payloads;
- coupling feature modules to transport technology;
- turning domain events into implicit workflow orchestration.

The Event Bus is infrastructure.

It transports already-defined event contracts.

It does not own the semantic meaning of module events.

---

## 2. Module Goal

The module must provide a predictable event-delivery boundary with:

- typed publication;
- typed subscription;
- immutable envelopes;
- correlation and causation propagation;
- bounded asynchronous delivery;
- subscriber isolation;
- entity-scoped ordering;
- duplicate-awareness;
- stale-event support;
- priority handling;
- cancellation-aware shutdown;
- safe observability;
- security classification;
- payload validation;
- event-version compatibility;
- optional durable publication extension.

The primary optimization target is not maximum throughput.

It is:

```text
safe, bounded, explainable module decoupling
```

---

## 3. Architectural Position

```text
Publishing Module
    ↓ creates domain/integration event
Core Event Contract
    ↓ validates envelope and payload
Event Bus
    ├── routing
    ├── ordering
    ├── queueing
    ├── subscriber isolation
    ├── delivery
    ├── observability
    └── shutdown coordination
    ↓
Subscribed Modules
```

The Core layer may define event abstractions and shared envelope contracts.

The Infrastructure Event Bus implements transport, routing, queueing, and delivery behavior.

Composition Root wires publishers, subscribers, and the concrete bus.

---

## 4. Core Architectural Principle

An event is an immutable notification that a fact has already occurred.

```text
Command
    asks for an action

Event
    reports an accepted fact

Query
    asks for current state
```

The Event Bus must not blur these boundaries.

---

## 5. Responsibilities

### 5.1 Event publication

The module accepts validated event envelopes from approved publishers.

It owns:

- publication admission;
- envelope validation;
- publication timestamps;
- routing lookup;
- queue admission;
- publication result;
- safe publication diagnostics.

### 5.2 Subscription registration

The module registers typed subscribers.

A subscription may declare:

- event type;
- supported versions;
- subscriber identity;
- delivery mode;
- ordering key requirement;
- visibility clearance;
- priority acceptance;
- concurrency policy;
- timeout policy;
- shutdown behavior;
- failure policy.

### 5.3 Event routing

The module routes events according to:

- event type;
- version compatibility;
- visibility;
- security classification;
- subscriber authorization;
- optional filter;
- local process scope;
- application instance scope.

### 5.4 Queue management

The module owns bounded queues used for asynchronous delivery.

Queueing must support:

- bounded capacity;
- priority classes;
- ordering lanes;
- overflow policy;
- progress-event coalescing;
- shutdown drain;
- queue metrics.

### 5.5 Subscriber isolation

One failing or slow subscriber must not block unrelated subscribers.

Isolation may use:

- independent handler invocation;
- bounded concurrency;
- per-subscriber timeout;
- failure capture;
- circuit or disable policy;
- dedicated ordering lane where necessary.

### 5.6 Ordering

The Event Bus provides scoped ordering, not global ordering.

Possible ordering keys:

```text
applicationInstanceId
sessionId
pipelineId
taskId
entityId
secretId
providerId
configurationRevision
custom partitionKey
```

The publisher or event contract determines the meaningful ordering scope.

### 5.7 Delivery result tracking

The module records safe delivery outcomes such as:

```text
ACCEPTED
DISPATCHED
DELIVERED
SKIPPED
FILTERED
REJECTED
TIMED_OUT
FAILED
DROPPED
COALESCED
```

Delivery outcomes do not alter the originating module state.

### 5.8 Version compatibility

The module validates whether a subscriber supports the event version.

It may:

- deliver directly;
- use a registered upcaster;
- skip an unsupported optional subscriber;
- reject mandatory incompatible delivery;
- report compatibility failure.

### 5.9 Security enforcement

The module enforces:

- visibility boundaries;
- subscriber authorization;
- restricted-channel routing;
- payload size limits;
- sensitive-type rejection;
- metadata safety;
- no-secret rules;
- no raw user-content rules where prohibited.

### 5.10 Observability

The module provides safe metrics and traces for:

- publication volume;
- queue depth;
- dispatch latency;
- handler latency;
- dropped events;
- coalesced events;
- subscriber failures;
- timeouts;
- version mismatches;
- unauthorized subscriptions;
- shutdown drain.

---

## 6. Non-Responsibilities

The Event Bus does not own the following.

### 6.1 Workflow orchestration

The Event Bus does not decide which pipeline stage runs next.

Incorrect:

```text
OCR_COMPLETED
    ↓ subscriber automatically starts segmentation
SEGMENTATION_COMPLETED
    ↓ subscriber automatically starts translation
```

Correct:

```text
OCR completed
    ↓ event informs interested modules
Pipeline Orchestrator evaluates current state
    ↓ issues explicit next command when appropriate
```

### 6.2 State ownership

An event cannot directly mutate another module's state.

The receiving module validates relevance and applies its own transition.

### 6.3 Query service

The Event Bus is not a source of current truth.

Consumers must query the owning module when current state is required.

### 6.4 Command routing

Commands may have a dedicated command router.

The Event Bus transports facts, not arbitrary action requests by default.

### 6.5 Retry policy for business work

The Event Bus may retry transport delivery in future durable modes.

It does not decide whether Translation, Recognition, or provider execution should retry.

### 6.6 Domain event definitions

Each owning module defines its own event names and payload meaning.

### 6.7 Persistent message broker

The MVP is not Kafka, RabbitMQ, Redis Pub/Sub, NATS, or another external broker.

### 6.8 Large artifact transport

Large images, documents, OCR blocks, and translation outputs must use artifact references.

### 6.9 Secret transport

Secret material is never transported by the Event Bus.

### 6.10 Audit-store implementation

The Event Bus may route audit events to a restricted sink.

It does not itself define long-term audit retention.

---

## 7. MVP Architecture

The initial CRAI Event Bus is:

```text
In-process
In-memory
Typed
Asynchronous handlers
At-most-once delivery
Bounded queues
Scoped ordering
Best-effort shutdown drain
```

Not used in the MVP:

```text
Kafka
RabbitMQ
Redis Pub/Sub
external broker
distributed consumer groups
cross-device event replication
durable replay as primary workflow
exactly-once delivery
```

---

## 8. Delivery Guarantee Decision

### 8.1 MVP

```text
At-most-once
```

An event accepted into memory may be lost if:

- the process crashes;
- the application terminates before dispatch;
- queue overflow policy drops it;
- a subscriber fails and no local retry is configured.

Correctness-critical module state must therefore be persisted before event publication.

### 8.2 Future durable mode

A persistent outbox or durable adapter may provide:

```text
At-least-once
```

Consumers must remain idempotent because duplicates become possible.

### 8.3 Exactly-once

Exactly-once delivery is not promised.

Semantic idempotency belongs to the consumer and owning module.

---

## 9. Event Categories

Canonical categories:

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

Commands are intentionally excluded from the canonical Event Bus category list.

---

## 10. Event Naming

Conceptual class names use past tense:

```text
ConfigurationActivated
SecretRevoked
ProviderLeaseGranted
TranslationCompleted
ApplicationShutdownStarted
```

Canonical event type:

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

## 11. Event Envelope

Conceptual shared envelope:

```text
EventEnvelope<TPayload> {
    eventId
    eventType
    eventVersion
    category

    occurredAt
    publishedAt

    sourceModule
    sourceComponent?

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

    orderingKey?
    partitionKey?

    priority
    visibility
    securityClassification

    payload
    metadata
}
```

---

## 12. Envelope Rules

Every event must contain:

```text
eventId
eventType
eventVersion
category
occurredAt
publishedAt
sourceModule
correlationId
applicationInstanceId
priority
visibility
securityClassification
payload
```

Optional identity fields are included only when relevant.

The envelope must remain immutable after publication.

---

## 13. Correlation and Causation

Correlation groups related work across modules.

Causation references the command or event that directly caused the fact.

```text
TranslateCommand C1
    ↓
TranslationStarted E1, causationId=C1
    ↓
ProviderLeaseGranted E2, causationId=E1
```

All events in the chain retain the same `correlationId`.

---

## 14. Event Priority

Canonical priorities:

```text
CRITICAL
HIGH
NORMAL
LOW
BACKGROUND
```

Priority affects queue admission, dispatch preference, overflow selection, shutdown drain order, and observability.

Priority must not bypass security or same-key ordering.

---

## 15. Visibility

Canonical visibility classes:

```text
PUBLIC_INTERNAL
MODULE_INTERNAL
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
AUDIT_ONLY
LOCAL_COMPONENT_ONLY
```

The Event Bus validates subscriber clearance before routing.

No visibility class permits raw secret material.

---

## 16. Publisher Contract

A publisher must:

1. own or be authorized to publish the event type;
2. publish only after its authoritative fact is accepted;
3. provide a valid immutable payload;
4. provide correlation context;
5. choose the correct visibility;
6. avoid large payloads;
7. avoid secret material;
8. avoid mutable SDK objects;
9. choose a meaningful ordering key when ordering matters;
10. tolerate publication failure without corrupting authoritative state.

---

## 17. Subscriber Contract

A subscriber must:

1. declare supported event types and versions;
2. validate relevance;
3. handle duplicates safely;
4. handle stale events safely;
5. avoid blocking the dispatcher indefinitely;
6. honor cancellation and shutdown;
7. not mutate the event;
8. not assume global ordering;
9. query the state owner when current truth matters;
10. isolate its own failures;
11. avoid republishing loops;
12. preserve correlation context;
13. obey security classification.

---

## 18. Dispatch Model

```text
Publisher creates event
    ↓
Envelope validation
    ↓
Security inspection
    ↓
Queue admission
    ↓
Ordering-lane selection
    ↓
Subscriber resolution
    ↓
Asynchronous dispatch
    ↓
Per-subscriber delivery outcome
```

The publisher does not wait for all handlers by default.

---

## 19. Publish Modes

Possible modes:

```text
FIRE_AND_OBSERVE
ENQUEUE_CONFIRMED
DELIVERY_SUMMARY
```

`ENQUEUE_CONFIRMED` is the preferred MVP default.

`DELIVERY_SUMMARY` is bounded and must not become a distributed transaction.

---

## 20. Queue Topology

The MVP should support:

```text
Global system lane
Per-session ordered lane
Optional per-entity ordered lane
Restricted security lane
Observability lane
```

A practical implementation may use logical keyed queues rather than one physical thread per key.

---

## 21. Ordering Model

Ordering is guaranteed only within the same ordering lane while the event remains in the same bus instance.

Examples:

```text
session:<SessionId>
pipeline:<PipelineId>
secret:<SecretId>
provider:<ProviderId>
configuration:global
application:global
```

Events in different lanes may be delivered concurrently.

---

## 22. Ordering Limitations

Ordering does not guarantee:

- subscriber completion order across different subscribers;
- global application ordering;
- ordering after process restart in the in-memory MVP;
- ordering across future distributed bus instances;
- historical delivery to late subscribers.

Consumers must use revisions and state versions.

---

## 23. Duplicate and Stale Handling

Consumers should deduplicate using `eventId` or a stable entity revision key.

The bus may provide a bounded recent-event cache but must not claim semantic exactly-once behavior.

The state-owning consumer decides whether an event is stale.

The bus transports events; it does not decide domain relevance.

---

## 24. Subscriber Concurrency and Timeout

A subscription may declare:

```text
SERIAL
ORDERED_BY_KEY
BOUNDED_PARALLEL
```

Unbounded parallelism is prohibited.

Every asynchronous handler must have a bounded timeout policy.

Subscriber timeout or failure does not roll back the publisher's committed state.

---

## 25. Subscriber Failure Isolation

When one subscriber fails:

- its failure is captured;
- other subscribers continue;
- the publisher is not rolled back;
- sensitive payloads are not logged;
- subscriber health may degrade;
- optional retry occurs only under explicit policy;
- critical local security behavior uses a direct fallback path.

---

## 26. Local Delivery Retry

The MVP keeps delivery retry minimal.

Possible policy:

```text
NONE
ONE_IMMEDIATE_RETRY
BOUNDED_TRANSIENT_RETRY
```

Default:

```text
NONE
```

Durable retries belong to future outbox or broker adapters.

---

## 27. Dead Letter Handling

A durable dead-letter queue is not required in the in-memory MVP.

The MVP may provide a bounded diagnostic failure buffer containing only safe envelope and failure metadata.

Future durable adapters may support a dead-letter store.

---

## 28. Queue Capacity and Overflow

All queues must be bounded.

Possible overflow policies:

```text
REJECT_NEW
DROP_LOWEST_PRIORITY
DROP_OLDEST_PROGRESS
COALESCE_PROGRESS
BLOCK_PUBLISHER_BOUNDED
ESCALATE
```

Guidance:

- domain and integration facts: reject new or bounded publisher wait;
- progress: coalesce or drop old progress;
- observability: sample or drop;
- critical security: reserve capacity and escalate;
- audit: use a dedicated reliable sink.

---

## 29. Progress Events

Progress events are non-authoritative, high-frequency, replaceable, throttleable, and coalescible.

Completion, failure, cancellation, and security state changes must never be coalesced away.

---

## 30. Backpressure

Backpressure may use:

- queue admission rejection;
- bounded publish wait;
- progress coalescing;
- low-priority dropping;
- subscriber concurrency reduction;
- upstream scheduler pressure;
- operational warnings.

The Event Bus must not create unbounded memory pressure.

---

## 31. Payload Policy

Large content uses references:

```text
ArtifactId
DocumentId
TranslationResultId
ConfigurationRevision
SnapshotReference
```

The bus must reject raw secrets, authorization headers, private keys, decrypted credentials, provider-native credential objects, and unsafe exception objects.

Generic infrastructure events should avoid full source or translated text.

---

## 32. Event Validation

Publication validation includes:

```text
Envelope schema
Event type ownership
Version
Required identity fields
Timestamp validity
Correlation context
Priority
Visibility
Security classification
Payload type
Payload size
Sensitive-type inspection
Metadata safety
Ordering key validity
```

Invalid events are rejected before queue admission.

---

## 33. Event Immutability and Versioning

The implementation should use immutable record types, read-only collections, and defensive copying where necessary.

Each event type has an independent version.

Changing semantic meaning requires a new version.

Security restrictions cannot be weakened by versioning.

Future upcasters must be deterministic, pure, side-effect free, and unable to invent business facts.

---

## 34. Event Registry

The module should maintain a registry of:

```text
eventType
currentVersion
ownerModule
category
payloadType
visibility
securityClassification
maximumPayloadSize
orderingRequirement
retentionClass
```

The registry supports validation and diagnostics.

---

## 35. Publisher Ownership

Only the owning module or an approved adapter may publish an event namespace.

```text
Secret Management → secret-management.*
Provider Management → provider-management.*
Translation → translation.*
```

---

## 36. Event Loops and Reentrancy

The Event Bus should detect causation cycles, repeated event-type sequences, and excessive chain depth.

An event published by a handler should be enqueued rather than dispatched recursively on the same call stack.

This prevents hidden synchronous coupling and reentrant state mutation.

---

## 37. Transaction Boundary

Publishing an event is not a distributed transaction.

```text
Module commits its own state
    ↓
Module publishes event
```

For stronger future delivery:

```text
Commit state + outbox record
    ↓
Outbox adapter publishes
```

---

## 38. Outbox, Replay, and Retention

Future persistent mode may add transactional outbox, delivery attempts, durable offsets, dead-letter handling, and replay.

General replay is deferred.

The in-memory MVP retains events only while queued or in bounded safe diagnostics.

Event retention never replaces module state persistence.

---

## 39. Application Startup

Recommended startup order:

```text
Core contracts available
    ↓
Event registry built
    ↓
Subscribers registered
    ↓
Event Bus initialized
    ↓
Publishers enabled
    ↓
Application ready
```

---

## 40. Application Shutdown

Recommended shutdown:

```text
Stop accepting normal publications
    ↓
Allow critical shutdown events
    ↓
Stop new subscriber work
    ↓
Drain bounded queues
    ↓
Cancel timed-out handlers
    ↓
Record abandoned deliveries safely
    ↓
Dispose subscriptions
    ↓
Terminate bus
```

Shutdown must be bounded.

---

## 41. Event Bus Lifecycle

Conceptual lifecycle:

```text
CREATED
    ↓
INITIALIZING
    ↓
RUNNING
    ↓
QUIESCING
    ↓
DRAINING
    ↓
TERMINATED
```

Failure states may include:

```text
DEGRADED
FAILED
```

Detailed states belong in `STATES.md`.

---

## 42. Critical Event Handling

Critical local safety must not rely solely on asynchronous subscribers.

The owning module performs its mandatory safety transition directly, then publishes an event for secondary reactions.

Examples include backend compromise, secret revocation, application shutdown, and fatal runtime state.

---

## 43. Observability

Safe logging fields include event type, version, category, source module, subscriber ID, correlation ID, priority, visibility, queue wait, handler duration, and normalized outcomes.

Do not log full payloads by default.

Recommended metrics include publication, rejection, drop, coalescing, dispatch, failure, timeout, queue depth, queue wait, handler duration, subscriber health, version mismatch, unauthorized subscription, and shutdown abandonment.

Trace spans may include:

```text
event.publish
event.queue
event.dispatch
event.handle
```

Trace attributes remain payload-free by default.

---

## 44. Configuration

Event Bus configuration may include:

```text
enabled
defaultQueueCapacity
criticalQueueReserve
maximumPayloadSize
defaultHandlerTimeout
shutdownDrainTimeout
progressThrottleInterval
progressCoalescingEnabled
subscriberFailureThreshold
subscriberDisablePolicy
diagnosticFailureBufferSize
durableAdapterEnabled
```

Transport implementation, durable adapter, queue topology, serialization format, and restricted-channel implementation should normally require restart.

---

## 45. Security Model

Security controls include:

- publisher authorization;
- subscriber authorization;
- event namespace ownership;
- visibility filtering;
- security classification;
- payload inspection;
- sensitive-type rejection;
- bounded metadata;
- restricted lanes;
- audit routing;
- safe diagnostics;
- no arbitrary production subscriber discovery.

---

## 46. Composition Root

Composition Root owns:

- concrete Event Bus creation;
- event registry assembly;
- subscriber registration;
- security clearance assignment;
- handler dependency injection;
- durable adapter wiring;
- lifecycle startup and shutdown.

Feature modules must not instantiate their own global buses.

---

## 47. Testing Support

The module should provide:

```text
TestEventBus
RecordingEventBus
SynchronousTestDispatcher
FaultInjectingSubscriber
ManualClock
DeterministicScheduler
```

Testing support must preserve envelope validation, security rules, ordering semantics, duplicate behavior, bounded capacity, and subscriber isolation.

---

## 48. Required Tests

Required test groups:

- publication admission and rejection;
- publisher authorization;
- payload size and secret rejection;
- routing and version compatibility;
- visibility enforcement;
- same-key ordering;
- different-key concurrency;
- subscriber isolation and timeout;
- bounded queue and overflow;
- progress coalescing;
- critical reserve;
- payload-free diagnostics;
- startup and shutdown lifecycle;
- reentrant publication;
- event-loop detection.

---

## 49. Performance Model

The Event Bus should optimize for:

- low publication overhead;
- bounded queue memory;
- predictable handler latency;
- no UI thread blocking;
- no global serialization bottleneck;
- minimal wasted progress delivery;
- stable burst behavior.

It should not optimize for distributed-scale throughput in the MVP.

---

## 50. Module Integrations

### Runtime

Runtime publishes work and lifecycle facts but remains state owner.

### Configuration

Configuration events preserve revision ordering and commit-before-publish.

### Secret Management

Secret events may use restricted security and audit lanes, but never transport secret material.

### Provider Management

Provider events remain provider-neutral and credential-safe.

### Presentation

Presentation receives safe status, progress, completion, error, and user-action facts, then queries owners for current view models.

### Storage

Future durable outbox, audit, dead-letter, and replay features may depend on Storage ports.

### Logging and Metrics

Infrastructure telemetry paths must prevent recursive event loops.

---

## 51. Core Invariants

1. Events are immutable facts.
2. State owners commit before publishing.
3. Events do not directly mutate state.
4. Event Bus does not orchestrate pipelines.
5. Event Bus does not replace queries.
6. Event Bus does not transport commands by default.
7. Subscriber failures are isolated.
8. Queues are bounded.
9. Ordering is scoped, not global.
10. Progress may be coalesced.
11. Completion and security facts are not coalesced away.
12. Event payloads do not contain secrets.
13. Large payloads use references.
14. Consumers are duplicate-aware.
15. Consumers validate stale relevance.
16. Visibility and security classification are enforced.
17. Publication failure does not roll back committed source state.
18. Critical local safety does not rely only on asynchronous delivery.
19. MVP delivery is at-most-once.
20. Future durable delivery may be at-least-once.
21. Exactly-once is not promised.
22. Shutdown is bounded.
23. Composition Root owns wiring.
24. Observability does not expose payloads.
25. Reentrant publication is queued, not recursively dispatched.

---

## 52. Key Architectural Decisions

1. The MVP uses an in-process typed bus.
2. The MVP uses at-most-once delivery.
3. Dispatch is asynchronous by default.
4. Ordering is scoped by session, pipeline, entity, or explicit key.
5. All queues are bounded.
6. Pipeline orchestration remains explicit.
7. Diagnostics do not log raw payloads.
8. Security and audit use restricted routing.
9. Event ownership and versions are registered.
10. Consumers remain idempotent.
11. Large data uses artifact references.
12. Events report committed truth.

---

## 53. Initial MVP Scope

The MVP should support:

```text
typed event envelope
typed publication
typed subscription
asynchronous dispatch
at-most-once delivery
bounded queues
per-session ordering
optional per-entity ordering
priority
visibility
security classification
subscriber isolation
handler timeout
progress throttling
progress coalescing
safe metrics
safe tracing
startup and shutdown lifecycle
event registry
payload size validation
sensitive-type rejection
```

---

## 54. Deferred Capabilities

Deferred capabilities include:

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
schema registry service
dynamic plugin subscriptions
exactly-once claims
event sourcing
long-term event archive
```

---

## 55. Open Decisions

### Contract decisions

- exact publisher interface;
- exact subscriber interface;
- publication receipt;
- delivery summary;
- event filter contract;
- event registry schema;
- ordering-key contract;
- restricted subscriber authorization.

### State decisions

- bus lifecycle;
- subscription lifecycle;
- queue lane lifecycle;
- subscriber health;
- dispatcher state;
- durable adapter state.

### Event decisions

- Event Bus self-observability events;
- subscriber-disabled event;
- queue-overflow event;
- security-inspection failure event;
- shutdown-drain event.

### Error decisions

- publication errors;
- queue capacity errors;
- routing errors;
- version mismatch;
- subscriber timeout;
- unauthorized publication;
- unsafe payload;
- shutdown abandonment.

### Policy decisions

- queue capacities;
- critical reserve;
- default timeout;
- subscriber failure threshold;
- overflow policy by category;
- maximum payload size;
- maximum causation depth;
- diagnostic buffer retention;
- local delivery retry.

### Implementation decisions

- queue/channel primitive;
- keyed ordering implementation;
- handler scheduling;
- UI scheduler integration;
- cancellation propagation;
- subscription reference lifetime;
- subscription disposal behavior.

---

## 56. Documentation Order

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

- `EventEnvelope`;
- `EventPublisher`;
- `EventSubscription`;
- `EventHandler`;
- `PublishRequest`;
- `PublishReceipt`;
- `DeliverySummary`;
- `SubscriptionDescriptor`;
- `EventTypeDescriptor`;
- ordering keys;
- filters;
- visibility;
- priority;
- lifecycle controls;
- shutdown contracts.

---

## 57. Related Documents

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

Future Event Bus documents:

```text
03-infrastructure/event-bus/CONTRACT.md
03-infrastructure/event-bus/STATES.md
03-infrastructure/event-bus/EVENTS.md
03-infrastructure/event-bus/ERRORS.md
03-infrastructure/event-bus/README.md
```

---

## 58. Summary

The Event Bus is the shared CRAI infrastructure for safe, typed, immutable, asynchronous event delivery.

Its central flow is:

```text
Committed module fact
    ↓
Typed immutable event
    ↓
Envelope and security validation
    ↓
Bounded queue
    ↓
Scoped ordered dispatch
    ↓
Isolated subscribers
    ↓
Consumer relevance validation
    ↓
Consumer-owned reaction
```

The MVP implementation is:

```text
in-process
in-memory
typed
asynchronous
bounded
at-most-once
scoped-ordering
```

The module deliberately excludes workflow orchestration, state ownership, query behavior, business retry policy, external brokers, large artifact transport, secret transport, and exactly-once guarantees.

This document is the architectural source of truth for subsequent Event Bus contracts, states, events, errors, and implementation documentation.
