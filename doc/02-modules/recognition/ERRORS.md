# Recognition Module Errors

- Module: Recognition
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines all error contracts produced by the Recognition Module.

The Recognition Module is responsible for recognizing textual and structural information from captured images.

It does not perform:

- Screen capture
- Translation
- Text formatting
- Presentation
- Data persistence

---

# Error Principles

## Stable Error Codes

Consumers must rely on `ErrorCode` rather than implementation-specific exceptions.

---

## Recognition Never Guesses

When confidence is below the configured threshold, Recognition should report uncertainty instead of producing unreliable results.

---

## Preserve Input

Recognition failures must never modify the captured image.

---

## Privacy

Errors must never expose:

- captured images
- browser contents
- personal information
- API credentials

---

# Error Code Format

```text
REC-<CATEGORY>-<NUMBER>
```

Examples

```text
REC-REQ-001
REC-IMG-002
REC-OCR-003
REC-MODEL-001
```

---

# Severity Levels

| Severity | Meaning |
|----------|---------|
| Info | Expected condition |
| Warning | Recognition incomplete |
| Error | Recognition failed |
| Critical | Internal invariant broken |

---

# Retry Policies

| Policy | Meaning |
|---------|---------|
| Never | Retry will not help |
| AfterCorrection | Retry after correcting input |
| Transient | Retry later |
| WithFallback | Retry using another recognition strategy |
| ResetRequired | Reset module before retry |

---

# Error Categories

| Prefix | Category |
|---------|----------|
| REQ | Request Validation |
| IMG | Image Validation |
| OCR | OCR Processing |
| MODEL | Recognition Model |
| LANG | Language Detection |
| REGION | Region Detection |
| STATE | Module State |
| RES | Resources |
| INT | Internal |

---

# Request Errors

## REC-REQ-001 MissingImage

Meaning

No image was provided.

Severity

Warning

Retry

AfterCorrection

---

## REC-REQ-002 InvalidImage

Meaning

Image cannot be decoded.

Severity

Warning

Retry

AfterCorrection

---

## REC-REQ-003 UnsupportedImageFormat

Meaning

Image format is unsupported.

Severity

Warning

Retry

AfterCorrection

---

# Image Errors

## REC-IMG-001 ImageTooSmall

Meaning

Image resolution is insufficient for OCR.

Severity

Warning

Retry

AfterCorrection

---

## REC-IMG-002 ImageTooLarge

Meaning

Image exceeds configured processing limits.

Severity

Error

Retry

AfterCorrection

Recovery

Split image into multiple regions.

---

## REC-IMG-003 LowImageQuality

Meaning

Image quality is insufficient.

Examples

- blurred
- noisy
- compressed
- low contrast

Severity

Warning

Retry

AfterCorrection

---

## REC-IMG-004 EmptyImage

Meaning

No visible content exists.

Severity

Warning

Retry

Never

---

# OCR Errors

## REC-OCR-001 NoTextDetected

Meaning

No text was detected.

Severity

Info

Retry

Never

---

## REC-OCR-002 LowConfidence

Meaning

Recognition confidence is below the configured threshold.

Severity

Warning

Retry

WithFallback

Recovery

Try another OCR engine or preprocessing pipeline.

---

## REC-OCR-003 TextExtractionFailed

Meaning

OCR process failed.

Severity

Error

Retry

Transient

---

## REC-OCR-004 InvalidRecognitionResult

Meaning

OCR output is malformed.

Severity

Error

Retry

WithFallback

---

## REC-OCR-005 ReadingOrderUnknown

Meaning

Reading order cannot be determined.

Severity

Warning

Retry

WithFallback

---

# Model Errors

## REC-MODEL-001 ModelUnavailable

Meaning

Recognition model cannot be loaded.

Severity

Error

Retry

Transient

---

## REC-MODEL-002 ModelInitializationFailed

Meaning

Recognition model failed during initialization.

Severity

Error

Retry

Transient

---

## REC-MODEL-003 UnsupportedModel

Meaning

Configured recognition model is unsupported.

Severity

Warning

Retry

AfterCorrection

---

## REC-MODEL-004 ModelExecutionFailed

Meaning

Recognition model failed during inference.

Severity

Error

Retry

Transient

---

# Language Errors

## REC-LANG-001 LanguageNotDetected

Meaning

Unable to determine source language.

Severity

Warning

Retry

WithFallback

Recovery

Use configured default language.

---

## REC-LANG-002 UnsupportedLanguage

Meaning

Detected language is unsupported.

Severity

Warning

Retry

Never

---

# Region Errors

## REC-REGION-001 RegionNotFound

Meaning

Expected text region cannot be located.

Severity

Warning

Retry

AfterCorrection

---

## REC-REGION-002 InvalidRegion

Meaning

Detected region is invalid.

Severity

Warning

Retry

AfterCorrection

---

## REC-REGION-003 OverlappingRegions

Meaning

Detected regions overlap beyond configured limits.

Severity

Warning

Retry

WithFallback

---

## REC-REGION-004 RegionSegmentationFailed

Meaning

Text regions cannot be segmented correctly.

Severity

Error

Retry

WithFallback

---

# State Errors

## REC-STATE-001 RecognitionAlreadyRunning

Meaning

Recognition is already running.

Severity

Info

Retry

Transient

---

## REC-STATE-002 RecognitionNotStarted

Meaning

Recognition has not started.

Severity

Warning

Retry

AfterCorrection

---

## REC-STATE-003 InvalidStateTransition

Meaning

Operation is not allowed in the current state.

Severity

Warning

Retry

AfterCorrection

---

# Resource Errors

## REC-RES-001 MemoryLimitExceeded

Meaning

Recognition exceeded memory budget.

Severity

Error

Retry

Transient

---

## REC-RES-002 Timeout

Meaning

Recognition exceeded configured timeout.

Severity

Warning

Retry

Transient

---

## REC-RES-003 GPUUnavailable

Meaning

Configured GPU is unavailable.

Severity

Warning

Retry

WithFallback

Recovery

Run using CPU.

---

## REC-RES-004 QueueOverflow

Meaning

Recognition queue exceeded configured capacity.

Severity

Warning

Retry

Transient

---

# Internal Errors

## REC-INT-001 InternalFailure

Meaning

Unexpected internal failure.

Severity

Critical

Retry

ResetRequired

---

## REC-INT-002 InvariantViolation

Meaning

Recognition module invariant was violated.

Severity

Critical

Retry

ResetRequired

---

## REC-INT-003 AtomicCommitFailed

Meaning

Recognition result could not be committed safely.

Severity

Critical

Retry

ResetRequired

---

# Error to State Mapping

| Error | Result |
|--------|--------|
| Invalid request | Reject request |
| Invalid image | Reject input |
| Low confidence | Use fallback |
| OCR failure | Retry |
| Model failure | Retry or fallback |
| Internal failure | Failed state |

---

# Logging Rules

Logs should include:

- ErrorCode
- SessionId
- OperationId
- ImageId
- OCR Engine
- Duration
- Timestamp

Logs must not include:

- image data
- recognized text
- browser contents
- personal information
- authentication data

---

# Metrics

Recommended metrics

```text
recognition_error_total
recognition_timeout_total
recognition_low_confidence_total
recognition_no_text_total
recognition_model_failure_total
recognition_gpu_unavailable_total
recognition_memory_limit_total
recognition_internal_failure_total
```

---

# Architecture Invariants

The Recognition Module must guarantee:

1. Source image is never modified.
2. Recognition output is immutable after publication.
3. Low-confidence results are explicitly marked.
4. Invalid OCR output is never published.
5. Recognition failures never corrupt capture results.
6. Internal failures transition the module to `Failed`.

---

# Completion Criteria

This document is complete when:

- Every recognition failure has a stable error code.
- Retry behavior is defined.
- Fallback behavior is specified.
- Privacy requirements are enforced.
- Internal failures are distinguishable from operational failures.
- Error contracts remain backward compatible.