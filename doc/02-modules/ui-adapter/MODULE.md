# UI Adapter Module

> **Project:** CRAI
> **Module:** `ui-adapter`
> **Path:** `doc/02-modules/ui-adapter/MODULE.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Module Definition

The UI Adapter Module is CRAI's application-facing presentation adapter.

Its primary responsibility is to translate between:

```text
User / Platform UI
        ↕
UI Adapter
        ↕
Application / Module Contracts
```

The inbound direction is:

```text
User Interaction
    ↓
UI Intent
    ↓
UI Adapter
    ↓
Application / Module Command
```

The outbound direction is:

```text
Application / Module State
        ↓
UI Adapter
        ↓
Immutable ViewModel
        ↓
Platform UI
```

The UI Adapter answers:

> How should application state and commands cross the UI boundary safely?

It does not answer:

> What business decision should be made?

That belongs to the relevant domain module, Application orchestration, Business Pipeline Orchestration, or Runtime.

---

# 2. Module Identity

```text
Module ID: ui-adapter
Module Type: Application Adapter / Presentation Boundary
Primary Domain: UI-facing adaptation
Primary Inputs: UI intents + application/module state
Primary Outputs: application/module commands + ViewModels
Business Authority: None
Runtime Authority: None
Persistence Authority: None
MVP Priority: Required
```

UI Adapter is not:

```text
Reading Session
Presentation Domain
Business Pipeline Orchestrator
Runtime Controller
Preferences Domain
Storage
Diagnostics
Native UI Framework
```

---

# 3. Architectural Position

Preferred architecture:

```text
┌─────────────────────────────┐
│ Native / Platform UI        │
│ Desktop / Web / Extension   │
└──────────────┬──────────────┘
               ↕
        ┌───────────────┐
        │  UI Adapter   │
        └───────┬───────┘
                ↕
        ┌───────────────────┐
        │ Application Layer │
        └───────┬───────────┘
                ↓
 ┌────────────────────────────────┐
 │ Domain / Runtime Modules       │
 │                                │
 │ Reading Session                │
 │ Preferences                    │
 │ Presentation                   │
 │ Diagnostics                    │
 │ Runtime / Pipeline             │
 └────────────────────────────────┘
```

The UI Adapter prevents native UI concerns from leaking into domain/runtime contracts.

---

# 4. Core Ownership Rule

```text
UI Adapter
    owns UI adaptation

Application
    owns cross-module use-case coordination

Domain Modules
    own business semantics

Runtime
    owns execution

Platform UI
    owns native rendering and controls
```

---

# 5. Primary Responsibilities

UI Adapter owns:

```text
UI Intent normalization
command adaptation
query adaptation
ViewModel construction
UI-facing state projection
navigation model
dialog model
notification model
UI capability adaptation
platform interaction bindings
localization key adaptation
accessibility metadata adaptation
UI-local ephemeral state
```

---

# 6. Explicit Non-Responsibilities

UI Adapter MUST NOT:

* perform OCR;
* perform Translation;
* perform Text Processing;
* execute Capture;
* own Reading Session business rules;
* own Preferences validation;
* create Runtime WorkItems;
* create Runtime Attempts;
* retry Runtime work;
* restart pipelines;
* decide whether a processing stage must rerun;
* decide Artifact compatibility;
* persist domain state directly;
* query Storage directly;
* own provider selection;
* own diagnostics semantics;
* redefine Presentation semantics.

---

# 7. Inbound Adapter Role

UI Adapter converts user-facing interactions into stable application intents.

Example:

```text
User clicks "Start Reading"
        ↓
Native UI Event
        ↓
UI Adapter
        ↓
StartReading intent/command
        ↓
Application
```

The native control event is not itself a domain command.

---

# 8. Outbound Adapter Role

Application/domain state is converted into immutable UI models.

Example:

```text
ReadingSessionSnapshot
        +
Presentation state
        +
Diagnostics summary
        ↓
UI Adapter
        ↓
ReaderViewModel
        ↓
Platform UI
```

The UI Adapter may combine already-authoritative data for display.

It must not infer new business state.

---

# 9. UI Intent

Conceptually:

```text
UiIntent
├── intentId
├── intentType
├── sourceViewId?
├── correlationId?
├── payload?
└── occurredAt
```

Examples:

```text
StartReadingIntent
PauseReadingIntent
ResumeReadingIntent
StopReadingIntent

ChangeSessionPreferenceIntent

OpenSettingsIntent
SavePreferenceIntent

RetryFailedOperationIntent

DismissNotificationIntent
```

---

# 10. Intent Is Not Command Authority

The UI Adapter may receive:

```text
RetryFailedOperationIntent
```

but it does not decide:

```text
which Runtime Attempt to retry
whether retry is allowed
which pipeline stage to restart
```

The adapter forwards the user's intent to the proper application/runtime contract.

---

# 11. Removed `Retry Pipeline` Ownership

The v1 relationship:

```text
UI Adapter
    ↓
Reading Session
    ↓
Retry Pipeline
```

is removed.

Reading Session does not own pipeline retry.

Preferred:

```text
User Retry Intent
    ↓
UI Adapter
    ↓
Application / Runtime-facing command
    ↓
Runtime Retry / Business Pipeline policy
```

---

# 12. Application Command Boundary

UI Adapter should prefer application-level commands when a use case crosses module boundaries.

Example:

```text
StartReading
```

may require:

```text
Reading Session creation
+
Preference resolution
+
Runtime initialization
+
Presentation setup
```

Therefore UI Adapter should not manually orchestrate those modules.

Preferred:

```text
UI Adapter
    ↓
Application.StartReading
```

not:

```text
UI Adapter
    ↓
ReadingSession.Create
    ↓
Preferences.Resolve
    ↓
Runtime.Start
    ↓
Presentation.Initialize
```

---

# 13. Direct Module Commands

Direct module contracts may still be used when the interaction belongs clearly to one module.

Examples:

```text
Preferences.SetPreference

Diagnostics.GetDiagnosticHealth
```

provided Application ownership rules allow the direct adapter dependency.

---

# 14. ViewModel

A ViewModel is an immutable UI-facing projection.

Conceptually:

```text
ViewModel
├── viewModelId?
├── viewType
├── revision?
├── displayState
├── actions
├── accessibility
├── localization
└── metadata?
```

ViewModels contain no native control objects.

---

# 15. ViewModel Rules

ViewModels MUST be:

```text
immutable
serializable where practical
framework-neutral
business-rule-free
platform-safe
localization-ready
accessibility-ready
```

---

# 16. ViewModel Is Not Domain State

A ViewModel may contain:

```text
statusLabel
buttonEnabled
progressText
translatedDisplayBlocks
diagnosticStatus
```

but it is not authoritative domain state.

If a ViewModel disappears:

```text
Reading Session
Runtime
Preferences
Presentation
```

remain authoritative independently.

---

# 17. ViewModel Derivation

Preferred:

```text
Authoritative snapshots
        ↓
UI projection mapping
        ↓
ViewModel
```

Not:

```text
ViewModel
    ↓
derive authoritative business state
```

---

# 18. Reader ViewModel

Possible:

```text
ReaderViewModel
├── sessionStatus
├── sourceSummary
├── readingPositionSummary
├── presentationContentRef/View
├── availableActions[]
├── progressSummary?
├── warningSummary?
└── accessibilityMetadata
```

Exact schema belongs to `CONTRACT.md`.

---

# 19. Settings ViewModel

Possible:

```text
SettingsViewModel
├── categories[]
├── preferenceDefinitions[]
├── storedValues
├── validationState
├── availableActions[]
└── localizationMetadata
```

Preferences remains the validation authority.

---

# 20. Diagnostics ViewModel

Possible:

```text
DiagnosticsViewModel
├── overallHealth
├── componentHealth[]
├── recentIssues[]
├── capabilitySummary
└── availableSupportActions[]
```

Diagnostics remains semantic owner of the underlying diagnostic data.

---

# 21. Dialog Model

UI Adapter may own:

```text
DialogModel
├── dialogId
├── dialogType
├── titleKey
├── messageKey
├── actions[]
├── severity?
└── accessibilityMetadata
```

Dialog meaning should originate from application/domain outcomes.

---

# 22. Notification Model

UI Adapter may own:

```text
NotificationModel
├── notificationId
├── notificationType
├── titleKey?
├── messageKey
├── severity
├── actions[]
└── expiryPolicy?
```

Notifications are UI projections.

They are not domain events.

---

# 23. UI-Local State

UI Adapter may own ephemeral UI-only state such as:

```text
selected tab
expanded panel
scroll position
dialog visibility
temporary form draft
hover/focus state
navigation stack
```

This state does not become business state.

---

# 24. Window State

The v1 generic ownership:

```text
Window State
```

must be narrowed.

UI Adapter may own UI-local window/view state such as:

```text
window size
window position
selected panel
visibility
```

only when it has no domain meaning.

---

# 25. Window State vs CaptureSource

A native window selected for Capture is not merely UI window state.

Example:

```text
User selects application window
        ↓
UI Adapter identifies platform selection
        ↓
Capture/Application source-selection contract
        ↓
CaptureSource
```

Capture owns CaptureSource semantics.

UI Adapter owns only the platform interaction required to select it.

---

# 26. Theme State

Theme is primarily UI/application appearance state.

UI Adapter may adapt it to native platform rendering.

If CRAI treats theme as a persistent user preference:

```text
Preferences
    → owns persistent theme preference

UI Adapter
    → applies resolved appearance to UI
```

UI Adapter should not independently become persistent preference authority.

---

# 27. Localization Resources

The v1 statement that UI Adapter owns all localization resources is too broad.

Preferred ownership:

```text
Application localization catalog / resource subsystem
    → owns localized strings/resources

UI Adapter
    → uses localization keys/context

Platform UI
    → renders localized result
```

A simple implementation may package localization resources with the UI layer, but domain contracts should use stable message/localization keys rather than hard-coded native strings.

---

# 28. User-Visible Text

Domain modules should prefer:

```text
errorCode
messageKey
structured metadata
```

rather than platform-specific final text.

UI Adapter/Application localization resolves those values into user-facing text.

---

# 29. Accessibility

Accessibility is a first-class UI concern.

UI Adapter may define platform-neutral accessibility metadata such as:

```text
semantic role
accessible name key
accessible description key
focus order hint
keyboard action
screen-reader hint
```

Native accessibility APIs remain platform implementation details.

---

# 30. Platform Bindings

UI Adapter may integrate with platform-specific UI capabilities such as:

```text
desktop window selection
file picker
clipboard
system notifications
browser extension popup
keyboard shortcuts
accessibility APIs
```

These integrations must remain behind adapter contracts.

---

# 31. Platform Binding Boundary

Do not expose:

```text
DOM Element
HTMLElement
React component
Flutter Widget
Qt QObject
WinUI control
native window pointer
browser extension API object
```

through domain/application contracts.

---

# 32. Presentation Relationship

Presentation owns:

```text
how translated/processed content
should be semantically presented
```

Examples:

```text
layout result
overlay placement
text fitting result
presentation artifact
presentation revision
```

UI Adapter owns:

```text
how that presentation state
is adapted to platform UI
```

---

# 33. Presentation Flow

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
Platform-facing ViewModel
    ↓
Native UI
```

---

# 34. UI Adapter Does Not Recompute Presentation

UI Adapter must not independently:

```text
fit translated text into bubbles
determine reading order
recalculate overlay geometry
choose content layout
truncate semantic output
```

Those are Presentation responsibilities.

---

# 35. Native Rendering

Actual rendering may occur in:

```text
Desktop UI
Web UI
Browser Extension UI
Mobile UI
```

The UI Adapter may provide the platform-facing representation required for rendering.

Native rendering framework code remains outside business modules.

---

# 36. Reading Session Relationship

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
send session-related user intents
query/read session snapshots
render session state
```

It does not own session transitions.

---

# 37. Reading Session Commands

Possible user intents:

```text
StartReading
PauseReading
ResumeReading
StopReading
ChangeReadingContext
ChangeSessionConfiguration
```

UI Adapter forwards these through the proper application/module boundary.

---

# 38. Reading Session Errors

Reading Session errors retain:

```text
SES-...
```

identity.

UI Adapter converts them into:

```text
safe error ViewModel
dialog
notification
inline validation message
```

without changing their semantic ownership.

---

# 39. Preferences Relationship

Preferences owns:

```text
PreferenceDefinition
persistent preference state
validation
PreferenceRevision
resolution semantics
```

UI Adapter may:

```text
query definitions
display values
submit changes
display validation errors
```

---

# 40. Preference Validation

Invalid:

```text
Settings UI
    ↓
UI Adapter validates Preference rules independently
```

Correct:

```text
UI Adapter
    ↓
SetPreference
    ↓
Preferences validation
    ↓
result/error
    ↓
UI Adapter ViewModel
```

Client-side convenience validation may exist but is never authoritative.

---

# 41. Session Overrides

Session-only settings use Reading Session-owned configuration.

UI Adapter must distinguish:

```text
Save globally
Save for source
Use only in this session
```

because those commands have different owners.

---

# 42. Runtime Relationship

Runtime owns:

```text
WorkItem
Attempt
RuntimeRevision
cancellation
retry
deadline
queueing
supersession
```

UI Adapter may display Runtime-derived progress or failure information.

It must not implement Runtime policy.

---

# 43. Retry Intent

A user-facing:

```text
Retry
```

button does not mean:

```text
UI Adapter retries the last stage.
```

It means:

```text
UI Adapter
    ↓
Retry Intent
    ↓
Application / Runtime policy
```

Runtime decides whether and how retry occurs.

---

# 44. Progress

UI progress indicators may be derived from:

```text
Application use-case state
Runtime summaries
Reading Session state
processing status
```

UI Adapter may map these into:

```text
ProgressViewModel
```

It does not own progress authority.

---

# 45. Storage Relationship

UI Adapter MUST NOT access Storage implementation directly.

If the UI needs:

```text
history
maintenance
storage status
cleanup actions
```

it uses application/storage public contracts.

---

# 46. Diagnostics Relationship

UI Adapter may query:

```text
DiagnosticHealthSnapshot
DiagnosticCapabilities
RecentDiagnosticIssues
```

and render safe diagnostic views.

It must not expose raw internal telemetry by default.

---

# 47. Diagnostics Privacy

Developer diagnostics UI may expose more detail than normal UI.

However UI Adapter still must respect:

```text
redaction
privacy classification
diagnostic access policy
```

---

# 48. Application Relationship

Application is the preferred coordination point for cross-module user actions.

Examples:

```text
StartReading
StopReading
ChangeReaderSource
RetryCurrentOperation
ExportSupportBundle
```

UI Adapter should not reproduce use-case orchestration internally.

---

# 49. Application State

Application may expose UI-oriented use-case snapshots.

UI Adapter may adapt them into platform-specific ViewModels.

This reduces the need for UI Adapter to query many modules independently.

---

# 50. Event Boundary

UI Adapter should distinguish:

```text
native UI events
UI intents
application/domain events
UI-local events
```

These are not the same event family.

---

# 51. Native UI Events

Examples:

```text
ButtonClicked
TextChanged
PointerMoved
WindowResized
KeyPressed
```

These are platform implementation events.

They do not belong on CRAI business Event Bus.

---

# 52. UI Intents

UI Adapter translates relevant native events into semantic user intent.

Example:

```text
ButtonClicked
    ↓
RetryCurrentOperationIntent
```

This intent may then become an application command.

---

# 53. Domain/Application Events

UI Adapter/Application may observe stable facts from modules when event-driven UI projection is useful.

Examples:

```text
ReadingContextChanged
PreferenceChanged
DiagnosticCapabilityChanged
```

UI Adapter does not need to subscribe to every event if query/snapshot-based state projection is more appropriate.

---

# 54. UI-Local Events

Events such as:

```text
ViewOpened
ViewClosed
DialogConfirmed
NotificationShown
ThemeApplied
```

are UI-local interaction/analytics facts.

They are not automatically business Event Bus events.

---

# 55. Event Bus Is Not UI Event Bus

Do not send every:

```text
click
scroll
hover
focus
dialog open
```

through CRAI business Event Bus.

Use local UI event mechanisms.

---

# 56. `ThemeChanged`

If theme is Preferences-owned:

```text
PreferenceChanged
    ↓
Application/UI state update
    ↓
UI Adapter applies theme
```

A separate UI-owned `ThemeChanged` business event is usually unnecessary.

---

# 57. Dialog Events

`DialogConfirmed` may remain local to the UI layer.

The semantic action produced from it is what crosses the application boundary.

Example:

```text
DialogConfirmed
    ↓
DeleteSourceProfileIntent
    ↓
Preferences/Application command
```

---

# 58. Notification Events

`NotificationShown` is normally UI telemetry/local state.

It should not be treated as domain authority.

---

# 59. Module Lifecycle

UI Adapter lifecycle should remain small:

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

Detailed semantics belong to `STATES.md`.

---

# 60. No Global `RENDERING`

Rendering is an operation performed by native UI/platform rendering.

It should not turn the entire UI Adapter into:

```text
RENDERING
```

for every frame/view update.

---

# 61. No Global `WAITING_FOR_USER`

A dialog waiting for user input is local interaction state.

Other views may remain fully usable.

Therefore:

```text
WAITING_FOR_USER
```

should not be a global module lifecycle state.

---

# 62. No Global `UPDATING`

Individual ViewModel projection/update operations may be in progress while the UI Adapter remains READY.

---

# 63. DEGRADED

UI Adapter may become DEGRADED when one UI capability is impaired.

Examples:

```text
system notifications unavailable
clipboard unavailable
one optional view failed
localization fallback active
accessibility integration partially unavailable
```

Core UI may remain usable.

---

# 64. No Generic Global `FAILED`

A single view rendering failure should not make the entire UI Adapter unusable.

Prefer:

```text
operation/view error
or
DEGRADED
```

depending on failure scope.

---

# 65. View Lifecycle

Individual views may have local lifecycle such as:

```text
CREATED
MOUNTED
VISIBLE
HIDDEN
DISPOSED
```

if the implementation needs it.

This is separate from UI Adapter module lifecycle.

---

# 66. UI Operation State

Scoped operations may include:

```text
LOADING
SUBMITTING
AWAITING_RESULT
COMPLETED
FAILED
```

for one user action/view.

These are not global module states.

---

# 67. Error Ownership

UI Adapter owns errors about UI adaptation itself.

Examples:

```text
InvalidUiIntent
ViewModelConstructionFailed
ViewUnavailable
NavigationInvalid
LocalizationResourceUnavailable
AccessibilityCapabilityUnavailable
PlatformBindingUnavailable
UnsupportedUiCapability
UiAdapterInvariantViolation
```

---

# 68. Errors UI Adapter Does Not Own

UI Adapter must not convert:

```text
SES-...
PREF-...
CAP-...
REC-...
TRN-...
RUN-...
DIAG-...
```

into generic UI Adapter errors merely because they are displayed in the UI.

---

# 69. Error Projection

Example:

```text
PREF-VAL-...
    ↓
UI Adapter
    ↓
SettingsValidationViewModel
```

The underlying error remains Preferences-owned.

---

# 70. Rendering Failure

Native rendering errors should be handled at the narrowest scope possible.

Example:

```text
one overlay view fails
    ↓
ViewRenderFailed
```

not automatically:

```text
UI Adapter module FAILED
```

---

# 71. Localization Failure

If one localization key is missing:

```text
use safe fallback
record diagnostics
```

where policy allows.

Do not generally fail the entire UI.

---

# 72. Platform Capability Failure

Example:

```text
system notification API unavailable
```

may produce:

```text
Notification capability unavailable
```

while the main application remains functional.

---

# 73. Configuration

UI Adapter may consume UI-facing configuration such as:

```text
appearance
language
accessibility
platform capability settings
view behavior
```

Persistent user preference authority remains Preferences/Application as defined elsewhere.

---

# 74. Platform Independence

Business/application contracts must remain independent from:

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
browser extension APIs
```

---

# 75. Framework Independence

The UI Adapter's stable public contracts should use:

```text
plain data
opaque IDs
commands
queries
ViewModels
capability descriptors
```

not framework object types.

---

# 76. Accessibility Boundary

Accessibility behavior must remain available across supported platform implementations.

A platform may map:

```text
AccessibilityMetadata
```

to:

```text
ARIA
Windows UI Automation
Android Accessibility
macOS Accessibility
```

without changing domain contracts.

---

# 77. Localization Boundary

Domain/application modules should avoid hard-coded final user-facing language.

Preferred:

```text
ErrorCode
MessageKey
safe parameters
```

then:

```text
UI Adapter / localization layer
    ↓
localized text
```

---

# 78. ViewModel Immutability

Once published:

```text
ViewModel N
```

must not mutate in place.

A state change creates:

```text
ViewModel N+1
```

or an equivalent immutable replacement.

---

# 79. Revision in ViewModels

Where useful, ViewModels may include:

```text
sourceRevision
presentationRevision
readingContextRevision
preferenceRevision
```

for projection consistency.

UI Adapter does not own those revisions.

---

# 80. Stale UI Projection

If a ViewModel is based on stale authoritative state:

```text
new authoritative snapshot
    ↓
new ViewModel
```

The adapter does not mutate domain state to match the old UI.

---

# 81. User Command Concurrency

Repeated user actions may race.

Example:

```text
user clicks Stop
user clicks Start
```

UI Adapter may debounce obvious duplicate UI gestures.

Authoritative concurrency validation remains with Application/domain contracts.

---

# 82. Disable Controls vs Authority

A disabled UI button improves UX.

It is not a security/business invariant.

Even if the UI disables:

```text
Stop
```

the receiving domain contract must still validate incoming commands.

---

# 83. Optimistic UI

Optimistic UI updates may be used carefully.

However:

```text
optimistic ViewModel
```

must never become authoritative domain state before the domain command commits.

---

# 84. Rollback of Optimistic UI

If an optimistic action is rejected:

```text
domain rejection
    ↓
new authoritative snapshot/result
    ↓
UI Adapter rebuilds ViewModel
```

No domain rollback is performed by UI Adapter.

---

# 85. Notifications

Application/domain outcomes may produce UI notifications.

Example:

```text
CaptureSourceUnavailable
    ↓
Application safe summary
    ↓
UI Adapter
    ↓
NotificationModel
```

---

# 86. Dialog Coordination

UI Adapter may coordinate UI dialog flow such as:

```text
confirm destructive action
choose file
select capture source
request optional permission explanation
```

Actual business authorization remains external.

---

# 87. Permission UI

UI Adapter may guide the user through platform permission UX.

Platform permission state itself belongs to the relevant platform/provider capability.

UI Adapter must not claim permission was granted until the owner reports it.

---

# 88. Capture Source Selection

Example:

```text
User chooses screen region
        ↓
Platform UI selection
        ↓
UI Adapter
        ↓
Application/Capture source command
        ↓
CaptureSource
```

The selected geometry/platform object must be normalized before crossing stable contracts.

---

# 89. File Selection

File picker:

```text
Native File Picker
    ↓
UI Adapter
    ↓
safe application input reference
```

Native file handles should not leak into domain contracts unless represented by an explicit platform-neutral abstraction.

---

# 90. Browser Extension

Browser Extension UI is another UI Adapter implementation/profile.

Business modules must not know whether an intent originated from:

```text
desktop UI
web UI
mobile UI
browser extension
```

unless the use case explicitly requires source capability metadata.

---

# 91. Multiple UI Frontends

CRAI may support multiple simultaneous UI frontends.

Example:

```text
Desktop main window
+
overlay window
+
browser extension panel
```

They may share application/domain state.

UI-local state remains frontend-specific.

---

# 92. UI Adapter Instances

Different UI frontends may use separate adapter instances.

They must not create duplicate domain authorities.

---

# 93. Diagnostics

UI Adapter should instrument important UI operations through Diagnostics abstractions.

Examples:

```text
view projection duration
platform binding failure
navigation failure
support export request
```

Do not publish every UI gesture onto the business Event Bus.

---

# 94. Privacy

UI Adapter must avoid leaking sensitive content into:

```text
notifications
logs
window titles
clipboard
accessibility labels
diagnostics
```

unless explicitly intended and safe.

---

# 95. Accessibility Privacy

Screen-reader labels may expose content to platform accessibility services.

CRAI should avoid placing unnecessarily sensitive reading content in global accessibility metadata.

---

# 96. Clipboard Privacy

Copy operations require explicit user intent.

UI Adapter must not silently place captured/OCR/translated content on the system clipboard.

---

# 97. Native Notification Privacy

System notifications may appear outside the application.

Avoid including raw reading content by default.

Prefer generic safe notifications.

---

# 98. Performance

UI Adapter should prioritize:

```text
responsive user interaction
cheap ViewModel projection
incremental native rendering
bounded UI-local state
minimal blocking on UI thread
```

---

# 99. Blocking Rule

UI Adapter must not perform heavy:

```text
OCR
Translation
image processing
large persistence operations
diagnostic export
```

synchronously on the UI thread.

---

# 100. Runtime Async Boundary

Long-running operations use:

```text
user intent
    ↓
Application / Runtime
    ↓
async result/state
    ↓
UI ViewModel update
```

---

# 101. Common Architecture Mistake — Business Logic in ViewModel Mapper

Wrong:

```text
if OCR confidence < X
    rerun Recognition
```

inside UI Adapter.

Correct:

```text
Business Pipeline / Recognition policy
    owns decision

UI Adapter
    only displays state/result
```

---

# 102. Common Architecture Mistake — UI Orchestrates Modules

Wrong:

```text
Start button
    ↓
create Reading Session
    ↓
resolve Preferences
    ↓
schedule Capture
    ↓
start Recognition
```

inside UI Adapter.

Correct:

```text
Start button
    ↓
StartReadingIntent
    ↓
Application use case
```

---

# 103. Common Architecture Mistake — Domain Depends on UI

Wrong:

```text
Translation
    ↓
show dialog
```

Correct:

```text
Translation result/error
    ↓
Application
    ↓
UI Adapter
    ↓
Dialog/Notification
```

---

# 104. Common Architecture Mistake — Domain Event for Every Click

Wrong:

```text
DialogOpened Event Bus event
ButtonHovered Event Bus event
ScrollChanged Event Bus event
```

Correct:

```text
local UI events
```

unless there is an explicit business/audit requirement.

---

# 105. Common Architecture Mistake — ViewModel Becomes State Authority

Wrong:

```text
UI says session active
therefore session is active
```

Correct:

```text
Reading Session snapshot
    ↓
UI says session active
```

---

# 106. Common Architecture Mistake — Retry Pipeline

Wrong:

```text
UI Adapter
    ↓
Reading Session.RetryPipeline()
```

Correct:

```text
Retry Intent
    ↓
Application / Runtime policy
```

---

# 107. Common Architecture Mistake — UI Owns Theme Persistence

Wrong:

```text
UI Adapter
    owns persistent theme setting
```

when theme is a user preference.

Correct:

```text
Preferences
    owns persisted value

UI Adapter
    applies effective value
```

---

# 108. Architecture Invariants

1. UI Adapter contains no business rules.

2. Business modules never reference native UI frameworks.

3. UI Adapter is an adapter, not an orchestrator.

4. Cross-module use cases prefer Application commands.

5. UI Adapter does not create Runtime WorkItems.

6. UI Adapter does not create Runtime Attempts.

7. UI Adapter does not execute Runtime retry.

8. UI Adapter does not restart pipelines.

9. Reading Session owns session semantics.

10. Preferences owns persistent preference semantics.

11. Presentation owns presentation semantics.

12. Diagnostics owns diagnostic semantics.

13. Runtime owns execution authority.

14. ViewModels are immutable.

15. ViewModels are non-authoritative projections.

16. Native UI events are not business commands.

17. UI Adapter translates native events into semantic intents.

18. Semantic intents become application/module commands.

19. Domain/application state becomes ViewModels.

20. UI-local state remains separate from domain state.

21. Platform-specific code remains behind UI adapter/platform bindings.

22. Native UI objects never cross stable domain contracts.

23. Error ownership remains with the original module.

24. UI Adapter errors describe only UI adaptation failures.

25. UI Adapter does not perform Preferences validation authoritatively.

26. UI Adapter does not access Storage implementation directly.

27. UI Adapter does not recompute Presentation semantics.

28. User retry intent does not imply UI-owned retry policy.

29. Event Bus is not used for every UI event.

30. UI-local events remain local unless a real cross-module consumer exists.

31. `Rendering` is not a global module state.

32. `WaitingForUser` is not a global module state.

33. `Updating` is not a global module state.

34. One view failure does not require global FAILED state.

35. Theme persistence remains with Preferences when configurable.

36. Localization contracts use stable keys rather than native strings.

37. Accessibility metadata remains platform-neutral at the stable boundary.

38. UI rendering must remain responsive.

39. Heavy processing does not run synchronously in UI Adapter.

40. UI diagnostics remain privacy-safe.

---

# 109. MVP Scope

Recommended MVP:

```text
desktop UI adapter
ReaderViewModel
SettingsViewModel
Diagnostics summary ViewModel

UI intent contracts
application command adaptation
Reading Session state projection
Preference editing projection
Presentation state projection
basic dialogs
basic notifications
localization keys
accessibility metadata

platform capability abstraction
basic error projection
```

---

# 110. Deferred Scope

Potential future features:

```text
mobile UI adapter
browser extension adapter
multi-window support
advanced overlay controls
keyboard shortcut customization
screen-reader optimization
theme packs
advanced localization
remote UI
headless UI API
```

---

# 111. Testing Strategy

UI Adapter must be testable without:

```text
real OCR
real Translation
real Runtime worker
real database
real provider
real native UI framework
```

Core mappings should be testable with plain data.

---

# 112. Unit Tests

Test:

```text
native-event → UiIntent mapping
UiIntent → command mapping
domain snapshot → ViewModel mapping
error → UI projection
localization-key mapping
accessibility metadata
navigation model
dialog/notification models
```

---

# 113. Ownership Tests

Verify UI Adapter never:

```text
creates WorkItem
creates Attempt
executes OCR
executes Translation
retries Runtime work
validates domain preference rules independently
accesses Storage implementation
changes ReadingContext directly without command
takes ownership of domain errors
```

---

# 114. ViewModel Tests

Verify:

```text
immutability
stable mapping
no native UI object
no provider SDK object
no mutable domain reference
safe localization fields
safe accessibility fields
```

---

# 115. Runtime Boundary Tests

Given:

```text
RetryCurrentOperationIntent
```

verify UI Adapter produces the correct application/runtime-facing request and does not independently choose:

```text
stage
Attempt
retry count
backoff
```

---

# 116. Presentation Boundary Tests

Verify Presentation output is adapted but not semantically recomputed.

---

# 117. Preferences Boundary Tests

Verify:

```text
Global preference
Source preference
Session override
```

produce commands to the correct owner.

---

# 118. UI Event Boundary Tests

Verify high-frequency local events such as:

```text
pointer move
scroll
focus
hover
render tick
```

do not enter CRAI Event Bus by default.

---

# 119. Privacy Tests

Verify UI Adapter does not leak raw sensitive reading content through:

```text
logs
notifications
window titles
diagnostics
clipboard
accessibility metadata
```

without explicit intended behavior.

---

# 120. Related Documents

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

# 121. Documentation Ownership

This file defines:

```text
UI Adapter module identity
adapter responsibilities
UI Intent boundary
ViewModel ownership
Application relationship
Reading Session relationship
Preferences relationship
Presentation relationship
Runtime relationship
Diagnostics relationship
platform boundary
localization/accessibility boundary
architecture invariants
```

Detailed contracts belong to:

```text
CONTRACT.md
```

Detailed lifecycle belongs to:

```text
STATES.md
```

Detailed UI Adapter-owned facts belong to:

```text
EVENTS.md
```

Detailed UI adaptation errors belong to:

```text
ERRORS.md
```

---

# 122. Completion Criteria

UI Adapter is architecturally synchronized when:

* UI Adapter is classified as an application/presentation adapter;
* native UI interactions become semantic UiIntents;
* cross-module operations go through Application orchestration;
* UI Adapter does not orchestrate the processing pipeline;
* `Retry Pipeline` is removed from Reading Session interaction;
* Runtime retry remains Runtime-owned;
* ViewModels are immutable projections;
* ViewModels are not authoritative domain state;
* Presentation semantics remain Presentation-owned;
* persistent Preferences remain Preferences-owned;
* session-specific configuration remains Reading Session-owned;
* UI-local state is distinguished from business state;
* platform-specific objects remain behind adapter boundaries;
* UI-local events do not flood Event Bus;
* global Rendering/WaitingForUser/Updating states are removed;
* global Failed is not required for isolated view/capability failures;
* localization and accessibility contracts remain platform-neutral;
* diagnostics and notifications remain privacy-safe;
* tests verify mappings and ownership boundaries.

---

# 123. Summary

UI Adapter v2 has two primary directions.

Inbound:

```text
Native UI Event
    ↓
UI Intent
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

Ownership remains:

```text
UI Adapter
    owns adaptation

Application
    owns use-case coordination

Domain modules
    own business state

Runtime
    owns execution

Platform UI
    owns native rendering
```

The central invariant is:

```text
UI Adapter translates
between UI and application contracts.

It never becomes
the business logic,
pipeline orchestrator,
or Runtime authority.
```
