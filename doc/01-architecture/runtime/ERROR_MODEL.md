# runtime/ERROR_MODEL.md

# Runtime Error Model

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime:

- normalize execution failure;
- phân loại error;
- xác định scope và severity;
- giữ correlation context;
- phân biệt failure với cancellation, stale và abandonment;
- hỗ trợ recovery decision;
- map error sang user-safe presentation;
- quan sát error mà không lộ user content.

Tài liệu này không sở hữu WorkItem lifecycle hoặc terminal outcome.

Canonical terminal outcome được định nghĩa tại `PIPELINE_RUNTIME.md`.

---

## 2. Architectural Position

```text
Worker / Provider / Runtime Component
        ↓ detects failure
Boundary Adapter
        ↓ normalizes
RuntimeError
        ↓
Runtime Control
        ↓ validates relevance and authority
Recovery Policy
        ↓
Retry / Degrade / Fail / User Action
        ↓
User-Safe Presentation and Observability
```

Ranh giới cốt lõi:

```text
PIPELINE_RUNTIME owns terminal outcomes.

ERROR_MODEL owns normalized error meaning.

RETRY_POLICY owns retry strategy.

Runtime Control owns relevance and accepted state transition.
```

---

## 3. Error vs Outcome

Error và terminal outcome là hai khái niệm khác nhau.

### RuntimeError

Mô tả nguyên nhân, category, severity, scope, recoverability và diagnostic context.

### Terminal Outcome

Mô tả trạng thái logic cuối cùng được Runtime Control chấp nhận:

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

Một `RuntimeError` thường đi kèm `FAILED`, nhưng không phải mọi terminal outcome đều có error.

Ví dụ:

```text
CANCELED
    → expected control flow, có thể không có RuntimeError

STALE
    → authority rejection, không nhất thiết có failure

ABANDONED
    → runtime stopped waiting, physical execution may continue
```

---

## 4. Core Principles

1. Mọi failure phải được normalize trước khi vượt architecture boundary.
2. Provider-specific exception không đi trực tiếp tới UI.
3. Cancellation không mặc định là failure.
4. Stale result không mặc định là failure.
5. Abandoned execution phải được phân biệt với canceled execution.
6. Error không tự quyết định retry.
7. Error không tự thay đổi WorkItem state.
8. Error chỉ ảnh hưởng current state sau relevance validation.
9. Error context không chứa user content mặc định.
10. Fatal error chỉ dùng khi runtime invariant không thể giữ an toàn.
11. Warnings không tạo terminal outcome mới.
12. Duplicate error signal phải xử lý idempotently.

---

## 5. RuntimeError Model

Conceptual model:

```text
RuntimeError
├── ErrorCode
├── Category
├── Severity
├── Scope
├── Recoverability
├── RetryHint
├── OwnerModule
├── BusinessStageId
├── WorkType
├── Operation
├── MessageKey
├── TechnicalMessage
├── UserMessageKey
├── ProviderCode
├── CauseRef
├── Context
├── OccurredAt
└── Correlation
```

Exact implementation structure là language-specific.

---

## 6. Stable Error Code

Mỗi error phải có machine-readable `ErrorCode`.

Ví dụ:

```text
INPUT_INVALID
ARTIFACT_NOT_FOUND
ARTIFACT_INTEGRITY_FAILED
PROVIDER_TIMEOUT
PROVIDER_RATE_LIMITED
PROVIDER_AUTH_FAILED
RESOURCE_BUDGET_EXCEEDED
CONFIGURATION_INVALID
PERSISTENCE_WRITE_FAILED
INVALID_RUNTIME_STATE
INVARIANT_VIOLATION
```

Error code phải:

- stable;
- không chứa variable data;
- hỗ trợ metrics;
- hỗ trợ mapping;
- hỗ trợ test;
- không phụ thuộc wording UI.

---

## 7. Error Category

Runtime v2 dùng category ổn định:

```text
INPUT
VALIDATION
BUSINESS_MODULE
PROVIDER
RESOURCE
ARTIFACT
STATE
CONFIGURATION
SECURITY
PERSISTENCE
INTEGRATION
OBSERVABILITY
INTERNAL
```

Category không được phản chiếu mọi capability nội bộ.

Module-specific error code vẫn được phép, ví dụ:

```text
RECOGNITION_PROVIDER_TIMEOUT
TRANSLATION_INPUT_INVALID
PRESENTATION_COMMIT_REJECTED
```

---

## 8. Severity

```text
INFO
WARNING
ERROR
CRITICAL
FATAL
```

### INFO

Expected operational outcome hoặc suppressed stale event.

### WARNING

Recoverable degradation.

### ERROR

Current operation failed.

### CRITICAL

Major runtime capability unavailable hoặc repeated failure threatens service quality.

### FATAL

Runtime không thể tiếp tục an toàn mà vẫn giữ invariant.

Severity không quyết định retry strategy.

---

## 9. Error Scope

Impact scope chuẩn:

```text
ATTEMPT
WORK_ITEM
REVISION
SESSION
PROVIDER
RUNTIME_COMPONENT
APPLICATION
```

### Attempt

Chỉ physical execution hiện tại bị ảnh hưởng.

### WorkItem

Logical work không thể hoàn thành bằng current path.

### Revision

Required output của revision không thể tạo.

### Session

Reading Session không thể tiếp tục.

### Provider

Provider cần degrade, disable hoặc reinitialize.

### Runtime Component

Một runtime component mất khả năng hoạt động đúng.

### Application

Runtime không thể tiếp tục an toàn.

`BusinessStageId` là context, không phải canonical impact scope.

---

## 10. Recoverability

```text
RECOVERABLE
RECOVERABLE_WITH_DEGRADATION
RECOVERABLE_AFTER_CONFIGURATION_CHANGE
RECOVERABLE_AFTER_USER_ACTION
NON_RECOVERABLE_FOR_WORK_ITEM
NON_RECOVERABLE_FOR_REVISION
NON_RECOVERABLE_FOR_SESSION
NON_RECOVERABLE_FOR_APPLICATION
```

Recoverability khác retryability.

Ví dụ:

```text
Provider unavailable
    → recoverable through fallback
    → không nhất thiết retry cùng provider
```

---

## 11. Retry Hint

Error Model chỉ cung cấp hint:

```text
NONE
TRANSIENT
AFTER_CONFIGURATION_CHANGE
AFTER_USER_ACTION
PROVIDER_ALTERNATIVE_POSSIBLE
RESOURCE_RECOVERY_POSSIBLE
UNKNOWN
```

`RETRY_POLICY.md` quyết định:

```text
RETRY_NOW
RETRY_LATER
RETRY_WITH_FALLBACK
WAIT_FOR_RESOURCE
DO_NOT_RETRY
```

Error không chứa retry decision cuối.

---

## 12. Correlation

Error correlation nên hỗ trợ:

```text
ApplicationInstanceId
SessionId
RevisionId
WorkItemId
AttemptId
BusinessStageId
WorkType
ProviderId
Operation
```

Không phải field nào cũng bắt buộc trong mọi error.

---

## 13. Cause Chain

Error có thể giữ internal cause chain:

```text
TRANSLATION_REQUEST_FAILED
    caused by
PROVIDER_NETWORK_ERROR
    caused by
SOCKET_TIMEOUT
```

Recovery dùng normalized top-level code.

Raw cause chỉ dành cho diagnostics và phải redaction.

---

## 14. Error Context

Context chỉ chứa metadata cần thiết:

```text
Timeout
InputSize
RegionCount
MemoryPressureLevel
ProviderRequestId
ModelId
ConfigurationVersion
ArtifactType
ArtifactVersion
OperationPhase
```

Không chứa mặc định:

- screenshot;
- OCR text;
- source text;
- translation output;
- prompt;
- API key;
- token;
- complete provider payload;
- private file path chưa sanitize.

---

## 15. Error Ownership

| Component | Responsibility |
|---|---|
| Worker | Detect local execution failure |
| Provider Adapter | Normalize provider-specific failure |
| Business Module | Validate semantic correctness |
| Runtime Control | Validate relevance and authority |
| Retry Policy | Decide retry eligibility and strategy |
| Scheduler | Admit new Attempt |
| Artifact Store | Detect publication/integrity error |
| Resource Manager | Detect disposal and ownership error |
| Storage | Normalize persistence failure |
| Presentation Boundary | Map current relevant error to user-safe model |
| Observability | Record diagnostics and metrics |

Detecting component không mặc định sở hữu recovery decision.

---

## 16. Exception Boundary

Incorrect:

```text
Provider SDK Exception
    ↓
UI
```

Correct:

```text
Provider SDK Exception
    ↓
Provider Adapter
    ↓
Normalized RuntimeError
    ↓
AttemptCompletion
    ↓
Runtime Control
```

Raw exception không trở thành domain control flow.

---

## 17. AttemptCompletion Boundary

Worker hoặc provider chỉ báo completion.

Ví dụ:

```text
AttemptFailed
├── SessionId
├── RevisionId
├── WorkItemId
├── AttemptId
├── RuntimeError
└── TimingMetadata
```

`AttemptFailed` chưa phải accepted WorkItem outcome.

Runtime Control phải validation trước.

---

## 18. Relevance Validation

Trước khi error ảnh hưởng state, retry hoặc UI, Runtime Control kiểm tra:

- session còn active;
- revision còn relevant;
- WorkItem chưa terminal;
- Attempt còn hợp lệ;
- authority chưa revoke;
- error không duplicate;
- configuration context còn phù hợp;
- newer outcome chưa được accept.

Error từ obsolete execution có thể vẫn ghi diagnostics nhưng không thay đổi current user state.

---

## 19. Failure, Cancellation, Stale and Abandoned

### Failure

Execution không tạo được valid result.

### Cancellation

Execution được yêu cầu dừng và cancellation outcome được chấp nhận.

### Stale

Result hoặc error không còn authority.

### Abandoned

Runtime ngừng chờ physical execution nhưng chưa xác nhận execution đã dừng.

Các khái niệm này không được collapse.

---

## 20. Cancellation Normalization

Provider SDK có thể biểu diễn cancellation như exception.

Adapter phải normalize expected cancellation thành cancellation completion, không phải generic failure.

Chỉ cancellation cleanup failure mới có thể tạo additional `RuntimeError`.

---

## 21. Stale Error Suppression

Ví dụ:

```text
Revision A provider timeout
Revision B already committed
```

Timeout của Revision A:

- được giữ cho diagnostics;
- không hiển thị như current failure;
- không làm downgrade Revision B;
- không tự tạo retry nếu authority đã mất.

---

## 22. Abandoned Error Handling

Khi Attempt abandoned:

- authority đã revoke;
- downstream work bị cấm;
- provider capacity có thể vẫn occupied;
- late Completion bị reject;
- resource vẫn được theo dõi;
- repeated abandonment có thể degrade provider.

Abandonment không chứng minh provider failure.

---

## 23. Expected vs Unexpected Error

### Expected

Runtime đã có policy xử lý:

- provider timeout;
- permission denied;
- invalid input;
- rate limit;
- configuration missing;
- resource pressure;
- persistence unavailable;
- cancellation cleanup timeout.

### Unexpected

Có thể là invariant defect:

- impossible transition;
- ownership corruption;
- duplicate accepted publication;
- null required Artifact;
- unhandled native crash;
- impossible WorkItem lineage.

Unexpected error cần diagnostics mạnh hơn.

---

## 24. Input Errors

Input error là external hoặc business input invalid.

Ví dụ:

```text
INPUT_MISSING
INPUT_INVALID
INPUT_TOO_LARGE
SOURCE_UNSUPPORTED
LANGUAGE_PAIR_UNSUPPORTED
```

Thường:

- không retry với cùng input;
- cần user action hoặc replan;
- scope WorkItem, Revision hoặc Session.

---

## 25. Validation Errors

Validation error xảy ra khi data không đáp ứng contract hoặc integrity.

Ví dụ:

```text
ARTIFACT_TYPE_MISMATCH
ARTIFACT_VERSION_UNSUPPORTED
OUTPUT_CONTRACT_INVALID
GEOMETRY_INVALID
TRACEABILITY_INVALID
```

Validation failure phải ngăn publication hoặc commit.

---

## 26. Business Module Errors

Business Module sở hữu catalog chi tiết của mình.

Runtime chỉ yêu cầu các module normalize về shared model.

Ví dụ:

```text
modules/recognition/ERRORS.md
modules/translation/ERRORS.md
modules/presentation/ERRORS.md
```

Runtime Error Model không duy trì catalog đầy đủ của OCR, Layout hoặc Translation.

---

## 27. Provider Error Model

Provider Adapter normalize raw error thành:

```text
PROVIDER_TIMEOUT
PROVIDER_UNAVAILABLE
PROVIDER_RATE_LIMITED
PROVIDER_AUTH_FAILED
PROVIDER_QUOTA_EXCEEDED
PROVIDER_REQUEST_INVALID
PROVIDER_RESPONSE_INVALID
PROVIDER_CONTENT_REJECTED
PROVIDER_INTERNAL_ERROR
PROVIDER_NETWORK_ERROR
PROVIDER_CANCELED
```

Optional diagnostic fields:

```text
ProviderCode
ProviderHttpStatus
ProviderRequestId
TimeoutPhase
```

Recovery không phụ thuộc raw message string.

---

## 28. Provider Authentication Error

Authentication error thường:

```text
Recoverability = RECOVERABLE_AFTER_USER_ACTION
RetryHint = AFTER_USER_ACTION
```

Không automatic retry lặp lại.

---

## 29. Provider Rate Limit

Rate limit khác unavailable.

Runtime có thể:

- tôn trọng Retry-After;
- giảm admission;
- delay retry;
- fallback;
- mark provider degraded.

Policy chi tiết thuộc Retry và Provider management.

---

## 30. Provider Quota Error

Quota exhaustion thường cần:

- provider switch;
- local fallback;
- billing/configuration change;
- user notification.

Không retry unchanged request liên tục.

---

## 31. Resource Errors

```text
RESOURCE_BUDGET_EXCEEDED
MEMORY_BUDGET_EXCEEDED
GPU_MEMORY_EXHAUSTED
BUFFER_ALLOCATION_FAILED
RESOURCE_OWNERSHIP_INVALID
RESOURCE_DISPOSAL_FAILED
NATIVE_RESOURCE_UNAVAILABLE
```

Resource error có thể là expected race nếu scope đã canceled.

Runtime phải kiểm tra cancellation và revision state trước khi escalate.

---

## 32. Artifact Errors

```text
ARTIFACT_NOT_FOUND
ARTIFACT_NOT_PUBLISHED
ARTIFACT_ALREADY_DISPOSED
ARTIFACT_LEASE_FAILED
ARTIFACT_TYPE_MISMATCH
ARTIFACT_INTEGRITY_FAILED
DUPLICATE_ARTIFACT_PUBLICATION
```

Không phải mọi Artifact miss là internal failure.

Ví dụ artifact bị dispose sau cancellation có thể dẫn tới `CANCELED` hoặc `STALE`.

---

## 33. State and Invariant Errors

```text
INVALID_RUNTIME_STATE
INVALID_STATE_TRANSITION
WORKITEM_ALREADY_TERMINAL
ATTEMPT_ALREADY_SUPERSEDED
COMMIT_WITHOUT_AUTHORITY
DUPLICATE_TERMINAL_COMPLETION
INVARIANT_VIOLATION
```

Duplicate signal có thể benign nếu idempotent.

Ownership corruption hoặc impossible transition có thể là fatal.

---

## 34. Configuration Errors

```text
CONFIGURATION_INVALID
PROVIDER_NOT_CONFIGURED
CREDENTIAL_REFERENCE_MISSING
INVALID_TIMEOUT
INVALID_CONCURRENCY_LIMIT
INVALID_MEMORY_BUDGET
UNSUPPORTED_RUNTIME_MODE
```

Thường yêu cầu configuration change hoặc user action.

---

## 35. Security Errors

```text
CREDENTIAL_ACCESS_DENIED
CREDENTIAL_RESOLUTION_FAILED
UNTRUSTED_PROVIDER_CONFIGURATION
UNSAFE_PATH
INVALID_PLUGIN_SIGNATURE
PERMISSION_REQUIRED
```

Security error phải:

- không leak secret;
- không unsafe fallback;
- có elevated severity phù hợp;
- cung cấp user action rõ ràng khi cần.

---

## 36. Persistence Errors

Storage là Persistence Capability đã được thiết kế, không còn là future optional concern.

Storage boundary normalize:

```text
PERSISTENCE_UNAVAILABLE
PERSISTENCE_READ_FAILED
PERSISTENCE_WRITE_FAILED
PERSISTENCE_CONFLICT
PERSISTENCE_CORRUPT
PERSISTENCE_QUOTA_EXCEEDED
PERSISTENCE_VERSION_UNSUPPORTED
PERSISTENCE_MIGRATION_FAILED
RECOVERY_POINT_INVALID
```

Business data ownership vẫn thuộc owning module.

Persistence error không được làm Runtime Artifact Store biến thành durable Storage.

---

## 37. Observability Errors

```text
TELEMETRY_EXPORT_FAILED
TRACE_BUFFER_FULL
METRIC_PUBLISH_FAILED
DIAGNOSTIC_SNAPSHOT_FAILED
```

Default behavior:

```text
record bounded local diagnostic if possible
degrade observability
do not change runtime correctness
```

---

## 38. Integration Errors

```text
INTEGRATION_CONTRACT_INVALID
ADAPTER_UNAVAILABLE
PROTOCOL_MISMATCH
UNSUPPORTED_PROVIDER_CAPABILITY
```

Integration error phải được normalize tại adapter boundary.

---

## 39. Internal Errors

```text
INVARIANT_VIOLATION
OWNERSHIP_ACCOUNTING_CORRUPT
UNEXPECTED_EXECUTION_STATE
UNHANDLED_EXCEPTION
CONTROL_LOOP_FAILED
```

Internal error phải có:

- high diagnostic severity;
- correlation;
- safe snapshot;
- no user content by default;
- explicit escalation scope.

---

## 40. Fatal Error Policy

Fatal error chỉ khi Runtime không thể giữ invariant.

Ví dụ:

- Runtime Control loop mất khả năng xử lý;
- Artifact ownership state corrupt;
- security boundary compromised;
- repeated main-process native crash;
- cannot guarantee safe commit or disposal.

Handling:

```text
Stop new admission
    ↓
Revoke authority
    ↓
Prevent new commits
    ↓
Cancel sessions
    ↓
Bounded cleanup
    ↓
Persist safe diagnostics
    ↓
Exit or restart safely
```

---

## 41. Error Propagation

```text
Failure detected
    ↓
RuntimeError normalized
    ↓
AttemptCompletion submitted
    ↓
Runtime Control validates relevance
    ↓
Accepted outcome or suppressed stale diagnostic
    ↓
Recovery policy evaluated
    ↓
Event and user-safe mapping
```

Worker không tự quyết định toàn bộ recovery.

---

## 42. Error Aggregation

Một WorkItem hoặc Revision có thể có nhiều Attempt error.

Conceptual aggregate:

```text
FailureAggregate
├── PrimaryError
├── AttemptErrors[]
├── FallbackErrors[]
├── CleanupErrors[]
├── FinalDisposition
└── UserVisibleCause
```

User chỉ nhận một clear final message.

---

## 43. Root Error Selection

Chọn root cause theo:

1. final blocking cause;
2. user actionability;
3. impact scope;
4. causal relation;
5. severity;
6. current relevance.

Diagnostics vẫn giữ full bounded chain.

---

## 44. UserVisibleError

```text
UserVisibleError
├── TitleKey
├── MessageKey
├── Level
├── SuggestedAction
├── RetryAllowed
├── OpenSettingsAllowed
├── PreservePreviousPresentation
└── TechnicalReference
```

UI không nhận raw exception hoặc provider message.

---

## 45. User Error Levels

```text
INLINE_NOTICE
NON_BLOCKING_WARNING
REVISION_ERROR
SESSION_BLOCKING_ERROR
APPLICATION_BLOCKING_ERROR
```

Level không map trực tiếp từ technical severity; phải xét user impact và relevance.

---

## 46. Suggested User Actions

```text
RETRY
RESELECT_SOURCE
CHECK_NETWORK
OPEN_PROVIDER_SETTINGS
CHANGE_PROVIDER
REDUCE_INPUT
RESTART_SESSION
RESTART_APPLICATION
REPORT_PROBLEM
NONE
```

Action dựa trên normalized code và current runtime state.

---

## 47. Preserve Previous Presentation

Khi current revision lỗi, Presentation có thể giữ previous valid output nếu:

- được đánh dấu rõ là previous;
- không bị hiểu nhầm là current content;
- current error được hiển thị phù hợp;
- user vẫn có thể tiếp tục thao tác.

Policy chi tiết thuộc Presentation.

---

## 48. Warning Model

Warnings không invalidate output.

```text
Warning
├── WarningCode
├── Severity
├── OwnerModule
├── UserVisible
└── Metadata
```

MVP dùng:

```text
SUCCEEDED + bounded warnings
```

Không tạo `SUCCEEDED_WITH_WARNINGS` terminal outcome.

---

## 49. Warning Rules

Warnings phải:

- bounded;
- observable;
- privacy-safe;
- không tự kích hoạt retry;
- không thay đổi immutable Artifact;
- được attach vào Completion metadata hoặc Artifact metadata phù hợp.

---

## 50. Error Events

Conceptual events:

```text
RUNTIME_ERROR_NORMALIZED
WORK_FAILED
REVISION_DEGRADED
REVISION_FAILED
SESSION_DEGRADED
SESSION_FAILED
PROVIDER_DEGRADED
PROVIDER_RECOVERED
RESOURCE_CLEANUP_FAILED
PERSISTENCE_DEGRADED
RUNTIME_FATAL
```

Tên cuối phải tuân theo Event Standard.

---

## 51. Event Payload

```text
ErrorCode
Category
Severity
Scope
Recoverability
RetryHint
SessionId
RevisionId
WorkItemId
AttemptId
BusinessStageId
WorkType
ProviderId
OccurredAt
```

Không chứa raw content.

---

## 52. Logging Policy

### Trace / Debug

- stale result rejected;
- expected cancellation;
- duplicate terminal signal ignored.

### Info

- fallback activated;
- provider recovered;
- degraded mode entered.

### Warning

- transient provider failure;
- cache/persistence degraded;
- cleanup slow.

### Error

- WorkItem or Revision failed.

### Critical / Fatal

- invariant violation;
- Runtime Control failure;
- application-wide unsafe state.

Expected cancellation không flood logs.

---

## 53. Metrics

Theo dõi:

- errors by code/category/scope;
- provider failures;
- resource failures;
- persistence failures;
- transient vs permanent;
- retry hint distribution;
- stale error suppressed;
- canceled and abandoned count;
- revision/session failure rate;
- fallback success;
- fatal count;
- cleanup failure;
- user action rate;
- warning count.

Không dùng `stage` làm metric dimension bắt buộc; ưu tiên `OwnerModule`, `BusinessStageId`, `WorkType`.

---

## 54. Error Tracing

Trace nên trả lời:

- session nào;
- revision nào;
- WorkItem nào;
- Attempt nào;
- owner module nào;
- operation nào;
- provider nào;
- authority còn hay không;
- retry/fallback có xảy ra không;
- final disposition là gì.

---

## 55. Privacy

Standard error diagnostics không chứa:

- screenshots;
- recognized text;
- translated text;
- prompts;
- tokens;
- API keys;
- raw provider body;
- window title;
- source URL;
- private path chưa sanitize.

Content diagnostics chỉ có khi explicit consent và bounded retention.

---

## 56. Error Deduplication

Dedup key có thể gồm:

```text
ErrorCode
OwnerModule
ProviderId
SessionId
TimeWindow
```

Metrics vẫn giữ occurrence count.

UI và logs có thể rate-limit repeated notification.

---

## 57. Error Storm Protection

Protection:

- deduplication;
- log throttling;
- event sampling;
- provider degradation;
- retry backoff;
- aggregate notification;
- session pause khi cần;
- bounded diagnostic buffer.

Error handling không được tự tạo thêm overload.

---

## 58. Provider Degradation

Repeated provider error có thể chuyển health:

```text
HEALTHY
  ↓
DEGRADED
  ↓
UNAVAILABLE
  ↓
PROBING
  ↓
HEALTHY
```

Provider Manager sở hữu health state.

Error Model chỉ cung cấp normalized signals.

---

## 59. Revision Failure

Revision fail khi required output không thể tạo và recovery path đã hết.

```text
Required WorkItem failed
    ↓
Retry/fallback exhausted
    ↓
Revision failure accepted
```

Revision failure không tự kết thúc Session.

---

## 60. Session Failure

Session fail khi không thể tiếp tục configured reading intent.

Ví dụ:

- source permanently unavailable;
- permission unavailable;
- required capability unavailable;
- configuration invalid;
- repeated critical resource failure.

Application vẫn có thể hoạt động.

---

## 61. Application Failure

Chỉ dùng khi runtime itself unsafe.

Revision, provider hoặc Storage error không được escalate lên Application nếu còn cô lập được.

---

## 62. Artifact Publication Boundary

Candidate output từ:

```text
FAILED
CANCELED
STALE
ABANDONED
```

không được publish như accepted reusable Artifact.

MVP:

```text
Failure
    → no successful cache promotion
```

Negative cache là future capability riêng.

---

## 63. Cleanup Failure

Cleanup failure không thay primary logical outcome.

```text
Primary Outcome
+
Cleanup RuntimeError
```

Cleanup error có thể degrade Resource Manager hoặc Provider health.

---

## 64. Error and Shutdown

Trong shutdown:

- expected cancellations normalize thành `CANCELED`;
- unresponsive execution có thể thành `ABANDONED`;
- stale late results bị reject;
- chỉ cleanup failure đe dọa safety mới escalate.

User không nên thấy hàng loạt error do expected shutdown.

---

## 65. Error and Race Resolution

### Failure vs Cancellation

Serialized Runtime Control quyết định event nào được accept trước.

### Completion vs Timeout

First accepted terminal command wins.

### Duplicate Completion

First accepted outcome wins; duplicate ignored and recorded.

### Late Attempt Result

Authority validation rejects stale result.

---

## 66. Error Mapping Registry

Conceptual registry:

```text
Raw Provider Error
    → RuntimeError

RuntimeError
    → RetryHint

RuntimeError + Current State
    → UserVisibleError

RuntimeError
    → Severity / Scope / Recoverability
```

Không bắt buộc một global class duy nhất trong MVP.

---

## 67. MVP Error Policy

### Required Error Fields

```text
ErrorCode
Category
Severity
Scope
Recoverability
RetryHint
TechnicalMessage
Correlation
```

### Required Behaviors

1. Provider error được normalize.
2. Cancellation không log như failure.
3. Stale error không ảnh hưởng current UI.
4. Failed candidate không promote.
5. One WorkItem accepts one terminal outcome.
6. Error không chứa secret hoặc reading content.
7. Retry decision centralized.
8. Session failure không làm Application fail vô cớ.
9. Fatal error dừng admission trước shutdown.
10. Warning không tạo outcome mới.
11. Persistence error normalize tại Storage boundary.
12. Observability failure không phá runtime correctness.

---

## 68. Suggested MVP Error Codes

### Runtime

```text
RUNTIME_INTERNAL_ERROR
RUNTIME_INVALID_STATE
RUNTIME_SHUTTING_DOWN
RUNTIME_CONTROL_FAILED
```

### Provider

```text
PROVIDER_TIMEOUT
PROVIDER_UNAVAILABLE
PROVIDER_AUTH_FAILED
PROVIDER_RATE_LIMITED
PROVIDER_QUOTA_EXCEEDED
PROVIDER_RESPONSE_INVALID
```

### Resource and Artifact

```text
ARTIFACT_NOT_FOUND
ARTIFACT_INTEGRITY_FAILED
ARTIFACT_DISPOSED
RESOURCE_BUDGET_EXCEEDED
RESOURCE_CLEANUP_FAILED
```

### Configuration and Security

```text
CONFIGURATION_INVALID
CREDENTIAL_REFERENCE_MISSING
CREDENTIAL_ACCESS_DENIED
PERMISSION_REQUIRED
```

### Persistence

```text
PERSISTENCE_UNAVAILABLE
PERSISTENCE_READ_FAILED
PERSISTENCE_WRITE_FAILED
PERSISTENCE_CONFLICT
PERSISTENCE_CORRUPT
```

Module-specific code nằm trong tài liệu module.

---

## 69. Example: Provider Timeout

```text
Provider request times out
        ↓
Adapter creates PROVIDER_TIMEOUT
        ↓
AttemptFailed submitted
        ↓
Runtime Control validates relevance
        ↓
Retry Policy evaluates
```

Nếu revision stale, error chỉ dùng cho diagnostics.

---

## 70. Example: Invalid Provider Response

```text
Provider response malformed
        ↓
Adapter normalization fails validation
        ↓
PROVIDER_RESPONSE_INVALID
        ↓
No accepted Artifact publication
        ↓
Retry/fallback evaluated
```

---

## 71. Example: Artifact Missing During Cancellation

```text
Worker requests Artifact Lease
        ↓
Revision cancellation already draining
        ↓
Lease denied
```

Expected outcome có thể là `CANCELED` hoặc `STALE`, không tự động là internal error.

---

## 72. Example: Persistence Conflict

```text
Storage write uses stale PersistenceVersion
        ↓
PERSISTENCE_CONFLICT
        ↓
Owning module decides merge/reload behavior
```

Storage không chiếm business ownership.

---

## 73. Example: Observability Export Failure

```text
Trace export fails
        ↓
TELEMETRY_EXPORT_FAILED
        ↓
Local bounded diagnostic retained if possible
        ↓
Runtime execution continues
```

---

## 74. Example: Fatal Invariant

```text
Artifact ownership accounting corrupt
        ↓
INVARIANT_VIOLATION
        ↓
Stop admission
        ↓
Revoke authority
        ↓
Bounded cleanup
        ↓
Safe shutdown
```

---

## 75. Architecture Invariants

1. Terminal outcome canonical thuộc `PIPELINE_RUNTIME.md`.
2. Error Model không định nghĩa lại WorkItem lifecycle.
3. RuntimeError khác terminal outcome.
4. Failure, cancellation, stale và abandoned khác nhau.
5. Provider exception phải normalize.
6. Error không tự retry.
7. Error không tự commit.
8. Error không tự thay state.
9. Relevance validation bắt buộc.
10. Obsolete error không thay current UI.
11. Warnings không tạo outcome mới.
12. Failed output không promote.
13. Cleanup failure không đổi primary outcome.
14. Duplicate terminal signal idempotent.
15. Fatal error dừng admission trước shutdown.
16. Persistence error thuộc Storage boundary.
17. Artifact error không mặc định là persistence error.
18. Observability failure không phá correctness.
19. User-visible mapping không nhận raw exception.
20. Standard diagnostics không chứa user content.
21. Error category không hard-code capability nội bộ.
22. RetryHint không phải Retry decision.
23. Business Module sở hữu semantic error catalog.
24. Runtime Control sở hữu final relevance.
25. Error handling phải bounded dưới error storm.

---

## 76. Testing Requirements

Test phải bao phủ:

- provider timeout;
- auth failure;
- rate limit;
- quota exhaustion;
- malformed response;
- invalid input;
- validation failure;
- Artifact lease denied after cancellation;
- Artifact integrity failure;
- persistence conflict;
- persistence unavailable;
- observability export failure;
- stale error suppression;
- cancellation exception normalization;
- abandoned provider request;
- duplicate completion;
- timeout/completion race;
- cleanup failure after primary failure;
- provider degradation;
- user-safe mapping;
- warning handling;
- privacy validation;
- error deduplication;
- fatal invariant.

---

## 77. Open Questions

- Module-specific error catalogs nằm ở file nào cho từng module?
- Error Mapping Registry dùng code hay configuration?
- Provider degradation threshold là bao nhiêu?
- Warning nào user-visible mặc định?
- Previous presentation được giữ với error nào?
- Error technical detail trong diagnostics UI đến mức nào?
- Persistent negative cache có cần không?
- Fatal subsystem có restart riêng được không?
- Provider name có nên xuất hiện trong UI error không?
- Error deduplication window là bao lâu?

---

## 78. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | Terminal outcome và authority |
| `RUNTIME_COMPONENTS.md` | Error ownership theo component |
| `RETRY_POLICY.md` | Retry strategy |
| `CANCELLATION.md` | Canceled, stale và abandoned |
| `SCHEDULER.md` | Admission after recovery |
| `WORK_QUEUE.md` | Queued removal và dispatch integrity |
| `MEMORY_MODEL.md` | Artifact and revision state |
| `RESOURCE_LIFECYCLE.md` | Cleanup ownership |
| `CACHE_POLICY.md` | Failed-result promotion |
| `PERFORMANCE_MODEL.md` | Failure and recovery latency |
| `RUNTIME_OBSERVABILITY.md` | Error logs, metrics and traces |
| `RUNTIME_CONFIG.md` | Error and recovery configuration |
| `BOOT_SEQUENCE.md` | Fatal startup/shutdown handling |
| `../../modules/storage/ERRORS.md` | Persistence-specific error codes |
| `../../modules/*/ERRORS.md` | Business Module error catalogs |

---

## 79. Completion Criteria

`ERROR_MODEL.md` được xem là đồng bộ khi:

- outcome ownership được trả về `PIPELINE_RUNTIME.md`;
- RuntimeError tách khỏi AttemptCompletion và accepted outcome;
- taxonomy không còn hard-code OCR/Layout pipeline;
- `Stage` được thay bằng OwnerModule, BusinessStageId, WorkType và Operation;
- RetryHint không còn là Retry decision;
- scope chuẩn có Attempt, WorkItem, Revision, Session, Provider, Runtime Component và Application;
- module-specific catalog được đẩy về module;
- Storage persistence được phản ánh là capability hiện hữu;
- WorkItem/Revision state không bị định nghĩa lại;
- warnings không tạo outcome mới;
- privacy, deduplication và error storm được định nghĩa;
- fatal policy và ownership rõ ràng.

---

## 80. Summary

CRAI Runtime Error Model chuẩn hóa ý nghĩa của failure:

```text
Failure detected
        ↓
RuntimeError normalized
        ↓
Runtime Control validates relevance
        ↓
Recovery decision
        ↓
User-safe mapping
        ↓
Observability
```

Ranh giới cốt lõi:

```text
Outcome says what finally happened.

RuntimeError explains why execution failed.

Retry Policy decides whether another Attempt is useful.

Runtime Control decides whether the error still matters.
```
