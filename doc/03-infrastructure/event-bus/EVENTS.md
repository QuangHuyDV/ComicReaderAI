# Event Bus Events

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Event Bus  
> **Document:** Integration Events  
> **Path:** `03-infrastructure/event-bus/EVENTS.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/event-bus/MODULE.md`
> - `03-infrastructure/event-bus/CONTRACT.md`
> - `03-infrastructure/event-bus/STATES.md`
> - `docs/architecture/EVENT_BUS.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`

---

## 1. Purpose

This document defines the events published and consumed by the Event Bus infrastructure module itself.

These events communicate safe operational facts concerning:

- Event Bus startup and shutdown;
- registry lifecycle;
- event-type registration;
- subscription lifecycle;
- subscriber health;
- queue pressure;
- ordering-lane lifecycle;
- publication admission;
- event coalescing and dropping;
- delivery outcomes;
- handler timeouts and abandonment;
- drain operations;
- security inspection;
- unauthorized publication or subscription attempts;
- diagnostic-buffer behavior;
- future durable adapter and outbox behavior.

This document does not redefine events owned by Configuration, Secret Management, Provider Management, Runtime, Translation, Recognition, or Presentation.

The Event Bus transports those module-owned events.

It does not become their semantic owner.

---

## 2. Event Principles

### 2.1 Events represent committed facts

Correct:

```text
EventBusStarted
SubscriptionDisabled
EventDropped
DeliveryTimedOut
```

Incorrect:

```text
StartEventBus
DisableSubscription
DropEvent
TimeoutDelivery
```

### 2.2 Events are immutable

Once published, an Event Bus event cannot be changed.

A later correction requires another event.

### 2.3 Self-events must not recurse indefinitely

Event Bus self-events require a protected publication path.

They must not create cycles such as:

```text
Event Bus publishes EventDropped
    ↓
queue overflow drops EventDropped
    ↓
Event Bus publishes EventDropped
    ↓
infinite loop
```

Critical self-events may use:

- reserved internal lane;
- direct observability sink;
- restricted diagnostic channel;
- bounded non-recursive reporting.

### 2.4 Self-events never carry original payloads

An Event Bus event may identify the affected event by:

```text
eventId
eventType
eventVersion
category
sourceModule
priority
visibility
```

It must not copy the original event payload.

### 2.5 State commits before self-event publication

```text
Internal state transition
    ↓
state committed
    ↓
self-event emitted
```

### 2.6 Self-events do not repair failures

They report facts.

Recovery remains owned by lifecycle controls, administration, or the affected module.

---

## 3. Event Visibility

Recommended visibility classes:

```text
PUBLIC_INTERNAL
MODULE_INTERNAL
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
AUDIT_ONLY
LOCAL_COMPONENT_ONLY
```

Most Event Bus self-events should be:

```text
OBSERVABILITY_ONLY
MODULE_INTERNAL
```

Security violations should be:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

Application lifecycle events may be:

```text
PUBLIC_INTERNAL
```

---

## 4. Event Envelope

Event Bus self-events use the shared envelope:

```text
EventEnvelope<TPayload> {
    eventId
    eventType
    eventVersion
    category
    occurredAt
    publishedAt
    sourceModule = "event-bus"
    sourceComponent?
    publisherId
    correlationId
    causationId?
    applicationInstanceId
    entityId?
    ordering
    priority
    visibility
    securityClassification
    payload
    metadata
}
```

---

## 5. Naming Convention

Canonical event type:

```text
event-bus.<entity>.<past-tense-fact>
```

Examples:

```text
event-bus.lifecycle.started
event-bus.subscription.disabled
event-bus.queue.backpressured
event-bus.delivery.timed-out
```

---

# Part I — Event Bus Lifecycle Events

## 6. EventBusInitializationStarted

Event type:

```text
event-bus.lifecycle.initialization-started
```

Payload:

```text
EventBusInitializationStartedPayload {
    applicationInstanceId
    registryExpected
    durableAdapterRequested
    startedAt
}
```

Visibility:

```text
OBSERVABILITY_ONLY
```

---

## 7. EventBusReady

Published when the bus reaches `READY`.

Event type:

```text
event-bus.lifecycle.ready
```

Payload:

```text
EventBusReadyPayload {
    registeredEventTypeCount
    registeredSubscriptionCount
    mandatorySubscriptionCount
    degradedComponents[]
    readyAt
}
```

---

## 8. EventBusStarted

Published after the bus enters `RUNNING`.

Event type:

```text
event-bus.lifecycle.started
```

Payload:

```text
EventBusStartedPayload {
    applicationInstanceId
    deliveryMode
    deliveryGuarantee
    queueTopologyClass
    startedAt
}
```

Expected values for MVP:

```text
deliveryMode = IN_PROCESS_ASYNC
deliveryGuarantee = AT_MOST_ONCE
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 9. EventBusDegraded

Published after the bus enters `DEGRADED`.

Event type:

```text
event-bus.lifecycle.degraded
```

Payload:

```text
EventBusDegradedPayload {
    previousState
    currentState = DEGRADED
    degradedComponents[]
    capabilityImpact[]
    reasonCode
    degradedAt
}
```

---

## 10. EventBusRecovered

Published after `DEGRADED → RUNNING`.

Event type:

```text
event-bus.lifecycle.recovered
```

Payload:

```text
EventBusRecoveredPayload {
    previousState = DEGRADED
    currentState = RUNNING
    recoveredComponents[]
    recoveredAt
}
```

---

## 11. EventBusQuiescing

Event type:

```text
event-bus.lifecycle.quiescing
```

Payload:

```text
EventBusQuiescingPayload {
    previousState
    currentState = QUIESCING
    allowedCategories[]
    rejectedCategories[]
    reasonCode
    effectiveAt
}
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 12. EventBusDrainStarted

Event type:

```text
event-bus.lifecycle.drain-started
```

Payload:

```text
EventBusDrainStartedPayload {
    drainId
    queueDepthAtStart
    activeDeliveryCount
    timeout
    includedPriorities[]
    startedAt
}
```

---

## 13. EventBusDrainCompleted

Event type:

```text
event-bus.lifecycle.drain-completed
```

Payload:

```text
EventBusDrainCompletedPayload {
    drainId
    outcome
    delivered
    dropped
    canceled
    abandoned
    remainingQueueDepth
    completedAt
}
```

Possible outcomes:

```text
DRAINED
PARTIALLY_DRAINED
TIMED_OUT
FAILED
```

---

## 14. EventBusStopping

Event type:

```text
event-bus.lifecycle.stopping
```

Payload:

```text
EventBusStoppingPayload {
    previousState
    currentState = STOPPING
    activeSubscriptions
    remainingQueueDepth
    reasonCode
    stoppingAt
}
```

---

## 15. EventBusTerminated

Event type:

```text
event-bus.lifecycle.terminated
```

Payload:

```text
EventBusTerminatedPayload {
    finalState = TERMINATED
    drainOutcome?
    abandonedDeliveryCount
    disabledSubscriptionCount
    terminatedAt
}
```

Visibility:

```text
PUBLIC_INTERNAL
```

---

## 16. EventBusFailed

Published after the bus enters `FAILED`.

Event type:

```text
event-bus.lifecycle.failed
```

Payload:

```text
EventBusFailedPayload {
    previousState
    currentState = FAILED
    normalizedErrorCode
    failedComponent
    normalPublicationBlocked
    stopRequired
    failedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
PUBLIC_INTERNAL safe projection
```

---

# Part II — Registry Events

## 17. EventRegistryBuildStarted

Event type:

```text
event-bus.registry.build-started
```

Payload:

```text
EventRegistryBuildStartedPayload {
    registryId
    expectedDescriptorCount?
    startedAt
}
```

Visibility:

```text
OBSERVABILITY_ONLY
```

---

## 18. EventRegistryValidated

Event type:

```text
event-bus.registry.validated
```

Payload:

```text
EventRegistryValidatedPayload {
    registryId
    registeredTypeCount
    deprecatedTypeCount
    warningCount
    validatedAt
}
```

---

## 19. EventRegistrySealed

Event type:

```text
event-bus.registry.sealed
```

Payload:

```text
EventRegistrySealedPayload {
    registryId
    descriptorCount
    registryRevision
    sealedAt
}
```

---

## 20. EventRegistryDegraded

Event type:

```text
event-bus.registry.degraded
```

Payload:

```text
EventRegistryDegradedPayload {
    registryId
    unavailableDescriptors[]
    unavailableUpcasters[]
    reasonCode
    degradedAt
}
```

Lists must contain safe identifiers only and remain bounded.

---

## 21. EventRegistryInvalidated

Event type:

```text
event-bus.registry.invalidated
```

Payload:

```text
EventRegistryInvalidatedPayload {
    registryId
    normalizedErrorCode
    conflictingEventTypes[]
    publicationBlocked
    invalidatedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

# Part III — Event Type Registration Events

## 22. EventTypeRegistered

Event type:

```text
event-bus.event-type.registered
```

Payload:

```text
EventTypeRegisteredPayload {
    registeredEventType
    eventVersion
    ownerModule
    category
    defaultVisibility
    orderingRequirement
    registeredAt
}
```

---

## 23. EventTypeActivated

Event type:

```text
event-bus.event-type.activated
```

Payload:

```text
EventTypeActivatedPayload {
    registeredEventType
    eventVersion
    activatedAt
}
```

---

## 24. EventTypeDeprecated

Event type:

```text
event-bus.event-type.deprecated
```

Payload:

```text
EventTypeDeprecatedPayload {
    registeredEventType
    eventVersion
    replacementEventType?
    replacementVersion?
    deprecatedAt
}
```

---

## 25. EventTypeDisabled

Event type:

```text
event-bus.event-type.disabled
```

Payload:

```text
EventTypeDisabledPayload {
    registeredEventType
    eventVersion
    reasonCode
    disabledAt
}
```

---

## 26. EventTypeRegistrationRejected

Event type:

```text
event-bus.event-type.registration-rejected
```

Payload:

```text
EventTypeRegistrationRejectedPayload {
    proposedEventType
    proposedVersion
    ownerModule
    normalizedErrorCode
    rejectionClass
    rejectedAt
}
```

---

# Part IV — Subscription Lifecycle Events

## 27. SubscriptionRegistered

Event type:

```text
event-bus.subscription.registered
```

Payload:

```text
SubscriptionRegisteredPayload {
    subscriptionId
    subscriberId
    subscriberModule
    subscribedEventType
    supportedVersions[]
    mandatory
    registeredAt
}
```

Visibility:

```text
MODULE_INTERNAL
OBSERVABILITY_ONLY
```

---

## 28. SubscriptionActivated

Event type:

```text
event-bus.subscription.activated
```

Payload:

```text
SubscriptionActivatedPayload {
    subscriptionId
    subscriberId
    subscribedEventType
    previousState
    currentState = ACTIVE
    activatedAt
}
```

---

## 29. SubscriptionPauseStarted

Event type:

```text
event-bus.subscription.pause-started
```

Payload:

```text
SubscriptionPauseStartedPayload {
    subscriptionId
    subscriberId
    activeDeliveryCount
    pauseReason
    startedAt
}
```

---

## 30. SubscriptionPaused

Event type:

```text
event-bus.subscription.paused
```

Payload:

```text
SubscriptionPausedPayload {
    subscriptionId
    subscriberId
    subscribedEventType
    queuedItemPolicy
    pausedAt
}
```

---

## 31. SubscriptionResumed

Event type:

```text
event-bus.subscription.resumed
```

Payload:

```text
SubscriptionResumedPayload {
    subscriptionId
    subscriberId
    subscribedEventType
    resultingHealthState
    resumedAt
}
```

---

## 32. SubscriptionDegraded

Event type:

```text
event-bus.subscription.degraded
```

Payload:

```text
SubscriptionDegradedPayload {
    subscriptionId
    subscriberId
    subscribedEventType
    reasonCode
    reducedConcurrency?
    healthState
    degradedAt
}
```

---

## 33. SubscriptionDisabled

Event type:

```text
event-bus.subscription.disabled
```

Payload:

```text
SubscriptionDisabledPayload {
    subscriptionId
    subscriberId
    subscribedEventType
    mandatory
    disableReason
    activeDeliveryPolicy
    disabledAt
}
```

Mandatory subscription disablement may degrade application readiness.

---

## 34. SubscriptionDrainStarted

Event type:

```text
event-bus.subscription.drain-started
```

Payload:

```text
SubscriptionDrainStartedPayload {
    subscriptionId
    subscriberId
    activeDeliveryCount
    deadline
    startedAt
}
```

---

## 35. SubscriptionDisposed

Event type:

```text
event-bus.subscription.disposed
```

Payload:

```text
SubscriptionDisposedPayload {
    subscriptionId
    subscriberId
    subscribedEventType
    finalDeliveryCount
    abandonedDeliveryCount
    disposedAt
}
```

---

## 36. SubscriptionRegistrationRejected

Event type:

```text
event-bus.subscription.registration-rejected
```

Payload:

```text
SubscriptionRegistrationRejectedPayload {
    subscriberId
    subscriberModule
    requestedEventType
    normalizedErrorCode
    rejectionClass
    rejectedAt
}
```

---

# Part V — Subscriber Health Events

## 37. SubscriberHealthChanged

Event type:

```text
event-bus.subscriber.health-changed
```

Payload:

```text
SubscriberHealthChangedPayload {
    subscriberId
    subscriptionId?
    previousHealth
    currentHealth
    failureRateClass?
    latencyClass?
    reasonCode
    changedAt
}
```

---

## 38. SubscriberSlowDetected

Event type:

```text
event-bus.subscriber.slow-detected
```

Payload:

```text
SubscriberSlowDetectedPayload {
    subscriberId
    subscriptionId
    handlerDurationClass
    configuredTimeout
    consecutiveSlowCount
    detectedAt
}
```

---

## 39. SubscriberCircuitOpened

Event type:

```text
event-bus.subscriber.circuit-opened
```

Payload:

```text
SubscriberCircuitOpenedPayload {
    subscriberId
    subscriptionId
    qualifyingFailureCount
    circuitDuration?
    openedAt
}
```

---

## 40. SubscriberRecoveryStarted

Event type:

```text
event-bus.subscriber.recovery-started
```

Payload:

```text
SubscriberRecoveryStartedPayload {
    subscriberId
    subscriptionId
    recoveryMode
    startedAt
}
```

---

## 41. SubscriberRecovered

Event type:

```text
event-bus.subscriber.recovered
```

Payload:

```text
SubscriberRecoveredPayload {
    subscriberId
    subscriptionId
    previousHealth
    currentHealth = HEALTHY
    recoveredAt
}
```

---

# Part VI — Ordering Lane Events

## 42. OrderingLaneCreated

Event type:

```text
event-bus.ordering-lane.created
```

Payload:

```text
OrderingLaneCreatedPayload {
    laneId
    orderingKeyKind
    capacity
    overflowPolicy
    createdAt
}
```

The ordering-key value should not be exposed by default.

---

## 43. OrderingLaneActivated

Event type:

```text
event-bus.ordering-lane.activated
```

Payload:

```text
OrderingLaneActivatedPayload {
    laneId
    previousState
    currentState = ACTIVE
    queueDepth
    activatedAt
}
```

---

## 44. OrderingLaneBackpressured

Event type:

```text
event-bus.ordering-lane.backpressured
```

Payload:

```text
OrderingLaneBackpressuredPayload {
    laneId
    queueDepth
    capacity
    utilizationClass
    affectedPriorityClasses[]
    overflowPolicy
    detectedAt
}
```

---

## 45. OrderingLaneRecovered

Event type:

```text
event-bus.ordering-lane.recovered
```

Payload:

```text
OrderingLaneRecoveredPayload {
    laneId
    previousState = BACKPRESSURED
    currentState = ACTIVE
    queueDepth
    recoveredAt
}
```

---

## 46. OrderingLaneDrainStarted

Event type:

```text
event-bus.ordering-lane.drain-started
```

Payload:

```text
OrderingLaneDrainStartedPayload {
    laneId
    queueDepth
    activeDeliveryCount
    startedAt
}
```

---

## 47. OrderingLaneClosed

Event type:

```text
event-bus.ordering-lane.closed
```

Payload:

```text
OrderingLaneClosedPayload {
    laneId
    finalQueueDepth
    droppedCount
    abandonedCount
    closeReason
    closedAt
}
```

---

## 48. OrderingLaneFailed

Event type:

```text
event-bus.ordering-lane.failed
```

Payload:

```text
OrderingLaneFailedPayload {
    laneId
    normalizedErrorCode
    queueDepth
    orderingPreservedUntilFailure
    failedAt
}
```

---

# Part VII — Publication Events

## 49. PublicationAccepted

High-volume publication success should normally remain metrics-only.

Event type:

```text
event-bus.publication.accepted
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
OBSERVABILITY_ONLY
```

Payload:

```text
PublicationAcceptedPayload {
    affectedEventId
    affectedEventType
    affectedEventVersion
    sourceModule
    laneId?
    subscriberMatchCount?
    acceptedAt
}
```

---

## 50. PublicationAcceptedWithoutSubscribers

Event type:

```text
event-bus.publication.accepted-without-subscribers
```

Payload:

```text
PublicationAcceptedWithoutSubscribersPayload {
    affectedEventId
    affectedEventType
    sourceModule
    subscriberRequired
    acceptedAt
}
```

---

## 51. PublicationRejected

Event type:

```text
event-bus.publication.rejected
```

Payload:

```text
PublicationRejectedPayload {
    affectedEventId?
    affectedEventType?
    sourceModule?
    publisherId?
    rejectionStage
    normalizedErrorCode
    retryable
    rejectedAt
}
```

Must not include the rejected payload.

---

## 52. PublicationTimedOut

Event type:

```text
event-bus.publication.timed-out
```

Payload:

```text
PublicationTimedOutPayload {
    affectedEventId
    affectedEventType
    laneId?
    admissionTimeout
    queueDepth?
    timedOutAt
}
```

---

## 53. EventCoalesced

Event type:

```text
event-bus.publication.coalesced
```

Payload:

```text
EventCoalescedPayload {
    incomingEventId
    retainedEventId?
    affectedEventType
    laneId
    coalescingOutcome
    coalescedAt
}
```

Possible outcomes:

```text
REPLACED_EXISTING
MERGED
INCOMING_SUPERSEDED
```

---

## 54. EventDropped

Event type:

```text
event-bus.publication.dropped
```

Payload:

```text
EventDroppedPayload {
    affectedEventId
    affectedEventType
    category
    priority
    laneId?
    dropReason
    shutdownRelated
    droppedAt
}
```

Recommended visibility:

```text
OBSERVABILITY_ONLY
```

Domain, result, security, or audit event drops should escalate.

---

## 55. EventFiltered

Event type:

```text
event-bus.publication.filtered
```

Payload:

```text
EventFilteredPayload {
    affectedEventId
    affectedEventType
    filterClass
    noEligibleRoute
    filteredAt
}
```

---

# Part VIII — Routing Events

## 56. RoutingCompleted

Normally metrics-only.

Event type:

```text
event-bus.routing.completed
```

Payload:

```text
RoutingCompletedPayload {
    affectedEventId
    affectedEventType
    matchedCount
    filteredCount
    unauthorizedCount
    unsupportedVersionCount
    completedAt
}
```

---

## 57. RoutingRejected

Event type:

```text
event-bus.routing.rejected
```

Payload:

```text
RoutingRejectedPayload {
    affectedEventId
    affectedEventType
    normalizedErrorCode
    routingStage
    rejectedAt
}
```

---

## 58. UnauthorizedSubscriberExcluded

Event type:

```text
event-bus.routing.unauthorized-subscriber-excluded
```

Payload:

```text
UnauthorizedSubscriberExcludedPayload {
    affectedEventId
    affectedEventType
    subscriberId
    subscriptionId
    visibility
    securityClassification
    exclusionReason
    excludedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

## 59. UnsupportedSubscriberVersionExcluded

Event type:

```text
event-bus.routing.unsupported-version-excluded
```

Payload:

```text
UnsupportedSubscriberVersionExcludedPayload {
    affectedEventId
    affectedEventType
    eventVersion
    subscriberId
    subscriptionId
    supportedVersions[]
    excludedAt
}
```

---

# Part IX — Delivery Events

## 60. DeliveryScheduled

High-volume event; normally local or metrics-only.

Event type:

```text
event-bus.delivery.scheduled
```

Payload:

```text
DeliveryScheduledPayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    laneId?
    attemptNumber
    scheduledAt
}
```

---

## 61. DeliveryStarted

Event type:

```text
event-bus.delivery.started
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
```

Payload:

```text
DeliveryStartedPayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    attemptNumber
    startedAt
}
```

---

## 62. DeliveryHandled

Event type:

```text
event-bus.delivery.handled
```

Recommended visibility:

```text
OBSERVABILITY_ONLY
```

Payload:

```text
DeliveryHandledPayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    handlerDuration
    attemptNumber
    handledAt
}
```

---

## 63. DeliveryIgnoredNotRelevant

Event type:

```text
event-bus.delivery.ignored-not-relevant
```

Payload:

```text
DeliveryIgnoredNotRelevantPayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    ignoredAt
}
```

---

## 64. DeliveryIgnoredStale

Event type:

```text
event-bus.delivery.ignored-stale
```

Payload:

```text
DeliveryIgnoredStalePayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    observedRevision?
    currentRevision?
    ignoredAt
}
```

---

## 65. DeliveryDuplicateDetected

Event type:

```text
event-bus.delivery.duplicate-detected
```

Payload:

```text
DeliveryDuplicateDetectedPayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    deduplicationClass
    detectedAt
}
```

---

## 66. DeliveryFailed

Event type:

```text
event-bus.delivery.failed
```

Payload:

```text
DeliveryFailedPayload {
    deliveryId
    affectedEventId
    affectedEventType
    subscriptionId
    subscriberId
    attemptNumber
    normalizedErrorCode
    retryable
    retryScheduled
    failedAt
}
```

No raw exception or payload may be included.

---

## 67. DeliveryRetryScheduled

Event type:

```text
event-bus.delivery.retry-scheduled
```

Payload:

```text
DeliveryRetryScheduledPayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    previousAttemptNumber
    nextAttemptNumber
    retryMode
    retryAt
}
```

---

## 68. DeliveryTimedOut

Event type:

```text
event-bus.delivery.timed-out
```

Payload:

```text
DeliveryTimedOutPayload {
    deliveryId
    affectedEventId
    affectedEventType
    subscriptionId
    subscriberId
    timeout
    cancellationRequested
    physicalExecutionMayContinue
    timedOutAt
}
```

---

## 69. DeliveryCanceled

Event type:

```text
event-bus.delivery.canceled
```

Payload:

```text
DeliveryCanceledPayload {
    deliveryId
    affectedEventId
    subscriptionId
    subscriberId
    cancellationReason
    canceledAt
}
```

---

## 70. DeliveryAbandoned

Event type:

```text
event-bus.delivery.abandoned
```

Payload:

```text
DeliveryAbandonedPayload {
    deliveryId
    affectedEventId
    affectedEventType
    subscriptionId
    subscriberId
    abandonmentReason
    logicalAuthorityRemoved
    physicalExecutionUnconfirmed
    abandonedAt
}
```

---

## 71. DeliveryUnsupportedVersionRejected

Event type:

```text
event-bus.delivery.unsupported-version-rejected
```

Payload:

```text
DeliveryUnsupportedVersionRejectedPayload {
    deliveryId
    affectedEventId
    affectedEventType
    eventVersion
    subscriptionId
    subscriberId
    supportedVersions[]
    rejectedAt
}
```

---

# Part X — Dispatcher Events

## 72. DispatcherStarted

Event type:

```text
event-bus.dispatcher.started
```

Payload:

```text
DispatcherStartedPayload {
    dispatcherId
    concurrencyClass
    laneCount
    startedAt
}
```

---

## 73. DispatcherIdle

High-frequency and optional.

Event type:

```text
event-bus.dispatcher.idle
```

Visibility:

```text
LOCAL_COMPONENT_ONLY
```

---

## 74. DispatcherBackpressured

Event type:

```text
event-bus.dispatcher.backpressured
```

Payload:

```text
DispatcherBackpressuredPayload {
    dispatcherId
    activeDeliveryCount
    totalQueueDepth
    pressureClass
    detectedAt
}
```

---

## 75. DispatcherRecovered

Event type:

```text
event-bus.dispatcher.recovered
```

Payload:

```text
DispatcherRecoveredPayload {
    dispatcherId
    previousState = BACKPRESSURED
    currentState = DISPATCHING
    recoveredAt
}
```

---

## 76. DispatcherDrainStarted

Event type:

```text
event-bus.dispatcher.drain-started
```

Payload:

```text
DispatcherDrainStartedPayload {
    dispatcherId
    queueDepth
    activeDeliveryCount
    startedAt
}
```

---

## 77. DispatcherStopped

Event type:

```text
event-bus.dispatcher.stopped
```

Payload:

```text
DispatcherStoppedPayload {
    dispatcherId
    finalQueueDepth
    abandonedDeliveryCount
    stoppedAt
}
```

---

## 78. DispatcherFailed

Event type:

```text
event-bus.dispatcher.failed
```

Payload:

```text
DispatcherFailedPayload {
    dispatcherId
    normalizedErrorCode
    affectedLaneCount
    orderingAtRisk
    failedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

---

# Part XI — Queue and Backpressure Events

## 79. QueueCapacityWarningRaised

Event type:

```text
event-bus.queue.capacity-warning-raised
```

Payload:

```text
QueueCapacityWarningRaisedPayload {
    laneClass
    queueDepth
    capacity
    utilizationClass
    affectedPriorityClasses[]
    raisedAt
}
```

---

## 80. QueueCapacityRecovered

Event type:

```text
event-bus.queue.capacity-recovered
```

Payload:

```text
QueueCapacityRecoveredPayload {
    laneClass
    queueDepth
    capacity
    recoveredAt
}
```

---

## 81. CriticalQueueReserveUsed

Event type:

```text
event-bus.queue.critical-reserve-used
```

Payload:

```text
CriticalQueueReserveUsedPayload {
    reserveCapacity
    reserveRemaining
    affectedEventType
    usedAt
}
```

Visibility:

```text
OBSERVABILITY_ONLY
```

---

## 82. CriticalQueueReserveExhausted

Event type:

```text
event-bus.queue.critical-reserve-exhausted
```

Payload:

```text
CriticalQueueReserveExhaustedPayload {
    reserveCapacity
    affectedEventType
    normalizedErrorCode
    exhaustedAt
}
```

Visibility:

```text
RESTRICTED_SECURITY
```

This may degrade or fail the Event Bus depending on safety impact.

---

# Part XII — Security Events

## 83. UnsafeEventPayloadBlocked

Event type:

```text
event-bus.security.unsafe-payload-blocked
```

Payload:

```text
UnsafeEventPayloadBlockedPayload {
    affectedEventId?
    affectedEventType?
    publisherId?
    sourceModule?
    findingClasses[]
    publicationBlocked = true
    blockedAt
}
```

Possible finding classes:

```text
SECRET_TYPE
AUTHORIZATION_HEADER
PRIVATE_KEY_BLOCK
PASSWORD_FIELD
TOKEN_PATTERN
RAW_USER_CONTENT
LARGE_BINARY
UNSAFE_EXCEPTION
MUTABLE_PLATFORM_OBJECT
UNREGISTERED_PAYLOAD_TYPE
OVERSIZED_PAYLOAD
SENSITIVE_METADATA
```

The matched value is never included.

Visibility:

```text
RESTRICTED_SECURITY
AUDIT_ONLY
```

---

## 84. UnauthorizedPublisherBlocked

Event type:

```text
event-bus.security.unauthorized-publisher-blocked
```

Payload:

```text
UnauthorizedPublisherBlockedPayload {
    publisherId
    sourceModule?
    attemptedEventType
    ownershipMismatch
    visibilityRequested
    blockedAt
}
```

---

## 85. UnauthorizedSubscriptionBlocked

Event type:

```text
event-bus.security.unauthorized-subscription-blocked
```

Payload:

```text
UnauthorizedSubscriptionBlockedPayload {
    subscriberId
    subscriberModule
    attemptedEventType
    requestedVisibility?
    requestedClassification?
    blockedAt
}
```

---

## 86. EventNamespaceOwnershipViolationDetected

Event type:

```text
event-bus.security.namespace-ownership-violation-detected
```

Payload:

```text
EventNamespaceOwnershipViolationDetectedPayload {
    publisherId
    sourceModule
    attemptedEventType
    registeredOwnerModule
    blocked
    detectedAt
}
```

---

## 87. RestrictedEventRoutingViolationBlocked

Event type:

```text
event-bus.security.restricted-routing-violation-blocked
```

Payload:

```text
RestrictedEventRoutingViolationBlockedPayload {
    affectedEventId
    affectedEventType
    subscriberId
    subscriptionId
    visibility
    classification
    blockedAt
}
```

---

## 88. EventPayloadLoggingBlocked

Event type:

```text
event-bus.security.payload-logging-blocked
```

Payload:

```text
EventPayloadLoggingBlockedPayload {
    affectedEventId?
    affectedEventType?
    loggingBoundary
    findingClass
    blockedAt
}
```

---

## 89. EventLoopDetected

Event type:

```text
event-bus.security.event-loop-detected
```

Payload:

```text
EventLoopDetectedPayload {
    correlationId
    repeatedEventTypes[]
    causationDepth
    loopPolicyAction
    detectedAt
}
```

The repeated type list must be bounded.

---

## 90. MaximumCausationDepthExceeded

Event type:

```text
event-bus.security.maximum-causation-depth-exceeded
```

Payload:

```text
MaximumCausationDepthExceededPayload {
    affectedEventId
    affectedEventType
    correlationId
    maximumDepth
    publicationBlocked
    detectedAt
}
```

---

# Part XIII — Diagnostic Buffer Events

## 91. DiagnosticFailureBuffered

Event type:

```text
event-bus.diagnostics.failure-buffered
```

Recommended visibility:

```text
LOCAL_COMPONENT_ONLY
```

Payload:

```text
DiagnosticFailureBufferedPayload {
    failureRecordId
    affectedEventId
    subscriberId
    normalizedErrorCode
    bufferedAt
}
```

---

## 92. DiagnosticFailureEvicted

Event type:

```text
event-bus.diagnostics.failure-evicted
```

Payload:

```text
DiagnosticFailureEvictedPayload {
    failureRecordId
    evictionReason
    bufferUtilizationClass
    evictedAt
}
```

---

## 93. DiagnosticBufferCapacityReached

Event type:

```text
event-bus.diagnostics.buffer-capacity-reached
```

Payload:

```text
DiagnosticBufferCapacityReachedPayload {
    capacity
    evictionPolicy
    reachedAt
}
```

This event must use a non-recursive reporting path.

---

# Part XIV — Durable Adapter and Outbox Events

## 94. DurableAdapterInitializationStarted

Future event type:

```text
event-bus.durable-adapter.initialization-started
```

Payload:

```text
DurableAdapterInitializationStartedPayload {
    adapterId
    adapterType
    startedAt
}
```

---

## 95. DurableAdapterAvailable

Event type:

```text
event-bus.durable-adapter.available
```

Payload:

```text
DurableAdapterAvailablePayload {
    adapterId
    adapterType
    deliveryGuarantee
    availableAt
}
```

---

## 96. DurableAdapterDegraded

Event type:

```text
event-bus.durable-adapter.degraded
```

Payload:

```text
DurableAdapterDegradedPayload {
    adapterId
    degradedCapabilities[]
    reasonCode
    degradedAt
}
```

---

## 97. DurableAdapterUnavailable

Event type:

```text
event-bus.durable-adapter.unavailable
```

Payload:

```text
DurableAdapterUnavailablePayload {
    adapterId
    normalizedErrorCode
    durablePublicationBlocked
    inMemoryBusAvailable
    unavailableAt
}
```

---

## 98. OutboxRecordCreated

Event type:

```text
event-bus.outbox.record-created
```

Payload:

```text
OutboxRecordCreatedPayload {
    outboxRecordId
    affectedEventId
    affectedEventType
    createdAt
}
```

---

## 99. OutboxPublicationStarted

Event type:

```text
event-bus.outbox.publication-started
```

Payload:

```text
OutboxPublicationStartedPayload {
    outboxRecordId
    affectedEventId
    attemptNumber
    startedAt
}
```

---

## 100. OutboxPublicationAcknowledged

Event type:

```text
event-bus.outbox.publication-acknowledged
```

Payload:

```text
OutboxPublicationAcknowledgedPayload {
    outboxRecordId
    affectedEventId
    deliveryReference?
    acknowledgedAt
}
```

---

## 101. OutboxRetryScheduled

Event type:

```text
event-bus.outbox.retry-scheduled
```

Payload:

```text
OutboxRetryScheduledPayload {
    outboxRecordId
    affectedEventId
    previousAttemptNumber
    nextAttemptNumber
    retryAt
}
```

---

## 102. OutboxRecordDeadLettered

Event type:

```text
event-bus.outbox.record-dead-lettered
```

Payload:

```text
OutboxRecordDeadLetteredPayload {
    outboxRecordId
    affectedEventId
    affectedEventType
    finalAttemptCount
    normalizedErrorCode
    deadLetteredAt
}
```

---

# Part XV — Consumed Events

## 103. Events Consumed by Event Bus

The Event Bus consumes a limited set of application and infrastructure lifecycle events.

It must not consume domain events to orchestrate feature workflows.

---

## 104. ApplicationShutdownStarted

Potential source:

```text
application.shutdown.started
```

Reaction:

```text
RUNNING / DEGRADED
    ↓
QUIESCING
    ↓
DRAINING
```

---

## 105. ConfigurationSnapshotActivated

Potential source:

```text
configuration.snapshot.activated
```

Event Bus may evaluate:

- handler timeout;
- progress throttle;
- diagnostics limits;
- queue thresholds;
- metrics options.

Transport or topology changes may require restart.

---

## 106. SecurityPolicyChanged

Potential source:

```text
security.policy.changed
```

Event Bus may revalidate:

- publisher authorization;
- subscriber clearance;
- restricted routes;
- payload inspection policy.

Active restricted deliveries require explicit policy handling.

---

## 107. LoggingUnavailable

Potential source:

```text
logging.backend.unavailable
```

Event Bus must avoid recursive failure.

It may:

- reduce diagnostics;
- retain safe in-memory counters;
- degrade observability;
- continue core delivery when safe.

---

## 108. MetricsUnavailable

Potential source:

```text
metrics.backend.unavailable
```

Metrics failure must not block event delivery.

---

## 109. SecretManagementBackendCompromised

The Event Bus may receive the safe restricted event:

```text
secret-management.backend.compromised
```

It routes the fact to authorized subscribers.

It does not independently mutate Secret Management state.

---

# Part XVI — Event Ordering

## 110. Lifecycle Ordering

Expected sequence:

```text
EventBusInitializationStarted
    ↓
EventRegistryValidated
    ↓
EventRegistrySealed
    ↓
EventBusReady
    ↓
EventBusStarted
```

Shutdown:

```text
EventBusQuiescing
    ↓
EventBusDrainStarted
    ↓
EventBusDrainCompleted
    ↓
EventBusStopping
    ↓
EventBusTerminated
```

---

## 111. Subscription Ordering

```text
SubscriptionRegistered
    ↓
SubscriptionActivated
    ↓
SubscriptionDegraded / SubscriptionPaused
    ↓
SubscriptionResumed / SubscriptionDisabled
    ↓
SubscriptionDrainStarted
    ↓
SubscriptionDisposed
```

---

## 112. Delivery Ordering

```text
DeliveryScheduled
    ↓
DeliveryStarted
    ↓
one terminal event
```

Terminal event is exactly one of:

```text
DeliveryHandled
DeliveryIgnoredNotRelevant
DeliveryIgnoredStale
DeliveryDuplicateDetected
DeliveryUnsupportedVersionRejected
DeliveryFailed
DeliveryTimedOut
DeliveryCanceled
DeliveryAbandoned
```

---

## 113. Rotation of Subscriber Health

Example:

```text
SubscriberSlowDetected
    ↓
SubscriberHealthChanged(HEALTHY → SLOW)
    ↓
SubscriptionDegraded
```

The health event reports evidence.

The subscription transition remains policy-controlled.

---

# Part XVII — Duplicate and Stale Handling

## 114. Duplicate Self-Events

Consumers must deduplicate using:

```text
eventId
```

or:

```text
entityId + stateVersion + eventType
```

---

## 115. Out-of-Order Self-Events

Consumers compare:

- lifecycle state version;
- subscription state version;
- occurredAt;
- entity identity.

A delayed `SubscriptionDegraded` must not overwrite a newer `SubscriptionDisabled`.

---

## 116. Late Delivery Completion

When `DeliveryTimedOut` or `DeliveryAbandoned` is already terminal, a later handler completion must not publish `DeliveryHandled` as authoritative.

It may produce a restricted late-completion diagnostic.

Optional event:

```text
event-bus.delivery.late-completion-observed
```

Payload:

```text
DeliveryLateCompletionObservedPayload {
    deliveryId
    terminalState
    physicalCompletionObservedAt
}
```

---

# Part XVIII — Publication Rules

## 117. Self-Event Admission

Event Bus self-events should use:

- reserved capacity;
- non-recursive publication;
- restricted fallback sink;
- bounded payload;
- no original event payload.

---

## 118. Events That May Be Sampled

Potentially high-volume:

```text
PublicationAccepted
DeliveryScheduled
DeliveryStarted
DeliveryHandled
RoutingCompleted
DispatcherIdle
DiagnosticFailureBuffered
```

These may be sampled, aggregated, or metrics-only.

---

## 119. Events That Must Not Be Sampled Away

```text
EventBusFailed
EventRegistryInvalidated
SubscriptionDisabled
OrderingLaneFailed
CriticalQueueReserveExhausted
UnsafeEventPayloadBlocked
UnauthorizedPublisherBlocked
RestrictedEventRoutingViolationBlocked
DeliveryAbandoned
EventBusDrainCompleted
EventBusTerminated
```

---

## 120. Coalescing Rules

Event Bus self-events describing progress or repeated pressure may be coalesced when safe.

Terminal lifecycle and security events may not be coalesced away.

---

# Part XIX — Observability

## 121. Metrics Mapping

Self-events may feed metrics such as:

```text
event_bus_lifecycle_transition_total
event_bus_publication_rejected_total
event_bus_event_dropped_total
event_bus_event_coalesced_total
event_bus_delivery_failed_total
event_bus_delivery_timeout_total
event_bus_delivery_abandoned_total
event_bus_subscription_disabled_total
event_bus_lane_backpressure_total
event_bus_security_block_total
event_bus_drain_outcome_total
event_bus_outbox_dead_letter_total
```

---

## 122. Logging

Allowed fields:

```text
selfEventType
affectedEventId
affectedEventType
subscriptionId
subscriberId
laneId
deliveryId
normalizedErrorCode
stateTransition
priority
visibility
correlationId
```

Prohibited:

```text
original payload
secret material
user content
raw exception
provider-native response
authorization data
```

---

## 123. Tracing

Self-events may create or annotate spans:

```text
event-bus.lifecycle
event-bus.publication
event-bus.routing
event-bus.delivery
event-bus.drain
```

Tracing remains payload-free.

---

# Part XX — Security Validation

## 124. Pre-Publication Validation

Every self-event passes:

```text
schema validation
    ↓
bounded payload validation
    ↓
restricted-field inspection
    ↓
non-recursion guard
    ↓
visibility validation
    ↓
publication
```

---

## 125. Sensitive Data Rule

Self-events must never include:

- the original event payload;
- secret values;
- user text;
- screenshots;
- binary artifacts;
- raw exception messages;
- arbitrary metadata maps from the original event.

---

## 126. Wildcard Subscriber Rule

Generic wildcard subscribers may receive Event Bus self-events only when:

- explicitly authorized;
- visibility permits;
- payload remains safe;
- restricted events remain excluded without clearance.

---

# Part XXI — Event Catalog Summary

## 127. Lifecycle Events

```text
EventBusInitializationStarted
EventBusReady
EventBusStarted
EventBusDegraded
EventBusRecovered
EventBusQuiescing
EventBusDrainStarted
EventBusDrainCompleted
EventBusStopping
EventBusTerminated
EventBusFailed
```

## 128. Registry and Type Events

```text
EventRegistryBuildStarted
EventRegistryValidated
EventRegistrySealed
EventRegistryDegraded
EventRegistryInvalidated
EventTypeRegistered
EventTypeActivated
EventTypeDeprecated
EventTypeDisabled
EventTypeRegistrationRejected
```

## 129. Subscription Events

```text
SubscriptionRegistered
SubscriptionActivated
SubscriptionPauseStarted
SubscriptionPaused
SubscriptionResumed
SubscriptionDegraded
SubscriptionDisabled
SubscriptionDrainStarted
SubscriptionDisposed
SubscriptionRegistrationRejected
```

## 130. Health and Lane Events

```text
SubscriberHealthChanged
SubscriberSlowDetected
SubscriberCircuitOpened
SubscriberRecoveryStarted
SubscriberRecovered
OrderingLaneCreated
OrderingLaneActivated
OrderingLaneBackpressured
OrderingLaneRecovered
OrderingLaneDrainStarted
OrderingLaneClosed
OrderingLaneFailed
```

## 131. Publication and Routing Events

```text
PublicationAccepted
PublicationAcceptedWithoutSubscribers
PublicationRejected
PublicationTimedOut
EventCoalesced
EventDropped
EventFiltered
RoutingCompleted
RoutingRejected
UnauthorizedSubscriberExcluded
UnsupportedSubscriberVersionExcluded
```

## 132. Delivery Events

```text
DeliveryScheduled
DeliveryStarted
DeliveryHandled
DeliveryIgnoredNotRelevant
DeliveryIgnoredStale
DeliveryDuplicateDetected
DeliveryFailed
DeliveryRetryScheduled
DeliveryTimedOut
DeliveryCanceled
DeliveryAbandoned
DeliveryUnsupportedVersionRejected
DeliveryLateCompletionObserved
```

## 133. Dispatcher and Queue Events

```text
DispatcherStarted
DispatcherIdle
DispatcherBackpressured
DispatcherRecovered
DispatcherDrainStarted
DispatcherStopped
DispatcherFailed
QueueCapacityWarningRaised
QueueCapacityRecovered
CriticalQueueReserveUsed
CriticalQueueReserveExhausted
```

## 134. Security Events

```text
UnsafeEventPayloadBlocked
UnauthorizedPublisherBlocked
UnauthorizedSubscriptionBlocked
EventNamespaceOwnershipViolationDetected
RestrictedEventRoutingViolationBlocked
EventPayloadLoggingBlocked
EventLoopDetected
MaximumCausationDepthExceeded
```

## 135. Durable Events

```text
DurableAdapterInitializationStarted
DurableAdapterAvailable
DurableAdapterDegraded
DurableAdapterUnavailable
OutboxRecordCreated
OutboxPublicationStarted
OutboxPublicationAcknowledged
OutboxRetryScheduled
OutboxRecordDeadLettered
```

---

# Part XXII — MVP Event Boundary

## 136. Required MVP Events

The MVP should implement:

```text
EventBusStarted
EventBusDegraded
EventBusRecovered
EventBusQuiescing
EventBusDrainStarted
EventBusDrainCompleted
EventBusTerminated
EventBusFailed

EventRegistryValidated
EventRegistryInvalidated

SubscriptionActivated
SubscriptionDegraded
SubscriptionDisabled
SubscriptionDisposed
SubscriptionRegistrationRejected

SubscriberHealthChanged

OrderingLaneBackpressured
OrderingLaneRecovered
OrderingLaneFailed

PublicationRejected
PublicationTimedOut
EventCoalesced
EventDropped

DeliveryFailed
DeliveryTimedOut
DeliveryCanceled
DeliveryAbandoned
DeliveryUnsupportedVersionRejected

DispatcherBackpressured
DispatcherFailed

QueueCapacityWarningRaised
CriticalQueueReserveExhausted

UnsafeEventPayloadBlocked
UnauthorizedPublisherBlocked
UnauthorizedSubscriptionBlocked
RestrictedEventRoutingViolationBlocked
EventLoopDetected
```

---

## 137. Optional MVP Events

May remain metrics-only:

```text
PublicationAccepted
DeliveryScheduled
DeliveryStarted
DeliveryHandled
RoutingCompleted
DispatcherIdle
DiagnosticFailureBuffered
OrderingLaneCreated
```

---

## 138. Deferred Events

May be deferred until durable mode:

```text
DurableAdapter*
Outbox*
dead-letter lifecycle
replay lifecycle
distributed partition lifecycle
consumer-group lifecycle
```

---

# Part XXIII — Event Decisions

## 139. Decisions

### Decision 1 — Self-events do not carry original payloads

Only safe event identity and delivery metadata are allowed.

### Decision 2 — Self-events use non-recursive reporting

The Event Bus cannot depend on an unprotected ordinary path to report its own failure.

### Decision 3 — High-volume success events are optional

Metrics may replace publication of every success event.

### Decision 4 — Security and terminal failures are never sampled away

Critical facts remain explicit.

### Decision 5 — Publication and delivery events remain separate

Acceptance does not imply handling.

### Decision 6 — Late completion is non-authoritative

It cannot overwrite timeout or abandonment.

### Decision 7 — State commits precede self-events

Self-events report accepted lifecycle truth.

### Decision 8 — Subscriber health and subscription state remain separate

Health evidence may trigger policy, but does not directly own subscription state.

### Decision 9 — Queue pressure is observable

Backpressure and reserve exhaustion are explicit.

### Decision 10 — Durable events are future extensions

They do not alter MVP at-most-once semantics.

---

# Part XXIV — Open Decisions

## 140. Visibility Decisions

Still to finalize:

- which lifecycle events are public;
- exact restricted subscriber list;
- whether `SubscriptionDisabled` is public when mandatory;
- whether queue reserve events reach application health;
- audit duplication rules.

---

## 141. Sampling Decisions

Still to finalize:

- success-event sample rates;
- delivery-success aggregation;
- routing event aggregation;
- lane-created event retention;
- slow-subscriber event throttle.

---

## 142. Security Decisions

Still to finalize:

- exact non-recursive reporting implementation;
- restricted sink;
- causation-depth threshold;
- event-loop detection policy;
- security-event retention;
- payload-inspector finding classes.

---

## 143. Durable Decisions

Still to finalize:

- outbox event visibility;
- dead-letter audit behavior;
- adapter acknowledgment event;
- replay events;
- cross-process delivery events.

---

# Part XXV — Related Documents

## 144. Related Documents

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
03-infrastructure/event-bus/STATES.md

03-infrastructure/configuration/EVENTS.md
03-infrastructure/secret-management/EVENTS.md
02-modules/provider-management/EVENTS.md
```

Future Event Bus documents:

```text
03-infrastructure/event-bus/ERRORS.md
03-infrastructure/event-bus/README.md
```

---

## 145. Summary

Event Bus self-events expose safe infrastructure facts without carrying the original event payload.

The lifecycle flow is:

```text
EventBusInitializationStarted
    ↓
EventBusReady
    ↓
EventBusStarted
    ↓
EventBusQuiescing
    ↓
EventBusDrainStarted
    ↓
EventBusDrainCompleted
    ↓
EventBusTerminated
```

The publication and delivery flow is:

```text
PublicationAccepted / Rejected / Dropped
    ↓
DeliveryScheduled
    ↓
DeliveryStarted
    ↓
one terminal delivery event
```

The event model guarantees:

- immutable past-tense facts;
- state commits before publication;
- no original payload copies;
- no secrets or user content;
- non-recursive failure reporting;
- publication and delivery remain distinct;
- security violations use restricted visibility;
- high-volume success events may be metrics-only;
- critical failures are never sampled away;
- late completion cannot rewrite terminal delivery state;
- queue pressure and subscriber health remain observable;
- durable events remain optional future extensions.

This document is the event source of truth for subsequent Event Bus errors and implementation documentation.
