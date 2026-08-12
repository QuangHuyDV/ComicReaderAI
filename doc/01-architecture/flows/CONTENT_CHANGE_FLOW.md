# CRAI Content Change Flow

> **Project:** CRAI
> **Path:** `doc/01-architecture/flows/CONTENT_CHANGE_FLOW.md`
> **Version:** 1.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines how CRAI reacts when the currently observed reading content changes.

It focuses on continuously changing sources such as:

```text
screen regions
application windows
browser viewports
comic readers
image viewers
dynamic documents
```

The flow covers:

```text
observation
change detection
stability
duplicate suppression
content acceptance
RuntimeRevision creation
supersession
cancellation
late-result handling
backpressure
freshness
Artifact publication
UI continuity
```

This document does not redefine:

```text
Reading Session lifecycle
Capture algorithms
Runtime state machines
module Artifact schemas
provider APIs
UI rendering
```

Those remain with their owners.

---

# 2. Why This Flow Exists

Continuous reading creates a fundamental race:

```text
User changes visible content
        ↓
CRAI starts processing
        ↓
User changes content again
        ↓
old processing may still be running
```

Without explicit authority rules:

```text
old Recognition result
old Translation result
old Presentation result
```

could arrive later and overwrite newer user-visible content.

The Content Change Flow exists to prevent that.

---

# 3. Central Correctness Rule

The core invariant is:

```text
Newer authoritative content
must never be replaced
by a result produced
for older authority.
```

Cancellation helps resource efficiency.

Authority validation guarantees correctness.

---

# 4. Core Architecture Principle

CRAI separates:

```text
Observation
    ↓
Candidate content

Authority
    ↓
current executable content

Execution
    ↓
WorkItem / Attempt

Semantic result
    ↓
Candidate Artifact

Publication
    ↓
Published Artifact
```

These must not be collapsed into one generic "revision".

---

# 5. Main Participants

The flow may involve:

```text
User

Application

Reading Session

Capture / Source Observation

Business Pipeline Orchestration

Runtime

Recognition

Text Processing

Translation

Presentation

UI Adapter

Diagnostics
```

Supporting mechanisms may include:

```text
Scheduler
Resource Manager
Cache
Event Bus
Logging
Telemetry
Platform Adapters
```

---

# 6. Main Authorities

| Concern                         | Owner                           |
| ------------------------------- | ------------------------------- |
| Reading source/context          | Reading Session                 |
| ReadingContextRevision          | Reading Session                 |
| Source observation semantics    | Capture / source adapter        |
| Stable-content acceptance       | Capture/source policy           |
| Business execution requirements | Business Pipeline Orchestration |
| RuntimeRevisionId               | Runtime                         |
| WorkItem/Attempt                | Runtime                         |
| Retry                           | Runtime                         |
| Cancellation execution          | Runtime                         |
| Supersession                    | Runtime                         |
| Semantic Artifact               | Producing module                |
| User-visible projection         | UI Adapter                      |

---

# 7. Content Change vs Reading Context Change

These are different.

## Content Change

Example:

```text
same screen region
same application window
user scrolls comic
```

The ReadingContext may remain unchanged.

## Reading Context Change

Example:

```text
user selects another window
user changes capture region
user changes source language
```

This may create a new:

```text
ReadingContextRevision
```

---

# 8. Content Change Does Not Automatically Change ReadingContextRevision

Example:

```text
Session S1
ReadingContextRevision C7

user scrolls comic
    ↓
different visible content
```

The context may remain:

```text
C7
```

while Runtime establishes new execution authority.

---

# 9. Why This Distinction Matters

If every visible frame created a ReadingContextRevision:

```text
Reading Session
```

would become the owner of high-frequency processing identity.

That would incorrectly couple domain/session state to Runtime execution.

Runtime v2 keeps these responsibilities separate.

---

# 10. Observation Loop

Conceptually:

```text
Observe Source
    ↓
Capture Observation
    ↓
Compare With Previous Observation
    ↓
Changed?
    ├── No → Continue Observing
    └── Yes
         ↓
      Evaluate Stability
```

---

# 11. Raw Observation

A raw observation may contain:

```text
visual frame
region fingerprint
DOM snapshot
source locator state
geometry
timestamp
```

Raw observations are not automatically Published Artifacts.

---

# 12. Observation Is High Frequency

Observation may happen much more frequently than semantic processing.

Example:

```text
Capture observation:
10–60 observations/sec

Recognition:
far less frequent

Translation:
far less frequent
```

The architecture must therefore prevent observation frequency from determining downstream processing frequency.

---

# 13. Change Detection

The observation owner determines whether meaningful source change occurred.

Possible techniques include:

```text
pixel difference
perceptual hash
region comparison
DOM mutation
layout fingerprint
source locator change
```

Exact algorithms remain implementation-specific.

---

# 14. Meaningful Change

Not every visual difference should trigger expensive processing.

Examples that may be ignored:

```text
cursor movement
caret blink
loading spinner
video thumbnail
minor animation
hover effect
small advertisement change
```

The source/observation policy determines meaningfulness.

---

# 15. Stable Content

For rapidly changing visual sources:

```text
change detected
    ↓
content moving
    ↓
wait / continue observing
    ↓
content sufficiently stable
```

Only sufficiently stable current content should normally become eligible for expensive semantic processing.

---

# 16. Stability Is Policy

Stability may depend on:

```text
time window
similarity threshold
motion threshold
region-specific rules
source type
scroll velocity
```

No single universal stability algorithm is defined here.

---

# 17. Stability Candidate

When content appears stable:

```text
StableContentCandidate
```

may be created inside Capture/source-observation boundaries.

This candidate is provisional.

---

# 18. Stable Does Not Mean Authoritative Forever

Content may change immediately after stability is detected.

Therefore:

```text
stable candidate
```

does not guarantee that downstream execution will remain current.

Runtime supersession still protects the system.

---

# 19. Duplicate Detection

Before expensive work begins, CRAI may determine that the current content is semantically equivalent to previously processed content.

Possible fingerprints:

```text
exact image hash
perceptual image hash
region fingerprint
normalized text hash
source locator identity
```

---

# 20. Duplicate Is Not Authority

A duplicate match may permit cache reuse.

It does not automatically publish cached results.

Cached data still requires:

```text
compatibility validation
scope validation
current authority validation
```

---

# 21. Content Acceptance

Once current source content is considered eligible:

```text
stable
meaningful
non-obsolete
```

the Application/business flow may accept it for processing.

Conceptually:

```text
Stable Content Candidate
    ↓
Accept Current Content
    ↓
Business Execution Planning
```

---

# 22. Content Acceptance Is Not ReadingContext Mutation

For continuous screen reading:

```text
accept new visible content
```

normally does not require:

```text
ReadingContextRevision N+1
```

unless session semantics themselves changed.

---

# 23. RuntimeRevision Creation

Current accepted content may result in a new:

```text
RuntimeRevisionId
```

Example:

```text
ReadingContextRevision C7

Visible Content A
    ↓
RuntimeRevision R20

Visible Content B
    ↓
RuntimeRevision R21
```

Both may belong to the same ReadingContextRevision.

---

# 24. RuntimeRevision Represents Execution Authority

The newest applicable RuntimeRevision determines which execution results may become current.

Example:

```text
R20
    ↓
superseded by
    ↓
R21
```

Results from R20 may still finish.

They no longer have authority to replace R21 output.

---

# 25. Supersession

Supersession occurs when newer execution authority replaces older authority.

Conceptually:

```text
RuntimeRevision R20 = current
        ↓
new accepted content
        ↓
RuntimeRevision R21 created
        ↓
R20 = SUPERSEDED
        ↓
R21 = current
```

Exact Runtime states remain defined in Runtime architecture.

---

# 26. Supersession Is Not Failure

Supersession usually means:

```text
work became obsolete
```

not:

```text
work was incorrect
```

It should not automatically produce user-visible errors.

---

# 27. Supersession Consequences

When R20 is superseded:

```text
queued obsolete WorkItems
    → should not begin

running obsolete Attempts
    → cancellation may be requested

late Candidate Artifacts
    → cannot become current

cached intermediate results
    → may remain reusable if policy allows
```

---

# 28. Cancellation

Runtime owns cancellation mechanics.

Supersession may cause Runtime to cancel obsolete work.

Conceptually:

```text
R20 superseded
    ↓
Runtime identifies affected WorkItems
    ↓
cancel queued work
    ↓
request cancellation of running Attempts
```

---

# 29. Cancellation Is Best Effort

Provider or platform operations may not stop immediately.

Possible behavior:

```text
cancellation accepted immediately

cancellation cooperatively observed

network request cannot be interrupted

provider returns anyway

native OCR library completes current call
```

Therefore cancellation alone cannot guarantee correctness.

---

# 30. Publication Validation Is Mandatory

Even after cancellation:

```text
old Attempt may finish
```

Before any Candidate becomes Published:

```text
current authority must be validated
```

This is the final correctness boundary.

---

# 31. Canonical Late-Result Flow

```text
RuntimeRevision R20
    ↓
Translation Attempt T20 running
    ↓
RuntimeRevision R21 becomes current
    ↓
R20 superseded
    ↓
T20 cancellation requested
    ↓
provider ignores cancellation
    ↓
T20 finishes
    ↓
Candidate TranslationArtifact
    ↓
authority validation
    ↓
R20 is no longer current
    ↓
Candidate rejected as current publication
```

---

# 32. Candidate Rejection

A stale Candidate may be:

```text
discarded

retained temporarily for Diagnostics

stored as compatible cache candidate

kept as historical execution output
```

according to policy.

It must not overwrite current Published state.

---

# 33. Candidate Rejection Is Not Necessarily Error

Expected supersession should not produce noisy user errors.

Diagnostics may count:

```text
superseded candidates
cancelled work
late results
```

as normal Runtime behavior.

---

# 34. Publication Authority Inputs

Artifact publication may consider:

```text
SessionId
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
input Artifact provenance
module semantic validity
```

Not every Artifact requires every field directly.

Owner contracts determine exact requirements.

---

# 35. No Generic `Revision == Revision` Check

Deprecated:

```text
if result.revision == currentRevision:
    publish
```

because CRAI has multiple typed revision authorities.

Prefer explicit typed checks.

---

# 36. Example Authority Check

Conceptually:

```text
Candidate
    ↓
candidate.runtimeRevisionId == currentRuntimeRevisionId?
    ├── No → stale
    └── Yes
         ↓
      candidate input provenance still valid?
         ├── No → reject
         └── Yes
              ↓
           semantic validation
              ↓
           publish
```

Exact logic remains owner/Runtime-defined.

---

# 37. Semantic Processing After Content Acceptance

Typical image path:

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

Runtime WorkItems execute this semantic dependency graph.

---

# 38. WorkItem Dependency

Example:

```text
Recognition WorkItem
```

depends on:

```text
Published CaptureArtifact
```

When dependency becomes available under current authority, Runtime may mark the WorkItem executable.

---

# 39. Event Bus Is Not the Trigger

Do not implement:

```text
CaptureArtifactPublished Event
    ↓
Recognition starts
```

as the execution authority.

The Runtime dependency graph already defines readiness.

The event may notify asynchronous observers.

---

# 40. Multiple Runtime Revisions May Overlap

Example:

```text
R20
    Translation Attempt running

R21
    Recognition Attempt running

R22
    Capture Candidate being evaluated
```

This overlap is legal.

Only current-authority publication rules determine what becomes current.

---

# 41. Freshness Priority

For continuous reading, freshness often matters more than completing every historical content state.

Preferred policy:

```text
latest useful content
    > obsolete queued content
```

Runtime scheduling should reflect this where appropriate.

---

# 42. Backpressure

If the source changes faster than processing:

```text
content A
content B
content C
content D
```

CRAI should not blindly queue:

```text
A
B
C
D
```

for full Recognition + Translation.

---

# 43. Coalescing

Runtime/source policy may coalesce obsolete pending content.

Example:

```text
A queued
B observed
C observed
D stable/current
```

Potential result:

```text
A superseded
B discarded
C discarded
D processed
```

Exact coalescing policy belongs to Runtime/source architecture.

---

# 44. Bounded Queue

Continuous observation must never create unbounded memory/work growth.

Required principles:

```text
bounded pending work

bounded retained Candidates

bounded provider concurrency

bounded cached intermediate data

bounded diagnostics buffering
```

---

# 45. Observation vs Processing Queue

Observation may continue even while processing is saturated.

It should not create one WorkItem per observation.

Conceptually:

```text
many observations
    ↓
one current meaningful content authority
    ↓
bounded Runtime work
```

---

# 46. Scroll Storm

Example:

```text
user rapidly scrolls through 15 comic panels
```

Desired behavior:

```text
observe movement
    ↓
avoid full processing of transient panels
    ↓
wait for stable current content
    ↓
process final/current panel
```

Not:

```text
queue 15 Translation jobs
```

---

# 47. User Reverses Scroll

Example:

```text
A
↓
B
↓
C
↓
B again
```

If compatible cached outputs exist for B:

```text
reuse may be possible
```

after compatibility and current-authority validation.

---

# 48. Cache Reuse During Change

Potential flow:

```text
new current content
    ↓
fingerprint
    ↓
cache lookup
    ↓
compatible Published Artifact available?
    ├── Yes
    │    ↓
    │  authority validation
    │    ↓
    │  reuse
    │
    └── No
         ↓
      execute work
```

---

# 49. Cache Hit Can Reduce Supersession Waste

Fast compatible reuse may allow CRAI to update user-visible content before expensive provider work becomes necessary.

This is an optimization.

It does not alter ownership.

---

# 50. Recognition Supersession

Example:

```text
R31
Recognition Attempt A
    ↓
new content
    ↓
R32
```

Runtime may cancel A.

If A finishes:

```text
Recognition Candidate
```

cannot become current under R32.

---

# 51. Text Processing Supersession

Text Processing is usually cheaper than Recognition/Translation.

Runtime may sometimes allow nearly completed obsolete work to finish if cancellation cost exceeds benefit.

Even so:

```text
publication authority
```

still determines whether output becomes current.

---

# 52. Translation Supersession

Translation may involve expensive remote requests.

When obsolete:

```text
cancel where possible
```

but always assume:

```text
remote response may arrive late
```

The late result must remain non-current.

---

# 53. Presentation Supersession

Presentation work may also be superseded.

Example:

```text
TranslationArtifact T20
    ↓
Presentation Candidate P20

before publish:
T21 becomes current
```

P20 must not overwrite a newer current presentation.

---

# 54. UI Projection Supersession

UI Adapter also protects against stale projection.

Example:

```text
ViewModel projection V20 building
    ↓
V21 source state arrives
    ↓
V21 publishes first
    ↓
V20 completes later
```

V20 must not replace V21.

This uses UI Adapter's own projection revision/authority.

---

# 55. Distinct Staleness Layers

CRAI may therefore have:

```text
Runtime Candidate staleness

Artifact publication staleness

Application projection staleness

ViewModel projection staleness
```

These are related but not identical.

---

# 56. User-Visible Continuity

While newer content is being processed, CRAI may keep displaying:

```text
last valid Published PresentationArtifact
```

rather than partially replacing it.

---

# 57. Previous Presentation During Scroll

Possible UX policy:

```text
user scrolls
    ↓
old translation remains temporarily
```

or:

```text
old translation becomes visually de-emphasized/hidden
```

depending on Presentation/UI policy.

The semantic Artifact remains valid as historical output.

---

# 58. Never Mix Revisions Visually

Avoid:

```text
Recognition from content B
+
Translation from content A
+
geometry from content C
```

in one user-visible current presentation.

All published semantic relationships must preserve compatible provenance.

---

# 59. Atomic Current Projection

Current UI projection should correspond to a coherent accepted semantic state.

Partial progressive presentation requires explicit contracts.

It must not emerge accidentally from out-of-order completion.

---

# 60. Content Change During Retry

Example:

```text
R40
Translation Attempt T1 fails
    ↓
retry scheduled
    ↓
before T2 starts:
new content
    ↓
R41 current
```

Runtime should not start obsolete T2 unless policy explicitly allows cache/history work.

---

# 61. Content Change During Retry Backoff

If obsolete work is waiting in retry backoff:

```text
supersession
```

should normally eliminate that pending retry.

No new Attempt should be created for obsolete current-display work.

---

# 62. Content Change During Provider Fallback

Example:

```text
Provider A failed
Provider B fallback pending
```

then new content supersedes execution.

Fallback should not proceed for current-display authority unless the old work has another justified purpose.

---

# 63. Manual Retranslation vs Content Change

A user may request retranslation of current content.

Conceptually:

```text
Content A
RuntimeRevision R50
    ↓
user requests alternate Translation
    ↓
new execution requirement
```

If content changes to B before completion:

```text
the retranslation may become obsolete for current display
```

unless explicitly retained for history/correction workflows.

---

# 64. User Correction vs Content Change

Explicit user corrections may have longer semantic lifetime than current screen visibility.

Example:

```text
user corrects character name
```

The correction may update:

```text
Translation memory
Glossary
Correction domain
```

according to architecture.

It should not be discarded merely because the source view changed.

---

# 65. Durable User Intent vs Ephemeral Content

Important distinction:

```text
screen frame/content
    → ephemeral

user glossary correction
    → potentially durable
```

Supersession applies to execution authority, not automatically to durable user intent.

---

# 66. Reading Session Pause During Content Change

If user pauses while content is changing:

```text
Reading Session ACTIVE
    ↓
PAUSED
```

Application/Runtime may:

```text
stop accepting new content work

cancel selected work

retain current presentation
```

according to policy.

---

# 67. Resume After Pause

After resume:

```text
current source must be observed again
```

Do not assume the content visible before pause remains current.

Potential flow:

```text
PAUSED
    ↓
ACTIVE
    ↓
observe current source
    ↓
establish current content authority
    ↓
new RuntimeRevision if required
```

---

# 68. Session Stop During Processing

```text
Session STOPPING
    ↓
Runtime work associated with session
        → cancellation/supersession
```

Late Candidates after session termination must not become current.

---

# 69. Application Shutdown

Application shutdown supersedes normal content-change handling.

Priority becomes:

```text
stop accepting new work

cancel current execution

bounded cleanup

prevent late publication
```

---

# 70. Source Loss

Content may disappear because:

```text
window closed
permission revoked
browser tab removed
capture source invalidated
```

This is not ordinary content change.

It is a source/capability condition.

Application/Reading Session determines recovery behavior.

---

# 71. Temporary Source Obstruction

Temporary obstruction may include:

```text
menu overlay
notification
window switch
loading dialog
```

Observation policy may:

```text
wait

treat as unstable

accept as new content

pause processing
```

depending on source mode.

---

# 72. Source Returns

When the original readable content returns:

```text
new current source observation
```

is evaluated normally.

Previously cached Artifacts may be reusable.

---

# 73. Structured Text Change

The same architecture applies to structured sources.

Example:

```text
browser chapter content changes
    ↓
new structured-source content authority
    ↓
new RuntimeRevision
```

No Capture/Recognition step is required.

---

# 74. Structured DOM Mutation

Not every DOM mutation should create new semantic processing.

Ignore unrelated mutations such as:

```text
advertisements
navigation counters
comments
analytics elements
style changes
```

when source isolation can distinguish them.

---

# 75. Structured Content Stability

Structured sources may need a different stability policy than screen images.

Example:

```text
DOM mutation batch
    ↓
content extraction settles
    ↓
semantic source snapshot accepted
```

The same supersession principles apply afterward.

---

# 76. File-Based Content Change

For imported files:

```text
file modified externally
```

may create a new source/input revision depending on import policy.

This may be treated more like context/source replacement than high-frequency screen change.

---

# 77. Content Fingerprint

Content identity may use:

```text
content fingerprint
```

but fingerprint is not authority.

It supports:

```text
duplicate detection
cache lookup
diagnostics
```

Authority remains typed Runtime/session/module state.

---

# 78. Fingerprint Collision

Architecture must not assume fingerprints are infallible.

Critical reuse may require additional compatibility/provenance validation.

---

# 79. Runtime Priority

Suggested relative priority:

```text
current user-visible content
    ↓ highest

explicit user retry/retranslation

current background processing

prefetch

obsolete/historical work
    ↓ lowest
```

Exact priority values belong to Runtime/Scheduler configuration.

---

# 80. Resource Pressure

If Runtime is under CPU/GPU/memory/provider pressure:

```text
current fresh content
```

should generally outrank obsolete work.

Resource Manager/Scheduler enforce actual limits.

---

# 81. Rate-Limited Provider

If Translation provider is rate-limited while content changes rapidly:

```text
do not queue every obsolete translation request
```

Runtime/provider policy should prioritize current useful work.

---

# 82. Cost-Sensitive Remote Work

Remote Translation may incur monetary cost.

Supersession/backpressure should prevent unnecessary requests where possible.

Cost optimization is secondary to correctness but important to product efficiency.

---

# 83. Work Already Sent Remotely

Once a remote request has been sent:

```text
cost may already be incurred
```

Cancellation may not recover that cost.

Late-result rejection still protects correctness.

---

# 84. Reusable Obsolete Results

An obsolete result may still be valuable if:

```text
content is likely to reappear

cache policy permits reuse

privacy scope permits storage

configuration is compatible
```

Therefore obsolete does not necessarily mean destroy immediately.

---

# 85. Privacy of Obsolete Data

Superseded raw screen data should not be retained indefinitely merely because it might be useful.

Retention must follow:

```text
cache policy
privacy policy
resource lifecycle
```

---

# 86. Raw Capture Lifetime

Typically:

```text
raw observations
    → shortest lifetime

accepted Capture Artifact
    → bounded processing lifetime

semantic Artifacts
    → cache/history policy
```

---

# 87. Diagnostics

Diagnostics should observe content-change behavior without becoming control authority.

Useful observations include:

```text
change detections

stable candidates

Runtime supersessions

cancelled WorkItems

cancelled Attempts

late results

stale Candidate rejections

queue coalescing

cache reuse

time-to-current-presentation
```

---

# 88. Diagnostics Correlation

A useful trace may include:

```text
SessionId
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
content fingerprint?
```

High-cardinality values belong primarily to traces/logs, not metric labels.

---

# 89. Metrics

Possible metrics:

```text
content_change_detected_total

content_stable_total

runtime_revision_superseded_total

workitem_superseded_total

attempt_cancelled_total

candidate_stale_rejected_total

content_coalesced_total

late_provider_result_total

current_content_latency_ms
```

Exact metric names belong to Diagnostics/Telemetry architecture.

---

# 90. Important Product Metric

One key measure is:

```text
time from readable stable content
to useful current translation visible
```

This matters more to the user than the completion rate of obsolete work.

---

# 91. False Positive Change

A false positive change may trigger unnecessary Runtime work.

Architecture should tolerate this safely:

```text
extra work may execute
```

but correctness remains protected through Artifact/provenance authority.

---

# 92. False Negative Change

A missed meaningful content change may leave stale presentation visible too long.

This is primarily a Capture/source-observation quality issue.

It should be measured separately from Runtime stale-result safety.

---

# 93. Stability Too Short

If debounce/stability threshold is too short:

```text
transient content
```

may create excessive work.

---

# 94. Stability Too Long

If threshold is too long:

```text
translation appears late
```

and disrupts reading.

The correct value requires prototype measurement.

---

# 95. Adaptive Stability

Future versions may adapt stability based on:

```text
scroll behavior

source type

processing latency

device performance

user reading speed
```

This is optional policy, not current architecture requirement.

---

# 96. Example — Normal Scroll

```text
Content A current
    ↓
user scrolls
    ↓
motion observed
    ↓
no expensive new processing
    ↓
motion stops
    ↓
Content B stable
    ↓
RuntimeRevision R2
    ↓
old R1 superseded
    ↓
process B
    ↓
PresentationArtifact B published
```

---

# 97. Example — Rapid Scroll

```text
A
↓
B
↓
C
↓
D
↓
E stable
```

Preferred:

```text
B/C/D never fully processed
E becomes current
```

rather than:

```text
five complete pipelines queued
```

---

# 98. Example — Late Recognition

```text
R10
Recognition running
    ↓
R11 current
    ↓
Recognition R10 completes
    ↓
Candidate RecognitionArtifact R10
    ↓
stale authority
    ↓
not published as current
```

---

# 99. Example — Late Translation

```text
R20
Translation provider request sent
    ↓
R21 current
    ↓
cancellation unavailable
    ↓
R20 response arrives
    ↓
Candidate TranslationArtifact
    ↓
rejected from current publication
```

---

# 100. Example — Cache Return

```text
Content A processed earlier
    ↓
user scrolls away
    ↓
Content A appears again
    ↓
fingerprint match
    ↓
compatible cached Artifact
    ↓
current authority validation
    ↓
fast reuse
```

---

# 101. Example — Context Change During Content Processing

```text
Session S1
ReadingContextRevision C5
RuntimeRevision R22
Translation running
    ↓
user changes capture region
    ↓
ReadingContextRevision C6
    ↓
Application replans
    ↓
RuntimeRevision R23
    ↓
R22 superseded
```

This changes both domain context and execution authority.

---

# 102. Example — Content Change Only

```text
Session S1
ReadingContextRevision C5
RuntimeRevision R22
    ↓
user scrolls inside same region
    ↓
ReadingContextRevision remains C5
    ↓
RuntimeRevision R23
```

This changes execution/content authority only.

---

# 103. Example — Pause During Scroll

```text
scrolling
    ↓
user pauses CRAI
    ↓
Reading Session PAUSED
    ↓
new current-content processing suppressed
    ↓
active Runtime work handled by cancellation policy
```

---

# 104. Example — Resume

```text
Reading Session PAUSED
    ↓
user resumes
    ↓
observe current source again
    ↓
current stable content determined
    ↓
new RuntimeRevision if needed
```

---

# 105. Example — Source Lost

```text
screen region source invalid
    ↓
Capture source error
    ↓
Application evaluates source recovery
```

This is not represented merely as:

```text
new RuntimeRevision
```

because the source/context itself may no longer be valid.

---

# 106. Error Ownership

Content-change flow does not introduce generic:

```text
FLOW-*
```

errors for module failures.

Original owners remain:

```text
CAP-*
REC-*
TXT-*
TRN-*
PRES-*
RUN-*
SES-*
```

---

# 107. Supersession Is Not `RUN-*` Failure by Default

Normal supersession may be represented through Runtime state/outcome without generating an error.

Only unexpected Runtime behavior should produce Runtime-owned error semantics.

---

# 108. Event Bus

Possible committed facts may be published according to owner `EVENTS.md`.

Examples may include:

```text
ReadingContextChanged

RuntimeRevisionSuperseded

Artifact publication facts
```

This flow does not establish exact canonical names.

---

# 109. Event Bus Does Not Propagate Cancellation

Do not use:

```text
ContentChanged
    ↓
CancelPipelineRequested
```

as global Event Bus command flow.

Application/Runtime contracts handle execution changes explicitly.

---

# 110. Event Bus Does Not Create RuntimeRevision

A content-change event/fact may inform observers.

Creation/supersession of RuntimeRevision belongs to explicit Runtime/business orchestration.

---

# 111. Current-State Query

Consumers that miss events should be able to query/rebuild from current authoritative state where appropriate.

Current authority is not reconstructed solely from Event history.

---

# 112. Crash During Content Change

If CRAI crashes while content is changing:

```text
old live Attempts
```

are not restored as running.

After restart:

```text
restore durable session context
    ↓
observe current source
    ↓
accept current content
    ↓
create new Runtime execution authority
```

---

# 113. Restart Does Not Continue Old Frame Queue

Pending observations/content candidates from before crash are ephemeral.

They should not be replayed blindly.

---

# 114. Multi-Session Change

If multiple Reading Sessions are eventually supported:

```text
Session A content changes
Session B content stable
```

Runtime may interleave their WorkItems.

Supersession must remain scoped to the relevant session/execution authority.

---

# 115. Supersession Scope

A new RuntimeRevision for Session A must not automatically supersede independent Session B work.

Typed ownership/correlation prevents cross-session cancellation.

---

# 116. Cross-Session Cache

Cache reuse across sessions may be possible only when:

```text
privacy scope
source compatibility
configuration compatibility
semantic cache policy
```

allow it.

---

# 117. Security

Content-change processing must not increase data exposure unnecessarily.

Rapidly changing screen content should not produce uncontrolled remote uploads.

---

# 118. Remote Request Gate

Before remote provider execution, Runtime/module policy should ensure:

```text
work is still useful/current enough
```

where practical.

This reduces:

```text
privacy exposure
cost
wasted bandwidth
provider rate-limit pressure
```

---

# 119. Data Minimization

Only required current content should cross provider boundaries.

Obsolete full-screen data should not be sent simply because it was once observed.

---

# 120. Design Principles

The Content Change Flow follows:

```text
observation is high frequency

semantic work is bounded

stable content is preferred

content and reading context are distinct

RuntimeRevision owns current execution authority

new work supersedes obsolete work

cancellation is best effort

publication validation guarantees correctness

freshness outranks obsolete completeness

cache may accelerate reuse

previous valid presentation remains safe
```

---

# 121. Critical Invariants

1. Content change is not automatically ReadingContext change.

2. ReadingContextRevision belongs to Reading Session.

3. RuntimeRevisionId belongs to Runtime.

4. One ReadingContextRevision may produce many RuntimeRevisions.

5. Observation frequency does not equal processing frequency.

6. Not every observed frame becomes a WorkItem.

7. Stable-content policy gates expensive processing.

8. Duplicate detection is optimization, not authority.

9. New Runtime authority may supersede old authority at any time.

10. Supersession is not automatically failure.

11. Runtime owns WorkItem cancellation.

12. Runtime owns Attempt cancellation.

13. Cancellation is best effort.

14. Late results are expected.

15. Candidate publication always checks current authority.

16. Attempt success does not imply publication.

17. Stale Candidate does not overwrite current Artifact.

18. Event Bus does not control supersession.

19. Event Bus does not control cancellation.

20. Event Bus does not trigger downstream execution.

21. Runtime queues remain bounded.

22. Obsolete pending work may be coalesced.

23. Current useful content receives scheduling priority.

24. Provider retries/fallbacks for obsolete work should not continue unnecessarily.

25. Cache results require compatibility and authority validation.

26. UI projection also protects against stale local projection.

27. Previous valid Presentation remains safe while current content processes.

28. Semantic Artifacts never mix incompatible provenance.

29. Durable user corrections are not discarded merely due to content supersession.

30. Crash recovery starts from current authoritative source, not old live work.

---

# 122. Deprecated Legacy Flow

Deprecated:

```text
Frame Changed
    ↓
Revision++
    ↓
Cancel Pipeline
    ↓
Start OCR
    ↓
OCR Completed
    ↓
Translation
```

Runtime v2 uses:

```text
Source Observation
    ↓
Stable Current Content
    ↓
Business Execution Planning
    ↓
RuntimeRevision
    ↓
WorkItems / Attempts
    ↓
Candidate Artifacts
    ↓
Authority Validation
    ↓
Published Artifacts
```

When newer content appears:

```text
RuntimeRevision N
    ↓
superseded by
    ↓
RuntimeRevision N+1
```

Old work may finish.

It cannot regain current authority.

---

# 123. Relationship to SCREEN_COMIC_FLOW.md

`SCREEN_COMIC_FLOW.md` applies this flow specifically to:

```text
screen-based comic reading
```

This document defines the reusable content-change principles underneath that use case.

---

# 124. Relationship to READING_SESSION_FLOW.md

`READING_SESSION_FLOW.md` defines:

```text
Session lifecycle
ReadingContext
ReadingContextRevision
```

This document defines how content may repeatedly change while the same Reading Session remains ACTIVE.

---

# 125. Relationship to STRUCTURED_TEXT_FLOW.md

Structured text sources may use different observation/stability mechanisms.

They still follow the same:

```text
new current content
    ↓
Runtime authority
    ↓
supersession
    ↓
stale-result protection
```

principles.

---

# 126. Related Documents

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
├── CANCELLATION.md
├── CACHE_POLICY.md
└── RESOURCE_LIFECYCLE.md

doc/01-architecture/flows/
├── READING_SESSION_FLOW.md
├── CONTENT_CHANGE_FLOW.md
├── SCREEN_COMIC_FLOW.md
└── STRUCTURED_TEXT_FLOW.md

doc/02-modules/
├── capture/
├── reading-session/
├── recognition/
├── text-processing/
├── translation/
├── presentation/
└── ui-adapter/
```

---

# 127. Open Decisions

The following remain prototype/policy decisions:

```text
observation sampling rate

visual difference threshold

stability debounce duration

adaptive stability algorithm

coalescing strategy

maximum pending current-content work

whether nearly completed obsolete work may finish

remote-provider cancellation threshold

cache retention for superseded content

UI behavior while source is unstable

when previous presentation should be hidden

whether current work should be delayed briefly
to reduce scroll-related waste

structured-source mutation debounce policy

resource-pressure priority policy
```

These do not change the core authority model.

---

# 128. Completion Criteria

This flow is synchronized when:

* content change is separated from ReadingContext change;
* ReadingContextRevision remains Reading Session-owned;
* RuntimeRevision remains Runtime-owned;
* observation does not create unbounded downstream work;
* stability gating is explicit;
* duplicate detection does not become authority;
* supersession replaces generic pipeline cancellation semantics;
* cancellation remains best effort;
* late-result rejection is mandatory;
* Candidate vs Published boundary is preserved;
* stale results cannot overwrite current Artifacts;
* event-driven cancellation/stage chaining is absent;
* cache reuse still requires current-authority validation;
* UI projection staleness is recognized separately;
* crash recovery starts from current source rather than old queued work.

---

# 129. Summary

The normal content-change loop is:

```text
Observe
    ↓
Change Detected
    ↓
Wait for Stability
    ↓
Accept Current Content
    ↓
RuntimeRevision
    ↓
WorkItems / Attempts
    ↓
Candidate Artifacts
    ↓
Authority Validation
    ↓
Published Artifacts
```

If content changes again:

```text
RuntimeRevision N
    ↓
SUPERSEDED
```

and:

```text
RuntimeRevision N+1
    ↓
becomes current
```

The old work may:

```text
cancel

finish late

become cacheable

be discarded
```

but it must never become current again.

The central invariant is:

```text
Cancellation saves resources.

Supersession changes authority.

Publication validation
protects correctness.
```
