# Text Processing Events

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `modules/text-processing/EVENTS.md`
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-22

---

## 1. Purpose

This document defines the events produced and consumed by the Text Processing module.

It specifies:

* event names;
* event envelopes;
* lifecycle events;
* request events;
* cancellation events;
* progress events;
* configuration events;
* result references;
* correlation rules;
* ordering rules;
* duplicate handling;
* stale-result handling;
* retry behavior;
* cancellation races;
* privacy requirements;
* compatibility rules.

The Text Processing event flow is:

```text
Recognition completed
        ↓
Text Processing requested
        ↓
Text Processing started
        ↓
Text Processing completed
        ↓
Translation requested
```

Alternative terminal outcomes are:

```text
Text Processing failed
Text Processing cancelled
```

---

## 2. Event Boundary

Text Processing events coordinate processing lifecycle.

They must not be used to transport large document payloads directly.

Events should carry:

* identifiers;
* lifecycle status;
* result references;
* warning summaries;
* error summaries;
* processing metadata;
* correlation information.

Events should not carry:

* complete Recognition results;
* complete `SourceDocument`;
* complete raw OCR text;
* complete normalized text;
* image bytes;
* translation units;
* translated text;
* provider credentials.

---

## 3. Event Naming

Text Processing events use:

```text
text_processing.<event_name>
```

Required lifecycle events:

```text
text_processing.requested
text_processing.started
text_processing.completed
text_processing.failed
text_processing.cancellation_requested
text_processing.cancelled
```

Optional progress events:

```text
text_processing.input_adapted
text_processing.order_resolved
text_processing.normalization_completed
text_processing.lines_reconstructed
text_processing.regions_grouped
text_processing.blocks_classified
text_processing.document_built
text_processing.traceability_validated
```

Configuration and lifecycle events:

```text
text_processing.configuration_changed
text_processing.module_ready
text_processing.module_degraded
text_processing.module_unavailable
text_processing.module_stopping
text_processing.module_stopped
```

---

## 4. Event Categories

Events are divided into:

```text
Command Events
Lifecycle Events
Progress Events
Configuration Events
Module Health Events
```

### Command events

Request work or state change.

Examples:

```text
text_processing.requested
text_processing.cancellation_requested
```

### Lifecycle events

Describe request-level lifecycle.

Examples:

```text
text_processing.started
text_processing.completed
text_processing.failed
text_processing.cancelled
```

### Progress events

Describe optional internal milestones.

Examples:

```text
text_processing.order_resolved
text_processing.document_built
```

### Configuration events

Notify that effective processing configuration changed.

### Module health events

Describe module availability.

---

# Event Envelope

## 5. Standard Event Envelope

All Text Processing events must use the shared CRAI event envelope.

Conceptual structure:

```text
EventEnvelope
├── event_id
├── event_name
├── event_version
├── occurred_at
├── producer
├── correlation
├── causation
├── subject
├── sequence?
├── partition_key?
├── delivery
├── privacy
├── payload
└── extensions?
```

---

## 6. Envelope Example

```json
{
  "event_id": "evt_01J8N2QPKM8N54TWR7GMDM4W0Z",
  "event_name": "text_processing.completed",
  "event_version": "1.0.0",
  "occurred_at": "2026-07-22T03:10:00.025Z",
  "producer": {
    "module": "text-processing",
    "instance_id": "text-processing-local-1",
    "version": "0.1.0"
  },
  "correlation": {
    "trace_id": "trc_01J8N2MP9EY82NX5W9TH6D2MZG",
    "session_id": "ses_01J8N2JVPP0R4RYCPQKZT2CHFV",
    "request_id": "tpr_01J8N2N9CZ0QJ7TPMQKM5E0VZM",
    "recognition_id": "rec_01J8N2KQVCM4RMX71K27Z51H4S",
    "processing_id": "tps_01J8N2P8PM6W5S4J9SK57J8GPE",
    "source_id": "src_01J8N2J4NFRNT2VKJKH7S3H9ME",
    "content_id": "cnt_01J8N2JD0NEMVNJEN7D5YNE6QJ",
    "frame_id": "frm_01J8N2K6N7BQG6CY1WMNMKZFKX"
  },
  "causation": {
    "causation_event_id": "evt_01J8N2N7R9BGMVFMM70DFYMBHD",
    "root_event_id": "evt_01J8N2KZ8Z8QDWCRDV7KV4NBQG"
  },
  "subject": {
    "subject_type": "TextProcessingRequest",
    "subject_id": "tpr_01J8N2N9CZ0QJ7TPMQKM5E0VZM"
  },
  "sequence": {
    "stream_id": "text-processing:tpr_01J8N2N9CZ0QJ7TPMQKM5E0VZM",
    "number": 3
  },
  "partition_key": "ses_01J8N2JVPP0R4RYCPQKZT2CHFV",
  "delivery": {
    "delivery_mode": "AtLeastOnce",
    "durability": "Transient"
  },
  "privacy": {
    "classification": "SensitiveMetadata",
    "contains_source_text": false,
    "contains_image_data": false
  },
  "payload": {}
}
```

---

## 7. Event ID

```text
event_id
```

must:

* identify one event publication;
* be unique enough for deduplication;
* remain unchanged during redelivery;
* not be reused for another semantic event.

A retry that republishes the same event must preserve the same `event_id`.

A newly generated compensating or replacement event must use a new `event_id`.

---

## 8. Event Version

```text
event_version
```

versions the payload schema for one event name.

Recommended format:

```text
MAJOR.MINOR.PATCH
```

Event schema versioning is independent from:

```text
Text Processing contract version
module implementation version
processing profile version
```

---

## 9. Producer

```text
producer
```

identifies the module and instance publishing the event.

```text
EventProducer
├── module
├── instance_id?
└── version?
```

Required module value:

```text
text-processing
```

Commands may be produced by other modules.

---

## 10. Event Subject

The event subject identifies the primary entity described by the event.

For request lifecycle events:

```text
subject_type = TextProcessingRequest
subject_id = request_id
```

For module health events:

```text
subject_type = TextProcessingModule
subject_id = module instance or logical module ID
```

For configuration events:

```text
subject_type = TextProcessingConfiguration
subject_id = configuration snapshot ID
```

---

# Correlation

## 11. Correlation Fields

Relevant correlation fields include:

```text
trace_id
session_id
request_id
recognition_id
processing_id
source_id
content_id
frame_id
page_id
chapter_id
```

Not every event requires every field.

---

## 12. Required Correlation by Event

### `text_processing.requested`

Required:

```text
request_id
recognition_id
source_id
content_id
```

Recommended:

```text
trace_id
session_id
frame_id
```

### `text_processing.started`

Required:

```text
request_id
recognition_id
```

### Terminal events

Required:

```text
request_id
recognition_id
```

Completed should also contain:

```text
processing_id
```

---

## 13. Trace Correlation

All events in one processing lifecycle should share:

```text
trace_id
request_id
recognition_id
```

The Text Processing span should become a child of the event or operation that initiated processing.

---

## 14. Causation

```text
causation_event_id
```

points to the event that directly caused the current event.

Example:

```text
recognition.completed
        ↓ causes
text_processing.requested
        ↓ causes
text_processing.started
        ↓ causes
text_processing.completed
```

The exact publisher of `text_processing.requested` may be:

* Session;
* Orchestration;
* Workflow;
* Application Core.

Recognition should not necessarily command Text Processing directly.

---

## 15. Root Event

```text
root_event_id
```

identifies the initial event that started the workflow.

Possible root events:

```text
capture.completed
observation.stable_frame_ready
user.translation_requested
session.source_changed
```

This field supports end-to-end tracing.

---

# Event Streams and Ordering

## 16. Request Event Stream

Every request has a logical event stream:

```text
text-processing:<request_id>
```

Request events should include a monotonic sequence number.

Example:

```text
1 text_processing.requested
2 text_processing.started
3 text_processing.completed
```

or:

```text
1 text_processing.requested
2 text_processing.started
3 text_processing.cancellation_requested
4 text_processing.cancelled
```

---

## 17. Terminal Event Rule

Each accepted processing request must produce exactly one terminal lifecycle event:

```text
text_processing.completed
```

or:

```text
text_processing.failed
```

or:

```text
text_processing.cancelled
```

No request may validly produce more than one different terminal outcome.

---

## 18. Started Event Rule

An accepted request should produce:

```text
text_processing.started
```

before its terminal event.

A request rejected before acceptance may produce:

```text
text_processing.failed
```

without `started`, depending on the selected rejection policy.

Recommended policy:

```text
invalid command before acceptance
→ rejected by command handler
→ text_processing.failed with stage = Validation
```

This still creates a terminal event for observability.

---

## 19. Event Ordering Scope

Ordering is required per:

```text
request_id
```

Global ordering across all Text Processing requests is not required.

Ordering across different sessions is not required.

---

## 20. Out-of-Order Delivery

Consumers must tolerate out-of-order delivery.

They should use:

* request ID;
* sequence number;
* terminal-state precedence;
* event timestamp;
* state version.

A progress event received after a terminal event must not reopen the request.

---

## 21. Terminal State Precedence

Once a terminal event has been accepted:

```text
Completed
Failed
Cancelled
```

all later non-terminal events for the same request must be ignored or recorded as invalid late events.

A later conflicting terminal event must be treated as a consistency violation.

---

# Consumed Command Events

## 22. `text_processing.requested`

Requests processing of one Recognition result into a `SourceDocument`.

Producer may be:

```text
session
orchestration
workflow
application-core
```

Consumer:

```text
text-processing
```

---

## 23. Requested Event Payload

```text
TextProcessingRequestedPayload
├── request
├── recognition_result_reference?
├── configuration_snapshot_id?
├── supersession?
└── dispatch_metadata?
```

The `request` field follows `TextProcessingRequest`.

When the request already contains the result reference, the separate event-level reference may be omitted.

---

## 24. Requested Event Example

```json
{
  "event_name": "text_processing.requested",
  "event_version": "1.0.0",
  "payload": {
    "request": {
      "contract_version": "1.0.0",
      "request_id": "tpr_01J8N4BESD9DDB25RYDR1QSV8Y",
      "recognition": {
        "recognition_id": "rec_01J8N49EJZAMKMJ61P2H1XB0ZK",
        "result_reference": {
          "reference_id": "result_ref_01J8N49KYZTSP0G5AGZJZ07M7F",
          "reference_type": "RecognitionResult",
          "access_scope": "Session",
          "expires_at": "2026-07-22T04:00:00Z"
        }
      },
      "profile": {
        "profile_id": "ComicPage",
        "profile_version": "1.0.0"
      },
      "options": {
        "normalization_level": "Conservative",
        "enable_order_refinement": true,
        "enable_line_reconstruction": true,
        "enable_region_grouping": true,
        "enable_block_classification": true,
        "enable_noise_filtering": true,
        "preserve_excluded_blocks": true,
        "allow_partial_result": true
      },
      "context": {
        "session_id": "ses_01J8N46M7EX6T0FS3R0B5TSZ3W",
        "source_id": "src_01J8N45K41DNTQR6GJP1A9GJR4",
        "content_id": "cnt_01J8N45ZWGCNA7RG9NMTG6F4NE",
        "frame_id": "frm_01J8N4842QSPNFHB9Y8SBCYVY2",
        "expected_language": "zh-Hans",
        "document_type_hint": "ComicPage",
        "reading_direction_hint": "RightToLeft",
        "privacy_policy": {
          "processing_location": "LocalOnly",
          "diagnostic_text_allowed": false
        }
      },
      "timeout_ms": 5000,
      "priority": "Interactive",
      "requested_at": "2026-07-22T03:20:00Z",
      "trace_context": {
        "trace_id": "trc_01J8N4BNN1RG0NDW48WD7AQW7P",
        "parent_span_id": "spn_01J8N49ZAK9B57B9M53P0E4YWV"
      }
    },
    "configuration_snapshot_id": "cfg-tp-20260722-001",
    "supersession": {
      "supersedes_request_id": null,
      "supersession_key": "session:ses_01J8N46M7EX6T0FS3R0B5TSZ3W:frame"
    }
  }
}
```

---

## 25. Request Acceptance Rules

Text Processing should accept the command only when:

* the event version is supported;
* request contract version is supported;
* request ID is valid;
* request ID is not already bound to conflicting input;
* Recognition result is available or retrievable;
* source identity is consistent;
* requested profile is supported or fallback is allowed;
* privacy policy is satisfiable;
* the module is available;
* the request is not already superseded.

---

## 26. Duplicate Request Events

A duplicate `text_processing.requested` event may occur due to at-least-once delivery.

If the same `request_id` and equivalent request fingerprint are received:

```text
do not create new processing work
```

The handler should:

* acknowledge the duplicate;
* return the existing request status;
* optionally republish the known terminal event if recovery policy requires it.

If the same `request_id` carries different semantic input:

```text
reject as DuplicateRequestIdConflict
```

---

## 27. Request Fingerprint

The command handler should derive a request fingerprint from:

```text
recognition_id
recognition result fingerprint
profile reference
processing options
source identity
relevant context
```

Runtime fields such as timestamp should not change semantic equivalence.

---

## 28. `text_processing.cancellation_requested`

Requests cancellation of an active Text Processing request.

Producer may be:

```text
session
orchestration
application-core
user-interface
shutdown-coordinator
```

Consumer:

```text
text-processing
```

---

## 29. Cancellation Requested Payload

```text
TextProcessingCancellationRequestedPayload
├── request_id
├── recognition_id?
├── reason
├── requested_at
├── requested_by
├── superseding_request_id?
├── superseding_frame_id?
└── trace_context?
```

---

## 30. Cancellation Request Example

```json
{
  "event_name": "text_processing.cancellation_requested",
  "event_version": "1.0.0",
  "payload": {
    "request_id": "tpr_01J8N4BESD9DDB25RYDR1QSV8Y",
    "recognition_id": "rec_01J8N49EJZAMKMJ61P2H1XB0ZK",
    "reason": "NewerFrameAvailable",
    "requested_at": "2026-07-22T03:20:00.012Z",
    "requested_by": {
      "module": "session"
    },
    "superseding_request_id": "tpr_01J8N4C6SKB0EWVWJGSF7XNM4K",
    "superseding_frame_id": "frm_01J8N4BXX1DQKWMSSDNZ53WW6M"
  }
}
```

---

## 31. Cancellation Acceptance

Cancellation may be accepted while the request is in any non-terminal state.

Example states:

```text
Received
Validating
AdaptingInput
ResolvingOrder
Normalizing
ReconstructingLines
GroupingRegions
ClassifyingBlocks
BuildingDocument
ValidatingTraceability
AssemblingResult
PublishingResult
```

Cancellation after a terminal outcome is a no-op.

---

## 32. Duplicate Cancellation

Cancellation requests are idempotent.

Repeated cancellation commands must not create repeated terminal events.

At most one:

```text
text_processing.cancelled
```

may be produced.

---

# Produced Lifecycle Events

## 33. `text_processing.started`

Published when a request has been accepted and processing execution begins.

This event means:

* the request passed command-level acceptance;
* the request has an active execution context;
* an effective configuration snapshot has been selected;
* terminal-event ownership belongs to Text Processing.

---

## 34. Started Payload

```text
TextProcessingStartedPayload
├── request_id
├── recognition_id
├── effective_profile
├── configuration_snapshot_id
├── processing_mode
├── started_at
├── timeout_at?
├── queue_duration_ms?
└── supersession_key?
```

---

## 35. Started Event Example

```json
{
  "event_name": "text_processing.started",
  "event_version": "1.0.0",
  "payload": {
    "request_id": "tpr_01J8N4BESD9DDB25RYDR1QSV8Y",
    "recognition_id": "rec_01J8N49EJZAMKMJ61P2H1XB0ZK",
    "effective_profile": {
      "profile_id": "ComicPage",
      "profile_version": "1.0.0",
      "resolution_source": "ExplicitRequest"
    },
    "configuration_snapshot_id": "cfg-tp-20260722-001",
    "processing_mode": "Asynchronous",
    "started_at": "2026-07-22T03:20:00.003Z",
    "timeout_at": "2026-07-22T03:20:05.003Z",
    "queue_duration_ms": 3,
    "supersession_key": "session:ses_01J8N46M7EX6T0FS3R0B5TSZ3W:frame"
  }
}
```

---

## 36. Started Event Invariants

A request must produce no more than one semantic `started` event.

Redelivery of the same event is allowed.

`started` must not contain:

* Recognition payload;
* source text;
* source image;
* output document.

---

## 37. `text_processing.completed`

Published when Text Processing has successfully created, validated, stored, and committed a `TextProcessingResult`.

Completion means:

* `SourceDocument` is contract-valid;
* traceability validation succeeded;
* result storage succeeded when references are used;
* cancellation did not win the terminal race;
* exactly one terminal outcome was committed.

---

## 38. Completed Payload

```text
TextProcessingCompletedPayload
├── request_id
├── processing_id
├── recognition_id
├── result_reference
├── document_summary
├── effective_profile
├── warning_summary
├── metrics_summary
├── processing_fingerprint?
├── completed_at
└── supersession?
```

---

## 39. Document Summary

```text
SourceDocumentSummary
├── document_id
├── document_type
├── root_block_count
├── included_block_count
├── excluded_block_count
├── textual_block_count
├── synthetic_block_count
├── language_hints[]
├── reading_direction
├── partial
└── empty
```

The summary must not contain source text.

---

## 40. Warning Summary

```text
WarningSummary
├── total
├── info_count
├── warning_count
├── error_count
└── codes[]
```

Only warning codes and counts should be present.

---

## 41. Metrics Summary

Recommended completion metrics:

```text
total_duration_ms
input_region_count
input_line_count
output_block_count
excluded_block_count
order_change_count
line_join_count
region_group_count
```

Detailed stage metrics may remain in the result.

---

## 42. Completed Event Example

```json
{
  "event_name": "text_processing.completed",
  "event_version": "1.0.0",
  "payload": {
    "request_id": "tpr_01J8N4BESD9DDB25RYDR1QSV8Y",
    "processing_id": "tps_01J8N4CDWTKQBH4HWCZ2ZN3WCA",
    "recognition_id": "rec_01J8N49EJZAMKMJ61P2H1XB0ZK",
    "result_reference": {
      "reference_id": "result_ref_01J8N4CJWA19FCR63WTG70Y1EF",
      "reference_type": "TextProcessingResult",
      "processing_id": "tps_01J8N4CDWTKQBH4HWCZ2ZN3WCA",
      "content_hash": "sha256:56d31f6ff6495f8d502c8a6d8627439a...",
      "access_scope": "Session",
      "created_at": "2026-07-22T03:20:00.021Z",
      "expires_at": "2026-07-22T04:00:00Z"
    },
    "document_summary": {
      "document_id": "doc_01J8N4C9R0XEN7BXQDJ5GCQYS7",
      "document_type": "ComicPage",
      "root_block_count": 4,
      "included_block_count": 9,
      "excluded_block_count": 1,
      "textual_block_count": 9,
      "synthetic_block_count": 0,
      "language_hints": [
        "zh-Hans"
      ],
      "reading_direction": "RightToLeft",
      "partial": false,
      "empty": false
    },
    "effective_profile": {
      "profile_id": "ComicPage",
      "profile_version": "1.0.0",
      "resolution_source": "ExplicitRequest"
    },
    "warning_summary": {
      "total": 2,
      "info_count": 1,
      "warning_count": 1,
      "error_count": 0,
      "codes": [
        "ReadingOrderChanged",
        "LowClassificationConfidence"
      ]
    },
    "metrics_summary": {
      "total_duration_ms": 18,
      "input_region_count": 10,
      "input_line_count": 14,
      "output_block_count": 9,
      "excluded_block_count": 1,
      "order_change_count": 2,
      "line_join_count": 4,
      "region_group_count": 3
    },
    "processing_fingerprint": "sha256:4c53b8ce480acf12a87b72084682bbd4...",
    "completed_at": "2026-07-22T03:20:00.021Z",
    "supersession": {
      "supersession_key": "session:ses_01J8N46M7EX6T0FS3R0B5TSZ3W:frame",
      "is_current_at_publication": true
    }
  }
}
```

---

## 43. Completed Result Visibility

A completion event should be published only after the result reference is usable.

The invalid sequence is:

```text
publish completed
        ↓
store result
```

The correct sequence is:

```text
assemble result
        ↓
validate result
        ↓
store result
        ↓
commit terminal state
        ↓
publish completed
```

If publication fails after terminal commit, the outbox or recovery mechanism should republish the same event.

---

## 44. `text_processing.failed`

Published when a request reaches a non-recoverable failure.

A failure means no valid completed `SourceDocument` was committed.

---

## 45. Failed Payload

```text
TextProcessingFailedPayload
├── request_id
├── recognition_id?
├── processing_id?
├── error
├── warning_summary?
├── metrics_summary?
├── failed_at
├── retry_guidance?
└── supersession?
```

---

## 46. Event Error Summary

```text
EventErrorSummary
├── code
├── category
├── retryable
├── stage?
├── target_id?
├── message?
└── cause_reference?
```

Sensitive internal stack traces must not appear.

---

## 47. Failed Event Example

```json
{
  "event_name": "text_processing.failed",
  "event_version": "1.0.0",
  "payload": {
    "request_id": "tpr_01J8N5T5ED0EY6C8A7PSZYJN7S",
    "recognition_id": "rec_01J8N5SFA1ZNF29Z9BQH3KDWW2",
    "error": {
      "code": "InvalidRegionReference",
      "category": "Input",
      "retryable": false,
      "stage": "AdaptingInput",
      "target_id": "rgn_missing",
      "message": "A reading-order entry references an unavailable Recognition region."
    },
    "warning_summary": {
      "total": 0,
      "info_count": 0,
      "warning_count": 0,
      "error_count": 0,
      "codes": []
    },
    "metrics_summary": {
      "total_duration_ms": 3,
      "input_region_count": 8,
      "input_line_count": 11
    },
    "failed_at": "2026-07-22T03:30:00.006Z",
    "retry_guidance": {
      "retry_allowed": false,
      "requires_new_request_id": true,
      "requires_input_change": true
    }
  }
}
```

---

## 48. Failure Publication Rules

A failed event must:

* use a stable error code;
* identify whether retry may help;
* identify the failing stage when safe;
* avoid exposing source text;
* avoid exposing internal exception details;
* become the only terminal event.

---

## 49. Retry after Failure

A retry must use:

```text
new request_id
```

It may retain:

```text
recognition_id
source identity
trace lineage
previous request reference
```

A retry should not replay the same request ID as new work.

---

## 50. `text_processing.cancelled`

Published when cancellation commits before completion or failure.

---

## 51. Cancelled Payload

```text
TextProcessingCancelledPayload
├── request_id
├── recognition_id?
├── reason
├── requested_at?
├── cancellation_accepted_at?
├── cancelled_at
├── stage_at_cancellation?
├── superseding_request_id?
├── superseding_frame_id?
└── discarded_result?
```

---

## 52. Cancelled Event Example

```json
{
  "event_name": "text_processing.cancelled",
  "event_version": "1.0.0",
  "payload": {
    "request_id": "tpr_01J8N6EPJ5F5KVT4YBHQJVG4K2",
    "recognition_id": "rec_01J8N6DXTWQN3JW74AH3G23FGS",
    "reason": "NewerFrameAvailable",
    "requested_at": "2026-07-22T03:40:00.008Z",
    "cancellation_accepted_at": "2026-07-22T03:40:00.009Z",
    "cancelled_at": "2026-07-22T03:40:00.011Z",
    "stage_at_cancellation": "GroupingRegions",
    "superseding_request_id": "tpr_01J8N6F95BB9P01HDJ3TBM2RH1",
    "superseding_frame_id": "frm_01J8N6F3BTY1S0BP96P1TJHX7C",
    "discarded_result": false
  }
}
```

---

## 53. Discarded Late Result

If internal processing finishes after cancellation committed:

```text
discarded_result = true
```

may be used.

The late result must not:

* be stored as a completed result;
* be published as completed;
* trigger Translation;
* replace the current SourceDocument.

---

# Optional Progress Events

## 54. Progress Event Principles

Progress events are optional and observational.

Other modules must not require them for correctness.

They may support:

* diagnostics;
* development tooling;
* performance monitoring;
* detailed UI progress;
* profiling.

Consumers must remain correct when no progress events are emitted.

---

## 55. Progress Event Common Payload

```text
TextProcessingProgressPayload
├── request_id
├── recognition_id
├── stage
├── progress?
├── counts?
├── duration_ms?
├── warning_summary?
└── occurred_at
```

No full text payload should be included.

---

## 56. Progress Value

Optional `progress` may use:

```text
0.0 ≤ progress ≤ 1.0
```

Progress is approximate unless stage weights are fixed.

For fast local processing, emitting granular percentages may create more overhead than value.

---

## 57. `text_processing.input_adapted`

Published after Recognition input has been validated and converted into the internal processing model.

Payload may include:

```text
input_region_count
input_line_count
coordinate_space
source_dimensions_present
upstream_warning_count
duration_ms
```

---

## 58. `text_processing.order_resolved`

Published after effective reading order has been produced.

Payload may include:

```text
original_entry_count
resolved_entry_count
order_change_count
reading_direction
confidence
warning_codes[]
duration_ms
```

No ordered text or block payload should be included.

---

## 59. `text_processing.normalization_completed`

Published after raw text nodes have been normalized.

Payload may include:

```text
node_count
changed_node_count
normalization_change_count
normalization_level
warning_codes[]
duration_ms
```

---

## 60. `text_processing.lines_reconstructed`

Published after line groups have been created.

Payload may include:

```text
input_line_count
output_line_group_count
line_join_count
ambiguous_join_count
duration_ms
```

---

## 61. `text_processing.regions_grouped`

Published after source groups have been created.

Payload may include:

```text
input_region_count
output_group_count
region_group_count
preserved_region_count
ambiguous_group_count
duration_ms
```

---

## 62. `text_processing.blocks_classified`

Published after source groups have received structural classifications.

Payload may include:

```text
block_count
classified_count
unknown_count
low_confidence_count
type_counts
duration_ms
```

`type_counts` may contain block-type names and counts only.

---

## 63. `text_processing.document_built`

Published after `SourceDocument` assembly but before final traceability validation and terminal commit.

Payload may include:

```text
document_id
document_type
block_count
root_block_count
excluded_block_count
synthetic_block_count
partial_candidate
empty_candidate
duration_ms
```

The event does not mean the result is safe for downstream consumption.

---

## 64. `text_processing.traceability_validated`

Published after traceability validation succeeds.

Payload may include:

```text
document_id
total_input_regions
included_region_count
excluded_region_count
unresolved_region_count
coverage_ratio
duration_ms
```

This event still does not replace `text_processing.completed`.

---

## 65. Progress Event Frequency

Text Processing is expected to be low latency.

Recommended policy:

* progress events disabled by default in production;
* stage events enabled for diagnostics;
* no per-block events in normal mode;
* no per-character events;
* no event for every normalization rule;
* aggregate counts instead.

---

# Module Health Events

## 66. `text_processing.module_ready`

Published when the module can accept requests.

Payload:

```text
module_instance_id
supported_contract_versions[]
supported_profiles[]
active_configuration_snapshot_id
ready_at
```

---

## 67. `text_processing.module_degraded`

Published when the module can accept only a reduced set of requests or features.

Possible reasons:

```text
profile unavailable
rule-set load failure
result registry degraded
resource pressure
diagnostics unavailable
configuration fallback
```

Payload:

```text
module_instance_id
reason_codes[]
available_profiles[]
unavailable_profiles[]
degraded_at
```

---

## 68. `text_processing.module_unavailable`

Published when the module cannot accept new processing requests.

Payload:

```text
module_instance_id
reason_code
retryable
unavailable_at
expected_recovery?
```

The module should reject new requests with:

```text
ModuleUnavailable
```

---

## 69. `text_processing.module_stopping`

Published when shutdown begins.

New requests should no longer be accepted unless graceful-shutdown policy explicitly permits them.

Payload may include:

```text
active_request_count
shutdown_reason
shutdown_started_at
grace_period_ms?
```

---

## 70. `text_processing.module_stopped`

Published after module shutdown completes.

Payload may include:

```text
completed_request_count
cancelled_request_count
failed_request_count
stopped_at
```

---

# Configuration Events

## 71. `text_processing.configuration_changed`

Indicates that a new Text Processing configuration snapshot is available.

Producer:

```text
configuration
application-core
```

Consumer:

```text
text-processing
```

---

## 72. Configuration Changed Payload

```text
TextProcessingConfigurationChangedPayload
├── configuration_snapshot_id
├── previous_snapshot_id?
├── changed_categories[]
├── effective_at
├── requires_restart
└── compatibility
```

---

## 73. Configuration Categories

Possible values:

```text
DefaultProfile
ProfileDefinitions
NormalizationRules
OrderRules
GroupingRules
ClassificationRules
NoiseRules
ConfidenceThresholds
PerformanceLimits
Diagnostics
ResultRetention
Privacy
```

---

## 74. Configuration Snapshot Rule

An active request must use one immutable configuration snapshot.

A configuration event must not modify an in-flight request.

New configuration applies to requests accepted after:

```text
effective_at
```

or after atomic activation.

---

## 75. Configuration Compatibility

Payload may describe:

```text
BackwardCompatible
ResultChanging
RestartRequired
Unsupported
```

A result-changing configuration must update at least one of:

```text
profile version
rule-set version
configuration snapshot ID
processing fingerprint inputs
```

---

# Upstream Events

## 76. `recognition.completed`

Text Processing may observe `recognition.completed`, but should not necessarily begin processing automatically.

Recommended architecture:

```text
recognition.completed
        ↓
Session / Orchestration decides
        ↓
text_processing.requested
```

This avoids coupling Recognition directly to Text Processing policy.

---

## 77. Recognition Completion Requirements

When used to create a Text Processing request, the Recognition completion event should provide:

```text
recognition_id
result_reference
source identity
warning summary
completed_at
```

Text Processing must retrieve and validate the actual Recognition result.

---

## 78. Recognition Failure

`recognition.failed` normally prevents creation of a Text Processing request.

Text Processing should not receive an invalid placeholder Recognition result.

If a command still references a failed or unavailable result, Text Processing must fail with a normalized input/reference error.

---

## 79. Recognition Cancellation

A cancelled Recognition request should not lead to Text Processing.

If an already-created Text Processing request references a result later invalidated by upstream cleanup, the module should:

* cancel when active;
* fail when retrieval becomes impossible;
* never fabricate a document.

---

# Session and Source Events

## 80. `session.stopped`

Text Processing consumes or is indirectly notified of session shutdown.

Active requests associated with the session should be cancelled using:

```text
SessionStopped
```

Completed session-scoped result references may be invalidated during cleanup.

---

## 81. `source.closed`

When a source is closed:

* active requests for the source may be cancelled;
* stale results must not be presented;
* session-scoped result retention may be shortened;
* downstream Translation should not start for invalidated source scopes.

---

## 82. `application.shutdown_requested`

Text Processing should:

1. stop accepting new work;
2. cancel or drain active work according to policy;
3. publish valid terminal outcomes;
4. release result references and resources;
5. publish module stopped state.

---

# Supersession and Stale Results

## 83. Supersession Purpose

Live screen reading may create multiple rapidly changing frames.

A newer frame may make older Text Processing work irrelevant.

Supersession prevents stale documents from reaching Translation or Presentation.

---

## 84. Supersession Key

A request may include:

```text
supersession_key
```

Example:

```text
session:<session_id>:frame
```

or:

```text
source:<source_id>:selected-region
```

Only one request may be current for a key, depending on policy.

---

## 85. Superseding Request

A newer request may declare:

```text
supersedes_request_id
```

The orchestration layer should request cancellation of the older request.

Text Processing may also use a request registry to mark the older request stale.

---

## 86. Stale Completion Prevention

Before terminal completion, Text Processing should check:

* whether the request is still current;
* whether cancellation is committed;
* whether source/frame identity remains active;
* whether the result reference scope remains valid.

If stale:

```text
cancel or discard
```

according to supersession policy.

---

## 87. Completed but No Longer Current

A completion event may be valid but become stale before a consumer receives it.

Consumers such as Translation and Presentation must validate:

```text
session ID
source ID
frame ID
supersession key
current request ID
```

before acting.

---

## 88. Current-at-Publication Flag

A completion payload may include:

```text
is_current_at_publication
```

This is informative.

It does not replace consumer-side stale-result validation.

---

## 89. Historical Processing

Supersession policy differs for historical or batch processing.

For history/import/batch flows:

```text
older result
```

does not necessarily mean:

```text
stale result
```

Supersession must be explicitly scoped, not globally assumed.

---

# Duplicate Delivery and Idempotency

## 90. Delivery Semantics

The event system should assume:

```text
AtLeastOnce
```

unless stronger guarantees are proven.

Therefore:

* events may be duplicated;
* events may arrive late;
* events may arrive out of order;
* publication may be retried.

---

## 91. Consumer Deduplication

Consumers should deduplicate using:

```text
event_id
```

and, when necessary:

```text
request_id + event_name + terminal state
```

A deduplication record should live at least as long as the relevant result reference or workflow.

---

## 92. Command Idempotency

Command idempotency is based on:

```text
request_id
semantic request fingerprint
```

Same ID and same fingerprint:

```text
duplicate
```

Same ID and different fingerprint:

```text
conflict
```

---

## 93. Terminal Event Idempotency

Republishing the same terminal event after a broker or process failure must use the same:

```text
event_id
request_id
processing_id, for completion
terminal outcome
```

---

## 94. Duplicate Completion Consumption

Translation may receive the same `text_processing.completed` event more than once.

It must not create duplicate translation work.

Translation should derive its request idempotency key from:

```text
processing_id
translation policy
target language
translation request identity
```

---

# Retries

## 95. Internal Retry

Text Processing may internally retry transient operations such as:

* result retrieval;
* result-registry write;
* event publication through outbox;
* temporary resource acquisition.

Internal retry does not create a new `request_id`.

---

## 96. Processing Rule Retry

Deterministic local processing stages normally should not be retried blindly.

Examples:

```text
NormalizationFailed
RegionGroupingFailed
TraceabilityValidationFailed
```

These usually indicate:

* invalid input;
* implementation defect;
* unsupported data;
* broken configuration.

A retry with unchanged input is unlikely to help.

---

## 97. External Retry

After a terminal failure, orchestration may submit a new request.

It must use:

```text
new request_id
```

and may include:

```text
previous_request_id
retry_reason
retry_attempt
```

---

## 98. Retry Event Metadata

Optional metadata:

```text
RetryMetadata
├── attempt
├── previous_request_id?
├── retry_reason?
├── original_request_id
└── maximum_attempts?
```

Retry policy should remain outside the core SourceDocument contract.

---

## 99. Retry Limits

Retries must be bounded.

Suggested policies:

```text
result retrieval: small bounded retry
result storage: bounded retry with backoff
event publication: durable outbox retry
invalid input: no automatic retry
traceability failure: no automatic retry
```

---

# Cancellation Races

## 100. Completion vs Cancellation Race

Completion and cancellation may occur concurrently.

The module must atomically commit exactly one terminal state.

Conceptual rule:

```text
compare-and-set:
NonTerminal → Completed
```

or:

```text
compare-and-set:
NonTerminal → Cancelled
```

Only one may succeed.

---

## 101. Cancellation Before Result Storage

If cancellation commits before result storage begins:

* discard assembled document;
* do not store completed result;
* publish cancelled.

---

## 102. Cancellation During Result Storage

The module should define a commit boundary.

Recommended boundary:

```text
result stored but not terminally committed
```

does not yet mean completed.

If cancellation wins, the stored temporary result must be:

* deleted;
* marked orphaned;
* made inaccessible;
* or allowed to expire without publication.

---

## 103. Cancellation During Publication

If completion already committed and publication fails:

* cancellation cannot replace completion;
* the same completion event must be republished.

If cancellation committed first:

* no completion event may be published.

---

## 104. Failure vs Cancellation Race

Recommended rule:

* cancellation wins if accepted before non-recoverable failure commit;
* failure wins if terminal failure committed first;
* later command/event is a no-op;
* exactly one terminal event remains visible.

The chosen policy must be implemented atomically.

---

# Result Reference Events

## 105. Result Reference Requirement

`text_processing.completed` should contain a secure reference to:

```text
TextProcessingResult
```

The reference may indirectly provide access to:

```text
SourceDocument
```

---

## 106. Reference Validity

At publication time, the reference must be:

* stored;
* accessible within its scope;
* unexpired;
* integrity-checkable;
* linked to the same processing ID;
* linked to the same source identity.

---

## 107. Reference Expiry

If a completion event is consumed after reference expiry, the consumer should not treat it as a successful usable result.

Possible recovery:

* request reprocessing;
* retrieve persistent copy when available;
* ignore as stale;
* report result unavailable.

---

## 108. Reference Revocation

A reference may be revoked when:

* session ends;
* source is closed;
* privacy cleanup runs;
* result is superseded and policy removes old results;
* application shuts down;
* integrity validation fails.

Revocation should not rewrite historical event records.

---

## 109. Result Integrity

The completion payload may include:

```text
content_hash
```

The consumer should verify it when transport and registry support integrity checking.

---

# Event Privacy

## 110. Privacy Classification

Recommended classifications:

```text
PublicMetadata
InternalMetadata
SensitiveMetadata
SensitiveContent
```

Text Processing lifecycle events should normally use:

```text
SensitiveMetadata
```

because source and session identifiers may still be sensitive.

---

## 111. Source Text Prohibition

Normal events must use:

```text
contains_source_text = false
```

Raw or normalized text must not appear in:

* completion summaries;
* warning messages;
* error messages;
* progress
