# CRAI Core Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/core/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

Thư mục này chứa các tài liệu kiến trúc cốt lõi của CRAI.

Các tài liệu tại đây định nghĩa những quy tắc architecture-wide về:

```text
product capabilities
state ownership
data authority
cross-module data flow
event semantics
event distribution
```

Đây là các nguyên tắc nền mà:

```text
module architecture
Runtime architecture
infrastructure
platform adapters
provider integrations
```

phải tuân theo.

---

# 2. Core Architecture Is Technology-Neutral

Các tài liệu trong `core/` không phụ thuộc vào:

```text
UI framework
programming language
OCR SDK
Translation SDK
database
message bus implementation
operating system API
provider implementation
```

Core Architecture mô tả:

```text
what must remain true
```

chứ không mô tả:

```text
how a specific technology implements it
```

---

# 3. Scope

`core/` chịu trách nhiệm cho các vấn đề architecture-wide.

Bao gồm:

```text
Capability Model
State Ownership Model
Data Flow Model
Artifact Authority
Event Semantics
Event Distribution
Cross-cutting Architecture Invariants
```

---

# 4. Out of Scope

`core/` không định nghĩa chi tiết:

```text
module APIs
module-local state machines
module-local event catalogs
Runtime scheduler algorithms
Work Queue implementation
Retry algorithm implementation
provider-native schemas
storage schemas
native UI rendering
platform APIs
source-code structure
```

Các nội dung đó thuộc tài liệu owner tương ứng.

---

# 5. Core Architecture Documents

```text
core/
├── README.md
├── CAPABILITY_MAP.md
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── EVENT_CONVENTION.md
└── EVENT_BUS.md
```

Mỗi tài liệu trả lời một câu hỏi architecture khác nhau.

---

# 6. CAPABILITY_MAP.md

## Question

```text
CRAI cần có khả năng làm được những gì?
```

`CAPABILITY_MAP.md` định nghĩa:

```text
product capabilities
capability status
feasibility
MVP capability direction
prototype gates
future capabilities
```

Nó không quyết định trực tiếp:

```text
module boundaries
Runtime ownership
source-code packages
```

---

# 7. Capability vs Ownership

Ví dụ:

```text
Capability:
Recover from retryable Translation failure
```

không có nghĩa:

```text
Translation module owns retry execution
```

Architecture có thể phân chia:

```text
Translation
    → error classification

Runtime
    → Retry Policy
    → new Attempt
```

Capability và ownership là hai câu hỏi khác nhau.

---

# 8. STATE_MACHINE.md

## Question

```text
Ai sở hữu state và authority?
```

`STATE_MACHINE.md` định nghĩa architecture-wide rules cho:

```text
state ownership
domain authority
Runtime authority
revision ownership
transition authority
projection state
supersession
Candidate vs Published authority
```

---

# 9. STATE_MACHINE.md Does Not Own Every State Machine

Chi tiết state machine của từng owner nằm tại tài liệu owner tương ứng.

Ví dụ:

```text
Reading Session lifecycle
    → 02-modules/reading-session/

Runtime execution state
    → 01-architecture/runtime/

Presentation state
    → 02-modules/presentation/

UI projection state
    → 02-modules/ui-adapter/
```

`STATE_MACHINE.md` định nghĩa quy tắc chung để các state machine đó không xung đột authority.

---

# 10. DATA_FLOW.md

## Question

```text
Authoritative data di chuyển qua CRAI như thế nào?
```

`DATA_FLOW.md` định nghĩa:

```text
data ownership
Artifact boundaries
Candidate → Published flow
Runtime execution references
cross-module Artifact movement
stale-result rejection
cache interaction
provider boundaries
storage/retention
privacy
```

---

# 11. Canonical Semantic Data Flow

Architecture-level semantic flow:

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

Not every reading path requires every stage.

For example:

```text
Structured Text
    ↓
Text Processing
    ↓
SourceDocumentArtifact
```

may skip Recognition entirely.

---

# 12. Runtime Data Flow

Execution authority is represented separately:

```text
ReadingContextRevision
    ↓
Business Execution Planning
    ↓
RuntimeRevision
    ↓
WorkItem
    ↓
Attempt
    ↓
Candidate Artifact
    ↓
Authority Validation
    ↓
Published Artifact
```

Runtime identities do not replace semantic Artifact ownership.

---

# 13. EVENT_CONVENTION.md

## Question

```text
Một CRAI Event hợp lệ phải có ý nghĩa và hình dạng như thế nào?
```

`EVENT_CONVENTION.md` defines:

```text
fact-only semantics
event naming
event ownership
payload conventions
event identity
versioning
typed authority references
privacy rules
event necessity
```

---

# 14. Central Event Convention

A CRAI Event describes:

```text
a fact that has already become true
```

Example:

```text
ReadingContextChanged
TranslationArtifactPublished
RuntimeRevisionSuperseded
```

Not:

```text
TranslationRequested
RetryRequested
CancelRequested
```

---

# 15. EVENT_BUS.md

## Question

```text
Committed facts được phân phối giữa các component như thế nào?
```

`EVENT_BUS.md` defines:

```text
publish/subscribe transport semantics
delivery expectations
subscriber isolation
ordering scope
deduplication expectations
Event Bus boundaries
failure handling
```

---

# 16. EVENT_BUS.md vs EVENT_CONVENTION.md

Do not merge these responsibilities.

```text
EVENT_CONVENTION.md
    ↓
What is a valid Event?

EVENT_BUS.md
    ↓
How is that Event distributed?
```

---

# 17. Event Bus Is Not Execution Control

CRAI does not use:

```text
CaptureRequested
    ↓
RecognitionRequested
    ↓
TranslationRequested
```

to orchestrate processing.

Execution belongs to Runtime.

Event Bus reports committed facts.

---

# 18. Core Authority Model

Core architecture separates five important authority categories.

```text
Domain Authority
Runtime Authority
Artifact Authority
Presentation Authority
Projection Authority
```

---

# 19. Domain Authority

Examples:

```text
Reading Session
Preferences
module-owned semantic state
```

These owners determine domain truth.

---

# 20. Runtime Authority

Runtime owns execution concepts:

```text
RuntimeRevision
WorkItem
Attempt
queueing
scheduling
retry
cancellation
deadline
supersession
```

Runtime does not own semantic Artifact meaning.

---

# 21. Artifact Authority

Semantic modules own their Artifacts.

Examples:

```text
Capture
    → Capture Artifact

Recognition
    → RecognitionArtifact

Text Processing
    → SourceDocumentArtifact

Translation
    → TranslationArtifact

Presentation
    → PresentationArtifact
```

---

# 22. Projection Authority

UI Adapter owns disposable frontend projections such as:

```text
ViewModel
```

A ViewModel is not business/domain authority.

---

# 23. Candidate vs Published

One of the central Runtime v2 rules is:

```text
Attempt completes
    ↓
Candidate Artifact
    ↓
Authority Validation
    ↓
Published Artifact
```

Therefore:

```text
execution success
```

does not automatically mean:

```text
current authoritative result
```

---

# 24. Typed Authority

Core architecture avoids generic identity concepts such as:

```text
pipelineId
taskId
contentRevision
processingAttemptId
```

when the actual owner can be named.

Prefer:

```text
SessionId
ReadingContextRevision

RuntimeRevisionId
WorkItemId
AttemptId

ArtifactId

PreferenceRevision
PresentationRevision
ViewModelRevision
```

---

# 25. Core Dependency Direction

Conceptually:

```text
.meta architecture rules
        ↓
Core Architecture
        ↓
Module / Runtime Architecture
        ↓
Infrastructure / Platform
        ↓
Implementation
```

This is documentation authority direction, not necessarily source-code dependency direction.

---

# 26. `.meta` Relationship

Core Architecture must comply with project-level governance defined under `.meta/`.

Relevant examples may include:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES_RULE.md
.meta/MODULES.md
```

Therefore the old statement:

```text
Core Architecture does not depend on any other document
```

must not be interpreted literally.

---

# 27. Core vs Module Architecture

Module architecture answers:

```text
Which module owns this responsibility?
What does it expose?
What state/events/errors does it own?
```

Core Architecture answers:

```text
What architecture-wide rule must every module respect?
```

---

# 28. Core vs Runtime Architecture

Runtime architecture defines:

```text
BusinessExecutionPlan
RuntimeRevision
WorkItem
Attempt
Scheduler
Work Queue
Retry Policy
Cancellation
Backpressure
Runtime Observability
```

Core Architecture defines the authority rules Runtime must respect.

---

# 29. Core vs Infrastructure

Infrastructure provides mechanisms such as:

```text
Event Bus implementation
Logging
Telemetry
Storage
Cache
Secret Management
Scheduler mechanisms
```

Core documents define semantic constraints around those mechanisms.

---

# 30. Core vs Provider Architecture

Provider architecture defines:

```text
provider discovery
provider capability
provider configuration
provider adapter
provider health
provider selection support
```

Core defines constraints such as:

```text
provider-native data must not leak
credentials must remain isolated
provider result is not automatically an Artifact
```

---

# 31. Recommended Reading Order

For a new contributor or AI agent:

```text
1. .meta/AI_BOOT.md
2. .meta/PROJECT_RULE.md
3. .meta/MODULES_RULE.md

4. core/README.md
5. core/CAPABILITY_MAP.md
6. core/STATE_MACHINE.md
7. core/DATA_FLOW.md
8. core/EVENT_CONVENTION.md
9. core/EVENT_BUS.md
```

---

# 32. Why This Order

`CAPABILITY_MAP.md` first answers:

```text
What must CRAI be able to do?
```

Then `STATE_MACHINE.md` answers:

```text
Who owns authority?
```

Then `DATA_FLOW.md` answers:

```text
How does authoritative data move?
```

Then:

```text
EVENT_CONVENTION.md
```

defines valid committed facts.

Finally:

```text
EVENT_BUS.md
```

defines how those facts are distributed.

---

# 33. Runtime Reading Order

After Core Architecture:

```text
01-architecture/runtime/
```

should be read when working on:

```text
execution planning
WorkItem
Attempt
Scheduler
Work Queue
Retry
Cancellation
Backpressure
Runtime observability
```

---

# 34. Module Reading Order

When working on a module, read:

```text
01-architecture/modules/
    ↓
02-modules/<module>/
```

Architecture-level ownership must be understood before modifying module-local contracts.

---

# 35. Cross-Document Conflict Rule

If two documents appear to conflict:

```text
1. determine whether they own the same concern
2. identify the actual authority owner
3. do not silently merge semantics
4. update the stale document
```

Do not solve architecture conflict by introducing duplicate ownership.

---

# 36. Core Consistency Rule

The following must remain consistent across all core documents:

```text
ReadingContextRevision
RuntimeRevision
WorkItem
Attempt
Candidate Artifact
Published Artifact
Event
ViewModel
```

Each concept must retain one meaning.

---

# 37. Core Invariants

1. Capabilities do not define module ownership.

2. Every authoritative state has one owner.

3. Reading Session owns reading context, not Runtime execution.

4. Runtime owns WorkItem/Attempt execution.

5. Runtime does not own semantic Artifact meaning.

6. Semantic modules own their Published Artifacts.

7. Attempt success does not automatically publish an Artifact.

8. Candidate Artifacts require authority validation.

9. Published Artifacts are immutable.

10. Stale/superseded Candidates cannot become current.

11. Event is a committed fact.

12. Event is not a Command.

13. Event Bus does not orchestrate processing.

14. Event naming/semantics belong to EVENT_CONVENTION.

15. Event transport/distribution belongs to EVENT_BUS.

16. Structured text is preferred over OCR when reliable.

17. TranslationUnit construction belongs to Translation.

18. Presentation semantics and native rendering are separate.

19. UI ViewModels are disposable projections.

20. Cache is optimization, not authority.

21. Provider-native structures do not become core contracts.

22. Credentials never flow through normal Artifacts/events/ViewModels.

23. Diagnostics observes but does not become business authority.

24. Core Architecture remains technology-neutral.

---

# 38. Change Rules

When modifying a Core Architecture document:

```text
identify affected authority
    ↓
check sibling core documents
    ↓
check Runtime architecture
    ↓
check module ownership
    ↓
check affected module contracts
```

A core change may require downstream synchronization.

---

# 39. Common Architecture Mistakes

Avoid:

```text
adding a capability and automatically creating a module

putting retry inside every processing module

putting processing state inside Reading Session

using Event Bus as command routing

treating provider completion as Artifact publication

treating cache data as current authority

using generic pipeline IDs everywhere

letting UI own business state

letting Diagnostics own errors from other modules
```

---

# 40. Current Runtime Model

Core Architecture is aligned with:

```text
Runtime v2
```

Central execution model:

```text
Business Authority
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

# 41. Current Semantic Processing Model

```text
Capture
    ↓
Recognition
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
```

But this is a semantic dependency chain, not a mandatory serialized execution pipeline.

---

# 42. Alternate Paths

Example structured-text path:

```text
Structured Source
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
```

Recognition is skipped.

Future paths may skip or introduce capabilities without violating core authority rules.

---

# 43. Architecture Validation Questions

Before accepting a cross-cutting design, ask:

```text
Who owns the state?

Who owns the data?

Which revision makes it authoritative?

Is this execution or domain semantics?

Is this Candidate or Published?

Is this a Command or Event?

Does Runtime own this lifecycle?

Does this leak provider/platform implementation?

Can stale work overwrite current authority?

Can UI rebuild without rerunning processing?
```

---

# 44. Directory Relationship

```text
doc/
├── .meta/
│
├── 01-architecture/
│   ├── core/
│   ├── modules/
│   ├── runtime/
│   └── ...
│
├── 02-modules/
│
└── 03-infrastructure/
```

`core/` provides architecture-wide semantics used by the more specialized layers below it.

---

# 45. Completion Criteria

The `core/` architecture set is synchronized when:

* `CAPABILITY_MAP.md` separates product capability from architecture ownership;
* `STATE_MACHINE.md` defines architecture-wide state authority;
* `DATA_FLOW.md` uses Runtime v2 and Artifact authority;
* `EVENT_CONVENTION.md` defines fact-only event semantics;
* `EVENT_BUS.md` defines event distribution rather than execution control;
* all five documents agree on RuntimeRevision/WorkItem/Attempt;
* all five documents agree on Candidate vs Published;
* Reading Session does not own Runtime execution state;
* Translation owns TranslationUnit construction;
* Event Bus does not carry execution requests;
* Presentation remains separate from UI-native rendering;
* generic v1 pipeline identity is no longer architecture authority.

---

# 46. Summary

`core/` answers five foundational questions:

```text
CAPABILITY_MAP
    What must CRAI be able to do?

STATE_MACHINE
    Who owns state and authority?

DATA_FLOW
    How does authoritative data move?

EVENT_CONVENTION
    What is a valid committed Event?

EVENT_BUS
    How are committed Events distributed?
```

Together they establish:

```text
Capability
    ↓
Authority
    ↓
Data
    ↓
Facts
    ↓
Distribution
```

The central CRAI architecture rule is:

```text
Product capability
does not imply ownership.

Execution
does not imply authority.

Completion
does not imply publication.

Events
report facts.

Runtime
executes work.

Modules
own semantics.
```
