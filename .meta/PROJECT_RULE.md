# CRAI Project Rules

Version: 1.0

---

# 1. Purpose

This document defines the engineering principles and architectural constraints of CRAI.

These rules apply to every module, feature and contributor.

If a design conflicts with this document, the design should be reconsidered before implementation.

---

# 2. Design Philosophy

CRAI is designed for long-term maintainability.

Core principles:

- Simplicity over complexity.
- Modularity over monolithic design.
- Explicit behavior over hidden behavior.
- Composition over inheritance where practical.
- Replaceability over vendor lock-in.
- Documentation before implementation.
- Optimize only after measurement.

---

# 3. Single Responsibility

Every module should have one clear responsibility.

A module should answer only one primary question.

Bad:

OCR module
- OCR
- Translation
- Cache

Good:

OCR
Translation
Cache

---

# 4. Dependency Direction

Dependencies must only flow downward.

Presentation
↓

Application
↓

Domain
↓

Infrastructure

Lower layers must never depend on upper layers.

---

# 5. Replaceability

Every external dependency should be replaceable.

Examples:

OCR Engine

Translator

AI Provider

Image Processor

Storage

Switching providers should require minimal code changes.

---

# 6. Loose Coupling

Modules communicate through contracts.

Avoid direct implementation dependencies.

Prefer:

Interface

↓

Implementation

Never expose internal implementation details.

---

# 7. Plugin First

Whenever reasonable, new capabilities should be implemented as plugins.

Core should remain lightweight.

Plugins should be independently loadable.

---

# 8. Configuration

Never hardcode environment-specific values.

Configuration should be externalized.

Configuration must be validated before use.

---

# 9. Error Handling

Errors should never disappear silently.

Every unexpected error should be:

- logged
- traceable
- actionable

Fail clearly.

---

# 10. Logging

Logs should explain:

What happened

Where

Why

Avoid meaningless logs.

Bad:

Error occurred.

Good:

OCR timeout after 5 seconds while processing page 17.

---

# 11. Performance

Performance optimization should be evidence-driven.

Do not optimize prematurely.

Measure first.

Optimize second.

Document the reason.

---

# 12. Thread Safety

UI thread should remain responsive.

Heavy work belongs to background workers.

Avoid blocking operations on UI.

---

# 13. Documentation

Architecture changes require documentation updates.

Documentation is part of implementation.

---

# 14. Backward Compatibility

Public APIs should remain stable whenever practical.

Breaking changes should be documented.

Migration path should be provided.

---

# 15. AI Constraints

AI should not:

- invent requirements
- silently rewrite architecture
- rename modules without discussion
- introduce new dependencies without justification
- modify unrelated components

When uncertain:

Ask first.

---

# 16. Decision Priority

When conflicts occur:

User Request

↓

Architecture

↓

Project Rules

↓

Coding Style

↓

Implementation Convenience

Never violate a higher priority.