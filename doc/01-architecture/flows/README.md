# CRAI Architecture Flows

> **Project:** CRAI
> **Path:** `doc/01-architecture/flows/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

Thư mục này chứa các end-to-end architecture flows của CRAI.

Một flow mô tả:

```text
what happens
across multiple architecture owners
during one coherent scenario
```

Flow documents tập trung vào:

```text
user/system scenario
cross-module behavior
authority handoff
data movement
Runtime interaction
supersession
recovery
user-visible outcome
```

Chúng không định nghĩa implementation cụ thể.

---

# 2. Why Flow Documents Exist

Các tài liệu module trả lời:

```text
Who owns this responsibility?
```

Runtime trả lời:

```text
How is executable work controlled?
```

Core Architecture trả lời:

```text
What architecture-wide rules must remain true?
```

Flow documents trả lời:

```text
How do those owners work together
for one end-to-end scenario?
```

---

# 3. Flow Is Not Ownership Authority

Một flow có thể mô tả nhiều owner.

Nó không trở thành owner của các concept đó.

Ví dụ:

```text
SCREEN_COMIC_FLOW.md
```

có thể mô tả:

```text
Reading Session
Capture
Recognition
Text Processing
Translation
Presentation
Runtime
UI Adapter
```

nhưng ownership vẫn nằm ở các tài liệu tương ứng.

---

# 4. Flow Is Not a Module

Không tạo:

```text
ScreenComicFlowModule
StructuredTextFlowModule
ContentChangeModule
```

chỉ vì có flow document.

Flow là một **cross-owner behavior view**, không phải module boundary.

---

# 5. Flow Is Not Runtime

Flow mô tả:

```text
what logical behavior occurs
```

Runtime mô tả:

```text
how executable work
is scheduled, retried, cancelled,
superseded and resource-controlled
```

---

# 6. Flow Responsibilities

Một flow có thể mô tả:

```text
actors

preconditions

user intents

Application coordination

Reading Session effects

business execution planning

RuntimeRevision relationship

semantic Artifact movement

content change

supersession

recovery

error projection

user-visible outcomes
```

---

# 7. Flow Must Not Redefine

Flow documents không được sở hữu lại:

```text
module state machines

module event catalogs

module error taxonomies

Runtime WorkItem states

Runtime Attempt states

Retry Policy

Cancellation mechanism

Scheduler algorithm

queue structures

provider APIs

storage schemas

native UI implementation
```

---

# 8. Current Flow Set

Current architecture flow set:

```text
01-architecture/flows/
├── README.md
├── READING_SESSION_FLOW.md
├── CONTENT_CHANGE_FLOW.md
├── SCREEN_COMIC_FLOW.md
└── STRUCTURED_TEXT_FLOW.md
```

---

# 9. Flow Taxonomy

Bốn flow hiện tại thuộc ba loại khác nhau:

```text
Lifecycle Flow
    └── READING_SESSION_FLOW.md

Cross-Cutting Behavior Flow
    └── CONTENT_CHANGE_FLOW.md

End-to-End Source Flows
    ├── SCREEN_COMIC_FLOW.md
    └── STRUCTURED_TEXT_FLOW.md
```

---

# 10. READING_SESSION_FLOW.md

## Question

```text
How does one reading activity live
from creation to stop?
```

Flow này mô tả:

```text
Session creation

source/context establishment

ReadingContext

ReadingContextRevision

ACTIVE / PAUSED / STOPPING / STOPPED relationships

Preferences interaction

Application coordination

Reading Session → Runtime relationship

session stop/recovery
```

---

# 11. Central Reading Session Rule

```text
Reading Session
    owns reading authority.

Runtime
    owns execution authority.
```

Therefore:

```text
ReadingContextRevision
    ≠
RuntimeRevisionId
```

---

# 12. READING_SESSION_FLOW Is Not Processing Pipeline

Flow này không mô tả:

```text
CAPTURING
→ OCR_PROCESSING
→ TRANSLATING
→ RENDERING
```

như Reading Session lifecycle.

Một session có thể vẫn:

```text
ACTIVE
```

trong khi nhiều Runtime WorkItems/Attempts tồn tại.

---

# 13. CONTENT_CHANGE_FLOW.md

## Question

```text
What happens when the source changes
faster than processing can finish?
```

Flow này mô tả:

```text
observation

change detection

stability

duplicate suppression

new current content

RuntimeRevision supersession

cancellation

late-result handling

Candidate rejection

backpressure

freshness
```

---

# 14. Central Content Change Rule

```text
Newer authoritative content
must never be replaced
by an older late result.
```

---

# 15. Supersession vs Cancellation

`CONTENT_CHANGE_FLOW.md` distinguishes:

```text
Supersession
    → changes authority

Cancellation
    → saves resources

Publication validation
    → protects correctness
```

Cancellation is therefore not the final stale-result guarantee.

---

# 16. Content Change vs ReadingContext Change

The flow also establishes:

```text
content change
    ≠
ReadingContext change
```

Example:

```text
same selected comic region
user scrolls
```

may keep:

```text
ReadingContextRevision C1
```

while producing:

```text
RuntimeRevision R10
RuntimeRevision R11
RuntimeRevision R12
```

---

# 17. SCREEN_COMIC_FLOW.md

## Question

```text
How does CRAI translate
continuously changing visual comic content?
```

This is the primary image-based reading flow.

---

# 18. Screen Comic Semantic Path

```text
Screen / Visual Source
    ↓
CaptureArtifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
    ↓
UI Adapter
```

---

# 19. Screen Comic Execution Path

Execution is separately represented as:

```text
BusinessExecutionPlan
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

---

# 20. Screen Comic Observation

The flow emphasizes:

```text
continuous observation

stable-content gating

scroll handling

bounded work

supersession

late provider results

current-content priority
```

It does not assume every observed frame becomes full processing work.

---

# 21. STRUCTURED_TEXT_FLOW.md

## Question

```text
How does CRAI process text
that already exists in machine-readable form?
```

Typical sources:

```text
web novel

DOM text

clipboard text

text document

future structured-document source
```

---

# 22. Structured Text Semantic Path

```text
Structured Source
    ↓
Source Adapter
    ↓
Structured Source Snapshot
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
```

---

# 23. No OCR by Default

The central rule is:

```text
If trustworthy structured text exists,
do not reconstruct it through OCR.
```

Therefore structured-text flow normally skips:

```text
Capture
Recognition
```

---

# 24. Flow Convergence

Image and structured paths converge at:

```text
SourceDocumentArtifact
```

Image path:

```text
Capture
    ↓
Recognition
    ↓
Text Processing
    ↓
SourceDocumentArtifact
```

Structured path:

```text
Structured Source
    ↓
Text Processing
    ↓
SourceDocumentArtifact
```

---

# 25. Shared Downstream Flow

After convergence:

```text
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
UI Adapter
```

Downstream modules therefore do not need to know whether source text came from OCR or a structured source.

---

# 26. Relationship Between Flows

The current flow set can be viewed as:

```text
READING_SESSION_FLOW
        │
        ├───────────────┐
        │               │
        ▼               ▼
SCREEN_COMIC_FLOW   STRUCTURED_TEXT_FLOW
        │               │
        └───────┬───────┘
                ▼
      CONTENT_CHANGE_FLOW
      applies where source
      changes over time
```

This is conceptual reuse, not document inheritance.

---

# 27. Another View

```text
Reading Session
    ↓
establishes reading authority

Source-Specific Flow
    ├── Screen Comic
    └── Structured Text

Content Change
    ↓
governs freshness/supersession

Runtime
    ↓
executes required work
```

---

# 28. Relationship With Core Architecture

Flows must respect:

```text
01-architecture/core/
```

especially:

```text
STATE_MACHINE.md
DATA_FLOW.md
EVENT_CONVENTION.md
EVENT_BUS.md
CAPABILITY_MAP.md
```

---

# 29. STATE_MACHINE Relationship

`STATE_MACHINE.md` answers:

```text
Who owns state and authority?
```

Flow documents may show transitions but must not create competing owner state machines.

---

# 30. DATA_FLOW Relationship

`DATA_FLOW.md` defines canonical:

```text
Artifact boundaries

Candidate → Published

Runtime/data separation

cross-module provenance
```

Flows apply those rules to concrete scenarios.

---

# 31. EVENT_CONVENTION Relationship

Flow documents may reference committed events.

They must not invent command-like events such as:

```text
TranslationRequested

RetryRequested

CancelPipelineRequested
```

---

# 32. EVENT_BUS Relationship

Event Bus reports committed facts.

Flows must not use Event Bus as the execution graph.

Forbidden:

```text
RecognitionCompleted
    ↓
TranslationRequested
```

as stage control.

---

# 33. Relationship With Module Architecture

Flows use module ownership defined by:

```text
01-architecture/modules/
├── MODULE_MAP.md
├── OWNERSHIP_MAP.md
└── MODULE_DEPENDENCY.md
```

---

# 34. MODULE_MAP Relationship

A flow may only use current module topology unless explicitly discussing historical architecture.

Current primary modules:

```text
Capture

Recognition

Text Processing

Translation

Presentation

Reading Session

Preferences

Diagnostics

UI Adapter
```

---

# 35. OWNERSHIP_MAP Relationship

Flow descriptions must preserve:

```text
one concept
    ↓
one semantic owner
```

Example:

```text
Runtime executes Translation work
```

does not mean Runtime owns:

```text
TranslationArtifact
```

---

# 36. MODULE_DEPENDENCY Relationship

A flow may show semantic order:

```text
RecognitionArtifact
    ↓
Text Processing
```

but must not imply direct forbidden implementation calls such as:

```text
RecognitionService
    → TextProcessingService.process()
```

Execution dependencies remain Runtime/Application controlled.

---

# 37. Relationship With Runtime

Runtime architecture defines:

```text
BusinessExecutionPlan

RuntimeRevision

WorkItem

Attempt

Retry

Cancellation

Scheduler

Work Queue

Resource Lifecycle

Backpressure
```

Flow documents reference those concepts but do not redefine them.

---

# 38. What Flow Decides

A flow may state:

```text
old work becomes obsolete
```

or:

```text
current content should outrank obsolete content
```

as scenario requirements.

---

# 39. What Runtime Decides

Runtime defines:

```text
how supersession is represented

which WorkItems are cancelled

how cancellation propagates

whether retry occurs

when another Attempt is created

queue behavior

resource admission
```

---

# 40. Correct Cancellation Wording

Preferred flow wording:

```text
new content supersedes old execution

Runtime handles obsolete WorkItems/Attempts
according to cancellation policy
```

Avoid architecture wording like:

```text
Flow cancels OCR
```

because flow documents are not execution owners.

---

# 41. RuntimeRevision Rule

Flows must distinguish:

```text
ReadingContextRevision
```

from:

```text
RuntimeRevisionId
```

Do not use one generic `RevisionId` to mean both.

---

# 42. WorkItem / Attempt Rule

Flow documents may reference:

```text
WorkItem
Attempt
```

when execution behavior matters.

They should not reintroduce generic:

```text
TaskId
ProcessingAttemptId
PipelineId
```

as universal execution identities.

---

# 43. Candidate / Published Rule

Every flow involving computed semantic output must preserve:

```text
Attempt
    ↓
Candidate Artifact
    ↓
Authority Validation
    ↓
Published Artifact
```

Execution completion alone does not grant publication authority.

---

# 44. Stale Result Rule

A flow must never assume:

```text
provider returned
    ↓
therefore current UI updates
```

A late result may belong to superseded execution.

---

# 45. Cache Rule

Flows may use cache as an optimization.

They must not treat:

```text
cache hit
```

as current semantic authority.

Cached output still needs:

```text
compatibility validation
+
current authority validation
```

---

# 46. Presentation Rule

Presentation is separate from native UI rendering.

Flows use:

```text
TranslationArtifact
    ↓
PresentationArtifact
    ↓
UI Adapter
    ↓
ViewModel
```

rather than:

```text
Translation
    ↓
Render UI directly
```

---

# 47. Preferences Rule

Persistent preferences belong to:

```text
Preferences
```

Session-specific overrides belong to:

```text
Reading Session
```

Flow documents must not collapse both into one mutable global configuration authority.

---

# 48. Diagnostics Rule

Diagnostics may observe:

```text
flow latency

Runtime correlation

errors

supersession

late results

cache behavior
```

It does not control the flow.

---

# 49. Platform Rule

Platform-specific concerns remain in adapters.

Examples:

```text
screen capture API

DOM APIs

window handles

clipboard objects

native UI objects
```

must not become stable flow-level semantic contracts.

---

# 50. Provider Rule

Provider-specific DTOs remain inside provider adapters.

Flow documents should refer to:

```text
Recognition Provider

Translation Provider
```

through semantic/provider contracts rather than specific SDK response types.

---

# 51. Flow Naming Convention

Flow filenames should describe a coherent scenario or reusable cross-cutting behavior.

Preferred:

```text
SCREEN_COMIC_FLOW.md

STRUCTURED_TEXT_FLOW.md

READING_SESSION_FLOW.md

CONTENT_CHANGE_FLOW.md
```

---

# 52. When to Add a New Flow

Create a new flow when it represents a distinct cross-owner scenario that cannot be understood clearly by a small section in an existing flow.

A new flow should answer a meaningful question such as:

```text
How does CRAI recover a durable session after restart?

How does a manual correction propagate through Artifacts?

How does mixed image/text content converge?
```

---

# 53. When Not to Add a Flow

Do not create a flow merely because there is:

```text
a module

a provider

a function

an error

a Runtime mechanism

a UI screen
```

Those normally belong to owner-specific documents.

---

# 54. Flows Not Currently Needed

Do not create redundant current-authority files such as:

```text
OCR_FLOW.md

TRANSLATION_FLOW.md

PRESENTATION_FLOW.md

CACHE_FLOW.md

RETRY_FLOW.md

CANCELLATION_FLOW.md
```

because those concerns already have dedicated owners.

---

# 55. Possible Future Flows

Potential future flows may include:

```text
MANUAL_IMAGE_FLOW.md

CORRECTION_FLOW.md

SESSION_RECOVERY_FLOW.md

MIXED_CONTENT_FLOW.md

IMPORT_DOCUMENT_FLOW.md
```

Only add them when the scenario gains enough architecture-specific behavior to justify a separate document.

---

# 56. Correction Flow Consolidation

Earlier ideas included separate:

```text
OCR_CORRECTION_FLOW.md

TRANSLATION_CORRECTION_FLOW.md
```

Before creating both, first determine whether a shared:

```text
CORRECTION_FLOW.md
```

can describe:

```text
source correction

translation correction

provenance

durable user intent

Artifact regeneration
```

without duplicating owner-specific contracts.

---

# 57. Manual Image Flow

`MANUAL_IMAGE_FLOW.md` may eventually be useful if manual image import develops behavior sufficiently different from `SCREEN_COMIC_FLOW.md`.

Potential differences:

```text
no continuous observation

no scroll stability

explicit file/clipboard input

different session lifecycle

different cache/reprocessing behavior
```

Until then it may remain covered by core/module architecture.

---

# 58. Session Recovery Flow

`SESSION_RECOVERY_FLOW.md` should be added only when durable session recovery semantics are decided.

Open questions include:

```text
preserve SessionId or create new?

recover PAUSED or READY?

which ReadingContext fields persist?

how current source is revalidated?

what history is restored?
```

---

# 59. Mixed Content Flow

A future `MIXED_CONTENT_FLOW.md` may be justified for sources containing:

```text
structured prose

embedded comic panels

image-only dialogue

captions
```

because both structured and visual source paths may need composition.

---

# 60. Flow Reading Order

Recommended reading order:

```text
1. README.md

2. READING_SESSION_FLOW.md

3. CONTENT_CHANGE_FLOW.md

4. SCREEN_COMIC_FLOW.md

5. STRUCTURED_TEXT_FLOW.md
```

---

# 61. Why This Order

First:

```text
READING_SESSION_FLOW
```

establishes:

```text
SessionId
ReadingContext
ReadingContextRevision
Runtime relationship
```

Then:

```text
CONTENT_CHANGE_FLOW
```

establishes:

```text
freshness
supersession
cancellation
late-result handling
```

Then source-specific end-to-end flows apply those rules.

---

# 62. Source Flow Reading Order

For image/screen work:

```text
READING_SESSION_FLOW
    ↓
CONTENT_CHANGE_FLOW
    ↓
SCREEN_COMIC_FLOW
```

For structured text:

```text
READING_SESSION_FLOW
    ↓
CONTENT_CHANGE_FLOW
    ↓
STRUCTURED_TEXT_FLOW
```

`CONTENT_CHANGE_FLOW` is especially relevant for continuously changing structured/browser sources.

---

# 63. Cross-Flow Invariants

All flow documents must preserve:

1. Reading Session owns ReadingContext.

2. Reading Session owns ReadingContextRevision.

3. Runtime owns RuntimeRevision.

4. Runtime owns WorkItem.

5. Runtime owns Attempt.

6. Modules own semantic Artifacts.

7. Candidate does not equal Published.

8. Publication requires authority validation.

9. Content change is not automatically ReadingContext change.

10. Retry belongs to Runtime.

11. Cancellation mechanics belong to Runtime.

12. Supersession is distinct from failure.

13. Late work cannot regain current authority.

14. Event Bus reports facts only.

15. Event Bus does not orchestrate processing.

16. Persistent Preferences remain Preferences-owned.

17. Translation owns TranslationUnit/TranslationBatch.

18. Text Processing owns SourceDocumentArtifact.

19. Presentation owns PresentationArtifact.

20. UI Adapter owns ViewModel.

21. Platform/provider-native objects remain isolated.

22. Cache remains optimization, not authority.

23. Flow documents never redefine owner-local state/events/errors.

---

# 64. Common Flow Mistake — Monolithic Pipeline

Avoid:

```text
SESSION_STARTED
    ↓
CAPTURING
    ↓
OCR_RUNNING
    ↓
TRANSLATING
    ↓
RENDERING
```

as one global flow/state authority.

Current architecture separates domain and execution state.

---

# 65. Common Flow Mistake — Direct Stage Calls

Avoid:

```text
Recognition
    ↓
calls Text Processing
    ↓
calls Translation
```

for orchestration.

Semantic dependency does not imply direct imperative dependency.

---

# 66. Common Flow Mistake — Event-Driven Commands

Avoid:

```text
RecognitionCompleted
    ↓
TranslationRequested
```

through Event Bus.

Use Runtime dependency readiness.

---

# 67. Common Flow Mistake — Generic Revision

Avoid:

```text
revision++
```

to represent:

```text
session context
source content
Runtime execution
Artifact version
UI projection
```

Use typed authorities.

---

# 68. Common Flow Mistake — Cancellation as Correctness

Avoid assuming:

```text
cancel old work
    ↓
therefore stale result impossible
```

Providers may complete late.

Publication validation remains mandatory.

---

# 69. Common Flow Mistake — UI Owns Flow

Avoid letting:

```text
UI Adapter
```

directly call:

```text
Recognition provider

Translation provider

Scheduler

Storage implementation
```

UI sends semantic intents through Application contracts.

---

# 70. Common Flow Mistake — Flow Owns Errors

Flow documents do not create a generic:

```text
FLOW-*
```

error taxonomy.

Errors remain with:

```text
CAP-*
REC-*
TXT-*
TRN-*
PRES-*
SES-*
RUN-*
...
```

according to owner.

---

# 71. Common Flow Mistake — Flow Owns Events

Flow documents may reference module events.

They do not create independent competing event catalogs.

Exact events live under:

```text
02-modules/<module>/EVENTS.md
```

---

# 72. Architecture Validation Checklist

When creating or reviewing a flow, ask:

```text
Which owner commits each state?

Which owner owns each Artifact?

What is the ReadingContextRevision?

What is the RuntimeRevision?

Which WorkItems/Attempts exist?

Can old work finish late?

Where is publication authority checked?

Does the flow accidentally use Event Bus as command routing?

Does the flow create a direct forbidden module dependency?

Are provider/platform objects leaking?

Can the user-visible result remain coherent under concurrency?
```

---

# 73. Flow Completeness Checklist

A meaningful end-to-end flow should normally identify:

```text
entry intent

preconditions

main owners

authority changes

semantic data path

Runtime relationship

error/degradation behavior

supersession behavior where relevant

user-visible outcome

termination / continuation behavior
```

---

# 74. Relationship With MVP

`SCREEN_COMIC_FLOW.md` currently represents the strongest concrete MVP image-reading scenario.

`STRUCTURED_TEXT_FLOW.md` represents the architecture for the major text-native follow-up/use case.

Capability status remains governed by:

```text
core/CAPABILITY_MAP.md
```

Flow documentation alone does not make a feature `Validated` or implemented.

---

# 75. Current Architecture Model

At the broadest level:

```text
User
    ↓
UI Adapter
    ↓
Application
    ↓
Reading Session / Domain Authority
    ↓
Business Execution Planning
    ↓
Runtime
    ↓
Semantic Modules
    ↓
Published Artifacts
    ↓
Presentation
    ↓
UI Projection
```

---

# 76. Source-Specific Branching

```text
                 ┌─ Screen / Image
                 │       ↓
                 │    Capture
                 │       ↓
                 │  Recognition
                 │       ↓
Source Input ────┤
                 │
                 │  Structured Text
                 │       ↓
                 └── Text Processing
                         ↓
                 SourceDocumentArtifact
                         ↓
                    Translation
                         ↓
                    Presentation
```

---

# 77. Runtime View

Regardless of source path:

```text
BusinessExecutionPlan
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

---

# 78. Documentation Authority

This directory defines:

```text
cross-owner scenario behavior
```

It does not supersede:

```text
core architecture

module ownership

module contracts

Runtime architecture

Infrastructure contracts
```

If a flow conflicts with an owner document, the stale document must be identified and corrected rather than inventing duplicate authority.

---

# 79. Related Documents

```text
doc/01-architecture/core/
├── CAPABILITY_MAP.md
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── EVENT_CONVENTION.md
└── EVENT_BUS.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── OWNERSHIP_MAP.md
└── MODULE_DEPENDENCY.md

doc/01-architecture/runtime/

doc/01-architecture/flows/
├── READING_SESSION_FLOW.md
├── CONTENT_CHANGE_FLOW.md
├── SCREEN_COMIC_FLOW.md
└── STRUCTURED_TEXT_FLOW.md

doc/02-modules/

doc/03-infrastructure/
```

---

# 80. Completion Criteria

The `flows/` architecture set is synchronized when:

* Reading Session lifecycle is separated from processing execution;
* ReadingContextRevision is separated from RuntimeRevision;
* content-change/supersession behavior has its own reusable flow;
* screen/image and structured-text paths are both represented;
* structured-text flow does not require OCR;
* visual and structured paths converge at SourceDocumentArtifact;
* TranslationUnit remains Translation-owned;
* Runtime owns WorkItem/Attempt/retry/cancellation;
* Event Bus does not orchestrate stages;
* Candidate/Published boundaries are preserved;
* late results cannot overwrite current authority;
* source-specific flows do not duplicate module-local state/events/errors;
* current flow set is reflected accurately in this README.

---

# 81. Summary

The `flows/` directory currently contains four architecture views:

```text
READING_SESSION_FLOW
    → lifecycle and reading authority

CONTENT_CHANGE_FLOW
    → freshness and supersession

SCREEN_COMIC_FLOW
    → image/screen end-to-end reading

STRUCTURED_TEXT_FLOW
    → machine-readable text end-to-end reading
```

Together they describe:

```text
Reading Activity
    ↓
Source Behavior
    ↓
Execution Authority
    ↓
Semantic Artifacts
    ↓
User-visible Translation
```

The central rule is:

```text
Flows describe collaboration.

Modules own semantics.

Runtime owns execution.

Core owns invariants.

A flow must never
silently replace any of them.
```
