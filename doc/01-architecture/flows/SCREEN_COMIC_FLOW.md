# CRAI Screen Comic Reading Flow

> **Project:** CRAI
> **Path:** `doc/01-architecture/flows/SCREEN_COMIC_FLOW.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the end-to-end CRAI flow for reading and translating comics directly from a selected screen region.

The flow describes:

```text
user interaction
reading-session behavior
screen observation
capture
semantic processing
Runtime execution
Artifact publication
presentation
UI projection
supersession
cancellation
recovery
continuous reading
```

This document describes architecture behavior.

It does not define provider-native or implementation-specific algorithms.

---

# 2. Primary Scenario

The primary scenario is:

```text
Desktop CRAI
    ↓
User selects comic region
    ↓
CRAI observes that region
    ↓
User scrolls / changes page
    ↓
CRAI detects stable readable content
    ↓
CRAI processes current content
    ↓
Translation is presented
    ↓
CRAI continues observing
```

The experience should require minimal manual interaction while reading.

---

# 3. Scope

This flow covers:

```text
desktop application
screen-region reading source
comic/image content
continuous observation
stable-content detection
capture
recognition
text processing
translation
presentation
side-panel / supported presentation projection
automatic continuation
```

---

# 4. Out of Scope

This document does not define:

```text
browser-extension implementation
OCR model internals
translation-provider API
native screen-capture API
database schema
UI framework
provider authentication
exact scheduler algorithm
exact cache implementation
```

Those belong to their respective architecture owners.

---

# 5. Main Architecture Participants

The flow involves:

```text
User

UI Adapter

Application

Reading Session

Preferences

Capture

Recognition

Text Processing

Translation

Presentation

Runtime

Diagnostics
```

Supporting mechanisms may include:

```text
Event Bus
Scheduler
Resource Manager
Logging
Telemetry
Storage
Provider Adapters
Platform Adapters
```

---

# 6. Authority Boundaries

The flow must preserve:

```text
Reading Session
    owns reading context

Preferences
    owns persistent preferences

Application
    coordinates use case

Runtime
    owns execution authority

Semantic modules
    own semantic Artifacts

UI Adapter
    owns UI projection
```

No participant may silently take another owner's authority.

---

# 7. Important Identities

This flow distinguishes:

```text
SessionId

ReadingContextRevision

RuntimeRevisionId

WorkItemId

AttemptId

ArtifactId
```

These identities have different meanings.

They must not be collapsed into one generic:

```text
RevisionId
PipelineId
TaskId
```

---

# 8. SessionId

`SessionId` identifies one reading activity.

Example:

```text
SessionId = S42
```

The session may survive many:

```text
screen changes
captures
Runtime revisions
processing attempts
presentation updates
```

---

# 9. ReadingContextRevision

`ReadingContextRevision` identifies the authoritative revision of the reading context.

It may change when:

```text
selected source changes
selected screen region changes
source mode changes
session-level language/configuration changes
```

It is owned by Reading Session.

---

# 10. RuntimeRevisionId

`RuntimeRevisionId` identifies the currently authoritative execution plan/revision.

It is owned by Runtime.

A Runtime revision may be superseded without changing the identity of the Reading Session.

---

# 11. WorkItemId

`WorkItemId` identifies logical executable work.

Examples:

```text
Capture stable content

Recognize content

Build SourceDocument

Translate document

Build presentation
```

---

# 12. AttemptId

`AttemptId` identifies one concrete execution attempt of a WorkItem.

Retry creates:

```text
new AttemptId
```

not a mutation of the previous Attempt.

---

# 13. ArtifactId

`ArtifactId` identifies semantic output.

Examples:

```text
CaptureArtifact

RecognitionArtifact

SourceDocumentArtifact

TranslationArtifact

PresentationArtifact
```

Artifact identity is not execution identity.

---

# 14. Preconditions

Before continuous comic reading starts:

```text
CRAI application is running

required architecture components are ready

a Reading Session can be created

a usable capture capability exists

a usable Recognition capability exists

a usable Translation capability exists

effective configuration can be resolved
```

The user must select or approve a readable source region before screen processing begins.

---

# 15. High-Level User Flow

```text
Launch CRAI
    ↓
Start Screen Comic Reading
    ↓
Select Screen Region
    ↓
Create / Update Reading Session
    ↓
Resolve Effective Configuration
    ↓
Start Observation
    ↓
Detect Stable Content
    ↓
Process Current Content
    ↓
Publish Presentation
    ↓
Project Reader UI
    ↓
Continue Observation
```

---

# 16. High-Level Processing Flow

Semantic processing is:

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

This is a semantic Artifact flow.

It is not a direct imperative call chain between modules.

---

# 17. Execution Flow

Execution is:

```text
Application / Business Pipeline Orchestration
    ↓
BusinessExecutionPlan
    ↓
RuntimeRevision
    ↓
WorkItem dependency graph
    ↓
Attempts
    ↓
Candidate Artifacts
    ↓
Publication validation
    ↓
Published Artifacts
```

---

# 18. Start Reading

User initiates:

```text
Start Screen Comic Reading
```

UI Adapter converts the interaction into an Application intent/use case.

UI Adapter does not directly start:

```text
Capture worker
Recognition provider
Translation provider
Scheduler
```

---

# 19. Source Selection

The user selects a screen region.

Platform-specific selection is performed through a Platform Adapter.

The result is converted into platform-neutral source information.

Reading Session validates and accepts the new reading context.

---

# 20. Reading Context Creation

Reading Session creates or updates:

```text
ReadingContext
```

with a new:

```text
ReadingContextRevision
```

Possible context information includes:

```text
source type
capture region
source-language selection
target-language selection
session-only overrides
reading mode
```

Exact schema belongs to Reading Session.

---

# 21. Preferences Resolution

Persistent preferences are read from:

```text
Preferences
```

Session-specific overrides belong to:

```text
Reading Session
```

Application resolves the effective configuration required for execution.

Reading Session does not own persistent Preferences.

---

# 22. Runtime Activation

Application creates or updates the execution requirements corresponding to the accepted Reading Context.

Business Pipeline Orchestration derives a:

```text
BusinessExecutionPlan
```

Runtime establishes the corresponding:

```text
RuntimeRevisionId
```

---

# 23. Observation

The selected screen region is observed continuously.

Conceptually:

```text
screen region
    ↓
Capture observation
    ↓
change detection
    ↓
stability evaluation
```

Observation should avoid expensive downstream processing while the user is actively scrolling.

---

# 24. Scrolling Behavior

While meaningful motion is occurring:

```text
Capture may observe frames

Recognition should not begin for unstable content

Translation should not begin for unstable content

Presentation should remain on the last valid Published Artifact
```

The user should be free to scroll naturally.

---

# 25. Stability Detection

Conceptually:

```text
Frame A
    ↓
Frame B
    ↓
Movement detected
    ↓
continue observing
    ↓
Frame C
    ↓
Frame D
    ↓
sufficient stability
    ↓
stable-content candidate
```

Exact stability algorithm is not defined here.

---

# 26. Stability Is Not a Business Event Command

A stability observation may participate in internal Capture processing.

It must not be interpreted as:

```text
frame.stable
    ↓
Event Bus commands Recognition
```

Execution readiness belongs to Runtime.

---

# 27. Capture WorkItem

When stable readable content is eligible for processing, Runtime may execute a Capture WorkItem.

Conceptually:

```text
WorkItem
    CaptureCurrentContent

Attempt
    CAPTURE-A17
```

Capture receives the relevant immutable execution/configuration snapshot.

---

# 28. Capture Candidate

Capture produces a provisional:

```text
CaptureArtifact Candidate
```

Possible semantic information includes:

```text
captured content
source geometry
capture timestamp
source provenance
content fingerprint/checksum
```

Exact schema belongs to Capture.

---

# 29. Capture Publication

Before the Candidate becomes authoritative output:

```text
semantic validation
+
Runtime authority validation
```

must succeed.

If the execution has already been superseded:

```text
Candidate is not published
```

---

# 30. Published CaptureArtifact

After successful publication:

```text
CaptureArtifact
```

becomes immutable semantic input for downstream work.

Runtime may then determine dependent WorkItems are ready.

---

# 31. Recognition WorkItem

Recognition consumes:

```text
Published CaptureArtifact
```

Runtime creates/executes the corresponding Recognition Attempt.

Recognition may internally perform:

```text
preprocessing
text detection
text recognition
layout analysis
text direction analysis
reading-order reconstruction
OCR postprocessing
```

according to detailed OCR architecture.

---

# 32. OCR Internals

Detailed OCR architecture remains under:

```text
01-architecture/ocr/
```

This screen flow does not independently redefine:

```text
Detection Result
Layout Tree
Reading Order
OCR Quality
provider normalization
```

---

# 33. Recognition Candidate

Recognition produces:

```text
RecognitionArtifact Candidate
```

containing the public Recognition semantic output.

It must not expose provider-native DTOs.

---

# 34. Recognition Publication

Publication requires:

```text
Recognition semantic validity
+
current Runtime authority
```

A late Recognition result from a superseded Runtime revision must not become Published.

---

# 35. Text Processing WorkItem

Text Processing consumes:

```text
Published RecognitionArtifact
```

Its purpose is to construct normalized semantic source content.

Possible responsibilities include:

```text
text normalization
cross-region reconstruction
semantic grouping
source-document construction
```

---

# 36. SourceDocumentArtifact

Text Processing produces:

```text
SourceDocumentArtifact Candidate
```

and publishes it only if still authoritative.

Published output:

```text
SourceDocumentArtifact
```

becomes Translation input.

---

# 37. TranslationUnit Construction

TranslationUnit construction belongs to:

```text
Translation
```

not Text Processing.

Translation consumes:

```text
SourceDocumentArtifact
```

and may derive:

```text
TranslationUnits
TranslationBatch
context
neighbor relationships
glossary references
```

---

# 38. Translation WorkItem

Runtime executes Translation work according to current Runtime authority.

Conceptually:

```text
Translation WorkItem
    ↓
Attempt T1
    ↓
Translation Provider Port
```

Provider implementation may be:

```text
local
cloud
hybrid
```

without changing the semantic flow.

---

# 39. Translation Candidate

Translation produces:

```text
TranslationArtifact Candidate
```

Possible semantic content includes:

```text
translated text
source-target alignment
translation provenance
warnings
completeness
```

Provider-native metadata must be normalized before crossing the module boundary.

---

# 40. Translation Publication

A Translation result may arrive after the user has already scrolled.

Therefore publication must validate current Runtime authority.

Conceptually:

```text
Translation Candidate
    ↓
Is producing RuntimeRevision still authoritative?
    ├── yes → publish
    └── no  → discard / retain only as non-current cache candidate if policy allows
```

It must never overwrite newer current content.

---

# 41. Presentation WorkItem

Presentation consumes:

```text
Published TranslationArtifact
```

and builds platform-neutral semantic presentation.

---

# 42. PresentationArtifact

Presentation produces:

```text
PresentationArtifact Candidate
```

Possible content includes:

```text
ordered presentation units
source/translated text relationship
semantic geometry
text-fitting result
interaction references
```

It does not contain native UI components.

---

# 43. Presentation Publication

Publication again requires:

```text
semantic validity
+
current Runtime authority
```

Only a valid current PresentationArtifact becomes available for UI projection.

---

# 44. UI Projection

Application/UI Adapter receives the current presentation state.

UI Adapter builds:

```text
ViewModel
```

from the current public state.

The frontend renders that ViewModel.

---

# 45. Atomic User-Visible Update

The user should not observe:

```text
half old + half new translation

mixed Runtime revisions

partially published semantic layout

Recognition result paired with unrelated Translation result
```

User-visible current presentation should switch atomically at the architecture boundary.

---

# 46. Previous Presentation During Processing

While new content is processing, CRAI may continue displaying the previous valid PresentationArtifact.

Conceptually:

```text
Current Published Presentation A
        ↓
new content detected
        ↓
Presentation A remains visible
        ↓
new Presentation B becomes Published
        ↓
atomic switch A → B
```

This avoids flicker and partially translated UI.

---

# 47. Continuous Monitoring

After presentation publication:

```text
screen observation continues
```

There is no requirement to stop capture merely because Translation or Presentation completed.

Observation and processing may overlap subject to Runtime policy.

---

# 48. New Screen Content

When the user scrolls or changes the page:

```text
screen content changes
    ↓
new stable content discovered
    ↓
new execution requirements
```

Runtime may supersede obsolete work.

---

# 49. Supersession

Example:

```text
RuntimeRevision R18
    Recognition running
        ↓
user scrolls
        ↓
new stable content accepted
        ↓
RuntimeRevision R19 becomes authoritative
        ↓
R18 superseded
```

Supersession is Runtime-owned.

---

# 50. Cancellation After Supersession

After R18 is superseded:

```text
queued obsolete WorkItems
    → should not start

running obsolete Attempts
    → cancellation requested where appropriate

late results
    → cannot publish as current
```

Cancellation is an optimization and resource-control mechanism.

Publication authority is the final correctness boundary.

---

# 51. Cancellation Is Not the Only Protection

Correctness must not rely solely on successfully cancelling provider work.

A provider may:

```text
ignore cancellation
finish too late
return after timeout
return during supersession
```

Therefore:

```text
late Candidate
    ↓
authority validation
    ↓
publication rejected
```

must still protect current state.

---

# 52. Stale Result Protection

The old generic check:

```text
Current Revision == Result Revision
```

is insufficient because CRAI now distinguishes multiple revision authorities.

Current publication validation should consider the relevant:

```text
SessionId
ReadingContextRevision
RuntimeRevisionId
Artifact provenance
```

according to the owner contract.

---

# 53. ReadingContextRevision Change

If the user changes:

```text
capture region
source
session language
session-only configuration affecting semantics
```

Reading Session may create:

```text
ReadingContextRevision N+1
```

Application then updates Runtime execution authority accordingly.

---

# 54. RuntimeRevision Change Without Session End

Not every RuntimeRevision change ends the Reading Session.

Example:

```text
same Session
same reading source
same reading activity

but

new stable screen content
    ↓
new Runtime execution revision
```

The session remains active.

---

# 55. Session End

Reading Session may end when:

```text
user explicitly stops reading

application closes

source becomes permanently unavailable

session termination is requested
```

Changing screen content alone does not necessarily end the session.

---

# 56. Region Change

Changing selected region normally means:

```text
same or new Session
    ↓
new ReadingContextRevision
    ↓
new RuntimeRevision
```

Exact UX decision about retaining the same SessionId belongs to Reading Session/Application policy.

---

# 57. Retry

Failures do not cause modules to run their own global retry loops.

Example:

```text
Translation Attempt T1
    ↓
timeout
    ↓
TRN-* error
    ↓
Runtime Retry Policy
    ↓
Attempt T2
```

Each retry is a new Attempt.

---

# 58. Provider Fallback

Provider fallback may involve:

```text
semantic module
    → classifies failure/suitability

Provider Management
    → provides eligible alternatives

Runtime
    → creates next Attempt
```

No standalone flow-level Fallback Coordinator is required.

---

# 59. Capture Recovery

If screen capture temporarily fails:

```text
Capture Attempt
    ↓
CAP-* error
    ↓
Runtime policy
```

Possible outcomes include:

```text
retry
wait
degrade
request user action
fail current WorkItem
```

The flow document does not hardcode `Restart Capture` for every failure.

---

# 60. Recognition Recovery

Recognition failure may lead to:

```text
retry
alternate provider
degraded RecognitionArtifact
terminal failure
```

depending on:

```text
error semantics
quality requirements
Runtime policy
provider availability
```

---

# 61. Translation Recovery

Translation failure may lead to:

```text
retry
alternate provider
partial/degraded result if contract permits
terminal failure
```

according to Translation + Runtime contracts.

---

# 62. Presentation Recovery

Presentation failure must not corrupt the last valid current presentation.

The previous Published PresentationArtifact may remain visible while recovery is attempted.

---

# 63. Error Projection

Module errors remain module-owned:

```text
CAP-*
REC-*
TXT-*
TRN-*
PRES-*
```

Runtime execution errors remain:

```text
RUN-*
```

UI Adapter may project them into user-facing messages without changing ownership.

---

# 64. Diagnostics

Diagnostics may observe:

```text
module health
Runtime health
provider availability
recent failures
latency
degradation
```

It does not control the processing flow.

---

# 65. Event Bus

Event Bus may distribute committed facts such as:

```text
ReadingContextChanged
RecognitionArtifactPublished
TranslationArtifactPublished
```

when those events are actually defined by their owner modules.

This document does not create new canonical event names.

---

# 66. Event Timeline Is Observational

A possible observational timeline may look conceptually like:

```text
Reading Session fact
    ↓
Capture publication fact
    ↓
Recognition publication fact
    ↓
SourceDocument publication fact
    ↓
Translation publication fact
    ↓
Presentation publication fact
```

But these events do not command downstream stages.

---

# 67. No Stage-Command Event Chain

Forbidden:

```text
capture.completed
    ↓
recognition.requested
    ↓
recognition.completed
    ↓
translation.requested
```

Runtime dependency readiness replaces this model.

---

# 68. Cache Lookup

Cache may be consulted before expensive work.

Possible semantic cache domains include:

```text
Capture-derived data
Recognition
Text Processing
Translation
Presentation
```

Exact cache ownership/policy is defined elsewhere.

---

# 69. Cache Is Not Authority

A cache hit does not automatically become current output.

Cached data must still satisfy:

```text
semantic compatibility
configuration compatibility
provenance requirements
current Runtime authority
```

before publication/use as current output.

---

# 70. Cache Key

Cache validity must not depend solely on:

```text
Revision checksum
```

because semantic compatibility may also depend on:

```text
source content
provider/model profile
language pair
glossary/context
processing configuration
schema version
```

Exact cache key semantics belong to cache/module policy.

---

# 71. Duplicate Content

If the user returns to previously seen content:

```text
content fingerprint match
    ↓
compatible cached Artifacts may be reusable
```

subject to current policy.

This can significantly reduce repeated Recognition/Translation cost.

---

# 72. Resource Priority

User interaction has high scheduling importance.

Examples:

```text
scroll
change source
stop reading
close application
change region
```

should promptly supersede obsolete background work.

Exact scheduler priority belongs to Runtime.

---

# 73. Backpressure

Continuous capture must not create an unbounded processing queue.

Conceptually:

```text
screen changes faster than processing
    ↓
obsolete content is superseded/coalesced
    ↓
current useful work is prioritized
```

CRAI should prefer freshness over processing every observed frame.

---

# 74. No Frame-by-Frame Translation

CRAI must not translate every captured frame.

Capture observation frequency and semantic processing frequency are different concerns.

Only eligible stable/current content should progress into expensive processing.

---

# 75. Performance Goals

Performance remains an important product requirement.

However, fixed stage numbers such as:

```text
OCR < 300 ms
Translation < 700 ms
```

must not be treated as architecture guarantees before provider/device benchmarking.

---

# 76. MVP Performance Objective

The desired experience is:

```text
stable visible comic content
    ↓
translated presentation
```

with latency low enough not to interrupt natural reading.

A useful initial product target may remain approximately:

```text
~1 second class latency
```

for favorable configurations.

This is a target to validate experimentally, not a guaranteed SLA.

---

# 77. Performance Measurement

Measure at least:

```text
stability-detection latency

Capture latency

Recognition latency

Text Processing latency

Translation latency

Presentation latency

UI projection latency

end-to-end latency

superseded-work ratio

cache-hit ratio

provider latency

time-to-cancel

late-result rejection count
```

---

# 78. Progressive Presentation

The MVP architecture prefers:

```text
atomic Published PresentationArtifact
```

for correctness.

Future versions may support:

```text
progressive translation
streaming presentation
partial Artifact publication
```

only if explicit partial-publication semantics are designed.

Do not infer progressive publication from provider streaming support.

---

# 79. Side-by-Side Presentation

For the MVP, Presentation may produce a model suitable for:

```text
original content
+
translated text
```

in a side-by-side reading experience.

UI Adapter decides how that semantic model maps to the selected frontend.

---

# 80. Future Overlay Mode

Future overlay rendering may reuse:

```text
PresentationArtifact
```

but may require additional geometry/text-fitting capabilities.

Overlay implementation belongs to Presentation + UI Adapter/Platform boundaries according to ownership.

---

# 81. Screen Comic Flow State View

Do not model the whole system as one monolithic state machine:

```text
OBSERVING
→ OCR_RUNNING
→ TRANSLATING
→ PRESENTING
```

because multiple authority owners now exist.

Instead, state is distributed by owner.

---

# 82. Reading Session State

Reading Session owns its own lifecycle, for example conceptually:

```text
inactive
    ↓
active
    ↓
stopping
    ↓
ended
```

Exact states belong to:

```text
02-modules/reading-session/STATES.md
```

---

# 83. Runtime State

Runtime separately owns:

```text
RuntimeRevision
WorkItem
Attempt
```

state machines.

Exact states belong to Runtime architecture.

---

# 84. Module State

Capture, Recognition, Text Processing, Translation and Presentation each own their semantic module states where applicable.

This flow references them but does not redefine them.

---

# 85. UI State

UI Adapter owns UI-local state such as:

```text
selection interaction
dialog state
loading projection
error notification
current ViewModel
```

UI state is not Runtime state.

---

# 86. Example — Normal Reading Cycle

```text
User starts screen comic mode
    ↓
Application creates Reading Session
    ↓
User selects screen region
    ↓
ReadingContextRevision C1
    ↓
Application resolves effective configuration
    ↓
RuntimeRevision R1
    ↓
Capture observes selected region
    ↓
stable content detected
    ↓
CaptureArtifact A1 published
    ↓
Recognition WorkItem READY
    ↓
RecognitionArtifact A2 published
    ↓
Text Processing WorkItem READY
    ↓
SourceDocumentArtifact A3 published
    ↓
Translation WorkItem READY
    ↓
TranslationArtifact A4 published
    ↓
Presentation WorkItem READY
    ↓
PresentationArtifact A5 published
    ↓
UI Adapter builds ViewModel
    ↓
Reader UI updates
    ↓
observation continues
```

---

# 87. Example — User Scrolls During Recognition

```text
RuntimeRevision R18
    ↓
Recognition Attempt running
    ↓
user scrolls
    ↓
screen becomes unstable
    ↓
old processing may continue briefly
    ↓
new stable content accepted
    ↓
RuntimeRevision R19 authoritative
    ↓
R18 superseded
    ↓
cancellation requested for obsolete Attempt
    ↓
late R18 Recognition Candidate arrives
    ↓
publication authority check fails
    ↓
Candidate cannot replace current state
```

---

# 88. Example — User Scrolls During Translation

```text
Translation Attempt T18
    ↓
provider request in flight
    ↓
user scrolls
    ↓
new stable content
    ↓
RuntimeRevision R19
    ↓
T18 becomes obsolete
    ↓
provider may ignore cancellation
    ↓
T18 returns
    ↓
Translation Candidate is stale
    ↓
not published as current
```

---

# 89. Example — Cache Hit

```text
stable content detected
    ↓
compatible content fingerprint
    ↓
cached RecognitionArtifact available
    ↓
compatibility validated
    ↓
Runtime may avoid Recognition Attempt
    ↓
downstream dependency satisfied
```

The same principle may apply to Translation or Presentation caches.

---

# 90. Example — Translation Retry

```text
Translation WorkItem W42
    ↓
Attempt T1
    ↓
timeout
    ↓
Runtime Retry Policy
    ↓
Attempt T2
    ↓
success
    ↓
Translation Candidate
    ↓
authority validation
    ↓
Published TranslationArtifact
```

---

# 91. Example — Translation Provider Fallback

```text
Attempt T1
Provider A
    ↓
provider unavailable
    ↓
semantic error classification
    ↓
Runtime retry/fallback decision
    ↓
Attempt T2
Provider B
    ↓
success
```

Translation remains semantic owner of the result.

Runtime remains owner of Attempt execution.

---

# 92. Example — Stop Reading

```text
User
    ↓
Stop Reading
    ↓
UI Adapter
    ↓
Application
    ↓
Reading Session stop transition
    +
Runtime cancellation/supersession
    ↓
running Attempts receive cancellation
    ↓
late Candidates cannot publish as current
    ↓
UI leaves active reading state
```

---

# 93. Example — Change Capture Region

```text
User selects new region
    ↓
UI Adapter
    ↓
Application
    ↓
Reading Session
    ↓
ReadingContextRevision C2
    ↓
RuntimeRevision R2
    ↓
old work superseded
    ↓
observation continues on new region
```

---

# 94. Failure Principle

A failed WorkItem must not corrupt previously Published Artifacts.

Conceptually:

```text
Published Presentation A
    ↓
new Translation fails
    ↓
Presentation A remains valid
```

until architecture policy decides otherwise.

---

# 95. Partial Failure

A processing failure does not automatically mean:

```text
Reading Session failed
```

Possible states include:

```text
session active
+
current content processing degraded/failed
+
previous presentation still visible
```

---

# 96. Provider Availability

Provider availability may change while the Reading Session remains active.

Runtime/Provider Management may adapt without recreating the whole session.

---

# 97. Offline Behavior

If compatible local capabilities/cache are available, CRAI may continue in degraded/offline mode.

Exact offline policy belongs to provider/capability architecture.

---

# 98. Privacy

Screen capture may contain sensitive information.

The flow must minimize unnecessary propagation.

For example:

```text
Capture
    owns captured source data

Recognition provider
    receives only required image data

Translation provider
    receives only required semantic text/context
```

Translation provider should not receive the entire screenshot unless explicitly required by a multimodal Translation capability.

---

# 99. Data Minimization

Each downstream module receives only the data required for its semantic responsibility.

This is both:

```text
dependency discipline
+
privacy discipline
```

---

# 100. Logging Safety

Captured image content and translated text must not be logged indiscriminately.

Logging/Diagnostics should prefer:

```text
IDs
timings
status
error codes
provider IDs
safe metadata
```

unless explicit debug/privacy policy permits content logging.

---

# 101. Design Principles

The screen comic flow follows:

```text
continuous observation

stable-content gating

immutable semantic Artifacts

explicit semantic ownership

Runtime-owned execution

Candidate → Published publication

supersession-first freshness

cancellation-aware execution

late-result protection

cache compatibility validation

atomic user-visible presentation

provider isolation

platform isolation

privacy-aware data movement
```

---

# 102. Critical Invariants

1. Reading Session owns ReadingContext.

2. Reading Session owns ReadingContextRevision.

3. Runtime owns RuntimeRevisionId.

4. Runtime owns WorkItem.

5. Runtime owns Attempt.

6. Semantic modules own semantic Artifacts.

7. Stable-content observation does not directly command Recognition.

8. Processing stages do not directly call the next stage for orchestration.

9. Event Bus does not route processing commands.

10. TranslationUnit belongs to Translation.

11. RecognitionArtifact is the public Recognition boundary.

12. SourceDocumentArtifact is Text Processing output.

13. TranslationArtifact is Translation output.

14. PresentationArtifact is Presentation output.

15. UI Adapter owns ViewModel, not Presentation semantics.

16. Retry creates a new Attempt.

17. Runtime owns cancellation mechanics.

18. Superseded work cannot publish as current.

19. Successful cancellation is not required for stale-result correctness.

20. Cache hit does not bypass authority validation.

21. User scrolling should supersede obsolete expensive work.

22. Continuous observation must not create an unbounded processing queue.

23. Previously Published presentation remains safe during new processing.

24. Provider-native DTOs never become public Artifacts.

25. Screen content is propagated according to data-minimization rules.

---

# 103. Related Documents

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
├── WORK_QUEUE.md
├── SCHEDULER.md
├── RETRY_POLICY.md
└── CANCELLATION.md

doc/01-architecture/ocr/

doc/02-modules/
├── capture/
├── recognition/
├── text-processing/
├── translation/
├── presentation/
├── reading-session/
├── preferences/
├── diagnostics/
└── ui-adapter/
```

---

# 104. Open Decisions

The following remain open and must not be silently frozen by this flow:

```text
exact stability-detection algorithm

capture sampling frequency

content-change threshold

screen-region tracking behavior

same SessionId vs new SessionId after major source change

exact cache-key composition

progressive/streaming presentation

overlay-mode behavior

multimodal Translation path

offline behavior

exact MVP latency budget

provider selection strategy

process topology
```

---

# 105. Completion Criteria

This flow is architecture-aligned when:

* Reading Session does not own Runtime processing state;
* Preferences does not become session-owned persistent state;
* RuntimeRevision is separate from ReadingContextRevision;
* WorkItem/Attempt identities are explicit;
* processing uses semantic Artifact boundaries;
* OCR internals remain under Recognition/OCR architecture;
* Translation owns TranslationUnit construction;
* stage chaining is not performed through Event Bus;
* retry and cancellation are Runtime-owned;
* supersession protects freshness;
* Candidate → Published protects current authority;
* cache is not treated as authority;
* Presentation and UI projection are separated;
* continuous capture is protected by backpressure/coalescing;
* old monolithic `OCR_RUNNING → TRANSLATING → PRESENTING` state machine is removed.

---

# 106. Summary

The screen-comic experience appears simple to the user:

```text
Select region
    ↓
Read
    ↓
Scroll
    ↓
Translation keeps following
```

Internally, CRAI preserves strict authority boundaries:

```text
Reading Session
    ↓
ReadingContext

Application
    ↓
BusinessExecutionPlan

Runtime
    ↓
RuntimeRevision
    ↓
WorkItems
    ↓
Attempts

Semantic Modules
    ↓
Published Artifacts

UI Adapter
    ↓
ViewModel
```

The semantic processing path is:

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

The central correctness rule is:

```text
New screen content
may supersede old execution at any time.

Old execution may finish,
but it must never regain current authority.
```
