# Recognition Module Events

> **Project:** CRAI
> **Module:** Recognition
> **Path:** `doc/02-modules/recognition/EVENTS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-22

---

## 1. Purpose

This document defines the event contract of the Recognition module.

It specifies:

* events produced by Recognition;
* events consumed by Recognition;
* event payloads;
* correlation rules;
* ordering guarantees;
* cancellation behavior;
* stale-result behavior;
* retry behavior;
* event privacy rules;
* delivery and idempotency expectations.

This document focuses only on event-driven communication.

Command and data contracts are defined separately in:

```text
doc/02-modules/recognition/CONTRACT.md
```

Internal implementation details are defined separately in:

```text
doc/02-modules/recognition/MODULE.md
```

---

## 2. Event Role of Recognition

Recognition is a processing module.

It consumes requests referring to image content and produces events describing the progress and terminal outcome of recognition work.

```text
Input Event or Command
        ↓
Recognition Processing
        ↓
Progress Events
        ↓
Exactly One Terminal Event
```

Recognition events communicate processing state.

They must not become a substitute for direct access to large recognition results.

---

## 3. Event Design Principles

### 3.1 Events Represent Facts

Event names must describe something that has already happened.

Correct:

```text
recognition.started
recognition.completed
recognition.failed
```

Avoid command-like event names:

```text
recognition.start
recognition.complete
recognition.fail
```

Commands request behavior.

Events report observed state changes.

---

### 3.2 Exactly One Terminal Outcome

Every accepted recognition request must eventually produce exactly one terminal outcome:

```text
recognition.completed
recognition.failed
recognition.cancelled
```

A request must never publish more than one terminal event.

---

### 3.3 Results Are Referenced

Large recognition results should be passed by reference.

```text
recognition.completed
    ↓
RecognitionResultReference
    ↓
Consumer loads RecognitionResult
```

The Event Bus should not routinely carry:

* complete OCR result objects;
* raw images;
* image buffers;
* large provider payloads;
* diagnostic image artifacts.

---

### 3.4 Events Are Immutable

An event must not be changed after publication.

Corrections require a new event.

---

### 3.5 Events Are Safe to Duplicate

Consumers must assume the Event Bus may deliver an event more than once.

Every event must have a unique `event_id`.

Consumers must deduplicate using this identifier.

---

### 3.6 Events Are Not Globally Ordered

Ordering is guaranteed only within the lifecycle of one `request_id` where transport permits.

Consumers must not assume ordering across different recognition requests.

---

### 3.7 Privacy Is Part of the Contract

Normal Recognition events must not contain:

* image bytes;
* complete recognized source text;
* provider credentials;
* authorization headers;
* sensitive local paths;
* full remote-provider responses.

---

## 4. Event Naming Convention

Recognition event names use lowercase dot-separated notation.

```text
recognition.<event>
```

Examples:

```text
recognition.started
recognition.completed
recognition.failed
recognition.cancelled
recognition.regions_detected
```

Names must represent stable domain meaning.

Implementation-stage names must not leak into public events unless they are part of the documented contract.

---

## 5. Event Categories

Recognition events are divided into four categories.

### 5.1 Lifecycle Events

Required events describing the request lifecycle.

```text
recognition.started
recognition.completed
recognition.failed
recognition.cancelled
```

### 5.2 Progress Events

Optional events describing intermediate processing.

```text
recognition.preprocessing_completed
recognition.regions_detected
recognition.region_recognized
recognition.reading_order_resolved
```

### 5.3 Provider Events

Optional events describing provider operational changes.

```text
recognition.provider_ready
recognition.provider_degraded
recognition.provider_unavailable
recognition.provider_configuration_changed
```

### 5.4 Diagnostic Events

Internal or protected events intended for debugging and evaluation.

```text
recognition.diagnostic_recorded
recognition.benchmark_sample_recorded
```

Diagnostic events must not be consumed by normal product workflows.

---

## 6. Event Envelope

All public Recognition events use a shared envelope.

```text
RecognitionEventEnvelope<T>
├── event_id: EventId
├── event_name: string
├── contract_version: string
├── producer: ModuleId
├── occurred_at: Timestamp
├── trace_context: TraceContext
├── partition_key?: string
├── sequence_number?: integer
└── payload: T
```

### 6.1 Event ID

```text
EventId = opaque string
```

Requirements:

1. globally unique within the event retention window;
2. immutable;
3. not reused;
4. safe for deduplication;
5. not interpreted for business meaning.

---

### 6.2 Producer

For public Recognition events:

```text
producer = recognition
```

Provider adapters must not publish public events under their own provider-specific namespace.

---

### 6.3 Contract Version

```text
contract_version = 1.0.0
```

Consumers must reject unsupported major versions.

---

### 6.4 Trace Context

```text
TraceContext
├── trace_id
├── parent_span_id?
├── correlation_id?
└── baggage?
```

Trace baggage must not contain source image data or recognized text.

---

### 6.5 Partition Key

Recommended partition key:

```text
request_id
```

This improves per-request event ordering where the Event Bus supports partitioned delivery.

---

### 6.6 Sequence Number

A per-request sequence number may be included.

Example:

```text
recognition.started              sequence_number = 1
recognition.regions_detected     sequence_number = 2
recognition.completed            sequence_number = 3
```

Consumers must not rely solely on sequence numbers because transport delivery may still be duplicated or delayed.

---

## 7. Shared Event Context

Recognition lifecycle events should contain common correlation fields.

```text
RecognitionEventContext
├── request_id: RequestId
├── recognition_id?: RecognitionId
├── previous_recognition_id?: RecognitionId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id?: ProviderId
└── attempt: integer
```

### Field Meaning

`request_id`

Identifies one Recognition request lifecycle.

`recognition_id`

Identifies a completed immutable Recognition result.

`previous_recognition_id`

Links a retry result to a previous result.

`session_id`

Links work to a reading session when applicable.

`source_id`

Identifies the source or capture origin.

`content_id`

Identifies the logical content being processed.

`frame_id`

Identifies the exact observed frame.

`provider_id`

Identifies the selected provider.

`attempt`

Indicates the current provider or retry attempt inside one request.

---

## 8. Required Produced Events

Recognition must produce the following events:

```text
recognition.started
recognition.completed
recognition.failed
recognition.cancelled
```

These are the only events normal consumers may rely on.

All other Recognition events are optional.

---

# Produced Lifecycle Events

## 9. `recognition.started`

### 9.1 Meaning

Published after:

* the request has been accepted;
* validation has passed;
* provider selection has completed;
* execution is about to begin.

It must not be published merely because a request was received.

---

### 9.2 Payload

```text
RecognitionStartedEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id: ProviderId
├── recognition_mode: RecognitionMode
├── priority: ProcessingPriority
├── attempt: integer
├── queued_duration_ms?: integer
├── started_at: Timestamp
└── provider_execution:
    ├── execution_location
    └── execution_device
```

---

### 9.3 Publication Rules

1. Published once per accepted execution attempt.
2. Must precede the terminal event for that attempt.
3. Must not include the image reference.
4. Must not include recognized text.
5. Must expose remote execution through `execution_location`.
6. May be omitted for synchronous in-process invocation only when the caller directly receives the result and no Event Bus lifecycle is expected.
7. If an event-based request was accepted, this event is required.

---

### 9.4 Example

```json
{
  "event_id": "evt_rec_started_001",
  "event_name": "recognition.started",
  "contract_version": "1.0.0",
  "producer": "recognition",
  "occurred_at": "2026-07-22T03:15:42.190Z",
  "partition_key": "req_20260722_0001",
  "sequence_number": 1,
  "trace_context": {
    "trace_id": "trace_01"
  },
  "payload": {
    "request_id": "req_20260722_0001",
    "session_id": "session_01",
    "source_id": "desktop_region_01",
    "content_id": "page_104",
    "frame_id": "frame_104_08",
    "provider_id": "local_ocr_01",
    "recognition_mode": "ComicPage",
    "priority": "Interactive",
    "attempt": 1,
    "started_at": "2026-07-22T03:15:42.190Z",
    "provider_execution": {
      "execution_location": "LocalProcess",
      "execution_device": "GPU"
    }
  }
}
```

---

## 10. `recognition.completed`

### 10.1 Meaning

Published when Recognition has successfully created an immutable result.

A successful result may:

* contain recognized regions;
* contain warnings;
* contain no readable regions.

---

### 10.2 Payload

```text
RecognitionCompletedEvent
├── recognition_id: RecognitionId
├── request_id: RequestId
├── previous_recognition_id?: RecognitionId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id: ProviderId
├── status: RecognitionStatus
├── attempt: integer
├── region_count: integer
├── line_count: integer
├── character_count: integer
├── warning_count: integer
├── total_duration_ms: integer
├── result_reference: RecognitionResultReference
└── completed_at: Timestamp
```

---

### 10.3 Recognition Status

```text
Completed
CompletedWithWarnings
```

An empty result may use:

```text
Completed
```

with a `NoReadableTextDetected` warning inside the stored result.

---

### 10.4 Result Reference

```text
RecognitionResultReference
├── reference_type
├── reference_value
├── expires_at?
└── integrity_checksum?
```

Supported reference types:

```text
InMemoryResult
TemporaryResultStore
PersistentResultStore
InlinePermitted
```

`InlinePermitted` must only be used for small, trusted, in-process events.

---

### 10.5 Publication Rules

1. Published only after result validation succeeds.
2. Published only after result storage or registration succeeds when a reference is required.
3. Must be the only terminal event for the request.
4. Must not be published after accepted cancellation.
5. Must not include the full result by default.
6. Must not include recognized text.
7. Must contain the exact `frame_id` used for recognition.
8. May complete after a newer frame has already been observed.
9. Stale-result rejection belongs to the consumer or Session orchestration.
10. Recognition must not suppress a valid result solely because it suspects it may be stale, unless cancellation was already accepted.

---

### 10.6 Example

```json
{
  "event_id": "evt_rec_completed_001",
  "event_name": "recognition.completed",
  "contract_version": "1.0.0",
  "producer": "recognition",
  "occurred_at": "2026-07-22T03:15:42.872Z",
  "partition_key": "req_20260722_0001",
  "sequence_number": 3,
  "trace_context": {
    "trace_id": "trace_01"
  },
  "payload": {
    "recognition_id": "rec_20260722_0001",
    "request_id": "req_20260722_0001",
    "session_id": "session_01",
    "source_id": "desktop_region_01",
    "content_id": "page_104",
    "frame_id": "frame_104_08",
    "provider_id": "local_ocr_01",
    "status": "CompletedWithWarnings",
    "attempt": 1,
    "region_count": 12,
    "line_count": 18,
    "character_count": 143,
    "warning_count": 1,
    "total_duration_ms": 682,
    "result_reference": {
      "reference_type": "TemporaryResultStore",
      "reference_value": "recognition-result://rec_20260722_0001",
      "expires_at": "2026-07-22T04:15:42.872Z"
    },
    "completed_at": "2026-07-22T03:15:42.872Z"
  }
}
```

---

## 11. `recognition.failed`

### 11.1 Meaning

Published when Recognition cannot produce a valid usable result.

Examples:

* image cannot be resolved;
* preprocessing fails;
* provider fails;
* provider response is invalid;
* coordinate mapping fails;
* result assembly fails.

No-text detection is not a failure.

---

### 11.2 Payload

```text
RecognitionFailedEvent
├── request_id: RequestId
├── recognition_id?: RecognitionId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id?: ProviderId
├── attempt: integer
├── error: RecognitionError
├── partial_result_reference?: RecognitionResultReference
└── failed_at: Timestamp
```

A partial result reference is allowed only when partial-result policy explicitly permits it.

---

### 11.3 Publication Rules

1. Published only when no acceptable final result can be produced.
2. Must contain a normalized Recognition error.
3. Must be the only terminal event for the request.
4. Must not include complete provider error payloads.
5. Must not include raw source text.
6. Must not expose credentials.
7. Must preserve `request_id`, `source_id`, and `content_id`.
8. Should preserve `frame_id` when provided.
9. Should expose whether the error is retryable.
10. Must not be published after `recognition.cancelled`.

---

### 11.4 Example

```json
{
  "event_id": "evt_rec_failed_001",
  "event_name": "recognition.failed",
  "contract_version": "1.0.0",
  "producer": "recognition",
  "occurred_at": "2026-07-22T03:20:14.421Z",
  "partition_key": "req_20260722_0003",
  "sequence_number": 2,
  "trace_context": {
    "trace_id": "trace_03"
  },
  "payload": {
    "request_id": "req_20260722_0003",
    "session_id": "session_01",
    "source_id": "desktop_region_01",
    "content_id": "page_106",
    "frame_id": "frame_106_02",
    "provider_id": "remote_ocr_01",
    "attempt": 1,
    "error": {
      "contract_version": "1.0.0",
      "error_code": "ProviderTimeout",
      "stage": "TextRecognition",
      "message": "The recognition provider exceeded the request timeout.",
      "retryable": true,
      "request_id": "req_20260722_0003",
      "provider_id": "remote_ocr_01",
      "occurred_at": "2026-07-22T03:20:14.421Z",
      "trace_context": {
        "trace_id": "trace_03"
      }
    },
    "failed_at": "2026-07-22T03:20:14.421Z"
  }
}
```

---

## 12. `recognition.cancelled`

### 12.1 Meaning

Published when a Recognition request has entered a terminal cancelled state.

The provider process may or may not have been interrupted immediately.

The important guarantee is that the request will no longer produce a completed result event.

---

### 12.2 Payload

```text
RecognitionCancelledEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id?: ProviderId
├── attempt: integer
├── reason: CancellationReason
├── provider_interrupted: boolean
├── cancellation_requested_at?: Timestamp
└── cancelled_at: Timestamp
```

---

### 12.3 Cancellation Reasons

```text
UserCancelled
SessionStopped
SourceChanged
NewerFrameAvailable
RequestSuperseded
ApplicationShutdown
Timeout
ResourcePressure
```

---

### 12.4 Publication Rules

1. Published only after cancellation has been accepted.
2. Must be the only terminal event for the request.
3. Completion arriving later from a non-interruptible provider must be discarded.
4. `provider_interrupted = false` does not permit later completion publication.
5. Must not include partial OCR text.
6. Must preserve the original request correlation fields.
7. Repeated cancellation requests must not produce repeated terminal events.
8. Cancellation before `recognition.started` may produce only `recognition.cancelled` if execution never started.
9. Cancellation after completion returns an `AlreadyCompleted` command result and must not publish `recognition.cancelled`.

---

### 12.5 Example

```json
{
  "event_id": "evt_rec_cancelled_001",
  "event_name": "recognition.cancelled",
  "contract_version": "1.0.0",
  "producer": "recognition",
  "occurred_at": "2026-07-22T03:25:10.144Z",
  "partition_key": "req_20260722_0004",
  "sequence_number": 2,
  "trace_context": {
    "trace_id": "trace_04"
  },
  "payload": {
    "request_id": "req_20260722_0004",
    "session_id": "session_01",
    "source_id": "desktop_region_01",
    "content_id": "page_107",
    "frame_id": "frame_107_04",
    "provider_id": "local_ocr_01",
    "attempt": 1,
    "reason": "NewerFrameAvailable",
    "provider_interrupted": false,
    "cancellation_requested_at": "2026-07-22T03:25:10.139Z",
    "cancelled_at": "2026-07-22T03:25:10.144Z"
  }
}
```

---

# Optional Progress Events

## 13. Progress Event Policy

Progress events are optional.

Consumers must not require them for correctness.

They may be enabled for:

* debugging;
* performance profiling;
* progress UI;
* provider evaluation;
* long-running recognition jobs.

Progress events may be disabled because of:

* performance cost;
* privacy policy;
* transport volume;
* provider limitations;
* implementation simplicity.

---

## 14. `recognition.preprocessing_completed`

### 14.1 Meaning

Published when image preprocessing has completed successfully.

### 14.2 Payload

```text
RecognitionPreprocessingCompletedEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id: ProviderId
├── preprocessing_profile_id?: PreprocessingProfileId
├── operation_count: integer
├── source_width: integer
├── source_height: integer
├── processed_width: integer
├── processed_height: integer
├── duration_ms: integer
└── completed_at: Timestamp
```

### 14.3 Rules

1. Must not include processed image data.
2. Must not include temporary image paths.
3. Must not be required by consumers.
4. Must not be published after cancellation.
5. Geometry-changing preprocessing must already be tracked internally.

---

## 15. `recognition.regions_detected`

### 15.1 Meaning

Published after text-region detection completes.

### 15.2 Payload

```text
RecognitionRegionsDetectedEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id: ProviderId
├── detected_region_count: integer
├── low_confidence_region_count: integer
├── detection_duration_ms: integer
└── detected_at: Timestamp
```

### 15.3 Optional Trusted Payload

A trusted in-process channel may additionally carry:

```text
region_summaries:
├── region_id
├── geometry
├── orientation
└── confidence_level
```

It must not include recognized text.

---

## 16. `recognition.region_recognized`

### 16.1 Meaning

Published when one region has completed OCR.

This event is useful for:

* progressive UI;
* long page processing;
* diagnostics;
* performance inspection.

### 16.2 Payload

```text
RecognitionRegionRecognizedEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id: ProviderId
├── region_id: RegionId
├── region_index: integer
├── total_region_count: integer
├── recognition_confidence_level: ConfidenceLevel
├── character_count: integer
├── duration_ms: integer
└── recognized_at: Timestamp
```

### 16.3 Rules

1. Raw recognized text must not appear in normal Event Bus payloads.
2. Event frequency must be bounded.
3. Region events may arrive out of spatial order.
4. Consumers must not treat this as a completed result.
5. Region events may be lost without affecting correctness.
6. A cancelled request may have emitted earlier region progress events.
7. Consumers must discard partial progress after terminal cancellation or failure.

---

## 17. `recognition.reading_order_resolved`

### 17.1 Meaning

Published when initial reading order has been computed.

### 17.2 Payload

```text
RecognitionReadingOrderResolvedEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── provider_id: ProviderId
├── ordered_region_count: integer
├── reading_direction: ReadingDirection
├── order_source: ReadingOrderSource
├── confidence_level: ConfidenceLevel
├── warning_count: integer
├── duration_ms: integer
└── resolved_at: Timestamp
```

### 17.3 Rules

1. Full order entries should remain in the final result.
2. This event is informational.
3. Uncertain order must be represented explicitly.
4. Consumers must not start translation from this event unless a dedicated streaming workflow is later defined.

---

# Provider Operational Events

## 18. Provider Event Scope

Provider events communicate operational availability.

They do not communicate OCR quality.

A provider may be operationally ready but unsuitable for CRAI's required content.

---

## 19. `recognition.provider_ready`

### Payload

```text
RecognitionProviderReadyEvent
├── provider_id: ProviderId
├── provider_version: string
├── execution_locations: ExecutionLocation[]
├── supported_modes: RecognitionMode[]
├── readiness_source: ProviderReadinessSource
└── ready_at: Timestamp
```

Readiness sources:

```text
Initialization
HealthCheck
ConfigurationReload
ManualRecovery
```

---

## 20. `recognition.provider_degraded`

### Payload

```text
RecognitionProviderDegradedEvent
├── provider_id: ProviderId
├── degradation_code: string
├── message: string
├── affected_capabilities: RecognitionCapability[]
├── requests_may_continue: boolean
└── degraded_at: Timestamp
```

Examples:

* GPU unavailable, CPU fallback active;
* remote provider latency elevated;
* some language model unavailable;
* cancellation temporarily unsupported.

---

## 21. `recognition.provider_unavailable`

### Payload

```text
RecognitionProviderUnavailableEvent
├── provider_id: ProviderId
├── reason_code: string
├── message: string
├── retryable: boolean
├── expected_recovery_at?: Timestamp
└── unavailable_at: Timestamp
```

This event must not expose credentials or full provider errors.

---

## 22. `recognition.provider_configuration_changed`

### Payload

```text
RecognitionProviderConfigurationChangedEvent
├── provider_id: ProviderId
├── previous_configuration_version?: string
├── new_configuration_version: string
├── restart_required: boolean
├── capability_change_detected: boolean
└── changed_at: Timestamp
```

Sensitive configuration values must not be included.

---

# Events Consumed by Recognition

## 23. Consumed Event Policy

Recognition should prefer direct commands for work that requires a clear response.

Events are suitable when:

* the producer and Recognition are decoupled;
* asynchronous processing is expected;
* multiple consumers may observe the same fact;
* the workflow does not require immediate synchronous acknowledgment.

Recognition must not create hidden behavior by subscribing to broad unrelated events.

---

## 24. `source.image_imported`

### Meaning

A new image has been imported and may require recognition.

### Expected Payload

```text
SourceImageImportedEvent
├── source_id: SourceId
├── content_id: ContentId
├── image_reference: ImageReference
├── coordinate_space: CoordinateSpace
├── media_type: string
├── imported_at: Timestamp
├── recognition_requested: boolean
├── recognition_options?: RecognitionOptions
└── trace_context: TraceContext
```

### Consumption Rules

1. Recognition acts only when `recognition_requested = true`.
2. Import alone must not automatically trigger OCR unless product workflow explicitly defines it.
3. The image reference must remain valid until completion.
4. Recognition creates a new `request_id` only if orchestration has not already provided one.
5. Prefer an orchestration-generated explicit request event for production flows.

---

## 25. `observation.stable_frame_ready`

### Meaning

Observation has identified a stable frame suitable for processing.

### Expected Payload

```text
StableFrameReadyEvent
├── session_id: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id: FrameId
├── image_reference: ImageReference
├── coordinate_space: CoordinateSpace
├── stability_score?: decimal
├── region_of_interest?: Geometry
├── recognition_options?: RecognitionOptions
├── observed_at: Timestamp
└── trace_context: TraceContext
```

### Consumption Rules

1. Recognition must preserve `frame_id`.
2. The exact image reference must correspond to that frame.
3. A later frame may supersede this request.
4. Recognition does not decide whether the frame remains current.
5. Observation or Session may later request cancellation.
6. Region-of-interest processing should use `RecognizeRegion`.
7. Stable-frame events must not be processed multiple times without idempotency controls.

---

## 26. `recognition.requested`

### Meaning

A module requests asynchronous recognition.

This is the event equivalent of the Recognize command.

### Expected Payload

```text
RecognitionRequestedEvent
├── request_id: RequestId
├── session_id?: SessionId
├── source_id: SourceId
├── content_id: ContentId
├── frame_id?: FrameId
├── image_reference: ImageReference
├── source_coordinate_space: CoordinateSpace
├── region?: Geometry
├── options: RecognitionOptions
├── priority: ProcessingPriority
├── requested_at: Timestamp
└── trace_context: TraceContext
```

### Consumption Rules

1. `request_id` is required.
2. Duplicate request IDs must be handled idempotently.
3. Region presence selects region recognition behavior.
4. Request validation failure produces `recognition.failed`.
5. Accepted requests produce `recognition.started`.
6. Event acceptance does not guarantee immediate execution.
7. The image reference must remain valid until terminal outcome.
8. The producer must not reuse `request_id` for different content.

---

## 27. `recognition.cancellation_requested`

### Meaning

A request should stop because it is no longer needed or cannot continue.

### Expected Payload

```text
RecognitionCancellationRequestedEvent
├── request_id: RequestId
├── reason: CancellationReason
├── requested_at: Timestamp
└── trace_context: TraceContext
```

### Consumption Rules

1. Recognition resolves the active request by `request_id`.
2. Cancellation is idempotent.
3. Already completed requests remain completed.
4. Accepted cancellation eventually produces `recognition.cancelled`.
5. A provider result arriving later must be discarded.
6. Request-not-found handling may be logged but should not produce a false terminal event.
7. The cancellation event must not identify requests only by `frame_id`.

---

## 28. `session.stopped`

### Meaning

A reading session has ended.

### Expected Payload

```text
SessionStoppedEvent
├── session_id: SessionId
├── reason: SessionStopReason
├── stopped_at: Timestamp
└── trace_context: TraceContext
```

### Consumption Rules

1. Recognition cancels active requests associated with the session.
2. Independent imported-image recognition must not be cancelled.
3. Completed results remain immutable.
4. Temporary resources associated with active requests must be released.
5. Each affected request produces its own cancellation terminal event.
6. No aggregate Recognition cancellation event is required.

---

## 29. `source.closed`

### Meaning

A source is no longer available.

### Expected Payload

```text
SourceClosedEvent
├── source_id: SourceId
├── reason: SourceCloseReason
├── closed_at: Timestamp
└── trace_context: TraceContext
```

### Consumption Rules

1. Active requests using the closed source should be cancelled.
2. Already materialized image references may continue only when policy explicitly permits it.
3. New requests for the source must be rejected.
4. Recognition does not own source lifecycle state.
5. Recognition should clear source-scoped temporary handles.

---

## 30. `application.shutdown_requested`

### Meaning

The application is preparing to terminate.

### Expected Payload

```text
ApplicationShutdownRequestedEvent
├── shutdown_id: string
├── reason: string
├── deadline?: Timestamp
└── requested_at: Timestamp
```

### Consumption Rules

1. Stop accepting new requests.
2. Cancel active requests.
3. Release providers and models.
4. Publish terminal cancellation events where practical.
5. Do not wait indefinitely for non-interruptible providers.
6. Avoid publishing provider-ready events during shutdown.
7. Shutdown cleanup must be bounded.

---

## 31. `configuration.recognition_changed`

### Meaning

Recognition-related configuration has changed.

### Expected Payload

```text
RecognitionConfigurationChangedEvent
├── configuration_version: string
├── changed_sections: string[]
├── restart_required: boolean
├── changed_at: Timestamp
└── trace_context: TraceContext
```

### Consumption Rules

1. Validate new configuration before activation.
2. Do not mutate configuration used by an active request.
3. Active requests use their original configuration snapshot.
4. New requests use the new validated configuration.
5. Provider restart may be required.
6. Provider operational events may be published after reload.
7. Sensitive configuration values must not be included.

---

# Event Lifecycle and State

## 32. Normal Successful Lifecycle

```text
recognition.requested
        ↓
recognition.started
        ↓
[optional progress events]
        ↓
recognition.completed
```

Example:

```text
recognition.requested
recognition.started
recognition.preprocessing_completed
recognition.regions_detected
recognition.reading_order_resolved
recognition.completed
```

---

## 33. Failure Lifecycle

```text
recognition.requested
        ↓
recognition.started
        ↓
[optional progress events]
        ↓
recognition.failed
```

Validation may fail before execution begins.

In that case:

```text
recognition.requested
        ↓
recognition.failed
```

`recognition.started` is not required if execution never started.

---

## 34. Cancellation Lifecycle

### Before Execution

```text
recognition.requested
        ↓
recognition.cancellation_requested
        ↓
recognition.cancelled
```

### During Execution

```text
recognition.requested
        ↓
recognition.started
        ↓
recognition.regions_detected
        ↓
recognition.cancellation_requested
        ↓
recognition.cancelled
```

### Non-Interruptible Provider

```text
recognition.cancelled
        ↓
provider returns internally later
        ↓
result discarded
```

No additional completion event is published.

---

## 35. Empty Result Lifecycle

```text
recognition.requested
        ↓
recognition.started
        ↓
recognition.completed
```

The referenced Recognition result contains:

```text
regions = []
warnings = [NoReadableTextDetected]
```

Recognition must not publish `recognition.failed`.

---

## 36. Retry Lifecycle

A retry is a new request.

```text
Original Request
    ↓
recognition.failed
    ↓
Retry Requested with New request_id
    ↓
recognition.started
    ↓
recognition.completed
```

The completed result should contain:

```text
previous_recognition_id
```

when retrying a previous completed or partial result.

Retries must not reuse the original request ID.

---

## 37. Provider Fallback Lifecycle

Fallback may occur inside one Recognition request.

```text
Attempt 1 Provider A
    ↓ failure
Attempt 2 Provider B
    ↓ success
recognition.completed
```

The final event includes:

```text
provider_id = Provider B
attempt = 2
```

The result must include:

```text
fallback_index = 1
warning = FallbackProviderUsed
```

Public `recognition.failed` must not be emitted for an internal attempt if fallback continues.

Optional protected diagnostics may record failed attempts.

---

# Ordering, Delivery, and Idempotency

## 38. Per-Request Event Ordering

Preferred ordering:

```text
started
    ↓
zero or more progress events
    ↓
one terminal event
```

The Event Bus should partition by `request_id`.

Recognition must assign sequence numbers monotonically per request when supported.

---

## 39. Delivery Guarantees

The architecture must assume at-least-once event delivery.

Possible effects:

* duplicate started events;
* duplicate completion events;
* duplicate cancellation requests;
* delayed progress events;
* out-of-order delivery after retries;
* terminal event arriving before an earlier progress event.

Consumers must handle these safely.

---

## 40. Consumer Deduplication

Consumers should maintain:

```text
ProcessedEvent
├── event_id
├── processed_at
└── handler_id
```

A duplicate `event_id` must not produce duplicate downstream work.

For terminal events, consumers should also protect against duplicate handling by:

```text
request_id + terminal_event_type
```

---

## 41. Terminal Event Conflict Handling

A consumer receiving conflicting terminal events for one request should:

1. retain the first valid terminal event;
2. reject subsequent conflicting terminal events;
3. record a contract violation;
4. avoid triggering downstream work twice;
5. surface diagnostics.

Recognition implementation tests must ensure this never happens under normal operation.

---

## 42. Late Progress Events

A delayed progress event may arrive after a terminal event.

Consumers must ignore progress events for terminal requests.

Example:

```text
recognition.completed
        ↓
delayed recognition.region_recognized
        ↓
ignored
```

---

## 43. Stale Recognition Results

A result is stale when it belongs to an older frame or content version than the consumer currently needs.

Recognition does not determine staleness.

The consumer compares:

```text
session_id
source_id
content_id
frame_id
```

Example:

```text
Current frame = frame_106
Completed result frame = frame_105
```

Session orchestration may:

* ignore the result;
* discard the temporary reference;
* retain it for diagnostics;
* avoid starting Text Processing;
* request cancellation earlier when possible.

A stale result is not necessarily an invalid Recognition result.

---

## 44. Event Correlation

Primary correlation keys:

```text
request_id
trace_id
```

Secondary context:

```text
session_id
source_id
content_id
frame_id
recognition_id
```

Rules:

1. `request_id` correlates one processing lifecycle.
2. `recognition_id` identifies one immutable result.
3. `trace_id` correlates cross-module workflow.
4. `frame_id` supports stale-result detection.
5. `session_id` must not replace `request_id`.
6. `content_id` must not replace `frame_id` when frame freshness matters.

---

# Privacy and Security

## 45. Public Event Privacy Classification

Recognition events should be classified as:

```text
Operational Metadata
```

They may contain:

* identifiers;
* counts;
* durations;
* provider identity;
* warning count;
* normalized error codes;
* result references.

They must not contain protected source content by default.

---

## 46. Prohibited Event Fields

Public Recognition events must not contain:

```text
image_bytes
image_base64
raw_image_path
complete_raw_text
complete_surface_text
provider_api_key
provider_access_token
authorization_header
remote_provider_full_response
temporary_file_credentials
user_glossary_content
translated_text
```

---

## 47. Result Reference Security

A result reference must:

1. be scoped to authorized consumers;
2. expire when temporary;
3. avoid predictable sensitive file paths;
4. not expose direct unrestricted filesystem access;
5. support integrity validation when crossing processes;
6. be deleted or invalidated according to retention policy.

---

## 48. Remote Provider Disclosure

When remote processing occurs:

* `recognition.started` exposes remote execution location;
* `recognition.completed` exposes the remote provider identity;
* the stored result includes `RemoteProviderUsed`;
* normal events still do not include image or text content.

---

## 49. Diagnostic Event Security

Protected diagnostic events may contain references to:

* processed image artifacts;
* provider raw response files;
* region visualizations;
* benchmark samples.

They require:

* explicit diagnostic mode;
* access control;
* bounded retention;
* secure storage;
* redaction;
* auditability.

Normal modules must not subscribe to diagnostic event streams.

---

# Event Error Handling

## 50. Event Publication Failure

If Recognition completes processing but cannot publish the terminal event:

1. persist or retain the terminal outcome;
2. retry event publication according to Event Bus policy;
3. use the same `event_id` for retries;
4. do not rerun OCR solely because publication failed;
5. record the publication failure;
6. preserve idempotency.

---

## 51. Result Storage Failure

If Recognition creates a result but cannot store or register the result reference:

```text
recognition.failed
```

should be published with:

```text
error_code = ResultAssemblyFailed
```

or a more specific future storage-reference error.

Recognition must not publish `recognition.completed` with an unusable reference.

---

## 52. Consumed Event Validation Failure

When `recognition.requested` is malformed:

Recognition should publish:

```text
recognition.failed
```

when sufficient correlation data exists.

When even `request_id` or routing context is missing:

* reject the event;
* route it to dead-letter handling;
* record a safe validation error;
* do not invent identifiers silently.

---

## 53. Dead-Letter Policy

Events may be sent to a dead-letter stream when:

* contract major version is unsupported;
* payload cannot be parsed;
* required identity is missing;
* event repeatedly fails processing;
* result reference is invalid;
* privacy validation fails.

Dead-letter entries must not expose protected image or text content.

---

# Consumer Expectations

## 54. Text Processing Consumer

Text Processing should consume:

```text
recognition.completed
```

Then:

1. verify the result belongs to the expected frame;
2. retrieve the result from `result_reference`;
3. validate result version;
4. consume regions using explicit reading order;
5. handle empty results;
6. preserve raw Recognition output;
7. perform semantic cleanup separately;
8. avoid parsing provider metadata for core logic.

Text Processing must not consume progress events for correctness.

---

## 55. Session Consumer

Session or Orchestration should consume:

```text
recognition.started
recognition.completed
recognition.failed
recognition.cancelled
```

It uses them to:

* update session processing state;
* reject stale results;
* cancel obsolete requests;
* decide whether to continue to Text Processing;
* surface recoverable failures;
* retry according to policy.

---

## 56. Presentation Consumer

Presentation may consume lifecycle summaries for user feedback.

Examples:

```text
recognition.started
recognition.failed
recognition.cancelled
```

Presentation should normally receive final translated content through higher-level workflow events rather than directly consuming Recognition results.

Presentation must not become coupled to provider details.

---

## 57. Diagnostics Consumer

Diagnostics may consume all Recognition events.

It may calculate:

* latency distribution;
* failure rates;
* cancellation rates;
* provider fallback rates;
* no-text rates;
* average region counts;
* provider health changes.

It must not infer OCR quality from operational success alone.

---

# Testing

## 58. Event Contract Tests

Required tests:

### Lifecycle

* request produces started then completed;
* request produces started then failed;
* request produces started then cancelled;
* validation failure produces failed without started;
* empty OCR result produces completed;
* exactly one terminal event;
* no completion after cancellation.

### Ordering

* per-request sequence numbers increase;
* duplicate delivery is safe;
* late progress event is ignored;
* different requests may complete out of order;
* terminal conflict is detected.

### Correlation

* request ID preserved;
* frame ID preserved;
* source and content IDs preserved;
* retry uses new request ID;
* retry result links previous recognition ID;
* trace context propagates.

### Privacy

* no image bytes in events;
* no complete OCR text in events;
* no provider credentials in errors;
* no temporary sensitive paths;
* remote execution is disclosed.

### Result References

* completed event points to valid result;
* expired result reference is handled;
* storage failure does not produce completed;
* duplicate completion does not duplicate downstream work.

### Cancellation

* cancellation before start;
* cancellation during preprocessing;
* cancellation during provider execution;
* non-interruptible provider result discarded;
* repeated cancellation is idempotent;
* completed request cannot be cancelled retroactively.

---

## 59. Event Invariants

The following invariants must always hold.

1. Every public event has a unique `event_id`.
2. Every public event declares a contract version.
3. Every Recognition lifecycle event contains `request_id`.
4. Every accepted request has exactly one terminal event.
5. A completed request never later becomes cancelled.
6. A cancelled request never later becomes completed.
7. A failed request never later becomes completed under the same request ID.
8. Retries use new request IDs.
9. Full image data is never included in public Recognition events.
10. Complete recognized text is never included in normal events.
11. Provider credentials never appear in events.
12. Completion references a valid immutable result.
13. Progress events are optional.
14. Consumers do not depend on progress events for correctness.
15. Stale-result handling is performed outside Recognition.
16. Recognition preserves frame identity.
17. Duplicate event delivery does not produce duplicate work.
18. Event names describe completed facts.
19. Provider-specific SDK types never appear in event payloads.
20. Terminal event publication is idempotent.
21. Event timestamps use UTC.
22. Event payloads remain backward-compatible within the major version.
23. Local-only execution never generates remote-provider metadata.
24. Remote execution is visible in lifecycle metadata.
25. No-text detection produces completion, not failure.

---

## 60. MVP Event Set

The MVP requires only the following produced events:

```text
recognition.started
recognition.completed
recognition.failed
recognition.cancelled
```

The MVP may consume:

```text
recognition.requested
recognition.cancellation_requested
observation.stable_frame_ready
session.stopped
application.shutdown_requested
```

The MVP does not require:

```text
recognition.preprocessing_completed
recognition.regions_detected
recognition.region_recognized
recognition.reading_order_resolved
recognition.provider_degraded
recognition.diagnostic_recorded
```

Optional events should be added only when a concrete consumer requires them.

---

## 61. Deferred Event Extensions

Potential future events:

```text
recognition.partial_result_available
recognition.long_page_chunk_completed
recognition.provider_benchmark_completed
recognition.model_loaded
recognition.model_unloaded
recognition.region_retry_completed
recognition.quality_evaluation_completed
recognition.manual_correction_received
recognition.result_expired
recognition.result_evicted
```

These are deferred because they introduce additional lifecycle and storage complexity.

They must not be added without a documented use case.

---

## 62. Related Documents

```text
doc/02-modules/recognition/README.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/recognition/CONTRACT.md
docs/architecture/EVENT_BUS.md
docs/architecture/STATE_MACHINE.md
docs/architecture/DATA_FLOW.md
docs/architecture/MODULE_DEPENDENCY.md
```

---

## 63. Summary

Recognition events communicate the lifecycle of image-to-text processing without exposing large or sensitive content.

The essential event sequence is:

```text
recognition.started
        ↓
recognition.completed
    or recognition.failed
    or recognition.cancelled
```

The event contract guarantees:

* one terminal outcome per request;
* stable request correlation;
* explicit frame identity;
* safe duplicate delivery;
* immutable event facts;
* result references instead of large payloads;
* no raw image data in normal events;
* no complete OCR text in normal events;
* cancellation-safe completion behavior;
* provider-independent communication;
* separation between Recognition processing and downstream Text Processing.

Recognition progress events are optional.

Only lifecycle events form the stable public contract required by other CRAI modules.
