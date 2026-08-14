# Coding Standards and Rules

> **Project:** CRAI
> **Document:** Coding Standards
> **Path:** `.meta/CODING_RULE.md`
> **Version:** 1.0
> **Status:** Active

---

## 1. Purpose

This document defines the programming style, formatting rules, and coding standards for CRAI.

These rules apply to all source code. They supplement the architecture rules in `PROJECT_RULE.md`.

Implementation has not yet started. These rules will be applied when coding begins.

---

## 2. Language

The implementation language will be determined during Technology Selection.

These rules describe principles that apply regardless of language choice.

Specific language-level style rules (linter config, formatter config) will be added after Technology Selection is complete.

---

## 3. Naming Rules

### General

- Names must describe **what**, not **how**.
- Names must be meaningful in the CRAI domain.
- Avoid abbreviations unless they are universally understood (e.g., `OCR`, `UI`, `ID`).
- Avoid generic names: `Manager`, `Helper`, `Utils`, `Common`, `Data` without a qualifier.

### Classes and Interfaces

Use names that reflect CRAI domain concepts:

```text
Good:
    ITextRecognizer
    RecognitionResult
    TranslationRequest
    ReadingSessionId

Avoid:
    Manager2
    DataHelper
    CommonUtil
    AbstractBaseProcessor
```

### Events

Events use past-tense names:

```text
Good:
    FrameCaptured
    TranslationCompleted
    ReadingSessionStopped

Avoid:
    OnCapture
    DoTranslate
    SessionStop
```

### Commands and Queries

Commands use imperative form:

```text
StartReadingSession
UpdateCaptureRegion
ClearTranslationCache
```

Queries use noun or question form:

```text
GetActiveReadingSession
GetCachedTranslation
GetAvailableProviders
```

---

## 4. Module and File Organization

- Each module directory contains one primary responsibility.
- Implementation files mirror the architecture described in module documents.
- Public interfaces are defined before implementations.
- Internal implementation details are not exposed in public APIs.

---

## 5. Dependency Rules

- Dependencies flow downward: Presentation → Application → Domain → Infrastructure.
- No circular dependencies.
- Business logic must not depend on UI frameworks.
- Domain must not depend on OCR SDKs, translation SDKs, or vendor models.
- Provider-specific models must be converted at the provider boundary.

---

## 6. Error Handling

- Errors must never disappear silently.
- Every unexpected error must be logged with context.
- Use domain-typed errors, not raw exception types from vendor libraries.
- Errors must be actionable: include what happened, where, and why.
- Distinguish expected failures from unexpected failures.

---

## 7. Asynchronous Code

- Long-running operations must be asynchronous.
- The UI thread must never block on computation, network, or disk I/O.
- Cancellation must be supported for all long-running operations.
- Obsolete results must not overwrite current results.
- Immutable inputs and outputs are preferred.

---

## 8. Logging

- Log what happened, where, and why.
- Do not log secret values, API keys, or user content by default.
- Use structured logging where the logging infrastructure supports it.
- Log at the appropriate level: DEBUG for development detail, INFO for significant events, WARNING for degraded behavior, ERROR for failures.

```text
Good:
    OCR timeout after 5 seconds on page 17 of session abc-123.

Avoid:
    Error occurred.
    Something went wrong.
```

---

## 9. Testing

- Business logic must be independently testable without UI or network.
- Module contracts must be testable through their public interfaces.
- Provider adapters must have contract tests against their interfaces.
- Integration tests may use real backends in isolated environments.

---

## 10. Documentation

- Non-trivial public interfaces must have documentation comments.
- Architecture changes require documentation updates.
- Code comments explain **why**, not **what** (the code already shows what).
- Remove outdated comments promptly.

---

## 11. Performance

- Do not optimize prematurely.
- Measure before optimizing.
- Document the reason for any intentional optimization.
- Performance targets are defined in `01-architecture/runtime/PERFORMANCE_MODEL.md`.

---

## 12. Security

- Never hardcode secrets, API keys, or credentials in source code.
- Use Secret Management for all sensitive values.
- Do not log sensitive data.
- Validate all external inputs at module boundaries.
