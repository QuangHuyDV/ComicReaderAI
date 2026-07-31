# Text Processing Module Errors

- Module: Text Processing
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines all error contracts produced by the Text Processing Module.

The Text Processing Module is responsible for transforming OCR output into normalized, structured text suitable for translation.

It does not perform:

- Screen capture
- OCR
- Translation
- Presentation
- Data persistence

---

# Error Principles

## Stable Error Codes

Consumers must depend on `ErrorCode` instead of implementation-specific exceptions.

---

## Preserve Original OCR Result

Text Processing must never modify the original OCR output.

Every transformation produces a new processed result.

---

## Deterministic Processing

The same input must always produce the same output.

---

## Privacy

Errors must never expose:

- original page images
- browser contents
- API credentials
- user information

---

# Error Code Format

```text
TXT-<CATEGORY>-<NUMBER>
```

Examples

```text
TXT-REQ-001
TXT-NORM-002
TXT-SEG-003
TXT-INT-001
```

---

# Severity Levels

| Severity | Meaning |
|----------|---------|
| Info | Expected condition |
| Warning | Processing incomplete |
| Error | Processing failed |
| Critical | Internal invariant broken |

---

# Retry Policies

| Policy | Meaning |
|---------|---------|
| Never | Retry will not help |
| AfterCorrection | Retry after correcting input |
| Transient | Retry later |
| WithFallback | Retry using another strategy |
| ResetRequired | Reset module before retry |

---

# Error Categories

| Prefix | Category |
|---------|----------|
| REQ | Request Validation |
| NORM | Text Normalization |
| SEG | Segmentation |
| LANG | Language Processing |
| FORMAT | Text Formatting |
| STATE | Module State |
| RES | Resources |
| INT | Internal |

---

# Request Errors

## TXT-REQ-001 MissingOCRResult

Meaning

OCR result is missing.

Severity

Warning

Retry

AfterCorrection

---

## TXT-REQ-002 InvalidOCRResult

Meaning

OCR result is malformed.

Severity

Warning

Retry

AfterCorrection

---

## TXT-REQ-003 UnsupportedOCRVersion

Meaning

OCR contract version is unsupported.

Severity

Error

Retry

AfterCorrection

---

# Normalization Errors

## TXT-NORM-001 EmptyText

Meaning

Input contains no text.

Severity

Info

Retry

Never

---

## TXT-NORM-002 InvalidUnicode

Meaning

Input contains invalid Unicode characters.

Severity

Warning

Retry

AfterCorrection

---

## TXT-NORM-003 NormalizationFailed

Meaning

Text normalization failed.

Severity

Error

Retry

WithFallback

---

## TXT-NORM-004 UnsupportedEncoding

Meaning

Text encoding is unsupported.

Severity

Warning

Retry

AfterCorrection

---

# Segmentation Errors

## TXT-SEG-001 SegmentationFailed

Meaning

Unable to split text into valid segments.

Severity

Error

Retry

WithFallback

---

## TXT-SEG-002 ReadingOrderConflict

Meaning

Reading order cannot be determined.

Severity

Warning

Retry

WithFallback

Recovery

Use OCR reading order.

---

## TXT-SEG-003 SegmentTooLarge

Meaning

Segment exceeds configured limits.

Severity

Warning

Retry

WithFallback

Recovery

Split into smaller segments.

---

## TXT-SEG-004 InvalidSegment

Meaning

Generated segment is invalid.

Severity

Warning

Retry

AfterCorrection

---

# Language Errors

## TXT-LANG-001 LanguageMismatch

Meaning

Detected language differs from expected language.

Severity

Warning

Retry

WithFallback

---

## TXT-LANG-002 UnsupportedLanguage

Meaning

Language is unsupported.

Severity

Warning

Retry

Never

---

## TXT-LANG-003 MixedLanguageContent

Meaning

Multiple languages exist in the same segment.

Severity

Info

Retry

WithFallback

Recovery

Split into language-specific segments.

---

# Formatting Errors

## TXT-FORMAT-001 InvalidStructure

Meaning

Text structure is invalid.

Examples

- unmatched quotation
- broken paragraph
- malformed sentence

Severity

Warning

Retry

WithFallback

---

## TXT-FORMAT-002 FormattingFailed

Meaning

Unable to build formatted text.

Severity

Error

Retry

WithFallback

---

## TXT-FORMAT-003 InvalidWhitespace

Meaning

Whitespace normalization failed.

Severity

Info

Retry

Never

---

# State Errors

## TXT-STATE-001 ProcessingAlreadyRunning

Meaning

Text Processing is already running.

Severity

Info

Retry

Transient

---

## TXT-STATE-002 ProcessingNotStarted

Meaning

Processing has not started.

Severity

Warning

Retry

AfterCorrection

---

## TXT-STATE-003 InvalidStateTransition

Meaning

Operation is not allowed in the current state.

Severity

Warning

Retry

AfterCorrection

---

# Resource Errors

## TXT-RES-001 MemoryLimitExceeded

Meaning

Processing exceeded memory budget.

Severity

Error

Retry

Transient

---

## TXT-RES-002 Timeout

Meaning

Processing exceeded configured timeout.

Severity

Warning

Retry

Transient

---

## TXT-RES-003 QueueOverflow

Meaning

Too many processing requests.

Severity

Warning

Retry

Transient

---

# Internal Errors

## TXT-INT-001 InternalFailure

Meaning

Unexpected internal failure.

Severity

Critical

Retry

ResetRequired

---

## TXT-INT-002 InvariantViolation

Meaning

Text Processing module invariant was violated.

Severity

Critical

Retry

ResetRequired

---

## TXT-INT-003 AtomicCommitFailed

Meaning

Processed text could not be committed safely.

Severity

Critical

Retry

ResetRequired

---

# Error to State Mapping

| Error | Result |
|--------|--------|
| Invalid request | Reject request |
| Empty text | Ignore |
| Normalization failure | Retry or fallback |
| Segmentation failure | Retry or fallback |
| Formatting failure | Retry or fallback |
| Internal failure | Failed state |

---

# Logging Rules

Logs should include:

- ErrorCode
- SessionId
- OperationId
- OCRResultId
- ProcessingStage
- Duration
- Timestamp

Logs must not include:

- original OCR text
- page images
- browser contents
- personal information
- credentials

---

# Metrics

Recommended metrics

```text
text_processing_error_total
text_processing_timeout_total
text_processing_segmentation_failed_total
text_processing_normalization_failed_total
text_processing_language_mismatch_total
text_processing_format_failed_total
text_processing_memory_limit_total
text_processing_internal_failure_total
```

---

# Architecture Invariants

The Text Processing Module must guarantee:

1. Original OCR result is immutable.
2. Processing is deterministic.
3. Invalid segments are never published.
4. Reading order is preserved whenever possible.
5. Processing failures never modify OCR output.
6. Internal failures transition the module to `Failed`.

---

# Completion Criteria

This document is complete when:

- Every processing failure maps to a stable error code.
- Retry behavior is defined.
- Fallback behavior is documented.
- Privacy requirements are enforced.
- Internal failures are distinguishable from operational failures.
- Error contracts remain backward compatible.