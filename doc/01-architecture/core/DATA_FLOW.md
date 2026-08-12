# CRAI Data Flow Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/core/DATA_FLOW.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines how authoritative data moves through CRAI from a reading source to user-visible translated presentation.

It describes:

```text
data ownership
data representations
Artifact boundaries
Runtime execution references
Candidate and Published result flow
image-based reading flow
structured-text reading flow
translation preparation
presentation flow
stale-result protection
cache interaction
storage/retention
provider boundaries
privacy
correlation
```

It does not define:

```text
complete Runtime state machines
module implementation APIs
provider-native schemas
database schemas
Event Bus implementation
native UI rendering
```

Those belong to their owning documents.

---

# 2. Central Data-Flow Rule

CRAI v2 uses:

```text
Domain Authority
    ↓
Runtime Planning
    ↓
Execution
    ↓
Candidate Result
    ↓
Authority Validation
    ↓
Published Artifact
    ↓
Next Consumer / Projection
```

A result does not become authoritative merely because computation completed.

---

# 3. Authority Before Movement

Every important data movement must answer:

```text
Who owns this data?

What authority/revision produced it?

Is it Candidate or Published?

Which consumer contract may accept it?
```

Data movement without ownership is prohibited.

---

# 4. High-Level Architecture

The primary flow is:

```text
Reading Source
    ↓
Reading Session / Application Context
    ↓
ReadingContextRevision
    ↓
Business Execution Planning
    ↓
RuntimeRevision
    ↓
WorkItems / Attempts
    ↓
Module Candidate Artifacts
    ↓
Authority Validation
    ↓
Published Artifacts
    ↓
Presentation
    ↓
UI Adapter Projection
    ↓
Native UI
```

---

# 5. Business Data vs Runtime Data

CRAI separates:

```text
Business / Domain Data
```

from:

```text
Runtime Execution Data
```

Business data describes:

```text
what content means
what source is selected
what configuration is authoritative
what Artifact was published
```

Runtime data describes:

```text
what work should execute
what Attempt is running
retry
deadline
queueing
cancellation
supersession
```

---

# 6. No Global Mutable Pipeline Context

Do not use:

```text
GlobalProcessingContext
├── frame
├── OCR result
├── Translation result
├── UI state
├── retry state
├── cancellation state
├── credentials
└── cache
```

Instead, use explicit immutable contracts and references.

---

# 7. Core Data Flow Goals

CRAI data flow must provide:

1. minimal reading interruption;
2. best available input representation;
3. explicit ownership;
4. end-to-end traceability;
5. stale-result rejection;
6. bounded execution;
7. Presentation independence;
8. provider isolation;
9. privacy-safe retention;
10. cache correctness.

---

# 8. Prefer Highest-Quality Source Representation

Preferred order:

```text
Structured webpage/document data
        ↓
Extracted semantic text
        ↓
Imported text
        ↓
Image with known structure
        ↓
Raw image
        ↓
Screen capture
```

OCR should not be used when reliable structured text is already available.

---

# 9. Source Quality Does Not Transfer Ownership

Whether content came from:

```text
DOM
screen
clipboard
image file
document
```

does not change ownership of downstream Artifacts.

All source integrations normalize into CRAI contracts before processing.

---

# 10. Main Data Layers

CRAI separates data into:

```text
External Data
    ↓
Normalized Source/Input Data
    ↓
Published Processing Artifacts
    ↓
Translation Data
    ↓
Presentation Artifacts
    ↓
UI Projections
```

Runtime metadata flows alongside these layers rather than becoming their semantic contents.

---

# 11. External Data

Examples:

```text
browser DOM
screen pixels
window surface
clipboard
imported file
provider response
```

External data is untrusted.

It must pass adapter/provider normalization before entering stable core contracts.

---

# 12. Reading Context

Reading Session owns the current reading context.

Conceptually:

```text
ReadingContext
├── source
├── sourceMode
├── selectedArea?
├── language settings
├── session configuration
└── other session-owned context
```

A committed context change creates:

```text
ReadingContextRevision
```

according to Reading Session contracts.

---

# 13. ReadingContextRevision

`ReadingContextRevision` identifies domain/session authority.

Examples that may create a new revision:

```text
selected source changed
capture region changed
session-specific language changed
relevant session configuration changed
```

It is not an execution Attempt identity.

---

# 14. Removed Generic `SourceRevisionId` Authority

v1 used:

```text
SourceRevisionId
```

as a universal parent for almost all downstream processing.

v2 uses the actual typed owner authority.

For continuously changing source content, Capture/source-observation may still expose source/capture revision concepts if defined by that module.

They must not replace:

```text
ReadingContextRevision
RuntimeRevisionId
ArtifactId
```

with one generic revision hierarchy.

---

# 15. RuntimeRevision

Business/Application planning converts authoritative context into Runtime execution authority.

Conceptually:

```text
ReadingContextRevision
+
resolved configuration
+
business execution requirements
        ↓
RuntimeRevision
```

Runtime owns:

```text
RuntimeRevisionId
```

---

# 16. RuntimeRevision Is Not Data Content

A RuntimeRevision describes execution authority.

It should not contain all processing data as one mutable object.

WorkItems refer to immutable inputs/Artifacts.

---

# 17. WorkItem

Runtime creates schedulable logical work:

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

---

# 18. Attempt

Each concrete execution is represented by:

```text
Attempt
├── AttemptId
├── WorkItemId
├── provider/config selection
├── deadline
├── execution state
└── diagnostics context
```

Retry creates another Attempt.

---

# 19. Removed `ProcessingAttemptId`

The generic v1:

```text
ProcessingAttemptId
```

is replaced by:

```text
WorkItemId
AttemptId
```

owned by Runtime.

Provider-level request IDs may exist additionally inside module/provider adapters.

---

# 20. No `ProcessingEnvelope`

The v1 `ProcessingEnvelope` combining:

```text
sessionId
sourceId
revisionId
attemptId
operationType
priority
deadline
cancellationToken
```

is removed as a core data contract.

These concerns now belong to:

```text
RuntimeRevision
WorkItem
Attempt
Runtime cancellation/deadline contracts
```

---

# 21. Candidate Artifact

A Candidate Artifact is output produced by execution but not yet accepted as current authoritative output.

Examples:

```text
Candidate Capture Artifact
Candidate Recognition Artifact
Candidate SourceDocument Artifact
Candidate Translation Artifact
Candidate Presentation Artifact
```

---

# 22. Candidate Does Not Mean Published

Execution:

```text
Attempt SUCCEEDED
```

does not automatically mean:

```text
Artifact Published
```

A Candidate must still pass authority and contract validation.

---

# 23. Published Artifact

A Published Artifact is:

```text
validated
immutable
owner-approved
provenance-linked
safe for downstream consumption
```

Published Artifacts become stable module boundaries.

---

# 24. Publication Flow

Canonical:

```text
WorkItem
    ↓
Attempt
    ↓
Candidate Artifact
    ↓
validate structure
    ↓
validate provenance
    ↓
validate current Runtime authority
    ↓
publish
    ↓
Published Artifact
```

---

# 25. Artifact Provenance

Each Published Artifact should preserve enough provenance to answer:

```text
which input produced it?
which RuntimeRevision accepted it?
which configuration mattered?
which module/provider produced it?
which Artifact(s) preceded it?
```

---

# 26. Artifact Identity

Use:

```text
ArtifactId
```

for canonical Artifact identity.

Artifact-specific revisions/versions may exist according to owner contracts.

---

# 27. Core Artifact Flow — Image Source

Architecture-level flow:

```text
ReadingContext
    ↓
Capture
    ↓
Capture Artifact
    ↓
Recognition
    ↓
RecognitionArtifact
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
    ↓
PresentationArtifact
    ↓
Application / UI Adapter
    ↓
ViewModel
```

---

# 28. Core Artifact Flow — Structured Text Source

When structured text is reliable:

```text
Structured Source
    ↓
normalized source/input boundary
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
```

Recognition may be skipped entirely.

---

# 29. Core Artifact Flow — Manual Text

```text
Manual Text Input
    ↓
input validation
    ↓
normalized source input
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
```

---

# 30. Core Artifact Flow — Manual Image

```text
Image File / Clipboard Image
    ↓
Capture/import normalization
    ↓
Capture Artifact
    ↓
Recognition
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
```

Continuous source observation is not required.

---

# 31. Capture Ownership

Capture owns source acquisition/capture semantics.

Possible Capture-owned concepts include:

```text
CaptureSource
Capture Candidate
Capture Artifact
geometry
pixel representation
capture metadata
```

Exact contracts belong to `02-modules/capture/`.

---

# 32. Capture Artifact

A Capture Artifact may represent:

```text
accepted visual content
source geometry
coordinate space
capture provenance
bounded metadata
```

It must not expose platform-native capture handles downstream.

---

# 33. Capture Candidate Stability

For continuous screen reading:

```text
raw observations
    ↓
change detection
    ↓
stability policy
    ↓
candidate
    ↓
acceptance
```

Not every captured frame becomes a Published Artifact.

---

# 34. Continuous Observation

Continuous observation may generate many temporary candidates.

These should remain:

```text
bounded
short-lived
discardable
```

until Capture/source policy determines meaningful content.

---

# 35. Duplicate Detection

Potential strategies:

```text
exact hash
perceptual hash
region-aware similarity
semantic text fingerprint
```

Duplicate detection is an optimization/policy decision.

It must not bypass Artifact compatibility/authority validation.

---

# 36. Recognition Ownership

Recognition consumes its accepted input and owns Recognition semantics such as:

```text
recognized blocks
recognized lines
recognized tokens
geometry
confidence
text direction
reading hints
provider provenance
```

---

# 37. RecognitionArtifact

Architecture-level representation:

```text
RecognitionArtifact
├── artifactId
├── sourceArtifactRef
├── regions/blocks
├── recognized text
├── geometry
├── reading hints
├── confidence
├── provenance
└── warnings
```

Exact schema belongs to Recognition contracts.

---

# 38. Removed `OcrResult` as Cross-Module Core Type

v1 used:

```text
OcrResult
```

as a core architecture data representation.

v2 uses:

```text
RecognitionArtifact
```

as the stable Recognition → Text Processing boundary.

Provider/OCR-specific results remain internal to Recognition/provider adapters.

---

# 39. Recognition Provider Data

Provider-native response may contain:

```text
provider tokens
provider bounding boxes
provider confidence
provider metadata
```

Recognition normalizes these before publication.

Provider-native structures must not become downstream contracts.

---

# 40. Text Processing Ownership

Text Processing consumes:

```text
RecognitionArtifact
```

or normalized structured-text source input.

It owns:

```text
normalization
structural reconstruction
reading-order normalization
grouping
semantic segmentation
source-document preparation
```

---

# 41. SourceDocument Candidate

Text Processing may produce:

```text
Candidate SourceDocument
```

containing normalized semantic text structure.

---

# 42. SourceDocumentArtifact

After validation/publication:

```text
SourceDocumentArtifact
```

is the stable Text Processing → Translation boundary.

---

# 43. SourceDocument Structure

Conceptually:

```text
SourceDocumentArtifact
├── artifactId
├── sourceRefs
├── language
├── blocks[]
├── semantic order
├── geometry refs?
├── source provenance
├── confidence/warnings
└── normalization metadata
```

Exact schema remains Text Processing-owned.

---

# 44. Removed Generic `SourceSegment` Authority

v1 used `SourceSegment` as a cross-pipeline universal object.

v2 may still have text-processing segment/block types.

But their identity and meaning belong to:

```text
Text Processing
```

not to a global architecture hierarchy.

Translation must consume the published SourceDocument contract, not generic mutable segments.

---

# 45. Translation Ownership

Translation owns:

```text
TranslationUnit construction
TranslationBatch planning
translation context assembly
provider request semantics
provider response normalization
source-target alignment
Translation Artifact
```

---

# 46. TranslationUnit Ownership

`TranslationUnit` belongs to Translation.

It must not be created as a Text Processing output.

Flow:

```text
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationUnit / TranslationBatch
```

---

# 47. Translation Unit

Conceptually:

```text
TranslationUnit
├── translationUnitId
├── sourceDocumentRef
├── sourceBlockRefs[]
├── sourceLanguage
├── targetLanguage
├── sourceText
├── alignment metadata
├── context
├── glossary snapshot
├── style/config snapshot
└── constraints
```

---

# 48. Translation Context

Context is deliberately constructed.

Possible inputs:

```text
nearby SourceDocument blocks
chapter/document metadata
recent accepted Translation Artifacts
glossary
terminology
style configuration
```

Do not send the whole reading history automatically.

---

# 49. Context Provenance

Context items should preserve:

```text
source
reason for inclusion
privacy classification
retention scope
remote-send eligibility
```

where relevant.

---

# 50. Glossary Snapshot

A translation execution should use immutable glossary/configuration snapshots.

Example:

```text
current glossary
    ↓
relevant-term selection
    ↓
GlossarySnapshot
    ↓
Translation Attempt
```

---

# 51. Glossary Change During Execution

If glossary changes while an Attempt runs:

```text
Attempt may finish using old snapshot
```

but Candidate provenance records the old snapshot.

New authority may decide the Candidate is:

```text
acceptable
stale
or requires retranslation
```

according to policy.

---

# 52. Translation Provider Boundary

```text
Translation canonical input
    ↓
Provider Adapter
    ↓
data minimization
    ↓
credentials attached internally
    ↓
remote/local provider
    ↓
provider-native response
    ↓
validation
    ↓
canonical Translation result
```

---

# 53. Credentials

Credentials must remain inside:

```text
Secret Management
Provider Management / Adapter
```

They must never appear in:

```text
TranslationUnit
Artifact
Event
ViewModel
cache key
diagnostic bundle
```

---

# 54. Translation Candidate

Provider completion creates a Translation execution result.

Translation normalizes/alignment-validates it into:

```text
Candidate TranslationArtifact
```

---

# 55. TranslationArtifact

After authority validation/publication:

```text
TranslationArtifact
```

contains semantic translation output independent from UI mode.

It remains valid whether UI shows:

```text
side panel
overlay
reader
export preview
```

---

# 56. Translation Alignment

Source-target alignment must use explicit identity/markers.

Do not rely solely on array ordering when provider responses may:

```text
merge
split
omit
reorder
```

content.

---

# 57. Translation Correction

Provider output and user-corrected output must remain distinguishable.

Conceptually:

```text
Published provider translation
    +
Correction Record
    ↓
effective user-facing translation projection
```

Do not silently mutate historical provider output.

---

# 58. Presentation Ownership

Presentation consumes accepted semantic translation results.

It owns:

```text
presentation layout
semantic display structure
geometry mapping
text fitting
Presentation Artifact
Presentation Revision
```

---

# 59. PresentationArtifact

Conceptually:

```text
PresentationArtifact
├── artifactId
├── translationArtifactRef
├── mode-independent semantic presentation
├── layout
├── geometry
├── fitting decisions
├── warnings
└── provenance
```

Exact contract belongs to Presentation.

---

# 60. Presentation Is Not UI Rendering

Presentation may define what should be displayed.

UI Adapter/native UI defines how it is rendered on a platform.

Flow:

```text
PresentationArtifact
    ↓
Application projection
    ↓
UI Adapter
    ↓
ViewModel
    ↓
Native Renderer
```

---

# 61. UI Adapter Projection

UI Adapter creates disposable immutable ViewModels.

ViewModels are not processing Artifacts and are not domain authority.

---

# 62. ViewModel Flow

```text
Application/Module snapshots
        ↓
UI Adapter
        ↓
Candidate ViewModel
        ↓
validation
        ↓
Published local ViewModel
```

A ViewModel may be rebuilt without re-running Translation.

---

# 63. Geometry Flow

Visual geometry should remain traceable through processing.

Conceptually:

```text
Capture coordinate space
    ↓
Recognition geometry
    ↓
SourceDocument geometry refs
    ↓
Presentation transforms
    ↓
UI coordinates
```

---

# 64. Coordinate Spaces

Examples:

```text
captured-frame
source-image
screen
application-window
browser-viewport
presentation-space
display-space
```

Transforms must be explicit.

---

# 65. Geometry Failure Independence

If overlay geometry becomes invalid:

```text
Presentation overlay
    → degraded/suspended
```

while:

```text
TranslationArtifact
    → remains valid
```

Do not rerun Recognition/Translation merely because a display transform changed.

---

# 66. Runtime Correlation

Every execution Candidate should be traceable to Runtime authority.

Relevant references may include:

```text
RuntimeRevisionId
WorkItemId
AttemptId
```

These do not become semantic content ownership.

---

# 67. Candidate Acceptance

Canonical validation:

```text
Candidate received
    ↓
Artifact structurally valid?
    ├── No → reject
    └── Yes
         ↓
RuntimeRevision still authoritative?
    ├── No → stale/superseded
    └── Yes
         ↓
WorkItem/Attempt result accepted?
    ├── No → reject
    └── Yes
         ↓
input/provenance compatible?
    ├── No → reject
    └── Yes
         ↓
publish Artifact
```

---

# 68. Stale Result

A result is stale when it belongs to legitimate historical execution but no longer has authority to become current published state.

Examples:

```text
new RuntimeRevision superseded old work
ReadingContext changed
manual retranslation superseded auto translation
new provider Attempt already won
session stopped
```

---

# 69. Stale Is Not Failure

Staleness/supersession may be expected control flow.

Do not report every stale Candidate as a user-visible error.

---

# 70. Late Provider Result

Remote execution may return after cancellation.

Therefore:

```text
cancellation
```

never replaces:

```text
authority validation
```

---

# 71. Cancellation

Cancellation authority belongs to Runtime.

Possible origin:

```text
session stopped
ReadingContextRevision changed
RuntimeRevision superseded
user cancelled
deadline
shutdown
resource policy
```

---

# 72. Cancellation Flow

```text
Application/owner condition
    ↓
Runtime cancellation request
    ↓
WorkItem/Attempt authority changes
    ↓
provider cancellation attempted where supported
    ↓
late Candidate still validated before publication
```

---

# 73. Removed “Cancellation Flows Down the Pipeline”

Do not model:

```text
Session
    ↓
Capture cancel
    ↓
OCR cancel
    ↓
Translation cancel
```

as module-to-module propagation.

Runtime owns execution cancellation across affected work.

---

# 74. Retry

Retry belongs to Runtime execution.

Flow:

```text
Attempt 1
    ↓
FAILED / TIMED_OUT
    ↓
Runtime retry policy
    ↓
Attempt 2
```

No data Artifact is mutated backward.

---

# 75. Provider Fallback

Fallback may cause a new Attempt with another provider/configuration.

Both Attempts retain separate identities/provenance.

Only the accepted Candidate may publish current output.

---

# 76. Backpressure

Runtime/Scheduler/Resource Manager own processing backpressure.

Potential policies:

```text
latest-authority preference
bounded pending WorkItems
visible-content priority
manual request priority
provider concurrency limits
memory/GPU limits
prefetch deprioritization
```

---

# 77. No Global “One Pending Revision” Invariant

MVP may configure:

```text
one latest pending visual work item
```

but this is Runtime policy.

It is not a universal data-model rule.

---

# 78. Concurrency

CRAI permits concurrent WorkItems.

Example:

```text
Capture WorkItem A
Recognition WorkItem B
Translation WorkItem C
Presentation Artifact N still visible
```

Data correctness comes from provenance and authority, not serialized global stages.

---

# 79. Structured Text Flow

Preferred structured-text flow:

```text
Browser / Document Adapter
    ↓
sanitized structured source
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
```

OCR is skipped.

---

# 80. Browser Boundary

Browser integration may provide:

```text
chapter/body content
headings
paragraphs
dialogue blocks
safe source locators
safe metadata
```

Do not expose:

```text
DOM nodes
browser objects
scripts
unrestricted HTML
```

as stable core types.

---

# 81. Content Isolation

Browser/document adapters should remove unrelated content such as:

```text
navigation
ads
comments
menus
recommendations
hidden duplicate layouts
```

before stable source processing where possible.

---

# 82. Text Normalization

Text Processing may handle:

```text
Unicode normalization
whitespace cleanup
paragraph preservation
dialogue preservation
punctuation normalization
structure reconstruction
language hints
```

Exact responsibilities remain Text Processing-owned.

---

# 83. Long Text

Long chapters should be processed incrementally.

Translation owns TranslationUnit/Batch construction according to:

```text
semantic boundaries
provider limits
context requirements
latency targets
```

---

# 84. Incremental Translation

Different Translation Units may complete independently.

They may produce:

```text
partial Translation Artifacts/projections
```

only if Translation/Presentation contracts explicitly support completeness metadata.

---

# 85. Partial Results

Partial success must be explicit.

Possible completeness:

```text
Complete
Partial
Incomplete
Degraded
```

or owner-defined equivalent.

Partial output must never appear indistinguishable from complete output.

---

# 86. Presentation Incrementality

Presentation may expose incremental accepted results without making UI Adapter infer Translation execution state.

Flow:

```text
accepted Translation output
    ↓
Presentation Artifact N
    ↓
later accepted output
    ↓
Presentation Artifact N+1
```

---

# 87. Cache Role

Cache is an optimization.

It is not authority.

---

# 88. Cache Flow

```text
WorkItem input
    ↓
cache lookup
    ↓
compatible entry?
    ├── No → execute
    └── Yes
         ↓
      Candidate cached Artifact
         ↓
      compatibility + authority validation
         ↓
      publish/reuse
```

---

# 89. Cache Hit Must Be Revalidated

A cache hit must still match relevant:

```text
input fingerprint
language configuration
model/provider compatibility
normalization version
context fingerprint
glossary snapshot
style/configuration
privacy scope
current Runtime authority
```

---

# 90. Recognition Cache

Possible key inputs:

```text
image/region fingerprint
provider/model class
language configuration
detection configuration
preprocessing version
recognition normalization version
```

Exact cache ownership belongs to cache/module architecture.

---

# 91. Translation Cache

Possible key inputs:

```text
normalized source content
source language
target language
context fingerprint
glossary fingerprint
style/configuration
provider/model policy
prompt/adapter version
```

Source text alone is insufficient for context-sensitive reuse.

---

# 92. Cache Scope

Possible policies:

```text
Attempt-local
RuntimeRevision-local
Reading Session
local user
series/work
global-local
```

Privacy policy constrains reuse.

---

# 93. Storage Lifetimes

Architecture recognizes:

```text
Operation
Runtime/Revision
Session
Cache
Persistent
Diagnostic
```

Different data types need different retention.

---

# 94. Raw Visual Content

Default:

```text
memory first
short retention
no normal persistence
no logs
release obsolete buffers quickly
```

Persistent screenshots require explicit policy/user action.

---

# 95. Source Text Retention

Source text may support:

```text
alignment
correction
retranslation
context
cache
recovery
```

Persistence depends on privacy policy.

---

# 96. Persistent Knowledge

Potential persistent user data:

```text
preferences
glossary
accepted terminology
style settings
source profiles
optional reading history
explicit corrections
```

Keep it separate from transient captured content.

---

# 97. Provider Boundary

All external provider processing follows:

```text
canonical CRAI input
    ↓
Provider Adapter
    ↓
validation/minimization
    ↓
credential attachment
    ↓
provider
    ↓
provider-native result
    ↓
normalization
    ↓
module Candidate
```

---

# 98. Provider Response Is Not Artifact

A provider response must be checked for:

```text
schema
size
alignment
missing data
duplicate data
rate limits
timeouts
content filtering
malformed output
```

Only normalized validated module output may become Candidate Artifact.

---

# 99. Remote Data Minimization

Send only required information.

Examples:

```text
region crop instead of whole screen
selected chapter content instead of whole webpage
relevant glossary entries only
bounded context
safe metadata only
```

---

# 100. Event Bus Relationship

Event Bus reports committed facts.

It does not move data through the execution pipeline by request events.

---

# 101. Removed v1 Event-Driven Pipeline

Deprecated:

```text
source.revision.accepted
    ↓
extraction.requested
    ↓
extraction.completed
    ↓
segments.prepared
    ↓
translation.requested
    ↓
translation.completed
    ↓
presentation.updated
```

Execution does not use Event Bus stage chaining.

---

# 102. Runtime-Driven Movement

Preferred:

```text
BusinessExecutionPlan
    ↓
Runtime WorkItems
    ↓
dependencies satisfied
    ↓
Attempts
    ↓
Artifacts
```

Events may notify consumers after publication.

---

# 103. Artifact Event Example

```text
TranslationArtifact published
    ↓
TranslationArtifactPublished
    ↓
Event Bus
```

The event does not itself create Presentation execution authority.

---

# 104. State Machine Relationship

State machine answers:

```text
who owns current state and authority?
```

Data flow answers:

```text
what data exists and how does it cross boundaries?
```

Runtime state and module Artifact data must remain separate.

---

# 105. No Global `TRANSLATING` Data Flow State

A TranslationUnit existing does not imply:

```text
application state = TRANSLATING
```

A Translation Attempt may run while other Runtime work also exists.

---

# 106. Ownership Table

Architecture-level ownership:

| Data                                    | Owner                                 |
| --------------------------------------- | ------------------------------------- |
| ReadingContext / ReadingContextRevision | Reading Session                       |
| RuntimeRevision                         | Runtime                               |
| WorkItem                                | Runtime                               |
| Attempt                                 | Runtime                               |
| Capture Artifact                        | Capture                               |
| RecognitionArtifact                     | Recognition                           |
| SourceDocumentArtifact                  | Text Processing                       |
| TranslationUnit / TranslationBatch      | Translation                           |
| TranslationArtifact                     | Translation                           |
| PresentationArtifact                    | Presentation                          |
| ViewModel                               | UI Adapter                            |
| Diagnostic Observation/Snapshot         | Diagnostics                           |
| Credentials                             | Secret Management / provider boundary |
| Physical cache/storage representation   | Infrastructure                        |

---

# 107. No “Translation Orchestration” Pseudo-Owner

v1 used conceptual owners such as:

```text
Translation orchestration
Presentation orchestration
Text understanding
```

v2 uses actual module ownership:

```text
translation
presentation
text-processing
```

---

# 108. Mutation Rules

Published canonical Artifacts are immutable.

Corrections create:

```text
new correction record
new revision
or new Artifact
```

according to owner semantics.

---

# 109. Correction Provenance

Always preserve:

```text
original provider/module result
+
user correction
```

as distinguishable data.

---

# 110. Error Data Flow

Errors retain original module ownership.

Examples:

```text
CAP-*
REC-*
TXT-*
TRN-*
PRES-*
RUN-*
SES-*
```

UI/Diagnostics may project/observe them without replacing ownership.

---

# 111. Attempt Error vs Artifact Error

Example:

```text
Provider timeout
    ↓
AttemptTimedOut
```

does not necessarily mean:

```text
Translation module unavailable
```

and does not necessarily publish a failed Translation Artifact.

---

# 112. Partial Module Output

A module may publish partial output only if its contract explicitly defines:

```text
completeness
missing sections
warnings
provenance
```

---

# 113. Diagnostics Data Flow

Diagnostics observes:

```text
Runtime identity
module operation
Artifact publication
errors
duration
queue delay
retry
cancellation
capability health
```

through observability abstractions.

---

# 114. Diagnostics Does Not Own Data Flow

Diagnostics may correlate:

```text
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
```

but does not own or mutate them.

---

# 115. Diagnostic Privacy

Normal diagnostics exclude:

```text
raw screenshots
full OCR text
full source chapter
full translated content
provider prompts/responses
credentials
clipboard contents
```

unless an explicit support/debug contract permits otherwise.

---

# 116. Product Performance Flow

Important end-to-end measurement:

```text
Readable source change
    ↓
authoritative source/context accepted
    ↓
Runtime processing
    ↓
Published Translation/Presentation
    ↓
useful Vietnamese content visible
```

This remains a primary product metric.

---

# 117. Presentation Update Strategy

CRAI may support:

```text
atomic presentation
incremental presentation
```

depending on content type and latency.

---

# 118. Atomic Presentation

Useful for:

```text
short comic page
small contextual batch
manual short input
```

when stability matters more than earliest partial output.

---

# 119. Incremental Presentation

Useful for:

```text
long chapter
large document
slow provider
streaming Translation
```

Each published increment must preserve provenance/completeness.

---

# 120. UI Must Not Reorder Authoritative Meaning

UI may render incrementally but must not reinterpret:

```text
source ordering
translation alignment
presentation semantics
```

defined by owner Artifacts.

---

# 121. Security Boundaries

Important boundaries:

```text
OS capture
browser connector
file import
IPC/process boundary
provider network
persistent storage
clipboard
UI/system notification
export
```

---

# 122. Boundary Requirements

At each security boundary define:

```text
accepted types
size limits
validation
sanitization
authentication
privacy classification
timeout
cancellation
logging restrictions
```

---

# 123. Initial MVP — Screen Reading

Recommended logical MVP:

```text
User selects region/window
    ↓
Reading Session commits context
    ↓
Capture observes source
    ↓
stable Capture Artifact published
    ↓
RecognitionArtifact published
    ↓
SourceDocumentArtifact published
    ↓
Translation creates Units/Batches
    ↓
TranslationArtifact published
    ↓
PresentationArtifact published
    ↓
UI Adapter builds Reader ViewModel
    ↓
user sees Vietnamese translation
```

---

# 124. Runtime View of Same MVP

Execution perspective:

```text
RuntimeRevision
    ↓
Capture WorkItem / Attempt
    ↓
Recognition WorkItem / Attempt
    ↓
Text Processing WorkItem / Attempt
    ↓
Translation WorkItem / Attempt
    ↓
Presentation WorkItem / operation
```

Dependencies may allow overlap.

---

# 125. Source Change During Translation

Example:

```text
ReadingContextRevision 10
    ↓
RuntimeRevision A
    ↓
Translation Attempt running

User scrolls / source changes
    ↓
ReadingContextRevision 11
    ↓
RuntimeRevision B
    ↓
A superseded/cancelled
```

Late Candidate from A:

```text
arrives
    ↓
authority check fails
    ↓
not published
```

---

# 126. Cache + Glossary Example

```text
SourceDocumentArtifact
    ↓
Translation builds cache identity
    ↓
matching source exists
    ↓
glossary fingerprint differs
    ↓
cache entry incompatible
    ↓
new Attempt
```

---

# 127. Overlay Misalignment Example

```text
PresentationArtifact valid
    ↓
window/zoom changes
    ↓
overlay geometry invalid
    ↓
overlay suspended/rebuilt
```

TranslationArtifact remains valid.

---

# 128. Remote Provider Failure Example

```text
Translation Attempt 1
    ↓
remote timeout
    ↓
Attempt 1 TIMED_OUT
    ↓
Runtime/provider policy
    ↓
Attempt 2
```

No duplicate visible result is published.

---

# 129. Manual Correction Example

```text
Published TranslationArtifact
    ↓
user correction
    ↓
Correction Record
    ↓
Presentation/Application projection updates
```

Optional accepted terminology may update persistent glossary through its owner.

---

# 130. Core Data Invariants

1. Every canonical data type has one owner.

2. Runtime identities do not replace domain identities.

3. ReadingContextRevision is not RuntimeRevisionId.

4. WorkItemId is not AttemptId.

5. Attempt success does not imply Artifact publication.

6. Candidate Artifact is not authoritative.

7. Published Artifacts are immutable.

8. Published Artifact provenance is explicit.

9. Stale Candidates cannot publish.

10. Cancellation does not replace authority validation.

11. Retry creates a new Attempt.

12. Provider responses are normalized before becoming Candidates.

13. Structured text is preferred over OCR when available.

14. RecognitionArtifact is the stable Recognition output.

15. SourceDocumentArtifact is the stable Text Processing output.

16. TranslationUnit belongs to Translation.

17. TranslationArtifact remains independent from UI mode.

18. PresentationArtifact remains independent from native rendering framework.

19. ViewModel is non-authoritative and disposable.

20. Event Bus does not control data execution flow.

21. Cache is optimization, not authority.

22. Cached output requires compatibility/authority validation.

23. Raw captures do not enter normal logs.

24. Credentials never enter Artifacts/events/ViewModels/cache keys.

25. User corrections remain distinguishable from original outputs.

26. Geometry failure does not invalidate semantic Translation output.

27. Continuous sources remain bounded by Runtime resource/backpressure policy.

28. Diagnostics observes but does not own pipeline data.

---

# 131. Deprecated v1 Core Types

The following v1 architecture-wide concepts are deprecated as universal core contracts:

```text
SourceRevisionId hierarchy
ProcessingAttemptId
ProcessingEnvelope
OcrResult
SourceSegment as universal cross-module object
TranslationResult as generic pipeline object
PresentationModel as global semantic authority
```

Equivalent concepts may still exist locally inside owner modules.

---

# 132. Preserved v1 Principles

The following v1 principles remain valid:

```text
prefer structured text over OCR
preserve geometry
preserve source-target alignment
protect against stale results
minimize sensitive data retention
avoid provider-native leakage
use bounded contextual translation
separate Translation from Presentation
preserve correction provenance
support incremental long-text presentation
```

---

# 133. Architecture Review Checklist

Before accepting a data flow, verify:

* Is every canonical object owned by one module?
* Is the current ReadingContextRevision explicit where relevant?
* Is Runtime execution represented by RuntimeRevision/WorkItem/Attempt?
* Does each Candidate carry provenance?
* Does publication require authority validation?
* Can stale work complete without changing current state?
* Is retry Runtime-owned?
* Is cancellation Runtime-owned?
* Does Translation own TranslationUnit construction?
* Does Text Processing publish SourceDocumentArtifact?
* Does Presentation consume TranslationArtifact rather than provider output?
* Can UI projection be rebuilt without rerunning processing?
* Are provider-native objects isolated?
* Are cache entries compatibility-checked?
* Are large/sensitive values retained minimally?
* Are credentials isolated?
* Can errors/partial results preserve original ownership?
* Does Event Bus report facts rather than control execution?

---

# 134. Validation Scenarios

Required scenarios include:

```text
continuous comic scrolling
vertical Chinese comic text
long web novel chapter
source changes during Translation
user correction
provider timeout/fallback
overlay geometry failure
cache hit with changed glossary
application shutdown during remote request
late result after RuntimeRevision supersession
```

---

# 135. Validation — Continuous Comic

Verify:

```text
candidate capture remains bounded
stable content is accepted
duplicate content does not create unnecessary expensive work
late old Artifacts cannot publish
latest content receives appropriate priority
```

---

# 136. Validation — Vertical Chinese

Verify:

```text
geometry survives Capture → Recognition
text direction survives Recognition → Text Processing
reading order remains traceable
translation alignment remains explicit
Presentation can map output correctly
```

---

# 137. Validation — Long Novel

Verify:

```text
structured source avoids OCR
semantic document structure preserved
Translation creates bounded units
partial accepted output is explicit
earlier visible content remains stable
```

---

# 138. Validation — Supersession

Verify:

```text
RuntimeRevision A
    ↓
late Candidate

RuntimeRevision B current
```

does not permit Candidate A to publish current output.

---

# 139. Validation — Provider Failure

Verify:

```text
bounded retry
separate Attempt identities
fallback provenance
no duplicate publication
credentials never logged
session remains usable
```

---

# 140. Validation — Presentation Failure

Verify Presentation/UI failure cannot destroy valid TranslationArtifact.

---

# 141. Open Decisions

Prototype evidence is still needed for:

```text
frame-change/stability heuristics
Capture cadence
full-frame vs region Recognition
Recognition preprocessing
Translation contextual batch sizing
streaming presentation
browser extraction granularity
cache retention durations
overlay fallback UX
```

These decisions do not change core ownership boundaries.

---

# 142. Related Documents

```text
doc/01-architecture/core/
├── DATA_FLOW.md
├── STATE_MACHINE.md
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
├── CAPABILITY_MAP.md
└── README.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── CANCELLATION.md
├── RETRY_POLICY.md
├── SCHEDULER.md
├── WORK_QUEUE.md
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

# 143. Documentation Authority

This file defines:

```text
architecture-wide data movement
Artifact boundary conventions
data ownership relationships
Runtime/data separation
Candidate/Published flow
provider/cache/storage boundaries
cross-module data traceability
```

Module contracts remain authoritative for exact schemas.

---

# 144. Completion Criteria

This document is synchronized when:

* `ProcessingEnvelope` is removed as core execution authority;
* generic `ProcessingAttemptId` is replaced by Runtime WorkItem/Attempt identities;
* generic SourceRevision hierarchy no longer owns all downstream identity;
* ReadingContextRevision and RuntimeRevision are distinct;
* Capture → Recognition → Text Processing → Translation → Presentation Artifact boundaries are explicit;
* RecognitionArtifact replaces generic core `OcrResult`;
* SourceDocumentArtifact is the Text Processing output;
* TranslationUnit belongs to Translation;
* Candidate and Published Artifacts are distinct;
* publication requires authority validation;
* Runtime owns retry/cancellation/backpressure;
* Event Bus no longer moves work by requested/completed stage chain;
* cache results still pass compatibility/authority validation;
* Presentation and native UI rendering remain separate;
* provider-native objects and credentials remain isolated;
* privacy and retention principles remain explicit.

---

# 145. Summary

CRAI v1 broadly modeled:

```text
Source Revision
    ↓
OCR Result
    ↓
Source Segments
    ↓
Translation Units
    ↓
Translation Result
    ↓
Presentation Model
```

CRAI Runtime v2 models:

```text
ReadingContextRevision
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

The semantic processing chain is:

```text
Capture Artifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
    ↓
UI Adapter ViewModel
```

The central invariant is:

```text
Runtime owns execution.

Modules own semantic data.

Artifacts cross module boundaries.

Candidates are provisional.

Published Artifacts are authoritative.

UI projections are disposable.
```
