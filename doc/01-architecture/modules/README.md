# CRAI Module Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/modules/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

Thư mục này định nghĩa kiến trúc module ở cấp toàn hệ thống CRAI.

Nó trả lời ba câu hỏi chính:

```text
Which modules exist?

Who owns each architectural concept?

Which dependencies are allowed?
```

Ba câu hỏi này được tách thành ba tài liệu authority khác nhau:

```text
MODULE_MAP.md

OWNERSHIP_MAP.md

MODULE_DEPENDENCY.md
```

---

# 2. Scope

`01-architecture/modules/` chịu trách nhiệm cho:

```text
module topology
semantic ownership
module classification
dependency direction
allowed cross-module contracts
forbidden dependency patterns
legacy module migration
module-boundary invariants
```

---

# 3. Out of Scope

Thư mục này không định nghĩa chi tiết:

```text
Runtime algorithms
Work Queue implementation
Scheduler implementation
Retry algorithm
Cancellation algorithm
provider-native APIs
storage schema
UI framework
source-code package layout
module-internal implementation
```

Những nội dung đó thuộc owner tương ứng.

---

# 4. Current Documents

```text
01-architecture/modules/
├── README.md
├── MODULE_MAP.md
├── OWNERSHIP_MAP.md
└── MODULE_DEPENDENCY.md
```

---

# 5. MODULE_MAP.md

## Question

```text
CRAI hiện có những module nào?
```

`MODULE_MAP.md` định nghĩa:

```text
current primary modules
cross-cutting modules
Runtime classification
Infrastructure classification
legacy module mapping
module grouping
open module-boundary questions
```

---

# 6. Current Primary Modules

Current canonical business/application modules:

```text
Capture

Recognition

Text Processing

Translation

Presentation

Reading Session

Preferences
```

Cross-cutting modules:

```text
Diagnostics

UI Adapter
```

---

# 7. Runtime Is Not a Primary Business Module

Runtime is a separate architecture authority.

Runtime owns:

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
backpressure
resource admission
```

Runtime architecture lives under:

```text
01-architecture/runtime/
```

---

# 8. Infrastructure Is Not Business Module Ownership

Infrastructure areas include mechanisms such as:

```text
Event Bus
Scheduler
Logging
Telemetry
Configuration
Secret Management
Resource Management
Persistence
```

They provide technical capabilities.

They do not become semantic owners of business data.

---

# 9. OWNERSHIP_MAP.md

## Question

```text
Who owns each architectural concept?
```

`OWNERSHIP_MAP.md` defines the authoritative semantic owner for concepts such as:

```text
ReadingContext
ReadingContextRevision

RuntimeRevision
WorkItem
Attempt

Capture Artifact
RecognitionArtifact
SourceDocumentArtifact
TranslationArtifact
PresentationArtifact

TranslationUnit
PreferenceRevision
ViewModel
DiagnosticHealthSnapshot
```

---

# 10. Central Ownership Rule

```text
One concept
    ↓
One semantic owner
    ↓
0..N consumers
```

A consumer may:

```text
read
observe
persist
transport
cache
display
execute work involving
```

a concept without becoming its semantic owner.

---

# 11. Ownership Does Not Follow Storage

Example:

```text
Preferences
    owns Preference semantics

Storage
    persists Preferences
```

Storage does not become the Preference owner.

---

# 12. Ownership Does Not Follow Execution

Example:

```text
Translation
    owns TranslationArtifact

Runtime
    executes Translation WorkItems
```

Runtime does not become semantic owner of Translation output.

---

# 13. Ownership Does Not Follow UI Projection

Example:

```text
Reading Session
    owns ReadingContext

UI Adapter
    projects ReadingContext
```

UI Adapter does not become ReadingContext owner.

---

# 14. MODULE_DEPENDENCY.md

## Question

```text
Which architecture boundaries may depend on which others?
```

`MODULE_DEPENDENCY.md` defines:

```text
allowed dependency directions
public-contract dependency rules
Artifact dependencies
Query dependencies
Runtime/module dependency
Infrastructure/Platform boundaries
dependency injection
deep-import prohibition
cycle prevention
forbidden orchestration dependencies
```

---

# 15. Central Dependency Rule

```text
Depend on stable contracts,
not another owner's internals.
```

And:

```text
Dependency
does not transfer ownership.
```

---

# 16. Semantic Processing Dependencies

Canonical semantic data flow:

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
```

These are data compatibility relationships.

They do not mean:

```text
Capture directly calls Recognition
Recognition directly calls Text Processing
Text Processing directly calls Translation
Translation directly calls Presentation
```

---

# 17. Runtime Execution Flow

Execution is controlled separately:

```text
Business Execution Planning
    ↓
RuntimeRevision
    ↓
WorkItems
    ↓
Attempts
```

Runtime determines executable dependency readiness.

---

# 18. Semantic Dependency vs Execution Dependency

Example:

```text
Translation
    consumes SourceDocumentArtifact
```

means Translation depends on the Text Processing public Artifact contract.

It does not mean:

```text
Text Processing
    calls Translation
```

---

# 19. Application Role

Application coordinates cross-module use cases such as:

```text
StartReading
StopReading
ChangeReadingSource
RetryCurrentOperation
SavePreference
```

Application does not replace module ownership.

---

# 20. Business Pipeline Orchestration

Business Pipeline Orchestration determines:

```text
what logical work is required
what dependencies exist
what conditions apply
```

Runtime determines:

```text
when work runs
which Attempt runs
retry
cancellation
queueing
resource admission
```

---

# 21. Event Bus Relationship

Event Bus distributes committed facts.

It does not perform dependency orchestration.

Forbidden current architecture:

```text
RecognitionCompleted
    ↓
TranslationRequested
```

Preferred:

```text
Published Artifact
    ↓
Runtime dependency condition satisfied
    ↓
next WorkItem READY
```

---

# 22. Command Relationship

Commands use explicit owner/Application/Runtime contracts.

Do not use:

```text
*_REQUESTED
```

Event Bus messages as dependency routing.

---

# 23. Module Boundary Documents

Architecture-level module definition is stored here:

```text
01-architecture/modules/
```

Detailed module definition is stored in:

```text
02-modules/<module>/
```

---

# 24. Standard Module Documentation

Current module documentation normally contains:

```text
MODULE.md
CONTRACT.md
STATES.md
EVENTS.md
ERRORS.md
README.md
```

Each file owns a different concern.

---

# 25. MODULE.md

Defines:

```text
responsibility
boundary
owned concepts
non-responsibilities
high-level dependencies
```

---

# 26. CONTRACT.md

Defines:

```text
public operations
queries
commands
public data contracts
Artifact contracts
integration ports
```

---

# 27. STATES.md

Defines module-owned lifecycle/state.

It must not duplicate Runtime execution state.

---

# 28. EVENTS.md

Defines committed module-owned facts.

Events must follow:

```text
core/EVENT_CONVENTION.md
```

---

# 29. ERRORS.md

Defines module-owned errors.

Errors remain with their semantic owner even when observed by:

```text
Application
Runtime
Diagnostics
UI Adapter
```

---

# 30. README.md

Provides module-local entry point and reading order.

---

# 31. Architecture Relationship

Current relationship:

```text
.meta governance
        ↓
Core Architecture
        ↓
Module Topology / Ownership / Dependencies
        ├─────────────┐
        ↓             ↓
Module Design      Runtime Architecture
        ↓             ↓
        └──────┬──────┘
               ↓
       Infrastructure / Platform
               ↓
          Implementation
```

Runtime and Module Design are related authorities.

Neither is simply a subordinate implementation detail of the other.

---

# 32. Core Architecture Relationship

`01-architecture/core/` defines architecture-wide rules such as:

```text
state authority
data flow
Candidate vs Published
event semantics
Event Bus role
capability model
```

Module architecture applies those rules to ownership boundaries.

---

# 33. Runtime Relationship

`01-architecture/runtime/` defines execution authority.

Module architecture defines semantic authority.

Therefore:

```text
Module
    owns meaning

Runtime
    owns execution
```

---

# 34. Infrastructure Relationship

Infrastructure implements mechanisms required by modules and Runtime.

Examples:

```text
Event Bus implementation
Scheduler implementation
Logging transport
Telemetry transport
Resource Manager
Secret Management
Storage
```

Infrastructure does not redefine semantic ownership.

---

# 35. Platform Relationship

Platform adapters isolate:

```text
OS APIs
browser APIs
desktop-native APIs
```

from semantic modules.

Platform code must not perform business orchestration.

---

# 36. Composition Relationship

Composition Root is responsible for:

```text
concrete implementation selection
dependency injection
module wiring
provider wiring
process topology
boot/shutdown composition
```

It does not contain business logic.

---

# 37. Reading Order

Recommended order:

```text
1. README.md

2. MODULE_MAP.md

3. OWNERSHIP_MAP.md

4. MODULE_DEPENDENCY.md
```

---

# 38. Why This Order

First:

```text
MODULE_MAP
    → What modules exist?
```

Then:

```text
OWNERSHIP_MAP
    → Who owns what?
```

Finally:

```text
MODULE_DEPENDENCY
    → Who may depend on whom?
```

Dependency rules cannot be evaluated correctly before ownership is known.

---

# 39. Reading After This Directory

After `01-architecture/modules/`, read:

```text
01-architecture/runtime/
```

when working on execution semantics.

Then read the relevant:

```text
02-modules/<module>/
```

for module-local contracts.

---

# 40. Canonical Current Module Set

```text
02-modules/
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

This set is the current primary module topology.

---

# 41. Legacy Module Names

Older architecture documents may contain names such as:

```text
Source
Observation
Classification
Extraction
Understanding
OCR
Segmentation
Rendering
Reader
Session
Settings
Cache
Provider
```

These are not automatically current top-level modules.

Use `MODULE_MAP.md` to determine current ownership.

---

# 42. Legacy OCR

The old top-level:

```text
OCR
```

is now represented publicly by:

```text
Recognition
```

Detailed OCR processing architecture remains under:

```text
01-architecture/ocr/
```

---

# 43. Legacy Understanding / Segmentation

Text semantic reconstruction is now primarily owned by:

```text
Text Processing
```

TranslationUnit and TranslationBatch belong to:

```text
Translation
```

---

# 44. Legacy Rendering

Semantic presentation is owned by:

```text
Presentation
```

Native frontend adaptation is owned by:

```text
UI Adapter / Platform
```

---

# 45. Legacy Reader

No monolithic Reader module exists.

Responsibilities are split across:

```text
Reading Session
Application
Presentation
UI Adapter
```

---

# 46. Legacy Settings

Persistent settings semantics are represented by:

```text
Preferences
```

Session-only configuration belongs to Reading Session.

UI Settings screens belong to UI Adapter.

---

# 47. Legacy Pipeline Orchestrator

The old stage-driven `Pipeline Orchestrator` is no longer execution authority.

Current architecture uses:

```text
Business Pipeline Orchestration
        ↓
Runtime dependency graph
        ↓
WorkItems / Attempts
```

---

# 48. Retry and Cancellation

Retry and cancellation are not module-to-module dependencies.

They belong to Runtime execution authority.

Modules only expose:

```text
semantic result
error classification
provider cooperation
```

where needed.

---

# 49. Artifact Dependency

Cross-module semantic dependencies should prefer immutable public Artifacts.

Examples:

```text
RecognitionArtifact
SourceDocumentArtifact
TranslationArtifact
PresentationArtifact
```

Do not depend on internal provider/module data.

---

# 50. Query Dependency

A direct read-only Query dependency may be valid.

Example:

```text
Translation
    → GlossaryQuery
```

if Glossary/Knowledge remains a distinct owner.

Query dependency does not transfer ownership.

---

# 51. Explicit Port Dependency

External capabilities are injected through ports.

Example:

```text
Recognition
    → RecognitionProviderPort

Translation
    → TranslationProviderPort

Capture
    → ScreenCapturePort
```

Concrete implementations remain outside semantic modules.

---

# 52. Forbidden Dependencies

Examples:

```text
UI Adapter → SQLite

Translation → OpenAI concrete client

Recognition → Text Processing internal implementation

Text Processing → Translation execution

Translation → Presentation execution

Infrastructure → Application

Platform → Reading Session mutation

Diagnostics → module private state

Business module → Scheduler implementation
```

---

# 53. Public vs Internal

Module consumers depend only on public contracts.

Internal code includes:

```text
provider mapping
parsers
implementation helpers
temporary models
repository implementations
optimization internals
```

These must not be imported from outside the module.

---

# 54. Dependency Injection

Dependencies should be explicit through:

```text
constructor
factory
function parameters
explicit module context
```

Do not hide dependencies in global containers.

---

# 55. Circular Dependencies

Circular dependencies are prohibited.

If two modules appear to require each other:

```text
identify the actual owner
extract a Query
extract a stable Artifact contract
introduce a neutral port only when justified
```

Do not solve cycles with a Service Locator.

---

# 56. Source-Code Structure

This directory does not define final source-code folder structure.

Earlier structures such as:

```text
presentation/
application/
features/
core/
infrastructure/
platform/
shared/
```

remain architecture exploration, not current committed implementation topology.

Final source layout depends on Technology Selection.

---

# 57. Technology Neutrality

Dependency rules must remain applicable whether CRAI is implemented using:

```text
TypeScript
Rust
C#
Kotlin
Go
or another stack
```

Tool-specific enforcement is selected later.

---

# 58. Multi-Process Readiness

Public contracts should remain serializable where practical.

Avoid crossing boundaries with:

```text
native pointers
framework controls
SDK objects
database connections
threads
mutexes
closures
```

---

# 59. Typed Identity

Use current architecture identities such as:

```text
SessionId
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
```

Avoid legacy generic:

```text
PipelineId
TaskId
contentRevision
```

as universal dependencies.

---

# 60. Module Architecture Invariants

1. `MODULE_MAP.md` owns current topology.

2. `OWNERSHIP_MAP.md` owns semantic ownership mapping.

3. `MODULE_DEPENDENCY.md` owns dependency rules.

4. Capability does not automatically become Module.

5. Every concept has one semantic owner.

6. Dependency does not transfer ownership.

7. Runtime is separate from semantic modules.

8. Infrastructure is separate from business ownership.

9. Modules depend on public contracts only.

10. Deep imports are forbidden.

11. Semantic Artifacts are valid cross-module dependencies.

12. Module internals are not valid cross-module dependencies.

13. Runtime owns WorkItem/Attempt execution.

14. Runtime owns retry/cancellation mechanics.

15. Event Bus does not orchestrate processing.

16. `_REQUESTED` Event Bus commands are not dependency routing.

17. Composition Root owns concrete wiring.

18. Service Locator is forbidden in business code.

19. Provider DTOs remain inside adapters.

20. UI Adapter does not directly depend on Infrastructure implementations.

21. Platform adapters contain no business orchestration.

22. Diagnostics observes but does not take ownership.

23. Source-code topology remains technology-neutral until Technology Selection.

---

# 61. Architecture Change Rule

A change to module topology or ownership may affect:

```text
MODULE_MAP.md
OWNERSHIP_MAP.md
MODULE_DEPENDENCY.md
02-modules/*/
runtime architecture
core architecture
PROJECT_STATUS.md
```

Such changes should be synchronized explicitly.

---

# 62. New Module Checklist

Before adding a top-level module, ask:

```text
Does it own unique semantic state?

Does it own a distinct lifecycle?

Does it expose a stable contract?

Is that responsibility already owned?

Would a capability/submodule be sufficient?

Would the new module reduce coupling?
```

Do not add modules merely because:

```text
a provider exists
a database table exists
a screen exists
a helper folder exists
```

---

# 63. New Dependency Checklist

Before adding a dependency, ask:

```text
Who owns the target concept?

Am I using a public contract?

Is this dependency read-only or mutating?

Is this hidden orchestration?

Could an Artifact/Query/Port be enough?

Does this create a cycle?

Does it leak implementation?

Does it move semantic ownership accidentally?
```

---

# 64. Completion Criteria

The `modules/` architecture set is synchronized when:

* `MODULE_MAP.md` reflects the current module set;
* `OWNERSHIP_MAP.md` is project-wide and Runtime v2 aligned;
* `MODULE_DEPENDENCY.md` uses current modules and Runtime boundaries;
* legacy Feature/Pipeline Orchestrator dependency model is removed;
* ownership is reviewed before dependency;
* Runtime is not treated as ordinary business module;
* semantic Artifact dependencies are explicit;
* Event Bus command routing is removed;
* Composition/Infrastructure/Platform boundaries remain clear;
* source-code structure is not prematurely frozen.

---

# 65. Summary

This directory answers:

```text
MODULE_MAP
    What modules exist?

OWNERSHIP_MAP
    Who owns what?

MODULE_DEPENDENCY
    Who may depend on whom?
```

The current architecture model is:

```text
Core Architecture
        ↓
Module Topology
        ↓
Semantic Ownership
        ↓
Dependency Rules
        ↓
Module Contracts
        ↕
Runtime Execution
        ↓
Infrastructure / Platform
```

The central principle is:

```text
First identify the owner.

Then define the dependency.

Never let dependency
silently redefine ownership.
```
