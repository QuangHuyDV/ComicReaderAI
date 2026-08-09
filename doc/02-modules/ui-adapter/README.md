# UI Adapter Module

> **Project:** CRAI
> **Module:** `ui-adapter`
> **Path:** `doc/02-modules/ui-adapter/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Overview

The UI Adapter Module is CRAI's application-facing presentation adapter.

It isolates supported UI technologies from the CRAI core architecture by translating between:

```text
Native / Platform UI
        ↕
UI Adapter
        ↕
Application / Domain / Runtime Contracts
```

Inbound:

```text
Native UI Event
    ↓
UiIntent
    ↓
UI Adapter
    ↓
Application / Module Command
```

Outbound:

```text
Application / Module Snapshot
        ↓
UI Adapter
        ↓
Immutable ViewModel
        ↓
Native UI
```

The UI Adapter exists so that CRAI's business architecture remains independent from:

```text
desktop UI frameworks
web frameworks
mobile frameworks
browser-extension APIs
native windowing APIs
platform accessibility APIs
```

---

# 2. Central Architecture Rule

```text
UI Adapter
    owns adaptation

Application
    owns use-case coordination

Domain Modules
    own business semantics

Runtime
    owns execution

Platform UI
    owns native rendering
```

The UI Adapter translates between these layers.

It does not become any of them.

---

# 3. Module Identity

```text
Module ID: ui-adapter
Module Type: Application Adapter / Presentation Boundary
Primary Domain: UI-facing adaptation
Business Authority: None
Runtime Authority: None
Persistence Authority: None
Native Rendering Authority: Platform UI
MVP Priority: Required
```

UI Adapter is not:

```text
Reading Session
Preferences
Presentation Domain
Business Pipeline Orchestration
Runtime Controller
Storage
Diagnostics
Native UI Framework
```

---

# 4. Primary Responsibilities

UI Adapter owns:

```text
UiIntent normalization
application-command adaptation
query adaptation
ViewModel projection
UI-local view state
logical navigation
dialog models
notification models
frontend capability adaptation
localization application
appearance application
accessibility metadata adaptation
platform binding abstraction
UI-safe error projection
```

---

# 5. Explicit Non-Responsibilities

UI Adapter MUST NOT:

* execute OCR;
* execute Translation;
* execute Capture;
* execute Text Processing;
* own Reading Session lifecycle;
* own persistent Preferences;
* validate domain preference rules authoritatively;
* own Presentation semantics;
* create Runtime WorkItems;
* create Runtime Attempts;
* execute Runtime retry;
* restart processing pipelines;
* determine which processing stage must rerun;
* select providers;
* directly access persistence implementation;
* become the owner of errors produced by other modules.

---

# 6. Architecture Position

Preferred architecture:

```text
┌─────────────────────────────┐
│ Native / Platform UI        │
│                             │
│ Desktop                     │
│ Web                         │
│ Browser Extension           │
│ Mobile                      │
└──────────────┬──────────────┘
               ↕
        ┌───────────────┐
        │  UI Adapter   │
        └───────┬───────┘
                ↕
        ┌───────────────────┐
        │ Application Layer │
        └────────┬──────────┘
                 ↓
   ┌───────────────────────────────┐
   │ Domain / Runtime Modules      │
   │                               │
   │ Reading Session               │
   │ Preferences                   │
   │ Presentation                  │
   │ Diagnostics                   │
   │ Runtime / Pipeline            │
   │ Storage contracts             │
   └───────────────────────────────┘
```

The preferred cross-module UI path goes through Application rather than making UI Adapter an orchestrator.

---

# 7. Why the Application Layer Matters

A user action may require several owners.

Example:

```text
Start Reading
```

may involve:

```text
Reading Session
Preferences
Runtime Configuration
Business Pipeline
Presentation
```

UI Adapter must not manually coordinate those modules.

Preferred:

```text
Start button
    ↓
StartReadingIntent
    ↓
Application
    ↓
use-case orchestration
```

---

# 8. UI Adapter as Inbound Adapter

The inbound responsibility is:

```text
User Interaction
    ↓
Native UI Event
    ↓
UiIntent
    ↓
Application / Module Command
```

The adapter converts implementation-specific UI interaction into semantic intent.

---

# 9. Native UI Events

Examples:

```text
ButtonClicked
PointerMoved
KeyPressed
TextChanged
WindowResized
ScrollChanged
FocusChanged
```

These remain inside the UI/platform implementation.

They do not automatically become CRAI business events.

---

# 10. UiIntent

UiIntent represents what the user wants.

Examples:

```text
StartReadingIntent
PauseReadingIntent
ResumeReadingIntent
StopReadingIntent

ChangeReaderSourceIntent
ChangeSessionConfigurationIntent

SavePreferenceIntent
ChangeSessionPreferenceIntent

RetryCurrentOperationIntent

ExportDiagnosticBundleIntent
```

UiIntent contains no Runtime retry mechanics or business decision.

---

# 11. UiIntent vs Domain Command

Example:

```text
Retry button
    ↓
RetryCurrentOperationIntent
```

does not mean:

```text
retry Translation stage
retry Attempt A32
restart pipeline
```

Application/Runtime determines the actual consequence.

---

# 12. Removed Retry Pipeline Ownership

UI Adapter v2 does not call:

```text
ReadingSession.RetryPipeline()
```

or equivalent.

Preferred:

```text
User Retry Intent
    ↓
UI Adapter
    ↓
Application / Runtime-facing contract
    ↓
Runtime retry policy
```

---

# 13. UI Adapter as Outbound Adapter

Outbound flow:

```text
Authoritative Application / Module State
        ↓
UI projection input
        ↓
UI Adapter
        ↓
Immutable ViewModel
        ↓
Platform UI
```

UI Adapter adapts state for display.

It does not create authoritative business state.

---

# 14. ViewModel

A ViewModel is:

```text
immutable
UI-facing
framework-neutral
non-authoritative
safe for presentation
```

Conceptually:

```text
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

# 15. ViewModel Rules

ViewModels MUST NOT contain:

```text
business behavior
mutable domain entities
native UI controls
provider SDK objects
Storage handles
Runtime workers
framework component instances
```

---

# 16. ViewModel Is Not Business State

Example:

```text
ReaderViewModel.sessionStatus = Active
```

is true because:

```text
Reading Session / Application state
    ↓
ReaderViewModel
```

not because the ViewModel itself defines session state.

---

# 17. ViewModel Immutability

Once published:

```text
ViewModel N
```

never mutates in place.

A UI state change creates:

```text
ViewModel N+1
```

or equivalent immutable replacement.

---

# 18. Candidate ViewModel Isolation

Projection may internally use:

```text
Current ViewModel N
+
Candidate ViewModel N+1
```

Until publication succeeds, consumers continue seeing:

```text
ViewModel N
```

A failed Candidate must not partially replace it.

---

# 19. ReaderViewModel

Possible data:

```text
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

# 20. SettingsViewModel

Possible:

```text
SettingsViewModel
├── categories[]
├── preferenceItems[]
├── validationFeedback[]
├── saveActions[]
└── localizationMetadata
```

Preferences remains validation authority.

---

# 21. DiagnosticsViewModel

Possible:

```text
DiagnosticsViewModel
├── overallHealth
├── componentHealth[]
├── recentIssues[]
├── diagnosticCapabilities[]
└── supportActions[]
```

Diagnostics remains semantic owner of the source data.

---

# 22. ErrorViewModel

Errors from any owner may become:

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

Original ownership must remain visible.

---

# 23. Reading Session Relationship

Reading Session owns:

```text
session identity
session lifecycle
SessionConfiguration
ReadingContext
ReadingContextRevision
```

UI Adapter may:

```text
submit user intents
display session state
display session errors
display progress projections
```

It does not implement Reading Session transitions itself.

---

# 24. Reading Session Error Example

```text
SES-REV-001
ReadingContextRevisionConflict
    ↓
UI Adapter
    ↓
ErrorViewModel
```

The error remains:

```text
owner = reading-session
code = SES-REV-001
```

---

# 25. Preferences Relationship

Preferences owns:

```text
persistent preference definitions
Global preferences
Source preferences
PreferenceRevision
validation
resolution semantics
```

UI Adapter may:

```text
display settings
submit persistent change intent
display validation result
apply resolved appearance/localization
```

---

# 26. Session Preference Relationship

Temporary active-session values belong to Reading Session.

Therefore the UI must distinguish:

```text
Save globally
Save for source
Use only for this session
```

These are different intents with different owners.

---

# 27. Theme / Appearance

UI Adapter applies resolved appearance.

Example:

```text
Preferences/Application
    ↓
resolved appearance
    ↓
ApplyTheme
    ↓
UI Adapter
```

UI Adapter does not become persistent theme preference authority.

---

# 28. Localization

Similarly:

```text
Preferences / Application localization state
        ↓
ApplyLocalization
        ↓
UI Adapter
```

UI Adapter applies locale/resources to the frontend.

Persistent preference ownership remains external.

---

# 29. Presentation Relationship

Presentation owns:

```text
semantic presentation layout
text placement
text fitting
overlay geometry
Presentation Artifact
PresentationRevision
```

UI Adapter owns:

```text
platform-facing adaptation
native-renderer mapping
UI interaction binding
```

---

# 30. Presentation Flow

Preferred:

```text
Presentation
    ↓
Presentation Artifact / Snapshot
    ↓
Application
    ↓
UI Adapter
    ↓
PresentationViewModel
    ↓
Native Renderer
```

---

# 31. UI Adapter Does Not Recompute Presentation

UI Adapter must not:

```text
reorder translated text
fit text into bubbles
recalculate semantic geometry
choose translation layout
change presentation meaning
```

---

# 32. Runtime Relationship

Runtime owns:

```text
RuntimeRevision
WorkItem
Attempt
queueing
deadlines
retry
cancellation
supersession
```

UI Adapter may display Runtime-derived status.

It does not own Runtime behavior.

---

# 33. Runtime Retry

User-facing retry:

```text
RetryCurrentOperationIntent
```

is only a request.

Runtime/Application decides:

```text
whether retry is allowed
what work is retried
whether a new Attempt is required
retry count
backoff
```

---

# 34. Storage Relationship

UI Adapter does not access persistence implementation.

If UI needs:

```text
history
storage status
maintenance status
cleanup
```

it receives safe Application/Storage contract data.

---

# 35. Diagnostics Relationship

UI Adapter may display:

```text
DiagnosticHealthSnapshot
DiagnosticCapabilities
RecentDiagnosticIssues
```

It does not expose raw diagnostics indiscriminately.

---

# 36. UI Adapter and Diagnostics

UI Adapter itself may instrument:

```text
view projection duration
navigation failure
platform capability failure
frontend availability
```

through Diagnostics abstractions.

It does not publish every UI gesture into the business Event Bus.

---

# 37. UI-Local State

UI Adapter may own ephemeral state such as:

```text
selected tab
navigation stack
dialog visibility
scroll position
expanded section
focus state
temporary form draft
frontend visibility
```

This state has no business authority.

---

# 38. Closing a View vs Stopping Work

Important:

```text
ViewClosed
```

does not automatically mean:

```text
StopReading
CancelRuntime
```

unless Application policy explicitly maps that user action into such intent.

---

# 39. Window State

UI Adapter may own UI-local window state:

```text
position
size
visibility
focus
maximized state
```

It does not own CaptureSource semantics merely because Capture may target a window.

---

# 40. Capture Window Selection

Example:

```text
User selects application window
        ↓
platform selection UI
        ↓
UI Adapter
        ↓
normalized source selection
        ↓
Application / Capture
        ↓
CaptureSource
```

Capture owns the resulting source semantics.

---

# 41. Platform Capabilities

Possible UI capabilities:

```text
Windowing
OverlayWindow
SystemNotification
Clipboard
FilePicker
ScreenRegionPicker
ApplicationWindowPicker
KeyboardShortcut
Accessibility
Localization
```

Availability may differ by frontend/platform.

---

# 42. Capability State

Each capability may be:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
DISABLED
UNKNOWN
```

One capability becoming unavailable does not necessarily disable the entire UI.

---

# 43. Capability vs Platform

v2 prefers capability-oriented compatibility.

Example:

```text
Browser Extension

Reader Panel = available
Settings = available
OverlayWindow = unavailable
```

This is better than declaring:

```text
UnsupportedPlatform
```

for the entire frontend.

---

# 44. Accessibility

Accessibility is a first-class concern.

Stable accessibility metadata may include:

```text
role
accessible-name key
description key
focus-order hint
keyboard-action hints
screen-reader hints
```

Native accessibility API objects remain inside platform implementation.

---

# 45. Localization Keys

Domain/application modules should prefer:

```text
ErrorCode
messageKey
safe parameters
```

rather than hard-coded final strings.

UI/localization infrastructure resolves those into visible localized text.

---

# 46. Event Domains

UI Adapter v2 distinguishes:

```text
Native UI Events
UI-Local Events
UiIntent
Application / Domain Events
```

These must not be merged.

---

# 47. Native UI Events

Examples:

```text
ButtonClicked
ScrollChanged
PointerMoved
FocusChanged
WindowResized
```

remain local to platform UI.

---

# 48. UI-Local Events

Examples:

```text
ViewOpened
ViewClosed
NavigationCompleted
DialogResponded
NotificationShown
AppearanceApplied
LocalizationApplied
FrontendConnected
FrontendDisconnected
```

These remain local by default.

---

# 49. Event Bus Is Not UI Event Bus

Do not publish every:

```text
click
scroll
focus
hover
dialog open
notification
window resize
```

through CRAI Event Bus.

Use UI-local mechanisms or Diagnostics telemetry where appropriate.

---

# 50. UiIntent Is Not Event Bus Command Transport

Preferred:

```text
UiIntent
    ↓
Application command boundary
```

not:

```text
UiIntent event
    ↓
Event Bus subscriber
```

as hidden command transport.

---

# 51. Application Projection

For cross-module screens, preferred:

```text
Domain / Runtime State
        ↓
Application Projection
        ↓
ApplicationUiSnapshot
        ↓
UI Adapter
        ↓
ViewModel
```

This prevents UI Adapter from reconstructing business state by listening to every module event.

---

# 52. No Mandatory Event-Only UI

UI Adapter may update using:

```text
Application projection
snapshot/query
observable state
selected stable domain events
```

The architecture does not require every update to come from Event Bus.

---

# 53. Removed Mandatory Event Dependencies

UI Adapter v2 does not require:

```text
ReadingSessionChanged
EffectivePreferencesChanged
PresentationUpdated
DiagnosticsUpdated
StorageReady
StorageFailed
TranslationCompleted
RecognitionCompleted
```

as mandatory subscriptions.

---

# 54. Effective Preferences

There is no global:

```text
EffectivePreferencesChanged
```

event in Preferences v2.

UI Adapter receives context-appropriate Application/settings/session state.

---

# 55. Diagnostics Updates

There is no generic:

```text
DiagnosticsUpdated
```

dependency.

Use:

```text
DiagnosticHealthSnapshot
DiagnosticCapabilities
selected Diagnostics-owned state events
```

when needed.

---

# 56. Storage Updates

UI Adapter should not bind directly to Storage lifecycle by default.

Application projections should expose storage UX state where necessary.

---

# 57. Processing Completion

UI Adapter should not require direct:

```text
RecognitionCompleted
TranslationCompleted
```

subscriptions merely to render the reader.

Preferred:

```text
Processing
    ↓
Business Pipeline / Presentation
    ↓
Application state
    ↓
UI projection
```

---

# 58. Module Lifecycle

UI Adapter module lifecycle:

```text
UNINITIALIZED
      ↓
INITIALIZING
      ↓
READY
  ↕
DEGRADED
      ↓
STOPPING
      ↓
STOPPED
```

---

# 59. READY

While READY:

```text
views may project
navigation may occur
dialogs may wait
notifications may display
UiIntents may be submitted
multiple frontends may operate
```

These do not require global lifecycle transitions.

---

# 60. Removed Global `RENDERING`

Rendering/projection belongs to a scoped view/operation.

The module remains READY.

---

# 61. Removed Global `NAVIGATING`

Navigation is scoped.

Other UI operations may continue concurrently.

---

# 62. Removed Global `WAITING_FOR_USER`

A dialog may await user response while the rest of the UI continues.

---

# 63. Removed Global `UPDATING`

ViewModel updates are normal READY behavior.

---

# 64. DEGRADED

Possible causes:

```text
optional frontend unavailable
system notifications unavailable
clipboard unavailable
localization fallback
accessibility integration degraded
overlay frontend unavailable
```

Working capabilities remain available.

---

# 65. No Generic Global `FAILED`

A single:

```text
view projection failure
dialog failure
notification failure
overlay failure
```

does not make the whole UI Adapter failed.

Use narrow operation/view/capability state.

---

# 66. Frontend Lifecycle

Possible frontend states:

```text
CREATED
INITIALIZING
ACTIVE
INACTIVE
DEGRADED
DISPOSING
DISPOSED
```

Multiple frontends may coexist.

---

# 67. View Lifecycle

Logical view lifecycle:

```text
CREATED
   ↓
MOUNTED
   ↓
VISIBLE ↔ HIDDEN
   ↓
DISPOSING
   ↓
DISPOSED
```

View lifecycle is independent from business lifecycle.

---

# 68. Scoped UI Operations

Examples:

```text
navigation
ViewModel projection
dialog presentation
appearance application
localization application
UiIntent forwarding
```

Each operation may have its own local phase/outcome.

---

# 69. ViewModel Projection

Typical phases:

```text
READING_SOURCE_SNAPSHOTS
        ↓
BUILDING
        ↓
VALIDATING
        ↓
PUBLISHING
        ↓
COMPLETED
```

Possible outcomes:

```text
STALE
REJECTED
FAILED
```

---

# 70. Stale Projection

A stale Candidate ViewModel must never overwrite a newer projection.

Use typed source provenance and ViewModelRevision where appropriate.

---

# 71. ViewModelRevision

UI Adapter may own:

```text
ViewModelRevision
```

for one projection stream.

It is separate from:

```text
ReadingContextRevision
PreferenceRevision
PresentationRevision
RuntimeRevisionId
```

---

# 72. Error Model

UI Adapter owns only errors about UI adaptation.

Categories:

```text
Intent
Navigation
Projection
View
Dialog
Notification
Localization / Appearance
Accessibility
Capability
Privacy
Internal
```

---

# 73. UI Adapter Error Examples

```text
InvalidUiIntent
InvalidNavigation
ViewModelConstructionFailed
StaleViewModelProjection
ViewRenderFailed
DialogPresentationFailed
NotificationCapabilityUnavailable
LocalizationResourceUnavailable
AccessibilityCapabilityUnavailable
UiCapabilityUnavailable
PlatformBindingUnavailable
UiAdapterInvariantViolation
```

---

# 74. Errors UI Adapter Does Not Own

UI Adapter does not take ownership of:

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
Storage-owned errors
```

---

# 75. Error Projection

External error:

```text
Domain / Runtime Error
    ↓
UI Adapter
    ↓
ErrorViewModel
```

Original owner and ErrorCode remain unchanged.

---

# 76. Removed `RenderingFailed`

v2 distinguishes:

```text
ViewModelConstructionFailed
ViewModelPublicationFailed
ViewRenderFailed
```

because projection and native rendering are different failure domains.

---

# 77. Removed Generic `ResourceMissing`

Resource errors use ownership-specific semantics such as:

```text
ViewResourceUnavailable
LocalizationResourceUnavailable
AppearanceResourceUnavailable
UiCapabilityUnavailable
```

---

# 78. Removed `UnsupportedPlatform`

Prefer:

```text
UiCapabilityUnavailable
FrontendUnavailable
```

so partially supported platforms remain usable.

---

# 79. Removed `EventDispatchFailed`

The v1 name is too ambiguous.

Use the actual failed boundary:

```text
UiIntentForwardingFailed
ViewModelPublicationFailed
UiCapabilityUnavailable
```

or keep optional UI-local event failure internal.

---

# 80. Privacy

UI surfaces have different exposure risks.

Special care is required for:

```text
system notifications
window titles
clipboard
accessibility metadata
logs/diagnostics
browser extension surfaces
```

---

# 81. System Notification Privacy

Avoid raw:

```text
OCR text
translated content
document content
```

in system notifications by default.

Use safe generic summaries where possible.

---

# 82. Clipboard

Clipboard operations require explicit user intent.

UI Adapter must never silently copy reading content.

---

# 83. Window Titles

Avoid using sensitive reading content as application/window title metadata.

---

# 84. Accessibility Privacy

Accessibility metadata should contain only what is required for accessible operation.

Do not leak unrelated sensitive content.

---

# 85. Platform Independence

Stable UI Adapter contracts must remain independent from:

```text
React
Vue
Svelte
Electron
Flutter
Qt
WinUI
WPF
Android View
SwiftUI
browser extension API objects
```

---

# 86. Platform Object Boundary

Stable contracts MUST NOT contain:

```text
HTMLElement
DOM Node
React Component
Flutter Widget
Qt pointer
HWND
native window handle
browser tab object
```

Normalize them behind adapter abstractions.

---

# 87. Supported Frontend Profiles

Potential frontend implementations include:

```text
Windows Desktop
Linux Desktop
macOS Desktop
Web
Browser Extension
Mobile
Overlay Window
```

The core architecture remains unchanged.

---

# 88. Multiple Frontends

CRAI may operate with:

```text
desktop main window
+
overlay frontend
+
browser extension panel
```

simultaneously.

They may share Application/domain state.

Their UI-local state remains independent.

---

# 89. Frontend Failure Isolation

Example:

```text
Overlay frontend = unavailable
Desktop frontend = active
```

does not automatically stop the reading session or Runtime.

Application decides product behavior.

---

# 90. Performance

UI Adapter should prioritize:

```text
responsive interaction
cheap ViewModel projection
bounded UI-local state
incremental native rendering
minimal UI-thread blocking
```

---

# 91. Heavy Work Rule

UI Adapter must not synchronously execute:

```text
OCR
Translation
image processing
large persistence operations
support-bundle generation
```

on the UI thread.

---

# 92. Async Use Cases

Preferred:

```text
User Intent
    ↓
Application / Runtime
    ↓
async state/result
    ↓
new ViewModel
```

---

# 93. Optimistic UI

Optimistic UI may be used as a provisional projection.

It never becomes authoritative business state before downstream commit.

---

# 94. Optimistic Rejection

If downstream rejects:

```text
Optimistic ViewModel
    ↓
authoritative result
    ↓
replacement ViewModel
```

UI Adapter does not rollback business state because it never owned it.

---

# 95. Common Architecture Mistake — UI Orchestration

Wrong:

```text
Start button
    ↓
UI Adapter creates session
    ↓
resolves preferences
    ↓
starts capture
    ↓
starts recognition
```

Correct:

```text
Start button
    ↓
StartReadingIntent
    ↓
Application
```

---

# 96. Common Architecture Mistake — Retry Pipeline

Wrong:

```text
UI Adapter
    ↓
Reading Session.RetryPipeline()
```

Correct:

```text
RetryCurrentOperationIntent
    ↓
Application / Runtime
```

---

# 97. Common Architecture Mistake — Business Logic in ViewModel Mapper

Wrong:

```text
if translation confidence low:
    retry Translation
```

inside UI Adapter.

Correct:

```text
Business Pipeline / module policy
    owns decision
```

UI Adapter only projects the result.

---

# 98. Common Architecture Mistake — UI Owns Error

Wrong:

```text
Translation fails
    ↓
UIA-GenericError
```

Correct:

```text
TRN-* error
    ↓
ErrorViewModel
```

---

# 99. Common Architecture Mistake — Event Bus for Every UI Action

Wrong:

```text
ButtonClicked
ViewOpened
ScrollChanged
DialogOpened
NotificationShown
```

all sent to the global Event Bus.

Correct:

```text
native/local UI event mechanisms
+
UiIntent
+
selected business events
```

---

# 100. Common Architecture Mistake — ViewModel as Authority

Wrong:

```text
ReaderViewModel says session active
therefore session is active
```

Correct:

```text
Reading Session/Application
    ↓
ReaderViewModel says session active
```

---

# 101. Common Architecture Mistake — Theme Ownership

Wrong:

```text
UI Adapter
    owns persistent theme preference
```

Correct:

```text
Preferences
    owns persisted value

UI Adapter
    applies resolved appearance
```

---

# 102. Common Architecture Mistake — Direct Storage Access

Wrong:

```text
Settings UI
    ↓
UI Adapter
    ↓
database/file
```

Correct:

```text
UI Adapter
    ↓
Application / Storage contract
```

---

# 103. Architecture Invariants

1. UI Adapter is an adapter, not an orchestrator.

2. Native UI events remain platform-local.

3. Relevant interaction becomes semantic UiIntent.

4. Cross-module use cases prefer Application-level commands.

5. UI Adapter does not create Runtime WorkItems.

6. UI Adapter does not create Runtime Attempts.

7. UI Adapter does not execute Runtime retry.

8. UI Adapter does not restart pipelines.

9. Reading Session retains session authority.

10. Preferences retains persistent preference authority.

11. Presentation retains presentation semantics.

12. Runtime retains execution authority.

13. Diagnostics retains diagnostic semantics.

14. ViewModels are immutable.

15. ViewModels are non-authoritative.

16. ViewModels contain no business behavior.

17. Candidate ViewModels remain isolated until publication.

18. Stale ViewModels never overwrite newer projections.

19. UI-local state is separate from domain state.

20. View lifecycle is separate from business lifecycle.

21. Native UI objects never cross stable contracts.

22. Application/domain errors retain original ownership.

23. UIA errors describe UI adaptation failure only.

24. Theme application is separate from preference persistence.

25. Localization application is separate from locale preference persistence.

26. Accessibility metadata remains platform-neutral.

27. Event Bus is not used as a global UI event bus.

28. UI-local events remain local by default.

29. UI telemetry uses Diagnostics abstractions where appropriate.

30. UI Adapter does not require direct Recognition/Translation completion subscriptions.

31. UI Adapter does not require `EffectivePreferencesChanged`.

32. UI Adapter does not require generic `DiagnosticsUpdated`.

33. UI Adapter does not require direct Storage lifecycle events.

34. Module lifecycle remains small.

35. Normal rendering/projection occurs while READY.

36. Navigation is scoped.

37. Waiting for user is scoped.

38. One view failure does not imply global module failure.

39. One optional capability failure does not imply global module failure.

40. Privacy is enforced according to UI surface.

41. Clipboard requires explicit user intent.

42. Heavy processing never runs synchronously inside UI Adapter.

43. Multiple frontends may coexist without creating duplicate domain authority.

---

# 104. MVP Scope

Recommended MVP:

```text
desktop UI adapter

UiIntent contracts
ReaderViewModel
SettingsViewModel
DiagnosticsViewModel
ErrorViewModel

navigation
dialogs
notifications
appearance application
localization application
accessibility metadata

Reading Session projection
Preferences editing projection
Presentation projection
Diagnostics projection

basic UI capabilities
platform-neutral source selection
error projection
fake frontend tests
```

---

# 105. Deferred Scope

Possible future capabilities:

```text
browser extension adapter
mobile adapter
advanced overlay UI
multi-window UX
advanced keyboard shortcut customization
screen-reader optimization
theme packs
advanced localization
remote UI
headless UI interface
```

---

# 106. Testing Priorities

UI Adapter tests should focus on:

```text
native-event → UiIntent mapping
UiIntent → command adaptation
snapshot → ViewModel projection
ViewModel immutability
projection staleness
error ownership
navigation
capability isolation
frontend isolation
privacy
localization
accessibility
platform independence
```

---

# 107. Ownership Tests

Verify UI Adapter never:

```text
executes OCR
executes Translation
creates WorkItem
creates Attempt
owns Runtime retry
validates Preferences authoritatively
mutates Reading Session state without command
recomputes Presentation semantics
accesses Storage implementation directly
renames external errors
```

---

# 108. Event Boundary Tests

Verify:

```text
pointer move
scroll
hover
focus
ViewOpened
NotificationShown
```

do not enter CRAI business Event Bus by default.

---

# 109. Error Ownership Tests

Inject:

```text
SES-*
PREF-*
TRN-*
RUN-*
DIAG-*
```

and verify:

```text
ownerModule
originalErrorCode
```

remain intact in UI projection.

---

# 110. Capability Tests

Disable:

```text
SystemNotification
Clipboard
OverlayWindow
Accessibility
```

independently.

Verify unrelated core UI remains operational where permitted.

---

# 111. Projection Tests

Test:

```text
Candidate construction failure
stale Candidate
publication failure
out-of-order projection completion
```

and verify previous/newer valid projection is preserved.

---

# 112. Privacy Tests

Verify UI Adapter does not leak raw sensitive content through:

```text
notifications
window titles
clipboard
accessibility metadata
diagnostics
```

without explicit intended behavior.

---

# 113. Document Set

```text
02-modules/
└── ui-adapter/
    ├── README.md
    ├── MODULE.md
    ├── CONTRACT.md
    ├── STATES.md
    ├── EVENTS.md
    └── ERRORS.md
```

---

# 114. Document Responsibilities

## README.md

Defines:

```text
module overview
architecture position
ownership summary
major usage rules
reading path
```

## MODULE.md

Defines:

```text
module identity
responsibilities
Application boundary
domain/runtime ownership boundaries
platform boundary
architecture invariants
```

## CONTRACT.md

Defines:

```text
UiIntent
ViewModel
DialogModel
NotificationModel
UiCapability
projection contracts
UI-local commands
error projection contracts
```

## STATES.md

Defines:

```text
module lifecycle
frontend lifecycle
view lifecycle
scoped operation phases
capability state
ViewModelRevision
```

## EVENTS.md

Defines:

```text
native event boundary
UI-local events
UiIntent distinction
Application/domain event consumption policy
business Event Bus boundary
```

## ERRORS.md

Defines:

```text
UIA error codes
intent errors
navigation errors
projection errors
view errors
capability errors
privacy errors
ownership preservation
```

---

# 115. Recommended Reading Order

For a new contributor:

```text
1. README.md
2. MODULE.md
3. CONTRACT.md
4. STATES.md
5. EVENTS.md
6. ERRORS.md
```

---

# 116. Implementation Reading Order

Recommended:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
ERRORS.md
    ↓
EVENTS.md
```

This establishes ownership and state boundaries before event integration.

---

# 117. Related Documents

```text
doc/02-modules/ui-adapter/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md

doc/02-modules/reading-session/
doc/02-modules/preferences/
doc/02-modules/presentation/
doc/02-modules/diagnostics/

doc/01-architecture/core/
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
└── STATE_MACHINE.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
└── OWNERSHIP_MAP.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
└── RUNTIME_OBSERVABILITY.md
```

---

# 118. Completion Checklist

UI Adapter is synchronized when:

* [ ] UI Adapter is defined as an application/presentation adapter;
* [ ] inbound native events become semantic UiIntents;
* [ ] cross-module use cases prefer Application orchestration;
* [ ] UI Adapter does not orchestrate the processing pipeline;
* [ ] `RetryPipeline` ownership is absent;
* [ ] Runtime retry remains Runtime-owned;
* [ ] ViewModels are immutable and non-authoritative;
* [ ] Candidate projection isolation is explicit;
* [ ] Presentation semantics remain Presentation-owned;
* [ ] Reading Session remains session authority;
* [ ] persistent Preferences remain Preferences-owned;
* [ ] theme/localization application is separated from persistence;
* [ ] external errors preserve original ownership;
* [ ] UI-local events remain local by default;
* [ ] Event Bus is not used for native UI activity;
* [ ] direct Recognition/Translation completion dependencies are absent;
* [ ] global `EffectivePreferencesChanged` dependency is absent;
* [ ] generic `DiagnosticsUpdated` dependency is absent;
* [ ] direct Storage lifecycle dependency is absent;
* [ ] module lifecycle uses READY/DEGRADED rather than global rendering/waiting states;
* [ ] UI capability degradation is explicit;
* [ ] native platform objects do not cross stable contracts;
* [ ] privacy rules cover notification/clipboard/accessibility/window-title surfaces;
* [ ] all six UI Adapter documents use the same terminology and ownership model.

---

# 119. Summary

UI Adapter v2 has two core directions.

Inbound:

```text
Native UI Event
    ↓
UiIntent
    ↓
UI Adapter
    ↓
Application / Module Command
```

Outbound:

```text
Authoritative Application / Module State
        ↓
UI Adapter
        ↓
Immutable ViewModel
        ↓
Native UI
```

The architecture boundary is:

```text
UI Adapter
    owns adaptation

Application
    owns use-case coordination

Domain Modules
    own business semantics

Runtime
    owns execution

Platform UI
    owns native rendering
```

The central invariant is:

```text
UI Adapter translates
between the user interface
and CRAI application contracts.

It never becomes
the business logic,
pipeline orchestrator,
Runtime authority,
or domain state owner.
```
