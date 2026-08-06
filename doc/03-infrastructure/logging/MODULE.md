# Logging Module

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Logging  
> **Document:** Module Architecture  
> **Path:** `03-infrastructure/logging/MODULE.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/configuration/MODULE.md`
> - `03-infrastructure/secret-management/MODULE.md`
> - `03-infrastructure/secret-management/CONTRACT.md`
> - `03-infrastructure/secret-management/EVENTS.md`
> - `03-infrastructure/secret-management/ERRORS.md`
> - `03-infrastructure/event-bus/MODULE.md`
> - `03-infrastructure/event-bus/CONTRACT.md`
> - `03-infrastructure/event-bus/EVENTS.md`
> - `03-infrastructure/event-bus/ERRORS.md`

---

## 1. Purpose

The Logging module provides the shared CRAI infrastructure for safe, structured, bounded, and searchable application logs.

It accepts structured log records from approved modules, applies policy and redaction, routes records to configured sinks, and manages buffering, formatting, persistence, rotation, retention, diagnostics, and shutdown.

Logging exists to support:

- development diagnostics;
- production troubleshooting;
- runtime observability;
- failure investigation;
- lifecycle analysis;
- safe support bundles;
- restricted security investigation;
- administrative audit routing.

Logging is not the source of domain truth.

Logs describe observations.

They do not replace module state, events, metrics, traces, or audit records.

---

## 2. Module Goal

The module must provide:

- structured log records;
- consistent severity;
- correlation and causation context;
- module and component identity;
- bounded asynchronous buffering;
- safe redaction;
- sensitive-field blocking;
- sink isolation;
- file rotation;
- retention control;
- backpressure behavior;
- safe exception normalization;
- restricted security logging;
- optional audit routing;
- diagnostics export;
- deterministic shutdown;
- fallback behavior when normal sinks fail.

The primary optimization target is:

```text
safe and useful diagnostic evidence
without leaking user data or secret material
```

---

## 3. Architectural Position

```text
Application / Module
    ↓ creates structured LogRecord
Logging API / Port
    ↓ enriches context
Logging Policy
    ↓ level + category + privacy checks
Redaction and Safety Inspection
    ↓
Bounded Log Buffer
    ↓
Sink Router
    ├── Console Sink
    ├── Rolling File Sink
    ├── Restricted Security Sink
    ├── Audit Sink Adapter
    ├── Debug Sink
    └── Future Remote Sink
```

Composition Root owns concrete logger construction, sink wiring, and startup/shutdown order.

---

## 4. Responsibilities

### 4.1 Structured logging

Logging accepts typed or structured records rather than unstructured string concatenation.

A record may contain:

- timestamp;
- severity;
- category;
- message template;
- safe properties;
- module identity;
- component identity;
- operation identity;
- correlation ID;
- causation ID;
- session ID;
- pipeline ID;
- task ID;
- work item ID;
- attempt ID;
- entity ID;
- normalized error code;
- exception reference;
- security classification.

### 4.2 Context enrichment

Logging may enrich records with safe execution context:

```text
applicationInstanceId
processInstanceId
thread or execution context class
sourceModule
sourceComponent
environment
applicationVersion
correlationId
causationId
sessionId
pipelineId
taskId
workItemId
attemptId
```

Enrichment must not inspect arbitrary application objects.

### 4.3 Severity policy

Logging owns normalized severity levels:

```text
TRACE
DEBUG
INFO
NOTICE
WARNING
ERROR
CRITICAL
FATAL
```

Severity indicates diagnostic importance.

It does not directly define:

- user impact;
- retryability;
- domain state;
- alerting policy;
- application shutdown behavior.

### 4.4 Category policy

Records are grouped into stable categories such as:

```text
APPLICATION
LIFECYCLE
CONFIGURATION
SECURITY
AUDIT
RUNTIME
PROVIDER
TRANSLATION
RECOGNITION
PRESENTATION
STORAGE
NETWORK
EVENT_BUS
SECRET_MANAGEMENT
PERFORMANCE
DIAGNOSTICS
INTERNAL
```

### 4.5 Redaction

Logging owns a mandatory redaction boundary.

It must detect and block or sanitize:

- secret values;
- authorization headers;
- access tokens;
- refresh tokens;
- passwords;
- private keys;
- credentials;
- raw environment values;
- unsafe URLs;
- sensitive query parameters;
- full user documents;
- screenshots;
- OCR text;
- translated content;
- provider raw responses;
- unredacted exception messages;
- unsafe file paths where policy requires masking.

### 4.6 Buffering

Logging owns bounded buffers between producers and sinks.

Buffers must support:

- bounded capacity;
- priority-aware admission;
- low-severity dropping;
- duplicate suppression where safe;
- burst tolerance;
- shutdown drain;
- emergency reserve for critical records.

### 4.7 Sink routing

Logging routes records based on:

- severity;
- category;
- security classification;
- environment;
- sink capability;
- retention policy;
- user setting;
- diagnostic mode.

### 4.8 File lifecycle

For file sinks, Logging owns:

- file creation;
- append behavior;
- rolling;
- maximum file size;
- maximum retained files;
- age-based retention;
- compression where supported;
- atomic handoff;
- cleanup;
- shutdown flush.

### 4.9 Safe exception handling

Logging normalizes exceptions before persistence.

It may retain:

- exception type;
- normalized error code;
- safe message;
- safe stack frames;
- correlation context;
- module identity.

It must not persist raw exceptions when they may contain sensitive data.

### 4.10 Diagnostics export

Logging may produce a safe diagnostics bundle containing:

- selected recent logs;
- configuration summary;
- application version;
- platform summary;
- normalized errors;
- module health summaries;
- redaction report.

Exports must pass a second redaction and safety inspection.

### 4.11 Restricted security logging

Security-relevant records use stricter routing and visibility.

Examples:

- secret exposure blocked;
- unauthorized Event Bus publisher;
- secret consumer mismatch;
- backend compromise;
- unsafe serialization;
- raw secret found in configuration.

### 4.12 Audit routing

Logging may expose an adapter for durable audit records.

Audit semantics remain distinct from ordinary logs.

Audit records should use:

- stronger retention;
- restricted access;
- append-only behavior;
- explicit actor and action;
- independent failure policy.

---

## 5. Non-Responsibilities

The Logging module does not own the following.

### 5.1 Metrics

Counters, gauges, histograms, and time-series aggregation belong to Telemetry / Metrics.

### 5.2 Distributed tracing

Span lifecycle and trace sampling belong to Telemetry / Tracing.

Logging consumes trace and correlation context but does not own tracing.

### 5.3 Domain events

Logs are not integration events.

The Event Bus transports domain and integration facts.

### 5.4 Domain state

Logs cannot determine current module state.

### 5.5 Business audit semantics

Each owning module defines which actions require audit.

Logging only routes or persists approved audit records.

### 5.6 User-facing notifications

Logs are not UI messages.

Presentation owns user-facing communication.

### 5.7 Secret lifecycle

Secret Management owns secret identity, storage, rotation, revocation, and material access.

Logging only enforces that secret material never enters log output.

### 5.8 Crash reporting service

A future crash-reporting adapter may consume safe diagnostics.

Logging does not automatically upload crash data in the MVP.

### 5.9 Remote support access

The MVP does not provide remote log browsing or automatic cloud upload.

---

## 6. Core Architectural Principle

Logs are diagnostic observations.

```text
State
    = current accepted truth

Event
    = immutable business or integration fact

Log
    = diagnostic observation

Metric
    = aggregated measurement

Trace
    = causal execution path

Audit Record
    = restricted administrative evidence
```

These concepts must remain separate.

---

## 7. MVP Architecture

The initial CRAI Logging implementation should be:

```text
In-process
Structured
Asynchronous
Bounded
Redaction-first
Local-first
Rolling-file capable
Console/debug capable
Payload-safe
```

MVP sinks:

```text
Console or Debug Sink
Rolling File Sink
Restricted Security File Sink
In-memory Diagnostic Buffer
```

Optional MVP sink:

```text
Audit File Sink
```

Deferred:

```text
Remote log aggregation
Cloud upload
OpenTelemetry log exporter
Syslog
Windows Event Log
macOS Unified Logging adapter
Linux journald adapter
Centralized organization log service
```

---

## 8. Logging API

Conceptual API:

```text
Logger {
    trace(record)
    debug(record)
    info(record)
    notice(record)
    warning(record)
    error(record)
    critical(record)
    fatal(record)
}
```

Preferred usage is structured:

```text
logger.error(
    messageTemplate = "Provider request failed",
    properties = {
        providerId,
        operationId,
        normalizedErrorCode
    }
)
```

Avoid:

```text
logger.error(
    "Provider request failed: " + rawResponse + " token=" + accessToken
)
```

---

## 9. Log Record

Conceptual structure:

```text
LogRecord {
    recordId
    occurredAt
    receivedAt?

    severity
    category

    messageTemplate
    renderedMessage?

    sourceModule
    sourceComponent?

    correlationId?
    causationId?

    applicationInstanceId
    processInstanceId?

    sessionId?
    pipelineId?
    taskId?
    workItemId?
    attemptId?
    entityId?
    operationId?

    normalizedErrorCode?
    exceptionSummary?

    securityClassification
    privacyClassification

    properties
    tags
}
```

Detailed contracts belong in `CONTRACT.md`.

---

## 10. Message Template Rule

Message templates should be stable.

Preferred:

```text
"Provider request failed"
```

with structured fields:

```text
providerId
operationId
normalizedErrorCode
```

Avoid embedding variable data directly into the template.

This improves:

- searchability;
- aggregation;
- redaction;
- cardinality control;
- localization independence;
- diagnostics.

---

## 11. Severity Model

### TRACE

Very detailed execution diagnostics.

Disabled by default in production.

### DEBUG

Developer-oriented state and flow information.

### INFO

Normal lifecycle and successful significant operations.

### NOTICE

Expected but noteworthy conditions.

### WARNING

Degraded or recoverable behavior.

### ERROR

An operation failed.

### CRITICAL

A security, invariant, or broad subsystem failure.

### FATAL

The application cannot continue safely.

---

## 12. Severity Does Not Own Action

A `CRITICAL` log does not itself:

- shut down the application;
- revoke a secret;
- retry work;
- show a dialog;
- publish an event.

The owning module performs the state transition or recovery action.

Logging records the result.

---

## 13. Privacy Classification

Canonical privacy classes:

```text
PUBLIC
INTERNAL
CONFIDENTIAL_METADATA
RESTRICTED_SECURITY
USER_CONTENT
SECRET
```

Rules:

- `SECRET` data must be blocked, not logged;
- `USER_CONTENT` is denied by default;
- `RESTRICTED_SECURITY` uses restricted sinks;
- `CONFIDENTIAL_METADATA` may require masking;
- `PUBLIC` does not imply unrestricted remote upload.

---

## 14. Security Classification

Suggested classifications:

```text
INTERNAL
CONFIDENTIAL
RESTRICTED_SECURITY
AUDIT_RESTRICTED
```

Security classification controls routing and retention.

It does not weaken redaction.

---

## 15. Structured Property Policy

Allowed examples:

```text
providerId
modelId
operationId
normalizedErrorCode
state
previousState
currentState
durationClass
retryCount
queueDepth
revision
backendType
eventType
subscriberId
```

Prohibited examples:

```text
rawPrompt
fullNovelText
fullComicText
screenshotBytes
imageBase64
apiKey
password
accessToken
refreshToken
privateKey
authorizationHeader
rawProviderResponse
rawEnvironmentDump
```

---

## 16. User Content Policy

User content is denied by default.

This includes:

- novel text;
- comic text;
- OCR output;
- translated output;
- images;
- screenshots;
- clipboard contents;
- selected text;
- filenames when sensitive;
- page URLs when they expose reading history.

Where diagnostics need content context, use:

```text
contentLength
contentType
language
artifactId
documentId
contentHashReference
boundedSafeSnippet?
```

A safe snippet is permitted only under an explicit contract and privacy mode.

---

## 17. Secret Safety

Logging must integrate with Secret Management redaction rules.

The module must block:

```text
known secret values
secret-bearing types
authorization headers
PEM private-key blocks
token-like fields
password fields
unsafe references
decrypted credential objects
```

Debug mode never disables secret blocking.

---

## 18. Exception Safety

Raw exception messages can contain:

- URLs;
- headers;
- request bodies;
- file paths;
- credentials;
- user text;
- provider responses.

Therefore:

```text
Raw Exception
    ↓
Exception Normalizer
    ↓
Sensitive Data Inspection
    ↓
Safe Exception Summary
    ↓
Log Record
```

---

## 19. Stack Trace Policy

Stack traces may be retained when:

- environment policy permits;
- frames are application or approved library frames;
- arguments and local variables are excluded;
- paths are normalized or masked;
- exception text is redacted.

Production may use reduced stack traces.

---

## 20. Correlation

Logging preserves:

```text
correlationId
causationId
operationId
sessionId
pipelineId
taskId
workItemId
attemptId
```

This enables investigation across:

- Runtime;
- Event Bus;
- Secret Management;
- Provider Management;
- Translation;
- Recognition;
- Presentation.

---

## 21. Context Propagation

Context should flow through:

- application services;
- Runtime work items;
- Event Bus envelopes;
- provider calls;
- async tasks;
- cancellation scopes;
- child processes where supported.

Missing optional context must not block logging.

Invalid or unsafe context must be removed.

---

## 22. Log Scope

A log scope may add temporary safe properties:

```text
using LogScope {
    sessionId
    pipelineId
    operationId
}
```

Scopes must:

- be immutable;
- be nestable;
- restore correctly;
- avoid user content;
- avoid secret values;
- not leak between unrelated async operations.

---

## 23. Asynchronous Pipeline

Normal logging flow:

```text
Producer creates LogRecord
    ↓
Fast validation
    ↓
Context enrichment
    ↓
Redaction and safety inspection
    ↓
Bounded buffer admission
    ↓
Sink routing
    ↓
Sink write
```

Producers should not block on slow file I/O during normal operation.

---

## 24. Synchronous Emergency Path

Critical or fatal records may use a minimal synchronous fallback when:

- the asynchronous pipeline is unavailable;
- normal sinks failed;
- shutdown is near completion;
- a security incident must be recorded locally.

The emergency path must be:

- bounded;
- payload-safe;
- minimal;
- non-recursive;
- local;
- unable to throw back into application code.

---

## 25. Buffering

Logging buffers are bounded.

Possible buffer classes:

```text
NORMAL
SECURITY
AUDIT
EMERGENCY
```

Properties:

- fixed or configured capacity;
- severity-aware admission;
- bounded producer wait;
- drop accounting;
- shutdown drain;
- no unbounded memory growth.

---

## 26. Buffer Overflow

Recommended overflow policy:

```text
TRACE / DEBUG
    → drop first

INFO / NOTICE
    → sample or drop under pressure

WARNING
    → retain when possible

ERROR / CRITICAL / FATAL
    → reserved capacity or emergency path
```

Logging must record aggregate drop counts without recursively logging each drop.

---

## 27. Duplicate Suppression

Repeated identical low-value records may be suppressed.

Example:

```text
"Backend unavailable"
repeated 10,000 times
```

may become:

```text
Backend unavailable
suppressedCount = 9,999
```

Suppression must not hide:

- state transitions;
- security violations;
- first occurrence;
- recovery occurrence;
- terminal failures;
- audit records.

---

## 28. Sampling

Sampling may apply to:

- TRACE;
- DEBUG;
- repetitive INFO;
- high-frequency progress logs;
- repeated transient failures.

Sampling must not apply to:

- FATAL;
- CRITICAL security incidents;
- audit records;
- secret exposure;
- backend compromise;
- state corruption;
- first terminal failure.

---

## 29. Sink Types

Potential sink types:

```text
CONSOLE
DEBUG_OUTPUT
ROLLING_FILE
RESTRICTED_SECURITY_FILE
AUDIT_FILE
IN_MEMORY
REMOTE
PLATFORM_NATIVE
NULL
```

---

## 30. Sink Contract

A sink should expose:

```text
initialize
write
flush
rotate?
health
shutdown
```

Sinks must not receive records they are unauthorized to store.

---

## 31. Sink Isolation

One failing sink must not block unrelated sinks.

Possible behavior:

- mark sink degraded;
- buffer temporarily;
- disable sink;
- reroute eligible records;
- use fallback sink;
- emit safe health signal.

A restricted record must not fall back to an unrestricted sink.

---

## 32. Console Sink

Console output is intended for:

- development;
- command-line diagnostics;
- local debugging.

Production desktop builds may disable it.

Console output must still use redacted records.

---

## 33. Rolling File Sink

The rolling file sink should support:

```text
maximumFileSize
maximumRetainedFiles
maximumRetentionAge
rollOnStartup?
rollOnSize
rollOnDate?
compression?
flushInterval
```

The default file format should be structured line-oriented data, such as JSON Lines or a stable text template.

Exact format belongs in `CONTRACT.md`.

---

## 34. Restricted Security Sink

Security records may use a separate sink with:

- tighter file permissions;
- restricted diagnostics access;
- shorter or policy-specific retention;
- no general UI browsing;
- no fallback to normal console in production;
- mandatory redaction.

---

## 35. Audit Sink

Audit records differ from ordinary logs.

Audit records should be:

- append-only;
- actor-aware;
- action-aware;
- result-aware;
- tamper-evident where practical;
- restricted;
- retained by policy;
- not sampled;
- not suppressed.

The MVP may implement a local restricted audit file.

A future dedicated Audit module may replace this adapter.

---

## 36. In-Memory Diagnostic Buffer

A bounded in-memory sink may retain recent safe records for:

- diagnostics screen;
- crash summary;
- support bundle;
- tests.

It must:

- have bounded capacity;
- use safe records only;
- support filtering;
- be cleared on shutdown where policy requires;
- not retain secret material;
- not retain full user content.

---

## 37. Remote Sink

Remote logging is deferred.

Future remote sinks must define:

- explicit opt-in or policy;
- encryption in transit;
- authentication;
- batching;
- retry;
- offline buffering;
- privacy;
- redaction;
- retention;
- regional policy;
- user consent where required.

---

## 38. File Location

Log files should use platform-appropriate application data directories.

The module must not hard-code user-visible project folders.

Exact paths belong to platform adapters and Configuration.

---

## 39. File Permissions

Logging should apply the most restrictive permissions practical.

Restricted security and audit logs require stronger access restrictions than normal logs.

The module must not claim guarantees stronger than the operating system provides.

---

## 40. Rotation

Rotation flow:

```text
Active log file
    ↓ size/date/startup threshold
Flush
    ↓
Close active file
    ↓
Rename or finalize
    ↓
Open new active file
    ↓
Retention cleanup
```

Rotation must avoid losing already accepted critical records where practical.

---

## 41. Retention

Retention may be based on:

```text
age
file count
total size
classification
environment
sink type
```

Retention policy must be bounded.

Logs must not grow indefinitely.

---

## 42. Compression

Old log files may be compressed if:

- compression is bounded;
- failure does not block active logging;
- restricted classification is preserved;
- permissions remain safe;
- shutdown does not wait indefinitely.

---

## 43. Flush Policy

Possible flush modes:

```text
PER_RECORD
INTERVAL
ON_ERROR
ON_CRITICAL
ON_ROTATION
ON_SHUTDOWN
MANUAL
```

Recommended MVP:

```text
interval flush
immediate or expedited flush for CRITICAL/FATAL
flush on rotation
bounded flush on shutdown
```

---

## 44. Logging Lifecycle

Conceptual lifecycle:

```text
CREATED
    ↓
INITIALIZING
    ↓
RUNNING
    ↓
DEGRADED
    ↓
QUIESCING
    ↓
FLUSHING
    ↓
STOPPING
    ↓
TERMINATED
```

Failure state:

```text
FAILED
```

Detailed states belong in `STATES.md`.

---

## 45. Sink Lifecycle

Conceptual lifecycle:

```text
UNREGISTERED
REGISTERED
INITIALIZING
AVAILABLE
DEGRADED
UNAVAILABLE
ROTATING
FLUSHING
STOPPING
TERMINATED
FAILED
```

---

## 46. Startup

Recommended startup order:

```text
Configuration available
    ↓
Secret-safe logging policy available
    ↓
Minimal emergency logger available
    ↓
Normal sinks initialize
    ↓
Logging pipeline starts
    ↓
Other infrastructure starts
```

Logging should be available before Event Bus, Runtime, and most feature modules begin normal work.

---

## 47. Bootstrap Logging

Before the full pipeline starts, a minimal bootstrap logger may record:

- startup stage;
- configuration load failure;
- sink initialization failure;
- fatal dependency failure.

Bootstrap logging must be:

- synchronous;
- local;
- minimal;
- payload-safe;
- replaceable by normal logging after startup.

---

## 48. Shutdown

Recommended order:

```text
Stop normal application work
    ↓
Event Bus drains
    ↓
Logging rejects low-priority new records
    ↓
Logging drains buffers
    ↓
Flush critical and audit sinks
    ↓
Close files
    ↓
Terminate
```

Logging should shut down after most other modules so their final lifecycle records can be captured.

---

## 49. Shutdown Bound

Shutdown flush must use a finite deadline.

Remaining records may be:

- flushed;
- dropped by severity policy;
- written through emergency path;
- summarized as lost count.

The application must not wait indefinitely.

---

## 50. Logging Failure Semantics

Logging failure must not normally crash business operations.

However, some failures may require fail-closed behavior:

- security sink unavailable while policy mandates it;
- audit sink unavailable for a mandatory audited action;
- redaction unavailable;
- payload inspection corrupted;
- unrestricted fallback would leak restricted records.

---

## 51. Redaction Failure

If redaction cannot determine that a record is safe:

```text
block the record
```

Fail-open logging is prohibited for sensitive boundaries.

A minimal safe failure record may be emitted without the unsafe content.

---

## 52. Sink Failure

Sink failure flow:

```text
Write fails
    ↓
Normalize failure
    ↓
Mark sink DEGRADED or UNAVAILABLE
    ↓
Attempt eligible fallback
    ↓
Preserve classification
    ↓
Record aggregate loss safely
```

---

## 53. Fallback Rules

Fallback is allowed only when the destination can preserve:

- privacy classification;
- security classification;
- retention requirements;
- redaction guarantees.

Examples:

```text
Normal rolling file unavailable
    → fallback to safe console or in-memory buffer

Restricted security sink unavailable
    → do not fallback to unrestricted console

Audit sink unavailable
    → block mandatory audit action or use approved emergency audit store
```

---

## 54. Error Handling

Logging errors must themselves be logged through a non-recursive internal path.

They must not cause:

```text
Logging fails
    ↓
Logging logs the failure
    ↓
Logging fails
    ↓
infinite recursion
```

The module needs an internal recursion guard and emergency reporter.

---

## 55. Event Bus Interaction

Logging may consume safe Event Bus lifecycle and failure facts.

Logging must not depend on Event Bus to write its own critical internal failures.

Logging may publish safe health events only after its state transition is committed.

Self-reporting must avoid Event Bus ↔ Logging loops.

---

## 56. Secret Management Interaction

Secret Management provides:

- secret-type identification;
- safe reference redaction rules;
- known secret value matching where permitted;
- restricted security classifications.

Logging must never resolve secrets for logging.

It may only use safe inspection and redaction capabilities.

---

## 57. Configuration Interaction

Configuration controls:

```text
minimum severity
category overrides
sink enablement
file size
retention
flush interval
buffer capacity
diagnostic mode
restricted sink settings
audit settings
```

Raw secrets must not appear in Logging configuration.

Remote sink credentials use `SecretReference`.

---

## 58. Runtime Interaction

Runtime logs:

- work lifecycle;
- queue pressure;
- cancellation;
- retries;
- abandonment;
- resource pressure;
- shutdown.

Runtime should log normalized IDs and states, not payloads.

---

## 59. Provider Management Interaction

Provider logs may include:

```text
providerId
modelId
operationId
capability
normalizedErrorCode
latencyClass
retryCount
circuitState
rateLimitState
```

They must not include:

```text
raw request body
raw response body
authorization header
API key
user text
translated text
OCR content
```

---

## 60. Presentation Interaction

Presentation may expose a safe log viewer for development or diagnostics.

It must not:

- show restricted security logs without authorization;
- show audit logs by default;
- expose raw files;
- allow secret-bearing search;
- block UI while logs load.

---

## 61. Telemetry Interaction

Telemetry may consume safe logging pipeline health.

Logging may enrich records with trace IDs and span IDs.

Logging must not own:

- trace creation;
- metric aggregation;
- sampling decisions for traces;
- exporter lifecycle for metrics/traces.

---

## 62. Audit Versus Security Logs

Security log:

```text
diagnostic record about a security-relevant condition
```

Audit record:

```text
restricted evidence that an actor performed or attempted an action
```

One operation may produce both.

Example:

```text
User removes secret
    → Audit record: actor removed secret
    → Security log: provider revocation failed
```

---

## 63. Diagnostics Bundle

A support bundle may include:

```text
application version
platform summary
configuration summary without secrets
recent safe logs
normalized error summary
module health summary
Event Bus health
Secret Management health
logging health
redaction report
```

It must exclude:

```text
secret values
raw user content
full URLs when sensitive
raw provider payloads
private keys
authorization data
clipboard contents
screenshots unless explicitly selected
```

---

## 64. Diagnostics Export Workflow

```text
User requests export
    ↓
Select approved records
    ↓
Apply export-specific redaction
    ↓
Inspect complete bundle
    ↓
Generate manifest
    ↓
Write bundle
    ↓
Return safe path/reference
```

The export must not simply copy active log files without inspection.

---

## 65. Log Format

Preferred formats:

```text
JSON Lines
Structured text template
```

JSON Lines is suitable for machine analysis.

Structured text is suitable for direct local reading.

The architecture should support both through formatters.

---

## 66. Formatter Contract

A formatter converts a safe `LogRecord` into sink output.

It must:

- preserve stable fields;
- avoid adding unsafe data;
- escape values;
- enforce size limits;
- not inspect arbitrary object graphs;
- remain deterministic.

---

## 67. Record Size

Each record has a maximum serialized size.

Oversized properties should be:

- rejected;
- truncated using explicit marker;
- summarized;
- replaced by artifact reference.

Critical identity fields must remain intact.

---

## 68. Property Cardinality

Logging may store high-cardinality properties because logs are event records, but fields must still be bounded.

Metrics derived from logs must not automatically use:

- event ID;
- session ID;
- task ID;
- file path;
- full error message;
- user-controlled values;

as metric labels.

---

## 69. Time Semantics

Every record uses:

```text
occurredAt
```

The pipeline may add:

```text
receivedAt
writtenAt
```

Wall-clock time supports investigation.

Monotonic durations should be logged as explicit duration values rather than inferred from wall-clock timestamps.

---

## 70. Clock Failure

If the wall clock changes:

- record ordering may use sequence numbers within one process;
- timestamps remain as observed;
- the module may mark clock discontinuity;
- logs must not silently rewrite prior timestamps.

---

## 71. Thread Safety

The logging API must be safe for concurrent calls.

Producers must not share mutable records after submission.

---

## 72. Immutability

A `LogRecord` becomes immutable after admission.

Sink-specific formatting must not mutate the original record.

---

## 73. Cancellation

Normal log calls should not require caller cancellation.

Lifecycle operations such as:

- initialize;
- flush;
- rotate;
- export;
- shutdown;

accept cancellation or bounded deadlines.

Canceling a flush does not mutate already written records.

---

## 74. Performance

Logging should minimize application disruption.

Guidelines:

- no normal disk I/O on UI thread;
- bounded serialization cost;
- bounded property count;
- bounded record size;
- asynchronous sink writes;
- batching where safe;
- severity-based filtering before expensive formatting;
- no arbitrary object reflection.

---

## 75. UI Thread Rule

Logging from UI code must be non-blocking.

The logging module must not marshal normal sink operations onto the UI thread.

---

## 76. Development Mode

Development mode may enable:

- DEBUG;
- TRACE;
- console output;
- richer stack traces;
- local log viewer;
- source file and line metadata.

It must not disable:

- redaction;
- secret blocking;
- payload limits;
- restricted routing;
- bounded buffers.

---

## 77. Production Mode

Production defaults should favor:

```text
INFO and above
rolling file
restricted security sink
bounded retention
safe stack traces
no console
no user content
no remote upload by default
```

---

## 78. Test Support

The module should provide:

```text
TestLogger
RecordingLogger
InMemorySink
FaultInjectingSink
ManualClock
DeterministicFormatter
RedactionTestHarness
```

Tests must preserve safety behavior.

A test logger must not accept secret values simply because it is used in tests.

---

## 79. Required Tests

### Record validation

- missing severity;
- missing category;
- invalid template;
- oversized property;
- mutable property;
- invalid correlation context.

### Redaction

- API key;
- password;
- token;
- authorization header;
- PEM private key;
- raw user text;
- URL query secret;
- nested object;
- unsafe exception.

### Buffering

- capacity;
- severity reserve;
- low-level drop;
- critical fallback;
- shutdown drain;
- drop count aggregation.

### Sink isolation

- one sink fails;
- restricted sink fails;
- file rotation fails;
- retention cleanup fails;
- flush timeout;
- fallback classification.

### File lifecycle

- roll on size;
- retention by age;
- maximum file count;
- compression failure;
- permission failure;
- startup recovery.

### Recursion

- sink failure while reporting sink failure;
- Event Bus ↔ Logging loop;
- formatter failure;
- emergency path.

### Diagnostics export

- second redaction pass;
- manifest;
- restricted record exclusion;
- no raw file copy;
- cancellation.

---

## 80. Core Invariants

1. Logs are diagnostic observations, not state.
2. Structured records are preferred over concatenated strings.
3. Secret material is never logged.
4. User content is denied by default.
5. Redaction precedes sink admission.
6. Debug mode never disables safety.
7. Buffers are bounded.
8. Low-severity records drop before critical records.
9. Critical capacity is reserved.
10. Sink failure is isolated.
11. Restricted records never fall back to unrestricted sinks.
12. Audit records are not sampled or suppressed.
13. Logs do not replace metrics or traces.
14. Logs do not replace Event Bus events.
15. Raw exceptions do not cross the logging boundary.
16. Log records are immutable after admission.
17. Shutdown flush is bounded.
18. Logging self-failure uses a non-recursive path.
19. Support bundles receive a second redaction pass.
20. Remote upload is disabled by default in the MVP.
21. File retention is bounded.
22. Composition Root owns sink wiring.
23. Logging remains available early in startup and late in shutdown.
24. Logging failure normally does not roll back business state.
25. Mandatory audit or security policy may fail closed.

---

## 81. Key Architectural Decisions

### Decision 1 — Structured logging

All modules should emit structured records.

### Decision 2 — Redaction-first

Safety inspection occurs before buffering and sink routing.

### Decision 3 — Asynchronous bounded pipeline

Normal producers do not write directly to disk.

### Decision 4 — Local-first MVP

The MVP stores logs locally and does not upload automatically.

### Decision 5 — Rolling file sink

A bounded rolling file sink is the primary production sink.

### Decision 6 — Restricted security sink

Security records may use a separate protected sink.

### Decision 7 — Audit remains logically distinct

Audit may share infrastructure but not semantics or policy.

### Decision 8 — Metrics and tracing remain separate

Telemetry owns those concerns.

### Decision 9 — No user content by default

Content is represented by safe metadata and artifact IDs.

### Decision 10 — Non-recursive failure reporting

Logging cannot rely on itself to report all failures.

### Decision 11 — Bounded shutdown

Flush and sink shutdown use deadlines.

### Decision 12 — Debug is not unsafe mode

Safety rules remain enabled in development.

---

## 82. Initial MVP Scope

The MVP should support:

```text
structured LogRecord
severity and category
message templates
safe structured properties
correlation context
async bounded buffer
severity-aware overflow
redaction
sensitive-type rejection
safe exception summaries
console/debug sink
rolling file sink
restricted security sink
in-memory diagnostic sink
file rotation
retention
flush
shutdown drain
sink health
safe diagnostics export
bootstrap logger
emergency logger
```

---

## 83. Deferred Capabilities

Deferred:

```text
remote log aggregation
cloud upload
OpenTelemetry log exporter
platform-native logging sinks
tamper-evident audit chain
organization-wide log policy
cross-device diagnostics
automatic crash upload
encrypted log archive
full-text indexed local log database
central support portal
```

---

## 84. Open Decisions

### Contract decisions

- exact `Logger` interface;
- `LogRecord` shape;
- property value types;
- exception summary contract;
- scope contract;
- sink contract;
- flush receipt;
- export request and manifest;
- audit adapter contract.

### State decisions

- Logging lifecycle;
- buffer lifecycle;
- sink lifecycle;
- file lifecycle;
- rotation lifecycle;
- export lifecycle;
- emergency-path lifecycle.

### Event decisions

- Logging started;
- sink degraded;
- sink recovered;
- records dropped;
- rotation completed;
- retention cleanup failed;
- redaction blocked;
- export completed;
- logging failed.

### Error decisions

- invalid record;
- unsafe property;
- buffer full;
- sink unavailable;
- write failed;
- rotation failed;
- flush timeout;
- retention cleanup failed;
- recursion blocked;
- export unsafe;
- audit unavailable.

### Policy decisions

- default minimum level;
- development and production defaults;
- queue capacity;
- critical reserve;
- file size;
- retained file count;
- retention age;
- flush interval;
- restricted log retention;
- support-bundle record count;
- path masking;
- safe stack-trace depth.

### Implementation decisions

- logging library;
- JSON Lines versus structured text default;
- queue primitive;
- file writer;
- atomic rotation method;
- compression;
- file locking;
- per-platform permissions;
- bootstrap-to-normal logger handoff.

---

## 85. Documentation Order

Recommended order:

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

- `LogRecord`;
- `Logger`;
- `LogScope`;
- `LogProperty`;
- `ExceptionSummary`;
- `LogPolicy`;
- `LogBuffer`;
- `LogSink`;
- `LogFormatter`;
- `RedactionResult`;
- `WriteResult`;
- `FlushRequest`;
- `FlushResult`;
- `RotationPolicy`;
- `RetentionPolicy`;
- `DiagnosticsExportRequest`;
- `DiagnosticsManifest`;
- `AuditRecord`;
- lifecycle controls.

---

## 86. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/configuration/MODULE.md
03-infrastructure/configuration/CONTRACT.md
03-infrastructure/configuration/EVENTS.md
03-infrastructure/configuration/ERRORS.md

03-infrastructure/secret-management/MODULE.md
03-infrastructure/secret-management/CONTRACT.md
03-infrastructure/secret-management/EVENTS.md
03-infrastructure/secret-management/ERRORS.md

03-infrastructure/event-bus/MODULE.md
03-infrastructure/event-bus/CONTRACT.md
03-infrastructure/event-bus/STATES.md
03-infrastructure/event-bus/EVENTS.md
03-infrastructure/event-bus/ERRORS.md
```

Future Logging documents:

```text
03-infrastructure/logging/CONTRACT.md
03-infrastructure/logging/STATES.md
03-infrastructure/logging/EVENTS.md
03-infrastructure/logging/ERRORS.md
03-infrastructure/logging/README.md
```

---

## 87. Summary

Logging is the CRAI infrastructure for safe, structured, bounded diagnostic records.

The normal flow is:

```text
Structured LogRecord
    ↓
Context enrichment
    ↓
Redaction and safety inspection
    ↓
Bounded buffer
    ↓
Sink routing
    ↓
Rolling file / console / restricted sink
```

The module deliberately separates:

```text
Log
Event
Metric
Trace
Audit
State
```

The MVP favors:

```text
local-first
structured
asynchronous
bounded
redaction-first
searchable
predictable
```

The architecture guarantees:

- no secret material;
- no user content by default;
- bounded memory and retention;
- sink isolation;
- restricted routing;
- non-recursive failure handling;
- bounded shutdown;
- safe diagnostics export;
- clear separation from Event Bus and Telemetry.

This document is the architectural source of truth for subsequent Logging contracts, states, events, errors, and implementation documentation.
