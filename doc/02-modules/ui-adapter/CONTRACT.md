# UI Adapter Contract

> **Project:** CRAI
> **Module:** `ui-adapter`
> **Path:** `doc/02-modules/ui-adapter/CONTRACT.md`
> **Contract Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the public contract boundary of the UI Adapter module.

The UI Adapter provides a stable adaptation layer between:

```text id="aqh7pg"
Native / Platform UI
        ↕
UI Adapter
        ↕
Application / Domain / Runtime Contracts
```

Its two primary directions are:

```text id="z75nft"
Native UI Event
    ↓
UiIntent
    ↓
Application / Module Command
```

and:

```text id="3gu8uw"
Application / Module Snapshot
        ↓
UI Adapter
        ↓
Immutable ViewModel
        ↓
Native UI
```

The UI Adapter does not own business execution.

---

# 2. Contract Scope

This file defines:

```text id="81ib5a"
UiIntent
UiActionResult
ViewId
ViewModel
ViewStateSnapshot
NavigationModel
DialogModel
NotificationModel
AccessibilityMetadata
LocalizationMetadata
UiCapability
UI-local commands
application-facing intents
projection/query contracts
platform-safe references
```

This file does not define:

```text id="6bej07"
Reading Session business rules
Preference validation
Runtime retry policy
pipeline restart
WorkItem
Attempt
Presentation semantics
Storage implementation
native UI framework types
```

---

# 3. Contract Principles

## 3.1 Adapter-Only Responsibility

UI Adapter translates.

It does not orchestrate business workflows.

---

## 3.2 Immutable Projections

ViewModels are immutable snapshots.

---

## 3.3 Platform Independence

Stable public contracts contain no native UI objects.

---

## 3.4 Authority Preservation

UI Adapter never becomes authoritative for domain state.

---

## 3.5 Intent over Native Event

Business/application contracts receive semantic user intent, not raw UI framework events.

---

# 4. Contract Domains

UI Adapter v2 separates three public contract families:

```text id="i9ivxm"
UI Adapter Contracts
├── UI-Local Contracts
├── Application Intent Contracts
└── Projection / Query Contracts
```

This prevents local UI behavior from being confused with business commands.

---

# 5. ViewId

```text id="qtqi6f"
ViewId
- value
```

Represents a logical UI view.

Examples:

```text id="erxdyo"
Reader
Settings
Diagnostics
History
SourcePicker
```

Rules:

* stable logical identifier;
* framework-independent;
* not a URL/route object;
* not a native window handle.

---

# 6. ViewStateSnapshot

Conceptually:

```text id="ex59r1"
ViewStateSnapshot
├── viewId
├── lifecycleState
├── viewModelRevision?
├── visible
├── focused?
├── uiLocalState?
└── observedAt
```

This is UI-local state only.

---

# 7. UiIntent

```text id="jql64u"
UiIntent
├── intentId
├── intentType
├── sourceViewId?
├── correlationId?
├── payload?
└── occurredAt
```

Examples:

```text id="i94u0c"
StartReadingIntent
PauseReadingIntent
ResumeReadingIntent
StopReadingIntent

ChangeReadingSourceIntent
ChangeSessionConfigurationIntent

SavePreferenceIntent
RemovePreferenceIntent

RetryCurrentOperationIntent

ExportDiagnosticBundleIntent
```

---

# 8. UiIntent Rules

A UiIntent:

* represents user intention;
* is immutable;
* contains no native control object;
* contains no business decision;
* does not imply successful execution.

---

# 9. UiActionResult

Conceptually:

```text id="qctf83"
UiActionResult
├── intentId
├── status
├── resultingViewModelRef?
├── domainResultRef?
├── errorProjection?
└── completedAt
```

Possible status:

```text id="z2jjdd"
Accepted
Completed
Rejected
Failed
NoOp
```

---

# 10. UI-Local Commands

UI-local commands control adapter/view behavior only.

Recommended:

```text id="ty0tfl"
OpenView
CloseView
Navigate
ShowDialog
CloseDialog
ShowNotification
DismissNotification
ApplyTheme
ApplyLocalization
```

These do not represent business-domain actions by themselves.

---

# 11. OpenView

```text id="bdgrq7"
OpenView
├── viewId
├── navigationContext?
└── correlationId?
```

Result:

```text id="f9cu9m"
ViewStateSnapshot
```

---

# 12. CloseView

```text id="v3kpq6"
CloseView
├── viewId
└── reason?
```

Closing a view does not automatically stop the business operation represented by that view.

---

# 13. Navigate

```text id="1ntt7c"
Navigate
├── targetViewId
├── navigationContext?
├── replaceCurrent?
└── correlationId?
```

Navigation remains logical.

No route/framework objects cross the stable boundary.

---

# 14. ShowDialog

```text id="r4309r"
ShowDialog
├── dialogModel
└── correlationId?
```

This displays an already-defined UI dialog model.

It does not create business authorization.

---

# 15. DialogResponse

```text id="ddmyiw"
DialogResponse
├── dialogId
├── selectedActionId
├── cancelled
└── respondedAt
```

The UI Adapter may translate this into a semantic UiIntent.

---

# 16. ShowNotification

```text id="vsij90"
ShowNotification
├── notificationModel
└── correlationId?
```

Notification display is UI behavior.

---

# 17. ApplyTheme

Preferred replacement for v1:

```text id="k9mlgp"
UpdateTheme
```

Conceptually:

```text id="g4d0ph"
ApplyTheme
├── themeDescriptor
└── sourcePreferenceRevision?
```

UI Adapter applies the already-resolved appearance.

It does not persist the user's theme preference.

---

# 18. ApplyLocalization

Preferred replacement for:

```text id="e7t9ww"
UpdateLocalization
```

Conceptually:

```text id="wscj3o"
ApplyLocalization
├── locale
├── resourceSetRef?
└── sourcePreferenceRevision?
```

Persistent language preference remains outside UI Adapter ownership.

---

# 19. Application-Facing Intents

Cross-module user actions should normally be expressed as application-level intents.

Examples:

```text id="l2vbww"
StartReadingIntent
StopReadingIntent
ChangeReaderSourceIntent
RetryCurrentOperationIntent
ExportSupportBundleIntent
```

UI Adapter does not manually orchestrate their underlying modules.

---

# 20. StartReadingIntent

```text id="gr16xx"
StartReadingIntent
├── sourceSelection?
├── initialReadingContext?
├── sessionConfigurationOverrides?
└── correlationId?
```

Application decides the full use-case flow.

---

# 21. StopReadingIntent

```text id="4qee22"
StopReadingIntent
├── sessionId?
├── reason?
└── correlationId?
```

UI Adapter does not directly dispose arbitrary Runtime work.

---

# 22. PauseReadingIntent

```text id="h7yz56"
PauseReadingIntent
├── sessionId
└── correlationId?
```

The application/session contract validates whether pause is allowed.

---

# 23. ResumeReadingIntent

```text id="qnbcq5"
ResumeReadingIntent
├── sessionId
└── correlationId?
```

---

# 24. ChangeSessionConfigurationIntent

```text id="o2mol8"
ChangeSessionConfigurationIntent
├── sessionId
├── changes
├── expectedReadingContextRevision?
└── correlationId?
```

The UI Adapter does not validate Reading Session domain semantics authoritatively.

---

# 25. SavePreferenceIntent

```text id="jmd3ze"
SavePreferenceIntent
├── key
├── value
├── persistentScope
├── scopeIdentity?
├── expectedPreferenceRevision?
└── correlationId?
```

Allowed persistent scopes are defined by Preferences.

---

# 26. Session-Only Preference Intent

For temporary session configuration:

```text id="st4jbf"
ChangeSessionPreferenceIntent
├── sessionId
├── key
├── value
├── expectedReadingContextRevision?
└── correlationId?
```

This goes to Reading Session/Application, not persistent Preferences storage.

---

# 27. Removed `SubmitPreferenceChange`

The v1 command:

```text id="t0euwy"
SubmitPreferenceChange
```

is too ambiguous because it does not distinguish:

```text id="q1w8qy"
Global preference
Source preference
Session-only override
```

v2 uses explicit intent semantics.

---

# 28. Removed `StartReadingSession`

The v1:

```text id="o7981w"
StartReadingSession
```

implies direct UI Adapter → Reading Session ownership.

Preferred:

```text id="7lkjzy"
StartReadingIntent
    ↓
Application use case
```

The Application may create/init Reading Session as part of that flow.

---

# 29. Removed `RetryPipeline`

The v1 command:

```text id="u6rujj"
RetryPipeline
```

is removed.

Reason:

UI Adapter does not own pipeline topology or retry authority.

Preferred:

```text id="l1c0oc"
RetryCurrentOperationIntent
```

---

# 30. RetryCurrentOperationIntent

```text id="4ku89k"
RetryCurrentOperationIntent
├── sessionId?
├── failedOperationRef?
├── displayedErrorRef?
├── correlationId?
└── userInitiated = true
```

The receiving Application/Runtime layer decides:

```text id="b5jndh"
whether retry is allowed
what work should be retried
whether a new Attempt is created
what backoff/policy applies
```

---

# 31. No Stage Identifier Required from UI

UI should not normally need to know:

```text id="byrs2d"
Capture stage
Recognition stage
Translation Attempt ID
Scheduler queue
```

unless developer/diagnostic UX explicitly exposes such data.

Retry semantics remain high-level.

---

# 32. Projection Contract

UI Adapter consumes authoritative snapshots/results and constructs UI-facing ViewModels.

Conceptually:

```text id="ngycgn"
ProjectViewModel
├── viewId
├── sourceSnapshots[]
├── previousViewModelRevision?
└── projectionContext?
```

Returns:

```text id="bzaijb"
ViewModel
```

---

# 33. ViewModel

```text id="dck60g"
ViewModel
├── viewModelId
├── viewId
├── revision?
├── displayState
├── availableActions[]
├── localizationMetadata
├── accessibilityMetadata
└── safeMetadata?
```

---

# 34. ViewModel Invariants

Every ViewModel MUST:

1. be immutable;
2. contain only presentation/UI-facing data;
3. contain no business behavior;
4. contain no native UI controls;
5. contain no provider SDK objects;
6. remain safe for display;
7. preserve source revision/provenance where required.

---

# 35. ReaderViewModel

Conceptually:

```text id="0cag92"
ReaderViewModel
├── sessionSummary
├── sourceSummary
├── readingPositionSummary
├── presentationView
├── progressView?
├── warningView?
├── availableActions[]
└── accessibilityMetadata
```

---

# 36. SettingsViewModel

```text id="w3g9ky"
SettingsViewModel
├── categories[]
├── preferenceItems[]
├── validationFeedback[]
├── saveActions[]
└── localizationMetadata
```

---

# 37. PreferenceItemViewModel

```text id="jpw02g"
PreferenceItemViewModel
├── key
├── labelKey
├── descriptionKey?
├── controlType
├── displayedValue
├── allowedValues?
├── validationState?
├── scopeDisplay?
└── accessibilityMetadata
```

UI control type is UI metadata.

Preference validation remains Preferences-owned.

---

# 38. DiagnosticsViewModel

```text id="6a7w87"
DiagnosticsViewModel
├── overallHealth
├── componentHealth[]
├── recentIssues[]
├── diagnosticCapabilities[]
├── supportActions[]
└── observedAt
```

---

# 39. ErrorViewModel

```text id="vgmtcp"
ErrorViewModel
├── ownerModule
├── originalErrorCode
├── titleKey
├── messageKey
├── severity
├── recoveryActions[]
├── diagnosticRef?
└── safeParameters?
```

---

# 40. Error Ownership Preservation

Example:

```text id="gm3ik1"
SES-REV-001
ReadingContextRevisionConflict
```

becomes:

```text id="fwctn0"
ErrorViewModel
ownerModule = reading-session
originalErrorCode = SES-REV-001
```

It does not become:

```text id="pcuclo"
UIA-ERR-GenericFailure
```

---

# 41. PresentationViewModel

UI Adapter may adapt Presentation output into:

```text id="xy1it5"
PresentationViewModel
```

but must preserve Presentation semantics.

Conceptually:

```text id="2nemzd"
PresentationViewModel
├── presentationRevision?
├── contentBlocks[]
├── geometry[]
├── styleTokens[]
├── visibilityState
└── interactionHints?
```

---

# 42. Presentation Boundary

UI Adapter MUST NOT:

```text id="urr0lz"
recompute bubble fitting
reorder text
change translation
change semantic layout
mutate Presentation Artifact
```

---

# 43. Current View Query

```text id="gxce7i"
GetCurrentView()
→ ViewStateSnapshot?
```

UI-local read only.

---

# 44. GetViewModel

```text id="an05ag"
GetViewModel
- viewId

→ ViewModel?
```

Returns the latest UI projection known to the adapter.

It is not authoritative application state.

---

# 45. GetNavigationState

Preferred query:

```text id="xwt2q0"
GetNavigationState()
→ NavigationStateSnapshot
```

instead of exposing native routes/windows.

---

# 46. GetAppliedAppearance

Preferred replacement for ambiguous:

```text id="8tp558"
GetTheme
```

Conceptually:

```text id="n2wl77"
GetAppliedAppearance()
→ AppliedAppearanceSnapshot
```

This reports UI-applied appearance, not persisted Preferences authority.

---

# 47. GetAppliedLocalization

Preferred replacement for:

```text id="1ghvzt"
GetLocalization
```

Returns:

```text id="tqwy26"
AppliedLocalizationSnapshot
```

It reports currently applied UI localization.

---

# 48. GetWindowState

May remain only as a UI-local query.

```text id="5qqel0"
GetWindowState
├── windowId?
└── frontendId?
```

Returns:

```text id="3ntfbp"
WindowStateSnapshot
```

No native handle is exposed.

---

# 49. WindowStateSnapshot

```text id="yzp3ob"
WindowStateSnapshot
├── windowId
├── bounds?
├── visibility
├── maximized?
├── focused?
└── observedAt
```

Only UI-local properties belong here.

---

# 50. Native Handle Prohibition

Public contract MUST NOT contain:

```text id="vo48si"
HWND
DOM node
HTMLElement
Flutter BuildContext
Qt Widget pointer
React component
browser tab object
native window pointer
```

---

# 51. Platform Capabilities

```text id="o3zc8o"
UiCapability
- Windowing
- OverlayWindow
- SystemNotification
- Clipboard
- FilePicker
- ScreenRegionPicker
- ApplicationWindowPicker
- KeyboardShortcut
- Accessibility
- Localization
```

Capability existence may differ by platform.

---

# 52. UiCapabilitySnapshot

```text id="8nlbv3"
UiCapabilitySnapshot
├── capability
├── available
├── degraded?
├── reasonCode?
└── observedAt
```

---

# 53. Capability Is Not Business Authorization

Example:

```text id="4rw2yc"
ScreenRegionPicker available = true
```

does not imply:

```text id="vk0rpk"
Capture permission granted
```

Capture/platform permission owner remains authoritative.

---

# 54. Platform Contract

Stable contracts are platform-neutral.

Implementations may exist for:

```text id="sxg96o"
Desktop
Web
Mobile
BrowserExtension
```

The contract semantics remain the same even when capabilities differ.

---

# 55. AccessibilityMetadata

Conceptually:

```text id="q8hvpj"
AccessibilityMetadata
├── role
├── accessibleNameKey?
├── descriptionKey?
├── focusOrderHint?
├── keyboardActionHints[]
└── liveRegionHint?
```

No native accessibility API object crosses the stable boundary.

---

# 56. LocalizationMetadata

```text id="ie5gpp"
LocalizationMetadata
├── locale
├── textKeys[]
├── formattingContext?
└── direction?
```

---

# 57. Localization Resource Ownership

UI Adapter may consume localization resources through an abstraction.

It should not expose:

```text id="39uotw"
resource file object
framework localization context
native bundle object
```

---

# 58. NavigationModel

```text id="s1vnny"
NavigationModel
├── currentViewId
├── backStack[]
├── forwardStack[]
├── availableDestinations[]
└── navigationRevision?
```

Navigation is UI-local.

---

# 59. DialogModel

```text id="hav29v"
DialogModel
├── dialogId
├── dialogType
├── titleKey
├── messageKey
├── actions[]
├── severity?
├── dismissPolicy?
└── accessibilityMetadata
```

---

# 60. NotificationModel

```text id="8c5chj"
NotificationModel
├── notificationId
├── notificationType
├── titleKey?
├── messageKey
├── severity
├── actions[]
├── expiryPolicy?
└── accessibilityMetadata?
```

---

# 61. Event Boundary

UI Adapter does not use the business Event Bus for all UI activity.

Distinguish:

```text id="vnntj6"
Native UI Events
UI-local Adapter Events
Application/Domain Events
Business Event Bus Events
```

---

# 62. Removed Mandatory Consumed Events

The v1 mandatory table is removed:

```text id="q7erj9"
ReadingSessionChanged
EffectivePreferencesChanged
PresentationUpdated
DiagnosticsUpdated
StorageReady
```

UI Adapter may use explicit subscriptions, queries, projections, or Application snapshots depending on architecture.

---

# 63. Why `EffectivePreferencesChanged` Is Removed

Preferences v2 no longer exposes one global:

```text id="5eb9kb"
EffectivePreferencesChanged
```

because effective snapshots are contextual.

UI state should receive the appropriate resolved/application snapshot.

---

# 64. Why `DiagnosticsUpdated` Is Removed

Diagnostics v2 exposes:

```text id="h0nwj4"
DiagnosticHealthSnapshot
DiagnosticCapabilities
Diagnostic-owned state events
```

There is no generic mandatory:

```text id="n33ol6"
DiagnosticsUpdated
```

event.

---

# 65. Why `StorageReady` Is Removed

UI Adapter should not depend directly on Storage lifecycle for correctness.

Application/storage status projections may be provided when UI requires them.

---

# 66. Presentation Updates

UI may observe Presentation-owned facts/snapshots as needed.

However UI Adapter should preferably receive an application-level projection rather than directly binding to every Presentation event.

---

# 67. Published UI Events

The v1 events:

```text id="pavixz"
ViewOpened
ViewClosed
NavigationCompleted
DialogConfirmed
DialogCancelled
NotificationShown
ThemeChanged
LocalizationChanged
```

are not automatically business Event Bus events.

Most are UI-local events.

---

# 68. UI-Local Event Contract

Possible local event:

```text id="gwzovb"
UiLocalEvent
├── eventId
├── eventType
├── frontendId?
├── viewId?
├── occurredAt
└── safeMetadata?
```

Use only within UI/application adapter infrastructure.

---

# 69. When UI Event May Become Business Event

Only when:

1. a stable business fact exists;
2. another module needs asynchronous awareness;
3. UI Adapter owns that fact;
4. the event is not merely interaction telemetry.

This should be rare.

---

# 70. Dialog Confirmation Flow

Correct:

```text id="b6j363"
DialogConfirmed
    ↓
UI Adapter local event
    ↓
semantic UiIntent
    ↓
Application command
```

The local dialog event itself does not need to enter the global Event Bus.

---

# 71. Theme Change Flow

If theme is a Preferences-owned value:

```text id="71hbqf"
SavePreferenceIntent
    ↓
Preferences commit
    ↓
Application effective appearance
    ↓
ApplyTheme
```

A UI-owned global `ThemeChanged` domain event is normally unnecessary.

---

# 72. Localization Change Flow

Likewise:

```text id="pm1ewa"
persistent locale preference
    ↓
Preferences/Application
    ↓
ApplyLocalization
```

UI Adapter reports applied UI state separately.

---

# 73. Error Contract

UI Adapter owns only UI adaptation failures.

Conceptually:

```text id="tv66sq"
UiAdapterError
├── errorCode
├── category
├── severity
├── recoveryHint?
├── viewId?
├── intentId?
├── capability?
├── diagnosticRef?
└── safeMetadata?
```

---

# 74. UI Adapter Error Categories

Recommended:

```text id="1f7aez"
Intent
Navigation
Projection
View
Dialog
Notification
Localization
Accessibility
PlatformCapability
Internal
```

---

# 75. UI Adapter-Owned Errors

Examples:

```text id="42xx9r"
InvalidUiIntent
ViewUnavailable
InvalidNavigation
ViewModelConstructionFailed
DialogPresentationFailed
NotificationPresentationFailed
LocalizationUnavailable
AccessibilityCapabilityUnavailable
PlatformBindingUnavailable
UnsupportedUiCapability
UiAdapterInvariantViolation
```

---

# 76. Removed Generic `RenderingFailed`

The v1:

```text id="8f3whu"
RenderingFailed
```

is too broad.

Native rendering failure should be scoped where possible:

```text id="di4d3m"
ViewRenderFailed
OverlayRenderFailed
PlatformBindingUnavailable
```

Detailed taxonomy belongs to `ERRORS.md`.

---

# 77. Removed Generic `ResourceMissing`

The v1:

```text id="jf5mut"
ResourceMissing
```

is ambiguous.

Prefer:

```text id="k6bsxi"
LocalizationUnavailable
ViewResourceUnavailable
PlatformCapabilityUnavailable
```

depending on ownership.

---

# 78. Unsupported Platform vs Capability

Prefer capability-specific failure over whole-platform rejection.

Example:

```text id="gzvgzk"
Browser Extension
supports reader panel
but not native overlay window
```

Return:

```text id="uux87u"
UnsupportedUiCapability
capability = OverlayWindow
```

instead of:

```text id="f0yfw7"
UnsupportedPlatform
```

where possible.

---

# 79. External Error Projection

Errors from:

```text id="wnqrfb"
Reading Session
Preferences
Capture
Recognition
Translation
Runtime
Diagnostics
Storage
```

remain externally owned.

UI Adapter only converts them into display models.

---

# 80. Security Contract

UI Adapter must never expose:

```text id="ug3uq4"
secret
provider credential
token
private certificate
raw internal persistence details
unsafe diagnostics
native handle
```

---

# 81. Content Privacy

UI-facing values must respect the intended surface.

Especially avoid raw reading content in:

```text id="xakwip"
system notifications
window titles
diagnostic labels
clipboard
accessibility labels
```

unless explicitly required.

---

# 82. Clipboard Contract

Conceptually:

```text id="j05xvg"
CopyToClipboardIntent
├── contentRef/value
├── userInitiated = true
└── purpose?
```

Clipboard actions must be explicit.

---

# 83. File Picker Contract

UI Adapter may expose:

```text id="12f3ay"
SelectLocalInputIntent
```

which returns a platform-neutral input reference.

Native file handles should not escape the adapter boundary.

---

# 84. Screen Region Selection Contract

Conceptually:

```text id="85g3pm"
SelectCaptureRegionIntent
```

UI Adapter manages the platform interaction.

Result is normalized before being passed to Application/Capture contracts.

---

# 85. Application Window Selection Contract

Conceptually:

```text id="ktvczt"
SelectApplicationWindowIntent
```

Stable output should use an opaque platform source reference, not a native handle.

Capture later owns `CaptureSource`.

---

# 86. Async Interaction Contract

Long-running application actions return asynchronously through:

```text id="gshvi8"
result
snapshot
projection update
or state event
```

UI Adapter must not block the UI thread waiting for processing completion.

---

# 87. Loading State

ViewModels may contain:

```text id="z4ga8o"
loading
submitting
awaitingResult
```

as UI projection state.

These are not UI Adapter module lifecycle states.

---

# 88. Optimistic UI

An optimistic ViewModel may be produced before business commit.

It must be clearly provisional.

On rejection:

```text id="v8d8xe"
domain result
    ↓
authoritative projection
    ↓
replacement ViewModel
```

---

# 89. Concurrency

UI Adapter may suppress duplicate gestures.

Authoritative concurrency remains with Application/domain contracts.

---

# 90. Request Identity

Application-facing intents should include:

```text id="11yjmh"
intentId
correlationId?
```

Where downstream idempotency requires a separate requestId, Application may derive or propagate it according to contract.

---

# 91. ViewModel Revision

A UI projection may have:

```text id="bg3fb1"
viewModelRevision
```

for UI-local ordering.

This revision is not:

```text id="ockuhk"
ReadingContextRevision
PreferenceRevision
RuntimeRevisionId
PresentationRevision
```

---

# 92. Source Revision Provenance

A ViewModel may record source revisions:

```text id="14hcng"
readingContextRevision
preferenceRevision
presentationRevision
diagnosticSnapshotVersion?
```

for staleness detection.

UI Adapter does not create those revisions.

---

# 93. Stale Projection Handling

If a newer authoritative source snapshot exists:

```text id="cwib5z"
old ViewModel
    ↓
discard/replace
    ↓
new ViewModel
```

UI Adapter must not modify domain state to conform to stale UI.

---

# 94. Serialization

Stable contracts must not contain:

```text id="2iqzka"
callback closure
framework component
native widget
file handle
DOM object
SDK object
mutable domain entity
```

---

# 95. Versioning

Public contract uses:

```text id="3gfmwc"
MAJOR.MINOR.PATCH
```

Major change required for incompatible:

* UiIntent semantics;
* ViewModel ownership;
* application boundary;
* platform-reference semantics;
* error ownership;
* privacy contract.

---

# 96. Compatibility

Compatible additions may include:

```text id="sdwojd"
optional ViewModel fields
optional safe metadata
new optional UI capabilities
new optional UiIntent types
```

Unknown required enum values must be rejected or handled according to explicit compatibility rules.

---

# 97. Example — Start Reading

```text id="0qjvo8"
User clicks Start
    ↓
Native UI event
    ↓
StartReadingIntent
    ↓
Application
    ↓
Reading Session / Preferences / Runtime orchestration
    ↓
Application snapshot
    ↓
ReaderViewModel
```

UI Adapter does not orchestrate the middle steps.

---

# 98. Example — Preference Change

```text id="avcw7i"
User changes target language globally
    ↓
SavePreferenceIntent
scope = Global
    ↓
Preferences
    ↓
Preference result
    ↓
Application projection
    ↓
SettingsViewModel
```

---

# 99. Example — Session Override

```text id="74qqho"
User selects target language
"only for this reading session"
    ↓
ChangeSessionPreferenceIntent
    ↓
Reading Session/Application
```

No persistent Preferences Session scope is created.

---

# 100. Example — Retry

```text id="uyc9bd"
User presses Retry
    ↓
RetryCurrentOperationIntent
    ↓
Application / Runtime
    ↓
retry policy evaluates
```

UI Adapter does not call:

```text id="vi7akg"
ReadingSession.RetryPipeline()
```

---

# 101. Example — Presentation

```text id="wng10e"
Presentation Artifact/Snapshot
    ↓
Application projection
    ↓
UI Adapter
    ↓
PresentationViewModel
    ↓
native renderer
```

No Presentation semantic recomputation occurs.

---

# 102. Example — Error

```text id="2n2n11"
Translation
    ↓
TRN-PROV-003
    ↓
Application
    ↓
UI Adapter
    ↓
ErrorViewModel
```

Original error ownership is preserved.

---

# 103. Example — Diagnostics

```text id="sw1uqr"
GetDiagnosticHealth
    ↓
DiagnosticHealthSnapshot
    ↓
UI Adapter
    ↓
DiagnosticsViewModel
```

---

# 104. Architecture Invariants

1. UI Adapter is an adapter, not a business orchestrator.

2. UiIntent represents user intention.

3. Native UI events do not cross stable application contracts directly.

4. Cross-module actions prefer Application-level intents.

5. `RetryPipeline` is removed.

6. Retry intent does not define Runtime retry mechanics.

7. UI Adapter does not create WorkItems.

8. UI Adapter does not create Attempts.

9. UI Adapter does not own Reading Session lifecycle.

10. UI Adapter does not own persistent Preferences.

11. Session-only configuration routes to Reading Session/Application.

12. ViewModels are immutable.

13. ViewModels are non-authoritative.

14. ViewModels contain no business logic.

15. ViewModels contain no native UI objects.

16. Presentation semantics remain Presentation-owned.

17. UI Adapter does not recompute Presentation output.

18. Application/domain errors retain original ownership.

19. UI Adapter errors describe UI adaptation failures only.

20. Theme application is distinct from theme persistence.

21. Localization application is distinct from localization preference persistence.

22. UI-local events are not automatically business Event Bus events.

23. `EffectivePreferencesChanged` is not a mandatory consumed event.

24. `DiagnosticsUpdated` is not a mandatory consumed event.

25. `StorageReady` is not a mandatory consumed event.

26. UI Adapter may use query/snapshot projection rather than event-only updates.

27. Stable contracts are platform-neutral.

28. Native handles never cross stable contracts.

29. UI capability does not imply business permission.

30. UI accessibility metadata remains platform-neutral.

31. UI localization metadata remains platform-neutral.

32. Heavy processing does not execute inside UI Adapter.

33. UI thread must not block on long-running Runtime processing.

34. Sensitive data exposure is surface-aware.

35. Clipboard actions require explicit user intent.

36. System notifications must remain privacy-safe.

37. Optimistic UI never becomes authoritative business state.

38. Authoritative concurrency remains downstream.

39. Revision provenance is typed and non-authoritative to UI Adapter.

40. Public contracts remain serializable.

---

# 105. Testing — Intent Mapping

Verify native interactions map to the correct:

```text id="krsjzz"
UiIntent
```

without embedding business policy.

---

# 106. Testing — Application Boundary

Verify:

```text id="lf4dvx"
StartReadingIntent
RetryCurrentOperationIntent
```

do not trigger direct module orchestration inside UI Adapter.

---

# 107. Testing — Preference Scope

Verify UI actions distinguish:

```text id="1u6888"
Global
Source
Session-only
```

and route to the correct owner.

---

# 108. Testing — ViewModel

Verify:

```text id="p6e4vw"
immutability
serializability
platform neutrality
no native objects
no mutable domain entities
```

---

# 109. Testing — Error Ownership

Inject domain errors and verify:

```text id="2iwi6l"
ownerModule
originalErrorCode
```

remain intact in ErrorViewModel.

---

# 110. Testing — Platform Capability

Disable optional capability such as:

```text id="4hhyqr"
SystemNotification
```

Verify main UI remains usable and receives capability-specific failure.

---

# 111. Testing — Event Boundary

Verify:

```text id="2uxqpf"
ViewOpened
DialogConfirmed
NotificationShown
```

remain UI-local by default and do not enter business Event Bus automatically.

---

# 112. Testing — Retry

Verify UI Adapter never chooses:

```text id="dt54vd"
stage
WorkItem
Attempt
backoff
retry count
```

for `RetryCurrentOperationIntent`.

---

# 113. Testing — Privacy

Verify no unsafe content leaks through:

```text id="qxl1wj"
window title
notification
clipboard
accessibility metadata
diagnostics
```

without explicit allowed behavior.

---

# 114. Related Documents

```text id="mtn0o7"
doc/02-modules/ui-adapter/MODULE.md
doc/02-modules/ui-adapter/STATES.md
doc/02-modules/ui-adapter/EVENTS.md
doc/02-modules/ui-adapter/ERRORS.md
doc/02-modules/ui-adapter/README.md

doc/02-modules/reading-session/
doc/02-modules/preferences/
doc/02-modules/presentation/
doc/02-modules/diagnostics/

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md
doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
```

---

# 115. Completion Criteria

This contract is synchronized when:

* UI-local operations are separated from application intents;
* `RetryPipeline` is removed;
* `StartReadingSession` is replaced by application-level intent semantics;
* preference changes distinguish Global/Source/Session ownership;
* theme application is separated from preference persistence;
* localization application is separated from persistent preference ownership;
* ViewModels are immutable and non-authoritative;
* native UI/framework objects are absent;
* domain errors preserve ownership;
* generic mandatory consumed-event list is removed;
* UI lifecycle events remain local by default;
* capability-specific platform behavior is explicit;
* long-running work remains asynchronous;
* privacy constraints cover user-facing surfaces.

---

# 116. Summary

UI Adapter v2 exposes three boundaries.

UI-local behavior:

```text id="w1c1d1"
OpenView
Navigate
ShowDialog
ShowNotification
ApplyTheme
ApplyLocalization
```

Application intent:

```text id="pl53f7"
User Interaction
    ↓
UiIntent
    ↓
Application / Module Command
```

Projection:

```text id="jsmb5l"
Authoritative Snapshot
    ↓
UI Adapter
    ↓
Immutable ViewModel
```

The central contract rule is:

```text id="60hf3h"
UI Adapter translates
user intent and application state.

It does not own
business decisions,
Runtime retry,
or domain authority.
```
