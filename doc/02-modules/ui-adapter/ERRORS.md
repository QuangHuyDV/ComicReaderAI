# UI Adapter Errors

> **Project:** CRAI
> **Module:** `ui-adapter`
> **Path:** `doc/02-modules/ui-adapter/ERRORS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the public error model owned by the UI Adapter module.

UI Adapter errors describe failures involving:

```text
UiIntent adaptation
ViewModel construction
navigation
view lifecycle
dialog presentation
notification presentation
localization application
accessibility adaptation
platform/UI capability integration
UI-local state invariants
```

UI Adapter errors do not describe:

```text
Reading Session business failures
Preference validation failures
Capture failures
Recognition failures
Translation failures
Runtime execution failures
Presentation semantic failures
Diagnostics failures
Storage failures
```

Those errors retain their original owner and ErrorCode.

---

# 2. Error Boundary

Canonical rule:

```text
UI Adapter-owned operation fails
        ↓
UIA-* Error
```

External failure:

```text
Domain / Runtime / Infrastructure Error
        ↓
UI Adapter
        ↓
ErrorViewModel / Dialog / Notification
```

The second flow does not create a new `UIA-*` business error.

---

# 3. Error Principles

## 3.1 UI Adaptation Only

UI Adapter errors describe UI adaptation and platform-facing failures only.

---

## 3.2 Business Error Ownership Preservation

Example:

```text
SES-REV-001
ReadingContextRevisionConflict
```

must remain:

```text
ownerModule = reading-session
originalErrorCode = SES-REV-001
```

when displayed.

---

## 3.3 Stable Error Codes

Public UI Adapter errors use stable machine-readable codes.

Human-readable messages are not authoritative API.

---

## 3.4 Narrow Failure Scope

An error should affect the smallest relevant state domain.

Example:

```text
Notification presentation fails
    ↓
Notification operation failed
```

not:

```text
UI Adapter failed globally
```

---

## 3.5 Graceful Degradation

Optional capability failure should reduce UI functionality where possible instead of terminating the entire adapter.

---

## 3.6 Privacy

Errors must never expose secrets, unsafe reading content, or native implementation objects.

---

# 4. Error Code Format

```text
UIA-<CATEGORY>-<NUMBER>
```

Examples:

```text
UIA-INTENT-001
UIA-NAV-001
UIA-PROJ-001
UIA-VIEW-001
UIA-DIALOG-001
UIA-NOTIFY-001
UIA-LOC-001
UIA-A11Y-001
UIA-CAP-001
UIA-INT-001
```

---

# 5. Error Categories

| Prefix   | Category                              |
| -------- | ------------------------------------- |
| `INTENT` | UiIntent / adapter request            |
| `NAV`    | Navigation                            |
| `PROJ`   | ViewModel projection                  |
| `VIEW`   | View lifecycle / presentation         |
| `DIALOG` | Dialog                                |
| `NOTIFY` | Notification                          |
| `LOC`    | Localization / appearance application |
| `A11Y`   | Accessibility                         |
| `CAP`    | UI/platform capability                |
| `PRIV`   | UI privacy / safe presentation        |
| `INT`    | Internal invariant                    |

---

# 6. Severity

Recommended:

```text
Info
Warning
Error
Critical
```

Meaning:

| Severity   | Meaning                                                 |
| ---------- | ------------------------------------------------------- |
| `Info`     | Expected UI-local non-success condition                 |
| `Warning`  | Scoped UI issue; fallback usually possible              |
| `Error`    | Requested UI Adapter operation failed                   |
| `Critical` | Core UI Adapter invariant or safe operation compromised |

Severity does not imply business retry/restart behavior.

---

# 7. Recovery Classification

Replace coarse:

```text
Recoverable
Non-Recoverable
```

with:

```text
RecoveryClassification
- NoAction
- CorrectInput
- RetryUiOperation
- RefreshProjection
- NavigateFallback
- UseFallbackResource
- RetryAfterCapabilityRecovery
- RecreateView
- RecreateFrontend
- ApplicationRecovery
```

---

# 8. UiAdapterError Contract

Conceptually:

```text
UiAdapterError
├── errorCode
├── category
├── severity
├── recoveryClassification
├── intentId?
├── frontendId?
├── viewId?
├── capability?
├── correlationId?
├── diagnosticRef?
├── messageKey?
└── safeMetadata?
```

---

# 9. Intent Errors

## UIA-INTENT-001 — InvalidUiIntent

Meaning:

```text
UiIntent payload violates UI Adapter contract.
```

Examples:

```text
missing intent type
malformed payload
missing required adapter-level field
invalid source ViewId
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

No business command is forwarded.

---

# 10. UIA-INTENT-002 — UnsupportedUiIntent

Meaning:

The UI Adapter/runtime profile does not support the requested intent.

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
```

---

# 11. UIA-INTENT-003 — UiIntentForwardingFailed

Meaning:

The adapter could not forward a valid UiIntent through the intended application boundary.

Examples:

```text
application command binding unavailable
frontend/application bridge unavailable
```

Severity:

```text
Error
```

Recovery:

```text
RetryUiOperation
or
ApplicationRecovery
```

This error does not describe downstream business rejection.

---

# 12. Downstream Rejection Is Not UIA Failure

Example:

```text
SavePreferenceIntent
    ↓
Preferences rejects invalid value
```

Result:

```text
PREF-* error
```

not:

```text
UIA-INTENT-003
```

The UI Adapter successfully forwarded the intent.

---

# 13. Navigation Errors

## UIA-NAV-001 — ViewNotFound

Meaning:

The requested logical `ViewId` is unknown.

Severity:

```text
Warning
```

Recovery:

```text
NavigateFallback
```

---

# 14. UIA-NAV-002 — InvalidNavigation

Meaning:

The requested navigation transition violates UI-local navigation rules.

Examples:

```text
unknown target
invalid frontend destination
invalid navigation context
```

Severity:

```text
Warning
```

Recovery:

```text
CorrectInput
or
NavigateFallback
```

Current valid navigation state remains intact.

---

# 15. UIA-NAV-003 — NavigationFailed

Meaning:

Navigation was valid but could not be applied.

Severity:

```text
Error
```

Recovery:

```text
RetryUiOperation
or
NavigateFallback
```

---

# 16. UIA-NAV-004 — NavigationStale

Meaning:

A navigation operation completed against stale UI-local navigation authority.

Severity:

```text
Info
```

Recovery:

```text
RefreshProjection
```

Stale navigation result must not overwrite newer state.

---

# 17. Removed `ViewAlreadyOpen`

The v1:

```text
ViewAlreadyOpen
```

should normally become either:

```text
NoOp
```

or a navigation-rule outcome.

It does not need a dedicated public error unless the final navigation contract requires strict singleton semantics.

---

# 18. Projection Errors

## UIA-PROJ-001 — ViewModelConstructionFailed

Meaning:

UI Adapter could not build a valid Candidate ViewModel from authoritative source snapshots.

Severity:

```text
Error
```

Recovery:

```text
RefreshProjection
or
RetryUiOperation
```

Previous valid ViewModel should remain active where safe.

---

# 19. UIA-PROJ-002 — InvalidViewModel

Meaning:

A Candidate ViewModel violates UI Adapter invariants.

Examples:

```text
missing required projection field
native UI object leaked
unsafe mutable domain object
invalid accessibility metadata
invalid localization metadata
```

Severity:

```text
Error
```

Recovery:

```text
ApplicationRecovery
```

Candidate must not be published.

---

# 20. UIA-PROJ-003 — StaleViewModelProjection

Meaning:

Candidate ViewModel was built from superseded source provenance.

Severity:

```text
Info
```

Recovery:

```text
RefreshProjection
```

No existing valid ViewModel is mutated.

---

# 21. UIA-PROJ-004 — ViewModelPublicationFailed

Meaning:

A valid Candidate ViewModel could not be atomically published to the frontend.

Severity:

```text
Error
```

Recovery:

```text
RetryUiOperation
or
RecreateView
```

---

# 22. UIA-PROJ-005 — ProjectionSourceUnavailable

Meaning:

One or more source snapshots required for projection are currently unavailable.

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
or
RefreshProjection
```

Do not fabricate authoritative state.

---

# 23. View Errors

## UIA-VIEW-001 — ViewUnavailable

Meaning:

A known logical view cannot currently be instantiated or used.

Severity:

```text
Warning
```

Recovery:

```text
RecreateView
or
NavigateFallback
```

---

# 24. UIA-VIEW-002 — InvalidViewLifecycleTransition

Meaning:

A requested view lifecycle transition is invalid.

Example:

```text
DISPOSED → VISIBLE
```

Severity:

```text
Warning
```

Recovery:

```text
RecreateView
```

---

# 25. UIA-VIEW-003 — ViewRenderFailed

Meaning:

The platform-facing rendering of one view failed.

Severity:

```text
Error
```

Recovery:

```text
RetryUiOperation
or
RecreateView
```

Business/domain state remains unchanged.

---

# 26. UIA-VIEW-004 — ViewResourceUnavailable

Meaning:

A view-specific UI resource required for presentation is unavailable.

Examples:

```text
icon asset
UI-only style asset
frontend resource
```

Severity:

```text
Warning
```

Recovery:

```text
UseFallbackResource
```

---

# 27. Removed Generic `RenderingFailed`

The v1:

```text
RenderingFailed
```

is replaced by narrower semantics:

```text
ViewModelConstructionFailed
ViewModelPublicationFailed
ViewRenderFailed
```

This distinguishes projection from native rendering.

---

# 28. Removed Generic `ResourceMissing`

The v1 error mixed:

```text
icon
theme
font
localization file
```

under one category.

v2 routes these according to ownership:

```text
ViewResourceUnavailable
LocalizationResourceUnavailable
AppearanceResourceUnavailable
UiCapabilityUnavailable
```

---

# 29. Dialog Errors

## UIA-DIALOG-001 — DialogPresentationFailed

Meaning:

A dialog could not be shown on the requested frontend.

Severity:

```text
Warning / Error
```

Recovery:

```text
RetryUiOperation
or
UseFallbackResource
```

---

# 30. UIA-DIALOG-002 — DialogConflict

Meaning:

A UI-local dialog exclusivity rule prevents the requested dialog from opening.

Severity:

```text
Info / Warning
```

Recovery:

```text
NoAction
```

This replaces implementation-oriented:

```text
DialogAlreadyOpen
```

---

# 31. UIA-DIALOG-003 — InvalidDialogResponse

Meaning:

A dialog response references an invalid action or stale dialog instance.

Severity:

```text
Warning
```

Recovery:

```text
RefreshProjection
```

No application command should be produced.

---

# 32. Notification Errors

## UIA-NOTIFY-001 — NotificationPresentationFailed

Meaning:

A notification could not be displayed on the requested UI surface.

Severity:

```text
Warning
```

Recovery:

```text
NoAction
or
RetryUiOperation
```

---

# 33. UIA-NOTIFY-002 — NotificationCapabilityUnavailable

Meaning:

The requested notification capability is unavailable.

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

Main UI remains usable.

---

# 34. UIA-NOTIFY-003 — UnsafeNotificationContent

Meaning:

Notification content is unsafe for an external/system notification surface.

Examples:

```text
raw OCR text
sensitive reading content
credential-like data
```

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

Fail closed or replace with a safe generic notification.

---

# 35. Localization and Appearance Errors

## UIA-LOC-001 — LocalizationResourceUnavailable

Meaning:

Required localization resources for the requested locale cannot be loaded.

Severity:

```text
Warning
```

Recovery:

```text
UseFallbackResource
```

---

# 36. UIA-LOC-002 — UnsupportedLocale

Meaning:

The requested locale is not supported by the active UI profile.

Severity:

```text
Warning
```

Recovery:

```text
UseFallbackResource
```

---

# 37. UIA-LOC-003 — LocalizationApplyFailed

Meaning:

Localization resources were available but could not be applied to the UI.

Severity:

```text
Error
```

Recovery:

```text
RetryUiOperation
or
UseFallbackResource
```

---

# 38. UIA-LOC-004 — AppearanceResourceUnavailable

Meaning:

Required UI appearance/theme resources cannot be applied.

Severity:

```text
Warning
```

Recovery:

```text
UseFallbackResource
```

---

# 39. UIA-LOC-005 — AppearanceApplyFailed

Meaning:

Resolved appearance/theme could not be applied successfully.

Severity:

```text
Warning / Error
```

Recovery:

```text
UseFallbackResource
or
RetryUiOperation
```

---

# 40. Removed `ThemeNotFound`

Theme persistence/definition may belong to Preferences/Application resource ownership.

UI Adapter should expose application failure only when applying the resolved appearance fails.

---

# 41. Accessibility Errors

## UIA-A11Y-001 — AccessibilityCapabilityUnavailable

Meaning:

One required/optional platform accessibility capability is unavailable.

Severity:

```text
Warning / Error
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

---

# 42. UIA-A11Y-002 — AccessibilityMetadataInvalid

Meaning:

UI-facing accessibility metadata violates adapter rules.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

# 43. UIA-A11Y-003 — FocusOperationFailed

Meaning:

A UI-local accessibility/focus operation could not be applied.

Severity:

```text
Warning
```

Recovery:

```text
RetryUiOperation
```

This replaces v1 `FocusManagementFailed`.

---

# 44. Capability Errors

## UIA-CAP-001 — UiCapabilityUnavailable

Meaning:

A requested UI capability currently cannot operate.

Examples:

```text
OverlayWindow
Clipboard
FilePicker
SystemNotification
ScreenRegionPicker
KeyboardShortcut
```

Severity:

```text
Warning
```

Recovery:

```text
RetryAfterCapabilityRecovery
```

---

# 45. UIA-CAP-002 — UiCapabilityDisabled

Meaning:

The requested capability is intentionally disabled by configuration or policy.

Severity:

```text
Info
```

Recovery:

```text
NoAction
```

---

# 46. UIA-CAP-003 — UiCapabilityDegraded

Meaning:

A capability remains usable with reduced functionality.

Severity:

```text
Warning
```

Recovery:

```text
NoAction
or
RetryAfterCapabilityRecovery
```

---

# 47. UIA-CAP-004 — PlatformBindingUnavailable

Meaning:

A platform adapter/binding required for the requested capability cannot currently operate.

Severity:

```text
Error
```

Recovery:

```text
RecreateFrontend
or
ApplicationRecovery
```

---

# 48. Removed `UnsupportedPlatform`

The v1 whole-platform error is too broad.

Prefer capability-based compatibility.

Example:

```text
Browser Extension frontend
    supports Settings
    supports Reader panel
    does not support OverlayWindow
```

Return:

```text
UIA-CAP-001
capability = OverlayWindow
```

rather than declaring the entire platform unsupported.

---

# 49. Removed `ExtensionUnavailable`

Browser extension is a frontend/profile, not a universal UI Adapter domain concept.

Prefer:

```text
FrontendUnavailable
UiCapabilityUnavailable
```

depending on the failed boundary.

---

# 50. Frontend Errors

## UIA-CAP-005 — FrontendUnavailable

Meaning:

One configured frontend cannot currently operate.

Examples:

```text
overlay frontend disconnected
browser extension frontend unavailable
secondary desktop window unavailable
```

Severity:

```text
Warning / Error
```

Recovery:

```text
RecreateFrontend
or
RetryAfterCapabilityRecovery
```

Other frontends may remain active.

---

# 51. Privacy Errors

## UIA-PRIV-001 — UnsafeUiContent

Meaning:

Content cannot safely be displayed on the requested surface.

Examples:

```text
raw sensitive reading text in system notification
secret in dialog metadata
unsafe window title
```

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

# 52. UIA-PRIV-002 — UnsafeClipboardContent

Meaning:

Clipboard operation violates UI privacy policy or lacks explicit user intent.

Severity:

```text
Error
```

Recovery:

```text
CorrectInput
```

---

# 53. UIA-PRIV-003 — UnsafeAccessibilityContent

Meaning:

Accessibility metadata contains data disallowed for the platform accessibility boundary.

Severity:

```text
Warning / Error
```

Recovery:

```text
CorrectInput
```

---

# 54. Internal Errors

## UIA-INT-001 — InternalUiAdapterFailure

Meaning:

Unexpected failure inside UI Adapter-owned logic.

Severity:

```text
Error
```

Recovery:

```text
RetryUiOperation
or
ApplicationRecovery
```

depending on scope.

---

# 55. UIA-INT-002 — UiAdapterInvariantViolation

Meaning:

A core architecture invariant was violated.

Examples:

```text
native UI object escaped stable contract
ViewModel mutated after publication
UI Adapter created business authority
source revision ownership became ambiguous
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

# 56. UIA-INT-003 — ViewModelAuthorityViolation

Meaning:

A ViewModel or UI-local state was incorrectly treated as authoritative business state.

Severity:

```text
Critical
```

Recovery:

```text
ApplicationRecovery
```

---

# 57. UIA-INT-004 — PlatformObjectLeak

Meaning:

A native/platform object crossed a stable application/domain boundary.

Examples:

```text
DOM node
HWND
Qt pointer
framework component
native window object
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

# 58. Removed `EventDispatchFailed`

The v1:

```text
EventDispatchFailed
```

is too ambiguous.

Possible replacements:

```text
UiIntentForwardingFailed
ViewModelPublicationFailed
UiCapabilityUnavailable
```

depending on what actually failed.

UI-local event publication failure usually does not require a public business error at all.

---

# 59. External Error Projection

UI Adapter may project errors from:

```text
SES-*
PREF-*
CAP-*
REC-*
TXT-*
TRN-*
PRES-*
RUN-*
DIAG-*
STO-*
```

without changing their identity.

---

# 60. ErrorViewModel

Conceptually:

```text
ErrorViewModel
├── ownerModule
├── originalErrorCode
├── severity
├── titleKey
├── messageKey
├── recoveryActions[]
├── safeParameters?
└── diagnosticRef?
```

---

# 61. Example — Preference Validation Failure

```text
SavePreferenceIntent
    ↓
Preferences
    ↓
PREF-VAL-xxx
```

UI Adapter:

```text
PREF-VAL-xxx
    ↓
SettingsValidationViewModel
```

No `UIA-*` error is created.

---

# 62. Example — Reading Session Conflict

```text
ChangeSessionPreferenceIntent
    ↓
SES-REV-001
```

UI Adapter may display:

```text
"Session changed. Refresh and try again."
```

but original code remains:

```text
SES-REV-001
```

---

# 63. Example — Runtime Retry Exhausted

```text
Runtime
    ↓
RUN-...
RetryExhausted
```

UI Adapter:

```text
Runtime Error
    ↓
ErrorViewModel
    ↓
Retry action shown only if Application says available
```

UI Adapter does not invent retry authority.

---

# 64. Example — Translation Failure

```text
TRN-PROV-003
    ↓
UI Adapter
    ↓
safe error projection
```

No:

```text
UIA-VIEW-003
```

unless the UI itself also failed to render the error state.

---

# 65. Example — View Projection Failure

```text
Application snapshot
    ↓
Candidate ReaderViewModel
    ↓
construction fails
    ↓
UIA-PROJ-001
```

Previous valid ReaderViewModel remains displayed where safe.

---

# 66. Example — Notification Capability

```text
SystemNotification
AVAILABLE → UNAVAILABLE
    ↓
UIA-NOTIFY-002
```

Main application window remains usable.

---

# 67. Example — Overlay Frontend Failure

```text
Overlay frontend
    ↓
native binding unavailable
    ↓
UIA-CAP-005 FrontendUnavailable
```

Desktop main frontend may remain active.

---

# 68. Error-to-State Mapping

| Error                           | State Effect                                |
| ------------------------------- | ------------------------------------------- |
| InvalidUiIntent                 | None                                        |
| InvalidNavigation               | Scoped operation rejected                   |
| ViewModelConstructionFailed     | Projection operation failed                 |
| StaleViewModelProjection        | Candidate discarded                         |
| ViewRenderFailed                | View operation failed                       |
| DialogPresentationFailed        | Dialog operation failed                     |
| Notification unavailable        | Notification capability unavailable         |
| Localization unavailable        | Localization degraded/fallback              |
| Optional capability unavailable | Capability state only                       |
| Frontend unavailable            | Frontend degraded/unavailable               |
| Core invariant violation        | Module DEGRADED or STOPPING                 |
| External business/domain error  | No UI Adapter lifecycle transition required |

---

# 69. No Global `FAILED` Mapping

The v1 recovery model treated some errors as effectively terminal:

```text
UnsupportedPlatform
PlatformBindingFailed
InternalUIError
```

In v2, failure scope is narrower.

Example:

```text
overlay binding fails
    ↓
Overlay frontend = DEGRADED
```

not automatically:

```text
UI Adapter = failed
```

---

# 70. Business Failure Independence

External operation failure does not corrupt UI Adapter state.

Example:

```text
Translation fails
    ↓
existing ReaderViewModel remains valid
    ↓
new error projection may be displayed
```

---

# 71. Projection Failure Independence

If a new Candidate ViewModel fails:

```text
Candidate discarded
previous ViewModel retained
```

where safe.

---

# 72. Navigation Failure Independence

Navigation failure must preserve current valid navigation state.

---

# 73. Dialog Failure Independence

A failed dialog should not invalidate unrelated views.

---

# 74. Notification Failure Independence

System notification failure should not invalidate in-app notification capability if separate.

---

# 75. Capability Recovery

Example:

```text
Clipboard = UNAVAILABLE
    ↓
platform recovers
    ↓
Clipboard = AVAILABLE
```

No business restart occurs.

---

# 76. Error Reporting

Safe UI Adapter error reporting may include:

```text
UIA ErrorCode
intentId
viewId
frontendId
capability
severity
recovery classification
correlationId
diagnosticRef
```

---

# 77. Public Message Rule

Public errors should provide:

```text
ErrorCode
messageKey
safe parameters
```

Final localized message is produced by UI localization/presentation.

---

# 78. Sensitive Data Prohibition

Errors must never expose:

```text
raw screenshot
OCR text
translation text
password
API key
token
cookie
private key
native UI handle
raw provider payload
```

---

# 79. Platform Detail Boundary

Public errors may expose:

```text
capability = OverlayWindow
```

but should not expose raw implementation exceptions such as:

```text
HRESULT
DOM exception object
native stack pointer
framework component object
```

These belong behind `diagnosticRef`.

---

# 80. DiagnosticRef

Implementation-specific failure detail may be retained safely through:

```text
diagnosticRef
```

for developer/support diagnostics.

---

# 81. Diagnostics Observation

UI Adapter-owned errors may be reported to Diagnostics using:

```text
ObserveError
ownerModule = ui-adapter
originalErrorCode = UIA-...
```

No generic `UiErrorOccurred` Event Bus event is required.

---

# 82. Event Publication Failure

If an optional UI-local event cannot be published after a valid UI-local state change:

```text
valid UI state remains committed
```

Do not rollback a successfully opened view merely because a local subscriber failed.

---

# 83. Retry Semantics

`RetryUiOperation` refers only to UI Adapter-owned operation.

Examples:

```text
retry navigation
retry ViewModel projection
retry applying localization
retry showing dialog
```

It does not mean:

```text
retry Translation
retry OCR
retry Runtime Attempt
retry pipeline
```

---

# 84. Application Recovery

Use `ApplicationRecovery` only when:

```text
core adapter invariant violated
stable boundary corrupted
required frontend cannot be reconstructed safely
```

It is not the default response to ordinary UI operation failure.

---

# 85. Metrics

Recommended UI Adapter error metrics:

```text
ui_adapter_error_total
ui_adapter_intent_rejected_total
ui_adapter_navigation_failure_total
ui_adapter_projection_failure_total
ui_adapter_view_failure_total
ui_adapter_capability_unavailable_total
ui_adapter_localization_fallback_total
ui_adapter_privacy_rejection_total
ui_adapter_invariant_violation_total
```

Avoid high-cardinality labels such as:

```text
raw ViewModel ID
full URL
window title
user content
```

---

# 86. Recursion Safety

Displaying an error must not cause an infinite failure loop.

Example:

```text
error dialog fails
    ↓
attempt to show error dialog
    ↓
error dialog fails
    ↓
...
```

Use bounded fallback such as:

```text
safe inline fallback
simple native fallback
diagnostics-only record
one-shot suppression
```

---

# 87. Compatibility

Adding a new `UIA-*` code is generally backward-compatible.

Changing the meaning of an existing code is not.

Implementation-specific platform errors may evolve without changing public UI Adapter taxonomy.

---

# 88. Architecture Invariants

1. UIA errors represent UI Adapter-owned failures only.

2. Domain errors retain original owner and ErrorCode.

3. Runtime errors retain Runtime ownership.

4. Presentation semantic errors retain Presentation ownership.

5. Preferences validation errors are not converted into UIA errors.

6. Reading Session revision errors are not converted into UIA errors.

7. UI Adapter errors do not trigger business retry.

8. `RetryUiOperation` applies only to UI-local/adaptation work.

9. UI errors affect the narrowest relevant scope.

10. One view failure does not imply module failure.

11. One frontend failure does not imply all frontends fail.

12. One optional capability failure does not imply module failure.

13. Previous valid ViewModel is preserved after Candidate failure where safe.

14. Invalid navigation preserves current navigation state.

15. Stale projection never overwrites newer projection.

16. Native objects never appear in public errors.

17. Platform-specific exception details remain internal.

18. Error payloads remain privacy-safe.

19. User-facing messages use localization keys.

20. UI Adapter does not use generic global FAILED for ordinary failures.

21. Capability-based errors are preferred over whole-platform errors.

22. `UnsupportedPlatform` is removed where capability-specific semantics are possible.

23. `RenderingFailed` is replaced by narrower projection/render errors.

24. `ResourceMissing` is replaced by ownership-specific resource errors.

25. `EventDispatchFailed` is removed as an ambiguous public error.

26. Error observation through Diagnostics preserves original UIA code.

27. UI error presentation recursion remains bounded.

28. Event publication failure does not rollback valid UI-local state.

---

# 89. Testing — Intent Errors

Test:

```text
malformed UiIntent
unsupported UiIntent
forwarding bridge failure
downstream business rejection
```

Verify only actual adapter failures create `UIA-*`.

---

# 90. Testing — Navigation

Verify:

```text
ViewNotFound
InvalidNavigation
NavigationFailed
NavigationStale
```

preserve valid current navigation state.

---

# 91. Testing — Projection

Inject:

```text
construction failure
invalid ViewModel
stale Candidate
publication failure
```

Verify Candidate isolation and previous projection preservation.

---

# 92. Testing — External Error Ownership

Inject:

```text
SES-*
PREF-*
TRN-*
RUN-*
DIAG-*
```

Verify original owner/code remains in ErrorViewModel.

---

# 93. Testing — Capability Isolation

Disable:

```text
SystemNotification
Clipboard
OverlayWindow
```

independently.

Verify unrelated UI capabilities remain usable.

---

# 94. Testing — Multiple Frontends

Fail overlay frontend.

Verify desktop main frontend remains active where architecture permits.

---

# 95. Testing — Localization

Inject missing locale resources.

Verify fallback semantics and no global adapter failure.

---

# 96. Testing — Privacy

Attempt to display unsafe content in:

```text
system notification
window title
clipboard
accessibility metadata
```

Verify safe rejection/fallback.

---

# 97. Testing — Native Object Leak

Inject a native control object into Candidate ViewModel.

Verify:

```text
UIA-INT-004 PlatformObjectLeak
```

or equivalent invariant rejection.

---

# 98. Testing — Recursion

Cause the preferred error dialog itself to fail.

Verify fallback is bounded and does not recurse indefinitely.

---

# 99. Testing — Diagnostics

Observe a `UIA-*` error through Diagnostics.

Verify:

```text
ownerModule = ui-adapter
originalErrorCode = UIA-...
```

is preserved.

---

# 100. Related Documents

```text
doc/02-modules/ui-adapter/MODULE.md
doc/02-modules/ui-adapter/CONTRACT.md
doc/02-modules/ui-adapter/STATES.md
doc/02-modules/ui-adapter/EVENTS.md
doc/02-modules/ui-adapter/README.md

doc/02-modules/reading-session/
doc/02-modules/preferences/
doc/02-modules/presentation/
doc/02-modules/diagnostics/

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
```

---

# 101. Completion Criteria

This specification is synchronized when:

* `UIA-*` is limited to UI Adapter-owned failures;
* external module errors preserve ownership;
* coarse Recoverable/Non-Recoverable is replaced;
* errors are capability/view/operation scoped;
* `RenderingFailed` is replaced by projection/render-specific errors;
* `ResourceMissing` is ownership-specific;
* `UnsupportedPlatform` is replaced by capability semantics where possible;
* `ExtensionUnavailable` is generalized to frontend/capability failure;
* `EventDispatchFailed` is removed as an ambiguous public error;
* Candidate ViewModel failures preserve previous projection where safe;
* optional UI failure does not produce global module failure;
* privacy/safe-surface errors are explicit;
* native platform implementation details remain internal;
* error display recursion is bounded.

---

# 102. Summary

UI Adapter-owned failure:

```text
UI-local / Adapter Operation
        ↓
UI Adapter
        ↓
UIA-* Error
```

External failure:

```text
Domain / Runtime Module
        ↓
Original Error
        ↓
UI Adapter
        ↓
ErrorViewModel
```

Projection failure:

```text
Authoritative Snapshot
        ↓
Candidate ViewModel
        ↓
UI Adapter failure
        ↓
Candidate discarded
        ↓
Previous valid ViewModel preserved
```

The central rule is:

```text
UI Adapter owns
failures of UI adaptation.

It never takes ownership
of the business or Runtime errors
it merely displays.
```
