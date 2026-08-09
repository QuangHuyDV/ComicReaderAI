# UI Adapter Events

> **Project:** CRAI
> **Module:** `ui-adapter`
> **Path:** `doc/02-modules/ui-adapter/EVENTS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the event boundary of the UI Adapter module.

UI Adapter v2 distinguishes three event domains:

```text
Native UI Events
        ↓
UI-Local Events / UiIntent
        ↓
Application / Domain Boundary
        ↓
Business Event Bus only where appropriate
```

The primary rule is:

```text
not every UI event
is a business event
```

UI Adapter uses events only when they fit the ownership and communication semantics of the relevant layer.

---

# 2. Event Categories

UI Adapter works with four categories:

```text
UI Event Domains
├── Native Platform Events
├── UI-Local Adapter Events
├── UiIntents
└── Application / Domain Events
```

These categories must not be merged.

---

# 3. Native Platform Events

Examples:

```text
ButtonClicked
PointerMoved
KeyPressed
WindowResized
FocusChanged
TextChanged
ScrollChanged
NativeDialogClosed
```

These originate from:

```text
React
Electron
Qt
Flutter
browser
desktop toolkit
mobile toolkit
```

They remain inside the platform/UI implementation boundary.

---

# 4. Native Events Are Not Business Events

The following must not normally enter CRAI Event Bus:

```text
ButtonClicked
PointerMoved
ScrollChanged
FocusChanged
HoverChanged
RenderTick
WindowMoved
```

They are implementation details.

---

# 5. UiIntent

UI Adapter converts relevant native interaction into semantic user intent.

Example:

```text
ButtonClicked
    ↓
RetryCurrentOperationIntent
```

UiIntent is defined in:

```text
CONTRACT.md
```

It is not necessarily an Event Bus event.

---

# 6. UiIntent Examples

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

---

# 7. UiIntent Transport

Preferred:

```text
UI Adapter
    ↓
Application command/use-case boundary
```

Do not use Event Bus as hidden command transport.

Invalid:

```text
RetryRequested event
    ↓
Runtime subscriber
```

Preferred:

```text
RetryCurrentOperationIntent
    ↓
Application / Runtime-facing command
```

---

# 8. UI-Local Events

UI Adapter may use local events for UI coordination.

Examples:

```text
ViewOpened
ViewClosed
ViewShown
ViewHidden

NavigationCompleted
NavigationFailed

DialogOpened
DialogResponded
DialogDismissed

NotificationShown
NotificationDismissed

AppearanceApplied
LocalizationApplied

FrontendConnected
FrontendDisconnected
```

These are usually local to the UI layer.

---

# 9. UI-Local Event Contract

Conceptually:

```text
UiLocalEvent
├── eventId
├── eventType
├── frontendId?
├── viewId?
├── occurredAt
├── correlationId?
└── safeMetadata?
```

---

# 10. UI-Local Events and Business Event Bus

Default:

```text
UiLocalEvent
    ≠
Business Event Bus Event
```

A UI-local event should enter the global Event Bus only when there is an explicit cross-module consumer and clear ownership reason.

---

# 11. ViewOpened

## Meaning

One UI view became logically open/available.

Scope:

```text
UI-local
```

Typical consumers:

```text
navigation coordinator
UI analytics
frontend state projection
```

Not a domain state transition.

---

# 12. ViewClosed

## Meaning

A UI view instance closed or disposed.

Important:

```text
ViewClosed
```

does not automatically mean:

```text
ReadingSessionStopped
RuntimeCancelled
```

---

# 13. NavigationCompleted

## Meaning

A scoped navigation operation completed.

Scope:

```text
UI-local
```

It does not imply any business use-case completed.

---

# 14. DialogOpened

Dialog became visible.

Scope:

```text
UI-local
```

No business authority is implied.

---

# 15. DialogResponded

Preferred replacement for separate global:

```text
DialogConfirmed
DialogCancelled
```

Conceptually:

```text
DialogResponded
├── dialogId
├── selectedActionId?
├── cancelled
└── respondedAt
```

UI Adapter may translate this into a UiIntent.

---

# 16. Dialog Response Flow

```text
DialogResponded
    ↓
UI Adapter
    ↓
semantic UiIntent
    ↓
Application command
```

The dialog event itself normally stays local.

---

# 17. NotificationShown

Represents successful UI presentation of one notification.

Scope:

```text
UI-local
```

It is not a business fact.

---

# 18. AppearanceApplied

Preferred replacement for:

```text
ThemeChanged
```

when describing UI Adapter-owned state.

Meaning:

```text
resolved appearance/theme
was successfully applied to the UI
```

Persistent preference ownership remains Preferences/Application.

---

# 19. LocalizationApplied

Preferred replacement for:

```text
LocalizationChanged
```

when describing the adapter.

Meaning:

```text
a locale/resource set
was successfully applied
```

It does not mean a persistent locale preference changed.

---

# 20. AccessibilityApplied

Optional UI-local fact:

```text
AccessibilityApplied
```

when accessibility configuration has been applied to a frontend.

Persistent accessibility preference, if configurable, remains externally owned.

---

# 21. Frontend Events

For multiple frontends:

```text
FrontendConnected
FrontendDisconnected
FrontendDegraded
FrontendRecovered
```

may exist as UI-local/application integration facts.

Examples:

```text
desktop frontend
overlay frontend
browser extension frontend
```

---

# 22. FrontendConnected

Meaning:

```text
a UI frontend became available
to the adapter/application
```

This does not create domain authority.

---

# 23. FrontendDisconnected

Meaning:

one frontend became unavailable.

Other frontends may remain active.

---

# 24. Application / Domain Events

UI Adapter may observe stable external facts when useful.

Examples:

```text
PreferenceChanged
ReadingContextChanged
DiagnosticCapabilityChanged
DiagnosticCollectionDegraded
Presentation-owned update fact
```

Exact events remain owned by their producing modules.

---

# 25. Application Projection Preferred

For complex screens, preferred flow is often:

```text
Domain / Runtime changes
        ↓
Application projection
        ↓
ApplicationUiSnapshot
        ↓
UI Adapter
        ↓
ViewModel
```

rather than subscribing the UI Adapter directly to every domain event.

---

# 26. No Mandatory Event-Only UI

The v1 rule:

```text
UI updates only from application events
instead of polling/module queries
```

is removed.

Allowed architecture mechanisms include:

```text
snapshot/query
application projection
selected domain events
observable state stream
```

provided ownership remains correct.

---

# 27. Removed Mandatory `ReadingSessionChanged`

UI Adapter does not require one generic:

```text
ReadingSessionChanged
```

event for correctness.

It may receive:

```text
ReadingSessionSnapshot
ApplicationUiSnapshot
specific Reading Session facts
```

depending on projection architecture.

---

# 28. Removed `EffectivePreferencesChanged`

Preferences v2 does not own one global:

```text
EffectivePreferencesChanged
```

because effective preference state is contextual.

UI Adapter should consume the relevant settings/application projection instead.

---

# 29. Preference UI Flow

Example:

```text
PreferenceChanged
    ↓
Application/settings projection
    ↓
SettingsViewModel N+1
```

No global effective-preference event is required.

---

# 30. Removed Generic `PresentationUpdated`

A generic:

```text
PresentationUpdated
```

may be too broad unless it is actually defined by Presentation.

UI Adapter should consume the authoritative Presentation contract/event/snapshot defined by that module.

Do not invent UI-owned presentation event semantics.

---

# 31. Removed `TranslationCompleted`

UI Adapter does not need direct Translation completion for normal Reader UI.

Preferred:

```text
Translation / Business Pipeline
    ↓
Presentation/Application state
    ↓
Reader projection
```

This reduces direct module coupling.

---

# 32. Removed `RecognitionCompleted`

Same rule:

```text
RecognitionCompleted
```

should not be a mandatory UI dependency.

Raw Recognition completion is usually not the UI's real display concern.

---

# 33. Removed `DiagnosticsUpdated`

Diagnostics v2 does not publish generic:

```text
DiagnosticsUpdated
```

UI may consume:

```text
DiagnosticHealthSnapshot
DiagnosticCapabilities
DiagnosticCapabilityChanged
DiagnosticCollectionDegraded
```

as appropriate.

---

# 34. Removed `StorageReady`

UI Adapter should not depend directly on Storage lifecycle.

If storage status matters to UI:

```text
Storage/Application projection
    ↓
StorageStatusViewModel
```

---

# 35. Removed `StorageFailed`

Storage errors remain Storage/application-owned.

UI Adapter projects them into UI-safe error state without becoming a direct Storage event consumer by default.

---

# 36. Event Consumption Strategy

Recommended precedence:

```text
Application projection/snapshot
        ↓
selected stable domain events
        ↓
direct module query where appropriate
```

Avoid:

```text
subscribe to every module event
and reconstruct the application state in UI Adapter
```

---

# 37. Why UI Adapter Must Not Reconstruct Business State

If UI Adapter combines:

```text
ReadingSessionChanged
TranslationCompleted
RecognitionCompleted
StorageReady
PreferencesChanged
```

to infer current application state, it becomes an accidental orchestrator/projection authority.

That belongs to Application or the owning domain modules.

---

# 38. Event Naming

For actual events, use past-tense facts.

Examples:

```text
ViewOpened
NavigationCompleted
DialogResponded
AppearanceApplied
FrontendDisconnected
```

Commands/intents remain imperative/intent-style:

```text
OpenView
Navigate
RetryCurrentOperationIntent
```

---

# 39. Event Immutability

Published events are immutable.

A later correction creates a new fact rather than mutating an old event.

---

# 40. UI-Local Event Ordering

Ordering may exist within one scoped UI operation.

Example:

```text
OpenView
    ↓
ViewOpened
    ↓
NavigationCompleted
```

This is a UI-local sequence.

It is not a global application ordering guarantee.

---

# 41. Dialog Ordering

One dialog instance may follow:

```text
DialogOpened
    ↓
DialogResponded
```

or:

```text
DialogOpened
    ↓
DialogDismissed
```

Terminal outcomes must not occur twice for the same dialog instance.

---

# 42. Appearance Ordering

```text
ApplyTheme
    ↓
resources validated
    ↓
appearance applied
    ↓
AppearanceApplied
```

The event occurs after adapter-local state is committed.

---

# 43. Localization Ordering

```text
ApplyLocalization
    ↓
resource set loaded
    ↓
UI projection updated
    ↓
LocalizationApplied
```

---

# 44. No Cross-Domain Ordering Assumption

UI Adapter must not assume:

```text
PreferenceChanged
always precedes
Presentation update
```

or similar cross-module ordering unless the owning architecture explicitly guarantees it.

---

# 45. Projection Revision Beats Event Timing

When rebuilding ViewModels, use:

```text
typed source revisions
projection provenance
ViewModelRevision
```

where relevant.

Do not rely only on event arrival time.

---

# 46. Event Idempotency

Every actual event has:

```text
EventId
```

Duplicate processing must not corrupt local projection state.

---

# 47. Deduplication Identity

The v1 suggestion to use:

```text
EventId
Timestamp
View Identifier
```

for duplicate detection is removed.

Use:

```text
EventId
```

as event identity.

`ViewId` and timestamp are not unique event identities.

---

# 48. UI Intent Idempotency

UiIntent uses:

```text
intentId
```

for intent identity.

This is distinct from EventId.

---

# 49. CorrelationId

CorrelationId groups related operations.

It is not a deduplication identity.

Example:

```text
StartReadingIntent
ReadingSessionCreated
RuntimeRevisionStarted
ReaderViewModelUpdated
```

may all share one CorrelationId.

---

# 50. Delivery Guarantees

UI Adapter does not define infrastructure-level:

```text
At-least-once
ordered within interaction
```

for the CRAI Event Bus.

Those belong to:

```text
EVENT_BUS.md
```

---

# 51. UI-Local Delivery

UI-local event mechanisms may have platform-specific delivery semantics.

They remain implementation details and must not redefine CRAI Event Bus guarantees.

---

# 52. Event Publication Timing

For UI-local facts:

```text
local state transition
    ↓
commit local state
    ↓
publish local event
```

Example:

```text
view becomes visible
    ↓
ViewOpened
```

---

# 53. Event Publication Failure

Failure to publish an optional UI-local event does not roll back valid UI state.

Example:

```text
ViewOpened state committed
    ↓
analytics/local event subscriber fails
```

The view remains open.

---

# 54. Business Event Publication

UI Adapter should rarely publish business Event Bus facts.

If it does, the fact must:

1. be UI Adapter-owned;
2. be stable after publication;
3. have a genuine asynchronous consumer;
4. not be just telemetry or local interaction.

---

# 55. User Intent Is Not a Published Fact

Example:

```text
user clicked retry
```

should normally become:

```text
RetryCurrentOperationIntent
```

not:

```text
UserClickedRetry Event Bus event
```

---

# 56. UI Telemetry

Events such as:

```text
ViewOpened
NotificationShown
ShortcutTriggered
```

may be useful for telemetry.

If so, instrument them through Diagnostics/telemetry abstractions.

Do not automatically publish them to the business Event Bus for analytics.

---

# 57. `WindowResized`

The v1 future candidate:

```text
WindowResized
```

is UI-local and high-frequency.

Keep it out of business Event Bus.

---

# 58. `OverlayActivated`

Usually UI-local.

If activation has actual business semantics, translate it into a semantic intent instead.

---

# 59. `OverlayHidden`

UI-local presentation/frontend fact.

It does not mean:

```text
ReadingSessionPaused
```

unless Application policy explicitly maps the action.

---

# 60. `ShortcutTriggered`

Platform/native event.

Translate into semantic UiIntent.

Example:

```text
ShortcutTriggered
    ↓
ToggleOverlayIntent
```

---

# 61. `ExtensionConnected`

May be represented as:

```text
FrontendConnected
```

if browser extension UI joins the application.

Still primarily UI/frontend lifecycle.

---

# 62. `ExtensionDisconnected`

Preferred generalized form:

```text
FrontendDisconnected
```

---

# 63. Error Events

Normal UI Adapter errors are:

```text
UiAdapterError / operation result
```

not global Event Bus events.

Example:

```text
NavigationInvalid
```

should be returned to the local caller/projection.

---

# 64. No `UiErrorOccurred` Event by Default

Do not publish every UI error to Event Bus.

Diagnostics may observe UI Adapter errors through:

```text
ObserveError
```

---

# 65. UI Adapter State Events

Potential UI Adapter-owned state facts:

```text
UiAdapterDegraded
UiAdapterRecovered
UiCapabilityChanged
FrontendConnected
FrontendDisconnected
```

should only be introduced if real Application consumers need them.

---

# 66. UiCapabilityChanged

Optional fact:

```text
UiCapabilityChanged
├── capability
├── previousState
├── state
├── frontendId?
├── reasonCode?
└── changedAt
```

Useful when Application must adapt UX to capability changes.

---

# 67. UiAdapterDegraded

Optional fact representing meaningful degradation of the adapter's required capability set.

This is not emitted for every failed dialog or notification.

---

# 68. UiAdapterRecovered

Optional fact after recovery from an adapter-level degradation.

---

# 69. FrontendDisconnected Event Use

Application may need asynchronous awareness when an important frontend disappears.

Example:

```text
overlay frontend disconnected
```

Application may decide whether to keep the reading use case alive.

The UI Adapter itself does not make that business decision.

---

# 70. Event Payload Safety

UI events must not expose:

```text
raw screenshot
OCR text
translation text
credential
token
cookie
private key
native control object
native window handle
```

---

# 71. View Metadata Safety

Avoid putting raw user content in:

```text
ViewOpened metadata
NavigationCompleted metadata
NotificationShown metadata
```

Use opaque identifiers where possible.

---

# 72. Notification Privacy

Do not place full notification body containing sensitive reading content into global event payloads.

Notification telemetry should use safe codes/identifiers.

---

# 73. Accessibility Privacy

Accessibility application events must not include arbitrary full reading content as metadata.

---

# 74. Event Payload Size

UI events should remain small.

Avoid:

```text
entire ViewModel
Presentation Artifact
full settings object
diagnostic bundle
image data
```

in Event Bus messages.

---

# 75. ViewModel Updates Are Not Events by Default

A new:

```text
ViewModel N+1
```

may be published through local observable/projection mechanism.

It does not require:

```text
ViewModelUpdated
```

business Event Bus event.

---

# 76. Query vs Event

Queries/snapshots answer:

> What should the UI show now?

Events answer:

> What stable fact changed?

Do not reconstruct all UI state from event history unless explicitly designing an event-sourced projection.

---

# 77. Event-Sourced UI Is Not Required

UI Adapter v2 does not require an event-sourced frontend.

Simple immutable projection replacement is acceptable.

---

# 78. Multiple Frontends

UI-local events may include:

```text
frontendId
```

to separate:

```text
desktop
overlay
extension
mobile
```

state.

---

# 79. Frontend Event Isolation

An event from one frontend must not accidentally mutate UI-local state of another frontend unless a shared application action is intentionally produced.

---

# 80. Example — Start Reading

```text
Native Start Button Click
        ↓
StartReadingIntent
        ↓
Application
        ↓
application/domain changes
        ↓
ApplicationUiSnapshot
        ↓
UI Adapter
        ↓
ReaderViewModel
```

No UI business event is required.

---

# 81. Example — Dialog Confirmation

```text
DialogOpened
    ↓
user clicks Confirm
    ↓
DialogResponded
    ↓
DeleteSourceProfileIntent
    ↓
Application / Preferences
```

---

# 82. Example — Theme

```text
SavePreferenceIntent
theme = Dark
    ↓
Preferences
    ↓
Application effective appearance
    ↓
ApplyTheme
    ↓
AppearanceApplied
```

`AppearanceApplied` is UI-local.

---

# 83. Example — Session Override

```text
User changes language
for current session only
    ↓
ChangeSessionPreferenceIntent
    ↓
Reading Session/Application
    ↓
new authoritative session/application snapshot
    ↓
new ReaderViewModel
```

No global `EffectivePreferencesChanged` required.

---

# 84. Example — Translation Completion

Incorrect:

```text
TranslationCompleted
    ↓
UI Adapter directly refreshes text
```

Preferred:

```text
Translation result
    ↓
Business Pipeline / Presentation
    ↓
Application presentation state
    ↓
UI projection
```

---

# 85. Example — Diagnostics

```text
DiagnosticCapabilityChanged
    ↓
Application/Diagnostics projection
    ↓
DiagnosticsViewModel N+1
```

---

# 86. Example — Storage Error

```text
Storage error
    ↓
Storage/Application-owned result
    ↓
Error projection
    ↓
UI Adapter
```

UI Adapter does not need a mandatory `StorageFailed` subscription.

---

# 87. Example — UI Capability Loss

```text
SystemNotification
AVAILABLE → UNAVAILABLE
    ↓
UiCapabilityChanged
        [optional]
```

Main Reader UI remains active.

---

# 88. Example — Frontend Disconnect

```text
Overlay frontend disconnected
        ↓
FrontendDisconnected
        ↓
Application
        ↓
decides whether reading continues
```

UI Adapter does not cancel Runtime automatically.

---

# 89. Failure Handling

If an external event/projection cannot be processed:

```text
preserve previous valid ViewModel
discard invalid Candidate
record UiAdapterError
request/resync authoritative snapshot if appropriate
```

Do not mutate business state.

---

# 90. Stale Event Handling

If an event refers to older authoritative provenance:

```text
discard or re-query current state
```

according to projection policy.

Do not allow stale UI events to overwrite newer ViewModels.

---

# 91. Lost Event Recovery

Because UI state should be recoverable from current snapshots/projections where practical:

```text
missed optional event
    ↓
query current authoritative state
    ↓
rebuild ViewModel
```

This reduces coupling to perfect event delivery.

---

# 92. Architecture Invariants

1. Native UI events remain platform-local.

2. UI Adapter converts relevant interaction into UiIntent.

3. UiIntent is not automatically an Event Bus event.

4. Event Bus is not used as a UI event bus.

5. `ViewOpened` is UI-local by default.

6. `ViewClosed` is UI-local by default.

7. `NavigationCompleted` is UI-local by default.

8. Dialog lifecycle events are UI-local by default.

9. `NotificationShown` is UI-local by default.

10. `ThemeChanged` is replaced by UI-local `AppearanceApplied` where needed.

11. `LocalizationChanged` is replaced by UI-local `LocalizationApplied` where needed.

12. `EffectivePreferencesChanged` is not a consumed dependency.

13. `DiagnosticsUpdated` is not a consumed dependency.

14. `StorageReady` is not a mandatory consumed dependency.

15. `StorageFailed` is not a mandatory consumed dependency.

16. `TranslationCompleted` is not a mandatory UI dependency.

17. `RecognitionCompleted` is not a mandatory UI dependency.

18. UI Adapter does not reconstruct business state from every domain event.

19. Application projection is preferred for cross-module UI state.

20. Selected domain events may still be consumed when appropriate.

21. External event ownership remains with producer modules.

22. UI Adapter does not rename external facts into UI-owned business facts.

23. UI-local events publish after local state commit.

24. Event publication failure does not roll back valid UI state.

25. EventId is the event identity.

26. IntentId is the UiIntent identity.

27. CorrelationId is not a deduplication identity.

28. Timestamp is not a deduplication identity.

29. Event Bus delivery guarantees belong to canonical EVENT_BUS.md.

30. Event payloads remain platform-independent.

31. Event payloads remain privacy-safe.

32. Event payloads remain small.

33. Native handles never appear in public events.

34. UI telemetry uses Diagnostics/telemetry abstractions rather than business Event Bus by default.

35. One UI-local event failure does not change business state.

36. Lost optional UI events should be recoverable from authoritative snapshots where practical.

---

# 93. Testing — Native Event Boundary

Verify:

```text
pointer move
scroll
hover
focus
window resize
```

do not enter CRAI Event Bus by default.

---

# 94. Testing — UiIntent

Verify native user interaction produces semantic UiIntent with no native control object.

---

# 95. Testing — Dialog

Verify:

```text
DialogOpened
DialogResponded
```

remain local and produce a domain/application intent only when required.

---

# 96. Testing — Theme

Verify applying appearance does not publish a false persistent preference event.

---

# 97. Testing — Mandatory Subscription Removal

UI Adapter must remain functional without direct subscriptions to:

```text
EffectivePreferencesChanged
DiagnosticsUpdated
StorageReady
StorageFailed
TranslationCompleted
RecognitionCompleted
```

---

# 98. Testing — Projection Recovery

Drop one optional external event.

Verify UI can rebuild from current authoritative snapshot/projection.

---

# 99. Testing — Idempotency

Deliver same local/application event twice.

Verify `EventId` deduplication prevents duplicate logical effect where required.

---

# 100. Testing — Correlation

Verify multiple events/intents may share one CorrelationId without being treated as duplicates.

---

# 101. Testing — Frontend Isolation

Verify overlay/frontend events do not corrupt other frontend-local state.

---

# 102. Testing — Privacy

Verify event payloads exclude:

```text
raw image
OCR text
translation text
credential
token
native handle
unsafe window title
```

---

# 103. Testing — External Error

Inject Storage/Translation/Reading Session failure.

Verify UI Adapter projects it without publishing an incorrect UI-owned business event.

---

# 104. Related Documents

```text
doc/02-modules/ui-adapter/MODULE.md
doc/02-modules/ui-adapter/CONTRACT.md
doc/02-modules/ui-adapter/STATES.md
doc/02-modules/ui-adapter/ERRORS.md
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

# 105. Completion Criteria

This specification is synchronized when:

* native UI events are separated from UiIntent;
* UiIntent is separated from business events;
* UI-local events remain local by default;
* global Event Bus is not used for every UI interaction;
* mandatory consumed-event list is removed;
* `EffectivePreferencesChanged` dependency is absent;
* `DiagnosticsUpdated` dependency is absent;
* `StorageReady/StorageFailed` dependencies are absent;
* Recognition/Translation completion are not mandatory UI dependencies;
* Application projection is recognized as preferred cross-module UI input;
* event identity uses EventId only;
* CorrelationId and timestamp are not used for deduplication;
* Event Bus delivery semantics defer to canonical architecture;
* UI telemetry uses observability abstractions;
* privacy and native-object boundaries are explicit.

---

# 106. Summary

UI Adapter v2 separates:

```text
Native UI Event
    ↓
UiIntent
    ↓
Application Command
```

from:

```text
UI-local State Change
    ↓
UiLocalEvent
```

and from:

```text
Domain / Runtime Fact
    ↓
Application Projection
    ↓
ViewModel
```

The central rule is:

```text
UI events describe UI behavior.

Domain events describe domain facts.

UiIntent describes what the user wants.

These three concepts
must not be merged.
```
