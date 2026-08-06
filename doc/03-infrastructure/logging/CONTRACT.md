# Logging Contract

> **Project:** CRAI  
> **Layer:** Infrastructure  
> **Module:** Logging  
> **Document:** Public and Internal Contracts  
> **Path:** `03-infrastructure/logging/CONTRACT.md`  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-08-06  
> **Source of Truth:**
>
> - `03-infrastructure/logging/MODULE.md`
> - `docs/architecture/MODULE_DEPENDENCY.md`
> - `docs/architecture/DATA_FLOW.md`
> - `docs/architecture/runtime/ERROR_MODEL.md`
> - `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md`
> - `03-infrastructure/configuration/MODULE.md`
> - `03-infrastructure/configuration/CONTRACT.md`
> - `03-infrastructure/secret-management/MODULE.md`
> - `03-infrastructure/secret-management/CONTRACT.md`
> - `03-infrastructure/secret-management/ERRORS.md`
> - `03-infrastructure/event-bus/MODULE.md`
> - `03-infrastructure/event-bus/CONTRACT.md`
> - `03-infrastructure/event-bus/ERRORS.md`

---

## 1. Purpose

This document defines the contracts exposed and consumed by the Logging infrastructure module.

It specifies:

- structured log records;
- logger ports;
- log scopes;
- message templates;
- structured properties;
- severity and category;
- correlation and causation context;
- privacy and security classification;
- record validation;
- redaction and sensitive-data inspection;
- exception normalization;
- asynchronous buffer admission;
- overflow and drop behavior;
- sink registration and routing;
- sink write results;
- formatting;
- rolling file behavior;
- retention;
- flush and shutdown;
- bootstrap and emergency logging;
- diagnostics queries;
- diagnostics export;
- security-log routing;
- audit adapter boundaries;
- configuration and runtime controls;
- testing contracts.

This document does not define:

- concrete logging-library APIs;
- concrete queue implementation;
- exact file paths;
- exact operating-system permissions;
- concrete compression library;
- metrics and tracing contracts;
- Event Bus payload contracts;
- detailed Logging states;
- Logging self-events;
- normalized Logging error catalog;
- UI wording;
- remote log aggregation protocols.

Detailed lifecycles belong in `STATES.md`.

Logging self-events belong in `EVENTS.md`.

Normalized failures belong in `ERRORS.md`.

---

## 2. Contract Goals

Logging contracts must:

1. provide a structured and consistent producer API;
2. prevent raw secret material from entering the pipeline;
3. deny user content by default;
4. preserve correlation and operation context;
5. remain asynchronous for normal writes;
6. keep buffers and files bounded;
7. isolate sink failures;
8. preserve privacy and security classification;
9. support safe exception diagnostics;
10. support development and production modes without weakening safety;
11. support rolling files and retention;
12. support bounded flush and shutdown;
13. support non-recursive emergency reporting;
14. keep audit semantics distinct;
15. support future remote and platform-native sinks;
16. remain independent from a specific logging framework;
17. remain compatible with Telemetry trace context;
18. avoid using logs as application state.

---

## 3. Contract Groups

The Logging module defines these contract groups.

### 3.1 Record contracts

```text
LogRecord
LogRecordDraft
LogMessage
LogProperty
LogPropertyBag
LogContext
LogIdentity
ExceptionSummary
```

### 3.2 Producer contracts

```text
Logger
LoggerFactory
LogScope
LogScopeFactory
LogWriteRequest
LogWriteReceipt
```

### 3.3 Policy and safety contracts

```text
LogPolicy
LogFilter
LogPrivacyPolicy
LogSecurityPolicy
LogRedactor
RedactionRequest
RedactionResult
SensitiveDataInspector
RecordSafetyResult
```

### 3.4 Buffer contracts

```text
LogBuffer
LogBufferConfiguration
LogAdmissionRequest
LogAdmissionResult
LogDropSummary
```

### 3.5 Sink and formatter contracts

```text
LogSink
LogSinkDescriptor
LogSinkRouter
LogWriteBatch
SinkWriteResult
LogFormatter
FormattedLogRecord
```

### 3.6 File lifecycle contracts

```text
RollingFilePolicy
RetentionPolicy
RotationRequest
RotationResult
RetentionCleanupRequest
RetentionCleanupResult
```

### 3.7 Lifecycle and diagnostics contracts

```text
LoggingControl
LoggingStatus
FlushRequest
FlushResult
LoggingDiagnosticsQuery
LoggingDiagnosticsResult
DiagnosticsExportRequest
DiagnosticsExportResult
DiagnosticsManifest
```

### 3.8 Audit contracts

```text
AuditRecord
AuditWriter
AuditWriteRequest
AuditWriteResult
AuditPolicy
```

---

# Part I — Core Identifiers

## 4. Core Identifiers

```text
LogRecordId
LoggerId
LogScopeId
LogSinkId
LogBufferId
LogBatchId
LoggingInstanceId
ApplicationInstanceId
ProcessInstanceId
CorrelationId
CausationId
OperationId
SessionId?
PipelineId?
TaskId?
WorkItemId?
AttemptId?
EntityId?
AuditRecordId?
DiagnosticsExportId?
```

Rules:

- identifiers are opaque;
- identifiers must not embed secret or user content;
- identifiers should be safe for local diagnostics unless classified otherwise;
- `LogRecordId` is unique per Logging instance;
- audit identity is separate from ordinary log identity.

---

# Part II — Log Record Contracts

## 5. LogRecordDraft

A producer creates a draft.

```text
LogRecordDraft {
    severity
    category

    message
    sourceModule
    sourceComponent?

    context?
    exception?
    properties?
    tags?

    privacyClassification?
    securityClassification?
}
```

The draft is mutable only before submission.

After admission, the resulting `LogRecord` is immutable.

---

## 6. LogRecord

```text
LogRecord {
    recordId

    occurredAt
    receivedAt
    sequenceNumber?

    severity
    category

    messageTemplate
    renderedMessage?

    sourceModule
    sourceComponent?

    loggingInstanceId
    applicationInstanceId
    processInstanceId?

    context

    normalizedErrorCode?
    exceptionSummary?

    privacyClassification
    securityClassification

    properties
    tags

    safetySummary
}
```

---

## 7. Required Fields

Every accepted record requires:

```text
recordId
occurredAt
severity
category
messageTemplate
sourceModule
loggingInstanceId
applicationInstanceId
privacyClassification
securityClassification
properties
```

---

## 8. Log Severity

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

Severity is ordered.

A sink or policy may declare a minimum accepted severity.

Severity must not be inferred from message text.

---

## 9. Log Category

Canonical categories:

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

Modules may define registered subcategories.

Unregistered free-form categories are discouraged.

---

## 10. LogMessage

```text
LogMessage {
    template
    templateId?
    rendered?
}
```

Rules:

- `template` must be stable;
- variable values belong in properties;
- message templates must not contain secret values;
- templates should avoid full user-controlled text;
- `rendered` is produced only after safety processing;
- sinks may format from template and properties without storing a pre-rendered message.

---

## 11. Message Template Examples

Preferred:

```text
"Provider request failed"
```

Properties:

```text
providerId
operationId
normalizedErrorCode
```

Not preferred:

```text
"Provider request failed for " + providerId + ": " + rawResponse
```

---

## 12. LogContext

```text
LogContext {
    correlationId?
    causationId?

    operationId?
    sessionId?
    pipelineId?
    taskId?
    workItemId?
    attemptId?
    entityId?

    traceId?
    spanId?

    contentRevision?
    configurationRevision?
}
```

Rules:

- context values are copied into an immutable context;
- missing optional context is allowed;
- invalid context is rejected or removed according to policy;
- trace IDs are imported from Telemetry;
- Logging does not create trace spans.

---

## 13. Context Precedence

When the same context key appears in multiple sources:

```text
Explicit record context
    ↓
Active LogScope
    ↓
Ambient execution context
    ↓
Application defaults
```

Explicit safe values take precedence.

Unsafe values are never preserved merely because they have higher precedence.

---

## 14. Sequence Number

A process-local monotonic sequence may be included:

```text
sequenceNumber
```

It supports local ordering when wall-clock time changes.

It does not provide distributed ordering.

---

# Part III — Structured Property Contracts

## 15. LogProperty

```text
LogProperty {
    name
    value
    valueType
    privacyClassification?
    displayPolicy?
}
```

Supported value types should be bounded:

```text
STRING
BOOLEAN
INTEGER
DECIMAL
DURATION
TIMESTAMP
ENUM
IDENTIFIER
SAFE_URI
SAFE_PATH
STRING_LIST
IDENTIFIER_LIST
OBJECT_REFERENCE
```

Arbitrary object graphs are prohibited.

---

## 16. LogPropertyBag

```text
LogPropertyBag {
    properties[]
}
```

Rules:

- property count is bounded;
- names are normalized;
- duplicate names are resolved deterministically;
- nested depth is bounded;
- values are immutable after admission;
- unknown complex types are rejected rather than reflected recursively.

---

## 17. Reserved Property Names

Reserved names include:

```text
recordId
occurredAt
severity
category
sourceModule
sourceComponent
correlationId
causationId
applicationInstanceId
processInstanceId
privacyClassification
securityClassification
normalizedErrorCode
```

Producer properties must not override reserved fields.

---

## 18. Allowed Structured Properties

Examples:

```text
providerId
modelId
operationId
state
previousState
currentState
revision
retryCount
duration
durationClass
queueDepth
backendType
eventType
subscriberId
sinkId
fileIndex
dropCount
```

---

## 19. Prohibited Properties

Examples:

```text
apiKey
password
accessToken
refreshToken
clientSecret
privateKey
authorizationHeader
cookie
rawPrompt
rawSourceText
rawTranslatedText
rawOcrText
rawImage
rawDocument
rawProviderResponse
rawEnvironment
clipboardContent
```

A property name matching a prohibited class is blocked even when the producer claims it is safe.

---

## 20. Property Size Limits

Policy defines:

```text
maximumPropertyCount
maximumStringLength
maximumListLength
maximumNestedDepth
maximumRecordSize
```

Oversized values may be:

```text
REJECTED
TRUNCATED_WITH_MARKER
SUMMARIZED
REPLACED_BY_REFERENCE
```

Secret-like values are never preserved through truncation.

---

## 21. Safe Truncation

When truncation is allowed:

```text
originalLength
retainedLength
truncated = true
```

The truncated value must still pass safety inspection.

---

# Part IV — Privacy and Security Contracts

## 22. Privacy Classification

```text
PUBLIC
INTERNAL
CONFIDENTIAL_METADATA
RESTRICTED_SECURITY
USER_CONTENT
SECRET
```

Rules:

- `SECRET` is never admitted;
- `USER_CONTENT` is denied by default;
- `RESTRICTED_SECURITY` requires restricted sinks;
- `CONFIDENTIAL_METADATA` may require masking;
- a record's effective classification is the strongest classification of any accepted field.

---

## 23. Security Classification

```text
INTERNAL
CONFIDENTIAL
RESTRICTED_SECURITY
AUDIT_RESTRICTED
```

Security classification affects:

- sink routing;
- file permissions;
- diagnostics visibility;
- retention;
- export eligibility;
- fallback eligibility.

---

## 24. Classification Escalation

A producer may request a stronger classification.

A producer may not downgrade a field's detected classification.

Example:

```text
Detected RESTRICTED_SECURITY
Requested INTERNAL
    → effective RESTRICTED_SECURITY
```

---

## 25. RecordSafetyResult

```text
RecordSafetyResult {
    safe
    blocked
    effectivePrivacyClassification
    effectiveSecurityClassification

    findings[]
    redactedPropertyNames[]
    removedPropertyNames[]
    transformations[]

    safeRecordDraft?
}
```

---

## 26. Safety Findings

Possible finding classes:

```text
KNOWN_SECRET_VALUE
SECRET_BEARING_TYPE
AUTHORIZATION_HEADER
PASSWORD_FIELD
TOKEN_FIELD
PRIVATE_KEY_BLOCK
SENSITIVE_QUERY_PARAMETER
COOKIE_FIELD
RAW_USER_CONTENT
RAW_PROVIDER_PAYLOAD
RAW_ENVIRONMENT_VALUE
UNSAFE_PATH
UNSAFE_URI
UNSAFE_EXCEPTION
OVERSIZED_VALUE
UNSUPPORTED_VALUE_TYPE
HIGH_ENTROPY_SUSPECT
POLICY_VIOLATION
```

---

## 27. SensitiveDataInspector

```text
SensitiveDataInspector {
    inspectRecord(draft, context)
        -> RecordSafetyResult

    inspectProperty(property, context)
        -> PropertySafetyResult

    inspectException(exception, context)
        -> ExceptionSafetyResult

    inspectExport(bundleDraft, context)
        -> ExportSafetyResult
}
```

Inspection must be:

- bounded;
- non-recursive beyond configured depth;
- deterministic for the same policy snapshot;
- safe under malformed input;
- unable to expose matched secret text in its findings.

---

## 28. LogRedactor

```text
LogRedactor {
    redact(request)
        -> RedactionResult
}
```

---

## 29. RedactionRequest

```text
RedactionRequest {
    recordDraft
    policyRevision
    environment
    targetSinkClass?
    exportMode?
}
```

---

## 30. RedactionResult

```text
RedactionResult {
    outcome
    safeRecordDraft?
    findings[]
    blockedReason?
    policyRevision
}
```

Possible outcomes:

```text
UNCHANGED_SAFE
REDACTED
REMOVED_FIELDS
BLOCKED
FAILED_SAFE
```

`FAILED_SAFE` means the record was blocked because safety could not be proven.

---

## 31. Redaction Transformations

Allowed transformations:

```text
MASK
REMOVE
HASH_REFERENCE
CLASSIFY_ONLY
TRUNCATE_SAFE
REPLACE_WITH_CONSTANT
REPLACE_WITH_OBJECT_REFERENCE
```

Examples:

```text
user@example.com
    → u***@example.com

/home/alice/Documents/book.txt
    → <user-data>/book.txt

https://provider/api?token=abc
    → https://provider/api?token=<redacted>
```

---

## 32. Hashing Rule

Hashing is not automatically safe.

A hash may be retained only when:

- it cannot be used to recover the source;
- it does not expose a low-entropy secret;
- policy explicitly allows it;
- salt/key handling is defined;
- it is used as a reference, not as secret validation material.

---

## 33. User Content Admission

User content may be admitted only when all are true:

- explicit diagnostic mode allows it;
- the specific field contract permits it;
- size is bounded;
- content is redacted;
- user or policy consent exists where required;
- target sink permits it;
- export policy permits it.

Default result:

```text
blocked or replaced by metadata
```

---

# Part V — Exception Contracts

## 34. ExceptionInput

```text
ExceptionInput {
    exceptionObject
    normalizedErrorCode?
    operationStage?
}
```

Raw exception objects remain inside the Logging boundary.

---

## 35. ExceptionSummary

```text
ExceptionSummary {
    exceptionType
    normalizedErrorCode?
    safeMessage
    stackFrames[]
    innerCauseCodes[]
    retryClass?
    operationStage?
}
```

---

## 36. StackFrameSummary

```text
StackFrameSummary {
    module
    type
    method
    fileReference?
    line?
    frameClass
}
```

Rules:

- local variables are excluded;
- method arguments are excluded;
- full source paths are masked by policy;
- framework frames may be collapsed;
- maximum frame count is bounded.

---

## 37. Exception Normalizer

```text
ExceptionNormalizer {
    normalize(exceptionInput, context)
        -> ExceptionNormalizationResult
}
```

Possible outcomes:

```text
NORMALIZED
NORMALIZED_WITH_REDACTION
BLOCKED
FAILED_SAFE
```

---

## 38. Exception Cause Chain

Cause-chain depth is bounded.

Raw nested exception messages are not copied automatically.

Only normalized safe codes and types may be retained.

---

# Part VI — Producer Contracts

## 39. Logger

```text
Logger {
    isEnabled(severity, category)

    write(request)

    trace(message, properties?)
    debug(message, properties?)
    info(message, properties?)
    notice(message, properties?)
    warning(message, properties?)
    error(message, properties?, exception?)
    critical(message, properties?, exception?)
    fatal(message, properties?, exception?)
}
```

Convenience methods build a `LogWriteRequest`.

The canonical contract remains `write`.

---

## 40. LogWriteRequest

```text
LogWriteRequest {
    recordDraft
    writeOptions?
}
```

---

## 41. LogWriteOptions

```text
LogWriteOptions {
    admissionMode
    admissionTimeout?
    requirePersistence?
    preferredSinkClass?
    allowSampling
    allowSuppression
    emergencyEligible
}
```

Possible admission modes:

```text
FIRE_AND_OBSERVE
BUFFER_CONFIRMED
PERSISTENCE_CONFIRMED
```

Default:

```text
FIRE_AND_OBSERVE
```

for low-severity records.

Recommended:

```text
BUFFER_CONFIRMED
```

for `ERROR` and above where practical.

`PERSISTENCE_CONFIRMED` is reserved for audit or explicit administrative diagnostics.

---

## 42. LogWriteReceipt

```text
LogWriteReceipt {
    recordId?
    outcome
    admittedAt?
    effectiveSeverity?
    effectiveClassification?
    targetSinkClasses[]
    warningCodes[]
    rejectionCode?
}
```

Possible outcomes:

```text
ACCEPTED
FILTERED
SAMPLED_OUT
SUPPRESSED
REDACTED_AND_ACCEPTED
REJECTED_UNSAFE
REJECTED_CAPACITY
TIMED_OUT
LOGGING_NOT_RUNNING
EMERGENCY_WRITTEN
FAILED_SAFE
```

---

## 43. Producer Success Semantics

`ACCEPTED` means the safe record entered the Logging pipeline.

It does not guarantee:

- every sink persisted it;
- file flush completed;
- remote upload occurred;
- retention preserved it indefinitely.

---

## 44. LoggerFactory

```text
LoggerFactory {
    createLogger(sourceModule, sourceComponent?)
        -> Logger
}
```

Logger identity is stable for the component.

Modules should not construct sink-specific loggers.

---

# Part VII — Scope Contracts

## 45. LogScopeFactory

```text
LogScopeFactory {
    beginScope(scopeDraft)
        -> LogScope
}
```

---

## 46. LogScopeDraft

```text
LogScopeDraft {
    context
    properties
    privacyClassification?
    securityClassification?
}
```

---

## 47. LogScope

```text
LogScope {
    scopeId
    parentScopeId?
    effectiveContext
    effectiveProperties

    dispose()
}
```

Rules:

- scopes are immutable after creation;
- scopes are nested;
- disposal restores parent context;
- scope context must not leak across unrelated async work;
- repeated disposal is idempotent;
- scope properties still pass record-level safety checks.

---

## 48. Scope Merge

Merge order:

```text
Outer scope
    ↓
Inner scope
    ↓
Explicit record
```

Explicit record values win when safe.

Classification always uses the strongest effective value.

---

## 49. Scope Prohibitions

Scopes must not contain:

- secret values;
- raw user content;
- raw provider payloads;
- mutable service objects;
- large collections;
- file streams;
- UI controls.

---

# Part VIII — Policy Contracts

## 50. LogPolicy

```text
LogPolicy {
    revision

    defaultMinimumSeverity
    categoryOverrides
    moduleOverrides
    environmentOverrides

    privacyPolicy
    securityPolicy
    samplingPolicy
    suppressionPolicy
    sizePolicy
    exceptionPolicy

    sinkRoutingRules
}
```

---

## 51. LogFilter

```text
LogFilter {
    evaluate(recordDraft, policy)
        -> LogFilterResult
}
```

---

## 52. LogFilterResult

```text
LogFilterResult {
    enabled
    effectiveMinimumSeverity
    reasonCode?
}
```

Filtering should occur before expensive formatting.

Safety inspection may still be required for records sent through bootstrap or emergency paths.

---

## 53. SamplingPolicy

```text
SamplingPolicy {
    enabled
    rules[]
}
```

A rule may match:

```text
severity
category
sourceModule
messageTemplateId
normalizedErrorCode
```

Sampling must not match arbitrary user-controlled values.

---

## 54. SuppressionPolicy

```text
SuppressionPolicy {
    enabled
    timeWindow
    maximumIdenticalRecords
    aggregationMode
    excludedSeverities[]
    excludedCategories[]
}
```

Suppression key should use safe stable fields:

```text
sourceModule
category
severity
messageTemplateId
normalizedErrorCode
```

---

## 55. SizePolicy

```text
LogSizePolicy {
    maximumRecordBytes
    maximumPropertyCount
    maximumStringLength
    maximumListLength
    maximumStackFrames
    maximumCauseDepth
}
```

---

# Part IX — Buffer Contracts

## 56. LogBuffer

```text
LogBuffer {
    admit(request)
        -> LogAdmissionResult

    readBatch(request)
        -> LogReadBatch

    status()
    drain(request)
    clear(policy)
}
```

---

## 57. LogBufferConfiguration

```text
LogBufferConfiguration {
    bufferId
    bufferClass

    capacityRecords
    capacityBytes?

    criticalReserveRecords
    admissionTimeout

    overflowPolicyBySeverity
    batchSize
    maximumRecordAge?
}
```

Buffer classes:

```text
NORMAL
SECURITY
AUDIT
EMERGENCY
```

---

## 58. LogAdmissionRequest

```text
LogAdmissionRequest {
    safeRecord
    estimatedSize
    deadline?
}
```

---

## 59. LogAdmissionResult

```text
LogAdmissionResult {
    outcome
    bufferId
    admittedAt?
    queueDepthAfter?
    droppedRecordCount?
    suppressionReference?
    reasonCode?
}
```

Possible outcomes:

```text
ADMITTED
FILTERED
SAMPLED_OUT
SUPPRESSED
DROPPED_LOW_SEVERITY
REJECTED_CAPACITY
TIMED_OUT
BUFFER_NOT_RUNNING
```

---

## 60. Overflow Policies

```text
DROP_NEW
DROP_OLDEST_LOW_SEVERITY
SAMPLE_LOW_SEVERITY
SUPPRESS_DUPLICATE
BLOCK_PRODUCER_BOUNDED
USE_CRITICAL_RESERVE
USE_EMERGENCY_PATH
REJECT
```

---

## 61. Severity-Aware Admission

Recommended precedence:

```text
FATAL
CRITICAL
ERROR
WARNING
NOTICE
INFO
DEBUG
TRACE
```

A lower-severity record must not evict a higher-severity record.

---

## 62. Critical Reserve

Critical reserve may accept:

```text
ERROR
CRITICAL
FATAL
mandatory SECURITY
mandatory AUDIT
```

Policy decides whether `ERROR` is eligible.

Critical reserve must be bounded.

---

## 63. LogDropSummary

```text
LogDropSummary {
    bufferId
    timeWindow
    droppedBySeverity
    droppedByCategory
    sampledCount
    suppressedCount
    capacityRejectionCount
}
```

The summary must avoid recursively generating one log per drop.

---

# Part X — Sink Contracts

## 64. LogSink

```text
LogSink {
    descriptor()

    initialize(request)
    write(batch, cancellationToken)
    flush(request, cancellationToken)
    rotate(request, cancellationToken)?
    health()
    shutdown(request, cancellationToken)
}
```

---

## 65. LogSinkDescriptor

```text
LogSinkDescriptor {
    sinkId
    sinkType

    acceptedSeverities[]
    acceptedCategories[]
    acceptedPrivacyClassifications[]
    acceptedSecurityClassifications[]

    supportsBatching
    supportsFlush
    supportsRotation
    supportsCompression
    supportsPersistenceConfirmation

    maximumRecordSize?
    fallbackGroup?
}
```

---

## 66. Sink Types

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
EMERGENCY
```

---

## 67. Sink Routing

```text
LogSinkRouter {
    route(safeRecord, routingContext)
        -> SinkRoutingResult
}
```

---

## 68. SinkRoutingResult

```text
SinkRoutingResult {
    selectedSinkIds[]
    excludedSinkIds[]
    fallbackCandidates[]
    mandatorySinkIds[]
    warnings[]
}
```

---

## 69. Routing Rules

Routing considers:

- severity;
- category;
- privacy classification;
- security classification;
- environment;
- sink health;
- diagnostic mode;
- audit requirement;
- persistence requirement.

A restricted record cannot route to an unrestricted sink.

---

## 70. LogWriteBatch

```text
LogWriteBatch {
    batchId
    sinkId
    records[]
    createdAt
    totalEstimatedBytes
}
```

Records remain immutable.

---

## 71. SinkWriteResult

```text
SinkWriteResult {
    sinkId
    batchId
    outcome

    acceptedCount
    writtenCount
    failedCount

    persistenceConfirmed
    flushRequired

    normalizedErrorCode?
    retryable
    warnings[]
}
```

Possible outcomes:

```text
WRITTEN
PARTIALLY_WRITTEN
REJECTED
TIMED_OUT
SINK_UNAVAILABLE
FAILED
```

---

## 72. Sink Failure Isolation

When one sink fails:

- other sinks continue;
- mandatory sink failure is surfaced;
- fallback is evaluated;
- classification cannot be weakened;
- the original record is not mutated;
- raw sink exceptions remain internal.

---

## 73. Fallback Eligibility

```text
FallbackDecision {
    eligible
    fallbackSinkId?
    reasonCode?
}
```

Fallback requires the target sink to accept:

- severity;
- category;
- privacy classification;
- security classification;
- persistence requirement.

---

## 74. Restricted Fallback Rule

```text
RESTRICTED_SECURITY
AUDIT_RESTRICTED
```

must never fall back to:

```text
CONSOLE
DEBUG_OUTPUT
ordinary ROLLING_FILE
unrestricted REMOTE
```

unless the sink is explicitly configured and authorized for that classification.

---

# Part XI — Formatter Contracts

## 75. LogFormatter

```text
LogFormatter {
    format(record, formatContext)
        -> FormatResult
}
```

---

## 76. FormatContext

```text
FormatContext {
    sinkId
    format
    includeRenderedMessage
    includeStackTrace
    pathDisplayPolicy
    timestampFormat
}
```

---

## 77. FormatResult

```text
FormatResult {
    outcome
    formattedRecord?
    estimatedBytes
    warnings[]
}
```

Possible outcomes:

```text
FORMATTED
FORMATTED_WITH_TRUNCATION
REJECTED_UNSAFE
FAILED_SAFE
```

---

## 78. FormattedLogRecord

```text
FormattedLogRecord {
    bytesOrText
    format
    recordId
}
```

It may exist only inside the sink pipeline.

It must not be added back into structured properties.

---

## 79. Supported Formats

MVP:

```text
JSON_LINES
STRUCTURED_TEXT
```

Future:

```text
PLATFORM_NATIVE
OTLP_LOGS
SYSLOG
```

---

## 80. JSON Lines Contract

Each line represents one record.

Required stable fields:

```text
timestamp
severity
category
messageTemplate
sourceModule
recordId
applicationInstanceId
privacyClassification
securityClassification
```

Optional fields remain structured.

---

# Part XII — Rolling File Contracts

## 81. RollingFilePolicy

```text
RollingFilePolicy {
    rollOnStartup
    rollOnSize
    maximumFileSize
    rollOnDate?
    dateBoundary?
    maximumOpenDuration?

    fileNamePattern
    activeFileName

    compressionPolicy?
    retentionPolicy
}
```

---

## 82. RotationRequest

```text
RotationRequest {
    sinkId
    reason
    requestedAt
    force
}
```

Reasons:

```text
SIZE_LIMIT
DATE_BOUNDARY
STARTUP
MANUAL
CONFIGURATION_CHANGE
SHUTDOWN
FILE_CORRUPTION_RECOVERY
```

---

## 83. RotationResult

```text
RotationResult {
    sinkId
    outcome
    previousFileReference?
    newFileReference?
    recordsFlushed
    compressionScheduled
    retentionCleanupScheduled
    completedAt
    warnings[]
}
```

Possible outcomes:

```text
ROTATED
NOT_REQUIRED
PARTIALLY_ROTATED
TIMED_OUT
FAILED
```

---

## 84. File Reference

```text
LogFileReference {
    logicalFileId
    sinkId
    fileClass
    createdAt
    sequence
}
```

Raw absolute paths should not be exposed broadly.

Platform adapters may resolve the physical path.

---

## 85. RetentionPolicy

```text
RetentionPolicy {
    maximumAge
    maximumFileCount
    maximumTotalBytes?
    minimumFilesToKeep?
    preserveCurrentFile
    classificationOverrides?
}
```

---

## 86. RetentionCleanupRequest

```text
RetentionCleanupRequest {
    sinkId
    policyRevision
    triggeredBy
    requestedAt
}
```

---

## 87. RetentionCleanupResult

```text
RetentionCleanupResult {
    sinkId
    outcome
    examinedFiles
    deletedFiles
    retainedFiles
    bytesFreed?
    warnings[]
}
```

Possible outcomes:

```text
COMPLETED
PARTIALLY_COMPLETED
NOT_REQUIRED
TIMED_OUT
FAILED
```

---

## 88. CompressionPolicy

```text
CompressionPolicy {
    enabled
    minimumFileAge
    format
    maximumConcurrentJobs
}
```

Compression must not block active log writing.

---

# Part XIII — Flush Contracts

## 89. FlushRequest

```text
FlushRequest {
    sinkIds?
    minimumSeverity?
    includeAudit
    includeSecurity
    deadline
    reason
}
```

Reasons:

```text
INTERVAL
MANUAL
ERROR
CRITICAL
ROTATION
DIAGNOSTICS_EXPORT
SHUTDOWN
```

---

## 90. FlushResult

```text
FlushResult {
    outcome
    startedAt
    completedAt

    buffersDrained
    recordsAttempted
    recordsWritten
    recordsDropped
    sinksSucceeded[]
    sinksFailed[]

    warnings[]
}
```

Possible outcomes:

```text
FLUSHED
PARTIALLY_FLUSHED
TIMED_OUT
FAILED
CANCELED
```

---

## 91. Persistence Confirmation

A sink may support:

```text
persistenceConfirmed = true
```

This means the sink has confirmed its own persistence boundary.

It does not guarantee storage durability beyond the operating system's actual guarantees.

---

# Part XIV — Bootstrap and Emergency Contracts

## 92. BootstrapLogger

```text
BootstrapLogger {
    writeMinimal(record)
    handoff(normalLogger)
    shutdown()
}
```

Bootstrap records support only a minimal safe schema.

---

## 93. BootstrapLogRecord

```text
BootstrapLogRecord {
    occurredAt
    severity
    sourceComponent
    messageTemplate
    normalizedErrorCode?
    safeProperties
}
```

No exception object graphs or user content are accepted.

---

## 94. EmergencyLogger

```text
EmergencyLogger {
    tryWrite(record, deadline)
        -> EmergencyWriteResult
}
```

---

## 95. EmergencyWriteResult

```text
EmergencyWriteResult {
    outcome
    sinkClass?
    writtenAt?
}
```

Possible outcomes:

```text
WRITTEN
BLOCKED_UNSAFE
TIMED_OUT
FAILED_SILENTLY
```

Emergency logging must never throw into caller code.

---

## 96. Emergency Path Rules

Emergency path:

- is synchronous and bounded;
- uses a minimal formatter;
- writes locally;
- has recursion protection;
- accepts only critical safe metadata;
- does not accept raw exceptions;
- does not use Event Bus;
- does not use normal asynchronous buffers.

---

# Part XV — Diagnostics Contracts

## 97. LoggingDiagnosticsQuery

```text
LoggingDiagnosticsQuery {
    minimumSeverity?
    categories?
    sourceModules?
    normalizedErrorCodes?
    timeRange?
    maximumRecords
    includeRestricted
    callerClearance
}
```

---

## 98. LoggingDiagnosticsResult

```text
LoggingDiagnosticsResult {
    loggingStatus
    sinkStatuses[]
    bufferStatuses[]
    recentRecords[]
    dropSummary
    warnings[]
}
```

Restricted records depend on caller clearance.

---

## 99. Safe Diagnostic Record

```text
SafeDiagnosticRecord {
    recordId
    occurredAt
    severity
    category
    messageTemplate
    safeRenderedMessage?
    sourceModule
    sourceComponent?
    contextSummary
    normalizedErrorCode?
    safeProperties
}
```

---

## 100. LoggingStatus

```text
LoggingStatus {
    lifecycleState
    healthState

    activePolicyRevision
    acceptingRecords
    activeSinkCount
    degradedSinkCount
    unavailableSinkCount

    normalBufferUtilization
    securityBufferUtilization
    auditBufferUtilization?

    recentDropSummary
    lastFlushAt?
    lastRotationAt?
}
```

---

# Part XVI — Diagnostics Export Contracts

## 101. DiagnosticsExportRequest

```text
DiagnosticsExportRequest {
    exportId?
    requestedBy

    timeRange
    minimumSeverity?
    categories?
    sourceModules?

    includeApplicationSummary
    includeConfigurationSummary
    includeModuleHealth
    includeRecentLogs
    includeRestrictedSecurity
    includeAudit

    maximumRecords
    maximumBundleSize

    destination
    callerClearance
    userConsentReference?
}
```

---

## 102. Export Destination

```text
DiagnosticsExportDestination {
    destinationType
    destinationReference
}
```

Possible types:

```text
LOCAL_FILE
USER_SELECTED_FILE
TEMPORARY_SUPPORT_BUNDLE
```

Remote upload is deferred.

---

## 103. DiagnosticsExportResult

```text
DiagnosticsExportResult {
    exportId
    outcome

    bundleReference?
    manifest
    recordsIncluded
    recordsExcluded
    bytesWritten?

    redactionSummary
    warnings[]
}
```

Possible outcomes:

```text
EXPORTED
PARTIALLY_EXPORTED
BLOCKED_UNSAFE
TIMED_OUT
FAILED
CANCELED
```

---

## 104. DiagnosticsManifest

```text
DiagnosticsManifest {
    exportId
    createdAt
    applicationVersion
    platformClass

    sections[]
    policyRevision
    redactionPolicyRevision

    includesRestrictedData
    includesAuditData
    userConsentReference?

    checksums?
}
```

---

## 105. Export Redaction Summary

```text
ExportRedactionSummary {
    examinedRecords
    redactedRecords
    excludedRecords
    blockedFindingsByClass
}
```

Matched values are never included.

---

## 106. Export Safety Rule

The export pipeline must inspect the complete bundle after assembly.

```text
Select safe records
    ↓
Apply export redaction
    ↓
Assemble bundle
    ↓
Inspect full bundle
    ↓
Write destination
```

Copying raw log files directly is prohibited by default.

---

# Part XVII — Audit Contracts

## 107. AuditRecord

```text
AuditRecord {
    auditRecordId
    occurredAt

    actor
    action
    target
    outcome
    reasonCode?

    correlationId?
    operationId?

    sourceModule
    applicationInstanceId

    securityClassification = AUDIT_RESTRICTED
    properties
}
```

---

## 108. Audit Actor

```text
AuditActor {
    actorType
    actorId
    displayHint?
}
```

Possible actor types:

```text
USER
APPLICATION
MODULE
ADMINISTRATOR
SYSTEM
EXTERNAL_SERVICE
```

---

## 109. Audit Action

```text
AuditAction {
    actionType
    actionVersion
}
```

Examples:

```text
SECRET_REGISTERED
SECRET_ROTATED
SECRET_REMOVED
CONFIGURATION_CHANGED
DIAGNOSTICS_EXPORTED
PROVIDER_CREDENTIAL_CHANGED
```

---

## 110. Audit Target

```text
AuditTarget {
    targetType
    targetId
    revision?
}
```

Audit targets must not include secret material or raw user content.

---

## 111. Audit Outcome

```text
SUCCEEDED
FAILED
REJECTED
PARTIALLY_COMPLETED
CANCELED
UNCERTAIN
```

---

## 112. AuditWriter

```text
AuditWriter {
    write(request, cancellationToken)
        -> AuditWriteResult
}
```

---

## 113. AuditWriteRequest

```text
AuditWriteRequest {
    record
    requirePersistenceConfirmation
    deadline
}
```

---

## 114. AuditWriteResult

```text
AuditWriteResult {
    auditRecordId
    outcome
    persistenceConfirmed
    writtenAt?
    normalizedErrorCode?
}
```

Possible outcomes:

```text
WRITTEN
REJECTED_UNSAFE
TIMED_OUT
AUDIT_SINK_UNAVAILABLE
FAILED
```

---

## 115. Audit Policy

```text
AuditPolicy {
    mandatoryActions[]
    retentionPolicy
    sinkRequirements
    failureMode
}
```

Failure modes:

```text
FAIL_ACTION_CLOSED
ALLOW_ACTION_WITH_CRITICAL_WARNING
USE_APPROVED_EMERGENCY_AUDIT
```

The owning module selects whether an action is mandatory-audited.

---

# Part XVIII — Lifecycle Contracts

## 116. LoggingControl

```text
LoggingControl {
    initialize(request)
    start()
    updatePolicy(request)
    flush(request)
    rotate(request)
    quiesce(request)
    shutdown(request)
    status()
}
```

---

## 117. LoggingInitializeRequest

```text
LoggingInitializeRequest {
    loggingInstanceId
    applicationInstanceId

    policy
    bufferConfigurations[]
    sinkDescriptors[]
    formatterDescriptors[]
    redactionPolicy

    bootstrapHandoff?
}
```

---

## 118. LoggingStartResult

```text
LoggingStartResult {
    outcome
    startedAt?
    activeSinkIds[]
    degradedSinkIds[]
    unavailableSinkIds[]
    warnings[]
}
```

Possible outcomes:

```text
RUNNING
RUNNING_DEGRADED
FAILED
```

---

## 119. Policy Update Request

```text
LogPolicyUpdateRequest {
    expectedPolicyRevision
    newPolicy
    applyMode
}
```

Apply modes:

```text
LIVE
ON_NEXT_ROTATION
RESTART_REQUIRED
```

---

## 120. Quiesce Request

```text
LoggingQuiesceRequest {
    minimumAcceptedSeverity
    allowCategories[]
    rejectCategories[]
    effectiveAt
    reasonCode
}
```

Typical shutdown behavior:

```text
accept:
    WARNING
    ERROR
    CRITICAL
    FATAL
    SECURITY
    AUDIT

reject or sample:
    TRACE
    DEBUG
    INFO
    PERFORMANCE
```

---

## 121. LoggingShutdownRequest

```text
LoggingShutdownRequest {
    deadline
    flush
    includeAudit
    includeSecurity
    forceAfterDeadline
    reasonCode
}
```

---

## 122. LoggingShutdownResult

```text
LoggingShutdownResult {
    outcome
    flushResult?
    sinksTerminated[]
    sinksAbandoned[]
    recordsLostBySeverity
    completedAt
}
```

Possible outcomes:

```text
TERMINATED
PARTIALLY_TERMINATED
TIMED_OUT
FAILED
```

---

# Part XIX — Configuration Contracts

## 123. LoggingConfiguration

```text
LoggingConfiguration {
    enabled

    defaultMinimumSeverity
    categoryOverrides
    moduleOverrides

    normalBuffer
    securityBuffer
    auditBuffer?

    sinks[]
    formatters[]

    redactionPolicy
    exceptionPolicy
    samplingPolicy
    suppressionPolicy

    diagnosticsPolicy
    exportPolicy

    developmentMode
}
```

---

## 124. Sink Configuration

```text
LogSinkConfiguration {
    sinkId
    sinkType
    enabled

    minimumSeverity
    categories?
    acceptedClassifications[]

    formatterId
    filePolicy?
    retentionPolicy?
    flushPolicy?

    remoteCredentialReference?
}
```

Remote credentials use `SecretReference`.

Raw credentials are prohibited.

---

## 125. Live Configuration Changes

Potentially live:

- minimum severity;
- category overrides;
- sampling;
- suppression;
- flush interval;
- some retention thresholds;
- diagnostics buffer size within safe bounds.

---

## 126. Restart-Required Changes

Typically restart-required:

- sink type;
- base file directory;
- restricted sink implementation;
- audit sink implementation;
- serialization format;
- encryption mode;
- remote exporter technology.

---

# Part XX — Testing Contracts

## 127. TestLogger

```text
TestLogger {
    write(request)
    records()
    clear()
    assertNoUnsafeRecords()
}
```

---

## 128. RecordingSink

```text
RecordingSink {
    records()
    batches()
    flushes()
    rotations()
}
```

---

## 129. FaultInjectingSink

Supported injected failures:

```text
INITIALIZATION_FAILURE
WRITE_FAILURE
PARTIAL_WRITE
FLUSH_TIMEOUT
ROTATION_FAILURE
RETENTION_FAILURE
PERMISSION_DENIED
DISK_FULL
SHUTDOWN_TIMEOUT
```

---

## 130. RedactionTestHarness

```text
RedactionTestHarness {
    inspect(input)
    assertBlocked()
    assertRedacted()
    assertNoSecretMaterial()
    assertNoUserContent()
}
```

---

## 131. Deterministic Clock and Sequence

Tests should support:

```text
ManualClock
DeterministicSequenceProvider
```

to verify ordering, rotation, retention, and suppression windows.

---

# Part XXI — Validation Rules

## 132. Record Validation

Reject or block when:

- severity missing;
- category invalid;
- message template missing;
- source module missing;
- property name invalid;
- property count exceeded;
- unsupported property type;
- record size exceeded;
- privacy classification invalid;
- security classification invalid;
- exception unsafe and cannot be normalized.

---

## 133. Safety Validation

Block when:

- secret-bearing type detected;
- known secret value detected;
- authorization header detected;
- password field detected;
- private key detected;
- raw user content prohibited;
- unsafe provider payload detected;
- redaction failed;
- classification downgrade attempted.

---

## 134. Scope Validation

Reject scope creation when:

- context contains prohibited data;
- property count exceeds policy;
- nested scope depth exceeds policy;
- mutable object provided;
- classification invalid.

---

## 135. Sink Validation

Reject sink registration when:

- sink ID duplicates;
- accepted classifications conflict with sink type;
- formatter missing;
- retention unbounded;
- restricted sink permissions cannot be configured as required;
- audit sink permits sampling;
- fallback group weakens classification.

---

## 136. Rotation Validation

Reject rotation when:

- sink does not support rotation;
- active file state unknown;
- target naming conflicts unsafely;
- flush precondition cannot be met;
- requested path escapes approved directory.

---

## 137. Export Validation

Reject export when:

- caller clearance insufficient;
- destination prohibited;
- bundle size unbounded;
- restricted data requested without authorization;
- raw file copy requested;
- final safety inspection fails.

---

# Part XXII — Cross-Module Rules

## 138. Configuration

Configuration supplies policy and sink settings.

It stores only `SecretReference` for remote sink credentials.

Configuration activation logs must not include raw changed values when those values may be sensitive.

---

## 139. Secret Management

Logging may use Secret Management safety inspection and redaction contracts.

Logging must never call secret resolution for diagnostic output.

---

## 140. Event Bus

Event Bus and Logging must avoid recursive loops.

Rules:

- Logging critical failures do not depend only on Event Bus;
- Event Bus payloads are not logged;
- only safe envelope metadata is logged;
- Logging health events use guarded publication;
- Event Bus failure reporting may use Logging emergency path.

---

## 141. Runtime

Runtime emits structured lifecycle and error records.

Work payloads and user content are represented by safe IDs and summaries.

---

## 142. Provider Management

Provider logs use normalized provider IDs, models, states, and error codes.

Raw requests, responses, credentials, and translated content are prohibited.

---

## 143. Telemetry

Telemetry provides:

```text
traceId
spanId
metric aggregation
trace sampling
exporters
```

Logging may consume safe trace context.

Logging does not own Telemetry lifecycle.

---

## 144. Presentation

Presentation may query safe recent logs through diagnostics contracts.

Restricted and audit records require explicit clearance.

---

# Part XXIII — Contract Decisions

## 145. Decisions

### Decision 1 — Immutable structured records

Accepted records are immutable.

### Decision 2 — Stable message templates

Variable data belongs in structured properties.

### Decision 3 — Safety before buffering

Redaction and classification occur before normal buffer admission.

### Decision 4 — User content denied by default

Content uses references and summaries.

### Decision 5 — Bounded property model

Arbitrary object reflection is prohibited.

### Decision 6 — Asynchronous normal path

Normal producers do not write directly to file sinks.

### Decision 7 — Emergency path is separate

Critical internal failure reporting is synchronous, bounded, and non-recursive.

### Decision 8 — Sink classification is enforced

Restricted records never fall back to weaker sinks.

### Decision 9 — Audit contract is distinct

Audit may share infrastructure but uses different semantics and failure policy.

### Decision 10 — Export uses second inspection

Raw log-file copying is prohibited by default.

### Decision 11 — Persistence confirmation is sink-local

It does not overstate operating-system durability.

### Decision 12 — Telemetry remains separate

Logs may carry trace context but do not own metrics or spans.

---

# Part XXIV — Open Decisions

## 146. API Decisions

Still to finalize:

- exact language-level logger interface;
- whether convenience methods return receipts;
- exact property value union;
- exact scope propagation mechanism;
- exact template ID generation;
- whether record IDs are producer- or logger-generated.

---

## 147. Safety Decisions

Still to finalize:

- known-secret matcher integration;
- high-entropy detection threshold;
- safe email masking;
- path masking policy;
- URL normalization policy;
- user-content diagnostic opt-in;
- safe snippet maximum length.

---

## 148. Buffer Decisions

Still to finalize:

- normal capacity;
- security reserve;
- audit capacity;
- admission timeout;
- batch size;
- record-age expiration;
- severity eligibility for emergency path.

---

## 149. File Decisions

Still to finalize:

- JSON Lines versus structured text default;
- maximum file size;
- retained file count;
- retention age;
- compression format;
- roll-on-startup default;
- per-platform permissions;
- file-locking behavior.

---

## 150. Lifecycle Decisions

Still to finalize:

- bootstrap handoff semantics;
- live policy update barrier;
- quiesce thresholds;
- flush timeout;
- shutdown loss summary;
- sink restart behavior;
- failed sink reactivation.

---

## 151. Audit Decisions

Still to finalize:

- whether audit remains inside Logging long-term;
- tamper-evident format;
- mandatory audited actions;
- retention duration;
- emergency audit storage;
- user-facing audit export.

---

# Part XXV — Documentation Order

## 152. Recommended Order

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

- Logging lifecycle;
- policy lifecycle;
- buffer lifecycle;
- record lifecycle;
- sink lifecycle;
- sink-health lifecycle;
- file lifecycle;
- rotation lifecycle;
- flush lifecycle;
- retention cleanup lifecycle;
- diagnostics export lifecycle;
- audit-write lifecycle;
- emergency logger lifecycle.

---

# Part XXVI — Related Documents

## 153. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md

03-infrastructure/logging/MODULE.md

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
03-infrastructure/logging/STATES.md
03-infrastructure/logging/EVENTS.md
03-infrastructure/logging/ERRORS.md
03-infrastructure/logging/README.md
```

---

## 154. Summary

The Logging contract defines a safe structured boundary from application producers to bounded buffers and authorized sinks.

The normal write flow is:

```text
LogWriteRequest
    ↓
Policy filtering
    ↓
Context enrichment
    ↓
Exception normalization
    ↓
Sensitive-data inspection
    ↓
Redaction
    ↓
Immutable LogRecord
    ↓
Bounded buffer admission
    ↓
Sink routing
    ↓
Formatting and write
```

The diagnostics export flow is:

```text
Select safe records
    ↓
Apply export policy
    ↓
Second redaction pass
    ↓
Assemble bundle
    ↓
Inspect complete bundle
    ↓
Write destination
```

The contract guarantees:

- structured immutable records;
- stable message templates;
- no secret material;
- no user content by default;
- bounded properties and record size;
- safe exception summaries;
- asynchronous bounded buffering;
- severity-aware overflow;
- sink isolation;
- restricted routing;
- rolling files and bounded retention;
- bounded flush and shutdown;
- non-recursive emergency reporting;
- distinct audit semantics;
- safe diagnostics export;
- separation from Event Bus and Telemetry.

This document is the contract source of truth for subsequent Logging states, events, errors, and implementation documentation.
