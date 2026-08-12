# CRAI State Machine Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/core/STATE_MACHINE.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the architecture-wide state-machine rules used by CRAI.

It does not define one monolithic state machine for the entire application.

Instead, CRAI uses multiple independently owned state domains:

```text
Application Lifecycle

Reading Session Lifecycle

Runtime Execution
├── RuntimeRevision
├── WorkItem
└── Attempt

Module Lifecycles
├── Capture
├── Recognition
├── Text Processing
├── Translation
├── Presentation
├── Preferences
├── Diagnostics
└── UI Adapter

Scoped Operations
├── export
├── projection
├── retry attempt
├── provider request
└── other bounded operations
```

The purpose of these state machines is to ensure:

* explicit ownership;
* deterministic transitions;
* stale-result protection;
* bounded retries;
* cancellation safety;
* immutable published results;
* recoverable degradation;
* concurrency without authority ambiguity.

---

# 2. Central Rule

The most important rule is:

```text
State belongs
to the component
that owns the authority
represented by that state.
```

Examples:

```text
Reading Session
    owns session lifecycle

Runtime
    owns execution lifecycle

Recognition
    owns Recognition-local lifecycle

Diagnostics
    owns Diagnostics lifecycle

UI Adapter
    owns UI Adapter lifecycle
```

A module must not mirror another module's lifecycle as though it owns it.

---

# 3. No Global Processing State Machine

CRAI v2 does not use:

```text
CAPTURING
    ↓
OCR_PROCESSING
    ↓
SEGMENTING
    ↓
TRANSLATING
    ↓
RENDERING
```

as one authoritative application/session state machine.

Those activities may:

* overlap;
* execute concurrently;
* be retried independently;
* produce multiple Attempts;
* complete out of order;
* be superseded independently.

Therefore execution state belongs to Runtime.

---

# 4. State Domains

CRAI separates state into five major domains.

```text
1. Application Lifecycle State

2. Domain Lifecycle State

3. Runtime Execution State

4. Module Capability/Lifecycle State

5. Scoped Operation State
```

These state domains must not be merged.

---

# 5. Application Lifecycle State

Application lifecycle describes whether the CRAI process/application environment is usable.

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

An application may internally distinguish foreground/background behavior, but that is a platform/application policy concern rather than the core processing state model.

---

# 6. Application `UNINITIALIZED`

Meaning:

```text
application composition has not started
```

No runtime/module readiness is guaranteed.

---

# 7. Application `INITIALIZING`

Typical work:

```text
load configuration
initialize infrastructure
initialize required modules
discover capabilities
initialize Runtime
restore allowed persistent state
```

Partial optional capability failure should normally lead to:

```text
DEGRADED
```

rather than a global fatal state.

---

# 8. Application `READY`

Meaning:

Required application capabilities are usable.

`READY` does not mean:

```text
no Reading Session exists
```

and does not mean:

```text
no Runtime work is executing
```

Those are separate state domains.

---

# 9. Application `DEGRADED`

Meaning:

The application remains usable while some capabilities are impaired.

Examples:

```text
optional Translation provider unavailable
Tracing unavailable
System notifications unavailable
one UI frontend unavailable
```

Working capabilities should remain available.

---

# 10. Application `STOPPING`

Meaning:

The application is shutting down.

Typical coordination:

```text
stop accepting new top-level use cases
request session shutdown
request Runtime cancellation
perform bounded infrastructure flush
release resources
```

Shutdown must remain bounded.

---

# 11. Application `STOPPED`

Terminal state for the application instance.

---

# 12. No Ordinary Global `FATAL_ERROR`

CRAI does not require a long-lived:

```text
FATAL_ERROR
```

state for ordinary failures.

If the application cannot safely operate:

```text
INITIALIZING / READY / DEGRADED
        ↓
STOPPING
        ↓
STOPPED
```

may be sufficient.

Crash reporting/restart policy belongs to Application/runtime deployment.

---

# 13. Reading Session State

Reading Session state describes the lifecycle of a user's reading context.

It does not describe which processing stage Runtime is currently executing.

Canonical Reading Session lifecycle is defined by:

```text
doc/02-modules/reading-session/STATES.md
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

Additional owner-specific states may exist as defined by the module.

---

# 14. Reading Session Authority

Reading Session owns:

```text
SessionId
SessionConfiguration
ReadingContext
ReadingContextRevision
session lifecycle
```

It does not own:

```text
WorkItem lifecycle
Attempt lifecycle
Runtime retry
Scheduler admission
provider retry
Artifact publication
```

---

# 15. No Session `PROCESSING` Authority

A session may have Runtime work while remaining:

```text
ACTIVE
```

It does not need to transition:

```text
ACTIVE
    ↓
PROCESSING
    ↓
DISPLAYING
```

for every piece of content.

Processing is Runtime state.

Presentation/display state belongs to Presentation/UI projection.

---

# 16. Session and Runtime May Progress Independently

Example:

```text
Reading Session = ACTIVE

Runtime:
    WorkItem A = RUNNING
    WorkItem B = READY
    Attempt C = CANCELLED

Presentation:
    current Published Artifact = revision 17
```

These may all coexist.

No single session enum should attempt to encode all of them.

---

# 17. ReadingContextRevision

Reading Session may own:

```text
ReadingContextRevision
```

A new revision indicates a committed change to the reading context.

Examples:

```text
source changed
region changed
session-specific language changed
relevant session configuration changed
```

---

# 18. Revision Is Not Task Identity

Do not use:

```text
contentRevision
taskId
```

as generic architecture-wide identities.

Runtime v2 distinguishes:

```text
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
```

Each identifier has a different owner and meaning.

---

# 19. RuntimeRevision

Runtime owns:

```text
RuntimeRevisionId
```

A RuntimeRevision represents one coherent execution authority derived from a committed application/session context.

Conceptually:

```text
Reading Context / Configuration
        ↓
Business Execution Planning
        ↓
RuntimeRevision
```

---

# 20. RuntimeRevision and ReadingContextRevision

They are related but not identical.

```text
ReadingContextRevision
    → domain/session authority

RuntimeRevisionId
    → execution authority
```

One ReadingContextRevision may lead to a RuntimeRevision.

A RuntimeRevision may also be replaced for Runtime-specific reasons without changing Reading Session domain meaning.

---

# 21. WorkItem

A WorkItem represents one schedulable unit of logical Runtime work.

Conceptually:

```text
WorkItem
├── WorkItemId
├── RuntimeRevisionId
├── workType
├── inputRefs
├── dependencyRefs
├── priority
└── execution policy
```

Detailed contract belongs to Runtime architecture.

---

# 22. WorkItem Lifecycle

Architecture-level lifecycle:

```text
CREATED
   ↓
READY
   ↓
RUNNING
   ↓
SUCCEEDED
```

Possible terminal alternatives:

```text
SKIPPED
CANCELLED
FAILED
SUPERSEDED
```

Exact vocabulary remains authoritative in Runtime documents.

---

# 23. Attempt

An Attempt represents one concrete execution of a WorkItem.

A retry creates another Attempt.

It does not rewind the entire pipeline state.

---

# 24. Attempt Lifecycle

Conceptually:

```text
CREATED
   ↓
QUEUED
   ↓
RUNNING
   ↓
SUCCEEDED
```

Alternative outcomes may include:

```text
FAILED
CANCELLED
TIMED_OUT
SUPERSEDED
```

Exact definitions belong to Runtime.

---

# 25. Retry Ownership

Retry is Runtime execution behavior.

Correct:

```text
WorkItem
    ↓
Attempt 1 FAILED
    ↓
retry policy
    ↓
Attempt 2
```

Incorrect:

```text
TRANSLATING
    ↓
RETRY_WAIT
    ↓
TRANSLATING
```

as a global pipeline state transition.

---

# 26. Retry Does Not Change Domain Lifecycle

Example:

```text
Reading Session = ACTIVE

Translation WorkItem:
    Attempt 1 = FAILED
    Attempt 2 = RUNNING
```

Reading Session does not need:

```text
RECOVERING
```

merely because one processing Attempt is retrying.

---

# 27. Recovery vs Retry

These concepts are distinct.

```text
Retry
    → Runtime execution mechanism

Recovery
    → owner-specific restoration of capability/state
```

Example:

```text
Capture provider request timeout
    → Runtime retry

Capture source permanently lost
    → Capture/Reading Session recovery
```

---

# 28. Provider Fallback

Provider fallback is not a global state machine.

Preferred:

```text
Attempt
    ↓
provider failure
    ↓
policy evaluation
    ↓
next Attempt with another provider
```

Provider selection/fallback semantics belong to the relevant module/provider-management/runtime policy.

---

# 29. Cancellation Ownership

Runtime owns execution cancellation.

Cancellation may originate from:

```text
application shutdown
session stop
ReadingContextRevision supersession
RuntimeRevision supersession
deadline
resource policy
user cancellation
```

---

# 30. Cancellation Does Not Mean Immediate Stop

Cancellation is a request/authority transition.

Actual provider execution may:

```text
stop immediately
complete cooperatively
ignore cancellation temporarily
return later
```

Therefore stale-result validation remains mandatory.

---

# 31. Supersession

When newer authority replaces older Runtime work:

```text
RuntimeRevision A
    ↓
RuntimeRevision B committed
```

work from A may become:

```text
SUPERSEDED
```

or receive cancellation.

The old result must never overwrite current authority.

---

# 32. Stale Result Protection

This remains a mandatory CRAI invariant.

Before a Candidate result becomes Published state, the owner/runtime must validate its authority.

Conceptually:

```text
Attempt completes
    ↓
Candidate Result
    ↓
Authority Validation
    ↓
Accepted
or
Rejected as stale/superseded
```

---

# 33. Candidate Result

A Candidate is an execution result not yet accepted as authoritative published state.

Examples:

```text
Candidate Capture Artifact
Candidate Recognition Artifact
Candidate SourceDocument Artifact
Candidate Translation Artifact
Candidate Presentation Artifact
```

Candidate creation does not imply publication.

---

# 34. Publication Boundary

Preferred:

```text
Attempt
    ↓
Candidate Artifact
    ↓
Runtime/owner authority validation
    ↓
Published Artifact
```

Publication is a committed architecture state change.

---

# 35. Published Artifact Immutability

Once published:

```text
Artifact N
```

must not mutate into:

```text
Artifact N+1
```

A new accepted result creates a new Artifact/version.

---

# 36. Stale Candidates

A stale Candidate may:

```text
be discarded
be cached when policy permits
remain available for diagnostics
```

but it must not update the current authoritative projection.

---

# 37. No Stage-to-Stage State Chaining

Do not model execution authority as:

```text
Recognition COMPLETED
    ↓
Text Processing starts

Text Processing COMPLETED
    ↓
Translation starts
```

Instead:

```text
BusinessExecutionPlan
        ↓
Runtime dependencies
        ↓
WorkItems become READY
```

Module completion events are facts, not downstream commands.

---

# 38. Business Pipeline vs Runtime State

Business Pipeline Orchestration defines:

```text
what logical processing should happen
dependencies
conditions
artifact relationships
```

Runtime defines:

```text
when work executes
which Attempt executes
retry
cancellation
deadline
queueing
resource admission
```

These must remain separate.

---

# 39. Module Lifecycle State

Every module may own its own lifecycle.

Common recommended shape:

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

Not every module must use exactly this vocabulary, but lifecycle meaning should remain small.

---

# 40. Module Lifecycle Does Not Encode Every Operation

Incorrect:

```text
Diagnostics = EXPORTING
UI Adapter = RENDERING
Translation = TRANSLATING
```

for every active operation.

Preferred:

```text
Module = READY
Scoped Operation = active
```

---

# 41. Scoped Operation State

Longer bounded operations may have their own local state machine.

Examples:

```text
Diagnostic Bundle Export

ViewModel Projection

Navigation

Provider Request

Artifact Import
```

These scoped states do not change the entire module lifecycle.

---

# 42. Example — Diagnostic Export

```text
Diagnostics = READY

Export Operation:
VALIDATING
    ↓
COLLECTING
    ↓
REDACTING
    ↓
SERIALIZING
    ↓
COMPLETED
```

---

# 43. Example — UI Projection

```text
UI Adapter = READY

Projection:
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

---

# 44. State Authority Rule

Only the owner may commit its state transition.

Example:

```text
Runtime
    owns Attempt RUNNING → SUCCEEDED
```

Recognition may return execution output.

Recognition must not directly mark:

```text
Runtime Attempt = SUCCEEDED
```

unless invoking an explicit Runtime contract that owns the transition.

---

# 45. No Universal `StateTransitionService`

CRAI does not require one central:

```text
StateTransitionService
```

for all modules.

Each state owner may implement its own transition mechanism.

Architecture requires consistency of rules, not one god-service.

---

# 46. Transition Contract

A transition should conceptually validate:

```text
owner
current state
requested transition
preconditions
expected revision/version when required
```

and produce:

```text
new committed state
or
explicit rejection
```

---

# 47. Transition Atomicity

A committed transition must appear atomic to external consumers.

They must observe either:

```text
state N
```

or:

```text
state N+1
```

not a partially mutated state.

---

# 48. Transition Cause

Important transitions should retain safe causation metadata.

Conceptually:

```text
StateTransitionRecord
├── ownerId
├── fromState
├── toState
├── occurredAt
├── reasonCode?
├── correlationId?
├── causationId?
└── safeMetadata?
```

This record is diagnostics/audit metadata, not necessarily durable domain state.

---

# 49. Event Publication

When a transition has a corresponding domain event:

```text
state commit
    ↓
event publish
```

Never:

```text
publish event
    ↓
then attempt state commit
```

---

# 50. Publication Failure

If state commit succeeds but Event Bus publication fails:

```text
committed state remains authoritative
```

Infrastructure handles publication recovery.

Do not rollback valid state merely because event delivery failed.

---

# 51. Events Are Facts

State events should use committed facts such as:

```text
ReadingContextChanged
PreferenceChanged
DiagnosticCapabilityChanged
```

They should not act as hidden commands.

---

# 52. State Event Is Not Execution Authority

Incorrect:

```text
RecognitionCompleted
    ↓
Translation subscribes
    ↓
Translation begins
```

Correct:

```text
RecognitionArtifact published
    ↓
Runtime dependency condition satisfied
    ↓
Translation WorkItem becomes READY
```

---

# 53. Pause Semantics

Pause is owner-specific.

A Reading Session may become:

```text
PAUSED
```

which means no new session-authorized work should normally be created.

Existing Runtime work is handled according to Runtime/session cancellation policy.

---

# 54. Pause Does Not Own Attempt State

Reading Session pause may request:

```text
cancel relevant WorkItems
stop watchers
suppress new planning
```

but Runtime commits WorkItem/Attempt execution states.

---

# 55. Resume Semantics

On resume:

```text
validate source/context
re-establish required capabilities
create/update authoritative ReadingContext if needed
plan new Runtime work only from current state
```

Do not resume old network/provider execution from process memory after restart.

---

# 56. Crash Recovery

After process crash/restart:

```text
restore persistent domain/configuration state
    ↓
mark previous Runtime execution interrupted
    ↓
reinitialize capabilities
    ↓
recompute current authoritative work
```

Do not attempt to restore an in-memory Attempt as though it were still running.

---

# 57. Persistent vs Ephemeral State

Persist only state that represents durable user/domain intent or required recovery authority.

Typical persistent candidates:

```text
Reading Session configuration
ReadingContext when appropriate
persistent Preferences
source profile
last accepted persistent reading position
user-selected pause state
```

---

# 58. Runtime Execution State Is Primarily Ephemeral

Normally do not persist a live:

```text
Attempt = RUNNING
```

and resume it as RUNNING after restart.

Historical execution records may be persisted for diagnostics/history.

That is different from restoring live authority.

---

# 59. Cache Is Not State Authority

Cache may contain previous results.

A cache hit does not automatically make cached data authoritative.

Correct:

```text
cache lookup
    ↓
Candidate cached Artifact
    ↓
compatibility + authority validation
    ↓
Published Artifact
```

---

# 60. Cache Hit Does Not Skip Authority Validation

Even a valid cached Artifact must still match:

```text
input identity
configuration
required revisions
provider/model compatibility where relevant
current Runtime authority
```

---

# 61. Content Stability

Content stability is not a global pipeline state.

It may be represented by:

```text
Capture/detection candidate state
Watcher observation state
Runtime prerequisite
```

depending on the implementation.

Architecture only requires:

```text
unstable dynamic source
    ↓
must not trigger expensive processing prematurely
```

---

# 62. Stability and Runtime Planning

Preferred:

```text
source observations
    ↓
stability policy satisfied
    ↓
committed source/capture candidate
    ↓
Runtime work planned/admitted
```

Do not require a global:

```text
WAITING_FOR_STABILITY
```

pipeline lifecycle.

---

# 63. Concurrency

CRAI v2 explicitly permits concurrency.

Examples:

```text
Capture WorkItem A running
Recognition WorkItem B running
Translation WorkItem C running
Presentation ViewModel still showing Artifact N
```

for related or independent content.

Correctness comes from typed authority and dependency checks, not from forcing one global active processing state.

---

# 64. Per-Session Concurrency

CRAI does not require:

```text
maxActivePipelinesPerSession = 1
```

as an architectural invariant.

MVP may choose conservative limits through Runtime configuration.

That is a resource/policy decision, not a state-machine truth.

---

# 65. Runtime Concurrency Limits

Concurrency limits belong to:

```text
Scheduler
Resource Manager
Runtime Configuration
```

Examples:

```text
max concurrent Capture work
max local Recognition concurrency
max remote provider requests
memory/GPU budgets
priority limits
```

---

# 66. Module Parallelism

A module may internally parallelize work.

Examples:

```text
Recognition regions
Translation units
Capture candidate analysis
```

The module must return deterministic/typed results compatible with its contract.

Runtime remains execution authority.

---

# 67. Multi-Session Behavior

Multiple Reading Sessions may coexist.

Each session owns its independent domain lifecycle.

Runtime may interleave WorkItems from multiple sessions.

Application state must not attempt to encode all session states in one enum.

---

# 68. Application Readiness vs Session Activity

Avoid:

```text
Application = ACTIVE
because session exists
```

as a required core state.

Prefer:

```text
Application = READY

Session A = ACTIVE
Session B = PAUSED
```

Application readiness and session activity are orthogonal.

---

# 69. Capability State

Modules may expose capability/health states independently from lifecycle.

Example:

```text
Translation Module = READY

Provider A = AVAILABLE
Provider B = UNAVAILABLE
```

or:

```text
Diagnostics Module = DEGRADED

Tracing = UNAVAILABLE
Logging = AVAILABLE
```

---

# 70. DEGRADED vs FAILED

Prefer:

```text
DEGRADED
```

when the owner remains partially usable.

Use terminal/failed operation outcomes only at the narrowest scope.

---

# 71. Error State Principles

Do not create global error states for errors that can be represented as:

```text
operation result
Attempt failure
capability degradation
module-owned error
```

State should represent durable meaningful authority, not every exception.

---

# 72. Business Error vs Execution Failure

Example:

```text
Translation provider timeout
```

may cause:

```text
Attempt 1 = FAILED
```

but Translation module may remain:

```text
READY
```

and Reading Session may remain:

```text
ACTIVE
```

---

# 73. Module Error Ownership

Errors remain with their module owner.

Examples:

```text
CAP-*
REC-*
TXT-*
TRN-*
PRES-*
SES-*
PREF-*
RUN-*
DIAG-*
UIA-*
```

A state transition must not erase the original error identity.

---

# 74. Timeout

Timeout belongs primarily to Runtime/Attempt execution policy.

Examples:

```text
queue deadline
Attempt timeout
provider request timeout
WorkItem deadline
```

Do not encode timeout as a global pipeline state.

---

# 75. Timeout Flow

Conceptually:

```text
Attempt RUNNING
    ↓
deadline exceeded
    ↓
Attempt TIMED_OUT
    ↓
Runtime retry/cancellation policy
```

---

# 76. Deadline vs Timeout

A WorkItem may have:

```text
deadline
```

while an individual Attempt has:

```text
timeout
```

These are not necessarily identical.

---

# 77. Cancellation Reasons

Canonical cancellation reasons belong to Runtime cancellation architecture.

Potential causes include:

```text
RuntimeRevisionSuperseded
ReadingSessionStopped
ReadingSessionPaused
ApplicationStopping
DeadlineExceeded
UserCancelled
ResourcePolicy
```

Do not duplicate a competing reason taxonomy here.

---

# 78. Supersession Is Not Error

When old work becomes irrelevant because newer authority exists:

```text
SUPERSEDED
```

is usually expected control flow.

It should not automatically be presented as a user-visible failure.

---

# 79. Skipped Work

A WorkItem may be:

```text
SKIPPED
```

when work is unnecessary.

Examples:

```text
compatible cached result accepted
no relevant text
condition not satisfied
output already current
```

This is not a module lifecycle state.

---

# 80. Presentation State

Presentation owns semantic presentation results.

Native UI rendering belongs to UI/platform implementation.

Do not put:

```text
RENDERING
```

into the global processing state machine.

---

# 81. Presentation Artifact Flow

```text
Translation Artifact
    ↓
Presentation WorkItem / operation
    ↓
Candidate Presentation Artifact
    ↓
authority validation
    ↓
Published Presentation Artifact
    ↓
UI Adapter projection
```

---

# 82. UI State

UI Adapter lifecycle and view lifecycle are separately defined in:

```text
doc/02-modules/ui-adapter/STATES.md
```

A dialog waiting for user input is not an application-wide state.

---

# 83. Diagnostics State

Diagnostics lifecycle is separately defined in:

```text
doc/02-modules/diagnostics/STATES.md
```

Telemetry collection does not create global:

```text
COLLECTING
MONITORING
EXPORTING
```

application states.

---

# 84. State Machine Composition

Architecture composes state through references rather than one mega-enum.

Conceptually:

```text
ApplicationStateSnapshot
├── applicationLifecycle
├── sessionSummaries[]
├── runtimeSummary
├── capabilitySummary
└── uiSummary?
```

Each field preserves its original authority.

---

# 85. Aggregate State Is a Projection

An aggregate state/snapshot is:

```text
read-only projection
```

It is not a new authority that may overwrite the owners.

---

# 86. Transition Idempotency

Some repeated transition requests may be treated as no-op.

Example:

```text
PAUSED → request Pause
```

may return:

```text
AlreadyPaused / NoOp
```

depending on owner contract.

Do not assume every same-state request is universally idempotent.

---

# 87. Terminal States

Terminal state is owner-specific.

Examples:

```text
Reading Session STOPPED
Attempt SUCCEEDED
Attempt FAILED
View DISPOSED
Application STOPPED
```

A terminal state must not be revived on the same logical instance unless its owner explicitly defines revival semantics.

---

# 88. New Instance vs Revival

Example:

```text
Session A STOPPED
```

should normally not become:

```text
Session A ACTIVE
```

Instead:

```text
restore configuration
    ↓
create Session B
```

if Reading Session contract defines stopped instances as terminal.

---

# 89. State Versioning

Where concurrent mutation is possible, owners should expose revision/version control.

Examples:

```text
ReadingContextRevision
PreferenceRevision
ViewModelRevision
PresentationRevision
RuntimeRevisionId
```

These revision types are distinct.

---

# 90. No Generic Revision Type

Avoid using one architecture-wide:

```text
revision
contentRevision
version
```

for unrelated domains.

Typed revisions prevent authority confusion.

---

# 91. Optimistic Concurrency

Owner mutations may require:

```text
expectedRevision
```

Example:

```text
ChangeReadingContext
expectedReadingContextRevision = 12
```

If current revision is 13:

```text
reject conflict
```

rather than silently overwrite.

---

# 92. State and Event Version

State revision and EventVersion are separate concepts.

```text
ReadingContextRevision
    ≠
EventVersion
```

EventVersion represents schema compatibility.

Revision represents state authority progression.

---

# 93. State and Artifact Version

Likewise:

```text
RuntimeRevisionId
    ≠
ArtifactId
    ≠
ArtifactVersion
```

Do not infer one from another unless explicitly defined.

---

# 94. State Observability

State transitions may be instrumented through Diagnostics.

Recommended measurements:

```text
state transition count
state duration
Attempt duration
WorkItem queue duration
cancellation count
supersession count
retry count
stale Candidate rejection count
```

---

# 95. Metrics Ownership

Metric meaning belongs to the producing owner.

Diagnostics may aggregate but does not redefine state semantics.

---

# 96. Metric Cardinality

Avoid high-cardinality metric labels such as:

```text
SessionId
WorkItemId
AttemptId
ArtifactId
full URL
```

Use traces/logs for high-cardinality correlation.

---

# 97. Logging

Structured transition diagnostics may include:

```text
ownerModule
ownerId
fromState
toState
reasonCode
correlationId
RuntimeRevisionId?
WorkItemId?
AttemptId?
safe ErrorCode?
```

---

# 98. Logging Privacy

Do not log:

```text
raw screenshot
OCR text
translation text
credentials
tokens
cookies
private keys
```

as state-transition metadata.

---

# 99. Crash History

Historical Runtime records may record that an Attempt was interrupted.

This historical observation must not be confused with live state restoration.

---

# 100. Implementation Rule

State-machine logic should be placed with its owner.

Examples:

```text
Reading Session transition code
    → Reading Session module

Attempt transition code
    → Runtime

View lifecycle
    → UI Adapter

Diagnostics capability state
    → Diagnostics
```

---

# 101. No State God Object

Avoid:

```text
GlobalStateManager
    owns all CRAI states
```

The architecture requires distributed ownership with explicit contracts.

---

# 102. No State God Event

Avoid generic:

```text
StateChanged
```

for all domains.

Prefer owner-specific events where asynchronous notification is required.

---

# 103. State Querying

Consumers should query or consume projections from the appropriate owner.

Example:

```text
UI
    ↓
Application projection
```

rather than:

```text
UI
    ↓
read all internal Runtime state machines directly
```

---

# 104. Application Projection

Application may combine:

```text
Reading Session snapshot
Runtime summary
Presentation snapshot
Diagnostics health
```

into a UI/use-case projection.

This does not transfer authority to Application projection.

---

# 105. State Machine Boundaries by Owner

```text
Application
    → application lifecycle

Reading Session
    → session lifecycle / ReadingContext

Runtime
    → RuntimeRevision / WorkItem / Attempt

Capture
    → Capture-owned local lifecycle/capabilities

Recognition
    → Recognition-owned local lifecycle/capabilities

Text Processing
    → Text Processing-owned lifecycle/capabilities

Translation
    → Translation-owned lifecycle/capabilities

Presentation
    → Presentation-owned lifecycle/artifact state

Preferences
    → Preferences lifecycle / revisions

Diagnostics
    → diagnostic lifecycle/capabilities

UI Adapter
    → adapter/view/scoped UI states
```

---

# 106. Canonical Authority Diagram

```text
User / Source Change
        ↓
Application / Reading Session
        ↓
Committed ReadingContextRevision
        ↓
Business Pipeline Orchestration
        ↓
RuntimeRevision
        ↓
WorkItems
        ↓
Attempts
        ↓
Candidate Artifacts
        ↓
Authority Validation
        ↓
Published Artifacts
        ↓
Application / UI Projection
```

---

# 107. Content Change Example

Old model:

```text
contentRevision++
    ↓
cancel old pipeline
    ↓
start pipeline
```

v2 model:

```text
source change observed
    ↓
Reading Session commits
ReadingContextRevision N+1
    ↓
Application/Business Pipeline evaluates impact
    ↓
RuntimeRevision N+1 created
    ↓
obsolete WorkItems from old revision
cancelled/superseded as policy requires
```

---

# 108. OCR/Recognition Example

```text
Recognition WorkItem
    ↓
Attempt 1
    ↓
provider timeout
    ↓
Attempt 1 FAILED
    ↓
Runtime retry policy
    ↓
Attempt 2
    ↓
Candidate RecognitionArtifact
    ↓
authority validation
    ↓
Published RecognitionArtifact
```

Recognition module does not enter a global `RETRY_WAIT` state.

---

# 109. Translation Example

```text
Translation WorkItem
    ↓
Attempt A
    ↓
Candidate TranslationArtifact
    ↓
RuntimeRevision no longer current
    ↓
Candidate rejected as superseded
```

The old Translation result must not overwrite current published state.

---

# 110. Presentation Example

```text
Published TranslationArtifact
    ↓
Presentation WorkItem becomes READY
    ↓
Candidate PresentationArtifact
    ↓
validation
    ↓
Published PresentationArtifact
```

No direct `TranslationCompleted → Rendering` command chain is required.

---

# 111. Pause Example

```text
Reading Session ACTIVE
    ↓
Pause committed
    ↓
Reading Session PAUSED
```

Then separately:

```text
Application/Runtime
    ↓
cancel/suppress relevant Runtime work
```

Runtime commits actual WorkItem/Attempt transitions.

---

# 112. Resume Example

```text
Reading Session PAUSED
    ↓
validate current source/context
    ↓
ACTIVE
    ↓
plan work from current ReadingContextRevision
```

Do not restart stale Attempts.

---

# 113. Shutdown Example

```text
Application READY
    ↓
STOPPING
```

Coordinates:

```text
Reading Sessions → STOPPING
Runtime → cancellation
UI Adapter → STOPPING
Diagnostics → STOPPING
Infrastructure → bounded flush
```

Then:

```text
Application → STOPPED
```

---

# 114. Invalid Architecture Patterns

The following are prohibited as current architecture:

```text
session state = OCR_PROCESSING

session state = TRANSLATING

single processing pipeline lifecycle owns all stages

pipelineId as universal execution identity

taskId as universal async identity

module-owned retry loop controlling downstream execution

module-owned Scheduler admission

module completion event directly starts next module

Candidate creation automatically publishes current result

global StateTransitionService owns all module states

Event Bus controls state by hidden commands
```

---

# 115. Deprecated v1 Concepts

The following concepts from STATE_MACHINE v1 are deprecated as architecture authorities:

```text
Processing Pipeline State Machine
PIPELINE_CREATED
WAITING_FOR_STABILITY
ACQUIRING_CONTENT
NORMALIZING
FINGERPRINTING
CACHE_LOOKUP
TEXT_EXTRACTING
OCR_PROCESSING
SEGMENTING
TRANSLATING
POST_PROCESSING
PREPARING_RENDER
RENDERING
RETRY_WAIT
PIPELINE_ERROR
pipelineId as universal authority
taskId as universal task identity
contentRevision as universal revision
```

Some names may still appear as local operational labels.

They must not be treated as the global CRAI execution state model.

---

# 116. Preserved v1 Principles

The following v1 goals remain valid:

```text
prevent stale results
avoid duplicate expensive work
bound retries
support cancellation
avoid uncontrolled concurrency
avoid infinite provider waits
limit resource usage
avoid logging sensitive content
support crash recovery
```

Only ownership and modeling changed.

---

# 117. State Persistence Rules

Persist:

```text
durable domain/user authority
```

Do not persist solely because something is an internal runtime state.

Owner-specific persistence rules belong to each module/infrastructure contract.

---

# 118. Runtime Persistence

If CRAI later supports durable Runtime queues:

```text
WorkItem/Attempt persistence
```

must be explicitly designed in Runtime architecture.

It must not be implied by this generic state-machine document.

---

# 119. Open Questions Belong to Owners

Questions such as:

```text
stability debounce duration
provider fallback ordering
retry attempts
concurrency limits
pause cancellation policy
```

belong to:

```text
Capture / OCR architecture
Provider Management
Runtime Retry Policy
Runtime Configuration
Reading Session
Scheduler
```

This document should not duplicate their decision authority.

---

# 120. MVP State Model

Recommended MVP architecture:

```text
Application:
UNINITIALIZED
→ INITIALIZING
→ READY / DEGRADED
→ STOPPING
→ STOPPED

Reading Session:
owner-defined lifecycle from reading-session/STATES.md

Runtime:
RuntimeRevision
WorkItem
Attempt

Artifacts:
Candidate
→ authority validation
→ Published

Modules:
small lifecycle + scoped operations

UI:
immutable projections
```

---

# 121. MVP Concurrency

MVP may configure conservative limits such as:

```text
one active Reading Session
limited Recognition concurrency
limited Translation concurrency
one overlay frontend
```

These are configuration decisions.

They are not core state-machine invariants.

---

# 122. Testing — Transition Ownership

Verify no module can directly mutate another owner's state.

---

# 123. Testing — Stale Result

Scenario:

```text
RuntimeRevision A starts
RuntimeRevision B supersedes A
Attempt from A completes late
```

Verify:

```text
Candidate from A
is never published as current state.
```

---

# 124. Testing — Retry

Verify:

```text
Attempt 1 FAILED
    ↓
Attempt 2
```

without rewinding a global pipeline state.

---

# 125. Testing — Cancellation

Verify cancellation request plus late provider completion cannot publish stale results.

---

# 126. Testing — Concurrent Work

Verify multiple WorkItems/Attempts may coexist without requiring one global processing state.

---

# 127. Testing — Event Ordering

Verify state commits before corresponding events are published.

---

# 128. Testing — Event Failure

Inject Event Bus publication failure after state commit.

Verify committed state remains authoritative.

---

# 129. Testing — Module Degradation

Disable one optional capability.

Verify:

```text
module/application can remain usable
```

when policy allows.

---

# 130. Testing — Crash Recovery

Simulate process restart while Attempts were RUNNING.

Verify old Attempts are not blindly resumed as live execution.

---

# 131. Testing — Artifact Publication

Verify Candidate Artifact cannot become Published without authority validation.

---

# 132. Testing — Typed Revision Safety

Verify code/contracts do not interchange:

```text
ReadingContextRevision
PreferenceRevision
RuntimeRevisionId
ViewModelRevision
PresentationRevision
```

as generic integers/strings without type meaning.

---

# 133. Related Documents

```text
doc/01-architecture/core/
├── STATE_MACHINE.md
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
├── DATA_FLOW.md
├── CAPABILITY_MAP.md
└── README.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── CANCELLATION.md
├── RETRY_POLICY.md
├── SCHEDULER.md
├── WORK_QUEUE.md
├── RUNTIME_COMPONENTS.md
└── RUNTIME_OBSERVABILITY.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
└── OWNERSHIP_MAP.md

doc/02-modules/
├── reading-session/
├── capture/
├── recognition/
├── text-processing/
├── translation/
├── presentation/
├── preferences/
├── diagnostics/
└── ui-adapter/
```

---

# 134. Documentation Authority

This document defines:

```text
architecture-wide state ownership rules
state-domain separation
Runtime vs domain state boundary
transition principles
revision semantics
Candidate/Published state boundary
stale-result rules
event publication relationship
```

It does not replace module-specific `STATES.md`.

---

# 135. Completion Criteria

This document is synchronized when:

* there is no global stage-based Processing Pipeline State Machine;
* Reading Session does not own OCR/Translation/Rendering execution state;
* `pipelineId` is not used as universal execution authority;
* `taskId` is replaced by typed Runtime identities;
* `contentRevision` is not used as a generic architecture-wide revision;
* RuntimeRevision/WorkItem/Attempt ownership is explicit;
* retry belongs to Runtime execution;
* cancellation belongs to Runtime execution authority;
* provider fallback does not become a global state machine;
* Candidate Artifact is separated from Published Artifact;
* stale-result validation occurs before publication;
* module lifecycle is separated from scoped operation state;
* events publish only after committed state;
* Event Bus is not hidden command transport;
* no global StateTransitionService ownership is required;
* crash recovery does not resume stale in-memory Attempts;
* concurrency is allowed without one global processing state;
* module-specific state documents remain authoritative for their own domains.

---

# 136. Summary

CRAI v1 modeled execution as:

```text
Reading Session
    ↓
Processing Pipeline
    ↓
Capture
    ↓
OCR
    ↓
Translation
    ↓
Render
```

CRAI Runtime v2 uses:

```text
Domain Authority
    ↓
ReadingContextRevision
    ↓
Business Pipeline Orchestration
    ↓
RuntimeRevision
    ↓
WorkItems
    ↓
Attempts
    ↓
Candidate Artifacts
    ↓
Authority Validation
    ↓
Published Artifacts
```

The central invariant is:

```text
Domain state describes
what the application means.

Runtime state describes
what work is executing.

Module state describes
whether a module is usable.

Scoped operation state describes
one bounded operation.

These state domains
must never be merged.
```
