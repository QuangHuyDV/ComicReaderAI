# CRAI Reading Session Flow

> **Project:** CRAI
> **Path:** `doc/01-architecture/flows/READING_SESSION_FLOW.md`
> **Version:** 1.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the architecture-wide flow of a CRAI Reading Session.

It explains how a user reading activity moves through:

```text
user intent
    ↓
Application coordination
    ↓
Reading Session lifecycle
    ↓
ReadingContext
    ↓
ReadingContextRevision
    ↓
Business Execution Planning
    ↓
RuntimeRevision
    ↓
processing / presentation
```

This document focuses on the relationship between:

```text
Reading Session
Application
Preferences
Runtime
processing modules
Presentation
UI Adapter
```

It does not redefine their individual contracts or state machines.

---

# 2. Why This Flow Exists

Reading Session is one of the central domain authorities in CRAI.

Without an explicit session flow, it is easy to incorrectly assume:

```text
Reading Session
    owns current OCR state

Reading Session
    owns Translation state

Reading Session
    owns pipeline retry

Reading Session
    owns Runtime cancellation

Reading Session
    owns current Presentation state
```

Runtime v2 explicitly rejects those assumptions.

The Reading Session owns:

```text
the user's reading activity
+
its authoritative reading context
```

not processing execution.

---

# 3. Core Rule

The central rule is:

```text
Reading Session
    owns reading authority.

Runtime
    owns execution authority.
```

Their authoritative revisions are distinct:

```text
ReadingContextRevision
    ≠
RuntimeRevisionId
```

---

# 4. Main Participants

The session flow may involve:

```text
User

UI Adapter

Application

Reading Session

Preferences

Business Pipeline Orchestration

Runtime

Capture

Recognition

Text Processing

Translation

Presentation

Diagnostics
```

Not every session uses every processing module.

---

# 5. Supporting Infrastructure

Possible supporting infrastructure:

```text
Configuration

Storage

Event Bus

Logging

Telemetry

Secret Management

Scheduler

Resource Manager
```

Infrastructure provides mechanisms.

It does not own Reading Session semantics.

---

# 6. Main Authorities

| Concern                            | Owner                           |
| ---------------------------------- | ------------------------------- |
| User interaction adaptation        | UI Adapter                      |
| Cross-module use-case coordination | Application                     |
| Session lifecycle                  | Reading Session                 |
| ReadingContext                     | Reading Session                 |
| ReadingContextRevision             | Reading Session                 |
| Persistent preferences             | Preferences                     |
| Business execution requirements    | Business Pipeline Orchestration |
| RuntimeRevision                    | Runtime                         |
| WorkItem                           | Runtime                         |
| Attempt                            | Runtime                         |
| Semantic processing Artifacts      | Producing modules               |
| PresentationArtifact               | Presentation                    |
| ViewModel                          | UI Adapter                      |

---

# 7. Session Identity

Each Reading Session has:

```text
SessionId
```

The SessionId identifies one logical reading activity.

A session may span:

```text
many content changes
many Runtime revisions
many WorkItems
many Attempts
many Presentation updates
```

without changing SessionId.

---

# 8. Session Lifecycle

The canonical lifecycle is defined in:

```text
02-modules/reading-session/STATES.md
```

Architecture-level shape:

```text
CREATED
   ↓
READY
   ↓
ACTIVE
 ↕
PAUSED
   ↓
STOPPING
   ↓
STOPPED
```

This file does not create another competing state machine.

---

# 9. CREATED

A Reading Session exists but may not yet have enough valid context to begin reading.

Possible conditions:

```text
SessionId allocated

initial SessionConfiguration established

source not yet selected

required context incomplete
```

---

# 10. READY

The session has sufficient valid context to become active.

Examples:

```text
valid source selected

required language/configuration known

required session capabilities available
```

READY does not mean processing is currently executing.

---

# 11. ACTIVE

The session is actively authorizing reading behavior.

While ACTIVE:

```text
source may be observed

new ReadingContext changes may occur

Runtime revisions may be created

processing may occur

Presentation may update
```

Reading Session remains ACTIVE while Runtime work executes.

---

# 12. PAUSED

PAUSED means:

```text
the Reading Session
temporarily stops authorizing
normal new reading activity
```

It does not mean:

```text
all provider work stopped instantly
```

Actual WorkItem/Attempt cancellation is Runtime-owned.

---

# 13. STOPPING

The session is terminating.

Typical work:

```text
stop accepting session mutations

request relevant Runtime cancellation

stop session-authorized source observation

release session-owned resources

finalize permitted persistence
```

---

# 14. STOPPED

The logical session instance is terminal.

A stopped session should normally not become ACTIVE again.

A future reading activity should use a new SessionId unless Reading Session contracts explicitly define restoration semantics.

---

# 15. Session Creation Flow

User intent:

```text
StartReadingIntent
```

Typical architecture flow:

```text
User
    ↓
UI Adapter
    ↓
Application
    ↓
Create Reading Session
    ↓
Reading Session
    ↓
SessionId
```

The UI Adapter does not create SessionId itself.

---

# 16. Session Creation Does Not Start Processing Automatically

Creating:

```text
SessionId
```

does not automatically mean:

```text
Capture starts
Recognition starts
Translation starts
```

The session first requires authoritative context.

---

# 17. Source Selection

The user chooses or supplies a reading source.

Examples:

```text
screen region
application window
browser content
image file
clipboard text
clipboard image
future document source
```

Platform-specific source selection is normalized before becoming Reading Session context.

---

# 18. Source Selection Flow

Conceptually:

```text
Native source selection
    ↓
UI / Platform Adapter
    ↓
normalized source description
    ↓
Application
    ↓
Reading Session
```

Reading Session validates whether the source may become part of its ReadingContext.

---

# 19. ReadingContext

ReadingContext represents the currently committed reading context of the session.

Conceptually:

```text
ReadingContext
├── source
├── source mode
├── language configuration
├── session-specific configuration
├── selected area / source locator?
├── reading mode
└── other session-owned context
```

Exact schema remains Reading Session-owned.

---

# 20. ReadingContext Is Domain Authority

ReadingContext answers:

```text
What is this user currently reading?

Under what session-specific context?
```

It does not answer:

```text
Which Translation Attempt is running?

Which provider is executing?

How many retries occurred?
```

Those belong to Runtime.

---

# 21. ReadingContextRevision

Every committed semantic context change creates:

```text
ReadingContextRevision N+1
```

where required by Reading Session contracts.

Examples:

```text
capture region changed

source changed

session source language changed

target language changed

session-only processing option changed
```

---

# 22. ReadingContextRevision Is Not Screen Frame Revision

A continuously changing comic screen does not necessarily create:

```text
ReadingContextRevision
```

for every frame.

Example:

```text
same selected screen region
same session settings
user scrolls comic
```

may retain:

```text
ReadingContextRevision C1
```

while producing new Runtime/content work.

---

# 23. Context Change vs Content Change

These must remain distinct.

## Context Change

Example:

```text
user selects a different capture region
```

Possible consequence:

```text
ReadingContextRevision C1
    ↓
C2
```

## Content Change

Example:

```text
comic scrolls inside same capture region
```

Possible consequence:

```text
ReadingContextRevision stays C1

Runtime/content authority changes
```

---

# 24. Why This Distinction Matters

If every visual content change increments ReadingContextRevision:

```text
Reading Session
```

would accidentally become the owner of frame/content execution identity.

Runtime v2 instead separates:

```text
domain context
```

from:

```text
execution/content processing authority
```

---

# 25. Preferences Relationship

Persistent preferences belong to:

```text
Preferences
```

Examples:

```text
default language

preferred provider policy

font preference

theme preference

OCR preference

translation style preference
```

Reading Session does not persistently own these values.

---

# 26. Session Overrides

Reading Session may own temporary overrides.

Example:

```text
default target language = Vietnamese

this session:
use Japanese → Vietnamese
```

The session override remains session-owned.

The persistent default remains Preferences-owned.

---

# 27. Effective Configuration

Application may resolve effective execution configuration from:

```text
persistent Preferences
+
source/profile configuration
+
Reading Session overrides
+
capability availability
```

Conceptually:

```text
Preferences
      \
       \
Reading Session
         ↓
Application Configuration Resolution
         ↓
Effective Execution Snapshot
```

---

# 28. No Global EffectivePreferences Authority

Do not create a mutable architecture-wide object:

```text
EffectivePreferences
```

owned by Preferences.

Effective configuration is contextual.

It may depend on:

```text
Session
Source
Runtime capability
Use case
```

---

# 29. Session Activation

Once valid reading context exists:

```text
READY
    ↓
ACTIVE
```

may occur.

Activation authorizes normal reading behavior.

---

# 30. Activation Does Not Mean One Pipeline Instance

Deprecated:

```text
Session ACTIVE
    ↓
create PipelineId
    ↓
pipeline executes until completed
```

Runtime v2 may create many Runtime revisions and WorkItems during one ACTIVE session.

---

# 31. Business Execution Planning

Application uses the current authoritative context to determine what logical work is required.

Conceptually:

```text
ReadingContextRevision
+
effective configuration
+
current source/content conditions
    ↓
Business Pipeline Orchestration
    ↓
BusinessExecutionPlan
```

---

# 32. BusinessExecutionPlan

BusinessExecutionPlan describes:

```text
logical required work

dependencies

conditional paths

semantic input/output requirements
```

It does not own:

```text
Attempt lifecycle

queue scheduling

provider execution

retry timing
```

---

# 33. RuntimeRevision Creation

Runtime accepts the current execution requirements and establishes:

```text
RuntimeRevisionId
```

Conceptually:

```text
BusinessExecutionPlan
    ↓
Runtime
    ↓
RuntimeRevision R1
```

---

# 34. RuntimeRevision Is Execution Authority

RuntimeRevision identifies:

```text
which execution context
is currently authoritative
```

It is not Reading Session domain state.

---

# 35. Relationship Between Revisions

Typical:

```text
ReadingContextRevision C1
    ↓
RuntimeRevision R1
```

Later:

```text
same ReadingContextRevision C1
    ↓
new stable content
    ↓
RuntimeRevision R2
```

Therefore:

```text
one ReadingContextRevision
may correspond to
many RuntimeRevisions
```

---

# 36. Context Revision Change

If ReadingContext changes:

```text
C1
    ↓
C2
```

Application must re-evaluate execution needs.

Typically:

```text
C2
    ↓
new RuntimeRevision
```

Old Runtime work may become obsolete.

---

# 37. Processing While Session Remains ACTIVE

Example:

```text
Reading Session = ACTIVE

RuntimeRevision R5

Capture Attempt = RUNNING

Recognition WorkItem = READY

Translation from R4 = SUPERSEDED

PresentationArtifact P3 = current
```

All may coexist.

There is no need for Reading Session states such as:

```text
CAPTURING

OCR_PROCESSING

TRANSLATING

RENDERING
```

---

# 38. Semantic Artifact Flow

Depending on source mode:

```text
CaptureArtifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
```

Reading Session does not own these Artifacts.

---

# 39. Session References to Artifacts

Reading Session may retain safe references required for its domain behavior.

It must not redefine Artifact semantics.

Example:

```text
currentPresentationArtifactId?
```

may be a reference if the Reading Session contract actually needs it.

The Presentation Artifact remains Presentation-owned.

---

# 40. Session Progress

Product UI may need:

```text
processing...

translated

waiting for source

degraded
```

These are often aggregate projections.

Do not add them automatically to Reading Session lifecycle.

---

# 41. Progress Projection

Preferred:

```text
Reading Session state
+
Runtime state
+
module capability state
+
current Artifact state
    ↓
Application projection
    ↓
ReaderViewModel
```

---

# 42. Pause Flow

User chooses Pause.

```text
User
    ↓
PauseReadingIntent
    ↓
UI Adapter
    ↓
Application
    ↓
Reading Session
    ↓
ACTIVE → PAUSED
```

---

# 43. Pause and Runtime

After pause commits:

```text
Application
    ↓
Runtime
```

may:

```text
suppress new work

cancel selected active work

allow selected work to complete

retain current Presentation
```

according to current policy.

Reading Session does not directly iterate through Runtime Attempts.

---

# 44. Pause Does Not Destroy ReadingContext

Normally:

```text
PAUSED
```

retains:

```text
SessionId

ReadingContext

ReadingContextRevision

session configuration
```

so that reading may resume.

---

# 45. Pause Does Not Imply Context Revision

A simple:

```text
ACTIVE → PAUSED
```

does not necessarily change:

```text
ReadingContextRevision
```

because reading context itself may be unchanged.

---

# 46. Resume Flow

```text
User
    ↓
ResumeReadingIntent
    ↓
Application
    ↓
Reading Session
    ↓
PAUSED → ACTIVE
```

Before new execution:

```text
current context validity
capability availability
source validity
```

may be re-evaluated.

---

# 47. Resume Does Not Restart Old Attempts

If previous Attempts were cancelled or interrupted:

```text
do not revive the same Attempt
```

Runtime creates new executable work/Attempts when required.

---

# 48. Source Change Flow

User changes source:

```text
Source A
    ↓
Source B
```

Architecture flow:

```text
UI Adapter
    ↓
Application
    ↓
Reading Session
    ↓
commit ReadingContextRevision C2
    ↓
Application replans execution
    ↓
RuntimeRevision R2
```

---

# 49. Region Change Flow

For screen reading:

```text
region A
    ↓
region B
```

typically changes ReadingContext.

Therefore:

```text
ReadingContextRevision C1
    ↓
C2
```

Old Runtime work becomes obsolete.

---

# 50. Language Change Flow

Session-only language change:

```text
Chinese → Vietnamese
    ↓
English → Vietnamese
```

may require:

```text
new ReadingContextRevision
```

because semantic processing requirements changed.

---

# 51. Persistent Preference Change During Session

Example:

```text
user changes default translation provider
```

Preferences commits:

```text
PreferenceRevision P2
```

Application determines whether this affects the current active session.

Do not automatically mutate ReadingContext.

---

# 52. Preference Impact Evaluation

Possible outcomes:

```text
persistent preference changes only for future sessions

current session effective configuration changes

current Runtime work must be superseded

no processing change required
```

Application owns this use-case evaluation.

---

# 53. User Retranslation Flow

User asks:

```text
Retry / Retranslate current content
```

UI Adapter sends:

```text
RetryCurrentOperationIntent
```

Application determines the relevant action.

It must not assume:

```text
Translation module retry its current provider request
```

---

# 54. Runtime Retranslation

Possible flow:

```text
current semantic input
    ↓
Application
    ↓
Runtime
    ↓
new Translation WorkItem / Attempt
```

The exact execution depends on current Runtime/business state.

---

# 55. Content Change During Session

For continuous reading:

```text
Reading Session = ACTIVE
```

content may change without changing session context.

Example:

```text
user scrolls comic
```

Capture/source observation detects new content.

Application/Runtime updates execution authority.

---

# 56. Content Change Flow Relationship

Detailed continuous-content behavior belongs to:

```text
01-architecture/flows/CONTENT_CHANGE_FLOW.md
```

This Reading Session flow only establishes:

```text
content change
does not automatically mean
ReadingContextRevision change
```

---

# 57. Supersession

When new execution authority replaces old work:

```text
RuntimeRevision R1
    ↓
RuntimeRevision R2
```

R1 becomes:

```text
superseded
```

according to Runtime rules.

---

# 58. Session Does Not Commit Runtime Supersession

Reading Session may cause a domain condition that requires supersession.

Runtime commits the actual execution transition.

---

# 59. Stale Result Protection

Reading Session contributes domain authority:

```text
SessionId
ReadingContextRevision
```

Runtime contributes execution authority:

```text
RuntimeRevisionId
WorkItemId
AttemptId
```

Producing modules contribute Artifact semantics/provenance.

Publication checks use the appropriate combination.

---

# 60. Session Stop Flow

User chooses Stop:

```text
User
    ↓
StopReadingIntent
    ↓
UI Adapter
    ↓
Application
    ↓
Reading Session
```

Reading Session transitions toward:

```text
STOPPING
```

---

# 61. Runtime Stop Coordination

Application requests Runtime to stop/supersede work associated with the session.

Conceptually:

```text
Session S stopping
    ↓
Runtime cancellation/supersession
    ↓
affected WorkItems/Attempts
```

Runtime owns actual execution state changes.

---

# 62. Stop and Source Observation

Session-authorized source observation should stop.

Capture itself may remain globally usable for future sessions.

Do not treat:

```text
Session STOPPED
```

as:

```text
Capture module STOPPED
```

---

# 63. Stop and Published Artifacts

Published Artifacts may:

```text
remain cached
remain in history
remain available for export
be released
```

depending on retention policy.

Stopping a session does not redefine their semantic ownership.

---

# 64. Stop Completion

After required session cleanup:

```text
STOPPING
    ↓
STOPPED
```

`STOPPED` is terminal for that logical session.

---

# 65. Application Shutdown

Application shutdown may stop active sessions.

Conceptually:

```text
Application STOPPING
    ↓
active Reading Sessions → STOPPING
    ↓
Runtime cancellation
    ↓
bounded cleanup
```

---

# 66. Crash Recovery

After unexpected process termination:

```text
do not restore Runtime Attempts as RUNNING
```

Potential recovery flow:

```text
application restart
    ↓
restore durable session/configuration data
    ↓
create/recover valid Reading Session context
    ↓
revalidate source
    ↓
plan current work
    ↓
new RuntimeRevision / Attempts
```

---

# 67. Recoverable Session Data

Possible durable data:

```text
SessionConfiguration

ReadingContext if safe/useful

reading position

source profile reference

pause state

user-approved history
```

Exact persistence belongs to Reading Session/Storage contracts.

---

# 68. Ephemeral Session Data

Normally ephemeral:

```text
live provider request

Attempt execution handle

native window handle

screen buffer

temporary Candidate Artifact

UI control state
```

These must not be restored as Reading Session authority.

---

# 69. Session Recovery vs Session Revival

Architecture may choose either:

```text
restore prior logical SessionId
```

or:

```text
create new SessionId
using recovered configuration/context
```

This remains a Reading Session design decision.

It must be explicit before implementation.

---

# 70. Source Lost

If the current source disappears:

```text
window closed

browser tab unavailable

capture permission revoked

file removed
```

Application/Reading Session evaluate whether:

```text
session remains ACTIVE but degraded

session moves PAUSED

user must choose another source

session stops
```

Do not hardcode this flow generically.

---

# 71. Capability Degradation

A provider/capability may fail while Reading Session remains ACTIVE.

Example:

```text
Translation Provider A unavailable
```

does not automatically mean:

```text
Reading Session FAILED
```

Runtime/provider policy may recover.

---

# 72. Previous Presentation

During new work or temporary degradation:

```text
previous valid PresentationArtifact
```

may remain visible.

Session lifecycle does not need a separate `DISPLAYING` state.

---

# 73. Session and UI Lifecycle

Closing a Reader view does not automatically stop Reading Session.

Possible UI behavior:

```text
ViewClosed
```

may mean:

```text
hide UI only
```

or:

```text
StopReadingIntent
```

depending on explicit Application policy.

---

# 74. Session and Frontend Failure

If one frontend fails:

```text
UI Adapter frontend unavailable
```

the Reading Session may remain ACTIVE.

A later frontend may project the same current session state.

---

# 75. Multiple Frontends

One Reading Session may theoretically be projected through:

```text
desktop window

overlay

browser extension
```

without creating multiple session authorities.

---

# 76. Multiple Sessions

Architecture may eventually support multiple sessions:

```text
Session A = ACTIVE

Session B = PAUSED
```

Runtime may interleave work across them.

MVP may restrict active-session count through Application policy.

---

# 77. Session Selection

If multiple sessions exist, UI/Application may track:

```text
selectedSessionId
```

This is not necessarily domain ownership inside Reading Session.

---

# 78. Reading Position

Reading Session may own reading-position semantics.

Examples:

```text
chapter position

page index

viewport anchor

source locator
```

Exact representation depends on reading mode.

---

# 79. Position Change vs Context Change

A reading-position change may or may not produce:

```text
ReadingContextRevision
```

depending on whether the position affects semantic processing authority.

This must be defined by Reading Session contract.

---

# 80. Screen Reading Position

For screen-region reading, individual frame changes should not automatically become ReadingContextRevision.

Source observation/Runtime handles content freshness.

---

# 81. Structured Reading Position

For structured novels:

```text
chapter / paragraph / viewport
```

may be stored as reading position independently of Translation execution.

---

# 82. Session Configuration

SessionConfiguration may contain:

```text
source mode
session-specific language choices
temporary behavior options
presentation mode selection?
```

Only Reading Session-owned semantics belong here.

---

# 83. Presentation Mode Ownership

If a presentation mode is temporary session behavior:

```text
side-panel
overlay
reader
```

Reading Session/Application may reference the selected mode.

Presentation owns semantic Presentation output.

UI Adapter owns frontend adaptation.

---

# 84. Theme Is Not Session-Owned by Default

Persistent:

```text
theme
font
locale
```

normally belong to Preferences.

The session may reference resolved values without becoming their persistent owner.

---

# 85. Session Error Ownership

Reading Session owns only session-specific errors.

Examples:

```text
invalid context transition

ReadingContext revision conflict

invalid session lifecycle operation
```

It does not rename:

```text
REC-*

TRN-*

RUN-*
```

into Session errors.

---

# 86. Runtime Failure During Session

Example:

```text
Translation Attempt failed
```

Possible state:

```text
Reading Session = ACTIVE

Runtime WorkItem = failed / retrying

previous PresentationArtifact = current
```

This is valid architecture.

---

# 87. Event Publication

Reading Session may publish committed session facts defined in:

```text
02-modules/reading-session/EVENTS.md
```

Possible examples include:

```text
ReadingSessionCreated

ReadingSessionPaused

ReadingSessionResumed

ReadingSessionStopped

ReadingContextChanged
```

Use exact module-defined names.

---

# 88. Events Do Not Drive Session Commands

Invalid:

```text
PauseRequested Event
    ↓
Reading Session
```

Preferred:

```text
PauseReading command
    ↓
Reading Session
    ↓
ReadingSessionPaused event
```

---

# 89. ReadingContextChanged

Canonical order:

```text
validate context mutation
    ↓
commit ReadingContextRevision
    ↓
ReadingContextChanged
```

The Event reports the committed fact.

---

# 90. Runtime Response to Context Change

Runtime work should not begin merely because it subscribed to:

```text
ReadingContextChanged
```

as a hidden command.

Application/business orchestration explicitly evaluates execution requirements.

---

# 91. Diagnostics Correlation

Useful correlation chain:

```text
SessionId
    ↓
ReadingContextRevision
    ↓
RuntimeRevisionId
    ↓
WorkItemId
    ↓
AttemptId
    ↓
ArtifactId
```

Diagnostics may observe the chain.

It does not own it.

---

# 92. Session Metrics

Potential measurements:

```text
session duration

active duration

paused duration

context-change count

source-loss count

recovery count

Runtime revisions per session

user retranslation count
```

Metrics should avoid sensitive/high-cardinality labels where inappropriate.

---

# 93. Session Privacy

ReadingContext may contain sensitive source metadata.

Do not log blindly:

```text
full URL

window title

raw content

selected text

source document text
```

Use privacy-safe metadata.

---

# 94. Session Persistence Privacy

Persistent Reading Session history should be opt-in or policy-controlled where it could reveal:

```text
what user was reading

source URL

chapter identity

captured application
```

---

# 95. Normal Screen Session Flow

```text
User starts reading
    ↓
Create Session S1
    ↓
Select screen region
    ↓
ReadingContextRevision C1
    ↓
Session ACTIVE
    ↓
Application creates execution plan
    ↓
RuntimeRevision R1
    ↓
processing Artifacts
    ↓
PresentationArtifact P1
    ↓
UI projection
    ↓
user scrolls
    ↓
same C1
    ↓
RuntimeRevision R2
    ↓
PresentationArtifact P2
```

This example demonstrates:

```text
content change
    ≠
ReadingContext change
```

---

# 96. Region Change Example

```text
Session S1
ReadingContextRevision C1
    ↓
user selects new region
    ↓
ReadingContextRevision C2
    ↓
Application replans
    ↓
RuntimeRevision R7
    ↓
old Runtime revision superseded
```

---

# 97. Pause Example

```text
Session S1 ACTIVE
    ↓
PauseReading
    ↓
Session S1 PAUSED
    ↓
Runtime policy handles active work
    ↓
ReadingContext C5 preserved
```

---

# 98. Resume Example

```text
Session S1 PAUSED
    ↓
ResumeReading
    ↓
validate current source/context
    ↓
Session S1 ACTIVE
    ↓
new Runtime work planned when required
```

---

# 99. Stop Example

```text
Session S1 ACTIVE
    ↓
StopReading
    ↓
STOPPING
    ↓
Runtime cancellation/supersession
    ↓
source observation stops
    ↓
session cleanup
    ↓
STOPPED
```

---

# 100. Preference Change Example

```text
Session S1 ACTIVE
    ↓
user changes persistent Translation preference
    ↓
Preferences commits P12
    ↓
Application evaluates current-session impact
```

Possible:

```text
future-session only
```

or:

```text
effective current configuration changes
    ↓
new RuntimeRevision
```

No automatic ownership transfer occurs.

---

# 101. Retranslation Example

```text
current TranslationArtifact T4
    ↓
user requests retranslation
    ↓
RetryCurrentOperationIntent
    ↓
Application
    ↓
new Translation execution requirement
    ↓
Runtime WorkItem / Attempt
    ↓
Candidate TranslationArtifact
    ↓
publication validation
```

Reading Session remains ACTIVE.

---

# 102. Source Loss Example

```text
Session ACTIVE
    ↓
capture source disappears
    ↓
Capture reports source/capability failure
    ↓
Application evaluates policy
```

Possible response:

```text
pause session

request new source

degrade session projection

stop session
```

Reading Session must not infer Capture implementation failures itself.

---

# 103. Architecture Invariants

1. SessionId identifies reading activity.

2. Reading Session owns SessionId.

3. Reading Session owns ReadingContext.

4. Reading Session owns ReadingContextRevision.

5. Runtime owns RuntimeRevisionId.

6. ReadingContextRevision is not RuntimeRevisionId.

7. Content change does not automatically mean ReadingContext change.

8. One ReadingContextRevision may produce many RuntimeRevisions.

9. Reading Session does not own processing-stage state.

10. Reading Session does not own WorkItem.

11. Reading Session does not own Attempt.

12. Reading Session does not own Runtime retry.

13. Reading Session does not own Runtime cancellation mechanics.

14. Persistent preferences belong to Preferences.

15. Session-only overrides belong to Reading Session.

16. Effective execution configuration is contextual.

17. Semantic Artifacts remain module-owned.

18. PresentationArtifact remains Presentation-owned.

19. ViewModel remains UI Adapter-owned.

20. Session pause does not automatically change ReadingContextRevision.

21. Session resume does not revive old Attempts.

22. Session stop does not stop entire processing modules globally.

23. Runtime failure does not automatically terminate Reading Session.

24. UI frontend failure does not automatically terminate Reading Session.

25. Module errors retain original ownership.

26. Reading Session events describe committed facts only.

27. Event Bus does not route Reading Session commands.

28. Crash recovery does not restore old live Attempts.

29. Current-state projection may combine Session and Runtime data without transferring authority.

30. Session persistence obeys privacy/retention rules.

---

# 104. Deprecated Legacy Flow

Deprecated architecture:

```text
Create Session
    ↓
Start Pipeline
    ↓
Session = OCR_PROCESSING
    ↓
Session = TRANSLATING
    ↓
Session = RENDERING
    ↓
Session = DISPLAYING
```

Runtime v2 replaces it with:

```text
Reading Session ACTIVE
        │
        ├── ReadingContextRevision
        │
        └── Application planning
                ↓
             RuntimeRevision
                ↓
             WorkItems
                ↓
              Attempts
                ↓
             Artifacts
```

---

# 105. Related Documents

```text
doc/01-architecture/core/
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── EVENT_CONVENTION.md
└── EVENT_BUS.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── OWNERSHIP_MAP.md
└── MODULE_DEPENDENCY.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── CANCELLATION.md
├── RETRY_POLICY.md
└── WORK_QUEUE.md

doc/01-architecture/flows/
├── SCREEN_COMIC_FLOW.md
├── READING_SESSION_FLOW.md
├── CONTENT_CHANGE_FLOW.md
└── STRUCTURED_TEXT_FLOW.md

doc/02-modules/
├── reading-session/
├── preferences/
├── capture/
├── recognition/
├── text-processing/
├── translation/
├── presentation/
└── ui-adapter/
```

---

# 106. Open Decisions

The following remain open:

```text
whether restored sessions preserve SessionId

whether one MVP application may have multiple concurrent sessions

exact source-loss behavior

which preference changes affect the active session automatically

which reading-position changes create ReadingContextRevision

whether closing the final reading UI stops the session

how much session history is persisted

whether session recovery defaults to PAUSED or READY
```

These decisions belong primarily to Reading Session/Application design.

---

# 107. Completion Criteria

This flow is synchronized when:

* SessionId is separated from Runtime execution identity;
* ReadingContextRevision is separated from RuntimeRevisionId;
* content changes do not automatically create ReadingContextRevision;
* persistent Preferences remain Preferences-owned;
* temporary session overrides remain Reading Session-owned;
* Reading Session has no OCR/Translation/Presentation processing states;
* WorkItem/Attempt remain Runtime-owned;
* retry/cancellation mechanics remain Runtime-owned;
* session pause/resume/stop behavior is separated from Attempt lifecycle;
* semantic Artifacts retain their producing owners;
* Application coordinates session → Runtime effects;
* Event Bus is fact-only;
* stale-result protection uses typed authority rather than one generic revision;
* crash recovery does not revive old Runtime Attempts.

---

# 108. Summary

The Reading Session flow is:

```text
User Intent
    ↓
Application
    ↓
Reading Session
    ↓
SessionId
    ↓
ReadingContext
    ↓
ReadingContextRevision
```

Execution is separate:

```text
ReadingContext
    ↓
Application / Business Execution Planning
    ↓
RuntimeRevision
    ↓
WorkItems
    ↓
Attempts
    ↓
Published Artifacts
```

During normal continuous reading:

```text
Reading Session
    may remain ACTIVE
```

while Runtime repeatedly processes new content.

The central invariant is:

```text
Reading Session answers:

"What is the user reading?"

Runtime answers:

"What work is executing now?"

Those are different authorities.
```
