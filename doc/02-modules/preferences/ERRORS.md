# Reading Session Errors

> **Project:** CRAI
> **Module:** `reading-session`
> **Path:** `doc/02-modules/reading-session/ERRORS.md`
> **Version:** 3.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the public error model of the Reading Session module.

Reading Session errors describe failures involving:

```text
Reading Session identity
Reading Session lifecycle
SessionConfiguration
SessionOverride
ReadingContext
ReadingContextRevision
session command validation
session-owned state persistence
session concurrency
session invariants
```

Reading Session errors do not describe failures involving:

```text
Capture execution
OCR execution
Text Processing execution
Translation execution
Presentation rendering
Runtime WorkItem scheduling
Runtime Attempt execution
pipeline retry
provider availability
Artifact cache execution
```

Those failures belong to their respective owners.

---

# 2. Error Boundary

Reading Session owns errors only when the failed invariant or operation belongs to Reading Session.

Correct:

```text
SessionNotFound
InvalidSessionStateTransition
ReadingContextRevisionConflict
InvalidSessionOverride
SessionCommitFailed
```

Incorrect:

```text
OCRTimeout
TranslationProviderUnavailable
PipelineRestartFailed
StageSchedulingFailed
CaptureMemoryLimitExceeded
```

---

# 3. Error Principles

## 3.1 Stable Error Codes

Consumers depend on:

```text
ErrorCode
```

rather than implementation-specific exceptions.

---

## 3.2 Session Integrity

A failed operation must not corrupt the last committed Reading Session state.

```text
Current committed state
        +
invalid/failed operation
        ↓
Current committed state preserved
```

---

## 3.3 Revision Safety

A failed Candidate revision must never replace a committed ReadingContextRevision.

---

## 3.4 Ownership Safety

Reading Session must not translate external Runtime or processing failures into fake Reading Session domain failures.

---

## 3.5 Privacy

Errors must never expose raw:

```text
captured image data
OCR text
translated text
credentials
tokens
private provider data
sensitive document contents
```

---

# 4. Error Code Format

Reading Session errors use:

```text
SES-<CATEGORY>-<NUMBER>
```

Examples:

```text
SES-REQ-001
SES-SESSION-001
SES-STATE-001
SES-CTX-001
SES-CONFIG-001
SES-REV-001
SES-PERSIST-001
SES-INT-001
```

---

# 5. Error Categories

Recommended categories:

| Prefix     | Category                     |
| ---------- | ---------------------------- |
| `REQ`      | Request Validation           |
| `SESSION`  | Session Identity / Lifecycle |
| `STATE`    | Session State                |
| `CTX`      | Reading Context              |
| `CONFIG`   | Session Configuration        |
| `REV`      | Reading Context Revision     |
| `CONFLICT` | Concurrency / Authority      |
| `PERSIST`  | Persistence Coordination     |
| `SCHEMA`   | Contract / Schema            |
| `SEC`      | Security / Privacy           |
| `INT`      | Internal Invariant           |

Removed from Reading Session:

```text
PIPE
RES
```

Pipeline and execution resource failures belong to Runtime/processing owners.

---

# 6. Severity Levels

| Severity   | Meaning                                                           |
| ---------- | ----------------------------------------------------------------- |
| `Info`     | Expected non-failure condition                                    |
| `Warning`  | Request rejected without state corruption                         |
| `Error`    | Session-owned operation failed                                    |
| `Critical` | Session-owned invariant or authoritative state may be compromised |

Severity does not prescribe retry automatically.

---

# 7. Recovery Model

Reading Session errors expose:

```text
RecoveryHint
```

rather than module-owned Runtime retry policies.

Recommended values:

```text
None
CorrectRequest
RefreshSessionState
RefreshRevision
RetryOperation
RecreateSession
ApplicationRecovery
```

---

# 8. Removed Retry Policies

The v1 policies:

```text
Transient
RestartSession
ResetRequired
```

are not used as generic Reading Session retry semantics.

Reason:

```text
retry
restart
pipeline recovery
Runtime reset
```

may belong to external orchestration.

`RecreateSession` may remain only when the Reading Session itself is no longer usable and creating a new session is the correct user/application action.

---

# 9. Public Error Contract

Conceptually:

```text
ReadingSessionError
├── errorCode
├── category
├── severity
├── messageKey?
├── recoveryHint?
├── sessionId?
├── sessionState?
├── readingContextRevision?
├── expectedRevision?
├── currentRevision?
├── requestId?
├── correlationId?
├── diagnosticRef?
└── metadata?
```

---

# 10. Error Message Rule

Public errors should expose stable machine-readable semantics.

User-facing localized text should normally be derived from:

```text
errorCode
+
messageKey
+
safe metadata
```

Do not treat exception text as public API.

---

# 11. Request Errors

## SES-REQ-001 — MissingSessionIdentifier

Meaning:

```text
A command requiring SessionId did not provide one.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

State mutation:

```text
None
```

---

# 12. SES-REQ-002 — InvalidSessionIdentifier

Meaning:

```text
SessionId format or contract is invalid.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 13. SES-REQ-003 — UnsupportedOperation

Meaning:

```text
The requested Reading Session operation is not supported
by the current contract/version/state model.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 14. SES-REQ-004 — MissingRequiredField

Meaning:

A required command field is absent.

Examples:

```text
source identity
expected revision
configuration field
context input
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 15. SES-REQ-005 — InvalidRequestPayload

Meaning:

```text
The command payload is malformed or violates
the Reading Session command schema.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 16. Session Identity Errors

## SES-SESSION-001 — SessionNotFound

Meaning:

```text
The requested Reading Session does not exist
or is no longer available.
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshSessionState
```

---

# 17. SES-SESSION-002 — SessionAlreadyExists

Meaning:

```text
A create operation attempted to create
an already-existing SessionId.
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshSessionState
```

---

# 18. SES-SESSION-003 — SessionUnavailable

Meaning:

```text
The Reading Session exists but cannot currently
accept the requested session-owned operation.
```

Possible reasons:

```text
disposing
disposed
recovery in progress
authoritative state unavailable
```

Severity:

```text
Warning / Error
```

Recovery:

```text
RefreshSessionState
```

---

# 19. SES-SESSION-004 — SessionDisposed

Meaning:

```text
The requested Reading Session has already been disposed.
```

Severity:

```text
Info
```

Recovery:

```text
RecreateSession
```

No existing disposed session is revived implicitly.

---

# 20. Session State Errors

## SES-STATE-001 — InvalidStateTransition

Meaning:

```text
The requested Reading Session lifecycle transition
is not allowed from the current state.
```

Example:

```text
Disposed → Active
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshSessionState
```

---

# 21. SES-STATE-002 — OperationNotAllowedInCurrentState

Meaning:

```text
The command is valid in general but not valid
for the session's current lifecycle state.
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshSessionState
```

---

# 22. SES-STATE-003 — SessionTransitionConflict

Meaning:

A concurrent lifecycle transition committed before this operation.

Severity:

```text
Warning
```

Recovery:

```text
RefreshSessionState
```

---

# 23. Removed State Errors

The following v1 concepts are removed:

```text
SessionAlreadyRunning
SessionAlreadyCompleted
SessionAlreadyCancelled
SessionPaused
SessionFailed
```

as hard-coded universal errors unless those states exist in the authoritative `STATES.md`.

Reading Session errors must follow the current lifecycle vocabulary rather than preserve obsolete state names.

---

# 24. Reading Context Errors

## SES-CTX-001 — ReadingContextNotAvailable

Meaning:

```text
The requested operation requires ReadingContext
but no committed context is available.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
or
RefreshSessionState
```

---

# 25. SES-CTX-002 — InvalidReadingContext

Meaning:

```text
Candidate ReadingContext violates Reading Session invariants.
```

Examples:

```text
invalid source identity
invalid reading mode
invalid context structure
unsupported context combination
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 26. SES-CTX-003 — ReadingContextBuildFailed

Meaning:

```text
Reading Session could not construct a valid Candidate
ReadingContext from an otherwise accepted command.
```

Severity:

```text
Error
```

Recovery:

```text
RetryOperation
```

Committed ReadingContext remains unchanged.

---

# 27. SES-CTX-004 — ReadingContextUnavailable

Meaning:

The previously committed ReadingContext cannot currently be retrieved safely.

Severity:

```text
Error
```

Recovery:

```text
ApplicationRecovery
```

This may indicate persistence/recovery impairment.

---

# 28. Session Configuration Errors

## SES-CONFIG-001 — InvalidSessionConfiguration

Meaning:

```text
Candidate SessionConfiguration violates
Reading Session configuration invariants.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 29. SES-CONFIG-002 — InvalidSessionOverride

Meaning:

```text
A temporary SessionOverride is invalid.
```

Examples:

```text
unsupported preference key
invalid value
override not permitted for that preference
incompatible override combination
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 30. SES-CONFIG-003 — SessionOverrideNotFound

Meaning:

```text
An operation attempted to remove or modify
a SessionOverride that does not exist.
```

Severity:

```text
Info
```

Recovery:

```text
RefreshSessionState
```

May be treated as idempotent `NoOp` where contract allows.

---

# 31. SES-CONFIG-004 — SessionConfigurationConflict

Meaning:

```text
The supplied SessionConfiguration authority/version
is stale relative to the current session.
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshSessionState
```

---

# 32. Preference Boundary

Reading Session may validate SessionOverride structure using Preferences contracts.

However errors such as:

```text
unknown persistent PreferenceKey
PreferenceSchema mismatch
invalid persistent Global preference
```

remain Preferences errors when the operation belongs to Preferences.

Reading Session should not duplicate Preferences error ownership unnecessarily.

---

# 33. Reading Context Revision Errors

## SES-REV-001 — ReadingContextRevisionConflict

Meaning:

```text
The supplied expected ReadingContextRevision
does not equal the current authoritative revision.
```

Example:

```text
expected = 14
current = 15
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshRevision
```

---

# 34. SES-REV-002 — ObsoleteReadingContextRevision

Meaning:

```text
The referenced ReadingContextRevision has been superseded.
```

Severity:

```text
Info
```

Recovery:

```text
RefreshRevision
```

---

# 35. SES-REV-003 — UnknownReadingContextRevision

Meaning:

```text
The requested ReadingContextRevision does not exist
for this Reading Session.
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshRevision
```

---

# 36. SES-REV-004 — InvalidReadingContextRevision

Meaning:

```text
ReadingContextRevision identity or provenance is invalid.
```

Severity:

```text
Error
```

Recovery:

```text
RefreshSessionState
```

---

# 37. Removed Generic Revision Errors

The v1 errors:

```text
DuplicateRevision
InvalidRevisionSequence
```

are not preferred public errors.

ReadingContextRevision is Reading Session-owned immutable authority.

Internal revision-generation defects should normally surface as:

```text
InvariantViolation
```

rather than exposing implementation sequencing details.

---

# 38. Revision Namespace Safety

Reading Session errors must distinguish:

```text
ReadingContextRevision
```

from:

```text
PreferenceRevision
RuntimeRevisionId
ConfigurationSnapshot version
Artifact provenance revision
```

`SES-REV-*` refers only to Reading Session-owned ReadingContextRevision unless explicitly stated otherwise.

---

# 39. Concurrency Errors

## SES-CONFLICT-001 — ConcurrentSessionMutation

Meaning:

```text
Another Reading Session mutation committed
before the current Candidate could commit.
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshSessionState
```

---

# 40. SES-CONFLICT-002 — StaleCommandAuthority

Meaning:

```text
A command was created against older session authority
and is no longer allowed to mutate current state.
```

Severity:

```text
Warning
```

Recovery:

```text
RefreshRevision
```

---

# 41. SES-CONFLICT-003 — DuplicateRequest

Meaning:

```text
The same idempotent request identity was already processed.
```

Severity:

```text
Info
```

Recovery:

```text
None
```

Where possible, return the previous logical result.

---

# 42. Persistence Errors

## SES-PERSIST-001 — SessionLoadFailed

Meaning:

```text
Reading Session-owned persisted state could not be loaded.
```

Severity:

```text
Error
```

Recovery:

```text
RetryOperation
or
ApplicationRecovery
```

---

# 43. SES-PERSIST-002 — SessionCommitFailed

Meaning:

```text
A Candidate Reading Session state could not be committed safely.
```

Severity:

```text
Error
```

Recovery:

```text
RetryOperation
```

Invariant:

```text
previous committed state remains authoritative
```

when failure is known to occur before commit.

---

# 44. SES-PERSIST-003 — SessionCommitOutcomeUnknown

Meaning:

```text
The persistence operation returned without a trustworthy
determination of whether the commit succeeded.
```

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

Do not blindly retry the mutation.

First reconcile authoritative state.

---

# 45. SES-PERSIST-004 — SessionStateCorrupted

Meaning:

```text
Persisted Reading Session-owned state violates
required invariants and cannot be trusted.
```

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

---

# 46. Schema Errors

## SES-SCHEMA-001 — UnsupportedContractVersion

Meaning:

```text
The caller uses an unsupported Reading Session contract version.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 47. SES-SCHEMA-002 — UnsupportedSessionSchemaVersion

Meaning:

```text
Persisted or supplied Reading Session data uses
an unsupported schema version.
```

Severity:

```text
Error
```

Recovery:

```text
ApplicationRecovery
```

---

# 48. SES-SCHEMA-003 — SessionMigrationFailed

Meaning:

```text
Reading Session-owned persisted state could not be
safely migrated to the supported schema.
```

Severity:

```text
Error
```

Recovery:

```text
ApplicationRecovery
```

Previous known-good state must be preserved where possible.

---

# 49. Security Errors

## SES-SEC-001 — SensitiveDataRejected

Meaning:

```text
A Reading Session command attempted to persist or expose
data forbidden by the session privacy contract.
```

Severity:

```text
Error
```

Recovery:

```text
CorrectRequest
```

---

# 50. SES-SEC-002 — UnsafeSessionMetadata

Meaning:

```text
Session metadata contains data that cannot safely
cross the Reading Session contract boundary.
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectRequest
```

---

# 51. Internal Errors

## SES-INT-001 — InternalFailure

Meaning:

```text
Unexpected Reading Session-owned internal failure.
```

Severity:

```text
Error
```

Recovery:

```text
RetryOperation
or
ApplicationRecovery
```

depending on whether authoritative state remains trustworthy.

---

# 52. SES-INT-002 — InvariantViolation

Meaning:

```text
A Reading Session architectural invariant was violated.
```

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

Examples:

```text
two authoritative current ReadingContext revisions
mutable committed revision
session identity changed after creation
Candidate exposed before commit
```

---

# 53. SES-INT-003 — CandidateStateLeak

Meaning:

```text
Uncommitted Candidate Reading Session state became externally observable.
```

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

---

# 54. SES-INT-004 — AuthorityInvariantViolation

Meaning:

```text
Reading Session authority boundaries became inconsistent.
```

Examples:

```text
RuntimeRevision treated as ReadingContextRevision
Preferences revision used as session authority
disposed session accepts mutation
```

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

---

# 55. Removed Pipeline Errors

The v1 Reading Session errors:

```text
PipelineAlreadyRunning
PipelineNotRunning
PipelineRestartFailed
PipelineCancelled
StageSchedulingFailed
```

are removed.

Reason:

Reading Session no longer owns Runtime pipeline execution.

---

# 56. Pipeline Failure Ownership

Failures such as:

```text
WorkItem scheduling failed
Attempt timed out
stage execution failed
pipeline superseded
retry exhausted
```

belong to:

```text
Pipeline Runtime
Business Pipeline Orchestration
Scheduler
processing module
```

depending on the specific invariant.

---

# 57. Removed SchedulerFailure

The v1:

```text
SES-INT-003 SchedulerFailure
```

is removed.

Scheduler is infrastructure/Runtime-owned.

Reading Session should not expose Scheduler implementation failure as its own internal domain error.

---

# 58. Removed Resource Errors

The v1 errors:

```text
MemoryLimitExceeded
Timeout
TooManySessions
```

are removed from the generic Reading Session error set.

Their ownership depends on the failed resource boundary.

---

# 59. Memory Limit Ownership

Example:

```text
OCR process exceeds memory
```

belongs to Recognition/Runtime/resource infrastructure.

If Reading Session itself has a product-level limit such as:

```text
maximum simultaneously retained session records
```

a separate explicit session capacity contract may define an appropriate Reading Session error later.

Do not reuse generic execution resource errors.

---

# 60. Timeout Ownership

A processing timeout is not a Reading Session timeout.

Example:

```text
Translation Attempt timeout
```

belongs to Runtime/Translation execution.

A future Reading Session-specific operation timeout may be defined separately if the session command itself has such a contract.

---

# 61. No Runtime Retry Errors

Reading Session must not expose:

```text
RetryExhausted
AttemptFailed
RuntimeTimeout
RuntimeCancelled
WorkItemSuperseded
```

as `SES-*` errors.

---

# 62. No Provider Errors

Reading Session must not expose:

```text
ProviderUnavailable
ProviderAuthenticationFailed
ProviderRateLimited
ProviderTimeout
```

as session errors.

---

# 63. No Processing Errors

Reading Session must not translate:

```text
CaptureError
RecognitionError
TextProcessingError
TranslationError
PresentationError
```

into generic session failure.

---

# 64. External Failure Observation

Reading Session/Application may observe an external failure for UX or history.

Observation does not change error ownership.

Example:

```text
Translation Attempt failed
        ↓
Runtime Translation error
        ↓
Application displays failure
```

Do not rewrite it as:

```text
SES-SESSION-xxx
```

---

# 65. Error-to-State Mapping

Errors do not automatically force lifecycle transitions.

Recommended mapping:

| Error Class                   | Session State Effect                              |
| ----------------------------- | ------------------------------------------------- |
| Invalid request               | None                                              |
| Session not found             | None                                              |
| Invalid transition            | None                                              |
| Invalid SessionOverride       | None                                              |
| Revision conflict             | None                                              |
| Concurrent mutation           | None                                              |
| Safe commit failure           | Previous committed state retained                 |
| Schema incompatibility        | Session may become unavailable                    |
| Corrupted authoritative state | Recovery/unavailable path                         |
| Invariant violation           | Recovery/unavailable path                         |
| External Runtime failure      | No automatic Reading Session lifecycle transition |

---

# 66. No Universal `Failed` Transition

The v1 invariant:

```text
Internal failure
    ↓
Failed state
```

is removed.

Reason:

not every internal operation failure invalidates the Reading Session.

Example:

```text
Candidate build fails
        ↓
Candidate discarded
        ↓
committed session remains valid
```

---

# 67. Commit Failure Semantics

Known pre-commit failure:

```text
Candidate
    ↓
commit fails safely
    ↓
Candidate discarded
    ↓
previous committed state remains authoritative
```

Unknown commit outcome:

```text
commit requested
    ↓
outcome uncertain
    ↓
do not blindly retry
    ↓
reconcile persisted authority
```

---

# 68. Error and Revision Relationship

An error may include:

```text
readingContextRevision
expectedRevision
currentRevision
```

when relevant.

It must not include unrelated revision identifiers under ambiguous names such as:

```text
revision
processingRevision
```

without explicit type.

---

# 69. ReadingContextRevisionConflict Example

```text
Current ReadingContextRevision = 18

UpdateReadingContext
expectedRevision = 17
        ↓
SES-REV-001
ReadingContextRevisionConflict
        ↓
currentRevision = 18
        ↓
no mutation
```

---

# 70. SessionOverride Error Example

```text
SetSessionOverride
key = translation.target_language
value = invalid-value
        ↓
validation
        ↓
SES-CONFIG-002
InvalidSessionOverride
        ↓
existing SessionConfiguration unchanged
```

---

# 71. Persistence Example

```text
ReadingContextRevision 10
        ↓
Build Candidate Revision 11
        ↓
persistence fails before commit
        ↓
SES-PERSIST-002
        ↓
Revision 10 remains authoritative
```

---

# 72. Unknown Commit Example

```text
Revision 10
        ↓
commit Candidate 11
        ↓
storage connection lost
commit outcome unknown
        ↓
SES-PERSIST-003
        ↓
reconcile storage
```

Do not immediately create Candidate 12.

---

# 73. Runtime Failure Example

```text
Reading Session S1
ReadingContextRevision 7
        ↓
Runtime Attempt A12
        ↓
Translation timeout
```

Result:

```text
Runtime/Translation error
```

Reading Session remains:

```text
S1
ReadingContextRevision 7
```

unless a separate session-owned command changes it.

---

# 74. Supersession Example

```text
ReadingContextRevision 7
        ↓
user changes reading context
        ↓
ReadingContextRevision 8
```

Runtime may later classify work for revision 7 as obsolete/superseded.

That is not:

```text
SES-REV-002
```

unless a caller explicitly attempts a Reading Session operation against obsolete session authority.

---

# 75. Error Idempotency

Repeated equivalent rejected commands should not mutate Reading Session state.

Duplicate idempotent request identities may return:

```text
previous logical result
```

where available.

---

# 76. Error Logging

Safe logs may include:

```text
ErrorCode
SessionId
SessionState
ReadingContextRevision
ExpectedRevision
CurrentRevision
RequestId
CorrelationId
OperationType
Timestamp
DiagnosticRef
```

---

# 77. Removed Ambiguous Logging Fields

Avoid generic:

```text
ProcessingRevision
PipelineStage
```

in Reading Session error logs unless they are explicitly external correlation metadata.

Reading Session should not imply ownership of Runtime processing identifiers.

---

# 78. Privacy Logging Rules

Logs must not contain raw:

```text
captured images
OCR text
translated text
document contents
credentials
tokens
private provider payloads
```

Source metadata should be minimized/redacted according to privacy policy.

---

# 79. Metrics

Recommended:

```text
reading_session_error_total
reading_session_request_rejected_total
reading_session_revision_conflict_total
reading_session_concurrent_mutation_total
reading_session_commit_failure_total
reading_session_commit_unknown_total
reading_session_schema_error_total
reading_session_invariant_violation_total
```

Optional dimensions:

```text
error_code
operation
session_state
recovery_hint
```

Avoid high-cardinality SessionId labels.

---

# 80. Removed Metrics

The v1 metrics:

```text
reading_session_restart_total
reading_session_cancel_total
reading_session_scheduler_failure_total
```

are not core Reading Session error metrics unless corresponding ownership is explicitly reintroduced elsewhere.

---

# 81. Error Publication Policy

Normal Reading Session errors are command/query results and diagnostics.

They are not automatically Event Bus domain events.

Example:

```text
InvalidSessionOverride
```

should normally return an error result, not publish:

```text
SessionOverrideValidationFailed
```

to the global Event Bus.

---

# 82. Domain Events After Errors

A rejected operation emits no success state-change event.

Example:

```text
UpdateReadingContext
        ↓
SES-REV-001
```

must not publish:

```text
ReadingContextChanged
```

---

# 83. Event Publication Failure

If Reading Session state commits successfully but its corresponding event publication fails:

```text
new committed Reading Session state remains authoritative
```

Do not roll back valid state solely because publication failed.

Publication recovery belongs to infrastructure.

---

# 84. Event Publication Error Ownership

Event Bus publication failure is primarily infrastructure-owned.

Reading Session may expose a safe coordination/diagnostic error when required, but must not misreport the already committed domain mutation as failed.

---

# 85. Security Invariants

Errors must never allow callers to infer protected data through:

```text
raw rejected values
full source paths
document text
provider secrets
credential identifiers beyond safe references
```

---

# 86. Public vs Diagnostic Error

Public:

```text
SES-PERSIST-002
SessionCommitFailed
```

Internal diagnostic:

```text
SQLite transaction error
filesystem error
serialization stack trace
```

The diagnostic detail should be referenced through:

```text
diagnosticRef
```

rather than exposed directly.

---

# 87. Compatibility

Adding a new error code is generally backward-compatible.

Changing the semantic meaning of an existing code is not.

Removing a public error code requires:

```text
deprecation
migration guidance
major contract change
```

where applicable.

---

# 88. Architecture Invariants

1. Reading Session errors describe Reading Session-owned failures only.

2. Runtime execution errors are not converted into `SES-*` errors.

3. Processing module errors retain their original ownership.

4. Scheduler errors are not Reading Session internal errors.

5. Provider failures are not Reading Session errors.

6. SessionOverride errors belong to Reading Session when the override operation is session-owned.

7. Persistent Global/Source preference errors remain Preferences-owned.

8. ReadingContextRevision is distinct from PreferenceRevision.

9. ReadingContextRevision is distinct from RuntimeRevisionId.

10. Failed Candidates never replace committed Reading Session state.

11. Safe pre-commit failure preserves previous authoritative state.

12. Unknown commit outcome requires reconciliation.

13. Revision conflict does not mutate state.

14. Invalid request does not mutate state.

15. Invalid SessionOverride does not mutate state.

16. External Runtime failure does not automatically transition Reading Session lifecycle.

17. Internal failure does not universally imply a `Failed` session state.

18. Error codes are stable.

19. Error payloads remain privacy-safe.

20. Error events are not required for normal command rejection.

21. Success events are published only after successful commit.

22. Event publication failure does not roll back committed session state.

23. Public error contracts remain serializable.

24. Implementation exceptions do not cross the public boundary directly.

---

# 89. Testing — Request Errors

Verify:

```text
missing SessionId
invalid SessionId
unsupported operation
missing required field
malformed payload
```

produce stable request errors without mutation.

---

# 90. Testing — Lifecycle Errors

Verify invalid lifecycle transitions:

```text
are rejected
preserve committed state
do not create new ReadingContextRevision
do not emit success events
```

---

# 91. Testing — SessionOverride Errors

Verify:

```text
invalid key
invalid value
unsupported override
conflicting override
```

do not mutate SessionConfiguration.

---

# 92. Testing — Revision Conflicts

Verify:

```text
expected != current
```

returns:

```text
SES-REV-001
```

with safe:

```text
expectedRevision
currentRevision
```

metadata.

---

# 93. Testing — Candidate Isolation

Inject failure during Candidate construction.

Verify:

```text
Candidate discarded
committed session unchanged
no success event
```

---

# 94. Testing — Persistence

Inject:

```text
known pre-commit failure
unknown commit outcome
corrupt persisted state
```

and verify distinct errors and recovery behavior.

---

# 95. Testing — Runtime Independence

Inject:

```text
Capture failure
Recognition timeout
Translation provider failure
Runtime cancellation
Retry exhaustion
Presentation failure
```

Verify none is rewritten as a Reading Session domain error.

---

# 96. Testing — Internal Failure

Verify an internal failure that leaves authoritative state intact:

```text
does not automatically destroy the session
```

Only invariant/authority corruption should enter recovery/unavailable handling.

---

# 97. Testing — Privacy

Verify errors/logs never contain:

```text
image bytes
OCR text
translation text
secret values
credentials
unsafe source content
```

---

# 98. Testing — Event Failure

Inject event publication failure after successful session commit.

Verify:

```text
committed state remains authoritative
event recovery is external
command is not blindly rerun
```

---

# 99. Deprecated v1 Errors

Removed from Reading Session ownership:

```text
SES-PIPE-001 PipelineAlreadyRunning
SES-PIPE-002 PipelineNotRunning
SES-PIPE-003 PipelineRestartFailed
SES-PIPE-004 PipelineCancelled
SES-PIPE-005 StageSchedulingFailed

SES-RES-001 MemoryLimitExceeded
SES-RES-002 Timeout
SES-RES-003 TooManySessions

SES-INT-003 SchedulerFailure
```

Reworked:

```text
SES-REV-001 RevisionMismatch
    →
ReadingContextRevisionConflict
```

Obsolete lifecycle-specific errors are replaced by:

```text
InvalidStateTransition
OperationNotAllowedInCurrentState
SessionTransitionConflict
```

---

# 100. Related Documents

```text
doc/02-modules/reading-session/MODULE.md
doc/02-modules/reading-session/CONTRACT.md
doc/02-modules/reading-session/STATES.md
doc/02-modules/reading-session/EVENTS.md
doc/02-modules/reading-session/README.md

doc/02-modules/preferences/
doc/02-modules/capture/
doc/02-modules/recognition/
doc/02-modules/text-processing/
doc/02-modules/translation/
doc/02-modules/presentation/

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/03-infrastructure/storage/
doc/03-infrastructure/scheduler/
```

---

# 101. Completion Criteria

This specification is synchronized when:

* Reading Session no longer owns pipeline execution errors;
* SchedulerFailure is removed from Reading Session;
* generic Runtime resource errors are removed;
* ReadingContextRevision terminology is explicit;
* PreferenceRevision and RuntimeRevisionId remain distinct;
* SessionOverride validation errors remain session-owned;
* persistent Preferences errors remain Preferences-owned;
* failed Candidate state never replaces committed state;
* safe commit failure and unknown commit outcome are distinguished;
* internal failure does not universally force a Failed state;
* Runtime/processing failures remain external;
* retry/restart execution semantics remain outside Reading Session;
* privacy rules cover error payloads and diagnostics;
* event publication failure preserves committed state.

---

# 102. Summary

Reading Session error handling follows:

```text
Session Command
      ↓
Validate
      ↓
Build Candidate
      ↓
Commit
```

On safe failure:

```text
Error
      ↓
Candidate discarded
      ↓
Previous committed
Reading Session state preserved
```

Revision conflict:

```text
Expected ReadingContextRevision
        ≠
Current ReadingContextRevision
        ↓
ReadingContextRevisionConflict
        ↓
No mutation
```

Runtime failure:

```text
Runtime Attempt
      ↓
Processing failure
      ↓
Runtime / Processing Error
```

not:

```text
Reading Session Failed
```

The central ownership rule is:

```text
Reading Session owns
session lifecycle,
session configuration,
ReadingContext,
and ReadingContextRevision errors.

Runtime owns execution errors.

Processing modules own processing errors.

Preferences owns persistent preference errors.
```
