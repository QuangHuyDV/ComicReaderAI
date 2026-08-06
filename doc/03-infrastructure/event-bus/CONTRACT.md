# Event Bus Contract

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Event Bus  
> **Document:** Public and Internal Contracts  
> **Path:** `03-infrastructure/event-bus/CONTRACT.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/event-bus/MODULE.md`
> - `docs/architecture/EVENT_BUS.md`
> - `docs/architecture/STATE_MACHINE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/CANCELLATION.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/configuration/EVENTS.md`
> - `03-infrastructure/secret-management/EVENTS.md`
> - `02-modules/provider-management/EVENTS.md`

---

## 1. Purpose

This document defines the contracts exposed and consumed by the Event Bus infrastructure module.

It specifies:

- event envelopes;
- publisher contracts;
- publication requests and receipts;
- subscription contracts;
- handler contracts;
- event type registration;
- event version compatibility;
- ordering keys and lanes;
- routing filters;
- visibility and security classification;
- priority;
- queue admission;
- delivery outcomes;
- handler timeout and cancellation;
- subscriber isolation;
- duplicate handling;
- progress coalescing;
- shutdown and drain contracts;
- optional outbox and durable adapter boundaries;
- diagnostics and observability contracts;
- serialization and payload-safety rules;
- lifecycle controls;
- contract versioning.

This document does not define:

- domain-specific event payloads;
- application commands;
- query interfaces;
- workflow orchestration;
- business retry policy;
- external broker implementation;
- detailed state machines;
- Event Bus self-events;
- detailed error catalogs;
- persistence tables;
- concrete thread or queue primitives.

Detailed lifecycles belong in `STATES.md`.

Event Bus integration events belong in `EVENTS.md`.

Normalized failures belong in `ERRORS.md`.

---

## 2. Contract Goals

Event Bus contracts must:

1. preserve immutable event semantics;
2. keep publisher and subscriber APIs transport-neutral;
3. support typed publication and subscription;
4. support asynchronous dispatch by default;
5. preserve scoped ordering;
6. keep all queues bounded;
7. isolate subscriber failure;
8. support version compatibility;
9. enforce event ownership;
10. enforce visibility and security classification;
11. prevent secrets and unsafe payloads from entering the bus;
12. avoid large artifact transport;
13. support duplicate-aware consumers;
14. support stale-event relevance checks;
15. preserve correlation and causation;
16. support deterministic shutdown;
17. allow future durable delivery without changing domain contracts;
18. avoid promising exactly-once behavior;
19. prevent implicit pipeline orchestration;
20. keep observability payload-safe.

---

## 3. Contract Classification

The Event Bus defines five contract groups.

### 3.1 Core event contracts

```text
EventEnvelope<TPayload>
EventIdentity
EventContext
EventOrdering
EventSecurity
EventMetadata
```

### 3.2 Publication contracts

```text
EventPublisher
PublishRequest<TPayload>
PublishOptions
PublishReceipt
PublicationOutcome
```

### 3.3 Subscription contracts

```text
EventSubscriber
SubscriptionRequest<TPayload>
SubscriptionDescriptor
EventHandler<TPayload>
SubscriptionHandle
SubscriptionFilter
```

### 3.4 Delivery contracts

```text
DeliveryContext
DeliveryResult
DeliverySummary
DeliveryFailure
HandlerExecutionPolicy
```

### 3.5 Administrative and lifecycle contracts

```text
EventTypeRegistry
EventTypeDescriptor
EventBusControl
EventBusStatus
DrainRequest
DrainResult
OutboxPublisherPort
DurableEventAdapter
```

---

## 4. Core Identifiers

```text
EventId
EventType
EventVersion
CorrelationId
CausationId
ApplicationInstanceId
PublisherId
SubscriberId
SubscriptionId
DeliveryId
OrderingLaneId
OutboxRecordId?
ReplaySessionId?
SessionId?
PipelineId?
TaskId?
WorkItemId?
AttemptId?
EntityId?
```

Rules:

- identifiers are opaque;
- event IDs are globally unique within practical application scope;
- subscription IDs are unique per application instance;
- delivery IDs identify one event-to-subscriber attempt;
- IDs must not embed raw payload content;
- IDs must not embed secrets;
- IDs should be safe for logs unless otherwise classified.

---

## 5. EventEnvelope

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

The envelope is immutable after publication admission.

---

## 6. Event Category

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

`COMMAND` is not a canonical Event Bus category.

Action requests belong to command contracts or explicit application services.

---

## 7. Event Priority

```text
CRITICAL
HIGH
NORMAL
LOW
BACKGROUND
```

Priority affects dispatch preference and overflow policy.

Priority does not:

- override security;
- override same-lane ordering;
- grant delivery guarantees;
- grant synchronous behavior;
- change event meaning.

---

## 8. Visibility

```text
PUBLIC_INTERNAL
MODULE_INTERNAL
RESTRICTED_SECURITY
OBSERVABILITY_ONLY
AUDIT_ONLY
LOCAL_COMPONENT_ONLY
```

Visibility is routing policy.

It is not payload encryption.

---

## 9. Security Classification

```text
INTERNAL
CONFIDENTIAL_METADATA
RESTRICTED_SECURITY
```

No classification permits raw secret material.

---

## 10. EventOrdering

```text
EventOrdering {
    mode
    orderingKey?
    partitionKey?
    sequence?
}
```

Possible modes:

```text
UNORDERED
APPLICATION_ORDERED
SESSION_ORDERED
PIPELINE_ORDERED
ENTITY_ORDERED
CUSTOM_KEY_ORDERED
```

Rules:

- ordered modes require an ordering key or derivable identity;
- `sequence` is optional producer-owned monotonic metadata;
- the bus does not invent domain sequence meaning;
- ordering is guaranteed only inside the same active bus instance and lane for the MVP.

---

## 11. Event Metadata

```text
EventMetadata {
    schemaRevision?
    stateVersion?
    entityRevision?
    replay?
    replaySessionId?
    publisherClockSkewClass?
    tags
    extensions
}
```

Rules:

- metadata is bounded;
- extensions must be registered or explicitly tolerated;
- metadata must not contain payload copies;
- metadata must not contain secrets;
- high-cardinality values must not automatically become metric labels.

---

## 12. Event Payload

`TPayload` must:

- be immutable or effectively immutable;
- be typed;
- be serializable when required by a configured adapter;
- be bounded;
- avoid large artifacts;
- avoid mutable SDK objects;
- avoid UI objects;
- avoid provider-native clients;
- avoid secret-bearing types.

Preferred payload style:

```text
Stable IDs
State changes
Revision numbers
Reason codes
Safe summaries
Artifact references
```

---

## 13. Prohibited Payload Types

The bus must reject payloads containing or typed as:

```text
SecretHandle
SecretMaterialInput
SecureBuffer
AuthorizationHeader
PrivateKeyMaterial
PasswordValue
AccessTokenValue
RefreshTokenValue
DecryptedCredential
ProviderSdkClient
PlatformCredentialObject
RawImageBytes
RawDocumentBytes
MutableUiControl
OpenDatabaseConnection
FileStream
UnredactedException
```

---

## 14. Event Type Naming

Canonical event type format:

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

Rules:

- lowercase;
- dot-separated;
- stable;
- fact-oriented;
- owned by one module namespace;
- no transport technology in the name.

---

## 15. Event Version

```text
EventVersion {
    major
    minor?
}
```

The MVP may represent event version as an integer major version.

Compatibility rules:

- additive optional fields may remain within the same compatible version policy;
- field removal requires a new major version;
- semantic meaning change requires a new major version;
- security weakening is prohibited;
- subscribers declare supported versions.

---

## 16. EventContext

```text
EventContext {
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
}
```

A publisher should inherit correlation and causation from the current operation context.

New root workflows create a new correlation ID.

---

# Part I — Publisher Contracts

## 17. EventPublisher

```text
EventPublisher {
    publish<TPayload>(request, cancellationToken)
}
```

`EventPublisher` is the normal publication port.

The publisher does not expose queue or dispatcher internals.

---

## 18. PublishRequest

```text
PublishRequest<TPayload> {
    envelopeDraft
    payload
    options
}
```

`envelopeDraft` contains publisher-supplied event facts except fields owned by the bus, such as `publishedAt`.

---

## 19. EventEnvelopeDraft

```text
EventEnvelopeDraft {
    eventId?
    eventType
    eventVersion
    category
    occurredAt

    sourceModule
    sourceComponent?
    publisherId

    eventContext
    ordering

    priority
    visibility
    securityClassification

    metadata
}
```

The bus may generate `eventId` when omitted.

---

## 20. PublishOptions

```text
PublishOptions {
    mode
    admissionTimeout?
    requireRegisteredType
    requireAtLeastOneSubscriber
    allowProgressCoalescing
    overflowPolicyOverride?
    publicationClass?
}
```

Possible modes:

```text
FIRE_AND_OBSERVE
ENQUEUE_CONFIRMED
DELIVERY_SUMMARY
```

Default:

```text
ENQUEUE_CONFIRMED
```

---

## 21. Publish Admission

Before accepting an event, the bus validates:

```text
bus lifecycle
publisher authorization
event ownership
event registry
event version
envelope fields
payload type
payload size
sensitive-type safety
visibility
security classification
ordering key
queue capacity
shutdown policy
```

---

## 22. PublishReceipt

```text
PublishReceipt {
    eventId
    eventType
    eventVersion

    outcome
    acceptedAt?
    queuedAt?
    laneId?
    subscriberMatchCount?

    coalescedIntoEventId?
    droppedReason?
    rejectionCode?

    deliverySummary?
}
```

Possible outcomes:

```text
ACCEPTED
ACCEPTED_NO_SUBSCRIBERS
COALESCED
FILTERED
REJECTED
DROPPED
BUS_NOT_RUNNING
TIMED_OUT
```

---

## 23. Publish Success Semantics

`ACCEPTED` means:

- envelope validation passed;
- security inspection passed;
- queue admission succeeded.

It does not mean:

- every subscriber handled the event;
- subscriber state changed;
- downstream work succeeded;
- durable delivery occurred;
- exactly-once processing occurred.

---

## 24. Publish Cancellation

Cancellation before queue admission may return canceled/rejected admission.

Cancellation after queue admission:

- does not recall the event by default;
- may stop waiting for a delivery summary;
- does not mutate the immutable event;
- must not violate ordering.

A dedicated administrative cancellation contract is not provided for ordinary events.

---

## 25. Publisher Authorization

```text
PublisherAuthorization {
    publisherId
    allowedNamespaces[]
    allowedCategories[]
    maximumVisibility
    allowedSecurityClassifications[]
    maximumPayloadSize?
}
```

Publishers cannot publish arbitrary module namespaces.

---

## 26. Ownership Rule

Example:

```text
sourceModule = "secret-management"
eventType = "secret-management.secret.revoked"
```

is valid when publisher ownership permits it.

This is invalid:

```text
sourceModule = "translation"
eventType = "secret-management.secret.revoked"
```

unless an explicitly approved bridge owns that publication path.

---

# Part II — Event Registry Contracts

## 27. EventTypeRegistry

```text
EventTypeRegistry {
    register(descriptor)
    unregister(eventType, version)?
    describe(eventType)
    list(filter)
    validate(envelope)
}
```

Runtime unregistration of active event types may be prohibited in the MVP.

---

## 28. EventTypeDescriptor

```text
EventTypeDescriptor {
    eventType
    currentVersion
    supportedVersions[]
    ownerModule
    payloadType

    category
    defaultPriority
    defaultVisibility
    defaultSecurityClassification

    maximumPayloadSize
    orderingRequirement
    coalescingPolicy
    replayPolicy
    retentionClass

    allowedPublishers[]
    allowedSubscriberClearances[]

    schemaReference?
    upcasters[]
}
```

---

## 29. Ordering Requirement

```text
NONE
OPTIONAL
REQUIRED_APPLICATION
REQUIRED_SESSION
REQUIRED_PIPELINE
REQUIRED_ENTITY
REQUIRED_CUSTOM_KEY
```

---

## 30. Coalescing Policy

```text
NOT_ALLOWED
LATEST_BY_ORDERING_KEY
LATEST_BY_ENTITY
CUSTOM_SAFE_COALESCER
```

Only registered progress or observability event types may allow coalescing by default.

---

## 31. Replay Policy

```text
NOT_REPLAYABLE
REPLAYABLE_IDEMPOTENT
REPLAYABLE_WITH_EXPLICIT_MODE
AUDIT_ONLY_REPLAY
```

General replay is deferred, but the contract preserves the boundary.

---

## 32. Retention Class

```text
NONE
EPHEMERAL
OPERATIONAL_SHORT
AUDIT_RESTRICTED
DURABLE_INTEGRATION
```

The in-memory bus normally uses `NONE` or `EPHEMERAL`.

---

## 33. Event Registration Validation

An event type registration must be rejected when:

- ownership conflicts;
- payload type is mutable or unsafe;
- namespace is invalid;
- version already exists with incompatible meaning;
- required ordering metadata is absent;
- coalescing is allowed for a terminal fact;
- security classification is weaker than policy;
- payload limit is invalid;
- publisher or subscriber authorization is contradictory.

---

# Part III — Subscription Contracts

## 34. EventSubscriber

```text
EventSubscriber {
    subscribe<TPayload>(request)
}
```

Returns a `SubscriptionHandle`.

---

## 35. SubscriptionRequest

```text
SubscriptionRequest<TPayload> {
    descriptor
    handler
    filter?
    executionPolicy
}
```

---

## 36. SubscriptionDescriptor

```text
SubscriptionDescriptor {
    subscriptionId?
    subscriberId
    subscriberModule
    subscriberComponent?

    eventType
    supportedVersions[]

    clearance
    deliveryMode
    concurrencyMode

    mandatory
    replaySupport
    shutdownPolicy

    metadata
}
```

---

## 37. Subscriber Clearance

```text
SubscriberClearance {
    allowedVisibilities[]
    allowedSecurityClassifications[]
    allowedSourceModules[]
    allowedCategories[]
}
```

The bus routes only when clearance permits.

---

## 38. Delivery Mode

```text
ASYNC
ASYNC_ORDERED
ASYNC_PARALLEL
SYNCHRONOUS_TEST_ONLY
```

Production default:

```text
ASYNC_ORDERED
```

when an ordering key exists.

---

## 39. Concurrency Mode

```text
SERIAL
ORDERED_BY_KEY
BOUNDED_PARALLEL
```

Unbounded parallel delivery is prohibited.

---

## 40. Subscription Filter

```text
SubscriptionFilter {
    sourceModules?
    categories?
    priorities?
    sessionIds?
    entityTypes?
    metadataPredicates?
}
```

Rules:

- filters use safe envelope fields;
- filters must be deterministic;
- filters must not mutate the event;
- filters must not perform remote I/O;
- filters must not inspect secret-bearing data;
- payload-based filters require explicit registration and bounded evaluation.

---

## 41. EventHandler

```text
EventHandler<TPayload> {
    handle(envelope, deliveryContext, cancellationToken)
        -> DeliveryResult
}
```

A handler must not throw raw provider or platform exceptions across the Event Bus boundary.

---

## 42. DeliveryContext

```text
DeliveryContext {
    deliveryId
    subscriptionId
    subscriberId

    receivedAt
    attemptNumber

    busInstanceId
    laneId?

    isReplay
    isShutdownDrain
    deadline?

    traceContext?
}
```

---

## 43. DeliveryResult

```text
DeliveryResult {
    outcome
    handledAt
    normalizedError?
    retryHint?
    consumerStateReference?
    warnings[]
}
```

Possible outcomes:

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

---

## 44. Delivery Result Semantics

`HANDLED` means the subscriber accepted and completed its reaction.

It does not imply that every downstream action succeeded unless the subscriber contract explicitly defines that meaning.

`IGNORED_STALE` is a valid consumer outcome, not necessarily an error.

`DUPLICATE` is a valid idempotency outcome.

---

## 45. HandlerExecutionPolicy

```text
HandlerExecutionPolicy {
    timeout
    concurrencyLimit
    retryPolicy
    failureThreshold?
    disablePolicy?
    cancellationMode
    shutdownGrace?
}
```

---

## 46. Local Delivery Retry Policy

```text
NONE
ONE_IMMEDIATE_RETRY
BOUNDED_TRANSIENT_RETRY
```

Default:

```text
NONE
```

Retries require idempotent handlers.

---

## 47. Handler Cancellation Mode

```text
COOPERATIVE
TIMEOUT_ONLY
SHUTDOWN_COOPERATIVE
NON_CANCELABLE_NOT_ALLOWED
```

Handlers must honor cooperative cancellation where supported.

---

## 48. SubscriptionHandle

```text
SubscriptionHandle {
    subscriptionId
    subscriberId
    eventType

    pause()
    resume()
    drain()
    dispose()

    status()
}
```

Operations are idempotent where practical.

---

## 49. Subscription Registration Result

```text
SubscriptionRegistrationResult {
    subscriptionId
    outcome
    effectiveAt?
    warnings[]
    rejectionCode?
}
```

Possible outcomes:

```text
REGISTERED
REGISTERED_DEGRADED
REJECTED
DUPLICATE
BUS_NOT_INITIALIZED
```

---

## 50. Mandatory Subscription

A `mandatory` subscriber means its absence or disabled state affects Event Bus health or application readiness.

It does not turn publication into a distributed transaction.

Critical local safety should still not depend solely on asynchronous mandatory delivery.

---

# Part IV — Routing and Ordering Contracts

## 51. Routing Decision

```text
RoutingDecision {
    eventId
    matchedSubscriptions[]
    filteredSubscriptions[]
    unauthorizedSubscriptions[]
    unsupportedVersionSubscriptions[]
    decidedAt
}
```

Detailed unauthorized information may be restricted.

---

## 52. OrderingLaneDescriptor

```text
OrderingLaneDescriptor {
    laneId
    mode
    orderingKey
    capacity
    priorityPolicy
    overflowPolicy
    activeSubscriberCount
}
```

---

## 53. Ordering Key

```text
OrderingKey {
    kind
    value
}
```

Possible kinds:

```text
APPLICATION
SESSION
PIPELINE
TASK
WORK_ITEM
ENTITY
SECRET
PROVIDER
CONFIGURATION
CUSTOM
```

The value must be opaque and safe.

---

## 54. Lane Assignment

```text
assignLane(envelope, eventTypeDescriptor)
    -> OrderingLaneId
```

Lane assignment must be deterministic for the same ordering key in one bus instance.

---

## 55. Scoped Ordering Guarantee

For events accepted into the same lane:

```text
E1 accepted before E2
    → E1 dispatch begins before E2 dispatch begins
```

Stronger completion ordering depends on subscription concurrency mode.

For `SERIAL` or `ORDERED_BY_KEY`:

```text
E1 handler completes or terminates
before E2 handler begins
for the same subscription and key
```

---

## 56. Cross-Subscriber Ordering

The bus does not guarantee that subscriber A completes before subscriber B.

Each subscription receives its own isolated delivery sequence.

---

## 57. Global Ordering

Global ordering is not provided by default.

Application lifecycle events may use a dedicated application-ordered lane.

---

## 58. Sequence Metadata

A producer may include:

```text
sequence
stateVersion
entityRevision
contentRevision
```

Consumers use these to reject stale events.

The bus validates basic monotonic format only when configured.

---

# Part V — Queue Contracts

## 59. QueueAdmissionRequest

```text
QueueAdmissionRequest {
    eventEnvelope
    targetLane
    estimatedSize
    priority
    timeout?
    overflowPolicy
}
```

---

## 60. QueueAdmissionResult

```text
QueueAdmissionResult {
    outcome
    laneId
    queuedAt?
    queueDepthAfter?
    coalescedEventId?
    droppedEventId?
    reasonCode?
}
```

Possible outcomes:

```text
ADMITTED
COALESCED
REJECTED_CAPACITY
DROPPED_LOW_PRIORITY
DROPPED_PROGRESS
TIMED_OUT
BUS_QUIESCING
```

---

## 61. Overflow Policy

```text
REJECT_NEW
DROP_LOWEST_PRIORITY
DROP_OLDEST_PROGRESS
COALESCE_PROGRESS
BLOCK_PUBLISHER_BOUNDED
ESCALATE
```

Policies are selected by event category and lane configuration.

---

## 62. Default Overflow Guidance

```text
DOMAIN / INTEGRATION / RESULT
    → REJECT_NEW or BLOCK_PUBLISHER_BOUNDED

PROGRESS
    → COALESCE_PROGRESS or DROP_OLDEST_PROGRESS

OBSERVABILITY
    → DROP_LOWEST_PRIORITY or sample

SECURITY
    → reserved capacity + ESCALATE

AUDIT
    → dedicated reliable sink
```

---

## 63. Progress Coalescing Request

```text
ProgressCoalescingRequest {
    eventType
    orderingKey
    currentQueuedEvent
    incomingEvent
}
```

Result:

```text
KEEP_CURRENT
REPLACE_WITH_INCOMING
MERGE_SAFE
DO_NOT_COALESCE
```

Coalescing functions must be pure and registered.

---

## 64. Non-Coalescible Facts

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

# Part VI — Delivery Contracts

## 65. DeliveryAttempt

```text
DeliveryAttempt {
    deliveryId
    eventId
    subscriptionId
    attemptNumber
    scheduledAt
    startedAt?
    deadline?
    outcome?
}
```

---

## 66. DeliveryFailure

```text
DeliveryFailure {
    deliveryId
    eventId
    subscriptionId

    normalizedErrorCode
    category
    retryable
    timedOut
    abandoned

    occurredAt
    safeMetadata
}
```

No raw event payload or exception is included.

---

## 67. DeliverySummary

```text
DeliverySummary {
    eventId

    matched
    handled
    ignored
    duplicates
    stale
    failed
    timedOut
    canceled
    abandoned

    completedAt
    mandatorySubscriberFailures[]
}
```

A delivery summary is bounded and safe.

---

## 68. Publication Delivery Summary Mode

When `DELIVERY_SUMMARY` is requested:

- the publisher waits only up to a configured deadline;
- missing optional subscribers do not necessarily fail publication;
- handler failures do not roll back source state;
- the result is diagnostic or administrative;
- this mode must not be used as a distributed transaction.

---

## 69. Subscriber Failure Threshold

```text
SubscriberFailurePolicy {
    window
    threshold
    qualifyingOutcomes[]
    action
}
```

Possible actions:

```text
REPORT_ONLY
MARK_DEGRADED
PAUSE
DISABLE
OPEN_CIRCUIT
ESCALATE
```

---

## 70. Disabled Subscriber Contract

A disabled subscriber:

- receives no new events;
- may remain registered;
- exposes safe status;
- may require administrative recovery;
- must not block other subscribers;
- may affect application health when mandatory.

---

## 71. Diagnostic Failure Buffer

```text
DeliveryFailureRecord {
    deliveryId
    eventId
    eventType
    subscriberId
    normalizedErrorCode
    outcome
    occurredAt
}
```

The buffer is:

- bounded;
- payload-free by default;
- in-memory for MVP;
- not a durable dead-letter queue.

---

# Part VII — Duplicate and Stale Contracts

## 72. Duplicate Detection

The bus may expose:

```text
DuplicateDetector {
    seen(eventId, subscriberId)
    mark(eventId, subscriberId)
}
```

This is an optimization.

Consumers remain responsible for semantic idempotency.

---

## 73. Consumer Deduplication Key

Recommended keys:

```text
eventId
```

or:

```text
entityId + stateVersion + eventType
```

or a module-defined operation identity.

---

## 74. Stale Event Assessment

```text
StaleAssessment {
    relevant
    staleReason?
    observedRevision?
    currentRevision?
}
```

This belongs to the consuming module, not the Event Bus.

---

## 75. Replay Metadata

When replay exists:

```text
ReplayMetadata {
    isReplay
    replaySessionId
    originalPublishedAt
    replayedAt
}
```

Subscribers must explicitly declare replay support.

---

# Part VIII — Security Contracts

## 76. EventPayloadInspector

```text
EventPayloadInspector {
    inspect(envelopeDraft, payload)
        -> PayloadInspectionResult
}
```

---

## 77. PayloadInspectionResult

```text
PayloadInspectionResult {
    safe
    blocked
    findings[]
    estimatedSize
    sanitizedMetadata?
}
```

Possible findings:

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

---

## 78. Security Routing Decision

```text
SecurityRoutingDecision {
    allowed
    publisherAuthorized
    subscriberAuthorized
    visibilityAllowed
    classificationAllowed
    reasonCode?
}
```

---

## 79. Restricted Channel

A restricted channel is a logical routing boundary.

It may use the same physical in-process bus but must enforce:

- explicit subscriber clearance;
- restricted diagnostics;
- payload-safe logs;
- no generic wildcard subscription;
- limited observability.

---

## 80. Wildcard Subscription

Wildcard subscriptions are restricted.

Allowed only for:

- approved observability;
- diagnostics;
- audit routing;
- testing.

They must not receive restricted events unless explicitly cleared.

---

## 81. Payload Logging Contract

The Event Bus does not log full payloads by default.

A payload field may be logged only when:

- explicitly declared safe;
- bounded;
- non-secret;
- non-user-content;
- allowed by visibility policy.

---

# Part IX — Lifecycle Contracts

## 82. EventBusControl

```text
EventBusControl {
    initialize(request)
    start()
    quiesce(request)
    drain(request)
    terminate(request)
    status()
}
```

---

## 83. InitializeRequest

```text
InitializeRequest {
    applicationInstanceId
    registrySnapshot
    queueConfiguration
    securityPolicy
    diagnosticsPolicy
    durableAdapterConfiguration?
}
```

---

## 84. Start Result

```text
EventBusStartResult {
    outcome
    startedAt?
    registeredEventTypeCount
    registeredSubscriptionCount
    degradedComponents[]
}
```

Possible outcomes:

```text
RUNNING
RUNNING_DEGRADED
FAILED
```

---

## 85. QuiesceRequest

```text
QuiesceRequest {
    allowCategories[]
    rejectCategories[]
    effectiveAt
    reasonCode
}
```

During normal shutdown, allowed categories may include:

```text
SECURITY
SYSTEM
AUDIT
```

---

## 86. DrainRequest

```text
DrainRequest {
    timeout
    priorities
    includeProgress
    includeObservability
    cancelHandlersAfterTimeout
    reasonCode
}
```

---

## 87. DrainResult

```text
DrainResult {
    outcome
    startedAt
    completedAt

    delivered
    dropped
    canceled
    abandoned
    remainingQueueDepth

    warnings[]
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

## 88. TerminateRequest

```text
TerminateRequest {
    force
    timeout?
    clearDiagnosticBuffer
    reasonCode
}
```

---

## 89. EventBusStatus

```text
EventBusStatus {
    lifecycleState
    health

    registeredEventTypes
    activeSubscriptions
    pausedSubscriptions
    degradedSubscriptions
    disabledSubscriptions

    totalQueueDepth
    criticalLaneUtilization
    oldestQueuedAge?

    acceptingPublications
    acceptingSubscriptions
    shutdownDeadline?
}
```

---

# Part X — Configuration Contracts

## 90. EventBusConfiguration

```text
EventBusConfiguration {
    enabled

    defaultQueueCapacity
    perLaneCapacity?
    criticalQueueReserve

    maximumPayloadSize
    defaultAdmissionTimeout
    defaultHandlerTimeout
    shutdownDrainTimeout

    defaultOverflowPolicy
    progressThrottleInterval
    progressCoalescingEnabled

    subscriberFailureThreshold
    subscriberFailureWindow
    subscriberDisablePolicy

    diagnosticFailureBufferSize

    localRetryPolicy
    durableAdapterEnabled
}
```

---

## 91. Live Configuration Change

A live configuration update may apply to:

- handler timeout;
- progress throttle;
- diagnostics buffer;
- some concurrency limits;
- metrics settings.

It must not silently invalidate active deliveries.

---

## 92. Restart-Required Configuration

Typically restart-required:

- transport implementation;
- queue topology;
- durable adapter;
- serialization format;
- restricted channel implementation;
- event registry ownership changes.

---

# Part XI — Durable Extension Contracts

## 93. OutboxPublisherPort

```text
OutboxPublisherPort {
    publishPending(batchRequest, cancellationToken)
    markDelivered(recordId, deliveryInfo)
    markFailed(recordId, failure)
}
```

This is a future extension.

---

## 94. OutboxRecord

```text
OutboxRecord {
    outboxRecordId
    eventEnvelope
    createdAt
    attemptCount
    nextEligibleAttemptAt?
    status
}
```

An outbox record still must not contain prohibited payloads.

---

## 95. DurableEventAdapter

```text
DurableEventAdapter {
    publish(envelope)
    subscribe(descriptor, handler)
    acknowledge(deliveryId)
    reject(deliveryId, reason)
    health()
}
```

The core bus does not assume Kafka, RabbitMQ, Redis, NATS, or another transport.

---

## 96. Delivery Guarantee Descriptor

```text
DeliveryGuaranteeDescriptor {
    mode
    duplicatePossible
    orderingScope
    durable
}
```

MVP:

```text
mode = AT_MOST_ONCE
duplicatePossible = true
durable = false
```

Future durable adapter:

```text
mode = AT_LEAST_ONCE
duplicatePossible = true
durable = true
```

---

## 97. Exactly-Once Rule

No contract may claim:

```text
EXACTLY_ONCE
```

unless a future module defines both transport and semantic transaction boundaries.

Current consumers must remain idempotent.

---

# Part XII — Observability Contracts

## 98. EventBusMetricsSnapshot

```text
EventBusMetricsSnapshot {
    publishedByCategory
    rejectedByReason
    droppedByReason
    coalescedCount
    deliveryOutcomes
    queueDepthByLaneClass
    handlerLatencySummary
    queueWaitSummary
    subscriberHealthSummary
    generatedAt
}
```

No event payloads are included.

---

## 99. EventBusDiagnosticsQuery

```text
EventBusDiagnosticsQuery {
    includeRegistry
    includeSubscriptionStatus
    includeQueueStatus
    includeRecentFailures
    includeRestrictedSummary
    callerClearance
}
```

---

## 100. EventBusDiagnosticsResult

```text
EventBusDiagnosticsResult {
    status
    registrySummary?
    subscriptionSummaries[]
    queueSummaries[]
    recentFailures[]
    warnings[]
}
```

Restricted details depend on caller clearance.

---

## 101. SubscriptionStatus

```text
SubscriptionStatus {
    subscriptionId
    subscriberId
    eventType
    lifecycleState
    healthState

    activeDeliveries
    recentFailureCount
    recentTimeoutCount
    lastHandledAt?
    disabledReason?
}
```

---

# Part XIII — Serialization Contracts

## 102. EventSerializer

```text
EventSerializer {
    serialize(envelope)
    deserialize(bytes, descriptor)
}
```

The in-process MVP may not serialize normal delivery.

The contract supports future durable adapters and tests.

---

## 103. Serialization Rules

Serialization must:

- preserve event type and version;
- preserve immutable meaning;
- reject sensitive types;
- enforce size limits;
- avoid runtime-specific object graphs;
- avoid provider SDK types;
- avoid platform handles;
- avoid arbitrary polymorphic deserialization.

---

## 104. Unknown Fields

Consumers should ignore unknown optional fields when compatible.

Unknown required semantics require version rejection.

---

## 105. Unknown Event Type

An unknown event type:

- is rejected by registered-only publication;
- may be captured as a safe compatibility failure;
- must not be dispatched through arbitrary reflection;
- must not be dynamically trusted.

---

# Part XIV — Testing Contracts

## 106. TestEventBus

```text
TestEventBus {
    publish(envelope)
    subscribe(descriptor, handler)
    dispatchNext()
    dispatchAll()
    inspectPublished()
    inspectDeliveries()
    injectFailure()
}
```

---

## 107. RecordingEventBus

Records safe envelope and delivery metadata.

It must not bypass payload-safety checks.

---

## 108. SynchronousTestDispatcher

May execute handlers synchronously for deterministic tests.

It must preserve:

- ordering;
- filtering;
- version checks;
- security checks;
- duplicate behavior.

It must not define production timing semantics.

---

## 109. Fault Injection

Supported faults may include:

```text
queue full
handler timeout
handler failure
subscriber disabled
version mismatch
payload rejected
bus quiescing
shutdown timeout
durable adapter unavailable
```

---

# Part XV — Validation Rules

## 110. Envelope Validation

Reject when:

- event type missing;
- event version invalid;
- occurred time invalid;
- source module missing;
- correlation ID missing when required;
- application instance missing;
- priority invalid;
- visibility invalid;
- security classification invalid;
- payload missing where required;
- ordering metadata incomplete.

---

## 111. Publisher Validation

Reject when:

- publisher identity missing;
- publisher not authorized;
- namespace ownership mismatch;
- category not permitted;
- visibility exceeds authorization;
- payload limit exceeded.

---

## 112. Subscription Validation

Reject when:

- subscriber identity missing;
- event type unregistered;
- no supported versions;
- clearance insufficient;
- concurrency unbounded;
- timeout absent or invalid;
- wildcard scope too broad;
- restricted event requested without clearance;
- handler type incompatible.

---

## 113. Ordering Validation

Reject when:

- required key absent;
- key type conflicts with event descriptor;
- ordering key contains unsafe data;
- partition and ordering requirements contradict;
- custom key type unregistered.

---

## 114. Coalescing Validation

Reject coalescing when:

- event category is terminal/result/security/audit;
- event descriptor forbids it;
- coalescer is non-deterministic;
- coalescer performs I/O;
- merged payload exceeds limits;
- coalescer weakens event meaning.

---

## 115. Delivery Validation

Before invoking a handler:

- subscription still active;
- version still supported;
- clearance still valid;
- event not canceled by shutdown policy;
- lane authority still valid;
- handler deadline established.

---

# Part XVI — Cross-Module Rules

## 116. Configuration

Configuration publishes committed snapshot facts.

Consumers query Configuration for the latest snapshot.

Configuration events must not contain raw secrets.

---

## 117. Secret Management

Secret Management events may use restricted lanes.

The Event Bus must reject secret-bearing types even for restricted events.

---

## 118. Provider Management

Provider events contain normalized provider metadata.

Provider-native clients, requests, responses, and credentials remain internal.

---

## 119. Runtime

Runtime owns work state.

Event handlers may request orchestration through explicit application services, but events do not directly advance pipeline stages.

---

## 120. Presentation

Presentation consumes safe view-relevant events.

UI updates must marshal onto the UI scheduler.

Presentation queries owning modules for complete current state.

---

## 121. Logging and Telemetry

Logging, metrics, and tracing consume safe envelope and delivery metadata.

Recursive telemetry loops must be prevented.

---

# Part XVII — Contract Decisions

## 122. Decisions

### Decision 1 — Typed contracts

Publishers and subscribers use typed payloads.

### Decision 2 — Asynchronous default

Production handlers are asynchronous and not invoked recursively on the publisher stack.

### Decision 3 — Enqueue-confirmed publication

The default receipt confirms queue admission, not downstream success.

### Decision 4 — Scoped ordering

Ordering is lane-based and explicit.

### Decision 5 — Bounded queues

Every queue and failure buffer has a capacity.

### Decision 6 — At-most-once MVP

The in-memory MVP does not promise redelivery.

### Decision 7 — Consumer idempotency

Duplicate-safe handling remains required.

### Decision 8 — Security before queueing

Payload and authorization validation occur before admission.

### Decision 9 — No secret payloads

No visibility or adapter mode permits secrets.

### Decision 10 — No implicit orchestration

Events report facts; orchestrators issue commands.

### Decision 11 — No distributed transaction

Delivery summary cannot roll back committed source state.

### Decision 12 — Durable extension by adapter

Outbox and at-least-once delivery are future ports.

---

# Part XVIII — Open Decisions

## 123. Contract Decisions

Still to finalize:

- exact language-level interface names;
- whether event version is integer or semantic pair;
- event ID generation policy;
- exact metadata extension type;
- exact mandatory-subscriber readiness behavior;
- exact wildcard subscription restrictions;
- exact publication summary timeout behavior.

---

## 124. Queue Decisions

Still to finalize:

- default queue capacity;
- per-session lane creation and disposal;
- critical reserve size;
- queue fairness;
- starvation prevention;
- lane idle expiration;
- publisher blocking timeout;
- queue memory accounting.

---

## 125. Delivery Decisions

Still to finalize:

- default handler timeout;
- default subscriber concurrency;
- one-retry eligibility;
- disabled subscriber recovery;
- mandatory subscriber degradation;
- shutdown abandonment mapping;
- delivery summary limits.

---

## 126. Security Decisions

Still to finalize:

- publisher identity establishment;
- subscriber clearance source;
- restricted lane implementation;
- payload inspector integration;
- event schema review process;
- audit sink wiring;
- user-content classification.

---

## 127. Durable Decisions

Still to finalize:

- outbox storage;
- acknowledgment model;
- replay boundary;
- dead-letter store;
- adapter health;
- retry backoff;
- cross-process transport;
- schema registry.

---

# Part XIX — Documentation Order

## 128. Recommended Order

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

`STATES.md` should next define:

- Event Bus lifecycle;
- subscription lifecycle;
- subscriber health;
- queue lane lifecycle;
- delivery attempt lifecycle;
- dispatcher lifecycle;
- outbox adapter lifecycle;
- valid shutdown transitions;
- degraded and failed behavior.

---

# Part XX — Related Documents

## 129. Related Documents

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

03-infrastructure/event-bus/MODULE.md

03-infrastructure/configuration/EVENTS.md

03-infrastructure/secret-management/EVENTS.md

02-modules/provider-management/EVENTS.md
```

Future Event Bus documents:

```text
03-infrastructure/event-bus/STATES.md
03-infrastructure/event-bus/EVENTS.md
03-infrastructure/event-bus/ERRORS.md
03-infrastructure/event-bus/README.md
```

---

## 130. Summary

The Event Bus contract defines a safe, typed, bounded, asynchronous communication boundary.

The primary publication flow is:

```text
PublishRequest
    ↓
Publisher authorization
    ↓
Envelope and registry validation
    ↓
Payload safety inspection
    ↓
Ordering lane selection
    ↓
Bounded queue admission
    ↓
PublishReceipt
```

The primary delivery flow is:

```text
Queued Event
    ↓
Subscription routing
    ↓
Version and clearance validation
    ↓
Isolated handler execution
    ↓
DeliveryResult
    ↓
Safe delivery observability
```

The contract guarantees:

- immutable typed events;
- explicit ownership;
- scoped ordering;
- bounded queues;
- subscriber isolation;
- payload safety;
- no secret transport;
- no implicit orchestration;
- at-most-once MVP semantics;
- duplicate-aware consumers;
- bounded shutdown;
- future durable extension without changing event meaning.

This document is the contract source of truth for subsequent Event Bus states, events, errors, and implementation documentation.
