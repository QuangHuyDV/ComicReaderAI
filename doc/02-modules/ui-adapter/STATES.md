# UI Adapter States

> **Project:** CRAI
> **Module:** `ui-adapter`
> **Path:** `doc/02-modules/ui-adapter/STATES.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the state model owned by the UI Adapter module.

It specifies:

```text
UI Adapter module lifecycle
frontend lifecycle
view lifecycle
UI-local interaction state
scoped UI operation phases
ViewModel revision semantics
capability degradation
recovery
shutdown
```

It does not define:

```text
Reading Session lifecycle
Runtime lifecycle
WorkItem lifecycle
Attempt lifecycle
Preference state
Presentation lifecycle
business processing state
```

---

# 2. State Ownership

UI Adapter owns:

```text
UiAdapterModuleState
FrontendState
ViewLifecycleState
UiOperationPhase
UiCapabilityState
ViewModelRevision
UI-local ephemeral state
```

It does not own:

```text
ReadingContextRevision
PreferenceRevision
RuntimeRevisionId
PresentationRevision
processing stage state
business retry state
```

---

# 3. State Domains

UI Adapter v2 separates four state domains:

```text
UI Adapter
├── Module Lifecycle
├── Frontend / View Lifecycle
├── Scoped UI Operations
└── UI Capability State
```

These state domains are independent.

---

# 4. Module Lifecycle

Recommended lifecycle:

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

# 5. `UNINITIALIZED`

## Meaning

UI Adapter has not initialized platform bindings or initial UI projections.

Characteristics:

```text
no public UI operation guaranteed
no active frontend bindings
no ViewModel projection guarantee
```

Allowed next:

```text
INITIALIZING
STOPPED
```

---

# 6. `INITIALIZING`

## Meaning

UI Adapter initializes its platform-facing capabilities.

Possible work:

```text
initialize frontend bindings
load localization access
load applied appearance
initialize navigation
prepare initial ViewModels
discover UI capabilities
validate accessibility bindings
```

Allowed next:

```text
READY
DEGRADED
STOPPING
```

---

# 7. Initialization Success

Normal flow:

```text
UNINITIALIZED
    ↓
INITIALIZING
    ↓
required UI capability available
    ↓
READY
```

Optional capability failure may lead to:

```text
DEGRADED
```

rather than total failure.

---

# 8. `READY`

## Meaning

UI Adapter core functionality is available.

While READY:

```text
multiple views may render
dialogs may be open
navigation may occur
user intents may be submitted
ViewModels may update
notifications may display
```

No global lifecycle transition is required for those operations.

---

# 9. READY Invariants

When READY:

```text
core adapter contracts available
native UI bindings usable
ViewModel projection available
privacy-safe UI boundary active
```

---

# 10. `DEGRADED`

## Meaning

UI Adapter remains usable but one or more optional or degradable UI capabilities are impaired.

Examples:

```text
system notifications unavailable
clipboard unavailable
localization fallback active
one optional overlay view unavailable
accessibility integration partially unavailable
one frontend disconnected
```

Core UI may remain functional.

---

# 11. DEGRADED Is Not Business Failure

UI Adapter becoming DEGRADED does not imply:

```text
Reading Session failed
Runtime failed
Translation failed
Presentation failed
```

It only describes UI adaptation capability.

---

# 12. `STOPPING`

## Meaning

UI Adapter is shutting down.

Possible actions:

```text
reject new UI use-case submissions
close/dispose views
release platform bindings
finish bounded local UI cleanup
unsubscribe local projections
```

---

# 13. `STOPPED`

## Meaning

UI Adapter resources are released.

No further operations are accepted.

`STOPPED` is terminal for that adapter instance.

---

# 14. Module Lifecycle Diagram

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

# 15. Removed Global `RENDERING`

The v1 state:

```text
Rendering
```

is removed from module lifecycle.

Reason:

multiple views may independently render or project state while UI Adapter remains READY.

Preferred:

```text
UI Adapter = READY

Reader View = PROJECTING
Settings View = VISIBLE
Diagnostics View = IDLE
```

---

# 16. Removed Global `NAVIGATING`

Navigation is a scoped UI-local operation.

It does not put the entire UI Adapter into:

```text
NAVIGATING
```

while all other views stop.

---

# 17. Removed Global `WAITING_FOR_USER`

A dialog waiting for user input does not mean the entire module is idle.

Example:

```text
Settings dialog waiting
Reader overlay still active
Diagnostics view still updating
```

Therefore `WAITING_FOR_USER` is not a module state.

---

# 18. Removed Global `UPDATING`

ViewModel updates are scoped projection operations.

The UI Adapter stays READY during normal updates.

---

# 19. Removed Global `FAILED`

A failed view/render/navigation operation does not normally make the whole UI Adapter unusable.

Use:

```text
operation failure
view failure
capability degradation
module DEGRADED
```

at the narrowest scope.

---

# 20. Frontend State

CRAI may have multiple UI frontends.

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

Examples of frontends:

```text
desktop main window
overlay frontend
browser extension panel
mobile frontend
```

---

# 21. Frontend Independence

One frontend may fail while another remains usable.

Example:

```text
Overlay Frontend = DEGRADED
Desktop Main Frontend = ACTIVE
```

UI Adapter module may remain READY or DEGRADED depending on frontend importance.

---

# 22. View Lifecycle

Recommended logical view states:

```text
CREATED
MOUNTED
VISIBLE
HIDDEN
DISPOSING
DISPOSED
```

---

# 23. `CREATED`

View identity exists but is not yet attached to an active frontend surface.

---

# 24. `MOUNTED`

The logical view is attached to a frontend.

It may not yet be visible.

---

# 25. `VISIBLE`

View is currently visible to the user.

---

# 26. `HIDDEN`

View remains mounted but is not currently visible.

---

# 27. `DISPOSING`

View is releasing UI-local resources.

---

# 28. `DISPOSED`

View lifecycle is terminal.

A disposed view instance is not revived.

A new view instance may be created.

---

# 29. View Lifecycle Diagram

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

---

# 30. View Lifecycle Is Not Business Lifecycle

Example:

```text
Reader View = HIDDEN
```

does not imply:

```text
Reading Session = stopped
```

Closing/hiding UI and stopping the reading use case are different commands.

---

# 31. Dialog State

Dialogs may use local states such as:

```text
CREATED
VISIBLE
AWAITING_RESPONSE
RESPONDED
DISMISSED
```

These states belong to one dialog instance.

---

# 32. Awaiting User Response

`AWAITING_RESPONSE` is scoped to the dialog.

It does not block:

```text
other views
Runtime processing
Reading Session
background diagnostics
```

unless the application use case explicitly waits for that response.

---

# 33. Notification State

Notification lifecycle may be:

```text
QUEUED
VISIBLE
DISMISSED
EXPIRED
FAILED_TO_DISPLAY
```

This remains UI-local.

---

# 34. Navigation Operation

Navigation is modeled as a scoped operation.

Recommended phases:

```text
VALIDATING
RESOLVING
APPLYING
COMPLETED
```

Possible outcomes:

```text
REJECTED
FAILED
CANCELLED
```

---

# 35. Navigation `VALIDATING`

Validate:

```text
target ViewId
navigation context
frontend capability
local navigation rules
```

No business processing occurs.

---

# 36. Navigation `RESOLVING`

Resolve:

```text
view definition
frontend destination
navigation stack change
```

---

# 37. Navigation `APPLYING`

Apply UI-local navigation state.

---

# 38. Navigation `COMPLETED`

Navigation finished successfully.

UI Adapter module remains READY.

---

# 39. ViewModel Projection Operation

Projection phases:

```text
READING_SOURCE_SNAPSHOTS
BUILDING
VALIDATING
PUBLISHING
COMPLETED
```

Possible outcomes:

```text
STALE
REJECTED
FAILED
```

---

# 40. Projection `BUILDING`

Construct Candidate ViewModel from authoritative source snapshots.

---

# 41. Projection `VALIDATING`

Verify:

```text
no native objects
privacy-safe fields
required localization metadata
required accessibility metadata
revision provenance
```

---

# 42. Projection `PUBLISHING`

Atomically replace the previous UI projection with the new immutable ViewModel.

---

# 43. ViewModel Candidate Isolation

Preferred:

```text
Current ViewModel N
+
Candidate ViewModel N+1
```

Existing ViewModel remains valid until Candidate publication succeeds.

---

# 44. Projection Failure

If Candidate projection fails:

```text
Candidate discarded
Current ViewModel remains valid
```

where safe.

Do not globally fail UI Adapter.

---

# 45. Stale Projection

A Candidate may become stale if its authoritative source snapshots are superseded before publication.

Outcome:

```text
STALE
```

The Candidate is discarded.

No business rollback occurs.

---

# 46. ViewModelRevision

UI Adapter may own:

```text
ViewModelRevision
```

for one logical view/projection stream.

Example:

```text
Reader ViewModel Revision 12
    ↓
new projection
    ↓
Revision 13
```

---

# 47. ViewModelRevision Is UI-Local

It is not:

```text
ReadingContextRevision
PreferenceRevision
PresentationRevision
RuntimeRevisionId
```

---

# 48. Source Revision Provenance

A ViewModel may record:

```text
readingContextRevision
preferenceRevision
presentationRevision
diagnostic snapshot identity
```

These remain externally owned.

---

# 49. UI Intent Operation

Application-facing UiIntent may use scoped phases:

```text
RECEIVED
VALIDATING
FORWARDING
AWAITING_RESULT
COMPLETED
```

Possible outcomes:

```text
REJECTED
FAILED
CANCELLED
```

---

# 50. Intent `VALIDATING`

UI Adapter validates only adapter-level rules such as:

```text
required UI fields
supported local capability
payload shape
```

It does not validate domain business rules authoritatively.

---

# 51. Intent `FORWARDING`

Intent is adapted to Application/module command.

---

# 52. Intent `AWAITING_RESULT`

UI may show a loading/progress projection while Application/Runtime performs long-running work.

UI Adapter itself does not execute that work.

---

# 53. Intent `COMPLETED`

Application/domain result has been projected back to UI.

---

# 54. Retry Intent

For:

```text
RetryCurrentOperationIntent
```

UI operation may be:

```text
RECEIVED
    ↓
FORWARDING
    ↓
AWAITING_RESULT
```

Runtime owns actual retry mechanics.

---

# 55. Preference Save Intent

For persistent preference update:

```text
SavePreferenceIntent
```

UI Adapter may display:

```text
SUBMITTING
AWAITING_RESULT
```

as view-local state.

Preferences owns validation and commit.

---

# 56. Session Override Intent

For session-only settings:

```text
ChangeSessionPreferenceIntent
```

Reading Session/Application owns authoritative mutation.

UI-local optimistic state is provisional only.

---

# 57. Optimistic UI State

UI may temporarily display:

```text
OPTIMISTIC
```

for a control/value.

This state is not authoritative.

---

# 58. Optimistic Confirmation

If domain commit succeeds:

```text
OPTIMISTIC
    ↓
CONFIRMED
```

through a new authoritative projection.

---

# 59. Optimistic Rejection

If domain rejects:

```text
OPTIMISTIC
    ↓
REVERTED
```

through authoritative projection.

UI Adapter does not rollback domain state.

---

# 60. UI Capability State

Each platform-facing capability may expose:

```text
AVAILABLE
DEGRADED
UNAVAILABLE
DISABLED
UNKNOWN
```

---

# 61. Capability Examples

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

---

# 62. `AVAILABLE`

Capability exists and operates normally.

---

# 63. `DEGRADED`

Capability works partially.

Examples:

```text
fallback localization active
limited accessibility support
overlay operating without animation
```

---

# 64. `UNAVAILABLE`

Capability is supported conceptually but cannot currently operate.

---

# 65. `DISABLED`

Capability intentionally disabled by configuration/policy.

This is not an error.

---

# 66. `UNKNOWN`

Capability status is not yet known.

---

# 67. Capability vs Module State

Example:

```text
SystemNotification = UNAVAILABLE
Clipboard = AVAILABLE
Reader UI = ACTIVE
```

UI Adapter may remain:

```text
READY
```

if notification capability is optional.

---

# 68. Required Capability Failure

If a required capability becomes unavailable:

```text
UI Adapter
    ↓
DEGRADED
```

or Application may decide the frontend cannot continue.

The capability itself remains the narrowest state owner.

---

# 69. Platform Binding Failure

Example:

```text
OverlayWindow
AVAILABLE → UNAVAILABLE
```

Possible result:

```text
overlay frontend = DEGRADED
UI Adapter = DEGRADED
main desktop frontend still ACTIVE
```

---

# 70. Localization Failure

Missing one localization resource may produce:

```text
Localization = DEGRADED
```

with safe fallback.

Do not automatically fail the entire UI Adapter.

---

# 71. Accessibility Failure

Optional accessibility integration failure may produce:

```text
Accessibility = DEGRADED / UNAVAILABLE
```

Application policy determines whether this is acceptable for the target product/profile.

---

# 72. Theme/Application Appearance

Applying theme is a scoped UI operation.

It does not create a module lifecycle state.

Possible phases:

```text
VALIDATING
APPLYING
COMPLETED
```

with:

```text
REJECTED
FAILED
```

outcomes.

---

# 73. Localization Apply Operation

Likewise:

```text
VALIDATING
LOADING_RESOURCE_SET
APPLYING
COMPLETED
```

Possible:

```text
FALLBACK_APPLIED
FAILED
```

---

# 74. UI Adapter Error Scope

An error affects the narrowest relevant state domain.

Example:

```text
one view projection failed
    → view projection operation FAILED

notification API unavailable
    → notification capability UNAVAILABLE

core platform binding corrupted
    → UI Adapter DEGRADED / STOPPING
```

---

# 75. Error-to-State Mapping

| Error Scope                      | State Effect                            |
| -------------------------------- | --------------------------------------- |
| Invalid UiIntent                 | None                                    |
| Invalid navigation               | Navigation operation rejected           |
| ViewModel construction failure   | Projection operation failed             |
| One dialog fails                 | Dialog operation failed                 |
| Notification unavailable         | Notification capability unavailable     |
| Missing localization key         | Localization degraded/fallback          |
| Optional capability unavailable  | Capability state only                   |
| Required frontend unavailable    | Frontend/UI Adapter degraded            |
| Core adapter invariant violation | DEGRADED or STOPPING                    |
| External business error          | No UI Adapter lifecycle change required |

---

# 76. External Error Independence

Errors from:

```text
Reading Session
Preferences
Runtime
Capture
Recognition
Translation
Presentation
Diagnostics
```

do not directly change UI Adapter module lifecycle.

They are projected into UI state.

---

# 77. Business Processing During UI Failure

Example:

```text
overlay view fails to render
```

does not automatically cancel:

```text
Translation Attempt
Reading Session
Runtime Revision
```

Application policy may decide otherwise only when user experience requires it.

---

# 78. Navigation Failure

Navigation failure should generally:

```text
preserve current View
return scoped error
```

not make the UI Adapter FAILED.

---

# 79. Projection Failure

If a new ViewModel cannot be constructed:

```text
previous ViewModel remains valid
```

where safe.

---

# 80. Dialog Failure

If one dialog cannot display:

```text
dialog operation = FAILED
```

The rest of UI continues.

---

# 81. Notification Failure

If system notification fails:

```text
Notification capability = DEGRADED / UNAVAILABLE
```

The main UI remains unaffected.

---

# 82. Frontend Disconnection

For remote/browser/mobile frontends:

```text
frontend connection lost
    ↓
frontend = INACTIVE / DEGRADED
```

Other frontends remain independent.

---

# 83. Multiple Frontends

Example:

```text
Desktop Main = ACTIVE
Overlay = ACTIVE
Browser Extension = INACTIVE
```

All may share application state without sharing UI-local state.

---

# 84. Shutdown

Preferred:

```text
READY / DEGRADED
        ↓
STOPPING
        ↓
dispose dialogs/views
        ↓
release frontend bindings
        ↓
STOPPED
```

---

# 85. Shutdown Is Bounded

UI Adapter shutdown must not wait indefinitely for:

```text
Runtime completion
Translation completion
remote telemetry
background provider response
```

Application owns coordination with long-running work.

---

# 86. Closing UI vs Stopping Business Work

Closing a window does not automatically imply:

```text
StopReading
CancelRuntime
```

unless Application policy explicitly maps that UI action to such intent.

---

# 87. Event Relationship

UI-local state transitions may produce local events.

Examples:

```text
ViewVisible
ViewHidden
NavigationCompleted
DialogResponded
```

These are not business Event Bus events by default.

---

# 88. Application/Domain Events

Authoritative external facts may trigger new UI projections.

Example:

```text
PreferenceChanged
    ↓
Application projection
    ↓
new SettingsViewModel
```

UI Adapter stays READY.

---

# 89. No `ApplicationEventReceived → Updating`

The v1 transition:

```text
WaitingForUser
    ↓
ApplicationEventReceived
    ↓
Updating
```

is removed.

Any ViewModel may update at any time while the adapter is READY.

---

# 90. No `UserIdle` Global Transition

User inactivity does not change the entire module lifecycle.

Idle state, when useful, is frontend/view-local.

---

# 91. State Concurrency

Multiple scoped operations may coexist.

Example:

```text
ReaderView projection = BUILDING
Settings navigation = COMPLETED
Dialog = AWAITING_RESPONSE
Notification = VISIBLE
```

while:

```text
UI Adapter = READY
```

---

# 92. Determinism

Within one scoped state machine:

```text
same committed state
+
same input
```

must yield the same valid transition semantics.

Global serialization across unrelated UI operations is not required.

---

# 93. Threading

Platform implementation may require a UI thread.

This is an implementation constraint.

Stable state contracts remain platform-independent.

---

# 94. UI Thread Rule

Heavy business work must never be performed synchronously as part of a UI state transition.

---

# 95. ViewModel Publication Atomicity

Consumers should observe either:

```text
ViewModel N
```

or:

```text
ViewModel N+1
```

not a partially constructed projection.

---

# 96. ViewModel Immutability

After publication:

```text
ViewModel N
```

must never mutate in place.

---

# 97. Projection Ordering

If Candidate:

```text
ViewModel N+2
```

completes before older Candidate:

```text
ViewModel N+1
```

the stale projection must not overwrite newer authoritative UI state.

---

# 98. Stale Projection Rule

Compare typed projection/source provenance.

Do not rely solely on completion time.

---

# 99. Module Recovery

Possible recovery:

```text
DEGRADED
    ↓
required capability recovers
    ↓
validate capability
    ↓
READY
```

---

# 100. View Recovery

A failed/disposed view may be recreated as a new view instance.

Do not mutate terminal:

```text
DISPOSED
```

back to VISIBLE.

---

# 101. Capability Recovery

Example:

```text
SystemNotification
UNAVAILABLE
    ↓
platform recovers
    ↓
AVAILABLE
```

No business processing restart occurs.

---

# 102. Application Recovery

If UI Adapter core cannot safely function:

```text
core invariant violation
    ↓
DEGRADED
or
STOPPING
```

Application may recreate the adapter/frontend.

---

# 103. Architecture Invariants — Module

1. UI Adapter has one small module lifecycle.

2. `READY` allows concurrent UI operations.

3. `DEGRADED` preserves working capabilities.

4. `STOPPED` is terminal.

5. One view failure does not imply module failure.

6. One optional capability failure does not imply module failure.

---

# 104. Architecture Invariants — Views

1. View lifecycle is independent from business lifecycle.

2. Multiple views may be active simultaneously.

3. ViewModels are immutable.

4. Candidate ViewModels are not externally visible.

5. Projection failure preserves previous valid ViewModel where possible.

6. Disposed view instances are terminal.

---

# 105. Architecture Invariants — Operations

1. Navigation is scoped.

2. Dialog waiting is scoped.

3. Rendering/projection is scoped.

4. User intent submission is scoped.

5. Long-running business processing remains external.

6. External business failure does not automatically change UI Adapter lifecycle.

---

# 106. Architecture Invariants — Ownership

1. UI Adapter does not own Reading Session state.

2. UI Adapter does not own Runtime state.

3. UI Adapter does not own Preferences state.

4. UI Adapter does not own Presentation semantics.

5. UI Adapter does not execute retry policy.

6. Source revisions remain owned by original modules.

7. ViewModelRevision is UI-local only.

---

# 107. Removed v1 Global States

Removed:

```text
Rendering
Navigating
WaitingForUser
Updating
Failed
Shutdown
```

as global lifecycle states.

Replaced with:

```text
READY normal operation
DEGRADED partial capability loss
STOPPING
STOPPED

ViewLifecycleState
UiOperationPhase
UiCapabilityState
```

---

# 108. State Transition Table — Module Lifecycle

| Current         | Trigger                         | Next           |
| --------------- | ------------------------------- | -------------- |
| `UNINITIALIZED` | Initialize                      | `INITIALIZING` |
| `INITIALIZING`  | Required capabilities available | `READY`        |
| `INITIALIZING`  | Partial capability availability | `DEGRADED`     |
| `READY`         | Required capability degraded    | `DEGRADED`     |
| `DEGRADED`      | Required capability recovered   | `READY`        |
| `READY`         | Shutdown                        | `STOPPING`     |
| `DEGRADED`      | Shutdown                        | `STOPPING`     |
| `INITIALIZING`  | Shutdown                        | `STOPPING`     |
| `STOPPING`      | Cleanup complete                | `STOPPED`      |

---

# 109. State Transition Table — View

| Current     | Trigger          | Next        |
| ----------- | ---------------- | ----------- |
| `CREATED`   | Mount            | `MOUNTED`   |
| `MOUNTED`   | Show             | `VISIBLE`   |
| `VISIBLE`   | Hide             | `HIDDEN`    |
| `HIDDEN`    | Show             | `VISIBLE`   |
| `MOUNTED`   | Dispose          | `DISPOSING` |
| `VISIBLE`   | Dispose          | `DISPOSING` |
| `HIDDEN`    | Dispose          | `DISPOSING` |
| `DISPOSING` | Cleanup complete | `DISPOSED`  |

---

# 110. State Transition Table — Projection

| Phase                      | Outcome         | Next/Result  |
| -------------------------- | --------------- | ------------ |
| `READING_SOURCE_SNAPSHOTS` | Sources ready   | `BUILDING`   |
| `BUILDING`                 | Candidate built | `VALIDATING` |
| `BUILDING`                 | Failure         | `FAILED`     |
| `VALIDATING`               | Valid           | `PUBLISHING` |
| `VALIDATING`               | Stale           | `STALE`      |
| `VALIDATING`               | Invalid         | `REJECTED`   |
| `PUBLISHING`               | Success         | `COMPLETED`  |
| `PUBLISHING`               | Superseded      | `STALE`      |

---

# 111. State Transition Table — Intent

| Phase             | Outcome                     | Next/Result       |
| ----------------- | --------------------------- | ----------------- |
| `RECEIVED`        | Begin                       | `VALIDATING`      |
| `VALIDATING`      | Invalid adapter input       | `REJECTED`        |
| `VALIDATING`      | Valid                       | `FORWARDING`      |
| `FORWARDING`      | Accepted downstream         | `AWAITING_RESULT` |
| `FORWARDING`      | Immediate rejection         | `REJECTED`        |
| `AWAITING_RESULT` | Result arrives              | `COMPLETED`       |
| `AWAITING_RESULT` | Adapter operation cancelled | `CANCELLED`       |

---

# 112. Testing — Module Lifecycle

Verify:

```text
UNINITIALIZED → INITIALIZING
INITIALIZING → READY
INITIALIZING → DEGRADED
READY ↔ DEGRADED
READY → STOPPING → STOPPED
```

---

# 113. Testing — Concurrent UI Activity

Verify:

```text
dialog waiting
+
reader projection
+
navigation
+
notification display
```

can coexist while module remains READY.

---

# 114. Testing — Projection Isolation

Inject Candidate ViewModel failure.

Verify:

```text
previous ViewModel remains visible
module remains READY
```

where safe.

---

# 115. Testing — View Failure

Fail one view.

Verify another view remains operational.

---

# 116. Testing — Capability Isolation

Disable:

```text
SystemNotification
```

Verify core Reader UI remains usable.

---

# 117. Testing — External Failure Independence

Inject:

```text
Translation failure
Runtime retry exhaustion
Preference validation error
Reading Session revision conflict
```

Verify UI Adapter projects the error without entering global failure state.

---

# 118. Testing — Stale Projection

Produce two Candidate ViewModels out of order.

Verify stale Candidate cannot replace the newer projection.

---

# 119. Testing — Shutdown

Verify shutdown:

```text
disposes UI-local resources
does not wait indefinitely for Runtime
reaches STOPPED
```

---

# 120. Related Documents

```text
doc/02-modules/ui-adapter/MODULE.md
doc/02-modules/ui-adapter/CONTRACT.md
doc/02-modules/ui-adapter/EVENTS.md
doc/02-modules/ui-adapter/ERRORS.md
doc/02-modules/ui-adapter/README.md

doc/02-modules/reading-session/
doc/02-modules/preferences/
doc/02-modules/presentation/
doc/02-modules/diagnostics/

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
└── PIPELINE_RUNTIME.md
```

---

# 121. Completion Criteria

This specification is synchronized when:

* module lifecycle is reduced to initialization/readiness/degradation/shutdown;
* `Rendering` is removed as global module state;
* `Navigating` is removed as global module state;
* `WaitingForUser` is removed as global module state;
* `Updating` is removed as global module state;
* global `Failed` is absent for ordinary UI failures;
* view lifecycle is explicit;
* dialogs have scoped waiting state;
* navigation has scoped operation state;
* ViewModel projection has scoped phases;
* ViewModel Candidate isolation is explicit;
* ViewModelRevision is separated from domain revisions;
* capability-specific degradation is explicit;
* external business failures do not mutate UI Adapter lifecycle;
* multiple UI operations may coexist;
* shutdown remains bounded.

---

# 122. Summary

UI Adapter v2 separates:

```text
Module Lifecycle
+
View Lifecycle
+
Scoped UI Operations
+
Capability State
```

Module lifecycle:

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

View lifecycle:

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

ViewModel projection:

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

The central state rule is:

```text
UI Adapter module state describes
whether the adapter is usable.

View state describes
one UI view.

Operation state describes
one interaction/projection.

These state domains must not be merged.
```
