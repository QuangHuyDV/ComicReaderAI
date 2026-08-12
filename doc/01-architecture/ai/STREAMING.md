# AI Streaming

* **Document:** AI Architecture / Streaming
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI processes incremental output from streaming-capable AI execution routes.

Streaming reduces perceived latency by allowing provisional AI output to become available before the complete response has been finalized.

Streaming MUST preserve the same logical AI operation contract as non-streaming execution.

It MUST remain:

* provider-neutral,
* incremental,
* ordered,
* cancelable,
* bounded,
* validation-aware,
* recovery-aware,
* observable.

Streaming changes **delivery timing**.

It MUST NOT silently change business semantics.

---

# Core Principle

```text
Model Execution
      |
      v
Raw Provider Stream
      |
      v
Provider Stream Adaptation
      |
      v
AIResponseChunk(s)
      |
      v
Stream Assembly
      |
      v
AIResponseCandidate
      |
      v
Final Validation
      |
      v
AIResponse
```

Optional provisional delivery may occur while chunks are being assembled.

The finalized `AIResponse` remains the canonical AI-operation result.

---

# Scope

Streaming architecture covers:

* provider stream normalization,
* provider-neutral chunk contracts,
* stream ordering,
* incremental assembly,
* provisional output,
* incremental checks,
* completion detection,
* cancellation,
* timeout/idle handling,
* stream failure classification,
* final response construction,
* streaming observability.

It does NOT own:

* Presentation rendering,
* UI state,
* Retry policy,
* Fallback selection,
* Routing,
* business-domain commit,
* provider credentials,
* Session state.

---

# Non-Goals

Streaming is NOT:

* a separate business operation,
* a separate Translation domain,
* a Presentation pipeline,
* a provider-specific protocol abstraction leaking into business code,
* automatic partial-result persistence,
* automatic fallback orchestration.

---

# Streaming vs Non-Streaming

Streaming and non-streaming are execution modes for the same logical AI capability.

Conceptually:

```text
AI Request
      |
      +--> Streaming Route
      |
      +--> Non-Streaming Route
```

Both SHOULD converge to:

```text
AIResponse
```

with equivalent logical output contracts.

---

# Streaming Requirement

AI Request MAY express:

```text
DISABLED
ALLOWED
REQUIRED
PREFERRED
```

or an equivalent provider-neutral streaming requirement.

Routing MUST reject non-streaming candidates when streaming is a hard requirement.

If streaming is merely preferred, Fallback MAY use non-streaming execution when explicitly allowed.

---

# Streaming Architecture

Recommended:

```text
Route Plan
    |
    v
Model Execution
    |
    v
Provider Stream Adapter
    |
    v
Chunk Normalization
    |
    v
Sequence Validation
    |
    v
Incremental Assembly
    |
    +--> Provisional Delivery
    |
    v
Completion Detection
    |
    v
Final Candidate
    |
    v
Final Validation
    |
    v
AIResponse Finalization
```

Recovery decisions remain external.

---

# Provider Stream Adapter

Provider Stream Adapter owns provider-specific stream protocols.

It MAY handle:

* SSE,
* chunked HTTP,
* WebSocket-like provider streams,
* SDK callbacks,
* provider event objects,
* provider-specific finish markers.

Provider-specific stream formats MUST NOT escape this boundary.

---

# Provider-Neutral Chunk

Recommended:

```text
AIResponseChunk
├── streamId
├── requestId
├── routePlanId?
├── sequence
├── chunkType
├── payload
├── provisionalMetadata?
├── finishState?
├── usageDelta?
├── createdAt
└── providerProvenanceReference?
```

The exact payload depends on capability.

---

# Chunk Types

Possible generic chunk types:

```text
TEXT_DELTA
STRUCTURED_DELTA
OBJECT_FIELD
TOOL_DELTA
USAGE_UPDATE
WARNING
FINISH
ERROR
HEARTBEAT
CUSTOM
```

MVP may support only a subset.

---

# Chunk Is Provisional

Critical rule:

```text
AIResponseChunk
    !=
AIResponse
```

A chunk is provisional execution output.

It MUST NOT automatically become durable business truth.

---

# Sequence

Chunks MUST carry deterministic ordering information.

Recommended:

```text
sequence = monotonically increasing integer
```

Provider-native sequence markers MAY be normalized.

---

# Arrival Order

Arrival order and logical sequence SHOULD normally agree.

If they differ:

```text
logical sequence
```

is authoritative after provider normalization where available.

---

# Duplicate Chunks

Streaming infrastructure SHOULD detect duplicate chunks where possible.

Duplicate delivery MUST NOT duplicate semantic output.

Deduplication MAY use:

* stream ID,
* sequence,
* provider event ID,
* chunk hash.

---

# Missing Chunks

A detected sequence gap MAY indicate:

```text
STREAM_SEQUENCE_GAP
```

Whether the stream can continue depends on provider/runtime capabilities.

---

# Out-of-Order Chunks

Out-of-order delivery MAY be buffered when:

* sequence is known,
* memory limits permit,
* deterministic reordering is safe.

Otherwise the stream SHOULD fail explicitly.

---

# Stream ID

Every streaming execution SHOULD have a provider-neutral:

```text
streamId
```

It MUST NOT depend on provider-native stream identity.

Provider stream IDs may be retained only as provenance.

---

# Stream State

Recommended runtime state:

```text
CREATED
OPENING
OPEN
RECEIVING
FINALIZING
COMPLETED
FAILED
CANCELLED
TIMED_OUT
```

These are runtime execution states.

They are not domain lifecycle states.

---

# Opening

`OPENING` means the provider request has been initiated but no usable stream has yet been established.

---

# Receiving

`RECEIVING` means provider-neutral chunks are being accepted and assembled.

---

# Finalizing

`FINALIZING` means no further semantic chunks are expected and final validation/normalization is running.

---

# Completed

`COMPLETED` means a finalized acceptable `AIResponse` has been produced.

---

# Failed

`FAILED` means streaming execution could not produce an acceptable final response.

The failure MAY still lead to Retry/Fallback through recovery orchestration.

---

# Cancellation

`CANCELLED` means the operation was intentionally cancelled.

Cancellation SHOULD stop:

* provider reads,
* parsing,
* assembly,
* provisional delivery,
* downstream recovery unless explicitly restarted.

---

# Stream Assembly

`StreamAssembler` incrementally combines normalized chunks into provisional state.

Recommended:

```text
StreamAssemblyState
├── streamId
├── requestId
├── lastSequence
├── assembledSemanticState
├── completenessState
├── warnings[]
├── usageAccumulator?
├── finishState?
└── version
```

This is runtime state.

---

# Assembly Must Be Deterministic

Given the same valid ordered chunk sequence and assembler version:

```text
assembled semantic state
```

SHOULD be semantically equivalent.

---

# Text Assembly

Text deltas MAY be appended incrementally.

Assembly MUST preserve intentional:

* spacing,
* Unicode,
* paragraph boundaries,
* mapping identifiers.

Provider-specific framing artifacts SHOULD be removed before generic assembly.

---

# Structured Assembly

Structured output MAY require incremental parsing.

Possible approaches:

* token accumulation,
* incremental JSON parser,
* schema-aware field assembly,
* provider-native structured events normalized into fields.

Partial structured objects MUST remain provisional.

---

# Partial Structured Output

A syntactically incomplete object MAY still be displayed provisionally where safe.

It MUST NOT be treated as a finalized valid structured response.

---

# Completion Detection

Stream completion MAY be indicated by:

* provider finish event,
* transport completion,
* normalized finish marker,
* expected structured object completion.

Completion detection belongs to stream adaptation/assembly.

---

# Finish State

Provider finish reasons SHOULD be normalized into CRAI semantics.

Possible:

```text
COMPLETED
LENGTH_LIMIT
CONTENT_RESTRICTED
TOOL_REQUIRED
CANCELLED
ERROR
UNKNOWN
```

A transport close alone MUST NOT automatically imply successful semantic completion.

---

# Provisional Delivery

Streaming MAY expose provisional output to callers.

Recommended:

```text
AIProvisionalOutput
├── streamId
├── sequence
├── content
├── completeness
├── provisionalWarnings[]
└── replaceable
```

Exact contract MAY reuse `AIResponseChunk`.

---

# Provisional Means Replaceable

Consumers MUST assume provisional output can:

* grow,
* be corrected,
* be discarded,
* restart after Retry,
* disappear after validation failure.

Therefore UI/Application MUST distinguish it from committed output.

---

# Presentation Boundary

Streaming MAY publish provisional output events.

Presentation MAY render them.

But:

```text
Presentation
    !=
AI Streaming
```

A rendering failure MUST NOT retroactively mean model execution failed.

---

# Business Commit Boundary

Critical rule:

```text
Provisional Stream Output
    !=
Durable Domain Commit
```

Examples:

```text
partial Translation text
    !=
TranslationRevision
```

```text
partial recognition output
    !=
committed TextBlock change
```

The owning capability determines commit rules.

---

# Incremental Validation

Some validation MAY run during streaming.

Examples:

* size limits,
* forbidden structural shape,
* sequence integrity,
* early schema impossibility,
* incremental safety signals,
* invalid encoding,
* impossible mapping keys.

---

# Final Validation

Other validation MUST wait until completion.

Examples:

* complete required-field presence,
* exact output mapping completeness,
* final JSON/schema validity,
* complete Language check,
* final terminology coverage,
* final semantic contract checks.

Therefore:

```text
Incremental Validation
    !=
Final Validation
```

---

# Validation Failure

Critical incremental failure MAY terminate the stream when continuing cannot produce a valid result.

Example:

```text
output exceeds hard maximum
```

or:

```text
mandatory schema becomes irrecoverably invalid
```

---

# Recoverable Validation Failure

Some validation failures may be recoverable after stream completion through:

* Repair,
* Retry,
* Fallback.

Streaming itself does not choose those strategies.

---

# Safety During Streaming

Safety evaluation MAY operate incrementally when required.

Possible behavior:

* suppress unsafe provisional delivery,
* terminate stream,
* mark chunks pending validation,
* require final safety confirmation.

Exact policy belongs to `SAFETY.md`.

---

# Provisional Safety

If output has not yet passed required safety checks:

```text
do not present it as fully trusted
```

Presentation behavior depends on application policy.

---

# Streaming and Response Validation

Finalized `AIResponse` MUST satisfy the same required logical validation contract as a non-streaming response.

Streaming MUST NOT create a weaker business-validation path.

---

# Usage Updates

Providers MAY emit incremental usage data.

Streaming Adapter MAY normalize:

```text
usageDelta
```

or cumulative usage snapshots.

Authoritative usage accounting belongs to Usage/Cost infrastructure.

---

# Usage Accumulation

Stream runtime MAY accumulate usage for operational reporting.

It MUST handle providers that report:

* per-chunk delta,
* cumulative totals,
* final-only usage,
* no usage.

---

# Time to First Output

Streaming observability SHOULD distinguish:

```text
timeToFirstTransportChunk
```

from:

```text
timeToFirstSemanticChunk
```

and where useful:

```text
timeToFirstRenderableOutput
```

These are not always equal.

---

# Idle Timeout

Streaming MAY have a dedicated:

```text
streamIdleTimeout
```

separate from:

```text
attemptTimeout
operationDeadline
```

Idle timeout measures absence of expected stream progress.

---

# Heartbeats

Provider/runtime heartbeats MAY reset transport-level idle detection without counting as semantic output.

Heartbeat events SHOULD normally not reach business consumers.

---

# Cancellation Sources

Streaming may be cancelled by:

* explicit user action,
* Session/application cancellation,
* operation deadline,
* attempt timeout,
* provider shutdown,
* policy/security intervention,
* application shutdown.

All SHOULD normalize into consistent cancellation semantics where possible.

---

# Cancellation Propagation

Recommended:

```text
Cancellation
    |
    +--> Provider Adapter
    +--> Stream Reader
    +--> Parser
    +--> Assembler
    +--> Provisional Delivery
```

Cancellation SHOULD propagate promptly.

---

# Provider Cancellation

If provider supports remote cancellation, adapter SHOULD use it.

If not, CRAI SHOULD:

* stop consuming locally,
* close transport where possible,
* mark attempt cancelled,
* preserve usage/provenance uncertainty.

---

# Cancellation Is Not Failure

User cancellation SHOULD normally produce:

```text
CANCELLED
```

rather than:

```text
FAILED
```

unless the surrounding API explicitly models all non-success outcomes as failures.

---

# Stream Failure

Possible normalized stream failures include:

```text
STREAM_OPEN_FAILED
STREAM_CONNECTION_INTERRUPTED
STREAM_IDLE_TIMEOUT
STREAM_SEQUENCE_INVALID
STREAM_SEQUENCE_GAP
STREAM_CHUNK_MALFORMED
STREAM_PROVIDER_PROTOCOL_ERROR
STREAM_ASSEMBLY_FAILED
STREAM_VALIDATION_FAILED
STREAM_FINALIZATION_FAILED
STREAM_CANCELLED
```

---

# Provider Failure Normalization

Provider-specific stream errors MUST be normalized before generic Retry/Fallback logic consumes them.

---

# Renderer Failure Boundary

The previous architecture included:

```text
Renderer failure
```

as an AI stream failure.

This MUST NOT remain an AI Streaming failure category.

Presentation failure belongs to Presentation/Application architecture.

---

# Recovery Boundary

Streaming reports:

* normalized failure,
* partial/provisional state,
* whether any output was externally exposed,
* whether restart is safe,
* provider resume capability,
* route information.

Recovery Policy decides the next action.

---

# Retry

Retry semantics are defined by `RETRY.md`.

Streaming MUST NOT independently implement hidden retry loops.

---

# Clean Restart

MVP SHOULD prefer:

```text
clean stream restart
```

for retry.

Recommended behavior:

```text
Stream A
    provisional chunks 1..N
    fails

Retry

Stream B
    starts from sequence 1 / new streamId
```

The application must know Stream A is superseded.

---

# Stream Supersession

Recommended event/metadata:

```text
StreamSuperseded
├── oldStreamId
├── newStreamId
├── reason
└── replacementMode
```

This prevents consumers from concatenating output from two independent attempts.

---

# Partial Output After Retry

Output from failed Stream A MUST NOT be automatically prepended to Stream B.

The new attempt owns a new semantic assembly sequence unless a verified resume protocol exists.

---

# Resume

Resume MAY be supported only when the provider/runtime provides a reliable continuation contract.

Resume semantics MUST specify:

* exact continuation position,
* chunk ordering,
* duplication handling,
* semantic equivalence,
* idempotency,
* provider provenance.

---

# Provider-Native Resume

Provider-native stream resume MUST NOT be assumed portable.

CRAI MAY expose a normalized resume capability only if semantic guarantees are strong enough.

MVP MAY defer it.

---

# Fallback

Fallback semantics are defined by `FALLBACK.md`.

When a new route is selected:

```text
new RoutePlan
+
new Execution Attempt
+
new streamId
```

MUST be created.

---

# Streaming to Non-Streaming Fallback

If:

```text
streamingPreferred
```

is degradable, fallback MAY select a non-streaming route.

If:

```text
streamingRequired
```

then non-streaming fallback is invalid.

---

# Non-Streaming Replacement

When streaming fallback becomes non-streaming:

* previous provisional stream must be marked superseded/terminated,
* final non-streaming response must not be concatenated blindly with previous partial output,
* UI may replace provisional output with final result.

---

# Route Change

A stream MUST NOT silently continue under another Model/Provider while retaining the same Route Plan identity.

Route change requires:

```text
new RoutePlan
new Attempt
new streamId
```

---

# Cost Boundary

Streaming MAY affect cost because:

* partial output may already be billed,
* cancellation may still incur usage,
* retry restarts generation,
* fallback may create another full execution.

Streaming records usage signals.

Cost Control owns authoritative cost policy/accounting.

---

# Cache Boundary

Partial stream output MUST NOT normally populate final-result Cache.

Only a finalized compatible response SHOULD enter ordinary AI result cache.

Special partial caches, if ever introduced, require separate semantics.

---

# Cache Hit Streaming

A cached finalized response MAY be delivered incrementally for UX purposes.

This is:

```text
delivery streaming
```

not:

```text
provider inference streaming
```

The distinction SHOULD remain observable.

---

# Backpressure

Streaming SHOULD support backpressure.

If consumer processing is slower than provider output, runtime MAY:

* buffer within limits,
* pause reads where transport permits,
* coalesce provisional chunks,
* drop only non-semantic telemetry chunks,
* cancel on overflow.

Semantic chunks MUST NOT be silently lost.

---

# Buffer Limits

Recommended constraints:

```text
maximumBufferedChunks
maximumBufferedBytes
maximumAssemblySize
```

Exceeding limits SHOULD produce explicit failure or backpressure behavior.

---

# Chunk Coalescing

UI-oriented provisional delivery MAY coalesce adjacent text chunks.

This MUST NOT alter final semantic assembly.

---

# Consumer Isolation

A slow Presentation/UI consumer SHOULD NOT necessarily block model stream assembly indefinitely.

Runtime MAY separate:

```text
canonical assembly
```

from:

```text
provisional delivery subscription
```

---

# Multiple Consumers

A stream MAY have multiple observers:

* Presentation,
* observability,
* debugging,
* application workflow.

Only the canonical assembler/finalizer owns final AIResponse construction.

---

# Streaming Events

Possible normalized events:

```text
AIStreamOpened
AIStreamChunkReceived
AIStreamProvisionalOutputAvailable
AIStreamCompleted
AIStreamFailed
AIStreamCancelled
AIStreamSuperseded
```

High-volume per-chunk events MAY remain runtime telemetry instead of durable event-bus events.

---

# Event Volume

Streaming MAY generate very high event frequency.

CRAI SHOULD distinguish:

```text
domain/application events
```

from:

```text
high-frequency runtime stream signals
```

Per-token events SHOULD NOT automatically be placed on the global durable Event Bus.

---

# Observability

Recommended metrics:

* stream count,
* stream open latency,
* time to first transport chunk,
* time to first semantic chunk,
* time to first provisional output,
* total stream duration,
* chunk count,
* semantic chunk count,
* bytes/units streamed,
* average inter-chunk delay,
* idle timeout count,
* cancellation rate,
* completion rate,
* stream restart count,
* stream fallback count,
* validation failure count,
* assembly failure count.

---

# Retry Metrics Boundary

Streaming may expose:

```text
streamRestartCount
```

but authoritative operation:

```text
retryCount
```

belongs to Retry/runtime observability.

---

# Fallback Metrics Boundary

Likewise:

```text
routeChangedDuringRecovery
```

may be observable.

Fallback history belongs to recovery/runtime records.

---

# Sensitive Observability

Streaming telemetry MUST NOT log raw chunk content by default.

Prefer:

```text
streamId
sequence
chunkType
size
hash
latency
```

Sensitive content logging requires explicit policy.

---

# Trace

Streaming spans MAY include:

```text
stream-open
first-chunk
assembly
finalization
```

Per-token spans SHOULD normally be avoided due to volume.

---

# Determinism

Given identical:

* ordered normalized chunks,
* assembler version,
* normalization rules,

final assembly SHOULD be semantically deterministic.

Provider generation itself may remain non-deterministic.

---

# Final Response Equivalence

Critical invariant:

```text
Streaming execution
and
Non-streaming execution
```

with semantically equivalent provider output SHOULD produce equivalent CRAI logical `AIResponse` structures.

---

# Final Response Identity

The finalized response receives its own:

```text
responseId
```

independent from:

```text
streamId
```

A stream may fail without producing a finalized Response.

---

# Stream Retention

Streaming data may have shorter retention than final AIResponse.

Possible policy:

```text
per-chunk raw payload
    very short / disabled

normalized chunk metadata
    short

final AIResponse
    normal execution retention

domain artifact
    owning-domain retention
```

---

# Raw Stream Retention

Raw provider chunks SHOULD NOT be persisted by default.

When diagnostics require retention:

* access must be restricted,
* Workspace policy must permit it,
* sensitive content must be protected,
* retention should be short.

---

# Stream Recovery Record

Runtime MAY retain:

```text
AIStreamExecutionRecord
├── streamId
├── requestId
├── routePlanId
├── attemptId
├── state
├── firstChunkAt?
├── lastChunkAt?
├── lastSequence?
├── provisionalExposed
├── finalResponseId?
├── normalizedFailure?
└── diagnosticReference?
```

This belongs to runtime execution architecture.

---

# Validation

Streaming architecture SHOULD validate:

* valid stream identity,
* valid Request/Attempt relationship,
* valid sequence,
* valid normalized chunk type,
* assembly limits,
* output contract compatibility,
* cancellation state,
* final completeness,
* final response contract.

---

# Architecture Invariants

1. Streaming is an execution mode, not a separate business workflow.

2. Streaming MUST remain provider-neutral after Provider Stream Adaptation.

3. Provider-specific stream formats MUST NOT escape the adapter boundary.

4. `AIResponseChunk` is distinct from finalized `AIResponse`.

5. Partial output is provisional.

6. Provisional output MUST NOT automatically become durable business truth.

7. Stream Assembly State is runtime state.

8. Final AIResponse is created only after completion/finalization rules succeed.

9. Chunk ordering MUST be preserved.

10. Duplicate chunks MUST NOT duplicate semantic output.

11. Sequence gaps MUST NOT be silently ignored.

12. Provider transport completion MUST NOT automatically imply semantic completion.

13. Streaming and non-streaming share the same logical final response contract.

14. Rendering is outside AI Streaming ownership.

15. Renderer/UI failure MUST NOT be classified as model streaming failure.

16. Presentation MAY consume provisional stream output.

17. Presentation MUST treat provisional output as replaceable.

18. Business commit remains owned by the calling capability/domain.

19. Incremental Validation and Final Validation are separate concepts.

20. Some validation rules MAY run incrementally.

21. Final required contract validation MUST occur before finalized AIResponse where required.

22. Streaming MUST NOT weaken mandatory output validation.

23. Safety MAY operate incrementally.

24. Streaming MUST respect cancellation.

25. Cancellation SHOULD propagate to provider/runtime promptly.

26. Cancellation is not automatically a failure.

27. Streaming MUST respect attempt timeout, idle timeout and overall operation deadline.

28. Idle timeout is distinct from total attempt timeout.

29. Retry is external to Streaming ownership.

30. Streaming MUST NOT hide automatic retry loops.

31. Fallback is external to Streaming ownership.

32. Route changes require a new RoutePlan.

33. Retry attempts create a new stream identity.

34. Fallback route execution creates a new stream identity.

35. Failed stream partial output MUST NOT be blindly concatenated with a restarted stream.

36. Stream supersession MUST be explicit when provisional output was exposed.

37. Provider-native resume MUST NOT be assumed portable.

38. Required streaming MUST NOT silently degrade to non-streaming.

39. Preferred streaming MAY degrade only when Fallback Policy allows it.

40. Partial stream output MUST NOT populate ordinary final-result Cache by default.

41. Streaming MUST support bounded buffering/backpressure.

42. Semantic chunks MUST NOT be silently dropped under backpressure.

43. High-frequency per-chunk events SHOULD NOT automatically become durable global events.

44. Streaming telemetry SHOULD avoid raw chunk content by default.

45. Usage updates are execution metadata.

46. Streaming does not own authoritative Usage/Cost accounting.

47. Finalized Response identity is separate from stream identity.

48. A stream may end without producing AIResponse.

49. Final assembly SHOULD be deterministic for identical normalized chunk sequences.

50. Raw provider stream retention SHOULD be minimized.

51. Stream execution records belong to runtime, not Domain.

52. Streaming failures SHOULD use normalized provider-neutral categories.

53. Retry/Fallback decisions SHOULD consume normalized stream failures.

54. Adding a new streaming provider SHOULD require adapter support, not changes to business capability contracts.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* provider-neutral `streamId`,
* `AIResponseChunk`,
* text deltas,
* basic structured deltas,
* monotonically increasing sequence,
* provider stream adapter,
* deterministic text assembly,
* basic structured assembly,
* completion detection,
* provisional text delivery,
* final response candidate,
* final response validation,
* cancellation,
* attempt timeout,
* stream idle timeout,
* clean retry restart,
* stream supersession,
* fallback to a new RoutePlan,
* optional streaming-to-non-streaming fallback when streaming is not required,
* bounded buffers,
* basic backpressure,
* time-to-first-semantic-output metric,
* chunk count,
* completion/cancellation metrics,
* safe streaming telemetry,
* no raw chunk logging by default.

MVP MAY defer:

* provider-native stream resume,
* cross-provider stream resume,
* resumable structured streams,
* tool-call streaming,
* multimodal output streaming,
* audio/video streaming,
* persistent per-chunk history,
* advanced chunk coalescing,
* multiple independent semantic streams per Request,
* distributed stream fan-out,
* exactly-once chunk transport across processes,
* partial-result Cache,
* adaptive backpressure.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact `AIResponseChunk` schema,
* chunk-type taxonomy,
* whether chunk sequence begins at 0 or 1,
* whether all providers can expose deterministic sequence,
* duplicate detection strategy,
* missing-chunk behavior,
* structured-output incremental parser,
* provisional-output contract,
* whether provisional output is event-based or callback/stream API,
* exact Stream State machine,
* finalization timeout,
* idle timeout defaults,
* heartbeat handling,
* stream buffer limits,
* backpressure implementation,
* chunk-coalescing rules,
* cancellation acknowledgment,
* provider usage-delta normalization,
* stream supersession event contract,
* provisional UI replacement semantics,
* whether retry always creates a new streamId,
* provider-native resume abstraction,
* streaming-to-non-streaming fallback UX,
* whether cached results may simulate streaming delivery,
* Stream Execution Record persistence,
* raw chunk diagnostic retention,
* streaming trace granularity,
* safety evaluation during provisional delivery,
* final response equivalence testing.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `STAGES.md`
* `REQUEST.md`
* `RESPONSE.md`
* `CONTEXT.md`
* `MEMORY.md`
* `PROMPTS.md`
* `MODELS.md`
* `ROUTING.md`
* `RETRY.md`
* `FALLBACK.md`
* `COST_CONTROL.md`
* `CACHE.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/TRANSLATION.md`
* `../domain/SESSION.md`
* `../domain/WORKSPACE.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/translation/`
* `../../02-modules/presentation/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`

Infrastructure:

* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/logging/`
