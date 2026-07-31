# UI Adapter Module Errors

- Module: UI Adapter
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the error model of the UI Adapter Module.

The UI Adapter owns errors related to presentation, navigation, rendering, platform integration, localization, accessibility and user interface resources.

UI Adapter errors describe presentation-layer failures only and never business rule violations.

---

# Error Principles

## Presentation Only

Errors describe failures in the UI layer.

Business modules are responsible for business validation and domain errors.

---

## Stable Error Codes

Each error has a stable identifier that remains compatible across versions.

---

## Recoverability

Errors are classified as:

- Recoverable
- Non-Recoverable

---

## Graceful Degradation

Whenever possible, the UI should continue operating with reduced functionality rather than terminating.

---

# Error Categories

## View Errors

### ViewNotFound

The requested logical view does not exist.

Recovery:

- Navigate to a default view.
- Log the missing identifier.

---

### InvalidNavigation

The requested navigation target is invalid.

---

### ViewAlreadyOpen

The requested view is already active.

---

## Rendering Errors

### RenderingFailed

A ViewModel could not be rendered.

Recovery:

- Retry rendering.
- Preserve the previous ViewModel.

---

### InvalidViewModel

The supplied ViewModel is incomplete or malformed.

---

### ResourceMissing

A required UI resource could not be loaded.

Examples:

- Icon
- Theme asset
- Font
- Localization file

---

## Theme & Localization Errors

### ThemeNotFound

The requested theme does not exist.

---

### LocalizationNotFound

The requested language resources are unavailable.

---

### UnsupportedLocale

The selected locale is not supported.

---

## Dialog Errors

### DialogFailed

The dialog could not be displayed.

---

### DialogAlreadyOpen

A conflicting dialog is already active.

---

## Platform Errors

### UnsupportedPlatform

The current platform is unsupported.

---

### PlatformBindingFailed

Failed to initialize platform-specific bindings.

---

### ExtensionUnavailable

A required browser extension or platform integration is unavailable.

---

## Accessibility Errors

### AccessibilityUnavailable

Accessibility services are unavailable.

---

### FocusManagementFailed

Keyboard or accessibility focus could not be updated.

---

## Notification Errors

### NotificationFailed

A notification could not be displayed.

---

## Internal Errors

### InternalUIError

Unexpected internal UI failure.

---

### EventDispatchFailed

A UI event could not be dispatched.

---

# Error Severity

| Severity | Description |
|----------|-------------|
| Info | Informational only |
| Warning | Recoverable UI issue |
| Error | Operation failed |
| Critical | UI cannot safely continue |

---

# Recovery Strategy

Recoverable errors include:

- ViewNotFound
- InvalidNavigation
- RenderingFailed
- ResourceMissing
- ThemeNotFound
- LocalizationNotFound
- NotificationFailed

Non-Recoverable errors include:

- UnsupportedPlatform
- PlatformBindingFailed
- InternalUIError

---

# Error Reporting

Each error should include:

- ErrorCode
- Message
- Timestamp
- View Identifier (when applicable)
- Platform
- CorrelationId (if available)

Sensitive information must never be exposed to the user.

---

# Architecture Invariants

1. UI Adapter errors never represent business rule failures.
2. Rendering failures never modify business state.
3. Navigation failures never corrupt UI state.
4. Stable error codes are preserved across versions.
5. Recoverable errors provide deterministic recovery guidance.
6. Platform-specific details remain hidden from business modules.
7. User-facing messages should be localized.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- EVENTS.md
- STATES.md
- README.md
