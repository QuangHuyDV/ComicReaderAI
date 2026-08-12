# CRAI Module Map

> **Project:** CRAI
> **Path:** `doc/01-architecture/modules/MODULE_MAP.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the current logical module topology of CRAI.

It answers:

```text
Which architecture modules currently exist?

What semantic responsibility does each module own?

Which areas are modules,
which are Runtime architecture,
and which are Infrastructure?

How do product capabilities map onto stable ownership boundaries?
```

This document focuses on logical architecture boundaries.

It does not define:

```text
source-code package structure
deployment topology
process boundaries
provider implementations
database schemas
framework selection
```

---

# 2. Governing Documents

All module definitions must remain consistent with:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES_RULE.md

01-architecture/core/CAPABILITY_MAP.md
01-architecture/core/STATE_MACHINE.md
01-architecture/core/DATA_FLOW.md
01-architecture/core/EVENT_CONVENTION.md
01-architecture/core/EVENT_BUS.md

01-architecture/modules/OWNERSHIP_MAP.md
01-architecture/modules/MODULE_DEPENDENCY.md

01-architecture/runtime/
```

---

# 3. Current Architecture Status

CRAI is no longer in the original module-discovery stage.

The architecture now has established boundaries for the main processing modules.

Current status:

```text
Core architecture
    → Runtime v2 synchronized

Primary modules
    → ownership boundaries established

Runtime
    → execution authority established

Infrastructure
    → separated from business module semantics
```

Module boundaries may still evolve, but changes now require explicit ownership justification.

---

# 4. Module Design Principles

A module should:

* own one coherent semantic responsibility;
* expose an explicit public contract;
* hide implementation/provider/platform details;
* avoid circular dependencies;
* avoid duplicating another owner's authority;
* keep state/errors/events within its semantic boundary;
* remain testable independently;
* remain replaceable where implementation variability is expected.

---

# 5. Capability Is Not Module

A capability describes:

```text
what CRAI must be able to do
```

A module describes:

```text
who owns semantic responsibility
```

Therefore:

```text
capability
    ≠
module
```

Example:

```text
Capability:
Provider fallback
```

may involve:

```text
Translation / Recognition
    → semantic suitability/error classification

Provider Management
    → provider capability/availability

Runtime
    → new Attempt
```

It does not imply one `ProviderFallbackModule`.

---

# 6. Architecture Layers

CRAI currently separates four logical architecture areas:

```text
Application / Domain Modules

Cross-Cutting Support Modules

Runtime Architecture

Infrastructure
```

These areas have different authority.

---

# 7. Application / Domain Modules

Current primary modules:

```text
Capture

Recognition

Text Processing

Translation

Presentation

Reading Session

Preferences
```

These modules own CRAI business/application semantics.

---

# 8. Cross-Cutting Support Modules

Current cross-cutting modules:

```text
Diagnostics

UI Adapter
```

These modules support multiple domains but do not own core processing semantics.

---

# 9. Runtime Architecture

Runtime is not treated as an ordinary business module.

Runtime owns execution authority:

```text
RuntimeRevision
WorkItem
Attempt
scheduling
queueing
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

# 10. Infrastructure

Infrastructure provides technical mechanisms.

Current areas include:

```text
configuration
event-bus
logging
resource-manager
scheduler
secret-management
telemetry
storage/cache mechanisms
```

Infrastructure does not become semantic owner of business data merely because it persists/transports/executes it.

---

# 11. Current Module Topology

```text
                    Application / Reading Use Cases
                               │
                               ▼
                       Reading Session
                               │
                               ▼
                    Business Execution Planning
                               │
                               ▼
                           Runtime
                               │
             ┌─────────────────┼─────────────────┐
             │                 │                 │
             ▼                 ▼                 ▼
          Capture         Recognition      Text Processing
             │                 │                 │
             └─────────────────┴─────────┬───────┘
                                         ▼
                                    Translation
                                         │
                                         ▼
                                    Presentation
                                         │
                                         ▼
                                     UI Adapter
```

Cross-cutting:

```text
Preferences
Diagnostics
```

support the appropriate owners without becoming execution authority.

---

# 12. Semantic Processing Chain

The main semantic data path is:

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

This is a semantic dependency chain.

It is not a mandatory serialized execution-state machine.

---

# 13. Structured Text Path

For reliable structured text:

```text
Structured Source
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
Presentation
```

Capture/Recognition may be skipped.

---

# 14. Image Reading Path

For image/screen sources:

```text
Source
    ↓
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

Runtime controls actual executable WorkItems/Attempts.

---

# 15. Capture Module

## Primary Responsibility

Acquire and normalize visual/source content into Capture-owned semantics.

Owns:

```text
CaptureSource semantics
capture candidates
accepted Capture Artifacts
source geometry
capture capability state
source-observation integration
```

Does not own:

```text
Recognition
Translation
Runtime retry
Scheduler
Reading Session lifecycle
```

---

# 16. Recognition Module

## Primary Responsibility

Convert accepted visual content into recognized text and geometry.

Owns:

```text
recognized regions
recognized text
confidence
geometry
text direction
reading hints
RecognitionArtifact
Recognition provider normalization
```

Does not own:

```text
SourceDocument semantic reconstruction
TranslationUnit
Runtime retry
downstream orchestration
```

---

# 17. Text Processing Module

## Primary Responsibility

Transform Recognition/structured-source data into normalized semantic source-document structure.

Owns:

```text
normalization
line reconstruction
paragraph reconstruction
reading-order normalization
grouping
semantic segmentation
SourceDocument
SourceDocumentArtifact
```

Does not own:

```text
TranslationUnit
TranslationBatch
Translation context assembly
Runtime execution
```

---

# 18. Translation Module

## Primary Responsibility

Translate SourceDocument content while preserving alignment, context and terminology.

Owns:

```text
TranslationUnit
TranslationBatch
context assembly
glossary application
translation alignment
TranslationArtifact
provider-response normalization
```

Does not own:

```text
Runtime retry
Runtime cancellation
Scheduler admission
Presentation layout
```

---

# 19. Presentation Module

## Primary Responsibility

Transform accepted semantic Translation output into platform-neutral presentation semantics.

Owns:

```text
PresentationArtifact
semantic layout
geometry mapping
text fitting
presentation revision
presentation warnings
```

Does not own:

```text
native UI controls
native rendering framework
Runtime execution
Translation semantics
```

---

# 20. Reading Session Module

## Primary Responsibility

Own one active reading experience and its authoritative reading context.

Owns:

```text
SessionId
session lifecycle
SessionConfiguration
ReadingContext
ReadingContextRevision
session-local configuration
```

Does not own:

```text
processing-stage lifecycle
WorkItem
Attempt
retry
cancellation execution
Artifact publication
```

---

# 21. Preferences Module

## Primary Responsibility

Own persistent user preference semantics.

Owns:

```text
PreferenceDefinition
Global preferences
Source-scoped preferences
PreferenceRevision
validation
resolution inputs
persistent preference state
```

Does not own:

```text
session-only overrides
ReadingContext
UI rendering
Runtime execution
```

Session-specific configuration belongs to Reading Session.

---

# 22. Diagnostics Module

## Primary Responsibility

Provide cross-cutting diagnostic semantics and operational visibility.

Owns:

```text
DiagnosticObservation semantics
diagnostic correlation
health aggregation
diagnostic capabilities
support-bundle semantics
privacy-safe diagnostic projection
```

Does not own:

```text
business errors from other modules
business health authority
Logging transport
Telemetry transport
Runtime execution
```

---

# 23. UI Adapter Module

## Primary Responsibility

Adapt between native/platform UI and CRAI application contracts.

Inbound:

```text
Native UI Event
    ↓
UiIntent
    ↓
Application / Module Command
```

Outbound:

```text
Application / Module State
    ↓
Immutable ViewModel
    ↓
Native UI
```

Does not own:

```text
business logic
pipeline orchestration
Runtime retry
Presentation semantics
persistent preference authority
```

---

# 24. Runtime

## Primary Responsibility

Execute planned work safely.

Owns:

```text
RuntimeRevision
WorkItem
Attempt
dependency readiness
queueing
scheduling
deadline
timeout
retry
cancellation
supersession
backpressure
resource admission
```

Runtime does not own:

```text
recognized text semantics
TranslationUnit semantics
Presentation layout
Reading Session domain meaning
```

---

# 25. Application Layer

Cross-module use cases may be coordinated by an Application layer.

Examples:

```text
StartReading
StopReading
ChangeReadingSource
RetryCurrentOperation
ExportSupportBundle
```

Application coordinates owners.

It does not replace them.

---

# 26. Business Pipeline Orchestration

Business Pipeline Orchestration determines:

```text
which logical work is required
which inputs/Artifacts are required
dependency relationships
conditional processing path
```

It does not own:

```text
queue
thread
Attempt lifecycle
retry timing
provider request execution
```

Those belong to Runtime.

---

# 27. Provider Management

Provider Management is an architecture/infrastructure capability rather than a primary business module in the current module set.

It may own:

```text
provider registration
provider capability description
provider availability
provider configuration
provider adapter discovery
```

Semantic modules still own provider suitability and normalized results.

Runtime owns Attempt execution.

---

# 28. Storage

Storage is infrastructure/application support.

It owns physical persistence mechanics and storage contracts.

It does not become semantic owner of:

```text
Preference
ReadingContext
TranslationArtifact
Glossary
```

merely because those objects are persisted.

---

# 29. Cache

Cache is an optimization mechanism.

It does not define current authority.

A cache hit still requires:

```text
compatibility validation
scope validation
current authority validation
```

---

# 30. Scheduler

Scheduler belongs to Runtime/Infrastructure.

It does not become a business module.

It handles:

```text
admission
priority
queue selection
resource-aware scheduling
```

under Runtime authority.

---

# 31. Event Bus

Event Bus belongs to Infrastructure.

It distributes committed facts.

It does not:

```text
start stages
perform retry
perform cancellation
create WorkItems
orchestrate pipeline
```

---

# 32. Logging and Telemetry

Logging/Telemetry infrastructure owns:

```text
transport
buffering
sampling
export
storage/backend integration
```

Diagnostics owns semantic diagnostic representation.

---

# 33. Current Documentation Mapping

Current logical module documentation:

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

Each module currently follows the standard six-file set:

```text
MODULE.md
CONTRACT.md
STATES.md
EVENTS.md
ERRORS.md
README.md
```

where applicable.

---

# 34. Runtime Documentation Mapping

Runtime architecture:

```text
01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── RUNTIME_COMPONENTS.md
├── RUNTIME_CONFIG.md
├── WORK_QUEUE.md
├── SCHEDULER.md
├── RETRY_POLICY.md
├── CANCELLATION.md
├── RESOURCE_LIFECYCLE.md
├── THREADING_MODEL.md
├── MEMORY_MODEL.md
├── CACHE_POLICY.md
├── PERFORMANCE_MODEL.md
├── RUNTIME_OBSERVABILITY.md
├── BOOT_SEQUENCE.md
├── ERROR_MODEL.md
└── README.md
```

---

# 35. Infrastructure Documentation Mapping

Current infrastructure areas:

```text
03-infrastructure/
├── configuration/
├── event-bus/
├── logging/
├── resource-manager/
├── scheduler/
├── secret-management/
└── telemetry/
```

Storage/cache-related infrastructure may be documented separately according to current project structure.

---

# 36. Removed Legacy Module Names

The following v1 module names are no longer the current primary module map:

```text
Source
Observation
Image Processing
OCR
Layout
Reader
Session
Settings
Provider
Cache
```

Their responsibilities were either:

```text
merged
renamed
moved into Runtime
moved into Infrastructure
or retained as capabilities/subdomains
```

---

# 37. Legacy `Source`

The old `Source` module responsibility is now distributed across:

```text
Reading Session
    → authoritative source context

Capture
    → acquisition/capture semantics

platform/browser adapters
    → source integration
```

A single global `Source` module is not currently required.

---

# 38. Legacy `Observation`

Observation capabilities remain important.

They currently belong mainly to:

```text
Capture/source-observation logic
```

plus Runtime/Application consequences.

No standalone Observation module is currently required.

---

# 39. Legacy `Image Processing`

Image preprocessing belongs primarily inside:

```text
Recognition/Capture processing boundary
```

according to exact OCR architecture.

It is not currently a top-level primary module.

---

# 40. Legacy `OCR`

The current module is:

```text
Recognition
```

because its semantic output includes more than raw OCR string extraction:

```text
text
geometry
confidence
direction
reading hints
normalized provider output
```

Detailed OCR architecture remains under:

```text
01-architecture/ocr/
```

---

# 41. Legacy `Layout`

Layout/reading-order capabilities are distributed primarily across:

```text
Recognition
Text Processing
Presentation
```

depending on semantic stage.

There is no standalone top-level Layout module in the current module set.

---

# 42. Legacy `Reader`

The old Reader responsibility has been separated into:

```text
Reading Session
    → reading context/lifecycle

Application
    → use-case coordination

Presentation
    → semantic presentation

UI Adapter
    → native/frontend adaptation
```

No monolithic Reader module is currently required.

---

# 43. Legacy `Session`

Renamed/current module:

```text
reading-session
```

with narrower explicit ownership.

---

# 44. Legacy `Settings`

Current module:

```text
preferences
```

Persistent preference semantics live there.

UI Settings screens belong to UI Adapter.

Session-only configuration belongs to Reading Session.

---

# 45. Legacy `Provider`

Provider functionality remains an architecture/support concern.

Provider adapters are owned behind semantic modules/provider-management boundaries.

Provider is not currently one of the primary `02-modules/` business modules.

---

# 46. Legacy `Cache`

Cache remains Infrastructure/Runtime policy.

It does not appear as a semantic `02-modules/` business module.

---

# 47. Module Groups

For conceptual navigation, current modules may be grouped as:

## 47.1 Source and Understanding

```text
Capture
Recognition
Text Processing
```

## 47.2 Language

```text
Translation
```

## 47.3 Presentation and Interaction

```text
Presentation
Reading Session
UI Adapter
```

## 47.4 Configuration

```text
Preferences
```

## 47.5 Cross-Cutting

```text
Diagnostics
```

Runtime and Infrastructure remain separate architecture areas rather than module groups.

---

# 48. Grouping Does Not Create Authority

Module groups are documentation/navigation aids.

They do not create:

```text
super-module state
shared group lifecycle
group-level error ownership
group-level event ownership
```

---

# 49. Shared Module Rules

Shared/cross-cutting components must not import product-specific authority.

Examples:

```text
Diagnostics
    must not own Translation failure

UI Adapter
    must not own Runtime retry

Preferences
    must not own ReadingContext

Runtime
    must not own Translation semantics

Storage
    must not own persisted domain meaning
```

---

# 50. Allowed Communication Style

Modules communicate through:

```text
explicit contracts
immutable Artifacts
queries
commands
committed facts/events
```

Event Bus is not required for every interaction.

---

# 51. Direct Contract vs Event

Use direct contract when:

```text
one owner is being asked to do something
or
current state/result is required
```

Use Event when:

```text
a committed fact has asynchronous consumers
```

---

# 52. No Direct Stage Chaining by Events

Do not define module dependency as:

```text
RecognitionCompleted
    ↓
Translation starts
```

Runtime/business orchestration owns executable dependencies.

---

# 53. Module Dependency Principle

Business dependency may exist:

```text
Translation
    consumes SourceDocumentArtifact
```

without requiring:

```text
Translation source code
    imports Text Processing internals
```

Stable contracts/Artifact schemas define the boundary.

---

# 54. Module State Principle

Each module owns its own lifecycle.

Typical form:

```text
UNINITIALIZED
INITIALIZING
READY
DEGRADED
STOPPING
STOPPED
```

Scoped operations should not expand the global module lifecycle unnecessarily.

---

# 55. Module Error Principle

Each module owns only errors within its semantic boundary.

Examples:

```text
REC-*
TXT-*
TRN-*
PRES-*
SES-*
PREF-*
DIAG-*
UIA-*
```

External error identity must remain intact when observed/projected elsewhere.

---

# 56. Module Event Principle

Each module `EVENTS.md` defines only committed module-owned facts that genuinely need asynchronous consumers.

Do not create events for every:

```text
function call
stage start
stage completion
retry request
UI interaction
metric
trace
```

---

# 57. Current Module Status

The primary module boundaries currently considered established are:

| Module          | Current architecture status |
| --------------- | --------------------------- |
| Capture         | Runtime v2 synchronized     |
| Recognition     | Runtime v2 synchronized     |
| Text Processing | Runtime v2 synchronized     |
| Translation     | Runtime v2 synchronized     |
| Presentation    | Runtime v2 synchronized     |
| Reading Session | Runtime v2 synchronized     |
| Preferences     | Runtime v2 synchronized     |
| Diagnostics     | Runtime v2 synchronized     |
| UI Adapter      | Runtime v2 synchronized     |

This status refers to architecture synchronization, not implementation completeness.

---

# 58. Module Status Does Not Mean Implemented

`Runtime v2 synchronized` means:

```text
ownership
contracts
states
events
errors
README
```

are aligned with current architecture.

It does not mean:

```text
production code complete
prototype validated
provider selected
performance proven
```

---

# 59. Areas Not Currently Promoted to Top-Level Modules

The following remain capabilities/features/subsystems unless later ownership analysis justifies promotion:

```text
Reading History
Download Manager
Browser Extension
Local Library
AI Assistant
OCR Correction
Translation Memory
Glossary
Offline Model Package
Cross-device Synchronization
```

---

# 60. Translation Memory

Translation Memory may ultimately involve:

```text
Translation semantics
Knowledge domain
Storage
Cache
```

A standalone top-level module should not be created until ownership and lifecycle require it.

---

# 61. Glossary

Glossary is currently treated as Translation/Knowledge-related capability with persistent support.

Do not create a module solely because it needs a table/file/API.

---

# 62. Browser Extension

Browser Extension is a frontend/integration profile.

It does not automatically become a business-domain module.

It may use:

```text
UI Adapter
source/browser adapter
Application contracts
```

---

# 63. Local Library

A local content library may later justify a domain module if it develops independent ownership such as:

```text
content identity
library lifecycle
catalog
metadata
user-owned collections
```

It is currently deferred.

---

# 64. AI Assistant

AI Assistant remains a potential product feature.

It must not become the owner of core Translation/Recognition/Reading Session state merely because it uses AI providers.

---

# 65. Resolved Questions — Reader

Previously open:

```text
Is Reader a coordinator or full domain module?
```

Current answer:

```text
No monolithic Reader module.

Reading Session
    owns reading lifecycle/context.

Application
    coordinates use cases.

Presentation
    owns semantic presentation.

UI Adapter
    owns frontend adaptation.
```

---

# 66. Resolved Questions — OCR and Layout

Current ownership:

```text
Recognition
    → visual text recognition
    → geometry/confidence/direction

Text Processing
    → semantic reconstruction/order normalization

Presentation
    → display layout/geometry/fitting
```

User-edited OCR/source corrections must retain source/provenance and be routed through the relevant owner contract.

---

# 67. Resolved Questions — Translation and Knowledge

Current:

```text
Translation
    owns TranslationUnit
    owns TranslationBatch
    owns context assembly
    consumes glossary/knowledge snapshots
```

Persistent glossary/translation-memory storage remains a separate ownership concern from Translation execution.

Further Knowledge-module promotion remains open.

---

# 68. Resolved Questions — Runtime

Previously open:

```text
Which module creates processing revisions?
Which module owns cancellation?
```

Current answer:

```text
Runtime owns RuntimeRevision.
Runtime owns WorkItem/Attempt execution.
Runtime owns cancellation mechanics.
Runtime owns retry mechanics.
```

Reading Session owns:

```text
ReadingContextRevision
```

These revisions are distinct.

---

# 69. Remaining Open Questions

Meaningful unresolved module-boundary questions include:

```text
Should Knowledge become a dedicated top-level module?

Should persistent Reading History become a domain module?

Should browser integration remain adapter-only
or require a Source Integration module?

Should imported-document/library management
become a domain module after MVP?

What permanent correction/annotation model
should exist across Recognition/Translation?
```

These should not block current MVP architecture unless required.

---

# 70. Module Creation Checklist

Before introducing a new top-level module, verify:

* Does it own unique semantic state?
* Does it own a distinct lifecycle?
* Does it expose a stable public contract?
* Is its responsibility not already owned?
* Can dependencies remain directional?
* Can it be tested independently?
* Is it more than provider/platform/infrastructure detail?
* Does it reduce coupling rather than create another orchestration layer?
* Would treating it as a capability/submodule be insufficient?

If most answers are no, do not create the module.

---

# 71. Module Removal / Merge Checklist

A module candidate should be merged or demoted when:

```text
it owns no unique state
it merely wraps another owner
it duplicates Runtime execution
it duplicates Infrastructure
it exists only because a folder/API/provider exists
```

---

# 72. Current Directory Mapping

Canonical current mapping:

```text
01-architecture/modules/
    → architecture-level module topology/ownership/dependency

01-architecture/runtime/
    → Runtime execution architecture

02-modules/<module>/
    → module-local contracts/state/events/errors

03-infrastructure/
    → technical mechanisms
```

Legacy numbered documentation-directory mappings are deprecated.

---

# 73. Architecture Invariants

1. Capability does not automatically become Module.

2. Every module owns one coherent semantic boundary.

3. Runtime is not an ordinary processing-domain module.

4. Infrastructure is not a business module.

5. Capture owns acquisition semantics.

6. Recognition owns recognized text/geometry semantics.

7. Text Processing owns SourceDocument semantics.

8. Translation owns TranslationUnit/Batch and TranslationArtifact semantics.

9. Presentation owns semantic presentation.

10. Reading Session owns ReadingContext and session lifecycle.

11. Preferences owns persistent preferences.

12. Diagnostics owns diagnostic semantics only.

13. UI Adapter owns frontend adaptation only.

14. Runtime owns WorkItem/Attempt execution.

15. Runtime owns retry/cancellation mechanics.

16. Scheduler does not own business semantics.

17. Storage does not become owner of persisted domain meaning.

18. Cache does not become current authority.

19. Provider adapters do not become business state owners.

20. Event Bus does not orchestrate module execution.

21. Module completion events do not directly start downstream stages.

22. Application may coordinate cross-module use cases without replacing ownership.

23. Shared Artifacts/contracts define semantic module boundaries.

24. Module groups do not create super-module authority.

25. Current module directories under `02-modules/` are the canonical primary module set.

---

# 74. Related Documents

```text
doc/01-architecture/core/
├── CAPABILITY_MAP.md
├── DATA_FLOW.md
├── STATE_MACHINE.md
├── EVENT_CONVENTION.md
└── EVENT_BUS.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
├── OWNERSHIP_MAP.md
└── README.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── RUNTIME_COMPONENTS.md
├── RETRY_POLICY.md
├── CANCELLATION.md
├── SCHEDULER.md
└── WORK_QUEUE.md

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

# 75. Completion Criteria

This Module Map is synchronized when:

* legacy module list is removed;
* current `02-modules/` set is authoritative;
* OCR is represented by Recognition module;
* Reader is decomposed into Reading Session/Application/Presentation/UI Adapter;
* Settings is represented by Preferences;
* Runtime is separated from business modules;
* Scheduler/Cache/Storage/Event Bus are separated from business modules;
* Text Processing does not own TranslationUnit;
* Translation does not own Runtime retry/cancellation;
* Reading Session does not own processing-stage state;
* Presentation does not own native UI rendering;
* current Runtime v2 synchronized module status is explicit;
* old numbered documentation-directory mapping is removed;
* resolved questions are no longer listed as open.

---

# 76. Summary

The v1 topology broadly looked like:

```text
Source
Observation
Image Processing
OCR
Layout
Translation
Reader
Session
Settings
Runtime
Cache
Storage
Provider
Diagnostics
```

The current topology is:

```text
Business / Application Modules
├── Capture
├── Recognition
├── Text Processing
├── Translation
├── Presentation
├── Reading Session
└── Preferences

Cross-Cutting Modules
├── Diagnostics
└── UI Adapter

Runtime
└── RuntimeRevision / WorkItem / Attempt execution

Infrastructure
├── Event Bus
├── Scheduler
├── Logging
├── Telemetry
├── Resource Management
├── Secrets
├── Storage/Cache mechanisms
└── Configuration
```

The central rule is:

```text
Modules own semantics.

Runtime owns execution.

Infrastructure provides mechanisms.

Capabilities describe product needs.

These concepts must not be merged.
```
