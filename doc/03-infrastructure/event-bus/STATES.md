# Event Bus States

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Event Bus  
> **Document:** State Machines  
> **Path:** `03-infrastructure/event-bus/STATES.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/event-bus/MODULE.md`
> - `03-infrastructure/event-bus/CONTRACT.md`
> - `docs/architecture/EVENT_BUS.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`

---

## 1. Purpose

This document defines lifecycle states and valid transitions owned by the Event Bus infrastructure module.

It covers:

- Event Bus lifecycle;
- event registry lifecycle;
- subscription lifecycle;
- subscriber health lifecycle;
- ordering-lane lifecycle;
- queue-item lifecycle;
- publication lifecycle;
- routing-decision lifecycle;
- delivery-attempt lifecycle;
- dispatcher lifecycle;
- progress-coalescing lifecycle;
- drain lifecycle;
- shutdown lifecycle;
- diagnostic failure-buffer lifecycle;
- future outbox lifecycle;
- future durable-adapter lifecycle;
- cross-state invariants;
- concurrency and crash-recovery behavior;
- invalid transitions.

This document does not define:

- event payload schemas;
- publisher and subscriber method signatures;
- Event Bus self-event payloads;
- normalized error codes;
- queue implementation classes;
- thread primitives;
- concrete broker technology;
- business workflow states;
- module-owned domain states.

---

## 2. State Ownership

The Event Bus owns lifecycle state for:

```text
EventBus
EventRegistry
EventTypeRegistration
Subscription
SubscriberHealth
OrderingLane
QueueItem
Publication
RoutingDecision
DeliveryAttempt
Dispatcher
ProgressCoalescingEntry
DrainOperation
DiagnosticFailureRecord
OutboxRecord
DurableAdapter
```

The Event Bus does not own:

```text
ConfigurationSnapshot
SecretDescriptor
ProviderDefinition
ProviderLease
TranslationJob
RecognitionJob
RuntimeWorkItem
ReadingSession
ApplicationCommand
DomainAggregate
UIState
```

A delivered event may cause another module to evaluate a transition.

The receiving module remains the only owner of that transition.

---

## 3. State-Machine Separation

The Event Bus must not use one global state enumeration.

Independent state machines are required:

```text
EventBusState
EventRegistryState
EventTypeRegistrationState
SubscriptionState
SubscriberHealthState
OrderingLaneState
QueueItemState
PublicationState
RoutingDecisionState
DeliveryAttemptState
DispatcherState
ProgressCoalescingState
DrainState
DiagnosticFailureRecordState
OutboxRecordState
DurableAdapterState
```

This separation is necessary because:

- the bus may be `RUNNING` while one subscriber is `DEGRADED`;
- a subscription may be `PAUSED` while its subscriber health is `HEALTHY`;
- a queue lane may be `DRAINING` while the bus still accepts critical events;
- publication may be `ACCEPTED` while one delivery later `FAILED`;
- one delivery may be `TIMED_OUT` while other deliveries are `HANDLED`;
- the registry may be `SEALED` while subscriptions still change;
- a durable adapter may be `UNAVAILABLE` while the in-memory bus remains `RUNNING`;
- a progress item may be `COALESCED` without being dispatched;
- a drain operation may time out while the bus still reaches `TERMINATED`.

---

## 4. State Principles

### 4.1 State represents accepted current truth

```text
State
    = current lifecycle condition

Event
    = immutable fact that a transition occurred
```

### 4.2 Publication and delivery are distinct

```text
Publication ACCEPTED
    ≠
Subscriber HANDLED
```

### 4.3 Subscriber failure does not roll back publication

Once the event is accepted into the bus, downstream handler failure does not mutate the publisher's committed state.

### 4.4 Ordering is lane-scoped

Ordering state belongs to one lane and one subscription delivery sequence.

There is no global event order.

### 4.5 Queues are bounded

Queue-item state must account for rejection, dropping, and coalescing.

### 4.6 Shutdown is explicit

The bus moves through quiescing and drain states rather than terminating immediately under normal conditions.

### 4.7 Terminal states do not reactivate

Terminal queue items, publications, deliveries, and drain operations do not return to active states.

### 4.8 Security transitions take priority

Unsafe payload, unauthorized routing, and compromised inspection paths must fail closed.

---

# Part I — Event Bus Lifecycle

## 5. EventBusState

Canonical states:

```text
CREATED
INITIALIZING
READY
RUNNING
DEGRADED
QUIESCING
DRAINING
STOPPING
TERMINATED
FAILED
```

Primary lifecycle:

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

Failure and recovery paths:

```text
INITIALIZING → FAILED
RUNNING ↔ DEGRADED
DEGRADED → QUIESCING
RUNNING → FAILED
FAILED → STOPPING
```

---

## 6. CREATED

The Event Bus instance exists but has not initialized:

- registry;
- queue topology;
- security policy;
- dispatcher;
- subscription infrastructure;
- diagnostics;
- optional durable adapter.

Valid outgoing transitions:

```text
CREATED → INITIALIZING
CREATED → TERMINATED
```

No normal publication or subscription activation is permitted.

---

## 7. INITIALIZING

The Event Bus is validating and assembling:

- event registry;
- publisher authorization;
- subscriber authorization;
- queue capacities;
- ordering-lane factory;
- dispatcher;
- payload inspector;
- diagnostics;
- optional durable adapter.

Valid outgoing transitions:

```text
INITIALIZING → READY
INITIALIZING → DEGRADED
INITIALIZING → FAILED
INITIALIZING → STOPPING
```

`INITIALIZING → DEGRADED` is allowed only when optional infrastructure failed but safe core delivery remains available.

---

## 8. READY

The bus is initialized but has not begun normal publication.

At this point:

- registry is valid;
- mandatory subscriptions are checked;
- dispatcher may be prepared but not accepting normal work;
- startup events may be admitted under explicit policy.

Valid outgoing transitions:

```text
READY → RUNNING
READY → DEGRADED
READY → STOPPING
READY → FAILED
```

---

## 9. RUNNING

The bus accepts normal publications and subscriptions according to policy.

Properties:

- queues active;
- dispatchers active;
- security inspection active;
- ordering lanes created as needed;
- health monitored;
- diagnostics available.

Valid outgoing transitions:

```text
RUNNING → DEGRADED
RUNNING → QUIESCING
RUNNING → FAILED
```

---

## 10. DEGRADED

The bus remains partially operational but one or more non-fatal components are unhealthy.

Possible causes:

- optional subscriber disabled;
- observability lane unavailable;
- durable adapter unavailable;
- queue pressure;
- high timeout rate;
- diagnostic buffer unavailable;
- non-critical registry extension failed;
- progress delivery throttled aggressively.

Properties:

- security inspection must remain safe;
- critical and normal publication may continue under policy;
- degraded capabilities are explicit;
- no unsafe silent fallback.

Valid outgoing transitions:

```text
DEGRADED → RUNNING
DEGRADED → QUIESCING
DEGRADED → FAILED
```

---

## 11. QUIESCING

The bus stops accepting ordinary new work.

Typical policy:

```text
reject:
    DOMAIN
    INTEGRATION
    RESULT
    PROGRESS
    OBSERVABILITY

allow:
    SYSTEM
    SECURITY
    AUDIT
```

Actual category policy is supplied by `QuiesceRequest`.

Valid outgoing transitions:

```text
QUIESCING → DRAINING
QUIESCING → STOPPING
QUIESCING → FAILED
```

Returning to `RUNNING` is permitted only for an explicitly reversible administrative quiesce, not ordinary shutdown.

---

## 12. DRAINING

The bus processes accepted queued items within a bounded deadline.

Properties:

- no ordinary new publications;
- critical shutdown events may remain admissible;
- lane ordering preserved;
- progress may be dropped;
- active handlers receive shutdown cancellation context;
- queue depth decreases toward zero.

Valid outgoing transitions:

```text
DRAINING → STOPPING
DRAINING → FAILED
```

Drain timeout does not leave the bus permanently in `DRAINING`.

Remaining work becomes dropped, canceled, or abandoned according to policy.

---

## 13. STOPPING

The bus is releasing infrastructure:

- dispatchers stop;
- subscriptions dispose;
- lanes close;
- diagnostics finalize;
- durable adapter stops;
- internal buffers clear.

Valid outgoing transitions:

```text
STOPPING → TERMINATED
STOPPING → FAILED
```

No new publication or subscription is accepted.

---

## 14. TERMINATED

The bus instance is no longer usable.

Properties:

- publication rejected;
- subscription rejected;
- handlers not invoked;
- queues closed;
- terminal status query may remain available.

`TERMINATED` is terminal.

A new application instance requires a new Event Bus instance.

---

## 15. FAILED

The bus cannot maintain its required safety or delivery invariants.

Examples:

- security inspection unavailable and cannot fail closed;
- registry corruption;
- dispatcher invariant failure;
- critical queue unusable;
- unauthorized routing cannot be prevented;
- lifecycle state corrupted.

Required behavior:

- reject normal publication;
- isolate unsafe components;
- begin stop sequence;
- expose critical safe diagnostics.

Valid outgoing transitions:

```text
FAILED → STOPPING
FAILED → TERMINATED
```

Direct `FAILED → RUNNING` is prohibited.

A new initialization or application restart is required.

---

## 16. Event Bus Transition Table

| Current | Condition | Next |
|---|---|---|
| `CREATED` | initialize | `INITIALIZING` |
| `INITIALIZING` | core ready | `READY` |
| `INITIALIZING` | optional failure | `DEGRADED` |
| `INITIALIZING` | fatal failure | `FAILED` |
| `READY` | start | `RUNNING` |
| `RUNNING` | recoverable degradation | `DEGRADED` |
| `DEGRADED` | recovered | `RUNNING` |
| `RUNNING` | quiesce | `QUIESCING` |
| `DEGRADED` | quiesce | `QUIESCING` |
| `QUIESCING` | drain accepted | `DRAINING` |
| `DRAINING` | deadline reached or empty | `STOPPING` |
| `STOPPING` | cleanup complete | `TERMINATED` |
| Any active state | fatal invariant failure | `FAILED` |

---

# Part II — Event Registry Lifecycle

## 17. EventRegistryState

Canonical states:

```text
EMPTY
BUILDING
VALIDATING
READY
SEALED
UPDATING
DEGRADED
INVALID
DISPOSED
```

---

## 18. EMPTY

No event types are registered.

Valid outgoing transition:

```text
EMPTY → BUILDING
```

---

## 19. BUILDING

Composition Root or startup code registers descriptors.

Valid outgoing transitions:

```text
BUILDING → VALIDATING
BUILDING → INVALID
```

---

## 20. VALIDATING

Registry checks:

- namespace ownership;
- version conflicts;
- payload safety;
- ordering requirements;
- coalescing rules;
- publisher authorization;
- subscriber clearances;
- duplicate definitions.

Valid outgoing transitions:

```text
VALIDATING → READY
VALIDATING → DEGRADED
VALIDATING → INVALID
```

---

## 21. READY

The registry is valid and may still accept approved startup registrations.

Valid outgoing transitions:

```text
READY → SEALED
READY → UPDATING
READY → INVALID
```

---

## 22. SEALED

Normal runtime mutation is prohibited.

Properties:

- event descriptors stable;
- version compatibility deterministic;
- ownership cannot change silently;
- publication validation uses a stable snapshot.

Valid outgoing transitions:

```text
SEALED → UPDATING
SEALED → INVALID
SEALED → DISPOSED
```

Runtime update requires explicit administrative mode.

---

## 23. UPDATING

A controlled registry change is being prepared.

Existing publication uses the last committed registry snapshot until the update activates.

Valid outgoing transitions:

```text
UPDATING → SEALED
UPDATING → DEGRADED
UPDATING → INVALID
```

---

## 24. DEGRADED

Optional descriptors or upcasters are unavailable, but the core registry remains safe.

Unsupported types are rejected.

Valid outgoing transitions:

```text
DEGRADED → READY
DEGRADED → SEALED
DEGRADED → INVALID
```

---

## 25. INVALID

The registry cannot safely validate event ownership or schema.

Normal publication must stop.

`INVALID` is terminal for that registry snapshot.

---

## 26. DISPOSED

Registry resources are released.

`DISPOSED` is terminal.

---

# Part III — Event Type Registration Lifecycle

## 27. EventTypeRegistrationState

Canonical states:

```text
PROPOSED
VALIDATING
REGISTERED
ACTIVE
DEPRECATED
DISABLED
REJECTED
REMOVED
```

---

## 28. PROPOSED

A descriptor was supplied but not validated.

---

## 29. VALIDATING

Ownership, payload, version, visibility, and ordering rules are checked.

Valid outgoing transitions:

```text
VALIDATING → REGISTERED
VALIDATING → REJECTED
```

---

## 30. REGISTERED

The descriptor exists but is not yet active for publication.

Valid outgoing transitions:

```text
REGISTERED → ACTIVE
REGISTERED → DISABLED
REGISTERED → REMOVED
```

---

## 31. ACTIVE

The event type may be published and subscribed to.

Valid outgoing transitions:

```text
ACTIVE → DEPRECATED
ACTIVE → DISABLED
```

Runtime removal of an active type is prohibited by default.

---

## 32. DEPRECATED

Publication may remain allowed under migration policy.

New subscribers should prefer the replacement type or version.

Valid outgoing transitions:

```text
DEPRECATED → DISABLED
DEPRECATED → REMOVED
```

---

## 33. DISABLED

Publication is rejected.

Historical descriptor metadata may remain available.

Valid outgoing transitions:

```text
DISABLED → ACTIVE
DISABLED → REMOVED
```

Reactivation requires explicit validation.

---

## 34. REJECTED

The proposal failed validation.

`REJECTED` is terminal for that proposal.

---

## 35. REMOVED

The registration is no longer active.

`REMOVED` is terminal for that registration identity.

---

# Part IV — Subscription Lifecycle

## 36. SubscriptionState

Canonical states:

```text
PROPOSED
VALIDATING
REGISTERED
ACTIVE
PAUSING
PAUSED
RESUMING
DEGRADED
DISABLING
DISABLED
DRAINING
DISPOSING
DISPOSED
REJECTED
```

Primary lifecycle:

```text
PROPOSED
    ↓
VALIDATING
    ↓
REGISTERED
    ↓
ACTIVE
    ↓
DRAINING
    ↓
DISPOSING
    ↓
DISPOSED
```

---

## 37. PROPOSED

A subscription request exists but is not accepted.

---

## 38. VALIDATING

The bus checks:

- subscriber identity;
- event type;
- version support;
- handler compatibility;
- visibility clearance;
- security classification;
- concurrency bounds;
- timeout;
- filter safety;
- mandatory status;
- duplicate subscription policy.

Valid outgoing transitions:

```text
VALIDATING → REGISTERED
VALIDATING → REJECTED
```

---

## 39. REGISTERED

The subscription is accepted but not receiving events.

Valid outgoing transitions:

```text
REGISTERED → ACTIVE
REGISTERED → DISPOSING
REGISTERED → DISABLED
```

---

## 40. ACTIVE

The subscription is eligible for routing and delivery.

Valid outgoing transitions:

```text
ACTIVE → PAUSING
ACTIVE → DEGRADED
ACTIVE → DISABLING
ACTIVE → DRAINING
```

---

## 41. PAUSING

No new delivery should start after the pause barrier.

Existing handlers may finish.

Valid outgoing transitions:

```text
PAUSING → PAUSED
PAUSING → DEGRADED
PAUSING → DISABLING
```

---

## 42. PAUSED

The subscription remains registered but receives no new delivery.

Queued items may:

- remain queued;
- be skipped;
- be rerouted;
- expire;

according to policy.

Valid outgoing transitions:

```text
PAUSED → RESUMING
PAUSED → DISABLING
PAUSED → DRAINING
PAUSED → DISPOSING
```

---

## 43. RESUMING

The bus revalidates:

- clearance;
- event type;
- handler;
- health;
- queue state.

Valid outgoing transitions:

```text
RESUMING → ACTIVE
RESUMING → DEGRADED
RESUMING → DISABLED
```

---

## 44. DEGRADED

The subscription still receives some delivery but is unhealthy.

Possible causes:

- elevated failure rate;
- timeouts;
- reduced concurrency;
- temporary dependency unavailable;
- optional version upcaster unavailable.

Valid outgoing transitions:

```text
DEGRADED → ACTIVE
DEGRADED → PAUSING
DEGRADED → DISABLING
DEGRADED → DRAINING
```

---

## 45. DISABLING

The bus prevents new deliveries and handles active attempts according to policy.

Valid outgoing transitions:

```text
DISABLING → DISABLED
DISABLING → DRAINING
```

---

## 46. DISABLED

The subscription remains known but inactive.

Valid outgoing transitions:

```text
DISABLED → RESUMING
DISABLED → DISPOSING
```

Mandatory disabled subscriptions may degrade Event Bus health.

---

## 47. DRAINING

No new delivery is admitted.

Existing delivery attempts finish within a bounded deadline.

Valid outgoing transitions:

```text
DRAINING → DISPOSING
DRAINING → DISABLED
```

---

## 48. DISPOSING

The subscription releases:

- handler references;
- queue references;
- diagnostic state;
- filters;
- cancellation scopes.

Valid outgoing transition:

```text
DISPOSING → DISPOSED
```

---

## 49. DISPOSED

The subscription can no longer receive events.

`DISPOSED` is terminal.

---

## 50. REJECTED

Registration failed.

`REJECTED` is terminal.

---

# Part V — Subscriber Health Lifecycle

## 51. SubscriberHealthState

Canonical states:

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

Health state is derived from delivery observations and policy.

---

## 52. UNKNOWN

Insufficient observations exist.

---

## 53. HEALTHY

Failure and latency remain within policy.

---

## 54. SLOW

Handler latency exceeds warning threshold but delivery remains successful.

Valid outgoing transitions:

```text
SLOW → HEALTHY
SLOW → DEGRADED
SLOW → FAILING
```

---

## 55. FAILING

Recent failures exceed a warning threshold.

Valid outgoing transitions:

```text
FAILING → HEALTHY
FAILING → DEGRADED
FAILING → CIRCUIT_OPEN
FAILING → UNHEALTHY
```

---

## 56. DEGRADED

The subscriber remains usable under reduced concurrency or restricted delivery.

Valid outgoing transitions:

```text
DEGRADED → HEALTHY
DEGRADED → CIRCUIT_OPEN
DEGRADED → UNHEALTHY
```

---

## 57. CIRCUIT_OPEN

Delivery is temporarily blocked.

Valid outgoing transitions:

```text
CIRCUIT_OPEN → RECOVERING
CIRCUIT_OPEN → DISABLED
```

The Event Bus MVP may defer a full circuit-breaker implementation, but the state boundary is preserved.

---

## 58. RECOVERING

Controlled probe delivery is allowed.

Valid outgoing transitions:

```text
RECOVERING → HEALTHY
RECOVERING → CIRCUIT_OPEN
RECOVERING → UNHEALTHY
```

---

## 59. UNHEALTHY

The subscriber cannot safely or reliably handle events.

Valid outgoing transitions:

```text
UNHEALTHY → RECOVERING
UNHEALTHY → DISABLED
```

---

## 60. DISABLED

No delivery is allowed.

Health reactivation requires administrative or policy-driven recovery.

---

# Part VI — Ordering Lane Lifecycle

## 61. OrderingLaneState

Canonical states:

```text
CREATED
IDLE
ACTIVE
BACKPRESSURED
PAUSED
DRAINING
CLOSING
CLOSED
FAILED
```

---

## 62. CREATED

Lane identity and capacity exist.

Valid outgoing transition:

```text
CREATED → IDLE
```

---

## 63. IDLE

The lane has no queued or active delivery.

Valid outgoing transitions:

```text
IDLE → ACTIVE
IDLE → CLOSING
IDLE → PAUSED
```

Idle lanes may expire after policy-defined duration.

---

## 64. ACTIVE

The lane contains queued or active items.

Valid outgoing transitions:

```text
ACTIVE → IDLE
ACTIVE → BACKPRESSURED
ACTIVE → PAUSED
ACTIVE → DRAINING
ACTIVE → FAILED
```

---

## 65. BACKPRESSURED

Capacity or consumer throughput is under pressure.

Possible effects:

- bounded publisher wait;
- admission rejection;
- progress coalescing;
- low-priority drop;
- warning;
- scheduler pressure.

Valid outgoing transitions:

```text
BACKPRESSURED → ACTIVE
BACKPRESSURED → DRAINING
BACKPRESSURED → FAILED
```

---

## 66. PAUSED

No dispatch begins.

Queued items remain according to policy.

Valid outgoing transitions:

```text
PAUSED → ACTIVE
PAUSED → DRAINING
PAUSED → CLOSING
```

---

## 67. DRAINING

The lane accepts no normal new items and processes current items in order.

Valid outgoing transitions:

```text
DRAINING → CLOSING
DRAINING → FAILED
```

---

## 68. CLOSING

Remaining lane resources are released.

Valid outgoing transition:

```text
CLOSING → CLOSED
```

---

## 69. CLOSED

The lane cannot be reused.

`CLOSED` is terminal.

A later event with the same ordering key creates a new lane identity if policy permits.

---

## 70. FAILED

Lane ordering or queue invariants cannot be maintained.

Remaining items must be:

- rejected;
- dropped;
- rerouted only if ordering semantics remain safe;
- escalated.

`FAILED` is terminal for the lane instance.

---

# Part VII — Publication Lifecycle

## 71. PublicationState

Canonical states:

```text
CREATED
VALIDATING
INSPECTING_SECURITY
RESOLVING_LANE
ADMITTING
ACCEPTED
ACCEPTED_NO_SUBSCRIBERS
COALESCED
FILTERED
REJECTED
DROPPED
TIMED_OUT
```

---

## 72. CREATED

A `PublishRequest` exists.

---

## 73. VALIDATING

The bus validates:

- publisher;
- event type;
- version;
- envelope;
- registry;
- ordering metadata;
- lifecycle admission.

Valid outgoing transitions:

```text
VALIDATING → INSPECTING_SECURITY
VALIDATING → REJECTED
```

---

## 74. INSPECTING_SECURITY

Payload safety and routing classification are checked.

Valid outgoing transitions:

```text
INSPECTING_SECURITY → RESOLVING_LANE
INSPECTING_SECURITY → REJECTED
```

---

## 75. RESOLVING_LANE

The bus selects routing and ordering lanes.

Valid outgoing transitions:

```text
RESOLVING_LANE → ADMITTING
RESOLVING_LANE → FILTERED
RESOLVING_LANE → REJECTED
```

---

## 76. ADMITTING

The queue attempts bounded admission.

Valid outgoing transitions:

```text
ADMITTING → ACCEPTED
ADMITTING → ACCEPTED_NO_SUBSCRIBERS
ADMITTING → COALESCED
ADMITTING → DROPPED
ADMITTING → TIMED_OUT
ADMITTING → REJECTED
```

---

## 77. ACCEPTED

The event entered the queue.

`ACCEPTED` is terminal for publication state.

Delivery continues in separate delivery state machines.

---

## 78. ACCEPTED_NO_SUBSCRIBERS

The event passed validation but no active subscriber matched.

Whether this is successful depends on `requireAtLeastOneSubscriber`.

This state is terminal.

---

## 79. COALESCED

The incoming event was safely merged into or replaced another queued progress event.

It will not have an independent delivery.

`COALESCED` is terminal.

---

## 80. FILTERED

Policy determined that no route should be created.

`FILTERED` is terminal.

---

## 81. REJECTED

Publication failed before successful queue admission.

`REJECTED` is terminal.

---

## 82. DROPPED

Overflow policy intentionally discarded the event.

`DROPPED` is terminal.

Terminal domain, result, security, and audit facts should not normally reach this state.

---

## 83. TIMED_OUT

Bounded queue admission did not complete before its deadline.

`TIMED_OUT` is terminal.

---

# Part VIII — Queue Item Lifecycle

## 84. QueueItemState

Canonical states:

```text
QUEUED
WAITING_FOR_ORDER
READY
DISPATCHING
DISPATCHED
COALESCED
DROPPED
EXPIRED
CANCELED
ABANDONED
```

---

## 85. QUEUED

The item is stored in a bounded lane.

---

## 86. WAITING_FOR_ORDER

Earlier same-lane items prevent dispatch.

Valid outgoing transitions:

```text
WAITING_FOR_ORDER → READY
WAITING_FOR_ORDER → COALESCED
WAITING_FOR_ORDER → DROPPED
WAITING_FOR_ORDER → EXPIRED
WAITING_FOR_ORDER → CANCELED
```

---

## 87. READY

Ordering and dispatch conditions permit processing.

Valid outgoing transitions:

```text
READY → DISPATCHING
READY → DROPPED
READY → CANCELED
```

---

## 88. DISPATCHING

The bus resolves active subscriptions and creates delivery attempts.

Valid outgoing transitions:

```text
DISPATCHING → DISPATCHED
DISPATCHING → ABANDONED
```

---

## 89. DISPATCHED

All planned delivery attempts were created or conclusively skipped.

`DISPATCHED` is terminal for the queue item.

Handler completion is represented separately.

---

## 90. COALESCED

The item was replaced or merged before dispatch.

`COALESCED` is terminal.

---

## 91. DROPPED

The item was discarded by overflow or shutdown policy.

`DROPPED` is terminal.

---

## 92. EXPIRED

The item exceeded a queue-age or relevance policy.

`EXPIRED` is terminal.

---

## 93. CANCELED

The item was canceled before dispatch under an explicit lifecycle policy.

Ordinary publisher cancellation after admission does not automatically create this transition.

`CANCELED` is terminal.

---

## 94. ABANDONED

The dispatcher stopped waiting for safe dispatch completion.

`ABANDONED` is terminal.

---

# Part IX — Routing Decision Lifecycle

## 95. RoutingDecisionState

Canonical states:

```text
CREATED
MATCHING
AUTHORIZING
VERSION_CHECKING
FINALIZED
EMPTY
REJECTED
```

---

## 96. CREATED

Routing begins for one event.

---

## 97. MATCHING

Subscriptions are matched by event type and filters.

---

## 98. AUTHORIZING

Visibility and security clearance are evaluated.

Unauthorized subscriptions are excluded and safely reported.

---

## 99. VERSION_CHECKING

Supported event versions and upcasters are resolved.

---

## 100. FINALIZED

A stable delivery plan exists.

`FINALIZED` is terminal.

---

## 101. EMPTY

No eligible subscription remains.

`EMPTY` is terminal.

---

## 102. REJECTED

Routing could not safely complete.

`REJECTED` is terminal.

---

# Part X — Delivery Attempt Lifecycle

## 103. DeliveryAttemptState

Canonical states:

```text
CREATED
SCHEDULED
WAITING_FOR_LANE
STARTING
RUNNING
RETRY_WAIT
HANDLED
IGNORED_NOT_RELEVANT
IGNORED_STALE
DUPLICATE
REJECTED_UNSUPPORTED_VERSION
FAILED
TIMED_OUT
CANCELED
ABANDONED
```

Primary flow:

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

---

## 104. CREATED

A delivery identity exists for one event and one subscription.

---

## 105. SCHEDULED

The delivery was admitted to the subscription execution policy.

Valid outgoing transitions:

```text
SCHEDULED → WAITING_FOR_LANE
SCHEDULED → STARTING
SCHEDULED → CANCELED
```

---

## 106. WAITING_FOR_LANE

The attempt waits for prior same-key delivery to finish.

Valid outgoing transitions:

```text
WAITING_FOR_LANE → STARTING
WAITING_FOR_LANE → CANCELED
WAITING_FOR_LANE → TIMED_OUT
```

---

## 107. STARTING

Final validation occurs:

- subscription active;
- version supported;
- deadline valid;
- clearance valid;
- handler available.

Valid outgoing transitions:

```text
STARTING → RUNNING
STARTING → REJECTED_UNSUPPORTED_VERSION
STARTING → CANCELED
STARTING → FAILED
```

---

## 108. RUNNING

The handler executes.

Valid outgoing transitions:

```text
RUNNING → HANDLED
RUNNING → IGNORED_NOT_RELEVANT
RUNNING → IGNORED_STALE
RUNNING → DUPLICATE
RUNNING → RETRY_WAIT
RUNNING → FAILED
RUNNING → TIMED_OUT
RUNNING → CANCELED
RUNNING → ABANDONED
```

---

## 109. RETRY_WAIT

A registered bounded local retry policy allows another attempt.

Valid outgoing transitions:

```text
RETRY_WAIT → SCHEDULED
RETRY_WAIT → FAILED
RETRY_WAIT → CANCELED
```

Attempt number increments.

At-most-once publication does not prohibit a configured local handler retry, but retry must be explicit and idempotent.

---

## 110. HANDLED

The subscriber completed its reaction.

`HANDLED` is terminal.

---

## 111. IGNORED_NOT_RELEVANT

The subscriber determined the event did not apply.

This is a valid terminal outcome.

---

## 112. IGNORED_STALE

The subscriber rejected obsolete revision or state context.

This is a valid terminal outcome.

---

## 113. DUPLICATE

The subscriber identified an already processed semantic event.

This is a valid terminal outcome.

---

## 114. REJECTED_UNSUPPORTED_VERSION

The subscriber could not safely process the event version.

This is terminal for that delivery attempt.

---

## 115. FAILED

The handler completed with a normalized failure.

`FAILED` is terminal unless a retry transition occurred before terminalization.

---

## 116. TIMED_OUT

The handler exceeded its deadline.

Logical delivery authority ends.

Physical work may still continue if cancellation is not honored.

`TIMED_OUT` is terminal.

---

## 117. CANCELED

The attempt stopped through cooperative cancellation before a normal outcome.

`CANCELED` is terminal.

---

## 118. ABANDONED

The bus stopped waiting but could not confirm physical handler termination.

`ABANDONED` is terminal.

---

## 119. Delivery Terminal States

```text
HANDLED
IGNORED_NOT_RELEVANT
IGNORED_STALE
DUPLICATE
REJECTED_UNSUPPORTED_VERSION
FAILED
TIMED_OUT
CANCELED
ABANDONED
```

Terminal delivery attempts never restart.

A retry creates or re-enters a non-terminal attempt stage only before terminal state is committed.

---

# Part XI — Dispatcher Lifecycle

## 120. DispatcherState

Canonical states:

```text
CREATED
STARTING
IDLE
DISPATCHING
BACKPRESSURED
PAUSED
DRAINING
STOPPING
STOPPED
FAILED
```

---

## 121. CREATED

Dispatcher resources are allocated but inactive.

---

## 122. STARTING

Worker loops and cancellation contexts initialize.

Valid outgoing transitions:

```text
STARTING → IDLE
STARTING → FAILED
```

---

## 123. IDLE

No dispatchable items exist.

Valid outgoing transitions:

```text
IDLE → DISPATCHING
IDLE → PAUSED
IDLE → DRAINING
IDLE → STOPPING
```

---

## 124. DISPATCHING

The dispatcher selects lanes and starts delivery attempts.

Valid outgoing transitions:

```text
DISPATCHING → IDLE
DISPATCHING → BACKPRESSURED
DISPATCHING → PAUSED
DISPATCHING → DRAINING
DISPATCHING → FAILED
```

---

## 125. BACKPRESSURED

Dispatch throughput cannot keep pace safely.

Valid outgoing transitions:

```text
BACKPRESSURED → DISPATCHING
BACKPRESSURED → DRAINING
BACKPRESSURED → FAILED
```

---

## 126. PAUSED

No new dispatch begins.

Valid outgoing transitions:

```text
PAUSED → IDLE
PAUSED → DRAINING
PAUSED → STOPPING
```

---

## 127. DRAINING

Only accepted work is processed.

Valid outgoing transitions:

```text
DRAINING → STOPPING
DRAINING → FAILED
```

---

## 128. STOPPING

Dispatcher loops cancel and release resources.

Valid outgoing transition:

```text
STOPPING → STOPPED
```

---

## 129. STOPPED

The dispatcher cannot restart.

`STOPPED` is terminal.

---

## 130. FAILED

The dispatcher cannot preserve scheduling or ordering invariants.

`FAILED` is terminal for that dispatcher instance.

The Event Bus normally transitions to `FAILED` or `DEGRADED` depending on scope.

---

# Part XII — Progress Coalescing Lifecycle

## 131. ProgressCoalescingState

Canonical states:

```text
NOT_ELIGIBLE
ELIGIBLE
MATCHING
REPLACED
MERGED
KEPT_SEPARATE
REJECTED
```

---

## 132. NOT_ELIGIBLE

The event type or category forbids coalescing.

Terminal.

---

## 133. ELIGIBLE

The event descriptor allows coalescing.

Valid outgoing transitions:

```text
ELIGIBLE → MATCHING
ELIGIBLE → KEPT_SEPARATE
```

---

## 134. MATCHING

The bus searches the same coalescing scope:

```text
eventType + orderingKey
```

Valid outgoing transitions:

```text
MATCHING → REPLACED
MATCHING → MERGED
MATCHING → KEPT_SEPARATE
MATCHING → REJECTED
```

---

## 135. REPLACED

The queued progress event was replaced by the incoming event.

Terminal.

---

## 136. MERGED

A registered pure coalescer produced a safe merged event.

Terminal.

---

## 137. KEPT_SEPARATE

No safe coalescing occurred.

Terminal.

---

## 138. REJECTED

Coalescing failed safety or semantic validation.

The publication continues under normal queue admission or is rejected according to policy.

Terminal for the coalescing decision.

---

# Part XIII — Drain Lifecycle

## 139. DrainState

Canonical states:

```text
REQUESTED
VALIDATING
QUIESCING
DRAINING_QUEUES
WAITING_FOR_HANDLERS
CANCELING_REMAINDER
FINALIZING
DRAINED
PARTIALLY_DRAINED
TIMED_OUT
FAILED
CANCELED
```

---

## 140. REQUESTED

A drain request exists.

---

## 141. VALIDATING

The bus validates:

- lifecycle state;
- deadline;
- category policy;
- handler cancellation policy;
- priority scope.

Valid outgoing transitions:

```text
VALIDATING → QUIESCING
VALIDATING → FAILED
VALIDATING → CANCELED
```

---

## 142. QUIESCING

Admission policy changes to prevent normal new work.

Valid outgoing transition:

```text
QUIESCING → DRAINING_QUEUES
```

---

## 143. DRAINING_QUEUES

Accepted items are dispatched in bounded order.

Valid outgoing transitions:

```text
DRAINING_QUEUES → WAITING_FOR_HANDLERS
DRAINING_QUEUES → CANCELING_REMAINDER
DRAINING_QUEUES → FAILED
```

---

## 144. WAITING_FOR_HANDLERS

The bus waits for active attempts within the drain deadline.

Valid outgoing transitions:

```text
WAITING_FOR_HANDLERS → FINALIZING
WAITING_FOR_HANDLERS → CANCELING_REMAINDER
WAITING_FOR_HANDLERS → FAILED
```

---

## 145. CANCELING_REMAINDER

Remaining attempts receive cancellation.

Unconfirmed termination may become abandoned.

Valid outgoing transition:

```text
CANCELING_REMAINDER → FINALIZING
```

---

## 146. FINALIZING

Counts and outcomes are computed.

Valid outgoing transitions:

```text
FINALIZING → DRAINED
FINALIZING → PARTIALLY_DRAINED
FINALIZING → TIMED_OUT
FINALIZING → FAILED
```

---

## 147. DRAINED

All required work reached terminal outcomes within policy.

Terminal.

---

## 148. PARTIALLY_DRAINED

Some lower-priority or optional work was dropped, canceled, or abandoned while required drain policy completed.

Terminal.

---

## 149. TIMED_OUT

The drain deadline expired with remaining work.

Terminal.

---

## 150. FAILED

Drain infrastructure failed.

Terminal.

---

## 151. CANCELED

The drain request was canceled before shutdown became irreversible.

Terminal.

Ordinary application shutdown should not cancel drain once stop sequencing begins.

---

# Part XIV — Diagnostic Failure Record Lifecycle

## 152. DiagnosticFailureRecordState

Canonical states:

```text
CREATED
BUFFERED
OBSERVED
EVICTED
CLEARED
```

---

## 153. CREATED

A safe delivery failure summary was produced.

---

## 154. BUFFERED

The record is stored in the bounded diagnostic buffer.

Valid outgoing transitions:

```text
BUFFERED → OBSERVED
BUFFERED → EVICTED
BUFFERED → CLEARED
```

---

## 155. OBSERVED

A diagnostic query accessed the record.

The record may remain buffered until retention expires.

Valid outgoing transitions:

```text
OBSERVED → EVICTED
OBSERVED → CLEARED
```

---

## 156. EVICTED

The record left the bounded buffer due to capacity or age.

Terminal.

---

## 157. CLEARED

The record was explicitly removed during shutdown or diagnostics cleanup.

Terminal.

---

# Part XV — Outbox Record Lifecycle

## 158. OutboxRecordState

Future durable extension states:

```text
CREATED
PENDING
PUBLISHING
PUBLISHED
ACKNOWLEDGED
RETRY_WAIT
FAILED
DEAD_LETTERED
CANCELED
```

---

## 159. CREATED

State and outbox record were committed atomically.

---

## 160. PENDING

The record awaits publication eligibility.

---

## 161. PUBLISHING

A durable adapter is attempting publication.

Valid outgoing transitions:

```text
PUBLISHING → PUBLISHED
PUBLISHING → RETRY_WAIT
PUBLISHING → FAILED
```

---

## 162. PUBLISHED

The adapter accepted the event.

Valid outgoing transitions:

```text
PUBLISHED → ACKNOWLEDGED
PUBLISHED → RETRY_WAIT
```

Acknowledgment semantics depend on adapter mode.

---

## 163. ACKNOWLEDGED

The outbox record may be retired.

Terminal.

---

## 164. RETRY_WAIT

The record waits for a bounded retry schedule.

Valid outgoing transitions:

```text
RETRY_WAIT → PUBLISHING
RETRY_WAIT → DEAD_LETTERED
RETRY_WAIT → CANCELED
```

---

## 165. FAILED

A non-retryable durable publication failure occurred.

Terminal unless an administrative replay creates a new operation.

---

## 166. DEAD_LETTERED

Retry policy was exhausted.

Terminal.

---

## 167. CANCELED

Publication was administratively canceled before delivery.

Terminal.

---

# Part XVI — Durable Adapter Lifecycle

## 168. DurableAdapterState

Future extension states:

```text
UNREGISTERED
REGISTERED
INITIALIZING
AVAILABLE
DEGRADED
UNAVAILABLE
DRAINING
STOPPING
TERMINATED
FAILED
```

The durable adapter state is independent from the in-memory bus.

The bus may remain `RUNNING` or `DEGRADED` while the adapter is `UNAVAILABLE`, depending on whether durable delivery is mandatory.

---

# Part XVII — Cross-State Rules

## 169. Bus and Registry Relationship

| Event Bus | Registry requirement |
|---|---|
| `CREATED` | `EMPTY` or absent |
| `INITIALIZING` | `BUILDING` / `VALIDATING` |
| `READY` | `READY` or `SEALED` |
| `RUNNING` | normally `SEALED` |
| `DEGRADED` | `SEALED` or `DEGRADED` |
| `FAILED` | may be `INVALID` |
| `TERMINATED` | `DISPOSED` |

---

## 170. Bus and Dispatcher Relationship

```text
Bus READY
    → Dispatcher CREATED or STARTING

Bus RUNNING
    → Dispatcher IDLE / DISPATCHING / BACKPRESSURED

Bus QUIESCING
    → Dispatcher DISPATCHING or DRAINING

Bus DRAINING
    → Dispatcher DRAINING

Bus STOPPING
    → Dispatcher STOPPING

Bus TERMINATED
    → Dispatcher STOPPED
```

---

## 171. Bus and Subscription Relationship

When the bus enters `QUIESCING`:

- new subscription registration is rejected by default;
- active subscriptions remain active for drain;
- optional subscriptions may enter `DRAINING`;
- progress-only subscriptions may be disabled early.

When the bus enters `STOPPING`:

- all subscriptions move to `DISPOSING`.

---

## 172. Publication and Delivery Relationship

```text
Publication ACCEPTED
    ↓
Queue item created
    ↓
Routing finalized
    ↓
Zero or more delivery attempts
```

Publication terminal state does not wait for delivery terminal states except in `DELIVERY_SUMMARY` mode.

---

## 173. Queue Item and Delivery Relationship

A queue item becomes `DISPATCHED` when all planned delivery attempts have been created or skipped.

It does not wait for handlers to finish.

---

## 174. Subscription and Delivery Relationship

A delivery may start only when subscription state is:

```text
ACTIVE
DEGRADED
```

A delivery must not start when subscription state is:

```text
PAUSED
DISABLED
DRAINING
DISPOSING
DISPOSED
REJECTED
```

Existing attempts may continue in `DRAINING`.

---

## 175. Subscriber Health and Subscription Relationship

Example policy:

```text
Health HEALTHY
    → Subscription ACTIVE

Health SLOW
    → Subscription ACTIVE or DEGRADED

Health FAILING
    → Subscription DEGRADED

Health CIRCUIT_OPEN
    → Subscription PAUSED or DISABLED

Health UNHEALTHY
    → Subscription DISABLED
```

Health does not directly mutate subscription state without a configured policy action.

---

## 176. Lane and Queue Item Relationship

A queue item can dispatch only when its lane is:

```text
ACTIVE
BACKPRESSURED
DRAINING
```

It cannot dispatch when the lane is:

```text
PAUSED
CLOSING
CLOSED
FAILED
```

---

## 177. Shutdown and Publication Relationship

During `QUIESCING` or `DRAINING`, publication admission depends on category.

Typical allowed states:

```text
SYSTEM
SECURITY
AUDIT
```

Typical rejected states:

```text
DOMAIN
INTEGRATION
PROGRESS
OBSERVABILITY
```

---

# Part XVIII — Invalid Transitions

## 178. Invalid Event Bus Transitions

```text
CREATED → RUNNING
RUNNING → TERMINATED
TERMINATED → RUNNING
FAILED → RUNNING
DRAINING → RUNNING during normal shutdown
```

---

## 179. Invalid Subscription Transitions

```text
PROPOSED → ACTIVE
REJECTED → ACTIVE
DISPOSED → ACTIVE
PAUSED → ACTIVE without RESUMING
DISABLED → ACTIVE without revalidation
```

---

## 180. Invalid Lane Transitions

```text
CREATED → ACTIVE without initialization
CLOSED → ACTIVE
FAILED → ACTIVE
DRAINING → ACTIVE
```

---

## 181. Invalid Publication Transitions

```text
ACCEPTED → REJECTED
COALESCED → ACCEPTED
DROPPED → ACCEPTED
REJECTED → ADMITTING
```

Publication terminal states never reactivate.

---

## 182. Invalid Delivery Transitions

```text
HANDLED → RUNNING
FAILED → RUNNING
TIMED_OUT → HANDLED
ABANDONED → HANDLED
DUPLICATE → RUNNING
CANCELED → SCHEDULED
```

A late physical handler completion after `TIMED_OUT` or `ABANDONED` must be ignored as authoritative delivery state.

---

## 183. Invalid Registry Transitions

```text
INVALID → SEALED
DISPOSED → BUILDING
SEALED → BUILDING without controlled update
```

---

# Part XIX — Concurrency and Authority

## 184. Single Logical Writer

The Event Bus is the single logical writer for:

- bus lifecycle;
- subscription lifecycle;
- lane lifecycle;
- queue-item lifecycle;
- publication outcome;
- delivery-attempt state;
- dispatcher state;
- drain state.

Handlers report results.

They do not mutate delivery state directly.

---

## 185. State Version

Mutable entities should include:

```text
stateVersion
```

A transition validates the expected current version when concurrent control operations are possible.

---

## 186. Subscription Concurrency

Pause, disable, drain, and dispose operations must be serialized per subscription.

A stale resume request must not reactivate a newer disabled state.

---

## 187. Lane Concurrency

Lane assignment and queue mutation must preserve:

- deterministic lane identity;
- queue capacity;
- same-key order;
- terminal item uniqueness;
- bounded memory.

---

## 188. Delivery Completion Race

Possible race:

```text
Handler completes
and timeout fires simultaneously
```

Only one terminal delivery transition may win.

The losing signal is recorded as late or duplicate and must not change terminal state.

---

## 189. Shutdown Race

Publications racing with quiesce are decided by the admission barrier:

```text
accepted before barrier
    → eligible for drain

not accepted before barrier
    → rejected under quiesce policy
```

---

## 190. Subscriber Disable Race

Delivery attempts already in `RUNNING` follow disable policy:

```text
ALLOW_TO_COMPLETE
CANCEL
ABANDON_AFTER_GRACE
```

No new delivery starts after the disable barrier.

---

# Part XX — Persistence and Crash Recovery

## 191. MVP In-Memory Recovery

The MVP Event Bus is non-durable.

After process crash:

- queued events are lost;
- active delivery attempts are lost;
- subscription instances are lost;
- lanes are lost;
- diagnostic failure buffer is lost;
- registry and subscriptions rebuild at startup.

Authoritative module state must already have been committed before publication.

---

## 192. Crash Recovery Rule

The Event Bus must not reconstruct domain truth from incomplete in-memory delivery state.

Consumers query owning modules after restart.

---

## 193. Outbox Recovery

When future outbox mode is enabled:

- pending records reload;
- published-but-unacknowledged records may retry;
- duplicates are possible;
- consumers remain idempotent;
- original event identity is preserved.

---

## 194. Orphaned Handler

After process termination, an in-process handler cannot continue.

For future cross-process adapters, unconfirmed delivery becomes retryable or abandoned according to adapter semantics.

---

# Part XXI — Command-to-State Mapping

## 195. Initialize

```text
EventBus CREATED → INITIALIZING → READY
Registry EMPTY → BUILDING → VALIDATING → READY / SEALED
Dispatcher CREATED → STARTING → IDLE
```

---

## 196. Start

```text
EventBus READY → RUNNING
Dispatcher IDLE / DISPATCHING
Registered subscriptions → ACTIVE
```

---

## 197. Publish

```text
Publication CREATED
    ↓ VALIDATING
    ↓ INSPECTING_SECURITY
    ↓ RESOLVING_LANE
    ↓ ADMITTING
    ↓ ACCEPTED / COALESCED / REJECTED / DROPPED
```

---

## 198. Dispatch

```text
QueueItem QUEUED
    ↓ WAITING_FOR_ORDER
    ↓ READY
    ↓ DISPATCHING
    ↓ DISPATCHED

DeliveryAttempt CREATED
    ↓ SCHEDULED
    ↓ RUNNING
    ↓ terminal outcome
```

---

## 199. Pause Subscription

```text
Subscription ACTIVE
    ↓ PAUSING
    ↓ PAUSED
```

---

## 200. Resume Subscription

```text
Subscription PAUSED / DISABLED
    ↓ RESUMING
    ↓ ACTIVE / DEGRADED / DISABLED
```

---

## 201. Disable Subscription

```text
Subscription ACTIVE / DEGRADED
    ↓ DISABLING
    ↓ DISABLED
```

---

## 202. Dispose Subscription

```text
Subscription any non-terminal
    ↓ DRAINING if required
    ↓ DISPOSING
    ↓ DISPOSED
```

---

## 203. Quiesce and Drain

```text
EventBus RUNNING / DEGRADED
    ↓ QUIESCING
    ↓ DRAINING
    ↓ STOPPING
    ↓ TERMINATED
```

Drain operation:

```text
REQUESTED
    ↓ VALIDATING
    ↓ QUIESCING
    ↓ DRAINING_QUEUES
    ↓ WAITING_FOR_HANDLERS
    ↓ FINALIZING
    ↓ terminal result
```

---

# Part XXII — State Events

## 204. Event Principle

Event Bus self-events report committed transitions such as:

```text
EventBusStarted
EventBusDegraded
EventBusQuiescing
EventBusDrainStarted
EventBusDrainCompleted
SubscriptionActivated
SubscriptionPaused
SubscriptionDisabled
SubscriberHealthChanged
OrderingLaneBackpressured
PublicationRejected
EventDropped
DeliveryTimedOut
DeliveryAbandoned
DispatcherFailed
DurableAdapterUnavailable
```

Detailed payloads belong in `EVENTS.md`.

---

# Part XXIII — Security Invariants

## 205. Unsafe Publication Invariant

An event failing payload security inspection must never reach:

```text
ADMITTING
ACCEPTED
QUEUED
DISPATCHING
```

---

## 206. Unauthorized Subscription Invariant

A subscription without sufficient clearance must never enter `ACTIVE`.

---

## 207. Restricted Routing Invariant

A restricted event must never create a delivery attempt for an unauthorized subscriber.

---

## 208. Terminal Delivery Invariant

One delivery attempt has exactly one terminal state.

---

## 209. Ordering Invariant

For one subscription and ordering key:

```text
Delivery N+1 must not begin before Delivery N terminates
```

when concurrency mode is `SERIAL` or `ORDERED_BY_KEY`.

---

## 210. Queue Bound Invariant

```text
queueDepth <= configuredCapacity
```

Reserved critical capacity is accounted for separately.

---

## 211. Shutdown Bound Invariant

Shutdown drain must have a finite deadline.

No handler can keep the application alive indefinitely.

---

## 212. No State Ownership Transfer

Successful delivery does not grant the Event Bus ownership over subscriber domain state.

---

# Part XXIV — MVP State Boundary

## 213. Required MVP State Machines

The MVP must implement:

```text
EventBusState
EventRegistryState
SubscriptionState
SubscriberHealthState
OrderingLaneState
PublicationState
QueueItemState
RoutingDecisionState
DeliveryAttemptState
DispatcherState
ProgressCoalescingState
DrainState
DiagnosticFailureRecordState
```

The MVP may defer active implementation of:

```text
OutboxRecordState
DurableAdapterState
```

The state contracts remain documented for future extension.

---

## 214. MVP Simplifications

Allowed:

- no durable recovery;
- no replay;
- no dead-letter store;
- no full subscriber circuit breaker;
- no runtime event type removal;
- no distributed lane ownership.

Not allowed:

- unbounded queues;
- global-ordering claim;
- exactly-once claim;
- unsafe payload admission;
- subscriber failure propagation to unrelated subscribers;
- indefinite shutdown wait;
- terminal delivery reactivation.

---

# Part XXV — State Decisions

## 215. Decisions

### Decision 1 — Independent state machines

Bus, registry, subscription, health, lane, publication, queue item, delivery, dispatcher, and drain remain separate.

### Decision 2 — Publication ends at admission

Delivery state is independent.

### Decision 3 — Terminal delivery is final

Late completion cannot overwrite timeout or abandonment.

### Decision 4 — Quiesce precedes drain

Normal shutdown stops admission before waiting for queues.

### Decision 5 — Ordering is lane-scoped

No global state is created for total event order.

### Decision 6 — Subscriber failure is isolated

A subscription can degrade or disable without failing the whole bus unless mandatory policy requires it.

### Decision 7 — Unsafe events fail before queueing

Security inspection precedes admission.

### Decision 8 — In-memory crash loses events

Authoritative state must not depend on the bus.

### Decision 9 — Progress coalescing has its own state

Coalesced progress is not considered independently delivered.

### Decision 10 — Durable state is an extension

Outbox and durable adapter lifecycles do not redefine core event meaning.

### Decision 11 — Shutdown is bounded

Timeout and abandonment are explicit outcomes.

### Decision 12 — State owner remains external

Event delivery never transfers domain-state authority to the bus.

---

# Part XXVI — Open Decisions

## 216. Lifecycle Decisions

Still to finalize:

- whether `READY` is externally observable;
- whether reversible quiesce is supported;
- exact recovery from `DEGRADED`;
- mandatory subscription readiness rules;
- lane idle-expiration state timing;
- registry live-update policy.

---

## 217. Delivery Decisions

Still to finalize:

- default handler timeout;
- timeout versus cancellation precedence;
- one-immediate-retry transition details;
- subscriber circuit-open thresholds;
- whether delivery attempts are reused for retry or recreated;
- abandoned-handler diagnostics.

---

## 218. Queue Decisions

Still to finalize:

- exact backpressure thresholds;
- fairness between lanes;
- starvation prevention;
- critical reserve exhaustion behavior;
- queue-item expiration;
- lane close behavior with queued progress.

---

## 219. Shutdown Decisions

Still to finalize:

- drain category defaults;
- progress drop timing;
- observability flush behavior;
- audit sink shutdown ordering;
- active restricted-security handler grace;
- force-terminate conditions.

---

## 220. Durable Decisions

Still to finalize:

- outbox acknowledgment meaning;
- durable adapter recovery;
- dead-letter lifecycle;
- replay state;
- broker partition ownership;
- cross-process abandonment semantics.

---

# Part XXVII — Documentation Order

## 221. Recommended Order

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

`EVENTS.md` should next define:

- Event Bus lifecycle events;
- registry events;
- subscription lifecycle events;
- subscriber-health events;
- lane backpressure events;
- publication rejection and drop events;
- delivery timeout and failure events;
- drain and shutdown events;
- security-inspection events;
- durable-adapter events.

---

# Part XXVIII — Related Documents

## 222. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/event-bus/MODULE.md
03-infrastructure/event-bus/CONTRACT.md

03-infrastructure/configuration/EVENTS.md
03-infrastructure/secret-management/EVENTS.md
02-modules/provider-management/EVENTS.md
```

Future Event Bus documents:

```text
03-infrastructure/event-bus/EVENTS.md
03-infrastructure/event-bus/ERRORS.md
03-infrastructure/event-bus/README.md
```

---

## 223. Summary

The Event Bus uses separate state machines for its own lifecycle, registry, subscriptions, subscriber health, ordering lanes, publications, queue items, deliveries, dispatcher, coalescing, drain operations, diagnostics, and future durable infrastructure.

The main bus lifecycle is:

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

The main publication lifecycle is:

```text
CREATED
    ↓
VALIDATING
    ↓
INSPECTING_SECURITY
    ↓
RESOLVING_LANE
    ↓
ADMITTING
    ↓
ACCEPTED / COALESCED / REJECTED / DROPPED
```

The main delivery lifecycle is:

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
HANDLED / STALE / DUPLICATE / FAILED / TIMED_OUT / CANCELED / ABANDONED
```

The architecture preserves these invariants:

- publication and delivery are distinct;
- queues are bounded;
- subscriber failures are isolated;
- ordering is scoped;
- unsafe payloads never enter queues;
- terminal delivery states never reactivate;
- late handler completion cannot overwrite timeout or abandonment;
- shutdown is bounded;
- in-memory crash loses unpersisted events;
- authoritative domain state remains outside the Event Bus;
- future durable delivery extends transport without changing event meaning.

This document is the state-machine source of truth for subsequent Event Bus events, errors, and implementation documentation.
