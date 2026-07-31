# Reading Session Module Errors

- Module: Reading Session
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines all error contracts produced by the Reading Session Module.

The Reading Session Module is responsible for coordinating the lifecycle of a reading session and orchestrating the processing pipeline.

It does not perform:

- Screen Capture
- OCR Recognition
- Text Processing
- Translation
- Presentation Rendering

---

# Error Principles

## Stable Error Codes

Consumers must depend on `ErrorCode` rather than implementation-specific exceptions.

---

## Session Integrity

Errors must never corrupt an existing Reading Session.

---

## Revision Safety

A failed revision must never replace a successful revision.

---

## Privacy

Errors must never expose:

- captured images
- OCR content
- translated text
- browser information
- authentication credentials

---

# Error Code Format

```text
SES-<CATEGORY>-<NUMBER>
```

Examples

```text
SES-REQ-001
SES-STATE-002
SES-REV-001
SES-INT-001
```

---

# Severity Levels

| Severity | Meaning |
|----------|---------|
| Info | Expected condition |
| Warning | Invalid operation |
| Error | Session operation failed |
| Critical | Internal invariant broken |

---

# Retry Policies

| Policy | Meaning |
|---------|---------|
| Never | Retry will not help |
| AfterCorrection | Retry after correcting the request |
| Transient | Retry later |
| RestartSession | Restart the current session |
| ResetRequired | Reset the module |

---

# Error Categories

| Prefix | Category |
|---------|----------|
| REQ | Request Validation |
| SESSION | Session |
| REV | Revision |
| PIPE | Pipeline |
| STATE | Module State |
| RES | Resources |
| INT | Internal |

---

# Request Errors

## SES-REQ-001 MissingSessionIdentifier

Meaning

The requested session identifier is missing.

Severity

Warning

Retry

AfterCorrection

---

## SES-REQ-002 InvalidSessionIdentifier

Meaning

The session identifier is invalid.

Severity

Warning

Retry

AfterCorrection

---

## SES-REQ-003 UnsupportedOperation

Meaning

The requested operation is not supported.

Severity

Warning

Retry

Never

---

# Session Errors

## SES-SESSION-001 SessionNotFound

Meaning

The requested session does not exist.

Severity

Warning

Retry

AfterCorrection

---

## SES-SESSION-002 SessionAlreadyExists

Meaning

The session already exists.

Severity

Warning

Retry

Never

---

## SES-SESSION-003 SessionAlreadyRunning

Meaning

The session is already running.

Severity

Info

Retry

Never

---

## SES-SESSION-004 SessionAlreadyCompleted

Meaning

The session has already completed.

Severity

Info

Retry

RestartSession

---

## SES-SESSION-005 SessionAlreadyCancelled

Meaning

The session has already been cancelled.

Severity

Info

Retry

RestartSession

---

# Revision Errors

## SES-REV-001 RevisionMismatch

Meaning

The supplied revision is not the current revision.

Severity

Warning

Retry

RestartSession

---

## SES-REV-002 ObsoleteRevision

Meaning

The revision is obsolete.

Severity

Info

Retry

Never

---

## SES-REV-003 DuplicateRevision

Meaning

The revision already exists.

Severity

Warning

Retry

Never

---

## SES-REV-004 InvalidRevisionSequence

Meaning

Revision ordering is invalid.

Severity

Error

Retry

RestartSession

---

# Pipeline Errors

## SES-PIPE-001 PipelineAlreadyRunning

Meaning

A processing pipeline is already active.

Severity

Info

Retry

Never

---

## SES-PIPE-002 PipelineNotRunning

Meaning

No active processing pipeline exists.

Severity

Warning

Retry

AfterCorrection

---

## SES-PIPE-003 PipelineRestartFailed

Meaning

The processing pipeline could not be restarted.

Severity

Error

Retry

RestartSession

---

## SES-PIPE-004 PipelineCancelled

Meaning

The pipeline was cancelled before completion.

Severity

Info

Retry

RestartSession

---

## SES-PIPE-005 StageSchedulingFailed

Meaning

A downstream processing stage could not be scheduled.

Severity

Error

Retry

Transient

---

# State Errors

## SES-STATE-001 InvalidStateTransition

Meaning

The requested state transition is not allowed.

Severity

Warning

Retry

AfterCorrection

---

## SES-STATE-002 SessionNotRunning

Meaning

The operation requires an active session.

Severity

Warning

Retry

AfterCorrection

---

## SES-STATE-003 SessionPaused

Meaning

The session is currently paused.

Severity

Info

Retry

AfterCorrection

---

## SES-STATE-004 SessionFailed

Meaning

The session entered the Failed state.

Severity

Error

Retry

RestartSession

---

# Resource Errors

## SES-RES-001 MemoryLimitExceeded

Meaning

The module exceeded its memory budget.

Severity

Error

Retry

Transient

---

## SES-RES-002 Timeout

Meaning

The session exceeded the configured timeout.

Severity

Warning

Retry

Transient

---

## SES-RES-003 TooManySessions

Meaning

The number of concurrent sessions exceeded the configured limit.

Severity

Error

Retry

Transient

---

# Internal Errors

## SES-INT-001 InternalFailure

Meaning

Unexpected internal failure.

Severity

Critical

Retry

ResetRequired

---

## SES-INT-002 InvariantViolation

Meaning

A Reading Session invariant was violated.

Severity

Critical

Retry

ResetRequired

---

## SES-INT-003 SchedulerFailure

Meaning

The internal scheduler failed unexpectedly.

Severity

Critical

Retry

ResetRequired

---

## SES-INT-004 AtomicCommitFailed

Meaning

The session state could not be committed safely.

Severity

Critical

Retry

ResetRequired

---

# Error to State Mapping

| Error | Result |
|--------|--------|
| Invalid request | Reject request |
| Session not found | Reject operation |
| Revision mismatch | Ignore obsolete work |
| Scheduling failure | Retry stage |
| Timeout | Retry |
| Internal failure | Failed state |

---

# Logging Rules

Logs should include:

- ErrorCode
- SessionId
- SessionRevision
- ProcessingRevision
- OperationId
- PipelineStage
- Timestamp

Logs must not include:

- image content
- OCR text
- translated text
- browser information
- credentials

---

# Metrics

Recommended metrics

```text
reading_session_error_total
reading_session_timeout_total
reading_session_restart_total
reading_session_cancel_total
reading_session_revision_conflict_total
reading_session_scheduler_failure_total
reading_session_internal_failure_total
```

---

# Architecture Invariants

The Reading Session Module must guarantee:

1. A failed operation never corrupts the active session.
2. Obsolete revisions are never scheduled.
3. Every active pipeline belongs to exactly one session.
4. Only one active revision exists for a session.
5. Session state transitions are deterministic.
6. Internal failures always transition the module to `Failed`.

---

# Completion Criteria

This document is complete when:

- Every session failure maps to a stable error code.
- Retry behavior is clearly defined.
- Revision conflicts are handled consistently.
- Privacy requirements are enforced.
- Internal failures are distinguishable from operational failures.
- Error contracts remain backward compatible.