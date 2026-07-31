# Recognition Module States

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `doc/02-modules/recognition/STATES.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-22

---

## 1. Purpose

This document defines the state model of the Recognition module.

It specifies:

* request lifecycle states;
* provider lifecycle states;
* module-level availability states;
* valid state transitions;
* transition triggers;
* transition guards;
* terminal states;
* cancellation behavior;
* timeout behavior;
* retry behavior;
* fallback behavior;
* concurrency constraints;
* state invariants.

This document focuses on state ownership and state transitions.

Related contracts are defined in:

```text
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/EVENTS.md
doc/02-modules/recognition/MODULE.md
```

---

## 2. State Ownership

Recognition owns the following state categories:

```text
Recognition Module State
Recognition Provider State
Recognition Request State
Recognition Attempt State
Recognition Result State
```

Recognition does not own:

```text
Reading Session State
Source Lifecycle State
Observation State
Current Frame Selection
Translation State
Presentation State
Storage Retention State
```

Recognition may react to external state changes but must not become their source of truth.

---

## 3. State Model Overview

```text
Recognition Module
├── Module Availability State
├── Provider Registry State
│   └── Provider Lifecycle State
├── Active Request Registry
│   └── Recognition Request State
│       └── Recognition Attempt State
└── Completed Result Registry
    └── Recognition Result State
```

Each request is isolated by:

```text
request_id
```

Each completed immutable result is isolated by:

```text
recognition_id
```

---

## 4. State Machine Principles

### 4.1 State Transitions Are Explicit

State changes must occur through documented transitions.

Implementation code must not mutate request state arbitrarily.

---

### 4.2 Terminal States Are Final

Terminal request states are:

```text
Completed
Failed
Cancelled
```

A request cannot leave a terminal state.

---

### 4.3 Exactly One Terminal State

Every accepted request must end in exactly one terminal state.

---

### 4.4 Provider State and Request State Are Separate

A provider can become degraded while an existing request remains active.

A request failure does not automatically make the provider unavailable.

---

### 4.5 Session Relevance Is External

A completed Recognition result may be stale for the current session.

That does not change its Recognition state from `Completed`.

---

### 4.6 Results Are Immutable

A completed result cannot transition back into processing.

A retry creates a new request and later a new result.

---

## 5. Module Availability State

The Recognition module has one top-level availability state.

```text
RecognitionModuleState
├── Uninitialized
├── Initializing
├── Ready
├── Degraded
├── Unavailable
├── ShuttingDown
└── Stopped
```

---

## 6. Module State Definitions

### 6.1 `Uninitialized`

Recognition has not started initialization.

Characteristics:

* no providers loaded;
* no requests accepted;
* configuration may not yet be validated;
* active-request registry is unavailable.

Allowed next states:

```text
Initializing
Stopped
```

---

### 6.2 `Initializing`

Recognition is validating configuration and preparing provider infrastructure.

Possible activities:

* loading configuration;
* building provider registry;
* checking provider capabilities;
* loading local models;
* checking runtime dependencies;
* initializing cancellation registry;
* initializing schedulers.

Allowed next states:

```text
Ready
Degraded
Unavailable
ShuttingDown
```

---

### 6.3 `Ready`

Recognition can accept normal requests.

Requirements:

* configuration is valid;
* scheduler is available;
* at least one eligible provider is ready;
* request registry is operational;
* result publication path is operational.

Allowed next states:

```text
Degraded
Unavailable
ShuttingDown
```

---

### 6.4 `Degraded`

Recognition can accept only some requests or must use reduced capability.

Examples:

* GPU provider unavailable but CPU provider available;
* vertical-text provider unavailable;
* remote provider unavailable but local provider ready;
* result store unavailable for asynchronous large results;
* provider latency exceeds threshold;
* only one provider remains healthy.

Allowed next states:

```text
Ready
Unavailable
ShuttingDown
```

Request acceptance depends on capability-specific guards.

---

### 6.5 `Unavailable`

Recognition cannot accept useful requests.

Examples:

* no eligible provider available;
* critical configuration invalid;
* scheduler failed;
* result registration unavailable;
* provider registry failed;
* required local runtime unavailable.

Allowed next states:

```text
Initializing
Degraded
Ready
ShuttingDown
```

New requests must be rejected with a normalized error.

---

### 6.6 `ShuttingDown`

Recognition is stopping.

Behavior:

* rejects new requests;
* requests cancellation of active work;
* releases providers;
* flushes terminal events where practical;
* performs bounded cleanup.

Allowed next state:

```text
Stopped
```

No transition back to `Ready` is allowed during the same shutdown sequence.

---

### 6.7 `Stopped`

Recognition has released runtime resources.

Characteristics:

* no requests accepted;
* providers stopped;
* schedulers stopped;
* active request registry empty;
* transient buffers released.

Allowed next state:

```text
Initializing
```

only through a new startup sequence.

---

## 7. Module State Diagram

```text
Uninitialized
      ↓
Initializing
   ┌──┼───────────────┐
   ↓  ↓               ↓
Ready Degraded    Unavailable
  ↕      ↕            ↕
  └──────┴────────────┘
          ↓
    ShuttingDown
          ↓
       Stopped
```

More precisely:

```text
Uninitialized → Initializing

Initializing → Ready
Initializing → Degraded
Initializing → Unavailable
Initializing → ShuttingDown

Ready → Degraded
Ready → Unavailable
Ready → ShuttingDown

Degraded → Ready
Degraded → Unavailable
Degraded → ShuttingDown

Unavailable → Initializing
Unavailable → Degraded
Unavailable → Ready
Unavailable → ShuttingDown

ShuttingDown → Stopped

Stopped → Initializing
```

---

# Provider State

## 8. Provider Lifecycle State

Each Recognition provider has an independent state.

```text
RecognitionProviderState
├── Unregistered
├── Registered
├── Initializing
├── Ready
├── Degraded
├── Unavailable
├── Misconfigured
├── ShuttingDown
└── Stopped
```

---

## 9. Provider State Definitions

### 9.1 `Unregistered`

The provider is not present in the active registry.

No request may select it.

---

### 9.2 `Registered`

The provider adapter has been discovered and registered but is not initialized.

Allowed next states:

```text
Initializing
Misconfigured
Stopped
```

---

### 9.3 `Initializing`

The provider is preparing required resources.

Examples:

* loading model files;
* creating API clients;
* allocating GPU memory;
* checking credentials;
* performing health checks.

Allowed next states:

```text
Ready
Degraded
Unavailable
Misconfigured
ShuttingDown
```

---

### 9.4 `Ready`

The provider is operational for its declared capabilities.

This does not guarantee it is suitable for every request.

Selection still depends on:

* language;
* script;
* orientation;
* mode;
* privacy policy;
* execution device;
* image limits.

---

### 9.5 `Degraded`

The provider remains usable with reduced capability or quality of service.

Examples:

* GPU fallback to CPU;
* increased latency;
* line geometry unavailable;
* language model partially unavailable;
* cancellation unsupported;
* remote rate limit approaching.

The degraded capabilities must be exposed separately.

---

### 9.6 `Unavailable`

The provider cannot process requests at the moment.

Possible causes:

* service outage;
* local runtime crash;
* model loading failure;
* network unavailable;
* resource exhaustion;
* unhealthy process.

The provider may later recover.

---

### 9.7 `Misconfigured`

The provider configuration is invalid.

Examples:

* missing model path;
* invalid API credentials;
* unsupported device setting;
* incompatible runtime version;
* missing dependency.

Automatic retries should be limited.

Configuration change is normally required.

---

### 9.8 `ShuttingDown`

The provider is releasing resources.

New requests must not select it.

Existing requests may:

* finish within a bounded deadline;
* be cancelled;
* have their late output discarded.

---

### 9.9 `Stopped`

The provider has released runtime resources.

It cannot process requests until reinitialized.

---

## 10. Provider State Diagram

```text
Unregistered
     ↓
Registered
     ↓
Initializing
 ┌────┼──────────────┬────────────────┐
 ↓    ↓              ↓                ↓
Ready Degraded   Unavailable     Misconfigured
 ↕      ↕            ↕                │
 └──────┴────────────┘                │
          ↓                           │
     ShuttingDown ←───────────────────┘
          ↓
        Stopped
```

---

## 11. Provider Selection Guards

A provider may be selected only when:

```text
provider.state ∈ {Ready, Degraded}
```

and all request requirements are satisfied.

Additional guards:

```text
provider supports requested media type
provider supports requested mode
provider supports required language or script
provider supports required orientation
provider satisfies local-only policy
provider is not excluded
provider image limits are not exceeded
provider concurrency capacity is available
provider is not shutting down
```

A degraded provider may be selected only if its degraded capabilities still satisfy the request.

---

# Request State

## 12. Recognition Request Lifecycle

Each request has the following state model:

```text
RecognitionRequestState
├── Received
├── Validating
├── Rejected
├── Queued
├── SelectingProvider
├── Preparing
├── Preprocessing
├── Detecting
├── Recognizing
├── PostProcessing
├── ResolvingReadingOrder
├── MappingCoordinates
├── AssemblingResult
├── PublishingResult
├── Cancelling
├── Completed
├── Failed
└── Cancelled
```

---

## 13. Request State Categories

### Pre-Execution

```text
Received
Validating
Rejected
Queued
SelectingProvider
```

### Active Processing

```text
Preparing
Preprocessing
Detecting
Recognizing
PostProcessing
ResolvingReadingOrder
MappingCoordinates
AssemblingResult
PublishingResult
```

### Cancellation

```text
Cancelling
Cancelled
```

### Terminal

```text
Rejected
Completed
Failed
Cancelled
```

`Rejected` is terminal but represents a request that was never accepted for execution.

For event semantics, a rejected asynchronous request may still publish `recognition.failed`.

---

## 14. `Received`

The request has entered the Recognition boundary.

No processing guarantee exists yet.

Stored state may include:

```text
request_id
received_at
request_context
image_reference
options
```

Allowed next state:

```text
Validating
```

---

## 15. `Validating`

Recognition validates:

* contract version;
* identifiers;
* image reference;
* image dimensions;
* coordinate space;
* region bounds;
* timeout;
* provider policy;
* privacy policy;
* module availability.

Allowed next states:

```text
Rejected
Queued
SelectingProvider
Cancelling
```

Direct transition to `SelectingProvider` is allowed when queueing is unnecessary.

---

## 16. `Rejected`

The request cannot be accepted.

Examples:

* invalid contract;
* invalid image;
* unsupported major version;
* invalid region;
* contradictory privacy policy;
* module unavailable;
* duplicate active request ID.

Properties:

* no provider execution occurred;
* no recognition result exists;
* rejection reason is normalized;
* state is terminal.

---

## 17. `Queued`

The request has been accepted but is waiting for execution capacity.

Possible reasons:

* provider concurrency limit;
* scheduler priority;
* GPU serialization;
* memory pressure;
* earlier interactive request;
* model initialization.

Allowed next states:

```text
SelectingProvider
Preparing
Cancelling
Failed
```

Timeout policy must define whether queue time counts toward request timeout.

---

## 18. `SelectingProvider`

Recognition evaluates provider eligibility.

Activities:

* filter providers;
* evaluate required capabilities;
* evaluate privacy constraints;
* evaluate execution device;
* evaluate provider health;
* evaluate fallback policy;
* reserve capacity.

Allowed next states:

```text
Preparing
Queued
Failed
Cancelling
```

If no provider is eligible:

```text
Failed
```

with:

```text
NoEligibleProvider
```

---

## 19. `Preparing`

Recognition resolves and normalizes image input.

Activities:

* resolve image reference;
* validate image checksum;
* decode image;
* create request-scoped buffers;
* initialize transform chain;
* build preprocessing plan.

Allowed next states:

```text
Preprocessing
Detecting
Recognizing
Failed
Cancelling
```

Direct transition to `Detecting` or `Recognizing` is allowed when preprocessing is unnecessary.

---

## 20. `Preprocessing`

Recognition applies configured image transformations.

Examples:

* resize;
* upscale;
* grayscale;
* contrast adjustment;
* denoise;
* deskew;
* rotation;
* threshold.

Allowed next states:

```text
Detecting
Recognizing
Failed
Cancelling
```

Every geometry-changing operation must update the transform chain before leaving this state.

---

## 21. `Detecting`

Recognition detects text regions.

This state may be skipped when:

* the request is a direct single-region request;
* the provider performs combined detection and recognition;
* the provider accepts the whole image directly.

Allowed next states:

```text
Recognizing
PostProcessing
Failed
Cancelling
```

`PostProcessing` may follow when combined OCR already returned text.

---

## 22. `Recognizing`

Recognition converts image regions into text.

Possible execution models:

```text
single combined OCR call
multiple region OCR calls
batched region OCR
provider streaming
provider ensemble
```

Allowed next states:

```text
PostProcessing
Failed
Cancelling
```

Cancellation must be checked between region operations where practical.

---

## 23. `PostProcessing`

Recognition normalizes provider output.

Activities may include:

* provider type conversion;
* deterministic surface cleanup;
* duplicate suppression;
* invalid-region filtering;
* region merging when non-semantic;
* confidence normalization;
* warning generation.

Allowed next states:

```text
ResolvingReadingOrder
MappingCoordinates
AssemblingResult
Failed
Cancelling
```

Semantic text correction is forbidden in this state.

---

## 24. `ResolvingReadingOrder`

Recognition computes initial reading order.

Activities:

* preserve provider order;
* apply spatial rules;
* apply orientation rules;
* create explicit order entries;
* generate uncertainty warnings.

Allowed next states:

```text
MappingCoordinates
AssemblingResult
Failed
Cancelling
```

This state may be skipped only when:

* no regions exist; or
* order is already valid and explicitly supplied.

---

## 25. `MappingCoordinates`

Recognition maps processed geometry back to source coordinate space.

Activities:

* apply inverse transforms;
* validate geometry bounds;
* map line geometry;
* map polygon geometry;
* record inferred geometry.

Allowed next states:

```text
AssemblingResult
Failed
Cancelling
```

A mapping failure must not publish a result with untrusted public geometry.

---

## 26. `AssemblingResult`

Recognition builds the immutable result object.

Activities:

* assign `recognition_id`;
* assemble provider identity;
* attach warnings;
* attach metrics;
* validate identifiers;
* validate reading order;
* validate geometry;
* calculate counts;
* finalize timestamps.

Allowed next states:

```text
PublishingResult
Completed
Failed
Cancelling
```

Direct `Completed` is allowed for synchronous in-process execution without result registration or Event Bus publication.

---

## 27. `PublishingResult`

Recognition registers or stores the result and publishes the terminal completion event.

Activities:

* register result reference;
* persist temporary result when needed;
* publish `recognition.completed`;
* record publication outcome.

Allowed next states:

```text
Completed
Failed
Cancelling
```

If terminal publication fails transiently, the state may remain here while retrying publication with the same event identity.

OCR must not be rerun solely because publication failed.

---

## 28. `Cancelling`

Cancellation has been accepted and termination is in progress.

Activities:

* set cancellation flag;
* stop scheduler work;
* invoke provider cancellation if supported;
* discard late provider output;
* release request resources;
* publish cancellation terminal event.

Allowed next states:

```text
Cancelled
```

A request in `Cancelling` must not transition to:

```text
Completed
Failed
```

unless cancellation acceptance itself is rolled back before becoming effective, which is not recommended.

---

## 29. `Completed`

Recognition successfully produced and exposed an immutable result.

Properties:

* `recognition_id` exists;
* exactly one completion terminal outcome exists;
* result is immutable;
* request resources are released;
* provider reservation is released;
* late cancellation has no effect.

Terminal state.

---

## 30. `Failed`

Recognition could not produce an acceptable result.

Properties:

* normalized error exists;
* exactly one failure terminal outcome exists;
* no valid completed result is exposed;
* request resources are released;
* retry may create a new request.

Terminal state.

---

## 31. `Cancelled`

Recognition work is no longer active and no completion will be published.

Properties:

* cancellation reason exists;
* request resources are released or scheduled for bounded cleanup;
* provider may have been interrupted or merely detached;
* late provider output is ignored;
* retry requires a new request.

Terminal state.

---

# Request Transition Model

## 32. Primary Successful Path

```text
Received
    ↓
Validating
    ↓
Queued
    ↓
SelectingProvider
    ↓
Preparing
    ↓
Preprocessing
    ↓
Detecting
    ↓
Recognizing
    ↓
PostProcessing
    ↓
ResolvingReadingOrder
    ↓
MappingCoordinates
    ↓
AssemblingResult
    ↓
PublishingResult
    ↓
Completed
```

---

## 33. Simplified Combined OCR Path

```text
Received
    ↓
Validating
    ↓
SelectingProvider
    ↓
Preparing
    ↓
Preprocessing
    ↓
Recognizing
    ↓
PostProcessing
    ↓
ResolvingReadingOrder
    ↓
MappingCoordinates
    ↓
AssemblingResult
    ↓
Completed
```

`Detecting` is skipped because the provider performs combined OCR.

---

## 34. Single Region Path

```text
Received
    ↓
Validating
    ↓
SelectingProvider
    ↓
Preparing
    ↓
Preprocessing
    ↓
Recognizing
    ↓
PostProcessing
    ↓
MappingCoordinates
    ↓
AssemblingResult
    ↓
Completed
```

Page-level detection and reading-order resolution may be unnecessary.

---

## 35. Empty Result Path

```text
Received
    ↓
Validating
    ↓
SelectingProvider
    ↓
Preparing
    ↓
Preprocessing
    ↓
Detecting
    ↓
PostProcessing
    ↓
AssemblingResult
    ↓
Completed
```

Final result:

```text
regions = []
reading_order = []
warning = NoReadableTextDetected
```

This is not a failure.

---

## 36. Validation Failure Path

```text
Received
    ↓
Validating
    ↓
Rejected
```

Possible reasons:

```text
InvalidRequest
DuplicateRequestId
InvalidImageReference
InvalidCoordinateSpace
InvalidRegion
Unsupported contract version
Invalid provider policy
```

---

## 37. Processing Failure Path

Any active processing state may transition to `Failed`.

Example:

```text
Recognizing
    ↓ ProviderTimeout
Failed
```

General failure transitions:

```text
Queued → Failed
SelectingProvider → Failed
Preparing → Failed
Preprocessing → Failed
Detecting → Failed
Recognizing → Failed
PostProcessing → Failed
ResolvingReadingOrder → Failed
MappingCoordinates → Failed
AssemblingResult → Failed
PublishingResult → Failed
```

---

## 38. Cancellation Path

Cancellation may be requested from any non-terminal state.

```text
Received → Cancelling
Validating → Cancelling
Queued → Cancelling
SelectingProvider → Cancelling
Preparing → Cancelling
Preprocessing → Cancelling
Detecting → Cancelling
Recognizing → Cancelling
PostProcessing → Cancelling
ResolvingReadingOrder → Cancelling
MappingCoordinates → Cancelling
AssemblingResult → Cancelling
PublishingResult → Cancelling
```

Then:

```text
Cancelling → Cancelled
```

Cancellation during terminal publication requires atomic terminal-state coordination.

---

## 39. Terminal Race Rules

Possible races:

```text
Completion vs Cancellation
Failure vs Cancellation
Timeout vs Provider Completion
Shutdown vs Completion
```

The first successfully committed terminal transition wins.

Conceptual operation:

```text
compare_and_set(
    current_state ∈ non_terminal_states,
    target_terminal_state
)
```

Once committed:

```text
Completed
Failed
Cancelled
```

all competing terminal transitions must be rejected.

---

## 40. Completion and Cancellation Race

Example:

```text
Provider returns result
Cancellation request arrives
```

Rule:

* if completion terminal transition commits first, request becomes `Completed`;
* if cancellation commits first, request becomes `Cancelled`;
* no second terminal event may be published.

The state commit and terminal event intent should be recorded atomically where practical.

---

# Attempt State

## 41. Recognition Attempt Model

One request may contain multiple provider attempts when fallback is allowed.

```text
RecognitionAttemptState
├── Pending
├── Starting
├── Running
├── Succeeded
├── Failed
├── Cancelled
└── Discarded
```

Each attempt is identified by:

```text
request_id + attempt_number
```

---

## 42. Attempt State Definitions

### `Pending`

Attempt is planned but has not started.

### `Starting`

Provider capacity is reserved and execution is being prepared.

### `Running`

Provider processing is active.

### `Succeeded`

Provider produced a valid candidate output.

This does not necessarily mean the full request is completed.

### `Failed`

Provider attempt failed.

Fallback may still continue.

### `Cancelled`

Provider execution was interrupted.

### `Discarded`

Provider produced or may later produce output that must not be used.

Examples:

* request already cancelled;
* newer fallback attempt already won;
* timeout terminal state already committed;
* provider output arrived too late.

---

## 43. Attempt Transition Diagram

```text
Pending
   ↓
Starting
   ↓
Running
 ┌─┼───────────────┐
 ↓ ↓               ↓
Succeeded        Failed
                  ↓
             next attempt

Running → Cancelled
Running → Discarded
Succeeded → Discarded
```

A successful attempt may still be discarded when the request terminal state was already committed elsewhere.

---

## 44. Provider Fallback State Flow

```text
Request: SelectingProvider
        ↓
Attempt 1: Running
        ↓
Attempt 1: Failed
        ↓
Fallback Allowed?
   ┌────┴─────┐
   │          │
  No         Yes
   │          │
Request     Attempt 2
Failed       Running
                ↓
             Succeeded
                ↓
         Request continues
```

Internal attempt failures must not emit public terminal failure when fallback remains active.

---

## 45. Fallback Guards

Fallback is allowed only when:

```text
fallback_allowed = true
attempt_count <= maximum_fallback_count
error.retryable = true
another eligible provider exists
privacy policy remains satisfied
timeout budget remains
request is not cancelling
module is not shutting down
```

Fallback must not silently change:

* local-only requirement;
* remote-processing permission;
* required language;
* required orientation;
* requested mode.

---

# Result State

## 46. Recognition Result Lifecycle

Recognition results have a simpler state model.

```text
RecognitionResultState
├── Building
├── Validating
├── Registered
├── Available
├── Expired
├── Evicted
└── Invalid
```

---

## 47. Result State Definitions

### 47.1 `Building`

The result object is being assembled.

It is not visible to consumers.

---

### 47.2 `Validating`

The result is undergoing contract validation.

Checks include:

* geometry;
* IDs;
* reading order;
* confidence;
* metrics;
* provider identity;
* timestamps.

---

### 47.3 `Registered`

The immutable result has been accepted by the result registry or temporary store.

A completion event may now safely reference it.

---

### 47.4 `Available`

The result can be retrieved by authorized consumers.

Recognition does not define how long availability lasts.

---

### 47.5 `Expired`

The result reference has passed its lifetime.

The immutable result may have been removed.

Consumers should not assume it remains retrievable.

---

### 47.6 `Evicted`

The result was removed before normal expiry.

Possible causes:

* cache pressure;
* storage policy;
* session cleanup;
* privacy cleanup;
* explicit invalidation.

---

### 47.7 `Invalid`

Result validation failed.

An invalid result must never be published as completed.

---

## 48. Result State Diagram

```text
Building
   ↓
Validating
 ┌─┴────────┐
 ↓          ↓
Registered Invalid
   ↓
Available
 ┌─┴───────┐
 ↓         ↓
Expired  Evicted
```

`Invalid`, `Expired`, and `Evicted` are terminal for that result instance.

---

## 49. Result Availability and Request Completion

A request may transition to `Completed` only when:

```text
result.state ∈ {Registered, Available}
```

for reference-based asynchronous workflows.

A request must not become `Completed` when:

```text
result.state = Invalid
```

or when no valid result reference can be produced.

---

# Transition Guards and Actions

## 50. State Transition Record

Every meaningful request transition should produce an internal transition record.

```text
RecognitionStateTransition
├── request_id
├── previous_state
├── next_state
├── trigger
├── occurred_at
├── attempt?
├── provider_id?
├── trace_id
└── metadata?
```

These records are internal diagnostics.

They are not necessarily public events.

---

## 51. Transition Triggers

Possible triggers:

```text
RequestReceived
ValidationPassed
ValidationFailed
SchedulerQueued
ExecutionCapacityAvailable
ProviderSelected
ImageResolved
PreprocessingCompleted
RegionsDetected
RecognitionCompleted
PostProcessingCompleted
ReadingOrderResolved
CoordinatesMapped
ResultAssembled
ResultRegistered
CompletionPublished
FailureOccurred
CancellationRequested
CancellationAccepted
ProviderCancelled
TimeoutExpired
ShutdownRequested
ProviderUnavailable
FallbackSelected
```

---

## 52. Transition Actions

State transitions may execute actions such as:

```text
reserve provider capacity
register cancellation token
start stage timer
publish lifecycle event
release image buffer
release provider capacity
discard late provider output
register result reference
normalize error
record metrics
schedule fallback
```

Actions must be idempotent where transition retries are possible.

---

## 53. Stage Entry Rules

On entering an active processing state:

1. check cancellation;
2. check request timeout;
3. record stage start time;
4. validate required previous outputs;
5. verify request remains non-terminal.

---

## 54. Stage Exit Rules

Before leaving an active processing state:

1. store stage output;
2. record duration;
3. validate stage output;
4. check cancellation;
5. release stage-local resources when no longer needed;
6. determine the next state explicitly.

---

# Timeouts

## 55. Timeout State Behavior

Timeout is not a dedicated request state.

It is a trigger that normally causes:

```text
Current Non-Terminal State
        ↓
Cancelling
        ↓
Cancelled
```

or:

```text
Current Non-Terminal State
        ↓
Failed
```

depending on timeout policy.

Recommended behavior:

```text
provider timeout → Failed
request supersession timeout → Cancelled
shutdown deadline → Cancelled
```

---

## 56. Timeout Sources

```text
QueueTimeout
RequestDeadline
ProviderTimeout
StageTimeout
ShutdownDeadline
ResourceWaitTimeout
```

Each timeout must record its source.

---

## 57. Timeout Guards

Before every expensive stage:

```text
remaining_timeout_budget > minimum_stage_budget
```

If insufficient:

```text
Failed(RequestExpired)
```

or:

```text
Cancelled(Timeout)
```

must occur before starting the stage.

---

# Retry State

## 58. Retry Is Not a State Transition

A retry does not move a terminal request back to an active state.

Incorrect:

```text
Failed → Recognizing
```

Correct:

```text
Request A: Failed

Request B:
Received
    ↓
Validating
    ↓
...
```

A retry must create:

```text
new request_id
new terminal lifecycle
new recognition_id on success
```

---

## 59. Retry Relationship

A retry request may reference:

```text
previous_request_id
previous_recognition_id
retry_reason
retry_scope
```

The original request and result remain unchanged.

---

# Concurrency State

## 60. Active Request Registry

Recognition maintains a bounded registry.

```text
ActiveRecognitionRequest
├── request_id
├── current_state
├── current_attempt
├── provider_id?
├── cancellation_status
├── deadline?
├── priority
├── acquired_resources[]
└── transition_version
```

---

## 61. Transition Version

Each request may maintain a monotonic transition version.

```text
transition_version = integer
```

It supports:

* compare-and-set transitions;
* race detection;
* duplicate callback suppression;
* terminal-state protection.

---

## 62. Concurrency Rules

1. State changes for one request must be serialized logically.
2. Provider callbacks must verify current request state.
3. Late callbacks must not overwrite terminal state.
4. One request may own only one active terminal transition.
5. Fallback attempts may overlap only when explicitly designed.
6. Result assembly must use one winning attempt.
7. Cancellation must be visible to all active stages.
8. Provider capacity must be released exactly once.
9. Image buffers must be released exactly once.
10. Terminal events must be emitted exactly once.

---

## 63. Duplicate Callback Handling

Provider SDKs may invoke callbacks more than once.

Recognition must ignore duplicate callbacks after:

```text
attempt.state ∈ {
    Succeeded,
    Failed,
    Cancelled,
    Discarded
}
```

A duplicate callback must not change request state.

---

# State and Events

## 64. State-to-Event Mapping

| State transition                        | Public event                       |
| --------------------------------------- | ---------------------------------- |
| Accepted execution → first active stage | `recognition.started`              |
| Any non-terminal state → `Completed`    | `recognition.completed`            |
| Any non-terminal state → `Failed`       | `recognition.failed`               |
| Any non-terminal state → `Cancelled`    | `recognition.cancelled`            |
| Provider → `Ready`                      | `recognition.provider_ready`       |
| Provider → `Degraded`                   | `recognition.provider_degraded`    |
| Provider → `Unavailable`                | `recognition.provider_unavailable` |

Internal stage transitions do not require public events.

---

## 65. Event Publication Does Not Define State Alone

State must be committed before or atomically with terminal event intent.

Incorrect:

```text
publish recognition.completed
then set state = Completed
```

Risk:

* cancellation may win between publication and state mutation.

Preferred:

```text
commit terminal state and event record
then publish event idempotently
```

---

## 66. Progress Events and State

Progress events may correspond to stage exits:

```text
Preprocessing → recognition.preprocessing_completed
Detecting → recognition.regions_detected
Recognizing region → recognition.region_recognized
ResolvingReadingOrder → recognition.reading_order_resolved
```

These events are optional and do not alter the public lifecycle guarantees.

---

# Recovery

## 67. Process Recovery

After an application crash, in-memory requests may be lost.

Recovery policy depends on deployment mode.

For desktop MVP:

```text
active in-memory request → considered interrupted
temporary image buffer → cleaned on startup
temporary result → validated or removed
provider model → reinitialized
```

Recognition should not automatically recreate old requests unless orchestration explicitly resubmits them.

---

## 68. Orphan Request Detection

On startup, persisted request records in non-terminal states may be marked:

```text
Failed
```

with:

```text
InternalError
ProcessInterrupted
```

if persistent request tracking is later introduced.

For MVP, persistent active-request state is not required.

---

## 69. Orphan Result Detection

A stored result without a valid completion record may be:

* retained temporarily for diagnostics;
* registered during recovery if validation succeeds;
* removed according to cleanup policy.

Recognition must not publish a new completion event without an idempotent recovery design.

---

# State Persistence

## 70. Persistent Versus Ephemeral State

### Ephemeral

```text
current request stage
image buffers
provider request handle
cancellation token
stage-local output
GPU resource handle
active timer
```

### Potentially Persistent

```text
completed RecognitionResult
terminal outcome record
event publication record
provider configuration version
benchmark results
diagnostic references
```

The MVP should keep active processing state ephemeral.

---

## 71. State Snapshot

An internal request snapshot may be represented as:

```text
RecognitionRequestSnapshot
├── request_id
├── state
├── state_version
├── attempt
├── provider_id?
├── started_at?
├── deadline?
├── cancellation_requested
├── terminal_outcome?
├── result_id?
├── error_code?
└── updated_at
```

Snapshots must not contain raw image bytes or complete recognized text.

---

# Invalid Transitions

## 72. Invalid Transition Examples

The following transitions are forbidden:

```text
Completed → Recognizing
Completed → Cancelled
Failed → Completed
Failed → Recognizing
Cancelled → Completed
Cancelled → Failed
Rejected → Queued
PublishingResult → Recognizing
Stopped → Ready
ShuttingDown → Ready
Provider Stopped → Ready without initialization
Result Invalid → Available
Result Expired → Available
```

---

## 73. Invalid Transition Handling

When invalid transition is attempted:

1. reject the transition;
2. preserve current state;
3. record a contract violation;
4. avoid publishing an event;
5. release duplicate callback resources if needed;
6. surface diagnostics.

An invalid transition must not crash the entire application unless state corruption is unrecoverable.

---

# State Invariants

## 74. Module Invariants

1. `Ready` requires at least one usable provider.
2. `Stopped` has no active requests.
3. `ShuttingDown` accepts no new requests.
4. `Unavailable` rejects normal requests.
5. Module state does not depend on one individual request result.

---

## 75. Provider Invariants

1. Only `Ready` or eligible `Degraded` providers may receive new work.
2. `ShuttingDown` providers receive no new work.
3. `Misconfigured` providers are not selected.
4. Provider capabilities remain versioned.
5. Provider state changes do not mutate completed results.
6. Provider state must not expose credentials.

---

## 76. Request Invariants

1. Every request has exactly one current state.
2. Every request has a monotonic state version.
3. Every accepted request reaches exactly one terminal state.
4. Terminal states are immutable.
5. A cancelled request never publishes completion.
6. A failed request never publishes completion under the same request ID.
7. A completed request has a valid immutable result.
8. A request never executes two winning provider attempts.
9. Request state is isolated by `request_id`.
10. Request hints remain unchanged during one execution unless explicitly copied into an internal derived plan.
11. Privacy policy cannot be weakened during fallback.
12. The original source identity remains stable throughout processing.
13. Frame identity remains stable throughout processing.
14. Stage output must be validated before the next dependent state.
15. Every acquired resource is released exactly once.

---

## 77. Result Invariants

1. A published result passed contract validation.
2. A result has exactly one `recognition_id`.
3. Result state never returns to `Building`.
4. Raw recognized text is immutable.
5. Public geometry remains in source coordinate space.
6. An invalid result is never exposed as completed.
7. Expiry does not mutate historical event meaning.
8. User correction creates a separate object.

---

## 78. Event-State Invariants

1. `recognition.started` is emitted at most once per execution lifecycle.
2. Exactly one terminal lifecycle event is emitted.
3. Terminal event type matches terminal request state.
4. Terminal event publication is idempotent.
5. Progress events never change terminal state.
6. Late progress events are ignored by terminal consumers.
7. Public events contain no raw image bytes.
8. Public events contain no complete OCR text.

---

# MVP State Model

## 79. Required MVP Module States

```text
Uninitialized
Initializing
Ready
Degraded
Unavailable
ShuttingDown
Stopped
```

---

## 80. Required MVP Provider States

```text
Registered
Initializing
Ready
Unavailable
Misconfigured
Stopped
```

`Degraded` may initially be represented only through capability status if implementation simplicity requires it.

---

## 81. Required MVP Request States

The MVP may implement the following simplified state set:

```text
Received
Validating
Queued
Processing
AssemblingResult
Cancelling
Completed
Failed
Cancelled
```

Where:

```text
Processing
=
Preparing
+ Preprocessing
+ Detecting
+ Recognizing
+ PostProcessing
+ ResolvingReadingOrder
+ MappingCoordinates
```

Detailed internal stages should still be recorded in metrics even if they are not modeled as separate runtime enum values.

---

## 82. Recommended MVP Request Diagram

```text
Received
    ↓
Validating
 ┌──┴──────────┐
 ↓             ↓
Failed       Queued
               ↓
           Processing
         ┌─────┼─────────┐
         ↓     ↓         ↓
 Assembling  Failed   Cancelling
     ↓                    ↓
 Completed            Cancelled
```

---

## 83. When to Expand MVP States

Split `Processing` into detailed states when at least one of these becomes necessary:

* stage-specific cancellation;
* progress UI;
* provider fallback by stage;
* stage-specific timeout;
* detailed recovery;
* stage-specific metrics;
* independent detector and OCR providers;
* long-page chunking;
* distributed processing;
* debugging state corruption.

---

# Testing

## 84. State Transition Tests

Required tests:

### Module State

* startup reaches `Ready`;
* startup with partial provider availability reaches `Degraded`;
* startup without providers reaches `Unavailable`;
* shutdown rejects new requests;
* shutdown reaches `Stopped`;
* stopped module can reinitialize.

### Provider State

* registered provider initializes;
* valid provider becomes ready;
* invalid credentials produce misconfigured;
* provider outage produces unavailable;
* provider recovery returns to ready;
* shutting-down provider receives no new work.

### Request State

* successful full lifecycle;
* validation rejection;
* queue then execution;
* no-text completion;
* provider failure;
* preprocessing failure;
* result validation failure;
* cancellation before execution;
* cancellation during provider execution;
* cancellation during result assembly;
* timeout;
* fallback success;
* fallback exhaustion;
* duplicate callback;
* late provider response;
* duplicate cancellation;
* completion/cancellation race.

### Result State

* building to available;
* invalid result rejected;
* completion requires valid reference;
* expiry;
* eviction;
* immutable result behavior.

---

## 85. Property Tests

Useful state-machine properties:

```text
terminal_state_count(request) <= 1
```

```text
state_version always increases
```

```text
Completed implies valid recognition_id
```

```text
Cancelled implies no completed event
```

```text
Failed implies no completed result
```

```text
provider selected implies provider was eligible
```

```text
public geometry always fits source coordinate space
```

```text
active request resources released at terminal state
```

---

## 86. Race Tests

Concurrency tests must cover:

```text
cancel vs provider success
cancel vs provider failure
timeout vs provider success
shutdown vs result publication
fallback success vs first provider late success
duplicate provider callback
duplicate terminal publication
result storage success vs cancellation
```

Each race must produce one deterministic terminal outcome.

---

# Open Decisions

## 87. Unresolved State Decisions

The following remain open:

* whether `Rejected` is a separate state or a subtype of `Failed`;
* whether queue time counts toward request timeout;
* whether cancellation transitions through `Cancelling` in all implementations;
* whether result publication state must be persisted;
* whether provider degradation requires a dedicated state in MVP;
* whether provider attempts may overlap;
* whether progress states are runtime enums or diagnostic labels;
* whether active request snapshots should survive process restart;
* whether timeout ends in `Failed` or `Cancelled`;
* whether result expiry events are required;
* how long terminal request state remains in memory;
* how provider recovery backoff is represented;
* whether model loading has separate provider substates;
* whether GPU resource pressure changes module state or scheduler state.

These should be resolved through implementation needs and prototype behavior.

---

## 88. Recommended Decisions for MVP

Recommended initial choices:

```text
Rejected = Failed before execution
queue time counts toward total request timeout
all accepted cancellations pass through Cancelling
provider attempts do not overlap
active request state remains in memory only
timeout during OCR ends in Failed
superseded frame ends in Cancelled
terminal request records remain briefly for deduplication
provider model loading stays inside Initializing
```

These choices minimize complexity while preserving correctness.

---

## 89. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
doc/02-modules/recognition/EVENTS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/DATA_FLOW.md
docs/architecture/MODULE_DEPENDENCY.md
```

---

## 90. Summary

The Recognition state model separates:

```text
module availability
provider availability
request processing
provider attempts
result availability
```

The central request lifecycle is:

```text
Received
    ↓
Validating
    ↓
Queued
    ↓
Processing
    ↓
AssemblingResult
    ↓
Completed
```

with alternative terminal outcomes:

```text
Failed
Cancelled
```

The most important guarantees are:

* exactly one terminal state per accepted request;
* terminal states never change;
* retries create new requests;
* cancellation suppresses late completion;
* provider state is separate from request state;
* stale session relevance is decided outside Recognition;
* completed results are immutable;
* state commits and terminal event publication are coordinated;
* late callbacks cannot overwrite terminal outcomes;
* resource ownership ends at terminal state.
