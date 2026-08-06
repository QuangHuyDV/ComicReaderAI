# Event Bus Errors

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Event Bus  
> **Document:** Errors and Warnings  
> **Path:** `03-infrastructure/event-bus/ERRORS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/event-bus/MODULE.md`
> - `03-infrastructure/event-bus/CONTRACT.md`
> - `03-infrastructure/event-bus/STATES.md`
> - `03-infrastructure/event-bus/EVENTS.md`
> - `docs/architecture/EVENT_BUS.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RETRY_POLICY.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`

---

## 1. Purpose

This document defines normalized errors and warnings owned by the Event Bus infrastructure module.

It covers:

- event-envelope validation errors;
- event-type registry errors;
- publisher authorization errors;
- namespace ownership errors;
- subscription registration errors;
- subscriber clearance errors;
- event-version compatibility errors;
- queue admission errors;
- queue overflow and backpressure errors;
- ordering-lane errors;
- publication timeout and rejection errors;
- routing errors;
- handler execution errors;
- delivery timeout, cancellation, and abandonment errors;
- subscriber-health errors;
- drain and shutdown errors;
- event-loop and causation-depth errors;
- payload-safety and security errors;
- diagnostics and observability errors;
- future outbox and durable-adapter errors;
- warnings and partial outcomes;
- retry and recovery guidance;
- cross-module normalization.

This document does not define:

- domain-specific event errors;
- business-operation retry policy;
- module-owned state-transition errors;
- raw subscriber exceptions;
- raw broker exceptions;
- UI wording;
- alert thresholds;
- persistence schema;
- exact retry schedules.

---

## 2. Error Design Goals

Event Bus errors must:

1. remain payload-safe;
2. never include raw event payloads by default;
3. distinguish publication failure from delivery failure;
4. distinguish queue rejection from handler failure;
5. distinguish timeout from cancellation;
6. distinguish logical abandonment from physical handler completion;
7. distinguish unsupported version from malformed event;
8. distinguish unauthorized publisher from unauthorized subscriber;
9. distinguish normal backpressure from fatal dispatcher failure;
10. preserve event, subscription, lane, and delivery identity;
11. support bounded recovery guidance;
12. avoid retrying unsafe or non-idempotent handlers blindly;
13. isolate subscriber failures;
14. keep publisher state authoritative;
15. prevent security-policy downgrade;
16. preserve at-most-once MVP semantics;
17. support future durable-adapter normalization.

---

## 3. Error Versus Warning

An error prevents safe admission, routing, delivery, or lifecycle completion.

A warning describes degraded but still functioning behavior.

Examples:

```text
Queue capacity near threshold
    → warning

Queue full and publication rejected
    → error

Subscriber slow but still handling
    → warning

Subscriber disabled after repeated failures
    → error

Durable adapter unavailable while in-memory bus still works
    → warning or error depending on policy
```

---

## 4. Error Versus Event Outcome

These outcomes are not automatically errors:

```text
Publication accepted without subscribers
Event filtered
Delivery ignored as stale
Delivery duplicate
Subscription paused
Progress event coalesced
Progress event dropped under policy
Drain partially completed
```

They become errors only when a caller contract or mandatory policy requires stronger behavior.

---

## 5. Error Versus Cancellation

Cancellation is expected control flow when explicitly requested.

```text
Handler cooperatively canceled during shutdown
    → cancellation outcome

Handler ignored cancellation and drain timed out
    → timeout / abandonment error

Publication wait canceled before queue admission
    → cancellation, not internal failure
```

---

## 6. Error Ownership

The Event Bus owns normalized errors related to:

- event admission;
- registry;
- publisher authorization;
- subscriber registration;
- routing;
- queue capacity;
- ordering lanes;
- dispatcher;
- delivery attempts;
- handler timeout;
- handler isolation;
- drain;
- shutdown;
- payload inspection;
- self-event reporting;
- durable adapters.

The Event Bus does not own the semantic error returned by a domain handler.

A handler-provided failure is wrapped as a delivery failure while preserving the normalized safe code.

---

## 7. Canonical Error Model

```text
EventBusError {
    errorId

    code
    category
    scope
    severity

    retryClass
    recoverability
    userActionRequired

    safeMessage
    developerMessage?

    recoveryActions[]
    retryAfter?

    affectedEventId?
    affectedEventType?
    affectedEventVersion?

    publisherId?
    subscriberId?
    subscriptionId?
    deliveryId?
    laneId?
    dispatcherId?
    registryId?
    drainId?
    outboxRecordId?
    durableAdapterId?

    correlationId
    causationId?
    applicationInstanceId

    occurredAt

    cause?
    metadata
}
```

---

## 8. Error Categories

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
SERIALIZATION
EVENT_LOOP
DIAGNOSTICS
OUTBOX
DURABLE_ADAPTER
CONFIGURATION
LIFECYCLE
CONCURRENCY
INTERNAL
```

---

## 9. Error Scopes

```text
EVENT
EVENT_TYPE
REGISTRY
PUBLISHER
SUBSCRIBER
SUBSCRIPTION
DELIVERY
ORDERING_LANE
QUEUE
DISPATCHER
DRAIN_OPERATION
EVENT_BUS
OUTBOX_RECORD
DURABLE_ADAPTER
APPLICATION_INSTANCE
```

---

## 10. Severity

```text
TRACE
NOTICE
WARNING
ERROR
CRITICAL
FATAL
```

### NOTICE

Expected rejection or caller correction.

### WARNING

Degraded behavior with safe continuation.

### ERROR

Operation failed.

### CRITICAL

Security, ordering, lifecycle, or critical-lane invariant failure.

### FATAL

The bus cannot safely validate, route, or isolate events.

---

## 11. Retry Class

```text
NEVER
IMMEDIATE
TRANSIENT
AFTER_CAPACITY_RECOVERY
AFTER_CONFIGURATION_CHANGE
AFTER_SUBSCRIBER_RECOVERY
AFTER_RESTART
IDEMPOTENT_ONLY
AFTER_RECONCILIATION
UNKNOWN
```

---

## 12. Recoverability

```text
AUTOMATIC
CALLER_CORRECTION
CONFIGURATION_CHANGE
SUBSCRIBER_RECOVERY
APPLICATION_RESTART
DURABLE_ADAPTER_RECOVERY
ADMIN_ACTION
NOT_RECOVERABLE
UNKNOWN
```

---

## 13. Recovery Actions

```text
RETRY_PUBLICATION
WAIT_AND_RETRY
REDUCE_EVENT_RATE
COALESCE_PROGRESS
REDUCE_PAYLOAD_SIZE
REGISTER_EVENT_TYPE
UPDATE_EVENT_VERSION
FIX_EVENT_ENVELOPE
USE_AUTHORIZED_PUBLISHER
USE_AUTHORIZED_SUBSCRIBER
UPDATE_SUBSCRIBER_CLEARANCE
FIX_ORDERING_KEY
INCREASE_QUEUE_CAPACITY
PAUSE_SUBSCRIBER
DISABLE_SUBSCRIBER
RESTART_SUBSCRIBER
RESTART_EVENT_BUS
CHECK_EVENT_REGISTRY
CHECK_DURABLE_ADAPTER
RECONCILE_OUTBOX
CONTACT_SUPPORT
NONE
```

---

## 14. Error Code Naming

Canonical format:

```text
EVENT_BUS_<CONCERN>_<CONDITION>
```

Examples:

```text
EVENT_BUS_ENVELOPE_INVALID
EVENT_BUS_QUEUE_CAPACITY_EXCEEDED
EVENT_BUS_DELIVERY_TIMED_OUT
EVENT_BUS_UNAUTHORIZED_PUBLISHER
```

Warnings use:

```text
EVENT_BUS_WARNING_<CONDITION>
```

Security errors use:

```text
EVENT_BUS_SECURITY_<CONDITION>
```

---

# Part I — Envelope and Event Type Errors

## 15. EVENT_BUS_ENVELOPE_INVALID

The event envelope is malformed.

```text
category: ENVELOPE
scope: EVENT
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Examples:

- missing event type;
- invalid version;
- missing correlation ID;
- invalid timestamps;
- invalid visibility;
- invalid priority.

---

## 16. EVENT_BUS_EVENT_TYPE_MISSING

```text
category: EVENT_TYPE
scope: EVENT
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 17. EVENT_BUS_EVENT_TYPE_UNKNOWN

The event type is not registered.

```text
category: EVENT_TYPE
scope: EVENT_TYPE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 18. EVENT_BUS_EVENT_TYPE_DISABLED

```text
category: EVENT_TYPE
scope: EVENT_TYPE
severity: NOTICE
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 19. EVENT_BUS_EVENT_TYPE_OWNERSHIP_CONFLICT

The event namespace is registered to another module.

```text
category: SECURITY
scope: EVENT_TYPE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 20. EVENT_BUS_EVENT_VERSION_INVALID

```text
category: VERSION
scope: EVENT
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 21. EVENT_BUS_EVENT_VERSION_UNSUPPORTED

No compatible subscriber or upcaster supports the event version.

```text
category: VERSION
scope: EVENT
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 22. EVENT_BUS_EVENT_VERSION_CONFLICT

A registration attempts to redefine an existing version incompatibly.

```text
category: REGISTRY
scope: EVENT_TYPE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 23. EVENT_BUS_EVENT_CATEGORY_INVALID

```text
category: ENVELOPE
scope: EVENT
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 24. EVENT_BUS_EVENT_PRIORITY_INVALID

```text
category: ENVELOPE
scope: EVENT
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 25. EVENT_BUS_EVENT_VISIBILITY_INVALID

```text
category: SECURITY
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 26. EVENT_BUS_EVENT_SECURITY_CLASSIFICATION_INVALID

```text
category: SECURITY
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

# Part II — Registry Errors

## 27. EVENT_BUS_REGISTRY_NOT_INITIALIZED

```text
category: REGISTRY
scope: REGISTRY
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 28. EVENT_BUS_REGISTRY_INVALID

```text
category: REGISTRY
scope: REGISTRY
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Normal publication must stop.

---

## 29. EVENT_BUS_REGISTRY_OWNERSHIP_CONFLICT

```text
category: REGISTRY
scope: REGISTRY
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 30. EVENT_BUS_REGISTRY_DUPLICATE_DESCRIPTOR

```text
category: REGISTRY
scope: EVENT_TYPE
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 31. EVENT_BUS_REGISTRY_PAYLOAD_TYPE_UNSAFE

```text
category: PAYLOAD_SAFETY
scope: EVENT_TYPE
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 32. EVENT_BUS_REGISTRY_ORDERING_REQUIREMENT_INVALID

```text
category: ORDERING
scope: EVENT_TYPE
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 33. EVENT_BUS_REGISTRY_COALESCING_POLICY_INVALID

```text
category: REGISTRY
scope: EVENT_TYPE
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

Example:

- completion event configured as coalescible.

---

## 34. EVENT_BUS_REGISTRY_UPCASTER_MISSING

```text
category: VERSION
scope: REGISTRY
severity: WARNING or ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 35. EVENT_BUS_REGISTRY_SEALED

A runtime mutation was attempted after sealing.

```text
category: REGISTRY
scope: REGISTRY
severity: NOTICE
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part III — Publisher Authorization Errors

## 36. EVENT_BUS_PUBLISHER_IDENTITY_MISSING

```text
category: PUBLISHER_AUTHORIZATION
scope: PUBLISHER
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 37. EVENT_BUS_UNAUTHORIZED_PUBLISHER

```text
category: SECURITY
scope: PUBLISHER
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 38. EVENT_BUS_NAMESPACE_OWNERSHIP_VIOLATION

```text
category: SECURITY
scope: PUBLISHER
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 39. EVENT_BUS_PUBLISHER_CATEGORY_NOT_ALLOWED

```text
category: PUBLISHER_AUTHORIZATION
scope: PUBLISHER
severity: ERROR
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 40. EVENT_BUS_PUBLISHER_VISIBILITY_NOT_ALLOWED

```text
category: SECURITY
scope: PUBLISHER
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 41. EVENT_BUS_PUBLISHER_SECURITY_CLASSIFICATION_NOT_ALLOWED

```text
category: SECURITY
scope: PUBLISHER
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

# Part IV — Subscription Errors

## 42. EVENT_BUS_SUBSCRIPTION_INVALID

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 43. EVENT_BUS_SUBSCRIBER_IDENTITY_MISSING

```text
category: SUBSCRIPTION
scope: SUBSCRIBER
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 44. EVENT_BUS_UNAUTHORIZED_SUBSCRIBER

```text
category: SECURITY
scope: SUBSCRIBER
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 45. EVENT_BUS_SUBSCRIBER_CLEARANCE_INSUFFICIENT

```text
category: SUBSCRIBER_AUTHORIZATION
scope: SUBSCRIBER
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 46. EVENT_BUS_SUBSCRIPTION_EVENT_TYPE_UNKNOWN

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 47. EVENT_BUS_SUBSCRIPTION_VERSION_UNSUPPORTED

```text
category: VERSION
scope: SUBSCRIPTION
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 48. EVENT_BUS_SUBSCRIPTION_HANDLER_TYPE_MISMATCH

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 49. EVENT_BUS_SUBSCRIPTION_TIMEOUT_INVALID

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: NOTICE
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 50. EVENT_BUS_SUBSCRIPTION_CONCURRENCY_UNBOUNDED

```text
category: SECURITY
scope: SUBSCRIPTION
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 51. EVENT_BUS_SUBSCRIPTION_FILTER_UNSAFE

```text
category: SECURITY
scope: SUBSCRIPTION
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 52. EVENT_BUS_SUBSCRIPTION_DUPLICATE

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 53. EVENT_BUS_SUBSCRIPTION_NOT_FOUND

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 54. EVENT_BUS_SUBSCRIPTION_PAUSED

A delivery was attempted against a paused subscription.

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: NOTICE
retryClass: AFTER_SUBSCRIBER_RECOVERY
recoverability: SUBSCRIBER_RECOVERY
```

---

## 55. EVENT_BUS_SUBSCRIPTION_DISABLED

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: ERROR
retryClass: AFTER_SUBSCRIBER_RECOVERY
recoverability: SUBSCRIBER_RECOVERY
```

---

## 56. EVENT_BUS_SUBSCRIPTION_DISPOSED

```text
category: SUBSCRIPTION
scope: SUBSCRIPTION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

# Part V — Ordering Errors

## 57. EVENT_BUS_ORDERING_KEY_REQUIRED

```text
category: ORDERING
scope: EVENT
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 58. EVENT_BUS_ORDERING_KEY_INVALID

```text
category: ORDERING
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 59. EVENT_BUS_ORDERING_KEY_UNSAFE

The ordering key contains sensitive or prohibited data.

```text
category: SECURITY
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 60. EVENT_BUS_ORDERING_MODE_CONFLICT

```text
category: ORDERING
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 61. EVENT_BUS_ORDERING_LANE_NOT_FOUND

```text
category: ORDERING
scope: ORDERING_LANE
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 62. EVENT_BUS_ORDERING_LANE_CLOSED

```text
category: ORDERING
scope: ORDERING_LANE
severity: NOTICE
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

A new lane may be created when policy permits.

---

## 63. EVENT_BUS_ORDERING_LANE_FAILED

```text
category: ORDERING
scope: ORDERING_LANE
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 64. EVENT_BUS_ORDERING_INVARIANT_BROKEN

```text
category: INTERNAL
scope: ORDERING_LANE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part VI — Queue and Backpressure Errors

## 65. EVENT_BUS_QUEUE_CAPACITY_EXCEEDED

```text
category: QUEUE
scope: QUEUE
severity: ERROR
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 66. EVENT_BUS_QUEUE_ADMISSION_TIMED_OUT

```text
category: QUEUE
scope: EVENT
severity: ERROR
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 67. EVENT_BUS_QUEUE_BACKPRESSURED

```text
category: BACKPRESSURE
scope: QUEUE
severity: WARNING
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 68. EVENT_BUS_EVENT_DROPPED_BY_OVERFLOW_POLICY

```text
category: QUEUE
scope: EVENT
severity: WARNING or ERROR
retryClass: conditional
recoverability: AUTOMATIC or CALLER_CORRECTION
```

Severity depends on category.

---

## 69. EVENT_BUS_PROGRESS_EVENT_DROPPED

```text
category: QUEUE
scope: EVENT
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 70. EVENT_BUS_EVENT_COALESCING_FAILED

```text
category: QUEUE
scope: EVENT
severity: WARNING
retryClass: NEVER
recoverability: AUTOMATIC
```

The event may proceed without coalescing.

---

## 71. EVENT_BUS_CRITICAL_RESERVE_EXHAUSTED

```text
category: QUEUE
scope: EVENT_BUS
severity: CRITICAL
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: APPLICATION_RESTART or AUTOMATIC
```

---

## 72. EVENT_BUS_QUEUE_MEMORY_LIMIT_EXCEEDED

```text
category: QUEUE
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 73. EVENT_BUS_QUEUE_ITEM_EXPIRED

```text
category: QUEUE
scope: EVENT
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

# Part VII — Publication Errors

## 74. EVENT_BUS_PUBLICATION_REJECTED

Generic safe publication rejection.

```text
category: PUBLICATION
scope: EVENT
severity: ERROR
retryClass: conditional
recoverability: depends on cause
```

---

## 75. EVENT_BUS_PUBLICATION_BUS_NOT_RUNNING

```text
category: LIFECYCLE
scope: EVENT_BUS
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 76. EVENT_BUS_PUBLICATION_BUS_QUIESCING

```text
category: LIFECYCLE
scope: EVENT_BUS
severity: NOTICE
retryClass: NEVER for normal work
recoverability: NONE
```

---

## 77. EVENT_BUS_PUBLICATION_NO_SUBSCRIBERS

Used only when at least one subscriber is required.

```text
category: PUBLICATION
scope: EVENT
severity: WARNING or ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 78. EVENT_BUS_PUBLICATION_PAYLOAD_TOO_LARGE

```text
category: PAYLOAD_SAFETY
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

Recovery:

```text
Use artifact reference or reduce payload.
```

---

## 79. EVENT_BUS_PUBLICATION_CANCELED

```text
category: PUBLICATION
scope: EVENT
severity: NOTICE
retryClass: conditional
recoverability: CALLER_CORRECTION
```

---

# Part VIII — Routing Errors

## 80. EVENT_BUS_ROUTING_FAILED

```text
category: ROUTING
scope: EVENT
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 81. EVENT_BUS_ROUTING_NO_ELIGIBLE_SUBSCRIBERS

```text
category: ROUTING
scope: EVENT
severity: NOTICE or ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 82. EVENT_BUS_ROUTING_UNAUTHORIZED_SUBSCRIBER_EXCLUDED

```text
category: SECURITY
scope: SUBSCRIBER
severity: WARNING
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 83. EVENT_BUS_ROUTING_VERSION_MISMATCH

```text
category: VERSION
scope: SUBSCRIPTION
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 84. EVENT_BUS_ROUTING_FILTER_FAILED

```text
category: ROUTING
scope: SUBSCRIPTION
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

Unsafe filter exceptions must be normalized.

---

# Part IX — Delivery and Handler Errors

## 85. EVENT_BUS_DELIVERY_SCHEDULING_FAILED

```text
category: DELIVERY
scope: DELIVERY
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 86. EVENT_BUS_HANDLER_START_FAILED

```text
category: HANDLER
scope: DELIVERY
severity: ERROR
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 87. EVENT_BUS_HANDLER_FAILED

The subscriber handler returned or threw a normalized failure.

```text
category: HANDLER
scope: DELIVERY
severity: ERROR
retryClass: conditional
recoverability: SUBSCRIBER_RECOVERY
```

---

## 88. EVENT_BUS_HANDLER_EXCEPTION_UNSAFE

The handler threw an exception containing potentially sensitive data.

```text
category: SECURITY
scope: DELIVERY
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The raw exception must not cross the bus boundary.

---

## 89. EVENT_BUS_DELIVERY_TIMED_OUT

```text
category: DELIVERY
scope: DELIVERY
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: SUBSCRIBER_RECOVERY
```

---

## 90. EVENT_BUS_DELIVERY_CANCELED

```text
category: DELIVERY
scope: DELIVERY
severity: NOTICE
retryClass: conditional
recoverability: NONE or SUBSCRIBER_RECOVERY
```

---

## 91. EVENT_BUS_DELIVERY_ABANDONED

The bus stopped waiting for handler termination.

```text
category: DELIVERY
scope: DELIVERY
severity: WARNING or ERROR
retryClass: NEVER for same attempt
recoverability: SUBSCRIBER_RECOVERY
```

---

## 92. EVENT_BUS_DELIVERY_LATE_COMPLETION_IGNORED

```text
category: DELIVERY
scope: DELIVERY
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 93. EVENT_BUS_DELIVERY_DUPLICATE

```text
category: DELIVERY
scope: DELIVERY
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 94. EVENT_BUS_DELIVERY_STALE

```text
category: DELIVERY
scope: DELIVERY
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 95. EVENT_BUS_DELIVERY_UNSUPPORTED_VERSION

```text
category: VERSION
scope: DELIVERY
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 96. EVENT_BUS_DELIVERY_RETRY_NOT_IDEMPOTENT

A retry was requested for a handler not declared idempotent.

```text
category: SECURITY
scope: DELIVERY
severity: CRITICAL
retryClass: NEVER
recoverability: CONFIGURATION_CHANGE
```

---

## 97. EVENT_BUS_DELIVERY_RETRY_EXHAUSTED

```text
category: DELIVERY
scope: DELIVERY
severity: ERROR
retryClass: NEVER
recoverability: SUBSCRIBER_RECOVERY
```

---

# Part X — Subscriber Health and Dispatcher Errors

## 98. EVENT_BUS_SUBSCRIBER_SLOW

```text
category: SUBSCRIBER_HEALTH
scope: SUBSCRIBER
severity: WARNING
retryClass: AFTER_SUBSCRIBER_RECOVERY
recoverability: SUBSCRIBER_RECOVERY
```

---

## 99. EVENT_BUS_SUBSCRIBER_FAILURE_THRESHOLD_EXCEEDED

```text
category: SUBSCRIBER_HEALTH
scope: SUBSCRIBER
severity: ERROR
retryClass: AFTER_SUBSCRIBER_RECOVERY
recoverability: SUBSCRIBER_RECOVERY
```

---

## 100. EVENT_BUS_SUBSCRIBER_CIRCUIT_OPEN

```text
category: SUBSCRIBER_HEALTH
scope: SUBSCRIBER
severity: WARNING
retryClass: AFTER_SUBSCRIBER_RECOVERY
recoverability: SUBSCRIBER_RECOVERY
```

---

## 101. EVENT_BUS_MANDATORY_SUBSCRIBER_DISABLED

```text
category: SUBSCRIBER_HEALTH
scope: EVENT_BUS
severity: CRITICAL
retryClass: AFTER_SUBSCRIBER_RECOVERY
recoverability: SUBSCRIBER_RECOVERY
```

---

## 102. EVENT_BUS_DISPATCHER_NOT_RUNNING

```text
category: DISPATCHER
scope: DISPATCHER
severity: ERROR
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

## 103. EVENT_BUS_DISPATCHER_BACKPRESSURED

```text
category: DISPATCHER
scope: DISPATCHER
severity: WARNING
retryClass: AFTER_CAPACITY_RECOVERY
recoverability: AUTOMATIC
```

---

## 104. EVENT_BUS_DISPATCHER_FAILED

```text
category: DISPATCHER
scope: DISPATCHER
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 105. EVENT_BUS_DISPATCHER_ORDERING_AT_RISK

```text
category: ORDERING
scope: DISPATCHER
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part XI — Drain and Shutdown Errors

## 106. EVENT_BUS_DRAIN_INVALID

```text
category: DRAIN
scope: DRAIN_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 107. EVENT_BUS_DRAIN_ALREADY_RUNNING

```text
category: DRAIN
scope: DRAIN_OPERATION
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 108. EVENT_BUS_DRAIN_TIMED_OUT

```text
category: DRAIN
scope: DRAIN_OPERATION
severity: WARNING or ERROR
retryClass: NEVER
recoverability: NONE
```

---

## 109. EVENT_BUS_DRAIN_PARTIALLY_COMPLETED

```text
category: DRAIN
scope: DRAIN_OPERATION
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 110. EVENT_BUS_DRAIN_HANDLER_ABANDONED

```text
category: DRAIN
scope: DELIVERY
severity: WARNING
retryClass: NEVER
recoverability: NONE
```

---

## 111. EVENT_BUS_SHUTDOWN_IN_PROGRESS

```text
category: SHUTDOWN
scope: EVENT_BUS
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 112. EVENT_BUS_SHUTDOWN_FAILED

```text
category: SHUTDOWN
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 113. EVENT_BUS_TERMINATION_FAILED

```text
category: SHUTDOWN
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

# Part XII — Security and Payload Safety Errors

## 114. EVENT_BUS_UNSAFE_PAYLOAD_BLOCKED

```text
category: PAYLOAD_SAFETY
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 115. EVENT_BUS_SECRET_PAYLOAD_BLOCKED

A secret-bearing type or value was detected.

```text
category: SECURITY
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 116. EVENT_BUS_AUTHORIZATION_HEADER_BLOCKED

```text
category: SECURITY
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 117. EVENT_BUS_PRIVATE_KEY_BLOCKED

```text
category: SECURITY
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 118. EVENT_BUS_RAW_USER_CONTENT_BLOCKED

```text
category: PAYLOAD_SAFETY
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 119. EVENT_BUS_LARGE_BINARY_BLOCKED

```text
category: PAYLOAD_SAFETY
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 120. EVENT_BUS_UNSAFE_METADATA_BLOCKED

```text
category: SECURITY
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 121. EVENT_BUS_UNSAFE_SERIALIZATION_BLOCKED

```text
category: SERIALIZATION
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 122. EVENT_BUS_RESTRICTED_ROUTING_VIOLATION_BLOCKED

```text
category: SECURITY
scope: SUBSCRIPTION
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 123. EVENT_BUS_EVENT_PAYLOAD_LOGGING_BLOCKED

```text
category: SECURITY
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part XIII — Event Loop Errors

## 124. EVENT_BUS_EVENT_LOOP_DETECTED

```text
category: EVENT_LOOP
scope: EVENT
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 125. EVENT_BUS_MAXIMUM_CAUSATION_DEPTH_EXCEEDED

```text
category: EVENT_LOOP
scope: EVENT
severity: ERROR
retryClass: NEVER
recoverability: CALLER_CORRECTION
```

---

## 126. EVENT_BUS_REENTRANT_DISPATCH_BLOCKED

The implementation attempted recursive same-stack dispatch.

```text
category: EVENT_LOOP
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The event should be enqueued instead.

---

## 127. EVENT_BUS_SELF_EVENT_RECURSION_BLOCKED

```text
category: EVENT_LOOP
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part XIV — Diagnostics Errors

## 128. EVENT_BUS_DIAGNOSTIC_BUFFER_FULL

```text
category: DIAGNOSTICS
scope: EVENT_BUS
severity: WARNING
retryClass: NEVER
recoverability: AUTOMATIC
```

Old safe records may be evicted.

---

## 129. EVENT_BUS_DIAGNOSTIC_RECORD_UNSAFE

```text
category: SECURITY
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

The record must be blocked.

---

## 130. EVENT_BUS_METRICS_EXPORT_FAILED

```text
category: DIAGNOSTICS
scope: EVENT_BUS
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

Event delivery continues.

---

## 131. EVENT_BUS_TRACE_EXPORT_FAILED

```text
category: DIAGNOSTICS
scope: EVENT_BUS
severity: WARNING
retryClass: TRANSIENT
recoverability: AUTOMATIC
```

---

## 132. EVENT_BUS_SELF_EVENT_REPORTING_FAILED

```text
category: DIAGNOSTICS
scope: EVENT_BUS
severity: WARNING or CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Severity depends on whether a critical security fact could not be reported.

---

# Part XV — Configuration Errors

## 133. EVENT_BUS_CONFIGURATION_INVALID

```text
category: CONFIGURATION
scope: EVENT_BUS
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 134. EVENT_BUS_QUEUE_CAPACITY_INVALID

```text
category: CONFIGURATION
scope: QUEUE
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 135. EVENT_BUS_HANDLER_TIMEOUT_INVALID

```text
category: CONFIGURATION
scope: SUBSCRIPTION
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 136. EVENT_BUS_CRITICAL_RESERVE_INVALID

```text
category: CONFIGURATION
scope: QUEUE
severity: CRITICAL
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 137. EVENT_BUS_TRANSPORT_CHANGE_REQUIRES_RESTART

```text
category: CONFIGURATION
scope: EVENT_BUS
severity: NOTICE
retryClass: AFTER_RESTART
recoverability: APPLICATION_RESTART
```

---

# Part XVI — Outbox and Durable Adapter Errors

## 138. EVENT_BUS_DURABLE_ADAPTER_NOT_REGISTERED

```text
category: DURABLE_ADAPTER
scope: DURABLE_ADAPTER
severity: ERROR
retryClass: AFTER_CONFIGURATION_CHANGE
recoverability: CONFIGURATION_CHANGE
```

---

## 139. EVENT_BUS_DURABLE_ADAPTER_INITIALIZATION_FAILED

```text
category: DURABLE_ADAPTER
scope: DURABLE_ADAPTER
severity: ERROR
retryClass: TRANSIENT
recoverability: DURABLE_ADAPTER_RECOVERY
```

---

## 140. EVENT_BUS_DURABLE_ADAPTER_UNAVAILABLE

```text
category: DURABLE_ADAPTER
scope: DURABLE_ADAPTER
severity: WARNING or ERROR
retryClass: TRANSIENT
recoverability: DURABLE_ADAPTER_RECOVERY
```

---

## 141. EVENT_BUS_DURABLE_PUBLICATION_FAILED

```text
category: DURABLE_ADAPTER
scope: EVENT
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: DURABLE_ADAPTER_RECOVERY
```

---

## 142. EVENT_BUS_DURABLE_ACKNOWLEDGMENT_FAILED

```text
category: DURABLE_ADAPTER
scope: DELIVERY
severity: ERROR
retryClass: AFTER_RECONCILIATION
recoverability: DURABLE_ADAPTER_RECOVERY
```

Duplicates may occur.

---

## 143. EVENT_BUS_OUTBOX_RECORD_NOT_FOUND

```text
category: OUTBOX
scope: OUTBOX_RECORD
severity: NOTICE
retryClass: NEVER
recoverability: NONE
```

---

## 144. EVENT_BUS_OUTBOX_PERSIST_FAILED

```text
category: OUTBOX
scope: OUTBOX_RECORD
severity: CRITICAL
retryClass: IDEMPOTENT_ONLY
recoverability: DURABLE_ADAPTER_RECOVERY
```

State and outbox atomicity may be at risk.

---

## 145. EVENT_BUS_OUTBOX_RETRY_EXHAUSTED

```text
category: OUTBOX
scope: OUTBOX_RECORD
severity: ERROR
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

## 146. EVENT_BUS_OUTBOX_DEAD_LETTERED

```text
category: OUTBOX
scope: OUTBOX_RECORD
severity: WARNING or ERROR
retryClass: NEVER
recoverability: ADMIN_ACTION
```

---

# Part XVII — Internal Invariant Errors

## 147. EVENT_BUS_INVALID_STATE_TRANSITION

```text
category: INTERNAL
scope: EVENT_BUS
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 148. EVENT_BUS_STATE_VERSION_CONFLICT

```text
category: CONCURRENCY
scope: EVENT_BUS
severity: ERROR
retryClass: IDEMPOTENT_ONLY
recoverability: AUTOMATIC
```

---

## 149. EVENT_BUS_DELIVERY_TERMINAL_STATE_CONFLICT

Two terminal outcomes raced.

```text
category: CONCURRENCY
scope: DELIVERY
severity: CRITICAL
retryClass: NEVER
recoverability: ADMIN_ACTION
```

Only one terminal state may win.

---

## 150. EVENT_BUS_QUEUE_DEPTH_INVARIANT_BROKEN

```text
category: INTERNAL
scope: QUEUE
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 151. EVENT_BUS_SUBSCRIPTION_STATE_CORRUPTED

```text
category: INTERNAL
scope: SUBSCRIPTION
severity: CRITICAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

---

## 152. EVENT_BUS_FATAL_SAFETY_INVARIANT_BROKEN

```text
category: INTERNAL
scope: EVENT_BUS
severity: FATAL
retryClass: NEVER
recoverability: APPLICATION_RESTART
```

Use only when payload safety, routing authorization, or ordering isolation cannot be trusted.

---

# Part XVIII — Warnings

## 153. Warning Model

```text
EventBusWarning {
    warningId
    code
    scope
    safeMessage
    recoveryActions[]
    metadata
}
```

Warnings are bounded, payload-free, and do not schedule retry.

---

## 154. EVENT_BUS_WARNING_QUEUE_PRESSURE

Queue utilization is high but admission still works.

---

## 155. EVENT_BUS_WARNING_SUBSCRIBER_SLOW

A handler is slower than policy but still completing.

---

## 156. EVENT_BUS_WARNING_SUBSCRIBER_DEGRADED

A subscriber remains usable under reduced concurrency or policy.

---

## 157. EVENT_BUS_WARNING_EVENT_COALESCED

A progress or observability event was safely coalesced.

---

## 158. EVENT_BUS_WARNING_PROGRESS_EVENT_DROPPED

A non-authoritative progress event was dropped under policy.

---

## 159. EVENT_BUS_WARNING_NO_SUBSCRIBERS

An event had no subscribers, but none were required.

---

## 160. EVENT_BUS_WARNING_DRAIN_PARTIAL

Shutdown completed with optional work dropped or abandoned.

---

## 161. EVENT_BUS_WARNING_DURABLE_ADAPTER_UNAVAILABLE

The in-memory bus remains available, but durable delivery is unavailable.

---

## 162. EVENT_BUS_WARNING_DIAGNOSTIC_RECORD_EVICTED

A bounded diagnostic record was removed due to capacity or age.

---

# Part XIX — Retry and Recovery Rules

## 163. Errors Do Not Retry Themselves

```text
Error normalized
    ↓
Current lifecycle and policy checked
    ↓
Retry coordinator or caller evaluates
    ↓
New publication or delivery attempt
```

---

## 164. Safe Publication Retry

Potentially safe when:

- the original event ID is preserved;
- the publisher's committed state remains unchanged;
- the event contract permits duplicate handling;
- the previous publication was definitely rejected before admission.

Examples:

- queue capacity rejection;
- transient bus startup race;
- durable adapter unavailable before publish.

---

## 165. Unsafe Blind Retry

Do not blindly retry when:

- admission outcome is unknown;
- durable adapter may have accepted the event;
- handler side effect may have occurred;
- timeout occurred and physical execution may continue;
- acknowledgment failed;
- outbox publication status is uncertain.

These require idempotency or reconciliation.

---

## 166. Handler Retry

Handler retry is allowed only when:

- configured;
- bounded;
- handler declared idempotent;
- failure classified transient;
- same-lane ordering remains preserved.

---

## 167. Backpressure Recovery

Recommended order:

```text
coalesce progress
    ↓
drop old progress
    ↓
reduce low-priority rate
    ↓
bounded publisher wait
    ↓
reject new publication
```

Critical security facts use reserved capacity and escalation.

---

# Part XX — State Transition Implications

## 168. Registry Invalid

```text
Registry → INVALID
Bus → FAILED or DEGRADED
Normal publication blocked
```

---

## 169. Unauthorized Publisher

```text
Publication → REJECTED
No queue item created
Restricted security event emitted
```

---

## 170. Queue Capacity Exceeded

```text
Publication → REJECTED / DROPPED / COALESCED
Lane → BACKPRESSURED
Bus may remain RUNNING or DEGRADED
```

---

## 171. Handler Timeout

```text
Delivery → TIMED_OUT
Cancellation requested
Subscription health may degrade
Late completion becomes non-authoritative
```

---

## 172. Subscriber Failure Threshold

```text
Health → FAILING / DEGRADED / CIRCUIT_OPEN
Subscription → DEGRADED / PAUSED / DISABLED
```

---

## 173. Dispatcher Failure

```text
Dispatcher → FAILED
Affected lanes → FAILED or closed
Bus → DEGRADED or FAILED
```

---

## 174. Critical Reserve Exhaustion

```text
Bus → DEGRADED or FAILED
Normal publication may be blocked
Critical safe diagnostics emitted through fallback path
```

---

## 175. Drain Timeout

```text
Drain → TIMED_OUT or PARTIALLY_DRAINED
Remaining deliveries → CANCELED / ABANDONED
Bus continues to STOPPING
```

---

# Part XXI — Cross-Module Normalization

## 176. Publisher Mapping

Publishers may receive:

```text
ENVELOPE_INVALID
EVENT_TYPE_UNKNOWN
UNAUTHORIZED_PUBLISHER
ORDERING_KEY_REQUIRED
QUEUE_CAPACITY_EXCEEDED
PUBLICATION_BUS_QUIESCING
PAYLOAD_TOO_LARGE
UNSAFE_PAYLOAD_BLOCKED
```

Publishers must not receive subscriber raw exceptions.

---

## 177. Subscriber Mapping

Subscribers may observe:

```text
SUBSCRIPTION_DISABLED
DELIVERY_TIMED_OUT
DELIVERY_CANCELED
DELIVERY_ABANDONED
DELIVERY_UNSUPPORTED_VERSION
```

A subscriber's domain failure remains owned by that subscriber module.

---

## 178. Runtime Mapping

Runtime may interpret:

```text
QUEUE_CAPACITY_EXCEEDED
    → infrastructure backpressure

DELIVERY_TIMED_OUT
    → subscriber infrastructure timeout

BUS_QUIESCING
    → no new event-based coordination

DISPATCHER_FAILED
    → application infrastructure degradation
```

Runtime must not assume Event Bus errors represent domain-operation failure.

---

## 179. Presentation Mapping

Potential user-facing impact:

```text
QUEUE_PRESSURE
    → usually hidden

MANDATORY_SUBSCRIBER_DISABLED
    → degraded feature warning

EVENT_BUS_FAILED
    → application blocking error

DRAIN_PARTIAL
    → shutdown diagnostics only
```

---

# Part XXII — Logging and Observability

## 180. Logging Policy

### TRACE

- duplicate delivery;
- stale delivery;
- no-subscriber optional event;
- progress coalesced.

### INFO

- subscriber recovered;
- lane recovered;
- bus recovered;
- drain completed.

### WARNING

- queue pressure;
- subscriber slow;
- progress dropped;
- drain partial;
- durable adapter unavailable.

### ERROR

- publication rejected;
- routing failed;
- delivery failed;
- handler timeout;
- subscription disabled.

### CRITICAL

- unauthorized publisher;
- unsafe payload;
- restricted routing violation;
- dispatcher ordering risk;
- registry invalid.

### FATAL

- payload safety or queue invariants cannot be enforced.

---

## 181. Log Fields

Allowed:

```text
errorCode
category
severity
affectedEventId
affectedEventType
publisherId
subscriberId
subscriptionId
deliveryId
laneId
dispatcherId
correlationId
retryClass
recoverability
```

Prohibited:

```text
original payload
secret values
user content
raw exception
provider response
authorization data
unsafe metadata
```

---

## 182. Metrics

Recommended metrics:

```text
event_bus_errors_total
event_bus_warnings_total
event_bus_publication_errors_total
event_bus_delivery_errors_total
event_bus_handler_timeouts_total
event_bus_subscriber_disabled_total
event_bus_queue_capacity_errors_total
event_bus_security_blocks_total
event_bus_drain_errors_total
event_bus_durable_adapter_errors_total
event_bus_fatal_total
```

Labels:

```text
code
category
scope
severity
eventCategory
subscriberModule
```

Avoid event IDs and lane IDs as metric labels.

---

## 183. Tracing

Trace spans may contain:

- normalized error code;
- publication stage;
- routing stage;
- delivery stage;
- subscriber module;
- lane class;
- retry class;
- timeout duration.

No payload content is allowed.

---

# Part XXIII — Testing Requirements

## 184. Envelope Tests

- missing event type;
- invalid version;
- invalid visibility;
- missing correlation;
- invalid timestamps;
- payload too large.

---

## 185. Registry Tests

- duplicate descriptor;
- ownership conflict;
- unsafe payload type;
- invalid ordering rule;
- invalid coalescing rule;
- sealed mutation.

---

## 186. Authorization Tests

- unauthorized publisher;
- namespace mismatch;
- unauthorized subscriber;
- insufficient clearance;
- restricted route blocked.

---

## 187. Queue Tests

- capacity exceeded;
- bounded wait timeout;
- progress dropped;
- progress coalesced;
- critical reserve used;
- critical reserve exhausted;
- memory limit.

---

## 188. Ordering Tests

- required key missing;
- unsafe key;
- lane failed;
- same-key order preserved;
- terminal delivery race.

---

## 189. Delivery Tests

- handler failed;
- unsafe exception;
- timeout;
- cancellation;
- abandonment;
- late completion;
- retry not idempotent;
- retry exhausted.

---

## 190. Drain Tests

- already running;
- partial drain;
- timeout;
- handler abandoned;
- termination failure.

---

## 191. Security Tests

- secret payload;
- authorization header;
- private key;
- raw user content;
- unsafe metadata;
- payload logging;
- event loop;
- self-event recursion.

---

## 192. Durable Tests

- adapter unavailable;
- acknowledgment failed;
- outbox persist failed;
- retry exhausted;
- dead-lettered.

---

# Part XXIV — MVP Error Boundary

## 193. Required MVP Codes

The MVP should implement at least:

```text
EVENT_BUS_ENVELOPE_INVALID
EVENT_BUS_EVENT_TYPE_UNKNOWN
EVENT_BUS_EVENT_VERSION_UNSUPPORTED
EVENT_BUS_REGISTRY_INVALID

EVENT_BUS_UNAUTHORIZED_PUBLISHER
EVENT_BUS_UNAUTHORIZED_SUBSCRIBER
EVENT_BUS_NAMESPACE_OWNERSHIP_VIOLATION
EVENT_BUS_SUBSCRIBER_CLEARANCE_INSUFFICIENT

EVENT_BUS_ORDERING_KEY_REQUIRED
EVENT_BUS_ORDERING_KEY_INVALID
EVENT_BUS_ORDERING_LANE_FAILED

EVENT_BUS_QUEUE_CAPACITY_EXCEEDED
EVENT_BUS_QUEUE_ADMISSION_TIMED_OUT
EVENT_BUS_QUEUE_BACKPRESSURED
EVENT_BUS_EVENT_DROPPED_BY_OVERFLOW_POLICY
EVENT_BUS_CRITICAL_RESERVE_EXHAUSTED

EVENT_BUS_PUBLICATION_BUS_NOT_RUNNING
EVENT_BUS_PUBLICATION_BUS_QUIESCING
EVENT_BUS_PUBLICATION_PAYLOAD_TOO_LARGE

EVENT_BUS_ROUTING_FAILED
EVENT_BUS_ROUTING_VERSION_MISMATCH

EVENT_BUS_HANDLER_FAILED
EVENT_BUS_HANDLER_EXCEPTION_UNSAFE
EVENT_BUS_DELIVERY_TIMED_OUT
EVENT_BUS_DELIVERY_CANCELED
EVENT_BUS_DELIVERY_ABANDONED
EVENT_BUS_DELIVERY_RETRY_NOT_IDEMPOTENT

EVENT_BUS_SUBSCRIBER_SLOW
EVENT_BUS_SUBSCRIBER_FAILURE_THRESHOLD_EXCEEDED
EVENT_BUS_MANDATORY_SUBSCRIBER_DISABLED

EVENT_BUS_DISPATCHER_FAILED
EVENT_BUS_DISPATCHER_ORDERING_AT_RISK

EVENT_BUS_DRAIN_TIMED_OUT
EVENT_BUS_DRAIN_PARTIALLY_COMPLETED

EVENT_BUS_UNSAFE_PAYLOAD_BLOCKED
EVENT_BUS_SECRET_PAYLOAD_BLOCKED
EVENT_BUS_UNSAFE_METADATA_BLOCKED
EVENT_BUS_RESTRICTED_ROUTING_VIOLATION_BLOCKED
EVENT_BUS_EVENT_LOOP_DETECTED
EVENT_BUS_SELF_EVENT_RECURSION_BLOCKED

EVENT_BUS_INVALID_STATE_TRANSITION
EVENT_BUS_DELIVERY_TERMINAL_STATE_CONFLICT
EVENT_BUS_FATAL_SAFETY_INVARIANT_BROKEN
```

---

## 194. Required MVP Warnings

```text
EVENT_BUS_WARNING_QUEUE_PRESSURE
EVENT_BUS_WARNING_SUBSCRIBER_SLOW
EVENT_BUS_WARNING_SUBSCRIBER_DEGRADED
EVENT_BUS_WARNING_EVENT_COALESCED
EVENT_BUS_WARNING_PROGRESS_EVENT_DROPPED
EVENT_BUS_WARNING_NO_SUBSCRIBERS
EVENT_BUS_WARNING_DRAIN_PARTIAL
```

---

# Part XXV — Decisions

## 195. Decisions

### Decision 1 — No original payload in errors

Errors carry event identity and safe metadata only.

### Decision 2 — Publication and delivery errors remain separate

Admission failure does not equal subscriber failure.

### Decision 3 — Handler errors are normalized

Raw exceptions never cross the Event Bus boundary.

### Decision 4 — Backpressure is not automatically fatal

Queue pressure may degrade safely before rejection.

### Decision 5 — Security errors fail closed

Unauthorized routing and unsafe payloads are blocked before delivery.

### Decision 6 — Timeout is terminal

Late completion cannot overwrite it.

### Decision 7 — Retry requires idempotency

Handler retry is never implicit.

### Decision 8 — Drain errors do not block bounded shutdown forever

Timeout leads to cancellation or abandonment.

### Decision 9 — At-most-once semantics remain explicit

Publication retries must consider admission certainty.

### Decision 10 — Durable errors are separate

Future adapter failures do not redefine in-memory bus semantics.

---

# Part XXVI — Open Decisions

## 196. Retry Decisions

Still to finalize:

- default publication retry guidance;
- exact one-immediate-retry policy;
- retryable handler categories;
- backpressure wait duration;
- durable acknowledgment reconciliation.

---

## 197. Severity Decisions

Still to finalize:

- when no-subscriber is warning versus error;
- when progress drop is metrics-only;
- mandatory subscriber disable severity;
- drain partial severity;
- durable adapter unavailable severity.

---

## 198. Security Decisions

Still to finalize:

- event-loop threshold;
- causation-depth limit;
- restricted error sink;
- security-error retention;
- payload-inspector false-positive handling.

---

## 199. User Mapping Decisions

Still to finalize:

- application-level messaging for Event Bus failure;
- mandatory subscriber failure UX;
- whether shutdown warnings are shown;
- diagnostics export wording.

---

# Part XXVII — Related Documents

## 200. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/event-bus/MODULE.md
03-infrastructure/event-bus/CONTRACT.md
03-infrastructure/event-bus/STATES.md
03-infrastructure/event-bus/EVENTS.md
```

Future document:

```text
03-infrastructure/event-bus/README.md
```

---

## 201. Summary

Event Bus errors normalize envelope, registry, authorization, subscription, ordering, queue, publication, routing, delivery, dispatcher, shutdown, security, diagnostics, and durable-adapter failures without exposing event payloads.

The error flow is:

```text
Raw infrastructure or handler failure
    ↓
Event Bus boundary catches failure
    ↓
Payload and exception data removed
    ↓
Normalized EventBusError created
    ↓
Lifecycle and authority validated
    ↓
State transition applied where appropriate
    ↓
Safe logging, metrics, tracing, and self-event
```

The model preserves these distinctions:

```text
Publication Error
    ≠ Delivery Error
    ≠ Handler Error
    ≠ Queue Pressure
    ≠ Cancellation
    ≠ Timeout
    ≠ Abandonment
```

The architecture guarantees:

- errors never include original event payloads;
- publication and delivery failures remain separate;
- subscriber failures are isolated;
- unsafe payloads fail before queue admission;
- unauthorized publisher and subscriber actions fail closed;
- timeout is terminal;
- late completion is non-authoritative;
- handler retry requires idempotency;
- drain remains bounded;
- backpressure remains observable;
- in-memory and durable delivery errors remain distinct;
- publisher state remains authoritative.

This document is the error source of truth for the Event Bus implementation and README.
