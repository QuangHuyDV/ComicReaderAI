# Capture Module Errors

- Module: Capture
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines all error contracts produced by the Capture Module.

The Capture Module is responsible only for acquiring raw content.

It does not perform:

- OCR
- Translation
- Image enhancement
- Text processing
- Presentation

---

# Error Principles

## Stable Error Codes

Consumers must depend on `ErrorCode`, not error messages.

Error messages are intended only for diagnostics.

---

## Fail Fast

Invalid capture requests should be rejected before allocating resources.

---

## Preserve Previous Session

Capture failures must not corrupt an active reading session.

---

## Privacy

Errors must never expose:

- image content
- screenshots
- browser page content
- user credentials
- cookies

---

# Error Code Format

```text
CAP-<CATEGORY>-<NUMBER>
```

Examples:

```text
CAP-REQ-001
CAP-SRC-002
CAP-IMG-001
CAP-DEV-003
```

---

# Severity Levels

| Severity | Meaning |
|----------|---------|
| Info | Expected condition |
| Warning | Invalid request |
| Error | Operation failed |
| Critical | Internal invariant broken |

---

# Retry Policies

| Policy | Meaning |
|---------|---------|
| Never | Retry will not help |
| AfterCorrection | Retry after fixing input |
| Transient | Retry later |
| ResetRequired | Reset module before retry |

---

# Error Categories

| Prefix | Category |
|---------|----------|
| REQ | Request Validation |
| SRC | Capture Source |
| IMG | Image Acquisition |
| DEV | Device |
| PERM | Permission |
| STATE | Module State |
| RES | Resources |
| INT | Internal |

---

# Request Errors

## CAP-REQ-001 MissingCaptureTarget

Meaning

No capture target was specified.

Severity

Warning

Retry

AfterCorrection

---

## CAP-REQ-002 InvalidCaptureTarget

Meaning

Capture target is invalid.

Examples

- invalid browser tab
- invalid monitor
- invalid window
- unsupported source

Severity

Warning

Retry

AfterCorrection

---

## CAP-REQ-003 UnsupportedCaptureMode

Meaning

Requested capture mode is not supported.

Examples

- video stream
- protected content
- unsupported browser

Severity

Warning

Retry

AfterCorrection

---

# Source Errors

## CAP-SRC-001 SourceUnavailable

Meaning

Capture source no longer exists.

Examples

- closed browser tab
- closed application
- disconnected monitor

Severity

Warning

Retry

Transient

---

## CAP-SRC-002 SourceBusy

Meaning

Source is temporarily unavailable.

Examples

- exclusive screen access
- operating system restriction

Severity

Warning

Retry

Transient

---

## CAP-SRC-003 SourceChanged

Meaning

The capture source changed while acquisition was running.

Examples

- navigation
- page reload
- window replacement

Severity

Info

Retry

Transient

---

# Image Errors

## CAP-IMG-001 EmptyFrame

Meaning

Captured image contains no usable pixels.

Severity

Warning

Retry

Transient

---

## CAP-IMG-002 InvalidImage

Meaning

Captured image is corrupted.

Severity

Error

Retry

Transient

---

## CAP-IMG-003 UnsupportedImageFormat

Meaning

Image format cannot be processed.

Severity

Warning

Retry

AfterCorrection

---

## CAP-IMG-004 ImageTooLarge

Meaning

Captured image exceeds configured limits.

Severity

Error

Retry

AfterCorrection

Recovery

Split image into smaller regions.

---

## CAP-IMG-005 ImageTooSmall

Meaning

Captured image is too small for OCR.

Severity

Warning

Retry

AfterCorrection

---

# Device Errors

## CAP-DEV-001 DeviceUnavailable

Meaning

Requested device cannot be accessed.

Examples

- monitor disconnected
- webcam unavailable

Severity

Error

Retry

Transient

---

## CAP-DEV-002 DeviceLost

Meaning

Device became unavailable during capture.

Severity

Warning

Retry

Transient

---

## CAP-DEV-003 UnsupportedDevice

Meaning

Device type is unsupported.

Severity

Warning

Retry

Never

---

# Permission Errors

## CAP-PERM-001 PermissionDenied

Meaning

Capture permission was denied.

Examples

- browser permission
- operating system permission

Severity

Error

Retry

AfterCorrection

---

## CAP-PERM-002 PermissionRevoked

Meaning

Permission was removed during capture.

Severity

Warning

Retry

AfterCorrection

---

# State Errors

## CAP-STATE-001 CaptureAlreadyRunning

Meaning

A capture operation is already active.

Severity

Info

Retry

Transient

---

## CAP-STATE-002 CaptureNotStarted

Meaning

Capture operation has not been started.

Severity

Warning

Retry

AfterCorrection

---

## CAP-STATE-003 InvalidStateTransition

Meaning

Requested operation is not allowed in the current state.

Severity

Warning

Retry

AfterCorrection

---

# Resource Errors

## CAP-RES-001 MemoryLimitExceeded

Meaning

Capture exceeded memory budget.

Severity

Error

Retry

Transient

---

## CAP-RES-002 Timeout

Meaning

Capture operation exceeded time limit.

Severity

Warning

Retry

Transient

---

## CAP-RES-003 TooManyRequests

Meaning

Capture requests exceed configured rate.

Severity

Warning

Retry

Transient

---

# Internal Errors

## CAP-INT-001 InternalFailure

Meaning

Unexpected internal failure.

Severity

Critical

Retry

ResetRequired

---

## CAP-INT-002 InvariantViolation

Meaning

Capture module invariant was violated.

Severity

Critical

Retry

ResetRequired

---

## CAP-INT-003 AtomicCommitFailed

Meaning

Capture result could not be committed safely.

Severity

Critical

Retry

ResetRequired

---

# Error to State Mapping

| Error | Result |
|--------|--------|
| Validation | Reject request |
| Source unavailable | Retry later |
| Permission denied | Wait for permission |
| Invalid image | Reject frame |
| Timeout | Retry |
| Internal failure | Failed state |

---

# Logging Rules

Logs should include:

- ErrorCode
- OperationId
- SessionId
- CaptureSource
- Duration
- Timestamp

Logs must not include:

- image data
- screenshots
- browser page content
- cookies
- authentication information

---

# Metrics

Recommended metrics

```text
capture_error_total
capture_timeout_total
capture_permission_denied_total
capture_source_unavailable_total
capture_invalid_image_total
capture_memory_limit_total
capture_internal_failure_total
```

---

# Architecture Invariants

The Capture Module must guarantee:

1. Failed captures never produce partial output.
2. Capture never modifies the original source.
3. Invalid frames are never published.
4. Image data is immutable after capture.
5. Capture failures never terminate an active reading session.
6. Internal failures transition the module to `Failed`.

---

# Completion Criteria

This document is complete when:

- Every failure maps to a stable error code.
- Retry behavior is defined.
- State transitions are deterministic.
- Sensitive information is never exposed.
- Internal failures are distinguishable from validation failures.
- Error contracts remain backward compatible.